<!-- agent-review:v1 enrollment=kmg-codex-s7-recovery-c3-effects-ownership-20261125235239-ba5d key=r04-request part=1/2 sha256=2ef52fc45f03e5bda7a22e0bd89c22b3909ad4a0c1fdd86e4f7d33ec251a5a5f -->
_Part 1 of 2._

### Claude Opus — review request — round 4 — candidate `43dd44a2ba`

| Field | Value |
|---|---|
| Request ID | `kmg-codex-s7-recovery-c3-effects-ownership-20261125235239-ba5d-r04-69cc0c` |
| Checkpoint | `final` (final) |
| Candidate H | `43dd44a2bafad0455ab74b6393515c9bae4fd27a` |
| Target B (`codex/s7-recovery-c2-ghaele-late-units`) | `eab90a5ac82f661ecb903c5456d951a57a085eb9` |
| Merge base M | `eab90a5ac82f661ecb903c5456d951a57a085eb9` |
| Mission | `s7-recovery-c3-mission` sha256 `b95649a59a` |
| Workflow | `6d613defe7` |
| Owner guidance | none |
| Owner amendments | revision 3, sha256 `a64ab24d9e7445495e895b5d7f851bbf6f846722f2ffba1c8f0bc7d48a47ef1a` |
| Developer observed | foreground session `438fdcd7-910d-4c0e-8441-dde58c90713d`: models ['claude-opus-5-5'], effort ['high'], permission modes ['auto'] (CLI-recorded transcript; the headless permission profile does not apply to a foreground session) |
| Reviewer requested | Codex `gpt-6.2-astra`, effort `max`, read-only sandbox |
| Round budget | 4 of 4 |

**Scope:** The complete mission as written.

**Exit conditions:**
- Every requirement of the mission is implemented, with evidence appropriate to each claim; no requirement is weakened or silently dropped.
- Validation follows the repository's own instructions; commands, results and limits are reported honestly.
- Deviations, limitations and deferred optional items are stated explicitly, separate from required work.

**Developer summary:**

Sprint 7 recovery child C3, review 4 of 4 under OWNER C3 AMENDMENT 3 (revision 3), on H=43dd44a2ba, PR #223 targeting C2's branch at its archive A eab90a5ac8. Full corrected-byte result: the amendment-2 corrections plus the amendment-3 corrections of the pre-build audit evidence (A1-F1, A1-F2, A1-F4, A1-F5, A1-F6, A1-F9, A2-F1 to A2-F3; both audits and their original corrections_required verdicts preserved, A1-F3/A1-F7/A1-F8 verified by the follow-up at their scopes) with actual-caller focused tests, and NATIVE QUALIFICATION of those bytes: recovery candidate 4 (source 6cef765a8a = code d9a07ef301 plus records, package 242f9a27358c, DLL a08d06b6e0a8, C2 harness d77754bad437) passed the mechanics-only session 20261126T232851Z-owner-0.33.0-s7-c3-mechanics, 43/43, every declared required cell admitted and completed, physical restoration true, drivers proven (no native transfer-failure claim), a real late delivery after pre-delivery disposal released with a known zero, guarded command cleanup performed with nothing withheld, real constructed cue posts and live/persisted/collector-shaped verdict agreement for that run. C1 and C2 acceptance unchanged. Recovery validation 75 min 47.4 s of 90; candidates 4 of 4; this is the last formal round.

**Validation (developer-reported):**

| Command | Required | Result | Duration | Scope and limits |
|---|---|---|---|---|
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-recovery-c3-effects-ownership-20261125235239-ba5d --gate focused -- -Filter sprint7-visual | yes | pass | 5.7s | Focused gate on H with production compilation; receipt 3.8s; worktree unchanged. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-recovery-c3-effects-ownership-20261125235239-ba5d --gate focused -- -Filter summon-playtest-r6 | yes | pass | 4.1s | Focused gate on H with production compilation; receipt 1.9s; worktree unchanged. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-recovery-c3-effects-ownership-20261125235239-ba5d --gate focused -- -Filter native-profile | yes | pass | 4.0s | Focused gate on H with production compilation; receipt 2.1s; worktree unchanged. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-recovery-c3-effects-ownership-20261125235239-ba5d --gate focused -- -Filter loader-compat-probe | yes | pass | 4.2s | Focused gate on H with production compilation; receipt 2.1s; worktree unchanged. |
| Build-Local -DevelopmentPurpose s7-recovery-c3 -DevelopmentTestFilter sprint7-visual (recovery candidate 4 of 4) on 6cef765a8a | yes | pass | 8m8.9s | Package 242f9a27358c11e35a43500e924e5011c06bf843ad23f7a661923a98e3270d93, DLL a08d06b6e0a81df22314314ad7b970f0cd264d29296f913e589f12e7ce5491c2; 25 selected tests. Source 6cef765a8a = code commit d9a07ef301 plus records (docs/ only); H differs from the source only in docs/ and tests/ (git diff outside them is empty). |
| Invoke-LoaderCompatSession.ps1 s7-c3-mechanics.json on owner-0.33.0 with recovery candidate 4 and harness d77754ba | yes | pass | 5m59s | Session 20261126T232851Z-owner-0.33.0-s7-c3-mechanics PASS, 43/43 checks; source 6cef765a8a, package 242f9a27358c11e35a43500e924e5011c06bf843ad23f7a661923a98e3270d93, DLL a08d06b6e0a81df22314314ad7b970f0cd264d29296f913e589f12e7ce5491c2, harness d77754bad4377734e87c9a70aaf0c980a306db82dbee25e335b9e9ba47ebb4f1 (C2's, unchanged, staged hash read back), fixture the admitted disposable outdoor native Default fixture of disposable-expanded-summoning-sprint32-native-review, selection summon-s7-effects-mechanics (session file unchanged; seven required cells declared before the run). All seven admitted and completed; physical restoration true and recorded separately (no failure, pending, unreadable or unreported entry); all five drivers proven with empty transferredResiduals (so no native claim for failures after a handoff); ordinary pre-delivery disposal in its readiness frame 11797 with a real late delivery (acquired 1) released at settlement (known presence 0); both foreign controls present after the owned release and kept; guarded queue maintenance and combat exit performed on every actor and nothing withheld (no foreign queued work natively: the withholding branch is focused coverage only); constructed same-frame cue posts with real IDs 471/472 both playing; live verdict null. Transaction completed; owner files and saves verified unchanged; game settings applied and registry restored. Window 23:28Z-23:35Z agreed with the other session on DATA. Mechanics-only: no frames, native-look comparison or listening samples. |
| Invoke-KmgGate.ps1 -Filter sprint7-visual (local, three runs during the amendment-3 correction) | no | fail | 29.6s | 21.7s FAIL 24/25 (an earlier R2-F2 terminal assertion still expected the old reading; updated to amendment 3 section 3B); 2.3s FAIL 24/25 (rerun with the correcting edit refused as stale, not applied); 5.6s PASS 25. Recorded as two failed attempts. |
| Invoke-KmgGate.ps1 -Filter sprint7-visual (local, with the candidate-4 same-run agreement) | no | pass | 4.9s | 25 tests. |
| Shared recovery ledger | no | pass | 75m47.4s | Recovery validation after these controller gates: 75 min 47.4 s of 90, remaining 14 min 12.6 s (the committed ledger stops at 75 min 28.9 s, before this 18.5s gate batch; receipts 9.9s). Candidates 4 of 4 (candidate 5 not authorized); formal reviews 3 of 4 before this one. Failed validation attempts in the recovery: twenty (two since C3 amendment 3). Candidate 3's 20261126T134425Z PASS stays bound to 66b0ff3f14 and does not qualify these bytes; 20261126T051633Z stays a FAIL. |

**Evidence references:**
- prepared under owner amendments revision 3 sha256 a64ab24d9e7445495e895b5d7f851bbf6f846722f2ffba1c8f0bc7d48a47ef1a
- owner-guidance S7-RECOVERY-C3-AMENDMENT-3-2026-11-26.md (message sha256 a82cb5ce8ba6c84f4b216c49d4739c0d287cc47e1c53affee1ea2f4aa83cf34b)
- docs/missions/sprint7/recovery/S7-RECOVERY-C3-MISSION.md
- docs/missions/sprint7/recovery/S7-RECOVERY-MANIFEST.md
- docs/evidence/sprint7/recovery-c3/pre-build-audit/audit-1-initial-a84d0b0d07.json
- docs/evidence/sprint7/recovery-c3/pre-build-audit/audit-2-followup-03a78d4987.json
- docs/evidence/sprint7/recovery-c3/round3-acceptance-ids-crosscheck.json
- docs/evidence/sprint7/recovery-c3/MANIFEST.json
- docs/evidence/sprint7/recovery-c3/20261126T232851Z-owner-0.33.0-s7-c3-mechanics-01-mechanics.json
- docs/evidence/sprint7/recovery-c3/20261126T232851Z-owner-0.33.0-s7-c3-mechanics-session.json
- docs/evidence/sprint7/recovery-c3/20261126T232851Z-callback-loss-row.json
- src/KingmakerGunslinger/RuntimeTesting/Sprint7ObserverPolicy.cs
- src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.Sprint7Presentation.cs
- src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.SummonPlaytestR6Presentation.cs
- tests/KingmakerGunslinger.DomainTests/Sprint7VisualRepairTests.cs
- scripts/compatibility/loader-compat/sessions/s7-c3-mechanics.json

**Prior findings to assess:** `R1-F11`, `R1-F13`, `R1-F9`, `R2-F7`, `R2-F8`, `R3-F1`, `R3-F2`, `R3-F3`, `R3-F4`, `R3-F5`

**Owner amendments to the mission — revision 3, captured-source sha256 `a64ab24d9e7445495e895b5d7f851bbf6f846722f2ffba1c8f0bc7d48a47ef1a`.** Applied by the owner with `resume --amend-mission`; each owner message is preserved verbatim, in order. The effective task is the original mission plus these amendments. The reported work comes from a developer invocation whose input included them, and the reviewer of this round receives a byte-exact copy of the captured document. Public rendering after the controller's standard sanitization; the digest identifies the private captured bytes, which this rendering can differ from.

```text
# Owner amendments to the mission

Cumulative record kept by the agent-review controller. Each owner message below was applied by the owner through `resume --amend-mission` and is preserved verbatim, in order, between its labeled marker lines. Later amendments supersede conflicting earlier requirements; requirements they do not change still apply.

----- BEGIN OWNER AMENDMENT 1 (captured 2026-11-26T12:18:28Z; message sha256 5294e5c6e920fe6c1d606148a8aa14bcbf53939f25ff6e7a50ebb7f7b89fa59f) -----
SPRINT 7 RECOVERY — C3 AMENDMENT 1: COMPLETE THE OWNERSHIP REPAIR AND HANDOFF  
2026-11-26

I authorize continuing the existing C3 recovery through the bounded sequence below. Complete the corrections, qualification, review and handoff autonomously within this authority. Do not return for permission between these already-authorized steps.

This amends C3's approach, task-specific validation/package allowances and review ceiling. The original mission, earlier evidence and all unaffected constraints remain in force. It does not authorize integration, resumption of the parent review, merge, release, tags or changes to my human installation.

## 1. Preserve the existing stack and record the accepted scopes

Repository: howardreith/KingmakerGunslingerAndOtherTabletopRules.

Parent #219:

- Branch codex/s7-visual-audio-remediation.
- Frozen head 0542b0592a668aad0c62c66d5f2dc862460818fe.
- Its enrollment remains BLOCKED at 7/7.
- That head is a records head, not an archive A. No parent archive exists.

C1 #221:

- Reviewed H 5a56d480a2827e0ddd3918e30d32a07c7554e112.
- Archive A 2c67a049ee33e0b29621fd29a8b25608ed31e956.
- Its implementation/domain acceptance remains in place.
- I accept fulfillment of its deferred same-run native evaluator criterion by session 20261126T054228Z on product source 6abd692fa7ee84604ef8122569f84a57e3b3c1a2, package de63d6f4d155cb7844f120355c20a73d77e0fc8626b63eb3200b5474a4b24b46, DLL ef66c870150fa7e100597fc8c57c8833b39d10b5c7afaa7d0fd4c4f078acb57a.
- The in-game callback row and its committed persisted row agree, and the persisted and live-shaped interpretations agree. Keep the constructed event scheduling explicit. This acceptance belongs to those bytes and that run; it does not qualify later corrected bytes.

C2 #222:

- Reviewed H da811b123c5720ad2d174db17142d8f4ef8fa5d6.
- Archive A eab90a5ac82f661ecb903c5456d951a57a085eb9.
- I accept its external-harness implementation and stated native lifecycle scope, including session 20261126T033419Z with harness d77754bad4377734e87c9a70aaf0c980a306db82dbee25e335b9e9ba47ebb4f1 on the unchanged parent candidate-3 package.
- Preserve that package/harness attribution. Do not rerun C2 solely because C3 changes.

C3 #223:

- Branch codex/s7-recovery-c3-effects-ownership, targeting codex/s7-recovery-c2-ghaele-late-units at C2's archive.
- Enrollment kmg-codex-s7-recovery-c3-effects-ownership-20261125235239-ba5d.
- Round-1 reviewed H 7fff8b34f6d925d91e80beddb6501cfad249e33d.
- Reported current head 254c08c53d171ff02055ad1c3ffa1c1e30b7961e, with records-only updates after review.
- It has no archive or handoff. Its reported controller state is ACTIVE/develop at 1/2; development stopped because task-specific package and validation allowances were insufficient.

Record these acceptances in the active C3 manifest. Keep #219, C1 and C2 frozen. Do not amend their branches, invalidate their handoffs, reset historical findings or relabel old FAILs.

Read the current workflow instructions and verify the current refs and controller state. Reconcile an unexpected remote change without overwriting it. Do not begin another enrollment or repair PR.

## 2. Apply this authority through the supported controller

Save this instruction verbatim as:  
C:/Dev/KingmakerGunslingerLab/owner-guidance/S7-RECOVERY-C3-AMENDMENT-1-2026-11-26.md

At a safe foreground boundary, apply it to the existing C3 enrollment using the installed controller's supported resume --amend-mission operation. Separately set the existing enrollment's round ceiling with resume --max-rounds 4, with an accurate owner-authorization reason. Do not combine those operations or use --reason as a substitute for the instruction.

Display and read every part of status --instructions in the enrolled developer session. Record the returned amendment revision and digest. Preserve the controller's pinned workflow, gates, role separation and required report provenance.

This authorizes four C3 reviews in total, not four additional reviews. Round 1 remains spent. No automatic extension, reset, alternate enrollment or manual controller-state edit is authorized.

## 3. Complete one coherent ownership repair

Read the actual round-1 review:  
https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/223#issuecomment-7184143447

Address R1-F1 through R1-F13 and directly related defects in the corrected callers. These are runtime-verification and evidence repairs. Gameplay, presentation, balance, assets and the accepted C1/C2 implementations remain outside this correction.

Trace each affected resource through acquisition, partial acquisition, ordinary completion, early interruption, iterator disposal, scenario teardown, fixture teardown, settlement, next-cell admission and the final verdict. Use the existing ownership structures and project style. No new library or general-purpose resource framework.

Required repairs:

### A. Cue-source retirement across all owners — F1

Carry each exact source's release-attempt and settlement result through direct retirement, phase teardown and outer fixture teardown. A failed request must not be issued again by another layer. A request returning does not establish disappearance. Keep unresolved sources accounted for; do not remove them from ownership merely to suppress a retry. Independent cleanup and final reporting must continue after one failure. Keep any shared-fixture change narrow to the affected path.

### B. Dust commands, executions and late effects — F2, F3, F4

Retain the exact execution before any interruption can clear it, including ordinary early exit and the step-4 cancellation. An unreadable execution is a named unknown associated with its command, not a known null.

Continue observing or explicitly reconciling attributable condition delivery until retained executions settle. Ordinary cell disposal before delivery must not leave a late condition unowned. Process completion alone does not prove its delivered effects are cleaned up.

Publish early release failures immediately into the cumulative result consumed by admission and the final phase. Later success must not erase them. Attempt independent command releases despite another failure. Preserve pre-existing and foreign effects by identity. Do not introduce a global engine tick or cleanup of unrelated commands.

### C. Hook rollback and partial setup — F5, F9

Create a cleanup scope and retain each attempted acquisition before it can partly succeed and throw. A constructor that fails to return an object must not lose the identity needed to release a subscription or hook.

Release known attempted hooks independently of whether diagnostic counts can be read. Failed count/readback operations are unknown, never zero. Preserve the original setup error alongside rollback and readback errors; one diagnostic failure must not prevent independent releases.

### D. Complete child outcomes, repair and runner completion — F6, F7, F8, F10

Consume the complete child result, including unreported owners. Match an intentional injected fault by its exact operation, resource identity and expected outcome. Substring matches and message text containing the expected phrase must not exempt another failure.

Publish the exact injected-condition repair result on every exit, including disposal and nonthrowing failure. Preserve its identity for settlement even if temporary injection bookkeeping is cleared. A repair attempt is not proof the condition disappeared.

Separate eventual physical restoration from completion of the declared qualification. A refused or skipped required parent cell remains incomplete even if its residual later settles. Define the required cell set for the selected mechanical profile. Expected refusals inside negative-control child scenarios remain distinguishable from refused required parent cells.

Enumeration, Current, disposal and completion-publication failures must reach consistent cell and phase outcomes. Disposal-only failure cannot publish successful completion. Preserve original and cleanup errors together.

### E. Exercise the actual failure boundaries — F11

Use the real shared orchestration, admission, disposal and publication paths with controlled failing boundary operations. Do not inject a finished failure list, supply an outcome after the real failure boundary, or maintain a test-only copy of production cleanup.

Cover the concrete counterexamples in F1–F11, including both source-retirement failure positions, a late effect after ordinary cell disposal, an unreadable execution before interruption, partial hook/subscription acquisition, an additional failure beside the intended injected fault, an unsuccessful nonthrowing repair, a refused required cell followed by eventual restoration, and disposal/publication failure.

Keep these behavioral tests focused. Use native checks for engine-owned lifecycle boundaries; do not add a game launch for every aggregation or representation case. No broad framework, unrelated suite or synthetic-case expansion beyond the affected contract.

### F. Correct the session scope and records — F12, F13

F12 is only partly addressed: the disclosure is present, but the existing selection still runs the full inherited presentation phase. Implement and test an explicit mechanics-only selection through the actual enclosing runner.

Retain the mechanical ownership, command/process/hook, cue, recolour-restoration and foreign-effect controls needed for this repair. Omit new frame captures, native-look taste comparisons, alternative-cue listening samples and other owner presentation work. Declare the required mechanical cells before the run; do not retain a stale check total as the acceptance criterion.

Finish the conditional integration plan and correct any overbroad "addressed, records only" wording. Distinguish reviewed H, current branch head, product source and archive A. Current linear ancestry does not guarantee future integration with master will be conflict-free.

## 4. Review the corrected code before building another package

First complete the coherent source, focused tests, mechanical selection and record corrections. Run the relevant controller-required Focused gates with production compilation.

Use C3 review 2 to review that corrected source/domain/records before spending the next package. This amendment expressly permits that review submission before corrected-byte native qualification.

Report each finding fixed only when the committed correction and its source/domain evidence support that disposition. State prominently that the newly changed compiled code has not run in the game and still needs C3's native qualification. Earlier receipts retain their original identities and scope. Do not invent a passing required validation or call C3 owner-ready.

Full C3 readiness still requires the native check in section 5. The reviewer may verify source fixes while returning changes_required for that explicit native gap and any remaining defects.

After review 2, repair its concrete code/test findings offline and rerun affected controls. Then proceed to the authorized build and native check without another owner permission request.

Do not submit a second code-only formal review while the native gap remains open. Do not disguise an unresolved blocker as an optional deferral to get a report accepted. The next formal report must include the corrected-byte native evidence.

Use the remaining formal rounds for that full-scope result and, if needed, one concrete correction. All required findings must be resolved before finalization; expected source-review progress is not a handoff.

## 5. Qualify the delivered correction

After the source-review corrections and affected gates are complete, build the next combined recovery candidate once and record its source, package and DLL hashes.

Run the mechanics-only effects session on that package, using the admitted disposable fixture and exactly identified compatible harness. Keep the actual enclosing runner, admission checks, final collector and evaluator.

Establish:

- Every declared required parent cell was admitted and completed; no later settlement can conceal a refused or skipped required cell.
- The ordinary Dust disposal-before-delivery path retains its execution and accounts for late attributable effects through settlement.
- The engine-dependent hook, command, abandonment and restoration boundaries affected by this repair work through their actual callers.
- Unexpected failures survive child repair and reach final isolation; intentional negative controls prove the expected refusal without exempting additional failures.
- Real production-handler cue posts, under declared constructed scheduling, yield the two-source observation without fabricated playing IDs or forced verdicts.
- The in-game verdict and the committed persisted-row verdict from that exact run agree, including a collector-shaped in-memory comparison.
- Foreign controls and unrelated effects remain accounted for, and transactional restoration verifies the owner files, settings and registry.

Do not repeat model, full Ghaele, broad expiry/combat, visual-taste or listening qualification. Keep C2's qualification at its own accepted package/harness identity. Keep the r5 listening packet and existing visual material unchanged.

A pure failure-publication case already established through the real orchestration in focused tests does not need a separate native launch. Keep setup disclosures, assertions, captured observations and source inferences distinct.

## 6. Exact amended ceilings and contingency

The shared recovery validation allowance increases from 45 to 75 minutes total. The existing 42 min 28.9 s remains spent, leaving 32 min 31.1 s at this amendment. This is 30 additional minutes, not a fresh 75-minute allowance. Retain earlier parent expenditure separately.

All validation-command wall time counts, including failed/refused attempts, builds, gates and game sessions. Source implementation and model review remain outside that clock under the existing policy.

The recovery product-candidate ceiling increases from two to four total:

- Two are already used.
- Target one additional combined recovery candidate: recovery candidate 3.
- Recovery candidate 4 is a contingency only for a concrete defect discovered while qualifying candidate 3 or in its subsequent scoped review that requires changed compiled code. Repair and run affected focused controls before that rebuild.
- This numbering is separate from the older parent candidate 3 reused by C2.

A same-byte rerun is allowed only for a diagnosed qualification/setup problem with a concrete correction and sufficient allowance. Do not repeat an unchanged attempt hoping for a pass.

Before each expensive command, update the ledger and estimate the remaining build, native and final-gate work together. Do not start a package if its required qualification cannot reasonably fit. Safe in-progress restoration must finish even at a ceiling.

The caps are ceilings, not targets. Do not consume a contingency without a concrete need, broaden the scope, reset the ledger or change global VALIDATION-POLICY.md.

Continue autonomously through authorized fixes, reviews and qualification. Stop with the exact controller state and concrete remaining blocker if a mandatory stop occurs or the amended allowances cannot complete the required work. Do not raise limits again, re-enroll or grant yourself another retry budget.

## 7. Complete the C3 handoff and return the concrete stack

After the full-scope reviewer result permits it, use the normal finalize and status --remote sequence. Verify AWAITING_OWNER_REVIEW and handoff_current=true. Preserve final reviewed H and archive A separately, and keep the handoff current.

Update the manifest, finding/evidence map, PR summary and status records. Preserve every earlier FAIL and all original receipts. Record C2's acceptance and C1's fulfilled native criterion in C3 only.

Return:

- The frozen parent and each child PR, target, base, reviewed H, archive A and exact acceptance/handoff scope.
- Every C3 finding's correction and verification, including F12's actual mechanical selection.
- The final package/DLL/source, harness, fixture, collector and same-run live/persisted evidence identities.
- Required-cell completion separately from physical restoration.
- The complete validation ledger, all failed attempts and unused allowance.
- Restoration receipts and any remaining qualified limits.
- A concrete proposed integration operation and order preserving the reviewed history, with any remaining master-integration or parent-review gate clearly identified.

The proposed plan may group routine integration checks under one later authorization. Do not require separate permission for each read-only check. Do not execute the integration plan now.

Do not advance #219, merge children into their prerequisites, resume the parent's 7/7 enrollment, merge to master, tag, release or change my human installation. Visual approval for T2/T3/T6 and listening choices for T5/T7 remain pending; do not substitute technical passes for those choices.

Finish at a current, reviewable C3 handoff or a concrete authorized-limit stop. The work between those boundaries is already authorized.
----- END OWNER AMENDMENT 1 -----

----- BEGIN OWNER AMENDMENT 2 (captured 2026-11-26T16:16:07Z; message sha256 bbde9e734c26919ac78e33acfca158fb3e1957daa203ca856e02b7f9e40dc2c2) -----
SPRINT 7 RECOVERY — C3 AMENDMENT 2  
2026-11-26

I authorize continuing the existing C3 enrollment past its recorded no-progress stop, under the scope and limits below. Finish the correction, bounded pre-build audit, candidate qualification and remaining formal review autonomously. Routine steps within this authority do not need another permission request.

This instruction supplements C3 amendment 1. It preserves the acceptance contract, frozen prerequisites and evidence. It does not authorize another product candidate beyond recovery candidate 4, another formal review beyond C3 round 4, integration, parent-review resumption, merge, release, tags or changes to my human installation.

## 1. Preserve the exact state and use the supported owner amendment

Repository: howardreith/KingmakerGunslingerAndOtherTabletopRules.  
C3 PR: #223.  
Branch: codex/s7-recovery-c3-effects-ownership.  
Target/base: codex/s7-recovery-c2-ghaele-late-units at eab90a5ac82f661ecb903c5456d951a57a085eb9.  
Enrollment: kmg-codex-s7-recovery-c3-effects-ownership-20261125235239-ba5d.  
Latest reviewed H: f0ca78cbab9b57583379a575208a15304aae7eb1.  
Reported current records head: 80d224bd5614573fb48f7a0fb01071a3a30a8f74.  
Controller: BLOCKED in develop at 3/4 because R1-F9, R1-F11 and R1-F13 remained open for three consecutive reviews. No C3 archive A or handoff exists.

Save this instruction verbatim at:  
C:/Dev/KingmakerGunslingerLab/owner-guidance/S7-RECOVERY-C3-AMENDMENT-2-2026-11-26.md

Read the installed workflow instructions and verify the current remote identities. Apply this instruction to the same enrollment through the supported resume --amend-mission operation, with a reason explicitly recording my authorization to continue after this no-progress stop. Display and read all returned instruction parts in the enrolled developer session, and record the amendment revision and digest.

Keep the formal review ceiling at four total; do not increase --max-rounds. The fourth review is already available once the authorized resume succeeds. Let the supported controller handle its normal consecutive-open counter bookkeeping; do not edit counters, findings, historical verdicts or state files.

Keep these prerequisites frozen:

- #219: head 0542b0592a668aad0c62c66d5f2dc862460818fe, BLOCKED 7/7, no archive A.
- C1 #221: H 5a56d480a2827e0ddd3918e30d32a07c7554e112; A 2c67a049ee33e0b29621fd29a8b25608ed31e956. Its accepted implementation/domain scope and accepted same-run native criterion remain unchanged.
- C2 #222: H da811b123c5720ad2d174db17142d8f4ef8fa5d6; A eab90a5ac82f661ecb903c5456d951a57a085eb9. Its accepted harness/native scope remains tied to harness d77754ba…b4f1 and session 20261126T033419Z.

Unexpected remote changes must be reconciled without overwriting another actor's work. No new enrollment or repair PR.

## 2. Retain the native progress at its actual scope

Preserve the PASS of 20261126T134425Z on:

- Source 66b0ff3f14efdd29c80f5f9091bec3521eacfc6d.
- Package 473f2f5a78229f3e2e15c3d97b863f2201f44ae372d2485de1fdb05010618366.
- DLL 8e574477cce11621ca2ec88fdeed358cb9f8f76ff8ced423e154e3c86d277d07.
- Harness d77754bad4377734e87c9a70aaf0c980a306db82dbee25e335b9e9ba47ebb4f1.

That run established 43 passing checks, all seven required cells admitted/completed, separate physical restoration, actual late Sickened delivery and removal after pre-delivery disposal, and same-run live/persisted evaluator agreement. It produced no new frame captures or listening samples.

Its five driver rows have empty transferredResiduals arrays. Do not describe it as native qualification of failures or new residuals produced after a driver hands pending cleanup to the phase. Preserve that limit alongside the PASS.

Keep every earlier FAIL and receipt. Changed compiled bytes still need their own affected native qualification.

## 3. Correct the ownership lifetime, then verify the actual callers

Read the full round-3 review:  
https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/223#issuecomment-7194404698

The outstanding findings are R3-F1 through R3-F5, R2-F7, R2-F8, R1-F9, R1-F11 and R1-F13. Address them within C3's runtime-verification code, focused tests and records. Keep gameplay, presentation, assets, balance, C1 and C2 outside the correction.

### A. Deferred ownership and its result destination — R3-F1 and R3-F4

Keep one continuously owned cleanup operation through immediate completion, pending settlement, child abandonment and terminal teardown.

When cleanup outlives its child, every subsequent failure and newly created residual must reach an owner that the phase continues consuming. Either retain the live child outcome until cleanup finishes or forward subsequent publications to the enclosing owner. Moving delegates while their closures still publish only to the departed child is insufficient.

Preserve the original child history and intentional-fault classification. Do not replay expected child failures as unexpected phase failures, and do not lose later unexpected failures or an outside-target retirement.

Retain reconciliation and its terminal action before attempting settlement. Remove that obligation only when completion is established. At terminal teardown, an obligation that cannot be established as settled becomes a named unresolved failure after its authorized cleanup attempts; it must not be reported as restored or waited on indefinitely. A final unreadable or pending result must retain executable cleanup until that terminal accounting, even if an earlier census said every execution had ended. Use the same reconciliation contract for immediate and deferred paths.

Use the simplification already allowed by the mission: an unnecessary recovery into later success cells may be removed from a destructive negative-control child. It may finish after proving failed isolation and refusal. Its enclosing owner must still attempt and account for cleanup and complete transactional restoration.

### B. Recover ownership without inventing delivery evidence — R3-F2

Reconcile present conditions against exact retained execution contexts when their identities become readable, including an already-ended process whose delivery notifications were missed.

Do this after context recovery even when the first live-execution census was zero. The earlier release cannot stand as proof that a subsequently attributable condition was absent.

Keep ownership-context knowledge separate from the observed-delivery collection used by Delivered() and the pre-delivery readiness probe. Discovering a process context must not fabricate an observed delivery or invalidate the demonstrated disposal-before-delivery ordering.

Preserve reference identity and pre-existing/foreign exclusions. Unresolved attribution remains a named uncertainty that prevents clean isolation; an unrelated unreadable process must not hide a positive match to a known one.

### C. Exact command cleanup and independent operations — R2-F7

Record the actual commands issued by the effects runner and select them by reference during teardown. Owning a summon does not make another owner's queued command part of this cleanup.

Keep the deliberately separate same-caster control command outside the owned-command set. Guard interruption, permitted queue maintenance and combat cleanup independently, so an interruption failure skips neither later commands nor that actor's other authorized cleanup.

Test preservation at the command-cleanup boundary. Intentional later destruction of a fixture-owned actor is a separate terminal; do not claim a foreign command survives destruction of its actor.

### D. Controls, outside-target ownership and setup — R3-F3 and R1-F9

Declare which controls a path requires before evaluating them. An ordinary path with a missing required same-caster handle must produce a named isolation failure. A path deliberately stopped before that control was due must retain its distinct expectation.

Read each required presence result once, preserving present, absent or unreadable. Populate diagnostics from that result. Diagnostic failure must not prevent independent releases, retirement or publication.

Retain the outside target's exact retirement obligation when it is acquired. Its cleanup must remain reachable through settlement and terminal teardown, even if a control read fails. Use the existing one-request retirement record and settlement accounting.

Include ground lookup and baseline hostile placement in the actual guarded phase-setup operation. A placement exception after subscription must remain in phase isolation while both subscription releases and position restoration are attempted.

### E. Exact condition failure identity — R2-F8

At the real removal boundary, retain the operation, exact condition identity and original exception. An engine exception message need not contain an ID. Publish the named failure once and keep it if the condition later expires or another read fails.

Preserve exact recognition of the intentional injected fault without using exception-message contents as the only source of resource identity.

### F. Test production responsibility, not a reconstructed result — R1-F11

Exercise the production retirement operation that writes the actual fixture retirement-attempt collection before native disposal. Pass that actual collection through the outer fixture teardown decision. Do not rebuild it in the test from sources.Requested or seed the expected output.

Cover both direct retirement failure positions: one request per source, original error retained, independent cleanup attempted, settlement accounted for, and the resulting admission decision.

Arrange the context-read case so its throwing getter executes before the successful match. Assert that the getter actually ran and that the independently known execution was still attributed.

For all repairs, use the existing production orchestration with small controlled boundary operations. The tests must include the caller that chooses resources and publishes outcomes. Generic helper tests, duplicated cleanup sequences and manually populated result lists are not sufficient.

No new library or general-purpose resource framework. Do not expand into unrelated failure permutations.

## 4. Finish the current records before packaging

R1-F13 is not fully fixed at 80d224bd56. The new current paragraph is accurate, but the active integration text still says recovery candidate 3 awaits building/qualification, and the active C3 finding-map rows still cite only 054228Z/6abd692fa7 and the old R1-F1–F11 stop.

Correct the active text and every affected active row. Keep historical sections explicitly dated. Map each parent obligation to the current correction, exact candidate/session, evidence class and remaining findings. Preserve source, reviewed H, current records head and eventual archive A as separate identities.

Keep the corrected R3-F5 arithmetic:

- 59 min 37.0 s spent.
- The round-3 controller receipt subtotal is 8.9 s.
- The reported outer gate-batch time is 17.3 s.
- Seventeen failed validation attempts were recorded through the current stop.

Reconcile the ledger from the actual recorded durations. Future attempts add to it. A corrected records paragraph is not evidence that the runtime findings are resolved.

## 5. Check the corrected source before spending candidate 4

After implementing the complete repair and running affected focused controls, obtain a bounded independent read-only pre-build audit using the established reviewer configuration and existing tool permissions.

I explicitly authorize an initial audit and, if it requests corrections, one verification follow-up of those corrections. This is additional review work and must be logged with its commit identities. It is not another controller enrollment, a controller review round, an approval, an archive or a substitute for formal review 4.

The audit must inspect:

- The complete immediate/deferred/terminal ownership path and the live destination of later outcomes.
- The actual production callers for command selection, outside-target retirement, setup reporting and fixture retirement handoff.
- The focused tests and their evidence that the intended failing operations executed.
- The corrected active finding map and arithmetic.

Keep the audit within C3's correctness obligations. Optional refactoring, stylistic preferences and unrelated theoretical permutations are not new acceptance requirements.

Repair concrete findings before the verification follow-up. Do not spend candidate 4 while this bounded audit still identifies a blocking C3 defect or an untested real failure boundary. Preserve and report an unresolved audit result if the authorized follow-up cannot clear it.

Do not manually mark controller findings verified from the audit. Supply the audit and correction evidence to formal review 4.

Run required focused gates and production compilation on the corrected source. No unrelated full suite. Any validation commands used during the audit or corrections count against the shared allowance.

## 6. Build and qualify once the corrected path is ready

Use the already-authorized recovery candidate 4 for the reviewed correction. Record the exact source, package and DLL hashes. Keep candidate numbering separate from the older parent candidate 3 used by C2.

Run the mechanics-only effects session with the admitted fixture and exactly identified compatible harness. Keep:

- The actual enclosing runner and declared required-cell census.
- Required completion separate from physical restoration.
- The real Dust disposal-before-delivery path, late attributable delivery and cleanup.
- The relevant hook, command, abandonment and retirement boundaries.
- Constructed production-handler cue posts with real playing IDs.
- The live verdict and persisted-row interpretation from that same run, including the live-shaped comparison.
- Foreign controls, unrelated effects and transaction/owner-file/registry restoration.

The deterministic late-publication, unreadable-reader, generic-removal and actual-caller failure controls belong in focused tests. Do not create a native launch for each combination. Do not claim native transfer-failure coverage from a row whose transferredResiduals is empty.

Retain the existing visual and listening materials. Do not repeat models, broad Ghaele qualification, presentation captures or listening samples.

Any necessary required-cell selection change must be declared before the run. A refused required cell cannot be removed from the expected set after the fact.

After affected native qualification and final required gates, use the existing fourth formal C3 review for the full result. Do not spend that last formal round on an intermediate code-only submission.

## 7. Exact authority and stop conditions

I increase the shared recovery validation allowance from 75 to 90 minutes total. The existing 59 min 37.0 s remains spent, leaving 30 min 23.0 s at this amendment. This adds fifteen minutes; it is not a new ninety-minute allowance.

The product-candidate ceiling stays four total, with three already used. No recovery candidate 5 is authorized.

The formal C3 review ceiling stays four total, with three already used. No fifth formal round is authorized. The bounded pre-build audit in section 5 is separately authorized and transparently recorded.

Implementation and model-review time remain outside the validation-command clock under the existing policy. Every validation attempt, build, gate and game session still counts. Keep earlier parent expenditure separately visible.

Before each expensive command, estimate the remaining build, session and final gates together against the actual remaining allowance. The previous build/session took 8 min 0.9 s and 6 min 3 s; use current evidence for the new estimate.

A same-package rerun requires a diagnosed setup/qualification problem, a concrete correction compatible with those compiled bytes, and sufficient allowance. Repeating an unchanged attempt hoping for a pass is not authorized. Any correction requiring another package after candidate 4 is a stop.

Do not change global workflow policy, permissions, review roles, gates or no-progress thresholds. Do not create another PR or enrollment to continue past a stop. Safe in-progress restoration always completes.

## 8. Finish at the C3 handoff

If formal review 4 permits readiness, finalize normally and verify status --remote reports AWAITING_OWNER_REVIEW with handoff_current=true. Preserve final reviewed H and archive A separately.

Return the concrete stack, current finding/evidence map, final source/package/DLL/harness identities, required-cell completion and physical-restoration results, same-run verdict agreement, audit records, full validation ledger, all retained failures and restoration receipts.

Keep the integration plan conditional and specific about the destination of each operation. Do not execute it, advance #219, merge children, resume the parent's 7/7 enrollment, integrate with master, tag or release.

C1 and C2 acceptance remains unchanged. Visual approval for T2/T3/T6 and listening choices for T5/T7 remain pending on the existing materials.

Complete the authorized work to a current C3 handoff. If a defined limit or unresolved blocking defect prevents that, preserve the exact state and report the concrete remaining obligation without extending your own authority.
----- END OWNER AMENDMENT 2 -----

----- BEGIN OWNER AMENDMENT 3 (captured 2026-11-26T22:27:31Z; message sha256 a82cb5ce8ba6c84f4b216c49d4739c0d287cc47e1c53affee1ea2f4aa83cf34b) -----
SPRINT 7 RECOVERY — C3 AMENDMENT 3  
2026-11-26

I authorize continuing the existing C3 enrollment through correction, recovery candidate 4, the affected mechanics qualification, formal review 4 and, if permitted, its normal handoff. Complete the authorized sequence autonomously. Routine implementation, focused verification, records and supported workflow operations within this authority do not need another permission request.

This amendment replaces amendment 2 §5's requirement for a clean auxiliary pre-build audit and its exhausted follow-up prerequisite. It also supersedes dependent wording that prevents packaging solely because that auxiliary audit did not clear. Both completed audits remain evidence. Their findings must be addressed; an additional audit verdict is no longer a prerequisite for candidate 4.

The existing acceptance contract, total validation allowance, candidate ceiling, formal-review ceiling and frozen prerequisites otherwise remain in force. This is not acceptance of C3 or authorization to integrate, merge, release, tag or alter my human installation.

## 1. Apply this amendment to the existing enrollment

Repository: howardreith/KingmakerGunslingerAndOtherTabletopRules.  
C3 PR: #223.  
Branch: codex/s7-recovery-c3-effects-ownership.  
Base: codex/s7-recovery-c2-ghaele-late-units at eab90a5ac82f661ecb903c5456d951a57a085eb9.  
Enrollment: kmg-codex-s7-recovery-c3-effects-ownership-20261125235239-ba5d.  
Latest formal reviewed H: f0ca78cbab9b57583379a575208a15304aae7eb1.  
Reported current remote head: 38f49b22b3a69046c09750b65c24bf4d01e90f24.  
Audited correction source: 03a78d498749b499ac8c5f97617ca282311c7591.  
Reported controller state: ACTIVE/develop at 3/4. The current stop is the exhausted owner-defined audit prerequisite. No C3 handoff or archive A exists.

Save this instruction verbatim at:

C:/Dev/KingmakerGunslingerLab/owner-guidance/S7-RECOVERY-C3-AMENDMENT-3-2026-11-26.md

Read the installed workflow instructions and verify the remote identities. Apply this new amendment through the supported resume --amend-mission operation at a safe foreground boundary, with a reason identifying the replacement of the exhausted pre-build audit prerequisite. Display and read all returned instruction parts and record the new revision and digest.

Do not increase --max-rounds, manufacture a counter reset, edit controller state or manually verify findings. Keep the existing PR and enrollment. Reconcile unexpected remote changes without overwriting another actor's work.

Keep frozen:

- Parent #219: 0542b0592a668aad0c62c66d5f2dc862460818fe, BLOCKED 7/7, no archive A.
- C1 #221: H 5a56d480a2827e0ddd3918e30d32a07c7554e112; A 2c67a049ee33e0b29621fd29a8b25608ed31e956. Its accepted implementation/domain and same-run native scope remain unchanged.
- C2 #222: H da811b123c5720ad2d174db17142d8f4ef8fa5d6; A eab90a5ac82f661ecb903c5456d951a57a085eb9. Its accepted qualification remains tied to harness d77754ba…b4f1 and session 20261126T033419Z.

## 2. Use the completed audits as correction evidence

Preserve the initial audit of a84d0b0d073055da8c192ba6e3edf0880644f550 and the follow-up of 03a78d498749b499ac8c5f97617ca282311c7591, including their findings and original verdicts. Keep A1-F3, A1-F7 and A1-F8's verified scopes and the earlier formal findings distinct.

Address A1-F1, A1-F2, A1-F4, A1-F5, A1-F6, A1-F9 and A2-F1 through A2-F3. Correct actual defects and supply the missing focused evidence. If a claim is factually inapplicable or stronger than the acceptance contract, give a specific source- and test-supported disposition; do not silently omit it or call it verified yourself. A known unresolved correctness defect or missing required proof remains a reason to stop.

No third blanket pre-build audit or new auxiliary all-clear gate is required or authorized. Targeted read-only assistance on a specific implementation or evidence question is permitted as ordinary development work under existing tool permissions. Record material conclusions and preserve independence of formal review 4. Do not turn that assistance into another enrollment or prerequisite owner approval.

After the corrections, actual-caller controls, required focused gates and production compilation are complete, proceed to candidate 4 without seeking another auxiliary audit verdict. Formal review 4 remains the independent readiness decision.

## 3. Complete the finite ownership corrections

Use the existing C3 runtime-verification design. Keep gameplay, presentation, assets, balance and accepted C1/C2 implementations outside this repair. Add no library or general-purpose resource framework.

### A. Recovered attribution and truthful absence — A1-F1 and A2-F2

Keep the corrected present/absent/unknown attribution semantics. Add the specific missing timing case: both delivery notifications are absent; the initial release cannot attribute the condition; the exact already-ended process becomes readable before the zero-live census; immediate reconciliation removes the exact condition. Assert that the process/context recovery did not fabricate an observed-delivery entry and that foreign and pre-existing instances remain intact.

Fix the exact-execution presence operation consumed by the pre-delivery driver. It must not filter unreadable attribution out and return zero. Preserve unknown through that operation and reject unknown as evidence of absence. Invoke that actual operation with a throwing condition-context read and prove that the driver's zero-remaining criterion cannot pass. Keep positive exact matches and known-foreign exclusions precise.

### B. Reconciliation completion and subscription outcome — A1-F2

Report subscription retention/release from its own observed state. An unfinished condition reconciliation is not proof that a subscription remains installed.

Track whether a late sweep completed normally. If it throws before completing, preserve the original failure and keep the existing bounded terminal cleanup opportunity available. Do not add indefinite retries or automatic retries of an individual removal already recorded as failed by the ordinary reconciliation ledger. Preserve the separately authorized bounded repair of the exact intentionally injected leftover.

Keep operation success, physical settlement and qualification separate. An earlier exception remains a qualification failure even if the resource later settles. Once the required cleanup opportunities are complete and authoritative reads establish absence and subscription release, physical settlement may be reported without erasing that error. At terminal accounting, an unsettled or unreadable obligation is a named unresolved result, never an implied success.

### C. Cleanup before final retirement classification — A2-F1

Give pending Dust reconciliation its control reads and releases while the outside target exists. Execute its bounded cleanup actions, then fallback retirement for units not previously requested. Keep existing retirement readbacks executable through that fallback. Re-read settlement before finalizing remaining live or unknown targets as terminal failures.

Preserve one retirement request per unit and the live destination of every subsequent outcome. Test target A remaining present after its request, then settling during target B's fallback retirement. Assert one request for each and A's final settlement observation. Retain the never-settles control and its named terminal failure.

### D. Actual Dust control declarations and publication — A1-F4

Exercise the production operation that declares the real caster-behind and same-caster controls and connects outside-target retirement to the consuming isolation owner.

Cover the ordinary path where the same-caster control is due and missing; a throwing presence read with independent release still attempted; and the stopped path where that actual control is not yet due. Do not substitute a fictitious optional control or recreate resource selection and publication solely in the test.

Use the existing one-request retirement accounting and child-to-phase handoff. Assert the appropriate admission result, exact outside-target retirement and publication to the owner still consuming cleanup.

### E. Register and capture before command submission or interruption — A1-F5 and A2-F3

Drive the actual passive issue operation. At its single native Run boundary, the exact command must already be registered with its Dust owner and the phase. Keep the deliberately foreign same-caster command outside both owned sets.

Before a phase retry interrupts an owned command, give the original Dust owner the opportunity to retain its currently readable process. Apply this to passive and direct Dust routes. A separate phase snapshot that the Dust reconciliation cannot consume is insufficient.

Use one cohesive focused case: the production issue path runs; the first process read is unavailable and the first cell interruption fails; cleanup transfers; the process becomes readable; the phase interruption clears the mutable command field; deferred reconciliation still has the exact retained process. Preserve the original failure and foreign control. Test production orchestration with controlled engine-boundary operations rather than duplicating its order in the test.

### F. Protect foreign queued work through the whole cleanup operation — A1-F6

The follow-up audit identifies a native LeaveCombat path that can issue a return move and clear queued commands. Preserve the cited engine evidence or label that mechanism as audit-sourced; do not claim an engine behavior was executed when it was only read.

Protect against both explicit queue maintenance and queue-changing combat exit. The minimal permitted approach is to withhold an unsafe operation when that actor has foreign queued work, record that withholding, and retain the established fixture-retirement boundary. Apply the same rule to the phase's separate combat-exit calls for the fixture caster and hostile.

Do not clear and reconstruct foreign queues or replay their commands. Keep other permitted cleanup independent of an interruption failure. Test the real guard with distinct owned and foreign command objects and a combat-exit boundary that would clear the queue if invoked. Assert preserved identity/order before intentional actor retirement and continued permitted cleanup for another actor. A withheld operation must not be recorded as executed.

## 4. Correct the finding map from the accepted structured review — A1-F9

The structured JSON in the published round-3 review already carries these bindings:

https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/223#issuecomment-7194404698

It is bound to H f0ca78cbab9b57583379a575208a15304aae7eb1 and request kmg-codex-s7-recovery-c3-effects-ownership-20261125235239-ba5d-r03-db7304.

Correct the active parent map:

- R4-F1: R2-F7, R2-F8, R1-F11.
- R7-F2: R3-F1, R3-F2, R2-F7, R3-F3, R3-F4, R1-F11.
- R7-F3: no outstanding round-3 binding.
- R7-F4: R3-F3, R1-F11.
- R7-F5: R3-F1, R3-F4.

Keep mission-only bindings separate:

- R1-F9: C3 §5.
- R1-F13: C3 §8.
- R3-F5: C3 §§6–8.

Withdraw the unsupported current claim that these bindings differ between the accepted published review and the audit's cited result. Preserve the earlier interpretation as dated history. If an actually different artifact is found, establish its request, commit and acceptance provenance before using it.

Use a one-off extraction/cross-check of the structured acceptance_ids rather than another hand transcription. No permanent mapping framework or new product test suite is needed.

Update active coverage claims for the passive callback and combat exit. Distinguish a code correction, a focused test, a native observation and a formal verification. Keep source commit, package, reviewed H, records head and eventual archive A distinct.

Audit A1-F*/A2-F* labels are evidence identifiers, not controller finding IDs. Preserve their correction mapping in the manifest and report evidence. The formal disposition array must use the actual pending controller IDs returned by the workflow; do not insert unknown audit IDs or manually change historical verdicts.

## 5. Qualify the corrected bytes within the remaining allowance

Keep the required focused gates and production compilation on the completed correction. Reuse current passing receipts only where the installed workflow and unchanged source allow. No unrelated full suite or presentation reruns.

Build recovery candidate 4 once and record its exact source, package and DLL hashes. Identify the compatible harness and admitted fixture. Retain the mechanics-only effects selection and its enclosing runner, with the required-cell set declared before launch.

The native result must cover:

- Every declared required cell admitted and completed, independently of physical restoration.
- Real Dust disposal before delivery, late attributable delivery, settlement and release.
- The affected hook, command, abandonment and retirement paths within that mechanics selection.
- Constructed production-handler cue posts with real IDs, labelled as constructed setup.
- The live evaluator verdict and persisted-row verdict from that same run, plus the live-shaped comparison.
- Foreign controls, unrelated effects and complete transaction, owner-file, save, settings and registry restoration.

Use focused tests for the deterministic recovery timing, unreadable readers, late-publication failures, actual caller wiring and cleanup ordering. Do not add a native launch for each failure combination. Do not claim transfer-failure coverage from empty transferredResiduals rows.

Preserve candidate 3's 20261126T134425Z PASS on source 66b0ff3f14efdd29c80f5f9091bec3521eacfc6d and its recorded package/DLL/harness identities. Its 43 passing checks do not qualify the subsequent code changes. Keep every earlier failed attempt and original verdict.

Do not repeat model captures, broad Ghaele qualification or listening samples. The existing visual/listening materials and pending owner decisions remain unchanged.

After affected native qualification and final required gates, submit the full corrected result to the existing formal review 4. Include both audits, their dispositions, the corrected formal finding map, focused evidence and exact native identities. Do not spend that last round on an intermediate code-only submission.

## 6. Existing ceilings remain unchanged

Shared recovery validation allowance: 90 minutes total.  
Recorded spend at this amendment: 60 min 46.5 s.  
Remaining: 29 min 13.5 s.  
Recorded failed validation attempts: 18; add subsequent attempts honestly.  
Recovery candidates: three used, four total permitted. Candidate 5 is not authorized.  
Formal C3 reviews: three used, four total permitted. Review 5 is not authorized.

Keep implementation and model-review time outside the validation-command clock under the existing policy. All validation commands, failed attempts, builds, gates and native sessions count. Keep earlier parent expenditure separate.

---

<!-- agent-review:v1 enrollment=kmg-codex-s7-recovery-c3-effects-ownership-20261125235239-ba5d key=r04-request part=2/2 sha256=dad3521c8b4b1a00ca5e021d4f57d724c0a9bec5ff8284c36e2751b09d3deb72 -->
_Part 2 of 2._



Before an expensive command, estimate the remaining build, session and final gates together. The preceding build and mechanics run took 8 min 0.9 s and 6 min 3 s; use those as reference durations, not guarantees.

A same-package rerun requires a diagnosed setup/qualification problem, a concrete correction compatible with those bytes and enough remaining allowance. No unchanged luck-based retries. A fix requiring another package after candidate 4, exhausted time, another formal round or a controller stop outside this authority requires stopping with exact remaining obligations. Always finish safe restoration already in progress.

Do not alter global workflow rules, permissions, reviewer roles, gates, no-progress thresholds, counters or state files. Do not create another PR or enrollment to continue beyond a limit.

## 7. Finish at the supported C3 handoff

If formal review 4 permits readiness, finalize normally and verify status --remote reports AWAITING_OWNER_REVIEW with handoff_current=true. Preserve final reviewed H and archive A separately.

Return the current stack; the formal/audit finding map and dispositions; final source/package/DLL/harness/session identities; required-cell completion and physical-restoration results; same-run verdict agreement; focused gates; both audit records; the full ledger; retained failures; and restoration receipts.

Keep the integration plan conditional and explicit about destinations. Do not integrate the children into #219, resume its 7/7 enrollment, merge into master, tag or release. C1/C2 acceptance remains unchanged. Visual approval for T2/T3/T6 and listening choices for T5/T7 remain pending on the existing materials.

Complete this authorized sequence to a current C3 handoff if its evidence and formal review permit. If a defined limit or concrete unresolved obligation prevents that, preserve and report the exact state without extending your own authority.
----- END OWNER AMENDMENT 3 -----
```

_Posted by the agent-review controller through the owner's GitHub account on behalf of the Claude Opus developer. Automated request; not a human review._
