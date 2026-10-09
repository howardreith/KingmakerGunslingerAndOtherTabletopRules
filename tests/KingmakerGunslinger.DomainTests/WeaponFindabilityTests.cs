using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using KingmakerGunslinger.Acquisition;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class WeaponFindabilityTests
    {
        private class IconFeature { internal string Guid; }
        private sealed class IconSelection : IconFeature { }

        internal static void NodachiObserverUsesExactSelectionAndChoiceReaders()
        {
            const string selectionGuid = "5ae9f898e45846d19d3802caf91e06b6";
            string[] choiceGuids = { "af205733f7fe49838edb37cdf1b90cbb",
                "4caf60ed8b264701a3965288a65eebc2", "e17fafa6f75641f8a2e3fe4b6f71da78" };
            int selections = 0, features = 0;
            var selection = new IconSelection { Guid = selectionGuid };
            var choices = choiceGuids.Select(guid => new IconFeature { Guid = guid }).ToArray();
            var observed = RuntimeTesting.HeirloomNodachiIconObservation.ReadConsumers<IconFeature, IconSelection>(
                guid => { Assertions.Equal(selectionGuid, guid, "Only the selection uses the exact selection reader."); selections++; return selection; },
                guid => { Assertions.False(guid == selectionGuid, "Exact feature lookup must never receive the derived selection."); features++; return choices.Single(value => value.Guid == guid); });
            Assertions.Equal(1, selections, "Selection is read once with its actual type.");
            Assertions.Equal(3, features, "All three visible choices use exact feature reads.");
            Assertions.True(observed.SequenceEqual(new IconFeature[] { selection }.Concat(choices)),
                "Read-only observation preserves all four actual references and deterministic order.");
            Assertions.Equal(4, observed.Select(value => value.Guid).Distinct().Count(), "No consumer is duplicated or omitted.");
        }

        internal static void CuratedCountsRequireTheirOwnMeasuredGates()
        {
            var report = JObject.Parse(File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "validation", "weapon-findability-runtime-qualification.json")));
            var rows = ((JArray)report["weapons"]).OfType<JObject>().ToArray();
            Assertions.Equal(29, rows.Length, "Curated qualification must cover the complete named registry.");
            Assertions.Equal(29, rows.Select(value => (string)value["targetGuid"]).Distinct().Count(), "No weapon may be omitted or double-counted.");
            var runs = (JObject)report["runs"];
            foreach (string gate in new[] { "blueprint", "nativeTreasure", "scene", "route", "pickup", "revisit", "saveReload" })
            {
                Assertions.Equal(rows.Count(value => (string)value["gates"][gate] == "PASS"),
                    (int)report["counts"][gate], "Evidence counts must be derived from individual gates: " + gate);
                foreach (var row in rows.Where(value => (string)value["gates"][gate] == "PASS"))
                {
                    string run = (string)row["provenance"][gate];
                    Assertions.True(run != null && runs[run] != null, "A passing gate needs exact sampled runtime provenance: " + gate);
                    if (gate == "scene")
                    {
                        var objects = ((JArray)row["sceneObjects"]).OfType<JObject>();
                        Assertions.True(objects.Any(value => !string.IsNullOrWhiteSpace((string)value["entityId"]) &&
                            !string.IsNullOrWhiteSpace((string)value["scene"]) &&
                            (bool?)value["activeInHierarchy"] == true && (bool?)value["isInGame"] == true &&
                            (bool?)value["persistentStaticObject"] == true &&
                            value["position"] is JArray && ((JArray)value["position"]).Count == 3),
                            "Scene PASS requires an identified active persistent entity and measured position; unresolved evidence cannot pass.");
                    }
                    if (gate == "saveReload")
                    {
                        Assertions.Equal("PASS", (string)runs[run]["result"], "Disk persistence requires the complete verification run to pass.");
                        Assertions.Equal(0, (int)row["persistence"]["sourceWeaponCount"], "The picked-up source must stay depleted.");
                        Assertions.Equal(1, (int)row["persistence"]["ownedWeaponCount"], "Reload must retain exactly one picked-up canonical weapon.");
                        Assertions.True((bool)row["persistence"]["freshProcessReadOnly"], "The verification process must not write a save.");
                    }
                }
            }
        }
        internal static void PersistenceOnlyWritesItsNewOwnedDescriptor()
        {
            const string transaction = "20261008T0200000000000Z_0123456789abcdef0123456789abcdef";
            string name = RuntimeTesting.WeaponFindabilitySaveContract.Name(transaction);
            Assertions.True(RuntimeTesting.WeaponFindabilitySaveContract.MayWrite(transaction, "prepare", name, true, true), "Exact leased prepare may save its disposable copy.");
            Assertions.False(RuntimeTesting.WeaponFindabilitySaveContract.MayWrite(transaction, "verify", name, true, true), "Reload phase is read-only.");
            Assertions.False(RuntimeTesting.WeaponFindabilitySaveContract.MayWrite(transaction, "prepare", "KMG_AUTOMATION_WORKING", true, true), "The seed cannot be overwritten.");
            Assertions.False(RuntimeTesting.WeaponFindabilitySaveContract.MayWrite(transaction, "prepare", "KMG_AUTOMATION_BASELINE", true, true), "The protected baseline cannot be overwritten.");
            Assertions.False(RuntimeTesting.WeaponFindabilitySaveContract.MayWrite(transaction, "prepare", name, false, true), "A stale lease cannot authorize writes.");
            Assertions.False(RuntimeTesting.WeaponFindabilitySaveContract.MayWrite(transaction, "prepare", name, true, false), "Wrong artifact cannot authorize writes.");
            Assertions.False(RuntimeTesting.WeaponFindabilitySaveContract.Next("prepare", "verify", 100, 100), "An in-process round trip is not persistence proof.");
            Assertions.True(RuntimeTesting.WeaponFindabilitySaveContract.Next("prepare", "verify", 100, 101), "Verification requires a new process.");
            Assertions.False(RuntimeTesting.WeaponFindabilitySaveContract.ValidPhase("verify-remove"), "No overwrite or migration phase is exposed.");
        }
        internal static void MissingPhysicalEvidenceNeverPasses()
        {
            var evidence = new CampaignWeaponQualification {
                BlueprintIdentity = WeaponEvidenceState.Pass, NativeTreasure = WeaponEvidenceState.Pass };
            Assertions.False(evidence.IsPhysicallyQualified, "Published blueprints cannot qualify physical availability.");
            evidence.ScenePresence = WeaponEvidenceState.Pass;
            Assertions.False(evidence.IsPhysicallyQualified, "Scene presence cannot qualify the walking route or pickup.");
            evidence.NormalRoute = evidence.NormalPickup = evidence.Revisit = WeaponEvidenceState.Pass;
            Assertions.False(evidence.IsPhysicallyQualified, "A missing save/reload observation cannot become PASS.");
            evidence.SaveReload = WeaponEvidenceState.Pass;
            Assertions.True(evidence.IsPhysicallyQualified, "Every required measured gate must pass.");
            evidence.NormalRoute = WeaponEvidenceState.Fail;
            Assertions.False(evidence.IsPhysicallyQualified, "Failed normal route cannot be hidden by successful pickup.");
            for (int bits = 0; bits < 128; bits++)
            {
                Func<int, WeaponEvidenceState> gate = bit => (bits & (1 << bit)) != 0 ?
                    WeaponEvidenceState.Pass : WeaponEvidenceState.Unverified;
                var combination = new CampaignWeaponQualification {
                    BlueprintIdentity = gate(0), NativeTreasure = gate(1), ScenePresence = gate(2),
                    NormalRoute = gate(3), NormalPickup = gate(4), Revisit = gate(5), SaveReload = gate(6) };
                Assertions.Equal(bits == 127, combination.IsPhysicallyQualified,
                    "Each missing gate must independently prevent physical qualification: " + bits);
            }
        }

        internal static void RecoveryRecognizesCanonicalAndUpgradeOwnership()
        {
            const string guid = "0d31f794ba294c1e834af44f918f6721";
            Assertions.True(CampaignWeaponRecoveryPolicy.IsOwnedIdentity(guid, guid), "Canonical copy must refuse recovery.");
            Assertions.True(CampaignWeaponRecoveryPolicy.IsOwnedIdentity(guid, guid + "#CraftMagicItems(enchantments=abc)"), "Supported upgraded form retains its canonical root identity.");
            Assertions.False(CampaignWeaponRecoveryPolicy.IsOwnedIdentity(guid, "other#CraftMagicItems"), "Unrelated upgraded weapon is not this selected weapon.");
            Assertions.False(CampaignWeaponRecoveryPolicy.IsOwnedIdentity(guid, guid + "other"), "A GUID prefix alone cannot identify a weapon.");
            Assertions.False(CampaignWeaponRecoveryPolicy.IsOwnedIdentity(null, guid), "Unknown identity must not be matched.");
        }

        internal static void RecoveryRefusesEveryMissingPrerequisite()
        {
            for (int bits = 0; bits < 128; bits++)
            {
                bool relocated = (bits & 1) != 0, enabled = (bits & 2) != 0,
                    complete = (bits & 4) != 0, visited = (bits & 8) != 0,
                    recorded = (bits & 16) != 0, owned = (bits & 32) != 0,
                    container = (bits & 64) != 0;
                string result = CampaignWeaponRecoveryPolicy.Refusal(relocated, enabled, complete,
                    visited, recorded, owned ? "companion equipment" : null, container ? "ordinary chest" : null);
                Assertions.Equal(relocated && enabled && complete && visited && !recorded && !owned && !container,
                    result == null, "Recovery admission violated a bounded prerequisite: " + bits);
                if (relocated && owned) Assertions.True(result.Contains("companion equipment"), "Owned-copy refusal must locate the copy.");
            }
        }

        internal static void NativeReferenceCoversCompleteRegistryAndPearlRows()
        {
            var reference = JObject.Parse(File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "validation", "weapon-findability-native-reference.json")));
            var weapons = ((JArray)reference["weapons"]).OfType<JObject>().ToArray();
            Assertions.Equal(29, weapons.Length, "All named weapon assignments need a native comparison contract.");
            Assertions.Equal(29, weapons.Select(value => (string)value["targetGuid"]).Distinct().Count(), "Every weapon has one distinct container.");
            Assertions.Equal(27, weapons.Select(value => (string)value["areaName"]).Distinct().Count(), "Cord adds the twenty-eighth area.");
            var barrel = weapons.Single(value => (string)value["key"] == "MoonlitCrossing");
            var pearls = ((JArray)barrel["nativeRows"]).Where(value => (string)value["name"] == "Pearl").ToArray();
            Assertions.Equal(2, pearls.Length, "The native barrel must retain two distinct pearl rows.");
            Assertions.True(pearls.All(value => (int)value["count"] == 2), "Each serialized pearl row contains two pearls.");
        }

        internal static void EveryActiveTargetIsExcludedFromOwnCleanup()
        {
            foreach (string file in new[] { "RareFirearmCampaignLootBlueprints.cs", "EasternWeaponCampaignBlueprints.cs", "ElvenBranchedSpearCampaignBlueprints.cs" })
            {
                string source = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "src", "KingmakerGunslinger", "Blueprints", file));
                int cleanup = source.IndexOf(file.StartsWith("Rare", StringComparison.Ordinal) ?
                    "private static readonly CleanupSpec[] CleanupTargets" : file.StartsWith("Eastern", StringComparison.Ordinal) ?
                    "private static readonly EasternLootSpec[] CleanupLoot" : "private static readonly CleanupSpec[] CleanupLoot", StringComparison.Ordinal);
                int end = source.IndexOf("internal static", cleanup, StringComparison.Ordinal);
                string[] active = Regex.Matches(source.Substring(0, cleanup), "\"[a-f0-9]{32}\"").Cast<Match>().Select(value => value.Value).ToArray();
                string[] retired = Regex.Matches(source.Substring(cleanup, end-cleanup), "\"[a-f0-9]{32}\"").Cast<Match>().Select(value => value.Value).ToArray();
                Assertions.Equal(0, active.Intersect(retired).Count(), "An active target would lose its weapon to cleanup: " + file);
            }
        }
    }
}
