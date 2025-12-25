using System.Reflection;
using Nightmare.Parser;

namespace Nightmare.Config;

public static class ConfigManager
{
    public static void CreateDefaultConfig(string path)
    {
        var assembly = Assembly.GetExecutingAssembly();
        const string resourceName = "Nightmare..nightmare.json";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Could not find embedded resource {0}", resourceName);
            return;
        }

        using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
        stream.CopyTo(fileStream);
    }

    public static string FindConfigFile(
        string cwd,
        string configFileName,
        int maxDepth,
        int currentDepth = 0
    )
    {
        while (true)
        {
            if (currentDepth >= maxDepth)
                throw new ConfigNotFoundException(configFileName);

            var path = Path.Join(cwd, configFileName);
            if (File.Exists(path)) return path;

            currentDepth++;

            var parentDir = Directory.GetParent(cwd);
            if (parentDir is null)
                throw new ConfigNotFoundException(configFileName);

            cwd = parentDir.FullName;
        }
    }

    public static JsonObject LoadConfig(string configFilePath)
    {
        var content = File.ReadAllText(configFilePath);

        var ast = JsonParser.Parse(content);
        if (ast is JsonObject obj) return obj;

        throw new Exception("The root node must be an object");
    }
}