using IconRipper.Interop.Managed;

namespace IconRipper.Interop;

public static partial class Kernel32
{
    public readonly static PeResourceKey RT_ICON = new(3);
    public readonly static PeResourceKey RT_GROUP_ICON = new(3 + 11);

    public const int MAX_PATH = 260;
}