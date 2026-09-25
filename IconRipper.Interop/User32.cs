namespace IconRipper.Interop;

public static partial class User32
{
    public static bool IsKeyDownNow(VirtualKeyCodes keyCode)
    {
        return GetAsyncKeyState(keyCode) < 0;
    }
}