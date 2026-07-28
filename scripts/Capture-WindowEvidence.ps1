[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][int]$ProcessId,
    [Parameter(Mandatory = $true)][string]$EvidenceDirectory,
    [string]$FileName = 'window-evidence.png'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')

try {
    $destination = Assert-KmgRuntimeEvidenceDirectory -Path $EvidenceDirectory
    if (-not (Test-Path -LiteralPath $destination -PathType Container)) {
        throw "Evidence directory does not exist: $destination"
    }
    if ([IO.Path]::GetFileName($FileName) -ne $FileName -or
        [IO.Path]::GetExtension($FileName) -ne '.png') {
        throw 'FileName must be one PNG filename without a directory.'
    }

    $process = Get-Process -Id $ProcessId -ErrorAction Stop
    $handle = $process.MainWindowHandle
    if ($handle -eq [IntPtr]::Zero) {
        throw "Process $ProcessId does not own a capturable main window."
    }

    Add-Type -AssemblyName System.Drawing
    if (-not ('KmgWindowCapture' -as [type])) {
        Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
public static class KmgWindowCapture {
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT { public int Left, Top, Right, Bottom; }
    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);
    [DllImport("user32.dll")]
    public static extern IntPtr GetWindowDC(IntPtr hWnd);
    [DllImport("user32.dll")]
    public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
    [DllImport("gdi32.dll")]
    public static extern bool BitBlt(IntPtr dest, int x, int y, int width, int height,
        IntPtr source, int sourceX, int sourceY, int operation);
}
'@
    }

    $rect = [KmgWindowCapture+RECT]::new()
    if (-not [KmgWindowCapture]::GetWindowRect($handle, [ref]$rect)) {
        throw 'GetWindowRect failed.'
    }
    $width = $rect.Right - $rect.Left
    $height = $rect.Bottom - $rect.Top
    if ($width -le 0 -or $height -le 0 -or $width -gt 16384 -or $height -gt 16384) {
        throw "Window dimensions are invalid: ${width}x${height}"
    }

    $bitmap = [Drawing.Bitmap]::new($width, $height)
    $graphics = [Drawing.Graphics]::FromImage($bitmap)
    $destinationDc = $graphics.GetHdc()
    $sourceDc = [KmgWindowCapture]::GetWindowDC($handle)
    try {
        if ($sourceDc -eq [IntPtr]::Zero -or
            -not [KmgWindowCapture]::BitBlt(
                $destinationDc, 0, 0, $width, $height, $sourceDc, 0, 0, 0x00CC0020)) {
            throw 'GDI BitBlt window capture failed.'
        }
    }
    finally {
        if ($sourceDc -ne [IntPtr]::Zero) {
            [void][KmgWindowCapture]::ReleaseDC($handle, $sourceDc)
        }
        $graphics.ReleaseHdc($destinationDc)
        $graphics.Dispose()
    }

    $path = Join-Path $destination $FileName
    $bitmap.Save($path, [Drawing.Imaging.ImageFormat]::Png)
    $bitmap.Dispose()
    Write-Host "Optional window evidence: $path"
    [pscustomobject]@{ Captured = $true; Path = $path; Warning = $null }
}
catch {
    $warning = "Optional window capture warning: $($_.Exception.Message)"
    Write-Warning $warning
    [pscustomobject]@{ Captured = $false; Path = $null; Warning = $warning }
}
