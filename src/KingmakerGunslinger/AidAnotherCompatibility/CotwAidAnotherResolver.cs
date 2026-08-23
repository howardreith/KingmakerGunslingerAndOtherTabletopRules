using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Components;
using UnityModManagerNet;

namespace KingmakerGunslinger.AidAnotherCompatibility
{
    internal static class CotwAidAnotherResolver
    {
        internal const string ModId = "CallOfTheWild";
        internal const string AssemblyName = "CallOfTheWild";
        internal const string RebalanceTypeName = "CallOfTheWild.Rebalance";
        internal const string CreationMethodName = "createAidAnother";
        internal const string AttackBuffGuid =
            "91c27d7593614e06a22c0d74106377f6";
        internal const string ArmorClassBuffGuid =
            "fd60ba2291144d9a89890dfb1fec561a";
        internal const string OrdinaryAbilityGuid =
            "ab00871bf2914b3ba492fdb2f1af8875";
        internal const string SelfAbilityGuid =
            "e24a160d13b549e8a36c219e686ac319";
        internal const string BenevolentFeatureGuid =
            "4b6bcd49fd20498bbbf278b0d65945bc";
        internal const string CommunityBoostFeatureGuid =
            "85f156e44c3c4dabb2da66dd7a35e7a3";

        internal static AidAnotherContractResolution<CotwAidAnotherContract>
            Resolve(UnityModManager.ModEntry entry)
        {
            if (entry == null)
                return Result(OptionalAidAnotherAvailability.Absent,
                    "cotw-absent", null);
            if (entry.Info == null || !string.Equals(entry.Info.Id, ModId,
                    StringComparison.Ordinal))
                return Result(OptionalAidAnotherAvailability.Blocked,
                    "cotw-umm-id", null);
            if (!entry.Loaded || !entry.Active || !entry.HasAssembly ||
                entry.ErrorOnLoading || entry.Assembly == null)
                return Result(OptionalAidAnotherAvailability.Pending,
                    "cotw-not-active", null);
            Assembly assembly = entry.Assembly;
            if (!string.Equals(assembly.GetName().Name, AssemblyName,
                    StringComparison.Ordinal))
                return Result(OptionalAidAnotherAvailability.Blocked,
                    "cotw-assembly-name", null);

            try
            {
                Type rebalance = assembly.GetType(RebalanceTypeName, false, false);
                if (rebalance == null)
                    return Result(OptionalAidAnotherAvailability.Blocked,
                        "cotw-rebalance-type", null);
                MethodInfo creation = rebalance.GetMethod(CreationMethodName,
                    BindingFlags.Static | BindingFlags.Public |
                    BindingFlags.NonPublic, null, Type.EmptyTypes, null);
                if (creation == null || creation.ReturnType != typeof(void) ||
                    creation.IsGenericMethod)
                    return Result(OptionalAidAnotherAvailability.Blocked,
                        "cotw-create-aid-signature", null);

                FieldInfo configField = RequireStaticField(rebalance,
                    "aid_another_config", typeof(ContextRankConfig));
                FieldInfo buffsField = RequireStaticField(rebalance,
                    "aid_another_buffs", typeof(BlueprintBuff[]));
                FieldInfo abilityField = RequireStaticField(rebalance,
                    "aid_another", typeof(BlueprintAbility));
                FieldInfo selfField = RequireStaticField(rebalance,
                    "aid_self_free", typeof(BlueprintAbility));
                if (configField == null || buffsField == null ||
                    abilityField == null || selfField == null)
                    return Result(OptionalAidAnotherAvailability.Blocked,
                        "cotw-static-fields", null);

                var configuration = configField.GetValue(null) as
                    ContextRankConfig;
                var buffs = buffsField.GetValue(null) as BlueprintBuff[];
                var ordinary = abilityField.GetValue(null) as BlueprintAbility;
                var self = selfField.GetValue(null) as BlueprintAbility;
                if (configuration == null || buffs == null || ordinary == null ||
                    self == null)
                    return Result(OptionalAidAnotherAvailability.Pending,
                        "cotw-aid-not-created", null);

                Type configType = typeof(ContextRankConfig);
                FieldInfo baseField = RequireInstanceField(configType,
                    "m_BaseValueType", typeof(ContextRankBaseValueType));
                FieldInfo progressionField = RequireInstanceField(configType,
                    "m_Progression", typeof(ContextRankProgression));
                FieldInfo stepField = RequireInstanceField(configType,
                    "m_StepLevel", typeof(int));
                FieldInfo listField = RequireInstanceField(configType,
                    "m_FeatureList", typeof(BlueprintFeature[]));
                if (baseField == null || progressionField == null ||
                    stepField == null || listField == null)
                    return Result(OptionalAidAnotherAvailability.Blocked,
                        "cotw-config-fields", null);
                if ((ContextRankBaseValueType)baseField.GetValue(configuration) !=
                        ContextRankBaseValueType.FeatureList ||
                    (ContextRankProgression)progressionField.GetValue(
                        configuration) != ContextRankProgression.BonusValue ||
                    (int)stepField.GetValue(configuration) !=
                        AidAnotherGrantResolver.NormalBaseGrant)
                    return Result(OptionalAidAnotherAvailability.Blocked,
                        "cotw-config-shape", null);

                BlueprintFeature[] features = listField.GetValue(configuration)
                    as BlueprintFeature[];
                if (features == null || features.Any(value => value == null))
                    return Result(OptionalAidAnotherAvailability.Blocked,
                        "cotw-feature-list", null);
                if (buffs.Length != 2 || buffs.Any(value => value == null) ||
                    !Exact(buffs[0], AttackBuffGuid,
                        "WarpriestCommunityBlessingAidAnother1Buff") ||
                    !Exact(buffs[1], ArmorClassBuffGuid,
                        "WarpriestCommunityBlessingAidAnother2Buff"))
                    return Result(OptionalAidAnotherAvailability.Blocked,
                        "cotw-aid-buffs", null);
                if (buffs.Any(value => value.ComponentsArray == null ||
                    value.ComponentsArray.Count(component =>
                        ReferenceEquals(component, configuration)) != 1))
                    return Result(OptionalAidAnotherAvailability.Blocked,
                        "cotw-shared-config-consumers", null);
                if (!Exact(ordinary, OrdinaryAbilityGuid, "AidAnotherAbilityBase") ||
                    !Exact(self, SelfAbilityGuid, "AidSelfFreeAbilityBase"))
                    return Result(OptionalAidAnotherAvailability.Blocked,
                        "cotw-aid-abilities", null);

                string contributors = string.Join(",", features.GroupBy(value =>
                    value.AssetGuid, StringComparer.Ordinal).OrderBy(value =>
                        value.Key, StringComparer.Ordinal).Select(value =>
                            value.Key + "*" + value.Count().ToString(
                                CultureInfo.InvariantCulture)).ToArray());
                string fingerprint = "ummId=" + entry.Info.Id +
                    ";ummVersion=" + entry.Info.Version +
                    ";assembly=" + assembly.FullName +
                    ";mvid=" + assembly.ManifestModule.ModuleVersionId +
                    ";sha256=" + Hash(assembly.Location) +
                    ";base=FeatureList;progression=BonusValue;step=2" +
                    ";attackBuff=" + buffs[0].AssetGuid +
                    ";acBuff=" + buffs[1].AssetGuid +
                    ";ordinary=" + ordinary.AssetGuid +
                    ";self=" + self.AssetGuid +
                    ";contributors=" + contributors;
                return Result(OptionalAidAnotherAvailability.Compatible,
                    string.Empty, new CotwAidAnotherContract(assembly, creation,
                        configuration, listField, buffs, ordinary, self,
                        fingerprint));
            }
            catch (Exception exception)
            {
                return Result(OptionalAidAnotherAvailability.Blocked,
                    "cotw-resolver-exception:" + exception.GetType().Name, null);
            }
        }

        private static bool Exact(BlueprintScriptableObject blueprint,
            string guid, string internalName)
        {
            return blueprint != null && string.Equals(blueprint.AssetGuid, guid,
                StringComparison.Ordinal) && string.Equals(blueprint.name,
                    internalName, StringComparison.Ordinal);
        }

        private static FieldInfo RequireStaticField(Type type, string name,
            Type expected)
        {
            FieldInfo field = type.GetField(name, BindingFlags.Static |
                BindingFlags.Public | BindingFlags.NonPublic);
            return field != null && field.FieldType == expected ? field : null;
        }

        private static FieldInfo RequireInstanceField(Type type, string name,
            Type expected)
        {
            FieldInfo field = type.GetField(name, BindingFlags.Instance |
                BindingFlags.Public | BindingFlags.NonPublic);
            return field != null && field.FieldType == expected ? field : null;
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

        private static AidAnotherContractResolution<CotwAidAnotherContract>
            Result(OptionalAidAnotherAvailability availability,
                string failedCheck, CotwAidAnotherContract contract)
        {
            return new AidAnotherContractResolution<CotwAidAnotherContract>(
                availability, failedCheck, contract);
        }
    }
}
