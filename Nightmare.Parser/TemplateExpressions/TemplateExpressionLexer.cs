using System.Text;

namespace Nightmare.Parser.TemplateExpressions;

public class TemplateExpressionLexer(string text)
{
    private int _column = 1;
    private int _line = 1;
    private int _position;

    private bool IsEnd => _position >= text.Length;

    private char Peek(int offset = 0)
    {
        var index = _position + offset;
        return index >= text.Length ? '\0' : text[index];
    }

    private void Advance()
    {
        if (IsEnd) return;

        var c = text[_position++];
        switch (c)
        {
            case '\n':
            case '\r' when Peek() != '\n':
                _line++;
                _column = 1;
                break;
            default:
                _column++;
                break;
        }
    }

    private void AdvanceLine(char c)
    {
        Advance();
        if (c == '\r' && Peek() == '\n') Advance();
    }

    private TextSpan CaptureStart()
    {
        return new TextSpan(
            _position,
            0,
            _line,
            _column,
            _line,
            _column
        );
    }

    private TextSpan CaptureSpan(TextSpan start)
    {
        var length = _position - start.Start;
        var endColumn = _column > 0 ? _column - 1 : 1;

        return new TextSpan(
            start.Start,
            length,
            start.StartLine,
            start.StartColumn,
            _line,
            endColumn
        );
    }

    private void SkipWhitespace()
    {
        while (!IsEnd)
        {
            var c = Peek();

            switch (c)
            {
                case ' ':
                case '\t':
                    Advance();
                    continue;
                case '\n':
                    AdvanceLine(c);
                    continue;
                case '\r':
                    {
                        if (Peek(1) == '\n')
                        {
                            Advance();
                            AdvanceLine('\n');
                        }
                        else
                        {
                            AdvanceLine(c);
                        }
                        continue;
                    }
            }

            break;
        }
    }

    private TemplateExpressionToken MakeSingle(TemplateTokenType tokenType)
    {
        var start = CaptureStart();
        Advance();
        return new TemplateExpressionToken(tokenType, null, CaptureSpan(start));
    }

    private TemplateExpressionToken ReadString()
    {
        var start = CaptureStart();
        var quote = Peek();
        Advance(); // Skip opening quote

        var sb = new StringBuilder();

        while (!IsEnd)
        {
            var c = Peek();

            if (c == quote)
            {
                Advance();
                return new TemplateExpressionToken(TemplateTokenType.String, sb.ToString(), CaptureSpan(start));
            }

            if (c == '\\')
            {
                Advance();
                if (IsEnd)
                    throw new TemplateExpressionException("Unterminated string literal", CaptureSpan(start));

                var escaped = Peek();
                switch (escaped)
                {
                    case '"':
                    case '\'':
                    case '\\':
                        sb.Append(escaped);
                        Advance();
                        break;
                    case 'n':
                        sb.Append('\n');
                        Advance();
                        break;
                    case 'r':
                        sb.Append('\r');
                        Advance();
                        break;
                    case 't':
                        sb.Append('\t');
                        Advance();
                        break;
                    default:
                        throw new TemplateExpressionException(
                            $"Invalid escape character '\\{escaped}' in string literal",
                            new TextSpan(_position, 1, _line, _column, _line, _column)
                        );
                }
                continue;
            }

            if (c == '\n' || c == '\r')
                throw new TemplateExpressionException("Unterminated string literal", CaptureSpan(start));

            sb.Append(c);
            Advance();
        }

        throw new TemplateExpressionException("Unterminated string literal", CaptureSpan(start));
    }

    private TemplateExpressionToken ReadNumber()
    {
        var start = CaptureStart();
        var sb = new StringBuilder();

        // Handle negative numbers
        if (Peek() == '-')
        {
            sb.Append(Peek());
            Advance();
        }

        if (!char.IsDigit(Peek()))
            throw new TemplateExpressionException(
                "Invalid number",
                new TextSpan(_position, 1, _line, _column, _line, _column)
            );

        // Read integer part
        while (char.IsDigit(Peek()))
        {
            sb.Append(Peek());
            Advance();
        }

        // Read decimal part
        if (Peek() == '.')
        {
            sb.Append(Peek());
            Advance();

            if (!char.IsDigit(Peek()))
                throw new TemplateExpressionException(
                    "Invalid number fraction",
                    new TextSpan(_position, 1, _line, _column, _line, _column)
                );

            while (char.IsDigit(Peek()))
            {
                sb.Append(Peek());
                Advance();
            }
        }

        // Read exponent part
        if (Peek() is 'e' or 'E')
        {
            sb.Append(Peek());
            Advance();

            if (Peek() is '+' or '-')
            {
                sb.Append(Peek());
                Advance();
            }

            if (!char.IsDigit(Peek()))
                throw new TemplateExpressionException(
                    "Invalid number exponent",
                    new TextSpan(_position, 1, _line, _column, _line, _column)
                );

            while (char.IsDigit(Peek()))
            {
                sb.Append(Peek());
                Advance();
            }
        }

        return new TemplateExpressionToken(TemplateTokenType.Number, sb.ToString(), CaptureSpan(start));
    }

    private TemplateExpressionToken ReadIdentifier()
    {
        var start = CaptureStart();
        var sb = new StringBuilder();

        while (!IsEnd && (char.IsLetterOrDigit(Peek()) || Peek() == '_'))
        {
            sb.Append(Peek());
            Advance();
        }

        var text = sb.ToString();
        var span = CaptureSpan(start);

        var type = text switch
        {
            "true" => TemplateTokenType.True,
            "false" => TemplateTokenType.False,
            "null" => TemplateTokenType.Null,
            _ => TemplateTokenType.Identifier
        };

        return new TemplateExpressionToken(type, text, span);
    }

    private TemplateExpressionToken NextToken()
    {
        SkipWhitespace();

        if (IsEnd)
            return new TemplateExpressionToken(
                TemplateTokenType.EndOfFile,
                null,
                new TextSpan(_position, 0, _line, _column, _line, _column)
            );

        var current = Peek();

        // Two-character operators
        if (current == '=' && Peek(1) == '=')
        {
            var start = CaptureStart();
            Advance();
            Advance();
            return new TemplateExpressionToken(TemplateTokenType.Equal, "==", CaptureSpan(start));
        }

        if (current == '!' && Peek(1) == '=')
        {
            var start = CaptureStart();
            Advance();
            Advance();
            return new TemplateExpressionToken(TemplateTokenType.NotEqual, "!=", CaptureSpan(start));
        }

        if (current == '<' && Peek(1) == '=')
        {
            var start = CaptureStart();
            Advance();
            Advance();
            return new TemplateExpressionToken(TemplateTokenType.LessOrEqual, "<=", CaptureSpan(start));
        }

        if (current == '>' && Peek(1) == '=')
        {
            var start = CaptureStart();
            Advance();
            Advance();
            return new TemplateExpressionToken(TemplateTokenType.GreaterOrEqual, ">=", CaptureSpan(start));
        }

        if (current == '&' && Peek(1) == '&')
        {
            var start = CaptureStart();
            Advance();
            Advance();
            return new TemplateExpressionToken(TemplateTokenType.And, "&&", CaptureSpan(start));
        }

        if (current == '|' && Peek(1) == '|')
        {
            var start = CaptureStart();
            Advance();
            Advance();
            return new TemplateExpressionToken(TemplateTokenType.Or, "||", CaptureSpan(start));
        }

        // Single-character operators and delimiters
        return current switch
        {
            '+' => MakeSingle(TemplateTokenType.Plus),
            '-' when !char.IsDigit(Peek(1)) => MakeSingle(TemplateTokenType.Minus),
            '*' => MakeSingle(TemplateTokenType.Star),
            '/' => MakeSingle(TemplateTokenType.Slash),
            '%' => MakeSingle(TemplateTokenType.Percent),
            '<' => MakeSingle(TemplateTokenType.LessThan),
            '>' => MakeSingle(TemplateTokenType.GreaterThan),
            '!' => MakeSingle(TemplateTokenType.Not),
            '(' => MakeSingle(TemplateTokenType.LeftParen),
            ')' => MakeSingle(TemplateTokenType.RightParen),
            '[' => MakeSingle(TemplateTokenType.LeftBracket),
            ']' => MakeSingle(TemplateTokenType.RightBracket),
            '.' => MakeSingle(TemplateTokenType.Dot),
            ',' => MakeSingle(TemplateTokenType.Comma),
            '?' => MakeSingle(TemplateTokenType.Question),
            ':' => MakeSingle(TemplateTokenType.Colon),
            '"' or '\'' => ReadString(),
            '-' or >= '0' and <= '9' => ReadNumber(),
            _ when char.IsLetter(current) || current == '_' => ReadIdentifier(),
            _ => throw new TemplateExpressionException(
                $"Unexpected character '{current}'",
                new TextSpan(_position, 1, _line, _column, _line, _column)
            )
        };
    }

    public IReadOnlyList<TemplateExpressionToken> Lex()
    {
        var tokens = new List<TemplateExpressionToken>();
        TemplateExpressionToken token;

        do
        {
            token = NextToken();
            tokens.Add(token);
        } while (token.Type != TemplateTokenType.EndOfFile);

        return tokens;
    }
}
