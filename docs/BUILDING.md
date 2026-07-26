# Building version 0.0.29

## Required target

- Pathfinder: Kingmaker Enhanced Plus Edition 2.1.7b
- Unity Mod Manager 0.32.4
- Harmony 1.2.0.1 (`0Harmony12.dll`)
- .NET Framework 4.7 reference surface
- C# 7.3
- Release / AnyCPU
- deterministic compilation
- warnings treated as errors

The private Kingmaker, Unity, Unity Mod Manager, Harmony, and Newtonsoft assemblies are compiler input only. They must never be committed to source or copied into a released UMM package.

## Windows installed-game path

```powershell
Set-ExecutionPolicy -Scope Process Bypass

.\scripts\new-game-path-props.ps1 `
  -KingmakerInstallDir 'C:\Path\To\Pathfinder Kingmaker'
```

`GamePath.props` is local and ignored.

## Verify the exact runtime contracts

```powershell
.\scripts\fingerprint-environment.ps1 `
  -KingmakerInstallDir 'C:\Path\To\Pathfinder Kingmaker' `
  -Storefront Steam `
  -DisplayedGameVersion 2.1.7b

.\scripts\inspect-runtime-contracts.ps1 `
  -KingmakerInstallDir 'C:\Path\To\Pathfinder Kingmaker'
```

The accepted Sprint 22 repair still requires exactly one declared callback on each intended rule-event type with this shape:

```text
System.Void OnTrigger(Kingmaker.RuleSystem.RulebookEventContext)
```

Sprint 23 additionally requires exactly these natural-roll contracts:

```text
private System.Void RuleAttackRoll.set_Roll(Kingmaker.RuleSystem.RulebookEvent+RollEntry)
public System.Boolean RuleAttackRoll.IsSuccessRoll(System.Int32)
```

`RulebookEvent.RollEntry` must remain a value type with writable `Value : Int32` and `RollHistory : List<Int32>` fields. The existing persistence gate also requires the exact zero-argument `Kingmaker.Items.ItemEntity.ApplyEnchantments()` base method and a concrete `Kingmaker.Items.ItemEntityWeapon` type. Sprint 29 additionally retains the exact inventory and ability contracts already used by Reload and Overhaul while registering the separate full-round Repair ability. The rejected `ItemEntityWeapon.UniqueId` vault is not a build requirement and must not be revived. Do not replace the exact Roll-setter diagnostic with a global dice patch.

## Validate and test

```powershell
.\scripts\validate-repository.ps1
.\scripts\test-domain.ps1 -Configuration Release -Clean
```

Expected final line:

```text
Completed 599 tests; failures=0.
```

For milestone qualification, run the full suite three times and require byte-identical output.

## Build

```powershell
.\scripts\build.ps1 -Configuration Release -Clean
```

External references remain `Private=False`. Any copied game, Unity, Harmony, UMM, or Newtonsoft binary fails output validation.

## Build from the supplied private reference bundle

The source also includes a cross-platform Roslyn evidence tool:

```text
python tools/build_mod_from_private_references.py \
  --reference-bundle-dir <extracted-private-bundle> \
  --dotnet <dotnet-host> \
  --csc <Roslyn-csc.dll> \
  --net47-ref-dir <.NETFramework/v4.7> \
  --output-dir <output> \
  --configuration Release
```

This path compiles against the same exact private assemblies without redistributing them.

## Package contract

The standalone smoke-test ZIP contains one root and exactly one binary:

```text
KingmakerGunslinger/
  CHANGELOG.md
  Info.json
  KingmakerGunslinger.dll
  LICENSE
  README.md
  SMOKE-TEST-GUIDE.md
  blueprints/
    blueprints.json
    blueprints.schema.json
```

A source or complete milestone archive is not itself a UMM package. Label a candidate **READY FOR KINGMAKER** only after the standalone ZIP exists and its package validator passes.
