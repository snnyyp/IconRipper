using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Input;

namespace IconRipper.Gui.Views;

public partial class AboutDialog : Window
{
    private const double Scale = 3;

    public AboutDialog()
    {
        InitializeComponent();

        //
        var primaryScreen = Screens.Primary!;

        Width = MinWidth = primaryScreen.WorkingArea.Width / primaryScreen.Scaling / Scale;
        Height = MinHeight = primaryScreen.WorkingArea.Height / primaryScreen.Scaling / Scale;

        //
        Resources["ProductVersion"] = FileVersionInfo.GetVersionInfo(Environment.ProcessPath!).ProductVersion!
            .Split("+", 2)[0];
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key != Key.Escape)
            return;

        Close();
        e.Handled = true;
    }
}