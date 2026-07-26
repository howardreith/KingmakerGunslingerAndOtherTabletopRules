# ADR-0005: Weapon-Based Attack Pipeline

- **Status:** Accepted
- **Date:** 2026-07-12

## Context

A spell-like “Fire Gun” ability would simplify some checks but would detach firearms from ordinary weapon systems.

## Decision

Firearms remain `BlueprintItemWeapon` items and attack through the normal weapon rule pipeline. Focused handlers modify AC selection, misfire, loaded-state validation, ammunition consumption, damage properties, and grit events.

## Consequences

- Normal attack bonuses, criticals, iterative attacks, enhancement bonuses, and ranged feats remain available.
- Event ordering is more complex and requires instrumentation.
- Idempotency is required so one shot cannot consume ammunition twice.

## Rejected alternative

Delivering all firearm damage through an ability/spell damage action.
