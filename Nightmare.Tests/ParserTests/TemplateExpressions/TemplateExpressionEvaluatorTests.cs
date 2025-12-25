using Nightmare.Parser.TemplateExpressions;

namespace Nightmare.Tests.ParserTests.TemplateExpressions;

public class TemplateExpressionEvaluatorTests
{
    [Theory]
    [InlineData("42", 42.0)]
    [InlineData("3.14", 3.14)]
    [InlineData("-10", -10.0)]
    public void Evaluate_NumberLiteral_ReturnsNumber(string input, double expected)
    {
        var context = new EvaluationContext();
        var result = TemplateExpressionEvaluator.Evaluate(input, context);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("\"hello\"", "hello")]
    [InlineData("'world'", "world")]
    public void Evaluate_StringLiteral_ReturnsString(string input, string expected)
    {
        var context = new EvaluationContext();
        var result = TemplateExpressionEvaluator.Evaluate(input, context);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    public void Evaluate_BooleanLiteral_ReturnsBoolean(string input, bool expected)
    {
        var context = new EvaluationContext();
        var result = TemplateExpressionEvaluator.Evaluate(input, context);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Evaluate_NullLiteral_ReturnsNull()
    {
        var context = new EvaluationContext();
        var result = TemplateExpressionEvaluator.Evaluate("null", context);

        Assert.Null(result);
    }

    [Fact]
    public void Evaluate_Identifier_ReturnsVariableValue()
    {
        var context = new EvaluationContext();
        context.SetVariable("myVar", 42);

        var result = TemplateExpressionEvaluator.Evaluate("myVar", context);

        Assert.Equal(42, result);
    }

    [Fact]
    public void Evaluate_UndefinedVariable_ThrowsException()
    {
        var context = new EvaluationContext();

        Assert.Throws<TemplateExpressionException>(() =>
            TemplateExpressionEvaluator.Evaluate("undefined", context));
    }

    [Theory]
    [InlineData("2 + 3", 5.0)]
    [InlineData("10 - 4", 6.0)]
    [InlineData("3 * 4", 12.0)]
    [InlineData("20 / 4", 5.0)]
    [InlineData("10 % 3", 1.0)]
    public void Evaluate_ArithmeticOperations_ReturnsCorrectResult(string input, double expected)
    {
        var context = new EvaluationContext();
        var result = TemplateExpressionEvaluator.Evaluate(input, context);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Evaluate_DivisionByZero_ThrowsException()
    {
        var context = new EvaluationContext();

        Assert.Throws<TemplateExpressionException>(() =>
            TemplateExpressionEvaluator.Evaluate("10 / 0", context));
    }

    [Theory]
    [InlineData("5 == 5", true)]
    [InlineData("5 == 3", false)]
    [InlineData("5 != 3", true)]
    [InlineData("5 != 5", false)]
    [InlineData("5 > 3", true)]
    [InlineData("5 > 10", false)]
    [InlineData("5 >= 5", true)]
    [InlineData("5 < 10", true)]
    [InlineData("5 <= 5", true)]
    public void Evaluate_ComparisonOperations_ReturnsCorrectResult(string input, bool expected)
    {
        var context = new EvaluationContext();
        var result = TemplateExpressionEvaluator.Evaluate(input, context);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("true && true", true)]
    [InlineData("true && false", false)]
    [InlineData("false && true", false)]
    [InlineData("true || false", true)]
    [InlineData("false || false", false)]
    public void Evaluate_LogicalOperations_ReturnsCorrectResult(string input, bool expected)
    {
        var context = new EvaluationContext();
        var result = TemplateExpressionEvaluator.Evaluate(input, context);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("!true", false)]
    [InlineData("!false", true)]
    [InlineData("-10", -10.0)]
    [InlineData("-(5 + 3)", -8.0)]
    public void Evaluate_UnaryOperations_ReturnsCorrectResult(string input, object expected)
    {
        var context = new EvaluationContext();
        var result = TemplateExpressionEvaluator.Evaluate(input, context);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("'hello' + 'world'", "helloworld")]
    [InlineData("'Count: ' + 5", "Count: 5")]
    [InlineData("5 + ' items'", "5 items")]
    public void Evaluate_StringConcatenation_ReturnsCorrectResult(string input, string expected)
    {
        var context = new EvaluationContext();
        var result = TemplateExpressionEvaluator.Evaluate(input, context);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Evaluate_MemberAccess_ReturnsPropertyValue()
    {
        var context = new EvaluationContext();
        var obj = new Dictionary<string, object?> { ["prop"] = "value" };
        context.SetVariable("obj", obj);

        var result = TemplateExpressionEvaluator.Evaluate("obj.prop", context);

        Assert.Equal("value", result);
    }

    [Fact]
    public void Evaluate_NestedMemberAccess_ReturnsNestedPropertyValue()
    {
        var context = new EvaluationContext();
        var obj = new Dictionary<string, object?>
        {
            ["nested"] = new Dictionary<string, object?> { ["prop"] = "deep value" }
        };
        context.SetVariable("obj", obj);

        var result = TemplateExpressionEvaluator.Evaluate("obj.nested.prop", context);

        Assert.Equal("deep value", result);
    }

    [Fact]
    public void Evaluate_IndexAccess_ReturnsArrayElement()
    {
        var context = new EvaluationContext();
        var arr = new List<object?> { "first", "second", "third" };
        context.SetVariable("arr", arr);

        var result = TemplateExpressionEvaluator.Evaluate("arr[1]", context);

        Assert.Equal("second", result);
    }

    [Fact]
    public void Evaluate_IndexAccessOutOfRange_ThrowsException()
    {
        var context = new EvaluationContext();
        var arr = new List<object?> { "first" };
        context.SetVariable("arr", arr);

        Assert.Throws<TemplateExpressionException>(() =>
            TemplateExpressionEvaluator.Evaluate("arr[10]", context));
    }

    [Fact]
    public void Evaluate_FunctionCall_CallsFunction()
    {
        var context = new EvaluationContext();
        var callCount = 0;
        context.RegisterFunction(new SideEffectFunction(() => callCount++));
        context.RegisterFunction(new TestFunction());

        var result = TemplateExpressionEvaluator.Evaluate("sideEffect()", context);

        Assert.Null(result);
        Assert.Equal(1, callCount);
    }

    [Fact]
    public void Evaluate_FunctionCallWithArguments_PassesArguments()
    {
        var context = new EvaluationContext();
        context.RegisterFunction(new AddFunction());

        var result = TemplateExpressionEvaluator.Evaluate("add(5, 3)", context);

        Assert.Equal(8.0, result);
    }

    [Fact]
    public void Evaluate_UndefinedFunction_ThrowsException()
    {
        var context = new EvaluationContext();

        Assert.Throws<TemplateExpressionException>(() =>
            TemplateExpressionEvaluator.Evaluate("undefined()", context));
    }

    [Theory]
    [InlineData("true ? 'yes' : 'no'", "yes")]
    [InlineData("false ? 'yes' : 'no'", "no")]
    [InlineData("5 > 3 ? 10 : 20", 10.0)]
    [InlineData("5 < 3 ? 10 : 20", 20.0)]
    public void Evaluate_ConditionalExpression_ReturnsCorrectBranch(string input, object expected)
    {
        var context = new EvaluationContext();
        var result = TemplateExpressionEvaluator.Evaluate(input, context);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Evaluate_NestedConditional_ReturnsCorrectValue()
    {
        var context = new EvaluationContext();
        context.SetVariable("score", 85);

        var result = TemplateExpressionEvaluator.Evaluate(
            "score > 90 ? 'A' : (score > 80 ? 'B' : 'C')",
            context
        );

        Assert.Equal("B", result);
    }

    [Fact]
    public void Evaluate_ComplexExpression_ReturnsCorrectValue()
    {
        var context = new EvaluationContext();
        context.SetVariable("a", 10);
        context.SetVariable("b", 5);
        context.RegisterFunction(new MaxFunction());

        var result = TemplateExpressionEvaluator.Evaluate(
            "(a + b) * 2 > 25 ? max(a, b) : 0",
            context
        );

        Assert.Equal(10.0, result);
    }

    [Fact]
    public void Evaluate_ShortCircuitAnd_DoesNotEvaluateSecondOperand()
    {
        var context = new EvaluationContext();
        var called = false;
        context.RegisterFunction(new SideEffectFunction(() => called = true));

        TemplateExpressionEvaluator.Evaluate("false && sideEffect()", context);

        Assert.False(called);
    }

    [Fact]
    public void Evaluate_ShortCircuitOr_DoesNotEvaluateSecondOperand()
    {
        var context = new EvaluationContext();
        var called = false;
        context.RegisterFunction(new SideEffectFunction(() => called = true));

        TemplateExpressionEvaluator.Evaluate("true || sideEffect()", context);

        Assert.False(called);
    }

    [Fact]
    public void Evaluate_OperatorPrecedence_EvaluatesCorrectly()
    {
        var context = new EvaluationContext();

        var result = TemplateExpressionEvaluator.Evaluate("2 + 3 * 4", context);

        Assert.Equal(14.0, result);
    }

    [Fact]
    public void Evaluate_ParenthesesOverridePrecedence_EvaluatesCorrectly()
    {
        var context = new EvaluationContext();

        var result = TemplateExpressionEvaluator.Evaluate("(2 + 3) * 4", context);

        Assert.Equal(20.0, result);
    }

    [Fact]
    public void EvaluationContext_HasVariable_ReturnsTrueForExistingVariable()
    {
        var context = new EvaluationContext();
        context.SetVariable("test", 42);

        Assert.True(context.HasVariable("test"));
        Assert.False(context.HasVariable("nonexistent"));
    }

    [Fact]
    public void EvaluationContext_HasFunction_ReturnsTrueForRegisteredFunction()
    {
        var context = new EvaluationContext();
        context.RegisterFunction(new TestNullFunction());

        Assert.True(context.HasFunction("test"));
        Assert.False(context.HasFunction("nonexistent"));
    }
}
