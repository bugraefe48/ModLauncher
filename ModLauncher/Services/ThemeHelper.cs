using System.Windows;
using Wpf.Ui.Appearance;

namespace ModLauncher;

public static class ThemeHelper
{
    // Seçilen temayı uygulamaya uygular
    public static void Apply(AppTheme theme, Window window)
    {
        switch (theme)
        {
            case AppTheme.Dark:
                SystemThemeWatcher.UnWatch(window);   // Windows'u takip etmeyi bırak
                ApplicationThemeManager.Apply(ApplicationTheme.Dark);
                break;

            case AppTheme.Light:
                SystemThemeWatcher.UnWatch(window);
                ApplicationThemeManager.Apply(ApplicationTheme.Light);
                break;

            default:   // System
                ApplicationThemeManager.ApplySystemTheme();
                SystemThemeWatcher.Watch(window);     // Windows değişince biz de değişelim
                break;
        }
    }
}