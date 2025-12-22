using Nightmare.Parser;

namespace Nightmare.Tests.ParserTests;

public class TemplateStringTests
{
    [Fact]
    public void HasExpressions_NoExpressions_ReturnsFalse()
    {
        var span = new TextSpan(0, 5, 1, 1, 1, 5);
        var segments = new List<TemplateSegment>
        {
            new TemplateTextSegment("hello", span)
        };
        var template = new TemplateString(segments);

        Assert.False(template.HasExpressions);
    }

    [Fact]
    public void HasExpressions_WithExpressions_ReturnsTrue()
    {
        var span = new TextSpan(0, 5, 1, 1, 1, 5);
        var segments = new List<TemplateSegment>
        {
            new TemplateTextSegment("hello ", span),
            new TemplateExpressionSegment("name", span)
        };
        var template = new TemplateString(segments);

        Assert.True(template.HasExpressions);
    }

    [Fact]
    public void ToString_OnlyText_ReturnsText()
    {
        var span = new TextSpan(0, 5, 1, 1, 1, 5);
        var segments = new List<TemplateSegment>
        {
            new TemplateTextSegment("hello world", span)
        };
        var template = new TemplateString(segments);

        Assert.Equal("hello world", template.ToString());
    }

    [Fact]
    public void ToString_WithExpression_ReturnsFormattedString()
    {
        var span = new TextSpan(0, 5, 1, 1, 1, 5);
        var segments = new List<TemplateSegment>
        {
            new TemplateTextSegment("Hello ", span),
            new TemplateExpressionSegment("name", span),
            new TemplateTextSegment("!", span)
        };
        var template = new TemplateString(segments);

        Assert.Equal("Hello {{name}}!", template.ToString());
    }

    [Fact]
    public void ToString_MultipleExpressions_ReturnsFormattedString()
    {
        var span = new TextSpan(0, 5, 1, 1, 1, 5);
        var segments = new List<TemplateSegment>
        {
            new TemplateExpressionSegment("first", span),
            new TemplateTextSegment(" ", span),
            new TemplateExpressionSegment("last", span)
        };
        var template = new TemplateString(segments);

        Assert.Equal("{{first}} {{last}}", template.ToString());
    }

    [Fact]
    public void ToString_EmptySegments_ReturnsEmptyString()
    {
        var template = new TemplateString([]);

        Assert.Equal("", template.ToString());
    }
}
