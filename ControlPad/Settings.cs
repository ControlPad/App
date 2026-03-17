using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ControlPad
{
    public static class Settings
    {
        private static bool _trayIconMessageShown = false;
        private static bool _startWithWindows = false;
        private static bool _startMinimized = false;
        private static bool _minimizeToSystemTray = true;
        private static double _translationExponent = 1d;
        private static int _selectedThemeIndex = 0;
        private static int _selectedBackgroundIndex = 3;
        private static int _sliderDeadZone = 4;
        private static bool _unmuteOnSliderChange = true;

        static Settings()
        {
            Load();
        }

        public static bool TrayIconMessageShown
        {
            get => _trayIconMessageShown;
            set
            {
                _trayIconMessageShown = value;
                Save();
            }
        }

        public static bool StartWithWindows
        {
            get => _startWithWindows;
            set
            {
                _startWithWindows = value;
                Save();
            }
        }

        public static bool StartMinimized
        {
            get => _startMinimized;
            set
            {
                _startMinimized = value;
                Save();
            }
        }

        public static bool MinimizeToSystemTray
        {
            get => _minimizeToSystemTray;
            set
            {
                _minimizeToSystemTray = value;
                Save();
            }
        }

        public static int SelectedThemeIndex
        {
            get => _selectedThemeIndex;
            set
            {
                _selectedThemeIndex = value;
                Save();
            }
        }

        public static int SelectedBackgroundIndex
        {
            get => _selectedBackgroundIndex;
            set
            {
                _selectedBackgroundIndex = value;
                Save();
            }
        }

        public static int SliderDeadZone
        {
            get => _sliderDeadZone;
            set
            {
                _sliderDeadZone = value;
                Save();
            }
        }

        public static bool UnmuteOnSliderChange
        {
            get => _unmuteOnSliderChange;
            set
            {
                _unmuteOnSliderChange = value;
                Save();
            }
        }

        public static double TranslationExponent
        {
            get => _translationExponent;
            set
            {
                _translationExponent = value;
                Save();
            }
        }

        private class Data
        {
            public bool TrayIconMessageShown { get; set; } = false;
            public bool StartWithWindows { get; set; } = false;
            public bool StartMinimized { get; set; } = true;
            public bool MinimizeToSystemTray { get; set; } = true;
            public double TranslationExponent { get; set; } = 1d;
            public int SelectedThemeIndex { get; set; } = 0;
            public int SelectedBackgroundIndex { get; set; } = 3;
            public int SliderDeadZone { get; set; } = 4;
            public bool UnmuteOnSliderChange { get; set; } = true;
        }

        public static void Load()
        {
            try
            {
                Data data = new Data();
                if (File.Exists(DataHandler.GetSettingsPath()))
                {
                    string json = File.ReadAllText(DataHandler.GetSettingsPath());
                    var dataTemp = JsonSerializer.Deserialize<Data>(json);
                    if (dataTemp != null) data = dataTemp;
                }

                _trayIconMessageShown = data.TrayIconMessageShown;
                _startWithWindows = data.StartWithWindows;
                _startMinimized = data.StartMinimized;
                _minimizeToSystemTray = data.MinimizeToSystemTray;               
                _selectedThemeIndex = data.SelectedThemeIndex;
                _selectedBackgroundIndex = data.SelectedBackgroundIndex;
                _sliderDeadZone = data.SliderDeadZone;
                _translationExponent = data.TranslationExponent;
                _unmuteOnSliderChange = data.UnmuteOnSliderChange;
            }
            catch
            {
                
            }
        }

        private static void Save()
        {
            try
            {
                var data = new Data
                {
                    TrayIconMessageShown = _trayIconMessageShown,
                    StartWithWindows = _startWithWindows,
                    StartMinimized = _startMinimized,
                    MinimizeToSystemTray = _minimizeToSystemTray,                    
                    SelectedThemeIndex = _selectedThemeIndex,
                    SelectedBackgroundIndex = _selectedBackgroundIndex,
                    SliderDeadZone = _sliderDeadZone,
                    TranslationExponent = _translationExponent,
                    UnmuteOnSliderChange = _unmuteOnSliderChange,
                };

                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(data, options);
                File.WriteAllText(DataHandler.GetSettingsPath(), json);
            }
            catch
            {
                
            }
        }
    }
}