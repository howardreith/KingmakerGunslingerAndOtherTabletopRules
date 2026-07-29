Set-StrictMode -Version Latest

$script:KmgRuntimeEvidenceRoot = 'C:\Dev\KingmakerGunslingerLab\runtime-evidence'
$script:KmgRuntimeScenarios = @(
    'mod-load-smoke',
    'observe-manual-save-load',
    'observe-save-catalog-and-selection',
    'observe-save-catalog-provider',
    'observe-load-game-navigation')
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
        [int]$StartupTimeoutSeconds = 180,
        [int]$CatalogTimeoutSeconds = 0,
        [int]$SelectionTimeoutSeconds = 0,
        [int]$CompletionTimeoutSeconds = 0,
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
    if ($StartupTimeoutSeconds -lt 5 -or $StartupTimeoutSeconds -gt 600) {
        throw 'StartupTimeoutSeconds must be from 5 through 600.'
    }
    if ($Parameters.Count -ne 0) {
        throw "Scenario '$Scenario' does not accept parameters."
    }
    $isCatalog = $Scenario -in @(
        'observe-save-catalog-and-selection',
        'observe-save-catalog-provider',
        'observe-load-game-navigation')
    $isSelectionCatalog = $Scenario -eq 'observe-save-catalog-and-selection'
    if ($isCatalog -and
        ($CatalogTimeoutSeconds -lt 5 -or $CatalogTimeoutSeconds -gt 1800)) {
        throw 'Catalog scenario timeout must be from 5 through 1800.'
    }
    if (-not $isCatalog -and $CatalogTimeoutSeconds -ne 0) {
        throw 'Catalog timeout is valid only for a catalog scenario.'
    }
    if ($isSelectionCatalog -and
        ($SelectionTimeoutSeconds -lt 5 -or $SelectionTimeoutSeconds -gt 1800 -or
         $CompletionTimeoutSeconds -lt 5 -or $CompletionTimeoutSeconds -gt 1800)) {
        throw 'Catalog selection stage timeouts must be from 5 through 1800.'
    }
    if (-not $isSelectionCatalog -and
        ($SelectionTimeoutSeconds -ne 0 -or $CompletionTimeoutSeconds -ne 0)) {
        throw 'Selection and completion timeouts are valid only for the selection scenario.'
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
        startupTimeoutSeconds = $StartupTimeoutSeconds
        catalogTimeoutSeconds = $CatalogTimeoutSeconds
        selectionTimeoutSeconds = $SelectionTimeoutSeconds
        completionTimeoutSeconds = $CompletionTimeoutSeconds
        exitAfterCompletion = $ExitAfterCompletion
        parameters = [ordered]@{}
    }
}

function Write-KmgUtf8NoBom {
    param(
        [Parameter(Mandatory = $true)][AllowNull()][object]$Path,
        [Parameter(Mandatory = $true)][string]$Content
    )
    $stage = 'validate-path'
    $destination = $null
    $temporary = $null
    try {
        if ($Path -isnot [string]) {
            throw 'Path must be exactly one scalar string.'
        }
        if ([string]::IsNullOrWhiteSpace($Path)) {
            throw 'Path must not be null, empty, or whitespace.'
        }
        if ($Path.Length -ge 2 -and $Path[0] -eq '"' -and
            $Path[$Path.Length - 1] -eq '"') {
            throw 'Path must not contain literal surrounding quotes.'
        }
        if ($Path.IndexOfAny([IO.Path]::GetInvalidPathChars()) -ge 0) {
            throw 'Path contains an invalid path character.'
        }
        if ($Path -match '^[A-Za-z][A-Za-z0-9+.-]*://' -or
            $Path -match '^[A-Za-z]+::') {
            throw 'Path must be a filesystem path, not a URI or provider path.'
        }

        $destination = [IO.Path]::GetFullPath($Path)
        $fileName = [IO.Path]::GetFileName($destination)
        if ([string]::IsNullOrWhiteSpace($fileName)) {
            throw 'Path must include a filename.'
        }
        if ($fileName.IndexOfAny([IO.Path]::GetInvalidFileNameChars()) -ge 0) {
            throw 'Filename contains an invalid character.'
        }
        if (Test-Path -LiteralPath $destination -PathType Container) {
            throw 'Path resolves to a directory.'
        }

        $directory = [IO.Path]::GetDirectoryName($destination)
        if ([string]::IsNullOrWhiteSpace($directory)) {
            throw 'Path must have a valid parent directory.'
        }
        if (Test-Path -LiteralPath $directory) {
            if (-not (Test-Path -LiteralPath $directory -PathType Container)) {
                throw 'The destination parent exists but is not a directory.'
            }
        }
        else {
            $stage = 'create-parent'
            [void][IO.Directory]::CreateDirectory($directory)
        }

        $temporary = Join-Path $directory (
            ".$fileName.$([Guid]::NewGuid().ToString('N')).tmp")
        $stage = 'write-temporary'
        $bytes = [Text.UTF8Encoding]::new($false).GetBytes($Content)
        $stream = [IO.File]::Open(
            $temporary,
            [IO.FileMode]::CreateNew,
            [IO.FileAccess]::Write,
            [IO.FileShare]::None)
        try {
            $stream.Write($bytes, 0, $bytes.Length)
            $stream.Flush($true)
        }
        finally {
            $stream.Dispose()
        }

        if (Test-Path -LiteralPath $destination) {
            if (-not (Test-Path -LiteralPath $destination -PathType Leaf)) {
                throw 'Destination is not a regular file.'
            }
            $stage = 'replace-destination'
            # Windows PowerShell 5.1 coerces a direct $null argument to an
            # empty string for File.Replace. NullString.Value preserves an
            # actual null through PowerShell's .NET method binder.
            [IO.File]::Replace(
                $temporary,
                $destination,
                [Management.Automation.Language.NullString]::Value)
        }
        else {
            $stage = 'move-new-destination'
            [IO.File]::Move($temporary, $destination)
        }
    }
    catch {
        $safeDestination = if ($destination) {
            ($destination -replace '[\x00-\x1f\x7f]', '?')
        }
        elseif ($Path -is [string]) {
            ($Path -replace '[\x00-\x1f\x7f]', '?')
        }
        else {
            '<non-scalar>'
        }
        throw "Atomic write failed at stage '$stage' for destination '$safeDestination': $($_.Exception.Message)"
    }
    finally {
        if ($temporary -and (Test-Path -LiteralPath $temporary -PathType Leaf)) {
            Remove-Item -LiteralPath $temporary -Force
        }
    }
}

function Test-KmgRuntimeStageMarker {
    param(
        [Parameter(Mandatory = $true)][object]$Marker,
        [Parameter(Mandatory = $true)][string]$RunId,
        [Parameter(Mandatory = $true)][string]$Scenario,
        [Parameter(Mandatory = $true)][string]$Stage,
        [Parameter(Mandatory = $true)][string]$ExpectedVersion,
        [Parameter(Mandatory = $true)][int]$ProcessId,
        [Parameter(Mandatory = $true)][DateTime]$RequestWrittenUtc
    )
    try {
        $utc = [DateTime]::Parse($Marker.timestampUtc,
            [Globalization.CultureInfo]::InvariantCulture,
            [Globalization.DateTimeStyles]::RoundtripKind).ToUniversalTime()
        return $Marker.schemaVersion -eq 1 -and
            $Marker.runId -ceq $RunId -and $Marker.scenario -ceq $Scenario -and
            $Marker.stage -ceq $Stage -and
            $Marker.loadedModVersion -ceq $ExpectedVersion -and
            $Marker.processId -eq $ProcessId -and
            $utc -ge $RequestWrittenUtc.ToUniversalTime()
    }
    catch { return $false }
}

function Test-KmgRuntimeReadyMarker {
    param(
        [Parameter(Mandatory = $true)][object]$Marker,
        [Parameter(Mandatory = $true)][string]$RunId,
        [Parameter(Mandatory = $true)][string]$Scenario,
        [Parameter(Mandatory = $true)][string]$ExpectedVersion,
        [Parameter(Mandatory = $true)][int]$ProcessId,
        [Parameter(Mandatory = $true)][DateTime]$RequestWrittenUtc
    )
    try {
        $readyUtc = [DateTime]::Parse(
            $Marker.readinessTimestampUtc,
            [Globalization.CultureInfo]::InvariantCulture,
            [Globalization.DateTimeStyles]::RoundtripKind).ToUniversalTime()
        return $Marker.schemaVersion -eq 1 -and
            $Marker.runId -ceq $RunId -and
            $Marker.scenario -ceq $Scenario -and
            $Marker.loadedModVersion -ceq $ExpectedVersion -and
            $Marker.processId -eq $ProcessId -and
            $readyUtc -ge $RequestWrittenUtc.ToUniversalTime() -and
            @($Marker.installedObservationHookIdentifiers).Count -gt 0
    }
    catch { return $false }
}

function Initialize-KmgRuntimeTestEvidence {
    param(
        [Parameter(Mandatory = $true)][string]$EvidenceDirectory,
        [Parameter(Mandatory = $true)][Collections.IDictionary]$Request,
        [Parameter(Mandatory = $true)][AllowNull()][object]$DeploymentManifestPath
    )
    $requestPath = Join-Path $EvidenceDirectory 'runtime-request.json'
    $resultPath = Join-Path $EvidenceDirectory 'runtime-result.json'
    $orchestration = [ordered]@{
        schemaVersion = 3
        runId = $Request.runId
        status = 'PREPARING'
        startedAtUtc = [DateTime]::UtcNow.ToString('o')
        requestPath = $requestPath
        resultPath = $resultPath
        deploymentCompleted = $true
        deploymentManifestPath = '<unavailable>'
        launchBegan = $false
        saveInteractionOccurred = $false
        guardedRequestAccepted = $false
        preLaunchKingmakerProcesses = @()
    }
    try {
        if ($DeploymentManifestPath -isnot [string] -or
            [string]::IsNullOrWhiteSpace($DeploymentManifestPath)) {
            throw 'Deployment manifest path must be exactly one scalar string.'
        }
        $deploymentPath = [IO.Path]::GetFullPath($DeploymentManifestPath)
        if (-not (Test-Path -LiteralPath $deploymentPath -PathType Leaf)) {
            throw 'The completed deployment manifest is missing.'
        }
        $orchestration.deploymentManifestPath = $deploymentPath
        if (Test-Path -LiteralPath $resultPath) {
            throw 'A runtime result already exists before request creation.'
        }
        Write-KmgUtf8NoBom -Path $requestPath `
            -Content (($Request | ConvertTo-Json -Depth 8) + [Environment]::NewLine)
        $orchestration.status = 'ACTIVE'
        [void](Write-KmgOrchestrationEvidence `
            -EvidenceDirectory $EvidenceDirectory -Record $orchestration)
        return [ordered]@{
            requestPath = $requestPath
            resultPath = $resultPath
            orchestration = $orchestration
        }
    }
    catch {
        $failure = $_
        $orchestration.status = 'ERROR'
        $orchestration.completedAtUtc = [DateTime]::UtcNow.ToString('o')
        $orchestration.failingOperation = 'pre-launch-request-and-evidence-write'
        $orchestration.exception = [ordered]@{
            type = $failure.Exception.GetType().FullName
            message = $failure.Exception.Message
        }
        try {
            [void](Write-KmgOrchestrationEvidence `
                -EvidenceDirectory $EvidenceDirectory -Record $orchestration)
        }
        catch {
            throw "Pre-launch request creation failed and ERROR evidence could not be written: $($failure.Exception.Message)"
        }
        throw $failure
    }
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
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [object[]]$Processes,
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [object[]]$ExistingProcesses,
        [Parameter(Mandatory = $true)][DateTime]$RequestedAtUtc
    )
    $existingIdentities = @($ExistingProcesses | ForEach-Object {
        '{0}:{1}' -f $_.Id, $_.StartTime.ToUniversalTime().Ticks
    })
    $matches = @($Processes | Where-Object {
        $identity = '{0}:{1}' -f $_.Id, $_.StartTime.ToUniversalTime().Ticks
        $_.ProcessName -eq 'Kingmaker' -and
        $identity -notin $existingIdentities -and
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
        [AllowEmptyCollection()]
        [Diagnostics.Process[]]$PreLaunchProcesses = @(),
        [ValidateRange(1, 300)][int]$SteamStartupTimeoutSeconds = 60,
        [ValidateRange(1, 300)][int]$GameStartupTimeoutSeconds = 60
    )
    Assert-KmgSteamAppId -AppId $AppId
    Assert-KmgUnelevated
    Assert-KmgNotRunning
    $PreLaunchProcesses = @($PreLaunchProcesses)
    $preExistingKingmaker = @($PreLaunchProcesses | Where-Object {
        $_.ProcessName -eq 'Kingmaker'
    })
    if ($preExistingKingmaker.Count -ne 0) {
        throw "Kingmaker was already running before Steam launch: PID=$($preExistingKingmaker.Id -join ', ')."
    }
    $currentOwner = [Security.Principal.WindowsIdentity]::GetCurrent().Name
    $steam = Wait-KmgSteamProcess -SteamPath $SteamPath -TimeoutSeconds $SteamStartupTimeoutSeconds
    if ($steam.Path -and
        -not $steam.Path.Equals(
            [IO.Path]::GetFullPath($script:KmgSteamExecutable),
            [StringComparison]::OrdinalIgnoreCase)) {
        throw 'The available Steam process does not use the approved Steam executable.'
    }
    Assert-KmgProcessOwner -ProcessId $steam.Id -ExpectedOwner $currentOwner -Label 'Steam'
    $arguments = @(Get-KmgSteamLaunchArguments -AppId $AppId -RequestPath $RequestPath)
    $requestedAt = [DateTime]::UtcNow
    [void](Start-Process -FilePath $SteamPath -ArgumentList $arguments -PassThru)
    $deadline = $requestedAt.AddSeconds($GameStartupTimeoutSeconds)
    do {
        $game = Select-KmgNewKingmakerProcess `
            -Processes @(Get-Process -Name Kingmaker -ErrorAction SilentlyContinue) `
            -ExistingProcesses $PreLaunchProcesses -RequestedAtUtc $requestedAt
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

function Write-KmgOrchestrationEvidence {
    param(
        [Parameter(Mandatory = $true)][string]$EvidenceDirectory,
        [Parameter(Mandatory = $true)][Collections.IDictionary]$Record
    )
    $path = Join-Path $EvidenceDirectory 'orchestration.json'
    Write-KmgUtf8NoBom -Path $path `
        -Content (($Record | ConvertTo-Json -Depth 6) + [Environment]::NewLine)
    return $path
}
