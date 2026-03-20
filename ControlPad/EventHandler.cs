using ControlPad.Converters;
using System.Diagnostics;

namespace ControlPad
{
    public class EventHandler
    {
        private AudioController AudioController; 
        private HomeUserControl HomeUserControl;
        private List<(CustomButton Button, int Value)>? oldButtonValues;
        private Dictionary<CustomSlider, int> oldSliderValues = new Dictionary<CustomSlider, int>();

        public EventHandler(HomeUserControl homeUserControl)
        {
            HomeUserControl = homeUserControl;
            AudioController = new AudioController();
        }
        public void Update(List<(CustomSlider Slider, int Value)> currentSliderValues,
                           List<(CustomButton Button, int Value)> currentButtonValues)
        {

            for (int i = 0; i < currentButtonValues.Count; i++)
            {
                if(oldButtonValues != null)
                {
                    ButtonEvent(currentButtonValues[i].Button, currentButtonValues[i].Value, oldButtonValues[i].Value);
                }                
            }
            oldButtonValues = currentButtonValues.Select(t => (t.Button, t.Value)).ToList();


            var updatedSliders = new List<(CustomSlider Slider, int Value)>();

            var snapshot = currentSliderValues.ToList();

            foreach (var (slider, currentValue) in snapshot)
            {
                int oldValue = oldSliderValues.TryGetValue(slider, out int val) ? val : 0;

                int diff = Math.Abs(oldValue - currentValue);

                if (diff > Settings.SliderDeadZone)
                {
                    SliderEvent(slider, currentValue);
                    updatedSliders.Add((slider, currentValue));
                }
            }

            foreach (var (slider, value) in updatedSliders)
            {
                oldSliderValues[slider] = value;
            }
        }

        private void SliderEvent(CustomSlider slider, int value)
        {
            HomeUserControl.Dispatcher.Invoke(() => HomeUserControl.UpdateUISlider(slider, value));

            if (slider.Category != null)
                foreach(AudioStream stream in slider.Category.AudioStreams)
                {
                    float volume = SliderToFloat(value);

                    if (Settings.UnmuteOnSliderChange)
                    {
                        if (stream.Process != null)
                            Task.Run(() => AudioController.MuteProcess(stream.Process, false));
                        else if (stream.MicName != null)
                            Task.Run(() => AudioController.MuteMic(stream.MicName, false));
                        else if (stream.Process == null && stream.MicName == null)
                            Task.Run(() => AudioController.MuteSystem(false));
                    }

                    if (stream.Process != null)
                        Task.Run(() => AudioController.SetProcessVolume(stream.Process, volume));
                    else if (stream.MicName != null)
                        Task.Run(() => AudioController.SetMicVolume(stream.MicName, volume));
                    else if (stream.Process == null && stream.MicName == null)
                        Task.Run(() => AudioController.SetSystemVolume(volume, stream.DeviceName));
                }                            
        }

        private void ButtonEvent(CustomButton button, int currentValue, int oldValue)
        {
            bool IsPressed = currentValue == 1;
            bool IsPressedOld = oldValue == 1;

            if (currentValue != oldValue)
            {
                HomeUserControl.Dispatcher.Invoke(() => HomeUserControl.UpdateUIButton(button, IsPressed));

                if (button.Category != null)
                {
                    foreach (ButtonAction buttonAction in button.Category.ButtonActions)
                    {
                        if (buttonAction.ActionProperty == null && buttonAction.ActionType.Type != EActionType.MuteMainAudio)
                            continue;

                        switch (buttonAction.ActionType.Type)
                        {
                            case EActionType.MuteProcess:
                                {
                                    if (IsPressed && !IsPressedOld)
                                    {                                        
                                        AudioController.MuteProcess(buttonAction.ActionProperty, !AudioController.IsProcessMute(buttonAction.ActionProperty));
                                    }
                                    break;
                                }
                            case EActionType.MuteMainAudio:
                                {
                                    if (IsPressed && !IsPressedOld)
                                    {
                                        AudioController.MuteSystem(!AudioController.IsSystemMute());
                                    }
                                    break;
                                }
                            case EActionType.MuteMic:
                                {
                                    if (IsPressed && !IsPressedOld)
                                    {
                                        AudioController.MuteMic(buttonAction.ActionProperty, !AudioController.IsMicMute(buttonAction.ActionProperty));
                                    }
                                    break;
                                }
                            case EActionType.OpenProcess:
                                {
                                    if (IsPressed && !IsPressedOld)
                                    {
                                        string path = buttonAction.ActionProperty;
                                        try
                                        {
                                            Process.Start(new ProcessStartInfo
                                            {
                                                FileName = path,
                                                UseShellExecute = true,
                                                WorkingDirectory = System.IO.Path.GetDirectoryName(path)
                                            });
                                        }
                                        catch (Exception ex)
                                        {
                                            Debug.WriteLine(ex);
                                        }
                                    }
                                    break;
                                }
                            case EActionType.OpenWebsite:
                                {
                                    if (IsPressed && !IsPressedOld)
                                    {
                                        try
                                        {
                                            Process.Start(new ProcessStartInfo
                                            {
                                                FileName = buttonAction.ActionProperty,
                                                UseShellExecute = true
                                            });
                                        }
                                        catch (Exception ex)
                                        {
                                            Debug.WriteLine(ex);
                                        }
                                    }
                                    break;
                                }
                            case EActionType.KeyPress:
                                {
                                    uint.TryParse(buttonAction.ActionProperty, out uint keyCode);
                                    if (IsPressed && !IsPressedOld)
                                    {                                       
                                        KeyController.HoldStart((ushort)keyCode);
                                    }
                                    if(!IsPressed && IsPressedOld)
                                    {
                                        KeyController.HoldStop((ushort)keyCode);
                                    }
                                    break;
                                }
                        }
                    }
                }                  
            }
        }

        private float SliderToFloat(int value, int mode = 0)
        {
            return SliderValueConverter.SliderToFloat(value, Settings.TranslationExponent);
        }
    }
}
