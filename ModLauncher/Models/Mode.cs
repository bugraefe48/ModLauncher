using System.Collections.ObjectModel;

namespace ModLauncher;

public record ModeColor(string Name, string Value);

public static class ModeColors
{
    public static readonly List<ModeColor> All = new()
    {
        new("None", "Transparent"),
        new("Red", "#E74C3C"),
        new("Dark red", "#C0392B"),
        new("Orange", "#E67E22"),
        new("Amber", "#F39C12"),
        new("Yellow", "#F1C40F"),
        new("Lime", "#A3CB38"),
        new("Green", "#2ECC71"),
        new("Dark green", "#27AE60"),
        new("Teal", "#1ABC9C"),
        new("Cyan", "#00CEC9"),
        new("Sky", "#3498DB"),
        new("Blue", "#0984E3"),
        new("Indigo", "#6C5CE7"),
        new("Purple", "#9B59B6"),
        new("Pink", "#E84393"),
        new("Rose", "#FD79A8"),
        new("Brown", "#8D6E63"),
        new("Gray", "#95A5A6"),
        new("Charcoal", "#636E72"),
        new("White", "#FFFFFF")
    };

    // Vurgu rengi için uygun olanlar
    public static readonly List<ModeColor> Accents = All
        .Where(c => c.Value != "Transparent" && c.Value != "#FFFFFF"
                    && c.Value != "#95A5A6" && c.Value != "#636E72")
        .ToList();
}

public class Mode : NotifyBase
{
    string name = "";
    string hotkey = "";
    string color = "Transparent";

    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name
    {
        get => name;
        set { name = value; OnPropertyChanged(); }
    }

    public string Hotkey
    {
        get => hotkey;
        set { hotkey = value; OnPropertyChanged(); }
    }

    public string Color
    {
        get => color;
        set { color = value; OnPropertyChanged(); }
    }

    public ObservableCollection<ModeAction> Actions { get; set; } = new();
}
