# Agent review setup — acceptance report

Mission: `planning/CLAUDE-GITHUB-REVIEW-SETUP.md` (owner brief), behavioral
specification `docs/GITHUB-AGENT-REVIEW-PROTOCOL.md`, owner addendum
`planning/AGENT-REVIEW-SETUP-OWNER-ADDENDUM.md` (generalize to every repository
under `C:\Dev`). Setup PR: #194 (draft, no requested reviewers), branch
`codex/agent-review-workflow`, base `master` at `2e61e78`.

This report states what was demonstrated with **real models**, what with
**labeled fixture simulations**, and what remains open.

## Environment established (2026-11-20)

| Item | Observed |
|---|---|
| Repository | `C:\Dev\KingmakerGunslingerLab\repo\KingmakerGunslinger`, origin `howardreith/KingmakerGunslingerAndOtherTabletopRules`, default branch `master`, no branch protection or rulesets, no hosted CI |
| Worktree | `C:\Dev\KingmakerGunslingerLab\worktrees\agent-review-workflow` (other 40 worktrees untouched) |
| Claude Code | 2.1.329, claude.ai subscription login (`apiKeySource: none`), config `~/.claude-personal` via the account shim |
| Codex CLI | 0.170.0, ChatGPT login (Pro plan) |
| gh | 2.99.0, login `howardreith`, scopes `gist, read:org, repo, workflow`, ADMIN on the repository — reused, unchanged |
| Push | Existing guarded wrapper `codex-policy/Push-KingmakerGunslinger.ps1` — reused, unchanged |
| Python | 3.14.7 (standard library only; no new dependencies) |

## Model configuration and provenance (WF01)

| Role | Requested | Observed (CLI/API metadata, not model statements) |
|---|---|---|
| Developer | `claude -p --model opus --effort high` (+ `--session-id`/`--resume`) | init event and API `message.model` = `claude-opus-5-5`; session transcript effort = `high` |
| Developer (foreground pilot) | this interactive session | transcript: every main-thread turn `claude-opus-5-5`, effort `high` |
| Reviewer | `codex exec -m gpt-6.2-astra -c model_reasoning_effort="high" --sandbox read-only` | rollout thread settings and every turn: `gpt-6.2-astra` / `high`, sandbox `read-only`, approval `never`, network `restricted` |

Limitations: the Claude init event does not report effort (the transcript
does); Codex does not expose a separate serving-backend model identity;
Claude's cost figure is a list-price estimate on subscription auth and Codex
reports tokens only, so no combined cost is claimed.

## Results

_Filled in at the final checkpoint._
