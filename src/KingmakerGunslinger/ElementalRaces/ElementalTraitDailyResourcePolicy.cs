using System;

namespace KingmakerGunslinger.ElementalRaces
{
    internal static class ElementalTraitDailyResourcePolicy
    {
        // Activation may reconstruct a resource with its full native amount,
        // or hydrate an already-spent native amount. Neither path may refill it.
        internal static int ActivationAmount(int nativeAmount, int? rememberedAmount)
        {
            int current = Math.Max(0, nativeAmount);
            return rememberedAmount.HasValue
                ? Math.Min(current, Math.Max(0, rememberedAmount.Value)) : current;
        }
    }
}
