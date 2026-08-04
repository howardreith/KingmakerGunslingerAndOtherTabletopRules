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
  `20260804T0021240668392Z` / `20260804T0022573382219Z`.
- Canonical working-save PASS pair:
  `20260804T0024344255556Z` / `20260804T0026123359766Z`.
- Structural presentation and asset observers:
  `20260804T0018308614483Z` / `20260804T0019578736107Z`.
- Package/DLL/bundle SHA-256: `dff6891d...9a02a` /
  `f804c5c1...10b6e` / `D902F279...FBFD`.

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
