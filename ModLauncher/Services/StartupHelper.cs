using Microsoft.Win32;

namespace ModLauncher;

public static class StartupHelper
{
    const string KeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    const string ValueName = "ModLauncher";

    // Windows ile başlamayı açar ya da kapatır
    public static void Set(bool enabled)
    {
        using var key = Registry.CurrentUser.OpenSubKey(KeyPath, writable: true);
        if (key == null) return;

        if (enabled && Environment.ProcessPath is string exe)
            key.SetValue(ValueName, $"\"{exe}\" --tray");
        else
            key.DeleteValue(ValueName, throwOnMissingValue: false);
    }
}