using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IconRipper.Gui.Models;
using IconRipper.Gui.Views;
using IconRipper.Interop;
using IconRipper.Interop.Managed;
using MsBox.Avalonia;
using RentADeveloper.ResXLocalization;
using SkiaSharp;

namespace IconRipper.Gui.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private static Window MainWindow =>
        ((IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!).MainWindow!;

    public string[] PresetDlls { get; } =
    [
        "shell32.dll",
        "user32.dll",
        "imageres.dll",
        "ddores.dll",
        "setupapi.dll",
        "pifmgr.dll",
        "moricons.dll",
        "compstui.dll",
        "connect.dll",
        "pnidui.dll",
        "themecpl.dll",
        "Vault.dll",
        "wdc.dll",
        "WorkFoldersRes.dll",
        "desk.cpl",
        "telephon.cpl",
        "PhotoScreensaver.scr",
        "wusa.exe",
        "wmphoto.dll",
    ];

    //
    [ObservableProperty]
    public partial ManagedModule? Module { get; set; }

    [ObservableProperty]
    public partial IconGroupViewModel? IconGroup { get; set; }

    [ObservableProperty]
    public partial IconViewModel? Icon { get; set; }

    [ObservableProperty]
    public partial IconGroupInfo? SelectedIconGroup { get; set; }

    [ObservableProperty]
    public partial IconInfo? SelectedIcon { get; set; }

    [ObservableProperty]
    public partial SortCriteria SortCriteria { get; set; } = SortCriteria.ID;

    [ObservableProperty]
    public partial string SelectedLanguage { get; set; } = Localizer.Current.CurrentCulture.Name;

    //
    public IRelayCommand ExitProgramCommand { get; } =
        new RelayCommand(()
            => ((IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!).Shutdown());

    //
    public IRelayCommand AboutCommand { get; }
    public IRelayCommand ToggleIconSortCriteriaCommand { get; }
    public IRelayCommand SaveIconCommand { get; }
    public IRelayCommand SaveIconGroupCommand { get; }
    public IRelayCommand LoadCommand { get; }
    public IRelayCommand UnloadCommand { get; }
    public IRelayCommand LoadPresetCommand { get; }
    public IRelayCommand ChangeLanguageCommand { get; }
    public IRelayCommand CopyToClipboardCommand { get; }

    public MainWindowViewModel(string[]? cmdLineArgs = null)
    {
        //
        AboutCommand = new AsyncRelayCommand(AboutCommand_OnExecute);
        ToggleIconSortCriteriaCommand = new RelayCommand(ToggleIconSortCriteriaCommand_OnExecute);
        SaveIconCommand = new AsyncRelayCommand<string?>(SaveIconCommand_OnExecute);
        SaveIconGroupCommand = new AsyncRelayCommand<string?>(SaveIconGroupCommand_OnExecute);
        LoadCommand = new AsyncRelayCommand(LoadCommand_OnExecute);
        UnloadCommand = new RelayCommand(Unload);
        LoadPresetCommand = new AsyncRelayCommand<string?>(LoadPresetCommand_OnExecute);
        ChangeLanguageCommand = new RelayCommand<string?>(ChangeLanguageCommand_OnExecute);
        CopyToClipboardCommand = new RelayCommand(CopyToClipboardCommand_OnExecute);

        // drag and drop
        OnFileDroppedCommand = new AsyncRelayCommand<object?>(OnFileDropped_OnExecute);

        // respond to command line args
        if (cmdLineArgs is null)
            return;

        if (cmdLineArgs.Length == 0)
            return;

        Dispatcher.UIThread.InvokeAsync(async () => { await Load(cmdLineArgs[0]); }, DispatcherPriority.Loaded);
    }

    private async Task AboutCommand_OnExecute()
    {
        if (!User32.IsKeyDownNow(User32.VirtualKeyCodes.VK_CONTROL))
        {
            await new AboutDialog().ShowDialog(MainWindow);

            return;
        }

        var dialog = new StressTestDialog();
        var count = await dialog.ShowDialog<int?>(MainWindow);

        if (!count.HasValue)
            return;

        for (var i = 0; i < count; i++)
        {
            Debug.WriteLine($"Load {i}");

            await Load($@"{Environment.GetFolderPath(Environment.SpecialFolder.System)}\shell32.dll");
            await Task.Delay(150);
            await Load($@"{Environment.GetFolderPath(Environment.SpecialFolder.System)}\imageres.dll");
            await Task.Delay(150);
        }

        Unload();
    }

    /// <summary>
    /// PNG - CF_DIBV5 (17)
    /// </summary>
    private unsafe void CopyToClipboardCommand_OnExecute()
    {
        if (SelectedIcon is null || Module is null)
            throw new UnreachableException();

        //
        using var ms = new MemoryStream();
        using var clipboard = new ManagedClipboard(nint.Zero);
        clipboard.Empty();

        // PNG
        SelectedIcon.Icon!.Save(ms, new PngBitmapEncoderOptions
        {
            CompressionLevel = CompressionLevel.NoCompression,
        });
        clipboard.SetData("PNG", ms);

        // CF_DIBV5 (17)
        var stride = SelectedIcon.Width * 4;
        var pixels = new byte[stride * SelectedIcon.Height];

        fixed (byte* pPixels = pixels)
        {
            SelectedIcon.Icon.CopyPixels(new PixelRect(SelectedIcon.Icon.PixelSize),
                (nint)pPixels, pixels.Length, stride);

            for (var y = 0; y < SelectedIcon.Height / 2; y++)
            {
                for (var x = 0; x < stride; x++)
                {
                    (pPixels[y * stride + x], pPixels[(SelectedIcon.Height - y - 1) * stride + x])
                        = (pPixels[(SelectedIcon.Height - y - 1) * stride + x], pPixels[y * stride + x]);
                }
            }
        }

        var v5Header = new IcoBitmapEncoder.BITMAPV5HEADER
        {
            bV5Width = SelectedIcon.Icon.PixelSize.Width,
            bV5Height = SelectedIcon.Icon.PixelSize.Height,
            bV5BitCount = IcoBitmapEncoder.V5BitCount.BC32,
            bV5Compression = IcoBitmapEncoder.V5Compression.BI_RGB,
            bV5SizeImage = (uint)pixels.Length,

            bV5RedMask = 0x00FF0000,
            bV5GreenMask = 0x0000FF00,
            bV5BlueMask = 0x000000FF,
            bV5AlphaMask = 0xFF000000,

            bV5CSType = IcoBitmapEncoder.V5ColorSpace.LCS_WINDOWS_COLOR_SPACE,
        };

        ms.SetLength(0);
        ms.Position = 0;

        ms.Write(MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref v5Header, 1)));
        ms.Write(pixels);

        clipboard.SetData(User32.ClipboardDataFormats.CF_DIBV5, ms);
    }

    partial void OnSelectedIconGroupChanged(IconGroupInfo? value)
    {
        if (Module is null)
            throw new UnreachableException();

        if (value is null)
        {
            Icon = ~Icon;
        }
        else
        {
            Icon = new IconViewModel(Module, value.GroupNameKey);
            Icon.ReSort(SortCriteria);
        }
    }

    private void ToggleIconSortCriteriaCommand_OnExecute()
    {
        if (Icon is null)
            throw new UnreachableException();

        SortCriteria = SortCriteria == SortCriteria.ID ? SortCriteria.IMAGE_QUALITY : SortCriteria.ID;
        Icon.ReSort(SortCriteria);
    }

    private async Task SaveIconCommand_OnExecute(string? format)
    {
        ArgumentNullException.ThrowIfNull(format);

        if (SelectedIcon is null || Module is null)
            throw new UnreachableException();

        var fileExtensions = new List<FilePickerFileType>(
        [
            new FilePickerFileType(Localizer.Current["FileDialog.Ext.png"])
            {
                Patterns = ["*.png"],
            },
            new FilePickerFileType(Localizer.Current["FileDialog.Ext.jpg"])
            {
                Patterns = ["*.jpg"],
            },
            new FilePickerFileType(Localizer.Current["FileDialog.Ext.webp"])
            {
                Patterns = ["*.webp"],
            },
            new FilePickerFileType(Localizer.Current["FileDialog.Ext.ico"])
            {
                Patterns = ["*.ico"],
            },
        ]);

        var storageFile = await MainWindow.StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                FileTypeChoices = fileExtensions,
                SuggestedFileName = $"{Path.GetFileName(Module.LibPath)}" +
                                    $"_{SelectedIcon.RtGroupIcon}" +
                                    $"_#{SelectedIcon.RtIcon}" +
                                    $"_{SelectedIcon.Width}x{SelectedIcon.Height}" +
                                    $"_{SelectedIcon.BitsPerPixel}" +
                                    $".{format}",
                SuggestedFileType = fileExtensions.First(ext => ext.Patterns![0].EndsWith(format)),
            });
        var filePath = storageFile?.TryGetLocalPath();

        if (filePath is null)
            return;

        switch (Path.GetExtension(filePath))
        {
            case ".png":
            {
                SelectedIcon.Icon!.Save(filePath, new PngBitmapEncoderOptions
                {
                    CompressionLevel = CompressionLevel.NoCompression,
                });

                break;
            }

            case ".jpg":
            {
                SelectedIcon.Icon!.Save(filePath, JpegBitmapEncoderOptions.Default);

                break;
            }

            case "*.ico":
            {
                var encoder = new IcoBitmapEncoder();
                await using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);

                encoder.Frame.Add(new IcoBitmapEncoder.IcoFrame(Module, SelectedIcon.IconDirEntry));
                await encoder.SaveAsync(fs);

                break;
            }

            case ".bmp":
            {
                break;
            }

            case ".webp":
            {
                var stride = SelectedIcon.Width * 4;
                var pixels = new byte[stride * SelectedIcon.Height];
                using var skBitmap = new SKBitmap();

                unsafe
                {
                    fixed (byte* pPixels = pixels)
                    {
                        SelectedIcon.Icon!.CopyPixels(new PixelRect(SelectedIcon.Icon.PixelSize),
                            (nint)pPixels, pixels.Length, stride);

                        skBitmap.InstallPixels(new SKPixmap(new SKImageInfo
                        {
                            Width = SelectedIcon.Width,
                            Height = SelectedIcon.Height,
                            AlphaType = SKAlphaType.Premul,
                            ColorType = SKColorType.Bgra8888,
                        }, (nint)pPixels, stride));
                    }
                }

                await using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);

                skBitmap.Encode(fs, SKEncodedImageFormat.Webp, 100);

                break;
            }
        }
    }

    private async Task SaveIconGroupCommand_OnExecute(string? format)
    {
        ArgumentNullException.ThrowIfNull(format);

        if (SelectedIconGroup is null || Module is null)
            throw new UnreachableException();

        if (format != "ico")
            return;

        var storageFile = await MainWindow.StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                FileTypeChoices =
                [
                    new FilePickerFileType(Localizer.Current["FileDialog.Ext.ico"])
                    {
                        Patterns = ["*.ico"],
                    },
                ],
                SuggestedFileName = $"{Path.GetFileName(Module.LibPath)}" +
                                    $"_{SelectedIconGroup.GroupName}" +
                                    $".{format}",
            });
        var filePath = storageFile?.TryGetLocalPath();

        if (filePath is null)
            return;

        var encoder = new IcoBitmapEncoder();
        var iconGroupInfo = Module.GetResInfo(Kernel32.RT_GROUP_ICON, SelectedIconGroup.GroupNameKey);

        foreach (ref readonly var iconEntry in iconGroupInfo.GetRes<Kernel32.GRPICONDIR>().idEntries)
        {
            encoder.Frame.Add(new IcoBitmapEncoder.IcoFrame(Module, iconEntry));
        }

        await using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        await encoder.SaveAsync(fs);
    }

    private async Task LoadCommand_OnExecute()
    {
        var fileList = await MainWindow.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                FileTypeFilter =
                [
                    new FilePickerFileType(Localizer.Current["FileDialog.Ext.dll"])
                    {
                        Patterns = ["*.dll"],
                    },
                    new FilePickerFileType(Localizer.Current["FileDialog.Ext.exe"])
                    {
                        Patterns = ["*.exe"],
                    },
                    new FilePickerFileType(Localizer.Current["FileDialog.Ext.cpl"])
                    {
                        Patterns = ["*.cpl"],
                    },
                    FilePickerFileTypes.All,
                ],
            });

        if (fileList.Count != 1)
            return;

        await Load(fileList[0].TryGetLocalPath() ?? throw new NullReferenceException());
    }

    private async Task LoadPresetCommand_OnExecute(string? preset)
    {
        Debug.WriteLine(preset);
        ArgumentNullException.ThrowIfNull(preset);

        await Load($@"{Environment.GetFolderPath(Environment.SpecialFolder.System)}\{preset}");
    }

    private void ChangeLanguageCommand_OnExecute(string? s)
    {
        ArgumentNullException.ThrowIfNull(s);

        if (s == SelectedLanguage)
            return;

        Localizer.Current.CurrentCulture = CultureInfo.GetCultureInfo(s);
        SelectedLanguage = s;
    }

    private async Task Load(string peFilePath)
    {
        Unload();

        try
        {
            Module = new ManagedModule(
                peFilePath,
                Kernel32.LoadLibraryExFlags.LOAD_LIBRARY_AS_DATAFILE |
                Kernel32.LoadLibraryExFlags.LOAD_LIBRARY_AS_IMAGE_RESOURCE
            );
        }
        catch (Win32Exception e)
        {
            await MessageBoxManager.GetMessageBoxStandard(
                    "IconRipper",
                    string.Format(Localizer.Current["ErrorMessage.Win32"],
                        peFilePath, e.NativeErrorCode, e.Message),
                    icon: MsBox.Avalonia.Enums.Icon.Error)
                .ShowWindowDialogAsync(MainWindow);

            return;
        }

        //
        try
        {
            IconGroup = new IconGroupViewModel(Module);
        }
        catch (Win32Exception e)
        {
            await MessageBoxManager.GetMessageBoxStandard(
                    "IconRipper",
                    string.Format(Localizer.Current["ErrorMessage.Win32"], e.NativeErrorCode, e.Message),
                    icon: MsBox.Avalonia.Enums.Icon.Error)
                .ShowWindowDialogAsync(MainWindow);

            Unload();
        }
    }

    private void Unload()
    {
        if (Module is null)
            return;

        //
        SelectedIcon = ~SelectedIcon;
        SelectedIconGroup = ~SelectedIconGroup;
        Icon = ~Icon;
        IconGroup = ~IconGroup;

        //
        Module?.Dispose();
        Module = null;

        Debug.WriteLine("Unloaded");
    }
}

public partial class MainWindowViewModel
{
    public IRelayCommand OnFileDroppedCommand { get; }

    private readonly byte[] _lnkHeader =
    [
        0x4C, 0x00, 0x00, 0x00,
        0x01, 0x14, 0x02, 0x00,
        0x00, 0x00, 0x00, 0x00,
        0xC0, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x46,
    ];

    private async Task OnFileDropped_OnExecute(object? file)
    {
        if (file is not IStorageFile storageFile)
            throw new UnreachableException();

        if (storageFile.TryGetLocalPath() is not { } filePath)
        {
            await MessageBoxManager.GetMessageBoxStandard(
                    "IconRipper",
                    Localizer.Current["ErrorMessage.DragNDrop.LocalPath"],
                    icon: MsBox.Avalonia.Enums.Icon.Error)
                .ShowWindowDialogAsync(MainWindow);

            return;
        }

        await using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            var cmpBuf = new byte[_lnkHeader.Length];
            await fs.ReadExactlyAsync(cmpBuf, 0, cmpBuf.Length);

            if (cmpBuf.SequenceEqual(_lnkHeader))
            {
                filePath = Shell32.GetLnkShortcutTargetPath(filePath);

                if (string.IsNullOrEmpty(filePath))
                {
                    await MessageBoxManager.GetMessageBoxStandard(
                            "IconRipper",
                            Localizer.Current["ErrorMessage.DragNDrop.LnkShortcut"],
                            icon: MsBox.Avalonia.Enums.Icon.Error)
                        .ShowWindowDialogAsync(MainWindow);

                    return;
                }
            }
        }

        await Load(filePath);
    }
}