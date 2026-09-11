# Character Visibility Repair — Final Report

Mission: repair newly created characters having invisible bodies while clothes
and weapons remain visible (new-game creation and mercenary recruitment,
persisting after creation), in `howardreith/KingmakerGunslingerAndOtherTabletopRules`.

Branch: `codex/z-character-visibility-repair` (from master `221f6080`).
Status: **REPAIRED AND QUALIFIED through the guarded native creator paths
exercised in this mission; several mandatory §6 matrix cells NOT RUN in this
window (listed below). No merge, tag, release, or PR. Human visual acceptance
remains pending Howie's playtest.**

## 1. Root cause and first failing operation (evidence-backed)

**Primary destruction (naturally reproduced, unfixed build, runs 04/05/06,
guarded harness, full owner stack, `KMG_AUTOMATION_WORKING` identity-verified
load, real native mercenary creators):**

During ordinary native character creation, `CharGenDollRoom.UpdateDollCoroutine`
plans removals for entities dropped from the doll state (e.g. the previous
default-race body/head/hair after race selection). For each removed entity it
calls, in order:

1. `EquipmentEntity.UnloadInnerAssetsExceptGiven(exclusion)` — destroyed ramp
   textures that the 0.0.117-era retention never covered (it extended the
   exclusion set with `GetInnerAssets()` only, never `PrimaryRamps`/
   `SecondaryRamps`): all seven `CR_Horns_*` ramps, `CR_Skin_White/Blue/DarkBlue`
   (run 06 recorder: removed instances were the native Ifrit donors
   `EE_Naked_M_TF`, `EE_Head_Face01_M_TF`, `EE_HornsTieflingRam_M_TF`).
2. `ResourcesLibrary.TryUnloadResource(assetId)` → `LoadedResource.Unload()` →
   **`AssetBundle.Unload(true)`** — a forceful destroy of every asset loaded
   from that bundle, including the shared materials still referenced by every
   live KMG proxy clone: `Character_Diffuse_Cutout`, `lambert1`,
   `whiteSquare`, `Character_Diffuse_EmissionCharacter_Cutout`,
   `shared_aiStandard10` (alive immediately after the inner-asset call, dead
   by cleanup — run 06 recorder).

Result on the unfixed build (run 04, renderer-verified): the committed
character's world view contained all five correct entities — the registered
KMG Ifrit body proxy was present and resolved — but **zero renderable
renderers / zero baked `Renderer_Character_*` parts**: the body cannot draw.
All 28 registered proxies and 74/75 bound native donors had destroyed inner
assets after a single creator session. Native class clothing and weapons
reload from their bundles on demand, so they stayed visible — exactly the
reported symptom, and because proxies are cache-only clones that cannot
reload, it persisted after creation.

**Secondary effect (the mission's incoming hypothesis, confirmed as
consequence, not cause):** after resources are destroyed, the next creator
session's `DollStateUpdated` prefix (`RetainCharacterCreatorResources`) throws
"Owned character creator inner asset was destroyed:" — Harmony 1.2 prefix
exceptions skip the native method, `LateUpdate` never clears
`m_DollStateForUpdate`, and the doll never updates again for ANY race.

**Area-boundary destruction (IL-verified, run-06-corroborated):** every
`Game.LoadArea` (including `ReloadArea`) runs
`ResourcesLibrary.CleanupLoadedCache()`, which evicts and destroys every
loaded-resource entry whose `RequestCounter` decayed to zero, then
`Resources.UnloadUnusedAssets()`. KMG's injected proxies (counter 0, never
natively loaded) and untouched donors were destroyed/evicted at these
boundaries. The 0.0.117-era qualification never crossed a second boundary in
one process, which is why this remained latent.

## 2. The repair (scoped to the proven boundaries)

Production changes (`src/KingmakerGunslinger/ElementalRaces/Visuals/`):

- **Retention completeness** — the creator retention plan now extends the
  native exception set with proxy AND donor ramp textures
  (`ProtectedInnerAssets`), not only `GetInnerAssets()`.
- **Donor unload guard** — a `ResourcesLibrary.TryUnloadResource` prefix keeps
  the exact 75 registered native dependency identities cached and alive
  (reports success to the caller, skips the bundle unload). No foreign
  resource is retained or pinned.
- **Native anchor** — a hidden `DontDestroyOnLoad` GameObject holds references
  to the 28 proxies and 75 donors so Unity's unused-asset sweep cannot destroy
  them; cache counters are armed at set construction and after every native
  cleanup (`CleanupLoadedCache` postfix) so counter decay cannot evict them;
  the anchor is refreshed on every rebind/replace and destroyed on rollback.
- **Damage-aware recovery** (`ElementalVisualResourceRecovery`,
  `ElementalVisualResourceRecoveryPolicy`): read-only damage classification
  (Unity-destroyed vs CLR-null construction slots vs evicted vs replaced),
  provenance-validated reconstruction (donors reloaded from bundle and
  validated by recorded original object name plus Unity liveness; proxies
  re-cloned from catalog provenance under their original stable GUIDs;
  `ReplaceOwnedRegistration` refuses to displace a foreign object),
  dependencies healed before proxies, rate-gated (2 s), bounded per boundary.
- **Failure isolation** — the `DollStateUpdated` prefix heals first and can no
  longer abort the native creator update (the secondary stuck-doll effect);
  unrecoverable elemental states are isolated with rate-limited diagnostics
  while ordinary native character creation proceeds.
- **Boundaries**: `DollStateUpdated` prefix, `CharGenDollRoom.OnDisable`
  postfix (creator close — covers post-final-update damage), and
  `ResourcesLibrary.CleanupLoadedCache` postfix (every area boundary).

Preserved: all blueprint/GUID identities, appearance construction contracts,
foreign mods and their resources, native unload planning for non-registered
entities, settings, and saves. No new dependencies; no general graphics
framework; module behavior unchanged.

## 3. Unfixed-versus-fixed behavioral evidence

| Evidence | Unfixed build | Repaired build |
| --- | --- | --- |
| Creator session resource state | All 28 proxies + 74/75 donors inner-asset-destroyed in one session (runs 04/05/06) | Zero damage at every checkpoint across two committed creators (run 13); zero asset unloads across four consecutive main-menu creators (run 14) |
| Committed world view | 5/5 correct entities but 0 renderable renderers (cannot draw body) | Run 13 per-character acceptance PASS; world-view body entity present and resolving with healthy control-relative renderer metrics |
| Destructive native calls during creator | `UnloadInnerAssetsExceptGiven` destroyed un-excepted ramps; `TryUnloadResource` bundle unloads destroyed shared materials (run 06 recorder, caller stacks captured) | Zero destructive unload observations (runs 13/14 probes) |
| Native creation flow | Phases completed but committed character invisible; next session's doll update would abort on the retention exception | Both real mercenary creators commit; four main-menu creators complete their full selection contracts with clean cleanup |

## 4. Qualification performed in this mission (final source)

- Static: repository validation, 1,567 deterministic domain/reflection tests
  (0 failures), clean Release build, strict standalone UMM package validation.
- Guarded native runs (Steam App 640820, full owner stack unless noted):
  - `working-save-creator-visual-lifecycle` run 13: **PASS** — two real native
    mercenary (`CustomCompanion`) creators through commitment and world
    appearance, one native `ReloadArea` boundary between them, zero resource
    damage, exact semantic restoration.
  - `working-save-smoke` on the repaired build: **PASS** (canonical save-load
    contract).
  - `disposable-elemental-character-creation-baseline` run 14 (final HEAD
    `dc3367b5`): **PASS** — all four elemental races through the real
    main-menu full-screen creator, per-character acceptance PASS, zero
    unresolved selections, zero instrumentation failures, zero asset unloads.

## 5. Gates NOT RUN in this mission window (honest status)

Per mission §6, the following mandatory cells were not executed on the final
artifact in this window and remain open for a follow-up session (records and
probe scenarios are in place to continue cheaply):

- Main-menu new-game creator through native campaign-start commitment
  (distinct entry surface; 0.0.117-era history shows main-menu commit
  triggers a native SaveRoutine that the guarded save-mutation boundary
  blocks — needs its own authorized scenario; NOT RUN).
- Both-sexes matrix per race; representative head/skin/hair/preset browsing;
  cancellation/reopening repetition; repeated recruitment beyond two creators.
- Persistence: committed disposable elemental character across save, full
  exit, fresh launch, reload.
- Module-OFF with legacy elemental data on the final artifact; KMG-only
  focused comparison; native Human/Aasimar module-ON controls.
- Batched cells above were partially covered on older builds (0.0.117–0.0.120
  qualifications) — not claimed here as final-artifact evidence.

Historical evidence for the diagnosis chain (runs 04–13, logs and JSON
evidence preserved under
`C:/Dev/KingmakerGunslingerLab/runtime-evidence/character-visibility-repair/`).

## 6. Delivery identity and installation

- Final source: branch `codex/z-character-visibility-repair`; runtime
  qualification ran on `dc3367b5`-equivalent production source (subsequent
  commits are documentation only).
- Final validated package:
  `artifacts/local-runtime/0.0.122/KingmakerGunslinger-0.0.122-local-runtime.zip`,
  SHA-256 `839350AB046A08C2DFC95C694212F9BE9D4E285D5C30C430D51A7C9054552F5A`;
  DLL SHA-256 `A872F2517E8819E422DFA45E2C81F825B144B8038E709A68BC849F595B56E73B`
  (1,567 tests 0 failures; clean Release; strict package validation).
- Installation: the live `Mods/KingmakerGunslinger` DLL hash is verified EQUAL
  to that package DLL (`A872F251…`) — the exact final candidate is installed
  for Howie's playtest. The pre-mission original (0.0.122 teleportation build,
  DLL SHA-256
  `6965AAB9326BBEEF6DFF845B46DD6912C8081BBC87E6EEF88F875A0368E9CEC1`) is
  preserved in `runtime-backups/live-mod/`; rollback:
  `scripts/Restore-Live-Mod.ps1 -BackupDirectory <backup>`.
- No merge, no tag, no release, no PR opened.

## 7. Human checklist for Howie's playtest (acceptance pending)

1. New game: create any elemental-race character (both sexes if convenient) —
   the preview body/head should be visible through customization; finish
   creation and check the world model.
2. Create a Human/Aasimar character — body visible, creation unaffected.
3. Recruit a mercenary at the capital — body visible in creator and world.
4. Travel between areas, re-enter, save/quit/reload — bodies stay visible.
5. Expect zero "[KMG] … visual-resource" ERROR spam in the log; warnings
   about recovery, if any, should appear at most rarely.
