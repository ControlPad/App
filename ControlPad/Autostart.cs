using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace ControlPad
{
    public static class AutoStart
    {
        const string RunKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        const string ApprovedKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run";
        const string AppName = "Slidr";

        public static void Set(bool enable, bool startHidden)
        {
            using var run = Registry.CurrentUser.OpenSubKey(RunKey, true) ?? Registry.CurrentUser.CreateSubKey(RunKey);

            if (enable)
            {
                string exe = Environment.ProcessPath!;
                string value = $"\"{exe}\"" + (startHidden ? " --hidden" : "");
                run!.SetValue(AppName, value, RegistryValueKind.String);

                // StartupApproved-Blockierung zurücksetzen
                using var approved = Registry.CurrentUser.OpenSubKey(ApprovedKey, true);
                approved?.DeleteValue(AppName, false);
            }
            else
            {
                run?.DeleteValue(AppName, false);
                using var approved = Registry.CurrentUser.OpenSubKey(ApprovedKey, true);
                approved?.DeleteValue(AppName, false);
            }
        }

        public static bool IsEnabled()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false))
            {
                return key?.GetValue(AppName) != null;
            }
        }
    }
}
