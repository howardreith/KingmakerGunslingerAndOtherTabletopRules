using System;
using Harmony12;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Assets.UI;
using Kingmaker.UI;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.ActivatableAbilities;
using KingmakerGunslinger.FavoredClass.Mechanics;
using UnityEngine;

namespace KingmakerGunslinger.FavoredClass.Hooks
{
    /// <summary>
    /// O01 displayed range: the owner's own performance feature and toggle
    /// facts describe the range the owner's area actually has: while the
    /// owner has live areas of that performance they decide (all widened, or
    /// all native: FavoredClassPerformanceInstances, FavoredClassRangeGroup);
    /// with none, the owner's configured range is shown. No outcome is
    /// remembered after its area ends. The shared blueprints and every other
    /// bard's text are unchanged.
    /// </summary>
    internal static class FavoredClassPerformanceOwnerText
    {
        /// <summary>The owner's description of a fact, or null when it keeps the native text.</summary>
        internal static string For(UnitDescriptor owner, string blueprintGuid, string native)
        {
            string key = FavoredClassPerformanceManifest.KeyForFact(blueprintGuid);
            if (key == null || owner == null || native == null ||
                FavoredClassRuntime.IsEffectUnavailable(FavoredClassCatalog.EffectPerformanceRange) ||
                FavoredClassRuntime.IsTargetUnavailable(FavoredClassCatalog.EffectPerformanceRange, key))
                return null;
            int steps = FavoredClassEarnedSteps.For(owner, FavoredClassCatalog.EffectPerformanceRange, key);
            FavoredClassPerformanceTarget target = FavoredClassPerformanceManifest.For(key);
            // The owner's live areas decide, even after the steps changed;
            // with none, the configured range (none without steps).
            int? feet = FavoredClassPerformanceInstances.Feet(owner, key,
                steps > 0 ? FavoredClassPerformanceManifest.OwnerFeet(target, steps) : (int?)null);
            return feet == null ? null :
                FavoredClassPerformanceText.OwnerDescription(native, target.BaseFeet, feet.Value);
        }
    }

    /// <summary>The owner's description in front of the native data provider.</summary>
    internal sealed class FavoredClassOwnerDescription : IUIDataProvider
    {
        private readonly IUIDataProvider _native;
        private readonly string _description;

        internal FavoredClassOwnerDescription(IUIDataProvider native, string description)
        {
            _native = native;
            _description = description;
        }

        public string Name
        {
            get { return _native.Name; }
        }

        public string Description
        {
            get { return _description; }
        }

        public Sprite Icon
        {
            get { return _native.Icon; }
        }
    }

    /// <summary>Character sheet and tooltips: <c>Fact.Description</c> of a feature or toggle.</summary>
    [HarmonyPatch(typeof(Fact), "SelectUIData")]
    internal static class FavoredClassPerformanceFactTextPatch
    {
        private static void Postfix(Fact __instance, UIDataType type, ref IUIDataProvider __result)
        {
            if (type != UIDataType.Description || __result == null || __instance == null ||
                __instance.Blueprint == null)
                return;
            try
            {
                var owned = __instance as OwnedFact<UnitDescriptor>;
                string text = owned == null ? null :
                    FavoredClassPerformanceOwnerText.For(owned.Owner, __instance.Blueprint.AssetGuid,
                        __result.Description);
                if (text != null)
                    __result = new FavoredClassOwnerDescription(__result, text);
            }
            catch (Exception)
            {
                // Keep the native text.
            }
        }
    }

    /// <summary>Action bar: the toggle slot reads its blueprint's description directly.</summary>
    [HarmonyPatch(typeof(MechanicActionBarSlotActivableAbility), "GetDescription")]
    internal static class FavoredClassPerformanceActionBarTextPatch
    {
        private static void Postfix(MechanicActionBarSlotActivableAbility __instance, ref string __result)
        {
            try
            {
                ActivatableAbility ability = __instance == null ? null : __instance.ActivatableAbility;
                string text = ability == null || ability.Blueprint == null ? null :
                    FavoredClassPerformanceOwnerText.For(ability.Owner, ability.Blueprint.AssetGuid, __result);
                if (text != null)
                    __result = text;
            }
            catch (Exception)
            {
                // Keep the native text.
            }
        }
    }
}
