using Nightmare.Parser;
using Nightmare.Parser.TemplateExpressions;

namespace Nightmare.Tests.ParserTests.TemplateExpressions;

public class TemplateStringEvaluatorTests
{
    [Fact]
    public void Evaluate_PlainText_ReturnsText()
    {
        var template = new TemplateString(new TemplateSegment[]
        {
            new TemplateTextSegment("Hello World", new TextSpan(0, 11, 1, 1, 1, 11))
        });

        var context = new EvaluationContext();
        var result = TemplateStringEvaluator.Evaluate(template, context);

        Assert.Equal("Hello World", result);
    }

    [Fact]
    public void Evaluate_SingleExpression_ReturnsEvaluatedValue()
    {
        var template = new TemplateString(new TemplateSegment[]
        {
            new TemplateExpressionSegment("5 + 3", new TextSpan(0, 5, 1, 1, 1, 5))
        });

        var context = new EvaluationContext();
        var result = TemplateStringEvaluator.Evaluate(template, context);

        Assert.Equal("8", result);
    }

    [Fact]
    public void Evaluate_MixedTextAndExpression_ReturnsCombinedResult()
    {
        var template = new TemplateString(new TemplateSegment[]
        {
            new TemplateTextSegment("Result: ", new TextSpan(0, 8, 1, 1, 1, 8)),
            new TemplateExpressionSegment("10 * 2", new TextSpan(8, 6, 1, 9, 1, 14))
        });

        var context = new EvaluationContext();
        var result = TemplateStringEvaluator.Evaluate(template, context);

        Assert.Equal("Result: 20", result);
    }

    [Fact]
    public void Evaluate_MultipleExpressions_ReturnsAllEvaluated()
    {
        var template = new TemplateString(new TemplateSegment[]
        {
            new TemplateTextSegment("a=", new TextSpan(0, 2, 1, 1, 1, 2)),
            new TemplateExpressionSegment("1 + 1", new TextSpan(2, 5, 1, 3, 1, 7)),
            new TemplateTextSegment(", b=", new TextSpan(7, 4, 1, 8, 1, 11)),
            new TemplateExpressionSegment("2 * 3", new TextSpan(11, 5, 1, 12, 1, 16))
        });

        var context = new EvaluationContext();
        var result = TemplateStringEvaluator.Evaluate(template, context);

        Assert.Equal("a=2, b=6", result);
    }

    [Fact]
    public void Evaluate_ExpressionWithVariable_ReturnsEvaluatedValue()
    {
        var template = new TemplateString(new TemplateSegment[]
        {
            new TemplateTextSegment("Hello ", new TextSpan(0, 6, 1, 1, 1, 6)),
            new TemplateExpressionSegment("name", new TextSpan(6, 4, 1, 7, 1, 10))
        });

        var context = new EvaluationContext();
        context.SetVariable("name", "John");

        var result = TemplateStringEvaluator.Evaluate(template, context);

        Assert.Equal("Hello John", result);
    }

    [Fact]
    public void Evaluate_ExpressionReturnsNull_OutputsNullString()
    {
        var template = new TemplateString(new TemplateSegment[]
        {
            new TemplateTextSegment("Value: ", new TextSpan(0, 7, 1, 1, 1, 7)),
            new TemplateExpressionSegment("null", new TextSpan(7, 4, 1, 8, 1, 11))
        });

        var context = new EvaluationContext();
        var result = TemplateStringEvaluator.Evaluate(template, context);

        Assert.Equal("Value: null", result);
    }

    [Fact]
    public void TryEvaluate_ValidTemplate_ReturnsTrue()
    {
        var template = new TemplateString(new TemplateSegment[]
        {
            new TemplateExpressionSegment("5 + 5", new TextSpan(0, 5, 1, 1, 1, 5))
        });

        var context = new EvaluationContext();
        var success = TemplateStringEvaluator.TryEvaluate(template, context, out var result, out var error);

        Assert.True(success);
        Assert.Equal("10", result);
        Assert.Null(error);
    }

    [Fact]
    public void TryEvaluate_InvalidExpression_ReturnsFalse()
    {
        var template = new TemplateString(new TemplateSegment[]
        {
            new TemplateExpressionSegment("undefinedVar", new TextSpan(0, 12, 1, 1, 1, 12))
        });

        var context = new EvaluationContext();
        var success = TemplateStringEvaluator.TryEvaluate(template, context, out var result, out var error);

        Assert.False(success);
        Assert.Null(result);
        Assert.NotNull(error);
        Assert.IsType<TemplateExpressionException>(error);
    }

    [Fact]
    public void ValidateSyntax_ValidExpressions_DoesNotThrow()
    {
        var template = new TemplateString(new TemplateSegment[]
        {
            new TemplateExpressionSegment("1 + 1", new TextSpan(0, 5, 1, 1, 1, 5)),
            new TemplateExpressionSegment("x * 2", new TextSpan(5, 5, 1, 6, 1, 10))
        });

        var exception = Record.Exception(() => TemplateStringEvaluator.ValidateSyntax(template));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidateSyntax_InvalidSyntax_ThrowsException()
    {
        var template = new TemplateString(new TemplateSegment[]
        {
            new TemplateExpressionSegment("1 +", new TextSpan(0, 3, 1, 1, 1, 3))
        });

        Assert.Throws<TemplateExpressionException>(() =>
            TemplateStringEvaluator.ValidateSyntax(template));
    }

    [Fact]
    public void GetReferencedVariables_NoVariables_ReturnsEmpty()
    {
        var template = new TemplateString(new TemplateSegment[]
        {
            new TemplateTextSegment("Hello", new TextSpan(0, 5, 1, 1, 1, 5)),
            new TemplateExpressionSegment("5 + 3", new TextSpan(5, 5, 1, 6, 1, 10))
        });

        var variables = TemplateStringEvaluator.GetReferencedVariables(template);

        Assert.Empty(variables);
    }

    [Fact]
    public void GetReferencedVariables_SingleVariable_ReturnsVariable()
    {
        var template = new TemplateString(new TemplateSegment[]
        {
            new TemplateExpressionSegment("myVar", new TextSpan(0, 5, 1, 1, 1, 5))
        });

        var variables = TemplateStringEvaluator.GetReferencedVariables(template);

        Assert.Single(variables);
        Assert.Contains("myVar", variables);
    }

    [Fact]
    public void GetReferencedVariables_MultipleVariables_ReturnsAllVariables()
    {
        var template = new TemplateString(new TemplateSegment[]
        {
            new TemplateExpressionSegment("a + b", new TextSpan(0, 5, 1, 1, 1, 5)),
            new TemplateExpressionSegment("c * 2", new TextSpan(5, 5, 1, 6, 1, 10))
        });

        var variables = TemplateStringEvaluator.GetReferencedVariables(template);

        Assert.Equal(3, variables.Count());
        Assert.Contains("a", variables);
        Assert.Contains("b", variables);
        Assert.Contains("c", variables);
    }

    [Fact]
    public void GetReferencedVariables_DuplicateVariables_ReturnsUniqueSet()
    {
        var template = new TemplateString(new TemplateSegment[]
        {
            new TemplateExpressionSegment("x + x", new TextSpan(0, 5, 1, 1, 1, 5)),
            new TemplateExpressionSegment("x * 2", new TextSpan(5, 5, 1, 6, 1, 10))
        });

        var variables = TemplateStringEvaluator.GetReferencedVariables(template);

        Assert.Single(variables);
        Assert.Contains("x", variables);
    }

    [Fact]
    public void GetReferencedVariables_MemberAccess_ReturnsRootVariable()
    {
        var template = new TemplateString(new TemplateSegment[]
        {
            new TemplateExpressionSegment("obj.prop", new TextSpan(0, 8, 1, 1, 1, 8))
        });

        var variables = TemplateStringEvaluator.GetReferencedVariables(template);

        Assert.Single(variables);
        Assert.Contains("obj", variables);
    }

    [Fact]
    public void GetReferencedVariables_ComplexExpression_ReturnsAllVariables()
    {
        var template = new TemplateString(new TemplateSegment[]
        {
            new TemplateExpressionSegment("user.name + ' ' + user.age", new TextSpan(0, 26, 1, 1, 1, 26))
        });

        var variables = TemplateStringEvaluator.GetReferencedVariables(template);

        Assert.Single(variables);
        Assert.Contains("user", variables);
    }

    [Fact]
    public void GetReferencedVariables_InvalidSyntax_ReturnsEmpty()
    {
        var template = new TemplateString(new TemplateSegment[]
        {
            new TemplateExpressionSegment("1 +", new TextSpan(0, 3, 1, 1, 1, 3))
        });

        var variables = TemplateStringEvaluator.GetReferencedVariables(template);

        Assert.Empty(variables);
    }

    [Fact]
    public void Evaluate_RealWorldExample_EvaluatesCorrectly()
    {
        // Simulate: "{{ base_url }}/users/{{ userId }}"
        var template = new TemplateString(new TemplateSegment[]
        {
            new TemplateExpressionSegment("base_url", new TextSpan(0, 8, 1, 1, 1, 8)),
            new TemplateTextSegment("/users/", new TextSpan(8, 7, 1, 9, 1, 15)),
            new TemplateExpressionSegment("userId", new TextSpan(15, 6, 1, 16, 1, 21))
        });

        var context = new EvaluationContext();
        context.SetVariable("base_url", "https://api.example.com");
        context.SetVariable("userId", 123);

        var result = TemplateStringEvaluator.Evaluate(template, context);

        Assert.Equal("https://api.example.com/users/123", result);
    }
}
