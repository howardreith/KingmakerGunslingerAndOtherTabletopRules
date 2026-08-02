# Sprint 64 entry criteria: production critical profiles and special-ammunition scope

## Authority and scope

The authoritative production catalog defines Pistol, Musket, Advanced Rifle,
and Advanced Revolver with a x4 critical multiplier and Blunderbuss with a x2
critical multiplier. All five use the native Kingmaker weapon critical
pipeline and retain its confirmation and damage behavior.

Mission section 4.4 requires special ammunition only where it is already
included by the roadmap. The roadmap does not include a special-ammunition
deliverable. Alchemical cartridges occur only in Sprint 52 as an explicitly
absent prerequisite whose future implementation would require its own
authority and qualification. Sprint 64 therefore must not invent paper or
alchemical cartridges.

## Observable contract

- Every production firearm weapon type has native critical edge 20.
- Pistol, Musket, Advanced Rifle, and Advanced Revolver have native x4
  critical modifiers.
- Blunderbuss has native x2 critical modifier while remaining unavailable
  until scatter distance authority exists.
- The production-catalog guard compares the registered runtime blueprint
  fields, not only domain catalog values.
- Native critical confirmation, damage, concealment, and other ordinary
  weapon-pipeline behavior remain unmodified.

## Qualification

Focused source validation must require the five explicit runtime critical
profiles. Repository validation, the complete domain suite, a clean Release
build, strict package validation, and scenario preflight must pass. The exact
assembly must pass mod-load smoke and two independent fresh-process
`production-firearm-catalog` runs.

## Non-goals and failure behavior

This checkpoint does not add ammunition types, change Lightning Reload, alter
critical confirmation or damage, enable Blunderbuss, or patch the native
critical pipeline. Any missing or mismatched runtime field fails the guarded
scenario closed.
