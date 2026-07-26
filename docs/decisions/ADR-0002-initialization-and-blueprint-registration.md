# ADR-0002: Initialization and Blueprint Registration

- **Status:** Accepted
- **Date:** 2026-07-12

## Context

Custom blueprints must be registered after Kingmaker loads its blueprint dictionary, and registration must happen once.

## Decision

Use Unity Mod Manager `Main.Load` to initialize logging/settings and install Harmony patches. Patch `LibraryScriptableObject.LoadDictionary` with a postfix guarded against repeated execution. The postfix captures the blueprint library and invokes an explicit ordered module runner.

## Consequences

- Blueprint code does not run prematurely from `Main.Load`.
- Initialization order is deterministic.
- Every module can report contextual failure.
- Method signature and timing must be verified against 2.1.7b.

## Rejected alternatives

- Static constructors that mutate blueprints unpredictably.
- Patching a later UI scene as the main registration point.
- Re-running all registration every time the dictionary method appears.
