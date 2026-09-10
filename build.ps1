$ErrorActionPreference = "Stop"

$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$srcDir = Join-Path $projectRoot "src"
$binDir = Join-Path $projectRoot "bin"

if (-not (Test-Path $binDir)) {
    New-Item -ItemType Directory -Path $binDir | Out-Null
}

$csc = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if (-not (Test-Path $csc)) {
    $csc = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe"
}

$wpfDir = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\WPF"
if (-not (Test-Path $wpfDir)) {
    $wpfDir = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\WPF"
}

$fxDir = Split-Path -Parent $wpfDir

$refs = @(
    (Join-Path $wpfDir "PresentationFramework.dll"),
    (Join-Path $wpfDir "PresentationCore.dll"),
    (Join-Path $wpfDir "WindowsBase.dll"),
    (Join-Path $fxDir "System.Xaml.dll"),
    (Join-Path $fxDir "System.dll"),
    (Join-Path $fxDir "System.Core.dll"),
    (Join-Path $fxDir "System.Drawing.dll"),
    (Join-Path $fxDir "System.Windows.Forms.dll")
)

$refArgs = $refs | ForEach-Object { "/r:`"$_`"" }

$sources = Get-ChildItem -Path $srcDir -Filter "*.cs" | ForEach-Object { "`"$($_.FullName)`"" }

$outputExe = Join-Path $binDir "CountdownWidget.exe"

Write-Host "Compiling CountdownWidget to $outputExe..." -ForegroundColor Cyan

$allArgs = @("/target:winexe", "/out:`"$outputExe`"", "/nologo", "/optimize+") + $refArgs + $sources

$process = Start-Process -FilePath $csc -ArgumentList $allArgs -NoNewWindow -Wait -PassThru

if ($process.ExitCode -eq 0) {
    Write-Host "Build succeeded! Executable created at: $outputExe" -ForegroundColor Green
} else {
    Write-Error "Build failed with exit code $($process.ExitCode)"
}
