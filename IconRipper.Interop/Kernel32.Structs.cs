using System.Runtime.InteropServices;

namespace IconRipper.Interop;

public static partial class Kernel32
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 2)]
    public struct GRPICONDIRENTRY : IEquatable<GRPICONDIRENTRY>
    {
        public byte bWidth;
        public byte bHeight;
        public byte bColorCount;
        private byte bReserved = 0;
        public ushort wPlanes;
        public ushort wBitCount;
        public uint dwBytesInRes;
        public ushort nId;

        //
        public readonly int Width => unchecked((byte)(bWidth - 1)) + 1;
        public readonly int Height => unchecked((byte)(bHeight - 1)) + 1;

        public GRPICONDIRENTRY()
        {
        }

        public bool Equals(GRPICONDIRENTRY other)
        {
            return bWidth == other.bWidth
                   && bHeight == other.bHeight
                   && bColorCount == other.bColorCount
                   && wPlanes == other.wPlanes
                   && wBitCount == other.wBitCount
                   && dwBytesInRes == other.dwBytesInRes
                   && nId == other.nId;
        }

        public override bool Equals(object? obj)
        {
            return obj is GRPICONDIRENTRY other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(bWidth, bHeight, bColorCount, wPlanes, wBitCount, dwBytesInRes, nId);
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 2)]
    public unsafe struct GRPICONDIR : IEquatable<GRPICONDIR>
    {
        private ushort idReserved = 0;
        public ushort idType;
        public ushort idCount;
        private fixed byte _idEntries[1];

        public readonly ReadOnlySpan<GRPICONDIRENTRY> idEntries
        {
            get
            {
                fixed (byte* ptr = _idEntries)
                {
                    return new ReadOnlySpan<GRPICONDIRENTRY>(ptr, idCount);
                }
            }
        }

        public GRPICONDIR()
        {
        }

        public bool Equals(GRPICONDIR other)
        {
            fixed (byte* ptr = _idEntries)
            {
                return idType == other.idType
                       && idCount == other.idCount
                       && ptr == other._idEntries;
            }
        }

        public override bool Equals(object? obj)
        {
            return obj is GRPICONDIR other && Equals(other);
        }

        public override int GetHashCode()
        {
            fixed (byte* ptr = _idEntries)
            {
                return HashCode.Combine(idType, idCount, unchecked((int)(long)ptr));
            }
        }
    }
}