<!-- agent-review:v1 enrollment=kmg-codex-agent-review-workflow-20261120170436 key=r04-review part=1/1 sha256=1ffb57da5bb41968c40e7d976702560b441664caee31d5af55ac1b2462df2cbf -->
### Codex GPT-6.2 Astra — review — round 4 — candidate `a7a008ee84`

**Decision:** `changes_required` — The candidate fixes several earlier failures, including blocking-finding downgrades, foreground effort attribution, and reuse of an altered review at acceptance. The checkpoint still has paths to a stale handoff, acceptance with an unsatisfied hosted check, loss of a substantive publication on resume, and publication of archive content that differs from the accepted record. The candidate-bound test receipt records 62 passes and one skipped live smoke. Its passing cases do not cover the triggers below.

#### Findings

**R1-F1** · P1 · defect · blocks readiness — Recheck freshness after the final PR body edit
- Location: `tools/agent_review/agent_review.py:2118`; acceptance: WF04, WF12
- Trigger: The head or a required handoff condition changes while the controller updates the PR description.
- Expected: The acceptance transition uses the conditions observed after the last potentially slow publication and corrects a superseded public status.
- Actual: The second readiness check precedes update_pr_body. That method checks the head before calling gh.edit_body. Once the edit returns, finalize unconditionally saves AWAITING_OWNER_REVIEW. Even a new review request visible in update_pr_body's own PR read is ignored.
- Why it blocks / does not: A change during the last remote write can leave both the PR index and durable state declaring a stale handoff.
- Requested correction: Recheck handoff conditions after the PR body edit and before saving the completed state. If they changed, correct the public index and handoff receipt and persist the paused or superseded outcome.
- Requested evidence: Have the GitHub fixture move the branch or add a requested reviewer during edit_body. Verify the final status, PR body, and correction comment.

**R1-F6** · P1 · defect · blocks readiness — Preserve every required check when contexts share a name
- Location: `tools/agent_review/agent_review.py:469`; acceptance: WF08, WF12
- Trigger: Branch protection and an applicable ruleset require the same check name from different providers, or multiple check results share a required name.
- Expected: Every applicable required check and provider has a current, satisfactory result on the reviewed source.
- Actual: required_checks stores a single app ID per context. A later ruleset overwrites a protection or ruleset requirement for the same name. Readiness also accepts any matching success, even when another required result with that name is pending.
- Why it blocks / does not: A passing result from one provider can satisfy the controller while a separate required provider or check is still pending.
- Requested correction: Retain distinct context/provider requirements across protection and rulesets. Resolve the current results for each requirement and fail closed on ambiguous or pending matches.
- Requested evidence: Exercise the GitHub adapter with two policies requiring the same context from different app IDs. Supply a success for only one. Also cover a successful and a pending same-name result on H.

**R1-F7** · P1 · defect · blocks readiness — Authenticate posted bodies and accepted envelopes before archiving
- Location: `tools/agent_review/agent_review.py:1908`; acceptance: WF06, WF09
- Trigger: A local outbox body or review envelope changes after acceptance and before a manual or final archive.
- Expected: Every archived body and envelope matches the content authenticated when the controller published or accepted it.
- Actual: archive reads posted body files directly and rereads each envelope without checking its acceptance digest. It then computes fresh hashes over those bytes and pushes them. The later missing_publications check can detect a changed body only after push. A changed envelope can remain in the committed archive alongside a correct PR comment.
- Why it blocks / does not: The controller can permanently publish different content under the identity of an accepted review. A subsequent readiness failure cannot undo that publication.
- Requested correction: Verify each outbox part against its recorded digest and each envelope against the round's stored digest before creating archive intent or writing Git files. Apply the same integrity check to archive retries.
- Requested evidence: After an accepted review, change one posted body and, separately, one envelope before archive. Verify both cases stop before the archive commit or push and preserve the original remote head.

**R1-F8** · P1 · defect · blocks readiness — Enforce single ownership of a PR across local clones
- Location: `tools/agent_review/agent_review.py:831`; acceptance: WF07, WF10
- Trigger: Two controller invocations select different homes and different clones of the same enrolled PR branch.
- Expected: There is one writer for the enrolled repository and branch across supported local worktrees and clones.
- Actual: The branch-level lock and registry are under the selected AGENT_REVIEW_HOME. The independent lock and enrollment marker are under one worktree's git directory. Two local clones using different homes share neither claim.
- Why it blocks / does not: Two local controllers can enroll the same PR, launch different writers, and maintain incompatible round and publication ledgers.
- Requested correction: Anchor repository/branch ownership to a shared host-level location independent of both the selected state home and the checkout. Keep the physical worktree lock as an additional guard.
- Requested evidence: Use two clones of one temporary origin, separate workflow homes, the same branch, and the same simulated PR. Verify the second enrollment and competing writer are refused.

**R1-F9** · P2 · defect · blocks readiness — Require positive author identification for a posted comment
- Location: `tools/agent_review/agent_review.py:1082`; acceptance: WF06, WF10
- Trigger: GitHub returns a successful comment response without a usable author login.
- Expected: The controller observes the expected actor on the stored comment before recording success.
- Actual: The post-response check explicitly accepts a missing user. The GitHub adapter returns None when the author/login is missing, and later missing_publications compares the body by comment ID without verifying its author.
- Why it blocks / does not: A publication with unverified posting identity can be recorded as successful and used for handoff.
- Requested correction: Require the expected author on the post response or reconcile the returned comment through an authenticated read. Treat missing identity as unknown until verified.
- Requested evidence: Return a valid comment ID and URL with no login. Verify that the controller requires an expected-actor readback before accepting it.

**R1-F12** · P1 · defect · blocks readiness — Require the complete archive inventory when adopting a commit
- Location: `tools/agent_review/agent_review.py:1940`; acceptance: WF07, WF09
- Trigger: Recovery finds an unrecorded commit with the expected parent and some, but not all, of the pending archive records.
- Expected: An adopted interrupted archive commit contains exactly the persisted transaction inventory.
- Actual: The adoption check validates each changed path against expected, but never verifies that every expected path was added. A single-parent commit containing a proper subset of the intended records passes. Recovery can push it, mark every intended record archived, and clear the pending intent.
- Why it blocks / does not: A partial recovery commit can strand the workflow with missing durable records and a ledger that says they were stored.
- Requested correction: For recovery of one pending archive, require equality of the actual and intended path sets, along with parent, operation, mode and digest checks, before adoption, push or ledger updates.
- Requested evidence: Resume a persisted multi-file archive intent with a single-parent commit containing only one intended record. Verify it is refused before push and the missing records remain pending.

**R1-F22** · P2 · defect · blocks readiness — Include the active final receipt in handoff revalidation
- Location: `tools/agent_review/agent_review.py:2130`; acceptance: WF06, WF08
- Trigger: The PR head remains at A but its final handoff comment is removed or altered.
- Expected: A current handoff includes the recorded final receipt on the enrolled PR.
- Actual: missing_publications skips all keys beginning with final-. An owner handoff comment can disappear or be edited at an unchanged H/A, and status --remote can still report handoff_current true.
- Why it blocks / does not: The workflow reports a current handoff even when the substantive public handoff that identifies H and A is gone.
- Requested correction: Verify the active final receipt when checking an existing handoff. The exemption from recursively archiving that receipt should apply only to archive creation.
- Requested evidence: Complete a handoff, delete or alter its final comment, and inspect the unchanged head with status --remote. Verify loss of the receipt is surfaced.

**R3-F3** · P2 · defect · blocks readiness — Include validation evidence in disposition identity
- Location: `tools/agent_review/agent_review.py:1232`; acceptance: WF06
- Trigger: The developer corrects validation details while resubmitting the same candidate and finding dispositions.
- Expected: Replaying identical substantive content is idempotent. An amended developer reply has a visible correction.
- Actual: The disposition key hashes the candidate and dispositions, while the published body also includes validation. Correcting a command, result, duration, or scope at the same H with the same dispositions reuses the old outbox body.
- Why it blocks / does not: The accepted developer report and the visible developer reply can disagree about the verification performed.
- Requested correction: Derive the idempotency key from the complete canonical substantive body or all fields used to render it.
- Requested evidence: Replay an identical developer report, then resubmit it with only the validation evidence changed. Verify one comment for the replay and a distinct visible reply for the amendment.

**R3-F6** · P1 · defect · blocks readiness — Bind foreground execution receipts to their source candidate
- Location: `tools/agent_review/agent_review.py:1358`; acceptance: WF04, WF08
- Trigger: A foreground report cites an older receipt retained in the same enrollment's evidence directory.
- Expected: Execution evidence presented as candidate-bound has a controller-verifiable association with its original source and content.
- Actual: write_review_inputs accepts any cited file whose resolved parent is the enrollment evidence directory. It checks no candidate identity or receipt digest before describing the selected files as candidate-bound. The regression fixture itself accepts a receipt containing only a test count and OK.
- Why it blocks / does not: An earlier candidate's run can be presented to the independent reviewer as execution on H.
- Requested correction: Keep foreground transcript isolation. Verify the candidate and content identity of receipts represented as current execution, and label any historical or unverified references accurately.
- Requested evidence: Retain a valid receipt from H1 in the enrollment directory, cite it from H2, and verify the reviewer cannot receive it labeled as execution on H2. Cover alteration of a registered receipt as well.

**R4-F1** · P1 · defect · blocks readiness — Recover historical review publications before advancing
- Location: `tools/agent_review/agent_review.py:1573`; acceptance: WF04, WF06, WF07
- Trigger: A review becomes historical during a publication outage or while its ordinary review body is already queued.
- Expected: A historical review and any necessary correction survive an outage and appear exactly once with their correct status before the workflow completes.
- Actual: make_historical clears the active round and saves the develop phase before publishing. If publication fails, resume can start new development without retrying that message. Archive and missing_publications both skip pending records. If a normal review body was already queued before the head moved, the historical publication also reuses that original body under the same key.
- Why it blocks / does not: A later handoff can omit a substantive review or leave a passing review visible without its historical qualification.
- Requested correction: Persist the historical publication or correction as a required pending operation. Reconcile it on resume before advancing, and require every substantive publication to be settled before handoff.
- Requested evidence: Move H while a validated review is awaiting publication, then inject an outage while publishing its historical status. Restart, complete a later round, and verify the original review, correction, and archive all agree.

**R4-F2** · P1 · required_gap · blocks readiness — Enforce the behavior suite in the shipped controller-core plan
- Location: `tools/agent_review/plans/agent-review-setup.json:8`; acceptance: WF08, WF12
- Trigger: A controller-core report omits its required suite and the reviewer returns a passing decision with no explicit gap.
- Expected: The production setup checkpoint enforces its required focused suite and its candidate-bound execution evidence.
- Actual: The controller-core plan requires behavior tests in prose but has no required_validation entry. accept_report accepts empty validation and evidence arrays for this checkpoint. The added gate test exercises a custom fixture plan. Even configured gates use substring matching against a free-text command.
- Why it blocks / does not: The shipped checkpoint can accept a ready report and passing review with its explicitly mandatory behavior suite omitted.
- Requested correction: Declare the mandatory behavior suite in the shipped checkpoint and validate it using an unambiguous gate identity and evidence binding.
- Requested evidence: Load the shipped plan. Verify that an omitted suite, an unrelated command mentioning the suite, and a receipt for the wrong H are refused, while the genuine candidate-bound run is accepted.

**R4-F3** · P1 · defect · blocks readiness — Close the child-launch gap in single-writer recovery
- Location: `tools/agent_review/agent_review.py:688`; acceptance: WF07
- Trigger: The controller exits after creating the headless developer process and before persisting its PID.
- Expected: Resume establishes that the previous writer has exited before starting or resuming the enrolled session.
- Actual: The controller saves launch intent before dispatch, then starts Claude and records active_pid through a callback. A controller exit between Popen and the PID save leaves a live child with no recorded PID. assert_no_active_developer checks only that PID, and the interrupted-launch branch proceeds to dispatch.
- Why it blocks / does not: Recovery can create two live developers for the same worktree or session even when both controllers use the same home.
- Requested correction: Use a recoverable child-owned lease or equivalent launch handshake. Treat an outstanding launch without a verifiable process outcome as ambiguous until reconciled.
- Requested evidence: Use a harmless local child and interrupt the controller after child creation but before PID persistence. Verify a new controller refuses a second writer while the first remains alive.

**R4-F4** · P1 · defect · blocks readiness — Check enrollment authority before a generic helper push
- Location: `tools/agent_review/agent_review.py:2692`; acceptance: WF07, WF10
- Trigger: An enrolled branch remains locally pushable after its draft PR is closed, retargeted, or handed off.
- Expected: The developer's push capability remains limited to the current writable enrollment and its verified draft PR.
- Actual: The generic push CLI loads the enrollment and calls guarded_push directly. It verifies the checkout, URLs, branch, and fast-forward relationship, but does not check whether the enrolled PR is still an open draft with the expected head/base or whether the enrollment already reached handoff.
- Why it blocks / does not: The helper can perform an external write after the enrollment's PR authority has ended or changed.
- Requested correction: Validate current enrollment and PR authority at the helper entry point in a way that works while the owning headless controller holds its lock. Retain the existing branch and effective push-URL checks.
- Requested evidence: For a wrapper-free fixture, close or retarget the PR, and separately complete the handoff. Invoke the helper and verify the remote branch does not move.

**R4-F5** · P2 · defect · blocks readiness — Require successful completion in model preflight
- Location: `tools/agent_review/agent_review.py:2487`; acceptance: WF01, WF05
- Trigger: A preflight process emits the expected model settings but fails before delivering the requested result.
- Expected: Preflight success records successful completion on the requested model and the expected response from each role.
- Actual: The developer preflight ok field tests model and effort metadata, independently of exit code, error subtype, session agreement, and structured_output_ok. The reviewer preflight does not validate its requested final JSON response either. The CLI uses these ok fields for its success exit.
- Why it blocks / does not: An unsuccessful model invocation can produce a successful controller preflight, compromising the model configuration evidence used by this workflow.
- Requested correction: Apply the normal terminal outcome and provenance checks to preflight and validate each expected preflight response before returning success.
- Requested evidence: Supply correct model metadata with a failed developer terminal event, and separately a missing or malformed reviewer result. Both preflights should return a visible failure.

#### Prior finding assessments

- **R1-F1** — `fix_rejected` — Review publication now has a useful post-publication freshness check. Finalization still performs another remote write after its last full check and commits readiness unconditionally afterward.
- **R1-F11** — `fix_verified` — finish_round saves the accepted round, counter, next phase, and blocked outcome together. review_attempt likewise persists successful-attempt accounting with promotion to validated.
- **R1-F12** — `fix_rejected` — Missing files can be recreated from the persisted intent and a full unrecorded commit can be adopted. The adoption predicate also accepts a strict subset of that intent.
- **R1-F13** — `fix_verified` — Candidate capture now checks the full excluded M..H transcript delta, including other PR directories, against addition status, mode and recorded content. The foreign-record fixture covers the previous omission.
- **R1-F14** — `fix_verified` — Bare configuration names are constrained to identifiers, and both named and explicit files are resolved and checked against trusted parent directories.
- **R1-F16** — `fix_verified` — Foreground effort extraction now uses the main assistant event's effort fields. The test includes user text mentioning effort and a sidechain event with another model and effort.
- **R1-F17** — `fix_verified` — The normal headless report path requires an explicit false error flag, successful terminal subtype, successful exit, enrolled session and structured report. The separate preflight predicate is covered by R4-F5.
- **R1-F18** — `fix_verified` — Operational JSON decoding exceptions within guarded work now reach the persisted PAUSED path. The recorded regression reads the resulting state from disk.
- **R1-F19** — `fix_verified` — A project with push_command is refused by the built-in helper, and its composed allowances omit that helper. This enforces the repository-specific wrapper requirement.
- **R1-F22** — `fix_rejected` — Remote status now reevaluates comparison and gate conditions at unchanged A. It still reports the handoff current when its final receipt is missing.
- **R1-F3** — `fix_verified` — semantic_problems rejects a required_gap marked nonblocking and rejects passing results with explicit required evidence gaps. The separate omission of the suite from the shipped plan is R4-F2.
- **R1-F4** — `fix_verified` — An open blocking finding cannot be restated as nonblocking, its blocking flag remains set while open, and a developer cannot defer it as optional.
- **R1-F6** — `fix_rejected` — The single-provider case now checks the app ID and lookup failures generally stop readiness. Requirements sharing a context are still collapsed before evaluation.
- **R1-F7** — `fix_rejected` — Pending sends now verify the body digest and reject rival same-actor markers. The archive path consumes posted body files and accepted envelope files without authenticating them before push.
- **R1-F8** — `fix_rejected` — The physical git-directory lock fixes two homes targeting one checkout. Two clones and two homes targeting the same PR still have independent ownership claims.
- **R1-F9** — `fix_rejected` — The login is refreshed by verify_inputs and an explicitly wrong posting login is refused. Missing author identity is still accepted as a successful publication.
- **R3-F1** — `fix_verified` — A preserved validated result must match its stored digest and workflow fingerprint before acceptance. Activation invalidates a validated or pre-handoff ready result from the previous workflow. R1-F7 covers the later archive read.
- **R3-F2** — `fix_verified` — The inbox is rejected if already present, including a dangling link, and its newly created directory is checked against the snapshot parent before input copies.
- **R3-F3** — `fix_rejected` — Identical dispositions now replay once and a changed disposition creates a new message. The key omits validation fields that are part of the published reply.
- **R3-F4** — `fix_verified` — readiness checks the configured runtime lock independently of the restoration claim. The fixture shows finalization stopping while the lock exists.
- **R3-F5** — `fix_verified` — The generic helper checks every effective push URL before invoking Git. The wrong-pushurl regression verifies that the approved remote head stays fixed.
- **R3-F6** — `fix_rejected` — The foreground session log is withheld and headless output is sanitized before truncation. Receipt selection is scoped to an enrollment directory but still represents unbound files as candidate-specific evidence.

#### Evidence assessment

- **Inspected:** The pinned setup mission, repository instructions, validation policy, workflow protocol, checkpoint plan, full feature diff, prior ledger, developer report, foreground transcript notice, and candidate-bound test receipt.; The controller's CLI dispatch, model adapters, permission configuration, schemas, prompts, enrollment and process locks, report acceptance, publication, archive recovery, readiness checks, and their regression fixtures.
- **Executed verification:** Read-only Git checks confirmed detached HEAD H and merge base M. The supplied 6,190-line feature diff and the Git-generated M..H diff with the transcript exclusion had the same Git blob hash, c973da79b562813066aa9c23bedd188370822b86. The mission file matched the supplied SHA-256.; The controller-supplied receipt for H records unittest discovery on a clean detached worktree, exit 0, 63 tests in 117.706 seconds, including one skipped live smoke. The behavior fixtures use temporary Git repositories and simulated model and GitHub responses.
- **Source inspection only:** The final PR edit, the archive adoption and push sequence, pending-outbox handling, the hosted-check inventory reduction, cross-clone ownership, child-process launch recovery, and the generic push CLI were traced in source.; No GitHub mutation, model invocation, installation, game workflow, or repository change was performed for this review.
- **Required gaps:** Candidate-bound behavior evidence for final freshness, provider collisions, durable publication recovery, archive integrity, and the writer-exclusion cases described in the findings.; Evidence that the shipped controller-core plan refuses a checkpoint report that omits its required behavior suite, and that a receipt from an earlier candidate cannot be represented as current execution.
- **Limitations:** This review used the read-only snapshot. The behavior-suite result comes from the supplied receipt. An attempted read-only Python inspection could not start the sandbox's Python executable, so the additional failure paths below are source-inspection findings.; The foreground transcript contains a receipt pointer. The separately reported live smoke run 6 summary is outside the supplied inbox. Its real-model outcome could not be independently confirmed here and is not needed to establish these controller defects.

#### Summary

- **Delivered behavior:** The candidate implements detached candidate review, explicit model selection and provenance checks, structured decisions, persistent round accounting, foreground and headless modes, GitHub publication, and transcript archiving. The behavior suite demonstrates a complete simulated correction cycle and intermediate advancement.
- **Scope reviewed:** Intermediate controller-core checkpoint for draft PR #194 at H, assessed against the pinned setup mission, protocol, 22 pending prior findings, and supplied candidate-bound evidence.
- **Pending later-phase work:** The setup-final checkpoint owns the completed installation and permissions account, workflow documentation, Black Tentacles launch binding, live pilot evidence, and setup acceptance report.
- **Unresolved questions:** none
- **Remaining owner review:** Final acceptance of the completed workflow and its operational handoff, followed by the owner's separate decision about merge and use for Black Tentacles.

#### Provenance (controller-recorded)

| | Requested | Observed |
|---|---|---|
| Model | `gpt-6.2-astra` | `gpt-6.2-astra` (turns ['gpt-6.2-astra']) |
| Reasoning effort | `high` | `high` (turns ['high']) |
| Sandbox / approvals | read-only / never | `read-only` / `never` |

Codex CLI `0.170.0`, thread `01a1fb4c-23ea-70a3-9327-66345340f822`, duration 345.9s, response sha256 `463327a02cfb00dccd25a5dfb475be1b28b3ac80b256534c500cfdbe23729a49`, request `kmg-codex-agent-review-workflow-20261120170436-r04-50061e`. Observed values come from the Codex session rollout's recorded thread settings, not from the model's own statements; the serving backend's model identity is not separately exposed.

<details><summary>Complete structured review (agent-review.review.v1)</summary>

```json
{
  "candidate_commit": "a7a008ee849d4971f3e5930f5369a8dc37c03654",
  "checkpoint_id": "controller-core",
  "checkpoint_type": "intermediate",
  "decision": "changes_required",
  "decision_rationale": "The candidate fixes several earlier failures, including blocking-finding downgrades, foreground effort attribution, and reuse of an altered review at acceptance. The checkpoint still has paths to a stale handoff, acceptance with an unsatisfied hosted check, loss of a substantive publication on resume, and publication of archive content that differs from the accepted record. The candidate-bound test receipt records 62 passes and one skipped live smoke. Its passing cases do not cover the triggers below.",
  "evidence_assessment": {
    "executed_verification": [
      "Read-only Git checks confirmed detached HEAD H and merge base M. The supplied 6,190-line feature diff and the Git-generated M..H diff with the transcript exclusion had the same Git blob hash, c973da79b562813066aa9c23bedd188370822b86. The mission file matched the supplied SHA-256.",
      "The controller-supplied receipt for H records unittest discovery on a clean detached worktree, exit 0, 63 tests in 117.706 seconds, including one skipped live smoke. The behavior fixtures use temporary Git repositories and simulated model and GitHub responses."
    ],
    "inspected": [
      "The pinned setup mission, repository instructions, validation policy, workflow protocol, checkpoint plan, full feature diff, prior ledger, developer report, foreground transcript notice, and candidate-bound test receipt.",
      "The controller's CLI dispatch, model adapters, permission configuration, schemas, prompts, enrollment and process locks, report acceptance, publication, archive recovery, readiness checks, and their regression fixtures."
    ],
    "limitations": [
      "This review used the read-only snapshot. The behavior-suite result comes from the supplied receipt. An attempted read-only Python inspection could not start the sandbox's Python executable, so the additional failure paths below are source-inspection findings.",
      "The foreground transcript contains a receipt pointer. The separately reported live smoke run 6 summary is outside the supplied inbox. Its real-model outcome could not be independently confirmed here and is not needed to establish these controller defects."
    ],
    "required_gaps": [
      "Candidate-bound behavior evidence for final freshness, provider collisions, durable publication recovery, archive integrity, and the writer-exclusion cases described in the findings.",
      "Evidence that the shipped controller-core plan refuses a checkpoint report that omits its required behavior suite, and that a receipt from an earlier candidate cannot be represented as current execution."
    ],
    "source_inspection_only": [
      "The final PR edit, the archive adoption and push sequence, pending-outbox handling, the hosted-check inventory reduction, cross-clone ownership, child-process launch recovery, and the generic push CLI were traced in source.",
      "No GitHub mutation, model invocation, installation, game workflow, or repository change was performed for this review."
    ]
  },
  "findings": [
    {
      "acceptance_ids": [
        "WF04",
        "WF12"
      ],
      "actual": "The second readiness check precedes update_pr_body. That method checks the head before calling gh.edit_body. Once the edit returns, finalize unconditionally saves AWAITING_OWNER_REVIEW. Even a new review request visible in update_pr_body's own PR read is ignored.",
      "blocking_rationale": "A change during the last remote write can leave both the PR index and durable state declaring a stale handoff.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "The acceptance transition uses the conditions observed after the last potentially slow publication and corrects a superseded public status.",
      "id": "R1-F1",
      "location": {
        "file": "tools/agent_review/agent_review.py",
        "line": 2118
      },
      "priority": "P1",
      "requested_correction": "Recheck handoff conditions after the PR body edit and before saving the completed state. If they changed, correct the public index and handoff receipt and persist the paused or superseded outcome.",
      "requested_evidence": "Have the GitHub fixture move the branch or add a requested reviewer during edit_body. Verify the final status, PR body, and correction comment.",
      "title": "Recheck freshness after the final PR body edit",
      "trigger": "The head or a required handoff condition changes while the controller updates the PR description."
    },
    {
      "acceptance_ids": [
        "WF08",
        "WF12"
      ],
      "actual": "required_checks stores a single app ID per context. A later ruleset overwrites a protection or ruleset requirement for the same name. Readiness also accepts any matching success, even when another required result with that name is pending.",
      "blocking_rationale": "A passing result from one provider can satisfy the controller while a separate required provider or check is still pending.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Every applicable required check and provider has a current, satisfactory result on the reviewed source.",
      "id": "R1-F6",
      "location": {
        "file": "tools/agent_review/agent_review.py",
        "line": 469
      },
      "priority": "P1",
      "requested_correction": "Retain distinct context/provider requirements across protection and rulesets. Resolve the current results for each requirement and fail closed on ambiguous or pending matches.",
      "requested_evidence": "Exercise the GitHub adapter with two policies requiring the same context from different app IDs. Supply a success for only one. Also cover a successful and a pending same-name result on H.",
      "title": "Preserve every required check when contexts share a name",
      "trigger": "Branch protection and an applicable ruleset require the same check name from different providers, or multiple check results share a required name."
    },
    {
      "acceptance_ids": [
        "WF06",
        "WF09"
      ],
      "actual": "archive reads posted body files directly and rereads each envelope without checking its acceptance digest. It then computes fresh hashes over those bytes and pushes them. The later missing_publications check can detect a changed body only after push. A changed envelope can remain in the committed archive alongside a correct PR comment.",
      "blocking_rationale": "The controller can permanently publish different content under the identity of an accepted review. A subsequent readiness failure cannot undo that publication.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Every archived body and envelope matches the content authenticated when the controller published or accepted it.",
      "id": "R1-F7",
      "location": {
        "file": "tools/agent_review/agent_review.py",
        "line": 1908
      },
      "priority": "P1",
      "requested_correction": "Verify each outbox part against its recorded digest and each envelope against the round's stored digest before creating archive intent or writing Git files. Apply the same integrity check to archive retries.",
      "requested_evidence": "After an accepted review, change one posted body and, separately, one envelope before archive. Verify both cases stop before the archive commit or push and preserve the original remote head.",
      "title": "Authenticate posted bodies and accepted envelopes before archiving",
      "trigger": "A local outbox body or review envelope changes after acceptance and before a manual or final archive."
    },
    {
      "acceptance_ids": [
        "WF07",
        "WF10"
      ],
      "actual": "The branch-level lock and registry are under the selected AGENT_REVIEW_HOME. The independent lock and enrollment marker are under one worktree's git directory. Two local clones using different homes share neither claim.",
      "blocking_rationale": "Two local controllers can enroll the same PR, launch different writers, and maintain incompatible round and publication ledgers.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "There is one writer for the enrolled repository and branch across supported local worktrees and clones.",
      "id": "R1-F8",
      "location": {
        "file": "tools/agent_review/agent_review.py",
        "line": 831
      },
      "priority": "P1",
      "requested_correction": "Anchor repository/branch ownership to a shared host-level location independent of both the selected state home and the checkout. Keep the physical worktree lock as an additional guard.",
      "requested_evidence": "Use two clones of one temporary origin, separate workflow homes, the same branch, and the same simulated PR. Verify the second enrollment and competing writer are refused.",
      "title": "Enforce single ownership of a PR across local clones",
      "trigger": "Two controller invocations select different homes and different clones of the same enrolled PR branch."
    },
    {
      "acceptance_ids": [
        "WF06",
        "WF10"
      ],
      "actual": "The post-response check explicitly accepts a missing user. The GitHub adapter returns None when the author/login is missing, and later missing_publications compares the body by comment ID without verifying its author.",
      "blocking_rationale": "A publication with unverified posting identity can be recorded as successful and used for handoff.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "The controller observes the expected actor on the stored comment before recording success.",
      "id": "R1-F9",
      "location": {
        "file": "tools/agent_review/agent_review.py",
        "line": 1082
      },
      "priority": "P2",
      "requested_correction": "Require the expected author on the post response or reconcile the returned comment through an authenticated read. Treat missing identity as unknown until verified.",
      "requested_evidence": "Return a valid comment ID and URL with no login. Verify that the controller requires an expected-actor readback before accepting it.",
      "title": "Require positive author identification for a posted comment",
      "trigger": "GitHub returns a successful comment response without a usable author login."
    },
    {
      "acceptance_ids": [
        "WF07",
        "WF09"
      ],
      "actual": "The adoption check validates each changed path against expected, but never verifies that every expected path was added. A single-parent commit containing a proper subset of the intended records passes. Recovery can push it, mark every intended record archived, and clear the pending intent.",
      "blocking_rationale": "A partial recovery commit can strand the workflow with missing durable records and a ledger that says they were stored.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "An adopted interrupted archive commit contains exactly the persisted transaction inventory.",
      "id": "R1-F12",
      "location": {
        "file": "tools/agent_review/agent_review.py",
        "line": 1940
      },
      "priority": "P1",
      "requested_correction": "For recovery of one pending archive, require equality of the actual and intended path sets, along with parent, operation, mode and digest checks, before adoption, push or ledger updates.",
      "requested_evidence": "Resume a persisted multi-file archive intent with a single-parent commit containing only one intended record. Verify it is refused before push and the missing records remain pending.",
      "title": "Require the complete archive inventory when adopting a commit",
      "trigger": "Recovery finds an unrecorded commit with the expected parent and some, but not all, of the pending archive records."
    },
    {
      "acceptance_ids": [
        "WF06",
        "WF08"
      ],
      "actual": "missing_publications skips all keys beginning with final-. An owner handoff comment can disappear or be edited at an unchanged H/A, and status --remote can still report handoff_current true.",
      "blocking_rationale": "The workflow reports a current handoff even when the substantive public handoff that identifies H and A is gone.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "A current handoff includes the recorded final receipt on the enrolled PR.",
      "id": "R1-F22",
      "location": {
        "file": "tools/agent_review/agent_review.py",
        "line": 2130
      },
      "priority": "P2",
      "requested_correction": "Verify the active final receipt when checking an existing handoff. The exemption from recursively archiving that receipt should apply only to archive creation.",
      "requested_evidence": "Complete a handoff, delete or alter its final comment, and inspect the unchanged head with status --remote. Verify loss of the receipt is surfaced.",
      "title": "Include the active final receipt in handoff revalidation",
      "trigger": "The PR head remains at A but its final handoff comment is removed or altered."
    },
    {
      "acceptance_ids": [
        "WF06"
      ],
      "actual": "The disposition key hashes the candidate and dispositions, while the published body also includes validation. Correcting a command, result, duration, or scope at the same H with the same dispositions reuses the old outbox body.",
      "blocking_rationale": "The accepted developer report and the visible developer reply can disagree about the verification performed.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Replaying identical substantive content is idempotent. An amended developer reply has a visible correction.",
      "id": "R3-F3",
      "location": {
        "file": "tools/agent_review/agent_review.py",
        "line": 1232
      },
      "priority": "P2",
      "requested_correction": "Derive the idempotency key from the complete canonical substantive body or all fields used to render it.",
      "requested_evidence": "Replay an identical developer report, then resubmit it with only the validation evidence changed. Verify one comment for the replay and a distinct visible reply for the amendment.",
      "title": "Include validation evidence in disposition identity",
      "trigger": "The developer corrects validation details while resubmitting the same candidate and finding dispositions."
    },
    {
      "acceptance_ids": [
        "WF04",
        "WF08"
      ],
      "actual": "write_review_inputs accepts any cited file whose resolved parent is the enrollment evidence directory. It checks no candidate identity or receipt digest before describing the selected files as candidate-bound. The regression fixture itself accepts a receipt containing only a test count and OK.",
      "blocking_rationale": "An earlier candidate's run can be presented to the independent reviewer as execution on H.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Execution evidence presented as candidate-bound has a controller-verifiable association with its original source and content.",
      "id": "R3-F6",
      "location": {
        "file": "tools/agent_review/agent_review.py",
        "line": 1358
      },
      "priority": "P1",
      "requested_correction": "Keep foreground transcript isolation. Verify the candidate and content identity of receipts represented as current execution, and label any historical or unverified references accurately.",
      "requested_evidence": "Retain a valid receipt from H1 in the enrollment directory, cite it from H2, and verify the reviewer cannot receive it labeled as execution on H2. Cover alteration of a registered receipt as well.",
      "title": "Bind foreground execution receipts to their source candidate",
      "trigger": "A foreground report cites an older receipt retained in the same enrollment's evidence directory."
    },
    {
      "acceptance_ids": [
        "WF04",
        "WF06",
        "WF07"
      ],
      "actual": "make_historical clears the active round and saves the develop phase before publishing. If publication fails, resume can start new development without retrying that message. Archive and missing_publications both skip pending records. If a normal review body was already queued before the head moved, the historical publication also reuses that original body under the same key.",
      "blocking_rationale": "A later handoff can omit a substantive review or leave a passing review visible without its historical qualification.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "A historical review and any necessary correction survive an outage and appear exactly once with their correct status before the workflow completes.",
      "id": "R4-F1",
      "location": {
        "file": "tools/agent_review/agent_review.py",
        "line": 1573
      },
      "priority": "P1",
      "requested_correction": "Persist the historical publication or correction as a required pending operation. Reconcile it on resume before advancing, and require every substantive publication to be settled before handoff.",
      "requested_evidence": "Move H while a validated review is awaiting publication, then inject an outage while publishing its historical status. Restart, complete a later round, and verify the original review, correction, and archive all agree.",
      "title": "Recover historical review publications before advancing",
      "trigger": "A review becomes historical during a publication outage or while its ordinary review body is already queued."
    },
    {
      "acceptance_ids": [
        "WF08",
        "WF12"
      ],
      "actual": "The controller-core plan requires behavior tests in prose but has no required_validation entry. accept_report accepts empty validation and evidence arrays for this checkpoint. The added gate test exercises a custom fixture plan. Even configured gates use substring matching against a free-text command.",
      "blocking_rationale": "The shipped checkpoint can accept a ready report and passing review with its explicitly mandatory behavior suite omitted.",
      "blocks_readiness": true,
      "category": "required_gap",
      "expected": "The production setup checkpoint enforces its required focused suite and its candidate-bound execution evidence.",
      "id": "R4-F2",
      "location": {
        "file": "tools/agent_review/plans/agent-review-setup.json",
        "line": 8
      },
      "priority": "P1",
      "requested_correction": "Declare the mandatory behavior suite in the shipped checkpoint and validate it using an unambiguous gate identity and evidence binding.",
      "requested_evidence": "Load the shipped plan. Verify that an omitted suite, an unrelated command mentioning the suite, and a receipt for the wrong H are refused, while the genuine candidate-bound run is accepted.",
      "title": "Enforce the behavior suite in the shipped controller-core plan",
      "trigger": "A controller-core report omits its required suite and the reviewer returns a passing decision with no explicit gap."
    },
    {
      "acceptance_ids": [
        "WF07"
      ],
      "actual": "The controller saves launch intent before dispatch, then starts Claude and records active_pid through a callback. A controller exit between Popen and the PID save leaves a live child with no recorded PID. assert_no_active_developer checks only that PID, and the interrupted-launch branch proceeds to dispatch.",
      "blocking_rationale": "Recovery can create two live developers for the same worktree or session even when both controllers use the same home.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Resume establishes that the previous writer has exited before starting or resuming the enrolled session.",
      "id": "R4-F3",
      "location": {
        "file": "tools/agent_review/agent_review.py",
        "line": 688
      },
      "priority": "P1",
      "requested_correction": "Use a recoverable child-owned lease or equivalent launch handshake. Treat an outstanding launch without a verifiable process outcome as ambiguous until reconciled.",
      "requested_evidence": "Use a harmless local child and interrupt the controller after child creation but before PID persistence. Verify a new controller refuses a second writer while the first remains alive.",
      "title": "Close the child-launch gap in single-writer recovery",
      "trigger": "The controller exits after creating the headless developer process and before persisting its PID."
    },
    {
      "acceptance_ids": [
        "WF07",
        "WF10"
      ],
      "actual": "The generic push CLI loads the enrollment and calls guarded_push directly. It verifies the checkout, URLs, branch, and fast-forward relationship, but does not check whether the enrolled PR is still an open draft with the expected head/base or whether the enrollment already reached handoff.",
      "blocking_rationale": "The helper can perform an external write after the enrollment's PR authority has ended or changed.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "The developer's push capability remains limited to the current writable enrollment and its verified draft PR.",
      "id": "R4-F4",
      "location": {
        "file": "tools/agent_review/agent_review.py",
        "line": 2692
      },
      "priority": "P1",
      "requested_correction": "Validate current enrollment and PR authority at the helper entry point in a way that works while the owning headless controller holds its lock. Retain the existing branch and effective push-URL checks.",
      "requested_evidence": "For a wrapper-free fixture, close or retarget the PR, and separately complete the handoff. Invoke the helper and verify the remote branch does not move.",
      "title": "Check enrollment authority before a generic helper push",
      "trigger": "An enrolled branch remains locally pushable after its draft PR is closed, retargeted, or handed off."
    },
    {
      "acceptance_ids": [
        "WF01",
        "WF05"
      ],
      "actual": "The developer preflight ok field tests model and effort metadata, independently of exit code, error subtype, session agreement, and structured_output_ok. The reviewer preflight does not validate its requested final JSON response either. The CLI uses these ok fields for its success exit.",
      "blocking_rationale": "An unsuccessful model invocation can produce a successful controller preflight, compromising the model configuration evidence used by this workflow.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Preflight success records successful completion on the requested model and the expected response from each role.",
      "id": "R4-F5",
      "location": {
        "file": "tools/agent_review/agent_review.py",
        "line": 2487
      },
      "priority": "P2",
      "requested_correction": "Apply the normal terminal outcome and provenance checks to preflight and validate each expected preflight response before returning success.",
      "requested_evidence": "Supply correct model metadata with a failed developer terminal event, and separately a missing or malformed reviewer result. Both preflights should return a visible failure.",
      "title": "Require successful completion in model preflight",
      "trigger": "A preflight process emits the expected model settings but fails before delivering the requested result."
    }
  ],
  "prior_finding_assessments": [
    {
      "assessment": "fix_rejected",
      "finding_id": "R1-F1",
      "rationale": "Review publication now has a useful post-publication freshness check. Finalization still performs another remote write after its last full check and commits readiness unconditionally afterward."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R1-F11",
      "rationale": "finish_round saves the accepted round, counter, next phase, and blocked outcome together. review_attempt likewise persists successful-attempt accounting with promotion to validated."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R1-F12",
      "rationale": "Missing files can be recreated from the persisted intent and a full unrecorded commit can be adopted. The adoption predicate also accepts a strict subset of that intent."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R1-F13",
      "rationale": "Candidate capture now checks the full excluded M..H transcript delta, including other PR directories, against addition status, mode and recorded content. The foreign-record fixture covers the previous omission."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R1-F14",
      "rationale": "Bare configuration names are constrained to identifiers, and both named and explicit files are resolved and checked against trusted parent directories."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R1-F16",
      "rationale": "Foreground effort extraction now uses the main assistant event's effort fields. The test includes user text mentioning effort and a sidechain event with another model and effort."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R1-F17",
      "rationale": "The normal headless report path requires an explicit false error flag, successful terminal subtype, successful exit, enrolled session and structured report. The separate preflight predicate is covered by R4-F5."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R1-F18",
      "rationale": "Operational JSON decoding exceptions within guarded work now reach the persisted PAUSED path. The recorded regression reads the resulting state from disk."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R1-F19",
      "rationale": "A project with push_command is refused by the built-in helper, and its composed allowances omit that helper. This enforces the repository-specific wrapper requirement."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R1-F22",
      "rationale": "Remote status now reevaluates comparison and gate conditions at unchanged A. It still reports the handoff current when its final receipt is missing."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R1-F3",
      "rationale": "semantic_problems rejects a required_gap marked nonblocking and rejects passing results with explicit required evidence gaps. The separate omission of the suite from the shipped plan is R4-F2."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R1-F4",
      "rationale": "An open blocking finding cannot be restated as nonblocking, its blocking flag remains set while open, and a developer cannot defer it as optional."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R1-F6",
      "rationale": "The single-provider case now checks the app ID and lookup failures generally stop readiness. Requirements sharing a context are still collapsed before evaluation."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R1-F7",
      "rationale": "Pending sends now verify the body digest and reject rival same-actor markers. The archive path consumes posted body files and accepted envelope files without authenticating them before push."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R1-F8",
      "rationale": "The physical git-directory lock fixes two homes targeting one checkout. Two clones and two homes targeting the same PR still have independent ownership claims."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R1-F9",
      "rationale": "The login is refreshed by verify_inputs and an explicitly wrong posting login is refused. Missing author identity is still accepted as a successful publication."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R3-F1",
      "rationale": "A preserved validated result must match its stored digest and workflow fingerprint before acceptance. Activation invalidates a validated or pre-handoff ready result from the previous workflow. R1-F7 covers the later archive read."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R3-F2",
      "rationale": "The inbox is rejected if already present, including a dangling link, and its newly created directory is checked against the snapshot parent before input copies."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R3-F3",
      "rationale": "Identical dispositions now replay once and a changed disposition creates a new message. The key omits validation fields that are part of the published reply."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R3-F4",
      "rationale": "readiness checks the configured runtime lock independently of the restoration claim. The fixture shows finalization stopping while the lock exists."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R3-F5",
      "rationale": "The generic helper checks every effective push URL before invoking Git. The wrong-pushurl regression verifies that the approved remote head stays fixed."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R3-F6",
      "rationale": "The foreground session log is withheld and headless output is sanitized before truncation. Receipt selection is scoped to an enrollment directory but still represents unbound files as candidate-specific evidence."
    }
  ],
  "request_id": "kmg-codex-agent-review-workflow-20261120170436-r04-50061e",
  "round": 4,
  "schema_version": "agent-review.review.v1",
  "summary": {
    "delivered_behavior": "The candidate implements detached candidate review, explicit model selection and provenance checks, structured decisions, persistent round accounting, foreground and headless modes, GitHub publication, and transcript archiving. The behavior suite demonstrates a complete simulated correction cycle and intermediate advancement.",
    "pending_later_phase_work": [
      "The setup-final checkpoint owns the completed installation and permissions account, workflow documentation, Black Tentacles launch binding, live pilot evidence, and setup acceptance report."
    ],
    "remaining_owner_review": [
      "Final acceptance of the completed workflow and its operational handoff, followed by the owner's separate decision about merge and use for Black Tentacles."
    ],
    "scope_reviewed": "Intermediate controller-core checkpoint for draft PR #194 at H, assessed against the pinned setup mission, protocol, 22 pending prior findings, and supplied candidate-bound evidence.",
    "unresolved_questions": []
  }
}
```
</details>

_Model-generated review by Codex using the requested model, posted by the agent-review controller through the owner's GitHub account. Not a human review or approval._
