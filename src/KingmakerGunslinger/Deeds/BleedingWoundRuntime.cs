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

namespace KingmakerGunslinger.Deeds
{
    internal static class BleedingWoundRuntime
    {
        private static readonly BleedingWoundService Service =
            new BleedingWoundService();

        internal static void AfterAttack(RuleAttackWithWeapon attack)
        {
            if (attack == null || attack.Initiator == null || attack.Target == null ||
                attack.AttackRoll == null) return;
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
            bool eligible = FirearmMisfireRuntime.IsEligibleAttack(
                attack.AttackRoll);
            var request = new BleedingWoundRequest(kind,
                firearm.IsExactFirearm, eligible, attack.AttackRoll.IsHit,
                !attack.Target.Descriptor.IsUndead,
                attack.AttackRoll.ImmuneToSneakAttack,
                attack.Initiator.Descriptor.Resources.GetResourceAmount(
                    BlueprintBootstrap.GunslingerClass.Grit.Resource),
                attack.Initiator.Stats.Dexterity.Bonus);
            BleedingWoundDecision decision = Service.Evaluate(request);
            if (!decision.ConsumeMarker) return;
            attack.Initiator.Descriptor.Buffs.RemoveFact(marker);
            if (!decision.Apply) return;
            try
            {
                attack.Initiator.Descriptor.Resources.Spend(
                    BlueprintBootstrap.GunslingerClass.Grit.Resource,
                    decision.GritCost);
                Buff applied = attack.Target.Descriptor.Buffs.AddBuff(
                    set.GetBleed(kind), marker.Context, null);
                if (applied == null)
                    throw new InvalidOperationException(
                        "Bleeding Wound native Bleed fact was rejected.");
            }
            catch (Exception exception)
            {
                attack.Initiator.Descriptor.Resources.Restore(
                    BlueprintBootstrap.GunslingerClass.Grit.Resource,
                    decision.GritCost);
                ModContext context;
                if (ModContext.TryGet(out context))
                    context.Logger.Failure("bleeding-wound", "delivery.failed",
                        "Bleeding Wound failed closed after its firearm hit.",
                        exception);
            }
        }
    }
}
