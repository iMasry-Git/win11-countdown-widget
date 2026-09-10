@echo off
setlocal EnableDelayedExpansion
title Countdown Widget Installer

echo ========================================================
echo        Windows 11 Desktop Countdown Widget Installer
echo ========================================================
echo.

set "TARGET_DIR=%LOCALAPPDATA%\Programs\CountdownWidget"
set "TARGET_EXE=%TARGET_DIR%\CountdownWidget.exe"

echo [*] Installing to: %TARGET_DIR%

:: Stop any running instance
taskkill /F /IM CountdownWidget.exe >nul 2>&1

:: Create target directory
if not exist "%TARGET_DIR%" mkdir "%TARGET_DIR%"

:: Copy files
copy /Y "%~dp0CountdownWidget.exe" "%TARGET_EXE%" >nul
if errorlevel 1 (
    echo [ERROR] Failed to copy files.
    pause
    exit /b 1
)

:: Create Desktop Shortcut
powershell -NoProfile -Command "$ws = New-Object -ComObject WScript.Shell; $s = $ws.CreateShortcut([System.IO.Path]::Combine([Environment]::GetFolderPath('Desktop'), 'Countdown Widget.lnk')); $s.TargetPath = '%TARGET_EXE%'; $s.WorkingDirectory = '%TARGET_DIR%'; $s.Description = 'Windows 11 Desktop Countdown Widget'; $s.Save()" >nul 2>&1
echo [+] Desktop shortcut created.

:: Create Start Menu Shortcut
powershell -NoProfile -Command "$ws = New-Object -ComObject WScript.Shell; $startDir = [Environment]::GetFolderPath('Programs'); $s = $ws.CreateShortcut([System.IO.Path]::Combine($startDir, 'Countdown Widget.lnk')); $s.TargetPath = '%TARGET_EXE%'; $s.WorkingDirectory = '%TARGET_DIR%'; $s.Description = 'Windows 11 Desktop Countdown Widget'; $s.Save()" >nul 2>&1
echo [+] Start Menu shortcut created.

echo.
echo ========================================================
echo          Installation completed successfully!
echo ========================================================
echo.

set /p RUNNOW="Would you like to launch Countdown Widget now? (Y/N): "
if /I "!RUNNOW!"=="Y" (
    start "" "%TARGET_EXE%"
)

exit /b 0
