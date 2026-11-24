# Unity Mod Manager compatibility (declared minimum 0.31.1)

Status: **released.** v0.0.188 (GitHub release 2026-11-14, tag commit `097e522f9f`) was the first release to declare
Unity Mod Manager 0.31.1, and every release since keeps it. Verified 2026-11-24 from the published GitHub assets:

| Release | ZIP SHA-256 | `Info.json` `ManagerVersion` | Packaged DLL SHA-256 | DLL loader references |
|---|---|---|---|---|
| v0.0.188 (`KingmakerGunslinger-0.0.188-umm-0311-minimum.zip`) | `64d83955478c3824c5fee485f4189b7d0be1141938a89e70e4cdab037461d412` | 0.31.1 | `0eb2f0027f1b8639cb8988f24fffda263cf92bfdadd180f40b1aebe79019b151` | `UnityModManager 0.31.1.0`, `0Harmony12 1.2.0.1` |
| v0.0.195 (`KingmakerGunslinger-0.0.195-gunslinger-remediation-sprint6.zip`, tag commit `ef0e1fe15d`) | `456d504ddde0ad3d0bb41a7dc141848a99feeee83d9665db9eeadcd95c359444` | 0.31.1 | `3ef052ea441af368381b1cea7d2ee7acaa813000986de8644430ff63ee1d6a6b` | `UnityModManager 0.31.1.0`, `0Harmony12 1.2.0.1` |

Each package contains exactly one DLL (the mod's), and its bundled `INSTALLATION-COMPATIBILITY.md` states the 0.31.1
minimum. The DLL references were read from the AssemblyRef table of the packaged file. The newer loaders remain separate
compatibility targets; the in-game matrix below was run once, on the development candidate that became v0.0.188.

## Current development candidate: pinned-loader build change (2026-11-24)

This candidate checks the pinned-loader build change (PR #220). It is a development receipt, not a release.

| Item | Value |
|---|---|
| Producing commit | `91139e4368` |
| Command | `scripts\Build-Local.ps1 -DevelopmentPurpose umm-0311-baseline -DevelopmentTestFilter model-d.ItemIdentityEconomicsEnchantmentsAndCraftingUnchanged -ReferenceBundleDir <private bundle>`, about 8 minutes (validation summary 07:46:27 UTC, receipt 07:54:13 UTC; its console log was not retained), 1 of 3189 registered domain tests selected and passed. The loader references of this DLL are checked by `validate-package.ps1` after the exact build, and the receipt records the exact compiler output, which equals the packaged DLL (`compilerDllSha256` = `packagedDllSha256`), with the embedded source stamp `91139e4368` |
| Compiler loader references | `UnityModManager.dll` 0.31.1.0 `2e038ff2…c2c4` and `0Harmony12.dll` 1.2.0.1 `aa1cd483…1292` from `artifacts\references\loader` (pins file `b13bed40…`) |
| Package | `KingmakerGunslinger-0.0.195-development-umm-0311-baseline-91139e4368ed.zip`, SHA-256 `376ce2532e7895bcb7dfe6bea89177e8cb93a853bc81136ac285c58f4698edd0` |
| Packaged DLL | SHA-256 `1ef167cca4135ef8f561ac437c55984ed59777c3962012bf90d738cdc4d1c0e9`, MVID `7c7a1479-98af-4eea-af25-e88ea9986839` |
| DLL loader references (AssemblyRef table) | `UnityModManager, Version=0.31.1.0, Culture=neutral, PublicKeyToken=null`, `0Harmony12, Version=1.2.0.1, Culture=neutral, PublicKeyToken=null` |
| Standalone validation | `scripts\validate-package.ps1 -PassThru`: passed (one binary, complete archive entry set, packaged DLL references equal the pins) |
| Candidate admission | `scripts\Test-CandidateAdmission.ps1`: 29/29 on a development candidate that Build-Local built in a temporary clone of `91139e4368`. The matching candidate is admitted. Refused before staging: a newer-compiled DLL (v0.0.187, UnityModManager 0.33.0.0); an extra runtime DLL at the archive root or in the mod tree; a noncanonical entry landing beside the mod directory; an array-valued minimum; a receipt naming another build's MVID or DLL hash; another baseline-compatible build (the v0.0.195 DLL, stamped `ef0e1fe15d`) supplying both the loose and packaged DLL under a receipt naming this producer; a candidate after a project, reference-helper, shipped README or packaging-helper change. A change confined to `docs/` keeps the candidate admitted. A relative package path after a location change is accepted. Historical control: the recorded v0.0.188 release is admitted, while a changed archive, a same-version development package and conflicting mode arguments are refused. Staging refuses a package replaced before the private copy and stages only the admitted bytes after the supplied path changes (Deploy-Local and loader harness). A purpose containing an apostrophe gets the purpose-validation error before any child process. |

**Static loader audit of this DLL.** The frozen packaged DLL was checked against four deduplicated targets. Each target is
a UnityModManager build plus the Harmony 2 it references and 0Harmony12 1.2.0.1:
- UMM 0.31.1 (the declared minimum);
- UMM 0.32.4;
- the owner's installed 0.33.0;
- 0.33.4, the newest stable release (author NuGet package dated 2026-11-19; GitHub HEAD `3d14bf0d`, project version
  0.33.4).

Identities and provenance are in the fleet profile record (`docs/evidence/umm-0311/loader-profiles.md`). Each bound
UnityModManager, 0Harmony12 and 0Harmony member (23 types, 67 members) resolves by name and decoded signature on all
four: **RESOLVED** on each. Tool: `loader-api-audit-Program.cs.txt` (.NET 8), invoked as
`LoaderApiAudit.exe --mod <dll> --json <out> --assemblies UnityModManager,0Harmony12,0Harmony --profile "<id>=<UnityModManager.dll>;<0Harmony.dll>;<0Harmony12.dll>"`
per profile. This is static evidence, not a native run.

**Native rows for this candidate: NOT RUN.**

| Loader | Reason |
|---|---|
| 0.31.1, 0.32.4, 0.33.0 | This repository's guarded harness runs in the disposable game copy `game-copies\loader-compat`. Another enrollment (`kmg-codex-s7-visual-audio-remediation`) was launching games from that copy repeatedly on 2026-11-24, so this candidate was not run there. The owner installation is not used for these runs. |
| 0.33.4 | No reviewed `umm-0.33.4` harness profile exists (only `umm-0.31.1`, `umm-0.32.4`, `owner033`). |

No combined-stack check ran for this candidate. The rows below describe the 2026-11-14 candidate.

The rest of this record is the historical account of that development candidate (`b9400333`, built as version 0.0.187
before v0.0.188 was opened). Its identities, results and gaps are kept as they were recorded.

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
* **Every MSBuild compile (added 2026-11-24).** Until then the project's default `UnityModManagerDir` was the installed
  loader (`<KingmakerInstallDir>\Kingmaker_Data\Managed\UnityModManager`), so a direct MSBuild and the development
  compiles of `scripts\build.ps1` and `Invoke-KmgGate.ps1` were compiled against whichever loader was installed.
  `scripts\Publish-Release.ps1` has two packaging routes. With a private reference bundle it packages Build-Local's
  exact-reference compiler output (`tools\build_mod_from_private_references.py`, given the pinned loader directory).
  Without one it runs `build.ps1` and `package.ps1`, the MSBuild route, which compiled against the project's
  `UnityModManagerDir` (the installed loader by default until this change). The verified assurance is therefore limited
  to v0.0.188 and v0.0.195, whose packaged DLLs reference UnityModManager 0.31.1.0 and 0Harmony12 1.2.0.1 (status table);
  this record makes no claim about other builds. The default is now the pinned cache
  `artifacts\references\loader\UnityModManager`, which `scripts\build.ps1` fetches and verifies first. Before
  references resolve, the project verifies the folder it is given against the pins (`scripts\Assert-PinnedLoaderReferences.ps1`),
  so a direct MSBuild pointed at the installed loader or at altered files is refused; after reference selection, the
  UnityModManager and 0Harmony12 inputs Csc receives must be exactly those verified files.
* **Game and Unity references** keep their exact bundle-equals-installation check.
* **Receipts.** Build receipts record each loader row's role, assembly version, source URL and the pins hash.
  Development receipts are refused if their loader rows differ from the current pins. A changed pins file changes the
  source state, which invalidates earlier receipts.
* **Package validation.** The packaged `ManagerVersion` must equal the pinned floor. The package must contain exactly one
  binary, the mod, and no loader, Harmony, dnlib, game or Unity assembly.
* **Packaged DLL and admission (added 2026-11-24).**
  - `validate-package.ps1` resolves the package path once. It checks every archive entry's normalized destination (under
    the mod root, no `.`/`..` segment, no duplicate, nothing at the extraction root) and extracts exactly those entries.
    It reads the AssemblyRef table of the DLL inside the ZIP (isolated reflection-only load) and requires exactly the
    pinned `UnityModManager` and `0Harmony12` identities, and a string `ManagerVersion` equal to the pinned floor.
  - Build-Local measures the exact compiler output in its private folder. The DLL inside the finished ZIP and the loose
    copy must equal it before either receipt is written. The receipt records the compiler and packaged identities and the
    embedded source stamp (`AssemblyMetadata` `GitCommit`).
  - Candidate admission (`Deploy-Local.ps1`, the loader harness preflight) requires the packaged DLL's SHA-256 and MVID to
    equal the receipt's, and its embedded source stamp to be the receipt's producing commit. Every file outside `docs/`
    must be unchanged since that commit: product sources, shipped files, assets and every build, packaging and
    validation helper. A change confined to documentation under `docs/` keeps the candidate admissible.
  - Staging copies the admitted package privately and requires the copy's digest to be the admitted one. It extracts the
    checked entries and requires the extracted DLL to be the admitted DLL before any target is replaced, so bytes that
    replace the supplied path after admission are never staged.
  - The loader harness admits in its own process, with every value passed as a bound parameter. A development purpose is
    validated before that process starts, and the harness files are checked first.
  - A released package is admitted only as an explicit historical release control: `-HistoricalReleaseControl` with
    `-HistoricalReleaseVersion`, exclusive of `-DevelopmentPurpose`. It is checked against the committed release record
    `scripts/historical-release-records.json`: package digest, release tag resolving to the recorded commit, and
    packaged DLL digest, MVID and source stamp.
  - Direct MSBuild checks `Info.json` `ManagerVersion` against the pins.
  - A pin failure names its recovery. An absent file is fetched by `scripts\build.ps1`. A file present with different
    bytes is left untouched, to be inspected and removed by hand before rerunning (as in `docs\BUILDING.md`).
* **Tests.**
  - `scripts/Test-LoaderReferencePins.ps1` (35 checks: fixture pins, and stub assemblies of known identity for the
    package reference cases) and `tools/test_loader_reference_pins.py` (5) run inside `validate-repository.ps1`.
  - `scripts/Test-PinnedLoaderMsBuild.ps1` (12 checks) runs MSBuild on a temporary clone. The historical control is the
    pinned commit `35763430ac`, with its checkout verified. `build.ps1` and a direct MSBuild compile against the pinned
    files. Loader sets are judged by bytes, on controlled reference directories (exact set accepted; UMM-only mismatch and
    Harmony-only mismatch refused naming the differing file) and on the installation, read only, with the expected
    verdict taken from measurement. Refused before compilation: an altered copy, a folder not named `UnityModManager`, a
    mismatched `Info.json` minimum, and a stray HintPath to a copy outside the verified folder.
  - `scripts/Test-CandidateAdmission.ps1` (29 checks) runs on a real Build-Local development candidate in a temporary
    clone (see the current development candidate above). It covers package entries, receipt binding including another
    baseline-compatible build's source stamp, material inputs, historical release controls, staging from the admitted copy
    and the purpose passed as data.
  - The domain check `loader.production-dll-references-pinned-loader` inspects a verified compiler output wherever it is
    selected. `test-domain.ps1` binds the DLL that `build.ps1` or the focused gate has just compiled, or compiles one
    itself (for a complete-suite run), by path and SHA-256, with the expected identities read by the pin reader. A
    mismatched binding is refused.
  - `Test-DevelopmentCandidate.ps1` proves that the compiler receives the pinned directory and that a tampered loader is
    refused before any child step.

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
* **Packaged documentation.** The candidate ZIP's bundled `INSTALLATION-COMPATIBILITY.md` still had the pre-change
  requirement wording. The DLL and `Info.json` are the tested identity. The released v0.0.188 and v0.0.195 packages carry
  the updated text (see the status table).

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
