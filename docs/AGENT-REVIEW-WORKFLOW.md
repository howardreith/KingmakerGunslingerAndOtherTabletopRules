# Agent review workflow in this repository

Claude Opus (`opus`, high effort) develops; Codex GPT-6.2 Astra
(`gpt-6.2-astra`, high effort) independently reviews in a read-only sandbox.
A local controller runs the exchange on a GitHub draft PR, records observed
model settings, and stops at owner review. Howie controls merging and release.

The controller is installed at `C:/Dev/agent-review/`. Its maintained
[operating guide](https://github.com/howardreith/development-process/blob/main/agent_review/README.md)
and [behavioral specification](https://github.com/howardreith/development-process/blob/main/docs/GITHUB-AGENT-REVIEW-PROTOCOL.md)
live in `howardreith/development-process`. This document describes the
Tabletop Rules Expansion repository's integration with that controller.

The workflow was first piloted on
[PR #194](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/194),
which was closed without merging after the controller moved to
development-process. The pilot conversation and
[historical records](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/tree/codex/agent-review-workflow/docs/agent-reviews/pr-194)
remain available there; restoring this guide does not require copying those
records into the current branch.

## Repository integration

The project selector is `kingmaker-gunslinger`, with project ID `kmg`.
The reviewed project configuration is
[`agent_review/projects/kingmaker-gunslinger.json`](https://github.com/howardreith/development-process/blob/main/agent_review/projects/kingmaker-gunslinger.json).
The controller uses the installed configuration and pins it for each run.

| Setting | Value here |
|---|---|
| Repository | `howardreith/KingmakerGunslingerAndOtherTabletopRules` |
| Target branch | `master` |
| Work branches | Dedicated `codex/*` branches |
| Push | `C:/Dev/KingmakerGunslingerLab/codex-policy/Push-KingmakerGunslinger.ps1 -RepositoryRoot <worktree>` for developer checkpoints and controller archive commits; this project's controller push helper refuses direct pushes |
| Required gate | `focused`: `scripts/Invoke-KmgGate.ps1 -Filter <name>`, run through `verify --gate focused -- -Filter <name>` on the committed candidate |
| Instructions for both agents | `AGENTS.md`, `docs/VALIDATION-POLICY.md`, `docs/AGENT-REVIEW-WORKFLOW.md` |
| Private state | `C:/Dev/KingmakerGunslingerLab/agent-review-state/` |
| Review records | `docs/agent-reviews/pr-<N>/`, written and archived by the controller |
| Runtime protection | All six shared Kingmaker lab transaction locks listed in the project configuration; an active runtime transaction prevents developer termination and final handoff |

The project requires its `focused` gate at every enrolled checkpoint,
including the generic `single` plan. Choose the named filter for the changed
behavior and its actual dependencies; reject zero-test selections. A plan
cannot drop the project's required gate.

The project configuration supplies the headless developer's specific build,
test, guarded runtime and push permissions. It denies the listed deployment,
publication and direct live-mod operations, and edits to
`docs/VALIDATION-POLICY.md` and `GamePath.props`. This guide does not broaden
those permissions.

## Responsibilities and validation

Read [AGENTS.md](../AGENTS.md) and
[VALIDATION-POLICY.md](VALIDATION-POLICY.md) before selecting work and checks.
The current validation policy governs checkpoint scope and takes precedence
over older blanket pipelines and historical handoff chronology.

- The developer implements the assigned mission, commits coherent changes,
  pushes through the repository wrapper, runs required gates through the
  controller's `verify` helper, and reports checkpoints and finding
  dispositions to the controller.
- The controller alone posts workflow requests, reviews, dispositions and
  status to the PR and writes the committed review records. Developers do
  not edit `docs/agent-reviews/`.
- The reviewer independently assesses the fixed candidate, mission and
  evidence. Agent agreement and passing builds do not establish native
  engine behavior or owner visual acceptance.
- Keep one controller and one developer per worktree, with a sequential
  development/review cycle. Keep the controller version and project
  configuration fixed during the run. The foreground operator must not
  continue editing the same worktree while the headless developer owns it.

Guarded runtime requests, leases, disposable fixtures, the immutable
`KMG_AUTOMATION_BASELINE`, current installation/save protection and exact
transactional restoration apply unchanged. Use the repository's authorized
runtime procedures when native evidence is needed. Human-only observation
procedures do not authorize automated game input or save mutation.

## Prepare and start an ordinary mission

Use the task's assigned worktree, `codex/*` branch and mission. If the task
already has an unenrolled worktree and a local mission commit, continue there.

Before enrollment, ensure all three instruction files listed above exist and
are readable in that worktree. Commit any restored instruction file and the
mission before enrollment so they are included in the reviewed source.
Record a resolved setup gap as resolved in the mission. Restoring this guide
uses documentation and link checks under the validation policy; it does not
require a separate gameplay build or workflow enrollment.

Set the two task-specific paths below to the actual worktree and mission:

```powershell
$ar = 'C:/Dev/agent-review/agent_review.py'
$wt = '<absolute path to this task worktree>'
$mission = '<absolute path to this task mission file>'

python $ar check-project --project kingmaker-gunslinger --repo $wt
```

After `check-project` succeeds and the prepared changes are committed, push
the branch through the guarded wrapper, enroll it, and run the returned
enrollment ID:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File C:/Dev/KingmakerGunslingerLab/codex-policy/Push-KingmakerGunslinger.ps1 -RepositoryRoot $wt
python $ar enroll --project kingmaker-gunslinger --worktree $wt --plan single --mission $mission --mode headless
python $ar run --enrollment <id printed by enroll>
```

The operator starts the enrollment and run; the controller then owns the
headless developer and independent reviewer. Keep the host awake, signed in
and online while the controller runs. Passing model preflight does not
replace checking this repository's instruction files and project setup.

At each checkpoint the developer uses the enrollment's supplied `verify`
command with `--gate focused -- -Filter <name>` and the repository push
wrapper. The controller handles review, corrections and final handoff.

## Inspect, pause and finish

Use `list` to discover enrollment IDs and `status --enrollment <id>` for
details. At final handoff, `status --enrollment <id> --remote` checks whether
the recorded review and archive remain current. `AWAITING_OWNER_REVIEW`
means the owner can review the candidate; it does not authorize merging,
release, tagging or requesting human reviewers.

`stop --enrollment <id> --reason ...` requests a cooperative pause and returns
before an active developer necessarily finishes. Wait for the safe stopping
point and runtime restoration before cleanup. Keep the installed version and
state for continuation. `resume --enrollment <id> --reason ...` resumes the
headless loop itself; do not start a second `run` alongside it. Cancellation
is an operator action for abandoning an enrollment and retaining its history.

An existing installation does not need reinstalling merely to restore this
repository document. Installation or activation of a new controller version
is a separate operator decision between runs; follow the maintained operating
guide.

## Task-specific plans

Ordinary repairs use `--plan single` unless their authorized mission selects
another plan. The separately authorized Black Tentacles mission uses
`black-tentacles`:
an intermediate review after Phases 0-1, followed by final review of the
complete Phase 4 candidate. Both checkpoints require `focused`. Preserve all
36 acceptance IDs, native evidence requirements and the distinction between
mechanically complete, ready for visual review and released. Restoring this
guide does not start that mission.

## Restoration provenance

Restored on November 21, 2026 from the
[PR #194 guide](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/codex/agent-review-workflow/docs/AGENT-REVIEW-WORKFLOW.md),
with the historical archive reference corrected and ordinary-mission startup
made explicit. Integration settings were checked against the
[merged project configuration at 6d7f44a](https://github.com/howardreith/development-process/blob/6d7f44aeaa9122154117d8c7c894ee810b363a41/agent_review/projects/kingmaker-gunslinger.json)
and the repository's current instructions and validation policy. The
maintained controller guide and the enrollment's pinned configuration remain
the references for controller behavior.
