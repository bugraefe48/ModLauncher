using System.Collections.ObjectModel;

namespace ModLauncher;

public enum AppTheme
{
    System,
    Dark,
    Light
}

public class AppSettings
{
    public AppTheme Theme { get; set; } = AppTheme.System;
    public string DefaultBrowserPath { get; set; } = "";   // boşsa Windows'un varsayılan tarayıcısı
    public bool ConfirmBeforeRun { get; set; } = true;     // modu çalıştırmadan önce sor
    public int DelayBetweenActionsMs { get; set; } = 500;  // eylemler arası bekleme (milisaniye)
    public bool StartWithWindows { get; set; } = false;
    public bool MinimizeToTray { get; set; } = false;      // kapatınca tepsiye küçül
}

public class AppData
{
    public AppSettings Settings { get; set; } = new();
    public ObservableCollection<Mode> Modes { get; set; } = new();
}