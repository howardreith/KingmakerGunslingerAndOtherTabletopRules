[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem
$root=Split-Path -Parent $PSScriptRoot
$fixture=Join-Path $root ('artifacts/cleanup2-gallery-fixtures/'+[Guid]::NewGuid().ToString('N'))
$gallery=Join-Path $fixture 'gallery'
$evidence=Join-Path $fixture 'evidence'
$run=Join-Path $evidence '20261022T0000000000000Z-disposable-expanded-summoning-cleanup2-native-review'
[void][IO.Directory]::CreateDirectory($gallery);[void][IO.Directory]::CreateDirectory($run)
$export=Join-Path $PSScriptRoot 'Export-SummonCleanup2ReviewGallery.ps1'
$pass=0
function Write-TestJson([string]$Path,$Value) {[IO.File]::WriteAllText($Path,($Value|ConvertTo-Json -Depth 8),(New-Object Text.UTF8Encoding($false)))}
function Invoke-TestGallery([string]$Mode,[string]$Zip,[bool]$ShouldPass,[string]$Expected='') {
    $previous=$ErrorActionPreference
    try {
        # A native child's expected refusal writes stderr. Capture it without
        # letting PowerShell promote that expected text into a parent abort.
        $ErrorActionPreference='Continue'
        $out=@(& powershell.exe -NoProfile -ExecutionPolicy Bypass -File $export -Mode $Mode -OutputPath $Zip -GalleryDirectory $gallery -EvidenceRoot $evidence 2>&1)
        $exit=$LASTEXITCODE
    } finally {$ErrorActionPreference=$previous}
    if(($exit -eq 0) -ne $ShouldPass -or ($Expected -and ($out -join "`n") -notmatch $Expected)) {
        throw "Gallery fixture $Mode ($Zip) unexpected exit $exit`: $($out -join ' ')"
    }
    $script:pass++
}
function Edit-TestZip([string]$Name,[string]$Entry,[string]$Text,[bool]$Replace=$true) {
    $path=Join-Path $fixture $Name
    Copy-Item -LiteralPath (Join-Path $fixture 'valid.zip') -Destination $path
    $zip=[IO.Compression.ZipFile]::Open($path,[IO.Compression.ZipArchiveMode]::Update)
    try {
        if($Replace -and $zip.GetEntry($Entry)) {$zip.GetEntry($Entry).Delete()}
        $item=$zip.CreateEntry($Entry);$writer=New-Object IO.StreamWriter($item.Open(),(New-Object Text.UTF8Encoding($false)))
        try {$writer.Write($Text)} finally {$writer.Dispose()}
    } finally {$zip.Dispose()}
    return $path
}
# Synthetic fixture identities test the exporter; they are never native evidence.
$commit='a'*40;$dll='b'*64;$runId='synthetic-gallery-fixture'
Write-TestJson (Join-Path $run 'runtime-request.json') ([ordered]@{runId=$runId;scenario='disposable-expanded-summoning-cleanup2-native-review';parameters=@{reviewPhase='synthetic-unit-fixture'}})
Write-TestJson (Join-Path $run 'runtime-result.json') ([ordered]@{runId=$runId;scenario='disposable-expanded-summoning-cleanup2-native-review';gitCommit=$commit;
    status='FAIL';loadedModVersion='synthetic';gameVersion='synthetic';startUtc='synthetic';endUtc='synthetic';durationMilliseconds=0;
    assertions=@(@{name='synthetic-not-native';status='FAIL'})})
Write-TestJson (Join-Path $run 'runtime-loaded-build-identity.json') ([ordered]@{gitCommit=$commit;loadedModuleSha256=$dll;moduleVersionId='00000000-0000-0000-0000-000000000000'})
$identityNames=@('native-image-identities.json','review7-ettin-crab-identities.json','review7-menu-identities.json','review8-crab-identities.json','review9-tooltip-identities.json')
$png=[Convert]::FromBase64String('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+jA1sAAAAASUVORK5CYII=')
for($i=0;$i -lt $identityNames.Count;$i++) {
    $source=Join-Path $run ('native-ui-{0:D3}.png' -f $i);$name='original-'+$i+'.png'
    [IO.File]::WriteAllBytes($source,$png);[IO.File]::WriteAllBytes((Join-Path $gallery $name),$png)
    Write-TestJson (Join-Path $gallery $identityNames[$i]) @(@{source=$source;file=$name;sha256=(Get-FileHash -LiteralPath $source).Hash.ToLowerInvariant()})
}
$valid=Join-Path $fixture 'valid.zip'
Invoke-TestGallery Export $valid $true
Invoke-TestGallery Verify $valid $true
Invoke-TestGallery Export $valid $false 'overwrite'
$bad=Edit-TestZip 'changed-png.zip' 'images/original-0.png' 'changed pixels'
Invoke-TestGallery Verify $bad $false '(length|hash mismatch)'
$bad=Edit-TestZip 'extra-dll.zip' 'KingmakerGunslinger.dll' 'must never be exported'
Invoke-TestGallery Verify $bad $false 'Unexpected/duplicate'
$bad=Edit-TestZip 'changed-index.zip' 'index.html' '<img src="https://example.invalid/tracker">'
Invoke-TestGallery Verify $bad $false 'document hash mismatch'
$bad=Edit-TestZip 'duplicate-path.zip' 'images/ORIGINAL-0.png' 'duplicate' $false
Invoke-TestGallery Verify $bad $false 'Unexpected/duplicate'
[IO.File]::WriteAllBytes((Join-Path $gallery 'original-0.png'),[byte[]]@(1,2,3))
Invoke-TestGallery Export (Join-Path $fixture 'mismatched-source.zip') $false 'Source/gallery identity mismatch'
[pscustomobject]@{status='PASS';checks=$pass;fixtureDirectory=$fixture;nativeExecution=$false}
