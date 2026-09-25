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

public sealed partial record IconGroupInfo
{
    public required string GroupName { get; init; }

    public Bitmap? Thumbnail
    {
        get;
        private set => SetField(ref field, value);
    }

    public required PeResourceKey GroupNameKey { get; init; }
    public required ManagedModule Module { get; init; }
    public required Kernel32.GRPICONDIRENTRY BestIconEntry { get; init; }

    public static IconGroupInfo? operator ~(IconGroupInfo? thiz)
    {
        thiz?.Thumbnail?.Dispose();

        return null;
    }
}

public sealed partial record IconGroupInfo : IReloadableModel
{
    private const int ThumbnailMaxWidth = 64;

    private readonly ReloadableProperty<Bitmap> _thumbnailReloadable = new();

    public async Task Load()
    {
        await _thumbnailReloadable.LoadProperty(async token =>
            {
                token.ThrowIfCancellationRequested();
                await using var ms = new MemoryStream();

                //
                token.ThrowIfCancellationRequested();
                var encoder = new IcoBitmapEncoder();

                token.ThrowIfCancellationRequested();
                encoder.Frame.Add(new IcoBitmapEncoder.IcoFrame(Module, BestIconEntry));
                token.ThrowIfCancellationRequested();
                await encoder.SaveAsync(ms, token);

                //
                token.ThrowIfCancellationRequested();
                ms.Position = 0;

                token.ThrowIfCancellationRequested();
                return await Task.Run(() => Bitmap.DecodeToWidth(ms, Math.Min(ThumbnailMaxWidth, BestIconEntry.Width)),
                    token);
            },
            thumbnail => Thumbnail = thumbnail,
            thumbnail => thumbnail?.Dispose());
    }

    public async Task Unload()
    {
        await _thumbnailReloadable.UnloadProperty(() =>
        {
            if (Thumbnail is null)
                return;

            Thumbnail.Dispose();
            Thumbnail = null;
        });
    }
}

public sealed partial record IconGroupInfo : INotifyPropertyChanged
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