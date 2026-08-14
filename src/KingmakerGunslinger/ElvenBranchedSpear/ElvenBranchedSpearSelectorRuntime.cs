using System;
using System.Linq;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using KingmakerGunslinger.CustomWeapons;

namespace KingmakerGunslinger.ElvenBranchedSpear
{
    internal static class ElvenBranchedSpearSelectorRuntime
    {
        private const string SourceKey = "elven-branched-spear";

        internal static void Configure(BlueprintParametrizedFeature[] selectors,
            bool publicationEnabled)
        {
            if (selectors == null || selectors.Length == 0 ||
                selectors.Any(value => value == null) ||
                selectors.Distinct().Count() != selectors.Length)
                throw new ArgumentException("Spear parameter selectors are incomplete.");
            CustomWeaponSelectorRuntime.Configure(SourceKey, selectors,
                new[] { new CustomWeaponSelectorOption(
                    ElvenBranchedSpearCategoryRuntime.Category,
                    ElvenBranchedSpearCategoryRuntime.DisplayName,
                    ElvenBranchedSpearCategoryRuntime.Monogram) },
                publicationEnabled);
        }

        internal static void Rollback()
        {
            CustomWeaponSelectorRuntime.Rollback(SourceKey);
        }
    }
}
