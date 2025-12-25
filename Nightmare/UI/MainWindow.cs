using Nightmare.Config;
using Nightmare.Parser;
using Nightmare.Parser.TemplateExpressions;
using Nightmare.Parser.TemplateExpressions.FunctionsSyntax;
using Terminal.Gui.Views;

namespace Nightmare.UI;

public class MainWindow : Window
{
    private readonly string _configFilePath;

    public readonly EvaluationContext EvaluationContext;
    private readonly WindowReloader _reloader;

    private JsonObject _ast;

    public MainWindow(string configFilePath)
    {
        _configFilePath = configFilePath;

        EvaluationContext = new EvaluationContext();
        _reloader = new WindowReloader(this);

        IsRunningChanged += (_, args) =>
        {
            if (args.Value)
                Reload();
        };
    }

    public void Reload()
    {
        try
        {
            _ast = ConfigManager.LoadConfig(_configFilePath);
            _reloader.Reload(_ast);

            if (!IsCurrentTop) App.RequestStop();
        }
        catch (ParserException e)
        {
            var title = e switch
            {
                JsonParseException => "Json Parse Error",
                TemplateFunctionException => "Template Function Error",
                TemplateExpressionException => "Template Expression Error",
                _ => "Unknown Error"
            };

            MessageBox.ErrorQuery(
                App,
                title,
                $"{e.Message} at line {e.Line}, column {e.Column}"
            );
        }
    }
}