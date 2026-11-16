namespace ModLauncher;

public static class HelpContent
{
    // (başlık anahtarı, metin anahtarı)
    public static readonly List<(string Title, string Body)> Topics = new()
    {
        ("Help_T_Welcome", "Help_B_Welcome"),
        ("Help_T_Modes", "Help_B_Modes"),
        ("Help_T_Actions", "Help_B_Actions"),
        ("Help_T_Hotkeys", "Help_B_Hotkeys"),
        ("Help_T_Tray", "Help_B_Tray"),
        ("Help_T_Settings", "Help_B_Settings"),
        ("Help_T_Backup", "Help_B_Backup")
    };
}
