[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$tokens = $null; $errors = $null
$scriptPath = Join-Path $PSScriptRoot 'Invoke-TeleportationPersistenceQualification.ps1'
$ast = [Management.Automation.Language.Parser]::ParseInput(([IO.File]::ReadAllText($scriptPath) + [Environment]::NewLine + [IO.File]::ReadAllText((Join-Path $PSScriptRoot 'TeleportationPersistence.Common.ps1'))), [ref]$tokens, [ref]$errors)
if ($errors.Count -ne 0) { throw 'Persistence orchestrator has syntax errors.' }
foreach ($name in @('Restore-PersistenceSettings', 'Get-PersistenceModsInventory', 'Register-PersistenceOwnedSave', 'Restore-PersistenceSidecars')) {
    $function = @($ast.FindAll({ param($node) $node -is [Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -ceq $name }, $true))
    if ($function.Count -ne 1) { throw 'Expected exact persistence transaction seam.' }
    . ([scriptblock]::Create($function[0].Extent.Text))
}
$testRoot = Join-Path (Split-Path -Parent $PSScriptRoot) ('artifacts\tests\teleport-persistence-' + [Guid]::NewGuid().ToString('N'))
[void](New-Item -ItemType Directory -Path $testRoot)
$modsRoot = Join-Path $testRoot 'Mods'
[void](New-Item -ItemType Directory -Path $modsRoot)
[void](New-Item -ItemType Directory -Path (Join-Path $modsRoot 'KingmakerGunslinger'))
$fixtureKmgDirectory = (Resolve-Path -LiteralPath (Join-Path $modsRoot 'KingmakerGunslinger')).Path
$fixtureAllowedRoot = [IO.Path]::GetFullPath((Join-Path (Split-Path -Parent $PSScriptRoot) 'artifacts\tests')) + '\'
if (-not $fixtureKmgDirectory.StartsWith($fixtureAllowedRoot, [StringComparison]::OrdinalIgnoreCase)) { throw 'Fixture path is outside repository artifacts/tests.' }
$settingsPath = Join-Path $fixtureKmgDirectory 'FeatureModules.json'
[IO.File]::WriteAllText($settingsPath, "{`r`n  `"schemaVersion`": 11, `"teleportation-spells`": true, `"unrelated`": false`r`n}")
[IO.File]::WriteAllText((Join-Path $modsRoot 'Foreign.txt'), 'foreign content')
$settingsBytes = [IO.File]::ReadAllBytes($settingsPath)
$item = Get-Item -LiteralPath $settingsPath
$settingsTimes = @($item.CreationTimeUtc, $item.LastWriteTimeUtc, $item.Attributes)
$tx = '20260908T2130001234567Z_0123456789abcdef0123456789abcdef'
$catalog = [pscustomobject]@{ Directory = $testRoot; Files = @([pscustomobject]@{ path = (Join-Path $testRoot 'Manual_299_KMG_AUTOMATION_WORKING.zks') }) }
$owned = New-Object 'System.Collections.Generic.List[object]'
function Write-PersistenceEvidence([string]$name, $value) { }
$checks = 0
try {
    $before = Get-PersistenceModsInventory | ConvertTo-Json -Depth 10 -Compress
    [IO.File]::WriteAllText($settingsPath, '{"schemaVersion":11,"teleportation-spells":false,"unrelated":false}')
    Restore-PersistenceSettings
    if ($before -cne (Get-PersistenceModsInventory | ConvertTo-Json -Depth 10 -Compress)) { throw 'Settings restoration changed the complete fixture Mods tree.' }
    $checks++
    $name = 'KMG_TELEPORT_PERSISTENCE_' + $tx + '_A'
    $receipt = [ordered]@{ transactionId = $tx; phase = 'A'; name = $name; file = 'Manual_301_' + $name + '.zks'
        path = (Join-Path $testRoot ('Manual_301_' + $name + '.zks')); runId = 'exact-test-run'
        existedBeforePreparation = $false; lifecycle = 'native-prepared-before-write' }
    $receiptPath = Join-Path $testRoot 'teleportation-persistence-owned-save.json'
    foreach ($bad in @($catalog.Files[0].path, (Join-Path $modsRoot ('Manual_301_' + $name + '.zks')))) {
        $trial = [ordered]@{}
        foreach ($key in $receipt.Keys) { $trial[$key] = $receipt[$key] }
        $trial.path = $bad
        $trial | ConvertTo-Json | Set-Content -LiteralPath $receiptPath
        $rejected = $false
        try { Register-PersistenceOwnedSave $testRoot } catch { $rejected = $true }
        if (-not $rejected -or $owned.Count -ne 0) { throw 'Unowned save path was accepted.' }
        $checks++
    }
    $receipt | ConvertTo-Json | Set-Content -LiteralPath $receiptPath
    Register-PersistenceOwnedSave $testRoot
    if ($owned.Count -ne 1 -or $owned[0].path -cne $receipt.path) { throw 'Exact new native save receipt did not establish ownership.' }
    $checks++
    $rejected = $false
    try { Register-PersistenceOwnedSave $testRoot } catch { $rejected = $true }
    if (-not $rejected -or $owned.Count -ne 1) { throw 'Duplicate native save receipt was accepted.' }
    $checks++
    $settingsOriginal = [Text.Encoding]::UTF8.GetString($settingsBytes) | ConvertFrom-Json
    $previousPath = $settingsPath + '.previous'; $previousBytes = $null; $previousTimes = $null
    $fixturePayloadPath = Join-Path $testRoot 'expected-payload.bin'
    [IO.File]::WriteAllText($fixturePayloadPath, 'harmless transaction fixture')
    $dllSha = (Get-FileHash -LiteralPath $fixturePayloadPath -Algorithm SHA256).Hash.ToLowerInvariant()
    $modsBefore = @(Get-PersistenceModsInventory)
    $cachePath = Join-Path $fixtureKmgDirectory 'KingmakerGunslinger.dll.12345.cache'
    Copy-Item -LiteralPath $fixturePayloadPath -Destination $cachePath
    [IO.File]::WriteAllText($previousPath, '{"schemaVersion":11,"teleportation-spells":false,"unrelated":false}')
    Restore-PersistenceSidecars
    if ((Test-Path -LiteralPath $cachePath) -or (Test-Path -LiteralPath $previousPath) -or
        ($modsBefore | ConvertTo-Json -Depth 10 -Compress) -cne (Get-PersistenceModsInventory | ConvertTo-Json -Depth 10 -Compress)) {
        throw 'Owned cache/backup cleanup did not restore the exact original tree.'
    }
    $checks++
    [IO.File]::WriteAllText($previousPath, 'historical backup bytes remain uninterpreted')
    $previousBytes = [IO.File]::ReadAllBytes($previousPath)
    $old = Get-Item -LiteralPath $previousPath; $previousTimes = @($old.CreationTimeUtc, $old.LastWriteTimeUtc, $old.Attributes)
    [IO.File]::WriteAllText($previousPath, '{"schemaVersion":11,"teleportation-spells":false,"unrelated":false}')
    Restore-PersistenceSidecars
    if ([Convert]::ToBase64String([IO.File]::ReadAllBytes($previousPath)) -cne [Convert]::ToBase64String($previousBytes)) { throw 'Existing sidecar was not preserved byte for byte.' }
    $checks++
    [IO.File]::WriteAllText($cachePath, 'different fixture data')
    $rejected = $false
    try { Restore-PersistenceSidecars } catch { $rejected = $true }
    if (-not $rejected -or -not (Test-Path -LiteralPath $cachePath)) { throw 'Unproven cache content was deleted.' }
    Remove-Item -LiteralPath $cachePath
    $checks++
    [IO.File]::WriteAllText($previousPath, '{"schemaVersion":11,"teleportation-spells":false,"unrelated":true}')
    $rejected = $false
    try { Restore-PersistenceSidecars } catch { $rejected = $true }
    if (-not $rejected) { throw 'Unrelated sidecar mutation was accepted.' }
    $checks++
    $exactMatrix = '{"schemaVersion":11,"teleportation-spells":false,"unrelated":true}' | ConvertFrom-Json
    Restore-PersistenceSidecars -AuthorizedSettingsStates @($exactMatrix)
    if ([Convert]::ToBase64String([IO.File]::ReadAllBytes($previousPath)) -cne [Convert]::ToBase64String($previousBytes)) {
        throw 'An exact explicitly authorized matrix state did not restore the original backup bytes.'
    }
    $checks++
    [IO.File]::WriteAllText($previousPath, '{"schemaVersion":11,"teleportation-spells":true,"unrelated":true}')
    $rejected = $false
    try { Restore-PersistenceSidecars -AuthorizedSettingsStates @($exactMatrix) } catch { $rejected = $true }
    if (-not $rejected) { throw 'An unlisted matrix tuple was accepted.' }
    $checks++
} finally {
    $resolved = (Resolve-Path -LiteralPath $testRoot).Path
    $allowed = [IO.Path]::GetFullPath((Join-Path (Split-Path -Parent $PSScriptRoot) 'artifacts\tests')) + '\'
    if (-not $resolved.StartsWith($allowed, [StringComparison]::OrdinalIgnoreCase)) { throw 'Fixture cleanup escaped artifacts/tests.' }
    Remove-Item -LiteralPath $resolved -Recurse -Force
}
Write-Host "Persistence settings/ownership transaction tests passed: $checks"
