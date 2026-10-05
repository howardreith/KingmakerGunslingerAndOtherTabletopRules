# Phase 2B canonical finalization reconciliation

## Authority and current disposition

Owner Coordination and Phase 2B Finalization Mission, October 5, 2026.
Sprint 16 **NOT QUALIFIED**; six Dire roots withheld. Sprints 14-15 accepted;
Sprint 17 not started. Finish 16-17 and full Phase 2B closure, then STOP for
owner review. Phase 2C authorization remains true, with start deferred until
Phase 2B owner acceptance. No merge/release/tag/version bump/deployment permanence.
The old ownership/Phase-2C statements are superseded, not technical failures.

## Exact preserved ancestry

- Canonical branch: `codex/expanded-summoning-phase2b-finalize-20261005`.
- Actual fetched/imported historical tip:
  `3fca5aba1cdaaf6215fd5712409fc208855c79cf`.
- Local patch set/prior local tip:
  `c5058203f51633ed11d15ad35100dc01c53e250c`.
- Earlier local correction: `2d33437567ad2983763fd13c0900ba717a5b0f35`.
- Common ancestor of canonical/c505:
  `634b6c18972747a4bff0ceb27f29af5189e5f586`.
- Common ancestor of canonical/2d:
  `390f9a39d9cce2a40e6528d3215a678d6cd15757`.
- Intervening published repair:
  `08df2611bfaf22f675ea13c31babf35f304de1d7`.
- Later frozen-branch motion:
  `31c9ff0f4949bfda5db21225366f52d110953d13`; preserved, not integrated.
  It does not restart reconciliation or invalidate the canonical base.

Permanent local refs under `codex/local-safety/` (none removed):

| Ref suffix | Tip |
| --- | --- |
| phase2b-2d334375-owner-20261005 | 2d334375 |
| phase2b-previous-tip-owner-20261005 | 2d334375 |
| phase2b-fetched-634b6c18-owner-20261005 | 634b6c18 |
| phase2b-concurrent-08df2611-owner-20261005 | 08df2611 |
| phase2b-frozen-3fca5aba-finalize-20261005 | 3fca5aba |
| phase2b-c5058203-finalize-20261005 | c5058203 |
| phase2b-2d334375-finalize-20261005 | 2d334375 |
| phase2b-prior-local-tip-finalize-20261005 | c5058203 |
| phase2b-frozen-later-31c9ff0f-finalize-20261005 | 31c9ff0f |

The old worktree's `artifacts/phase2b-c5058203-recovery.bundle` verified:
SHA-256 `92EB8F0FE4E43D5918A1DB15C4576883E477C41D006773B31D6EE87A577FD22D`.
It contains c505 and requires 634. The earlier 2d bundle/ledger and all failed
artifacts remain in their existing ignored locations. No recovery was needed.
The older remote-ledger 3ebefe7 ref is absent in this local database, not deleted.

`git range-diff 634b6c18..c5058203 634b6c18..3fca5aba` found the single local
patch versus two distinct published runtime repairs; `git cherry -v` marks
c505 unique. Full source diffs and per-hunk overlaps were inspected, not merged
blindly. Replays use narrowly selected patches, not cherry-picking stale state.
The containing checkpoint is a normal descendant of imported 3fca5aba. No
reset, rebase, merge, force-push, rewrite or frozen-branch push was used.

## Ownership and policy

New canonical worktree is under the repository's `.worktrees/` directory.
Only root AGENTS.md governs; no nested instructions. Ordinary process metadata,
start/parent identities, worktree/Git-operation state and runtime leases found
no live owner of this exact canonical branch. No arbitrary process-memory CWD
query was used or claimed. No game, compatibility lock or active runtime lease.
The old source helper was released; its receipt cannot block this branch.

Canonical receipt: ignored `artifacts/canonical-owner-20261005T1930.json`.
Owner 31796/start `2026-10-05T19:29:25.9127956Z`; helper 11596/start
`2026-10-05T19:31:23.3048649Z`; acquired 19:31:23 UTC. The common-Git
CreateNew/FileShare.None handle expires on owner end or missing heartbeat.
Release before ending, and verify terminal receipt/process/lock state.
Unexpected canonical remote motion stops work; historical remote motion does not.

The policy helper initially rejected the new branch before source edits.
Owner added exactly the branch to its allowlist. This session did not edit or
bypass that policy. All publication uses the exact AGENTS.md command.
PR #26 stays draft/open as evidence; the new stacked draft supersedes it.

## Every c505 file / hunk disposition

Runtime abbreviates `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.`;
Summoning abbreviates `src/KingmakerGunslinger/Summoning/`.

| File | Disposition and resolution |
| --- | --- |
| Autonomous state | REWRITE current section for canonical authority/base/receipt, exact open gates and deferred Phase 2C. No layered contradictory current header. |
| Evidence index | RECONCILE current pointer and preserve local 390 failed artifact separately from remote 7c/634/08 evidence. |
| Implementation report | REWRITE current section; prior artifact passes do not qualify reconciliation. |
| Journal | RECORD canonical takeover and all decisions; prior entries remain history. |
| Program-state JSON | RECONCILE 9-21 authorization, canonical tranche B, Phase 2C true plus owner-acceptance deferral and mission endpoint 17; all published counts/accepted decisions preserved. |
| Fidelity matrix | REWRITE current Sprint 16 section with honest historical partial evidence, current NOT QUALIFIED and unchanged accepted omissions. |
| Prior owner reconciliation ledger | PRESERVED at c505 safety ref/worktree; obsolete stop, continuous Phase 2C and save-snapshot prescriptions are not replayed. This ledger replaces its current authority. |
| Prior sibling reconciliation ledger | RETAINED historical file unchanged; obsolete scope notice from c505 replaced by this controlling ledger. |
| Hidden candidate review | REPLAY local 390 artifact/result/restoration history; explicitly historical, proposed fixes superseded where this ledger differs. |
| Pre-candidate review | REWRITE current section; complete corrected batch remains mandatory. |
| Reconciled batch review | RETAINED immutable historical failure record; c505 supersession header unnecessary under current ledger. |
| Invoke-KingmakerRuntimeTest.ps1 | REPLAY closed optional mechanics/combat/lifecycle diagnostic scope; exact working save and automatic exit required. |
| RuntimeAutomation.Common.ps1 | REPLAY matching typed allowlist, parameter count and request serialization. Full default unchanged. |
| Test-ExpandedSummoningWorkingSavePersistence.ps1 | REPLAY 3 diagnostic round trips / 8 rejection cases alongside existing persistence boundary tests. |
| RuntimeTestRequest.cs | REPLAY matching in-game validation; no wider scenario authorization. |
| Runtime ExpandedSummoningSprint16.cs | REPLAY diagnostic routing/warning; RETAIN remote starting-census native stale-summon cleanup. Default runs every stage. |
| Runtime ExpandedSummoningSprint16Lifecycle.cs | REPLAY dead/mode/clock/deadline diagnostics. OMIT longer visible-frame loop: remote native wake/lifecycle passed on 08df; retain its exact path pending new evidence. |
| Runtime ExpandedSummoningSprint16Mechanics.cs | REPLAY real pre/post Animal Growth physical oracle, creature/item/rule size diagnostics, lethal before/after-life and no-check/no-damage replay, idempotent production reset on explicit owner/prey cohort with outside-cohort Purple Worm control. RETAIN remote native/explicit blueprint diagnostics. |
| Runtime ExpandedSummoningSprint16Persistence.cs | SUPERSEDED by remote equivalent appearance cleanup, live-versus-cached census and buff diagnostics; retain remote broader live census and native destruction without premature Dispose. Do not replace it with local earlier cleanup path. |
| Runtime ExpandedSummoningSprint16Ui.cs | REPLAY diagnostic completion and section diagnostics. OMIT mode/clock mutation and extra close-toggle/wait: remote native Show(false) passed, while the local opened=false would bypass it. Preserve exact native restoration predicates. |
| Summoning CrocodilianRulesPolicy.cs | REPLAY Fine/Diminutive enum bounds and invalid-size rejection. RETAIN remote ResolveDiceBaseline: native Medium-relative versus explicit creature-baseline dice. |
| Summoning ExpandedSummoningPteranodonViewPatch.cs | REPLAY original-donor reference exclusion from rollback clone destruction. RETAIN remote exact CrocodilianPose attachment/restoration/destruction. |
| ExpandedSummoningCrocodilianRulesTests.cs | REPLAY live-body/fixed-item/dynamic-item size behavior and Fine/Diminutive bounds; RETAIN remote native-vs-explicit baseline tests. Tiny-minus-one expectation corrected to native Diminutive. |
| DomainTests Program.cs | REPLAY one behavior-test registration: 2019 total, 217 focused expected. |
| validate_expanded_summoning_phase2a141.py | RECONCILE count 2019 and current authority/deferral schema; immutable released metadata unchanged. |
| static-validation.json | RECONCILE canonical branch, 9-21 principle authorization, current mission end 17, Phase 2C true/deferred, count 2019. No roster/package-count waiver. |

## Why published runtime-derived corrections survive

The canonical tree starts at the complete 3fca5aba commit, including its
fixed crocodilian contact adapter and exact native manual-control request.
SpecialCombatComponents, SummonGrappleLinkState, main cleanup/destruction,
Sprint16Combat and weighted world-contact measurement are not replaced by c505.
The native-vs-explicit dice baseline is retained beside the local size bounds.
Dead/destroyed-prey maintain refusal and all-owned native load release remain
the production paths exercised by the added checks. Original art, identities,
reach, contact tolerance, AI action list and publication suppression are unchanged.
No duplicate disposal, swallow damage, UI close or damage-scaling path was added.

## Qualification boundary and next action

Reconciliation inner gates PASS: 217/217 focused of 2019 registered; active
static/manifest validation; incremental exact-reference Release compile;
68 orchestration assertions; persistence 11 wiring + 3 round trips/19 rejections;
diagnostic 3 round trips/8 rejections; crowd 5 round trips/7 rejections.
The orchestration script's default ScriptRoot was empty under this invocation;
explicitly passing the canonical scripts directory passed without source changes.
No full exact-head package or runtime has run yet.

Post-reconciliation review identified two imported pre-candidate concerns:
the contact adapter looks for weapon stats on the unit rather than its declared
AddFacts combat trait; native capital direct control checks Master against the
main character before it considers the summon-control part. Inspect and correct
only these demonstrated ownership/fixture seams before freezing the candidate.
Neither concern is a reason to discard any native dice/destruction repair.

After focused/static/request/compile checks, commit/policy-push NOT QUALIFIED,
open the canonical stacked draft and label PR #26 historical without closing it.
Run every owner-required prelaunch gate on that exact committed candidate:
full unfiltered suite, repository/static/icon/manifest, preflight/orchestration/
persistence/crowd boundaries, clean exact-reference Release, deterministic
package and strict validation. Record source fingerprint, DLL SHA/MVID, ZIP,
member count and requests. Prior source qualification is not reused.

Then lease BEFORE actual installation observation/snapshot, and run one immutable
six-scenario fresh-Steam batch: smoke, full main, crowd, prepare, cleanup, absent.
This machine's working save still has the local 390 failed cleanup fixtures;
the remote environment's clean absence is not a local-save observation.
Only native guarded writes may normalize it. Never copy/parse/edit/replace saves.
Restore the exact actual installation snapshot; verify count/version/fingerprint
and no game/lease/lock/staging. No new runtime or save write is claimed here.
All four accepted decisions and HumanReview: NOT_PERFORMED_NONBLOCKING remain.
