# Sprint 6 checkpoint C3a (VER-02, G3): Purple Worm and Giant Flytrap swallow/engulf lifecycle

Status: **C3a R1 returned for manual review** (2026-11-23). **C3a is not complete coverage of G3.** VER-02 stays open.
Nothing is merged or released.

- **Accepted by the first C3a review:**
  - the D1 victim-death repair, for the demonstrated Purple Worm/Giant Flytrap scope;
  - D2's constructed-fixture reproduction, immediate save preservation, and the complete-save/fresh-load-reset
    evidence, at their stated scope.
- **R1** (section 7) adds a focused check of three things: active-engulf expiry, a real repeated terminal, and
  continuing periodic payload behavior after a save.
- Where section 7 corrects an earlier section, section 7 governs.

- Branch `codex/s6-ver02-c3a-swallow-engulf` is stacked on C2a's acceptance-records head `afb4af0ba0`
  (`codex/s6-ver02-c2a-save-suspension`, draft PR #206), then #205, #204, #203, #200 and #198. The stack is not rebased.
- C2a R1 is accepted (C2a record, section 9). A1, A2, B, C1 and C2 stay accepted. RISK-03 keeps its limitations.
  REP-05 and REP-06 stay BLOCKED. G4, G5 (fixture-blocked), the G1 variants and the outstanding G2 configurations are
  unchanged.
- **Baseline product:** accepted C2a R1 `044355c2a9` (package `47ec71d0…06fe`, DLL `3746ff5a…a6b2`).
- **Candidate:** `9e902dfa80` (section 4).

## 1. G3 owner map

Sources: `Summoning/ExpandedSummoningSpecialCombatComponents.cs`, `Summoning/SummonGrappleLinkState.cs`,
`Blueprints/ExpandedSummoningSpecialBuilder.cs` (+ `.Theropods.cs`), and the decompiled 2.1.7b `UnitPartSwallowWhole`,
`UnitPartSwallowed` and `UnitSwallowWholeController`.

**Shared machinery (all four consumers).**

- **Promotion.** The grab is G1 (single link) or G2 (multi-link). A later-round successful maintain (`MaintainLink`)
  selects the swallow rider when the target began the round held and its size is allowed. `SummonGrabComponent.SwallowHeld`
  then:
  1. deals the establishing limb's damage;
  2. ends the hold link (`ReleaseLink`);
  3. calls native `UnitPartSwallowWhole.Swallow(target, SwallowedBuff)`;
  4. re-records the mouth and marks it `Engulfed`.
- **Native parts.**
  - The swallower's `UnitPartSwallowWhole.SwallowedUnits` list.
  - The victim's `UnitPartSwallowed`: swallower reference, untargetable, CantMove and CantAct, and `m_Buff`, the
    payload, added with the swallower as caster.
  - The native `UnitSwallowWholeController`: every 6 s on a living swallowed unit, `TryToBreakFree` runs
    (`TryBreakFree`, Default flags). A unit marked free is spat out (with an animation if the view has one).
  - `HandleUnitDeath` and `HandleUnitDestroyed` on the swallower spit everyone out.
  - The native part does not release a unit merely because that unit dies inside. Its releases are the swallower's own
    death or destruction, a successful break-free, and explicit `Free`/`SpitOut` calls.
- **Link ownership.** The swallower's `UnitPartSummonGrappleLinks` record `target|limb|index|weapon|Engulfed|Repaired`.
  The live answer (`Reconcile`) keeps an Engulfed record while the target is in the native `SwallowedUnits` list.
  Limb occupancy and `HeldCount` count swallowed units.
- **Carrier.** `SummonSwallowLifecycleComponent` on each swallower's **combat-traits buff**. That buff also carries the
  grab component, and it lasts the summon's lifetime. Its components:
  - **baseline behavior:** `OnTurnOff` → `SpitOut(true)` unconditionally (changed in section 3);
  - `HandleUnitDeath`/`HandleUnitDestroyed` → exact victim release, only when `ReleaseOnVictimDeath`
    (`Free`, record `Release`, payload `RemoveFact`).
- **Load.** `ResetLoadedGrapples` frees every swallowed unit and removes the payload (the accepted reset).

**Per consumer.**

| Consumer | Grab | Promotion | Payload (periodic) | Victim-death release before C3a |
|---|---|---|---|---|
| Purple Worm (Gargantuan) | G1, bite | swallow ≤ Huge (`PurpleWormSwallowSizeDelta` −1) | `KMG…PurpleWorm.Swallowed`: deep clone of the native `PurpleWormSwallowed` components | **No** (native default) |
| Giant Flytrap (Huge) | G2, four bites | engulf ≤ Medium (absolute) | `KMG…GiantFlytrap.Engulfed`: per-round crush and acid (`AddFactContextActions`), CMB and Dexterity modifiers | **No** (native default) |
| Dire Crocodile | G1 with death-roll rider | swallow (crocodilian size delta) | `KMG…DireCrocodile.Swallowed` | **Yes** (`ReleaseSwallowedOnVictimDeath`) |
| Tyrannosaurus | G1 (`.Theropods.cs`) | swallow | its own payload | **Yes** |

- **C3a exercises:** the Purple Worm and the Giant Flytrap.
- **Remaining G3 coverage:** the Dire Crocodile and the Tyrannosaurus. They share the changed carrier component
  (section 3) but were **not** natively exercised here.

## 2. Baseline findings (accepted C2a R1 product, unchanged)

Every principal relationship was made the ordinary way:

1. the swallower summoned through the product's execution ability;
2. a real eligible bite (`RuleAttackWithWeapon`) establishing the hold;
3. promotion by the product's own later-round `MaintainLink` (`swallowed:N`), with the native `Swallow` witnessed.

The probe never inserts parts, payloads or records, and never calls `SwallowHeld` or `Swallow`.

**Disclosed setup:**

- holder BAB 100;
- victims' HP raised;
- appear buffs removed, brains off, units woken, navmesh placement;
- Medium `DefaultPlayerCharacter` spawns (eligible) and a Large horse (Flytrap engulf-ineligible, stays held).

**D1: a victim that dies inside stays swallowed and keeps its mouth shut** (`20261123T000525Z` launch 1, harness
`b3029e3b…3bea`). After the swallowed Worm victim died inside:

- it stayed in the native `SwallowedUnits` list, with `UnitPartSwallowed` and untargetable on the corpse;
- its Engulfed link stayed live, and the Worm's only mouth stayed occupied (`HeldCount` 1), observed over the following
  round;
- four real bites at a new Medium target were refused, so the mouth could not be reused within the observed window.
  No longer duration was measured.

The Flytrap mixed case behaved the same for the engulfed victim's mouth: four bites refused, while the held sibling
stayed intact.

- **Cause.** The native part only watches its owner's death, and Worm/Flytrap had not opted into the existing exact
  victim-death release.
- **Consequence.** This is a consequence for a surviving owner, so it is not accepted F2/F3-style residue.

**D2: a permitted save released active swallows in the running session** (`20261122T235633Z` launch 3, harness
`76cbb9b1…78d2`; constructed fixture).

- Native admission allowed the save (not in combat).
- During the save the observer recorded, for each swallower: `swallow.OnTurnOff` (owner off), then native
  `SpitOut(all)`, then the victim's `UnitPartSwallowed.OnRemove`.
- Afterwards both victims were free in the continuing session. The Flytrap's held sibling (G2, repaired in C2a) was kept.
- **Written file** `Manual_1_ver02c3a.zks` (SHA-256 `380094c5…646a`, read-only): empty `SwallowWhole` lists; victims
  with no swallowed part, payload or condition; stale raw Engulfed records; the held sibling intact.
- The launch-4 reload (PASS) therefore established only residue cleanup.

**Ordinary reachability of D2: unresolved after a bounded effort** (retained FAIL). The intended route was the Worm's
own AI grab, native Dominate Person on the held enemy, combat ending, then the Worm's maintain swallowing the dominated
victim.

- `20261123T001335Z` (harness `5a90a90d…b3b7`):
  - the Worm's AI did hold the hostile, and saving was refused in combat;
  - Dominate Person ended `Interrupt` with the Worm about 7 m from the caster;
  - the Worm then swallowed the still-hostile enemy, which became untargetable, and saving stayed refused.
- `20261123T001728Z` (harness `43afad08…92bf`): with the Worm left at its summon point, its AI did not hold the hostile
  within 90 s.
- The search was stopped there. D2 is a **constructed-fixture** demonstration. It is not claimed as a demonstrated
  ordinary-play defect, nor as unreachable.

**Fixture and observer failures, retained:**

- `20261122T235633Z` launches 1–2: the control crocodile's target was placed 3 m away, out of bite reach.
- `20261123T000525Z`: a probe cast exception when reading a destroyed holder's null JSON token, in `trapExpiry` and in
  launch 2.

## 3. Repair (product `9e902dfa80`)

**D1.** The Purple Worm's and the Giant Flytrap's `GrabSpec` now set `ReleaseSwallowedOnVictimDeath = true`. This is
the exact, already qualified owned victim-death release that Dire Crocodile and Tyrannosaurus use:

- `SummonSwallowLifecycleComponent.HandleUnitDeath`/`HandleUnitDestroyed` act only when the dead unit is in **this**
  owner's swallowed list, the owner is the configured blueprint, and the payload is the configured buff;
- they then `Free` that unit, drop its record, and remove its payload.

New domain case `expanded-summoning.c3a-swallow-victim-death`: all four registered swallowers opt in.

**D2.** `SummonSwallowLifecycleComponent`, on the combat-traits carrier, follows the accepted C2a pattern, owner-local:

- `OnTurnOff` spits out only on a genuine end (owner still turned on) or during an area unload that this instance
  observed through its own `ISceneHandler.OnAreaBeginUnloading`. That state is per instance and not serialized, and
  it clears on its own `OnTurnOn` (after every save) and on `OnAreaDidLoad`.
- New `OnFactDeactivate` spits out on genuine removal and disposal (idempotent).
- **Carrier-lifetime check.** The traits buff lasts the summon's lifetime and also carries the grab. Its genuine ends
  are removal or dispel (owner on), the death purge (owner on; the native death spit-out already ran), and disposal
  (deactivation). The save is the only owner-off turn-off without an observed unload.

**Unchanged:** the native parts, promotion, payloads, the load reset (holds are still not reconstructed), G1/G2 hold
owners and G4. No process-global latch, save interception, save format or engine patch was added.

**Shared-consumer note:** Dire Crocodile and Tyrannosaurus use the same carrier component, so they inherit D2's save
behavior and the `OnFactDeactivate` release. Neither was natively exercised in C3a (section 6).

## 4. Validation and candidate

- **Gate:** `scripts/Invoke-KmgGate.ps1 -Filter 'grapple;c2a-hold;c3a-swallow;swallow;expanded-summoning.sprint4'`.
  Production compilation 25.8 s; 14 of 3179 tests passed, 0 failures. They include the Sprint 24 swallow reload scope,
  the grab/swallow gates and the crocodilian swallow numbers.
- **Candidate:** `scripts/Build-Local.ps1 -DevelopmentPurpose ver02-c3a-swallow-lifecycle` (same filter), run on a
  detached `9e902dfa80`, 00:22:12Z–00:34:21Z.
  - Package `KingmakerGunslinger-0.0.193-development-ver02-c3a-swallow-lifecycle-9e902dfa8054.zip`, SHA-256
    `704cb0b49f44d9bd614064e947951709f445a93058eb584267f6cbf9f0df79c9`.
  - DLL `431f13da1c90d0681d890cf7987cc7435bd98d87774adc342020929bba1b5676`.
- **Harness:** `43afad08…92bf` (`tools/LoaderCompatHarness/SwallowProbes.cs`, plus swallow witnesses in
  `HoldLifecycleProbes.cs`).
- **Sessions:**
  - `scripts/compatibility/loader-compat/sessions/ver02-c3a-swallow-engulf.json` (victims, holders, save, reload);
  - `ver02-c3a-swallow-terminals.json` (the first two launches);
  - `ver02-c3a-swallow-reach.json`.
- **Fixture:** `KMCS_AFTER_NORMAL_012_MODONLY.zks` (`0be64b3f…a249`); loader profile `owner-0.33.0`; product alone;
  RTWP.

## 5. Candidate results (`20261123T003442Z`, PASS 4 of 4: 128 s, 128 s, 91 s, 96 s)

**Asserted** (fail the run):

- **Promotion**, for each case:
  - a reciprocal native swallow (swallower list and victim part);
  - the exact payload with the swallower as caster;
  - untargetable, CantMove and CantAct;
  - the superseded held state gone;
  - one raw Engulfed record;
  - a live engulfed link;
  - the establishing mouth occupied by the victim.
- **Payload round:** damage in one real round (Worm +27, Flytrap +24).
- **Victim death** (Worm; Flytrap with the held sibling):
  - no native relationship and no payload left;
  - no live link;
  - **a new victim taken by that mouth through a real bite at the first attempt**;
  - the Flytrap sibling still held, with its unrelated effect kept;
  - the control pair intact.
- **Flytrap accelerated expiry** (shortened deadline, then explicit native `BuffCollection.UpdateNextEvent`/`Tick`):
  holder destroyed, the held sibling released, control intact.
- **Worm holder death:** the victim freed, with capabilities back (no CantMove, CantAct, untargetable or Entangled);
  its unrelated effect kept; then a quiet check with **≥ 12 s of game time** (13.0 s), no damage, and no relationship
  recreated.
- **Repeated terminal:** the dead Worm no longer had a `SummonedUnitBuff` (the death purge had removed it), so no second
  expiry fired. This step therefore only let 2 s of game time pass and re-asserted that the victim stayed free (and
  the control held). It is a weak repeated-terminal check.
- **Flytrap carrier removal:** native `RemoveFact` on the actual combat-traits buff, holder alive. Both relationships
  ended (the engulfed victim freed; the held sibling released); then the quiet check (13.0 s). Control intact.
- **Save:**
  - native admission allowed it (asserted);
  - it completed;
  - **both swallows and the held sibling were kept** in the continuing session: reciprocal parts, payloads, constraints,
    Engulfed records, live links and occupancy.
- **Reload** (fresh process):
  - all five named units found;
  - at load and after 13.0 s: no swallow part or list, held state, payload, untargetable, CantMove or CantAct on any of
    them, and no damage.

**Captured observations** (from the read-only observer; not gates):

- **Victim-death path:** `swallow.HandleUnitDeath`, then `native.Free`, then `UnitPartSwallowed.OnRemove`, in the same
  frame as the death.
- **Worm holder death:** native `SpitOut(all)` (the swallower's own death handler) ran first; then the carrier's
  `OnTurnOff` (owner on) ran `SpitOut` again, a no-op.
- **Flytrap carrier removal:**
  - `swallow.OnTurnOff` (owner on), then `SpitOut`, freed the engulfed victim immediately;
  - the held sibling was released at its next round: with the traits buff gone, `SummonGrabComponent.Find` returns
    null, so `SummonHeldComponent.OnNewRound` removed the held state;
  - the living holder kept both raw records. Their live answers are empty, occupancy is free, and without its traits
    it has no grab component. Recorded as inert residue; no consequence demonstrated.
- **The save:** `swallow.OnTurnOff` ×2 with owner off, released nothing; the C2a hold owner was suspended and resumed.
- The Worm holder-death raw Engulfed record stayed on the dead holder (the accepted F3 pattern).
- Maintain results for the control pair (`death-roll`, holds kept).

**Written file** (`Manual_1_ver02c3a.zks`, SHA-256 `72425544…90dd`, read-only):

- Worm: `SwallowWhole` lists its victim, and the Engulfed record is present.
- Worm victim: `UnitPartSwallowed` (swallower id, payload buff `8a25b841…`), CantMove and CantAct counters.
- Flytrap: `SwallowWhole` lists its victim; both records are present (held sibling plus Engulfed).
- Flytrap victim: `UnitPartSwallowed` (payload `6f8239e5…`).
- Held sibling: multi-held buff.

So a **complete** swallow/engulf relationship was serialized, and the reload established the intentional reset of that
complete input.

**Source inferences:**

- the area-unload branch for an area-state swallower;
- the pre-exit autosave;
- the reachability of D2 in ordinary play;
- Dire Crocodile/Tyrannosaurus behavior under the changed carrier.

## 6. Remaining G3 scope and limits

- **Dire Crocodile and Tyrannosaurus:** their swallow, victim-death, terminal and save boundaries under the changed
  carrier are not natively exercised.
- **Not exercised for Worm/Flytrap:**
  - an escape or break-free (native `TryToBreakFree`; with BAB 100 the holder's CMD defeats it);
  - Worm carrier removal and Worm expiry (only the Flytrap's were exercised);
  - Flytrap holder death;
  - turn-based mode;
  - several victims in one Flytrap.
- D2's ordinary reachability is unresolved.
- The concurrent independent control was a crocodile hold. A swallowing control was not used.
- **Retained FAIL sessions:** `235633Z` (launches 1–3), `000525Z`, `001335Z` and `001728Z`, with the causes in
  section 2.
- **Restoration:** every session's transaction completed with game settings applied and owner files verified. No peer
  session or Kingmaker process was present when checked before the runs and the build.

**VER-02: open.**

## 7. C3a R1: active-engulf expiry, payload cadence through a save, and two real terminals (2026-11-23)

**Identities and validation.**

- Reviewed head `cb6bf3f12d`.
- The product is **unchanged**: candidate `9e902dfa80` (package `704cb0b4…79c9`, DLL `431f13da…5676`) was reused.
  - The only product-source edit in R1 is a comment: the victim-death handler comment now says all four owners opt in,
    and that the native part does not release merely on victim death.
  - The package was not rebuilt.
- **Test cleanup.** `EverySwallowerReleasesAVictimThatDiesInside()` and its registration were removed. Its source-string
  count did not establish one correct opt-in per owner. The original C3a gate result (14 of 3179) stays as the
  historical record of what was run.
- **R1 gate:** `-Filter 'grapple;c2a-hold;swallow;expanded-summoning.sprint4'`. Production compilation 16.6 s; 13 of
  3178 tests passed, 0 failures.
- **Harness:** `bd8892be…c617` (`artifacts/loader-compat-harness-ver02-c3a-r1`), with two new `kmgSwallow` modes,
  `activeExpiry` and `saveCadence`.
- **Session:** `scripts/compatibility/loader-compat/sessions/ver02-c3a-r1-focused.json`.
- **Fixture:** `KMCS_AFTER_NORMAL_012_MODONLY.zks`; product alone; RTWP. Disclosed setup as in section 2.

**Native session `20261123T012518Z-owner-0.33.0-ver02-c3a-r1-focused`: PASS 2 of 2 (96 s, 110 s).**

### 7.1 Active engulf expiry (launch 1)

- **Preconditions** (asserted immediately before expiry, paused):
  - the Flytrap held a living Medium victim **engulfed through the real maintain path** (`MaintainLink`
    `swallowed:63`): reciprocal native swallow, Engulfed payload with the Flytrap as caster, untargetable,
    CantMove/CantAct, raw Engulfed record, live link, mouth occupied;
  - it also held a living, engulf-ineligible Large horse, which is held, CantMove and occupying its mouth;
  - the crocodile control pair was held;
  - both victims were alive.
- **Unrelated effects:** a Bull's Strength on each victim and on the control target.
- **Expiry:** `SummonedUnitBuff` deadline shortened, then explicit native `BuffCollection.UpdateNextEvent`/`Tick`; the
  marker was removed by that tick.
- **Asserted outcomes:**
  - the holder was destroyed;
  - both owned relationships were released: no swallowed part or list membership, no payload, no held state, no live
    link, no CantMove/CantAct/untargetable/Entangled on either living victim;
  - over **13.0 s of game time** that followed, neither victim took damage and nothing was recreated;
  - both victims' unrelated effects were kept, and the control pair stayed held.
- **Captured path:** native `SpitOut(all)` (the dying holder's own handler), then `UnitPartSwallowed.OnRemove`, then the
  carrier's `swallow.OnTurnOff` (owner on; `SpitOut` again, a no-op), then the hold `OnTurnOff`, `ReleaseLink` for the
  sibling, then `OnFactDeactivate`.
- **Captured damage:** the victims took periodic damage before the release (engulfed 129 → 145, sibling 130 → 196) and
  none after it.

### 7.2 Payload cadence through a permitted save (launch 2)

The Worm swallow and the mixed Flytrap engulf were each established through the real summon, bite and maintain path.
Each payload then aged through a real round.

**Native payload state.** `Buff.RoundNumber` and, by reflection, the internal `NextTickTime`/`TickTime`. These are
asserted where marked.

| | Paused, before save | After the finished save | Next genuine round (same running game) |
|---|---|---|---|
| Worm payload (`PurpleWorm.Swallowed`) | instance −1048864000, round 1, next tick 432490790000, interval 6 s, damage 218 | identical | round **2**, next tick 432550790000 (exactly +6 s), same instance, damage **250** |
| Flytrap payload (`GiantFlytrap.Engulfed`) | instance 1598095360, round 1, next tick 432492050000, interval 6 s, damage 152 | identical | round **2**, next tick 432552050000 (exactly +6 s), same instance, damage **173** |

**Asserted:**

- native save admission (allowed);
- full completion: callback received, no commit in progress, no loading in progress. A wait that expired with work
  unfinished would fail;
- an unchanged game clock;
- an unchanged payload instance, round number, next tick, interval, caster and victim damage (no save-time damage);
- exact raw records and mouth occupancy;
- both swallows and the held sibling preserved;
- the control's unrelated effect kept.

**Then, in the same running game** (asserted), at game time past the later pre-save next tick:

- each payload's round number is exactly +1;
- its next tick is exactly the pre-save schedule plus one interval;
- the instance is unchanged, so there was no restart and no duplicate;
- its victim took new damage;
- the held sibling is still held. An untraced disappearance would fail; a traced maintain release would be recorded as
  legitimate. None occurred.

**Captured:**

- the save's callbacks: hold `OnTurnOff` ×2 and `swallow.OnTurnOff` ×2, all with owner off and no release;
- the next-round outcomes (maintain results);
- no escape occurred.

The fresh-load reset evidence from section 5 is reused. No reload was rerun, because the changed observer does not
touch that path.

### 7.3 Two real terminals on the post-save Worm (launch 2)

1. **First terminal: carrier removal** with the holder alive. Native `RemoveFact` on the Worm's actual
   `PurpleWorm.CombatTraits` buff ran `swallow.OnTurnOff` (owner on), then `SpitOut(all)`, then `UnitPartSwallowed.OnRemove`.
   - Asserted: the carrier is gone, the holder stayed alive, and the victim is freed (no swallow state, payload or
     constraints).
   - Captured: the raw Engulfed record stayed on the living holder. Its live answer is empty, its occupancy is free,
     and without its traits the Worm has no grab component (inert).
2. **Second terminal: native expiry.** `SummonedUnitBuff` was present; its deadline was shortened, then explicit native
   `UpdateNextEvent`/`Tick` ran.
   - Asserted: the Worm was destroyed; over 13.0 s of game time the freed victim took no further damage and was not
     re-swallowed; the control pair and its unrelated effect were intact.
   - Captured: native `SpitOut(all)` at the death, a no-op.

This also covers the previously unexecuted **Worm carrier-removal** boundary and a **Worm expiry** (of an already
released Worm).

### 7.4 Corrections to the first C3a record and summaries

- **Flytrap expiry.** `trapExpiry` in `003442Z` ran after the engulfed victim had died. Its regrabbed replacement was
  never promoted and was not tracked, and the assertions covered only the held sibling. It is **not** an active-engulf
  expiry; active engulf expiry was unqualified until 7.1.
- **The old "repeated terminal"** (`003442Z` launch 2) was a quiet recheck only: the dead Worm had no expiry buff. No
  second-terminal credit. The real second terminal is 7.3.
- **Mouth reuse.** Reuse at the first bite was **observed**; successful reuse within the bounded attempts was
  **asserted**.
- **Carrier removal.** In `003442Z`, the holder being alive was **captured**. In 7.3 it is **asserted**.
- **Reload check.** It allows an empty `UnitPartSwallowWhole` on a swallower. It rejects live list membership, victim
  swallowed parts, payloads, held states, untargetable, CantMove/CantAct, and raw records.
- **D1** is post-death blockage and refused regrabs within the observed window. No "for the rest of the summon's life"
  duration was demonstrated.
- **Native release paths.** The native swallow part does not release merely because a victim dies. Its other release
  paths (swallower death or destruction, break-free, explicit free) are not limited to swallower death.
- **Carrier turn-off.** The unconditional `OnTurnOff` → `SpitOut(true)` is **baseline** behavior; the repaired behavior
  is section 3.
- **D2 ordinary reachability remains unresolved.** It was not reattempted in R1.

**Remaining G3 and VER-02 coverage:**

- Dire Crocodile and Tyrannosaurus (they share the changed carrier; not natively exercised).
- Escape/break-free.
- Flytrap holder death.
- Turn-based mode.
- Several victims in one Flytrap.
- D2 ordinary reachability.
- G4, G5 (fixture-blocked), the G1 variants, and the Xill/Scorpion/Crab configurations.

**VER-02 open.**

**Restoration.** The R1 session's transactions completed with game settings applied and owner files verified. No peer
session or Kingmaker process was present before the run.
