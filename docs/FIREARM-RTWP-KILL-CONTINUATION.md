# Firearm RTWP continuation after a kill — development checkpoint

Branch `codex/firearm-rtwp-kill-retarget`, based on `origin/master`
`fd6c56ebd8eb82e313c6005972e4538d9b1444c9` (v0.0.154 release record). This is an
ordinary focused checkpoint under [VALIDATION-POLICY.md](VALIDATION-POLICY.md),
not release, Sprint or compatibility qualification. Nothing is merged, tagged,
published or permanently installed.

## Report and installed identity

Owner report: a stationary RTWP Gunslinger (pistol, matching Rapid Reload, paper
cartridges, auto-use Reload, four planned attacks, several enemies within about
20 ft) kills A with the first shot and then stops, while ordinary weapons go on.

The lab's live install is the 0.0.140 favored-class build: `Info.json` 0.0.140,
DLL SHA-256 `00ef67570d30e4ff9c54a2ad2027393660436d5724bcee2bb99e4c5718e966fd`,
MVID `842d73db-a49f-459f-9b47-ca80cf0f273a`, embedded commit `fff3b778f0`. Its
`Firing`, `Reloading`, `Misfires`, `Actions`, `Deeds` and `Scatter` sources equal
current master, so an installed-build mismatch does not explain the report.

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

## Evidence (guarded save-free `disposable-firearm-break-interruption`)

New CR-04 lanes (`RuntimeTestRunner.FirearmReviewKillChain.cs`) click A through
native `ClickUnitHandler.OnClick` with BAB 16 (four native planned attacks),
Gun Training, paper cartridges and auto-use Reload. Native `UpdateTarget`
chooses every replacement after real projectile damage and native death. The
disclosed roll control is natural 10 on firearm attack rolls (hit, no misfire).
A fixture-only observer records order cancellations, command endings,
`MayExecute` and submission rejections with call stacks; it changes nothing.

| Run | Source / DLL | Result |
| --- | --- | --- |
| 1 `20261015T2108007138654Z` | `e98a9402` / `7fedc6cf…` | Loaded lanes: four attacks A→B→C→D from one command. Empty lanes: order lost to a fixture ordering artifact (AI ticked between reload end and same-frame delivery); every sequence ended `Interrupt` because the host never finishes animations. |
| 2 `20261015T2127116112635Z` | `b5f48fee` / `c76925a3…` | Native frame order + finish cues. Primary lanes correct; **final-kill next cycle stalls** (baseline reproduction). One fixture verdict was read after the next cycle exhausted ammunition (fixed). |
| 3 `20261015T2134449241861Z` | `9336cac6` / `f120026f…` (package `9208f68b…`, MVID `d65a32d4…`) | Correction: all 55 CR-04 checks PASS and 124 existing checks PASS; aborted by an external read of the evidence file (sharing violation), not by a firearm defect. |
| 4 `20261015T2137515866390Z` | same installed artifact (`-ReuseInstalledArtifact`) | **PASS 468/468**: 55 CR-04 + all 413 existing firearm native checks (merge, hover, reload/turn costs, CR-02 retarget, coexistence, ownership, repair, desktop/controller × RTWP/TB misfire→Broken→Wrecked, recovery). 232 s. |

Run 2 baseline facts (production firing code unchanged):

- Loaded, empty, paused-loaded and paused-empty: one command, four attacks,
  A→B→C→D, `Success`, order current on its surviving command, exact cost and
  three inline Free reloads; the next cycle continues against surviving D.
  **The remaining attacks of the full attack are preserved; that part of the
  report is not reproduced in this fixture.**
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
current. The ordered pistol stops; the unordered pistol and ordinary weapons
continue.

## Correction

`FirearmAttackOrderLedger.Conclude` and `NativeFirearmAttackOrder.CurrentForAutomatic`
apply, when native automatic work (AI attack, AI or auto-use reload) binds to the
accepted order, the rule a captured reload continuation already applies in
`ResumeAttack`: an **undegraded** order whose owner has finished and whose target
is dead or no longer attackable is concluded, without suppression. Native AI then
proceeds as with an ordinary weapon.

- An unfinished owner always carries its order (native retargeting continues on
  its next tick; input prediction swaps `actor.Commands`).
- Degraded (Broken) orders are unchanged: AI stays blocked and a new explicit
  order is still required and fires while Broken. Wrecked stays unusable.
- Unchanged: in-command retargeting, inline reloads, attack indices, iterative
  penalties, cooldowns, action costs, misfire, Dead Shot, scatter, ordinary
  weapons, turn-based rules, blueprints, identities and save data.

Run 3 with the correction: final-kill concludes the order at the AI reload's
construction (`CreatedAutomatic → CaptureReloadBinding → CurrentForAutomatic →
Conclude`), then native AI reloads and full-attacks E. Broken final kill: AI
reloads stay rejected; the explicit click on E is accepted and fires while
Broken.

## Validation and limits

- Focused gate `-Filter 'firearm-orders;full-attack-reload;paper-full-attack;broken-sequence'`:
  baseline 28/28, corrected 30/30 PASS (2 new ledger tests), each under 10 s.
- Candidate DLL `f120026f8cd64b58d349d72952b41355ed11126a7671b27d5c31df915f237f88`,
  package `9208f68bd9014416033b923dac7900346d4155c2de96d6b212e5a249a92d219a`, built
  once (about 45 s) and reused for run 4.
- Sprint 23 scope gate names this checkpoint's exact files; its tests pass.
- Candidate builds used `Build-Local.ps1 -SkipDomainTests` with the 2026-10-15
  refreshed reference bundle (repository wrapper, exact build, package and
  strict validation ran; the complete domain suite is deferred by policy).
- Not run: complete domain suite, compatibility profiles, save-backed play,
  physical-input/visual acceptance, an ally kill inside the one-frame
  reload→resume gap. The save-free host plays no animation clips; act and finish
  cues are fixture-supplied.
- Budget: runtime iteration exceeded the 30-minute ordinary ceiling because the
  first fixture needed two corrections and run 3 was aborted by an evidence-file
  read during the run; run 4 reused the unchanged candidate.

Open design questions (unchanged, for owner review): after a completed explicit
order, native AI is still held to that order's *living* target if it prefers
another enemy; a Broken firearm's AI does not continue past its order's dead
target without a new explicit order.

Infrastructure observations: the launcher's explicit `-PackagePath` candidate
branch is unreachable behind an older guard; the default private reference
bundle lacks `UnityEngine.PhysicsModule.dll` (the refreshed bundle was supplied
through `$PSDefaultParameterValues`); the launcher leaves the candidate installed,
so each run was followed by `Restore-Live-Mod.ps1` from its pre-test backup and a
byte comparison. Scripts require Windows PowerShell 5.1.

## Owner check

In RTWP, order a pistol Gunslinger (matching Rapid Reload, paper cartridges,
auto-use Reload, AI on) to attack one of several weak enemies within 20 ft.
Expect the full attack to move to each next nearest enemy as targets die, and,
once its last target dies, the Gunslinger to reload and keep attacking the
remaining enemies without another click.
