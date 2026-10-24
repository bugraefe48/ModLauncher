using System.IO;
using System.Text.Json.Serialization;

namespace ModLauncher;

public enum ActionType
{
    App,        // bir programı aç
    Url,        // bir web sitesi aç
    CloseApp    // çalışan bir programı kapat
}

public class ModeAction : NotifyBase
{
    ActionType type = ActionType.App;
    string target = "";
    string arguments = "";
    string browserPath = "";
    bool forceClose;

    public ActionType Type
    {
        get => type;
        set { type = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayName)); }
    }

    public string Target   // programın yolu, işlem adı ya da sitenin adresi
    {
        get => target;
        set { target = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayName)); }
    }

    public string Arguments   // program için isteğe bağlı ek komutlar
    {
        get => arguments;
        set { arguments = value; OnPropertyChanged(); }
    }

    public string BrowserPath   // boşsa ayarlardaki tarayıcı kullanılır
    {
        get => browserPath;
        set { browserPath = value; OnPropertyChanged(); }
    }

    public bool ForceClose   // sadece CloseApp için: kapanmazsa zorla kapat
    {
        get => forceClose;
        set { forceClose = value; OnPropertyChanged(); }
    }

    // "C:\...\chrome.exe" ya da "chrome" yazısından işlem adını ("chrome") çıkarır
    public static string ProcessNameOf(string target) =>
        Path.GetFileNameWithoutExtension(target.Trim());

    // Listede görünen ad (JSON dosyasına yazılmaz)
    [JsonIgnore]
    public string DisplayName => Type switch
    {
        ActionType.Url => Target,
        ActionType.CloseApp => "Kapat: " + ProcessNameOf(Target),
        _ => ProcessNameOf(Target)
    };
}