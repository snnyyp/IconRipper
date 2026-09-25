using System.Collections.Frozen;
using System.ComponentModel;
using Microsoft.Win32.SafeHandles;

namespace IconRipper.Interop.Managed;

public sealed class ManagedModule : SafeHandleZeroOrMinusOneIsInvalid, IEquatable<ManagedModule>
{
    public string LibPath { get; }

    public ManagedModule(string libPath, Kernel32.LoadLibraryExFlags flags) : base(true)
    {
        var module = Kernel32.LoadLibraryEx(libPath, nint.Zero, flags);

        if (module != nint.Zero)
            SetHandle(module);
        else
            throw new Win32Exception();

        LibPath = libPath;
    }

    protected override bool ReleaseHandle()
    {
        return Kernel32.FreeLibrary(handle);
    }

    public FrozenSet<PeResourceKey> GetResTypes()
    {
        var result = new List<PeResourceKey>();

        var err = Kernel32.EnumResourceTypes(this, (_, type, _) =>
        {
            result.Add(new PeResourceKey(type));

            return true;
        }, nint.Zero);

        return err ? result.ToFrozenSet() : throw new Win32Exception();
    }

    public FrozenSet<PeResourceKey> GetResNames(PeResourceKey resType)
    {
        var result = new List<PeResourceKey>();

        var err = Kernel32.EnumResourceNames(this, resType, (_, _, name, _) =>
        {
            result.Add(new PeResourceKey(name));

            return true;
        }, nint.Zero);

        return err ? result.ToFrozenSet() : throw new Win32Exception();
    }

    public PeResourceInfo GetResInfo(PeResourceKey resType, PeResourceKey resName)
    {
        return new PeResourceInfo(this, resType, resName);
    }

    #region

    public override int GetHashCode()
    {
        return handle.GetHashCode();
    }

    public override string ToString()
    {
        return $"ManagedModule[lib={LibPath}, handle={handle}]";
    }

    public bool Equals(ManagedModule? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return handle == other.handle;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;

        return obj is ManagedModule other && Equals(other);
    }

    public static bool operator ==(ManagedModule left, ManagedModule right) => left.Equals(right);
    public static bool operator !=(ManagedModule left, ManagedModule right) => !left.Equals(right);

    #endregion

}