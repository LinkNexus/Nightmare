using Nightmare.Parser.TemplateExpressions.Functions;

namespace Nightmare.Parser.TemplateExpressions.FunctionsSyntax;

public record FunctionParameter(
    string Name,
    FunctionParamValueType[] Types,
    bool Required = true,
    object? DefaultValue = null,
    bool Variadic = false
)
{
}