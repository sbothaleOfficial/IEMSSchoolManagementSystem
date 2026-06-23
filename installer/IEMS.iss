; Inno Setup script for the IEMS School Management System
; Builds a single Setup.exe that installs the self-contained app, adds Start-menu
; (and optional desktop) shortcuts with the app icon, and registers an uninstaller.
;
; Per-user install (no admin/UAC needed) into a WRITABLE folder, so the app can
; create its school.db next to the exe exactly like the portable copy does.
;
; Build:  "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" installer\IEMS.iss
; (publish the app first:  dotnet publish IEMS.WPF\IEMS.WPF.csproj -c Release -r win-x64 --self-contained true -o publish)

#define MyAppName "IEMS School Management"
#define MyAppShortName "IEMS"
#define MyAppVersion "1.1.2"
#define MyAppPublisher "Inspire English Medium School, Mardi"
#define MyAppExeName "IEMS.exe"
#define MyPublishDir "..\publish"
#define MyIcon "..\IEMS.WPF\exact_color.ico"

[Setup]
; A fixed AppId so upgrades and uninstall are tracked correctly across versions.
AppId={{8F3A1C42-2D7E-4B6A-9C1F-A1B2C3D4E5F6}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={localappdata}\Programs\{#MyAppShortName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
OutputDir=Output
OutputBaseFilename=IEMS_Setup_{#MyAppVersion}
SetupIconFile={#MyIcon}
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Additional shortcuts:"

[Files]
; Ship everything from the publish output EXCEPT the runtime database, so a fresh
; install always starts clean (default admin / admin123 on first launch).
Source: "{#MyPublishDir}\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs ignoreversion; \
    Excludes: "school.db,school.db-shm,school.db-wal,*.pdf"

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName} now"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; Remove logs/leftovers we created, but NOT the database (see note below).
Type: filesandordirs; Name: "{app}\logs"

; NOTE: school.db is intentionally never deleted on uninstall so the school's
; student/fee data is preserved across reinstalls and upgrades.
