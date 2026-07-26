# ADR-0007: Asset and Animation Strategy

- **Status:** Accepted
- **Date:** 2026-07-12

## Context

Custom models and animations are the least certain part of the Kingmaker pipeline and can consume disproportionate effort.

## Decision

Ship mechanical milestones using crossbow-compatible visuals and animations. Keep custom models, sounds, icons, projectiles, and animations independently replaceable. Package substantial custom 3D assets separately; the core mod must run without them.

## Consequences

- Mechanical progress is not blocked by rigging or asset-bundle work.
- Early visuals may be imperfect but explicitly documented.
- Asset provenance and licensing can be isolated.
- Dual-wield polish is post-1.0 work.
