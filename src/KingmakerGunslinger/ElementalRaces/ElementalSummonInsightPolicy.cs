using System;
using System.Collections.Generic;

namespace KingmakerGunslinger.ElementalRaces
{
    internal static class ElementalSummonInsightPolicy
    {
        // Native Kingmaker summon-family identities, independently usable
        // while the Expanded Summoning selector module is disabled.
        private static readonly string[] Parents = {
            "8fd74eddd9b6c224693d9ab241f25e84", "1724061e89c667045a6891179ee2e8e7",
            "5d61dde0020bbf54ba1521f7ca0229dc", "7ed74a3ec8c458d4fb50b192fd7be6ef",
            "630c8b85d9f07a64f917d79cb5905741", "e740afbab0147944dab35d83faa0ae1c",
            "ab167fd8203c1314bac6568932f1752f", "d3ac756a229830243a72e84f3ab050d0",
            "52b5df2a97df18242aec67610616ded0", "c6147854641924442a3bb736080cfeb6",
            "298148133cdc3fd42889b99c82711986", "fdcf7e57ec44f704591f11b45f4acf61",
            "c83db50513abdf74ca103651931fac4b", "8f98a22f35ca6684a983363d32e51bfe",
            "55bbce9b3e76d4a4a8c8e0698d29002c", "051b979e7d7f8ec41b9fa35d04746b33",
            "ea78c04f0bd13d049a1cce5daf8d83e0", "a7469ef84ba50ac4cbf3d145e3173f8e"
        };

        internal static IReadOnlyList<string> NativeParentGuids
        { get { return Array.AsReadOnly(Parents); } }

        internal static string NativeSubtypeGuid(ElementalAlternateTraitId trait)
        {
            switch (trait)
            {
                case ElementalAlternateTraitId.FireInsight:
                    return "23dc7b90d148b9d439f48e015a520a9c";
                case ElementalAlternateTraitId.EarthInsight:
                    return "e147258e5b7c40643893d80c9f2816e8";
                case ElementalAlternateTraitId.AirInsight:
                    return "dd3d0c7f4f57f304cbdbb68170b1b775";
                default:
                    return null;
            }
        }

        internal static int BonusRounds(ElementalAlternateTraitId trait,
            bool ordinarySpellbookCast, bool namedSummonFamily,
            bool matchingNativeSubtype, bool temporaryLinkedSummon)
        {
            return NativeSubtypeGuid(trait) != null && ordinarySpellbookCast &&
                namedSummonFamily && matchingNativeSubtype &&
                temporaryLinkedSummon ? 2 : 0;
        }
    }
}
