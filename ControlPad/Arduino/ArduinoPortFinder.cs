using System;
using System.IO.Ports;
using System.Management;

public static class ArduinoPortFinder
{
    // Gängige USB-Vendor-IDs der Boards
    private static readonly string[] KnownVids =
    {
        "VID_2341", // Original Arduino LLC/Genuino-Boards (Uno, Mega 2560 …) :contentReference[oaicite:0]{index=0}
        "VID_1A86", // CH340/CH341-Klone (Nano, Mini …)              :contentReference[oaicite:1]{index=1}
        "VID_0403"  // FTDI-basierte Klone (ältere Duemilanove usw.)
    };

    /// <summary>Returns the COM port and PNPDeviceID, or (null, null) if no Arduino was found.</summary>
    public static (string? port, string? deviceId) FindFirstArduinoPort()
    {
        foreach (string port in SerialPort.GetPortNames())          // 1. alle Ports holen
        {
            // 2. WMI-Abfrage: Welches USB-Gerät steckt hinter diesem Port?
            using var searcher = new ManagementObjectSearcher(
                $"SELECT * FROM Win32_PnPEntity WHERE Name LIKE '%({port})%'");

            foreach (ManagementObject dev in searcher.Get())
            {
                string name = dev["Name"]?.ToString() ?? "";
                string id = dev["PNPDeviceID"]?.ToString() ?? "";

                // 2a) Originale melden sich mit "Arduino XY (COMn)"
                if (name.IndexOf("Arduino", StringComparison.OrdinalIgnoreCase) >= 0)
                    return (port, id);

                // 2b) Klone: auf Vendor-ID prüfen
                if (Array.Exists(KnownVids, vid =>
                        id.IndexOf(vid, StringComparison.OrdinalIgnoreCase) >= 0))
                    return (port, id);
            }
        }
        return (null, null); // nichts gefunden
    }

    /// <summary>Extracts the serial number segment from a PNPDeviceID string (e.g. USB\VID_2341&amp;PID_0043\SERIAL).</summary>
    public static string? ExtractSerial(string? pnpDeviceId)
    {
        if (string.IsNullOrEmpty(pnpDeviceId))
            return null;

        // PNPDeviceID format: USB\VID_XXXX&PID_XXXX\SERIAL
        var parts = pnpDeviceId.Split('\\');
        return parts.Length >= 3 ? parts[2] : null;
    }
}
