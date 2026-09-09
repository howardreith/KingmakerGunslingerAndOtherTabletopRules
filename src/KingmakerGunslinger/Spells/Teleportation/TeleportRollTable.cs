using System;

namespace KingmakerGunslinger.Spells.Teleportation
{
    internal sealed class TeleportOutcomeDecision
    {
        internal TeleportOutcomeDecision(int onTargetMax, int offTargetMax, int similarMax)
        { OnTargetMax = onTargetMax; OffTargetMax = offTargetMax; SimilarMax = similarMax; }
        internal int OnTargetMax { get; private set; }
        internal int OffTargetMax { get; private set; }
        internal int SimilarMax { get; private set; }
        internal int OnTargetPercent { get { return OnTargetMax; } }
        internal int OffTargetPercent { get { return OffTargetMax - OnTargetMax; } }
        internal int SimilarLocationPercent { get { return SimilarMax - OffTargetMax; } }
        internal int MishapPercent { get { return 100 - SimilarMax; } }
        internal TeleportOutcomeKind Resolve(int d100)
        {
            if (d100 < 1 || d100 > 100) throw new ArgumentOutOfRangeException("d100");
            if (d100 <= OnTargetMax) return TeleportOutcomeKind.OnTarget;
            if (d100 <= OffTargetMax) return TeleportOutcomeKind.OffTarget;
            if (d100 <= SimilarMax) return TeleportOutcomeKind.SimilarLocation;
            return TeleportOutcomeKind.Mishap;
        }
    }

    internal static class TeleportRollTable
    {
        internal static TeleportOutcomeDecision For(TeleportFamiliarity familiarity)
        {
            switch (familiarity)
            {
                case TeleportFamiliarity.VeryFamiliar: return new TeleportOutcomeDecision(97, 99, 100);
                case TeleportFamiliarity.StudiedCarefully: return new TeleportOutcomeDecision(94, 97, 99);
                case TeleportFamiliarity.SeenCasually: return new TeleportOutcomeDecision(88, 94, 98);
                case TeleportFamiliarity.ViewedOnce: return new TeleportOutcomeDecision(76, 88, 96);
                default: throw new ArgumentOutOfRangeException("familiarity");
            }
        }

        internal static TeleportOutcomeKind Resolve(TeleportFamiliarity familiarity, int d100)
        { return For(familiarity).Resolve(d100); }
    }
}
