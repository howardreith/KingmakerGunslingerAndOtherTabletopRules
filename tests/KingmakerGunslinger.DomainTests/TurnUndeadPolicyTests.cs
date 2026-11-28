using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KingmakerGunslinger.FeatureModules;
using KingmakerGunslinger.TurnUndead;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class TurnUndeadPolicyTests
    {
        private const string NegativeClericChannelFeature = "3adb2c906e031ee41a01bfc1d5fb7eea";
        private const string ChannelPositiveAllowed = "8c769102f3996684fb6e09a2c4e7e5b9";

        internal static void SourceCatalogMatchesVerifiedNativeContracts()
        {
            TurnUndeadSourceContract[] all = TurnUndeadSourceCatalog.All.ToArray();
            Assertions.Equal(4, all.Length, "Exactly four native positive-channel sources.");
            var expected = new[]
            {
                new { Kind = TurnUndeadSourceKind.Cleric, Feature = "a79013ff4bcd4864cb669622a29ddafb",
                    Resource = "5e2bba3e07c37be42909a12945c27de7", Cost = 1,
                    Class = "67819271767a9dd4fbfd4ae700befea0", Stat = TurnUndeadDcStat.Charisma,
                    Harm = "279447a6bf2d3544d93a0a39c3b8e91d", Lawful = false },
                new { Kind = TurnUndeadSourceKind.Paladin, Feature = "cb6d55dda5ab906459d18a435994a760",
                    Resource = "9dedf41d995ff4446a181f143c3db98c", Cost = 2,
                    Class = "bfa11238e7ae3544bbeb4d0b92e897ec", Stat = TurnUndeadDcStat.Charisma,
                    Harm = "4937473d1cfd7774a979b625fb833b47", Lawful = true },
                new { Kind = TurnUndeadSourceKind.Hospitaler, Feature = "a9ab1bbc79ecb174d9a04699986ce8d5",
                    Resource = "b0e0c7716ab27c64fb4b131c9845c596", Cost = 1,
                    Class = "bfa11238e7ae3544bbeb4d0b92e897ec", Stat = TurnUndeadDcStat.Charisma,
                    Harm = "cc17243b2185f814aa909ac6b6599eaa", Lawful = true },
                new { Kind = TurnUndeadSourceKind.Empyreal, Feature = "7d49d7f590dc9a948b3bd1c8b7979854",
                    Resource = "f9af9354fb8a79649a6e512569387dc5", Cost = 1,
                    Class = "b3a505fb61437dc4097f43c3f8f9a4cf", Stat = TurnUndeadDcStat.Wisdom,
                    Harm = "e1536ee240c5d4141bf9f9485a665128", Lawful = false },
            };
            foreach (var row in expected)
            {
                TurnUndeadSourceContract source = TurnUndeadSourceCatalog.Require(row.Kind);
                Assertions.True(source.SourceFeatureGuid == row.Feature &&
                    source.ResourceGuid == row.Resource && source.ResourceCost == row.Cost &&
                    source.DcClassGuid == row.Class && source.DcStat == row.Stat &&
                    source.NativeHarmGuid == row.Harm && source.RequiresLawfulGood == row.Lawful,
                    "Source contract drifted for " + row.Kind + ".");
            }
            Assertions.Equal(4, all.Select(value => value.ResourceGuid).Distinct().Count(),
                "Each source spends its own existing pool; no pool is shared or created.");
            Assertions.True(all.All(value => value.ResourceGuid !=
                    TurnUndeadSourceCatalog.WizardTurnUndeadResourceGuid &&
                    value.SourceFeatureGuid != TurnUndeadSourceCatalog.WizardTurnUndeadGuid),
                "The wizard necromancer turning power is not a Turn Undead source.");
            string[] symbols = TurnUndeadSourceCatalog.AllSymbols().ToArray();
            Assertions.Equal(SummoningCurrentSurface.TurnUndeadLedgerIdentityCount, symbols.Length,
                "Feat, flight buff and two actions per source.");
            JToken[] manifest = JObject.Parse(File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "blueprints", "blueprints.json")))["entries"].ToArray();
            foreach (string symbol in symbols)
            {
                JToken entry = manifest.Single(value => (string)value["symbol"] == symbol);
                string type = symbol == TurnUndeadSourceCatalog.FeatSymbol ? "BlueprintFeature" :
                    symbol == TurnUndeadSourceCatalog.FleeingBuffSymbol ? "BlueprintBuff" : "BlueprintAbility";
                Assertions.True((string)entry["status"] == "active" &&
                    (string)entry["plannedType"] == type &&
                    (string)entry["milestone"] == "Turn Undead feat",
                    "Manifest identity drifted: " + symbol);
            }
            Assertions.True(manifest.Skip(manifest.Length - symbols.Length)
                    .Select(value => (string)value["symbol"]).SequenceEqual(symbols),
                "Turn Undead identities are the exact append-only manifest tail.");
            Assertions.Equal(10, symbols.Distinct(StringComparer.Ordinal).Count(),
                "Source-specific child identities never share a symbol.");
        }

        internal static void EligibilityFollowsActualPositiveSourcesOnly()
        {
            Assertions.Equal(0, TurnUndeadSourceCatalog.Owned(guid => guid ==
                    NegativeClericChannelFeature || guid == ChannelPositiveAllowed).Length,
                "A negative-only channel or a positive-permission fact never qualifies.");
            Assertions.Equal(0, TurnUndeadSourceCatalog.Owned(guid => false).Length,
                "A character with no channel feature has no source.");
            TurnUndeadSourceContract[] multiclass = TurnUndeadSourceCatalog.Owned(guid =>
                guid == "a79013ff4bcd4864cb669622a29ddafb" || guid == "cb6d55dda5ab906459d18a435994a760");
            Assertions.True(multiclass.Length == 2 &&
                multiclass[0].Kind == TurnUndeadSourceKind.Cleric &&
                multiclass[1].Kind == TurnUndeadSourceKind.Paladin &&
                multiclass[0].DirectAbilitySymbol != multiclass[1].DirectAbilitySymbol &&
                multiclass[0].ResourceGuid != multiclass[1].ResourceGuid,
                "Two sources give two explicit actions with separate pools.");
        }

        internal static void SaveDcUsesOnlyTheSourceClassLevelAndStatistic()
        {
            Assertions.Equal(10, TurnUndeadRules.SaveDc(1, 0, 0), "Level 1, +0.");
            Assertions.Equal(11, TurnUndeadRules.SaveDc(2, 0, 0), "Even level halves exactly.");
            Assertions.Equal(11, TurnUndeadRules.SaveDc(3, 0, 0), "Odd level floors.");
            Assertions.Equal(17, TurnUndeadRules.SaveDc(9, 3, 0), "Cleric 9, Charisma 16: 10 + 4 + 3.");
            Assertions.Equal(8, TurnUndeadRules.SaveDc(1, -2, 0), "A negative modifier lowers the DC.");
            Assertions.Equal(19, TurnUndeadRules.SaveDc(9, 3, 2), "A genuine bonus adds once.");
            bool refused = false;
            try { TurnUndeadRules.SaveDc(0, 3, 0); }
            catch (ArgumentOutOfRangeException) { refused = true; }
            Assertions.True(refused, "A source requires a level in its class.");
        }

        internal static void RepeatSavesAreIntelligentOnly()
        {
            Assertions.Equal(TurnUndeadRoundDecision.NotOwned, TurnUndeadRules.DecideRound(false, false),
                "An uninitialized effect owns no round save.");
            Assertions.Equal(TurnUndeadRoundDecision.NoRepeatSave, TurnUndeadRules.DecideRound(true, true),
                "Mindless undead receive no repeat save.");
            Assertions.Equal(TurnUndeadRoundDecision.RollRepeatSave, TurnUndeadRules.DecideRound(true, false),
                "Intelligent undead roll each round.");
        }

        internal static void ClassificationMatchesReviewedNativeInventory()
        {
            JObject fixture = JObject.Parse(File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "tests", "fixtures", "turn-undead",
                "native-undead-classification.json")));
            JToken[] rows = fixture["rows"].ToArray();
            Assertions.True(rows.Length >= 130, "The reviewed native inventory is incomplete.");
            int mindless = 0;
            foreach (JToken row in rows)
            {
                var record = new TurnUndeadUnitRecord((string)row["unit"], (string)row["typeGuid"],
                    row["identityFacts"].Select(value => (string)value));
                TurnUndeadClassification result = TurnUndeadClassificationPolicy.Classify(record);
                string expected = (string)row["expectedMind"];
                Assertions.Equal(expected, result.Mind.ToString(),
                    "Classification of " + row["name"] + " (" + row["unit"] + ") disagrees with review: " +
                    row["reviewReason"] + "; rule=" + result.Rule);
                if (result.Mind == TurnUndeadMind.Mindless) mindless++;
            }
            Assertions.True(mindless >= 30, "The inventory lost its mindless families.");
        }

        internal static void IntelligenceNameAndBrainAreNotEvidence()
        {
            // Ordinary skeleton and zombie both have Intelligence 10 and share
            // "Dumb Monster Brain" with the zombie lord (Intelligence 12).
            var skeleton = Classify("e8658c2a211043e4b983a666d3c7068e",
                TurnUndeadClassificationPolicy.SkeletonType);
            var zombie = Classify("4c44d7b485cbc8741a1c91affd8a03f3",
                TurnUndeadClassificationPolicy.ZombieType);
            var lord = Classify("a2803487ba242f144a3df7c672cf58ac",
                TurnUndeadClassificationPolicy.ZombieType, "8464aa8993bc72343918be0e0c42874a");
            var champion = Classify("f2917899f6976fc4ebc16724a592d0b7",
                TurnUndeadClassificationPolicy.SkeletonType);
            Assertions.True(skeleton.Mind == TurnUndeadMind.Mindless &&
                zombie.Mind == TurnUndeadMind.Mindless, "Int-10 skeleton/zombie stay mindless.");
            Assertions.True(lord.Mind == TurnUndeadMind.Intelligent &&
                lord.Rule == TurnUndeadClassificationPolicy.RuleIntelligentFact,
                "A zombie lord sharing the zombie type and brain is intelligent by identity.");
            Assertions.True(champion.Mind == TurnUndeadMind.Intelligent &&
                champion.Rule == TurnUndeadClassificationPolicy.RuleUnitException,
                "A skeletal champion inside the Skeleton type is intelligent.");
            // The same zombie-lord unit loses its intelligence only if its
            // identity fact is absent; a renamed copy keeps it.
            var unknownUnitWithMarker = Classify("00000000000000000000000000000001",
                TurnUndeadClassificationPolicy.ZombieType, "97db9c223d908ce42a26e2ed04b54129");
            Assertions.Equal(TurnUndeadMind.Intelligent, unknownUnitWithMarker.Mind,
                "Identity facts classify unseen units of a mindless family.");
            var thirdParty = Classify("00000000000000000000000000000002", "0123456789abcdef0123456789abcdef");
            Assertions.True(thirdParty.Mind == TurnUndeadMind.Intelligent &&
                thirdParty.Rule == TurnUndeadClassificationPolicy.RuleUnclassified,
                "Unknown content keeps native every-round saves and is reported as unclassified.");
            var review = Classify("976d4937eb2620c4baf465bb6c7a00a7",
                TurnUndeadClassificationPolicy.SkeletonType);
            Assertions.True(review.Mind == TurnUndeadMind.Mindless && review.OwnerReview,
                "Owner-review defaults are deterministic and flagged.");
        }

        internal static void ForeignListAppendIsIdempotentAndPreserving()
        {
            object a = new object(), b = new object(), turn1 = new object(), turn2 = new object();
            object[] before = { a, b };
            object[] after = TurnUndeadListPolicy.AppendMissing(before, new[] { turn1, turn2 });
            Assertions.True(after != null && after.Length == 4 && ReferenceEquals(after[0], a) &&
                ReferenceEquals(after[1], b) && ReferenceEquals(after[2], turn1) &&
                ReferenceEquals(after[3], turn2), "Existing entries and order are preserved.");
            Assertions.True(TurnUndeadListPolicy.AppendMissing(after, new[] { turn1, turn2 }) == null,
                "Repeated publication appends nothing.");
            object[] partial = TurnUndeadListPolicy.AppendMissing(new[] { a, turn2 }, new[] { turn1, turn2 });
            Assertions.True(partial.Length == 3 && partial.Count(value => ReferenceEquals(value, turn2)) == 1,
                "A partially present list gains only the missing identity.");
            Assertions.Equal(2, TurnUndeadListPolicy.AppendMissing<object>(null, new[] { turn1, turn2 }).Length,
                "A null list is treated as empty.");
        }

        internal static void SettingsKeyDefaultsOnWithoutSchemaMigration()
        {
            string path = Path.Combine(Path.GetTempPath(), "kmg-turn-undead-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            try
            {
                string settings = Path.Combine(path, FeatureModuleSettingsStore.FileName);
                Assertions.True(FeatureModuleSettingsStore.Load(path).ActiveTurnUndead,
                    "Missing settings offer Turn Undead.");
                File.WriteAllText(settings, "{\"schemaVersion\":12,\"bodyguard-feats\":false}");
                FeatureModuleSettingsState existing = FeatureModuleSettingsStore.Load(path);
                Assertions.True(existing.ActiveTurnUndead && existing.Source == "settings" &&
                    !existing.Active.BodyguardFeats, "A schema-12 file without the key is ON and not migrated.");
                existing.SetPendingTurnUndead(false);
                Assertions.True(existing.RestartRequired && existing.ActiveTurnUndead,
                    "Turning the feat off applies only after restart.");
                FeatureModuleSettingsStore.Save(existing);
                JObject saved = JObject.Parse(File.ReadAllText(settings));
                Assertions.True((int)saved["schemaVersion"] == FeatureModuleSettingsStore.CurrentSchemaVersion &&
                    saved[FeatureModuleSettingsStore.TurnUndeadId].Type == JTokenType.Boolean &&
                    !(bool)saved[FeatureModuleSettingsStore.TurnUndeadId] &&
                    !(bool)saved["bodyguard-feats"], "The key saves beside unchanged modules.");
                FeatureModuleSettingsState reloaded = FeatureModuleSettingsStore.Load(path);
                Assertions.True(!reloaded.ActiveTurnUndead && !reloaded.RestartRequired &&
                    reloaded.Active.Equals(existing.Active), "OFF round-trips without touching modules.");
                File.WriteAllText(settings, "{\"schemaVersion\":12,\"turn-undead\":\"no\"}");
                FeatureModuleSettingsState malformed = FeatureModuleSettingsStore.Load(path, value => { });
                Assertions.True(malformed.Recovered && malformed.ActiveTurnUndead,
                    "A malformed value uses the existing quarantined recovery.");
            }
            finally { Directory.Delete(path, true); }
        }

        private static TurnUndeadClassification Classify(string unit, string type,
            params string[] facts)
        { return TurnUndeadClassificationPolicy.Classify(new TurnUndeadUnitRecord(unit, type, facts)); }
    }
}
