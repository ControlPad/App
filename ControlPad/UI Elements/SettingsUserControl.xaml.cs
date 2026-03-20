using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Controls;

namespace ControlPad
{
    public partial class SettingsUserControl : UserControl
    {
        private readonly bool _isInitialized = false;
        private readonly MainWindow _mainWindow;

        public SettingsUserControl(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _isInitialized = true;
            SetControls();
        }

        private void cb_StartWithWindows_Checked(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized)
                return;

            bool startWithWindows = cb_StartWithWindows.IsChecked == true;
            bool startMinimized = cb_StartMinimized.IsChecked == true;

            AutoStart.Set(startWithWindows, startMinimized);
            Settings.StartWithWindows = startWithWindows;

            GridStartMinimized.IsEnabled = startWithWindows;
        }

        private void cb_StartMinimized_Checked(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized)
                return;

            bool startMinimized = cb_StartMinimized.IsChecked == true;

            AutoStart.Set(true, startMinimized);
            Settings.StartMinimized = startMinimized;
        }      

        private void cb_MinimizeToTray_Checked(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized)
                return;

            Settings.MinimizeToSystemTray = cb_MinimizeToTray.IsChecked == true;
        }

        private void cb_UnmuteOnSliderChange_Checked(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized)
                return;

            Settings.UnmuteOnSliderChange = cb_UnmuteOnSliderChange.IsChecked == true;
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitialized)
                return;

            ChangeAppTheme(ThemeComboBox.SelectedIndex);
            Settings.SelectedThemeIndex = ThemeComboBox.SelectedIndex;
        }

        private void BackgroundComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitialized)
                return;

            ChangeAppBackground(BackgroundComboBox.SelectedIndex);
            Settings.SelectedBackgroundIndex = BackgroundComboBox.SelectedIndex;
        }

        private void nb_TranslationExponent_ValueChanged(object sender, Wpf.Ui.Controls.NumberBoxValueChangedEventArgs e)
        {
            if (!_isInitialized)
                return;

            if (nb_TranslationExponent.Value != null)
                Settings.TranslationExponent = (double)nb_TranslationExponent.Value;
            else
                Settings.TranslationExponent = 1.0d;
        }

        public static void ChangeAppTheme(int index)
        {
            switch (index)
            {
                case 0:
                    Wpf.Ui.Appearance.ApplicationThemeManager.ApplySystemTheme();
                    break;
                case 1:
                    Wpf.Ui.Appearance.ApplicationThemeManager.Apply(Wpf.Ui.Appearance.ApplicationTheme.Light);
                    break;
                case 2:
                    Wpf.Ui.Appearance.ApplicationThemeManager.Apply(Wpf.Ui.Appearance.ApplicationTheme.Dark);
                    break;
            }
        }

        public void ChangeAppBackground(int index)
        {
            switch (index)
            {
                case 0:
                    _mainWindow.WindowBackdropType = WindowBackdropType.None;
                    break;
                case 1:
                    _mainWindow.WindowBackdropType = WindowBackdropType.Acrylic;
                    break;
                case 2:
                    _mainWindow.WindowBackdropType = WindowBackdropType.Mica;
                    break;
                case 3:
                    _mainWindow.WindowBackdropType = WindowBackdropType.Tabbed;
                    break;
                case 4:
                    _mainWindow.WindowBackdropType = WindowBackdropType.Auto;
                    break;
            }
        }

        public void SetControls()
        {
            cb_StartWithWindows.IsChecked = Settings.StartWithWindows;
            cb_StartMinimized.IsChecked = Settings.StartMinimized;
            cb_MinimizeToTray.IsChecked = Settings.MinimizeToSystemTray;
            cb_UnmuteOnSliderChange.IsChecked = Settings.UnmuteOnSliderChange;
            ThemeComboBox.SelectedIndex = Settings.SelectedThemeIndex;
            BackgroundComboBox.SelectedIndex = Settings.SelectedBackgroundIndex;
            nb_TranslationExponent.Value = Settings.TranslationExponent;

            var infoVersion = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            lbl_AppVersion.Content = !string.IsNullOrEmpty(infoVersion) ? infoVersion : "Unknown";
        }

        private void Btn_Presets_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new PresetManagerWindow(this);
            dialog.ShowDialog();
        }
    }
}
