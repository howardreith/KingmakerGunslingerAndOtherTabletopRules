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

The guarded `disposable-teleportation-casting` scenario extends the same named
working-save context fixture. It selects a native destination, invokes the
appended source button and the native confirmation buttons, then observes the
production resource transaction, outcomes and relocation. The request-local dice
sequence is bound to those rows only. It exercises cancellation, duplicate
callbacks, source labels/layout, prepared Teleport, spontaneous Greater Teleport,
pre/post-capital Recall, graph-ranked alternatives and repeated native damage.
The fixture restores map records, ledger, temporary books and damage without a
save write; it requires automatic process exit. Never use the baseline save.

```powershell
.\scripts\Invoke-KingmakerRuntimeTest.ps1 -Scenario disposable-teleportation-casting `
  -ExpectedVersion 0.0.116 -SaveName KMG_AUTOMATION_WORKING `
  -AllowDirtyGit -ExitAfterCompletion:$true -Confirm:$false
```

Native associated pets have separate traveler evidence below. Mod-provided mounts,
full campaign disk persistence, normal spellbook/level-up UI cases and full compatibility
profiles still require qualification. Run IDs and exact limits are in the implementation report.

The feature branch adds the twelfth default-ON module, `teleportation-spells`,
using settings schema 11. The fast domain settings/publication matrix covers
4,096 combinations; the guarded runtime boundary matrix contains 26 states.
Historical release results retain their original module and assertion counts.
All 26 boundary states passed 845 assertions at source ba32ac2; added gamepad
hooks subsequently passed all ON, all OFF, Teleportation alone ON and alone OFF
checks (134 assertions); a new full matrix has not been claimed. The native destination-panel observation is documented in
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


The guarded `disposable-teleportation-interaction` scenario uses the same named
working save and automatic-exit requirement as the casting probe. It waits for
actual Unity frames, scrolls the native camera to a selected point, and inspects
native panel geometry and callbacks. It exercises native Travel before and after
real spell sources exist, native Escape, forced dialog closure, stale destination
cancellation, native dialog replacement, live/exhausted source rows, and long-list
scrolling. Native movement-start events establish exactly one ordinary route;
real contextual confirmation establishes one magical spell use. Its iterator
restores the fixture even when the outer runner stops early. This scenario writes
no save and cannot be invoked through normal player UI.

```powershell
.\scripts\Invoke-KingmakerRuntimeTest.ps1 `
  -Scenario disposable-teleportation-interaction `
  -ExpectedVersion 0.0.116 -SaveName KMG_AUTOMATION_WORKING `
  -ExitAfterCompletion:$true -Confirm:$false
```

### Disposable teleportation traveler probe

`disposable-teleportation-travelers` uses the same guarded named working-save
load and world-map fixture as the interaction scenario. It temporarily creates
real native leopard pets with reciprocal master relationships and an unowned
nontraveler control. Actual appended spell rows and native Cast confirmation
exercise repeated mishaps, death exclusion, unconscious living travelers and
exact prepared expenditure. Native damage/life events and before/after rosters
are written to `teleportation-travelers.json`. No life-state threshold is
reimplemented. The request restores original health/damage attribution, books,
pet relationships, cross-scene entities, map fields, ledger and prefab identity
under save-write sentinels; automatic exit is mandatory.

```powershell
.\scripts\Invoke-KingmakerRuntimeTest.ps1 -Scenario disposable-teleportation-travelers `
  -ExpectedVersion 0.0.116 -SaveName KMG_AUTOMATION_WORKING `
  -ExitAfterCompletion:$true -Confirm:$false
```

The corrected probe passed 15 structured assertions, including deliberate
protected-state/resource changes, associated native death and unconsciousness.
Exact run IDs and the rejected initial probe are in the implementation report.
An assertion failure remains a failure even when cleanup succeeds. Do not use
it on a personal campaign save.

### Guarded native gamepad destination scenario

`disposable-teleportation-gamepad` first loads the exact desktop working save,
then loads the desktop world map before switching controller mode. Native
loading-UI disposal/load/initialization and the same-area Game.LoadArea path
replace the UI scenes without running old local controls under gamepad mode. It checks the actual scene and
native modal hosts, original destination controls/defaults, native directional
navigation, cancellation and modal replacement, exact prepared/spontaneous
casting, pre/post-capital Recall, live/exhausted sources, long-list reachability
and input-layer cleanup across real frames. An exception observer covers the
controller transition through cleanup. Rendered confirmation text is checked
with native uppercase styling, complete animation and its actual overlay canvas.
The same source/confirmation/transaction/outcome paths are used in normal play.
There is no OS input, controller emulation or pointer-event patch. The fixture
restores books, map fields, ledger and time input; it disposes request-local
native UI contexts and restores controller mode before exit;
save-write sentinels remain armed and automatic process exit is required.

```powershell
.\scripts\Invoke-KingmakerRuntimeTest.ps1 -Scenario disposable-teleportation-gamepad `
  -ExpectedVersion 0.0.116 -SaveName KMG_AUTOMATION_WORKING `
  -AllowDirtyGit -ExitAfterCompletion:$true -Confirm:$false
```

Evidence is `teleportation-gamepad.json` in the named runtime directory. A
successful compile or controller-mode flag alone does not qualify this adapter.
Run `20260908T1010179638483Z-9fc7ced7478444cf954b1bf08ce52cc1` passed all 39
assertions with zero exceptions/save writes; the implementation report records
rejected probes, corrections, desktop regressions and exact result directories.
