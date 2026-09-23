# Testing

For 0.0.119, the owner explicitly waived remaining runtime tests and authorized
publication after the completed candidate checks. The original full gate plan
below is retained for future qualification; it is not a claim that every final
release-artifact repeat passed. See the hardening report for exact artifact
boundaries. Run script tests in separate `powershell.exe -NoProfile
-ExecutionPolicy Bypass -File <script>` processes so fixture stubs cannot affect
a later native launch in the calling session.

Current public release is 0.0.119. The mandatory gates are recorded in
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


## Better Vendors progression (0.0.138 candidate)

The optional Better Vendors integration is covered by 38 dependency-free
`better-vendors.*` domain cases in
`tests/KingmakerGunslinger.DomainTests/BetterVendorsProgressionTests.cs`. They
cover:

- the exact 50-entry catalog, reused identities, manifest append, pricing,
  exclusions, module ownership and visual mappings;
- the verified Military schedule;
- stock-call classification and pass-scope unwinding;
- ordinary-query recognition;
- grant planning for current-tier, catch-up, native-selection, module,
  milestone and catalog-expansion cases;
- the write-ahead ledger. Claims are written before stock changes and released
  only on a confirmed no-op. A grant whose outcome is uncertain is never
  granted again by a later catch-up, and a failed claim or count leaves the
  grant eligible;
- a model of the native fixed-row reconciliation, which pins the one-copy
  module-off effect on reused +1 stacks;
- a simulated campaign lifecycle: fresh, existing, both event orders, Better
  Vendors' own first pass, disabling and campaign switching;
- the exact-binary contract gate (file SHA-256 and MVID) and one-time status
  reporting;
- source checks that the hooks stay narrow and read-only, that the only
  removal is the ledger's claim release, that other acquisition paths exclude
  the variants, and that bootstrap registration is unconditional.

These cases model the rules with a dictionary shop and an in-memory ledger.
They do not serialize a save, reconstruct the ledger part, drive the real
trading hook or buy anything, so they are not merchant or persistence
evidence.

The machine-readable catalog must match the code. After building the domain
tests, regenerate it with:

```powershell
.\artifacts\tests\Release\KingmakerGunslinger.DomainTests\KingmakerGunslinger.DomainTests.exe `
  --write-better-vendors-catalog docs\better-vendors-progression-catalog.json
```

The in-game stock behaviour is not yet qualified, and this blocks merge and
release. It needs a save whose kingdom has reached Military I or higher. The
only authorized disposable fixture, `KMG_AUTOMATION_WORKING`, predates kingdom
creation, so Better Vendors' progression never runs in it. Qualifying the
merchant, purchase, event-coordination, settings and persistence paths needs
an owner-authorized, disposable kingdom-stage fixture. Never fabricate one or
reuse a real campaign save. The acceptance areas are listed in
[docs/BETTER-VENDORS-COMPATIBILITY.md](docs/BETTER-VENDORS-COMPATIBILITY.md#acceptance-requirements-before-merge-or-release).
The canonical `working-save-smoke` scenario can still show that the candidate
loads, registers the 43 new blueprints and accepts the installed Better
Vendors binary. Read the `better-vendors` lines in the UMM log for that.


## Rapid Reload proficiency gate

`disposable-rapid-reload-proficiency-gate` is the guarded scenario for the
Rapid Reload firearm-proficiency requirement. It reads the registered parent
and child blueprints and drives the real `LevelUpController` on disposable
units.

Two setup rules make its observations meaningful:

- **Reserved feat slot.** Each case reserves the specific ordinary or Fighter
  combat feat slot it intends to exercise, identified the way the engine
  identifies one (selection blueprint plus the occurrence index
  `LevelUpState.AddSelection` assigns), so it survives the preview rebuilds that
  attribute, skill, class and archetype actions trigger. The filler pass that
  settles every unrelated requirement skips that slot, and the case proves the
  slot is still resolved, still unselected and still of the expected native
  selection before acting. A filler feat in the target slot is a setup failure,
  not evidence of a refusal. Where the reservation is no longer needed — before
  an unrelated final confirmation — it is released explicitly, and only after
  native invalidation has already been observed.
- **Proved fixtures.** Every case states the proficiency scope it was set up
  with next to the fact it grants, and the evaluator that scores it checks the
  measured full/one-handed/two-handed ranks against that declaration. An
  intended grant is never evidence, and a silently different fixture cannot pass.

Coverage:

- **Refusal, both catalogs.** A Fighter with no firearm proficiency (but with
  native martial crossbow proficiency) is offered the parent in the reserved
  slot, `CanSelect` is false, `SelectFeature` is refused and no parent or child
  is held. The reservation is then released, the remaining choices are filled
  legally, and the level is confirmed through the native completion gate — so
  the character demonstrably could finish a normal level-up and still has no
  Rapid Reload.
- **Out-of-scope firearm.** A scoped-proficiency character selects the parent
  legally and is refused the wrong firearm. The resulting empty Rapid Reload
  choice is inspected *before* any cleanup: it keeps `LevelUpState.IsComplete`
  false (the exact check `CharacterBuildController.Next` enforces before it
  calls `Commit`), the Rapid Reload state is named in the blocking set, and no
  fact is banked. The same build then completes once a legal firearm is chosen,
  so the block is attributable to the target selection.
- **Empty selection.** A parent held with no child is applied deliberately
  without native completion. This row is labelled a defensive probe
  (`appliedWithoutNativeCompletion`) and can never stand in for a claimed
  confirmation; it proves only that an empty choice banks nothing.
- **Pending class change.** One live controller: a qualifying Gunslinger build
  holds Rapid Reload (Pistol), then `SelectClass` moves the pending class to
  Fighter. The engine rebuilds its own preview and re-checks every pending
  action. Because the parent itself stops qualifying as a Fighter, both the
  parent hold and the exact child must be gone. Eligibility returns when the
  class is changed back, a re-held choice again does not survive, and the final
  Fighter level is confirmed through the completion gate. A second controller
  instance in this evidence is itself a failure, so cancelled visits cannot
  stand in for the transition.
- **Pending archetype change.** The same single-transaction treatment for
  `AddArchetype`/`RemoveArchetype`. Musket Master narrows the scope to
  two-handed firearms and brings the automatic Rapid Reload (Musket) grant.
  **The parent may legitimately survive this change** — it still qualifies
  through two-handed proficiency — so the scenario tracks the exact child
  instead: Pistol must not remain selected, must not be granted, must not be
  eligible and must not be selectable. Both native outcomes are accepted (the
  engine clearing only the invalid child, or removing the whole selection). If
  the valid parent and its nested state remain, that retained state is used for
  the selectability observations and nothing is created or withdrawn; only when
  the transition removed the parent is a probe opened in the reserved slot, and
  only that probe-created selection is withdrawn afterwards. Removing the
  archetype restores the full scope and drops the automatic grant.
- **Musket Master.** Built through the native archetype route with nothing
  granted by hand. The scenario proves Gunslinger levels, Musket Master
  archetype identity, two-handed-only proficiency and the automatic Rapid
  Reload (Musket) grant, then works through the real nested child selection:
  the owned Musket choice cannot consume another feat, Pistol stays out of
  scope, and Blunderbuss is acquired by exactly one feat slot, confirmed
  through the completion gate.
- **Class identity without proficiency.** A genuine Gunslinger is built and
  confirmed through the native completion gate; its firearm-proficiency facts
  are then removed fixture-locally and their absence is re-proved on the
  committed descriptor and again on a live preview that still shows Gunslinger
  levels. Because a second character level offers no new ordinary feat, the
  Fighter bonus combat feat is the reserved slot (the ordinary catalog is only
  the fallback). The parent and every child fail and the parent cannot be
  selected. If native restoration were to put proficiency back, the fixture
  fails as invalid rather than scoring as a negative result.
- Retained coverage: OR-grouped (`GroupType.Any`) parent prerequisites with the
  correct full/scoped proficiency identity, firearm-exact child gates, no
  class or archetype prerequisite, the same gated parent in both feat catalogs,
  compatibility-only Rifle/Revolver choices unpublished, the legacy wrapper
  unpublished but still satisfying the gate, and independent proficiency grants
  on non-Gunslingers qualifying through the ordinary feat route.
- **Combat-feat classification.** The `rapid-reload-combat-feat-classification`
  assertion reads native `BlueprintFeature.HasGroup` on the registered parent
  and every official child, next to native Combat Reflexes. All of them must be
  both `Feat` and `CombatFeat`, and the parent must not be hidden from feat
  menus. Catalog membership alone never satisfies it. A selection's own
  `Group`/`Group2` do not classify it: they only name the category it offers.
- **Fighter combat-feat route.** Scoped refusal (one-handed refuses Musket,
  two-handed refuses Pistol) and scoped acquisition (one-handed Pistol,
  two-handed Musket) also run through the Fighter bonus combat-feat slot.
- **Gunslinger 1 takes Fighter 1.** A Gunslinger level is built and confirmed
  natively, and its class package is the only proficiency source. The next
  visit takes Fighter 1. It must have no ordinary feat slot, and it must offer
  the parent from the reserved Fighter slot's own `ExtractSelectionItems`.
  Pistol is then chosen, the Fighter slot (and no ordinary slot) must hold the
  parent, and the level is confirmed through the native completion gate.

Status of the classification follow-up: domain coverage passes, including a
source regression that fails if the parent's classification is removed or
moved into the generic selection helper.

Native run on commit `40710b6f` (evidence
`runtime-evidence/20260923T1504537002644Z-disposable-rapid-reload-proficiency-gate`,
loaded 0.0.136, installed DLL SHA-256
`077169c47c98bc1d78c700d609f7c55061c5bfdb3a82a9a897adacce4e1b0fc0`) reported
overall **FAIL**. The assertions for this change passed:

- `rapid-reload-combat-feat-classification`, `rapid-reload-catalog-publication`,
  `rapid-reload-parent-proficiency-prerequisites`, the native prerequisite
  matrix, Musket Master and duplicates, the class-identity control, visit
  cleanup, isolation and loaded version all PASS.
- Every acquisition row passes, including the combat-route rows
  `D.combat-one-handed-pistol` and `E.combat-two-handed-musket`, and row J.
  In row J a natively confirmed Gunslinger 1 takes Fighter 1 with no ordinary
  slot. The Fighter slot's own menu offers Rapid Reload and it can be selected.
  Pistol is taken, and only the Fighter slot holds the parent. The level
  confirms natively and grants the child.

That run had three failures. They were pre-existing expectations in the
earlier harness, and at the owner's direction they were corrected in
`0eee3be9`:

- B rows (`empty-selection-granted-a-fact`). Every refusal behavior passed, but
  `parentRankWhileEmpty` was 1, because the engine applies a chosen selection's
  own feature to the preview at once. The check now requires that the refused
  firearm has no rank (`empty-selection-granted-a-firearm`). The empty choice
  must still block completion, and the parent rank stays recorded.
- `B.empty-selection-blocked` (`empty-choice-was-banked`). Native completion
  refused the empty build, and the parent was only banked by the deliberate
  `ApplyLevelup` probe that bypasses that gate. The probe now has to show that
  no firearm is banked (`empty-choice-banked-a-firearm`).
- `pending-class-change`. Final confirmation failed with
  `skillPointsRemaining = -3`, because the fixture spent Gunslinger skill
  points before switching the pending class to Fighter. The shared resolver
  now refunds overspent points through native
  `LevelUpController.UnspendSkillPoint` and records `skillPointsRefunded`.

Passing native run on commit `0eee3be9` (evidence
`runtime-evidence/20260923T1545397015359Z-disposable-rapid-reload-proficiency-gate`,
loaded 0.0.136, installed DLL SHA-256
`c6cccdac914ed59fa4d85d020108588d7d12cfb4ac38cf5a162772bacc9b465c`): status
**PASS**, all eleven assertions. The pending class change refunded three
points, confirmed natively, and left the Fighter without Rapid Reload.

The first runs found two general fixture defects, now fixed. Independent
proficiency fixtures were unregistered, so native `ReapplyFeaturesOnLevelUp`
threw on the rebuilt preview. Disposable character-creation visits also never
settled race, name, portrait, gender or voice, so no build could complete.

Every claimed successful confirmation records `LevelUpState.IsComplete()`
immediately before the level is applied, and the harness refuses to apply an
incomplete build through that path. The detached-unit harness cannot call
`LevelUpController.Commit` itself (it disposes the preview and touches the unit
view), so it enforces the same gate and then calls `ApplyLevelup`, the method
`Commit` calls; the evidence is labelled as exactly that and never as an
exercised UI button.

Fixture lifecycle: `OpenRapidReloadVisit` owns the level-up controller it
creates until initialisation succeeds. If a later step rejects or throws, the
caller's assignment never completed and its `finally` block holds `null`, so the
helper cancels that controller itself before the failure propagates; the
original setup error keeps its stack through a bare rethrow, and a cancellation
that also fails is reported beside it as an `AggregateException`
(`RapidReloadVisitCleanupRules.Compose`) rather than replacing or hiding either
failure. On success, ownership transfers to the caller and nothing is cancelled
twice. The scenario's own `rapid-reload-visit-ownership-cleanup` assertion
drives that failure window with a natively rejected archetype (Musket Master is
not a Fighter archetype) and checks the original error, its stack, that a
controller existed in the window, that the caller never owned it, and that a
successful initialisation returns the same usable controller to the caller.

Caller-owned cleanup is scored, not just described. `CloseRapidReloadVisit`
routes any cancellation failure through `RapidReloadVisitCleanupRules.Report`,
which records the message and full detail on the evidence row **and** adds an
entry to the failure collection that decides the assertion; where a case is
scored by its evaluator instead, `EvaluateCleanup` rejects any row carrying
`cleanupError`. The boundary never throws, so the caller's `finally` still
reaches `unit.Dispose()` and an in-flight body exception is never masked, and a
null controller stays a no-op. The self-check also exercises that exact
boundary: one controller is cancelled cleanly through it, and a second — left
completely intact — has a controlled failure supplied at the cancellation call
through a private injection overload (`cancel` is null on every production
path, which still runs the native `Cancel()`). The injected error therefore
travels the production catch-and-report path without the controller being
damaged to manufacture it. That controller is then torn down natively and
unconditionally, with its outcome collected under its own
`injection.real-teardown` label so a genuine teardown failure fails the
scenario and can never read as the expected injected one, and its fixture unit
is disposed in an independent boundary. This is an **injected** boundary
failure: it is not evidence that the native `Cancel()` throws or that
cancellation executed.

`RapidReloadGateEvidenceRules` scores that evidence, and the domain suite
exercises those rules with truncated and corrupted fixtures
(`rapid-reload-evidence.*`). That is regression coverage for the scoring only:
it proves a run which skipped a native operation, lost a precondition, never
really held its feat slot, used a different proficiency fixture, repaired an
invalid state, used separate cancelled visits, or applied a build the
completion gate refused cannot be scored as a pass. The four
`rapid-reload-gate.*` cases are policy coverage of
`RapidReloadPrerequisiteRules`. Neither set proves the registered blueprints or
the native selection flow.

As of this branch the scenario has **not** been run. The guarded orchestrator
refuses administrator elevation by design and the authoring sessions were
elevated; runtime qualification for this change is blocked, not passed. A green
domain suite does not close any of the runtime findings.

## Current teleportation hardening qualification

Run Windows PowerShell 5.1 after deploying the exact committed Build-Local ZIP:

```powershell
.\scripts\Test-TeleportationHardeningPlan.ps1
.\scripts\Test-TeleportationPersistenceTransaction.ps1
.\scripts\Test-WordOfRecallFavoredClassPersistence.ps1
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
