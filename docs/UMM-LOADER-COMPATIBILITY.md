# Unity Mod Manager compatibility (declared minimum 0.31.1)

Status: **development candidate, tested, not released.** This record covers one development candidate.
The owner has not accepted it, and there is no Nexus upload. v0.0.187 stays published and unchanged.

## Candidate

| Item | Value |
|---|---|
| Starting release | v0.0.187, tag commit `bfaac01f52`, ZIP `230e9298afdc534d6017fd2914101649827d7d53ffaed10056361cbade1fa48c`, DLL `7b23241c3a219153e688041c905f8485ea70a7f36abb6f6a94e8beb7bbafc368`, `ManagerVersion` 0.32.4, DLL references `UnityModManager 0.33.0.0` |
| Candidate commit | `b9400333e5b6f75db0271786efecc5436c97afaf` (branch `codex/umm-0.31.1-compat`) |
| Candidate ZIP | `KingmakerGunslinger-0.0.187-development-umm-0311-compat-b9400333e5b6.zip`, SHA-256 `c82c73abf3efb62e740ee42687127a93355a711a7646e39863a846879c44867a` |
| Candidate DLL | SHA-256 `7fa421d7a783512e60d8efe56d65c8da758b3529d18cebed6b833c5e7266ab1b`, MVID `50027f0e-df16-4171-bcf4-b9af24842615`, references `UnityModManager 0.31.1.0` and `0Harmony12 1.2.0.1` |
| Declared minimum | `Info.json` `ManagerVersion` **0.31.1** |
| Change from v0.0.187 | Same 458 packaged files. Only `Info.json` (`ManagerVersion`) and the DLL differ. The DLL differs because it is compiled against the pinned 0.31.1 loader and carries a new commit stamp. `git diff v0.0.187 b9400333e5 -- src blueprints assets` is empty. **No gameplay or production C# change.** |
| Build | `Build-Local.ps1 -DevelopmentPurpose umm-0311-compat -DevelopmentCompleteSuite`: complete domain suite 2971/2971 passed, strict package validation passed. This is a development receipt, not a release qualification. |

The version string stays 0.0.187 because this repository builds development candidates against the released version. A
release would be opened as 0.0.188 through the normal release process with the owner's approval.

## Loader contract audit

A metadata audit of the compiled DLL (`System.Reflection.Metadata`, extended from the Cutscene Skip lab's contract tool)
listed every type and member it binds in `UnityModManager` (41 entries) and `0Harmony12` (101 entries). It also checked
for any type that derives from or implements a loader type (there are none). Each member was then looked up by name and
decoded signature, including inherited members, in:

* UMM 0.31.1 `UnityModManager.dll` `2e038ff2…` (NuGet, assembly 0.31.1.0)
* UMM 0.32.4 `1387468b…` (NuGet, 0.32.4.0)
* UMM 0.33.0 `63e5baf7…` (the owner's Nexus installation, 0.33.0.0; NuGet has no 0.33.0)
* `0Harmony12.dll` `aa1cd483…` (1.2.0.1), byte-identical in all three loaders

**Result: compatible.** Every bound member is present with a matching signature in all three. The v0.0.187 DLL and the
candidate bind exactly the same set.

The bound surface:

* `ModEntry` (`Info`, `Path`, `Logger`, `Enabled`, `Active`, `Assembly`, `Loaded`, `HasAssembly`, `ErrorOnLoading`, `OnGUI`, `OnSaveGUI`, `OnToggle`, `OnUpdate`)
* `ModLogger.Log`
* `ModInfo` (`Id`, `DisplayName`, `Version`, `ManagerVersion`)
* `UnityModManager.modEntries`
* `UI.Instance`, `UI.Opened`, `UI.ToggleWindow(bool)`
* Harmony 1.2 `HarmonyInstance`, `HarmonyMethod`, the patch attributes, `AccessTools`, `Traverse`, `CodeInstruction` and the patch-info types

Name-based reflection, also checked:

* **Production.** The optional-integration coordinators discover mods through the static `modEntries` field. It is
  present with the same type in all three loaders.
* **Craft Magic Items bridge.** It resolves Harmony 2 from CMI's own `HarmonyInstance` field type. It needs
  `Harmony(string)`, `HarmonyMethod(MethodInfo)`, exactly one public 5-parameter `Patch(MethodBase, …)` and exactly one
  `UnpatchAll(string)`. All hold in Harmony 2.3.1.1 (the 0.31.1 profile) and 2.3.6 (0.32.4 and 0.33.0), net35 and net48.
* **Runtime-testing only.** `UI.mShowModSettings`, `UI.mPreviousShowModSettings` and `GameScripts.scripts` are present in
  all three.

Lifecycle: the mod uses no UMM `ModSettings`. Feature-module settings are its own `FeatureModules.json` under
`ModEntry.Path`. It registers no `OnToggle`; module changes take effect after a restart. UMM's `SaveSettingsAndParams`
(the window's Save button and exit) calls `OnSaveGUI` the same way in all three versions.

No compatibility adapter was needed and none was added.

## Build contract

* **Pinned loader references.** `scripts/loader-reference-pins.json` (a tracked product input) pins `UnityModManager.dll`
  from the UMM 0.31.1 NuGet package (`05a82b66…`, entry `lib/net35`) and `0Harmony12.dll` from the UMM repository at the
  0.31.1 commit `10cfb1cc`. These identities were taken from the Cutscene Skip work and re-downloaded fresh on 2026-11-13;
  every hash matched.
* **Fetching and refusal.** `Build-Local.ps1` fetches absent files into `artifacts/references/loader`. It refuses a file
  that differs from its pin and never falls back to the installed or bundled loader. The exact-reference compiler
  re-verifies the pins.
* **Game and Unity references** keep their exact bundle-equals-installation check.
* **Receipts.** Build receipts record each loader row's role, assembly version, source URL and the pins hash.
  Development receipts are refused if their loader rows differ from the current pins. A changed pins file changes the
  source state, which invalidates earlier receipts.
* **Package validation.** The packaged `ManagerVersion` must equal the pinned floor. The package must contain exactly one
  binary, the mod, and no loader, Harmony, dnlib, game or Unity assembly.
* **Tests.** `scripts/Test-LoaderReferencePins.ps1` (32 checks) and `tools/test_loader_reference_pins.py` (5) run inside
  `validate-repository.ps1`. `Test-DevelopmentCandidate.ps1` now also proves that the compiler receives the pinned
  directory and that a tampered loader is refused before any child step.

## How the runtime matrix was run

* **Disposable game copy.** All runs used a byte-for-byte copy of the owner's installation (Kingmaker 2.1.7b, 46,068
  files) under `C:\Dev\KingmakerGunslingerLab\game-copies\loader-compat`, launched directly with `steam_appid.txt`.
* **Owner protection.** The owner's installation was only hashed: before and after every session, the loader, proxy,
  `Kingmaker.exe`, `Assembly-CSharp.dll`, `Mods` and every save were compared. The game's registry settings were backed up
  and restored exactly. Steam Cloud sync for 640820 was confirmed disabled.
* **Loader profiles** (`scripts/compatibility/loader-compat/loader-profiles.json`):
  * **0.31.1 and 0.32.4 are component-assembled**, not the original Nexus installer ZIPs. Each is a clean DoorstopProxy
    install from the upstream NuGet `UnityModManager` and `Lib.Harmony` packages, `dnlib`, and the UMM repository at the
    matching commit, every file SHA-256 verified. Harmony is 2.3.1.1 for 0.31.1 and 2.3.6 for 0.32.4.
  * **0.33.0 is a byte copy of the owner's Nexus-installed loader** (Harmony 2.3.6).
* **Candidate staging.** The candidate ZIP was extracted into `Mods` exactly as a user installs it. The optional mods are
  the owner's installed builds: Craft Magic Items 2.1.0, Kingmaker Bugfixes 1.3.3, Call of the Wild 1.14.4c-2.1 and
  ZFavoredClass 1.3.1.
* **Test-only harness** (`tools/LoaderCompatHarness`, never packaged):
  * redirects saves and the area cache to a per-session profile;
  * records, in-process, the UnityModManager, Harmony 2, Harmony 1.2 and mod assemblies by the SHA-256 of the file each
    was loaded from, and the session runner requires these to be the installed profile files and the candidate DLL;
  * runs its own steps, or isolates while one of the candidate's existing guarded scenarios runs.
* **Fresh processes.** Every launch is a fresh process. Settings and saves persist between launches within a session.
* **Proof of reload.** A same-process reload must load the file the game wrote, see loading begin, replace the player
  state and undo a 1234-money marker. A fresh-process reload loads the written file from the main menu. The verified item
  exists only in that file.
* **Fixture save** for in-game steps: `KMCS_AFTER_NORMAL_012_MODONLY.zks` from the Cutscene Skip lab (made with no other
  mod), copied into the session profile.

Evidence: `C:\Dev\KingmakerGunslingerLab\runtime-evidence\loader-compat\<session>\` (machine-local, not committed). Each
launch keeps its requests, harness result and events, product result, in-process identity, UMM log and player log.

## Results

Runs were on 2026-11-14 UTC, machine DATA, one sequential batch of 3,918 s. Every row uses the same candidate:
ZIP `c82c73ab…` and DLL `7fa421d7…`. The harness DLL was `f92ba77b…`, from the source committed in `5cb558961c`.

**Identity.** In every launch of every session the in-process identity matched the installed profile exactly:

| Profile | UnityModManager | 0Harmony | 0Harmony12 | Candidate DLL |
|---|---|---|---|---|
| 0.31.1 | 0.31.1.0 `2e038ff2…` | 2.3.1.1 `5f7ad0df…` | `aa1cd483…` | `7fa421d7…` |
| 0.32.4 | 0.32.4.0 `1387468b…` | 2.3.6.0 `df043f05…` | `aa1cd483…` | `7fa421d7…` |
| 0.33.0 | 0.33.0.0 `63e5baf7…` | 2.3.6.0 `df043f05…` | `aa1cd483…` | `7fa421d7…` |

**Owner integrity.** Every session reports the owner's saves unchanged, the owner's loader, proxy, `Mods` and game files
unchanged, and the registry settings restored.

### Matrix

| Check | 0.31.1 | 0.32.4 | 0.33.0 |
|---|---|---|---|
| Candidate alone (optional mods absent): startup, bootstrap, Harmony 1.2 patch ownership, Gunslinger class published exactly once, required blueprints resolve | PASS | PASS | PASS |
| UMM panel drawn on real IMGUI passes, no logged exception (in the menu and, with CMI, in game) | PASS | PASS | PASS |
| One Urban Barbarian toggle clicked and saved through UMM's own `SaveSettingsAndParams`, then a full restart: still disabled, nothing reset to all-enabled, survives a second panel save | PASS | PASS | PASS |
| Fresh process `observe-feature-module-settings` with the module off (47 assertions, publication follows settings) | PASS | PASS | PASS |
| `mod-load-smoke` | PASS | PASS | PASS |
| Gunslinger character creation commit and level-up commit (native `LevelUpController`) | PASS | PASS | PASS |
| Firearm: real paper-cartridge reload (9 assertions) and full attack (5) | PASS | PASS | PASS |
| Expanded summons: `public-routes`, casting from a prepared spellbook slot on natively levelled casters (46 assertions) | PASS | PASS | PASS |
| Disposable save with a loaded Tabletop pistol: save, same-process load of the written file (marker undone), fresh-process load, state token kept | PASS | PASS | PASS |
| With CMI 2.1.0 and Bugfixes 1.3.3: all mods load without error; Bugfixes `Dart.IsLight` = True; CMI crafting interface (its UMM panel) drawn in game | PASS | PASS | PASS |
| CMI's own `BuildCustomRecipeItemGuid` and `CraftItem` upgrade the Tabletop pistol with Enhancement1. The crafted item keeps the pistol weapon type, the product still classifies it as a firearm, and the product's hook transfers the loaded-cartridge state token | PASS | PASS | PASS |
| The crafted firearm and its state survive a same-process load and a fresh-process load of the written save | PASS | PASS | PASS |
| `observe-craft-magic-items-compatibility` and `observe-craft-magic-items-ammunition-ui` (CMI's IMGUI recipe control, project and `CraftItem`, for ammunition) | PASS | PASS | PASS |
| With Call of the Wild 1.14.4c-2.1 and ZFavoredClass 1.3.1: startup, Gunslinger published once, panel | PASS | not run | PASS |
| Favored Class Gunslinger race menus, archetype replacement and progressions (8); Brown-Fur contract and publication gate (14); Favored Class host state (6) | PASS | not run | PASS |

CMI crafted GUID: `a303d71d244640959827e9464df5a867#CraftMagicItems(enchantments=(d42fc23b92c640846ac137dc26e000d4))`.
The base Tabletop Early Pistol was replaced by the clone `KMG_EarlyPistol_Item(Clone)`, enchantment value 1, carrying
`KMG_StateToken_LoadedNormal_PaperCartridge`.

| Session | 0.31.1 | 0.32.4 | 0.33.0 |
|---|---|---|---|
| alone (11 launches) | `20261114T011241Z` | `20261114T013630Z` | `20261114T015425Z` |
| cmi-bugfixes (4) | `20261114T012526Z` | `20261114T014926Z` | `20261114T020656Z` |
| cotw-favored-class (4) | `20261114T013017Z` | not run | `20261114T021146Z` |

### Earlier runs: test-tool errors, kept as they are

These runs preceded the matrix. They are not evidence about the product either way.

* **Pilot `004627Z`.** The harness was required to write a result in a launch where the product's own scenario owned the
  exit. The rule was corrected: such launches must instead confirm profile isolation and identity.
* **First `005023Z` alone session.** The harness assumed a new firearm carries one state token. It carries none in the
  default state. The sessions now set a real loaded state, as the product's token store does, and require exactly that
  token.
* **First `010301Z` CMI session.** A fixed four-second panel window recorded 0 repaints. Diagnostic `011015Z` drew 145
  (product) and 181 (CMI) repaints in game. The probe now requires 30 repaints within a bounded timeout.

## Not covered

* **Loader versions.** 0.27.10 was unavailable and has no claim. 0.27.12, other 0.31.x, 0.32.0-0.32.3 as published, and
  0.33.1 or later were not run.
* **Installers.** The original Nexus installer ZIPs for 0.31.1 and 0.32.4 were not run; those two profiles are
  component-assembled. A user installation could differ, for example in the doorstop proxy build.
* **Harmony 2.3.5.** The early-0.32 Harmony 2.3.5 combination was not run with this mod. The Cutscene Skip work
  reproduced Craft Magic Items failing on it without any other mod. This mod's own Harmony 2 lookups resolve on 2.3.1.1
  and 2.3.6.
* **Call of the Wild / Favored Class.** Not run on 0.32.4. Native character creation and level-up were committed in the
  candidate-alone profile only; the CotW profile observed the race menus on detached units.
* **CMI with firearms.** A real attack or reload with the CMI-crafted firearm was not driven. Firearm attack and reload
  were exercised on the base firearms; the crafted item's type, classification and state carrier were verified.
* **CMI crafting route.** The CMI upgrade used CMI's own code path, invoked by the harness, rather than mouse input on the
  crafting window. CMI's IMGUI crafting route was exercised for ammunition by the product's own scenario.
* **Larger mod stacks.** The owner's full 16-mod stack was not part of this matrix. Two of its mods (Kingmaker Dice Roller,
  Last Azlanti Preserver) declare `ManagerVersion` 0.32.0 and 0.33.0, so a 0.31.1 loader would be expected to refuse
  them by their own declaration (not tested here). That is unrelated to this mod.
* **Owner's data folder.** Each launch overwrote the owner's shared `output_log.txt` (the player log, not a save). It was
  copied into each launch's evidence.
* **Packaged documentation.** The candidate ZIP's bundled `INSTALLATION-COMPATIBILITY.md` still has the pre-change
  requirement wording. The DLL and `Info.json` are the tested identity; a release build carries the updated text.

## Proposed Nexus wording

> **Requirements:** Pathfinder: Kingmaker – Enhanced Plus Edition 2.1.7b (Steam) with Unity Mod Manager **0.31.1 or
> newer**.
>
> **Tested with** Unity Mod Manager 0.31.1, 0.32.4 and 0.33.0, alone and together with Craft Magic Items 2.1.0 and
> Kingmaker Bugfixes 1.3.3. Call of the Wild and Favored Class were also tested on 0.31.1 and 0.33.0. Other versions
> have not been tested. If your newer Unity Mod Manager already works, keep it; there is no need to downgrade.
>
> Install the mod's ZIP through Unity Mod Manager as usual. It does not include, replace or require changes to Unity Mod
> Manager, Harmony or any game files.
>
> If Craft Magic Items fails to load on an early 0.32 release of Unity Mod Manager, that is a known Harmony issue on
> those releases and happens without this mod too. Unity Mod Manager 0.32.4 or newer, or 0.31.1, avoids it.

The last paragraph rests on the Cutscene Skip work's synthetic reproduction (UMM 0.32.3 with Harmony 2.3.5) and the
public reports it cites. Which published early-0.32 builds actually shipped Harmony 2.3.5 is unverified. Keep or soften
that paragraph at the owner's discretion.
