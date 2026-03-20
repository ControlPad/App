using System.Windows;
using Wpf.Ui.Controls;

namespace ControlPad
{
    public partial class SelectOutputDevicePopup : FluentWindow
    {
        public string? SelectedOutputDeviceName { get; set; }

        public SelectOutputDevicePopup()
        {
            InitializeComponent();
            cb_OutputDevices.ItemsSource = new AudioController().GetOutputDeviceNames();
        }

        private void btn_Ok_Click(object sender, RoutedEventArgs e)
        {
            if (cb_OutputDevices.SelectedItem is not string outputName) return;

            SelectedOutputDeviceName = outputName;
            DialogResult = true;
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e) => Close();
    }
}
