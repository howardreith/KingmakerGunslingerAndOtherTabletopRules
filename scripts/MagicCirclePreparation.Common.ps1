Set-StrictMode -Version Latest

function Get-KmgMagicCircleBytesHash([byte[]]$Bytes) {
    $hash = [Security.Cryptography.SHA256]::Create()
    try { return ([BitConverter]::ToString($hash.ComputeHash($Bytes))).Replace('-', '').ToLowerInvariant() }
    finally { $hash.Dispose() }
}

function ConvertFrom-KmgCircleBindingJson([string]$Json) {
    # ConvertFrom-Json alone accepts duplicate members on Windows PowerShell.
    # Tokenize strings as whole tokens so braces/colons inside them cannot
    # masquerade as structure. Decode property escapes with the JSON parser.
    $pattern = '"(?:\\.|[^"\\])*"|[{}\[\],:]|-?(?:0|[1-9][0-9]*)(?:\.[0-9]+)?(?:[eE][+-]?[0-9]+)?|true|false|null'
    $tokens = [regex]::Matches($Json, $pattern)
    $objects = [Collections.Generic.Stack[object]]::new()
    $offset = 0
    for ($index=0; $index -lt $tokens.Count; $index++) {
        $token=$tokens[$index]
        if (-not [string]::IsNullOrWhiteSpace($Json.Substring($offset, $token.Index-$offset))) { throw 'Nonstandard preparation JSON.' }
        $offset=$token.Index+$token.Length
        if ($token.Value -ceq '{') { $objects.Push([Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)) }
        elseif ($token.Value -ceq '}') { if ($objects.Count -eq 0) { throw 'Malformed preparation object.' }; [void]$objects.Pop() }
        elseif ($token.Value.StartsWith('"') -and $index+1 -lt $tokens.Count -and $tokens[$index+1].Value -ceq ':') {
            $key=('{' + '"key":' + $token.Value + '}') | ConvertFrom-Json
            if ($objects.Count -eq 0 -or -not $objects.Peek().Add([string]$key.key)) { throw 'Duplicate preparation JSON member.' }
        }
    }
    if ($objects.Count -ne 0 -or -not [string]::IsNullOrWhiteSpace($Json.Substring($offset))) { throw 'Malformed preparation JSON.' }
    return ($Json | ConvertFrom-Json)
}

function Read-KmgMagicCirclePreparationBinding([string]$Binding, [string]$ExpectedVersion) {
    if ([string]::IsNullOrWhiteSpace($Binding) -or $Binding.Length -gt 1048576) { throw 'Missing or oversized Magic Circle preparation binding.' }
    $value = ConvertFrom-KmgCircleBindingJson $Binding
    if ((($value.PSObject.Properties.Name | Sort-Object) -join ',') -cne 'consumerArtifact,producerIdentityBase64,producerIdentitySha256,recordBase64,recordSha256,resultBase64,resultSha256,schemaVersion' -or $value.schemaVersion -ne 2) {
        throw 'Malformed Magic Circle preparation envelope.'
    }
    $decoded = @{}
    foreach ($prefix in @('record','result','producerIdentity')) {
        $hash = $value.($prefix + 'Sha256')
        if ($hash -isnot [string] -or $hash.Length -ne 64 -or $hash -cnotmatch '^[a-f0-9]{64}$') { throw 'Malformed preparation hash.' }
        $bytes = [Convert]::FromBase64String($value.($prefix + 'Base64'))
        if ($bytes.Length -eq 0 -or $bytes.Length -gt 524288 -or (Get-KmgMagicCircleBytesHash $bytes) -cne $hash) { throw 'Preparation bytes/hash mismatch.' }
        $decoded[$prefix] = ConvertFrom-KmgCircleBindingJson ([Text.UTF8Encoding]::new($false, $true).GetString($bytes).TrimStart([char]0xFEFF))
    }
    $record = $decoded.record; $result = $decoded.result
    if ($record.schemaVersion -ne 2 -or $record.phase -cne 'prepare' -or $null -ne $record.exception -or
        $record.runId -cnotmatch '^[A-Za-z0-9._-]{1,100}$' -or $record.runId -cne $result.runId -or
        $result.status -cne 'PASS' -or $result.scenario -cne 'working-save-magic-circle-prepare' -or
        $result.loadedModVersion -cne $ExpectedVersion -or $record.artifact.version -cne $ExpectedVersion -or
        $record.artifact.gitCommit -cne $result.gitCommit -or $record.workingSave.Name -cne 'KMG_AUTOMATION_WORKING' -or
        $result.automaticExitRequested -ne $true -or $result.automaticExitInitiated -ne $true -or
        @($result.assertions).Count -eq 0 -or @($result.assertions | Where-Object status -CNE 'PASS').Count -ne 0) {
        throw 'Preparation record/result identity or successful completion is invalid.'
    }
    foreach ($artifact in @($record.artifact,$value.consumerArtifact)) {
        $mvid=[Guid]::Empty
        if ((($artifact.PSObject.Properties.Name | Sort-Object) -join ',') -cne 'dllSha256,gitCommit,mvid,version' -or
            $artifact.version -cne $ExpectedVersion -or $artifact.dllSha256 -cnotmatch '^[a-f0-9]{64}$' -or
            $artifact.gitCommit -cnotmatch '^[a-f0-9]{40}$' -or -not [Guid]::TryParse([string]$artifact.mvid,[ref]$mvid)) {
            throw 'Malformed producer or consumer artifact identity.'
        }
    }
    $producer=$decoded.producerIdentity
    if ($producer.semanticVersion -cne $record.artifact.version -or $producer.loadedModuleSha256 -cne $record.artifact.dllSha256 -or
        $producer.moduleVersionId -cne $record.artifact.mvid -or $producer.gitCommit -cne $record.artifact.gitCommit) {
        throw 'Preparation record differs from its actual loaded producer identity.'
    }
    $guard = $result.workingSaveSmoke
    if ($guard.descriptorReferenceCorrelated -ne $true -or $guard.completionCallbackObserved -ne $true -or
        $guard.saveWritingApiObserved -ne $false -or $guard.hooksRemoved -ne $true -or
        $guard.expectedWorkingSaveRoutineCount -ne 1 -or $guard.expectedWorkingStashedAreaCount -lt 1) { throw 'Preparation write guard did not qualify.' }
    foreach ($key in @('Name','FileName','FolderName','GameName','GameId','Area')) {
        if ([string]::IsNullOrWhiteSpace($record.workingSave.$key) -or
            @($guard.resolvedDescriptor.safeFields | Where-Object { $_.Key -ceq $key -and $_.Value -ceq $record.workingSave.$key }).Count -ne 1) {
            throw 'Preparation working-save descriptor mismatch.'
        }
    }
    $actors = @($record.fixtureIdentity.actors)
    if ($actors.Count -ne 4 -or @($actors.id | Sort-Object -Unique).Count -ne 4 -or @($record.fixtureIdentity.carriers).Count -ne 8) { throw 'Malformed prepared fixture identity.' }
    foreach ($actor in $actors) {
        $id = [Guid]::Empty
        if (-not [Guid]::TryParse([string]$actor.id, [ref]$id) -or $id -eq [Guid]::Empty -or
            [string]::IsNullOrEmpty($actor.role) -or $actor.blueprint -cnotmatch '^[a-f0-9]{32}$' -or
            $actor.books -isnot [array]) { throw 'Malformed fixture actor identity.' }
    }
    foreach ($carrier in $record.fixtureIdentity.carriers) {
        if ($carrier.bearer -cne $actors[2].id -or $carrier.caster -cnotin @($actors[0].id,$actors[1].id) -or
            $carrier.blueprint -cnotmatch '^[a-f0-9]{32}$' -or $carrier.sourceSpell -cnotmatch '^[a-f0-9]{32}$' -or
            $carrier.level -le 0 -or $carrier.endTimeTicks -le 0 -or $carrier.extend -isnot [bool]) { throw 'Malformed fixture carrier identity.' }
    }
    $fixture=$record.fixtureIdentity; $mvid=[Guid]::Empty
    if ($fixture.area -cnotmatch '^[a-f0-9]{32}$' -or $null -eq $fixture.favoredOracle -or
        $fixture.marketReceipt.TableId -cnotmatch '^[a-f0-9]{32}$' -or $fixture.inventory -isnot [array] -or
        @($fixture.control).Count -ne 1 -or $fixture.control[0].source -cne $actors[0].id -or $fixture.control[0].endTimeTicks -le 0 -or
        $record.artifact.dllSha256 -cnotmatch '^[a-f0-9]{64}$' -or $record.artifact.gitCommit -cnotmatch '^[a-f0-9]{40}$' -or
        -not [Guid]::TryParse([string]$record.artifact.mvid,[ref]$mvid)) { throw 'Malformed preparation artifact/fixture.' }
    return $record
}

function New-KmgMagicCirclePreparationBinding([string]$PreparedEvidencePath, [string]$ExpectedVersion, $Build) {
    $path = (Resolve-Path -LiteralPath $PreparedEvidencePath).Path
    $root = [IO.Path]::GetFullPath($script:KmgRuntimeEvidenceRoot).TrimEnd('\') + '\'
    if (-not $path.StartsWith($root, [StringComparison]::OrdinalIgnoreCase) -or [IO.Path]::GetFileName($path) -cne 'magic-circle-persistence.json') {
        throw 'Prepare evidence must be an exact persistence record in the guarded evidence root.'
    }
    for ($item = Get-Item -LiteralPath $path; $null -ne $item; $item = if ($item -is [IO.DirectoryInfo]) { $item.Parent } else { $item.Directory }) {
        if (($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) { throw 'Preparation evidence cannot cross a reparse point.' }
    }
    $recordBytes = [IO.File]::ReadAllBytes($path)
    $resultPath=Join-Path (Split-Path -Parent $path) 'runtime-result.json'
    $identityPath=Join-Path (Split-Path -Parent $path) 'runtime-loaded-build-identity.json'
    foreach ($sidecar in @($resultPath,$identityPath)) {
        if (-not (Test-Path -LiteralPath $sidecar -PathType Leaf) -or
            ((Get-Item -LiteralPath $sidecar).Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) { throw 'Exact preparation sidecar is missing or crosses a reparse point.' }
    }
    $resultBytes = [IO.File]::ReadAllBytes($resultPath)
    $identityBytes = [IO.File]::ReadAllBytes($identityPath)
    $binding = [ordered]@{ schemaVersion = 2; recordBase64 = [Convert]::ToBase64String($recordBytes)
        recordSha256 = Get-KmgMagicCircleBytesHash $recordBytes; resultBase64 = [Convert]::ToBase64String($resultBytes)
        resultSha256 = Get-KmgMagicCircleBytesHash $resultBytes
        producerIdentityBase64 = [Convert]::ToBase64String($identityBytes); producerIdentitySha256 = Get-KmgMagicCircleBytesHash $identityBytes
        consumerArtifact = [ordered]@{version=$ExpectedVersion;dllSha256=$Build.dllSha256;mvid=$Build.dllMvid;gitCommit=$Build.commit} } | ConvertTo-Json -Depth 4 -Compress
    [void](Read-KmgMagicCirclePreparationBinding $binding $ExpectedVersion)
    return $binding
}
