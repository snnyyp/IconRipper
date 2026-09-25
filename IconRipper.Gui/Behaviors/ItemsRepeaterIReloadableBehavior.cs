using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;
using IconRipper.Gui.Models;

namespace IconRipper.Gui.Behaviors;

public class ItemsRepeaterIReloadableBehavior : Behavior<ItemsRepeater>
{
    protected override void OnAttached()
    {
        base.OnAttached();

        AssociatedObject!.ElementPrepared += OnElementPrepared;
        AssociatedObject!.ElementClearing += OnElementClearing;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        AssociatedObject!.ElementPrepared -= OnElementPrepared;
        AssociatedObject!.ElementClearing -= OnElementClearing;
    }

    private async void OnElementPrepared(object? sender, ItemsRepeaterElementPreparedEventArgs e)
    {
        if (e.Element.DataContext is not IReloadableModel reloadableModel)
            return;

        await reloadableModel.Load();
    }

    private async void OnElementClearing(object? sender, ItemsRepeaterElementClearingEventArgs e)
    {
        if (e.Element.DataContext is not IReloadableModel reloadableModel)
            return;

        if (AssociatedObject!.GetElementIndex(e.Element) == 0)
            return;

        await reloadableModel.Unload();
    }
}