# ADR-0024: Execute the net47 harness and require exact private runtime references

## Status

Accepted for Sprint 17.

## Context

Sprint 16 had 371 declared tests but no compiler or runtime execution in the packaging environment. It also could not compile the main mod because the selected Kingmaker, Unity, Harmony, and Unity Mod Manager assemblies were unavailable.

Continuing into ammunition without executable evidence would preserve known uncertainty in both the pure rules and the runtime boundary.

## Decision

Sprint 17 will:

1. Compile the 373-case dependency-free harness against the official .NET Framework 4.7 reference surface with C# 7.3 and warnings as errors.
2. Execute it repeatedly and retain machine-readable evidence.
3. Fix every defect exposed by that run and add regression coverage.
4. Add a private reference exporter limited to the ten exact managed assemblies required by the project.
5. Add a cross-platform compiler that can create a UMM compile candidate once that private bundle is supplied.
6. Keep the persistence gate at `NoGoIncomplete` until the candidate runs in Kingmaker and the lifecycle matrix passes.

## Consequences

- Pure-code evidence advances from static/model validation to executed tests.
- Sprint 17 adds no firearm gameplay content or blueprint IDs.
- The main DLL remains unavailable until exact runtime references are supplied.
- A compile candidate is not described as runtime-qualified.
- Proprietary assemblies remain private inputs and are never redistributed.
