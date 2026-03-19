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
        public string DisplayName { get; set; }

        public AudioStream(string? process, string? micName)
        {
            Process = process;
            MicName = micName;
            DisplayName = "Main Audio";

            if (MicName != null)
                DisplayName = MicName;
            else if (Process != null)
                DisplayName = Process;
        }
    }
}
