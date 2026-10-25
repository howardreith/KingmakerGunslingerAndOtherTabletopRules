Set-StrictMode -Version Latest

# Admission only: this never runs a candidate/game or interrupts an admitted
# operation's children, evidence collection or restoration.
function Invoke-KmgSprints3031Budget {
    [CmdletBinding()]
    param([Parameter(Mandatory=$true)][string]$Path,
        [Parameter(Mandatory=$true)][ValidateSet('Check','Build','Game','Ready','Stop')][string]$Action,
        [Parameter(Mandatory=$true)][string]$Stage,
        [int]$MinimumRemainingMinutes=0)
    if(-not (Test-Path -LiteralPath $Path -PathType Leaf)) { throw 'Persistent owner budget is missing; do not recreate it.' }
    $b=Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json
    $amended=$b.PSObject.Properties.Name -contains 'budgetIsolationAmendment'
    $maximum=3
    if($amended) {
        $a=$b.budgetIsolationAmendment
        if($a.authority -cne 'Owner repair budget isolation reconcile accounting continue closeout' -or
            $a.oldMaximumBuildAttempts -ne 3 -or $a.newMaximumBuildAttempts -ne 5 -or
            $a.oldBuildAttemptCount -ne 3 -or $a.effectiveBuildAttemptCount -ne 2 -or
            [string]::IsNullOrWhiteSpace($a.reason) -or @($a.evidence).Count -lt 2 -or
            @($b.events | Where-Object { $_.action -ceq 'Build' -and $_.utc -ceq $a.nonCandidateEventUtc -and $_.builds -eq 2 }).Count -ne 1) {
            throw 'Unsupported owner budget reconciliation.'
        }
        $maximum=5
    }
    if($b.authority -cne 'Owner bounded Sprints30-31 closeout decision' -or
        $b.maximumBuildAttempts -ne $maximum -or $b.maximumGameLaunches -ne 24 -or
        $b.buildAttemptCount -lt 0 -or $b.buildAttemptCount -gt $maximum -or
        $b.actualGameLaunchCount -lt 0 -or $b.actualGameLaunchCount -gt 24 -or $MinimumRemainingMinutes -lt 0) {
        throw 'Unexpected owner budget.'
    }
    $now=[DateTime]::UtcNow
    $start=[DateTime]::Parse($b.startUtc).ToUniversalTime()
    $end=[DateTime]::Parse($b.deadlineUtc).ToUniversalTime()
    $ready=[DateTime]::Parse($b.readinessDeadlineUtc).ToUniversalTime()
    $readyEvents=@($b.events | Where-Object {$_.action -ceq 'Ready'})
    if($readyEvents.Count -gt 1) { throw 'Readiness transition is duplicated.' }
    $readyClosed=if($readyEvents.Count){[DateTime]::Parse($readyEvents[0].utc).ToUniversalTime()}else{$null}
    $suspended=[TimeSpan]::Zero;$readinessSuspended=[TimeSpan]::Zero;$lastResume=$start
    # Authorizations and their exact intervals live in the persistent record.
    # Validate chronology/durations rather than adding per-session date clauses.
    $authorities=[ordered]@{
        authorizedSuspension='Owner S30-31 compiler unblock addendum'
        compileCeilingAmendment='Owner remove compile-count bottleneck amendment'
        budgetIsolationAmendment='Owner repair budget isolation reconcile accounting continue closeout'
    }
    foreach($name in $authorities.Keys) {
        if($b.PSObject.Properties.Name -notcontains $name){continue}
        $pause=$b.$name
        $pauseStart=[DateTime]::Parse($pause.pauseStartUtc).ToUniversalTime()
        $resume=[DateTime]::Parse($pause.resumeUtc).ToUniversalTime()
        $duration=$resume-$pauseStart
        if($pause.authority -cne $authorities[$name] -or $pauseStart -lt $lastResume -or
            $resume -gt $now -or $duration.Ticks -le 0 -or $duration.Ticks -ne [long]$pause.excludedTicks) {
            throw 'Unsupported owner suspension adjustment.'
        }
        $suspended=$suspended.Add($duration)
        if($null -eq $readyClosed -or $resume -le $readyClosed){$readinessSuspended=$readinessSuspended.Add($duration)}
        elseif($pauseStart -lt $readyClosed){throw 'Suspension crosses the recorded readiness completion.'}
        $lastResume=$resume
    }
    if($end-$start-$suspended -ne [TimeSpan]::FromHours(7) -or
        $ready-$start-$readinessSuspended -ne [TimeSpan]::FromMinutes(120)) { throw 'Owner deadlines were changed.' }
    if($Action -ne 'Stop') {
        if($b.status -cne 'ACTIVE' -or (Test-Path -LiteralPath $b.ownerStopMarker)) { throw 'Owner CONTROL stop: no new dispatch.' }
        if($now -ge $end -or $now.AddMinutes($MinimumRemainingMinutes) -ge $end) { throw 'Elapsed budget cannot fit the next stage.' }
        if($b.currentStage -ceq 'no-game-readiness' -and $now -ge $ready) { throw 'No-game readiness deadline expired.' }
        if($Action -ceq 'Build' -and $b.buildAttemptCount -ge $maximum) { throw 'Candidate build admission ceiling reached.' }
        if($Action -ceq 'Game' -and $b.actualGameLaunchCount -ge 24) { throw 'Game admission ceiling reached.' }
        if($Action -in @('Game','Build') -and $b.currentStage -ceq 'no-game-readiness') { throw 'Readiness has not been closed; candidate/game dispatch refused.' }
    }
    if($Action -ceq 'Check') { return $b }
    if($Action -ceq 'Ready' -and ($now -ge $ready -or $b.currentStage -cne 'no-game-readiness' -or $readyEvents.Count)) { throw 'Readiness transition is late or duplicate.' }
    if($Action -ceq 'Build') { $b.buildAttemptCount++ }
    if($Action -ceq 'Game') { $b.actualGameLaunchCount++ }
    if($Action -ceq 'Stop') { $b.status='BOUNDED_STOP' }
    $b.currentStage=$Stage
    $b.events=@($b.events)+[pscustomobject]@{utc=$now.ToString('o');action=$Action;stage=$Stage;
        builds=$b.buildAttemptCount;gameLaunches=$b.actualGameLaunchCount;controlOnly=$true}
    $temp=$Path+'.'+[Guid]::NewGuid().ToString('N')+'.tmp'
    $backup=$Path+'.'+$now.ToString('yyyyMMddTHHmmssfffffffZ')+'.previous'
    [IO.File]::WriteAllText($temp,($b|ConvertTo-Json -Depth 20),[Text.UTF8Encoding]::new($false))
    [IO.File]::Replace($temp,$Path,$backup)
    return $b
}
