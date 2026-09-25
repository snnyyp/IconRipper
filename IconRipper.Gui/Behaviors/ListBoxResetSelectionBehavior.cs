using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Threading;
using Avalonia.Xaml.Interactivity;

namespace IconRipper.Gui.Behaviors;

public class ListBoxResetSelectionBehavior : Behavior<ListBox>
{
    public readonly static DirectProperty<ListBoxResetSelectionBehavior, object?> BasedOnProperty =
        AvaloniaProperty.RegisterDirect<ListBoxResetSelectionBehavior, object?>(
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
                AssociatedObject!.SelectedItem = null;
                AssociatedObject!.SelectedItems?.Clear();
            }, DispatcherPriority.Background);
        }
    }
}