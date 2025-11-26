namespace SrcGen;

internal static class TextReaderExtensions
{
    public static string? SeekLineWithPrefix(this TextReader reader, string prefix, bool ignoreLeadingSpaces = false)
    {
        while (reader.Peek() > -1)
        {
            var line = reader.ReadLine();
            var span = line.AsSpan();

            if (ignoreLeadingSpaces)
            {
                span = span.TrimStart();
            }

            if (span.StartsWith(prefix))
            {
                return line;
            }
        }

        return null;
    }

}
