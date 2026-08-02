using System;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class BleedingWoundService
    {
        internal BleedingWoundDecision Evaluate(BleedingWoundRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            int cost = request.Kind == BleedingWoundKind.HitPoints ? 1 : 2;
            int amount = request.Kind == BleedingWoundKind.HitPoints ?
                Math.Max(0, request.DexterityModifier) : 1;
            bool consume = request.ExactFirearm && request.EligibleAttack;
            if (!consume) return Reject(request, false, cost, amount,
                "not-exact-eligible-firearm");
            if (!request.Hit) return Reject(request, true, cost, amount, "miss");
            if (!request.LivingTarget) return Reject(request, true, cost, amount,
                "nonliving-target");
            if (request.ImmuneToSneakAttack) return Reject(request, true, cost,
                amount, "sneak-immune");
            if (amount <= 0) return Reject(request, true, cost, amount,
                "nonpositive-bleed");
            if (request.Grit < cost) return Reject(request, true, cost, amount,
                "insufficient-grit");
            return new BleedingWoundDecision(request.Kind, true, true, cost,
                amount, "eligible");
        }

        private static BleedingWoundDecision Reject(BleedingWoundRequest request,
            bool consume, int cost, int amount, string reason)
        {
            return new BleedingWoundDecision(request.Kind, consume, false, cost,
                amount, reason);
        }
    }
}
