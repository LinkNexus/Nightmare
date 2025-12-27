using Nightmare.Parser;
using Nightmare.Parser.TemplateExpressions;
using Nightmare.Parser.TemplateExpressions.FunctionsSyntax;
using Xunit;

namespace Nightmare.Tests.ParserTests.TemplateExpressions;

public class TemplateExpressionErrorPositionTests
{
    [Fact]
    public void UndefinedVariable_ShouldHaveCorrectPosition()
    {
        var context = new EvaluationContext();
        var expression = "undefinedVar";

        var ex = Assert.Throws<TemplateExpressionException>(() =>
            TemplateExpressionEvaluator.Evaluate(expression, context)
        );

        Assert.Contains("undefinedVar", ex.Message);
        Assert.True(ex.Line > 0);
        Assert.True(ex.Column > 0);
    }

    [Fact]
    public void UndefinedFunction_ShouldHaveCorrectPosition()
    {
        var context = new EvaluationContext();
        var expression = "undefinedFunc()";

        var ex = Assert.Throws<TemplateExpressionException>(() =>
            TemplateExpressionEvaluator.Evaluate(expression, context)
        );

        Assert.Contains("undefinedFunc", ex.Message);
        Assert.Contains("not found", ex.Message);
        Assert.True(ex.Line > 0);
        Assert.True(ex.Column > 0);
    }

    [Fact]
    public void FunctionThrowingError_ShouldHaveCorrectPosition()
    {
        var context = new EvaluationContext();
        context.RegisterFunction(new FailingFunction());

        var expression = "failing()";

        var ex = Assert.Throws<TemplateFunctionException>(() =>
            TemplateExpressionEvaluator.Evaluate(expression, context)
        );

        Assert.Contains("failing", ex.Message);
        Assert.Contains("Test error", ex.Message);
        Assert.True(ex.Line > 0);
        Assert.True(ex.Column > 0);
    }

    [Fact]
    public void FunctionThrowingError_WithArguments_ShouldHaveCorrectPosition()
    {
        var context = new EvaluationContext();
        context.RegisterFunction(new DivideFunction());

        var expression = "divide(10, 0)";

        var ex = Assert.Throws<TemplateFunctionException>(() =>
            TemplateExpressionEvaluator.Evaluate(expression, context)
        );

        Assert.Contains("divide", ex.Message);
        Assert.Contains("Division by zero", ex.Message);
        Assert.True(ex.Line > 0);
        Assert.True(ex.Column > 0);
    }

    [Fact]
    public void NestedFunctionError_ShouldHaveCorrectPosition()
    {
        var context = new EvaluationContext();
        context.RegisterFunction(new FailingFunction());
        context.RegisterFunction(new InnerFunction());

        var expression = "failing()";

        var ex = Assert.Throws<TemplateFunctionException>(() =>
            TemplateExpressionEvaluator.Evaluate(expression, context)
        );

        Assert.Contains("failing", ex.Message);
        Assert.Contains("Test error", ex.Message);
        Assert.True(ex.Line > 0);
        Assert.True(ex.Column > 0);
    }

    [Fact]
    public void UndefinedVariableInMemberAccess_ShouldHaveCorrectPosition()
    {
        var context = new EvaluationContext();
        var expression = "user.name";

        var ex = Assert.Throws<TemplateExpressionException>(() =>
            TemplateExpressionEvaluator.Evaluate(expression, context)
        );

        Assert.Contains("user", ex.Message);
        Assert.Contains("not found", ex.Message);
        Assert.True(ex.Line > 0);
        Assert.True(ex.Column > 0);
    }

    [Fact]
    public void ErrorInComplexExpression_ShouldHaveCorrectPosition()
    {
        var context = new EvaluationContext();
        context.SetVariable("a", 10);
        context.SetVariable("b", 20);
        // 'c' is undefined

        var expression = "a + b + c";

        var ex = Assert.Throws<TemplateExpressionException>(() =>
            TemplateExpressionEvaluator.Evaluate(expression, context)
        );

        Assert.Contains("c", ex.Message);
        Assert.Contains("not found", ex.Message);
        Assert.True(ex.Line > 0);
        Assert.True(ex.Column > 0);
    }

    [Fact]
    public void ErrorInConditionalExpression_ShouldHaveCorrectPosition()
    {
        var context = new EvaluationContext();
        context.SetVariable("isTrue", true);
        // 'value' is undefined

        var expression = "isTrue ? value : 0";

        var ex = Assert.Throws<TemplateExpressionException>(() =>
            TemplateExpressionEvaluator.Evaluate(expression, context)
        );

        Assert.Contains("value", ex.Message);
        Assert.Contains("not found", ex.Message);
        Assert.True(ex.Line > 0);
        Assert.True(ex.Column > 0);
    }

    [Fact]
    public void MultilineExpression_ErrorShouldHaveCorrectLineNumber()
    {
        var context = new EvaluationContext();
        context.SetVariable("x", 10);

        // Multi-line expression with error on line 3
        var expression = @"
            x + 
            undefinedVar";

        var ex = Assert.Throws<TemplateExpressionException>(() =>
            TemplateExpressionEvaluator.Evaluate(expression, context)
        );

        Assert.Contains("undefinedVar", ex.Message);
        // Line should be 3 (the line with undefinedVar)
        Assert.True(ex.Line >= 3);
    }

    [Fact]
    public void FunctionWithTemplateExpressionException_ShouldPreserveOriginalError()
    {
        var context = new EvaluationContext();
        context.RegisterFunction(new ValidatorFunction());

        var expression = "validator('test')";

        var ex = Assert.Throws<TemplateFunctionException>(() =>
            TemplateExpressionEvaluator.Evaluate(expression, context)
        );

        // Should have the error from the function
        Assert.Contains("validation failed", ex.Message);
        Assert.True(ex.Line > 0);
        Assert.True(ex.Column > 0);
    }

    [Fact]
    public void ErrorInFunctionArgument_ShouldHaveCorrectPosition()
    {
        var context = new EvaluationContext();
        context.RegisterFunction(new PrintFunction());
        // 'msg' is undefined

        var expression = "print(msg)";

        var ex = Assert.Throws<TemplateExpressionException>(() =>
            TemplateExpressionEvaluator.Evaluate(expression, context)
        );

        Assert.Contains("msg", ex.Message);
        Assert.Contains("not found", ex.Message);
        Assert.True(ex.Line > 0);
        Assert.True(ex.Column > 0);
    }

    [Fact]
    public void ErrorPositionInTemplateString()
    {
        var context = new EvaluationContext();
        context.SetVariable("base_url", "https://example.com");
        // userId is undefined

        var template = new TemplateString(new TemplateSegment[]
        {
            new TemplateTextSegment("URL: ", new TextSpan(0, 5, 1, 1, 1, 5)),
            new TemplateExpressionSegment(
                "base_url + '/users/' + userId",
                new TextSpan(5, 32, 1, 6, 1, 38)
            )
        });

        var ex = Assert.Throws<TemplateExpressionException>(() =>
            TemplateStringEvaluator.Evaluate(template, context)
        );

        Assert.Contains("userId", ex.Message);
        Assert.Contains("not found", ex.Message);
        // Should have position from the expression segment
        Assert.Equal(1, ex.Line);
        Assert.True(ex.Column > 0);
    }
}