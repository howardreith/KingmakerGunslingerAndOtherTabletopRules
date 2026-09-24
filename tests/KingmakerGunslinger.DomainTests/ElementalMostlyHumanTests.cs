using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KingmakerGunslinger.ElementalRaces;
using KingmakerGunslinger.FavoredClass;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>The four-race Mostly Human companion trait and its scoped host bridge (Phase 5).</summary>
    internal static class ElementalMostlyHumanTests
    {
        private const int PrecedingManifestEntries = 1913 + 43;

        private static string Read(params string[] parts)
        {
            return File.ReadAllText(Path.Combine(new[] { Environment.CurrentDirectory }
                .Concat(parts).ToArray()));
        }

        internal static void PolicyInventoryIsExact()
        {
            ElementalMostlyHumanPolicy.Validate();
            IList<string> symbols = ElementalMostlyHumanPolicy.Symbols();
            Assertions.Equal(13, symbols.Count, "Identity plus selection, standard and trait per parent.");
            Assertions.Equal("KMG.MostlyHuman.Identity", symbols[0], "The shared identity comes first.");
            Assertions.True(symbols.All(value => value.StartsWith(ElementalMostlyHumanPolicy.Prefix,
                StringComparison.Ordinal)), "Its own family, outside the pinned KMG.ElementalRaces inventory.");
            string[] languages = { "Ignan", "Terran", "Auran", "Aquan" };
            IList<ElementalMostlyHumanDefinition> ordered = ElementalMostlyHumanPolicy.Ordered();
            Assertions.Equal(4, ordered.Count, "Four parent races.");
            for (int index = 0; index < ordered.Count; index++)
            {
                ElementalMostlyHumanDefinition definition = ordered[index];
                Assertions.Equal((ElementalHeritageRace)index, definition.Race, "Parent order.");
                Assertions.Equal(languages[index], definition.ElementalLanguage, "Elemental language.");
                Assertions.True(definition.TraitDescription.Contains("human (humanoid)") &&
                    definition.TraitDescription.Contains(definition.RaceName + " (native outsider)") &&
                    definition.TraitDescription.Contains("only one favored class bonus per level") &&
                    definition.TraitDescription.Contains("no language system") &&
                    definition.TraitDescription.Contains(languages[index]),
                    definition.RaceName + " discloses the dual identity and the nonfunctional language part.");
                Assertions.True(string.CompareOrdinal(
                    ElementalMostlyHumanPolicy.ForSymbol(definition.StandardSymbol).Guid,
                    ElementalMostlyHumanPolicy.ForSymbol(definition.TraitSymbol).Guid) < 0,
                    definition.RaceName + " standard entry sorts first.");
            }
            Assertions.Equal(ElementalMostlyHumanPolicy.IdentitySymbol,
                "KMG.MostlyHuman.Identity", "Permission-graph identity.");
        }

        // L08: the committed Mostly Human block follows the Favored Class block exactly.
        internal static void ManifestBlockFollowsTheFavoredClassBlock()
        {
            JToken[] entries = JObject.Parse(Read("blueprints", "blueprints.json"))["entries"].ToArray();
            int start = PrecedingManifestEntries + FavoredClassIdentityCatalog.IdentityCount;
            JToken[] block = entries.Skip(start).ToArray();
            IList<ElementalMostlyHumanIdentity> identities = ElementalMostlyHumanPolicy.All;
            Assertions.Equal(identities.Count, block.Length, "Mostly Human block is the manifest tail.");
            for (int index = 0; index < identities.Count; index++)
            {
                Assertions.Equal(identities[index].Symbol, (string)block[index]["symbol"], "Symbol at " + index);
                Assertions.Equal(identities[index].Guid, (string)block[index]["guid"], "GUID at " + index);
                Assertions.Equal(identities[index].PlannedType, (string)block[index]["plannedType"],
                    "Type at " + index);
                Assertions.Equal("active", (string)block[index]["status"], "Status at " + index);
                Assertions.Equal(ElementalMostlyHumanPolicy.Milestone, (string)block[index]["milestone"],
                    "Milestone at " + index);
            }
            Assertions.Equal(240, entries.Count(value => ((string)value["symbol"]).StartsWith(
                "KMG.ElementalRaces.", StringComparison.Ordinal) && (string)value["status"] == "active"),
                "The pinned elemental race inventory is unchanged.");
        }

        // The companion trait is real racial material: a Heritage-phase
        // choice that keeps the race blueprint, stats, heritages, traits and
        // creature-type facts, and grants one shared identity fact.
        internal static void FactoryAddsAHeritageChoiceWithoutTouchingRaceIdentity()
        {
            string factory = Read("src", "KingmakerGunslinger", "ElementalRaces",
                "ElementalMostlyHumanBlueprints.cs");
            foreach (string token in new[]
            {
                "selection.Group = FeatureGroup.AasimarHeritage;",
                "selection.Obligatory = false;",
                "selection.AllFeatures = new[] { standard, trait };",
                "grant.Facts = new BlueprintUnitFact[] { identity };",
                "feature.HideInUI = true;",
                "before.Concat(new BlueprintFeatureBase[] { race.Selection })",
                "if (index < _published.Count && ReferenceEquals(race.Features, _published[index]))"
            })
                Assertions.True(factory.Contains(token), "Mostly Human factory token: " + token);
            foreach (string forbidden in new[]
            {
                "9054d3988d491d944ac144e27b6bc318", "OutsiderType", "0a5d473ead98b0646b94495af250fdc4",
                "SetRace", "RaceId", "AddStatBonus", "Doll", "RemoveFeature"
            })
                Assertions.False(factory.Contains(forbidden),
                    "The companion trait never rewrites race or type identity: " + forbidden);
            string bootstrap = Read("src", "KingmakerGunslinger", "Bootstrap", "BlueprintBootstrap.cs");
            Assertions.True(bootstrap.Contains("var mostlyHumanRegistry = new BlueprintRegistry(") &&
                bootstrap.Contains("mostlyHumanRegistry.RollbackAll();") &&
                bootstrap.Contains("bool offerMostlyHuman = publicationPlan.ElementalRaceSelectors &&") &&
                bootstrap.Contains("_mostlyHuman.Publication = ElementalMostlyHumanPublication.Apply(_mostlyHuman);") &&
                bootstrap.Contains("FavoredClassRuntime.ConfigureMostlyHumanIdentity(\n" +
                    "                        _mostlyHuman.Identity);"),
                "Registered in a contained registry, identity configured, published only when enabled.");
        }

        // Charter 5.2/6.5: exactly the host's own Human race prerequisite
        // instances (its human favored-class leaves and human race traits),
        // only the verified Mostly Human permission, fail closed, and owned by
        // the racial trait rather than the favored-class integration switch.
        internal static void HostBridgeIsExactlyScoped()
        {
            string bridge = Read("src", "KingmakerGunslinger", "FavoredClass", "Hooks",
                "FavoredClassHostRaceBridge.cs");
            foreach (string token in new[]
            {
                "foreach (KeyValuePair<string, BlueprintFeatureSelection> entry in host.BonusSelections)",
                "foreach (BlueprintScriptableObject blueprint in library.BlueprintsByAssetId.Values.Distinct())",
                "component.GetType() == host.PrerequisiteRaceType &&",
                "ReferenceEquals(host.PrerequisiteRaceField.GetValue(component), human)",
                "if (__result || __1 == null || !IsTracked(__instance))",
                "if (FavoredClassRuntime.GrantsHostHumanAccess(__1))",
                "check.DeclaringType != host.PrerequisiteRaceType"
            })
                Assertions.True(bridge.Contains(token), "Bridge scope token: " + token);
            foreach (string forbidden in new[] { "SetRace", "Progression.Race =", "__result = false",
                "ZFavoredClass", "[HarmonyPatch", ".name.Contains(", "Appearance", "Doll" })
                Assertions.False(bridge.Contains(forbidden), "Bridge must not: " + forbidden);
            string coordinator = Read("src", "KingmakerGunslinger", "FavoredClass",
                "FavoredClassIntegrationCoordinator.cs");
            int prepare = coordinator.IndexOf("PrepareAncestryBridge(context, host);", StringComparison.Ordinal);
            int disabled = coordinator.IndexOf("if (!profile.IntegrationEnabled)\n", StringComparison.Ordinal);
            int commit = coordinator.IndexOf("publication.Commit();", StringComparison.Ordinal);
            Assertions.True(prepare > 0 && disabled > prepare && commit > prepare,
                "The racial bridge is scoped before the integration switch and before any publication commits.");
            Assertions.True(coordinator.Contains("Hooks.FavoredClassHostRaceBridge.Prepare(context.Harmony,") &&
                coordinator.Contains("Hooks.FavoredClassHostRaceBridge.Clear();"),
                "A failed bridge preparation empties only the bridge scope.");
        }

        internal static void HostBridgeAddsOnlyMostlyHumanGeniekin()
        {
            const string fact = "kmg.test.mostly-human";
            foreach (string parent in FavoredClassAncestry.ElementalParents)
            {
                Assertions.True(FavoredClassEligibility.GrantsHostHumanAccess(
                    new FavoredClassAncestryEvidence(parent, new[] { fact }), fact),
                    parent + " with the verified trait counts as human for the host.");
                Assertions.False(FavoredClassEligibility.GrantsHostHumanAccess(
                    new FavoredClassAncestryEvidence(parent, new string[0]), fact),
                    parent + " without the trait gains nothing.");
            }
            foreach (string other in new[]
            {
                FavoredClassAncestry.HalfElf, FavoredClassAncestry.HalfOrc, FavoredClassAncestry.Aasimar,
                FavoredClassAncestry.Tiefling, FavoredClassAncestry.Dwarf, FavoredClassAncestry.Suli,
                FavoredClassAncestry.Human
            })
                Assertions.False(FavoredClassEligibility.GrantsHostHumanAccess(
                    new FavoredClassAncestryEvidence(other, new[] { fact }), fact),
                    other + " keeps exactly the host's own policy.");
            Assertions.False(FavoredClassEligibility.GrantsHostHumanAccess(
                new FavoredClassAncestryEvidence(null, new[] { fact }), fact),
                "An unrecognized race gains nothing.");
            Assertions.False(FavoredClassEligibility.GrantsHostHumanAccess(
                new FavoredClassAncestryEvidence(FavoredClassAncestry.Ifrit, new[] { fact }), null),
                "Without a verified identity nothing is granted.");
            Assertions.False(FavoredClassEligibility.GrantsHostHumanAccess(null, fact),
                "Missing evidence grants nothing.");
        }
    }
}
