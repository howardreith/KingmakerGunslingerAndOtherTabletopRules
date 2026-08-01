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

- Exact commit `47fb861` passed `mod-load-smoke`, run ID
  `20260801T0435526657821Z-4ba4ea84718947f1a8cfc3de1d6ad76a`.
- Exact commit `47fb861` passed canonical `working-save-smoke`, run ID
  `20260801T0437220565711Z-d8664d7f634542f58d8d95126e90fe51`.
- Working-save evidence directory:
  `C:\Dev\KingmakerGunslingerLab\runtime-evidence\20260801T0437220409268Z-working-save-smoke`.
- Deployed DLL SHA-256:
  `b1422ae9a2aed50a0ae8a8d2d3f4ff0defc5d03d31f142d7a19c57f5eb973d7b`.
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

## Next action

Implement and test a guarded `sprint30-generic-actions` scenario. Reuse
`WorkingSaveSmokeScenario` for exact load and save-write sentinels; only after
its stable fingerprint invoke
`DevelopmentControls.RunMaintenanceQualificationImmediately()`. Record the
fixture's `MaintenanceLoopPassed` result plus marker-first selection and native
Heavy Crossbow isolation. The action is in-memory only and must never call a
save API. Then run full source/package gates, commit, and execute it through
`Invoke-KingmakerRuntimeTest.ps1` with explicit
`-SaveName KMG_AUTOMATION_WORKING`.

## Safety boundaries

Launch only through Steam App ID 640820 and the guarded request mechanism. Use
only `KMG_AUTOMATION_WORKING`; never load or mutate
`KMG_AUTOMATION_BASELINE`. Never save, quicksave, send UI input, or infer a save
from Continue/newest ordering. Stop on ambiguous identity, entitlement, UI,
prerequisite, save-write, or result evidence.
