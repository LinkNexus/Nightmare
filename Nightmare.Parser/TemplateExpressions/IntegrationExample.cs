using Nightmare.Parser.TemplateExpressions;

namespace Nightmare.Parser.Examples;

/// <summary>
/// Integration example showing how to use template expressions with the JSON parser
/// </summary>
public static class IntegrationExample
{
    public static void RunExample()
    {
        Console.WriteLine("=== Template Expression Integration Example ===\n");

        // Example JSON configuration (similar to Bruno/Postman)
        var jsonConfig = @"{
  ""profiles"": {
    ""dev"": {
      ""base_url"": ""https://httpbin.org"",
      ""timeout"": ""5000"",
      ""retries"": ""3""
    }
  },
  ""requests"": {
    ""getUser"": {
      ""url"": ""{{ base_url }}/get?user={{ userId }}"",
      ""method"": ""GET"",
      ""timeout"": ""{{ timeout }}"",
      ""headers"": {
        ""Authorization"": ""Bearer {{ token }}"",
        ""X-Request-ID"": ""{{ requestId }}""
      }
    },
    ""createPost"": {
      ""url"": ""{{ base_url }}/post"",
      ""method"": ""POST"",
      ""body"": ""{{ 'User: ' + userName + ', Score: ' + (score * 2) }}""
    }
  }
}";

        // Step 1: Parse the JSON
        Console.WriteLine("1. Parsing JSON configuration...");
        var jsonAst = JsonParser.Parse(jsonConfig);
        Console.WriteLine("   ✓ JSON parsed successfully\n");

        // Step 2: Create evaluation context with variables
        Console.WriteLine("2. Setting up evaluation context...");
        var context = new EvaluationContext();

        // Variables from profile
        context.SetVariable("base_url", "https://httpbin.org");
        context.SetVariable("timeout", 5000);
        context.SetVariable("retries", 3);

        // Request-specific variables
        context.SetVariable("userId", 123);
        context.SetVariable("userName", "JohnDoe");
        context.SetVariable("score", 42);
        context.SetVariable("token", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...");

        Console.WriteLine("   ✓ Context configured\n");

        // Step 3: Navigate to request configuration
        Console.WriteLine("3. Extracting request configuration...");
        var root = (JsonObject)jsonAst;
        var requests = (JsonObject)root.GetProperty("requests")!;
        var getUserRequest = (JsonObject)requests.GetProperty("getUser")!;

        // Step 4: Evaluate template expressions
        Console.WriteLine("4. Evaluating template expressions:\n");

        // Evaluate URL
        var urlNode = (JsonString)getUserRequest.GetProperty("url")!;
        var evaluatedUrl = TemplateStringEvaluator.Evaluate(urlNode.Template, context);
        Console.WriteLine($"   URL Template: {urlNode.Text}");
        Console.WriteLine($"   Evaluated URL: {evaluatedUrl}\n");

        // Evaluate headers
        var headersNode = (JsonObject)getUserRequest.GetProperty("headers")!;

        var authHeader = (JsonString)headersNode.GetProperty("Authorization")!;
        var evaluatedAuth = TemplateStringEvaluator.Evaluate(authHeader.Template, context);
        Console.WriteLine($"   Auth Template: {authHeader.Text}");
        Console.WriteLine($"   Evaluated Auth: {evaluatedAuth}\n");

        // Generate request ID dynamically
        context.SetVariable("requestId", Guid.NewGuid().ToString());
        var requestIdHeader = (JsonString)headersNode.GetProperty("X-Request-ID")!;
        var evaluatedRequestId = TemplateStringEvaluator.Evaluate(requestIdHeader.Template, context);
        Console.WriteLine($"   Request ID Template: {requestIdHeader.Text}");
        Console.WriteLine($"   Evaluated Request ID: {evaluatedRequestId}\n");

        // Evaluate createPost body
        var createPostRequest = (JsonObject)requests.GetProperty("createPost")!;
        var bodyNode = (JsonString)createPostRequest.GetProperty("body")!;
        var evaluatedBody = TemplateStringEvaluator.Evaluate(bodyNode.Template, context);
        Console.WriteLine($"   Body Template: {bodyNode.Text}");
        Console.WriteLine($"   Evaluated Body: {evaluatedBody}\n");

        // Step 5: Validate syntax before evaluation
        Console.WriteLine("5. Validating template syntax...");
        try
        {
            TemplateStringEvaluator.ValidateSyntax(urlNode.Template);
            TemplateStringEvaluator.ValidateSyntax(authHeader.Template);
            Console.WriteLine("   ✓ All templates are valid\n");
        }
        catch (TemplateExpressionException ex)
        {
            Console.WriteLine($"   ✗ Syntax error at line {ex.Line}, column {ex.Column}: {ex.Message}\n");
        }

        // Step 6: Get referenced variables
        Console.WriteLine("6. Analyzing variable dependencies...");
        var urlVariables = TemplateStringEvaluator.GetReferencedVariables(urlNode.Template);
        Console.WriteLine($"   URL depends on: {string.Join(", ", urlVariables)}\n");

        // Step 7: Error handling example
        Console.WriteLine("7. Error handling example...");
        var invalidTemplate = new TemplateString(new[]
        {
            new TemplateExpressionSegment("undefinedVar + 10", new TextSpan(0, 18, 1, 1, 1, 18))
        });

        if (TemplateStringEvaluator.TryEvaluate(invalidTemplate, context, out var result, out var error))
            Console.WriteLine($"   Result: {result}");
        else
            Console.WriteLine(
                $"   ✗ Evaluation failed: {error?.Message} at line {error?.Line}, column {error?.Column}");

        Console.WriteLine("\n=== Example Complete ===");
    }

    /// <summary>
    /// Example showing advanced use cases
    /// </summary>
    public static void RunAdvancedExample()
    {
        Console.WriteLine("\n=== Advanced Template Expression Usage ===\n");

        var context = new EvaluationContext();

        // Complex nested data structures
        var config = new Dictionary<string, object?>
        {
            ["api"] = new Dictionary<string, object?>
            {
                ["endpoints"] = new List<object?>
                {
                    new Dictionary<string, object?> { ["name"] = "users", ["version"] = "v1" },
                    new Dictionary<string, object?> { ["name"] = "posts", ["version"] = "v2" }
                }
            },
            ["environment"] = "production",
            ["features"] = new Dictionary<string, object?>
            {
                ["authentication"] = true,
                ["ratelimit"] = new Dictionary<string, object?>
                {
                    ["enabled"] = true,
                    ["maxRequests"] = 100
                }
            }
        };

        context.SetVariable("config", config);
        context.SetVariable("userRole", "admin");

        var examples = new[]
        {
            ("Array access", "{{ config.api.endpoints[0].name }}"),
            ("Deep nesting", "{{ config.features.ratelimit.maxRequests }}"),
            ("Conditional feature", "{{ config.features.authentication ? 'Auth enabled' : 'Auth disabled' }}"),
            ("Environment with default", "{{ env('MISSING_VAR', 'default_value') }}"),
            ("Complex URL", "{{ 'https://api.' + config.environment + '.com/' + config.api.endpoints[1].version }}"),
            ("Role-based logic",
                "{{ userRole == 'admin' ? config.features.ratelimit.maxRequests * 2 : config.features.ratelimit.maxRequests }}")
        };

        foreach (var (description, expression) in examples)
            try
            {
                var result = TemplateExpressionEvaluator.Evaluate(expression, context);
                Console.WriteLine($"{description}:");
                Console.WriteLine($"  Expression: {expression}");
                Console.WriteLine($"  Result: {result}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{description}: ERROR - {ex.Message}\n");
            }

        Console.WriteLine("=== Advanced Example Complete ===");
    }
}