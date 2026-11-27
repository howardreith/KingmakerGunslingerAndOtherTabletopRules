<!-- agent-review:v1 enrollment=kmg-codex-s7-recovery-c3-effects-ownership-20261125235239-ba5d key=r02-request part=1/1 sha256=6932d290a2b5fc9184112e6471b3b2ea34926866239b5e1c55ad4fadb86e32ef -->
### Claude Opus — review request — round 2 — candidate `89d4efa347`

| Field | Value |
|---|---|
| Request ID | `kmg-codex-s7-recovery-c3-effects-ownership-20261125235239-ba5d-r02-8d4658` |
| Checkpoint | `final` (final) |
| Candidate H | `89d4efa34732aa85c0a3975c5770865e245e6766` |
| Target B (`codex/s7-recovery-c2-ghaele-late-units`) | `eab90a5ac82f661ecb903c5456d951a57a085eb9` |
| Merge base M | `eab90a5ac82f661ecb903c5456d951a57a085eb9` |
| Mission | `s7-recovery-c3-mission` sha256 `b95649a59a` |
| Workflow | `6d613defe7` |
| Owner guidance | none |
| Owner amendments | revision 1, sha256 `c91a52616ba921b1919042152d8d0a6a09c78c2c7857621e5b3117a624846233` |
| Developer observed | foreground session `438fdcd7-910d-4c0e-8441-dde58c90713d`: models ['claude-opus-5-5'], effort ['high'], permission modes ['auto'] (CLI-recorded transcript; the headless permission profile does not apply to a foreground session) |
| Reviewer requested | Codex `gpt-6.2-astra`, effort `max`, read-only sandbox |
| Round budget | 2 of 4 |

**Scope:** The complete mission as written.

**Exit conditions:**
- Every requirement of the mission is implemented, with evidence appropriate to each claim; no requirement is weakened or silently dropped.
- Validation follows the repository's own instructions; commands, results and limits are reported honestly.
- Deviations, limitations and deferred optional items are stated explicitly, separate from required work.

**Developer summary:**

Sprint 7 recovery child C3, review 2 of 4 under OWNER C3 AMENDMENT 1 (revision 1), on H=89d4efa347, PR #223 targeting C2's branch at its archive A eab90a5ac8. CODE, DOMAIN AND RECORDS ONLY: THE NEWLY CHANGED COMPILED CODE HAS NOT RUN IN THE GAME; C3 still needs its native qualification on recovery candidate 3 (amendment section 5) before it can be ready. One coherent correction of C3 review findings R1-F1 to R1-F13 on the effects phase's acquisition-to-admission contract: a phase-wide retirement record across observer, phase and fixture teardown; held condition ledger and settlement reconciliation of late Dust effects; cumulative immediate command-release failures and unreadable processes as named unknowns; count-independent hook rollback and repair; exact expected failures with unreported owners refused; exact repair results; required-cell completion separate from physical restoration; set-up scope; consistent cell runner outcomes; focused tests at the production failure boundaries; a mechanics-only selection; a completed integration plan. A new native control (s7-dust-pre-delivery-disposed) is added for the ordinary disposal-before-delivery path. The owner's acceptance of C1's same-run criterion (20261126T054228Z on 6abd692fa7) and of C2's scope are recorded in C3 only. Recovery validation 44 min 23.4 s of 75; candidates 2 of 4.

**Validation (developer-reported):**

| Command | Required | Result | Duration | Scope and limits |
|---|---|---|---|---|
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-recovery-c3-effects-ownership-20261125235239-ba5d --gate focused -- -Filter sprint7-visual | yes | pass | 5.2s | 25 tests on H, production compilation; receipt 3.5s; worktree unchanged. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-recovery-c3-effects-ownership-20261125235239-ba5d --gate focused -- -Filter summon-playtest-r6 | yes | pass | 3.4s | 11 tests on H, production compilation; receipt 1.7s; worktree unchanged. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-recovery-c3-effects-ownership-20261125235239-ba5d --gate focused -- -Filter native-profile | yes | pass | 3.8s | 9 tests on H, production compilation; receipt 2.1s; worktree unchanged. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-recovery-c3-effects-ownership-20261125235239-ba5d --gate focused -- -Filter loader-compat-probe | yes | pass | 3.8s | 5 tests on H, production compilation; receipt 2.0s; worktree unchanged. |
| C3 native qualification of the corrected bytes (recovery candidate 3, s7-c3-mechanics session) | no | not_run | 0s | NOT RUN, AND STILL REQUIRED FOR C3 READINESS: marked not required only for this pre-native review, which C3 amendment 1 section 4 expressly authorizes (the controller refuses a submission with a required validation not passing); it is not an optional deferral and the next formal report must include it. The newly changed compiled code has not run in the game. As C3 amendment 1 section 4 directs, this review covers the corrected source, focused domain behavior and records before the next package; C3 is not owner-ready until recovery candidate 3 is built and the mechanics-only effects session (required cells declared in the session) qualifies it. Earlier receipts keep their own identities and scope: 20261126T054228Z ran product source 6abd692fa7 (package de63d6f4...4b46), which the owner accepted for C1's same-run evaluator criterion only; 20261126T051633Z is a recorded FAIL. |
| Invoke-KmgGate.ps1 -Filter sprint7-visual (local; six runs) | no | pass | 16.9s + 19.3s + 34.2s + 5.4s + 17.1s + 4.9s | Three FAILED on compile errors (old focused test against the new driver API; unassigned test fields; an assertion inserted between if and else); one PASS waited out a 30 s settle window (then shortened); final PASS 25 tests. All counted in the ledger. |
| Shared recovery ledger | no | pass | 44m23.4s | Recovery validation after these gates: 44 min 23.4 s of the amended 75 minutes, remaining 30 min 36.6 s (the committed ledger stops at 44 min 6.7 s, before these controller gates: 16.7s batch wall, receipts 9.3s). Product candidates 2 of 4 used; recovery candidate 3 is next, after this review. Failed validation attempts in the recovery: thirteen. |

**Evidence references:**
- prepared under owner amendments revision 1 sha256 c91a52616ba921b1919042152d8d0a6a09c78c2c7857621e5b3117a624846233
- docs/missions/sprint7/recovery/S7-RECOVERY-C3-MISSION.md
- docs/missions/sprint7/recovery/S7-RECOVERY-MANIFEST.md
- src/KingmakerGunslinger/RuntimeTesting/Sprint7ObserverPolicy.cs
- src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.Sprint7Presentation.cs
- src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningCorrection.cs
- src/KingmakerGunslinger/RuntimeTesting/ExpandedSummoningNativeProfilePolicy.cs
- tests/KingmakerGunslinger.DomainTests/Sprint7VisualRepairTests.cs
- scripts/compatibility/loader-compat/sessions/s7-c3-mechanics.json

**Prior findings to assess:** `R1-F1`, `R1-F10`, `R1-F11`, `R1-F12`, `R1-F13`, `R1-F2`, `R1-F3`, `R1-F4`, `R1-F5`, `R1-F6`, `R1-F7`, `R1-F8`, `R1-F9`

**Owner amendments to the mission — revision 1, captured-source sha256 `c91a52616ba921b1919042152d8d0a6a09c78c2c7857621e5b3117a624846233`.** Applied by the owner with `resume --amend-mission`; each owner message is preserved verbatim, in order. The effective task is the original mission plus these amendments. The reported work comes from a developer invocation whose input included them, and the reviewer of this round receives a byte-exact copy of the captured document. Public rendering after the controller's standard sanitization; the digest identifies the private captured bytes, which this rendering can differ from.

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
```

_Posted by the agent-review controller through the owner's GitHub account on behalf of the Claude Opus developer. Automated request; not a human review._
