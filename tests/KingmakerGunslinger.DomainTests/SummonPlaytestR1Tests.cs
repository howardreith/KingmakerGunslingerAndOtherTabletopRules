using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Repair sprint R1 (owner manual playtest, 2026-11-07): the five owner
    /// cuts remove public and automatic routes while every registered
    /// identity stays allocated for older saves.
    /// </summary>
    internal static class SummonPlaytestR1Tests
    {
        private static SummonVariantSpec[] All()
        {
            return ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                .Concat(ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly))
                .ToArray();
        }

        // CUT-01: the derived set is exactly Stirge's nine SNA I-IX
        // placements, and nothing else leaves the public surface.
        internal static void StirgeLeavesEveryCanonicalRouteAndNothingElseDoes()
        {
            SummonVariantSpec[] all = All();
            SummonVariantSpec[] withheld = all.Where(value =>
                !SummonVisibilityCatalog.IsPublished(value)).ToArray();
            Assertions.Equal(9, withheld.Length, "Exactly nine logical placements are withheld.");
            Assertions.True(withheld.All(value => value.Creature.Key == SummonPlaytestWithdrawalPolicy.StirgeKey &&
                    value.Family == SummonFamily.NaturesAlly && !value.Creature.MonsterTemplated),
                "Every withheld placement is an untemplated Stirge SNA route.");
            Assertions.True(withheld.Select(value => value.ParentTier).OrderBy(value => value)
                    .SequenceEqual(SummonPlaytestWithdrawalPolicy.StirgeWithheldNaturesAllyTiers),
                "Stirge is withheld at every Nature's Ally parent I-IX, including the higher-tier 1d3/1d4+1 rows.");
            Assertions.False(all.Any(value => value.Creature.Key == SummonPlaytestWithdrawalPolicy.StirgeKey &&
                value.Family == SummonFamily.Monster), "Stirge has no Summon Monster row to escape the cut.");
            Assertions.True(all.Where(value => value.Creature.Key != SummonPlaytestWithdrawalPolicy.StirgeKey)
                .All(SummonVisibilityCatalog.IsPublished), "Every unrelated placement stays published.");
            Assertions.Equal(1188, SummonVisibilityCatalog.RegisteredLogicalPlacementCount,
                "Registered placements do not move: the nine Stirge roots stay allocated.");
            Assertions.Equal(1179, SummonVisibilityCatalog.PublishedLogicalPlacementCount,
                "The public surface goes from 1188 to 1179 logical placements.");
            Assertions.Equal(all.Count(SummonVisibilityCatalog.IsPublished),
                SummonVisibilityCatalog.PublishedLogicalPlacementCount, "The published count is derived, not stated.");
            // Unrelated controls on the same SNA I-IX parents remain.
            foreach (string control in new[] { "dire-rat", "viper", "eagle", "giant-centipede" })
                Assertions.True(ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly)
                    .Where(value => value.Creature.Key == control).All(SummonVisibilityCatalog.IsPublished),
                    control + " keeps every Nature's Ally route.");
            Assertions.Equal(SummonFamilyCoverage.Registered,
                ExpandedSummoningCoveragePolicy.Coverage(SummonPlaytestWithdrawalPolicy.StirgeKey, SummonFamily.NaturesAlly, 1),
                "Stirge remains registered coverage for saves, not a live choice.");
            SummonVisibilityCatalog.Validate();
        }

        // CUT-01: every supported alternate caller reads the same predicate.
        // Its Stirge identities stay registered (saved adapters resolve) but
        // none is exposed; the same callers still expose their other choices.
        internal static void StirgeLeavesEveryAlternateCallerButStaysRegistered()
        {
            AlternateSummoningRoster production = AlternateSummoningRoster.Production();
            int natureCallers = 0;
            foreach (AlternateSummoningFamilyIdentity family in AlternateSummoningSourceCatalog.Identities)
            {
                AlternateSummoningChoice[] stirge = AlternateSummoningProjection.Identities(family, production)
                    .Where(value => value.Kind == AlternateSummoningChoiceKind.Creature &&
                        value.Placement.Creature.Key == SummonPlaytestWithdrawalPolicy.StirgeKey).ToArray();
                if (family.Family != SummonFamily.NaturesAlly)
                {
                    Assertions.Equal(0, stirge.Length, family.Key + " never had a Stirge identity.");
                    continue;
                }
                natureCallers++;
                Assertions.Equal(family.Tiers.Count(), stirge.Length,
                    family.Key + " keeps one registered Stirge identity per tier it serves.");
                Assertions.True(stirge.All(value => !value.Published),
                    family.Key + " marks every Stirge identity unpublished.");
                foreach (int tier in family.Tiers)
                {
                    AlternateSummoningChoice[] exposed = AlternateSummoningProjection
                        .Exposed(family, production, tier, null).ToArray();
                    Assertions.False(exposed.Any(value =>
                        value.Placement.Creature.Key == SummonPlaytestWithdrawalPolicy.StirgeKey),
                        family.Key + " tier " + tier + " does not offer Stirge.");
                    Assertions.True(exposed.Length > 0, family.Key + " tier " + tier + " still offers its other creatures.");
                }
            }
            Assertions.True(natureCallers > 0, "At least one supported Nature's Ally alternate caller was checked.");
        }

        // CUT-02..05: the withdrawal inventory is exactly the owner's list,
        // every withdrawn identity stays registered and active in the
        // manifest, and the retained Wall cleanup identities do not move.
        internal static void WithdrawnIdentitiesStayRegisteredForSaves()
        {
            string[] keys = SummonPlaytestWithdrawalPolicy.All.Select(value => value.CreatureKey).ToArray();
            Assertions.True(keys.OrderBy(value => value, StringComparer.Ordinal).SequenceEqual(
                new[] { "bone-devil", "cloud-giant", "fire-giant", "ice-devil", "lillend-azata", "magma-mephit" }),
                "Exactly the owner's four capability cuts on five creatures, plus Repair R5's retired Cloud Giant Fog Cloud route.");
            var registered = new HashSet<string>(ExpandedSummoningIdentityCatalog.Build()
                .Select(value => value.Symbol), StringComparer.Ordinal);
            JArray entries = (JArray)JObject.Parse(File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "blueprints", "blueprints.json")))["entries"];
            var active = new HashSet<string>(entries.Where(value => (string)value["status"] == "active")
                .Select(value => (string)value["symbol"]), StringComparer.Ordinal);
            string[] retained = SummonPlaytestWithdrawalPolicy.All.SelectMany(value =>
                    new[] { value.UnitSymbol }.Concat(value.FactSymbols).Concat(value.AiActionSymbols)
                        .Concat(value.ResourceSymbols))
                .Concat(new[] {
                    "KMG.Summoning.Infernal.BoneDevil.WallSectionUnit",
                    "KMG.Summoning.Infernal.BoneDevil.WallResidualArea",
                    "KMG.Summoning.Infernal.BoneDevil.WallSectionTraits",
                    "KMG.Summoning.Infernal.IceDevil.WallSectionUnit",
                    "KMG.Summoning.Infernal.IceDevil.WallResidualArea",
                    "KMG.Summoning.Infernal.IceDevil.WallSectionTraits",
                    "KMG.Summoning.Special.MagmaMephit.MagmaFormState",
                    "KMG.Summoning.Unit.Stirge",
                    "KMG.Summoning.Special.Stirge.Hold",
                    "KMG.Summoning.Special.Stirge.Remove" }).ToArray();
            foreach (string symbol in retained)
            {
                Assertions.True(registered.Contains(symbol), symbol + " stays a registered identity.");
                Assertions.True(active.Contains(symbol), symbol + " stays active in the frozen manifest.");
            }
            foreach (SummonVariantSpec stirge in All().Where(value =>
                value.Creature.Key == SummonPlaytestWithdrawalPolicy.StirgeKey))
            {
                string symbol = ExpandedSummoningIdentityCatalog.AbilitySymbol(stirge);
                Assertions.True(registered.Contains(symbol) && active.Contains(symbol),
                    "Withheld Stirge root " + symbol + " still resolves for a saved reference.");
            }
            Assertions.Equal(IceDevilRules40Policy.WallSectionSymbol,
                "KMG.Summoning.Infernal.IceDevil.WallSectionUnit", "Ice Devil wall section identity is unchanged.");
            string[] names = SummonPlaytestWithdrawalPolicy.All.Select(value =>
                SummonPlaytestWithdrawalPolicy.FixerName(value.CreatureKey)).ToArray();
            Assertions.Equal(names.Length, names.Distinct(StringComparer.Ordinal).Count(),
                "Each load fixer has its own once-per-unit name.");
            Assertions.Equal("$KMG_R1_WithdrawnCapabilities_FireGiant",
                SummonPlaytestWithdrawalPolicy.FixerName("fire-giant"),
                "Fixer names are fixed: the native fixer records them per unit.");
        }

        // CUT-03 and CUT-05 decisions, with the retained neighbours.
        internal static void OnlyTheNamedCapabilitiesAreWithdrawn()
        {
            Assertions.True(SummonPlaytestWithdrawalPolicy.IsWithdrawnMephitSpellLike("magma-mephit", "MagmaForm"),
                "Magma Form is withdrawn.");
            Assertions.False(SummonPlaytestWithdrawalPolicy.IsWithdrawnMephitSpellLike("magma-mephit", "Pyrotechnics"),
                "Magma Mephit keeps Pyrotechnics.");
            foreach (MephitVariantProfile profile in ExpandedSummoningSpecialProfiles.MephitVariants)
                foreach (string kind in new[] { profile.SpellLikeOne, profile.SpellLikeTwo }.Where(value => value != null))
                    Assertions.Equal(profile.Key == "magma-mephit" && kind == "MagmaForm",
                        SummonPlaytestWithdrawalPolicy.IsWithdrawnMephitSpellLike(profile.Key, kind),
                        profile.Key + " " + kind + " withdrawal matches the owner's single cut.");
            Assertions.Equal("MagmaForm", ExpandedSummoningSpecialProfiles.MephitVariant("magma-mephit").SpellLikeTwo,
                "The Magma Form profile slot itself is retained, so its identities still allocate.");

            const string sunder = SummonPlaytestWithdrawalPolicy.NativeImprovedSunderGuid;
            const string cleave = "cc9c862ef2e03af4f89be5088851ea35";
            string[] fire = { cleave, sunder, "175d1577bb6c9a04baf88eec99c66334", "9972f33f977fc724c838e59641b2fca5" };
            string[] granted = SummonPlaytestWithdrawalPolicy.GrantedNativeFeats("fire-giant", fire);
            Assertions.True(granted.SequenceEqual(new[] { cleave, "175d1577bb6c9a04baf88eec99c66334",
                "9972f33f977fc724c838e59641b2fca5" }), "Fire Giant loses only Improved Sunder; Great Cleave and its other feats stay.");
            Assertions.True(SummonPlaytestWithdrawalPolicy.GrantedNativeFeats("fire-giant",
                    new[] { sunder.ToUpperInvariant() }).Length == 0,
                "The withdrawn feat is matched regardless of GUID case.");
            Assertions.True(SummonPlaytestWithdrawalPolicy.GrantedNativeFeats("storm-giant", new[] { sunder })
                .SequenceEqual(new[] { sunder }), "Storm Giant keeps its separate Improved Sunder grant.");
            Assertions.True(SummonPlaytestWithdrawalPolicy.GrantedNativeFeats("hill-giant", fire)
                .SequenceEqual(fire), "No other giant's feat list changes.");
            Assertions.True(SummonPlaytestWithdrawalPolicy.For("fire-giant").FactSymbols.Count == 0 &&
                SummonPlaytestWithdrawalPolicy.For("fire-giant").AiActionSymbols.Count == 0,
                "The Fire Giant cut withdraws a native feat source, not a hidden hotbar action.");
            Assertions.True(SummonPlaytestWithdrawalPolicy.For("lillend-azata").FactSymbols
                .SequenceEqual(new[] { "KMG.Summoning.FeyCelestial.LillendAzata.BardicSuggestion" }),
                "Lillend loses only Bardic Suggestion, never its Suggestion spell.");
            Assertions.Equal(null, SummonPlaytestWithdrawalPolicy.For("storm-giant"), "Storm Giant has no withdrawal.");
            Assertions.Equal(null, SummonPlaytestWithdrawalPolicy.For("satyr"), "Satyr's Suggestion is untouched.");
        }

        private sealed class Ref { internal Ref(string name) { Name = name; } internal string Name; }

        // The load-time reconciliation removes exactly the withdrawn
        // identities from the exact owner, and nothing from anyone else.
        internal static void LoadReconciliationIsExactOwnerAndExactIdentity()
        {
            var wall = new Ref("WallOfIce"); var wallAi = new Ref("WallAi");
            var cone = new Ref("ConeOfCold"); var coneAi = new Ref("ConeAi");
            var lookalike = new Ref("WallOfIce");
            Ref[] saved = { cone, wall, coneAi, wallAi, null };
            Ref[] withdrawn = { wall, wallAi };
            Ref[] removed = SummonPlaytestWithdrawalPolicy.SelectWithdrawn(true, saved, withdrawn);
            Assertions.True(removed.SequenceEqual(new[] { wall, wallAi }),
                "An older saved owner loses exactly its withdrawn grant and AI action.");
            Assertions.Equal(0, SummonPlaytestWithdrawalPolicy.SelectWithdrawn(false, saved, withdrawn).Length,
                "A unit that is not the exact owning blueprint is never touched.");
            Assertions.Equal(0, SummonPlaytestWithdrawalPolicy.SelectWithdrawn(true, new[] { lookalike, cone }, withdrawn).Length,
                "A same-named different object is not matched.");
            Assertions.Equal(0, SummonPlaytestWithdrawalPolicy.SelectWithdrawn(true, new[] { cone, coneAi }, withdrawn).Length,
                "A fresh summon without the withdrawn facts is unchanged.");
            Assertions.Equal(0, SummonPlaytestWithdrawalPolicy.SelectWithdrawn<Ref>(true, saved, null).Length,
                "Missing configuration fails closed.");
            Assertions.Equal(0, SummonPlaytestWithdrawalPolicy.SelectWithdrawn(true, null, withdrawn).Length,
                "A unit with no saved facts is unchanged.");
        }
    }
}
