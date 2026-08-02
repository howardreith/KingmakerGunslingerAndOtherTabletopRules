using System;
using System.Linq;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Buffs;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Diagnostics;
using KingmakerGunslinger.Misfires;

namespace KingmakerGunslinger.Deeds
{
    internal static class ExpertLoadingRuntime
    {
        private static readonly ExpertLoadingService Service =
            new ExpertLoadingService();

        internal static FirearmMisfireConditionDecision Apply(
            RuleAttackRoll attack, FirearmMisfireDecision misfire,
            FirearmMisfireConditionDecision condition, bool firstEvaluation)
        {
            if (attack == null || misfire == null || condition == null ||
                attack.Initiator == null || BlueprintBootstrap.GunslingerClass == null)
                return condition;
            var set = BlueprintBootstrap.GunslingerClass.ExpertLoading;
            Buff[] markers = attack.Initiator.Descriptor.Buffs.RawFacts.OfType<Buff>()
                .Where(value => ReferenceEquals(value.Blueprint,
                    set.ArmedMarker)).ToArray();
            if (markers.Length != 1) return condition;
            Buff marker = markers[0];
            int currentGrit = attack.Initiator.Descriptor.Resources.GetResourceAmount(
                BlueprintBootstrap.GunslingerClass.Grit.Resource);
            TrueGritDecision trueGrit = TrueGritRuntime.Evaluate(
                attack.Initiator.Descriptor, TrueGritDeed.ExpertLoading, 1, false);
            var request = new ExpertLoadingRequest(true, true, firstEvaluation,
                misfire.IsMisfire,
                condition.Transition ==
                    FirearmMisfireConditionTransition.BrokenToWrecked,
                trueGrit.Available ? Math.Max(1, currentGrit) : currentGrit);
            ExpertLoadingDecision decision = Service.Evaluate(request);
            if (!decision.ConsumeMarker) return condition;
            attack.Initiator.Descriptor.Buffs.RemoveFact(marker);
            if (!decision.SuppressExplosion) return condition;

            int before = currentGrit;
            try
            {
                var replacement = new FirearmMisfireConditionDecision(misfire,
                    condition.Before, condition.Before,
                    FirearmMisfireConditionTransition
                        .ExpertLoadingBrokenRemainsBroken);
                attack.Initiator.Descriptor.Resources.Spend(
                    BlueprintBootstrap.GunslingerClass.Grit.Resource,
                    trueGrit.EffectiveCost);
                int after = attack.Initiator.Descriptor.Resources.GetResourceAmount(
                    BlueprintBootstrap.GunslingerClass.Grit.Resource);
                if (after != before - trueGrit.EffectiveCost)
                    throw new InvalidOperationException(
                        "Expert Loading grit spend was not exact.");
                return replacement;
            }
            catch (Exception exception)
            {
                int after = attack.Initiator.Descriptor.Resources.GetResourceAmount(
                    BlueprintBootstrap.GunslingerClass.Grit.Resource);
                if (after < before)
                    attack.Initiator.Descriptor.Resources.Restore(
                        BlueprintBootstrap.GunslingerClass.Grit.Resource,
                        before - after);
                ModContext context;
                if (ModContext.TryGet(out context))
                    context.Logger.Failure("expert-loading", "suppression.failed",
                        "Expert Loading failed closed to ordinary explosion behavior.",
                        exception);
                return condition;
            }
        }
    }
}
