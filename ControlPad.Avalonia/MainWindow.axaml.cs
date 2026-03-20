using Avalonia.Controls;
using Avalonia.Interactivity;
using ControlPad.Avalonia.Views;

namespace ControlPad.Avalonia;

public partial class MainWindow : Window
{
    private readonly HomeView _homeView = new();
    private readonly SliderCategoriesView _sliderCategoriesView = new();
    private readonly ButtonCategoriesView _buttonCategoriesView = new();
    private readonly SettingsView _settingsView = new();

    public MainWindow()
    {
        InitializeComponent();
        MainContentHost.Content = _homeView;
    }

    private void HomeButton_Click(object? sender, RoutedEventArgs e)
    {
        MainContentHost.Content = _homeView;
    }

    private void SliderButton_Click(object? sender, RoutedEventArgs e)
    {
        MainContentHost.Content = _sliderCategoriesView;
    }

    private void ButtonButton_Click(object? sender, RoutedEventArgs e)
    {
        MainContentHost.Content = _buttonCategoriesView;
    }

    private void SettingsButton_Click(object? sender, RoutedEventArgs e)
    {
        MainContentHost.Content = _settingsView;
    }

    private void ExitButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
