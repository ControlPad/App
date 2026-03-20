using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Xml.Linq;
using Windows.Storage.BulkAccess;
using ControlPad.Utils;

namespace ControlPad
{
    public static class DataHandler
    {
        public static string AppDataRoaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public static Preset CurrentPreset { get; set; } = new Preset(0, "Preset1 (current)");
        public static string GetSliderCategoriesPath() => Path.Combine(AppDataRoaming, GetPresetPath(CurrentPreset.Id) ?? @$"Slidr\Presets\{CurrentPreset.Name}", "SliderCategories.json");
        public static string GetButtonCategoriesPath() => Path.Combine(AppDataRoaming, GetPresetPath(CurrentPreset.Id) ?? @$"Slidr\Presets\{CurrentPreset.Name}", "ButtonCategories.json");
        public static string GetCategoryControlsPath() => Path.Combine(AppDataRoaming, GetPresetPath(CurrentPreset.Id) ?? @$"Slidr\Presets\{CurrentPreset.Name}", "CategoryControls.txt");
        public static string GetSettingsPath() => Path.Combine(AppDataRoaming, GetPresetPath(CurrentPreset.Id) ?? $@"Slidr\Presets\{CurrentPreset.Name}", "Settings.json");
        public static ObservableCollection<SliderCategory> SliderCategories { get; set; } = new ObservableCollection<SliderCategory>();
        public static ObservableCollection<ButtonCategory> ButtonCategories { get; set; } = new ObservableCollection<ButtonCategory>();
        public static List<(CustomSlider slider, int value)> SliderValues { get; set; } = new();
        public static List<(CustomButton button, int value)> ButtonValues { get; set; } = new();
        

        public static void SaveDataToFile<T>(string path, List<T> data)
        {
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true, NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals });
            File.WriteAllText(path, json);
        }

        public static List<T> LoadDataFromFile<T>(string path)
        {
            var list = new List<T>();
            if (!File.Exists(path))
            {
                Debug.WriteLine($"File does not exist: {path}");
            }
            else
            {
                string json = File.ReadAllText(path);
                list = JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
            }

            return list;
        }

        public static void SaveCategoryControls(string path)
        {
            var sliderLines = SliderValues.Select((item, i) => $"Slider{i + 1}.Category.Id is: {item.slider.Category?.Id}");
            var buttonLines = ButtonValues.Select((item, i) => $"Switch{i + 1}.Category.Id is: {item.button.Category?.Id}");
            File.WriteAllLines(path, sliderLines.Concat(buttonLines));
        }

        public static void LoadCategoryControls(string path)
        {
            if (!File.Exists(path))
            {
                Debug.WriteLine($"File does not exist: {path}");

                for (int i = 0; i < SliderValues.Count; i++)
                    SliderValues[i].slider.Category = null;
                for (int i = 0; i < ButtonValues.Count; i++)
                    ButtonValues[i].button.Category = null;
            }
            else
            {
                string[] lines = File.ReadAllLines(path);

                for (int i = 0; i < SliderValues.Count; i++)
                {
                    if (int.TryParse(lines[i].Split(':')[1].Trim(), out int sliderCategoryId))
                    {
                        var category = SliderCategories.FirstOrDefault(c => c.Id == sliderCategoryId);
                        SliderValues[i].slider.Category = category;
                    }
                }
                for (int i = 0; i < ButtonValues.Count; i++)
                {
                    if (int.TryParse(lines[i + SliderValues.Count].Split(':')[1].Trim(), out int buttonCategoryId))
                    {
                        var category = ButtonValues[i].button.Category = ButtonCategories.FirstOrDefault(c => c.Id == buttonCategoryId);
                        ButtonValues[i].button.Category = category;
                    }
                }
            }
        }

        public static int GetFreeId<T>(this IEnumerable<T> items, Func<T, int> idSelector) // gets the lowest, not yet existing id
        {
            return IdAllocator.GetFreeId(items, idSelector);
        }

        public static void SetSliderTextBlocks()
        {
            foreach ((CustomSlider slider, int) categorySlider in DataHandler.SliderValues)
                if (categorySlider.slider.Category != null)
                    categorySlider.slider.TextBlock.Text = categorySlider.slider.Category.Name;
                else
                    categorySlider.slider.TextBlock.Text = "";
        }

        public static void SetButtonTextBlocks()
        {
            foreach ((CustomButton button, int) categoryButton in DataHandler.ButtonValues)
                if (categoryButton.button.Category != null)
                    categoryButton.button.TextBlock.Text = categoryButton.button.Category?.Name ?? "";
                else
                    categoryButton.button.TextBlock.Text = "";
        }

        public static readonly List<ActionType> ActionTypes = new()
        {
            new ActionType(EActionType.MuteProcess,   "Mute Process"),
            new ActionType(EActionType.MuteMainAudio, "Mute Main Audio Stream"),
            new ActionType(EActionType.MuteMic,       "Mute Microphone"),
            new ActionType(EActionType.OpenProcess,   "Open Process"),
            new ActionType(EActionType.OpenWebsite,   "Open Website"),
            new ActionType(EActionType.KeyPress,      "Key Press"),
        };

        public static List<Preset> GetPresets()
        {
            var presets = new List<Preset>();
            var presetFolders = Directory.GetDirectories(Path.Combine(DataHandler.AppDataRoaming, "Slidr", "Presets"));
            foreach (var folder in presetFolders)
            {
                if (File.Exists(@$"{folder}\ID.txt") && int.TryParse(File.ReadAllText(@$"{folder}\ID.txt"), out int id))
                {
                    presets.Add(new Preset(id, Path.GetFileName(folder)));
                }
                else
                {
                    MessageBox.Show($"Could not get Id from {folder}");
                }
            }
            return presets;
        }

        public static string? GetPresetPath(int idToSearchFor)
        {
            var presetFolders = Directory.GetDirectories(Path.Combine(DataHandler.AppDataRoaming, "Slidr", "Presets"));
            foreach (var folder in presetFolders)
            {
                if (int.TryParse(File.ReadAllText(@$"{folder}\ID.txt"), out int id))
                    if (idToSearchFor == id) return folder;
            }
            return null;
        }

        public static void CheckAppDataFolder()
        {
            if (!Directory.Exists(Path.Combine(AppDataRoaming, @"Slidr\Presets")))
            {
                Directory.CreateDirectory(Path.Combine(AppDataRoaming, @"Slidr\Presets\"));
                CreatePreset(new Preset(GetPresets().GetFreeId(p => p.Id), "Preset1 (current)"));
            }
        }

        public static bool CreatePreset(Preset preset)
        {
            string path = Path.Combine(AppDataRoaming, @$"Slidr\Presets\{preset.Name}\");
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    File.WriteAllText(Path.Combine(path, "ID.txt"), preset.Id.ToString());
                    return true;
                }
                else if (preset.Name != "Preset1 (current)")
                {
                    MessageBox.Show($"The preset '{preset.Name}' already exists");
                    return false;
                }
            }
            catch { }
            return false;
        }

        public static void LoadPreset(Preset preset, SettingsUserControl settingsUserControl,
                                      HomeUserControl? homeUserControl = null, bool FirstTime = false)
        {
            if (!FirstTime && CurrentPreset.Id == preset.Id)
                return;

            CurrentPreset = preset;

            DataHandler.SliderCategories.Clear();
            foreach (var sliderCategory in DataHandler.LoadDataFromFile<SliderCategory>(GetSliderCategoriesPath()))
                DataHandler.SliderCategories.Add(sliderCategory);

            DataHandler.ButtonCategories.Clear();
            foreach (var buttonCategory in DataHandler.LoadDataFromFile<ButtonCategory>(GetButtonCategoriesPath()))
                DataHandler.ButtonCategories.Add(buttonCategory);

            DataHandler.LoadCategoryControls(GetCategoryControlsPath());

            if (homeUserControl != null)
                homeUserControl.SetTextBlocks();

            DataHandler.SetSliderTextBlocks();
            DataHandler.SetButtonTextBlocks();

            Settings.Load();
            settingsUserControl.SetControls();
            SettingsUserControl.ChangeAppTheme(Settings.SelectedThemeIndex);
            settingsUserControl.ChangeAppBackground(Settings.SelectedBackgroundIndex);
        }
    }
}
