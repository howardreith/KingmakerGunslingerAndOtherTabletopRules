# agent-review: Claude Opus develops, Codex GPT-6.2 Astra reviews

A machine-level development workflow for any GitHub repository under
`C:\Dev`. Claude Opus (high effort) implements a mission; Codex with GPT-6.2
Astra (`gpt-6.2-astra`, high reasoning effort) independently reviews each
checkpoint; a small local controller runs both and keeps the whole exchange on
a GitHub **draft PR**. The owner is involved only for a human decision, a
precise blocker, or the final review. Nothing merges, releases or requests
human reviewers.

Behavioral specification: the owner's *GitHub agent review protocol*
(committed in the first onboarded repository as
`docs/GITHUB-AGENT-REVIEW-PROTOCOL.md`).

## Layout on this machine

| Path | What it is |
|---|---|
| `C:\Dev\agent-review\` | Pinned, installed controller (`agent_review.py`, schemas, prompts, permission base, shipped projects/plans) with `MANIFEST.json`. Installed from a reviewed checkout; never edited in place. |
| `~\.agent-review\projects\<name>.json` | Per-repository project configs written by `init-project` (owner-controlled). |
| `~\.agent-review\plans\<name>.json` | Optional mission plans with intermediate checkpoints. |
| `~\.agent-review\locks\`, `registry\` | Single-writer OS locks and the branch → enrollment registry. |
| `~\.agent-review\state\<enrollment>\` | Private enrollment state (unless a project sets `state_root`). Never committed. |
| `<repo>\docs\agent-reviews\pr-<N>\` | Committed, sanitized review records for each PR. |

Shipped project: `kingmaker-gunslinger` (tabletop rules mod). Shipped plans:
`single` (any mission, one final checkpoint), `black-tentacles`,
`agent-review-setup`.

## Architecture

```text
 owner ──start/stop/resume──▶ controller (python, this host, one writer per branch)
          ┌─────────────────────┼──────────────────────────────┐
          ▼                     ▼                              ▼
  Claude Code -p          Codex exec (read-only)          gh (owner's auth)
  opus / high             gpt-6.2-astra / high            draft PR comments,
  developer worktree      detached snapshot of H          PR body status
  commits + guarded push  structured review JSON          (controller only)
```

- **Controller** owns enrollment, candidate identity (H = reviewed commit,
  B = target tip, M = merge base), model launch settings and provenance checks,
  schema/request validation, the OS-level single-writer lock, idempotent
  publication, retries and round budgets, archive commits and readiness. It is
  the only component that writes to GitHub.
- **Developer** — Claude Code. *Headless mode* (normal missions): the
  controller launches `claude -p --model opus --effort high --session-id <uuid>`
  and later `--resume <uuid>` (never `--continue`), with
  `--permission-mode dontAsk`, the enrollment's pinned permission file, no MCP
  servers and a required structured report. *Foreground mode*: an interactive
  Claude session is the developer and submits reports with `review --report`;
  the report is bound to that session's recorded model/effort.
- **Reviewer** — `codex -a never exec --ignore-user-config -m gpt-6.2-astra
  -c model_reasoning_effort="high" -c windows.sandbox="elevated" --sandbox
  read-only --disable <connectors/agents/browser/...> -C <snapshot>
  --output-schema <schema> -o <result> --json -`, prompt on stdin, in a
  detached worktree of H inside the private enrollment directory.
- **Owner model policy** is pinned in the controller code (`POLICY`): a project
  file cannot select other models. Changing it is a reviewed workflow change.

The host must stay on, signed in and online while a run is active. Nothing
continues after it sleeps or loses access; `resume` reconciles state and
GitHub afterwards.

### Round lifecycle

1. Developer commits and pushes a coherent checkpoint (repository wrapper or
   the built-in guarded push), then reports `checkpoint_ready`.
2. Controller verifies a clean worktree, H = pushed head = PR head (same
   repository, no forks), binds the report to H, fetches B, computes M, assigns
   a request ID and round, and posts **Claude Opus — review request**.
3. Codex reviews the snapshot with the diff (M..H, transcripts excluded), the
   mission snapshot, prior findings and the developer report.
4. Controller validates exit status, complete JSON object, schema
   `agent-review.review.v1`, echoed identity, decision vs checkpoint type,
   findings/evidence consistency (no pass with blocking findings or required
   evidence gaps), prior-finding assessments (unique, pending, compatible with
   the developer's disposition, no deferral of blocking items) and provenance
   from the Codex rollout (model, effort per turn, read-only sandbox, approval
   `never`, no sub-agents). Failed attempts retry with backoff and never count
   as rounds; wrong model/effort or a usage limit stops at once.
5. Before acceptance the controller re-checks PR head, branch head and merge
   base. If they moved, the review is published as HISTORICAL and development
   must report the current candidate. Otherwise it publishes **Codex GPT-6.2
   Astra — review** and updates the findings ledger in one atomic state write.
6. `changes_required` → developer disposes every pending finding (`fixed` with
   commits in the candidate, `rebutted`, `deferred_optional` for non-blocking
   items only, `unresolved_blocker`); the controller posts **Claude Opus —
   finding dispositions**. `checkpoint_passed` (intermediate) advances
   automatically. `ready_for_owner_review` (final) goes to finalize.

### Records and the archive commit

After a ready review of H the controller writes the exact sanitized PR bodies
and each accepted review's envelope to `docs/agent-reviews/pr-<N>/` as archive
commit A (intent persisted first; restartable), pushed through the same guarded
push. Readiness at A requires every commit in H..A to be a non-merge pure
addition of plain files the controller wrote, with matching hashes, for this PR
only. Anything else invalidates the ready result. The final status comment is
the receipt and is not archived, so there is no re-review loop. Records never
contain raw event streams, credentials (full PEM blocks and tokens are
redacted; partial key blocks block publication), local configuration or raw
runtime evidence; home paths become `~`.

## Onboarding a repository

```powershell
$ar = 'C:/Dev/agent-review/agent_review.py'
python $ar init-project --repo C:/Dev/<repo> --name <name> [--branch-prefix claude/] `
    [--allow "npm test" --allow "npx eslint"] [--push-command '["pwsh","-File","<wrapper>"]']
python $ar preflight --project <name>
```

`init-project` reads origin (GitHub only), the default branch, the GitHub
actor and instruction files (AGENTS.md, CLAUDE.md, CONTRIBUTING.md), and writes
`~/.agent-review/projects/<name>.json`. Review the file: add the repository's
focused validation commands to `developer.permissions.allow`, protected paths
to `deny`, and `runtime_lock_paths` if the repository has transactions that
must never be interrupted. Repositories under `C:\Dev\nima` default to the
`~/.claude` Claude config (work account); others use `~/.claude-personal`,
matching the account shim.

## Running a mission

```powershell
git -C C:/Dev/<repo> worktree add -b claude/<feature> C:/Dev/<repo>-worktrees/<feature> origin/<default>
# commit the mission file (or any first change) and push it with the push helper or wrapper
python $ar enroll --project <name> --worktree <worktree> --plan single --mission <MISSION.md> --mode headless
python $ar run    --enrollment <id>      # develop → review → correct → … → owner handoff
python $ar status --enrollment <id> [--remote]
python $ar stop   --enrollment <id> --reason "..."
python $ar resume --enrollment <id> --reason "what changed" [--max-rounds N]
```

Use `--plan <name>` for a plan with intermediate checkpoints. Exit codes:
0 done/awaiting owner, 2 BLOCKED, 3 PAUSED, 4 refused.

`stop` writes a `STOP` file; the developer finishes or restores its current
transaction, commits, pushes and returns `paused`. At the soft deadline (8 h)
the controller requests that stop itself; after the grace period (1 h) it
terminates the developer only if no configured runtime lock exists.

## Permissions

| Surface | Effective permission |
|---|---|
| GitHub | The existing `gh` login is reused (no new tokens, Actions, webhooks or status writes). Tokens are not branch-limited, so the controller enforces repository, PR number, same-repository head, head/base, draft state, expected actor and full-body marker identity. Required status checks are read from branch protection and rulesets and verified on H. |
| Push | Repository wrapper if configured, else the built-in guarded push: enrolled branch only, matches the project's prefix, clean tree, approved origin, fast-forward, no credential patterns in the feature diff, never force. |
| Headless developer | `settings/developer.base.json` + project extras, composed and hash-pinned per enrollment; `--permission-mode dontAsk`. Reads anywhere; edits only inside the worktree; git inspection/add/commit (no amend/no-verify); the push helper for its own enrollment; the project's validation commands. Denied: direct push, `gh`, history rewriting, network fetchers, every other controller subcommand, edits to the workflow, transcripts, `.claude/`, `AGENTS.md`, `CLAUDE.md`. Running tests the developer wrote is inherent to development; the boundary is no blanket shell, no bypass mode and no GitHub writes. Denials are recorded. |
| Foreground session | Recommended owner-added user-level allowances (an agent does not grant itself permissions): `Bash(python C:/Dev/agent-review/agent_review.py <sub> *)` and the matching `PowerShell(...)` rule for `status`, `review`, `finalize`, `archive`, `stop` and `push`, in `~/.claude-personal/settings.json` (and `~/.claude/settings.json` if nima is onboarded). Without them each call simply asks for approval. `run`, `resume`, `enroll`, `activate`, `install` and `init-project` stay owner-initiated. |
| Reviewer | Read-only filesystem sandbox, restricted network, approvals `never`, user config ignored (no MCP servers, plugins, hooks), connectors/browser/computer-use/image generation disabled; any sub-agent call invalidates the review. `~/.codex/config.toml` is untouched. |

## Recovery

| Situation | Behavior | Owner action |
|---|---|---|
| Host restart / crash | OS lock released by the kernel; pending publications reconciled by marker + full body; interrupted developer invocation resumed with a reconcile prompt (its budget was charged at launch); interrupted archive completed from its persisted intent | `resume --reason "host restart"` |
| GitHub outage while publishing | PAUSED with the validated result preserved; resume re-checks freshness and publishes without rerunning the review | `resume` |
| Reviewer failure / malformed output | Up to 3 attempts per round, then BLOCKED; attempt files are numbered monotonically | `resume` (fresh attempt budget) |
| Wrong model/effort, usage limit | BLOCKED at once; never downgrades | Restore access, `resume` |
| Head or merge base moved | Review published as HISTORICAL; new report needed | Reconcile, `resume` |
| Material change after a ready review or handoff | Ready result invalidated; `status --remote` shows a superseded handoff; `resume` reopens review | `resume` |
| Round cap / no progress | BLOCKED with a visible status comment | Decide; `resume --max-rounds N` |
| Developer `blocked` / `needs_owner_decision` | Dispositions published; BLOCKED / PAUSED | Decide, `resume --reason` |
| Operational error (git/gh/process) | Persisted as PAUSED and posted | Fix, `resume` |
| Workflow reinstalled | PAUSED until `activate` records the reviewed version | Review, `activate` |
| Mission, mission snapshot or permission file changed | BLOCKED | Restore or re-enroll |

## Costs and limits

Both roles use existing subscriptions (Claude claude.ai login, Codex ChatGPT
Pro). Usage is recorded per role: Claude `total_cost_usd`/`modelUsage` per
invocation (list-price estimate on subscription auth) and Codex token counts
and plan type per review. No combined cost is claimed. Defaults: 5 review
rounds per checkpoint, 3 attempts per round, 30 developer invocations,
3-round no-progress limit, 8 h soft developer deadline + 1 h grace, 60-minute
reviewer timeout. A first real setup review of ~2,900 changed lines took about
5 minutes and ~1M (mostly cached) input tokens.

## Install, upgrade, disable

```powershell
python <reviewed-checkout>/tools/agent_review/agent_review.py install --dest C:/Dev/agent-review
python $ar activate --enrollment <id>   # for each running enrollment, after reviewing the change
```

`install` refuses while any controller holds a lock. To disable: `stop`
running enrollments, then delete or rename `C:\Dev\agent-review\` and remove
any user-level allowances you added for it. State in
`~/.agent-review\` can be archived or deleted. No GitHub settings, tokens,
Actions or webhooks exist to revoke; Codex configuration was never edited.

## Tests

```powershell
python -m unittest discover -s tools/agent_review -p "test_*.py" -v
$env:AGENT_REVIEW_LIVE = "1"; python -m unittest tools/agent_review/test_live_smoke.py -v   # real models
```

The behavior suite uses real temporary Git repositories and stubs only the
model and GitHub boundaries with labeled simulated responses. The live smoke
runs a real headless Claude Opus developer and a real Astra reviewer against a
harmless temporary repository with simulated GitHub.
