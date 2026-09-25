using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace IconRipper.Interop;

public static partial class Shell32
{
    [GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
    [Guid(IID_IPersistFile)]
    internal partial interface IPersistFile
    {
        void GetClassID(out Guid pClassID);

        [PreserveSig]
        int IsDirty();

        void Load(string pszFileName, uint dwMode);
        void Save(string pszFileName, [MarshalAs(UnmanagedType.Bool)] bool fRemember);
        void SaveCompleted(string pszFileName);
        void GetCurFile(out string ppszFileName);
    }

    [GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid(IID_IShellLinkW)]
    internal unsafe partial interface IShellLinkW
    {
        void GetPath(char* pszFile, int cchMaxPath, nint pfd, uint fFlags);
        void GetIDList(out nint ppidl);
        void SetIDList(nint pidl);
        void GetDescription(char* pszName, int cchMaxName);
        void SetDescription(string pszName);
        void GetWorkingDirectory(char* pszDir, int cchMaxPath);
        void SetWorkingDirectory(string pszDir);
        void GetArguments(char* pszArgs, int cchMaxPath);
        void SetArguments(string pszArgs);
        void GetHotkey(out short pwHotkey);
        void SetHotkey(short wHotkey);
        void GetShowCmd(out int piShowCmd);
        void SetShowCmd(int iShowCmd);
        void GetIconLocation(char* pszIconPath, int cchIconPath, out int piIcon);
        void SetIconLocation(string pszIconPath, int iIcon);
        void SetRelativePath(string pszPathRel, uint dwReserved);
        void Resolve(nint hwnd, uint fFlags);
        void SetPath(string pszFile);
    }
}