# Sprint 6 VER-02 checkpoint G1/G2: Barbed Devil Impale cleanup and Giant Crab two-pincer holds

Status: **qualified natively on the accepted C3a product with no product change; returned for manual review**
(2026-11-23), with a proposed G1/G2 disposition (section 7). VER-02 stays open. Nothing is merged or released.

- Branch `codex/s6-ver02-g1g2-barbed-crab` is stacked on the G4 acceptance-records head `69da3d490e`
  (`codex/s6-ver02-g4-stirge-attachment`, draft PR #210). The stack is not rebased.
- **Product (unchanged, reused):** accepted C3a `9e902dfa80`. Package `704cb0b4…79c9`, DLL `431f13da…5676` (both
  re-read by the runner at every launch).
- **Harness:** `tools/LoaderCompatHarness/BarbedCrabProbes.cs` (step `kmgBarbedCrab`, modes `barbed`, `crab`, `save`,
  `reload`), plus read-only additions to `HoldLifecycleObserver`; commit `2789212f82`. Builds: `d6fb0bc6…08df`
  (session 1), `2ac26f41…6434` (session 2), `f5e75d70…7e1e` (session 3, the qualifying run; its source is the commit).
- **Session file:** `scripts/compatibility/loader-compat/sessions/ver02-g1g2-barbed-crab.json` (four launches).
- **Fixture:** `KMCS_AFTER_NORMAL_012_MODONLY.zks`; loader profile `owner-0.33.0`; product alone; RTWP.
- **Kept:** all earlier acceptance (A1/A2/B, C1–C3c, G3 and G4 for their scopes); RISK-03's limits; the REP-05/06
  blocks; G5 (blocked on the admitted real-transition fixture); every cross-cutting limit (turn-based timing among them).
  G3/G4 acceptance qualifies none of G5, turn-based timing or the other retained limitations.

## 1. Owner map (source, current product)

`Summoning/ExpandedSummoningSpecialCombatComponents.cs`, `Blueprints/ExpandedSummoningSpecialBuilder.Infernal35.cs`,
`Blueprints/ExpandedSummoningSpecialBuilder.cs` (`ConfigureGrabber`), `Summoning/InfernalRules35Policy.cs`.

**Barbed Devil (single-link, native parts).**

| Element | Owner | Behavior |
|---|---|---|
| Grab | `SummonGrabComponent` on the **feature** `KMG_Summoning_Infernal_BarbedDevil_Traits` (no buff carries it; `Find` falls through to active features) | Primary claw plus one additional claw grab; `MaxHeldTargets = 1`; Medium or smaller |
| Hold / held state | native `UnitPartGrappleInitiator` (hold buff `KMG_Summoning_Special_Grapple_Hold`, `SummonHoldComponent`) and `UnitPartGrappleTarget` (`…_Grappled`) | the shared single-link owner (C1) |
| Rider | `ImpaleProfileOwner = unit` → `MaintainLink` takes the exclusive `impale` branch | from the first held round, one successful maintain deals direct `3d8 + 1½×Str` piercing (no attack, no weapon) instead of claw damage; never on the grab; once per held round (`m_LastRiderRound`, serialized) |
| Post-damage branch | `DealImpale` | if the Impale kills the target, `ReleaseLink(…, true)` |
| Deferred owner cleanup | `ReleaseLink` (single) → `ScheduleReleasedImpaleOwnerCleanup` | schedules `Game.ScheduleAction` only when the owner is the exact Impale owner, its initiator still names the target, and the target no longer names it back; the callback re-checks the same three facts, then removes only that owner's initiator part, hold buff and record |
| Fear | `BarbedClawFear35` (claw, DC 20 Will, `…_FearFrightened`) | a separate effect; not grapple state |

**Giant Crab (multi-link).** `ConfigureGrabber(GiantCrab…)`: primary + 1 additional claw (`Claw1d4`, limbs `1|-1` and
`2|0`), `MaxHeldTargets = 2`, `MultiHold` / `MultiHeld`, `CrabProfileOwner = unit`. The shared multi-link owner (C2)
holds; `DealConstrict` adds `1d4 + live Str` bludgeoning with **the pincer that established that target's hold**
(`SummonGrappleLinks.EstablishingWeapon`), both on the grab (`TryGrab`) and on each successful maintain. The Crab
branch of `DealConstrict` releases nothing itself; a dead target is released at the multi-hold's next round.

**Common with Xill and Giant Scorpion:** `SummonMultiHoldComponent`, `SummonHeldComponent`, multi-link `ReleaseLink`,
the record store and the load reset. Differences are configuration only: Xill four additional claws / four links, no
hold rider (bite paralysis is a separate effect); Scorpion primary + 1 additional claws / two links, no hold rider (the
sting never grabs; its poison is a separate effect).

## 2. Method and disclosed setup

- Every hold came from the engine's `RuleAttackWithWeapon` with the creature's exact grab limb, reaching the product's
  grab handler and its native grapple maneuver. Maintains, Impale, constrict, releases and the deferred cleanup ran
  only from the game's round ticks and before-tick scheduler. Nothing called `ReleaseLink`, removed a native part,
  invoked the scheduled callback, swapped parts, delayed the scheduler or forced a die.
- **Disclosed setup:**
  - holder BAB 100 (hits, grabs and maintains succeed except on a natural 1; holder CMD high);
  - targets: HP 100000, AdditionalCMB / Mobility / Athletics −100 (no break-free);
  - summon deadlines extended to 600 s through the native `SummonedUnitBuff` deadline, except where native expiry is
    the terminal (deadline moved to now, native `UpdateNextEvent`/`Tick`);
  - appear buffs removed; brains off; units woken; navmesh placement; pauses for observation;
  - **failed maintain only:** the target's AdditionalCMD base 0 → 1000 until the release (the Sprint 35 pattern; the
    natural d20 is kept, so a natural 20 would still succeed), restored immediately (asserted);
  - **Dismissal only:** the native Dismissal fact granted to the main character if absent (it was), cast parameters
    CL 40 / spell level 4, and the Barbed Devil's Will base −100, natural rolls kept. The cast was legally refused
    before any of this mattered (section 3.4);
  - pairs placed within 10.5 m of the summoner and at least 5.5 m apart (session 3; see section 5).
- **Controls:** two fresh independent same-owner controls (Barbed or Crab) per terminal, each with an unrelated Bull's
  Strength. A terminal needs at least one control intact; a control that ends must end by an exactly attributed
  ordinary event (an exact `released` maintain with this holder's failed maneuver in the window, or a native
  break-free with a passed check), and is then no longer a witness. Unrelated effects on every control target must be
  kept. Controls are disposed after their last check (not a terminal under test).
- **Retry rule (correcting the C3c helper limits):** a window is ordinary only if every release in it is an exact
  `released` maintain result for that holder/target with a failed maneuver by that holder in the same window; any
  invalid-state, unattributed or mixed window is a failure. Attempt rows are completed before insertion into a single
  container. Budget: three attempts per case.
- **Instrumentation (read-only):** frame numbers; native part state at every `Release`/`ReleaseLink`; the cleanup
  scheduling and its callback (begin/end); per-link `DealConstrict` (limb, damage delta); multi-hold and held-state
  round callbacks; a rule trace of grapple maneuvers, damage, saves, skill checks and spell resistance.

## 3. Barbed Devil (`20261123T144444Z-owner-0.33.0-ver02-g1g2-barbed-crab`, launch 1 PASS, 114 s)

### 3.1 Owner and establishment

- **Asserted:** the grab is carried by exactly one feature (`KMG_Summoning_Infernal_BarbedDevil_Traits`) and no buff;
  `ImpaleProfileOwner` is the holder's blueprint; `MaxHeldTargets` 1; grab limbs `…_Claw@1|-1` and `@2|0`.
- **Asserted at each establishment:** reciprocal native parts; `HeldTarget` names the target; exactly one hold buff and
  one grappled buff; establishing limb = primary claw (`1|-1`); exactly one raw and one live record; the primary claw
  occupied; **no maintain rider on the grab**.
- **Captured:** the holder carries the native `SubtypeExtraplanar` (`136fa034…`) and `KMG_Summoning_Subtype_Extraplanar`.

### 3.2 A later successful maintain with Impale

- **Attempt 1 (retained):** the first decisive maintain rolled a **natural 1** (failed), the result was exactly
  `released`, and the deferred cleanup completed in the next frame. Classified ordinary; one retry.
- **Attempt 2, asserted:** one successful holder/target maneuver (natural 20), then
  `impale:3d8+9;liveStrengthModifier=6;dealt=23;targetDied=False;heldKept=True` (+9 = 1½ × live +6); exactly one
  damage rule in the window, with no attack roll and no weapon (no replayed claw); the hold still established as in 3.1.

### 3.3 A real failed maintain and the deferred owner cleanup

Two fresh Barbed controls. Target AdditionalCMD raised to 1000 (disclosed); the next decisive round was the release.

- **Asserted:**
  - exactly one holder/target grapple maneuver in the round: natural **12**, CMB 115 against CMD **1010**, failed;
  - the maintain result is exactly `released`;
  - no Impale after the failed check and no holder→target damage after it (through the cleanup and a 7.03 s quiet);
  - a `ReleaseLink` for the exact pair, and `impaleCleanup.schedule` for it;
  - the scheduled callback ran **in a later frame** (4450, against the release frame 4449) through the game loop, and
    at its end the owner had no initiator part, no hold buff, and the target no grappled part;
  - afterwards: target no part and no grappled buff; holder no initiator part, no hold buff, no raw or live record,
    `HeldCount` 0, both claws unoccupied, no CantAct/CantMove;
  - the target's AdditionalCMD restored; its unrelated effect kept; the controls' witness rule held.
- **Captured path:** at the `ReleaseLink` call the target part was already gone while the owner **still had** its
  initiator part and hold buff (the retention the deferred cleanup exists for); the callback found that state and
  removed it.
- **Fear kept separate:** after the release the target carried `…_FearFrightened` and the native
  `FrightenedSystemCondition`. They were not cleared or treated as residue; the target could move and act.
- **Guard analysis (source evidence only):** the callback's exact-owner, exact-initiator-target and non-reciprocal
  checks make it a no-op once anything re-establishes or replaces the relationship. No stale-callback race was
  constructed. One natural instance of a no-op callback was captured at load (3.6).

### 3.4 Reacquisition

- A real primary-claw attack on a fresh adjacent living target held at the first attack. **Asserted:** the full 3.1
  contract, then again after 61 engine frames and ≥ 2 s of game time, with no `ReleaseLink` or cleanup callback for
  the new pair. The old initiator, hold and occupancy did not prevent the grab.
- **Captured:** the observer records a `DealConstrict` call from `TryGrab` with 0 dice and no damage. The Barbed Devil
  has no constrict; it is a no-op.

### 3.5 Owner terminal: Dismissal legally rejected; native expiry used

- **Dismissal (native `95f7cdce…`), not PASS:** `AbilityData.CanTarget` is false for the summoned Barbed Devil. The
  extraplanar checker passes (`AbilityTargetHasFact` = true, `SubtypeExtraplanar`); the rejection is the ability's
  targeting: `CanTargetEnemies` true, `CanTargetFriends` **false**, and the main character is an **ally** of its own
  summon. No targeting override, faction change or forced success was used, and no cast was issued.
- **Fallback: native summon expiry**, with the hold asserted active immediately before. **Release 1.82 s** of game time
  later (holder destroyed). **Asserted:** the target's owned state clear; no relationship recreated; over 13.03 s no
  damage to the target and no pair activity; its unrelated effect kept; controls: one intact, the other ended by an
  exactly attributed ordinary failed maintain (natural 1) during the quiet.
- **Captured path:** the hold's `SummonHoldComponent.Release` with the holder's parts still present at the call.

## 4. Giant Crab (same session, launch 2 PASS, 99 s; attempt 1 of 1)

### 4.1 Two targets, two pincers

- **Asserted per link:** exactly one held state naming the Crab; one raw record on its own limb (`1|-1` for target
  A, `2|0` for target B); the limb occupied by that target only (distinct occupants); establishing limb = that limb;
  Entangled + CantMove; no native parts; one multi-hold buff; the Crab without CantAct/CantMove.
- **Captured on the grab:** each pincer's grab constricted once on its own limb (`1|-1` +4, `2|0` +4).

### 4.2 Per-link maintain and constrict

- **Asserted (first round):** link A `maintained:3;limb=Claw1d4` and constrict on `1|-1` (+4); link B
  `maintained:6;limb=Claw1d4` and constrict on `2|0` (+6); no `substituted` limb; no Crab damage or constrict on any
  unit that is not a live link.

### 4.3 One link ends by target death; the sibling continues

Two fresh Crab controls.

- `GameHelper.KillUnit(A)`: released **0.053 s** later.
- **Asserted:** A has no held state, no live answer, `1|-1` unoccupied; B still held by the same Crab on `2|0`, with
  its record, occupancy and conditions; B's unrelated effect kept; controls intact.
- **Asserted, sibling round:** B `maintained:4` and constrict on `2|0` (+6); **no maintain or constrict attributed to
  A** after its death.
- **Captured (accepted F2):** the surviving Crab kept A's **raw** record (1) after the death; live membership and
  occupancy were correct. Not repaired.

### 4.4 Reuse of the freed pincer

- A real primary-pincer attack on a replacement target held at once. **Asserted:** the replacement held on `1|-1`
  with its own record; B still held on `2|0`.
- **Captured:** after the reuse, A's raw record was gone (0): the record store's put for the same limb replaced it.

### 4.5 Holder-wide terminal: native expiry with both links active

- **Asserted immediately before:** B on `2|0` and the replacement on `1|-1`, both full contracts.
- Native expiry (deadline moved to now): released **1.81 s** later; the Crab destroyed.
- **Asserted:** both targets free (no held state, Entangled, CantMove or CantAct); unrelated effects kept; over the
  quiet (14.8 s from the expiry) no damage to either and no maintain or constrict from the Crab; controls: one intact,
  one ended by an exactly attributed ordinary failed maintain.
- **Captured path:** `hold.OnTurnOff` → `ReleaseLink` (B) → `ReleaseLink` (replacement) → `hold.OnFactDeactivate`.

## 5. Shared save/load envelope

### 5.1 Save (launch 3 PASS, 85 s)

- **Setup, asserted:** an active Barbed hold after one Impale round (`dealt=23`) and a two-link Crab after one
  maintain/constrict round per pincer; all three windows ordinary-held.
- **Native admission:** allowed. **Save, asserted:** callback received, no commit or loading in progress (finished).
  The unchanged paused clock is **captured**.
- **Asserted unchanged across the save** (one snapshot before, one after, compared exactly):
  - Barbed: reciprocal native parts; hold and grappled buff instances and clocks (round 1, same next tick);
    `m_LastRiderRound` 1; establishing limb `1|-1`; one raw and one live record; claw occupancy; target damage 90;
  - Crab: one multi-hold instance (round 1, same next tick); held-state instances naming the Crab, each round 1 with
    its own next tick; both raw records and both occupancies; target damages 18 and 16;
  - the buff-name sets.
- **Asserted during the save:** no maintain, rider, damage or release. **Captured:** both holds received
  `OnTurnOff` with the owner off and no area unload, then `OnTurnOn` (the C2a policy).
- **Captured:** the native condition buff `EntangledBuff` on the Crab targets moved after Bull's Strength in buff order
  (re-applied by the owner off/on cycle). See the session 1 failure (section 6).
- **Normal continuation, asserted:** in the first round after the save each link had exactly **one** decisive
  maintain, all held: Barbed `impale:…dealt=27` with exactly one Impale damage; each Crab link `maintained:3` with
  exactly one constrict; no release.

### 5.2 Written file (manual read-only inspection)

`Manual_1_ver02g12.zks`, SHA-256 `e35fd2d3a838188c3ed22421686cec1b9ec173ed44b6dadf4a2494687c21dd83`, in the session
profile. It contains **complete active relationships**:

- `KMG_G12_Barbed`: `UnitPartGrappleInitiator` naming the target, its hold buff (`5af4e99c…`, `Grapple_Hold`) with
  component data `m_LastRiderRound: 1`, `TickTime` 6 s; `UnitPartSummonGrappleLinks._links` =
  `[target|1|-1|KMG_Summoning_Infernal_BarbedDevil_Claw|0|0]`;
- `KMG_G12_BarbedTarget`: `UnitPartGrappleTarget` naming the Barbed Devil with the grappled buff (`e49fc099…`); the
  Barbed fear buff (`a33a7c9b…`) and Bull's Strength;
- `KMG_G12_Crab`: the multi-hold buff (`a6ff9b2a…`, round 1); `_links` with both targets on `1|-1` and `2|0`;
- `KMG_G12_CrabTargetA` / `B`: each one held-state buff (`c6a4dd2e…`, `MultiHeld`, round 1) whose caster is the Crab,
  and Bull's Strength.

### 5.3 Fresh-process load (launch 4 PASS, 89 s)

- **Asserted:** the five named participants found.
- **Asserted at load and after 13.03 s:** no Barbed initiator or target part, hold or grappled buff, raw (`[]`) or live
  record, or occupied claw; the Barbed Devil without grapple conditions; the Crab without multi-hold, with `_links`
  `[]`, no live link and both pincers unoccupied; the Crab targets without held states, Entangled, CantMove, CantAct
  or native parts; no maintain, rider or holder damage during the quiet; the unrelated effects kept.
- **Captured load path** (loading in progress): `Game.OnAreaLoaded` → `ResetLoadedGrapples` → Barbed `Release`,
  `impaleCleanup.schedule`, `ReleaseLink`, hold `OnTurnOff`/`OnFactDeactivate`; Crab `ReleaseLink` ×2, hold
  `OnTurnOff`/`OnFactDeactivate`. The scheduled Barbed callback then ran after loading and found **no** owner state
  (the reset had removed it): a natural no-op of the exact-owner guard.
- **Captured, independent effects:** the Barbed fear on its target survived the load under its own lifetime.
- No active-hold restoration, new save format or relationship rebuild was introduced.

## 6. Failures and their dispositions (retained)

| Session | Harness | Launch | Cause and disposition |
|---|---|---|---|
| `20261123T142611Z` (7 m 32 s) | `d6fb0bc6…` | barbed PASS 145 s | Dismissal rejected; the reason was not yet recorded (added for session 2) |
| | | crab **FAIL** 109 s | A control ended during the final quiet with an **unattributed** release; the harness had not captured the round callbacks to attribute it. **Harness observation gap.** |
| | | save **FAIL** 88 s | **Oracle defect:** the snapshot compared buff-name lists in order; the only difference was the native `EntangledBuff` moving after Bull's Strength (re-applied by the save's owner off/on). Every owned instance, clock and record matched. Corrected to compare sets and capture order. |
| | | reload PASS 82 s | — |
| `20261123T143658Z` (6 m 30 s) | `2ac26f41…` | barbed PASS 106 s, save PASS 85 s, reload PASS 87 s | — |
| | | crab **FAIL** 97 s | Now attributed: a control Crab was walked away from its held target by the **native summon-follow controller** (out of combat, summoner > 11.7 m; a multi-link holder has no CantMove by design). The multi-hold's own round released the link: `conscious=True; reach=False; distance=12.7 m`. The other control ended by an ordinary natural-1 maintain, so no control was intact. **Fixture placement defect**, corrected (all pairs within 10.5 m). |
| `20261123T144444Z` (6 m 41 s) | `f5e75d70…` | **PASS 4 of 4** (114 s, 99 s, 85 s, 89 s) | Qualifying session |

- **Captured from session 2 (not a qualification):** the multi-hold's out-of-reach release branch ran natively on a
  real, conscious, held target. C2 listed that branch as unexercised; this is one natural instance, recorded only.
- Every transaction completed: game settings applied, owner files verified, the installation restored by the runner.
  No Kingmaker process was present before any session.

## 7. Proposed G1/G2 disposition

Execution classes: **D** = direct execution on the accepted product `9e902dfa80`; **S** = inspected shared
implementation; **H** = historical evidence on a named earlier build; **L** = retained limitation; **F** = concrete
distinct follow-up.

| Mechanism | Evidence | Disposition |
|---|---|---|
| Common single-link cleanup (native parts, `SummonHoldComponent`, holder/target death, expiry, save suspension, load reset) | **D** C1 (crocodile), C2a R1, C3b; this checkpoint (Barbed failed maintain, expiry, save, load) | Accepted scope; residue F3 stays accepted |
| Common multi-link cleanup (`SummonMultiHoldComponent`, `SummonHeldComponent`, multi `ReleaseLink`, record store, load reset) | **D** C2 (Flytrap), C2a R1, C3c; this checkpoint (Crab target death, reuse, expiry, save, load) | Accepted scope; residue F2 stays accepted (re-observed 4.3) |
| Impale: feature carrier, exclusive rider, deferred exact-owner cleanup | **D** this checkpoint (3.1–3.5, 5.1–5.3); **H** Sprint 35 runtime phase (release 0.0.173) | Qualified for the executed RTWP scope. **L:** the stale-callback race is source analysis only; the Impale-kills-target branch (`DealImpale` → `ReleaseLink`) is unexecuted |
| Crab per-link constrict (establishing pincer, per target) | **D** this checkpoint (4.1–4.5, 5.1); **H** Sprint 21 profile review (0.0.150) and the product's Cleanup2 creature-command constrict observation (its build not re-identified here) | Qualified for two links on the executed RTWP scope |
| Xill configuration (4 additional claws, 4 links, no hold rider; paralysis separate) | **S** same multi handlers, limb classification by additional index (executed for additional index 0, `2|0`, here and in C2; higher indices are configuration only); **H** Sprint 19 (0.0.148) | No distinct follow-up. **L:** three or four simultaneous links unexecuted (C2 limit) |
| Giant Scorpion configuration (primary + 1 claw, 2 links, sting excluded by limb count; poison separate) | **S** the same two-limb shape as the Crab without a rider; **H** Sprint 20 (0.0.149) | No distinct follow-up; sting exclusion is a grab-eligibility check (historical) |
| Crocodilian death roll (non-lethal) | **D** captured in C3a R1 `012518Z` and C3b `015820Z` (`death-roll:` results) on this product | Executed incidentally; no separate checkpoint |
| Crocodilian death roll **lethal** branch (`DealDeathRoll` → target dead → `ReleaseLink(…, true)`, `targetDied=True`) | **S** only; never observed in VER-02 evidence | **F** (below) |
| Constrictor Snake / Lillend live constrict (`DealConstrict` post-damage `ReleaseLink`; `MaintainLink` live-constrict weapon-damage death branch) | **S**; **H** Snake 0.0.146, Lillend 0.0.169; no VER-02 execution | **F** (below) |
| Salamander live constrict (`SalamanderConstrictDamage` + post-damage `ReleaseLink`) | **S**; **H** 0.0.146 | **F** (below) |
| Save under the session policy, and the load reset | **D** C2a R1 (single/multi), C3a–C3c, G4, this checkpoint (complete serialized single + two-link relationships, reset at load) | Accepted scope; D2 ordinary-play reachability stays unresolved |
| Independent effects (Barbed fear, Xill paralysis, Scorpion poison) | **D** Barbed fear observed surviving release and load, not treated as residue; **S** paralysis and poison are separate owners | Kept separate from hold residue |
| Turn-based timing, G5 leave/unload | — | **L** (cross-cutting; G5 blocked on the admitted real-transition fixture) |

**What remains after this checkpoint, and why.** Impale and the Crab do **not** qualify every G1 rider. Each live
rider has its own **post-damage release branch** that runs inside the rider when the rider's own damage kills the held
target. That is a different path from an external target death, which C1, C2 and this checkpoint qualified. No run has
executed it for any rider. Proposed follow-up **G1-R (lethal rider release)**, bounded and one launch:

1. a Constrictor Snake whose live constrict kills its held target (the `DealConstrict` release; Lillend shares that
   block behind its own gate and stays inspected-shared);
2. a Salamander whose constrict kills its held target (its separate damage owner);
3. a crocodile whose death roll kills its held target (`targetDied=True`).

Each would assert release from inside the rider, no further rider, and a clean surviving holder. The Impale lethal
branch could ride along as a fourth row, or stay a limit. No other G1/G2 checkpoint is proposed. Turn-based timing and
G5 stay cross-cutting limits.

## 8. Commands and durations

- **Harness build** (each session): `scripts/compatibility/loader-compat/Build-LoaderCompatHarness.ps1 -SourceDir
  tools/LoaderCompatHarness -OutputDir <scratch> -LoaderReferenceDir <repo>/artifacts/references/loader`, then
  `Info.json` copied from `artifacts/loader-compat-harness-risk03` (staged under the ignored `artifacts/`).
- **Run:** `Invoke-LoaderCompatSession.ps1 -SessionFile …/ver02-g1g2-barbed-crab.json -LoaderProfile owner-0.33.0
  -CandidateZip …/KingmakerGunslinger-0.0.193-development-ver02-c3a-swallow-lifecycle-9e902dfa8054.zip -HarnessDir …`.
- **Durations:** 7 m 32 s, 6 m 30 s and 6 m 41 s (20 m 43 s in total, inside the 30-minute ordinary ceiling; over the
  15-minute planning target because of two corrected harness runs).
- No product build, focused gate or broad suite: no product source changed. Evidence lives under
  `C:\Dev\KingmakerGunslingerLab\runtime-evidence\loader-compat\` and is not committed.

## 9. Recommendation

**Accept this checkpoint for its demonstrated development scope.** On the accepted product:

- the Barbed Devil's feature-carried grab holds through real claws;
- a later successful maintain resolves exactly one Impale;
- a real failed maintain releases without Impale, and the scheduled exact-owner cleanup completes through the game loop
  in the next frame, so a fresh claw grab holds;
- native expiry ends the hold cleanly;
- the Giant Crab keeps two independent pincer links with per-link constrict, ends one without disturbing the other,
  reuses the freed pincer exactly, and releases both at holder expiry;
- one native save preserved both relationships unchanged with a normal continuation, and a fresh load reset them
  without residue.

**No product defect was demonstrated.** Dismissal is recorded as legally inapplicable to the caster's own summons
(it does not target allies), not as PASS.

**VER-02 stays open** for: the proposed G1-R lethal-rider follow-up; G5 (fixture-blocked); turn-based timing; and the
retained per-mechanism limits.
