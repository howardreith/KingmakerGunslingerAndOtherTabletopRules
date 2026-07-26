# ADR-0009: Blueprint Manifest Deployment

- **Status:** Accepted
- **Date:** 2026-07-12

## Context

The stable blueprint-ID manifest must be available to development diagnostics and, beginning with Sprint 4, runtime initialization. Embedding it would make accidental mismatch between source tooling and the loaded resource harder to inspect during the early milestones.

## Decision

Copy `blueprints/blueprints.json` and `blueprints/blueprints.schema.json` into the build and install output beneath `blueprints/`. The source-controlled JSON remains the only authority. The build does not generate, transform, or rewrite identifiers.

## Consequences

- Packages gain two small project-owned JSON files.
- Runtime initialization can log and validate the exact deployed manifest.
- Players and bug reports can inspect the active ID set.
- A later move to an embedded resource requires a new decision and a migration-safe compatibility plan.
