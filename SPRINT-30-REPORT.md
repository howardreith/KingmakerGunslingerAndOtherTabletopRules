# Sprint 30 report — generic definition-driven firearm actions

## Outcome

Sprint 29 was accepted by documented contract reuse. Reload, Overhaul, and
Repair now select exactly one equipped marked firearm through a shared
marker-first context instead of comparing item blueprint identity with the
Test Musket.

## Implementation

- Added a reference-deduplicating primary/secondary-hand selector.
- Required exactly one valid `FirearmDefinitionComponent` before repository
  access, preserving native Heavy Crossbow isolation.
- Added a common action kind, immutable decision, and dependency-free
  eligibility policy for Reload, Overhaul, and Repair.
- Added definition-owned ammunition identity to `ReloadProfile` and the
  marker-component round trip.
- Made Reload derive capacity and ammunition identity from the selected
  definition.
- Kept capacity-one, one-round, full-round Reload as the only supported
  transaction and failed closed for Sprint 33 behavior.
- Preserved the stable Test Musket abilities and accepted transaction/rollback
  services as compatibility adapters.

No new firearm content, class progression, grit, deeds, vendors, assets, or
enemy behavior was added.

## Automated evidence

The portable dependency-free suite passes 611 tests with zero failures,
including 12 focused generic-action and ammunition-profile cases. The main
runtime assembly compiles successfully against the exact supplied Kingmaker
2.1.7b private references.

## Runtime acceptance

Commit `0052dad0dae299eeefd302e511a0ae4b57dcdbac` passed exact-assembly
`mod-load-smoke`, then two consecutive independent fresh-process guarded
`generic-firearm-actions` runs:

- `20260801T0446479175229Z-feac50caa3fd439a80b9a09c7a383cc0`
- `20260801T0448285054152Z-4e5925080ce1422fbcb44c2ee07adcac`

Both runs used deployed DLL SHA-256
`de9f8507e5180adeb5df8dab4559e901da68022be556ef4fe1ffb874034e3d3f`,
loaded exactly `KMG_AUTOMATION_WORKING`, reached `MaintenanceLoopPassed` with
all exact-item, independent-second-item, revision, inventory, fault, duplicate,
Overhaul, Repair, and Reload checks passing, and observed
`nativeMarkers=0;markedMarkers=1`. Save-write sentinels observed no save API.
The game exited automatically without persisting fixture mutations.

The definition-driven Sprint 30 runtime gate is accepted. Previously qualified
action-bar delivery/interruption and persistence evidence remains carried
forward; this checkpoint did not reclassify those behaviors from indirect to
new direct evidence.
