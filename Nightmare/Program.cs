using System.CommandLine;
using Nightmare.Config;
using Nightmare.Parser;
using Nightmare.UI;
using Terminal.Gui.App;

namespace Nightmare;

internal class Program
{
    private const string DefaultConfigFile = "nightmare.json";
    private static FileSystemWatcher _configFileWatcher = new();
    private static MainWindow _mainWindow;

    private static int Main(string[] args)
    {
        RootCommand rootCommand = new("A TUI app for doing HTTP calls ease.");

        Option<string> configFileNameOption = new("--config", "-c")
        {
            Description = "The name of the configuration file to use.",
            Required = false,
            DefaultValueFactory = _ => DefaultConfigFile
        };

        Option<int> depthOption = new("--depth", "-d")
        {
            Description = "The maximum depth the program should search for the given config file name upwards.",
            Required = false,
            DefaultValueFactory = _ => 5
        };

        rootCommand.Add(AddCreateCommand());
        rootCommand.Options.Add(configFileNameOption);
        rootCommand.Options.Add(depthOption);

        rootCommand.SetAction(parseResult =>
        {
            var configFileName = parseResult.GetValue(configFileNameOption);
            var depth = parseResult.GetValue(depthOption);

            var cwd = Directory.GetCurrentDirectory();
            var configFilePath = ConfigManager.FindConfigFile(cwd, configFileName!, depth);

            MonitorConfigFile(configFilePath);

            using var app = Application.Create();
            app.Init();
            app.Run(_mainWindow = new MainWindow(configFilePath));
        });

        return rootCommand.Parse(args).Invoke();
    }

    private static Command AddCreateCommand()
    {
        Option<string> fileOption = new("--file", "-f")
        {
            Description = "The name of the config file to create.",
            Required = false,
            DefaultValueFactory = _ => DefaultConfigFile
        };

        Command createCommand = new("new", "Creates a new template config file")
        {
            fileOption
        };

        createCommand.SetAction(parseResult =>
        {
            var cwd = Directory.GetCurrentDirectory();
            var configPath = Path.Join(cwd, parseResult.GetValue(fileOption));

            if (!File.Exists(configPath))
            {
                ConfigManager.CreateDefaultConfig(configPath);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Config file created at {0}", configPath);
                return;
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Config file already exists at {0}", configPath);
        });

        return createCommand;
    }

    private static void MonitorConfigFile(string configFilePath)
    {
        var dirName = Path.GetDirectoryName(configFilePath);
        if (dirName is null) return;

        _configFileWatcher = new FileSystemWatcher(dirName, Path.GetFileName(configFilePath));
        _configFileWatcher.NotifyFilter = NotifyFilters.LastWrite;
        _configFileWatcher.EnableRaisingEvents = true;

        _configFileWatcher.Changed += OnConfigFileChanged;
        return;

        void OnConfigFileChanged(object? sender, FileSystemEventArgs args)
        {
            switch (args.ChangeType)
            {
                case WatcherChangeTypes.Changed:
                    _mainWindow.Reload();
                    break;
            }
        }
    }
}