Add-Type -AssemblyName System.Drawing

function Generate-ShieldBase64 {
    param(
        [string]$ShieldColorHex = "#5B4DFF",
        [string]$BorderColorHex = "#F1F3FE"
    )

    $width = 120
    $height = 120
    $bmp = New-Object System.Drawing.Bitmap($width, $height)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.Clear([System.Drawing.Color]::Transparent)

    $shieldColor = [System.Drawing.ColorTranslator]::FromHtml($ShieldColorHex)
    $borderColor = [System.Drawing.ColorTranslator]::FromHtml($BorderColorHex)

    # 1. Outer White Circle Background
    $bgBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
    $g.FillEllipse($bgBrush, 4, 4, 112, 112)

    # 2. Outer Soft Border
    $borderPen = New-Object System.Drawing.Pen($borderColor, 2)
    $g.DrawEllipse($borderPen, 4, 4, 112, 112)

    # 3. Shield Outline
    $shieldPen = New-Object System.Drawing.Pen($shieldColor, 6)
    $shieldPen.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round

    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $path.AddLine(60, 26, 90, 38)
    $path.AddBezier(90, 38, 92, 62, 78, 80, 60, 94)
    $path.AddBezier(60, 94, 42, 80, 28, 62, 30, 38)
    $path.AddLine(30, 38, 60, 26)
    $g.DrawPath($shieldPen, $path)

    # 4. Padlock Shackle
    $shacklePen = New-Object System.Drawing.Pen($shieldColor, 4)
    $g.DrawArc($shacklePen, 51, 46, 18, 18, 180, 180)

    # 5. Padlock Body
    $lockBrush = New-Object System.Drawing.SolidBrush($shieldColor)
    $g.FillRectangle($lockBrush, 48, 56, 24, 18)

    # Convert to Base64 PNG
    $ms = New-Object System.IO.MemoryStream
    $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
    $bytes = $ms.ToArray()
    $base64 = [Convert]::ToBase64String($bytes)

    $g.Dispose()
    $bmp.Dispose()
    $ms.Dispose()

    return $base64
}

$purpleBase64 = Generate-ShieldBase64 -ShieldColorHex "#5B4DFF" -BorderColorHex "#F1F3FE"
$redBase64 = Generate-ShieldBase64 -ShieldColorHex "#EF4444" -BorderColorHex "#FEE2E2"

Write-Host "PURPLE_BASE64_LENGTH: "$purpleBase64.Length
Write-Host "RED_BASE64_LENGTH: "$redBase64.Length

$purpleBase64 | Out-File -FilePath "d:\DATN\StudyHub\scratch\purple_shield.txt" -Encoding utf8
$redBase64 | Out-File -FilePath "d:\DATN\StudyHub\scratch\red_shield.txt" -Encoding utf8
