using System.Windows;
using System.Windows.Media;
using Wpf.Ui.Appearance;

namespace ModLauncher;

public static class ThemeHelper
{
    static AppSettings? current;
    static bool hooked;
    static bool applying;

    public static void Apply(AppSettings settings, Window window)
    {
        current = settings;
        applying = true;
        try
        {
            switch (settings.Theme)
            {
                case AppTheme.Dark:
                    SystemThemeWatcher.UnWatch(window);
                    ApplicationThemeManager.Apply(ApplicationTheme.Dark);
                    break;

                case AppTheme.Light:
                    SystemThemeWatcher.UnWatch(window);
                    ApplicationThemeManager.Apply(ApplicationTheme.Light);
                    break;

                default:
                    ApplicationThemeManager.ApplySystemTheme();
                    SystemThemeWatcher.Watch(window);
                    break;
            }

            ApplyAccent(settings);
        }
        finally
        {
            applying = false;
        }

        if (!hooked)
        {
            hooked = true;

            // Windows teması değişince WPF-UI vurgu rengini sıfırlar, seçileni geri koy
            ApplicationThemeManager.Changed += (_, _) =>
            {
                if (!applying && current != null)
                    ApplyAccent(current);
            };
        }
    }

    static void ApplyAccent(AppSettings settings)
    {
        try
        {
            Color color = ApplicationAccentColorManager.SystemAccent;

            if (!string.IsNullOrWhiteSpace(settings.AccentColor) &&
                ColorConverter.ConvertFromString(settings.AccentColor) is Color custom)
            {
                color = custom;
            }

            ApplicationAccentColorManager.Apply(color, ApplicationThemeManager.GetAppTheme());
            Application.Current.Resources["AppAccentBrush"] = new SolidColorBrush(color);
        }
        catch (Exception)
        {
            // Vurgu rengi uygulanamazsa varsayılan renkler kalır
        }
    }
}
