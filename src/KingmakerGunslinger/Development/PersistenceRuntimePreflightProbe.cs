using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Kingmaker.Items;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Persistence;

namespace KingmakerGunslinger.Development
{
    /// <summary>
    /// Captures the trusted runtime measurements for persistence-matrix rows I01 and
    /// I02. The pure evaluator decides PASS, FAIL, or BLOCKED from these observations.
    /// </summary>
    internal static class PersistenceRuntimePreflightProbe
    {
        private const string IdentityMemberName = "UniqueId";

        internal static PersistenceRuntimePreflightReport Capture()
        {
            bool bootstrapInspectionSucceeded;
            bool bootstrapInitialized = false;
            int initializationCount = -1;
            int registeredBlueprintCount = -1;
            string bootstrapDetail;
            try
            {
                bootstrapInitialized = BlueprintBootstrap.IsInitialized;
                initializationCount = BlueprintBootstrap.InitializationCount;
                registeredBlueprintCount = BlueprintBootstrap.RegisteredBlueprintCount;
                bootstrapInspectionSucceeded = true;
                bootstrapDetail = string.Format(
                    CultureInfo.InvariantCulture,
                    "observations={0}",
                    BlueprintBootstrap.ObservationCount);
            }
            catch (Exception exception)
            {
                bootstrapInspectionSucceeded = false;
                bootstrapDetail = exception.GetType().Name + ": " + exception.Message;
            }

            bool identityInspectionSucceeded;
            int identityMemberCount = -1;
            bool identityMemberReadable = false;
            string identityMemberValueType = string.Empty;
            string identityDetail;
            try
            {
                List<MemberInfo> members = FindIdentityMembers(typeof(ItemEntityWeapon));
                identityMemberCount = members.Count;
                if (members.Count == 1)
                {
                    MemberInfo member = members[0];
                    identityMemberReadable = IsReadable(member);
                    Type valueType = GetValueType(member);
                    identityMemberValueType = valueType == null
                        ? string.Empty
                        : valueType.FullName ?? valueType.Name;
                    identityDetail = string.Format(
                        CultureInfo.InvariantCulture,
                        "member={0}.{1}; kind={2}",
                        member.DeclaringType == null
                            ? "<unknown>"
                            : member.DeclaringType.FullName,
                        member.Name,
                        member.MemberType);
                }
                else
                {
                    identityDetail = members.Count == 0
                        ? "No inherited member named UniqueId was found."
                        : "Multiple inherited members named UniqueId were found: " +
                          string.Join(",", members.ConvertAll(DescribeMember).ToArray());
                }

                identityInspectionSucceeded = true;
            }
            catch (Exception exception)
            {
                identityInspectionSucceeded = false;
                identityDetail = exception.GetType().Name + ": " + exception.Message;
            }

            var probe = new PersistenceRuntimePreflightProbeData(
                bootstrapInspectionSucceeded,
                bootstrapInitialized,
                initializationCount,
                registeredBlueprintCount,
                BlueprintBootstrap.ExpectedRegisteredBlueprintCount,
                bootstrapDetail,
                identityInspectionSucceeded,
                identityMemberCount,
                identityMemberReadable,
                identityMemberValueType,
                identityDetail);
            return PersistenceRuntimePreflightEvaluator.Evaluate(probe);
        }

        private static List<MemberInfo> FindIdentityMembers(Type runtimeType)
        {
            if (runtimeType == null)
            {
                throw new ArgumentNullException("runtimeType");
            }

            var result = new List<MemberInfo>();
            const BindingFlags Flags = BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.DeclaredOnly;
            for (Type current = runtimeType; current != null; current = current.BaseType)
            {
                PropertyInfo property = current.GetProperty(IdentityMemberName, Flags);
                if (property != null && property.GetIndexParameters().Length == 0)
                {
                    result.Add(property);
                }

                FieldInfo field = current.GetField(IdentityMemberName, Flags);
                if (field != null)
                {
                    result.Add(field);
                }
            }

            return result;
        }

        private static bool IsReadable(MemberInfo member)
        {
            PropertyInfo property = member as PropertyInfo;
            if (property != null)
            {
                return property.GetGetMethod(true) != null &&
                    property.GetIndexParameters().Length == 0;
            }

            return member is FieldInfo;
        }

        private static Type GetValueType(MemberInfo member)
        {
            PropertyInfo property = member as PropertyInfo;
            if (property != null)
            {
                return property.PropertyType;
            }

            FieldInfo field = member as FieldInfo;
            return field == null ? null : field.FieldType;
        }

        private static string DescribeMember(MemberInfo member)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}.{1}:{2}",
                member.DeclaringType == null ? "<unknown>" : member.DeclaringType.FullName,
                member.Name,
                member.MemberType);
        }
    }
}
