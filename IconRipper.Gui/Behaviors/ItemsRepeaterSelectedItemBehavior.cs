using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Xaml.Interactivity;
using IconRipper.Gui.CustomControls;

namespace IconRipper.Gui.Behaviors;

public sealed class ItemsRepeaterSelectedItemBehavior : Behavior<ItemsRepeater>
{
    public readonly static DirectProperty<ItemsRepeaterSelectedItemBehavior, object?> SelectedItemProperty =
        AvaloniaProperty.RegisterDirect<ItemsRepeaterSelectedItemBehavior, object?>(
            nameof(SelectedItem),
            o => o.SelectedItem,
            (o, v) => o.SelectedItem = v,
            defaultBindingMode: BindingMode.TwoWay);

    public object? SelectedItem
    {
        get;
        set
        {
            if (ReferenceEquals(field, value))
                return;

            _updateSilently = true;

            foreach (var radioToggleButton in AssociatedObject!.GetLogicalChildren()
                         .OfType<RadioToggleButton>())
            {
                radioToggleButton.IsChecked =
                    ReferenceEquals(radioToggleButton.DataContext, value ?? field) && value is not null;
            }

            _updateSilently = false;

            field = value;
            RaisePropertyChanged(SelectedItemProperty, field, value);
        }
    }

    private bool _updateSilently;

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

    private void OnElementPrepared(object? sender, ItemsRepeaterElementPreparedEventArgs e)
    {
        if (e.Element is not ToggleButton toggleButton)
            return;

        if (ReferenceEquals(SelectedItem, toggleButton.DataContext))
            toggleButton.IsChecked = true;

        toggleButton.IsCheckedChanged += OnIsCheckedChanged;
    }

    private void OnElementClearing(object? sender, ItemsRepeaterElementClearingEventArgs e)
    {
        if (e.Element is not ToggleButton toggleButton)
            return;

        toggleButton.IsCheckedChanged -= OnIsCheckedChanged;
        toggleButton.IsChecked = false;
    }

    private void OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton toggleButton)
            return;

        if (_updateSilently)
            return;

        if (toggleButton.IsChecked == true)
            SelectedItem = toggleButton.DataContext;
        else if (ReferenceEquals(SelectedItem, toggleButton.DataContext))
            SelectedItem = null;
    }
}