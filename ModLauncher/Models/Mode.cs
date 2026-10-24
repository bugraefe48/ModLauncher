using System.Collections.ObjectModel;

namespace ModLauncher;

// Renk seçeneği: ekranda görünen ad ve renk kodu
public record ModeColor(string Name, string Value);

public static class ModeColors
{
    public static readonly List<ModeColor> All = new()
    {
        new("Renksiz", "Transparent"),
        new("Kırmızı", "#E74C3C"),
        new("Turuncu", "#F39C12"),
        new("Sarı", "#F1C40F"),
        new("Yeşil", "#2ECC71"),
        new("Mavi", "#3498DB"),
        new("Mor", "#9B59B6"),
        new("Pembe", "#E84393")
    };
}

public class Mode : NotifyBase
{
    string name = "";
    string hotkey = "";
    string color = "Transparent";

    public Guid Id { get; set; } = Guid.NewGuid();   // her modun benzersiz kimliği

    public string Name
    {
        get => name;
        set { name = value; OnPropertyChanged(); }
    }

    public string Hotkey   // örn. "Ctrl+Shift+1", boşsa kısayol yok
    {
        get => hotkey;
        set { hotkey = value; OnPropertyChanged(); }
    }

    public string Color   // listede modun yanındaki noktanın rengi
    {
        get => color;
        set { color = value; OnPropertyChanged(); }
    }

    public ObservableCollection<ModeAction> Actions { get; set; } = new();
}