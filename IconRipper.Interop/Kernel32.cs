using System.Buffers;
using System.ComponentModel;

namespace IconRipper.Interop;

public static partial class Kernel32
{
    public static string ExpandEnvironmentStrings(string raw)
    {
        uint bufCharSize = 0;

        for (var _ = 0; _ < 5; _++)
        {
            var buf = bufCharSize switch
            {
                0 => Span<char>.Empty,
                _ => new char[bufCharSize],
            };

            var writtenCharSize = ExpandEnvironmentStrings(raw, buf, bufCharSize);

            if (writtenCharSize == 0)
                throw new Win32Exception();
            else if (writtenCharSize <= bufCharSize)
                return new string(buf[..(int)(writtenCharSize - 1)]);
            else
                bufCharSize = writtenCharSize;
        }

        throw new Win32Exception();
    }
}