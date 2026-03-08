; Slidr Inno Setup Script
; Requires Inno Setup 6.0+

#define MyAppName "Slidr"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Slidr"
#define MyAppExeName "Slidr.exe"
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
OutputBaseFilename=Slidr-Setup
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
Name: "autostart"; Description: "Start Slidr with Windows"; GroupDescription: "Other options:"

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
  TempFile: String;
  Lines: TArrayOfString;
  I: Integer;
  FindRec: TFindRec;
begin
  Result := False;
  TempFile := ExpandConstant('{tmp}\dotnet_check.txt');

  // Use dotnet CLI to list runtimes and check for WindowsDesktop 9.0.x
  if Exec('cmd.exe', '/c dotnet --list-runtimes > "' + TempFile + '" 2>&1', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    if LoadStringsFromFile(TempFile, Lines) then
    begin
      for I := 0 to GetArrayLength(Lines) - 1 do
      begin
        if (Pos('Microsoft.WindowsDesktop.App 9.0.', Lines[I]) > 0) then
        begin
          Result := True;
          Exit;
        end;
      end;
    end;
  end;

  // Fallback: check if the runtime folder exists on disk
  if not Result then
  begin
    if FindFirst(ExpandConstant('{commonpf}\dotnet\shared\Microsoft.WindowsDesktop.App\9.0.*'), FindRec) then
    begin
      Result := True;
      FindClose(FindRec);
    end;
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
