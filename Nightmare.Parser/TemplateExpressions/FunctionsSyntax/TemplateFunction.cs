namespace Nightmare.Parser.TemplateExpressions.FunctionsSyntax;

public abstract class TemplateFunction
{
    protected readonly FunctionParameter[] Args;

    protected abstract FunctionParameter[] ListArgs();
    public abstract string GetName();
    protected abstract object? Execute(object?[] args, TextSpan span);

    protected TemplateFunction()
    {
        Args = ListArgs();
    }

    private string? CheckArgs(ref object?[] argsList)
    {
        // Find if there's a variadic parameter
        var variadicIndex = -1;
        for (var i = 0; i < Args.Length; i++)
            if (Args[i].Variadic)
            {
                variadicIndex = i;
                break;
            }

        // Check non-variadic parameters
        var nonVariadicCount = variadicIndex >= 0 ? variadicIndex : Args.Length;

        for (var i = 0; i < nonVariadicCount; i++)
        {
            var currentArg = Args[i];

            if (i >= argsList.Length)
            {
                if (currentArg.Required)
                    return $"Missing required argument '{currentArg.Name}'.";
                argsList = [..argsList, currentArg.DefaultValue];
            }
            else
            {
                // Type check
                var arg = argsList[i]; // Capture for lambda
                if (!currentArg.Types.Any(T => T.CompareTo(arg)))
                {
                    var expectedTypes = currentArg.Types.Select(T => T.Name).Aggregate((a, b) => $"{a} or {b}");
                    var actualType = arg?.GetType().Name ?? "null";
                    return $"Argument '{currentArg.Name}' expects {expectedTypes}, but {actualType} was given.";
                }
            }
        }

        // Handle variadic parameter
        if (variadicIndex >= 0)
        {
            var variadicParam = Args[variadicIndex];
            var variadicArgs = new List<object?>();

            // Collect all remaining arguments into variadic array
            for (var i = variadicIndex; i < argsList.Length; i++)
            {
                // Type check each variadic argument
                var arg = argsList[i]; // Capture for lambda
                if (!variadicParam.Types.Any(T => T.CompareTo(arg)))
                {
                    var expectedTypes = variadicParam.Types.Select(T => T.Name).Aggregate((a, b) => $"{a} or {b}");
                    var actualType = arg?.GetType().Name ?? "null";
                    return
                        $"Variadic argument '{variadicParam.Name}' at index {i - variadicIndex} expects {expectedTypes}, but {actualType} was given.";
                }

                variadicArgs.Add(arg);
            }

            // Check if at least one variadic arg is required
            if (variadicParam.Required && variadicArgs.Count == 0)
                return $"Variadic argument '{variadicParam.Name}' requires at least one value.";

            // Replace individual variadic args with array
            var newArgsList = new object?[variadicIndex + 1];
            Array.Copy(argsList, newArgsList, variadicIndex);
            newArgsList[variadicIndex] = variadicArgs.ToArray();
            argsList = newArgsList;
        }
        else
        {
            // No variadic - check we don't have too many args
            if (argsList.Length > Args.Length)
                return $"Expects at most {Args.Length} argument(s), but {argsList.Length} were given.";
        }

        return null;
    }

    protected TemplateFunctionException Error(string error, TextSpan span)
    {
        return new TemplateFunctionException(GetName(), error, span);
    }

    public object? Call(object?[] argsList, TextSpan span)
    {
        var error = CheckArgs(ref argsList);

        return error != null
            ? throw Error(error, span)
            : Execute(argsList, span);
    }
}

public class TemplateFunctionException(string name, string message, TextSpan span)
    : TemplateExpressionException(
        $"Function '{name}': {message}", span
    )
{
}