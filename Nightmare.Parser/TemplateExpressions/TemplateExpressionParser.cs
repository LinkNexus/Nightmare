using System.Globalization;

namespace Nightmare.Parser.TemplateExpressions;

/// <summary>
/// Parses template expressions into an abstract syntax tree (AST)
/// Supports:
/// - Literals: numbers, strings, booleans, null
/// - Identifiers and member access (obj.prop)
/// - Index access (arr[0])
/// - Function calls (func(arg1, arg2))
/// - Binary operations: +, -, *, /, %, ==, !=, <, <=, >, >=, &&, ||
/// - Unary operations: !, -
/// - Ternary/conditional: condition ? then : else
/// Operator precedence (from lowest to highest):
/// 1. Ternary (?:)
/// 2. Logical OR (||)
/// 3. Logical AND (&&)
/// 4. Equality (==, !=)
/// 5. Relational (<, <=, >, >=)
/// 6. Additive (+, -)
/// 7. Multiplicative (*, /, %)
/// 8. Unary (!, -)
/// 9. Postfix (., [], ())
/// </summary>
public sealed class TemplateExpressionParser
{
    private readonly IReadOnlyList<TemplateExpressionToken> _tokens;
    private int _position;

    private TemplateExpressionToken Current => _tokens[_position];

    private TemplateExpressionParser(IReadOnlyList<TemplateExpressionToken> tokens)
    {
        _tokens = tokens;
    }

    private TemplateExpressionException Error(string message)
    {
        return new TemplateExpressionException(message, Current.Span);
    }

    private TemplateExpressionToken Eat(TemplateTokenType type)
    {
        if (Current.Type != type)
            throw Error($"Expected token '{type}' but got '{Current.Type}'");

        var current = Current;
        _position++;
        return current;
    }

    private static TextSpan Combine(TextSpan a, TextSpan b)
    {
        return new TextSpan(
            a.Start,
            Math.Abs(b.End - a.Start),
            a.StartLine,
            a.StartColumn,
            b.EndLine,
            b.EndColumn
        );
    }

    /// <summary>
    /// Primary expression: literals, identifiers, grouped expressions
    /// </summary>
    private TemplateExpression ParsePrimary()
    {
        switch (Current.Type)
        {
            case TemplateTokenType.Number:
                {
                    var token = Eat(TemplateTokenType.Number);
                    var value = double.Parse(token.Value!, CultureInfo.InvariantCulture);
                    return new NumberLiteralExpression(value, token.Span);
                }

            case TemplateTokenType.String:
                {
                    var token = Eat(TemplateTokenType.String);
                    return new StringLiteralExpression(token.Value!, token.Span);
                }

            case TemplateTokenType.True:
                {
                    var token = Eat(TemplateTokenType.True);
                    return new BooleanLiteralExpression(true, token.Span);
                }

            case TemplateTokenType.False:
                {
                    var token = Eat(TemplateTokenType.False);
                    return new BooleanLiteralExpression(false, token.Span);
                }

            case TemplateTokenType.Null:
                {
                    var token = Eat(TemplateTokenType.Null);
                    return new NullLiteralExpression(token.Span);
                }

            case TemplateTokenType.Identifier:
                {
                    var token = Eat(TemplateTokenType.Identifier);
                    return new IdentifierExpression(token.Value!, token.Span);
                }

            case TemplateTokenType.LeftParen:
                {
                    Eat(TemplateTokenType.LeftParen);
                    var expr = ParseExpression();
                    Eat(TemplateTokenType.RightParen);
                    return expr;
                }

            default:
                throw Error($"Unexpected token '{Current.Type}' in expression");
        }
    }

    /// <summary>
    /// Postfix expressions: member access, index access, function calls
    /// </summary>
    private TemplateExpression ParsePostfix()
    {
        var expr = ParsePrimary();

        while (true)
        {
            switch (Current.Type)
            {
                case TemplateTokenType.Dot:
                    {
                        Eat(TemplateTokenType.Dot);
                        var memberToken = Eat(TemplateTokenType.Identifier);
                        var span = Combine(expr.Span, memberToken.Span);
                        expr = new MemberAccessExpression(expr, memberToken.Value!, span);
                        break;
                    }

                case TemplateTokenType.LeftBracket:
                    {
                        var start = Eat(TemplateTokenType.LeftBracket);
                        var index = ParseExpression();
                        var end = Eat(TemplateTokenType.RightBracket);
                        var span = Combine(expr.Span, end.Span);
                        expr = new IndexAccessExpression(expr, index, span);
                        break;
                    }

                case TemplateTokenType.LeftParen:
                    {
                        Eat(TemplateTokenType.LeftParen);
                        var arguments = new List<TemplateExpression>();

                        if (Current.Type != TemplateTokenType.RightParen)
                        {
                            while (true)
                            {
                                arguments.Add(ParseExpression());

                                if (Current.Type == TemplateTokenType.Comma)
                                {
                                    Eat(TemplateTokenType.Comma);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }

                        var end = Eat(TemplateTokenType.RightParen);
                        var span = Combine(expr.Span, end.Span);
                        expr = new CallExpression(expr, arguments, span);
                        break;
                    }

                default:
                    return expr;
            }
        }
    }

    /// <summary>
    /// Unary expressions: !, -
    /// </summary>
    private TemplateExpression ParseUnary()
    {
        switch (Current.Type)
        {
            case TemplateTokenType.Not:
                {
                    var token = Eat(TemplateTokenType.Not);
                    var operand = ParseUnary();
                    var span = Combine(token.Span, operand.Span);
                    return new UnaryExpression(UnaryOperator.Not, operand, span);
                }

            case TemplateTokenType.Minus:
                {
                    var token = Eat(TemplateTokenType.Minus);
                    var operand = ParseUnary();
                    var span = Combine(token.Span, operand.Span);
                    return new UnaryExpression(UnaryOperator.Negate, operand, span);
                }

            default:
                return ParsePostfix();
        }
    }

    /// <summary>
    /// Multiplicative expressions: *, /, %
    /// </summary>
    private TemplateExpression ParseMultiplicative()
    {
        var left = ParseUnary();

        while (true)
        {
            BinaryOperator? op = Current.Type switch
            {
                TemplateTokenType.Star => BinaryOperator.Multiply,
                TemplateTokenType.Slash => BinaryOperator.Divide,
                TemplateTokenType.Percent => BinaryOperator.Modulo,
                _ => null
            };

            if (op == null)
                return left;

            var opToken = Current;
            _position++;
            var right = ParseUnary();
            var span = Combine(left.Span, right.Span);
            left = new BinaryExpression(op.Value, left, right, span);
        }
    }

    /// <summary>
    /// Additive expressions: +, -
    /// </summary>
    private TemplateExpression ParseAdditive()
    {
        var left = ParseMultiplicative();

        while (true)
        {
            BinaryOperator? op = Current.Type switch
            {
                TemplateTokenType.Plus => BinaryOperator.Add,
                TemplateTokenType.Minus => BinaryOperator.Subtract,
                _ => null
            };

            if (op == null)
                return left;

            _position++;
            var right = ParseMultiplicative();
            var span = Combine(left.Span, right.Span);
            left = new BinaryExpression(op.Value, left, right, span);
        }
    }

    /// <summary>
    /// Relational expressions: <, <=, >, >=
    /// </summary>
    private TemplateExpression ParseRelational()
    {
        var left = ParseAdditive();

        while (true)
        {
            BinaryOperator? op = Current.Type switch
            {
                TemplateTokenType.LessThan => BinaryOperator.LessThan,
                TemplateTokenType.LessOrEqual => BinaryOperator.LessOrEqual,
                TemplateTokenType.GreaterThan => BinaryOperator.GreaterThan,
                TemplateTokenType.GreaterOrEqual => BinaryOperator.GreaterOrEqual,
                _ => null
            };

            if (op == null)
                return left;

            _position++;
            var right = ParseAdditive();
            var span = Combine(left.Span, right.Span);
            left = new BinaryExpression(op.Value, left, right, span);
        }
    }

    /// <summary>
    /// Equality expressions: ==, !=
    /// </summary>
    private TemplateExpression ParseEquality()
    {
        var left = ParseRelational();

        while (true)
        {
            BinaryOperator? op = Current.Type switch
            {
                TemplateTokenType.Equal => BinaryOperator.Equal,
                TemplateTokenType.NotEqual => BinaryOperator.NotEqual,
                _ => null
            };

            if (op == null)
                return left;

            _position++;
            var right = ParseRelational();
            var span = Combine(left.Span, right.Span);
            left = new BinaryExpression(op.Value, left, right, span);
        }
    }

    /// <summary>
    /// Logical AND expressions: &&
    /// </summary>
    private TemplateExpression ParseLogicalAnd()
    {
        var left = ParseEquality();

        while (Current.Type == TemplateTokenType.And)
        {
            _position++;
            var right = ParseEquality();
            var span = Combine(left.Span, right.Span);
            left = new BinaryExpression(BinaryOperator.And, left, right, span);
        }

        return left;
    }

    /// <summary>
    /// Logical OR expressions: ||
    /// </summary>
    private TemplateExpression ParseLogicalOr()
    {
        var left = ParseLogicalAnd();

        while (Current.Type == TemplateTokenType.Or)
        {
            _position++;
            var right = ParseLogicalAnd();
            var span = Combine(left.Span, right.Span);
            left = new BinaryExpression(BinaryOperator.Or, left, right, span);
        }

        return left;
    }

    /// <summary>
    /// Conditional/ternary expressions: condition ? then : else
    /// </summary>
    private TemplateExpression ParseConditional()
    {
        var condition = ParseLogicalOr();

        if (Current.Type == TemplateTokenType.Question)
        {
            Eat(TemplateTokenType.Question);
            var thenExpr = ParseExpression();
            Eat(TemplateTokenType.Colon);
            var elseExpr = ParseExpression();
            var span = Combine(condition.Span, elseExpr.Span);
            return new ConditionalExpression(condition, thenExpr, elseExpr, span);
        }

        return condition;
    }

    /// <summary>
    /// Top-level expression parsing
    /// </summary>
    private TemplateExpression ParseExpression()
    {
        return ParseConditional();
    }

    /// <summary>
    /// Parse a template expression from a string
    /// </summary>
    public static TemplateExpression Parse(string expression)
    {
        var lexer = new TemplateExpressionLexer(expression);
        var tokens = lexer.Lex();
        var parser = new TemplateExpressionParser(tokens);
        var result = parser.ParseExpression();

        if (parser.Current.Type != TemplateTokenType.EndOfFile)
            throw parser.Error("Unexpected token after expression");

        return result;
    }
}
