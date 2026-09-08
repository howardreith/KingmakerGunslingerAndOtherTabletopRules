# Contextual world-map teleportation native forensics

Status: initial exact-assembly audit; no casting or relocation runtime qualification yet.

The clean base is `58d9511082af30f1a4ec88c1238ae7ae2b3651c2`, verified against
freshly fetched `origin/master` on 2026-09-07 before creating
`codex/contextual-world-map-teleportation`. Expected `6874dc15` is an ancestor.

Intervening commits are retained: `a788d22` adds the Brown-Fur direct-cast
provider; `e985aad` documents its contract; `636d70b`, `473f83b`, and `dfd5510`
prepare, merge, and record 0.0.115. `1f6e000` adds Salesman firearms and
Protection descriptions; `46a7936` records qualification; `9a8b2f2`, `4e10b64`,
`cae9f79`, and `58d9511` prepare, merge, and record 0.0.116 and its evidence.
The feature retains Salesman publication/rollback, Brown-Fur transactions,
prior blueprint identities, and the newer validation gates. Requested release
0.0.115 is already published; metadata remains 0.0.116 while release-number
reconciliation and feature qualification are pending.

Installed Assembly-CSharp SHA-256:
`3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`.
The existing ignored native IL disassembly reports MVID
`07fa1e4d-8618-41b3-9b8d-faa17d3b26f7`; the guarded inventory rechecks the
executing assembly and records relevant method bodies. Proprietary IL and
raw runtime inventories remain uncommitted. Installed UMM was independently
read as 0.33.0.0; the mission's 0.32.4 environment is not yet qualified.

## Destination interactions

`Kingmaker.Globalmap.GlobalMapLocation.HandleClick` raises
`ILocationSelectionHandler.OnLocationSelect(BlueprintLocation, bool)` after
native movement, reveal, cutscene, and selection checks. It is an observation
target, not a proposed raw-input patch.

Desktop `Kingmaker.UI.GlobalMap.GlobalMapMessageBox.OnLocationSelect` resolves
the exact `GlobalMapLocation` and calls `FillDialogInfoLocation(bool)` or its
resource variant. This is a native destination panel, not an extensible action
collection. It already preserves Travel, Enter, resource actions, Close/OK,
and settlement-circle controls. `FillDialogInfoLocation` owns native route
preview and button states. Augmentation belongs after native composition.

`GlobalMapMessageBox.Accept()` calls `GlobalMapRules.GoToLocationRevealed`
when the target differs from the party point; otherwise it claims the resource
or calls `EnterLocation`. `Hide()` removes the native Esc subscription and
hides the panel. Native Accept and its listeners should remain untouched.
The no-spell path must create no view and perform no extra route calculation.
UI hierarchy and layout still require guarded live observation.

## Relocation and revelation

`Kingmaker.UI.GlobalMap.Teleport.TeleportModel.Go()` checks settlement circles
and calls `GlobalMapRules.TeleportParty(BlueprintLocation)`. `TeleportParty`
finishes/clears travel, raises pawn events, calls
`GlobalMapLocation.OpenOutgoingEdges(null)`, then `UpdatePawnPosition()`.

`OpenOutgoingEdges` unconditionally sets `EdgesOpened`, can explore outgoing
edge segments, and calls `RevealLocation`; it also sets current position when
`StopWhenRevealingNewEdges` is true. Reusing that wrapper would not establish
the mission's no-revelation contract.

The narrower native candidates are `GlobalMapRules.SetCurrentPosition(new
MapPosition(destination))` followed by `UpdatePawnPosition()`. The former
sets canonical `GlobalMapState.PartyPosition`, updates highlights and
`LastLocation`; the latter positions the canonical pawn, scrolls the camera,
and updates party UV state. No mod transform assignment is needed. These
remain candidate contracts pending structured in-game invariant evidence.

## Real spell resources

`AbilityData.SpendFromSpellbook()` follows `ConvertedFrom`, then calls
`Spellbook.Spend(AbilityData, false)`, discarding its Boolean result.
`Spellbook.Spend` calls `SpendInternal` with `doSpend=true`.

`SpendInternal` validates spell level, maximum level and casting attribute.
For spontaneous books it decrements `m_SpontaneousSlots[level]` once.
For prepared books it scans memorized slots in reverse order, matches an
available `SpellSlot.Spell` through native `AbilityData.Equals`, and calls
`SpellSlot.Spend()`. The adapter must verify the exact resource delta.
Items and converted sources must be rejected.

`RestoreSpontaneousSlots(level, count)` adds and clamps to `GetSpellsPerDay`.
Compensation requires a captured exact original count, an unchanged resource
pool, and verified restoration. Prepared-slot linked/opposition behavior and
exact restoration still require further inspection.

## Ordinary arrival and persistence

`LocationData` persists `IsExplored`, `IsSeen`, `EdgesOpened`, `LastVisited`,
`IsClosed`, `IsFake`, and reveal state. There is no ordinary-arrival counter.
`LastVisited` changes during local-area entry/reconstruction and cannot
implement familiarity.

`MapMovementController.MoveAlongEdge` calls `OpenOutgoingEdges` at ordinary
intermediate-point arrivals and the final destination. A narrowly matched
call seam can observe real movement without counting clicks, loading,
map reconstruction, local-area reentry, or magical relocation. Deduplication
and save/load behavior remain live qualification work.

`UnitPartControlledRageSelection` establishes the save-owned serialized-string
pattern. The new familiarity policy has a versioned deterministic stable-ID
encoding, one-time idempotent legacy migration, and malformed-state rejection.

## Pending inventory and reflection seams

Guarded `observe-teleportation-native-contracts` emits blueprint point inventory,
enum kinds, components, candidate native spell lists, icon donors, and method
IL. Its PASS can prove inventory observation only.

Known native kinds are Location, Landmark, HiddenLocation, Waypoint, and
SystemWaypoint. Kind alone does not prove safe placement: current scene anchors
and campaign restrictions must also pass. `LocationRestriction` supplies native
condition and required-companion checks. The deny catalog must be populated
only after the stable-ID inventory audit.

Candidate reflection seams are the panel's private control references, the
private movement method for an exact Harmony call-site match, and native
spell-list cache invalidation. No reflection writes to slot pools or world-map
transforms follow from these findings.

## Build-host reconciliation

The required clean MSBuild build initially rejected UMM because its private
Harmony 2 dependency targets .NET 4.8. The UMM reference now carries MSBuild's
ExternallyResolved metadata: UMM owns that host dependency, while the mod retains
its explicit Harmony 1.2 reference, .NET 4.7 and warnings-as-errors. Both the
standard clean Release/package build and the provenance-checked exact-reference
build pass. The preflight regression script also contained stale positive
0.0.115 requests against the active 0.0.116 guard; those requests now match
the actual base without relaxing the runtime version check.


## Initial guarded observation

Run `20260907T2322166014106Z-3837017401a84cdca40cdffddc59a7f6` passed
three assertions, including round-trip verification of all 706 point rows.
Evidence directory: `20260907T2322165933939Z-observe-teleportation-native-contracts`
under the configured runtime-evidence root. This observed the installed optional
mod profile, not standalone eligibility or contextual casting. No save loaded.

The earlier run `20260907T2315465500907Z-871a1527392f473ebe21ef0c842b786d`
is rejected as evidence: PowerShell 7.6.5 wraps `Write-Output -NoEnumerate` in a
list that fails the launch-result type guard, and the first probe's anonymous
payload was omitted by Kingmaker's default JSON resolver. The probe now uses
an explicit default contract resolver and verifies the persisted point array.
Windows PowerShell 5.1 preserves the launch helper's typed scalar result;
subsequent guarded commands use that host. Neither guard was weakened.

## Foundation checkpoint validation

The initial policy/observer slice passes repository validation, all 1,405 domain
tests, the clean Release build, and strict installable-package validation.
The package is still 0.0.116; no release promotion or gameplay qualification is
claimed. The focused runtime preflight passes 193 checks under PowerShell 7.
Raw build logs and runtime inventories remain outside version control.

Standalone inventory is still pending. Its first profile attempt stopped
before changing the installed mod profile because the compatibility runner's
legacy `examples` directory is absent. The runner also embeds stale 0.0.114
package/version assumptions. These prerequisites require a narrow repair and
separate verification; the installed-profile observation cannot substitute for
standalone or feature compatibility qualification.

## Standalone inventory and compatibility runner repair

Standalone run `20260907T2336260492141Z-78c2851fd60a41468e664a68683e50a8`
passed with 706 persisted point rows and exact game MVID agreement. Result
folder: `20260907T2336260376893Z-observe-teleportation-native-contracts`.
Profile transaction `compat-20260907T233547Z-878395056426` verified restoration
of the original Mods tree, SoundBank and exact settings bytes.

Counts: Location 117; HiddenLocation 102; Landmark 23; Waypoint 321;
SystemWaypoint 143. This proves the inventory, not safe campaign placement.
Oleg's exact ID is `758559f44d15fc844bf30a10a83154d5`; the capital point ID is
`f83de5c382e087b4ab6ce0b7397a2a13`. Native spell lists are Wizard
`ba0401fdeb4062f40a7aa95b6f07fe89`, Travel domain
`ab90308db82342f47bf0d636fe941434`, Cleric
`8443ce803d2d31347897a3d85cc32f53`, and Druid
`bad8638d40639d04fa2f80a1cac67d6b`.

The profile runner now reads version identity from the supplied package and
current repository metadata, passes the exact package to profile entry, and
accepts an explicit local reference root. Standalone requires no optional-mod
reference directory. Wrong or missing package identities fail before entry.
Disposable filesystem tests cover package identity and exact restoration;
AST binding checks verify entry/restore arguments against the actual scripts.
The required clean Release/package build again passed all 1,405 domain tests.

An intermediate standalone run passed observation but failed automatic cleanup
because the initial runner edit passed entry-only arguments to restoration.
Transaction `compat-20260907T233119Z-678d60cfc67c` was restored explicitly with
`restorationVerified=true`; the corrected complete run above supersedes it.
No gameplay qualification is inferred from any inventory observation.

## Contextual policy and transaction checkpoint

The dependency-light service layer now covers positive destination eligibility,
exact recall destination IDs, real-source availability facts, stable source
ordering/deduplication, native action collection preservation, alternate-distance
preference and severity buckets, and an idempotent selected-source transaction.
It does not yet install point-action patches or publish the three spells.

Outcome resolution applies the locked table, repeats mishap damage through a
per-traveler damage adapter, retains a single expenditure, and stops at the
high defensive limit. Exact spells perform no destination/damage roll. Resource
compensation requires proven exact expenditure before any material effect or
resolved rules result. Recording diagnostics happens after the result is
retained, so a logging exception cannot refund a legitimate no-alternate failure.

All 1,428 domain tests, repository validation, clean Release build, and strict
package validation pass. Guarded Steam working-save regression
`20260908T0002218529266Z-95161ed64ae242d38f4dc117ebb8a9d3` passes 11 assertions;
result directory `20260908T0002218529266Z-working-save-smoke`. This proves the
existing working-save path remains functional for this artifact. It does not
prove live contextual casting, which remains pending native adapters and
feature qualification. Release metadata remains 0.0.116.


## Guarded native destination panel observation

`observe-teleportation-world-map` requires the exact guarded
`KMG_AUTOMATION_WORKING` load, then creates a disposable world-map scene fixture
with `Game.LoadArea(BlueprintRoot.GlobalMap.GlobalMapEnterPoint, AutoSaveMode.None)`.
It never invokes a travel spell or claims relocation qualification. The working
save is in the prologue: the first run correctly failed because no revealed
non-origin point was available. Its run ID was
`20260908T0017310600627Z-090f3920894d48c1b50bf1599e9c6639`.

The revised probe records all scene points before fixture changes and selects
Oleg's exact stable blueprint ID when an ordinary revealed fixture is absent.
It temporarily sets that point's reveal/visited flags. The reflection seam is
`LocationData.IsRevealed`'s private setter, whose entire native body assigns the
backing boolean. Calling native `Reveal()` would also execute campaign triggers,
so this setter is used only inside the guarded disposable observation. All three
flags are restored in `finally`; the save-write sentinel remains installed.

Run `20260908T0028468837443Z-700a1cbb6ead4159812161c32be81f08` passed all five
assertions. Directory: `20260908T0028468636122Z-observe-teleportation-world-map`.
The structured files `teleportation-scene-points.json` and
`teleportation-world-map-forensics.json` record 611 native scene points, exact
selection correlation, unchanged origin/time/travel command and party IDs,
restored fixture flags, a closed panel, and no observed save writes.
The remaining 95 inventory blueprints are not current main-campaign scene
anchors; blueprint registration alone must never make them eligible.

The live desktop `GlobalMapMessageBox` owns a `LocationGoToDialog` CanvasGroup
with `VerticalLayoutGroupWorkaround` and `ContentSizeFitter`. Its native child
`Controllers` contains the standard Accept/Cancel pair and the alternative OK
control; `TeleportControllers` is the settlement-circle action. Buttons are
native `ButtonPF` objects with persistent listeners `Accept`, `Hide`, and
`OnTeleportPressed`. Additional positive spell rows can be appended to this
existing vertical layout after native composition. No native listener needs
replacement, no normal-travel continuation needs synthesis, and no raw click
patch is justified. Presentation sizing still needs live casting qualification.

Canonical token placement uses the `GlobalMapLocation` component's own transform
as the anchor, through native `UpdatePawnPosition`; `LocationVisualPostion` is a
separate visual field. The probe now records both explicitly. This inventory
proves structural anchors, not campaign permission or safe spell arrival.

Validation for this observer: 1,428 domain tests, repository validation, clean
Release/package build, strict installable-package validation, and guarded native
panel observation pass. The preflight fingerprint assertion now captures its
post-operation snapshot once and emits exact deltas on failure. A later diagnostic
identified only stale directory enumeration timestamps (deploy-staging, the exact
build bin directory, and packages); no file or directory identity delta appeared.
The fingerprint now refreshes each FileSystemInfo before reading its metadata,
retaining both file and directory comparisons. Failed preflight runs are not
used as successful qualification evidence.


## Twelve-module settings checkpoint

The default-ON `teleportation-spells` setting is the twelfth independent module.
Schema 11 preserves every explicit old value, migrates an absent Teleportation
key ON, preserves explicit OFF, and retains malformed quarantine/future-schema
rejection. Configuration copies, equality, bit 2048, formatting, UMM settings,
publication intent, the guarded typed request, and module observer include it.

All 1,429 domain cases pass, including 4,096 actual settings round trips and
4,096 independent publication plans. The PowerShell catalog produces exactly
12 modules and 26 boundary states; guarded preflight passes 208 checks plus the
world-map metadata check. Repository validation, clean Release build, and strict
package validation pass. The canonical Steam working-save regression passes 11
assertions: run `20260908T0044326727368Z-252ae5a1786547be8ab5ee7ea16f3d2d`,
directory `20260908T0044326571115Z-working-save-smoke`.

The module observer currently proves restart-bound settings intent only for
Teleportation. Actual three-spell publication, OFF patch absence, contextual UI,
resource expenditure, and the 26-state live publication matrix remain pending.
No release metadata was promoted. The curated point inventory and unresolved
exclusions are in `TELEPORTATION-MAP-POINT-AUDIT.md` and its companion CSV.


## Real spell blueprint and publication checkpoint

Three project-owned stable spell identities are appended after the immutable
0.0.116 manifest prefix. Teleport and Greater Teleport publish at native Wizard
5/7 and exact optional Travel domain 5/7; Word of Recall publishes at Cleric 6
and Druid 8. Identity registration uses a separate registry and publication uses
an independent list transaction. A failure disables Teleportation without
rolling back unrelated modules. No character receives a direct known-spell or
prepared-slot grant.

The Dimension Door donor's Parent is explicitly cleared. All local target flags
are false, no delivery/effect component is retained, metamagic and spell resistance
are absent, material-component data is non-null/empty, and action-bar autofill is
ignored. The checker rejects local areas. Full contextual cast-state and resource
revalidation remain future adapter work.

The first two live publication probes were rejected:
`20260908T0110532931491Z-f0ed01a798b640cca67cd77017cd0165` and
`20260908T0118069315518Z-71ca0716a33b4e9c93bde69f465d2d61`.
Narrow diagnostics showed late `SpellListComponent` metadata in the installed
profile, with every local-cast restriction still intact. Its exact native class
has only `SpellList` and `SpellLevel` fields and a constructor. Validation now
allows that exact metadata type and rejects every other extra component.
Standalone retains only the two owned components; the installed profile adds
two list components to Teleport/Greater Teleport and three to Word of Recall.
This comparison does not identify which optional provider added each component.

Native list publication first resolves every required target, then replaces
individual `SpellLevelList.Spells` references and clears `m_SpellsFiltered`.
Different physical levels that previously shared a list are handled separately;
conflicting requests for one physical level fail before mutation. Duplicate
publication preserves already valid list/cache instances. Rollback checks all
ownership references before restoring exact previous list and cache instances.
`SpellLevelList.m_SpellsFiltered` is the sole production reflection seam in this
publication transaction.

Qualified runs:

- Installed profile ON: `20260908T0121349574829Z-c5b927ca303441749237d48dd72e9151`,
  directory `20260908T0121349454809Z-observe-teleportation-native-contracts`,
  seven assertions PASS.
- Standalone ON: `20260908T0125261375669Z-778971707ad141d0a3b7afab95922da6`,
  directory `20260908T0125261285579Z-observe-teleportation-native-contracts`,
  seven assertions PASS; profile `compat-20260908T012436Z-1388860e92dc` restored
  Mods, SoundBank, and settings exactly.
- Teleportation OFF / other eleven ON:
  `20260908T0129144377215Z-2b8748198eca415fbcca4e2006bd264f`, directory
  `20260908T0129144220691Z-observe-feature-module-settings`, 31 assertions PASS;
  settings restored byte-for-byte. The OFF probe performs no publication fixture.
- Working save: `20260908T0133209151824Z-e0fd19bcdadc456dbe3a1b1ea77bc991`,
  directory `20260908T0133208995428Z-working-save-smoke`, eleven assertions PASS.

The guarded main-menu publication fixture removes owned spells only from copied
in-memory lists, calls the actual publication and rollback methods, verifies
exact reference restoration, and restores the original native lists/cache in
`finally`. It does not learn/prepare spells, cast, relocate, or write a save.
The module boundary observer uses the same publication checks. Native spellbook
interaction and actual contextual expenditure remain unqualified.

All 1,432 domain tests, clean Release build, repository checks, and strict
installable-package validation pass. The manifest contains 1,711 entries:
1,709 active and two reserved. The complete published 0.0.116 prefix is pinned
by a separate hash in validation. Release metadata remains 0.0.116.
