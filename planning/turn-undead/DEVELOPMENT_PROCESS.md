# Development process for the Turn Undead mission

Prepared 2026-11-27. This is an operating guide for the accompanying Turn Undead mission. It does not enroll a run, authorize a controller upgrade, alter an existing mission, grant a budget extension, or implement any gameplay code.

## 1. Verified workflow baseline

Repository: `howardreith/development-process`.

Inspected `main` commit: `a6aa2b1fcfa0f946d94183acca36b2b5ddd04c0b` (2026-11-23, foreground-development PR #7). The main branch was checked directly while preparing this packet. A later reviewed installation may be used, but record its identity and follow its actual supported interfaces.

Read these current installed/source documents before enrolling:

1. `agent_review/README.md`: maintained operating guide.
2. `docs/GITHUB-AGENT-REVIEW-PROTOCOL.md`: behavioral specification.
3. `agent_review/projects/kingmaker-gunslinger.json`: project constraints and gates.
4. Target repository `AGENTS.md`, `CLAUDE.md` if present, `docs/VALIDATION-POLICY.md`, and `docs/AGENT-REVIEW-WORKFLOW.md`.

The inspected workflow uses **Claude Opus at high effort as the developer and Codex gpt-6.2-astra at max reasoning effort as the independent reviewer**. These are explicit existing owner policies, not model suggestions. Do not downgrade either role to work around limits.

**Foreground development is the default.** The real interactive Claude conversation receiving this mission is the developer. It performs implementation visibly, invokes the controller for review, reads the accepted findings, and continues corrections in the same conversation. A separate hidden/headless developer is not selected by this packet.

The controller binds foreground enrollment to `CLAUDE_CODE_SESSION_ID` and the CLI-recorded transcript. Do not manufacture environment values, session IDs, transcript entries, models, effort, or permission-mode evidence. A clean provenance check has `foreground_session.problems: []`. `bypassPermissions` is refused. Normal supported interactive permissions remain in force.

## 2. Isolate this feature from existing missions

The target is `howardreith/KingmakerGunslingerAndOtherTabletopRules`, target branch `master`, project name `kingmaker-gunslinger`, project ID `kmg`. Its configured feature branch pattern is `codex/*`, even though Claude is developing.

Before creating a worktree:

- Discover the actual host, clone, installed controller, its version manifest, Git remotes, current default-branch tip, and worktrees.
- Run the controller's read-only `list` and inspect any relevant enrollment with `status`.
- Preserve existing active Gunslinger/Tabletop work, working directories, pending changes, claims, locks, review records, and installed game files.
- Use a new dedicated branch and worktree, for example `codex/turn-undead`, if that name is unused. A different unused suffix is routine; changing an existing enrollment is not.
- Do not attach this mission to a running remediation PR, reuse its budget, cancel it, amend it, or delete its ownership claim.
- Game transactions share locks across the user's Kingmaker labs. Worktree isolation does not authorize overlapping runtime transactions. Respect the configured locks and wait for a safe boundary without terminating another run.

Use the actual verified paths. Historic paths such as `C:/Dev/agent-review/agent_review.py` and `C:/Dev/KingmakerGunslingerLab/` are discovery leads, not proof that a machine or installation is currently available.

## 3. Keep the PR and review scope bounded

The inspected controller supports **one branch and one draft PR per enrollment**, with intermediate checkpoints inside that PR. An intermediate checkpoint does not create a separate PR. It has no documented scope-unit or stacked-PR orchestrator.

For this bounded feature, the proposed structure is one focused Turn Undead PR with:

1. An early checkpoint after the native contracts, undead classification approach, resource/UI design, and a small working native turning path are established.
2. A final checkpoint covering the complete feature and acceptance evidence.

This is an implementation planning choice, not a claim that the controller enforces PR size. Keep Command Undead, Control Undead, unrelated summoning repairs, and controller development out of the branch.

If discovery shows a genuinely independent prerequisite that deserves a separate PR, record the bounded dependency and proposed separate mission. Initiate another enrollment only when that additional work is covered by the owner's instructions. Do not silently convert checkpoint approval into a merge, change an enrolled PR's target, reset review limits by slicing the same failed work into new enrollments, or grow this feature into a general undead framework.

The accompanying optional `turn-undead.plan.json` uses the real `agent-review.plan.v1` format. Its custom plan must first be placed in a trusted plan directory. In the inspected controller, an arbitrary JSON path from the extracted packet is refused: accepted directories are the installed `plans` directory and `~/.agent-review/plans`. Use the user-local directory for a mission plan; do not edit the pinned installed controller package. For the name used below, copy the packet's `turn-undead.plan.json` to `~/.agent-review/plans/turn-undead.json` (the registered name is `turn-undead`). If a same-named plan already exists, compare it and use a distinct name instead of overwriting unrelated content.

## 4. Start sequence

These examples use actual supported CLI arguments. Angle-bracket values are placeholders to replace with observed values. They are not fabricated IDs or ready-to-run machine paths.

```powershell
$turnController = '<DISCOVERED_PINNED_CONTROLLER>/agent_review.py'
$turnRepo = '<DISCOVERED_TARGET_CLONE>'
$turnWorktree = '<NEW_ISOLATED_TURN_UNDEAD_WORKTREE>'
$turnPacket = '<EXTRACTED_PACKET>'

python $turnController list
python $turnController check-project --project kingmaker-gunslinger --repo $turnRepo
git -C $turnRepo remote -v
git -C $turnRepo worktree list
git -C $turnRepo status --short
```

Check the installed controller's `--help` and relevant subcommand help. If genuine model/auth preflight evidence is missing or stale, one bounded `preflight --project kingmaker-gunslinger` is supported; it makes real model calls. Do not repeatedly preflight a verified installation or run the unrelated controller test suite.

Refresh the target ref through the permitted read-only fetch route, then create the dedicated branch from the observed target. For example, after confirming the branch and directory do not already belong to another run:

```powershell
git -C $turnRepo fetch origin master
git -C $turnRepo worktree add -b codex/turn-undead $turnWorktree origin/master
```

Copy the mission and required resources into appropriate repository planning/art locations, preserving their source hashes. Commit a coherent mission/intake checkpoint. Keep credentials, game binaries, native dumps, raw machine logs, and private controller state out of Git.

The repository's configured push route is:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File C:/Dev/KingmakerGunslingerLab/codex-policy/Push-KingmakerGunslinger.ps1 -RepositoryRoot $turnWorktree
```

Verify that wrapper exists and applies to the discovered host. Do not replace it with direct `git push` or the controller's generic `push`: this project explicitly requires its repository wrapper.

Install the optional mission plan in the user-local trusted plans directory under a unique name, or use the shipped `single` plan if the mission deliberately selects only a final review. Do not claim to be using the two-checkpoint plan when enrolling with `single`.

From the genuine developing Claude session:

```powershell
python $turnController enroll --project kingmaker-gunslinger --worktree $turnWorktree --plan turn-undead --mission '<COMMITTED_MAIN_MISSION_PATH>' --mode foreground --title '[draft] Add Turn Undead feat'
```

Capture the returned enrollment ID, PR URL, mission hash, project configuration, plan, and workflow identity. Do not predict IDs. Leave `--max-rounds` and `--state-root` unset unless a current explicit owner instruction requires a different value.

```powershell
$turnEnrollment = '<RETURNED_ENROLLMENT_ID>'
python $turnController status --enrollment $turnEnrollment
```

Enrollment must verify the genuine foreground session and preserve the existing repository identity. If it refuses because of provenance, authentication, an occupied branch, or an incompatible installed workflow, report the precise refusal. Do not patch the controller or manufacture evidence to proceed.

## 5. Development, gates, and independent review

The normal cycle is: implement a coherent checkpoint, inspect the diff, commit, guarded push, required gate receipt, developer report, independent review, then corrections.

The current project requires the `focused` gate at every checkpoint. It invokes `scripts/Invoke-KmgGate.ps1`, and accepts extra arguments matching `-Filter [A-Za-z0-9._-]+`. The configured deadline is 30 minutes. The target repository's policy supplies a 15-minute planning target and 30-minute ordinary validation ceiling; necessary restoration already in progress must complete safely.

Example syntax, after adding and verifying a real focused test group:

```powershell
python $turnController verify --enrollment $turnEnrollment --gate focused -- -Filter turn-undead
```

`turn-undead` is a proposed test filter to establish in the target repository, not a claim that the unmodified repository already contains those tests. Confirm that the filter selects meaningful nonzero tests and the appropriate production build. Run the actual relevant native checks through the approved target-repository procedures. A controller receipt, compile, or domain-test PASS does not prove fleeing AI, channel-submenu behavior, save/load, or in-game icon presentation.

Bind every evidence claim to its correct source commit, DLL/package, observer/fixture, and environment. Reuse an unchanged qualified artifact when appropriate; do not label evidence from old binaries as tests of new code. Follow the mission's acceptance matrix and preserve failing evidence.

Use the installed `developer-report.v1.schema.json`. Required fields are:

- `schema_version: "agent-review.developer.v1"`
- `status` (`checkpoint_ready`, `blocked`, `needs_owner_decision`, or `paused`)
- `checkpoint_id`, `candidate_commit`, `summary`
- `validation`, `dispositions`, `evidence_refs`
- `runtime_restoration` (`not_applicable`, `restored_confirmed`, or `pending`)
- `blocker`, `owner_decision_needed`

Each validation entry carries its actual command, whether required, result, duration, and scope/limits. Each pending finding receives a disposition: fixed, rebutted with evidence, deferred only if optional, or unresolved blocker. Use real commits and evidence; an empty finding list is appropriate only when no findings are pending.

```powershell
python $turnController review --enrollment $turnEnrollment --report '<ACTUAL_REPORT_JSON>'
```

Run long review/gate commands using the environment's supported background facility and observe their live output. In a separate monitor:

```powershell
python $turnController watch --enrollment $turnEnrollment
```

Or tail the actual `live_log` returned by `status`. Keep the owner informed of material progress; a running process alone is not evidence of progress.

Read the accepted review's structured findings and `Continuation for Claude`. Correct root causes in a coherent batch, run the affected reproduction and neighboring checks, submit the updated report, and continue without asking the owner to relay ordinary corrections. Preserve finding IDs and history. Optional polish does not expand the mission.

Only the controller publishes review requests, accepted reviews, dispositions, and review archives. Do not hand-edit `docs/agent-reviews`, issue homemade approval comments, request human reviewers, or treat an informal second opinion as a controller-accepted review.

## 6. Limits, amendments, and failures

At the inspected project snapshot, defaults are:

| Limit | Configured value |
|---|---:|
| Accepted review rounds per checkpoint | 5 |
| Reviewer attempts per round | 3 |
| No-progress rounds | 3 |
| Maximum headless developer dispatches | 30 (not a foreground turn/tool-call ceiling) |
| Publication attempts | 4 |
| Reviewer deadline | 60 minutes |
| Focused gate deadline | 30 minutes |

The configured developer-invocation counter belongs to headless `develop_step` dispatches in the inspected controller. It is not a limit of 30 messages or tool calls in the genuine foreground Claude conversation. Accepted review rounds, reviewer attempts and no-progress limits still apply to foreground review.

Use the limits actually captured by this enrollment. These values are source facts, not permission to extend or reset counters. Other missions' five-, fifteen-, or later-round special authorizations do not apply here.

The controller version and project configuration stay pinned during the run. A new library, controller modification, permission bypass, model switch, budget extension, or cancellation/re-enrollment is not a routine fix for a failing review.

An explicit later owner change to scope/acceptance uses the supported amendment path. Save the owner's exact text in a new durable file outside the worktree, then from the enrolled foreground session between its own completed commands:

```powershell
python $turnController resume --enrollment $turnEnrollment --reason '<ACTUAL_OWNER_UPDATE_CONTEXT>' --amend-mission '<EXACT_OWNER_TEXT_FILE>'
```

Read the complete returned effective task and all required parts, preserve its delivery marker, and cite the returned instruction revision in the next report. An amendment does not alter models, limits, permissions, or gates. Do not invent an amendment, authorization number, owner signature, or approval artifact.

When a model limit, runtime access issue, required check, or no-progress/round cap blocks work, preserve the exact state and return the real status, counters, evidence, and smallest unresolved decision. Do not silently resume after an owner-decision stop, repeat unchanged failing campaigns, or remove locks to gain access.

## 7. Completion is a verified owner-review handoff

After the final accepted review says `ready_for_owner_review`:

```powershell
python $turnController finalize --enrollment $turnEnrollment
python $turnController status --enrollment $turnEnrollment --remote
```

Report completion only when the controller reports `AWAITING_OWNER_REVIEW` and the remote handoff is current. A ready reviewer verdict alone is insufficient: finalization also checks archived records, exact commit identities, current remote state, required hosted checks, and cleared runtime transactions.

If hosted-check inventory or branch-protection access fails, report that failure. An HTTP 403 or unavailable inventory is not evidence that no checks are required. Do not make the repository public, change its plan, weaken controller checks, or bypass finalization.

The final message to the owner must identify:

- Draft PR URL and branch.
- Reviewed source commit and archive commit.
- Which acceptance requirements passed, failed, were blocked, or were not run.
- Native observations and their limitations.
- Icon integration evidence and any remaining subjective owner visual review.
- Review/invocation expenditure and current controller state.
- Restoration confirmation for every runtime transaction.
- Remaining owner action, without claiming merge or release.

This mission ends at reviewable feature completion. It does not authorize merging, release publication, tagging, a public deployment, or replacement of the owner's normal-play installation.

## Pinned sources

- [Workflow repository AGENTS](https://github.com/howardreith/development-process/blob/a6aa2b1fcfa0f946d94183acca36b2b5ddd04c0b/AGENTS.md)
- [Maintained operating guide](https://github.com/howardreith/development-process/blob/a6aa2b1fcfa0f946d94183acca36b2b5ddd04c0b/agent_review/README.md)
- [Behavioral protocol](https://github.com/howardreith/development-process/blob/a6aa2b1fcfa0f946d94183acca36b2b5ddd04c0b/docs/GITHUB-AGENT-REVIEW-PROTOCOL.md)
- [Kingmaker project configuration](https://github.com/howardreith/development-process/blob/a6aa2b1fcfa0f946d94183acca36b2b5ddd04c0b/agent_review/projects/kingmaker-gunslinger.json)
- [Controller CLI and plan loader](https://github.com/howardreith/development-process/blob/a6aa2b1fcfa0f946d94183acca36b2b5ddd04c0b/agent_review/agent_review.py)
- [Developer report schema](https://github.com/howardreith/development-process/blob/a6aa2b1fcfa0f946d94183acca36b2b5ddd04c0b/agent_review/developer-report.v1.schema.json)
- [Generic plan](https://github.com/howardreith/development-process/blob/a6aa2b1fcfa0f946d94183acca36b2b5ddd04c0b/agent_review/plans/single.json)

