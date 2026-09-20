[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
# Retain the complete Circle binding/transport regressions in this same session.
. (Join-Path $PSScriptRoot 'Test-MagicCircleRuntimeRequest.ps1')
$originalEvidenceRoot = $script:KmgRuntimeEvidenceRoot
$testRoot = Join-Path (Split-Path $PSScriptRoot) ('artifacts\tests\circle-release-' + [Guid]::NewGuid().ToString('N'))
[void](New-Item -ItemType Directory -Path $testRoot)
$script:KmgRuntimeEvidenceRoot = $testRoot
$synthetic = Join-Path $testRoot 'request'
$transaction = '20260920T1234561234567Z_' + [Guid]::NewGuid().ToString('N')
$checks = 0
try {
    foreach ($phase in @('prepare','verify')) {
        $saveName = if ($phase -ceq 'prepare') { 'KMG_AUTOMATION_WORKING' } else { 'KMG_FCB_PERSISTENCE_' + $transaction + '_prepare' }
        $inputPath = Join-Path $testRoot ($phase + '.fixture')
        [IO.File]::WriteAllText($inputPath, 'disposable request identity only')
        $planPath = Join-Path $testRoot ($phase + '-plan.json')
        $plan = [ordered]@{schemaVersion=1;transactionId=$transaction;phase=$phase;version=$version
            input=@{name=$saveName;path=$inputPath;sha256=(Get-FileHash -LiteralPath $inputPath -Algorithm SHA256).Hash.ToLowerInvariant()}}
        [IO.File]::WriteAllText($planPath, ($plan | ConvertTo-Json -Depth 5))
        $parameters = @{saveName=$saveName;phase=$phase;planPath=$planPath}
        $request = New-KmgRuntimeRequest -Scenario 'disposable-word-of-recall-favored-class-persistence' @workingTimeouts `
            -ExpectedVersion $version -TimeoutSeconds 180 -ExitAfterCompletion $true -EvidenceDirectory $synthetic -Parameters $parameters
        if ($request.parameters.Count -ne 3 -or $request.parameters.planPath -cne $planPath -or $request.parameters.saveName -cne $saveName) {
            $failures.Add("integrated-fcb-$phase-keeps-three-exact-parameters")
        }
        $checks++
        Assert-Throws {
            New-KmgRuntimeRequest -Scenario 'working-save-magic-circle-cleanup' @workingTimeouts `
                -ExpectedVersion $version -TimeoutSeconds 180 -ExitAfterCompletion $true -EvidenceDirectory $synthetic -Parameters $parameters
        } "word-of-recall-plan-cannot-authorize-circle-$phase"
        $checks++
        $parameters.preparationBinding = $binding
        Assert-Throws {
            New-KmgRuntimeRequest -Scenario 'disposable-word-of-recall-favored-class-persistence' @workingTimeouts `
                -ExpectedVersion $version -TimeoutSeconds 180 -ExitAfterCompletion $true -EvidenceDirectory $synthetic -Parameters $parameters
        } "integrated-fcb-$phase-rejects-extra-circle-binding"
        $checks++
        $parameters.Remove('preparationBinding')
        [IO.File]::AppendAllText($inputPath, ' changed')
        Assert-Throws {
            New-KmgRuntimeRequest -Scenario 'disposable-word-of-recall-favored-class-persistence' @workingTimeouts `
                -ExpectedVersion $version -TimeoutSeconds 180 -ExitAfterCompletion $true -EvidenceDirectory $synthetic -Parameters $parameters
        } "integrated-fcb-$phase-rejects-replaced-input"
        $checks++
    }
    Assert-Throws {
        New-KmgRuntimeRequest -Scenario 'disposable-word-of-recall-favored-class-persistence' @workingTimeouts `
            -ExpectedVersion $version -TimeoutSeconds 180 -ExitAfterCompletion $true -EvidenceDirectory $synthetic `
            -Parameters @{saveName='KMG_AUTOMATION_WORKING';preparationBinding=$binding}
    } 'circle-binding-cannot-authorize-word-of-recall'
    $checks++
    $driver = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot 'Invoke-WordOfRecallFavoredClassPersistence.ps1')
    if (-not $driver.Contains('$settingsOriginal.schemaVersion -ne 12')) { $failures.Add('word-of-recall-driver-current-settings-schema') }
    $checks++
} finally {
    $script:KmgRuntimeEvidenceRoot = $originalEvidenceRoot
}
if ($failures.Count -gt 0) { throw ($failures -join ', ') }
Write-Output "PASS integrated Circle/Word of Recall request contracts; checks=$checks; disposable evidence=$testRoot"
