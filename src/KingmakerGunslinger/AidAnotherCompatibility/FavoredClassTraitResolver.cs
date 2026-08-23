using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using UnityModManagerNet;

namespace KingmakerGunslinger.AidAnotherCompatibility
{
    internal static class FavoredClassTraitResolver
    {
        internal const string ModId = "ZFavoredClass";
        internal const string AssemblyName = "ZFavoredClass";
        internal const string TraitsTypeName = "ZFavoredClass.Traits";
        internal const string MainTypeName = "ZFavoredClass.Main";
        internal const string LoadMethodName = "load";
        internal const string CombatTraitsGuid =
            "43d763957f364315b5fff85f9e91ca51";
        internal const string RaceTraitsGuid =
            "331ed3c4a988415785f71a37b826d0f1";
        internal const string FirstTraitGuid =
            "34e2812e0f8241bb9e1bee5240c9eb2e";
        internal const string SecondTraitGuid =
            "5253dcee502a49249bdd8bfdfe525e9f";
        internal const string AdoptedGuid =
            "987e573c15e241c285e0fa1d5ac0a0a2";
        internal const string AdditionalTraitsGuid =
            "6a1f65b204a74c22b0f47e1e2c808441";
        internal const string HalflingHelpfulGuid =
            "c9bd9f6cc24f41e684a68e6510afc726";
        internal const string HalflingRaceGuid =
            "b0c3ef2729c498f47970bb50fa1acd30";
        private const string RacePrerequisiteTypeName =
            "ZFavoredClass.NewMechanics.PrerequisiteRace";
        private const string AddSelectionTypeName =
            "CallOfTheWild.EvolutionMechanics.addSelection";

        internal static AidAnotherContractResolution<FavoredClassTraitContract>
            Resolve(UnityModManager.ModEntry entry,
                BlueprintFeature kmgHelpful)
        {
            if (entry == null)
                return Result(OptionalAidAnotherAvailability.Absent,
                    "favored-class-absent", null);
            if (entry.Info == null || !string.Equals(entry.Info.Id, ModId,
                    StringComparison.Ordinal))
                return Result(OptionalAidAnotherAvailability.Blocked,
                    "favored-class-umm-id", null);
            if (!entry.Loaded || !entry.Active || !entry.HasAssembly ||
                entry.ErrorOnLoading || entry.Assembly == null)
                return Result(OptionalAidAnotherAvailability.Pending,
                    "favored-class-not-active", null);
            Assembly assembly = entry.Assembly;
            if (!string.Equals(assembly.GetName().Name, AssemblyName,
                    StringComparison.Ordinal))
                return Result(OptionalAidAnotherAvailability.Blocked,
                    "favored-class-assembly-name", null);

            try
            {
                Type traits = assembly.GetType(TraitsTypeName, false, false);
                Type main = assembly.GetType(MainTypeName, false, false);
                if (traits == null || main == null)
                    return Result(OptionalAidAnotherAvailability.Blocked,
                        "favored-class-types", null);
                MethodInfo load = traits.GetMethod(LoadMethodName,
                    BindingFlags.Static | BindingFlags.Public |
                    BindingFlags.NonPublic, null, new[] { typeof(bool) }, null);
                if (load == null || load.ReturnType != typeof(void) ||
                    load.IsGenericMethod)
                    return Result(OptionalAidAnotherAvailability.Blocked,
                        "favored-class-traits-load-signature", null);

                BlueprintFeatureSelection combat = ReadStatic<
                    BlueprintFeatureSelection>(traits, "combat_traits");
                BlueprintFeatureSelection race = ReadStatic<
                    BlueprintFeatureSelection>(traits, "racial_traits");
                BlueprintFeatureSelection first = ReadStatic<
                    BlueprintFeatureSelection>(traits, "traits_selection");
                BlueprintFeatureSelection second = ReadStatic<
                    BlueprintFeatureSelection>(traits, "traits_selection2");
                BlueprintFeatureSelection adopted = ReadStatic<
                    BlueprintFeatureSelection>(traits, "adopted");
                BlueprintFeature additional = ReadStatic<BlueprintFeature>(
                    traits, "additional_traits");
                BlueprintFeature helpful = ReadStatic<BlueprintFeature>(traits,
                    "helpful");
                if (combat == null || race == null || first == null ||
                    second == null || adopted == null || additional == null ||
                    helpful == null)
                    return Result(OptionalAidAnotherAvailability.Pending,
                        "favored-class-traits-not-created", null);

                if (!Exact(combat, CombatTraitsGuid, "CombatTrait") ||
                    !Exact(race, RaceTraitsGuid, "RacialTrait") ||
                    !Exact(first, FirstTraitGuid, "TraitsSelection") ||
                    !Exact(second, SecondTraitGuid, "TraitSelection2Feature") ||
                    !Exact(adopted, AdoptedGuid, "AdoptedTraitSelection") ||
                    !Exact(additional, AdditionalTraitsGuid,
                        "AdditionalTraitsFeature") ||
                    !Exact(helpful, HalflingHelpfulGuid, "HelpfulTrait"))
                    return Result(OptionalAidAnotherAvailability.Blocked,
                        "favored-class-blueprint-identities", null);
                if (!string.Equals(helpful.Name, "Helpful",
                        StringComparison.Ordinal) || helpful.Ranks != 1 ||
                    helpful.HideInUI || helpful.IsClassFeature ||
                    helpful.Groups == null || helpful.Groups.Length != 1 ||
                    !helpful.Groups.Contains(FeatureGroup.Trait))
                    return Result(OptionalAidAnotherAvailability.Blocked,
                        "favored-class-helpful-presentation", null);

                BlueprintComponent[] helpfulComponents =
                    helpful.ComponentsArray ?? new BlueprintComponent[0];
                BlueprintComponent racePrerequisite = helpfulComponents
                    .SingleOrDefault(value => value != null && string.Equals(
                        value.GetType().FullName, RacePrerequisiteTypeName,
                        StringComparison.Ordinal));
                if (racePrerequisite == null || helpfulComponents.Any(value =>
                    !ReferenceEquals(value, racePrerequisite) &&
                    !(value is PrerequisiteNoFeature && kmgHelpful != null &&
                        ReferenceEquals(((PrerequisiteNoFeature)value).Feature,
                            kmgHelpful))))
                    return Result(OptionalAidAnotherAvailability.Blocked,
                        "favored-class-helpful-components", null);
                FieldInfo raceField = racePrerequisite.GetType().GetField("race",
                    BindingFlags.Instance | BindingFlags.Public |
                    BindingFlags.NonPublic);
                BlueprintRace halfling = raceField == null ? null :
                    raceField.GetValue(racePrerequisite) as BlueprintRace;
                if (raceField == null || raceField.FieldType !=
                        typeof(BlueprintRace) || halfling == null ||
                    !string.Equals(halfling.AssetGuid, HalflingRaceGuid,
                        StringComparison.Ordinal))
                    return Result(OptionalAidAnotherAvailability.Blocked,
                        "favored-class-helpful-race", null);

                if (Count(race.AllFeatures, helpful) != 1 ||
                    Count(adopted.AllFeatures, helpful) != 1 ||
                    !adopted.IgnorePrerequisites)
                    return Result(OptionalAidAnotherAvailability.Blocked,
                        "favored-class-race-membership", null);
                if (!Contains(first.AllFeatures, combat) ||
                    !Contains(first.AllFeatures, race) ||
                    !Contains(second.AllFeatures, combat) ||
                    !Contains(second.AllFeatures, race))
                    return Result(OptionalAidAnotherAvailability.Blocked,
                        "favored-class-top-level-routes", null);
                BlueprintFeatureSelection[] additionalRoutes =
                    (additional.ComponentsArray ?? new BlueprintComponent[0])
                    .Where(value => value != null && string.Equals(
                        value.GetType().FullName, AddSelectionTypeName,
                        StringComparison.Ordinal)).Select(ReadSelection).ToArray();
                if (additionalRoutes.Length != 2 ||
                    additionalRoutes.Count(value => ReferenceEquals(value,
                        first)) != 1 || additionalRoutes.Count(value =>
                            ReferenceEquals(value, second)) != 1)
                    return Result(OptionalAidAnotherAvailability.Blocked,
                        "favored-class-additional-traits-routes", null);

                bool traitsEnabled;
                if (!TryReadTraitsSetting(main, out traitsEnabled))
                    return Result(OptionalAidAnotherAvailability.Blocked,
                        "favored-class-enable-traits-setting", null);
                string fingerprint = "ummId=" + entry.Info.Id +
                    ";ummVersion=" + entry.Info.Version +
                    ";assembly=" + assembly.FullName +
                    ";mvid=" + assembly.ManifestModule.ModuleVersionId +
                    ";sha256=" + Hash(assembly.Location) +
                    ";enable_traits=" + traitsEnabled +
                    ";combat=" + combat.AssetGuid +
                    ";race=" + race.AssetGuid +
                    ";helpful=" + helpful.AssetGuid +
                    ";top1=" + first.AssetGuid +
                    ";top2=" + second.AssetGuid +
                    ";additional=" + additional.AssetGuid +
                    ";adoptedIgnorePrerequisites=" +
                        adopted.IgnorePrerequisites;
                return Result(OptionalAidAnotherAvailability.Compatible,
                    string.Empty, new FavoredClassTraitContract(assembly, load,
                        traitsEnabled, combat, race, first, second, adopted,
                        additional, helpful, fingerprint));
            }
            catch (Exception exception)
            {
                return Result(OptionalAidAnotherAvailability.Blocked,
                    "favored-class-resolver-exception:" +
                        exception.GetType().Name, null);
            }
        }

        private static T ReadStatic<T>(Type type, string fieldName)
            where T : class
        {
            FieldInfo field = type.GetField(fieldName, BindingFlags.Static |
                BindingFlags.Public | BindingFlags.NonPublic);
            return field != null && field.FieldType == typeof(T) ?
                field.GetValue(null) as T : null;
        }

        private static BlueprintFeatureSelection ReadSelection(
            BlueprintComponent component)
        {
            FieldInfo field = component.GetType().GetField("selection",
                BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.NonPublic);
            return field != null && field.FieldType ==
                typeof(BlueprintFeatureSelection) ? field.GetValue(component)
                    as BlueprintFeatureSelection : null;
        }

        private static bool TryReadTraitsSetting(Type main, out bool enabled)
        {
            enabled = false;
            FieldInfo settingsField = main.GetField("settings",
                BindingFlags.Static | BindingFlags.Public |
                BindingFlags.NonPublic);
            object settings = settingsField == null ? null :
                settingsField.GetValue(null);
            PropertyInfo property = settings == null ? null :
                settings.GetType().GetProperty("enable_traits",
                    BindingFlags.Instance | BindingFlags.Public |
                    BindingFlags.NonPublic);
            if (property == null || property.PropertyType != typeof(bool) ||
                property.GetIndexParameters().Length != 0) return false;
            enabled = (bool)property.GetValue(settings, null);
            return true;
        }

        private static bool Contains(BlueprintFeature[] values,
            BlueprintFeature value)
        {
            return values != null && values.Any(candidate =>
                ReferenceEquals(candidate, value));
        }

        private static int Count(BlueprintFeature[] values,
            BlueprintFeature feature)
        {
            return values == null ? 0 : values.Count(value =>
                ReferenceEquals(value, feature) || value != null &&
                string.Equals(value.AssetGuid, feature.AssetGuid,
                    StringComparison.Ordinal));
        }

        private static bool Exact(BlueprintScriptableObject blueprint,
            string guid, string internalName)
        {
            return blueprint != null && string.Equals(blueprint.AssetGuid, guid,
                StringComparison.Ordinal) && string.Equals(blueprint.name,
                    internalName, StringComparison.Ordinal);
        }

        private static string Hash(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return "missing";
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace(
                    "-", string.Empty);
        }

        private static AidAnotherContractResolution<FavoredClassTraitContract>
            Result(OptionalAidAnotherAvailability availability,
                string failedCheck, FavoredClassTraitContract contract)
        {
            return new AidAnotherContractResolution<FavoredClassTraitContract>(
                availability, failedCheck, contract);
        }
    }
}
