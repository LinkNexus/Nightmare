using Nightmare.Parser;
using Nightmare.Parser.TemplateExpressions;

namespace Nightmare.Tests.ParserTests.TemplateExpressions;

public class TemplateExpressionIntegrationTests
{
    [Fact]
    public void Integration_ParseJsonWithTemplates_AndEvaluate()
    {
        var json = @"{
            ""url"": ""{{ base_url }}/api/{{ version }}"",
            ""timeout"": ""{{ retries * 1000 }}""
        }";

        var ast = JsonParser.Parse(json);
        var obj = Assert.IsType<JsonObject>(ast);

        var context = new EvaluationContext();
        context.SetVariable("base_url", "https://example.com");
        context.SetVariable("version", "v1");
        context.SetVariable("retries", 3);

        var urlNode = Assert.IsType<JsonString>(obj.GetProperty("url"));
        var url = TemplateStringEvaluator.Evaluate(urlNode.Template, context);

        var timeoutNode = Assert.IsType<JsonString>(obj.GetProperty("timeout"));
        var timeout = TemplateStringEvaluator.Evaluate(timeoutNode.Template, context);

        Assert.Equal("https://example.com/api/v1", url);
        Assert.Equal("3000", timeout);
    }

    [Fact]
    public void Integration_ComplexNestedJson_EvaluatesCorrectly()
    {
        var json = """
                   {
                    "requests": {
                        "login": {
                            "url": "{{ base_url }}/auth/login",
                            "headers": {
                                "Authorization": "Bearer {{ token }}"
                            },
                            "body": "{{ user.username + ':' + user.password }}"
                        }
                    }
                    }
                   """;

        var ast = JsonParser.Parse(json);
        var root = Assert.IsType<JsonObject>(ast);

        var context = new EvaluationContext();
        context.SetVariable("base_url", "https://api.example.com");
        context.SetVariable("token", "abc123");
        context.SetVariable("user", new Dictionary<string, object?>
        {
            ["username"] = "john",
            ["password"] = "secret"
        });

        var requests = Assert.IsType<JsonObject>(root.GetProperty("requests"));
        var login = Assert.IsType<JsonObject>(requests.GetProperty("login"));

        var urlNode = Assert.IsType<JsonString>(login.GetProperty("url"));
        var url = TemplateStringEvaluator.Evaluate(urlNode.Template, context);

        var headers = Assert.IsType<JsonObject>(login.GetProperty("headers"));
        var authNode = Assert.IsType<JsonString>(headers.GetProperty("Authorization"));
        var auth = TemplateStringEvaluator.Evaluate(authNode.Template, context);

        var bodyNode = Assert.IsType<JsonString>(login.GetProperty("body"));
        var body = TemplateStringEvaluator.Evaluate(bodyNode.Template, context);

        Assert.Equal("https://api.example.com/auth/login", url);
        Assert.Equal("Bearer abc123", auth);
        Assert.Equal("john:secret", body);
    }

    [Fact]
    public void Integration_WithCustomFunctions_EvaluatesCorrectly()
    {
        var json = @"{
            ""requestId"": ""{{ uuid() }}"",
            ""timestamp"": ""{{ timestamp() }}"",
            ""uppercaseName"": ""{{ upper(name) }}""
        }";

        var ast = JsonParser.Parse(json);
        var obj = Assert.IsType<JsonObject>(ast);

        var context = new EvaluationContext();
        context.SetVariable("name", "john");
        context.RegisterFunction(new TestUuidFunction());
        context.RegisterFunction(new TestTimestampFunction());
        context.RegisterFunction(new TestUpperFunction());

        var requestIdNode = Assert.IsType<JsonString>(obj.GetProperty("requestId"));
        var requestId = TemplateStringEvaluator.Evaluate(requestIdNode.Template, context);

        var timestampNode = Assert.IsType<JsonString>(obj.GetProperty("timestamp"));
        var timestamp = TemplateStringEvaluator.Evaluate(timestampNode.Template, context);

        var nameNode = Assert.IsType<JsonString>(obj.GetProperty("uppercaseName"));
        var uppercaseName = TemplateStringEvaluator.Evaluate(nameNode.Template, context);

        Assert.Equal("test-uuid-123", requestId);
        Assert.Equal("1234567890", timestamp);
        Assert.Equal("JOHN", uppercaseName);
    }

    [Fact]
    public void Integration_ConditionalUrl_EvaluatesCorrectly()
    {
        var json = @"{
            ""url"": ""{{ isProd ? 'https://api.prod.com' : 'http://localhost:3000' }}""
        }";

        var ast = JsonParser.Parse(json);
        var obj = Assert.IsType<JsonObject>(ast);

        // Test production
        var prodContext = new EvaluationContext();
        prodContext.SetVariable("isProd", true);

        var urlNode = Assert.IsType<JsonString>(obj.GetProperty("url"));
        var prodUrl = TemplateStringEvaluator.Evaluate(urlNode.Template, prodContext);

        Assert.Equal("https://api.prod.com", prodUrl);

        // Test development
        var devContext = new EvaluationContext();
        devContext.SetVariable("isProd", false);

        var devUrl = TemplateStringEvaluator.Evaluate(urlNode.Template, devContext);

        Assert.Equal("http://localhost:3000", devUrl);
    }

    [Fact]
    public void Integration_ArrayAccess_EvaluatesCorrectly()
    {
        var json = @"{
            ""firstUser"": ""{{ users[0].name }}"",
            ""secondUserEmail"": ""{{ users[1].email }}""
        }";

        var ast = JsonParser.Parse(json);
        var obj = Assert.IsType<JsonObject>(ast);

        var context = new EvaluationContext();
        context.SetVariable("users", new List<object?>
        {
            new Dictionary<string, object?> { ["name"] = "Alice", ["email"] = "alice@example.com" },
            new Dictionary<string, object?> { ["name"] = "Bob", ["email"] = "bob@example.com" }
        });

        var firstUserNode = Assert.IsType<JsonString>(obj.GetProperty("firstUser"));
        var firstUser = TemplateStringEvaluator.Evaluate(firstUserNode.Template, context);

        var secondEmailNode = Assert.IsType<JsonString>(obj.GetProperty("secondUserEmail"));
        var secondEmail = TemplateStringEvaluator.Evaluate(secondEmailNode.Template, context);

        Assert.Equal("Alice", firstUser);
        Assert.Equal("bob@example.com", secondEmail);
    }

    [Fact]
    public void Integration_MathOperations_EvaluatesCorrectly()
    {
        var json = @"{
            ""total"": ""{{ price * quantity * (1 + tax) }}"",
            ""discount"": ""{{ total > 100 ? total * 0.1 : 0 }}""
        }";

        var ast = JsonParser.Parse(json);
        var obj = Assert.IsType<JsonObject>(ast);

        var context = new EvaluationContext();
        context.SetVariable("price", 50);
        context.SetVariable("quantity", 3);
        context.SetVariable("tax", 0.2);

        var totalNode = Assert.IsType<JsonString>(obj.GetProperty("total"));
        var total = TemplateStringEvaluator.Evaluate(totalNode.Template, context);

        Assert.Equal("180", total);

        context.SetVariable("total", 180);
        var discountNode = Assert.IsType<JsonString>(obj.GetProperty("discount"));
        var discount = TemplateStringEvaluator.Evaluate(discountNode.Template, context);

        Assert.Equal("18", discount);
    }

    [Fact]
    public void Integration_ValidateSyntaxBeforeEvaluation()
    {
        var json = @"{
            ""valid"": ""{{ a + b }}"",
            ""invalid"": ""{{ c + }}""
        }";

        var ast = JsonParser.Parse(json);
        var obj = Assert.IsType<JsonObject>(ast);

        var validNode = Assert.IsType<JsonString>(obj.GetProperty("valid"));
        var invalidNode = Assert.IsType<JsonString>(obj.GetProperty("invalid"));

        // Valid expression should not throw
        var validException = Record.Exception(() =>
            TemplateStringEvaluator.ValidateSyntax(validNode.Template));
        Assert.Null(validException);

        // Invalid expression should throw
        Assert.Throws<TemplateExpressionException>(() =>
            TemplateStringEvaluator.ValidateSyntax(invalidNode.Template));
    }

    [Fact]
    public void Integration_GetReferencedVariables_FindsAllDependencies()
    {
        var json = @"{
            ""url"": ""{{ base_url }}/users/{{ user.id }}"",
            ""auth"": ""{{ token }}"",
            ""computed"": ""{{ price * quantity }}""
        }";

        var ast = JsonParser.Parse(json);
        var obj = Assert.IsType<JsonObject>(ast);

        var urlNode = Assert.IsType<JsonString>(obj.GetProperty("url"));
        var urlVars = TemplateStringEvaluator.GetReferencedVariables(urlNode.Template);

        Assert.Contains("base_url", urlVars);
        Assert.Contains("user", urlVars);

        var authNode = Assert.IsType<JsonString>(obj.GetProperty("auth"));
        var authVars = TemplateStringEvaluator.GetReferencedVariables(authNode.Template);

        Assert.Single(authVars);
        Assert.Contains("token", authVars);

        var computedNode = Assert.IsType<JsonString>(obj.GetProperty("computed"));
        var computedVars = TemplateStringEvaluator.GetReferencedVariables(computedNode.Template);

        Assert.Equal(2, computedVars.Count());
        Assert.Contains("price", computedVars);
        Assert.Contains("quantity", computedVars);
    }

    [Fact]
    public void Integration_ErrorHandling_ProvidesDetailedErrorInfo()
    {
        var json = @"{
            ""url"": ""{{ undefinedVar + 10 }}""
        }";

        var ast = JsonParser.Parse(json);
        var obj = Assert.IsType<JsonObject>(ast);
        var urlNode = Assert.IsType<JsonString>(obj.GetProperty("url"));

        var context = new EvaluationContext();

        var exception = Assert.Throws<TemplateExpressionException>(() =>
            TemplateStringEvaluator.Evaluate(urlNode.Template, context));

        Assert.Contains("undefinedVar", exception.Message);
        Assert.True(exception.Span.StartLine > 0);
        Assert.True(exception.Span.StartColumn > 0);
    }

    [Fact]
    public void Integration_StringConcatenationInJson_WorksCorrectly()
    {
        var json = @"{
            ""greeting"": ""{{ 'Hello, ' + name + '!' }}"",
            ""message"": ""{{ 'You have ' + count + ' new messages' }}""
        }";

        var ast = JsonParser.Parse(json);
        var obj = Assert.IsType<JsonObject>(ast);

        var context = new EvaluationContext();
        context.SetVariable("name", "Alice");
        context.SetVariable("count", 5);

        var greetingNode = Assert.IsType<JsonString>(obj.GetProperty("greeting"));
        var greeting = TemplateStringEvaluator.Evaluate(greetingNode.Template, context);

        var messageNode = Assert.IsType<JsonString>(obj.GetProperty("message"));
        var message = TemplateStringEvaluator.Evaluate(messageNode.Template, context);

        Assert.Equal("Hello, Alice!", greeting);
        Assert.Equal("You have 5 new messages", message);
    }

    [Fact]
    public void Integration_MultipleTemplatesInOneString_EvaluatesAll()
    {
        var json = @"{
            ""path"": ""{{ protocol }}://{{ host }}:{{ port }}/{{ endpoint }}""
        }";

        var ast = JsonParser.Parse(json);
        var obj = Assert.IsType<JsonObject>(ast);

        var context = new EvaluationContext();
        context.SetVariable("protocol", "https");
        context.SetVariable("host", "api.example.com");
        context.SetVariable("port", 8080);
        context.SetVariable("endpoint", "users");

        var pathNode = Assert.IsType<JsonString>(obj.GetProperty("path"));
        var path = TemplateStringEvaluator.Evaluate(pathNode.Template, context);

        Assert.Equal("https://api.example.com:8080/users", path);
    }

    [Fact]
    public void Integration_RealWorldRestClientConfig_EvaluatesCompletely()
    {
        var json = @"{
            ""profiles"": {
                ""dev"": {
                    ""base_url"": ""https://httpbin.org"",
                    ""timeout"": ""5000""
                }
            },
            ""requests"": {
                ""login"": {
                    ""url"": ""{{ base_url }}/post"",
                    ""method"": ""POST"",
                    ""headers"": {
                        ""Content-Type"": ""application/json"",
                        ""X-Request-ID"": ""{{ requestId }}""
                    },
                    ""body"": ""{{ 'username=' + username + '&password=' + password }}""
                }
            }
        }";

        var ast = JsonParser.Parse(json);
        var root = Assert.IsType<JsonObject>(ast);

        // Extract profile data
        var profiles = Assert.IsType<JsonObject>(root.GetProperty("profiles"));
        var dev = Assert.IsType<JsonObject>(profiles.GetProperty("dev"));
        var baseUrlNode = Assert.IsType<JsonString>(dev.GetProperty("base_url"));

        // Setup context
        var context = new EvaluationContext();
        context.SetVariable("base_url", baseUrlNode.Text);
        context.SetVariable("username", "testuser");
        context.SetVariable("password", "testpass");
        context.SetVariable("requestId", "req-123");

        // Evaluate request
        var requests = Assert.IsType<JsonObject>(root.GetProperty("requests"));
        var login = Assert.IsType<JsonObject>(requests.GetProperty("login"));

        var urlNode = Assert.IsType<JsonString>(login.GetProperty("url"));
        var url = TemplateStringEvaluator.Evaluate(urlNode.Template, context);

        var headers = Assert.IsType<JsonObject>(login.GetProperty("headers"));
        var requestIdNode = Assert.IsType<JsonString>(headers.GetProperty("X-Request-ID"));
        var requestIdHeader = TemplateStringEvaluator.Evaluate(requestIdNode.Template, context);

        var bodyNode = Assert.IsType<JsonString>(login.GetProperty("body"));
        var body = TemplateStringEvaluator.Evaluate(bodyNode.Template, context);

        Assert.Equal("https://httpbin.org/post", url);
        Assert.Equal("req-123", requestIdHeader);
        Assert.Equal("username=testuser&password=testpass", body);
    }
}