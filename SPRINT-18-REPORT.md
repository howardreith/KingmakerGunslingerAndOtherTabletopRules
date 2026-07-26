# Sprint 18 report — exact-reference runtime smoke candidate

Version: `0.0.18-s18-runtime-smoke-candidate`

## Result

The full Kingmaker mod compiled successfully with Roslyn 4.7.0 against the exact private assembly set exported from the user's Pathfinder: Kingmaker 2.1.7b installation, including Unity Mod Manager 0.32.4 and Harmony 1.2.0.1.

## Compiler-discovered fixes

- Initialized `BlueprintWeaponEnchantment` output before a short-circuit dictionary lookup.
- Renamed two LINQ lambda variables that conflicted with later locals under the C# 7.3 scope rules.
- Removed the omitted `UnityEngine.IMGUIModule.dll` compile-time dependency by introducing a reflection-only, fail-closed immediate-mode GUI adapter. The running game already references that Unity module.
- Matched the UMM minimum version in `Info.json` to the supplied 0.32.4 runtime.

## Evidence

- Full main-project compile: success, warnings treated as errors.
- Main DLL target: .NET Framework 4.7, AnyCPU / PE32 Mono-compatible assembly.
- Dependency-free regression suite: 373 tests, three runs, zero failures, byte-identical output.
- Install archive contains only the project DLL, UMM metadata, blueprint data, and smoke-test guide. It contains no game, Unity, UMM, Harmony, or Newtonsoft binaries.

## Remaining gate

The build is ready for a Kingmaker smoke test, not yet accepted for campaign use. Kingmaker must now prove Harmony installation, blueprint registration, IMGUI panel rendering, Test Musket spawning/equipping/firing, strict item identity, and persistence behavior.
