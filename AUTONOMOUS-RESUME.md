# Autonomous Gunslinger resume handoff

## Durable objective

Execute `AUTONOMOUS-GUNSLINGER-MISSION.md` continuously until its complete
definition of done or a listed genuine human-input hard stop.

## Repository state

- Branch: `codex/complete-gunslinger`
- Audited HEAD: `5c92012701873421adff1fc0e127b0b3597c352c`
- Qualified baseline contained: `4f28dcf` runtime implementation and `5c92012`
  documentation.
- Current checkpoint: Sprint 30 runtime qualification.
- Version: `0.0.30`.
- User-supplied worktree inputs `AGENTS.md` and
  `AUTONOMOUS-GUNSLINGER-MISSION.md` must be preserved.

## Last runtime evidence

- Canonical working-save smoke was qualified twice at implementation commit
  `4f28dcf` as documented in `docs/WORKING-SAVE-SMOKE.md`.
- No current-session run ID yet.
- First unproved invariant: generic marker-first Reload/Repair/Overhaul retains
  Sprint 29 behavior and Heavy Crossbow isolation in the exact 0.0.30 assembly.

## Commands already run

- Read mission, roadmap, Sprint 30 report/entry criteria, architecture, source
  and test inventories, version files, and local class/firearm rule headings.
- Verified `codex/complete-gunslinger` descends from the qualified baseline.
- Repository validation passed. Exact Release domain suite passed 611/611.
  Exact private-reference Release build and strict package validation passed.
  Runtime package SHA-256 is
  `b253eaed27bccfd7841ca938032373bb13146984e94c021b1994cbd901397dfd`;
  DLL SHA-256 is
  `5ce1b5bf0d3563648e9fcd9629981c4ee41cf2fb59143df7dedf4f94fbe373de`.

## Next command

```powershell
.\scripts\Invoke-KingmakerRuntimeTest.ps1 `
  -Scenario working-save-smoke `
  -ExpectedVersion 0.0.30 `
  -SaveName KMG_AUTOMATION_WORKING `
  -ExitAfterCompletion:$true `
  -Confirm:$false
```

Then inspect structured evidence and determine whether an existing guarded
scenario proves Sprint 30 feature acceptance or whether a narrow feature
scenario must be added and source-qualified first.

## Safety boundaries

Launch only through Steam App ID 640820 and the guarded request mechanism. Use
only `KMG_AUTOMATION_WORKING`; never load or mutate
`KMG_AUTOMATION_BASELINE`. Never save, quicksave, send UI input, or infer a save
from Continue/newest ordering. Stop on ambiguous identity, entitlement, UI,
prerequisite, save-write, or result evidence.
