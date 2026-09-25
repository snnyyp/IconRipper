using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace IconRipper.Interop;

public static partial class Shell32
{
    public static string GetLnkShortcutTargetPath(string lnkPath)
    {
        var errHr = Ole32.CoCreateInstance(CLSID_ShellLink_G, nint.Zero, 1,
            IID_IShellLinkW_G, out var comObj);

        Marshal.ThrowExceptionForHR(errHr);

        try
        {
            var wrappers = new StrategyBasedComWrappers();
            var comLink = (IShellLinkW)wrappers.GetOrCreateObjectForComInstance(comObj, CreateObjectFlags.Unwrap);
            var comFile = (IPersistFile)comLink;

            comFile.Load(lnkPath, (int)STGM.STGM_READ);
            comLink.Resolve(nint.Zero, SLR_NO_UI | SLR_NOSEARCH);

            unsafe
            {
                var sb = stackalloc char[Kernel32.MAX_PATH];
                comLink.GetPath(sb, Kernel32.MAX_PATH, nint.Zero, 0);

                return new string(sb);
            }
        }
        finally
        {
            Marshal.Release(comObj);
        }
    }
}