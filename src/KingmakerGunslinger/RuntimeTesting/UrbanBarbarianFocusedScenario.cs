using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.UrbanBarbarian;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static class UrbanBarbarianFocusedScenario
    {
        private const BindingFlags Members = BindingFlags.Instance |
            BindingFlags.Public | BindingFlags.NonPublic;
        private const string NativeRageFeatureGuid =
            "2479395977cfeeb46b482bc3385f4647";
        private const string NativeRageActivatableGuid =
            "df6a2cce8e3a9bd4592fb1968b83f730";
        private const string LongswordGuid =
            "d56c44bc9eb102c4ab3e2a7de8fcee48";

        internal static RuntimeTestResult Run(ModContext context,
            RuntimeTestRequest request)
        {
            DateTime started = DateTime.UtcNow;
            var assertions = new List<RuntimeTestAssertion>();
            var diagnostics = new List<string>();
            UrbanBarbarianBlueprintSet set = BlueprintBootstrap.UrbanBarbarian;
            object allUnits = Game.Instance.State.Units.All;
            object[] unitsBefore = Snapshot(allUnits);
            Kingmaker.EntitySystem.SceneEntitiesState scene = null;
            UnitEntityData urban = null;
            UnitEntityData enemyOne = null;
            UnitEntityData enemyTwo = null;
            UnitEntityData enemyThree = null;
            BlueprintUnit hostileSource = null;
            ItemEntityWeapon weapon = null;
            bool cleaned = false;
            string stage = "fixture";
            try
            {
                scene = new Kingmaker.EntitySystem.SceneEntitiesState(
                    "KMG_Urban_Barbarian_Focused_Fixture");
                BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
                urban = Game.Instance.EntityCreator.SpawnUnit(source,
                    Vector3.zero, Quaternion.identity, scene);
                enemyOne = ElvenBranchedSpearCombatScenario.SpawnHostileTarget(
                    urban, source, new Vector3(9f, 0f, 0f), scene,
                    out hostileSource);
                enemyTwo = Game.Instance.EntityCreator.SpawnUnit(hostileSource,
                    new Vector3(-9f, 0f, 0f), Quaternion.identity, scene);
                enemyThree = Game.Instance.EntityCreator.SpawnUnit(hostileSource,
                    new Vector3(0f, 0f, 9f), Quaternion.identity, scene);
                if (urban == null || enemyOne == null || enemyTwo == null ||
                    enemyThree == null || urban.View == null)
                    throw new InvalidOperationException(
                        "The disposable Urban live-unit fixture is incomplete.");
                enemyOne.Descriptor.State.Immortality.Retain();
                enemyTwo.Descriptor.State.Immortality.Retain();
                enemyThree.Descriptor.State.Immortality.Retain();

                stage = "level-one-archetype";
                ApplyLevel(urban.Descriptor, set.BarbarianClass, set.Archetype,
                    true);
                Add(assertions, "urban-level-one-progression",
                    "level 1 Urban owner has proficiency, Crowd Control, Controlled Rage, selector, and no Fast Movement",
                    "level=" + urban.Descriptor.Progression.GetClassLevel(
                        set.BarbarianClass) + ";facts=" +
                    string.Join(",", new BlueprintUnitFact[] { set.Proficiency, set.CrowdControl,
                        set.ControlledRage, set.Selector }.Select(value =>
                            value.name + ":" + urban.Descriptor.HasFact(value))),
                    urban.Descriptor.Progression.GetClassLevel(
                        set.BarbarianClass) == 1 &&
                    urban.Descriptor.HasFact(set.Proficiency) &&
                    urban.Descriptor.HasFact(set.CrowdControl) &&
                    urban.Descriptor.HasFact(set.ControlledRage) &&
                    urban.Descriptor.HasFact(set.Selector) &&
                    !urban.Descriptor.HasFact(BlueprintBootstrap.Library
                        .GetAllBlueprints().OfType<BlueprintFeature>().Single(
                            value => value.AssetGuid ==
                                UrbanBarbarianBlueprints.FastMovementGuid)),
                    "native LevelUpController with exact Urban archetype");

                stage = "ordinary-controlled-rage";
                urban.Descriptor.Stats.Strength.BaseValue = 10;
                urban.Descriptor.Stats.Dexterity.BaseValue = 12;
                urban.Descriptor.Stats.Constitution.BaseValue = 14;
                urban.Descriptor.Stats.HitPoints.BaseValue = 100;
                int strBefore = urban.Descriptor.Stats.Strength.ModifiedValue;
                int dexBefore = urban.Descriptor.Stats.Dexterity.ModifiedValue;
                int conBefore = urban.Descriptor.Stats.Constitution.ModifiedValue;
                int maxBefore = urban.MaxHP;
                var rageContext = new MechanicsContext(urban, urban.Descriptor,
                    set.ControlledRage, null, new TargetWrapper(urban));
                Buff rage = urban.Descriptor.Buffs.AddBuff(
                    set.NativeRageBuff, rageContext, null);
                bool substituted = rage != null && ReferenceEquals(
                    rage.Blueprint, set.RageBuff) &&
                    !urban.Descriptor.HasFact(set.NativeRageBuff);
                int strRaging = urban.Descriptor.Stats.Strength.ModifiedValue;
                int dexRaging = urban.Descriptor.Stats.Dexterity.ModifiedValue;
                int conRaging = urban.Descriptor.Stats.Constitution.ModifiedValue;
                int maxRaging = urban.MaxHP;
                Ability selectorFact = urban.Descriptor.Abilities.GetAbility(
                    set.Selector);
                AbilityData selector = selectorFact == null ? null :
                    new AbilityData(selectorFact);
                AbilityData[] visibleOrdinary = selector == null ?
                    new AbilityData[0] : selector.Variants.ToArray();
                bool locked = visibleOrdinary.Length == 6 &&
                    visibleOrdinary.All(value => !value.IsAvailable);
                Add(assertions, "urban-ordinary-rage",
                    "owner-scoped substitution; default STR +4 only; no temporary HP; six options locked while raging",
                    "substituted=" + substituted + ";stats=" + strBefore + "/" +
                        dexBefore + "/" + conBefore + "->" + strRaging + "/" +
                        dexRaging + "/" + conRaging + ";maxHp=" + maxBefore +
                        "->" + maxRaging + ";tempHp=" +
                        urban.Descriptor.Stats.TemporaryHitPoints.ModifiedValue +
                        ";variants=" + visibleOrdinary.Length + ";locked=" + locked,
                    substituted && strRaging == strBefore + 4 &&
                        dexRaging == dexBefore && conRaging == conBefore &&
                        urban.Descriptor.Stats.TemporaryHitPoints.ModifiedValue == 0 &&
                        visibleOrdinary.Length == 6 && locked,
                    "live BuffCollection substitution, actual stats, and AbilityData variants");
                urban.Descriptor.RemoveFact(set.RageBuff);

                stage = "ordinary-split-and-hp";
                SelectDirect(urban.Descriptor, set,
                    ControlledRageTier.Ordinary, 0, 2, 2);
                int damageBefore = 17;
                urban.Descriptor.Damage = damageBefore;
                int hpBefore = urban.HPLeft;
                rage = urban.Descriptor.Buffs.AddBuff(
                    set.NativeRageBuff, rageContext, null);
                int hpDuring = urban.HPLeft;
                int maxDuring = urban.MaxHP;
                int dexSplit = urban.Descriptor.Stats.Dexterity.ModifiedValue;
                int conSplit = urban.Descriptor.Stats.Constitution.ModifiedValue;
                urban.Descriptor.RemoveFact(set.RageBuff);
                int hpAfter = urban.HPLeft;
                int maxAfter = urban.MaxHP;
                Add(assertions, "urban-constitution-hp-cycle",
                    "DEX +2/CON +2 actual scores; damage deficit preserved through entry and exit",
                    "hp=" + hpBefore + "/" + hpDuring + "/" + hpAfter +
                        ";max=" + maxBefore + "/" + maxDuring + "/" + maxAfter +
                        ";damage=" + urban.Descriptor.Damage + ";dex=" + dexSplit +
                        ";con=" + conSplit,
                    dexSplit == dexBefore + 2 && conSplit == conBefore + 2 &&
                        maxDuring >= maxAfter && hpAfter == hpBefore &&
                        maxAfter == maxBefore &&
                        urban.Descriptor.Damage == damageBefore,
                    "genuine Constitution morale modifier and native HP/damage accounting");

                stage = "ordinary-native-rage-toggle";
                BlueprintActivatableAbility rageBlueprint =
                    BlueprintBootstrap.Library.GetAllBlueprints()
                    .OfType<BlueprintActivatableAbility>().Single(value =>
                        value.AssetGuid == NativeRageActivatableGuid);
                ActivatableAbility ordinaryToggle = urban.Descriptor
                    .ActivatableAbilities.Enumerable.SingleOrDefault(value =>
                        value != null && ReferenceEquals(value.Blueprint,
                            rageBlueprint));
                bool ordinaryActivated = false, ordinaryCanceled = false,
                    ordinaryFatigued = false;
                if (ordinaryToggle != null)
                {
                    ordinaryToggle.IsOn = true;
                    ordinaryActivated = ordinaryToggle.IsOn &&
                        urban.Descriptor.HasFact(set.RageBuff);
                    ordinaryToggle.IsOn = false;
                    ordinaryCanceled = !ordinaryToggle.IsOn &&
                        !urban.Descriptor.HasFact(set.RageBuff);
                    ordinaryFatigued = urban.Descriptor.State.HasCondition(
                        UnitCondition.Fatigued);
                    urban.Descriptor.State.RemoveCondition(UnitCondition.Fatigued);
                }
                Add(assertions, "urban-native-rage-lifecycle",
                    "native Rage toggle activates Urban buff, cancels, and applies ordinary fatigue",
                    "toggle=" + (ordinaryToggle != null) + ";activated=" +
                        ordinaryActivated + ";canceled=" + ordinaryCanceled +
                        ";fatigued=" + ordinaryFatigued,
                    ordinaryToggle != null && ordinaryActivated &&
                        ordinaryCanceled && ordinaryFatigued,
                    "native Rage activatable and retained AddFactContextActions lifecycle");

                stage = "tier-transitions";
                for (int level = 2; level <= 11; level++)
                    ApplyLevel(urban.Descriptor, set.BarbarianClass, null, false);
                AbilityData[] greater = new AbilityData(selectorFact).Variants.ToArray();
                bool greaterDefault = Selected(set, urban.Descriptor,
                    ControlledRageTier.Greater, 6, 0, 0);
                SelectDirect(urban.Descriptor, set,
                    ControlledRageTier.Greater, 2, 2, 2);
                rage = urban.Descriptor.Buffs.AddBuff(
                    set.NativeRageBuff, rageContext, null);
                int greaterTotal = AbilityDeltaTotal(urban.Descriptor,
                    strBefore, dexBefore, conBefore);
                urban.Descriptor.RemoveFact(set.RageBuff);
                for (int level = 12; level <= 20; level++)
                    ApplyLevel(urban.Descriptor, set.BarbarianClass, null, false);
                AbilityData[] mighty = new AbilityData(selectorFact).Variants.ToArray();
                bool mightyDefault = Selected(set, urban.Descriptor,
                    ControlledRageTier.Mighty, 8, 0, 0);
                SelectDirect(urban.Descriptor, set,
                    ControlledRageTier.Mighty, 4, 2, 2);
                rage = urban.Descriptor.Buffs.AddBuff(
                    set.NativeRageBuff, rageContext, null);
                int mightyTotal = AbilityDeltaTotal(urban.Descriptor,
                    strBefore, dexBefore, conBefore);
                urban.Descriptor.RemoveFact(set.RageBuff);
                Add(assertions, "urban-greater-mighty-tiers",
                    "actual level 11 exposes only ten +6 options and level 20 only fifteen +8 options with independent STR defaults",
                    "level=" + urban.Descriptor.Progression.GetClassLevel(
                        set.BarbarianClass) + ";greater=" + greater.Length +
                        "/default:" + greaterDefault + "/total:" + greaterTotal +
                        ";mighty=" + mighty.Length + "/default:" +
                        mightyDefault + "/total:" + mightyTotal,
                    greater.Length == 10 && greaterDefault && greaterTotal == 6 &&
                        mighty.Length == 15 && mightyDefault && mightyTotal == 8,
                    "actual Barbarian progression facts, filtered AbilityData variants, and live score modifiers");

                stage = "native-rage-toggle";
                ActivatableAbility nativeToggle = urban.Descriptor
                    .ActivatableAbilities.Enumerable.SingleOrDefault(value =>
                        value != null && ReferenceEquals(value.Blueprint,
                            rageBlueprint));
                bool nativeFeature = urban.Descriptor.HasFact(
                    BlueprintBootstrap.Library.GetAllBlueprints()
                        .OfType<BlueprintFeature>().Single(value =>
                            value.AssetGuid == NativeRageFeatureGuid));
                bool activated = false, canceled = false, fatigued = true;
                if (nativeToggle != null)
                {
                    nativeToggle.IsOn = true;
                    activated = nativeToggle.IsOn &&
                        urban.Descriptor.HasFact(set.RageBuff);
                    nativeToggle.IsOn = false;
                    canceled = !nativeToggle.IsOn &&
                        !urban.Descriptor.HasFact(set.RageBuff);
                    fatigued = urban.Descriptor.State.HasCondition(
                        UnitCondition.Fatigued);
                }
                Add(assertions, "urban-tireless-rage-lifecycle",
                    "level-20 native Rage activates/cancels Urban buff without fatigue",
                    "feature=" + nativeFeature + ";toggle=" +
                        (nativeToggle != null) + ";activated=" + activated +
                        ";canceled=" + canceled + ";fatigued=" + fatigued,
                    nativeFeature && urban.Descriptor.HasFact(
                        set.NativeTirelessRage) && nativeToggle != null &&
                        activated && canceled && !fatigued,
                    "native Rage activatable and retained AddFactContextActions lifecycle");

                stage = "crowd-control";
                BlueprintItemWeapon longsword = BlueprintBootstrap.Library
                    .GetAllBlueprints().OfType<BlueprintItemWeapon>().Single(
                        value => value.AssetGuid == LongswordGuid);
                weapon = ElvenBranchedSpearCombatScenario.Equip(urban, longsword);
                int attackZero = Attack(urban, weapon);
                int acZero = ArmorClass(urban, enemyOne);
                SetPosition(enemyOne, new Vector3(1.5f, 0f, 0f));
                int attackOne = Attack(urban, weapon);
                int acOne = ArmorClass(urban, enemyOne);
                SetPosition(enemyTwo, new Vector3(-1.5f, 0f, 0f));
                int attackTwo = Attack(urban, weapon);
                int acTwo = ArmorClass(urban, enemyOne);
                SetPosition(enemyThree, new Vector3(0f, 0f, 1.5f));
                int attackThree = Attack(urban, weapon);
                int acThree = ArmorClass(urban, enemyOne);
                SetPosition(enemyTwo, new Vector3(-9f, 0f, 0f));
                SetPosition(enemyThree, new Vector3(0f, 0f, 9f));
                int attackMovedOut = Attack(urban, weapon);
                Add(assertions, "urban-crowd-control-rule-events",
                    "zero/one grant none; two/three grant exactly +1 attack and +1 dodge AC; movement updates immediately",
                    "attack=" + attackZero + "/" + attackOne + "/" +
                        attackTwo + "/" + attackThree + "/" + attackMovedOut +
                        ";ac=" + acZero + "/" + acOne + "/" + acTwo + "/" +
                        acThree,
                    attackOne == attackZero && attackTwo == attackZero + 1 &&
                        attackThree == attackZero + 1 &&
                        attackMovedOut == attackZero && acOne == acZero &&
                        acTwo == acZero + 1 && acThree == acZero + 1,
                    "live attack/AC Rulebook events and native edge-to-edge DistanceTo");
            }
            catch (Exception exception)
            {
                diagnostics.Add("stage=" + stage + ";exception=" + exception);
            }
            finally
            {
                ElvenBranchedSpearCombatScenario.RemoveEquipped(urban, ref weapon);
                foreach (UnitEntityData unit in new[] { enemyThree, enemyTwo,
                    enemyOne })
                {
                    if (unit == null) continue;
                    unit.Descriptor.State.Immortality.ReleaseAll();
                    unit.Dispose();
                }
                if (urban != null) urban.Dispose();
                if (scene != null) scene.Dispose();
                if (hostileSource != null)
                    UnityEngine.Object.DestroyImmediate(hostileSource);
                cleaned = Same(unitsBefore, Snapshot(allUnits));
            }
            Add(assertions, "urban-focused-cleanup",
                "global-unit snapshot restored after request-local fixture",
                "cleaned=" + cleaned, cleaned,
                "disposable SceneEntitiesState, units, facts, and weapon");
            Add(assertions, "loaded-mod-version", request.ExpectedModVersion,
                context.ModEntry.Info.Version,
                request.ExpectedModVersion == context.ModEntry.Info.Version,
                "Unity Mod Manager ModEntry.Info.Version");
            RuntimeBuildIdentity identity = RuntimeBuildIdentity.Capture(
                context.Assembly, context.ModEntry.Info.Version);
            bool pass = diagnostics.Count == 0 && assertions.All(value =>
                value.Status == RuntimeTestStatuses.Pass);
            return new RuntimeTestResult
            {
                SchemaVersion = 1, RunId = request.RunId,
                Scenario = request.Scenario,
                Status = pass ? RuntimeTestStatuses.Pass :
                    RuntimeTestStatuses.Fail,
                LoadedModVersion = context.ModEntry.Info.Version,
                RuntimeIdentity = context.Assembly.FullName + ";pid=" +
                    Process.GetCurrentProcess().Id,
                GitCommit = identity.GitCommit,
                GameVersion = Application.version ?? string.Empty,
                StartUtc = started.ToString("o"), EndUtc = string.Empty,
                Assertions = assertions, Diagnostics = diagnostics,
                Warnings = new List<string>(), ExceptionSummary =
                    diagnostics.Count == 0 ? string.Empty : diagnostics[0],
                EvidenceFiles = new List<string>(),
                AutomaticExitRequested = request.ExitAfterCompletion,
                EvidenceDirectory = request.EvidenceDirectory
            };
        }

        private static void ApplyLevel(UnitDescriptor unit,
            BlueprintCharacterClass characterClass, BlueprintArchetype archetype,
            bool first)
        {
            Type type = typeof(LevelUpController);
            MethodInfo start = type.GetMethods(BindingFlags.Public |
                BindingFlags.NonPublic | BindingFlags.Static).Single(value =>
                    value.Name == "StartWithoutAssigningStaticInstance" &&
                    value.GetParameters().Length == 5);
            object mode = Enum.Parse(start.GetParameters()[4].ParameterType,
                first ? "CharGen" : "LevelUp", false);
            object controller = start.Invoke(null,
                new object[] { unit, false, null, null, mode });
            try
            {
                MethodInfo select = type.GetMethod("SelectClass", Members,
                    null, new[] { typeof(BlueprintCharacterClass), typeof(bool) },
                    null);
                if (!(bool)select.Invoke(controller,
                    new object[] { characterClass, false }))
                    throw new InvalidOperationException(
                        "Barbarian class selection was rejected.");
                if (archetype != null)
                {
                    MethodInfo add = type.GetMethod("AddArchetype", Members,
                        null, new[] { typeof(BlueprintArchetype) }, null);
                    if (add == null || !(bool)add.Invoke(controller,
                        new object[] { archetype }))
                        throw new InvalidOperationException(
                            "Urban archetype selection was rejected.");
                }
                type.GetMethod("ApplyClassMechanics", Members).Invoke(
                    controller, null);
                type.GetMethod("ApplyLevelup", Members).Invoke(controller,
                    new object[] { unit });
            }
            finally
            {
                type.GetMethod("Cancel", Members).Invoke(controller, null);
            }
        }

        private static void SelectDirect(UnitDescriptor owner,
            UrbanBarbarianBlueprintSet set, ControlledRageTier tier,
            int strength, int dexterity, int constitution)
        {
            foreach (BlueprintFeature feature in set.SelectionFacts)
                if (owner.HasFact(feature)) owner.RemoveFact(feature);
            ControlledRageAllocation allocation =
                ControlledRageAllocationPolicy.Generate(tier).Single(value =>
                    value.Strength == strength && value.Dexterity == dexterity &&
                    value.Constitution == constitution);
            string symbol = UrbanBarbarianIdentityCatalog.SelectionFeature(
                allocation);
            string guid = UrbanBarbarianIdentityCatalog.All.Single(value =>
                value.Symbol == symbol).Guid;
            BlueprintFeature selected = set.SelectionFacts.Single(value =>
                value.AssetGuid == guid);
            if (owner.AddFact(selected) == null)
                throw new InvalidOperationException(
                    "Controlled Rage selection fact was rejected.");
        }

        private static bool Selected(UrbanBarbarianBlueprintSet set,
            UnitDescriptor owner, ControlledRageTier tier, int strength,
            int dexterity, int constitution)
        {
            ControlledRageAllocation allocation =
                ControlledRageAllocationPolicy.Generate(tier).Single(value =>
                    value.Strength == strength && value.Dexterity == dexterity &&
                    value.Constitution == constitution);
            string symbol = UrbanBarbarianIdentityCatalog.SelectionFeature(
                allocation);
            string guid = UrbanBarbarianIdentityCatalog.All.Single(value =>
                value.Symbol == symbol).Guid;
            return set.SelectionFacts.Any(value => value.AssetGuid == guid &&
                owner.HasFact(value));
        }

        private static int AbilityDeltaTotal(UnitDescriptor owner,
            int strength, int dexterity, int constitution)
        {
            return owner.Stats.Strength.ModifiedValue - strength +
                owner.Stats.Dexterity.ModifiedValue - dexterity +
                owner.Stats.Constitution.ModifiedValue - constitution;
        }

        private static int Attack(UnitEntityData unit, ItemEntityWeapon weapon)
        {
            return Rulebook.Trigger(new RuleCalculateAttackBonusWithoutTarget(
                unit, weapon, 0)).Result;
        }

        private static int ArmorClass(UnitEntityData defender,
            UnitEntityData attacker)
        {
            return Rulebook.Trigger(new RuleCalculateAC(defender, attacker,
                AttackType.Melee)).TargetAC;
        }

        private static void SetPosition(UnitEntityData unit, Vector3 position)
        {
            PropertyInfo property = typeof(UnitEntityData).GetProperty(
                "Position", Members);
            property.SetValue(unit, position, null);
        }

        private static object Read(object value, string name)
        {
            Type type = value.GetType();
            PropertyInfo property = type.GetProperty(name, Members);
            if (property != null) return property.GetValue(value, null);
            FieldInfo field = type.GetField(name, Members);
            if (field != null) return field.GetValue(value);
            throw new MissingMemberException(type.FullName, name);
        }

        private static object[] Snapshot(object source)
        {
            IEnumerable enumerable = source as IEnumerable;
            return enumerable == null ? new object[0] :
                enumerable.Cast<object>().ToArray();
        }

        private static bool Same(object[] left, object[] right)
        {
            return left.Length == right.Length && left.Zip(right,
                (a, b) => ReferenceEquals(a, b)).All(value => value);
        }

        private static void Add(ICollection<RuntimeTestAssertion> assertions,
            string id, string expected, string observed, bool passed,
            string evidence)
        {
            assertions.Add(new RuntimeTestAssertion { Name = id,
                Expected = expected, Observed = observed,
                Status = passed ? RuntimeTestStatuses.Pass :
                    RuntimeTestStatuses.Fail, Evidence = evidence });
        }
    }
}
