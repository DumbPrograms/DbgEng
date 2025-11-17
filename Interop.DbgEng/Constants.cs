using System.Reflection;
using System.Runtime.InteropServices.Marshalling;

namespace Interop.DbgEng;

public static partial class Constants
{
    public static Guid GetIid<IComInterface>()
    {
        var type = typeof(IComInterface);

        if (type.IsInterface && type.GetCustomAttribute(typeof(IUnknownDerivedAttribute<,>)) is IIUnknownDerivedDetails details)
        {
            return details.Iid;
        }

        throw new ArgumentException("Expect a source generated COM interface.", nameof(IComInterface));
    }
}
