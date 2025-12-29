using Nightmare.Parser;
using Nightmare.Parser.TemplateExpressions;
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

    private static KeyValuePair<string, JsonObject> GetDefaultProfile(JsonObject profiles)
    {
        try
        {
            var profilePair = profiles
                .Properties
                .First(p => p.Value is JsonObject pValue &&
                            pValue.TryGetProperty<JsonBoolean>("default", out var defaultProp)
                            && defaultProp is { Value: true });

            return new KeyValuePair<string, JsonObject>(
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
                ? new KeyValuePair<string, JsonObject>(selectedProfileName, profile)
                : GetDefaultProfile(profiles);

        ProcessVariables(profilePair.Value);

        return (
            profilePair.Key,
            profiles.Properties.Select(p => p.Key).ToArray()
        );
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

    private HttpContent ProcessBody(JsonObject request)
    {
        if (!request.TryGetProperty("body", out var body)) return null;

        if (body is JsonString bodyString)
        {
            return new StringContent(
                bodyString.Template.HasExpressions
                    ? TemplateStringEvaluator.Evaluate(bodyString.Template, _context)
                    : bodyString.Text
            );
        }

        else if (body is JsonObject bodyObject)
        {
            if (!bodyObject.TryGetProperty<JsonString>("type", out var type))
                type = null;

            switch (type)
            {
                case "text":
            }
        }
    }

    private static async Task<HttpResponseMessage> ExecuteRequest(JsonProperty requestProp, EvaluationContext context)
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

        if (!requestObject.TryGetProperty<JsonString>("method", out var method))
            method = null;

        var httpRequest = new HttpRequestMessage(
            new HttpMethod(
                method is not null
                    ? method.Template.HasExpressions
                        ? TemplateStringEvaluator.Evaluate(method.Template, context)
                        : method.Text
                    : "GET"
            ),
            url.Template.HasExpressions ? TemplateStringEvaluator.Evaluate(url.Template, context) : url.Text
        );
    }

    public async Task<HttpResponseMessage> ProcessAndExecuteRequest(JsonProperty request)
    {
        return await ExecuteRequest(request, _context);
    }

    public static async Task<HttpResponseMessage> ProcessAndExecuteRequest(
        JsonProperty request,
        EvaluationContext context
    )
    {
        return await ExecuteRequest(request, context);
    }
}