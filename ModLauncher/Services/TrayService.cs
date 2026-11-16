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
        menu.Opening += (_, _) => BuildMenu(menu);

        icon = new System.Windows.Forms.NotifyIcon
        {
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(Environment.ProcessPath!)
                   ?? System.Drawing.SystemIcons.Application,
            Text = "Mod Launcher",
            ContextMenuStrip = menu,
            Visible = true
        };

        icon.MouseClick += (_, e) =>
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                showWindow();
        };
    }

    void BuildMenu(System.Windows.Forms.ContextMenuStrip menu)
    {
        menu.Items.Clear();
        menu.Items.Add(Loc.T("Tray_Open"), null, (_, _) => showWindow());
        menu.Items.Add(new System.Windows.Forms.ToolStripSeparator());

        if (data.Modes.Count == 0)
        {
            var empty = menu.Items.Add(Loc.T("Tray_NoModes"));
            empty.Enabled = false;
        }

        foreach (var mode in data.Modes.ToList())
        {
            menu.Items.Add("▶ " + mode.Name, null, async (_, _) => await RunAsync(mode));
        }

        menu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
        menu.Items.Add(Loc.T("Tray_Exit"), null, (_, _) => exitApp());
    }

    // Tepsiden ya da kısayoldan çalıştırılan modlar onay sormaz
    public async Task RunAsync(Mode mode)
    {
        var errors = await ModeRunner.RunAsync(mode, data.Settings);

        if (errors.Count > 0)
            Notify(Loc.T("Msg_ActionsFailedTitle"), string.Join("\n", errors));
    }

    public void Notify(string title, string text)
    {
        icon.ShowBalloonTip(4000, title, text, System.Windows.Forms.ToolTipIcon.Warning);
    }

    public void Dispose()
    {
        icon.Visible = false;
        icon.Dispose();
    }
}
