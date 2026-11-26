<!-- agent-review:v1 enrollment=kmg-codex-s7-recovery-c1-cue-evaluator-20261125045625-4a34 key=r05-request part=1/1 sha256=33bfa638c9295f025368c19ade9d48d4ce3fdec1ab34277ce6efad5fb81b40cf -->
### Claude Opus — review request — round 5 — candidate `5a56d480a2`

| Field | Value |
|---|---|
| Request ID | `kmg-codex-s7-recovery-c1-cue-evaluator-20261125045625-4a34-r05-b36ed4` |
| Checkpoint | `final` (final) |
| Candidate H | `5a56d480a2827e0ddd3918e30d32a07c7554e112` |
| Target B (`codex/s7-visual-audio-remediation`) | `0542b0592a668aad0c62c66d5f2dc862460818fe` |
| Merge base M | `0542b0592a668aad0c62c66d5f2dc862460818fe` |
| Mission | `s7-recovery-c1-mission` sha256 `01b1fa7d63` |
| Workflow | `6d613defe7` |
| Owner guidance | none |
| Owner amendments | revision 2, sha256 `cc70f3e973334d6267b49bcb230351b26ccf58a06d1473b393ad639ac53c7e74` |
| Developer observed | foreground session `438fdcd7-910d-4c0e-8441-dde58c90713d`: models ['claude-opus-5-5'], effort ['high'], permission modes ['auto'] (CLI-recorded transcript; the headless permission profile does not apply to a foreground session) |
| Reviewer requested | Codex `gpt-6.2-astra`, effort `max`, read-only sandbox |
| Round budget | 5 of 5 |

**Scope:** The complete mission as written.

**Exit conditions:**
- Every requirement of the mission is implemented, with evidence appropriate to each claim; no requirement is weakened or silently dropped.
- Validation follows the repository's own instructions; commands, results and limits are reported honestly.
- Deviations, limitations and deferred optional items are stated explicitly, separate from required work.

**Developer summary:**

Sprint 7 recovery child C1, closure review 5 of 5 under OWNER AMENDMENT 2 (C1 amendment 2, revision 2, bounded closure), on H=5a56d480a2, PR #221 targeting the frozen #219 branch (0542b0592a). Revised acceptance boundary (amendment 2 section 2): the evaluator's acceptance/rejection for the current collector, live versus persisted equivalence, the named identity/missing-evidence/lifecycle controls, the round-4 outcome corrections and the retained rows at their supported scopes; forensic validation of arbitrary edited traces, a general old-collector chronology model and exact refusal wording are withdrawn as prerequisites; a refusal establishes only that the evidence does not support the claim. All seven round-4 findings are fixed: the five outcome findings (R4-F1 to R4-F4, R4-F7) and, being local, the R4-F5 and R4-F6 distinctions (no rebuttal needed). The contract comment now grounds stop/count agreement in consecutive reads without a yield, not in a shared frame, and states which deadline bound each generation establishes. Evidence type: the round-4 counterexamples were source-traced by the reviewer; they are now executed regressions on H with the outcomes listed. Retained rows: 202648Z characterized (CallbackLossRound6, Round6Second); 200320Z and 223809Z not-both-playing; 224949Z characterized by CallbackLossRound7 in both representations; original native verdicts and evaluator identities unchanged. Records: the manifest states the amended boundary and corrects two earlier summaries (round 3 left R2-F3 open; six, not five, failed attempts at amendment 2). ACCEPTANCE BOUNDARY: implementation and domain behavior only; the changed compiled evaluator has NOT been executed in game; C3 owns that combined native qualification. Recovery validation 4 min 24.6 s of 45; product candidates 0 of 2.

**Validation (developer-reported):**

| Command | Required | Result | Duration | Scope and limits |
|---|---|---|---|---|
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-recovery-c1-cue-evaluator-20261125045625-4a34 --gate focused -- -Filter sprint7-visual | yes | pass | 4.1s | 22 tests on H; receipt duration 2.1s. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-recovery-c1-cue-evaluator-20261125045625-4a34 --gate focused -- -Filter summon-playtest-r6 | yes | pass | 3.4s | 11 tests on H; receipt duration 1.7s. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-recovery-c1-cue-evaluator-20261125045625-4a34 --gate focused -- -Filter sprint22- | yes | pass | 3.9s | 11 tests on H; receipt duration 2.1s. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-recovery-c1-cue-evaluator-20261125045625-4a34 --gate focused -- -Filter native-profile | yes | pass | 3.3s | 9 tests on H; receipt duration 1.7s. |
| Invoke-KmgGate.ps1 -Filter sprint7-visual (local, round-5 source, two runs) | no | pass | 18.4s + 3.4s | The first FAILED on one existing diagnostic-only assertion (end recorded on all three other-cue snapshots now refused by the earlier R4-F1 check); its input is kept and it now asserts refusal, as amendment 2 section 5 directs. The second passed 22 tests. Both counted. |
| Shared recovery ledger | no | pass | 4m24.6s | Cumulative recovery validation after these gates: 4 min 24.6 s of 45 min (round-5 local 21.8s; round-5 controller 15.5s wall, receipts 7.6s); failed attempts recorded in the ledger: seven (two in round 2, four in round 4, one in round 5). The committed manifest ledger stops at 4 min 9.1 s, before these gates. Product candidates 0 of 2. |
| Native execution of the changed evaluator | no | not_run | 0s | Deferred to C3's combined native qualification. No product package, harness build or game session in C1. |

**Evidence references:**
- prepared under owner amendments revision 2 sha256 cc70f3e973334d6267b49bcb230351b26ccf58a06d1473b393ad639ac53c7e74
- docs/missions/sprint7/recovery/S7-RECOVERY-C1-MISSION.md
- docs/missions/sprint7/recovery/S7-RECOVERY-MANIFEST.md
- src/KingmakerGunslinger/RuntimeTesting/Sprint7ObserverPolicy.cs
- src/KingmakerGunslinger/Summoning/SummonCastCueLifecycle.cs
- src/KingmakerGunslinger/Summoning/SummonAbilityCastCue.cs

**Prior findings to assess:** `R4-F1`, `R4-F2`, `R4-F3`, `R4-F4`, `R4-F5`, `R4-F6`, `R4-F7`

**Owner amendments to the mission — revision 2, captured-source sha256 `cc70f3e973334d6267b49bcb230351b26ccf58a06d1473b393ad639ac53c7e74`.** Applied by the owner with `resume --amend-mission`; each owner message is preserved verbatim, in order. The effective task is the original mission plus these amendments. The reported work comes from a developer invocation whose input included them, and the reviewer of this round receives a byte-exact copy of the captured document. Public rendering after the controller's standard sanitization; the digest identifies the private captured bytes, which this rendering can differ from.

```text
# Owner amendments to the mission

Cumulative record kept by the agent-review controller. Each owner message below was applied by the owner through `resume --amend-mission` and is preserved verbatim, in order, between its labeled marker lines. Later amendments supersede conflicting earlier requirements; requirements they do not change still apply.

----- BEGIN OWNER AMENDMENT 1 (captured 2026-11-25T15:20:02Z; message sha256 e87355d5f980b0aa819018a76a6788d7c5340971414a952a709f743e843a96f6) -----
SPRINT 7 RECOVERY — C1 AMENDMENT 1  
Owner instruction, 2026-11-25

Continue the existing three-PR Sprint 7 recovery. I authorize two additional reviews for C1 on its existing enrollment, increasing its total cap from two to four. Complete the bounded evaluator repair below, then proceed autonomously through the already-authorized C2 and C3 once their prerequisite handoffs exist.

This amendment changes only C1's review allowance and clarifies its existing observation contract. It does not increase the shared validation or candidate allowances, authorize a replacement enrollment, or authorize merging, releasing, tagging or resuming the parent.

## 1. Identities and unchanged boundaries

C1:

- PR #221, codex/s7-recovery-c1-cue-evaluator.
- Target: codex/s7-visual-audio-remediation.
- Enrollment: kmg-codex-s7-recovery-c1-cue-evaluator-20261125045625-4a34.
- Last reviewed H: e4c9ed9c5e1681ef7ba29419ba4e57dc495cc010.
- Reported branch head: bb670b13f48dc533a3f6d4bdc782e0672810beba; its difference from H is the records-only stop.
- Round-2 review: https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/221#issuecomment-7159882725.
- No archive A or scoped handoff exists yet.

Parent:

- PR #219 remains frozen at 0542b0592a668aad0c62c66d5f2dc862460818fe.
- Its enrollment remains BLOCKED at 7/7.
- Preserve its findings, amendments, evidence, dispositions and history.

All R2-F1 through R2-F6 references below mean C1's round-2 findings on #221, not findings with the same IDs on the parent.

C1 remains implementation/domain-only. Production compilation through focused gates is allowed. No product package, harness build or game launch belongs to C1. Native execution of the changed evaluator remains C3's combined qualification.

Keep the recovery manifest's finding allocation:

- C1: parent R7-F1, R7-F6 and R6-F5 records.
- C2: parent R5-F1.
- C3: parent R4-F1 and R7-F2 through R7-F5.

The shared recovery allowance stays 45 minutes of new validation-command wall time and at most two new product candidates across C1-C3. At this handoff, validation spent is 1m43.4s, remaining is 43m16.6s, and candidates used are 0 of 2. Continue the existing ledger, including failed attempts and the existing command/batch accounting. Do not reset it or add the old unused parent allowance. The earlier 65m42s remains separate. Validation-command time is not total engineering or reviewer elapsed time.

## 2. Apply this amendment through the supported controller

Save this complete instruction verbatim as a new durable owner file outside the worktree:  
C:/Dev/KingmakerGunslingerLab/owner-guidance/S7-RECOVERY-C1-AMENDMENT-1-2026-11-25.md

Do not overwrite the original mission. Verify current remote identities and the enrollment's safe stopped state before changing anything. Preserve concurrent work if the reported identities have moved.

Use the installed controller's current instructions and supported amendment flow. For the currently verified interface, the sequence is:

python C:/Dev/agent-review/agent_review.py status --enrollment kmg-codex-s7-recovery-c1-cue-evaluator-20261125045625-4a34 --remote

python C:/Dev/agent-review/agent_review.py resume --enrollment kmg-codex-s7-recovery-c1-cue-evaluator-20261125045625-4a34 --reason "Owner-authorized bounded C1 continuation" --amend-mission C:/Dev/KingmakerGunslingerLab/owner-guidance/S7-RECOVERY-C1-AMENDMENT-1-2026-11-25.md

python C:/Dev/agent-review/agent_review.py resume --enrollment kmg-codex-s7-recovery-c1-cue-evaluator-20261125045625-4a34 --reason "Owner authorizes four total C1 reviews" --max-rounds 4

python C:/Dev/agent-review/agent_review.py status --enrollment kmg-codex-s7-recovery-c1-cue-evaluator-20261125045625-4a34 --instructions

Do not combine --amend-mission and --max-rounds. At the exhausted 2/2 cap, successful amendment application is expected to leave the enrollment BLOCKED pending the separate extension. This instruction expressly authorizes that immediately following extension; that expected intermediate state does not require another owner decision.

Read every returned instruction part and follow the controller's amendment-citation requirements. Do not edit controller state, reset findings or counters, suppress review, or create another enrollment to obtain a fresh allowance.

## 3. Repair one coherent observation contract

Address all six findings in one cohesive pass before spending another review. A small refactoring in the project's existing style is allowed if it removes duplicated or inconsistent interpretation. Add no library or general-purpose schema framework.

Before editing, write a compact contract beside the affected evaluator or in the developer report: what identifies one posted cue, what a snapshot actually observes, which lifecycle fields can advance, and what missing or malformed observations mean. Use that contract to review both the evaluator and its actual collector.

**A. Bind selection to a posting, not merely to a numerical ID — R2-F1**

For collector generations that emitted per-source inventories, resolve each selected positive playing ID to its actual source record. Match the recorded immutable posting identity: playing ID, event and posting frame/time. Preserve distinct survivor/other ownership and reject an ambiguous or contradictory association.

Carry that same posting identity through the relevant later snapshots. An earlier ended cue on the correct source must not be substituted for the selected cue while retaining the selected cue's observations.

Do not require deep equality of whole snapshots. End and stop observations are mutable and can legitimately appear later. Match identity and reconcile lifecycle transitions at their observed times.

**B. Interpret counts at the times they were observed — R2-F2**

Periodic samples and the terminal callback list are taken at different times. Require readable, nonnegative, nondecreasing cumulative counts consistent with the later terminal total. Permit a callback after the final periodic sample to appear only in the terminal list.

Reject a callback dated before its own selected cue was posted. Reconcile clearly contradictory callback/sample timing using the actual collector's recording order.

Do not impose atomic-snapshot semantics the producer does not provide: its sample timestamp is read before its callback count, and its terminal array is collected after the loop exits. Do not require exact count equality at a sample timestamp or invent a terminal-read timestamp. The nominal window deadline alone is not the terminal array's capture time.

Handle uncertain boundaries conservatively and identify unavailable evidence. Do not add speculative clock-drift or arbitrary scheduling requirements.

**C. Reconcile earlier and later lifecycle observations — R2-F3**

Read the earlier, retirement-frame, settled and disposal observations for the same posted cue. A completion or stop already observed cannot disappear or change incompatibly in a later snapshot. Cumulative stop attempts cannot decrease.

An earlier recorded completion of the other cue must not be ignored when crediting its later retirement. A stop timestamp read after the survivor's disposal must not be credited to that disposal if it places the stop inside the interval already reported to have zero survivor stop attempts.

Permit genuine later lifecycle progression, including the survivor's own post-window disposal stop. Preserve the distinction between a stop request, a completion callback and native playback becoming inactive.

**D. Handle the producer's legitimate failed-post record — R2-F4**

An unselected post whose native result is playing ID 0 is a known non-owning failed attempt. It must not invalidate otherwise readable source evidence for two positively identified selected cues.

It cannot satisfy selection or ownership. Do not turn this narrow rule into permission to discard arbitrary malformed source records or assume an unreadable inventory proves ownership.

**E. Preserve unknown versus observed — R2-F5**

Counts must be nonnegative and in the supported range. Negative, malformed or oversized values are unreadable; they are not evidence that a stop occurred. Zero and a positive observed count retain their distinct meanings.

Apply the same interpretation consistently to the affected count fields and both supported row representations.

**F. Identify the evaluator actually used — R2-F6**

New rows must identify the current evaluator entry point they invoke. Update the stale CallbackLossSettled metadata accordingly, using a language-bound identifier or the project's established equivalent where practical.

Preserve original evaluator names and verdicts in historical rows. A current reinterpretation must identify the current evaluator separately.

## 4. Focused behavioral evidence

Use the actual retained 224949Z row and its genuinely collector-shaped in-memory counterpart. Keep the direct String-typed null JValues in the live counterpart; do not serialize it first and thereby remove the original distinction under test.

The reviewer source-traced the six new counterexamples but did not execute them. Add and run the targeted behavioral regressions, then report their actual outcomes. Do not describe those earlier inspections as executed reproductions.

Cover this finite set in both representations where applicable:

- The original positive row still characterizes.
- Survivor 586 cannot be replaced with the already-ended same-source cue 513; likewise other 622 cannot be replaced with earlier cue 539 while keeping the later posting's observations.
- A legitimate callback between the last periodic sample and the terminal read is accepted.
- A callback predating its selected cue, and timing/count contradictions established by the producer's observation order, are refused.
- Earlier completion cannot be erased or contradicted by a favorable settled record; retirement-frame stop counts cannot exceed their later cumulative count.
- A survivor stop timestamp inside the protected zero-stop interval is refused; the actual later disposal stop remains valid.
- An unselected known ID-0 failed post does not spoil valid selected evidence, while selection of ID 0 is refused.
- Zero, positive, negative and oversized affected counts retain the correct no-stop, observed-stop or unreadable interpretation.
- A newly produced row identifies the entry point it actually invokes.

Retain existing meaningful regression coverage, live/persisted equivalence, terminal playback checks and generation-specific interpretation of the older Round6 rows. Require only observations a generation actually emitted. Do not invent old provenance.

Run the applicable focused gates required by the controller. Keep all executed failures and exact source identities. No full-suite or native expansion is authorized for C1.

Review the correction against the finite attribution, lifecycle, observation and unknown-value contract. Meaningful defects remain in scope when they produce a concrete incorrect verdict or unsupported attribution under the actual collector, a declared historical generation or the identity/missing-evidence controls above. Unused-field hardening and speculative telemetry frameworks are outside this repair.

## 5. Handoffs and autonomous continuation

Once the controller returns ready_for_owner_review for C1, finalize it through the normal supported process. Confirm status --remote reaches AWAITING_OWNER_REVIEW with handoff_current=true, retaining reviewed H and archive A separately.

C1's handoff must say: implementation/domain scope only; changed-evaluator native execution deferred to C3. Do not relabel the historical 224949Z native FAIL as PASS because a corrected offline reader characterizes its stored observations.

Then proceed without another owner pause under the original recovery authority:

- Freeze C1's prerequisite branch and branch/target C2 from its stable archive A.
- Complete C2's Ghaele late-unit repair and authorized short lifecycle session using the existing candidate-3 package with the corrected external harness.
- After C2's scoped handoff, freeze it and branch/target C3 from its archive A.
- Complete C3's effects-ownership repairs and combined candidate qualification, including C1's changed evaluator in game and live-versus-saved-row verdict agreement from the same run.
- Record the later fulfillment of C1's deferred native check in C3 and the integration manifest, without modifying the frozen C1 handoff.

C2 and C3 retain their existing two-review limits. The shared 45-minute validation and two-candidate limits remain aggregate. Their existing transaction, restoration and game-window requirements remain in force.

Return the three-PR reviewable stack and integration handoff before any parent review resumes or anything merges. Keep #219 frozen. Visual acceptance of T2/T3/T6 and listening choices for T5/T7 remain pending and separate from this evaluator repair.

## 6. Stops and reporting

The authorization covers two additional C1 reviews only. Any later mandatory controller stop, exhausted allowance, unsafe restoration state or unresolved identity conflict must be preserved and reported. Do not create substitute enrollments or obtain more rounds by restarting a PR.

Report the actual finding dispositions, reviewed heads, archive commits if any, source/package/harness identities where applicable, focused results, cumulative validation spend, native deferrals and restoration status. Distinguish source inference, executed domain evidence and native evidence.

This amendment grants no merge, release, tag, parent-resumption or human-installation authority.
----- END OWNER AMENDMENT 1 -----

----- BEGIN OWNER AMENDMENT 2 (captured 2026-11-26T00:10:32Z; message sha256 1a99894da067f782deefbabae3ba6b805a8f80f109f722897b5178fb8261f9c0) -----
SPRINT 7 RECOVERY — C1 AMENDMENT 2: BOUNDED CLOSURE  
Owner instruction, 2026-11-25

I authorize one final C1 correction pass and one additional formal review on the existing enrollment, increasing its total cap from four to five. This amendment changes C1's acceptance boundary as specified below. It does not merely grant more time under the previous broad interpretation.

## 1. Preserve the existing recovery

C1 is PR #221 on codex/s7-recovery-c1-cue-evaluator, targeting codex/s7-visual-audio-remediation.

Enrollment: kmg-codex-s7-recovery-c1-cue-evaluator-20261125045625-4a34.  
Reviewed H: 2f975db0754d94cf12ddc289c76363a67dde3148.  
Reported branch head: a9070b5ba594708ac1574d1ce4cfc32136f1590e, with the records-only stop.  
Review: https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/221#issuecomment-7167869845.

Verify those identities before proceeding. Preserve concurrent work if they have moved.

Keep parent #219 frozen at 0542b0592a668aad0c62c66d5f2dc862460818fe and its enrollment BLOCKED at 7/7. Keep every original finding, verdict, amendment, session and package identity.

C1 remains implementation/domain-only. No C1 product package, harness build or game launch. C3 still owns native execution of the changed evaluator.

The original recovery allowance remains 45 minutes of validation-command wall time across C1-C3. At this handoff, 3m47.3s is spent and 41m12.7s remains. Product candidates remain 0 of a maximum of 2. Do not reset the ledger, add the old unused parent allowance, or change required gates. The earlier 65m42s remains separate.

## 2. Revised acceptance boundary

C1 qualifies the evaluator's acceptance/rejection behavior for the current collector, live-object versus persisted-row equivalence, the existing named identity/missing-evidence/lifecycle controls, the outcome corrections below, and the retained historical rows at their supported scopes.

I withdraw any interpretation of the original “strict evaluator” requirement or amendment 1 that makes these additional requirements prerequisites for C1:

- Complete forensic validation of every arbitrarily edited historical trace.
- A general chronology model for hypothetical executions of old collectors beyond the already named controls.
- Exact diagnostic wording or subclassification for inputs that already cannot characterize.

This does not permit missing required evidence to produce a positive characterization. Preserve the explicit-null handling necessary for valid live/persisted equivalence. Preserve the existing meaningful positive and negative cases.

A refused characterization means the evidence does not establish the claim. Its diagnostic label alone is not proof that a gameplay stop, audio defect or other product failure occurred.

New findings remain blocking if they demonstrate an incorrect acceptance/rejection for a qualifying real row, a concrete supported current-collector path, a violation of the named acceptance controls, or a regression introduced by this correction. Source-traced producer paths are sufficient to identify such defects; another native reproduction is not required in C1.

Additional invented edited-row combinations outside this boundary may be reported as limitations. They must not silently expand the effective task into a universal trace validator. This is an explicit owner scope decision, not an instruction to suppress relevant defects or force a passing review.

## 3. Close the five outcome-affecting findings together

The R4 IDs below refer to C1's round-4 review on #221.

- **R4-F1:** reject the named transition where completion is already observed with zero attempts but the settled record later claims a first retirement stop. Also reject a retirement-frame stop/end timestamp strictly later than the subsequent first sample. Preserve a legitimate completion following an already-attempted stop, and permit equal timestamps where known operation order supports them.
- **R4-F2:** reject the specified Round7 sequence requiring another periodic iteration after a preceding sample is already beyond the recorded loop deadline. Preserve a final sample that straddles its own guard and a callback first appearing in the later terminal list.
- **R4-F3:** use the exact retirement-plus-duration bound only where that retirement timestamp was recorded. Do not treat a Round6 phase mark as its unrecorded loop start. Use bounds its actual observations establish and preserve the named valid between-deadlines control.
- **R4-F4:** equal UTC readings do not establish reversed operation order. Use the collector's known read order; reject strict inversions, preserve the valid shared-clock-tick control, and do not invent precision absent from the record.
- **R4-F7:** when attributing a successful request to retirement, require the recorded production reason entity-retired or view-destroyed. Refuse empty or explicitly unrelated reasons. Preserve failed-call handling. A request followed by completion records those observations; it does not prove that the request caused completion.

Make these changes coherently in the existing implementation. Add no library, generic schema framework, new timestamp instrumentation or historical collector generation.

Retain conservative sampling semantics. Being read within one frame does not by itself make all fields an atomic snapshot.

## 4. Disposition of R4-F5 and R4-F6

I withdraw precise refusal diagnosis as a C1 handoff requirement for these two cases:

- **R4-F5:** an impossible pre-disposal count already refuses characterization; distinguishing it from an observed stop is diagnostic precision.
- **R4-F6:** missing, malformed and explicitly null settled completion already refuse characterization; distinguishing those refusal subtypes is diagnostic precision.

The required remaining behavior is refusal in both representations. Missing or malformed required evidence must never become evidence for a positive result. Do not infer a proven product fault from the less-specific refusal label.

Fix these distinctions in the same pass if straightforward and local. If fixed, report and verify the actual fix normally.

If either distinction remains unfixed, use the controller's supported amended-requirement disposition:

- Developer: rebutted, citing OWNER AMENDMENT 2, this section, the withdrawn requirement and the preserved refusal behavior.
- Reviewer: rebuttal_accepted only after confirming the amendment covers the finding and the remaining supported behavior is sound; its rationale must name the amendment and withdrawn requirement.

Do not use deferred_optional for a prior blocking defect. Do not call an unfixed finding fixed or a valid historical finding a false positive. Preserve its original assessment.

## 5. Focused evidence and closure review

Use the retained 224949Z row and its actual collector-shaped in-memory counterpart, including direct String-typed null JValues. Do not serialize the live counterpart before testing it.

Execute the current review's concrete R4-F1/F2/F3/F4/F7 controls and the valid neighboring cases. For R4-F5/F6, demonstrate that the relevant inputs cannot characterize; exact diagnostic strings are no longer readiness criteria.

Preserve the four retained-row outcomes:

- 202648Z: characterized by the current historical reader.
- 200320Z and 223809Z: not-both-playing.
- 224949Z: characterized by the corrected reader in both representations.

Keep their original native verdicts and original evaluator identities unchanged. A current offline reinterpretation is separate evidence.

Retain existing cases when adjusting diagnostic-only assertions: keep their inputs and require the correct acceptance/rejection behavior. Do not delete meaningful controls to obtain a passing gate.

Run the applicable controller-required focused gates with production compilation. No broad suite or native expansion. The new round-4 counterexamples were source-traced, not executed by that reviewer; report the new regression results accurately.

Submit one complete correction and disposition report for the fifth review. The reviewer must evaluate the effective amended task, all five outcome repairs and any amendment-cited rebuttals. A genuine remaining in-scope defect still blocks handoff.

Correct two summary errors during the normal records update: round 3 left R2-F3 open, and the current ledger lists six failed validation attempts rather than five. These corrections require no recreated evidence or separate validation campaign.

## 6. Apply the amendment and review extension correctly

Save this entire instruction verbatim outside the worktree at:  
C:/Dev/KingmakerGunslingerLab/owner-guidance/S7-RECOVERY-C1-AMENDMENT-2-2026-11-25.md

Use the existing controller's supported sequence:

python C:/Dev/agent-review/agent_review.py status --enrollment kmg-codex-s7-recovery-c1-cue-evaluator-20261125045625-4a34 --remote

python C:/Dev/agent-review/agent_review.py resume --enrollment kmg-codex-s7-recovery-c1-cue-evaluator-20261125045625-4a34 --reason "Owner narrows C1 acceptance for bounded closure" --amend-mission C:/Dev/KingmakerGunslingerLab/owner-guidance/S7-RECOVERY-C1-AMENDMENT-2-2026-11-25.md

python C:/Dev/agent-review/agent_review.py resume --enrollment kmg-codex-s7-recovery-c1-cue-evaluator-20261125045625-4a34 --reason "Owner authorizes one C1 closure review, five total" --max-rounds 5

python C:/Dev/agent-review/agent_review.py status --enrollment kmg-codex-s7-recovery-c1-cue-evaluator-20261125045625-4a34 --instructions

Read all returned instruction parts and use the controller's actual amendment label and digest in the report.

Do not combine the amendment and limit flags. The expected intermediate cap-blocked state after applying the amendment expressly permits the immediately following authorized extension, without another owner question.

Do not reset history, edit controller state, replace the enrollment, or obtain more reviews through another PR.

## 7. Continue the already-authorized stack after a legitimate handoff

Once C1 receives ready_for_owner_review, finalize through the supported process and verify status --remote reports AWAITING_OWNER_REVIEW with handoff_current=true. Preserve reviewed H and archive A separately.

C1's handoff must state its revised acceptance boundary, any accepted diagnostic limitations and the deferred C3 native qualification.

Then proceed autonomously under the original recovery authority:

- Freeze C1 and branch/target C2 from its stable archive A.
- Complete C2's already-scoped Ghaele late-unit repair and short external-harness qualification.
- After C2's scoped handoff, freeze it and branch/target C3 from its archive A.
- Complete C3's already-scoped effects-ownership repairs and combined native qualification, including live/saved verdict agreement from the same run.
- Record fulfillment of C1's deferred native check in C3 and the integration manifest without modifying the frozen C1 handoff.

C2/C3 retain their existing review limits and share the same remaining validation and candidate allowances. Their transaction, restoration and game-window requirements remain unchanged. Do not move unfinished C1 outcome repairs into C3.

If the fifth C1 review identifies an in-scope blocker or another mandatory stop occurs, preserve and report it. No further automatic extension is authorized.

Return the reviewable recovery stack before any parent review resumes or anything merges. Visual and listening acceptance remain separate owner decisions. This amendment grants no merge, release, tag, parent-resumption or human-installation authority.
----- END OWNER AMENDMENT 2 -----
```

_Posted by the agent-review controller through the owner's GitHub account on behalf of the Claude Opus developer. Automated request; not a human review._
