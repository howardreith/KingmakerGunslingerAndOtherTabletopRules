using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;

namespace KingmakerGunslinger.FavoredClass.Mechanics
{
    /// <summary>
    /// I08/S06: the owner's earned steps raise the effective sorcerer level of
    /// exactly one chosen bloodline power's ability (and its variants), for
    /// that ability's own level-scaled values only. In the native parameter
    /// rule it adds the steps as bonus caster level, which the game also
    /// passes to every class-level rank of the ability (damage dice and
    /// damage bonus) and to its caster-level checks; where the power binds its
    /// DC to half its class level, it adds exactly the resulting difference.
    /// The power's own use thresholds (Elemental Blast's extra daily uses at
    /// 17th and 20th level, granted natively by the bloodline progression)
    /// follow the effective level of that progression. Other powers, spells,
    /// spell slots, general caster level, BAB and saves are unchanged, and no
    /// power or bloodline feature is granted early.
    /// </summary>
    public sealed class FavoredClassSelectedPowerLevel : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>,
        IResourceAmountBonusHandler, IUnitSubscriber
    {
        public BlueprintAbility Ability;
        public BlueprintFeature PowerFeature;
        public int Divisor = 6;

        /// <summary>Printed cap in steps; zero means uncapped.</summary>
        public int CapSteps;

        /// <summary>The power's own use resource (its ability's required resource).</summary>
        public BlueprintAbilityResource UsesResource;

        /// <summary>The eligible bloodline progressions whose own use thresholds the power follows.</summary>
        public string[] BloodlineGuids;

        private static readonly Dictionary<string, List<KeyValuePair<int, int>>> Thresholds =
            new Dictionary<string, List<KeyValuePair<int, int>>>();

        public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            Fact fact = Fact;
            if (evt == null || fact == null || !fact.Active || Owner == null || Ability == null ||
                !FavoredClassRuntime.MechanicsEnabled || !IsChosenAbility(evt.Spell))
                return;
            int earned = Earned(fact);
            if (earned <= 0)
                return;
            evt.AddBonusCasterLevel(earned);
            BindAbilitiesToClass binding = Binding();
            if (binding != null && !binding.Cantrip)
            {
                int level = binding.GetLevel(Owner);
                int delta = FavoredClassMechanicsPolicy.HalfLevelDelta(level, earned);
                if (delta != 0)
                    evt.AddBonusDC(delta);
            }
        }

        public override void OnEventDidTrigger(RuleCalculateAbilityParams evt) { }

        /// <summary>
        /// The power's use thresholds between the real and the effective
        /// level of the owner's eligible bloodline progression (the real ones
        /// are already granted natively); raising a maximum never refills.
        /// </summary>
        public void CalculateMaxResourceAmount(BlueprintAbilityResource resource, ref int bonus)
        {
            Fact fact = Fact;
            if (fact == null || !fact.Active || Owner == null || UsesResource == null || resource != UsesResource ||
                BloodlineGuids == null || PowerFeature == null || !FavoredClassRuntime.MechanicsEnabled ||
                !Owner.HasFact(PowerFeature))
                return;
            int earned = Earned(fact);
            if (earned <= 0)
                return;
            LibraryScriptableObject library = ResourcesLibrary.LibraryObject;
            if (library == null || library.BlueprintsByAssetId == null)
                return;
            foreach (string guid in BloodlineGuids)
            {
                BlueprintScriptableObject found;
                var progression = guid != null && library.BlueprintsByAssetId.TryGetValue(guid, out found)
                    ? found as BlueprintProgression : null;
                if (progression == null || !Owner.Progression.Features.HasFact(progression))
                    continue;
                int real = progression.CalcLevel(Owner);
                bonus += FavoredClassMechanicsPolicy.ThresholdUsesBetween(UseThresholds(progression, UsesResource),
                    real, real + earned);
            }
        }

        internal bool IsChosenAbility(BlueprintAbility spell)
        {
            return spell != null && (spell == Ability || (spell.Parent != null && spell.Parent == Ability));
        }

        /// <summary>The power feature's own DC binding for this ability, if any.</summary>
        internal BindAbilitiesToClass Binding()
        {
            if (PowerFeature == null || PowerFeature.ComponentsArray == null)
                return null;
            return PowerFeature.ComponentsArray.OfType<BindAbilitiesToClass>().FirstOrDefault(value =>
                value.Abilites != null && value.Abilites.Contains(Ability));
        }

        /// <summary>
        /// The progression's own (level, uses) entries for the resource: the
        /// level entries whose features increase that resource natively.
        /// </summary>
        internal static IList<KeyValuePair<int, int>> UseThresholds(BlueprintProgression progression,
            BlueprintAbilityResource resource)
        {
            string key = progression.AssetGuid + "|" + resource.AssetGuid;
            List<KeyValuePair<int, int>> thresholds;
            lock (Thresholds)
            {
                if (Thresholds.TryGetValue(key, out thresholds))
                    return thresholds;
                thresholds = new List<KeyValuePair<int, int>>();
                foreach (LevelEntry entry in progression.LevelEntries ?? new LevelEntry[0])
                    foreach (BlueprintFeature feature in (entry.Features ?? new List<BlueprintFeatureBase>())
                        .OfType<BlueprintFeature>())
                        foreach (IncreaseResourceAmount increase in (feature.ComponentsArray ?? new BlueprintComponent[0])
                            .OfType<IncreaseResourceAmount>())
                            if (increase.Resource == resource && increase.Value > 0)
                                thresholds.Add(new KeyValuePair<int, int>(entry.Level, increase.Value));
                Thresholds[key] = thresholds;
                return thresholds;
            }
        }

        private int Earned(Fact fact)
        {
            return FavoredClassRankPolicy.BenefitSteps(
                new FavoredClassRate(Divisor, CapSteps > 0 ? CapSteps : (int?)null), fact.GetRank());
        }
    }
}
