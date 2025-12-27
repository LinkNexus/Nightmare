namespace Nightmare.Parser;

public class JsonParseException(string message, TextSpan span)
    : TracedException(message, span)
{
}