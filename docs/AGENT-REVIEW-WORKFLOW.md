# Agent review workflow in this repository

This repository is onboarded to the machine-level agent-review workflow:
Claude Opus (high effort) develops, Codex GPT-6.2 Astra (`gpt-6.2-astra`, high
effort) independently reviews, and a local controller keeps the exchange on a
GitHub draft PR. The workflow's source, operating guide and behavioral
specification live in
[howardreith/development-process](https://github.com/howardreith/development-process)
(`agent_review/README.md`, `docs/GITHUB-AGENT-REVIEW-PROTOCOL.md`); it is
installed at `C:\Dev\agent-review\`.

The workflow was first piloted here on PR #194; that review history is
archived in `docs/agent-reviews/pr-194/`.

## Repository integration

Project config: `agent_review/projects/kingmaker-gunslinger.json` in
development-process (project ID `kmg`).

| Setting | Value here |
|---|---|
| Branches | `codex/*`, matching the guarded push wrapper |
| Push | `C:/Dev/KingmakerGunslingerLab/codex-policy/Push-KingmakerGunslinger.ps1` (developer checkpoints and controller archive commits); the controller's built-in push helper is refused for this project |
| Gate | `focused`: `scripts/Invoke-KmgGate.ps1 -Filter <name>`, run by the controller on the committed candidate (`verify --gate focused -- -Filter <name>`) |
| Instructions given to both agents | `AGENTS.md`, `docs/VALIDATION-POLICY.md`, this document |
| Developer extras | `dotnet build/test`, portable/net47 domain test runners, `python -m unittest`, `scripts/Invoke-KmgGate.ps1`, the guarded runtime entry points `Invoke-KingmakerRuntimeTest.ps1` and `Complete-KingmakerRuntimeLease.ps1`, and the push wrapper |
| Developer denials (beyond the generic base) | `Deploy-Local.ps1`, `Publish-*`, `Restore-Live-Mod.ps1`, `Backup-Live-Mod.ps1`, `Launch-Kingmaker.ps1`; edits to `docs/VALIDATION-POLICY.md` and `GamePath.props` |
| Protected runtime | All six shared Kingmaker lab transaction locks (including `compatibility-state/compatibility.lock`): while any exists the controller never terminates the developer and never declares a handoff |
| Private state | `C:/Dev/KingmakerGunslingerLab/agent-review-state/` |
| Records | `docs/agent-reviews/pr-<N>/` (controller-owned; developers never edit them) |

The validation policy, runtime leases, disposable fixtures, immutable
`KMG_AUTOMATION_BASELINE` and transactional restoration apply unchanged to
headless runs. Agreement between the developer and reviewer never establishes
native engine behavior.

## Starting Black Tentacles

The mission stays at `C:\Dev\KingmakerGunslingerLab\BLACK-TENTACLES-MISSION.md`
(hash-pinned at enrollment). The `black-tentacles` plan adds automated review
after Phases 0–1 (`phase-0-1`, intermediate, advances automatically) and at the
complete Phase 4 candidate (`final`), keeping all 36 acceptance IDs and the
mission's distinction between mechanically complete, ready for visual review
and released; both checkpoints require a passing `focused` gate receipt.

```powershell
$ar   = 'C:/Dev/agent-review/agent_review.py'
$repo = 'C:/Dev/KingmakerGunslingerLab/repo/KingmakerGunslinger'
$wt   = 'C:/Dev/KingmakerGunslingerLab/worktrees/black-tentacles'
git -C $repo fetch origin
git -C $repo worktree add -b codex/black-tentacles $wt origin/master
Copy-Item C:/Dev/KingmakerGunslingerLab/BLACK-TENTACLES-MISSION.md "$wt/planning/BLACK-TENTACLES-MISSION.md"
git -C $wt add planning/BLACK-TENTACLES-MISSION.md
git -C $wt commit -m "Add Black Tentacles mission contract"
Push-Location $wt; powershell.exe -NoProfile -ExecutionPolicy Bypass -File C:/Dev/KingmakerGunslingerLab/codex-policy/Push-KingmakerGunslinger.ps1; Pop-Location
python $ar enroll --project kingmaker-gunslinger --worktree $wt --plan black-tentacles --mission C:/Dev/KingmakerGunslingerLab/BLACK-TENTACLES-MISSION.md --mode headless --title "[draft] Black Tentacles"
python $ar run --enrollment <id printed by enroll>
```

Configured models: developer Claude `opus` (resolved to `claude-opus-5-5` on
2026-11-20) at `high`; reviewer Codex `gpt-6.2-astra` at `high`. Inspect with
`status --remote`, pause with `stop`, continue with `resume --reason ...`,
abandon with `cancel`.

Other missions in this repository use the same sequence with `--plan single`
(or a new plan) and their own mission file.
