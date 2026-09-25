using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using IconRipper.Interop.Managed;

namespace IconRipper.Interop;

public sealed partial class IcoBitmapEncoder
{
    public IList<IcoFrame> Frame { get; } = new List<IcoFrame>();

    public async Task SaveAsync(Stream stream, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        token.ThrowIfCancellationRequested();
        var iconDir = new ICONDIR
        {
            idType = 1,
            idCount = (ushort)Frame.Count,
        };

        token.ThrowIfCancellationRequested();
        stream.Write(MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref iconDir, 1)));

        token.ThrowIfCancellationRequested();
        var ofs = (uint)(Marshal.SizeOf<ICONDIR>() + Frame.Count * Marshal.SizeOf<ICONDIRENTRY>());

        foreach (var frame in Frame)
        {
            //
            ArgumentNullException.ThrowIfNull(frame);
            ArgumentNullException.ThrowIfNull(frame.Module);

            token.ThrowIfCancellationRequested();

            if (frame.Module.IsClosed)
                throw new InvalidOperationException();

            //
            var iconDirEntry = new ICONDIRENTRY(frame.IconDirEntry)
            {
                dwImageOffset = ofs,
            };

            token.ThrowIfCancellationRequested();
            stream.Write(MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref iconDirEntry, 1)));

            ofs += iconDirEntry.dwBytesInRes;
        }

        foreach (var frame in Frame)
        {
            token.ThrowIfCancellationRequested();
            var iconInfo = frame.Module.GetResInfo(Kernel32.RT_ICON, new PeResourceKey(frame.IconDirEntry.nId));

            UnmanagedMemoryStream ums;

            unsafe
            {
                ums = new UnmanagedMemoryStream((byte*)iconInfo.GetRes(), iconInfo.GetSize());
            }

            await using (ums)
            {
                await ums.CopyToAsync(stream, token).ConfigureAwait(false);
            }
        }
    }

    public unsafe void Save(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var iconDir = new ICONDIR
        {
            idType = 1,
            idCount = (ushort)Frame.Count,
        };

        stream.Write(MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref iconDir, 1)));
        var ofs = (uint)(Marshal.SizeOf<ICONDIR>() + Frame.Count * Marshal.SizeOf<ICONDIRENTRY>());

        foreach (var frame in Frame)
        {
            //
            ArgumentNullException.ThrowIfNull(frame);
            ArgumentNullException.ThrowIfNull(frame.Module);

            if (frame.Module.IsClosed)
                throw new InvalidOperationException();

            //
            var iconDirEntry = new ICONDIRENTRY(frame.IconDirEntry)
            {
                dwImageOffset = ofs,
            };

            stream.Write(MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref iconDirEntry, 1)));

            ofs += iconDirEntry.dwBytesInRes;
        }

        foreach (var frame in Frame)
        {
            var iconInfo = frame.Module.GetResInfo(Kernel32.RT_ICON, new PeResourceKey(frame.IconDirEntry.nId));

            stream.Write(new ReadOnlySpan<byte>(iconInfo.GetRes().ToPointer(), (int)iconInfo.GetSize()));
        }
    }
}

public sealed partial class IcoBitmapEncoder
{
    private readonly static byte[] PngHeader = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    public enum FrameFormat
    {
        DIB,
        PNG,
        UNKNOWN,
    }

    public record IcoFrame
    {
        //
        public required ManagedModule Module { get; init; }
        public required Kernel32.GRPICONDIRENTRY IconDirEntry { get; init; }

        //
        private readonly Lazy<FrameFormat> _format;
        public FrameFormat Format => _format.Value;

        public unsafe IcoFrame()
        {
            _format = new Lazy<FrameFormat>(() =>
            {
                var iconInfo = Module.GetResInfo(Kernel32.RT_ICON, new PeResourceKey(IconDirEntry.nId));
                var header = new ReadOnlySpan<byte>(iconInfo.GetRes().ToPointer(), PngHeader.Length);

                return header.StartsWith(PngHeader) ? FrameFormat.PNG : FrameFormat.DIB;
            });
        }

        [SetsRequiredMembers]
        public IcoFrame(ManagedModule module, Kernel32.GRPICONDIRENTRY iconDirEntry) : this()
        {
            Module = module;
            IconDirEntry = iconDirEntry;
        }
    }

    public struct ICONDIRENTRY
    {
        public byte bWidth;
        public byte bHeight;
        public byte bColorCount;
        private readonly byte bReserved = 0;
        public ushort wPlanes;
        public ushort wBitCount;
        public uint dwBytesInRes;
        public uint dwImageOffset;

        //
        public readonly int Width => unchecked((byte)(bWidth - 1)) + 1;
        public readonly int Height => unchecked((byte)(bHeight - 1)) + 1;

        public ICONDIRENTRY()
        {
        }

        public ICONDIRENTRY(Kernel32.GRPICONDIRENTRY grpIconDirEntry) : this()
        {
            bWidth = unchecked((byte)grpIconDirEntry.Width);
            bHeight = unchecked((byte)grpIconDirEntry.Height);
            bColorCount = grpIconDirEntry.bColorCount;
            wPlanes = grpIconDirEntry.wPlanes;
            wBitCount = grpIconDirEntry.wBitCount;
            dwBytesInRes = grpIconDirEntry.dwBytesInRes;
            // dwImageOffset
        }
    }

    public struct ICONDIR
    {
        private readonly ushort idReserved = 0;
        public ushort idType; // 1 for icons, 2 for cursors
        public ushort idCount;
        // public ICONDIRENTRY[] idEntries;

        public ICONDIR()
        {
        }

        public ICONDIR(Kernel32.GRPICONDIR grpIconDir) : this()
        {
            idType = grpIconDir.idType;
            idCount = grpIconDir.idCount;
        }
    }
}

public sealed partial class IcoBitmapEncoder
{
    public enum V5BitCount : ushort
    {
        BC0 = 0,
        BC1 = 1,
        BC4 = 4,
        BC8 = 8,
        BC16 = 16,
        BC24 = 24,
        BC32 = 32,
    }

    public enum V5Compression : uint
    {
        BI_RGB = 0,
        BI_RLE8 = 1,
        BI_RLE4 = 2,
        BI_BITFIELDS = 3,
        BI_JPEG = 4,
        BI_PNG = 5,
    }

    public enum V5ColorSpace : uint
    {
        LCS_CALIBRATED_RGB = 0x00000000,
        LCS_sRGB = 0x73524742,
        LCS_WINDOWS_COLOR_SPACE = 0x57696E20,
        PROFILE_LINKED = 0x4c494e4b,
        PROFILE_EMBEDDED = 0x4d424544,
    }

    public struct BITMAPV5HEADER()
    {
        public readonly uint bV5Size = (uint)Marshal.SizeOf<BITMAPV5HEADER>();
        public int bV5Width;
        public int bV5Height;
        public readonly ushort bV5Planes = 1;
        public V5BitCount bV5BitCount;
        public V5Compression bV5Compression;
        public uint bV5SizeImage;
        public int bV5XPelsPerMeter;
        public int bV5YPelsPerMeter;
        public uint bV5ClrUsed;
        public uint bV5ClrImportant;
        public uint bV5RedMask;
        public uint bV5GreenMask;
        public uint bV5BlueMask;
        public uint bV5AlphaMask;
        public V5ColorSpace bV5CSType;
        public int bV5Endpoints1 = 0;
        public int bV5Endpoints2 = 0;
        public int bV5Endpoints3 = 0;
        public int bV5Endpoints4 = 0;
        public int bV5Endpoints5 = 0;
        public int bV5Endpoints6 = 0;
        public int bV5Endpoints7 = 0;
        public int bV5Endpoints8 = 0;
        public int bV5Endpoints9 = 0;
        public uint bV5GammaRed;
        public uint bV5GammaGreen;
        public uint bV5GammaBlue;
        public uint bV5Intent;
        public uint bV5ProfileData;
        public uint bV5ProfileSize;
        private readonly uint bV5Reserved = 0;
    }
}