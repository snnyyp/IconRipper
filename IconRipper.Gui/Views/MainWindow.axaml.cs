using Avalonia.Controls;

namespace IconRipper.Gui.Views;

public partial class MainWindow : Window
{
    private const double Scale = 1.4;

    public MainWindow()
    {
        InitializeComponent();

        //
        var primaryScreen = Screens.Primary!;

        Width = MinWidth = primaryScreen.WorkingArea.Width / primaryScreen.Scaling / Scale;
        Height = MinHeight = primaryScreen.WorkingArea.Height / primaryScreen.Scaling / Scale;
    }
}