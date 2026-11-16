using System.Windows;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace ModLauncher;

public partial class MainWindow : FluentWindow
{
    readonly AppData data;
    readonly TrayService tray;
    readonly HotkeyService hotkeys;
    readonly AppServices services;
    readonly List<NavItem> pages;
    bool exiting;

    // Windows ile başlayınca pencere açılmasın mı?
    public bool StartHidden => data.Settings.MinimizeToTray;

    public MainWindow()
    {
        SystemThemeWatcher.Watch(this);
        InitializeComponent();

        data = Storage.Load();
        Loc.Instance.SetLanguage(data.Settings.Language);
        ThemeHelper.Apply(data.Settings, this);

        if (data.Settings.StartWithWindows)
            StartupHelper.Set(true);

        tray = new TrayService(data, ShowWindow, ExitApp);
        hotkeys = new HotkeyService(OnHotkeyPressed);
        services = new AppServices(data, tray, hotkeys, this);
        services.NavigateByKey = Navigate;

        var failed = hotkeys.RegisterAll(data.Modes);
        if (failed.Count > 0)
        {
            tray.Notify(Loc.T("Tray_HotkeyFailTitle"),
                string.Join(", ", failed.Select(m => $"{m.Name} ({m.Hotkey})")));
        }

        pages = PageRegistry.Build(services);
        NavList.ItemsSource = pages;
        NavList.SelectedIndex = 0;

        System.Windows.Application.Current.SessionEnding += (_, _) => exiting = true;
        System.Windows.Application.Current.Exit += (_, _) =>
        {
            hotkeys.Dispose();
            tray.Dispose();
        };

        Closing += OnWindowClosing;
        Loaded += (_, _) => ShowWelcomeOnce();
    }

    async void OnHotkeyPressed(Mode mode) => await tray.RunAsync(mode);

    void NavList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (NavList.SelectedItem is NavItem item)
            PageHost.Content = item.GetPage();
    }

    void Navigate(string key)
    {
        int index = pages.FindIndex(p => p.Key == key);
        if (index >= 0)
            NavList.SelectedIndex = index;
    }

    void Help_Click(object sender, RoutedEventArgs e) => services.OpenHelp();

    // İlk açılışta yardımı bir kez göster
    void ShowWelcomeOnce()
    {
        if (data.Settings.TourShown) return;

        data.Settings.TourShown = true;
        Storage.Save(data);
        Dispatcher.BeginInvoke(new Action(() => services.OpenHelp("Help_T_Welcome")));
    }

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
}
