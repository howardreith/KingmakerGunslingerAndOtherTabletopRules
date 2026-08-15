using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics;

namespace KingmakerGunslinger.BrownFur
{
    internal static class BrownFurModifierAdjustmentRuntime
    {
        private sealed class Scope
        {
            internal string TransactionIdentity;
            internal MechanicsContext RootContext;
            internal UnitEntityData Caster;
            internal BlueprintAbility Spell;
            internal BrownFurAbilityScore SelectedStat;
            internal int Increase;
            internal HashSet<string> BuffGuids;
            internal HashSet<string> CarrierFamilies;
        }

        private static readonly object Gate = new object();
        private static readonly Dictionary<string, Scope> Scopes =
            new Dictionary<string, Scope>(StringComparer.Ordinal);
        private static readonly BrownFurModifierAdjustmentTracker<
            ModifiableValue.Modifier> Tracker =
                new BrownFurModifierAdjustmentTracker<
                    ModifiableValue.Modifier>();

        internal static int ActiveScopeCount
        { get { lock (Gate) return Scopes.Count; } }

        internal static bool Begin(string transactionIdentity,
            MechanicsContext rootContext, UnitEntityData caster,
            BlueprintAbility spell, BrownFurAbilityScore selectedStat,
            int increase, IEnumerable<string> buffGuids,
            IEnumerable<string> carrierFamilies)
        {
            if (string.IsNullOrWhiteSpace(transactionIdentity) ||
                rootContext == null || caster == null || spell == null ||
                selectedStat == BrownFurAbilityScore.None ||
                (increase != 2 && increase != 4)) return false;
            var buffs = new HashSet<string>((buffGuids ??
                Enumerable.Empty<string>()).Select(NormalizeGuid).Where(value =>
                    value.Length == 32), StringComparer.Ordinal);
            var families = new HashSet<string>((carrierFamilies ??
                Enumerable.Empty<string>()).Where(value =>
                    BrownFurModifierAdjustmentPolicy.IsSupportedCarrier(value)),
                StringComparer.Ordinal);
            if (buffs.Count == 0 || families.Count == 0) return false;
            var scope = new Scope { TransactionIdentity = transactionIdentity,
                RootContext = rootContext, Caster = caster, Spell = spell,
                SelectedStat = selectedStat, Increase = increase,
                BuffGuids = buffs, CarrierFamilies = families };
            lock (Gate)
            {
                if (Scopes.ContainsKey(transactionIdentity) || Scopes.Values.Any(
                    value => ReferenceEquals(value.RootContext, rootContext)))
                    return false;
                Scopes.Add(transactionIdentity, scope);
                return true;
            }
        }

        internal static bool Release(string transactionIdentity)
        {
            if (string.IsNullOrWhiteSpace(transactionIdentity)) return false;
            bool removed;
            lock (Gate) removed = Scopes.Remove(transactionIdentity);
            Tracker.Release(transactionIdentity);
            return removed;
        }

        internal static void Clear()
        {
            lock (Gate) Scopes.Clear();
            Tracker.Clear();
        }

        internal static int AdjustedModifierCount(string transactionIdentity)
        { return Tracker.AdjustedModifierCount(transactionIdentity); }

        internal static bool TryAdjust(ModifiableValue destination,
            ModifiableValue.Modifier modifier)
        {
            if (destination == null || modifier == null ||
                modifier.Source == null) return false;
            Buff source = modifier.Source as Buff;
            MechanicsContext sourceContext = modifier.Source.MaybeContext;
            if (source == null || sourceContext == null) return false;
            Scope scope = FindScope(sourceContext);
            if (scope == null ||
                !ReferenceEquals(sourceContext.MaybeCaster, scope.Caster) ||
                !ReferenceEquals(sourceContext.SourceAbility, scope.Spell) ||
                !scope.BuffGuids.Contains(NormalizeGuid(
                    source.Blueprint.AssetGuid.ToString()))) return false;
            string family = CarrierFamily(modifier.SourceComponent);
            if (!scope.CarrierFamilies.Contains(family)) return false;
            BrownFurAbilityScore stat = AbilityScore(destination.Type);
            var request = new BrownFurModifierAdjustmentRequest {
                ExecutionCommitted = true,
                SelectedAbilityScore = scope.SelectedStat,
                ModifierAbilityScore = stat,
                OriginalValue = modifier.ModValue,
                Increase = scope.Increase,
                OriginalDescriptor = modifier.ModDescriptor.ToString(),
                CarrierFamily = family,
                SourceFact = source,
                ExpectedSourceFact = source,
                SourceContext = sourceContext,
                ExpectedSourceContext = sourceContext
            };
            BrownFurModifierAdjustmentDecision decision;
            if (!Tracker.TryAdjust(scope.TransactionIdentity, modifier, request,
                out decision)) return false;
            modifier.ModValue = decision.AdjustedValue;
            return true;
        }

        private static Scope FindScope(MechanicsContext sourceContext)
        {
            lock (Gate)
            {
                MechanicsContext current = sourceContext;
                for (int depth = 0; current != null && depth < 24; depth++)
                {
                    Scope match = Scopes.Values.FirstOrDefault(value =>
                        ReferenceEquals(value.RootContext, current));
                    if (match != null) return match;
                    MechanicsContext parent = current.ParentContext;
                    if (ReferenceEquals(parent, current)) return null;
                    current = parent;
                }
                return null;
            }
        }

        private static string CarrierFamily(string sourceComponent)
        {
            if (string.IsNullOrWhiteSpace(sourceComponent) ||
                sourceComponent[0] != '$') return string.Empty;
            int end = sourceComponent.IndexOf('$', 1);
            return end <= 1 ? string.Empty :
                sourceComponent.Substring(1, end - 1);
        }

        private static BrownFurAbilityScore AbilityScore(StatType stat)
        {
            switch (stat)
            {
                case StatType.Strength: return BrownFurAbilityScore.Strength;
                case StatType.Dexterity: return BrownFurAbilityScore.Dexterity;
                case StatType.Constitution:
                    return BrownFurAbilityScore.Constitution;
                case StatType.Intelligence:
                    return BrownFurAbilityScore.Intelligence;
                case StatType.Wisdom: return BrownFurAbilityScore.Wisdom;
                case StatType.Charisma: return BrownFurAbilityScore.Charisma;
                default: return BrownFurAbilityScore.None;
            }
        }

        private static string NormalizeGuid(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            string normalized = value.Replace("-", string.Empty).ToLowerInvariant();
            Guid parsed;
            return normalized.Length == 32 &&
                Guid.TryParseExact(normalized, "N", out parsed) ? normalized :
                string.Empty;
        }
    }
}
