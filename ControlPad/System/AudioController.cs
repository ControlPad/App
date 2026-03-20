using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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

            List<int> processIds = Process.GetProcessesByName(processName).Select(c => c.Id).ToList(); // this might be slow
            

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

            List<int> processIds = Process.GetProcessesByName(processName).Select(c => c.Id).ToList(); // this might be slow

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

            List<int> processIds = Process.GetProcessesByName(processName).Select(c => c.Id).ToList(); // this might be slow

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
