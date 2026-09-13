# 🏪 Microsoft Store Publishing Guide: Windows 11 Desktop Countdown Widget

This guide provides a comprehensive, step-by-step walkthrough on how to **sign, package, and publish** the Windows 11 Desktop Countdown Widget to the **Microsoft Store**.

---

## 🧭 Executive Summary: The Two Publishing Paths

Microsoft Store now supports two different submission models for Win32 apps:

| Feature | Option A: MSIX Package (Recommended) | Option B: Win32 (Unpackaged / Installer) |
| :--- | :--- | :--- |
| **Package Format** | `.msix` container | `.exe` / `.zip` hosted on GitHub Releases |
| **User Experience** | Seamless 1-click install from Store app | Store launches your installer / download |
| **Auto-Updates** | Differential updates via Windows Update / Store | In-app check or manual download |
| **Uninstall** | 100% clean, zero residue left on system | Standard uninstaller (`Uninstall.cmd` or script) |
| **Code Signing Cost** | **$0 (Free)** — Microsoft signs it for you! | Standard certificate or Store certification check |

---

## 🔑 Part 1: Code Signing Demystified

### The #1 Myth
> *"I need to purchase an expensive EV Code Signing Certificate ($300–$500/year) to publish to Microsoft Store."*

**Fact: You DO NOT need to purchase any certificate!**

1. **For Microsoft Store Submissions**:
   - When you upload your `.msix` to Microsoft Partner Center, Microsoft's Store Ingestion Service verifies your developer account and **signs your package with Microsoft's official trusted CA certificate** (`CN=Microsoft Corporation`).
   - When users install your app from the Microsoft Store, Windows automatically trusts it without any SmartScreen warnings.
2. **For Local Testing (Sideloading before Store submission)**:
   - Windows requires locally installed MSIX packages to be digitally signed with a certificate that is trusted in your machine's `Trusted People` certificate store.
   - You can generate a **free self-signed certificate** using PowerShell in 10 seconds (see below).

---

## 📦 Part 2: Generating the MSIX Package

We have created an automated packaging pipeline in this repository under [`packaging/`](packaging/):

### Method 1: Automated Cloud Build via GitHub Actions (Easiest — Zero Local Setup)
GitHub Actions includes the full Windows SDK (`MakeAppx.exe`, `SignTool.exe`) pre-installed on its `windows-latest` runners.

1. Push your code or tag to GitHub:
   ```bash
   git push origin main
   ```
2. Go to **Actions** tab on your GitHub repository $\rightarrow$ Select **Build & Package MSIX** $\rightarrow$ Click **Run workflow**.
3. Once finished, download the ready-to-use **`CountdownWidget-MSIX`** artifact from the workflow run!

---

### Method 2: Standalone MSIX Packaging Tool (GUI — No SDK required)
If you prefer a local GUI without installing the large Windows SDK:

1. Open **Microsoft Store** on your PC.
2. Search for and install **MSIX Packaging Tool** (official Microsoft free tool).
3. Open the tool $\rightarrow$ Select **Package editor** or **Create package from a directory**.
4. Point to the pre-built directory in this repo:
   ```text
   dist\msix-layout
   ```
5. Click **Save** $\rightarrow$ It outputs a valid `.msix` package!

---

### Method 3: Command-Line with Windows SDK (`MakeAppx.exe`)
If you have the Windows SDK installed:

1. **Prepare Layout & Assets**:
   ```powershell
   powershell -ExecutionPolicy Bypass -File packaging\Package-MSIX.ps1
   ```
2. **Pack the MSIX**:
   ```powershell
   MakeAppx.exe pack /d dist\msix-layout /p dist\CountdownWidget.msix /o
   ```
3. **(Optional) Sign with a Self-Signed Cert for Local Testing**:
   ```powershell
   powershell -ExecutionPolicy Bypass -File packaging\Package-MSIX.ps1 -SelfSign
   ```

---

## 🏛️ Part 3: Step-by-Step Microsoft Store Submission

### Step 1: Create a Microsoft Partner Center Account
1. Visit the [Microsoft Partner Center](https://partner.microsoft.com/dashboard/registration).
2. Sign in with your Microsoft Account (e.g. Outlook/Hotmail/Work email).
3. Select **Developer programs** $\rightarrow$ **Windows & Xbox**.
4. Pay the one-time registration fee:
   - **Individual**: ~$19 USD (one-time fee, no annual subscriptions).
   - **Company**: ~$99 USD (one-time fee; requires business verification).

---

### Step 2: Reserve Your Application Name
1. In Partner Center, go to **Apps and games** $\rightarrow$ **Windows & Xbox** $\rightarrow$ **Overview**.
2. Click **New product** $\rightarrow$ Select **MSIX or PWA app**.
3. Enter your desired app name:
   - e.g. `Windows 11 Desktop Countdown Widget` or `Countdown Widget`.
4. Click **Check availability** $\rightarrow$ **Reserve product name**.

---

### Step 3: Link Package Identity to `AppxManifest.xml` (Critical!)
When your app name is reserved, Microsoft assigns your app a unique identity:

1. In Partner Center, go to **App management** $\rightarrow$ **App identity**.
2. Copy the three assigned values:
   - **Package/Identity/Name** (e.g. `67241iMasry.Windows11CountdownWidget`)
   - **Package/Identity/Publisher** (e.g. `CN=12345678-ABCD-EF01-2345-6789ABCDEF01`)
   - **Package/Properties/PublisherDisplayName** (e.g. `iMasry`)
3. Open [`packaging/AppxManifest.xml`](packaging/AppxManifest.xml) and replace the `<Identity>` section:
   ```xml
   <Identity
     Name="YOUR_PARTNER_CENTER_PACKAGE_NAME"
     ProcessorArchitecture="neutral"
     Publisher="YOUR_PARTNER_CENTER_PUBLISHER_ID"
     Version="1.2.0.0" />

   <Properties>
     <DisplayName>Windows 11 Desktop Countdown Widget</DisplayName>
     <PublisherDisplayName>YOUR_PARTNER_CENTER_DISPLAY_NAME</PublisherDisplayName>
     <Logo>Assets\StoreLogo.png</Logo>
     <Description>A sleek, lightweight desktop countdown widget designed for Windows 11</Description>
   </Properties>
   ```
4. Re-run packaging (or let GitHub Actions package it with these updated values).

---

### Step 4: Complete Store Submission Properties

Click **Start your submission** in Partner Center and fill out each section:

#### 1. Pricing and Availability
- **Base price**: Free
- **Markets**: All markets (worldwide)
- **Discoverability**: Publicly available in Store search

#### 2. Properties
- **Category**: *Productivity* (or *Utilities & tools*)
- **Support contact info**: Your GitHub repository issues URL: `https://github.com/iMasry-Git/win11-countdown-widget/issues`
- **Privacy policy**: URL to your README or privacy page

#### 3. Age Ratings (IARC)
- Complete the short IARC questionnaire (answer No to violence, offensive language, gambling, etc.).
- Rating will automatically be generated: **All Ages / PEGI 3 / ESRB Everyone**.

#### 4. Packages
- Drag and drop your generated **`CountdownWidget.msix`** (from `dist/` or GitHub Actions artifact).
- Partner Center will validate the package structure, capabilities (`runFullTrust`), and visual assets.

#### 5. Store Listings (English - United States)
- **Product description**:
  ```text
  A sleek, lightweight desktop countdown widget designed for Windows 11.
  It sits quietly on your desktop wallpaper underneath all open application windows, displaying the exact number of days remaining until your event.

  Features:
  - Wallpaper-layer pinned (underneath all open apps and browser windows)
  - Minimal dark slate card design with vivid electric orange typography
  - Streamlined 2-info setup: Event Name & Target Date
  - Draggable anywhere on multi-monitor setups with automatic position memory
  - One-click position lock to prevent accidental movement
  - System tray icon with hover days count
  - Pure native performance: 38 KB binary with zero bloat
  ```
- **Screenshots**: Upload [`assets/preview.jpg`](assets/preview.jpg) (the minimal zen desktop wallpaper screenshot).
- **Search keywords**:
  - `countdown`
  - `widget`
  - `days left`
  - `new year`
  - `event tracker`
  - `desktop gadget`

---

### Step 5: Submit to the Store
1. Click **Submit to the Store**.
2. Certification status will change to **In certification**:
   - Automated checks: 1–2 hours
   - Content compliance & security review: 24–48 hours
3. Once approved, the status changes to **Published**!
4. Your widget will be live on the Microsoft Store worldwide with a direct URL like:
   `https://apps.microsoft.com/detail/<YOUR_STORE_ID>`

---

## 🔄 Releasing Future Updates to the Store
For subsequent releases (e.g. `v1.3.0`):
1. Bump version in `src/AssemblyInfo.cs` and `packaging/AppxManifest.xml` (e.g. `1.3.0.0`).
2. Build new MSIX package.
3. In Partner Center $\rightarrow$ App $\rightarrow$ Click **Update**.
4. Under **Packages**, remove the old package and upload the new `.msix`.
5. Enter what's new in the release notes.
6. Click **Submit to the Store**. Updates roll out automatically to users via Microsoft Store background updates!
