namespace Nightmare.Parser.TemplateExpressions.FunctionsSyntax.Functions;

public class JsonEncodeFunction(EvaluationContext context) : TemplateFunction
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
                    FunctionParamValueType.Null,
                    FunctionParamValueType.Boolean,
                    FunctionParamValueType.Array,
                    FunctionParamValueType.Object
                ]
            )
        ];
    }

    public override string GetName()
    {
        return "jsonEncode";
    }

    protected override object? Execute(object?[] args, TextSpan span)
    {
        return JsonValueExtensions.Serialize(args[0], context);
    }
}

public class JsonDecodeFunction(EvaluationContext context) : TemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return
        [
            new FunctionParameter(
                "jsonString",
                [FunctionParamValueType.String]
            ),
            new FunctionParameter(
                "parseTemplates",
                [FunctionParamValueType.Boolean],
                false,
                false
            )
        ];
    }

    public override string GetName()
    {
        return "jsonDecode";
    }

    protected override object? Execute(object?[] args, TextSpan span)
    {
        var json = (string)args[0]!;
        var parseTemplates = (bool)args[1]!;

        try
        {
            return JsonValueExtensions.Convert(
                JsonParser.Parse(json),
                parseTemplates ? context : null
            );
        }
        catch (JsonParseException ex)
        {
            throw Error(
                $"{ex.Message} at {ex.Line}:{ex.Column}",
                span
            );
        }
    }
}