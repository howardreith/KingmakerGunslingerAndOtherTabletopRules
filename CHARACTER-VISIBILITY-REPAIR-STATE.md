# Character Visibility Repair — Working State and Resume Record

Last updated: 2026-09-11 (repair implemented; source qualification passed;
runtime qualification of the repaired build pending)

## Current position

- Branch: `codex/z-character-visibility-repair` (from master `221f6080`).
- Phase: root cause reproduced (run 04) and repair implemented in
  `ElementalRaces/Visuals`; full static qualification passed (repository
  validation with 1,566 deterministic tests, complete domain suite, clean
  Release build, strict package validation). The repaired build has NOT yet
  been deployed or runtime-qualified.

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

## Static root-cause model (2026-09-10, IL-verified)

`Game.LoadArea` (every area transition, including `Game.ReloadArea()` =
`LoadArea(current, null, AutoSaveMode.None, true, null)`) starts
`Game.UnloadUnusedAssetsCoroutine`, which runs:

1. `ResourcesLibrary.CleanupLoadedCache()` — for every `s_LoadedResources`
   entry with `RequestCounter <= 0` (except pooled particle GameObjects):
   `LoadedResource.Unload()` = bundle `Unload(true)` + `Object.Destroy`,
   then evicts the cache entry. Survivors' counters are reset to 0.
2. `GC.Collect()`.
3. `Resources.UnloadUnusedAssets()` — unloads natively-unreferenced assets
   (donor EquipmentEntities are referenced only from managed state).

`RequestCounter` semantics: `TryGetResource`/`LoadResource` increment on every
cache hit; `CleanupLoadedCache` resets to 0 after each pass. It is a
"touched-since-last-cleanup" flag.

KMG impact:
- The 28 registered proxies were injected via `new LoadedResource(proxy)`
  (counter 0) and are NEVER loaded through the native load API — the counter
  only exceeds 0 incidentally (bootstrap validation `TryGetResource` calls,
  creator/world entity-link loads). Any area boundary where nothing touched
  them since the previous reset destroys and evicts them.
- The 29 donors/palette sources are native assets but are not part of any
  creator/world link lists (the proxies replaced them), so their counters also
  decay to 0, and their objects are additionally natively unreferenced.
- Consequences after the boundary: `RetainCharacterCreatorResources` throws
  ("Owned character creator visual resource was lost:" /
  "Native visual donor was unloaded:") on the FIRST `DollStateUpdated` of any
  creator session for ANY race (the check is unconditional); Harmony 1.2
  prefix exceptions skip the native method, so `LateUpdate`'s
  `m_DollStateForUpdate` is never cleared and the doll never updates/assembles
  again (invisible preview bodies for new game and mercenaries alike).
  Committed elemental characters lose their body at the next area entry when
  the evicted proxy fails to resolve, while class clothing/firearm entities
  reload from their native bundles — exactly "invisible body, clothes and
  weapons visible", persisting in the world.

Why earlier qualification missed it: the 0.0.117-era creator/persistence runs
loaded one save and created/committed within ONE loaded-area session; the
proxies' bootstrap counters carried them through that single cleanup, and no
second boundary ran between registration and the creators. The visual
qualification asserted inner-asset liveness during creators, not across area
boundaries.

## Probe implementation (diagnostic instrumentation, in progress)

New guarded scenario `working-save-creator-visual-lifecycle` (request-scoped,
inactive in ordinary gameplay):
- Loads `KMG_AUTOMATION_WORKING` through the established identity-verified
  smoke machinery; creator visit 1 creates and commits one real native
  mercenary fixture (`CustomCompanion` unit, real `CharacterBuildController`)
  through final review.
- CaptureLifecycleCommit: records the committed `DollData`, its body proxy
  asset ID, a registry liveness snapshot (cache membership, RequestCounter,
  object/inner-asset liveness, which retention precondition would fail first
  and for which asset), and a native `DollData.CreateUnitView` world-appearance
  checkpoint (renderable renderers, baked Renderer_Character_* count, null
  materials/shaders, body entity presence).
- The visit-1 fixture is intentionally kept registered across one
  `Game.Instance.ReloadArea()` (the exact native area boundary, AutoSaveMode
  None, save-write sentinels retained). After it settles: snapshot again,
  world-view evidence again, retire the fixture through the normal owned
  cleanup with exact restoration.
- Creator visit 2 runs the same real creation path after the boundary; each
  capture includes doll-room state (`m_DollStateForUpdate` stuck, loaded links,
  avatar entity counts) as renderer-independent evidence of the aborted native
  update.
- All observations read-only; no donor reloads, proxy rebuilds, or retention
  extensions performed by the probe.

Wiring: `RuntimeTestScenarioCatalog` (constant + allowlist),
`RuntimeTestRequest` (validation: 4 params, working-smoke prerequisites, exit
required), `RuntimeTestRunner` (dispatch, working-save construction),
`ElementalCharacterCreationBaselineScenario` (mode + boundary insertion +
deferred cleanup + assertions), new partial
`ElementalCharacterCreationVisualLifecycle.cs`, PS metadata/preflight/invoke
entries.

## Reproduction evidence (2026-09-11, run 04, unfixed build)

Guarded run `20260911T0204512649293Z-working-save-creator-visual-lifecycle`
(current source 6c855f7d ≡ owner-installed production + diagnostic probe only;
full owner stack; KMG_AUTOMATION_WORKING through the identity-verified load;
real native mercenary creator; commit d443a1ce-equivalent artifact deployed by
the harness):

- Visit 1 (Ifrit Fighter point-buy) completed the real native creator and
  `CharacterBuildController.Commit` — the phases are NOT blocked.
- `lifecycleCommit.registry` at the moment of commit:
  - ALL 28 registered proxies: objectAlive=true, cacheContains=true, yet
    state=owned-inner-asset-destroyed (materials/textures destroyed).
  - 74 of 75 native donors: donor-inner-asset-destroyed (one survivor).
  - Body.Male requestCounter=31, Head.Male.01=38 (heavily resolved during the
    creator); every unused proxy/donor counter=0.
- `committedWorldView` (native `DollData.CreateUnitView`): view created, 5
  avatar entities with correct names (KMG Ifrit body proxy + head + eyebrows +
  hair + empty facial), body resource resolves and IS on the avatar —
  **renderableRenderers=0 and bakedCharacterRenderers=0**: the committed
  character cannot draw its body/head. This is the owner's invisible body,
  renderer-verified, before any area boundary.
- First destroyed resources: shared materials/textures owned by every proxy —
  `Character_Diffuse_Cutout`, `lambert1`, `whiteSquare`,
  `Character_Diffuse_EmissionCharacter_Cutout` (first owner recorded: Ifrit
  Head Male 02 proxy `f9f25529-...`).
- The retention prefix did NOT throw during the creator (log contains exactly
  one `visual-retained` with additions=579 and zero retention ERRORs): the
  destruction happens in the LAST `UpdateDollCoroutine` removal pass, after the
  final `DollStateUpdated`. The mission's hypothesized prefix exception is the
  SECONDARY effect for the NEXT creator session; the primary destruction is the
  native removal pass's `ResourcesLibrary.TryUnloadResource` →
  `LoadedResource.Unload()` → `AssetBundle.Unload(true)`, which force-destroys
  shared bundle materials still referenced by every live proxy clone
  (`UnloadInnerAssetsExceptGiven` exceptions cannot protect against it — the
  0.0.117-era donor-ID retention covered the donors themselves but not other
  bundle-mates unloaded by the creator's own removal planning).
- Native clothes/weapons reload from their bundles on demand, so they remain
  visible — matching the report. Proxy bodies/heads cannot reload (they are
  cache-only clones), so they stay invisible — matching "remains after
  creation".

Run 05 (in flight) adds a read-only `TryUnloadResource` recorder to name the
exact unloaded asset and caller stack.

## Run 05 outcome (2026-09-11)

Guarded run `20260911T0214573548640Z-working-save-creator-visual-lifecycle`
(build f3b6e789 ≡ 0.0.122 production + probe): FAIL after 69 s. Creator visit
1's request-local cleanup threw
`Registered visual inner assets destroyed during creator`
(KMG_ElementalRaces_Ifrit_Race; first destroyed: `Character_Diffuse_Cutout`,
`lambert1`, `whiteSquare`, `Character_Diffuse_EmissionCharacter_Cutout` under
Ifrit Head Male 02 proxy `f9f25529-...`) — the same shared-material set as
run 04, re-confirming the destruction boundary at the creator's own removal
pass. The abort preceded snapshot capture and visit 2, so the
`nativeTryUnloadResource` recorder entries were never extracted; the static
IL model (removal pass → `TryUnloadResource` → `LoadedResource.Unload()` →
bundle `Unload(true)` force-destroys shared bundle-mates) plus two
independent runtime reproductions remain the causal record. No new launch
environment problem; the FAIL is the probe's fail-closed cleanup assertion.

## Repair implementation (2026-09-11, this branch, uncommitted→committed)

Design (mission doc §5 "protect/reconstruct" direction — reconstruct, never
unload/pin/displace foreign resources):

- `ElementalVisualResourceRecoveryPolicy.cs` (new): pure damage
  classification (owned proxy: destroyed > evicted > replaced >
  inner-assets-destroyed; native dependency: evicted > destroyed > replaced >
  inner-assets-destroyed), recoverability (all kinds except
  native-dependency-replaced, which fails closed), all-or-nothing retention
  gating, 2 s report/recovery rate gate.
- `ElementalVisualResourceRecovery.cs` (new): boundary `Heal` — evicts dead
  native-dependency corpses so the native loader reloads from bundle
  (rebind validated by recorded original object name), re-clones owned
  proxies from catalog provenance (donor reloaded + validated by expected
  name, palette re-derived, fallback donor as last resort) under the
  original stable GUIDs via `ReplaceOwnedRegistration` (refuses to displace a
  foreign object), rate-gated, reports remaining damage.
- `ElementalRaceVisualResourceRegistry.cs`: `AssessDamage`,
  `ReplaceOwnedRegistration` (swap cache entry + `RebindResource` in place so
  every registration holder observes the healed instance),
  `TryRebindNativeDependency`/`EvictDeadNativeDependency`,
  `ArmRetentionCounters` (sets `RequestCounter=1` on exactly the registered
  identities after each native cleanup so counter-decay cannot evict them),
  provenance name record for donors.
- `ElementalRaceVisualBlueprintSet.cs`: `EnsureCreatorResourcesRetained` —
  heal first, then extend native retention only when no damage remains.
- `ElementalRaceVisualFactory.cs`: `RecreateProxy`/`RecreatePalette`
  (recovery entry points reusing the exact construction contract).
- `ElementalCharGenVisualRetentionPatch.cs`: the `DollStateUpdated` prefix now
  heals + isolates its own failures (a prefix exception can no longer abort
  native doll update — the secondary stuck-`m_DollStateForUpdate` effect);
  new `ResourcesLibrary.CleanupLoadedCache` postfix (heal + re-arm counters
  after every native cleanup/area boundary); new `CharGenDollRoom.OnDisable`
  postfix (heal when the creator closes so the committed world unit never
  spawns against destroyed proxies — covers the primary damage that occurs
  after the final `DollStateUpdated`).

Boundaries fire only through native lifecycle methods already IL-verified
(`CleanupLoadedCache`, `OnDisable` confirmed present in the retained IL).
No foreign resource is unloaded, pinned, or displaced; blueprint/GUID
identities unchanged; no new dependencies.

Static qualification (2026-09-11, all PASS):
- Repository validation via `validate_teleportation122.py` chain with
  deterministic test count advanced 1,561 → 1,566 (5 new
  `elemental-visual-recovery.*` cases; both live static blocks updated).
- Complete domain suite: 1,566 tests, failures=0.
- Clean Release build + build-output validation.
- Strict standalone UMM package validation;
  `KingmakerGunslinger-0.0.122-local-runtime.zip`
  SHA-256 `cf1afd4e4cffc2f085c548a2b78cef982b2f09b1b2fce7c0b8aac79e20c6a185`,
  DLL SHA-256 `daa2ee906792622fbd90f88fb208ffd735af1aa5d71465e6bf2cbd138ebf3711`.

## Repair v2 (2026-09-11, after run 06 diagnosis)

Run 06 (`20260911T0456251445313Z`, build 3c0281e4, log preserved as
`repair-run-06-output_log.txt`) FAILED identically to run 05, and its
recorder finally named the true destroyers (probe `assetUnloads`, caller
`CharGenDollRoom.UpdateDollCoroutine`):

1. `EquipmentEntity.UnloadInnerAssetsExceptGiven` on removed NATIVE donor
   entities (EE_Naked_M_TF, EE_Head_Face01_M_TF, EE_HornsTieflingRam_M_TF —
   the Ifrit body/head/horn DONORS; `guid:null` proves the removed instances
   were not our proxies) destroyed ramp textures absent from the exception
   set — ALL seven CR_Horns_* ramps and CR_Skin_White/Blue/DarkBlue. The
   0.0.117-era retention extended only `GetInnerAssets()`, never
   `PrimaryRamps`/`SecondaryRamps`.
2. `ResourcesLibrary.TryUnloadResource(donorId)` → `LoadedResource.Unload()`
   → `LoadedBundle.Unload(true)` force-destroyed the shared bundle-mate
   materials (Character_Diffuse_Cutout, lambert1, whiteSquare,
   shared_aiStandard10) still referenced by every proxy clone — these were
   alive immediately after the inner-asset call and dead by cleanup.
3. The v1 heal was ineffective: `HealNativeDependency`'s
   `if (current != null)` gate skipped eviction exactly when the cache held a
   corpse, and live-but-gutted donors had no reload path at all; proxies were
   also healed before the donors they clone from. Three heal boundaries fired
   (loaded-cache-cleanup, character-creator-update ×2) each reporting
   damaged=102, recloned=28, rebound=74, remaining=102 — the IL-verified
   cache-hit-without-liveness-check behavior confirmed.

Repair v2 (all static qualification PASS; 1,567 deterministic tests;
local-runtime package SHA-256
`5396494a671c2461d89ce8307e583b2f14a27889795c038336186dfa96ef8fe4`, DLL
SHA-256 `556eb5217293c7ed076aa99088fff00831b1b18871c78ff2c3229471bb42227d`):

- Retention completeness: the creator retention plan now extends the native
  exception set with proxy AND donor ramp textures
  (`ProtectedInnerAssets`), not just `GetInnerAssets()`.
- Donor unload guard: `TryUnloadResource` prefix keeps the exact 75
  registered native dependency identities cached and alive (reports success,
  skips the bundle unload); no foreign resource is retained.
- Native anchor: hidden `DontDestroyOnLoad` GameObject
  (`ElementalVisualResourceAnchor`) holds references to the 28 proxies and
  75 donors so Unity's unused-asset sweep cannot destroy them; cache
  counters armed at set construction so the first counter-based cleanup
  cannot evict them; anchor refreshed on every rebind/replace and destroyed
  on rollback.
- Heal correctness: damaged dependency entries (corpse or live-but-gutted
  registered instance) are evicted before reload
  (`EvictReloadableNativeDependency` + policy), dependencies heal before
  proxies, recloned proxies are integrity-checked before registration, and
  the native loader's name-only validation is backed by Unity liveness
  checks on every accepted instance.

## Next exact action

1. Deploy repair v2 through the guarded harness and re-run
   `working-save-creator-visual-lifecycle` (expect: no inner-asset
   destruction during creator, both visits complete, renderable committed
   world view, boundary survival).
2. Then run the canonical `working-save-smoke`.
3. Then both reported creation paths per mission gate 4 — mercenary
   recruitment covered by the lifecycle scenario; new-game creation coverage
   still needs its guarded scenario or an explicitly documented equivalent
   (0.0.117 history forbids DefaultPlayerCharacter-in-loaded-campaign as a
   substitute).
4. Record evidence, update the report, final checkpoint.

## Deployment / installation state

- No deployment performed by this mission. Installed artifact remains the
  0.0.122 teleportation build (see baseline identities). No backup taken yet
  (take immediately before any install).
