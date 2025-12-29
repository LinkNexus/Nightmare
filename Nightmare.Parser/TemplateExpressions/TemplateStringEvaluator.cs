namespace Nightmare.Parser.TemplateExpressions;

/// <summary>
/// Utility class to evaluate TemplateString instances from the JSON parser
/// </summary>
public static class TemplateStringEvaluator
{
    /// <summary>
    /// Evaluate all expressions in a TemplateString and return the final string
    /// </summary>
    public static string Evaluate(TemplateString template, EvaluationContext context)
    {
        var segments = template.GetSegments();
        var result = new System.Text.StringBuilder();

        foreach (var segment in segments)
            switch (segment)
            {
                case TemplateTextSegment text:
                    result.Append(text.Text);
                    break;

                case TemplateExpressionSegment expression:
                    {
                        try
                        {
                            var value = TemplateExpressionEvaluator.Evaluate(
                                expression.Expression,
                                context
                            );
                            result.Append(value?.ToString() ?? "null");
                        }
                        catch (TemplateExpressionException ex)
                        {
                            // Re-throw with the correct span from the segment
                            throw new TemplateExpressionException(ex.Message, expression.Span);
                        }

                        break;
                    }
            }

        return result.ToString();
    }

    /// <summary>
    /// Evaluate a TemplateString. If it's a single expression segment, return the raw evaluated value;
    /// otherwise return the concatenated string result (for mixed text + expressions).
    /// </summary>
    public static object? EvaluateValue(TemplateString template, EvaluationContext context)
    {
        var segments = template.GetSegments();

        if (segments is not [TemplateExpressionSegment expression]) return Evaluate(template, context);

        try
        {
            return TemplateExpressionEvaluator.Evaluate(expression.Expression, context);
        }
        catch (TemplateExpressionException ex)
        {
            // Re-throw with the correct span from the segment
            throw new TemplateExpressionException(ex.Message, expression.Span);
        }
    }

    /// <summary>
    /// Check if a TemplateString can be evaluated without errors
    /// </summary>
    public static bool TryEvaluate(
        TemplateString template,
        EvaluationContext context,
        out string? result,
        out TemplateExpressionException? error)
    {
        try
        {
            result = Evaluate(template, context);
            error = null;
            return true;
        }
        catch (TemplateExpressionException ex)
        {
            result = null;
            error = ex;
            return false;
        }
    }

    /// <summary>
    /// Parse all expressions in a TemplateString to validate syntax
    /// </summary>
    public static void ValidateSyntax(TemplateString template)
    {
        var segments = template.GetSegments();

        foreach (var segment in segments)
            if (segment is TemplateExpressionSegment expression)
                try
                {
                    // Just parse, don't evaluate
                    TemplateExpressionParser.Parse(expression.Expression);
                }
                catch (TemplateExpressionException ex)
                {
                    // Re-throw with the correct span from the segment
                    throw new TemplateExpressionException(ex.Message, expression.Span);
                }
    }

    /// <summary>
    /// Get all variable names referenced in a TemplateString
    /// </summary>
    public static IEnumerable<string> GetReferencedVariables(TemplateString template)
    {
        var segments = template.GetSegments();
        var variables = new HashSet<string>();

        foreach (var segment in segments)
            if (segment is TemplateExpressionSegment expression)
                try
                {
                    var ast = TemplateExpressionParser.Parse(expression.Expression);
                    CollectVariables(ast, variables);
                }
                catch
                {
                    // Ignore parse errors when collecting variables
                }

        return variables;
    }

    private static void CollectVariables(TemplateExpression expr, HashSet<string> variables)
    {
        switch (expr)
        {
            case IdentifierExpression identifier:
                variables.Add(identifier.Name);
                break;

            case MemberAccessExpression member:
                CollectVariables(member.Target, variables);
                break;

            case IndexAccessExpression index:
                CollectVariables(index.Target, variables);
                CollectVariables(index.Index, variables);
                break;

            case CallExpression call:
                CollectVariables(call.Callee, variables);
                foreach (var arg in call.Arguments)
                    CollectVariables(arg, variables);
                break;

            case UnaryExpression unary:
                CollectVariables(unary.Operand, variables);
                break;

            case BinaryExpression binary:
                CollectVariables(binary.Left, variables);
                CollectVariables(binary.Right, variables);
                break;

            case ConditionalExpression conditional:
                CollectVariables(conditional.Condition, variables);
                CollectVariables(conditional.ThenExpression, variables);
                CollectVariables(conditional.ElseExpression, variables);
                break;
        }
    }
}