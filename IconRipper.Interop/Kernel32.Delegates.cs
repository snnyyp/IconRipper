namespace IconRipper.Interop;

public static partial class Kernel32
{
    public delegate bool ENUMRESTYPEPROCW(
        nint hModule,
        nint lpType,
        nint lParam
    );

    public delegate bool ENUMRESNAMEPROCW(
        nint hModule,
        nint lpType,
        nint lpName,
        nint lParam
    );
}