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
| 1 Post-teleport first-arrow movement | TODO |
| 2 Conjuration specialist slots | TODO |
| 3 Compact UI + settlement coexistence | TODO |
| 4 Scrolls: items, vendors, learning, casting | TODO |
| 5 Persistence + final install candidate | TODO |

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

1. Live guarded reproduction (see discriminating experiment above) via a
   narrow diagnostic scenario modeled on
   `RuntimeTestRunner.TeleportationDestinations.cs` (it already drives real
   casts + native travel); capture marker state + CalculatePathByMarker
   results at the four boundaries.
2. Decide the fix from evidence: likely candidates are (a) TeleportParty-parity
   travel-state cleanup (`TravelData?.Finish(); TravelData = null;`) plus the
   pawn start/stop events so `GlobalMapUI` clears/rebuilds markers natively —
   WITHOUT OpenOutgoingEdges; or (b) a narrow marker refresh. One-shot,
   request-bound, no reveal.
3. Implement fix + first-legal-arrow regression test through the real
   `GlobalMapUiDirectionMarker.HandleClick` / `GoToMarker` path.
