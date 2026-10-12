using System;
using System.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static class Sprint22DonorSurveyPolicy
    {
        // Closed selection: existing proxy, existing crocodilian donor and
        // heavy quadruped reference. No name discovery or roster census.
        internal static readonly string[][] Donors = {
            new[] { "hodag", "c3524f96954a1d94f8525b86e7626633" },
            new[] { "monitor", "4109b40f6bbb49640840644cc84ada67" },
            new[] { "mastodon", "028cc6f46e7998f46855a33ffde89567" }
        };

        internal static void Validate()
        {
            if (Donors.Length != 3 || Donors.Any(value => value.Length != 2 ||
                value[1].Length != 32 || value[1].Any(c => !Uri.IsHexDigit(c))) ||
                Donors.Select(value => value[1]).Distinct().Count() != Donors.Length)
                throw new InvalidOperationException("Sprint 22 donor selection is invalid.");
        }
    }
}
