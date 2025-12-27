using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace Nightmare.Parser.TemplateExpressions.FunctionsSyntax.Functions;

public class UuidFunction : TemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return [];
    }

    public override string GetName()
    {
        return "uuid";
    }

    protected override object? Execute(object?[] args, TextSpan span)
    {
        return Guid.NewGuid().ToString();
    }
}

public class HashFunction : TemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return
        [
            new FunctionParameter(
                "input",
                [FunctionParamValueType.String, FunctionParamValueType.Number]
            )
        ];
    }

    public override string GetName()
    {
        return "hash";
    }

    protected override object? Execute(object?[] args, TextSpan span)
    {
        var input = args[0]?.ToString() ?? string.Empty;
        var bytes = System.Text.Encoding.UTF8.GetBytes(input);
        var hash = System.Security.Cryptography.SHA256.HashData(bytes);
        return Convert.ToHexStringLower(hash);
    }
}

public class EnvFunction : TemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return
        [
            new FunctionParameter(
                "name",
                [FunctionParamValueType.String]
            ),
            new FunctionParameter(
                "defaultValue",
                [FunctionParamValueType.String, FunctionParamValueType.Null],
                false,
                null
            )
        ];
    }

    public override string GetName()
    {
        return "env";
    }

    protected override object? Execute(object?[] args, TextSpan span)
    {
        return Environment.GetEnvironmentVariable((string)args[0]!) ?? args[1];
    }
}

public class IfElseFunction : TemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return
        [
            new FunctionParameter(
                "condition",
                [FunctionParamValueType.Boolean]
            ),
            new FunctionParameter(
                "if",
                [
                    FunctionParamValueType.String,
                    FunctionParamValueType.Number,
                    FunctionParamValueType.Array,
                    FunctionParamValueType.Boolean,
                    FunctionParamValueType.Object,
                    FunctionParamValueType.Null
                ]
            ),
            new FunctionParameter("else", [
                FunctionParamValueType.String,
                FunctionParamValueType.Number,
                FunctionParamValueType.Array,
                FunctionParamValueType.Boolean,
                FunctionParamValueType.Object,
                FunctionParamValueType.Null
            ])
        ];
    }

    public override string GetName()
    {
        return "ifElse";
    }

    protected override object? Execute(object?[] args, TextSpan span)
    {
        return (bool)args[0]! ? args[1] : args[2];
    }
}

public class MinFunction : TemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return
        [
            new FunctionParameter(
                "numbers",
                [FunctionParamValueType.Number],
                true,
                Variadic: true
            )
        ];
    }

    public override string GetName()
    {
        return "min";
    }

    protected override object? Execute(object?[] args, TextSpan span)
    {
        var numbers = (object?[])args[0]!;
        return numbers.Length == 0
            ? throw Error("min requires at least one argument", span)
            : numbers.Min(Convert.ToDouble);
    }
}

public class LenFunction : TemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return
        [
            new FunctionParameter(
                "value",
                [FunctionParamValueType.String, FunctionParamValueType.Array]
            )
        ];
    }

    public override string GetName()
    {
        return "len";
    }

    protected override object? Execute(object?[] args, TextSpan span)
    {
        return args[0] switch
        {
            string str => (double)str.Length,
            List<object?> list => (double)list.Count,
            _ => throw Error("len requires a string or array", span)
        };
    }
}

public class MaxFunction : TemplateFunction
{
    protected override FunctionParameter[] ListArgs()
    {
        return
        [
            new FunctionParameter(
                "numbers",
                [FunctionParamValueType.Number],
                Variadic: true
            )
        ];
    }

    public override string GetName()
    {
        return "max";
    }

    protected override object? Execute(object?[] args, TextSpan span)
    {
        var numbers = (object?[])args[0]!;
        return numbers.Length == 0
            ? throw Error("max requires at least one argument", span)
            : numbers.Max(Convert.ToDouble);
    }
}

public class PromptFunction : TemplateFunction
{
    private readonly IApplication _application;
    private readonly Dialog _dialog;
    private readonly Label _label;
    private readonly TextField _textField;

    private string _input = string.Empty;

    public PromptFunction(IApplication application)
    {
        _application = application;
        _dialog = new Dialog
        {
            Width = Dim.Percent(50),
            Height = Dim.Auto(),
            Title = "Prompt"
        };

        _label = new Label();

        _textField = new TextField
        {
            X = Pos.Right(_label) + 1,
            Y = Pos.Top(_label),
            Width = Dim.Fill()
        };
        _textField.TextChanged += (_, _) => { _input = _textField.Text; };

        _dialog.Add(_label, _textField);
        _dialog.Accepting += (_, args) =>
        {
            application.RequestStop();
            args.Handled = true;
        };
    }

    protected override FunctionParameter[] ListArgs()
    {
        return
        [
            new FunctionParameter(
                "message",
                [FunctionParamValueType.String],
                false,
                "Enter the value: "
            )
        ];
    }

    public override string GetName()
    {
        return "prompt";
    }

    protected override object? Execute(object?[] args, TextSpan span)
    {
        _label.Text = (string)args[0]!;
        _application.Run(_dialog);
        return _input;
    }
}