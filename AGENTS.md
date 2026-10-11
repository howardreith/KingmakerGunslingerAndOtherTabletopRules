# Kingmaker Gunslinger agent instructions

Read [docs/VALIDATION-POLICY.md](docs/VALIDATION-POLICY.md) before selecting checks.
It is the current owner directive and overrides earlier blanket pipelines and
mission/handoff next-action chronology. Ordinary work ends at a reviewable
development checkpoint; Invoke-KmgGate.ps1 defaults to Focused with a named
filter and production compilation. Broad Sprint/Tranche boundaries are explicit.
Do not resume the old release campaign merely by reading its handoff.

## Product and architecture

Work only on the assigned Tabletop Added Rules/Gunslinger scope. Preserve stable
blueprint GUIDs, released keys and save compatibility. Verify engine contracts
against the actual Kingmaker 2.1.7b references and installed Harmony12/UMM surface;
build/domain PASS never proves native behavior. Do not invent balance decisions
or missing tabletop adaptations; record unresolved design questions for review.

## Runtime and machine safety

- Use repository-owned guarded runtime requests, Steam App ID 640820, runtime
  leases, current package validation and transactional staging/restoration.
- KMG_AUTOMATION_BASELINE is immutable. Only the named disposable working save
  or explicitly authorized isolated fixture may be mutable; all other saves
  and the actual current human installation are protected.
- Snapshot current intake and restore it exactly. Never substitute old version
  pins. Stop on version/save ambiguity, missing prerequisites, DLC entitlement
  failure, Steam authentication, purchases, cloud conflicts or unexpected UI.
- Preserve fresh processes where fixture/save semantics require them. Ambiguous
  evidence is failure/unknown, never PASS. Screenshots support presentation,
  not mechanical correctness; use structured native observations.
- Read the relevant docs/WIN10-AUTONOMOUS-RUNTIME-TESTING.md and save-observation
  procedure when selecting a runtime task. Human-only observation procedures do
  not authorize automatic selection, loading, input or save mutation.
- After an isolated failure, inspect evidence and narrow the reproduction or
  observer. Preserve failure records and safe cleanup; do not repeatedly launch
  unchanged campaigns. Access only assigned labs and required game references.

## Art and review

For player-visible icon changes first read docs/ICON-ART-GUIDE.md,
docs/art/ICON-REFERENCE-INDEX.md and assets-source/original-icons/icon-catalog.json.
Preserve approved families, protected consumers and provenance. Verify affected
exports/presentation; technical PASS does not grant owner visual approval.

## Git and handoff

- Use a dedicated codex/* review branch, preserving all other work and frozen
  artifacts. Never merge/release, rewrite history, force-push, reset or clean
  without explicit owner authorization for that action.
- Do not commit generated packages, raw machine-local evidence, saves, credentials,
  local configuration or proprietary assemblies/assets.
- Record scope, commands, durations, results, deferred checks and exact artifact
  identities when relevant. Update the active resume/state, not old PASS records.
- Push coherent checkpoints from the current worktree through:
  `powershell.exe -NoProfile -ExecutionPolicy Bypass -File C:/Dev/KingmakerGunslingerLab/codex-policy/Push-KingmakerGunslinger.ps1`
- Older rules and chronology are retained in docs/history/AGENTS-before-workflow-reset-2026-10-11.md
  as historical reference. Missions remain gameplay/release contracts, governed
  by the current validation policy for development checkpoints.
