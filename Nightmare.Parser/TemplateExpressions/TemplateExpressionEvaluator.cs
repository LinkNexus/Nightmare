using System.Globalization;
using Nightmare.Parser.TemplateExpressions.Functions;
using Nightmare.Parser.TemplateExpressions.FunctionsSyntax;
using Nightmare.Parser.TemplateExpressions.FunctionsSyntax.Functions;

namespace Nightmare.Parser.TemplateExpressions;

/// <summary>
/// Context for evaluating template expressions, providing variable values and functions
/// </summary>
public class EvaluationContext
{
    private readonly Dictionary<string, object?> _variables = new();

    private readonly List<BaseTemplateFunction> _functions =
    [
        new LowerFunction(),
        new ConcatFunction()
    ];

    public void SetVariable(string name, object? value)
    {
        _variables[name] = value;
    }

    public object? GetVariable(string name, TextSpan span)
    {
        if (_variables.TryGetValue(name, out var value))
            return value;

        throw new TemplateExpressionException(
            $"Variable '{name}' not found in context",
            span
        );
    }

    public bool HasVariable(string name)
    {
        return _variables.ContainsKey(name);
    }

    public void RegisterFunction(BaseTemplateFunction function)
    {
        _functions.Add(function);
    }

    public BaseTemplateFunction GetFunction(string name, TextSpan span)
    {
        try
        {
            return _functions
                .First(f => f.GetName() == name);
        }
        catch (InvalidOperationException)
        {
            throw new TemplateExpressionException(
                $"Function '{name}' not found in context",
                span
            );
        }
    }

    public bool HasFunction(string name)
    {
        return _functions.Any(f => f.GetName() == name);
    }
}

/// <summary>
/// Evaluates parsed template expressions
/// </summary>
public sealed class TemplateExpressionEvaluator(EvaluationContext context)
{
    private static bool IsTruthy(object? value)
    {
        return value switch
        {
            null => false,
            bool b => b,
            double d => d != 0,
            string s => !string.IsNullOrEmpty(s),
            _ => true
        };
    }

    private static double ToNumber(object? value, TextSpan span)
    {
        return value switch
        {
            double d => d,
            int i => i,
            long l => l,
            float f => f,
            string s when double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) => result,
            bool b => b ? 1.0 : 0.0,
            null => 0.0,
            _ => throw new TemplateExpressionException($"Cannot convert {value.GetType().Name} to number", span)
        };
    }

    private static string ToString(object? value)
    {
        return value switch
        {
            null => "null",
            string s => s,
            double d => d.ToString(CultureInfo.InvariantCulture),
            bool b => b ? "true" : "false",
            _ => value.ToString() ?? string.Empty
        };
    }

    private static bool AreEqual(object? left, object? right)
    {
        if (left == null && right == null) return true;
        if (left == null || right == null) return false;

        // Try numeric comparison
        if (left is not (double or int or long or float) ||
            right is not (double or int or long or float)) return left.Equals(right);

        var leftNum = Convert.ToDouble(left);
        var rightNum = Convert.ToDouble(right);
        return Math.Abs(leftNum - rightNum) < double.Epsilon;
    }

    private static int Compare(object? left, object? right, TextSpan span)
    {
        switch (left)
        {
            case double or int or long or float:
            {
                var leftNum = Convert.ToDouble(left);
                var rightNum = ToNumber(right, span);
                return leftNum.CompareTo(rightNum);
            }
            case string leftStr when right is string rightStr:
                return string.Compare(leftStr, rightStr, StringComparison.Ordinal);
            default:
                throw new TemplateExpressionException(
                    $"Cannot compare {left?.GetType().Name ?? "null"} with {right?.GetType().Name ?? "null"}",
                    span
                );
        }
    }

    public object? Evaluate(TemplateExpression expression)
    {
        return expression switch
        {
            NumberLiteralExpression number => number.Value,
            StringLiteralExpression str => str.Value,
            BooleanLiteralExpression boolean => boolean.Value,
            NullLiteralExpression => null,

            IdentifierExpression identifier => EvaluateIdentifier(identifier),
            MemberAccessExpression member => EvaluateMemberAccess(member),
            IndexAccessExpression index => EvaluateIndexAccess(index),
            CallExpression call => EvaluateCall(call),
            UnaryExpression unary => EvaluateUnary(unary),
            BinaryExpression binary => EvaluateBinary(binary),
            ConditionalExpression conditional => EvaluateConditional(conditional),

            _ => throw new TemplateExpressionException(
                $"Unknown expression type: {expression.GetType().Name}",
                expression.Span
            )
        };
    }

    private object? EvaluateIdentifier(IdentifierExpression identifier)
    {
        return context.GetVariable(identifier.Name, identifier.Span);
    }

    private object? EvaluateMemberAccess(MemberAccessExpression member)
    {
        var target = Evaluate(member.Target);

        if (target == null)
            throw new TemplateExpressionException(
                "Cannot access member of null",
                member.Span
            );

        // Handle dictionary-like objects (primary approach for AOT compatibility)
        if (target is IDictionary<string, object?> dict)
        {
            if (dict.TryGetValue(member.MemberName, out var value))
                return value;

            throw new TemplateExpressionException(
                $"Property '{member.MemberName}' not found",
                member.Span
            );
        }

        // For AOT compatibility, only support dictionary-based member access
        // Users should use Dictionary<string, object?> for nested objects
        throw new TemplateExpressionException(
            $"Member access is only supported for Dictionary<string, object?> types. " +
            $"Found: {target.GetType().Name}. Use dictionaries for nested data structures.",
            member.Span
        );
    }

    private object? EvaluateIndexAccess(IndexAccessExpression index)
    {
        var target = Evaluate(index.Target);
        var indexValue = Evaluate(index.Index);

        if (target == null)
            throw new TemplateExpressionException(
                "Cannot index into null",
                index.Span
            );

        // Handle array/list indexing
        if (target is System.Collections.IList list)
        {
            var idx = (int)ToNumber(indexValue, index.Index.Span);

            if (idx < 0 || idx >= list.Count)
                throw new TemplateExpressionException(
                    $"Index {idx} out of range [0..{list.Count - 1}]",
                    index.Span
                );

            return list[idx];
        }

        // Handle dictionary indexing
        if (target is IDictionary<string, object?> dict)
        {
            var key = ToString(indexValue);

            if (dict.TryGetValue(key, out var value))
                return value;

            throw new TemplateExpressionException(
                $"Key '{key}' not found",
                index.Span
            );
        }

        throw new TemplateExpressionException(
            $"Cannot index into {target.GetType().Name}",
            index.Span
        );
    }

    private object? EvaluateCall(CallExpression call)
    {
        // Evaluate the callee to get the function name
        if (call.Callee is not IdentifierExpression identifier)
            throw new TemplateExpressionException(
                "Only named functions can be called",
                call.Callee.Span
            );

        var functionName = identifier.Name;
        var function = context.GetFunction(functionName, call.Span);

        // Evaluate arguments
        var arguments = call.Arguments
            .Select(Evaluate)
            .ToArray();

        // Call the function - it will handle its own errors with proper span
        return function.Call(arguments, call.Span);
    }

    private object? EvaluateUnary(UnaryExpression unary)
    {
        var operand = Evaluate(unary.Operand);

        return unary.Operator switch
        {
            UnaryOperator.Not => !IsTruthy(operand),
            UnaryOperator.Negate => -ToNumber(operand, unary.Operand.Span),
            _ => throw new TemplateExpressionException(
                $"Unknown unary operator: {unary.Operator}",
                unary.Span
            )
        };
    }

    private object? EvaluateBinary(BinaryExpression binary)
    {
        // Short-circuit evaluation for logical operators
        if (binary.Operator == BinaryOperator.And)
        {
            var left = Evaluate(binary.Left);
            if (!IsTruthy(left)) return false;
            return IsTruthy(Evaluate(binary.Right));
        }

        if (binary.Operator == BinaryOperator.Or)
        {
            var left = Evaluate(binary.Left);
            if (IsTruthy(left)) return true;
            return IsTruthy(Evaluate(binary.Right));
        }

        // Evaluate both operands for other operators
        var leftValue = Evaluate(binary.Left);
        var rightValue = Evaluate(binary.Right);

        return binary.Operator switch
        {
            // Arithmetic
            BinaryOperator.Add => EvaluateAddition(leftValue, rightValue, binary.Span),
            BinaryOperator.Subtract => ToNumber(leftValue, binary.Left.Span) - ToNumber(rightValue, binary.Right.Span),
            BinaryOperator.Multiply => ToNumber(leftValue, binary.Left.Span) * ToNumber(rightValue, binary.Right.Span),
            BinaryOperator.Divide => EvaluateDivision(leftValue, rightValue, binary),
            BinaryOperator.Modulo => ToNumber(leftValue, binary.Left.Span) % ToNumber(rightValue, binary.Right.Span),

            // Comparison
            BinaryOperator.Equal => AreEqual(leftValue, rightValue),
            BinaryOperator.NotEqual => !AreEqual(leftValue, rightValue),
            BinaryOperator.LessThan => Compare(leftValue, rightValue, binary.Span) < 0,
            BinaryOperator.LessOrEqual => Compare(leftValue, rightValue, binary.Span) <= 0,
            BinaryOperator.GreaterThan => Compare(leftValue, rightValue, binary.Span) > 0,
            BinaryOperator.GreaterOrEqual => Compare(leftValue, rightValue, binary.Span) >= 0,

            _ => throw new TemplateExpressionException(
                $"Unknown binary operator: {binary.Operator}",
                binary.Span
            )
        };
    }

    private object? EvaluateAddition(object? left, object? right, TextSpan span)
    {
        // String concatenation
        if (left is string || right is string) return ToString(left) + ToString(right);

        // Numeric addition
        return ToNumber(left, span) + ToNumber(right, span);
    }

    private object? EvaluateDivision(object? left, object? right, BinaryExpression binary)
    {
        var leftNum = ToNumber(left, binary.Left.Span);
        var rightNum = ToNumber(right, binary.Right.Span);

        if (Math.Abs(rightNum) < double.Epsilon)
            throw new TemplateExpressionException(
                "Division by zero",
                binary.Span
            );

        return leftNum / rightNum;
    }

    private object? EvaluateConditional(ConditionalExpression conditional)
    {
        var condition = Evaluate(conditional.Condition);

        return IsTruthy(condition)
            ? Evaluate(conditional.ThenExpression)
            : Evaluate(conditional.ElseExpression);
    }

    /// <summary>
    /// Evaluate a template expression string
    /// </summary>
    public static object? Evaluate(string expression, EvaluationContext context)
    {
        var parsed = TemplateExpressionParser.Parse(expression);
        var evaluator = new TemplateExpressionEvaluator(context);
        return evaluator.Evaluate(parsed);
    }
}