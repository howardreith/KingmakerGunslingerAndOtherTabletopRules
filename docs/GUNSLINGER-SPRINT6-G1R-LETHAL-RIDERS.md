# Sprint 6 VER-02 checkpoint G1-R: lethal rider release (five selected paths)

Status: **not qualified; a product contract defect is identified and characterized, and the repair is held for an owner
decision** (2026-11-23). None of the five selected lethal-release paths runs on the accepted product. No product code
changed. VER-02 stays open. Nothing is merged or released.

- Branch `codex/s6-ver02-g1r-lethal-riders`, stacked on the G1/G2 acceptance-records head `13f80c22cd`
  (`codex/s6-ver02-g1g2-barbed-crab`, draft PR #211). The stack is not rebased.
- **Product (unchanged, reused):** accepted C3a `9e902dfa80`. Package `704cb0b4…79c9`, DLL `431f13da…5676`.
- **Harness:** commit source = the diagnostic build `b95905fd…`; earlier builds are listed in section 7. A `lethal` mode of `kmgBarbedCrab` (`tools/LoaderCompatHarness/LethalRiderProbes.cs`, a partial of
  `BarbedCrabProbe`), plus read-only observer additions in `HoldLifecycleProbes.cs`.
- **Sessions:** `scripts/compatibility/loader-compat/sessions/ver02-g1r-lethal-riders.json` (five cases in one launch)
  and `ver02-g1r-diag-a.json` (case A only, with diagnostic observers).
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

### 4.1 Five-case table (clean baseline `20261123T153430Z`, harness `49265269…`, and diagnostic `154102Z`)

| Case | Rider round before | Decisive maintain result | Crossing (asserted) | Intended release branch | Final native death and release |
|---|---|---|---|---|---|
| A Snake, weapon-lethal | `maintained:5` (attempt 2; attempt 1 retried after an exact ordinary failed maintain) | `maintained:8;limb=…Bite1d4` | the bite, inside `rider.weapon` (8 → Damage 100007 of HP 100000) | **did not run.** Constrict **executed** after the lethal bite (+7) | next crossing frame 5025 → Dead 5026 → `SummonHoldComponent.Release` 5026 |
| B Snake, constrict-lethal | `maintained:8` | `maintained:8;limb=…Bite1d4` | weapon 8 survived (99999), then **constrict** crossed, inside `DealConstrict` (7 → 100006) | **did not run** | 6013 → Dead 6014 → Release 6014 |
| C Salamander | `maintained:34` (attempt 1) | `maintained:38;limb=…Salamander_Tail` | tail 38 survived (99992), then the **Salamander constrict** bundle crossed, inside `rider.salamanderConstrict` (16 → 100008) | **did not run** | 7001 → Dead 7002 → Release 7002 |
| D Crocodile | `death-roll:` | `death-roll:…dealt=10;…proneAfter=True;heldKept=True` | the death roll, inside `rider.deathRoll` (10 → 100009) | **did not run** (`heldKept=True`, prone applied) | 8021 → Dead 8022 → Release 8022 |
| E Barbed Devil | `impale:` | `impale:3d8+9;…dealt=25;targetDied=False;heldKept=True` | the Impale, inside `rider.impale` (25 → 100024) | **did not run**; no `impaleCleanup.schedule` | 9066 → Dead 9067 → Release 9067 |

All five: **failed** on the accepted product for the intended branch. Damage owner identification (the bracket) and
the B/C survival of the weapon damage are asserted. The A constrict-after-lethal-bite is asserted.

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

**Captured for the other four cases (same sessions):** the same pattern. The first crossing returned Damage to
HP − 1 within the crossing frame (no per-frame decrease was observable, and no heal rule), and the final death came
one frame after the second crossing. That cases B–E follow step 2's ordering is **source-inferred** from the shared
synchronous check (section 1) and the engine contract (section 2). Only case A has the direct life-tick trace.

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

## 5. Defect classification

**Contract defect in the product (owning code: the five rider post-damage branches):**

- Each branch checks `IsDead` synchronously after its own damage rule, but the engine assigns Dead only on the life
  controller's later tick (same frame, after the rider returns). So the branches are unreachable on the accepted
  product:
  - a lethal maintain continues into further rider effects on a target past its native death threshold (A constricts
    after the lethal bite; D applies prone; C and B return as held);
  - a lethal Impale reports `heldKept=True` and schedules no owner cleanup.
- Cleanup then comes from the native death path one frame later. In these runs the end state was clean (4.3).
- **Severity, as observed:** a mismatch with the product's own documented contract and an extra rider effect on a
  dying target. No lasting residue, recreated hold or later damage was observed.
- Not every reason is product-side: the target's revive after its first death is native player-faction behavior, a
  **fixture** property of the `DefaultPlayerCharacter` target. It is not a product defect.

## 6. Repair: held for an owner decision (not built)

Two narrow options; each needs the focused gate, one candidate and a requalification launch.

1. **Synchronize the native life state after each rider's damage** (recommended if the documented contract is
   wanted).
   - A small `UnitLifeController` subclass, following `TeleportationMishapDamageTarget`, runs the native
     `ShouldTickOnUnit`/`TickOnUnit` for the target right after the rider's damage rule.
   - The five existing checks then read the game's actual death state, including Ferocity, the dying allowance and
     regeneration.
   - **Risk to review:** the native death handling (`OnUnitDeath` events, view death, `LeaveCombat`, `InterruptAll`)
     then runs inside the holder's buff tick.
2. **Retire the unreachable branches** and accept native next-frame cleanup as the contract. The post-lethal rider
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
| `20261123T153430Z` | `49265269…b1d7` | 4 m 41 s | FAIL 5/5 | **Clean matched baseline.** Cheat Death grit search added (no Cheat Death component on the target) |
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
| A Snake weapon-lethal (`MaintainLink` death return) | **Failed**, unreachable on `9e902dfa80` (4.1, 5) |
| B Snake/Lillend physical constrict post-damage release | **Failed**, unreachable (Lillend inspected-shared, same block) |
| C Salamander post-damage release | **Failed**, unreachable |
| D Death-roll release | **Failed**, unreachable |
| E Impale lethal release and owner cleanup | **Failed**, unreachable; the owner cleanup was not scheduled. The native path cleaned up |
| Native death-path cleanup after a rider kill | Executed in the five cases (4.3): clean end state, quiet, reacquisition |
| Earlier accepted G1/G2 scope (C1–C3c, G3, G4, G1/G2 Barbed/Crab) | Unchanged |

**G1/G2 is not proposed for acceptance of the representative lethal-rider scope** until the repair decision is
resolved.

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

## 10. Recommendation

Decide the repair option (section 6) and the requalification target fixture. Then run one bounded repair checkpoint
for the five paths. After G1-R is resolved, move to the original Sprint 6 checkpoint D (legacy/migration) while G5
stays separately blocked. Checkpoint D was not started.
