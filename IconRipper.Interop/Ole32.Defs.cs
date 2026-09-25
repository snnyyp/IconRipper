using System.Runtime.InteropServices;

namespace IconRipper.Interop;

public static partial class Ole32
{
    [LibraryImport("ole32.dll")]
    internal static partial int CoCreateInstance(in Guid rclsid, nint pUnkOuter, uint dwClsContext,
        in Guid riid, out nint ppv);
}