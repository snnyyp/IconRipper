using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace IconRipper.Gui.Views;

public partial class StressTestDialog : Window
{
    private const double Scale = 3;

    public StressTestDialog()
    {
        InitializeComponent();

        //
        var primaryScreen = Screens.Primary!;

        Width = MinWidth = primaryScreen.WorkingArea.Width / primaryScreen.Scaling / Scale;
        Height = MinHeight = primaryScreen.WorkingArea.Height / primaryScreen.Scaling / Scale;
    }

    protected override void OnOpened(EventArgs e)
    {
        NumericInput.Focus();
    }

    private void OkButton_OnClick(object sender, RoutedEventArgs e)
    {
        Close((int?)NumericInput.Value);
    }

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        Close(null);
    }
}