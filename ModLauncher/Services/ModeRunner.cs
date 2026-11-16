using System.Diagnostics;
using System.IO;

namespace ModLauncher;

public static class ModeRunner
{
    public static async Task<List<string>> RunAsync(Mode mode, AppSettings settings)
    {
        var errors = new List<string>();

        for (int i = 0; i < mode.Actions.Count; i++)
        {
            var action = mode.Actions[i];

            var error = await TryExecuteAsync(action, settings);
            if (error != null)
                errors.Add($"{action.DisplayName}: {error}");

            bool isLast = i == mode.Actions.Count - 1;
            if (!isLast && settings.DelayBetweenActionsMs > 0)
                await Task.Delay(settings.DelayBetweenActionsMs);
        }

        return errors;
    }

    public static async Task<string?> TryExecuteAsync(ModeAction action, AppSettings settings)
    {
        try
        {
            await ExecuteAsync(action, settings);
            return null;
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    static async Task ExecuteAsync(ModeAction action, AppSettings settings)
    {
        if (string.IsNullOrWhiteSpace(action.Target))
            throw new InvalidOperationException(Loc.T("Err_EmptyTarget"));

        switch (action.Type)
        {
            case ActionType.App:
                StartApp(action);
                break;
            case ActionType.Url:
                StartUrl(action, settings);
                break;
            case ActionType.CloseApp:
                await CloseAppAsync(action);
                break;
        }
    }

    static void StartApp(ModeAction action)
    {
        var target = Environment.ExpandEnvironmentVariables(action.Target.Trim().Trim('"'));

        if (File.Exists(target))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = target,
                Arguments = action.Arguments,
                UseShellExecute = true,
                WorkingDirectory = Path.GetDirectoryName(target) ?? ""
            });
            return;
        }

        // Yol değil de sadece ad yazılmışsa ("brave", "notepad") Windows'ta ara
        bool looksLikePath = target.Contains('\\') || target.Contains('/') || target.Contains(':');
        if (!looksLikePath)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = target,
                    Arguments = action.Arguments,
                    UseShellExecute = true
                });
                return;
            }
            catch (System.ComponentModel.Win32Exception)
            {
                throw new FileNotFoundException(Loc.T("Err_AppNotFound", target));
            }
        }

        throw new FileNotFoundException(Loc.T("Err_FileNotFound", target));
    }

    static void StartUrl(ModeAction action, AppSettings settings)
    {
        var text = action.Target.Trim();
        if (!text.Contains("://"))
            text = "https://" + text;

        // Sadece http ve https adreslerine izin ver
        if (!Uri.TryCreate(text, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException(Loc.T("Err_BadUrl"));
        }

        var url = uri.AbsoluteUri;

        var browser = !string.IsNullOrWhiteSpace(action.BrowserPath)
            ? action.BrowserPath
            : settings.DefaultBrowserPath;

        if (!string.IsNullOrWhiteSpace(browser))
        {
            if (!File.Exists(browser))
                throw new FileNotFoundException(Loc.T("Err_BrowserNotFound", browser));

            Process.Start(new ProcessStartInfo
            {
                FileName = browser,
                Arguments = $"\"{url}\"",
                UseShellExecute = true
            });
        }
        else
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
    }

    static async Task CloseAppAsync(ModeAction action)
    {
        var name = ModeAction.ProcessNameOf(action.Target);
        var processes = Process.GetProcessesByName(name);
        if (processes.Length == 0) return;

        try
        {
            var windowed = processes.Where(p => p.MainWindowHandle != IntPtr.Zero).ToList();
            foreach (var p in windowed)
                p.CloseMainWindow();

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            try
            {
                foreach (var p in windowed)
                    await p.WaitForExitAsync(cts.Token);
            }
            catch (OperationCanceledException) { }

            if (action.ForceClose)
            {
                foreach (var p in processes.Where(p => !p.HasExited))
                    p.Kill(entireProcessTree: true);
                return;
            }

            if (windowed.Count == 0)
                throw new InvalidOperationException(Loc.T("Err_NoWindow"));

            if (windowed.Any(p => !p.HasExited))
                throw new InvalidOperationException(Loc.T("Err_NotClosed"));
        }
        finally
        {
            foreach (var p in processes)
                p.Dispose();
        }
    }
}
