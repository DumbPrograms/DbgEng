using System.Text;

namespace SrcGenTests;

public class TestsBase
{
    protected static void AssertGenerated(string expected, string hppSrc, string missingSrc)
    {
        var hpp = new StringReader(hppSrc);
        var missing = new StringReader(missingSrc);
        var result = new StringBuilder();

        var program = new SrcGen.Program(new StringWriter(result));
        program.Generate(hpp, missing);

        Assert.Equal(expected.AsSpan().Trim(), result.ToString().AsSpan().Trim());
    }
}
