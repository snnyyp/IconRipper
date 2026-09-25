using System;
using Avalonia.Controls;
using Avalonia.Input;

namespace IconRipper.Gui.CustomControls;

public class RadioToggleButton : RadioButton
{
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (!e.Properties.IsRightButtonPressed)
            return;

        IsChecked = true;
        e.Handled = true;
    }
}