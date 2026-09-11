# Character Visibility Repair — Working State and Resume Record

Last updated: 2026-09-10 (mission start checkpoint 0)

## Current position

- Branch: `codex/z-character-visibility-repair` (from master `221f6080`).
- Phase: baseline established; investigation in progress; NO source change
  made yet; NO game launch performed by this mission yet.
- Working tree: mission docs + durable records only (plus the untracked
  assignment document). Nothing else modified.

## Exact resume instructions

1. Read `Kingmaker_Character_Visibility_Repair_Z_Mission.md`,
   `AGENTS.md`, this file, and
   `CHARACTER-VISIBILITY-REPAIR-MISSION.md` fully.
2. Verify `git status`, current branch, and that no other agent holds a
   runtime/deployment lease (check running Kingmaker processes and
   `runtime-evidence/deployments`).
3. Continue from the "Next exact action" below.
4. Before any game launch: preserve the current
   `%USERPROFILE%\AppData\LocalLow\Owlcat Games\Pathfinder Kingmaker\Unity\output_log.txt`
   (note: at mission start the log had been rotated to the LocalLow root —
   check both locations) into
   `C:/Dev/KingmakerGunslingerLab/runtime-evidence/character-visibility-repair/`.
5. Validation entry points:
   `powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Build-Local.ps1`
   (repository validation + domain tests + Release build + packaging +
   package validation). Guarded launches:
   `scripts/Invoke-KingmakerRuntimeTest.ps1`.
6. Publish checkpoints only via
   `powershell.exe -NoProfile -ExecutionPolicy Bypass -File C:/Dev/KingmakerGunslingerLab/codex-policy/Push-KingmakerGunslinger.ps1`.

## Evidence ledger

| Date (UTC) | Item | Location / identity |
| --- | --- | --- |
| 2026-09-10 | Preserved 0.0.121 automated-session log (2.3 MB; owner failing-session log already lost before mission start) | `runtime-evidence/character-visibility-repair/baseline-20260910/output_log_root_copy.txt` |
| 2026-09-10 | Installed 0.0.122 DLL hash recorded | `6965AAB9326BBEEF6DFF845B46DD6912C8081BBC87E6EEF88F875A0368E9CEC1` (7,706,624 bytes) |

## Hypotheses

| # | Statement | Status |
| --- | --- | --- |
| H1 | A lost/destroyed elemental visual resource makes `RetainCharacterCreatorResources` throw inside the `DollStateUpdated` prefix, interrupting the native doll update for any race and leaving the body unassembled while equipment (loaded separately) stays visible. | Unverified — previous review's hypothesis; code path confirmed present, trigger conditions unproven. |
| H2 | The regression entered between the qualified 0.0.117 creator runs and 0.0.121/0.0.122 through interaction with newer changes (firearm presentation, teleportation, vendor work) rather than the Visuals sources (unchanged since 0.0.120-era `df6b28c3`). | Unverified. |
| H3 | The failing boundary is specific to a lifecycle not covered by the 0.0.117 qualification (mercenary recruitment path, post-commit world appearance, scene transition/save-load, fresh-process second creation). | Unverified. |

## Investigation findings (2026-09-10, static phase)

Native contract facts (ildasm of installed Assembly-CSharp.dll, Unity 2018.4.10f1;
IL retained machine-locally under `C:/Dev/KingmakerGunslingerLab/private/charvis-native-il/`):

- `CharGenDollRoom.HandleDollStateUpdated` (EventBus `ILevelUpDollHandler`) only
  stores the state; `LateUpdate()` invokes `DollStateUpdated(state)` per frame and
  clears `m_DollStateForUpdate` only AFTER the call returns. A throwing Harmony
  prefix therefore re-fires every frame with the stale state, and the native
  update (load/removal planning + `UpdateDollCoroutine` start) is skipped
  entirely.
- `DollStateUpdated` stops the prior update coroutine, starts
  `LoadEquipmentEntitiesCoroutine`, computes to-load/to-remove lists (entities
  in `m_InitiallyLoadedEquipmentEntityIds` are never removed), then starts
  `UpdateDollCoroutine`.
- `UpdateDollCoroutine`: builds an inner-asset exclusion set from currently
  loaded entities plus `m_InitiallyLoadedEquipmentEntityInnerAssets`, then for
  each to-remove entity calls `UnloadInnerAssetsExceptGiven` +
  `ResourcesLibrary.TryUnloadResource`; when `m_LoadsDone > 100` it calls
  `Resources.UnloadUnusedAssets()` and resets the counter.
  `LoadEquipmentEntitiesCoroutine` increments `m_LoadsDone` once per loaded
  equipment-entity link.
- `LoadedResource.Unload()` = `LoadedBundle.Unload(true)` + `Destroy(Resource)`.
- Model assembly happens in `DollStateLoadedAllEquipmentEntities`:
  `SetAvatar`, `SetSkeleton` (from `DollState.GetSkeleton()`),
  `RemoveAllEquipmentEntities(false)`, then
  `AddEquipmentEntities(CollectEntitiesLinks().Select(link -> TryGetResource))`
  and `ApplyRamps`. A body link that resolves to a destroyed/unloaded resource
  yields exactly "body missing while other entities render" — the reported
  symptom shape.

Hypothesis refinement:

| # | Statement | Status |
| --- | --- | --- |
| H1 | Lost elemental resource makes `RetainCharacterCreatorResources` throw in the `DollStateUpdated` prefix; LateUpdate retries with stale state; native doll update never runs; preview body never assembles. | Mechanism confirmed statically; trigger unproven. |
| H4 | Long manual creation sessions exceed `m_LoadsDone > 100` (each hair/beard/head browse loads entities), firing native `Resources.UnloadUnusedAssets()`, which unloads native donor EquipmentEntities (referenced only from managed state). The next retention check then throws "Native visual donor was unloaded" for EVERY race. The bounded automated scenarios never reached 100 loads, so this boundary was never qualified. | Leading candidate; requires runtime proof. |

Visuals production code is unchanged since the qualified donor-retention work
(`520e2cd7`; only an additive property in `df6b28c3`), so a source regression in
Visuals is unlikely; the trigger is expected to be a lifecycle boundary the
0.0.117-era qualification did not cover.

Existing reusable machinery confirmed:
`ElementalCharacterCreationBaselineScenario` (real native creator, retention
checks, doll-room observation), `ElementalRaceVisualAuditScenario`
(world-view `Character.EquipmentEntities` verification),
`GunslingerOutfitProductionPersistenceScenario` (native `DollState.SetHair`
driving). `DollState` exposes `SetHair/SetBeard/SetHead/SetRacePreset` etc. for
driving the native browse path.

## Next exact action

1. Complete baseline `Build-Local.ps1` on master `221f6080` (running;
   log at `runtime-evidence/character-visibility-repair/baseline-20260910/build-local-master-221f6080.log`).
2. Commit mission records; keep tree clean for guarded runs.
3. Healthy control: guarded `disposable-elemental-character-creation-baseline`
   on current source (ExpectedVersion 0.0.122).
4. Causal probe: extend the baseline scenario with an optional long native
   browse phase (real `SetHair`/`SetBeard` cycling until `m_LoadsDone > 100`,
   read-only observation), then continue creation and capture the first dead
   resource, the first prefix exception, and renderer-level body state.

## Deployment / installation state

- No deployment performed by this mission. Installed artifact remains the
  0.0.122 teleportation build (see baseline identities). No backup taken yet
  (take immediately before any install).
