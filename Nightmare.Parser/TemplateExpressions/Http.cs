using System.Net;

namespace Nightmare.Parser.TemplateExpressions;

public class Request
{
    public string Id { get; set; }

    public string Method { get; set; }

    public string Url { get; set; }

    public string FullUrl { get; set; }

    public Dictionary<string, string> Query { get; set; } = new();

    public Dictionary<string, string[]> Headers { get; set; } = new();

    public Dictionary<string, string> Cookies { get; set; } = new();

    public double? Timeout { get; set; }

    public string Type { get; set; }

    public object? Content { get; set; }

    public HttpContent? Body { get; set; }

    public HttpRequestMessage ToMessage()
    {
        var req = new HttpRequestMessage(new HttpMethod(Method), FullUrl);
        req.Content = Body;

        foreach (var (k, v) in Headers)
        {
            if (req.Headers.TryAddWithoutValidation(k, v))
                continue;
            req.Content?.Headers.TryAddWithoutValidation(k, v);
        }

        foreach (var (cName, cVal) in Cookies)
            req.Headers.TryAddWithoutValidation("Cookie", $"{cName}={WebUtility.UrlEncode(cVal)}");

        return req;
    }
}

public class Response
{
    public int StatusCode { get; set; }
    public string? ReasonPhrase { get; set; }

    public Dictionary<string, string> Headers { get; } = new();

    public Dictionary<string, string> Cookies { get; } = new();

    public string Content { get; set; }

    public DateTime Timestamp { get; set; }

    public long ResponseTimeMs { get; set; }

    private Response()
    {
    }

    public static async Task<Response> Create(HttpResponseMessage res, long responseTimeMs)
    {
        var instance = new Response
        {
            ResponseTimeMs = responseTimeMs,
            StatusCode = res.StatusCode.GetHashCode(),
            ReasonPhrase = res.ReasonPhrase,
            Content = await res.Content.ReadAsStringAsync(),
            Timestamp = DateTime.UtcNow
        };

        foreach (var (key, value) in res.Headers)
            instance.Headers[key] = string.Join(", ", value);

        foreach (var (key, value) in res.Content.Headers)
            instance.Headers[key] = string.Join(", ", value);

        if (res.Headers.TryGetValues("set-cookie", out var setCookies))
            foreach (var cookie in setCookies)
            {
                var parts = cookie.Split(';')[0].Split('=', 2);
                var key = parts[0].Trim();
                var value = parts.Length > 1 ? parts[1].Trim() : "";
                instance.Cookies.Add(key, value);
            }

        return instance;
    }

    public bool IsStale(TimeSpan maxAge)
    {
        return DateTime.UtcNow - Timestamp > maxAge;
    }

    public Dictionary<string, object?> Convert()
    {
        return new Dictionary<string, object?>
        {
            ["statusCode"] = (double)StatusCode,
            ["reasonPhrase"] = ReasonPhrase,
            ["timestamp"] = Timestamp.ToString("o"),
            ["headers"] = Headers.ToDictionary(kvp => kvp.Key, object? (kvp) => kvp.Value),
            ["cookies"] = Cookies.ToDictionary(kvp => kvp.Key, object? (kvp) => kvp.Value),
            ["body"] = Content
        };
    }
}