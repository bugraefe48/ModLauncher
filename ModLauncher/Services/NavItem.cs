using System.Windows.Controls;

namespace ModLauncher;

public class NavItem : NotifyBase
{
    readonly Func<UserControl> create;
    UserControl? page;

    public string Key { get; }
    public string Icon { get; }
    public string Title => Loc.T(Key);

    public NavItem(string key, string icon, Func<UserControl> create)
    {
        Key = key;
        Icon = icon;
        this.create = create;

        // Dil değişince menü yazısı güncellensin
        Loc.Instance.PropertyChanged += (_, _) => OnPropertyChanged(nameof(Title));
    }

    // Sayfayı ilk açılışta oluşturur, sonra aynı sayfayı kullanır
    public UserControl GetPage() => page ??= create();
}
