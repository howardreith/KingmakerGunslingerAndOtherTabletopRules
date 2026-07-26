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

## Remaining gate

The reconstructed 0.0.30 source is compile-qualified, not runtime-accepted.
Before Sprint 31, use the focused 0.0.30 smoke test to prove native Heavy
Crossbow non-leakage, unchanged Test Musket maintenance behavior, one Repair
interruption, and final persistence in Kingmaker.
