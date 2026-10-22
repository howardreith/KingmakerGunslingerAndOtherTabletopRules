[CmdletBinding()]
param(
    [ValidateSet('Export','Verify')][string]$Mode='Export',
    [string]$GalleryDirectory,
    [Parameter(Mandatory=$true)][string]$OutputPath,
    [string]$EvidenceRoot='C:/Dev/KingmakerGunslingerLab/runtime-evidence'
)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
if([string]::IsNullOrWhiteSpace($GalleryDirectory)) {$GalleryDirectory=Join-Path (Split-Path -Parent $PSScriptRoot) 'artifacts/cleanup2-review-gallery'}
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

function Get-GalleryStreamHash([IO.Stream]$Stream) {
    $sha=[Security.Cryptography.SHA256]::Create()
    try {return ([BitConverter]::ToString($sha.ComputeHash($Stream))).Replace('-','').ToLowerInvariant()}
    finally {$sha.Dispose()}
}
function Read-GalleryJson([string]$Path) {return (Get-Content -Raw -LiteralPath $Path | ConvertFrom-Json)}
function Get-GalleryOptional($Object,[string]$Name) {
    if($null -ne $Object -and $null -ne $Object.PSObject.Properties[$Name]) {return $Object.$Name}
    return $null
}
function Write-GalleryText($Archive,[string]$Name,[string]$Text) {
    $entry=$Archive.CreateEntry($Name,[IO.Compression.CompressionLevel]::Optimal)
    $stream=$entry.Open();$writer=New-Object IO.StreamWriter($stream,(New-Object Text.UTF8Encoding($false)))
    try {$writer.Write($Text)} finally {$writer.Dispose()}
}
function Read-GalleryEntryText($Entry) {
    $reader=New-Object IO.StreamReader($Entry.Open())
    try {return $reader.ReadToEnd()} finally {$reader.Dispose()}
}
function Get-GalleryTextHash([string]$Text) {
    $stream=New-Object IO.MemoryStream(,[Text.Encoding]::UTF8.GetBytes($Text))
    try {return Get-GalleryStreamHash $stream} finally {$stream.Dispose()}
}
function Test-GalleryArchive([string]$Path) {
    $archive=[IO.Compression.ZipFile]::OpenRead($Path)
    try {
        $names=New-Object 'Collections.Generic.HashSet[string]' ([StringComparer]::OrdinalIgnoreCase)
        foreach($entry in $archive.Entries) {
            if(-not $names.Add($entry.FullName) -or $entry.FullName -cnotmatch '^(index\.html|README\.md|manifest\.json|images/[A-Za-z0-9][A-Za-z0-9_.-]*\.png)$') {
                throw "Unexpected/duplicate portable gallery entry: $($entry.FullName)"
            }
        }
        $manifestEntry=$archive.GetEntry('manifest.json')
        if($null -eq $manifestEntry) {throw 'Portable gallery manifest is missing.'}
        $manifest=(Read-GalleryEntryText $manifestEntry)|ConvertFrom-Json
        if($manifest.schemaVersion -ne 1 -or $manifest.kind -cne 'cleanup2-original-native-screenshots' -or @($manifest.images).Count -lt 1) {
            throw 'Unsupported or empty portable gallery manifest.'
        }
        $expected=New-Object 'Collections.Generic.HashSet[string]' ([StringComparer]::OrdinalIgnoreCase)
        foreach($name in @('index.html','README.md','manifest.json')) {[void]$expected.Add($name)}
        $runIds=@($manifest.runs | ForEach-Object runId)
        foreach($image in $manifest.images) {
            if($image.path -cnotmatch '^images/[A-Za-z0-9][A-Za-z0-9_.-]*\.png$' -or -not $expected.Add($image.path) -or
                $image.sha256 -cnotmatch '^[0-9a-f]{64}$' -or $image.runId -cnotin $runIds) {throw 'Invalid image identity in portable manifest.'}
            $entry=$archive.GetEntry($image.path)
            if($null -eq $entry -or $entry.Length -ne $image.length) {throw "Missing/changed image length: $($image.path)"}
            $stream=$entry.Open()
            try {$hash=Get-GalleryStreamHash $stream} finally {$stream.Dispose()}
            if($hash -cne $image.sha256) {throw "PNG byte hash mismatch: $($image.path)"}
        }
        if($expected.Count -ne $names.Count -or @($expected | Where-Object {-not $names.Contains($_)}).Count -ne 0) {throw 'Portable gallery entry set differs from manifest.'}
        foreach($run in $manifest.runs) {
            if($run.sourceCommit -cnotmatch '^[0-9a-f]{40}$' -or $run.dllSha256 -cnotmatch '^[0-9a-f]{64}$' -or
                $run.dllMvid -cnotmatch '^[0-9a-f-]{36}$' -or $run.status -cnotin @('PASS','FAIL')) {throw 'Incomplete native group identity.'}
        }
        foreach($name in @('index.html','README.md')) {
            $text=Read-GalleryEntryText ($archive.GetEntry($name))
            $record=@($manifest.documents | Where-Object {$_.path -ceq $name})
            if($record.Count -ne 1 -or (Get-GalleryTextHash $text) -cne $record[0].sha256) {throw "Portable document hash mismatch: $name"}
        }
        $index=Read-GalleryEntryText ($archive.GetEntry('index.html'))
        if($index -match '(?i)(?:href|src)\s*=\s*["''](?:file:|https?:|[A-Z]:|/|\\)') {throw 'Gallery HTML contains a nonportable/external link.'}
        foreach($image in $manifest.images) {
            if(-not $index.Contains('src="'+$image.path+'"')) {throw "Image not reachable in portable index: $($image.path)"}
        }
        return [pscustomobject]@{status='PASS';mode='Verify';zipPath=[IO.Path]::GetFullPath($Path);
            zipSha256=(Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant();images=@($manifest.images).Count;
            groups=@($manifest.runs).Count;entries=$names.Count;bytes=(Get-Item -LiteralPath $Path).Length}
    } finally {$archive.Dispose()}
}

$OutputPath=[IO.Path]::GetFullPath($OutputPath)
if($Mode -eq 'Verify') {Test-GalleryArchive $OutputPath;return}
$repo=[IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$artifacts=[IO.Path]::GetFullPath((Join-Path $repo 'artifacts')).TrimEnd('\','/')+[IO.Path]::DirectorySeparatorChar
if(-not $OutputPath.StartsWith($artifacts,[StringComparison]::OrdinalIgnoreCase) -or [IO.Path]::GetExtension($OutputPath) -cne '.zip') {
    throw 'Export must create a new ZIP inside this worktree artifacts directory.'
}
if(Test-Path -LiteralPath $OutputPath) {throw 'Export refuses to overwrite an existing packet.'}
$GalleryDirectory=[IO.Path]::GetFullPath($GalleryDirectory)
$EvidenceRoot=[IO.Path]::GetFullPath($EvidenceRoot).TrimEnd('\','/')+[IO.Path]::DirectorySeparatorChar
$identityFiles=@('native-image-identities.json','review7-ettin-crab-identities.json','review7-menu-identities.json',
    'review8-crab-identities.json','review9-tooltip-identities.json')
$excluded=@('horse-32-48-64-review.png','review7-menu-crops-1.png','review7-menu-crops-2.png','review7-menu-crops-3.png')
$images=New-Object Collections.Generic.List[object]
$runs=[ordered]@{};$identitySources=New-Object Collections.Generic.List[object]
$seen=New-Object 'Collections.Generic.HashSet[string]' ([StringComparer]::OrdinalIgnoreCase)
foreach($identityName in $identityFiles) {
    $identityPath=Join-Path $GalleryDirectory $identityName
    $rows=Read-GalleryJson $identityPath
    $identitySources.Add([ordered]@{file=$identityName;sha256=(Get-FileHash -LiteralPath $identityPath -Algorithm SHA256).Hash.ToLowerInvariant()})
    foreach($row in $rows) {
        $name=Get-GalleryOptional $row 'file';if(-not $name){$name=Get-GalleryOptional $row 'gallery'}
        if($name -cnotmatch '^[A-Za-z0-9][A-Za-z0-9_.-]*\.png$' -or -not $seen.Add($name) -or $row.sha256 -cnotmatch '^[0-9a-f]{64}$') {throw 'Invalid/duplicate gallery image identity.'}
        $source=[IO.Path]::GetFullPath($row.source);$runDirectory=Split-Path -Parent $source;$sourceName=Split-Path -Leaf $source
        if(-not $source.StartsWith($EvidenceRoot,[StringComparison]::OrdinalIgnoreCase) -or
            (Split-Path -Leaf $runDirectory) -cnotmatch '^\d{8}T\d+Z-disposable-expanded-summoning-cleanup2-native-review$' -or
            $sourceName -cnotmatch '^(?:[a-z0-9-]+-refine-[a-z0-9-]+|native-ui-\d+|cleanup2-(?:ettin|crab)-[a-zA-Z0-9_-]+)\.png$') {
            throw "Source is not an admitted original native framebuffer capture: $name"
        }
        $galleryPath=Join-Path $GalleryDirectory $name
        $sourceHash=(Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash.ToLowerInvariant()
        if($sourceHash -cne $row.sha256 -or (Get-FileHash -LiteralPath $galleryPath -Algorithm SHA256).Hash.ToLowerInvariant() -cne $sourceHash) {throw "Source/gallery identity mismatch: $name"}
        if(-not $runs.Contains($runDirectory)) {
            $qPath=Join-Path $runDirectory 'runtime-request.json';$rPath=Join-Path $runDirectory 'runtime-result.json';$idPath=Join-Path $runDirectory 'runtime-loaded-build-identity.json'
            $q=Read-GalleryJson $qPath;$r=Read-GalleryJson $rPath;$id=Read-GalleryJson $idPath
            if($q.runId -cne $r.runId -or $q.scenario -cne 'disposable-expanded-summoning-cleanup2-native-review' -or
                $r.scenario -cne $q.scenario -or $id.gitCommit -cne $r.gitCommit) {throw 'Native metadata does not describe one exact source-bound run.'}
            $fixture=New-Object Collections.Generic.List[object]
            foreach($a in $r.assertions) {
                if($a.name -cin @('summon-cleanup2-cleanup2-spellbook-fixture','summon-cleanup2-cleanup2-verified-human','summon-cleanup2-fixture-restored')) {
                    $fixture.Add([ordered]@{name=$a.name;status=$a.status;observed=($a.observed|ConvertFrom-Json)})
                }
            }
            $runs[$runDirectory]=[ordered]@{runId=$r.runId;evidenceDirectoryName=(Split-Path -Leaf $runDirectory);
                scenario=$r.scenario;reviewPhase=$q.parameters.reviewPhase;status=$r.status;sourceCommit=$r.gitCommit;
                modVersion=$r.loadedModVersion;dllSha256=$id.loadedModuleSha256;dllMvid=$id.moduleVersionId;
                gameVersion=$r.gameVersion;pass=@($r.assertions|Where-Object status -eq 'PASS').Count;
                fail=@($r.assertions|Where-Object status -eq 'FAIL').Count;startUtc=$r.startUtc;endUtc=$r.endUtc;
                failedAssertionNames=@($r.assertions|Where-Object status -eq 'FAIL'|ForEach-Object name);
                durationMilliseconds=$r.durationMilliseconds;fixtureKind='request-owned save-free disposable first-area fixture';
                fixtureObservations=@($fixture.ToArray());areaIdentity='Not separately attested by the selected metadata; source commit and phase bind the fixture.';
                requestSha256=(Get-FileHash -LiteralPath $qPath -Algorithm SHA256).Hash.ToLowerInvariant();
                resultSha256=(Get-FileHash -LiteralPath $rPath -Algorithm SHA256).Hash.ToLowerInvariant();
                loadedIdentitySha256=(Get-FileHash -LiteralPath $idPath -Algorithm SHA256).Hash.ToLowerInvariant()}
        }
        $label=@((Get-GalleryOptional $row 'key'),(Get-GalleryOptional $row 'pose'),(Get-GalleryOptional $row 'stage')) | Where-Object {$_}
        if(-not $label){$label=@($sourceName)}
        $images.Add([ordered]@{path=('images/'+$name);length=(Get-Item -LiteralPath $galleryPath).Length;sha256=$sourceHash;
            runId=$runs[$runDirectory].runId;sourcePngName=$sourceName;identityFile=$identityName;label=($label -join ' / ')})
    }
}
$unknown=@(Get-ChildItem -LiteralPath $GalleryDirectory -File -Filter '*.png' | Where-Object {-not $seen.Contains($_.Name) -and $_.Name -cnotin $excluded})
if($unknown.Count -gt 0) {throw 'Unclassified gallery PNGs exist; add explicit provenance before exporting them.'}
$limits=@('Screenshots are presentation evidence; no owner art approval, mechanics, release qualification, old-save hydration or current installed-stack compatibility is inferred.',
    'Each run retains its original source/DLL identity and PASS/FAIL. Older FAIL bodies/Ettin/Crab frames remain historical evidence; newer groups do not rewrite those verdicts.',
    'The fixture explicitly grants canonical spell access to a disposable caster; it does not prove natural class learning, Druid conversion, domain, item or saved hotbar access.',
    'Review 7 menus cover canonical SNA I-IX pages only. Review 9 tooltips cover five allowlisted live rows; the native HeaderItem is text-only (tooltipArtDisplayed=false). Painted art is on the visible source row.',
    'Visual scale targets, six-foot Human reference, camera differences and incomplete attack/body findings require their original structured observations; these PNGs alone do not calibrate native height.',
    'Only original byte-identical framebuffer PNGs are included. Three derived menu crop sheets and the Horse size/contact sheet are deliberately excluded.',
    'No saves, DLLs, native meshes/textures, credentials, raw logs or broad private observation graphs are included. Source/result metadata is a bounded scalar extract with hashes of the local originals.')
$manifest=[ordered]@{schemaVersion=1;kind='cleanup2-original-native-screenshots';createdUtc=[DateTime]::UtcNow.ToString('o');
    generator='scripts/Export-SummonCleanup2ReviewGallery.ps1';generatorSha256=(Get-FileHash -LiteralPath $PSCommandPath -Algorithm SHA256).Hash.ToLowerInvariant();
    limits=$limits;excludedDerivedImages=$excluded;identitySources=@($identitySources.ToArray());runs=@($runs.Values);images=@($images.ToArray())}
$readme="# Summon Cleanup 2 portable native gallery`n`nOpen index.html after extracting the entire ZIP. Images are original PNG bytes; HTML only scales their display. manifest.json maps every image to an exact run, source commit, loaded DLL SHA-256/MVID, phase and original result.`n`n"+
    (($limits | ForEach-Object {'- '+$_}) -join "`n")+"`n`nVerify without the game or original machine:`n`n    powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Export-SummonCleanup2ReviewGallery.ps1 -Mode Verify -OutputPath <packet.zip>`n"
$html=New-Object Text.StringBuilder
[void]$html.Append('<!doctype html><html><head><meta charset="utf-8"><title>Summon Cleanup 2 native gallery</title><style>body{font:16px system-ui;background:#17191d;color:#eee;margin:24px}a{color:#9dd3ff}img{max-width:100%;height:auto}article{display:inline-block;vertical-align:top;width:460px;max-width:100%;margin:8px}code{overflow-wrap:anywhere}section{border-top:2px solid #777;margin-top:28px}summary{cursor:pointer}.FAIL{color:#ffad99}.PASS{color:#a4e7ad}</style></head><body><h1>Summon Cleanup 2 native gallery</h1><p><a href="manifest.json">Exact image/run identities</a> | <a href="README.md">Scope and verification</a></p><ul>')
foreach($limit in $limits){[void]$html.Append('<li>'+[Net.WebUtility]::HtmlEncode($limit)+'</li>')}
[void]$html.Append('</ul>')
foreach($run in $runs.Values) {
    [void]$html.Append('<section><h2>'+[Net.WebUtility]::HtmlEncode($run.reviewPhase)+' — <span class="'+$run.status+'">'+$run.status+'</span></h2><p>Run <code>'+$run.runId+'</code><br>Source <code>'+$run.sourceCommit+'</code><br>DLL <code>'+$run.dllSha256+'</code><br>MVID <code>'+$run.dllMvid+'</code><br>'+$run.pass+' PASS / '+$run.fail+' FAIL. Fixture: '+$run.fixtureKind+'.</p>')
    foreach($image in $images | Where-Object {$_.runId -ceq $run.runId}) {
        $label=[Net.WebUtility]::HtmlEncode($image.label)
        [void]$html.Append('<article><a href="'+$image.path+'"><img loading="lazy" src="'+$image.path+'" alt="'+$label+'"></a><p>'+$label+'</p></article>')
    }
    [void]$html.Append('</section>')
}
[void]$html.Append('</body></html>')
$manifest.documents=@([ordered]@{path='index.html';sha256=(Get-GalleryTextHash $html.ToString())},
    [ordered]@{path='README.md';sha256=(Get-GalleryTextHash $readme)})
[void][IO.Directory]::CreateDirectory((Split-Path -Parent $OutputPath))
$archive=[IO.Compression.ZipFile]::Open($OutputPath,[IO.Compression.ZipArchiveMode]::Create)
try {
    Write-GalleryText $archive 'manifest.json' ($manifest|ConvertTo-Json -Depth 15)
    Write-GalleryText $archive 'README.md' $readme
    Write-GalleryText $archive 'index.html' $html.ToString()
    foreach($image in $images) {[void][IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive,(Join-Path $GalleryDirectory (Split-Path -Leaf $image.path)),$image.path,[IO.Compression.CompressionLevel]::Optimal)}
} finally {$archive.Dispose()}
Test-GalleryArchive $OutputPath
