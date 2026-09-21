[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')

$failures = [Collections.Generic.List[string]]::new()
function Assert-Throws([scriptblock]$Action, [string]$Name) {
    try { & $Action; $failures.Add($Name) } catch { }
}

$version = (Get-Content -Raw (Join-Path $PSScriptRoot '../Info.json') | ConvertFrom-Json).Version
$synthetic = Join-Path $script:KmgRuntimeEvidenceRoot 'magic-circle-request-test'
$circleAudit = New-KmgRuntimeRequest -Scenario 'observe-magic-circle-native-contracts' `
    -ExpectedVersion $version -TimeoutSeconds 30 -ExitAfterCompletion $true `
    -EvidenceDirectory $synthetic
if ($circleAudit.parameters.Count -ne 0 -or
    (Get-KmgRuntimeScenarioMetadata -Scenario $circleAudit.scenario).RequiresSaveName) {
    $failures.Add('magic-circle-audit-is-read-only-save-free')
}
Assert-Throws {
    New-KmgRuntimeRequest -Scenario 'observe-magic-circle-native-contracts' `
        -ExpectedVersion $version -TimeoutSeconds 30 -ExitAfterCompletion $true `
        -EvidenceDirectory $synthetic -Parameters @{ saveName = 'KMG_AUTOMATION_WORKING' }
} 'magic-circle-audit-rejects-save-parameters'
$workingTimeouts = @{
    CatalogTimeoutSeconds = 30; SelectionTimeoutSeconds = 30
    CompletionTimeoutSeconds = 30; MainMenuTimeoutSeconds = 30
    ActionResolutionTimeoutSeconds = 30; ActionInvocationTimeoutSeconds = 30
    DescriptorResolutionTimeoutSeconds = 30; LoadEntryTimeoutSeconds = 30
    FingerprintTimeoutSeconds = 30
}
Assert-Throws {
    New-KmgRuntimeRequest -Scenario 'working-save-magic-circle-cleanup' @workingTimeouts `
        -ExpectedVersion $version -TimeoutSeconds 180 -ExitAfterCompletion $true `
        -EvidenceDirectory $synthetic -Parameters @{ saveName = 'KMG_AUTOMATION_WORKING' }
} 'circle-cleanup-rejects-missing-preparation-binding-before-launch'
$native = New-KmgRuntimeRequest -Scenario 'disposable-magic-circle-evil' @workingTimeouts `
    -ExpectedVersion $version -TimeoutSeconds 180 -ExitAfterCompletion $true `
    -EvidenceDirectory $synthetic -Parameters @{ saveName = 'KMG_AUTOMATION_WORKING' }
if ($native.parameters.saveName -cne 'KMG_AUTOMATION_WORKING') {
    $failures.Add('magic-circle-native-exact-working-save')
}
Assert-Throws {
    New-KmgRuntimeRequest -Scenario 'disposable-magic-circle-evil' @workingTimeouts `
        -ExpectedVersion $version -TimeoutSeconds 180 -ExitAfterCompletion $true `
        -EvidenceDirectory $synthetic -Parameters @{ saveName = 'KMG_AUTOMATION_BASELINE' }
} 'magic-circle-native-rejects-baseline'
$native = New-KmgRuntimeRequest -Scenario 'disposable-magic-circle-ui' @workingTimeouts `
    -ExpectedVersion $version -TimeoutSeconds 180 -ExitAfterCompletion $true `
    -EvidenceDirectory $synthetic -Parameters @{ saveName = 'KMG_AUTOMATION_WORKING' }
if ($native.parameters.saveName -cne 'KMG_AUTOMATION_WORKING') {
    $failures.Add('magic-circle-ui-exact-working-save')
}
Assert-Throws {
    New-KmgRuntimeRequest -Scenario 'disposable-magic-circle-ui' @workingTimeouts `
        -ExpectedVersion $version -TimeoutSeconds 180 -ExitAfterCompletion $true `
        -EvidenceDirectory $synthetic -Parameters @{ saveName = 'KMG_AUTOMATION_BASELINE' }
} 'magic-circle-ui-rejects-baseline'
$terrain = New-KmgRuntimeRequest -Scenario 'disposable-magic-circle-terrain' @workingTimeouts `
    -ExpectedVersion $version -TimeoutSeconds 180 -ExitAfterCompletion $true `
    -EvidenceDirectory $synthetic -Parameters @{ saveName = 'KMG_AUTOMATION_WORKING' }
if ($terrain.parameters.saveName -cne 'KMG_AUTOMATION_WORKING') { $failures.Add('terrain-exact-working-save') }
foreach ($invalid in @(@{ saveName='KMG_AUTOMATION_BASELINE' }, @{ saveName='KMG_AUTOMATION_WORKING'; area='arbitrary' })) {
    Assert-Throws {
        New-KmgRuntimeRequest -Scenario 'disposable-magic-circle-terrain' @workingTimeouts `
            -ExpectedVersion $version -TimeoutSeconds 180 -ExitAfterCompletion $true `
            -EvidenceDirectory $synthetic -Parameters $invalid
    } 'terrain-rejects-unowned-save-or-area'
}
Assert-Throws {
    New-KmgRuntimeRequest -Scenario 'disposable-magic-circle-terrain' @workingTimeouts `
        -ExpectedVersion $version -TimeoutSeconds 180 -ExitAfterCompletion $false `
        -EvidenceDirectory $synthetic -Parameters @{ saveName='KMG_AUTOMATION_WORKING' }
} 'terrain-requires-exit'
$save = [ordered]@{ Name='KMG_AUTOMATION_WORKING'; FileName='Working.zks'; FolderName='Working.zks'; GameName='fixture'; GameId='fixture'; Area='fixture' }
$fixtureActors=@(1..4 | ForEach-Object { @{ id=[Guid]::NewGuid().ToString('D');role="role$_";blueprint=('a'*32);books=@() } })
$record = [ordered]@{ schemaVersion=2; phase='prepare'; runId='prepare-fixture-A'; exception=$null; workingSave=$save
    artifact=[ordered]@{ version=$version; dllSha256=('a'*64); mvid='ea9ac240-4984-421b-b2e1-4336fa5770c6'; gitCommit=('b'*40) }
    fixtureIdentity=@{ actors=$fixtureActors; area=('b'*32);favoredOracle=@{};marketReceipt=@{TableId=('c'*32)};inventory=@();gold=123
        control=@(@{source=$fixtureActors[0].id;endTimeTicks=1234567})
        carriers=@(1..8 | ForEach-Object { @{bearer=$fixtureActors[2].id;caster=$fixtureActors[0].id;blueprint=('d'*32);sourceSpell=('e'*32);level=8;endTimeTicks=1234567;extend=$true} }) } }
$result = [ordered]@{ runId=$record.runId; scenario='working-save-magic-circle-prepare'; status='PASS'; loadedModVersion=$version; gitCommit=('b'*40)
    automaticExitRequested=$true; automaticExitInitiated=$true; assertions=@(@{status='PASS'})
    workingSaveSmoke=@{ descriptorReferenceCorrelated=$true; completionCallbackObserved=$true; saveWritingApiObserved=$false
        hooksRemoved=$true; expectedWorkingSaveRoutineCount=1; expectedWorkingStashedAreaCount=1
        resolvedDescriptor=@{safeFields=@($save.Keys | ForEach-Object { @{Key=$_;Value=$save[$_]} })} } }
$recordBytes=[Text.Encoding]::UTF8.GetBytes(($record | ConvertTo-Json -Depth 12 -Compress))
$resultBytes=[Text.Encoding]::UTF8.GetBytes(($result | ConvertTo-Json -Depth 12 -Compress))
$identity=[ordered]@{semanticVersion=$version;loadedModuleSha256=$record.artifact.dllSha256;moduleVersionId=$record.artifact.mvid;gitCommit=$record.artifact.gitCommit}
$identityBytes=[Text.Encoding]::UTF8.GetBytes(($identity | ConvertTo-Json -Compress))
$binding=[ordered]@{schemaVersion=2;recordBase64=[Convert]::ToBase64String($recordBytes);recordSha256=(Get-KmgMagicCircleBytesHash $recordBytes)
    resultBase64=[Convert]::ToBase64String($resultBytes);resultSha256=(Get-KmgMagicCircleBytesHash $resultBytes)
    producerIdentityBase64=[Convert]::ToBase64String($identityBytes);producerIdentitySha256=(Get-KmgMagicCircleBytesHash $identityBytes)
    consumerArtifact=$record.artifact} | ConvertTo-Json -Depth 4 -Compress
foreach ($invalidBinding in @('', '{}', $binding.Replace('"schemaVersion":2','"schemaVersion":2,"schemaVersion":2'),
    $binding.Replace((Get-KmgMagicCircleBytesHash $identityBytes), ('d'*64)),
    $binding.Replace($record.runId, 'unused').Replace((Get-KmgMagicCircleBytesHash $recordBytes), ('c'*64)))) {
    Assert-Throws {
        New-KmgRuntimeRequest -Scenario 'working-save-magic-circle-cleanup' @workingTimeouts `
            -ExpectedVersion $version -TimeoutSeconds 180 -ExitAfterCompletion $true -EvidenceDirectory $synthetic `
            -Parameters @{ saveName='KMG_AUTOMATION_WORKING'; preparationBinding=$invalidBinding }
    } 'circle-cleanup-rejects-malformed-or-hash-mismatched-binding'
}
Assert-Throws { ConvertFrom-KmgCircleBindingJson '{"record":{"a":1,"\u0061":2}}' } 'duplicate-escaped-record-member-rejected'
foreach ($phase in @('prepare', 'verify', 'cleanup', 'absent', 'scene')) {
    $phaseParameters = @{ saveName = 'KMG_AUTOMATION_WORKING' }
    if ($phase -in @('verify','scene','cleanup')) { $phaseParameters.preparationBinding = $binding }
    $request = New-KmgRuntimeRequest -Scenario "working-save-magic-circle-$phase" @workingTimeouts `
        -ExpectedVersion $version -TimeoutSeconds 180 -Parameters $phaseParameters `
        -EvidenceDirectory $synthetic -ExitAfterCompletion:$true
    if ($request.parameters.saveName -cne 'KMG_AUTOMATION_WORKING') {
        $failures.Add("magic-circle-persistence-$phase-exact-save")
    }
    if ($phase -in @('verify','scene','cleanup')) {
        if ($request.parameters.Count -ne 2 -or -not $request.parameters.Contains('preparationBinding') -or
            $request.parameters.preparationBinding -cne $binding) {
            $failures.Add("magic-circle-persistence-$phase-transports-exact-binding")
        } else {
            # Exercise the production request's actual transport representation,
            # not just the preflight validator that accepted the input binding.
            $transport = ($request | ConvertTo-Json -Depth 8) | ConvertFrom-Json
            if ($transport.parameters.preparationBinding -cne $binding) {
                $failures.Add("magic-circle-persistence-$phase-binding-json-roundtrip")
            }
            [void](Read-KmgMagicCirclePreparationBinding $transport.parameters.preparationBinding $version)
        }
    }
    Assert-Throws {
        New-KmgRuntimeRequest -Scenario "working-save-magic-circle-$phase" @workingTimeouts `
            -ExpectedVersion $version -TimeoutSeconds 180 -Parameters @{ saveName = 'KMG_AUTOMATION_BASELINE' } `
            -EvidenceDirectory $synthetic -ExitAfterCompletion:$true
    } "magic-circle-persistence-$phase-rejects-baseline"
}

$profile = New-KmgRuntimeRequest -Scenario 'disposable-magic-circle-profile' -ExpectedVersion $version -TimeoutSeconds 180 -ExitAfterCompletion $true -EvidenceDirectory $synthetic
if ($profile.parameters.Count -ne 0 -or (Get-KmgRuntimeScenarioMetadata -Scenario $profile.scenario).RequiresSaveName) {
    $failures.Add('circle-profile-native-save-free-scope')
}
Assert-Throws {
    New-KmgRuntimeRequest -Scenario 'disposable-magic-circle-profile' -ExpectedVersion $version -TimeoutSeconds 180 -ExitAfterCompletion $false -EvidenceDirectory $synthetic
} 'circle-profile-requires-native-exit'
Assert-Throws {
    New-KmgRuntimeRequest -Scenario 'disposable-magic-circle-profile' -ExpectedVersion $version -TimeoutSeconds 180 -ExitAfterCompletion $true -EvidenceDirectory $synthetic -Parameters @{saveName='KMG_AUTOMATION_WORKING'}
} 'circle-profile-rejects-even-working-save'
Assert-Throws {
    New-KmgRuntimeRequest -Scenario 'disposable-magic-circle-profile' -ExpectedVersion $version -TimeoutSeconds 180 -ExitAfterCompletion $true -EvidenceDirectory $synthetic -CatalogTimeoutSeconds 30
} 'circle-profile-rejects-save-catalog-timeout'

if ($failures.Count -gt 0) { throw ($failures -join ', ') }
Write-Output 'PASS Magic Circle guarded request tests.'
