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

    public string ProcessProfiles(
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
        return profilePair.Key;
    }

    private void ProcessVariables(JsonObject profile)
    {
        _context.ClearVariables();

        if (!profile.TryGetProperty<JsonObject>("data", out var variables)) return;

        foreach (var (key, value) in variables.Properties)
            _context.SetVariable(key, JsonValueExtensions.Convert(value, _context));
    }
}