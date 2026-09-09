using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static partial class GunslingerOutfitRenderScenario
    {
        internal sealed partial class ElementalRacePersistenceSession
        {
            private readonly JArray _physicalLifecycleRecords = new JArray();
            private ElementalPersistenceObservation _physicalSourceObservation;
            private UnitEntityData _physicalActor;
            private BlueprintScriptableObject[] _physicalOwned;
            private JObject _physicalStableBefore, _physicalReturnedStats;
            private ItemSlot[] _physicalSlots;
            private ItemEntity[] _physicalItems;
            private object[] _physicalInventory;
            private ItemEntity _physicalOriginalArmor;
            private ItemEntityArmor _physicalArmor;
            private Buff _physicalPolymorph;
            private Buff[] _physicalBuffsBefore;
            private TimeSpan _physicalTime;
            private UnityEngine.Random.State _physicalRandom;
            private int _physicalStep, _physicalUpdates, _physicalImmortality;
            private bool _physicalComplete;

            // These are the same source checks historically performed immediately
            // before fixed-shell respec. Capture them once BEFORE native death can
            // legitimately remove transient spell buffs; never recreate those buffs.
            private ElementalPersistenceObservation CaptureRestoredPhysicalSource(ElementalPersistenceFixture fixture)
            {
                if (_physicalSourceObservation != null) return _physicalSourceObservation;
                var observation = ObserveFixture(fixture, _currentUnit, _currentExpectedDoll,
                    fixture.Heritage, 0, 2, expectedSizeCasterLevel: 2);
                if (!observation.Exact) throw new InvalidOperationException("Restored source before lifecycle is not exact: " + observation.Evidence);
                RecordTraitPersistence(fixture, _currentUnit, 2, 1, false, "module-restored-source-before-respec");
                CaptureRestoredSourceFeatPersistence(fixture, _currentUnit);
                _physicalSourceObservation = observation;
                return observation;
            }

            private bool PollVisibleTraitPhysicalLifecycle()
            {
                var fixture = _fixtures[_fixtureIndex];
                if (!_moduleRestored || !IsFixtureUnit(_currentUnit, fixture) || !Game.Instance.IsPaused)
                    throw new InvalidOperationException("Physical lifecycle requires the exact paused restored disposable fixture.");
                _stage = "visible-trait-physical-" + fixture.Label + "-" + _physicalStep;
                if (_physicalActor == null)
                {
                    CaptureRestoredPhysicalSource(fixture);
                    _physicalActor = _currentUnit;
                    if (_physicalActor.Blueprint.IsCheater || _physicalActor.Descriptor.State.IsDead || _physicalActor.Body.IsPolymorphed)
                        throw new InvalidOperationException("Physical fixture must be alive, ordinary and non-cheater; no shared blueprint mutation is permitted.");
                    _physicalOwned = PhysicalOwnedBlueprints();
                    _physicalStableBefore = CapturePhysicalStable();
                    _physicalTime = Game.Instance.TimeController.GameTime;
                    _physicalRandom = UnityEngine.Random.state;
                    _physicalSlots = _physicalActor.Body.AllSlots.ToArray();
                    _physicalItems = _physicalSlots.Select(value => value.MaybeItem).ToArray();
                    _physicalInventory = Snapshot(_inventory);
                    _physicalOriginalArmor = _physicalActor.Body.Armor.MaybeItem;
                    _physicalBuffsBefore = _physicalActor.Buffs.Enumerable.ToArray();
                    _physicalImmortality = _physicalActor.Descriptor.State.Immortality.Count;
                    // The paused harness owns the ordinary buff phase as well as
                    // the life controller. Tick alive first so native death-edge
                    // tracking is established before applying lethal damage.
                    if (_nereidPersistence) _physicalActor.Buffs.Tick();
                    RecordPhysicalLifecycle("before-death", false, false, false);
                    _physicalActor.Descriptor.State.Immortality.ReleaseAll();
                    int lethal = _physicalActor.MaxHP + Math.Max(1, _physicalActor.Stats.Constitution.ModifiedValue) + 10;
                    var damage = Rulebook.Trigger(new RuleDealDamage(_physicalActor, _physicalActor,
                        new DamageBundle(new DirectDamage(new DiceFormula(0, DiceType.D6), lethal)))
                        { DisablePrecisionDamage = true, IgnoreDamageReduction = true });
                    if (damage.Damage < lethal || _physicalActor.HPLeft > -_physicalActor.Stats.Constitution.ModifiedValue)
                        throw new InvalidOperationException("Native lethal damage did not reach the ordinary death threshold.");
                    TickPhysicalLife();
                    _physicalStep = 1; _physicalUpdates = 0;
                    WriteProgress("native-visible-trait-lethal-damage");
                    return false;
                }
                if (!ReferenceEquals(_physicalActor, _currentUnit)) throw new InvalidOperationException("Physical actor identity changed.");
                if (Game.Instance.TimeController.GameTime != _physicalTime)
                    throw new InvalidOperationException("Paused physical lifecycle advanced the campaign clock.");
                if (++_physicalUpdates > MaximumSettleUpdates)
                    throw new TimeoutException("Native physical transition did not settle: " + _stage + ";" + PhysicalWaitDiagnostic());
                Game.Instance.EntityCreator.Tick();
                if (_physicalActor.View != null && _physicalActor.View.AnimationManager != null)
                    _physicalActor.View.AnimationManager.Tick();
                if (_physicalStep == 1)
                {
                    TickPhysicalLife();
                    if (!_physicalActor.Descriptor.State.IsDead || _physicalUpdates < MinimumSettleUpdates) return false;
                    RecordPhysicalLifecycle("native-dead", true, false, false);
                    _physicalActor.Descriptor.ResurrectAndFullRestore();
                    _diagnostics.Add("physical-resurrection-requested=" + fixture.Label + ";" + PhysicalWaitDiagnostic());
                    _physicalStep = 2; _physicalUpdates = 0;
                    return false;
                }
                if (_physicalStep == 2)
                {
                    // Resurrect clears wounds and death conditions. The native
                    // life controller still owns the subsequent LifeState change;
                    // the ordinary paused world does not tick it for this actor.
                    TickPhysicalLife();
                    if (_physicalActor.Descriptor.State.IsDead || _physicalActor.HPLeft != _physicalActor.MaxHP ||
                        !PhysicalViewReady(false) || _physicalUpdates < MinimumSettleUpdates) return false;
                    RestorePhysicalImmortality();
                    RecordPhysicalLifecycle("native-resurrected", false, false, true);
                    _physicalReturnedStats = CapturePhysicalStats();
                    var spell = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library,
                        "5d4028eb28a106d4691ed1b92bbb1915", "visible-trait-physical-beast-shape-ii");
                    var buff = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(BlueprintBootstrap.Library,
                        "8dc6510d31614345a8c718208fbac1f8", "visible-trait-physical-beast-shape-ii-buff");
                    var context = new MechanicsContext(_physicalActor, _physicalActor.Descriptor, spell, null, new TargetWrapper(_physicalActor));
                    context.Params.CasterLevel = 20;
                    _physicalPolymorph = _physicalActor.Buffs.AddBuff(buff, context, TimeSpan.FromMinutes(20));
                    if (_physicalPolymorph == null) throw new InvalidOperationException("Native Beast Shape II was rejected.");
                    _physicalStep = 3; _physicalUpdates = 0;
                    return false;
                }
                if (_physicalStep == 3)
                {
                    if (!PhysicalViewReady(true) || _physicalUpdates < MinimumSettleUpdates) return false;
                    RecordPhysicalLifecycle("native-polymorphed", false, true, false);
                    _physicalPolymorph.Remove();
                    _physicalStep = 4; _physicalUpdates = 0;
                    return false;
                }
                if (_physicalStep == 4)
                {
                    if (!PhysicalViewReady(false) || _physicalUpdates < MinimumSettleUpdates) return false;
                    RecordPhysicalLifecycle("native-polymorph-return", false, false, true);
                    _physicalActor.Body.Armor.RemoveItem(false);
                    var armor = BlueprintLibraryLookup.RequireExact<BlueprintItemArmor>(BlueprintBootstrap.Library,
                        "559b0b6f194656c428c403a000ceee78", "visible-trait-physical-native-armor");
                    _physicalArmor = new ItemEntityArmor(armor);
                    _physicalActor.Body.Armor.InsertItem(_physicalArmor);
                    _physicalActor.View.UpdateClassEquipment(); CurrentAvatar().RebuildOutfit();
                    _physicalStep = 5; _physicalUpdates = 0;
                    return false;
                }
                if (_physicalStep == 5)
                {
                    if (!PhysicalViewReady(false) || _physicalUpdates < MinimumSettleUpdates) return false;
                    if (!ReferenceEquals(_physicalActor.Body.Armor.MaybeItem, _physicalArmor))
                        throw new InvalidOperationException("The owned native armor did not occupy its exact slot.");
                    RecordPhysicalLifecycle("native-armor-equipped", false, false, false);
                    RemovePhysicalArmor();
                    _physicalActor.View.UpdateClassEquipment(); CurrentAvatar().RebuildOutfit();
                    _physicalStep = 6; _physicalUpdates = 0;
                    return false;
                }
                if (_physicalStep == 6)
                {
                    if (!PhysicalViewReady(false) || _physicalUpdates < MinimumSettleUpdates) return false;
                    RecordPhysicalLifecycle("native-equipment-and-doll-restored", false, false, true);
                    if (!_physicalSlots.SequenceEqual(_physicalActor.Body.AllSlots) ||
                        !_physicalItems.SequenceEqual(_physicalSlots.Select(value => value.MaybeItem)) ||
                        !_physicalInventory.SequenceEqual(Snapshot(_inventory)))
                        throw new InvalidOperationException("Physical lifecycle changed an original slot, item or inventory reference.");
                    _physicalComplete = true;
                    UnityEngine.Random.state = _physicalRandom;
                    WriteProgress("native-visible-trait-physical-complete");
                    return true;
                }
                throw new InvalidOperationException("Unknown visible-trait physical step.");
            }

            private BlueprintScriptableObject[] PhysicalOwnedBlueprints()
            {
                var owned = new List<BlueprintScriptableObject>();
                foreach (var race in _blueprintSet.OrderedBlueprints())
                {
                    owned.Add(race.Race); owned.Add(race.Resistance); owned.Add(race.Heritages.Selection);
                    foreach (var heritage in race.Heritages.Choices())
                    {
                        owned.Add(heritage.Marker); owned.Add(heritage.Affinity); owned.Add(heritage.SlaFeature);
                        owned.Add(heritage.SlaAbility); owned.Add(heritage.SlaResource); owned.AddRange(heritage.AuxiliaryBlueprints);
                    }
                    AddAlternateTraitIdentities(race, owned);
                }
                return owned.Distinct().OrderBy(value => value.AssetGuid, StringComparer.Ordinal).ToArray();
            }

            private JObject CapturePhysicalStable()
            {
                var owner = _physicalActor.Descriptor;
                var blood = owner.Get<UnitPartElementalBloodCapacity>();
                return new JObject { ["raceGuid"] = owner.Progression.Race.AssetGuid,
                    ["features"] = new JArray(_physicalOwned.OfType<BlueprintFeature>().Select(value =>
                        new JObject { ["guid"] = value.AssetGuid, ["rank"] = owner.Progression.Features.GetRank(value) })),
                    ["abilities"] = new JArray(_physicalOwned.OfType<BlueprintAbility>().Where(value =>
                        !_nereidPersistence || value.AssetGuid != ElementalNereidFactory.ShakeFreeGuid).Select(value =>
                        new JObject { ["guid"] = value.AssetGuid, ["count"] = owner.Abilities.Enumerable.Count(fact => ReferenceEquals(fact.Blueprint, value)) })),
                    ["resources"] = new JArray(_physicalOwned.OfType<BlueprintAbilityResource>().Select(value =>
                        new JObject { ["guid"] = value.AssetGuid, ["count"] = owner.Resources.PersistantResources.Count(resource => ReferenceEquals(resource.Blueprint, value)),
                            ["amount"] = owner.Resources.GetResourceAmount(value) })),
                    ["blood"] = new JArray(BloodPersistenceTraits.Select(value => new JObject {
                        ["trait"] = value.ToString(), ["spent"] = blood == null ? 0 : blood.Spent(value) })) };
            }

            private JObject CapturePhysicalStats()
            {
                var result = new JObject();
                foreach (var type in new[] { StatType.Strength, StatType.Dexterity, StatType.Constitution,
                    StatType.Intelligence, StatType.Wisdom, StatType.Charisma, StatType.AC, StatType.Speed,
                    StatType.Initiative, StatType.SkillStealth, StatType.SkillPerception })
                {
                    var stat = _physicalActor.Stats.GetStat(type);
                    result[type.ToString()] = new JObject { ["base"] = stat.BaseValue, ["modified"] = stat.ModifiedValue,
                        ["ownedModifiers"] = new JArray(stat.Modifiers.Where(value => value.Source != null &&
                            _physicalOwned.Contains(value.Source.Blueprint)).Select(value => new JObject {
                                ["source"] = value.Source.Blueprint.AssetGuid, ["descriptor"] = value.ModDescriptor.ToString(),
                                ["value"] = value.ModValue }).OrderBy(value => value.ToString(Formatting.None), StringComparer.Ordinal)) };
                }
                return result;
            }

            private bool PhysicalViewReady(bool polymorphed)
            {
                return _physicalActor.View != null && ReferenceEquals(_physicalActor.View.Data, _physicalActor) &&
                    _physicalActor.Body.IsPolymorphed == polymorphed && ActiveRenderers(_physicalActor).Length > 0 &&
                    (polymorphed || (CurrentAvatarOrNull() != null && HasExactHumanoidRig(_physicalActor.View.transform)));
            }

            private void RecordPhysicalLifecycle(string phase, bool dead, bool polymorphed, bool returned)
            {
                var fixture = _fixtures[_fixtureIndex];
                var stable = CapturePhysicalStable();
                var stats = CapturePhysicalStats();
                bool stableExact = JToken.DeepEquals(stable, _physicalStableBefore);
                bool statsExact = !returned || _physicalReturnedStats == null || JToken.DeepEquals(stats, _physicalReturnedStats);
                bool graphExact = AlternateTraitsExact(_physicalActor.Descriptor, fixture, ExpectedPersistenceTraits(fixture, fixture.Heritage)) &&
                    HeritageProvidersExact(_physicalActor.Descriptor, fixture, fixture.Heritage, 1);
                bool dollExact = _currentExpectedDoll.Matches(_physicalActor.Descriptor.Doll);
                bool appearanceExact = true;
                JObject visible = null, appearanceEvidence = null;
                if (returned)
                {
                    int sizeCasterLevel = EfreetiPersistenceBuffs(fixture, _physicalActor).Length == 0 ? 0 : 2;
                    var appearance = ObserveFixture(fixture, _physicalActor, _currentExpectedDoll, fixture.Heritage, 0, 2,
                        expectedSizeCasterLevel: sizeCasterLevel);
                    appearanceExact = appearance.Exact;
                    appearanceEvidence = appearance.Evidence;
                    visible = RecordVisibleStatPersistence(fixture, _physicalActor,
                        ExpectedPersistenceTraits(fixture, fixture.Heritage), "physical-" + phase);
                }
                bool exact = stableExact && statsExact && graphExact && dollExact && appearanceExact &&
                    _physicalActor.Descriptor.State.IsDead == dead && _physicalActor.Body.IsPolymorphed == polymorphed;
                JObject nereidTransient = _nereidPersistence ? RecordNereidPhysicalTransientState(phase) : null;
                var row = new JObject { ["fixture"] = fixture.Label, ["phase"] = phase, ["step"] = _physicalStep,
                    ["updates"] = _physicalUpdates, ["traits"] = new JArray(ExpectedPersistenceTraits(fixture, fixture.Heritage).Select(value => value.Definition.Id.ToString())),
                    ["nativeDead"] = _physicalActor.Descriptor.State.IsDead, ["nativePolymorphed"] = _physicalActor.Body.IsPolymorphed,
                    ["wounds"] = _physicalActor.Damage, ["maxHP"] = _physicalActor.MaxHP, ["immortalityCount"] = _physicalActor.Descriptor.State.Immortality.Count,
                    ["stableExact"] = stableExact, ["statsExact"] = statsExact, ["graphExact"] = graphExact, ["dollExact"] = dollExact,
                    ["appearanceExact"] = appearanceExact, ["appearanceObservation"] = appearanceEvidence, ["stable"] = stable, ["stats"] = stats, ["visibleMechanics"] = visible,
                    ["nereidTransient"] = nereidTransient,
                    ["buffsRetained"] = new JArray(_physicalActor.Buffs.Enumerable.Select(value => value.Blueprint.AssetGuid)),
                    ["nativeRemovedBuffs"] = new JArray(_physicalBuffsBefore.Where(value => !_physicalActor.Buffs.Enumerable.Contains(value)).Select(value => value.Blueprint.AssetGuid)),
                    ["exact"] = exact };
                _physicalLifecycleRecords.Add(row);
                string path = Path.Combine(_request.EvidenceDirectory, "elemental-visible-trait-physical-lifecycle.json");
                WriteJsonAtomic(path, new JObject { ["schemaVersion"] = 1, ["scenario"] = _request.Scenario,
                    ["nativeLifeController"] = true, ["sharedBlueprintMutated"] = false,
                    ["observations"] = _physicalLifecycleRecords.DeepClone() });
                if (!_evidenceFiles.Contains(path)) _evidenceFiles.Add(path);
                Add(_assertions, "elemental-physical-" + fixture.Label + "-" + phase,
                    "native transition preserves exact owned providers, spent resources, blood expenditure and final racial state",
                    row.ToString(Formatting.None), exact, "real loaded actor, native death/resurrection, buff polymorph, equipment and doll callbacks; no direct KMG reconciliation");
                if (!exact) throw new InvalidOperationException("Visible-trait physical lifecycle diverged: " + row);
            }

            private bool PhysicalLifecycleEvidenceExact()
            {
                if (!_moduleRestored) return _physicalLifecycleRecords.Count == 0;
                string path = Path.Combine(_request.EvidenceDirectory, "elemental-visible-trait-physical-lifecycle.json");
                var phases = new[] { "before-death", "native-dead", "native-resurrected", "native-polymorphed",
                    "native-polymorph-return", "native-armor-equipped", "native-equipment-and-doll-restored" };
                var rows = _physicalLifecycleRecords.OfType<JObject>().ToArray();
                return rows.Length == _fixtures.Length * phases.Length && rows.All(value => value.Value<bool>("exact")) &&
                    _evidenceFiles.Count(value => string.Equals(value, path, StringComparison.OrdinalIgnoreCase)) == 1 && File.Exists(path) &&
                    _fixtures.All(fixture => rows.Where(value => value.Value<string>("fixture") == fixture.Label)
                        .Select(value => value.Value<string>("phase")).SequenceEqual(phases)) &&
                    rows.SelectMany(value => value["traits"].Values<string>()).Distinct().OrderBy(value => value, StringComparer.Ordinal)
                        .SequenceEqual(Enum.GetValues(typeof(ElementalAlternateTraitId)).Cast<ElementalAlternateTraitId>()
                            .Where(ElementalAlternateTraitPolicy.IsPublished)
                            // This explicit scope substitutes all six Undine SLA
                            // fixtures with Nereid; breath lifecycle coverage stays
                            // in the unchanged ordinary nineteen-trait matrix.
                            .Where(value => !_nereidPersistence ||
                                (value != ElementalAlternateTraitId.AcidBreath && value != ElementalAlternateTraitId.OozeBreath))
                            .Select(value => value.ToString()).OrderBy(value => value, StringComparer.Ordinal));
            }

            private string PhysicalWaitDiagnostic()
            {
                return "dead=" + _physicalActor.Descriptor.State.IsDead + ";hp=" + _physicalActor.HPLeft +
                    ";maxHP=" + _physicalActor.MaxHP + ";wounds=" + _physicalActor.Damage +
                    ";polymorphed=" + _physicalActor.Body.IsPolymorphed + ";view=" + (_physicalActor.View != null) +
                    ";renderers=" + ActiveRenderers(_physicalActor).Length + ";humanoidReady=" + PhysicalViewReady(false) +
                    ";paused=" + Game.Instance.IsPaused;
            }

            private void TickPhysicalLife()
            {
                MethodInfo tick = typeof(UnitLifeController).GetMethod("TickOnUnit", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null, new[] { typeof(UnitEntityData) }, null);
                if (tick == null) throw new MissingMethodException("Native UnitLifeController.TickOnUnit");
                tick.Invoke(new UnitLifeController(), new object[] { _physicalActor });
                // Native BuffCollection.Tick consumes StayOnDeath and removes
                // AddFacts grants. Life-state events alone do not own that phase.
                if (_nereidPersistence) _physicalActor.Buffs.Tick();
            }

            private void RestorePhysicalImmortality()
            {
                var flag = _physicalActor.Descriptor.State.Immortality;
                while (flag.Count > _physicalImmortality) flag.Release();
                while (flag.Count < _physicalImmortality) flag.Retain();
            }

            private void RemovePhysicalArmor()
            {
                if (_physicalArmor == null) return;
                if (ReferenceEquals(_physicalActor.Body.Armor.MaybeItem, _physicalArmor)) _physicalActor.Body.Armor.RemoveItem(false);
                if (_physicalOriginalArmor != null) _physicalActor.Body.Armor.InsertItem(_physicalOriginalArmor);
                if (_physicalArmor.Collection != null) _physicalArmor.Collection.Remove(_physicalArmor);
                _physicalArmor.Dispose(); _physicalArmor = null;
            }

            private void ResetPhysicalLifecycle()
            {
                // Completed sources have already restored temporary state and
                // may now be retired by fixed-shell respec. Never touch them again.
                if (_physicalActor != null && !_physicalComplete)
                {
                    if (_physicalPolymorph != null && _physicalActor.Buffs.Enumerable.Contains(_physicalPolymorph)) _physicalPolymorph.Remove();
                    RemovePhysicalArmor(); RestorePhysicalImmortality();
                    UnityEngine.Random.state = _physicalRandom;
                }
                _physicalActor = null; _physicalPolymorph = null; _physicalSourceObservation = null;
                _physicalReturnedStats = null; _physicalStableBefore = null;
                _physicalStep = _physicalUpdates = 0; _physicalComplete = false;
            }
        }
    }
}
