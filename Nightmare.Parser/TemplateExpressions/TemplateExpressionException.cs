namespace Nightmare.Parser.TemplateExpressions;

public class TemplateExpressionException(string message, TextSpan span)
    : Exception(message)
{
    public TextSpan Span { get; } = span;
    public int Line => Span.StartLine;
    public int Column => Span.StartColumn;
}
