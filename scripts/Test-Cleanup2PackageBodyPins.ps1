<# No-game package body integrity regression. Executes the curated function
   defined and invoked by validate-package.ps1, with its actual hash helper.
   Only new ignored repository artifact fixtures are mutable. #>
[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'common.ps1')
$watch=[Diagnostics.Stopwatch]::StartNew()
$repositoryRoot=Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot
$validatorPath=Join-Path $PSScriptRoot 'validate-package.ps1'
$tokens=$null;$parseErrors=$null
$ast=[Management.Automation.Language.Parser]::ParseFile($validatorPath,[ref]$tokens,[ref]$parseErrors)
if(@($parseErrors).Count) {throw 'Current package validator does not parse.'}
$functions=@($ast.FindAll({param($node) $node -is [Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -ceq 'Assert-KmgProtectedBodyExports'},$true))
if($functions.Count -ne 1) {throw 'Expected exactly one production protected body validator.'}
# Load only the production function: the package entry point still requires its
# normal PackagePath, full allowlist, assembly and sound-bank validation.
Invoke-Expression $functions[0].Extent.Text
$policyNodes=@($functions[0].FindAll({param($node) $node -is [Management.Automation.Language.AssignmentStatementAst] -and $node.Left.Extent.Text -ceq '$protectedBodyExports'},$true))
if($policyNodes.Count -ne 1) {throw 'Expected one curated literal export policy.'}
$literal=$policyNodes[0].Right.Find({param($node) $node -is [Management.Automation.Language.HashtableAst]},$true)
if($null -eq $literal) {throw 'Protected export policy must remain a curated literal map.'}
$policy=$literal.SafeGetValue()
$root=Join-Path $repositoryRoot ('artifacts/cleanup2-package-body-pins/'+[Guid]::NewGuid().ToString('N'))
$modDirectory=Join-Path $root 'KingmakerGunslinger'
[void][IO.Directory]::CreateDirectory($modDirectory)
$script:checks=0;$script:failures=New-Object 'Collections.Generic.List[string]'
function Check([bool]$Value,[string]$Name) {$script:checks++;if(-not $Value){$script:failures.Add($Name)}}
function Reject-Body([string]$ExpectedPath,[string]$Name) {
    $refused=$false
    try {Assert-KmgProtectedBodyExports -ModDirectory $modDirectory}
    catch {$refused=$_.Exception.Message -clike ('*'+$ExpectedPath)}
    Check $refused $Name
}
foreach($path in $policy.Keys) {
    $source=Join-Path $repositoryRoot $path;$target=Join-Path $modDirectory $path
    [void][IO.Directory]::CreateDirectory((Split-Path -Parent $target))
    Copy-Item -LiteralPath $source -Destination $target
    Check ((Get-KmgSha256 $source) -ceq $policy[$path]) ('source-admitted-'+$path)
}
Assert-KmgProtectedBodyExports -ModDirectory $modDirectory
Check $true 'all-exact-current-pairs-admitted'

# These existing controls are deliberately unchanged by Cleanup 2 admission.
$unaffected=@{
    'assets\sprint19-fourarmed\xill-mesh.json'='d7a0cb0ade14f00e6df09bc8b5a57654083cbd1ebe23599fcdb37ec497f1a243'
    'assets\sprint19-fourarmed\xill-albedo.png'='eca0206533fad6c67cb2cbce79c5ff4eac7987b3311229c8d9f6b883522999fa'
    'assets\sprint20-arachnids\giant-scorpion-mesh.json'='1b48a74e4296f4fc0e4903b3fa9ec1b776aae1b85f512d91ebc833e3619bba7f'
    'assets\sprint20-arachnids\giant-scorpion-albedo.png'='c4a5a29d54ee52c3c1679de55a6c0f3d7e46e8ad56d02b0c25e51c43f55feb1a'
    'assets\sprint21-arthropods\bebelith-mesh.json'='7ff4e4dd8fc8edf4beb9dd983f245c329f40d94c751111888e8c5c658d1ab68e'
    'assets\sprint21-arthropods\bebelith-albedo.png'='afd98003bcc96ae32db4c3da63fdf3377d84f07141148ef7742cf8d3742fd554'
    'assets\ankylosaurus\ankylosaurus-mesh.json'='3eb7585e6f410711c7bf84bf4e23ff0df2c04fbe06ec226a0552a715c9fa9ca9'
    'assets\ankylosaurus\ankylosaurus-albedo.png'='2cdd8bc7e9139e748a9a88ec52e8a360245b6ca5ecb7829eec78a57d5a598bac'
    'assets\sprint18-primates\ape-mesh.json'='2f0196eca297019770894b66d69cb474e9bba59c449a95590326f888086631ee'
    'assets\sprint18-primates\ape-albedo.png'='85c7f480eeac7dddb125961061f0e62ba2460505fcee8198e88c208a95c6c7d9'
    'assets\sprint18-primates\dire-ape-mesh.json'='d0c1d0324cf7a12bbbc9b64874dfefb3ba010fb82020b87f72a47c9d9658700a'
    'assets\sprint18-primates\dire-ape-albedo.png'='37bdc23e705bfa0771105b768d1cd052157dfdc1d2c6bbd2a99236cf8268e219'
}
foreach($path in $unaffected.Keys) {Check ($policy.ContainsKey($path) -and $policy[$path] -ceq $unaffected[$path]) ('unaffected-pin-retained-'+$path)}

# Every revised body must have both halves curated, including Girallon's
# intentionally unchanged painting. Cross-wiring uses actual other exports.
$revised=@(
    'assets\ungulates\gorgon',
    'assets\sprint19-fourarmed\girallon',
    'assets\sprint21-arthropods\giant-crab',
    'assets\sprint27-griffon\griffon',
    'assets\sprint14-insects\giant-stag-beetle',
    'assets\sprint23-armored-dinosaurs\stegosaurus',
    'assets\sprint25-giant-bodies\elephant',
    'assets\sprint25-giant-bodies\brachiosaurus')
foreach($body in $revised) {
    foreach($suffix in @('-mesh.json','-albedo.png')) {Check ($policy.ContainsKey($body+$suffix)) ('revised-pair-member-'+$body+$suffix)}
    $target=$body+'-albedo.png'
    $foreign=if($body -clike '*girallon') {'assets\sprint27-griffon\griffon-albedo.png'}else{'assets\sprint19-fourarmed\girallon-albedo.png'}
    Copy-Item -LiteralPath (Join-Path $repositoryRoot $foreign) -Destination (Join-Path $modDirectory $target) -Force
    Reject-Body $target ('reject-cross-wired-albedo-'+$body)
    Copy-Item -LiteralPath (Join-Path $repositoryRoot $target) -Destination (Join-Path $modDirectory $target) -Force
}

# Wrong and missing bytes are refused for revised and unaffected controls alike.
foreach($path in $policy.Keys) {
    $target=Join-Path $modDirectory $path
    [IO.File]::WriteAllText($target,'wrong export fixture')
    Reject-Body $path ('reject-wrong-bytes-'+$path)
    Remove-Item -LiteralPath $target -Force
    Reject-Body $path ('reject-missing-'+$path)
    Copy-Item -LiteralPath (Join-Path $repositoryRoot $path) -Destination $target
}

# Actual retained original project inputs, rather than invented stale hashes.
$stale=@{
    'assets\ungulates\gorgon-mesh.json'='assets-source\original-models\sprint26-gorgon\cleanup2-gorgon-input.json'
    'assets\sprint27-griffon\griffon-mesh.json'='assets-source\original-models\sprint27-griffon\cleanup2-griffon-input.json'
    'assets\sprint25-giant-bodies\elephant-mesh.json'='assets-source\original-models\sprint25-giant-bodies\cleanup2-elephant-input.json'
    'assets\sprint25-giant-bodies\brachiosaurus-mesh.json'='assets-source\original-models\sprint25-giant-bodies\cleanup2-brachiosaurus-input.json'
}
foreach($path in $stale.Keys) {
    Copy-Item -LiteralPath (Join-Path $repositoryRoot $stale[$path]) -Destination (Join-Path $modDirectory $path) -Force
    Reject-Body $path ('reject-actual-stale-original-mesh-'+$path)
    Copy-Item -LiteralPath (Join-Path $repositoryRoot $path) -Destination (Join-Path $modDirectory $path) -Force
}
Assert-KmgProtectedBodyExports -ModDirectory $modDirectory
Check $true 'exact-pairs-admitted-after-negative-cases'
$report=[ordered]@{kind='Cleanup2PackageBodyPinsNoGame';checks=$checks;failures=@($failures.ToArray());
    elapsedSeconds=$watch.Elapsed.TotalSeconds;powerShellVersion=$PSVersionTable.PSVersion.ToString();
    validatorSha256=Get-KmgSha256 $validatorPath;bodyPins=$policy;evidenceDirectory=$root;nativeRuntimeExecuted=$false}
$report | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $root 'test-summary.json') -Encoding UTF8
if($failures.Count) {throw ('Body export admission tests failed: '+($failures -join ', '))}
$display=[ordered]@{}
foreach($key in $report.Keys) {if($key -cne 'bodyPins') {$display[$key]=$report[$key]}}
Write-Output ($display | ConvertTo-Json -Depth 5)
