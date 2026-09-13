using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Spells.Teleportation
{
    // The verified scroll contract has one UMD d20 (when required), followed
    // by the native item/spell failure d100. This is arithmetic over a
    // read-only native snapshot, never a rule event or simulated activation.
    internal sealed class TeleportScrollActivationChance
    {
        internal readonly bool Supported, NoCheck;
        internal readonly decimal Probability;
        internal readonly string Diagnostic;
        private TeleportScrollActivationChance(bool supported, bool noCheck, decimal probability, string diagnostic)
        { Supported = supported; NoCheck = noCheck; Probability = probability; Diagnostic = diagnostic ?? ""; }
        internal static TeleportScrollActivationChance Native(bool needsUmd, int modifier, int dc, int failurePercent,
            bool takeTen, int successBonus, string diagnostic)
        {
            decimal skill = !needsUmd || takeTen && 10 + modifier >= dc ? 1m :
                Math.Max(0, Math.Min(20, 21 + modifier + Math.Max(0, successBonus) - dc)) / 20m;
            int failure = Math.Max(0, Math.Min(100, failurePercent));
            return new TeleportScrollActivationChance(true, !needsUmd && failure == 0,
                skill * (100 - failure) / 100m, diagnostic);
        }
        internal static TeleportScrollActivationChance Unsupported(string diagnostic)
        { return new TeleportScrollActivationChance(false, false, 0m, diagnostic); }
    }
    internal static class TeleportScrollReaderPolicy
    {
        // Input is ONE material variant. UI identity never includes this winner.
        // With a single reader no probability assumption is needed to choose.
        // Multiple unsupported chances fail closed rather than rank a heuristic.
        internal static TeleportCastSourceSnapshot Select(IEnumerable<TeleportCastSourceSnapshot> sources)
        {
            var eligible = sources.Where(value => value != null && value.Kind == TeleportCastSourceKind.Scroll &&
                TeleportCastAvailabilityPolicy.Usable(value)).OrderBy(value => value.PartyOrder)
                .ThenBy(value => value.CasterId, StringComparer.Ordinal).ToArray();
            if (eligible.Length == 0) return null;
            if (eligible.Length == 1) return eligible[0];
            if (eligible.Any(value => value.ActivationChance == null || !value.ActivationChance.Supported)) return null;
            return eligible.OrderByDescending(value => value.ActivationChance.Probability)
                .ThenByDescending(value => value.ActivationChance.NoCheck).ThenBy(value => value.PartyOrder)
                .ThenBy(value => value.CasterId, StringComparer.Ordinal).First();
        }
    }
}
