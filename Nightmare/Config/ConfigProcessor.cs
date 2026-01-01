using Nightmare.Parser;
using Nightmare.Parser.TemplateExpressions;
using Terminal.Gui.App;

namespace Nightmare.Config;

public partial class ConfigProcessor
{
    private readonly EvaluationContext _context;

    public ConfigProcessor(IApplication application)
    {
        _context = new EvaluationContext(application)
        {
            RequestExecutor = ProcessAndExecuteRequest
        };
    }

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

        ProcessVariables(profile, ast);
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

        ProcessVariables(profilePair.Value, ast);

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

    private void ProcessVariables(JsonObject profile, JsonObject ast)
    {
        _context.ClearVariables();

        if (!profile.TryGetProperty<JsonObject>("data", out var variables)) return;

        foreach (var (key, value) in variables.Properties)
            _context.SetVariable(key, Utilities.Convert(value, _context));

        _context.Ast = ast;
    }
}