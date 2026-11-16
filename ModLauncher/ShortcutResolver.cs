using System.Runtime.InteropServices;

namespace ModLauncher;

public static class ShortcutResolver
{
    public static (string Target, string Arguments)? Resolve(string shortcutPath)
    {
        try
        {
            var type = Type.GetTypeFromProgID("WScript.Shell");
            if (type == null) return null;

            dynamic shell = Activator.CreateInstance(type)!;
            dynamic shortcut = shell.CreateShortcut(shortcutPath);

            string target = shortcut.TargetPath ?? "";
            string arguments = shortcut.Arguments ?? "";

            Marshal.FinalReleaseComObject(shortcut);
            Marshal.FinalReleaseComObject(shell);

            if (string.IsNullOrWhiteSpace(target)) return null;
            return (target, arguments);
        }
        catch (Exception)
        {
            return null;
        }
    }
}