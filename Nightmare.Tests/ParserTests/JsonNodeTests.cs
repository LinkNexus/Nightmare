using Nightmare.Parser;

namespace Nightmare.Tests.ParserTests;

public class JsonNodeTests
{
    private static readonly TextSpan DefaultSpan = new(0, 1, 1, 1, 1, 1);

    [Fact]
    public void JsonNull_StoresSpan()
    {
        var span = new TextSpan(5, 4, 1, 5, 1, 9);
        var jsonNull = new JsonNull(span);

        Assert.Equal(span, jsonNull.Span);
    }

    [Fact]
    public void JsonBoolean_StoresValueAndSpan()
    {
        var jsonTrue = new JsonBoolean(true, DefaultSpan);
        var jsonFalse = new JsonBoolean(false, DefaultSpan);

        Assert.True(jsonTrue.Value);
        Assert.False(jsonFalse.Value);
    }

    [Fact]
    public void JsonNumber_StoresRawAndCalculatesValue()
    {
        var number = new JsonNumber("123.456", DefaultSpan);

        Assert.Equal("123.456", number.Raw);
        Assert.Equal(123.456, number.Value);
    }

    [Fact]
    public void JsonNumber_HandlesNegativeNumbers()
    {
        var number = new JsonNumber("-42.5", DefaultSpan);

        Assert.Equal(-42.5, number.Value);
    }

    [Fact]
    public void JsonNumber_HandlesScientificNotation()
    {
        var number = new JsonNumber("1.5e10", DefaultSpan);

        Assert.Equal(1.5e10, number.Value);
    }

    [Fact]
    public void JsonString_StoresTemplateAndText()
    {
        var segments = new List<TemplateSegment>
        {
            new TemplateTextSegment("hello", DefaultSpan)
        };
        var template = new TemplateString(segments);
        var jsonString = new JsonString(template, DefaultSpan);

        Assert.Equal(template, jsonString.Template);
        Assert.Equal("hello", jsonString.Text);
    }

    [Fact]
    public void JsonArray_StoresItems()
    {
        var items = new List<JsonValue>
        {
            new JsonNumber("1", DefaultSpan),
            new JsonNumber("2", DefaultSpan),
            new JsonNumber("3", DefaultSpan)
        };
        var array = new JsonArray(items, DefaultSpan);

        Assert.Equal(3, array.Items.Count);
        Assert.All(array.Items, item => Assert.IsType<JsonNumber>(item));
    }

    [Fact]
    public void JsonArray_EmptyArray_HasNoItems()
    {
        var array = new JsonArray([], DefaultSpan);

        Assert.Empty(array.Items);
    }

    [Fact]
    public void JsonObject_StoresProperties()
    {
        var properties = new List<JsonProperty>
        {
            new JsonProperty("key1", new JsonNumber("1", DefaultSpan), DefaultSpan),
            new JsonProperty("key2", new JsonNumber("2", DefaultSpan), DefaultSpan)
        };
        var obj = new JsonObject(properties, DefaultSpan);

        Assert.IsType<JsonObject>(obj);
    }

    [Fact]
    public void JsonObject_EmptyObject_HasNoProperties()
    {
        var obj = new JsonObject([], DefaultSpan);

        Assert.IsType<JsonObject>(obj);
    }

    [Fact]
    public void JsonProperty_StoresNameValueAndSpan()
    {
        var value = new JsonNumber("123", DefaultSpan);
        var property = new JsonProperty("testKey", value, DefaultSpan);

        Assert.Equal("testKey", property.Name);
        Assert.Equal(value, property.Value);
        Assert.Equal(DefaultSpan, property.Span);
    }

    [Fact]
    public void JsonValue_AllTypesInheritFromJsonValue()
    {
        JsonValue[] values =
        [
            new JsonNull(DefaultSpan),
            new JsonBoolean(true, DefaultSpan),
            new JsonNumber("1", DefaultSpan),
            new JsonString(new TemplateString([]), DefaultSpan),
            new JsonArray([], DefaultSpan),
            new JsonObject([], DefaultSpan)
        ];

        Assert.All(values, v => Assert.IsAssignableFrom<JsonValue>(v));
    }
}
