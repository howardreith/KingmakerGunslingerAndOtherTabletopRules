<!-- agent-review:v1 enrollment=kmg-codex-agent-review-workflow-20261120170436 key=r03-review part=1/2 sha256=ed94ecd2848313c9d148b53f3c18c7496566954540b7d91d0c8e6644edfcc8d4 -->
_Part 1 of 2._

### Codex GPT-6.2 Astra — review — round 3 — candidate `dad2e808a5`

**Decision:** `changes_required` — The focused suite passed, and the candidate fixes several round-1 failures, including JSON null acceptance and reuse of a developer report across source commits. Controller-core still fails its exit conditions. A wrapper-configured project can push through the generic helper, a preserved verdict can survive a change to its validation rules, and a handoff can be marked current after its comparison or required checks change. These affect the workflow used by the pilot and subsequent missions.

#### Findings

**R1-F19** · P1 · defect · blocks readiness — Enforce the repository push wrapper at the helper entry point
- Location: `tools/agent_review/agent_review.py:2485`; acceptance: WF10
- Trigger: A developer uses the permitted installed push helper for a Kingmaker enrollment.
- Expected: Every authorized Kingmaker push uses the configured repository wrapper.
- Actual: The base settings grant the installed push helper to every enrollment. The CLI push handler always calls guarded_push, which runs git push directly, even when the Kingmaker project specifies its mandatory PowerShell wrapper.
- Why it blocks / does not: The shipped permission profile authorizes a second push route for the pilot repository.
- Requested correction: Have the helper enforce the project's selected push route and compose its permissions accordingly.
- Requested evidence: Exercise the developer-facing helper with a wrapper-configured fixture and show the wrapper is invoked or the generic operation is refused. Retain the successful no-wrapper case.

**R1-F14** · P1 · defect · blocks readiness — Apply the trusted-directory check to configuration names
- Location: `tools/agent_review/agent_review.py:2040`; acceptance: WF10
- Trigger: An extensionless project or plan argument contains path components.
- Expected: Both supported argument forms resolve to a file directly inside an approved configuration directory.
- Actual: _trusted validates the resolved parent only in the .json branch. Its name branch joins caller text onto each trusted directory and accepts any resulting file, including a path that resolves outside that directory. Project files control executable paths, permissions and push commands as well as model names.
- Why it blocks / does not: Pinning the model labels leaves operational authority selectable outside the trusted configuration roots.
- Requested correction: Validate bare names as identifiers and apply the same resolved-path containment check to every selected file.
- Requested evidence: Cover extensionless out-of-root references and linked files for both project and plan lookup.

**R1-F1** · P1 · defect · blocks readiness — Check freshness after a publication retry
- Location: `tools/agent_review/agent_review.py:1951`; acceptance: WF04, WF09
- Trigger: The branch changes while a valid review or final status is being published.
- Expected: The controller reconciles the latest comparison when committing the acceptance or handoff transition and marks a superseded publication historical.
- Actual: finish_round checks freshness before publish, which can spend minutes retrying, then advances without checking again. Finalize likewise checks readiness before final publication. update_pr_body subsequently reads the PR but ignores a changed headRefOid before writing the handoff and setting AWAITING_OWNER_REVIEW.
- Why it blocks / does not: A change observed during publication can still leave a current passing state for the previous candidate.
- Requested correction: Recheck the expected head and comparison at the end of publication, including the PR read used for the status update. Preserve any already-posted message with a visible correction if freshness changed.
- Requested evidence: Move the head or merge base from the GitHub publication boundary after the initial freshness check. Assert no current checkpoint pass or handoff survives.

**R1-F22** · P1 · defect · blocks readiness — Revalidate an existing handoff when its head stays fixed
- Location: `tools/agent_review/agent_review.py:1962`; acceptance: WF04, WF08
- Trigger: The target comparison or a required gate changes after final publication.
- Expected: Remote status and resume evaluate the candidate, PR identity and readiness gates relevant to the stored handoff.
- Actual: handoff_superseded checks only the PR head OID, draft flag and open state. A moved merge base, changed target branch, newly pending required check or requested reviewer can leave status --remote reporting handoff_current=true. Resume then refuses to reopen a handoff it considers current.
- Why it blocks / does not: The owner's explicit currency check can affirm a handoff whose readiness conditions have ceased to hold.
- Requested correction: Share the applicable identity, comparison and gate checks with handoff reconciliation. Distinguish a superseded handoff from an unavailable observation.
- Requested evidence: Keep A fixed, move M or change a required check, and verify remote status and resume agree that the handoff needs reconciliation.

**R1-F6** · P1 · defect · blocks readiness — Match hosted results to the actual required check
- Location: `tools/agent_review/agent_review.py:450`; acceptance: WF05, WF08
- Trigger: A protected branch requires a check from a particular integration, or its protection inventory is inaccessible.
- Expected: Each effective required check is resolved for H with its configured provider and current result. Ambiguous inventory stays unknown.
- Actual: Required checks are reduced to context names. Readiness accepts any completed success with that name on H, discarding the required app or integration identity. A success from one provider can satisfy a pending check from the required provider. The protection lookup also treats any HTTP 404 as verified absence.
- Why it blocks / does not: An unverified protection configuration or an unrelated check result can authorize readiness.
- Requested correction: Carry provider identity and current status through inventory and evaluation, and distinguish verified absence from inaccessible protection metadata.
- Requested evidence: Cover a pending required-provider check alongside a same-name success, an inaccessible inventory and a verified unprotected branch.

**R1-F3** · P1 · defect · blocks readiness — Reject a passing result that explicitly identifies a required gap
- Location: `tools/agent_review/agent_review.py:1372`; acceptance: WF05, WF08
- Trigger: Required work is disclosed in a finding but omitted from the review's separate required_gaps array.
- Expected: A review that explicitly identifies missing required work cannot pass. Required checkpoint gates remain required across submissions.
- Actual: semantic_problems accepts a passing decision containing a finding explicitly categorized required_gap when blocks_readiness is false and required_gaps is empty. Similarly, the report check only considers validation rows the developer chose to mark required.
- Why it blocks / does not: The semantic contradiction can pass through both schema validation and the readiness ledger.
- Requested correction: Reject contradictory required-gap classifications and check declared mandatory gates against the checkpoint's requirements where those gates are known.
- Requested evidence: Submit a passing result with a nonblocking required_gap and a report that omits or demotes a declared mandatory gate. Both should be refused.

**R1-F4** · P1 · defect · blocks readiness — Preserve the blocking status of a rejected fix
- Location: `tools/agent_review/agent_review.py:1433`; acceptance: WF08
- Trigger: A changes_required review restates a rejected blocking finding with a weaker flag.
- Expected: A rejected fix retains its outstanding requirement until a supported disposition resolves or explicitly reclassifies it.
- Actual: A rejected prior fix can be restated with blocks_readiness=false alongside a different blocking finding. finish_round overwrites the first entry's blocking flag. The next report can defer that still-unresolved item as optional, and the deferral now passes the ledger check.
- Why it blocks / does not: A required finding can reach a final disposition without verification of its fix or acceptance of a supported rebuttal.
- Requested correction: Validate changes to an open finding's blocking classification against its assessment before overwriting the ledger entry.
- Requested evidence: Exercise rejection and restatement across two real rounds and verify that an unresolved blocking item cannot subsequently be deferred through a flag change.

**R3-F1** · P1 · defect · blocks readiness — Bind resumed results to their content and workflow version
- Location: `tools/agent_review/agent_review.py:1414`; acceptance: WF05, WF07
- Trigger: Publication pauses after validation and the workflow or preserved envelope changes before resume.
- Expected: A resumed verdict retains its verified request, content, provenance and validation-rule identity.
- Actual: A preserved validated round is consumed by reading envelope_path and using its result directly. The controller does not compare the envelope's workflow fingerprint with the active workflow or verify its saved content against an accepted digest. activate changes the enrollment fingerprint without invalidating validated or ready results.
- Why it blocks / does not: A result validated under superseded rules, or a mismatched preserved envelope, can drive the current transition.
- Requested correction: Bind the preserved envelope to durable round state and reconcile validated and ready results on workflow activation. Require fresh validation or review when the applicable rules changed.
- Requested evidence: Pause with a validated result, activate changed validation rules, and resume. Also substitute a different fixture envelope. Neither case should consume the old verdict unchecked.

**R1-F13** · P1 · defect · blocks readiness — Verify every path excluded from the candidate diff
- Location: `tools/agent_review/agent_review.py:1280`; acceptance: WF09, WF10
- Trigger: A feature commit modifies transcript history before the reviewed source H is captured.
- Expected: Every transcript change omitted from review is proven to be an authorized, append-only plain record.
- Actual: write_review_inputs excludes the entire transcript tree from the feature diff, while verify_transcripts inspects only this PR's directory. Changes to another PR's historical records are invisible to both checks. For previously archived files in the current PR, pre-review verification compares blob content without checking the Git file mode.
- Why it blocks / does not: A candidate can acquire a valid source verdict while changing protected records outside the checked inventory.
- Requested correction: Verify the complete excluded M..H transcript delta, including path, operation and mode. Reject foreign alterations or expose them as material changes.
- Requested evidence: Before candidate capture, edit a previous PR's record and separately change only the mode of an archived record. Neither candidate should obtain the exemption.

**R3-F2** · P1 · defect · blocks readiness — Reserve the reviewer inbox before copying authoritative inputs
- Location: `tools/agent_review/agent_review.py:1298`; acceptance: WF10
- Trigger: The checked-out candidate already contains the proposed inbox path.
- Expected: Reviewer inputs occupy a fresh controller-owned directory with no candidate-controlled destination components.
- Actual: The input copier opens an existing .agent-review-inputs directory with exist_ok=True and copies files over its entries. The destination belongs to the candidate checkout until the controller establishes otherwise. A tracked collision changes the apparent snapshot, and filesystem links can redirect controller writes.
- Why it blocks / does not: The privileged input-preparation step must preserve the fixed candidate and contain its writes.
- Requested correction: Reserve the inbox namespace, refuse any pre-existing entry there and verify destination containment before copying.
- Requested evidence: Use a harmless tracked directory collision and a supported linked-path fixture. Input preparation must stop before overwriting anything.

**R1-F7** · P1 · defect · blocks readiness — Reject ambiguous or changed publication records
- Location: `tools/agent_review/agent_review.py:1050`; acceptance: WF06, WF10
- Trigger: Reconciliation finds one exact comment and another conflicting comment, or recorded content changes after posting.
- Expected: Each part has one unambiguous remote record matching the controller's recorded content.
- Actual: reconcile returns as soon as there is one exact body, even if another same-actor comment carries the same marker with different content. A locally posted publication is returned without checking the remote body at handoff. The outbox file is also reread without checking its stored digest before publication or archival.
- Why it blocks / does not: Conflicting or altered substantive records can be acknowledged as the authentic exchange.
- Requested correction: Require a unique matching marker and body, validate the outbox against its saved digest, and verify substantive records are still available when handing off.
- Requested evidence: Cover an exact comment plus a conflicting duplicate, an altered outbox part and an edited or removed posted review before finalization.

**R3-F3** · P2 · defect · blocks readiness — Make developer disposition delivery idempotent
- Location: `tools/agent_review/agent_review.py:1167`; acceptance: WF06, WF07
- Trigger: A foreground submission is replayed after interruption or corrected while its previous publication is pending.
- Expected: An identical submission reconciles once. A materially different submission has its own durable publication identity.
- Actual: publish_dispositions creates a new suffixed key whenever the previous key is posted. Replaying the same accepted report after a crash therefore posts it twice. If the previous key is pending, a different report reuses its old body while the new dispositions are saved to the ledger.
- Why it blocks / does not: The GitHub reply can be duplicated or disagree with the dispositions sent to the reviewer.
- Requested correction: Key the disposition operation by a persisted submission identity or digest, and recover the same ledger and publication transition on restart.
- Requested evidence: Replay an identical submission after posting, then submit a changed report while its predecessor is pending. Verify the comment bodies and ledger agree.

**R1-F12** · P1 · defect · blocks readiness — Recover partially written and just-committed archives
- Location: `tools/agent_review/agent_review.py:1796`; acceptance: WF06, WF07, WF09
- Trigger: The controller exits between two archive file writes or between Git commit creation and the next state save.
- Expected: Recovery reconstructs missing planned files from authenticated inputs and recognizes an already-created exact archive commit.
- Actual: Archive intent is saved before any files are written, but complete_archive refuses a missing planned file. A crash after git commit and before saving pending.commit is treated as an unexpected branch move. Neither state is repaired by resume.
- Why it blocks / does not: An ordinary interruption can permanently strand the archive despite intact publication bodies and a verifiable local commit.
- Requested correction: Persist recoverable file contents or references, complete verified partial writes, and recognize a matching child of the recorded parent before refusing a moved HEAD.
- Requested evidence: Interrupt after the first of several file writes and immediately after commit creation. Resume must produce one verified archive commit and one push.

**R1-F11** · P1 · defect · blocks readiness — Commit the stop outcome with the round
- Location: `tools/agent_review/agent_review.py:1470`; acceptance: WF07, WF08
- Trigger: The process exits immediately after saving a blocked round or a successful reviewer attempt.
- Expected: The durable transition includes its stop outcome, and a completed valid attempt can be recovered before another dispatch.
- Actual: finish_round saves acceptance, counters and the next phase, then raises the no-progress or reviewer-blocked Stop. The BLOCKED status is saved later by guarded. A restart between those writes sees ACTIVE and can dispatch another invocation. A successful attempt is also saved before its round is marked validated, leaving a window for a duplicate reviewer launch.
- Why it blocks / does not: Restart can lose a mandatory stop or rerun a successful review.
- Requested correction: Persist the stop with round acceptance and recover a completed validated attempt from its recorded envelope.
- Requested evidence: Interrupt at both state boundaries, reload through the normal command path and assert that the stop, review count and invocation count are preserved.

**R1-F8** · P1 · defect · blocks readiness — Keep writer exclusion independent of the selected state home
- Location: `tools/agent_review/agent_review.py:875`; acceptance: WF07
- Trigger: Two launches target the same branch using different workflow homes.
- Expected: The same physical worktree and enrolled branch share writer exclusion across controller configurations.
- Actual: Both registry and lock authority follow AGENT_REVIEW_HOME. Two controller processes using different values obtain independent registries and locks for the same physical worktree and PR. The target-validation test itself changes this variable to bypass the existing registry.
- Why it blocks / does not: The single-writer guarantee depends on callers choosing identical private-home configuration.
- Requested correction: Anchor production writer exclusion to a shared authoritative location or the physical repository/worktree as well as enrollment state. Keep test isolation explicit.
- Requested evidence: Use two processes with different state and home settings against the same temporary worktree. The second writer must be refused.

**R1-F9** · P1 · defect · blocks readiness — Refresh GitHub actor identity during long-running enrollments
- Location: `tools/agent_review/agent_review.py:886`; acceptance: WF10
- Trigger: The authenticated gh account changes after the first guard succeeds.
- Expected: The identity authorizing a mutation matches the account that will perform it.
- Actual: Controller.viewer caches the login for the lifetime of a headless loop. Later verify_inputs calls reuse it even though each gh subprocess uses the currently authenticated account. A changed account with repository access can publish later rounds.
- Why it blocks / does not: The pre-mutation guard can authorize GitHub writes using stale actor identity.
- Requested correction: Refresh actor identity at mutation boundaries or bind the GitHub client to a verified immutable authentication context.
- Requested evidence: Switch the simulated viewer between phases of one running controller. Subsequent publication and dispatch should stop.

**R1-F16** · P1 · defect · blocks readiness — Read developer effort from session metadata
- Location: `tools/agent_review/agent_review.py:612`; acceptance: WF01, WF05
- Trigger: An enrolled transcript contains an effort field outside the main developer's settings event.
- Expected: Observed effort comes from an identified CLI metadata field for the enrolled main session.
- Actual: session_provenance extracts effort by regex over entire raw JSONL lines before parsing event type or main-thread ownership. Structured tool arguments or sidechain events mentioning effort can supply or contradict the value attributed to the foreground developer.
- Why it blocks / does not: The accepted effort can come from content unrelated to the setting of the process that produced the report.
- Requested correction: Parse the documented or observed metadata event structure and scope it to the main invocation. Record unavailable effort explicitly.
- Requested evidence: Use raw transcript fixtures containing genuine settings, unrelated tool argument fields and sidechain settings. Only the authoritative field should affect provenance.

**R1-F17** · P1 · defect · blocks readiness — Require an explicit successful developer terminal event
- Location: `tools/agent_review/agent_review.py:729`; acceptance: WF05, WF08
- Trigger: The developer stream ends with structured output but incomplete or contradictory terminal metadata.
- Expected: Only an explicitly successful terminal result for the enrolled invocation supplies a checkpoint report.
- Actual: ClaudeDeveloper.parse assigns is_error=false when the result event omits is_error. develop_step accepts a dictionary output on exit zero without requiring a success subtype. A malformed or contradictory completion can therefore supply the accepted developer report.
- Why it blocks / does not: The controller has not established successful completion of the invocation to which it attributes the report.
- Requested correction: Validate the terminal event's required success fields and reject missing or contradictory outcomes before accepting structured_output.
- Requested evidence: Feed the actual parser a valid-shaped report with a missing success field and an error subtype. Verify neither reaches review.

**R1-F18** · P2 · defect · blocks readiness — Persist failures to decode required workflow inputs
- Location: `tools/agent_review/agent_review.py:2404`; acceptance: WF07, WF08
- Trigger: A required JSON response or preserved review artifact cannot be decoded.
- Expected: An unreadable external response or required state artifact produces a precise recoverable stop when the enrollment state is available.
- Actual: Malformed JSON from a GitHub response or persisted envelope raises ValueError. guarded catches command, OS and timeout failures but leaves this error uncaught. The process exits without persisting a stop reason and can leave the enrollment ACTIVE.
- Why it blocks / does not: A failed operational boundary can leave the controller's reported state inconsistent with the stopped process.
- Requested correction: Translate decoding and response-shape failures at the relevant boundaries to typed operational stops with the affected artifact or operation identified.
- Requested evidence: Return invalid JSON from a GitHub read and provide an unreadable pending envelope. Verify the durable status and absence of a verdict.

**R3-F4** · P1 · defect · blocks readiness — Check the runtime transaction guard at handoff
- Location: `tools/agent_review/agent_review.py:1866`; acceptance: WF08
- Trigger: The report claims restored or not applicable while the configured runtime lock still exists.
- Expected: An active or unresolved protected transaction prevents handoff until restoration is reconciled.
- Actual: readiness checks the report's runtime_restoration string but never consults runtime_active. The same controller already knows the Kingmaker lock path and uses it to protect developer termination.
- Why it blocks / does not: A handoff can claim readiness while the configured restoration guard still reports an active transaction.
- Requested correction: Check the configured runtime guard at readiness and surface any conflict with the developer's restoration report.
- Requested evidence: Use the temporary fixture lock with an otherwise ready report. Handoff should pause while it exists and succeed after verified cleanup.

**R3-F5** · P1 · defect · blocks readiness — Validate the generic helper's actual push destination
- Location: `tools/agent_review/agent_review.py:2087`; acceptance: WF10
- Trigger: A generic project's approved origin has a separate push URL.
- Expected: Every effective push destination is verified against the enrolled repository before any write.
- Actual: The generic helper validates remote get-url origin, which is the fetch destination, then pushes using origin's effective push destination. Git permits these destinations to differ. The post-push read of the approved fetch destination takes place after the write.
- Why it blocks / does not: A repository configuration difference can direct a write outside the enrolled repository.
- Requested correction: Resolve and validate the effective push destination or destinations before pushing. Refuse ambiguous or foreign targets.
- Requested evidence: Use two harmless local bare repositories as distinct fetch and push destinations. Verify the foreign repository receives no update.

**R3-F6** · P1 · defect · blocks readiness — Limit the developer transcript to evidence for the enrolled candidate
- Location: `tools/agent_review/agent_review.py:624`; acceptance: WF10
- Trigger: The foreground Claude session performs other machine-local work between review rounds.
- Expected: Reviewer inputs contain scoped validation observations with sensitive content removed before excerpts are formed.
- Actual: tool_transcript selects every main-session shell command after a timestamp. The supplied reviewer input includes an unrelated project's memory entry and machine-local scratch output. It also truncates command output before sanitization, which can remove the delimiters needed to recognize a sensitive block.
- Why it blocks / does not: The pilot already demonstrates that unrelated local material is transferred to the independent model as purported review evidence.
- Requested correction: Select candidate-relevant validation commands or receipts, sanitize before truncation, and retain sufficient identity, command and outcome information for independent inspection.
- Requested evidence: Mix a candidate test run with unrelated session activity and a synthetic sensitive multiline output. Only the scoped, sanitized evidence should reach the reviewer inbox.

#### Prior finding assessments

- **R1-F1** — `fix_rejected` — Freshness is recomputed on resumed validation and at the start of finalization. A move during publication can still be observed and then ignored before the passing transition.
- **R1-F2** — `fix_verified` — New rounds require last_report_for to match captured H and store their report. A material post-review change clears readiness and requires a new report.
- **R1-F3** — `fix_rejected` — Explicit required_gaps and required failing validation rows are rejected. An explicitly categorized required_gap can still pass when its blocking flag is false.
- **R1-F4** — `fix_rejected` — Membership, uniqueness and disposition compatibility are checked. Restating a rejected fix can still overwrite its blocking status and enable later deferral.
- **R1-F5** — `fix_verified` — Successful parsing is tracked separately from the parsed value. JSON null goes through object-schema validation, and the fixture asserts it earns no round.
- **R1-F6** — `fix_rejected` — Checks are queried on H, but provider identity is discarded and ambiguous protection lookup failures can be treated as verified absence.
- **R1-F7** — `fix_rejected` — A lone altered remote body is rejected. One exact body alongside a conflicting duplicate is accepted, and persisted body digests are not checked on reuse.
- **R1-F8** — `fix_rejected` — The OS lock fixes dead-holder cleanup and works across state_root values within one home. Different workflow homes independently enroll and lock the same worktree.
- **R1-F9** — `fix_rejected` — Archive and foreground resume now enter the shared guard. Actor verification inside that guard can reuse a login cached before a later authentication change.
- **R1-F10** — `fix_verified` — Foreground review and finalize check STOP before dispatch. The retry loop checks before each further reviewer attempt, and finalize checks at entry.
- **R1-F11** — `fix_rejected` — The canonical round reference and atomic counter update fix double accounting. The blocked outcome and successful-attempt promotion still cross separate durable writes.
- **R1-F12** — `fix_rejected` — Recovery covers an already-recorded commit with a failed push and a fully written set of records. Partial file creation and a commit made just before state persistence remain stranded.
- **R1-F13** — `fix_rejected` — The H-to-A exemption checks this PR's paths and digests. Candidate capture excludes a broader transcript tree than it authenticates.
- **R1-F14** — `fix_rejected` — Explicit JSON paths are checked and model labels are pinned. Extensionless lookup still accepts paths that resolve outside the trusted directories.
- **R1-F15** — `fix_verified` — Enrollment and pr_view compare the number, repository URL, head repository owner/name, cross-repository flag and branch pair. The target fixtures cover mismatched head ownership.
- **R1-F16** — `fix_rejected` — Foreground session binding and reviewer turn-effort comparisons are present. The foreground effort extractor still treats unscoped transcript fields as authoritative settings.
- **R1-F17** — `fix_rejected` — Launch charging, returned session ID and process exit checks are present. The terminal parser still accepts missing or contradictory success metadata.
- **R1-F18** — `fix_rejected` — CommandError, timeout and OS failures become durable stops. Decoding failures from operational inputs still escape the guard.
- **R1-F19** — `fix_rejected` — Several broad allowances were removed. The common permission base now grants a helper that bypasses the repository-specific push policy.
- **R1-F20** — `fix_verified` — Strict publication sanitization consumes complete PEM blocks and rejects unmatched opening markers. The test includes plain and JSON-escaped representations. R3-F6 concerns the separate reviewer-input path.
- **R1-F21** — `fix_verified` — Enrollment hashes the copied mission and verify_inputs checks the snapshot against the pin before work. The changed-snapshot fixture exercises refusal. The report overstates one detail: disappearance of the original alone is allowed while the verified snapshot survives.
- **R1-F22** — `fix_rejected` — A later PR-head push is detected and can reopen review. Changes to the comparison or required gates at an unchanged archive head remain undetected.
- **R1-F23** — `fix_verified` — The supplied CLI transcript now exposes the discovery command, successful output, commit, guarded push and subsequent receipt invocation for H. This supplies inspectable execution evidence for the existing suite.

#### Evidence assessment

- **Inspected:** Fixed request, mission, feature diff, changed-file list, developer report, developer tool transcript and all 23 pending ledger entries.; Repository instructions, validation policy, review protocol, controller, both schemas, role prompts, settings, project and plan configuration, behavior tests and opt-in smoke test.; The repository runtime coordination code identifying compatibility.lock as the transaction lock.
- **Executed verification:** Read-only Git inspection confirmed HEAD dad2e808a5b720d96f16560326e4327ed1b0a5e4 and merge base 2e61e78097623d984630eadab873d8d87417f2c3. The supplied mission file has the requested SHA-256.; The developer CLI transcript records unittest discovery reporting 46 tests in 74.730 seconds, OK with one skip, immediately before the candidate commit. It also records the guarded push of the exact H and a subsequent detached-worktree receipt invocation reporting OK with one skip for H.; The transcript records a real dirty-worktree refusal during the previous submission. The earlier sanitizer failure and the dirty-worktree refusal produced no completed review.
- **Source inspection only:** The failure paths in the findings were traced through production entry points and compared with the test fixtures. They were not executed by the reviewer.; The later setup acceptance report, installation and Black Tentacles handoff were inspected for context only.
- **Required gaps:** Candidate-bound focused regression results for the publication, recovery, identity, permission and readiness cases identified below.
- **Limitations:** The reviewer used read-only inspection. The executed suite evidence comes from the developer CLI transcript. The focused discovery run skipped the opt-in live smoke.; The receipt script prints the developer worktree's H and does not independently assert the detached worktree's HEAD after creating it. The transcript supports the reported run but does not expose its complete per-test receipt.; External model and GitHub failure responses in the focused suite are simulated. The separately cited live smoke and preflight are outside the supplied candidate-bound transcript.

#### Summary

- **Delivered behavior:** The candidate implements detached review snapshots, explicit role models, schema validation, candidate-bound developer reports, OS-backed locking, PR publication, archive inventory and a real-Git behavior suite. The suite confirms several correction paths. The outstanding findings affect authorization, recovery and the conditions under which the controller can claim a current pass.
- **Scope reviewed:** Intermediate controller-core checkpoint for PR #194 at the fixed H, including production controller entry points, model and permission configuration, schemas, prompts, focused tests and the recorded correction evidence.
- **Pending later-phase work:** At setup-final, assess the complete workflow guide, effective installation and permissions, real pilot exchange, Black Tentacles binding and completed setup acceptance report.
- **Unresolved questions:** none
- **Remaining owner review:** After a valid final checkpoint, the owner decides whether to adopt the workflow and whether to merge or release it.

#### Provenance (controller-recorded)

| | Requested | Observed |
|---|---|---|
| Model | `gpt-6.2-astra` | `gpt-6.2-astra` (turns ['gpt-6.2-astra']) |
| Reasoning effort | `high` | `high` (turns ['high']) |
| Sandbox / approvals | read-only / never | `read-only` / `never` |

Codex CLI `0.170.0`, thread `01a1fb32-a976-7371-baa9-940ced5bcb01`, duration 301.8s, response sha256 `b0c61c76fcdce5542247333bd03cf0315cf0eba313e07ebd61146867a93b39c0`, request `kmg-codex-agent-review-workflow-20261120170436-r03-d0c2ae`. Observed values come from the Codex session rollout's recorded thread settings, not from the model's own statements; the serving backend's model identity is not separately exposed.

<details><summary>Complete structured review (agent-review.review.v1)</summary>

```json
{
  "candidate_commit": "dad2e808a5b720d96f16560326e4327ed1b0a5e4",
  "checkpoint_id": "controller-core",
  "checkpoint_type": "intermediate",
  "decision": "changes_required",
  "decision_rationale": "The focused suite passed, and the candidate fixes several round-1 failures, including JSON null acceptance and reuse of a developer report across source commits. Controller-core still fails its exit conditions. A wrapper-configured project can push through the generic helper, a preserved verdict can survive a change to its validation rules, and a handoff can be marked current after its comparison or required checks change. These affect the workflow used by the pilot and subsequent missions.",
  "evidence_assessment": {
    "executed_verification": [
      "Read-only Git inspection confirmed HEAD dad2e808a5b720d96f16560326e4327ed1b0a5e4 and merge base 2e61e78097623d984630eadab873d8d87417f2c3. The supplied mission file has the requested SHA-256.",
      "The developer CLI transcript records unittest discovery reporting 46 tests in 74.730 seconds, OK with one skip, immediately before the candidate commit. It also records the guarded push of the exact H and a subsequent detached-worktree receipt invocation reporting OK with one skip for H.",
      "The transcript records a real dirty-worktree refusal during the previous submission. The earlier sanitizer failure and the dirty-worktree refusal produced no completed review."
    ],
    "inspected": [
      "Fixed request, mission, feature diff, changed-file list, developer report, developer tool transcript and all 23 pending ledger entries.",
      "Repository instructions, validation policy, review protocol, controller, both schemas, role prompts, settings, project and plan configuration, behavior tests and opt-in smoke test.",
      "The repository runtime coordination code identifying compatibility.lock as the transaction lock."
    ],
    "limitations": [
      "The reviewer used read-only inspection. The executed suite evidence comes from the developer CLI transcript. The focused discovery run skipped the opt-in live smoke.",
      "The receipt script prints the developer worktree's H and does not independently assert the detached worktree's HEAD after creating it. The transcript supports the reported run but does not expose its complete per-test receipt.",
      "External model and GitHub failure responses in the focused suite are simulated. The separately cited live smoke and preflight are outside the supplied candidate-bound transcript."
    ],
    "required_gaps": [
      "Candidate-bound focused regression results for the publication, recovery, identity, permission and readiness cases identified below."
    ],
    "source_inspection_only": [
      "The failure paths in the findings were traced through production entry points and compared with the test fixtures. They were not executed by the reviewer.",
      "The later setup acceptance report, installation and Black Tentacles handoff were inspected for context only."
    ]
  },
  "findings": [
    {
      "acceptance_ids": [
        "WF10"
      ],
      "actual": "The base settings grant the installed push helper to every enrollment. The CLI push handler always calls guarded_push, which runs git push directly, even when the Kingmaker project specifies its mandatory PowerShell wrapper.",
      "blocking_rationale": "The shipped permission profile authorizes a second push route for the pilot repository.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Every authorized Kingmaker push uses the configured repository wrapper.",
      "id": "R1-F19",
      "location": {
        "file": "tools/agent_review/agent_review.py",
        "line": 2485
      },
      "priority": "P1",
      "requested_correction": "Have the helper enforce the project's selected push route and compose its permissions accordingly.",
      "requested_evidence": "Exercise the developer-facing helper with a wrapper-configured fixture and show the wrapper is invoked or the generic operation is refused. Retain the successful no-wrapper case.",
      "title": "Enforce the repository push wrapper at the helper entry point",
      "trigger": "A developer uses the permitted installed push helper for a Kingmaker enrollment."
    },
    {
      "acceptance_ids": [
        "WF10"
      ],
      "actual": "_trusted validates the resolved parent only in the .json branch. Its name branch joins caller text onto each trusted directory and accepts any resulting file, including a path that resolves outside that directory. Project files control executable paths, permissions and push commands as well as model names.",
      "blocking_rationale": "Pinning the model labels leaves operational authority selectable outside the trusted configuration roots.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Both supported argument forms resolve to a file directly inside an approved configuration directory.",
      "id": "R1-F14",
      "location": {
        "file": "tools/agent_review/agent_review.py",
        "line": 2040
      },
      "priority": "P1",
      "requested_correction": "Validate bare names as identifiers and apply the same resolved-path containment check to every selected file.",
      "requested_evidence": "Cover extensionless out-of-root references and linked files for both project and plan lookup.",
      "title": "Apply the trusted-directory check to configuration names",
      "trigger": "An extensionless project or plan argument contains path components."
    },
    {
      "acceptance_ids": [
        "WF04",
        "WF09"
      ],
      "actual": "finish_round checks freshness before publish, which can spend minutes retrying, then advances without checking again. Finalize likewise checks readiness before final publication. update_pr_body subsequently reads the PR but ignores a changed headRefOid before writing the handoff and setting AWAITING_OWNER_REVIEW.",
      "blocking_rationale": "A change observed during publication can still leave a current passing state for the previous candidate.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "The controller reconciles the latest comparison when committing the acceptance or handoff transition and marks a superseded publication historical.",
      "id": "R1-F1",
      "location": {
        "file": "tools/agent_review/agent_review.py",
        "line": 1951
      },
      "priority": "P1",
      "requested_correction": "Recheck the expected head and comparison at the end of publication, including the PR read used for the status update. Preserve any already-posted message with a visible correction if freshness changed.",
      "requested_evidence": "Move the head or merge base from the GitHub publication boundary after the initial freshness check. Assert no current checkpoint pass or handoff survives.",
      "title": "Check freshness after a publication retry",
      "trigger": "The branch changes while a valid review or final status is being published."
    },
    {
      "acceptance_ids": [
        "WF04",
        "WF08"
      ],
      "actual": "handoff_superseded checks only the PR head OID, draft flag and open state. A moved merge base, changed target branch, newly pending required check or requested reviewer can leave status --remote reporting handoff_current=true. Resume then refuses to reopen a handoff it considers current.",
      "blocking_rationale": "The owner's explicit currency check can affirm a handoff whose readiness conditions have ceased to hold.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Remote status and resume evaluate the candidate, PR identity and readiness gates relevant to the stored handoff.",
      "id": "R1-F22",
      "location": {
        "file": "tools/agent_review/agent_review.py",
        "line": 1962
      },
      "priority": "P1",
      "requested_correction": "Share the applicable identity, comparison and gate checks with handoff reconciliation. Distinguish a superseded handoff from an unavailable observation.",
      "requested_evidence": "Keep A fixed, move M or change a required check, and verify remote status and resume agree that the handoff needs reconciliation.",
      "title": "Revalidate an existing handoff when its head stays fixed",
      "trigger": "The target comparison or a required gate changes after final publication."
    },
    {
      "acceptance_ids": [
        "WF05",
        "WF08"
      ],
      "actual": "Required checks are reduced to context names. Readiness accepts any completed success with that name on H, discarding the required app or integration identity. A success from one provider can satisfy a pending check from the required provider. The protection lookup also treats any HTTP 404 as verified absence.",
      "blocking_rationale": "An unverified protection configuration or an unrelated check result can authorize readiness.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Each effective required check is resolved for H with its configured provider and current result. Ambiguous inventory stays unknown.",
      "id": "R1-F6",
      "location": {
        "file": "tools/agent_review/agent_review.py",
        "line": 450
      },
      "priority": "P1",
      "requested_correction": "Carry provider identity and current status through inventory and evaluation, and distinguish verified absence from inaccessible protection metadata.",
      "requested_evidence": "Cover a pending required-provider check alongside a same-name success, an inaccessible inventory and a verified unprotected branch.",
      "title": "Match hosted results to the actual required check",
      "trigger": "A protected branch requires a check from a particular integration, or its protection inventory is inaccessible."
    },
    {
      "acceptance_ids": [
        "WF05",
        "WF08"
      ],
      "actual": "semantic_problems accepts a passing decision containing a finding explicitly categorized required_gap when blocks_readiness is false and required_gaps is empty. Similarly, the report check only considers validation rows the developer chose to mark required.",
      "blocking_rationale": "The semantic contradiction can pass through both schema validation and the readiness ledger.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "A review that explicitly identifies missing required work cannot pass. Required checkpoint gates remain required across submissions.",
      "id": "R1-F3",
      "location": {
        "file": "tools/agent_review/agent_review.py",
        "line": 1372
      },
      "priority": "P1",
      "requested_correction": "Reject contradictory required-gap classifications and check declared mandatory gates against the checkpoint's requirements where those gates are known.",
      "requested_evidence": "Submit a passing result with a nonblocking required_gap and a report that omits or demotes a declared mandatory gate. Both should be refused.",
      "title": "Reject a passing result that explicitly identifies a required gap",
      "trigger": "Required work is disclosed in a finding but omitted from the review's separate required_gaps array."
    },
    {
      "acceptance_ids": [
        "WF08"
      ],
      "actual": "A rejected prior fix can be restated with blocks_readiness=false alongside a different blocking finding. finish_round overwrites the first entry's blocking flag. The next report can defer that still-unresolved item as optional, and the deferral now passes the ledger check.",
      "blocking_rationale": "A required finding can reach a final disposition without verification of its fix or acceptance of a supported rebuttal.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "A rejected fix retains its outstanding requirement until a supported disposition resolves or explicitly reclassifies it.",
      "id": "R1-F4",
      "location": {
        "file": "tools/agent_review/agent_review.py",
        "line": 1433
      },
      "priority": "P1",
      "requested_correction": "Validate changes to an open finding's blocking classification against its assessment before overwriting the ledger entry.",
      "requested_evidence": "Exercise rejection and restatement across two real rounds and verify that an unresolved blocking item cannot subsequently be deferred through a flag change.",
      "title": "Preserve the blocking status of a rejected fix",
      "trigger": "A changes_required review restates a rejected blocking finding with a weaker flag."
    },
    {
      "acceptance_ids": [
        "WF05",
        "WF07"
      ],
      "actual": "A preserved validated round is consumed by reading envelope_path and using its result directly. The controller does not compare the envelope's workflow fingerprint with the active workflow or verify its saved content against an accepted digest. activate changes the enrollment fingerprint without invalidating validated or ready results.",
      "blocking_rationale": "A result validated under superseded rules, or a mismatched preserved envelope, can drive the current transition.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "A resumed verdict retains its verified request, content, provenance and validation-rule identity.",
      "id": "R3-F1",
      "location": {
        "file": "tools/agent_review/agent_review.py",
        "line": 1414
      },
      "priority": "P1",
      "requested_correction": "Bind the preserved envelope to durable round state and reconcile validated and ready results on workflow activation. Require fresh validation or review when the applicable rules changed.",
      "requested_evidence": "Pause with a validated result, activate changed validation rules, and resume. Also substitute a different fixture envelope. Neither case should consume the old verdict unchecked.",
      "title": "Bind resumed results to their content and workflow version",
      "trigger": "Publication pauses after validation and the workflow or preserved envelope changes before resume."
    },
    {
      "acceptance_ids": [
        "WF09",
        "WF10"
      ],
      "actual": "write_review_inputs excludes the entire transcript tree from the feature diff, while verify_transcripts inspects only this PR's directory. Changes to another PR's historical records are invisible to both checks. For previously archived files in the current PR, pre-review verification compares blob content without checking the Git file mode.",
      "blocking_rationale": "A candidate can acquire a valid source verdict while changing protected records outside the checked inventory.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Every transcript change omitted from review is proven to be an authorized, append-only plain record.",
      "id": "R1-F13",
      "location": {
        "file": "tools/agent_review/agent_review.py",
        "line": 1280
      },
      "priority": "P1",
      "requested_correction": "Verify the complete excluded M..H transcript delta, including path, operation and mode. Reject foreign alterations or expose them as material changes.",
      "requested_evidence": "Before candidate capture, edit a previous PR's record and separately change only the mode of an archived record. Neither candidate should obtain the exemption.",
      "title": "Verify every path excluded from the candidate diff",
      "trigger": "A feature commit modifies transcript history before the reviewed source H is captured."
    },
    {
      "acceptance_ids": [
        "WF10"
      ],
      "actual": "The input copier opens an existing .agent-review-inputs directory with exist_ok=True and copies files over its entries. The destination belongs to the candidate checkout until the controller establishes otherwise. A tracked collision changes the apparent snapshot, and filesystem links can redirect controller writes.",
      "blocking_rationale": "The privileged input-preparation step must preserve the fixed candidate and contain its writes.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Reviewer inputs occupy a fresh controller-owned directory with no candidate-controlled destination components.",
      "id": "R3-F2",
      "location": {
        "file": "tools/agent_review/agent_review.py",
        "line": 1298
      },
      "priority": "P1",
      "requested_correction": "Reserve the inbox namespace, refuse any pre-existing entry there and verify destination containment before copying.",
      "requested_evidence": "Use a harmless tracked directory collision and a supported linked-path fixture. Input preparation must stop before overwriting anything.",
      "title": "Reserve the reviewer inbox before copying authoritative inputs",
      "trigger": "The checked-out candidate already contains the proposed inbox path."
    },
    {
      "acceptance_ids": [
        "WF06",
        "WF10"
      ],
      "actual": "reconcile returns as soon as there is one exact body, even if another same-actor comment carries the same marker with different content. A locally posted publication is returned without checking the remote body at handoff. The outbox file is also reread without checking its stored digest before publication or archival.",
      "blocking_rationale": "Conflicting or altered substantive records can be acknowledged as the authentic exchange.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Each part has one unambiguous remote record matching the controller's recorded content.",
      "id": "R1-F7",
      "location": {
        "file": "tools/agent_review/agent_review.py",
        "line": 1050
      },
      "priority": "P1",
      "requested_correction": "Require a unique matching marker and body, validate the outbox against its saved digest, and verify substantive records are still available when handing off.",
      "requested_evidence": "Cover an exact comment plus a conflicting duplicate, an altered outbox part and an edited or removed posted review before finalization.",
      "title": "Reject ambiguous or changed publication records",
      "trigger": "Reconciliation finds one exact comment and another conflicting comment, or recorded content changes after posting."
    },
    {
      "acceptance_ids": [
        "WF06",
        "WF07"
      ],
      "actual": "publish_dispositions creates a new suffixed key whenever the previous key is posted. Replaying the same accepted report after a crash therefore posts it twice. If the previous key is pending, a different report reuses its old body while the new dispositions are saved to the ledger.",
      "blocking_rationale": "The GitHub reply can be duplicated or disagree with the dispositions sent to the reviewer.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "An identical submission reconciles once. A materially different submission has its own durable publication identity.",
      "id": "R3-F3",
      "location": {
        "file": "tools/agent_review/agent_review.py",
        "line": 1167
      },
      "priority": "P2",
      "requested_correction": "Key the disposition operation by a persisted submission identity or digest, and recover the same ledger and publication transition on restart.",
      "requested_evidence": "Replay an identical submission after posting, then submit a changed report while its predecessor is pending. Verify the comment bodies and ledger agree.",
      "title": "Make developer disposition delivery idempotent",
      "trigger": "A foreground submission is replayed after interruption or corrected while its previous publication is pending."
    },
    {
      "acceptance_ids": [
        "WF06",
        "WF07",
        "WF09"
      ],
      "actual": "Archive intent is saved before any files are written, but complete_archive refuses a missing planned file. A crash after git commit and before saving pending.commit is treated as an unexpected branch move. Neither state is repaired by resume.",
      "blocking_rationale": "An ordinary interruption can permanently strand the archive despite intact publication bodies and a verifiable local commit.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Recovery reconstructs missing planned files from authenticated inputs and recognizes an already-created exact archive commit.",
      "id": "R1-F12",
      "location": {
        "file": "tools/agent_review/agent_review.py",
        "line": 1796
      },
      "priority": "P1",
      "requested_correction": "Persist recoverable file contents or references, complete verified partial writes, and recognize a matching child of the recorded parent before refusing a moved HEAD.",
      "requested_evidence": "Interrupt after the first of several file writes and immediately after commit creation. Resume must produce one verified archive commit and one push.",
      "title": "Recover partially written and just-committed archives",
      "trigger": "The controller exits between two archive file writes or between Git commit creation and the next state save."
    },
    {
      "acceptance_ids": [
        "WF07",
        "WF08"
      ],
      "actual": "finish_round saves acceptance, counters and the next phase, then raises the no-progress or reviewer-blocked Stop. The BLOCKED status is saved later by guarded. A restart between those writes sees ACTIVE and can dispatch another invocation. A successful attempt is also saved before its round is marked validated, leaving a window for a duplicate reviewer launch.",
      "blocking_rationale": "Restart can lose a mandatory stop or rerun a successful review.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "The durable transition includes its stop outcome, and a completed valid attempt can be recovered before another dispatch.",
      "id": "R1-F11",
      "location": {
        "file": "tools/agent_review/agent_review.py",
        "line": 1470
      },
      "priority": "P1",
      "requested_correction": "Persist the stop with round acceptance and recover a completed validated attempt from its recorded envelope.",
      "requested_evidence": "Interrupt at both state boundaries, reload through the normal command path and assert that the stop, review count and invocation count are preserved.",
      "title": "Commit the stop outcome with the round",
      "trigger": "The process exits immediately after saving a blocked round or a successful reviewer attempt."
    },
    {
      "acceptance_ids": [
        "WF07"
      ],
      "actual": "Both registry and lock authority follow AGENT_REVIEW_HOME. Two controller processes using different values obtain independent registries and locks for the same physical worktree and PR. The target-validation test itself changes this variable to bypass the existing registry.",
      "blocking_rationale": "The single-writer guarantee depends on callers choosing identical private-home configuration.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "The same physical worktree and enrolled branch share writer exclusion across controller configurations.",
      "id": "R1-F8",
      "location": {
        "file": "tools/agent_review/agent_review.py",
        "line": 875
      },
      "priority": "P1",
      "requested_correction": "Anchor production writer exclusion to a shared authoritative location or the physical repository/worktree as well as enrollment state. Keep test isolation explicit.",
      "requested_evidence": "Use two processes with different state and home settings against the same temporary worktree. The second writer must be refused.",
      "title": "Keep writer exclusion independent of the selected state home",
      "trigger": "Two launches target the same branch using different workflow homes."
    },
    {
      "acceptance_ids": [
        "WF10"
      ],
      "actual": "Controller.viewer caches the login for the lifetime of a headless loop. Later verify_inputs calls reuse it even though each gh subprocess uses the currently authenticated account. A changed account with repository access can publish later rounds.",
      "blocking_rationale": "The pre-mutation guard can authorize GitHub writes using stale actor identity.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "The identity authorizing a mutation matches the account that will perform it.",
      "id": "R1-F9",
      "location": {
        "file": "tools/agent_review/agent_review.py",
        "line": 886
      },
      "priority": "P1",
      "requested_correction": "Refresh actor identity at mutation boundaries or bind the GitHub client to a verified immutable authentication context.",
      "requested_evidence": "Switch the simulated viewer between phases of one running controller. Subsequent publication and dispatch should stop.",
      "title": "Refresh GitHub actor identity during long-running enrollments",
      "trigger": "The authenticated gh account changes after the first guard succeeds."
    },
    {
      "acceptance_ids": [
        "WF01",
        "WF05"
      ],
      "actual": "session_provenance extracts effort by regex over entire raw JSONL lines before parsing event t

---

<!-- agent-review:v1 enrollment=kmg-codex-agent-review-workflow-20261120170436 key=r03-review part=2/2 sha256=cc14c6da38461afd28b03466b24f4b9e43a18f941b5213df20701e2b229ea96d -->
_Part 2 of 2._

ype or main-thread ownership. Structured tool arguments or sidechain events mentioning effort can supply or contradict the value attributed to the foreground developer.",
      "blocking_rationale": "The accepted effort can come from content unrelated to the setting of the process that produced the report.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Observed effort comes from an identified CLI metadata field for the enrolled main session.",
      "id": "R1-F16",
      "location": {
        "file": "tools/agent_review/agent_review.py",
        "line": 612
      },
      "priority": "P1",
      "requested_correction": "Parse the documented or observed metadata event structure and scope it to the main invocation. Record unavailable effort explicitly.",
      "requested_evidence": "Use raw transcript fixtures containing genuine settings, unrelated tool argument fields and sidechain settings. Only the authoritative field should affect provenance.",
      "title": "Read developer effort from session metadata",
      "trigger": "An enrolled transcript contains an effort field outside the main developer's settings event."
    },
    {
      "acceptance_ids": [
        "WF05",
        "WF08"
      ],
      "actual": "ClaudeDeveloper.parse assigns is_error=false when the result event omits is_error. develop_step accepts a dictionary output on exit zero without requiring a success subtype. A malformed or contradictory completion can therefore supply the accepted developer report.",
      "blocking_rationale": "The controller has not established successful completion of the invocation to which it attributes the report.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Only an explicitly successful terminal result for the enrolled invocation supplies a checkpoint report.",
      "id": "R1-F17",
      "location": {
        "file": "tools/agent_review/agent_review.py",
        "line": 729
      },
      "priority": "P1",
      "requested_correction": "Validate the terminal event's required success fields and reject missing or contradictory outcomes before accepting structured_output.",
      "requested_evidence": "Feed the actual parser a valid-shaped report with a missing success field and an error subtype. Verify neither reaches review.",
      "title": "Require an explicit successful developer terminal event",
      "trigger": "The developer stream ends with structured output but incomplete or contradictory terminal metadata."
    },
    {
      "acceptance_ids": [
        "WF07",
        "WF08"
      ],
      "actual": "Malformed JSON from a GitHub response or persisted envelope raises ValueError. guarded catches command, OS and timeout failures but leaves this error uncaught. The process exits without persisting a stop reason and can leave the enrollment ACTIVE.",
      "blocking_rationale": "A failed operational boundary can leave the controller's reported state inconsistent with the stopped process.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "An unreadable external response or required state artifact produces a precise recoverable stop when the enrollment state is available.",
      "id": "R1-F18",
      "location": {
        "file": "tools/agent_review/agent_review.py",
        "line": 2404
      },
      "priority": "P2",
      "requested_correction": "Translate decoding and response-shape failures at the relevant boundaries to typed operational stops with the affected artifact or operation identified.",
      "requested_evidence": "Return invalid JSON from a GitHub read and provide an unreadable pending envelope. Verify the durable status and absence of a verdict.",
      "title": "Persist failures to decode required workflow inputs",
      "trigger": "A required JSON response or preserved review artifact cannot be decoded."
    },
    {
      "acceptance_ids": [
        "WF08"
      ],
      "actual": "readiness checks the report's runtime_restoration string but never consults runtime_active. The same controller already knows the Kingmaker lock path and uses it to protect developer termination.",
      "blocking_rationale": "A handoff can claim readiness while the configured restoration guard still reports an active transaction.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "An active or unresolved protected transaction prevents handoff until restoration is reconciled.",
      "id": "R3-F4",
      "location": {
        "file": "tools/agent_review/agent_review.py",
        "line": 1866
      },
      "priority": "P1",
      "requested_correction": "Check the configured runtime guard at readiness and surface any conflict with the developer's restoration report.",
      "requested_evidence": "Use the temporary fixture lock with an otherwise ready report. Handoff should pause while it exists and succeed after verified cleanup.",
      "title": "Check the runtime transaction guard at handoff",
      "trigger": "The report claims restored or not applicable while the configured runtime lock still exists."
    },
    {
      "acceptance_ids": [
        "WF10"
      ],
      "actual": "The generic helper validates remote get-url origin, which is the fetch destination, then pushes using origin's effective push destination. Git permits these destinations to differ. The post-push read of the approved fetch destination takes place after the write.",
      "blocking_rationale": "A repository configuration difference can direct a write outside the enrolled repository.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Every effective push destination is verified against the enrolled repository before any write.",
      "id": "R3-F5",
      "location": {
        "file": "tools/agent_review/agent_review.py",
        "line": 2087
      },
      "priority": "P1",
      "requested_correction": "Resolve and validate the effective push destination or destinations before pushing. Refuse ambiguous or foreign targets.",
      "requested_evidence": "Use two harmless local bare repositories as distinct fetch and push destinations. Verify the foreign repository receives no update.",
      "title": "Validate the generic helper's actual push destination",
      "trigger": "A generic project's approved origin has a separate push URL."
    },
    {
      "acceptance_ids": [
        "WF10"
      ],
      "actual": "tool_transcript selects every main-session shell command after a timestamp. The supplied reviewer input includes an unrelated project's memory entry and machine-local scratch output. It also truncates command output before sanitization, which can remove the delimiters needed to recognize a sensitive block.",
      "blocking_rationale": "The pilot already demonstrates that unrelated local material is transferred to the independent model as purported review evidence.",
      "blocks_readiness": true,
      "category": "defect",
      "expected": "Reviewer inputs contain scoped validation observations with sensitive content removed before excerpts are formed.",
      "id": "R3-F6",
      "location": {
        "file": "tools/agent_review/agent_review.py",
        "line": 624
      },
      "priority": "P1",
      "requested_correction": "Select candidate-relevant validation commands or receipts, sanitize before truncation, and retain sufficient identity, command and outcome information for independent inspection.",
      "requested_evidence": "Mix a candidate test run with unrelated session activity and a synthetic sensitive multiline output. Only the scoped, sanitized evidence should reach the reviewer inbox.",
      "title": "Limit the developer transcript to evidence for the enrolled candidate",
      "trigger": "The foreground Claude session performs other machine-local work between review rounds."
    }
  ],
  "prior_finding_assessments": [
    {
      "assessment": "fix_rejected",
      "finding_id": "R1-F1",
      "rationale": "Freshness is recomputed on resumed validation and at the start of finalization. A move during publication can still be observed and then ignored before the passing transition."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R1-F2",
      "rationale": "New rounds require last_report_for to match captured H and store their report. A material post-review change clears readiness and requires a new report."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R1-F3",
      "rationale": "Explicit required_gaps and required failing validation rows are rejected. An explicitly categorized required_gap can still pass when its blocking flag is false."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R1-F4",
      "rationale": "Membership, uniqueness and disposition compatibility are checked. Restating a rejected fix can still overwrite its blocking status and enable later deferral."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R1-F5",
      "rationale": "Successful parsing is tracked separately from the parsed value. JSON null goes through object-schema validation, and the fixture asserts it earns no round."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R1-F6",
      "rationale": "Checks are queried on H, but provider identity is discarded and ambiguous protection lookup failures can be treated as verified absence."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R1-F7",
      "rationale": "A lone altered remote body is rejected. One exact body alongside a conflicting duplicate is accepted, and persisted body digests are not checked on reuse."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R1-F8",
      "rationale": "The OS lock fixes dead-holder cleanup and works across state_root values within one home. Different workflow homes independently enroll and lock the same worktree."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R1-F9",
      "rationale": "Archive and foreground resume now enter the shared guard. Actor verification inside that guard can reuse a login cached before a later authentication change."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R1-F10",
      "rationale": "Foreground review and finalize check STOP before dispatch. The retry loop checks before each further reviewer attempt, and finalize checks at entry."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R1-F11",
      "rationale": "The canonical round reference and atomic counter update fix double accounting. The blocked outcome and successful-attempt promotion still cross separate durable writes."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R1-F12",
      "rationale": "Recovery covers an already-recorded commit with a failed push and a fully written set of records. Partial file creation and a commit made just before state persistence remain stranded."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R1-F13",
      "rationale": "The H-to-A exemption checks this PR's paths and digests. Candidate capture excludes a broader transcript tree than it authenticates."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R1-F14",
      "rationale": "Explicit JSON paths are checked and model labels are pinned. Extensionless lookup still accepts paths that resolve outside the trusted directories."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R1-F15",
      "rationale": "Enrollment and pr_view compare the number, repository URL, head repository owner/name, cross-repository flag and branch pair. The target fixtures cover mismatched head ownership."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R1-F16",
      "rationale": "Foreground session binding and reviewer turn-effort comparisons are present. The foreground effort extractor still treats unscoped transcript fields as authoritative settings."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R1-F17",
      "rationale": "Launch charging, returned session ID and process exit checks are present. The terminal parser still accepts missing or contradictory success metadata."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R1-F18",
      "rationale": "CommandError, timeout and OS failures become durable stops. Decoding failures from operational inputs still escape the guard."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R1-F19",
      "rationale": "Several broad allowances were removed. The common permission base now grants a helper that bypasses the repository-specific push policy."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R1-F20",
      "rationale": "Strict publication sanitization consumes complete PEM blocks and rejects unmatched opening markers. The test includes plain and JSON-escaped representations. R3-F6 concerns the separate reviewer-input path."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R1-F21",
      "rationale": "Enrollment hashes the copied mission and verify_inputs checks the snapshot against the pin before work. The changed-snapshot fixture exercises refusal. The report overstates one detail: disappearance of the original alone is allowed while the verified snapshot survives."
    },
    {
      "assessment": "fix_rejected",
      "finding_id": "R1-F22",
      "rationale": "A later PR-head push is detected and can reopen review. Changes to the comparison or required gates at an unchanged archive head remain undetected."
    },
    {
      "assessment": "fix_verified",
      "finding_id": "R1-F23",
      "rationale": "The supplied CLI transcript now exposes the discovery command, successful output, commit, guarded push and subsequent receipt invocation for H. This supplies inspectable execution evidence for the existing suite."
    }
  ],
  "request_id": "kmg-codex-agent-review-workflow-20261120170436-r03-d0c2ae",
  "round": 3,
  "schema_version": "agent-review.review.v1",
  "summary": {
    "delivered_behavior": "The candidate implements detached review snapshots, explicit role models, schema validation, candidate-bound developer reports, OS-backed locking, PR publication, archive inventory and a real-Git behavior suite. The suite confirms several correction paths. The outstanding findings affect authorization, recovery and the conditions under which the controller can claim a current pass.",
    "pending_later_phase_work": [
      "At setup-final, assess the complete workflow guide, effective installation and permissions, real pilot exchange, Black Tentacles binding and completed setup acceptance report."
    ],
    "remaining_owner_review": [
      "After a valid final checkpoint, the owner decides whether to adopt the workflow and whether to merge or release it."
    ],
    "scope_reviewed": "Intermediate controller-core checkpoint for PR #194 at the fixed H, including production controller entry points, model and permission configuration, schemas, prompts, focused tests and the recorded correction evidence.",
    "unresolved_questions": []
  }
}
```
</details>

_Model-generated review by Codex using the requested model, posted by the agent-review controller through the owner's GitHub account. Not a human review or approval._
