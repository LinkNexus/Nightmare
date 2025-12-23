namespace Nightmare.Parser.TemplateExpressions;

public enum TemplateTokenType
{
    // Literals
    Number,
    String,
    True,
    False,
    Null,

    // Identifiers and keywords
    Identifier,

    // Operators
    Plus,           // +
    Minus,          // -
    Star,           // *
    Slash,          // /
    Percent,        // %

    // Comparison
    Equal,          // ==
    NotEqual,       // !=
    LessThan,       // <
    LessOrEqual,    // <=
    GreaterThan,    // >
    GreaterOrEqual, // >=

    // Logical
    And,            // &&
    Or,             // ||
    Not,            // !

    // Delimiters
    LeftParen,      // (
    RightParen,     // )
    LeftBracket,    // [
    RightBracket,   // ]
    Dot,            // .
    Comma,          // ,
    Question,       // ?
    Colon,          // :

    // Special
    EndOfFile
}

public sealed class TemplateExpressionToken(
    TemplateTokenType type,
    string? value,
    TextSpan span
)
{
    public TemplateTokenType Type { get; } = type;
    public string? Value { get; } = value;
    public TextSpan Span { get; } = span;

    public override string ToString()
    {
        return Value != null ? $"{Type} ({Value}) @{Span}" : $"{Type} @{Span}";
    }
}
