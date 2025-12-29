using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace Nightmare.Parser.TemplateExpressions.FunctionsSyntax.Functions;

public class ReadFileFunction : TemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return
        [
            new FunctionParameter(
                "path",
                [FunctionParamValueType.String]
            )
        ];
    }

    public override string GetName()
    {
        return "readFile";
    }

    protected override object? Execute(object?[] args, TextSpan span)
    {
        var path = (string)args[0]!;
        return File.Exists(path)
            ? File.ReadAllText(path)
            : throw Error($"File {path} not found", span);
    }
}

public class FilePromptFunction : TemplateFunction
{
    private readonly FileDialog _dialog;
    private readonly IApplication _application;

    private string _filePath = string.Empty;

    public FilePromptFunction(IApplication application)
    {
        _application = application;

        _dialog = new FileDialog
        {
            Title = "File Prompt",
            Height = Dim.Percent(50),
            Width = Dim.Percent(50),
            OpenMode = OpenMode.File,
            AllowsMultipleSelection = false
        };

        _dialog.FilesSelected += (_, _) => { _filePath = _dialog.Path; };
    }

    protected override FunctionParameter[] ListArgs()
    {
        return [];
    }

    public override string GetName()
    {
        return "filePrompt";
    }

    protected override object? Execute(object?[] args, TextSpan span)
    {
        if (_application.TopRunnableView is Dialog) _application.RequestStop();

        _application.Invoke(() => { _application.Run(_dialog); });
        return _filePath;
    }
}

public class FileFunction : TemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return
        [
            new FunctionParameter(
                "path",
                [FunctionParamValueType.String]
            )
        ];
    }

    public override string GetName()
    {
        return "file";
    }

    protected override object? Execute(object?[] args, TextSpan span)
    {
        if (!File.Exists((string)args[0]!))
            throw Error("The file does not exists", span);

        // return
    }
}