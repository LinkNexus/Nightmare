using Nightmare.Parser;

namespace Nightmare.Tests.ParserTests;

public class TokenTests
{
    [Fact]
    public void Token_Constructor_StoresAllProperties()
    {
        var span = new TextSpan(0, 5, 1, 1, 1, 5);
        var template = new TemplateString([]);
        var token = new Token(TokenType.String, "test", template, span);

        Assert.Equal(TokenType.String, token.Type);
        Assert.Equal("test", token.Value);
        Assert.Equal(template, token.Template);
        Assert.Equal(span, token.Span);
    }

    [Fact]
    public void Token_WithNullValue_HandlesCorrectly()
    {
        var span = new TextSpan(0, 1, 1, 1, 1, 1);
        var token = new Token(TokenType.LeftBrace, null, null, span);

        Assert.Null(token.Value);
        Assert.Null(token.Template);
    }

    [Fact]
    public void Token_ToString_FormatsCorrectly()
    {
        var span = new TextSpan(0, 3, 1, 1, 1, 3);
        var token = new Token(TokenType.Number, "123", null, span);

        var result = token.ToString();

        Assert.Contains("Number", result);
        Assert.Contains("123", result);
    }

    [Theory]
    [InlineData(TokenType.LeftBrace)]
    [InlineData(TokenType.RightBrace)]
    [InlineData(TokenType.LeftBracket)]
    [InlineData(TokenType.RightBracket)]
    [InlineData(TokenType.Colon)]
    [InlineData(TokenType.Comma)]
    [InlineData(TokenType.String)]
    [InlineData(TokenType.Number)]
    [InlineData(TokenType.True)]
    [InlineData(TokenType.False)]
    [InlineData(TokenType.Null)]
    [InlineData(TokenType.EndOfFile)]
    public void TokenType_AllTypesAreValid(TokenType type)
    {
        var span = new TextSpan(0, 1, 1, 1, 1, 1);
        var token = new Token(type, null, null, span);

        Assert.Equal(type, token.Type);
    }
}
