; ControlPad Inno Setup Script
; Requires Inno Setup 6.0+

#define MyAppName "ControlPad"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "ControlPad"
#define MyAppExeName "ControlPad.exe"
#define MyAppURL "https://github.com/your-username/ControlPad"

; .NET 9.0 Desktop Runtime download URL (x64)
#define DotNetRuntimeURL "https://download.visualstudio.microsoft.com/download/pr/dotnet-windowsdesktop-runtime-9.0-win-x64.exe"
#define DotNetInstallerFile "windowsdesktop-runtime-9.0-win-x64.exe"

[Setup]
AppId={{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputBaseFilename=ControlPad-Setup
SetupIconFile=..\ControlPad\Resources\logo.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.19041
PrivilegesRequired=admin
; Uncomment the next line if you have a license file
; LicenseFile=..\LICENSE.txt

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "autostart"; Description: "Start ControlPad with Windows"; GroupDescription: "Other options:"

[Files]
; Application files from publish output
Source: "..\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

; Bundle .NET runtime installer (optional — comment out if downloading instead)
; Source: "redist\{#DotNetInstallerFile}"; DestDir: "{tmp}"; Flags: deleteafterinstall; Check: not IsDotNet9Installed

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Registry]
; Autostart entry (only if task selected)
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "{#MyAppName}"; ValueData: """{app}\{#MyAppExeName}"" --hidden"; Flags: uninsdeletevalue; Tasks: autostart

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}"

[Code]
function IsDotNet9Installed(): Boolean;
var
  ResultCode: Integer;
  Output: AnsiString;
  TempFile: String;
begin
  Result := False;
  TempFile := ExpandConstant('{tmp}\dotnet_check.txt');

  // Check if the .NET 9.0 Desktop Runtime is installed via registry
  Result := RegKeyExists(HKLM, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App\9.0');

  if not Result then
  begin
    // Fallback: try checking via registry for any 9.0.x version
    Result := RegKeyExists(HKLM, 'SOFTWARE\WOW6432Node\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App\9.0');
  end;
end;

function InstallDotNetRuntime(): Boolean;
var
  ResultCode: Integer;
  RuntimePath: String;
begin
  Result := True;
  RuntimePath := ExpandConstant('{tmp}\{#DotNetInstallerFile}');

  // If bundled runtime exists, use it
  if FileExists(RuntimePath) then
  begin
    Exec(RuntimePath, '/install /quiet /norestart', '', SW_SHOW, ewWaitUntilTerminated, ResultCode);
    Result := (ResultCode = 0) or (ResultCode = 3010); // 3010 = success, reboot needed
  end
  else
  begin
    // Runtime not bundled — inform the user
    MsgBox('.NET 9.0 Desktop Runtime is required but was not found.' + #13#10 +
           'Please download and install it from:' + #13#10 +
           'https://dotnet.microsoft.com/download/dotnet/9.0', mbInformation, MB_OK);
    Result := False;
  end;
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
  Result := '';

  if not IsDotNet9Installed() then
  begin
    if not InstallDotNetRuntime() then
      Result := '.NET 9.0 Desktop Runtime installation failed or was cancelled. Please install it manually.';
  end;
end;
