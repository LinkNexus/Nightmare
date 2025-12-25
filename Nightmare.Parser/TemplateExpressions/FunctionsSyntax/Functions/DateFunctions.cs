using Nightmare.Parser.TemplateExpressions.FunctionsSyntax;

namespace Nightmare.Parser.TemplateExpressions.Functions;

public class TimeStampFunction : BaseTemplateFunction
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
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}

public class DateFunction : BaseTemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return
        [
            new FunctionParameter(
                "time",
                [FunctionParamValueType.String, FunctionParamValueType.Number],
                false,
                "now"
            ),
            new FunctionParameter(
                "format",
                [FunctionParamValueType.String],
                false,
                "yyyy-MM-dd HH:mm:ss"
            )
        ];
    }

    public override string GetName()
    {
        return "date";
    }

    protected override object? Execute(object?[] args, TextSpan span)
    {
        var time = args[0]!;
        var format = (string)args[1]!;

        switch (time)
        {
            case double unixTime:
            {
                var dateTime = DateTimeOffset.FromUnixTimeSeconds((long)unixTime).UtcDateTime;
                return dateTime.ToString(format);
            }

            case "years" or "months" or "weeks" or "days" or "hours" or "minutes" or "seconds":
                var parts = format.Split(" ");
                if (parts.Length != 2) throw Error("Invalid date format", span);

                if (int.TryParse(parts[0], out var count))
                    return parts[1] switch
                    {
                        "years" => DateTime.UtcNow.AddYears(count).ToString(format),
                        "months" => DateTime.UtcNow.AddMonths(count).ToString(format),
                        "weeks" => DateTime.UtcNow.AddDays(count * 7).ToString(format),
                        "days" => DateTime.UtcNow.AddDays(count).ToString(format),
                        "hours" => DateTime.UtcNow.AddHours(count).ToString(format),
                        "minutes" => DateTime.UtcNow.AddMinutes(count).ToString(format),
                        "seconds" => DateTime.UtcNow.AddSeconds(count).ToString(format),
                        _ => throw Error("Invalid time unit", span)
                    };
                throw Error("The first part of the time must be a number", span);

            case "now":
                return DateTime.Now.ToString(format);

            default:
                throw Error("Invalid time", span);
        }
    }
}