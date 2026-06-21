; IEMS School Management System - Inno Setup Script
; This script creates a professional installer (IEMS_Setup.exe)

[Setup]
AppName=IEMS School Management System
AppVersion=1.0
DefaultDirName={autopf}\IEMS School Management
DefaultGroupName=IEMS School Management
OutputDir=.
OutputBaseFilename=IEMS_Setup
Compression=lzma2
SolidCompression=yes
PrivilegesRequired=admin
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64

[Files]
Source: "IEMS_Release_Package\IEMS.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "IEMS_Release_Package\*.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "IEMS_Release_Package\INSTALLATION_INSTRUCTIONS.txt"; DestDir: "{app}"; Flags: ignoreversion isreadme

[Icons]
Name: "{group}\IEMS School Management"; Filename: "{app}\IEMS.exe"
Name: "{commondesktop}\IEMS School Management"; Filename: "{app}\IEMS.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop icon"; GroupDescription: "Additional icons:"

[Run]
Filename: "{app}\IEMS.exe"; Description: "Launch IEMS School Management System"; Flags: nowait postinstall skipifsilent

[Code]
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    MsgBox('Installation completed successfully!' + #13#10 + #13#10 +
           'Default Login:' + #13#10 +
           'Username: admin' + #13#10 +
           'Password: admin123' + #13#10 + #13#10 +
           'IMPORTANT: Change the default password after first login!', mbInformation, MB_OK);
  end;
end;
