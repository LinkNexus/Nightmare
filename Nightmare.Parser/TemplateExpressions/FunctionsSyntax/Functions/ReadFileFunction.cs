using Nightmare.Parser.TemplateExpressions.FunctionsSyntax;

namespace Nightmare.Parser.TemplateExpressions.Functions;

public class ReadFileFunction : BaseTemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return
        [
            new FunctionParameter(
                "path",
                [FunctionParamValueType.String]
            )
        ];
    }

    public override string GetName()
    {
        return "readFile";
    }

    protected override object? Execute(object?[] args, TextSpan span)
    {
        var path = (string)args[0]!;
        return File.Exists(path)
            ? File.ReadAllText(path)
            : throw Error($"File {path} not found", span);
    }
}