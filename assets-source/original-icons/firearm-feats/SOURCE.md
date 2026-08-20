# Firearm feat icon source

These six icons are original project-owned procedural artwork created for the
overnight firearm-feat presentation repair. No Kingmaker, Wrath, third-party,
or downloaded pixels or font files are included.

`icon-spec.json` is the editable art specification. It fixes the 64-by-64
canvas, exact monograms, deterministic wear seeds, and the Windows system font
family used only during rendering. `tools/New-FirearmFeatIcons.ps1` contains
the editable vector-like drawing operations, muted parchment/oxblood palette,
border treatment, lettering paths, reload arrow, and ramrod symbol.

Run the exporter from the repository root:

```powershell
.\tools\New-FirearmFeatIcons.ps1
```

It deterministically writes five distinct firearm monograms and the Rapid
Reload icon to `assets/game/icons/`, writes a 64/32-pixel inspection map beside
this record, and refreshes `SHA256SUMS.txt`. Palatino Linotype is a standard
Windows system font used at build time only; no font binary is copied or
redistributed. Final aesthetic acceptance remains a real in-game UI check.
