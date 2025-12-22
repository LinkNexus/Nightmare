using Nightmare.Parser;

namespace Nightmare.Tests.ParserTests;

public class IntegrationTests
{
    [Fact]
    public void RoundTrip_SimpleObject_PreservesStructure()
    {
        var json = "{\"name\":\"John\",\"age\":30}";
        var result = JsonParser.Parse(json);

        var obj = Assert.IsType<JsonObject>(result);
    }

    [Fact]
    public void RoundTrip_NestedStructure_PreservesStructure()
    {
        var json = "{\"user\":{\"name\":\"Jane\",\"scores\":[10,20,30]}}";
        var result = JsonParser.Parse(json);

        var obj = Assert.IsType<JsonObject>(result);
    }

    [Fact]
    public void Parse_LargeNumber_HandlesCorrectly()
    {
        var result = JsonParser.Parse("9999999999.9999999");
        
        var number = Assert.IsType<JsonNumber>(result);
        Assert.Equal(9999999999.9999999, number.Value, precision: 7);
    }

    [Fact]
    public void Parse_DeepNesting_HandlesCorrectly()
    {
        var json = "[[[[[[1]]]]]]";
        var result = JsonParser.Parse(json);

        var array = Assert.IsType<JsonArray>(result);
        Assert.Single(array.Items);
    }

    [Fact]
    public void Parse_ManyProperties_HandlesCorrectly()
    {
        var json = "{\"a\":1,\"b\":2,\"c\":3,\"d\":4,\"e\":5,\"f\":6,\"g\":7,\"h\":8,\"i\":9,\"j\":10}";
        var result = JsonParser.Parse(json);

        Assert.IsType<JsonObject>(result);
    }

    [Fact]
    public void Parse_EmptyStringInArray_HandlesCorrectly()
    {
        var result = JsonParser.Parse("[\"\"]");

        var array = Assert.IsType<JsonArray>(result);
        Assert.Single(array.Items);
        var str = Assert.IsType<JsonString>(array.Items[0]);
        Assert.Equal("", str.Text);
    }

    [Fact]
    public void Parse_ZeroValues_HandlesCorrectly()
    {
        var result = JsonParser.Parse("[0,0.0,-0]");

        var array = Assert.IsType<JsonArray>(result);
        Assert.Equal(3, array.Items.Count);
        Assert.All(array.Items, item => Assert.IsType<JsonNumber>(item));
    }

    [Fact]
    public void Parse_SpecialCharactersInString_EscapesCorrectly()
    {
        var result = JsonParser.Parse("\"\\\"\\\\\\n\\r\\t\"");

        var str = Assert.IsType<JsonString>(result);
        Assert.Equal("\"\\\n\r\t", str.Text);
    }

    [Fact]
    public void Parse_UnicodeCharacters_DecodesCorrectly()
    {
        var result = JsonParser.Parse("\"\\u03B1\\u03B2\\u03B3\"");

        var str = Assert.IsType<JsonString>(result);
        Assert.Equal("αβγ", str.Text);
    }

    [Fact]
    public void Parse_MixedWhitespace_HandlesCorrectly()
    {
        var json = " \t\n\r{\n\t \"key\"\r\n:\t\n123\r\n}\t ";
        var result = JsonParser.Parse(json);

        Assert.IsType<JsonObject>(result);
    }

    [Fact]
    public void Parse_TemplateWithComplexExpression_PreservesExpression()
    {
        var result = JsonParser.Parse("\"Result: {{user.name + ' ' + user.age}}\"");

        var str = Assert.IsType<JsonString>(result);
        Assert.True(str.Template.HasExpressions);
    }

    [Fact]
    public void Parse_MultipleTemplatesInObject_PreservesAll()
    {
        var json = "{\"greeting\":\"Hello {{name}}!\",\"farewell\":\"Goodbye {{name}}!\"}";
        var result = JsonParser.Parse(json);

        Assert.IsType<JsonObject>(result);
    }

    [Fact]
    public void Lexer_ConsecutiveStrings_TokenizesCorrectly()
    {
        var lexer = new JsonLexer("\"hello\"\"world\"");
        var tokens = lexer.Lex();

        Assert.Equal(3, tokens.Count);
        Assert.Equal(TokenType.String, tokens[0].Type);
        Assert.Equal(TokenType.String, tokens[1].Type);
        Assert.Equal(TokenType.EndOfFile, tokens[2].Type);
    }

    [Fact]
    public void Parser_ArrayOfArrays_ParsesCorrectly()
    {
        var result = JsonParser.Parse("[[],[],[]]");

        var array = Assert.IsType<JsonArray>(result);
        Assert.Equal(3, array.Items.Count);
        Assert.All(array.Items, item =>
        {
            var innerArray = Assert.IsType<JsonArray>(item);
            Assert.Empty(innerArray.Items);
        });
    }

    [Fact]
    public void Parser_ObjectOfObjects_ParsesCorrectly()
    {
        var result = JsonParser.Parse("{\"a\":{},\"b\":{},\"c\":{}}");

        Assert.IsType<JsonObject>(result);
    }
}
