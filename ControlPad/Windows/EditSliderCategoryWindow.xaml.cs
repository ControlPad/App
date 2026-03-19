using System;
using System.Windows;
using Wpf.Ui.Controls;

namespace ControlPad
{
    public partial class EditSliderCategoryWindow : FluentWindow
    {
        private int indexOfCategory;

        public EditSliderCategoryWindow(int indexOfCategory)
        {
            InitializeComponent();
            this.indexOfCategory = indexOfCategory;

            tb_CategoryName.Text = DataHandler.SliderCategories[indexOfCategory].Name;
            lb_AudioStreams.ItemsSource = DataHandler.SliderCategories[indexOfCategory].AudioStreams;
            lb_AudioStreams.DisplayMemberPath = "DisplayName";
            btn_DetectGames.Visibility = string.Equals(DataHandler.SliderCategories[indexOfCategory].Name, "Games", StringComparison.OrdinalIgnoreCase)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void btn_AddProcess_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SelectProcessPopup() { Owner = this };

            if (dialog.ShowDialog() == true)
            {
                DataHandler.SliderCategories[indexOfCategory].AudioStreams.Add(new AudioStream(dialog.SelectedProcessName, null));
            }               
        }

        private void btn_Remove_Click(object sender, RoutedEventArgs e)
        {
            int index = lb_AudioStreams.SelectedIndex;
            if (index == -1) return;
            DataHandler.SliderCategories[indexOfCategory].AudioStreams.RemoveAt(index);
        }

        private void btn_Close_Click(object sender, RoutedEventArgs e)
        {
            if(!string.IsNullOrEmpty(tb_CategoryName.Text))
            {
                DataHandler.SliderCategories[indexOfCategory].Name = tb_CategoryName.Text.Trim();
                this.Close();
            }
        }

        private void btn_AddMic_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SelectMicPopup() { Owner = this };

            if (dialog.ShowDialog() == true)
            {
                DataHandler.SliderCategories[indexOfCategory].AudioStreams.Add(new AudioStream(null, dialog.SelectedMic?.DeviceFriendlyName));
            }
        }

        private void btn_AddMain_Click(object sender, RoutedEventArgs e)
        {
            bool containsMain = false;
            foreach (var item in lb_AudioStreams.Items)
                if (((AudioStream)item).DisplayName == "Main Audio")
                    containsMain = true;

            if (!containsMain)
                DataHandler.SliderCategories[indexOfCategory].AudioStreams.Add(new AudioStream(null, null));
        }

        private void btn_DetectGames_Click(object sender, RoutedEventArgs e)
        {
            int count = DataHandler.RefreshAutoDetectedGames();
            lb_AudioStreams.ItemsSource = DataHandler.SliderCategories[indexOfCategory].AudioStreams;
            lb_AudioStreams.Items.Refresh();
            System.Windows.MessageBox.Show($"Detected {count} games.", "Slidr", System.Windows.MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
