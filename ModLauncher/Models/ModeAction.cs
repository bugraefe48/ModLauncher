using System.IO;
using System.Text.Json.Serialization;

namespace ModLauncher;

public enum ActionType
{
    App,
    Url,
    CloseApp
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

    public string Target
    {
        get => target;
        set
        {
            // Kısayol (.lnk) seçildiyse gösterdiği gerçek programa çevir
            if (type != ActionType.Url && value is not null &&
                value.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase) && File.Exists(value))
            {
                var resolved = ShortcutResolver.Resolve(value);
                if (resolved != null)
                {
                    value = resolved.Value.Target;
                    if (string.IsNullOrWhiteSpace(arguments) &&
                        !string.IsNullOrWhiteSpace(resolved.Value.Arguments))
                    {
                        Arguments = resolved.Value.Arguments;
                    }
                }
            }

            target = value ?? "";
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayName));
        }
    }

    public string Arguments
    {
        get => arguments;
        set { arguments = value; OnPropertyChanged(); }
    }

    public string BrowserPath
    {
        get => browserPath;
        set { browserPath = value; OnPropertyChanged(); }
    }

    public bool ForceClose
    {
        get => forceClose;
        set { forceClose = value; OnPropertyChanged(); }
    }

    public static string ProcessNameOf(string target) =>
        Path.GetFileNameWithoutExtension(target.Trim());

    [JsonIgnore]
    public string DisplayName => Type == ActionType.Url ? Target : ProcessNameOf(Target);
}
