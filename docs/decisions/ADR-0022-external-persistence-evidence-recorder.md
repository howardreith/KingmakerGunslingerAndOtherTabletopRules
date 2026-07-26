# ADR-0022: External persistence evidence recorder

## Status

Accepted for Sprint 15 diagnostics. It is explicitly rejected as a save carrier.

## Context

The engine-item-identity UnitPart candidate cannot be accepted or rejected without observing a compiled build across save/load, process restart, merchants, duplication, migration, and compatibility cases. Hand-copied notes are too easy to mismatch across builds and runs.

## Decision

Add an external, build-fingerprinted evidence recorder that captures structured before/after snapshots for a fixed lifecycle matrix and evaluates a deterministic GO/NO-GO rule.

The recorder writes only to an `evidence/` directory next to the installed mod. Firearm mechanics never read those files.

## Consequences

Positive:

- Evidence is tied to the exact mod and game build.
- Reproduction requirements are enforced mechanically.
- Failed, blocked, and incomplete rows remain visible.
- A Markdown report can be shared without transcribing the UMM log.

Negative:

- The recorder adds development UI and diagnostic code.
- Lifecycle operations are still manual.
- External files can be deleted or edited, so they are evidence rather than authoritative save data.

## Rejected alternatives

- Treating static source validation as persistence proof.
- Starting ammunition work before the gate passes.
- Selecting another persistence carrier without an observed failure.
- Using the JSON file as a sidecar save database.
