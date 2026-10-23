<# One explicit save-free Cleanup2 observation on a frozen development artifact.
   The profile transaction owns staging/restoration; this same original host
   retains the complete protected-save catalog until confirmed game exit. #>
[CmdletBinding(SupportsShouldProcess=$true,ConfirmImpact='High')]
param(
    [Parameter(Mandatory=$true)][string]$PackagePath,
    [Parameter(Mandatory=$true)][ValidateSet('0.0.163')][string]$ExpectedVersion,
    [Parameter(Mandatory=$true)][ValidateSet('production-attacks','ettin-lifecycle','ettin-commands','elementals-presentation','elementals-console','elementals-sources','elementals-learning','elementals-off','menu-adapters','giant-lineup','giants-off','scale-prototype','shared-controls','gorgon-presentation','recovery-bodies','creature-commands','alternate-console')][string]$Phase,
    [switch]$WithAlternateSources
)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
if($WithAlternateSources -and $Phase -cnotin @('menu-adapters','elementals-off','alternate-console')) {
    throw 'Alternate-source profile is restricted to exact menu-adapters, elementals-off or alternate-console review.'
}
if($Phase -ceq 'alternate-console' -and -not $WithAlternateSources) {
    throw 'The alternate-console review requires the fixed alternate-source profile.'
}
$profile=if($WithAlternateSources){'gunslinger-call-of-the-wild'}else{'gunslinger-only'}
$runtimeSeconds=if($WithAlternateSources -and $Phase -ceq 'menu-adapters'){600}else{450}
$root=Split-Path -Parent $PSScriptRoot
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')
. (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')
. (Join-Path $PSScriptRoot 'ExpandedSummoningOrchestration.Common.ps1')
. (Join-Path $PSScriptRoot 'TeleportationPersistence.Common.ps1')
. (Join-Path $PSScriptRoot 'SummonCleanup2CurrentStack.Common.ps1')
if($Phase -cnotin @('production-attacks','ettin-lifecycle','ettin-commands','elementals-presentation','elementals-console','elementals-sources','elementals-learning','elementals-off','menu-adapters','giant-lineup','giants-off','scale-prototype','shared-controls','gorgon-presentation','recovery-bodies','creature-commands','alternate-console')){throw 'Exact case-sensitive Cleanup2 phase required.'}
Assert-KmgUnelevated
Assert-KmgNotRunning
$candidate=Read-KmgBuildLocalManifest -PackagePath $PackagePath -RepositoryRoot $root -DevelopmentPurpose 'summon-cleanup-iteration2'
Assert-KmgDevelopmentSourceBinding -Receipt $candidate -Git (Get-KmgGitState -RepositoryRoot $root) `
    -SourceStateSha256 (Get-KmgSourceStateFingerprint -RepositoryRoot $root) -ExpectedVersion $ExpectedVersion
if($candidate.candidateKind -cne 'development' -or $candidate.releaseQualification -ne $false -or
    $candidate.purpose -cne 'summon-cleanup-iteration2' -or $candidate.version -cne $ExpectedVersion){throw 'Exact Cleanup2 development artifact required.'}
if(-not $PSCmdlet.ShouldProcess('Steam App ID 640820',"run the one save-free Cleanup2 $Phase observation and restore current installation/settings")){return}
$saves=Join-Path ([Environment]::GetFolderPath('UserProfile')) 'AppData/LocalLow/Owlcat Games/Pathfinder Kingmaker/Saved Games'
$evidence=Join-Path $root ('artifacts/cleanup2-protected-native/'+[DateTime]::UtcNow.ToString('yyyyMMddTHHmmssZ')+'-'+[Guid]::NewGuid().ToString('N'))
$record=[ordered]@{schemaVersion=1;phase=$Phase;profileId=$profile;candidate=$candidate;status='PREFLIGHT';restoration=$null}
$catalog=$null;$failure=$null
try {
    $catalog=Open-KmgProtectedSaveCatalog -SaveDirectory $saves -EvidenceDirectory $evidence
    [void](Assert-KmgProtectedSaveCatalog $catalog)
    $record.status='ACTIVE'
    Write-KmgCompatibilityJsonAtomic (Join-Path $evidence 'protected-native.json') $record
    & (Join-Path $PSScriptRoot 'compatibility/Invoke-KingmakerCompatibilityProfile.ps1') `
        -ProfileId $profile -Scenario @('disposable-expanded-summoning-cleanup2-native-review') `
        -Parameters @{reviewPhase=$Phase} -RuntimeTimeoutSeconds $runtimeSeconds -ReuseInstalledArtifact `
        -ExpandedSummoningDisabled:($Phase -cin @('elementals-off','giants-off')) `
        -DevelopmentPurpose 'summon-cleanup-iteration2' -PackagePath $candidate.packagePath -Confirm:$false
} catch {$failure=$_;$record.status='FAILED';$record.error=$_.Exception.Message}
finally {
    if($catalog) {
        try {Wait-KmgSummonCleanup2Exit}
        catch {
            if(-not $failure){$failure=$_;$record.error=$_.Exception.Message}
            Wait-KmgSummonCleanup2ProtectedExit -Record $record -RecordPath (Join-Path $evidence 'protected-native.json') -ExitFailure $_
        }
        try {
            [void](Assert-KmgProtectedSaveCatalog $catalog)
            $record.restoration=[ordered]@{allOriginalSavesExact=$true;gameExitConfirmed=$true}
        } catch {$failure=$_;$record.status='FAILED';$record.error=$_.Exception.Message}
        finally {Close-KmgProtectedSaveCatalog $catalog}
    }
    if(-not $failure){$record.status='RESTORED'}
    Write-KmgCompatibilityJsonAtomic (Join-Path $evidence 'protected-native.json') $record
}
if($failure){throw $failure}
