Set-StrictMode -Version Latest

# Outer dispatch control for this single owner-bounded continuation. It never
# launches, kills or classifies a native result. Existing transaction guards run.
function Invoke-KmgSprints3031Budget {
    [CmdletBinding()]
    param([Parameter(Mandatory=$true)][string]$Path,
        [Parameter(Mandatory=$true)][ValidateSet('Check','Build','Game','Ready','Stop')][string]$Action,
        [Parameter(Mandatory=$true)][string]$Stage,
        [int]$MinimumRemainingMinutes=0)
    if(-not (Test-Path -LiteralPath $Path -PathType Leaf)) { throw 'Persistent owner budget is missing; do not recreate it.' }
    $b=Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json
    if($b.authority -cne 'Owner bounded Sprints30-31 closeout decision' -or
        $b.maximumBuildAttempts -ne 3 -or $b.maximumGameLaunches -ne 24 -or
        $b.buildAttemptCount -lt 0 -or $b.actualGameLaunchCount -lt 0) { throw 'Unexpected owner budget.' }
    $now=[DateTime]::UtcNow
    $start=[DateTime]::Parse($b.startUtc).ToUniversalTime()
    $end=[DateTime]::Parse($b.deadlineUtc).ToUniversalTime()
    $ready=[DateTime]::Parse($b.readinessDeadlineUtc).ToUniversalTime()
    $suspended=[TimeSpan]::Zero
    if($b.PSObject.Properties.Name -contains 'authorizedSuspension') {
        $pause=$b.authorizedSuspension
        # Only the specific fully suspended interval approved by this addendum.
        # Retain original consumption and ceilings; no generic pause/reset switch.
        if($pause.authority -cne 'Owner S30-31 compiler unblock addendum' -or
            $pause.pauseStartUtc -cne '2026-10-24T21:52:48.7845360Z' -or
            $pause.resumeUtc -cne '2026-10-24T23:18:09.0000000Z' -or
            $pause.excludedTicks -ne 51202154640 -or
            ([DateTime]::Parse($pause.originalDeadlineUtc).ToUniversalTime()-$start) -ne [TimeSpan]::FromHours(7) -or
            ([DateTime]::Parse($pause.originalReadinessDeadlineUtc).ToUniversalTime()-$start) -ne [TimeSpan]::FromMinutes(120)) { throw 'Unsupported owner suspension adjustment.' }
        $suspended=[TimeSpan]::FromTicks([long]$pause.excludedTicks)
    }
    if($b.PSObject.Properties.Name -contains 'compileCeilingAmendment') {
        $amendment=$b.compileCeilingAmendment
        if($amendment.authority -cne 'Owner remove compile-count bottleneck amendment' -or
            $amendment.pauseStartUtc -cne '2026-10-24T23:43:24.9945340Z' -or
            $amendment.resumeUtc -cne '2026-10-25T00:28:29.0000000Z' -or
            $amendment.excludedTicks -ne 27040054660) { throw 'Unsupported compile-ceiling suspension adjustment.' }
        $suspended=$suspended.Add([TimeSpan]::FromTicks([long]$amendment.excludedTicks))
    }
    if($end-$start-$suspended -ne [TimeSpan]::FromHours(7) -or $ready-$start-$suspended -ne [TimeSpan]::FromMinutes(120)) { throw 'Owner deadlines were changed.' }
    if($Action -ne 'Stop') {
        if($b.status -cne 'ACTIVE' -or (Test-Path -LiteralPath $b.ownerStopMarker)) { throw 'Owner CONTROL stop: no new dispatch.' }
        if($now -ge $end -or $now.AddMinutes($MinimumRemainingMinutes) -ge $end) { throw 'Elapsed budget cannot fit the next stage.' }
        if($b.currentStage -ceq 'no-game-readiness' -and $now -ge $ready) { throw 'No-game readiness deadline expired.' }
        if($b.buildAttemptCount -ge 3 -or $b.actualGameLaunchCount -ge 24) { throw 'Cumulative dispatch ceiling reached.' }
        if($Action -in @('Game','Build') -and $b.currentStage -ceq 'no-game-readiness') { throw 'Readiness has not been closed; candidate/game dispatch refused.' }
    }
    if($Action -ceq 'Check') { return $b }
    if($Action -ceq 'Ready' -and ($now -ge $ready -or $b.currentStage -cne 'no-game-readiness')) { throw 'Readiness transition is late or duplicate.' }
    if($Action -ceq 'Build') { $b.buildAttemptCount++ }
    if($Action -ceq 'Game') { $b.actualGameLaunchCount++ }
    if($Action -ceq 'Stop') { $b.status='BOUNDED_STOP' }
    $b.currentStage=$Stage
    $b.events=@($b.events)+[pscustomobject]@{utc=$now.ToString('o');action=$Action;stage=$Stage;
        builds=$b.buildAttemptCount;gameLaunches=$b.actualGameLaunchCount;controlOnly=$true}
    # Same-directory atomic replacement; preserve an immutable previous record.
    $temp=$Path+'.'+[Guid]::NewGuid().ToString('N')+'.tmp'
    $backup=$Path+'.'+$now.ToString('yyyyMMddTHHmmssfffffffZ')+'.previous'
    [IO.File]::WriteAllText($temp,($b|ConvertTo-Json -Depth 12),[Text.UTF8Encoding]::new($false))
    [IO.File]::Replace($temp,$Path,$backup)
    return $b
}
