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
    public string AccentColor { get; set; } = "";       // "#RRGGBB", boşsa Windows vurgu rengi
    public string Language { get; set; } = "";          // "tr" ya da "en", boşsa Windows dili
    public string DefaultBrowserPath { get; set; } = "";
    public bool ConfirmBeforeRun { get; set; } = true;
    public int DelayBetweenActionsMs { get; set; } = 500;
    public bool StartWithWindows { get; set; } = false;
    public bool MinimizeToTray { get; set; } = false;
    public bool TourShown { get; set; } = false;        // karşılama yardımı gösterildi mi
}

public class AppData
{
    public AppSettings Settings { get; set; } = new();
    public ObservableCollection<Mode> Modes { get; set; } = new();
}
