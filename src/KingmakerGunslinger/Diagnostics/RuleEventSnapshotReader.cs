using System;
using System.Collections.Generic;
using System.Globalization;
using KingmakerGunslinger.Development;
using UnityEngine;

namespace KingmakerGunslinger.Diagnostics
{
    /// <summary>
    /// Copies the currently available public/private rule-event values into strings and
    /// primitive values. Candidate names are diagnostic adapters, not gameplay decisions.
    /// </summary>
    internal static class RuleEventSnapshotReader
    {
        private const double MetersPerFoot = 0.3048d;
        private const string Unavailable = "<unavailable>";

        internal static CombatTraceObservation Read(
            CombatTraceStage stage,
            CombatTracePhase phase,
            object ruleEvent,
            int eventIdentity,
            int? parentEventIdentity,
            FirearmMarkerSnapshot marker,
            string markerSource)
        {
            if (ruleEvent == null)
            {
                throw new ArgumentNullException("ruleEvent");
            }

            marker = marker ?? FirearmMarkerSnapshot.NoWeapon();
            var fields = new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                { "eventType", ruleEvent.GetType().FullName },
                { "initiator", Unavailable },
                { "target", Unavailable },
                { "naturalD20", Unavailable },
                { "attackBonus", Unavailable },
                { "attackTotal", Unavailable },
                { "attackResult", Unavailable },
                { "isHit", Unavailable },
                { "targetAC", Unavailable },
                { "ordinaryAC", Unavailable },
                { "touchAC", Unavailable },
                { "distanceMeters", Unavailable },
                { "rangeIncrement", Unavailable },
                { "source", Unavailable },
                { "isFullAttack", Unavailable },
                { "isFirstAttack", Unavailable },
                { "isAttackOfOpportunity", Unavailable },
                { "attackNumber", Unavailable }
            };

            marker.AddFields(fields, markerSource);

            object initiator = ReadObject(ruleEvent, "Initiator", "m_Initiator");
            object target = ReadObject(ruleEvent, "Target", "m_Target");
            fields["initiator"] = DescribeObject(initiator);
            fields["target"] = DescribeObject(target);

            fields["naturalD20"] = ReadNaturalD20(ruleEvent);
            fields["attackBonus"] = ReadNumeric(
                ruleEvent,
                "AttackBonus",
                "Bonus",
                "AttackBonusWithoutTarget",
                "Result.AttackBonus");
            fields["attackTotal"] = ReadNumeric(
                ruleEvent,
                "AttackRoll",
                "Total",
                "Result.Total",
                "Result.AttackRoll");
            fields["attackResult"] = ReadText(
                ruleEvent,
                "Result",
                "AttackResult",
                "Outcome");
            fields["isHit"] = ReadBoolean(
                ruleEvent,
                "IsHit",
                "Result.IsHit",
                "AttackResult.IsHit");
            fields["targetAC"] = ReadNumeric(
                ruleEvent,
                "TargetAC",
                "Result.TargetAC");

            if (target != null)
            {
                fields["ordinaryAC"] = ReadNumeric(
                    target,
                    "Stats.AC.ModifiedValue",
                    "Stats.AC.Value",
                    "Descriptor.Stats.AC.ModifiedValue");
                fields["touchAC"] = ReadNumeric(
                    target,
                    "Stats.AC.Touch",
                    "Stats.AC.TouchAC",
                    "Stats.AC.TouchValue",
                    "Descriptor.Stats.AC.Touch");
            }

            double distanceMeters;
            if (TryReadDistanceMeters(initiator, target, out distanceMeters))
            {
                fields["distanceMeters"] = distanceMeters.ToString("0.###", CultureInfo.InvariantCulture);
                if (marker.Definition != null && marker.Definition.HasFixedRangeIncrement)
                {
                    double incrementMeters = marker.Definition.RangeIncrementFeet * MetersPerFoot;
                    fields["rangeIncrement"] = FirearmRangeMath.CalculateIncrement(
                        distanceMeters,
                        incrementMeters).ToString(CultureInfo.InvariantCulture);
                }
                else if (marker.Definition != null)
                {
                    fields["rangeIncrement"] = "special";
                }
            }

            object weaponAttack = ResolveWeaponAttackContext(ruleEvent);
            fields["isFullAttack"] = ReadBoolean(weaponAttack, "IsFullAttack");
            fields["isFirstAttack"] = ReadBoolean(weaponAttack, "IsFirstAttack");
            fields["isAttackOfOpportunity"] = ReadBoolean(
                weaponAttack,
                "IsAttackOfOpportunity",
                "AttackOfOpportunity");
            fields["attackNumber"] = ReadNumeric(
                weaponAttack,
                "AttackNumber",
                "AttackIndex",
                "NumberOfAttacks");
            fields["source"] = ReadSource(ruleEvent);

            return new CombatTraceObservation(
                stage,
                phase,
                eventIdentity,
                parentEventIdentity,
                marker.IsExactFirearm,
                marker.MarkerCount,
                fields);
        }

        private static object ResolveWeaponAttackContext(object ruleEvent)
        {
            if (ruleEvent == null)
            {
                return null;
            }

            string fullName = ruleEvent.GetType().FullName;
            if (fullName != null && fullName.EndsWith(".RuleAttackWithWeapon", StringComparison.Ordinal))
            {
                return ruleEvent;
            }

            object value;
            string ignored;
            if (ReflectionAccess.TryGetFirstNonNullMember(
                ruleEvent,
                new[] { "RuleAttackWithWeapon", "AttackWithWeapon" },
                out value,
                out ignored))
            {
                return value;
            }

            if (ReflectionAccess.TryGetPath(ruleEvent, "Reason.Rule", out value) && value != null)
            {
                return value;
            }

            return ruleEvent;
        }

        private static string ReadNaturalD20(object source)
        {
            string[] paths =
            {
                "D20.Result",
                "D20.Value",
                "D20.Roll",
                "DiceRoll.Result",
                "RollResult",
                "NaturalRoll"
            };

            foreach (string path in paths)
            {
                object value;
                if (ReflectionAccess.TryGetPath(source, path, out value))
                {
                    int number;
                    if (TryConvertInt32(value, out number) && number >= 1 && number <= 20)
                    {
                        return number.ToString(CultureInfo.InvariantCulture);
                    }
                }
            }

            object direct;
            if (ReflectionAccess.TryGetMember(source, "D20", out direct))
            {
                int number;
                if (TryConvertInt32(direct, out number) && number >= 1 && number <= 20)
                {
                    return number.ToString(CultureInfo.InvariantCulture);
                }
            }

            return Unavailable;
        }

        private static object ReadObject(object source, params string[] members)
        {
            object value;
            string ignored;
            return ReflectionAccess.TryGetFirstNonNullMember(
                source,
                members,
                out value,
                out ignored)
                ? value
                : null;
        }

        private static string ReadNumeric(object source, params string[] paths)
        {
            if (source == null)
            {
                return Unavailable;
            }

            foreach (string path in paths)
            {
                object value;
                bool found = path.IndexOf('.') >= 0
                    ? ReflectionAccess.TryGetPath(source, path, out value)
                    : ReflectionAccess.TryGetMember(source, path, out value);
                if (!found || value == null)
                {
                    continue;
                }

                double number;
                if (TryConvertDouble(value, out number))
                {
                    return number.ToString("0.###", CultureInfo.InvariantCulture);
                }
            }

            return Unavailable;
        }

        private static string ReadBoolean(object source, params string[] paths)
        {
            if (source == null)
            {
                return Unavailable;
            }

            foreach (string path in paths)
            {
                object value;
                bool found = path.IndexOf('.') >= 0
                    ? ReflectionAccess.TryGetPath(source, path, out value)
                    : ReflectionAccess.TryGetMember(source, path, out value);
                if (found && value is bool)
                {
                    return ((bool)value).ToString(CultureInfo.InvariantCulture);
                }
            }

            return Unavailable;
        }

        private static string ReadText(object source, params string[] paths)
        {
            if (source == null)
            {
                return Unavailable;
            }

            foreach (string path in paths)
            {
                object value;
                bool found = path.IndexOf('.') >= 0
                    ? ReflectionAccess.TryGetPath(source, path, out value)
                    : ReflectionAccess.TryGetMember(source, path, out value);
                if (found && value != null)
                {
                    return ToInvariantString(value);
                }
            }

            return Unavailable;
        }

        private static string ReadSource(object ruleEvent)
        {
            string[] paths =
            {
                "Reason.Context.SourceAbility",
                "Reason.Ability",
                "SourceAbility",
                "Ability",
                "Reason.Rule",
                "Reason"
            };

            foreach (string path in paths)
            {
                object value;
                if (ReflectionAccess.TryGetPath(ruleEvent, path, out value) && value != null)
                {
                    return DescribeObject(value);
                }
            }

            return Unavailable;
        }

        private static bool TryReadDistanceMeters(object initiator, object target, out double distance)
        {
            distance = 0d;
            if (initiator == null || target == null)
            {
                return false;
            }

            try
            {
                object result;
                string ignored;
                if (ReflectionAccess.TryInvokeAny(
                    initiator,
                    new[] { "DistanceTo" },
                    new[] { new[] { target } },
                    out result,
                    out ignored) &&
                    TryConvertDouble(result, out distance) &&
                    distance >= 0d)
                {
                    return true;
                }
            }
            catch
            {
                // The position fallback below remains read-only and diagnostic.
            }

            object initiatorPosition;
            object targetPosition;
            if (!ReflectionAccess.TryGetMember(initiator, "Position", out initiatorPosition) ||
                !ReflectionAccess.TryGetMember(target, "Position", out targetPosition))
            {
                return false;
            }

            if (initiatorPosition is Vector3 && targetPosition is Vector3)
            {
                distance = Vector3.Distance((Vector3)initiatorPosition, (Vector3)targetPosition);
                return !double.IsNaN(distance) && !double.IsInfinity(distance);
            }

            return false;
        }

        private static string DescribeObject(object value)
        {
            if (value == null)
            {
                return Unavailable;
            }

            object name;
            string ignored;
            if (ReflectionAccess.TryGetFirstNonNullMember(
                value,
                new[] { "CharacterName", "Name", "name" },
                out name,
                out ignored))
            {
                return ToInvariantString(name);
            }

            return value.GetType().FullName;
        }

        private static bool TryConvertInt32(object value, out int result)
        {
            result = 0;
            if (value == null || value is bool)
            {
                return false;
            }

            try
            {
                result = Convert.ToInt32(value, CultureInfo.InvariantCulture);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool TryConvertDouble(object value, out double result)
        {
            result = 0d;
            if (value == null || value is bool)
            {
                return false;
            }

            try
            {
                result = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                return !double.IsNaN(result) && !double.IsInfinity(result);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static string ToInvariantString(object value)
        {
            IFormattable formattable = value as IFormattable;
            return formattable == null
                ? value.ToString()
                : formattable.ToString(null, CultureInfo.InvariantCulture);
        }
    }
}
