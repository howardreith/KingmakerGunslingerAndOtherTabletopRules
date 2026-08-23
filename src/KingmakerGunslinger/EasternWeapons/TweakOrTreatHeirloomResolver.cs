using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using KingmakerGunslinger.AidAnotherCompatibility;
using UnityModManagerNet;

namespace KingmakerGunslinger.EasternWeapons
{
    internal sealed class TweakOrTreatHeirloomContract
    {
        internal TweakOrTreatHeirloomContract(Assembly assembly,
            MethodInfo loadMethod, int transformedRacialChoices,
            string fingerprint)
        {
            Assembly = assembly;
            LoadMethod = loadMethod;
            TransformedRacialChoices = transformedRacialChoices;
            Fingerprint = fingerprint ?? string.Empty;
        }

        internal Assembly Assembly { get; private set; }
        internal MethodInfo LoadMethod { get; private set; }
        internal int TransformedRacialChoices { get; private set; }
        internal string Fingerprint { get; private set; }
    }

    /// <summary>
    /// Read-only structural observer for Tweak or Treat 1.1.0's installed
    /// HeirloomWeapon integration. KMG never invokes or patches the method;
    /// it proves that the earlier LoadDictionary postfix completed before
    /// appending its own non-racial Nodachi choice.
    /// </summary>
    internal static class TweakOrTreatHeirloomResolver
    {
        internal const string ModId = "TweakOrTreat";
        internal const string AssemblyName = "TweakOrTreat";
        internal const string TypeName = "TweakOrTreat.HeirloomWeapon";
        internal const string MethodName = "load";
        private const string ForeignRacePrerequisite =
            "ZFavoredClass.NewMechanics.PrerequisiteRace";

        internal static AidAnotherContractResolution<
            TweakOrTreatHeirloomContract> Resolve(
                UnityModManager.ModEntry entry,
                FavoredClassTraitContract favored)
        {
            if (entry == null) return Result(
                OptionalAidAnotherAvailability.Absent,
                "tweak-or-treat-absent", null);
            if (entry.Info == null || !string.Equals(entry.Info.Id, ModId,
                    StringComparison.Ordinal))
                return Result(OptionalAidAnotherAvailability.Blocked,
                    "tweak-or-treat-umm-id", null);
            if (!entry.Loaded || !entry.Active || !entry.HasAssembly ||
                entry.ErrorOnLoading || entry.Assembly == null)
                return Result(OptionalAidAnotherAvailability.Pending,
                    "tweak-or-treat-not-active", null);
            if (favored == null)
                return Result(OptionalAidAnotherAvailability.Blocked,
                    "tweak-or-treat-without-favored-contract", null);
            try
            {
                Assembly assembly = entry.Assembly;
                if (!string.Equals(assembly.GetName().Name, AssemblyName,
                        StringComparison.Ordinal))
                    return Result(OptionalAidAnotherAvailability.Blocked,
                        "tweak-or-treat-assembly-name", null);
                Type type = assembly.GetType(TypeName, false, false);
                MethodInfo method = type == null ? null : type.GetMethod(
                    MethodName, BindingFlags.Static | BindingFlags.Public |
                    BindingFlags.NonPublic, null, Type.EmptyTypes, null);
                if (method == null || method.ReturnType != typeof(void) ||
                    method.IsGenericMethod)
                    return Result(OptionalAidAnotherAvailability.Blocked,
                        "tweak-or-treat-heirloom-signature", null);

                BlueprintFeature[] choices = favored.EquipmentTraits
                    .AllFeatures ?? new BlueprintFeature[0];
                int foreignRace = choices.Sum(choice =>
                    (choice.ComponentsArray ?? new BlueprintComponent[0])
                        .Count(component => component != null && string.Equals(
                            component.GetType().FullName,
                            ForeignRacePrerequisite,
                            StringComparison.Ordinal)));
                if (foreignRace != 0)
                    return Result(OptionalAidAnotherAvailability.Pending,
                        "tweak-or-treat-heirloom-not-reconciled", null);
                int transformed = choices.Sum(choice =>
                    (choice.ComponentsArray ?? new BlueprintComponent[0])
                        .Count(component => component != null &&
                            component.GetType() ==
                                typeof(PrerequisiteFeature)));
                if (transformed != 5)
                    return Result(OptionalAidAnotherAvailability.Blocked,
                        "tweak-or-treat-racial-heirloom-transformations", null);
                string fingerprint = "ummId=" + entry.Info.Id +
                    ";ummVersion=" + entry.Info.Version + ";assembly=" +
                    assembly.FullName + ";mvid=" +
                    assembly.ManifestModule.ModuleVersionId + ";sha256=" +
                    Hash(assembly.Location) + ";equipmentChoices=" +
                    choices.Length + ";foreignRacePrerequisites=0" +
                    ";transformedRacialChoices=" + transformed;
                return Result(OptionalAidAnotherAvailability.Compatible,
                    string.Empty, new TweakOrTreatHeirloomContract(assembly,
                        method, transformed, fingerprint));
            }
            catch (Exception exception)
            {
                return Result(OptionalAidAnotherAvailability.Blocked,
                    "tweak-or-treat-resolver-exception:" +
                    exception.GetType().Name, null);
            }
        }

        private static string Hash(string path)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace(
                    "-", string.Empty);
        }

        private static AidAnotherContractResolution<
            TweakOrTreatHeirloomContract> Result(
                OptionalAidAnotherAvailability availability,
                string failedCheck, TweakOrTreatHeirloomContract contract)
        {
            return new AidAnotherContractResolution<
                TweakOrTreatHeirloomContract>(availability, failedCheck,
                    contract);
        }
    }
}
