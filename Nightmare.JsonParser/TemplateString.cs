namespace Nightmare.JsonParser;

public sealed class TemplateString(IReadOnlyList<TemplateSegment> segments)
{
    public bool HasExpressions => segments
        .OfType<TemplateExpressionSegment>()
        .Any();

    public override string ToString()
    {
        return string.Concat(
            segments.Select(s => s switch
            {
                TemplateTextSegment text => text.Text,
                TemplateExpressionSegment expression => $"{{{{{expression.Expression}}}}}",
                _ => string.Empty
            })
        );
    }
}

public abstract record TemplateSegment(TextSpan Span);

public sealed record TemplateTextSegment(string Text, TextSpan Span) : TemplateSegment(Span);

public sealed record TemplateExpressionSegment(string Expression, TextSpan Span) : TemplateSegment(Span);