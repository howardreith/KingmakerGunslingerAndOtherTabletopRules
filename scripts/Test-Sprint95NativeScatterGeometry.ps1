[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$path = Join-Path $root 'src\KingmakerGunslinger\Scatter\NativeScatterConeTargetResolver.cs'
$source = Get-Content -Raw -LiteralPath $path
$required = @(
    'Distance.ResolveBlunderbuss(',
    'new Feet(distance.DistanceFeet)',
    '"WouldTargetUnitCone", BindingFlags.Static | BindingFlags.NonPublic',
    'caster.EyePosition, direction,',
    'distance.DistanceMeters',
    'ReferenceEquals(candidate, caster)',
    'new HashSet<UnitEntityData>()',
    'direction.sqrMagnitude <= 0f',
    'method.ReturnType != typeof(bool)'
)
foreach ($token in $required) {
    if (-not $source.Contains($token)) { throw "Missing native scatter geometry token: $token" }
}
if ($source.Contains('Vector3.Distance(') -or $source.Contains('Math.Atan')) {
    throw 'Native scatter geometry was replaced by project-owned geometry.'
}
Write-Output 'Sprint 95 native scatter geometry contract passed.'
