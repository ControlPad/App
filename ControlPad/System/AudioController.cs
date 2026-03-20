using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
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
        private readonly ConcurrentDictionary<string, (long Tick, string DeviceId)> _micDeviceIdCache = new();
        private const int ProcessIdsCacheLifetimeMs = 500;
        private const int ProcessIdsCacheTrimIntervalMs = 5000;
        private const int ProcessIdsCacheMaxEntries = 128;
        private const int OutputDeviceCacheLifetimeMs = 3000;
        private const int MicDeviceCacheLifetimeMs = 3000;
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
            using var mic = GetMic(micName);
            volume = Math.Clamp(volume, 0f, 1f);
            if (mic != null) mic.AudioEndpointVolume.MasterVolumeLevelScalar = volume;
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
            using var mic = GetMic(micName);
            if (mic != null) mic.AudioEndpointVolume.Mute = mute;
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
            using var mic = GetMic(micName);
            if (mic != null) return mic.AudioEndpointVolume.Mute;
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

        public List<string> GetMicNames()
        {
            var names = new List<string>();
            using var enumerator = new MMDeviceEnumerator();
            foreach (var device in enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active))
            {
                names.Add(device.DeviceFriendlyName);
                device.Dispose();
            }
            return names;
        }

        /// <summary>
        /// Resolves an active capture endpoint by friendly name using a short-lived ID cache.
        /// Returned device instance must be disposed by the caller.
        /// </summary>
        private MMDevice? GetMic(string micName)
        {
            if (string.IsNullOrWhiteSpace(micName))
                return null;

            long nowTick = Environment.TickCount64;
            if (_micDeviceIdCache.TryGetValue(micName, out var cacheEntry) &&
                (nowTick - cacheEntry.Tick) <= MicDeviceCacheLifetimeMs)
            {
                try
                {
                    return _enum.GetDevice(cacheEntry.DeviceId);
                }
                catch
                {
                    _micDeviceIdCache.TryRemove(micName, out _);
                }
            }

            MMDevice? matchedDevice = null;
            foreach (var device in _enum.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active))
            {
                if (device.DeviceFriendlyName == micName)
                {
                    matchedDevice = device;
                    break;
                }
                device.Dispose();
            }

            if (matchedDevice != null)
            {
                _micDeviceIdCache[micName] = (nowTick, matchedDevice.ID);
            }

            return matchedDevice;
        }

        public SessionCollection GetAudioSessions()
        {
            using var device = _enum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            var sessions = device.AudioSessionManager.Sessions;
            return sessions;
        }

        private int[] GetProcessIds(string processName)
        {
            if (string.IsNullOrWhiteSpace(processName))
                return Array.Empty<int>();

            if (ProcessIdentifierHelper.IsExecutablePathIdentifier(processName))
                return GetProcessIdsByExecutablePath(processName);

            return GetProcessIdsByName(processName);
        }

        private int[] GetProcessIdsByName(string processName)
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
                (nowTick - lastTrimTick) > ProcessIdsCacheTrimIntervalMs)
            {
                if (Interlocked.CompareExchange(ref _lastProcessCacheTrimTick, nowTick, lastTrimTick) == lastTrimTick)
                {
                    TrimProcessCache(nowTick);
                }
            }
            return processIds;
        }

        private int[] GetProcessIdsByExecutablePath(string processIdentifier)
        {
            var processName = Path.GetFileNameWithoutExtension(processIdentifier);
            if (string.IsNullOrWhiteSpace(processName))
                return Array.Empty<int>();

            var matches = new List<int>();
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
                finally
                {
                    process.Dispose();
                }
            }

            if (matches.Count > 0)
                return matches.ToArray();

            // If no exact path matches are visible (e.g., inaccessible MainModule), fall back to process name.
            return GetProcessIdsByName(processName);
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

        private void TrimProcessCache(long nowTick)
        {
            var staleEntries = new List<(string Key, long Tick)>();
            foreach (var entry in _processIdsCache)
            {
                if ((nowTick - entry.Value.Tick) > ProcessIdsCacheLifetimeMs)
                    staleEntries.Add((entry.Key, entry.Value.Tick));
            }

            foreach (var entry in staleEntries)
            {
                _processIdsCache.TryRemove(entry.Key, out _);
            }

            int overflow = _processIdsCache.Count - ProcessIdsCacheMaxEntries;
            if (overflow <= 0) return;

            var staleKeySet = staleEntries.Count > 0
                ? new HashSet<string>(staleEntries.Select(e => e.Key))
                : null;
            var oldestKeys = _processIdsCache
                .Where(entry => staleKeySet == null || !staleKeySet.Contains(entry.Key))
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

        public List<string> GetOutputDeviceNames()
        {
            var names = new List<string>();
            foreach (var device in _enum.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
            {
                names.Add(device.DeviceFriendlyName);
                device.Dispose();
            }
            return names;
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

            MMDevice? matchedDevice = null;
            foreach (var device in _enum.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
            {
                if (matchedDevice == null && device.DeviceFriendlyName == outputDeviceName)
                {
                    matchedDevice = device;
                }
                else
                {
                    device.Dispose();
                }
            }

            if (matchedDevice != null)
            {
                _outputDeviceIdCache[outputDeviceName] = (nowTick, matchedDevice.ID);
                return matchedDevice;
            }

            return _enum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        }
    }
}
