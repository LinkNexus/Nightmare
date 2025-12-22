using Nightmare.Parser;

namespace Nightmare.Tests.ParserTests;

public class TextSpanTests
{
    [Fact]
    public void End_CalculatesCorrectly()
    {
        var span = new TextSpan(10, 5, 1, 1, 1, 5);

        Assert.Equal(15, span.End);
    }

    [Fact]
    public void Constructor_StoresAllProperties()
    {
        var span = new TextSpan(10, 5, 2, 3, 2, 8);

        Assert.Equal(10, span.Start);
        Assert.Equal(5, span.Length);
        Assert.Equal(2, span.StartLine);
        Assert.Equal(3, span.StartColumn);
        Assert.Equal(2, span.EndLine);
        Assert.Equal(8, span.EndColumn);
    }

    [Fact]
    public void ToString_FormatsCorrectly()
    {
        var span = new TextSpan(0, 10, 1, 1, 2, 5);

        var result = span.ToString();

        Assert.Contains("1", result);
        Assert.Contains("2", result);
        Assert.Contains("5", result);
    }

    [Fact]
    public void TextSpan_IsRecordStruct_SupportsEquality()
    {
        var span1 = new TextSpan(10, 5, 1, 1, 1, 5);
        var span2 = new TextSpan(10, 5, 1, 1, 1, 5);
        var span3 = new TextSpan(10, 6, 1, 1, 1, 6);

        Assert.Equal(span1, span2);
        Assert.NotEqual(span1, span3);
    }
}
