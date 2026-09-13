# Package-MSIX.ps1 - Automated MSIX Packaging for Windows 11 Desktop Countdown Widget
[CmdletBinding()]
param(
    [switch]$SelfSign,
    [string]$CertificatePath,
    [string]$CertificatePassword = "Password123"
)

$ErrorActionPreference = "Stop"
$projectRoot = Split-Path -Parent $PSScriptRoot
$packagingDir = Join-Path $projectRoot "packaging"
$binDir = Join-Path $projectRoot "bin"
$distDir = Join-Path $projectRoot "dist"
$layoutDir = Join-Path $distDir "msix-layout"
$outputMsix = Join-Path $distDir "CountdownWidget-v1.2.0.msix"

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "  MSIX Packager: Windows 11 Countdown Widget" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan

# 1. Ensure binary is compiled
$exePath = Join-Path $binDir "CountdownWidget.exe"
if (-not (Test-Path $exePath)) {
    Write-Host "Compiling executable..." -ForegroundColor Yellow
    & (Join-Path $projectRoot "build.ps1")
}

# 2. Ensure assets exist
$assetsDir = Join-Path $packagingDir "Assets"
$requiredAssets = @("StoreLogo.png", "Square44x44Logo.png", "Square150x150Logo.png", "Wide310x150Logo.png", "SplashScreen.png")
$missingAssets = $requiredAssets | Where-Object { -not (Test-Path (Join-Path $assetsDir $_)) }
if ($missingAssets) {
    Write-Host "Generating missing MSIX visual assets..." -ForegroundColor Yellow
    & (Join-Path $packagingDir "Generate-Assets.ps1")
}

# 3. Prepare layout directory
Write-Host "Preparing package layout in $layoutDir..." -ForegroundColor Yellow
if (Test-Path $layoutDir) {
    Remove-Item -Path $layoutDir -Recurse -Force
}
New-Item -ItemType Directory -Path $layoutDir -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $layoutDir "Assets") -Force | Out-Null

Copy-Item -Path $exePath -Destination (Join-Path $layoutDir "CountdownWidget.exe") -Force
Copy-Item -Path (Join-Path $packagingDir "AppxManifest.xml") -Destination (Join-Path $layoutDir "AppxManifest.xml") -Force
Copy-Item -Path (Join-Path $assetsDir "*") -Destination (Join-Path $layoutDir "Assets") -Force

Write-Host "Layout directory prepared." -ForegroundColor Green

# 4. Find MakeAppx.exe and SignTool.exe
$sdkPaths = @(
    "C:\Program Files (x86)\Windows Kits\10\bin\*\x64\makeappx.exe",
    "C:\Program Files\Windows Kits\10\bin\*\x64\makeappx.exe",
    "C:\Program Files (x86)\Windows Kits\10\App Certification Kit\makeappx.exe"
)

$makeAppx = $null
foreach ($pattern in $sdkPaths) {
    $match = Get-Item -Path $pattern -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($match) {
        $makeAppx = $match.FullName
        break
    }
}

if (-not $makeAppx) {
    $cmd = Get-Command makeappx.exe -ErrorAction SilentlyContinue
    if ($cmd) { $makeAppx = $cmd.Source }
}

if (-not $makeAppx) {
    Write-Host ""
    Write-Host "[NOTE] MakeAppx.exe (Windows SDK) was not detected on this machine." -ForegroundColor Yellow
    Write-Host "The layout folder has been prepared at:" -ForegroundColor Cyan
    Write-Host "  $layoutDir"
    Write-Host ""
    Write-Host "You can package this layout into an MSIX via any of the following 3 options:" -ForegroundColor White
    Write-Host "  1. GitHub Actions: Run the automated workflow (.github/workflows/package-msix.yml)" -ForegroundColor White
    Write-Host "  2. MSIX Packaging Tool: Open the free tool from Microsoft Store, choose 'Create package from directory', point to '$layoutDir'." -ForegroundColor White
    Write-Host "  3. Install Windows SDK: Install 'Windows App Certification Kit' from Microsoft (adds MakeAppx.exe & SignTool.exe)." -ForegroundColor White
    return
}

# 5. Pack MSIX
Write-Host "Found MakeAppx: $makeAppx" -ForegroundColor Green
Write-Host "Building MSIX package: $outputMsix..." -ForegroundColor Cyan

& $makeAppx pack /d $layoutDir /p $outputMsix /o

if ($LASTEXITCODE -ne 0) {
    Write-Error "MakeAppx failed with exit code $LASTEXITCODE"
    return
}

Write-Host "Successfully generated MSIX: $outputMsix" -ForegroundColor Green

# 6. Signing (Optional / Local Dev)
if ($SelfSign) {
    $signTool = $null
    $signtoolPaths = @(
        "C:\Program Files (x86)\Windows Kits\10\bin\*\x64\signtool.exe",
        "C:\Program Files\Windows Kits\10\bin\*\x64\signtool.exe"
    )
    foreach ($pattern in $signtoolPaths) {
        $match = Get-Item -Path $pattern -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($match) { $signTool = $match.FullName; break }
    }
    if (-not $signTool) {
        $cmd = Get-Command signtool.exe -ErrorAction SilentlyContinue
        if ($cmd) { $signTool = $cmd.Source }
    }

    if ($signTool) {
        Write-Host "Creating self-signed developer certificate..." -ForegroundColor Yellow
        $pfxPath = Join-Path $distDir "dev_certificate.pfx"
        
        $cert = New-SelfSignedCertificate -Type Custom -Subject "CN=iMasry" `
            -KeyUsage DigitalSignature -FriendlyName "Countdown Widget Dev" `
            -CertStoreLocation "Cert:\CurrentUser\My" `
            -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3")

        $secPwd = ConvertTo-SecureString -String $CertificatePassword -Force -AsPlainText
        Export-PfxCertificate -Cert $cert -FilePath $pfxPath -Password $secPwd | Out-Null

        Write-Host "Signing package with SignTool..." -ForegroundColor Yellow
        & $signTool sign /fd SHA256 /a /f $pfxPath /p $CertificatePassword $outputMsix
        Write-Host "Package signed for local testing with $pfxPath" -ForegroundColor Green
    } else {
        Write-Host "SignTool.exe not found. Skipping self-signing." -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "Ready for Microsoft Store:" -ForegroundColor Cyan
Write-Host "- For Store submission, upload this MSIX to Microsoft Partner Center." -ForegroundColor White
Write-Host "- Microsoft signs the package automatically during store ingestion." -ForegroundColor White
