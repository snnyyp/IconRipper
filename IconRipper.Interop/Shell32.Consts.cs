namespace IconRipper.Interop;

public static partial class Shell32
{
    internal const uint SLR_NO_UI = 0x1;
    internal const uint SLR_NOSEARCH = 0x10;

    internal const string CLSID_ShellLink = "00021401-0000-0000-C000-000000000046";
    internal const string IID_IShellLinkW = "000214F9-0000-0000-C000-000000000046";
    internal const string IID_IPersistFile = "0000010b-0000-0000-C000-000000000046";

    internal readonly static Guid CLSID_ShellLink_G = new("00021401-0000-0000-C000-000000000046");
    internal readonly static Guid IID_IShellLinkW_G = new("000214F9-0000-0000-C000-000000000046");
}