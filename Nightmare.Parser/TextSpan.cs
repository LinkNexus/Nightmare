namespace Nightmare.Parser;

public readonly record struct TextSpan(
    int Start,
    int Length,
    int StartLine,
    int StartColumn,
    int EndLine,
    int EndColumn
    )
{
    public int End => Start + Length;

    public override string ToString() => $"({StartLine},{StartColumn}) - ({EndLine}, {EndColumn})";
}