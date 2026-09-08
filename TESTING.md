# Testing

The released base is 0.0.116; contextual teleportation qualification is in progress
below. Historical Expanded Summoning gates retain their original counts. Repository validation,
dependency-free domain tests, exact-reference Release builds, package checks,
and native runtime evidence are separate gates; none substitutes for another.

The final presentation gate validates the 77-row project-owned icon manifest
and all 693 visible child placements, asserts zero runtime fallbacks and no
generic SNA I child, and measures Eagle's live vertical renderer bounds against
a Medium humanoid. The checked-in contact sheet is supporting visual evidence;
mechanical acceptance continues to use structured runtime events.

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
`disposable-expanded-summoning-player-path`,
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

`disposable-expanded-summoning-player-path` is the acceptance gate. It learns
the native parent in a real spellbook, resolves each published logical root
through the parent/variant `AbilityData` chain, advances native command,
animation, execution and entity-creation work, and checks exact slot spend,
live-world survival, kind/quantity, template/alignment and cleanup. Directly
granting a Celestial/Fiendish compatibility child is lower-layer evidence only.

The polish acceptance matrix contains 667 visible generated roots (14 Dire
Bat placements are intentionally publication-suppressed), 17 exact
Owlcat-backed Summon Monster split choices, and nine creature-named Summon
Nature's Ally preservation wrappers. `observe-expanded-summoning-inventory` also
checks all 18 menu counts, singleton-before-quantity ordering, five umbrella
suppressions, icon-category differentiation, and unchanged standalone Summon
Elemental roots. The visual scenario measures the approved relative scale
pairs after live view attachment rather than inferring scale from blueprint
metadata.


## Contextual teleportation qualification in progress

The feature branch adds the twelfth default-ON module, `teleportation-spells`,
using settings schema 11. The fast domain settings/publication matrix covers
4,096 combinations; the guarded runtime boundary matrix contains 26 states.
Historical release results retain their original module and assertion counts.
Full twelve-module runtime qualification remains pending the contextual casting
implementation. The native destination-panel observation is documented in
`docs/TELEPORTATION-NATIVE-FORENSICS.md`; it is not spellcasting proof.


The guarded `observe-teleportation-native-contracts` scenario now also checks the
three real strategic blueprints, native list levels, repeated-publication
identity, exact list/cache rollback, and fixture cleanup. It is main-menu-only
and save-free. `observe-feature-module-settings` shares these publication checks;
when Teleportation is OFF it verifies absence and does not invoke a publication
fixture. These checks do not exercise contextual casting or spend spell slots.

The guarded `disposable-teleportation-familiarity` scenario requires the exact
working save and automatic exit. It uses native destination Accept and native
movement with a request-local time input, records ordinary arrivals/revisits,
checks selection/cancel/placement exclusions, round-trips an ownerless UnitPart
payload, and restores tracked fixture state under save-write sentinels. This is
not full campaign disk persistence or contextual spellcasting qualification.

```powershell
.\scripts\Invoke-KingmakerRuntimeTest.ps1 -Scenario disposable-teleportation-familiarity `
  -ExpectedVersion 0.0.116 -SaveName KMG_AUTOMATION_WORKING `
  -ExitAfterCompletion:$true -Confirm:$false
.\scripts\compatibility\Test-FeatureModuleCompatibilityParameters.ps1
```

The latter executes the compatibility runner's actual module validation and
settings assignment across all 4,096 combinations and rejects each missing or
mistyped key, without staging a profile or launching the game. The publication
observer additionally audits actual familiarity patch metadata: two hooks ON,
zero hooks OFF. Full current contextual feature qualification remains pending.

The guarded `disposable-teleportation-resources` probe uses the named working save
and native world-map mode. It temporarily supplies unused native Wizard,
Sorcerer and Cleric books to two active-party owners, learns/prepares project
spells through native book methods, and restores all original books/resources
and casting attributes. It never writes a save or relocates the party.

```powershell
.\scripts\Invoke-KingmakerRuntimeTest.ps1 -Scenario disposable-teleportation-resources `
  -ExpectedVersion 0.0.116 -SaveName KMG_AUTOMATION_WORKING `
  -ExitAfterCompletion:$true -Confirm:$false
```

This proves the resource adapter: absent/known-only/unrested sources, distinct
caster/book pools, linked opposition preparations, exact native spend, duplicate
suppression, verified one-time restoration, stale and exhausted source omission,
and refusal to refund another operation's expenditure. It does not qualify
contextual UI, confirmation, Teleport outcomes or relocation. Those must use the
same production source and transaction adapters in the later casting scenarios.

The guarded `disposable-teleportation-context` scenario verifies the production
current-map/destination/Recall adapter with real temporary native books and exact
capital-state controls. It reads all current native points without creating map
records, changing familiarity, planning travel or spending a resource. It is a
composition checkpoint; rendered rows, confirmation and completed casting remain
separate required gates.

```powershell
.\scripts\Invoke-KingmakerRuntimeTest.ps1 -Scenario disposable-teleportation-context `
  -ExpectedVersion 0.0.116 -SaveName KMG_AUTOMATION_WORKING `
  -ExitAfterCompletion:$true -Confirm:$false
```

The working save predates kingdom creation. This scenario constructs native
kingdom data temporarily using its deserialization constructor, restores the
original absent reference and real spellbook/map/ledger state, and verifies no
save write. It never claims a region or founds a settlement through campaign actions.
