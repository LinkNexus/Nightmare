using System.Net;
using System.Text;

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

public class UrlDecodeFunction : TemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return
        [
            new FunctionParameter("input", [FunctionParamValueType.String])
        ];
    }

    public override string GetName()
    {
        return "urlDecode";
    }

    protected override object? Execute(object?[] args, TextSpan span)
    {
        return WebUtility.UrlDecode((string)args[0]!);
    }
}

public class UrlEncodeFunction : TemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return
        [
            new FunctionParameter("input", [FunctionParamValueType.String])
        ];
    }

    public override string GetName()
    {
        return "urlEncode";
    }

    protected override object? Execute(object?[] args, TextSpan span)
    {
        return WebUtility.UrlEncode((string)args[0]!);
    }
}

public class Base64EncodeFunction : TemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return
        [
            new FunctionParameter("input", [FunctionParamValueType.String])
        ];
    }

    public override string GetName()
    {
        return "base64Encode";
    }

    protected override object? Execute(object?[] args, TextSpan span)
    {
        return Convert.ToBase64String(
            Encoding.UTF8.GetBytes((string)args[0]!)
        );
    }
}

public class Base64DecodeFunction : TemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return
        [
            new FunctionParameter("input", [FunctionParamValueType.String])
        ];
    }

    public override string GetName()
    {
        return "base64Decode";
    }

    protected override object? Execute(object?[] args, TextSpan span)
    {
        return Encoding.UTF8.GetString(
            Convert.FromBase64String((string)args[0]!)
        );
    }
}