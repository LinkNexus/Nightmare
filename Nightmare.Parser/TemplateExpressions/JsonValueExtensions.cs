namespace Nightmare.Parser.TemplateExpressions;

public static class JsonValueExtensions
{
    public static object? Convert(JsonValue? value, EvaluationContext context)
    {
        return value switch
        {
            JsonObject obj => Convert(obj, context),
            JsonArray array => Convert(array, context),
            JsonString str => Convert(str, context),
            JsonBoolean boolean => boolean.Value,
            JsonNull => null,
            JsonNumber number => number.Value,
            null => null,
            _ => throw new ArgumentException($"Unexpected value type: {value.GetType()}")
        };
    }

    public static string Convert(JsonString str, EvaluationContext context)
    {
        return str.Template.HasExpressions
            ? TemplateStringEvaluator.Evaluate(str.Template, context)
            : str.Text;
    }

    public static object?[] Convert(JsonArray array, EvaluationContext context)
    {
        return array.Items.Select(item => Convert(item, context)).ToArray();
    }

    public static Dictionary<string, object?> Convert(JsonObject obj, EvaluationContext context)
    {
        var dict = new Dictionary<string, object?>();

        foreach (var (key, value) in obj.Properties) dict[key] = Convert(value, context);

        return dict;
    }
}