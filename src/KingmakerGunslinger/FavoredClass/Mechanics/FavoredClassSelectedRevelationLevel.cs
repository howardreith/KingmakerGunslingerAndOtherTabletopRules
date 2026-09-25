using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;

namespace KingmakerGunslinger.FavoredClass.Mechanics
{
    /// <summary>
    /// I06/S04: the owner's earned steps raise the effective oracle level of
    /// exactly one chosen revelation, only where the revelation itself reads
    /// oracle level (see FavoredClassRevelationScopes for the audited read
    /// points). Here: the caster level and half-level spell level that Call of
    /// the Wild's Oracle engine computes for the revelation's own abilities
    /// (so their save DCs and caster-level-based durations and dice follow),
    /// and the native maximum of its own resources at the effective level.
    /// Its class-level ranks are raised by the rank hook and its own level
    /// gates by the gate hook. After a rank change it refreshes the
    /// revelation's persistent feature contexts and re-decides its gates;
    /// spell slots, other revelations, revelation choices, BAB and saves never
    /// change.
    /// </summary>
    public sealed class FavoredClassSelectedRevelationLevel : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>,
        IResourceAmountBonusHandler, IUnitSubscriber
    {
        /// <summary>The manifest key of the chosen revelation.</summary>
        public string TargetKey;

        public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            if (evt == null || !evt.ReplaceCasterLevel.HasValue || Owner == null)
                return;
            FavoredClassRevelationScope scope = FavoredClassRevelationScopes.ForKey(TargetKey);
            if (scope == null || evt.Blueprint == null || !scope.ParamsAbilities.Contains(evt.Blueprint))
                return;
            int earned = scope.EarnedSteps(Owner);
            if (earned <= 0)
                return;
            int level = evt.ReplaceCasterLevel.Value;
            evt.ReplaceCasterLevel = level + earned;
            if (evt.ReplaceSpellLevel.HasValue)
                evt.ReplaceSpellLevel = evt.ReplaceSpellLevel.Value +
                    FavoredClassMechanicsPolicy.HalfLevelDelta(level, earned);
        }

        public override void OnEventDidTrigger(RuleCalculateAbilityParams evt) { }

        public void CalculateMaxResourceAmount(BlueprintAbilityResource resource, ref int bonus)
        {
            Fact fact = Fact;
            if (fact == null || !fact.Active || resource == null || Owner == null)
                return;
            FavoredClassRevelationScope scope = FavoredClassRevelationScopes.ForKey(TargetKey);
            if (scope == null || !scope.Resources.ContainsKey(resource))
                return;
            int earned = scope.EarnedSteps(Owner);
            if (earned > 0)
                bonus += scope.ResourceDelta(resource, Owner, earned);
        }

        public override void OnTurnOn()
        {
            Refresh();
        }

        public override void OnFactDeactivate()
        {
            // A rank change deactivates and reactivates the leaf (refreshed on
            // turn-on); only a real removal refreshes without this leaf.
            if (IsReapplying)
                return;
            Fact fact = Fact;
            FavoredClassRevelationScopes.BeginDeparture(fact);
            try
            {
                Refresh();
            }
            finally
            {
                FavoredClassRevelationScopes.EndDeparture(fact);
            }
        }

        /// <summary>
        /// Recalculates the owner's revelation features whose contexts hold a
        /// scaled rank, and re-decides the revelation's own level gates (the
        /// native gate adds or removes its feature at the effective level).
        /// </summary>
        internal void Refresh()
        {
            FavoredClassRevelationScope scope = FavoredClassRevelationScopes.ForKey(TargetKey);
            if (scope == null || Owner == null ||
                scope.RefreshFeatures.Count == 0 && scope.GateOwners.Count == 0)
                return;
            UnitDescriptor owner = Owner;
            foreach (Feature feature in owner.Progression.Features.Enumerable.ToArray())
            {
                if (feature == null)
                    continue;
                if (scope.RefreshFeatures.Contains(feature.Blueprint))
                    feature.Recalculate();
                if (scope.GateOwners.Contains(feature.Blueprint))
                    feature.CallComponents<AddFeatureOnClassLevel>(gate => gate.HandleUnitGainLevel(owner, null));
            }
        }
    }
}
