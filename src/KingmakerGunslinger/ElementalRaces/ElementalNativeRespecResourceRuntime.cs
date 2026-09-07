using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Class.LevelUp;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.ElementalRaces
{
    // The native respec replacement begins without the original's UnitParts,
    // and SetupNewCharacher applies a rest before its success callback copies
    // that replacement back. This bridge preserves only exact owned daily
    // resource identities and blood healing expenditure through those seams. It never changes selections,
    // facts, stats, resource maxima, or another mod's resources.
    internal static class ElementalNativeRespecResourceRuntime
    {
        [ThreadStatic] private static Scope _pending;
        private static readonly ConditionalWeakTable<UnitDescriptor, Snapshot> Replacements =
            new ConditionalWeakTable<UnitDescriptor, Snapshot>();

        internal sealed class Scope
        {
            internal Scope Previous;
            internal Snapshot Captured;
        }

        internal sealed class Snapshot
        {
            internal UnitEntityData Original;
            internal UnitDescriptor Replacement;
            internal BlueprintAbilityResource[] Catalog;
            internal Dictionary<string, int> Amounts;
            internal int[] BloodSpent;
            internal bool Active = true;

            internal void Preserve(UnitDescriptor target, bool original = false)
            {
                if (!Active || target == null || target.Unit == null ||
                    target.Unit.UniqueId != Original.UniqueId ||
                    (original ? !ReferenceEquals(target, Original.Descriptor) : ReferenceEquals(target, Original.Descriptor)))
                    throw new InvalidOperationException("Elemental respec resource target lost its native owner correlation.");
                if (BloodSpent != null)
                    target.Ensure<UnitPartElementalBloodCapacity>().PreserveRespecExpenditure(BloodSpent[0], BloodSpent[1], BloodSpent[2]);
                var ledger = target.Ensure<UnitPartElementalHeritageState>();
                foreach (var resource in Catalog)
                {
                    int remembered;
                    if (!Amounts.TryGetValue(resource.AssetGuid, out remembered)) continue;
                    int count = target.Resources.PersistantResources.Count(value => value != null && ReferenceEquals(value.Blueprint, resource));
                    if (count > 1) throw new InvalidOperationException("Elemental respec resource identity is duplicated.");
                    if (count == 1)
                    {
                        int current = target.Resources.GetResourceAmount(resource);
                        int desired = ElementalRespecResourcePolicy.Amount(current, remembered);
                        if (current > desired) target.Resources.Spend(resource, current - desired);
                        remembered = desired;
                    }
                    ledger.Remember(resource.AssetGuid, remembered);
                }
            }
        }

        internal static Scope Begin(UnitEntityData original, ref Action continuation)
        {
            var scope = new Scope { Previous = _pending };
            var races = BlueprintBootstrap.ElementalRaces;
            if (original == null || races == null) { _pending = scope; return scope; }
            var race = races.OrderedBlueprints().SingleOrDefault(value => ReferenceEquals(value.Race, original.Descriptor.Progression.Race));
            if (race == null) { _pending = scope; return scope; }
            var catalog = races.OrderedBlueprints().SelectMany(value => value.Heritages.Choices().Select(choice => choice.SlaResource)
                .Concat(value.AlternateTraits.Traits().SelectMany(trait => trait.Mechanics()).OfType<BlueprintAbilityResource>()))
                .Concat(ElementalRaceIdentityCatalog.FeatSymbols().Select(symbol =>
                    BlueprintBootstrap.ElementalFeats.RequireSymbol<BlueprintScriptableObject>(symbol)).OfType<BlueprintAbilityResource>())
                .Distinct().ToArray();
            var ledger = original.Descriptor.Get<UnitPartElementalHeritageState>();
            var remembered = ledger == null ? new Dictionary<string, int>(StringComparer.Ordinal) : ledger.CopyResourceAmounts();
            var present = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var resource in catalog)
            {
                int count = original.Descriptor.Resources.PersistantResources.Count(value => value != null && ReferenceEquals(value.Blueprint, resource));
                if (count > 1) throw new InvalidOperationException("Cannot capture a duplicated elemental respec resource.");
                if (count == 1) present.Add(resource.AssetGuid, original.Descriptor.Resources.GetResourceAmount(resource));
            }
            var snapshot = new Snapshot { Original = original, Catalog = catalog,
                Amounts = ElementalRespecResourcePolicy.Capture(catalog.Select(value => value.AssetGuid), remembered, present) };
            var blood = original.Descriptor.Get<UnitPartElementalBloodCapacity>();
            if (blood != null)
            {
                snapshot.BloodSpent = new[] { blood.Spent(ElementalAlternateTraitId.FireInTheBlood),
                    blood.Spent(ElementalAlternateTraitId.StoneInTheBlood), blood.Spent(ElementalAlternateTraitId.StormInTheBlood) };
                if (snapshot.BloodSpent.Any(value => value < 0))
                    throw new InvalidOperationException("Cannot capture unknown elemental blood expenditure during respec.");
            }
            scope.Captured = snapshot;
            Action next = continuation;
            continuation = () => {
                try { snapshot.Preserve(original.Descriptor, true); }
                finally { Forget(snapshot); }
                if (next != null) next();
            };
            _pending = scope;
            return scope;
        }

        internal static void End(Scope scope, bool failed)
        {
            if (scope == null) return;
            if (ReferenceEquals(_pending, scope)) _pending = scope.Previous;
            if (failed) Forget(scope.Captured);
        }

        internal static void Seed(UnitDescriptor replacement, LevelUpState.CharBuildMode mode)
        {
            Snapshot snapshot = _pending == null ? null : _pending.Captured;
            if (snapshot == null || mode != LevelUpState.CharBuildMode.Respec || replacement == null ||
                ReferenceEquals(replacement, snapshot.Original.Descriptor) ||
                replacement.Unit.UniqueId != snapshot.Original.UniqueId) return;
            if (snapshot.Replacement != null && !ReferenceEquals(snapshot.Replacement, replacement))
                throw new InvalidOperationException("A native elemental respec produced multiple replacement owners.");
            snapshot.Replacement = replacement;
            Replacements.Remove(replacement); Replacements.Add(replacement, snapshot);
            snapshot.Preserve(replacement);
        }

        internal static void Preserve(LevelUpController controller, UnitDescriptor target)
        {
            Snapshot snapshot;
            if (controller != null && controller.Unit != null &&
                Replacements.TryGetValue(controller.Unit, out snapshot) && snapshot.Active)
                snapshot.Preserve(target);
        }

        internal static void Cancel(LevelUpController controller)
        {
            Snapshot snapshot;
            if (controller != null && controller.Unit != null && Replacements.TryGetValue(controller.Unit, out snapshot))
                Forget(snapshot);
        }

        private static void Forget(Snapshot snapshot)
        {
            if (snapshot == null) return;
            snapshot.Active = false;
            if (snapshot.Replacement != null) Replacements.Remove(snapshot.Replacement);
        }
    }

    [HarmonyPatch(typeof(Player), "RespecCompanion", new[] { typeof(UnitEntityData), typeof(Action) })]
    internal static class ElementalNativeRespecEntryPatch
    {
        private static void Prefix(UnitEntityData unit, ref Action successCallback,
            ref ElementalNativeRespecResourceRuntime.Scope __state)
        {
            __state = ElementalNativeRespecResourceRuntime.Begin(unit, ref successCallback);
        }
        private static Exception Finalizer(ElementalNativeRespecResourceRuntime.Scope __state, Exception __exception)
        {
            ElementalNativeRespecResourceRuntime.End(__state, __exception != null);
            return __exception;
        }
    }

    [HarmonyPatch(typeof(LevelUpController), "Start")]
    internal static class ElementalNativeRespecStartPatch
    {
        private static void Prefix(UnitDescriptor unit, LevelUpState.CharBuildMode mode)
        { ElementalNativeRespecResourceRuntime.Seed(unit, mode); }
    }

    [HarmonyPatch(typeof(LevelUpController), "RequestPreview")]
    internal static class ElementalNativeRespecPreviewPatch
    {
        private static void Postfix(LevelUpController __instance, UnitDescriptor __result)
        { ElementalNativeRespecResourceRuntime.Preserve(__instance, __result); }
    }

    [HarmonyPatch(typeof(LevelUpController), "ApplyLevelup")]
    internal static class ElementalNativeRespecApplyPatch
    {
        private static void Postfix(LevelUpController __instance, UnitDescriptor unitDescriptor)
        { ElementalNativeRespecResourceRuntime.Preserve(__instance, unitDescriptor); }
    }

    [HarmonyPatch(typeof(LevelUpController), "SetupNewCharacher")]
    internal static class ElementalNativeRespecSetupPatch
    {
        private static void Postfix(LevelUpController __instance)
        { ElementalNativeRespecResourceRuntime.Preserve(__instance, __instance.Unit); }
    }

    [HarmonyPatch(typeof(LevelUpController), "Cancel")]
    internal static class ElementalNativeRespecCancelPatch
    {
        private static void Postfix(LevelUpController __instance)
        { ElementalNativeRespecResourceRuntime.Cancel(__instance); }
    }
}
