namespace Nightmare.Parser.TemplateExpressions;

public sealed class FileReference
{
    public string Path { get; }
    public string? FileName { get; set; }
    public string? ContentType { get; }

    public FileReference(string path, string? fileName = null, string? contentType = null)
    {
        Path = path;
        FileName = fileName;
        ContentType = contentType;
    }

    public override string ToString()
    {
        return Path;
    }
}