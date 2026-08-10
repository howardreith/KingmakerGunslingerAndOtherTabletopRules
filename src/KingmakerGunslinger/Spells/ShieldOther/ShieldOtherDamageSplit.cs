using System;

namespace KingmakerGunslinger.Spells.ShieldOther
{
    internal sealed class ShieldOtherDamageSplit
    {
        internal ShieldOtherDamageSplit(int subjectShare, int casterShare,
            string status)
        {
            if (subjectShare < 0) throw new ArgumentOutOfRangeException("subjectShare");
            if (casterShare < 0) throw new ArgumentOutOfRangeException("casterShare");
            SubjectShare = subjectShare;
            CasterShare = casterShare;
            Status = status ?? string.Empty;
        }

        internal int SubjectShare { get; private set; }
        internal int CasterShare { get; private set; }
        internal string Status { get; private set; }
        internal bool Transfers { get { return CasterShare > 0; } }
    }
}
