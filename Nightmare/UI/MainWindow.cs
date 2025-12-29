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

    private readonly FrameView _errorView;
    private readonly FrameView _currentProfileView;
    private readonly ProfilesDialog _profilesDialog = new();
    private readonly RecipesView _recipesView;
    private readonly RequestView _requestView;
    private readonly ResponseView _responseView;

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
            Title = "Selected Profile",
            Arrangement = ViewArrangement.Resizable
        };

        _recipesView = new RecipesView
        {
            Y = Pos.Bottom(_currentProfileView) + 1
        };

        _requestView = new RequestView
        {
            X = Pos.Right(_recipesView) + 1,
            Y = Pos.Top(_currentProfileView)
        };

        _responseView = new ResponseView
        {
            X = Pos.Right(_recipesView) + 1,
            Y = Pos.Bottom(_requestView) + 1
        };

        Add(_errorView, _currentProfileView, _recipesView, _requestView, _responseView);

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
            _recipesView.Y = Pos.Bottom(_currentProfileView) + 1;
            _requestView.Y = Pos.Top(_currentProfileView);
            _responseView.Y = Pos.Bottom(_requestView) + 1;
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
                _errorView.Visible = false;
                _configProcessor?.ProcessProfile(_ast, args);
            }
            catch (TracedException e)
            {
                DisplayError(e);
            }
        };

        var progressTimer = new System.Timers.Timer(100) { AutoReset = true };
        var progressBar = new ProgressBar
        {
            Id = "Response Progress Bar",
            BidirectionalMarquee = true,
            ProgressBarStyle = ProgressBarStyle.MarqueeBlocks,
            Width = Dim.Fill()
        };
        var progressDialog = new Dialog
        {
            Title = "Loading...",
            X = Pos.Center(),
            Y = Pos.Center(),
            Height = Dim.Auto(),
            Width = Dim.Percent(50)
        };
        progressDialog.Add(progressBar);
        progressTimer.Elapsed += (_, _) => { progressBar.Pulse(); };

        _recipesView.RequestSelected += async (_, args) =>
        {
            try
            {
                var requestTask = _configProcessor!.ProcessAndExecuteRequest(args);

                // progressTimer.Start();

                // _ = requestTask.ContinueWith(_ =>
                // {
                //     App.RequestStop();
                //     progressTimer.Stop();
                // });

                // App.Run(progressDialog);

                var (req, res) = await requestTask;
                await _requestView.OnRequestSelected(req);
                await _responseView.OnResponseReceived(res);
            }
            catch (TracedException ex)
            {
                // App.RequestStop();
                // progressTimer.Stop();
                DisplayError(ex);
            }
        };
    }

    public void Reload()
    {
        try
        {
            _errorView.Visible = false;
            _ast = ConfigManager.LoadConfig(_configFilePath);

            Title = _configProcessor.ProcessName(_ast);
            (
                CurrentProfile,
                _profilesDialog.ProfilesNames
            ) = _configProcessor.ProcessProfiles(_ast, CurrentProfile);

            _recipesView.Requests = _configProcessor.ProcessRequests(_ast);
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

        _errorView.Visible = true;
        _errorView.Title = title;
        _errorView.Text = $"{e.Message} at line {e.Line}, column {e.Column}";
    }
}