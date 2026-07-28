Set-StrictMode -Version Latest

$script:KmgRuntimeEvidenceRoot = 'C:\Dev\KingmakerGunslingerLab\runtime-evidence'
$script:KmgRuntimeScenarios = @('mod-load-smoke')
$script:KmgSteamAppId = 640820
$script:KmgSteamExecutable = 'C:\Program Files (x86)\Steam\steam.exe'

function Assert-KmgRuntimeEvidenceDirectory {
    param([Parameter(Mandatory = $true)][string]$Path)
    $root = [IO.Path]::GetFullPath($script:KmgRuntimeEvidenceRoot).TrimEnd('\')
    $full = [IO.Path]::GetFullPath($Path).TrimEnd('\')
    if (-not $full.StartsWith($root + '\', [StringComparison]::OrdinalIgnoreCase)) {
        throw "Runtime evidence directory must be beneath $root"
    }
    return $full
}

function New-KmgRuntimeRequest {
    param(
        [Parameter(Mandatory = $true)][string]$Scenario,
        [Parameter(Mandatory = $true)][string]$ExpectedVersion,
        [Parameter(Mandatory = $true)][int]$TimeoutSeconds,
        [Parameter(Mandatory = $true)][bool]$ExitAfterCompletion,
        [Parameter(Mandatory = $true)][string]$EvidenceDirectory,
        [hashtable]$Parameters = @{}
    )
    if ($Scenario -notin $script:KmgRuntimeScenarios) {
        throw "Scenario is not allowlisted: $Scenario"
    }
    if ([string]::IsNullOrWhiteSpace($ExpectedVersion)) {
        throw 'ExpectedVersion is required.'
    }
    if ($TimeoutSeconds -lt 5 -or $TimeoutSeconds -gt 1800) {
        throw 'TimeoutSeconds must be from 5 through 1800.'
    }
    if ($Parameters.Count -ne 0) {
        throw "Scenario '$Scenario' does not accept parameters."
    }
    $evidence = Assert-KmgRuntimeEvidenceDirectory -Path $EvidenceDirectory
    $runId = [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffffffZ') + '-' +
        [Guid]::NewGuid().ToString('N')
    return [ordered]@{
        schemaVersion = 1
        enabled = $true
        runId = $runId
        scenario = $Scenario
        expectedModVersion = $ExpectedVersion
        evidenceDirectory = $evidence
        timeoutSeconds = $TimeoutSeconds
        exitAfterCompletion = $ExitAfterCompletion
        parameters = [ordered]@{}
    }
}

function Write-KmgUtf8NoBom {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Content
    )
    [IO.File]::WriteAllText($Path, $Content, [Text.UTF8Encoding]::new($false))
}

function Assert-KmgSteamAppId {
    param([Parameter(Mandatory = $true)][int]$AppId)
    if ($AppId -ne $script:KmgSteamAppId) {
        throw "Steam App ID must be exactly $($script:KmgSteamAppId); received $AppId."
    }
}

function Assert-KmgSteamExecutable {
    param([Parameter(Mandatory = $true)][string]$SteamPath)
    $expected = [IO.Path]::GetFullPath($script:KmgSteamExecutable)
    $actual = [IO.Path]::GetFullPath($SteamPath)
    if (-not $actual.Equals($expected, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Steam executable must be exactly: $expected"
    }
    if (-not (Test-Path -LiteralPath $actual -PathType Leaf)) {
        throw "Steam executable is missing: $actual"
    }
    return $actual
}

function Assert-KmgUnelevated {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = [Security.Principal.WindowsPrincipal]::new($identity)
    if ($principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        throw 'Runtime tests must run without administrator elevation.'
    }
}

function Get-KmgProcessOwner {
    param([Parameter(Mandatory = $true)][int]$ProcessId)
    $instance = Get-CimInstance Win32_Process -Filter "ProcessId = $ProcessId"
    if (-not $instance) { throw "Process disappeared before its owner could be verified: PID=$ProcessId" }
    $owner = Invoke-CimMethod -InputObject $instance -MethodName GetOwner
    if ($owner.ReturnValue -ne 0 -or [string]::IsNullOrWhiteSpace($owner.User)) {
        throw "Unable to verify the Windows user for PID=$ProcessId."
    }
    return "$($owner.Domain)\$($owner.User)"
}

function Assert-KmgProcessOwner {
    param(
        [Parameter(Mandatory = $true)][int]$ProcessId,
        [Parameter(Mandatory = $true)][string]$ExpectedOwner,
        [Parameter(Mandatory = $true)][string]$Label
    )
    $actual = Get-KmgProcessOwner -ProcessId $ProcessId
    if (-not $actual.Equals($ExpectedOwner, [StringComparison]::OrdinalIgnoreCase)) {
        throw "$Label is running as a different Windows user."
    }
}

function Get-KmgSteamLaunchArguments {
    param(
        [Parameter(Mandatory = $true)][int]$AppId,
        [string]$RequestPath
    )
    Assert-KmgSteamAppId -AppId $AppId
    $arguments = @('-applaunch', $AppId.ToString([Globalization.CultureInfo]::InvariantCulture))
    if (-not [string]::IsNullOrWhiteSpace($RequestPath)) {
        $safePath = Assert-KmgPathWithin -Path $RequestPath -Root $script:KmgRuntimeEvidenceRoot
        if (-not [IO.Path]::IsPathRooted($safePath) -or $safePath.Contains('"')) {
            throw 'The runtime request path cannot be quoted safely.'
        }
        $arguments += @('-kmgRuntimeTestRequest', "`"$safePath`"")
    }
    return $arguments
}

function Wait-KmgSteamProcess {
    param(
        [Parameter(Mandatory = $true)][string]$SteamPath,
        [ValidateRange(1, 300)][int]$TimeoutSeconds = 60
    )
    $SteamPath = Assert-KmgSteamExecutable -SteamPath $SteamPath
    $steam = @(Get-Process -Name steam -ErrorAction SilentlyContinue |
        Sort-Object StartTime, Id | Select-Object -First 1)
    if ($steam.Count -eq 0) {
        [void](Start-Process -FilePath $SteamPath -PassThru)
    }
    $deadline = [DateTime]::UtcNow.AddSeconds($TimeoutSeconds)
    do {
        $steam = @(Get-Process -Name steam -ErrorAction SilentlyContinue |
            Sort-Object StartTime, Id | Select-Object -First 1)
        if ($steam.Count -eq 1) { return $steam[0] }
        Start-Sleep -Milliseconds 250
    } while ([DateTime]::UtcNow -lt $deadline)
    throw "Steam client process did not become available within $TimeoutSeconds seconds."
}

function Select-KmgNewKingmakerProcess {
    param(
        [Parameter(Mandatory = $true)][object[]]$Processes,
        [Parameter(Mandatory = $true)][int[]]$ExistingProcessIds,
        [Parameter(Mandatory = $true)][DateTime]$RequestedAtUtc
    )
    $matches = @($Processes | Where-Object {
        $_.ProcessName -eq 'Kingmaker' -and
        $_.Id -notin $ExistingProcessIds -and
        $_.StartTime.ToUniversalTime() -ge $RequestedAtUtc.AddSeconds(-2)
    } | Sort-Object StartTime, Id)
    if ($matches.Count -gt 1) {
        throw "More than one newly launched Kingmaker process was found: $($matches.Id -join ', ')."
    }
    if ($matches.Count -eq 1) { return $matches[0] }
    return $null
}

function Start-KmgSteamKingmaker {
    param(
        [Parameter(Mandatory = $true)][string]$SteamPath,
        [Parameter(Mandatory = $true)][int]$AppId,
        [string]$RequestPath,
        [ValidateRange(1, 300)][int]$SteamStartupTimeoutSeconds = 60,
        [ValidateRange(1, 300)][int]$GameStartupTimeoutSeconds = 60
    )
    Assert-KmgSteamAppId -AppId $AppId
    Assert-KmgUnelevated
    Assert-KmgNotRunning
    $currentOwner = [Security.Principal.WindowsIdentity]::GetCurrent().Name
    $steam = Wait-KmgSteamProcess -SteamPath $SteamPath -TimeoutSeconds $SteamStartupTimeoutSeconds
    if ($steam.Path -and
        -not $steam.Path.Equals(
            [IO.Path]::GetFullPath($script:KmgSteamExecutable),
            [StringComparison]::OrdinalIgnoreCase)) {
        throw 'The available Steam process does not use the approved Steam executable.'
    }
    Assert-KmgProcessOwner -ProcessId $steam.Id -ExpectedOwner $currentOwner -Label 'Steam'
    $existing = @(Get-Process -Name Kingmaker -ErrorAction SilentlyContinue | ForEach-Object Id)
    $arguments = @(Get-KmgSteamLaunchArguments -AppId $AppId -RequestPath $RequestPath)
    $requestedAt = [DateTime]::UtcNow
    [void](Start-Process -FilePath $SteamPath -ArgumentList $arguments -PassThru)
    $deadline = $requestedAt.AddSeconds($GameStartupTimeoutSeconds)
    do {
        $game = Select-KmgNewKingmakerProcess `
            -Processes @(Get-Process -Name Kingmaker -ErrorAction SilentlyContinue) `
            -ExistingProcessIds $existing -RequestedAtUtc $requestedAt
        if ($game) {
            Assert-KmgProcessOwner -ProcessId $game.Id -ExpectedOwner $currentOwner -Label 'Kingmaker'
            return [ordered]@{
                steamExecutable = (Resolve-Path -LiteralPath $SteamPath).Path
                steamAppId = $AppId
                sanitizedLaunchArguments = if ($RequestPath) {
                    '-applaunch 640820 -kmgRuntimeTestRequest "<approved-evidence-path>"'
                } else {
                    '-applaunch 640820'
                }
                steamProcessId = $steam.Id
                kingmakerProcess = $game
                kingmakerProcessId = $game.Id
                kingmakerStartedAtUtc = $game.StartTime.ToUniversalTime()
            }
        }
        Start-Sleep -Milliseconds 250
    } while ([DateTime]::UtcNow -lt $deadline)
    throw "Kingmaker did not start through Steam App ID $AppId within $GameStartupTimeoutSeconds seconds; direct-executable fallback is disabled."
}
