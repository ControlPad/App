using ControlPad;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Win32;
using NAudio.SoundFont;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Wpf.Ui.Controls;
using Wpf.Ui.Tray;

namespace ControlPad
{
    public partial class MainWindow : FluentWindow
    {
        public HomeUserControl _homeUserControl;
        private ManageSliderCategoriesUserControl _manageSliderCategoriesUserControl;
        private ManageButtonCategoriesUserControl _manageButtonCategoriesUserControl;
        private SettingsUserControl _settingsUserControl;
        public ProgressRing progressRing = new() { IsIndeterminate = true };
        private bool realShutDown = false;
        Mutex _mutex;

        public MainWindow(Mutex mutex)
        {
            InitializeComponent();
            _mutex = mutex;
            Directory.SetCurrentDirectory(AppContext.BaseDirectory);
            DataHandler.CheckAppDataFolder();
            _homeUserControl = new HomeUserControl(this);
            _settingsUserControl = new SettingsUserControl(this);
            ArduinoController.Initialize(this, new EventHandler(_homeUserControl));
            DataContext = this;
            MainContentFrame.Navigate(progressRing);
            SetActive(NVI_Home);
            DataHandler.LoadPreset(GetCurrentPreset(), _settingsUserControl, _homeUserControl, true);
            _manageSliderCategoriesUserControl = new ManageSliderCategoriesUserControl(this);
            _manageButtonCategoriesUserControl = new ManageButtonCategoriesUserControl(this);
        }

        private Preset GetCurrentPreset()
        {
            foreach (Preset preset in DataHandler.GetPresets())
                if (preset.Name.EndsWith(" (current)"))
                    return preset;

            return DataHandler.CurrentPreset;
        }     

        private void mainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Settings.MinimizeToSystemTray && !realShutDown)
            {
                e.Cancel = true;
                Hide();

                if (!Settings.TrayIconMessageShown)
                {
                    new ToastContentBuilder().AddText("Slidr minimized to System Tray").Show();
                    Settings.TrayIconMessageShown = true;
                }
            }
        }
        private void mainWindow_Closed(object sender, EventArgs e) { }

        public void MI_Open_Click(object sender, EventArgs e)        
        {
            WindowState = WindowState.Normal;
            this.Show();
        }

        public void MI_Exit_Click(object sender, EventArgs e) => ShutDown();

        private void Exit_Click(object sender, RoutedEventArgs e) => ShutDown();

        private void ShutDown()
        {
            realShutDown = true;
            _mutex?.ReleaseMutex();
            _mutex?.Dispose();
            System.Windows.Application.Current.Shutdown();
        }

        private void NVI_Home_Click(object sender, RoutedEventArgs e)
        {
            if(!NVI_Home.IsActive)
            {
                if (ArduinoController.IsConnected)
                {
                    MainContentFrame.Navigate(_homeUserControl);
                }
                else
                {
                    MainContentFrame.Navigate(progressRing);
                }
                SetActive(NVI_Home);
            }            
        }       

        private void NVI_Slider_Categories_Click(object sender, RoutedEventArgs e)
        {
            if (!NVI_Slider_Categories.IsActive)
            {
                MainContentFrame.Navigate(_manageSliderCategoriesUserControl);
                SetActive(NVI_Slider_Categories);
            }
        }
        private void NVI_Button_Categories_Click(object sender, RoutedEventArgs e)
        {
            if (!NVI_Button_Categories.IsActive)
            {
                MainContentFrame.Navigate(_manageButtonCategoriesUserControl);
                SetActive(NVI_Button_Categories);
            }
        }
        private void NVI_Settings_Click(object sender, RoutedEventArgs e)
        {                       
            if (!NVI_Settings.IsActive)
            {
                MainContentFrame.Navigate(_settingsUserControl);
                SetActive(NVI_Settings);
            }
        }

        private void SetActive(NavigationViewItem item)
        {
            NVI_Home.IsActive = false;
            NVI_Slider_Categories.IsActive = false;
            NVI_Button_Categories.IsActive = false;
            NVI_Settings.IsActive = false;

            NVI_EditMode.Visibility = item == NVI_Home ? Visibility.Visible : Visibility.Collapsed;

            if (NVI_EditMode.Icon is SymbolIcon symbolIconEditMode) EditModeUnchecked(symbolIconEditMode);
            if (NVI_Home.Icon is SymbolIcon symbolIconHome) symbolIconHome.Filled = false;
            if (NVI_Slider_Categories.Icon is SymbolIcon symbolIconCategories) symbolIconCategories.Filled = false;
            if (NVI_Button_Categories.Icon is SymbolIcon symbolIconButtonCategories) symbolIconButtonCategories.Filled = false;
            if (NVI_Settings.Icon is SymbolIcon symbolIconSettings) symbolIconSettings.Filled = false;
            

            if (item.Icon is SymbolIcon symbolIcon) symbolIcon.Filled = true;
            item.IsActive = true;
        }

        private void NVI_EditMode_Click(object sender, RoutedEventArgs e)
        {
            if (NVI_Home.IsActive && NVI_EditMode.Icon is SymbolIcon symbolIconEditMode)
            {
                if (symbolIconEditMode.Symbol == SymbolRegular.CheckboxChecked24)
                {
                    EditModeUnchecked(symbolIconEditMode);
                }
                else
                {
                    EditModeChecked(symbolIconEditMode);                    
                }
            }
        }
        private void EditModeChecked(SymbolIcon symbolIcon)
        {
            symbolIcon.Symbol = SymbolRegular.CheckboxChecked24;
            _homeUserControl.SliderCell1.Visibility = Visibility.Visible;
            _homeUserControl.SliderCell2.Visibility = Visibility.Visible;
            _homeUserControl.SliderCell3.Visibility = Visibility.Visible;
            _homeUserControl.SliderCell4.Visibility = Visibility.Visible;
            _homeUserControl.SliderCell5.Visibility = Visibility.Visible;
            _homeUserControl.SliderCell6.Visibility = Visibility.Visible;
            _homeUserControl.ButtonCell1.Visibility = Visibility.Visible;
            _homeUserControl.ButtonCell2.Visibility = Visibility.Visible;
            _homeUserControl.ButtonCell3.Visibility = Visibility.Visible;
            _homeUserControl.ButtonCell4.Visibility = Visibility.Visible;
            _homeUserControl.ButtonCell5.Visibility = Visibility.Visible;
            _homeUserControl.ButtonCell6.Visibility = Visibility.Visible;
            _homeUserControl.ButtonCell7.Visibility = Visibility.Visible;
            _homeUserControl.ButtonCell8.Visibility = Visibility.Visible;
            _homeUserControl.ButtonCell9.Visibility = Visibility.Visible;
            _homeUserControl.ButtonCell10.Visibility = Visibility.Visible;
            _homeUserControl.ButtonCell11.Visibility = Visibility.Visible;
        }

        private void EditModeUnchecked(SymbolIcon symbolIcon)
        {
            symbolIcon.Symbol = SymbolRegular.CheckboxUnchecked24;
            _homeUserControl.SliderCell1.Visibility = Visibility.Hidden;
            _homeUserControl.SliderCell2.Visibility = Visibility.Hidden;
            _homeUserControl.SliderCell3.Visibility = Visibility.Hidden;
            _homeUserControl.SliderCell4.Visibility = Visibility.Hidden;
            _homeUserControl.SliderCell5.Visibility = Visibility.Hidden;
            _homeUserControl.SliderCell6.Visibility = Visibility.Hidden;
            _homeUserControl.ButtonCell1.Visibility = Visibility.Hidden;
            _homeUserControl.ButtonCell2.Visibility = Visibility.Hidden;
            _homeUserControl.ButtonCell3.Visibility = Visibility.Hidden;
            _homeUserControl.ButtonCell4.Visibility = Visibility.Hidden;
            _homeUserControl.ButtonCell5.Visibility = Visibility.Hidden;
            _homeUserControl.ButtonCell6.Visibility = Visibility.Hidden;
            _homeUserControl.ButtonCell7.Visibility = Visibility.Hidden;
            _homeUserControl.ButtonCell8.Visibility = Visibility.Hidden;
            _homeUserControl.ButtonCell9.Visibility = Visibility.Hidden;
            _homeUserControl.ButtonCell10.Visibility = Visibility.Hidden;
            _homeUserControl.ButtonCell11.Visibility = Visibility.Hidden;
        }
    }   
}