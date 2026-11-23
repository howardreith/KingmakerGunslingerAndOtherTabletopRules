# Sprint 6 checkpoint C3b (VER-02, G3): Dire Crocodile and Tyrannosaurus swallow lifecycle

Status: **C3b accepted by manual review (2026-11-23) for its demonstrated development scope**, at head `647af2c1ba`
(base `1efff0fa3e`; product `9e902dfa80` unchanged). Section 7 records the decision and the narrowed claims; where it
corrects an earlier section, section 7 governs. VER-02 stays open. Nothing is merged or released.

- Branch `codex/s6-ver02-c3b-crocodile-tyrannosaurus` is stacked on the C3a acceptance-records head `1efff0fa3e`
  (`codex/s6-ver02-c3a-swallow-engulf`, draft PR #207), then #206, #205, #204, #203, #200 and #198. The stack is not
  rebased.
- **Product (unchanged, reused):** accepted C3a `9e902dfa80`. Package `704cb0b4…79c9`, DLL `431f13da…5676`.
- **Fixture:** `KMCS_AFTER_NORMAL_012_MODONLY.zks` (`0be64b3f…a249`); loader profile `owner-0.33.0`; product alone;
  RTWP.
- **Kept:** A1/A2/B, C1, C2, C2a R1 and C3a R1 acceptance; RISK-03's limits; the REP-05/06 blocks; G4; G5
  (fixture-blocked); the G1 variants; the outstanding G2 configurations.

## 1. Two-consumer owner map and prior evidence

Both consumers share the C3a machinery (C3a record, section 1):

- a later-round successful maintain promotes the hold, through `SwallowHeld`, native `Swallow` and an Engulfed record;
- the carrier is `SummonSwallowLifecycleComponent` on each combat-traits buff.

| | Dire Crocodile | Tyrannosaurus |
|---|---|---|
| Grab | G1, primary bite (`KMG_Summoning_Natural_Bite3d6`), with the crocodilian **death-roll rider** | G1, the exact primary bite (`KMG_Summoning_Natural_Tyrannosaurus_Bite4d6`), +4 on the grapple; grab up to its own size |
| Promotion | `CrocodilianRulesPolicy.SelectMaintainRider`: swallow when the swallow size is allowed (at least one size smaller), otherwise death roll | swallow only of a target at least one size smaller, after a new successful later-round check; no immediate swallow |
| Payload | `KMG…DireCrocodile.Swallowed`: the native swallowed state's −2 CMB and −4 Dexterity plus one constructed crush action on `NewRound` | `KMG…Tyrannosaurus.Swallowed`: its own constructed periodic interior damage (2d8+11) on `NewRound` |
| Separate behavior | — | Powerful Bite; **Bleeding Critical** (native `Bleed2d6Buff` on a confirmed critical exact bite) |
| Victim-death release | opted in before C3a (`ReleaseSwallowedOnVictimDeath`) | opted in before C3a |
| C3a change inherited | the carrier's save suspension (`OnTurnOff` keeps on save) and `OnFactDeactivate` release | same |

**Prior evidence** (consulted for unchanged mechanics; it stays attached to its original candidates and is not
presented as a fresh check of `9e902dfa80`):

- **Sprint 16:** the Dire Crocodile's own swallowed state and its numbers
  (`planning/EXPANDED-SUMMONING-SPRINT16-*`, `…SWALLOW-INTERIOR-BLOCKER.md`).
- **Sprint 24:** the Tyrannosaurus' grab/swallow gates, the distinct swallow bite versus periodic interior damage, the
  Bleeding Critical boundary, and the clean-reset decision (`docs/SPRINT24-COVERAGE.md`).
- **The remaining risk** these do not cover is the C3a carrier change: save preservation, payload continuation through
  a save, removal/disposal, victim-death release and escape under the changed component. C3b targets exactly that.
- Presentation, spell roots, combat modes and the full creature mechanics were not repeated.

## 2. Method and disclosed setup

- Each swallower was summoned through the product's execution ability, and held a Medium `DefaultPlayerCharacter`
  spawn through a real eligible bite (`RuleAttackWithWeapon`). The product's own later-round `MaintainLink` promoted it.
- **Asserted:** the maintain trace shows `swallowed:N` for the holder (`selectedSwallow`), so a death-roll hold is never
  counted.
- **Tyrannosaurus bleed-free principal setup** (the enforced condition is bleed absent): if the establishing bite applied the configured Bleeding Critical buff
  (read from the creature's own `TyrannosaurusBleedingCritical` component), that setup was discarded for a fresh one,
  up to three times. Bleed was asserted absent on the principal victim at promotion and at the next payload round.
  - In the first, failed run (`014648Z`) one setup was discarded this way.
  - In the passing runs no discard was needed.
- **Disclosed:**
  - holder BAB 100;
  - victims' HP raised;
  - appear buffs removed, brains off, units woken, navmesh placement;
  - a crocodile pair plus unrelated Bull's Strength effects as independent controls;
  - for the escape only, the swallowed victim's Athletics raised to 300, so that the native break-free check can
    succeed.
- **Harness:**
  - `tools/LoaderCompatHarness/SwallowProbes.cs`, modes `c3bLifecycle`, `c3bSave` and `c3bReload`;
  - the read-only `HoldLifecycleObserver`, with its event cap raised from 600 to 6000 in C3b;
  - the old `Save()` completion guard aligned with `SaveCadence()`.
- **Sessions:** `ver02-c3b-crocodile-tyrannosaurus.json` (lifecycle, save, reload) and `ver02-c3b-lifecycle.json`
  (lifecycle only).

## 3. Results

### 3.1 Promotion, payload and continuing rounds through a save

Session `20261123T015820Z-owner-0.33.0-ver02-c3b-lifecycle`, PASS (125 s), harness `b218b38c…0c4d`.

- **Promotion** (asserted, each consumer):
  - `selectedSwallow` (Dire Crocodile `swallowed:26`, Tyrannosaurus `swallowed:42`);
  - reciprocal native swallow;
  - the creature-owned payload with its holder as caster;
  - untargetable, CantMove and CantAct;
  - one raw Engulfed record, a live link, and the establishing mouth occupied.
- **Native payload state:** `Buff.RoundNumber`, plus `NextTickTime`/`TickTime` by reflection.

| | Paused, before save | After the finished save | Next genuine round |
|---|---|---|---|
| Dire Crocodile payload | round 2, next tick 432550810000, interval 6 s, damage 101 | identical | round **3**, next tick exactly +6 s, same instance, damage **124** |
| Tyrannosaurus payload | round 1, next tick 432550900000, interval 6 s, damage 94 | identical | round **2**, next tick exactly +6 s, same instance, damage **117**; no bleed |

- **Asserted:**
  - the payloads aged through a real round;
  - native save admission allowed;
  - full completion (callback received, no commit and no loading in progress);
  - an unchanged paused clock;
  - an unchanged payload instance, round number, next tick, interval, caster and victim damage;
  - exact raw records and occupancy;
  - both swallows preserved;
  - the unrelated effects kept;
  - then, at the next native round, round +1 on the retained schedule, the same instance and new damage;
  - bleed absent on the Tyrannosaurus victim, so the damage is attributable to the swallowed payload;
  - the crocodile control held (or a traced legitimate maintain release; none occurred).

### 3.2 Real native escape: Dire Crocodile (Tyrannosaurus swallow as the ownership control)

Same session, after the cadence.

- The Dire Crocodile victim's Athletics was raised (0 → 300; disclosed). The native controller and rule made the
  attempt (`UnitSwallowWholeController` → `UnitPartSwallowed.TryToBreakFree` → `TryBreakFree`). Nothing set the free
  flag, called `Free` or removed the payload.
- **Matched evidence.** The gate requires the exact holder's `native.SpitOut(all: false)`, followed by the exact
  victim's `native.Free` by this holder and the exact victim's `UnitPartSwallowed.OnRemove` naming this swallower.
  The full ordered trace below is captured; the gate does not separately enforce `Free` before `OnRemove`, prove call
  nesting, or observe the successful break-free rule result (that upstream mechanism is supported by source):
  1. `native.SpitOut(all: false)` by the Dire Crocodile;
  2. `native.Free` of this victim by this holder (native `SpitOut(false)` calls `Free`; source);
  3. this victim's `UnitPartSwallowed.OnRemove`, naming this swallower.
  - It fails on any other cause for this holder: `SpitOut(all: true)`, the victim-death handler, or a `Free` before the
    spit-out.
- **Asserted outcomes:**
  - that relationship released;
  - the victim's capabilities restored (no CantMove, CantAct, untargetable or Entangled);
  - no payload;
  - a quiet check of **13.0 s** of game time with no further damage and no re-swallow;
  - the freed mouth took a new victim through a real bite (observed at the first bite; asserted within four);
  - the **Tyrannosaurus swallow intact** before and after the reuse;
  - the control intact.
- **Captured:**
  - the victim's damage rose 124 → 145 during the wait, while it was still swallowed (payload rounds before the
    escape), and not afterwards;
  - the raw Engulfed record stayed on the living Dire Crocodile until the new grab replaced it by limb (the live answer
    was empty).

### 3.3 Exact victim-death release: Tyrannosaurus

Same session.

- `GameHelper.KillUnit` of the swallowed victim.
- **Asserted:** released (no swallow state, payload or live link), and the freed mouth took a new victim (first bite).
  The control was intact.
- **Captured path:** `swallow.HandleUnitDeath`, then `native.Free`, then `UnitPartSwallowed.OnRemove`.
- **Captured:** the victim's unrelated Bull's Strength was removed by the engine's death purge (not `StayOnDeath`).
  That is native death behavior, not swallow cleanup.

### 3.4 Shared save, active-swallow holder terminal and fresh-load reset

Session `20261123T015126Z-owner-0.33.0-ver02-c3b-crocodile-tyrannosaurus`, launches 2–3 PASS (112 s, 119 s), harness
`f3a4c99b…db8a`.

- **Save** (asserted):
  - both consumers promoted with `selectedSwallow`;
  - admission allowed;
  - a finished save;
  - both swallows preserved in the running session.
- **Written file** (`Manual_2_ver02c3b.zks`, SHA-256 `b1898e6f…594e`, manual read-only inspection):
  - each holder's `SwallowWhole` lists its victim, with its Engulfed record;
  - each victim carries `UnitPartSwallowed` naming its swallower, with its configured payload (Dire Crocodile
    `327f8a01…`, Tyrannosaurus `b53c756d…`) and CantMove/CantAct counters.
  - So a **complete** relationship was serialized for both.
  - Also captured: the Tyrannosaurus holder still carried its single-link initiator part and hold buff. This is the
    one-frame window between `SwallowHeld`'s `ReleaseLink` and the native controller's next-tick removal of the
    orphaned initiator, because the probe paused immediately after promotion.
- **Active-swallow holder terminal: Dire Crocodile native expiry** (shortened deadline, then explicit native
  `UpdateNextEvent`/`Tick`) while its victim was swallowed, with the **Tyrannosaurus swallow as the ownership control**.
  - Asserted: the holder destroyed; the victim freed with capabilities back; 13.0 s quiet; the Tyrannosaurus swallow
    intact before and after.
  - Captured path: native `SpitOut(all)` (the dying holder's handler), then `OnRemove`, then the carrier's `OnTurnOff`
    (owner on).
  - This fills the gap left by C3a R1, whose Worm expiry came after the victim had been released.
- **Fresh-process reload** (asserted):
  - all four named participants found;
  - at load and after 13.0 s of game time: no live swallowed membership (an empty swallower part is allowed), no victim
    swallowed part, payload, held state, initiator or target part, untargetable, CantMove or CantAct; no raw record;
    no damage.
  - Bleed was not present in this setup, so its preservation was not exercised.

### 3.5 Failed runs (retained)

| Session | Harness | Launch | Cause |
|---|---|---|---|
| `20261123T014648Z` | `efc35569…90a7` | 1 FAIL | **FAIL: the control hold ended for an unattributed reason.** The observer had exhausted its 600-event limit, which prevented attribution; it does not establish the cause. The observer capacity was raised afterwards. The cadence assertions in that launch passed (Dire Crocodile 98 → 117, Tyrannosaurus 104 → 124 at the next round). |
| | | 2 FAIL | **Legitimate rule outcome.** The Tyrannosaurus' maintain returned `released` (traced) before promotion. |
| | | 3 blocked | No written save. |
| `20261123T015126Z` | `f3a4c99b…db8a` | 1 FAIL | **Oracle defect.** The escape happened (`SpitOut(false)` → `Free` → `OnRemove` for the exact victim), but the check counted native `Free` called inside `SpitOut` as a foreign cause. Launches 2–3 PASS (section 3.4). |

**Fixture fixes, labelled:**

- the observer cap raised;
- a traced legitimate maintain release of the control is recorded (an untraced loss still fails);
- after a traced legitimate release before promotion, a fresh setup is allowed; the implementation's budget is two additional promotion setups across the probe;
- the escape oracle fixed to the native chain.

## 4. Assertions, observations and inferences

- **Asserted:** as listed in 3.1–3.4. Escape matching and quiet periods are enforced.
- **Captured:**
  - callback paths;
  - damage during waits;
  - the transitional Tyrannosaurus initiator in the save;
  - the death-purged unrelated buff;
  - the raw record on the living Dire Crocodile after the escape.
- **Manual saved-file inspection:** section 3.4.
- **Source inferences:**
  - the native escape and spit-out mechanics beyond the observed chain;
  - turn-based behavior;
  - bleed's survival through the reset (Sprint 24 records it; not exercised here).
- **Historical evidence reused after dependency inspection:** Sprint 16 and Sprint 24 numbers and gates (section 1).

## 5. Defects

None. No product defect was demonstrated, and no product change was made.

## 6. Remaining G3 and VER-02 coverage

**G3:**

- several engulfed victims in one Flytrap;
- escape for the Worm, Flytrap and Tyrannosaurus (C3b exercised the Dire Crocodile's);
- Flytrap holder death;
- turn-based mode;
- Dire Crocodile/Tyrannosaurus carrier removal;
- bleed preservation through reset;
- D2 ordinary reachability (unresolved; not reattempted).

**VER-02:** G4 stirge; G5 real party exit (fixture-blocked); the G1 variant owners; the Xill, Giant Scorpion and Giant
Crab configurations.

**VER-02: open.**

**Restoration:** every session's transaction completed with game settings applied and owner files verified. No peer
session or Kingmaker process was present when checked before the runs.

## 7. Review decision and narrowed claims (2026-11-23)

**C3b is accepted** for its demonstrated development scope.

| Area | Disposition |
|---|---|
| Promotion and payload timing | Accepted for both consumers |
| Dire Crocodile escape | Accepted for the controlled native escape case (Athletics 300 disclosed) |
| Tyrannosaurus victim death | Release and mouth reuse asserted; the callback path is captured |
| Dire Crocodile active-swallow expiry | Accepted; representative active-swallow expiry evidence |
| Written save and fresh reload | Accepted at the stated evidence levels |

**Narrowed claims:**

- **`014648Z` launch 1** remains FAIL, with an unattributed control release. Observer saturation explains the missing
  attribution, not the release's cause.
- **The escape gate** requires the exact-identity `Free` and `OnRemove` after the exact holder's `SpitOut(false)`. The
  full internal ordering, the call nesting and the upstream successful break-free rule are captured or source-inferred,
  not directly asserted.
- **Tyrannosaurus victim death:** release and mouth reuse are asserted; the `HandleUnitDeath` → `Free` → `OnRemove`
  path is captured.
- **Mouth reuse:** reuse at the first bite was observed; reuse within the four allowed attempts was asserted.
- **Tyrannosaurus setup:** the enforced condition is bleed-free. "Noncritical" is stronger than the gate establishes.
- **Dire Crocodile active expiry** supplies representative active-swallow expiry evidence. It does not turn the Worm's
  earlier post-release expiry into an active-swallow test.
- **Retry budget:** the implementation allows two additional promotion setups across the probe, not one.
- **Retry classification:** it accepted any result beginning with `released`. Before reuse (C3c) it is narrowed to the
  exact ordinary failed-maintain result `released`, for the exact holder and target, inside that attempt's observation
  window. Invalid-owner or invalid-target releases keep their own diagnosis.

This is a records-only update, with no product build or native rerun.
