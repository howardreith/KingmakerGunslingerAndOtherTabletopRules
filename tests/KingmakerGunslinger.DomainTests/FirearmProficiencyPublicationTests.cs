using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class FirearmProficiencyPublicationTests
    {
        internal static void PublicationPolicyIsCompatibilityOnly()
        {
            string source = Read("src", "KingmakerGunslinger", "Blueprints",
                "FirearmFeatBlueprints.cs");
            Assertions.True(source.Contains(
                    "publication.Publish(set.RapidReload") &&
                source.Contains("set.ExoticWeaponProficiency);") &&
                source.Contains("feature.Groups = Array.Empty<FeatureGroup>();") &&
                source.Contains("feature.HideInUI = false;") &&
                source.Contains("Contains(selection.Features, legacyProficiency)") &&
                source.Contains("Contains(selection.AllFeatures, legacyProficiency)") &&
                source.Contains("RemoveAll(selection.Features") &&
                source.Contains("RemoveAll(selection.AllFeatures") &&
                !source.Contains("set.RapidReload, set.ExoticWeaponProficiency"),
                "The legacy firearm-proficiency wrapper is not retained solely as an unpublished compatibility fact.");
            Assertions.True(source.Contains("new SelectionSnapshot(selection)") &&
                source.Contains("snapshot.Selection.Features = snapshot.Features") &&
                source.Contains("snapshot.Selection.AllFeatures = snapshot.AllFeatures"),
                "Feat-catalog rollback does not restore exact original array references.");
            Assertions.True(source.Contains(
                    "Count(_basic.Features, rapidReload) != 1") &&
                source.Contains("Count(_fighter.AllFeatures, rapidReload) != 1"),
                "Rapid Reload exact-once publication is not fail-closed.");
        }

        // Group/Group2 only name what a selection offers; the published Rapid
        // Reload parent is classified by its inherited BlueprintFeature.Groups,
        // which the engine leaves empty unless it is assigned.
        internal static void RapidReloadParentIsACombatFeat()
        {
            string source = Read("src", "KingmakerGunslinger", "Blueprints",
                "FirearmFeatBlueprints.cs");
            string rapid = Body(source,
                "private static BlueprintFeatureSelection CreateRapidReloadSelection(");
            string generic = Body(source,
                "private static BlueprintFeatureSelection CreateSelection(");
            Assertions.True(rapid.Contains(
                    "selection.Groups = new[] { FeatureGroup.Feat, FeatureGroup.CombatFeat };"),
                "The Rapid Reload parent is not classified as a feat and a combat feat.");
            Assertions.True(rapid.IndexOf("selection.Groups =", StringComparison.Ordinal) ==
                    rapid.LastIndexOf("selection.Groups =", StringComparison.Ordinal),
                "The Rapid Reload parent classification is assigned more than once.");
            Assertions.False(generic.Contains(".Groups ="),
                "The generic selection helper classifies every firearm selection.");
            Assertions.True(generic.Contains("selection.Group = FeatureGroup.Feat;") &&
                generic.Contains("selection.Group2 = FeatureGroup.CombatFeat;"),
                "The generic selection helper's offered-category groups changed.");
            Assertions.True(rapid.Contains("RapidReloadPrerequisiteRules.ParentGateKinds") &&
                rapid.Contains("CreateParentProficiencyPrerequisite("),
                "The Rapid Reload parent lost its proficiency gate.");
            Assertions.True(source.Contains(
                    "new[] { FeatureGroup.Feat, FeatureGroup.CombatFeat } :"),
                "The official firearm child classification changed.");
        }

        private static string Body(string source, string signature)
        {
            int start = source.IndexOf(signature, StringComparison.Ordinal);
            Assertions.True(start >= 0, "Missing method: " + signature);
            int open = source.IndexOf('{', start);
            int depth = 0;
            for (int index = open; index < source.Length; index++)
            {
                if (source[index] == '{') depth++;
                else if (source[index] == '}' && --depth == 0)
                    return source.Substring(open, index - open + 1);
            }
            throw new InvalidOperationException("Unbalanced method: " + signature);
        }

        internal static void StableIdentitiesRemainExact()
        {
            JObject manifest = JObject.Parse(Read("blueprints",
                "blueprints.json"));
            var expected = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { "KMG.Firearms.FirearmProficiency", "5148f69223044799800b65732b6cabea" },
                { "KMG.Firearms.OneHandedFirearmProficiency", "1c6f66f734f64535a7de50030023a0dd" },
                { "KMG.Firearms.TwoHandedFirearmProficiency", "cbf3d4e79b6144b9b76480d8b242d37c" },
                { "KMG.Feats.ExoticWeaponProficiencyFirearms", "b1a58cfdbf004f04ade7765373484c29" },
                { "KMG.Feats.RapidReload", "85103137b2f54b7dacd98d51e856d8c3" },
                { "KMG.Classes.GunslingerProficiencies", "b9b6769f8a654a58a6bd55e10801ea22" }
            };
            JToken[] entries = manifest["entries"].ToArray();
            foreach (KeyValuePair<string, string> pair in expected)
            {
                JToken[] matches = entries.Where(value => string.Equals(
                    (string)value["symbol"], pair.Key,
                    StringComparison.Ordinal)).ToArray();
                Assertions.Equal(1, matches.Length,
                    "Stable proficiency identity is missing or duplicated: " + pair.Key);
                Assertions.Equal(pair.Value, (string)matches[0]["guid"],
                    "Stable proficiency GUID changed: " + pair.Key);
                Assertions.Equal("active", (string)matches[0]["status"],
                    "Compatibility blueprint registration was retired: " + pair.Key);
            }
        }

        internal static void RuntimeScenariosExerciseRealOwnersAndArchetypes()
        {
            string runtime = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs");
            foreach (string token in new[]
            {
                "firearm-proficiency-acquisition-policy",
                "legacy-firearm-proficiency-compatibility",
                "real AddFact propagation on a detached legacy owner",
                "actionGrant.Facts.All(fact => descriptor.HasFact(fact))",
                "base-to-pistolero",
                "base-to-musket-master",
                "base-to-mysterious-stranger",
                "only the intended full or scoped proficiency",
                "feature-module-legacy-firearm-proficiency"
            })
                Assertions.True(runtime.Contains(token),
                    "The guarded runtime proficiency contract lacks: " + token);
        }

        internal static void ScopedRespecReconciliationIsLegacySafe()
        {
            string scoped = Read("src", "KingmakerGunslinger", "Blueprints",
                "FirearmScopedProficiencyBlueprints.cs");
            string gunslinger = Read("src", "KingmakerGunslinger", "Blueprints",
                "GunslingerClassBlueprints.cs");
            string bootstrap = Read("src", "KingmakerGunslinger", "Bootstrap",
                "BlueprintBootstrap.cs");
            string runtime = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs");
            foreach (string token in new[]
            {
                "class FirearmProficiencyScopeReconciler",
                "Owner.HasFact(LegacyCompatibilityWrapper)",
                "Owner.RemoveFact(incompatible)",
                "AppendReconciler(basePresentation",
                "AppendReconciler(archetypePresentations.Pistolero",
                "AppendReconciler(archetypePresentations.MusketMaster"
            })
                Assertions.True(scoped.Contains(token),
                    "Scoped respec reconciliation lacks: " + token);
            Assertions.True(gunslinger.Contains(
                    "AttachScopeReconciliation(") &&
                bootstrap.Contains(
                    "firearmFeats.ExoticWeaponProficiency, gunsmithing") &&
                runtime.Contains("targetMysteriousStranger") &&
                runtime.Contains("MysteriousStranger.Grit"),
                "Class wiring, legacy preservation, or Mysterious Stranger grit observation is incomplete.");
        }

        private static string Read(params string[] parts)
        {
            string path = Environment.CurrentDirectory;
            for (int index = 0; index < parts.Length; index++)
                path = Path.Combine(path, parts[index]);
            return File.ReadAllText(path);
        }
    }
}
