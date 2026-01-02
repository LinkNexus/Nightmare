using System.Diagnostics;
using Nightmare.Config;
using Nightmare.Parser;
using Nightmare.Parser.TemplateExpressions;
using Nightmare.Parser.TemplateExpressions.FunctionsSyntax;
using Terminal.Gui.Configuration;
using Terminal.Gui.Drawing;
using Terminal.Gui.Drivers;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace Nightmare.UI;

public class MainWindow : Window
{
    private readonly string _configFilePath;

    private ConfigProcessor _configProcessor;

    private readonly FrameView _infoView;
    private readonly ProgressBar _progressBar;
    private readonly Label _errorLabel;
    private readonly System.Timers.Timer _progressTimer = new();

    private readonly FrameView _currentProfileView;
    private readonly ProfilesDialog _profilesDialog = new();
    private readonly RecipesView _recipesView;

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
        TabStop = TabBehavior.TabGroup;

        _configFilePath = configFilePath;

        // Info View
        _infoView = new FrameView
        {
            Height = Dim.Auto(),
            Width = Dim.Fill(),
            Y = Pos.Top(this) + 1
        };
        _errorLabel = new Label
        {
            Width = Dim.Fill()
        };
        _progressTimer = new System.Timers.Timer(100) { AutoReset = true };
        _progressBar = new ProgressBar
        {
            Id = "Response Progress Bar",
            BidirectionalMarquee = true,
            ProgressBarStyle = ProgressBarStyle.MarqueeBlocks,
            Width = Dim.Fill()
        };
        _progressTimer.Elapsed += (_, _) => { _progressBar.Pulse(); };

        _currentProfileView = new FrameView
        {
            Height = Dim.Auto(),
            Width = Dim.Percent(30),
            Title = "Selected Profile",
            Arrangement = ViewArrangement.Resizable
        };

        _recipesView = new RecipesView
        {
            Y = Pos.Bottom(_currentProfileView) + 1
        };

        var requestView = new RequestView
        {
            X = Pos.Right(_recipesView) + 1,
            Y = Pos.Top(_currentProfileView)
        };

        var responseView = new ResponseView
        {
            X = Pos.Right(_recipesView) + 1,
            Y = Pos.Bottom(requestView) + 1
        };

        Add(_infoView, _currentProfileView, _recipesView, requestView, responseView);

        IsRunningChanged += (_, args) =>
        {
            _configProcessor ??= new ConfigProcessor(App!);

            if (args.Value)
                Reload();
        };

        _infoView.VisibleChanged += (_, _) =>
        {
            _currentProfileView.Y = _infoView.Visible
                ? Pos.Bottom(_infoView) + 1
                : Pos.Top(this) + 1;
            _recipesView.Y = Pos.Bottom(_currentProfileView) + 1;
            requestView.Y = Pos.Top(_currentProfileView);
            responseView.Y = Pos.Bottom(requestView) + 1;
        };

        KeyDown += (_, args) =>
        {
            if (args.KeyCode == KeyCode.P)
                App.Run(_profilesDialog);
        };

        _profilesDialog.SelectedProfileChanged += (_, args) =>
        {
            if (args == CurrentProfile) return;

            CurrentProfile = args;

            try
            {
                _infoView.Visible = false;
                _configProcessor?.ProcessProfile(_ast, args);
            }
            catch (TracedException e)
            {
                DisplayError(e);
            }
        };

        _recipesView.RequestSelected += async (_, args) =>
        {
            try
            {
                var req = _configProcessor!.ProcessRequest((JsonObject)args.Value);
                var requestTask = _configProcessor.ExecuteRequest(req);

                _progressTimer.Start();

                _infoView.RemoveAll();
                _infoView.Add(_progressBar);
                _infoView.Title = "Loading...";
                _infoView.SchemeName = SchemeManager.SchemesToSchemeName(Schemes.Base);
                _infoView.Visible = true;

                var res = await requestTask;
                requestView.OnRequestSelected(req);
                responseView.OnResponseReceived(res);
            }
            catch (TracedException ex)
            {
                DisplayError(ex);
            }
            finally
            {
                _infoView.Remove(_progressBar);
                _infoView.Visible = false;
            }
        };
    }

    public void Reload()
    {
        try
        {
            _infoView.Visible = false;
            _ast = ConfigManager.LoadConfig(_configFilePath);

            Title = _configProcessor.ProcessName(_ast);
            (
                CurrentProfile,
                _profilesDialog.ProfilesNames
            ) = _configProcessor.ProcessProfiles(_ast, CurrentProfile);

            _recipesView.Requests =
                _ast.TryGetProperty<JsonObject>("requests", out var requests)
                    ? requests.Properties
                        .Select(p => new JsonProperty(p.Key, p.Value, p.Value.Span))
                        .ToList()
                    : []
                ;
        }
        catch (TracedException e)
        {
            DisplayError(e);
        }
    }

    private void DisplayError(TracedException e)
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

        _infoView.RemoveAll();

        _infoView.SchemeName = SchemeManager.SchemesToSchemeName(Schemes.Error);
        _infoView.Title = title;
        _errorLabel.Text = $"{e.Message} at line {e.Line}, column {e.Column}";
        _infoView.Add(_errorLabel);
        _infoView.Visible = true;
    }
}