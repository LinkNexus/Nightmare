namespace Nightmare.Parser.TemplateExpressions.FunctionsSyntax;

public readonly struct FunctionParamValueType
{
    public string Name { get; }

    private FunctionParamValueType(string name)
    {
        Name = name;
    }

    public static FunctionParamValueType String => new("String");
    public static FunctionParamValueType Number => new("Number");
    public static FunctionParamValueType Boolean => new("Boolean");
    public static FunctionParamValueType Array => new("Array");
    public static FunctionParamValueType Object => new("Object");
    public static FunctionParamValueType Null => new("Null");

    public bool CompareTo(object? value)
    {
        return Name switch
        {
            "Null" => value is null,
            "String" => value is string,
            "Number" => value is double or float or decimal or int or long,
            "Boolean" => value is bool,
            "Array" => value is List<object?>,
            "Object" => value is Dictionary<string, object?>,
            _ => false
        };
    }
}