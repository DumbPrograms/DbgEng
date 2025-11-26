using System.Text;
using SrcGen;

namespace SrcGenTests;

public class TestsBase
{
    protected static void AssertGeneratedWithMissing(string expected, string hppSrc, string missingSrc)
    {
        var hpp = new StringReader(hppSrc);
        var missing = new StringReader(missingSrc);
        var sb = new StringBuilder();

        var translator = new Translator(new StringWriter(sb), Documents.Empty);
        translator.Generate(hpp, missing);

        AssertLinesEqual(expected, sb.ToString());
    }

    protected static void AssertGeneratedWithDocuments(string expected, string hppSrc, params string[] documents)
    {
        var hpp = new StringReader(hppSrc);
        var missing = StreamReader.Null;
        var sb = new StringBuilder();

        var docs = new Documents();
        docs.Parse(documents.Select(text => new StringReader(text)));

        var translator = new Translator(new StringWriter(sb), docs);

        translator.Generate(hpp, missing);

        AssertLinesEqual(expected, sb.ToString());
    }

    protected static void AssertLinesEqual(string expected, string result)
    {
        var resultLines = result.AsSpan().Trim().EnumerateLines();
        var expectLines = (Translator.GeneratedHeader + Environment.NewLine + expected).AsSpan().Trim().EnumerateLines();

        while (expectLines.MoveNext())
        {
            Assert.True(resultLines.MoveNext());
            Assert.Equal(expectLines.Current.Trim(), resultLines.Current.Trim());
        }

        Assert.False(resultLines.MoveNext());
    }
}
