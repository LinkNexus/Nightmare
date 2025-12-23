# Extending Nightmare

## Adding Custom Functions

The function system is designed to be easily extensible. Here's how to add your own functions:

### Step 1: Create a Custom Function Provider

```csharp
using Nightmare.Parser;

public class MyCustomFunctionProvider : HttpConfigFunctionProvider
{
    public MyCustomFunctionProvider()
    {
        // Add your custom functions
        RegisterFunction("reverse", ReverseFunc);
        RegisterFunction("repeat", RepeatFunc);
        RegisterFunction("random", RandomFunc);
    }

    private static object? ReverseFunc(object?[] args, IExpressionContext context)
    {
        if (args.Length != 1)
            throw new ExpressionEvaluationException("reverse requires 1 argument");

        var str = args[0]?.ToString() ?? "";
        return new string(str.Reverse().ToArray());
    }

    private static object? RepeatFunc(object?[] args, IExpressionContext context)
    {
        if (args.Length != 2)
            throw new ExpressionEvaluationException("repeat requires 2 arguments");

        var str = args[0]?.ToString() ?? "";
        var count = Convert.ToInt32(args[1]);
        
        return string.Concat(Enumerable.Repeat(str, count));
    }

    private static object? RandomFunc(object?[] args, IExpressionContext context)
    {
        if (args.Length == 0)
            return Random.Shared.NextDouble();
        
        if (args.Length == 2)
        {
            var min = Convert.ToInt32(args[0]);
            var max = Convert.ToInt32(args[1]);
            return (double)Random.Shared.Next(min, max);
        }

        throw new ExpressionEvaluationException("random requires 0 or 2 arguments");
    }
}
```

### Step 2: Use Your Custom Provider

```csharp
var context = new ExpressionContext();
var functionProvider = new MyCustomFunctionProvider();
var evaluator = new ExpressionEvaluator(context, functionProvider);
```

### Step 3: Use in Configuration

```json
{
  "requests": {
    "test": {
      "url": "{{ base_url }}/{{ reverse('tset') }}",
      "headers": {
        "X-Request-ID": "{{ repeat('a', 10) }}",
        "X-Random": "{{ random(1, 100) }}"
      }
    }
  }
}
```

## Adding Custom AST Nodes

To add new expression types, extend the AST:

### Step 1: Create New Expression Type

```csharp
public class TernaryExpression : Expression
{
    public Expression Condition { get; }
    public Expression TrueExpr { get; }
    public Expression FalseExpr { get; }

    public TernaryExpression(Expression condition, Expression trueExpr, Expression falseExpr)
    {
        Condition = condition;
        TrueExpr = trueExpr;
        FalseExpr = falseExpr;
    }

    public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.VisitTernary(this);
}
```

### Step 2: Update Visitor Interface

```csharp
public interface IExpressionVisitor<T>
{
    // ... existing methods ...
    T VisitTernary(TernaryExpression expr);
}
```

### Step 3: Update Interpreter

```csharp
public object? VisitTernary(TernaryExpression expr)
{
    var condition = Evaluate(expr.Condition);
    return ToBoolean(condition) ? Evaluate(expr.TrueExpr) : Evaluate(expr.FalseExpr);
}
```

### Step 4: Update Parser

```csharp
private Expression ParseTernary()
{
    var expr = ParseLogicalOr();

    if (Match(ExpressionTokenType.Question))
    {
        var trueExpr = ParseLogicalOr();
        Consume(ExpressionTokenType.Colon, "Expected ':' in ternary expression");
        var falseExpr = ParseTernary();
        return new TernaryExpression(expr, trueExpr, falseExpr);
    }

    return expr;
}
```

## Adding Context Variables

You can add custom context variables that are accessible in expressions:

```csharp
var context = new ExpressionContext();

// Simple values
context.SetValue("api_version", "v2");
context.SetValue("timeout", 30.0);

// Nested objects
context.SetValue("config", new Dictionary<string, object?>
{
    ["retry_count"] = 3.0,
    ["backoff_ms"] = 1000.0
});

// Complex data structures
context.SetValue("endpoints", new Dictionary<string, object?>
{
    ["users"] = "/api/users",
    ["posts"] = "/api/posts",
    ["comments"] = "/api/comments"
});
```

Then use in expressions:

```json
{
  "url": "{{ base_url + endpoints.users }}",
  "timeout": "{{ config.retry_count * config.backoff_ms }}"
}
```

## Custom JSON Value Types

To support custom data types in JSON:

### Step 1: Create Custom JsonValue

```csharp
public sealed class JsonDate : JsonValue
{
    public DateTime Value { get; }

    public JsonDate(DateTime value, TextSpan span) : base(span)
    {
        Value = value;
    }
}
```

### Step 2: Update Lexer/Parser

Add detection logic in the lexer or parser to recognize your custom format.

### Step 3: Update ConfigLoader

```csharp
private static object? ConvertJsonValue(JsonValue value)
{
    return value switch
    {
        JsonString str => str.Template,
        JsonNumber num => num.Value,
        JsonBoolean b => b.Value,
        JsonNull => null,
        JsonDate date => date.Value,  // Handle custom type
        JsonObject obj => ConvertJsonObject(obj),
        JsonArray arr => arr.Items.Select(ConvertJsonValue).ToList(),
        _ => null
    };
}
```

## Context-Aware Functions

Functions can access and modify the context:

```csharp
private static object? SetVarFunc(object?[] args, IExpressionContext context)
{
    if (args.Length != 2)
        throw new ExpressionEvaluationException("setVar requires 2 arguments");

    var name = args[0]?.ToString() ?? throw new ExpressionEvaluationException("Variable name cannot be null");
    var value = args[1];

    context.SetValue(name, value);
    return value;
}

private static object? GetVarFunc(object?[] args, IExpressionContext context)
{
    if (args.Length != 1)
        throw new ExpressionEvaluationException("getVar requires 1 argument");

    var name = args[0]?.ToString() ?? throw new ExpressionEvaluationException("Variable name cannot be null");
    return context.GetValue(name);
}
```

Usage:

```json
{
  "body": "{{ setVar('token', uuid()) }}"
}
```

## Error Handling

Add custom error messages for better debugging:

```csharp
private static object? ValidateEmailFunc(object?[] args, IExpressionContext context)
{
    if (args.Length != 1)
        throw new ExpressionEvaluationException("validateEmail requires 1 argument");

    var email = args[0]?.ToString() ?? "";
    
    if (!email.Contains('@'))
        throw new ExpressionEvaluationException($"Invalid email format: '{email}' - missing '@' symbol");
    
    if (!email.Contains('.'))
        throw new ExpressionEvaluationException($"Invalid email format: '{email}' - missing domain");

    return true;
}
```

## AOT Compatibility Tips

When extending, maintain AOT compatibility:

### ✅ DO:
- Use dictionaries for dynamic lookups
- Use visitor pattern for extensibility
- Use interfaces and abstract classes
- Pre-register all types and functions
- Use explicit type checking with `is` and `switch`

### ❌ DON'T:
- Use `System.Reflection` for runtime type inspection
- Use `dynamic` keyword
- Use `Activator.CreateInstance`
- Use `Expression.Compile()`
- Use JSON serializers that require reflection

### Example: AOT-Safe Type Handling

```csharp
// ❌ BAD: Uses reflection
var property = obj.GetType().GetProperty(name);
var value = property?.GetValue(obj);

// ✅ GOOD: Uses type checking
if (obj is IDictionary<string, object?> dict)
    return dict.TryGetValue(name, out var value) ? value : null;
else if (obj is JsonObject jsonObj)
    return jsonObj.GetProperty(name);
else
    throw new ExpressionEvaluationException($"Cannot access member '{name}' on object");
```

## Example: Complete Custom Extension

Here's a complete example adding mathematical functions:

```csharp
using Nightmare.Parser;

public class MathFunctionProvider : HttpConfigFunctionProvider
{
    public MathFunctionProvider()
    {
        RegisterFunction("abs", (args, _) => Math.Abs(ToDouble(args[0])));
        RegisterFunction("ceil", (args, _) => Math.Ceiling(ToDouble(args[0])));
        RegisterFunction("floor", (args, _) => Math.Floor(ToDouble(args[0])));
        RegisterFunction("round", (args, _) => Math.Round(ToDouble(args[0])));
        RegisterFunction("sqrt", (args, _) => Math.Sqrt(ToDouble(args[0])));
        RegisterFunction("pow", (args, _) => Math.Pow(ToDouble(args[0]), ToDouble(args[1])));
        RegisterFunction("min", (args, _) => args.Select(ToDouble).Min());
        RegisterFunction("max", (args, _) => args.Select(ToDouble).Max());
        RegisterFunction("sum", (args, _) => args.Select(ToDouble).Sum());
        RegisterFunction("avg", (args, _) => args.Select(ToDouble).Average());
    }

    private static double ToDouble(object? value)
    {
        return value switch
        {
            double d => d,
            int i => i,
            long l => l,
            float f => f,
            string s => double.Parse(s),
            _ => throw new ExpressionEvaluationException($"Cannot convert {value} to number")
        };
    }
}
```

Usage:

```json
{
  "total": "{{ sum(10, 20, 30) }}",
  "average": "{{ avg(prices) }}",
  "rounded": "{{ round(price * 1.15) }}"
}
```

## Testing Your Extensions

Always test your extensions:

```csharp
[Test]
public void TestCustomFunction()
{
    var context = new ExpressionContext();
    var provider = new MyCustomFunctionProvider();
    var evaluator = new ExpressionEvaluator(context, provider);

    var template = new TemplateString(new[]
    {
        new TemplateExpressionSegment("reverse('hello')", new TextSpan(0, 16, 1, 1, 1, 16))
    });

    var result = evaluator.EvaluateTemplate(template);
    Assert.AreEqual("olleh", result);
}
```

This ensures your extensions work correctly and maintain AOT compatibility.
