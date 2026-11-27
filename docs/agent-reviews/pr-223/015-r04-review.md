<!-- agent-review:v1 enrollment=kmg-codex-s7-recovery-c3-effects-ownership-20261125235239-ba5d key=r04-review part=1/1 sha256=457519bc7793ba4ca2e86842379957e9c0806ba70dad65ed522615f2b8c96d9e -->
### Codex GPT-6.2 Astra — review — round 4 — candidate `43dd44a2ba`

**Decision:** `changes_required` — An unfinished Dust command can outlive its cleanup owner when its process is still null and the first interruption fails. The four Focused gates and the 43-check native session passed, but neither exercised that branch. The required passive-caller test and parts of the handoff record also remain incomplete.

Continuation for Claude

Record the authorized-limit stop. Recovery candidate 4 and formal review 4 exhaust both ceilings under OWNER AMENDMENT 3 §6. C3 has no archive or current handoff. A product correction would need to retain the unfinished command through confirmed cancellation or execution settlement, then prove late-effect cleanup through the actual passive submission. The records can be reconciled against the existing receipts and any contemporaneous budget estimate. Preserve the native PASS, both audit verdicts, earlier failures and the frozen #219/C1/C2 stack. Another package or formal review requires new owner authority.

#### Findings

**R4-F1** · P1 · defect · blocks readiness — A failed interruption can discharge Dust cleanup before the command starts
- Location: `src/KingmakerGunslinger/RuntimeTesting/Sprint7ObserverPolicy.cs:1476`; acceptance: R7-F2, C3 §5, OWNER AMENDMENT 1 §3B, OWNER AMENDMENT 3 §3E
- Trigger: A submitted Dust command is unfinished, ExecutionProcess still reads null, and its first cell interruption throws before stopping it.
- Expected: While an unsuccessfully interrupted command can still start, its original owner retains executable condition cleanup until cancellation is established or its execution and delivered effects settle.
- Actual: Capture treats a successful null process read as an empty result. ReleaseEach publishes the interruption error, but the settlement predicate checks only retained processes and commands whose process getter threw. With both lists empty, it can sweep, unsubscribe, run control teardown and discharge the reconciliation. The phase can later capture a newly started process before interrupting it, with no condition cleanup left to consume that capture. The later unfinished-command check records failure but does not restore the cleanup action.
- Why it blocks / does not: A late attributable Sickened can remain after the last owned sweep. Recording the qualification failure does not release that condition.
- Requested correction: Include still-unfinished commands with no process yet in the settlement decision after an unsuccessful release. Preserve the original error and the bounded terminal cleanup. Permit immediate settlement when cancellation before execution is established.
- Requested evidence: Use a successful initial null read, an interruption that leaves the command unfinished, and child disposal. Then expose a process, clear the command field during phase interruption and deliver an attributable condition. Show exact removal, preserved foreign controls and a failed qualification carrying the original error.

**R2-F7** · P1 · evidence_gap · blocks readiness — The passive-command test supplies the registration it must verify
- Location: `tests/KingmakerGunslinger.DomainTests/Sprint7VisualRepairTests.cs:2284`; acceptance: R4-F1, R7-F2, OWNER AMENDMENT 2 §§3C,3F, OWNER AMENDMENT 3 §3E
- Trigger: Evaluating the required Focused proof for the production Dust and Ice passive command route.
- Expected: The Focused case reaches the same Dust submission operation that selects both owners in production, and observes the exact command in both owners at its single Run boundary.
- Actual: The test calls IssueAndRun with a registration lambda written in the test. The production Dust binding is a separate lambda at RuntimeTestRunner.Sprint7Presentation.cs line 2757. Removing phase registration from that caller would leave the test green. The test does exercise capture before the phase retry for the command it registers.
- Why it blocks / does not: The required control cannot detect a recurrence of the passive Dust registration defect identified by audit A1-F5.
- Requested correction: Make the production passive submission, including its choice of both owners, callable with a controlled Run boundary. Exercise that operation in the existing handoff and phase-retry case.
- Requested evidence: Show one Run with the exact command already registered in both owners, the separate same-caster command in neither, and successful capture before the phase retry clears the field. Omitting the production phase registration must fail the control.

**R4-F2** · P2 · evidence_gap · blocks readiness — The native receipt does not establish ten LeaveCombat calls
- Location: `docs/missions/sprint7/recovery/S7-RECOVERY-MANIFEST.md:541`; acceptance: C3 §6, OWNER AMENDMENT 3 §§3F,4
- Trigger: Interpreting commandCleanupPerformed as a record of native combat exits.
- Expected: The handoff identifies which cleanup operations the receipt directly observed and which calls were conditional.
- Actual: The native delegate calls LeaveCombat only when IsInCombat is true. Guarded appends a performed row whenever that delegate returns, including when its body took no action. The manifest says combat exit ran on eight summons, the hostile and the caster. The rows establish that all ten callbacks were allowed and returned.
- Why it blocks / does not: The active handoff attributes native execution to individual actors beyond what the receipt establishes. The owner expressly required accurate coverage for this boundary.
- Requested correction: Describe the ten combat-exit callbacks as permitted and returned, with LeaveCombat conditional on IsInCombat. Keep the observed empty withholding list and the Focused foreign-queue result.
- Requested evidence: The existing receipt and delegate suffice to qualify the claim. A claim of ten actual LeaveCombat calls would require per-actor evidence of invocation.

**R1-F13** · P2 · required_gap · blocks readiness — The active integration plan still directs a candidate-4 build
- Location: `docs/missions/sprint7/recovery/S7-RECOVERY-MANIFEST.md:582`; acceptance: C3 §8, OWNER AMENDMENT 1 §§3F,7, OWNER AMENDMENT 2 §4, OWNER AMENDMENT 3 §4
- Trigger: Reading the conditional integration plan to determine the current C3 state and next authorized action.
- Expected: The conditional integration plan gives one current qualification state and clearly dates superseded instructions as history.
- Actual: The stack table correctly identifies candidate 4 and its session. The adjacent dependency paragraph still labels the amendment-2 state Current, names records head 80d224bd56 and directs that candidate 4 be built and qualified before review 4.
- Why it blocks / does not: The active plan gives a next action that has already consumed the last authorized package and review slots.
- Requested correction: Label the amendment-2 dependency check as historical and bring the active plan through candidate 4 and this review. Preserve the separate product source, reviewed H and absent C3 archive.
- Requested evidence: Cross-check the active paragraph against the existing stack table, candidate-4 receipt and controller review identity.

**R4-F3** · P2 · evidence_gap · blocks readiness — The candidate-4 pre-command budget estimates are unverified
- Location: `docs/missions/sprint7/recovery/S7-RECOVERY-MANIFEST.md:793`; acceptance: C3 §7, OWNER AMENDMENT 1 §6, OWNER AMENDMENT 3 §6
- Trigger: Checking the authorization conditions for the last recovery build and its mechanics session.
- Expected: Before each expensive command, the remaining build, session and final-gate work is estimated against the shared allowance.
- Actual: The ledger records 28 min 43.9 s available before the build and 20 min 35.0 s before the session. It contains a prospective estimate for candidate 3. I found no corresponding candidate-4 estimate in the supplied pre-build record, ledger or transcript.
- Why it blocks / does not: The final arithmetic establishes compliance with the time cap. The supplied evidence leaves the separate, explicitly required pre-command estimate unverified.
- Requested correction: Identify the existing contemporaneous estimates if available. Otherwise disclose that this preflight step cannot be verified from the retained record.
- Requested evidence: A dated pre-build and pre-session calculation or contemporaneous message covering the work then remaining. No further validation command is needed.

#### Prior finding assessments

- **R1-F11** — `fix_verified` — The production retirement operation marks the fixture attempt before disposal. The Focused controls use that operation and the outer fixture selection for both direct failure positions, observe one attempt, independent cleanup and refusal, and execute the throwing context getter before the successful attribution.
- **R1-F13** — `fix_rejected` — The structured acceptance map now matches all ten round-three bindings, and the earlier claim that the sources differ is withdrawn. The active integration paragraph still directs a future candidate-4 build beside the completed candidate-4 entry.
- **R1-F9** — `fix_verified` — Ground lookup and hostile placement now run inside guarded phase setup. The Focused failure reaches production outcome publication, retains the original lookup error, attempts position and subscription cleanup, and leaves the required sequence incomplete.
- **R2-F7** — `fix_rejected` — Phase interruption now selects exact issued references. The guard preserves foreign queue identity and order and continues permitted cleanup after a failure. The required passive-caller proof remains open because the test independently supplies the production caller's choice of owners.
- **R2-F8** — `fix_verified` — The shared early condition-release path publishes the exact condition identity with the original exception. The Focused control observes one named failure and refusal even when the later read is unavailable, and ordinary cleanup does not repeat that failed removal.
- **R3-F1** — `fix_verified` — After transfer, both new failures and newly registered residuals go to the consuming phase. The Focused deferred-cleanup case observes the late unsubscribe failure and outside-target residual alongside the original failure and verifies refusal.
- **R3-F2** — `fix_verified` — With both delivery notifications absent, the Focused case recovers the exact ended process before the zero-live census and removes its condition without fabricating a delivery observation. Foreign and pre-existing instances survive. A throwing context read remains unknown at the actual pre-delivery presence check.
- **R3-F3** — `fix_verified` — The tests exercise the production control declarations for missing, unreadable and not-yet-due controls, independent release and outside-target retirement. Final teardown re-reads the first target after fallback retires the second, keeps one request per target and names the never-settling control.
- **R3-F4** — `fix_verified` — Reconciliation is registered before the first settlement attempt. A changing end-state read or throwing sweep retains bounded terminal cleanup, subscription release has its own result, and an earlier error remains a qualification failure. R4-F1 concerns a different branch where no process has started yet.
- **R3-F5** — `fix_verified` — The corrected round-three total is 59 min 37.0 s, with 8.9 s of receipts and 17.3 s of outer batch time. The subsequent recorded durations reconcile through the reported final total of 75 min 47.4 s.

#### Evidence assessment

- **Inspected:** The fixed mission, all three owner amendments, repository review and validation instructions, feature diff, correction commits, prior findings, both pre-build audits, developer report, transcript, controller gate logs and candidate-4 native evidence.; The Dust passive and direct submission routes, command and condition owners, phase retry, child-to-phase transfer, terminal reconciliation, fixture retirement, Focused controls and active integration plan.; Candidate 4 ran product source 6cef765a8a1db854cdb64a28d09bbd65dd3b9765, package SHA-256 242f9a27358c11e35a43500e924e5011c06bf843ad23f7a661923a98e3270d93, DLL SHA-256 a08d06b6e0a81df22314314ad7b970f0cd264d29296f913e589f12e7ce5491c2 and C2 harness SHA-256 d77754bad4377734e87c9a70aaf0c980a306db82dbee25e335b9e9ba47ebb4f1. The subsequent change to H contains evidence and test changes.
- **Executed verification:** All four controller receipts bind production compilation and passing Focused gates to H. The filters ran 25 sprint7-visual tests, 11 summon-playtest-r6 tests, 9 native-profile tests and 5 loader-compat-probe tests. Receipt durations total 9.9 seconds.; Session 20261126T232851Z-owner-0.33.0-s7-c3-mechanics records 43/43 checks passing. All seven declared cells were admitted and completed. The separate restoration observations report a successful transaction, unchanged owner installation and saves, and restored settings and registry.; The native pre-delivery driver recorded the exact Dust execution #958676480 before delivery at frame 11797. At frame 12176 it had ended, one attributable Sickened had been acquired and released, and zero remained from that execution. Both foreign condition controls were observed present after owned release.; I compared the session callback row with the committed persisted row. They agree, including constructed same-frame scheduling at frame 5333, real playing IDs 471 and 472, and the live evaluator's null verdict. The H-bound sprint7-visual gate passed the persisted and collector-shaped comparison.; I cross-checked all ten round-three acceptance bindings against the structured review and reconciled the ledger arithmetic. The committed total is 75 min 28.9 s before the final gates. The reported 18.5 s outer gate batch brings it to 75 min 47.4 s of 90, leaving 14 min 12.6 s. Twenty failed validation attempts are recorded.
- **Source inspection only:** The null-before-start failure follows from Capture, ReleaseEach, ReleaseOwnedEffects and the later phase retry. The existing failure test makes the initial process getter throw.; The current passive caller registers with both owners. The Focused test supplies its own registration callback, so that caller binding was verified by inspection.; The possible queue-changing return move during LeaveCombat is audit-sourced. The Focused test uses a controlled queue-clearing callback.
- **Required gaps:** Retain cleanup for a still-unfinished command whose process reads null and whose interruption fails.; Exercise the production Dust passive submission that chooses both owners at the single Run boundary.; Correct the native combat-exit claim and the stale active integration instruction. Identify the pre-command budget estimates or record their evidentiary status.
- **Limitations:** This was a read-only review. Executed results come from the controller receipts and committed native observations. The controller withheld the mixed foreground session log.; All five native abandonment drivers recorded empty transferredResiduals. The native PASS establishes their observed paths. Evidence for failures occurring after a transfer comes from Focused controls.; The native command-cleanup rows record successful guard callbacks. They contain no per-actor observation of entry into LeaveCombat.

#### Summary

- **Delivered behavior:** Candidate 4 completed the mechanics-only session, demonstrated cleanup after disposal of an already-started Dust execution, preserved the two foreign controls, and produced matching live and persisted cue verdicts. Eight of the ten pending findings are verified. Cleanup after a failed interruption before process creation remains a product defect.
- **Scope reviewed:** Final C3 effects-ownership recovery against the original mission and all three owner amendments, with correction-round attention to the ten pending findings and their affected callers. Both pre-build audits retain their corrections_required verdicts. OWNER AMENDMENT 3 replaced the auxiliary audit prerequisite. C1 and C2 retain their accepted scopes at their recorded identities.
- **Pending later-phase work:** none
- **Unresolved questions:** Whether contemporaneous candidate-4 budget estimates exist outside the supplied review inputs.
- **Remaining owner review:** The owner decides whether to authorize work beyond four recovery candidates and four C3 reviews. C3 has no archive or handoff.; Visual approval for T2/T3/T6 and listening choices for T5/T7 remain with the owner on the existing materials.; Integration into frozen #219, any parent-review resumption, and eventual integration with master remain separate owner decisions.

#### Provenance (controller-recorded)

| | Requested | Observed |
|---|---|---|
| Model | `gpt-6.2-astra` | `gpt-6.2-astra` (turns ['gpt-6.2-astra']) |
| Reasoning effort | `max` | `max` (turns ['max']) |
| Sandbox / approvals | read-only / never | `read-only` / `never` |

Owner guidance bound to this review: none.

Owner amendments bound to this review: revision 3, sha256 `a64ab24d9e7445495e895b5d7f851bbf6f846722f2ffba1c8f0bc7d48a47ef1a`.

Codex CLI `0.170.0`, thread `01a21a3c-05ef-7fe1-8716-91698ca1c83d`, duration 1148.0s, response sha256 `458f7e1e432ae591f25916b9bc689baec40a1c9059553df3c3d3bf37b869a847`, request `kmg-codex-s7-recovery-c3-effects-ownership-20261125235239-ba5d-r04-69cc0c`. Observed values come from the Codex session rollout's recorded thread settings, not from the model's own statements; the serving backend's model identity is not separately exposed.

<details><summary>Complete structured review (agent-review.review.v1)</summary>

```json
{
  "candidate_commit": "43dd44a2bafad0455ab74b6393515c9bae4fd27a",
  "checkpoint_id": "final",
  "checkpoint_type": "final",
  "decision": "changes_required",
  "decision_rationale": "An unfinished Dust command can outlive its cleanup owner when its process is still null and the first interruption fails. The four Focused gates and the 43-check native session passed, but neither exercised that branch. The required passive-caller test and parts of the handoff record also remain incomplete.\n\nContinuation for Claude\n\nRecord the authorized-limit stop. Recovery candidate 4 and formal review 4 exhaust both ceilings under OWNER AMENDMENT 3 \u00a76. C3 has no archive or current handoff. A product correction would need to retain the unfinished command through confirmed cancellation or execution settlement, then prove late-effect cleanup through the actual passive submission. The records can be reconciled against the existing receipts and any contemporaneous budget estimate. Preserve the native PASS, both audit verdicts, earlier failures and the frozen #219/C1/C2 stack. Another package or formal review requires new owner authority.",
  "evidence_assessment": {
    "executed_verification": [
      "All four controller receipts bind production compilation and passing Focused gates to H. The filters ran 25 sprint7-visual tests, 11 summon-playtest-r6 tests, 9 native-profile tests and 5 loader-compat-probe tests. Receipt durations total 9.9 seconds.",
      "Session 20261126T232851Z-owner-0.33.0-s7-c3-mechanics records 43/43 checks passing. All seven declared cells were admitted and completed. The separate restoration observations report a successful transaction, unchanged owner installation and saves, and restored settings and registry.",
      "The native pre-delivery driver recorded the exact Dust execution #958676480 before delivery at frame 11797. At frame 12176 it had ended, one attributable Sickened had been acquired and released, and zero remained from that execution. Both foreign condition controls were observed present after owned release.",
      "I compared the session callback row with the committed persisted row. They agree, including constructed same-frame scheduling at frame 5333, real playing IDs 471 and 472, and the live evaluator's null verdict. The H-bound sprint7-visual gate passed the persisted and collector-shaped comparison.",
      "I cross-checked all ten round-three acceptance bindings against the structured review and reconciled the ledger arithmetic. The committed total is 75 min 28.9 s before the final gates. The reported 18.5 s outer gate batch brings it to 75 min 47.4 s of 90, leaving 14 min 12.6 s. Twenty failed validation attempts are recorded."
    ],
    "inspected": [
      "The fixed mission, all three owner amendments, repository review and validation instructions, feature diff, correction commits, prior findings, both pre-build audits, developer report, transcript, controller gate logs and candidate-4 native evidence.",
      "The Dust passive and direct submission routes, command and condition owners, phase retry, child-to-phase transfer, terminal reconciliation, fixture retirement, Focused controls and active integration plan.",
      "Candidate 4 ran product source 6cef765a8a1db854cdb64a28d09bbd65dd3b9765, package SHA-256 242f9a27358c11e35a43500e924e5011c06bf843ad23f7a661923a98e3270d93, DLL SHA-256 a08d06b6e0a81df22314314ad7b970f0cd264d29296f913e589f12e7ce5491c2 and C2 harness SHA-256 d77754bad4377734e87c9a70aaf0c980a306db82dbee25e335b9e9ba47ebb4f1. The subsequent change to H contains evidence and test changes."
    ],
    "limitations": [
      "This was a read-only review. Executed results come from the controller receipts and committed native observations. The controller withheld the mixed foreground session log.",
      "All five native abandonment drivers recorded empty transferredResiduals. The native PASS establishes their observed paths. Evidence for failures occurring after a transfer comes from Focused controls.",
      "The native command-cleanup rows record successful guard callbacks. They contain no per-actor observation of entry into LeaveCombat."
    ],
    "required_gaps": [
      "Retain cleanup for a still-unfinished command whose process reads null and whose interruption fails.",
      "Exercise the production Dust passive submission that chooses both owners at the single Run boundary.",
      "Correct the native combat-exit claim and the stale active integration instruction. Identify the pre-command budget estimates or record their evidentiary status."
    ],
    "source_inspection_only": [
      "The null-before-start failure follows from Capture, ReleaseEach, ReleaseOwnedEffects and the later phase retry. The existing failure test makes the initial process getter throw.",
      "The current passive caller registers with both owners. The Focused test supplies its own registration callback, so that caller binding was verified by inspection.",
      "The possible queue-changing return move during LeaveCombat is audit-sourced. The Focused test uses a controlled queue-clearing callback."
    ]
  },
  "findings": [
    {
      "acceptance_ids": [
        "R7-F2",
        "C3 \u00a75",
        "OWNER AMENDMENT 1 \u00a73B",
        "OWNER AMENDMENT 3 \u00a73E"
      ],
      "actual": "Capture treats a successful null process read as an empty result. ReleaseEach publishes the interruption error, but the settlement predicate checks only retained processes and commands whose process getter threw. With both lists empty, it can sweep, unsubscribe, run control teardown and discharge the reconciliation. The phase can later capture a newly started process before interrupting it, with no condition cleanup left to consume that capture. The later unfinished-command check records failure but does not restore the cleanup action.",
      "blocking_rationale": "A late attributable Sickened can remain after the last owned sweep. Recording the qualification failure does not release that condition.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "While an unsuccessfully interrupted command can still start, its original owner retains executable condition cleanup until cancellation is established or its execution and delivered effects settle.",
      "id": "R4-F1",
      "location": {
        "file": "src/KingmakerGunslinger/RuntimeTesting/Sprint7ObserverPolicy.cs",
        "line": 1476
      },
      "priority": "P1",
      "requested_correction": "Include still-unfinished commands with no process yet in the settlement decision after an unsuccessful release. Preserve the original error and the bounded terminal cleanup. Permit immediate settlement when cancellation before execution is established.",
      "requested_evidence": "Use a successful initial null read, an interruption that leaves the command unfinished, and child disposal. Then expose a process, clear the command field during phase interruption and deliver an attributable condition. Show exact removal, preserved foreign controls and a failed qualification carrying the original error.",
      "title": "A failed interruption can discharge Dust cleanup before the command starts",
      "trigger": "A submitted Dust command is unfinished, ExecutionProcess still reads null, and its first cell interruption throws before stopping it."
    },
    {
      "acceptance_ids": [
        "R4-F1",
        "R7-F2",
        "OWNER AMENDMENT 2 \u00a7\u00a73C,3F",
        "OWNER AMENDMENT 3 \u00a73E"
      ],
      "actual": "The test calls IssueAndRun with a registration lambda written in the test. The production Dust binding is a separate lambda at RuntimeTestRunner.Sprint7Presentation.cs line 2757. Removing phase registration from that caller would leave the test green. The test does exercise capture before the phase retry for the command it registers.",
      "blocking_rationale": "The required control cannot detect a recurrence of the passive Dust registration defect identified by audit A1-F5.",
      "blocks_readiness": true,
      "category": "evidence_gap",
      "expected": "The Focused case reaches the same Dust submission operation that selects both owners in production, and observes the exact command in both owners at its single Run boundary.",
      "id": "R2-F7",
      "location": {
        "file": "tests/KingmakerGunslinger.DomainTests/Sprint7VisualRepairTests.cs",
        "line": 2284
      },
      "priority": "P1",
      "requested_correction": "Make the production passive submission, including its choice of both owners, callable with a controlled Run boundary. Exercise that operation in the existing handoff and phase-retry case.",
      "requested_evidence": "Show one Run with the exact command already registered in both owners, the separate same-caster command in neither, and successful capture before the phase retry clears the field. Omitting the production phase registration must fail the control.",
      "title": "The passive-command test supplies the registration it must verify",
      "trigger": "Evaluating the required Focused proof for the production Dust and Ice passive command route."
    },
    {
      "acceptance_ids": [
        "C3 \u00a76",
        "OWNER AMENDMENT 3 \u00a7\u00a73F,4"
      ],
      "actual": "The native delegate calls LeaveCombat only when IsInCombat is true. Guarded appends a performed row whenever that delegate returns, including when its body took no action. The manifest says combat exit ran on eight summons, the hostile and the caster. The rows establish that all ten callbacks were allowed and returned.",
      "blocking_rationale": "The active handoff attributes native execution to individual actors beyond what the receipt establishes. The owner expressly required accurate coverage for this boundary.",
      "blocks_readiness": true,
      "category": "evidence_gap",
      "expected": "The handoff identifies which cleanup operations the receipt directly observed and which calls were conditional.",
      "id": "R4-F2",
      "location": {
        "file": "docs/missions/sprint7/recovery/S7-RECOVERY-MANIFEST.md",
        "line": 541
      },
      "priority": "P2",
      "requested_correction": "Describe the ten combat-exit callbacks as permitted and returned, with LeaveCombat conditional on IsInCombat. Keep the observed empty withholding list and the Focused foreign-queue result.",
      "requested_evidence": "The existing receipt and delegate suffice to qualify the claim. A claim of ten actual LeaveCombat calls would require per-actor evidence of invocation.",
      "title": "The native receipt does not establish ten LeaveCombat calls",
      "trigger": "Interpreting commandCleanupPerformed as a record of native combat exits."
    },
    {
      "acceptance_ids": [
        "C3 \u00a78",
        "OWNER AMENDMENT 1 \u00a7\u00a73F,7",
        "OWNER AMENDMENT 2 \u00a74",
        "OWNER AMENDMENT 3 \u00a74"
      ],
      "actual": "The stack table correctly identifies candidate 4 and its session. The adjacent dependency paragraph still labels the amendment-2 state Current, names records head 80d224bd56 and directs that candidate 4 be built and qualified before review 4.",
      "blocking_rationale": "The active plan gives a next action that has already consumed the last authorized package and review slots.",
      "blocks_readiness": true,
      "category": "required_gap",
      "expected": "The conditional integration plan gives one current qualification state and clearly dates superseded instructions as history.",
      "id": "R1-F13",
      "location": {
        "file": "docs/missions/sprint7/recovery/S7-RECOVERY-MANIFEST.md",
        "line": 582
      },
      "priority": "P2",
      "requested_correction": "Label the amendment-2 dependency check as historical and bring the active plan through candidate 4 and this review. Preserve the separate product source, reviewed H and absent C3 archive.",
      "requested_evidence": "Cross-check the active paragraph against the existing stack table, candidate-4 receipt and controller review identity.",
      "title": "The active integration plan still directs a candidate-4 build",
      "trigger": "Reading the conditional integration plan to determine the current C3 state and next authorized action."
    },
    {
      "acceptance_ids": [
        "C3 \u00a77",
        "OWNER AMENDMENT 1 \u00a76",
        "OWNER AMENDMENT 3 \u00a76"
      ],
      "actual": "The ledger records 28 min 43.9 s available before the build and 20 min 35.0 s before the session. It contains a prospective estimate for candidate 3. I found no corresponding candidate-4 estimate in the supplied pre-build record, ledger or transcript.",
      "blocking_rationale": "The final arithmetic establishes compliance with the time cap. The supplied evidence leaves the separate, explicitly required pre-command estimate unverified.",
      "blocks_readiness": true,
      "category": "evidence_gap",
      "expected": "Before each expensive command, the remaining build, session and final-gate work is estimated against the shared allowance.",
      "id": "R4-F3",
      "location": {
        "file": "docs/missions/sprint7/recovery/S7-RECOVERY-MANIFEST.md",
        "line": 793
      },
      "priority": "P2",
      "requested_correction": "Identify the existing contemporaneous estimates if available. Otherwise disclose that this preflight step cannot be verified from the retained record.",
      "requested_evidence": "A dated pre-build and pre-session calculation or contemporaneous message covering the work then remaining. No further validation command is needed.",
      "title": "The candidate-4 pre-command budget estimates are unverified",
      "trigger": "Checking the authorization conditions for the last recovery build and its mechanics session."
    }
  ],
  "prior_finding_assessments": [
    {
      "assessment": "fix_verified",
      "finding_id": "R1-F11",
      "rationale": "The production retirement operation marks the fixture attempt before disposal. The Focused controls use that operation and the outer fixture selection for both direct failure positions, observe one attempt, independent cleanup and refusal, and execute the throwing context getter before the successful attribution."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R1-F13",
      "rationale": "The structured acceptance map now matches all ten round-three bindings, and the earlier claim that the sources differ is withdrawn. The active integration paragraph still directs a future candidate-4 build beside the completed candidate-4 entry."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R1-F9",
      "rationale": "Ground lookup and hostile placement now run inside guarded phase setup. The Focused failure reaches production outcome publication, retains the original lookup error, attempts position and subscription cleanup, and leaves the required sequence incomplete."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R2-F7",
      "rationale": "Phase interruption now selects exact issued references. The guard preserves foreign queue identity and order and continues permitted cleanup after a failure. The required passive-caller proof remains open because the test independently supplies the production caller's choice of owners."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R2-F8",
      "rationale": "The shared early condition-release path publishes the exact condition identity with the original exception. The Focused control observes one named failure and refusal even when the later read is unavailable, and ordinary cleanup does not repeat that failed removal."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R3-F1",
      "rationale": "After transfer, both new failures and newly registered residuals go to the consuming phase. The Focused deferred-cleanup case observes the late unsubscribe failure and outside-target residual alongside the original failure and verifies refusal."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R3-F2",
      "rationale": "With both delivery notifications absent, the Focused case recovers the exact ended process before the zero-live census and removes its condition without fabricating a delivery observation. Foreign and pre-existing instances survive. A throwing context read remains unknown at the actual pre-delivery presence check."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R3-F3",
      "rationale": "The tests exercise the production control declarations for missing, unreadable and not-yet-due controls, independent release and outside-target retirement. Final teardown re-reads the first target after fallback retires the second, keeps one request per target and names the never-settling control."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R3-F4",
      "rationale": "Reconciliation is registered before the first settlement attempt. A changing end-state read or throwing sweep retains bounded terminal cleanup, subscription release has its own result, and an earlier error remains a qualification failure. R4-F1 concerns a different branch where no process has started yet."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R3-F5",
      "rationale": "The corrected round-three total is 59 min 37.0 s, with 8.9 s of receipts and 17.3 s of outer batch time. The subsequent recorded durations reconcile through the reported final total of 75 min 47.4 s."
    }
  ],
  "request_id": "kmg-codex-s7-recovery-c3-effects-ownership-20261125235239-ba5d-r04-69cc0c",
  "round": 4,
  "schema_version": "agent-review.review.v1",
  "summary": {
    "delivered_behavior": "Candidate 4 completed the mechanics-only session, demonstrated cleanup after disposal of an already-started Dust execution, preserved the two foreign controls, and produced matching live and persisted cue verdicts. Eight of the ten pending findings are verified. Cleanup after a failed interruption before process creation remains a product defect.",
    "pending_later_phase_work": [],
    "remaining_owner_review": [
      "The owner decides whether to authorize work beyond four recovery candidates and four C3 reviews. C3 has no archive or handoff.",
      "Visual approval for T2/T3/T6 and listening choices for T5/T7 remain with the owner on the existing materials.",
      "Integration into frozen #219, any parent-review resumption, and eventual integration with master remain separate owner decisions."
    ],
    "scope_reviewed": "Final C3 effects-ownership recovery against the original mission and all three owner amendments, with correction-round attention to the ten pending findings and their affected callers. Both pre-build audits retain their corrections_required verdicts. OWNER AMENDMENT 3 replaced the auxiliary audit prerequisite. C1 and C2 retain their accepted scopes at their recorded identities.",
    "unresolved_questions": [
      "Whether contemporaneous candidate-4 budget estimates exist outside the supplied review inputs."
    ]
  }
}
```
</details>

_Model-generated review by Codex using the requested model, posted by the agent-review controller through the owner's GitHub account. Not a human review or approval._
