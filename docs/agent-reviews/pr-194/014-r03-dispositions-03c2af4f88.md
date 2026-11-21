<!-- agent-review:v1 enrollment=kmg-codex-agent-review-workflow-20261120170436 key=r03-dispositions-03c2af4f88 part=1/1 sha256=b93387d187485cb74623775f7c91cf4ef7027b41feaaa68e9967732b48912d47 -->
### Claude Opus — finding dispositions — round 3

Candidate after corrections: `a7a008ee849d4971f3e5930f5369a8dc37c03654`

- **R1-F1** — `fixed` — commits: `4adde14fc0`
  finish_round re-checks freshness after the review publication (make_historical posts a visible correction and pauses); finalize re-runs readiness after posting the handoff and posts a correction + invalidates if anything changed; update_pr_body refuses a moved head. Regression test: test_freshness_is_rechecked_after_publication.
- **R1-F3** — `fixed` — commits: `4adde14fc0`
  A required_gap finding must block (rejected otherwise, in any decision); plans may declare checkpoint required_validation gates, and reports lacking a passing required entry for each are refused. Regression test: test_blocking_status_is_sticky_and_required_gaps_must_block, test_checkpoint_declared_gates_are_enforced.
- **R1-F4** — `fixed` — commits: `4adde14fc0`
  An open blocking finding cannot be restated as non-blocking (validation reason), and the ledger keeps blocks_readiness sticky while the finding stays open, so it cannot later be deferred. Regression test: test_blocking_status_is_sticky_and_required_gaps_must_block.
- **R1-F6** — `fixed` — commits: `4adde14fc0`
  Required checks carry provider identity (protection checks[].app_id, rulesets integration_id); hosted results carry app.id and must match; only GitHub's explicit 'Branch not protected' / 'Required status checks not enabled' answers count as verified absence, any other failure is 'inventory unavailable'. Regression test: test_readiness_checks_runtime_lock_records_and_check_providers.
- **R1-F7** — `fixed` — commits: `4adde14fc0`
  Reconciliation accepts only a single same-actor comment with the marker and an identical body (rivals pause); every reused outbox body is verified against its recorded digest; readiness verifies every posted record still exists unaltered on the PR; the posting account returned by GitHub must be the expected actor. Regression test: test_rival_marker_and_tampered_outbox_are_refused, test_readiness_checks_runtime_lock_records_and_check_providers.
- **R1-F8** — `fixed` — commits: `4adde14fc0`
  Writer exclusion now also takes an OS lock in the worktree's own git directory and enrollment writes/consults a marker there, so different AGENT_REVIEW_HOME values cannot both own the worktree; the target-validation test no longer switches homes (PR identity is tested through the shared checker). Regression test: test_writer_exclusion_does_not_depend_on_the_selected_home.
- **R1-F9** — `fixed` — commits: `4adde14fc0`
  The actor is re-read at every verify_inputs (no cache), and each posted comment's author returned by GitHub must equal the expected actor. Regression test: test_actor_is_refreshed_and_posting_identity_checked.
- **R1-F11** — `fixed` — commits: `4adde14fc0`
  The stop outcome (status and reason) is saved in the same write as round acceptance; a successful attempt promotes the round to validated in the same write that records the attempt. Regression test: test_stop_outcome_is_saved_with_the_round.
- **R1-F12** — `fixed` — commits: `4adde14fc0`
  Archive intent persists each record's content; a partially written set is completed from it; a commit made just before the crash is adopted only if its sole parent is the recorded parent and it passes the inventory-bound transcript-only check. Regression test: test_archive_recovers_partial_writes_and_an_unrecorded_commit.
- **R1-F13** — `fixed` — commits: `4adde14fc0`
  Each review verifies the complete excluded transcript delta M..H (any PR): only additions of the controller's recorded records as 100644 files with matching content; archived records are also mode-checked in H. Regression test: test_reserved_inbox_path_and_foreign_transcript_changes_block.
- **R1-F14** — `fixed` — commits: `4adde14fc0`
  Bare config names must be identifiers and every selected file (name or path) must resolve inside a trusted directory. Regression test: test_config_names_cannot_escape_trusted_directories.
- **R1-F16** — `fixed` — commits: `4adde14fc0`
  Effort is read only from main-thread assistant events' own effort/perTurnEffort fields; text mentioning effort and sidechain events are ignored. Regression test: ToolTranscriptTests.test_extracts_recorded_commands_and_outputs_since_a_time.
- **R1-F17** — `fixed` — commits: `4adde14fc0`
  The parser treats a missing is_error as failure, and a report is accepted only with exit 0, is_error false and subtype success. Regression test: test_developer_report_requires_an_explicit_successful_terminal_event.
- **R1-F18** — `fixed` — commits: `4adde14fc0`
  guarded converts ValueError (incl. JSON decode), KeyError and TypeError from operational inputs into persisted PAUSED stops with the failure text. Regression test: test_decode_failures_are_persisted_stops.
- **R1-F19** — `fixed` — commits: `4adde14fc0`
  For projects with push_command the helper refuses to push and its permission rule is omitted from the composed settings; the helper is only a push route for projects without a wrapper. Regression test: test_wrapper_projects_cannot_use_the_builtin_helper.
- **R1-F22** — `fixed` — commits: `4adde14fc0`
  handoff_superseded runs the full readiness evaluation (identity, archive exemption, merge base, requested reviewers, required checks, records, runtime lock) at an unchanged head; observation failures propagate as unknown instead of 'current'. Regression test: test_readiness_checks_runtime_lock_records_and_check_providers.
- **R3-F1** — `fixed` — commits: `4adde14fc0`
  The validated round records its envelope digest; finish_round consumes it only if the digest and the envelope's workflow fingerprint match, otherwise it invalidates and reruns the review (not counted); activate invalidates validated and (pre-handoff) ready results so they are re-reviewed under the new rules. Published comment IDs are kept in round state, so the envelope is never rewritten. Regression test: test_preserved_results_are_bound_to_content_and_workflow_version.
- **R3-F2** — `fixed` — commits: `4adde14fc0`
  The reviewer inbox is reserved: if the candidate contains .agent-review-inputs (or a link) the round blocks; the directory is created fresh and its resolved parent must be the snapshot. Regression test: test_reserved_inbox_path_and_foreign_transcript_changes_block.
- **R3-F3** — `fixed` — commits: `4adde14fc0`
  Disposition comments are keyed by a digest of (candidate, dispositions): replaying the same accepted report is idempotent, a different set gets its own comment. Regression test: test_disposition_publication_is_idempotent_per_content.
- **R3-F4** — `fixed` — commits: `4adde14fc0`
  Readiness fails while any configured runtime lock exists, naming the developer's restoration claim. Regression test: test_readiness_checks_runtime_lock_records_and_check_providers.
- **R3-F5** — `fixed` — commits: `4adde14fc0`
  The helper validates every effective push URL (git remote get-url --push --all origin) against the approved list before pushing. Regression test: GeneralRepositoryTests.test_builtin_guarded_push_refuses_unsafe_pushes.
- **R3-F6** — `fixed` — commits: `4adde14fc0`
  Foreground rounds no longer receive the whole-session log: the reviewer gets only controller-held receipts from the enrollment's evidence/ directory that the report cites; headless transcripts (dedicated session) sanitize each command and output before truncation. Regression test: test_foreground_reviewer_gets_receipts_not_the_whole_session_log.

**Validation (developer-reported):**

| Command | Required | Result | Duration | Scope and limits |
|---|---|---|---|---|
| python -m unittest discover -s tools/agent_review -p "test_*.py" -v (clean detached worktree of the candidate) | yes | pass | see receipt | Behavior suite; real Git/state/OS locks/archive commits; model and GitHub boundaries simulated with labeled responses. |
| AGENT_REVIEW_LIVE=1 python -m unittest tools/agent_review/test_live_smoke.py (run 6) | no | pass | 64s | Real headless Claude Opus + real Codex gpt-6.2-astra on a harmless temporary repository; GitHub simulated. Run on the working tree equal to this candidate's tools/agent_review. |

_Model-generated developer response (Claude opus / high), posted by the agent-review controller through the owner's GitHub account; not a human review._
