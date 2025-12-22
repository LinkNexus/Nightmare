namespace Nightmare.JsonParser;

public enum TokenType
{
    LeftBrace,
    RightBrace,
    LeftBracket,
    RightBracket,
    Colon,
    Comma,
    String,
    Number,
    True,
    False,
    Null,
    EndOfFile
}

public sealed class Token(
    TokenType type,
    string? value,
    TemplateString? template,
    TextSpan span
)
{
    public TokenType Type { get; } = type;
    public string? Value { get; } = value;
    public TemplateString? Template { get; } = template;
    public TextSpan Span { get; } = span;

    public override string ToString()
    {
        return $"{Type} ({Value}) @{Span}";
    }
}