using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace IconRipper.Interop.Managed;

public sealed class PeResourceKey : SafeHandleZeroOrMinusOneIsInvalid, IEquatable<PeResourceKey>
{
    //
    public static bool IsIntResource(nint handle) => (ulong)handle >> 16 == 0;

    internal PeResourceKey(nint handle) : base(!IsIntResource(handle))
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(handle);

        if (IsIntResource(handle))
        {
            SetHandle(handle);

            return;
        }

        //
        var s = Marshal.PtrToStringUni(handle)!;

        SetHandle(Marshal.StringToHGlobalUni(s));
    }

    public PeResourceKey(string key) : base(true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        SetHandle(Marshal.StringToHGlobalUni(key));
    }

    public PeResourceKey(int key) : base(false)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(key);

        SetHandle(key);
    }

    protected override bool ReleaseHandle()
    {
        Marshal.FreeHGlobal(handle);
        Debug.WriteLine("PeResourceTypeOrName disposed");

        return true;
    }

    #region

    public override int GetHashCode()
    {
        return handle.GetHashCode();
    }

    public string ToString(bool wrap = true)
    {
        var s = IsIntResource(handle) ? $"#{handle}" : Marshal.PtrToStringUni(handle)!;

        return wrap ? $"PrResourceKey[key={s}]" : s;
    }

    public override string ToString()
    {
        return ToString();
    }

    public bool Equals(PeResourceKey? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return handle == other.handle;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;

        return obj is PeResourceKey other && Equals(other);
    }

    public static bool operator ==(PeResourceKey left, PeResourceKey right) => left.Equals(right);
    public static bool operator !=(PeResourceKey left, PeResourceKey right) => !left.Equals(right);

    #endregion

}