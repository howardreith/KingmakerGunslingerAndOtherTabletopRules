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
using Kingmaker.Enums.Damage;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.LevelUp;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
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
                stage = "named-effects";
                string named = QualifyNamedEffects(set, attacker, target,
                    ref equipped, assertions);
                diagnostics.Add(movementObserved);
                diagnostics.Add(baselineDex);
                diagnostics.Add(agileObserved);
                diagnostics.Add(named);
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

        private static string QualifyNamedEffects(
            ElvenBranchedSpearBlueprintSet set, UnitEntityData attacker,
            UnitEntityData target, ref ItemEntityWeapon equipped,
            ICollection<RuntimeTestAssertion> assertions)
        {
            NamedSpearBuffSet buffs = set.Named.Buffs;
            int originalBab = attacker.Descriptor.Stats.BaseAttackBonus.BaseValue;
            int originalSneak = attacker.Descriptor.Stats.SneakAttack.BaseValue;
            int originalFortitude = target.Descriptor.Stats.SaveFortitude.BaseValue;
            int seed = FindNativeD20Seed(10);
            var observed = new List<string>();
            UnitEntityData flankingAlly = null;
            try
            {
                attacker.Descriptor.Stats.BaseAttackBonus.BaseValue = 100;

                RemoveEquipped(attacker, ref equipped);
                equipped = Equip(attacker, set.Named.Require(
                    NamedSpearKind.Boughkeeper).Item);
                ClearNamedBuffs(attacker, target, buffs);
                NamedSpearEffectDiagnostics.Reset();
                int armorBefore = attacker.Descriptor.Stats.AC.ModifiedValue;
                AutoHitAttack(attacker, target, equipped);
                bool ordinaryBough = HasBuff(attacker, buffs.Boughkeeper);
                ExecuteFreshOpportunity(attacker, target, false, seed);
                Buff bough = GetBuff(attacker, buffs.Boughkeeper);
                int armorWithBough = attacker.Descriptor.Stats.AC.ModifiedValue;
                ExecuteFreshOpportunity(attacker, target, false, seed);
                int boughCount = CountBuff(attacker, buffs.Boughkeeper);
                int boughApplications = NamedSpearEffectDiagnostics
                    .ApplicationCount(NamedSpearKind.Boughkeeper);
                RemoveEquipped(attacker, ref equipped);
                equipped = Equip(attacker, set.Require(
                    ElvenBranchedSpearItemKind.Mundane).Item);
                int armorAfterSwap = attacker.Descriptor.Stats.AC.ModifiedValue;
                RemoveEquipped(attacker, ref equipped);
                equipped = Equip(attacker, set.Named.Require(
                    NamedSpearKind.Boughkeeper).Item);
                RemoveBuff(attacker, buffs.Boughkeeper);
                attacker.Descriptor.Stats.BaseAttackBonus.BaseValue = -100;
                ExecuteFreshOpportunity(attacker, target, false, seed);
                int boughAfterMiss = NamedSpearEffectDiagnostics
                    .ApplicationCount(NamedSpearKind.Boughkeeper);
                attacker.Descriptor.Stats.BaseAttackBonus.BaseValue = 100;
                string boughObserved = "ordinary=" + ordinaryBough +
                    ";ac=" + armorBefore + "->" + armorWithBough + "->" +
                    armorAfterSwap + ";applications=" + boughApplications +
                    "->" + boughAfterMiss + ";buffs=" + boughCount +
                    ";duration=" + (bough == null ? -1d :
                        bough.TimeLeft.TotalSeconds);
                Add(assertions, "spear-named-boughkeeper",
                    "ordinary hit and AoO miss rejected; AoO hits refresh one +1 dodge buff; swap invalidates AC",
                    boughObserved, !ordinaryBough && bough != null &&
                    armorWithBough == armorBefore + 1 && boughCount == 1 &&
                    boughApplications == 2 && boughAfterMiss == 2 &&
                    armorAfterSwap == armorBefore && bough.TimeLeft >
                        TimeSpan.Zero && bough.TimeLeft <=
                        TimeSpan.FromSeconds(6.1d),
                    "live native attacks, timed buff, Dodge modifier, and equipment callbacks");
                observed.Add("bough{" + boughObserved + "}");

                RemoveEquipped(attacker, ref equipped);
                equipped = Equip(attacker, set.Named.Require(
                    NamedSpearKind.Thornstep).Item);
                ClearNamedBuffs(attacker, target, buffs);
                NamedSpearEffectDiagnostics.Reset();
                int speedBefore = target.Descriptor.Stats.Speed.ModifiedValue;
                ExecuteFreshOpportunity(attacker, target, false, seed);
                int thornAfterDirect = NamedSpearEffectDiagnostics
                    .ApplicationCount(NamedSpearKind.Thornstep);
                ExecuteFreshOpportunity(attacker, target, true, seed);
                Buff thorn = GetBuff(target, buffs.ThornPenalty);
                Buff thornMarker = GetBuff(attacker, buffs.ThornMarker);
                int speedPenalized = target.Descriptor.Stats.Speed.ModifiedValue;
                ExecuteFreshOpportunity(attacker, target, true, seed);
                int thornAfterRepeat = NamedSpearEffectDiagnostics
                    .ApplicationCount(NamedSpearKind.Thornstep);
                RemoveBuff(attacker, buffs.ThornMarker);
                ExecuteFreshOpportunity(attacker, target, true, seed);
                int thornAfterNextRound = NamedSpearEffectDiagnostics
                    .ApplicationCount(NamedSpearKind.Thornstep);
                int thornCount = CountBuff(target, buffs.ThornPenalty);
                RemoveBuff(target, buffs.ThornPenalty);
                int speedRestored = target.Descriptor.Stats.Speed.ModifiedValue;
                string thornObserved = "applications=" + thornAfterDirect +
                    "->" + thornAfterRepeat + "->" + thornAfterNextRound +
                    ";speed=" + speedBefore + "->" + speedPenalized + "->" +
                    speedRestored + ";buffs=" + thornCount + ";movement=" +
                    NamedSpearEffectDiagnostics.MovementEvaluations;
                Add(assertions, "spear-named-thornstep",
                    "nonmovement rejected; movement AoO applies one -10 speed effect per round; next round refreshes; removal restores",
                    thornObserved, thornAfterDirect == 0 && thorn != null &&
                    thornMarker != null && speedPenalized == speedBefore - 10 &&
                    thornAfterRepeat == 1 && thornAfterNextRound == 2 &&
                    thornCount == 1 && speedRestored == speedBefore,
                    "live Disengage-correlated AoOs, one-round marker, and native Speed stat buff");
                observed.Add("thorn{" + thornObserved + "}");

                RemoveEquipped(attacker, ref equipped);
                equipped = Equip(attacker, set.Named.Require(
                    NamedSpearKind.VipersReach).Item);
                ClearNamedBuffs(attacker, target, buffs);
                NamedSpearEffectDiagnostics.Reset();
                int reflexBefore = target.Descriptor.Stats.SaveReflex.ModifiedValue;
                RuleAttackWithWeapon ordinaryViper = AutoHitAttack(attacker,
                    target, equipped);
                int viperAfterOrdinary = NamedSpearEffectDiagnostics
                    .ApplicationCount(NamedSpearKind.VipersReach);
                attacker.Descriptor.Stats.SneakAttack.BaseValue = 3;
                flankingAlly = CreateFlankingAlly(attacker, target);
                RuleAttackWithWeapon sneakViper = NativeHitAttack(attacker,
                    target, equipped, seed);
                int appliedSneak = AppliedSneakDamage(sneakViper);
                int reflexPenalized = target.Descriptor.Stats.SaveReflex.ModifiedValue;
                int viperAfterSneak = NamedSpearEffectDiagnostics
                    .ApplicationCount(NamedSpearKind.VipersReach);
                NativeHitAttack(attacker, target, equipped, seed);
                int viperAfterRepeat = NamedSpearEffectDiagnostics
                    .ApplicationCount(NamedSpearKind.VipersReach);
                int viperCount = CountBuff(target, buffs.ViperPenalty);
                RemoveBuff(target, buffs.ViperPenalty);
                int reflexRestored = target.Descriptor.Stats.SaveReflex.ModifiedValue;
                string viperObserved = "ordinarySneak=" +
                    ordinaryViper.AttackRoll.IsSneakAttackUsed +
                    ";sneakUsed=" + sneakViper.AttackRoll.IsSneakAttackUsed +
                    ";sneakDamage=" + appliedSneak + ";applications=" +
                    viperAfterOrdinary + "->" + viperAfterSneak + "->" +
                    viperAfterRepeat + ";reflex=" + reflexBefore + "->" +
                    reflexPenalized + "->" + reflexRestored + ";buffs=" +
                    viperCount;
                Add(assertions, "spear-named-vipers-reach",
                    "ordinary damage rejected; positive native sneak packet applies one -2 Reflex effect per round; removal restores",
                    viperObserved, !ordinaryViper.AttackRoll.IsSneakAttackUsed &&
                    sneakViper.AttackRoll.IsSneakAttackUsed && appliedSneak > 0 &&
                    viperAfterOrdinary == 0 && viperAfterSneak == 1 &&
                    viperAfterRepeat == 1 && viperCount == 1 &&
                    reflexPenalized == reflexBefore - 2 &&
                    reflexRestored == reflexBefore,
                    "live flat-footed RuleAttackWithWeapon damage values whose source is Sneak");
                observed.Add("viper{" + viperObserved + "}");

                RemoveEquipped(attacker, ref equipped);
                equipped = Equip(attacker, set.Named.Require(
                    NamedSpearKind.BriarCrownedSpear).Item);
                ClearNamedBuffs(attacker, target, buffs);
                NamedSpearEffectDiagnostics.Reset();
                attacker.Commands.InterruptAll(true);
                PrepareOpportunity(attacker, target);
                SetProperty(attacker.CombatState, "AttackOfOpportunityCount", 4);
                attacker.CombatState.AttackOfOpportunity(target, false);
                UnitAttackOfOpportunity sourceCommand = FindOpportunity(attacker,
                    target, null);
                UnityEngine.Random.InitState(seed);
                int sourceBonus = ExecuteOpportunityCommand(sourceCommand);
                UnitAttackOfOpportunity generatedCommand = FindOpportunity(
                    attacker, target, sourceCommand);
                int countAfterGeneration = attacker.CombatState
                    .AttackOfOpportunityCount;
                UnityEngine.Random.InitState(seed);
                int generatedBonus = ExecuteOpportunityCommand(generatedCommand);
                int commandCountAfterGenerated = attacker.Commands.Raw
                    .OfType<UnitAttackOfOpportunity>().Count(value =>
                        ReferenceEquals(value.Target, target));
                int briarApplications = NamedSpearEffectDiagnostics
                    .ApplicationCount(NamedSpearKind.BriarCrownedSpear);
                int briarGenerated = NamedSpearEffectDiagnostics
                    .GeneratedEvaluations;
                int briarPenalties = NamedSpearEffectDiagnostics
                    .BriarPenaltyApplications;
                attacker.Commands.InterruptAll(true);
                RemoveBuff(attacker, buffs.BriarMarker);
                NamedSpearEffectDiagnostics.Reset();
                PrepareOpportunity(attacker, target);
                SetProperty(attacker.CombatState, "AttackOfOpportunityCount", 1);
                attacker.CombatState.AttackOfOpportunity(target, false);
                UnitAttackOfOpportunity lastOpportunity = FindOpportunity(
                    attacker, target, null);
                UnityEngine.Random.InitState(seed);
                ExecuteOpportunityCommand(lastOpportunity);
                int noResourceCommands = attacker.Commands.Raw
                    .OfType<UnitAttackOfOpportunity>().Count(value =>
                        ReferenceEquals(value.Target, target));
                string briarObserved = "bonus=" + sourceBonus + "->" +
                    generatedBonus + ";remaining=" + countAfterGeneration +
                    ";commands=" + commandCountAfterGenerated +
                    ";applications=" + briarApplications + ";generated=" +
                    briarGenerated + ";penalties=" + briarPenalties +
                    ";noResourceCommands=" + noResourceCommands;
                Add(assertions, "spear-named-briar-crowned",
                    "one generated same-target AoO consumes native resource, applies -5 once, cannot recurse, and requires another AoO",
                    briarObserved, generatedCommand != null &&
                    countAfterGeneration == 2 && generatedBonus ==
                    sourceBonus - 5 && commandCountAfterGenerated == 1 &&
                    briarApplications == 1 && briarGenerated == 1 &&
                    briarPenalties == 1 && noResourceCommands == 1,
                    "explicit generated-command boundary, native AoO count, and live attack bonus calculation");
                observed.Add("briar{" + briarObserved + "}");

                attacker.Commands.InterruptAll(true);
                RemoveEquipped(attacker, ref equipped);
                equipped = Equip(attacker, set.Named.Require(
                    NamedSpearKind.SpearOfTheFirstBranch).Item);
                ClearNamedBuffs(attacker, target, buffs);
                NamedSpearEffectDiagnostics.Reset();
                attacker.Descriptor.Stats.SneakAttack.BaseValue = 0;
                target.Descriptor.Stats.SaveFortitude.BaseValue = -100;
                ExecuteFreshOpportunity(attacker, target, false, seed);
                Buff entangled = target.Descriptor.Buffs.GetBuff(
                    FindFirstBranchEntangled(set));
                int expectedDc = NamedSpearEffectPolicy
                    .FirstBranchDifficultyClass(Math.Max(1, attacker.Descriptor
                        .Progression.CharacterLevel), attacker.Descriptor.Stats
                        .Dexterity.Bonus);
                int savesAfterFailure = NamedSpearEffectDiagnostics.FirstBranchSaves;
                bool failedSave = !NamedSpearEffectDiagnostics.LastFirstBranchPassed;
                RemoveBuff(attacker, buffs.FirstMarker);
                if (entangled != null)
                    target.Descriptor.Buffs.RemoveFact(entangled);
                target.Descriptor.Stats.SaveFortitude.BaseValue = 100;
                ExecuteFreshOpportunity(attacker, target, false, seed);
                Buff firstPenalty = GetBuff(target, buffs.FirstPenalty);
                int savesAfterSuccess = NamedSpearEffectDiagnostics.FirstBranchSaves;
                bool passedSave = NamedSpearEffectDiagnostics.LastFirstBranchPassed;
                RemoveBuff(attacker, buffs.FirstMarker);
                RemoveBuff(target, buffs.FirstPenalty);
                attacker.Descriptor.Stats.SneakAttack.BaseValue = 3;
                EstablishFlanking(attacker, flankingAlly, target);
                int savesBeforeSneak = NamedSpearEffectDiagnostics.FirstBranchSaves;
                RuleAttackWithWeapon firstSneak = NativeHitAttack(attacker,
                    target, equipped, seed);
                int firstSneakDamage = AppliedSneakDamage(firstSneak);
                int savesAfterSneak = NamedSpearEffectDiagnostics.FirstBranchSaves;
                NativeHitAttack(attacker, target, equipped, seed);
                int savesAfterRepeat = NamedSpearEffectDiagnostics.FirstBranchSaves;
                int observedFirstDc = NamedSpearEffectDiagnostics
                    .LastFirstBranchDc;
                RemoveBuff(attacker, buffs.FirstMarker);
                RemoveBuff(target, buffs.FirstPenalty);
                NamedSpearEffectDiagnostics.Reset();
                attacker.Commands.InterruptAll(true);
                PrepareOpportunity(attacker, target);
                BriarGeneratedOpportunityAttackTracker.EnterGeneration();
                try
                {
                    attacker.CombatState.AttackOfOpportunity(target, false);
                }
                finally
                {
                    BriarGeneratedOpportunityAttackTracker.ExitGeneration();
                }
                UnitAttackOfOpportunity markedGenerated = FindOpportunity(
                    attacker, target, null);
                UnityEngine.Random.InitState(seed);
                ExecuteOpportunityCommand(markedGenerated);
                int generatedFirstApplications = NamedSpearEffectDiagnostics
                    .ApplicationCount(NamedSpearKind.SpearOfTheFirstBranch);
                int generatedFirstSaves = NamedSpearEffectDiagnostics.FirstBranchSaves;
                string firstObserved = "dc=" + observedFirstDc + "/" +
                    expectedDc + ";failure=" + failedSave + "/" +
                    savesAfterFailure + ";success=" + passedSave + "/" +
                    savesAfterSuccess + ";sneak=" + firstSneakDamage + "/" +
                    savesBeforeSneak + "->" + savesAfterSneak + "->" +
                    savesAfterRepeat + ";generated=" +
                    generatedFirstApplications + "/" + generatedFirstSaves;
                Add(assertions, "spear-named-first-branch",
                    "AoO failure Entangles; success slows; genuine sneak triggers once; repeated and generated attacks cannot recurse; DC exact",
                    firstObserved, entangled != null && failedSave &&
                    savesAfterFailure == 1 && firstPenalty != null && passedSave &&
                    savesAfterSuccess == 2 && firstSneakDamage > 0 &&
                    savesAfterSneak == savesBeforeSneak + 1 &&
                    savesAfterRepeat == savesAfterSneak &&
                    generatedFirstApplications == 0 && generatedFirstSaves == 0 &&
                    observedFirstDc == expectedDc,
                    "native Fortitude save, native Entangled condition, timed speed buff, sneak damage, and generated-command guard");
                observed.Add("first{" + firstObserved + "}");

                BlueprintItemWeapon moonlit = set.Named.Require(
                    NamedSpearKind.MoonlitFork).Item;
                BlueprintItemWeapon firstBranch = set.Named.Require(
                    NamedSpearKind.SpearOfTheFirstBranch).Item;
                PhysicalDamageMaterial moonlitMaterial = GetWeaponMaterial(
                    moonlit);
                PhysicalDamageMaterial firstMaterial = GetWeaponMaterial(
                    firstBranch);
                string nativeObserved = "moonlitMaterial=" + moonlitMaterial +
                    ";firstMaterial=" + firstMaterial +
                    ";moonlitEnchantments=" + moonlit.Enchantments.Count +
                    ";firstEnchantments=" + firstBranch.Enchantments.Count;
                Add(assertions, "spear-named-native-properties",
                    "Moonlit Fork and First Branch are native cold iron; all approved enhancement enchantments resolve",
                    nativeObserved, moonlitMaterial == PhysicalDamageMaterial
                        .ColdIron && firstMaterial == PhysicalDamageMaterial
                        .ColdIron &&
                    moonlit.Enchantments.All(value => value != null) &&
                    firstBranch.Enchantments.All(value => value != null),
                    "live BlueprintItemWeapon material and enchantment references");
                observed.Add("native{" + nativeObserved + "}");
            }
            finally
            {
                attacker.Commands.InterruptAll(true);
                ClearNamedBuffs(attacker, target, buffs);
                attacker.Descriptor.Stats.BaseAttackBonus.BaseValue = originalBab;
                attacker.Descriptor.Stats.SneakAttack.BaseValue = originalSneak;
                target.Descriptor.Stats.SaveFortitude.BaseValue = originalFortitude;
                if (flankingAlly != null)
                {
                    if (flankingAlly.CombatState.IsInCombat)
                        flankingAlly.CombatState.LeaveCombat();
                    flankingAlly.Dispose();
                }
            }
            return string.Join(";", observed.ToArray());
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

        private static RuleAttackWithWeapon AutoHitAttack(UnitEntityData unit,
            UnitEntityData target, ItemEntityWeapon weapon)
        {
            int damage = target.Descriptor.Damage;
            var attack = new RuleAttackWithWeapon(unit, target, weapon, 0)
            {
                AutoHit = true
            };
            Rulebook.Trigger(attack);
            target.Descriptor.Damage = damage;
            if (attack.AttackRoll == null || !attack.AttackRoll.IsHit)
                throw new InvalidOperationException(
                    "Native AutoHit spear attack did not hit.");
            return attack;
        }

        private static RuleAttackWithWeapon NativeHitAttack(UnitEntityData unit,
            UnitEntityData target, ItemEntityWeapon weapon, int seed)
        {
            int damage = target.Descriptor.Damage;
            UnityEngine.Random.InitState(seed);
            var attack = Rulebook.Trigger(new RuleAttackWithWeapon(unit, target,
                weapon, 0));
            target.Descriptor.Damage = damage;
            if (attack.AttackRoll == null || !attack.AttackRoll.IsHit)
                throw new InvalidOperationException(
                    "The deterministic native spear attack did not hit.");
            return attack;
        }

        private static int AppliedSneakDamage(RuleAttackWithWeapon attack)
        {
            if (attack == null || attack.MeleeDamage == null ||
                attack.MeleeDamage.ResultDamage == null) return 0;
            return attack.MeleeDamage.ResultDamage.Where(value =>
                value.Source != null && value.Source.Sneak &&
                !value.Source.Immune && value.FinalValue > 0)
                .Sum(value => value.FinalValue);
        }

        private static UnitEntityData CreateFlankingAlly(
            UnitEntityData attacker, UnitEntityData target)
        {
            UnitEntityData ally = new ChargenUnit(
                BlueprintRoot.Instance.DefaultPlayerCharacter).Unit;
            if (ally == null) throw new InvalidOperationException(
                "The request-local flanking ally could not be created.");
            ally.CombatState.JoinCombat();
            attacker.CombatState.Engage(target);
            ally.CombatState.Engage(target);
            EstablishFlanking(attacker, ally, target);
            if (!target.CombatState.IsFlanked)
            {
                ally.Dispose();
                throw new InvalidOperationException(
                    "Two native allied engagements did not flank the target.");
            }
            return ally;
        }

        private static void EstablishFlanking(UnitEntityData attacker,
            UnitEntityData ally, UnitEntityData target)
        {
            if (attacker == null || ally == null || target == null)
                throw new ArgumentNullException("flanking fixture");
            attacker.Commands.InterruptAll(true);
            ally.Commands.InterruptAll(true);
            RunTargetingAttack(attacker, target);
            RunTargetingAttack(ally, target);
        }

        private static void RunTargetingAttack(UnitEntityData attacker,
            UnitEntityData target)
        {
            var command = new UnitAttack(attacker);
            SetProperty(command, "Target", target);
            attacker.Commands.Run(command);
            if (!attacker.Commands.AnyCommandTargets(target))
                throw new InvalidOperationException(
                    "The native targeting command was not installed.");
        }

        private static BlueprintBuff FindFirstBranchEntangled(
            ElvenBranchedSpearBlueprintSet set)
        {
            NamedSpearEffectComponent component = set.Named.Enchantments
                .FirstBranch.ComponentsArray.OfType<NamedSpearEffectComponent>()
                .Single();
            return component.EntangledBuff;
        }

        private static PhysicalDamageMaterial GetWeaponMaterial(
            BlueprintItemWeapon weapon)
        {
            FieldInfo field = typeof(BlueprintItemWeapon).GetField(
                "m_DamageType", Members);
            DamageTypeDescription damage = field == null ? null :
                field.GetValue(weapon) as DamageTypeDescription;
            if (damage == null || damage.Type != DamageType.Physical)
                throw new InvalidOperationException(
                    "The named spear lacks a physical damage profile.");
            return damage.Physical.Material;
        }

        private static Buff GetBuff(UnitEntityData unit, BlueprintBuff blueprint)
        {
            return unit == null || unit.Descriptor == null || blueprint == null
                ? null : unit.Descriptor.Buffs.GetBuff(blueprint);
        }

        private static bool HasBuff(UnitEntityData unit, BlueprintBuff blueprint)
        {
            return GetBuff(unit, blueprint) != null;
        }

        private static int CountBuff(UnitEntityData unit,
            BlueprintBuff blueprint)
        {
            return unit == null || unit.Descriptor == null || blueprint == null
                ? 0 : unit.Descriptor.Buffs.RawFacts.OfType<Buff>().Count(value =>
                    ReferenceEquals(value.Blueprint, blueprint));
        }

        private static void RemoveBuff(UnitEntityData unit,
            BlueprintBuff blueprint)
        {
            Buff buff = GetBuff(unit, blueprint);
            if (buff != null) unit.Descriptor.Buffs.RemoveFact(buff);
        }

        private static void ClearNamedBuffs(UnitEntityData attacker,
            UnitEntityData target, NamedSpearBuffSet buffs)
        {
            if (buffs == null) return;
            foreach (BlueprintBuff buff in buffs.All)
            {
                RemoveBuff(attacker, buff);
                RemoveBuff(target, buff);
            }
        }

        private static int FindNativeD20Seed(int expected)
        {
            for (int seed = 1; seed <= 100000; seed++)
            {
                UnityEngine.Random.InitState(seed);
                if (RulebookEvent.Dice.D20.Value == expected) return seed;
            }
            throw new InvalidOperationException(
                "No deterministic native d20 seed produced " + expected + ".");
        }

        private static int ExecuteFreshOpportunity(UnitEntityData attacker,
            UnitEntityData target, bool movement, int seed)
        {
            attacker.Commands.InterruptAll(true);
            PrepareOpportunity(attacker, target);
            bool queued;
            if (movement)
            {
                attacker.CombatState.DisengageAttackTargets.Clear();
                attacker.CombatState.Engage(target);
                attacker.CombatState.Disengage(target);
                queued = true;
            }
            else
            {
                queued = attacker.CombatState.AttackOfOpportunity(target, false);
            }
            if (!queued) throw new InvalidOperationException(
                "The native opportunity attack was not queued.");
            UnitAttackOfOpportunity command = FindOpportunity(attacker, target,
                null);
            UnityEngine.Random.InitState(seed);
            int result = ExecuteOpportunityCommand(command);
            attacker.Commands.InterruptAll(true);
            return result;
        }

        private static UnitAttackOfOpportunity FindOpportunity(
            UnitEntityData attacker, UnitEntityData target,
            UnitAttackOfOpportunity excluded)
        {
            UnitAttackOfOpportunity[] commands = attacker.Commands.Raw
                .OfType<UnitAttackOfOpportunity>().Where(value =>
                    ReferenceEquals(value.Target, target) &&
                    !ReferenceEquals(value, excluded)).ToArray();
            if (commands.Length != 1) throw new InvalidOperationException(
                "Expected one native opportunity command; observed " +
                commands.Length + ".");
            return commands[0];
        }

        private static int ExecuteOpportunityCommand(
            UnitAttackOfOpportunity command)
        {
            if (command == null) throw new ArgumentNullException("command");
            MethodInfo action = typeof(UnitAttackOfOpportunity).GetMethod(
                "OnAction", Members, null, Type.EmptyTypes, null);
            if (action == null) throw new MissingMethodException(
                typeof(UnitAttackOfOpportunity).FullName, "OnAction");
            action.Invoke(command, null);
            return MovementOpportunityAccuracyDiagnostics.LastAttackBonus;
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
