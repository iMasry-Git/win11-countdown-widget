# Windows 11 Desktop Countdown Widget - Installation & User Guide

A lightweight, portable countdown widget designed specifically for Windows 11 and Windows 10. It sits directly on your desktop wallpaper under all open application windows and shows how many days remain until your event.

---

## 1. System Requirements & Prerequisites

### Minimum Requirements:
- **Operating System**: Windows 11 or Windows 10 (Version 1607 or newer).
- **Architecture**: 64-bit or 32-bit.
- **Framework**: **Microsoft .NET Framework 4.6.2, 4.7.x, or 4.8**.
  - *Note*: .NET Framework 4.8 comes **pre-installed by default** on virtually all Windows 10 and 11 installations. Under normal circumstances, you do not need to install anything.

### If .NET Framework is Missing:
If running on a stripped or custom Windows edition that lacks .NET Framework 4.8:
1. **Via Official Microsoft Web Installer**:
   Download and install from:
   [https://dotnet.microsoft.com/download/dotnet-framework/net48](https://dotnet.microsoft.com/download/dotnet-framework/net48)
2. **Or via Windows Package Manager (Command Prompt / PowerShell)**:
   ```cmd
   winget install Microsoft.DotNet.Framework.DeveloperPack_4
   ```

---

## 2. Installation Options

### Option A: Standard Installation (Recommended)
1. Extract the `CountdownWidget-v1.1-Windows.zip` archive anywhere on the computer.
2. Double-click **`Install.cmd`**.
3. The installer will:
   - Copy `CountdownWidget.exe` to `%LOCALAPPDATA%\Programs\CountdownWidget\`
   - Create a **Desktop shortcut**
   - Create a **Start Menu shortcut**
   - Ask if you want to launch the widget immediately.

### Option B: Portable Mode (No Installation)
1. You can run the widget directly from any folder or USB flash drive without installing.
2. Simply double-click **`CountdownWidget.exe`** or **`Run_Portable.cmd`**.
3. All user settings are saved locally to your user profile in `%APPDATA%\CountdownWidget\config.json`.

---

## 3. How to Use & Configure

- **First Launch**: When opened for the first time, the Settings dialog appears automatically so you can enter your Event Name and pick the Target Date.
- **Edit Event & Date**: Double-click anywhere on the widget card or click the **⚙️** icon.
- **Move the Widget**: Click and drag anywhere on the widget card to place it in any corner of your desktop. Its position is automatically remembered across restarts.
- **Lock Position**: Click the **🔓/🔒** button to lock the card and prevent accidental dragging.
- **Desktop Layer**: The widget is configured to sit quietly on your desktop wallpaper underneath all active windows, browsers, and applications.
- **Start with Windows**: Check "Start automatically with Windows" inside the settings dialog, or right-click the widget and select **Start with Windows**.
- **System Tray**: The widget displays a clock icon in the notification area (bottom right near the clock) where you can view days remaining on hover, reopen the settings, or exit.
- **Close / Exit**:
  - Click **✕** to minimize the widget to the system tray.
  - Right-click the widget or tray icon and choose **Exit Widget** to quit completely.

---

## 4. How to Uninstall
If you used `Install.cmd`, run **`Uninstall.cmd`** in this folder. It will cleanly remove:
- The desktop shortcut
- The start menu shortcut
- Application files in `%LOCALAPPDATA%\Programs\CountdownWidget\`
- Startup registry entries
