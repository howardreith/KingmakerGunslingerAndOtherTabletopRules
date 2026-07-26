# ADR-0013 — Clone native weapon blueprints for the first firearm slice

**Status:** accepted
**Date:** 2026-07-12

## Context

The first firearm must participate in Kingmaker's ordinary weapon pipeline, but authoring every weapon field and visual reference from memory would create unnecessary compatibility risk. Call of the Wild demonstrates a stable Kingmaker pattern of cloning blueprints with `UnityEngine.Object.Instantiate` and then registering the clone under a new GUID.

## Decision

Sprint 6 clones the native Heavy Crossbow weapon type and Standard Heavy Crossbow item. Only the clones are mutated:

- the weapon type receives one `FirearmDefinitionComponent`;
- the item is rewired to the custom weapon type;
- both receive stable manifest GUIDs and unique internal names.

Native source identities and references are validated before and after cloning. Registration is collision-safe and transactionally rolled back on failure.

## Consequences

The Test Musket inherits crossbow presentation and ordinary ranged-weapon behavior, which is desirable for the vertical slice. It also temporarily inherits crossbow statistics and category interactions. Those inherited details are not promises of final firearm behavior and must be audited in later sprints.

The design avoids a spell/ability-only surrogate and keeps future touch-AC, ammunition, and misfire rules attached to real weapon attacks.
