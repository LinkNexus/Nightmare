namespace Nightmare.Parser.TemplateExpressions.FunctionsSyntax.Functions;

public class UpperFunction : TemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return
        [
            new FunctionParameter(
                "input",
                [FunctionParamValueType.String]
            )
        ];
    }

    public override string GetName()
    {
        return "upper";
    }

    protected override object? Execute(object?[] args, TextSpan span)
    {
        return ((string)args[0]!).ToUpperInvariant();
    }
}

public class LowerFunction : TemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return
        [
            new FunctionParameter(
                "input",
                [FunctionParamValueType.String]
            )
        ];
    }

    public override string GetName()
    {
        return "lower";
    }

    protected override object? Execute(object?[] args, TextSpan span)
    {
        return ((string)args[0]!).ToLowerInvariant();
    }
}

public class ConcatFunction : TemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return
        [
            new FunctionParameter(
                "values",
                [FunctionParamValueType.String],
                true,
                Variadic: true
            )
        ];
    }

    public override string GetName()
    {
        return "concat";
    }

    protected override object? Execute(object?[] args, TextSpan span)
    {
        // Args[0] will be an array of strings (variadic)
        var values = (object?[])args[0]!;
        return string.Concat(values.Select(v => v?.ToString() ?? ""));
    }
}