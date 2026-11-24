# Sprint 7 record: presentation and audio remediation (S7 T1–T7)

Status: **BLOCKED (2026-11-24): review round 7 of 7 on `3e2baea4b4` returned changes_required and the controller stopped
the enrollment at the review cap ("review-round cap reached for final (7/7)"). Owner amendment 3 makes this a mandatory
stop: it authorizes no further review, candidate or resumption. Nothing is merged or released, and no visual or listening
choice has been accepted.**

Round-7 review outcome:
- verified: R5-F3, R6-F1, R6-F2, R6-F3 and R6-F4 (the final collector was executed natively in `…224949Z`);
- still open: R4-F1 (the abandonment drivers' recovery decisions check selected fields, not the complete child outcome,
  so an additional cleanup failure beside the injected one could pass), R5-F1 (a late unit that survives its destruction
  wait, or a further correlated unit, reaches only the probe's problems, not the cleanup outcome ProbeSlot reads) and
  R6-F5 (this record's round-7 tally and harness suffix, corrected below);
- raised: R7-F1 (the evaluator's null test misreads String-typed nulls in memory and accepts an omitted endUtc), R7-F2
  (ordinary Dust commands can be interrupted before their process is retained; one release wraps all command
  interruptions), R7-F3 (a hook or trace constructor whose rollback fails leaves no instance, so its residual never
  reaches the isolation), R7-F4 (a direct cue-source retirement that throws is marked retired and its failure is lost),
  R7-F5 (the Dust driver does not repair its injected leftover if it is itself disposed during recovery) and R7-F6 (the
  settled evaluator does not correlate the selected and terminal cue IDs or consume the emitted stop count).
All are verification-code or record findings; no product presentation or mechanics finding was raised. Correcting the
RuntimeTesting findings would need a fourth candidate and further review, which need a new owner decision.

**Owner amendment 3** (revision 3, sha256 `3321dd31…44e0`; message sha256 `70e46f79…45cf4`; stored outside the worktree at
`<lab>/owner-guidance/S7-OWNER-AMENDMENT-3-2026-11-24.md`) authorized one resumption after the round-6 no-progress stop
(applied with `resume --amend-mission`; the cap stays seven reviews, six spent), exactly one additional product candidate
(three in all) and a combined rounds-6-and-7 validation ceiling of 75 minutes. It keeps every earlier scope decision.
The applicable owner amendments are 1 (revision 1, `59be8e3f…31e0`), 2 (revision 2, `26409cd8…3ab4`) and 3.

**Round 7** addresses the eight findings open after round 6 (matrix in §7): R4-F1, R5-F1, R5-F3 and R6-F1 to R6-F5. All
are verification code or records; no product presentation or mechanics changed. One cumulative cleanup result now governs
the effects phase (cell admission and final restoration) and the probe slot. The third and final candidate
**`9c39b1ff7f`** (package `49f8b88c…6413`, DLL `5969f596…e2c3`, harness `ee7742a4…a373`) ran:
- `…223809Z` effects: 35 of 36 structured checks (r6 16/16). Every new isolation path passed (callback-hook abandonment,
  Dust abandonment with the exact execution observed settled, phase restoration). The callback-loss characterization
  found no overlap in its declared 16 s ("not-both-playing", retained FAIL);
- `…224649Z` Ghaele lifecycle and visibility: PASS;
- `…224949Z` effects, one rerun of the same package: 35 of 36 (r6 16/16). The final collector ran its whole live path. Its
  in-run verdict, "survivor-stopped-by-other-retirement", is retained FAIL and is an evaluator defect (§4.5): in memory a
  null C# string is a String-typed JSON value, which the evaluator's null test misreads. The same evaluator, unchanged,
  over that run's recorded row gives "characterized". Correcting the evaluator would change compiled RuntimeTesting
  bytes after the third candidate, which amendment 3 does not allow; it is the remaining blocker for an in-game PASS of
  that predicate.

Validation spent: 65 min 42 s of the 75-minute ceiling: 39 min 4 s in round 6, 25 min 58 s of round-7 commands, and the
eleven round-7 controller gates (40.1 s wrapper wall time; their receipts record 19.1 s inside the gates). 9 min 18 s of
the ceiling remain unused.

Round-6 stop (historical): after review round 6 on `b37faaf883` the controller stopped the enrollment under its mandatory
no-progress rule ("no progress: R4-F1 open for 3 consecutive reviews"). Owner amendment 3 authorized the resumption.

Round-6 review outcome (decision changes_required):
- verified: R1-F1, R2-F2, R4-F2, R5-F2, R5-F4, R5-F5 and R5-F6;
- **R4-F5: rebuttal accepted.** The reviewer accepted the conditional owner disposition (amendment 2 §4) on the retained
  `…202648Z` trace: both IDs live, only 684 stopped, 656's callback withheld with its ID inactive through the window, its
  component and view observed destroyed on its own retirement, the hook removed. The cause of the historical 497 failure
  remains unknown;
- still open: R4-F1 (the abandoned Dust execution is not carried through the final isolation decision), R5-F1 (the
  negative-control summon is delivered before, not after, the cast owner's disposal) and R5-F3 (the capture producer
  reports a missing EntityFader as visible);
- raised: R6-F1 (failed early Ghaele releases do not reach ProbeSlot), R6-F2 (the callback hook's cleanup outcome does not
  reach the effects phase), R6-F3 (terminal playback reads are not verdict inputs), R6-F4 (the settled evaluation is not
  bound to the collector that ran) and R6-F5 (this record's expenditure and scope rows, corrected below).

At the round-6 stop, R4-F1, R6-F2 and the in-game half of R6-F4 needed RuntimeTesting changes and a third product
candidate, which amendment 3 then authorized. Round 6 spent 39 min 4 s. After the controller stopped the enrollment at the round-5 cap, the owner issued owner amendment 2
(revision 2, sha256 `26409cd8…3ab4`; message sha256 `cc57e9e9…9bb7`, stored outside the worktree at
`<lab>/owner-guidance/S7-OWNER-AMENDMENT-2-2026-11-24.md`). Applied to the stopped enrollment, it left it BLOCKED at the
exhausted cap, as the owner expected. The owner's separate extension (`resume --max-rounds 7`) then raised the cap from
five to seven reviews. Amendment 1 (revision 1, sha256 `59be8e3f…31e0`) and every spent round, finding, failed session and
candidate identity are kept.

Amendment 2 also records two owner acceptances:
- the bounded Ghaele scope: the demonstrated released and current interior observations; the released outdoor/bright case
  stays unexecuted and the October incident unattributed;
- the verified controller-admission disposition: controller rendering stays unexecuted, not PASS.

Verified by the independent reviewer:
- rounds 2–3: R1-F2, R1-F4, R1-F6, R1-F7, R1-F8, R1-F10, R1-F11, R1-F12, R1-F13, R1-F14, R2-F1, R2-F3;
- round 4: R1-F3, R1-F9, R2-F4, R3-F1, R3-F2, R3-F3, R3-F4;
- round 5: R1-F5, R3-F5, R3-F6, R4-F3, R4-F4, R4-F6, R4-F7.

**Round 6** addresses the eleven findings open after round 5 (matrix in §7). None is a gameplay defect, and no product
presentation or mechanics changed:
- two are record reconciliations: R1-F1 and R5-F6;
- eight concern observers and harnesses: R2-F2, R4-F1, R4-F2 and R5-F1 to R5-F5;
- R4-F5 is the impact-cue playback and tracking requirement. Round 6 characterized the missing-callback path (§4.5) and
  claimed the conditional owner disposition of amendment 2 §4; the round-6 review accepted it.

**Validation exception (owner amendment 2 §7, raised by owner amendment 3 §6).** For rounds 6 and 7 together, the owner
authorized up to 60 minutes of cumulative validation-command time and at most two new product candidates (amendment 2),
then 75 minutes and three candidates (amendment 3, which adds 15 minutes and one candidate without resetting the clock).
It is a one-off exception to the 30-minute ceiling; `VALIDATION-POLICY.md` is unchanged.

Spent in round 7 (wall clock, failed attempts included):

| # | Command | Result | Time |
|---|---|---|---|
| 1 | Harness build (uncommitted round-7 source) | PASS | 1.2 s |
| 2 | Controller `verify` (uncommitted) | Refused: gates run on a committed candidate | 1.7 s |
| 3 | Focused gate `sprint7-visual` (local) | PASS, 22 tests | 20.8 s |
| 4 | Harness build (`9c39b1ff7f` source) | PASS, `ee7742a4…a373` | 1.3 s |
| 5 | Focused gate `sprint7-visual` (local) | PASS | 1.7 s |
| 6 | Build-Local candidate 3 (`9c39b1ff7f`) | PASS | 7 min 2 s |
| 7 | Session `20261124T223809Z-…-s7-fx` | FAIL, retained (no cue overlap in the declared search) | 8 min 0 s |
| 8 | Session `20261124T224649Z-…-s7-ghaele` | PASS | 2 min 15 s |
| 9 | Session `20261124T224949Z-…-s7-fx` (rerun, same package) | FAIL, retained (evaluator null typing) | 8 min 9 s |
| 10 | Focused gate `sprint7-visual` (local, test-only addition) | PASS | 5.5 s |

| 11 | Eleven required controller gates on `3e2baea4b4` (118 selected-test executions: 22 `sprint7-visual`, 96 neighbour) | PASS | 40.1 s wall (receipts 19.1 s) |

Round 7 total: 25 min 58 s before the controller's required gates, 26 min 38 s with them; cumulative 65 min 42 s of 75. The game window
(22:30Z–22:58Z) was agreed with the other Kingmaker session on this machine and handed back; every session completed its
transaction and verified the owner files.

Spent in round 6 (wall clock, failed attempts included):

| # | Command | Result | Time |
|---|---|---|---|
| 1 | Focused gate `sprint7-visual` (uncommitted) | FAIL: a test field never assigned (compile) | 18 s |
| 2 | Focused gate `sprint7-visual` | PASS, 20 tests | 4 s |
| 3 | Harness build | FAIL: a local name shadowed the new field (compile) | 2 s |
| 4 | Harness build (`f3bfab1303`) | PASS, `9240a3d6…` | 2 s |
| 5 | Build-Local candidate 1 (`f3bfab1303`) | PASS | 7 min 23 s |
| 6 | Session `20261124T200320Z-…-s7-fx` | FAIL, retained (observer) | 8 min 12 s |
| 7 | Session `20261124T201133Z-…-s7-trumpet-reload` | PASS | 2 min 1 s |
| 8 | Session `20261124T201333Z-…-s7-ghaele` | PASS | 4 min 11 s |
| 9 | Focused gate `sprint7-visual` | PASS | 17 s |
| 10 | Build-Local candidate 2 (`740d94a995`) | PASS | 7 min 15 s |
| 11 | Session `20261124T202648Z-…-s7-fx` | FAIL, retained (observer) | 8 min 16 s |
| 12 | Focused gate `sprint7-visual` | PASS | 24 s |

Total before the controller's required gates: 38 min 25 s. The eleven required controller gates on `b37faaf883` took
39 s, for 39 min 4 s in all, which leaves 20 min 56 s of the exception. Both authorized candidates are used. Restoration was never
interrupted: every session completed its transaction and verified the owner files. Each game window was agreed with the
other Kingmaker session on this machine and handed back at 20:35Z.

- Draft PR #219; one agent-review enrollment (`kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1`, foreground,
  single plan); branch `codex/s7-visual-audio-remediation`.
- Mission: `docs/missions/sprint7/SPRINT7-AUTONOMOUS-MISSION.md` (verbatim, sha256 `c232f135…f56f0`), plus owner amendment 1.
- Amended acceptance (owner amendment 1):
  - The Dust breath keeps its released Sickened behaviour; the mission's "blindness" was a mistake.
  - The Trample/Overrun wording "Target point within twice this creature's speed." is approved.
  - `Sweetener_Low_06_Quake` and `InspireCourage` are kept provisionally; neither is acoustically accepted.
  - Technical audio readiness means cue ownership, timing, retirement, provenance, and a playable private packet for the
    owner. The text-based reviewer is not required to hear audio.
  - The Ghaele conclusion stays bounded, and the October incident stays unattributed.
- Technical status, observed behaviour, unexecuted paths and owner taste are kept separate below. Every visual and
  listening judgment remains the owner's.
- Review history:
  - round 1 (head `0910995241`): 14 findings;
  - round 2 (head `0d976f4fd1`): 4 verified, 10 open plus 4 new;
  - round 3 (head `511160dc6e`): 8 resolved, 6 open plus 6 new, then the controller's no-progress stop;
  - round 4 (head `6095fa4a75`, under owner amendment 1): 7 verified, 5 open plus 7 new.

## 1. Starting point

- Sprint 6 was merged (PRs #198–#216) and released: stable **v0.0.195**, release commit `ef0e1fe15d` (PR #217), records
  PR #218. Post-release `origin/master` is **`35763430ac`**, the base of this branch.
- Released package `456d504d…9444`, DLL `3ef052ea…6a6b`.
- Environment: disposable loader-compat game copy, Unity Mod Manager owner profile 0.33.0 (its `0Harmony12` is a shim over
  Harmony 2), no other mods.
- Harness `KmgLoaderCompatHarness.dll` from this branch, by round:
  - round 1 `04c9d1d4…`; round 2 `9e90d22c…`; round 3 `3d86bd3a…`;
  - round 4 `dcdf4574…` (first runs, retained) and **`71b0c6f03de48fce27d3e6d8ef384769b524e09b3c47f21b740b1bfdb7475701`**
    (final).
- Blender 4.5.10 `8b15ef73…313c` with `PYTHONHASHSEED=0`.
- Carried forward unchanged and outside this sprint: Sprint 6 accepted scopes, REP-05/REP-06 BLOCKED, VER-02 open with
  G5 fixture-blocked, and the other limits in `docs/GUNSLINGER-SPRINT6-DISPOSITION.md`.

## 2. Outcome

| Story / record | Original problem | Result | Evidence | Technical status | Retained limitation / owner decision |
|---|---|---|---|---|---|
| S7 T1 / REP-04 | October review: "increasing" Ghaele dissolve; a scripted move that did not start | **No product change.** On released v0.0.195 and on the candidate, an ordinary Ghaele keeps its body (cutout 0.00) to its native 120 s expiry, moves, and fights. Its raw `_Dissolve` is the native looping non-cutout animation the unsummoned donor also runs: 11/11 positive samples per package, with driven body-material identity recorded. | Harness `…182757Z` (released) and `…183206Z` (candidate) in round 5; `…162819Z`/`…163235Z` in round 4; product `…050957Z` `gh`/`ghd` (round 3) | Current-build disposition, bounded to the observed packages and conditions | Released baseline observed only in the admitted fixture's interior throne room. The outdoor/bright released case has no admitted route (§4.1). **October incident unattributed.** Bounded scope accepted by owner amendment 2. |
| S7 T2 / VIS-01 | 48 zero-area wing-root triangles (and inside-out welded membranes) | **Repaired at the generator.** 0 zero-area triangles, 0 repeated edges, all parts outward | `ed88ad994e`; export receipt; models `…162110Z` (round 4, party views bound to the body) and `…050957Z` | Implemented and verified | Owner visual acceptance |
| S7 T3 / VIS-02 | 19/12,924 normal corners at a waist pleat and the left-hand fingers | **Repaired from the exact private input.** 16 → 0 opposing corners | `d864e7b01f`; export receipt; `…162110Z`, `…050957Z` | Implemented and verified | Owner visual acceptance |
| S7 T4 / VIS-03 | Owned Trample/Overrun windows: "Target point within 0 ft. feet." and school "None" | **Scoped presentation override:** "Target point within twice this creature's speed." (owner-approved wording), no school | `2614ea3bd6`; `…050957Z` `ui` | Implemented and verified for desktop fact and HUD | **Controller mode unexecuted** (§4.4): no admitted route creates or selects a summon in the console UI. Not a controller PASS. |
| S7 T5 / VIS-04 | No added impact sound on the Ankylosaurus tail strike | **One owned cue per resolved tail roll on the roll's own in-game target** (`Sweetener_Low_06_Quake`, provisional) | `90bc2ae7eb`; fx `…180635Z` (round 5), `…170720Z` and `…061437Z`; concurrent retirement also passed in round 6 (`…200320Z`, `…202648Z`) and round 7 (`…223809Z`, `…224949Z`) | Cue ownership and timing verified; private packet delivered | **R4-F5 disposition accepted by the round-6 review** (amendment 2 §4): under synthetic callback loss playback ends normally, no other source is affected, and the residual tracking belongs to the survivor's own source and ends with its disposal. The cause of the `…164749Z` missing callback stays unknown (§4.5). **Listening and choice pending (owner).** |
| S7 T6 / VIS-05 | The Dust Mephit breath shows the ice cone | **Owned per-instance dust recolour** of the one cone CreateView spawns for the executing owned Dust breath, now bound to that native spawn call; restored on release; released mechanics (Sickened, damage, save, enemies only, action) unchanged | `9733a0c9b7`; fx `…180635Z` and `…170720Z` | Implemented and verified | Owner visual acceptance. Allies' harmless saves stay documented. |
| S7 T7 / VIS-06 | Provisional `InspireCourage` cue on the Trumpet Blast | **Kept provisionally** (no code change); six alternatives sampled; cue verified across an in-process save and load. The current probe (`…201133Z`) marks only the observed blast's own save passed through its `AutoPass` field; all dice roll natively | `r6` in `…180635Z` and `…170720Z`; Trumpet reload `…182558Z` (round 5) and `…162605Z` | Cue technically verified; private packet delivered | **Listening and choice pending (owner).** |

## 3. Candidates, sessions and identities

**Round 7 (owner amendment 3).** No product change: only `RuntimeTesting/`, the harness, the tests and the sessions changed.
- **Candidate 3** `9c39b1ff7f95c995ead0ecdfca2e679e62506825`: package
  `49f8b88c052ca448b214552a6aab7a0e615697c10ecbea842ae013456faf6413`, DLL
  `5969f596811984eaaf69014bc3bc8ddb39d733d8837cce280a76655abd98e2c3`.
- **Harness:** `ee7742a493eb3879cffa30c69879592a51da60a3b7205a902832d8e4395ea373`, built from the `9c39b1ff7f` source.
- **Collector and evaluator:** the effects collector is `RuntimeTestRunner.Sprint7Presentation.cs` at `9c39b1ff7f`. Its one
  callback-loss evaluator is `Sprint7ObserverPolicy.CallbackLossSettled` at the same commit. Later commits change records
  and tests only; RuntimeTesting is unchanged after the candidate.
- **Counts:** each round-7 effects extract has 36 structured checks.

| Session | Result |
|---|---|
| `20261124T223809Z-…-s7-fx` (fx, r6) | FAIL, retained: 35/36, r6 16/16. `s7-anky-callback-loss-characterized` found no two cues confirmed playing at once within the declared 16 s (607 ended 13.6043 s, 641 posted 13.6023 s; the per-frame selection did not land in the 2 ms overlap). Every other check passed, including `s7-anky-callback-abandoned-unwound`, `s7-dust-abandoned-unwound` and `s7-effects-owned-state-restored`. |
| **`20261124T224649Z-…-s7-ghaele`** (lifecycle, visibility) | **PASS** |
| `20261124T224949Z-…-s7-fx` (fx, r6; rerun of the same package) | FAIL, retained: 35/36, r6 16/16. The callback-loss collector reached overlap and ran its whole live path; the in-run verdict misread the survivor's null stop fields (§4.5). Every other check passed. |

Reused at their original identities, not rerun: models, desktop UI, Trumpet reload (`…201133Z`, harness `9240a3d6…`; the
round-7 harness changes the Trumpet probe only by recording destruction requests in its cumulative outcome), the audio
packet and the full Ghaele lifetimes (`…182757Z`, `…183206Z`, `…201333Z`).

**Round 6.** No product change: only `RuntimeTesting/`, the harness, the tests and the sessions changed. Product code
outside `RuntimeTesting/` is identical from the round-5 head `091446a079` to the round-6 head.
- **Harness:** `9240a3d633a801c86775eb678d7be2818190e7f10cb9b5e39e8485f62cdb08a0`, built from `f3bfab1303`.
- **Counts:** each effects extract has 34 structured checks. The runs also emit a plain-text native-profile restoration
  line, which is not counted.

| Candidate | Package / DLL SHA-256 | Session | Result |
|---|---|---|---|
| `f3bfab1303` | `a6ac5d79f2d794ea5f9707c003885160c0392b0f6f1036b49e806de3a59d0017` / `270f7aa4d897274808c4523e654a0c468f354cac37c250f60a30eefc7975c355` | `20261124T200320Z-…-s7-fx` (fx, r6) | FAIL, retained (observer). `s7-anky-callback-loss-characterized` latched the other source's cue once seen; it had ended before the survivor's cue posted ("not-both-playing"). The other 33 structured checks and r6 16/16 pass. |
| `f3bfab1303` | as above | **`20261124T201133Z-…-s7-trumpet-reload`** | **PASS** |
| `f3bfab1303` | as above | **`20261124T201333Z-…-s7-ghaele`** | **PASS** |
| `740d94a995` | `e0eda5b9ee7bde2b6c6bacb5f6167dc531cc0c470091182665fc5b8b0c55b7a7` / `f145ed84917be449c1ca994b838644f0a270d262a69553dc15f786535c9a36cb` | `20261124T202648Z-…-s7-fx` (fx, r6) | FAIL, retained (observer). Both cues were confirmed playing, but the predicate read the other source's stop in the retirement frame, before the product requests it in that view's destroy callback ("other-not-stopped-by-own-id"). The other 33 structured checks and r6 16/16 pass. The offline re-evaluation of this run's own records is in §4.5. |

What the round-6 head changed after these runs (historical; superseded by round 7):
- `c9c4c33154` made the in-run predicate read the other source's retirement once settled (`RetiredByOwnId`), as the
  normal-callback control does.
- That observer change was not natively exercised in round 6. Round 7 replaced it with the one settled evaluator,
  natively run on candidate 3.
- The Trumpet and Ghaele sessions ran on candidate 1. Candidate 2 changed only the callback-loss observer, which they do
  not use.

**Round 5.** No product change: only `RuntimeTesting/` (`ac96bab4c2`), the harness (`ac96bab4c2`, `c151cdfdc5`), the
tests and the sessions changed. Product code outside `RuntimeTesting/` is identical from `1ac4324617` to the round-5
head.

| Candidate | Package / DLL SHA-256 | Harness | Session | Result |
|---|---|---|---|---|
| **`ac96bab4c2`** | `1bca116c5c0a0a47cc3a29ae88fb87b2b564d8b5f7c9af03c165a74faf1e851d` / `f8fd7251925fe3a5ad5c90ac2de37373e6b74d6f67f5090cba9f88d962423545` | n/a | **`20261124T180635Z-…-s7-fx`** (fx, r6) | **PASS 2/2** (fx: 33 structured checks in the extract, all passing; the plain-text native-profile restoration line is not counted; r6 16/16), 7 min 58 s |
| `ac96bab4c2` | as above | `5d39a60b…` | `20261124T181432Z-…-s7-trumpet-reload` | FAIL, retained (observer): the probe's own nested foreign save inherited the blast's rule context and was treated as owned, which also shifted the following draw. 1 min 37 s |
| released v0.0.195 / `ac96bab4c2` | `456d504d…` / `1bca116c…` | `5d39a60b…` | `20261124T181609Z`, `20261124T182018Z` `-s7-ghaele` | FAIL, retained (observer): everything held, but the target's destruction was read in the same call as `Destroy()`. 4 min 9 s each |
| `ac96bab4c2` | as above | **`5d2350f83f965bfba45c87f64c06a64f70631a63ce72c1507f8166a6d6bba545`** | **`20261124T182558Z-…-s7-trumpet-reload`** | **PASS**, 1 min 59 s |
| released v0.0.195 | `456d504d…9444` / `3ef052ea…6a6b` | `5d2350f8…` | **`20261124T182757Z-…-s7-ghaele`** | **PASS**, 4 min 8 s |
| `ac96bab4c2` | as above | `5d2350f8…` | **`20261124T183206Z-…-s7-ghaele`** | **PASS**, 4 min 10 s |

Round-5 extracts: `docs/evidence/sprint7/r5/`. Models are not rerun: `R6Cell`'s ordinary path is unchanged, so the
`…162110Z` models evidence (`a07716e3ab`) stands.

**Round 4.** Product code changed only in `9733a0c9b7` (Dust call-site binding). Every later commit changes only
`RuntimeTesting/`, the harness, the tests and the sessions; product code outside `RuntimeTesting/` is identical from
`1ac4324617` to `bc67438f3b` (`git diff` empty).

| Candidate | Package / DLL SHA-256 | Harness | Session | Result |
|---|---|---|---|---|
| `1ac4324617` | `fcc8ee81…1e0e` / `d826ae76…5335` | `dcdf4574…` | 5 launches refused before acquiring anything (harness `Info.json` missing from the build folder) | refused, nothing acquired |
| `1ac4324617` | as above | `dcdf4574…` | `20261124T154136Z-…-s7-fx` | FAIL, retained (observer): the first Dust and Ice Mephits reached their native 120 s expiry during the added steps, and the early-exit cast only refreshed an existing Sickened. Nested interruption PASS. 7 min 52 s |
| `1ac4324617` | as above | `dcdf4574…` | `20261124T154929Z-…-s7-models` | FAIL, retained (observer): the setup-failure trace rollback read a never-patched method through the Harmony12 shim. Party captures, failed-normal-disposal case and dim phase PASS. 4 min 53 s |
| `1ac4324617` | as above | `dcdf4574…` | `20261124T155421Z-…-s7-trumpet-reload` | PASS (superseded by `…162605Z`), 2 min 17 s |
| released v0.0.195 / `1ac4324617` | `456d504d…9444` / `fcc8ee81…` | `dcdf4574…` | `20261124T155639Z`, `20261124T155845Z` `-s7-ghaele` | FAIL, retained (observer): every ownership readback held; the bystander move had ended on its own before it was read. 2 min 7 s, 2 min 0 s |
| `a07716e3ab` | `4dc92312a19670b116fd2482b9083312261b01124f8c6d99bfb52096cebec41a` / `c26b7aa02b5650e6277c1ae208efbc58baa0cd9193356d9ed1ab3bb53369857f` | `71b0c6f0…` | `20261124T161344Z-…-s7-fx` | FAIL, retained (observer): the fresh Dust Mephit's Sickened refreshed an instance the cell's own Ice breath had introduced, which the ledger did not own. 7 min 25 s |
| `a07716e3ab` | as above | `71b0c6f0…` | **`20261124T162110Z-…-s7-models`** (mod, modd) | **PASS 2/2**, 4 min 56 s |
| `a07716e3ab` | as above | `71b0c6f0…` | **`20261124T162605Z-…-s7-trumpet-reload`** | **PASS**, 2 min 13 s |
| released v0.0.195 | `456d504d…9444` / `3ef052ea…6a6b` | `71b0c6f0…` | **`20261124T162819Z-…-s7-ghaele`** (lifecycle, timeline) | **PASS**, 4 min 16 s |
| `a07716e3ab` | as above | `71b0c6f0…` | **`20261124T163235Z-…-s7-ghaele`** | **PASS**, 4 min 19 s |
| `2e2737f8c6` | `620d04e9…8c53` / `e4672be5…7952` | `71b0c6f0…` | `20261124T164749Z-…-s7-fx` (diagnostics) | FAIL, retained: the same Sickened ownership gap, with diagnostics; and a lost native end callback for the surviving Ankylosaurus cue (cause unknown). 8 min 42 s |
| **`bc67438f3b`** (final) | `7adde59cce585da3543a849c010ba186dbbca3407c8a67bfb27df6c5f0d2d894` / `aeb633f7d82ecb7d81a188dd80da98a2ea279723d958b8b7ad41939c04829fd2` | `71b0c6f0…` | **`20261124T170720Z-…-s7-fx`** (fx, r6) with loopback audio | **PASS 2/2** (fx 31/31, r6 16/16), 7 min 16 s |

Identity binding:
- Effects and R6 bind to `bc67438f3b`.
- Models, Trumpet and candidate Ghaele bind to `a07716e3ab`. From `a07716e3ab` to `bc67438f3b`, only the effects observer
  changed.
- UI and the product Ghaele phases bind to round 3's `6f2f16ca88`. Their dependencies are unchanged, so they were not
  rerun.
- Every session completed its transaction, applied and restored game settings, and verified owner saves and the
  installation; the outer receipts are in `docs/evidence/sprint7/r4/*-session.json`. The five refused launches acquired
  nothing.
- The human installation was never touched. The game-copy window was agreed with the peer session sharing this machine,
  and released afterwards.

Round 3 (retained): `6f2f16ca88` (`8039a627…` / `d7088878…`) core `…050957Z` PASS 5/5; Trumpet reload `…052423Z` PASS;
Ghaele `…052619Z` (released) and `…052954Z` PASS; `f564cbb844` (`d4ded9a8…` / `192c9726…`) fx `…061437Z` PASS 2/2; failures
`…050257Z`, `…054209Z` and `…055920Z` retained. Rounds 1–2: `docs/evidence/sprint7/r2/MANIFEST.json`.

Builds: `Build-Local -DevelopmentPurpose s7-visual-audio -DevelopmentTestFilter sprint7-visual` with the pinned reference
bundle. Each took 7–8 min, passed strict package validation, and deployed nothing.

Expenditure. The round-4 cost proposed before any native work was 1 build and 6 sessions, about 42 minutes. The actual
cost was higher because four observer defects needed new candidates; every failed run is retained above.

| Round | Builds | Sessions | Game-copy time |
|---|---|---|---|
| 1 | 5 | 5 | 68 min |
| 2 | 4 | 6 | 68 min 9 s |
| 3 | 4, plus 1 refused at the clean-commit check | 8 | 51 min 57 s |
| 4 | 4 candidates and 2 harnesses | 12, plus 5 refused before acquisition | 58 min 16 s |
| 5 | 1 candidate and 2 harnesses (proposed: 1 and 1) | 7 (proposed: 4) | 28 min 10 s (proposed: about 21 min) |

Reviewable evidence (committed):
- `evidence/sprint7-review/`: 94 curated stills, including the 12 added in round 4 (party views bound to the body, and the
  released/candidate Ghaele idle and combat views). `INDEX.json` gives each lab original's SHA-256, the committed file's
  SHA-256 and its source session. PNGs are re-encoded as JPEG quality 92.
- `docs/evidence/sprint7/r4/` (and `r3/`, `r2/`): sanitized extracts of every launch and of each session's outer receipt;
  `MANIFEST.json` gives each raw source's SHA-256.
- `docs/evidence/sprint7/exports.json`: generator, input and output identities.
- `docs/evidence/sprint7/audio-context.json`: round-3 excerpts, with the distance labels corrected in round 4.
- `docs/evidence/sprint7/audio-context-r4.json`: the round-4 packet manifest (§4.5).

The private listening packet is described in §4.5. Game audio is never committed.

## 4. Per story

### 4.1 T1 / REP-04 Ghaele

**Round 7 (owner amendment 3; `…224649Z`, candidate `9c39b1ff7f`, harness `ee7742a4…`).**
- **Lifecycle (R5-F1, R6-F1).** The positive pending case held again: command pending and loan held before disposal, the
  witness saw no owned or unattributed summon, verdict null. The new post-disposal negative control ran in this order:
  queued; before disposal (zero owned results); cast owner disposed; disposable unit registered with its owner; result
  delivered to the witness handler by a labelled controlled notification injection, a never-triggered `RuleSummonUnit`
  whose context is a fresh execution context of the abandoned cast's own AbilityData. The witness matched it by
  ability-data. The verdict, computed from the case's own readbacks, was "late-owned-summon". The foreign same-caster
  control was kept, the witness unsubscribed, and the late unit was requested once and destroyed. In the real-spawn case
  the witness matched the cast's own summon by its retained execution.
- **Visibility (R5-F3), a short current capture.** The Ghaele's row read its own fader present (component identity
  recorded) and visible, and qualified. The disposable donor, with its fader set hidden by the probe (held, restored),
  was refused as "fader-hidden". A missing component is exercised through the same producer and verdict offline only:
  removing a live view's fader was not attempted.
- **Retained lighting rows.** No retained row recorded the subject's component presence; their `faderVisible=true` came
  from the shared helper's fallback, which also reports true without a fader. Under the corrected reader they are
  "fader-presence-unrecorded", while every other readback still qualifies, and their recorded in-run values are kept.
  This limits only the fader-presence part of those lighting observations. The accepted bounded body, movement, combat
  and expiry observations stand.

Harness probe `kmgGhaeleLifetime`, round 4, run identically on the released v0.0.195 package (`…162819Z`) and the
candidate (`…163235Z`), in the admitted fixture save KMCS_AFTER_NORMAL_012_MODONLY (Jamandi mansion throne room):

**Cast (R2-F4, R3-F5).**
- The published summon is cast as one owned native `UnitUseAbility` from the main character, at caster level 20 for that
  cast only.
- The ability loan (borrowed parameters, or the exact temporary grant) ends when the cast settles, before any lifetime
  observation. At the first post-cast observation the grant is gone.
- The command, and any Ghaele it produces, is owned from submission. Each release is held in the probe's resources.

**Lifecycle cases (both packages, all held).**
- A borrowed ability with non-default parameters (CL 7, SL 3) was cast at CL 20 and restored, same instance, same
  parameter object.
- A temporary grant replaced during the cast: the replacement survived the loan's release untouched.
- Abandoned with the command pending (not acted, no execution): the command was settled, the grant removed, and no late
  summon appeared.
- Abandoned after the Ghaele spawned but before registration: the early summon was destroyed (1), and no owned command or
  grant remained.
- In both abandonment cases, an unrelated unit's command started just before the abandonment was still held after it.

**Timeline.**
- Appearance ends at 1.72/1.73 s and is kept.
- The body's `_DissolveCutout` stays 0.00 in life; raw `_Dissolve` reaches 1.00.
- Ordinary move of 4.0 m.
- Native lifetime ends at 119.7 s; the unit is removed at 121.55/121.56 s.

**Combat and lighting (R3-F1).**
- Combat: the Ghaele made three commanded attacks, all hits, on a disposable hostile target. The target is a
  `DefaultPlayerCharacter` copy under the loaded faction `StagFort_AuchDovan` (the sixth faction tried; brain off).
- Lighting was recorded idle and in combat: ambient 1.0 (0.375, 0.329, 0.270); interpolated light-probe DC luminance
  0.241 idle and 0.207 in combat; 20 and 18 lights in range.
- The game camera's party view, bound to the enabled body renderer, was in frame, lit, with no fade:
  - released: luma 36.0 idle, 41.9 in combat;
  - candidate: luma 35.9 idle, 45.5 in combat.

**Round 5 (`…182757Z` released, `…183206Z` candidate; R3-F5, R4-F2, R4-F3, R4-F6, R3-F6).**
- **Summon identity.** The cast's Ghaele is the unit that the native `RuleSummonUnit` of its own command's execution
  produced. Registration and pre-registration cleanup use exactly that unit.
- **Foreign Ghaele.** Both abandonment cases add a foreign same-caster Ghaele from another execution (the product's
  summon path). In each, only the owned summon was destroyed (1, or none while pending), and the foreign one survived.
- **Loan verdict.** The timeline's verdict compares the exact handle: the same borrowed instance with the same parameter
  object, or removal of the exact grant (a distinct replacement is allowed). It held for a borrowed CL 20 override and a
  borrowed CL 7 override, and refused an injected incorrect restoration and a vanished borrowed ability.
- **Combat teardown.** Only the probe's own attacks are interrupted and its own target destroyed. Combat is left only for
  units that were not fighting at intake; an incompatible intake would be refused. An independent move command on the
  main character was still held after the teardown.
- **Lighting.** Each row is judged on its readbacks: body readable, cutout ≤ 0.02, light probe read, and a capture image
  in frame, lit, with the body's own mesh. The capture helper's `intact` flag reads the raw `_Dissolve`, which on the
  Ghaele is the native loop, so it is recorded but not used. Idle and combat were valid on both packages.
- **Attribution.** Current snapshots with an unreadable body are inconclusive. Qualifying donor loops are those the donor
  was observed running with a matching readable value. 11/11 positive with body identity on both packages.
- **Re-evaluation.** The retained round-3 and round-4 rows still attribute: 11/11 each, body identity not recorded in
  round 3. The round-4 lighting rows qualify.

**Attribution (R3-F6).** Only positive samples count. A sample is positive when an active, looping, non-cutout animation
that the donor also runs drives a readable body material to its own current value. Empty, inactive, delayed-only or
unreadable samples are inconclusive.
- Round 4: 11/11 in-life snapshots positive per package, with body-material identity recorded; 0 contradicting.
- Re-evaluation of the round-3 raw rows (`…052619Z` released, `…052954Z` candidate) through the corrected predicate, by
  the domain test `sprint7-visual.ghaele-attribution`: 11/11 positive each. Body-material identity was not recorded in
  those rows. Their original verdicts are kept.

Candidate, product observer (round 3, `…050957Z`, bright outdoor and dim interior): ordinary casts, body cutout 0.00 to
expiry, moves of 3.9–4.0 m, three native weapon-attack rules (bright), and removal 121.6–122.0 s after appearance.

**Released-baseline boundary (R3-F1).** The admitted fixtures that can run the released package were each inspected:
- KMCS_AFTER_NORMAL_012_MODONLY (harness ops): used above. This gives one interior lighting condition, plus combat.
- The KMG automation baseline/working saves admit only named product scenarios with the owner's mod stack. v0.0.195 has
  no Ghaele presentation scenario there.
- The product's fresh-world review fixtures (outdoor Default, static interior) admit only the phases compiled into the
  loaded package. In v0.0.195 the runtime code that references the Ghaele checks its owned grant and descriptions
  (`summon-playtest-r3`), cleric conversion spawns and roster spawns. None of it observes the Ghaele's body, lifetime or
  combat presentation.

The bright outdoor released case therefore has no admitted route. It stays a retained limitation; candidate outdoor
evidence does not replace it.

Disposition and open scope:
- No product material, lifetime or despawn change is warranted for the observed released or current packages under these
  conditions.
- The October incident is **not attributed**. Its run's writer, build and frames were not re-read, and no observation
  ties its reading to this loop.

### 4.2 T2 / VIS-01 Nalfeshnee

See `assets-source/original-models/sprint39-nalfeshnee/SOURCE.md` and `docs/evidence/sprint7/exports.json` (base
generator → R7 `d34e6c19…`; repaired → mesh `6ff85493…`, albedo `1e9f5303…`, twice each).

Native, round 4 (`…162110Z`, R1-F3):
- **Gameplay-distance party view.** For each state (after and before), the shared capture helper is given the enabled
  body renderer's exact name.
  - Its readback is judged in the verdict: image written, in frame, lit, fade cleared, renderer enabled, the renderer's
    current mesh, body intact.
  - After reads `KMG_nalfeshnee_Original`; before reads `KMG_nalfeshnee_S7BeforeEvidence`. Bright luma 56.5, dim 32.4.
  - A domain test covers absent, wrong-body, disabled, unlit, out-of-frame, faded and dissolving captures.
- **Natural appearance and body binding.** As in round 3: the published spell, a native appearance completing on its own,
  and every view's body readback naming the enabled renderer (11,593 vertices after; 12,402 for the pinned R7 export
  before).
- **Whole-body views.** These are context only, judged on nine sampled body rays. Dim rear (after and before): 4/9 rays
  blocked by the gallery at the best yaw (+45°), so it is identified as obstructed. All bright views and every wing-root
  detail are clear on nominal bearings. The anatomical claims rest on the details.
- **Motion.** Idle, move, a turn over 45°, and an attack (bright), after and before.

### 4.3 T3 / VIS-02 Salamander

See `assets-source/original-models/sprint17-serpents/SOURCE.md` and the export receipt (exact private capture
`eb0d5eaa…`; base generator → released `b5524a69…`; repaired → `0837e796…`, twice).

Native, round 4 (`…162110Z`):
- Party views bound to the enabled body: `KMG_SalamanderHuman_…_Mesh` after and `KMG_salamander_S7BeforeEvidence`
  before; luma 50.4 bright, 33.3 dim.
- Waist front and rear and both hands, after and before, all clear. Only the dim right-hand detail uses a yaw offset (−45°).
  No obstructed whole-body view.

### 4.4 T4 / VIS-03 Trample/Overrun windows

Released baseline (`…233302Z` r3): both owned windows read "Target point within 0 ft. feet." with the header "None".

Candidate (`…050957Z` ui, 14/14):
- The Brachiosaurus Overrun and Trample and the Mastodon Trample, through the description window and the HUD path, read
  "Target point within twice this creature's speed.", with no "0 ft" and no "None". The owner approved this wording.
- Controls: native Overrun keeps its native target text; native Magic Missile keeps "Evocation".
- Ownership: an existing control fact is used as it is. Only a fact the observer granted is released, by instance, while
  the actor still holds that instance.

Controller mode (R1-F5), traced through both existing console routes. This trace was read from source and native
decompilation; nothing was executed. It corrects the round-3 text, which wrongly said only the teleportation and firearm
console routes exist. **Controller mode is unexecuted. This is not a controller PASS.**

**Admission.** `IsCleanup2ConsoleReview` (`RuntimeTestRunner.SummonCleanup2ConsoleScene.cs:11-13`) also admits the
cleanup2 phases `elementals-console` and `alternate-console`:
- The scenario is `disposable-expanded-summoning-cleanup2-native-review` with exactly one parameter, `reviewPhase`
  (`RuntimeAutomation.Common.ps1` `Test-KmgCleanup2ReviewPhaseScope`, `Test-RuntimeScenarioPreflight.ps1`).
- The runtime refuses any other parameter count (`SummonCleanup2ElementalConsole.cs:195-198`,
  `SummonCleanup2AlternateMenus.cs:186-190`). No creature, ability or target can be named.
- Both phases set controller mode before world creation (`SummonCleanup2ConsoleScene.cs:23-67`). Both load the Oleg
  outdoor area with no save, through `ExpandedSummoningNativeProfile.cs:114-131,165-170`. In that fresh world the native
  console in-game UI is live.

**`elementals-console`**
- **Units and selection.** The only unit is the fixture caster, a level-20 Wizard clone of `DefaultPlayerCharacter`
  (`SummonCleanup2Review.cs:115-146`). The phase calls `selection.SelectUnit(fixture.Caster.View)`
  (`SummonCleanup2ElementalConsole.cs:281-284`).
- **Guards.** `uiOwned` (`:224-230`) requires `bar.Unit.Value` and `CurrentSelectUnitValue` to be `fixture.Caster`, and
  the selector guard (`:231-251`) requires the selector owner to be the caster.
- **Input.** Only the selector handlers and the converted-panel handler are driven, through
  `InvokeTeleportGamepadInput`. The fixed small-fire-elemental source is cast; Summon Monster II, Summon Nature's Ally II
  and Magic Missile appear only as selector rows.

**`alternate-console`**
- **Units and selection.** It selects only the alternate-source casters it creates (`SummonCleanup2AlternateMenus.cs:608-611`).
- **Input.** It opens the converted panel without casting. Its one description-like step is a hover tooltip on the
  converted Horse row (`:845-852`), not the description window.

Neither route calls `OpenDescriptionWindow`, `HandleOpenDescription` or `OnRightStickButton`, or presses the console
description button.

**Native console path (Assembly-CSharp)**
- **Bar binding.** `ActionBarVM.Unit` is `UnitSelectionManager.Instance.CurrentSelectUnit`, and its pages are built from
  that unit's `UISettings`. `ActionBarSelectorVm` reads the same unit's abilities. Every slot, selector row and
  converted row therefore belongs to the selected unit.
- **Opening the description.** Button 19 is bound to `ActionBarVM.OnRightStickButton` (gated by `TooltipEnabled`). That
  toggles the current slot's tooltip, and `ActionBarSlotView.ShowHideDescriptionWindow` calls
  `TooltipTrigger.OpenDescriptionWindow`, which reaches `HandleOpenDescription`.
- **Selection guard.** The console `UnitSelectionManager.SelectUnit` returns without selecting when
  `!selectedUnit.IsDirectlyControllable` (line 80, verified in the decompiled source). A summoned unit takes that flag
  from `UnitPartSummonedMonster`, which is set from the spawning `ContextActionSpawnMonster.IsDirectlyControllable`.
- **Bypass.** `ForceSelectUnit` skips the check, but `SelectionManagerBase.LateUpdate` deselects units that are not
  directly controllable, so using it would be a bypass.

**First unsupported operation.** In both routes, the first operation not admitted is creating a summoned creature
that owns Trample or Overrun (a Brachiosaurus or Mastodon). Neither phase casts such a summon or can be given one:
the single-parameter guard above.

The next operation, selecting that summon as the console action-bar unit, is refused twice:
- by the routes' own actor guards (`uiOwned`, `SummonCleanup2ElementalConsole.cs:229`;
  `SummonCleanup2AlternateMenus.cs:610`);
- natively, by `UnitSelectionManager.SelectUnit`'s controllability check. It is inferred, not checked per blueprint,
  that this product's summon spawns keep the native `false` flag; no repository code sets it.

**Console Inspect (InspectVm), traced in round 5 (R1-F5).** This is a separate path from the slot description.
- **Activation.** In `InGameInputLayerView`, button 7 on the "InGameMainInputContext" layer calls
  `InGameUiContext.OnActivateInspect`, which calls `InGameUiStaticPartContext.OnActivateInspect`. That asks
  `ActionBarVM.GetInspectUnit()`, which returns `ActionBarVM.InspectVm.Unit.Value`; if it is null, nothing opens.
- **Unit source.** The only writer of that unit is `ActionBarInspectVm.HandleHoverChange`, which sets it only for a
  hovered unit that is not directly controllable and is the player's enemy:
  `isHover && !IsDirectlyControllable && IsPlayersEnemy`. This was verified in the decompiled source.
- **What it opens.** `InspectVm` lists that unit and the visible enemies in combat as a `TooltipType.UnitInspect`
  sheet, which is not an ability description window.
- **Why the summon is refused.** An allied summon is not the player's enemy, so Inspect never accepts it.
- **Route coverage.** Neither cleanup2 console route hovers a unit, creates a hostile, or presses button 7; no repository
  file references `OnActivateInspect`, `InspectVm` or `GetInspectUnit`.

So the slot description, which requires the summon to be the selected unit, is the only native path to an owned
summon action's description. The first unsupported operation remains creating the summon in either route. The next blocks
differ by path: the selection guard on the slot path, and the hover filter on the Inspect path.

The owned Trample and Overrun belong to the summon blueprints (`ExpandedSummoningSpecialBuilder.cs:925-935`), so no
caster's bar can legitimately hold them. Native Overrun is also absent from the console casters.

Magic Missile is on the elemental caster's selector, but no admitted step opens its description window. The desktop
fact/HUD evidence above is unaffected. Reaching the console description of an owned summon action would need a new
admission (a summon cast plus selection of a non-controllable unit, or a new owned-description dispatch). This
continuation does not create one.

### 4.5 T5 / VIS-04 Ankylosaurus impact cue

Product (round 3): the cue has its own once-per-roll claim. It is taken only after the exact owner, tail weapon, resolved
hit, in-game victim and the roll's own target all hold. The strike-rider stamp is unchanged.

Native, round 4 (`…170720Z`, `bc67438f3b`):
- **Hits.** Two resolved tail hits each posted exactly one cue in the hit's own frame (IDs 105 and 175), ending naturally
  after 1.56 s and 2.14 s.
- **Listener distance.** Measured in the resolving frame: 37.45 m camera to emitter.
- **Controls and replays.**
  - production-path controls: valid roll 1 cue; foreign weapon, wrong-target wrapper and out-of-game target 0; the same
    roll's valid wrapper 1;
  - replays 0; another owner 0; a later hit 1; miss and cancel 0.
- **Retirement with both cues live.** Retired ID 451 had its stop requested and its native end within 0.055 s. The
  survivor 449 ended naturally after 2.28 s.
- **End-of-phase dump.** All 8 cue records of the phase have their native end.

**Lost survivor callback (R4-F5), established from the failed session first (`…164749Z`):**
- Survivor 497 (first Ankylosaurus) and retired 499 (second Ankylosaurus) both posted in frame 5329, each on its own
  source's view.
- Retiring the second source requested a stop only for 499 (stop-requested, native end 0.04 s later). The product never
  requested a stop for 497, and 497's end-of-event callback had not arrived 10 s later.
- In the product, a record leaves its source's tracking only on its end callback. An uncallbacked record therefore stays
  tracked until its own source retires, and that retirement then stops it by playing ID.
- What the failed session cannot establish is whether Wwise was still playing 497 (no playback query was recorded) or
  why its callback did not come.

**Narrowed observation, round 5 (`…180635Z`).** The retirement now records each playing ID's native event
(`AkSoundEngine.GetEventIDFromPlayingID`) and the survivor's tracking before, right after, 3 s after and at the end of the
wait. In this run both cues were playing at retirement (event 4293635115), and both had ended natively with tracking
released by the end; the failure did not reproduce. The check passed in `…154136Z` and `…161344Z` (before the failure) and in
`…170720Z` and `…180635Z` (after it); round 2 retained a separate run that lost both end callbacks.

Round 5 carried this concurrent outcome as an unresolved technical requirement; both existing verdicts are kept.

**Round 6: the normal-callback control.** The concurrent retirement's verdict now includes the per-ID trace. It goes
through the installed query (`GetEventIDFromPlayingID`), which distinguishes confirmed playing, confirmed inactive, and
unknown (engine not initialized, query failed, or no playing ID). Unknown is never completion. The check requires:
- both IDs confirmed playing before retirement;
- no stop attempt on the survivor;
- the survivor's own end-of-event callback;
- two frames later, the survivor confirmed inactive and no longer tracked;
- the retired ID confirmed inactive.

The check passed in `…200320Z` and `…202648Z`.

**Round 6: bounded characterization (synthetic callback loss; R4-F5).**
- **Method.** Two fresh Ankylosaurus sources post overlapping cues. The observer's prefix on the product's callback
  receiver (`SummonAbilityCastCueRuntime.OnEnded`) withholds the end-of-event callback for one exact survivor record from
  the product; every other record's callback passes through. Native playback is untouched and read separately.
- **Declared beforehand:** a 16 s overlap search, a 12 s observation window after the other source retires, and a 6 s
  disposal wait.
- **`…202648Z` (candidate `740d94a995`), from its retained records:**
  - before retirement, survivor 656 (posted 20:29:00.197) and other 684 (posted 02.314) were both confirmed playing;
  - the other source was retired at 02.326; the product requested a stop for 684 alone at 02.330 (view-destroyed), and 684
    ended natively at 02.379;
  - 656's callback arrived at the withholding prefix at 02.395, 2.2 s after it posted, its natural length, and was
    withheld. The native query confirmed 656 inactive from 02.833 through the window end;
  - the product recorded no end and kept tracking 656 on its own source's playback component. That is the only residual;
  - retiring the survivor's source at 14.334 made the product request a stop for the already inactive 656
    (view-destroyed, 14.339). A request on an inactive playing ID is not completion, and `EndUtc` stays unset;
  - the source's view and playback component were then observed destroyed;
  - no other cue record changed, and the observer's hook was removed.
- **The in-run verdict, "other-not-stopped-by-own-id", is retained as recorded.** It read 684's stop in the retirement
  frame, before the product's destroy callback ran.
- **Offline re-evaluation of the same records** (labelled as such; domain case `sprint7-visual.impact-cue-playback-verdicts`):
  - the settled predicate (`RetiredByOwnId` over the end-of-phase record of 684) and `CallbackLossVerdict` give
    "characterized" for `…202648Z`;
  - the same reader keeps `…200320Z` at "not-both-playing". That run's survivor 599 also played out naturally (callback
    withheld 37.54, confirmed inactive 38.01), but its other cue had already ended.
- **Disposition (amendment 2 §4; accepted by the round-6 review).** Under synthetic callback loss:
  - playback ended normally;
  - the other source was unaffected;
  - the only residual was source-local tracking, which disappeared when that source's view and component were destroyed.
  No product change and no watchdog. The historical `…164749Z` failure and its unknown cause are kept: the
  characterization does not explain why that callback was missing, only how the product behaves when one is.

**Round 7: the one settled evaluator and the final collector (R6-F3, R6-F4).**
- **Evaluator.** `Sprint7ObserverPolicy.CallbackLossSettled` reads the callback-loss row as the collector emits it, with
  the other source's settled cue record and the retirement time. It is the in-game verdict and the reader of retained
  rows. Beyond the round-6 inputs, it requires the survivor's terminal reads, at the window end and after its own source's
  disposal, to be confirmed inactive. Playing is persistence; unknown or missing is never completion; playing again after
  an inactive sample contradicts. Query states, callback records, stop requests and tracking are separate inputs.
- **Retained rows, as a reader over fields their collector already emitted** (not a later live-sampling path):
  `…202648Z` (collector `740d94a995`) gives "characterized"; `…200320Z` (`f3bfab1303`) stays "not-both-playing". In copies
  of the `…202648Z` row, setting either terminal read to playing, unknown or missing prevents characterization (domain
  `impact-cue-playback-verdicts`).
- **Final collector in game (candidate `9c39b1ff7f`).**
  - `…223809Z`: no overlap in the declared search; the in-run "not-both-playing" stands.
  - `…224949Z`: survivor 586 (posted 22:51:54.350) and other 622 (posted 56.414) were both confirmed playing. The other
    source was retired at 56.429; the product requested a stop for 622 alone at 56.433, and 622 ended natively at 56.442.
    586's callback was withheld at 56.624. The query confirmed it inactive from 56.956 and in every later window sample,
    and inactive at the window end and after disposal. The product kept tracking it on its own source. Retiring that
    source made the product request a stop for the already inactive 586 (view-destroyed, 22:52:08.463); `EndUtc` stayed
    unset. The view and playback component were observed destroyed, no other record changed, and the hook was removed.
  - **Its in-run verdict, "survivor-stopped-by-other-retirement", is retained as recorded and is wrong.** The evaluator
    received the collector's in-memory row, where the survivor's null stop fields are String-typed JSON values, and its
    null test treats only JSON nulls as null. The recorded row has JSON nulls, and the same evaluator, unchanged, gives
    "characterized" for it. The domain case reproduces the in-run verdict by rebuilding those fields as String-typed nulls.
  - **Remaining blocker:** an in-game PASS of this predicate needs that null test corrected in compiled RuntimeTesting, a
    fourth candidate that amendment 3 does not authorize. The collector itself is qualified: candidate 3 ran its whole live
    path and emitted every field the verdict reads.

**Private listening packet (R1-F1, owner amendment 1).**
- **Location (round 5):** `C:\Dev\KingmakerGunslingerLab\private\s7-listening-packet-r5\` and
  `C:\Dev\KingmakerGunslingerLab\private\s7-listening-packet-r5.zip` (sha256 `2b5f4610…2565`).
  - The round-4 packet (`…-r4`) is superseded and kept.
  - Round 5 fills the twelve isolated samples' playing IDs from the native rows (2091–2697).
  - It labels the Trumpet (37.09 m) and Vrock (40.05 m) distances as observed one frame after posting (frames 4975→4976
    and 4638→4639); the exact posting distance is not available.
  - The two tail-hit distances stand, measured in the frame the cue posted.
- **Contents:** `index.html` (players per clip, grouped and labelled), `playlist.m3u8`, `MANIFEST.json`, `README.txt` and
  `clips/`.
- **Clips:** 18 excerpts from the `…170720Z` loopback recording (`f67b1d4f…e37f`):
  - the two in-play Ankylosaurus hits (actual);
  - the in-play Trumpet Blast (actual, ID 397, 37.09 m one frame after posting);
  - the Vrock neighbour (40.05 m, likewise);
  - two quiet baselines;
  - twelve isolated observer samples: the two current cues and ten alternatives.
- **Per clip:** hash, session, package, UTC interval, event, playing ID, emitter, game volumes (all 100) and listener
  context.
- **Verification:** every clip matches its lab excerpt's hash, decodes as 16-bit PCM, and loads in the Windows WAV
  decoder; every link resolves; the zip tests clean. Browser playback itself was not exercised.
- **Kept separate:**
  - technical playback, timing and ownership: verified;
  - clip integrity and accessibility: verified;
  - actual listening: none, since no agent here has supported audio perception;
  - suitability and the final choice: the owner's. `Sweetener_Low_06_Quake` stays provisional.

Distance corrections: the round-3 tail-hit distance (37.45 m) was sampled after both strikes plus a 4 s wait, not at
posting. It is now labelled that way in `audio-context.json`, the round-3 event-time distance is "not measured", and the
round-3 R6 distance is "not measured". Nothing is estimated.

### 4.6 T6 / VIS-05 Dust Mephit breath

**Product change, round 4 (R1-F9).** The round-3 creation tracker is removed.
- A Harmony12 transpiler on `ProjectileController.CreateView` routes its one
  `FxHelper.SpawnFxOnPoint(GameObject, Vector3)` call through `DustMephitBreathPresentation.SpawnCastFx`, passing the
  creation's projectile.
- `SpawnCastFx` makes the same native call. It adopts the returned instance only when that creation is the owned Dust
  breath's (`CastEffectAdoption.Spawn`: executing ability, launcher, cold-cone projectile, own prefab, own launch point).
- Nothing is pending between calls, so:
  - an interrupted creation lends nothing;
  - a spawn by any other caller never reaches the adoption;
  - if the call is not found exactly once, CreateView is left native and no cone is recoloured (`Installed` readback).
- Reproduced before the repair: a domain case on the round-3 tracker gave the foreign spawn the tint
  (`foreignTints=1`; `<lab>/runtime-evidence/s7/r4-offline/r1f9-repro-before-repair.txt`).

Native, round 4 (`…170720Z`):

**Nested interruption (R1-F9)**
- While an owned Dust creation was in progress, an inner owned creation of the same breath was interrupted
  (NullReferenceException, after setting its launch point and before its spawn).
- Before the enclosing call returned, another caller spawned the same cast effect at that exact point. That instance
  stayed native: no tint, the native signature `fee8e469…`.
- The enclosing creation's own cast effect was then tinted exactly once and restored on release.

**Casts and mechanics** (unchanged released mechanics, by each breath's own context)
- Three consecutive breaths ran from one position on the same pooled instance −237330, with the installed call site:
  - natural 1: 4 damage, full, Sickened;
  - natural 20: 1 damage, half, no condition;
  - natural 1: 4 damage, full, Sickened.
- The ally rolls a harmless save; the enemy outside and the caster behind are never selected.

**Controls (unchanged and passing)**
- The Dust Mephit's own Ice breath and the Ice Mephit's breath reused the instance untouched.
- The Ice donor held its native signature through the Dust release.
- Of two live Dust instances, only the exact one was restored.
- A pre-execution cancel created nothing.
- Source loss read native.

**Conditions (R3-F3)**
- The cell's ledger owns every Sickened instance delivered by an ability execution of the cell's own units, captured at
  delivery. This matters because native Sickened does not stack: in `…161344Z`/`…164749Z`, later Dust applications only
  refreshed an instance the cell's own Ice breath had introduced.
- Final run: 13 owned executions, 4 instances acquired, 1 live at cleanup after the last cast and removed, 0 remaining.
- The foreign control (now on the caster behind the cone, a unit that outlives the cell) was still present at cleanup.
- Early exit: a fresh Dust Mephit's cast, owning its own ledger, was abandoned the moment its Sickened was applied, with
  the command still running. Its one instance was removed, its command interrupted, and the foreign control kept.

**Round 5 (`…180635Z`; R4-F1, R4-F4)**
- **Ledger ownership.** The ledger now owns only executions of commands the cell issued: every Dust cast and every Ice
  control, registered before it runs (12 executions).
- **Separate execution.** A separate same-caster Dust execution the cell did not issue (cast at the enemy outside the
  cone) applied Sickened that the ledger did not own, and it was still present after cleanup.
- **Runner disposal.** `R6Cell` now disposes its cell on every exit, and the early-cast enumerator is disposed in a
  `finally`. The restoration verdict (`S7DustRestored`) fails any acquired trace or ledger without a clean cleanup result.
- **Abandon case `s7-dust-abandoned`.** The enclosing runner was disposed while the cell was yielded right after its owned
  cast applied Sickened, with the command still running and a CreateView unpatch failure injected. Results:
  - owned command interrupted by reference (1 ended, 0 left);
  - Sickened removed (1); foreign control kept; ledger subscription closed;
  - the injected failure reached the restoration verdict (false);
  - a later disposal took the remaining hooks from 2 to 0.

**Trace release (R3-F4)**
- Normal disposal feeds `Sprint7ObserverPolicy.Released` into the cell check and the outer restoration check.
- Final run: 4 hooks → 0, no failure.
- Models phase (`…162110Z`): a normal disposal with an injected CreateView unpatch failure gave a failed verdict. The native
  spawn and product-wrapper releases still ran, leaving 2 CreateView hooks and none elsewhere; a later disposal removed
  them.
- The setup-failure rollback still leaves 0 hooks, and the body-swap rollback is unchanged. Under the Harmony12 shim, hook
  counts and unpatches now act only on methods the trace patched.

Design questions resolved by owner amendment 1: the breath keeps Sickened. Allies' harmless saves stay documented, out
of scope.

### 4.7 T7 / VIS-06 Trumpet Archon

R6 (`…170720Z`):
- One `InspireCourage` cue per blast, ending naturally (ID 397; listener 37.09 m observed one frame after posting).
- None on a cancelled cast.
- Early retirement stopped by playing ID.
- The Vrock neighbour completed naturally.

Reload (`…162605Z`, R2-F2, R3-F2):
- **Reload.** The saved cue (ID 159) ended natively and naturally. Nothing was posted after the load, and the fresh blast
  posted one new cue (ID 323).
- **Round 6 (R2-F2; `…201133Z`, candidate `f3bfab1303`, harness `9240a3d6…`). No die is forced.**
  - **Why some intervention stays.** The blast's native outcome would paralyze non-archons in range, the fixture party
    included, and the condition would be carried into the save the reload takes.
  - **The intervention.** Only the observed blast's own top-level save is marked passed, through its own `AutoPass`
    field. No d20 rule is pre-rolled and no generator draw is touched; `OwnedD20Claim` and the d20 handler are removed.
  - **Decisive interval.** Inside the first owned save, before its native roll, a foreign `RuleRollD20` for the main
    character was dispatched directly. It rolled 8, equal to the untreated generator copy's 8, and was not pre-rolled. The
    owned save then rolled its own natural 17 (control 17) and passed through `AutoPass` alone. The following draw equalled
    the control.
  - **Other saves.** All 49 owned saves passed with natural dice (1 to 20). The pending-command interleave was untouched
    (d20 15 = 15, not marked).
  - **Reload and abandonment.** The reload held: saved cue 160 ended naturally, nothing replayed, and the fresh blast
    posted 324. The abandonment check found nothing held, the Trumpet destroyed with a single destruction request, and no
    release failure.
- **Round 5 (R2-F2; `…182558Z`, superseded).** The shared generator is no longer touched.
  - A blast save is a top-level save (no other saving throw below it on the rule stack) whose reason context is the
    observed blast's execution and whose saver is not the Trumpet.
  - Its own d20 rule (the d20 whose previous event on the rule stack is that save) is pre-rolled to 20 through the native
    `m_PreRolledResult` (`OwnedD20Claim`), once.
  - Inside the first owned save's callbacks, a foreign draw (equal to control) and a foreign Will save (d20 11 = control 11,
    no claim) were dispatched; the first draw after the owned save ended equalled the control's next value.
  - 25 owned saves got 20. The pending-command interleave (d20 8 = 8), the reload (159 natural, no replay, fresh 327) and
    the abandonment check held.
  - Retained: `…181432Z`, whose own nested save inherited the blast context before the top-level rule existed.
- **Blast-owned seeding (round 4, superseded).**
  - A saving throw is seeded only when its rule's context is the observed blast's own execution and its saver is not the
    Trumpet. The generator is saved at that rule's start and put back at that rule's end (`OwnedRollScope`).
  - 25 Fortitude saves of the first blast were seeded.
  - While the blast command was pending (before its execution began), an unrelated Will save and an unrelated draw kept
    the generator's own control values (d20 9 and 9; draws equal), with no seeding.
  - After the command, the generator was not back at its blast-start state.
- **Abandonment.** A blast probe was abandoned by its 4 s step timeout (expected failure, isolation restored). The check
  step then found the subscription ended, no intervention open, the observed command cleared, nothing held, the Trumpet
  destroyed, and no release failure.
- **Cleanup propagation (R3-F2).** Both probes use `YieldedProbeLifecycle`: every held release's outcome, from whichever
  drain ran it, reaches `ProbeSlot`.
  - Reproduced before the repair with the round-3 pattern: isolationRestored stayed true after a failed unsubscribe
    (`<lab>/runtime-evidence/s7/r4-offline/r3f2-repro-before-repair.txt`).
  - The domain case `sprint7-visual.probe-cleanup-reaches-slot` now shows, through `ProbeSlot`: the named failure, every
    other release attempted once, the original error kept, IsolationRestored=false, and refusal to start another probe.

`InspireCourage` is kept provisionally; the listening comparisons are in the private packet (§4.5).

## 5. Validation

Focused domain tests, `sprint7-visual.*`: 20 cases. Round 6 changes:
- replaces `trumpet-owned-d20` with `trumpet-owned-save`;
- adds `probe-slot-destroys-each-unit-once`, which runs the real ProbeSlot → Release → iterator finally → remaining
  resources sequence with deferred destruction;
- adds `impact-cue-playback-verdicts`, which includes the offline re-evaluation of the retained round-6 records;
- extends `ghaele-attribution` and `ghaele-lighting-and-loan` with the R5-F2/R5-F3 negatives, the pending and combat
  verdicts, and the re-evaluation of the complete retained `…182757Z` and `…183206Z` datasets.

Round 5 had 18 cases: it added `ghaele-lighting-and-loan` and `probe-final-releases-per-item`, and replaced
`trumpet-owned-roll` with `trumpet-owned-d20`. The suite covers:
- VIS-01/VIS-02 decoded exports;
- VIS-03 identities and text;
- VIS-04 qualification;
- VIS-05 identity, tint, and call-site ownership through the reviewer's interval (`vis05-call-site-ownership`);
- Sprint 7 admission and screen-capture decision; observer ownership;
- Trumpet reload verdict and abandonment;
- cleanup reaching `ProbeSlot` (`probe-cleanup-reaches-slot`);
- blast-owned roll scope (`trumpet-owned-roll`);
- positive Ghaele attribution with negative cases and the retained-row re-evaluation (`ghaele-attribution`);
- the condition ledger, release verdict and party-capture verdict (`cell-ownership-and-capture`).

Neighbour selections: `summon-playtest-r3`, `-r4`, `-r6`, `-r7`, `sprint22-`, `sprint5-`, `salamander`,
`native-admission`, `native-profile` and `loader-compat-probe`. Counts by gate run: round 5 ran 111 tests locally; the
round-6 controller receipts on `b37faaf883` report 116 selected-test executions (20 in `sprint7-visual`, 96 in the
neighbour filters), all passing. Round 7's local focused runs selected 22 `sprint7-visual` tests; the round-7 controller
receipts on `3e2baea4b4` report 118 selected-test executions (22 in `sprint7-visual`, 96 in the neighbour filters), all
passing. The controller
records the required gate receipts on the reviewed head.

## 6. Owner decisions pending and open scopes

1. T1: the bounded scope is accepted by owner amendment 2 (released/current interior observations). The released
   outdoor/bright case stays unexecuted and the October incident unattributed.
2. T2, T3, T6: visual acceptance of the Nalfeshnee wing roots, the Salamander waist and fingers, and the dust cone.
3. T4: the controller-admission disposition is accepted by owner amendment 2. Controller rendering stays unexecuted,
   not PASS (§4.4).
4. T5, T7: listen with the private packet and choose; both cues stay provisional until then.
5. T5 / R4-F5: the conditional disposition of amendment 2 §4 was accepted by the round-6 review on the retained
   `…202648Z` records (§4.5). The cause of the `…164749Z` missing callback stays unknown. The in-run callback-loss
   predicates of `…200320Z`, `…202648Z`, `…223809Z` and `…224949Z` stay FAIL as recorded.
6. Merge and release remain the owner's; this sprint authorizes neither.

## 7. Review findings and corrections

Round-7 correction matrix (the eight findings open after round 6; owner amendment 3). Sessions are in
`docs/evidence/sprint7/r7/`.

| Finding | Trigger | Owning caller | Change | Decisive test | Evidence | Disposition |
|---|---|---|---|---|---|---|
| R4-F1 | Enclosing runner disposed inside the Dust early-exit cast, with the owned removal failing and the execution still live | `ReviewSprint7Effects` (cell sequence), `S7Dust`, `S7DustCastOwningConditions`, `S7DustConditions`, `S7DustAbandoned` | `Sprint7CellIsolation`: the cell registers before acquiring and publishes from its finally. Named failures stay; each retained execution process not yet ended is a residual re-read by the admission guard. The driver disposes the phase's own `Sequence` over a child isolation with a next cell, repairs its leftover and reports safety to the phase only on explicit observation | Domain `effects-cell-sequence-isolation` (live, unreadable and failed-release refusal; recovery does not erase the failure). Native `s7-dust-abandoned-unwound` | `…223809Z`, `…224949Z`: named failure in the child; execution pending at cleanup, then observed settled; next cell refused and never started; repair proven; phase isolation restored | Corrected |
| R6-F2 | Callback-loss observer abandoned while holding its hook, unpatch failing | `S7CallbackLoss`, `S7CallbackAbandoned`, the cell sequence | The observer registers with the phase and publishes the hook's outcome from its finally after every independent release (commands, source retirement). The driver disposes the sequence with an injected unpatch failure, then repairs its own hook | Domain `effects-cell-sequence-isolation` (hook case and zero-hook control). Native `s7-anky-callback-abandoned-unwound` | `…223809Z`, `…224949Z`: named unpatch failure and "hooks remaining: 1" in the child; both sources retired; next cell refused; zero hooks after repair. The ordinary observer left zero hooks in both runs | Corrected |
| R6-F1 | Early direct destruction throws; witness still held after settlement | `OwnedDisposables`, `YieldedProbeLifecycle.Release`, `WitnessSettlement`, `GhaeleLifetimeProbe` | Every request, direct or final, is recorded in the probe's one cumulative outcome; a throwing request is a named failure and stays requested; an unsettled witness throws a named failure. Release reads that outcome last | Domain `ghaele-early-release-failures-reach-slot` through `ProbeSlot` / `Poll` / `Release` / unwind: one request, original error kept, independent releases run, isolation fails, next probe refused; settled control | domain; `…224649Z` lifecycle (normal path) | Corrected |
| R5-F1 | A result first delivered after the cast owner's disposal | `GhaeleLifetimeProbe.Lifecycle`, `SummonWitness`, `GhaeleWitness`, `GhaelePending` | The witness holds its own correlation (the cast's exact AbilityData and retained execution contexts). New post-disposal negative control: queue, verify pending and loan with zero owned results, dispose the cast owner, register a disposable unit with its owner, then a labelled controlled notification injection to the witness handler. Every verdict input is a readback | Domain witness correlation cases | `…224649Z`: order queued → before-disposal (0) → cast-owner-disposed (0) → late-unit-registered (0) → delivered (1, by ability-data); verdict "late-owned-summon"; foreign kept; witness unsubscribed; late unit requested once and destroyed. Positive pending verdict null | Corrected |
| R5-F3 | A live view without an EntityFader read as visible | `GhaeleLifetimeProbe.Lighting`, `GhaeleLighting.Subject`/`Valid` | The subject, its view and its own fader component (identity, Visible) are read at the Ghaele boundary; missing, hidden and visible stay distinct; the helper's own field is not used and is unchanged for other callers | Domain producer-to-verdict cases for all three, no-view and unrecorded | `…224649Z` visibility: Ghaele present-visible valid; donor present-hidden "fader-hidden". Retained rows: "fader-presence-unrecorded" (§4.1) | Corrected; retained-row limit stated |
| R6-F3 | Terminal playback playing or unknown after earlier inactivity | `Sprint7ObserverPolicy.CallbackLossSettled` | Terminal reads at the window end and after disposal are inputs; inactivity earlier never overrides them | Domain mutations of the retained `…202648Z` row; retained rows re-evaluated | §4.5 | Corrected |
| R6-F4 | Final collector qualified by a run that never executed it | collector at `9c39b1ff7f`, one evaluator | One evaluator for native and retained judgments; retained rows labelled as a reader over their collector's own fields; candidate 3 ran the final collector | Native `…224949Z` live path; domain re-evaluation of its row | §3, §4.5. The in-run verdict of `…224949Z` is a retained FAIL from the evaluator's in-memory null test; the same evaluator over the recorded row characterizes | Collector natively executed; in-game PASS blocked by the evaluator null-typing defect (a fourth candidate is not authorized) |
| R6-F5 | Stale expenditure, counts and accepted-scope rows | this record, status table, known issues | Cumulative expenditure, counts by gate run, R4-F5 accepted throughout, Trumpet `AutoPass` wording, Ghaele scope accepted, amendments identified | Record against receipts | this record | Corrected |

Round-6 correction matrix (the eleven findings open after round 5; owner amendment 2). All candidate sessions listed are in
`docs/evidence/sprint7/r6/`.

| Finding | Exact trigger | Owning code | Correction or disposition | Decisive check | Evidence |
|---|---|---|---|---|---|
| R1-F1 (record) | Trumpet distance and isolated-clip counts traced to the packet | this record | Twelve isolated clips: the two current-cue samples and ten alternatives (four impact, six Trumpet). Trumpet 397 posted in frame 4975 and its 37.09 m was observed in frame 4976, labelled so everywhere. The packet MANIFEST's role field groups all twelve isolated samples as "alternative"; each label names the current-cue samples. No new recording | Record against `<lab>/private/s7-listening-packet-r5/MANIFEST.json` and `audio-context-r4.json` | packet unchanged (`2b5f4610…2565`) |
| R5-F6 (record) | Recount of the effects predicates and concurrent-retirement runs | this record, `KNOWN-ISSUES.md` | 33 structured checks for `…180635Z` (the plain-text profile line is not counted). Concurrent retirement passed in four of five round-4/5 runs: `…154136Z` and `…161344Z` before the `…164749Z` failure, `…170720Z` and `…180635Z` after it. The stale "Six are observer…" introduction is replaced | Extract MANIFEST counts | `docs/evidence/sprint7/r4/`, `r5/` |
| R2-F2 | A foreign `RuleRollD20` dispatched directly inside the owned save before its native roll | `TrumpetCueProbes.cs`, `TrumpetSaveOwnership` | Claim machinery and pre-roll removed. The blast's own top-level save passes through `AutoPass`, and its die rolls natively. The intervention stays because paralysis would otherwise enter the reload's save | Native: foreign d20 8 = control 8 (not pre-rolled); owned die 17 = control 17; following draw = control. Domain `trumpet-owned-save` | `…201133Z` |
| R4-F1 | Enclosing runner disposed while the early-exit cast is active | `S7DustEarlyExit`, `S7DustCastOwningConditions`, `S7DustConditions`, `S7DustRestored`, `S7DustAbandoned`, effects runner | One acquisition tree: the early cast uses the cell's ledger and writes its failures into the cell's list. Still-live executions are reported. The restoration verdict names every reason. Later cells do not run on lost isolation | Native: runner disposed inside the early cast after it applied Sickened, with one owned removal made to fail. The verdict failed naming it; the foreign condition was kept; the other releases ran; the observer repaired its leftover | `…200320Z`, `…202648Z` (`s7-dust-abandoned-unwound`, `s7-dust-early-exit-released`) |
| R4-F2 | A second engagement acquired after clean intake | `GhaeleLifetimeProbes.cs` (`Observe`), `GhaeleCombat` | `LeaveCombat` removed. Only owned attacks and the target are released (single destruction request). An independent engagement control is preserved; unowned engagements are reported, never cleared; native exit is required | Native: control preserved in combat through teardown, no unowned engagement, native combat exit 3.04 s after the control's release. Domain combat verdict | `…201333Z` |
| R4-F5 (requirement) | Survivor callback missing while the other source retires | product cue tracking (unchanged); `S7Ankylosaurus`, `S7CallbackLoss`, `Sprint7ObserverPolicy` | Per-ID playback and tracking in the verdict. Synthetic callback-loss characterization; conditional owner disposition (amendment 2 §4) claimed; no product change | Normal control PASS ×2. Characterization in-run FAIL (`…202648Z`, observer timing) retained. Offline re-evaluation of its records: characterized. `…200320Z` stays not established | §4.5; domain `impact-cue-playback-verdicts` |
| R5-F1 | Owned summon appearing after the pending cast was disposed | `GhaeleLifetimeProbes.cs` (`Lifecycle`), `SummonWitness`, `GhaelePending` | Foreign summon prepared first. Exact command and loan verified just before disposal. A separate execution-correlated witness runs through the 6 s window. A real owned spawn is the negative control | Native: pending true, loan held (CL20, cast's own grant), witness saw no owned or unattributed summon, foreign kept, witness unsubscribed. Negative control: "late-owned-summon" | `…201333Z` |
| R5-F2 | Null body plus a contradictory animation | `GhaeleAttribution.Classify` | Contradictions evaluated before body credit | Domain: five positive rows plus a null-body cutout or unknown row do not attribute. Six retained datasets re-evaluated, all 11/11 positive | `ghaele-attribution` |
| R5-F3 | Lit, enabled renderer while the unit's fader reports it hidden | `GhaeleLighting.Valid` | `faderVisible=true` required; "fader-hidden" and "fader-unavailable" reasons | Domain negatives. Retained `…182757Z`/`…183206Z` rows re-evaluated valid; native `…201333Z` rows valid | `ghaele-lighting-and-loan` |
| R5-F4 | Timeline abandoned after the witness readback | `Observe` (witness move) | Witness owned separately, registered before it is issued, settled right after its readback or on abandonment | Native: held through teardown, then settled. Domain combat verdict ("witness-not-settled") | `…201333Z` |
| R5-F5 | Slot release with combat active: target held by both the cleanup and the final collection | `OwnedDisposables`, `YieldedProbeLifecycle.Release` (final releases built after the iterator), both probes | One destruction-request owner per unit; final releases only for units never requested | Domain `probe-slot-destroys-each-unit-once` through `ProbeSlot` with deferred destruction: one request per target, the failure retained, the others attempted | domain; `…201133Z` abandonment |

Retained round-6 failures (observer): `…200320Z` and `…202648Z`, both on `s7-anky-callback-loss-characterized`.

Round-5 review outcome: seven verified (R1-F5, R3-F5, R3-F6, R4-F3, R4-F4, R4-F6, R4-F7); R1-F1, R2-F2, R4-F1, R4-F2 and
R4-F5 still open; R5-F1 to R5-F6 raised; the controller stopped at the round cap (status above).

Round-5 correction matrix (the twelve findings pending after round 4):

| Finding | Exact trigger | Owning code | Change or disposition | Decisive check | Evidence | Remaining limit |
|---|---|---|---|---|---|---|
| R4-F1 runner disposal | Request disposed while `R6Cell` yields a step of S7Dust | `R6Cell`, `S7Dust`, `S7DustRestored`, `S7DustAbandoned` | Cell disposed on every exit; early cast disposed in a finally; acquired resources must report cleanup | `s7-dust-abandoned-unwound` | `…180635Z` | — |
| R3-F5 summon identity | Another execution summons a same-caster Ghaele before registration | `GhaeleLifetimeProbes.cs` (`RuleSummonUnit`, `CastOwned`) | Owned summon from its own execution's native summon rule | Lifecycle pending and after-spawn with a foreign Ghaele | `…182757Z`, `…183206Z` | — |
| R4-F2 combat teardown | Teardown interrupting the main character's unrelated work | `Observe` combat block | Only owned attacks, own target, combat state introduced at intake; incompatible intake refused | Independent main command held after teardown | same | — |
| R4-F5 lost callback | Survivor cue without native end after the other source retires | product cue tracking; Ankylosaurus observer | Analysed from the failed session; native playback readback added (not reproduced) | Retirement trace | `…164749Z`, `…180635Z` | **Carried as an unresolved technical requirement** |
| R1-F1 packet labels | R6 distances and sample IDs traced to the cue records | lab packet; `audio-context-r4.json` | Posting/observation frames labelled; posting distance marked unavailable; sample IDs filled; packet rebuilt | Packet verification and Windows decoder load | `<lab>/private/s7-listening-packet-r5` | Owner listening pending; browser playback not exercised |
| R1-F5 Inspect | Inspect dispatch omitted from the trace | native console UI; cleanup2 routes | InspectVm source and dispatch traced; hover filter named | Decompiled references (§4.4) | this record | Unexecuted; not a controller PASS |
| R2-F2 nested draws | A foreign draw or save inside the owned save's callbacks | `TrumpetCueProbes.cs`, `OwnedD20Claim` | No generator intervention; owned d20 pre-rolled through the rule contract; top-level saves only | Nested foreign draw and save plus following draw equal to control | `…182558Z`; domain `trumpet-owned-d20` | — |
| R3-F6 attribution | Unreadable current body; donor loop never running | `GhaeleAttribution.cs` | Explicit null body inconclusive; qualifying donor loops observed running | Domain negatives; native 11/11; retained re-evaluation | `…182757Z`, `…183206Z`; domain | Round-3 rows lack body identity (labelled) |
| R4-F3 loan verdict | Borrowed CL 20 override, replacement, wrong or missing restore | `GhaeleLoan`, `LoanVerdict` | Exact handle comparison in the timeline and lifecycle | Native CL 20 and CL 7 borrowed, injected wrong restore, vanished borrow; domain | same | — |
| R4-F4 ledger acquisition | A same-caster execution the cell did not issue | `S7DustConditions` (`Own`), `R6RunPassive` | Ledger owns only commands the cell issued | Separate execution's Sickened kept through cleanup | `…180635Z` | — |
| R4-F6 lighting | Lighting row with no body or an empty capture | `GhaeleLighting` | Each row judged on its readbacks | Domain negatives; native rows valid; retained round-4 rows qualify | same | — |
| R4-F7 per-item release | One item failing in a grouped final release | `ProbeReleases.PerItem` (both probes) | One release per item | Domain `probe-final-releases-per-item` through `ProbeSlot` | domain | — |

Round-4 correction matrix (the twelve findings pending after round 3):

| Finding | Exact trigger | Owning code | Change or amended disposition | Decisive check | Evidence | Remaining limit |
|---|---|---|---|---|---|---|
| R1-F1 audio | Cue recommendations followed to recordings and distances | lab packet; `RuntimeTestRunner.Sprint7Presentation.cs` (`S7AttackLog`), `SummonPlaytestR6Presentation.cs` (`R6RunPassive`) | Private playable packet; event-time distances recorded; round-3 labels corrected; no listening claim (owner amendment 1) | Packet self-verification and Windows decoder load; distances in the resolving or post frame | `<lab>/private/s7-listening-packet-r4`, `audio-context-r4.json`, `…170720Z` | Owner listening and choice pending; browser playback not exercised |
| R1-F3 party view | Normal-distance capture of a disabled accessory or an empty image | `S7Stills`, `Sprint7ObserverPolicy.PartyCaptureValid` | Capture bound to the enabled body renderer and judged in the verdict; whole-body views sample 9 rays and are context only | Party valid on all 8 states; negative cases in the domain test | `…162110Z`, 8 stills | Dim Nalfeshnee rear whole-body view 4/9 rays obstructed (identified, context only) |
| R1-F5 controller | Claim that console routes need new admission | `SummonCleanup2*Console*.cs`, native console UI | Both cleanup2 console routes traced; wrong round-3 claim corrected; first unsupported operation and guards named | Source and decompiled citations (§4.4) | this record | Unexecuted; not a controller PASS |
| R1-F9 Dust | Interrupted inner owned creation, then a foreign same-point spawn before the outer returns | `DustMephitBreathPresentation.cs`, `MephitBreathPolicy.cs` (`CastEffectAdoption`) | Adoption bound to CreateView's own spawn call (transpiler) | Native nested-interruption check; domain interval case; pre-repair reproduction | `…170720Z`, `r1f9-repro-before-repair.txt` | — |
| R2-F2 Trumpet seeding | An unrelated save or draw while the blast yields | `TrumpetCueProbes.cs`, `TrumpetCueVerdict.cs` (`OwnedRollScope`) | Seeding only on the blast execution's own saves, generator put back at that rule's end | Native interleave and no rewind; abandonment check; domain roll-scope case | `…162605Z` | The interleave happened while the command was pending, before its execution began |
| R2-F4 Ghaele loan | Borrowed or granted ability held through the lifetime | `GhaeleLifetimeProbes.cs` (`CastOwned`) | Loan released when the cast settles | Lifecycle borrowed and replaced cases; post-cast readback | `…162819Z`, `…163235Z` | — |
| R3-F1 released combat/lighting | Attack and lighting attributed to the released package | `GhaeleLifetimeProbes.cs` (`Observe`) | Released-package combat and lighting observed in the admitted fixture; boundary for outdoor/bright recorded | 3 hits, idle and combat lighting and party views | `…162819Z` | Released outdoor/bright: no admitted route |
| R3-F2 cleanup propagation | Failed restore during iterator disposal | `TrumpetCueVerdict.cs` (`YieldedProbeLifecycle`, `OwnedProbeResources.Outcome`) | Every drain's outcome reaches the slot | Domain case through `ProbeSlot`; pre-repair reproduction | `r3f2-repro-before-repair.txt`; Trumpet abandonment `…162605Z` | — |
| R3-F3 later Sickened | A later cast's Sickened surviving cleanup | `S7DustConditions`, `AcquisitionLedger` | Cell-unit executions own their conditions (captured at delivery); early-exit case | Live later-cast instance removed; early exit; foreign kept | `…170720Z` | Sickened that expires natively before cleanup is gone, not counted as live |
| R3-F4 trace release | Failed normal unpatch with PASS | `S7DisposeTrace`, `Sprint7ObserverPolicy.Released`, `S7ProjectileTrace` | Release verdict in the cell and outer checks; injected failed normal disposal | 4→0 positive control; injected 2 remaining, failed verdict, other releases run | `…170720Z`, `…162110Z` | — |
| R3-F5 Ghaele command | Timeout or abandonment after `Commands.Run` and before registration | `GhaeleLifetimeProbes.cs` (`CastOwned`) | Command and late summon owned from submission | Lifecycle pending and after-spawn cases with an unrelated command | `…162819Z`, `…163235Z` | — |
| R3-F6 attribution | Empty, inactive or unreadable samples counted as consistent | `GhaeleAttribution.cs` | Positive-only predicate with body-material identity; inconclusive classes | Domain negatives; native 11/11 positive; re-evaluated retained rows | `…162819Z`, `…163235Z`, `ghaele-attribution` | Retained round-3 rows lack body identity (labelled) |

Earlier findings: R1-F2, F4, F6, F7, F8, F10, F11, F12, F13, F14, R2-F1 and R2-F3 were verified in rounds 2–3. Their
evidence is retained; the round-4 runs re-exercise R1-F7/F8/F11/F12/F14 and R2-F3 on the final candidates.
