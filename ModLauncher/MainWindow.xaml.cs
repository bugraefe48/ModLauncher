using System.Windows;
using System.Windows.Input;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;
using MessageBoxResult = System.Windows.MessageBoxResult;

namespace ModLauncher;

public partial class MainWindow : FluentWindow
{
    readonly AppData data;
    readonly TrayService tray;
    readonly HotkeyService hotkeys;
    object? selected;       // ağaçta şu an seçili olan (Mode veya ModeAction)
    bool exiting;           // gerçekten çıkılıyor mu? (yoksa tepsiye küçül)

    // Windows ile başlayınca pencere açılmasın mı?
    public bool StartHidden => data.Settings.MinimizeToTray;

    public MainWindow()
    {
        SystemThemeWatcher.Watch(this);
        InitializeComponent();

        data = Storage.Load();
        ThemeHelper.Apply(data.Settings.Theme, this);
        ModeTree.ItemsSource = data.Modes;
        ColorBox.ItemsSource = ModeColors.All;

        // Windows ile başlama açıksa kayıt, güncel konumu göstersin
        if (data.Settings.StartWithWindows)
            StartupHelper.Set(true);

        tray = new TrayService(data, ShowWindow, ExitApp);

        // Kısayolları Windows'a kaydet, kaydedilemeyenleri bildir
        hotkeys = new HotkeyService(OnHotkeyPressed);
        var failed = hotkeys.RegisterAll(data.Modes);
        if (failed.Count > 0)
        {
            tray.Notify("Bazı kısayollar kaydedilemedi",
                string.Join(", ", failed.Select(m => $"{m.Name} ({m.Hotkey})")));
        }

        // Bilgisayar kapanırken pencere engel olmasın
        System.Windows.Application.Current.SessionEnding += (_, _) => exiting = true;

        // Uygulama kapanırken (tepsiden Çıkış dahil) kısayolları ve tepsi simgesini temizle
        System.Windows.Application.Current.Exit += (_, _) =>
        {
            hotkeys.Dispose();
            tray.Dispose();
        };

        Closing += OnWindowClosing;
        ShowPanel(null);
    }

    static Visibility Show(bool visible) => visible ? Visibility.Visible : Visibility.Collapsed;

    // ---------- Kısayollar ----------

    async void OnHotkeyPressed(Mode mode) => await tray.RunAsync(mode);

    void HotkeyBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        e.Handled = true;   // basılan tuş kutuya harf olarak yazılmasın
        if (selected is not Mode mode) return;

        // Alt ile basılan tuşlar "System" olarak gelir, asıl tuş SystemKey'de
        var key = e.Key == Key.System ? e.SystemKey : e.Key;

        // Sadece Ctrl/Alt/Shift/Win'e basıldıysa asıl tuşu bekle
        if (key is Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt
            or Key.LeftShift or Key.RightShift or Key.LWin or Key.RWin
            or Key.None or Key.ImeProcessed)
            return;

        var modifiers = Keyboard.Modifiers;

        // Hiçbir değiştirici yokken Backspace ya da Delete: kısayolu sil
        if (modifiers == ModifierKeys.None && (key is Key.Back or Key.Delete))
        {
            SetHotkey(mode, "");
            return;
        }

        if (modifiers == ModifierKeys.None)
        {
            MessageBox.Show("Kısayol için Ctrl, Shift ya da Win tuşlarından biriyle birlikte bas. Örnek: Ctrl+Shift+1",
                "Kısayol", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        // Ctrl+Alt, Türkçe klavyede AltGr ile aynı: @ { [ ] gibi karakterleri bozar
        if (modifiers.HasFlag(ModifierKeys.Control) && modifiers.HasFlag(ModifierKeys.Alt))
        {
            MessageBox.Show("Ctrl+Alt birlikte kullanılamaz, Türkçe klavyede AltGr ile karışır ve bazı karakterleri yazamaz hale getirir. Ctrl+Shift ya da Alt+Shift dene.",
                "Kısayol", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        SetHotkey(mode, HotkeyHelper.Format(modifiers, key));
    }

    void ClearHotkey_Click(object sender, RoutedEventArgs e)
    {
        if (selected is Mode mode)
            SetHotkey(mode, "");
    }

    // Kısayolu değiştirir, Windows kabul etmezse eskisine döner
    void SetHotkey(Mode mode, string text)
    {
        var old = mode.Hotkey;
        mode.Hotkey = text;

        var failed = hotkeys.RegisterAll(data.Modes);
        if (text != "" && failed.Contains(mode))
        {
            mode.Hotkey = old;
            hotkeys.RegisterAll(data.Modes);
            MessageBox.Show("Bu kısayol başka bir program ya da başka bir mod tarafından kullanılıyor. Başka bir tane dene.",
                "Kısayol kullanımda", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Storage.Save(data);
    }

    // ---------- Tepsi ve kapatma ----------

    void OnWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        Storage.Save(data);

        // Tepsiye küçül açıksa kapatmayı iptal edip pencereyi gizle
        if (!exiting && data.Settings.MinimizeToTray)
        {
            e.Cancel = true;
            Hide();
        }
    }

    // Tepsiden ya da ikinci kopya açılınca pencereyi öne getirir
    public void ShowWindow()
    {
        Show();
        if (WindowState == WindowState.Minimized)
            WindowState = WindowState.Normal;
        Activate();
    }

    void ExitApp()
    {
        exiting = true;
        Storage.Save(data);
        System.Windows.Application.Current.Shutdown();
    }

    // ---------- Ayarlar ----------

    void Settings_Click(object sender, RoutedEventArgs e)
    {
        var window = new SettingsWindow(data, OnSettingsChanged, OnModesImported) { Owner = this };
        window.ShowDialog();
    }

    // Ayarlar penceresinde bir şey değişince çalışır
    void OnSettingsChanged()
    {
        Storage.Save(data);
        ThemeHelper.Apply(data.Settings.Theme, this);
        StartupHelper.Set(data.Settings.StartWithWindows);
    }

    // Modlar içe aktarılınca çalışır
    void OnModesImported()
    {
        Storage.Save(data);

        // Kaydedilemeyen kısayolları temizle ki kafa karışmasın
        var failed = hotkeys.RegisterAll(data.Modes);
        if (failed.Count > 0)
        {
            foreach (var mode in failed)
                mode.Hotkey = "";
            hotkeys.RegisterAll(data.Modes);
            Storage.Save(data);

            tray.Notify("Bazı kısayollar temizlendi",
                "İçe aktarılan bazı kısayollar kullanımda olduğu için silindi.");
        }
    }

    // ---------- Sağ paneli seçime göre göster ----------

    void ShowPanel(object? item)
    {
        selected = item;
        EmptyText.Visibility = Visibility.Visible;
        ModePanel.Visibility = Visibility.Collapsed;
        ActionPanel.Visibility = Visibility.Collapsed;

        if (item is Mode mode)
        {
            ModePanel.DataContext = mode;
            ModePanel.Visibility = Visibility.Visible;
            EmptyText.Visibility = Visibility.Collapsed;
        }
        else if (item is ModeAction action)
        {
            bool isUrl = action.Type == ActionType.Url;
            bool isClose = action.Type == ActionType.CloseApp;

            ActionPanel.DataContext = action;
            TargetLabel.Text = isUrl ? "Site adresi"
                : isClose ? "Kapatılacak uygulama (exe yolu ya da işlem adı)"
                : "Program yolu";

            BrowseTargetButton.Visibility = Show(!isUrl);
            ArgsPanel.Visibility = Show(!isUrl && !isClose);
            BrowserPanel.Visibility = Show(isUrl);
            ForcePanel.Visibility = Show(isClose);

            ActionPanel.Visibility = Visibility.Visible;
            EmptyText.Visibility = Visibility.Collapsed;
        }
    }

    void ModeTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        => ShowPanel(e.NewValue);

    // ---------- Ağaçta öğe seçme yardımcıları ----------

    void SelectMode(Mode mode)
    {
        ModeTree.UpdateLayout();
        if (ModeTree.ItemContainerGenerator.ContainerFromItem(mode) is System.Windows.Controls.TreeViewItem item)
        {
            item.IsSelected = true;
            item.BringIntoView();
        }
    }

    void SelectAction(Mode mode, ModeAction action)
    {
        ModeTree.UpdateLayout();
        if (ModeTree.ItemContainerGenerator.ContainerFromItem(mode) is System.Windows.Controls.TreeViewItem parent)
        {
            parent.IsExpanded = true;
            parent.UpdateLayout();
            if (parent.ItemContainerGenerator.ContainerFromItem(action) is System.Windows.Controls.TreeViewItem child)
            {
                child.IsSelected = true;
                child.BringIntoView();
            }
            else
            {
                parent.IsSelected = true;
            }
        }
    }

    // ---------- Mod işlemleri ----------

    string UniqueName(string baseName)
    {
        var name = baseName;
        int i = 2;
        while (data.Modes.Any(m => m.Name == name))
            name = $"{baseName} {i++}";
        return name;
    }

    void NewMode_Click(object sender, RoutedEventArgs e)
    {
        var mode = new Mode { Name = UniqueName("Yeni Mod") };
        data.Modes.Add(mode);
        Storage.Save(data);
        SelectMode(mode);
    }

    void DeleteMode_Click(object sender, RoutedEventArgs e)
    {
        if (selected is not Mode mode) return;

        var answer = MessageBox.Show(
            $"\"{mode.Name}\" modu ve içindeki tüm eylemler silinsin mi?",
            "Modu sil", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (answer != MessageBoxResult.Yes) return;

        data.Modes.Remove(mode);
        hotkeys.RegisterAll(data.Modes);   // silinen modun kısayolu serbest kalsın
        Storage.Save(data);
        ShowPanel(null);
    }

    // Renk değişince kaydet
    void Color_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (selected is Mode)
            Storage.Save(data);
    }

    // ---------- Sıralama ----------

    // Seçili mod ya da eylemi bir adım yukarı (-1) ya da aşağı (+1) taşır
    void MoveSelected(int delta)
    {
        if (selected is Mode mode)
        {
            int from = data.Modes.IndexOf(mode);
            int to = from + delta;
            if (to < 0 || to >= data.Modes.Count) return;

            data.Modes.Move(from, to);
            Storage.Save(data);
            SelectMode(mode);
        }
        else if (selected is ModeAction action)
        {
            var parent = data.Modes.FirstOrDefault(m => m.Actions.Contains(action));
            if (parent is null) return;

            int from = parent.Actions.IndexOf(action);
            int to = from + delta;
            if (to < 0 || to >= parent.Actions.Count) return;

            parent.Actions.Move(from, to);
            Storage.Save(data);
            SelectAction(parent, action);
        }
    }

    void MoveUp_Click(object sender, RoutedEventArgs e) => MoveSelected(-1);
    void MoveDown_Click(object sender, RoutedEventArgs e) => MoveSelected(1);

    // ---------- Eylem ekleme ----------

    void AddApp_Click(object sender, RoutedEventArgs e)
    {
        if (selected is not Mode mode) return;

        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Açılacak uygulamayı seç",
            Filter = "Uygulamalar (*.exe;*.lnk)|*.exe;*.lnk|Tüm dosyalar (*.*)|*.*"
        };
        if (dialog.ShowDialog() != true) return;

        var action = new ModeAction { Type = ActionType.App, Target = dialog.FileName };
        mode.Actions.Add(action);
        Storage.Save(data);
        SelectAction(mode, action);
    }

    void AddUrl_Click(object sender, RoutedEventArgs e)
    {
        if (selected is not Mode mode) return;

        var action = new ModeAction { Type = ActionType.Url, Target = "https://" };
        mode.Actions.Add(action);
        Storage.Save(data);
        SelectAction(mode, action);
    }

    void AddCloseApp_Click(object sender, RoutedEventArgs e)
    {
        if (selected is not Mode mode) return;

        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Kapatılacak uygulamayı seç",
            Filter = "Uygulamalar (*.exe)|*.exe|Tüm dosyalar (*.*)|*.*"
        };
        if (dialog.ShowDialog() != true) return;

        var action = new ModeAction { Type = ActionType.CloseApp, Target = dialog.FileName };
        mode.Actions.Add(action);
        Storage.Save(data);
        SelectAction(mode, action);
    }

    // ---------- Eylem düzenleme ----------

    void BrowseTarget_Click(object sender, RoutedEventArgs e)
    {
        if (selected is not ModeAction action) return;

        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Uygulamayı seç",
            Filter = "Uygulamalar (*.exe;*.lnk)|*.exe;*.lnk|Tüm dosyalar (*.*)|*.*"
        };
        if (dialog.ShowDialog() != true) return;

        action.Target = dialog.FileName;
        Storage.Save(data);
    }

    void BrowseBrowser_Click(object sender, RoutedEventArgs e)
    {
        if (selected is not ModeAction action) return;

        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Tarayıcıyı seç",
            Filter = "Uygulamalar (*.exe)|*.exe"
        };
        if (dialog.ShowDialog() != true) return;

        action.BrowserPath = dialog.FileName;
        Storage.Save(data);
    }

    void DeleteAction_Click(object sender, RoutedEventArgs e)
    {
        if (selected is not ModeAction action) return;

        var parent = data.Modes.FirstOrDefault(m => m.Actions.Contains(action));
        if (parent is null) return;

        parent.Actions.Remove(action);
        Storage.Save(data);
        SelectMode(parent);
    }

    // Seçili eylemi tek başına çalıştırıp dener
    async void TestAction_Click(object sender, RoutedEventArgs e)
    {
        if (selected is not ModeAction action) return;

        var button = sender as System.Windows.Controls.Button;
        if (button != null) button.IsEnabled = false;

        var error = await ModeRunner.TryExecuteAsync(action, data.Settings);

        if (button != null) button.IsEnabled = true;

        if (error != null)
        {
            MessageBox.Show(error, "Eylem çalışmadı",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    // Bir kutudan çıkılınca ya da kutuya tıklanınca kaydet
    void Field_LostFocus(object sender, RoutedEventArgs e) => Storage.Save(data);

    // ---------- Modu çalıştır ----------

    async void RunMode_Click(object sender, RoutedEventArgs e)
    {
        if (selected is not Mode mode) return;

        if (mode.Actions.Count == 0)
        {
            MessageBox.Show("Bu modda henüz eylem yok.", "Mod boş",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (data.Settings.ConfirmBeforeRun)
        {
            var answer = MessageBox.Show(
                $"\"{mode.Name}\" modu çalıştırılsın mı? ({mode.Actions.Count} eylem)",
                "Modu çalıştır", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (answer != MessageBoxResult.Yes) return;
        }

        // Çalışırken butona art arda basılmasın
        var button = sender as System.Windows.Controls.Button;
        if (button != null) button.IsEnabled = false;

        var errors = await ModeRunner.RunAsync(mode, data.Settings);

        if (button != null) button.IsEnabled = true;

        if (errors.Count > 0)
        {
            MessageBox.Show(string.Join("\n", errors),
                "Bazı eylemler çalışmadı", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}