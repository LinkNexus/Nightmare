using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Nightmare.Parser.TemplateExpressions;

public static class JsonValueExtensions
{
    public static object? Convert(JsonValue? value, EvaluationContext? context = null)
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

    public static string? ToString(object? obj, EvaluationContext context)
    {
        return obj switch
        {
            null => null,
            string str => str,
            bool b => b.ToString(),
            double d => d.ToString(CultureInfo.InvariantCulture),
            Dictionary<string, object?> dict => Serialize(dict, context),
            List<object?> list => Serialize(list, context),
            JsonValue jsonValue => ToString(Convert(jsonValue, context), context),
            _ => throw new ArgumentException($"Unexpected value type: {obj?.GetType()}")
        };
    }

    public static string Serialize(object? value, EvaluationContext context)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(
            stream,
            new JsonWriterOptions { Indented = true }
        );

        WriteValue(value);
        writer.Flush();
        return Encoding.UTF8.GetString(stream.ToArray());

        void WriteValue(object? value)
        {
            switch (value)
            {
                case null:
                    writer.WriteNullValue();
                    break;
                case string str:
                    writer.WriteStringValue(str);
                    break;
                case bool boolVal:
                    writer.WriteBooleanValue(boolVal);
                    break;
                case double num:
                    writer.WriteNumberValue(num);
                    break;
                case IEnumerable<object?> array:
                    writer.WriteStartArray();
                    foreach (var item in array) WriteValue(item);
                    writer.WriteEndArray();
                    break;
                case Dictionary<string, object?> dict:
                    writer.WriteStartObject();
                    foreach (var (k, v) in dict)
                    {
                        writer.WritePropertyName(k);
                        WriteValue(v);
                    }

                    writer.WriteEndObject();
                    break;
                case JsonValue jsonValue:
                    WriteValue(Convert(jsonValue, context));
                    break;
            }
        }
    }

    public static string Serialize(JsonValue value, EvaluationContext context)
    {
        return Serialize(Convert(value, context), context);
    }

    private static object? Convert(JsonString str, EvaluationContext? context = null)
    {
        if (context is null) return str.Text;

        return str.Template.HasExpressions
            ? TemplateStringEvaluator.EvaluateValue(str.Template, context)
            : str.Text;
    }

    private static object?[] Convert(JsonArray array, EvaluationContext? context = null)
    {
        return array.Items.Select(item => Convert(item, context)).ToArray();
    }

    private static Dictionary<string, object?> Convert(JsonObject obj, EvaluationContext? context = null)
    {
        var dict = new Dictionary<string, object?>();

        foreach (var (key, value) in obj.Properties) dict[key] = Convert(value, context);

        return dict;
    }
}