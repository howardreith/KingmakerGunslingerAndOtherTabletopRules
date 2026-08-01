# Current architecture

## Sprint 29 current layer

Version 0.0.29 retains the accepted token-backed runtime architecture, condition-preserving Broken reload, exact loaded-round enforcement, exact natural-d20 misfire classification, exact-item Normal → Broken → Wrecked transitions, native five-foot Reflex-half second-misfire burst, and same-item Wrecked → Broken Overhaul.

Sprint 29 completes the first player-facing Test Musket maintenance loop with a separate full-round personal extraordinary Repair action. Completed Repair consumes exactly one Firearm Repair Kit and atomically changes the exact equipped empty/Broken item to empty/Normal. It verifies unchanged repository identity and in-process runtime reference, exactly one revision increment, exact one-kit consumption, and rollback of both resources after a fault. Reload remains a separate operation and the only stage that consumes Black Powder and a Lead Ball.

Sprint 29 also adds a deterministic two-item maintenance fixture, a pure process-local PASS/FAIL evaluator, and a one-command immediate transaction runner. These diagnostics accelerate regression checks but never participate in gameplay decisions or persistence; manual action-bar testing still proves full-round delivery and interruption behavior.

The destructive Test Musket cleanup diagnostic still requires a separate arm and confirm action. This is a test-harness safety boundary rather than firearm gameplay.

The active vertical slice includes Firearm Proficiency, twenty-four active blueprints, stackable repair and ammunition resources, atomic Overhaul/Repair/Reload transactions, quicksave token reconciliation, range-limited touch AC, loaded-round attack enforcement, exact-firearm natural-d20 misfire detection, condition transitions, native five-foot burst delivery, and exact-item lifecycle diagnostics.

The item-owned inert `BlueprintWeaponEnchantment` state token remains authoritative. The runtime-rejected `ItemEntityWeapon.UniqueId` vault remains rejected.

# Historical Sprint 17 executed-evidence and handoff layer

Sprint 17 leaves the firearm persistence carrier, blueprint ledger, bootstrap, touch-AC rule, and runtime preflight unchanged. It adds three non-gameplay capabilities:

1. Exact C# 7.3 compilation of the dependency-free suite against the .NET Framework 4.7 reference surface.
2. Retained three-run executable evidence for all 373 tests, including fixes for equality recursion and invalid-distance fail-open behavior.
3. A private managed-reference exporter and cross-platform compile-candidate builder for the exact Kingmaker/UMM runtime boundary.

No evidence file is read by firearm mechanics. No proprietary assembly enters source or release output. Ammunition remains blocked until the in-game persistence matrix evaluates to `Go`.

## Historical Sprint 16 qualification layer

Sprint 16 leaves Sprint 14's engine-issued `UniqueId` plus save-owned `UnitPart` vault unchanged. It adds a pure I01/I02 evaluator, an engine probe, strict identity-only evidence snapshots, a deterministic A-D fixture for I03, and a local qualification workflow. These are diagnostics and test fixtures, not a new save carrier. Ammunition and reload work remains blocked until the 35-row runtime matrix evaluates to `Go`.

# Architecture

## 1. Decision summary

Kingmaker Gunslinger is a standalone Unity Mod Manager mod for **Pathfinder: Kingmaker Enhanced Plus Edition 2.1.7b**. It uses Kingmaker's blueprint lifecycle and legacy Harmony 1.2 integration rather than importing Wrath's BlueprintCore or modification-template stack.

The current Test Musket remains a real weapon using Kingmaker's native attack and damage pipeline. Firearm identity comes only from exactly one `FirearmDefinitionComponent` on the concrete weapon type. The Heavy Crossbow category, presentation, name, icon, and model are adapters and never sufficient firearm identity.

The authoritative runtime state carrier is the exact item's inert enchantment token. The earlier weak repository, direct-reference UnitPart, and `UniqueId` vault implementations remain checked in for test history and migration research, but none is the current runtime source of truth and the rejected `UniqueId` design must not be revived.

Sprint 28 is runtime-accepted from the supplied player-facing Overhaul evidence and explicit user approval. Sprint 29 completes the staged Overhaul → Repair → Reload maintenance loop and adds deterministic qualification automation. Sprint 30 is gated on live proof of the complete action-bar loop, interruption safety, exact resource deltas, same-item identity, second-item isolation, fail-closed rejection, matrix output, and persistence.

## 2. Runtime boundaries

```text
KingmakerGunslinger/
  Bootstrap/       UMM entry point, logging, Harmony, blueprint lifecycle
  Blueprints/      Blueprint creation, cloning, registration, verification
  Firearms/        Definitions, immutable state, exact-item repository, engine adapters
  Recovery/        Player-facing Overhaul and Repair availability, delivery, rollback, diagnostics
  Qualification/   Pure process-local maintenance baseline, observation, and PASS/FAIL evaluator
  Development/     Manual UMM controls and fail-closed reflection adapters
  Diagnostics/     Marker lookup, event snapshots, correlation, formatting
  Rules/           Pure firearm rules and thin Kingmaker mutation adapters
  Explosions/      Pure second-misfire policy, exact runtime adapter, diagnostics
  Services/        Future inventory and class business rules
  Engine/          Future save, item, and additional Harmony adapters
```

Blueprint factories compose engine data. Combat and inventory business rules belong in services. Harmony patches and rule listeners remain thin adapters. Diagnostics may observe engine state, but gameplay mutations live in the rules/engine boundary rather than the trace subsystem.

The project has no runtime dependency on Call of the Wild, Cowboys and Demons, BlueprintCore, or a custom asset package.

## 3. Bootstrap and blueprint initialization

The selected blueprint hook is a Harmony postfix on:

```text
Kingmaker.Blueprints.LibraryScriptableObject.LoadDictionary()
```

The current sequence is:

1. UMM invokes `KingmakerGunslinger.Main.Load`.
2. A process-lifetime loader guard accepts only the first attempt.
3. `ModContext` publishes the UMM entry, logger, assembly, and one Harmony instance.
4. Harmony patches the executing assembly exactly once.
5. Optional combat, discharge, and AC patch classes resolve the exact installed `OnTrigger(RulebookEventContext)` targets. Missing or ambiguous targets skip the affected patch and are logged.
6. The `LoadDictionary` postfix passes the first library instance to `BlueprintBootstrap.Observe`.
7. Initialization waits for both the library and a patch-ready context.
8. The deployed stable-ID manifest is loaded and validated.
9. An in-memory `FirearmDefinitionComponent` round-trip proves the marker can be constructed.
10. One `BlueprintRegistry` transaction registers the diagnostic feature, Firearm Proficiency, Test Musket type/item, four component-only firearm-state token enchantments, Black Powder Charge, Lead Ball, Firearm Repair Kit, Reload Test Musket, Overhaul Test Musket, and Repair Test Musket.
11. The Test Musket type is cloned from the native Heavy Crossbow type and receives exactly one firearm marker.
12. The Test Musket item is cloned from the native Standard Heavy Crossbow, rewired to the custom type, and receives exactly one Firearm Proficiency restriction.
13. Each firearm-state token blueprint contains exactly one passive marker component and no gameplay components.
14. Clone, marker, restriction, type-link, token, GUID, and native-source invariants are verified.
15. The token-backed runtime repository is configured only after all fourteen active registrations validate. Historical vault and direct-reference experiments are not configured for new runtime state writes.
16. Any failure triggers best-effort reverse rollback of every owned registration.
17. After successful bootstrap, the mod attaches manual development controls and an off-by-default trace toggle to the UMM panel.

### State machines

```text
Loader:     NotStarted -> Loading -> Loaded
                              \----> Failed

Context:    Created -> InstallingPatches -> PatchesInstalled
                       \-----------------> Failed

Blueprints: WaitingForLibrary -> WaitingForContext -> Initializing -> Initialized
                                                        \----------> Failed
```

A failed bootstrap is not retried in the same process. The lifecycle postfix and combat observers are exception-contained and do not deliberately throw into Kingmaker's native methods.

## 4. Stable blueprint identifiers

`blueprints/blueprints.json` is the sole authority for custom blueprint IDs.

Rules:

- lowercase 32-character hexadecimal GUIDs only;
- no runtime GUID generation;
- no silent symbol rename or GUID reassignment;
- retired identifiers remain reserved;
- exact planned type must match registration type;
- collisions are rejected before a Unity object factory runs;
- dictionary insertion uses `Add`, never a replacing indexer;
- save migrations will refer to symbolic names and schema versions.

The manifest contains 12 stable identifiers: eleven active and one reserved. The active ledger includes the diagnostic feature, Firearm Proficiency, Test Musket type/item, four firearm-state tokens, Reload Test Musket, Black Powder Charge, and Lead Ball. The touch-AC enchantment remains reserved because touch AC is implemented through a rule patch.

## 5. Firearm identity and category adaptation

Kingmaker's `WeaponCategory` is a compiled enum. The mod does not invent a runtime enum member and does not globally redefine crossbows as firearms.

The authoritative identity is an immutable marker:

```text
FirearmDefinition
  era
  firearm kind
  capacity
  range increment
  base misfire value
  reload profile
  scatter flag
```

A borrowed vanilla category supplies only low-level engine and animation compatibility. Every firearm-specific rule and diagnostic must ask for `FirearmDefinition`, not merely compare `WeaponCategory`.

The Test Musket currently borrows the native Heavy Crossbow category because it is cloned from that weapon type. Category-specific proficiency leakage remains a known integration problem, not the definition of a firearm.

## 6. Firearm proficiency

`KMG.Firearms.FirearmProficiency` is a one-rank, hidden, component-free `BlueprintFeature`. It is the shared fact that later class progression, feats, archetypes, and items will reference.

The Test Musket item receives one `FirearmProficiencyRestriction : EquipmentRestriction`:

```text
CanBeEquippedBy(unit)
  -> unit exists
  -> required feature exists
  -> unit.GetFeature(required feature) != null
```

This is strict equip denial rather than an attack-roll penalty. The restriction is attached only to the custom Test Musket clone. Native Heavy Crossbow blueprints are snapshotted and verified unchanged.

Passing this item-level gate does not prove that Kingmaker will ignore inherited Heavy Crossbow proficiency during attack resolution. Initial positive-path runtime testing should therefore use a character already proficient with martial weapons.

## 7. Development controls

The UMM panel exposes explicit disposable-save controls to grant Firearm Proficiency, add or remove Test Muskets, inspect equipped firearm state, mutate the first equipped firearm among Empty/Loaded/Broken/Wrecked diagnostic states, add/count/consume ammunition, inspect reload readiness, and exercise the full-round reload ability.

The panel also shows process-local diagnostics for reload delivery, attack enforcement, range-limited touch AC, optional combat tracing, and native state-token reconciliation. The trace toggle defaults to off and is not campaign state.

The controls never run automatically, require successful blueprint initialization, fail closed outside an active campaign, and catch exceptions at the command boundary. `KingmakerDevelopmentBridge` and `ReflectionAccess` isolate runtime API uncertainty from the UI; they are development infrastructure, not persistence keys or gameplay identity.

## 8. Blueprint transaction

```text
BlueprintRegistry transaction
  -> DiagnosticBlueprints.Register
  -> FirearmProficiencyBlueprints.Register
  -> clone/register Test Musket weapon type and item
  -> append exactly one FirearmDefinitionComponent
  -> append exactly one FirearmProficiencyRestriction
  -> register four inert firearm-state token enchantments
  -> register Black Powder Charge and Lead Ball
  -> register Reload Test Musket
  -> validate eleven active registrations and native-source immutability
  -> configure the token-backed runtime state repository
  -> commit, or RollbackAll in reverse order
```

Collision checks happen before factories run and again immediately before dictionary insertion. `BlueprintsByAssetId.Add` prevents accidental replacement. Stable GUIDs are never regenerated or repurposed.

## 9. Combat tracing and firearm AC integration

Three optional Harmony patch classes observe the exact declared callbacks whose installed signature is `void OnTrigger(Kingmaker.RuleSystem.RulebookEventContext)` on:

```text
Kingmaker.RuleSystem.Rules.RuleAttackWithWeapon
Kingmaker.RuleSystem.Rules.RuleAttackRoll
Kingmaker.RuleSystem.Rules.RuleCalculateAC
```

Target resolution requires exactly one non-static, non-generic, `void OnTrigger(RulebookEventContext)` method declared on each intended rule-event type. Each class may omit the native context argument from its Harmony prefix/postfix because it needs only `__instance`; target resolution still requires the exact one-argument installed method. The patches accept no `ref`, `out`, or `__result` parameters and return no Boolean control value. Trace code is read-only; the AC postfix delegates one authorized `TargetAC` write to `FirearmArmorClassRuntime`.

### Eligibility

A root trace begins only when the concrete weapon item's exact `BlueprintWeaponType` contains exactly one `FirearmDefinitionComponent`. `WeaponCategory.HeavyCrossbow` and native Heavy Crossbow identity are not consulted.

### Correlation

A thread-local callback stack provides parent event identities. `CombatTraceCorrelator` joins nested callbacks with integer runtime identities, numbers repeated stage/phase callbacks, and completes at the root event's postfix. A standalone firearm `RuleAttackRoll` may also serve as a root.

The correlator retains only:

- integer trace and event identities;
- enums;
- primitive values;
- immutable strings;
- read-only copies of field dictionaries.

It retains no rule event, unit, item, blueprint, or Unity object after the callback returns. A diagnostic exception increments a fault counter and clears current-thread trace state. The AC adapter separately catches failures and retains ordinary AC wherever possible.

### Evidence fields

The snapshot reader attempts to copy the weapon/item/type identities, marker definition, initiator, target, natural roll candidates, attack arithmetic, outcome candidates, target/ordinary/touch AC, distance, calculated range increment, source, and full-attack shape. Missing runtime members are emitted as `<unavailable>` rather than guessed.

The meter-to-foot conversion and candidate member names remain runtime gates. See [COMBAT-TRACE-SCHEMA.md](COMBAT-TRACE-SCHEMA.md), [ADR-0015](decisions/ADR-0015-read-only-firearm-combat-tracing.md), and [ADR-0016](decisions/ADR-0016-range-limited-touch-ac-delta.md).

### Range-limited touch AC

For an exact early firearm, the AC adapter applies `current TargetAC + (touch AC - ordinary AC)` only in range increment one. This preserves contextual event changes such as cover and flat-footed state. A 0.1-millimeter tolerance absorbs floating-point noise at the exact range boundary. Native Heavy Crossbows and ambiguous markers are ignored. A weak event stamp prevents duplicate adjustment.

## 10. Per-item firearm state

The immutable state contract is:

```text
FirearmState
  schemaVersion: 1
  loadedRounds
  loadedAmmunition: AmmunitionId or null
  condition: Normal, Broken, Wrecked
```

The supported runtime path is:

```text
exact ItemEntityWeapon
  -> exact BlueprintItemWeapon / BlueprintWeaponType
  -> exactly one FirearmDefinitionComponent
  -> FirearmItemStateService
  -> TokenBackedFirearmStateRepository
  -> KingmakerFirearmStateTokenStore
  -> zero or one known inert BlueprintWeaponEnchantment token on that item
```

Absence of a known token is canonical Empty/Normal. Exactly one known token encodes Loaded/Normal, Broken/Empty, Broken/Loaded, or Wrecked. Duplicate, foreign, or ambiguous known-token sets fail closed rather than being guessed or normalized. State writes verify the exact resulting token set.

New tokens receive a `MechanicsContext` when a current wielder or owner is available. A prefix/postfix around the exact zero-argument `ItemEntity.ApplyEnchantments()` method captures and verifies known tokens only for `ItemEntityWeapon`. If native reconciliation changes one unambiguous known token to zero, the exact token is restored and verified. Non-weapon items bypass firearm inspection.

The earlier weak process-local repository, direct-reference UnitPart, and engine-identity vault remain in source for history and pure tests. The installed Kingmaker runtime disproved the assumed `ItemEntityWeapon.UniqueId` contract; those carriers are not configured as the runtime source of truth and must not be revived.

## 11. Combat-rule integration

Firearms remain real `BlueprintItemWeapon`/`ItemEntityWeapon` instances and use the normal weapon attack pipeline.

| Event boundary | Current or next responsibility |
|---|---|
| `RuleCalculateAC.OnTrigger(RulebookEventContext)` | Current: touch AC only within firearm penetration range |
| `RuleAttackRoll.OnTrigger(RulebookEventContext)` | Consume one loaded round or force Empty/Wrecked miss on the exact marked firearm |
| Weapon attack/damage | Preserve native modifiers, criticals, concealment, cover, damage, and other mod composition |
| `RuleAttackRoll.set_Roll(RollEntry)` / `IsSuccessRoll(int)` | Observe or deterministically force the exact eligible firearm natural d20, force configured misfires to miss, and apply one exact-item condition transition |
| Reload ability delivery | Atomically consume one powder plus one Lead Ball and load the exact empty Normal or Broken firearm without changing condition; reject Wrecked |
| Overhaul ability delivery | Atomically consume one Firearm Repair Kit and change the exact empty/Wrecked firearm to empty/Broken; preserve item identity; reject Normal/Broken/ambiguous targets |
| Repair ability delivery | Atomically consume one Firearm Repair Kit and change the exact empty/Broken firearm to empty/Normal; preserve item identity; reject Normal/Wrecked/loaded/ambiguous targets |
| Maintenance qualification | Observe exact target, second-item isolation, revisions, resources, completions, faults, and duplicates; never mutate gameplay or persistence state |
| Later class systems | Gun Training, deeds, grit, and class progression |

Loaded Normal and Loaded Broken attacks consume one round at the start of the exact firearm attack roll. Empty, Wrecked, or state-faulted marked firearms are forced to miss. A weak reference-identity gate prevents duplicate callbacks from consuming twice. Firing never consumes shared-inventory ammunition again.

Natural-roll misfire detection, force-next-roll diagnostics, exact-item Normal → Broken → Wrecked transitions, native definition-sized second-misfire burst delivery, player-facing Wrecked → Broken Overhaul, separate Broken → Normal Repair, and the complete Overhaul → Repair → Reload loop are active in 0.0.29. Definition-driven generic actions, scatter triple damage, Quick Clear, and automatic iterative reloads remain outside this version.

## 12. Persistence evidence boundary

Sprint 15 adds a fixed 35-row matrix catalog, a pure gate evaluator, and an external recorder under the installed mod's `evidence/` directory. Sessions are tied to the exact mod DLL, blueprint manifest, game assembly, UMM assembly, Harmony assembly, and game version. Each observation records structured before/after firearm snapshots plus optional notes and save hashes.

The evidence directory is never read by `FirearmRuntimeState`, any repository, migration layer, attack rule, or inventory service. It cannot restore state and is not a sidecar save. See [PERSISTENCE-EVIDENCE-RECORDER.md](PERSISTENCE-EVIDENCE-RECORDER.md) and [ADR-0022](decisions/ADR-0022-external-persistence-evidence-recorder.md).

## 13. Ammunition transaction boundary

A loaded round and an inventory item are different states. Black Powder Charge and Lead Ball are real stackable inventory items. The full-round `Reload Test Musket` ability resolves one exact empty Normal or Broken firearm and, only when delivery completes, uses a verified cross-resource transaction to consume exactly one of each component and write one loaded round to that item. Reload preserves Normal or Broken condition; Wrecked is rejected before mutation.

Rejected or interrupted reloads consume nothing. Missing either component consumes neither. Best-effort rollback restores both sides after a partial or unverifiable mutation. Firing consumes only the already-loaded item state; it does not remove another powder charge or Lead Ball from shared inventory.

## 14. Save and migration safety

Every future serialized payload has a schema version. Requirements include stable blueprint IDs, monotonic migrations, no reinterpretation of retired IDs, detection of orphaned ammunition definitions, conservative repair of impossible state, and explicit upgrade/uninstall warnings.

The manual controls can embed custom blueprint references in saves. Disposable saves are mandatory until compatibility and migration behavior are proven.

## 15. Asset boundary

Core mechanics have no dependency on custom models or animation controllers. The first visual fallback remains a verified crossbow-derived presentation, firearm icon, fast or hidden projectile, sound, and minimal muzzle flash. Custom art belongs in an optional, separately licensed asset package.

## 16. Custom blueprint roles

| Symbol | Sprint 22 repair status | Role |
|---|---|---|
| `KMG.Diagnostic.InitializedFeature` | Active | Hidden initialization proof |
| `KMG.Firearms.FirearmProficiency` | Active | Shared firearm permission fact |
| `KMG.Test.TestMusketWeaponType` | Active | Heavy-Crossbow-derived exact firearm type |
| `KMG.Test.TestMusketItem` | Active | Restricted Test Musket item |
| `KMG.Test.LoadedStateToken` | Active | Loaded/Normal item state |
| `KMG.Test.BrokenEmptyStateToken` | Active | Empty/Broken item state |
| `KMG.Test.BrokenLoadedStateToken` | Active | Loaded/Broken item state |
| `KMG.Test.WreckedStateToken` | Active | Wrecked item state |
| `KMG.Test.ReloadAbility` | Active | Full-round Test Musket reload ability |
| `KMG.Test.BlackPowderItem` | Active | Stackable powder component |
| `KMG.Test.LeadBulletItem` | Active | Stackable Lead Ball component; stable symbol retained |
| `KMG.Test.FirearmRepairKitItem` | Active | Stackable recovery resource consumed by Overhaul and Repair |
| `KMG.Test.OverhaulAbility` | Active | Full-round exact-item Wrecked-to-Broken ability |
| `KMG.Test.RepairAbility` | Active | Full-round exact-item Broken-to-Normal ability |
| `KMG.Test.TouchAcEnchantment` | Reserved | Unused because touch AC is implemented by rule patch |

Stable symbols and GUIDs are never regenerated or repurposed.

## 17. Dependency and packaging policy

Build-time references come from the developer's exact private Kingmaker 2.1.7b / UMM 0.32.4 bundle and remain compiler input only. Build-output and package validators reject copied game, Unity, Unity Mod Manager, Harmony, Newtonsoft, or other private-reference binaries.

A valid standalone UMM package contains one root and exactly one binary:

```text
KingmakerGunslinger/
  KingmakerGunslinger.dll
  Info.json
  CHANGELOG.md
  LICENSE
  README.md
  SMOKE-TEST-GUIDE.md
  blueprints/
    blueprints.json
    blueprints.schema.json
```

The complete milestone ZIP embeds the validated standalone install ZIP and source ZIP with checksums and evidence. The private reference archive is never included.

## 18. Current non-goals

Version 0.0.29 does not:

- implement generic definition-driven maintenance, Quick Clear, automatic iterative reloads, or Rapid Reload;
- change the accepted item-owned token carrier or revive `ItemEntityWeapon.UniqueId` persistence;
- identify firearms by Heavy Crossbow category, name, slot, inventory position, owner, runtime hash, or value equality;
- add production pistols, scatter weapons, custom firearm assets, vendors, crafting, the Gunslinger class, deeds, grit, or enemy firearm AI; or
- claim 0.0.29 runtime acceptance before the complete maintenance-loop smoke test passes in Kingmaker.

## Historical subsystem notes

Earlier sprint-specific architecture and persistence experiments remain in dedicated documents and ADRs. Where they conflict with this file's Sprint 29 layer, the current token-backed runtime path and exact installed method contracts above are authoritative.

## Historical Sprint 28 authoritative Overhaul layer

Sprint 28 added the first player-facing same-item recovery transaction on top of the accepted item-token state carrier. This historical section documents the Overhaul boundary that Sprint 29 retains.

```text
exactly one equipped empty/Wrecked exact Test Musket
        + one Firearm Repair Kit in shared inventory
        + completed full-round Overhaul Test Musket delivery
        ↓
same runtime item / same process-local repository identity
        ↓
empty/Broken, revision +1, kit count -1
```

The architecture has four layers:

1. `OverhaulTestMusketAbilityLogic` owns Kingmaker availability and delivery timing.
2. `OverhaulTestMusketRuntime` resolves exactly one equipped Test Musket and the shared inventory.
3. `FirearmOverhaulTransactionService` coordinates the exact item state and repair-kit count with independent rollback verification.
4. `FirearmOverhaulRuntimeResult` proves same-item identity and exactly one state revision increment.

Availability checks are read-only. Mutation occurs only during ability delivery. No powder or Lead Ball participates. The operation stops at Broken and does not remove or replace the item. Ordinary Broken-to-Normal repair remains a distinct future action.

At Sprint 28 the blueprint ledger contained 14 stable IDs, 13 active, and Firearm Proficiency granted Reload and Overhaul. Sprint 29 extends that historical layer rather than replacing its exact-item contract.

The `ItemEntityWeapon.UniqueId` vault remains rejected and absent. Item-owned inert `BlueprintWeaponEnchantment` tokens remain the authoritative persisted state.


## Historical player-facing same-item Overhaul

The Sprint 28 recovery adapter keeps the pure state machine, exact-item repository, inventory port, and Kingmaker ability delivery separate:

```text
BlueprintAbility + AbilityCustomLogic
        ↓ delivery only
OverhaulTestMusketRuntime
        ↓ exact equipped item + shared inventory adapters
FirearmOverhaulTransactionService
        ↓ verified writes / best-effort rollback
item-owned state token + Firearm Repair Kit stack
```

Readiness queries are read-only. The full-round command performs no mutation until `Deliver`. The transaction rejects before writes unless the exact item is Wrecked and at least one kit exists, consumes exactly one kit, writes empty/Broken once, verifies both resources, and restores the pre-operation values after a mutation-time failure when possible. Runtime evidence additionally requires unchanged in-process repository identity and reference hash plus exactly one revision increment.

## Sprint 29 authoritative maintenance layer

Sprint 29 composes three separately qualified transactions without collapsing their state or resource boundaries:

```text
exact empty/Wrecked firearm + Repair Kit
        -- completed Overhaul delivery -->
exact empty/Broken firearm + Repair Kit
        -- completed Repair delivery -->
exact empty/Normal firearm + powder + Lead Ball
        -- completed Reload delivery -->
exact loaded/Normal firearm
```

The ordinary Repair path mirrors the accepted Overhaul layering:

```text
BlueprintAbility + AbilityCustomLogic
        ↓ delivery only
RepairTestMusketRuntime
        ↓ exact equipped item + shared inventory adapters
FirearmRepairTransactionService
        ↓ verified writes / independent best-effort rollback
item-owned state token + Firearm Repair Kit stack
```

Availability remains read-only. Repair starts no transaction before `Deliver`, accepts only one exact equipped empty/Broken Test Musket, consumes one kit, writes empty/Normal once, verifies both resources, and restores the pre-operation values after a mutation-time failure when possible. `FirearmRepairRuntimeResult` requires unchanged process-local item identity and one revision increment.

The qualification harness remains outside gameplay. `MaintenanceQualificationBaseline` captures one target, one independent second item, resources, completion counters, fault totals, and duplicate totals. `MaintenanceQualificationService` compares later observations and emits one of four checkpoints: `FixtureReady`, `OverhaulPassed`, `RepairPassed`, or `MaintenanceLoopPassed`. The one-command runner uses immediate runtime adapters only for fast transaction regression; actual action-bar delivery and interruption remain live-test obligations.

The blueprint ledger contains 30 stable IDs: 29 active and one reserved. Firearm Proficiency grants Reload, Overhaul, and Repair. The Gunslinger level-one progression grants a persistent per-unit grit resource whose maximum is its one-point base floor plus the exact Wisdom amount above that floor. The standalone package continues to contain exactly one project-owned binary and no private reference assembly.
