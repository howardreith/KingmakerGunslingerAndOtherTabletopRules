# Sprint 7 entry criteria — firearm proficiency and development controls

## Goal

Add a Firearm Proficiency feature, enforce or expose Kingmaker's non-proficiency behavior for the Test Musket, and add development-only controls that can grant proficiency and place the Test Musket in a disposable party inventory for validation.

## Runtime gate

Sprint 7 must not be called runtime-complete until an actual UMM package has been compiled against the target Kingmaker/UMM installation and Sprint 6 has produced evidence that:

- the mod loads successfully;
- all three custom blueprints register once;
- `[firearms][test-musket.ready]` appears once;
- the native Heavy Crossbow assets remain usable;
- the custom Test Musket type contains exactly one firearm marker;
- the custom item references the custom type;
- new-game and save/load smoke tests do not crash.

Without that evidence, Sprint 7 may produce another source milestone but must not be labeled READY FOR KINGMAKER.

## Bounded implementation

Sprint 7 may add:

- the already reserved `KMG.Firearms.FirearmProficiency` blueprint;
- a marker/restriction component or engine adapter required to distinguish firearm proficiency;
- development controls in the UMM interface to grant proficiency, add one Test Musket, remove test items, and print equipped-firearm diagnostics;
- clear logging and reset behavior for those controls.

Sprint 7 must not add touch AC, ammunition, reload, misfire, per-item state, vendors, loot, class progression, or release-facing acquisition.

## Acceptance

- A character with firearm proficiency can use the Test Musket without the chosen non-proficiency consequence.
- A character without firearm proficiency receives the documented Kingmaker-compatible penalty or restriction.
- Development controls affect only the active disposable campaign and do not run automatically.
- Test items remain absent from ordinary campaign acquisition.
- Stable GUIDs remain collision-safe and no runtime GUID is generated.
