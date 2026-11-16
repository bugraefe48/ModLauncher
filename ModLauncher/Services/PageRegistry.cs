namespace ModLauncher;

// Soldaki menüde görünen sayfalar. Yeni sayfalar buraya eklenir.
public static class PageRegistry
{
    public static List<NavItem> Build(AppServices services) => new()
    {
        new NavItem("Nav_Home", "\uE80F", () => new HomePage(services)),
        new NavItem("Nav_Modes", "\uE71D", () => new ModesPage(services)),
        new NavItem("Nav_Settings", "\uE713", () => new SettingsPage(services))
    };
}
