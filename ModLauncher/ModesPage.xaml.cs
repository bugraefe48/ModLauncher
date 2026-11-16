using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ModLauncher;

public partial class ModesPage : UserControl
{
    readonly AppServices services;
    object? selected;

    AppData Data => services.Data;

    static string AppFilter =>
        $"{Loc.T("Dlg_Apps")} (*.exe;*.lnk)|*.exe;*.lnk|{Loc.T("Dlg_AllFiles")} (*.*)|*.*";

    static string ExeFilter =>
        $"{Loc.T("Dlg_Apps")} (*.exe)|*.exe|{Loc.T("Dlg_AllFiles")} (*.*)|*.*";

    static Visibility Vis(bool visible) => visible ? Visibility.Visible : Visibility.Collapsed;

    public ModesPage(AppServices services)
    {
        InitializeComponent();
        this.services = services;

        ModeTree.ItemsSource = Data.Modes;
        ColorList.ItemsSource = ModeColors.All;
        Loc.Instance.PropertyChanged += (_, _) => ShowPanel(selected);
        ShowPanel(null);
    }

    // ---------- Sağ paneli seçime göre göster ----------

    void ShowPanel(object? item)
    {
        selected = item;
        EmptyText.Visibility = Visibility.Visible;
        ModePanel.Visibility = Visibility.Collapsed;
        ActionPanel.Visibility = Visibility.Collapsed;

        if (item is Mode mode)
        {
            ModePanel.DataContext = mode;
            ModePanel.Visibility = Visibility.Visible;
            EmptyText.Visibility = Visibility.Collapsed;
        }
        else if (item is ModeAction action)
        {
            bool isUrl = action.Type == ActionType.Url;
            bool isClose = action.Type == ActionType.CloseApp;

            ActionPanel.DataContext = action;
            TargetLabel.Text = isUrl ? Loc.T("Action_Address")
                : isClose ? Loc.T("Action_ClosePath")
                : Loc.T("Action_AppPath");

            BrowseTargetButton.Visibility = Vis(!isUrl);
            ArgsPanel.Visibility = Vis(!isUrl && !isClose);
            BrowserPanel.Visibility = Vis(isUrl);
            ForcePanel.Visibility = Vis(isClose);

            ActionPanel.Visibility = Visibility.Visible;
            EmptyText.Visibility = Visibility.Collapsed;
        }
    }

    void ModeTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        => ShowPanel(e.NewValue);

    // ---------- Ağaçta öğe seçme ----------

    void SelectMode(Mode mode)
    {
        ModeTree.UpdateLayout();
        if (ModeTree.ItemContainerGenerator.ContainerFromItem(mode) is TreeViewItem item)
        {
            item.IsSelected = true;
            item.BringIntoView();
        }
    }

    void SelectAction(Mode mode, ModeAction action)
    {
        ModeTree.UpdateLayout();
        if (ModeTree.ItemContainerGenerator.ContainerFromItem(mode) is TreeViewItem parent)
        {
            parent.IsExpanded = true;
            parent.UpdateLayout();
            if (parent.ItemContainerGenerator.ContainerFromItem(action) is TreeViewItem child)
            {
                child.IsSelected = true;
                child.BringIntoView();
            }
            else
            {
                parent.IsSelected = true;
            }
        }
    }

    // ---------- Kısayol ----------

    void HotkeyBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        e.Handled = true;
        if (selected is not Mode mode) return;

        var key = e.Key == Key.System ? e.SystemKey : e.Key;

        if (key is Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt
            or Key.LeftShift or Key.RightShift or Key.LWin or Key.RWin
            or Key.None or Key.ImeProcessed)
            return;

        var modifiers = Keyboard.Modifiers;

        if (modifiers == ModifierKeys.None && (key is Key.Back or Key.Delete))
        {
            SetHotkey(mode, "");
            return;
        }

        if (modifiers == ModifierKeys.None)
        {
            MessageBox.Show(Loc.T("Msg_HotkeyNeedMod"), Loc.T("Msg_HotkeyTitle"),
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (modifiers.HasFlag(ModifierKeys.Control) && modifiers.HasFlag(ModifierKeys.Alt))
        {
            MessageBox.Show(Loc.T("Msg_HotkeyCtrlAlt"), Loc.T("Msg_HotkeyTitle"),
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        SetHotkey(mode, HotkeyHelper.Format(modifiers, key));
    }

    void ClearHotkey_Click(object sender, RoutedEventArgs e)
    {
        if (selected is Mode mode)
            SetHotkey(mode, "");
    }

    void SetHotkey(Mode mode, string text)
    {
        var old = mode.Hotkey;
        mode.Hotkey = text;

        var failed = services.Hotkeys.RegisterAll(Data.Modes);
        if (text != "" && failed.Contains(mode))
        {
            mode.Hotkey = old;
            services.Hotkeys.RegisterAll(Data.Modes);
            MessageBox.Show(Loc.T("Msg_HotkeyInUse"), Loc.T("Msg_HotkeyInUseTitle"),
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        services.Save();
    }

    // ---------- Mod işlemleri ----------

    string UniqueName(string baseName)
    {
        var name = baseName;
        int i = 2;
        while (Data.Modes.Any(m => m.Name == name))
            name = $"{baseName} {i++}";
        return name;
    }

    void NewMode_Click(object sender, RoutedEventArgs e)
    {
        var mode = new Mode { Name = UniqueName(Loc.T("Modes_NewName")) };
        Data.Modes.Add(mode);
        services.Save();
        SelectMode(mode);
    }

    void DeleteMode_Click(object sender, RoutedEventArgs e)
    {
        if (selected is not Mode mode) return;

        var answer = MessageBox.Show(Loc.T("Msg_DeleteMode", mode.Name), Loc.T("Msg_DeleteModeTitle"),
            MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (answer != MessageBoxResult.Yes) return;

        Data.Modes.Remove(mode);
        services.Hotkeys.RegisterAll(Data.Modes);
        services.Save();
        ShowPanel(null);
    }

    void Color_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (selected is Mode)
            services.Save();
    }

    async void RunMode_Click(object sender, RoutedEventArgs e)
    {
        if (selected is Mode mode)
            await services.RunModeFromUiAsync(mode);
    }

    // ---------- Sıralama ----------

    void MoveSelected(int delta)
    {
        if (selected is Mode mode)
        {
            int from = Data.Modes.IndexOf(mode);
            int to = from + delta;
            if (to < 0 || to >= Data.Modes.Count) return;

            Data.Modes.Move(from, to);
            services.Save();
            SelectMode(mode);
        }
        else if (selected is ModeAction action)
        {
            var parent = Data.Modes.FirstOrDefault(m => m.Actions.Contains(action));
            if (parent is null) return;

            int from = parent.Actions.IndexOf(action);
            int to = from + delta;
            if (to < 0 || to >= parent.Actions.Count) return;

            parent.Actions.Move(from, to);
            services.Save();
            SelectAction(parent, action);
        }
    }

    void MoveUp_Click(object sender, RoutedEventArgs e) => MoveSelected(-1);
    void MoveDown_Click(object sender, RoutedEventArgs e) => MoveSelected(1);

    // ---------- Eylem ekleme ----------

    void AddApp_Click(object sender, RoutedEventArgs e)
    {
        if (selected is not Mode mode) return;

        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = Loc.T("Dlg_PickApp"),
            Filter = AppFilter
        };
        if (dialog.ShowDialog() != true) return;

        var action = new ModeAction { Type = ActionType.App, Target = dialog.FileName };
        mode.Actions.Add(action);
        services.Save();
        SelectAction(mode, action);
    }

    void AddUrl_Click(object sender, RoutedEventArgs e)
    {
        if (selected is not Mode mode) return;

        var action = new ModeAction { Type = ActionType.Url, Target = "https://" };
        mode.Actions.Add(action);
        services.Save();
        SelectAction(mode, action);
    }

    void AddCloseApp_Click(object sender, RoutedEventArgs e)
    {
        if (selected is not Mode mode) return;

        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = Loc.T("Dlg_PickCloseApp"),
            Filter = ExeFilter
        };
        if (dialog.ShowDialog() != true) return;

        var action = new ModeAction { Type = ActionType.CloseApp, Target = dialog.FileName };
        mode.Actions.Add(action);
        services.Save();
        SelectAction(mode, action);
    }

    // ---------- Eylem düzenleme ----------

    void BrowseTarget_Click(object sender, RoutedEventArgs e)
    {
        if (selected is not ModeAction action) return;

        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = Loc.T("Dlg_PickApp"),
            Filter = AppFilter
        };
        if (dialog.ShowDialog() != true) return;

        action.Target = dialog.FileName;
        services.Save();
    }

    void BrowseBrowser_Click(object sender, RoutedEventArgs e)
    {
        if (selected is not ModeAction action) return;

        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = Loc.T("Dlg_PickBrowser"),
            Filter = $"{Loc.T("Dlg_Apps")} (*.exe)|*.exe"
        };
        if (dialog.ShowDialog() != true) return;

        action.BrowserPath = dialog.FileName;
        services.Save();
    }

    void DeleteAction_Click(object sender, RoutedEventArgs e)
    {
        if (selected is not ModeAction action) return;

        var parent = Data.Modes.FirstOrDefault(m => m.Actions.Contains(action));
        if (parent is null) return;

        parent.Actions.Remove(action);
        services.Save();
        SelectMode(parent);
    }

    async void TestAction_Click(object sender, RoutedEventArgs e)
    {
        if (selected is not ModeAction action) return;

        var button = sender as Button;
        if (button != null) button.IsEnabled = false;

        var error = await ModeRunner.TryExecuteAsync(action, Data.Settings);

        if (button != null) button.IsEnabled = true;

        if (error != null)
        {
            MessageBox.Show(error, Loc.T("Msg_ActionFailedTitle"),
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    void Field_LostFocus(object sender, RoutedEventArgs e) => services.Save();
}
