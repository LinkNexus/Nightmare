using Nightmare.Parser;

namespace Nightmare.Tests.ParserTests;

public class JsonParserTests
{
    [Fact]
    public void Parse_Null_ReturnsJsonNull()
    {
        var result = JsonParser.Parse("null");

        Assert.IsType<JsonNull>(result);
    }

    [Fact]
    public void Parse_True_ReturnsJsonBoolean()
    {
        var result = JsonParser.Parse("true");

        var boolean = Assert.IsType<JsonBoolean>(result);
        Assert.True(boolean.Value);
    }

    [Fact]
    public void Parse_False_ReturnsJsonBoolean()
    {
        var result = JsonParser.Parse("false");

        var boolean = Assert.IsType<JsonBoolean>(result);
        Assert.False(boolean.Value);
    }

    [Theory]
    [InlineData("0", 0.0)]
    [InlineData("123", 123.0)]
    [InlineData("-456", -456.0)]
    [InlineData("0.5", 0.5)]
    [InlineData("-0.5", -0.5)]
    [InlineData("123.456", 123.456)]
    [InlineData("1e10", 1e10)]
    [InlineData("1e+10", 1e+10)]
    [InlineData("1e-10", 1e-10)]
    public void Parse_Number_ReturnsJsonNumber(string input, double expected)
    {
        var result = JsonParser.Parse(input);

        var number = Assert.IsType<JsonNumber>(result);
        Assert.Equal(expected, number.Value);
        Assert.Equal(input, number.Raw);
    }

    [Theory]
    [InlineData("\"\"", "")]
    [InlineData("\"hello\"", "hello")]
    [InlineData("\"hello world\"", "hello world")]
    [InlineData("\"\\\"quoted\\\"\"", "\"quoted\"")]
    [InlineData("\"line1\\nline2\"", "line1\nline2")]
    public void Parse_String_ReturnsJsonString(string input, string expected)
    {
        var result = JsonParser.Parse(input);

        var str = Assert.IsType<JsonString>(result);
        Assert.Equal(expected, str.Text);
    }

    [Fact]
    public void Parse_StringWithTemplateExpression_PreservesTemplate()
    {
        var result = JsonParser.Parse("\"Hello {{name}}!\"");

        var str = Assert.IsType<JsonString>(result);
        Assert.True(str.Template.HasExpressions);
    }

    [Fact]
    public void Parse_EmptyArray_ReturnsJsonArray()
    {
        var result = JsonParser.Parse("[]");

        var array = Assert.IsType<JsonArray>(result);
        Assert.Empty(array.Items);
    }

    [Fact]
    public void Parse_ArrayWithSingleElement_ReturnsJsonArray()
    {
        var result = JsonParser.Parse("[123]");

        var array = Assert.IsType<JsonArray>(result);
        Assert.Single(array.Items);
        Assert.IsType<JsonNumber>(array.Items[0]);
    }

    [Fact]
    public void Parse_ArrayWithMultipleElements_ReturnsJsonArray()
    {
        var result = JsonParser.Parse("[1, 2, 3]");

        var array = Assert.IsType<JsonArray>(result);
        Assert.Equal(3, array.Items.Count);
        Assert.All(array.Items, item => Assert.IsType<JsonNumber>(item));
    }

    [Fact]
    public void Parse_ArrayWithMixedTypes_ReturnsJsonArray()
    {
        var result = JsonParser.Parse("[1, \"hello\", true, null]");

        var array = Assert.IsType<JsonArray>(result);
        Assert.Equal(4, array.Items.Count);
        Assert.IsType<JsonNumber>(array.Items[0]);
        Assert.IsType<JsonString>(array.Items[1]);
        Assert.IsType<JsonBoolean>(array.Items[2]);
        Assert.IsType<JsonNull>(array.Items[3]);
    }

    [Fact]
    public void Parse_NestedArray_ReturnsJsonArray()
    {
        var result = JsonParser.Parse("[[1, 2], [3, 4]]");

        var array = Assert.IsType<JsonArray>(result);
        Assert.Equal(2, array.Items.Count);
        Assert.IsType<JsonArray>(array.Items[0]);
        Assert.IsType<JsonArray>(array.Items[1]);
    }

    [Fact]
    public void Parse_EmptyObject_ReturnsJsonObject()
    {
        var result = JsonParser.Parse("{}");

        Assert.IsType<JsonObject>(result);
    }

    [Fact]
    public void Parse_ObjectWithSingleProperty_ReturnsJsonObject()
    {
        var result = JsonParser.Parse("{\"key\": 123}");

        Assert.IsType<JsonObject>(result);
    }

    [Fact]
    public void Parse_ObjectWithMultipleProperties_ReturnsJsonObject()
    {
        var result = JsonParser.Parse("{\"a\": 1, \"b\": 2, \"c\": 3}");

        Assert.IsType<JsonObject>(result);
    }

    [Fact]
    public void Parse_ObjectWithMixedValueTypes_ReturnsJsonObject()
    {
        var result = JsonParser.Parse("{\"num\": 123, \"str\": \"hello\", \"bool\": true, \"null\": null}");

        Assert.IsType<JsonObject>(result);
    }

    [Fact]
    public void Parse_NestedObject_ReturnsJsonObject()
    {
        var result = JsonParser.Parse("{\"outer\": {\"inner\": 123}}");

        var obj = Assert.IsType<JsonObject>(result);
    }

    [Fact]
    public void Parse_ComplexNestedStructure_ParsesCorrectly()
    {
        var json = """
                   {
                               "name": "John",
                               "age": 30,
                               "active": true,
                               "address": {
                                   "street": "123 Main St",
                                   "city": "Springfield"
                               },
                               "hobbies": ["reading", "gaming", "coding"]
                           }
                   """;

        var result = JsonParser.Parse(json);

        Assert.IsType<JsonObject>(result);
    }

    [Fact]
    public void Parse_ArrayWithTrailingComma_ThrowsException()
    {
        Assert.Throws<JsonParseException>(() => JsonParser.Parse("[1, 2,]"));
    }

    [Fact]
    public void Parse_ObjectWithTrailingComma_ThrowsException()
    {
        Assert.Throws<JsonParseException>(() => JsonParser.Parse("{\"key\": 123,}"));
    }

    [Fact]
    public void Parse_UnexpectedToken_ThrowsException()
    {
        Assert.Throws<JsonParseException>(() => JsonParser.Parse("@"));
    }

    [Fact]
    public void Parse_MissingCommaInArray_ThrowsException()
    {
        Assert.Throws<JsonParseException>(() => JsonParser.Parse("[1 2]"));
    }

    [Fact]
    public void Parse_MissingCommaInObject_ThrowsException()
    {
        Assert.Throws<JsonParseException>(() => JsonParser.Parse("{\"a\": 1 \"b\": 2}"));
    }

    [Fact]
    public void Parse_MissingColonInObject_ThrowsException()
    {
        Assert.Throws<JsonParseException>(() => JsonParser.Parse("{\"key\" 123}"));
    }

    [Fact]
    public void Parse_ObjectPropertyWithExpression_ThrowsException()
    {
        Assert.Throws<JsonParseException>(() => JsonParser.Parse("{\"key{{expr}}\": 123}"));
    }

    [Fact]
    public void Parse_MissingClosingBracket_ThrowsException()
    {
        Assert.Throws<JsonParseException>(() => JsonParser.Parse("[1, 2, 3"));
    }

    [Fact]
    public void Parse_MissingClosingBrace_ThrowsException()
    {
        Assert.Throws<JsonParseException>(() => JsonParser.Parse("{\"key\": 123"));
    }

    [Fact]
    public void Parse_ExtraContentAfterValue_ThrowsException()
    {
        Assert.Throws<JsonParseException>(() => JsonParser.Parse("123 456"));
    }

    [Fact]
    public void Parse_WithWhitespace_ParsesCorrectly()
    {
        var json = " \n\t { \n \"key\" \t : \r\n 123 \n } \t ";
        var result = JsonParser.Parse(json);

        Assert.IsType<JsonObject>(result);
    }

    [Fact]
    public void Parse_JsonValue_HasCorrectSpan()
    {
        var result = JsonParser.Parse("123");

        Assert.Equal(0, result.Span.Start);
        Assert.True(result.Span.Length > 0);
    }
}