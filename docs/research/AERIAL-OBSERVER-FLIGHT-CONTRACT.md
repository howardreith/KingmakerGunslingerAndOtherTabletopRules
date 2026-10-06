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

The first candidate used a temporary +2 Trait during RuleSkillCheck. Real active
and cached rules passed twice, but a final complete stat-consumer review exposed
a gap: world-map preselection/discovery and fog radius read raw Perception before
any skill rule. That candidate proves those rule paths only; it is superseded
for full Perception coverage by the event-driven provider below.

Additional exact native paths:

- `LocationRevealController.Tick` (476 bytes) takes the maximum raw Perception,
  uses it for discovery radius and last-roll eligibility, then calls
  `GameHelper.CheckPartySkillResult(SkillPerception, ...)` for secret locations.
- `LocationRevealController.<>c.<Tick>b__0_0(UnitEntityData): int` is its pure
  native raw-stat reader. The fixture invokes only this reader, never the
  controller's Tick or location revelation.
- `FogOfWarSettings.get_Radius` (147 bytes) uses raw Perception on the global map.
  Its pure `<>c.<get_Radius>b__19_0(UnitEntityData): int` reader can be measured
  on a disposable actor without changing fog, party, camera or discovery state.
- Camp guard sorting and skill description UI also read the same raw stat.
  A maintained native Trait modifier covers these reads without patching them.

## Exact event-driven skill lifecycle

`Fact.Components` is a public `List<GameLogicComponent>` getter (7 bytes).
`GameLogicComponent.Fact` has a public setter. Native
`Fact.InitializeLogicInternal` (207 bytes) uses that setter for fact-local cloned
components; registered blueprint components remain a separate immutable source.
`Fact.TurnOn` (177 bytes) and `TurnOff(bool)` (129 bytes) invoke each instance
component's native `OnTurnOn`/`OnTurnOff` callbacks. `Buff.Activate` (52 bytes) and
`Deactivate` (47 bytes) preserve those callbacks. `UnitPartBuffSuppress.Update`
(105 bytes) deactivates before setting suppression and activates after release.
`IUnitBuffHandler` adds exact owner-filtered membership notifications.

The unpublished provider attaches one request-owned
`AerialObserverFlightTransition` component to each exact carrier fact on its own
unit. It changes **only that fact's instance component list**. It never changes
the canonical flight blueprint, its three source components, or another unit's
facts. A listener records the native TurnOn/TurnOff boundary, so activation is
observed after the carrier's mechanical components turn on, and deactivation
removes the bonus before a subsequent raw-stat or skill resolution.

`AerialObserverPerceptionBonus` maintains one **+2 Trait SkillPerception** modifier
per provider. Native descriptor rules handle duplicate/foreign modifiers.
Perception rule handling reconciles exact membership once per rule as a narrow
additional guard; it does not add another rule modifier. Removing the provider
detaches only its exact listener instances and removes only its own modifier.
Flight removal, inactivity, native suppression/release and later reactivation
are event driven. No frame/round polling, global patch, movement mutation,
altitude test, foreign-unit scan or registered identity is introduced.

The public list/setter, exact native lifecycle, skill stat and carrier graph are
verified before construction. A mismatch fails closed. Request-local listener
names are unique; provider blueprints have random request GUIDs and remain
unregistered, hidden, iconless and unreachable in ordinary play.

Required runtime evidence is two independent fresh-process PASS runs on one
artifact: grounded/absent, exact flight, active and cached Perception, the actual
native passive stat readers, immediate inactivity/removal, native suppression
and release, foreign Trait and duplicate providers, unrelated skill, independent
units, replay and exact instance-list/modifier/actor cleanup. No save write,
player acquisition, icon or ordinary save identity is created.