using System;
using System.Linq;
using Kingmaker.Blueprints.Facts;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Buffs;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Diagnostics;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Misfires;
using KingmakerGunslinger.BodyguardFeats;

namespace KingmakerGunslinger.Deeds
{
    internal static class BleedingWoundRuntime
    {
        private static readonly BleedingWoundService Service =
            new BleedingWoundService();

        internal static void AfterAttack(RuleAttackRoll attack)
        {
            if (attack == null || attack.Initiator == null || attack.Target == null)
                return;
            var deliveryTarget = BodyguardRuntime.ResolveDeliveryTarget(attack);
            if (deliveryTarget == null || deliveryTarget.Descriptor == null) return;
            BleedingWoundBlueprintSet set = BlueprintBootstrap.GunslingerClass == null ?
                null : BlueprintBootstrap.GunslingerClass.BleedingWound;
            if (set == null) return;
            Buff[] armed = attack.Initiator.Descriptor.Buffs.RawFacts.OfType<Buff>()
                .Where(f => set.TryGetKind(f.Blueprint) != null).ToArray();
            if (armed.Length == 0) return;
            if (armed.Length != 1)
            {
                foreach (Buff duplicate in armed)
                    attack.Initiator.Descriptor.Buffs.RemoveFact(duplicate);
                return;
            }
            Buff marker = armed[0];
            BleedingWoundKind kind = set.TryGetKind(marker.Blueprint).Value;
            FirearmMarkerSnapshot firearm =
                FirearmMarkerLookup.ReadFromRuleEvent(attack);
            bool eligible = FirearmMisfireRuntime.IsEligibleAttack(attack);
            var request = new BleedingWoundRequest(kind,
                firearm.IsExactFirearm, eligible, attack.IsHit,
                !deliveryTarget.Descriptor.IsUndead,
                attack.ImmuneToSneakAttack,
                int.MaxValue,
                attack.Initiator.Stats.Dexterity.Bonus);
            BleedingWoundDecision decision = Service.Evaluate(request);
            TrueGritDecision trueGrit = TrueGritRuntime.Evaluate(
                attack.Initiator.Descriptor, TrueGritDeed.BleedingWound,
                decision.GritCost, false);
            if (!trueGrit.Available)
                decision = Service.Evaluate(new BleedingWoundRequest(kind,
                    firearm.IsExactFirearm, eligible, attack.IsHit,
                    !deliveryTarget.Descriptor.IsUndead,
                    attack.ImmuneToSneakAttack,
                    attack.Initiator.Descriptor.Resources.GetResourceAmount(
                        BlueprintBootstrap.GunslingerClass.Grit.Resource),
                    attack.Initiator.Stats.Dexterity.Bonus));
            if (!decision.ConsumeMarker) return;
            attack.Initiator.Descriptor.Buffs.RemoveFact(marker);
            if (!decision.Apply) return;
            try
            {
                attack.Initiator.Descriptor.Resources.Spend(
                    BlueprintBootstrap.GunslingerClass.Grit.Resource,
                    trueGrit.EffectiveCost);
                var bleed = set.GetBleed(kind);
                deliveryTarget.Descriptor.Buffs.AddBuff(
                    bleed, marker.Context, null);
                if (!deliveryTarget.Descriptor.Buffs.RawFacts.OfType<Buff>()
                    .Any(value => ReferenceEquals(value.Blueprint, bleed)))
                    throw new InvalidOperationException(
                        "Bleeding Wound native Bleed fact was rejected.");
            }
            catch (Exception exception)
            {
                attack.Initiator.Descriptor.Resources.Restore(
                    BlueprintBootstrap.GunslingerClass.Grit.Resource,
                    trueGrit.EffectiveCost);
                ModContext context;
                if (ModContext.TryGet(out context))
                    context.Logger.Failure("bleeding-wound", "delivery.failed",
                        "Bleeding Wound failed closed after its firearm hit.",
                        exception);
            }
        }
    }
}
