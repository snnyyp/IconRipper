using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using IconRipper.Interop;
using IconRipper.Interop.Managed;

namespace IconRipper.Gui.Models;

public sealed partial record IconInfo
{
    public required ManagedModule Module { get; init; }

    public required string RtGroupIcon { get; init; }

    public required Kernel32.GRPICONDIRENTRY IconDirEntry { get; init; }

    public Bitmap? Icon
    {
        get;
        private set => SetField(ref field, value);
    }

    public IcoBitmapEncoder.FrameFormat Format { get; private set; } = IcoBitmapEncoder.FrameFormat.UNKNOWN;

    public int Width => IconDirEntry.Width;
    public int Height => IconDirEntry.Height;
    public ushort RtIcon => IconDirEntry.nId;
    public string NumberOfColors => IconDirEntry.bColorCount == 0 ? "/" : IconDirEntry.bColorCount.ToString();
    public ushort Planes => IconDirEntry.wPlanes;
    public ushort BitsPerPixel => IconDirEntry.wBitCount;
    public uint Size => IconDirEntry.dwBytesInRes;
    public int Resolution => Width * Height;

    public static IconInfo? operator ~(IconInfo? thiz)
    {
        thiz?.Icon?.Dispose();

        return null;
    }
}

public sealed partial record IconInfo : IReloadableModel
{
    private readonly ReloadableProperty<Bitmap> _iconReloadable = new();

    public async Task Load()
    {
        await _iconReloadable.LoadProperty(async token =>
            {
                token.ThrowIfCancellationRequested();
                await using var ms = new MemoryStream();

                //
                token.ThrowIfCancellationRequested();
                var encoder = new IcoBitmapEncoder();

                token.ThrowIfCancellationRequested();
                var frame = new IcoBitmapEncoder.IcoFrame(Module, IconDirEntry);
                Format = frame.Format;

                token.ThrowIfCancellationRequested();
                encoder.Frame.Add(frame);

                token.ThrowIfCancellationRequested();
                await encoder.SaveAsync(ms, token);

                //
                token.ThrowIfCancellationRequested();
                ms.Position = 0;

                token.ThrowIfCancellationRequested();
                return await Task.Run(() => new Bitmap(ms), token);
            },
            icon => Icon = icon,
            icon => icon?.Dispose());
    }

    public async Task Unload()
    {
        await _iconReloadable.UnloadProperty(() =>
        {
            if (Icon is null)
                return;

            Icon.Dispose();
            Icon = null;
        });
    }
}

public sealed partial record IconInfo : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);

        return true;
    }
}