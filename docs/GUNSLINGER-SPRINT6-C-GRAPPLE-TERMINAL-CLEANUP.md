# Sprint 6 checkpoint C (VER-02): real grapple terminal cleanup, subcheckpoint C1

Status: **C1 accepted by manual review (2026-11-22) for its demonstrated development scope**, at head `01c0e2b476`
(base `024db08b98`; product `4e11e6c25c` unchanged, package `2608e9f5…374b`, DLL `d813f62c…2cc4`; final native session
`20261122T200515Z-owner-0.33.0-ver02-c1-grapple-terminals`; harness `33530ca8…f4bd72`). **VER-02 is not closed.**

- Acceptance covers the executed checks on the **tested crocodile and common single-link terminal subset**, and the
  reload observations, with the limitations in section 8. It does not qualify every G1 variant.
- The dead-holder residue (section 5) is an explicit, accepted C1 limitation. No eager corpse-cleanup hook is
  authorized.
- Section 8 records the review's corrections and the later inspection of the retained save. Where it corrects an
  earlier section, section 8 governs.
- Nothing is merged or released.

- Branch `codex/s6-ver02-grapple-terminal-cleanup` is stacked on B's acceptance-records head `024db08b98`
  (`codex/s6-risk02-powerful-change-restoration`, draft PR #203). The stack is not rebased; master is at v0.0.194
  (`06abcc775e`).
- A1 (#198), A2 (#200) and B (#203) stay accepted, as recorded. RISK-03 keeps its retained limitations (A2 record,
  section 10). REP-05 and REP-06 stay BLOCKED on their original inputs.
- The change is harness, session and records only. Product source, DLL and package are B's accepted candidate.
- ACTIVE_SUMMON_GRAPPLES_RESET_SAFELY_ON_RELOAD is preserved. Nothing persists an active grapple.

## 1. Owner map

The product's grapple, hold, swallow, engulf and attach state falls into these lifecycle groups. Sources:
`Summoning/SummonGrappleLinkState.cs` and `Summoning/ExpandedSummoningSpecialCombatComponents.cs`, plus the
decompiled 2.1.7b `UnitGrappleController`, `UnitPartGrappleInitiator`, `UnitPartGrappleTarget`, `BaseUnitController`,
`UnitPartsManager`, `SummonedUnitBuff`, `SummonedUnitsController` and `AreaTransitionGroupCommand`.

**Shared record store (all groups that grab).** `UnitPartSummonGrappleLinks` holds one record per target and per limb
on the holder (`TargetId|Limb|AdditionalIndex|Weapon|Engulfed|Repaired`).

- Write paths: `SummonGrappleLinks.Record`, `MarkEngulfed`, `Release` (drop one target) and `ReleaseAll`.
- `Put` also replaces a record when its target or its limb takes a new hold.
- Every read goes through `Reconcile`, which **filters** records against live native state without deleting them. A
  stale raw record therefore answers nothing but can stay in the list. This is why C inspects raw records separately.

**G1, single-link native grapple** (every `SummonGrabComponent` with `MaxHeldTargets = 1`). C1 (accepted) covers only
the tested crocodile/common single-link terminal subset; it does not qualify every G1 variant (section 8).

- Established by `SummonGrabComponent.OnEventDidTrigger(RuleAttackWithWeapon)` → `TryGrab`.
- Owners:
  - The native `UnitPartGrappleInitiator` on the holder owns the hold buff (`SummonHoldComponent`) and the holder's
    CantAct/CantMove.
  - The native `UnitPartGrappleTarget` on the target owns the grappled buff (Entangled, CantMove).
  - The holder's record.
- Native release (decompiled): `UnitGrappleController.TickOnUnit` runs only for awake, non-dead units.
  - On the target, it removes the target part when the holder is missing, not conscious or no longer names it.
  - On the holder, it removes the initiator part when the target is missing, not conscious, unheld or out of reach.
  - Part `OnRemove` runs while the part is still attached (`UnitPartsManager.Remove`).
- Product release: `SummonHoldComponent.OnTurnOff`, when the hold buff ends, calls `Release(owner, HeldTarget(owner))`,
  which drops the record and the target part, provided the reciprocal native parts still name each other.
- Variants that add owners on top of the shared path (not separately exercised in C1; see section 7): the Barbed
  Devil's Impale scheduled cleanup (`ScheduleReleasedImpaleOwnerCleanup`), the live constrict and rider profiles
  (constrictor snake, salamander, Lillend, crocodilian death roll), and the swallow promotion from a hold.

**G2, multi-link hold** (`MaxHeldTargets > 1`; four published consumers: Giant Flytrap, Xill, Giant Scorpion and
Giant Crab, the last with live constrict; see the C2 record, section 9). No native grapple parts.

- Owners: one hold buff on the holder (`SummonMultiHoldComponent`) and one held-state buff per target, whose context
  caster is the holder (`SummonHeldComponent`).
- Release: `SummonMultiHoldComponent.OnNewRound`/`OnTurnOff` releases every link. `SummonHeldComponent.OnNewRound`
  removes its own buff when the holder is gone or unconscious.
- The native controller plays no part, so this is a distinct release path.

**G3, swallow and engulf** (Purple Worm, Flytrap engulf and the other swallowers).

- Owners: the native `UnitPartSwallowWhole`/`UnitPartSwallowed`, the swallowed buff, and `Engulfed` records.
- Release: `SummonSwallowLifecycleComponent.OnTurnOff` (spit out). `HandleUnitDeath` frees the victim only for
  payloads with `ReleaseOnVictimDeath`.

**G4, stirge attach.** `StirgeAttachComponent`/`StirgeHoldComponent` keep a session `m_Target` and a remove-action fact
on the prey. Release: `StirgeHoldComponent.OnTurnOff` → `ReleaseRemoveAction`, and `SummonGrappleAreaSafeguard.SweepStirge`.

**G5, area safeguard** (`SummonGrappleAreaSafeguard`). Shared by G1–G4.

- On leaving: `HandlePartyLeaveArea` → `Sweep(true)` releases **party members** held, engulfed or swallowed by KMG
  summons, and stirge attachments. The engine raises this only from `AreaTransitionGroupCommand.ExecuteTransition`
  (the party using an exit), before `LoadArea`.
- On loading: `OnAreaLoadingComplete` → `ResetLoadedGrapples` (every KMG summon's swallow, native and multi-link
  holds, hold buff and records) plus `Sweep(false)`. This is the owner of ACTIVE_SUMMON_GRAPPLES_RESET_SAFELY_ON_RELOAD.

**Not grapple cleanup.** `RuntimeTestRunner.ExpandedSummoningHold32.cs` is a Lillend Hold Person scenario. The existing
`ExpandedSummoningSprint16Lifecycle` drill establishes holds through a direct `TryGrab`, uses a direct `Sweep` for
"transition", and judges release through `Reconcile`-filtered reads. It is not counted here.

## 2. C1 representative and rationale

- **Representative:** the Expanded Summoning **crocodile** (own-tier variant, one bite: `BiteDragonLarge1d8`).
- Why this creature (corrected in section 8):
  - It runs the shared G1 path (`TryGrab` single-link branch, native initiator and target parts,
    `SummonHoldComponent`, a record), with no swallow promotion, Impale, live constrict or multi-link.
  - It is **not** rider-free: it is configured with `DeathRollCreatureKey = "crocodile"`, a death-roll rider. C1 did
    not establish any particular rider occurrence, and it did not qualify rider-state cleanup.
- Summoned through the product's own execution ability, by the engine's `UnitUseAbility` (`CastExpandedSummoningVariant`).
- Holds are established by the engine's `RuleAttackWithWeapon` for the bite, which runs the product's grab handler and
  grapple maneuver as a real attack does.
- **Disclosed setup:**
  - Holder BAB raised to 100, so the attack and maneuver succeed.
  - The summon's `SummonedUnitAppearBuff` is removed, as the existing fixtures do.
  - Brains are inactive, and units are woken and registered awake so the native controllers tick them.
  - Targets are disposable `DefaultPlayerCharacter` spawns, and `EntityCreator.Tick()` puts them into `State.Units`.
  - Natural expiry moves `SummonedUnitBuff`'s deadline to now and lets the engine's `BuffCollection` scheduler remove it.
- **Baselines** (asserted before every bite): the holder has neither CantAct nor CantMove, the target has neither
  CantMove nor CantAct, and the target is in `State.Units`.

## 3. Terminal results (final session `20261122T200515Z`, PASS)

"Raw" is the holder's stored record list, read directly. "Live" covers native parts, buffs, conditions and
`Reconcile`. Every row was captured before harness teardown.

| Terminal (real engine operation) | Target after | Holder after | Raw records | Disposition |
|---|---|---|---|---|
| Holder death: `GameHelper.KillUnit(holder)` | Target part, grappled buff and CantMove gone; native move/act eligibility flags true; unrelated Bull's Strength kept (Entangled was not read directly) | Dead. Hold buff gone (death removed its buffs: `SummonedUnitBuff`, `RecentlyDeadBuff` remain). The native initiator part and its CantAct/CantMove stay on the corpse (dead units are not ticked) | **Record naming the target retained** on the dead holder; `Reconcile` = 0 | Target released by the native controller. Retained raw record observed (section 5) |
| Target death: `GameHelper.KillUnit(target)` | Dead; part and buff gone | Initiator part, hold buff, CantAct and CantMove all gone | Empty | Fully released (native controller → initiator `OnRemove` → hold buff `OnTurnOff` → `Release`) |
| Second terminal on the ended pair: the surviving holder's native expiry | — | Destroyed | Empty | No effect on other pairs; no exception |
| Accelerated summon expiry (shortened deadline, then explicit native `BuffCollection.UpdateNextEvent`/`Tick`) | Part, buff and conditions gone; native move/act eligibility flags true | Destroyed | Empty (captured, not asserted) | Target released |
| Summon dismissal: native **Dismissal** | — | — | — | **Inapplicable, demonstrated:** `CanTarget` false. Dismissal's `AbilityTargetHasFact` requires the native `SubtypeExtraplanar` (`136fa034…`). The crocodile carries only `KMG_Summoning_Subtype_Extraplanar`, consistent with Sprint 30–31's choice to give the native identity only to configured outsiders. Not counted as PASS |
| Repeated teardown, control only: `SummonHoldComponent.Release` + `SummonGrappleLinks.Release` ×3 on the ended pair | Unchanged | Unchanged | The retained record is dropped | Idempotent; no exception |
| Independent concurrent holding pair (a different holder and target) through every terminal above | Held throughout: part, grappled buff, one raw record, `Reconcile` = 1 | Holding | 1 | Intact |
| Unrelated effect: Bull's Strength on the holder-death target | Kept | — | — | Intact |
| Fresh-process load (launch 2) of the save written after the probe | At load and after 8 s: free; no part, buff or conditions | No initiator part, hold buff or conditions | Empty | Corrected in section 8: the save held only holder-side residue, not a full active hold |
| Actual scene transition: party exit via `AreaTransitionGroupCommand.ExecuteTransition` | — | — | — | **Unexecuted.** The fixture area's only exit (`JamandisMansionThroneroom_exitTransition` → `JamandisMansion`) is scripted: its blueprint action runs in place of the transition. The probe refused it rather than drive unexpected UI (`20261122T200003Z` launch 3). Needs a fixture with an unscripted exit (section 7) |

**Not exercised:** a summoner-loss `ForceKill` (`SummonedUnitsController`; it reaches the same death path as holder
death, which is an inference), a real break-free escape, turn-based mode, and two holders competing for the same
target.

## 4. Leak and repair

No gameplay leak was demonstrated for G1, so there is no product change.

- Every surviving target's grapple part and grappled buff were gone, and its native move/act eligibility flags were
  true.
- The one surviving holder (target death) dropped its part, buff, conditions and raw record.
- The reload reset cleared what entered the save, which section 8 shows was holder-side residue only.

## 5. Raw-versus-live observation (retained, not repaired; for review)

- **What remains after holder death:** the dead holder keeps
  - its raw record naming the freed target;
  - the native initiator part with CantAct/CantMove.

  It was not observed persisting until ordinary destruction: the direct-release control that followed dropped the raw
  record before teardown (section 8).
  - Observed: dead units are not ticked, the hold buff was gone, and the record was not dropped.
  - Inferred, not traced frame by frame: the hold buff turned off after the target's own tick had already removed the
    reciprocal target part, so `HeldTarget` was null in `OnTurnOff`.
- **Why it is inert:**
  - `Reconcile` returns 0.
  - The target is free.
  - `Put` replaces the record if that holder ever grabs again.
  - If such a holder were revived, the native controller would remove its orphaned initiator part (inferred from
    `TickOnUnit`, not exercised).
- **Classification:** stale data on a dead holder with no demonstrated player-visible effect. Under "repair only
  demonstrated leaks" it is not changed.
- **Open question for review:** should ended records on a dead holder be dropped eagerly, for example from a death
  handler? That would add a hook, not fix an observed defect.

## 6. Native evidence, commands and identities

Product (unchanged, B's accepted candidate):

- commit `4e11e6c25c`;
- package `KingmakerGunslinger-0.0.193-development-risk02-powerful-change-restoration-4e11e6c25c81.zip`, SHA-256
  `2608e9f593494dfbdd801e39f20c599b7e88e658c0dd875fb3ab9a46c431374b`;
- DLL `d813f62c059946f2d8355c1ff134bc154cf41a0f21a55547734d70a319b02cc4`.

The grapple sources are identical in this candidate, this branch and master (`git diff` empty for
`ExpandedSummoningSpecialCombatComponents.cs`, `SummonGrappleLinkState.cs` and the runtime-test helpers used).

Environment and fixture:

- Loader profile `owner-0.33.0` (UMM 0.33.0, SHA-256 `63e5baf7…2691`); product alone.
- Disposable fixture `KMCS_AFTER_NORMAL_012_MODONLY.zks` (area `JamandisMansionThroneroom`, RTWP).
- The Kingmaker window was granted by the Mounted Combat session and released afterwards.

Harness: `tools/LoaderCompatHarness/GrappleProbes.cs`, step `kmgGrapple` (`terminals`, `reload`, `transition`,
`afterTransition`). Sessions:

- `scripts/compatibility/loader-compat/sessions/ver02-c1-grapple-terminals.json`;
- `scripts/compatibility/loader-compat/sessions/ver02-c1-grapple-transition.json`.

Build and run commands:

```
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/compatibility/loader-compat/Build-LoaderCompatHarness.ps1 -SourceDir tools/LoaderCompatHarness -OutputDir artifacts/loader-compat-harness-ver02-c1 -LoaderReferenceDir <repo>/artifacts/references/loader
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/compatibility/loader-compat/Invoke-LoaderCompatSession.ps1 -SessionFile <repo>/scripts/compatibility/loader-compat/sessions/ver02-c1-grapple-terminals.json -LoaderProfile owner-0.33.0 -CandidateZip <package above> -HarnessDir <repo>/artifacts/loader-compat-harness-ver02-c1
```

| Session | Harness DLL | Result | Wall time | Note |
|---|---|---|---|---|
| `20261122T194948Z` | first build (not hashed; superseded) | **FAIL** launch 1 (90 s); launch 2 not run | 110 s | **Fixture defects, retained.** (1) Targets spawned while paused were not yet in `State.Units`, so `Reconcile` read 0 for later pairs. (2) The summon appear buff's CantAct/CantMove was read as the holder's residue after target death. Confirmed by the baselines added next |
| `20261122T195339Z` | `911bdcf5…6eb6ca` | PASS 2 of 2 (89 s, 92 s) | 202 s | Corrected fixture with baselines |
| `20261122T200003Z` | `33530ca8…f4bd72` | Launches 1–2 PASS (98 s, 89 s); launch 3 **refused** (79 s) | 289 s | Scripted exit refused (section 3); no unit was created before the refusal |
| `20261122T200515Z` | `33530ca8…f4bd72` | **PASS 2 of 2** (87 s, 80 s) | 182 s | Final, from the committed two-launch session file |

- Every transaction completed: game settings applied, owner files verified, and the installation restored by the
  runner.
- Evidence lives under `C:\Dev\KingmakerGunslingerLab\runtime-evidence\loader-compat\` and is not committed.
- No product compilation or domain tests: no product source changed (validation policy, point 1). The harness compiled
  with no errors.

## 7. VER-02 disposition and remaining groups

**VER-02: open.** C1 qualifies the shared single-link native grapple path, except for the actual scene transition. Not
yet qualified:

1. **G5 leaving hook.** A real party exit with a party member held (`Sweep(true)`). This needs an admitted fixture
   with an unscripted exit; the `transition` probe mode and session are ready.
2. **G2 multi-link** (Giant Flytrap): holder death, target death, expiry, sibling links on one holder, load.
3. **G3 swallow/engulf:** spit-out on holder death and expiry, victim death with and without `ReleaseOnVictimDeath`,
   engulfed records, load.
4. **G4 stirge attach:** detach and remove-action cleanup at terminals and load.
5. **G1 variant owners:** Impale's scheduled cleanup, the live constrict and rider profiles, and swallow promotion from
   a hold.

Also open for review:

- Section 5's retained raw record on dead holders.
- Native Dismissal cannot reach non-outsider KMG summons (a design consequence recorded by Sprint 30–31, not a VER-02
  leak).

## 8. Review decision and corrections (2026-11-22)

**Decision.** C1 is accepted for its demonstrated development scope: the executed checks on the tested crocodile and
common single-link terminal subset, and the reload observations, with the limitations below. It does not qualify every
G1 variant and does not close VER-02.

**Accepted limitation: dead-holder residue.**

- After holder death, the target was released, but complete removal of all state on the dead holder was not
  established. The dead holder kept its raw record naming the target and its native initiator part.
- The residue was **not** observed persisting until ordinary destruction. The direct-release control that followed
  (`repeatedReleaseControl`) dropped the raw record before teardown.
- Revival and natural disposal timing are inferred or unexecuted.
- No eager corpse-cleanup hook is authorized. Reopen only if a supported lifecycle shows a consequence for a
  surviving or revived owner, a target, or an area unload. The presence of data on a corpse is not such a defect.

**Corrections to sections 2 and 3.**

- **Rider.** The crocodile carries the death-roll rider (`DeathRollCreatureKey = "crocodile"`). No rider occurrence
  was established, and rider-state cleanup was not qualified.
- **Concurrent pair.** The control was an independent concurrent holding pair: a different holder and a different
  target. Same-target competing ownership was not exercised.
- **Expiry.** The deadline was shortened, then native `BuffCollection.UpdateNextEvent`/`Tick` was called explicitly.
- **Movement and action.** These are the native eligibility flags (`UnitState.CanMove`/`CanAct`). No movement or
  action command was executed.
- **Entangled.** Not read directly; the observer read CantMove and CantAct only. Entangled removal is inferred from
  the grappled buff's removal (its buff list no longer showed `EntangledBuff`), not inspected as a condition.
- **Not every terminal clears every record and reference.** The dead holder kept its record and initiator part, as
  above.

**Asserted versus captured.** The probe asserted:

- after a terminal: the target's grapple part and grappled buff, reconciled links, and the target's move/act flags;
- after target death: the surviving holder's part, buff, conditions and raw record naming the target;
- the concurrent pair's hold;
- the reload state.

Captured but not asserted:

- raw records after holder death, after summon expiry and after the second terminal;
- the ended pair's state after the repeated direct-release controls;
- the holder-side state on destroyed holders;
- the buff lists;
- Dismissal's checker facts.

**Dismissal.** Its inapplicability is specific to the native Dismissal spell (`95f7cdce…`) and this crocodile: the
spell's `AbilityTargetHasFact` requires `SubtypeExtraplanar`, which the crocodile does not carry.

**Retained-save inspection (later, read-only, original artifact).**

- The artifact: `Manual_1_ver02c1.zks` from session `20261122T200515Z` (profile `Saved Games`), SHA-256
  `9615eed14da6bd1bc7ef1224ab725b6699cbaa874428d3c94364ed72ced8476f`. It was inspected after the session, with no game
  running, by reading its JSON entries.
- The named pair: holder `KMG_C1_ReloadHolder` = `6fd9d3c3-0974-43ba-896f-b39cbe06f0ff` (in `party.json`, as a
  summon); target `KMG_C1_ReloadTarget` = `3142ad5b-0c1e-48a5-b887-1810d4abf135` (in the area state
  `0a89ba82….json`).
- At the end of the probe (captured in `reloadPairAtSave`) the pair held fully: initiator and target parts, hold and
  grappled buffs, and one raw record.
- **What entered the save:**
  - Target: no `UnitPartGrappleTarget`, no buffs at all (the grappled buff and `EntangledBuff` were absent), and every
    condition counter zero.
  - Holder: `UnitPartGrappleInitiator` still present (`Target` = the target's id, `m_Buff` = the hold buff
    `5af4e99c…`); condition counters CantMove (index 19) and CantAct (index 28) set; `UnitPartSummonGrappleLinks`
    serialized with `_links: []`.
- **Consequence.** Between the probe's capture and the save, the target side and the raw record were released while
  the holder-side native part stayed. What did it (a maintain or rider round, a break-free check, or the teardown's
  removal of awake registrations) was not observed and is not established. So the launch-2 reload reset an
  **incomplete holder-side residue**, not a full active hold. The load reset of a full active hold's target-side
  constraints is **not established** by C1.

**Evidence history.** It is unchanged:

- `194948Z`: FAIL, fixture defects; first harness build, explicitly unhashed.
- `195339Z`: PASS, harness `911bdcf5…6eb6ca`.
- `200003Z`: launches 1–2 PASS, launch 3 refused (scripted exit); harness `33530ca8…f4bd72`.
- `200515Z`: PASS, harness `33530ca8…f4bd72`.

## 9. Later checkpoints

- **C2 (G2 Giant Flytrap multi-link):** `docs/GUNSLINGER-SPRINT6-C2-MULTILINK-TERMINAL-CLEANUP.md`.
- C2's finding F1 gives a source-inferred explanation, consistent with C2 and not retroactively observed in C1, of
  section 8's save inspection. The engine's save routine turns facts off before serializing, so
  `SummonHoldComponent.OnTurnOff` would release the target side and the record, leaving the holder's native initiator
  part.
- C1 acceptance stays limited to the tested crocodile/common single-link subset.
