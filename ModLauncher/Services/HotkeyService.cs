using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

namespace ModLauncher;

// Windows'a "bu tuşlara basılınca bana haber ver" diyen servis
public class HotkeyService : IDisposable
{
    [DllImport("user32.dll", SetLastError = true)]
    static extern bool RegisterHotKey(IntPtr hWnd, int id, uint modifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    const int WM_HOTKEY = 0x0312;          // Windows'un "kısayola basıldı" mesajı
    const uint MOD_NOREPEAT = 0x4000;      // tuş basılı kalınca tekrar tetikleme
    static readonly IntPtr HWND_MESSAGE = new(-3);

    readonly HwndSource source;            // ekranda görünmeyen mesaj penceresi
    readonly Dictionary<int, Mode> registered = new();
    readonly Action<Mode> onTriggered;
    int nextId = 1;

    public HotkeyService(Action<Mode> onTriggered)
    {
        this.onTriggered = onTriggered;

        // Pencereye bağımlı olmayan görünmez bir mesaj penceresi,
        // böylece ana pencere gizliyken ya da hiç açılmamışken de çalışır
        var parameters = new HwndSourceParameters("ModLauncherHotkeys")
        {
            ParentWindow = HWND_MESSAGE
        };
        source = new HwndSource(parameters);
        source.AddHook(WndProc);
    }

    // Tüm modların kısayollarını baştan kaydeder, kaydedilemeyen modları döndürür
    public List<Mode> RegisterAll(IEnumerable<Mode> modes)
    {
        foreach (var id in registered.Keys)
            UnregisterHotKey(source.Handle, id);
        registered.Clear();

        var failed = new List<Mode>();

        foreach (var mode in modes)
        {
            if (string.IsNullOrWhiteSpace(mode.Hotkey)) continue;

            if (!HotkeyHelper.TryParse(mode.Hotkey, out var modifiers, out var vk))
            {
                failed.Add(mode);
                continue;
            }

            int id = nextId++;
            if (RegisterHotKey(source.Handle, id, modifiers | MOD_NOREPEAT, vk))
                registered[id] = mode;
            else
                failed.Add(mode);   // başka bir program ya da başka bir mod kullanıyor
        }

        return failed;
    }

    IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_HOTKEY && registered.TryGetValue(wParam.ToInt32(), out var mode))
        {
            handled = true;
            onTriggered(mode);
        }
        return IntPtr.Zero;
    }

    public void Dispose()
    {
        foreach (var id in registered.Keys)
            UnregisterHotKey(source.Handle, id);
        registered.Clear();

        source.RemoveHook(WndProc);
        source.Dispose();
    }
}

// Tuş kombinasyonu <-> yazı dönüşümü ("Ctrl+Shift+1" gibi)
public static class HotkeyHelper
{
    public static string Format(ModifierKeys modifiers, Key key)
    {
        var parts = new List<string>();
        if (modifiers.HasFlag(ModifierKeys.Control)) parts.Add("Ctrl");
        if (modifiers.HasFlag(ModifierKeys.Alt)) parts.Add("Alt");
        if (modifiers.HasFlag(ModifierKeys.Shift)) parts.Add("Shift");
        if (modifiers.HasFlag(ModifierKeys.Windows)) parts.Add("Win");
        parts.Add(KeyToText(key));
        return string.Join("+", parts);
    }

    static string KeyToText(Key key) =>
        key >= Key.D0 && key <= Key.D9
            ? ((int)key - (int)Key.D0).ToString()   // D1 yerine 1 yaz
            : key.ToString();

    public static bool TryParse(string text, out uint modifiers, out uint vk)
    {
        modifiers = 0;
        vk = 0;
        Key? key = null;

        foreach (var raw in text.Split('+'))
        {
            var part = raw.Trim();
            switch (part)
            {
                case "Alt": modifiers |= 0x0001; break;
                case "Ctrl": modifiers |= 0x0002; break;
                case "Shift": modifiers |= 0x0004; break;
                case "Win": modifiers |= 0x0008; break;
                default:
                    if (part.Length == 1 && char.IsDigit(part[0]))
                        key = Key.D0 + (part[0] - '0');
                    else if (Enum.TryParse<Key>(part, out var parsed))
                        key = parsed;
                    else
                        return false;
                    break;
            }
        }

        if (modifiers == 0 || key is null) return false;
        if ((modifiers & 0x0003) == 0x0003) return false;   // Ctrl+Alt = AltGr, yasak

        vk = (uint)KeyInterop.VirtualKeyFromKey(key.Value);
        return vk != 0;
    }
}