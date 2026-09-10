# Z teleportation mission — durable state

Companion to `Z-TELEPORTATION-MISSION.md` (the contract). This file records
current progress, evidence, and exact resumption instructions only.

## Repository / branch

- Repository: `C:/Dev/KingmakerGunslingerLab/repo/KingmakerGunslinger`
- Branch: `codex/z-teleportation-completion` (created from master `ab9eb762`)
- HEAD: `a83d09c1` (mission file committed; not yet pushed)
- Working tree: clean except this file (uncommitted until first coherent gate)
- Installed build identity: not yet re-verified this session (master metadata says
  0.0.121-unified-firearm-maintenance); verify before any runtime test.

## Acceptance checklist

| Gate | Status |
|---|---|
| 1 Post-teleport first-arrow movement | IMPLEMENTED + NATIVE-VERIFIED (core fix; breadth items below remain) |
| 2 Conjuration specialist slots | NATIVE-VERIFIED (publication + behavioral scenario; two consecutive PASS runs) |
| 3 Compact UI + settlement coexistence | NATIVE-VERIFIED (coexistence 26/26, interaction 29/29, casting 44/44, gamepad PASS) |
| 4 Scrolls: items, vendors, learning, casting | TODO |
| 5 Persistence + final install candidate | TODO |

Gate 1 remaining breadth (mission §3): Recall cast + arrow; scroll source (after
Gate 4); off-target/mishap arrival arrow; saved magical-arrival fresh-process
reload before workaround; cancellation/no-relocation controls (arrow state
unchanged); repeat casts. Desktop path verified; controller arrows via gamepad
scenario events verified (compass events are shared).

## Gate 2 diagnosis and fix (publication native-verified 2026-09-10)

Cause: the specialist/favorite preparation slot accepts only spells in the
book's special lists (`Spellbook.GetMemorizeSlots` requires
`GetSpecialSpells(level)` membership). The school special list is a separate
BlueprintSpellList attached per school by `AddSpecialSpellList` components
(`SpecializationSchoolConjuration` for Wizard/Arcanist). The mod published
only into WizardSpellList, so the Conjuration favorite slot rejected Teleport.

Verified identity (runtime inventory, observe-teleportation-native-contracts
with the new `specialSpellLists` section): WizardConjurationSpellList =
`69a6eba12bc77ea4191f573d63c9df12` (also attached by
ThassilonianConjurationFeature and TeleprotationSchoolBaseFeature; Travel
domain list `ab90308d…` publication confirmed present at L5/L7).

Fix (commit "Publish Teleport spells into the native Conjuration school
list"): Teleport@5 and GreaterTeleport@7 added as required targets of the
existing reversible/idempotent TeleportationSpellListPublication. Word of
Recall stays out of Wizard lists. Existing characters: the school feature
re-activates on load → `AddSpecialList` re-derives `m_SpecialSpells`, so no
respec is needed (IL: Spellbook.AddSpecialList adds every list spell).

Native verification: observe-teleportation-native-contracts PASS (11
assertions); inventory shows KMG Teleport/Greater in the Conjuration list at
L5/L7 across all three attaching features. Build/repository validation/1,554
domain tests PASS.

### Gate 2 behavioral proof (native-verified 2026-09-10)

New guarded scenario `disposable-teleportation-specialist`
(RuntimeTestRunner.TeleportationSpecialist.cs; registered in the catalog,
runner dispatch, request validator, Invoke-KingmakerRuntimeTest.ps1,
RuntimeAutomation.Common.ps1 metadata, and both preflight scenario lists).
Two consecutive PASS runs (9/9 assertions, zero save writes), e.g. evidence
dir 20260910T14*Z-disposable-teleportation-specialist (see newest under
runtime-evidence). Proven, in order:

1. Unsaturated wizard book has NO favorite slot (universalist negative).
2. Native AddSpecialList seam (exactly what SpecializationSchoolConjuration's
   OnFactActivate calls) attaches the school list; Conjuration list contains
   Teleport, Evocation's does not. (Fixture note: descriptor.AddFact of the
   school feature did NOT run its component in the working save; the fixture
   uses the public native seam directly. Real characters attach via feature
   activation at specialization/load.)
3. The Conjuration favorite slot refuses ConeOfCold (Evocation); the Evocation
   favorite slot refuses Teleport (other-school negative).
4. Native controller MemorizeWithSound(data, favoriteSlot) — the drag-drop UI
   boundary — lands exactly one unready Teleport in the FAVORITE slot; the
   favorite slot is displayed by the native memorize panel.
5. Mixed favorite + ordinary preparation counts exactly two; native Rest
   readies both.
6. World map (after native area load + camera settle, with bounded repeated
   selection when the augmentation refill is delayed): the favorite-only
   preparation composes as a real source (uses=1).
7. Cast with one request-local d100 roll: transaction Completed, exactly one
   use spent, arrival at the target, zero remaining ready preparations.
8. Exact cleanup (books, features, action bars, selection, world state), zero
   save writes.

Native facts learned: GetMemorizedSpells filters to FILLED slots (use private
SureMemorizedSpells via reflection for raw slots); favorite slots appear at
all levels 1..9 once a special list exists (CalcSlotsLimit), built by
UpdateAllSlotsSize; a single-book character hides the class-tab strip;
plain Teleport always consumes one d100 (empty fixture rolls = "Queue empty"
technical failure); panel augmentation can lag scene load — retry selection.

## Gate 3 compact UI + settlement coexistence (native-verified 2026-09-10)

Changes (commits e6d8737..cleanup, branch codex/z-teleportation-completion):
- Desktop rows are now ONE focusable two-line control: full spell name title
  ("Cast Teleport") over a caster · uses detail line
  (TeleportContextPresentation.Title/Detail/CompactRow). Console/gamepad rows
  keep the single-line Row(). Row extent = 2× native line height; the native
  line height is measured ONCE from the pristine first append (the donor's
  live rect stretches after taller rows exist — see NativeLineHeight).
- While spell rows coexist with the native settlement-teleport control, its
  button (found by its exact serialized OnTeleportPressed persistent callback
  in the panel hierarchy) is relabeled "Settlement Teleport"
  (TeleportationText key SettlementTeleport) and restored byte-for-byte when
  the rows are removed (Clear covers Hide/OnLocationSelect/Dispose). Callbacks,
  ownership, eligibility and shared localization assets untouched; the gate is
  the button's own activeSelf (mirrors the native control gate). Note: reading
  m_TeleportControllers via cached FieldInfo returned null at runtime despite
  the fixture reading it fine — the hierarchy search avoids that entirely.
- Domain tests: CompactRowUsesTitleAndDetailLines,
  CompactSpontaneousRowNamesLevelAndBookWhenAmbiguous,
  SettlementLabelDistinguishesNativeTeleport (suite now 1,557;
  validate_unified_repair121.py + validation/static-validation.json updated —
  no stale counts).

Verification (all PASS, zero save writes, latest evidence dirs under
runtime-evidence/20260910T15-16*Z-disposable-teleportation-*):
- coexistence 26/26 incl. NEW asserts: compact-rows-fit (every two-line row's
  preferred width within the native inner content width, exact CompactRow
  text), settlement-label-coexists (relabel + callbacks intact),
  settlement-label-restored (exact original label, no leftover containers).
- interaction 29/29 (reopen stability now compares reopens — the first append
  measures a fresh, larger native body; reopens are exactly stable at 426).
- casting 44/44, gamepad PASS (console rows unchanged).
- Native-inventory comparisons normalize only the authorized settlement label
  (TeleportationNativeButtons).

Remaining Gate 3 polish for final qualification: before/after screenshots as
optional supporting evidence; scroll-variant row text arrives with Gate 4.

## Gate 1 root cause and fix (native-verified 2026-09-10)

Cause (IL + runtime evidence): the on-screen direction arrows are
`CompassDirectionLabel` widgets built by `Kingmaker.UI.GlobalMap.CompassAvatarController.Set()`,
which runs ONLY from the native `IPawnMovementHandler.OnPawnMovementStopped`
event (subscribers: CompassAvatarController, GlobalMapAttachPointController,
GlobalMapPathsVisual, GlobalMapUiCommonPartVM, GlobalMapSoundManager,
GlobalMapUI, MapMovementController). Our relocation raised no events, so stale
origin arrows remained; `TravelByDirection` from them failed from the new
position. Diagnostic run 20260910T1242111047323Z-disposable-teleportation-arrows
proved ZERO GlobalMapUiDirectionMarker objects exist at runtime (that class and
GlobalMapUI.DirectionMarkerTemplate are unused legacy), ruling out the original
marker hypotheses; boundary captures showed travelData/partyLocation correct.

Fix (commit 7767b628): `TeleportationOutcomeWorld.Relocate` now raises the
native pawn-notification pair (OnPawnMovementStarted → SetCurrentPosition →
UpdatePawnPosition → OnPawnMovementStopped) — exactly native TeleportParty's
pair without its OpenOutgoingEdges reveal. No exploration occurs:
MapMovementController.OnPawnMovementStopped explores only mid-edge stops
(PartyLocation null), and PartyLocation is the arrival.

Verification (all PASS, zero save writes):
- disposable-teleportation-arrows, run dir 20260910T1258015459079Z: arrows
  rebuilt at each arrival (labels bound to arrival's edges), first arrow via
  the real `CompassDirectionLabel.OnClick` starts native travel along a legal
  edge, stationary frames add no mileage/time, exactly one notification pair
  per cast, exact cleanup.
- disposable-teleportation-destinations: 68/68 (20260910T…, /tmp/dest-run2.log).
- disposable-teleportation-casting, -interaction, -travelers, -gamepad: PASS.
- disposable-teleportation-disabled run bare = ERROR (requires its dedicated
  OFF-module orchestration via Invoke-TeleportationHardeningQualification; not
  caused by this change; rerun under that harness before final handoff).
- Release build, repository validation, 1,554 domain tests, 464-check preflight
  all PASS at each commit.

## Gate 1 investigation notes (IL forensics completed 2026-09-10)

Relocation path (`TeleportationOutcomeWorld.Relocate`):
`MarkArrival` → `SetCurrentPosition(new MapPosition(destination))` →
`UpdatePawnPosition()`. Native `TeleportParty` additionally does
`TravelData?.Finish(); TravelData = null;` + pawn started/stopped events +
`OpenOutgoingEdges` (rejected: reveal).

Established native contract (offline IL reflection against installed
Assembly-CSharp; tool source kept at /tmp/ildump, built from the repo's own
`BrownFurIlDisassembler`):

- The on-screen direction arrows are scene-prefab
  `Kingmaker.Globalmap.GlobalMapUiDirectionMarker` components (fields
  `Position`, `DirectionLocation`, serialized in scene; NO runtime creation
  code — `GlobalMapUI.UiDirectionMarkers`/`DirectionMarkerTemplate` are
  legacy, only `ClearDirectionMarkersUi` is called, from
  `GlobalMapUI.OnPawnMovementStarted`).
- `HandleClick` → `GlobalMapRules.GoToMarker` → `TravelToMarker` →
  `CalculatePathByMarker(marker)`. If that returns null, the click is a
  silent no-op. This is the refusal seam.
- `CalculatePathByMarker`: if `marker.Position.Location == State.PartyLocation`
  → trivial path + `AttachEnd(DirectionLocation)` (always non-null, edges only
  checked for `IsLocked`). Otherwise → `CalculatePathToPosition(marker.Position)`
  → `FindPath(from=State.CurrentPosition, to)`.
- `FindPath` rejects: locked edges, and any multi-hop edge whose
  `MapEdgeData.Revealed` is false. `Revealed` means FULLY explored
  (`Explored1 + Explored2 > 0.999999`). Same-edge travel only needs the
  relevant direction partially explored.
- `GlobalMapState.CurrentPosition` prefers `PartyPosition` (we set it), so
  position/PartyLocation are correct after our relocation.
- `LocationRevealController.Tick` is pure reveal (perception checks +
  `RevealLocation`); hypothesis A (guard suppresses needed Tick work) is
  RULED OUT at IL level.
- Stale `TravelData` is NOT overwritten-free: `TravelToMarker` replaces it, so
  a paused stale `TravelData` alone does not block arrow clicks; it only
  leaves Continue/Stop UI state (`GlobalMapUI.Update`) and OnBreak resume
  behavior. Secondary cleanup candidate (TeleportParty parity:
  `Finish(); = null;`) but not the arrow no-op cause by itself.
- What the owner's workaround changes: ordinary travel arrival calls
  `OpenOutgoingEdges(null)` (explores initial segments of outgoing edges +
  `RevealLocation`) and fires pawn start/stop events. That is exactly the
  native work our no-reveal relocation omits.

Primary hypothesis (B refined): the arrows fail because the marker's
`CalculatePathByMarker` → `FindPath` requires fully-Revealed edges, and the
edges at the teleport destination are not in that state after our no-reveal
relocation; the workaround's ordinary travel reveals/explores them.

CONFLICT TO RESOLVE IN FIX DESIGN: mission forbids revealing outgoing roads or
setting EdgesOpened just to satisfy arrows ("retain native refusal" for
blocked/unknown edges). But for genuinely familiar locations the player must
have revealed those roads during the ordinary visit. The live capture must
determine, per-arrow, whether the failing marker's edges were ALREADY revealed
before the cast (then something else is stale — e.g., an event-driven UI
refresh we skipped) or genuinely unrevealed (then native refusal is correct
and the reported bug must be something else, e.g. stale visible markers that
should have been cleared by `OnPawnMovementStarted`, which we never raise).
Note `GlobalMapUI.OnPawnMovementStarted` clears direction markers — if native
teleport raises it (via MapTravelData.Start()/Stop() events) and we do not,
stale markers from the ORIGIN remain on screen around the token; clicking
them routes from the new position to old-origin marker positions and can
legitimately fail. This matches "arrows around the party token do nothing"
and the workaround (real travel start clears markers, stop creates correct
ones). VERIFY: whether any native code recreates markers after clearing
(still unknown — no creation code found; possibly markers are shown/hidden,
not created/destroyed, in current build — check scene/edge prefab activation
in the live fixture).

Next smallest discriminating experiment (live, guarded, disposable fixture):
1. Stationary at origin with a working legal arrow: capture marker objects
   (scene instances, active state, Position, DirectionLocation),
   `CalculatePathByMarker` result for each.
2. Contextual cast → immediately after relocation: capture the same, plus
   whether the SAME marker objects persist (stale from origin?) and their
   click path calculation result.
3. First arrow click: capture refusal point (null path vs. locked vs. other).
4. Workaround sequence on a separate diagnostic attempt: capture what
   changed (marker set, edge Explored values, EdgesOpened, events raised).

## Evidence log

- 2026-09-10: mission file persisted; branch created; state established. No
  builds/tests run yet this session.
- 2026-09-10: Gate 1 IL forensics completed offline (see notes above). Tool:
  /tmp/ildump/ildump.exe + scan.exe (csc-compiled wrapper around the repo's
  BrownFurIlDisassembler, loaded the installed Assembly-CSharp with Managed
  deps). Key native files: artifacts/teleportation/native/GlobalMapRules.cs,
  MapMovementController.cs, LocationRevealController.cs. No source changes.
- 2026-09-10: Implemented guarded diagnostic scenario
  `disposable-teleportation-arrows` (new
  `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.TeleportationArrows.cs`;
  registered in RuntimeTestScenarioCatalog.cs, RuntimeTestRunner.cs guard
  sets, RuntimeTestRunner.TeleportationInteraction.cs dispatch/path/claims,
  Invoke-KingmakerRuntimeTest.ps1, RuntimeAutomation.Common.ps1 metadata,
  Test-RuntimeScenarioPreflight.ps1). Scenario captures native direction
  markers + CalculatePathByMarker results at all four mission boundaries and
  exercises the first arrow through the real `HandleClick` handler.
  Checks: Release build PASS; repository validation PASS (0.0.121); full
  domain suite PASS (1,554 tests); runtime scenario preflight PASS (464).
- 2026-09-10: Narrow preflight repair (pre-existing 0.0.121 staleness, not
  caused by this work): bumped 22 positive version literals
  0.0.120→0.0.121 in Test-RuntimeScenarioPreflight.ps1 (intentional invalid
  negatives at lines ~901/909/1345 kept), and added missing
  'working-save-unified-repair-alias' to its $expected list. Preflight was
  failing on master before this repair.
- 2026-09-10: guarded native run of disposable-teleportation-arrows in
  progress (first attempt failed on bash-mangled boolean parameter; rerun
  with numeric booleans). Result pending — CHECK /tmp/arrows-run.log and the
  newest runtime-evidence/ directory for teleportation-arrows.json on
  resume.

## Next concrete actions

1. Gate 1 breadth: add Recall + off-target arrow checks (extend arrows scenario
   or rely on resolver coverage); fresh-process reload-after-teleport arrow
   check (needs the persistence harness phase pattern from
   TeleportPersistence.Common.ps1).
2. Gate 2 (specialist slots): inspect spellbook specialist-slot filtering.
   Start from TeleportationSpellbookAdapter and the native Spellbook
   specialist-slot model in artifacts/teleportation/native/Spellbook.cs; the
   reported rejection is in the LOCAL spellbook preparation UI, so also inspect
   SpellBookView/SpellItem Memorize paths and any school filter
   (BlueprintSpellbook.SpecializedSchool / GetSpecialSpellList).
3. Gate 3 (compact UI): TeleportContextLayoutPolicy/TeleportContextPresentation.
4. Gate 4 (scrolls): new item blueprints + vendor grants + scroll adapter.
5. Gate 5: persistence + final qualification/install of combined candidate.
