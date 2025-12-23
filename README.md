# Nightmare - AOT-Compiled REST HTTP Client

A native-compiled REST HTTP client with a custom JSON parser and expression language, designed for Terminal UIs.

## Features

- ✅ **AOT Compatible**: Native compilation with .NET Native AOT (1.6MB binary)
- ✅ **Custom JSON Parser**: Hand-written parser with precise line/column error reporting
- ✅ **Expression Language**: Support for `{{ }}` template expressions with:
  - Variable interpolation (`{{ base_url }}`, `{{ user.password }}`)
  - Function calls (`{{ prompt('Enter password') }}`, `{{ readFile(path) }}`, `{{ env('VAR') }}`)
  - Arithmetic operations (`{{ 1 + 2 }}`, `{{ price * quantity }}`)
  - String concatenation (`{{ firstName + ' ' + lastName }}`)
  - Comparisons and logical operators
- ✅ **Reflection-Free**: Fully AOT-compatible without runtime reflection
- ✅ **Profile Support**: Multiple environment configurations (dev, prod, etc.)
- ✅ **Request Groups**: Organize requests hierarchically

## Architecture

### Parser Components

1. **JSON Lexer** (`JsonLexer.cs`)
   - Tokenizes JSON with position tracking
   - Detects `{{ }}` expressions in strings
   - Provides line/column information for errors

2. **JSON Parser** (`JsonParser.cs`)
   - Builds AST from tokens
   - Creates `JsonValue` nodes (Object, Array, String, Number, Boolean, Null)
   - Preserves template expressions without evaluation

3. **Expression Lexer** (`ExpressionLexer.cs`)
   - Tokenizes expressions inside `{{ }}`
   - Supports operators, identifiers, literals, function calls

4. **Expression Parser** (`ExpressionParser.cs`)
   - Builds expression AST
   - Precedence climbing for operators
   - Supports member access, indexing, function calls

5. **Expression Interpreter** (`ExpressionInterpreter.cs`)
   - Evaluates expression AST using visitor pattern
   - AOT-compatible (no reflection in hot paths)
   - Extensible function provider system

## Configuration Format

```json
{
  "profiles": {
    "dev": {
      "default": true,
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
    "auth": {
      "requests": {
        "login": {
          "url": "{{ base_url }}/post",
          "method": "POST",
          "headers": {
            "Accept": "application/json"
          },
          "body": "{{ user.username + ':' + user.password }}"
        }
      }
    }
  }
}
```

## Expression Syntax

### Variables
```
{{ base_url }}
{{ user.password }}
{{ data[0] }}
```

### Functions
```
{{ prompt('Enter value: ') }}
{{ filePrompt() }}
{{ readFile(path) }}
{{ env('HOME') }}
{{ uuid() }}
{{ timestamp() }}
{{ base64Encode(text) }}
{{ urlEncode(text) }}
```

### Operators
```
{{ 1 + 2 }}
{{ price * 1.1 }}
{{ firstName + ' ' + lastName }}
{{ age >= 18 }}
{{ isAdmin && isActive }}
```

## Building

### Debug Build
```bash
dotnet build
```

### Release Build
```bash
dotnet build -c Release
```

### AOT Compilation
```bash
dotnet publish -c Release -r linux-x64
# Binary output: Nightmare/bin/Release/net10.0/linux-x64/publish/Nightmare
```

## Running

```bash
# With .NET runtime
dotnet run --project Nightmare example-config.json

# AOT compiled binary
./Nightmare/bin/Release/net10.0/linux-x64/publish/Nightmare example-config.json
```

## Project Structure

```
Nightmare/
├── Nightmare.Parser/          # Parser library (AOT-compatible)
│   ├── JsonLexer.cs          # JSON tokenizer
│   ├── JsonParser.cs         # JSON parser
│   ├── JsonNode.cs           # JSON AST nodes
│   ├── ExpressionLexer.cs    # Expression tokenizer
│   ├── ExpressionParser.cs   # Expression parser
│   ├── ExpressionAst.cs      # Expression AST nodes
│   ├── ExpressionInterpreter.cs # Expression evaluator
│   ├── FunctionProvider.cs   # Built-in functions
│   ├── HttpConfigLoader.cs   # Config loader
│   └── TemplateString.cs     # Template string support
├── Nightmare/                 # Main application
│   └── Program.cs            # Console application
├── Nightmare.Tests/           # Unit tests
└── example-config.json        # Example configuration
```

## AOT Compatibility Notes

This project is designed to be fully AOT-compatible:

- ✅ No `System.Text.Json` (uses custom parser)
- ✅ No reflection in hot paths
- ✅ No dynamic code generation
- ✅ All types known at compile time
- ✅ Visitor pattern for extensibility
- ✅ Dictionary-based member access

## Future Enhancements

- [ ] Terminal.Gui UI (prepared in `NightmareApp.cs.future`)
- [ ] HTTP request execution
- [ ] Response display and formatting
- [ ] Request history
- [ ] Environment variable substitution
- [ ] File upload support
- [ ] Cookie management
- [ ] Authentication flows

## License

See LICENSE file.
