@echo off
title Uninstall Countdown Widget

echo ========================================================
echo        Uninstalling Countdown Widget...
echo ========================================================
echo.

:: Terminate running process
taskkill /F /IM CountdownWidget.exe >nul 2>&1
echo [*] Stopped any running instances.

:: Remove Shortcuts
powershell -NoProfile -Command "Remove-Item -Path ([System.IO.Path]::Combine([Environment]::GetFolderPath('Desktop'), 'Countdown Widget.lnk')) -ErrorAction SilentlyContinue; Remove-Item -Path ([System.IO.Path]::Combine([Environment]::GetFolderPath('Programs'), 'Countdown Widget.lnk')) -ErrorAction SilentlyContinue" >nul 2>&1
echo [*] Removed Desktop and Start Menu shortcuts.

:: Remove Startup entry if exists
reg delete "HKCU\Software\Microsoft\Windows\CurrentVersion\Run" /v "CountdownWidget" /f >nul 2>&1

:: Remove Program Files
set "TARGET_DIR=%LOCALAPPDATA%\Programs\CountdownWidget"
if exist "%TARGET_DIR%" (
    rmdir /S /Q "%TARGET_DIR%" >nul 2>&1
    echo [*] Removed application files.
)

echo.
echo ========================================================
echo           Countdown Widget has been uninstalled.
echo ========================================================
echo.
pause
exit /b 0
