using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.UnitLogic;

namespace KingmakerGunslinger.FavoredClass.Mechanics
{
    /// <summary>
    /// I08/S06 Elemental Resistance (charter 8.10): the chosen power's own
    /// level gates (resistance 10 before 9th level, 20 from 9th) decide at the
    /// owner's class level for that gate plus this counter's earned steps, at
    /// most two. Only the gates the owned power feature itself holds move; the
    /// power is never granted early, and no other power, spell, BAB or save
    /// changes. After a rank change, and on a real removal, the power's gates
    /// are re-decided.
    /// </summary>
    [AllowedOn(typeof(BlueprintUnitFact))]
    public sealed class FavoredClassSelectedPowerGates : OwnedGameLogicComponent<UnitDescriptor>
    {
        /// <summary>The owned bloodline power feature whose own gates move.</summary>
        public BlueprintFeature PowerFeature;

        /// <summary>The counter's full leaf (its rank is the owner's investment).</summary>
        public BlueprintFeature Leaf;

        public string TargetKey;
        public int Divisor = 6;
        public int CapSteps;

        private static readonly object Gate = new object();
        private static readonly Dictionary<BlueprintScriptableObject, FavoredClassSelectedPowerGates> ByPower =
            new Dictionary<BlueprintScriptableObject, FavoredClassSelectedPowerGates>();

        /// <summary>Indexes one power's counter (once per blueprint creation).</summary>
        internal static void Register(FavoredClassSelectedPowerGates component)
        {
            if (component == null || component.PowerFeature == null || component.Leaf == null)
                return;
            lock (Gate)
                ByPower[component.PowerFeature] = component;
        }

        /// <summary>
        /// The gate hook: a gate that the owned, invested power feature itself
        /// holds decides at the effective level; every other gate keeps the
        /// native decision.
        /// </summary>
        internal static void GateResult(AddFeatureOnClassLevel gate, ref bool result)
        {
            if (gate == null)
                return;
            Fact fact = gate.Fact;
            if (fact == null || fact.Blueprint == null)
                return;
            FavoredClassSelectedPowerGates registration;
            lock (Gate)
                if (!ByPower.TryGetValue(fact.Blueprint, out registration))
                    return;
            UnitDescriptor owner = gate.Owner;
            int steps = registration.EarnedSteps(owner);
            if (steps <= 0)
                return;
            int level = ReplaceCasterLevelOfAbility.CalculateClassLevel(gate.Class, gate.AdditionalClasses, owner,
                gate.Archetypes);
            result = FavoredClassMechanicsPolicy.GateApplies(level + steps, gate.Level, gate.BeforeThisLevel);
        }

        /// <summary>The owner's earned steps in this counter (0 while it is being removed or inert).</summary>
        internal int EarnedSteps(UnitDescriptor owner)
        {
            if (owner == null || owner.Progression == null || Leaf == null || !FavoredClassRuntime.MechanicsEnabled ||
                FavoredClassRuntime.IsEffectUnavailable(FavoredClassCatalog.EffectSelectedBloodlinePower) ||
                FavoredClassRuntime.IsTargetUnavailable(FavoredClassCatalog.EffectSelectedBloodlinePower, TargetKey))
                return 0;
            Fact leaf = owner.Progression.Features.GetFact(Leaf);
            if (leaf == null || FavoredClassRevelationScopes.IsDeparting(leaf))
                return 0;
            return FavoredClassRankPolicy.BenefitSteps(
                new FavoredClassRate(Divisor, CapSteps > 0 ? CapSteps : (int?)null), leaf.GetRank());
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

        /// <summary>Re-decides the owned power feature's own gates.</summary>
        internal void Refresh()
        {
            UnitDescriptor owner = Owner;
            if (owner == null || owner.Progression == null || PowerFeature == null)
                return;
            Fact power = owner.Progression.Features.GetFact(PowerFeature);
            if (power == null)
                return;
            power.CallComponents<AddFeatureOnClassLevel>(gate => gate.HandleUnitGainLevel(owner, null));
        }
    }
}
