# Firearm RTWP continuation after a kill — released in v0.0.156

**Released.** The owner accepted this checkpoint and it shipped as
[v0.0.156](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/tag/v0.0.156)
(PR #50, merge `607da683b1`; release candidate gates and public asset hashes
are in `AUTONOMOUS-RESUME.md`). The release candidate repeated the guarded
native scenario at 0.0.156: PASS 491/491 (run `20261016T0411002291236Z`). The
statuses below are unchanged by release: the original within-full-attack
symptom is still unreproduced and the owner's live RTWP check is still not
performed.

The checkpoint history follows. Branch `codex/firearm-rtwp-kill-retarget`
started from `origin/master` `fd6c56ebd8` (v0.0.154 record); master
`31672515529c` (v0.0.155) was merged in at `1d98af476b` and released master
`97c881ea84` at `ff8b5b8ade`. The production correction is `1890ba06cc`; the
second review changed only the CR-04 fixture and observer (`f463ef6d56`,
`acc620cb4c`). The checkpoint's tested code commit is `acc620cb4c`;
`1620870007` is the version bump. The checkpoint itself was an ordinary focused
checkpoint under [VALIDATION-POLICY.md](VALIDATION-POLICY.md); release
qualification is the Sprint-level gate and publisher run recorded in
`AUTONOMOUS-RESUME.md`, not a compatibility or Tranche qualification.

## Status

These are separate results:

- **Next-cycle stall: corrected**, with the evidence stated under "Evidence"
  and "Validation and limits". After an explicit full attack's last target
  died, the pistol never fired again without a new click. It now continues
  natively, including after Broken → Quick Clear → Normal (the first
  review's blocking case) and when the target dies in the reload gap.
- **Original report (later attacks of the same full attack stop after the
  first kill): not reproduced** in the save-free fixture. Loaded, empty and
  paused four-attack full attacks continue A→B→C→D natively. The fixture
  supplies animation act/finish timing, so this does not exclude a live
  animation/timing cause or a mod-stack cause.
- **Owner RTWP check with live animation playback: not performed.**

## Report and installed identity

Owner report: a stationary RTWP Gunslinger (pistol, matching Rapid Reload, paper
cartridges, auto-use Reload, four planned attacks, several enemies within about
20 ft) kills A with the first shot and then stops, while ordinary weapons go on.

The lab's live install is the 0.0.140 favored-class build: `Info.json` 0.0.140,
DLL SHA-256 `00ef67570d30e4ff9c54a2ad2027393660436d5724bcee2bb99e4c5718e966fd`,
MVID `842d73db-a49f-459f-9b47-ca80cf0f273a`, embedded commit `fff3b778f0`.
What was established about it:

- No relevant source difference: its `Firing`, `Reloading`, `Misfires`,
  `Actions`, `Deeds` and `Scatter` sources equal master `fd6c56ebd8`, where
  the original fixture runs (1–4) took place.
- The original in-full-attack symptom was not reproduced in the save-free
  fixture, on that source or on this branch.
- The fixture supplies animation act/finish timing and plays no clips, so it
  does not exclude a live animation/timing cause or a mod-stack cause on the
  owner's machine. This checkpoint does not claim the installed build is
  correct in live play.

## Native contract (installed 2.1.7b, Assembly-CSharp `3b6450ff…`)

- `UnitAttack.OnTick` calls `UpdateTarget` every tick and before each later
  attack. A dead current target is replaced by the nearest conscious remembered
  enemy within weapon range + 5 ft (pistol: 20 ft increment); with none, the
  command is interrupted. `OnAction` skips a dead target without advancing.
- A Free reload command has no animation; it acts and ends in one
  `UnitActionController` tick, and the ability executor, registered later in
  the same frame, applies the reload on its first tick. Scheduled callbacks run
  before the next frame's AI. AI never preempts a started player command.
- With animations finished, a completed full attack ends `Success`.
- Native AI decides at most once per 0.3–0.4 s of Unity time
  (`AiBrainController`: `NextCommandTime = Time.time + 0.3 + Random(0, 0.1)`),
  not once per frame.
- Melee `RuleAttackWithWeapon` deals its damage inside the attack rule;
  ranged attacks launch projectiles whose damage lands later.
- `UnitEntityData.CanAttack(unit)` is a faction test only:
  `unit.Descriptor.Faction.Neutral || Group.IsEnemy(unit)`. It does not consider
  range, line of sight or temporary untargetability.

## Evidence (guarded save-free `disposable-firearm-break-interruption`)

CR-04 lanes (`RuntimeTestRunner.FirearmReviewKillChain.cs`) click A through
native `ClickUnitHandler.OnClick` with BAB 16 (four native planned attacks),
Gun Training, paper cartridges and auto-use Reload. Native `UpdateTarget`
chooses every replacement after real projectile damage and native death. The
disclosed roll control is natural 10 on firearm attack rolls (hit, no misfire);
Broken lanes use a real natural-1 misfire first. A fixture-only observer records
order cancellations, command endings, target transitions, `MayExecute` and
submission rejections and the moment a scheduled reload resume executes, with
call stacks; it changes nothing. Automatic-continuation windows wait for the
first automatic command to finish, or, when none fires, for at least a frame
minimum and two seconds of Unity time (several native AI decisions).

| Run | Source / DLL | Result |
| --- | --- | --- |
| 1 `20261015T2108007138654Z` | `e98a9402` / `7fedc6cf…` | Loaded lanes: four attacks A→B→C→D from one command. Empty lanes: order lost to a fixture ordering artifact (AI ticked between reload end and same-frame delivery); every sequence ended `Interrupt` because the host never finishes animations. |
| 2 `20261015T2127116112635Z` | `b5f48fee` / `c76925a3…` | Native frame order + finish cues. Primary lanes correct; **final-kill next cycle stalls** (baseline reproduction). |
| 3 `20261015T2134449241861Z` | `9336cac6` / `f120026f…` | First correction: 55 CR-04 + 124 existing PASS, then aborted by an external read of the evidence file (sharing violation). |
| 4 `20261015T2137515866390Z` | `9336cac6`, same installed artifact | PASS 468/468. Superseded by review: it had no repaired-Normal lane. |
| 5 `20261015T2303585727403Z` | `1890ba06cc` (integrated v0.0.155) / `67c492d2…` | PASS 487/487: 74 CR-04 + all 413 existing firearm native checks. Its reload-gap lane was injected-state and its longbow retarget assertion was unsound (see below). Superseded by run 7. |
| 6 `20261016T0242514282821Z` | `f463ef6d56` / `f5e65d8f…` | **FAIL**, aborted at the first failed check (37 PASS, 1 FAIL of 38 asserted): `cr04-final-kill-next-native-cycle-continues`. Observer defect: the next-cycle window was 60 frames, and at this run's frame rate it closed before native AI's next 0.3–0.4 s decision (the repaired lane needed 51 frames; run 5 had needed 12–14). Production code was unchanged from run 5. |
| 7 `20261016T0249284291547Z` | `acc620cb4c` / `ca23fe61…` | **PASS 491/491**: 78 CR-04 + all 413 existing firearm native checks. Scenario 218 s; build + run 304 s. |

Run 2 baseline facts (production firing code unchanged):

- Loaded, empty, paused-loaded and paused-empty: one command, four attacks,
  A→B→C→D, `Success`, order current on its surviving command, exact cost and
  three inline Free reloads. **The remaining attacks of the full attack are
  preserved; the original report is not reproduced in this fixture.**
- Ordinary longbow: one native command made four attacks. The retarget
  assertion it then carried is withdrawn (see run 5 corrections).
- No replacement in range: one shot, native `Interrupt`, no reload or loop.
- **Final kill:** the explicit full attack kills A–D and ends `Success`; its
  order stays current with dead target D. Native AI builds the auto-use Reload
  for remaining enemy E and `NativeFirearmAttackOrder.Submitting` rejects it on
  every retry (binding target E ≠ order target D, owner finished). The pistol
  never fires again.
- AI-only control (same pistol and geometry, no explicit order): native AI
  reloads and keeps shooting E.

**First incorrect transition:** `Submitting` rejects native automatic
continuation because a completed explicit order whose target is dead is still
current.

Lanes added for the first review (run 5 PASS; run 7 PASS on current bytes):

- `repaired-final-kill`: real natural-1 misfire → Broken; production Quick Clear
  (native action bar, standard action, exact cost, grit unchanged) → item
  `Normal` with degradation epoch 1, suppression still on and no current order;
  new explicit full attack accepted at epoch 1; four native attacks A→B→C→D from
  one command with three inline Free reloads and exact cost; after D dies,
  native AI reloads and attacks E with no click. The order ended by completion
  (no current order was cancelled), suppression stayed off, the epoch stayed 1,
  and every next-cycle command was charged once, exactly. Ammunition was
  conserved: cartridges spent = shots + chambered round.
- `final-kill` and `ai-only-final-kill` gained the same next-cycle exact-economy
  check.
- `broken-final-kill` is unchanged: AI stays blocked with the dead-target order
  current; an explicit click on E is accepted and fires while Broken. In run 7
  its no-continuation window spans at least two seconds of Unity time.

### Run 5 corrections (second review)

- **Run 5 `repaired-reload-gap-kill` was an injected-state test, not an ally
  attack.** After the auto-use reload ended with its resume pending, the
  fixture assigned A's damage directly (`Descriptor.Damage = 20`) and then ran
  native projectile-hit and life processing. It shows the resume completing on
  a target already dead by assignment. Its final assertion required only the
  first automatic shot, so one shot followed by another stall would also have
  passed. The run's observations (AI shot B, then C) remain as recorded; they
  are not evidence of an ally hit or of a full continuation. The lane is
  replaced, not rerun.
- **The ordinary longbow control's retarget assertion was unsound.** It read
  each target's consciousness in the `OnAction` postfix, before the arrow's
  projectile impact and life processing, so it did not establish retargeting
  after a kill. The control is now a four-attack ordinary-weapon smoke test only
  (`four-native-attacks-smoke`). In run 7 its unforced rolls left targets
  down, dead, down and up with a fourth-shot miss, which the smoke test does
  not judge.

### Reload gap killed by a genuine ally attack (run 7)

`cr04-repaired-reload-gap-ally-kill-` (all PASS):

1. Real natural-1 misfire → Broken → production Quick Clear → `Normal`,
   epoch 1.
2. A selected party ally (BAB 1: one native attack; Strength 30; ordinary
   sword `57c8994d…`), 1.00 m from A, clicks A through native
   `ClickUnitHandler`. Its native attack starts and waits for its act cue
   (animation handle present, one planned attack, A alive, no firearm binding).
3. The Gunslinger's empty pistol: click A; the auto-use Free reload ends
   `Success`, one round chambered, exact Free cost, and its resume is scheduled
   (one callback, order current, no shot).
4. **In that same frame**, after the pistol's unit action and before
   projectiles, hits and life, the ally's attack acts: native melee
   `RuleAttackWithWeapon`, natural 10, hit, 15 damage, then the native life
   controller kills A. The resume is still pending and the order still current.
5. **Next frame**, the scheduled resume executes with A already dead and
   completes the order. Observed order:
   `reload-ended:continuation-pending > ally-hit:a > a-dead > continuation-executed:watched-dead`.
   The order is not current and not cancelled, no cancellation occurred, the
   resume queued no attack, suppression stayed off, `RequiresOrder` is clear,
   the epoch stayed 1, and the item is `Normal`.
6. **Native AI continuation, no player input:** one AI command,
   `IsFullAttack`, four planned attacks, attack index 4, `Success`. Shots 0–3
   all hit, at B, C, D, D. Native target transitions, each after the previous
   target's death: B(dead)→C, C(dead)→D. B and C dead, durable D alive, no
   current order.
7. **Exact economy:** four cartridges spent for four shots, none chambered at
   the end (one auto-use reload plus three inline Free reloads; no ended full
   attack, no reload fault). The actor ran exactly two commands, the reload
   and the AI attack, both `Success` and each charged once, exactly.
8. Fixture restored exactly.

Disclosed fixture controls in this lane:

- the host plays no animation clips, so the fixture supplies every act and
  finish cue;
- it supplies the ally's act cue in the reload frame, after the pistol's
  unit action (one valid native unit order; the opposite order is not
  exercised);
- the ally's attack d20 is fixed at natural 10 through the native
  `RuleRollDice.Override` seam, as firearm rolls are; the native rule still
  compares it with A's AC and rolls damage.

Nothing assigns death, targets, orders, attack progress or cooldowns. In native
play this gap is a single frame wide.

## Correction

### First correction (`9336cac6`) and the review finding

`9336cac6` concluded a finished order with a dead or "unattackable" target, but
only when `order.Epoch == 0`. Review found this blocking. The ledger epoch is a
monotonic generation that rejects work captured before a committed
degradation, and Quick Clear never resets it. A pistol that went Broken → Quick
Clear → Normal and then received a new explicit order kept that order after its
target died, and native AI stalled again. Removing the epoch test alone was
unsafe, because `Cancel` re-enabled suppression whenever the epoch was nonzero.

### Revised correction (`1890ba06cc`)

- **Epoch kept as history.** It is never reset. Every existing epoch comparison
  that rejects pre-degradation reloads, attacks and callbacks is unchanged.
- **Broken-sequence authority made explicit.** The ledger's per-item
  `RequiresOrder` is set by a committed degradation. `Cancel` and unordered
  automatic work (`Submitting`, `MayExecute`, `MayResume`) consult it instead of
  `epoch != 0`. Until a completion clears it, the two are identical, so
  degradation, supersession, Stop and stale-work behaviour are unchanged.
- **`FirearmAttackOrderLedger.Complete`: non-suppressing normal completion,
  separate from `Cancel`.** It ends an order only when all of these hold:
  - the order is current;
  - its native owner has finished (an unfinished owner always carries it);
  - its target is dead, or no longer a unit;
  - the exact equipped item's effective condition is `Normal`
    (`ExactEquippedFirearmResolver`, the same item-state path firing uses).

  It never sets suppression, and it clears `RequiresOrder`, so later native
  automatic work proceeds as with an ordinary weapon. A Broken, Wrecked or
  unresolved firearm keeps its explicit order exactly as before.
- **Callers.** Native automatic work binds through
  `NativeFirearmAttackOrder.CurrentForAutomatic` (AI attack, AI and auto-use
  reload). A captured reload resume whose target died uses
  `CompleteDefeated`. Any other resume failure, or a Broken firearm, still
  cancels.
- **Unchanged.** In-command retargeting, inline reloads, attack indices,
  iterative penalties, cooldowns, action costs, misfire, Dead Shot, scatter,
  ordinary weapons, turn-based rules, blueprints, identities and save data.

### Target rule: living but unattackable

Because native `CanAttack` is only a faction test, `9336cac6` would also have
ended an order whose living target stopped being hostile (charm, dialogue,
surrender) and might become hostile again. Out-of-range and no-line-of-attack
targets were never covered. The reproduced defect concerns dead targets, so the
rule is narrowed:

- **Only a dead target, or one that is no longer a unit, completes an order.**
- A living target keeps its order (the pre-branch behaviour) when it is:
  - out of range;
  - without a valid line of attack;
  - temporarily untargetable;
  - temporarily not hostile.

No native lane is needed to justify abandoning a living target, because none is
abandoned.

## Validation and limits

- **Focused gate** `-Filter 'firearm-orders;full-attack-reload;paper-full-attack;broken-sequence'`
  with production compilation: 33/33 PASS (2,615 registered), about 8–10 s,
  on `1890ba06cc` and again after each fixture change (`f463ef6d56`,
  `acc620cb4c`; no compiler warnings).
  Ledger cases, built on the production `FirearmStateMachine` misfire and
  Quick Clear item transitions:
  - **A, never broken:** a Normal pistol's finished dead-target order completes
    without suppression, and unordered native work resumes.
  - **B, currently Broken:** the explicit Broken order does not complete
    (Wrecked and unresolved conditions don't either); unordered work stays
    rejected; when the order ends, a new explicit order is needed again.
  - **C, repaired Normal:** after misfire and Quick Clear the epoch stays 1 and
    suppression stays on. A new order accepted at epoch 1 completes without
    suppression and later unordered work is permitted. A later cancel does not
    re-suppress, and a further misfire restores the Broken-sequence rule at
    epoch 2.
  - **D, stale orders:** a superseded order or an unaccepted proposal cannot
    complete the newer order or clear its authority.
  - **E, pre-degradation callbacks:** work captured before the misfire stays
    invalid after repair and completion.
- **Exact build and strict packaging** on `acc620cb4c` through the guarded
  launcher's `Build-Local.ps1`: version-aware repository validation (dispatched
  to the v0.0.155 validator), exact-reference compile candidate, build-output,
  SoundBank and strict standalone UMM package validation. Domain tests were
  skipped in that wrapper (`SkipDomainTests`); the complete domain suite is
  deferred by policy.
- **Tested artifact (run 7):**
  - commit `acc620cb4c8a2694e1195d3109486ae257f537d2`;
  - source state `f51ef9483109a58c398145460964ea711290420f71c0b423a0eaa204e1ae8181`;
  - package `9ddf7a5ff9e8634e80c146a8fbf3a1eb7b6ff9ca780d3b10b0bf5ffd544fa3e6`;
  - DLL `ca23fe61cb049654b879eae14c56c6cba6b50752d0fb92424122480d60fa726a`,
    MVID `829b84ad-f535-44d8-9df2-e88a9fc36238`;
  - the loaded module hash in the run equals the DLL;
  - deployment manifest `runtime-evidence\deployments\20261016T0249283831532Z`.

  Production code is identical to `1890ba06cc`, but the fixture is compiled
  into the same DLL, so run 7 is the evidence for these bytes. Earlier
  artifacts are not: run 5 (`1890ba06cc`, DLL `67c492d2…`, package
  `2d596ce6…`, MVID `dede384d…`), run 6 (`f463ef6d56`, DLL `f5e65d8f…`,
  package `7bb05b01…`, MVID `59a006c8…`, FAIL) and `9336cac6`/`f120026f…`.
- **Validation ownership.** The Sprint 23 armored-dinosaur validator is back to
  its released text. At v0.0.155 the repository dispatches to the Sprint 24
  validator, which now admits this checkpoint's five exact files as a separately
  named `FIREARM_CONTINUATION_PRODUCTION` set. Its theropod
  `ALLOWED_PRODUCTION` is untouched. Validator tests: Sprint 24 11/11,
  Sprint 23 8/8.
- **Restoration and saves.** After each guarded run the live mod was restored
  with `Restore-Live-Mod.ps1` from that run's pre-test backup (run 6:
  `20261016T0242441173288Z`; run 7: `20261016T0249216276661Z`). After run 7 it
  is byte-identical (254/254 files) to that backup and to the first pre-session
  backup `20261015T2107538162175Z` (`Info.json` 0.0.140, DLL `00ef6757…`). No
  Kingmaker process or lease remains. Runs 6 and 7 report `saveBacked: false`
  and `saveInteractionOccurred: false`; no save was selected, loaded or
  mutated.
- **Not run:**
  - complete domain suite;
  - compatibility profiles;
  - save-backed play;
  - physical-input/visual acceptance;
  - the owner's RTWP check with live animation playback (below).

  The save-free host plays no animation clips; act and finish cues are
  fixture-supplied, including the ally's act cue in the reload frame. The
  reload-gap lanes were not run on bytes without the resume-path change; only
  the corrected bytes were run. Existing non-CR-04 firearm observers were not
  reviewed for the same frame-rate dependence; they passed in run 7.
- **Budget.** The original pass exceeded the 30-minute ordinary ceiling
  (fixture corrections and the run 3 abort). The first review revision used one
  gate and one guarded run (356 s including build). This second revision used
  two focused gates and two guarded runs: run 6 (140 s, observer FAIL) and
  run 7 (304 s, PASS).

## Open design questions (owner review)

- After a completed explicit order, native AI is still held to that order's
  *living* target if it would prefer another enemy (pre-branch behaviour, kept
  by the narrowed target rule).
- A currently Broken firearm's AI does not continue past its order's dead
  target without a new explicit order (kept by review direction).
- A firearm that is effectively Broken without a degradation committed in this
  session (loaded from a save as Broken, or a battered firearm's effective
  Broken overlay) also keeps its dead-target order, so its next-cycle stall
  remains until a click. The ledger is transient and was never told of that
  break.
- On a repaired pistol, a Stop or supersession of its new explicit order
  *before* that order completes still restores suppression, as for any order
  after a committed degradation. After a completion the pistol is ordinary, and
  a later cancel no longer suppresses.

## Infrastructure observations

- The launcher's explicit `-PackagePath` candidate branch is unreachable behind
  an older guard.
- The default private reference bundle lacks `UnityEngine.PhysicsModule.dll`.
  The refreshed 2026-10-15 bundle was supplied through `$PSDefaultParameterValues`.
- The launcher leaves the candidate installed, so each run was followed by
  `Restore-Live-Mod.ps1` from its pre-test backup and a byte comparison.
- Scripts require Windows PowerShell 5.1. Do not read evidence files while the
  run holds its lease.

## Owner check (real RTWP, normal animation playback) — DEFERRED

This checkpoint did not perform it. It needs the actual human installation, a
save and live animation playback, none of which an agent may select or mutate.

Setup: a pistol Gunslinger with matching Rapid Reload, paper cartridges,
auto-use Reload and AI enabled, against several weak enemies within about
20 ft. Record these three behaviours separately:

1. **Within one full attack.** Click one enemy. Each later attack of the same
   full attack moves to the next nearest enemy as targets die, with no extra
   click. This is the original report; it was not reproduced in the fixture.
2. **Next automatic cycle.** After the ordered full attack's last target dies,
   the Gunslinger reloads and attacks a remaining enemy without another click.
   This was reproduced and corrected.
3. **After Broken → Quick Clear → Normal.** After a misfire breaks the pistol,
   use Quick Clear, then repeat (1) and (2). Expect the same as for a pistol
   that never broke. This is the review's blocking case; corrected.

If (1) still stops after the first kill, keep the combat log and the session
log from that moment. The fixture cannot exclude a live animation/timing or
mod-stack cause.
