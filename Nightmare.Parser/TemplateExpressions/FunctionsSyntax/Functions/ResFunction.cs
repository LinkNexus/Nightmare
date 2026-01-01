namespace Nightmare.Parser.TemplateExpressions.FunctionsSyntax.Functions;

/// <summary>
/// Function to retrieve cached response data from a previous request execution
/// </summary>
public class ResFunction(EvaluationContext context) : TemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return
        [
            new FunctionParameter(
                "requestId",
                [FunctionParamValueType.String]
            ),
            new FunctionParameter(
                "trigger",
                [FunctionParamValueType.String],
                false,
                "10 minutes"
            )
        ];
    }

    public override string GetName()
    {
        return "res";
    }

    protected override object? Execute(object?[] args, TextSpan span)
    {
        var requestId = (string)args[0]!;
        var trigger = (string)args[1]!;

        if (
            context.ResponseCache.TryGetValue(requestId, out var response)
            && !response.IsStale(ParseTrigger(trigger, span))
        )
            return response.Convert();

        var request = GetRequest(requestId, span);
        response = ExecuteRequest(request, requestId, span);

        return response.Convert();
    }

    private TimeSpan ParseTrigger(string trigger, TextSpan span)
    {
        var parts = trigger.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
            throw Error($"Invalid trigger format: '{trigger}'. Expected format: '<number> <unit>' (e.g., '10 minutes')",
                span);

        if (!double.TryParse(parts[0], out var value))
            throw Error($"Invalid number in trigger: '{parts[0]}'", span);

        var unit = parts[1].ToLowerInvariant();
        return unit switch
        {
            "second" or "seconds" => TimeSpan.FromSeconds(value),
            "minute" or "minutes" => TimeSpan.FromMinutes(value),
            "hour" or "hours" => TimeSpan.FromHours(value),
            "day" or "days" => TimeSpan.FromDays(value),
            "week" or "weeks" => TimeSpan.FromDays(value * 7),
            _ => throw Error($"Invalid time unit: '{unit}'. Valid units: seconds, minutes, hours, days, weeks", span)
        };
    }

    private JsonObject GetRequest(string requestId, TextSpan span)
    {
        var currentRequest = context.Ast;
        if (currentRequest == null)
            throw Error("No AST available in context", span);

        foreach (var part in requestId.Split("."))
        {
            if (!currentRequest.TryGetProperty<JsonObject>("requests", out var subRequests) ||
                !subRequests.TryGetProperty<JsonObject>(part, out var req))
                throw Error($"The request with the id '{requestId}' does not exist", span);

            currentRequest = req;
        }

        return currentRequest;
    }

    private Response ExecuteRequest(JsonObject request, string requestId, TextSpan span)
    {
        if (context.RequestExecutor == null)
            throw Error("Request executor not configured in context", span);

        try
        {
            return context.RequestExecutor(request, requestId).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            throw Error($"Failed to execute request '{requestId}': {ex.Message}", span);
        }
    }
}