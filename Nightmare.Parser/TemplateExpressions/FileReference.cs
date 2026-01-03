namespace Nightmare.Parser.TemplateExpressions;

public sealed class FileReference(string path, string? fileName = null, string? contentType = null)
{
    public string Path { get; } = path;
    public string? FileName { get; set; } = fileName;
    public string? ContentType { get; } = contentType;

    public override string ToString()
    {
        return Path;
    }
}