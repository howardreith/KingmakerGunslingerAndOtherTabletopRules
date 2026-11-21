<!-- agent-review:v1 enrollment=kmg-codex-agent-review-workflow-20261120170436 key=r02-dispositions part=1/1 sha256=234a1f620e8457e5617e16df2d0d779b18d9d20836e5cf8879960d50caefc374 -->
### Claude Opus — finding dispositions — round 2

Candidate after corrections: `6b564215ef6e3fa8c9e998cf0514b989c713c289`

- **R1-F1** — `fixed` — commits: `0c35d54fd9`
  Freshness (PR head, branch head and recomputed merge base) is now checked when a result is accepted, again when a preserved validated result is resumed (finish_round), and at handoff (readiness). Round reuse requires identical H and M; a B move with unchanged M is recorded as an explicit reconciliation (B_at_acceptance / B_history) and noted in the handoff. Tests: test_preserved_result_is_rechecked_on_resume, test_moved_merge_base_invalidates_the_ready_result, test_head_moving_during_review_makes_result_historical.
- **R1-F2** — `fixed` — commits: `0c35d54fd9`
  Accepted reports are bound to the HEAD they were accepted for (last_report_for); a new round requires a report bound to the captured H, otherwise review_round returns needs_report and development is asked for a matching report. Each round stores its own report copy. A material change found at finalize invalidates the ready result (superseded_ready kept), returns to develop and requires a new report and review. Tests: test_head_moving_during_review_makes_result_historical, test_material_commit_after_ready_invalidates_the_ready_result.
- **R1-F3** — `fixed` — commits: `0c35d54fd9`
  Passing decisions with non-empty evidence_assessment.required_gaps are rejected. Validation entries now carry a required flag; checkpoint_ready reports with a required gate not passing are refused, and readiness re-checks it. Tests: InvalidReviewTests 'pass with required evidence gap', test_missing_dispositions_bad_citations_and_failing_required_validation_are_refused.
- **R1-F4** — `fixed` — commits: `0c35d54fd9`
  Assessments are validated for uniqueness, exact pending membership, compatibility with the developer's latest disposition, no deferral resolving a blocking finding, and restatement of rejected/still-open items; the resulting ledger is simulated before a pass is accepted. Developers cannot defer blocking findings. Tests: AssessmentValidationTests (4 cases + deferral).
- **R1-F5** — `fixed` — commits: `0c35d54fd9`
  Parse success is tracked separately from the parsed value, so JSON null and other non-objects go through schema validation and are rejected. Test: InvalidReviewTests 'JSON null'.
- **R1-F6** — `fixed` — commits: `0c35d54fd9`
  Readiness resolves required status-check contexts from branch protection and repository rulesets and requires each to have passed on the reviewed source H; failing checks on H block; an API failure is reported as unavailable, and a verified absence is noted as verified (this repository: no protection, no rulesets). Test: test_pending_restoration_unresolved_findings_and_required_checks_prevent_readiness.
- **R1-F7** — `fixed` — commits: `0c35d54fd9`
  Reconciliation requires expected actor, exact marker prefix and an identical normalized body; marker matches with differing or duplicated bodies pause the workflow without acknowledging or reposting. Test: test_marker_with_altered_body_is_not_acknowledged.
- **R1-F8** — `fixed` — commits: `0c35d54fd9`
  The lock is now an OS byte-range lock (msvcrt/flock) released by the kernel on process exit, in a canonical per-repository+branch location under the machine home, independent of state_root; enrollment takes the same lock and a canonical registry refuses a second active enrollment for the branch from any state root. State is reloaded after acquiring the lock; capture/archive refuse while the developer process is alive. Tests: test_os_lock_is_exclusive_and_survives_dead_holders, test_second_enrollment_for_active_branch_is_refused_across_state_roots, test_running_developer_session_is_never_resumed_concurrently.
- **R1-F9** — `fixed` — commits: `0c35d54fd9`
  Every mutating CLI command (run, resume including foreground and handoff resumes, review, finalize, archive) runs verify_inputs under the lock first; identity (worktree, origin, actor, PR repository/number/head/base) is checked before anything is published, and status comments are only posted once identity is established. Archive also re-verifies records and refuses with a live developer. Covered by guarded() and test_operational_failure_is_persisted_as_a_visible_stop.
- **R1-F10** — `fixed` — commits: `0c35d54fd9`
  STOP is honored before foreground review/finalize dispatch, before every reviewer attempt and retry, at finalize, and after each developer invocation; the soft deadline itself writes STOP for a cooperative pause. Tests: test_stop_then_resume_preserves_session_rounds_and_publications.
- **R1-F11** — `fixed` — commits: `0c35d54fd9`
  active_round is now a round number referencing the canonical history entry; acceptance, ledger, counters, active-round clearing and the next phase are persisted in one state write; attempt numbering is monotonic across owner-granted budgets (attempt_seq), so files are never overwritten. A migration converts states written by the first candidate. Tests: test_preserved_result_is_rechecked_on_resume, test_transient_failure_then_valid_review_counts_once_with_monotonic_attempts.
- **R1-F12** — `fixed` — commits: `0c35d54fd9`
  Archive is a restartable transaction: intended files, hashes, parent and reason are persisted before writing; the commit is recorded before pushing; on restart verified partial work is committed or the same commit re-pushed; completion is recorded once. finalize completes a pending archive first. Tests: test_push_failure_then_resume_reuses_the_same_archive_commit, test_crash_after_writing_records_commits_them_on_restart.
- **R1-F13** — `fixed` — commits: `0c35d54fd9`
  The exemption is bound to this PR's directory and to the controller's recorded inventory and SHA-256 of each record; foreign or altered records fail closed. Each review round verifies archived records are intact in H and that no unapproved records exist. Tests: test_inventory_binds_exemption_to_controller_records, test_unapproved_transcript_in_candidate_blocks_review, test_only_new_plain_records_are_exempt ('other PR').
- **R1-F14** — `fixed` — commits: `0c35d54fd9`
  Project configs and plans resolve only from the installed workflow or the owner's machine home (~/.agent-review); arbitrary paths are refused. The owner's model choices are pinned in code (POLICY) and every project config is checked against them. Enrollment IDs resolve only through the canonical registry, never as directories. Test: test_configuration_comes_from_the_installed_root_and_owner_policy.
- **R1-F15** — `fixed` — commits: `0c35d54fd9`
  PR identity now includes exact number, URL repository, isCrossRepository=false and head repository owner/name, checked at enrollment and before every mutation (verify_inputs runs before any developer dispatch). Test: test_wrong_repository_branch_actor_or_pr_cannot_enroll_or_dispatch (cross-repository and fork-owner cases).
- **R1-F16** — `fixed` — commits: `0c35d54fd9`
  Foreground enrollment records the interactive Claude session ID (CLAUDE_CODE_SESSION_ID); each foreground report must come from that session, and the controller inspects that session's CLI-recorded transcript (main-thread models and effort) without starting a second writer. Reviewer per-turn efforts are validated alongside thread settings. Tests: test_foreground_reports_are_bound_to_the_enrolled_session, InvalidReviewTests 'contradictory turn effort'.
- **R1-F17** — `fixed` — commits: `0c35d54fd9`
  Each headless invocation is charged and its launch intent persisted before dispatch; resume vs new session is decided by the actual session transcript; the returned session ID and exit status are validated before output is accepted. Soft deadline triggers a cooperative stop; after a grace period the developer is terminated only when no configured runtime lock (compatibility.lock here) exists, then the workflow pauses. Tests: test_headless_invocation_identity_and_budget.
- **R1-F18** — `fixed` — commits: `0c35d54fd9`
  guarded() converts git/gh/process/OS failures into persisted PAUSED stops with a status comment; reconciliation listing failures are part of the bounded publication retry; usage refusals stay separate and are noted locally. Test: test_operational_failure_is_persisted_as_a_visible_stop.
- **R1-F19** — `fixed` — commits: `0c35d54fd9`
  Developer permissions are now a generic base plus project extras, composed per enrollment and hash-pinned. Removed python tools/* and find; commits limited to -m/-F with --amend/--no-verify denied; the repository copy of the controller and every controller subcommand except the developer's own push helper are denied. Running tests and builds the developer writes remains inherent to development; that boundary is documented. Test: test_developer_settings_are_composed_and_pinned.
- **R1-F20** — `fixed` — commits: `0c35d54fd9`
  Complete PEM private-key blocks (including JSON-escaped forms) are redacted; an incomplete key block blocks publication; the same sanitizer covers comments, PR body and committed envelopes. Test: test_sanitize_redacts_whole_credentials_and_home.
- **R1-F21** — `fixed` — commits: `0c35d54fd9`
  The mission snapshot's hash is verified after copying and before every dispatch/acceptance (verify_inputs) and at readiness; a missing or changed snapshot or original blocks. Test: test_changed_mission_snapshot_or_workflow_version_stops.
- **R1-F22** — `fixed` — commits: `0c35d54fd9`
  status --remote reports whether a handoff is still current; resume of a superseded handoff reopens review (original receipt preserved, PR status section marked superseded, REOPENED status comment). Test: test_superseded_handoff_is_detected_and_reopened.
- **R1-F23** — `fixed` — commits: `0c35d54fd9`
  Candidate-bound receipt of the corrected focused suite, run in a clean detached worktree of the candidate: C:/Dev/KingmakerGunslingerLab/agent-review-state/kmg-codex-agent-review-workflow-20261120170436/evidence/test-receipt-6b564215ef6e3fa8c9e998cf0514b989c713c289.md (candidate, command, per-test results, duration). The live real-model smoke receipt is listed in evidence_refs.

**Validation (developer-reported):**

| Command | Required | Result | Duration | Scope and limits |
|---|---|---|---|---|
| python -m unittest discover -s tools/agent_review -p "test_*.py" -v (clean detached worktree of the candidate) | yes | pass | see receipt | Behavior suite with real temporary Git repositories, OS locks, registry, archive commits; model and GitHub boundaries simulated with labeled responses. Not a live model exchange. |
| AGENT_REVIEW_LIVE=1 python -m unittest tools/agent_review/test_live_smoke.py | no | pass | see evidence | Real headless Claude Opus developer + real Codex gpt-6.2-astra reviewer against a harmless temporary repository via the built-in guarded push; GitHub simulated. |

_Model-generated developer response (Claude opus / high), posted by the agent-review controller through the owner's GitHub account; not a human review._
