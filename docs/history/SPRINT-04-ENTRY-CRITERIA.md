# Sprint 4 Entry Criteria and Scope

## Goal

Add stable blueprint-manifest loading, collision validation, and one invisible diagnostic blueprint to prove that the Sprint 3 lifecycle can safely register a project-owned asset. No firearm, class, feat, item, ability, UI, or save state is created.

## Required local evidence before claiming runtime completion

- `environment.json` from the supported Kingmaker installation.
- Successful Debug and Release builds of Sprint 3.
- Exact assembly identities for `UnityModManager.dll`, `0Harmony12.dll`, and `Assembly-CSharp.dll`.
- Reflection confirmation that `LibraryScriptableObject.LoadDictionary()` is an instance method with zero parameters.
- A clean launch showing one Harmony installation and one blueprint initialization.
- No other gameplay mods enabled during the baseline run.

If local runtime evidence is unavailable, Sprint 4 may still produce a source package and explicit test harness, but it must not claim an installable/runtime-verified milestone.

## Bounded Sprint 4 deliverables

```text
Blueprints/BlueprintId.cs
Blueprints/BlueprintManifest.cs
Blueprints/BlueprintRegistry.cs
Blueprints/DiagnosticBlueprints.cs
```

The sprint will:

- Load the copied `blueprints/blueprints.json` from the mod directory.
- Reject missing, malformed, duplicate, or runtime-generated IDs.
- Resolve one permanently reserved diagnostic feature ID.
- Check the loaded Kingmaker library for a GUID collision before creation.
- Create one invisible/non-selectable diagnostic `BlueprintFeature`.
- Register it once through the existing `BlueprintBootstrap.InitializeCore` path.
- Verify that a second initialization attempt cannot create or replace it.
- Log the exact symbolic name and GUID without dumping unrelated game assets.

## Acceptance

1. The Sprint 3 loader and Harmony counts remain one.
2. The manifest loads from the installed mod directory.
3. A deliberate duplicate manifest symbol or GUID fails closed before asset creation.
4. A deliberate collision with an existing game blueprint fails closed.
5. The diagnostic feature exists by its reserved GUID after initialization.
6. It is not presented as a normal selectable feat and changes no unit behavior.
7. Save/load, character creation, and a new game remain behaviorally unchanged.
8. The install ZIP contains only the project DLL, metadata, and manifest files.

## Explicit exclusions

Sprint 4 does not create firearm definitions, classes, proficiencies, weapons, ammunition, localization infrastructure beyond the diagnostic asset, settings UI, commands, or persistent state.
