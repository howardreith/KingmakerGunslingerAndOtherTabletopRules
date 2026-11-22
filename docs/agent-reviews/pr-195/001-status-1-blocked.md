<!-- agent-review:v1 enrollment=kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704 key=status-1-blocked part=1/1 sha256=7cc63d2d995b3631388321a601bb4f3c8aadc14e534a0035fb0f6cfd938612e0 -->
### Workflow — blocked — checkpoint final

developer blocker: 1. Required focused gate: candidate 288ce4dd07 fails at scripts/build.ps1:41 with "No Kingmaker path is configured". The worktree C:/Dev/KingmakerGunslingerLab/worktrees/spear-bootstrap-tooltip-repair has no gitignored GamePath.props. The developer is denied Write/Edit(./GamePath.props), and the gate accepts only "-Filter <name>". Operator fix: copy C:/Dev/KingmakerGunslingerLab/repo/KingmakerGunslinger/GamePath.props into the worktree root (local config, never committed), then rerun "verify --gate focused -- -Filter elven-branched-spear". The same product code passed locally with -KingmakerInstallDir.

2. Native evidence required by mission §3A.3, §3C.1–2 and §4.1.1, 4.1.3 and 4.1.6:
   - Denied: "powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Build-Local.ps1 -DevelopmentPurpose spear-bootstrap-tooltip-repair -DevelopmentTestFilter elven-branched-spear". It is needed for the development candidate ZIP.
   - Denied: "powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/compatibility/loader-compat/Build-LoaderCompatHarness.ps1". It is needed for the harness that injects the initialization failure and drives character creation, tooltips and the summon menu in the disposable UMM 0.31.1 game copy.
   - Not allowlisted: scripts/compatibility/loader-compat/Invoke-LoaderCompatSession.ps1 (plus the two outputs above).

   An operator-run (or permission-extended) loader-compat session is needed to close these items. No runtime transaction was started.

- Enrollment: `kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704`
- Phase: `develop`; rounds completed for this checkpoint: 0/5
- Inspect: `python C:/Dev/agent-review/agent_review.py status --enrollment kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704`

_Posted by the agent-review controller through the owner's GitHub account. Automated status; not a human review._
