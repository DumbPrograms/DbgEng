using System.Runtime.InteropServices;
using System.Text;

namespace Interop.DbgEng;

public static class SpanExtensions
{
    public static string GetString(this Span<byte> buffer, uint filledSize)
    {
        return Encoding.ASCII.GetString(buffer[..(int)(filledSize - 1)]);
    }

    public static string GetString(this ReadOnlySpan<byte> buffer, uint filledSize)
    {
        return Encoding.ASCII.GetString(buffer[..(int)(filledSize - 1)]);
    }

    public static string GetString(this Span<char> buffer, uint filledSize)
    {
        return Encoding.Unicode.GetString(MemoryMarshal.AsBytes(buffer[..(int)(filledSize - 1)]));
    }

    public static string GetString(this ReadOnlySpan<char> buffer, uint filledSize)
    {
        return Encoding.Unicode.GetString(MemoryMarshal.AsBytes(buffer[..(int)(filledSize - 1)]));
    }
}
