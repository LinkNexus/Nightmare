namespace Nightmare.Parser;

public class JsonParseException(string message, TextSpan span)
    : Exception(message)
{
    public TextSpan Span { get; } = span;
    public int Line => Span.StartLine;
    public int Column => Span.StartColumn;
}