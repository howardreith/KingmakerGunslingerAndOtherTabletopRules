# Contextual world-map teleportation native forensics

Status: native desktop/gamepad casting, relocation and associated-pet checkpoints have structured evidence; full feature qualification remains in progress.

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
pool, and verified restoration. `Spellbook.Memorize` assigns a shared `LinkedSlots`
array to all members of a preparation and marks them unavailable until native
restoration. `SpellSlot.Spend` clears its own availability and, for opposition
preparations, every linked member. Native `IsMainSlot` is a display/group marker;
`SpendInternal` may select a non-main member when scanning backwards. The adapter
therefore counts complete reciprocal groups, predicts the native selected use,
and verifies every affected member plus all other native resource levels.

`AbilityRestoreSpellSlot.Apply` restores the captured prepared slot by assigning
its `Available` field. Production compensation uses that proven operation on the
exact captured group; spontaneous compensation calls `RestoreSpontaneousSlots`
only after confirming its current capacity can restore the original count. Every
restoration is verified and guarded once. A change before this request invokes
native Spend is ambiguous, even if it resembles a one-use debit: it cannot be
attributed to or refunded by this request.

The production adapter uses public native APIs/fields only. It validates active
party membership, ownership, native living/CanAct state, known/prepared data,
casting restrictions, exact unmodified AbilityData and positive real resource
counts. Native `AddSpecial(int, BlueprintAbility)` also registers the spell in
`m_KnownSpellLevels`, with its AbilityData in `m_SpecialSpells`. Spontaneous
source discovery therefore includes both public `GetKnownSpells` and
`GetSpecialSpells`; capture and expenditure recheck physical known membership.
The guarded resource fixture alone invokes the exact private `AddSpecial` method
to qualify a special-only known instance. Production does not use that reflection
seam or grant any spell. Items, conversions, metamagic, summons, pets as casters, inactive owners,
unproven linked groups and over-cap spontaneous pools are omitted. It captures
all ten native resource levels and actual collection/slot/ability/link identities
before spending; no private slot-pool reflection write is used.

Installed-profile run `20260908T0318149717580Z-e4d810cac736401d9836f64429241bb7`
(directory `20260908T0318149620713Z-disposable-teleportation-resources`) passed
17 assertions. Seven real native sources included two casters, distinct books,
opposition preparations and spontaneous fifth/seventh-level slots. Every case
spent and restored exactly one use; cleanup and no-save-write checks passed.
This is lower-layer resource evidence, not a completed contextual spell cast.

## Ordinary arrival and persistence

`LocationData` persists `IsExplored`, `IsSeen`, `EdgesOpened`, `LastVisited`,
`IsClosed`, `IsFake`, and reveal state. There is no ordinary-arrival counter.
`LastVisited` changes during local-area entry/reconstruction and cannot
implement familiarity.

`MapMovementController.MoveAlongEdge` calls `OpenOutgoingEdges` at intermediate
points only when the prior `CurrentEdge` exists, its `RevealPath` flag is true,
and the edge changed. Both native `TravelEdge` constructors initialize that flag
true, including on already revealed roads; it is not native visited state. The
initial hypothesis that known roads automatically clear the flag was disproved
by guarded preview/actual-route evidence. The calls also miss intermediate
boundaries crossed in a first, large movement frame, so they are not a complete
arrival-count seam.

The implemented observer matches the unique local-0 `MapTravelData.WalkedDistance`
load followed by local-1 store, after native partial-edge initialization and
before movement. An invocation-local sample records the existing path's native
spline endpoint distances. At each normal method return it compares the native
completed distance, caps a stopped/replaced route at the actual settled point,
and records qualifying boundary crossings once. Branch labels are retained.
It never plans paths, advances time, moves the token, opens edges or reveals
points. Unknown IL/geometry disables this adapter; it does not alter vanilla
movement or other modules.

Manual hooks are `Kingmaker.Controllers.GlobalMap.MapMovementController.MoveAlongEdge`
(transpiler) and `Kingmaker.Player.OnAreaLoaded` (migration-only postfix). Neither
is installed when the module is OFF. `UnitPartTeleportFamiliarity` uses the exact
main-character descriptor's native Get/Ensure ownership. Its private JsonProperty
payload is the sole count source; native save/load callbacks leave bytes intact.
`EdgesOpened || IsExplored` seeds legacy counts once before new ordinary arrivals.
Reveals after migration cannot add ledger counts.

Guarded `disposable-teleportation-familiarity` passed nine assertions in run
`20260908T0223050324619Z-eb7985f6f51d4f098d522387a20eb2ee`. It used native panel
Accept, native route planning, and native Tick with a request-local time input,
then restored tracked state and verified no save write. Three actual trips
counted both directions and known intermediate crossroads. This establishes
that narrow native movement seam, not full contextual spell casting or disk
persistence. Ownerless UnitPart payload serialization passed; a prior generic
live-owner serialization probe failed on Unity Vector3 recursion and was narrowed.

Fixture-only reflection accesses LocationData.IsRevealed's assignment-only setter,
MapMovementController.CalcSpeedModifiers, the UnitPart payload, and captured
LocationData/MapEdgeData fields for restoration. Production has no fixture entry
point. Full game-save owner graph/reload and disabled-module persistence still
require the guarded save workflow.

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

### OFF hook registry and standalone load qualification

The installed Harmony12 compatibility bridge throws inside `ToHarmony12` when
`GetPatchInfo` receives a method absent from the patched-method registry. The
runtime observer now obtains `GetPatchedMethods` first and only requests details
for registered methods. OFF run `20260908T0250396138426Z-1b12dee6c0c04cd1977f7dcd49b0bac7`
proved zero owned familiarity hooks; standalone ON run
`20260908T0253148546708Z-b7f27f81de57400b93fcb15c9439d023` proved exactly two.

The standalone save-backed attempt
`20260908T0232569730289Z-b4cdb0b1cfee4e8b944bac6a69901fd9` failed before this
feature fixture. Native `Player.PostLoad` IL002e calls `Single` on cross-scene
entities using `<PostLoad>b__136_0`; that predicate compares each entity's exact
`UniqueId` to `Player.MainCharacter.UniqueId`. No match existed in that run.
The fixture never executed, no save was changed, and profile restoration was
verified. The underlying profile/save prerequisite remains unresolved. Neither
save-free startup nor an installed-profile load is a standalone save-backed PASS.

### Resource checkpoint completion

Final resource run `20260908T0342259125609Z-337c4bc0fcbd45f6ae14803d9563b042`
passed 19 assertions in `20260908T0342259005448Z-disposable-teleportation-resources`.
It proves special-only native known membership and attribution-safe compensation
in addition to the seven normal prepared/spontaneous sources. Native slot and
known-record operations were real; no point button, confirmation, damage or
relocation was exercised. The clean Release/package build and all 1,449 domain
cases passed. Installed-profile evidence does not qualify standalone casting.

### Confirmation callback and layout seams

`GlobalMapMessageBox.FillDialogInfoLocation(bool)` completes the native action
states before `OnLocationSelect` calls `Canvas.ForceUpdateCanvases` and chooses
its native point-anchored pivot. A narrow postfix there can add positive rows
before native placement, without another route calculation. Native `HandleAccept`
calls `Accept` while the panel is active; keyboard focus on an added row will
need explicit qualification to ensure it cannot also start normal travel.

`Kingmaker.UI.IDialogMessageBoxUIHandler.HandleOpen(string, BoxType, Action<BoxButton>,
string, string, string, Action<string>)` is the native confirmation event seam.
`DialogMessageBoxBase.BoxType.Dialog` provides Yes/No controls. `OnButtonYes` and
`OnButtonNo` call `Hide` first, then invoke their callback; Escape maps Dialog
to No. `Hide` clears `IsShown`, removes its Escape handler and hides the veil.
`HandleForceClose` hides without invoking a result callback, so an owned pending
request must detect that disappearance and cancel. At that earlier checkpoint, the inspected desktop
implementation returned early for gamepad mode; that frontend still needed its
own integration qualification.


## Current world-map context and capital state

`LoadingProcess.IsLoadingInProcess` includes both the executing process and its
queued processes. `IsLoadingScreenActive` covers the remaining screen lifecycle.
`Game.IsModeActive` reads the native mode stack, while `CurrentMode` must be
GlobalMap for composition. `DialogController.Dialog`, `Player.Dialog.Scheduled`,
`Game.CutsceneLock`, active cutscene pool entries, kingdom modes, current encounter,
and any current MapTravelData additionally block composition. A stationary pawn
must coincide with its current exact registered point's native placement anchor.

The map's native component scene is `Globalmap`; the area's CustomUIScene is
`UI_Globalmap_Scene`. These are distinct. The adapter checks the loaded area's
`GetStaticScene().SceneName` against the current GlobalMapRules scene, then verifies
point membership against the native map dictionary, active scene instances and
unique registered identities. `GetStaticScene` simply returns native StaticScene.
No raw pointer, scene label as point identity, or transform write is involved.

`GlobalMapRules.GetLocationObject` only reads its dictionary. In contrast, the
native location-data getter can create a persistent record. Production destination
reads therefore use existing `GlobalMapState.Locations.TryGetValue` records.
`LocationData.GetInfo/Name` evaluates native variation conditions and existing
settlements without creating a region; names are presentation only.
`LocationRestriction.IsRestricted` checks its native IgnoreCondition override,
AllowedCondition and required companions. Native IsClosed also prevents an action.

`RegionState(BlueprintRegion)` constructs a settlement when SettlementIsPrebuilt
is true; it does not claim the region. Native capital region
`caacbcf9f6d6561459f526e584ded703` is prebuilt and resolves capital point
`f83de5c382e087b4ab6ce0b7397a2a13`. Claiming that exact region establishes the
capital. A missing/unowned/wrong-point settlement after claim suppresses Recall
without falling back. A missing kingdom precedes establishment. Unknown or
non-prebuilt capital contracts fail closed for Recall.

The guarded context fixture uses the exact native private KingdomState
`JsonConstructorMark` constructor to initialize a detached temporary state without
the public constructor's BP event. Its Regions field is assigned only on that new
object, using a native RegionState. The assignment-only RegionState.IsClaimed setter
provides the before/after control. These reflection seams are fixture-only. The
original kingdom reference, region/settlement state, map state and spellbooks are
restored before return, with save-write sentinels active.

Run `20260908T0421564890455Z-9fc529962db34a83958398ca5b84c1e9` passed 14 assertions;
directory `20260908T0421564779636Z-disposable-teleportation-context`. It qualifies
current destination/Recall/source composition and all 611 map reads, not rendered
UI or completed casting. The two rejected fixture/scene-assumption runs and their
exact identifiers are recorded in the implementation report.

Additional traced contracts for the next adapter: Player.AddCharacterToLists
excludes pets from Party, but includes cross-scene units in AllCharacters and
relates pets through UnitDescriptor.Master. Native RulebookEvent.Dice exposes
D100 (RollEntry.Value), D10, and D(DiceFormula); DiceTypeExtension.Sides returns
the integer sides value directly. Native damage/relocation qualification remains
outstanding; these traces alone do not establish live mishap behavior.


## Native contextual cast execution checkpoint

Desktop augmentation is a postfix on `GlobalMapMessageBox.FillDialogInfoLocation`.
`OnLocationSelect`, `Hide` and `Dispose` prefixes remove only owned appended rows.
There is no Accept, GoToLocation, pointer or route-planning patch. The native
OnLocationSelect layout/onscreen positioning still runs after the appended rows.
No usable source returns before constructing any UI. Reflection reads the exact
`m_Dialog`, `m_Location`, and `m_AcceptText` fields; ownership uses the exact native
`DialogMessageBox.m_OnClose` callback field. OFF installs none of these hooks.

The native button donor has `LocalizedUIText.Awake`, which overwrites its label
from a serialized shared string, and an ignored LayoutElement. The owned inactive
clone removes that localization component, replaces all persistent click events,
clears ignoreLayout and adopts the measured native content width/row height.
The containing native panel and its original buttons are untouched. A native
ScrollRect bounds the appended list; no screen coordinates identify a point.
The selected unnamed native crossroads receives a localized descriptive label
in confirmation only; IDs alone continue to control mechanics.

`IDialogMessageBoxUIHandler.HandleOpen` owns the confirmation. Native Cancel/Esc
callbacks spend nothing. A request-lifetime component checks callback ownership,
force-close disappearance and live validity. Native Yes uses the same guarded
transaction and native resource lease. The pending guard spans the complete
synchronous execution. Feedback uses `UIUtility.SendWarning` through the narrow
TeleportationCombatLog adapter and the existing combat-log helper.

`Player.AllCharacters` includes cross-scene canonical associated units that
local ControllableCharacters omits. The roster retains native Party order,
then stable associated Master chains, and checks reciprocal Pet ownership.
Dead units remain transported; the mishap resolver selects living units for
native damage. The current working-save fixture contains three living active
members and no associated units, so pet/mount qualification remains open.

Graph distance reads `GlobalMapRules.m_Graph` and its exact private static
`GetGraphWeights(TaggedEdge<GlobalMapLocation, GlobalMapEdge>)` delegate, then
calls the game's QuickGraph `ShortestPathsDijkstra`. Every Edge.Data record must
already exist to avoid the native lazy creation path. No route command is made.
The native FindPath route wrapper uses the same graph and supplies no independent
fallback when it is absent. Coordinate fallback is explicit and logged.

Production RNG uses RulebookEvent.Dice.D100/D10 and Dice.D(DiceFormula).
DiceTypeExtension.Sides returns the numeric die size, allowing canonical uniform
selection within any percentile bucket. Injected queues live only on guarded
request rows and their single execution. Damage uses one DirectDamage fixed
bonus per living traveler through Rulebook.Trigger(RuleDealDamage), with the
selected spell as SourceAbility and no MinHPAfterDamage floor.

Relocation calls only native SetCurrentPosition(new MapPosition(point)) and
UpdatePawnPosition. Before/after structured snapshots compare exact player,
map, rules, pawn, area, scene, party and associated-unit references, GameTime,
UnitPartWeariness state, route/history, encounter/miles state, every point/edge
record and discovery flag, perception IDs and familiarity. LastLocation and
the canonical token position are the intended native changes. No local area,
route movement, time correction, party unit load or transform mutation is used.

First end-to-end run `20260908T0508534702414Z-e3ef7871730244eda49763aa3789b9db`
failed the label assertion and exposed overlapping ignored native LayoutElements.
All completed casts still proved exact expenditure and protected relocation;
that failed run is not acceptance evidence. The corrected run
`20260908T0518061328136Z-dcef820240d34772a501292ba833475e` passed 37 assertions
(directory `20260908T0518061237339Z-disposable-teleportation-casting`). It includes
normal and repeated native mishap packets of 1 then 2, all three travelers,
one spell use, graph alternatives, exact Recall before/after capital, native
cancel/duplicate callbacks and exact fixture cleanup with no save writes.

The final desktop cast checkpoint
`20260908T0542206317921Z-5180c9651e2840f1bbe49631dddaa2ef` passed 42 assertions
in `20260908T0542206207909Z-disposable-teleportation-casting`, adding real-book
replacement cancellation, no-alternate failure, pre-effect resource restoration,
stale caster and normal production RNG to the earlier 37-assertion run. Native
UnitDescriptor.DeleteSpellbook only removes m_Spellbooks' entry; the guarded
replacement fixture retains/restores the exact prior instance in that dictionary.
This reflection write exists only in the disposable runner. Production confirmation
binds native Player/Map/Rules/book references; it does not replace any book.

Module OFF run `20260908T0535060528821Z-be76dbc835524646b26a0df3844267ea` proved
zero actual destination hooks and zero arrival hooks, with no player-list
publication. Working-save regression
`20260908T0545391191224Z-94ee28fffc1445669dd335920b10d484` passed eleven assertions.
Neither result substitutes for remaining scene/input/associated-unit/persistence
or compatibility qualification.

Additional native input inspection found that desktop DialogMessageBox.Update
only calls TextInputChanged (the text-field confirmation mode), and the native
IMessageBoxUIHandler.HandleAccept has no direct call site in Assembly-CSharp IL.
Do not infer a duplicate Enter-key route from interface names. The native
OnYesSelect coroutine yields, calls Yes.Select, yields again, and calls
Yes.Select again; it does not invoke the button callback. Guarded keyboard
submission/navigation beyond the Escape stack remains unqualified.
Gamepad GlobalMapMessageBoxView.LocationNeedsMessageBox returns true for revealed
non-origin points, and SetFromLocation calls FillDialogInfoLocation before native
positioning. UpdateNavigation builds the existing native collection and selects
its default; ConsoleMultiNavigationCollection exposes AddRow/RemoveEntity, while
ConsoleButton.SetConfirmAction supplies a native action seam. These are inspected
integration candidates, not installed or qualified gamepad functionality.


## Native desktop lifetime and scrolling qualification

Native `DialogMessageBox.HandleOpen` returns immediately when IsShown is true.
`HandleForceClose` calls Hide without invoking the close callback. The production
confirmation's Update detects disappearance or callback replacement and cancels
its request. Native `EscHotkeyManager.OnEscPressed` invokes the last subscribed
action; point selection subscribes the original Hide callback, and confirmation
subscribes DialogMessageBox.OnEscPressed. No synthetic input or raw key patch is
needed to exercise these exact boundaries through the guarded runner.

`MapTravelData.Start` emits IPawnMovementHandler.OnPawnMovementStarted after
UpdateStartPosition sets Walking. A request-local native event subscriber counted
one start and one stop for each actual ordinary route. It installs no movement
patch, supplies no alternative route, and is removed before fixture cleanup.

`UIGlobalMapConsts.Pivots` contains eight native anchor pivots. The appended source
viewport reserves room for the native panel body and its native-sized row spacing
above or below the selected point, using native WorldToViewportPoint and current
canvas geometry. Native code still chooses the final panel pivot/position. The
original panel layout is recomputed only after positive actions were resolved;
this prevents its cached preferred height from including a previous source list.
The no-source path does not perform this extra layout operation.

Guarded run `20260908T0645599302678Z-3fbdcbc3efcc4ef3a07e1687ff737d38`
(`20260908T0645599222679Z-disposable-teleportation-interaction`) passed 29
assertions: native behavior without sources, native Travel with sources, actual
Escape and force-close handling, dialog replacement, no-op/unvisited omission,
live use count/exhaustion, repeated selection, real confirmation after frame
updates, and native scrolling to the final row among twelve real sources. The
eight reopened short lists retained exactly 213 layout units of viewport height.
The long list contained 426 units in a 370.1854-unit visible viewport. Native
UIUtility.IsTransformInScreen and RectTransform bounds supplied structured
geometry evidence. No screen coordinates, screenshot, OCR or pointer input was
used. The full 42-assertion casting regression also passed in
`20260908T0651138625548Z-e62f45df4e7247b9b97325b3cf24ffc8`.

The report retains both rejected interaction probes and their exact run IDs.
Neither failed probe wrote a save or left the temporary books/map fields behind.
This desktop evidence does not qualify gamepad navigation, all resolutions,
associated-unit death behavior or complete campaign save/reload.

Damage forensics at that checkpoint: RuleDealDamage.OnTrigger updates Damage and
raises native damage events, but does not itself update UnitState.LifeState.
UnitLifeController.TickOnUnit performs that native transition, including ordinary
unconsciousness, death, ferocity, regeneration and difficulty behavior. It is
registered for GlobalMap among its five native modes; BaseUnitController.Tick
iterates AwakeUnits unless TickSleeping is overridden. Native OnUnitDeath also
uses the unit view. The HP-safe three-member probes did not establish the
required off-scene living/dead/pet behavior. The following checkpoint supplies
that evidence and the resulting native boundary; no alternate life threshold or
HP floor has been introduced.

### Native mishap life-state boundary and JSON contracts

The guarded traveler probe proved that canonical global-map party members and
native pets had inactive views and were absent from AwakeUnits. A synchronous
RuleDealDamage packet does not itself settle UnitState.LifeState. The production
`TeleportationMishapDamageTarget` now derives narrowly from UnitLifeController and
uses its protected ShouldTickOnUnit/TickOnUnit methods for the affected target.
No new reflection seam, global controller, native threshold copy, HP floor or
view activation is involved. This occurs inside material-effect tracking.

Native UnitReturnToConsciousController.Tick iterates the native unit pool outside
combat and invokes MakeUnitConscious on non-finally-dead units. That explains the
first dead fixture control's recovery during the confirmation frames. The final
probe establishes native death immediately before commitment, observes the
normal native life event, and leaves difficulty/recovery behavior untouched.

Native EntityCreationController.SpawnUnit writes a fresh ID to the loaded donor
prefab before cloning it. The request-local pet fixture restores that prefab ID
immediately after each spawn. It uses SetMaster for reciprocal native ownership,
RemoveEntityData for disposal, and explicit original cross-scene/party/HP/damage
attribution checks. Fixture-only restoration uses the exact private setter of
UnitEntityData.LastHandledDamage; it never runs in production casting.

The native save contract resolver omitted anonymous snapshot properties under
JsonConvert defaults. `TeleportationDiagnosticJson` uses JsonSerializer.Create
with DefaultContractResolver, invariant culture and no reference/type metadata.
It is used for protected-state comparison, diagnostics and guarded native UI/book
fingerprints; it never replaces the game's save serializer. Negative controls
prove native time/miles/point changes and actual prepared expenditure are detected.
Earlier anonymous fingerprint assertions are superseded, as stated in the report.

Traveler run `20260908T0743051211896Z-0b845aad9e574a77bd53746466e5827e` passed 15
assertions, including native pet death on the first mishap, exclusion on reroll,
unconscious living damage, dead/nontraveling controls and complete cleanup.
Casting and interaction regressions with the corrected comparisons passed 42
and 29 assertions respectively; exact IDs and paths are in the report. Native
inactive death visuals emitted bounded coroutine warnings, not failed damage or
life transitions. No optional death visual was forced active in a world-map cast.

Additional inventory findings for subsequent qualification: base-game AddFamiliar
creates a Visual.Critters.Familiar and records it in UnitEntityData.Familiars; it
is not a UnitEntityData damage target. No base-game mount unit controller/type
was found in the exact assembly class inventory. Mod-provided canonical unit
associations still require profile qualification. RegionalBuffController.Tick
returns unless GlobalMapRules.PartyInTravel is true, so its LocationRadiusBuff
processing is not a stationary placement callback. Special-point casting remains
an open qualification item.

## Native gamepad adapter and guarded qualification

`GlobalMapMessageBoxView.SetFromLocation` has a cleanup prefix;
`FillDialogInfoLocation(bool)` has a positive-source append postfix;
`UpdateNavigation` has an append-registration postfix;
`DestroyViewImplementation` has a cleanup prefix. All four hooks are manual and
absent when Teleportation is OFF. Native Accept/Cancel, pointer methods and
navigation collection construction remain unchanged. The adapter reads exact
`m_Dialog`, `m_ConfirmButton`, `m_NavigationCollection`, and the protected generic
`ViewModel` property. Native ConsoleButton.SetConfirmAction replaces the clone's
action; AddRow/RemoveEntity manage only owned rows. Native default selection is
already established before registration. A pure minimal-scroll policy keeps the
selected row visible inside the appended ScrollRect.

The shared confirmation presenter captures either the desktop modal or one
uniquely identified, loaded native DialogMessageBoxView. Console ownership reads
its protected ViewModel and DialogMessageBoxVM.m_OnClose; it never changes those
fields. Native MessageBoxUiContext.HandleOpen replaces an existing console modal,
whereas desktop HandleOpen ignores an already shown dialog. Console ForceHide
calls HideDialogMessageBox, which disposes the VM and deactivates/unbinds the
view. OnDisable cancels a pending owned request immediately without closing a
replacement. Normal Yes calls the callback before disposal; the same idempotent
spell transaction handles both frontends.

Fixture-only setup first completes the native desktop global-map load. It then
calls RootUiContext.DisposeLoadingScreen, BundledSceneLoader.UnloadSceneAsync
for the old loading UI, LoadSceneAsync for SceneName.LoadingScreenUI under
gamepad mode, and RootUiContext.InitializeLoadingScreen. This creates the actual
console modal through the native startup contract. The exact private
Game.LoadArea(BlueprintArea, BlueprintAreaEnterPoint, AutoSaveMode, bool, SaveInfo)
overload reloads the same area with None/false/null. Its native SceneLoader
replaces m_LoadedUIScene using SceneName.GetCustomUIName and initializes the
console global-map context. The public enter-point overload would only call
Game.Teleport when already in the same area, so it cannot perform this UI reload.
No loader field is assigned by the fixture. Old local-area controls must be
unloaded before controller mode changes: otherwise CharacterUIDecal and local
action-bar controls dereference the absent console UnitSelectionManager.

The fixture reads DialogMessageBoxView.m_WindowAnimator and
WindowAnimator.m_CanvasGroup, and waits for native fade/local-rotation completion.
TextMeshPro.GetParsedText returns the actually parsed characters, including the
native uppercase/small-caps styling. The modal's root canvas is
ScreenSpaceOverlay; native UIUtility.IsTransformInScreen uses Game.UI.UICamera
and is unsuitable for that separate overlay. The fixture projects the actual
canvas corners with its owning camera (null for overlay) against dynamic screen
bounds. It verifies complete text, no truncation/overflow and on-screen controls.
Production confirmation styling and layout are unchanged.

Other fixture-only seams read native modal m_NavigationCollection/m_YesButton,
invoke exact OnConfirmPressed/OnCancelPressed/TryDoDown/OnConfirmClick/
OnDeclineClicked handlers with their verified Rewired.InputActionEventData
parameter, and read native ConsoleButton.m_OnConfirmAction for control evidence.
These are request-local method invocations; no OS input, controller emulation or
synthetic input event is published. Recall uses the already documented native
KingdomState deserialization constructor and RegionState.IsClaimed setter, with
exact restoration. Application.logMessageReceived captures exceptions from the
controller transition through cleanup. RootUiContext.DisposeUiScene and
DisposeLoadingScreen dispose the request-created native contexts before restoring
controller mode and mandatory process exit. Save-write sentinels remain armed.

Run `20260908T1010179638483Z-9fc7ced7478444cf954b1bf08ce52cc1` passed all 39
assertions, with zero native UI exceptions and zero save writes. The report
records actual native scenes, source/navigation controls, rendered confirmations,
real Teleport/Greater/Recall expenditure and relocation, native travel exactly
once, live/exhausted rows and complete cleanup. It also records rejected probes:
the original 25-assertion result did not detect its controller-transition
exception storm and is not UI-stability proof. Desktop regressions subsequently
passed 42 casting and 29 interaction assertions; all ON/OFF and Teleportation-only
ON/OFF settings checks prove actual module hook counts. Physical controller
hardware and every screen resolution are outside these structured checks.

## Native spellbook UI qualification seams

The guarded `disposable-teleportation-spellbook-ui` fixture uses
`ServiceWindowController.HandleOpenSpellbook` and normal
`SpellbookClassTab.Toggle` / `SpellBookLevelTabs.m_Tabs` controls.
`SpellBookView.GetSpellsForLevel` combines the actual book's known, special and
custom spells; `GoNextPage` / `GoPrevPage` preserve its native pagination.
`SpellItem.Toggle` drives native selected-spell events. `SpellItem.Memorize`
reaches `SpellBookController.MemorizeWithSound` and `Spellbook.Memorize`, with
`SpellSlotItem.MechanicSlot` providing the real displayed preparation identity.
`TooltipTrigger.OpenDescriptionWindow` uses the original row's collected data
and native `DescriptionController` / `DescriptionTemplatesAbility`, including
its material-component path.

`ActionBarManager.AddSpellHandler` and prepared-spell handlers schedule the
native `ActionBarSlots.Set` refresh. `UnitUISettings.TryToInitialize` checks
`ActionBarAutoFillIgnored` through `SetSlotAutomatically` for prepared and
spontaneous spell slots. The guarded fixture observes this across frames with
ordinary Dimension Door as a positive control. Its cleanup alone reads/restores
exact native `Slots` references and `m_Phase`, `m_ShowAdditionalActionBarOnce`,
`<Dirty>k__BackingField`, `m_AlreadyAutomaniclyAdded`, and
`m_ActivatableAbilityAlreadyAutomaniclyAdded` fields and list contents.
Production does not assign these UI fields. `SelectionManager.SelectUnit` and
`MultiSelect(..., false)` use native selection events without OS input. Pause
is request-local and restored. Native spellbook UI success remains subject to
its structured runtime result; these decompiled paths alone are not live proof.

Live qualification: `20260908T1116293771376Z-9db2a72926a64a2aaba36bc40c295800`
passes 31 assertions. Native `Spellbook.AddKnown` takes `isCopy`; true suppresses
`ILearnSpellHandler.HandleLearnSpell`. The fixture therefore uses the real native
selection change to refresh its action bar after setup. This is not a production
patch or synthetic event. Native service-window close releases the parent input
surface and restores the preexisting Pause mode. The complete structured result
records an inactive surface, alpha 0, actual spell rows/descriptions/preparations,
zero fixture UI exceptions and exact cleanup including deferred native refresh.
Normal level-up selection remains unqualified by this scenario.

## Native level-up selection and preview cancellation

`CharacterBuildController.HandleLevelUpStart` checks native
`LevelUpController.CanLevelUp` and calls `Start(..., instantCommit: false)`.
Native `UnitSerialization.Serialize` / `LevelUpPreviewThread.RequestPreview`
create a separate descriptor. `SetClass` delegates to native `SelectClass`,
class mechanics and `ApplySpellbook.Apply`. The latter adds one caster level
through `Spellbook.AddCasterLevel` and calculates actual `SpellSelectionData`
from the native book: known-spell count differences for Sorcerer, or
`SpellsPerLevel` / `ExtraMaxLevel` for Wizard. No fixture code writes those
selection counts, phase flags or native prerequisites.

The guarded fixture uses only native `SpendAttributePoint`, `SetFeature` with
`Selection.CanSelect`, and `SpendSkillPoint` to finish preview prerequisites.
`CharBPhaseSpells.SetupSelector` reaches `CharBSelectorLayer.FillSpellLevel`
and reads the actual published `SpellList.GetSpells(level)`. Native
`CharBSelectionSwitchItem.Toggle` selects the book/level slot;
`CharBuildSelectorItem.Toggle` invokes its bound `SetSpell` callback and native
`SelectSpell.Apply` / `Spellbook.AddKnown` on the preview. Deferred native
refresh can recycle widgets, so qualification re-resolves their exact current
spell binding and rendered name across eight frames before invoking the row.

Escape uses `CharacterBuildController.OnHotKeyEscPressed` and its native
`OnHideDialogAnswer` confirmation. The fixture verifies callback ownership,
invokes the native Yes button and waits for closure. Native `OnHide` drops the
presenter's backend reference; exact IL contains no caller of
`LevelUpController.Cancel`. The fixture explicitly cancels its own preview to
dispose the cloned unit and stop the preview thread. It never commits a level.

The main-menu `CharacterBuildController.Warmup` leaves an AutoCommit backend in
`UIAccess.LevelUpController` with Unit and Preview referring to the same
object. The exact base assembly only assigns this field in native Start and
never clears it. The fixture allows that object only with the actual presenter
closed and its backend null; it captures and restores the global reference
without cancelling or mutating it. Any unrelated active preview fails closed.
It also restores the original public presenter Unit reference.

The only added reflection seam is the native `UnitProgressionData.Experience`
private auto-property setter, used to establish request-local XP eligibility
and restore its exact original value. Unlike GainExperience, it emits no XP
or automatic level-up event. Existing shared guarded fixture seams handle real
book creation/removal, casting stats, action-bar references and confirmation
ownership. All fixture changes remain behind the explicit scenario and save
sentinels; no production level-up patch was added.

Run `20260908T1207363385307Z-e204a1b706bf41b6891074349d6bc571` passed all 18
assertions. Wizard and Sorcerer Teleport 5 / Greater Teleport 7 appear as actual
enabled native choices, enter only the preview book, and disappear on native
cancellation. Exact original class/feature references, XP, books, resources,
UI state, selection, party, positions, time and pause are restored. This proves
native spell choice and cancellation, not committed character advancement or
campaign disk persistence. The implementation report records rejected probes
and same-artifact regressions.


## Native deferred exploration after magical arrival

`Kingmaker.Controllers.GlobalMap.LocationRevealController.Tick()` reads
`GlobalMapRules.Instance.Pawn.Position` even when no travel command is walking.
It iterates unrevealed native locations, checks reveal conditions and distance,
and may assign `LocationData.LastPerceptionRolled`, call the native party skill
check and reveal a location. Its distance thresholds use maximum controllable-party
Perception: ordinary/hidden locations use `1 + perception / 4`, landmarks use
`6 + perception`. The narrow settlement relocation calls alone therefore cannot
prevent exploration on subsequent controller ticks.

`TeleportExplorationGuardPatches` installs one module-ON-only prefix on that exact
native Tick. Each committed magical arrival records a versioned map ID, point ID
and native mileage string in `_explorationBoundary` on the campaign main character's
existing `UnitPartTeleportFamiliarity`. It is separate from ordinary-arrival counts.
An absent field preserves native exploration. At the exact stationary magical
arrival, the prefix skips only this exploration controller. Native walking,
changed point/map or changed mileage clears the boundary and resumes the original
Tick. Walking also releases malformed saved data before parsing; stationary corrupt
state prevents further spell casting and emits one technical diagnostic.

The module OFF path installs no exploration prefix and preserves serialized data.
A hook-installation failure hides contextual casts while unrelated modules remain
active. Production uses only the exact Tick method reflection seam and native public
state. Guarded fixtures additionally capture/restore the project's private saved
field, including a request-local malformed value; no player UI can inject it.
The hook observer checks Harmony's patched-method registry before GetPatchInfo,
because the installed Harmony12 bridge throws for an unpatched target.

`disposable-teleportation-destinations`, run `20260908T1330037600506Z-cc0ea3d5a53a4123a156a651546ca8c3`,
passes all 68 assertions, including 22 actual contextual casts, deferred protected
snapshots, normal exploration after native Travel and corrupted-boundary recovery.
The first Travel uses revealed native edge `4f2e9de0dde02a741a02ab55a2baf376` to
point `e1edfc64e48a2964c8eb583e36ebd57e`. Native `OnBreak` stops but retains its
paused MapTravelData; `GoToLocationRevealed` had cleared stationary PartyPosition.
The fixture verifies zero walked distance and resets that finished control before
starting its second native Travel. Both controls spend no spell resource. All
fixture fields, books, perception records, ledger fields and UI are restored, with
zero fixture exceptions or save writes. Native campaign disk persistence of both
saved fields remains an independent unfinished gate.


The exact native `UnitPartsManager` marks its Type-to-UnitPart dictionary and
owner with JsonProperty. Its Ensure assigns that owner and subscribes the part;
PreSave and PostLoad delegate to every saved part, and PostLoad resubscribes them.
`UnitDescriptor.PreSave` calls this manager before its other fact/book collections.
`UnitSerialization.Serialize` is the native level-up owner-graph entry: it turns
the descriptor off, runs PreSave, creates a JToken through native JSON defaults,
then turns the descriptor on. These decompiled contracts identify the next narrow
persistence probe; they are not a claimed owner-graph or disk qualification.
