[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$ModAssembly,
    [string]$ReferenceBundleDir = 'C:\Dev\KingmakerGunslingerLab\private\extracted-references\KingmakerGunslinger-private-build-references'
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$clock = [Diagnostics.Stopwatch]::StartNew()
$managed = Join-Path $ReferenceBundleDir 'Managed'
$modPath = (Resolve-Path -LiteralPath $ModAssembly).Path
$modDir = Split-Path $modPath
$resolver = [ResolveEventHandler]{
    param($sender, $eventArgs)
    $name = ([Reflection.AssemblyName]$eventArgs.Name).Name + '.dll'
    foreach ($candidate in @((Join-Path $modDir $name), (Join-Path $managed $name),
        (Join-Path (Join-Path $managed 'UnityModManager') $name))) {
        if (Test-Path -LiteralPath $candidate -PathType Leaf) { return [Reflection.Assembly]::LoadFrom($candidate) }
    }
    return $null
}
[AppDomain]::CurrentDomain.add_AssemblyResolve($resolver)
$script:checks = 0
function Assert-Behavior([bool]$condition, [string]$name) {
    if (!$condition) { throw "FAILED: $name" }
    $script:checks++
}
function Invoke-One($method, $argument) {
    $argsOne = New-Object object[] 1
    $argsOne[0] = $argument
    return $method.Invoke($null, $argsOne)
}
function Test-ExpectedNotSupported([Exception]$Failure) {
    # Windows PowerShell may unwrap TargetInvocationException into a
    # MethodInvocationException whose direct inner is the target exception.
    # Match the real target type through either managed wrapper shape.
    while ($null -ne $Failure) {
        if ($Failure -is [NotSupportedException]) { return $true }
        $Failure = $Failure.InnerException
    }
    return $false
}
try {
    $mod = [Reflection.Assembly]::LoadFrom($modPath)
    $game = [Reflection.Assembly]::LoadFrom((Join-Path $managed 'Assembly-CSharp.dll'))
    $unity = [Reflection.Assembly]::LoadFrom((Join-Path $managed 'UnityEngine.CoreModule.dll'))
    $flags = [Reflection.BindingFlags]'Public,NonPublic,Static'
    $helper = $mod.GetType('KingmakerGunslinger.RuntimeTesting.Cleanup2CharacterCpuReference', $true)
    $requestType = $mod.GetType('KingmakerGunslinger.RuntimeTesting.RuntimeTestRequest', $true)
    $json = [Reflection.Assembly]::LoadFrom((Join-Path $managed 'Newtonsoft.Json.dll'))
    $jObject = $json.GetType('Newtonsoft.Json.Linq.JObject', $true)
    $jsonParse = $jObject.GetMethod('Parse', [Type[]]@([string]))
    $allowed = $helper.GetMethod('RequestAllowed', $flags)
    function New-Request([string]$phase, [bool]$exitAfter = $true, [string]$scenario = 'disposable-expanded-summoning-cleanup2-native-review', [bool]$extra = $false) {
        $request = [Activator]::CreateInstance($requestType, $true)
        $request.RunId = 'no-game-cpu-reference-test'
        $request.Scenario = $scenario
        $request.ExitAfterCompletion = $exitAfter
        $members = '{"reviewPhase":"' + $phase + '"' + $(if ($extra) { ',"extra":true' } else { '' }) + '}'
        $request.Parameters = $jsonParse.Invoke($null, @($members))
        return $request
    }
    foreach ($phase in @('giant-lineup', 'scale-prototype')) {
        Assert-Behavior ([bool](Invoke-One $allowed (New-Request $phase))) "exact $phase admission"
    }
    foreach ($phase in @('Giant-lineup', 'GIANT-LINEUP', 'giant-lineup ', 'menus', 'bodies', 'shared-controls', '')) {
        Assert-Behavior (!(Invoke-One $allowed (New-Request $phase))) "phase refusal: '$phase'"
    }
    Assert-Behavior (!(Invoke-One $allowed (New-Request 'giant-lineup' $false))) 'automatic exit required'
    Assert-Behavior (!(Invoke-One $allowed (New-Request 'giant-lineup' $true 'working-save-expanded-summoning-creature-review'))) 'different scenario refused'
    Assert-Behavior (!(Invoke-One $allowed (New-Request 'giant-lineup' $true 'disposable-expanded-summoning-cleanup2-native-review' $true))) 'extra request parameter refused'
    $request = New-Request 'giant-lineup'; $request.RunId = ''
    Assert-Behavior (!(Invoke-One $allowed $request)) 'missing run identity refused'
    Assert-Behavior (!(Invoke-One $allowed $null)) 'null request refused'

    # Execute the actual production rewrite on instruction objects only. Never
    # patch Harmony, instantiate a Unity object, invoke BuildMesh or Upload.
    $rewrite = $helper.GetMethod('RewriteUpload', $flags)
    $instructionType = $rewrite.GetParameters()[0].ParameterType.GetGenericArguments()[0]
    $listType = [Collections.Generic.List``1].MakeGenericType($instructionType)
    $upload = $unity.GetType('UnityEngine.Mesh', $true).GetMethod('UploadMeshData', [Type[]]@([bool]))
    function New-Instruction($opcode, $operand = $null) {
        return [Activator]::CreateInstance($instructionType, [object[]]@($opcode, $operand))
    }
    function New-Instructions([int]$uploads = 1, [bool]$literalTrue = $true) {
        $list = [Activator]::CreateInstance($listType)
        for ($i = 0; $i -lt $uploads; $i++) {
            $list.Add((New-Instruction ([Reflection.Emit.OpCodes]::Ldloc_0)))
            $literal = if ($literalTrue) { [Reflection.Emit.OpCodes]::Ldc_I4_1 } else { [Reflection.Emit.OpCodes]::Ldc_I4_0 }
            $list.Add((New-Instruction $literal))
            $list.Add((New-Instruction ([Reflection.Emit.OpCodes]::Callvirt) $upload))
        }
        $list.Add((New-Instruction ([Reflection.Emit.OpCodes]::Ret)))
        return ,$list
    }
    $rewriteInput = New-Instructions
    $dynamic = [Reflection.Emit.DynamicMethod]::new('LabelOnly', [void], [Type[]]@())
    $label = $dynamic.GetILGenerator().DefineLabel()
    $rewriteInput[2].labels.Add($label)
    $rewritten = @(Invoke-One $rewrite $rewriteInput)
    Assert-Behavior ($rewritten.Count -eq 5) 'one owner argument inserted'
    Assert-Behavior ($rewritten[1].opcode -eq [Reflection.Emit.OpCodes]::Ldc_I4_1) 'native discard argument retained'
    Assert-Behavior ($rewritten[2].opcode -eq [Reflection.Emit.OpCodes]::Ldarg_0) 'actual Character instance supplied'
    Assert-Behavior ($rewritten[3].opcode -eq [Reflection.Emit.OpCodes]::Call -and $rewritten[3].operand.Name -eq 'ObserveThenUpload') 'single exact upload wrapped'
    Assert-Behavior ($rewritten[2].labels.Count -eq 1 -and $rewritten[3].labels.Count -eq 0) 'branch labels moved to owner load'
    foreach ($case in @(@{Count=0; Literal=$true}, @{Count=2; Literal=$true}, @{Count=1; Literal=$false})) {
        $rejected = $false
        try { $null = Invoke-One $rewrite (New-Instructions $case.Count $case.Literal) }
        catch { if (Test-ExpectedNotSupported $_.Exception) { $rejected = $true } else { throw } }
        Assert-Behavior $rejected "ambiguous/missing/changed upload refused: $($case.Count)/$($case.Literal)"
    }

    # These are managed value arrays, not Unity runtime objects or donor data.
    $sameFloat = $helper.GetMethod('SameFloat', $flags)
    $negativeZero = [BitConverter]::ToSingle([BitConverter]::GetBytes([int]::MinValue), 0)
    Assert-Behavior ([bool]$sameFloat.Invoke($null, [object[]]@([single]1, [single]1))) 'same float bits accepted'
    Assert-Behavior (!$sameFloat.Invoke($null, [object[]]@([single]0, $negativeZero))) 'signed zero mutation retained exactly'
    $weightType = $unity.GetType('UnityEngine.BoneWeight', $true)
    $firstWeights = [Array]::CreateInstance($weightType, 1); $secondWeights = [Array]::CreateInstance($weightType, 1)
    $equalWeights = $helper.GetMethod('EqualWeights', $flags)
    Assert-Behavior ([bool]$equalWeights.Invoke($null, [object[]]@($firstWeights, $secondWeights))) 'identical weight records accepted'
    $weight = [Activator]::CreateInstance($weightType); $weight.boneIndex0 = 1; $secondWeights.SetValue($weight, 0)
    Assert-Behavior (!$equalWeights.Invoke($null, [object[]]@($firstWeights, $secondWeights))) 'palette ordinal mutation refused'
    $weight.boneIndex0 = 0; $weight.weight0 = $negativeZero; $secondWeights.SetValue($weight, 0)
    Assert-Behavior (!$equalWeights.Invoke($null, [object[]]@($firstWeights, $secondWeights))) 'weight bit mutation refused'
    $matrixType = $unity.GetType('UnityEngine.Matrix4x4', $true)
    $firstBinds = [Array]::CreateInstance($matrixType, 1); $secondBinds = [Array]::CreateInstance($matrixType, 1)
    $equalBinds = $helper.GetMethod('EqualBinds', $flags)
    Assert-Behavior ([bool]$equalBinds.Invoke($null, [object[]]@($firstBinds, $secondBinds))) 'identical bind records accepted'
    $matrix = [Activator]::CreateInstance($matrixType)
    # Read the actual managed field directly; PowerShell's member adapter may
    # inspect unrelated Unity native-attribute types absent from a build bundle.
    $matrixType.GetField('m03').SetValue($matrix, [single]0.000001)
    $secondBinds.SetValue($matrix, 0)
    Assert-Behavior (!$equalBinds.Invoke($null, [object[]]@($firstBinds, $secondBinds))) 'small bind mutation refused without tolerance'

    $native = $game.GetType('Kingmaker.Visual.CharacterSystem.Character', $true)
    $build = $native.GetMethod('BuildMesh', [Reflection.BindingFlags]'NonPublic,Instance')
    Assert-Behavior ($build.Module.ModuleVersionId -eq [guid]'07fa1e4d-8618-41b3-9b8d-faa17d3b26f7') 'exact installed Character module'
    $bytes = $build.GetMethodBody().GetILAsByteArray(); $literalUploads = 0
    for ($i = 1; $i -lt $bytes.Length - 4; $i++) {
        if ($bytes[$i] -eq 0x6f -and $bytes[$i-1] -eq 0x17) {
            try { $member = $build.Module.ResolveMember([BitConverter]::ToInt32($bytes, $i+1)) } catch { continue }
            if ($member.DeclaringType.FullName -eq 'UnityEngine.Mesh' -and $member.Name -eq 'UploadMeshData') { $literalUploads++ }
        }
    }
    Assert-Behavior ($literalUploads -eq 1) 'installed native literal-true upload observed once'
    $animationBase = $game.GetType('Kingmaker.Visual.Animation.AnimationManager', $true)
    Assert-Behavior ($animationBase.GetField('m_AnimationSet', [Reflection.BindingFlags]'Instance,NonPublic').FieldType.FullName -eq 'Kingmaker.Visual.Animation.AnimationSet') 'serialized animation field available without getter fallback'
    $settings = $game.GetType('Kingmaker.Visual.MaterialEffects.StandardMaterialController+MaterialSettings', $true)
    $cachedField = $mod.GetType('KingmakerGunslinger.Summoning.EttinSecondHeadVisual', $true).GetMethod('CachedField', $flags)
    $cachedDissolve = $cachedField.MakeGenericMethod([single])
    $settingsObject = [Runtime.Serialization.FormatterServices]::GetUninitializedObject($settings)
    $settings.GetField('Dissolve').SetValue($settingsObject, [single]0.375)
    Assert-Behavior ($cachedDissolve.Invoke($null, [object[]]@($settingsObject,'Dissolve')) -eq [single]0.375) 'actual native cached baseline read rather than forced zero'
    $rejected = $false
    try { $null = $cachedField.MakeGenericMethod([int]).Invoke($null, [object[]]@($settingsObject,'Dissolve')) }
    catch { if (Test-ExpectedNotSupported $_.Exception) { $rejected = $true } else { throw } }
    Assert-Behavior $rejected 'changed cached fade field type refused'
    $clock.Stop()
    [pscustomobject]@{Result='PASS';Checks=$script:checks;Seconds=$clock.Elapsed.TotalSeconds;
        ModAssemblySha256=(Get-FileHash -LiteralPath $modPath -Algorithm SHA256).Hash.ToLowerInvariant();
        Scope='No game/native calls, Harmony patch installation, Unity-object construction or geometry. Live construction lease ownership/mesh capture/controller adoption still requires guarded native evidence.'} | ConvertTo-Json
}
finally { [AppDomain]::CurrentDomain.remove_AssemblyResolve($resolver) }
