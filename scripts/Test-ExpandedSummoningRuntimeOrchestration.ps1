<#
.SYNOPSIS
Bounded tests for the Expanded Summoning runtime orchestration decisions.

.DESCRIPTION
Covers the six cases the owner's correction order names: result isolation,
stale-result rejection, parse failure, restoration failure, interrupted
operation, and a successful transaction.

Nothing here launches Kingmaker, takes a runtime lease, or writes to the
installation.

Two notes on what is and is not exercised for real.

The decision functions are driven directly and completely - every branch, with
the inputs that once produced a wrong answer.

The shipped Backup-Live-Mod.ps1 and Restore-Live-Mod.ps1 deliberately refuse any
directory other than the exact live mod path, so they cannot be pointed at a
fixture tree. That guard is worth more than the convenience of testing through
it, so these tests assert the refusal itself, and exercise the successful and
interrupted transaction shapes against a fixture with the same fingerprint
function the wrapper uses. That is stated here rather than implied, because a
test that looks like it covers the shipped copy path when it does not is worse
than one that says what it covers.
#>
[CmdletBinding()]
param(
    # Where the shipped harness scripts live. Defaults to this script's own
    # directory, which is where it sits in the repository; overridable so the
    # suite can be exercised from a staging copy.
    [string]$ScriptRoot = $PSScriptRoot
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'ExpandedSummoningOrchestration.Common.ps1')

$script:Passed = 0
function Assert-True {
    param([Parameter(Mandatory = $true)][bool]$Condition,
          [Parameter(Mandatory = $true)][string]$Message)
    if (-not $Condition) { throw "Orchestration test failed: $Message" }
    $script:Passed++
}
function Assert-Equal {
    param($Expected, $Actual, [Parameter(Mandatory = $true)][string]$Message)
    if ([string]$Expected -cne [string]$Actual) {
        throw "Orchestration test failed: $Message (expected '$Expected', got '$Actual')"
    }
    $script:Passed++
}

$bodyJson = '[{"key":"viper"},{"key":"constrictor-snake"},{"key":"salamander"}]'
$bodyRows = @(ConvertFrom-KmgSerpentineBodyReviewJson -Json $bodyJson)
Assert-Equal 3 $bodyRows.Count 'top-level research JSON emits three rows, never one wrapped array'
Assert-Equal 'viper,constrictor-snake,salamander' ($bodyRows.key -join ',') 'body research preserves the closed order'
foreach ($invalidBodyJson in @('null', '[]', '{}', '[',
    '[{"key":"viper"}]', ('[' + $bodyJson + ']'),
    '[{"key":"viper"},{"key":"viper"},{"key":"salamander"}]',
    '[{"key":"Viper"},{"key":"constrictor-snake"},{"key":"salamander"}]',
    '[{"key":"salamander"},{"key":"constrictor-snake"},{"key":"viper"}]',
    '[{"key":"viper"},{"key":"constrictor-snake"},{"key":"foreign"}]',
    '[{"key":"viper"},{"key":"constrictor-snake"},{}]')) {
    $rejected = $false
    try { $null = ConvertFrom-KmgSerpentineBodyReviewJson -Json $invalidBodyJson }
    catch { $rejected = $true }
    Assert-True $rejected 'malformed, missing, wrapped, duplicate, foreign or reordered body evidence fails closed'
}

$snakeRows = @()
$snakeAssertions = @()
foreach ($key in @('viper', 'constrictor-snake')) {
    foreach ($fault in @($true, $false)) {
        $snakeRows += [pscustomobject]@{ key=$key; scope='production hidden-snake body binding'
            faultInjected=$fault; gameplayQualified=$false; intact=$true }
        $case = if ($fault) { 'rollback' } else { 'normal' }
        foreach ($kind in @('binding', 'scale', 'once', 'destroy')) {
            $snakeAssertions += [pscustomobject]@{
                name=('sprint17-production-' + $kind + '-' + $key + '-' + $case); status='PASS' }
        }
    }
}
foreach ($key in @('viper', 'constrictor-snake')) {
    foreach ($kind in @('scores', 'defenses', 'land-skills', 'one-bite', 'base',
        'strength-plus4', 'strength-seven', 'native-animal-growth', 'modifiers-restored')) {
        $snakeRows += [pscustomobject]@{ key=$key; check=($key + '-' + $kind); passed=$true }
        $snakeAssertions += [pscustomobject]@{
            name=('sprint17-snake-profile-' + $key + '-' + $kind); status='PASS' }
    }
}
foreach ($name in @('sprint17-production-native-worm-negative-control',
    'sprint17-body-environment-restored', 'sprint17-body-fixture-cleanup', 'loaded-mod-version')) {
    $snakeAssertions += [pscustomobject]@{ name=$name; status='PASS' }
}
$snakeJson = ConvertTo-Json -InputObject $snakeRows -Depth 6
$collectedRows = @(ConvertFrom-KmgSnakeProfileSliceEvidence -Json $snakeJson -Assertions $snakeAssertions)
Assert-Equal 22 $collectedRows.Count 'Windows PowerShell collects22 real snake rows, not one nested array'
Assert-Equal 18 @($collectedRows | Where-Object { $_.PSObject.Properties['check'] }).Count 'all18 profile observations survive collection'
Assert-Equal 'viper,viper,constrictor-snake,constrictor-snake' ($collectedRows[0..3].key -join ',') 'all four normal/rollback rows retain exact scope'
foreach ($badJson in @('null', '{}', '[]', '[', ('[' + $snakeJson + ']'),
    (ConvertTo-Json -InputObject $snakeRows[0..20] -Depth 6),
    (ConvertTo-Json -InputObject ($snakeRows + $snakeRows[0]) -Depth 6))) {
    $rejected = $false
    try { $null = ConvertFrom-KmgSnakeProfileSliceEvidence -Json $badJson -Assertions $snakeAssertions }
    catch { $rejected = $true }
    Assert-True $rejected 'malformed, nested, missing or extra snake rows fail closed'
}
foreach ($mutate in @(
    { param($rows) $rows[0].key='salamander' },
    { param($rows) $rows[1].key='Viper' },
    { param($rows) $rows[0].scope='donor research' },
    { param($rows) $rows[0].faultInjected='true' },
    { param($rows) $rows[0].faultInjected=$false },
    { param($rows) $rows[0].gameplayQualified=$true },
    { param($rows) $rows[0].intact=$false },
    { param($rows) $rows[0].intact='true' },
    { param($rows) $rows[4].check=$rows[5].check },
    { param($rows) $rows[4].check='foreign-scores' },
    { param($rows) $rows[4].key='constrictor-snake' },
    { param($rows) $rows[4].passed=$false },
    { param($rows) $rows[4].passed='true' },
    { param($rows) $rows[4].PSObject.Properties.Remove('passed') },
    { param($rows) $rows[4]=$null }
)) {
    $changedRows = $snakeJson | ConvertFrom-Json
    & $mutate $changedRows
    $rejected = $false
    try { $null = ConvertFrom-KmgSnakeProfileSliceEvidence -Json (ConvertTo-Json -InputObject $changedRows -Depth 6) -Assertions $snakeAssertions }
    catch { $rejected = $true }
    Assert-True $rejected 'wrong keys, duplicate checks, invalid bools and failed observations cannot pass'
}
$assertionsJson = ConvertTo-Json -InputObject $snakeAssertions
foreach ($mutate in @(
    { param($rows) $rows[0].name=$rows[1].name },
    { param($rows) $rows[0].name='foreign-assertion' },
    { param($rows) $rows[0].status='FAIL' },
    { param($rows) $rows[0].PSObject.Properties.Remove('status') }
)) {
    $changedAssertions = $assertionsJson | ConvertFrom-Json
    & $mutate $changedAssertions
    $rejected = $false
    try { $null = ConvertFrom-KmgSnakeProfileSliceEvidence -Json $snakeJson -Assertions $changedAssertions }
    catch { $rejected = $true }
    Assert-True $rejected '38 native assertions must be exact, unique and explicitly passed'
}
foreach ($badAssertions in @(@(), $snakeAssertions[0..36], ($snakeAssertions + $snakeAssertions[0]))) {
    $rejected = $false
    try { $null = ConvertFrom-KmgSnakeProfileSliceEvidence -Json $snakeJson -Assertions $badAssertions }
    catch { $rejected = $true }
    Assert-True $rejected 'empty, partial or extra native assertion sets fail closed'
}

# The extended rules request cannot inherit PASS from the closed profile
# slice alone. Root arrays, all24 ordered rows and all62 verdicts are required.
$signatureNames = @('viper-contract', 'viper-wounds', 'viper-misses', 'viper-zero-damage',
    'viper-non-bite', 'viper-live-dc-0', 'viper-live-dc-4', 'viper-live-dc--6',
    'viper-six-exposures', 'viper-native-cure', 'viper-source-destruction',
    'constrict-grab-rejections', 'constrict-bite-delivery', 'constrict-no-application-frame-maintain',
    'constrict-later-maintain-once', 'constrict-strength-0', 'constrict-strength-4',
    'constrict-strength--10', 'constrict-native-growth', 'constrict-modifiers-restored',
    'constrict-lethal-prey', 'constrict-dead-prey-rejected', 'constrict-destroyed-prey-rejected',
    'constrict-owner-death')
$signatureRows = @($snakeRows)
$signatureAssertions = @($snakeAssertions)
foreach ($name in $signatureNames) {
    $signatureRows += [pscustomobject]@{ signature=$name; passed=$true }
    $signatureAssertions += [pscustomobject]@{ name=('sprint17-snake-signature-' + $name); status='PASS' }
}
$signatureJson = ConvertTo-Json -InputObject $signatureRows -Depth 6
$signatureAssertionsJson = ConvertTo-Json -InputObject $signatureAssertions
$observed = @(ConvertFrom-KmgSnakeSignatureSliceEvidence -Json $signatureJson -Assertions $signatureAssertions)
Assert-Equal 46 $observed.Count 'complete signature request emits46 flat rows'
Assert-Equal ($signatureNames -join ',') ($observed[22..45].signature -join ',') 'all24 signatures retain exact order'
foreach ($badJson in @('null', '{}', '[]', '[', ('[' + $signatureJson + ']'), $snakeJson,
    (ConvertTo-Json -InputObject $signatureRows[0..44] -Depth 6),
    (ConvertTo-Json -InputObject ($signatureRows + $signatureRows[0]) -Depth 6))) {
    $rejected = $false
    try { $null = ConvertFrom-KmgSnakeSignatureSliceEvidence -Json $badJson -Assertions $signatureAssertions }
    catch { $rejected = $true }
    Assert-True $rejected 'incomplete, nested or malformed signatures fail closed'
}
foreach ($mutate in @(
    { param($rows) $rows[22].signature=$rows[23].signature },
    { param($rows) $rows[22].signature='foreign' },
    { param($rows) $rows[22].passed=$false },
    { param($rows) $rows[22].passed='true' },
    { param($rows) $rows[22].PSObject.Properties.Remove('passed') },
    { param($rows) $rows[22]=$null },
    { param($rows) $rows[22]=@($rows[22]) },
    { param($rows) $rows[0].intact=$false },
    { param($rows) $rows[4].passed=$false }
)) {
    $changedRows = $signatureJson | ConvertFrom-Json
    & $mutate $changedRows
    $rejected = $false
    try { $null = ConvertFrom-KmgSnakeSignatureSliceEvidence -Json (ConvertTo-Json -InputObject $changedRows -Depth 6) -Assertions $signatureAssertions }
    catch { $rejected = $true }
    Assert-True $rejected 'signature rows and inherited body/profile rows must independently pass'
}
foreach ($mutate in @(
    { param($rows) $rows[38].name=$rows[39].name },
    { param($rows) $rows[38].name='sprint17-snake-signature-foreign' },
    { param($rows) $rows[38].status='FAIL' },
    { param($rows) $rows[38].PSObject.Properties.Remove('status') },
    { param($rows) $rows[0].status='FAIL' },
    { param($rows) $rows[38]=$null }
)) {
    $changedAssertions = $signatureAssertionsJson | ConvertFrom-Json
    & $mutate $changedAssertions
    $rejected = $false
    try { $null = ConvertFrom-KmgSnakeSignatureSliceEvidence -Json $signatureJson -Assertions $changedAssertions }
    catch { $rejected = $true }
    Assert-True $rejected '62 exact native assertions cannot hide missing, duplicate, foreign or failed results'
}
foreach ($badAssertions in @(@(), $snakeAssertions, $signatureAssertions[0..60],
    ($signatureAssertions + $signatureAssertions[0]))) {
    $rejected = $false
    try { $null = ConvertFrom-KmgSnakeSignatureSliceEvidence -Json $signatureJson -Assertions $badAssertions }
    catch { $rejected = $true }
    Assert-True $rejected 'profile-only, incomplete or extra assertion sets cannot qualify signatures'
}

# A distinct eight-cell request, never an alias of profile/rules qualification.
$commandRows = @()
$commandAssertions = @()
foreach ($key in @('viper', 'constrictor-snake')) {
    foreach ($mode in @('rtwp', 'turn-based')) {
        foreach ($driver in @('manual', 'ai')) {
            $id = "$key-$mode-$driver"
            $checks = [ordered]@{}
            foreach ($check in @('native-setup', 'approach', 'attack', 'signature', 'contact', 'cleanup')) {
                $checks[$check] = $true
                $commandAssertions += [pscustomobject]@{ name="sprint17-snake-command-$id-$check"; status='PASS' }
            }
            $commandRows += [pscustomobject]@{ cell=$id; scope='production hidden-snake native command/contact slice'; checks=[pscustomobject]$checks; passed=$true }
        }
    }
}
foreach ($name in @('sprint17-body-environment-restored', 'sprint17-body-fixture-cleanup', 'loaded-mod-version')) {
    $commandAssertions += [pscustomobject]@{ name=$name; status='PASS' }
}
$commandJson = ConvertTo-Json -InputObject $commandRows -Depth 8
$commandAssertionJson = ConvertTo-Json -InputObject $commandAssertions
$commandObserved = @(ConvertFrom-KmgSnakeCommandSliceEvidence -Json $commandJson -Assertions $commandAssertions)
Assert-Equal 8 $commandObserved.Count 'all eight native command cells collected separately'
foreach ($bad in @('null', '[]', '{}', '[', ('[' + $commandJson + ']'), $snakeJson,
    (ConvertTo-Json -InputObject $commandRows[0..6] -Depth 8),
    (ConvertTo-Json -InputObject ($commandRows + $commandRows[0]) -Depth 8))) {
    $rejected=$false
    try { $null=ConvertFrom-KmgSnakeCommandSliceEvidence -Json $bad -Assertions $commandAssertions } catch { $rejected=$true }
    Assert-True $rejected 'partial, nested, malformed or unrelated metadata is not a command pass'
}
foreach ($mutate in @(
    { param($rows) $rows[0].cell=$rows[1].cell },
    { param($rows) $rows[0].cell='salamander-rtwp-manual' },
    { param($rows) $rows[0].scope='donor research' },
    { param($rows) $rows[0].passed=$false },
    { param($rows) $rows[0].passed='true' },
    { param($rows) $rows[0].checks.contact=$false },
    { param($rows) $rows[0].checks.contact='true' },
    { param($rows) $rows[0].checks.PSObject.Properties.Remove('cleanup') },
    { param($rows) $rows[0]=$null }
)) {
    $changed=$commandJson | ConvertFrom-Json
    & $mutate $changed
    $rejected=$false
    try { $null=ConvertFrom-KmgSnakeCommandSliceEvidence -Json (ConvertTo-Json -InputObject $changed -Depth 8) -Assertions $commandAssertions } catch { $rejected=$true }
    Assert-True $rejected 'row-level pass never substitutes for exact six native requirements'
}
foreach ($mutate in @(
    { param($rows) $rows[0].name=$rows[1].name },
    { param($rows) $rows[0].status='FAIL' },
    { param($rows) $rows[0].name='foreign' },
    { param($rows) $rows[0]=$null }
)) {
    $changed=$commandAssertionJson | ConvertFrom-Json
    & $mutate $changed
    $rejected=$false
    try { $null=ConvertFrom-KmgSnakeCommandSliceEvidence -Json $commandJson -Assertions $changed } catch { $rejected=$true }
    Assert-True $rejected 'all51 exact native assertions independently mandatory'
}
foreach ($bad in @(@(), $snakeAssertions, $commandAssertions[0..49], ($commandAssertions+$commandAssertions[0]))) {
    $rejected=$false
    try { $null=ConvertFrom-KmgSnakeCommandSliceEvidence -Json $commandJson -Assertions $bad } catch { $rejected=$true }
    Assert-True $rejected 'incomplete or extra command assertion sets fail closed'
}

Assert-True (Test-KmgBatchCandidateUnavailable -FirstScenario $true -HasEvidence $false -HasDeployment $false -LauncherOutcome 'Unclean') `
    'a failed pre-launch candidate stops repeated full gates'
foreach ($case in @(
    @{ FirstScenario=$false; HasEvidence=$false; HasDeployment=$false; LauncherOutcome='Unclean' },
    @{ FirstScenario=$true; HasEvidence=$true; HasDeployment=$false; LauncherOutcome='Unclean' },
    @{ FirstScenario=$true; HasEvidence=$false; HasDeployment=$true; LauncherOutcome='Unclean' },
    @{ FirstScenario=$true; HasEvidence=$false; HasDeployment=$false; LauncherOutcome='Clean' }
)) {
    Assert-True (-not (Test-KmgBatchCandidateUnavailable @case)) `
        'an existing artifact or real current-run evidence preserves independent scenario execution'
}

# Parameter routing cannot leak a crowd or save-writing fixture scope into
# an unrelated scenario. These drive the shipped pure resolver, not tokens.
$batchNames = @('mechanics', 'crowd', 'persistence')
$parameterMap = @{ crowd = @{ creatures = 'crocodile,dire-crocodile'; quantity = 'OneD4PlusOne' }
    persistence = @{ persistenceScope = 'crocodilians' } }
$resolved = Resolve-KmgBatchScenarioParameters -Scenarios $batchNames -CurrentScenario 'crowd' -ParameterMap $parameterMap
Assert-Equal 'crocodile,dire-crocodile' $resolved.creatures 'crowd retains its exact creature scope'
Assert-Equal 2 $resolved.Count 'crowd receives only its own parameters'
$resolved.quantity = 'changed'
Assert-Equal 'OneD4PlusOne' $parameterMap.crowd.quantity 'returned parameters are a defensive copy'
$resolved = Resolve-KmgBatchScenarioParameters -Scenarios $batchNames -CurrentScenario 'mechanics' -ParameterMap $parameterMap
Assert-Equal 0 $resolved.Count 'parameterless mechanics does not inherit another scenario scope'
$resolved = Resolve-KmgBatchScenarioParameters -Scenarios $batchNames -CurrentScenario 'persistence' -ParameterMap $parameterMap
Assert-Equal 'crocodilians' $resolved.persistenceScope 'persistence retains only its own scope'
Assert-Equal 1 $resolved.Count 'persistence scope has no crowd parameters'
$resolved = Resolve-KmgBatchScenarioParameters -Scenarios $batchNames -CurrentScenario 'crowd' -DefaultParameters @{ historical = 'retained' }
Assert-Equal 'retained' $resolved.historical 'historical common-parameter behavior is retained'
foreach ($bad in @(
    @{ CurrentScenario = 'foreign'; ParameterMap = @{} },
    @{ CurrentScenario = 'crowd'; ParameterMap = @{ foreign = @{} } },
    @{ CurrentScenario = 'crowd'; ParameterMap = @{ Crowd = @{} } },
    @{ CurrentScenario = 'crowd'; ParameterMap = @{ crowd = 'not-a-hashtable' } },
    @{ CurrentScenario = 'crowd'; ParameterMap = @{ crowd = $null } },
    @{ CurrentScenario = 'crowd'; ParameterMap = $parameterMap; DefaultParameters = @{ extra = 'mixed' } }
)) {
    $rejected = $false
    try { $null = Resolve-KmgBatchScenarioParameters -Scenarios $batchNames @bad }
    catch { $rejected = $true }
    Assert-True $rejected 'invalid or ambiguous batch parameter routing fails closed before snapshot or launch'
}

# The outer restoration wrapper must expose the guarded harness's two stage
# deadlines. A slow first OnUpdate must not be mistaken for a mechanical test
# failure merely because the aggregate timeout was raised while the startup
# watchdog silently remained at its default.
$wrapperPath = Join-Path $ScriptRoot 'Invoke-ExpandedSummoningRuntimeScenario.ps1'
$wrapperSource = Get-Content -LiteralPath $wrapperPath -Raw
foreach ($contract in @(
    '[int]$ObserverStartupTimeoutSeconds = 180',
    '[int]$CompletionTimeoutSeconds = 180',
    'ObserverStartupTimeoutSeconds = $ObserverStartupTimeoutSeconds',
    'CompletionTimeoutSeconds = $CompletionTimeoutSeconds')) {
    Assert-True ($wrapperSource.Contains($contract)) `
        "runtime restoration wrapper must preserve stage-timeout contract: $contract"
}

$enabledSettings = '{"schemaVersion":10,"expanded-summoning":true,"gunslinger":true}'
$disabledBytes = ConvertTo-KmgDisabledExpandedSummoningSettingsBytes `
    -OriginalBytes ([Text.Encoding]::UTF8.GetBytes($enabledSettings))
Assert-Equal '{"schemaVersion":10,"expanded-summoning":false,"gunslinger":true}' `
    ([Text.Encoding]::UTF8.GetString($disabledBytes)) `
    'module-off staging changes only the one explicit JSON Boolean'
foreach ($invalidSettings in @(
    '{"schemaVersion":10,"gunslinger":true}',
    '{"schemaVersion":10,"expanded-summoning":false}',
    '{"schemaVersion":10,"expanded-summoning":"true"}',
    '{"expanded-summoning":true,"expanded-summoning":true}')) {
    $rejected = $false
    try {
        ConvertTo-KmgDisabledExpandedSummoningSettingsBytes `
            -OriginalBytes ([Text.Encoding]::UTF8.GetBytes($invalidSettings)) |
            Out-Null
    }
    catch { $rejected = $true }
    Assert-True $rejected 'module-off staging rejects absent, disabled, mistyped or duplicate settings'
}

$scenario = 'disposable-expanded-summoning'
$started = [DateTime]::SpecifyKind(
    [DateTime]::ParseExact('20260923T163838', 'yyyyMMddTHHmmss',
        [Globalization.CultureInfo]::InvariantCulture), 'Utc')

# ---------------------------------------------------------------- isolation --
# A batch runs several scenarios against one deployment, and the evidence root
# holds every run the machine has ever done.
$directories = @(
    '20260826T101500000000Z-disposable-expanded-summoning',
    '20260923T163830000000Z-disposable-expanded-summoning-player-path',
    '20260923T163840000000Z-disposable-expanded-summoning',
    '20260923T163845000000Z-disposable-expanded-summoning-visual-contracts',
    'not-an-evidence-directory'
)
Assert-Equal '20260923T163840000000Z-disposable-expanded-summoning' `
    (Select-KmgScenarioEvidence -DirectoryNames $directories -Scenario $scenario `
        -StartedUtc $started) `
    'evidence selection must pick this scenario from this run'

# A longer scenario name must not answer for a shorter one it starts with.
Assert-Equal '20260923T163830000000Z-disposable-expanded-summoning-player-path' `
    (Select-KmgScenarioEvidence -DirectoryNames $directories `
        -Scenario 'disposable-expanded-summoning-player-path' `
        -StartedUtc $started) `
    'evidence selection must match the whole scenario suffix'

# Two runs of the same scenario in one batch: the later one wins.
Assert-Equal '20260923T164510000000Z-disposable-expanded-summoning' `
    (Select-KmgScenarioEvidence -DirectoryNames @(
        '20260923T163840000000Z-disposable-expanded-summoning',
        '20260923T164510000000Z-disposable-expanded-summoning') `
        -Scenario $scenario -StartedUtc $started) `
    'the most recent qualifying directory must win'

# ------------------------------------------------------ stale-result rejection
# The defect this exists for: a wrapper adopted a month-old directory for a
# scenario that had never run, and reported it as a pass.
Assert-Equal $null `
    (Select-KmgScenarioEvidence `
        -DirectoryNames @('20260826T101500000000Z-disposable-expanded-summoning') `
        -Scenario $scenario -StartedUtc $started) `
    'evidence older than this run must be refused'

Assert-Equal '20260923T163730000000Z-disposable-expanded-summoning' `
    (Select-KmgScenarioEvidence `
        -DirectoryNames @('20260923T163730000000Z-disposable-expanded-summoning') `
        -Scenario $scenario -StartedUtc $started) `
    'a directory stamped 68s before the start is inside the 90s tolerance'
Assert-Equal $null `
    (Select-KmgScenarioEvidence `
        -DirectoryNames @('20260923T163600000000Z-disposable-expanded-summoning') `
        -Scenario $scenario -StartedUtc $started) `
    'a directory stamped 158s before the start is outside the tolerance'

Assert-Equal $null `
    (Select-KmgScenarioEvidence -DirectoryNames @() -Scenario $scenario `
        -StartedUtc $started) `
    'an empty evidence root yields nothing rather than throwing'

Assert-Equal 'NO-EVIDENCE' (Resolve-KmgScenarioOutcome -LauncherOutcome 'Clean' `
    -EvidenceStatus $null) 'exit zero without evidence is not a pass'
Assert-Equal 'ERROR' (Resolve-KmgScenarioOutcome -LauncherOutcome 'Unclean' `
    -EvidenceStatus $null) 'an errored run without evidence stays an error'

$temporary = Join-Path ([IO.Path]::GetTempPath()) ('kmg-orchestration-' +
    [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $temporary | Out-Null
try {
    # --------------------------------------------------------- parse failure --
    Assert-Equal $null (Read-KmgScenarioStatus `
        -ResultPath (Join-Path $temporary 'absent.json') -Scenario $scenario) `
        'a missing result file yields no status'

    $malformed = Join-Path $temporary 'malformed.json'
    Set-Content -LiteralPath $malformed -Value '{ "scenario": ' -Encoding UTF8
    Assert-Equal $null (Read-KmgScenarioStatus -ResultPath $malformed `
        -Scenario $scenario) 'truncated JSON yields no status rather than throwing'

    $empty = Join-Path $temporary 'empty.json'
    Set-Content -LiteralPath $empty -Value '' -Encoding UTF8
    Assert-Equal $null (Read-KmgScenarioStatus -ResultPath $empty `
        -Scenario $scenario) 'an empty result file yields no status'

    $statusless = Join-Path $temporary 'statusless.json'
    Set-Content -LiteralPath $statusless -Encoding UTF8 `
        -Value ('{ "scenario": "' + $scenario + '" }')
    Assert-Equal $null (Read-KmgScenarioStatus -ResultPath $statusless `
        -Scenario $scenario) 'a result without a status field yields no status'

    # Result isolation one level down: a correctly formed result belonging to a
    # different scenario must not be adopted.
    $foreign = Join-Path $temporary 'foreign.json'
    Set-Content -LiteralPath $foreign -Encoding UTF8 `
        -Value '{ "scenario": "mod-load-smoke", "status": "PASS" }'
    Assert-Equal $null (Read-KmgScenarioStatus -ResultPath $foreign `
        -Scenario $scenario) "another scenario's result must not be adopted"

    $good = Join-Path $temporary 'good.json'
    Set-Content -LiteralPath $good -Encoding UTF8 `
        -Value ('{ "scenario": "' + $scenario + '", "status": "PASS" }')
    Assert-Equal 'PASS' (Read-KmgScenarioStatus -ResultPath $good `
        -Scenario $scenario) 'a well formed matching result is read'

    # --------------------------------------------------- outcome reconciliation
    Assert-Equal 'PASS' (Resolve-KmgScenarioOutcome -LauncherOutcome 'Clean' `
        -EvidenceStatus 'PASS') 'both green is a pass'
    Assert-Equal 'FAIL' (Resolve-KmgScenarioOutcome -LauncherOutcome 'Clean' `
        -EvidenceStatus 'FAIL') 'the scenario decides, not the exit code'
    # The defect the extracted rules corrected: the wrapper's inline logic only
    # moved an outcome away from PASS when evidence was absent, so a scenario
    # that recorded FAIL while the launcher exited zero was counted as a pass.
    Assert-True (Test-KmgScenarioOutcomeFailed (Resolve-KmgScenarioOutcome `
        -LauncherOutcome 'Clean' -EvidenceStatus 'FAIL')) `
        ('a recorded FAIL counts against the batch even when the launcher ' +
         'exited cleanly')
    Assert-Equal 'PASS-WITH-TEARDOWN-FAULT' (Resolve-KmgScenarioOutcome `
        -LauncherOutcome 'Unclean' -EvidenceStatus 'PASS') `
        'a completed scenario that faulted during teardown is not a failure'
    Assert-Equal 'FAIL' (Resolve-KmgScenarioOutcome -LauncherOutcome 'Unclean' `
        -EvidenceStatus 'FAIL') 'a failed scenario stays failed'
    Assert-True (Test-KmgScenarioOutcomeFailed 'NO-EVIDENCE') `
        'no evidence counts against the batch'
    Assert-True (Test-KmgScenarioOutcomeFailed 'FAIL') 'a failure counts'
    Assert-True (-not (Test-KmgScenarioOutcomeFailed 'PASS-WITH-TEARDOWN-FAULT')) `
        'a teardown fault does not count as a scenario failure'

    # ----------------------------------------------------------- lock ownership
    # A batch holds the runtime exclusively, so a lock stamped inside its own
    # window is its own. The earlier rule compared the snapshot stamp as a
    # substring and missed by one second, which left a batch unable to release
    # its own lock and its restoration failed with the test build still live.
    $batchStart = [DateTime]::SpecifyKind(
        [DateTime]::ParseExact('20260923T195801', 'yyyyMMddTHHmmss',
            [Globalization.CultureInfo]::InvariantCulture), 'Utc')
    $batchNow = $batchStart.AddMinutes(7)
    Assert-True (Test-KmgCompatibilityLockOwned `
        -LockOwner 'runtime-20260923T195802Z-6fd7fa93a51f' `
        -BatchStartedUtc $batchStart -NowUtc $batchNow) `
        'a lock stamped one second after the batch started is its own'
    Assert-True (Test-KmgCompatibilityLockOwned `
        -LockOwner 'runtime-20260923T200341Z-abc' `
        -BatchStartedUtc $batchStart -NowUtc $batchNow) `
        'a lock stamped mid-batch is its own'
    Assert-True (-not (Test-KmgCompatibilityLockOwned `
        -LockOwner 'runtime-20260923T193000Z-abc' `
        -BatchStartedUtc $batchStart -NowUtc $batchNow)) `
        "a lock predating the batch belongs to someone else"
    Assert-True (-not (Test-KmgCompatibilityLockOwned `
        -LockOwner 'runtime-20260923T210000Z-abc' `
        -BatchStartedUtc $batchStart -NowUtc $batchNow)) `
        'a lock stamped after now is not this batch'
    Assert-True (-not (Test-KmgCompatibilityLockOwned -LockOwner '' `
        -BatchStartedUtc $batchStart -NowUtc $batchNow)) `
        'an empty lock file is not owned'
    Assert-True (-not (Test-KmgCompatibilityLockOwned -LockOwner 'no-timestamp' `
        -BatchStartedUtc $batchStart -NowUtc $batchNow)) `
        'a lock with no parseable stamp is not owned'

    # ------------------------------------ successful transaction and restoration
    # The fingerprint function is the shipped one; the copy is a stand-in,
    # because the shipped restore will not operate on a fixture. What this
    # establishes is the fingerprint contract the wrapper's verdicts rest on.
    $live = Join-Path $temporary 'live'
    $snapshot = Join-Path $temporary 'snapshot'
    New-Item -ItemType Directory -Path (Join-Path $live 'nested') | Out-Null
    Set-Content -LiteralPath (Join-Path $live 'Info.json') -Encoding UTF8 `
        -Value '{ "Version": "0.0.136" }'
    Set-Content -LiteralPath (Join-Path $live 'nested\asset.txt') -Encoding UTF8 `
        -Value 'original'
    $before = Get-KmgTreeFingerprint -Directory $live
    Assert-Equal 2 $before.Files 'the fixture tree has two files'
    Copy-Item -LiteralPath $live -Destination $snapshot -Recurse

    # A moved file must change the fingerprint even though the content set is
    # identical; that is why the digest covers relative paths and not just
    # hashes.
    Move-Item -LiteralPath (Join-Path $live 'nested\asset.txt') `
        -Destination (Join-Path $live 'asset.txt')
    Assert-True ((Get-KmgTreeFingerprint -Directory $live).Sha256 -cne `
        $before.Sha256) 'moving a file changes the fingerprint'
    Move-Item -LiteralPath (Join-Path $live 'asset.txt') `
        -Destination (Join-Path $live 'nested\asset.txt')
    Assert-Equal $before.Sha256 (Get-KmgTreeFingerprint -Directory $live).Sha256 `
        'moving it back restores the fingerprint'

    Set-Content -LiteralPath (Join-Path $live 'nested\asset.txt') -Encoding UTF8 `
        -Value 'deployed'
    Set-Content -LiteralPath (Join-Path $live 'extra.txt') -Encoding UTF8 `
        -Value 'deployed only'
    $deployed = Get-KmgTreeFingerprint -Directory $live
    Assert-True (Resolve-KmgRestorationNeeded -BeforeSha256 $before.Sha256 `
        -AfterScenarioSha256 $deployed.Sha256) 'a changed tree needs restoration'
    Assert-True (-not (Resolve-KmgRestorationNeeded `
        -BeforeSha256 $before.Sha256 -AfterScenarioSha256 $before.Sha256)) `
        'an unchanged tree needs no restoration'

    function Restore-KmgFixtureTree {
        param([string]$From, [string]$To)
        if (-not (Test-Path -LiteralPath $From -PathType Container)) {
            throw "Fixture snapshot is missing: $From"
        }
        Remove-Item -LiteralPath $To -Recurse -Force
        Copy-Item -LiteralPath $From -Destination $To -Recurse
    }

    Restore-KmgFixtureTree -From $snapshot -To $live
    $restored = Get-KmgTreeFingerprint -Directory $live
    Assert-Equal 'verified' (Resolve-KmgRestorationResult `
        -BeforeSha256 $before.Sha256 -RestoredSha256 $restored.Sha256) `
        'restoring the snapshot returns the exact pre-run tree'
    Assert-True (-not (Test-Path -LiteralPath (Join-Path $live 'extra.txt'))) `
        'a file the deployment added is gone after restoration'

    # ------------------------------------------------------- restoration failure
    Set-Content -LiteralPath (Join-Path $live 'nested\asset.txt') -Encoding UTF8 `
        -Value 'drifted'
    Assert-Equal 'MISMATCH' (Resolve-KmgRestorationResult `
        -BeforeSha256 $before.Sha256 `
        -RestoredSha256 (Get-KmgTreeFingerprint -Directory $live).Sha256) `
        'a tree that does not match the snapshot is a mismatch, not a pass'

    $restoreFailed = $false
    try { Restore-KmgFixtureTree -From (Join-Path $temporary 'no-such') -To $live }
    catch { $restoreFailed = $true }
    Assert-True $restoreFailed 'restoring from a missing snapshot fails loudly'
    Assert-True (Test-Path -LiteralPath (Join-Path $live 'Info.json')) `
        'a failed restore leaves the tree in place rather than emptying it'

    # ------------------------------------------------------ interrupted operation
    # The wrapper restores in a finally block so an interruption mid-batch still
    # returns the tree. This reproduces that shape and asserts both halves: the
    # interruption still propagates, and the tree still came back.
    Set-Content -LiteralPath (Join-Path $live 'nested\asset.txt') -Encoding UTF8 `
        -Value 'deployed again'
    $interrupted = $false
    try {
        try { throw 'simulated interruption' }
        finally { Restore-KmgFixtureTree -From $snapshot -To $live }
    }
    catch { $interrupted = $true }
    Assert-True $interrupted 'the interruption still propagates to the caller'
    Assert-Equal 'verified' (Resolve-KmgRestorationResult `
        -BeforeSha256 $before.Sha256 `
        -RestoredSha256 (Get-KmgTreeFingerprint -Directory $live).Sha256) `
        'an interrupted batch still restores the tree'
}
finally {
    Remove-Item -LiteralPath $temporary -Recurse -Force -ErrorAction SilentlyContinue
}

Write-Host "Expanded Summoning orchestration tests passed: $script:Passed assertions."
