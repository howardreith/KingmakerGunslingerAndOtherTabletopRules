# ADR-0001: Target Runtime and Toolchain

- **Status:** Accepted
- **Date:** 2026-07-12

## Context

Kingmaker is an older Unity/Mono application. A new project must match the runtime conventions proven by established Kingmaker mods rather than assume a modern .NET runtime.

## Decision

Target Pathfinder: Kingmaker 2.1.7b on Windows for initial validation. Compile AnyCPU for .NET Framework 4.7 with C# 7.3. Use the Unity Mod Manager assembly installed for Kingmaker and the Harmony 1.2 compatibility API. Pin UMM 0.32.5 as the development setup baseline, while limiting code to APIs confirmed in the installed assembly.

## Consequences

- Modern C#/.NET features unavailable to net47/C# 7.3 cannot be used.
- Local game paths and assemblies are required to build and run.
- The project should avoid unnecessary libraries that may not behave under the old Mono runtime.
- Non-Windows/controller support is uncommitted until tested.

## Verification

Sprint 2 records actual installed assembly versions and builds a reference-only project.
