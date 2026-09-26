using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Messaging;
using IconRipper.Gui.Message;

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

        //
        WeakReferenceMessenger.Default.Register<MainWindow, RequestMainWindowViewMessage>(this,
            (r, m) => { m.Reply(this); });
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        WeakReferenceMessenger.Default.UnregisterAll(this);
    }
}