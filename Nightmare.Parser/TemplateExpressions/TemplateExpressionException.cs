namespace Nightmare.Parser.TemplateExpressions;

public class TemplateExpressionException(string message, TextSpan span)
    : ParserException(message, span)
{
}