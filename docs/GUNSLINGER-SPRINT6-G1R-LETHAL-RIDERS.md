# Sprint 6 VER-02 checkpoint G1-R: lethal rider release (five selected paths)

Status: **G1-R R1 accepted by manual review (2026-11-23)** for its demonstrated representative RTWP lethal-terminal
scope under native life-update timing, at head `cb8ef00a34` (base `13f80c22cd`; product `9e902dfa80`; harness
`cf5d90c2…8fc8d`; session `20261123T162752Z`). Section 13 records the decision and corrections and governs where it
differs. Earlier status (as returned): **qualified under the reviewed native life-update contract and returned for
manual review**.
Sections 11 and 12 govern; sections 1–10 are the first characterization round, kept with its FAIL verdicts and
corrected in place where the review withdrew claims. The accepted product is unchanged: no product repair, build or
candidate. VER-02 stays open. Nothing is merged or released.

- **Characterization round (sections 1–10):** four FAIL sessions on a revivable player-faction fixture target.
- **Correction (sections 11–12):** the review chose the native-timing contract. Four more sessions ran on native
  hostile targets; the last, `20261123T162752Z`, **PASS 5 of 5**.

- Branch `codex/s6-ver02-g1r-lethal-riders`, stacked on the G1/G2 acceptance-records head `13f80c22cd`
  (`codex/s6-ver02-g1g2-barbed-crab`, draft PR #211). The stack is not rebased.
- **Product (unchanged, reused):** accepted C3a `9e902dfa80`. Package `704cb0b4…79c9`, DLL `431f13da…5676`.
- **Harness:** commit source = the diagnostic build `b95905fd…`; earlier builds are listed in section 7. A `lethal` mode of `kmgBarbedCrab` (`tools/LoaderCompatHarness/LethalRiderProbes.cs`, a partial of
  `BarbedCrabProbe`), plus read-only observer additions in `HoldLifecycleProbes.cs`.
- **Sessions:** `scripts/compatibility/loader-compat/sessions/ver02-g1r-lethal-riders.json` (five cases in one launch)
  and `ver02-g1r-diag-a.json` (case A only, with diagnostic observers), in the characterization round;
  `ver02-g1r-native-lethal.json` in the correction (section 12).
- **Fixture:** `KMCS_AFTER_NORMAL_012_MODONLY.zks`; loader profile `owner-0.33.0`; product alone; RTWP.
- **Kept:** all earlier acceptance and limits (A1/A2/B, C1–C3c, G3, G4, G1/G2 for their scopes; F2/F3; the C2a unload
  limits; RISK-03; REP-05/06; G5 blocked).

## 1. The five selected paths (source)

`Summoning/ExpandedSummoningSpecialCombatComponents.cs`, `Summoning/SalamanderCombatComponents.cs`.

| Case | Owner | Intended branch |
|---|---|---|
| A | Constrictor Snake | `MaintainLink` live-constrict: after `DealWeaponDamage`, `if (liveConstrict && (target.Destroyed \|\| target.Descriptor.State.IsDead)) ReleaseLink(…, true); return "maintained:N;targetDied=True;heldKept=False"`, before constrict |
| B | Constrictor Snake | `DealConstrict` physical path (shared with Lillend): after the constrict damage rule, `if ((ConstrictProfileOwner \|\| LillendProfileOwner) && dead) ReleaseLink(…, true)` |
| C | Salamander | `DealConstrict` Salamander path: after `SalamanderConstrictDamage.Deal`, `if (dead) ReleaseLink(…, true)` |
| D | Crocodile | `DealDeathRoll`: after the death-roll damage, `if (dead) { ReleaseLink(…, true); return "…targetDied=True;heldKept=False"; }` |
| E | Barbed Devil | `DealImpale`: `bool died = …IsDead; if (died) ReleaseLink(…, true)`; single-link `ReleaseLink` then schedules the exact-owner cleanup |

Every branch reads `target.Descriptor.State.IsDead` **synchronously, immediately after its own `RuleDealDamage`**.

## 2. Native contract (engine source, Kingmaker 2.1.7b, decompiled read-only)

- `RuleDealDamage.OnTrigger` adds to `Target.Damage` and raises `IDamageHandler`; it does not change the life state.
- `UnitState.IsDead` is `LifeState == Dead`. The life state is assigned only by `UnitLifeController.TickOnUnit`
  (per-unit controller tick): Dead at `Damage ≥ HP + Con`, or at `Damage + NonLethal ≥ HP` without the dying allowance
  or Ferocity.
- `UnitReturnToConsciousController.Tick` (out of combat) makes any unit that is not conscious and **not finally dead**
  conscious again, with `Damage = HP − max(1, level)`. `OnUnitDeath` leaves a unit whose **blueprint faction is the
  player faction** not finally dead unless the difficulty has true death. (The product's Gorgon transpiler on this
  controller only changes eligibility for Gorgon-petrified units.)
- The product already has a precedent for reading the native life state after its own damage:
  `Spells/Teleportation/TeleportationMishapDamageTarget` subclasses `UnitLifeController` and runs the protected
  `ShouldTickOnUnit`/`TickOnUnit` for one unit.

## 3. Method and disclosed setup

- **Each case:**
  - a real hold from a real attack with the exact grab limb (`EstablishSingle`, the G1/G2 contract);
  - one ordinary rider round, asserted held, with the expected rider (`maintained:` for A–C, `death-roll:` for D,
    `impale:` for E) and the target alive and conscious;
  - a disclosed health setting on the disposable target: `Damage = HP − margin`, with margin 1 for A, D and E, and the
    establishing weapon's maximum + 1 for B and C. The maximum comes from a read-only `RuleCalculateWeaponStats`.
    The target's native dying allowance is released, so Dead means `Damage ≥ HP`;
  - the pre-decisive contract asserted (holder alive and valid, target alive and conscious, reciprocal parts, buffs,
    establishing limb, record and occupancy);
  - then the decisive native round.
- **Other disclosed setup:**
  - holder BAB 100;
  - target HP 100000 and escape stats −100;
  - summon deadlines extended to 600 s;
  - appear buffs removed, brains off, units woken;
  - pairs within 10.5 m of the summoner;
  - two fresh same-owner controls per case, each with an unrelated Bull's Strength.
- **Instrumentation (read-only):**
  - brackets around each rider damage owner (`rider.weapon`, `DealConstrict`, `rider.salamanderConstrict`,
    `rider.deathRoll`, `rider.impale`);
  - the target's damage, HP and life state on every damage rule, maintain result and release;
  - per-frame Dead detection;
  - from session 2 on: heals and per-frame damage decreases;
  - in the diagnostic run: the target's `UnitDescriptor.Damage` setter (stack on decrease) and
    `UnitLifeController.TickOnUnit` on the target.
- Nothing called a rider, a release helper or `KillUnit`, set a death flag, removed a part or forced a die.

## 4. Results

### 4.1 Five-case table (`20261123T153430Z`, the clearest five-case baseline with documented fixture revival, harness `49265269…`; and diagnostic `154102Z`)

`153430Z` is **not** an unconfounded single lethal terminal. The first threshold crossing in each case produced a
transient native death followed by native recovery, then a later final death (4.2).

| Case | Rider round before | Decisive maintain result | Crossing (asserted) | Intended release branch | Final native death and release |
|---|---|---|---|---|---|
| A Snake, weapon-lethal | `maintained:5` (attempt 2; attempt 1 retried after an exact ordinary failed maintain) | `maintained:8;limb=…Bite1d4` | the bite, inside `rider.weapon` (8 → Damage 100007 of HP 100000) | **did not run.** Constrict **executed** after the lethal bite (+7) | next crossing frame 5025 → Dead 5026 → `SummonHoldComponent.Release` 5026 |
| B Snake, constrict-lethal | `maintained:8` | `maintained:8;limb=…Bite1d4` | weapon 8 survived (99999), then **constrict** crossed, inside `DealConstrict` (7 → 100006) | **did not run** | 6013 → Dead 6014 → Release 6014 |
| C Salamander | `maintained:34` (attempt 1) | `maintained:38;limb=…Salamander_Tail` | tail 38 survived (99992), then the **Salamander constrict** bundle crossed, inside `rider.salamanderConstrict` (16 → 100008) | **did not run** | 7001 → Dead 7002 → Release 7002 |
| D Crocodile | `death-roll:` | `death-roll:…dealt=10;…proneAfter=True;heldKept=True` | the death roll, inside `rider.deathRoll` (10 → 100009) | **did not run** (`heldKept=True`, prone applied) | 8021 → Dead 8022 → Release 8022 |
| E Barbed Devil | `impale:` | `impale:3d8+9;…dealt=25;targetDied=False;heldKept=True` | the Impale, inside `rider.impale` (25 → 100024) | **did not run**; no `impaleCleanup.schedule` | 9066 → Dead 9067 → Release 9067 |

All five: **failed** the then-required check that the synchronous release branch runs. The five synchronous
release branches **did not execute in the tested scenarios**. Damage-owner identification (the bracket) and the B/C
survival of the weapon damage are asserted. A's constrict after the lethal bite is asserted.

### 4.2 Mechanism (diagnostic `20261123T154102Z`, case A, harness `b95905fd…`)

In the crossing frame 4230:

1. the maintain ran: bite 8 (Damage 100007), constrict 8 (100015); `MaintainLink` returned `maintained:8` with the
   target **Conscious**;
2. **then** the native `UnitLifeController.TickOnUnit` on the target set **Dead** (Damage 100015, HP 100000, Con 10;
   dying allowance, regeneration and the death mark all false);
3. **then** the native `UnitReturnToConsciousController.MakeUnitConscious` set Damage to 99999 and Conscious (stack:
   `UnitReturnToConsciousController.Tick` → `MakeUnitConscious` → `UnitEntityData.set_Damage`), because the fixture's
   `DefaultPlayerCharacter` blueprint is player-faction and so not finally dead out of combat.

The relationship stayed active with a revived target. At the next round's crossing (frame 4410) the life tick set
Dead again; **no revive followed** (the reason was not established), and the hold released one frame later (4411)
through `SummonHoldComponent.Release`.

Sequence, per case: **first threshold crossing → transient native Dead → native recovery → later final death**,
then the cleanup checks in 4.3 against that later final death.

**Captured for the other four cases (same sessions):** the same pattern. The first crossing returned Damage to
HP − 1 within the crossing frame (no per-frame decrease was observable, and no heal rule), and the final death came
one frame after the second crossing. That cases B–E follow step 2's ordering is **source-inferred** from the shared
synchronous check (section 1) and the engine contract (section 2). Only case A has the direct life-tick trace.
Section 12 later traces the native life transition directly for all five cases.

### 4.3 What did hold (asserted after the final native death, each case)

- **Target:** no target part and no grappled buff.
- **Holder:** no initiator part, no hold buff, no CantAct or CantMove; alive and usable.
- **Live links:** none; no occupied limb. The raw stored records read 0 (captured).
- **Quiet:** 13.0 s of game time with no damage, maintain or constrict from the ended relationship, and no recreated
  hold.
- **Reacquisition:** a fresh living target held through 30 frames with no release or deferred callback.
- **Controls:** the witness rule held in `153430Z`. Controls that ended did so by exactly attributed ordinary failed
  maintains (`controlOutcomes`).

These are the native death path's cleanup, not the intended rider branches. They do not qualify A–E.

### 4.4 Unexplained observations (retained as observed)

- Why the second native death in each case was not revived out of combat.
- In `153430Z`, cases A and E rolled exactly the same damage in the revived round as in the first lethal round (A:
  8 and 7; E: 25). Captured only.

## 5. Defect classification (superseded by the review decision in section 11)

**Contract defect in the product (owning code: the five rider post-damage branches):**

- Each branch checks `IsDead` synchronously after its own damage rule. In the tested direct-damage scenarios the
  engine assigned Dead on the life controller's later tick (same frame, after the rider returned), so the branches
  **did not execute in those scenarios**. (Withdrawn: the earlier universal "unreachable" claim. The guards also handle
  destruction, and the observed direct-damage ordering does not establish every possible event-handler context.)
  In those scenarios:
  - a lethal maintain continues into further rider effects on a target past its native death threshold (A constricts
    after the lethal bite; D applies prone; C and B return as held);
  - a lethal Impale reports `heldKept=True` and schedules no owner cleanup.
- Cleanup then comes from the native death path one frame later. In these runs the end state was clean (4.3).
- **Severity, as observed:** a mismatch with the product's own documented contract and an extra rider effect on a
  dying target. No lasting residue, recreated hold or later damage was observed.
- Not every reason is product-side: the target's revive after its first death is native player-faction behavior, a
  **fixture** property of the `DefaultPlayerCharacter` target. It is not a product defect.

## 6. Repair options as returned (superseded: the review chose the native-timing contract, section 11)

Two narrow options; each needs the focused gate, one candidate and a requalification launch.

1. **Synchronize the native life state after each rider's damage** (recommended if the documented contract is
   wanted).
   - A small `UnitLifeController` subclass, following `TeleportationMishapDamageTarget`, runs the native
     `ShouldTickOnUnit`/`TickOnUnit` for the target right after the rider's damage rule.
   - The five existing checks then read the game's actual death state, including Ferocity, the dying allowance and
     regeneration.
   - **Risk to review:** the native death handling (`OnUnitDeath` events, view death, `LeaveCombat`, `InterruptAll`)
     then runs inside the holder's buff tick.
2. **Retire the branches that did not execute in the tested scenarios** and accept native next-frame cleanup as the
   contract. The post-lethal rider
   continuation (case A's constrict, case D's prone) is recorded as engine-boundary behavior.

**Fixture choice for any requalification.** A target whose death the engine keeps (finally dead) is needed:

- the player-faction `DefaultPlayerCharacter` is revived out of combat;
- a neutral faction is refused party damage (native `GameHelper.IsAttackingGreenNPC`);
- a hostile faction brings combat and party engagement into the fixture.

**Why it stopped here:**

- G1-R native time is 14 m 48 s over four sessions.
- The repair would add the gate, a Build-Local candidate (about 12 minutes on C3a's record) and a 5-case launch
  (about 4.5 minutes), passing the 30-minute ordinary ceiling.
- The repair itself is a design choice. Per the validation policy, the precise remaining check is reported instead of
  launched.

**Precise remaining check:**

1. Owner choice between option 1 and option 2, and of the target fixture.
2. Then the repair, the focused gate with production compilation, one identified `Build-Local -DevelopmentPurpose`
   candidate, and one `lethal` launch of all five cases on a finally-dead-capable target, asserting the intended
   branch (option 1) or the accepted native cleanup (option 2).

## 7. Sessions, failures and withdrawn hypotheses (all retained)

| Session | Harness | Duration | Result | Notes |
|---|---|---|---|---|
| `20261123T152231Z` | `4a29eded…c1fd` | 3 m 37 s | FAIL 5/5 | First baseline. The kill was then defined as the first HP crossing |
| `20261123T152834Z` | `b4ac2f10…9abd` | 4 m 30 s | FAIL 5/5 | Ferocity flags released (both already false); kill requires native Dead before further damage; heal and decrease traces |
| `20261123T153430Z` | `49265269…b1d7` | 4 m 41 s | FAIL 5/5 | **Clearest five-case baseline with documented fixture revival** (not an unconfounded single lethal terminal). Cheat Death grit search added (no Cheat Death component on the target) |
| `20261123T154102Z` | `b95905fd…83c0` | 2 m 00 s | FAIL (diagnostic, case A; also no intact control at its quiet check) | Damage-setter and life-tick observers: mechanism in 4.2 |

**Withdrawn hypotheses**, recorded because they were stated during the work:

- **Half-Orc Ferocity:** the target's Ferocity flags were false.
- **The product's Cheat Death deed:** the target has no Cheat Death component.

Neither caused the one-round delay; the native out-of-combat revive did.

**Retries:** in `153430Z`, A1 was an exactly attributed ordinary failed maintain in the ordinary rider round, retried
once. B, C, D and E needed no retry. Earlier sessions also retried exactly attributed ordinary failures (for example A1
and C1 in `152834Z`). Every attempt's trace is preserved in the evidence.

## 8. G1/G2 disposition after G1-R

| Item | Disposition |
|---|---|
| A Snake weapon-lethal (`MaintainLink` death return) | **Failed** as then defined: the synchronous branch did not execute in the tested scenario (4.1, 5) |
| B Snake/Lillend physical constrict post-damage release | **Failed** as then defined: did not execute in the tested scenario (Lillend inspected-shared, same block) |
| C Salamander post-damage release | **Failed** as then defined: did not execute in the tested scenario |
| D Death-roll release | **Failed** as then defined: did not execute in the tested scenario |
| E Impale lethal release and owner cleanup | **Failed** as then defined: did not execute in the tested scenario; the owner cleanup was not scheduled. The native path cleaned up |
| Native death-path cleanup after a rider kill | Executed in the five cases (4.3): clean end state, quiet, reacquisition |
| Earlier accepted G1/G2 scope (C1–C3c, G3, G4, G1/G2 Barbed/Crab) | Unchanged |

As returned, G1/G2's lethal-rider scope was not proposed for acceptance. **Superseded by section 12.**

**Retained limitations (not PASS):**

- the stale-callback replacement-hold race;
- successful native Dismissal;
- three or four simultaneous links;
- turn-based timing;
- the per-owner and independent-effect limits;
- G5's real-transition fixture block.

## 9. Commands

- **Harness build:** `scripts/compatibility/loader-compat/Build-LoaderCompatHarness.ps1 -SourceDir
  tools/LoaderCompatHarness -OutputDir <scratch> -LoaderReferenceDir <repo>/artifacts/references/loader`, then
  `Info.json` copied from `artifacts/loader-compat-harness-risk03` (staged under the ignored `artifacts/`).
- **Run:** `Invoke-LoaderCompatSession.ps1 -SessionFile …/ver02-g1r-lethal-riders.json` (or `ver02-g1r-diag-a.json`)
  `-LoaderProfile owner-0.33.0 -CandidateZip …/KingmakerGunslinger-0.0.193-development-ver02-c3a-swallow-lifecycle-9e902dfa8054.zip
  -HarnessDir …`.
- **Engine source reads:** `ilspycmd` on the game copy's `Assembly-CSharp.dll` (`RuleDealDamage`, `UnitState`,
  `CountableFlag`, `UnitPartFerocity`, `UnitReturnToConsciousController`); `UnitLifeController` from the earlier
  decompile.
- No product build, gate or suite: no product source changed.
- **Restoration:** every session's transaction completed, with game settings applied and owner files verified. No
  Kingmaker process was present before any run.

## 10. Recommendation as returned (superseded by sections 12.5–12.6)

Decide the repair option (section 6) and the requalification target fixture. Then run one bounded repair checkpoint
for the five paths. After G1-R is resolved, move to the original Sprint 6 checkpoint D (legacy/migration) while G5
stays separately blocked. Checkpoint D was not started.

## 11. Review decision and corrected characterization (2026-11-23)

The investigation is accepted as useful characterization. The review selected the contract; no policy question remains.

**Native life-update timing (the selected contract):**

- The accepted product and its defensive `IsDead`/`Destroyed` guards are preserved. No synchronous `UnitLifeController`
  update is added inside maintain/rider processing, and the guards are not deleted.
- The current synchronous maintain/rider may finish while native life-state settlement is pending.
- Once the engine establishes death, the exact relationship must clean up through the normal native/product lifecycle.
  There must be no subsequent damaging round from the ended relationship, no recreated hold, no loss of unrelated
  ownership and no stuck surviving holder.
- Additional constrict or prone after a damage threshold crossing but before native death is **retained behavior** for
  this reviewed RTWP scope. It is captured; it is not suppressed, repaired, or evidence of a synchronous release.
- A product repair is warranted only if the corrected scenario demonstrates a concrete failure of that contract.

**Corrections to sections 1–10 (applied in place):**

- The five synchronous release branches did not execute **in the tested scenarios**. The universal "unreachable" /
  "can never run" claims are withdrawn.
- A's native life-state ordering is directly traced; the explanation for B–E is source-inferred.
- The first threshold crossing, the transient native death, the recovery and the later final death are now
  distinguished. The cleanup checks (4.3) apply after that later final death.
- `153430Z` is the clearest five-case baseline with documented fixture revival, not an unconfounded single lethal
  terminal.
- Raw-record zero stays **captured** where the old harness only recorded it.
- The stale harness comment attributing the Damage reset to Cheat Death was removed with the fixture rewrite (it lived
  in harness commit `e54fbccf09`). The finding that no Cheat Death component was present is preserved, and is now
  asserted per target (section 12).
- No earlier FAIL is relabeled as PASS.

## 12. Native-timing qualification (correction checkpoint)

### 12.1 Fixture, oracle and deadline

- **Targets:** disposable native hostile NPCs, `CR7_BanditFighterRangedLevel8` (`02459ebf…`, faction Bandits, own
  blueprint faction and death policy), chosen by the deterministic hostile-Medium query used in C2a.
  - **Per spawned unit, asserted:** no Ferocity or Half-Orc Ferocity, regeneration, dying allowance, immortality or
    Cheat Death component; not a player-faction blueprint; an enemy of the main character.
  - The blueprint randomizes race per unit (Human, Elf, Half-Elf, Dwarf, Gnome, Halfling observed in the passing
    run). Half-orc instances carry Ferocity and were **rejected** (6 rejections, 26 admitted spawns). No survival flag or
    resource was released.
  - Ordinary RTWP combat followed. No difficulty, combat flag or death state was forced, and the life controller was
    not invoked.
- **Disclosed setup:**
  - target HP base 100000, then `Damage = HP − margin` before the decisive round. The margin is 1 for A/D/E and the
    establishing weapon's maximum + 1 for B/C (read-only `RuleCalculateWeaponStats`);
  - target escape stats −100; no XP on death; holder BAB 100; summon deadlines 600 s; placement within 10.5 m; two
    same-owner controls per case;
  - **brains:** `IsBrainActive` cleared for the main character and for every pre-existing living area NPC with an
    active brain, all restored at teardown. The fixture area holds native companions in their non-party "_Neutral"
    form (Valerie, Amiri, Harrim, Linzi, Jaethal, Kaessi) and prologue NPCs (Tartuccio, Kesten, Jamandi, Iosif, throne
    guards, ten start adventurers). These joined combat against the hostile targets (12.4). Their faction, combat and
    death state are untouched.
- **Oracle (read-only observers):**
  - a direct native life-transition trace (`UnitLifeController.SetLifeState`, with final-death status) and a recovery
    trace (`MakeUnitConscious`), for watched targets only;
  - native grapple-part removal (`OnRemove`, with a short stack);
  - completed begin/end damage-owner brackets on the exact pair;
  - an explicit observer drop counter: a truncated window cannot pass, and the passing session dropped 0;
  - an explicit postcondition after every wait, each recording game time, frames, wall time, re-pauses and combat state.
- **Retry rule:** a retry needs an exact ordinary failed maintain, or for B/C a demonstrated still-held expected
  execution. An unrelated attacker disqualifies a setup (it is identified, then a bounded fresh setup follows).
  Invalid, mixed or unexplained releases fail. Attempt records are completed before their single insertion.
- **Cleanup deadline, chosen before the run** from the native sequence (life tick → `UnitGrappleController` removes the
  initiator on a non-conscious target → hold-buff removal → `SummonHoldComponent.Release`): every grapple-owned element
  gone within **10 engine frames and 1.0 s of game time** after the native Dead transition. No activity from the ended
  relationship after native death.

### 12.2 Results (`20261123T162752Z-owner-0.33.0-ver02-g1r-native-lethal`, harness `cf5d90c2…8fc8d`, PASS 5 of 5; launch 223 s, session wall time 3 m 56 s)

| Case | Killing damage (asserted, completed bracket) | B/C survived weapon | Native Dead (asserted) | Synchronous continuation before Dead (captured) | Release → clean (asserted ≤ 10 frames / 1.0 s) |
|---|---|---|---|---|---|
| A Snake | bite 5 in `rider.weapon` (100036 ≥ HP 100032) | — | same frame 4236, final, no recovery | constrict 8 after the lethal bite; `maintained:5` returned | `SummonHoldComponent.Release` frame 4237; clean at 4238 (2 frames, 0.036 s) |
| B Snake | constrict 8 in `DealConstrict` (100037 ≥ 100032) | bite 6 → 100029 < 100032 | same frame 5040, final | `maintained:6` returned | Release 5041; clean 5042 (2 frames, 0.034 s) |
| C Salamander | constrict bundle 10 in `rider.salamanderConstrict` (100016 ≥ 100016) | tail 36 → 100006 < 100016 | same frame 5909, final | `maintained:36` returned | Release 5910; clean 5911 (2 frames, 0.031 s) |
| D Crocodile | death roll 14 in `rider.deathRoll` (100029 ≥ 100016); the rider asserted as death roll | — | same frame 7185, final | `death-roll:…` returned (hold kept, prone path) | Release 7186; clean 7187 (2 frames, 0.031 s) |
| E Barbed Devil | Impale 21 in `rider.impale` (100044 ≥ 100024) | — | same frame 8039, final | `impale:…dealt=21;targetDied=False;heldKept=True` returned | Release 8040; clean 8041 (2 frames, 0.031 s). Owner cleanup ran through the native death path; no `impaleCleanup` schedule or callback was needed |

**Asserted, per case:**

- a complete real hold before the decisive round: exact establishing limb, reciprocal parts, buffs, one raw and one
  live record, occupancy;
- living valid participants before the decisive round; the target survived the establishing attack and the ordinary
  rider round (first rider `maintained:` for A–C, `death-roll:` for D, `impale:` for E);
- no recovery, no later damaging round before death, and no activity from the ended relationship after death;
- after cleanup: no target part, grappled buff, initiator part, hold buff, live link or occupied limb; the surviving
  holder alive, conscious, without CantAct/CantMove;
- the control witness rule at the decisive checks and after the quiet;
- quiet: 13.0 s of game time (asserted ≥ 12 s) with no damage, maintain, constrict or recreated hold; the target still
  dead;
- reacquisition: a real attack held a fresh living hostile target through 30 engine frames, with no release or
  deferred callback touching it.

**Captured:**

- raw stored records 0 after cleanup;
- the per-wait game time and frames;
- controls that ended, each by an exactly attributed ordinary failed maintain (A1, B1, C1, D3, E1; at least one intact
  control at every check).

**Attempts:**

- A, B, C and E passed on attempt 1.
- D passed on attempt 3. D1 and D2 were exact ordinary failed maintains in the ordinary round: the crocodile rolled a
  natural 1 and the result was `released`. Each was retried with a fresh setup.

### 12.3 Evidence classes

- **Asserted:** as listed in 12.2.
- **Captured:**
  - the synchronous continuation between crossing and death;
  - raw records;
  - wait statistics;
  - the release path;
  - control outcomes;
  - hostile spawn races and rejections.
- **Source-inferred:** the cleanup ordering's controller names (life tick, `UnitGrappleController`, hold-buff removal).
  The part-removal stack directly traced this path in the failed `162428Z` run (12.4).
- **Historical:** the characterization round (sections 1–10); Sprint 35, Sprint 16 and the 0.0.146/0.0.169 rider
  evidence.

### 12.4 Correction sessions (all retained)

| Session | Harness | Duration | Result | Diagnosis (from that session's evidence) |
|---|---|---|---|---|
| `20261123T161325Z` | `7b326ff6…5ce1` | 3 m 02 s | FAIL | A1: no decisive maintain within a 30 s wait; about 4.3 s of game time passed (not attributed in that run). B1: no admissible hostile after a Ferocity (half-orc) instance was rejected, with no fallback spawn. Placement spots then exhausted because failed attempts did not retire their units. C, D and E met the native-timing checks but carry fixture problems, so they do not count |
| `20261123T161901Z` | `b8bc94e4…f7f9` | 3 m 42 s | FAIL | A1: the hold ended about 3.5 s after the health setting, with no maintain (no part-removal trace yet, so not attributed in that run). C1: a light-crossbow hit from `67c5e348` (identified in the next session as `Linzi_Neutral`) disqualified the attempt. B, D and E passed; C passed on retry |
| `20261123T162428Z` | `b0bf50d3…0c72` | 2 m 39 s | FAIL | Bystander attacks, each traced: Valerie (A), Amiri (B), Harrim (D) and Linzi (E) killed the targets before the decisive round. Native Dead followed, then `UnitGrappleController` removed the initiator part and `SummonHoldComponent.Release` removed the target part. C's three setups were disqualified by Harrim, by Amiri/Harrim/Linzi, and by Valerie |
| `20261123T162752Z` | `cf5d90c2…8fc8d` | 3 m 56 s session wall time (launch `01-nativeLethal` 223 s) | **PASS 5/5** | Area NPC brains cleared for the launch |

Correction native time: 13 m 19 s, within the 30-minute ceiling.

The product held the selected contract in every traced case, including the bystander kills in `162428Z`. **No product
defect was demonstrated; no product repair was made.**

### 12.5 Disposition

**Proposed: accept the selected representative G1/G2 lethal-terminal scope under native life-update timing.** The
five damage owners are A (weapon), B (physical constrict, shared with Lillend), C (Salamander constrict), D (death
roll) and E (Impale), on the accepted product `9e902dfa80`, in RTWP.

This does **not** claim that the original synchronous release branches were repaired or executed, or that they are
universally unnecessary.

**Retained, not PASS:**

- the stale-callback replacement-hold race;
- successful native Dismissal;
- three or four simultaneous links;
- turn-based timing;
- the per-owner and independent-effect limits;
- G5's transition-fixture block.

**Unchanged:**

- A1/A2/B and RISK-03's engine limits;
- G3 and G4;
- F2/F3 residue;
- the C2a unload limitations;
- REP-05/06's original-input blocks.

### 12.6 Commands, restoration, next

- **Commands:**
  - harness built with `Build-LoaderCompatHarness.ps1` (as in section 9);
  - run with `Invoke-LoaderCompatSession.ps1 -SessionFile …/ver02-g1r-native-lethal.json -LoaderProfile owner-0.33.0
    -CandidateZip …/KingmakerGunslinger-0.0.193-development-ver02-c3a-swallow-lifecycle-9e902dfa8054.zip -HarnessDir …`;
  - the package (`704cb0b4…`) and DLL (`431f13da…`) were re-read and matched at launch.
- **Not run:** no product build, gate, suite or save/load campaign.
- **Restoration:** every session's transaction completed, with game settings applied and owner files verified. The
  main character's and area NPCs' brain flags were restored at probe teardown. No Kingmaker process was present before
  any run.
- **Next:** after review, the planned next work is checkpoint D's legacy/migration stories, with G5 separately blocked.
  D was not started.

## 13. Review decision and corrections (2026-11-23)

**G1-R R1 is accepted** for its demonstrated representative RTWP lethal-terminal scope under native life-update
timing. The accepted outcomes are Snake weapon, Snake constrict, Salamander constrict, crocodile death roll and
Impale. No claim is made that the synchronous release branches were repaired or executed. Every earlier FAIL keeps its
diagnosis.

G5 stays fixture-blocked and VER-02 stays open. Every retained limitation stays explicit:

- the stale-callback race;
- successful Dismissal;
- three or four simultaneous links;
- turn-based timing;
- the per-owner and independent-effect limits.

None of these becomes a new mandatory grapple checkpoint.

**Corrections:**

1. **Cleanup deadline scope.**
   - The timed predicate (10 frames / 1.0 s after native Dead) covers only the native initiator part, the native
     target part, the hold buff and the held (grappled) buff. It was observed satisfied two frames after death.
   - The full live-link, occupancy and holder-condition checks (`after` in 12.2) run **after** the cleanup waits,
     about 0.3 s of game time later. They are asserted, but not against that measured deadline.
2. **Captured versus enforced.**
   - **Captured results:** death in exactly the crossing frame, and exactly two frames of cleanup latency.
   - **Enforced:** the causal ordering (crossing inside a completed bracket, then native Dead, with no recovery and no
     later damaging round between), finality, and the maximum deadline.
3. **Two separate scans.**
   - The **decisive-window scans** cover no recovery, no later damaging round before death, and no pair activity after
     death. They run over all observer events after the Dead transition, up to the end of the cleanup waits.
   - The **quiet-interval checks** are specific to the 13 s interval: no holder→target damage, no maintain or
     constrict for the pair, no live pair (`HeldSingle` false, no target part), and the target still dead at the end.
4. **Brain-suppression comment.** The harness's class comment said "the main character's IsBrainActive only". It now
   states the actual scope: the main character and every pre-existing living area NPC with an active brain, restored
   at teardown. Only the comment changed; the qualifying build `cf5d90c2…` was not rebuilt.
5. **Durations.** "223 s" is the runner's launch duration for `01-nativeLethal`; "3 m 56 s" is the session's wall time
   including staging and restoration. Both measurements keep their original identities.

This is a records-only update, with no product build or native rerun.
