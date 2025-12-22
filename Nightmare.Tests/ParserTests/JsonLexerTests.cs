using Nightmare.Parser;

namespace Nightmare.Tests.ParserTests;

public class JsonLexerTests
{
    [Fact]
    public void Lex_EmptyString_ReturnsEndOfFileToken()
    {
        var lexer = new JsonLexer("");
        var tokens = lexer.Lex();

        Assert.Single(tokens);
        Assert.Equal(TokenType.EndOfFile, tokens[0].Type);
    }

    [Fact]
    public void Lex_Whitespace_ReturnsEndOfFileToken()
    {
        var lexer = new JsonLexer("   \t\n\r\n  ");
        var tokens = lexer.Lex();

        Assert.Single(tokens);
        Assert.Equal(TokenType.EndOfFile, tokens[0].Type);
    }

    [Theory]
    [InlineData("{", TokenType.LeftBrace)]
    [InlineData("}", TokenType.RightBrace)]
    [InlineData("[", TokenType.LeftBracket)]
    [InlineData("]", TokenType.RightBracket)]
    [InlineData(":", TokenType.Colon)]
    [InlineData(",", TokenType.Comma)]
    public void Lex_SingleCharacterToken_ReturnsCorrectToken(string input, TokenType expectedType)
    {
        var lexer = new JsonLexer(input);
        var tokens = lexer.Lex();

        Assert.Equal(2, tokens.Count);
        Assert.Equal(expectedType, tokens[0].Type);
        Assert.Equal(TokenType.EndOfFile, tokens[1].Type);
    }

    [Theory]
    [InlineData("true", TokenType.True)]
    [InlineData("false", TokenType.False)]
    [InlineData("null", TokenType.Null)]
    public void Lex_Keywords_ReturnsCorrectToken(string input, TokenType expectedType)
    {
        var lexer = new JsonLexer(input);
        var tokens = lexer.Lex();

        Assert.Equal(2, tokens.Count);
        Assert.Equal(expectedType, tokens[0].Type);
        Assert.Equal(input, tokens[0].Value);
    }

    [Theory]
    [InlineData("0", "0")]
    [InlineData("123", "123")]
    [InlineData("-456", "-456")]
    [InlineData("0.5", "0.5")]
    [InlineData("-0.5", "-0.5")]
    [InlineData("123.456", "123.456")]
    [InlineData("1e10", "1e10")]
    [InlineData("1E10", "1E10")]
    [InlineData("1e+10", "1e+10")]
    [InlineData("1e-10", "1e-10")]
    [InlineData("1.5e10", "1.5e10")]
    [InlineData("-1.5e-10", "-1.5e-10")]
    public void Lex_ValidNumbers_ReturnsNumberToken(string input, string expectedValue)
    {
        var lexer = new JsonLexer(input);
        var tokens = lexer.Lex();

        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.Number, tokens[0].Type);
        Assert.Equal(expectedValue, tokens[0].Value);
    }

    [Theory]
    [InlineData("\"\"", "")]
    [InlineData("\"hello\"", "hello")]
    [InlineData("\"hello world\"", "hello world")]
    [InlineData("\"\\\"\"", "\"")]
    [InlineData("\"\\\\\"", "\\")]
    [InlineData("\"\\/\"", "/")]
    [InlineData("\"\\b\"", "\b")]
    [InlineData("\"\\f\"", "\f")]
    [InlineData("\"\\n\"", "\n")]
    [InlineData("\"\\r\"", "\r")]
    [InlineData("\"\\t\"", "\t")]
    public void Lex_ValidStrings_ReturnsStringToken(string input, string expectedValue)
    {
        var lexer = new JsonLexer(input);
        var tokens = lexer.Lex();

        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.String, tokens[0].Type);
        Assert.Equal(expectedValue, tokens[0].Value);
    }

    [Fact]
    public void Lex_StringWithUnicodeEscape_DecodesCorrectly()
    {
        var lexer = new JsonLexer("\"\\u0041\\u0042\\u0043\"");
        var tokens = lexer.Lex();

        Assert.Equal(TokenType.String, tokens[0].Type);
        Assert.Equal("ABC", tokens[0].Value);
    }

    [Fact]
    public void Lex_StringWithTemplateExpression_ParsesCorrectly()
    {
        var lexer = new JsonLexer("\"Hello {{name}}!\"");
        var tokens = lexer.Lex();

        Assert.Equal(TokenType.String, tokens[0].Type);
        Assert.NotNull(tokens[0].Template);
        Assert.True(tokens[0].Template.HasExpressions);
    }

    [Fact]
    public void Lex_StringWithMultipleTemplateExpressions_ParsesCorrectly()
    {
        var lexer = new JsonLexer("\"{{first}} {{last}}\"");
        var tokens = lexer.Lex();

        Assert.Equal(TokenType.String, tokens[0].Type);
        Assert.NotNull(tokens[0].Template);
        Assert.True(tokens[0].Template.HasExpressions);
    }

    [Fact]
    public void Lex_MultipleTokens_ReturnsAllTokens()
    {
        var lexer = new JsonLexer("{\"key\":123}");
        var tokens = lexer.Lex();

        Assert.Equal(6, tokens.Count);
        Assert.Equal(TokenType.LeftBrace, tokens[0].Type);
        Assert.Equal(TokenType.String, tokens[1].Type);
        Assert.Equal(TokenType.Colon, tokens[2].Type);
        Assert.Equal(TokenType.Number, tokens[3].Type);
        Assert.Equal(TokenType.RightBrace, tokens[4].Type);
        Assert.Equal(TokenType.EndOfFile, tokens[5].Type);
    }

    [Fact]
    public void Lex_TokensWithWhitespace_IgnoresWhitespace()
    {
        var lexer = new JsonLexer(" { \n \"key\" \t : \r\n 123 \n } ");
        var tokens = lexer.Lex();

        Assert.Equal(6, tokens.Count);
        Assert.Equal(TokenType.LeftBrace, tokens[0].Type);
        Assert.Equal(TokenType.String, tokens[1].Type);
        Assert.Equal(TokenType.Colon, tokens[2].Type);
        Assert.Equal(TokenType.Number, tokens[3].Type);
        Assert.Equal(TokenType.RightBrace, tokens[4].Type);
    }

    [Fact]
    public void Lex_UnterminatedString_ThrowsException()
    {
        var lexer = new JsonLexer("\"hello");

        Assert.Throws<JsonParseException>(() => lexer.Lex());
    }

    [Fact]
    public void Lex_StringWithNewline_ThrowsException()
    {
        var lexer = new JsonLexer("\"hello\nworld\"");

        Assert.Throws<JsonParseException>(() => lexer.Lex());
    }

    [Fact]
    public void Lex_InvalidEscapeSequence_ThrowsException()
    {
        var lexer = new JsonLexer("\"\\x\"");

        Assert.Throws<JsonParseException>(() => lexer.Lex());
    }

    [Fact]
    public void Lex_InvalidCharacter_ThrowsException()
    {
        var lexer = new JsonLexer("@");

        Assert.Throws<JsonParseException>(() => lexer.Lex());
    }

    [Fact]
    public void Lex_UnterminatedTemplateExpression_ThrowsException()
    {
        var lexer = new JsonLexer("\"{{name\"");

        Assert.Throws<JsonParseException>(() => lexer.Lex());
    }

    [Fact]
    public void Lex_TextSpan_TracksLineAndColumn()
    {
        var lexer = new JsonLexer("{\n  \"key\"\n}");
        var tokens = lexer.Lex();

        var stringToken = tokens[1];
        Assert.Equal(2, stringToken.Span.StartLine);
        Assert.Equal(3, stringToken.Span.StartColumn);
    }
}