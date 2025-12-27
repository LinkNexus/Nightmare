using Nightmare.Config;
using Nightmare.Parser;
using Nightmare.Parser.TemplateExpressions;
using Nightmare.Parser.TemplateExpressions.FunctionsSyntax;
using Terminal.Gui.Configuration;
using Terminal.Gui.Drawing;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace Nightmare.UI;

public class MainWindow : Window
{
    private readonly string _configFilePath;

    private ConfigProcessor _configProcessor;

    private readonly FrameView _errorView;
    private readonly FrameView _currentProfileView;

    private string CurrentProfile
    {
        get;
        set
        {
            field = value;
            _currentProfileView.Text = value;
        }
    }

    private JsonObject _ast;

    public MainWindow(string configFilePath)
    {
        _configFilePath = configFilePath;


        _errorView = new FrameView
        {
            Height = Dim.Auto(),
            Width = Dim.Fill(),
            SchemeName = SchemeManager.SchemesToSchemeName(Schemes.Error),
            Y = Pos.Top(this) + 1
        };

        _currentProfileView = new FrameView
        {
            Height = Dim.Auto(),
            Width = Dim.Percent(30),
            Title = "Selected Profile"
        };

        Add(_errorView, _currentProfileView);

        IsRunningChanged += (_, args) =>
        {
            _configProcessor ??= new ConfigProcessor(App);

            if (args.Value)
                Reload();
        };

        _errorView.VisibleChanged += (_, _) =>
        {
            _currentProfileView.Y = _errorView.Visible
                ? Pos.Bottom(_errorView) + 1
                : Pos.Top(this) + 1;
        };
    }

    public void Reload()
    {
        try
        {
            _errorView.Visible = false;
            _ast = ConfigManager.LoadConfig(_configFilePath);

            Title = _configProcessor.ProcessName(_ast);
            CurrentProfile = _configProcessor.ProcessProfiles(_ast, CurrentProfile);
        }
        catch (TracedException e)
        {
            var title = e switch
            {
                JsonParseException => "Json Parse Error",
                TemplateFunctionException => "Template Function Error",
                TemplateExpressionException => "Template Expression Error",
                ConfigProcessingException => "Config Processing Error",
                JsonProcessingException => "Json Processing Error",
                _ => "Unknown Error"
            };

            _errorView.Visible = true;
            _errorView.Title = title;
            _errorView.Text = $"{e.Message} at line {e.Line}, column {e.Column}";
        }
    }
}