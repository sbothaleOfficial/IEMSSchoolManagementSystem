@echo off
title IEMS School Management System - Published Version
echo ==========================================
echo   IEMS School Management System
echo   Building and Running Published Version
echo ==========================================
echo.

cd /d "%~dp0"

echo Stopping any running instances...
taskkill /F /IM "IEMS.exe" >nul 2>&1

echo.
echo Waiting for processes to close...
timeout /t 2 >nul

echo.
echo Publishing latest version...
call dotnet publish IEMS.WPF\IEMS.WPF.csproj -c Release -r win-x64 --self-contained false -o .\publish --force --nologo -v quiet

if %ERRORLEVEL% NEQ 0 (
    echo Publish failed! Please check for errors.
    pause
    exit /b 1
)

echo.
echo Starting application...
start "" "publish\IEMS.exe"

echo.
echo Application started! Check for the window.
timeout /t 3