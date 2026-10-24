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
            return JsonSerializer.Deserialize<AppData>(json, Options) ?? CreateDefault();
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
        Directory.CreateDirectory(Folder);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(data, Options));
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
        return imported?.Modes.ToList() ?? new List<Mode>();
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