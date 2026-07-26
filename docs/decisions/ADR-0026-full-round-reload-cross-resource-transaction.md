# ADR-0026: Full-round reload as a verified cross-resource transaction

## Status

Accepted for Sprint 21 smoke testing.

## Context

A reload changes two independently persisted resources: shared-inventory ammunition and state attached to one exact firearm item. Treating these as unrelated writes risks consuming ammunition without loading the gun, or loading the gun without consuming ammunition.

Kingmaker also permits an ability command to be selected and then cancelled. Inventory must not be consumed merely because the player clicked the action.

## Decision

Implement `Reload Test Musket` as a personal extraordinary full-round `BlueprintAbility` whose custom delivery component performs one transaction only when delivery executes.

The transaction has pure ports for firearm state and inventory. It runs all eligibility checks before mutation, consumes one Black Powder Charge and one Lead Ball, writes one loaded round to the exact Test Musket, verifies both resources, and attempts exact restoration of both resources after any failure.

Firearm Proficiency grants the ability through one `AddFacts` component. The disposable-save development command also repairs a missing ability directly because an already-instantiated feature fact may predate the new component.

## Consequences

- Cancellation before delivery should not consume resources.
- Failure cannot be reported as success merely because one resource changed.
- Rollback errors remain observable instead of being hidden by the original exception.
- The ability remains limited to the capacity-one Test Musket and basic Lead Ball load.
- Attack-time loaded-state enforcement is a separate sprint and does not complicate this transaction.
