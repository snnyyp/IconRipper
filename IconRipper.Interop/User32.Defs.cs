using System.Runtime.InteropServices;

namespace IconRipper.Interop;

public partial class User32
{
    [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool OpenClipboard(nint hWndNewOwner);

    [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool CloseClipboard();

    [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    internal static partial nint SetClipboardData(ClipboardDataFormats uFormat, nint hMem);

    [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool EmptyClipboard();

    [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true,
        EntryPoint = "RegisterClipboardFormatW")]
    internal static partial uint RegisterClipboardFormat(string lpszFormat);

    [LibraryImport("user32.dll")]
    internal static partial short GetAsyncKeyState(VirtualKeyCodes vKey);
}