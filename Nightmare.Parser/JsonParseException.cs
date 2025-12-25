namespace Nightmare.Parser;

public class JsonParseException(string message, TextSpan span)
    : ParserException(message, span)
{
}