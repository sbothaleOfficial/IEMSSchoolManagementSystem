@echo off
title IEMS School Management System - Latest Version
echo ==========================================
echo   IEMS School Management System
echo   Running Latest Development Version
echo ==========================================
echo.

cd /d "%~dp0"

echo Stopping any running instances...
taskkill /F /IM "IEMS.exe" >nul 2>&1

echo.
echo Building latest version...
call dotnet build --nologo -v quiet

if %ERRORLEVEL% NEQ 0 (
    echo Build failed! Please check for compilation errors.
    pause
    exit /b 1
)

echo.
echo Running application from source...
call dotnet run --project IEMS.WPF --no-build

echo.
echo Application closed.
pause