# CLAUDE.md

## Project Overview

ControlPad is a WPF desktop app (.NET 9.0, Windows-only) that connects to an Arduino control pad via serial and maps physical sliders/buttons to system actions (audio control, key simulation, app launching).

## Tech Stack

- **Framework:** .NET 9.0 (net9.0-windows10.0.19041.0), WPF
- **UI:** WPF-UI (Fluent design, `FluentWindow`, `NavigationView`, `SymbolIcon`)
- **Audio:** NAudio + NAudio.Wasapi (per-process volume via `MMDevice`/`AudioSessionManager`)
- **Serial:** System.IO.Ports (115200 baud, CSV protocol from Arduino)
- **Device detection:** System.Management (WMI `Win32_DeviceChangeEvent` for hot-plug)
- **Notifications:** Microsoft.Toolkit.Uwp.Notifications (toast when minimized to tray)
- **Installer:** Inno Setup 6 (`Installer/ControlPad.iss`)

## Build & Run

```bash
dotnet build ControlPad/ControlPad.csproj -c Release
dotnet run --project ControlPad/ControlPad.csproj
dotnet publish ControlPad/ControlPad.csproj -c Release -r win-x64 --no-self-contained -o publish
```

## Architecture

### Serial Protocol
Arduino sends CSV lines at 50Hz: `slider1,slider2,...,slider6,btn1,btn2,...,btn11\n`
- Slider values: 0–1023 (analog)
- Button values: 0 or 1 (digital, INPUT_PULLUP inverted)

### Key Classes
- `ArduinoController` — Static. Opens serial port, runs async read loop, dispatches to EventHandler. Uses WMI watchers for USB connect/disconnect.
- `EventHandler` — Processes slider/button value changes. Applies dead zone to sliders. Triggers AudioController or KeyController actions.
- `AudioController` — NAudio wrapper. Per-process volume, system volume, mic volume, mute toggles.
- `KeyController` — Win32 `SendInput` for key press simulation with hold/repeat support.
- `DataHandler` — Static. Persistence layer using JSON files in `%AppData%/ControlPad/Presets/`. Manages presets, categories, control-to-category mappings.
- `Settings` — Static. Auto-persisting settings (theme, tray behavior, dead zone, translation exponent).
- `MainWindow` — FluentWindow with NavigationView. Pages: Home, Slider Categories, Button Categories, Settings.

### Data Storage
All user data in `%AppData%/ControlPad/Presets/<PresetName>/`:
- `SliderCategories.json`, `ButtonCategories.json` — Category definitions
- `CategoryControls.txt` — Which slider/button maps to which category
- `Settings.json` — User preferences
- `ID.txt` — Preset identifier

### UI Controls
- 6 `CustomSlider` + 11 `CustomButton` — Represent physical controls
- `SliderCategory` / `ButtonCategory` — Group controls with assigned audio streams or button actions
- Edit mode toggle shows/hides category assignment cells

## Conventions
- Static classes for singletons (`ArduinoController`, `DataHandler`, `Settings`)
- `Dispatcher.BeginInvoke` / `Dispatcher.Invoke` for UI thread marshalling
- Mutex for single-instance enforcement
- `--hidden` CLI arg for starting minimized
