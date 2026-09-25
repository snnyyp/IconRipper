using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using IconRipper.Gui.Models;
using IconRipper.Interop;
using IconRipper.Interop.Managed;

namespace IconRipper.Gui.ViewModels;

public sealed class IconViewModel : ViewModelBase
{
    internal ObservableCollection<IconInfo> IconInfos { get; } = [];

    public IconViewModel(ManagedModule module, PeResourceKey groupName)
    {
        //
        var iconGroupInfo = module.GetResInfo(Kernel32.RT_GROUP_ICON, groupName);
        ref readonly var iconGroup = ref iconGroupInfo.GetRes<Kernel32.GRPICONDIR>();

        foreach (ref readonly var iconEntry in iconGroup.idEntries)
        {
            //
            IconInfos.Add(new IconInfo
            {
                Module = module,
                RtGroupIcon = groupName.ToString(false),
                IconDirEntry = iconEntry,
            });
        }
    }

    public void ReSort(SortCriteria criteria)
    {
        List<IconInfo> sorted = criteria switch
        {
            SortCriteria.IMAGE_QUALITY =>
            [
                .. IconInfos.OrderByDescending(info => info.Resolution)
                    .ThenByDescending(info => info.BitsPerPixel)
                    .ThenByDescending(info => info.Size),
            ],

            SortCriteria.ID => [.. IconInfos.OrderBy(info => info.RtIcon)],

            _ => throw new UnreachableException($"Unknown SortCriteria: {criteria}"),
        };

        for (var i = 0; i < sorted.Count; i++)
        {
            var oldIndex = IconInfos.IndexOf(sorted[i]);

            if (oldIndex != i)
                IconInfos.Move(oldIndex, i);
        }
    }

    public static IconViewModel? operator ~(IconViewModel? thiz)
    {
        if (thiz is null)
            return null;

        foreach (var iconInfo in thiz.IconInfos)
            _ = ~iconInfo;

        thiz.IconInfos.Clear();

        return null;
    }

}