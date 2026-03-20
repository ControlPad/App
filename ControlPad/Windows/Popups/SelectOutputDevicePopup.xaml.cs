using NAudio.CoreAudioApi;
using System.Windows;
using Wpf.Ui.Controls;

namespace ControlPad
{
    public partial class SelectOutputDevicePopup : FluentWindow
    {
        public MMDevice? SelectedOutputDevice { get; set; }

        public SelectOutputDevicePopup()
        {
            InitializeComponent();
            cb_OutputDevices.DisplayMemberPath = "DeviceFriendlyName";
            cb_OutputDevices.ItemsSource = new AudioController().GetOutputDevices();
        }

        private void btn_Ok_Click(object sender, RoutedEventArgs e)
        {
            if (cb_OutputDevices.SelectedItem is not MMDevice device) return;

            SelectedOutputDevice = device;
            DialogResult = true;
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e) => Close();
    }
}
