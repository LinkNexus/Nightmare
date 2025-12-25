using Nightmare.Parser.TemplateExpressions.FunctionsSyntax;

namespace Nightmare.Parser.TemplateExpressions.Functions;

public class UuidFunction : BaseTemplateFunction
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
        return Guid.NewGuid().ToString();
    }
}

public class HashFunction : BaseTemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return
        [
            new FunctionParameter(
                "input",
                [FunctionParamValueType.String, FunctionParamValueType.Number]
            )
        ];
    }

    public override string GetName()
    {
        return "hash";
    }

    protected override object? Execute(object?[] args, TextSpan span)
    {
        var input = args[0]?.ToString() ?? string.Empty;
        var bytes = System.Text.Encoding.UTF8.GetBytes(input);
        var hash = System.Security.Cryptography.SHA256.HashData(bytes);
        return Convert.ToHexStringLower(hash);
    }
}

public class EnvFunction : BaseTemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return
        [
            new FunctionParameter(
                "name",
                [FunctionParamValueType.String]
            ),
            new FunctionParameter(
                "defaultValue",
                [FunctionParamValueType.String, FunctionParamValueType.Null],
                false,
                null
            )
        ];
    }

    public override string GetName()
    {
        return "env";
    }

    protected override object? Execute(object?[] args, TextSpan span)
    {
        return Environment.GetEnvironmentVariable((string)args[0]!) ?? args[1];
    }
}

public class IfElseFunction : BaseTemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return
        [
            new FunctionParameter(
                "condition",
                [FunctionParamValueType.Boolean]
            ),
            new FunctionParameter(
                "if",
                [
                    FunctionParamValueType.String,
                    FunctionParamValueType.Number,
                    FunctionParamValueType.Array,
                    FunctionParamValueType.Boolean,
                    FunctionParamValueType.Object,
                    FunctionParamValueType.Null
                ]
            ),
            new FunctionParameter("else", [
                FunctionParamValueType.String,
                FunctionParamValueType.Number,
                FunctionParamValueType.Array,
                FunctionParamValueType.Boolean,
                FunctionParamValueType.Object,
                FunctionParamValueType.Null
            ])
        ];
    }

    public override string GetName()
    {
        return "ifElse";
    }

    protected override object? Execute(object?[] args, TextSpan span)
    {
        return (bool)args[0]! ? args[1] : args[2];
    }
}

public class MinFunction : BaseTemplateFunction
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
        return "min";
    }

    protected override object? Execute(object?[] args, TextSpan span)
    {
        var numbers = (object?[])args[0]!;
        if (numbers.Length == 0)
            throw Error("min requires at least one argument", span);
        return numbers.Min(Convert.ToDouble);
    }
}

public class LenFunction : BaseTemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return
        [
            new FunctionParameter(
                "value",
                [FunctionParamValueType.String, FunctionParamValueType.Array]
            )
        ];
    }

    public override string GetName()
    {
        return "len";
    }

    protected override object? Execute(object?[] args, TextSpan span)
    {
        return args[0] switch
        {
            string str => (double)str.Length,
            List<object?> list => (double)list.Count,
            _ => throw Error("len requires a string or array", span)
        };
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
        if (numbers.Length == 0)
            throw Error("max requires at least one argument", span);
        return numbers.Max(Convert.ToDouble);
    }
}