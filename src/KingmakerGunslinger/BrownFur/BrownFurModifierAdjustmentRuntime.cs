using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;

namespace KingmakerGunslinger.BrownFur
{
    internal static class BrownFurModifierAdjustmentRuntime
    {
        internal sealed class OrdinaryRecastState
        {
            internal string BuffGuid;
            internal string SpellGuid;
            internal string CasterId;
            internal StatType Stat;
            internal int OriginalValue;
            internal int Increase;
            internal string Descriptor;
            internal string CarrierFamily;
        }

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
            if (scope == null)
                return TryRestorePersisted(destination, modifier, source,
                    sourceContext);
            if (
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
            if (!Remember(destination, source, sourceContext, stat,
                decision.AdjustedValue - modifier.ModValue,
                modifier.ModValue, modifier.ModDescriptor.ToString(), family))
                return false;
            modifier.ModValue = decision.AdjustedValue;
            return true;
        }

        internal static int Forget(Buff source)
        {
            if (source == null || source.Owner == null ||
                source.Context == null || source.Context.MaybeCaster == null ||
                source.Context.SourceAbility == null) return 0;
            UnitPartBrownFurModifierPersistence part = source.Owner.Get<
                UnitPartBrownFurModifierPersistence>();
            return part == null ? 0 : part.Forget(NormalizeGuid(
                source.Blueprint.AssetGuid.ToString()), NormalizeGuid(
                source.Context.SourceAbility.AssetGuid.ToString()),
                source.Context.MaybeCaster.UniqueId);
        }

        internal static OrdinaryRecastState PrepareOrdinaryRecast(
            BuffCollection collection, BlueprintBuff blueprint,
            MechanicsContext applyingContext)
        {
            if (collection == null || blueprint == null ||
                applyingContext == null ||
                applyingContext.MaybeCaster == null ||
                applyingContext.SourceAbility == null ||
                FindScope(applyingContext) != null) return null;
            Buff[] sources = collection.RawFacts.OfType<Buff>().Where(value =>
                ReferenceEquals(value.Blueprint, blueprint)).Take(2).ToArray();
            if (sources.Length != 1 || sources[0].Owner == null) return null;
            Buff source = sources[0];
            UnitPartBrownFurModifierPersistence part = source.Owner.Get<
                UnitPartBrownFurModifierPersistence>();
            if (part == null) return null;
            string buffGuid = NormalizeGuid(
                source.Blueprint.AssetGuid.ToString());
            string spellGuid = NormalizeGuid(
                applyingContext.SourceAbility.AssetGuid.ToString());
            string casterId = applyingContext.MaybeCaster.UniqueId;
            var matches = new List<OrdinaryRecastState>();
            foreach (StatType type in new[] { StatType.Strength,
                StatType.Dexterity, StatType.Constitution,
                StatType.Intelligence, StatType.Wisdom,
                StatType.Charisma })
            {
                ModifiableValue stat = source.Owner.Stats.GetStat(type);
                foreach (ModifiableValue.Modifier modifier in stat.Modifiers
                    .Where(value => ReferenceEquals(value.Source, source))
                    .ToArray())
                {
                    string family = CarrierFamily(modifier.SourceComponent);
                    BrownFurPersistedModifierRecord record =
                        part.ResolveOrdinaryRecast(
                            new BrownFurOrdinaryRecastProbe {
                                BuffGuid = buffGuid, SpellGuid = spellGuid,
                                CasterId = casterId,
                                AbilityScore = AbilityScore(type),
                                CurrentValue = modifier.ModValue,
                                CurrentDescriptor =
                                    modifier.ModDescriptor.ToString(),
                                CarrierFamily = family
                            });
                    if (record == null) continue;
                    matches.Add(new OrdinaryRecastState {
                        BuffGuid = buffGuid, SpellGuid = spellGuid,
                        CasterId = casterId, Stat = type,
                        OriginalValue = record.OriginalValue,
                        Increase = record.Increase,
                        Descriptor = record.OriginalDescriptor,
                        CarrierFamily = record.CarrierFamily
                    });
                }
            }
            return matches.Count == 1 ? matches[0] : null;
        }

        internal static bool RestoreOrdinaryRecast(Buff source,
            OrdinaryRecastState state)
        {
            if (source == null || source.Owner == null || state == null)
                return false;
            ModifiableValue stat = source.Owner.Stats.GetStat(state.Stat);
            ModifiableValue.Modifier[] modifiers = stat.Modifiers.Where(value =>
                ReferenceEquals(value.Source, source) &&
                string.Equals(value.ModDescriptor.ToString(),
                    state.Descriptor, StringComparison.Ordinal) &&
                string.Equals(CarrierFamily(value.SourceComponent),
                    state.CarrierFamily, StringComparison.Ordinal) &&
                (value.ModValue == state.OriginalValue ||
                 value.ModValue == state.OriginalValue + state.Increase))
                .Take(2).ToArray();
            if (modifiers.Length != 1) return false;
            modifiers[0].ModValue = state.OriginalValue;
            UnitPartBrownFurModifierPersistence part = source.Owner.Get<
                UnitPartBrownFurModifierPersistence>();
            return part != null && part.Forget(state.BuffGuid,
                state.SpellGuid, state.CasterId) > 0;
        }

        private static bool TryRestorePersisted(ModifiableValue destination,
            ModifiableValue.Modifier modifier, Buff source,
            MechanicsContext sourceContext)
        {
            if (destination.Owner == null || sourceContext.MaybeCaster == null ||
                sourceContext.SourceAbility == null) return false;
            UnitPartBrownFurModifierPersistence part = destination.Owner.Get<
                UnitPartBrownFurModifierPersistence>();
            if (part == null) return false;
            BrownFurAbilityScore stat = AbilityScore(destination.Type);
            string family = CarrierFamily(modifier.SourceComponent);
            var probe = new BrownFurPersistedModifierProbe {
                BuffGuid = NormalizeGuid(source.Blueprint.AssetGuid.ToString()),
                SpellGuid = NormalizeGuid(
                    sourceContext.SourceAbility.AssetGuid.ToString()),
                CasterId = sourceContext.MaybeCaster.UniqueId,
                AbilityScore = stat,
                OriginalValue = modifier.ModValue,
                OriginalDescriptor = modifier.ModDescriptor.ToString(),
                CarrierFamily = family,
                EndTimeTicks = source.EndTime.Ticks
            };
            int increase = part.ResolveIncrease(probe);
            if (increase == 0) return false;
            BrownFurModifierAdjustmentDecision decision =
                BrownFurModifierAdjustmentPolicy.Decide(
                    new BrownFurModifierAdjustmentRequest {
                        ExecutionCommitted = true,
                        SelectedAbilityScore = stat,
                        ModifierAbilityScore = stat,
                        OriginalValue = modifier.ModValue,
                        Increase = increase,
                        OriginalDescriptor = modifier.ModDescriptor.ToString(),
                        CarrierFamily = family,
                        SourceFact = source,
                        ExpectedSourceFact = source,
                        SourceContext = sourceContext,
                        ExpectedSourceContext = sourceContext
                    });
            if (!decision.Eligible) return false;
            modifier.ModValue = decision.AdjustedValue;
            return true;
        }

        private static bool Remember(ModifiableValue destination, Buff source,
            MechanicsContext sourceContext, BrownFurAbilityScore stat,
            int increase, int originalValue, string originalDescriptor,
            string family)
        {
            if (destination.Owner == null || sourceContext.MaybeCaster == null ||
                sourceContext.SourceAbility == null) return false;
            UnitPartBrownFurModifierPersistence part = destination.Owner.Ensure<
                UnitPartBrownFurModifierPersistence>();
            if (part == null) return false;
            return part.Remember(new BrownFurPersistedModifierRecord(
                NormalizeGuid(source.Blueprint.AssetGuid.ToString()),
                NormalizeGuid(sourceContext.SourceAbility.AssetGuid.ToString()),
                sourceContext.MaybeCaster.UniqueId, stat, increase,
                originalValue, originalDescriptor, family,
                source.EndTime.Ticks));
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
