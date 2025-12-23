# Template Expression System - Implementation Summary

## Overview

A complete template expression parser and evaluator has been created for the Nightmare REST HTTP Client. This system allows dynamic expressions embedded in JSON configuration files using `{{ }}` syntax.

## Created Files

### Core Components (in `Nightmare.Parser/TemplateExpressions/`)

1. **TemplateExpressionToken.cs**
   - Token types for the lexer (operators, literals, keywords, etc.)
   - Similar structure to the existing `Token.cs` but specific to expressions

2. **TemplateExpressionLexer.cs**
   - Tokenizes expression strings into tokens
   - Handles numbers, strings, identifiers, operators, and delimiters
   - Tracks position information for error reporting

3. **TemplateExpression.cs**
   - AST (Abstract Syntax Tree) node classes
   - Node types: Literals, Identifiers, Binary/Unary operators, Member access, Index access, Calls, Conditionals

4. **TemplateExpressionParser.cs**
   - Parses tokens into an AST with proper operator precedence
   - Implements recursive descent parsing
   - Precedence levels: Ternary → OR → AND → Equality → Relational → Additive → Multiplicative → Unary → Postfix

5. **TemplateExpressionException.cs**
   - Exception type with position tracking
   - Consistent with existing `JsonParseException`

6. **TemplateExpressionEvaluator.cs**
   - Evaluates parsed AST with a context
   - Includes `EvaluationContext` for variables and functions
   - Supports dynamic typing with automatic conversions

7. **TemplateStringEvaluator.cs**
   - Integration utility for existing `TemplateString` class
   - Evaluates all expressions in a template
   - Provides validation and variable extraction

### Documentation and Examples

8. **README.md**
   - Comprehensive documentation
   - Feature list and usage examples
   - Integration guide

9. **Examples.cs**
   - Practical examples demonstrating all features
   - Arithmetic, strings, comparisons, conditionals, member access, etc.

10. **IntegrationExample.cs**
    - Real-world integration with JSON parser
    - Shows complete workflow from JSON parsing to expression evaluation

## Key Features

### Supported Operations

✅ **Literals**: numbers, strings, booleans, null  
✅ **Variables**: identifier lookup from context  
✅ **Arithmetic**: +, -, *, /, %  
✅ **Comparison**: ==, !=, <, <=, >, >=  
✅ **Logical**: &&, ||, !  
✅ **String concatenation**: "Hello " + "World"  
✅ **Member access**: obj.prop.nested  
✅ **Index access**: arr[0], dict['key']  
✅ **Function calls**: func(arg1, arg2)  
✅ **Ternary/Conditional**: condition ? then : else  
✅ **Grouped expressions**: (expr)  

### Design Principles

- **Lazy Evaluation**: Expressions are parsed but not evaluated until needed
- **Position Tracking**: Every token and AST node includes source location for precise error reporting
- **Extensibility**: Easy to add custom functions via `EvaluationContext`
- **Type Safety**: Runtime type checking with helpful error messages
- **Consistency**: Follows the same patterns as the existing JSON parser

## Usage Example

```csharp
using Nightmare.Parser.TemplateExpressions;

// 1. Parse JSON with template expressions
var json = @"{
  ""url"": ""{{ base_url }}/users/{{ userId }}"",
  ""auth"": ""Bearer {{ token }}""
}";

var lexer = new JsonLexer(json);
var tokens = lexer.Lex();
var ast = JsonParser.Parse(tokens);

// 2. Create evaluation context
var context = new EvaluationContext();
context.SetVariable("base_url", "https://api.example.com");
context.SetVariable("userId", 123);
context.SetVariable("token", "secret_token");

// 3. Register custom functions
context.RegisterFunction("prompt", args => {
    Console.Write(args[0]?.ToString());
    return Console.ReadLine();
});

context.RegisterFunction("readFile", args => {
    var path = args[0]?.ToString() ?? "";
    return File.ReadAllText(path);
});

// 4. Extract and evaluate template expressions
var obj = (JsonObject)ast;
var urlNode = (JsonString)obj.GetProperty("url")!;
var evaluatedUrl = TemplateStringEvaluator.Evaluate(urlNode.Template, context);
// Result: "https://api.example.com/users/123"
```

## Integration with Existing Code

The template expression system integrates seamlessly with your existing JSON parser:

1. **JSON Lexer** already extracts template expressions as `TemplateExpressionSegment`
2. **TemplateString** contains segments (text + expressions)
3. **New System** parses and evaluates the expression strings when needed

```csharp
// Your existing JSON parsing extracts TemplateString
var template = jsonString.Template;

// New system evaluates it
var result = TemplateStringEvaluator.Evaluate(template, context);
```

## Example from Your Config

```json
{
  "profiles": {
    "dev": {
      "data": {
        "base_url": "https://httpbin.org",
        "user": {
          "username": "johndoe",
          "password": "{{ prompt('Enter password: ') }}"
        }
      }
    }
  },
  "requests": {
    "login": {
      "url": "{{ base_url }}/post",
      "body": {
        "photo": {
          "path": "{{ filePrompt() }}"
        }
      }
    },
    "register": {
      "url": "{{ base_url }}/post",
      "body": "{{ readFile(filePrompt()) + ' ' + user.password }}"
    }
  }
}
```

With the template system, you can:
1. Parse this JSON
2. Extract profile variables into `EvaluationContext`
3. Register functions like `prompt()`, `filePrompt()`, `readFile()`
4. Evaluate expressions when needed (e.g., when making the HTTP request)

## Next Steps

### Recommended Workflow

1. **Parse configuration file** using existing JSON parser
2. **Extract profile variables** into `EvaluationContext`
3. **Register built-in functions** (prompt, readFile, filePrompt, etc.)
4. **Validate all expressions** before runtime using `TemplateStringEvaluator.ValidateSyntax()`
5. **Evaluate expressions** when executing requests

### Built-in Functions to Implement

Suggested functions for your REST client:

```csharp
// User input
context.RegisterFunction("prompt", args => { /* show input dialog */ });
context.RegisterFunction("filePrompt", args => { /* show file picker */ });

// File operations
context.RegisterFunction("readFile", args => { /* read file content */ });
context.RegisterFunction("base64", args => { /* encode to base64 */ });

// Environment
context.RegisterFunction("env", args => { /* get env variable */ });

// Utilities
context.RegisterFunction("uuid", args => Guid.NewGuid().ToString());
context.RegisterFunction("timestamp", args => DateTimeOffset.UtcNow.ToUnixTimeSeconds());
context.RegisterFunction("now", args => DateTime.UtcNow.ToString("O"));

// String utilities
context.RegisterFunction("upper", args => args[0]?.ToString().ToUpper());
context.RegisterFunction("lower", args => args[0]?.ToString().ToLower());
context.RegisterFunction("trim", args => args[0]?.ToString().Trim());
```

### Testing

The system is ready for testing. You can:
1. Run `Examples.cs` to see all features in action
2. Run `IntegrationExample.cs` to see real-world usage
3. Write unit tests similar to your existing parser tests

### Future Enhancements

Consider adding:
- Array/object literal syntax: `[1, 2, 3]`, `{key: value}`
- Spread operator: `...array`
- Optional chaining: `obj?.prop?.nested`
- Null coalescing: `value ?? 'default'`
- Regular expressions
- More built-in functions

## Files Structure

```
Nightmare.Parser/TemplateExpressions/
├── TemplateExpressionToken.cs          # Token definitions
├── TemplateExpressionLexer.cs          # Tokenizer
├── TemplateExpression.cs               # AST nodes
├── TemplateExpressionParser.cs         # Parser (tokens → AST)
├── TemplateExpressionException.cs      # Error handling
├── TemplateExpressionEvaluator.cs      # Evaluator (AST → value)
├── TemplateStringEvaluator.cs          # Integration utility
├── Examples.cs                         # Usage examples
├── IntegrationExample.cs               # Real-world example
└── README.md                           # Documentation
```

## Build Status

✅ Project builds successfully  
✅ No compilation errors  
✅ Compatible with existing code  
✅ Ready for use and testing  

---

**The template expression system is complete and ready to use!** 🎉
