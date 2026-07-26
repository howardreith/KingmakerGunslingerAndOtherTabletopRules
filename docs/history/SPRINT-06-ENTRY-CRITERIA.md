# Sprint 6 entry criteria — Test Musket blueprint

## Goal

Register a clone-based Test Musket weapon type and item that can be equipped and used as an ordinary ranged weapon. Attach the Sprint 5 `FirearmDefinitionComponent`, but do not yet add firearm proficiency, touch AC, ammunition, reload, or misfire behavior.

## Required runtime evidence before claiming completion

Sprint 6 must not be called runtime-complete until the following Sprint 5 evidence exists from a real Kingmaker installation:

- `scripts/inspect-runtime-contracts.ps1` passes;
- all 38 pure domain tests pass;
- Debug and Release mod builds complete with zero warnings;
- the Sprint 5 package loads in Kingmaker;
- exactly one diagnostic blueprint registration appears in logs;
- main menu, character creation, new game, existing-save load, new save, and reload smoke tests pass;
- `BlueprintComponent` is confirmed non-sealed, derives from `UnityEngine.ScriptableObject`, and the custom marker round-trip emits exactly one `firearms/domain.ready` event without type-initialization errors.

If this evidence is unavailable, Sprint 6 may produce a source package and discovery tooling, but it must remain non-runtime-certified.

## Native candidate confirmation

Before cloning, confirm in the installed library that these candidates have the expected exact runtime types:

| Purpose | Candidate | GUID |
|---|---|---|
| Weapon type | Heavy Crossbow | `36d0551b8a28587438a47fcbbf53c083` |
| Item | Standard Heavy Crossbow | `19a5092244dcf99478dcd73c974828b1` |

The original vanilla objects must never be mutated.

## Bounded deliverables

- Activate the existing reserved IDs:
  - `KMG.Test.TestMusketWeaponType`
  - `KMG.Test.TestMusketItem`
- Clone the confirmed heavy-crossbow type and item.
- Give both custom assets unique names.
- Attach one validated early-musket definition:
  - capacity 1;
  - 40-foot range increment;
  - misfire value 2;
  - full-round base reload;
  - no scatter.
- Add a local-only inspection path or exact debugger steps proving the marker can be read back.
- Add no player acquisition route yet; developer spawn controls remain Sprint 7.

## Explicit exclusions

- No proficiency.
- No inventory spawn command in shipped source.
- No touch-AC behavior.
- No loaded/empty state.
- No ammunition consumption.
- No misfire behavior.
- No custom projectile, icon, sound, model, or animation.
- No vanilla blueprint mutation.

## Acceptance

1. Both custom blueprints register collision-safely under their reserved IDs.
2. The item references the custom weapon type, not the vanilla type.
3. The type carries exactly one `FirearmDefinitionComponent` whose reconstructed definition matches the intended musket values.
4. A locally injected Test Musket can be equipped and perform an ordinary ranged attack without crashing.
5. Removing the temporary local injection path leaves no acquisition route in the shipped package.
6. All Sprint 5 domain tests and bootstrap regressions continue to pass.
