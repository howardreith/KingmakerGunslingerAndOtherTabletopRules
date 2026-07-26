# ADR-0004: Firearm Identity and Weapon-Category Adapter

- **Status:** Accepted
- **Date:** 2026-07-12

## Context

Kingmaker does not safely support adding a new enum member to `WeaponCategory`. Reusing a category is useful for animation and engine behavior but dangerous if it becomes the only firearm test.

## Decision

Attach a dedicated firearm definition/marker to every firearm. Use a vanilla crossbow category only as an engine adapter. Investigate `HandCrossbow` first if locally confirmed safe; otherwise use an appropriate crossbow category. Every firearm rule requires the marker.

## Consequences

- Real hand crossbows and unrelated mod items cannot accidentally become firearms from category alone.
- Firearm type/capacity/era can be data-driven.
- Some vanilla feat/UI systems may still require adapter code where they insist on a category.

## Rejected alternative

Treating all `HandCrossbow` weapons as guns.
