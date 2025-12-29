using Nightmare.Parser;
using Nightmare.Parser.TemplateExpressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.IO;
using System.Net;
using Terminal.Gui.App;

namespace Nightmare.Config;

public class ConfigProcessor(IApplication application)
{
    private readonly EvaluationContext _context = new(application);

    public string ProcessName(JsonObject ast)
    {
        return !ast.TryGetProperty<JsonString>("name", out var name)
            ? "Nightmare Requests Collection"
            : TemplateStringEvaluator.Evaluate(name.Template, _context);
    }


    public void ProcessProfile(
        JsonObject ast,
        string selectedProfileName
    )
    {
        var profile = ast
            .GetProperty<JsonObject>("profiles")!
            .GetProperty<JsonObject>(selectedProfileName);

        ProcessVariables(profile);
    }

    public (string, string[]) ProcessProfiles(
        JsonObject ast,
        string selectedProfileName
    )
    {
        if (!ast.TryGetProperty<JsonObject>("profiles", out var profiles))
            throw new ConfigProcessingException(
                "The property `profiles` is required",
                ast.Span
            );

        var profilePair =
            selectedProfileName is not null
            && profiles.TryGetProperty<JsonObject>(selectedProfileName, out var profile)
                ? (Key: selectedProfileName, Value: profile)
                : GetDefaultProfile();

        ProcessVariables(profilePair.Value);

        return (
            profilePair.Key,
            profiles.Properties.Select(p => p.Key).ToArray()
        );

        (string Key, JsonObject Value) GetDefaultProfile()
        {
            try
            {
                var profilePair = profiles
                    .Properties
                    .First(p => p.Value is JsonObject pValue &&
                                pValue.TryGetProperty<JsonBoolean>("default", out var defaultProp)
                                && defaultProp is { Value: true });

                return (
                    profilePair.Key,
                    (JsonObject)profilePair.Value
                );
            }
            catch (InvalidOperationException)
            {
                throw new ConfigProcessingException(
                    "The property `profiles` must be an object with at least one default profile",
                    profiles!.Span
                );
            }
        }
    }

    private void ProcessVariables(JsonObject profile)
    {
        _context.ClearVariables();

        if (!profile.TryGetProperty<JsonObject>("data", out var variables)) return;

        foreach (var (key, value) in variables.Properties)
            _context.SetVariable(key, JsonValueExtensions.Convert(value, _context));
    }

    public List<JsonProperty> ProcessRequests(JsonObject ast)
    {
        if (ast.TryGetProperty<JsonObject>("requests", out var requests))
            return requests
                .Properties
                .Select(p => new JsonProperty(p.Key, p.Value, p.Value.Span))
                .ToList();

        return [];
    }

    private static async Task<(HttpRequestMessage Request, HttpResponseMessage Response)> ExecuteRequest(
        JsonProperty requestProp, EvaluationContext context)
    {
        var request = requestProp.Value;

        if (request is not JsonObject requestObject)
            throw new ConfigProcessingException(
                "The value of a request must be an object",
                requestProp.Span
            );

        if (!requestObject.TryGetProperty<JsonString>("url", out var url))
            throw new ConfigProcessingException(
                "The request object must contain a `url` property",
                requestProp.Span
            );

        if (!requestObject.TryGetProperty<JsonString>("method", out var method)) method = null;

        var urlBuilder = new UriBuilder(
            url.Template.HasExpressions ? TemplateStringEvaluator.Evaluate(url.Template, context) : url.Text
        );

        if (requestObject.TryGetProperty<JsonObject>("query", out var queryParams))
        {
            var existingQuery = urlBuilder.Query.TrimStart('?');
            var newQuery = string.Join(
                "&",
                queryParams
                    .Properties
                    .Where(p => p.Value is not JsonNull)
                    .Select(p => $"{p.Key}={WebUtility.UrlEncode(JsonValueExtensions.Serialize(p.Value, context))}")
            );

            urlBuilder.Query = string.IsNullOrEmpty(existingQuery)
                ? newQuery
                : $"{existingQuery}&{newQuery}";
        }

        var httpRequest = new HttpRequestMessage(
            new HttpMethod(
                method is not null
                    ? method.Template.HasExpressions
                        ? TemplateStringEvaluator.Evaluate(method.Template, context)
                        : method.Text
                    : "GET"
            ),
            urlBuilder.ToString()
        );

        httpRequest.Content = BuildBody(requestObject);

        if (requestObject.TryGetProperty<JsonObject>("headers", out var headers))
            foreach (var (hName, hVal) in headers.Properties)
            {
                var hStr = hVal is JsonString js
                    ? js.Template.HasExpressions ? TemplateStringEvaluator.Evaluate(js.Template, context) : js.Text
                    : hVal switch
                    {
                        JsonNumber n => n.Value.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        JsonBoolean b => b.Value ? "true" : "false",
                        _ => throw new ConfigProcessingException(
                            "Header values must be strings, numbers or booleans",
                            hVal.Span
                        )
                    };

                if (httpRequest.Headers.TryAddWithoutValidation(hName, hStr))
                    continue;

                httpRequest.Content?.Headers.TryAddWithoutValidation(hName, hStr);
            }

        if (requestObject.TryGetProperty<JsonObject>("cookies", out var cookies))
            httpRequest.Headers
                .TryAddWithoutValidation(
                    "Cookie",
                    string.Join(
                        "; ",
                        cookies
                            .Properties
                            .Where(p => p.Value is not JsonNull)
                            .Select(p =>
                                $"{p.Key}={WebUtility.UrlEncode(JsonValueExtensions.Serialize(p.Value, context))}"
                            )
                    )
                );

        using var client = new HttpClient();

        if (requestObject.TryGetProperty<JsonNumber>("timeout", out var timeout))
            client.Timeout = TimeSpan.FromMilliseconds(timeout.Value);

        return (httpRequest, await client.SendAsync(httpRequest));

        HttpContent? BuildBody(JsonObject req)
        {
            if (!req.TryGetProperty("body", out var bodyVal)) return null;

            switch (bodyVal)
            {
                case JsonString bodyJs:
                {
                    return GetRawBody(bodyJs);
                }
                case JsonObject bodyObj:
                {
                    if (!bodyObj.TryGetProperty<JsonString>("type", out var typeJs))
                        typeJs = null;

                    var typeValue = typeJs is not null
                        ? typeJs.Template.HasExpressions
                            ? TemplateStringEvaluator.Evaluate(typeJs.Template, context)
                            : typeJs.Text
                        : null;

                    switch (typeValue)
                    {
                        case "raw":
                        {
                            return !bodyObj.TryGetProperty<JsonString>("value", out var rawBody)
                                ? null
                                : GetRawBody(rawBody);
                        }
                        case "text":
                        {
                            if (!bodyObj.TryGetProperty<JsonString>("value", out var valueStr))
                                return new StringContent(string.Empty, Encoding.UTF8, MediaTypeNames.Text.Plain);

                            var value = valueStr.Template.HasExpressions
                                ? TemplateStringEvaluator.Evaluate(valueStr.Template, context)
                                : valueStr.Text;

                            return new StringContent(value, Encoding.UTF8,
                                MediaTypeNames.Text.Plain);
                        }
                        case "json":
                        {
                            if (!bodyObj.TryGetProperty("value", out var json))
                                return new StringContent(string.Empty, Encoding.UTF8, MediaTypeNames.Application.Json);

                            return new StringContent(
                                JsonValueExtensions.Serialize(json, context),
                                Encoding.UTF8, MediaTypeNames.Application.Json
                            );
                        }
                        case "form":
                        {
                            var data = new List<KeyValuePair<string, string>>();

                            if (bodyObj.TryGetProperty("value", out var formData))
                                switch (formData)
                                {
                                    case JsonString formStr:
                                    {
                                        var evaluated = TemplateStringEvaluator
                                            .EvaluateValue(formStr.Template, context);

                                        if (evaluated is not Dictionary<string, object?> dict)
                                            throw new ConfigProcessingException(
                                                "Form data must be an object",
                                                formStr.Span
                                            );

                                        foreach (var (k, v) in dict)
                                            data.Add(new KeyValuePair<string, string>(k,
                                                JsonValueExtensions.Serialize(v)));

                                        break;
                                    }

                                    case JsonObject formObj:
                                    {
                                        foreach (var (k, v) in formObj.Properties)
                                            data.Add(new KeyValuePair<string, string>(k,
                                                JsonValueExtensions.Serialize(v, context)));

                                        break;
                                    }

                                    default:
                                        throw new ConfigProcessingException(
                                            "Form data must be an object or a template string evaluating to an object",
                                            formData!.Span
                                        );
                                }

                            return new FormUrlEncodedContent(data);
                        }
                        case "multipart":
                        {
                            var multipart = new MultipartFormDataContent();

                            if (!bodyObj.TryGetProperty<JsonObject>("value", out var parts)) return multipart;

                            foreach (var (name, partVal) in parts.Properties)
                                switch (partVal)
                                {
                                    case JsonNull:
                                        continue;
                                    case JsonString partStr:
                                    {
                                        var evaluated = partStr.Template.HasExpressions
                                            ? TemplateStringEvaluator.EvaluateValue(partStr.Template, context)
                                            : partStr.Text;

                                        if (evaluated is FileReference fileRef)
                                        {
                                            var stream = File.OpenRead(fileRef.Path);
                                            var streamContent = new StreamContent(stream);
                                            if (!string.IsNullOrEmpty(fileRef.ContentType))
                                                streamContent.Headers.ContentType =
                                                    MediaTypeHeaderValue.Parse(fileRef.ContentType);

                                            var fileName = fileRef.FileName ?? Path.GetFileName(fileRef.Path);
                                            multipart.Add(streamContent, name, fileName);
                                        }
                                        else
                                        {
                                            multipart.Add(
                                                new StringContent(evaluated?.ToString() ?? string.Empty, Encoding.UTF8),
                                                name);
                                        }

                                        break;
                                    }
                                    default:
                                        multipart.Add(
                                            new StringContent(JsonValueExtensions.Serialize(partVal, context),
                                                Encoding.UTF8),
                                            name
                                        );
                                        break;
                                }

                            return multipart;
                        }
                        default:
                        {
                            throw new ConfigProcessingException(
                                "Invalid body type. Must be one of: raw, text, json, multipart",
                                typeJs!.Span
                            );
                        }
                    }
                }
                default:
                    throw new ConfigProcessingException(
                        "Invalid body value. Must be a string or an object with a `type` property and a `value` property.",
                        bodyVal!.Span
                    );
            }

            HttpContent GetRawBody(JsonString body)
            {
                var value = body.Template.HasExpressions
                    ? TemplateStringEvaluator.EvaluateValue(body.Template, context)
                    : body.Text;

                if (value is not FileReference fileRef)
                    return new StringContent(value?.ToString() ?? string.Empty, Encoding.UTF8,
                        MediaTypeNames.Text.Plain);

                var stream = File.OpenRead(fileRef.Path);
                var content = new StreamContent(stream);
                if (!string.IsNullOrEmpty(fileRef.ContentType))
                    content.Headers.ContentType = MediaTypeHeaderValue.Parse(fileRef.ContentType);
                return content;
            }
        }
    }

    public async Task<(HttpRequestMessage Request, HttpResponseMessage Response)> ProcessAndExecuteRequest(
        JsonProperty request)
    {
        return await ExecuteRequest(request, _context);
    }

    public static async Task<(HttpRequestMessage Request, HttpResponseMessage Response)> ProcessAndExecuteRequest(
        JsonProperty request,
        EvaluationContext context
    )
    {
        return await ExecuteRequest(request, context);
    }
}