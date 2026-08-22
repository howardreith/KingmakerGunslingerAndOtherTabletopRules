using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums.Damage;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.BodyguardFeats;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed class BodyguardArmorClassSourceEvidence
    {
        [JsonProperty("bonus", Order = 1)] public int Bonus { get; set; }
        [JsonProperty("sourceName", Order = 2)]
        public string SourceName { get; set; }
        [JsonProperty("sourceBlueprintGuid", Order = 3)]
        public string SourceBlueprintGuid { get; set; }
        [JsonProperty("sourceBlueprintName", Order = 4)]
        public string SourceBlueprintName { get; set; }
        [JsonProperty("sourceFactType", Order = 5)]
        public string SourceFactType { get; set; }
        [JsonProperty("sourceFactIdentity", Order = 6)]
        public int SourceFactIdentity { get; set; }
    }

    internal sealed class BodyguardCombatCaseEvidence
    {
        [JsonProperty("name", Order = 1)] public string Name { get; set; }
        [JsonProperty("attackIdentity", Order = 2)]
        public int AttackIdentity { get; set; }
        [JsonProperty("attackFamily", Order = 3)]
        public string AttackFamily { get; set; }
        [JsonProperty("attacker", Order = 4)] public string Attacker { get; set; }
        [JsonProperty("originalTarget", Order = 5)]
        public string OriginalTarget { get; set; }
        [JsonProperty("roll", Order = 6)] public int Roll { get; set; }
        [JsonProperty("attackBonus", Order = 7)]
        public int AttackBonus { get; set; }
        [JsonProperty("targetAc", Order = 8)] public int TargetAc { get; set; }
        [JsonProperty("hit", Order = 9)] public bool Hit { get; set; }
        [JsonProperty("critical", Order = 10)] public bool Critical { get; set; }
        [JsonProperty("attackPenalty", Order = 11)]
        public int AttackPenalty { get; set; }
        [JsonProperty("aooBefore", Order = 12)]
        public int[] AooBefore { get; set; }
        [JsonProperty("aooAfter", Order = 13)] public int[] AooAfter { get; set; }
        [JsonProperty("swiftBefore", Order = 14)]
        public float[] SwiftBefore { get; set; }
        [JsonProperty("swiftAfter", Order = 15)]
        public float[] SwiftAfter { get; set; }
        [JsonProperty("hpLoss", Order = 16)] public int[] HpLoss { get; set; }
        [JsonProperty("rollTargetRestored", Order = 17)]
        public bool RollTargetRestored { get; set; }
        [JsonProperty("weaponTargetRestored", Order = 18)]
        public bool WeaponTargetRestored { get; set; }
        [JsonProperty("aidControl", Order = 19)]
        public string AidControl { get; set; }
        [JsonProperty("runtimeCounters", Order = 20)]
        public string RuntimeCounters { get; set; }
        [JsonProperty("runtimeObservations", Order = 21)]
        public string[] RuntimeObservations { get; set; }
        [JsonProperty("damageEvents", Order = 22)]
        public string[] DamageEvents { get; set; }
        [JsonProperty("damageKinds", Order = 23)]
        public string[] DamageKinds { get; set; }
        [JsonProperty("rider", Order = 24)] public string Rider { get; set; }
        [JsonProperty("combatLogCount", Order = 25)]
        public long CombatLogCount { get; set; }
        [JsonProperty("combatLogLastMessage", Order = 26)]
        public string CombatLogLastMessage { get; set; }
        [JsonProperty("nativeAcBeforeBodyguard", Order = 27)]
        public int NativeAcBeforeBodyguard { get; set; }
        [JsonProperty("bodyguardContribution", Order = 28)]
        public int BodyguardContribution { get; set; }
        [JsonProperty("bodyguardSources", Order = 29)]
        public BodyguardArmorClassSourceEvidence[] BodyguardSources { get; set; }
    }

    internal sealed class BodyguardCombatFixture : IDisposable
    {
        internal static readonly Vector3 DefaultAttackerPosition =
            new Vector3(1.5f, 0f, 0f);
        private const string StandardLongspearGuid =
            "f28f6031c2908d84d945865a80f67177";
        private const string StandardHeavyCrossbowGuid =
            "19a5092244dcf99478dcd73c974828b1";
        private const BindingFlags Members = BindingFlags.Instance |
            BindingFlags.Public | BindingFlags.NonPublic;
        private readonly object _allUnits;
        private readonly object[] _unitsBefore;
        private readonly SceneEntitiesState _scene;
        private readonly BlueprintUnit _hostileSource;
        private readonly BlueprintFeature _riderFeature;
        private readonly BlueprintFeature _damageProbeFeature;
        private readonly BlueprintFeature _physicalDefense;
        private readonly BlueprintFeature _energyDefense;
        private readonly BlueprintItemWeapon _rangedBlueprint;
        private readonly ItemEntityWeapon _protectorOneWeapon;
        private readonly ItemEntityWeapon _protectorTwoWeapon;
        private readonly ItemEntityWeapon _meleeAttackerWeapon;
        private readonly ItemEntityWeapon _rangedAttackerWeapon;
        private readonly List<UnitEntityData> _units;
        private ItemEnchantment _flaming;
        private bool _ranged;
        private bool _disposed;

        internal BodyguardCombatFixture()
        {
            BodyguardFeatBlueprintSet set = BlueprintBootstrap.BodyguardFeats;
            if (set == null) throw new InvalidOperationException(
                "Bodyguard identities were not registered.");
            _allUnits = Game.Instance.State.Units.All;
            _unitsBefore = Snapshot(_allUnits);
            _units = new List<UnitEntityData>();
            try
            {
                _scene = new SceneEntitiesState(
                    "KMG_Bodyguard_InHarmsWay_Fixture");
                BlueprintUnit source = BlueprintRoot.Instance
                    .DefaultPlayerCharacter;
                Target = Game.Instance.EntityCreator.SpawnUnit(source,
                    Vector3.zero, Quaternion.identity, _scene);
                _units.Add(Target);
                ProtectorOne = Game.Instance.EntityCreator.SpawnUnit(source,
                    new Vector3(0f, 0f, 0.75f), Quaternion.identity, _scene);
                _units.Add(ProtectorOne);
                ProtectorTwo = Game.Instance.EntityCreator.SpawnUnit(source,
                    new Vector3(0f, 0f, -0.75f), Quaternion.identity, _scene);
                _units.Add(ProtectorTwo);
                BlueprintUnit hostile;
                Attacker = ElvenBranchedSpearCombatScenario
                    .SpawnHostileTarget(Target, source,
                        DefaultAttackerPosition, _scene, out hostile);
                _units.Add(Attacker);
                _hostileSource = hostile;
                if (_units.Any(value => value == null || value.View == null))
                    throw new InvalidOperationException(
                        "Bodyguard live-unit fixture is incomplete.");
                foreach (UnitEntityData unit in _units)
                {
                    if (!Game.Instance.State.Units.All.Add(unit))
                        throw new InvalidOperationException(
                            "Bodyguard fixture unit did not register exactly once.");
                    unit.Descriptor.Stats.HitPoints.BaseValue = 500;
                    unit.Descriptor.State.Immortality.Retain();
                    unit.CombatState.JoinCombat();
                    unit.CombatState.OnNewRound();
                }
                Target.Descriptor.Stats.Dexterity.BaseValue = 10;
                ProtectorOne.Descriptor.Stats.Strength.BaseValue = 18;
                ProtectorTwo.Descriptor.Stats.Strength.BaseValue = 18;
                ProtectorOne.Descriptor.Stats.BaseAttackBonus.BaseValue = 10;
                ProtectorTwo.Descriptor.Stats.BaseAttackBonus.BaseValue = 10;
                Attacker.Descriptor.Stats.Strength.BaseValue = 18;
                Attacker.Descriptor.Stats.BaseAttackBonus.BaseValue = 10;
                BlueprintItemWeapon longspear = BlueprintLibraryLookup
                    .RequireExact<BlueprintItemWeapon>(BlueprintBootstrap.Library,
                        StandardLongspearGuid, "native Standard Longspear");
                _protectorOneWeapon = ElvenBranchedSpearCombatScenario.Equip(
                    ProtectorOne, longspear);
                _protectorTwoWeapon = ElvenBranchedSpearCombatScenario.Equip(
                    ProtectorTwo, longspear);
                _meleeAttackerWeapon = ElvenBranchedSpearCombatScenario.Equip(
                    Attacker, longspear);
                BlueprintItemWeapon nativeRanged = BlueprintLibraryLookup
                    .RequireExact<BlueprintItemWeapon>(BlueprintBootstrap.Library,
                        StandardHeavyCrossbowGuid,
                        "native Standard Heavy Crossbow");
                _rangedBlueprint = CreateSynchronousRangedClone(nativeRanged);
                _rangedAttackerWeapon = new ItemEntityWeapon(_rangedBlueprint);

                ProtectorOne.Descriptor.AddFact(set.CombatReflexes);
                ProtectorOne.Descriptor.AddFact(set.Bodyguard);
                ProtectorOne.Descriptor.AddFact(set.InHarmsWay);
                ProtectorTwo.Descriptor.AddFact(set.CombatReflexes);
                ProtectorTwo.Descriptor.AddFact(set.Bodyguard);
                ProtectorTwo.Descriptor.AddFact(set.InHarmsWay);
                PrepareNativeOpportunityState(ProtectorOne, Attacker);
                PrepareNativeOpportunityState(ProtectorTwo, Attacker);

                RiderBuff = CreateBuff("KMG_Runtime_Bodyguard_Rider");
                var rider = ScriptableObject.CreateInstance<
                    BodyguardQualificationRiderComponent>();
                rider.name = "$KMG_Runtime_Bodyguard_Rider_Component";
                rider.Rider = RiderBuff;
                _riderFeature = CreateFeature(
                    "KMG_Runtime_Bodyguard_Rider_Feature", rider);
                Attacker.Descriptor.AddFact(_riderFeature);
                var damageProbe = ScriptableObject.CreateInstance<
                    BodyguardQualificationDamageProbe>();
                damageProbe.name = "$KMG_Runtime_Bodyguard_Damage_Probe";
                _damageProbeFeature = CreateFeature(
                    "KMG_Runtime_Bodyguard_Damage_Probe_Feature", damageProbe);
                Target.Descriptor.AddFact(_damageProbeFeature);
                ProtectorOne.Descriptor.AddFact(_damageProbeFeature);
                ProtectorTwo.Descriptor.AddFact(_damageProbeFeature);

                var dr = ScriptableObject.CreateInstance<
                    AddDamageResistancePhysical>();
                dr.Value = new ContextValue {
                    ValueType = ContextValueType.Simple, Value = 100 };
                _physicalDefense = CreateFeature(
                    "KMG_Runtime_Bodyguard_DR100", dr);
                var immunity = ScriptableObject.CreateInstance<
                    AddEnergyImmunity>();
                immunity.Type = DamageEnergyType.Fire;
                _energyDefense = CreateFeature(
                    "KMG_Runtime_Bodyguard_Fire_Immunity", immunity);
                if (!Target.IsAlly(ProtectorOne) ||
                    !Target.IsAlly(ProtectorTwo) || !Target.IsEnemy(Attacker))
                    throw new InvalidOperationException(
                        "Bodyguard fixture relationships are not ally/ally/hostile.");
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        internal UnitEntityData Target { get; private set; }
        internal UnitEntityData ProtectorOne { get; private set; }
        internal UnitEntityData ProtectorTwo { get; private set; }
        internal UnitEntityData Attacker { get; private set; }
        internal BlueprintBuff RiderBuff { get; private set; }
        internal UnitEntityData[] Protectors
        { get { return new[] { ProtectorOne, ProtectorTwo }; } }
        internal bool Cleaned { get; private set; }

        internal void ResetEconomy(int firstAoo, float firstSwift,
            int secondAoo, float secondSwift)
        {
            ProtectorOne.CombatState.AttackOfOpportunityCount = firstAoo;
            ProtectorOne.CombatState.Cooldown.SwiftAction = firstSwift;
            ProtectorTwo.CombatState.AttackOfOpportunityCount = secondAoo;
            ProtectorTwo.CombatState.Cooldown.SwiftAction = secondSwift;
        }

        internal void ClearModes()
        {
            SetModes(ProtectorOne, false, false);
            SetModes(ProtectorTwo, false, false);
        }

        internal void SetModes(UnitEntityData unit, bool bodyguard,
            bool inHarmsWay)
        {
            SetMode(unit, BlueprintBootstrap.BodyguardFeats.Modes
                .BodyguardMarker, bodyguard);
            SetMode(unit, BlueprintBootstrap.BodyguardFeats.Modes
                .InHarmsWayMarker, inHarmsWay);
        }

        internal void SetAttackerPosition(Vector3 position)
        { SetPosition(Attacker, position); }

        internal void UseSynchronousRangedAttacker(bool ranged)
        { _ranged = ranged; }

        internal void AddFlaming()
        {
            if (_flaming != null) return;
            BlueprintWeaponEnchantment flaming = BlueprintLibraryLookup
                .RequireExact<BlueprintWeaponEnchantment>(
                    BlueprintBootstrap.Library,
                    EasternWeaponNamedBlueprints.FlamingGuid,
                    "native Flaming weapon enchantment");
            _flaming = _meleeAttackerWeapon.AddEnchantment(flaming, null, null);
            if (_flaming == null) throw new InvalidOperationException(
                "The request-local attacker rejected Flaming.");
        }

        internal void RemoveFlaming()
        {
            if (_flaming == null) return;
            _meleeAttackerWeapon.RemoveEnchantment(_flaming);
            _flaming = null;
        }

        internal void AddCompleteDefense(UnitEntityData unit)
        {
            unit.Descriptor.AddFact(_physicalDefense);
            unit.Descriptor.AddFact(_energyDefense);
        }

        internal void RemoveCompleteDefense(UnitEntityData unit)
        {
            unit.Descriptor.RemoveFact(_physicalDefense);
            unit.Descriptor.RemoveFact(_energyDefense);
        }

        internal void ApplyShieldOther(UnitEntityData subject,
            UnitEntityData caster)
        {
            var context = new MechanicsContext(caster, caster.Descriptor,
                BlueprintBootstrap.ShieldOther.Ability, null,
                new TargetWrapper(subject));
            context.Params.CasterLevel = 5;
            Buff link = subject.Descriptor.Buffs.AddBuff(
                BlueprintBootstrap.ShieldOther.TargetBuff, context,
                TimeSpan.FromHours(5));
            if (link == null) throw new InvalidOperationException(
                "The request-local Shield Other link was rejected.");
        }

        internal void RemoveShieldOther()
        {
            foreach (UnitEntityData unit in _units)
                foreach (Buff buff in unit.Descriptor.Buffs.RawFacts
                    .OfType<Buff>().Where(value => ReferenceEquals(
                        value.Blueprint, BlueprintBootstrap.ShieldOther
                            .TargetBuff)).ToArray())
                    buff.Remove();
        }

        internal bool HasRider(UnitEntityData unit)
        { return unit.Descriptor.HasFact(RiderBuff); }

        internal void RemoveRiders()
        {
            foreach (UnitEntityData unit in _units)
                foreach (Buff buff in unit.Descriptor.Buffs.RawFacts
                    .OfType<Buff>().Where(value => ReferenceEquals(
                        value.Blueprint, RiderBuff)).ToArray())
                    buff.Remove();
        }

        internal string PublicationCounts()
        {
            BodyguardFeatBlueprintSet set = BlueprintBootstrap.BodyguardFeats;
            var basic = BlueprintLibraryLookup.RequireExact<
                BlueprintFeatureSelection>(BlueprintBootstrap.Library,
                    BodyguardFeatCatalogPublication.BasicFeatSelectionGuid,
                    "native basic feat selection");
            var fighter = BlueprintLibraryLookup.RequireExact<
                BlueprintFeatureSelection>(BlueprintBootstrap.Library,
                    BodyguardFeatCatalogPublication
                        .FighterCombatFeatSelectionGuid,
                    "native Fighter feat selection");
            return "basic.Features=" + Count(basic.Features, set) +
                ";basic.AllFeatures=" + Count(basic.AllFeatures, set) +
                ";fighter.Features=" + Count(fighter.Features, set) +
                ";fighter.AllFeatures=" + Count(fighter.AllFeatures, set);
        }

        internal BodyguardCombatCaseEvidence Attack(string name,
            int incomingRoll, int attackPenalty, bool critical, bool rider,
            params int[] aidRolls)
        {
            BodyguardRuntime.ClearAll("qualification-case-start");
            BodyguardRuntimeDiagnostics.Reset();
            BodyguardQualificationRiderComponent.Reset(rider);
            BodyguardQualificationDamageProbe.Reset(true);
            RemoveRiders();
            foreach (UnitEntityData unit in _units)
                unit.Descriptor.Damage = 0;
            int[] aooBefore = Protectors.Select(value => value.CombatState
                .AttackOfOpportunityCount).ToArray();
            float[] swiftBefore = Protectors.Select(value => value.CombatState
                .Cooldown.SwiftAction).ToArray();
            long combatLogsBefore = BodyguardCombatLog.Published;
            int[] hpBefore = new[] { Target, ProtectorOne, ProtectorTwo }
                .Select(value => value.HPLeft).ToArray();
            RuleAttackWithWeapon attack = null;
            string control;
            BodyguardQualificationControl.Arm(incomingRoll, aidRolls);
            try
            {
                ItemEntityWeapon weapon = _ranged ? _rangedAttackerWeapon :
                    _meleeAttackerWeapon;
                attack = new RuleAttackWithWeapon(Attacker, Target, weapon,
                    attackPenalty) {
                    AutoCriticalThreat = critical,
                    AutoCriticalConfirmation = critical,
                    Maximized = true
                };
                Rulebook.Trigger(attack);
            }
            finally
            { control = BodyguardQualificationControl.DescribeAndClear(); }
            if (attack == null || attack.AttackRoll == null)
                throw new InvalidOperationException(
                    "Native attack did not expose RuleAttackRoll.");
            BodyguardArmorClassSourceEvidence[] bodyguardSources =
                ReadBodyguardArmorClassSources(attack.AttackRoll);
            int bodyguardContribution = bodyguardSources.Sum(value =>
                value.Bonus);
            int[] hpAfter = new[] { Target, ProtectorOne, ProtectorTwo }
                .Select(value => value.HPLeft).ToArray();
            string counters = "frames=" + BodyguardRuntimeDiagnostics.Frames +
                ";attempts=" + BodyguardRuntimeDiagnostics.Attempts +
                ";successful=" + BodyguardRuntimeDiagnostics.SuccessfulAttempts +
                ";interceptions=" + BodyguardRuntimeDiagnostics.Interceptions +
                ";faults=" + BodyguardRuntimeDiagnostics.Faults +
                ";duplicates=" + BodyguardRuntimeDiagnostics.DuplicateCallbacks +
                ";completed=" + BodyguardRuntimeDiagnostics.Completed;
            return new BodyguardCombatCaseEvidence {
                Name = name,
                AttackIdentity = System.Runtime.CompilerServices.RuntimeHelpers
                    .GetHashCode(attack.AttackRoll),
                AttackFamily = attack.Weapon.Blueprint.IsRanged ?
                    "ranged-weapon" : "melee-weapon",
                Attacker = Attacker.UniqueId,
                OriginalTarget = Target.UniqueId,
                Roll = attack.AttackRoll.Roll,
                AttackBonus = attack.AttackRoll.AttackBonus,
                TargetAc = attack.AttackRoll.TargetAC,
                Hit = attack.AttackRoll.IsHit,
                Critical = attack.AttackRoll.IsCriticalConfirmed,
                AttackPenalty = attackPenalty,
                AooBefore = aooBefore,
                AooAfter = Protectors.Select(value => value.CombatState
                    .AttackOfOpportunityCount).ToArray(),
                SwiftBefore = swiftBefore,
                SwiftAfter = Protectors.Select(value => value.CombatState
                    .Cooldown.SwiftAction).ToArray(),
                HpLoss = hpBefore.Zip(hpAfter, (before, after) => before - after)
                    .ToArray(),
                RollTargetRestored = ReferenceEquals(attack.AttackRoll.Target,
                    Target),
                WeaponTargetRestored = ReferenceEquals(attack.Target, Target),
                AidControl = control,
                RuntimeCounters = counters,
                RuntimeObservations = BodyguardRuntimeDiagnostics
                    .SnapshotObservations(),
                DamageEvents = BodyguardQualificationDamageProbe.Snapshot(),
                DamageKinds = attack.MeleeDamage == null ||
                    attack.MeleeDamage.ResultDamage == null ? new string[0] :
                    attack.MeleeDamage.ResultDamage.Select(value =>
                        value.Source == null ? "<null>" :
                            value.Source.GetType().FullName + ":" +
                            value.FinalValue).ToArray(),
                Rider = BodyguardQualificationRiderComponent.Describe(),
                CombatLogCount = BodyguardCombatLog.Published - combatLogsBefore,
                CombatLogLastMessage = BodyguardCombatLog.Published ==
                    combatLogsBefore ? string.Empty :
                    BodyguardCombatLog.LastMessage ?? string.Empty,
                NativeAcBeforeBodyguard = checked(
                    attack.AttackRoll.TargetAC - bodyguardContribution),
                BodyguardContribution = bodyguardContribution,
                BodyguardSources = bodyguardSources
            };
        }

        private static BodyguardArmorClassSourceEvidence[]
            ReadBodyguardArmorClassSources(RuleAttackRoll roll)
        {
            BodyguardFeatBlueprintSet set = BlueprintBootstrap.BodyguardFeats;
            if (roll == null || roll.ACRule == null ||
                roll.ACRule.BonusSources == null || set == null)
                throw new InvalidOperationException(
                    "Native Bodyguard AC source evidence is unavailable.");
            return roll.ACRule.BonusSources.Where(value =>
                value.Source != null && value.Source.Blueprint != null &&
                (ReferenceEquals(value.Source.Blueprint, set.Bodyguard) ||
                    string.Equals(value.Source.Blueprint.AssetGuid,
                        set.Bodyguard.AssetGuid, StringComparison.Ordinal)))
                .Select(value => DescribeBodyguardArmorClassSource(value))
                .ToArray();
        }

        private static BodyguardArmorClassSourceEvidence
            DescribeBodyguardArmorClassSource(BonusSource value)
        {
            Fact source = value.Source;
            return new BodyguardArmorClassSourceEvidence {
                Bonus = value.Bonus,
                SourceName = source.Name ?? string.Empty,
                SourceBlueprintGuid = source.Blueprint.AssetGuid,
                SourceBlueprintName = source.Blueprint.name ?? string.Empty,
                SourceFactType = source.GetType().FullName,
                SourceFactIdentity = System.Runtime.CompilerServices
                    .RuntimeHelpers.GetHashCode(source)
            };
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            RemoveFlaming();
            RemoveShieldOther();
            RemoveRiders();
            BodyguardRuntime.ClearAll("qualification-fixture-dispose");
            RemoveMemory(Attacker, ProtectorOne);
            RemoveMemory(Attacker, ProtectorTwo);
            if (Attacker != null && Attacker.Body.PrimaryHand.MaybeItem != null)
                Attacker.Body.PrimaryHand.RemoveItem(false);
            if (_rangedAttackerWeapon != null) _rangedAttackerWeapon.Dispose();
            if (_meleeAttackerWeapon != null) _meleeAttackerWeapon.Dispose();
            if (ProtectorOne != null &&
                ProtectorOne.Body.PrimaryHand.MaybeItem != null)
                ProtectorOne.Body.PrimaryHand.RemoveItem(false);
            if (_protectorOneWeapon != null) _protectorOneWeapon.Dispose();
            if (ProtectorTwo != null &&
                ProtectorTwo.Body.PrimaryHand.MaybeItem != null)
                ProtectorTwo.Body.PrimaryHand.RemoveItem(false);
            if (_protectorTwoWeapon != null) _protectorTwoWeapon.Dispose();
            foreach (UnitEntityData unit in (_units ??
                new List<UnitEntityData>()).Where(value => value != null)
                    .AsEnumerable().Reverse())
                Game.Instance.State.Units.All.Remove(unit);
            foreach (UnitEntityData unit in (_units ??
                new List<UnitEntityData>()).Where(value => value != null)
                    .AsEnumerable().Reverse())
            {
                if (unit.CombatState.IsInCombat) unit.CombatState.LeaveCombat();
                unit.Descriptor.State.Immortality.ReleaseAll();
                unit.Dispose();
            }
            if (_scene != null) _scene.Dispose();
            if (_hostileSource != null)
                UnityEngine.Object.DestroyImmediate(_hostileSource);
            DestroyBlueprint(_riderFeature);
            DestroyBlueprint(_damageProbeFeature);
            DestroyBlueprint(_physicalDefense);
            DestroyBlueprint(_energyDefense);
            if (RiderBuff != null)
                UnityEngine.Object.DestroyImmediate(RiderBuff);
            if (_rangedBlueprint != null)
                UnityEngine.Object.DestroyImmediate(_rangedBlueprint);
            Cleaned = Same(_unitsBefore, Snapshot(_allUnits));
        }

        private static void SetMode(UnitEntityData unit, BlueprintBuff marker,
            bool active)
        {
            foreach (Buff buff in unit.Descriptor.Buffs.RawFacts.OfType<Buff>()
                .Where(value => ReferenceEquals(value.Blueprint, marker))
                .ToArray()) buff.Remove();
            if (!active) return;
            var context = new MechanicsContext(unit, unit.Descriptor,
                BlueprintBootstrap.BodyguardFeats.Bodyguard, null,
                new TargetWrapper(unit));
            if (unit.Descriptor.Buffs.AddBuff(marker, context, null) == null)
                throw new InvalidOperationException(
                    "Bodyguard request-local mode marker was rejected.");
        }

        private static BlueprintItemWeapon CreateSynchronousRangedClone(
            BlueprintItemWeapon source)
        {
            BlueprintItemWeapon clone = UnityEngine.Object.Instantiate(source);
            try
            {
                clone.name = "KMG_Runtime_Synchronous_HeavyCrossbow";
                var visual = new WeaponVisualParameters();
                // Projectiles inherits from Prototype when this array is
                // empty. Keep Prototype null so RuleAttackWithWeapon follows
                // its native zero-projectile synchronous resolve branch.
                typeof(WeaponVisualParameters).GetField("m_Projectiles",
                    Members).SetValue(visual, new BlueprintProjectile[0]);
                typeof(Kingmaker.Blueprints.Items.Equipment
                    .BlueprintItemEquipmentHand).GetField("m_VisualParameters",
                        Members).SetValue(clone, visual);
                if (!clone.IsRanged || clone.VisualParameters.Projectiles ==
                    null || clone.VisualParameters.Projectiles.Length != 0)
                    throw new InvalidOperationException(
                        "Synchronous ranged clone contract failed.");
                return clone;
            }
            catch
            {
                UnityEngine.Object.DestroyImmediate(clone);
                throw;
            }
        }

        private static BlueprintBuff CreateBuff(string name)
        {
            var buff = ScriptableObject.CreateInstance<BlueprintBuff>();
            buff.name = name;
            buff.ComponentsArray = new BlueprintComponent[0];
            return buff;
        }

        private static BlueprintFeature CreateFeature(string name,
            params BlueprintComponent[] components)
        {
            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = name;
            feature.Ranks = 1;
            feature.ComponentsArray = components;
            return feature;
        }

        private static void DestroyBlueprint(BlueprintScriptableObject value)
        {
            if (value == null) return;
            foreach (BlueprintComponent component in value.ComponentsArray ??
                new BlueprintComponent[0])
                if (component != null)
                    UnityEngine.Object.DestroyImmediate(component);
            UnityEngine.Object.DestroyImmediate(value);
        }

        private static int Count(BlueprintFeature[] values,
            BodyguardFeatBlueprintSet set)
        {
            return (values ?? new BlueprintFeature[0]).Count(value =>
                value != null && (ReferenceEquals(value, set.Bodyguard) ||
                ReferenceEquals(value, set.InHarmsWay) || string.Equals(
                    value.AssetGuid, set.Bodyguard.AssetGuid,
                    StringComparison.Ordinal) || string.Equals(value.AssetGuid,
                    set.InHarmsWay.AssetGuid, StringComparison.Ordinal)));
        }

        private static void SetPosition(UnitEntityData unit, Vector3 position)
        {
            typeof(UnitEntityData).GetProperty("Position", Members)
                .SetValue(unit, position, null);
            if (unit.View != null) unit.View.transform.position = position;
        }

        private static void PrepareNativeOpportunityState(
            UnitEntityData protector, UnitEntityData attacker)
        {
            protector.CombatState.OnNewRound();
            protector.LastMoveTime = Game.Instance.TimeController.GameTime -
                TimeSpan.FromSeconds(1d);
            protector.PreviousPosition = protector.Position;
            attacker.Memory.Add(protector);
        }

        private static void RemoveMemory(UnitEntityData owner,
            UnitEntityData remembered)
        {
            if (owner != null && remembered != null && owner.Memory != null &&
                owner.Memory.Contains(remembered)) owner.Memory.Remove(remembered);
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
    }
}
