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
        public bool IsAutoDetected { get; set; }
        public string DisplayName { get; set; }

        public AudioStream(string? process, string? micName, bool isAutoDetected = false)
        {
            Process = process;
            MicName = micName;
            IsAutoDetected = isAutoDetected;

            if (MicName != null)
                DisplayName = MicName;
            else if (Process != null)
                DisplayName = Process;
            else if (MicName == null && Process == null)
                DisplayName = "Main Audio";
        }
    }
}
