# Sprint 6 checkpoint C3c (VER-02, G3): two engulfed victims in one Giant Flytrap, and the G3 disposition

Status: **qualified natively on the accepted C3a product with no product change; returned for manual review**
(2026-11-23), with a proposed G3 disposition (section 6). VER-02 stays open. Nothing is merged or released.

- Branch `codex/s6-ver02-c3c-flytrap-multi-engulf` is stacked on the C3b acceptance-records head `05a8044706`
  (`codex/s6-ver02-c3b-crocodile-tyrannosaurus`, draft PR #208), then #207 and the earlier drafts. The stack is not
  rebased.
- **Product (unchanged, reused):** accepted C3a `9e902dfa80`. Package `704cb0b4…79c9`, DLL `431f13da…5676`.
- **Harness:** `20c51cda516dbe0d3e0bf89ba01e51c562044049b989c0fc041e19209acaf234`
  (`tools/LoaderCompatHarness`, modes `c3cMulti` and `c3cReload`).
- **Session:** `scripts/compatibility/loader-compat/sessions/ver02-c3c-flytrap-multi-engulf.json`.
- **Fixture:** `KMCS_AFTER_NORMAL_012_MODONLY.zks` (`0be64b3f…a249`); loader profile `owner-0.33.0`; product alone;
  RTWP.
- **Kept:** A1/A2/B, C1, C2, C2a R1, C3a R1 and C3b acceptance; RISK-03's limits; the REP-05/06 blocks; G4; G5
  (fixture-blocked); the G1 variants; the outstanding G2 configurations.

## 1. Why this state shape

The earlier mixed Flytrap case (C3a) had one engulfed victim and one merely held target. Two engulfed victims instead
share one native `UnitPartSwallowWhole.SwallowedUnits` list and one lifecycle carrier. The product's two release
operations act on that collection differently:

- the victim-death handler calls native `Free` for the exact victim and releases that victim's owned link;
- the carrier's terminal path and the native holder-death handler call `SpitOut(all: true)`.

C3c exercises exact removal of one entry while the other survives, reuse of the freed mouth while the survivor stays
active, preservation of the two-entry relationship through a save, and holder-wide release of both entries.

## 2. Harness corrections carried from the C3b review (before reuse)

- **Ordinary failed maintain** is now the product's exact result `released` (a failed grapple check;
  `ExpandedSummoningSpecialCombatComponents.cs`, `MaintainLink`), for the attempt's exact holder and one of its exact
  eligible targets, inside that attempt's observation window (`IsOrdinaryFailedMaintain`). Invalid-owner/target
  releases (`released:invalid-…`) are not ordinary and fail with their own diagnosis.
- **Explicit budget:** `PromotionRetryBudget = 2` additional setups per probe, shared by `EstablishSwallow` and the new
  `EstablishPromoted`.
- **Evidence kept:** every promotion attempt is appended to `promotionAttempts.<name>`, and every setup attempt to
  `setup.<name>` / `setupAttempts.<name>`, with its holder, targets, eligibility and establishment state. Earlier
  attempts are no longer overwritten.
- **Control witness:** a control that ended through a traced ordinary failed maintain (exact result, holder and target,
  inside the control's own window) is recorded as **no longer an active ownership witness**; an untraced end still
  fails.
- **Mouth identity** is recorded by body slot (`primaryHand`, `additionalLimbN`) with its occupant, so a helper's list
  index does not stand in for the target-to-mouth mapping.

These do not reopen C3b. In this run no retry was used (each setup's first attempt promoted).

## 3. Method and disclosed setup

- **Independent ownership control:** a Purple Worm, summoned through the product, swallowing a Medium victim through a
  real bite and its own later-round maintain (`swallowed:98`), plus an unrelated Bull's Strength on that victim. A
  native swallow needs no per-round maintain, so it stays active through the decisive terminals.
- **Two-entry setup:** one Giant Flytrap summoned through the product's execution ability. Two Medium
  `DefaultPlayerCharacter` spawns, A and B, were bitten by **two distinct limbs** (A: `primaryHand`, B:
  `additionalLimb0`, both `BiteLarge1d8`). The Flytrap's own later-round `MaintainLink` engulfed both (`swallowed:63`
  for each). An unrelated Bull's Strength was added to B.
- **Disclosed:** holder BAB 100; victims' HP raised; appear buffs removed; brains off; units woken; navmesh placement;
  pauses for observation. Nothing populated the swallowed list, forced promotion, removed a payload or called a
  release helper.
- **Instrumentation:** the read-only `HoldLifecycleObserver`; native payload state by `Buff.RoundNumber` and
  `NextTickTime`/`TickTime` (reflection).

## 4. Results (`20261123T024057Z-owner-0.33.0-ver02-c3c-flytrap-multi-engulf`, PASS 2 of 2: 133 s, 116 s)

### 4.1 Two engulfed victims on one Flytrap

**Asserted:**

- the Flytrap's native swallowed list holds exactly A and B;
- for each: a reciprocal `UnitPartSwallowed` naming the Flytrap, the `GiantFlytrap.Engulfed` payload with the Flytrap
  as caster, untargetable, CantMove and CantAct, no superseded held state, one raw Engulfed record, a live engulfed link;
- exactly two raw Engulfed records;
- each limb occupied by its own victim (`primaryHand` → A, `additionalLimb0` → B);
- the Worm control swallow intact.

**Captured:** the multi-hold buff was gone once both targets had promoted (not required); both victims at damage 126.

### 4.2 Exact removal of A while B survives

`GameHelper.KillUnit(A)` while both were alive and engulfed.

**Asserted:**

- A: no native membership, victim part, payload, held state or live link;
- A's limb (`primaryHand`) free; B's limb still B; the native list holds exactly B;
- B fully engulfed by the same Flytrap, with the **same payload instance** and caster, and its round not reset;
- B's next genuine payload round on its retained schedule: round 0 → 1, next tick exactly +6 s, same instance, damage
  126 → 142;
- the Worm control intact.

**Captured:**

- path in the death frame: `swallow.HandleUnitDeath` → `native.Free` (A) → `UnitPartSwallowed.OnRemove` (A);
- **raw records** after the removal: only B's Engulfed record remained, and the live answer was B alone. No residue was
  left for A.

### 4.3 Reuse of A's freed mouth by C

C, a fresh Medium spawn, was bitten with A's freed `primaryHand` limb.

**Asserted:**

- C held by that limb within the four allowed bites (**first bite observed**);
- C engulfed by the real maintain path (`swallowed:69`);
- the native list holds exactly B and C; `primaryHand` → C, `additionalLimb0` → B; exactly two raw Engulfed records;
- B and C fully engulfed (reciprocal parts, owned payloads, constraints, live links);
- the Worm control intact.

### 4.4 The two-entry relationship through a save

B and C were both aged through genuine payload rounds, then the game was paused.

| | Paused, before save | After the finished save | Next genuine round |
|---|---|---|---|
| B payload | instance 519995392, round 3, next tick 432668340000, interval 6 s, damage 180 | identical | round **4**, next tick exactly +6 s, same instance, damage **197** |
| C payload | instance −2113905792, round 1, next tick 432673430000, interval 6 s, damage 157 | identical | round **2**, next tick exactly +6 s, same instance, damage **173** |

**Asserted:** native save admission allowed; full completion (callback received, no commit, no loading in progress);
an unchanged paused clock; unchanged payload instance, round number, next tick, interval, caster and damage for both;
an unchanged native list, raw records, live links and limb occupancy; both engulfed; B's unrelated effect kept; then
round +1 on the retained schedule with the same instance and new damage for both; the Worm control intact.

**Written file** (`Manual_1_ver02c3c.zks`, SHA-256
`911deb8149586c194e0cd50b89628146046dd9051f38fe36e0d5e9fc57b81cc1`; manual read-only inspection):

- `KMG_C3c_Trap`: `UnitPartSwallowWhole` lists B and C; `UnitPartSummonGrappleLinks` holds both Engulfed records
  (`…|2|0|BiteLarge1d8|1|0` for B, `…|1|-1|BiteLarge1d8|1|0` for C);
- `KMG_C3c_VictimB` and `KMG_C3c_VictimC`: each a `UnitPartSwallowed` naming the Flytrap, with its own
  `GiantFlytrap.Engulfed` (`6f8239e5…`) payload, the Flytrap as caster, and CantMove/CantAct counters;
- the saved payload state matches the live values: B round 3, next tick 12:01:06.834; C round 1, next tick
  12:01:07.343; interval 6 s;
- B's Bull's Strength is serialized.

So the complete two-entry relationship was serialized.

### 4.5 Holder death with both entries engulfed

After that save, the living Flytrap was killed with `GameHelper.KillUnit` while B and C were engulfed.

**Asserted:**

- immediately before: both relationships active;
- both released: no swallowed part, list membership, payload, held state, live link, CantMove, CantAct, untargetable or
  Entangled on either;
- over **13.0 s** of game time after it: no damage to either and nothing recreated;
- the Worm control swallow intact (payload round 7 by then), with its unrelated effect; B's unrelated effect kept.

**Captured:**

- **release timing:** 0.012 s of game time (the first frame after the kill);
- path: native `SpitOut(all: true)` by the dying Flytrap (its own death handler), then `UnitPartSwallowed.OnRemove` for B
  and for C. Later carrier callbacks were outside this capture window;
- **dead-holder raw residue:** the dead Flytrap kept both raw Engulfed records, with an empty live answer, no
  membership and no occupancy (the accepted F3 pattern). Its combat-traits buff was later removed by the death purge.
  Residue alone is not a product defect.

### 4.6 Fresh-process reset of the saved two-entry relationship

Launch 2 loaded `Manual_1_ver02c3c.zks`, written before the Flytrap's death, in a fresh process.

**Asserted:**

- all three named participants found (`KMG_C3c_Trap`, `KMG_C3c_VictimB`, `KMG_C3c_VictimC`);
- at load and after 13.0 s of game time: no live swallowed membership (the Flytrap's swallower part is present and
  empty), no victim swallowed part, payload, held state, initiator or target part, untargetable, CantMove or CantAct;
  no raw records; no damage.

**Captured:** B's Bull's Strength survived the load (an independent effect preserved).

No restoration, new save format or relationship rebuilding was added.

## 5. Evidence classes and defects

- **Asserted:** as listed in 4.1–4.6.
- **Captured:** callback paths, release timing, the dead-holder raw residue, the multi-hold buff's absence, the death
  purge of the dead holder's traits, B's effect after load.
- **Manual saved-file inspection:** section 4.4.
- **Source inferences:** that `HandleUnitDeath` reaches `Free` and `ReleaseLink` for the exact victim; the native
  holder-death handler's `SpitOut(all)`.
- **Historical evidence:** none needed for this case.
- **Defects:** none demonstrated. No product change, build or broad suite was run.
- **Failed attempts:** none in C3c.

## 6. Proposed G3 disposition

G3 covers the four published swallow consumers: Purple Worm and Giant Flytrap (C3a), Dire Crocodile and Tyrannosaurus
(C3b). All four share the product's swallow machinery: later-round promotion through `SwallowHeld`, the
`SummonSwallowLifecycleComponent` carrier, victim-death release, and the native swallow part and controller.

| Mechanism | Directly executed (product `9e902dfa80`) | Shared implementation coverage | Historical | Retained limitation |
|---|---|---|---|---|
| Promotion through the real later-round maintain | All four (C3a, C3b); the Flytrap with two entries (C3c) | — | Sprint 16/24 gates | — |
| Payload continuation through a permitted save | All four (C3a R1 7.2; C3b 3.1); two entries on one Flytrap (C3c 4.4) | — | — | Turn-based timing not executed |
| Exact victim removal and mouth reuse | Worm, Flytrap (C3a); Tyrannosaurus (C3b); Flytrap with a surviving second entry (C3c 4.2–4.3) | Dire Crocodile: the same handler and opt-in flag | — | Dire Crocodile victim death not executed |
| Multiple entries on one native owner | Flytrap, two engulfed (C3c); engulfed plus held (C3a) | The Flytrap is the only multi-mouth swallower | — | — |
| Native escape | Dire Crocodile (C3b; Athletics 300 disclosed) | Unmodified native controller and rule; the same native `SpitOut(false)` → `Free` path for every consumer | — | Escape on the Worm, Flytrap and Tyrannosaurus, and one-of-two escape, not executed |
| Active expiry | Flytrap with an engulfed victim (C3a R1 7.1); Dire Crocodile (C3b) | Same native death handler and carrier | — | Worm expiry only after release; Tyrannosaurus expiry not executed |
| Holder death | Worm (C3a); Flytrap with two entries (C3c 4.5) | Same native death handler and carrier | — | Dire Crocodile and Tyrannosaurus holder death not executed |
| Carrier removal | Flytrap (C3a); Worm (C3a R1 7.3) | The same carrier component on every traits buff | — | Dire Crocodile and Tyrannosaurus carrier removal not executed |
| Save preservation and fresh-load reset | All four (C3a, C3b); two entries (C3c 4.4, 4.6) | — | Sprint 24 clean-reset decision | Bleed through reset not exercised |
| Turn-based mode | None | Same components; a different round-timing route | — | Not executed |
| D2 ordinary-play reachability | Constructed reproduction only (C3a) | — | — | Unresolved |

**Recommendation: accept G3 for representative development scope.**

- Every mechanism has direct execution on at least one consumer, and the state shape that differs materially (several
  entries on one owner) has now been executed through removal, reuse, save, holder-wide release and reset.
- The remaining items are unexecuted repetitions of shared implementation: other species' escape, expiry, holder death
  or carrier removal, and Dire Crocodile victim death. None runs a separate product code path that existing evidence
  leaves unexercised, so none needs another G3 checkpoint.
- **Accurately retained, not PASS:**
  - **Turn-based mode** uses a distinct round-timing route. It is a cross-cutting VER-02/turn-based question rather
    than a G3 species gap; qualify it separately if the owner wants it.
  - **Bleed through reset** is a native `Bleed2d6Buff` that no swallow code touches. It relies on Sprint 24's
    historical record.
  - **D2 ordinary-play reachability** remains unresolved; it was not reattempted.

**VER-02 stays open:**

- G4 stirge;
- G5 real party exit (fixture-blocked on an admitted transition fixture; no further fixture searches here);
- the G1 variant owners;
- the Xill, Giant Scorpion and Giant Crab configurations.

**Restoration:** the session's transaction completed with game settings applied and owner files verified. No peer
session or Kingmaker process was present when checked before the run.
