# Testing

Current release preparation is 0.0.119. The mandatory gates are recorded in
[TELEPORTATION-HARDENING-REPORT.md](TELEPORTATION-HARDENING-REPORT.md). All final
runtime gates must use the same clean committed package. The four fresh-process
campaign persistence phases and desktop/gamepad coexistence have development
PASS evidence; final artifact evidence is recorded separately. Historical
Expanded Summoning and 0.0.118 results retain their original counts. Repository validation,
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

The historical Expanded Summoning authorization allowed working-save writes;
it does not apply to this hardening pass. Every pre-existing save, including
`KMG_AUTOMATION_WORKING`, must remain byte-for-byte and metadata unchanged.
Never select or modify `KMG_AUTOMATION_BASELINE`. Only uniquely named saves
owned by the guarded teleportation persistence transaction may be written or
deleted. Mechanical claims
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


## Current teleportation hardening qualification

Run Windows PowerShell 5.1 after deploying the exact committed Build-Local ZIP:

```powershell
.\scripts\Test-TeleportationHardeningPlan.ps1
.\scripts\Test-TeleportationPersistenceTransaction.ps1
.\scripts\Test-TeleportationSaveProtection.ps1
.\scripts\Test-RuntimeScenarioPreflight.ps1
.\scripts\compatibility\Test-FeatureModuleCompatibilityParameters.ps1
.\scripts\compatibility\Test-KingmakerCompatibilityProfile.ps1
.\scripts\Invoke-TeleportationHardeningQualification.ps1 -Scope Coexistence `
  -ExpectedVersion 0.0.119 -DeploymentManifestPath <deployment-json> `
  -PackagePath <tested-package-zip> -Confirm:$false
.\scripts\Invoke-TeleportationHardeningQualification.ps1 -Scope Native `
  -ExpectedVersion 0.0.119 -DeploymentManifestPath <deployment-json> `
  -PackagePath <tested-package-zip> -Confirm:$false
.\scripts\Invoke-TeleportationPersistenceQualification.ps1 `
  -ExpectedVersion 0.0.119 -DeploymentManifestPath <deployment-json> `
  -PackagePath <tested-package-zip> -Confirm:$false
.\scripts\Invoke-TeleportationHardeningQualification.ps1 -Scope Boundary `
  -ExpectedVersion 0.0.119 -DeploymentManifestPath <deployment-json> `
  -PackagePath <tested-package-zip> -Confirm:$false
```

The scopes cover 4 coexistence runs, 11 existing native runs, 4 persistence
phases and 26 module states. One additional original-configuration startup
restores a third-party generated blueprint diagnostic through its native writer;
no third-party code or settings are edited. All 46 launches require fresh Steam
processes, exact artifact identity, protected save inventories and complete
settings/Mods restoration. The same scripts fail closed on unknown sidecars.
Focused harness checks: 9 plan, 11 settings/sidecar, 6 save-protection, 283
preflight and 4,129 module parameter/settings assertions. See the
[persistence procedure](docs/TELEPORTATION-PERSISTENCE-QUALIFICATION.md) and
[UI coexistence procedure](docs/TELEPORTATION-UI-COEXISTENCE.md).

## Historical 0.0.118 teleportation checkpoints

The guarded `disposable-teleportation-casting` scenario extends the same named
working-save context fixture. It selects a native destination, invokes the
appended source button and the native confirmation buttons, then observes the
production resource transaction, outcomes and relocation. The request-local dice
sequence is bound to those rows only. It exercises cancellation, duplicate
callbacks, source labels/layout, prepared Teleport, spontaneous Greater Teleport,
pre/post-capital Recall, graph-ranked alternatives and repeated native damage.
The fixture restores map records, ledger, temporary books and damage without a
save write; it requires automatic process exit. Never use the baseline save.

This exact request also records bounded native deserialization errors and main
character/cross-scene PostLoad state in `teleportation-save-load.json`. Its five
request-owned hooks and error listener must be removed, with native JSON settings
retained. The extended casting control passes 44 assertions. No diagnostic hooks
exist during normal play. The current Working archive contains Craft Magic Items
blueprint data and cannot qualify isolated profiles that omit that dependency;
its isolated load failure is diagnosed in the implementation report. Do not strip
saved records to make a profile pass. At that historical checkpoint native loading changed LoadedTimes. The current
hardening loader suppresses only that exact header update/commit protocol and
holds read-only Windows leases on every pre-existing save.

```powershell
.\scripts\Invoke-KingmakerRuntimeTest.ps1 -Scenario disposable-teleportation-casting `
  -ExpectedVersion 0.0.118 -SaveName KMG_AUTOMATION_WORKING `
  -AllowDirtyGit -ExitAfterCompletion:$true -Confirm:$false
```

Native associated pets have separate traveler evidence below. Mod-provided mounts,
full campaign disk persistence and full compatibility profiles still require
qualification. Native spellbook/level-up UI results are recorded below. Run IDs and exact limits are in the implementation report.

The feature branch adds the twelfth default-ON module, `teleportation-spells`,
using settings schema 11. The fast domain settings/publication matrix covers
4,096 combinations; the guarded runtime boundary matrix contains 26 states.
Historical release results retain their original module and assertion counts.
All 26 current boundary states passed 897 assertions at the exploration-guard
checkpoint, including desktop, gamepad, arrival and exploration hook counts.
Historical results retain their original counts. The native destination-panel observation is documented in
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
  -ExpectedVersion 0.0.118 -SaveName KMG_AUTOMATION_WORKING `
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
  -ExpectedVersion 0.0.118 -SaveName KMG_AUTOMATION_WORKING `
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
  -ExpectedVersion 0.0.118 -SaveName KMG_AUTOMATION_WORKING `
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
  -ExpectedVersion 0.0.118 -SaveName KMG_AUTOMATION_WORKING `
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
  -ExpectedVersion 0.0.118 -SaveName KMG_AUTOMATION_WORKING `
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
  -ExpectedVersion 0.0.118 -SaveName KMG_AUTOMATION_WORKING `
  -AllowDirtyGit -ExitAfterCompletion:$true -Confirm:$false
```

Evidence is `teleportation-gamepad.json` in the named runtime directory. A
successful compile or controller-mode flag alone does not qualify this adapter.
Run `20260908T1010179638483Z-9fc7ced7478444cf954b1bf08ce52cc1` passed all 39
assertions with zero exceptions/save writes; the implementation report records
rejected probes, corrections, desktop regressions and exact result directories.

### Native Teleportation spellbook and action-bar qualification

`disposable-teleportation-spellbook-ui` loads only the named disposable
`KMG_AUTOMATION_WORKING` save through the guarded Steam workflow. It opens the
native local-area service-window spellbook, uses native class/level toggles and
pagination, selects project spell rows, opens their native descriptions, and
prepares real spells through those rows. An ordinary native Dimension Door is
the positive control for native action-bar auto-fill. Project spells must remain
absent from auto-fill and unusable in the local area. The request-local real
Wizard, Sorcerer, Cleric and Druid books, action-bar state, selection and pause
state are restored before the save-write sentinels close. Automatic exit is
mandatory. This does not complete level-up or establish campaign disk persistence.

```powershell
.\scripts\Invoke-KingmakerRuntimeTest.ps1 `
  -Scenario disposable-teleportation-spellbook-ui `
  -ExpectedVersion 0.0.118 -SaveName KMG_AUTOMATION_WORKING `
  -AllowDirtyGit -ExitAfterCompletion:$true -Confirm:$false
```

Read `teleportation-spellbook-ui.json` and `runtime-result.json`, including native
UI exceptions and deferred cleanup assertions. Compilation is not qualification.

Qualified run: `20260908T1116293771376Z-9db2a72926a64a2aaba36bc40c295800`,
31 assertions PASS, zero fixture UI exceptions and zero save writes. The native
copy-mode fixture setup is followed by a real selection refresh before checking
auto-fill. Native close preserves the fixture's Pause mode; the captured surface
is inactive, alpha 0 and blocks no input. Same-artifact working-save regression
`20260908T1119401911470Z-fd2dafbefbba4ea987af7857b8e42855` passes 11 assertions.
The implementation report records the rejected first probe and startup-only
ZFavoredClass diagnostics; this is not complete compatibility qualification.

### Native Teleportation level-up preview qualification

`disposable-teleportation-level-up` uses the same guarded working-save load and
mandatory exit. Request-local real Wizard/Sorcerer books sit immediately before
the native caster-level thresholds for fifth- and seventh-level spell choices.
A temporary native XP precondition permits opening the normal level-up preview;
no level is committed. Native class, feature, skill and spell-selection rules
unlock the native Spells phase. The actual published spell row is selected, and
only the preview may learn it. The native cancel dialog closes the UI, followed
by cancellation of the exact owned native preview/thread. Fixture books, XP,
resources, class/feature identities, UI state and pause are restored before the
save-write sentinels close. No spell-selection count, phase lock or prerequisite
is overwritten. Results are in `teleportation-level-up.json` and
`runtime-result.json`; runtime qualification is required.

```powershell
.\scripts\Invoke-KingmakerRuntimeTest.ps1 `
  -Scenario disposable-teleportation-level-up `
  -ExpectedVersion 0.0.118 -SaveName KMG_AUTOMATION_WORKING `
  -AllowDirtyGit -ExitAfterCompletion:$true -Confirm:$false
```

Qualified run: `20260908T1207363385307Z-e204a1b706bf41b6891074349d6bc571`,
18 of 18 assertions PASS. Native Wizard 8->9 / 12->13 and Sorcerer 9->10 /
13->14 caster-level transitions produce the actual fifth-/seventh-level spell
choices; the actual character stays at level 2. Native prerequisite choices
remain confined to the preview. The fixture preserves the proven inactive
main-menu warmup reference in UIAccess, and re-resolves native selector widgets
across deferred refreshes before selecting. It records zero fixture exceptions
and save writes. Spellbook UI (31), real resources (19), and working-save (11)
regressions pass on the same artifact. Exact run IDs/directories and the three
rejected exploratory runs are recorded in the implementation report.

### Special Teleportation destination audit

`disposable-teleportation-destinations` loads only `KMG_AUTOMATION_WORKING` through
Steam and requires automatic exit. It inventories the actual native map, then
uses request-local visited/revealed state for native book-event/component points
and a representative of every stable point type. Native campaign prohibitions
and closed states are never changed. It captures each eligible point's actual
no-source native controls before providing one real Sorcerer book, then selects
its Greater Teleport row and confirms through the native dialog. Exact slot
expenditure and protected relocation state are checked across subsequent frames.
Natively prohibited points must still offer no spell source. Books, map state,
familiarity, time input and party state are restored before save sentinels close.

```powershell
.\scripts\Invoke-KingmakerRuntimeTest.ps1 `
  -Scenario disposable-teleportation-destinations `
  -ExpectedVersion 0.0.118 -SaveName KMG_AUTOMATION_WORKING `
  -AllowDirtyGit -ExitAfterCompletion:$true -Confirm:$false
```

Read `teleportation-destinations.json` and `runtime-result.json`. This scenario
must pass before its selected points are treated as qualified. It does not
establish other campaign restriction states or non-main-map scene support.


Current destination qualification: `20260908T1330037600506Z-cc0ea3d5a53a4123a156a651546ca8c3` passes
all 68 assertions, with zero fixture exceptions or save writes. All 22 actual
casts preserve deferred protected map state. Native Travel releases exploration;
a second control proves malformed saved spell data cannot block ordinary travel.
The fixture opens one adjacent native edge only after all cast checks, verifies
zero traversal and restores it during cleanup. Read the curated point audit and
implementation report for exact IDs, rejected probes and same-artifact regressions.
The combined 0.0.118 domain inventory is 1,550 (retaining released 0.0.117 coverage). All 26 module boundaries passed
897 assertions with exploration hooks 1 ON / 0 OFF and exact settings
restoration. Full campaign disk persistence and required profiles remain open.

### Teleportation disabled world-map interaction

`disposable-teleportation-disabled` uses the actual native destination panel
with Teleportation OFF. Its disposable fixture provides real known spells and
available native slots, plus existing familiarity and exploration-boundary
fields. Three native selection/dismissal cycles must show no magical rows,
modal or resource changes. The native Travel button must start one route, spend
no spell and complete ordinary arrivals without changing either saved field.
The fixture restores the original UnitPart presence/data, books, map, roster,
time and UI; save-write sentinels remain armed throughout.

Use the settings transaction wrapper with one explicit OFF configuration:

```powershell
.\scripts\Invoke-FeatureModuleRuntimeMatrix.ps1 `
  -Scenario disposable-teleportation-disabled `
  -Combination on-on-on-on-on-on-on-on-on-on-on-off `
  -ExpectedVersion 0.0.118 -AllowDirtyGit `
  -ExitAfterCompletion:$true -Confirm:$false
```

The wrapper selects only `KMG_AUTOMATION_WORKING` and restores the exact original
settings bytes. Boundary/multiple configurations, Teleportation ON and missing
automatic exit are rejected for this scenario. Read `teleportation-disabled.json`
and `runtime-result.json`; this live-state probe does not establish campaign
disk save/reload persistence.

Qualified OFF run: `20260908T1454168314185Z-ccf0393dbee34f8d936bf1d837c4c1be`,
8 of 8 assertions PASS, zero fixture exceptions/save writes and exact cleanup.
The same artifact passes enabled desktop (29), gamepad (39) and working-save (11)
regressions. The implementation report records exact run IDs, paths and hashes.
Launcher preflight has 213 checks and compatibility/settings guards have 4,129.

### Native owner-graph familiarity round trip

The level-up scenario now supplies two request-local saved fields on the canonical
main character. The native UI must serialize the full UnitDescriptor and its
UnitPartsManager, deserialize a separate preview, run native PostLoad, and retain
both fields through subsequent preview rebuilds and spell selection. The probe
reads the actual native `LevelUpPreviewThread.s_Source` token and reports only the
matching part's path/type/property names and fixture values. It verifies the
preview part has the preview owner and is independent of the original; clearing
its arrival boundary must leave the original part and serialized token unchanged.
Native cancellation must clear the owned source and restore the original part's
presence/data. This extends `disposable-teleportation-level-up`; no disk save is
written and campaign disk persistence remains a separate gate.

Qualified owner-graph run: `20260908T1512053583403Z-53c1d3737d794e4ead9db0053421c19e`,
31 assertions PASS (the previous 18 spell-choice/cancel cases plus 13 owner-graph
checks). Native spellbook UI (31) and protected Working-save (11) regressions pass
on the same artifact, with zero fixture exceptions/save writes and exact cleanup.


## 0.0.118 owner-authorized release

The combined source passes 1,550 domain tests, clean Release, strict 135-file
package validation, 279 launcher preflight checks and compatibility transaction
contracts. Same-artifact native casting (44), spellbook (31) and controller (39)
checks pass 114 assertions through Steam App ID 640820. Exact run IDs and hashes
are in the implementation report's 0.0.118 section. The owner authorized release
and will test the remaining campaign/compatibility behavior in a high-level game;
full original qualification is not claimed. UMM 0.33.0.0 is the observed host.


The published 0.0.118 binary subsequently passed 44/44 guarded casting assertions
on clean commit `b439f5df22e2260322453c069b23eaee734b8a33`, run
`20260908T1743365922333Z-56147b3233b54d74b48e4187630715b0`.
The downloaded public ZIP matches the exact runtime package, both deterministic
release builds and all 135 installed files; settings are unchanged. See the
implementation report's publication addendum for hashes and result paths.

## Teleportation fresh-process persistence

The post-release repair adds a four-process native disk gate with protected
pre-existing saves and transaction-owned A/B/C saves. See
[the guarded procedure](docs/TELEPORTATION-PERSISTENCE-QUALIFICATION.md) and
[the development evidence report](TELEPORTATION-HARDENING-REPORT.md).
Run `scripts/Test-TeleportationSaveProtection.ps1` and
`scripts/Test-TeleportationPersistenceTransaction.ps1` before its first native
launch. Use Windows PowerShell for runtime/profile workflows; no manual save
selection is needed. A final release must repeat the disk gate on its own exact
committed artifact.

Guarded additive desktop/gamepad foreign-action regression and exact cleanup:
[Teleportation UI coexistence](docs/TELEPORTATION-UI-COEXISTENCE.md).
