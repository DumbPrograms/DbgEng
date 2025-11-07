using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Interop.DbgEng;

public partial interface IDebugClient
{
    static IDebugClient Create()
    {
        DbgEngApi.DebugCreate(new Guid(Constants.IID_IDebugClient), out var pDebugClient);

        var cw = new StrategyBasedComWrappers();

        return (IDebugClient)cw.GetOrCreateObjectForComInstance(pDebugClient, CreateObjectFlags.UniqueInstance);
    }
}
