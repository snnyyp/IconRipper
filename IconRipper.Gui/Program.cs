using Avalonia;
using System;
using System.Globalization;
using RentADeveloper.ResXLocalization;

namespace IconRipper.Gui;

static class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
    {
        //
        Localizer.Current.RegisterResourceManager(Localization.MainWindow.ResourceManager);
        Localizer.Current.RegisterResourceManager(Localization.AboutDialog.ResourceManager);
        Localizer.Current.CurrentCulture =
            GetNeutralCulture(CultureInfo.CurrentUICulture) ?? CultureInfo.GetCultureInfo("en");

        //
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            // .With(new FontManagerOptions
            // {
            //     DefaultFamilyName = "avares://IconRipper.Gui/Assets/Fonts/PingFang SC/#PingFang SC",
            // })
            .LogToTrace();
    }

    private static CultureInfo? GetNeutralCulture(CultureInfo? culture)
    {
        if (culture is null)
            return null;

        if (culture.IsNeutralCulture)
            return culture;

        return GetNeutralCulture(culture.Parent);
    }
}