<div align="center">

# ⏱️ Windows 11 Desktop Countdown Widget

A sleek, lightweight desktop gadget designed for Windows 11 & Windows 10.  
It sits directly on your desktop wallpaper **underneath all open application windows**, showing how many days remain until your event.

[![Platform](https://img.shields.io/badge/Platform-Windows%2011%20%7C%2010-0078D4?logo=windows)](https://github.com/iMasry-Git/win11-countdown-widget)
[![Framework](https://img.shields.io/badge/.NET%20Framework-4.8%2B-512BD4?logo=dotnet)](https://dotnet.microsoft.com/download/dotnet-framework/net48)
[![Release: v1.1.0](https://img.shields.io/badge/Release-v1.1.0-blue?logo=github)](https://github.com/iMasry-Git/win11-countdown-widget/releases)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](LICENSE)
[![Zero Dependencies](https://img.shields.io/badge/Dependencies-Zero%20External-success)](#)

<br/>

![Desktop Countdown Widget Preview](assets/preview.jpg)

</div>

---

## ✨ Features

- 🖥️ **Desktop Wallpaper Layer**: Stays strictly on the desktop background under all open apps, browser windows, and games (`HWND_BOTTOM` z-order).
- 📝 **Streamlined 2-Info Input**: Simple setup with just **Event Name** and **Target Date**—no time or hour setting required.
- 📐 **Compact & Minimalist**: Small footprint (230 × 105 px) that tucks into any corner of your desktop.
- 🎨 **Dark Grey & White Scheme**: Modern frosted dark grey acrylic card (`#222226`) with crisp white typography.
- 🎯 **Bold Countdown**: Displays clear remaining days (e.g. `45 DAYS LEFT` or `🎉 TODAY!`).
- 🖱️ **Draggable Anywhere**: Click and drag to place the card anywhere across multi-monitor setups; position is automatically saved.
- 🔒 **Position Lock**: One-click lock toggle (`🔒`/`🔓`) to prevent accidental movement.
- 🚀 **Start with Windows**: Built-in option to launch automatically on Windows login.
- 🔔 **System Tray Integration**: Notification icon displays remaining days on hover with quick access options.
- ⚡ **Zero External Dependencies**: Pure native .NET WPF compiled binary (38 KB). No Python, Node.js, or heavyweight runtimes required.

---

## 🚀 Installation & Quick Start

Pre-built binaries and distribution packages are available in the [`dist/`](dist/) folder:

### Option 1: Standard Installation (Recommended)
1. Download or extract [`CountdownWidget-v1.1-Windows.zip`](dist/CountdownWidget-v1.1-Windows.zip).
2. Double-click **`Install.cmd`**.
3. It will install the application to `%LOCALAPPDATA%\Programs\CountdownWidget\` and create **Desktop** & **Start Menu** shortcuts.

### Option 2: Portable Mode (No Installation)
1. Double-click **`CountdownWidget.exe`** (or `Run_Portable.cmd`).
2. Run directly from any folder or USB drive. Preferences are automatically saved in `%APPDATA%\CountdownWidget\config.json`.

---

## 🖱️ Controls & Shortcuts

| Action | How to Trigger |
| :--- | :--- |
| **Move Widget** | Click and drag anywhere on the widget card |
| **Edit Event & Date** | Double-click the card OR click the **⚙️** icon OR right-click > *Edit Event & Date...* |
| **Lock / Unlock Dragging** | Click the **🔓/🔒** button OR right-click > *Lock Position* |
| **Run on Windows Startup** | Check in settings OR right-click > *Start with Windows* |
| **Close to Tray** | Click **✕** (minimizes to system tray) |
| **Exit Completely** | Right-click the widget or tray icon > *Exit Widget* |

---

## ⚙️ Prerequisites

- **Operating System**: Windows 11 or Windows 10 (Version 1607+).
- **Runtime**: Microsoft .NET Framework 4.6.2, 4.7.x, or 4.8.
  - *.NET Framework 4.8 comes pre-installed on virtually all Windows 10/11 installations.*
  - If missing on custom stripped editions, install it via:
    ```cmd
    winget install Microsoft.DotNet.Framework.DeveloperPack_4
    ```

---

## 🛠️ Building from Source

You can compile the executable from source code using the built-in Windows 64-bit C# compiler without needing Visual Studio or the .NET SDK:

```powershell
# Clone the repository
git clone https://github.com/iMasry-Git/win11-countdown-widget.git
cd win11-countdown-widget

# Compile
powershell -ExecutionPolicy Bypass -File .\build.ps1
```

The compiled binary will be generated at `bin/CountdownWidget.exe`.

---

## 📄 License

This project is licensed under the **GNU General Public License v3.0** - see the [LICENSE](LICENSE) file for details.
