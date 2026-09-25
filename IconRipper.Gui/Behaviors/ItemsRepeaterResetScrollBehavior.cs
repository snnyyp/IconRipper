using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;

namespace IconRipper.Gui.Behaviors;

public sealed class ItemsRepeaterResetScrollBehavior : Behavior<ItemsRepeater>
{
    public readonly static DirectProperty<ItemsRepeaterResetScrollBehavior, object?> BasedOnProperty =
        AvaloniaProperty.RegisterDirect<ItemsRepeaterResetScrollBehavior, object?>(
            nameof(BasedOn),
            o => o.BasedOn,
            (o, v) => o.BasedOn = v,
            defaultBindingMode: BindingMode.OneWay);

    public object? BasedOn
    {
        get;
        set
        {
            if (!SetAndRaise(BasedOnProperty, ref field, value))
                return;

            Dispatcher.Post(() =>
            {
                var scrollViewer = AssociatedObject?
                    .GetVisualAncestors()
                    .OfType<ScrollViewer>()
                    .FirstOrDefault();

                if (scrollViewer is null)
                    return;

                if (scrollViewer.Offset.Length == 0)
                    return;

                scrollViewer.ScrollToHome();
            }, DispatcherPriority.Background);
        }
    }
}