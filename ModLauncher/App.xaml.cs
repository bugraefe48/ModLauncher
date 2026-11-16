using System.Threading;
using System.Windows;

namespace ModLauncher;

public partial class App : Application
{
    static Mutex? mutex;
    static EventWaitHandle? showEvent;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Beklenmeyen bir hata olursa uygulama sessizce kapanmasın
        DispatcherUnhandledException += (_, args) =>
        {
            MessageBox.Show(Loc.T("Err_Unexpected", args.Exception.Message),
                "Mod Launcher", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };

        var evt = new EventWaitHandle(false, EventResetMode.AutoReset, "ModLauncher_ShowEvent");
        showEvent = evt;
        mutex = new Mutex(true, "ModLauncher_SingleInstance", out bool createdNew);

        // Uygulama zaten çalışıyorsa çalışan pencereyi öne getir ve çık
        if (!createdNew)
        {
            evt.Set();
            Shutdown();
            return;
        }

        // Windows ile başlayınca "--tray" ile gelir
        bool launchedAtLogin = e.Args.Contains("--tray");

        var window = new MainWindow();
        MainWindow = window;

        if (!(launchedAtLogin && window.StartHidden))
            window.Show();

        // Arka planda "pencereyi göster" sinyalini bekle
        var listener = new Thread(() =>
        {
            while (evt.WaitOne())
            {
                try { Dispatcher.Invoke(() => window.ShowWindow()); }
                catch (Exception) { return; }
            }
        })
        { IsBackground = true };
        listener.Start();
    }
}
