using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Mechanics.Actions;
using KingmakerGunslinger.Blueprints;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Deliberately strict runtime proof for the learning-only filter. Reads
    /// fields, never executes getters/actions or enters referenced blueprints.
    /// Unlike the general summon signature, every nested AfterSpawn payload
    /// and private duration/rank field participates. An unsupported value,
    /// cycle or traversal limit preserves the source row.
    /// </summary>
    internal static class StandaloneElementalInvocationContract
    {
        private const BindingFlags Fields = BindingFlags.Instance | BindingFlags.Public |
            BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
        private static readonly string[] AbilityFields = {
            "Type", "Range", "CustomRange", "CanTargetPoint", "CanTargetEnemies",
            "CanTargetFriends", "CanTargetSelf", "SpellResistance", "NeedEquipWeapons",
            "EffectOnAlly", "EffectOnEnemy", "Animation", "HasFastAnimation", "AnimationStyle",
            "m_TargetMapObjects", "ActionType", "AvailableMetamagic", "m_IsFullRoundAction",
            "MaterialComponent"
        };

        internal static bool Equal(BlueprintAbility source, BlueprintAbility canonical,
            string creatureKey, BlueprintUnit canonicalUnit, out string reason)
        {
            reason = "preserve-contract-null-ability";
            if (source == null || canonical == null) return false;
            try
            {
                string nativeUnit = null;
                if (creatureKey != null)
                {
                    nativeUnit = ExpandedSummoningDonorCatalog.For(creatureKey).Guid;
                    if (canonicalUnit == null)
                    { reason = "preserve-contract-missing-canonical-unit"; return false; }
                    var sourceSpawns = AlternateSummoningGraph.Objects<ContextActionSpawnMonster>(
                        source.ComponentsArray);
                    var canonicalSpawns = AlternateSummoningGraph.Objects<ContextActionSpawnMonster>(
                        canonical.ComponentsArray);
                    if (sourceSpawns.Count == 0 || canonicalSpawns.Count == 0 ||
                        sourceSpawns.Any(value => value.Blueprint == null || value.Blueprint.AssetGuid != nativeUnit) ||
                        canonicalSpawns.Any(value => !ReferenceEquals(value.Blueprint, canonicalUnit)))
                    { reason = "preserve-contract-spawn-identity-mismatch"; return false; }
                }
                var left = Capture(source, nativeUnit, creatureKey);
                var right = Capture(canonical, canonicalUnit == null ? null : canonicalUnit.AssetGuid, creatureKey);
                foreach (string path in left.Keys.Concat(right.Keys).Distinct(StringComparer.Ordinal)
                    .OrderBy(value => value, StringComparer.Ordinal))
                {
                    string l, r;
                    if (!left.TryGetValue(path, out l) || !right.TryGetValue(path, out r) || l != r)
                    { reason = "preserve-contract-difference:" + path; return false; }
                }
                reason = "equivalent-live-invocation-contract";
                return true;
            }
            catch (Exception exception)
            {
                reason = "preserve-contract-unproved:" + exception.GetType().Name + ":" + exception.Message;
                if (reason.Length > 240) reason = reason.Substring(0, 240);
                return false;
            }
        }

        private static SortedDictionary<string, string> Capture(BlueprintAbility ability,
            string unitGuid, string creatureKey)
        {
            var capture = new Reader(unitGuid, creatureKey);
            foreach (string name in AbilityFields)
            {
                FieldInfo field = typeof(BlueprintAbility).GetField(name, Fields);
                if (field == null) throw new MissingFieldException(typeof(BlueprintAbility).FullName, name);
                capture.Visit(field.GetValue(ability), "ability." + name, 0);
            }
            // Membership is verified independently on this exact list/level.
            // Variant coverage is verified independently on the live parent.
            // Neither component controls the invocation of an individual child.
            var components = (ability.ComponentsArray ?? new BlueprintComponent[0])
                .Where(value => !(value is SpellListComponent) && !(value is AbilityVariants)).ToArray();
            capture.Visit(components, "components", 0);
            return capture.Values;
        }

        private sealed class Reader
        {
            internal readonly SortedDictionary<string, string> Values =
                new SortedDictionary<string, string>(StringComparer.Ordinal);
            private readonly HashSet<object> _path = new HashSet<object>(ReferenceComparer.Instance);
            private readonly string _unitGuid;
            private readonly string _creatureKey;
            private int _visits;
            internal Reader(string unitGuid, string creatureKey)
            { _unitGuid = unitGuid; _creatureKey = creatureKey; }

            internal void Visit(object value, string path, int depth)
            {
                if (++_visits > 8192 || depth > 48)
                    throw new InvalidOperationException("contract-traversal-limit");
                if (value == null) { Values.Add(path, "null"); return; }
                Type type = value.GetType();
                BlueprintScriptableObject blueprint = value as BlueprintScriptableObject;
                if (blueprint != null)
                {
                    Values.Add(path, type.FullName + ":" + (blueprint is BlueprintUnit &&
                        _creatureKey != null && blueprint.AssetGuid == _unitGuid ?
                            "elemental-species:" + _creatureKey : blueprint.AssetGuid));
                    return;
                }
                if (value is string) { Values.Add(path, "string:" + (string)value); return; }
                if (type.IsPrimitive || type.IsEnum || value is decimal)
                {
                    Values.Add(path, type.FullName + ":" + Convert.ToString(value, CultureInfo.InvariantCulture));
                    return;
                }
                if (value is Delegate || value is MemberInfo ||
                    (value is UnityEngine.Object && !(value is BlueprintComponent) && !(value is Element)))
                    throw new InvalidOperationException("unsupported-contract-value:" + type.FullName);
                if (!type.IsValueType && !_path.Add(value))
                    throw new InvalidOperationException("contract-cycle:" + type.FullName);
                try
                {
                    IEnumerable sequence = value as IEnumerable;
                    if (sequence != null)
                    {
                        // Array and List representations have the same engine
                        // semantics; the ordered contents remain exact.
                        int index = 0;
                        foreach (object item in sequence)
                            Visit(item, path + "[" + index++ + "]", depth + 1);
                        Values.Add(path + ".count", index.ToString(CultureInfo.InvariantCulture));
                        return;
                    }
                    Values.Add(path + ".type", type.FullName);
                    int fields = 0;
                    for (Type current = type; current != null && current != typeof(BlueprintComponent) &&
                        current != typeof(Element) && current != typeof(UnityEngine.ScriptableObject) &&
                        current != typeof(UnityEngine.Object) && current != typeof(object); current = current.BaseType)
                        foreach (FieldInfo field in current.GetFields(Fields)
                            .OrderBy(item => item.Name, StringComparer.Ordinal))
                        {
                            // Prototype links and owner backreferences are
                            // framework identity, not the component payload.
                            if (field.Name == "PrototypeLink" && current.FullName ==
                                "Kingmaker.Blueprints.PrototypeableObjectBase") continue;
                            Visit(field.GetValue(value), path + "." + current.FullName + "." + field.Name, depth + 1);
                            fields++;
                        }
                    if (fields == 0 && !(value is BlueprintComponent) && !(value is Element))
                        throw new InvalidOperationException("unread-contract-value:" + type.FullName);
                }
                finally { if (!type.IsValueType) _path.Remove(value); }
            }
        }

        private sealed class ReferenceComparer : IEqualityComparer<object>
        {
            internal static readonly ReferenceComparer Instance = new ReferenceComparer();
            public new bool Equals(object left, object right) { return ReferenceEquals(left, right); }
            public int GetHashCode(object value) { return RuntimeHelpers.GetHashCode(value); }
        }
    }
}
