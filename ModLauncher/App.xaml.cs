using System.Threading;
using System.Windows;

namespace ModLauncher;

public partial class App : Application
{
    static Mutex? mutex;                 // uygulamanın tek kopyasının çalışmasını sağlar
    static EventWaitHandle? showEvent;   // ikinci kopya, birinciye "pencereni göster" der

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var evt = new EventWaitHandle(false, EventResetMode.AutoReset, "ModLauncher_ShowEvent");
        showEvent = evt;
        mutex = new Mutex(true, "ModLauncher_SingleInstance", out bool createdNew);

        if (!createdNew)
        {
            evt.Set();    // çalışan kopyanın penceresini öne getir
            Shutdown();
            return;
        }

        // Windows ile başlayınca "--tray" ile gelir, o zaman pencere açılmadan tepside beklesin
        bool launchedAtLogin = e.Args.Contains("--tray");

        var window = new MainWindow();
        MainWindow = window;

        if (!(launchedAtLogin && window.StartHidden))
            window.Show();

        // Arka planda "göster" sinyalini bekle
        var listener = new Thread(() =>
        {
            while (evt.WaitOne())
            {
                try { Dispatcher.Invoke(() => window.ShowWindow()); }
                catch (Exception) { return; }   // uygulama kapanırken
            }
        })
        { IsBackground = true };
        listener.Start();
    }
}