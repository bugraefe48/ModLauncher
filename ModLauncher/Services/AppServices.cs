using System.Windows;

namespace ModLauncher;

// Sayfaların ortak kullandığı veri ve servisler
public class AppServices
{
    readonly HashSet<Mode> running = new();

    public AppData Data { get; }
    public TrayService Tray { get; }
    public HotkeyService Hotkeys { get; }
    public Window Window { get; }
    public Action<string>? NavigateByKey { get; set; }

    public AppServices(AppData data, TrayService tray, HotkeyService hotkeys, Window window)
    {
        Data = data;
        Tray = tray;
        Hotkeys = hotkeys;
        Window = window;
    }

    public void Save() => Storage.Save(Data);

    // Ayarlardan bir şey değişince
    public void ApplySettings()
    {
        Save();
        Loc.Instance.SetLanguage(Data.Settings.Language);
        ThemeHelper.Apply(Data.Settings, Window);
        StartupHelper.Set(Data.Settings.StartWithWindows);
    }

    // Modlar içe aktarılınca
    public void ModesImported()
    {
        Save();

        var failed = Hotkeys.RegisterAll(Data.Modes);
        if (failed.Count > 0)
        {
            foreach (var mode in failed)
                mode.Hotkey = "";
            Hotkeys.RegisterAll(Data.Modes);
            Save();

            Tray.Notify(Loc.T("Tray_HotkeyClearedTitle"), Loc.T("Tray_HotkeyClearedText"));
        }
    }

    public void OpenHelp(string? topicKey = null)
    {
        var help = new HelpWindow(topicKey) { Owner = Window };
        help.ShowDialog();
    }

    // Pencereden mod çalıştırma: boş mod uyarısı, onay sorusu ve hata özeti dahil
    public async Task RunModeFromUiAsync(Mode mode)
    {
        if (mode.Actions.Count == 0)
        {
            MessageBox.Show(Loc.T("Msg_NoActions"), Loc.T("Msg_NoActionsTitle"),
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (Data.Settings.ConfirmBeforeRun)
        {
            var answer = MessageBox.Show(
                Loc.T("Msg_RunConfirm", mode.Name, mode.Actions.Count),
                Loc.T("Msg_RunTitle"), MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (answer != MessageBoxResult.Yes) return;
        }

        if (!running.Add(mode)) return;   // zaten çalışıyorsa tekrar başlatma
        try
        {
            var errors = await ModeRunner.RunAsync(mode, Data.Settings);
            if (errors.Count > 0)
            {
                MessageBox.Show(string.Join("\n", errors), Loc.T("Msg_ActionsFailedTitle"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        finally
        {
            running.Remove(mode);
        }
    }
}
