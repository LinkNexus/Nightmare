using System.Globalization;

namespace Nightmare.Parser;

public abstract class JsonValue(TextSpan span)
{
    public TextSpan Span { get; } = span;
}

public sealed class JsonObject(IEnumerable<JsonProperty> properties, TextSpan span)
    : JsonValue(span)
{
    private readonly Dictionary<string, JsonValue> _properties = properties.ToDictionary(
        p => p.Name,
        p => p.Value,
        StringComparer.Ordinal
    );

    public IReadOnlyDictionary<string, JsonValue> Properties => _properties;

    public bool HasProperty(string name)
    {
        return _properties.ContainsKey(name);
    }

    public JsonValue? GetProperty(string name)
    {
        return _properties.GetValueOrDefault(name);
    }

    public bool TryGetProperty(string name, out JsonValue? value)
    {
        return _properties.TryGetValue(name, out value);
    }

    public bool TryGetProperty<T>(string name, out T value) where T : JsonValue
    {
        if (_properties.TryGetValue(name, out var v))
        {
            if (v is T typed)
            {
                value = typed;
                return true;
            }

            throw new JsonProcessingException($"Property '{name}' is not of type '{typeof(T).Name}'", v.Span);
        }

        value = default;
        return false;
    }
}

public sealed class JsonArray(IEnumerable<JsonValue> items, TextSpan span) : JsonValue(span)
{
    public IReadOnlyList<JsonValue> Items { get; } = [.. items];
}

public sealed class JsonString(TemplateString template, TextSpan span) : JsonValue(span)
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

public sealed record JsonProperty(string Name, JsonValue Value, TextSpan Span)
{
}

public class JsonProcessingException(string message, TextSpan span)
    : TracedException($"Error processing json data: {message}", span)
{
}