# Exact-reference compile evidence

## Result

Sprint 18 compiled the complete mod against the exact private managed-reference bundle exported from the target Pathfinder: Kingmaker installation.

```text
Game target:               Pathfinder: Kingmaker 2.1.7b
Unity Mod Manager:         0.32.4
Harmony:                   1.2.0.1
Target reference surface:  .NET Framework 4.7
Language version:          C# 7.3
Warnings as errors:        enabled
Compile exit code:         0
Compiler stderr:           empty
```

The resulting project-owned assembly has SHA-256:

```text
4d6297711f5304db726b5b729cd76d1aa44b888c9ff9f6e9c7d332f8e5492c5b
```

A repeat compile to the same output path produced the same byte sequence.

## Runtime references

The compiled assembly references:

```text
mscorlib 4.0.0.0
System 4.0.0.0
System.Core 4.0.0.0
Assembly-CSharp 0.0.0.0
UnityEngine.CoreModule 0.0.0.0
Newtonsoft.Json 8.0.0.0
0Harmony12 1.2.0.1
UnityModManager 0.32.4.0
```

The install archive does not redistribute any of those third-party assemblies.

## Defects found by the exact compile

The first exact-reference compile exposed and led to corrections for:

1. An output parameter that was not definitely assigned before a short-circuit lookup.
2. Two C# 7.3 scope collisions between LINQ lambda parameters and later local variables.
3. A direct compile-time dependency on `UnityEngine.IMGUIModule.dll`, which was not part of the private exporter allowlist.

The development panel now uses a reflection-only immediate-mode GUI facade. It resolves the running game's IMGUI types when available and fails closed with a diagnostic log if the runtime contract differs.

## Regression evidence

After the exact-reference fixes, the dependency-free suite was compiled against the .NET Framework 4.7 reference surface and executed three times:

```text
Completed 373 tests; failures=0.
Completed 373 tests; failures=0.
Completed 373 tests; failures=0.
```

All three stdout files were byte-identical, with SHA-256:

```text
3c7deee3a1c5c4b9f0e1d8a22dbdae2bc5d4b9a165a9108430731aee431637d4
```

## Remaining boundary

Compilation proves that the source matches the supplied assembly APIs. It does not prove Harmony callback behavior, live blueprint registration, Unity IMGUI resolution, inventory controls, or save persistence. Those are the purpose of the disposable-campaign smoke test.
