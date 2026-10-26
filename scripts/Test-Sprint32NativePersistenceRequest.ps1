<# Runs the compiled production request validator headlessly. No runtime
request, evidence directory, marker, deployment, game or save is created. #>
[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)][string]$ModAssemblyPath,
    [string]$KingmakerInstallDir='C:/Program Files (x86)/Steam/steamapps/common/Pathfinder Kingmaker'
)
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
$root=Split-Path -Parent $PSScriptRoot
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')
. (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')
$managed=Join-Path $KingmakerInstallDir 'Kingmaker_Data/Managed'
$resolver=[ResolveEventHandler]{param($sender,$eventArgs)
    $file=([Reflection.AssemblyName]$eventArgs.Name).Name+'.dll'
    foreach($directory in @($managed,(Join-Path $managed 'UnityModManager'))) {
        $path=Join-Path $directory $file
        if(Test-Path -LiteralPath $path){return [Reflection.Assembly]::LoadFrom($path)}
    }
    return $null
}
[AppDomain]::CurrentDomain.add_AssemblyResolve($resolver)
try {
    [void][Reflection.Assembly]::LoadFrom((Join-Path $managed 'Newtonsoft.Json.dll'))
    $mod=[Reflection.Assembly]::LoadFrom((Resolve-Path -LiteralPath $ModAssemblyPath).Path)
    $type=$mod.GetType('KingmakerGunslinger.RuntimeTesting.RuntimeTestRequest',$true)
    $parser=$mod.GetType('KingmakerGunslinger.RuntimeTesting.RuntimeTestRequestParser',$true)
    $validate=$parser.GetMethod('Validate',[Reflection.BindingFlags]'NonPublic,Static')
    if($null -eq $validate){throw 'Compiled production validator missing.'}
    $missing=Join-Path $script:KmgRuntimeEvidenceRoot ('uncreated-s32-parser-'+[Guid]::NewGuid().ToString('N'))
    if(Test-Path -LiteralPath $missing){throw 'Expected nonexistent test evidence path.'}
    $version=(Get-Content -LiteralPath (Join-Path $root 'Info.json') -Raw | ConvertFrom-Json).Version
    $base=@{ExpectedVersion=$version;TimeoutSeconds=300;CatalogTimeoutSeconds=180;SelectionTimeoutSeconds=300;
        CompletionTimeoutSeconds=180;MainMenuTimeoutSeconds=180;ActionResolutionTimeoutSeconds=180;
        ActionInvocationTimeoutSeconds=30;DescriptorResolutionTimeoutSeconds=30;LoadEntryTimeoutSeconds=30;
        FingerprintTimeoutSeconds=180;ExitAfterCompletion=$true;EvidenceDirectory=$missing}
    $rows=@()
    foreach($scenario in @('working-save-expanded-summoning-prepare','working-save-expanded-summoning-verify-cleanup','working-save-expanded-summoning-verify-absent')) {
      foreach($transition in @($null,'retained','removed','module-off')) {
        $parameters=@{saveName='KMG_AUTOMATION_WORKING';persistenceScope='fey-celestial'}
        if($null -ne $transition){$parameters.suggestionTransition=$transition}
        $good=New-KmgRuntimeRequest @base -Scenario $scenario -Parameters $parameters
        $json=$good | ConvertTo-Json -Depth 20
        foreach($case in @('exact-scope','baseline','other-save','no-exit','scope-case','extra-parameter','version','unknown-transition','transition-case','null-transition','wrong-transition-scope')) {
            $request=[Newtonsoft.Json.JsonConvert]::DeserializeObject($json,$type)
            $expected='evidence-directory-missing'
            switch($case) {
                'baseline' {$request.Parameters['saveName']=[Newtonsoft.Json.Linq.JValue]::new('KMG_AUTOMATION_BASELINE');$expected='baseline-save-forbidden'}
                'other-save' {$request.Parameters['saveName']=[Newtonsoft.Json.Linq.JValue]::new('Other');$expected='save-name-not-allowed'}
                'no-exit' {$request.ExitAfterCompletion=$false;$expected='summoning-persistence-scope-invalid'}
                'scope-case' {$request.Parameters['persistenceScope']=[Newtonsoft.Json.Linq.JValue]::new('Fey-celestial');$expected='summoning-persistence-scope-invalid'}
                'extra-parameter' {$request.Parameters['extra']=[Newtonsoft.Json.Linq.JValue]::new('untrusted');$expected='save-name-required'}
                'version' {$request.ExpectedModVersion='wrong';$expected='mod-version-mismatch'}
                'unknown-transition' {$request.Parameters['suggestionTransition']=[Newtonsoft.Json.Linq.JValue]::new('arbitrary');$expected='sprint32-suggestion-transition-invalid'}
                'transition-case' {$request.Parameters['suggestionTransition']=[Newtonsoft.Json.Linq.JValue]::new('Retained');$expected='sprint32-suggestion-transition-invalid'}
                'null-transition' {$request.Parameters['suggestionTransition']=[Newtonsoft.Json.Linq.JValue]::CreateNull();$expected='sprint32-suggestion-transition-invalid'}
                'wrong-transition-scope' {$request.Parameters['suggestionTransition']=[Newtonsoft.Json.Linq.JValue]::new('retained');$request.Parameters['persistenceScope']=[Newtonsoft.Json.Linq.JValue]::new('crocodilians');$expected='sprint32-suggestion-transition-invalid'}
            }
            $actual=$validate.Invoke($null,@($request,$version))
            $rows+=[pscustomobject]@{scenario=$scenario;transition=$transition;case=$case;expected=$expected;actual=$actual;pass=($actual -ceq $expected)}
        }
      }
    }
    $rows | Where-Object {-not $_.pass} | Format-Table -AutoSize
    if(@($rows | Where-Object {-not $_.pass}).Count){throw 'Compiled Sprint32 persistence request refusal differed.'}
    if(Test-Path -LiteralPath $missing){throw 'Headless validator unexpectedly created evidence.'}
    Write-Host ('PASS compiled parser admission/refusals={0}; DLL={1}; MVID={2}. Exact scopes reach the deliberate missing-evidence refusal; no full request activation/game/save is claimed.' -f $rows.Count,(Get-FileHash -LiteralPath $ModAssemblyPath -Algorithm SHA256).Hash,$mod.ManifestModule.ModuleVersionId)
} finally {[AppDomain]::CurrentDomain.remove_AssemblyResolve($resolver)}
