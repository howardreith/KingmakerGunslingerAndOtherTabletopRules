# ADR-0008: Dependency Policy

- **Status:** Accepted
- **Date:** 2026-07-12

## Context

A long-lived Kingmaker mod should minimize version conflicts and avoid importing Wrath-only frameworks.

## Decision

Runtime dependencies are limited to Kingmaker, its supported Unity Mod Manager installation, and the verified Harmony compatibility assembly. Call of the Wild, Cowboys and Demons, BlueprintCore, Wrath Modification Template, and other gameplay mods are not dependencies. Add no library unless it replaces substantial code and is maintained and compatible with net47/Mono.

## Consequences

- More project-owned blueprint helpers may be required.
- The mod remains usable in a relatively clean Kingmaker installation.
- Compatibility integrations remain optional adapters rather than hard dependencies.
