using Avalonia;
using Avalonia.Controls;

namespace IconRipper.Gui.CustomControls;

public partial class IconInfoRow : UserControl
{
    public readonly static DirectProperty<IconInfoRow, object?> LeftColTextProperty =
        AvaloniaProperty.RegisterDirect<IconInfoRow, object?>(
            nameof(LeftColText),
            o => o.LeftColText,
            (o, v) => o.LeftColText = v);

    public object? LeftColText
    {
        get;
        set => SetAndRaise(LeftColTextProperty, ref field, value);
    }

    public readonly static DirectProperty<IconInfoRow, object?> RightColTextProperty =
        AvaloniaProperty.RegisterDirect<IconInfoRow, object?>(
            nameof(RightColText),
            o => o.RightColText,
            (o, v) => o.RightColText = v);

    public object? RightColText
    {
        get;
        set => SetAndRaise(RightColTextProperty, ref field, value);
    }

    public IconInfoRow()
    {
        InitializeComponent();
    }
}