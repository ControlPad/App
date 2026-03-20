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
        private readonly ConcurrentDictionary<string, (long Tick, string DeviceId)> _outputDeviceIdCache = new();
        private const int ProcessIdsCacheLifetimeMs = 500;
        private const int ProcessIdsCacheMaxEntries = 128;
        private const int OutputDeviceCacheLifetimeMs = 3000;
        private long _lastProcessCacheTrimTick = Environment.TickCount64;

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
            using var device = GetOutputDevice(null);
            if (device != null) device.AudioEndpointVolume.Mute = mute;
        }

        public void MuteSystem(bool mute, string? outputDeviceName)
        {
            using var device = GetOutputDevice(outputDeviceName);
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
            using var device = GetOutputDevice(null);
            if (device != null) return device.AudioEndpointVolume.Mute;
            return false;
        }

        public bool IsSystemMute(string? outputDeviceName)
        {
            using var device = GetOutputDevice(outputDeviceName);
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
                if ((nowTick - cacheEntry.Tick) <= ProcessIdsCacheLifetimeMs)
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
            long lastTrimTick = Interlocked.Read(ref _lastProcessCacheTrimTick);
            if (_processIdsCache.Count >= ProcessIdsCacheMaxEntries ||
                (nowTick - lastTrimTick) > ProcessIdsCacheLifetimeMs)
            {
                TrimProcessCache(nowTick);
                Interlocked.Exchange(ref _lastProcessCacheTrimTick, nowTick);
            }
            return processIds;
        }

        private void TrimProcessCache(long nowTick)
        {
            var staleKeys = _processIdsCache
                .Where(entry => (nowTick - entry.Value.Tick) > ProcessIdsCacheLifetimeMs)
                .Select(entry => entry.Key)
                .ToList();
            foreach (var key in staleKeys)
                _processIdsCache.TryRemove(key, out _);

            int overflow = _processIdsCache.Count - ProcessIdsCacheMaxEntries;
            if (overflow <= 0) return;

            var oldestKeys = _processIdsCache
                .OrderBy(entry => entry.Value.Tick)
                .Take(overflow)
                .Select(entry => entry.Key)
                .ToList();
            foreach (var key in oldestKeys)
                _processIdsCache.TryRemove(key, out _);
        }

        public List<MMDevice> GetOutputDevices()
        {
            return _enum.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).ToList();
        }

        private MMDevice? GetOutputDevice(string? outputDeviceName)
        {
            if (string.IsNullOrWhiteSpace(outputDeviceName))
                return _enum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

            long nowTick = Environment.TickCount64;
            if (_outputDeviceIdCache.TryGetValue(outputDeviceName, out var cacheEntry) &&
                (nowTick - cacheEntry.Tick) <= OutputDeviceCacheLifetimeMs)
            {
                try
                {
                    return _enum.GetDevice(cacheEntry.DeviceId);
                }
                catch
                {
                    _outputDeviceIdCache.TryRemove(outputDeviceName, out _);
                }
            }

            var device = GetOutputDevices().FirstOrDefault(d => d.DeviceFriendlyName == outputDeviceName);
            if (device != null)
            {
                _outputDeviceIdCache[outputDeviceName] = (nowTick, device.ID);
                return device;
            }

            return _enum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        }
    }
}
