namespace Nightmare.Parser.TemplateExpressions;

public class TemplateExpressionException(string message, TextSpan span)
    : TracedException(message, span)
{
}