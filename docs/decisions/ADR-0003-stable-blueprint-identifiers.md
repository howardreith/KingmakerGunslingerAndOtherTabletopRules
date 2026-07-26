# ADR-0003: Stable Blueprint Identifiers

- **Status:** Accepted
- **Date:** 2026-07-12

## Context

Custom blueprint IDs may be serialized into saves. Unstable or reused IDs can break progression and corrupt references.

## Decision

Maintain a checked-in semantic-key-to-ID registry. Generate an ID once through an explicit development command and commit it. Release builds fail on missing IDs. Semantic renames preserve the underlying ID through aliases/migrations. Startup diagnostics detect duplicates, collisions, and type mismatches.

## Consequences

- Blueprint IDs are public save-file API.
- Registry review is part of every content change.
- Deleting/reusing an ID is prohibited.
- Migrations are required when content structure changes.
