# Autonomous local runtime testing

This workflow builds and records a candidate deterministically, but live game
interaction remains a controlled human/Computer Use activity. Always use a
disposable campaign and never overwrite the named baseline save.

## Build, backup, deploy, and restore

1. Run `scripts\Build-Local.ps1`. It validates the repository, runs all domain
   tests, compiles Release against the preserved qualified references, creates
   the strict eight-file package, and writes an adjacent Build-Local manifest.
2. Run `scripts\Check-Test-Environment.ps1 -PackagePath <zip>`.
3. Preview backup with `scripts\Backup-Live-Mod.ps1 -WhatIf`; run without
   `-WhatIf` only when live deployment is explicitly authorized.
4. Preview deployment with
   `scripts\Deploy-Local.ps1 -PackagePath <zip> -WhatIf`. Actual deployment
   backs up first and writes only the exact KingmakerGunslinger mod directory.
5. Restore only an explicitly named backup:
   `scripts\Restore-Live-Mod.ps1 -BackupDirectory <directory> -WhatIf`, then
   omit `-WhatIf` only with explicit restore authority.

`Launch-Kingmaker.ps1` uses Steam's normal `-applaunch 640820` mechanism,
records launch time and Steam PID, and performs no GUI automation.
`Wait-For-KingmakerExit.ps1` observes one identified Kingmaker process and
never terminates it.

## Test-run and evidence procedure

Create a unique run with `New-Runtime-TestRun.ps1 -Scenario <name>`. After the
single documented scenario, pass only explicit log and screenshot paths to
`Collect-Runtime-Evidence.ps1`. It copies files, hashes them, and records Git,
package, deployed DLL, and discoverable game-version facts. It never searches
for or collects saves, credentials, browser data, or unrelated user files.

## Outcomes

- **PASS:** every stated prerequisite, action, state/resource delta, isolation
  check, persistence check, and zero-fault requirement is directly observed.
- **FAIL:** any required result is contradicted or a blocking fault occurs.
- **AMBIGUOUS:** evidence is missing, unreadable, contradictory, or cannot be
  tied to the exact package/save/item. Ambiguous is not Pass.

Stop on any unexpected dialog. For an expected UI control, allow at most two
interaction attempts; if it still does not respond predictably, record
Ambiguous and exit normally. Never improvise around credentials, purchases,
cloud conflicts, installations, updates, or campaign-state surprises.

## Human-only responsibilities

Humans retain control of credentials, Steam account decisions, purchases,
cloud-save conflict resolution, update/install approval, selecting the verified
working save, judging visual/combat-log presentation, and authorizing actual
deployment or restore. Computer Use may follow the documented runbook only.

## Emergency recovery

Exit Kingmaker normally. Do not retry mutation or delete files. Record the
failure, identify the exact timestamped backup, preview
`Restore-Live-Mod.ps1 -BackupDirectory <exact> -WhatIf`, then perform the
restore only with explicit authority. Preserve the failed package, manifest,
logs, and screenshots for diagnosis.
