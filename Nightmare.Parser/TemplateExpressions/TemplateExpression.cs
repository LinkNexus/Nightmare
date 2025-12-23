namespace Nightmare.Parser.TemplateExpressions;

/// <summary>
/// Base class for all template expression AST nodes
/// </summary>
public abstract class TemplateExpression(TextSpan span)
{
    public TextSpan Span { get; } = span;
}

// Literals

public sealed class LiteralExpression(object? value, TextSpan span) : TemplateExpression(span)
{
    public object? Value { get; } = value;
}

public sealed class StringLiteralExpression(string value, TextSpan span) : TemplateExpression(span)
{
    public string Value { get; } = value;
}

public sealed class NumberLiteralExpression(double value, TextSpan span) : TemplateExpression(span)
{
    public double Value { get; } = value;
}

public sealed class BooleanLiteralExpression(bool value, TextSpan span) : TemplateExpression(span)
{
    public bool Value { get; } = value;
}

public sealed class NullLiteralExpression(TextSpan span) : TemplateExpression(span)
{
}

// Identifier

public sealed class IdentifierExpression(string name, TextSpan span) : TemplateExpression(span)
{
    public string Name { get; } = name;
}

// Member access

public sealed class MemberAccessExpression(
    TemplateExpression target,
    string memberName,
    TextSpan span
) : TemplateExpression(span)
{
    public TemplateExpression Target { get; } = target;
    public string MemberName { get; } = memberName;
}

// Index access

public sealed class IndexAccessExpression(
    TemplateExpression target,
    TemplateExpression index,
    TextSpan span
) : TemplateExpression(span)
{
    public TemplateExpression Target { get; } = target;
    public TemplateExpression Index { get; } = index;
}

// Function call

public sealed class CallExpression(
    TemplateExpression callee,
    IReadOnlyList<TemplateExpression> arguments,
    TextSpan span
) : TemplateExpression(span)
{
    public TemplateExpression Callee { get; } = callee;
    public IReadOnlyList<TemplateExpression> Arguments { get; } = arguments;
}

// Unary operations

public enum UnaryOperator
{
    Not,
    Negate
}

public sealed class UnaryExpression(
    UnaryOperator @operator,
    TemplateExpression operand,
    TextSpan span
) : TemplateExpression(span)
{
    public UnaryOperator Operator { get; } = @operator;
    public TemplateExpression Operand { get; } = operand;
}

// Binary operations

public enum BinaryOperator
{
    // Arithmetic
    Add,
    Subtract,
    Multiply,
    Divide,
    Modulo,

    // Comparison
    Equal,
    NotEqual,
    LessThan,
    LessOrEqual,
    GreaterThan,
    GreaterOrEqual,

    // Logical
    And,
    Or
}

public sealed class BinaryExpression(
    BinaryOperator @operator,
    TemplateExpression left,
    TemplateExpression right,
    TextSpan span
) : TemplateExpression(span)
{
    public BinaryOperator Operator { get; } = @operator;
    public TemplateExpression Left { get; } = left;
    public TemplateExpression Right { get; } = right;
}

// Ternary/Conditional

public sealed class ConditionalExpression(
    TemplateExpression condition,
    TemplateExpression thenExpression,
    TemplateExpression elseExpression,
    TextSpan span
) : TemplateExpression(span)
{
    public TemplateExpression Condition { get; } = condition;
    public TemplateExpression ThenExpression { get; } = thenExpression;
    public TemplateExpression ElseExpression { get; } = elseExpression;
}
