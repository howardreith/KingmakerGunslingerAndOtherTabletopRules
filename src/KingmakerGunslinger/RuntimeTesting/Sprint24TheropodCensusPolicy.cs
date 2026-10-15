using System;
using System.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Closed, source-controlled inputs for the one Sprint 24 theropod census.
    /// It is intentionally not a name-driven donor search: these are the two
    /// already recorded biped candidates whose exact rest/action metadata is
    /// still needed before authoring either original body.
    /// </summary>
    internal static class Sprint24TheropodCensusPolicy
    {
        internal static readonly string[][] Donors = {
            new[] {
                "lizardfolk-free-hands",
                "a8cbd4cdb0feaf64eab1452257c3c971",
                "d6db6ba3b2c459a48ba0730beef0bac8"
            },
            new[] {
                "troll-negative-control",
                "b98735a1737ae494dbe5cbeca1c7c083",
                "0bc98460fca38964aae3af6ad5c655ee"
            }
        };

        // Exact installed-engine identities. The first six are already used
        // by released summon profiles. Diehard, Endurance and the native bleed
        // cleanup graph are measured here before Sprint 24 may depend on them.
        internal static readonly string[][] FactAnchors = {
            new[] { "Pounce", "1a8149c09e0bdfc48a305ee6ac3729a8" },
            new[] { "CriticalFocus", "8ac59959b1b23c347a0361dc97cc786d" },
            new[] { "ImprovedCriticalBite", "8388e2e50bb779840adc8129b3d63bf1" },
            new[] { "ImprovedInitiative", "797f25d709f559546b29e7bcb181cc74" },
            new[] { "IronWill", "175d1577bb6c9a04baf88eec99c66334" },
            new[] { "SkillFocusPerception", "f74c6bdf5c5f5374fb9302ecdc1f7d64" },
            new[] { "Diehard", "86669ce8759f9d7478565db69b8c19ad" },
            new[] { "Endurance", "54ee847996c25cd4ba8773d7b8555174" },
            new[] { "Bleed1d6", "75039846c3d85d940aa96c249b97e562" }
        };

        internal static readonly string[] CriticalDiscoveryTerms = {
            "BleedingCritical", "Bleeding Critical", "Bleed"
        };

        internal const int MaximumDiscoveredCriticalBlueprints = 128;

        internal static void Validate()
        {
            if (Donors.Length != 2 || Donors.Any(value => value.Length != 3) ||
                Donors.SelectMany(value => value.Skip(1)).Any(value =>
                    value.Length != 32 || value.Any(c => !Uri.IsHexDigit(c))) ||
                Donors.Select(value => value[1]).Distinct().Count() != Donors.Length ||
                Donors.Select(value => value[2]).Distinct().Count() != Donors.Length)
                throw new InvalidOperationException(
                    "Sprint 24 closed theropod donor selection is invalid.");

            if (FactAnchors.Length != 9 ||
                FactAnchors.Any(value => value.Length != 2 ||
                    value[1].Length != 32 || value[1].Any(c => !Uri.IsHexDigit(c))) ||
                FactAnchors.Select(value => value[1]).Distinct().Count() !=
                    FactAnchors.Length ||
                CriticalDiscoveryTerms.Length != 3 ||
                MaximumDiscoveredCriticalBlueprints != 128)
                throw new InvalidOperationException(
                    "Sprint 24 closed native-fact selection is invalid.");
        }

        internal static bool MatchesCriticalCandidate(string name)
        {
            return !string.IsNullOrEmpty(name) && CriticalDiscoveryTerms.Any(term =>
                name.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }
}
