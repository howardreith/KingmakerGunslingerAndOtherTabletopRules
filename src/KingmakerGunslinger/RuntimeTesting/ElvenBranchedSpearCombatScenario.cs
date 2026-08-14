using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.LevelUp;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.FactLogic;
using KingmakerGunslinger.Assets;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElvenBranchedSpear;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Save-free live-unit acceptance for the spear's shared category and the
    /// native attack/damage/provocation rule surfaces. All entities and facts
    /// are request-local and disposed before the result is published.
    /// </summary>
    internal static class ElvenBranchedSpearCombatScenario
    {
        private const BindingFlags Members = BindingFlags.Instance |
            BindingFlags.Public | BindingFlags.NonPublic;
        private const string WeaponFinesseGuid =
            "90e54424d682d104ab36436bd527af09";

        internal static RuntimeTestResult Run(ModContext context,
            RuntimeTestRequest request)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (request == null) throw new ArgumentNullException("request");
            DateTime started = DateTime.UtcNow;
            var assertions = new List<RuntimeTestAssertion>();
            var diagnostics = new List<string>();
            ElvenBranchedSpearBlueprintSet set =
                BlueprintBootstrap.ElvenBranchedSpears;
            if (set == null || set.Named == null)
                throw new InvalidOperationException(
                    "The Elven Branched Spear blueprint family is unavailable.");

            object allUnits = Read(Game.Instance.State, "AllUnits");
            object[] allUnitsBefore = Snapshot(allUnits);
            SceneEntitiesState scene = null;
            UnitEntityData attacker = null;
            UnitEntityData target = null;
            UnitEntityData secondTarget = null;
            BlueprintUnit hostileSource = null;
            ItemEntityWeapon equipped = null;
            var facts = new List<BlueprintUnitFact>();
            GameObject presentation = null;
            bool cleaned = false;
            string stage = "create-live-fixture";
            try
            {
                scene = new SceneEntitiesState(
                    "KMG_Elven_Branched_Spear_Combat_Fixture");
                BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
                attacker = Game.Instance.EntityCreator.SpawnUnit(source,
                    Vector3.zero, Quaternion.identity, scene);
                target = SpawnHostileTarget(attacker, source,
                    new Vector3(1.5f, 0f, 0f), scene, out hostileSource);
                secondTarget = Game.Instance.EntityCreator.SpawnUnit(hostileSource,
                    new Vector3(-1.5f, 0f, 0f), Quaternion.identity, scene);
                if (attacker == null || target == null || secondTarget == null ||
                    attacker.View == null || target.View == null ||
                    secondTarget.View == null)
                    throw new InvalidOperationException(
                        "Native entity creation did not produce three live unit views.");
                target.Descriptor.State.Immortality.Retain();
                secondTarget.Descriptor.State.Immortality.Retain();
                attacker.Descriptor.Stats.Strength.BaseValue = 10;
                attacker.Descriptor.Stats.Dexterity.BaseValue = 20;
                attacker.Descriptor.Stats.BaseAttackBonus.BaseValue = 12;

                stage = "custom-presentation";
                presentation = ElvenBranchedSpearAssetRuntime.InstantiatePrefab();
                bool presentationExact = presentation != null &&
                    presentation.transform.Find("Visual") != null &&
                    presentation.transform.Find("Grip") != null &&
                    presentation.transform.Find("SupportHandTarget") != null &&
                    presentation.transform.Find("Tip") != null &&
                    presentation.transform.Find("Butt") != null;
                Add(assertions, "spear-custom-presentation",
                    "one validated custom prefab with all semantic anchors",
                    ElvenBranchedSpearAssetRuntime.Status,
                    ElvenBranchedSpearAssetRuntime.HasValidatedPrefab &&
                        presentationExact,
                    "dedicated AssetBundle runtime and instantiated GameObject");

                stage = "proficiency";
                equipped = Equip(attacker, set.Require(
                    ElvenBranchedSpearItemKind.Mundane).Item);
                RuleAttackWithWeapon untrained = WeaponAttack(attacker, target,
                    equipped);
                bool untrainedCategory = attacker.Descriptor.Proficiencies.Contains(
                    ElvenBranchedSpearCategoryRuntime.Category);
                BlueprintFeature martial = FindMartialProficiency();
                AddFact(attacker, martial, facts);
                RuleAttackWithWeapon martialOnly = WeaponAttack(attacker, target,
                    equipped);
                bool martialCategory = attacker.Descriptor.Proficiencies.Contains(
                    ElvenBranchedSpearCategoryRuntime.Category);
                RemoveFact(attacker, martial, facts);
                AddFact(attacker, set.ExoticWeaponProficiency, facts);
                RuleAttackWithWeapon exotic = WeaponAttack(attacker, target,
                    equipped);
                bool exoticCategory = attacker.Descriptor.Proficiencies.Contains(
                    ElvenBranchedSpearCategoryRuntime.Category);
                RemoveFact(attacker, set.ExoticWeaponProficiency, facts);
                BlueprintFeature familiarity = BlueprintLibraryLookup.RequireExact<
                    BlueprintFeature>(BlueprintBootstrap.Library,
                        ElvenBranchedSpearBlueprints
                            .NativeElvenWeaponFamiliarityGuid,
                        "native Elven Weapon Familiarity");
                AddFact(attacker, familiarity, facts);
                RuleAttackWithWeapon familiar = WeaponAttack(attacker, target,
                    equipped);
                bool familiarCategory = attacker.Descriptor.Proficiencies.Contains(
                    ElvenBranchedSpearCategoryRuntime.Category);
                RemoveFact(attacker, familiarity, facts);
                string proficiency = "untrained=" +
                    untrained.AttackRoll.AttackBonus +
                    "/" + untrainedCategory + ";martial=" +
                    martialOnly.AttackRoll.AttackBonus + "/" + martialCategory +
                    ";exotic=" + exotic.AttackRoll.AttackBonus + "/" +
                    exoticCategory + ";familiarity=" +
                    familiar.AttackRoll.AttackBonus + "/" +
                    familiarCategory;
                Add(assertions, "spear-proficiency-resolution",
                    "blanket Martial unchanged; exact EWP and native familiarity each remove -4",
                    proficiency,
                    !untrainedCategory && !martialCategory && exoticCategory &&
                    familiarCategory &&
                    martialOnly.AttackRoll.AttackBonus ==
                        untrained.AttackRoll.AttackBonus &&
                    exotic.AttackRoll.AttackBonus ==
                        untrained.AttackRoll.AttackBonus + 4 &&
                    familiar.AttackRoll.AttackBonus ==
                        untrained.AttackRoll.AttackBonus + 4,
                    "live RuleAttackWithWeapon and UnitDescriptor.Proficiencies with native AddProficiencies facts");

                stage = "selector-publication";
                string[] selectorGuids = {
                    "1e1f627d26ad36f43bbd26cc2bf8ac7e",
                    "09c9e82965fb4334b984a1e9df3bd088",
                    "f4201c85a991369408740c6888362e20",
                    "31470b17e8446ae4ea0dacd6c5817d86",
                    "7cf5edc65e785a24f9cf93af987d66b3",
                    "c0b4ec0175e3ff940a45fc21f318a39a",
                    "38ae5ac04463a8947b7c06a6c72dd6bb" };
                int[] selectorCounts = selectorGuids.Select(guid =>
                    BlueprintLibraryLookup.RequireExact<BlueprintParametrizedFeature>(
                        BlueprintBootstrap.Library, guid,
                        "native chosen-weapon selector")
                    .GetFullSelectionItems().Count(value => value != null &&
                        value.Param != null && value.Param.WeaponCategory.HasValue &&
                        value.Param.WeaponCategory.Value.Equals(
                            ElvenBranchedSpearCategoryRuntime.Category))).ToArray();
                int ewpCount = CountFeature(
                    BlueprintLibraryLookup.RequireExact<BlueprintFeatureSelection>(
                        BlueprintBootstrap.Library,
                        ElvenBranchedSpearBlueprints
                            .NativeExoticWeaponProficiencySelectionGuid,
                        "native Exotic Weapon Proficiency selection"),
                    set.ExoticWeaponProficiency);
                int finesseCount = CountFeature(
                    BlueprintLibraryLookup.RequireExact<BlueprintFeatureSelection>(
                        BlueprintBootstrap.Library,
                        ElvenBranchedSpearBlueprints
                            .NativeFinesseTrainingSelectionGuid,
                        "native Finesse Training selection"),
                    set.FinesseTraining);
                string selectors = "parameter=" + string.Join(",",
                    selectorCounts) + ";ewp=" + ewpCount +
                    ";finesse=" + finesseCount;
                Add(assertions, "spear-native-selectors",
                    "seven parameter selectors plus EWP and Finesse Training contain exactly one spear option",
                    selectors, selectorCounts.All(value => value == 1) &&
                        ewpCount == 1 && finesseCount == 1,
                    "live GetFullSelectionItems and BlueprintFeatureSelection catalogs");

                stage = "dexterity-routes";
                AddFact(attacker, set.ExoticWeaponProficiency, facts);
                RuleCalculateAttackBonusWithoutTarget strengthAttack = AttackBonus(
                    attacker, equipped);
                RuleCalculateWeaponStats strengthDamage = WeaponStats(attacker,
                    equipped);
                BlueprintFeature weaponFinesse = BlueprintLibraryLookup.RequireExact<
                    BlueprintFeature>(BlueprintBootstrap.Library,
                        WeaponFinesseGuid, "native Weapon Finesse");
                AddFact(attacker, weaponFinesse, facts);
                RuleCalculateAttackBonusWithoutTarget finesseAttack = AttackBonus(
                    attacker, equipped);
                RuleCalculateWeaponStats finesseOnlyDamage = WeaponStats(attacker,
                    equipped);
                AddFact(attacker, set.FinesseTraining, facts);
                RuleCalculateWeaponStats trainingDamage = WeaponStats(attacker,
                    equipped);
                string baselineDex = "baseline=" + strengthAttack.AttackBonusStat +
                    "/" + DescribeDamage(strengthDamage) + ";finesse=" +
                    finesseAttack.AttackBonusStat + "/" +
                    DescribeDamage(finesseOnlyDamage) + ";training=" +
                    DescribeDamage(trainingDamage);
                Add(assertions, "spear-dexterity-baseline-training",
                    "STR/STR; Weapon Finesse DEX/STR; Finesse Training DEX damage once with native two-hand multiplier",
                    baselineDex,
                    strengthAttack.AttackBonusStat == StatType.Strength &&
                    strengthDamage.DamageBonusStat == StatType.Strength &&
                    finesseAttack.AttackBonusStat == StatType.Dexterity &&
                    finesseOnlyDamage.DamageBonusStat == StatType.Strength &&
                    trainingDamage.DamageBonusStat == StatType.Dexterity &&
                    trainingDamage.DamageBonusStatMultiplier == 1.5f,
                    "live native attack-stat and weapon-stat rule events");

                stage = "agile-and-variant-family";
                RemoveEquipped(attacker, ref equipped);
                equipped = Equip(attacker, set.Named.Require(
                    NamedSpearKind.MoonlitFork).Item);
                RemoveFact(attacker, set.FinesseTraining, facts);
                RuleCalculateWeaponStats agile = WeaponStats(attacker, equipped);
                AddFact(attacker, set.FinesseTraining, facts);
                RuleCalculateWeaponStats agileAndTraining = WeaponStats(attacker,
                    equipped);
                BlueprintItemWeapon[] family = set.Entries.Select(value =>
                    value.Item).Concat(set.Named.Entries.Select(value =>
                        value.Item)).ToArray();
                bool familyDex = true;
                var familyStats = new List<string>();
                foreach (BlueprintItemWeapon blueprint in family)
                {
                    RemoveEquipped(attacker, ref equipped);
                    equipped = Equip(attacker, blueprint);
                    RuleCalculateWeaponStats stats = WeaponStats(attacker, equipped);
                    familyDex &= UsesOneDexterityModifier(attacker, stats);
                    familyStats.Add(blueprint.name + "=" + DescribeDamage(stats));
                }
                string agileObserved = "agile=" + DescribeDamage(agile) +
                    ";agileTraining=" + DescribeDamage(agileAndTraining) +
                    ";family=" + string.Join("|", familyStats.ToArray());
                Add(assertions, "spear-agile-and-family-dexterity",
                    "native Agile uses DEX; Agile plus Training is identical; all 12 variants share DEX category replacement",
                    agileObserved,
                    UsesOneDexterityModifier(attacker, agile) &&
                    UsesOneDexterityModifier(attacker, agileAndTraining) &&
                    agile.DamageBonusStatMultiplier == 1f &&
                    agileAndTraining.DamageBonusStatMultiplier == 1.5f &&
                    familyDex,
                    "native Agile enchantment plus WeaponTypeDamageStatReplacement on live equipped items");

                stage = "movement-opportunity";
                RemoveEquipped(attacker, ref equipped);
                equipped = Equip(attacker, set.Require(
                    ElvenBranchedSpearItemKind.Mundane).Item);
                BlueprintFeature combatReflexes = FindCombatReflexes();
                AddFact(attacker, combatReflexes, facts);
                attacker.CombatState.JoinCombat();
                target.CombatState.JoinCombat();
                secondTarget.CombatState.JoinCombat();
                attacker.CombatState.OnNewRound();
                SetProperty(attacker.CombatState, "AttackOfOpportunityCount",
                    attacker.CombatState.AttackOfOpportunityPerRound);
                attacker.LastMoveTime = Game.Instance.TimeController.GameTime -
                    TimeSpan.FromSeconds(1d);
                attacker.PreviousPosition = attacker.Position;
                PrepareOpportunity(attacker, target);
                string opportunityState = "inCombat=" +
                    attacker.CombatState.IsInCombat + ";canAct=" +
                    attacker.CombatState.CanActInCombat + ";canAoo=" +
                    attacker.CombatState.CanAttackOfOpportunity + ";count=" +
                    attacker.CombatState.AttackOfOpportunityCount +
                    ";perRound=" +
                    attacker.CombatState.AttackOfOpportunityPerRound +
                    ";memory=" + target.Memory.Contains(attacker) +
                    ";enemy=" + target.IsEnemy(attacker) + ";motion=" +
                    attacker.HasMotionThisTick;
                MovementOpportunityAccuracyDiagnostics.Reset();
                RuleCalculateAttackBonusWithoutTarget ordinary = AttackBonus(
                    attacker, equipped);
                bool directQueued = attacker.CombatState.AttackOfOpportunity(target, false);
                string directState;
                int nonMovement = ExecuteOpportunity(
                    attacker, target, out directState);
                attacker.Commands.InterruptAll(true);
                PrepareOpportunity(attacker, target);
                attacker.CombatState.Engage(target);
                attacker.CombatState.Disengage(target);
                string movementState;
                int movement = ExecuteOpportunity(
                    attacker, target, out movementState);
                attacker.Commands.InterruptAll(true);
                PrepareOpportunity(attacker, secondTarget);
                attacker.CombatState.Engage(secondTarget);
                attacker.CombatState.Disengage(secondTarget);
                string secondMovementState;
                int movementTwo = ExecuteOpportunity(
                    attacker, secondTarget, out secondMovementState);
                string movementObserved = "ordinary=" + ordinary.Result +
                    ";direct=" + nonMovement + ";movement=" + movement +
                    ";movement2=" + movementTwo +
                    ";directQueued=" + directQueued + ";commands=" +
                    directState + "|" + movementState + "|" +
                    secondMovementState +
                    ";evaluated=" + MovementOpportunityAccuracyDiagnostics.Evaluated +
                    ";applied=" + MovementOpportunityAccuracyDiagnostics.Applied +
                    ";last=" + MovementOpportunityAccuracyDiagnostics.LastBonus +
                    ";state=" + opportunityState;
                Add(assertions, "spear-movement-opportunity-accuracy",
                    "ordinary and direct nonmovement AoO +0; two Disengage movement AoOs +2 each with one source",
                    movementObserved,
                    directQueued && nonMovement == ordinary.Result &&
                    movement == ordinary.Result + 2 &&
                    movementTwo == ordinary.Result + 2 &&
                    MovementOpportunityAccuracyDiagnostics.Applied == 2 &&
                    MovementOpportunityAccuracyDiagnostics.LastBonus == 2,
                    "live UnitCombatState.AttackOfOpportunity versus Engage/Disengage command construction and native attack-bonus rule");
                diagnostics.Add(movementObserved);
                diagnostics.Add(baselineDex);
                diagnostics.Add(agileObserved);
            }
            catch (Exception exception)
            {
                Add(assertions, "spear-combat-scenario-exception",
                    "no exception", "stage=" + stage + ";" + exception,
                    false, "exception-contained disposable fixture");
            }
            finally
            {
                if (presentation != null)
                    UnityEngine.Object.DestroyImmediate(presentation);
                if (attacker != null)
                {
                    attacker.Commands.InterruptAll(true);
                    if (attacker.CombatState.IsInCombat)
                        attacker.CombatState.LeaveCombat();
                    RemoveEquipped(attacker, ref equipped);
                    foreach (BlueprintUnitFact fact in facts.ToArray())
                        if (fact != null && attacker.Descriptor.HasFact(fact))
                            attacker.Descriptor.RemoveFact(fact);
                }
                RemoveMemory(target, attacker);
                RemoveMemory(secondTarget, attacker);
                if (target != null && target.CombatState.IsInCombat)
                    target.CombatState.LeaveCombat();
                if (secondTarget != null && secondTarget.CombatState.IsInCombat)
                    secondTarget.CombatState.LeaveCombat();
                if (target != null)
                    target.Descriptor.State.Immortality.ReleaseAll();
                if (secondTarget != null)
                    secondTarget.Descriptor.State.Immortality.ReleaseAll();
                if (secondTarget != null) secondTarget.Dispose();
                if (target != null) target.Dispose();
                if (attacker != null) attacker.Dispose();
                if (scene != null) scene.Dispose();
                if (hostileSource != null)
                    UnityEngine.Object.DestroyImmediate(hostileSource);
                cleaned = SameReferences(allUnitsBefore, Snapshot(allUnits));
            }
            Add(assertions, "spear-combat-fixture-cleanup",
                "global-unit snapshot restored and all request-local objects disposed",
                "cleaned=" + cleaned, cleaned,
                "disposable SceneEntitiesState, units, items, facts, memory, and prefab");
            Add(assertions, "loaded-mod-version", request.ExpectedModVersion,
                context.ModEntry.Info.Version,
                string.Equals(request.ExpectedModVersion,
                    context.ModEntry.Info.Version, StringComparison.Ordinal),
                "Unity Mod Manager ModEntry.Info.Version");

            RuntimeBuildIdentity identity = RuntimeBuildIdentity.Capture(
                context.Assembly, context.ModEntry.Info.Version);
            bool pass = assertions.All(value =>
                value.Status == RuntimeTestStatuses.Pass);
            return new RuntimeTestResult
            {
                SchemaVersion = 1,
                RunId = request.RunId,
                Scenario = request.Scenario,
                Status = pass ? RuntimeTestStatuses.Pass :
                    RuntimeTestStatuses.Fail,
                LoadedModVersion = context.ModEntry.Info.Version,
                RuntimeIdentity = identity.RuntimeIdentity + "; mvid=" +
                    identity.ModuleVersionId + "; sha256=" +
                    identity.LoadedModuleSha256 + "; pid=" + identity.ProcessId,
                GitCommit = identity.GitCommit,
                GameVersion = Application.version ?? string.Empty,
                StartUtc = started.ToString("o"),
                EndUtc = DateTime.UtcNow.ToString("o"),
                Assertions = assertions,
                Diagnostics = diagnostics,
                Warnings = new List<string>(),
                ExceptionSummary = string.Empty,
                EvidenceFiles = new List<string>(),
                AutomaticExitRequested = request.ExitAfterCompletion,
                EvidenceDirectory = request.EvidenceDirectory
            };
        }

        private static ItemEntityWeapon Equip(UnitEntityData unit,
            BlueprintItemWeapon blueprint)
        {
            var item = new ItemEntityWeapon(blueprint);
            unit.Body.PrimaryHand.InsertItem(item);
            if (!ReferenceEquals(unit.Body.PrimaryHand.MaybeWeapon, item))
                throw new InvalidOperationException(
                    "The spear did not remain in the primary hand.");
            return item;
        }

        private static void RemoveEquipped(UnitEntityData unit,
            ref ItemEntityWeapon item)
        {
            if (unit != null && unit.Body != null &&
                unit.Body.PrimaryHand != null &&
                unit.Body.PrimaryHand.MaybeItem != null)
                unit.Body.PrimaryHand.RemoveItem(false);
            if (item != null) item.Dispose();
            item = null;
        }

        private static RuleCalculateAttackBonusWithoutTarget AttackBonus(
            UnitEntityData unit, ItemEntityWeapon weapon)
        {
            return Rulebook.Trigger(new RuleCalculateAttackBonusWithoutTarget(
                unit, weapon, 0));
        }

        private static RuleAttackWithWeapon WeaponAttack(UnitEntityData unit,
            UnitEntityData target, ItemEntityWeapon weapon)
        {
            int damage = target.Descriptor.Damage;
            int penalty = NativeSingleAttackPenalty(unit, target, weapon);
            var result = Rulebook.Trigger(new RuleAttackWithWeapon(unit, target,
                weapon, penalty));
            target.Descriptor.Damage = damage;
            if (result.AttackRoll == null)
                throw new InvalidOperationException(
                    "Native weapon attack did not expose its attack roll.");
            return result;
        }

        private static int NativeSingleAttackPenalty(UnitEntityData unit,
            UnitEntityData target, ItemEntityWeapon weapon)
        {
            var command = new UnitAttack(unit);
            SetProperty(command, "Executor", unit);
            SetProperty(command, "Target", target);
            MethodInfo create = typeof(UnitAttack).GetMethod(
                "CreateSingleAttack", Members);
            var attacks = create == null ? null : create.Invoke(command, null) as
                IEnumerable<AttackHandInfo>;
            AttackHandInfo planned = attacks == null ? null : attacks.SingleOrDefault(
                value => value != null && ReferenceEquals(value.Weapon, weapon));
            if (planned == null)
                throw new InvalidOperationException(
                    "Native UnitAttack did not plan the equipped spear attack.");
            return planned.AttackBonusPenalty;
        }

        private static UnitEntityData SpawnHostileTarget(UnitEntityData attacker,
            BlueprintUnit source, Vector3 position, SceneEntitiesState scene,
            out BlueprintUnit hostileSource)
        {
            FieldInfo faction = typeof(BlueprintUnit).GetFields(Members)
                .Single(value => value.Name == "Faction" ||
                    value.Name == "m_Faction");
            object playerFaction = faction.GetValue(source);
            var seen = new HashSet<object>();
            foreach (BlueprintUnit candidate in BlueprintBootstrap.Library
                .GetAllBlueprints().OfType<BlueprintUnit>()
                .OrderBy(value => value.AssetGuid, StringComparer.Ordinal))
            {
                object candidateFaction = faction.GetValue(candidate);
                if (candidateFaction == null || ReferenceEquals(candidateFaction,
                    playerFaction) || !seen.Add(candidateFaction)) continue;
                BlueprintUnit clone = UnityEngine.Object.Instantiate(source);
                clone.name = "KMG_Runtime_ElvenBranchedSpear_HostileTarget";
                clone.IsCheater = true;
                faction.SetValue(clone, candidateFaction);
                UnitEntityData target = null;
                try
                {
                    target = Game.Instance.EntityCreator.SpawnUnit(clone,
                        position, Quaternion.identity, scene);
                    if (target != null && target.IsEnemy(attacker))
                    {
                        hostileSource = clone;
                        return target;
                    }
                }
                catch { }
                if (target != null) target.Dispose();
                UnityEngine.Object.DestroyImmediate(clone);
            }
            hostileSource = null;
            throw new InvalidOperationException(
                "No loaded faction produced a hostile disposable target.");
        }

        private static RuleCalculateWeaponStats WeaponStats(UnitEntityData unit,
            ItemEntityWeapon weapon)
        {
            return Rulebook.Trigger(new RuleCalculateWeaponStats(unit, weapon,
                null));
        }

        private static string DescribeDamage(RuleCalculateWeaponStats value)
        {
            return value.DamageBonusStat + "x" +
                value.DamageBonusStatMultiplier + ";bonus=" + value.BonusDamage;
        }

        private static bool UsesOneDexterityModifier(UnitEntityData unit,
            RuleCalculateWeaponStats stats)
        {
            if (unit == null || stats == null ||
                stats.DamageBonusStat != StatType.Dexterity) return false;
            int expectedAttribute = (int)Math.Floor(
                unit.Descriptor.Stats.Dexterity.Bonus *
                stats.DamageBonusStatMultiplier);
            return stats.BonusDamage == expectedAttribute + stats.Enhancement;
        }

        private static void AddFact(UnitEntityData unit,
            BlueprintUnitFact fact, IList<BlueprintUnitFact> facts)
        {
            if (unit.Descriptor.AddFact(fact) == null)
                throw new InvalidOperationException(
                    "Could not add request-local fact " + fact.name + ".");
            facts.Add(fact);
        }

        private static void RemoveFact(UnitEntityData unit,
            BlueprintUnitFact fact, IList<BlueprintUnitFact> facts)
        {
            if (unit.Descriptor.HasFact(fact)) unit.Descriptor.RemoveFact(fact);
            facts.Remove(fact);
        }

        private static BlueprintFeature FindMartialProficiency()
        {
            BlueprintFeature[] candidates = BlueprintBootstrap.Library
                .GetAllBlueprints().OfType<BlueprintFeature>().Where(value =>
                    value != null && value.ComponentsArray != null &&
                    value.ComponentsArray.OfType<AddProficiencies>().Any(grant =>
                        (grant.WeaponProficiencies ??
                            Array.Empty<WeaponCategory>()).Length >= 10) &&
                    (value.name ?? string.Empty).IndexOf("Martial",
                        StringComparison.OrdinalIgnoreCase) >= 0 &&
                    (value.name ?? string.Empty).IndexOf("Proficien",
                        StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(value => value.AssetGuid, StringComparer.Ordinal)
                .ToArray();
            if (candidates.Length != 1)
                throw new InvalidOperationException(
                    "Expected one native blanket Martial Weapon Proficiency; observed " +
                    candidates.Length + ".");
            return candidates[0];
        }

        private static BlueprintFeature FindCombatReflexes()
        {
            BlueprintFeature[] candidates = BlueprintBootstrap.Library
                .GetAllBlueprints().OfType<BlueprintFeature>().Where(value =>
                    value != null && string.Equals(value.name,
                        "CombatReflexes", StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (candidates.Length != 1)
                throw new InvalidOperationException(
                    "Expected one native Combat Reflexes feature; observed " +
                    candidates.Length + ".");
            return candidates[0];
        }

        private static int CountFeature(BlueprintFeatureSelection selection,
            BlueprintFeature feature)
        {
            return (selection.AllFeatures ?? Array.Empty<BlueprintFeature>())
                .Count(value => ReferenceEquals(value, feature) || value != null &&
                    string.Equals(value.AssetGuid, feature.AssetGuid,
                        StringComparison.Ordinal));
        }

        private static void PrepareOpportunity(UnitEntityData attacker,
            UnitEntityData target)
        {
            target.Memory.Add(attacker);
            SetProperty(attacker.CombatState, "AttackOfOpportunityCount", 4);
        }

        private static int ExecuteOpportunity(
            UnitEntityData attacker, UnitEntityData target, out string detail)
        {
            UnitAttackOfOpportunity command = attacker.Commands.Raw
                .OfType<UnitAttackOfOpportunity>().SingleOrDefault(value =>
                    ReferenceEquals(value.Target, target));
            bool found = command != null;
            bool canStart = found && command.CanStart;
            int evaluationsBefore = MovementOpportunityAccuracyDiagnostics.Evaluated;
            object result = null;
            if (command != null)
            {
                MethodInfo action = typeof(UnitAttackOfOpportunity).GetMethod(
                    "OnAction", Members, null, Type.EmptyTypes, null);
                if (action == null) throw new MissingMethodException(
                    typeof(UnitAttackOfOpportunity).FullName, "OnAction");
                result = action.Invoke(command, null);
            }
            int bonus = MovementOpportunityAccuracyDiagnostics.LastAttackBonus;
            detail = "found=" + found + ",canStart=" + canStart +
                ",hand=" + (command != null && command.Hand != null) +
                ",nativeResult=" + (result ?? "<null>") +
                ",evaluated=" +
                (MovementOpportunityAccuracyDiagnostics.Evaluated -
                    evaluationsBefore) + ",attackBonus=" + bonus;
            return bonus;
        }

        private static void RemoveMemory(UnitEntityData owner,
            UnitEntityData remembered)
        {
            if (owner != null && remembered != null && owner.Memory != null &&
                owner.Memory.Contains(remembered)) owner.Memory.Remove(remembered);
        }

        private static void SetProperty(object owner, string name, object value)
        {
            PropertyInfo property = null;
            for (Type type = owner == null ? null : owner.GetType();
                type != null && property == null; type = type.BaseType)
                property = type.GetProperty(name, Members |
                    BindingFlags.DeclaredOnly);
            MethodInfo setter = property == null ? null :
                property.GetSetMethod(true);
            if (setter == null) throw new MissingMemberException(
                owner == null ? "<null>" : owner.GetType().FullName, name);
            setter.Invoke(owner, new[] { value });
        }

        private static object Read(object owner, string name)
        {
            if (owner == null) return null;
            PropertyInfo property = owner.GetType().GetProperty(name, Members);
            if (property != null) return property.GetValue(owner, null);
            FieldInfo field = owner.GetType().GetField(name, Members);
            return field == null ? null : field.GetValue(owner);
        }

        private static object[] Snapshot(object collection)
        {
            var result = new List<object>();
            var values = collection as System.Collections.IEnumerable;
            if (values != null)
                foreach (object value in values) result.Add(value);
            return result.ToArray();
        }

        private static bool SameReferences(object[] before, object[] after)
        {
            return before.Length == after.Length && before.All(value =>
                after.Any(candidate => ReferenceEquals(candidate, value)));
        }

        private static void Add(ICollection<RuntimeTestAssertion> assertions,
            string name, string expected, string observed, bool passed,
            string evidence)
        {
            assertions.Add(new RuntimeTestAssertion
            {
                Name = name,
                Expected = expected,
                Observed = observed ?? string.Empty,
                Status = passed ? RuntimeTestStatuses.Pass :
                    RuntimeTestStatuses.Fail,
                Evidence = evidence
            });
        }
    }
}
