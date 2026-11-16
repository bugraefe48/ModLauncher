using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace ModLauncher;

public partial class SettingsPage : UserControl
{
    const string DefaultAccent = "#3498DB";

    readonly AppServices services;
    readonly AppSettings settings;
    readonly List<BrowserInfo> browsers = new();
    bool loading = true;

    AppData Data => services.Data;

    public SettingsPage(AppServices services)
    {
        InitializeComponent();
        this.services = services;
        settings = services.Data.Settings;

        ThemeBox.SelectedIndex = (int)settings.Theme;
        LangBox.SelectedIndex = Loc.Instance.Language == "tr" ? 0 : 1;

        AccentList.ItemsSource = ModeColors.Accents;
        bool systemAccent = string.IsNullOrWhiteSpace(settings.AccentColor);
        SystemAccentBox.IsChecked = systemAccent;
        AccentList.IsEnabled = !systemAccent;
        AccentList.SelectedValue = settings.AccentColor;

        ConfirmBox.IsChecked = settings.ConfirmBeforeRun;
        DelaySlider.Value = settings.DelayBetweenActionsMs;
        DelayText.Text = $"{settings.DelayBetweenActionsMs} ms";
        TrayBox.IsChecked = settings.MinimizeToTray;
        StartupBox.IsChecked = settings.StartWithWindows;

        RebuildBrowsers();
        UpdateTexts();
        Loc.Instance.PropertyChanged += (_, _) =>
        {
            RebuildBrowsers();
            UpdateTexts();
        };

        loading = false;
    }

    void UpdateTexts()
    {
        var v = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version;
        string version = v == null ? "1.1.0" : $"{v.Major}.{v.Minor}.{v.Build}";
        VersionText.Text = Loc.T("Settings_Version", version);
    }

    // ---------- Tarayıcı ----------

    BrowserInfo CustomBrowser(string path) =>
        new(Loc.T("Browser_Custom", Path.GetFileName(path)), path);

    void RebuildBrowsers()
    {
        bool old = loading;
        loading = true;

        browsers.Clear();
        browsers.Add(new BrowserInfo(Loc.T("Browser_Default"), ""));
        browsers.AddRange(BrowserFinder.Find());

        var saved = settings.DefaultBrowserPath;
        if (!string.IsNullOrWhiteSpace(saved) &&
            !browsers.Any(b => b.Path.Equals(saved, StringComparison.OrdinalIgnoreCase)))
        {
            browsers.Add(CustomBrowser(saved));
        }

        BrowserBox.ItemsSource = null;
        BrowserBox.ItemsSource = browsers;
        BrowserBox.SelectedValue = saved ?? "";

        loading = old;
    }

    void BrowserBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (loading || BrowserBox.SelectedValue is not string path) return;
        settings.DefaultBrowserPath = path;
        services.ApplySettings();
    }

    void BrowseBrowser_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = Loc.T("Dlg_PickBrowser"),
            Filter = $"{Loc.T("Dlg_Apps")} (*.exe)|*.exe"
        };
        if (dialog.ShowDialog() != true) return;

        settings.DefaultBrowserPath = dialog.FileName;
        services.ApplySettings();
        RebuildBrowsers();
    }

    // ---------- Görünüm ve dil ----------

    void ThemeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (loading) return;
        settings.Theme = (AppTheme)ThemeBox.SelectedIndex;
        services.ApplySettings();
    }

    void SystemAccent_Changed(object sender, RoutedEventArgs e)
    {
        if (loading) return;

        bool useSystem = SystemAccentBox.IsChecked == true;
        AccentList.IsEnabled = !useSystem;

        if (useSystem)
        {
            settings.AccentColor = "";
        }
        else
        {
            settings.AccentColor = DefaultAccent;
            loading = true;
            AccentList.SelectedValue = DefaultAccent;
            loading = false;
        }

        services.ApplySettings();
    }

    void AccentList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (loading || AccentList.SelectedValue is not string value) return;
        settings.AccentColor = value;
        services.ApplySettings();
    }

    void LangBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (loading) return;
        settings.Language = LangBox.SelectedIndex == 0 ? "tr" : "en";
        services.ApplySettings();
    }

    // ---------- Çalıştırma, başlangıç ----------

    void Confirm_Changed(object sender, RoutedEventArgs e)
    {
        if (loading) return;
        settings.ConfirmBeforeRun = ConfirmBox.IsChecked == true;
        services.ApplySettings();
    }

    void DelaySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (loading) return;
        int ms = (int)DelaySlider.Value;
        settings.DelayBetweenActionsMs = ms;
        DelayText.Text = $"{ms} ms";
        services.ApplySettings();
    }

    void Tray_Changed(object sender, RoutedEventArgs e)
    {
        if (loading) return;
        settings.MinimizeToTray = TrayBox.IsChecked == true;
        services.ApplySettings();
    }

    void Startup_Changed(object sender, RoutedEventArgs e)
    {
        if (loading) return;
        settings.StartWithWindows = StartupBox.IsChecked == true;
        services.ApplySettings();
    }

    // ---------- Yedekleme ----------

    void Export_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Title = Loc.T("Dlg_ExportTitle"),
            Filter = $"{Loc.T("Dlg_ModeFile")} (*.json)|*.json",
            FileName = "modlar.json"
        };
        if (dialog.ShowDialog() != true) return;

        try
        {
            Storage.Export(dialog.FileName, Data.Modes);
            MessageBox.Show(Loc.T("Exp_Done", Data.Modes.Count), Loc.T("Exp_Title"),
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(Loc.T("Exp_Fail", ex.Message), Loc.T("Exp_Title"),
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    void Import_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = Loc.T("Dlg_ImportTitle"),
            Filter = $"{Loc.T("Dlg_ModeFile")} (*.json)|*.json|{Loc.T("Dlg_AllFiles")} (*.*)|*.*"
        };
        if (dialog.ShowDialog() != true) return;

        List<Mode> imported;
        try
        {
            imported = Storage.Import(dialog.FileName);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            MessageBox.Show(Loc.T("Imp_ReadFail", ex.Message), Loc.T("Imp_Title"),
                MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        int openCount = imported.Sum(m => m.Actions.Count(a => a.Type == ActionType.App));
        int closeCount = imported.Sum(m => m.Actions.Count(a => a.Type == ActionType.CloseApp));

        var answer = MessageBox.Show(
            Loc.T("Imp_Confirm", imported.Count, openCount, closeCount),
            Loc.T("Imp_Title"), MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (answer != MessageBoxResult.Yes) return;

        foreach (var mode in imported)
        {
            mode.Id = Guid.NewGuid();
            mode.Name = UniqueName(string.IsNullOrWhiteSpace(mode.Name) ? Loc.T("Imp_NewName") : mode.Name);

            if (!string.IsNullOrWhiteSpace(mode.Hotkey) && Data.Modes.Any(m => m.Hotkey == mode.Hotkey))
                mode.Hotkey = "";

            Data.Modes.Add(mode);
        }

        services.ModesImported();
        MessageBox.Show(Loc.T("Imp_Done", imported.Count), Loc.T("Imp_Title"),
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    string UniqueName(string baseName)
    {
        var name = baseName;
        int i = 2;
        while (Data.Modes.Any(m => m.Name == name))
            name = $"{baseName} {i++}";
        return name;
    }

    void Link_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
    {
        System.Diagnostics.Process.Start(
            new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        e.Handled = true;
    }
}
