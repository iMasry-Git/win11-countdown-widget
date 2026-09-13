# Generate-Assets.ps1 - Generates MSIX visual assets for Countdown Widget
Add-Type -AssemblyName System.Drawing

$assetsDir = Join-Path $PSScriptRoot "Assets"
if (-not (Test-Path $assetsDir)) {
    New-Item -ItemType Directory -Path $assetsDir -Force | Out-Null
}

$bgHex = "#222326"
$cardHex = "#2B2C30"
$borderHex = "#3A3C42"
$accentHex = "#FF6C00"
$whiteHex = "#FFFFFF"

function Draw-CardShape([System.Drawing.Graphics]$g, [float]$x, [float]$y, [float]$w, [float]$h, [float]$radius) {
    $path = [System.Drawing.Drawing2D.GraphicsPath]::new()
    $d = $radius * 2
    $path.AddArc($x, $y, $d, $d, 180, 90)
    $path.AddArc($x + $w - $d, $y, $d, $d, 270, 90)
    $path.AddArc($x + $w - $d, $y + $h - $d, $d, $d, 0, 90)
    $path.AddArc($x, $y + $h - $d, $d, $d, 90, 90)
    $path.CloseFigure()

    $brush = [System.Drawing.SolidBrush]::new([System.Drawing.ColorTranslator]::FromHtml($cardHex))
    $pen = [System.Drawing.Pen]::new([System.Drawing.ColorTranslator]::FromHtml($borderHex), [Math]::Max(1.0, $w * 0.02))
    $g.FillPath($brush, $path)
    $g.DrawPath($pen, $path)

    $brush.Dispose()
    $pen.Dispose()
    $path.Dispose()
}

function Create-SquareIcon([int]$size, [string]$filename) {
    $bmp = [System.Drawing.Bitmap]::new($size, $size)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
    $g.Clear([System.Drawing.ColorTranslator]::FromHtml($bgHex))

    $margin = [float]($size * 0.08)
    $innerSize = [float]($size - (2 * $margin))
    $radius = [float]($size * 0.16)

    Draw-CardShape $g $margin $margin $innerSize $innerSize $radius

    $fontFamily = "Segoe UI"
    try {
        $fontTest = [System.Drawing.Font]::new("Bahnschrift", 12)
        $fontFamily = "Bahnschrift"
        $fontTest.Dispose()
    } catch {}

    $accentBrush = [System.Drawing.SolidBrush]::new([System.Drawing.ColorTranslator]::FromHtml($accentHex))
    $whiteBrush = [System.Drawing.SolidBrush]::new([System.Drawing.ColorTranslator]::FromHtml($whiteHex))
    $sf = [System.Drawing.StringFormat]::new()
    $sf.Alignment = [System.Drawing.StringAlignment]::Center
    $sf.LineAlignment = [System.Drawing.StringAlignment]::Center

    if ($size -ge 100) {
        $numFont = [System.Drawing.Font]::new($fontFamily, [float]($size * 0.38), [System.Drawing.FontStyle]::Bold)
        $numRect = [System.Drawing.RectangleF]::new(0, [float]($size * 0.10), [float]$size, [float]($size * 0.50))
        $g.DrawString("10", $numFont, $accentBrush, $numRect, $sf)
        $numFont.Dispose()

        $subFont = [System.Drawing.Font]::new($fontFamily, [float]($size * 0.13), [System.Drawing.FontStyle]::Bold)
        $subRect = [System.Drawing.RectangleF]::new(0, [float]($size * 0.62), [float]$size, [float]($size * 0.22))
        $g.DrawString("DAYS", $subFont, $whiteBrush, $subRect, $sf)
        $subFont.Dispose()
    } else {
        $numFont = [System.Drawing.Font]::new($fontFamily, [float]($size * 0.50), [System.Drawing.FontStyle]::Bold)
        $numRect = [System.Drawing.RectangleF]::new(0, 0, [float]$size, [float]$size)
        $g.DrawString("10", $numFont, $accentBrush, $numRect, $sf)
        $numFont.Dispose()
    }

    $accentBrush.Dispose()
    $whiteBrush.Dispose()
    $sf.Dispose()
    $g.Dispose()

    $outPath = Join-Path $assetsDir $filename
    $bmp.Save($outPath, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    Write-Host "Generated $filename ($($size)x$($size))" -ForegroundColor Green
}

function Create-WideIcon([int]$w, [int]$h, [string]$filename) {
    $bmp = [System.Drawing.Bitmap]::new($w, $h)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
    $g.Clear([System.Drawing.ColorTranslator]::FromHtml($bgHex))

    $margin = [float]($h * 0.08)
    Draw-CardShape $g $margin $margin ([float]($w - 2 * $margin)) ([float]($h - 2 * $margin)) ([float]($h * 0.14))

    $fontFamily = "Segoe UI"
    try {
        $fontTest = [System.Drawing.Font]::new("Bahnschrift", 12)
        $fontFamily = "Bahnschrift"
        $fontTest.Dispose()
    } catch {}

    $accentBrush = [System.Drawing.SolidBrush]::new([System.Drawing.ColorTranslator]::FromHtml($accentHex))
    $whiteBrush = [System.Drawing.SolidBrush]::new([System.Drawing.ColorTranslator]::FromHtml($whiteHex))
    $sfCenter = [System.Drawing.StringFormat]::new()
    $sfCenter.Alignment = [System.Drawing.StringAlignment]::Center
    $sfCenter.LineAlignment = [System.Drawing.StringAlignment]::Center

    # Huge number on left
    $numFont = [System.Drawing.Font]::new($fontFamily, [float]($h * 0.48), [System.Drawing.FontStyle]::Bold)
    $numRect = [System.Drawing.RectangleF]::new(0, 0, [float]($w * 0.42), [float]$h)
    $g.DrawString("10", $numFont, $accentBrush, $numRect, $sfCenter)

    # DAYS LEFT on right
    $textLeft = [float]($w * 0.45)
    $textWidth = [float]($w * 0.50)
    $titleFont = [System.Drawing.Font]::new($fontFamily, [float]($h * 0.17), [System.Drawing.FontStyle]::Bold)
    $sfNear = [System.Drawing.StringFormat]::new()
    $sfNear.Alignment = [System.Drawing.StringAlignment]::Near
    $sfNear.LineAlignment = [System.Drawing.StringAlignment]::Center

    $textRect = [System.Drawing.RectangleF]::new($textLeft, 0, $textWidth, [float]$h)
    $g.DrawString("DAYS`nLEFT", $titleFont, $whiteBrush, $textRect, $sfNear)

    $numFont.Dispose()
    $titleFont.Dispose()
    $accentBrush.Dispose()
    $whiteBrush.Dispose()
    $sfCenter.Dispose()
    $sfNear.Dispose()
    $g.Dispose()

    $outPath = Join-Path $assetsDir $filename
    $bmp.Save($outPath, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    Write-Host "Generated $filename ($($w)x$($h))" -ForegroundColor Green
}

function Create-SplashScreen([int]$w, [int]$h, [string]$filename) {
    $bmp = [System.Drawing.Bitmap]::new($w, $h)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
    $g.Clear([System.Drawing.ColorTranslator]::FromHtml($bgHex))

    $fontFamily = "Segoe UI"
    try {
        $fontTest = [System.Drawing.Font]::new("Bahnschrift", 12)
        $fontFamily = "Bahnschrift"
        $fontTest.Dispose()
    } catch {}

    $accentBrush = [System.Drawing.SolidBrush]::new([System.Drawing.ColorTranslator]::FromHtml($accentHex))
    $whiteBrush = [System.Drawing.SolidBrush]::new([System.Drawing.ColorTranslator]::FromHtml($whiteHex))
    $sf = [System.Drawing.StringFormat]::new()
    $sf.Alignment = [System.Drawing.StringAlignment]::Center
    $sf.LineAlignment = [System.Drawing.StringAlignment]::Center

    $numFont = [System.Drawing.Font]::new($fontFamily, [float]($h * 0.35), [System.Drawing.FontStyle]::Bold)
    $numRect = [System.Drawing.RectangleF]::new(0, [float]($h * 0.18), [float]$w, [float]($h * 0.38))
    $g.DrawString("10", $numFont, $accentBrush, $numRect, $sf)

    $titleFont = [System.Drawing.Font]::new($fontFamily, [float]($h * 0.08), [System.Drawing.FontStyle]::Bold)
    $titleRect = [System.Drawing.RectangleF]::new(0, [float]($h * 0.58), [float]$w, [float]($h * 0.20))
    $g.DrawString("WINDOWS 11 COUNTDOWN WIDGET", $titleFont, $whiteBrush, $titleRect, $sf)

    $numFont.Dispose()
    $titleFont.Dispose()
    $accentBrush.Dispose()
    $whiteBrush.Dispose()
    $sf.Dispose()
    $g.Dispose()

    $outPath = Join-Path $assetsDir $filename
    $bmp.Save($outPath, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    Write-Host "Generated $filename ($($w)x$($h))" -ForegroundColor Green
}

# Generate all standard required AppX/MSIX tile logos
Create-SquareIcon 44 "Square44x44Logo.png"
Create-SquareIcon 50 "StoreLogo.png"
Create-SquareIcon 150 "Square150x150Logo.png"
Create-WideIcon 310 150 "Wide310x150Logo.png"
Create-SplashScreen 620 300 "SplashScreen.png"

Write-Host "All MSIX visual assets successfully created in $assetsDir" -ForegroundColor Cyan
