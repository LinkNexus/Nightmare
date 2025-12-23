# Quick Start Guide - Template Expression System

## What You Have Now

A complete expression parser and evaluator that works seamlessly with your existing JSON parser.

## 5-Minute Quick Start

### 1. Basic Expression Evaluation

```csharp
using Nightmare.Parser.TemplateExpressions;

// Create context
var context = new EvaluationContext();
context.SetVariable("name", "John");
context.SetVariable("age", 30);

// Evaluate expression
var result = TemplateExpressionEvaluator.Evaluate("{{ name + ' is ' + age }}", context);
// Result: "John is 30"
```

### 2. With Your JSON Config

```csharp
// Your JSON config
var json = @"{
  ""url"": ""{{ base_url }}/users/{{ userId }}"",
  ""timeout"": ""{{ retries * 1000 }}""
}";

// Parse JSON
var ast = JsonParser.Parse(json);

// Setup context
var context = new EvaluationContext();
context.SetVariable("base_url", "https://api.example.com");
context.SetVariable("userId", 123);
context.SetVariable("retries", 3);

// Extract and evaluate
var obj = (JsonObject)ast;
var urlString = (JsonString)obj.GetProperty("url")!;
var finalUrl = TemplateStringEvaluator.Evaluate(urlString.Template, context);
// Result: "https://api.example.com/users/123"
```

### 3. Register Custom Functions

```csharp
var context = new EvaluationContext();

// Simple function
context.RegisterFunction("upper", args => 
    args[0]?.ToString().ToUpper()
);

// Function with file I/O
context.RegisterFunction("readFile", args => {
    var path = args[0]?.ToString() ?? throw new Exception("Path required");
    return File.ReadAllText(path);
});

// Interactive function
context.RegisterFunction("prompt", args => {
    Console.Write(args[0]?.ToString() ?? "Enter value: ");
    return Console.ReadLine();
});

// Use them
var result = TemplateExpressionEvaluator.Evaluate(
    "{{ upper('hello') }}", 
    context
);
// Result: "HELLO"
```

## Common Use Cases for Your REST Client

### Use Case 1: Dynamic URLs

```csharp
// Profile variables
context.SetVariable("base_url", "https://httpbin.org");
context.SetVariable("api_version", "v1");

// Expression: {{ base_url }}/{{ api_version }}/users
// Result: https://httpbin.org/v1/users
```

### Use Case 2: Authentication Headers

```csharp
// Profile variables
context.SetVariable("token", "eyJhbGci...");

// Expression: {{ 'Bearer ' + token }}
// Result: Bearer eyJhbGci...
```

### Use Case 3: Conditional Configuration

```csharp
context.SetVariable("env", "production");

// Expression: {{ env == 'production' ? 'https://api.prod.com' : 'http://localhost:3000' }}
// Result: https://api.prod.com
```

### Use Case 4: File Upload Paths

```csharp
context.RegisterFunction("filePrompt", args => {
    // Show file picker dialog
    return "/path/to/selected/file.jpg";
});

// Expression: {{ filePrompt() }}
// Result: /path/to/selected/file.jpg
```

### Use Case 5: Complex Request Bodies

```csharp
// For AOT compatibility, use Dictionary<string, object?>
context.SetVariable("user", new Dictionary<string, object?> {
    ["name"] = "John",
    ["email"] = "john@example.com"
});

// Expression: {{ 'Name: ' + user.name + ', Email: ' + user.email }}
// Result: Name: John, Email: john@example.com
```

## Important: AOT Compatibility

This system is **AOT compatible** and does not use reflection. All data structures must use `Dictionary<string, object?>` for nested objects:

```csharp
// ✅ Correct - AOT compatible
var user = new Dictionary<string, object?> {
    ["profile"] = new Dictionary<string, object?> {
        ["name"] = "John"
    }
};

// ❌ Incorrect - will not work (no reflection support)
var user = new { profile = new { name = "John" } };
```

## Integration with Your Config File

Your config file structure:
```json
{
  "profiles": {
    "dev": {
      "data": {
        "base_url": "https://httpbin.org",
        "user": {
          "username": "johndoe"
        }
      }
    }
  },
  "requests": {
    "login": {
      "url": "{{ base_url }}/post",
      "body": "{{ user.username }}"
    }
  }
}
```

How to use it:
```csharp
// 1. Parse config
var config = JsonParser.Parse(File.ReadAllText("config.json"));

// 2. Extract profile data
var profiles = (JsonObject)((JsonObject)config).GetProperty("profiles")!;
var dev = (JsonObject)profiles.GetProperty("dev")!;
var data = (JsonObject)dev.GetProperty("data")!;

// 3. Build context from profile
var context = new EvaluationContext();
foreach (var prop in data.Properties)
{
    context.SetVariable(prop.Key, ConvertJsonValue(prop.Value));
}

// 4. Get request
var requests = (JsonObject)((JsonObject)config).GetProperty("requests")!;
var login = (JsonObject)requests.GetProperty("login")!;

// 5. Evaluate URL
var urlString = (JsonString)login.GetProperty("url")!;
var finalUrl = TemplateStringEvaluator.Evaluate(urlString.Template, context);
```

## Essential Functions to Implement

For your REST client, you'll want these functions:

```csharp
// User Input
context.RegisterFunction("prompt", args => {
    var message = args[0]?.ToString() ?? "Enter value: ";
    // Show TUI input dialog
    return GetUserInput(message);
});

context.RegisterFunction("filePrompt", args => {
    // Show TUI file picker
    return ShowFilePicker();
});

// File Operations
context.RegisterFunction("readFile", args => {
    var path = args[0]?.ToString() ?? throw new Exception("Path required");
    return File.ReadAllText(path);
});

context.RegisterFunction("base64", args => {
    var input = args[0]?.ToString() ?? "";
    var bytes = System.Text.Encoding.UTF8.GetBytes(input);
    return Convert.ToBase64String(bytes);
});

// Environment
context.RegisterFunction("env", args => {
    var name = args[0]?.ToString() ?? "";
    return Environment.GetEnvironmentVariable(name);
});

// Utilities
context.RegisterFunction("uuid", args => 
    Guid.NewGuid().ToString()
);

context.RegisterFunction("timestamp", args => 
    DateTimeOffset.UtcNow.ToUnixTimeSeconds()
);

context.RegisterFunction("now", args => 
    DateTime.UtcNow.ToString("O")
);
```

## Error Handling

```csharp
try
{
    var result = TemplateStringEvaluator.Evaluate(template, context);
}
catch (TemplateExpressionException ex)
{
    Console.WriteLine($"Error at line {ex.Line}, column {ex.Column}:");
    Console.WriteLine(ex.Message);
    // Show error in TUI with exact position
}
```

## Validation Before Execution

```csharp
// Validate all templates before running request
try
{
    TemplateStringEvaluator.ValidateSyntax(urlTemplate);
    TemplateStringEvaluator.ValidateSyntax(bodyTemplate);
    // All good, proceed with request
}
catch (TemplateExpressionException ex)
{
    // Show syntax error to user
    ShowError($"Invalid expression at {ex.Line}:{ex.Column} - {ex.Message}");
    return;
}
```

## Next Steps

1. **Test the examples**: Run `Examples.cs` and `IntegrationExample.cs`
2. **Implement built-in functions**: Add the functions your TUI will need
3. **Integrate with your request execution**: Evaluate templates when making HTTP requests
4. **Add error handling**: Show clear error messages in your TUI
5. **Write tests**: Create unit tests for your specific use cases

## Running Examples

To see it in action:

```csharp
// In your Program.cs or a test file
using Nightmare.Parser.Examples;

// Run all basic examples
TemplateExpressionExamples.RunAll();

// Run integration example
IntegrationExample.RunExample();

// Run advanced example
IntegrationExample.RunAdvancedExample();
```

## Summary

✅ Parse expressions: `TemplateExpressionParser.Parse(expr)`  
✅ Evaluate expressions: `TemplateExpressionEvaluator.Evaluate(expr, context)`  
✅ Integrate with JSON: `TemplateStringEvaluator.Evaluate(template, context)`  
✅ Add functions: `context.RegisterFunction(name, func)`  
✅ Set variables: `context.SetVariable(name, value)`  

**You're ready to go!** 🚀
