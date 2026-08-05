# Gunslinger's Dodge native timed-buff fix

This branch changes Gunslinger's Dodge from a manually attached timed `Buff` to Kingmaker's ordinary ability-action pipeline:

- `AbilityCasterHasNoFacts` prevents reactivation while the Dodge buff is active.
- `AbilityEffectRunAction` executes the effect under the engine-created ability context.
- `ContextActionApplyBuff` applies the AC buff to the caster for exactly one round.
- The temporary AC buff is no longer marked as a class feature.
- The existing `GunslingerDodgeArmorClassBonus` still owns the exact +2 Dodge modifier and removes it when the buff turns off.

Call of the Wild is **not** a build or runtime dependency for this implementation.

The downloadable revised-repository archive is **source-only** and deliberately excludes historical `artifacts` output so an older DLL cannot be mistaken for this fix. Build the repository before attempting to install it.

## Prerequisites

Use the Windows machine on which Pathfinder: Kingmaker is installed.

1. Pathfinder: Kingmaker installed and launched successfully at least once.
2. Unity Mod Manager installed for Pathfinder: Kingmaker.
3. Visual Studio 2022 or Visual Studio 2022 Build Tools with:
   - **.NET desktop build tools**
   - **MSBuild**
   - **.NET Framework 4.7 targeting pack / developer pack**
4. Python 3 available as the `python` command.
5. Windows PowerShell 5.1 or PowerShell 7.

The project targets .NET Framework 4.7 and compiles against Kingmaker's own managed assemblies plus the Unity Mod Manager assemblies installed inside the game directory.

## Configure the Kingmaker path

From the repository root, copy the example file:

```powershell
Copy-Item .\GamePath.props.example .\GamePath.props
```

Open `GamePath.props` and make sure `KingmakerInstallDir` points to your game folder. The usual Steam path is:

```text
C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker
```

The folder must contain `Kingmaker.exe` and `Kingmaker_Data`.

## Build and create the Unity Mod Manager package

Open **Developer PowerShell for Visual Studio 2022**, change to the repository root, and run:

```powershell
Set-ExecutionPolicy -Scope Process Bypass

.\scripts\build.ps1 `
  -Configuration Release `
  -Clean `
  -Package `
  -KingmakerInstallDir "C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker"
```

Change the path if Kingmaker is installed elsewhere.

The build script performs repository validation, runs the domain-test executable, compiles the mod against the installed game assemblies, validates the build output, and creates a standalone Unity Mod Manager package.

The generated package will be:

```text
artifacts\packages\KingmakerGunslinger-0.0.67-complete-maintenance-loop-smoke-test.zip
```

The unpackaged build output will be:

```text
artifacts\bin\Release\KingmakerGunslinger\
```

### When MSBuild is not detected automatically

Locate MSBuild with Visual Studio's `vswhere.exe` and pass it explicitly:

```powershell
$msbuild = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" `
  -latest `
  -requires Microsoft.Component.MSBuild `
  -find MSBuild\**\Bin\MSBuild.exe |
  Select-Object -First 1

.\scripts\build.ps1 `
  -Configuration Release `
  -Clean `
  -Package `
  -KingmakerInstallDir "C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker" `
  -MSBuildPath $msbuild
```

Do not use `scripts\Build-Local.ps1` for this ordinary build. That script is tied to the autonomous-development lab's private reference bundle and fixed local asset-build paths.

## Install with Unity Mod Manager

1. Close Pathfinder: Kingmaker completely.
2. Back up any saves you care about.
3. Remove or rename the existing mod folder so stale DLLs cannot survive the update:

   ```text
   <Kingmaker install folder>\Mods\KingmakerGunslinger
   ```

4. Start Unity Mod Manager.
5. Select **Pathfinder: Kingmaker**.
6. Open the **Mods** or **Install** tab.
7. Drag this generated package onto Unity Mod Manager, or select it through the install button:

   ```text
   artifacts\packages\KingmakerGunslinger-0.0.67-complete-maintenance-loop-smoke-test.zip
   ```

8. Confirm that Unity Mod Manager reports **Kingmaker Gunslinger** as installed.

Do **not** give Unity Mod Manager the full source-repository ZIP. It needs the generated package from `artifacts\packages`.

### Manual installation fallback

Open the generated package and extract its top-level `KingmakerGunslinger` folder into:

```text
<Kingmaker install folder>\Mods\
```

The final layout must include:

```text
Pathfinder Kingmaker\
  Mods\
    KingmakerGunslinger\
      Info.json
      KingmakerGunslinger.dll
      assets\
      blueprints\
```

## Test Gunslinger's Dodge

An already-stuck buff saved by the previous implementation may remain stuck in that save. Test from a save created before activating Gunslinger's Dodge, or create a fresh test character.

### Real-time-with-pause test

1. Record the character's normal AC and current Grit.
2. Activate **Gunslinger's Dodge**.
3. Confirm immediately:
   - Grit decreases by exactly 1, unless True Grit legitimately reduces the cost.
   - AC rises by exactly 2.
   - The Gunslinger's Dodge condition appears with a one-round countdown.
4. Leave the game unpaused for more than six seconds.
5. Confirm:
   - The condition icon disappears.
   - AC returns to the exact original value.
   - The ability becomes available again if sufficient Grit remains.

### Turn-based test

1. Enter turn-based combat.
2. Activate Gunslinger's Dodge.
3. Confirm the immediate +2 AC bonus.
4. Advance through the relevant one-round duration.
5. Confirm the buff and AC modifier are removed by Kingmaker's normal round lifecycle.

### Regression checks

Also confirm that:

- Activating Dodge a second time while the buff is present is rejected.
- The character does not become prone.
- Reload, firearm attacks, and Grit display still work normally.
- Saving and loading after the corrected buff has expired does not restore it.

## Logs if the test fails

Check Unity Mod Manager's log display and Kingmaker's Unity log:

```text
%USERPROFILE%\AppData\LocalLow\Owlcat Games\Pathfinder Kingmaker\output_log.txt
```

For an expiration failure, preserve the log plus screenshots showing AC before activation, during the countdown, and after the countdown reaches zero.

## Validation status

The source change and repository validator have been updated together. The final behavioral result still requires a Windows build against your installed Kingmaker assemblies and a live in-game test; this repository package does not claim that runtime verification has already occurred.
