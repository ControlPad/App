using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlPad
{
    public class AudioStream
    {
        public string? Process { get; set; }
        public string? MicName { get; set; }
        public string? DeviceName { get; set; }
        public string DisplayName
        {
            get
            {
                if (MicName != null)
                    return MicName;
                if (Process != null)
                    return Process;
                if (DeviceName != null)
                    return $"Main Audio ({DeviceName})";
                return "Main Audio";
            }
        }

        public AudioStream(string? process, string? micName)
        {
            Process = process;
            MicName = micName;
        }

        public AudioStream(string? process, string? micName, string? deviceName)
        {
            Process = process;
            MicName = micName;
            DeviceName = deviceName;
        }
    }
}
