using Nightmare.Parser;
using Nightmare.Parser.TemplateExpressions.Functions;
using Nightmare.Parser.TemplateExpressions.FunctionsSyntax;

namespace Nightmare.Tests.ParserTests.TemplateExpressions;

// Test helper functions for unit tests

public class TestFunction : BaseTemplateFunction
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

    public override string GetName() => "test";

    protected override object? Execute(object?[] args, TextSpan span)
    {
        return $"test: {args[0]}";
    }
}

public class TestNullFunction : BaseTemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return [];
    }

    public override string GetName() => "test";

    protected override object? Execute(object?[] args, TextSpan span)
    {
        return null;
    }
}

public class AddFunction : BaseTemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return
        [
            new FunctionParameter("a", [FunctionParamValueType.Number]),
            new FunctionParameter("b", [FunctionParamValueType.Number])
        ];
    }

    public override string GetName() => "add";

    protected override object? Execute(object?[] args, TextSpan span)
    {
        return Convert.ToDouble(args[0]) + Convert.ToDouble(args[1]);
    }
}

public class MaxFunction : BaseTemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return
        [
            new FunctionParameter(
                "numbers",
                [FunctionParamValueType.Number],
                Required: true,
                Variadic: true
            )
        ];
    }

    public override string GetName() => "max";

    protected override object? Execute(object?[] args, TextSpan span)
    {
        var numbers = (object?[])args[0]!;
        return numbers.Max(n => Convert.ToDouble(n));
    }
}

public class SideEffectFunction : BaseTemplateFunction
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

    public override string GetName() => "sideEffect";

    protected override object? Execute(object?[] args, TextSpan span)
    {
        _action();
        return null;
    }
}

public class FailingFunction : BaseTemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return [];
    }

    public override string GetName() => "failing";

    protected override object? Execute(object?[] args, TextSpan span)
    {
        throw Error("Test error", span);
    }
}

public class DivideFunction : BaseTemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return
        [
            new FunctionParameter("a", [FunctionParamValueType.Number]),
            new FunctionParameter("b", [FunctionParamValueType.Number])
        ];
    }

    public override string GetName() => "divide";

    protected override object? Execute(object?[] args, TextSpan span)
    {
        var a = Convert.ToDouble(args[0]);
        var b = Convert.ToDouble(args[1]);
        if (b == 0)
            throw Error("Division by zero", span);
        return a / b;
    }
}

public class OuterFunction : BaseTemplateFunction
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

    public override string GetName() => "outer";

    protected override object? Execute(object?[] args, TextSpan span)
    {
        return args[0];
    }
}

public class InnerFunction : BaseTemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return [];
    }

    public override string GetName() => "inner";

    protected override object? Execute(object?[] args, TextSpan span)
    {
        return "inner result";
    }
}

public class ValidatorFunction : BaseTemplateFunction
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

    public override string GetName() => "validator";

    protected override object? Execute(object?[] args, TextSpan span)
    {
        throw Error("validation failed", span);
    }
}

public class PrintFunction : BaseTemplateFunction
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

    public override string GetName() => "print";

    protected override object? Execute(object?[] args, TextSpan span)
    {
        return args[0]?.ToString();
    }
}

public class TestUuidFunction : BaseTemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return [];
    }

    public override string GetName() => "uuid";

    protected override object? Execute(object?[] args, TextSpan span)
    {
        return "test-uuid-123";
    }
}

public class TestTimestampFunction : BaseTemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return [];
    }

    public override string GetName() => "timestamp";

    protected override object? Execute(object?[] args, TextSpan span)
    {
        return 1234567890;
    }
}

public class TestUpperFunction : BaseTemplateFunction
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

    public override string GetName() => "upper";

    protected override object? Execute(object?[] args, TextSpan span)
    {
        return args[0]?.ToString()?.ToUpper();
    }
}
