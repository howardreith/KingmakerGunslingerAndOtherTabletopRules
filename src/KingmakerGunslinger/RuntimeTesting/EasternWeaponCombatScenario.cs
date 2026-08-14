using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.LevelUp;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.EasternWeapons;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Save-free live-rule qualification for the Eastern Weapons catalog. The
    /// fixture uses real spawned units, equipped ItemEntityWeapon instances,
    /// native attacks/stat rules, native activatable state, and exact cleanup.
    /// </summary>
    internal static class EasternWeaponCombatScenario
    {
        private const string WeaponFinesseGuid =
            "90e54424d682d104ab36436bd527af09";
        private const string ShortswordItemGuid =
            "57c8994d1f1becf49ac4f642e5d8ca9d";

        internal static RuntimeTestResult Run(ModContext context,
            RuntimeTestRequest request)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (request == null) throw new ArgumentNullException("request");
            DateTime started = DateTime.UtcNow;
            var assertions = new List<RuntimeTestAssertion>();
            var diagnostics = new List<string>();
            EasternWeaponBlueprintSet set = BlueprintBootstrap.EasternWeapons;
            if (set == null || set.Named == null)
                throw new InvalidOperationException(
                    "The Eastern Weapons blueprint catalog is unavailable.");

            object allUnits = ElvenBranchedSpearCombatScenario.Read(
                Game.Instance.State, "AllUnits");
            object[] allUnitsBefore = ElvenBranchedSpearCombatScenario.Snapshot(
                allUnits);
            SceneEntitiesState scene = null;
            UnitEntityData attacker = null;
            UnitEntityData target = null;
            BlueprintUnit hostileSource = null;
            ItemEntityWeapon equipped = null;
            ItemEntityWeapon offhand = null;
            var facts = new List<BlueprintUnitFact>();
            ActivatableAbility powerAttack = null;
            bool cleaned = false;
            string stage = "create-live-fixture";
            try
            {
                scene = new SceneEntitiesState(
                    "KMG_Eastern_Weapons_Combat_Fixture");
                BlueprintUnit source = BlueprintRoot.Instance
                    .DefaultPlayerCharacter;
                attacker = Game.Instance.EntityCreator.SpawnUnit(source,
                    Vector3.zero, Quaternion.identity, scene);
                target = ElvenBranchedSpearCombatScenario.SpawnHostileTarget(
                    attacker, source, new Vector3(1.5f, 0f, 0f), scene,
                    out hostileSource);
                if (attacker == null || target == null || attacker.View == null ||
                    target.View == null)
                    throw new InvalidOperationException(
                        "Native entity creation did not produce live unit views.");
                target.Descriptor.State.Immortality.Retain();
                attacker.Descriptor.Stats.Strength.BaseValue = 10;
                attacker.Descriptor.Stats.Dexterity.BaseValue = 20;
                attacker.Descriptor.Stats.BaseAttackBonus.BaseValue = 12;

                stage = "catalog-and-presentation";
                QualifyCatalog(set, assertions);

                stage = "proficiency";
                BlueprintFeature martial = ElvenBranchedSpearCombatScenario
                    .FindMartialProficiency();
                QualifyProficiency(set, attacker, target, martial, facts,
                    ref equipped, ref offhand, assertions, diagnostics);

                stage = "finesse";
                QualifyFinesse(set, attacker, facts, ref equipped, assertions,
                    diagnostics);

                stage = "named-effects";
                QualifyNamedEffects(set, attacker, target, facts,
                    ref equipped, ref offhand, ref powerAttack, assertions,
                    diagnostics);

                stage = "capstones";
                QualifyCapstones(set, attacker, ref equipped, assertions,
                    diagnostics);
            }
            catch (Exception exception)
            {
                ElvenBranchedSpearCombatScenario.Add(assertions,
                    "eastern-combat-scenario-exception", "no exception",
                    "stage=" + stage + ";" + exception, false,
                    "exception-contained request-local fixture");
            }
            finally
            {
                if (powerAttack != null && powerAttack.IsOn)
                    powerAttack.IsOn = false;
                if (attacker != null)
                {
                    attacker.Commands.InterruptAll(true);
                    RemoveOffhand(attacker, ref offhand);
                    ElvenBranchedSpearCombatScenario.RemoveEquipped(attacker,
                        ref equipped);
                    foreach (BlueprintUnitFact fact in facts.ToArray())
                        if (fact != null && attacker.Descriptor.HasFact(fact))
                            attacker.Descriptor.RemoveFact(fact);
                    foreach (Kingmaker.UnitLogic.Buffs.Blueprints.BlueprintBuff
                        buff in set.Named.Buffs.All)
                    {
                        Buff current = attacker.Descriptor.Buffs.GetBuff(buff);
                        if (current != null)
                            attacker.Descriptor.Buffs.RemoveFact(current);
                    }
                }
                if (target != null)
                    target.Descriptor.State.Immortality.ReleaseAll();
                if (target != null) target.Dispose();
                if (attacker != null) attacker.Dispose();
                if (scene != null) scene.Dispose();
                if (hostileSource != null)
                    UnityEngine.Object.DestroyImmediate(hostileSource);
                cleaned = ElvenBranchedSpearCombatScenario.SameReferences(
                    allUnitsBefore,
                    ElvenBranchedSpearCombatScenario.Snapshot(allUnits));
            }

            ElvenBranchedSpearCombatScenario.Add(assertions,
                "eastern-combat-fixture-cleanup",
                "global-unit snapshot restored and request-local objects disposed",
                "cleaned=" + cleaned, cleaned,
                "disposable SceneEntitiesState, units, items, facts, buffs");
            ElvenBranchedSpearCombatScenario.Add(assertions,
                "loaded-mod-version", request.ExpectedModVersion,
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

        private static void QualifyCatalog(EasternWeaponBlueprintSet set,
            ICollection<RuntimeTestAssertion> assertions)
        {
            bool exactRelations = set.Families.Length == 3 &&
                set.Entries.Length == 12 && set.Named.Entries.Length == 18 &&
                set.Families.All(family => family.Entries.Length == 4 &&
                    family.Entries.All(entry => ReferenceEquals(
                        entry.Item.Type, family.WeaponType))) &&
                set.Named.Entries.All(entry => ReferenceEquals(entry.Item.Type,
                    set.Require(entry.Spec.Family).WeaponType));
            string observed = "families=" + set.Families.Length +
                ";generic=" + set.Entries.Length + ";named=" +
                set.Named.Entries.Length;
            ElvenBranchedSpearCombatScenario.Add(assertions,
                "eastern-catalog-identity",
                "3 stable family types, 12 generic items, and 18 named items",
                observed, exactRelations,
                "live registered BlueprintItemWeapon/BlueprintWeaponType references");

            string titles = set.WakizashiProficiency.Name + "|" +
                set.KatanaProficiency.Name;
            ElvenBranchedSpearCombatScenario.Add(assertions,
                "eastern-proficiency-presentation",
                "Weapon Proficiency (Wakizashi) and Weapon Proficiency (Katana)",
                titles,
                string.Equals(set.WakizashiProficiency.Name,
                    "Weapon Proficiency (Wakizashi)", StringComparison.Ordinal) &&
                string.Equals(set.KatanaProficiency.Name,
                    "Weapon Proficiency (Katana)", StringComparison.Ordinal),
                "live localized static proficiency children");
        }

        private static void QualifyProficiency(EasternWeaponBlueprintSet set,
            UnitEntityData attacker, UnitEntityData target,
            BlueprintFeature martial, IList<BlueprintUnitFact> facts,
            ref ItemEntityWeapon equipped, ref ItemEntityWeapon offhand,
            ICollection<RuntimeTestAssertion> assertions,
            ICollection<string> diagnostics)
        {
            BlueprintItemWeapon wakizashi = set.Require(
                EasternWeaponFamily.Wakizashi,
                EasternWeaponGenericKind.Mundane).Item;
            equipped = ElvenBranchedSpearCombatScenario.Equip(attacker,
                wakizashi);
            int wakUntrained = Attack(attacker, target, equipped);
            ElvenBranchedSpearCombatScenario.AddFact(attacker, martial, facts);
            int wakMartial = Attack(attacker, target, equipped);
            ElvenBranchedSpearCombatScenario.RemoveFact(attacker, martial, facts);
            ElvenBranchedSpearCombatScenario.AddFact(attacker,
                set.WakizashiProficiency, facts);
            int wakExact = Attack(attacker, target, equipped);
            ElvenBranchedSpearCombatScenario.RemoveFact(attacker,
                set.WakizashiProficiency, facts);
            string wakObserved = wakUntrained + "/" + wakMartial + "/" +
                wakExact;
            ElvenBranchedSpearCombatScenario.Add(assertions,
                "eastern-wakizashi-proficiency",
                "blanket martial leaves -4; exact Wakizashi proficiency removes it",
                wakObserved, wakMartial == wakUntrained &&
                    wakExact == wakUntrained + 4,
                "live RuleAttackWithWeapon and exact AddProficiencies fact");

            Swap(attacker, set.Require(EasternWeaponFamily.Katana,
                EasternWeaponGenericKind.Mundane).Item, ref equipped);
            int katanaTwoUntrained = Attack(attacker, target, equipped);
            ElvenBranchedSpearCombatScenario.AddFact(attacker, martial, facts);
            int katanaTwoMartial = Attack(attacker, target, equipped);
            bool nativeTwoHands = equipped.HoldInTwoHands;
            offhand = EquipOffhand(attacker);
            bool nativeOneHand = !equipped.HoldInTwoHands;
            int katanaOneMartial = Attack(attacker, target, equipped);
            ElvenBranchedSpearCombatScenario.RemoveFact(attacker, martial, facts);
            ElvenBranchedSpearCombatScenario.AddFact(attacker,
                set.KatanaProficiency, facts);
            int katanaOneExact = Attack(attacker, target, equipped);
            RemoveOffhand(attacker, ref offhand);
            int katanaTwoExact = Attack(attacker, target, equipped);
            ElvenBranchedSpearCombatScenario.RemoveFact(attacker,
                set.KatanaProficiency, facts);
            string katanaObserved = "two=" + katanaTwoUntrained + "->" +
                katanaTwoMartial + "->" + katanaTwoExact + ";one=" +
                katanaOneMartial + "->" + katanaOneExact + ";grip=" +
                nativeTwoHands + "/" + nativeOneHand;
            ElvenBranchedSpearCombatScenario.Add(assertions,
                "eastern-katana-grip-proficiency",
                "martial removes -4 only in native two-hand grip; exact Katana proficiency removes it in both grips",
                katanaObserved, nativeTwoHands && nativeOneHand &&
                    katanaTwoMartial == katanaTwoUntrained + 4 &&
                    katanaOneExact == katanaOneMartial + 4 &&
                    katanaTwoExact == katanaTwoUntrained + 4,
                "ItemEntityWeapon.HoldInTwoHands and live RuleAttackWithWeapon");

            Swap(attacker, set.Require(EasternWeaponFamily.Nodachi,
                EasternWeaponGenericKind.Mundane).Item, ref equipped);
            int nodachiUntrained = Attack(attacker, target, equipped);
            ElvenBranchedSpearCombatScenario.AddFact(attacker, martial, facts);
            int nodachiMartial = Attack(attacker, target, equipped);
            ElvenBranchedSpearCombatScenario.RemoveFact(attacker, martial, facts);
            string nodachiObserved = nodachiUntrained + "->" + nodachiMartial;
            ElvenBranchedSpearCombatScenario.Add(assertions,
                "eastern-nodachi-martial-proficiency",
                "blanket martial proficiency removes the ordinary -4",
                nodachiObserved, nodachiMartial == nodachiUntrained + 4,
                "live broad native Martial Weapon Proficiency fact");
            diagnostics.Add("proficiency{wak=" + wakObserved + ";katana=" +
                katanaObserved + ";nodachi=" + nodachiObserved + "}");
        }

        private static void QualifyFinesse(EasternWeaponBlueprintSet set,
            UnitEntityData attacker, IList<BlueprintUnitFact> facts,
            ref ItemEntityWeapon equipped,
            ICollection<RuntimeTestAssertion> assertions,
            ICollection<string> diagnostics)
        {
            Swap(attacker, set.Require(EasternWeaponFamily.Wakizashi,
                EasternWeaponGenericKind.Mundane).Item, ref equipped);
            RuleCalculateAttackBonusWithoutTarget baseAttack =
                ElvenBranchedSpearCombatScenario.AttackBonus(attacker, equipped);
            RuleCalculateWeaponStats baseDamage =
                ElvenBranchedSpearCombatScenario.WeaponStats(attacker, equipped);
            BlueprintFeature finesse = BlueprintLibraryLookup.RequireExact<
                BlueprintFeature>(BlueprintBootstrap.Library,
                    WeaponFinesseGuid, "native Weapon Finesse");
            ElvenBranchedSpearCombatScenario.AddFact(attacker, finesse, facts);
            RuleCalculateAttackBonusWithoutTarget finesseAttack =
                ElvenBranchedSpearCombatScenario.AttackBonus(attacker, equipped);
            RuleCalculateWeaponStats finesseDamage =
                ElvenBranchedSpearCombatScenario.WeaponStats(attacker, equipped);
            ElvenBranchedSpearCombatScenario.AddFact(attacker,
                set.WakizashiFinesseTraining, facts);
            RuleCalculateWeaponStats trainingDamage =
                ElvenBranchedSpearCombatScenario.WeaponStats(attacker, equipped);
            string observed = baseAttack.AttackBonusStat + "/" +
                baseDamage.DamageBonusStat + "->" +
                finesseAttack.AttackBonusStat + "/" +
                finesseDamage.DamageBonusStat + "->" +
                trainingDamage.DamageBonusStat + "x" +
                trainingDamage.DamageBonusStatMultiplier;
            ElvenBranchedSpearCombatScenario.Add(assertions,
                "eastern-wakizashi-finesse",
                "STR/STR; Weapon Finesse DEX/STR; Finesse Training DEX damage once",
                observed,
                baseAttack.AttackBonusStat == StatType.Strength &&
                baseDamage.DamageBonusStat == StatType.Strength &&
                finesseAttack.AttackBonusStat == StatType.Dexterity &&
                finesseDamage.DamageBonusStat == StatType.Strength &&
                trainingDamage.DamageBonusStat == StatType.Dexterity &&
                trainingDamage.DamageBonusStatMultiplier == 1f,
                "live attack-stat and weapon-stat rule events");
            ElvenBranchedSpearCombatScenario.RemoveFact(attacker,
                set.WakizashiFinesseTraining, facts);
            ElvenBranchedSpearCombatScenario.RemoveFact(attacker, finesse, facts);
            diagnostics.Add("finesse{" + observed + "}");
        }

        private static void QualifyNamedEffects(EasternWeaponBlueprintSet set,
            UnitEntityData attacker, UnitEntityData target,
            IList<BlueprintUnitFact> facts, ref ItemEntityWeapon equipped,
            ref ItemEntityWeapon offhand, ref ActivatableAbility powerAttack,
            ICollection<RuntimeTestAssertion> assertions,
            ICollection<string> diagnostics)
        {
            int initiativeBefore = attacker.Descriptor.Stats.Initiative.ModifiedValue;
            Swap(attacker, set.Named.Require(
                EasternWeaponNamedKind.WayfarersOath).Item, ref equipped);
            int initiativeEquipped = attacker.Descriptor.Stats.Initiative.ModifiedValue;
            Swap(attacker, set.Require(EasternWeaponFamily.Katana,
                EasternWeaponGenericKind.Mundane).Item, ref equipped);
            int initiativeAfter = attacker.Descriptor.Stats.Initiative.ModifiedValue;
            string wayfarer = initiativeBefore + "->" + initiativeEquipped +
                "->" + initiativeAfter;
            ElvenBranchedSpearCombatScenario.Add(assertions,
                "eastern-wayfarers-oath",
                "active exact weapon grants one +2 competence Initiative and switching removes it",
                wayfarer, initiativeEquipped == initiativeBefore + 2 &&
                    initiativeAfter == initiativeBefore,
                "live equipped fact and Initiative ModifiableValue");

            Swap(attacker, set.Named.Require(
                EasternWeaponNamedKind.FallingPetal).Item, ref equipped);
            EasternNamedWeaponEffectDiagnostics.Reset();
            attacker.Descriptor.Stats.BaseAttackBonus.BaseValue = 100;
            int acBefore = attacker.Descriptor.Stats.AC.ModifiedValue;
            int seed = ElvenBranchedSpearCombatScenario.FindNativeD20Seed(19);
            RuleAttackWithWeapon critical = ElvenBranchedSpearCombatScenario
                .NativeHitAttack(attacker, target, equipped, seed);
            int acCritical = attacker.Descriptor.Stats.AC.ModifiedValue;
            int applications = EasternNamedWeaponEffectDiagnostics
                .FallingPetalApplications;
            ElvenBranchedSpearCombatScenario.AutoHitAttack(attacker, target,
                equipped);
            int afterOrdinary = EasternNamedWeaponEffectDiagnostics
                .FallingPetalApplications;
            Swap(attacker, set.Require(EasternWeaponFamily.Wakizashi,
                EasternWeaponGenericKind.Mundane).Item, ref equipped);
            int acAfterSwap = attacker.Descriptor.Stats.AC.ModifiedValue;
            string falling = "confirmed=" + critical.AttackRoll
                .IsCriticalConfirmed + ";ac=" + acBefore + "->" +
                acCritical + "->" + acAfterSwap + ";applications=" +
                applications + "->" + afterOrdinary;
            ElvenBranchedSpearCombatScenario.Add(assertions,
                "eastern-falling-petal",
                "native confirmed critical grants one +1 Dodge for one round; ordinary hit and weapon swap do not retain it",
                falling, critical.AttackRoll.IsCriticalConfirmed &&
                    acCritical == acBefore + 1 && applications == 1 &&
                    afterOrdinary == 1 && acAfterSwap == acBefore,
                "native seeded critical confirmation, timed buff, and equipment callback");

            Swap(attacker, set.Named.Require(
                EasternWeaponNamedKind.MoonlitCrossing).Item, ref equipped);
            EasternNamedWeaponEffectDiagnostics.Reset();
            int moonlitAc = attacker.Descriptor.Stats.AC.ModifiedValue;
            RuleCalculateWeaponStats twoHand =
                ElvenBranchedSpearCombatScenario.WeaponStats(attacker, equipped);
            bool observedTwoHand = equipped.HoldInTwoHands;
            int twoApplications = EasternNamedWeaponEffectDiagnostics
                .MoonlitDamageApplications;
            offhand = EquipOffhand(attacker);
            EasternNamedWeaponEffectDiagnostics.Reset();
            int oneHandAc = attacker.Descriptor.Stats.AC.ModifiedValue;
            RuleCalculateWeaponStats oneHand =
                ElvenBranchedSpearCombatScenario.WeaponStats(attacker, equipped);
            bool observedOneHand = !equipped.HoldInTwoHands;
            int oneApplications = EasternNamedWeaponEffectDiagnostics
                .MoonlitDamageApplications;
            string moonlit = "grip=" + observedTwoHand + "/" +
                observedOneHand + ";ac=" + moonlitAc + "->" +
                oneHandAc + ";damageApplications=" + twoApplications + "/" +
                oneApplications;
            ElvenBranchedSpearCombatScenario.Add(assertions,
                "eastern-moonlit-crossing",
                "two-hand grip applies one +2 damage source only; one-hand grip applies one +1 Dodge source only",
                moonlit, observedTwoHand && observedOneHand &&
                    twoApplications == 1 && oneApplications == 0 &&
                    oneHandAc == moonlitAc + 1,
                "same native HoldInTwoHands authority, weapon-stat event, and AC stat");
            RemoveOffhand(attacker, ref offhand);

            Swap(attacker, set.Named.Require(
                EasternWeaponNamedKind.MountainSunder).Item, ref equipped);
            BlueprintFeature powerAttackFeature = BlueprintLibraryLookup
                .RequireExact<BlueprintFeature>(BlueprintBootstrap.Library,
                    EasternWeaponNamedBlueprints.PowerAttackFeatureGuid,
                    "native Power Attack feat");
            ElvenBranchedSpearCombatScenario.AddFact(attacker,
                powerAttackFeature, facts);
            powerAttack = attacker.Descriptor.ActivatableAbilities.Enumerable
                .Single(value => value != null && string.Equals(
                    value.Blueprint.AssetGuid,
                    EasternWeaponNamedBlueprints.PowerAttackToggleGuid,
                    StringComparison.Ordinal));
            powerAttack.IsOn = false;
            powerAttack.Stop(true);
            EasternNamedWeaponEffectDiagnostics.Reset();
            ElvenBranchedSpearCombatScenario.AutoHitAttack(attacker, target,
                equipped);
            int inactive = EasternNamedWeaponEffectDiagnostics
                .MountainSunderApplications;
            powerAttack.IsOn = true;
            ElvenBranchedSpearCombatScenario.AutoHitAttack(attacker, target,
                equipped);
            int first = EasternNamedWeaponEffectDiagnostics
                .MountainSunderApplications;
            int force = EasternNamedWeaponEffectDiagnostics
                .LastMountainSunderDamage;
            ElvenBranchedSpearCombatScenario.AutoHitAttack(attacker, target,
                equipped);
            int repeated = EasternNamedWeaponEffectDiagnostics
                .MountainSunderApplications;
            RemoveBuff(attacker, set.Named.Buffs.MountainSunderMarker);
            ElvenBranchedSpearCombatScenario.AutoHitAttack(attacker, target,
                equipped);
            int nextRound = EasternNamedWeaponEffectDiagnostics
                .MountainSunderApplications;
            string mountain = inactive + "->" + first + "->" + repeated +
                "->" + nextRound + ";force=" + force + ";running=" +
                powerAttack.IsRunning;
            ElvenBranchedSpearCombatScenario.Add(assertions,
                "eastern-mountain-sunder",
                "inactive Power Attack rejected; first hit applies one 1d6 force packet; repeat is blocked until marker reset",
                mountain, inactive == 0 && first == 1 && repeated == 1 &&
                    nextRound == 2 && force >= 1 && force <= 6 &&
                    powerAttack.IsRunning,
                "native Power Attack activatable, live attacks, damage rule, and one-round buff marker");
            powerAttack.IsOn = false;
            powerAttack.Stop(true);

            Swap(attacker, set.Named.Require(
                EasternWeaponNamedKind.UnfixedForm).Item, ref equipped);
            EasternNamedWeaponEffectDiagnostics.Reset();
            RuleCalculateWeaponStats ordinary =
                ElvenBranchedSpearCombatScenario.WeaponStats(attacker, equipped);
            int ordinaryApplications = EasternNamedWeaponEffectDiagnostics
                .UnfixedFormApplications;
            Size originalSize = attacker.Descriptor.State.Size;
            attacker.Descriptor.State.Size = originalSize == Size.Medium ?
                Size.Large : Size.Medium;
            RuleCalculateWeaponStats transformed =
                ElvenBranchedSpearCombatScenario.WeaponStats(attacker, equipped);
            int transformedApplications = EasternNamedWeaponEffectDiagnostics
                .UnfixedFormApplications;
            attacker.Descriptor.State.Size = originalSize;
            string unfixed = "applications=" + ordinaryApplications + "->" +
                transformedApplications + ";weaponSize=" +
                ordinary.WeaponSize + "->" + transformed.WeaponSize;
            ElvenBranchedSpearCombatScenario.Add(assertions,
                "eastern-unfixed-form",
                "ordinary size rejected; changed current size applies exactly one native weapon-size step",
                unfixed, ordinaryApplications == 0 &&
                    transformedApplications == 1 &&
                    (int)transformed.WeaponSize ==
                        (int)ordinary.WeaponSize + 1,
                "exact original/current Size state and RuleCalculateWeaponStats.IncreaseWeaponSize");
            diagnostics.Add("named{wayfarer=" + wayfarer + ";falling=" +
                falling + ";moonlit=" + moonlit + ";mountain=" + mountain +
                ";unfixed=" + unfixed + "}");
        }

        private static void QualifyCapstones(EasternWeaponBlueprintSet set,
            UnitEntityData attacker, ref ItemEntityWeapon equipped,
            ICollection<RuntimeTestAssertion> assertions,
            ICollection<string> diagnostics)
        {
            EasternWeaponNamedKind[] capstones = {
                EasternWeaponNamedKind.NightWithoutMoon,
                EasternWeaponNamedKind.HeavensMeasure,
                EasternWeaponNamedKind.WorldTreeSeverer };
            var observed = new List<string>();
            bool exact = true;
            foreach (EasternWeaponNamedKind kind in capstones)
            {
                EasternWeaponNamedBlueprintEntry entry = set.Named.Require(kind);
                Swap(attacker, entry.Item, ref equipped);
                BlueprintItemEnchantment[] enchantments =
                    entry.Item.Enchantments.ToArray();
                int effective = entry.Spec.NativeEffectiveBonus;
                bool hasSpeed = entry.Spec.Has(
                    EasternWeaponNativeProperty.Speed);
                int speedCount = enchantments.Count(value => value != null &&
                    string.Equals(value.AssetGuid,
                        EasternWeaponNamedBlueprints.SpeedGuid,
                        StringComparison.Ordinal));
                exact &= enchantments.All(value => value != null) &&
                    effective <= 10 && speedCount == (hasSpeed ? 1 : 0);
                observed.Add(kind + "=" + effective + "/speed:" + speedCount);
            }
            string text = string.Join("|", observed.ToArray());
            ElvenBranchedSpearCombatScenario.Add(assertions,
                "eastern-capstone-native-properties",
                "all capstones stay at or below +10 and each approved Speed reference occurs once",
                text, exact,
                "live equipped capstone enchantment arrays and effective-bonus catalog");
            diagnostics.Add("capstones{" + text + "}");
        }

        private static int Attack(UnitEntityData attacker,
            UnitEntityData target, ItemEntityWeapon weapon)
        {
            return ElvenBranchedSpearCombatScenario.WeaponAttack(attacker,
                target, weapon).AttackRoll.AttackBonus;
        }

        private static void Swap(UnitEntityData unit,
            BlueprintItemWeapon blueprint, ref ItemEntityWeapon equipped)
        {
            ElvenBranchedSpearCombatScenario.RemoveEquipped(unit,
                ref equipped);
            equipped = ElvenBranchedSpearCombatScenario.Equip(unit, blueprint);
        }

        private static ItemEntityWeapon EquipOffhand(UnitEntityData unit)
        {
            BlueprintItemWeapon blueprint = BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(BlueprintBootstrap.Library,
                    ShortswordItemGuid, "native Shortsword offhand control");
            var item = new ItemEntityWeapon(blueprint);
            unit.Body.SecondaryHand.InsertItem(item);
            if (!ReferenceEquals(unit.Body.SecondaryHand.MaybeWeapon, item))
                throw new InvalidOperationException(
                    "The offhand control did not remain equipped.");
            return item;
        }

        private static void RemoveOffhand(UnitEntityData unit,
            ref ItemEntityWeapon item)
        {
            if (unit != null && unit.Body != null &&
                unit.Body.SecondaryHand != null &&
                unit.Body.SecondaryHand.MaybeItem != null)
                unit.Body.SecondaryHand.RemoveItem(false);
            if (item != null) item.Dispose();
            item = null;
        }

        private static void RemoveBuff(UnitEntityData unit,
            Kingmaker.UnitLogic.Buffs.Blueprints.BlueprintBuff blueprint)
        {
            Buff buff = unit == null || unit.Descriptor == null ? null :
                unit.Descriptor.Buffs.GetBuff(blueprint);
            if (buff != null) unit.Descriptor.Buffs.RemoveFact(buff);
        }

    }
}
