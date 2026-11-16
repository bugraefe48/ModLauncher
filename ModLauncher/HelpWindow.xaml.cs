using Wpf.Ui.Controls;

namespace ModLauncher;

public partial class HelpWindow : FluentWindow
{
    public HelpWindow(string? topicKey = null)
    {
        InitializeComponent();

        Title = Loc.T("Help_Title");
        HelpTitleBar.Title = Loc.T("Help_Title");

        int select = 0;
        for (int i = 0; i < HelpContent.Topics.Count; i++)
        {
            var (title, body) = HelpContent.Topics[i];
            TopicList.Items.Add(new System.Windows.Controls.ListBoxItem
            {
                Content = Loc.T(title),
                Tag = body
            });

            if (title == topicKey)
                select = i;
        }

        TopicList.SelectedIndex = select;
    }

    void TopicList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (TopicList.SelectedItem is System.Windows.Controls.ListBoxItem item && item.Tag is string bodyKey)
        {
            TopicTitle.Text = item.Content?.ToString() ?? "";
            TopicBody.Text = Loc.T(bodyKey);
        }
    }
}
