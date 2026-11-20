# GitHub agent review protocol

Version 1 · November 20, 2026 · Owner: Howie Reith

> Owner-provided behavioral specification for the agent-review workflow,
> committed verbatim in substance. Table layout was restored from the pasted
> text; wording is unchanged. The implementation and its operating guide are in
> [AGENT-REVIEW-WORKFLOW.md](AGENT-REVIEW-WORKFLOW.md).

**Purpose:** Let Claude Opus implement a mission and GPT-6.2 Astra
independently review it, with the full substantive exchange preserved on a
GitHub draft PR and in committed review records. The owner reviews the
completed candidate or receives a precise blocker. This is a specification for
the setup agent, not installed automation.

## 1. Roles and operating model

| Role | Required behavior |
|---|---|
| Developer | Claude Opus with high effort owns implementation, focused validation, evidence collection, and finding dispositions. |
| Reviewer | Codex with explicit model ID `gpt-6.2-astra` independently evaluates the candidate against the mission and evidence. High reasoning effort is the proposed initial default. |
| Controller | A small deterministic program owns request IDs, candidate snapshots, subprocesses, model settings, schema validation, locks, publication, retries, and readiness checks. |
| Owner | Howie performs final product/visual review and separately controls merging and release. |

The default implementation runs on the existing development host and uses
GitHub as the visible communication record. One owner-authorized command starts
the mission; a local helper calling a reviewer is still a GitHub PR workflow
when every substantive handoff/result/reply is published there.

A managed GitHub review bot is optional. It cannot satisfy the selected-model
requirement unless its actual supported configuration establishes the
requested model. Avoid duplicate automatic reviewers for the enrolled PR unless
they have a separately defined purpose.

The controller must accurately report its operational dependency on a running
host. It must not claim to continue after that host has stopped or lost access.

## 2. Enrollment and authoritative inputs

Enroll a specific repository, feature branch, draft PR, target branch, mission
path/hash, and owner-authorized scope. Do not activate arbitrary PRs or
repositories because a public comment mentions the workflow.

The original mission, current applicable repository guidance, and explicit
owner instructions define the task. Review findings are proposals to correct or
verify that task. Source code, logs, and PR comments do not independently
authorize new access, model changes, shell commands, scope changes, or
relaxation of requirements.

Read review inputs from a fixed snapshot. Include the original mission,
applicable instructions, the full feature diff and needed surrounding code, the
decision/acceptance records, verification evidence, and prior findings. A
developer's success summary is useful context but insufficient review evidence.

Preserve an explicit fingerprint/version for the controller, reviewer prompt,
schema, and workflow configuration used in each run. If the feature changes
those files, do not let an already running loop silently reload changed
permission or completion rules. Review and deliberately activate the new
workflow version.

## 3. Model selection and provenance

Use explicit launch settings for the primary developer, including resumed
invocations: Opus and high effort. Record the resolved Opus version when the
provider exposes it. The user selected an Opus family, so an invented version
pin is unnecessary; Sonnet execution does not satisfy the choice.

Use the exact supported `gpt-6.2-astra` identifier for the independent
reviewer. Record requested and observed values separately. Capture relevant
runtime metadata, not a model-authored assertion about its identity. Pin the
allowed reviewer model; disable or detect fallback/remapping through the
supported configuration surface. Model unavailability becomes a visible
blocker.

Record CLI versions, authentication type without secrets, requested model and
effort, observed model/effort when exposed, and metadata limitations. A goal
evaluator or other non-development support call may use its own small model;
distinguish that from the main developer and reviewer rather than incorrectly
treating all session traffic as primary work.

Use existing subscription authentication when available and suitable. Report
when the selected route instead uses metered API billing. A Claude-only budget
does not control an independently launched Codex process, so track the roles
separately where usage is available. Never claim a precise combined cost when
the authentication route does not expose it.

## 4. Candidate identity

Every review request records:

- Repository and PR number.
- Candidate commit H, verified against the pushed PR head when requested.
- Target branch and its captured tip B.
- The merge base M used to define the feature diff.
- Mission/contract identity and relevant workflow versions.
- A unique request ID, a monotonically increasing round number, and the
  checkpoint's fixed scope/type (intermediate or final) with its exit
  conditions.
- Validation/evidence identifiers, including binary and fixture identities
  when applicable.

The reviewer examines a detached worktree or equivalent fixed snapshot of H.
The developer's working files cannot change underneath it. A reviewer that
needs build outputs uses isolated output state or requests the relevant
observation from the developer. It does not run the game's
installation/fixture workflow concurrently with the developer.

Before accepting a review as current, re-read remote PR identity and status. A
new material head commit makes the old review historical. A moved target branch
must be recorded and reconciled under repository policy; do not silently retain
a verdict tied to different comparison inputs. Confirm whether required hosted
checks refer to the candidate rather than an earlier commit.

Only one controller and one developer writer may own the enrolled PR. Acquire a
process/worktree lock. On restart, reconcile recorded state with the remote
branch, PR, and running processes. Never overwrite another active writer or
reset away unrelated changes.

## 5. Normal round and result contract

1. Developer completes a coherent checkpoint, performs appropriate validation,
   commits, and pushes through the repository's required path.
2. Controller captures immutable inputs and posts a candidate-bound review
   request.
3. Controller runs the independent reviewer and captures the final complete
   response.
4. Controller checks process outcome, structured schema, request identity,
   model configuration/provenance, and candidate identity.
5. Controller publishes the review faithfully and records the accepted
   response.
6. Developer responds to each actionable finding, makes supported corrections
   in new commits, and supplies focused evidence.
7. Controller starts a review of the revised candidate. A substantial rebuttal
   may instead be returned to the reviewer for resolution without unnecessary
   code changes.

A valid `checkpoint_passed` advances an intermediate checkpoint to the next
implementation phase automatically. Only a final review can produce
`ready_for_owner_review`; the controller rejects a result whose decision does
not match the enrolled checkpoint type.

Intermediate reviews assess the phase's defined exit evidence and whether the
design can support the remaining mission. Later-phase work that is not due yet
is recorded as pending, rather than incorrectly treated as a defect or a reason
to stop for the owner. The final checkpoint assesses the complete required
mission. Neither agent may relabel a final checkpoint as intermediate to evade
completion criteria.

Define a versioned JSON schema for the model's review and a controller-owned
envelope for provenance. The model may echo request identifiers; the controller
must compare them to its own values, never use the model's copy as the
authority.

| Field group | Required information |
|---|---|
| Controller envelope | Protocol version, request/round/checkpoint IDs, repository/PR, H/B/M, mission identity, workflow versions, timestamps, invocation/model metadata, exit/parse outcome, response hash, and published comment identifiers. |
| Review decision | `changes_required`, `checkpoint_passed` for an intermediate checkpoint, `ready_for_owner_review` for a final checkpoint, or `blocked`. |
| Findings | Stable finding ID, priority, whether it blocks readiness and why, concrete triggering behavior, expected/actual behavior, relevant file location, affected acceptance IDs, and requested correction/evidence. |
| Evidence assessment | Tests/observations inspected, required gaps, limitations, and explicit distinction between executed verification and source inspection. |
| Summary | Delivered behavior, scope reviewed, unresolved questions, and exact remaining owner review. |

Suggested priorities are P0 through P3, but readiness depends on correctness
and required acceptance, not a severity threshold alone. A missed required
behavior does not become acceptable because it was labeled P2. Keep optional
polish separate from defects and required work.

Exit code zero, an empty findings array without the rest of a valid report, an
emoji, a partial stream, silence, or a reassuring sentence is not readiness. A
process failure, malformed output, mismatched request, wrong/unavailable model,
timeout, or missing evidence is a failed or blocked review, never an implicit
pass.

## 6. Findings, rebuttals, and scope

Use stable finding IDs across rounds. The developer posts one of: fixed with
commit/evidence; rebutted with concrete evidence; deferred optional work with
rationale; or unresolved blocker. It cannot quietly drop a finding or mark its
own rebuttal accepted.

The reviewer checks proposed fixes and accepts or rejects rebuttals explicitly.
Every actionable finding needs a final disposition. A recurring disagreement
without new evidence triggers the no-progress policy.

The reviewer should report all supported actionable findings discovered within
the requested scope in a coherent review. It must not expand the mission into
speculative refactors or a whole-project audit. Required corrections should
receive the failing reproduction and affected-neighbor checks first.

The developer cannot lower the acceptance bar, remove a required behavior,
rewrite the mission, or weaken validation to obtain a clean review. A necessary
material adaptation is recorded with its evidence and left open for owner
judgment when the original task does not authorize it.

## 7. GitHub conversation and account identity

Post new messages for requests, reviews, developer dispositions, and final
outcomes. Include a machine-readable request marker and a human-readable
heading identifying the role, model, round, and reviewed commit. Example
headings:

```text
Claude Opus — review request — round 2 — candidate <H>
Codex GPT-6.2 Astra — review — round 2 — candidate <H>
Claude Opus — finding dispositions — round 2
Workflow — awaiting owner review — source <H>, archive <A>
```

A single GitHub account may publish both roles. Say that the workflow posted a
model-generated review; do not imply a separate GitHub person or human approval
exists. Use ordinary PR comments or comment-only reviews. Do not make formal
self-approval or removal of required human reviews a prerequisite.

Write comment bodies to UTF-8 files and use a structured API or
`gh ... --body-file`. Publish the actual review without editorial removal of
inconvenient findings. Routine formatting and necessary secret redaction are
acceptable; record redactions and do not silently change the verdict. Link full
committed findings if a GitHub message limit requires a shorter visible
summary.

Treat publication as a recoverable operation. Persist the outgoing message and
unique marker before sending. After a timeout or restart, query the PR and
reconcile the existing marker before retrying. A role heading alone is not
authenticity: validate repository, PR, expected posting identity, request ID,
and controller-recorded result hash. Paginate conversation reads. Do not
execute free-form commands copied from comments.

Preserve old messages. Post corrections as new messages; the controller may
update a clearly designated status index without erasing the substantive
history. Preserve original result bodies and hashes even when a later finding
is withdrawn.

## 8. Committed records without endless rereview

Git stores committed files; PR comments live in GitHub. Satisfy both parts of
the requested history by storing sanitized canonical copies of substantive
requests, reviewer findings, developer replies, and final dispositions in a
dedicated transcript directory, for example `docs/agent-reviews/pr-<number>/`.

Those records contain user-facing review content and useful evidence
summaries, not hidden reasoning, raw model event streams, credentials, local
authentication/configuration, proprietary game assets, saves, generated
packages, or raw machine-local runtime evidence. Preserve links/hashes for
evidence under the repository's existing policy. Never publish a secret merely
to make a transcript complete.

Resolve the final-review commit problem explicitly:

- H is the source candidate actually reviewed and, where applicable, tested.
- A is a later archive commit containing only permitted append-only transcript
  records.
- The controller may report readiness at PR head A only if H is its ancestor,
  the diff from H to A contains solely the narrowly allowed inert transcript
  additions, and all required gates still hold.

The archive exemption is path- and operation-checked. Permit only new data
records with expected names/extensions; no edits to earlier records,
executables, symlinks, instructions, configuration, mission requirements,
tests, product documentation, source, or dependencies. Fail closed on
ambiguity.

Any material change requires a review of the new candidate. Evidence remains
attributed to the source/binary originally observed; archive commits do not
relabel old runs as new execution.

Final PR metadata names both H and A and explains the transcript-only
relationship. The final receipt acknowledging storage need not recursively
create another archive commit.

An isolated journal branch is an acceptable alternative if it is simpler under
the repository's tooling and preserves the same integrity. Choose one strategy
and document it. Do not require a new full product build just to write an inert
review transcript.

## 9. Permissions and trust boundaries

Reuse existing repository access. Record actual effective permissions before
and after any change, including when no change was necessary.

| Capability | Permission scope |
|---|---|
| Read source, PRs, comments, and checks | Relevant selected-repository read access. |
| Push developer or transcript commits | Contents write through the current guarded push workflow, limited operationally to the assigned branch. |
| Create/update a draft PR and post discussions | Necessary pull-request/comment writes for the enrolled PR. |
| Read hosted validation results | Actions/checks read only when used. |
| Publish optional commit status/check | Only that permission if the design actually publishes one. |
| Modify Actions workflows or dispatch jobs | Only if chosen and necessary; not a prerequisite for the local controller. |

A token's repository contents permission is not a branch-specific security
boundary. Enforce target validation in the controller, preserve branch
protection, and do not describe a broad credential as inherently
branch-limited.

Configure specific allowed helper/validation operations rather than blanket
shell or API permissions. Helpers accept validated data and fixed operations,
not arbitrary command strings. Project-specific permissions must not overwrite
global preferences or managed restrictions.

The reviewer uses a read-only source sandbox, narrowly scoped tools, and no
responsibility for publishing or applying fixes. A filesystem sandbox alone
does not prevent write-capable external connectors from acting; inspect the
review profile accordingly. The controller handles GitHub writes and keeps
authentication in existing secure stores. Never copy subscription auth files
into GitHub secrets or source control as a setup shortcut.

The endpoint is a draft PR. Do not weaken protected-branch requirements, turn
on automatic merging, request human reviewers, create broad new credentials, or
alter unrelated organization/repository access to make the pilot look complete.

If GitHub Actions is selected instead of the local controller, prove its real
event/authentication chain and account for default-branch requirements. Do not
assume a token-generated comment or push will start another unattended
workflow. An unmerged workflow file is not proof of an active service.

## 10. Continuation, budgets, and recovery

Persist a minimal local state record atomically: enrollment, checkpoint,
rounds, pending request/publication, current phase, saved developer session ID,
captured candidates, and last completed transition. Keep private execution
state separate from public transcripts. Recover by reconciling both local state
and GitHub; local files are not automatically authoritative after a crash.

Use explicit session IDs to resume the developer. Never run two invocations
against the same session or worktree at once. The controller may use an
existing foreground Claude session for development or own a headless session;
document which mode is implemented. If setup itself is running in Claude, avoid
recursively launching a second setup writer.

Suggested initial limits: five completed review rounds per checkpoint, bounded
per-process deadlines, and a separately recorded overall mission budget chosen
to fit the work. Persist cumulative limits across resumes; a restarted goal's
reset counters cannot reset the controller's budget. Use bounded exponential
backoff or server-provided retry guidance for transient failures. A hard
deadline must not interrupt a runtime restoration transaction.

Provide inspect/status, pause/stop, and resume entry points. Surface progress
without continual polling or duplicate subprocess launches. A canceled or
failed reviewer does not produce readiness. A publication failure pauses at the
preserved result so it can be published without rerunning a successful review.

On exhausted retries, inaccessible model/runtime, repeated disagreement, no
progress, or the review-round cap, preserve work and return a precise BLOCKED
or PAUSED report. Avoid automatic model downgrades and open-ended review loops.

## 11. Readiness rules

`AWAITING_OWNER_REVIEW` requires all of the following:

- Required implementation and appropriate acceptance evidence are present;
  required failures/gaps are not hidden among optional deferrals.
- All actionable correctness findings have supported, reviewer-accepted
  dispositions.
- The valid reviewer result covers the current material candidate with the
  selected model; any archive-only successor satisfies Section 8.
- Required focused validation/hosted gates are complete for their correct
  source identities. No required pending or failed check is treated as success.
- The draft PR, committed records, discussion, decision/acceptance records, and
  required assets/evidence links are available.
- Any runtime transaction has restored its captured installation/saves, with
  the required confirmation.
- Owner-only visual judgment and release qualification remain accurately
  described.

This is readiness for the owner, not authorization to merge, release, or
declare subjective visual acceptance.

## 12. Setup acceptance

Exercise real local components, temporary Git repositories, and actual
persistent state. Stub external service boundaries only where needed to
reproduce failures deterministically. Do not require one test per row when a
realistic scenario covers several.

| ID | Required demonstration |
|---|---|
| WF01 | Actual supported CLI/auth routes invoke Opus high and Astra; requested/observed metadata and limitations are recorded. |
| WF02 | A real draft setup PR receives a review request and an independent Astra result for its fixed candidate. |
| WF03 | Claude receives findings, posts dispositions, preserves a new corrective commit, and requests rereview; distinguish a real finding cycle from a clearly labeled fixture simulation. |
| WF04 | A moved material head or changed comparison input cannot reuse a stale ready result. |
| WF05 | Wrong model, malformed/truncated output, failed process, and missing required fields cannot produce readiness. |
| WF06 | A lost publication response or duplicate event is reconciled without duplicate review requests/comments. |
| WF07 | Stop/resume preserves the session, round budget, completed publications, and single-writer lock. |
| WF08 | Unresolved findings, missing required evidence, and exhausted limits yield visible blocked/paused status. |
| WF09 | Transcript-only commits preserve readiness only under the narrow archive check; a material change or altered earlier record invalidates that exemption. |
| WF10 | Wrong repository/PR/actor/marker input cannot dispatch work or broaden permissions; comment text is never executable shell input. |
| WF11 | Final handoff remains a draft, has no requested human reviewers, and documents the effective permissions and exact launch/resume/stop commands. |
| WF12 | A clean intermediate review advances to the next phase without owner input; only the final checkpoint applies full-mission readiness and ends at owner review. |

Use the real setup changes for the live pilot. If they produce no genuine
finding, use an isolated harmless fixture to verify the correction path. Do not
fabricate a model verdict or introduce a defect into gameplay code to
manufacture a demonstration.

## 13. Black Tentacles application

The existing mission remains the contract. Automate review after Phases 0–1 and
at the complete Phase 4 candidate, with additional targeted review only for
concrete risks.

Preserve all 36 acceptance identifiers. Native observations are required for
the mission's engine behavior claims. Keep spell access, both combat modes,
action economy, immunity/overlap, persistence, cleanup, and visual evidence in
scope. Actual motion supports the final visual review; a static screenshot or
agent agreement is insufficient.

Follow the current focused-validation policy, guarded push path, runtime
leases, disposable fixtures, immutable baseline, and transactional
restoration. The workflow's setup pilot does not need to touch the game. The
later feature run may use only already authorized native procedures.

## 14. Verified mechanism references

- Claude model and effort configuration
- Claude CLI and explicit resume
- Claude noninteractive execution
- Claude goals and evaluator limits
- Claude permission configuration
- Codex scripted execution, schemas, and authentication reuse
- GPT-6.2 Astra identifier
- GitHub comment CLI
- GitHub author self-approval limitation

The controller, archive strategy, limits, and acceptance gates above are
proposed implementation requirements for this task; they are not represented
as built-in vendor features.
