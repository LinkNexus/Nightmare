using System.Globalization;

namespace Nightmare.JsonParser;

public abstract class JsonValue(TextSpan span)
{
    public TextSpan Span { get; } = span;
}

public sealed class JsonObject(
    IEnumerable<JsonProperty> properties,
    TextSpan span
) : JsonValue(span)
{
    private readonly Dictionary<string, JsonValue> _properties =
        properties.ToDictionary(p => p.Name, p => p.Value, StringComparer.Ordinal);
}

public sealed class JsonArray(IEnumerable<JsonValue> items, TextSpan span) : JsonValue(span)
{
    public IReadOnlyList<JsonValue> Items { get; } = [..items];
}

public sealed class JsonString(
    TemplateString template,
    TextSpan span
) : JsonValue(span)
{
    public TemplateString Template { get; } = template;
    public string Text => Template.ToString();
}

public sealed class JsonNumber(string raw, TextSpan span) : JsonValue(span)
{
    public string Raw { get; } = raw;
    public double Value => double.Parse(Raw, CultureInfo.InvariantCulture);
}

public sealed class JsonBoolean(bool value, TextSpan span) : JsonValue(span)
{
    public bool Value { get; } = value;
}

public sealed class JsonNull(TextSpan span) : JsonValue(span)
{
}

public sealed record JsonProperty(
    string Name,
    JsonValue Value,
    TextSpan Span
)
{
}