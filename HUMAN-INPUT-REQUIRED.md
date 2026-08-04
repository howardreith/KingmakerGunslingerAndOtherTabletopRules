# Human input required

## Exact blocker

All deterministic, build, package, guarded runtime, comprehensive, and
working-save gates for `0.0.64` pass. The only remaining acceptance claims
depend on human perception: live doll socket/scale/orientation and visible mesh
suppression, projectile appearance, audible firearm quality/exact layering, and
final rendered UI readability/refresh.

## Evidence

- 880/880 deterministic tests pass.
- Comprehensive fresh-process PASS pair:
  `20260804T0127055727006Z` / `20260804T0128458736571Z` (190 assertions each).
- Canonical working-save PASS pair:
  `20260804T0130335785001Z` / `20260804T0132153365435Z`.
- Structural presentation and asset observers:
  `20260804T0124121027946Z` / `20260804T0125380338184Z`.
- Package/DLL/bundle SHA-256: `3aca9eab...45b9d` /
  `2864feb2...6c242` / `D902F279...FBFD`.
- Exact qualified commit: `f3f3ab0ff713e4992ec5eaa96fab280fe13daa3d`.
- Native combat-log notifications are runtime-proven for ordinary, Dead Shot,
  Scatter Shot, Quick Clear, Repair, and Overhaul condition transitions.

## Why autonomous evidence cannot resolve it

Repository safety rules explicitly prohibit treating Computer Use,
screenshots, OCR, mouse coordinates, or automated visual navigation as
mechanical runtime correctness. The remaining items are perceptual rather than
machine-state invariants, so further reflection or synthetic playback would
manufacture confidence rather than establish the observed player experience.

## Smallest precise question

Does the `0.0.64` candidate pass every box in
`FOURTH-PLAYTEST-VISUAL-ACCEPTANCE-CHECKLIST.md` during one supervised session?

## Choices

1. PASS every box and record concise notes; the repair can be promoted from
   functionally qualified to fully qualified.
2. Record each failed box and its exact visible/audible symptom; resume the
   repair from those observations.

Recommended: run the single consolidated session and record PASS/FAIL for all
boxes without saving the game.

## Exact continuation command

```powershell
.\scripts\Invoke-KingmakerRuntimeTest.ps1 `
  -Scenario working-save-smoke `
  -ExpectedVersion 0.0.64 `
  -SaveName KMG_AUTOMATION_WORKING `
  -ExitAfterCompletion:$false `
  -Confirm:$false
```
