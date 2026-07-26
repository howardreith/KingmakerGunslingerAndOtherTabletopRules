# Sprint 3 Completion Report

## Milestone

- **Sprint:** 3 — Unity Mod Manager and Harmony bootstrap
- **Version:** `0.0.3-s03-bootstrap`
- **Prepared:** 2026-07-12
- **Target:** Pathfinder: Kingmaker Enhanced Plus Edition 2.1.7b
- **Runtime baseline:** .NET Framework 4.7, C# 7.3, AnyCPU
- **Loader baseline:** Unity Mod Manager 0.32.5
- **Patch API:** Harmony12 compatibility surface

## Goal

Replace the Sprint 2 loader stub with a non-invasive, one-time Unity Mod Manager/Harmony bootstrap and a guarded `LibraryScriptableObject.LoadDictionary()` lifecycle hook. Sprint 3 must not create blueprints or change gameplay.

## Delivered source

```text
src/KingmakerGunslinger/
├── Main.cs
└── Bootstrap/
    ├── BlueprintBootstrap.cs
    ├── BlueprintLifecyclePatch.cs
    ├── ModContext.cs
    └── ModLogger.cs
```

### `Main.cs`

- Exposes the `KingmakerGunslinger.Main.Load` UMM entry point.
- Uses an explicit process-lifetime loader state machine.
- Rejects concurrent or post-failure retries.
- Treats a duplicate post-success load as a no-op.
- Publishes the mod context before installing Harmony patches so a lifecycle callback can always find its owner.
- Returns `false` on bootstrap failure rather than leaving content partially active.

### `ModContext.cs`

- Owns the UMM entry, executing assembly, structured logger, mod ID, and Harmony instance.
- Creates one `Harmony12.HarmonyInstance` using the UMM mod ID.
- Calls `PatchAll` exactly once.
- Does not report readiness until patch installation finishes.
- Moves to a permanent failed state when patching or later lifecycle initialization fails.

### `ModLogger.cs`

- Emits a stable, single-line envelope:

```text
[KMG][KingmakerGunslinger][0.0.3-s03-bootstrap][LEVEL][phase][event] message
```

- Includes the assembly informational version in every structured record.
- Normalizes multiline exception text for UMM logs.
- Never allows a logging exception to escape into the game loader.

### `BlueprintLifecyclePatch.cs`

- Patches the verified Kingmaker lifecycle shape:

```csharp
[HarmonyPatch(typeof(LibraryScriptableObject), "LoadDictionary")]
[HarmonyPatch(typeof(LibraryScriptableObject), "LoadDictionary", new Type[0])]
```

- Uses a postfix receiving `LibraryScriptableObject __instance`.
- Delegates all lifecycle behavior to `BlueprintBootstrap`.
- Contains all unexpected exceptions so a mod failure cannot break Kingmaker's blueprint-loading method.

### `BlueprintBootstrap.cs`

- Retains the first observed library instance as authoritative.
- Supports either timing order:
  - patch installation completes before `LoadDictionary`, or
  - `LoadDictionary` is observed while patch installation is still completing.
- Initializes only after both a library and a ready mod context exist.
- Suppresses later observations and records them diagnostically.
- Fails closed on an invalid library or initialization exception.
- Performs no asset registration or gameplay mutation in Sprint 3.

## Controlling invariants

1. `HarmonyInstance.Create` appears once in project source.
2. `PatchAll` appears once in project source.
3. The UMM loader can transition from `NotStarted` to only `Loading`, `Loaded`, or `Failed`.
4. A failed loader is never retried in the same process.
5. A failed mod context is never considered ready.
6. Blueprint initialization requires both the first observed library and a ready context.
7. Blueprint initialization increments its completion counter in one location only.
8. The lifecycle postfix never deliberately throws into Kingmaker.
9. `InitializeCore` contains no blueprint creation or mutation in this milestone.
10. Runtime blueprint GUID generation remains disabled.

## Static validation completed

The Sprint 3 validator checks:

- Required bootstrap, project, metadata, documentation, and script files.
- Version alignment across UMM metadata, shared build properties, and assembly attributes.
- All nine stable blueprint-ID reservations.
- .NET Framework 4.7 and the non-copying external-reference policy.
- The exact six C# compile items.
- Lexical delimiter balance for every C# source file.
- Full tree-sitter syntax parses for all six C# files and all ten PowerShell scripts in the packaging environment.
- One `HarmonyInstance.Create` and one `PatchAll` call.
- The zero-argument `LoadDictionary` patch declaration.
- Publication-before-patching order.
- Loader, context, and blueprint state guards.
- Absence of blueprint mutation from `InitializeCore`.
- Absence of runtime GUID generation and packaged binaries.
- Six modeled lifecycle scenarios, including duplicate observations and failure isolation.

Machine-readable results are written to `validation/static-validation.json`, `validation/tree-sitter-csharp.json`, and `validation/tree-sitter-powershell.json`.

## Runtime acceptance still pending

This environment does not include the user's Kingmaker installation, its managed assemblies, Windows MSBuild, or the .NET Framework 4.7 targeting pack. Therefore this milestone does **not** claim:

- a successful Debug or Release compile against the actual installation;
- confirmed reflection of the installed `LoadDictionary()` method;
- successful Harmony patch installation in Kingmaker;
- an observed in-game postfix;
- an installable UMM ZIP.

The included Windows scripts collect the necessary environment fingerprint, compile the project, validate the owned-file output, and package it after those local prerequisites are available.

## Expected runtime evidence

A clean launch should establish this partial order in the UMM log:

```text
bootstrap/load.start
harmony/patch.start
harmony/patch.complete
bootstrap/load.complete
blueprints/lifecycle.observed
blueprints/initialize.start
blueprints/content.skipped
blueprints/initialize.complete
```

`lifecycle.observed` may appear between `patch.start` and `patch.complete` if the target method executes during patch installation. Even in that case, `initialize.start` must occur only after `patch.complete`.

The required counts are:

```text
Harmony patch installation: 1
LoadDictionary observations on a normal launch: expected 1
Blueprint initialization: 1
Custom blueprints registered: 0
```

## Package boundary

The milestone contains source, documentation, validation output, checksums, and packaging scripts. It contains no Owlcat, Unity, Harmony, Newtonsoft.Json, or UMM binary and no substitute/stub-linked mod DLL.

## Sprint 4 readiness

Sprint 4 may begin after local runtime evidence confirms the loader, Harmony API, and lifecycle method. Its bounded feature is one invisible diagnostic blueprint plus manifest loading, collision detection, and a blueprint registration smoke test. See `planning/SPRINT-04-ENTRY-CRITERIA.md`.
