# Firearm RTWP continuation after a kill — development checkpoint

Branch `codex/firearm-rtwp-kill-retarget`. It started from `origin/master`
`fd6c56ebd8` (v0.0.154 record); current master `31672515529c` (v0.0.155) is
merged in at `1d98af476b`. The tested code commit is `1890ba06cc`; any later
commit on the branch changes records only. This is an ordinary focused
checkpoint under [VALIDATION-POLICY.md](VALIDATION-POLICY.md), not release,
Sprint or compatibility qualification. Nothing is merged, tagged, published or
permanently installed.

## Status

- **Original report (later attacks of the same full attack stop after the first
  kill): not reproduced** in the save-free fixture. Loaded, empty and paused
  four-attack full attacks continue A→B→C→D natively. The fixture supplies
  animation act/finish timing, so this does not exclude a live
  animation/timing cause or a mod-stack cause.
- **Related next-cycle stall: reproduced and corrected.** After an explicit full
  attack's last target died, the pistol never fired again without a new click.
- **Review blocking case: corrected.** The same stall after Broken → Quick
  Clear → Normal, which the first correction (`9336cac6`) still had.

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
order cancellations, command endings, `MayExecute` and submission rejections
with call stacks; it changes nothing.

| Run | Source / DLL | Result |
| --- | --- | --- |
| 1 `20261015T2108007138654Z` | `e98a9402` / `7fedc6cf…` | Loaded lanes: four attacks A→B→C→D from one command. Empty lanes: order lost to a fixture ordering artifact (AI ticked between reload end and same-frame delivery); every sequence ended `Interrupt` because the host never finishes animations. |
| 2 `20261015T2127116112635Z` | `b5f48fee` / `c76925a3…` | Native frame order + finish cues. Primary lanes correct; **final-kill next cycle stalls** (baseline reproduction). |
| 3 `20261015T2134449241861Z` | `9336cac6` / `f120026f…` | First correction: 55 CR-04 + 124 existing PASS, then aborted by an external read of the evidence file (sharing violation). |
| 4 `20261015T2137515866390Z` | `9336cac6`, same installed artifact | PASS 468/468. Superseded by review: it had no repaired-Normal lane. |
| 5 `20261015T2303585727403Z` | `1890ba06cc` (integrated v0.0.155) / `67c492d2…` | **PASS 487/487**: 74 CR-04 (55 earlier + 19 new) + all 413 existing firearm native checks. Scenario 264 s; build + run 356 s. |

Run 2 baseline facts (production firing code unchanged):

- Loaded, empty, paused-loaded and paused-empty: one command, four attacks,
  A→B→C→D, `Success`, order current on its surviving command, exact cost and
  three inline Free reloads. **The remaining attacks of the full attack are
  preserved; the original report is not reproduced in this fixture.**
- Ordinary longbow: the same native in-command continuation.
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

Run 5, new lanes (all PASS):

- `repaired-final-kill`: real natural-1 misfire → Broken; production Quick Clear
  (native action bar, standard action, exact cost, grit unchanged) → item
  `Normal` with degradation epoch 1, suppression still on and no current order;
  new explicit full attack accepted at epoch 1; four native attacks A→B→C→D from
  one command with three inline Free reloads and exact cost; after D dies,
  native AI reloads and attacks E with no click. The order ended by completion
  (no current order was cancelled), suppression stayed off, the epoch stayed 1,
  and every next-cycle command was charged once, exactly. Ammunition was
  conserved: cartridges spent = shots + chambered round.
- `repaired-reload-gap-kill`: same repair; empty pistol; click A; the auto-use
  Free reload ends with its resume scheduled (one callback, exact cost); A is
  then killed by native damage and the native life controller, as an ally's hit
  lands in native frame order. The resume completes the order instead of
  cancelling it; native AI then attacks B and C with no click. No suppression,
  epoch unchanged, exact costs, ammunition conserved.
- `final-kill` and `ai-only-final-kill` gained the same next-cycle exact-economy
  check.
- `broken-final-kill` is unchanged: AI stays blocked with the dead-target order
  current; an explicit click on E is accepted and fires while Broken.

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
  with production compilation: 33/33 PASS (2,615 registered), about 10 s.
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
- **Exact build and strict packaging** on `1890ba06cc` through the guarded
  launcher's `Build-Local.ps1`: version-aware repository validation (dispatched
  to the v0.0.155 validator), exact-reference compile candidate, build-output,
  SoundBank and strict standalone UMM package validation. Domain tests were
  skipped in that wrapper (`SkipDomainTests`); the complete domain suite is
  deferred by policy.
- **Tested artifact:**
  - commit `1890ba06ccac0c765350e2de0d6c56bbfda17407`;
  - source state `6d5c26ef519543efd86f1c0dce4abad0449da860836dae5ab46531cc9bd45c55`;
  - package `2d596ce6270c011e15523cd6923bb12246b56e280d97fcc26a9149666403852a`;
  - DLL `67c492d296b7aa67dd179cbdbbd1c43989b55111f3395f82db06588b4bcbe558`,
    MVID `dede384d-3922-4d19-830c-baa815090e80`;
  - the loaded module hash in the run equals the DLL.

  Earlier candidates (`9336cac6`/`f120026f…`) are superseded and are not
  evidence for these bytes.
- **Validation ownership.** The Sprint 23 armored-dinosaur validator is back to
  its released text. At v0.0.155 the repository dispatches to the Sprint 24
  validator, which now admits this checkpoint's five exact files as a separately
  named `FIREARM_CONTINUATION_PRODUCTION` set. Its theropod
  `ALLOWED_PRODUCTION` is untouched. Validator tests: Sprint 24 11/11,
  Sprint 23 8/8.
- **Restoration and saves.** The live mod was restored from the run's pre-test
  backup `20261015T2303515102026Z` with `Restore-Live-Mod.ps1`. It is
  byte-identical (254/254 files) to that backup and to the first pre-session
  backup `20261015T2107538162175Z` (DLL `00ef6757…`). No Kingmaker process or
  lease remains. The run reports `saveBacked: false` and
  `saveInteractionOccurred: false`; no save was selected, loaded or mutated.
- **Not run:**
  - complete domain suite;
  - compatibility profiles;
  - save-backed play;
  - physical-input/visual acceptance;
  - the owner's RTWP check with live animation playback (below).

  The save-free host plays no animation clips; act and finish cues are
  fixture-supplied. The reload-gap lane was not run on bytes without the
  resume-path change; only the corrected bytes were run.
- **Budget.** The original pass exceeded the 30-minute ordinary ceiling
  (fixture corrections and the run 3 abort). This revision used one gate and
  one guarded run (356 s including build).

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
