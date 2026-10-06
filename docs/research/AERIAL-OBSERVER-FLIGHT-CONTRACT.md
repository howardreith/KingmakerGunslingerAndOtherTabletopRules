# Aerial Observer flight and Perception contract — 2026-10-05

Mission base: `6a4dc1b26350c1045171b42099ef2ac5584e303d`.

Classification: **EXACT-MECHANICAL-FLIGHT-CARRIER**, bounded to the existing
project's released **Wings of Air** mechanical flight abstraction. Aerial
Observer itself is **ADAPTED**: +2 Trait Perception while this exact flight
state is active, not tabletop altitude above ground. This is not universal
support for another mod's flight or three-dimensional navigation.

Native authority: `Assembly-CSharp.dll` MVID
`07fa1e4d-8618-41b3-9b8d-faa17d3b26f7`, SHA-256
`3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`.
Exact private inspection artifacts remain ignored; no assembly/IL is committed.

## Candidate ledger

| Candidate | Exact findings | Disposition |
|---|---|---|
| `UnitCondition` | Installed enum has no Flying/Airborne/Levitation state | No carrier |
| Unit parts, creature blueprint flags | Complete installed metadata/IL exposes no qualified general gameplay-flight part/flag | No inferred native carrier |
| `UnitEntityData.FlyHeight: float` | Auto-property; consumed by view and movement-agent position calculations; also written by ability delivery and jump/pit controllers | Rejected: shared jump/fall/presentation height, not a flight identity |
| `AbilityCustomFly` | One-shot delivery/animation with `MaxHeight`, `FlyUpTime`, takeoff and landing curves; writes `FlyHeight` | Rejected as a persistent trait condition; do not substitute jump/flight animation |
| Native locomotion/navmesh/agent/view | Height and presentation carriers do not uniquely prove flight; ordinary navigation remains two-dimensional | No model, terrain height or hover inference |
| Difficult-terrain immunity / Feather Step | Native `AddConditionImmunity(DifficultTerrain)` alone also belongs to grounded Feather Step | Rejected as a flight predicate |
| Ground-descriptor immunity | `BuffDescriptorImmunity(Ground)` is mechanically meaningful but can be independent of flight | Rejected as a generic flight predicate |
| Wings of Air feature alone | Armor-gated controller removes its effect under disallowed armor | Feature ownership alone is insufficient |
| Exact active Wings of Air buff | Stable existing identity and complete native mechanical graph below | Accepted carrier, checked by canonical object identity and active/nonsuppressed state |
| Visual wings, hover, polymorph/model shape, elevation | No exact mechanically meaningful flight identity follows from appearance alone | Explicitly nonqualifying |

No optional-provider GUID is guessed and no native/foreign blueprint is changed.
If another mechanical flight carrier is later supported, it needs its own exact
identity and independent qualification; this mission does not broaden the rule.

## Accepted identity and full native graph

Existing baseline symbol: `KMG.ElementalRaces.Feats.WingsOfAir.Buff`.
GUID: `e116e1e0a17a4aceb001000000000019`.
This already published buff is not an Aerial Observer identity or new acquisition.

`ElementalFeatBlueprintFactory.CreateWingsBuff` constructs exactly:

1. `ACBonusAgainstAttacks`: +3 Dodge, melee only, no armor-category/shield/touch
   or AoO-only restriction.
2. `AddConditionImmunity`: `UnitCondition.DifficultTerrain`.
3. `BuffDescriptorImmunity`: exact `SpellDescriptor.Ground`, no fact check or
   bypass feature.

`ElementalWingsOfAirController` already grants/removes that buff under its armor
contract. Existing `ElementalTreacherousScenario` calls this exact carrier
"released-flight" and observes native terrain movement/immunity; mere Feather
Step is a separate grounded control. This supplies a current repository gameplay
precedent instead of a visual-flight guess. No prior flight source is edited.

The unpublished builder must reject a wrong GUID, incomplete graph, wrong
component values or missing exact native skill/cleanup contracts. At rule time
only a buff whose blueprint is **the verified canonical object**, `Active` is
true and `IsSuppressed` is false may qualify. An equal name, matching descriptor,
visual height or cloned object with a copied GUID is not enough.

## Perception paths and narrow mechanism

- `StatType.SkillPerception` is native value 20 (`0x14`).
- `RuleSkillCheck(UnitEntityData, StatType, int)` does not capture StatValue in
  its constructor. `OnTrigger(RulebookEventContext)` (377 bytes) reads the
  current exact stat's `ModifiedValue` before calculating the roll.
- `PartyPerceptionController.RollPerception(UnitEntityData, StaticEntityData)`
  (199 bytes) constructs/triggers a real `RuleSkillCheck` for SkillPerception;
  this is the ordinary passive map-object discovery route.
- `RuleCachedPerceptionCheck(UnitEntityData, int)` (11 bytes) derives from
  RuleSkillCheck and fixes SkillPerception. Its `OnTrigger` (106 bytes) uses the
  native cached d20 and invokes the same base stat resolution; it is the cached
  detection path, not a reason to retain a stale flight modifier.
- `IInitiatorRulebookHandler<T>` is contravariant in its exact native event type.
  Runtime must prove the base skill handler receives cached Perception too.

A per-fact `RuleInitiatorLogicComponent<RuleSkillCheck>` can evaluate the exact
carrier immediately before resolution and add a temporary **+2 Trait** to the
native Perception stat, using `RulebookEvent.AddTemporaryModifier` for cleanup.
This preserves native Trait stacking, affects both rule paths, and leaves no
persistent overlay after flight ends. No polling, movement patch, global stat
patch, scan of foreign units or flight-state mutation is needed. Cached rolls
remain native. UI character-sheet presentation between checks is future visible
publication work, not claimed by this unpublished rule-time foundation.

Required runtime evidence is two independent fresh-process PASS runs on one
artifact because the provider is a custom handler: grounded/absent, exact flight,
active and cached Perception, loss/suppression, foreign Trait and duplicate
providers, unaffected skill, independent units, replay and exact cleanup.
No player acquisition, icon or ordinary save identity is created.
