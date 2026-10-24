using System.IO;
using System.Text.Json;
using System.Windows;
using Wpf.Ui.Controls;

namespace ModLauncher;

public partial class SettingsWindow : FluentWindow
{
    readonly AppData data;
    readonly AppSettings settings;
    readonly Action onChanged;                  // bir ayar değişince ana pencereye haber verir
    readonly Action onModesChanged;             // modlar içe aktarılınca ana pencereye haber verir
    readonly List<BrowserInfo> browsers = new();
    bool loading = true;                        // açılırken olayların kaydetmesini engeller

    public SettingsWindow(AppData data, Action onChanged, Action onModesChanged)
    {
        InitializeComponent();
        this.data = data;
        settings = data.Settings;
        this.onChanged = onChanged;
        this.onModesChanged = onModesChanged;

        // Tema
        ThemeBox.SelectedIndex = (int)settings.Theme;

        // Tarayıcı listesi: ilk sırada "Windows varsayılanı"
        browsers.Add(new BrowserInfo("Windows varsayılanı", ""));
        browsers.AddRange(BrowserFinder.Find());

        // Kayıtlı tarayıcı listede yoksa (elle seçilmişse) onu da ekle
        var saved = settings.DefaultBrowserPath;
        if (!string.IsNullOrWhiteSpace(saved) &&
            !browsers.Any(b => b.Path.Equals(saved, StringComparison.OrdinalIgnoreCase)))
        {
            browsers.Add(CustomBrowser(saved));
        }
        RefreshBrowsers();
        BrowserBox.SelectedValue = saved ?? "";

        // Diğer ayarlar
        ConfirmBox.IsChecked = settings.ConfirmBeforeRun;
        DelaySlider.Value = settings.DelayBetweenActionsMs;
        DelayText.Text = $"{settings.DelayBetweenActionsMs} ms";
        TrayBox.IsChecked = settings.MinimizeToTray;
        StartupBox.IsChecked = settings.StartWithWindows;

        loading = false;
    }

    static BrowserInfo CustomBrowser(string path) =>
        new("Özel: " + System.IO.Path.GetFileName(path), path);

    void RefreshBrowsers()
    {
        BrowserBox.ItemsSource = null;
        BrowserBox.ItemsSource = browsers;
    }

    void ThemeBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (loading) return;
        settings.Theme = (AppTheme)ThemeBox.SelectedIndex;
        onChanged();
    }

    void BrowserBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (loading || BrowserBox.SelectedValue is not string path) return;
        settings.DefaultBrowserPath = path;
        onChanged();
    }

    void BrowseBrowser_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Tarayıcıyı seç",
            Filter = "Uygulamalar (*.exe)|*.exe"
        };
        if (dialog.ShowDialog() != true) return;

        loading = true;
        if (!browsers.Any(b => b.Path.Equals(dialog.FileName, StringComparison.OrdinalIgnoreCase)))
            browsers.Add(CustomBrowser(dialog.FileName));
        RefreshBrowsers();
        BrowserBox.SelectedValue = dialog.FileName;
        loading = false;

        settings.DefaultBrowserPath = dialog.FileName;
        onChanged();
    }

    void Confirm_Changed(object sender, RoutedEventArgs e)
    {
        if (loading) return;
        settings.ConfirmBeforeRun = ConfirmBox.IsChecked == true;
        onChanged();
    }

    void DelaySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (loading) return;
        int ms = (int)DelaySlider.Value;
        settings.DelayBetweenActionsMs = ms;
        DelayText.Text = $"{ms} ms";
        onChanged();
    }

    void Tray_Changed(object sender, RoutedEventArgs e)
    {
        if (loading) return;
        settings.MinimizeToTray = TrayBox.IsChecked == true;
        onChanged();
    }

    void Startup_Changed(object sender, RoutedEventArgs e)
    {
        if (loading) return;
        settings.StartWithWindows = StartupBox.IsChecked == true;
        onChanged();
    }

    // ---------- Yedekleme ----------

    void Export_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Title = "Modları dışa aktar",
            Filter = "Mod dosyası (*.json)|*.json",
            FileName = "modlar.json"
        };
        if (dialog.ShowDialog() != true) return;

        try
        {
            Storage.Export(dialog.FileName, data.Modes);
            System.Windows.MessageBox.Show($"{data.Modes.Count} mod dışa aktarıldı.", "Dışa aktar",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            System.Windows.MessageBox.Show("Dosya yazılamadı: " + ex.Message, "Dışa aktar",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    void Import_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Modları içe aktar",
            Filter = "Mod dosyası (*.json)|*.json|Tüm dosyalar (*.*)|*.*"
        };
        if (dialog.ShowDialog() != true) return;

        List<Mode> imported;
        try
        {
            imported = Storage.Import(dialog.FileName);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            System.Windows.MessageBox.Show("Dosya okunamadı ya da geçerli bir mod dosyası değil.\n" + ex.Message,
                "İçe aktar", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            return;
        }

        foreach (var mode in imported)
        {
            mode.Id = Guid.NewGuid();
            mode.Name = UniqueName(string.IsNullOrWhiteSpace(mode.Name) ? "İçe aktarılan mod" : mode.Name);

            // Aynı kısayol zaten başka bir modda varsa çakışmasın diye sil
            if (!string.IsNullOrWhiteSpace(mode.Hotkey) && data.Modes.Any(m => m.Hotkey == mode.Hotkey))
                mode.Hotkey = "";

            data.Modes.Add(mode);
        }

        onModesChanged();
        System.Windows.MessageBox.Show($"{imported.Count} mod eklendi.", "İçe aktar",
            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
    }

    string UniqueName(string baseName)
    {
        var name = baseName;
        int i = 2;
        while (data.Modes.Any(m => m.Name == name))
            name = $"{baseName} {i++}";
        return name;
    }
}