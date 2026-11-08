using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using KaedePhi.Tool.App.Gui.Models;

namespace KaedePhi.Tool.App.Gui.Views;

public partial class MessageDialog : Window
{
    public MessageDialog()
    {
        InitializeComponent();
    }

    public static MessageDialog ShowInfo(Window owner, string title, string message) =>
        Show(owner, title, message, MessageDialogType.Info);

    public static MessageDialog ShowSuccess(Window owner, string title, string message) =>
        Show(owner, title, message, MessageDialogType.Success);

    public static MessageDialog ShowWarning(Window owner, string title, string message) =>
        Show(owner, title, message, MessageDialogType.Warning);

    public static MessageDialog ShowError(Window owner, string title, string message) =>
        Show(owner, title, message, MessageDialogType.Error);

    private static MessageDialog Show(
        Window owner,
        string title,
        string message,
        MessageDialogType type
    )
    {
        var (glyph, color) = type switch
        {
            MessageDialogType.Success => ("\uf058", "#4CAF50"),
            MessageDialogType.Warning => ("\uf071", "#FF9800"),
            MessageDialogType.Error => ("\uf057", "#F44336"),
            _ => ("\uf05a", "#2196F3"),
        };
        var dialog = new MessageDialog
        {
            TitleText = { Text = title },
            MessageText = { Text = message },
            IconText = { Text = glyph, Foreground = new SolidColorBrush(Color.Parse(color)) },
        };

        dialog.ShowDialog(owner);
        return dialog;
    }

    private void OnOkClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
