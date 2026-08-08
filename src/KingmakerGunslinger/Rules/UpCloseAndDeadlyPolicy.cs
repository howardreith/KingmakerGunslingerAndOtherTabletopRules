using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Rules
{
    internal sealed class UpCloseAndDeadlyDecision
    {
        internal UpCloseAndDeadlyDecision(bool consume, bool apply,
            int dice, float modifier, string reason)
        {
            ConsumeMarker = consume;
            ApplyDamage = apply;
            Dice = dice;
            Modifier = modifier;
            Reason = reason;
        }

        internal bool ConsumeMarker { get; private set; }
        internal bool ApplyDamage { get; private set; }
        internal int Dice { get; private set; }
        internal float Modifier { get; private set; }
        internal string Reason { get; private set; }
    }

    internal static class UpCloseAndDeadlyPolicy
    {
        internal const int FixedGritCost = 1;

        internal static int DiceAtLevel(int gunslingerLevel)
        {
            if (gunslingerLevel < 1 || gunslingerLevel > 20)
                throw new ArgumentOutOfRangeException("gunslingerLevel");
            if (gunslingerLevel >= 20) return 5;
            if (gunslingerLevel >= 15) return 4;
            if (gunslingerLevel >= 10) return 3;
            if (gunslingerLevel >= 5) return 2;
            return 1;
        }

        internal static UpCloseAndDeadlyDecision Evaluate(bool exactFirearm,
            int markerCount, FirearmKind kind, bool scatter, bool eligibleDischarge,
            bool hit, bool precisionImmune, int gunslingerLevel, int grit)
        {
            if (markerCount < 0) throw new ArgumentOutOfRangeException("markerCount");
            if (grit < 0) throw new ArgumentOutOfRangeException("grit");
            if (!exactFirearm || markerCount != 1 || scatter ||
                !FirearmHandednessPolicy.Matches(kind,
                    FirearmHandedness.OneHanded))
                return new UpCloseAndDeadlyDecision(false, false, 0, 0f,
                    "nonqualifying-shot");
            if (!eligibleDischarge)
                return new UpCloseAndDeadlyDecision(false, false, 0, 0f,
                    "failed-discharge");
            int dice = DiceAtLevel(gunslingerLevel);
            if (precisionImmune)
                return new UpCloseAndDeadlyDecision(true, false, dice, 0f,
                    "precision-immune");
            if (grit < FixedGritCost)
                return new UpCloseAndDeadlyDecision(true, false, dice, 0f,
                    "insufficient-grit");
            return new UpCloseAndDeadlyDecision(true, true, dice,
                hit ? 1f : 0.5f, hit ? "hit" : "miss");
        }
    }
}
