# Autonomous local runtime testing

On Windows 10, use the guarded request/result workflow in
`WIN10-AUTONOMOUS-RUNTIME-TESTING.md`. Computer Use is not a correctness
mechanism because Kingmaker cannot be captured reliably on this platform.
Deployment and the first invocation remain explicitly supervised.

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

`Launch-Kingmaker.ps1` and every real guarded runtime run use Steam's normal
`steam.exe -applaunch 640820` mechanism. App ID 640820 is fixed and validated.
The guarded request flag and its safely quoted evidence-root path follow the
App ID. Automation records the Steam PID, then independently identifies and
records the newly started Kingmaker PID and start time; `steam.exe` is never
treated as the game process.

Launching `Kingmaker.exe` directly is not a valid save-backed qualification
environment because it can prevent Steam DLC entitlement detection. If all
known-good saves display `DLC Required`, stop and classify that as a
launch-environment failure. Do not modify the saves.
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

The `observe-manual-save-load` scenario is deliberately supervised rather than
autonomous. Follow `SAVE-LOAD-OBSERVATION.md`; the human selects
`KMG_AUTOMATION_WORKING`, and the orchestrator sends no keyboard or mouse input.

`observe-save-catalog-provider` is also supervised. Follow
`SAVE-CATALOG-PROVIDER-OBSERVATION.md`; after readiness the human opens the
normal Load Game screen once but never selects or loads a save. The probe
observes the upstream `List<SaveInfo>` provider and never invokes it.

## Emergency recovery

Exit Kingmaker normally. Do not retry mutation or delete files. Record the
failure, identify the exact timestamped backup, preview
`Restore-Live-Mod.ps1 -BackupDirectory <exact> -WhatIf`, then perform the
restore only with explicit authority. Preserve the failed package, manifest,
logs, and screenshots for diagnosis.
# Save catalog observation

The supervised `observe-save-catalog-and-selection` scenario is the only
approved mechanism for discovering the runtime save-catalog contract. See
`SAVE-CATALOG-OBSERVATION.md`. It is observational and does not authorize
autonomous save loading.
