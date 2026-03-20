using Avalonia.Controls;
using Avalonia.Interactivity;
using ControlPad.Avalonia.Views;

namespace ControlPad.Avalonia;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void HomeButton_Click(object? sender, RoutedEventArgs e)
    {
        MainContentHost.Content = new HomeView();
    }

    private void SliderButton_Click(object? sender, RoutedEventArgs e)
    {
        MainContentHost.Content = new SliderCategoriesView();
    }

    private void ButtonButton_Click(object? sender, RoutedEventArgs e)
    {
        MainContentHost.Content = new ButtonCategoriesView();
    }

    private void SettingsButton_Click(object? sender, RoutedEventArgs e)
    {
        MainContentHost.Content = new SettingsView();
    }

    private void ExitButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
