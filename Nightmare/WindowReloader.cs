using Nightmare.Parser;
using Nightmare.Parser.TemplateExpressions;
using Nightmare.UI;

namespace Nightmare;

public class WindowReloader(MainWindow window)
{
    private void ReloadTitle(JsonObject ast)
    {
        if (ast.TryGetProperty("name", out var name))
        {
            if (name is JsonString jsonStr)
            {
                if (jsonStr.Template.HasExpressions)
                    window.Title = TemplateStringEvaluator
                        .Evaluate(jsonStr.Template, window.EvaluationContext);
                else window.Text = jsonStr.Text;
            }
            else
            {
                throw new TemplateExpressionException("Name must be a string", name.Span);
            }
        }
        else
        {
            window.Text = "Requests Collection";
        }
    }

    public void Reload(JsonObject ast)
    {
        ReloadTitle(ast);
    }
}