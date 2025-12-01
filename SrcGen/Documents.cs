using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace SrcGen;

public class Documents
{
    const string UidPrefix = "UID:";
    const string UidDbgEngPrefix = "Nx:dbgeng.";
    const string UidWinNTPrefix = "Nx:winnt.";
    const string DescriptionPrefix = "description:";

    readonly Dictionary<string, string> TypeSummaries = [];
    readonly Dictionary<string, Dictionary<string, string>> MemberSummaries = [];
    readonly Dictionary<string, List<(bool isOut, string name, string summary)>> Parameters = [];

    public static Documents Empty { get; } = new();

    internal static Documents From(string dir)
    {
        if (!Directory.Exists(dir))
        {
            return Empty;
        }

        var documents = new Documents();

        documents.Parse(Directory.EnumerateFiles(dir, "*.md", SearchOption.AllDirectories).Select(File.OpenText));

        return documents;
    }

    public bool TryGetSummary(ReadOnlySpan<char> type, [MaybeNullWhen(false)] out string summary)
        => TypeSummaries.GetAlternateLookup<ReadOnlySpan<char>>().TryGetValue(type, out summary);

    public bool TryGetSummary(ReadOnlySpan<char> type, ReadOnlySpan<char> member, [MaybeNullWhen(false)] out string summary)
    {
        if (MemberSummaries.GetAlternateLookup<ReadOnlySpan<char>>().TryGetValue(type, out var members)
            && members.GetAlternateLookup<ReadOnlySpan<char>>().TryGetValue(member, out summary))
        {
            return true;
        }

        summary = null;
        return false;
    }

    public bool TryGetParameters(ReadOnlySpan<char> type, ReadOnlySpan<char> method, [MaybeNullWhen(false)] out IReadOnlyList<(bool isOut, string name, string summary)> parameters)
    {
        if (Parameters.GetAlternateLookup<ReadOnlySpan<char>>().TryGetValue([.. type, '.', .. method], out var list))
        {
            parameters = list;
            return true;
        }

        parameters = null;
        return false;
    }

    public void Parse(IEnumerable<TextReader> readers)
    {
        foreach (var reader in readers)
        {
            try
            {
                Parse(reader);
            }
            finally
            {
                reader.Dispose();
            }
        }
    }

    private void Parse(TextReader reader)
    {
        if (reader.SeekLineWithPrefix(UidPrefix) is not string fullLine)
        {
            return;
        }

        var uid = fullLine.AsSpan(UidPrefix.Length).Trim();
        var isDbgeng = uid.Contains(":dbgeng.", StringComparison.Ordinal);

        switch (uid[1])
        {
            case 'A':
            // index page, no use, skip
            case 'C':
            // callback functions, skip for now
            case 'E':
            // enums, not seen yet, skip
            case 'L':
            // IXyzCallbacks base implementations, skip
                return;

            case 'N':
                if (isDbgeng)
                {
                    ParseInterface(uid[UidDbgEngPrefix.Length..], reader);
                }
                return;
            case 'F':
                if (isDbgeng)
                {
                    ParseFunction(uid[UidDbgEngPrefix.Length..], reader);
                }
                return;
            case 'S':
                if (isDbgeng)
                {
                    ParseStruct(uid[(UidDbgEngPrefix.Length + 1)..], reader);
                }
                else if (uid.Contains(":winnt.", StringComparison.Ordinal))
                {
                    ParseStruct(uid[(UidWinNTPrefix.Length + 1)..], reader);
                }
                return;

            default:
                throw new NotImplementedException($"UID of N{uid[1]} is not seen yet.");
        }
    }

    private void ParseInterface(ReadOnlySpan<char> name, TextReader reader)
    {
        var fullLine = reader.SeekLineWithPrefix(DescriptionPrefix);
        TypeSummaries.Add(name.ToString(), fullLine.AsSpan(DescriptionPrefix.Length).Trim().ToString());
    }

    private void ParseFunction(ReadOnlySpan<char> functionName, TextReader reader)
    {
        var dot = functionName.IndexOf('.');
        if (dot < 0)
        {
            // DllExports, we hand write those, skip
            return;
        }

        var fullLine = reader.SeekLineWithPrefix(DescriptionPrefix);
        var summary = fullLine.AsSpan(DescriptionPrefix.Length).Trim().ToString();

        AddMemberSummary(functionName[..dot], functionName[(dot + 1)..].ToString(), summary);

        const string memberHeader = "### -param ";

        if (reader.SeekLineWithPrefix(memberHeader) is string memberLine)
        {
            var parameterName = getParameterName(memberLine, out var isOut);
            var lookup = Parameters.GetAlternateLookup<ReadOnlySpan<char>>();

            if (!lookup.TryGetValue(functionName, out var parameters))
            {
                lookup[functionName] = parameters = [];
            }

            do
            {
                var description = ParseMemberDescription(reader, memberHeader, out var nextMemberLine);

                parameters.Add((isOut, parameterName, description));

                parameterName = getParameterName(nextMemberLine, out isOut);
            }
            while (parameterName is not null);
        }

        [return: NotNullIfNotNull(nameof(memberLine))]
        static string? getParameterName(string? memberLine, out bool isOut)
        {
            isOut = false;

            if (memberLine is null)
            {
                return null;
            }

            var parameterName = memberLine.AsSpan(memberHeader.Length).Trim();

            var space = parameterName.IndexOf(' ');
            if (space > 0)
            {
                isOut = parameterName[space..].Contains("out", StringComparison.Ordinal);
                parameterName = parameterName[..space];
            }

            if (parameterName.SequenceEqual("..."))
            {
                return "Args";
            }

            return parameterName.ToString();
        }
    }

    private void ParseStruct(ReadOnlySpan<char> structName, TextReader reader)
    {
        var fullLine = reader.SeekLineWithPrefix(DescriptionPrefix);
        TypeSummaries.Add(structName.ToString(), fullLine.AsSpan(DescriptionPrefix.Length).Trim().ToString());

        const string memberHeader = "### -field ";

        if (reader.SeekLineWithPrefix(memberHeader) is string memberLine)
        {
            var fieldName = getFieldName(memberLine);
            var lookup = MemberSummaries.GetAlternateLookup<ReadOnlySpan<char>>();

            if (!lookup.TryGetValue(structName, out var fields))
            {
                lookup[structName] = fields = [];
            }

            do
            {
                var description = ParseMemberDescription(reader, memberHeader, out var nextMemberLine);

                fields.Add(fieldName, description);

                fieldName = getFieldName(nextMemberLine);
            }
            while (fieldName is not null);
        }

        [return: NotNullIfNotNull(nameof(memberLine))]
        static string? getFieldName(string? memberLine)
        {
            if (memberLine is null)
            {
                return null;
            }

            var fieldName = memberLine.AsSpan(memberHeader.Length).Trim();

            var square = fieldName.IndexOf('[');
            if (square > 0)
            {
                fieldName = fieldName[..square];
            }

            return fieldName.ToString();
        }
    }

    private static string ParseMemberDescription(TextReader reader, string memberHeader, out string? nextMemberLine)
    {
        var builder = new DefaultInterpolatedStringHandler(512, 0);

        while (reader.ReadLine() is string fullLine)
        {
            if (fullLine.StartsWith(memberHeader))
            {
                nextMemberLine = fullLine;
                goto exit;
            }
            else if (fullLine.StartsWith("## ") || fullLine.StartsWith("# "))
            {
                break;
            }

            builder.AppendLiteral(fullLine);
            builder.AppendLiteral(Environment.NewLine);
        }

        nextMemberLine = null;

    exit:
        var result = builder.Text.Trim().ToString();

        builder.Clear();
        return result;
    }

    private void AddMemberSummary(ReadOnlySpan<char> parent, string child, string summary)
    {
        var lookup = MemberSummaries.GetAlternateLookup<ReadOnlySpan<char>>();

        if (!lookup.TryGetValue(parent, out var members))
        {
            lookup[parent] = members = [];
        }

        members.Add(child, summary);
    }
}