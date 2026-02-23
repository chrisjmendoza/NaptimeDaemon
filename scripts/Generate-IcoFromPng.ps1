param(
    [Parameter(Mandatory=$true)]
    [string]$PngPath,

    [Parameter(Mandatory=$true)]
    [string]$IcoPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.Drawing

$sizes = @(16,24,32,48,64,128,256)

$src = [System.Drawing.Bitmap]::FromFile($PngPath)
try {
    $images = @()

    foreach ($size in $sizes) {
        $bmp = New-Object System.Drawing.Bitmap $size, $size, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
        $g = [System.Drawing.Graphics]::FromImage($bmp)
        try {
            $g.Clear([System.Drawing.Color]::Transparent)
            $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
            $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
            $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
            $g.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality

            $g.DrawImage($src, 0, 0, $size, $size)
        }
        finally {
            $g.Dispose()
        }

        $ms = New-Object System.IO.MemoryStream
        $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
        $bmp.Dispose()

        $images += [PSCustomObject]@{ Size = $size; PngBytes = $ms.ToArray() }
        $ms.Dispose()
    }

    $outDir = [System.IO.Path]::GetDirectoryName($IcoPath)
    if (![string]::IsNullOrWhiteSpace($outDir)) {
        [System.IO.Directory]::CreateDirectory($outDir) | Out-Null
    }

    $stream = [System.IO.File]::Open($IcoPath, [System.IO.FileMode]::Create, [System.IO.FileAccess]::Write, [System.IO.FileShare]::None)
    try {
        $bw = New-Object System.IO.BinaryWriter($stream)

        # ICONDIR
        $bw.Write([UInt16]0)   # reserved
        $bw.Write([UInt16]1)   # type = 1 (icon)
        $bw.Write([UInt16]$images.Count)

        $dirEntrySize = 16
        $dataOffset = 6 + ($images.Count * $dirEntrySize)
        $currentOffset = $dataOffset

        foreach ($img in $images) {
            $size = [int]$img.Size
            $png = [byte[]]$img.PngBytes

            $w = if ($size -ge 256) { 0 } else { $size }
            $h = if ($size -ge 256) { 0 } else { $size }

            $bw.Write([byte]$w)        # width
            $bw.Write([byte]$h)        # height
            $bw.Write([byte]0)         # color count
            $bw.Write([byte]0)         # reserved
            $bw.Write([UInt16]1)       # planes
            $bw.Write([UInt16]32)      # bit count
            $bw.Write([UInt32]$png.Length)      # bytes in resource
            $bw.Write([UInt32]$currentOffset)   # image offset

            $currentOffset += $png.Length
        }

        foreach ($img in $images) {
            $bw.Write([byte[]]$img.PngBytes)
        }

        $bw.Flush()
    }
    finally {
        $stream.Dispose()
    }
}
finally {
    $src.Dispose()
}
