using Nightmare.Parser;

namespace Nightmare.Tests.ParserTests;

public class JsonParseExceptionTests
{
    [Fact]
    public void Constructor_StoresMessageAndSpan()
    {
        var span = new TextSpan(10, 5, 2, 3, 2, 8);
        var exception = new JsonParseException("Test error", span);

        Assert.Equal("Test error", exception.Message);
        Assert.Equal(span, exception.Span);
    }

    [Fact]
    public void Line_ReturnsStartLine()
    {
        var span = new TextSpan(10, 5, 3, 5, 3, 10);
        var exception = new JsonParseException("Test error", span);

        Assert.Equal(3, exception.Line);
    }

    [Fact]
    public void Column_ReturnsStartColumn()
    {
        var span = new TextSpan(10, 5, 3, 7, 3, 12);
        var exception = new JsonParseException("Test error", span);

        Assert.Equal(7, exception.Column);
    }

    [Fact]
    public void JsonParseException_InheritsFromException()
    {
        var span = new TextSpan(0, 1, 1, 1, 1, 1);
        var exception = new JsonParseException("Test", span);

        Assert.IsAssignableFrom<Exception>(exception);
    }
}