using Nightmare.Parser.TemplateExpressions;

namespace Nightmare.Tests.ParserTests.TemplateExpressions;

public class TemplateExpressionLexerTests
{
    [Fact]
    public void Lex_EmptyString_ReturnsEndOfFileToken()
    {
        var lexer = new TemplateExpressionLexer("");
        var tokens = lexer.Lex();

        Assert.Single(tokens);
        Assert.Equal(TemplateTokenType.EndOfFile, tokens[0].Type);
    }

    [Fact]
    public void Lex_Whitespace_ReturnsEndOfFileToken()
    {
        var lexer = new TemplateExpressionLexer("   \t\n\r\n  ");
        var tokens = lexer.Lex();

        Assert.Single(tokens);
        Assert.Equal(TemplateTokenType.EndOfFile, tokens[0].Type);
    }

    [Theory]
    [InlineData("42", TemplateTokenType.Number, "42")]
    [InlineData("3.14", TemplateTokenType.Number, "3.14")]
    [InlineData("-10", TemplateTokenType.Number, "-10")]
    [InlineData("1.5e-10", TemplateTokenType.Number, "1.5e-10")]
    [InlineData("2.5E+5", TemplateTokenType.Number, "2.5E+5")]
    public void Lex_Numbers_ReturnsNumberToken(string input, TemplateTokenType expectedType, string expectedValue)
    {
        var lexer = new TemplateExpressionLexer(input);
        var tokens = lexer.Lex();

        Assert.Equal(2, tokens.Count);
        Assert.Equal(expectedType, tokens[0].Type);
        Assert.Equal(expectedValue, tokens[0].Value);
    }

    [Theory]
    [InlineData("\"hello\"", "hello")]
    [InlineData("'world'", "world")]
    [InlineData("\"hello world\"", "hello world")]
    [InlineData("\"\"", "")]
    public void Lex_Strings_ReturnsStringToken(string input, string expectedValue)
    {
        var lexer = new TemplateExpressionLexer(input);
        var tokens = lexer.Lex();

        Assert.Equal(2, tokens.Count);
        Assert.Equal(TemplateTokenType.String, tokens[0].Type);
        Assert.Equal(expectedValue, tokens[0].Value);
    }

    [Theory]
    [InlineData("\"\\n\"", "\n")]
    [InlineData("\"\\r\"", "\r")]
    [InlineData("\"\\t\"", "\t")]
    [InlineData("\"\\\"\"", "\"")]
    [InlineData("\"\\'\"", "'")]
    [InlineData("\"\\\\\"", "\\")]
    public void Lex_StringEscapes_ReturnsCorrectValue(string input, string expectedValue)
    {
        var lexer = new TemplateExpressionLexer(input);
        var tokens = lexer.Lex();

        Assert.Equal(expectedValue, tokens[0].Value);
    }

    [Theory]
    [InlineData("true", TemplateTokenType.True, "true")]
    [InlineData("false", TemplateTokenType.False, "false")]
    [InlineData("null", TemplateTokenType.Null, "null")]
    public void Lex_Keywords_ReturnsCorrectToken(string input, TemplateTokenType expectedType, string expectedValue)
    {
        var lexer = new TemplateExpressionLexer(input);
        var tokens = lexer.Lex();

        Assert.Equal(2, tokens.Count);
        Assert.Equal(expectedType, tokens[0].Type);
        Assert.Equal(expectedValue, tokens[0].Value);
    }

    [Theory]
    [InlineData("identifier", "identifier")]
    [InlineData("myVar", "myVar")]
    [InlineData("_test", "_test")]
    [InlineData("var123", "var123")]
    public void Lex_Identifiers_ReturnsIdentifierToken(string input, string expectedValue)
    {
        var lexer = new TemplateExpressionLexer(input);
        var tokens = lexer.Lex();

        Assert.Equal(2, tokens.Count);
        Assert.Equal(TemplateTokenType.Identifier, tokens[0].Type);
        Assert.Equal(expectedValue, tokens[0].Value);
    }

    [Theory]
    [InlineData("+", TemplateTokenType.Plus)]
    [InlineData("-", TemplateTokenType.Minus)]
    [InlineData("*", TemplateTokenType.Star)]
    [InlineData("/", TemplateTokenType.Slash)]
    [InlineData("%", TemplateTokenType.Percent)]
    public void Lex_ArithmeticOperators_ReturnsCorrectToken(string input, TemplateTokenType expectedType)
    {
        var lexer = new TemplateExpressionLexer(input);
        var tokens = lexer.Lex();

        Assert.Equal(2, tokens.Count);
        Assert.Equal(expectedType, tokens[0].Type);
    }

    [Theory]
    [InlineData("==", TemplateTokenType.Equal)]
    [InlineData("!=", TemplateTokenType.NotEqual)]
    [InlineData("<", TemplateTokenType.LessThan)]
    [InlineData("<=", TemplateTokenType.LessOrEqual)]
    [InlineData(">", TemplateTokenType.GreaterThan)]
    [InlineData(">=", TemplateTokenType.GreaterOrEqual)]
    public void Lex_ComparisonOperators_ReturnsCorrectToken(string input, TemplateTokenType expectedType)
    {
        var lexer = new TemplateExpressionLexer(input);
        var tokens = lexer.Lex();

        Assert.Equal(2, tokens.Count);
        Assert.Equal(expectedType, tokens[0].Type);
    }

    [Theory]
    [InlineData("&&", TemplateTokenType.And)]
    [InlineData("||", TemplateTokenType.Or)]
    [InlineData("!", TemplateTokenType.Not)]
    public void Lex_LogicalOperators_ReturnsCorrectToken(string input, TemplateTokenType expectedType)
    {
        var lexer = new TemplateExpressionLexer(input);
        var tokens = lexer.Lex();

        Assert.Equal(2, tokens.Count);
        Assert.Equal(expectedType, tokens[0].Type);
    }

    [Theory]
    [InlineData("(", TemplateTokenType.LeftParen)]
    [InlineData(")", TemplateTokenType.RightParen)]
    [InlineData("[", TemplateTokenType.LeftBracket)]
    [InlineData("]", TemplateTokenType.RightBracket)]
    [InlineData(".", TemplateTokenType.Dot)]
    [InlineData(",", TemplateTokenType.Comma)]
    [InlineData("?", TemplateTokenType.Question)]
    [InlineData(":", TemplateTokenType.Colon)]
    public void Lex_Delimiters_ReturnsCorrectToken(string input, TemplateTokenType expectedType)
    {
        var lexer = new TemplateExpressionLexer(input);
        var tokens = lexer.Lex();

        Assert.Equal(2, tokens.Count);
        Assert.Equal(expectedType, tokens[0].Type);
    }

    [Fact]
    public void Lex_ComplexExpression_ReturnsAllTokens()
    {
        var lexer = new TemplateExpressionLexer("a + b * 2");
        var tokens = lexer.Lex();

        Assert.Equal(6, tokens.Count);
        Assert.Equal(TemplateTokenType.Identifier, tokens[0].Type);
        Assert.Equal("a", tokens[0].Value);
        Assert.Equal(TemplateTokenType.Plus, tokens[1].Type);
        Assert.Equal(TemplateTokenType.Identifier, tokens[2].Type);
        Assert.Equal("b", tokens[2].Value);
        Assert.Equal(TemplateTokenType.Star, tokens[3].Type);
        Assert.Equal(TemplateTokenType.Number, tokens[4].Type);
        Assert.Equal("2", tokens[4].Value);
        Assert.Equal(TemplateTokenType.EndOfFile, tokens[5].Type);
    }

    [Fact]
    public void Lex_FunctionCall_ReturnsAllTokens()
    {
        var lexer = new TemplateExpressionLexer("func(a, b)");
        var tokens = lexer.Lex();

        Assert.Equal(7, tokens.Count);
        Assert.Equal(TemplateTokenType.Identifier, tokens[0].Type);
        Assert.Equal("func", tokens[0].Value);
        Assert.Equal(TemplateTokenType.LeftParen, tokens[1].Type);
        Assert.Equal(TemplateTokenType.Identifier, tokens[2].Type);
        Assert.Equal(TemplateTokenType.Comma, tokens[3].Type);
        Assert.Equal(TemplateTokenType.Identifier, tokens[4].Type);
        Assert.Equal(TemplateTokenType.RightParen, tokens[5].Type);
    }

    [Fact]
    public void Lex_MemberAccess_ReturnsAllTokens()
    {
        var lexer = new TemplateExpressionLexer("obj.prop");
        var tokens = lexer.Lex();

        Assert.Equal(4, tokens.Count);
        Assert.Equal(TemplateTokenType.Identifier, tokens[0].Type);
        Assert.Equal("obj", tokens[0].Value);
        Assert.Equal(TemplateTokenType.Dot, tokens[1].Type);
        Assert.Equal(TemplateTokenType.Identifier, tokens[2].Type);
        Assert.Equal("prop", tokens[2].Value);
    }

    [Theory]
    [InlineData("\"unterminated")]
    [InlineData("'unterminated")]
    public void Lex_UnterminatedString_ThrowsException(string input)
    {
        var lexer = new TemplateExpressionLexer(input);

        Assert.Throws<TemplateExpressionException>(() => lexer.Lex());
    }

    [Theory]
    [InlineData("@")]
    [InlineData("#")]
    [InlineData("$")]
    public void Lex_InvalidCharacter_ThrowsException(string input)
    {
        var lexer = new TemplateExpressionLexer(input);

        Assert.Throws<TemplateExpressionException>(() => lexer.Lex());
    }

    [Fact]
    public void Lex_TokensHaveCorrectSpan()
    {
        var lexer = new TemplateExpressionLexer("42");
        var tokens = lexer.Lex();

        var token = tokens[0];
        Assert.Equal(0, token.Span.Start);
        Assert.Equal(2, token.Span.Length);
        Assert.Equal(1, token.Span.StartLine);
        Assert.Equal(1, token.Span.StartColumn);
    }
}
