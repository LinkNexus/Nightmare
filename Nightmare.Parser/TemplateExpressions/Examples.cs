using Nightmare.Parser.TemplateExpressions;

namespace Nightmare.Parser.Examples;

/// <summary>
/// Demonstrates the Template Expression System with practical examples
/// </summary>
public static class TemplateExpressionExamples
{
    /// <summary>
    /// Basic arithmetic operations
    /// </summary>
    public static void ArithmeticExamples()
    {
        Console.WriteLine("=== Arithmetic Operations ===");

        var context = new EvaluationContext();
        context.SetVariable("price", 100.0);
        context.SetVariable("tax", 0.15);
        context.SetVariable("quantity", 3);

        var examples = new[]
        {
            ("Simple addition", "{{ 10 + 5 }}"),
            ("Multiplication", "{{ 6 * 7 }}"),
            ("Division", "{{ 100 / 4 }}"),
            ("Modulo", "{{ 17 % 5 }}"),
            ("Complex expression", "{{ (10 + 5) * 2 - 3 }}"),
            ("Calculate total", "{{ price * quantity * (1 + tax) }}"),
        };

        foreach (var (description, expression) in examples)
        {
            var result = TemplateExpressionEvaluator.Evaluate(expression, context);
            Console.WriteLine($"{description}: {expression} = {result}");
        }
        Console.WriteLine();
    }

    /// <summary>
    /// String concatenation and manipulation
    /// </summary>
    public static void StringExamples()
    {
        Console.WriteLine("=== String Operations ===");

        var context = new EvaluationContext();
        context.SetVariable("firstName", "John");
        context.SetVariable("lastName", "Doe");
        context.SetVariable("age", 30);
        context.SetVariable("baseUrl", "https://api.example.com");
        context.SetVariable("endpoint", "/users");

        var examples = new[]
        {
            ("Concatenation", "{{ 'Hello ' + 'World' }}"),
            ("Variable concat", "{{ firstName + ' ' + lastName }}"),
            ("Mixed types", "{{ 'Age: ' + age }}"),
            ("URL building", "{{ baseUrl + endpoint }}"),
            ("Complex URL", "{{ baseUrl + endpoint + '/' + age }}"),
        };

        foreach (var (description, expression) in examples)
        {
            var result = TemplateExpressionEvaluator.Evaluate(expression, context);
            Console.WriteLine($"{description}: {expression}");
            Console.WriteLine($"  Result: {result}");
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Comparison and logical operations
    /// </summary>
    public static void ComparisonExamples()
    {
        Console.WriteLine("=== Comparison & Logical Operations ===");

        var context = new EvaluationContext();
        context.SetVariable("score", 85);
        context.SetVariable("passing", 60);
        context.SetVariable("isActive", true);
        context.SetVariable("isAdmin", false);

        var examples = new[]
        {
            ("Equality", "{{ 5 == 5 }}"),
            ("Inequality", "{{ 10 != 5 }}"),
            ("Greater than", "{{ score > passing }}"),
            ("Less or equal", "{{ score <= 100 }}"),
            ("Logical AND", "{{ isActive && isAdmin }}"),
            ("Logical OR", "{{ isActive || isAdmin }}"),
            ("Logical NOT", "{{ !isAdmin }}"),
            ("Complex logic", "{{ (score > passing) && isActive }}"),
        };

        foreach (var (description, expression) in examples)
        {
            var result = TemplateExpressionEvaluator.Evaluate(expression, context);
            Console.WriteLine($"{description}: {expression} = {result}");
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Conditional (ternary) expressions
    /// </summary>
    public static void ConditionalExamples()
    {
        Console.WriteLine("=== Conditional Expressions ===");

        var context = new EvaluationContext();
        context.SetVariable("temperature", 25);
        context.SetVariable("isMember", true);
        context.SetVariable("points", 150);

        var examples = new[]
        {
            ("Simple ternary", "{{ true ? 'yes' : 'no' }}"),
            ("Number check", "{{ temperature > 20 ? 'warm' : 'cold' }}"),
            ("Membership", "{{ isMember ? 'Welcome back!' : 'Please sign up' }}"),
            ("Nested ternary", "{{ points > 200 ? 'Gold' : (points > 100 ? 'Silver' : 'Bronze') }}"),
            ("With calculation", "{{ isMember ? points * 2 : points }}"),
        };

        foreach (var (description, expression) in examples)
        {
            var result = TemplateExpressionEvaluator.Evaluate(expression, context);
            Console.WriteLine($"{description}: {expression}");
            Console.WriteLine($"  Result: {result}");
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Member access and nested objects
    /// </summary>
    public static void MemberAccessExamples()
    {
        Console.WriteLine("=== Member Access ===");

        var context = new EvaluationContext();

        var user = new Dictionary<string, object?>
        {
            ["username"] = "johndoe",
            ["email"] = "john@example.com",
            ["profile"] = new Dictionary<string, object?>
            {
                ["firstName"] = "John",
                ["lastName"] = "Doe",
                ["age"] = 30
            }
        };

        var config = new Dictionary<string, object?>
        {
            ["server"] = new Dictionary<string, object?>
            {
                ["host"] = "api.example.com",
                ["port"] = 8080
            }
        };

        context.SetVariable("user", user);
        context.SetVariable("config", config);

        var examples = new[]
        {
            ("Simple member", "{{ user.username }}"),
            ("Nested member", "{{ user.profile.firstName }}"),
            ("Deep nesting", "{{ config.server.host }}"),
            ("Combined", "{{ user.profile.firstName + ' ' + user.profile.lastName }}"),
            ("URL from config", "{{ 'https://' + config.server.host + ':' + config.server.port }}"),
        };

        foreach (var (description, expression) in examples)
        {
            var result = TemplateExpressionEvaluator.Evaluate(expression, context);
            Console.WriteLine($"{description}: {expression}");
            Console.WriteLine($"  Result: {result}");
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Array/list index access
    /// </summary>
    public static void IndexAccessExamples()
    {
        Console.WriteLine("=== Index Access ===");

        var context = new EvaluationContext();

        context.SetVariable("items", new List<object?> { "apple", "banana", "cherry" });
        context.SetVariable("numbers", new List<object?> { 10, 20, 30, 40 });

        var users = new List<object?>
        {
            new Dictionary<string, object?> { ["name"] = "Alice", ["age"] = 25 },
            new Dictionary<string, object?> { ["name"] = "Bob", ["age"] = 30 },
            new Dictionary<string, object?> { ["name"] = "Charlie", ["age"] = 35 }
        };
        context.SetVariable("users", users);

        var examples = new[]
        {
            ("First item", "{{ items[0] }}"),
            ("Last item", "{{ items[2] }}"),
            ("Numeric array", "{{ numbers[1] }}"),
            ("Array math", "{{ numbers[0] + numbers[1] }}"),
            ("Nested access", "{{ users[1].name }}"),
            ("Complex", "{{ users[0].name + ' is ' + users[0].age + ' years old' }}"),
        };

        foreach (var (description, expression) in examples)
        {
            var result = TemplateExpressionEvaluator.Evaluate(expression, context);
            Console.WriteLine($"{description}: {expression}");
            Console.WriteLine($"  Result: {result}");
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Function calls
    /// </summary>
    public static void FunctionCallExamples()
    {
        Console.WriteLine("=== Function Calls ===");

        var context = new EvaluationContext();
        context.SetVariable("name", "World");

        // Register custom functions
        context.RegisterFunction("upper", args =>
        {
            var str = args[0]?.ToString() ?? "";
            return str.ToUpper();
        });

        context.RegisterFunction("lower", args =>
        {
            var str = args[0]?.ToString() ?? "";
            return str.ToLower();
        });

        context.RegisterFunction("concat", args =>
        {
            return string.Join("", args.Select(a => a?.ToString() ?? ""));
        });

        context.RegisterFunction("max", args =>
        {
            return args.Select(a => Convert.ToDouble(a)).Max();
        });

        context.RegisterFunction("min", args =>
        {
            return args.Select(a => Convert.ToDouble(a)).Min();
        });

        context.RegisterFunction("len", args =>
        {
            var str = args[0]?.ToString() ?? "";
            return (double)str.Length;
        });

        var examples = new[]
        {
            ("Upper case", "{{ upper('hello') }}"),
            ("Lower case", "{{ lower('WORLD') }}"),
            ("Variable to upper", "{{ upper(name) }}"),
            ("Concatenate", "{{ concat('Hello', ' ', 'World', '!') }}"),
            ("Maximum", "{{ max(10, 25, 15, 30, 5) }}"),
            ("Minimum", "{{ min(10, 25, 15, 30, 5) }}"),
            ("String length", "{{ len('Hello World') }}"),
            ("Nested calls", "{{ upper(concat('hello', ' ', 'world')) }}"),
        };

        foreach (var (description, expression) in examples)
        {
            var result = TemplateExpressionEvaluator.Evaluate(expression, context);
            Console.WriteLine($"{description}: {expression}");
            Console.WriteLine($"  Result: {result}");
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Real-world REST API configuration example
    /// </summary>
    public static void RestApiConfigExample()
    {
        Console.WriteLine("=== REST API Configuration Example ===");

        var context = new EvaluationContext();

        // Profile configuration
        context.SetVariable("base_url", "https://httpbin.org");
        context.SetVariable("api_version", "v1");
        context.SetVariable("user", new Dictionary<string, object?>
        {
            ["username"] = "johndoe",
            ["id"] = 123
        });
        context.SetVariable("isProduction", false);

        // Register utility functions
        context.RegisterFunction("env", args =>
        {
            var name = args[0]?.ToString() ?? "";
            // In real scenario, would read from environment
            return name == "API_KEY" ? "secret_key_12345" : null;
        });

        context.RegisterFunction("timestamp", args =>
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        });

        context.RegisterFunction("uuid", args =>
        {
            return Guid.NewGuid().ToString();
        });

        var examples = new[]
        {
            ("Build API URL", "{{ base_url + '/api/' + api_version }}"),
            ("User endpoint", "{{ base_url + '/users/' + user.id }}"),
            ("Environment variable", "{{ env('API_KEY') }}"),
            ("Conditional URL", "{{ isProduction ? 'https://api.prod.com' : base_url }}"),
            ("Auth header", "{{ 'Bearer ' + env('API_KEY') }}"),
            ("Request ID", "{{ 'req_' + timestamp() }}"),
            ("Correlation ID", "{{ uuid() }}"),
            ("Dynamic route", "{{ base_url + '/users/' + user.id + '/posts' }}"),
        };

        foreach (var (description, expression) in examples)
        {
            var result = TemplateExpressionEvaluator.Evaluate(expression, context);
            Console.WriteLine($"{description}: {expression}");
            Console.WriteLine($"  Result: {result}");
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Run all examples
    /// </summary>
    public static void RunAll()
    {
        ArithmeticExamples();
        StringExamples();
        ComparisonExamples();
        ConditionalExamples();
        MemberAccessExamples();
        IndexAccessExamples();
        FunctionCallExamples();
        RestApiConfigExample();
    }
}
