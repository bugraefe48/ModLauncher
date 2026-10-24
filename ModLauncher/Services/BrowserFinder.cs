using Microsoft.Win32;
using System.IO;

namespace ModLauncher;

// Ayarlar listesinde görünecek tarayıcı bilgisi
public record BrowserInfo(string Name, string Path);

public static class BrowserFinder
{
    // Windows'a kayıtlı tarayıcıları bulur
    public static List<BrowserInfo> Find()
    {
        var result = new List<BrowserInfo>();

        foreach (var root in new[] { Registry.LocalMachine, Registry.CurrentUser })
        {
            using var clients = root.OpenSubKey(@"SOFTWARE\Clients\StartMenuInternet");
            if (clients == null) continue;

            foreach (var keyName in clients.GetSubKeyNames())
            {
                using var browserKey = clients.OpenSubKey(keyName);
                using var commandKey = clients.OpenSubKey(keyName + @"\shell\open\command");

                var path = ExtractExePath(commandKey?.GetValue(null) as string);
                if (path == null || !File.Exists(path)) continue;

                // Aynı tarayıcı iki kez eklenmesin
                if (result.Any(b => b.Path.Equals(path, StringComparison.OrdinalIgnoreCase))) continue;

                var name = browserKey?.GetValue(null) as string;
                result.Add(new BrowserInfo(string.IsNullOrWhiteSpace(name) ? keyName : name, path));
            }
        }

        return result;
    }

    // "C:\...\chrome.exe" --parametre gibi bir komuttan sadece .exe yolunu ayıklar
    static string? ExtractExePath(string? command)
    {
        if (string.IsNullOrWhiteSpace(command)) return null;
        command = command.Trim();

        if (command.StartsWith('"'))
        {
            int end = command.IndexOf('"', 1);
            return end > 1 ? command.Substring(1, end - 1) : null;
        }

        int exe = command.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
        return exe > 0 ? command.Substring(0, exe + 4) : null;
    }
}