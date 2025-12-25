using Nightmare.Parser;
using Nightmare.Parser.TemplateExpressions;
using Nightmare.Parser.TemplateExpressions.Functions;
using Nightmare.Parser.TemplateExpressions.FunctionsSyntax;
using Nightmare.Parser.TemplateExpressions.FunctionsSyntax.Functions;
using Xunit;

namespace Nightmare.Tests.ParserTests.TemplateExpressions;

public class TemplateFunctionTests
{
    [Fact]
    public void RegisterFunction_WithBaseTemplateFunction_ShouldWork()
    {
        var context = new EvaluationContext();
        context.RegisterFunction(new UpperFunction());

        var result = TemplateExpressionEvaluator.Evaluate("upper('hello')", context);

        Assert.Equal("HELLO", result);
    }

    [Fact]
    public void FunctionCall_WithCorrectArguments_ShouldExecute()
    {
        var context = new EvaluationContext();
        context.RegisterFunction(new LowerFunction());

        var result = TemplateExpressionEvaluator.Evaluate("lower('WORLD')", context);

        Assert.Equal("world", result);
    }

    [Fact]
    public void FunctionCall_WithWrongArgumentType_ShouldThrowException()
    {
        var context = new EvaluationContext();
        context.RegisterFunction(new UpperFunction());

        var ex = Assert.Throws<TemplateFunctionException>(() =>
            TemplateExpressionEvaluator.Evaluate("upper(123)", context)
        );

        Assert.Contains("upper", ex.Message);
        Assert.Contains("String", ex.Message);
        Assert.Contains("input", ex.Message);
    }

    [Fact]
    public void FunctionCall_WithMissingRequiredArgument_ShouldThrowException()
    {
        var context = new EvaluationContext();
        context.RegisterFunction(new UpperFunction());

        var ex = Assert.Throws<TemplateFunctionException>(() =>
            TemplateExpressionEvaluator.Evaluate("upper()", context)
        );

        Assert.Contains("upper", ex.Message);
        Assert.Contains("input", ex.Message);
        Assert.Contains("required", ex.Message.ToLower());
    }

    [Fact]
    public void FunctionCall_WithTooManyArguments_ShouldThrowException()
    {
        var context = new EvaluationContext();
        context.RegisterFunction(new UpperFunction());

        var ex = Assert.Throws<TemplateFunctionException>(() =>
            TemplateExpressionEvaluator.Evaluate("upper('hello', 'extra')", context)
        );

        Assert.Contains("upper", ex.Message);
        Assert.Contains("2", ex.Message);
    }

    [Fact]
    public void FunctionCall_WithOptionalParameter_DefaultValue_ShouldUseDefault()
    {
        var context = new EvaluationContext();
        context.RegisterFunction(new EnvFunction());

        var result = TemplateExpressionEvaluator.Evaluate("env('MISSING_VAR')", context);

        Assert.Null(result);
    }

    [Fact]
    public void FunctionCall_WithOptionalParameter_ProvidedValue_ShouldUseProvided()
    {
        var context = new EvaluationContext();
        context.RegisterFunction(new EnvFunction());

        var result = TemplateExpressionEvaluator.Evaluate("env('MISSING_VAR', 'default')", context);

        Assert.Equal("default", result);
    }

    [Fact]
    public void VariadicFunction_WithMultipleArguments_ShouldCollectAll()
    {
        var context = new EvaluationContext();
        context.RegisterFunction(new ConcatFunction());

        var result = TemplateExpressionEvaluator.Evaluate("concat('Hello', ' ', 'World', '!')", context);

        Assert.Equal("Hello World!", result);
    }

    [Fact]
    public void VariadicFunction_WithSingleArgument_ShouldWork()
    {
        var context = new EvaluationContext();
        context.RegisterFunction(new ConcatFunction());

        var result = TemplateExpressionEvaluator.Evaluate("concat('Solo')", context);

        Assert.Equal("Solo", result);
    }

    [Fact]
    public void VariadicFunction_WithNoArguments_RequiredVariadic_ShouldThrow()
    {
        var context = new EvaluationContext();
        context.RegisterFunction(new ConcatFunction());

        var ex = Assert.Throws<TemplateFunctionException>(() =>
            TemplateExpressionEvaluator.Evaluate("concat()", context)
        );

        Assert.Contains("concat", ex.Message);
        Assert.Contains("at least one", ex.Message.ToLower());
    }

    [Fact]
    public void VariadicFunction_WithWrongTypeInVariadic_ShouldThrow()
    {
        var context = new EvaluationContext();
        context.RegisterFunction(new ConcatFunction());

        var ex = Assert.Throws<TemplateFunctionException>(() =>
            TemplateExpressionEvaluator.Evaluate("concat('Hello', 123, 'World')", context)
        );

        Assert.Contains("concat", ex.Message);
        Assert.Contains("String", ex.Message);
    }

    [Fact]
    public void StringFunctions_AllBuiltIn_ShouldWork()
    {
        var context = new EvaluationContext();
        context.RegisterFunction(new UpperFunction());
        context.RegisterFunction(new LowerFunction());

        var upper = TemplateExpressionEvaluator.Evaluate("upper('test')", context);
        var lower = TemplateExpressionEvaluator.Evaluate("lower('TEST')", context);

        Assert.Equal("TEST", upper);
        Assert.Equal("test", lower);
    }

    [Fact]
    public void UtilityFunctions_Hash_ShouldWork()
    {
        var context = new EvaluationContext();
        context.RegisterFunction(new HashFunction());

        var result = TemplateExpressionEvaluator.Evaluate("hash('test')", context);

        Assert.NotNull(result);
        Assert.IsType<string>(result);
        Assert.Equal(64, ((string)result!).Length); // SHA256 produces 64 hex chars
    }

    [Fact]
    public void UtilityFunctions_Uuid_ShouldGenerateValidGuid()
    {
        var context = new EvaluationContext();
        context.RegisterFunction(new UuidFunction());

        var result = TemplateExpressionEvaluator.Evaluate("uuid()", context);

        Assert.NotNull(result);
        Assert.IsType<string>(result);
        Assert.True(Guid.TryParse((string)result!, out _));
    }

    [Fact]
    public void UtilityFunctions_IfElse_WithTrue_ShouldReturnThen()
    {
        var context = new EvaluationContext();
        context.RegisterFunction(new IfElseFunction());

        var result = TemplateExpressionEvaluator.Evaluate("ifElse(true, 'yes', 'no')", context);

        Assert.Equal("yes", result);
    }

    [Fact]
    public void UtilityFunctions_IfElse_WithFalse_ShouldReturnElse()
    {
        var context = new EvaluationContext();
        context.RegisterFunction(new IfElseFunction());

        var result = TemplateExpressionEvaluator.Evaluate("ifElse(false, 'yes', 'no')", context);

        Assert.Equal("no", result);
    }

    [Fact]
    public void DateFunctions_Timestamp_ShouldReturnNumber()
    {
        var context = new EvaluationContext();
        context.RegisterFunction(new TimeStampFunction());

        var result = TemplateExpressionEvaluator.Evaluate("timestamp()", context);

        Assert.NotNull(result);
        Assert.IsType<long>(result);
        Assert.True((long)result! > 0);
    }

    [Fact]
    public void DateFunctions_Date_ShouldReturnIsoString()
    {
        var context = new EvaluationContext();
        context.RegisterFunction(new DateFunction());

        var result = TemplateExpressionEvaluator.Evaluate("date()", context);

        Assert.NotNull(result);
        Assert.IsType<string>(result);
        // Should be parseable as DateTime
        Assert.True(DateTime.TryParse((string)result!, out _));
    }

    [Fact]
    public void FunctionError_ShouldIncludeFunctionNameAndPosition()
    {
        var context = new EvaluationContext();
        context.RegisterFunction(new UpperFunction());

        var ex = Assert.Throws<TemplateFunctionException>(() =>
            TemplateExpressionEvaluator.Evaluate("upper(123)", context)
        );

        Assert.Contains("upper", ex.Message);
        Assert.True(ex.Line > 0);
        Assert.True(ex.Column > 0);
    }

    [Fact]
    public void NestedFunctionCalls_ShouldWork()
    {
        var context = new EvaluationContext();
        context.RegisterFunction(new UpperFunction());
        context.RegisterFunction(new ConcatFunction());

        var result = TemplateExpressionEvaluator.Evaluate("upper(concat('hello', ' ', 'world'))", context);

        Assert.Equal("HELLO WORLD", result);
    }

    [Fact]
    public void FunctionWithMultipleTypeOptions_ShouldAcceptAny()
    {
        var context = new EvaluationContext();
        
        // Hash function accepts String or Number
        context.RegisterFunction(new HashFunction());

        var stringResult = TemplateExpressionEvaluator.Evaluate("hash('test')", context);
        var numberResult = TemplateExpressionEvaluator.Evaluate("hash(123)", context);

        Assert.NotNull(stringResult);
        Assert.NotNull(numberResult);
        Assert.NotEqual(stringResult, numberResult);
    }
}
