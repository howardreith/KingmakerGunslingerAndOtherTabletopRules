[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)][string]$PackagePath,
    [Parameter(Mandatory=$true)][string]$ReferenceRoot,
    [string]$KingmakerInstallDir='C:/Program Files (x86)/Steam/steamapps/common/Pathfinder Kingmaker'
)
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot 'CompatibilityProfile.Common.ps1')
$root=Get-KmgCompatibilityRepositoryRoot
$mods=Join-Path $KingmakerInstallDir 'Mods'
if(Get-Process Kingmaker -ErrorAction SilentlyContinue){throw 'Overlay tests require no live game.'}
$lock='C:/Dev/KingmakerGunslingerLab/compatibility-state/compatibility.lock'
if(Test-Path -LiteralPath $lock){throw 'Overlay tests require no active transaction.'}
$before=@(Get-KmgCompatibilityDirectoryManifest $mods)
$installed=@(Get-ChildItem -LiteralPath $mods -Directory | Where-Object Name -cne 'KingmakerGunslinger' | ForEach-Object {
    $info=Read-KmgCompatibilityJson (Join-Path $_.FullName 'Info.json')
    [pscustomobject]@{id=$info.Id;version=$info.Version;destinationName=$_.Name}
})
$original=$installed|ConvertTo-Json -Depth 5
$overlayArguments=@{InstalledRuntimeMods=$installed;SuggestionTransition='retained';ProviderPresent=$true;
    SuggestionReferenceRoot=$ReferenceRoot;PackagePath=$PackagePath;KingmakerInstallDir=$KingmakerInstallDir;RepositoryRoot=$root}
$count=0
$reference=Resolve-KmgCompatibilityProfile -ProfileId 'gunslinger-cowwithhat-suggestion' -ReferenceRoot $ReferenceRoot -PackagePath $PackagePath -KingmakerInstallDir $KingmakerInstallDir -RepositoryRoot $root
Assert-KmgSprint32SuggestionReferenceIdentity -Resolution $reference -RepositoryRoot $root
$count++
foreach($id in @('CallOfTheWild','CowWithHatsCustomSpellsMod')) {
    # Pure admission over external assembly identity records: even matching
    # installed/reference replacement bytes cannot replace the reviewed identity.
    $changed=ConvertFrom-Json -InputObject ($reference|ConvertTo-Json -Depth 15)
    $entry=@($changed.runtimeMods|Where-Object ummId -ceq $id)[0]
    $entry.assembly.sha256=('0'*64)
    $matchingReplacementInstalledHash=$entry.assembly.sha256
    $refused=$false
    try{Assert-KmgSprint32SuggestionReferenceIdentity -Resolution $changed -RepositoryRoot $root}catch{
        if($_.Exception.Message -notlike '*investigated reference identity*'){throw};$refused=$true
    }
    if(-not $refused -or $entry.assembly.sha256 -cne $matchingReplacementInstalledHash){throw 'Matching replacement references bypassed the investigated identity.'}
    $count++
}
foreach($present in @($true,$false)) {
    $case=$overlayArguments.Clone();$case.ProviderPresent=$present
    $resolved=Resolve-KmgSprint32SuggestionOverlay @case
    if($resolved.present -ne $present -or $resolved.intakeModsPreserved -ne 10 -or
       $resolved.provider.ummId -cne 'CowWithHatsCustomSpellsMod' -or
       $resolved.dependency.assemblyName -cne 'CallOfTheWild' -or
       ($installed|ConvertTo-Json -Depth 5) -cne $original) {throw 'Exact read-only overlay resolution differs.'}
    $count++
}
foreach($fault in @('missing-intake','missing-dependency','provider-already-present','destination-collision','duplicate-id','wrong-dependency-directory')) {
    $values=ConvertFrom-Json -InputObject $original
    $dependency=@($values|Where-Object id -ceq 'CallOfTheWild')[0]
    $other=@($values|Where-Object id -cne 'CallOfTheWild')[0]
    switch($fault) {
        'missing-intake' {$values=$values[0..8]}
        'missing-dependency' {$dependency.id='NoInstalledCotW'}
        'provider-already-present' {$other.id='CowWithHatsCustomSpellsMod'}
        'destination-collision' {$other.destinationName=$resolved.provider.destinationName}
        'duplicate-id' {$other.id='CallOfTheWild'}
        'wrong-dependency-directory' {$dependency.destinationName=$other.destinationName}
    }
    $case=$overlayArguments.Clone();$case.InstalledRuntimeMods=$values
    $refused=$false
    try{[void](Resolve-KmgSprint32SuggestionOverlay @case)}catch{
        if($_.Exception.Message -notmatch 'intact identifiable|overlay identity|Installed CotW differs'){throw}
        $refused=$true
    }
    if(-not $refused){throw "Unsafe provider overlay accepted: $fault"};$count++
}
if(-not(Test-KmgCompatibilityManifestEqual $before @(Get-KmgCompatibilityDirectoryManifest $mods)) -or
   (Test-Path -LiteralPath $lock) -or (Get-Process Kingmaker -ErrorAction SilentlyContinue)) {throw 'Read-only overlay tests changed runtime intake.'}
Write-Host "Sprint32 optional overlay PASS=$count; present/absent resolution preserves intake and rejects missing/duplicate/provider/dependency collisions; no staging, save or game."
