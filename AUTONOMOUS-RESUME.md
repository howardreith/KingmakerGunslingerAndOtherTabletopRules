# Races Unleashed Spider Summoner integration — incomplete with named blockers / STOP

Current (2026-11-10). See planning/races-unleashed-compatibility/STATUS.md. Branch
`codex/races-unleashed-summoning`, draft PR #125 against master. Activated by the owner's mission package;
the deferred handoff is preserved as `planning/races-unleashed-compatibility/BASELINE.md`.

- Review corrections and the scoped native CMB web-escape +2 are implemented.
- Candidate `e6723d872320` passed under controlled input (no human input, pointer parked): PASS 174/0.
  - Native skill, native CMB and Call of the Wild's loaded escape branches each went from 12 to 14 with the marker.
  - The fiendish Crab and Widow casts and the retained-toggle cast passed.
  - Its earlier FAIL 67/2 (popup-teardown guard refusal) is retained, recorded as possible input interference.
- Observer candidate `655ab8a8ae3e` FAIL 155/9 from observer defects:
  - a Json.NET null-token check;
  - an in-phase real web hit that left a second native mode.
  Its recorded values still confirm the per-branch rules and a real ordinary grapple unchanged at 12. The fix is
  `182717dd10`, committed but not built.
- Next step: build `182717dd10` once and run under controlled input. Then a real feat web hit in a fixture that owns
  its clock and hostile, the `gunslinger-races-unleashed` profile through
  `scripts/compatibility/Invoke-KingmakerCompatibilityProfile.ps1` after preflight, and the STATUS scenario table.
- Install, saves, settings and `Params.xml` restored exactly after each run (`verified`).
- No merge, release, permanent install or further native run follows without explicit owner authorization.

---

# (superseded) Cleric alignment restrictions — complete, owner-accepted and released in v0.0.181 / STOP

Current (2026-11-09). See planning/summon-cleric-alignment/STATUS.md. PR #122 merged as `2f5703f7c9`.
Stable v0.0.181 is Latest, from release commit `daf87b5456`, package `6733aea0...8532`, with the
accepted exception in `KNOWN-ISSUES.md` and its notes. The installed mod still reads 0.0.140.

Cleric alignment mission complete and owner-accepted for the present scope, with known Races
Unleashed compatibility limitations deferred to a separate mission. The latest native invocation
remains FAIL, 70 PASS / 1 FAIL; its unresolved third-party census findings are an explicit
owner-accepted exception, not a passing result.

- Results stay with their candidates: `059854c046e1` FAIL 4/3, `03a87b348912` FAIL 49/1, `efc75b8cc0b0` FAIL 70/1.
- Races Unleashed work is parked: planning/races-unleashed-compatibility/MISSION.md is DEFERRED and
  NOT AUTHORIZED TO START. Do not resume it from this file.
- Product code, observers, tests, blueprints and assets are frozen at `efc75b8cc0b0`.
- Install, saves, settings and `Params.xml` restored exactly. Ledger about 2,026 of 2,400 s, closed.
- No further build, native run, merge, release or permanent install follows without explicit owner authorization.

---

# (superseded) Alphabetical summon menus — accepted, PR #119 merged / STOP

Current (2026-11-09). See planning/summon-alphabetical-menus/STATUS.md. Branch
`codex/summon-alphabetical-menus`, draft PR #119 against master. Separate from R1-R7,
which integrated through PR #118 (`1efd8a011e`).

- Alphabetization accepted on existing source, focused-test and native menu evidence. Historical
  native invocation remains FAIL because of observer setup defects. Observer corrections are
  source/focused-check only; no further native run performed.
- Candidates keep their verdicts: `2f7a1afb6ebd` FAIL; `c55f40e53f36` 44 PASS / 3 FAIL. Two of those
  passes (alternate-row clicks) did not establish selection.
- Observer corrections: `fbd117d4a3`, focused 11 of 2,894 passing. Not executed natively.
- Established: census 93 of 93 arrays, six menus opened, two canonical selections, rollback
  refusal on a transient parent, Druid factory/popup agreement.
- Not established: a completed reordered cast, alternate selection, a feature-enabled spider
  popup, scroll movement, console navigation, saved hotbars.
- Install, saves, settings and `Params.xml` restored exactly on both runs. Ledger about 1,981 of
  2,400 s. No merge, release or permanent install.

---

# (superseded) Summoned creature manual-playtest repair — R7 complete for review / STOP

Current (2026-11-09). See planning/summon-playtest-repair/R7-STATUS.md. Branch
`codex/summon-playtest-r7-models`; draft R7 PR on #116 on #115 on #114.

- ART-01..03 revised on final candidate `e4b62528fa2c`. Bright and dim guarded runs
  passed. Owner aesthetic approval is pending, as are R6 viewing and listening.
- "After" stills caught the end of the summon fade-in. Start with `REVIEW-GUIDE.md` in
  `runtime-evidence/summon-playtest-r7/review-e4b62528fa2c`; its curated sheets carry
  fade-in, obstructed and diagnostic labels and exact frame provenance.
- Records closeout only after `e4b62528fa2c`: results stay on that candidate. No dim
  attack ran. The Ice Devil generator was not repeated on the final source. The
  Nalfeshnee has 48 zero-area wing-root triangles (nonblocking cleanup).
- Install, saves, settings and `Params.xml` restored exactly.
- R7 ledger about 1,448 s (900 target, 1,800 ceiling): the stated 1,406 s plus about
  42 s for the repeat-generation check. R5 about 1,389 s; cumulative R6 about 2,796 s.
- Blender 4.5.10 is at `C:/Dev/.toolchains/blender-4.5.10-windows-x64/`.

Next: owner review of the stack and models. Merge, release or permanent install only
on explicit authorization.

---

# (superseded) R6 complete for review / STOP

Current (2026-11-08). See planning/summon-playtest-repair/R6-STATUS.md. Branch
`codex/summon-playtest-r6-presentation`; draft PR #116 on #115 on #114.

- All required R6 technical gaps are closed. Final candidate `2f1c4eb9f90f`: one
  guarded run, all assertions passed. Cue retirement now stops playback by playing
  ID (Trumpet observed; Vrock shares the implementation).
- Owner viewing and listening remain separate. PRES-08 is deferred.
- Install, saves, settings and `Params.xml` restored exactly.
- Ledgers: original R6 about 1,498 s, repair about 1,298 of 1,500 s, cumulative R6
  about 2,796 s. R5 about 1,389 s.
- DATA is shared with `KingmakerSkipCinematicLab`: arrange a no-Kingmaker window
  before any build or guarded run.

Next: owner review of #114, #115 and #116, viewing and listening, then explicit
authorization for R7.

---

# (superseded) R6 repair checkpoint / STOP

Current (2026-11-08). See planning/summon-playtest-repair/R6-STATUS.md. Branch
`codex/summon-playtest-r6-presentation`; draft PR #116 on #115 on #114.

- Repair candidate `c31c6292f7d3`, one guarded run: all assertions passed except
  `r6-cue-early-retirement`. Install, saves, settings and `Params.xml` restored.
- PRES-02, PRES-03, PRES-07 technically established; PRES-04 supported by paired
  views. PRES-01 stays established from `0dd811be26b2`.
- Open: a Vrock/Trumpet cue outlives a destroyed caster view (4.6 s after retirement;
  the native registration is gone before the stop runs). Owner viewing and
  listening remain separate. PRES-08 deferred.
- Ledgers: original R6 about 1,498 s, repair about 688 of 1,200 s; a further build
  and run does not fit. R5 about 1,389 s.
- DATA is shared with `KingmakerSkipCinematicLab`: arrange a no-Kingmaker window
  before any build or guarded run.

Next: owner decision on a small allowance for the cue-retirement repair. R7 starts
only on explicit authorization.

---

# (superseded) R6 partly complete / STOP

Current (2026-11-08). See planning/summon-playtest-repair/R6-STATUS.md. Branch
`codex/summon-playtest-r6-presentation`; draft PR depends on #115, then #114.

- Candidate `0dd811be26b2`, one guarded run: all assertions passed except
  `r6-redcap-scythe`, a confirmed product defect. Install, saves, settings and
  UMM `Params.xml` restored exactly.
- PRES-01 technically established. PRES-05/06 cues work; owner listening pending.
- Open: PRES-02 self-targeted Aid, PRES-03 Redcap scythe swing, PRES-04 clear
  Lantern capture, PRES-07 Monster-family arrival rim. PRES-08 deferred.
- R6 ledger about 1,498 of 1,800 s; a second candidate does not fit.
- DATA is shared with `KingmakerSkipCinematicLab`: arrange a no-Kingmaker window
  before any build or guarded run.

Next: the owner decides whether to authorize a separate R6 repair allowance. R7
starts only on explicit authorization.

---

# (superseded) R5 UMM ownership repairs / STOP

Current (2026-11-08). The last two R5 UMM ownership findings are repaired on
`codex/summon-playtest-r5-spells` (scripts commit `307cf89356`, draft PR #115,
still stacked on draft #114). See the top section of
planning/summon-playtest-repair/R5-STATUS.md.

- Automatic owner restoration now holds a lease in the receipt's coordination
  root (its own or a validated inherited one) through recheck, restore and
  receipt completion. A foreign transaction is refused with no write.
- A game already running is refused before any lease is created, so the refusal
  leaves no lock or recovery debt; the recheck under the lease remains.
- Results: UMM tests 85 assertions pass, orchestration 297 pass, scripts parse.
  Three mutation checks fail as intended.
- No package was built, Kingmaker was not launched, and the live installation
  was not changed. Production C# is unchanged from `06d9bc4d0e`.
- The 22/0 native result stays with tested source `366fad0ac41b` only. The
  repaired binder is not natively observed.
- R5 ledger is about 1,389 of 1,800 s.

Next: owner review of #114, then #115. R6 starts only on explicit
authorization.

---

# (superseded) R5 review repairs / STOP

Current (2026-11-08). Three R5 review findings are repaired on
`codex/summon-playtest-r5-spells` (commit `06d9bc4d0e`, draft PR #115, still
stacked on draft #114). See the top section of
planning/summon-playtest-repair/R5-STATUS.md.

- The Call of the Wild Mist and Dance bindings are now atomic. On any failure
  the ability stays hidden, guarded and unavailable, and Dance keeps its
  one-use cost. The two bindings stay independent. Covered by
  failure-injection domain tests and a mutation check.
- UMM recovery is tied to the owner's lease and process identity. It is
  serialized through the coordination lock, and refuses while the owner, a
  foreign lease or Kingmaker is active.
- The wrapper's installation cleanup and UMM cleanup are independent in both
  directions, and an overall PASS is refused if either is unresolved.
- Results: UMM tests 52/52, orchestration 297, focused gate 149/149, and
  production compiled. No rebuild, launch or native run.
- The 22/0 native result stays with tested source `366fad0ac41b` only. The
  repaired binary is not natively observed.
- R4's Params.xml exception, R5's first observer failure, and the R5 coverage
  limits are unchanged.
- R5 ledger is about 1,354 of 1,800 s.

Next: owner review of #114, then #115. R6 starts only on explicit
authorization.

---

# (superseded) R5 complete for review / STOP

Current (2026-11-08). R5 is complete for review on branch
`codex/summon-playtest-r5-spells`, stacked on draft PR #114 (open; the R5
draft PR targets `codex/summon-playtest-r4-mephit`). Owner aesthetic
approval is pending. See planning/summon-playtest-repair/R5-STATUS.md.

- UMM `Params.xml` protection is now in the actual guarded DATA wrapper
  (byte backup + durable receipt, verified restore, recovery script);
  25/25 behavior tests. Both R5 runs restored-verified at the R5 intake
  `e44f4495…`.
- R4 restoration qualification remains: after R4, Params.xml did not return
  to its R4 intake hash and exact restoration of that file is not
  established; no matching historical backup exists.
- SPELL-01: Cloud Giant has one at-will Obscuring Mist SLA wrapping Call of
  the Wild's spell (point target); both custom fog routes removed.
- SPELL-02: Pixie has at-will native Entangle, full Confusion, Dispel Magic
  and Shield (owner adaptation).
- SPELL-03: Pixie Dance wraps CotW Irresistible Dance, once per summoning.
- MECH-01: Griffon's duplicate native Charge removed (engine already grants
  it).
- MECH-02: Mastodon trample 2d8+18, Reflex DC 29.
- Candidate `366fad0ac41b` (package `8862c485…3af7`, DLL `d5adb10c…e936`):
  22 PASS / 0 FAIL. First candidate `73bfc8ca97a9` failed on an observer
  type lookup (evidence kept).
- Gaps: provider-absent omission not observed natively; no legacy/cold-load
  cell; no autonomous AI cast; Dispel area variant not executed.
- R5 ledger ≈ 1,282 s (target 900 s exceeded, ceiling 1,800 s kept). R4
  remains 1,135 s.

Next: owner review of the R5 PR (after #114), then an explicitly authorized
R6. Do not start R6 automatically.

---

# (superseded) R4 complete for review / STOP

Current (2026-11-08). R4 is complete for review on draft PR #114 (branch
`codex/summon-playtest-r4-mephit`); owner aesthetic approval is pending.

- Finishing candidate `15f9f68a976a` (package `a5c1d4b7…d555`, DLL
  `2928e004…dc25`): 17 PASS / 0 FAIL.
- The first candidate `a1e6c264193f` (26/0) is retained.
- Salt now uses Metal Blast art (native art inspected first).
- Charter ally-safety is repaired on the four owned native-backed breaths;
  the native case shows an ally in the cone untouched.
- The Pyrotechnics flash was captured while alive and retired by itself.
- The KMG installation, settings and saves were restored, and the enabled
  profile is unchanged. UMM `Params.xml` did not return to its intake hash,
  so exact restoration of it is not established. An older-backup
  comparison found only a `LastUpdateCheck` difference.
- R4 ledger: 1,135 of 1,800 s.

Next: owner review of PR #114, then an explicitly authorized R5 (actual
spell reuse and missing abilities). Do not start R5 automatically.

---

# (superseded) R4 mephit breaths (partial) / STOP

Current (2026-11-08). R4 runs on branch `codex/summon-playtest-r4-mephit`
from master `5ad3e88767` (R3 merged), as a draft PR.

- Candidate: `a1e6c264193f` (package `69d293d7…1527`, DLL `b7fbc23a…1935`).
  One guarded run: 26 PASS / 0 FAIL.
- MEPH-01/02 complete for review:
  - Air (slashing), Earth (bludgeoning) and Water (acid) are repaired
    through owned breaths with the grant and brain redirected; native
    donors are untouched.
  - Water acid and Earth bludgeoning packets were executed.
- MEPH-03: nine icons fit; **Salt's icon is misleading** (open).
- MEPH-04: the flash spawns once per executed cast and retires about 3.1 s
  later. **No during-frame capture** (observer ordering; open).
- Restoration exact (0.0.140, `5C3E4AD4…`, 66 settings, 95 saves).
- R4 spend: 588 s of the 900 s target (ceiling 1,800 s).

Next: only the precise R4 continuation in R4-STATUS.md (Salt icon plus the
flash frame). Do not start R5 automatically.

---

# (superseded) R3 complete for review / STOP

Current (2026-11-08). R3 is complete for review; owner aesthetic approval is
pending.
- UI-01 keeps its inspected tracker screenshot from `57a8e659b1e6`; the
  undetected Auto End Turn prompt is that candidate's limitation.
- UI-02..06 are established by eight inspected native ability description
  windows on observer candidate `31e61aa73a01` (package `f8258ca4…1d5f`, DLL
  `af3ebc14…fe74`). The run gave 23 PASS, 0 FAIL.
- The observer now builds `TooltipData` with `Type = TooltipType.Ability` and
  refuses unrelated native modals. Product code has been frozen since
  `221cadc7a67e`.
- Restoration verified (0.0.140, `5C3E4AD4…`, 66 settings, 95 saves).
- Spend: 1,149 of the 1,500 s cap; cumulative R3 ≈ 2,910 s.

Next: owner review of PR #113, then an explicitly authorized R4 (mephit breath
audit). Do not start R4 automatically.

---

# (superseded) R3 UI-evidence checkpoint (partial) / STOP

Current. Owner allocation of 900 s (spent 573 s) on observer candidate
`57a8e659b1e6` (package `36f7054a…4f55`, DLL `fad71354…3eca`); product code
frozen since `221cadc7a67e`. UI-01 is established on an inspected native
tracker screenshot (all seven representatives' own art, overlays intact).
UI-02..06 rendered evidence is missing: the eight owned description windows
rendered empty because the observer omitted `TooltipData.Type =
TooltipType.Ability`. Data predicates pass for all six; owner visual approval
is pending. Restoration verified. Cumulative R3 ≈ 2,334 s across three ledgers.
Next: owner manual retest of UI-02..06, or an observer-only continuation
(set the tooltip type and refuse unowned modals) under a new allocation.
Not R4.

---

# (superseded) R3 rendered-UI continuation (partial) / STOP

Current. Continuation (owner allowance 600 s, spent 505 s) on candidate
`76a9cf269310` (package `d95bf1fe…5bd4`): all data predicates PASS; the native
tracker rendered all seven representatives' own portraits before the pause.
The capture failed because a brainless AI turn still ended after `IsPaused`
(observer defect), so there are no screenshots or description windows yet.
Restoration verified. Next: a narrow observer stabilization plus one run,
needing a new owner allowance. Not R4.

---

# (superseded) R3 completion (partial) / STOP

Current. PR #112 merged (`fa3eeb9e1a`); PR #113 retargeted to master.
UI-01..UI-06 product repairs pass their native data predicates on candidate
`221cadc7a67e` (package `6c338b28…502e`): the Ghaele ray is redirected through
its proven `AddAbilityToCharacterComponent` grant with an owned brain, Overrun
art differs from Trample, and the inherited trample/overrun rows are replaced.
Rendered-UI evidence is still missing: the tracker observer read the wrong
image (fixed in source, compile-only). Cumulative R3 ≈ 1,256 of 1,800 s;
the remaining ≈ 544 s cannot cover one build plus one run. Restoration
verified. Next: the narrow R3 rendered-UI continuation, with a new allowance
granted by the owner. Not R4.

---

# (superseded) R3 checkpoint (partial) / STOP

Current. R2 closed by owner acceptance (draft PR #112, not merged). R3
(UI-01..UI-06, presentation only) is a partial development checkpoint on
`codex/summon-playtest-r3-ui`, based on the #112 branch
([R3-STATUS](planning/summon-playtest-repair/R3-STATUS.md)).

- Candidate `42cd2e26978e` (package `8dc227c3…71b5d2`); one guarded save-free
  DATA run: 9/10 R3 predicates PASS.
- UI-01, UI-02, UI-04, UI-06 implemented with native data PASS; no visual PASS
  (captures covered by the prologue storybook overlay).
- UI-03 open: owner identified (native `AzataGhaelLightRay` `33e89979…`), but
  the clone swap found no unit-level grant (`factSwaps=0`). UI-05 open:
  Overrun icon duplicates Trample's and both carry inherited wrong tooltip rows.
- R3 spend ≈ 661 s (≈ 11 min). Restoration verified. Next: the narrow R3
  repair, not R4.

The R2 record below is retained unchanged.

---

# Summoned creature manual-playtest repair — R2 closed

Current. R1 merged (PR #111). R2 closed by owner acceptance on
`codex/summon-playtest-r2-lifecycle` (draft PR #112, base master
`9d15ba1897`) ([R2-STATUS](planning/summon-playtest-repair/R2-STATUS.md)).

- LIFE-02 repaired (native PASS 8/8, candidate `c370ec234b70`).
- LIFE-01 closed: Ordinary Acadamae readiness verified for the requested cases; the reported creature-specific delay was not reproduced. Surprise-round delay was observed and explained: an arriving summon is marked Surprised and first acts in round 1. Native arrival behavior is retained for this release.
- Native surprise arrival behavior retained; no KMG override. Unexplained
  hostile round-zero turn and the seconds-component lead stay recorded as
  nonblocking observations.
- Cumulative R2 validation ≈ 76.6 min plus ≈ 8 s recovery. No further R2 work
  is requested. R3 (portraits and interface) proceeds on its own branch.

The R1 record below is retained unchanged.

---

# Summoned creature manual-playtest repair — R1 checkpoint / STOP

Current. A new repair sequence (R1-R7) from the owner's 2026-11-07 manual
playtest, separate from the historical creature sprints. Records:
[planning/summon-playtest-repair/](planning/summon-playtest-repair/R1-STATUS.md).

R1 (five owner cuts: Stirge withheld from public routes; Wall of Ice removed
from Bone and Ice Devil; Magma Form; Lillend Bardic Suggestion; Fire Giant
Sunder) is a reviewable development checkpoint on
`codex/summon-playtest-r1-cuts`: focused 196/196, one development candidate
`ab24a92394ea` (package `2f14eebc…ff8c`), one guarded save-free DATA run
(16/18 original; a reader-only re-evaluation of the observer toggle-identity
defect gives a fact-level 18/18 that never observed the real replacement buff;
both kept), exact restoration of DATA's v0.0.140 intake, settings and saves.

Review follow-up on PR #111 (same day): no production defect found. Observer
only (`d46726d46d`): the real ProperFlanking2 Sunder chain
(`SunderActionToggleAbility` `d9604ead…`, `SunderActionToggleBuff`
`f9f7edc1…`) is resolved exactly and its toggle/buff states read separately,
and Lillend is re-read after reconciliation. Second development candidate
`d46726d46ddc` (package `492a8f12…980d`), one guarded save-free DATA run:
**PASS 20/20**; Storm Giant control buff active before and after, induced Fire
Giant buff active before the fixer and gone after it. Exact restoration again.
Cumulative R1 validation ≈ 20.4 min (past the 15-minute target, inside the
30-minute ceiling). Not a release. Stop for review; R2 (combat entry and fear
evaluations) is not started and needs its own authorization.

The section below is the previous record, retained unchanged.

---

# Sprint 40 — v0.0.178 RELEASED / STOP

Stable [v0.0.178](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/tag/v0.0.178)
is published and publicly verified. One adult, untemplated Ice Devil (Gelugon)
is functional through exactly one Summon Monster IX route. The public surface
goes from 133 creatures and 1,216 choices to 134 and 1,217 — this one route and
nothing else.

Tested source `b2be3ebe805f58f53acd3120f75479aa19a634f9` merged at
`839204cac36e2c2079377269b8024f8a76d4c4f6` through product
[PR #99](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/99);
the tested head is a direct parent of that merge and its product paths are
identical to it. Publication promoted the exact qualified ZIP without
rebuilding. The downloaded ZIP SHA-256 is
`4d6e262181e5abd461a537380efe1c05781b55e25bd492821f2bb911482dbe25`; DLL SHA-256
`c6bb7377b89e73bee89c9f747d7b1e0877af45318bfe8ea97d7e970794f4fda6`; MVID
`1b273dc4-236e-4f85-9395-af2ec00fae95`. The annotated tag peels to the product
merge and the release is stable, non-draft, non-prerelease and Latest.

The 2,817/2,817 unfiltered gate, exact byte reproducibility, ten fresh native
executions, six distinct ON/OFF persistence processes, the public download
check and exact laptop/Working restoration all passed.

[PR #97](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/97)
merged an earlier head, `1e7a9663f63dbb3f5458165e56c78ae17cece4ef`, which was
**not** published: a reviewed P2 wall-retirement exception-safety finding was
unresolved in those bytes. PR #99 carries the repair and is the released
candidate.

Wall retirement is now resumable and idempotent. An in-flight native grid
update defers and resumes once the pathfinder settles it, a replaced navigation
graph completes without applying stale restoration data to the replacement,
destruction forces completion rather than stranding ownership, and the residual
cold, the fire fog and the lifetime removal happen exactly once instead of
being skipped behind a one-way flag. Covered by
`expanded-summoning.sprint35-wall-retirement-resumable` and by the controlled
native regression `sprint32-sprint40-wall-interrupted-retirement-resumes`, in
which the fixture supplies and then clears the engine updating flag while the
production retirement code executes natively: 3 deferrals, 28 claims held while
pending, retirement completed, lifetime retired, claim count back to its
baseline of 0, 48 original node bits restored, no grid failure. That is a
fixture-induced precondition, not an observed natural engine race.

The Slow carried in the persistence envelopes is effect-only preparation: it
establishes the real production Slow state for cold-load testing and is not
attack, save, eligibility or action-cost proof. Those remain supplied by the
separate real-time and turn-based native combat runs on this same artifact.

Persistent Image is an owner-approved omission with no substitute and no
engine-impossibility claim; greater teleport and nested summoning remain
excluded; effective movement is the printed 60-foot fly with the separate
40-foot land mode unmodeled. The Nalfeshnee-owned Slow is corrected to its
printed CL12 close range of 55 feet and Unholy Nimbus is documented as Daze
rather than Blindness. Owner aesthetic approval of the model and icon is not
claimed.

Sprint 41 — the core ideal-roster release audit — is authorized next and is not
a release. Cleric alignment restrictions and alphabetical summon-list ordering
remain deferred implementation work after that audit. Variant elementals remain
excluded.

# Sprint 39 — v0.0.177 RELEASED / STOP

Stable [v0.0.177](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/tag/v0.0.177)
is published and publicly verified from product
[PR #94](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/94).
One adult, untemplated Nalfeshnee is functional through exactly one Summon
Monster IX route. Tested source `d1365d621295cf63c7b646247d6bf604edb208da`
merged at `6782721cf08c75c0377f777a265b2f8b01f2cbad`; publication promoted the
exact qualified ZIP without rebuilding. The downloaded ZIP SHA-256 is
`47ef6ee375164ab367ee8e8a7ecf178e60aa7344e3983127938abb18957014e4`; DLL
SHA-256 `e9b5544148d14d46c53623f727a533c9875c1d9b9a20822a37ccbae04157d016`;
MVID `251876ff-6cca-4e94-acea-b9052fb08a76`. The 2,810/2,810 unfiltered gate,
exact byte reproducibility, ten fresh mandatory coverage rows, six fresh
ON/OFF persistence processes, the public download check and exact
laptop/Working restoration all passed. The public surface goes from 132
creatures and 1,215 choices to 133 and 1,216 — this one route and nothing
else.

Unholy Nimbus arms a delayed burst one round later rather than resolving on
activation, rolls 1d10 rounds of daze at the burst, catches a latecomer
and spares a departer, and allows at most one pending activation per creature,
where a refusal spends no use and adds no cooldown. The constant Unholy Aura
is an exact-source self effect with no ally grant. Feeblemind imposes a real
spellcasting prohibition without banning magic items, survives its caster's
destruction, and is cured by native *heal* through one removal inserted beside
the native one — with heal's other cures shown still intact. Greater dispel
magic carries one decisive check to its printed CL/4 cap with no retry.

Summoning demon reinforcements is an owner-authorized omission with no
substitute; greater teleport, nested summoning and wish remain excluded for
every summon. Two deviations are disclosed rather than corrected: effective
movement is the printed 40-foot fly with the 30-foot land mode omitted, and
Slow carries this project's recorded medium reach. No owner aesthetic approval
is claimed; fine visual polish remains backlog. Full attribution and release
identities are in [Sprint 39 status](planning/SPRINT39-STATUS.md) and
[release evidence](planning/SPRINT39-RELEASE-EVIDENCE.json).
**Stop before Sprint 40.** Historical text below is not current authority.

# Sprint 38 — v0.0.176 RELEASED / STOP

Stable [v0.0.176](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/tag/v0.0.176)
is published and publicly verified from product
[PR #92](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/92).
One adult, untemplated Glabrezu is functional through exactly one Summon
Monster IX route. Tested source `e4c4d77a88ca19c3d22c0a516a645669d43eb64c`
merged at `6370addc14a438fc80adf54e9b0062bd5777ef99`; publication promoted the
exact qualified ZIP without rebuilding. The 2,801/2,801 unfiltered gate, exact
byte reproducibility, eleven fresh mandatory coverage rows, six fresh ON/OFF
persistence processes, the public download check and exact laptop/Working
restoration all passed.

The printed rend is scoped to the round with per-victim history, Power Word
Stun is once per summoning at its printed 60-foot close range with every
hit-point bucket and no saving throw, and the AI spends that single use only on
a candidate the printed spell can actually affect.

Reverse gravity and veil are owner-authorized omissions with no substitute;
greater teleport, nested summoning and wish remain excluded for every summon.
One measured deviation is disclosed rather than corrected: live Perception
reads 28 against the printed 26 because the installed Persuasive feat adds +2
Perception in this game, and this creature's printed feat list includes
Persuasive. No owner aesthetic approval is claimed; fine visual polish remains
backlog. Full attribution and release identities are in
[Sprint 38 status](planning/SPRINT38-STATUS.md) and
[release evidence](planning/SPRINT38-RELEASE-EVIDENCE.json).
**Stop before Sprint 39.** (Historical: superseded by the Sprint 39 record above.)

# Sprint 36 — v0.0.174 RELEASED / STOP

Stable [v0.0.174](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/tag/v0.0.174)
is published and publicly verified from product
[PR #88](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/88).
Babau and Hezrou are functional through seven untemplated Summon Monster routes.
Tested source `fc394d6281f9d70aa81c03d02df01f95a8927454` merged at
`bbe19b55e985b18823695c6d9fa269f0dcb9407c`; publication promoted the exact
qualified ZIP without rebuilding. The 2,785/2,785 gate, exact reproducibility,
mechanics/presentation, six fresh ON/OFF persistence processes, public downloads,
lineage and exact laptop/Working restoration passed.

The three owner-authorized omissions are manufactured-weapon/ammunition damage
from Protective Slime, Babau Darkness and Hezrou Gaseous Form, all with no
substitute. No owner aesthetic approval is claimed; fine visual polish remains
backlog. Full attribution and release identities are in
[Sprint 36 status](planning/SPRINT36-STATUS.md) and
[release evidence](planning/SPRINT36-RELEASE-EVIDENCE.json).
**Stop before Sprint 37.** Historical text below is not current authority.

# Sprint 35 — v0.0.173 RELEASED / STOP

Stable [v0.0.173](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/tag/v0.0.173)
is published and publicly verified from product
[PR #86](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/86).
Barbed Devil and Bone Devil are functional through five untemplated Summon
Monster VII–IX routes. Tested source `e42ae90092dc3f929ffcbda13ebd001281a839dd`
merged at `40296fd8db4b47109ed0f81e4705c4b37ceab0ab`; publication promoted the
exact qualified ZIP without rebuilding. The complete source gate, five grouped
native mechanics/presentation rows, six fresh ON/OFF persistence processes,
highest-risk integration, reproducibility, public downloads and exact laptop
restoration passed. The full attribution and bounded adaptations are in
[Sprint 35 status](planning/SPRINT35-STATUS.md) and
[release evidence](planning/SPRINT35-RELEASE-EVIDENCE.json).

The final audit retains its original aggregate FAIL and a separate reader PASS:
all immutable facts were passing, but the first reader used the `Length` of the
one-element Working/baseline arrays. Two launch-owned Unity session values and
the sound-bank creation timestamp were restored to intake before the complete
fact set was recorded. No Sprint 35 game, lease, task, transaction or staging
remains; fifteen stale pre-intake journals are preserved, not claimed as owned.
Steam remains running. No owner aesthetic approval is claimed. Fine cosmetic
polish and the disclosed unsupported SLA/Wall geometry gaps remain backlog.
**Stop before Sprint 36.** Historical text below is not current authority.

The following closed Sprint33 record is historical; its artifacts, models and
Succubus drain investigation remain closed.

# Historical Sprint 33 - v0.0.171 RELEASED / STOP

[Stable release](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/tag/v0.0.171) is published as Latest and its actual public ZIP,
manifest/checksum, embedded DLL/MVID and remote tag/source lineage are verified.
Product [PR #79](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/79) merged at `6cfd8a92d5354880dc87f9683816106799c53add`.
Tested/promoted source: `fb8d2431b3d41558f4689cc2227f39921ad6d0cc` in wt/s33; exact qualified bytes preserved.
This follow-up changes records only. [Sprint 33 evidence/status](planning/SPRINT33-STATUS.md)
is the current concise coverage, artifact, restoration and deferred-polish map.

Distinct original Invisible Stalker and Shadow Demon bodies are shipped; Succubus
body and all three icons preserved. Native temporary drain repaired and proven,
all twelve existing routes preserved, six fresh-process ON/OFF stages completed
with explicit original-artifact ON reuse, final combined15/15 and full2759/2759 PASS.
Exact reproduction and public download verification passed without a waiver.
Original v117 installation, Working bytes/metadata, protected saves/settings and
references restored; no game or active lease. No personal owner aesthetic approval
claimed. Fine cosmetic polish remains deferred; v167/v168 NOT_RUN debt remains.
**Stop before Sprint34.** Historical text below does not authorize further work.

## Historical v170 repair (preserved)

# v0.0.170 RELEASED / STOP

October29: owner approved the art/repair and authorized merge, push and release.
[Stable v0.0.170](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/tag/v0.0.170)
is published and verified as Latest from `43abeedfd223e95dec33a3556cc5606a029e4cd3`
(PR80 repair and PR81 package-count correction). The full2,759-test gate passed;
two exact-reference builds produced identical ZIP/DLL hashes and preserved the
primary archive. Public download, checksums, manifest, embedded Info/DLL/MVID and
remote tag passed verification at12:37:10.916UTC. Publisher723.177s; public
verification11.930s. The first392.520s attempt's packaging failure remains FAIL.

ZIP SHA256 `49811ef42e9f3205749f59f7989485f7525000049a15e81f6da71c396f93a723`.
[Release identities and commands](planning/SATYR-LILLEND-RELEASE170-EVIDENCE.json).
Native presentation remains reviewed reuse of its original development artifact;
no release DLL native execution is claimed. Affected cold save/reload stays
NOT_RUN and prior disclosed gaps remain open. Publication deployed nothing,
launched no game and wrote no save. No Sprint33 or further campaign is selected.

## Original repair checkpoint

Original October29 reviewable checkpoint: Satyr walking repaired; Lillend surfaces
rebuilt on the same rig. Native presentation PASS53/53, focused domain9/9,
Blender export3/3; exact installation/settings/save intake restored. Owner art
review now approved; affected cold save/reload deferred by the validation ceiling.
Tested source `44281097448efa9236f20faa2dfd74ee4dd85072`; retain its exact
package/receipt and original failed observations. Review branch
`codex/satyr-lillend-presentation-repair`, accepted master `71ea55cda5`.
See [the active repair record](planning/SATYR-LILLEND-PRESENTATION-REPAIR.md).
Local gallery/packet paths and exact identities are in the repair record.
The original checkpoint's no-publication boundary is superseded by this approval.
Do not resume the concluded Sprint32 release or start Sprint33.


# Sprint 32 - RELEASED / STOP

Stable [v0.0.169](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/tag/v0.0.169) is published and publicly verified from PR74 source
`d4e1f656971bd2010110c331fa51f65cdf919f7b`. PR77's tooling fix is accepted on master.
The owner waived further release procedures; two-build reproducibility is
NOT_RUN_OWNER_WAIVED. Existing product/native qualification retains its identities.
Protected intake is exact; no owned live transaction remains.
The sole active record is [planning/SPRINT32-STATUS.md](planning/SPRINT32-STATUS.md).
STOP before Sprint33. Do not restart waived testing from historical instructions.

Everything below is historical and does not select current work.

## Historical resume records

# v0.0.168 Sprints 30-31 follow-up — RELEASED / STOP

Stable [v0.0.168](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/tag/v0.0.168)
is published (2026-10-25T18:36:51Z, Latest) from merged master
`b306f85b5543fcaf9592e78f223f3e98455173b8` (PR #75), by the ordinary
`Publish-Release.ps1` route with two deterministic builds. Asset
`KingmakerGunslinger-0.0.168-expanded-summoning-sprints30-31-followup.zip`,
SHA-256 `b9cf5e2b8b97877d4d595c71c92cda0040ec10cc4636eea8d86ef66d39e9ace6`,
contained DLL SHA-256
`6f7e92fc5cce285d56f60780141eb30772f40388c497b9bd91f71d70f32e3340`, 416 files.
The downloaded asset, `SHA256SUMS.txt`, release manifest, contained DLL and
`Info.json` (0.0.168) and the tag's commit were checked after publication.

The owner's October 25 reviews of released v0.0.167 found three rules defects
and asked for model refinement and a fixed Hound sword grip, then authorized
finalization, merge and release without another approval. Work branch
`codex/s30-31-rules-and-model-refinement`, lab machine DATA, base master
`48e6fb4abf`. Full record:
[review follow-up](docs/SPRINT30-31-REVIEW-FOLLOWUP.md); notes:
[v0.0.168](docs/RELEASE-NOTES-0.0.168.md). No further work is selected.

- **R3 fixed:** the Astral aura is a lesser globe through the native effect
  dispatch and persistent-area membership, not spell resistance; native
  astral-core PASS 13/13. The native Movanic Deva aura implements no globe.
- **Grip fixed:** measured in game; hound-armed 9/9, hound-disarm 12/12.
- **Public surface unchanged:** 1,150 generated placements plus 29 native
  wrappers make 1,179 visible choices, as released in v0.0.167.

Earlier checkpoint summary (same branch):

- **R1 fixed:** Trumpet grants the native Airborne fact; native
  defense-consumers PASS 51/51 (KMG-only).
- **R2 fixed:** the Astral's 20-foot aura carries the configured circle control
  protection; native shared-circles PASS 20/20 with new reach probes.
- **Models rebuilt:** Lemure, Dretch, Hound Archon. Native outsider-bodies
  PASS 12/12, hound-armed 9/9, hound-disarm 12/12; Working-save refinement
  packets and cleanup PASS for all three.
- **Classified, retained failures:** the full-stack defense reset (Call of the
  Wild) and the creature-review motion check (accessory renderer); both
  reproduce on unchanged master.
- **Not run for v0.0.168:** cold-load, public routes and final integration.

v0.0.167's released records below stay as published.

# Sprints 30-31 - RELEASED / STOP

Stable [v0.0.167](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/tag/v0.0.167) is published from tested source `07166abedd17ec1abcf3a4806cd0939ff5c543c9`.
Product PR66 merged as `272b729f4094ce7bb5fa1ea9495c00d66f7d2a1d`. Actual downloaded ZIP/manifest/checksums,
contained DLL/MVID, source/merge/tag lineage and stable release state are verified.
The laptop and original Working bytes/metadata are restored exactly.

The owner directed release with three final executions explicitly deferred:
all 30 public routes/body/icons, standalone integration and high-risk combined
integration. They remain NOT_RUN; acceptance is
OWNER_ACCEPTED_WITH_DEFERRED_FINAL_CHECKS, not complete qualification.
The 2,747-check complete gate, six fresh persistence processes /149 assertions,
and 39 scoped reused complete mechanics cases retain their actual provenance.

No rebuild or game launch was used for publication or final records. Do not
resume old build/run instructions below. No Sprint 32 or new work is selected.
See [current status](planning/SPRINT30-31-STATUS.md),
[coverage](docs/SPRINT30-31-COVERAGE.md), and
[release identities](planning/SPRINT30-31-RELEASE-EVIDENCE.json).

## Prior pilot checkpoint (historical, superseded chronology)

# Active mission — Sprints30–31 combined pilot

Continue the owner-authorized six-creature batch through one qualified product merge/release, public verification, exact restoration and final-records PR; STOP before32. Ordinary checkpoints are milestones. No pending owner question.

Product root `C:/Dev/KingmakerGunslingerLab/wt/s30-31`; branch `codex/expanded-summoning-sprints30-31-outsider-celestial-pilot`; draft PR66. Only HOWARD-LAPTOP/Gunslinger lab. Released master114564ac includes stable164; next patch165 is not reserved. Preserve newer descendants.

Authoritative concise scope, abilities, coverage, artifact tuple and remaining work: [planning/SPRINT30-31-STATUS.md](planning/SPRINT30-31-STATUS.md). Older complete tuples/failures are retained in its immutable checkpoint links and local raw records.

Latest built source36500cf1/package159; counters159 completed packages/177 actual launches. ON persistence prepare177 failed before saving at a first-Heal-only spend assertion; all original Working bytes/metadata and protected intake restored, all tasks removed. Native IL shows Spellbook.SpendInternal searches equivalent preparations in reverse and AbilityData.Equals ignores ParamSpellSlot. The corrected observer requires/records exactly one of two native Heal slots debited and every other slot unchanged; it never writes availability. Fresh build and ON envelope are next, then separate OFF envelope.177 lacks the needed full delta and remains FAIL. Complete tuples and limits are in the map.

Publisher batch extension remains unimplemented. Six-owner persistence is integrated; cold cleanup/absence and OFF stages have not run. No pilot persistence save has been written. Main-only publisher drafts are planning, not evidence. Remaining creature action/cancellation/public/final requirements stay explicit in the map.

Use the sanctioned push wrapper and guarded limited Howard task wrappers. Keep source frozen during builds/native transactions. Current original intake observations: human0.0.117, Mods967/saves118/references18/settings83/tasks0/processes1Steam; recapture actual intake each transaction. Never alter baseline/other saves, Steam authentication/cloud/network or other labs. Retain fresh combat/profile/cold-load boundaries. Check disk headroom and preserve every unique archive/log/raw failure.

Read docs/VALIDATION-POLICY.md when selecting checks. Retain all owner rules decisions and canonical shared Magic Circle. One concise batch map, one final publisher contract, one combined final gate. No repeated donor/articulation census, generic testing framework, unqualified public exposure or personal aesthetic approval.

Offline attempt516f036f (19:01:56.747-19:01:58.729Z) stopped in the new source-only outer-route test: its Boolean ExitAfterCompletion argument was mistakenly supplied as a switch. No compilation, package, game or save occurred; original failure retained and exact Compare PASS/task removed. The invocation now passes the explicit Boolean; all selected tests and compilation remain required.

Offline attempt ae7b2fee (19:03:20.917-19:05:51.510Z): purpose/outer routing11, persistence30 round trips/280 refusals and installed-stack47 predicates PASS. Selected domain47/48 passed; the independent older alignment table still expected TrueNeutral for the five new species. Its explicit legal alignment cases are now extended (LemureLE, DretchCE, Hound/TrumpetLG, AstralNG), preserving all old cases. No production compilation, package, game or save; exact Compare PASS/task removed. Unchanged PS proofs may be reused, but the affected domain check and compilation must rerun.

Offline c5e78003 (19:07:55.363-19:10:50.479Z): all48 selected domain checks PASS; production compilation caught the missing explicit project include for the new observer file (CS0103 callers). The include is corrected. No package/game/save occurred; exact Compare PASS/task removed. Retain prior48-domain and unchanged PS proofs with original identities; the next focused build must compile the actual observer and pass package guards.

Offline b259cf6d (19:12:37.253-19:15:31.711Z) passed the selected scope check and reached actual observer compilation. It caught two private builder-prefix accesses and an Ability-versus-AbilityData mismatch. Observer lookup now uses the audited existing registry keys and the same native AbilityData(ownedAbility) constructor as the qualified Astral observer. No gameplay code change, package/game/save; exact Compare PASS/task removed. Recompile/package remains required.


## Released master records (historical)

# Druid duplicate rows and giant sizes — v0.0.166 RELEASED / STOP

**PR #71 is MERGED and v0.0.166 is PUBLISHED.** After v0.0.165 the owner
reported that a prepared Summon Nature's Ally I or II listed every creature
twice, and that the giants all stood at one height and took the same space
although Cloud and Storm Giants are Huge. On 2026-10-24 the owner wrote "Could
you go ahead and push, merge, and do a new release of this?" No identity or
route changes, so the public equation stays **1,127 generated placements + 29
retained wrappers = 1,156 visible choices**. See
[the state record](planning/SUMMON-DRUID-DUPLICATES-GIANT-SIZES-STATE.md).

- **Merge.** PR #71 merged normally as
  `abd737c22b2f918a9c792b82e7c4172dfa62fcf6`. Between the gated candidate
  `5d2760c091` and the merge, only the unpackaged record `af117865cc` changed.
- **Gates on the candidate.** The Sprint-level gate passed 2,742 / 2,742 with
  the v0.0.166 release validator, the exact-reference build and strict package
  validation (package `71cce0f2…`). The guarded visual contracts passed 15/15
  and the creature review, with the Fire, Storm and Cloud Giants, 38/38 on that
  package. The Druid and giant-size observations are bound to development
  candidate `9e754dd2a9` (giant lineup 74/0; menu-adapters 147/3 with only the
  known Feral Hunter hover stop, host interrupted by a memory kill and the
  machine restored by hand).
- **Publication.** `Publish-Release.ps1` ran once from the clean, pushed main
  checkout (`master` fast-forwarded to the merge) with the cleanup2 reference
  bundle, in 609 s. Its build passed 2,742 / 2,742 domain tests, the
  release-mode wrapper with
  `validate_expanded_summoning_druid_giant_sizes_release166.py` and strict
  package validation, and both clean builds produced identical bytes.

[Stable v0.0.166](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/tag/v0.0.166)
is published (2026-10-24T15:50:15Z) and Latest, titled "Tabletop Added Rules
v0.0.166". Annotated tag object `9cc989fb7b9245a88c16973118770ccb26f112ab`
peels to the merge commit. The downloaded public assets match GitHub's
digests:

- `KingmakerGunslinger-0.0.166-expanded-summoning-druid-giant-sizes.zip`:
  `9f51cb31a0a137c4475217956f06a6077c1a0506d400677493b418b01f22f681`
  (402 files; DLL
  `5dd1be6c13e086b98a4f3b203357b2dfa71526e8f2b92ead03b351bc3ef1e6cd`,
  MVID `3c2a6bfe-19fc-4bfa-837a-8c1c60857ff1`, assembly version 0.0.166.0);
- `SHA256SUMS.txt`:
  `30148ce7318fd0daca870ef324f2bd51d12e7f62621f5d3c304cf3dad62e6b44`;
- `release-manifest.json`:
  `f9652641da7410a2e35dd2cec48f4d5e0e5ce5e60913a53381cca99643c7230a`.

The downloaded ZIP passed strict package validation. Its DLL embeds the merge
commit and `0.0.166-expanded-summoning-druid-giant-sizes`. As for v0.0.165,
the bytes differ from the gated candidate only in build stamps: 401 of 402
package entries are identical, and the DLL's IL differs only in stamps and
layout addresses, with all 188 compiler static-data fields holding identical
bytes. Nothing was deployed permanently: every guarded run restored the live
mod exactly (254 files, `5C3E4AD4…BFBB`) and all 95 saves remain.

**Still open (owner decisions, not release blockers):**
- Owner visual acceptance of the giant heights and Huge space, and of the
  extra Cloud/Storm reach from the space edge (about 0.55 m).
- Native save/reload of a summoned Cloud or Storm Giant (the cold flow is
  pinned to 0.0.163).
- Other Phase 1 procedural icons, such as the six mephits, Owlbear and Tiger.

STOP: no further follow-up work is scheduled.


# Summon cleanup follow-up — v0.0.165 RELEASED / STOP

**PR #69 is MERGED and v0.0.165 is PUBLISHED.** After v0.0.164 the owner asked
for four corrections: contiguous Deinonychus arms, painted Pony and Cyclops
icons, no Summon Nature's Ally I paw row, and no falling ice on the Fire and
Storm Giants. On 2026-10-24 the owner reviewed them and wrote "Go ahead and
merge and release please." No identity or route changes, so the public
equation stays **1,127 generated placements + 29 retained wrappers = 1,156
visible choices**. See [the follow-up state](planning/CLEANUP2-FOLLOWUP-STATE.md).

- **Merge.** PR #69 merged normally as
  `53446777812f18c707ef74cdd787ab4c2fd664df` (tree `28a5259f…`). Between the
  gated candidate `f216160d8e` and the merge, only the unpackaged record
  `8acf138c01` changed.
- **Gates on the candidate.** The Sprint-level gate passed 2,740 / 2,740,
  including the v0.0.165 release validator, the exact-reference build and
  strict package validation (package `20ab0d0d…0542`). The guarded visual
  contracts passed 15/15 and the creature review, with the Fire and Storm
  Giants, 36/36 on that package. The Druid, menu, giant-effect and Deinonychus
  observations are bound to development candidate `05ba0dda49`.
- **Publication.** `Publish-Release.ps1` ran once from the clean, pushed main
  checkout (`master` fast-forwarded to the merge) with the cleanup2 reference
  bundle, in 710 s. Its build passed 2,740 / 2,740 domain tests, the
  release-mode wrapper with
  `validate_expanded_summoning_cleanup_followup_release165.py` and strict
  package validation, and both clean builds produced identical bytes.

[Stable v0.0.165](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/tag/v0.0.165)
is published (2026-10-24T08:06:36Z) and Latest, titled "Tabletop Added Rules
v0.0.165". Annotated tag object `711f163ebaff2c1c9934ba3093657eff5ec4ad2f`
peels to the merge commit. The downloaded public assets match GitHub's
digests:

- `KingmakerGunslinger-0.0.165-expanded-summoning-cleanup-followup.zip`:
  `60f8391d016a03a553e31334f83ddff1777402955c65c1cdeb083adc8b8a6184`
  (401 files; DLL
  `b0ac51791cc742d0c62cb5547eaa1426b61384792140813ab550ea81ce0946ce`,
  MVID `63472c01-b57c-4595-a34c-6e9f2812d344`, assembly version 0.0.165.0);
- `SHA256SUMS.txt`:
  `6c5363b50d3435687dc60d75b8c1152b2ab3388bb20c3f46cc8923055aa058aa`;
- `release-manifest.json`:
  `1315301ab18938493f8ecce7cd8e4ccf2a3049271ecc1c527820722ea9cecde9`.

The downloaded ZIP passed strict package validation. Its DLL embeds the merge
commit and `0.0.165-expanded-summoning-cleanup-followup`. As for v0.0.164, the
bytes differ from the gated candidate only in build stamps: 400 of 401 package
entries are identical, and the DLL's IL differs only in the commit stamp, MVID,
image base and the layout addresses of 188 compiler static-data fields, each
holding identical bytes. Nothing was deployed permanently: every guarded run
restored the live mod exactly (254 files, `5C3E4AD4…BFBB`), all 95 saves
remain, and `KMG_AUTOMATION_WORKING` is unchanged.

**Still open (owner decisions, not release blockers):**
- Owner visual acceptance of the Deinonychus body and the Pony and Cyclops
  icons (the owner authorized the release).
- Other Phase 1 procedural icons, such as the six mephits, Owlbear and Tiger.
- The Deinonychus creature-review summary check's accessory-renderer observer
  limitation, recorded since v0.0.157.

STOP: no further follow-up work is scheduled.

# Summon Cleanup Iteration 2 — v0.0.164 RELEASED / STOP

**PR #67 is MERGED and v0.0.164 is PUBLISHED.** On 2026-10-23 the owner
approved the Cleanup 2 artwork ("The artwork looks great") and authorized the
release once the Pro review's checks were clear: the release validator and
Sprint-level gate, merging PR #67, publishing through `Publish-Release.ps1`
and checking the public download. The release changes art, giant presentation
and summon menus, and no identity or route. The source-derived public equation
is unchanged from v0.0.163: **1,127 generated placements + 29 retained
wrappers = 1,156 visible choices**.

- **Merge.** PR #67 merged normally as
  `23952275bb8bbe5a69a8e64a961dd978093a2ec6` (tree `4a8f1791…`). Between the
  gated candidate `dcfffcd602` and the merge, only the unpackaged record
  `06fb138d9f` changed.
- **Gates on the candidate.** The Sprint-level gate passed 2,739 / 2,739,
  including the v0.0.164 release validator, the exact-reference build and
  strict package validation (package `eb60f199…64ae`). The guarded visual
  contracts passed 15/15 and the creature review 32/32 on that package. The
  first candidate `9a123f9675` failed visual contracts 14/1. The cause was the
  measurement, not the art: it read the Brachiosaurus attack gesture's
  deliberate culling envelope (25.035 m) as the body. `7c300008e1` measures the
  gesture-owned skin at its current pose (9.131 m) and keeps every limit. See the
  [release candidate gates](planning/CLEANUP2-PR67-REVIEW-FOLLOWUP.md#v00164-release-candidate-october-23-24).
- **Publication.** `Publish-Release.ps1` ran once from the clean, pushed main
  checkout (`master` fast-forwarded to the merge) with the cleanup2 reference
  bundle, in 624 s. Its build passed 2,739 / 2,739 domain tests, the
  release-mode wrapper with `validate_expanded_summoning_cleanup2_release164.py`
  and strict package validation, and both clean builds produced identical
  bytes.

[Stable v0.0.164](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/tag/v0.0.164)
is published (2026-10-24T03:47:11Z) and Latest, titled "Tabletop Added Rules
v0.0.164". Annotated tag object `ef6467ee42e084661e0e10dd99637f7e5bf1188e`
peels to the merge commit. The downloaded public assets match GitHub's
digests:

- `KingmakerGunslinger-0.0.164-expanded-summoning-cleanup2.zip`:
  `0a24dd9f8998e43234d64138c481b1969f1f6c671f5bebb2d724f9eb0df0a812`
  (400 files; DLL
  `61c3800017f7c6829310ecd4c63b8d319a0d6946d020be924697e66b2adb5940`,
  MVID `1a40087f-d91a-4952-979d-413d58198578`, assembly version 0.0.164.0);
- `SHA256SUMS.txt`:
  `b4b6b55a2c043f28d32931faff05669899b9b23687e73dedfc1f133f3196642f`;
- `release-manifest.json`:
  `22191e974f1fef021ad6c03e1069457dabcad359dbd5c346ca0ed8f7de1df49d`.

The downloaded ZIP passed strict package validation. Its DLL embeds the merge
commit and `0.0.164-expanded-summoning-cleanup2`. These bytes differ from the
gated candidate because the build stamps the commit and the PDB path. Disassembly
shows the same code: 399 of 400 package entries are identical, and the DLL's IL
differs only in the commit stamp, MVID, image base and the layout addresses of
188 compiler static-data fields, each holding identical bytes. The publisher's
own run qualified the released bytes, and the native evidence stays bound to
the candidate. Nothing was deployed permanently: every guarded run restored the
live mod exactly (254 files, `5C3E4AD4…BFBB`), all 95 saves remain, and
`KMG_AUTOMATION_WORKING` is unchanged.

**Still open (owner decisions, not release blockers):**
- The level-up preview folds a single elemental row behind "Elementals (1)".
- Generated descriptions read "Summons X through the native summon lifecycle."
- Feral Hunter and Preservationist PC menu hovers were last exercised on
  review43; the owner chose not to rerun menu-adapters on the final bytes.
- Owner notes on the Brachiosaurus: the torso reads as a broad dome and the
  tail is short.

See [the cleanup state](planning/EXPANDED-SUMMONING-CLEANUP-ITERATION2-STATE.md)
and [the release notes](docs/RELEASE-NOTES-0.0.164.md). STOP: no further
Cleanup 2 work is scheduled.

# Sprint 29 — RELEASED / STOP

Ettin, Cloud Giant and Storm Giant shipped in stable [v0.0.163](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/tag/v0.0.163) from [product PR #64](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/64), normal merge `c8c7054b3472e433ecb4bc74ae70a961620655ee`, exact tested source `a4c409c7cad4fdab5c83a6205255e52c29212662`. The 2,700/2,700 Sprint gate, 16 fresh exact-package native reviews, six fresh Working processes, publisher preflight and dry run, and actual public-asset download checks passed. All eight SNA roots are live: 1,127 generated roots plus 29 wrappers = 1,156 visible choices. The downloaded ZIP SHA-256 is `82a4d09d6394d47d3adc6a36bb5d7ee55460fe12793382133779b52d47172df4`; DLL SHA-256 `a7e03aeea0e65dafffffc45bf29f4dbb5a3795d83da947e4effa749d6b946b51`; MVID `d30f4d47-6f91-4ff7-ad58-0f87c56388aa`.

The final full protected intake and original Working bytes/metadata are restored exactly; no owned game process, task, lease, staging or permanent development deployment remains. See the [coverage map](docs/SPRINT29-COVERAGE.md) and [release evidence](planning/SPRINT29-RELEASE-EVIDENCE.json) for the qualified RunIds, asset identities, explicit owner scope decisions and visual backlog. Dated development failures below retain their original verdicts. No owner aesthetic approval is claimed. STOP before Sprint 30.

## Dated development history

Exact `c24a7c1b` passed fresh guarded owner-expiration cases: Cloud's CL16 mist and fog areas kept their finite native durations after the owning summon expired; Storm's Call state and commands ended with its owner while the sibling kept Freedom of Movement and independent lightning uses. The first exact Working-save prepare on `c24a7c1b` **failed**: Ettin's two dormant javelins and Storm's dormant bow were lootable, so no save was written. Its original Working bytes/metadata and full intake were restored; the parent's final comparison also threw from a scratch helper stdout/string bug, then was repaired. Exact `0a578514` passed guarded ON prepare with 19/19 native assertions, but ON cleanup failed: eight Ettin second-head renderer material instances survived the destroyed unit/view and were unresolved to the resource observer. No cleanup save or absent run followed. The parent restored original Working bytes/metadata and all protected intake parts. The exact material ownership/teardown repair is qualified by the `dd676afe` ON/OFF runs above. These earlier failures remain dated development evidence.

Exact `2187c8ed` passed the fresh negative and positive RTWP AI review on a guarded development package. Cloud recognized a request-owned Ettin as an ally near the hostile and chose a resolved attack instead of obscuring that ally; Storm recognized electricity immunity and attacked without consuming lightning resources. Positive controls retained Ettin's four attacks, safe Cloud fog, Storm Chain and Call activation, and two separate autonomous granted Call bolts with real saves and damage. Exact `dcca52ef` passed two fresh guarded turn-based phases: 11/11 Awesome Blow open/blocked assertions and 12/12 native AI assertions across Ettin, Cloud and Storm. The `16441221` cost-comparator and `b5978dde` fixture teardown failures remain dated and nonqualifying. Every transaction restored actual protected intake exactly. Cold-load hydration, affected controls, public routes and release remain.

The `4105be52` Awesome Blow native task exited PASS and restored the laptop, but its frame trace exposed a 34.53 m open push for a 3.048 m request. The PASS predicate lacked an upper bound and is **nonqualifying**. Exact `ae770c32` correctly failed its bounded observer: native Bull Rush queued a 42.0624 m margin-dependent chunk before the private 10-foot chunk. Exact `3718fd94` bounded the open push but failed blocked collision/prone. Exact `37312bd2` passed 15/15 native core RTWP predicates. Exact `f7a19f9f` passed 24/24 fresh seven-case RTWP predicates: bounded open and blocked movement, separate victim collision 1d6, real slam damage, prone, failed maneuver, immune, same-size and incorporeal exclusions, and exact native chunk cancellation without a delayed collision. Each native command spent a standard action. The owner approved documenting the installed engine limitation for the obstacle object's own printed 1d6: inspected static navmesh walls have no damageable HP consumer. The victim's collision damage remains implemented. Turn-based Awesome Blow is separately qualified on its recorded artifact. Every transaction restored actual laptop intake exactly.

Cloud Giant, Storm Giant and Ettin are hidden development candidates on branch `codex/expanded-summoning-sprint29-cloud-storm-ettin`. Native profiles, equipment modes, resolved RTWP attacks and turn-based commands passed on their recorded development artifacts. Exact `b8966336` passed 10/10 direct Cloud fog predicates; exact `32f8be8b` passed 13/13 direct Storm magic predicates after the recorded CL/DC repair. Earlier autonomous AI failures and repairs remain dated in the evidence map. Exact `b6dd3752` passed 13/13 fresh RTWP AI predicates: Ettin chose four flail attacks; Cloud chose safe CL16 Fog Cloud; Storm chose Chain Lightning, then activated CL15 Call Lightning and used its granted bolt with a real DC15 save, damage and finite-state debit. No fixture attack or forced roll was injected. The `c81a9941`, `0ec04e74` and `6307228a` item failures remain dated. Exact `ff42de09` passed 14/14 fresh native item predicates: Ettin quantity siblings owned five unique items each; Cloud and Storm equipment, transfer refusal, death/expiry no-loot and exact party inventory passed. Exact `e4ea70d1` passed 13/13 native disarm predicates, including a real two-attack offhand-only Ettin command and Cloud/Storm slam fallback with no item resupply. The first `ad90de97` Awesome Blow native run failed its open-displacement/prone observation after a real successful maneuver and slam hit; the source now normalizes the native push direction and samples the force-move outcome during the action, pending native recheck. All transactions restored actual laptop intake exactly. Negative fog/target cases, later bolt cadence, turn-based AI, Awesome Blow collision, cold-load hydration, affected controls, public routes and release remain. The source-derived public equation is **1,119 generated placements + 29 retained wrappers = 1,148 visible choices**; all eight new SNA roots remain suppressed. See [the current contract](planning/EXPANDED-SUMMONING-SPRINT29-CONTRACT.md) and [evidence map](planning/SPRINT29-EVIDENCE.json). Historical Sprint 28 release evidence remains below.

# Sprint 28 — RELEASED / STOP

Hill, Stone and Fire Giants shipped in stable [v0.0.162](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/tag/v0.0.162) from product [PR #62](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/62), merge `f590aa33ced091506510981598eab184bcb293a9`, tested source `b0f5b4bac9d9c78ab30b7a32697d3960b0890ef6`. The exact downloaded ZIP SHA-256 is `4d806ea4d235db95a945ee93b0af4a47c0b92f50ecd9ee44ddb98b6b021c2f12`; DLL SHA-256 `04b7e0cafeb4f03ec6a6e2ec640bfac5aca88b0eea2b55c802da63d3bb13b6b2`; MVID `0ac0af6e-bad3-41d0-a2bd-73fe4bc5e508`.

All eleven SNA-only roots are public. Nineteen fresh exact-candidate native RunIds and the full Sprint gate passed; the final machine and original Working save match intake exactly. See [current status](planning/SPRINT28-STATUS.md), [release coverage](docs/SPRINT28-COVERAGE.md), and [release evidence map](planning/SPRINT28-RELEASE-EVIDENCE.json). The only owner-approved gameplay omission is named Climb for these three and Fire Craft; a future focused pit/climbing review is backlog. Rough body/color/animation polish is backlog, with no owner aesthetic approval claimed. Sprint 29 was not started. STOP after this final records merge.

# Sprint 27 historical release

Griffon shipped in [v0.0.161](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/tag/v0.0.161) from product [PR #60](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/60), merge 05d75068e2f49a914c5cbeb2499944ff1298273c. The exact qualified source is 63ef0fa4845c7143daa6aa9a1505c059ef61096a and the downloaded ZIP SHA-256 is 7ae697de7b743ba58772a219e105d372a854461007cb4edd12952e11898129de. All six SNA-only roots are public; the source-derived total is 1,108 generated roots + 29 retained wrappers = 1,137 visible choices.

The [current coverage](docs/SPRINT27-COVERAGE.md) and [release evidence map](planning/SPRINT27-RELEASE-EVIDENCE.json) record the full gate, fresh native commands, retained postprocess failure/recovery, reuse audit, public downloaded assets, and exact machine/Working-save restoration. [Development evidence](planning/SPRINT27-EVIDENCE.json) remains historical. HumanReview is NOT_PERFORMED_NONBLOCKING; documented coarse wing/anatomy/transition polish is visual debt.

The final audit found no owned game, task, lease, active compatibility sentinel or staged Mods directory. All 118 saves, including the protected baseline and restored Working fixture, were exact. Sprint 26 remains closed. That historical stop closed Sprint 27; the current Sprint 28 authority and state appear above.
