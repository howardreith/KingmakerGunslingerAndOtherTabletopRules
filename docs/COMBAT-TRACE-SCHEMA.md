# Firearm combat-trace schema

## Purpose

Sprint 9 retains marker-scoped combat tracing while the firearm AC rule is active. Tracing remains diagnostic, defaults off, and is restricted to weapons whose exact weapon type contains one `FirearmDefinitionComponent`. Disabling tracing does not disable touch-AC behavior.

## Record sequence

One attack should normally produce:

```text
[combat][trace.begin]
[combat][trace.event] ... WeaponAttack/Before
[combat][trace.event] ... AttackRoll/Before
[combat][trace.event] ... ArmorClass/Before
[combat][trace.event] ... ArmorClass/After
[combat][trace.event] ... AttackRoll/After
[combat][trace.event] ... WeaponAttack/After
[combat][trace.complete]
```

The actual sequence is evidence. Missing, additional, or repeated callbacks must not be edited out. A repeated stage/phase/event combination receives `callback=2` or higher and contributes to `duplicateCallbacks`.

## Correlation fields

Every line carries the same trace ID, formatted as `KMG-000001`.

| Field | Meaning |
|---|---|
| `trace` | Process-lifetime diagnostic trace ID. |
| `stage` | `WeaponAttack`, `AttackRoll`, or `ArmorClass`. |
| `phase` | `Before` or `After` the event's `OnTrigger(RulebookEventContext)`. |
| `event` | Runtime identity hash of the current event object. |
| `parent` | Nested callback's parent event identity, or `<none>`. |
| `callback` | Ordinal for duplicate callbacks with the same event/stage/phase. |
| `markerCount` | Firearm marker count; exact firearm identity requires `1`. |

## Data fields

Fields are emitted in ordinal key order so traces can be diffed deterministically.

| Field | Intended source |
|---|---|
| `weapon` | Concrete item-entity name/type. |
| `weaponRuntimeId` | Runtime item ID when exposed. |
| `itemBlueprint` / `itemBlueprintId` | Concrete weapon item blueprint. |
| `weaponType` / `weaponTypeId` | Exact weapon-type blueprint carrying the marker. |
| `firearmDefinition` | Immutable definition copied from the marker. |
| `initiator` / `target` | Rule-event units. |
| `naturalD20` | Natural d20 when exposed independently of totals. |
| `attackBonus` / `attackTotal` | Available attack arithmetic. |
| `attackResult` / `isHit` | Available outcome fields. |
| `targetAC` | AC selected by `RuleCalculateAC`. |
| `ordinaryAC` | Target's ordinary AC stat when exposed. |
| `touchAC` | Target's touch AC stat when exposed. |
| `distanceMeters` | `UnitEntityData.DistanceTo` result, with a position fallback. |
| `rangeIncrement` | Range increment derived from distance and firearm metadata; the gameplay selector allows a 0.1-millimeter boundary tolerance. |
| `source` | Ability, reason, or originating rule when exposed. |
| `isFullAttack` / `isFirstAttack` | `RuleAttackWithWeapon` command shape. |
| `isAttackOfOpportunity` / `attackNumber` | Additional command details when exposed. |

## Missing data

`<unavailable>` is a first-class result. It means the installed runtime did not expose a value through the inspected candidates. It must not be interpreted as zero, false, a miss, ordinary AC, or touch AC.

## Safety properties

- No trace patch accepts a `ref`, `out`, or `__result` parameter.
- No trace patch returns a Boolean that can suppress the original method.
- Trace and correlation code assigns no combat values. The separate `Rules/FirearmArmorClassRuntime.cs` adapter may write only the selected Int32 `TargetAC`.
- The correlator stores no event, unit, item, blueprint, or Unity object.
- Any diagnostic exception is caught and logged; active diagnostic state is cleared.
- Native Heavy Crossbows and other marker-free weapons are ignored.

## Sprint 9 firearm-AC decision records

In addition to correlated `[combat]` lines, tracing enables these focused records:

```text
[firearms][ac.touch-selected]
[firearms][ac.ordinary-selected]
[firearms][ac.duplicate-skipped]
```

The decision record includes distance, range increment, previous and selected target AC, applied delta, resolved target member, and reason. Native Heavy Crossbows emit none of these firearm records.

## Runtime evidence still required

A close and distant Test Musket shot must establish that the installed callback order, unit distance, AC members, and `TargetAC` mutation behave as the source contract expects. Full attacks and ability-generated attacks also require runtime evidence.
