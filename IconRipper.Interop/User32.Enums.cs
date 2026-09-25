namespace IconRipper.Interop;

public static partial class User32
{
    public enum ClipboardDataFormats : uint
    {
        CF_TEXT = 1,
        CF_BITMAP = 2,
        CF_METAFILEPICT = 3,
        CF_SYLK = 4,
        CF_DIF = 5,
        CF_TIFF = 6,
        CF_OEMTEXT = 7,
        CF_DIB = 8,
        CF_PALETTE = 9,
        CF_PENDATA = 10,
        CF_RIFF = 11,
        CF_WAVE = 12,
        CF_UNICODETEXT = 13,
        CF_ENHMETAFILE = 14,
        CF_HDROP = 15,
        CF_LOCALE = 16,
        CF_DIBV5 = 17,
    }

    public enum VirtualKeyCodes
    {
        VK_LBUTTON = 0x01,
        VK_RBUTTON = 0x02,
        VK_CONTROL = 0x11,
    }
}