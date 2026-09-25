using System.Runtime.InteropServices;
using System.Text;
using IconRipper.Interop.Managed;

namespace IconRipper.Interop;

public static partial class Kernel32
{
    [LibraryImport("kernel32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true,
        EntryPoint = "LoadLibraryExW")]
    internal static partial nint LoadLibraryEx(
        string libFileName,
        nint hFile,
        LoadLibraryExFlags dwFlags
    );

    [LibraryImport("kernel32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true,
        EntryPoint = "FreeLibrary")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool FreeLibrary(
        nint hLibModule
    );

    [LibraryImport("kernel32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true,
        EntryPoint = "EnumResourceTypesW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool EnumResourceTypes(
        ManagedModule hModule,
        ENUMRESTYPEPROCW lpEnumFunc,
        nint lParam
    );

    [LibraryImport("kernel32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true,
        EntryPoint = "EnumResourceNamesW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool EnumResourceNames(
        ManagedModule hModule,
        PeResourceKey lpType,
        ENUMRESNAMEPROCW lpEnumFunc,
        nint lParam
    );

    [LibraryImport("kernel32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true,
        EntryPoint = "FindResourceW")]
    internal static partial nint FindResource(
        ManagedModule hModule,
        PeResourceKey lpName,
        PeResourceKey lpType
    );

    [LibraryImport("kernel32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    internal static partial nint LoadResource(
        ManagedModule hModule,
        nint hResInfo
    );

    [LibraryImport("kernel32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    internal static partial uint SizeofResource(
        ManagedModule hModule,
        nint hResInfo
    );

    [LibraryImport("kernel32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    internal static partial nint LockResource(
        nint hResData
    );

    [LibraryImport("kernel32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true,
        EntryPoint = "ExpandEnvironmentStringsW")]
    internal static partial uint ExpandEnvironmentStrings(
        string lpSrc,
        Span<char> lpDst,
        uint nSize
    );
}