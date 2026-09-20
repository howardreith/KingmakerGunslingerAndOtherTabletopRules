# Shared prologue for the Favored Class persistence finalization
# regressions: extracts the real driver seams and the real protected-catalog
# assertion/disposal, and defines the scenario fixtures. Each scenario file
# and this prologue are intentionally small, separate units.
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$script:repositoryRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$tokens = $null; $errors = $null
$driverAst = [Management.Automation.Language.Parser]::ParseInput(
    [IO.File]::ReadAllText((Join-Path $script:repositoryRoot 'scripts\Invoke-WordOfRecallFavoredClassPersistence.ps1')), [ref]$tokens, [ref]$errors)
if ($errors.Count -ne 0) { throw 'Favored Class persistence orchestrator has syntax errors.' }
foreach ($name in @('Remove-PersistenceOwnedSave', 'Invoke-FinalizationStages')) {
    $function = @($driverAst.FindAll({ param($node) $node -is [Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -ceq $name }, $true))
    if ($function.Count -ne 1) { throw "Expected the exact persistence seam: $name" }
    . ([scriptblock]::Create($function[0].Extent.Text))
}
$commonAst = [Management.Automation.Language.Parser]::ParseInput(
    [IO.File]::ReadAllText((Join-Path $script:repositoryRoot 'scripts\TeleportationPersistence.Common.ps1')), [ref]$tokens, [ref]$errors)
if ($errors.Count -ne 0) { throw 'Persistence common helpers have syntax errors.' }
foreach ($name in @('Assert-KmgProtectedSaveCatalog', 'Close-KmgProtectedSaveCatalog')) {
    $function = @($commonAst.FindAll({ param($node) $node -is [Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -ceq $name }, $true))
    if ($function.Count -ne 1) { throw "Expected the exact common seam: $name" }
    . ([scriptblock]::Create($function[0].Extent.Text))
}
$script:checks = 0
$script:sessionRoot = Join-Path (Join-Path $script:repositoryRoot 'artifacts\tests') ('fcb-finalize-' + [Guid]::NewGuid().ToString('N'))
function New-ScenarioRoot([string]$name) {
    $scenario = Join-Path $script:sessionRoot $name
    [void](New-Item -ItemType Directory -Path (Join-Path $scenario 'saves'))
    [void](New-Item -ItemType Directory -Path (Join-Path $scenario 'tx'))
    return $scenario
}
function New-RealCatalog([string]$scenario) {
    $saveDir = Join-Path $scenario 'saves'
    $workingPath = Join-Path $saveDir 'Manual_299_KMG_AUTOMATION_WORKING.zks'
    [IO.File]::WriteAllText($workingPath, 'pre-existing protected campaign')
    $item = Get-Item -LiteralPath $workingPath
    $sha = [Security.Cryptography.SHA256]::Create()
    try { $hash = [BitConverter]::ToString($sha.ComputeHash([IO.File]::ReadAllBytes($workingPath))).Replace('-', '').ToLowerInvariant() }
    finally { $sha.Dispose() }
    $leases = New-Object 'System.Collections.Generic.List[System.IO.FileStream]'
    $leases.Add([IO.File]::Open($workingPath, [IO.FileMode]::Open, [IO.FileAccess]::Read, [IO.FileShare]::Read))
    return [pscustomobject]@{
        Directory = $saveDir
        EvidenceDirectory = (Join-Path $scenario 'tx')
        Leases = $leases
        Files = @([ordered]@{ path = $workingPath; length = $item.Length; sha256 = $hash
            creationUtcTicks = $item.CreationTimeUtc.Ticks; writeUtcTicks = $item.LastWriteTimeUtc.Ticks
            attributes = [int]$item.Attributes }) }
}
function New-OwnedSave([string]$scenario, [string]$content) {
    $name = 'KMG_FCB_PERSISTENCE_20260920T1224343386995Z_cb0cf838be0c4e4d889c27b91cbfe644_prepare'
    $path = Join-Path (Join-Path $scenario 'saves') ('Manual_88_' + $name + '.zks')
    [IO.File]::WriteAllText($path, $content)
    $hash = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()
    return [ordered]@{ name = $name; path = $path; phase = 'prepare'; runId = 'exact-test-run'
        receipt = (Join-Path $scenario 'tx\owned.json')
        completedSha256 = $hash; createdSha256 = $hash
        TransactionDirectory = (Join-Path $scenario 'tx') }
}
$script:state = @{}
function Wait-PersistenceExit {
    if ($script:state.waitExitThrows) { throw 'Guarded game process did not terminate' } }
function Restore-PersistenceSettings { $script:state.settings = $script:state.settings + 1 }
function Restore-PersistenceSidecars {
    $script:state.sidecars = $script:state.sidecars + 1
    if ($script:state.sidecarThrows) { throw 'sidecar restoration failed' } }
$script:modsInventory = @(
    [ordered]@{ path = 'KingmakerGunslinger/FeatureModules.json'; directory = $false }
    [ordered]@{ path = 'KingmakerGunslinger/Info.json'; directory = $false })
$script:modsJson = ($script:modsInventory | ConvertTo-Json -Depth 10 -Compress)
function Get-PersistenceModsInventory { return $script:modsInventory }
function Write-PersistenceEvidence {
    param($name, $value)
    Set-Content -LiteralPath (Join-Path $script:state.txDir $name) -Value (ConvertTo-Json -InputObject $value -Depth 100) }
function Reset-Stubs([bool]$waitExitThrows = $false, [bool]$sidecarThrows = $false) {
    $script:state = @{ settings = 0; sidecars = 0; txDir = $script:state.txDir
        waitExitThrows = $waitExitThrows; sidecarThrows = $sidecarThrows } }
function Assert-LeasesReleased($catalog, [string]$scenarioName) {
    try {
        $stream = [IO.File]::Open([string]$catalog.Files[0].path, [IO.FileMode]::Open, [IO.FileAccess]::Read, [IO.FileShare]::None)
        $stream.Dispose()
    } catch { throw "$scenarioName`: catalog leases were not disposed." }
}
function Read-FinalRecord([string]$txDir) {
    return (Get-Content -LiteralPath (Join-Path $txDir 'transaction-result.json') -Raw | ConvertFrom-Json) }
function Read-StageStates($record) {
    $states = @{}
    foreach ($outcome in $record.stageOutcomes) { $states[$outcome.stage] = [string]$outcome.state }
    return $states }
$script:runs = New-Object 'System.Collections.Generic.List[object]'
[void]$script:runs.Add([ordered]@{ phase = 'prepare' })
[void]$script:runs.Add([ordered]@{ phase = 'verify' })
