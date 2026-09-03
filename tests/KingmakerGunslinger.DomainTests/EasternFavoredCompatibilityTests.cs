using System;
using System.IO;
using System.Linq;
using KingmakerGunslinger.AidAnotherCompatibility;
using KingmakerGunslinger.EasternWeapons;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class EasternFavoredCompatibilityTests
    {
        internal static void NodachiValueAndLateMartialPolicyAreExact()
        {
            Assertions.Equal(4934986,
                EasternWeaponMartialPublicationPolicy.NodachiCategoryValue,
                "Nodachi decimal category value changed.");
            Assertions.Equal(0x004b4d4a,
                EasternWeaponMartialPublicationPolicy.NodachiCategoryValue,
                "Nodachi hexadecimal category value changed.");
            int[] authority = Enumerable.Range(1, 20).ToArray();
            Assertions.True(EasternWeaponMartialPublicationPolicy.IsBroadGrant(
                    authority, authority.Concat(new[] { 99 })),
                "A superset of the native martial authority was not broad.");
            Assertions.False(EasternWeaponMartialPublicationPolicy.IsBroadGrant(
                    authority, authority.Take(19)),
                "A partial martial grant was classified as broad.");
            int[] appended = EasternWeaponMartialPublicationPolicy
                .AppendNodachiExactlyOnce(authority);
            Assertions.True(appended.Take(authority.Length)
                    .SequenceEqual(authority) && appended.Last() == 4934986 &&
                    appended.Count(value => value == 4934986) == 1,
                "Late publication did not preserve native order plus one Nodachi.");
            int[] repeated = EasternWeaponMartialPublicationPolicy
                .AppendNodachiExactlyOnce(appended);
            Assertions.True(repeated.SequenceEqual(appended) &&
                    repeated.Count(value => value == 4934986) == 1,
                "Repeated late reconciliation duplicated Nodachi.");
            Assertions.Throws<InvalidOperationException>(() =>
                EasternWeaponMartialPublicationPolicy
                    .AppendNodachiExactlyOnce(appended.Concat(new[]
                        { 4934986 })),
                "A duplicate Nodachi source array did not fail closed.");
            Assertions.True(EasternWeaponMartialPublicationPolicy
                    .NormalizeAuthority(appended).SequenceEqual(authority),
                "Nodachi was not removed when deriving native authority.");
        }

        internal static void LateMartialPolicyPreservesNativeFactShapes()
        {
            int[] authority = Enumerable.Range(10, 24).ToArray();
            int[][] facts = {
                authority,
                authority.Concat(new[] { 80, 81 }).ToArray(),
                authority.Reverse().Concat(new[] { 90 }).ToArray()
            };
            foreach (int[] fact in facts)
            {
                int[] before = (int[])fact.Clone();
                Assertions.True(EasternWeaponMartialPublicationPolicy
                        .IsBroadGrant(authority, fact),
                    "A legitimate broad-martial fact was not classified.");
                int[] after = EasternWeaponMartialPublicationPolicy
                    .AppendNodachiExactlyOnce(fact);
                Assertions.True(after.Take(before.Length).SequenceEqual(before)
                        && after.Length == before.Length + 1 &&
                        after.Last() == 4934986,
                    "Late publication changed native order/content instead of appending Nodachi once.");
            }
            Assertions.False(EasternWeaponMartialPublicationPolicy
                    .IsBroadGrant(authority, authority.Skip(1)),
                "A grant missing one native authority category was classified as broad.");
            Assertions.False(EasternWeaponMartialPublicationPolicy
                    .IsBroadGrant(authority.Take(19), authority),
                "An undersized native authority was accepted.");
            Assertions.False(EasternWeaponMartialPublicationPolicy
                    .IsBroadGrant(authority, null),
                "A null candidate was classified as broad.");
        }

        internal static void LoadDictionaryBoundaryIsLateAndTransactional()
        {
            string selector = Read("src", "KingmakerGunslinger",
                "EasternWeapons", "EasternWeaponSelectorPublication.cs");
            Assertions.False(selector.Contains("PublishMartial") ||
                    selector.Contains(
                        "EasternWeaponProficiencyRuntime.Configure") ||
                    selector.Contains("Concat(new[] { nodachi })"),
                "The early selector transaction still mutates broad martial arrays.");
            string publication = Read("src", "KingmakerGunslinger",
                "EasternWeapons", "EasternWeaponMartialPublication.cs");
            foreach (string token in new[] {
                "EasternWeaponMartialPublicationPolicy.IsBroadGrant",
                "UnityEngine.Object.Instantiate(grant)",
                "originals.Add(feature, components)",
                "feature.ComponentsArray = next",
                "EasternWeaponProficiencyRuntime.Configure(facts)",
                "entry.Key.ComponentsArray = entry.Value",
                "CountNodachiOnNative", "Validate()", "Rollback()" })
                Assertions.True(publication.Contains(token),
                    "Late martial transaction lacks: " + token);
            string coordinator = Read("src", "KingmakerGunslinger",
                "EasternWeapons",
                "EasternWeaponLatePublicationCoordinator.cs");
            foreach (string token in new[] {
                "FirstUpdate", "first-update-after-load-dictionary",
                "early != 0", "late-publication.complete",
                "late-publication.idempotent", "unrelated KMG modules remain active" })
                Assertions.True(coordinator.Contains(token),
                    "Late lifecycle coordinator lacks: " + token);
            Assertions.False(coordinator.Contains("HarmonyPatch"),
                "Late publication introduced an independently ordered Harmony patch.");
            string main = Read("src", "KingmakerGunslinger", "Main.cs");
            Assertions.True(main.IndexOf(
                    "BlueprintBootstrap.TryInitializePending",
                    StringComparison.Ordinal) < main.IndexOf(
                    "EasternWeaponLatePublicationCoordinator.AttachFirstUpdate",
                    StringComparison.Ordinal),
                "The late coordinator is attached before KMG bootstrap coordination.");
        }

        internal static void LateCoordinatorRollbackAndRetryAreExact()
        {
            string coordinator = Read("src", "KingmakerGunslinger",
                "EasternWeapons",
                "EasternWeaponLatePublicationCoordinator.cs");
            foreach (string token in new[] { "publication = existing",
                "rollback.Rollback()", "new AggregateException(exception",
                "ReferenceEquals(_publication, publication)",
                "favored-class-compatible-contract-absent" })
                Assertions.True(coordinator.Contains(token),
                    "Late coordinator exception safety lacks: " + token);
            string aid = Read("src", "KingmakerGunslinger",
                "AidAnotherCompatibility",
                "AidAnotherOptionalExtensionCoordinator.cs");
            int configure = aid.IndexOf("AidAnotherGrantRuntime.Configure(cotw.Contract",
                StringComparison.Ordinal);
            int retry = aid.IndexOf(
                "EasternWeaponLatePublicationCoordinator.TryPublish(",
                StringComparison.Ordinal);
            Assertions.True(configure >= 0 && retry > configure &&
                    aid.Contains("aid-another-compatible-reconcile"),
                "A later exact Aid Another reconciliation cannot retry late Heirloom publication.");
        }

        internal static void FavoredAndTweakContractsAreExact()
        {
            string favored = Read("src", "KingmakerGunslinger",
                "AidAnotherCompatibility", "FavoredClassTraitResolver.cs");
            foreach (string token in new[] {
                "af37d78d7bc5451d943b63356f438949",
                "equipment_traits", "EquipmentTrait",
                "favored-class-equipment-traits-structure",
                "heirloomChoices.Length < 20",
                "Contains(first.AllFeatures, equipment)",
                "Contains(second.AllFeatures, equipment)" })
                Assertions.True(favored.Contains(token),
                    "Favored Class full-trait contract lacks: " + token);
            string tweak = Read("src", "KingmakerGunslinger",
                "EasternWeapons", "TweakOrTreatHeirloomResolver.cs");
            foreach (string token in new[] { "TweakOrTreat",
                "TweakOrTreat.HeirloomWeapon", "MethodName = \"load\"",
                "ZFavoredClass.NewMechanics.PrerequisiteRace",
                "typeof(PrerequisiteFeature)",
                "transformedRacialChoices", "Hash(assembly.Location)" })
                Assertions.True(tweak.Contains(token),
                    "Tweak or Treat exact observer lacks: " + token);
            Assertions.False(tweak.Contains(".Invoke(") ||
                    tweak.Contains("Harmony.Patch"),
                "KMG invokes or patches Tweak or Treat instead of observing its completed contract.");
        }

        internal static void HeirloomNodachiIdentityAndMechanicsAreExact()
        {
            JObject manifest = JObject.Parse(Read("blueprints",
                "blueprints.json"));
            JObject[] entries = ((JArray)manifest["entries"]).Cast<JObject>()
                .Where(value => ((string)value["symbol"]).StartsWith(
                    "KMG.Traits.HeirloomWeapon.Nodachi.",
                    StringComparison.Ordinal)).ToArray();
            Assertions.Equal(5, entries.Length,
                "Heirloom Weapon (Nodachi) identity count changed.");
            Assertions.Equal(5, entries.Select(value =>
                    (string)value["guid"]).Distinct(StringComparer.Ordinal)
                    .Count(),
                "Heirloom Weapon (Nodachi) GUIDs are not unique.");
            Assertions.True(entries.All(value =>
                (string)value["status"] == "active" &&
                ((string)value["guid"]).Length == 32),
                "A Heirloom Weapon (Nodachi) identity is not active/stable.");

            string blueprints = Read("src", "KingmakerGunslinger",
                "Blueprints", "HeirloomNodachiBlueprints.cs");
            foreach (string token in new[] { "FeatureGroup.Trait",
                "Heirloom Weapon: Nodachi", "AddStartingEquipment",
                "PrerequisiteNotProficient", "AddProficiencies",
                "PrerequisiteProficiency", "ModifierDescriptor.Trait",
                "StatType.AdditionalCMB", "bonus.Value != 2",
                "AllFeatures.Length != 3", "PrerequisiteNoFeature" })
                Assertions.True(blueprints.Contains(token),
                    "Nodachi Heirloom blueprint lacks: " + token);
            string effects = Read("src", "KingmakerGunslinger",
                "EasternWeapons", "HeirloomNodachiEffects.cs");
            foreach (string token in new[] {
                "evt.RuleAttackWithWeapon.IsAttackOfOpportunity",
                "AddTemporaryModifier", "AdditionalAttackBonus",
                "ModifierDescriptor.Trait", "Owner.AddFact(Feature",
                "PrimaryHand", "SecondaryHand", "Owner.RemoveFact" })
                Assertions.True(effects.Contains(token),
                    "Nodachi Heirloom runtime effect lacks: " + token);
            string project = Read("src", "KingmakerGunslinger",
                "KingmakerGunslinger.csproj");
            Assertions.False(project.Contains(
                    "<Reference Include=\"ZFavoredClass\"") ||
                    project.Contains(
                        "<Reference Include=\"TweakOrTreat\"") ||
                    project.Contains(
                        "<Reference Include=\"CallOfTheWild\""),
                "The optional Heirloom integration gained a compile dependency.");
        }

        internal static void HeirloomForeignPublicationIsAtomic()
        {
            var nativeOne = new Identity("native-one");
            var nativeTwo = new Identity("native-two");
            var nodachi = new Identity("nodachi");
            Identity[] equipment = new[] { nativeOne, nativeTwo };
            Identity[] before = equipment;
            var publication = new HelpfulPublicationTransaction().Append(
                "favored-equipment-traits-heirloom-nodachi",
                () => equipment, values => equipment = values, nodachi,
                value => value.Id, false);
            publication.Commit();
            Assertions.True(equipment.Length == 3 &&
                ReferenceEquals(equipment[0], nativeOne) &&
                ReferenceEquals(equipment[1], nativeTwo) &&
                ReferenceEquals(equipment[2], nodachi),
                "Nodachi publication changed unrelated Equipment Traits.");
            Identity[] after = equipment;
            publication.Commit();
            Assertions.True(ReferenceEquals(after, equipment) &&
                equipment.Count(value => ReferenceEquals(value, nodachi)) == 1,
                "Repeated Nodachi publication was not idempotent.");
            publication.Rollback();
            Assertions.True(ReferenceEquals(before, equipment),
                "Nodachi rollback did not restore the exact foreign array.");

            Identity[] conflict = new[] { new Identity("nodachi") };
            var rejected = new HelpfulPublicationTransaction().Append(
                "guid-conflict", () => conflict,
                values => conflict = values, nodachi, value => value.Id,
                false);
            Assertions.Throws<InvalidOperationException>(() =>
                rejected.Commit(),
                "A foreign same-GUID Nodachi choice was not rejected.");
            Assertions.Equal(1, conflict.Length,
                "Conflict rejection partially mutated Equipment Traits.");
        }

        internal static void HeirloomPartialFailureRestoresEveryArray()
        {
            var nativeEquipment = new Identity("native-equipment");
            var nativeRoute = new Identity("native-route");
            var nodachi = new Identity("nodachi");
            Identity[] equipment = new[] { nativeEquipment };
            Identity[] route = new[] { new Identity("nodachi"), nativeRoute };
            Identity[] equipmentBefore = equipment;
            Identity[] routeBefore = route;
            var transaction = new HelpfulPublicationTransaction()
                .Append("equipment", () => equipment,
                    values => equipment = values, nodachi,
                    value => value.Id, false)
                .Append("conflicting-route", () => route,
                    values => route = values, nodachi,
                    value => value.Id, false);
            Assertions.Throws<InvalidOperationException>(() =>
                transaction.Commit(),
                "A partial foreign-array conflict did not fail closed.");
            Assertions.True(ReferenceEquals(equipmentBefore, equipment) &&
                    ReferenceEquals(routeBefore, route),
                "Partial failure did not restore every exact original array reference.");
        }

        internal static void RuntimeProfilesAndSettingsRestorationAreExact()
        {
            JObject document = JObject.Parse(Read("compatibility",
                "profiles.json"));
            JObject[] profiles = ((JArray)document["profiles"])
                .Cast<JObject>().ToArray();
            var expected = new[] {
                new { Id = "gunslinger-call-of-the-wild-favored-class",
                    Keys = new[] { "call-of-the-wild", "favored-class" } },
                new { Id = "gunslinger-call-of-the-wild-favored-class-traits-disabled",
                    Keys = new[] { "call-of-the-wild", "favored-class" } },
                new { Id = "gunslinger-high-risk-combined-favored-class",
                    Keys = new[] { "call-of-the-wild", "favored-class",
                        "tweak-or-treat", "races-unleashed" } }
            };
            foreach (var contract in expected)
            {
                JObject profile = profiles.Single(value =>
                    (string)value["id"] == contract.Id);
                Assertions.True((bool)profile["runtimeLoadableRequired"] &&
                        ((JArray)profile["modKeys"]).Values<string>()
                            .SequenceEqual(contract.Keys) &&
                        (string)profile["requiredGunslingerPackage"] ==
                            "KingmakerGunslinger-0.0.114-local-runtime.zip" &&
                        ((JArray)profile["scenarios"]).Values<string>()
                            .Contains("observe-aid-another-compatibility-contracts"),
                    "Required runtime profile is incomplete: " + contract.Id);
            }
            string script = Read("scripts", "compatibility",
                "Invoke-KingmakerCompatibilityProfile.ps1");
            foreach (string token in new[] { "favoredTraitsMode",
                "enable_traits", "favoredSettingsStagedBeforeSha",
                "favoredSettingsStagedAfterSha",
                "completeModsRestorationVerified",
                "state.restorationVerified" })
                Assertions.True(script.Contains(token),
                    "Favored settings/restoration transaction lacks: " + token);
        }

        internal static void ModuleAndTraitDisabledGatesRemainIndependent()
        {
            string late = Read("src", "KingmakerGunslinger",
                "EasternWeapons",
                "EasternWeaponLatePublicationCoordinator.cs");
            Assertions.True(late.Contains(
                    "favored.TraitsEnabled && context != null") &&
                    late.Contains(
                    "context.FeatureModules.Active.EasternWeapons") &&
                    late.Contains("favored-class-traits-disabled") &&
                    late.Contains("eastern-weapons-module-off"),
                "Nodachi Heirloom publication is not independently gated by traits and Eastern Weapons.");
            string aid = Read("src", "KingmakerGunslinger",
                "AidAnotherCompatibility",
                "AidAnotherOptionalExtensionCoordinator.cs");
            Assertions.True(aid.Contains(
                    "context.FeatureModules.Active.BodyguardFeats") &&
                    !late.Contains(
                    "context.FeatureModules.Active.BodyguardFeats"),
                "Eastern and Bodyguard optional publication gates were coupled.");
        }

        private static string Read(params string[] parts)
        {
            string path = Environment.CurrentDirectory;
            foreach (string part in parts) path = Path.Combine(path, part);
            return File.ReadAllText(path);
        }

        private sealed class Identity
        {
            internal Identity(string id) { Id = id; }
            internal string Id { get; private set; }
        }
    }
}
