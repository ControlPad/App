using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Linq;

namespace ControlPad
{
    public class AudioController
    {
        private MMDeviceEnumerator _enum;
        private readonly ConcurrentDictionary<string, (long Tick, int[] ProcessIds)> _processIdsCache = new();
        private const int ProcessIdsCacheLifetimeMs = 500;

        public AudioController()
        { 
            _enum = new MMDeviceEnumerator();           
        }

        public void SetProcessVolume(string processName, float volume)
        {
            var sessions = GetAudioSessions();

            volume = Math.Clamp(volume, 0f, 1f);

            int[] processIds = GetProcessIds(processName);
            

            for (int i = 0; i < sessions?.Count; i++)
            {
                var session = sessions[i];

                if (Array.IndexOf(processIds, (int)session.GetProcessID) >= 0)
                {
                    session.SimpleAudioVolume.Volume = volume;
                }
            }
        }

        public void SetSystemVolume(float volume)
        {
            using var device = _enum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            volume = Math.Clamp(volume, 0f, 1f);

            if(device != null) device.AudioEndpointVolume.MasterVolumeLevelScalar = volume;
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

            int[] processIds = GetProcessIds(processName);

            for (int i = 0; i < sessions?.Count; i++)
            {
                var session = sessions[i];
                if (Array.IndexOf(processIds, (int)session.GetProcessID) >= 0)
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

            int[] processIds = GetProcessIds(processName);

            for (int i = 0; i < sessions?.Count; i++)
            {
                var session = sessions[i];
                if (Array.IndexOf(processIds, (int)session.GetProcessID) >= 0)
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

        private int[] GetProcessIds(string processName)
        {
            long nowTick = Environment.TickCount64;
            if (_processIdsCache.TryGetValue(processName, out var cacheEntry))
            {
                if (nowTick - cacheEntry.Tick <= ProcessIdsCacheLifetimeMs)
                {
                    return cacheEntry.ProcessIds;
                }
            }

            var processes = Process.GetProcessesByName(processName);
            var processIds = new int[processes.Length];
            for (int i = 0; i < processes.Length; i++)
            {
                processIds[i] = processes[i].Id;
                processes[i].Dispose();
            }
            _processIdsCache[processName] = (nowTick, processIds);
            return processIds;
        }
    }
}
