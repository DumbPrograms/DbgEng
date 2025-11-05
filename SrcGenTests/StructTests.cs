namespace SrcGenTests;

public class StructTests : TestsBase
{
    [Fact]
    public void TestStruct1()
    {
        AssertGenerated("""
            namespace Interop.DbgEng;

            public struct DebugOffsetRegion
            {
                public ULONG64  Base;
                public ULONG64  Size;
            }
            """,
            hppSrc: """
            typedef struct _DEBUG_OFFSET_REGION
            {
                ULONG64 Base;
                ULONG64 Size;
            } DEBUG_OFFSET_REGION, *PDEBUG_OFFSET_REGION;
            """,
            "");
    }

    [Fact]
    public void TestStruct2()
    {
        AssertGenerated("""
            namespace Interop.DbgEng;

            public struct DebugOffsetRegion
            {
                public ULONG64  Base;
                public ULONG64  Size;
            }
            """,
            "",
            missingSrc: """
            typedef struct _DEBUG_OFFSET_REGION
            {
                ULONG64 Base;
                ULONG64 Size;
            } DEBUG_OFFSET_REGION, *PDEBUG_OFFSET_REGION;
            """);
    }

    [Fact]
    public void TestConstant1()
    {
        AssertGenerated("""
            namespace Interop.DbgEng;

            public static partial class Constants
            {
                public const UINT32 X = 0;
            }
            """,
            hppSrc: """
            #define X 0
            """,
            "");
    }

    [Fact]
    public void TestConstant2()
    {
        AssertGenerated("""
            namespace Interop.DbgEng;

            public static partial class Constants
            {
                public const UINT32 X = 0;
            }
            """,
            "",
            missingSrc: """
            #define X 0
            """);
    }

}
