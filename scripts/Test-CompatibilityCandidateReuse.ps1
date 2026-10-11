<# Offline orchestration test. Runs the actual matrix/profile wrappers and the
   actual runtime artifact-selection branch in a bounded filesystem. Compiler,
   deployment and game boundaries are fixtures, never live qualification. #>
[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$boundary = [IO.Path]::GetFullPath((Join-Path ([IO.Path]::GetTempPath()) ('kmg-candidate-reuse-' + [Guid]::NewGuid().ToString('N'))))
$scripts = Join-Path $boundary 'scripts'
$install = Join-Path $boundary 'game'
$state = Join-Path $boundary 'state'
$evidence = Join-Path $boundary 'evidence'
$oldFailure = $env:KMG_FIXTURE_FAIL_SCENARIO
$oldMismatch = $env:KMG_FIXTURE_MISMATCH
$oldFixtureRoot = $env:KMG_FIXTURE_ROOT
function Write-Fixture([string]$Name, [string]$Text) {
    $path = Join-Path $scripts $Name
    New-Item -ItemType Directory -Path (Split-Path $path) -Force | Out-Null
    [IO.File]::WriteAllText($path, $Text)
}
function Read-Trace { @(Get-Content -LiteralPath (Join-Path $boundary 'trace.jsonl') | ForEach-Object { $_ | ConvertFrom-Json }) }
try {
    $env:KMG_FIXTURE_FAIL_SCENARIO = $null
    $env:KMG_FIXTURE_MISMATCH = $null
    New-Item -ItemType Directory -Path $scripts,$state,$evidence,(Join-Path $install 'Mods\KingmakerGunslinger'),(Join-Path $install 'Mods\CallOfTheWild') -Force | Out-Null
    '{"Id":"KingmakerGunslinger","Version":"0.0.150"}' | Set-Content (Join-Path $boundary 'Info.json')
    Copy-Item (Join-Path $boundary 'Info.json') (Join-Path $install 'Mods\KingmakerGunslinger\Info.json')
    'original human DLL' | Set-Content (Join-Path $install 'Mods\KingmakerGunslinger\KingmakerGunslinger.dll')
    '{"original":"feature settings"}' | Set-Content (Join-Path $install 'Mods\KingmakerGunslinger\FeatureModules.json')
    '{"balance_fixes":true}' | Set-Content (Join-Path $install 'Mods\CallOfTheWild\settings.json')
    foreach ($name in @('Invoke-ExpandedSummoningCompatibilityMatrix.ps1','compatibility\Invoke-KingmakerCompatibilityProfile.ps1')) {
        $text = [IO.File]::ReadAllText((Join-Path $PSScriptRoot $name))
        $text = $text.Replace('C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker', $install).
            Replace('C:\Dev\KingmakerGunslingerLab\runtime-evidence', $evidence).
            Replace('C:\Dev\KingmakerGunslingerLab\compatibility-state', $state)
        Write-Fixture $name $text
    }
    Copy-Item (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1') $scripts
    Write-Fixture 'RuntimeHarness.Common.ps1' @'
$env:KMG_FIXTURE_ROOT = Split-Path -Parent $PSScriptRoot
function Trace-Fixture($event, $fields = @{}) {
    $fields.event = $event
    Add-Content (Join-Path $env:KMG_FIXTURE_ROOT 'trace.jsonl') ($fields | ConvertTo-Json -Compress)
}
function Read-KmgCompatibilityJson($path) { Get-Content -Raw -LiteralPath $path | ConvertFrom-Json }
function Get-KmgCompatibilityDefaultPackage($root) { Join-Path $root 'artifacts\local-runtime\0.0.150\KingmakerGunslinger-0.0.150-local-runtime.zip' }
function Get-KmgCompatibilityPackageVersion($path) { '0.0.150' }
function Get-KmgGitState($RepositoryRoot) { [pscustomobject]@{Commit='fixture-product-commit';Status=@()} }
function Get-KmgSourceStateFingerprint($RepositoryRoot) { if ($env:KMG_FIXTURE_MISMATCH) { 'changed' } else { 'fixture-source' } }
function Read-KmgBuildLocalManifest($PackagePath, $RepositoryRoot) {
    $record = Read-KmgCompatibilityJson ($PackagePath + '.manifest.json')
    if ((Get-FileHash $PackagePath).Hash -cne $record.packageSha256) { throw 'Fixture package was changed.' }
    $record
}
function Get-Process { param($Name,$ErrorAction) }
function Open-KmgCompatibilityRuntimeLease($runId,$StateRoot) {
    $stream = [IO.File]::Open((Join-Path $StateRoot "$runId\lease"), 'Create', 'ReadWrite', 'None')
    [pscustomobject]@{ Stream=$stream; RunId=$runId }
}
function Assert-KmgReusableDeployment($DeploymentManifestPath,$PackagePath,$RepositoryRoot,$ExpectedVersion,$AllowDirtyGit) {
    $receipt = Read-KmgCompatibilityJson $DeploymentManifestPath
    $active = Read-KmgCompatibilityJson (Join-Path $env:KMG_FIXTURE_ROOT 'state\active.json')
    if ($receipt.runId -cne $active.runId -or $receipt.packageSha256 -cne (Get-FileHash $PackagePath).Hash) {
        throw 'Stale profile receipt or changed package.'
    }
    Trace-Fixture 'reuse' @{receipt=$DeploymentManifestPath;package=$PackagePath;runId=$active.runId;profile=$active.profile}
    [pscustomobject]@{PackagePath=$PackagePath;DeploymentManifestPath=$DeploymentManifestPath}
}
'@
    Write-Fixture 'compatibility\CompatibilityProfile.Common.ps1' ''
    Write-Fixture 'RuntimeCoordination.Common.ps1' ''
    Write-Fixture 'Build-Local.ps1' @'
$root = Split-Path -Parent $PSScriptRoot
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')
$package = Get-KmgCompatibilityDefaultPackage $root
New-Item -ItemType Directory -Path (Split-Path $package) -Force | Out-Null
'immutable fixture candidate' | Set-Content $package
@{commit='fixture-product-commit';sourceStateSha256='fixture-source';packageSha256=(Get-FileHash $package).Hash} |
    ConvertTo-Json | Set-Content ($package + '.manifest.json')
Trace-Fixture 'build' @{package=$package}
'@
    Write-Fixture 'Deploy-Local.ps1' @'
[CmdletBinding(SupportsShouldProcess=$true)]
param($PackagePath,$LiveModDirectory,$RuntimeLease,[switch]$PassThru)
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')
if (-not $RuntimeLease.Stream.CanRead) { throw 'Deployment requires the active transaction lease.' }
$active = Read-KmgCompatibilityJson (Join-Path $env:KMG_FIXTURE_ROOT 'state\active.json')
if ($active.runId -cne $RuntimeLease.RunId) { throw 'Wrong deployment lease.' }
if ($LiveModDirectory -cne (Join-Path $env:KMG_FIXTURE_ROOT 'game\Mods\KingmakerGunslinger')) { throw 'Wrong deployment target.' }
if (-not $PSCmdlet.ShouldProcess('bounded fixture','deploy')) { Trace-Fixture 'deploy-preflight'; return }
$path = Join-Path $env:KMG_FIXTURE_ROOT ('state\' + $active.runId + '\deployment.json')
@{runId=$active.runId;packageSha256=(Get-FileHash $PackagePath).Hash} | ConvertTo-Json | Set-Content $path
'candidate DLL' | Set-Content (Join-Path $env:KMG_FIXTURE_ROOT 'game\Mods\KingmakerGunslinger\KingmakerGunslinger.dll')
Trace-Fixture 'deploy' @{receipt=$path;package=$PackagePath;runId=$active.runId}
if ($PassThru) { $path }
'@
    Write-Fixture 'compatibility\Enter-KingmakerCompatibilityProfile.ps1' @'
[CmdletBinding(SupportsShouldProcess=$true)]
param($ProfileId,$RunId,$KingmakerInstallDir,$StateRoot,$ReferenceRoot,$PackagePath)
$recordRoot = Join-Path $StateRoot $RunId
New-Item -ItemType Directory -Path $recordRoot | Out-Null
Move-Item -LiteralPath (Join-Path $KingmakerInstallDir 'Mods') -Destination (Join-Path $recordRoot 'originalMods')
Copy-Item -LiteralPath (Join-Path $recordRoot 'originalMods') -Destination (Join-Path $KingmakerInstallDir 'Mods') -Recurse
@{runId=$RunId;profile=$ProfileId;install=$KingmakerInstallDir} | ConvertTo-Json | Set-Content (Join-Path $StateRoot 'active.json')
Trace-Fixture 'enter' @{runId=$RunId;profile=$ProfileId}
'@
    Write-Fixture 'compatibility\Restore-KingmakerCompatibilityProfile.ps1' @'
[CmdletBinding(SupportsShouldProcess=$true)]
param($RunId,$StateRoot)
$active = Read-KmgCompatibilityJson (Join-Path $StateRoot 'active.json')
if ($active.runId -cne $RunId) { throw 'Wrong restoration target.' }
$recordRoot = Join-Path $StateRoot $RunId
# Exclusive open proves the actual wrapper disposed the profile lease first.
$check = [IO.File]::Open((Join-Path $recordRoot 'lease'), 'Open', 'ReadWrite', 'None'); $check.Dispose()
$mods = [IO.Path]::GetFullPath((Join-Path $active.install 'Mods'))
if (-not $mods.StartsWith($env:KMG_FIXTURE_ROOT + '\', [StringComparison]::OrdinalIgnoreCase)) { throw 'Unsafe fixture restoration.' }
Remove-Item -LiteralPath $mods -Recurse -Force
Move-Item -LiteralPath (Join-Path $recordRoot 'originalMods') -Destination $mods
@{status='Restored';restorationVerified=$true;stagedMutationObserved=$true} | ConvertTo-Json | Set-Content (Join-Path $recordRoot 'transaction.json')
Remove-Item -LiteralPath (Join-Path $StateRoot 'active.json')
Trace-Fixture 'restore' @{runId=$RunId}
'@
    Write-Fixture 'compatibility\Collect-KmgCompatibilityAttributionLog.ps1' @'
param($EvidenceDirectory,$ConfigurationId,$KingmakerInstallDir)
$path = Join-Path $EvidenceDirectory 'fixture-log-summary.json'
'offline fixture only' | Set-Content $path
[pscustomobject]@{rawLogSha256='fixture';summaryPath=$path;summarySha256=(Get-FileHash $path).Hash}
'@
    # Extract the executed branch from current runtime code, including its
    # Build-Local fallback. Do not replace it with a claimed list of commands.
    $tokens = $null; $errors = $null
    $runtimeAst = [Management.Automation.Language.Parser]::ParseFile((Join-Path $PSScriptRoot 'Invoke-KingmakerRuntimeTest.ps1'), [ref]$tokens, [ref]$errors)
    $branches = @($runtimeAst.FindAll({param($node) $node -is [Management.Automation.Language.IfStatementAst] -and $node.Clauses[0].Item1.Extent.Text -ceq '$qualifiedProducerReuse'}, $true))
    if ($errors.Count -or $branches.Count -ne 1) { throw 'Could not resolve the actual runtime artifact selection.' }
    $runtimeFixture = @'
[CmdletBinding(SupportsShouldProcess=$true)]
param($Scenario,$ExpectedVersion,$TimeoutSeconds,$ObserverStartupTimeoutSeconds,$CompletionTimeoutSeconds,$CompatibilityProfileId,$Parameters,$SaveName,$RuntimeLease,[switch]$ReuseInstalledArtifact,[switch]$AllowDirtyGit,[bool]$ExitAfterCompletion,$DeploymentManifestPath,$PackagePath)
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')
$root = Split-Path -Parent $PSScriptRoot
$qualifiedProducerReuse = $false
'@ + "`r`n" + $branches[0].Extent.Text + "`r`n" + @'
if ($env:KMG_FIXTURE_FAIL_SCENARIO -ceq $Scenario) { throw 'Injected native boundary failure.' }
$resultRoot = Join-Path $env:KMG_FIXTURE_ROOT ('evidence\' + [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffffffZ') + '-' + $Scenario)
New-Item -ItemType Directory -Path $resultRoot | Out-Null
@{scenario=$Scenario;status='PASS';runId=[IO.Path]::GetFileName($resultRoot)} | ConvertTo-Json | Set-Content (Join-Path $resultRoot 'runtime-result.json')
'@
    Write-Fixture 'Invoke-KingmakerRuntimeTest.ps1' $runtimeFixture
    $before = @(Get-ChildItem (Join-Path $install 'Mods') -Recurse -File | ForEach-Object { $_.FullName + ':' + (Get-FileHash $_.FullName).Hash }) -join '|'
    $matrix = Join-Path $scripts 'Invoke-ExpandedSummoningCompatibilityMatrix.ps1'
    foreach ($invalid in @(@{ ReuseCandidate=$true }, @{ PackagePath='ignored-input.zip' })) {
        $refused = $false
        try { & $matrix @invalid | Out-Null } catch { $refused = $true }
        if (-not $refused -or (Test-Path (Join-Path $boundary 'trace.jsonl')) -or
            @(Get-ChildItem -LiteralPath $evidence).Count -ne 0) { throw 'Invalid matrix candidate arguments were not refused before side effects.' }
    }
    & $matrix | Out-Null
    $trace = Read-Trace
    $report = Get-ChildItem (Join-Path $evidence 'expanded-summoning-compatibility') -File | Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1 | Get-Content -Raw | ConvertFrom-Json
    $scenarioCount = @($report.steps | ForEach-Object { $_.scenarios } | ForEach-Object { $_ }).Count
    $deploys = @($trace | Where-Object event -eq 'deploy')
    $reuses = @($trace | Where-Object event -eq 'reuse')
    if (@($trace | Where-Object event -eq 'build').Count -ne 1 -or $deploys.Count -ne $report.steps.Count -or $reuses.Count -ne $scenarioCount) { throw 'Nested build/deployment/scenario count mismatch.' }
    if (@($report.steps | Where-Object outcome -ne 'COMPLETED').Count -or -not $report.liveTreeRestored) { throw 'The bounded matrix failed or did not restore.' }
    if (@($deploys.receipt | Sort-Object -Unique).Count -ne $deploys.Count -or @($reuses.package | Sort-Object -Unique).Count -ne 1) { throw 'Candidate/receipt identity was not preserved.' }
    foreach ($deployment in $deploys) {
        $profileReuses = @($reuses | Where-Object runId -eq $deployment.runId)
        if (-not $profileReuses.Count -or @($profileReuses | Where-Object receipt -cne $deployment.receipt).Count) { throw 'A scenario reused another profile receipt.' }
    }
    $package = $deploys[0].package
    $launcher = Join-Path $scripts 'compatibility\Invoke-KingmakerCompatibilityProfile.ps1'
    $refused = $false
    try { & $launcher -ProfileId gunslinger-only -Scenario mod-load-smoke -ReuseInstalledArtifact -PackagePath $package -DeploymentManifestPath $deploys[0].receipt -Confirm:$false | Out-Null } catch { $refused = $_.Exception.Message -like '*fresh deployment record*' }
    if (-not $refused -or (Test-Path (Join-Path $state 'active.json'))) { throw 'Stale external receipt was not refused before entry.' }
    $env:KMG_FIXTURE_MISMATCH = 'yes'; $refused = $false
    try { & $launcher -ProfileId gunslinger-only -Scenario mod-load-smoke -ReuseInstalledArtifact -PackagePath $package -Confirm:$false | Out-Null } catch { $refused = $_.Exception.Message -like '*unchanged product worktree*' }
    if (-not $refused -or (Test-Path (Join-Path $state 'active.json'))) { throw 'Changed candidate was not refused before entry.' }
    Remove-Item Env:\KMG_FIXTURE_MISMATCH
    $env:KMG_FIXTURE_FAIL_SCENARIO = 'mod-load-smoke'; $refused = $false
    try { & $launcher -ProfileId gunslinger-only -Scenario mod-load-smoke -ReuseInstalledArtifact -PackagePath $package -Confirm:$false | Out-Null } catch { $refused = $_.Exception.Message -like '*Injected native boundary failure*' }
    $after = @(Get-ChildItem (Join-Path $install 'Mods') -Recurse -File | ForEach-Object { $_.FullName + ':' + (Get-FileHash $_.FullName).Hash }) -join '|'
    if (-not $refused -or $after -cne $before -or (Test-Path (Join-Path $state 'active.json'))) { throw 'Failure did not restore the bounded original bytes.' }
    Write-Host "Compatibility orchestration: PASS=6 FAIL=0; observed candidate builds=1 profile deployments=$($deploys.Count) runtime reuse calls=$($reuses.Count). No live/native qualification."
} finally {
    $env:KMG_FIXTURE_FAIL_SCENARIO = $oldFailure
    $env:KMG_FIXTURE_MISMATCH = $oldMismatch
    $env:KMG_FIXTURE_ROOT = $oldFixtureRoot
    $temporaryRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('\') + '\'
    if (-not $boundary.StartsWith($temporaryRoot, [StringComparison]::OrdinalIgnoreCase)) { throw 'Unsafe fixture cleanup path.' }
    Remove-Item -LiteralPath $boundary -Recurse -Force
}
