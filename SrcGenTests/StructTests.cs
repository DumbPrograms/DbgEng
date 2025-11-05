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
            """
            typedef struct _DEBUG_OFFSET_REGION
            {
                ULONG64 Base;
                ULONG64 Size;
            } DEBUG_OFFSET_REGION, *PDEBUG_OFFSET_REGION;
            """,
            "");
    }
}
