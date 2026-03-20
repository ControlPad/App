using System.Windows;
using Wpf.Ui.Controls;

namespace ControlPad
{
    public partial class SelectMicPopup : FluentWindow
    {
        AudioController audioController = new AudioController();
        public string? SelectedMicName { get; set; }
        public SelectMicPopup()
        {
            InitializeComponent();
            cb_Mics.ItemsSource = audioController.GetMicNames();
        }

        private void btn_Ok_Click(object sender, RoutedEventArgs e)
        {
            if (cb_Mics.SelectedItem is not string micName) return;

            SelectedMicName = micName;
            DialogResult = true;
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e) => Close();
    }
}
