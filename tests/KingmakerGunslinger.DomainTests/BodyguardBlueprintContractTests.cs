using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KingmakerGunslinger.BodyguardFeats;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class BodyguardBlueprintContractTests
    {
        private static readonly IDictionary<string, string> ExpectedIdentities =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { "KMG.Feats.Bodyguard", "b2baa3384b4d4328848cc07933b513be" },
                { "KMG.Feats.UseBodyguard", "ac31a9d5d34140978b7e778dc8d1e226" },
                { "KMG.Feats.BodyguardModeMarker", "a78147a3655f429883ad88e761ff9438" },
                { "KMG.Feats.InHarmsWay", "e481f30c8b6940e1b596e121443aa01e" },
                { "KMG.Feats.UseInHarmsWay", "ca1e74f0e60747209a8b7cf3737243ea" },
                { "KMG.Feats.InHarmsWayModeMarker", "57603d0b215e4ac6862bcdf9b5583568" },
                { "KMG.Feats.InHarmsWayImmediatePending", "a92164067bad3a85b1da48db5a787686" },
                { "KMG.Feats.InHarmsWayImmediateChargedTurn", "326e183f7791e83a38337c6a6d7a8644" },
                { "KMG.Traits.HelpfulCombat", "e4b29a7c8d5f4c1796ab03e1f72d8456" }
            };

        internal static void ManifestIdentitiesAreExact()
        {
            JObject manifest = JObject.Parse(Read("blueprints", "blueprints.json"));
            JToken[] entries = manifest["entries"].ToArray();
            Assertions.Equal(1784, entries.Length,
                "Current blueprint ledger count changed.");
            Assertions.Equal(1782, entries.Count(value => string.Equals(
                (string)value["status"], "active", StringComparison.Ordinal)),
                "Current active identity count changed.");
            Assertions.Equal(entries.Length, entries.Select(value =>
                (string)value["symbol"]).Distinct(StringComparer.Ordinal).Count(),
                "Blueprint symbols are not globally unique.");
            Assertions.Equal(entries.Length, entries.Select(value =>
                (string)value["guid"]).Distinct(StringComparer.Ordinal).Count(),
                "Blueprint GUIDs are not globally unique.");
            foreach (KeyValuePair<string, string> expected in ExpectedIdentities)
            {
                JToken[] matches = entries.Where(value => string.Equals(
                    (string)value["symbol"], expected.Key,
                    StringComparison.Ordinal)).ToArray();
                Assertions.Equal(1, matches.Length,
                    "Bodyguard identity is missing or duplicated: " + expected.Key);
                Assertions.Equal(expected.Value, (string)matches[0]["guid"],
                    "Bodyguard GUID changed: " + expected.Key);
                Assertions.Equal("active", (string)matches[0]["status"],
                    "Bodyguard identity is not active: " + expected.Key);
                Assertions.True(expected.Value.Length == 32 &&
                    expected.Value.All(value => value >= '0' && value <= '9' ||
                        value >= 'a' && value <= 'f'),
                    "Bodyguard GUID format changed: " + expected.Key);
            }
        }

        internal static void NativeAndFeatContractsAreExact()
        {
            string source = Read("src", "KingmakerGunslinger", "Blueprints",
                "BodyguardFeatBlueprints.cs");
            foreach (string token in new[]
            {
                "0f8939ae6f220984e8fb568abbdfba95",
                "CombatReflexesInternalName = \"CombatReflexes\"",
                "BlueprintLibraryLookup.RequireExact<",
                "ValidateCombatReflexes(combatReflexes)",
                "UnitCondition.AttackOfOpportunityBeforeInitiative",
                "FeatureGroup.Feat, FeatureGroup.CombatFeat",
                "feature.Ranks = 1",
                "feature.HideInUI = false",
                "feature.IsClassFeature = false",
                "prerequisiteComponent.Feature = prerequisite",
                "prerequisiteComponent.Group = Prerequisite.GroupType.All",
                "grant.Facts = new[] { mode }",
                "BodyguardSymbol = \"KMG.Feats.Bodyguard\"",
                "InHarmsWaySymbol = \"KMG.Feats.InHarmsWay\""
            })
                Assertions.True(source.Contains(token),
                    "Bodyguard feat/native source contract lacks: " + token);
            Assertions.True(source.Contains(
                    "CreateBodyguard(combatReflexes") &&
                source.Contains("CreateInHarmsWay(bodyguard") &&
                !source.Contains("GetAllBlueprints()") &&
                !source.Contains("DisplayName =="),
                "Bodyguard prerequisites are not bound by exact blueprint identity.");
        }

        internal static void ModeContractsArePersistentAndIndependent()
        {
            string source = Read("src", "KingmakerGunslinger", "Blueprints",
                "BodyguardModeBlueprints.cs");
            foreach (string token in new[]
            {
                "BodyguardDisplayName = \"Use Bodyguard\"",
                "InHarmsWayDisplayName = \"Use In Harm's Way\"",
                "ability.Group = ActivatableAbilityGroup.None",
                "ability.IsOnByDefault = false",
                "ability.ActivationType = AbilityActivationType.Immediately",
                "ability.DeactivateIfCombatEnded = false",
                "ability.DeactivateAfterFirstRound = false",
                "ability.DeactivateImmediately = true",
                "ability.OnlyInCombat = false",
                "Enum.Parse(flags.FieldType, \"HiddenInUi\")",
                "marker.ComponentsArray = Array.Empty<BlueprintComponent>()",
                "ability.ComponentsArray = Array.Empty<BlueprintComponent>()"
            })
                Assertions.True(source.Contains(token),
                    "Bodyguard persistent-mode contract lacks: " + token);
            Assertions.True(source.Contains("attack of opportunity is spent even if") &&
                source.Contains("full damage and associated effects") &&
                !source.Contains("OnlyInCombat = true") &&
                !source.Contains("IsOnByDefault = true"),
                "Bodyguard mode presentation or defaults changed.");
            Assertions.True(source.Contains(
                    "!ability.DeactivateImmediately") &&
                source.Contains("OnTurnOff path can leave IsRunning") &&
                source.Contains("marker buff active"),
                "Bodyguard mode validation does not enforce immediate opt-out and marker synchronization.");
        }

        internal static void PublicationIsAtomicAndDeterministic()
        {
            var alpha = new Fixture("native-alpha", "Alpha");
            var omega = new Fixture("native-omega", "Omega");
            var bodyguard = new Fixture("bodyguard", "Bodyguard");
            var inHarmsWay = new Fixture("in-harms-way", "In Harm's Way");
            var arrays = new[] {
                new[] { alpha, omega }, new[] { alpha, omega },
                new[] { alpha, omega }, new[] { alpha, omega } };
            Fixture[][] originals = arrays.Select(value => value).ToArray();
            BodyguardPublicationSurface<Fixture>[] surfaces = Surfaces(arrays);
            BodyguardFeatPublicationTransaction<Fixture> first =
                BodyguardFeatPublicationTransaction<Fixture>.Publish(surfaces,
                    new[] { inHarmsWay, bodyguard }, value => value.Id,
                    value => value.Name);
            foreach (Fixture[] values in arrays)
            {
                Assertions.True(values.SequenceEqual(new[] { alpha, bodyguard,
                    inHarmsWay, omega }),
                    "Feat insertion order is not deterministic.");
                AssertSingular(values, bodyguard, inHarmsWay);
            }

            BodyguardFeatPublicationTransaction<Fixture> repeated =
                BodyguardFeatPublicationTransaction<Fixture>.Publish(surfaces,
                    new[] { bodyguard, inHarmsWay }, value => value.Id,
                    value => value.Name);
            foreach (Fixture[] values in arrays) AssertSingular(values,
                bodyguard, inHarmsWay);
            repeated.Rollback();
            first.Rollback();
            for (int index = 0; index < arrays.Length; index++)
                Assertions.True(ReferenceEquals(arrays[index], originals[index]),
                    "Rollback did not restore an exact original array reference.");
        }

        internal static void PartialFailureRestoresEverySurface()
        {
            var native = new Fixture("native", "Native");
            var bodyguard = new Fixture("bodyguard", "Bodyguard");
            var inHarmsWay = new Fixture("in-harms-way", "In Harm's Way");
            var arrays = new[] { new[] { native }, new[] { native },
                new[] { native }, new[] { native } };
            Fixture[][] originals = arrays.Select(value => value).ToArray();
            bool failed = false;
            try
            {
                BodyguardFeatPublicationTransaction<Fixture>.Publish(Surfaces(arrays),
                    new[] { bodyguard, inHarmsWay }, value => value.Id,
                    value => value.Name, stage =>
                    {
                        if (stage == 2) throw new InvalidOperationException("fixture");
                    });
            }
            catch (InvalidOperationException) { failed = true; }
            Assertions.True(failed,
                "Injected second-selection publication failure did not fail closed.");
            for (int index = 0; index < arrays.Length; index++)
                Assertions.True(ReferenceEquals(arrays[index], originals[index]),
                    "Partial failure did not restore all original surfaces.");
        }

        internal static void GuidConflictFailsBeforeMutation()
        {
            var native = new Fixture("native", "Native");
            var bodyguard = new Fixture("bodyguard", "Bodyguard");
            var conflict = new Fixture("bodyguard", "Foreign conflict");
            var inHarmsWay = new Fixture("in-harms-way", "In Harm's Way");
            var arrays = new[] { new[] { native }, new[] { native, conflict },
                new[] { native }, new[] { native } };
            Fixture[][] originals = arrays.Select(value => value).ToArray();
            bool failed = false;
            try
            {
                BodyguardFeatPublicationTransaction<Fixture>.Publish(Surfaces(arrays),
                    new[] { bodyguard, inHarmsWay }, value => value.Id,
                    value => value.Name);
            }
            catch (InvalidOperationException) { failed = true; }
            Assertions.True(failed, "A foreign same-GUID feat did not fail closed.");
            for (int index = 0; index < arrays.Length; index++)
                Assertions.True(ReferenceEquals(arrays[index], originals[index]),
                    "Preflight GUID conflict mutated a publication surface.");
        }

        internal static void RuntimeAdapterTargetsExactSelections()
        {
            string source = Read("src", "KingmakerGunslinger", "Blueprints",
                "BodyguardFeatCatalogPublication.cs");
            foreach (string token in new[]
            {
                "247a4068296e8be42890143f451b4b45",
                "41c8486641f7d6d4283ca9dae4147a9f",
                "basic.Features", "basic.AllFeatures", "fighter.Features",
                "fighter.AllFeatures", "set.Bodyguard, set.InHarmsWay",
                "_transaction.Rollback()"
            })
                Assertions.True(source.Contains(token),
                    "Bodyguard publication adapter lacks: " + token);
            string bootstrap = Read("src", "KingmakerGunslinger", "Bootstrap",
                "BlueprintBootstrap.cs");
            Assertions.True(bootstrap.Contains(
                    "BodyguardFeatBlueprints.Register(library, registry)") &&
                bootstrap.Contains("if (publicationPlan.BodyguardFeats)") &&
                bootstrap.Contains("bodyguardFeatPublication.Rollback()") &&
                bootstrap.Contains("ExpectedRegisteredBlueprintCount = 341 + 1 +"),
                "Bodyguard always-register/module-gated publication wiring is incomplete.");
        }

        private static BodyguardPublicationSurface<Fixture>[] Surfaces(
            Fixture[][] arrays)
        {
            var result = new BodyguardPublicationSurface<Fixture>[arrays.Length];
            for (int index = 0; index < arrays.Length; index++)
            {
                int captured = index;
                result[index] = new BodyguardPublicationSurface<Fixture>(
                    "surface-" + index, () => arrays[captured],
                    value => arrays[captured] = value);
            }
            return result;
        }

        private static void AssertSingular(Fixture[] values, params Fixture[] feats)
        {
            foreach (Fixture feat in feats)
            {
                Assertions.Equal(1, values.Count(value => ReferenceEquals(value, feat)),
                    "Published feat is not singular by reference.");
                Assertions.Equal(1, values.Count(value => string.Equals(value.Id,
                    feat.Id, StringComparison.Ordinal)),
                    "Published feat is not singular by GUID.");
            }
        }

        private static string Read(params string[] parts)
        {
            string path = Environment.CurrentDirectory;
            foreach (string part in parts) path = Path.Combine(path, part);
            return File.ReadAllText(path);
        }

        private sealed class Fixture
        {
            internal Fixture(string id, string name) { Id = id; Name = name; }
            internal string Id { get; private set; }
            internal string Name { get; private set; }
        }
    }
}
