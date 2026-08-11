using System;
using System.Linq;
using System.IO;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ExpandedSummoningIdentityCatalogTests
    {
        internal static void FoundationLedgerIsExactAndDeterministic()
        {
            var first = ExpandedSummoningIdentityCatalog.Build();
            var second = ExpandedSummoningIdentityCatalog.Build();
            Assertions.Equal(1116, first.Count, "Foundation identity count changed.");
            Assertions.Equal(67, first.Count(value => value.PlannedType == "BlueprintUnit"), "Unit identity count changed.");
            Assertions.Equal(1045, first.Count(value => value.PlannedType == "BlueprintAbility"), "Ability identity count changed.");
            Assertions.Equal(4, first.Count(value => value.PlannedType == "BlueprintBuff"), "Template buff identity count changed.");
            Assertions.Equal(string.Join("|", first.Select(value => value.Symbol)),
                string.Join("|", second.Select(value => value.Symbol)), "Identity output is not deterministic.");
        }

        internal static void TemplateExecutionsAreFamilyScoped()
        {
            var identities = ExpandedSummoningIdentityCatalog.Build();
            Assertions.Equal(182, identities.Count(value => value.Symbol.EndsWith(".Celestial", StringComparison.Ordinal)),
                "Celestial execution count changed.");
            Assertions.Equal(182, identities.Count(value => value.Symbol.EndsWith(".Fiendish", StringComparison.Ordinal)),
                "Fiendish execution count changed.");
            Assertions.True(!identities.Any(value => value.Symbol.Contains(".SNA.") &&
                (value.Symbol.EndsWith(".Celestial", StringComparison.Ordinal) ||
                 value.Symbol.EndsWith(".Fiendish", StringComparison.Ordinal))),
                "SNA must not receive celestial or fiendish execution identities.");
        }

        internal static void SymbolsEncodeEveryLogicalPlacement()
        {
            var identities = ExpandedSummoningIdentityCatalog.Build();
            int found = 0;
            foreach (SummonFamily family in new[] { SummonFamily.Monster, SummonFamily.NaturesAlly })
            foreach (SummonVariantSpec variant in ExpandedSummoningCatalog.GenerateVariants(family))
            {
                string symbol = ExpandedSummoningIdentityCatalog.AbilitySymbol(variant);
                Assertions.Equal(1, identities.Count(value => value.Symbol == symbol),
                    "Logical placement identity missing or duplicated: " + symbol);
                found++;
            }
            Assertions.Equal(681, found, "Logical placement traversal changed.");
        }

        internal static void DonorsCoverEveryFrozenCreature()
        {
            ExpandedSummoningDonorCatalog.Validate();
            Assertions.Equal(67, ExpandedSummoningDonorCatalog.All.Count,
                "Every unique creature requires exactly one frozen donor decision.");
            Assertions.True(ExpandedSummoningDonorCatalog.All.Any(value =>
                !value.DedicatedSummon),
                "Proxy donors must remain explicit sanitizer obligations.");
        }

        internal static void AbilityBuilderPreservesNativeGraphContracts()
        {
            string source = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Blueprints",
                "ExpandedSummoningAbilityBuilder.cs"));
            foreach (string token in new[] { "MonsterParents", "AllyParents",
                "NativeTemplate", "DeepCloneComponent", "ReplaceSpawnUnits",
                "Expected at least one native spawn action", "MaterialComponentData",
                "variant.Multiplicity == SummonMultiplicity.OneD3" })
                Assertions.True(source.Contains(token),
                    "Ability builder contract is missing: " + token);
        }

        internal static void RuntimePublicationIsAdditiveAndTransactional()
        {
            string source = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Blueprints",
                "ExpandedSummoningPublication.cs"));
            foreach (string token in new[] { "SummonVariantMergePolicy.Merge",
                "OriginalComponents", "PublishedComponents",
                "rollback refused after unrelated mutation",
                "originals.Any(original => !variants.Variants.Contains(original))" })
                Assertions.True(source.Contains(token),
                    "Runtime publication contract is missing: " + token);
        }

        internal static void RuntimeUnitComponentsAreReferenceIsolated()
        {
            string source = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Blueprints",
                "ExpandedSummoningBlueprints.cs"));
            foreach (string token in new[] { "DeepCloneUnitComponent",
                "FormatterServices.GetUninitializedObject",
                "ScriptableObject.CreateInstance(type)",
                "source is BlueprintScriptableObject", "ReferenceComparer.Instance" })
                Assertions.True(source.Contains(token),
                    "Unit component isolation contract is missing: " + token);
            Assertions.False(source.Contains("!IsForbiddenComponent(component.GetType().Name)).ToArray()"),
                "Retained donor components must not remain shared instances.");
        }
    }
}
