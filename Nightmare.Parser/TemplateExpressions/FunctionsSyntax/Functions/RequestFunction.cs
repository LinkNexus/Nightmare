namespace Nightmare.Parser.TemplateExpressions.FunctionsSyntax.Functions;

public class RequestFunction(EvaluationContext context) : TemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return
        [
            new FunctionParameter(
                "requestId",
                [FunctionParamValueType.String]
            )
        ];
    }

    public override string GetName()
    {
        return "req";
    }

    protected override object? Execute(object?[] args, TextSpan span)
    {
        var reqId = (string)args[0]!;
        var currentRequest = context.Ast!;

        foreach (var part in reqId.Split("."))
        {
            if (!currentRequest.TryGetProperty<JsonObject>("requests", out var subRequests) ||
                !subRequests.TryGetProperty<JsonObject>(part, out var req))
                throw Error($"The request with the id {reqId} does not exist", span);

            currentRequest = req;
        }

        return currentRequest;
    }
}