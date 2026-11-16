namespace ModLauncher;

public static partial class Strings
{
    static readonly Dictionary<string, string> Tr = new();
    static readonly Dictionary<string, string> En = new();

    static void Add(string key, string tr, string en)
    {
        Tr[key] = tr;
        En[key] = en;
    }

    public static string Get(string language, string key)
    {
        var table = language == "tr" ? Tr : En;
        if (table.TryGetValue(key, out var value)) return value;
        return En.TryGetValue(key, out var fallback) ? fallback : key;
    }

    static Strings()
    {
        RegisterCore();
        RegisterTimer();
        RegisterStartupAndFavorites();
        RegisterPlanner();
    }

    // Sonraki paketlerde ayrı dosyalarda doldurulacak
    static partial void RegisterTimer();
    static partial void RegisterStartupAndFavorites();
    static partial void RegisterPlanner();

    static void RegisterCore()
    {
        // Menü ve genel
        Add("Nav_Home", "Ana Sayfa", "Home");
        Add("Nav_Modes", "Modlar", "Modes");
        Add("Nav_Settings", "Ayarlar", "Settings");
        Add("Help_Tooltip", "Nasıl kullanılır?", "How to use");
        Add("Common_Actions", "eylem", "actions");

        // Ana Sayfa
        Add("Home_Title", "Ana Sayfa", "Home");
        Add("Home_Subtitle", "Bir modu tek tıkla çalıştır. Uygulamalar açılır, siteler seçtiğin tarayıcıda açılır.", "Run a mode with one click. Apps launch and websites open in your chosen browser.");
        Add("Home_OpenHelp", "Nasıl kullanılır?", "How to use");
        Add("Home_QuickModes", "Hızlı modlar", "Quick modes");
        Add("Home_NoModes", "Henüz mod yok. Modlar sayfasından ilk modunu oluştur.", "No modes yet. Create your first mode on the Modes page.");
        Add("Home_GoModes", "Modlara git", "Go to Modes");
        Add("Home_Run", "▶ Çalıştır", "▶ Run");
        Add("Greet_Morning", "Günaydın", "Good morning");
        Add("Greet_Afternoon", "İyi günler", "Good afternoon");
        Add("Greet_Evening", "İyi akşamlar", "Good evening");
        Add("Greet_Night", "İyi geceler", "Good night");

        // Modlar sayfası
        Add("Modes_Empty", "Soldan bir mod seç ya da yeni mod ekle", "Select a mode on the left or add a new one");
        Add("Modes_New", "+ Yeni Mod", "+ New Mode");
        Add("Modes_NewName", "Yeni Mod", "New Mode");
        Add("Modes_General", "Genel", "General");
        Add("Modes_Name", "Mod adı", "Mode name");
        Add("Modes_Color", "Renk", "Color");
        Add("Modes_Hotkey", "Kısayol tuşu (kutuya tıkla, tuşlara bas)", "Hotkey (click the box and press the keys)");
        Add("Modes_HotkeyHint", "Ctrl, Shift ya da Win ile birlikte bir tuşa bas. Örnek: Ctrl+Shift+1. Ctrl+Alt kullanma, bazı klavyelerde AltGr ile karışır.", "Press a key together with Ctrl, Shift or Win. Example: Ctrl+Shift+1. Avoid Ctrl+Alt, it is the same as AltGr on some keyboards.");
        Add("Modes_Clear", "Temizle", "Clear");
        Add("Modes_Actions", "Eylemler", "Actions");
        Add("Modes_ActionsHint", "Eklediğin eylemler soldaki listede modun altında görünür. Sıra önemli, mesela önce kapat, sonra aç.", "Added actions appear under the mode in the list. Order matters, for example close first, then open.");
        Add("Modes_Run", "▶ Modu Çalıştır", "▶ Run Mode");
        Add("Modes_AddApp", "Uygulama Ekle", "Add App");
        Add("Modes_AddUrl", "Site Ekle", "Add Website");
        Add("Modes_AddClose", "Uygulama Kapat", "Close App");
        Add("Modes_Up", "▲ Yukarı", "▲ Up");
        Add("Modes_Down", "▼ Aşağı", "▼ Down");
        Add("Modes_Delete", "Modu Sil", "Delete Mode");

        // Eylem paneli
        Add("Action_Title", "Eylem", "Action");
        Add("Action_Address", "Site adresi", "Website address");
        Add("Action_AppPath", "Program yolu", "Program path");
        Add("Action_ClosePath", "Kapatılacak uygulama (exe yolu ya da işlem adı)", "App to close (exe path or process name)");
        Add("Action_Browse", "Gözat...", "Browse...");
        Add("Action_Args", "Ek komutlar (isteğe bağlı)", "Extra arguments (optional)");
        Add("Action_Browser", "Tarayıcı (boşsa ayarlardaki tarayıcı kullanılır)", "Browser (uses the one in Settings if empty)");
        Add("Action_Force", "Kapanmazsa zorla kapat", "Force close if it does not close");
        Add("Action_ForceWarn", "Dikkat: zorla kapatılan uygulamada kaydedilmemiş işler kaybolur.", "Warning: unsaved work is lost when an app is force closed.");
        Add("Action_Test", "▶ Eylemi Dene", "▶ Test Action");
        Add("Action_Delete", "Eylemi Sil", "Delete Action");

        // Dosya pencereleri
        Add("Dlg_PickApp", "Açılacak uygulamayı seç", "Choose the app to open");
        Add("Dlg_PickCloseApp", "Kapatılacak uygulamayı seç", "Choose the app to close");
        Add("Dlg_PickBrowser", "Tarayıcıyı seç", "Choose a browser");
        Add("Dlg_Apps", "Uygulamalar", "Applications");
        Add("Dlg_AllFiles", "Tüm dosyalar", "All files");
        Add("Dlg_ExportTitle", "Modları dışa aktar", "Export modes");
        Add("Dlg_ImportTitle", "Modları içe aktar", "Import modes");
        Add("Dlg_ModeFile", "Mod dosyası", "Mode file");

        // Mesajlar
        Add("Msg_NoActions", "Bu modda henüz eylem yok.", "This mode has no actions yet.");
        Add("Msg_NoActionsTitle", "Mod boş", "Empty mode");
        Add("Msg_RunConfirm", "\"{0}\" modu çalıştırılsın mı? ({1} eylem)", "Run mode \"{0}\"? ({1} actions)");
        Add("Msg_RunTitle", "Modu çalıştır", "Run mode");
        Add("Msg_ActionsFailedTitle", "Bazı eylemler çalışmadı", "Some actions failed");
        Add("Msg_ActionFailedTitle", "Eylem çalışmadı", "Action failed");
        Add("Msg_DeleteMode", "\"{0}\" modu ve içindeki tüm eylemler silinsin mi?", "Delete mode \"{0}\" and all its actions?");
        Add("Msg_DeleteModeTitle", "Modu sil", "Delete mode");
        Add("Msg_HotkeyNeedMod", "Kısayol için Ctrl, Shift ya da Win tuşlarından biriyle birlikte bas. Örnek: Ctrl+Shift+1", "Press the key together with Ctrl, Shift or Win. Example: Ctrl+Shift+1");
        Add("Msg_HotkeyCtrlAlt", "Ctrl+Alt birlikte kullanılamaz, bazı klavyelerde AltGr ile aynıdır ve karakter yazmayı bozar. Ctrl+Shift ya da Alt+Shift dene.", "Ctrl+Alt cannot be used together because it is the same as AltGr on many keyboards and breaks typing some characters. Try Ctrl+Shift or Alt+Shift.");
        Add("Msg_HotkeyInUse", "Bu kısayol başka bir program ya da başka bir mod tarafından kullanılıyor. Başka bir tane dene.", "This hotkey is used by another program or mode. Try a different one.");
        Add("Msg_HotkeyTitle", "Kısayol", "Hotkey");
        Add("Msg_HotkeyInUseTitle", "Kısayol kullanımda", "Hotkey in use");

        // Çalıştırma hataları
        Add("Err_EmptyTarget", "Hedef boş.", "The target is empty.");
        Add("Err_FileNotFound", "Dosya bulunamadı: {0}\nUygulama taşınmış ya da silinmiş olabilir. Eylem bir masaüstü kısayolunu gösteriyorsa, kısayol yerine programın kendi .exe dosyasını seç.", "File not found: {0}\nThe app may have been moved or deleted. If the action points to a desktop shortcut, choose the program's own .exe instead.");
        Add("Err_AppNotFound", "\"{0}\" adında bir uygulama bulunamadı. Programın tam yolunu (örn. C:\\...\\brave.exe) yazmayı dene.", "No app named \"{0}\" was found. Try entering the full path of the program (for example C:\\...\\brave.exe).");
        Add("Err_BrowserNotFound", "Tarayıcı bulunamadı: {0}", "Browser not found: {0}");
        Add("Err_BadUrl", "Geçerli bir http ya da https adresi değil.", "Not a valid http or https address.");
        Add("Err_NoWindow", "Açık pencere bulunamadı (arka planda çalışıyor olabilir). 'Kapanmazsa zorla kapat' seçeneğini işaretleyebilirsin.", "No open window found (it may be running in the background). You can enable 'Force close if it does not close'.");
        Add("Err_NotClosed", "Uygulama kapanmadı (kaydedilmemiş bir iş olabilir).", "The app did not close (there may be unsaved work).");
        Add("Err_Unexpected", "Beklenmeyen bir hata oluştu:\n\n{0}\n\nUygulama çalışmaya devam edecek. Sorun tekrarlanırsa GitHub sayfasından bildirebilirsin.", "An unexpected error occurred:\n\n{0}\n\nThe app will keep running. If this keeps happening, you can report it on the GitHub page.");

        // Tepsi
        Add("Tray_Open", "Mod Launcher'ı aç", "Open Mod Launcher");
        Add("Tray_NoModes", "(Henüz mod yok)", "(No modes yet)");
        Add("Tray_Exit", "Çıkış", "Exit");
        Add("Tray_HotkeyFailTitle", "Bazı kısayollar kaydedilemedi", "Some hotkeys could not be registered");
        Add("Tray_HotkeyClearedTitle", "Bazı kısayollar temizlendi", "Some hotkeys were cleared");
        Add("Tray_HotkeyClearedText", "İçe aktarılan bazı kısayollar kullanımda olduğu için silindi.", "Some imported hotkeys were removed because they are already in use.");

        // Ayarlar
        Add("Settings_Appearance", "Görünüm", "Appearance");
        Add("Settings_Theme", "Tema", "Theme");
        Add("Theme_System", "Windows temasını takip et", "Follow Windows theme");
        Add("Theme_Dark", "Koyu", "Dark");
        Add("Theme_Light", "Açık", "Light");
        Add("Settings_Accent", "Vurgu rengi", "Accent color");
        Add("Settings_AccentSystem", "Windows vurgu rengini kullan", "Use Windows accent color");
        Add("Settings_Language", "Dil", "Language");
        Add("Settings_Browser", "Tarayıcı", "Browser");
        Add("Settings_BrowserLabel", "Sitelerin açılacağı genel tarayıcı", "Default browser for websites");
        Add("Settings_BrowserHint", "Bir eylemde ayrıca tarayıcı seçtiysen o eylem için o geçerli olur.", "A browser chosen on a specific action overrides this one.");
        Add("Browser_Default", "Windows varsayılanı", "Windows default");
        Add("Browser_Custom", "Özel: {0}", "Custom: {0}");
        Add("Settings_Run", "Mod çalıştırma", "Running modes");
        Add("Settings_Confirm", "Modu çalıştırmadan önce onay sor", "Ask before running a mode");
        Add("Settings_Delay", "Eylemler arası bekleme:", "Delay between actions:");
        Add("Settings_Startup", "Başlangıç ve tepsi", "Startup and tray");
        Add("Settings_Tray", "Pencereyi kapatınca sistem tepsisine küçül", "Minimize to the system tray when the window is closed");
        Add("Settings_StartWithWindows", "Windows ile birlikte başlat", "Start with Windows");
        Add("Settings_StartupHint", "Windows açılınca pencere hiç açılmadan tepside beklemesi için ikisini de işaretle.", "Enable both to start silently in the tray when Windows starts.");
        Add("Settings_Backup", "Yedekleme", "Backup");
        Add("Settings_Export", "Modları dışa aktar...", "Export modes...");
        Add("Settings_Import", "Modları içe aktar...", "Import modes...");
        Add("Settings_BackupHint", "İçe aktarılan modlar mevcut modların üstüne yazılmaz, listeye eklenir. Ayarlar bilgisayara özel olduğu için aktarılmaz.", "Imported modes are added to the list and never overwrite existing ones. Settings are specific to each computer and are not exported.");
        Add("Settings_About", "Hakkında", "About");
        Add("Settings_Version", "Mod Launcher sürüm {0}", "Mod Launcher version {0}");
        Add("Exp_Title", "Dışa aktar", "Export");
        Add("Exp_Done", "{0} mod dışa aktarıldı.", "{0} modes exported.");
        Add("Exp_Fail", "Dosya yazılamadı: {0}", "Could not write the file: {0}");
        Add("Imp_Title", "İçe aktar", "Import");
        Add("Imp_ReadFail", "Dosya okunamadı ya da geçerli bir mod dosyası değil.\n{0}", "The file could not be read or is not a valid mode file.\n{0}");
        Add("Imp_Confirm", "Dosyada {0} mod var. Bu modlar {1} program açacak ve {2} program kapatacak.\n\nSadece güvendiğin kişilerden aldığın dosyaları içe aktar. Devam edilsin mi?", "The file contains {0} modes. They will open {1} programs and close {2} programs.\n\nOnly import files from people you trust. Continue?");
        Add("Imp_Done", "{0} mod eklendi.", "{0} modes added.");
        Add("Imp_NewName", "İçe aktarılan mod", "Imported mode");

        // Yardım
        Add("Help_Title", "Nasıl kullanılır?", "How to use");
        Add("Help_T_Welcome", "Hoş geldin", "Welcome");
        Add("Help_B_Welcome", "Mod Launcher, tek tıkla bir grup işi yapar. Bir \"mod\" oluştur, içine açılacak uygulamaları ve siteleri ekle, sonra moda tıklayınca hepsi sırayla çalışsın.\n\nÖrnek: \"Ders Modu\" için ders sitesini aç, not uygulamanı başlat, oyunu kapat.\n\nSol menüden sayfalar arasında gezebilirsin. Bu pencereye istediğin zaman sol alttaki ? tuşuyla dönebilirsin.", "Mod Launcher does a group of tasks with one click. Create a \"mode\", add the apps and websites to open, then click the mode and everything runs in order.\n\nExample: for a \"Study Mode\", open your course site, start your notes app and close your game.\n\nUse the menu on the left to move between pages. You can come back to this window any time with the ? button at the bottom left.");
        Add("Help_T_Modes", "Modlar", "Modes");
        Add("Help_B_Modes", "Modlar sayfasında soldaki listeden bir mod seç ya da + Yeni Mod ile yenisini oluştur. Adını ve rengini sağ panelden değiştirebilirsin; renk, listede ve Ana Sayfa'da modun yanında yuvarlak olarak görünür.\n\n▲ Yukarı ve ▼ Aşağı ile modları ya da eylemleri sıralayabilirsin. ▶ Modu Çalıştır eylemleri yukarıdan aşağı sırayla çalıştırır.", "On the Modes page pick a mode from the list on the left or create one with + New Mode. You can change its name and color in the right panel; the color shows as a dot next to the mode in the list and on the Home page.\n\nUse ▲ Up and ▼ Down to reorder modes or actions. ▶ Run Mode runs the actions from top to bottom.");
        Add("Help_T_Actions", "Eylemler", "Actions");
        Add("Help_B_Actions", "Üç tür eylem var:\n• Uygulama Ekle: bir programı açar. Masaüstü kısayolu seçersen gerçek programa çevrilir.\n• Site Ekle: bir web adresini açar. Tarayıcıyı Ayarlar'dan ya da eylemin kendisinden seçebilirsin.\n• Uygulama Kapat: çalışan bir programı kapatır. Önce nazikçe kapatmayı dener, istersen \"zorla kapat\" seçeneğini işaretleyebilirsin.\n\nBir eylemi seçip ▶ Eylemi Dene ile tek başına deneyebilirsin.", "There are three kinds of actions:\n• Add App: opens a program. If you pick a desktop shortcut it is converted to the real program.\n• Add Website: opens a web address. Choose the browser in Settings or on the action itself.\n• Close App: closes a running program. It tries to close it politely first; you can enable \"force close\" if needed.\n\nSelect an action and press ▶ Test Action to try it on its own.");
        Add("Help_T_Hotkeys", "Kısayol tuşları", "Hotkeys");
        Add("Help_B_Hotkeys", "Her moda genel bir kısayol atayabilirsin. Kısayol kutusuna tıkla ve tuşlara bas (örn. Ctrl+Shift+1). Uygulama tepside beklerken bile çalışır ve onay sormaz.\n\nCtrl+Alt kombinasyonu kullanılamaz; bazı klavyelerde AltGr ile aynıdır ve @, {, [ gibi karakterleri bozar. Kısayolu silmek için Temizle'ye bas.", "You can give every mode a global hotkey. Click the hotkey box and press the keys (for example Ctrl+Shift+1). It works even while the app waits in the tray and it does not ask for confirmation.\n\nCtrl+Alt cannot be used; on some keyboards it is the same as AltGr and breaks characters like @, { and [. Press Clear to remove a hotkey.");
        Add("Help_T_Tray", "Sistem tepsisi ve başlangıç", "Tray and startup");
        Add("Help_B_Tray", "Pencereyi kapatınca uygulamanın tepside (saatin yanındaki simge) beklemesini istersen Ayarlar > Başlangıç ve tepsi bölümünden tepsiye küçül seçeneğini aç. Simgeye sol tıklayınca pencere açılır, sağ tıklayınca modlar listelenir ve tek tıkla çalıştırabilirsin.\n\nWindows ile birlikte başlat seçeneği, bilgisayar açılınca uygulamayı otomatik başlatır.", "If you want the app to keep waiting in the tray (icon next to the clock) when you close the window, enable minimize to tray under Settings > Startup and tray. Left click the icon to open the window, right click to list the modes and run one with a click.\n\nStart with Windows launches the app automatically when the computer starts.");
        Add("Help_T_Settings", "Ayarlar", "Settings");
        Add("Help_B_Settings", "Ayarlar'dan tema (koyu, açık ya da Windows'u takip et), vurgu rengi, dil, tarayıcı, onay sorusu ve eylemler arası bekleme süresini değiştirebilirsin. Değişiklikler anında kaydedilir.", "In Settings you can change the theme (dark, light or follow Windows), accent color, language, browser, the confirmation prompt and the delay between actions. Changes are saved instantly.");
        Add("Help_T_Backup", "Yedekleme", "Backup");
        Add("Help_B_Backup", "Ayarlar > Yedekleme'den tüm modlarını bir dosyaya aktarabilir, başka bir bilgisayarda içe aktarabilirsin. İçe aktarılan modlar mevcutların üstüne yazılmaz, listeye eklenir. Bir dosyadaki modlar program çalıştırabileceği için dosyayı yalnızca güvendiğin kişilerden al.", "Under Settings > Backup you can export all your modes to a file and import them on another computer. Imported modes are added to the list and do not overwrite existing ones. Modes in a file can run programs, so only import files from people you trust.");
    }
}
