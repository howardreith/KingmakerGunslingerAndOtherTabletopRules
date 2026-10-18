<#
Proves the ported preflight integration through the REAL production
Build-Local.ps1 dispatch, not a filename search or a copied model.

Section 2 names five behaviours. Each is proved by executing the shipped gate
statements with the preflight child replaced by a recording shim, so the
assertions are about the production dispatch itself. Nothing is built,
deployed or launched, and no installation or save is touched.
#>
[CmdletBinding()]
param([string]$RepositoryRoot)

# $PSScriptRoot is not reliably bound while parameter defaults are evaluated in
# every invocation form, so the default is computed in the body instead.
if (-not $RepositoryRoot) { $RepositoryRoot = Split-Path -Parent $PSScriptRoot }

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$script:Passed = 0
$script:Failures = New-Object System.Collections.ArrayList
function Assert-True([bool]$Condition, [string]$Name) {
    if ($Condition) { $script:Passed++; Write-Host "PASS $Name" }
    else { [void]$script:Failures.Add($Name); Write-Host "FAIL $Name" }
}

$gatePath = Join-Path $RepositoryRoot 'scripts\Build-Local.ps1'
$tokens = $null
$errors = $null
$gateAst = [System.Management.Automation.Language.Parser]::ParseFile(
    $gatePath, [ref]$tokens, [ref]$errors)
Assert-True (@($errors).Count -eq 0) 'production gate parses'

# Master's development-candidate route put the release path inside an else
# branch, so the interesting statements are no longer top level. Find them
# anywhere in the tree and order them by line, which is robust to nesting.
$allStatements = @($gateAst.FindAll({
        param($node)
        $node -is [System.Management.Automation.Language.StatementAst]
    }, $true))

function Find-Line([string]$Needle) {
    $hits = @($allStatements | Where-Object {
        $_.Extent.Text -like ('*' + $Needle + '*') -and
        ($_.Extent.EndLineNumber - $_.Extent.StartLineNumber) -lt 6
    } | Sort-Object { $_.Extent.StartLineNumber })
    if ($hits.Count -eq 0) { return -1 }
    return $hits[0].Extent.StartLineNumber
}

$preflightLine = Find-Line 'Test-RuntimeScenarioPreflight.ps1'
$guardLine = Find-Line 'Runtime scenario preflight failed'
$wrapperLine = Find-Line "'validate-repository.ps1'"

Assert-True ($preflightLine -gt 0) 'the production gate contains a preflight invocation statement'
Assert-True ($guardLine -eq $preflightLine + 1) ('the statement immediately after the preflight call is its throwing guard; preflight=' + $preflightLine + ' guard=' + $guardLine)
Assert-True ($wrapperLine -gt 0 -and $wrapperLine -lt $preflightLine) ('the repository wrapper still runs, and runs before the preflight; wrapper=' + $wrapperLine + ' preflight=' + $preflightLine)

# The release path's build, package and validate steps must all follow the
# guard. test-domain.ps1 is deliberately NOT checked here: the gate also has a
# development-candidate route earlier in the file which runs its own wrapper
# and domain tests and never reaches packaging.
$buildish = @('build_mod_from_private_references', 'package.ps1', 'validate-package.ps1')
$tooEarly = @()
$after = 0
foreach ($needle in $buildish) {
    $line = Find-Line $needle
    if ($line -gt 0 -and $line -lt $preflightLine) { $tooEarly += ($needle + '@' + $line) }
    if ($line -gt $guardLine) { $after++ }
}
Assert-True ($tooEarly.Count -eq 0) ('no release build/package statement precedes the preflight guard; early=' + ($tooEarly -join ','))
Assert-True ($after -ge 2) ('the release build and package steps are downstream of the guard; found=' + $after)

# The shim cases below execute the shipped statement itself.
$statements = $allStatements
$preflightIndex = 0
for ($i = 0; $i -lt $allStatements.Count; $i++) {
    if ($allStatements[$i].Extent.StartLineNumber -eq $preflightLine -and
        $allStatements[$i].Extent.Text -like '*Test-RuntimeScenarioPreflight.ps1*' -and
        ($allStatements[$i].Extent.EndLineNumber - $allStatements[$i].Extent.StartLineNumber) -lt 6) {
        $preflightIndex = $i
        break
    }
}
$guard = ($allStatements | Where-Object {
    $_.Extent.StartLineNumber -eq $guardLine -and
    $_.Extent.Text -like '*LASTEXITCODE*'
} | Select-Object -First 1).Extent.Text

$shimDir = Join-Path $env:TEMP ('kmg-preflight-shim-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $shimDir -Force | Out-Null
try {
    $callLog = Join-Path $shimDir 'calls.txt'
    foreach ($case in @(@{ Name = 'passing'; Exit = 0 }, @{ Name = 'failing'; Exit = 3 })) {
        Set-Content -LiteralPath $callLog -Value '' -Encoding utf8
        $shim = Join-Path $shimDir 'Test-RuntimeScenarioPreflight.ps1'
        $shimBody = "Add-Content -LiteralPath '" + $callLog + "' -Value 'invoked'" + [Environment]::NewLine + 'exit ' + $case.Exit
        Set-Content -LiteralPath $shim -Value $shimBody -Encoding utf8

        # StrictMode refuses an unset $LASTEXITCODE, so it is seeded with a
        # value the child cannot produce; the assertion below proves the
        # real exit code replaced it.
        $global:LASTEXITCODE = -99
        $runner = [ScriptBlock]::Create('param($PSScriptRoot)' + [Environment]::NewLine + $statements[$preflightIndex].Extent.Text + [Environment]::NewLine + 'return $global:LASTEXITCODE')
        $code = $null
        $previous = $ErrorActionPreference
        $ErrorActionPreference = 'Continue'
        try { $code = & $runner $shimDir } finally { $ErrorActionPreference = $previous }
        $invocations = @(Get-Content -LiteralPath $callLog | Where-Object { $_ -eq 'invoked' }).Count

        Assert-True ($invocations -eq 1) ('the gate reaches the preflight child exactly once (' + $case.Name + ')')
        Assert-True ($code -eq $case.Exit) ('the child real exit code reaches the gate (' + $case.Name + '): ' + $code)

        $guardRunner = [ScriptBlock]::Create('param($LASTEXITCODE)' + [Environment]::NewLine + $guard)
        $guardThrew = $false
        try { & $guardRunner $case.Exit } catch { $guardThrew = $true }
        if ($case.Exit -eq 0) {
            Assert-True (-not $guardThrew) 'valid registry agreement succeeds and the gate continues'
        } else {
            Assert-True $guardThrew 'a deliberately failing child throws, preventing build/package/deploy'
        }
    }
}
finally {
    Remove-Item -LiteralPath $shimDir -Recurse -Force -ErrorAction SilentlyContinue
}

. (Join-Path $RepositoryRoot 'scripts\RuntimeHarness.Common.ps1')
. (Join-Path $RepositoryRoot 'scripts\RuntimeAutomation.Common.ps1')
$catalogPath = Join-Path $RepositoryRoot 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestScenarioCatalog.cs'
$catalog = Get-Content -Raw -LiteralPath $catalogPath
$catalogLiterals = @(@([regex]::Matches($catalog, '"([a-z][a-z0-9-]+)"') | ForEach-Object { $_.Groups[1].Value }) | Sort-Object -Unique)
# The real preflight sources one more scenario separately, from the owned-save
# contract rather than from a literal in the catalog file. Mirror that here
# instead of treating its absence as a registry disagreement.
$ownedContract = Get-Content -Raw -LiteralPath (Join-Path $RepositoryRoot 'src\KingmakerGunslinger\RuntimeTesting\WeaponFindabilitySaveContract.cs')
$ownedScenario = [regex]::Match($ownedContract, 'const string Scenario\s*=\s*"([a-z][a-z0-9-]+)"').Groups[1].Value
Assert-True (-not [string]::IsNullOrEmpty($ownedScenario)) 'the owned-save contract scenario is readable, as the real preflight reads it'
$csharp = @(@($catalogLiterals) + @($ownedScenario) | Sort-Object -Unique)
$powershell = @($script:KmgRuntimeScenarios | Sort-Object)

$onlyPowershell = @($powershell | Where-Object { $csharp -cnotcontains $_ })
Assert-True ($onlyPowershell.Count -eq 0) ('no scenario is registered in PowerShell alone; extra=' + ($onlyPowershell -join ','))

$researchOnly = 'observe-expanded-summoning-sprint25-donor-census'
Assert-True ($csharp -cnotcontains $researchOnly) 'the research-only census scenario is absent from the production C# catalog'
Assert-True ($powershell -cnotcontains $researchOnly) 'the research-only census scenario is absent from the production PowerShell metadata'

$refused = $false
try { $null = Get-KmgRuntimeScenarioMetadata 'observe-not-a-real-scenario' } catch { $refused = $true }
Assert-True $refused 'a scenario in no PowerShell registry is refused by the real metadata lookup'

$refusedResearch = $false
try { $null = Get-KmgRuntimeScenarioMetadata $researchOnly } catch { $refusedResearch = $true }
Assert-True $refusedResearch 'the research-only scenario is refused by production, at the metadata boundary, before any deployment'

$launcher = Get-Content -Raw -LiteralPath (Join-Path $RepositoryRoot 'scripts\Invoke-KingmakerRuntimeTest.ps1')
foreach ($contract in @(
    'Deployment/package paths are valid only with an explicit artifact-reuse authority.',
    'Artifact reuse requires exact -DeploymentManifestPath and -PackagePath.',
    'Current-source and qualified-legacy artifact reuse are mutually exclusive.',
    'ShouldProcess')) {
    Assert-True ($launcher.Contains($contract)) ('reuse/confirmation semantics preserved: ' + $contract)
}

$wrapper = Get-Content -Raw -LiteralPath (Join-Path $RepositoryRoot 'scripts\Invoke-ExpandedSummoningRuntimeScenario.ps1')
foreach ($contract in @("ConfirmImpact = 'High'", 'Get-KmgTreeFingerprint', 'Backup-Live-Mod.ps1', 'Assert-KmgNotRunning')) {
    Assert-True ($wrapper.Contains($contract)) ('snapshot/confirmation contract preserved: ' + $contract)
}
Assert-True ($wrapper -match '\$evidenceNames\s*=\s*@\(if') 'the ported evidence-name expression is array-wrapped, not collapsing'

Write-Host ''
if ($script:Failures.Count -ne 0) {
    throw ('Production preflight integration FAILED: ' + ($script:Failures -join '; '))
}
Write-Host ('Production preflight integration passed: ' + $script:Passed + ' assertions. Nothing built, deployed or launched.')
