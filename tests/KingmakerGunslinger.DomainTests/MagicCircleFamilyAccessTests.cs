using System;
using KingmakerGunslinger.Spells.MagicCircle;

namespace KingmakerGunslinger.DomainTests
{
    internal static class MagicCircleFamilyAccessTests
    {
        private sealed class Entry { internal Entry[] Variants; }
        private static readonly Entry Evil = new Entry(), Good = new Entry(), Chaos = new Entry(), Law = new Entry();
        private static readonly Entry Full = new Entry { Variants = new[] { Evil, Good, Chaos, Law } };
        private static readonly Entry Paladin = new Entry { Variants = new[] { Evil, Chaos } };
        private static readonly Entry Antipaladin = new Entry { Variants = new[] { Good, Law } };
        private static bool Eligible(Entry child, params Entry[] published)
        { return MagicCircleFamilyAccess.ContainsVariant(child, new[] { Evil, Good, Chaos, Law },
            new[] { Full, Paladin, Antipaladin }, published, parent => parent.Variants); }

        internal static void GeneralFamilyGrantsItsFourChildren()
        {
            foreach (var child in Full.Variants) Assertions.True(Eligible(child, Full), "A published general family admits each owned child.");
        }
        internal static void RestrictedFamiliesDoNotBroadenScrollAccess()
        {
            Assertions.True(Eligible(Evil, Paladin) && Eligible(Chaos, Paladin), "Paladin may use Evil/Chaos scrolls.");
            Assertions.True(!Eligible(Good, Paladin) && !Eligible(Law, Paladin), "Paladin cannot inherit the general parent's forbidden variants.");
            Assertions.True(Eligible(Good, Antipaladin) && Eligible(Law, Antipaladin), "Antipaladin may use Good/Law scrolls.");
            Assertions.True(!Eligible(Evil, Antipaladin) && !Eligible(Chaos, Antipaladin), "Antipaladin cannot inherit Evil/Chaos.");
        }
        internal static void AbsentPublicationDoesNotGrantAccess()
        {
            foreach (var child in Full.Variants) Assertions.True(!Eligible(child), "An owned parent reference alone is not a class entitlement.");
            Assertions.True(!Eligible(Evil, (Entry)null), "Null entries confer nothing.");
        }
        internal static void ForeignParentsCannotGrantOwnedVariants()
        {
            var foreign = new Entry { Variants = Full.Variants };
            Assertions.True(!Eligible(Evil, foreign), "A copied variant array on a foreign family is not an owned publication.");
            Assertions.True(Eligible(Evil, foreign, Paladin), "An independent legitimate entitlement still works.");
        }
        internal static void ForeignChildrenAndMalformedRootsAreIgnored()
        {
            var foreign = new Entry();
            Assertions.True(!Eligible(foreign, Full), "Foreign spells retain the native membership result.");
            Assertions.True(!Eligible(null, Full), "Null child is never eligible.");
            Assertions.True(!MagicCircleFamilyAccess.ContainsVariant(Evil, new[] { Evil }, new[] { Full },
                new[] { Full }, parent => null), "A parent without variants cannot grant a child.");
        }
    }
}
