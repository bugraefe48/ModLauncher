using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace ModLauncher;

// Dil yöneticisi: metinleri seçili dile göre verir, dil değişince arayüzü günceller
public sealed class Loc : INotifyPropertyChanged
{
    public static Loc Instance { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Language { get; private set; } = "en";

    public string this[string key] => Strings.Get(Language, key);

    public static string T(string key) => Instance[key];

    public static string T(string key, params object[] args) =>
        string.Format(CultureInfo.CurrentCulture, Instance[key], args);

    // "tr" ya da "en" döndürür, boşsa Windows'un diline bakar
    public static string Resolve(string? code)
    {
        if (code == "tr") return "tr";
        if (code == "en") return "en";
        return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "tr" ? "tr" : "en";
    }

    public void SetLanguage(string? code)
    {
        var resolved = Resolve(code);
        if (resolved == Language) return;

        Language = resolved;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Language)));
    }
}

// XAML'de kullanım: Text="{local:T Anahtar}"
[MarkupExtensionReturnType(typeof(string))]
public class TExtension : MarkupExtension
{
    public string Key { get; set; } = "";

    public TExtension() { }

    public TExtension(string key) { Key = key; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var binding = new Binding($"[{Key}]")
        {
            Source = Loc.Instance,
            Mode = BindingMode.OneWay
        };
        return binding.ProvideValue(serviceProvider);
    }
}
