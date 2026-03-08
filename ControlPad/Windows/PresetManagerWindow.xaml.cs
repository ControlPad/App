using NAudio.SoundFont;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Xml.Linq;
using Wpf.Ui.Controls;

namespace ControlPad
{
    public partial class PresetManagerWindow : FluentWindow
    {
        private SettingsUserControl _settingsUserControl;
        public PresetManagerWindow(SettingsUserControl settingsUserControl)
        {
            InitializeComponent();
            _settingsUserControl = settingsUserControl;

            var presets = DataHandler.GetPresets();
            lv_Presets.DisplayMemberPath = "Name";
            foreach (var preset in presets)
            {
                preset.Name = preset.Name.Replace(" (current)", "");
                if (preset.Id == DataHandler.CurrentPreset.Id)
                    preset.Name = preset.Name + " (current)";
                    
                lv_Presets.Items.Add(preset);
            }      
        }

        private void btn_NewPreset_Click(object sender, RoutedEventArgs e)
        {
            int i = lv_Presets.Items.Count + 1;
            bool success = false;

            do
            {
                success = AddPreset(new Preset(lv_Presets.Items.Cast<Preset>().GetFreeId(p => p.Id), $"Preset{i}"));
                if (i > 999)
                {
                    ShowError("Could not create a new Preset");
                    break;
                }
                i++;
            } while (!success);

            if (success)
                CollectionChanged();
        }

        private void btn_DeletePreset_Click(object sender, RoutedEventArgs e)
        {
            bool switchCurrentPreset = false;
            if (lv_Presets.SelectedItem != null)
            {  
                if (lv_Presets.SelectedItem is Preset selecetedPreset && selecetedPreset.Id == DataHandler.CurrentPreset.Id)
                    switchCurrentPreset = true;

                lv_Presets.Items.Remove(lv_Presets.SelectedItem);

                if (switchCurrentPreset)
                {
                    Preset preset = lv_Presets.Items.Cast<Preset>().First();
                    LoadPreset(preset);
                }

                CollectionChanged();
            }
        }

        private void Btn_Apply_Click(object sender, RoutedEventArgs e)
        {
            var duplicateName = lv_Presets.Items
                .Cast<Preset>()
                .GroupBy(p => p.Name)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .FirstOrDefault();

            if (duplicateName != null)
            {
                ShowError($"The preset {duplicateName} already exists.");
                return;
            }

            foreach (Preset preset in lv_Presets.Items)
                if (preset.Name.Replace(" (current)", "") == "")
                {
                    ShowError($"Empty name");
                    return;
                }

            var oldPresets = DataHandler.GetPresets();
            // delete
            foreach (var preset in oldPresets)
            {
                if (!lv_Presets.Items.Cast<Preset>().Select(p => p.Id).Contains(preset.Id))
                {
                    try
                    {
                        Directory.Delete(Path.Combine(DataHandler.AppDataRoaming, "Slidr", "Presets", preset.Name), true);
                    }
                    catch (Exception ex)
                    {
                        ShowError($"Could not delete directory {preset.Name} ex: {ex.Message}");
                    }
                }
            }

            // rename/create
            foreach (Preset preset in lv_Presets.Items)
            {
                string? currentPath = DataHandler.GetPresetPath(preset.Id);
                string newPath = Path.Combine(DataHandler.AppDataRoaming, "Slidr", "Presets", preset.Name);
                try
                {
                    if (currentPath != null && currentPath != newPath)
                        Directory.Move(currentPath, newPath);
                    else if (currentPath == null)
                        DataHandler.CreatePreset(preset);
                }
                catch (Exception ex)
                {
                    ShowError($"Could not rename directory {preset.Name} ex: {ex.Message}");
                }
            }

            DialogResult = true;
        }

        private bool AddPreset(Preset preset)
        {
            string name = preset.Name.Replace(" (current)", "");
            if (!lv_Presets.Items.Cast<Preset>().Select(p => p.Name).Contains(name) && !lv_Presets.Items.Cast<Preset>().Contains(preset))
            {
                lv_Presets.Items.Add(preset);
                return true;
            }
            return false;
        }

        

        private void btn_LoadPreset_Click(object sender, RoutedEventArgs e)
        {
            if (lv_Presets.SelectedItem is Preset preset)
            {
                LoadPreset(preset);
            } 
        }

        private void lv_Presets_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (lv_Presets.SelectedItem is Preset preset)
            {
                tb_PresetName.Text = preset.Name.Replace(" (current)", "");

                if (DataHandler.GetPresetPath(preset.Id) == null || preset.Name.EndsWith(" (current)"))
                    btn_LoadPreset.IsEnabled = false;
                else
                    btn_LoadPreset.IsEnabled = true;

                if (lv_Presets.Items.Count <= 1)
                    btn_DeletePreset.IsEnabled = false;
                else
                    btn_DeletePreset.IsEnabled = true;
            }
            else
            {
                tb_PresetName.Text = string.Empty;
            }
        }

        private void tb_PresetName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (lv_Presets.SelectedItem is Preset preset && preset.Name.Replace(" (current)", "") != tb_PresetName.Text)
            { 
                preset.Name = preset.Name.Replace(" (current)", "");
                if (preset.Id == DataHandler.CurrentPreset.Id)
                    preset.Name = tb_PresetName.Text + " (current)";
                else
                    preset.Name = tb_PresetName.Text;
            }
        }

        private void LoadPreset(Preset preset)
        {
            btn_LoadPreset.IsEnabled = false;

            DataHandler.LoadPreset(preset, _settingsUserControl);

            foreach (Preset p in lv_Presets.Items)
            {
                p.Name = p.Name.Replace(" (current)", "");
                if (p.Id == DataHandler.CurrentPreset.Id)
                    p.Name = p.Name + " (current)";
            }
        }

        private void CollectionChanged()
        {
            if (lv_Presets.Items.Count <= 1 && lv_Presets.SelectedIndex == 0)
                btn_DeletePreset.IsEnabled = false;
            else
                btn_DeletePreset.IsEnabled = true;
        }

        private void ShowError(string msg)
        {
            System.Windows.MessageBox.Show(msg, "Slidr", System.Windows.MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
