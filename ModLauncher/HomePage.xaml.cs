using System.Windows;
using System.Windows.Controls;

namespace ModLauncher;

public partial class HomePage : UserControl
{
    readonly AppServices services;

    public HomePage(AppServices services)
    {
        InitializeComponent();
        this.services = services;

        ModeCards.ItemsSource = services.Data.Modes;
        services.Data.Modes.CollectionChanged += (_, _) => UpdateEmpty();
        Loc.Instance.PropertyChanged += (_, _) => UpdateGreeting();

        UpdateEmpty();
        UpdateGreeting();
    }

    void UpdateEmpty()
    {
        EmptyPanel.Visibility = services.Data.Modes.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    void UpdateGreeting()
    {
        int hour = DateTime.Now.Hour;
        string key = hour < 6 ? "Greet_Night"
            : hour < 12 ? "Greet_Morning"
            : hour < 18 ? "Greet_Afternoon"
            : hour < 23 ? "Greet_Evening"
            : "Greet_Night";
        GreetingText.Text = Loc.T(key);
    }

    async void RunCard_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is not Mode mode) return;
        await services.RunModeFromUiAsync(mode);
    }

    void Help_Click(object sender, RoutedEventArgs e) => services.OpenHelp();

    void GoModes_Click(object sender, RoutedEventArgs e) => services.NavigateByKey?.Invoke("Nav_Modes");
}
