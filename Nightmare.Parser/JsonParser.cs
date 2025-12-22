using System.Transactions;

namespace Nightmare.Parser;

public sealed class JsonParser
{
    private readonly IReadOnlyList<Token> _tokens;
    private int _position;

    private Token Current => _tokens[_position];

    private JsonParser(IReadOnlyList<Token> tokens)
    {
        _tokens = tokens;
    }

    private JsonParseException Error(string message)
    {
        return new JsonParseException(message, Current.Span);
    }

    private Token Eat(TokenType type)
    {
        if (Current.Type != type) throw Error($"Expected token '{type}' but got '{Current.Type}'");
        var current = Current;
        _position++;
        return current;
    }

    private JsonNumber ParseNumber()
    {
        var token = Eat(TokenType.Number);
        return new JsonNumber(token.Value ?? "", token.Span);
    }

    private JsonString ParseString()
    {
        var token = Eat(TokenType.String);
        var template = token.Template ?? new TemplateString([]);
        return new JsonString(template, token.Span);
    }

    private JsonBoolean ParseBoolean(bool value)
    {
        var token = Eat(value ? TokenType.True : TokenType.False);
        return new JsonBoolean(value, token.Span);
    }

    private JsonNull ParseNull()
    {
        return new JsonNull(Eat(TokenType.Null).Span);
    }

    private static TextSpan Combine(TextSpan a, TextSpan b)
    {
        return new TextSpan(
            a.Start, Math.Abs(b.End - a.End), a.StartLine, a.StartColumn, b.EndLine, b.EndColumn
        );
    }

    private JsonArray ParseArray()
    {
        var start = Eat(TokenType.LeftBracket);
        var items = new List<JsonValue>();

        if (Current.Type == TokenType.RightBracket)
        {
            var end = Eat(TokenType.RightBracket);
            return new JsonArray(items, Combine(start.Span, end.Span));
        }

        while (true)
        {
            var value = ParseValue();
            items.Add(value);

            switch (Current.Type)
            {
                case TokenType.Comma:
                    Eat(TokenType.Comma);
                    continue;
                case TokenType.RightBracket:
                {
                    var end = Eat(TokenType.RightBracket);
                    return new JsonArray(items, Combine(start.Span, end.Span));
                }
                default:
                    throw Error("Expected ',' or ']' in array");
            }
        }
    }

    private JsonObject ParseObject()
    {
        var start = Eat(TokenType.LeftBrace);
        var properties = new List<JsonProperty>();

        if (Current.Type == TokenType.RightBrace)
        {
            var end = Eat(TokenType.RightBrace);
            return new JsonObject(properties, Combine(start.Span, end.Span));
        }

        while (true)
        {
            var nameToken = Eat(TokenType.String);
            var template = nameToken.Template ??
                           throw new JsonParseException("Property Name is missing", nameToken.Span);

            if (template.HasExpressions)
                throw new JsonParseException("Property Name cannot contain expressions", nameToken.Span);

            var propertyName = template.ToString();
            Eat(TokenType.Colon);
            var value = ParseValue();
            var propertySpan = Combine(nameToken.Span, value.Span);

            properties.Add(new JsonProperty(propertyName, value, propertySpan));

            switch (Current.Type)
            {
                case TokenType.Comma:
                    Eat(TokenType.Comma);
                    continue;
                case TokenType.RightBrace:
                {
                    var end = Eat(TokenType.RightBrace);
                    return new JsonObject(
                        properties, Combine(start.Span, end.Span)
                    );
                }
                default:
                    throw Error("Expected ',' or '}' in object");
            }
        }
    }

    private JsonValue ParseValue()
    {
        return Current.Type switch
        {
            TokenType.LeftBrace => ParseObject(),
            TokenType.LeftBracket => ParseArray(),
            TokenType.String => ParseString(),
            TokenType.Number => ParseNumber(),
            TokenType.True => ParseBoolean(true),
            TokenType.False => ParseBoolean(false),
            TokenType.Null => ParseNull(),
            _ => throw Error($"Unexpected token '{Current.Type}'")
        };
    }

    private void Expect(TokenType type)
    {
        if (Current.Type != type) throw Error($"Expected {type} but found {Current.Type}");

        _position++;
    }

    public static JsonValue Parse(string text)
    {
        var lexer = new JsonLexer(text);
        var tokens = lexer.Lex();

        var parser = new JsonParser(tokens);
        var value = parser.ParseValue();
        parser.Expect(TokenType.EndOfFile);
        return value;
    }
}