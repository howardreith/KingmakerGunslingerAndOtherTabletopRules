using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Compatibility;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class MartialPerformanceCompatibilityTests
    {
        private const string ExactType =
            "Kingmaker.Blueprints.Classes.Selection.BlueprintFeatureSelection";
        private const string ExactName =
            "MartialPerformanceFeatureSelection";

        internal static void ExactOptionalIdentityContract()
        {
            Assertions.True(
                CustomWeaponMartialPerformanceIdentityPolicy.IsPresent(true,
                    ExactType, ExactName, ExactType, ExactName),
                "Exact Martial Performance identity was rejected.");
            string source = Production();
            foreach (string token in new[] {
                "19d1ff4cf70845d094b0ec231473e97f",
                ExactType, ExactName,
                "b7786666fe5b4694b8c4560efa6053c3",
                "DaggerMartialPerformanceFeature",
                "BlueprintFeatureSelection", "BlueprintFeature" })
                Assertions.True(source.Contains(token),
                    "Martial Performance exact identity contract lacks " +
                    token + ".");
        }

        internal static void AbsentProviderIsInert()
        {
            Assertions.False(
                CustomWeaponMartialPerformanceIdentityPolicy.IsPresent(false,
                    "wrong", "wrong", ExactType, ExactName),
                "Absent optional provider was treated as present.");
            string source = Production();
            int discovery = source.IndexOf(
                "BlueprintFeatureSelection selection = FindSelection(library)",
                StringComparison.Ordinal);
            int inert = source.IndexOf(
                "if (selection == null) return publication",
                StringComparison.Ordinal);
            int firearmValidation = source.IndexOf(
                "if (firearmFeats == null)", StringComparison.Ordinal);
            int registration = source.IndexOf(
                "publication._registered = specs.Select",
                StringComparison.Ordinal);
            Assertions.True(discovery >= 0 && discovery < inert &&
                    inert < firearmValidation && inert < registration,
                "Absent provider can reach validation, registration, or selector mutation.");
        }

        internal static void WrongProviderContractFailsClosed()
        {
            Assertions.Throws<InvalidOperationException>(() =>
                CustomWeaponMartialPerformanceIdentityPolicy.IsPresent(true,
                    "Kingmaker.Blueprints.Classes.BlueprintFeature",
                    ExactName, ExactType, ExactName),
                "Wrong Martial Performance type did not fail closed.");
            Assertions.Throws<InvalidOperationException>(() =>
                CustomWeaponMartialPerformanceIdentityPolicy.IsPresent(true,
                    ExactType, "RenamedSelection", ExactType, ExactName),
                "Wrong Martial Performance internal name did not fail closed.");
        }

        internal static void RollbackRestoresExactOriginalState()
        {
            Fixture[] original = {
                new Fixture("native-dagger", "Dagger"),
                new Fixture("optional-entry", "Optional") };
            Fixture[] owned = Owned();
            var policy =
                new CustomWeaponMartialPerformanceSelectionPolicy<Fixture>(
                    original, value => value.Id);
            Fixture[] published = policy.Publish(owned, Active(owned));
            Assertions.False(ReferenceEquals(original, published),
                "Publication reused and mutated the original array.");
            Assertions.True(ReferenceEquals(original, policy.Rollback()),
                "Rollback did not return the exact original array reference.");
            Assertions.Equal("native-dagger", original[0].Id,
                "Publication mutated the original selector contents.");
        }

        internal static void ActiveCatalogIsExactAndOrdered()
        {
            Fixture[] native = {
                new Fixture("native-dagger", "Dagger"),
                new Fixture("native-longsword", "Longsword"),
                new Fixture("other-mod", "Other") };
            Fixture[] owned = Owned();
            var policy =
                new CustomWeaponMartialPerformanceSelectionPolicy<Fixture>(
                    native, value => value.Id);
            Fixture[] result = policy.Publish(owned, Active(owned).Reverse());
            Assertions.True(result.Take(native.Length).SequenceEqual(native),
                "Native or optional choices changed or moved.");
            foreach (Fixture value in owned)
                Assertions.Equal(1, result.Count(candidate =>
                    candidate.Id == value.Id),
                    "An active custom category is missing or duplicated: " +
                    value.Id);
            string[] expected = {
                "blunderbuss", "spear", "katana", "musket",
                "nodachi", "pistol", "wakizashi" };
            Assertions.True(result.Skip(native.Length).Select(value => value.Id)
                    .SequenceEqual(expected),
                "Custom Martial Performance ordering is not deterministic.");
        }

        internal static void ModuleDisabledCategoriesAreAbsent()
        {
            Fixture[] owned = Owned();
            Fixture[] existing = {
                new Fixture("native", "Native"), owned[0], owned[0], owned[4] };
            var policy =
                new CustomWeaponMartialPerformanceSelectionPolicy<Fixture>(
                    existing, value => value.Id);
            Fixture[] result = policy.Publish(owned,
                Active(owned.Take(3).ToArray()));
            Assertions.Equal(4, result.Length,
                "Disabled-module choices leaked into Martial Performance.");
            Assertions.Equal("native", result[0].Id,
                "Native choice changed while modules were disabled.");
            Assertions.True(result.Skip(1).All(value =>
                    value.Id == "pistol" || value.Id == "musket" ||
                    value.Id == "blunderbuss"),
                "A disabled spear or eastern category remained published.");
        }

        internal static void RepeatedPublicationIsIdempotent()
        {
            Fixture[] owned = Owned();
            Fixture[] original = { new Fixture("native", "Native") };
            var first =
                new CustomWeaponMartialPerformanceSelectionPolicy<Fixture>(
                    original, value => value.Id);
            Fixture[] once = first.Publish(owned, Active(owned));
            Fixture[] samePolicyTwice = first.Publish(owned, Active(owned));
            Assertions.True(once.Select(value => value.Id).SequenceEqual(
                    samePolicyTwice.Select(value => value.Id)),
                "Repeated selector enumeration changed the choices.");
            var second =
                new CustomWeaponMartialPerformanceSelectionPolicy<Fixture>(
                    once, value => value.Id);
            Fixture[] twice = second.Publish(owned, Active(owned));
            Assertions.True(once.Select(value => value.Id).SequenceEqual(
                    twice.Select(value => value.Id)),
                "Repeated publication accumulated custom choices.");
            foreach (Fixture value in owned)
                Assertions.Equal(1, twice.Count(candidate =>
                    candidate.Id == value.Id),
                    "Repeated publication duplicated " + value.Id + ".");
        }

        internal static void AuthoritativeProficiencyPolicy()
        {
            Assertions.True(
                CustomWeaponMartialPerformanceProficiencyPolicy.CanUse(
                    true, false, false),
                "Direct category proficiency was rejected.");
            Assertions.False(
                CustomWeaponMartialPerformanceProficiencyPolicy.CanUse(
                    false, false, false),
                "A non-proficient category was accepted.");
            Assertions.False(
                CustomWeaponMartialPerformanceProficiencyPolicy.CanUse(
                    false, true, false),
                "Unrelated broad martial proficiency was accepted.");
            Assertions.True(
                CustomWeaponMartialPerformanceProficiencyPolicy.CanUse(
                    false, true, true),
                "Two-handed martial Katana proficiency was rejected.");
            string source = Production();
            Assertions.True(source.Contains(
                    "unit.Proficiencies.Contains(Category)") &&
                source.Contains(
                    "EasternWeaponProficiencyRuntime.HasBroadMartial(unit)") &&
                source.Contains("PrerequisiteFirearmProficiency firearm"),
                "Production bypasses the authoritative category or firearm proficiency rules.");
        }

        internal static void PreviewUnitProficiencyPathIsNative()
        {
            string source = Production();
            string firearm = Read("src", "KingmakerGunslinger", "Feats",
                "PrerequisiteFirearmProficiency.cs");
            string observer = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "OptionalModCompatibilityObserver.cs");
            Assertions.True(source.Contains(
                    "Check(FeatureSelectionState selectionState,") &&
                source.Contains("UnitDescriptor unit, LevelUpState state)") &&
                source.Contains("unit.Proficiencies.Contains(Category)") &&
                firearm.Contains("unit.Progression.Features.GetRank(") &&
                source.Contains(
                    "components.OfType<Prerequisite>().Count() != 1"),
                "A child prerequisite no longer evaluates the engine-supplied preview descriptor.");
            Assertions.True(observer.Contains(
                    "IFeatureSelectionItem[] beforeItems") &&
                observer.Contains("selection.CanSelect(") &&
                observer.Contains("before.Descriptor, null") &&
                observer.Contains("preview.Descriptor, null") &&
                !observer.Contains("OfType<FeatureUIData>()"),
                "The runtime regression bypasses the native feature-selection item or CanSelect contract.");
        }

        internal static void SelectedEffectMatchesNativeShape()
        {
            string source = Production();
            foreach (string token in new[] {
                "components.Length != 2",
                "AddParametrizedFeatures",
                "PrerequisiteProficiency",
                "\"m_Features\"", "\"Feature\"", "\"ParamObject\"",
                "\"ParamWeaponCategory\"", "WeaponFocusGuid",
                "featureField.SetValue(row, weaponFocus)",
                "donor.Description" })
                Assertions.True(source.Contains(token),
                    "Native Martial Performance effect-shape contract lacks " +
                    token + ".");
            Assertions.False(source.Contains("HarmonyPatch") ||
                source.Contains("GetFullSelectionItems") ||
                source.Contains("ExtractSelectionItems"),
                "Martial Performance was implemented as a broad selector patch.");
        }

        internal static void BootstrapAndManifestAreTransactional()
        {
            string bootstrap = Read("src", "KingmakerGunslinger",
                "Bootstrap", "BlueprintBootstrap.cs");
            foreach (string token in new[] {
                "CustomWeaponMartialPerformancePublication",
                ".RegisterAndPublish(library, registry, firearmFeats",
                "publicationPlan.FirearmParameters",
                "publicationPlan.ElvenBranchedSpearSelectors",
                "publicationPlan.EasternWeaponSelectors",
                "martialPerformancePublication.Rollback()",
                "ExpectedRegisteredBlueprintCount +",
                "martialPerformancePublication.RegisteredCount" })
                Assertions.True(bootstrap.Contains(token),
                    "Bootstrap Martial Performance transaction lacks " +
                    token + ".");

            JObject manifest = JObject.Parse(Read("blueprints",
                "blueprints.json"));
            JArray entries = (JArray)manifest["entries"];
            Dictionary<string, string> expected =
                ExpectedManifestIdentities();
            foreach (KeyValuePair<string, string> identity in expected)
            {
                JToken[] matches = entries.Where(entry =>
                    (string)entry["symbol"] == identity.Key).ToArray();
                Assertions.Equal(1, matches.Length,
                    "Martial Performance manifest symbol is not singular: " +
                    identity.Key);
                Assertions.Equal(identity.Value,
                    (string)matches[0]["guid"],
                    "Martial Performance manifest GUID changed.");
                Assertions.Equal("BlueprintFeature",
                    (string)matches[0]["plannedType"],
                    "Martial Performance manifest type changed.");
                Assertions.Equal("active", (string)matches[0]["status"],
                    "Martial Performance manifest entry is inactive.");
            }
        }

        private static Fixture[] Owned()
        {
            return new[] {
                new Fixture("pistol", "Pistol"),
                new Fixture("musket", "Musket"),
                new Fixture("blunderbuss", "Blunderbuss"),
                new Fixture("spear", "Elven Branched Spear"),
                new Fixture("wakizashi", "Wakizashi"),
                new Fixture("katana", "Katana"),
                new Fixture("nodachi", "Nodachi") };
        }

        private static CustomWeaponMartialPerformanceChoice<Fixture>[]
            Active(Fixture[] values)
        {
            return values.Select(value =>
                new CustomWeaponMartialPerformanceChoice<Fixture>(
                    value.Id, value.Name, value)).ToArray();
        }

        private static Dictionary<string, string>
            ExpectedManifestIdentities()
        {
            return new Dictionary<string, string>(StringComparer.Ordinal) {
                { "KMG.CustomWeapons.MartialPerformance.Pistol",
                    "ae201a05ceca46ad91e1c1eeb6321563" },
                { "KMG.CustomWeapons.MartialPerformance.Musket",
                    "65ec2aa82f3847dc80e06e6b3d5c4436" },
                { "KMG.CustomWeapons.MartialPerformance.Blunderbuss",
                    "db37b2a40889495c8754607357c36447" },
                { "KMG.CustomWeapons.MartialPerformance.ElvenBranchedSpear",
                    "29820237e39f438ba649a0850b3734f4" },
                { "KMG.CustomWeapons.MartialPerformance.Wakizashi",
                    "bf303f6dac264528ba3c97ce719e12d6" },
                { "KMG.CustomWeapons.MartialPerformance.Katana",
                    "13f2e7047e1b48638fe2939e98007ac2" },
                { "KMG.CustomWeapons.MartialPerformance.Nodachi",
                    "72ec4ee09f44491aa3857b599fd206ef" } };
        }

        private static string Production()
        {
            return Read("src", "KingmakerGunslinger", "Compatibility",
                "CustomWeaponMartialPerformanceCompatibility.cs");
        }

        private static string Read(params string[] parts)
        {
            string path = Environment.CurrentDirectory;
            foreach (string part in parts) path = Path.Combine(path, part);
            return File.ReadAllText(path);
        }

        private sealed class Fixture
        {
            internal Fixture(string id, string name)
            {
                Id = id;
                Name = name;
            }

            internal string Id { get; private set; }
            internal string Name { get; private set; }
        }
    }
}
