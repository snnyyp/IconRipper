using System.ComponentModel;

namespace IconRipper.Interop.Managed;

public readonly struct PeResourceInfo : IEquatable<PeResourceInfo>
{
    private readonly ManagedModule _module;
    private readonly PeResourceKey _resType;
    private readonly PeResourceKey _resName;
    private readonly nint _resInfo;

    public PeResourceInfo(ManagedModule module, PeResourceKey resType, PeResourceKey resName)
    {
        ArgumentNullException.ThrowIfNull(module);
        ArgumentNullException.ThrowIfNull(resType);
        ArgumentNullException.ThrowIfNull(resName);

        _module = module;
        _resType = resType;
        _resName = resName;

        //
        _resInfo = Kernel32.FindResource(_module, _resName, _resType);

        if (_resInfo == nint.Zero)
            throw new Win32Exception();
    }

    public uint GetSize()
    {
        var size = Kernel32.SizeofResource(_module, _resInfo);

        return size != 0 ? size : throw new Win32Exception();
    }

    public nint GetRes()
    {
        var resData = Kernel32.LoadResource(_module, _resInfo);

        if (resData == nint.Zero)
            throw new Win32Exception();

        var lockedResData = Kernel32.LockResource(resData);

        return lockedResData != nint.Zero ? lockedResData : throw new Win32Exception();
    }

    public unsafe ref readonly T GetRes<T>()
    {
        return ref new ReadOnlySpan<T>(GetRes().ToPointer(), 1)[0];
    }

    #region

    public override string ToString()
    {
        return $"PeResourceInfo[module={_module}, resType={_resType}, resName={_resName}, resInfo={_resInfo}]";
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_module, _resType, _resName, _resInfo);
    }

    public bool Equals(PeResourceInfo other)
    {
        return _module.Equals(other._module)
               && _resType.Equals(other._resType)
               && _resName.Equals(other._resName)
               && _resInfo.Equals(other._resInfo);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;

        return obj is PeResourceInfo other && Equals(other);
    }

    public static bool operator ==(PeResourceInfo left, PeResourceInfo right) => left.Equals(right);
    public static bool operator !=(PeResourceInfo left, PeResourceInfo right) => !left.Equals(right);

    #endregion

}