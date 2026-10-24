namespace ModLauncher;

public class TrayService : IDisposable
{
    readonly System.Windows.Forms.NotifyIcon icon;
    readonly AppData data;
    readonly Action showWindow;
    readonly Action exitApp;

    public TrayService(AppData data, Action showWindow, Action exitApp)
    {
        this.data = data;
        this.showWindow = showWindow;
        this.exitApp = exitApp;

        var menu = new System.Windows.Forms.ContextMenuStrip();
        menu.Opening += (_, _) => BuildMenu(menu);   // her açılışta modları yeniden listele

        icon = new System.Windows.Forms.NotifyIcon
        {
            // Uygulamanın kendi simgesi (exe'nin ikonu)
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(Environment.ProcessPath!)
                   ?? System.Drawing.SystemIcons.Application,
            Text = "Mod Launcher",
            ContextMenuStrip = menu,
            Visible = true
        };

        // Sol tık: pencereyi aç
        icon.MouseClick += (_, e) =>
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                showWindow();
        };
    }

    void BuildMenu(System.Windows.Forms.ContextMenuStrip menu)
    {
        menu.Items.Clear();
        menu.Items.Add("Mod Launcher'ı aç", null, (_, _) => showWindow());
        menu.Items.Add(new System.Windows.Forms.ToolStripSeparator());

        if (data.Modes.Count == 0)
        {
            var empty = menu.Items.Add("(Henüz mod yok)");
            empty.Enabled = false;
        }

        foreach (var mode in data.Modes.ToList())
        {
            menu.Items.Add("▶ " + mode.Name, null, async (_, _) => await RunAsync(mode));
        }

        menu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
        menu.Items.Add("Çıkış", null, (_, _) => exitApp());
    }

    // Tepsiden ya da kısayoldan çalıştırılan modlar onay sormaz, çünkü seçim bilinçli
    public async Task RunAsync(Mode mode)
    {
        var errors = await ModeRunner.RunAsync(mode, data.Settings);

        if (errors.Count > 0)
            Notify("Bazı eylemler çalışmadı", string.Join("\n", errors));
    }

    // Saatin yanında küçük bir bildirim balonu gösterir
    public void Notify(string title, string text)
    {
        icon.ShowBalloonTip(4000, title, text, System.Windows.Forms.ToolTipIcon.Warning);
    }

    public void Dispose()
    {
        icon.Visible = false;   // yoksa kapanınca simge tepside hayalet gibi kalabilir
        icon.Dispose();
    }
}