# Sprint 3 Entry Criteria and Scope

## Goal

Replace the Sprint 2 loader stub with the non-invasive Unity Mod Manager/Harmony bootstrap and one-time lifecycle plumbing. No custom blueprint is registered until Sprint 4.

## Required local evidence

Before claiming runtime compatibility, capture:

- `environment.json` from the supported Kingmaker installation.
- A successful Debug and Release build.
- The exact assembly identity of `UnityModManager.dll` and `0Harmony12.dll`.
- Confirmation that `Harmony12.HarmonyInstance.Create` and `PatchAll` are available.
- Reflection output for `LibraryScriptableObject.LoadDictionary()`.
- A clean baseline with other gameplay mods disabled.

## Bounded Sprint 3 deliverables

```text
Bootstrap/Main.cs
Bootstrap/ModContext.cs
Bootstrap/ModLogger.cs
Bootstrap/BlueprintLifecyclePatch.cs
Bootstrap/BlueprintBootstrap.cs
```

The sprint will:

- Capture the UMM mod entry and logger.
- Create one Harmony instance using the mod ID.
- Patch the executing assembly exactly once.
- Add a postfix for the verified zero-argument `LibraryScriptableObject.LoadDictionary` signature.
- Add a one-time initialization guard.
- Log structured phase start/completion/failure records.
- Perform no custom blueprint registration and no gameplay changes.

## Acceptance

1. UMM calls the loader once and reports success.
2. Harmony patch installation succeeds without a duplicate-patch warning.
3. The blueprint-library postfix is observed exactly once per game launch.
4. The log sequence is deterministic and includes the mod version.
5. A disabled or failed bootstrap does not partially initialize content.
6. The install ZIP contains only project-owned output.

## Explicit exclusions

Sprint 3 does not create the diagnostic feature, class, firearm, settings UI, or save state.
