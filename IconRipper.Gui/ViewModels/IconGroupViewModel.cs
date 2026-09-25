using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Avalonia.Media.Imaging;
using IconRipper.Gui.Models;
using IconRipper.Interop;
using IconRipper.Interop.Managed;

namespace IconRipper.Gui.ViewModels;

public sealed class IconGroupViewModel : ViewModelBase
{
    private const int ThumbnailMaxWidth = 64;

    public ObservableCollection<IconGroupInfo> IconGroupInfos { get; } = [];

    public IconGroupViewModel()
    {
    }

    public IconGroupViewModel(ManagedModule module)
    {
        foreach (var iconGroupName in module.GetResNames(Kernel32.RT_GROUP_ICON))
        {
            //
            var iconGroupInfo = module.GetResInfo(Kernel32.RT_GROUP_ICON, iconGroupName);
            ref readonly var iconGroup = ref iconGroupInfo.GetRes<Kernel32.GRPICONDIR>();
            var bestIconEntry = GetBestIcon(in iconGroup);

            //
            IconGroupInfos.Add(new IconGroupInfo
            {
                GroupName = iconGroupName.ToString(false),
                GroupNameKey = iconGroupName,
                Module = module,
                BestIconEntry = bestIconEntry,
            });
        }
    }

    private static ref readonly Kernel32.GRPICONDIRENTRY GetBestIcon(ref readonly Kernel32.GRPICONDIR iconGroup)
    {
        if (iconGroup.idCount == 0)
            throw new IndexOutOfRangeException();

        ref readonly var best = ref iconGroup.idEntries[0];

        for (var i = 1; i < iconGroup.idCount; i++)
        {
            ref readonly var candidate = ref iconGroup.idEntries[i];

            var cmp = candidate.wBitCount.CompareTo(best.wBitCount);

            if (cmp == 0)
                cmp = (candidate.Width * candidate.Height).CompareTo(best.Width * best.Height);

            if (cmp == 0)
                cmp = candidate.dwBytesInRes.CompareTo(best.dwBytesInRes);

            if (cmp > 0)
                best = ref candidate;
        }

        return ref best;
    }

    public static IconGroupViewModel? operator ~(IconGroupViewModel? thiz)
    {
        if (thiz is null)
            return null;

        foreach (var iconGroupInfo in thiz.IconGroupInfos)
            _ = ~iconGroupInfo;

        thiz.IconGroupInfos.Clear();

        return null;
    }
}