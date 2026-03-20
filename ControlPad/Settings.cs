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
        private static string _translationCurvePreset = "linear";
        private static double _translationCurveX1 = 0d;
        private static double _translationCurveY1 = 0d;
        private static double _translationCurveX2 = 1d;
        private static double _translationCurveY2 = 1d;
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

        public static string TranslationCurvePreset
        {
            get => _translationCurvePreset;
            set
            {
                _translationCurvePreset = SliderTranslationCurve.IsSupportedPreset(value) ? value : "linear";
                Save();
            }
        }

        public static double TranslationCurveX1
        {
            get => _translationCurveX1;
            set
            {
                _translationCurveX1 = Math.Clamp(value, 0d, 1d);
                Save();
            }
        }

        public static double TranslationCurveY1
        {
            get => _translationCurveY1;
            set
            {
                _translationCurveY1 = Math.Clamp(value, 0d, 1d);
                Save();
            }
        }

        public static double TranslationCurveX2
        {
            get => _translationCurveX2;
            set
            {
                _translationCurveX2 = Math.Clamp(value, 0d, 1d);
                Save();
            }
        }

        public static double TranslationCurveY2
        {
            get => _translationCurveY2;
            set
            {
                _translationCurveY2 = Math.Clamp(value, 0d, 1d);
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
            public string TranslationCurvePreset { get; set; } = "linear";
            public double TranslationCurveX1 { get; set; } = 0d;
            public double TranslationCurveY1 { get; set; } = 0d;
            public double TranslationCurveX2 { get; set; } = 1d;
            public double TranslationCurveY2 { get; set; } = 1d;
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
                _translationCurvePreset = SliderTranslationCurve.IsSupportedPreset(data.TranslationCurvePreset) ? data.TranslationCurvePreset : "linear";
                _translationCurveX1 = Math.Clamp(data.TranslationCurveX1, 0d, 1d);
                _translationCurveY1 = Math.Clamp(data.TranslationCurveY1, 0d, 1d);
                _translationCurveX2 = Math.Clamp(data.TranslationCurveX2, 0d, 1d);
                _translationCurveY2 = Math.Clamp(data.TranslationCurveY2, 0d, 1d);
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
                    TranslationCurvePreset = _translationCurvePreset,
                    TranslationCurveX1 = _translationCurveX1,
                    TranslationCurveY1 = _translationCurveY1,
                    TranslationCurveX2 = _translationCurveX2,
                    TranslationCurveY2 = _translationCurveY2,
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
