using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;

namespace ControlPad
{
    public class AudioController
    {
        private MMDeviceEnumerator _enum;

        public AudioController()
        { 
            _enum = new MMDeviceEnumerator();           
        }

        public void SetProcessVolume(string processName, float volume)
        {
            var sessions = GetAudioSessions();

            volume = Math.Clamp(volume, 0f, 1f);

            var processIds = GetProcessIds(processName);
            

            for (int i = 0; i < sessions?.Count; i++)
            {
                var session = sessions[i];

                if (processIds.Contains((int)session.GetProcessID))
                {
                    session.SimpleAudioVolume.Volume = volume;
                }
            }
        }

        public void SetSystemVolume(float volume)
        {
            using var device = GetOutputDevice(null);
            volume = Math.Clamp(volume, 0f, 1f);

            if(device != null) device.AudioEndpointVolume.MasterVolumeLevelScalar = volume;
        }

        public void SetSystemVolume(float volume, string? outputDeviceName)
        {
            using var device = GetOutputDevice(outputDeviceName);
            volume = Math.Clamp(volume, 0f, 1f);

            if (device != null) device.AudioEndpointVolume.MasterVolumeLevelScalar = volume;
        }

        public void SetMicVolume(string micName, float volume)
        {
            List<MMDevice> mics = GetMics();
            foreach (MMDevice mic in mics)
            {
                if (mic.DeviceFriendlyName == micName)
                    mic.AudioEndpointVolume.MasterVolumeLevelScalar = volume;
            }
        }

        public void MuteProcess(string processName, bool mute)
        {
            using var device = _enum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            var sessions = device.AudioSessionManager.Sessions;

            var processIds = GetProcessIds(processName);

            for (int i = 0; i < sessions?.Count; i++)
            {
                var session = sessions[i];
                if (processIds.Contains((int)session.GetProcessID))
                {
                    session.SimpleAudioVolume.Mute = mute;
                }
            }
        }

        public void MuteSystem(bool mute)
        {
            using var device = _enum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            if (device != null) device.AudioEndpointVolume.Mute = mute;
        }

        public void MuteMic(string micName, bool mute)
        {
            List<MMDevice> mics = GetMics();
            foreach (MMDevice mic in mics)
            {
                if (mic.DeviceFriendlyName == micName)
                    mic.AudioEndpointVolume.Mute = mute;
            }
        }

        public bool IsProcessMute(string processName)
        {
            using var device = _enum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            var sessions = device.AudioSessionManager.Sessions;

            var processIds = GetProcessIds(processName);

            for (int i = 0; i < sessions?.Count; i++)
            {
                var session = sessions[i];
                if (processIds.Contains((int)session.GetProcessID))
                {
                    return session.SimpleAudioVolume.Mute;
                }
            }
            return false;
        }

        public bool IsSystemMute()
        {
            using var device = _enum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            if (device != null) return device.AudioEndpointVolume.Mute;
            return false;
        }

        public bool IsMicMute(string micName)
        {
            List<MMDevice> mics = GetMics();
            foreach (MMDevice mic in mics)
            {
                if (mic.DeviceFriendlyName == micName)
                    return mic.AudioEndpointVolume.Mute;
            }
            return false;
        }

        public List<MMDevice> GetMics()
        {
            var mics = new List<MMDevice>();
            var enumerator = new MMDeviceEnumerator();
            var devices = enumerator.EnumerateAudioEndPoints(
                DataFlow.Capture,
                DeviceState.Active
            );
           
            foreach (var device in devices)
            {
                mics.Add(device);
            }
            return mics;
        }

        public SessionCollection GetAudioSessions()
        {
            using var device = _enum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            var sessions = device.AudioSessionManager.Sessions;
            return sessions;
        }

        private static HashSet<int> GetProcessIds(string processIdentifier)
        {
            if (string.IsNullOrWhiteSpace(processIdentifier))
                return new HashSet<int>();

            bool isExeIdentifier = ProcessIdentifierHelper.IsExecutablePathIdentifier(processIdentifier);

            if (!isExeIdentifier)
            {
                return Process.GetProcessesByName(processIdentifier)
                    .Select(process => process.Id)
                    .ToHashSet();
            }

            var processName = Path.GetFileNameWithoutExtension(processIdentifier);
            var matches = new HashSet<int>();
            var resolvedTargetPath = TryGetFullPath(processIdentifier);

            foreach (var process in Process.GetProcessesByName(processName))
            {
                try
                {
                    var processPath = process.MainModule?.FileName;
                    if (string.IsNullOrWhiteSpace(processPath))
                        continue;

                    if (resolvedTargetPath == null)
                    {
                        // Intermediate fallback: when configured path cannot be resolved, collect name matches now.
                        // Final fallback below handles the case where no matches were collected in this loop.
                        matches.Add(process.Id);
                    }
                    else
                    {
                        var resolvedProcessPath = TryGetFullPath(processPath);
                        if (resolvedProcessPath != null &&
                            string.Equals(resolvedProcessPath, resolvedTargetPath, StringComparison.OrdinalIgnoreCase))
                        {
                            matches.Add(process.Id);
                        }
                    }
                }
                catch
                {
                    // Ignore processes where main module cannot be read.
                }
            }

            if (matches.Count > 0)
                return matches;

            // If no exact path matches are visible (e.g., inaccessible MainModule), fall back to process name.
            return Process.GetProcessesByName(processName)
                .Select(process => process.Id)
                .ToHashSet();
        }

        private static string? TryGetFullPath(string path)
        {
            try
            {
                return Path.GetFullPath(path);
            }
            catch
            {
                // Invalid or inaccessible paths should not break process matching; caller handles null as fallback.
                return null;
            }
        }
        
        public List<MMDevice> GetOutputDevices()
        {
            return _enum.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).ToList();
        }

        private MMDevice? GetOutputDevice(string? outputDeviceName)
        {
            if (string.IsNullOrWhiteSpace(outputDeviceName))
                return _enum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

            return GetOutputDevices().FirstOrDefault(d => d.DeviceFriendlyName == outputDeviceName)
                   ?? _enum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        }
    }
}
