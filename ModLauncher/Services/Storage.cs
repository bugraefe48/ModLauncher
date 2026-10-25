using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ModLauncher;

public static class Storage
{
    static readonly string Folder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ModLauncher");

    public static readonly string FilePath = Path.Combine(Folder, "data.json");

    static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,                           // dosya okunaklı olsun
        Converters = { new JsonStringEnumConverter() }  // enum'lar sayı değil yazı olarak kaydedilsin
    };

    public static AppData Load()
    {
        // İlk çalıştırma: dosya yoksa örnek verilerle oluştur
        if (!File.Exists(FilePath))
        {
            var fresh = CreateDefault();
            Save(fresh);
            return fresh;
        }

        try
        {
            var json = File.ReadAllText(FilePath);
            var data = JsonSerializer.Deserialize<AppData>(json, Options);
            return data == null ? CreateDefault() : Sanitize(data);
        }
        catch (JsonException)
        {
            // Dosya bozulmuşsa yedeğini al, uygulama çökmesin
            File.Copy(FilePath, FilePath + ".bozuk", overwrite: true);
            return CreateDefault();
        }
    }

    public static void Save(AppData data)
    {
        try
        {
            Directory.CreateDirectory(Folder);

            // Önce geçici dosyaya yaz, sonra asıl dosyanın yerine koy:
            // yazma sırasında bir şey ters giderse eski dosya bozulmaz
            var temp = FilePath + ".tmp";
            File.WriteAllText(temp, JsonSerializer.Serialize(data, Options));
            File.Move(temp, FilePath, overwrite: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Kaydedilemedi (disk dolu, dosya kilitli vb.), uygulama çökmesin.
            // Bir sonraki değişiklikte tekrar denenecek.
        }
    }

    // Sadece modları bir dosyaya yazar (ayarlar bilgisayara özel olduğu için dahil değil)
    public static void Export(string path, IEnumerable<Mode> modes)
    {
        var export = new AppData();
        foreach (var mode in modes)
            export.Modes.Add(mode);

        File.WriteAllText(path, JsonSerializer.Serialize(export, Options));
    }

    // Bir dosyadaki modları okur
    public static List<Mode> Import(string path)
    {
        var json = File.ReadAllText(path);
        var imported = JsonSerializer.Deserialize<AppData>(json, Options);
        return imported == null ? new List<Mode>() : Sanitize(imported).Modes.ToList();
    }

    // Bozuk ya da elle düzenlenmiş dosyalarda boş (null) kalan alanları düzeltir
    static AppData Sanitize(AppData data)
    {
        data.Settings ??= new AppSettings();
        data.Modes ??= new ObservableCollection<Mode>();

        for (int i = data.Modes.Count - 1; i >= 0; i--)
        {
            var mode = data.Modes[i];
            if (mode == null)
            {
                data.Modes.RemoveAt(i);
                continue;
            }

            mode.Name ??= "";
            mode.Hotkey ??= "";
            mode.Color ??= "Transparent";
            mode.Actions ??= new ObservableCollection<ModeAction>();

            for (int j = mode.Actions.Count - 1; j >= 0; j--)
            {
                var action = mode.Actions[j];
                if (action == null)
                {
                    mode.Actions.RemoveAt(j);
                    continue;
                }

                action.Target ??= "";
                action.Arguments ??= "";
                action.BrowserPath ??= "";
            }
        }

        return data;
    }

    static AppData CreateDefault() => new()
    {
        Modes =
        {
            new Mode
            {
                Name = "Ders Modu",
                Actions = { new ModeAction { Type = ActionType.Url, Target = "https://www.youtube.com" } }
            },
            new Mode { Name = "Oyun Modu" }
        }
    };
}