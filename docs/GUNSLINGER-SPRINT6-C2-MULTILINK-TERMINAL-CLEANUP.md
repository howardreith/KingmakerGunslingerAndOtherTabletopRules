# Sprint 6 checkpoint C2 (VER-02): Giant Flytrap multi-link terminal cleanup

Status: **C2 qualified natively on the unchanged baseline, with no gameplay change; returned for manual review**
(2026-11-22). **VER-02 is not closed.** Nothing is merged or released.

- Branch `codex/s6-ver02-c2-multilink-cleanup` is stacked on C1's acceptance-records head `350062e1ea`
  (`codex/s6-ver02-grapple-terminal-cleanup`, draft PR #204), which is stacked on #203, #200 and #198. The stack is not
  rebased; master is at v0.0.194 (`06abcc775e`).
- C1 is accepted for the tested crocodile/common single-link subset, with its dead-holder residue as an accepted
  limitation (C record, section 8). A1, A2 and B stay accepted. RISK-03 keeps its retained limitations. REP-05 and
  REP-06 stay BLOCKED on their original inputs.
- Product source, DLL and package are B's accepted candidate. Only harness, session and records change.
- ACTIVE_SUMMON_GRAPPLES_RESET_SAFELY_ON_RELOAD is preserved. Nothing persists or rebuilds an active hold.

## 1. G2 owner and limb map

Source: `Summoning/ExpandedSummoningSpecialCombatComponents.cs` and `Summoning/SummonGrappleLinkState.cs`.

**Configuration.** The published Giant Flytrap (Huge) is configured by
`ExpandedSummoningSpecialBuilder.ConfigureGrabber(GiantFlytrap…)`:

- `GrabWithPrimaryHand` plus 3 additional limbs, giving four bites (all `BiteLarge1d8`);
- `MaxHeldTargets = 4`, which makes `MultiLink` true;
- hold buff `KMG_Summoning_Special_Grapple_MultiHold` (`SummonMultiHoldComponent`);
- held state `KMG_Summoning_Special_Grapple_MultiHeld` (`AddCondition` Entangled, `AddCondition` CantMove,
  `SummonHeldComponent`);
- engulf through `SwallowedBuff`, Medium or smaller (absolute).

**Establishment.** `TryGrab`, multi-link branch:

- After the grapple maneuver succeeds, it adds the hold buff to the holder once.
- It adds one held-state buff per target, whose `MechanicsContext` caster is the holder.
- It records the link with `SummonGrappleLinks.Record(holder, target, limb)`.
- No native `UnitPartGrappleInitiator`/`UnitPartGrappleTarget` is created, so the native `UnitGrappleController` never
  touches this owner.

**Limb occupancy.**

- `OccupantOf`/`IsLimbOccupied` read `Reconcile`, which keeps a record only while
  `SummonHeldComponent.HolderOf(target) == holder`.
- The attack-roll gate (`OnEventAboutToTrigger(RuleAttackRoll)`) auto-misses a busy limb against anyone else.
- `HeldCount` counts the held-state buffs plus engulfed units.

**Release paths.**

- `SummonMultiHoldComponent.OnNewRound` releases a link whose target is not conscious or out of primary reach;
  otherwise it runs `MaintainLink`. It removes the hold buff when no link remains.
- `SummonMultiHoldComponent.OnTurnOff` releases every link (`ReleaseLink`), provided `SummonGrabComponent.Find(owner)`
  still answers.
- `SummonHeldComponent.OnNewRound`, on the target, removes its own held state when the holder is gone, unconscious or
  without the hold buff. On a successful break-free it calls `ReleaseLink`.
- `SummonHoldComponent.ReleaseLink`, multi-link branch: drops the record, removes the exact held state, and removes the
  hold buff with the last link.
- Load: `SummonGrappleAreaSafeguard.ResetLoadedGrapples` (`exactMulti`) → `ReleaseLink` for each held unit, then removes
  the hold buff and calls `ReleaseAll`.
- The held-state buff has **no** `OnTurnOff`. A held state removed by any other route (death's buff purge, a dispel)
  drops no record.

**Shared with other groups, but not qualified for them here:** `ReleaseLink`, the record store, `MaintainLink`
(the swallow rider is G3) and the area safeguard (G5).

## 2. Representative, targets and setup

- **Holder:** the published Giant Flytrap, summoned through the product's own execution ability
  (`CastExpandedSummoningVariant`, the engine's `UnitUseAbility`). It is the only shipped multi-link holder.
- **Targets:** Expanded Summoning **horses** (Large), summoned the same way. Eligibility was read from the product's
  own rules at every establishment:
  - `IsGrabSizeAllowed` was true (Large is not larger than Huge);
  - `IsSwallowSizeAllowed` was false (the engulf limit is Medium).

  So a successful maintain never takes the swallow rider, and G3 stays out of the C2 cases.
- **Holds** were established by the engine's `RuleAttackWithWeapon` for distinct grab limbs: link 0 used the primary
  bite (limb `1|-1`), link 1 the first additional bite (limb `2|0`). No record or held state was inserted directly.
- **Disclosed setup:**
  - holder BAB raised to 100, so attacks, grapple maneuvers and maintain checks succeed and break-free checks fail;
  - horses' HP raised to 100000, because maintain damage accrues each round;
  - `SummonedUnitAppearBuff` removed;
  - brains inactive;
  - units woken and registered awake;
  - placement on the navmesh by `TraceAlongNavmesh` and the runtime placer: holder about 6 m from the main character,
    horses 3 m from the holder at ±45°.
- **Baselines** (asserted before the first bite):
  - the holder has neither CantAct nor CantMove;
  - every target has no CantMove, CantAct or Entangled;
  - the grab size is allowed and the engulf size is not.
- **Native callback witness.** A read-only Harmony postfix counts calls to `SummonMultiHoldComponent.OnNewRound`,
  `SummonHeldComponent.OnNewRound`, `SummonMultiHoldComponent.OnTurnOff` and `BuffCollection.Tick`. Waits are bounded
  by **game time** (`TimeController.GameTime`), not by wall time.
- **Reload save.** The probe itself saves (`SaveManager.CreateNewSave` + `Game.SaveGame`) while paused, immediately
  after capturing the state. The game clock was unchanged across the save.

## 3. Results (final session `20261122T205434Z`, PASS 2 of 2)

Every row was captured before any teardown or direct-release control. "Asserted" items fail the run; "captured" items
are recorded only.

**Independent concurrent pair** (a second flytrap holding one horse) through every case.

- Asserted: exactly one held state naming its holder (`MaybeCaster` and saved caster id), one raw record on its limb,
  the limb occupied, Entangled and CantMove, no native parts, one hold buff.
- Captured: its damage rose round by round (maintain), and its round callbacks were counted.

**1. One link ends while its sibling stays** (`GameHelper.KillUnit(first target)`).

- Asserted:
  - at least one holder `OnNewRound` was witnessed after the death (1);
  - link 0: no held state, no live answer, limb `1|-1` free;
  - link 1, on the same holder: exactly one held state with holder context, one raw record on limb `2|0`, the limb
    occupied, Entangled and CantMove;
  - one hold buff, and the holder has no CantAct/CantMove;
  - the concurrent pair is intact;
  - the unrelated Bull's Strength on the sibling is kept.
- Captured: **the raw record for the dead target was retained on the surviving holder** (section 4).
- Real behavior control: a `UnitMoveTo` issued to the still-held sibling moved it **0.0 m**.

**1a. Consequence check** (a third horse bitten by the freed limb).

- Asserted: the new horse is held on limb `1|-1` with its own record, and the sibling is still held.
- Captured: one bite was enough; the stale record for the dead target **was replaced** (`Put` drops a record whose
  limb takes a new victim); `HeldCount` was 2.

**1b. Second terminal on the same holder** (its death, with two remaining links).

- Asserted: both remaining links ended, and both surviving targets are unconstrained: Entangled, CantMove and CantAct
  read directly and absent; native move/act flags true.
- Captured:
  - The release came at the targets' own round callback (`SummonHeldComponent.OnNewRound`, witnessed 1 each), within
    about one round of game time after the death, not at the death itself.
  - The dead holder **kept both raw records** (as in C1).

**1c. Repeated teardown, control only** (`ReleaseLink` and `SummonGrappleLinks.Release` ×3 on the ended links).

- Asserted: no exception, and the concurrent pair intact.
- Captured: the dead holder's records were dropped by the control. Nothing else changed.

**2. Holder death with two links.**

- Asserted: both links ended; both targets unconstrained (conditions read directly); concurrent pair intact.
- Real behavior:
  - A `UnitMoveTo` issued to a released horse moved it **2.15 m** (asserted ≥ 0.5 m).
  - The same command to the concurrent pair's still-held horse moved it **0.0 m** (asserted ≤ 0.25 m).
- Captured: release at the targets' round callback (about 6 s of game time after the death); the dead holder kept
  both raw records.

**3. Accelerated summon expiry** (`SummonedUnitBuff` deadline shortened, then explicit native
`BuffCollection.UpdateNextEvent`/`Tick`).

- Asserted: both links ended, both targets unconstrained, the holder destroyed, the concurrent pair intact.
- Captured: raw records empty.

**4. Save during an active two-link hold, then a fresh-process load.**

- At capture (asserted): both links held exactly as in case 1.
- Right after the save, in the **live session** (captured):
  - both targets free: no held state, Entangled or CantMove;
  - raw records empty;
  - the holder still had its hold buff;
  - one `SummonMultiHoldComponent.OnTurnOff` was witnessed during the save.
- One round later (captured): the hold buff had removed itself.
- Fresh-process load (asserted):
  - At load: no hold buff, raw records, live links, occupied limb, held state or condition on any target, and the
    holder has no CantAct/CantMove.
  - After **40 s of game time**: the same. The engine ticked every unit's buff collection (1281 ticks each), no
    hold/held round callback ran, and the targets took **no damage** (delta 0, 0).
- No delayed re-establishment or owned damage followed the reset.

**Not exercised:**

- a real break-free success;
- maintain failure;
- out-of-reach release;
- turn-based mode;
- a dispel of the held state;
- a holder holding three or four links;
- same-target competing holders (the shipped `TryGrab` refuses an already held target through `targetHeld`; not driven
  here);
- the engulf path (G3).

## 4. Findings

**F1: a save ends active holds in the live session (demonstrated; engine boundary plus product owner; not repaired).**

- Mechanism:
  - `SaveManager.SaveRoutine` (decompiled 2.1.7b) calls `LoadedAreaState.TurnOff()` and `CrossSceneState.TurnOff()`
    before serializing, and `TurnOn()` afterwards.
  - Turning off runs every `BuffLogic.OnTurnOff`. `SummonMultiHoldComponent.OnTurnOff` treats that as the end of the
    hold and releases every link; `SummonHoldComponent.OnTurnOff` (G1) releases its single link.
- Observed in C2: during the save one multi-hold `OnTurnOff` ran, both live targets were freed, and the hold buff came
  back on with no links and removed itself a round later.
- This also explains the C1 save inspection (C record, section 8): the target side and record were gone, and the
  holder's native initiator part stayed.
- Consequences:
  - A real save can never contain a full active hold, so the load reset only ever meets holder-side residue.
  - A player who saves while a summon holds a target loses that hold.
  - The engine refuses saves in combat (`SaveManager.IsSaveAllowed`), so this needs an out-of-combat hold; autosaves on
    area exit happen at the leave boundary as well.
- Applicability to released versions is inferred from identical source; it was not run on released bytes.
- **Not repaired.** Owner decision required: should a save preserve a live hold? That would mean distinguishing the
  save's temporary turn-off from a real end, in the shared G1/G2 `OnTurnOff` owners. It touches the accepted
  session-scoped design, and it is a release, not a leak.

**F2: a surviving multi-link holder keeps the raw record of an ended link whose held state was removed by another
route (observed; no demonstrated consequence; not repaired).**

- Mechanism: when the target died, the engine's death purge (`BuffCollection.Tick` on a dead owner removes non-
  `StayOnDeath` buffs) removed its held state, and the held state has no `OnTurnOff`. `OnNewRound` then never saw the
  link.
- Consequence check:
  - live answers and limb occupancy were correct (limb free);
  - the freed limb took a new target through a real bite;
  - the stale record was replaced by `Put`;
  - `HeldCount` was correct.
- No player-visible consequence was demonstrated. Under "repair only demonstrated leaks" this stays an observation.
  Reopen if a lifecycle shows a stale-record consequence, such as a revived target re-held through another limb.

**F3: dead multi-link holders keep their raw records (observed).** This is the C1 dead-holder pattern; the
direct-release control dropped the records before teardown. Inferred, not traced: the multi-hold `OnTurnOff` at death
found no grab component because the death purge had already removed the combat-traits buff, so the targets were
released by their own `SummonHeldComponent.OnNewRound`.

**F4: holder death releases multi-link targets at the targets' next round callback**, not at the death (observed:
about 6 s of game time). The published rule text says each held foe gets a break-free attempt each round, and the
held-state rule ends the link "when the holder is gone or unconscious" on its round. Recorded as observed timing, not
judged a defect.

## 5. Retained-save inspection (read-only, original artifact)

- The artifact: `Manual_1_ver02c2.zks` from session `20261122T205434Z` (profile `Saved Games`), SHA-256
  `ac463b384be5e255b4260cd1fc3a8a8368532a632b5d55abb97070b90f586ea4`. It was read after the session, with no game
  running.
- Holder `KMG_C2_ReloadHolder` = `7f04540b-c32e-4109-8b75-935d6ffb70e3` (in `party.json`): the multi-hold buff
  (`a6ff9b2a…`) is present; `UnitPartSummonGrappleLinks._links` is `[]`; no condition counters are set.
- Targets `KMG_C2_ReloadTarget0` = `51fa10d0-a5a8-4069-b7ce-26935d63bf27` and `KMG_C2_ReloadTarget1` =
  `ef080e11-a235-4ac6-a077-10e146169da6`: no held-state buff, no `EntangledBuff`, no grapple parts, no conditions.
- **What entered the save:** the holder's multi-hold buff only, consistent with F1. Launch 2's reset removed that buff.
  The load reset of a full active multi-link hold is therefore inapplicable through a real save.

## 6. G5 fixture intake and transition probe

- **Fixture intake (one bounded pass, read-only headers).** The loader-compat runner admits saves from
  `C:\Dev\KingmakerSkipCinematicLab\fixtures`. All five are in the same area, `JamandisMansionThroneroom`
  (`0a89ba82…`), whose only exit was shown to be scripted in C1:

  | Fixture | SHA-256 |
  |---|---|
  | `KMCS_AFTER_NORMAL_012_MODONLY.zks` | `0be64b3f…a249` |
  | `KMCS_AFTER_SKIPS.zks` | `d200bb84…90fc` |
  | `KMCS_AFTER_SKIPS_012_MODONLY.zks` | `5576f75e…7479` |
  | `KMCS_AFTER_SKIPS_012_OWNERMODS.zks` | `92384efd…6a33` |
  | `KMCS_CMI_PERSIST.zks` | `811d91cf…bd5b60` |

  **G5 stays blocked.** Requirement: an admitted, disposable, product-only-loadable save in an area with an
  **unscripted, unrestricted** exit (`AreaTransition` with no passing `BlueprintAreaTransition` action) to another
  local area, ideally without autosave dialogs.
- **Transition probe corrected** (compiled into harness `3b9d4498…`, **not executed**):
  - The leave phase refuses unless the companion is party-held before the exit.
  - It carries origin, destination, enter point, holder id and target id into the arrival phase.
  - It captures, separately and synchronously right after `ExecuteTransition`: target constraints, holder parts and
    buffs, and raw records (`leaveBoundaryHolderResidue`). This happens before any arrival `ResetLoadedGrapples`.
  - The arrival phase fails explicitly if the loading deadline expires, if loading is unfinished, if no area is loaded,
    or if the area is not the intended destination. Only then does it judge the arrival state.
  - It records the holder's disposition (loaded in the destination, or not).
  - Hypothesis to investigate, from source only: `Sweep(true)` removes the parts without dropping the record, and it
    leaves the holder in the old area. F1 adds that the autosave on exit (`AutoSaveMode.BeforeExit`) turns facts off
    first.

## 7. Native evidence, commands and identities

Product (unchanged, B's accepted candidate):

- commit `4e11e6c25c`;
- package `KingmakerGunslinger-0.0.193-development-risk02-powerful-change-restoration-4e11e6c25c81.zip`, SHA-256
  `2608e9f593494dfbdd801e39f20c599b7e88e658c0dd875fb3ab9a46c431374b`;
- DLL `d813f62c059946f2d8355c1ff134bc154cf41a0f21a55547734d70a319b02cc4`.

Environment and fixture:

- Loader profile `owner-0.33.0`; product alone.
- Fixture `KMCS_AFTER_NORMAL_012_MODONLY.zks` (`0be64b3f…a249`), RTWP.
- The Kingmaker window was granted by the Mounted Combat session and released afterwards.

Harness: `tools/LoaderCompatHarness/MultiLinkProbes.cs` (step `kmgMultiLink`: `multilink`, `multilinkReload`), plus the
`GrappleProbes.cs` transition corrections. Session:
`scripts/compatibility/loader-compat/sessions/ver02-c2-multilink-terminals.json`. Build and run:

```
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/compatibility/loader-compat/Build-LoaderCompatHarness.ps1 -SourceDir tools/LoaderCompatHarness -OutputDir artifacts/loader-compat-harness-ver02-c2 -LoaderReferenceDir <repo>/artifacts/references/loader
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/compatibility/loader-compat/Invoke-LoaderCompatSession.ps1 -SessionFile <repo>/scripts/compatibility/loader-compat/sessions/ver02-c2-multilink-terminals.json -LoaderProfile owner-0.33.0 -CandidateZip <package above> -HarnessDir <repo>/artifacts/loader-compat-harness-ver02-c2
```

| Session | Harness DLL | Result | Wall time | Note |
|---|---|---|---|---|
| `20261122T204736Z` | `8efdc153…a4a0` | Launch 1 PASS (117 s); launch 2 **FAIL** (113 s) | 245 s | **Observer defect, retained.** "A limb reports an occupant": Newtonsoft stores a C# null string as a String-typed JValue, so the null check read it as present. The captured occupancy was empty. This run first showed F1 |
| `20261122T205434Z` | `3b9d4498…bc41` | **PASS 2 of 2** (145 s, 114 s) | 274 s | Null check corrected; the save-time `OnTurnOff` witness and post-save capture added |

- Every transaction completed: game settings applied, owner files verified, and the installation restored by the
  runner.
- Evidence lives under `C:\Dev\KingmakerGunslingerLab\runtime-evidence\loader-compat\` and is not committed.
- No product compilation or domain tests: no product source changed. The harness compiled without errors.

## 8. VER-02 disposition

**VER-02: open.**

- C1 is accepted for the tested crocodile/common single-link subset.
- C2 qualifies the G2 multi-link terminals listed in section 3, for the Giant Flytrap with Large targets, with no
  gameplay change.

Remaining, separately identified:

1. **G5 leaving hook:** blocked on a fixture with an unscripted exit. The probe is corrected and ready.
2. **G3 swallow and engulf**, including the Flytrap's engulf promotion, which C2 deliberately excluded.
3. **G4 stirge attach.**
4. **G1 variant owners:** Impale's scheduled cleanup, the live constrict and rider profiles (including the crocodile's
   death roll), and swallow promotion from a single-link hold.
5. **G2 cases not exercised** (section 3): break-free success, maintain failure, out-of-reach release, turn-based
   mode, a dispelled held state, three or four links.

**Owner decisions requested:** F1 (a save ends live holds) and, if wanted, F2/F3 (eager dropping of ended records).
