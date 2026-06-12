# Generates the shared sprite PNGs from Unicode glyphs.
#
# Each icon is rendered as a white glyph on a transparent background into
# GameData/Textures/ of this shared module. These textures are merged into each
# consuming mod's release at build time.
#
# Why this is not a plain DrawString: a glyph drawn with the font's line
# metrics fills only a fraction of its em-box, and each symbol sits at a
# different height inside that box. Rendered later as a TMP sprite that fills the
# label line, that gives small, vertically inconsistent icons. So we render the
# glyph big, measure the REAL ink bounding box, then rescale it to fill a fixed
# fraction of the canvas, centered. A small downward bias compensates the TMP
# sprite anchoring (sprites are placed baseline->ascender, i.e. slightly high
# relative to text glyphs).
#
# Run from anywhere: powershell -ExecutionPolicy Bypass -File tools\create_sprites.ps1

Add-Type -AssemblyName System.Drawing

# Output size (px). 64 keeps the icon crisp when TMP downscales it to line height.
$size = 64

# Fraction of the canvas the ink should span (larger dimension). The remainder is
# transparent margin so the icon is not clipped.
$coverage = 0.82

# Downward bias (fraction of $size) applied to the ink, to counter the TMP sprite
# baseline anchoring that otherwise makes icons sit too high in a square button.
# Increase to push icons lower, decrease (or set to 0) to raise them.
$verticalBias = 0.08

# Supersampling size used to measure the ink precisely before downscaling.
$work = 256

# filename -> glyph to render
$icons = [ordered]@{
    'refresh_icon.png' = [char]0x21BB  # clockwise open circle arrow -> "Refresh"
}

# Resolve the Textures directory relative to this script, independent of the CWD.
$texturesDir = Join-Path $PSScriptRoot '..\GameData\Textures'
$texturesDir = [System.IO.Path]::GetFullPath($texturesDir)

# This font ships with Windows and covers the symbol glyphs used above.
$fontFamily = 'Segoe UI Symbol'
$font = New-Object System.Drawing.Font($fontFamily, ($work * 0.6), [System.Drawing.FontStyle]::Regular, [System.Drawing.GraphicsUnit]::Pixel)

$format = New-Object System.Drawing.StringFormat
$format.Alignment = [System.Drawing.StringAlignment]::Center
$format.LineAlignment = [System.Drawing.StringAlignment]::Center

$brush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::White)

# Returns the bounding box of non-transparent pixels in a 32bppArgb bitmap, as
# [left, top, right, bottom] (inclusive), or $null when the bitmap is fully transparent.
function Get-InkBounds([System.Drawing.Bitmap]$bitmap) {
    $w = $bitmap.Width
    $h = $bitmap.Height
    $rect = New-Object System.Drawing.Rectangle(0, 0, $w, $h)
    $data = $bitmap.LockBits($rect, [System.Drawing.Imaging.ImageLockMode]::ReadOnly, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    try {
        $stride = $data.Stride
        $bytes = New-Object byte[] ($stride * $h)
        [System.Runtime.InteropServices.Marshal]::Copy($data.Scan0, $bytes, 0, $bytes.Length)
    } finally {
        $bitmap.UnlockBits($data)
    }

    $minX = $w; $minY = $h; $maxX = -1; $maxY = -1
    for ($y = 0; $y -lt $h; $y++) {
        $row = $y * $stride
        for ($x = 0; $x -lt $w; $x++) {
            # BGRA: alpha is the 4th byte of each pixel.
            $alpha = $bytes[$row + $x * 4 + 3]
            if ($alpha -gt 16) {
                if ($x -lt $minX) { $minX = $x }
                if ($x -gt $maxX) { $maxX = $x }
                if ($y -lt $minY) { $minY = $y }
                if ($y -gt $maxY) { $maxY = $y }
            }
        }
    }
    if ($maxX -lt 0) { return $null }
    return @($minX, $minY, $maxX, $maxY)
}

foreach ($name in $icons.Keys) {
    $glyph = $icons[$name]

    # 1) Render the glyph large to measure its real ink extents.
    $big = New-Object System.Drawing.Bitmap($work, $work, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $gb = [System.Drawing.Graphics]::FromImage($big)
    $gb.Clear([System.Drawing.Color]::Transparent)
    $gb.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $gb.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAlias
    $gb.DrawString([string]$glyph, $font, $brush, (New-Object System.Drawing.RectangleF(0, 0, $work, $work)), $format)
    $gb.Dispose()

    $bounds = Get-InkBounds $big
    if ($null -eq $bounds) {
        Write-Warning "Glyph for $name produced no ink (missing in font?) - skipped"
        $big.Dispose()
        continue
    }
    $inkW = $bounds[2] - $bounds[0] + 1
    $inkH = $bounds[3] - $bounds[1] + 1

    # 2) Rescale the ink to cover the target fraction of the canvas, preserving aspect.
    $target = $size * $coverage
    $scale = [Math]::Min($target / $inkW, $target / $inkH)
    $drawW = $inkW * $scale
    $drawH = $inkH * $scale
    $offsetX = ($size - $drawW) / 2.0
    $offsetY = ($size - $drawH) / 2.0 + ($verticalBias * $size)

    $bmp = New-Object System.Drawing.Bitmap($size, $size, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    try {
        $g.Clear([System.Drawing.Color]::Transparent)
        $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality

        $srcRect = New-Object System.Drawing.RectangleF($bounds[0], $bounds[1], $inkW, $inkH)
        $dstRect = New-Object System.Drawing.RectangleF($offsetX, $offsetY, $drawW, $drawH)
        $g.DrawImage($big, $dstRect, $srcRect, [System.Drawing.GraphicsUnit]::Pixel)

        $path = Join-Path $texturesDir $name
        $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
        Write-Host ("Wrote {0}  (ink {1}x{2} -> {3:N0}x{4:N0})" -f $path, $inkW, $inkH, $drawW, $drawH)
    } finally {
        $g.Dispose()
        $bmp.Dispose()
        $big.Dispose()
    }
}

$brush.Dispose()
$font.Dispose()
$format.Dispose()
