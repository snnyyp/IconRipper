using System.ComponentModel;
using System.Runtime.InteropServices;

namespace IconRipper.Interop.Managed;

public sealed class ManagedClipboard : SafeHandle
{
    public override bool IsInvalid => false;

    public ManagedClipboard(nint owner) : base(0, true)
    {
        if (!User32.OpenClipboard(owner))
            throw new Win32Exception();
    }

    protected override bool ReleaseHandle() => User32.CloseClipboard();

    public bool Empty() => User32.EmptyClipboard();

    private uint RegisterFormat(string format)
    {
        var formatId = User32.RegisterClipboardFormat(format);

        return formatId == nint.Zero ? throw new Win32Exception() : formatId;
    }

    private void SetData(User32.ClipboardDataFormats format, nint bufPtr)
    {
        if (User32.SetClipboardData(format, bufPtr) != bufPtr)
        {
            Marshal.FreeHGlobal(bufPtr);

            throw new Win32Exception();
        }
    }

    public void SetData(User32.ClipboardDataFormats format)
    {
        SetData(format, nint.Zero);
    }

    public void SetData(string format)
    {
        SetData((User32.ClipboardDataFormats)RegisterFormat(format), nint.Zero);
    }

    public void SetData(User32.ClipboardDataFormats format, MemoryStream data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var buf = data.ToArray();
        var bufPtr = Marshal.AllocHGlobal(buf.Length);

        Marshal.Copy(buf, 0, bufPtr, buf.Length);

        SetData(format, bufPtr);
    }

    public void SetData(string format, MemoryStream data)
    {
        ArgumentNullException.ThrowIfNull(data);

        SetData((User32.ClipboardDataFormats)RegisterFormat(format), data);
    }
}