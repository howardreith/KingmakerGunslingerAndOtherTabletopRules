[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$iconDirectory = Join-Path $repositoryRoot 'assets\game\icons'
foreach ($path in Get-ChildItem -LiteralPath $iconDirectory -Filter '*.png' -File) {
    $source = [Drawing.Bitmap]::FromFile($path.FullName)
    try {
        $output = New-Object Drawing.Bitmap $source.Width, $source.Height,
            ([Drawing.Imaging.PixelFormat]::Format32bppArgb)
        try {
            for ($y = 0; $y -lt $source.Height; $y++) {
                for ($x = 0; $x -lt $source.Width; $x++) {
                    $pixel = $source.GetPixel($x, $y)
                    if ($pixel.G -gt 130 -and $pixel.G -gt ($pixel.R * 1.35) -and
                        $pixel.G -gt ($pixel.B * 1.35)) {
                        $output.SetPixel($x, $y, [Drawing.Color]::FromArgb(0, 0, 0, 0))
                    }
                    else {
                        $output.SetPixel($x, $y, $pixel)
                    }
                }
            }
            $temporary = $path.FullName + '.tmp.png'
            $output.Save($temporary, [Drawing.Imaging.ImageFormat]::Png)
        }
        finally {
            $output.Dispose()
        }
    }
    finally {
        $source.Dispose()
    }
    Move-Item -LiteralPath $temporary -Destination $path.FullName -Force
}

Write-Host 'Removed chroma-key backgrounds from project icons.'
