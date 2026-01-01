using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using Nightmare.Parser;
using Nightmare.Parser.TemplateExpressions;

namespace Nightmare.Config;

public partial class ConfigProcessor
{
    private readonly Stopwatch _stopWatch = new();

    private Dictionary<string, string[]> ProcessHeaders(JsonObject headersJson)
    {
        var h = new Dictionary<string, string[]>();

        foreach (var (hName, hVal) in headersJson.Properties)
            switch (hVal)
            {
                case JsonArray arr:
                {
                    h[hName] = arr.Items
                        .Where(i => i is not JsonNull)
                        .Select(i => Utilities.ToString(i, _context)!)
                        .ToArray();

                    break;
                }

                case JsonString str:
                {
                    var obj = str.ToObject(_context);

                    if (obj is IEnumerable<object?> enumerable)
                    {
                        h[hName] = enumerable
                            .Where(i => i is not null)
                            .Select(i => Utilities.ToString(i, _context)!)
                            .ToArray();
                    }
                    else
                    {
                        var serialized = Utilities.ToString(obj, _context);
                        if (serialized is not null)
                            h[hName] = [serialized];
                    }

                    break;
                }

                case JsonNull: break;

                default:
                {
                    h[hName] = [Utilities.ToString(hVal, _context)!];
                    break;
                }
            }

        return h;
    }

    private (string Url, string FullUrl, Dictionary<string, string>) ProcessUrl(
        JsonString urlJson,
        JsonObject? queryJson
    )
    {
        var uri = new Uri(urlJson.ToString(_context));
        var url = uri.GetLeftPart(UriPartial.Path);
        var query = new Dictionary<string, string>();

        var existingQuery = uri.Query.TrimStart('?');
        if (!string.IsNullOrEmpty(existingQuery))
        {
            var queryParams = existingQuery.Split('&');
            foreach (var param in queryParams)
            {
                var parts = param.Split('=');
                query.Add(parts[0], parts[1]);
            }
        }

        if (queryJson is not null)
            foreach (var (k, v) in queryJson.Properties)
            {
                var serializedValue = Utilities.ToString(v, _context);

                if (serializedValue is not null)
                    query[k] = serializedValue;
            }

        var queryString = string.Join(
            "&", query.Select(p => $"{p.Key}={p.Value}")
        );

        var fullUrl = $"{url}{(string.IsNullOrEmpty(queryString) ? "" : $"?{WebUtility.UrlEncode(queryString)}")}";

        return (
            url,
            fullUrl,
            query
        );
    }

    private (string Type, HttpContent? Body, object? Content) ProcessBody(JsonValue bodyJson)
    {
        switch (bodyJson)
        {
            case JsonString str:
            {
                var (body, content) = GetRawBody(str);
                return ("raw", body, content);
            }

            case JsonObject obj:
            {
                var bodyType = "raw";

                if (obj.TryGetProperty<JsonString>("type", out var typeJson)) bodyType = typeJson.ToString(_context);

                switch (bodyType)
                {
                    case "raw":
                    {
                        var (body, content) = obj.TryGetProperty<JsonString>("value", out var rawData)
                            ? GetRawBody(rawData)
                            : (null, null);
                        return (bodyType, body, content);
                    }

                    case "text":
                    case "json":
                    {
                        var content = obj.TryGetProperty("value", out var textData)
                            ? Utilities.ToString(textData, _context)
                            : null;

                        return (bodyType, new StringContent(content ?? string.Empty, Encoding.UTF8,
                            bodyType == "text" ? MediaTypeNames.Text.Plain : MediaTypeNames.Application.Json), content);
                    }

                    case "form":
                    {
                        var (body, content) = GetFormData(obj);
                        return (bodyType, body, content);
                    }

                    case "multipart":
                    {
                        var (body, content) = GetMultipartData(obj);
                        return (bodyType, body, content);
                    }

                    default:
                        throw new ConfigProcessingException(
                            "Invalid body type. Must be one of: raw, text, json, form, multipart", typeJson.Span);
                }
            }

            default:
                throw new ConfigProcessingException(
                    "Invalid body value. Must be a string or an object with a `type` property and a `value` property.",
                    bodyJson.Span
                );
        }


        (HttpContent Body, object? Content) GetRawBody(JsonString bodyJsonStr)
        {
            var value = bodyJsonStr.Template.HasExpressions
                ? TemplateStringEvaluator.EvaluateValue(bodyJsonStr.Template, _context)
                : bodyJsonStr.Text;

            if (value is not FileReference fileRef)
            {
                var serialized = Utilities.ToString(value, _context);
                return (
                    new StringContent(
                        serialized ?? string.Empty,
                        Encoding.UTF8,
                        MediaTypeNames.Text.Plain
                    ),
                    serialized
                );
            }

            var stream = File.OpenRead(fileRef.Path);
            var content = new StreamContent(stream);
            if (!string.IsNullOrEmpty(fileRef.ContentType))
                content.Headers.ContentType = MediaTypeHeaderValue.Parse(fileRef.ContentType);

            return (content, fileRef);
        }

        (FormUrlEncodedContent?, List<KeyValuePair<string, string>>) GetFormData(JsonObject bodyJsonObj)
        {
            var data = new List<KeyValuePair<string, string>>();
            var formEncodedData = bodyJsonObj.GetProperty("value");

            switch (formEncodedData)
            {
                case null:
                    return (null, data);

                case JsonString formEncodedStr:
                {
                    var evaluated = formEncodedStr.ToObject(_context);

                    if (evaluated is not Dictionary<string, object?> dict)
                        throw new ConfigProcessingException(
                            "The template expression for the form encoded body must evaluate to an object",
                            formEncodedStr.Span
                        );

                    AddEntries(dict);
                    break;
                }

                case JsonObject formEncodedObj:
                {
                    var evaluated = (Dictionary<string, object?>)Utilities.Convert(formEncodedObj, _context)!;
                    AddEntries(evaluated);
                    break;
                }

                default:
                    throw new ConfigProcessingException(
                        "Form data must be an object or a template string evaluating to an object",
                        formEncodedData.Span
                    );
            }

            return (new FormUrlEncodedContent(data), data);

            void AddEntries(Dictionary<string, object?> dict)
            {
                foreach (var (k, v) in dict)
                    switch (v)
                    {
                        case null: continue;

                        case object[] arr:
                            data.AddRange(arr.OfType<string>().Select(s => new KeyValuePair<string, string>(k, s)));
                            break;

                        default:
                        {
                            data.Add(
                                new KeyValuePair<string, string>(k, Utilities.ToString(v, _context)!)
                            );
                            break;
                        }
                    }
            }
        }

        (MultipartFormDataContent?, Dictionary<string, object>) GetMultipartData(JsonObject bodyJsonObj)
        {
            if (!bodyJsonObj.TryGetProperty("value", out var partsJson))
                return (null, new Dictionary<string, object>());

            var multipartData = new Dictionary<string, object>();
            var multipart = new MultipartFormDataContent();

            switch (partsJson)
            {
                case JsonNull:
                    break;

                case JsonString strJson:
                {
                    var evaluated = strJson.ToObject(_context);
                    if (evaluated is not Dictionary<string, object?> partsDict)
                        throw new ConfigProcessingException(
                            "The template expression for the multipart body must evaluate to an object",
                            strJson.Span
                        );

                    AddParts(partsDict);
                    break;
                }

                case JsonObject partsObj:
                {
                    var evaluated = (Dictionary<string, object?>)Utilities.Convert(partsObj, _context)!;
                    AddParts(evaluated);
                    break;
                }

                default:
                    throw new ConfigProcessingException(
                        "Multipart body must be an object or a template string evaluating to an object",
                        partsJson!.Span
                    );
            }

            return (multipart, multipartData);

            void AddParts(Dictionary<string, object?> partsDict)
            {
                foreach (var (partName, partVal) in partsDict)
                    switch (partVal)
                    {
                        case null:
                            continue;

                        case object[] arr:
                        {
                            foreach (var i in arr)
                                if (i is FileReference fileRef)
                                    fileRef.FileName ??= Path.GetFileName(fileRef.Path);

                            multipartData[partName] = arr;
                            break;
                        }

                        default:
                        {
                            var evaluated = Utilities.ToString(partVal, _context);
                            if (evaluated is not null) multipartData[partName] = evaluated;
                            break;
                        }
                    }

                foreach (var (k, v) in multipartData)
                    switch (v)
                    {
                        case FileReference fileReference:
                        {
                            AddFileRef(k, fileReference);
                            break;
                        }

                        case string str:
                        {
                            multipart.Add(new StringContent(str, Encoding.UTF8), k);
                            break;
                        }

                        case object?[] arr:
                        {
                            foreach (var i in arr)
                                switch (i)
                                {
                                    case null:
                                        continue;
                                    case FileReference fileRef:
                                        AddFileRef(k, fileRef);
                                        break;
                                    default:
                                        multipart.Add(
                                            new StringContent(Utilities.ToString(i, _context)!, Encoding.UTF8),
                                            k
                                        );
                                        break;
                                }

                            break;
                        }
                    }
            }

            void AddFileRef(string key, FileReference fileRef)
            {
                var stream = File.OpenRead(fileRef.Path);
                var streamContent = new StreamContent(stream);
                if (!string.IsNullOrEmpty(fileRef.ContentType))
                    streamContent.Headers.ContentType =
                        MediaTypeHeaderValue.Parse(fileRef.ContentType);

                multipart.Add(streamContent, key, fileRef.FileName!);
            }
        }
    }

    public Request ProcessRequest(JsonObject request)
    {
        var req = new Request();

        if (!request.TryGetProperty<JsonString>("url", out var urlJson))
            request.ThrowError("The request object must contain a `url` property");

        request.TryGetProperty<JsonObject>("query", out var queryJson);
        (req.Url, req.FullUrl, req.Query) = ProcessUrl(urlJson, queryJson);

        req.Method = request.TryGetProperty<JsonString>("method", out var method)
            ? method.ToString(_context)
            : "GET";

        if (request.TryGetProperty<JsonObject>("headers", out var headers))
            req.Headers = ProcessHeaders(headers);

        if (request.TryGetProperty<JsonObject>("cookies", out var cookies))
            foreach (var (cName, cVal) in cookies.Properties)
            {
                var serialized = Utilities.ToString(cVal, _context);
                if (serialized is not null)
                    req.Cookies[cName] = serialized;
            }

        if (request.TryGetProperty<JsonNumber>("timeout", out var timeout))
            req.Timeout = timeout.Value;

        if (request.TryGetProperty("body", out var bodyJson))
            (req.Type, req.Body, req.Content) = ProcessBody(bodyJson!);

        return req;
    }

    private async Task<Response> ProcessAndExecuteRequest(JsonObject requestJson, string requestId)
    {
        return await ExecuteRequest(ProcessRequest(requestJson), requestId);
    }

    public async Task<Response> ExecuteRequest(Request request, string? requestId = null)
    {
        var httpRequest = request.ToMessage();

        using var httpClient = new HttpClient();
        if (request.Timeout is not null) httpClient.Timeout = TimeSpan.FromMilliseconds(request.Timeout.Value);

        _stopWatch.Start();
        var httpResponse = await httpClient.SendAsync(httpRequest);
        _stopWatch.Stop();

        var response = await Response.Create(httpResponse, _stopWatch.ElapsedMilliseconds);

        _stopWatch.Reset();

        return response;
    }
}