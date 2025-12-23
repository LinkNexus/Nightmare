using Nightmare.Parser.TemplateExpressions;

namespace Nightmare.Tests.ParserTests.TemplateExpressions;

public class TemplateExpressionParserTests
{
    [Theory]
    [InlineData("42", 42.0)]
    [InlineData("3.14", 3.14)]
    [InlineData("-10", -10.0)]
    public void Parse_NumberLiteral_ReturnsNumberExpression(string input, double expectedValue)
    {
        var expr = TemplateExpressionParser.Parse(input);

        var numberExpr = Assert.IsType<NumberLiteralExpression>(expr);
        Assert.Equal(expectedValue, numberExpr.Value);
    }

    [Theory]
    [InlineData("\"hello\"", "hello")]
    [InlineData("'world'", "world")]
    public void Parse_StringLiteral_ReturnsStringExpression(string input, string expectedValue)
    {
        var expr = TemplateExpressionParser.Parse(input);

        var stringExpr = Assert.IsType<StringLiteralExpression>(expr);
        Assert.Equal(expectedValue, stringExpr.Value);
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    public void Parse_BooleanLiteral_ReturnsBooleanExpression(string input, bool expectedValue)
    {
        var expr = TemplateExpressionParser.Parse(input);

        var boolExpr = Assert.IsType<BooleanLiteralExpression>(expr);
        Assert.Equal(expectedValue, boolExpr.Value);
    }

    [Fact]
    public void Parse_NullLiteral_ReturnsNullExpression()
    {
        var expr = TemplateExpressionParser.Parse("null");

        Assert.IsType<NullLiteralExpression>(expr);
    }

    [Fact]
    public void Parse_Identifier_ReturnsIdentifierExpression()
    {
        var expr = TemplateExpressionParser.Parse("myVar");

        var identExpr = Assert.IsType<IdentifierExpression>(expr);
        Assert.Equal("myVar", identExpr.Name);
    }

    [Theory]
    [InlineData("1 + 2", BinaryOperator.Add)]
    [InlineData("5 - 3", BinaryOperator.Subtract)]
    [InlineData("4 * 3", BinaryOperator.Multiply)]
    [InlineData("10 / 2", BinaryOperator.Divide)]
    [InlineData("10 % 3", BinaryOperator.Modulo)]
    public void Parse_ArithmeticBinaryExpression_ReturnsBinaryExpression(string input, BinaryOperator expectedOp)
    {
        var expr = TemplateExpressionParser.Parse(input);

        var binaryExpr = Assert.IsType<BinaryExpression>(expr);
        Assert.Equal(expectedOp, binaryExpr.Operator);
        Assert.IsType<NumberLiteralExpression>(binaryExpr.Left);
        Assert.IsType<NumberLiteralExpression>(binaryExpr.Right);
    }

    [Theory]
    [InlineData("1 == 2", BinaryOperator.Equal)]
    [InlineData("1 != 2", BinaryOperator.NotEqual)]
    [InlineData("1 < 2", BinaryOperator.LessThan)]
    [InlineData("1 <= 2", BinaryOperator.LessOrEqual)]
    [InlineData("1 > 2", BinaryOperator.GreaterThan)]
    [InlineData("1 >= 2", BinaryOperator.GreaterOrEqual)]
    public void Parse_ComparisonBinaryExpression_ReturnsBinaryExpression(string input, BinaryOperator expectedOp)
    {
        var expr = TemplateExpressionParser.Parse(input);

        var binaryExpr = Assert.IsType<BinaryExpression>(expr);
        Assert.Equal(expectedOp, binaryExpr.Operator);
    }

    [Theory]
    [InlineData("true && false", BinaryOperator.And)]
    [InlineData("true || false", BinaryOperator.Or)]
    public void Parse_LogicalBinaryExpression_ReturnsBinaryExpression(string input, BinaryOperator expectedOp)
    {
        var expr = TemplateExpressionParser.Parse(input);

        var binaryExpr = Assert.IsType<BinaryExpression>(expr);
        Assert.Equal(expectedOp, binaryExpr.Operator);
    }

    [Fact]
    public void Parse_OperatorPrecedence_MultiplicationBeforeAddition()
    {
        var expr = TemplateExpressionParser.Parse("1 + 2 * 3");

        var addExpr = Assert.IsType<BinaryExpression>(expr);
        Assert.Equal(BinaryOperator.Add, addExpr.Operator);
        Assert.IsType<NumberLiteralExpression>(addExpr.Left);

        var mulExpr = Assert.IsType<BinaryExpression>(addExpr.Right);
        Assert.Equal(BinaryOperator.Multiply, mulExpr.Operator);
    }

    [Fact]
    public void Parse_OperatorPrecedence_ParenthesesOverride()
    {
        var expr = TemplateExpressionParser.Parse("(1 + 2) * 3");

        var mulExpr = Assert.IsType<BinaryExpression>(expr);
        Assert.Equal(BinaryOperator.Multiply, mulExpr.Operator);

        var addExpr = Assert.IsType<BinaryExpression>(mulExpr.Left);
        Assert.Equal(BinaryOperator.Add, addExpr.Operator);
    }

    [Fact]
    public void Parse_UnaryNot_ReturnsUnaryExpression()
    {
        var expr = TemplateExpressionParser.Parse("!true");

        var unaryExpr = Assert.IsType<UnaryExpression>(expr);
        Assert.Equal(UnaryOperator.Not, unaryExpr.Operator);
        Assert.IsType<BooleanLiteralExpression>(unaryExpr.Operand);
    }

    [Fact]
    public void Parse_UnaryNegate_ReturnsUnaryExpression()
    {
        var expr = TemplateExpressionParser.Parse("-(5)");

        var unaryExpr = Assert.IsType<UnaryExpression>(expr);
        Assert.Equal(UnaryOperator.Negate, unaryExpr.Operator);
        Assert.IsType<NumberLiteralExpression>(unaryExpr.Operand);
    }

    [Fact]
    public void Parse_MemberAccess_ReturnsMemberAccessExpression()
    {
        var expr = TemplateExpressionParser.Parse("obj.prop");

        var memberExpr = Assert.IsType<MemberAccessExpression>(expr);
        Assert.Equal("prop", memberExpr.MemberName);

        var identExpr = Assert.IsType<IdentifierExpression>(memberExpr.Target);
        Assert.Equal("obj", identExpr.Name);
    }

    [Fact]
    public void Parse_ChainedMemberAccess_ReturnsNestedMemberAccessExpression()
    {
        var expr = TemplateExpressionParser.Parse("obj.prop.nested");

        var outerMember = Assert.IsType<MemberAccessExpression>(expr);
        Assert.Equal("nested", outerMember.MemberName);

        var innerMember = Assert.IsType<MemberAccessExpression>(outerMember.Target);
        Assert.Equal("prop", innerMember.MemberName);

        var identExpr = Assert.IsType<IdentifierExpression>(innerMember.Target);
        Assert.Equal("obj", identExpr.Name);
    }

    [Fact]
    public void Parse_IndexAccess_ReturnsIndexAccessExpression()
    {
        var expr = TemplateExpressionParser.Parse("arr[0]");

        var indexExpr = Assert.IsType<IndexAccessExpression>(expr);

        var identExpr = Assert.IsType<IdentifierExpression>(indexExpr.Target);
        Assert.Equal("arr", identExpr.Name);

        var numberExpr = Assert.IsType<NumberLiteralExpression>(indexExpr.Index);
        Assert.Equal(0, numberExpr.Value);
    }

    [Fact]
    public void Parse_FunctionCall_ReturnsCallExpression()
    {
        var expr = TemplateExpressionParser.Parse("func()");

        var callExpr = Assert.IsType<CallExpression>(expr);
        Assert.Empty(callExpr.Arguments);

        var identExpr = Assert.IsType<IdentifierExpression>(callExpr.Callee);
        Assert.Equal("func", identExpr.Name);
    }

    [Fact]
    public void Parse_FunctionCallWithArguments_ReturnsCallExpressionWithArguments()
    {
        var expr = TemplateExpressionParser.Parse("func(1, 2, 3)");

        var callExpr = Assert.IsType<CallExpression>(expr);
        Assert.Equal(3, callExpr.Arguments.Count);
        Assert.All(callExpr.Arguments, arg => Assert.IsType<NumberLiteralExpression>(arg));
    }

    [Fact]
    public void Parse_ConditionalExpression_ReturnsConditionalExpression()
    {
        var expr = TemplateExpressionParser.Parse("true ? 1 : 2");

        var condExpr = Assert.IsType<ConditionalExpression>(expr);
        Assert.IsType<BooleanLiteralExpression>(condExpr.Condition);
        Assert.IsType<NumberLiteralExpression>(condExpr.ThenExpression);
        Assert.IsType<NumberLiteralExpression>(condExpr.ElseExpression);
    }

    [Fact]
    public void Parse_ComplexExpression_ParsesCorrectly()
    {
        var expr = TemplateExpressionParser.Parse("(a + b) * func(x) > 10 ? 'yes' : 'no'");

        Assert.IsType<ConditionalExpression>(expr);
    }

    [Theory]
    [InlineData("1 +")]
    [InlineData("* 2")]
    [InlineData("func(")]
    [InlineData("obj.")]
    public void Parse_IncompleteExpression_ThrowsException(string input)
    {
        Assert.Throws<TemplateExpressionException>(() => TemplateExpressionParser.Parse(input));
    }

    [Fact]
    public void Parse_UnexpectedToken_ThrowsException()
    {
        Assert.Throws<TemplateExpressionException>(() => TemplateExpressionParser.Parse("1 2"));
    }

    [Fact]
    public void Parse_StringConcatenation_ParsesAsAddition()
    {
        var expr = TemplateExpressionParser.Parse("'hello' + 'world'");

        var binaryExpr = Assert.IsType<BinaryExpression>(expr);
        Assert.Equal(BinaryOperator.Add, binaryExpr.Operator);
        Assert.IsType<StringLiteralExpression>(binaryExpr.Left);
        Assert.IsType<StringLiteralExpression>(binaryExpr.Right);
    }
}
