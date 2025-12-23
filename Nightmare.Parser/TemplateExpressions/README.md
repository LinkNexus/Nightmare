# Template Expression System

A comprehensive template expression parser and evaluator for the Nightmare REST HTTP Client.

## Overview

This system allows you to embed dynamic expressions within JSON configuration files using `{{ }}` syntax. Expressions are parsed into an Abstract Syntax Tree (AST) and can be evaluated on-demand with custom contexts.

## Features

### Literals
- **Numbers**: `42`, `3.14`, `-10`, `1.5e-10`
- **Strings**: `"hello"`, `'world'`
- **Booleans**: `true`, `false`
- **Null**: `null`

### Identifiers and Variables
Access variables from the evaluation context:
```
{{ base_url }}
{{ user.username }}
```

### Operators

#### Arithmetic
- Addition: `{{ 1 + 2 }}`
- Subtraction: `{{ 10 - 5 }}`
- Multiplication: `{{ 3 * 4 }}`
- Division: `{{ 20 / 4 }}`
- Modulo: `{{ 10 % 3 }}`

#### Comparison
- Equal: `{{ x == 5 }}`
- Not equal: `{{ x != 5 }}`
- Less than: `{{ x < 10 }}`
- Less or equal: `{{ x <= 10 }}`
- Greater than: `{{ x > 10 }}`
- Greater or equal: `{{ x >= 10 }}`

#### Logical
- AND: `{{ true && false }}`
- OR: `{{ true || false }}`
- NOT: `{{ !false }}`

#### String Concatenation
- `{{ "Hello " + "World" }}`
- `{{ "Count: " + count }}`

### Member Access
Access nested properties:
```
{{ user.name }}
{{ config.server.host }}
```

### Index Access
Access array/list elements and dictionary values:
```
{{ items[0] }}
{{ users[2].name }}
{{ config['api-key'] }}
```

### Function Calls
Call registered functions with arguments:
```
{{ prompt('Enter password: ') }}
{{ readFile('data.txt') }}
{{ max(10, 20, 30) }}
```

### Ternary/Conditional
Conditional expressions:
```
{{ isProduction ? prodUrl : devUrl }}
{{ count > 0 ? "Items: " + count : "No items" }}
```

## Operator Precedence

From lowest to highest:
1. Ternary (`?:`)
2. Logical OR (`||`)
3. Logical AND (`&&`)
4. Equality (`==`, `!=`)
5. Relational (`<`, `<=`, `>`, `>=`)
6. Additive (`+`, `-`)
7. Multiplicative (`*`, `/`, `%`)
8. Unary (`!`, `-`)
9. Postfix (`.`, `[]`, `()`)

## Usage Example

```csharp
using Nightmare.Parser.TemplateExpressions;

// Create an evaluation context
var context = new EvaluationContext();

// Set variables
context.SetVariable("base_url", "https://api.example.com");
context.SetVariable("user", new Dictionary<string, object?>
{
    ["username"] = "johndoe",
    ["id"] = 123
});

// Register functions
context.RegisterFunction("prompt", args =>
{
    var message = args.Length > 0 ? args[0]?.ToString() : "Enter value: ";
    Console.Write(message);
    return Console.ReadLine();
});

context.RegisterFunction("readFile", args =>
{
    var path = args[0]?.ToString() ?? throw new Exception("Path required");
    return File.ReadAllText(path);
});

// Parse and evaluate expressions
var expr1 = "{{ base_url + '/users/' + user.id }}";
var result1 = TemplateExpressionEvaluator.Evaluate(expr1, context);
// Result: "https://api.example.com/users/123"

var expr2 = "{{ user.id > 100 ? 'Premium' : 'Standard' }}";
var result2 = TemplateExpressionEvaluator.Evaluate(expr2, context);
// Result: "Premium"
```

## Integration with JSON Parser

The JSON parser already extracts template expressions from strings. To evaluate them:

```csharp
var json = @"{
  ""url"": ""{{ base_url }}/api/users/{{ userId }}"",
  ""timeout"": ""{{ retries * 1000 }}""
}";

var lexer = new JsonLexer(json);
var tokens = lexer.Lex();
var jsonAst = JsonParser.Parse(tokens);

// Extract and evaluate template expressions
var urlNode = (JsonString)((JsonObject)jsonAst).GetProperty("url");
var template = urlNode.Template;

foreach (var segment in template.GetSegments())
{
    if (segment is TemplateExpressionSegment exprSegment)
    {
        var result = TemplateExpressionEvaluator.Evaluate(
            exprSegment.Expression, 
            context
        );
        // Use result to build final string
    }
}
```

## Error Handling

All errors include precise position information:

```csharp
try
{
    var result = TemplateExpressionEvaluator.Evaluate("{{ 1 / 0 }}", context);
}
catch (TemplateExpressionException ex)
{
    Console.WriteLine($"Error at line {ex.Line}, column {ex.Column}: {ex.Message}");
}
```

## Architecture

### Components

1. **TemplateExpressionToken** - Token types for the lexer
2. **TemplateExpressionLexer** - Tokenizes expression strings
3. **TemplateExpression** - AST node classes representing parsed expressions
4. **TemplateExpressionParser** - Builds AST from tokens with proper precedence
5. **TemplateExpressionEvaluator** - Executes AST with provided context
6. **TemplateExpressionException** - Error reporting with position info

### Design Principles

- **Lazy Evaluation**: Expressions are parsed but not evaluated until needed
- **Position Tracking**: Every token and AST node has precise source location
- **Extensibility**: Easy to add custom functions via context
- **Type Flexibility**: Dynamic typing with automatic conversions
- **Error Recovery**: Clear error messages with exact error locations

## Extending with Custom Functions

```csharp
// File operations
context.RegisterFunction("filePrompt", args =>
{
    // Show file picker dialog
    return PickFile();
});

// HTTP operations  
context.RegisterFunction("env", args =>
{
    var name = args[0]?.ToString() ?? "";
    return Environment.GetEnvironmentVariable(name);
});

// Math operations
context.RegisterFunction("round", args =>
{
    var num = Convert.ToDouble(args[0]);
    var decimals = args.Length > 1 ? Convert.ToInt32(args[1]) : 0;
    return Math.Round(num, decimals);
});
```

## Future Enhancements

- Array/object literal syntax: `[1, 2, 3]`, `{key: value}`
- Spread operator: `...array`
- Optional chaining: `obj?.prop?.nested`
- Null coalescing: `value ?? default`
- String interpolation helpers
- Date/time operations
- Regular expressions
