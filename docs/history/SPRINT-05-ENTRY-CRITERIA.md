# Sprint 5 entry criteria — firearm domain model

## Goal

Introduce the immutable firearm-definition domain vocabulary and a marker blueprint component without creating a firearm weapon, attack rule, proficiency, item, ability, UI entry, or save state.

## Prerequisite runtime evidence

Before Sprint 5 can be called runtime-complete, Sprint 4 must pass the Windows/Kingmaker acceptance matrix in `TESTING.md`, including:

- exact installed assembly contract report;
- warning-free Debug and Release builds;
- one successful in-game diagnostic registration;
- lookup of the diagnostic GUID as the exact hidden, component-free feature;
- no duplicate registration across lifecycle transitions;
- clean new-game and save/load smoke tests.

If that evidence remains unavailable, Sprint 5 may still produce a source package and pure domain tests, but it must remain explicitly non-runtime-certified.

## Bounded deliverables

Expected source boundary:

```text
Firearms/
├── FirearmDefinition.cs
├── FirearmEra.cs
├── FirearmKind.cs
├── ReloadProfile.cs
└── FirearmDefinitionComponent.cs
```

The exact names may change if installed assembly inspection requires it, but the responsibilities may not expand.

## Required behavior

- `FirearmDefinition` is immutable after construction.
- It validates capacity, range increment, misfire value, firearm era, firearm kind, reload profile, and scatter flag.
- Invalid combinations fail before a blueprint component is attached.
- `FirearmDefinitionComponent` is a passive marker/configuration component only; it does not listen to combat events or store mutable per-item state.
- Firearm identity never depends solely on a borrowed vanilla weapon category.
- Pure domain tests cover representative valid definitions and every rejected invariant.
- No new blueprint GUID is activated unless an actual custom blueprint is registered in Sprint 5. A component type alone does not require a blueprint ID.

## Explicit exclusions

- No Test Musket; scheduled for Sprint 6.
- No firearm proficiency; scheduled for Sprint 7.
- No developer spawn controls; scheduled for Sprint 7.
- No touch-AC rule; scheduled for Sprint 9.
- No loaded ammunition or per-item state.
- No Harmony patch beyond the existing lifecycle patch.
- No custom icon, projectile, sound, model, or animation.

## Acceptance

1. Existing Sprint 4 validation still passes.
2. Harmony creation and `PatchAll` counts remain exactly one.
3. Domain tests demonstrate deterministic validation and value equality.
4. The marker component contains definition data only and no mutable runtime state.
5. No player-visible blueprint or gameplay mutation is added.
6. Source and package remain free of proprietary binaries.
