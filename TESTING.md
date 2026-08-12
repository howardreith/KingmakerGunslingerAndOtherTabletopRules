# Testing

The active release is 0.0.78 Expanded Summoning. Repository validation,
dependency-free domain tests, exact-reference Release builds, package checks,
and native runtime evidence are separate gates; none substitutes for another.

Run the static and domain suite with:

```powershell
.\scripts\test-domain.ps1 -Configuration Release
```

Build and validate the strict release package with:

```powershell
.\scripts\build.ps1 -Configuration Release -Clean -Package
```

Native automation must use the guarded request workflow in
`docs/WIN10-AUTONOMOUS-RUNTIME-TESTING.md` and Steam App ID 640820. Expanded
Summoning qualification uses `observe-expanded-summoning-inventory`,
`disposable-expanded-summoning`,
`disposable-expanded-summoning-visual-contracts`, the three
`working-save-expanded-summoning-*` persistence stages, and all 16
`observe-feature-module-settings` configurations.

Only `KMG_AUTOMATION_WORKING` may be written by the authorized persistence
workflow. Never select or modify `KMG_AUTOMATION_BASELINE`. Mechanical claims
come from structured runtime assertions, not screenshots, OCR, or coordinate
automation. Exact run IDs and result paths are recorded in
`EXPANDED-SUMMONING-STATE.json` and the implementation report.

Compatibility transactions must use
`scripts/compatibility/Invoke-KingmakerCompatibilityProfile.ps1`. The runner
stages one committed profile, launches a fresh Steam process, and restores the
Mods directory and feature settings exactly even on failure. Required release
profiles are standalone, Call of the Wild, Arms and Armor, Toggle Custom
Soundpacks, and the highest-risk combined profile; standalone, Call of the
Wild, and highest-risk combined require two fresh PASS runs.
