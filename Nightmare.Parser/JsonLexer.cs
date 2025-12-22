using System.Text;
using System.Text.RegularExpressions;

namespace Nightmare.Parser;

public class JsonLexer(string _text)
{
    private int _column = 1;
    private int _line = 1;
    private int _position;

    private bool IsEnd => _position >= _text.Length;

    private char Peek(int offset = 0)
    {
        var index = _position + offset;
        return index >= _text.Length ? '\0' : _text[index];
    }

    private void Advance()
    {
        if (IsEnd) return;

        var c = _text[_position++];
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

    private bool RemainingStartsWith(string input)
    {
        return input.Length <= _text.Length - _position && _text.AsSpan(_position, input.Length).StartsWith(input);
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

    private void SkipTrivialChars()
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
                    if ((c = Peek(1)) == '\n')
                    {
                        Advance();
                        AdvanceLine(c);
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

    private Token MakeSingle(TokenType tokenType)
    {
        var start = CaptureStart();
        Advance();
        return new Token(tokenType, null, null, CaptureSpan(start));
    }

    private void FlushTextSegment(StringBuilder sb, List<TemplateSegment> segments, int segmentStartIndex,
        int segmentStartPos, int segmentStartLine, int segmentStartColumn)
    {
        if (sb.Length <= segmentStartIndex) return;

        var text = sb.ToString(segmentStartIndex, sb.Length - segmentStartIndex);

        if (text.Length == 0) return;

        var endColumn = segmentStartColumn + text.Length - 1;
        var span = new TextSpan(
            segmentStartPos,
            text.Length,
            segmentStartLine,
            segmentStartColumn,
            _line,
            endColumn
        );

        segments.Add(new TemplateTextSegment(text, span));
    }

    private int ReadHexCodePoint()
    {
        var code = 0;

        for (var i = 0; i < 4; i++)
        {
            if (IsEnd)
                throw new JsonParseException(
                    "Unexpected end while reading hexadecimal escape sequence",
                    new TextSpan(_position, 1, _line, _column, _line, _column)
                );

            var c = Peek();
            var value = c switch
            {
                >= '0' and <= '9' => c - '0',
                >= 'a' and <= 'f' => c - 'a' + 10,
                >= 'A' and <= 'F' => c - 'A' + 10,
                _ => -1
            };

            if (value < 0)
                throw new JsonParseException(
                    "Invalid unicode escape",
                    new TextSpan(_position, 1, _line, _column, _line, _column)
                );
            code = (code << 4) + value;
            Advance();
        }

        return code;
    }

    private Token ReadString()
    {
        var start = CaptureStart();
        var startLine = _line;
        var startColumn = _column;
        Advance(); // We skip the opening quote

        var sb = new StringBuilder();
        var segments = new List<TemplateSegment>();
        var segmentStartIndex = 0;
        var segmentStartPos = _position;
        var segmentStartLine = _line;
        var segmentStartColumn = _column;

        while (!IsEnd)
        {
            var c = Peek();
            switch (c)
            {
                case '"':
                {
                    FlushTextSegment(sb, segments, segmentStartIndex, segmentStartPos, segmentStartLine,
                        segmentStartColumn);
                    Advance();
                    var span = CaptureSpan(start);
                    var template = new TemplateString(segments);
                    return new Token(TokenType.String, sb.ToString(), template, span);
                }
                case '\\':
                {
                    Advance();
                    if (IsEnd) throw new JsonParseException("Unterminated string literal", CaptureSpan(start));
                    var escaped = Peek();

                    switch (escaped)
                    {
                        case '"':
                            sb.Append('"');
                            Advance();
                            break;
                        case '\\':
                            sb.Append('\\');
                            Advance();
                            break;
                        case '/':
                            sb.Append('/');
                            Advance();
                            break;
                        case 'b':
                            sb.Append('\b');
                            Advance();
                            break;
                        case 'f':
                            sb.Append('\f');
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
                        case 'u':
                            Advance();
                            var codePoint = ReadHexCodePoint();
                            sb.Append(char.ConvertFromUtf32(codePoint));
                            break;
                        default:
                            throw new JsonParseException(
                                $"Invalid escape character '{escaped}' in string literal",
                                new TextSpan(
                                    _position, 1, _line, _column, _line, _column
                                )
                            );
                    }

                    continue;
                }
                case '{' when Peek(1) == '{':
                {
                    FlushTextSegment(sb, segments, segmentStartIndex, segmentStartPos, segmentStartLine,
                        segmentStartColumn);

                    var exprStartPos = _position;
                    var exprStartLine = _line;
                    var exprStartColumn = _column;

                    Advance();
                    Advance();

                    var exprBuilder = new StringBuilder();

                    while (true)
                    {
                        if (IsEnd)
                            throw new JsonParseException(
                                "Unterminated Expression",
                                new TextSpan(exprStartPos, 0, exprStartLine, exprStartColumn, _line, _column)
                            );

                        if (Peek() == '}' && Peek(1) == '}')
                        {
                            Advance();
                            Advance();

                            var exprSpan = new TextSpan(
                                exprStartPos, _position - exprStartPos, exprStartLine, exprStartColumn, _line,
                                _column - 1
                            );
                            segments.Add(new TemplateExpressionSegment(exprBuilder.ToString().Trim(), exprSpan));
                            segmentStartIndex = sb.Length;
                            segmentStartPos = _position;
                            segmentStartLine = _line;
                            segmentStartColumn = _column;
                            break;
                        }

                        exprBuilder.Append(Peek());
                        Advance();
                    }

                    continue;
                }
                case '\n' or '\r':
                    throw new JsonParseException("Unterminated string", CaptureSpan(start));
                default:
                    sb.Append(c);
                    Advance();
                    break;
            }
        }

        throw new JsonParseException("Unterminated string", CaptureSpan(start));
    }

    private Token ReadNumber()
    {
        var start = CaptureStart();
        var sb = new StringBuilder();

        if (Peek() == '-')
        {
            sb.Append(Peek());
            Advance();
        }

        if (!char.IsDigit(Peek()))
            throw new JsonParseException(
                "Invalid number",
                new TextSpan(_position, 1, _line, _column, _line, _column)
            );

        if (Peek() == '0')
        {
            sb.Append('0');
            Advance();
        }
        else
        {
            while (char.IsDigit(Peek()))
            {
                sb.Append(Peek());
                Advance();
            }
        }

        if (Peek() == '.')
        {
            sb.Append(Peek());
            Advance();

            if (!char.IsDigit(Peek()))
                throw new JsonParseException(
                    "Invalid number fraction",
                    new TextSpan(_position, 1, _line, _column, _line, _column)
                );

            while (char.IsDigit(Peek()))
            {
                sb.Append(Peek());
                Advance();
            }
        }

        if (Peek() == 'e' || Peek() == 'E')
        {
            sb.Append(Peek());
            Advance();

            if (Peek() == '+' || Peek() == '-')
            {
                sb.Append(Peek());
                Advance();
            }

            if (!char.IsDigit(Peek()))
                throw new JsonParseException(
                    "Invalid number exponent",
                    new TextSpan(_position, 1, _line, _column, _line, _column)
                );

            while (char.IsDigit(Peek()))
            {
                sb.Append(Peek());
                Advance();
            }
        }

        var text = sb.ToString();
        var finalSpan = CaptureSpan(start);
        return new Token(TokenType.Number, text, null, finalSpan);
    }

    private bool TryMatchKeyword(string text, TokenType type, out Token token)
    {
        if (RemainingStartsWith(text))
        {
            var start = CaptureStart();
            for (var i = 0; i < text.Length; i++) Advance();

            var span = CaptureSpan(start);
            token = new Token(type, text, null, span);

            return true;
        }

        token = null;
        return false;
    }

    private Token ReadKeywordOrError()
    {
        if (TryMatchKeyword("true", TokenType.True, out var token) ||
            TryMatchKeyword("false", TokenType.False, out token) ||
            TryMatchKeyword("null", TokenType.Null, out token)) return token;

        throw new JsonParseException(
            $"Unexpected character '{Peek()}'",
            new TextSpan(_position, 1, _line, _column, _line, _column)
        );
    }

    private Token NextToken()
    {
        SkipTrivialChars();

        if (IsEnd)
            return new Token(
                TokenType.EndOfFile,
                null,
                null,
                new TextSpan(_position, 0, _line, _column, _line, _column)
            );

        var current = Peek();
        return current switch
        {
            '{' => MakeSingle(TokenType.LeftBrace),
            '}' => MakeSingle(TokenType.RightBrace),
            '[' => MakeSingle(TokenType.LeftBracket),
            ']' => MakeSingle(TokenType.RightBracket),
            ':' => MakeSingle(TokenType.Colon),
            ',' => MakeSingle(TokenType.Comma),
            '"' => ReadString(),
            '-' or >= '0' and <= '9' => ReadNumber(),
            _ => ReadKeywordOrError()
        };
    }

    public IReadOnlyList<Token> Lex()
    {
        var tokens = new List<Token>();
        Token token;

        do
        {
            token = NextToken();
            tokens.Add(token);
        } while (token.Type != TokenType.EndOfFile);

        return tokens;
    }
}