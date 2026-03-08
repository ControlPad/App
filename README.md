# Slidr

A Windows desktop application that turns an Arduino-based control pad into a fully configurable audio mixer and macro board. Map physical sliders to per-app volume control and buttons to actions like muting, launching apps, opening websites, or simulating key presses.

> **Note for developers and AI assistants:** The C# namespaces, project file names, and internal identifiers still use `ControlPad`. Only the user-facing name has changed to **Slidr**. Do not rename namespaces, `.csproj` files, or the solution file.

## Features

- **Per-app volume control** — Assign audio streams (apps, microphones, system audio) to physical sliders
- **Button actions** — Mute processes/mic/system, open apps, open websites, simulate key presses
- **Categories** — Organize slider and button assignments into reusable categories
- **Presets** — Switch between different configurations instantly
- **System tray** — Minimize to tray, optional autostart with Windows
- **Dark/Light theme** — WPF-UI Fluent design with theme switching
- **Hot-plug** — Automatic Arduino detection and reconnection

## Requirements

### Software
- Windows 10 (build 19041) or later
- [.NET 9.0 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/9.0) (installed automatically by the installer)

### Hardware
- Arduino board (Uno, Nano, Mega, etc.)
- 6 analog sliders/potentiometers connected to pins A1–A6
- 11 push buttons/switches connected to pins 2–12 (using internal pull-up resistors)

## Arduino Wiring

| Component     | Arduino Pins         |
|---------------|----------------------|
| Sliders (6x)  | A1, A2, A3, A4, A5, A6 |
| Buttons (11x) | 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 |

Flash the `InoSkript/InoSkript.ino` sketch to your Arduino. It sends slider values and button states over serial at 115200 baud.

## Building from Source

```bash
# Clone the repository
git clone https://github.com/ControlPad/ControlPad.git
cd ControlPad

# Build
dotnet build ControlPad/ControlPad.csproj -c Release

# Run
dotnet run --project ControlPad/ControlPad.csproj
```

### Publishing

```bash
# Framework-dependent (requires .NET 9.0 runtime on target machine)
dotnet publish ControlPad/ControlPad.csproj -c Release -r win-x64 --no-self-contained -o publish

# Self-contained (bundles .NET runtime, ~80MB larger)
dotnet publish ControlPad/ControlPad.csproj -c Release -r win-x64 --self-contained -o publish
```

## Building the Installer

The installer is built with [Inno Setup](https://jrsoftware.org/isinfo.php) (v6+).

1. Publish the app (framework-dependent or self-contained)
2. Open `Installer/ControlPad.iss` in Inno Setup Compiler
3. Compile to produce `Slidr-Setup.exe`

Alternatively, use the GitHub Actions release workflow — push a version tag (`v*`) and the installer is built automatically.

## Project Structure

```
ControlPad/
  Arduino/           # Serial communication with Arduino
  System/            # Audio control (NAudio) and keyboard simulation
  UI Elements/       # WPF UserControls (Home, Settings, Categories)
  Windows/           # Main window and popup dialogs
  Resources/         # App icon
InoSkript/           # Arduino sketch
Installer/           # Inno Setup installer script
.github/workflows/   # CI/CD and release automation
```

## License

All rights reserved.
