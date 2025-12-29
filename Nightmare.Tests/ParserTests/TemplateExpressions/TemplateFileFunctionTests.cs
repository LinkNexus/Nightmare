using System.IO;
using Nightmare.Parser;
using Nightmare.Parser.TemplateExpressions;
using Xunit;

namespace Nightmare.Tests.ParserTests.TemplateExpressions;

public class TemplateFileFunctionTests
{
    [Fact]
    public void FileFunction_Returns_FileReference()
    {
        var tmp = Path.GetTempFileName();
        File.WriteAllText(tmp, "hello");

        try
        {
            var ctx = new EvaluationContext();
            var result = TemplateExpressionEvaluator.Evaluate($"file('{tmp.Replace("\\", "\\\\")}')", ctx);

            Assert.NotNull(result);
            Assert.IsType<FileReference>(result);
            Assert.Equal(tmp, ((FileReference)result).Path);
        }
        finally
        {
            File.Delete(tmp);
        }
    }

    [Fact]
    public void EvaluateValue_Preserves_FileReference_For_Single_Expression()
    {
        var tmp = Path.GetTempFileName();
        File.WriteAllText(tmp, "hello");

        try
        {
            var template = new TemplateString([
                new TemplateExpressionSegment($"file('{tmp.Replace("\\", "\\\\")}')", new TextSpan(0, 0, 1, 1, 1, 1))
            ]);

            var ctx = new EvaluationContext();
            var value = TemplateStringEvaluator.EvaluateValue(template, ctx);

            Assert.NotNull(value);
            Assert.IsType<FileReference>(value);
            Assert.Equal(tmp, ((FileReference)value).Path);
        }
        finally
        {
            File.Delete(tmp);
        }
    }
}
