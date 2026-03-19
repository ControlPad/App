using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ControlPad.Avalonia;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void ExitButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
