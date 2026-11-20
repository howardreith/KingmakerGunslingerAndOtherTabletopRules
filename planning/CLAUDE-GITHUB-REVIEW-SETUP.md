# Claude setup prompt: GitHub development and independent review

Prepared for Howie Reith · November 20, 2026

> Owner-provided assignment, committed verbatim in substance as the mission
> contract for the agent-review setup PR. Table layout was restored from the
> pasted text; wording is unchanged.

**Use:** Give this document and `GITHUB-AGENT-REVIEW-PROTOCOL.md` to Claude
Code. The existing Black Tentacles mission provides the first intended feature
contract. This document is an implementation assignment for the workflow; it is
not a claim that the workflow is already installed.

Please set up a reusable workflow in which you develop a feature, send a
committed candidate to an independent Codex reviewer, receive and address its
feedback, and repeat until the work is ready for my final review. Use GitHub
pull requests to preserve the changes and the discussion. I should not have to
copy review comments between agents or send routine continuation prompts.

The primary developer must be Claude Opus with high effort. The independent
reviewer must be Codex using GPT-6.2 Astra, model ID `gpt-6.2-astra`. Preserve
these choices. Use high reasoning effort for the reviewer as the initial
workflow default, if supported. Verify actual model selection; do not silently
substitute another model when access, configuration, or a usage limit causes a
problem.

## 1. Scope and authority

Target repository: `howardreith/KingmakerGunslingerAndOtherTabletopRules`.

Set up and demonstrate the workflow, then leave a concrete handoff and the
command for starting the Black Tentacles mission. The spell's gameplay
implementation belongs to that subsequent assignment.

You are authorized to do the routine setup needed here: inspect the current
environment, create an isolated worktree and branch, implement the scripts and
documentation, configure narrowly scoped project permissions, use existing
authenticated connections, create or update a draft PR, and post the automated
requests, findings, responses, and status reports on that PR. Continue through
setup and correction rounds without repeatedly asking me to authorize those
actions.

This authorization is scoped to this repository and this workflow. Keep the
result in draft PRs with no human reviewers requested. Merge, release
publication, tagging, force pushes, destructive history changes, purchases, and
modification of my normal-play installation are outside this assignment.
Preserve existing branch protections, runtime safeguards, credentials, other
worktrees, and unrelated settings.

If a real sign-in, missing credential, account entitlement, or managed
restriction prevents a necessary operation, finish the independent work first
and report the precise remaining action. Do not invent access, weaken a
safeguard, or treat an unconfigured integration as working.

## 2. Inspect before changing anything

Read the current AGENTS.md, CLAUDE.md if present, applicable nested
instructions, docs/VALIDATION-POLICY.md, and existing automation. Inspect
installed claude, codex, gh, Git, and scripting runtimes. Establish the actual
repository path, current default branch, remote, active worktrees, working-tree
state, authentication route, and existing permissions.

The default branch was master when this brief was prepared; verify it again.
Use a collision-free `codex/agent-review-workflow` branch or the repository's
equivalent.

The repository instructions currently require coherent checkpoint pushes
through:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File C:/Dev/KingmakerGunslingerLab/codex-policy/Push-KingmakerGunslinger.ps1
```

Read the current instructions and inspect that entry point before using it.
Integrate with the established push workflow; do not replace it with an
unrestricted push path to make automation easier.

Prefer the existing scripting conventions and dependencies. A small controller
and a focused helper are sufficient. Add a library only when it replaces
substantial code, is maintained, and its benefit is explained.

## 3. Implement a GitHub PR workflow with explicit model selection

Use the attached protocol as the behavioral specification.

The recommended execution arrangement is a small local controller in the
existing development environment. GitHub holds the draft PR, commits, requests,
reviews, replies, and final handoff. Local execution is suitable for this
project's native references and guarded game workflow. Document that the
controller's host must remain available while it runs.

Invoke the reviewer through supported Codex CLI functionality with the model
explicitly selected. Do not rely on a managed @codex review request to satisfy
the Astra requirement unless the current integration demonstrably supports and
reports that selection. A review produced by another model is not an Astra
review.

Current command shapes to verify against the installed versions are:

```text
claude --model opus --effort high
claude --model opus --effort high --resume SESSION_ID -p PROMPT --output-format stream-json --verbose
codex -a never exec --model gpt-6.2-astra --sandbox read-only --output-schema SCHEMA_PATH --output-last-message RESULT_PATH -
```

These are command shapes, not fully configured launch commands. Use proper
argument arrays and prompt files/stdin when implementing them. Never
interpolate PR text into executable shell strings.

Resolve opus to the real available Opus model for the account and record it.
Keep the main development work on Opus high; opusplan does not satisfy this
requirement because execution can switch to Sonnet. Record requested and
observed model/effort separately, using CLI/session metadata where exposed. A
model saying its own name is not verification. If the interface does not
expose an effective value, document that limitation rather than fabricate it.

Verify that the process which will actually do the work receives the correct
settings. Editing a config file does not establish that an already running
conversation changed models. Scope settings to the workflow/project and
preserve existing configuration. Keep authentication material private.

## 4. Implement the full exchange

For each substantial checkpoint:

1. You implement and run the appropriate focused validation.
2. You commit and push through the required workflow.
3. The controller records the PR, exact candidate/base/merge-base commits,
   mission identity, round ID, and available evidence, then posts a review
   request on the PR.
4. A separate Astra reviewer inspects the fixed candidate, mission, applicable
   policies, relevant surrounding source, acceptance report, and evidence.
5. The controller validates the complete result and posts the review
   faithfully to GitHub, with the reviewed commit and model attribution.
6. You post a disposition for every actionable finding, make the needed
   corrections in new commits, validate the affected behavior, and request
   another review.

A clean intermediate result is `checkpoint_passed` and automatically advances
development to the next phase. Only the final candidate ends at
`AWAITING_OWNER_REVIEW`. A precise BLOCKED/PAUSED state records a genuine
impediment. All substantive requests, findings, and replies remain visible.

Use existing Github authentication if it has the required access. If both
agents post through my account, label the actual role and model in the comment
body. Do not present those comments as independent human approvals. Use
ordinary PR comments or comment-only reviews; do not depend on approving the
PR under its author's account.

Preserve the conversation in GitHub and commit sanitized, append-only copies
of the meaningful round records to the repository. Implement the protocol's
explicit distinction between a reviewed source candidate and subsequent
transcript-only commits. Avoid a loop in which saving the final review creates
a new commit that itself endlessly demands another full review.

## 5. Configure permissions and continuation

Enable the narrow permissions actually needed for this implementation: reading
the selected repository and its PRs/checks, writing the assigned branch through
the approved wrapper, creating/updating draft PRs, and posting the workflow's
comments. Reuse existing access. Add check/status-write, Actions, or
workflow-file permissions only if the chosen design actually needs them.

Configure Claude's project-local command allowances around the dedicated
helpers and validation commands. The helpers must validate their repository,
PR, branch, model, paths, and allowed operations. Do not grant arbitrary
shell/API execution or disable approval/sandbox protections globally.

Keep Codex separate from the implementation worktree, with read-only source
access and no publishing responsibility. The controller publishes its result.
Inspect enabled tools/connectors so the review profile does not inadvertently
carry unrelated write capabilities.

Implement automatic continuation, bounded retries, a persistent round budget,
and stop/resume. A specific saved Claude session may be resumed only when its
prior invocation has exited. Avoid --last/most-recent-session shortcuts when
another task could be selected. There must be one controller/developer writer
for this PR at a time.

Claude's /goal or a Stop hook may help, but neither replaces the controller's
checks of commit identity, model, complete structured results, unresolved
findings, and required evidence. Preserve round counts across restarts. Respect
cancellation and finish any active runtime restoration before stopping.

Start with at most five completed review rounds per checkpoint. Make the limit
configurable. Use bounded retry/backoff for transient failures; a failed API
call is not a completed review round. If the required model or runtime is
unavailable, or a substantive disagreement is repeating without progress,
report the blocker without downgrading the model or requirements.

## 6. Produce the reusable materials

Choose paths that fit the current repository. Expected deliverables are:

- The controller/helper implementation, with launch, review, status, resume,
  and safe stop behavior as appropriate.
- A versioned reviewer-output schema and explicit model/settings configuration.
- A concise workflow document covering architecture, permissions, normal use,
  recovery, costs/limits, and uninstall/disable steps.
- Reviewer and developer handoff templates that read the original mission and
  evidence directly.
- Committed round records, a small resumable local state record, and a clear
  policy separating them from secrets and raw runtime evidence.
- A focused setup acceptance report and a draft setup/pilot PR.
- The exact command for later starting Black Tentacles, including its branch,
  mission path, and configured models.

Keep code conventions consistent. Use behavior-focused tests with real
cooperating components and temporary Git repositories. Stub only external
model/GitHub boundaries when necessary; do not mock entire internal modules or
test source text instead of behavior.

## 7. Prove the setup works

Run a real, bounded preflight for the requested models and demonstrate an
Astra review of the setup draft PR. Use the actual setup changes for that
pilot. If it identifies a supported defect, address it in a new commit and
obtain a fresh review.

Demonstrate the repair-and-resume branch with a harmless isolated fixture if
the real setup review has no findings. Clearly label simulated external
responses. Do not plant a gameplay defect, fabricate a Codex finding, or
describe a simulated cycle as an actual model exchange.

Verify the critical failure behavior listed in the protocol: stale candidates,
malformed/failed reviews, duplicate publication, wrong model, exhausted rounds,
resume, and transcript-only commits. Use focused tests; this tooling change
does not call for a historical whole-mod validation campaign or a game
installation.

The workflow is not operational merely because its scripts compile or a draft
Actions file exists. Demonstrate the actual authentication, invocation,
publication, feedback, and continuation path that will be used.

## 8. Bind the later Black Tentacles run

Keep the Black Tentacles mission as the feature contract. Add only the
review-workflow connection needed for its later execution, preserving its
original requirements.

Use automated review after the native-contract/early-visual proof and again on
the complete feature candidate. Routine corrections continue without owner
checkpoint prompts. Enforce the mission's required native evidence, ordinary
spell access, both combat modes, paid escape, persistence, overlap, visuals,
and restoration. The agents agreeing with each other cannot establish those
engine behaviors.

Final owner review includes actual candidate-bound motion and images. The
automated workflow must distinguish mechanical completion, readiness for visual
review, and release. Existing focused-validation budgets and runtime leases
remain in force.

## 9. Final handoff

Report the setup PR and branch, reviewed source identity, actual model/effort
evidence, files and settings changed, permissions reused or enabled, tests and
pilot conversation links, how to start/inspect/resume/stop the workflow, and
any precise remaining setup limitation.

State separately whether the workflow is operational, whether its repair path
was demonstrated with real models or fixtures, and whether Black Tentacles has
started. End with the useful completed result. Do not stop after proposing this
setup or after writing only its documentation.

## Reference sources

These establish the product mechanisms; the protocol's orchestration rules are
task-specific design choices. Verify the installed versions before
implementation.

- Claude model configuration
- Claude CLI reference
- Claude permissions
- Claude noninteractive execution
- Claude goals
- Codex noninteractive execution and structured output
- Codex command reference
- GPT-6.2 Astra model identifier
- GitHub PR comments through gh
- GitHub review and self-approval rules
- Current repository instructions
- Current validation policy
