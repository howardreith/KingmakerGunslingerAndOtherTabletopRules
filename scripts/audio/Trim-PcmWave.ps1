[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)][string]$InputPath,
    [Parameter(Mandatory=$true)][string]$OutputPath,
    [Parameter(Mandatory=$true)][double]$TrimSeconds
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$bytes = [IO.File]::ReadAllBytes([IO.Path]::GetFullPath($InputPath))
if($bytes.Length -lt 44 -or [Text.Encoding]::ASCII.GetString($bytes, 0, 4) -ne 'RIFF' -or
   [Text.Encoding]::ASCII.GetString($bytes, 8, 4) -ne 'WAVE') {
    throw 'Only RIFF/WAVE input is supported.'
}
$offset = 12
$formatOffset = -1
$formatLength = 0
$dataHeaderOffset = -1
$dataOffset = -1
$dataLength = 0
while($offset + 8 -le $bytes.Length) {
    $chunkName = [Text.Encoding]::ASCII.GetString($bytes, $offset, 4)
    $chunkLength = [BitConverter]::ToUInt32($bytes, $offset + 4)
    $chunkData = $offset + 8
    if($chunkData + $chunkLength -gt $bytes.Length) { throw "Invalid WAV chunk: $chunkName" }
    if($chunkName -eq 'fmt ') { $formatOffset = $chunkData; $formatLength = $chunkLength }
    if($chunkName -eq 'data') {
        $dataHeaderOffset = $offset
        $dataOffset = $chunkData
        $dataLength = $chunkLength
        break
    }
    $offset = $chunkData + $chunkLength + ($chunkLength % 2)
}
if($formatOffset -lt 0 -or $formatLength -lt 16 -or $dataOffset -lt 0) {
    throw 'WAV must contain fmt and data chunks.'
}
$formatTag = [BitConverter]::ToUInt16($bytes, $formatOffset)
$sampleRate = [BitConverter]::ToUInt32($bytes, $formatOffset + 4)
$blockAlign = [BitConverter]::ToUInt16($bytes, $formatOffset + 12)
if($formatTag -ne 1 -or $sampleRate -eq 0 -or $blockAlign -eq 0) {
    throw 'Only uncompressed PCM WAV input is supported.'
}
$trimFrames = [long][Math]::Round($TrimSeconds * $sampleRate,
    [MidpointRounding]::AwayFromZero)
$trimBytes = $trimFrames * $blockAlign
if($trimBytes -le 0 -or $trimBytes -ge $dataLength) {
    throw 'Trim duration must remove at least one frame and retain audio.'
}
$newDataLength = [int]($dataLength - $trimBytes)
$suffixOffset = $dataOffset + $dataLength
$suffixLength = $bytes.Length - $suffixOffset
$output = New-Object byte[] ($dataOffset + $newDataLength + $suffixLength)
[Array]::Copy($bytes, 0, $output, 0, $dataHeaderOffset + 4)
[Array]::Copy([BitConverter]::GetBytes([uint32]$newDataLength), 0,
    $output, $dataHeaderOffset + 4, 4)
[Array]::Copy($bytes, $dataOffset + $trimBytes, $output, $dataOffset,
    $newDataLength)
if($suffixLength -gt 0) {
    [Array]::Copy($bytes, $suffixOffset, $output, $dataOffset + $newDataLength,
        $suffixLength)
}
[Array]::Copy([BitConverter]::GetBytes([uint32]($output.Length - 8)), 0,
    $output, 4, 4)
$resolvedOutput = [IO.Path]::GetFullPath($OutputPath)
[IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($resolvedOutput)) | Out-Null
[IO.File]::WriteAllBytes($resolvedOutput, $output)
Write-Output ("Trimmed {0:F3} seconds ({1} frames) from PCM WAV: {2}" -f
    $TrimSeconds, $trimFrames, $resolvedOutput)
