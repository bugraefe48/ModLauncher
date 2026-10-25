using System.Diagnostics;
using System.IO;

namespace ModLauncher;

public static class ModeRunner
{
    // Modun eylemlerini sırayla çalıştırır, hata veren eylemlerin açıklamalarını döndürür
    public static async Task<List<string>> RunAsync(Mode mode, AppSettings settings)
    {
        var errors = new List<string>();

        for (int i = 0; i < mode.Actions.Count; i++)
        {
            var action = mode.Actions[i];

            // Bir eylem bozuksa diğerleri yine de çalışsın
            var error = await TryExecuteAsync(action, settings);
            if (error != null)
                errors.Add($"{action.DisplayName}: {error}");

            // Son eylemden sonra beklemeye gerek yok
            bool isLast = i == mode.Actions.Count - 1;
            if (!isLast && settings.DelayBetweenActionsMs > 0)
                await Task.Delay(settings.DelayBetweenActionsMs);
        }

        return errors;
    }

    // Tek bir eylemi dener, hata olursa açıklamasını döndürür (sorun yoksa null)
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
            throw new InvalidOperationException("Hedef boş.");

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
        if (!File.Exists(action.Target))
            throw new FileNotFoundException("Dosya bulunamadı.");

        Process.Start(new ProcessStartInfo
        {
            FileName = action.Target,
            Arguments = action.Arguments,
            UseShellExecute = true,
            WorkingDirectory = Path.GetDirectoryName(action.Target) ?? ""
        });
    }

    static void StartUrl(ModeAction action, AppSettings settings)
    {
        var text = action.Target.Trim();
        if (!text.Contains("://"))
            text = "https://" + text;   // "google.com" yazılmışsa başına https ekle

        // Sadece http ve https adreslerine izin ver (file:// gibi adresler yerel programları açabilir)
        if (!Uri.TryCreate(text, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException("Geçerli bir http ya da https adresi değil.");
        }

        var url = uri.AbsoluteUri;   // özel karakterleri güvenli hale getirir

        // Önce eylemin kendi tarayıcısına bak, boşsa ayarlardaki tarayıcıya
        var browser = !string.IsNullOrWhiteSpace(action.BrowserPath)
            ? action.BrowserPath
            : settings.DefaultBrowserPath;

        if (!string.IsNullOrWhiteSpace(browser))
        {
            if (!File.Exists(browser))
                throw new FileNotFoundException("Tarayıcı bulunamadı: " + browser);

            Process.Start(new ProcessStartInfo
            {
                FileName = browser,
                Arguments = $"\"{url}\"",
                UseShellExecute = true
            });
        }
        else
        {
            // Tarayıcı seçilmemişse Windows'un varsayılan tarayıcısı açar
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
        if (processes.Length == 0) return;   // zaten kapalı, sorun yok

        try
        {
            // Penceresi olan işlemler: nazikçe kapat (kaydet sorusu çıkabilir)
            var windowed = processes.Where(p => p.MainWindowHandle != IntPtr.Zero).ToList();
            foreach (var p in windowed)
                p.CloseMainWindow();

            // Kapanmaları için en fazla 3 saniye bekle
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            try
            {
                foreach (var p in windowed)
                    await p.WaitForExitAsync(cts.Token);
            }
            catch (OperationCanceledException) { }

            if (action.ForceClose)
            {
                // Hâlâ açık olan ne varsa (arka plan işlemleri dahil) zorla kapat
                foreach (var p in processes.Where(p => !p.HasExited))
                    p.Kill(entireProcessTree: true);
                return;
            }

            if (windowed.Count == 0)
                throw new InvalidOperationException(
                    "Açık pencere bulunamadı (arka planda çalışıyor olabilir). 'Kapanmazsa zorla kapat' seçeneğini işaretleyebilirsin.");

            if (windowed.Any(p => !p.HasExited))
                throw new InvalidOperationException(
                    "Uygulama kapanmadı (kaydedilmemiş bir iş olabilir).");
        }
        finally
        {
            foreach (var p in processes)
                p.Dispose();
        }
    }
}