using Nightmare.Parser;
using Nightmare.Parser.TemplateExpressions.FunctionsSyntax;

namespace Nightmare.Tests.ParserTests.TemplateExpressions;

// Test helper functions for unit tests

public class TestFunction : TemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return
        [
            new FunctionParameter(
                "input",
                [
                    FunctionParamValueType.String,
                    FunctionParamValueType.Number,
                    FunctionParamValueType.Boolean,
                    FunctionParamValueType.Array,
                    FunctionParamValueType.Object,
                    FunctionParamValueType.Null
                ]
            )
        ];
    }

    public override string GetName()
    {
        return "test";
    }

    protected override object? Execute(object?[] args, TextSpan span)
    {
        return $"test: {args[0]}";
    }
}

public class TestNullFunction : TemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return [];
    }

    public override string GetName()
    {
        return "test";
    }

    protected override object? Execute(object?[] args, TextSpan span)
    {
        return null;
    }
}

public class AddFunction : TemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return
        [
            new FunctionParameter("a", [FunctionParamValueType.Number]),
            new FunctionParameter("b", [FunctionParamValueType.Number])
        ];
    }

    public override string GetName()
    {
        return "add";
    }

    protected override object? Execute(object?[] args, TextSpan span)
    {
        return Convert.ToDouble(args[0]) + Convert.ToDouble(args[1]);
    }
}

public class MaxFunction : TemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return
        [
            new FunctionParameter(
                "numbers",
                [FunctionParamValueType.Number],
                true,
                Variadic: true
            )
        ];
    }

    public override string GetName()
    {
        return "max";
    }

    protected override object? Execute(object?[] args, TextSpan span)
    {
        var numbers = (object?[])args[0]!;
        return numbers.Max(n => Convert.ToDouble(n));
    }
}

public class SideEffectFunction : TemplateFunction
{
    private readonly Action _action;

    public SideEffectFunction(Action action)
    {
        _action = action;
    }

    protected override FunctionParameter[] ListArgs()
    {
        return [];
    }

    public override string GetName()
    {
        return "sideEffect";
    }

    protected override object? Execute(object?[] args, TextSpan span)
    {
        _action();
        return null;
    }
}

public class FailingFunction : TemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return [];
    }

    public override string GetName()
    {
        return "failing";
    }

    protected override object? Execute(object?[] args, TextSpan span)
    {
        throw Error("Test error", span);
    }
}

public class DivideFunction : TemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return
        [
            new FunctionParameter("a", [FunctionParamValueType.Number]),
            new FunctionParameter("b", [FunctionParamValueType.Number])
        ];
    }

    public override string GetName()
    {
        return "divide";
    }

    protected override object? Execute(object?[] args, TextSpan span)
    {
        var a = Convert.ToDouble(args[0]);
        var b = Convert.ToDouble(args[1]);
        if (b == 0)
            throw Error("Division by zero", span);
        return a / b;
    }
}

public class OuterFunction : TemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return
        [
            new FunctionParameter(
                "value",
                [
                    FunctionParamValueType.String,
                    FunctionParamValueType.Number,
                    FunctionParamValueType.Boolean,
                    FunctionParamValueType.Array,
                    FunctionParamValueType.Object,
                    FunctionParamValueType.Null
                ]
            )
        ];
    }

    public override string GetName()
    {
        return "outer";
    }

    protected override object? Execute(object?[] args, TextSpan span)
    {
        return args[0];
    }
}

public class InnerFunction : TemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return [];
    }

    public override string GetName()
    {
        return "inner";
    }

    protected override object? Execute(object?[] args, TextSpan span)
    {
        return "inner result";
    }
}

public class ValidatorFunction : TemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return
        [
            new FunctionParameter(
                "value",
                [
                    FunctionParamValueType.String,
                    FunctionParamValueType.Number,
                    FunctionParamValueType.Boolean,
                    FunctionParamValueType.Array,
                    FunctionParamValueType.Object,
                    FunctionParamValueType.Null
                ]
            )
        ];
    }

    public override string GetName()
    {
        return "validator";
    }

    protected override object? Execute(object?[] args, TextSpan span)
    {
        throw Error("validation failed", span);
    }
}

public class PrintFunction : TemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return
        [
            new FunctionParameter(
                "value",
                [
                    FunctionParamValueType.String,
                    FunctionParamValueType.Number,
                    FunctionParamValueType.Boolean,
                    FunctionParamValueType.Array,
                    FunctionParamValueType.Object,
                    FunctionParamValueType.Null
                ]
            )
        ];
    }

    public override string GetName()
    {
        return "print";
    }

    protected override object? Execute(object?[] args, TextSpan span)
    {
        return args[0]?.ToString();
    }
}

public class TestUuidFunction : TemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return [];
    }

    public override string GetName()
    {
        return "uuid";
    }

    protected override object? Execute(object?[] args, TextSpan span)
    {
        return "test-uuid-123";
    }
}

public class TestTimestampFunction : TemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return [];
    }

    public override string GetName()
    {
        return "timestamp";
    }

    protected override object? Execute(object?[] args, TextSpan span)
    {
        return 1234567890;
    }
}

public class TestUpperFunction : TemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return
        [
            new FunctionParameter(
                "input",
                [
                    FunctionParamValueType.String,
                    FunctionParamValueType.Number,
                    FunctionParamValueType.Boolean,
                    FunctionParamValueType.Array,
                    FunctionParamValueType.Object,
                    FunctionParamValueType.Null
                ]
            )
        ];
    }

    public override string GetName()
    {
        return "upper";
    }

    protected override object? Execute(object?[] args, TextSpan span)
    {
        return args[0]?.ToString()?.ToUpper();
    }
}