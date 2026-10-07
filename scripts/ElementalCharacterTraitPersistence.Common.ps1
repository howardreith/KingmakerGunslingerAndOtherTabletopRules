Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot 'TeleportationPersistence.Common.ps1')
# Native SaveInfo receipt + deny-write handles; no ZIP/header/payload inspection.
function Write-ElementalTraitSaveJson([string]$Path,$Value) {
    Write-KmgUtf8NoBom -Path $Path -Content (ConvertTo-Json -InputObject $Value -Depth 50)
}
function New-ElementalTraitSaveParameters([string]$SaveName,[hashtable]$Parameters) {
    if($null -eq $Parameters -or $Parameters.Count -ne 2 -or
        -not $Parameters.ContainsKey('phase') -or -not $Parameters.ContainsKey('planPath') -or
        $Parameters.phase -isnot [string] -or $Parameters.planPath -isnot [string] -or
        $Parameters.phase -cnotin @('prepare','verify-remove','verify-absent')) {
        throw 'Trait persistence requires typed -SaveName plus only the closed phase and planPath.'
    }
    if(($Parameters.phase -ceq 'prepare' -and $SaveName -cne 'KMG_AUTOMATION_WORKING') -or
        ($Parameters.phase -cne 'prepare' -and $SaveName -cnotmatch '^KMG_TRAITS_0142_[0-9]{8}T[0-9]{13}Z_[a-f0-9]{32}$')) {
        throw 'Trait persistence accepts only its load-only seed or unique owned descriptor.'
    }
    return @{saveName=$SaveName;phase=$Parameters.phase;planPath=$Parameters.planPath}
}
function Assert-ElementalTraitSaveLease($Lease,[string]$Phase) {
    $s=Get-Content -LiteralPath $Lease.Path -Raw|ConvertFrom-Json
    if($null -eq $Lease.Stream -or -not $Lease.Stream.CanRead -or $s.status -cne 'Active' -or
        $s.transactionId -cne $Lease.Transaction -or $s.ownerPid -ne $PID -or
        $s.ownerStartedUtc -cne (Get-Process -Id $PID).StartTime.ToUniversalTime().ToString('o') -or
        $s.descriptor -cne $Lease.Descriptor -or $s.phase -cne $Phase -or
        [DateTime]::Parse($s.expiresUtc).ToUniversalTime() -le [DateTime]::UtcNow -or
        (Get-Content -LiteralPath $Lease.LockPath -Raw).Trim() -cne $Lease.Transaction) {throw 'Stale/foreign/closed save lease.'}
    return $s
}
function Test-ElementalTraitOwnedPath([string]$Left,[string]$Right) {
    if([string]::IsNullOrWhiteSpace($Left) -or [string]::IsNullOrWhiteSpace($Right) -or
        -not [IO.Path]::IsPathRooted($Left) -or -not [IO.Path]::IsPathRooted($Right)){return $false}
    return [string]::Equals([IO.Path]::GetFullPath($Left),[IO.Path]::GetFullPath($Right),[StringComparison]::OrdinalIgnoreCase)
}
function Assert-ElementalTraitSavePlan([string]$Path,[string]$Phase,[string]$SaveName,[string]$Version) {
    [void](Assert-KmgPathWithin -Path $Path -Root 'C:/Dev/KingmakerGunslingerLab/runtime-evidence')
    if(-not(Test-Path -LiteralPath $Path -PathType Leaf)){throw 'Existing guarded plan required.'}
    for($p=Get-Item -LiteralPath $Path;$null -ne $p;$p=if($p -is [IO.FileInfo]){$p.Directory}else{$p.Parent}){
        if($p.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'No reparse plan escape.'}
    }
    $plan=Get-Content -LiteralPath $Path -Raw|ConvertFrom-Json
    if($Phase -cnotin @('prepare','verify-remove','verify-absent') -or $plan.schemaVersion -ne 1 -or
        $plan.phase -cne $Phase -or $plan.version -cne $Version -or
        $plan.transactionId -cnotmatch '^[0-9]{8}T[0-9]{13}Z_[a-f0-9]{32}$' -or
        (Split-Path -Leaf (Split-Path -Parent $Path)) -cne ('elemental-trait-save-'+$plan.transactionId)){throw 'Exact closed transaction plan required.'}
    $name='KMG_TRAITS_0142_'+$plan.transactionId
    $inputName=if($Phase -ceq 'prepare'){'KMG_AUTOMATION_WORKING'}else{$name}
    if($SaveName -cne $inputName -or $plan.input.name -cne $inputName -or
        $plan.input.file -cnotmatch ('^Manual_[0-9]+_'+[regex]::Escape($inputName)+'\.zks$') -or
        (Get-FileHash -LiteralPath $plan.input.path -Algorithm SHA256).Hash.ToLowerInvariant() -cne $plan.input.sha256){throw 'Wrong input/hash; seed is load-only.'}
    [void](Assert-KmgPathWithin -Path $plan.leasePath -Root 'C:/Dev/KingmakerGunslingerLab/runtime-evidence')
    $s=Get-Content -LiteralPath $plan.leasePath -Raw|ConvertFrom-Json
    $owner=Get-Process -Id $s.ownerPid -ErrorAction Stop
    if($s.status -cne 'Active' -or $s.transactionId -cne $plan.transactionId -or $s.descriptor -cne $name -or
        $s.phase -cne $Phase -or $s.ownerStartedUtc -cne $owner.StartTime.ToUniversalTime().ToString('o') -or
        [DateTime]::Parse($s.expiresUtc).ToUniversalTime() -le [DateTime]::UtcNow -or
        ($Phase -cne 'prepare' -and -not (Test-ElementalTraitOwnedPath $s.ownedPath $plan.input.path))){throw 'Exact active save lease required.'}
    return $plan
}
function Register-ElementalTraitOwnedSave($Catalog,$Lease,[string]$RunDirectory,[string]$Phase) {
    $receipt=Join-Path $RunDirectory 'elemental-trait-owned-save.json'
    if(-not(Test-Path -LiteralPath $receipt -PathType Leaf)){return $null}
    $e=Get-Content -LiteralPath $receipt -Raw|ConvertFrom-Json
    $request=Get-Content -LiteralPath (Join-Path $RunDirectory 'runtime-request.json') -Raw|ConvertFrom-Json
    if($request.runId -cne $e.runId -or $request.scenario -cne 'elemental-character-traits-owned-save' -or
        $request.enabled -ne $true -or $request.parameters.phase -cne $Phase) {throw 'Receipt is not owned by this exact guarded request.'}
    $s=Assert-ElementalTraitSaveLease $Lease $Phase
    $path=[IO.Path]::GetFullPath($e.path)
    if($e.transactionId -cne $Lease.Transaction -or $e.phase -cne $Phase -or $e.name -cne $Lease.Descriptor -or
        $e.lifecycle -cne 'native-prepared-before-write' -or [IO.Path]::GetDirectoryName($path) -cne $Catalog.Directory -or
        [IO.Path]::GetFileName($path) -cnotmatch ('^Manual_[0-9]+_'+[regex]::Escape($Lease.Descriptor)+'\.zks$') -or
        @($Catalog.Files|Where-Object path -CEQ $path).Count -ne 0 -or
        $e.existedBeforePreparation -ne ($Phase -ceq 'verify-remove') -or
        ($null -ne $s.ownedPath -and $s.ownedPath -cne $path)){throw 'Foreign or ambiguous native save receipt.'}
    $prepared=[IO.Path]::GetFullPath($e.nativePreparedPath)
    if($e.nativePreparedInitiallyAbsent -ne $true -or [IO.Path]::GetDirectoryName($prepared) -cne $Catalog.Directory -or
        [IO.Path]::GetFileName($prepared) -cnotmatch ('^Manual_[0-9]+_'+[regex]::Escape($Lease.Descriptor)+'\.zks$') -or
        @($Catalog.Files|Where-Object path -CEQ $prepared).Count -ne 0 -or
        ($Phase -ceq 'prepare' -and -not (Test-ElementalTraitOwnedPath $prepared $path)) -or
        ($Phase -ceq 'verify-remove' -and (Test-ElementalTraitOwnedPath $prepared $path))){throw 'Native temporary descriptor is not the exact initially-absent owned preparation.'}
    if($Phase -ceq 'verify-remove'){
        $stageHash=$null
        if(Test-Path -LiteralPath $prepared -PathType Leaf){
            if((Get-Item -LiteralPath $prepared).Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse native staging rejected.'}
            $stageHash=(Get-FileHash -LiteralPath $prepared -Algorithm SHA256).Hash.ToLowerInvariant()
        }
        $s|Add-Member -NotePropertyName stagingPath -NotePropertyValue $prepared -Force
        $s|Add-Member -NotePropertyName stagingSha256 -NotePropertyValue $stageHash -Force
    }
    if(Test-Path -LiteralPath $path){
        if((Get-Item -LiteralPath $path).Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse save rejected.'}
        $s.ownedSha256=(Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()
    }
    $s.ownedPath=$path;Write-ElementalTraitSaveJson $Lease.Path $s
    return $path
}
function Remove-ElementalTraitOwnedSave($Catalog,$Lease,[string]$Phase) {
    $s=Assert-ElementalTraitSaveLease $Lease $Phase
    $targets=@()
    if($s.PSObject.Properties['stagingPath'] -and $null -ne $s.stagingPath){
        $targets+=@([pscustomobject]@{path=$s.stagingPath;hash=$s.stagingSha256})
    }
    if($null -ne $s.ownedPath){$targets+=@([pscustomobject]@{path=$s.ownedPath;hash=$s.ownedSha256})}
    foreach($target in $targets){
        $path=[IO.Path]::GetFullPath($target.path)
        if([IO.Path]::GetDirectoryName($path) -cne $Catalog.Directory -or
            [IO.Path]::GetFileName($path) -cnotmatch ('^Manual_[0-9]+_'+[regex]::Escape($Lease.Descriptor)+'\.zks$') -or
            @($Catalog.Files|Where-Object path -CEQ $path).Count -ne 0){throw 'Cannot delete a preexisting/nonowned save.'}
        if(Test-Path -LiteralPath $path -PathType Leaf){
            if((Get-Item -LiteralPath $path).Attributes -band [IO.FileAttributes]::ReparsePoint -or
                (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant() -cne $target.hash){throw 'Owned save changed outside exact receipt.'}
            Remove-Item -LiteralPath $path
        }
        if(Test-Path -LiteralPath $path){throw 'Owned save deletion failed.'}
    }
}
