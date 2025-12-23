# Nightmare HTTP Client - Technical Overview

## What We Built

An AOT-compiled REST HTTP client with a custom JSON parser and expression language, designed to be:

1. **Native-compiled** - Compiles to a 1.6MB native binary with .NET Native AOT
2. **Reflection-free** - No runtime reflection for full AOT compatibility
3. **Custom parser** - Hand-written JSON parser with precise error reporting
4. **Expression language** - Template expressions with {{ }} syntax

## Key Components

### 1. JSON Lexer & Parser

**JsonLexer.cs** - Tokenizes JSON with position tracking
```csharp
// Tracks line/column for every token
// Detects {{ }} expressions in strings
// Example error output:
// "❌ JSON Parse Error at line 7, column 9: Expected ',' or '}' in object"
```

**JsonParser.cs** - Builds AST from tokens
```csharp
// Creates JsonValue hierarchy:
// - JsonObject (with dictionary of properties)
// - JsonArray (with list of items)
// - JsonString (with TemplateString containing {{ }} expressions)
// - JsonNumber, JsonBoolean, JsonNull
```

### 2. Expression Language

**ExpressionLexer.cs** - Tokenizes expressions
```csharp
// Supports:
// - Literals: "string", 123, true, false, null
// - Operators: +, -, *, /, %, ==, !=, <, <=, >, >=, &&, ||, !
// - Identifiers: variable, user.name, data[0]
// - Function calls: prompt('Enter: '), uuid(), env('VAR')
```

**ExpressionParser.cs** - Builds expression AST
```csharp
// Expression types:
// - LiteralExpression
// - VariableExpression
// - BinaryExpression (with operator precedence)
// - UnaryExpression
// - CallExpression (function calls)
// - MemberAccessExpression (obj.member)
// - IndexAccessExpression (obj[index])
```

**ExpressionInterpreter.cs** - Evaluates expressions
```csharp
// Uses visitor pattern for extensibility
// AOT-compatible (no reflection in hot paths)
// Type coercion for operations
```

### 3. Function System

**BuiltInFunctionProvider.cs** - String and utility functions
```
toString, upper, lower, trim, concat, length
substring, replace, split, join
indexOf, contains, startsWith, endsWith
```

**HttpConfigFunctionProvider.cs** - HTTP-specific functions
```
prompt       - User input prompting
filePrompt   - File selection prompt
readFile     - Read file contents
env          - Environment variables
uuid         - Generate UUID
timestamp    - Unix timestamp or formatted date
base64Encode - Base64 encoding
base64Decode - Base64 decoding
urlEncode    - URL encoding
urlDecode    - URL decoding
jsonParse    - Parse JSON string
jsonStringify - Stringify to JSON
```

## Expression Examples

### Variable Access
```json
{
  "url": "{{ base_url }}/api/v1/users"
}
```

### Nested Properties
```json
{
  "header": "{{ user.profile.name }}"
}
```

### Arithmetic
```json
{
  "total": "{{ price * quantity + shipping }}"
}
```

### String Concatenation
```json
{
  "auth": "{{ 'Bearer ' + token }}"
}
```

### Function Calls
```json
{
  "password": "{{ prompt('Enter password: ') }}",
  "id": "{{ uuid() }}",
  "timestamp": "{{ timestamp() }}",
  "encoded": "{{ base64Encode(readFile('data.txt')) }}"
}
```

### Conditionals and Comparisons
```json
{
  "premium": "{{ age >= 18 && hasSubscription }}",
  "discount": "{{ total > 100 ? 0.1 : 0 }}"
}
```

## Configuration Structure

### Profiles
Define environment-specific variables:

```json
{
  "profiles": {
    "dev": {
      "default": true,
      "data": {
        "base_url": "https://dev.api.example.com",
        "api_key": "{{ env('DEV_API_KEY') }}"
      }
    },
    "prod": {
      "data": {
        "base_url": "https://api.example.com",
        "api_key": "{{ env('PROD_API_KEY') }}"
      }
    }
  }
}
```

### Request Groups
Organize related requests:

```json
{
  "requests": {
    "auth": {
      "requests": {
        "login": {
          "url": "{{ base_url }}/auth/login",
          "method": "POST",
          "headers": {
            "Content-Type": "application/json"
          },
          "body": {
            "username": "{{ prompt('Username: ') }}",
            "password": "{{ prompt('Password: ') }}"
          }
        }
      }
    }
  }
}
```

### Flat Requests
Simple requests at root level:

```json
{
  "requests": {
    "ping": {
      "url": "{{ base_url }}/health",
      "method": "GET"
    }
  }
}
```

## AOT Compatibility

### What We Avoid
- ❌ System.Text.Json (uses reflection)
- ❌ Reflection in hot paths
- ❌ Dynamic code generation
- ❌ Expression trees
- ❌ Runtime type inspection

### What We Use Instead
- ✅ Custom parser with manual tokenization
- ✅ Visitor pattern for extensibility
- ✅ Dictionary-based member access
- ✅ Static type dispatch
- ✅ Compile-time known types

### Member Access Strategy
```csharp
// Instead of reflection:
// var property = type.GetProperty(name);
// var value = property.GetValue(obj);

// We use:
if (obj is IDictionary<string, object?> dict)
    return dict[memberName];
if (obj is JsonObject jsonObj)
    return jsonObj.GetProperty(memberName);
```

## Performance Characteristics

### Binary Size
- AOT compiled: ~1.6 MB
- Self-contained with no runtime dependencies

### Startup Time
- Near-instant (native binary)
- No JIT compilation overhead

### Memory Usage
- Lower than JIT (no IL metadata)
- Predictable allocation patterns

### Parsing Performance
- Hand-written lexer: faster than regex-based
- Single-pass parsing
- Lazy evaluation of expressions

## Testing

143 unit tests covering:
- JSON lexing and parsing
- Expression parsing and evaluation
- Template string handling
- Error reporting
- Edge cases

Run tests:
```bash
dotnet test
```

## Usage Examples

### Load and parse config
```bash
dotnet run --project Nightmare example-config.json
```

### Run expression demo
```bash
dotnet run --project Nightmare --demo
```

### Test error reporting
```bash
dotnet run --project Nightmare test-error.json
```

### AOT compilation
```bash
dotnet publish -c Release -r linux-x64
./Nightmare/bin/Release/net10.0/linux-x64/publish/Nightmare example-config.json
```

## Future Roadmap

### Phase 1: Core (✅ Complete)
- [x] JSON lexer and parser
- [x] Expression language
- [x] Configuration model
- [x] AOT compatibility

### Phase 2: HTTP Client (Planned)
- [ ] HTTP request execution
- [ ] Response handling
- [ ] Cookie management
- [ ] Authentication flows

### Phase 3: Terminal UI (Prepared)
- [ ] Terminal.Gui integration
- [ ] Request browser
- [ ] Response viewer
- [ ] Interactive prompts

### Phase 4: Advanced Features (Future)
- [ ] Request history
- [ ] Response caching
- [ ] Scripting support
- [ ] Plugin system

## Design Principles

1. **AOT First** - Every feature designed for native compilation
2. **Error Clarity** - Precise line/column error reporting
3. **Extensibility** - Plugin-based function system
4. **Simplicity** - Simple enough for the use case, not battle-tested complexity
5. **Performance** - Native speed, low memory footprint

## Conclusion

Nightmare demonstrates that complex parsing and expression evaluation can be achieved in an AOT-compatible manner without sacrificing features or performance. The custom JSON parser with embedded expression language provides a powerful configuration system while maintaining full native compilation support.
