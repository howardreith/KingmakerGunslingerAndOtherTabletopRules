using System;
using System.Reflection;
using KingmakerGunslinger.Ammunition;

namespace KingmakerGunslinger.CraftMagicItemsCompatibility
{
    /// <summary>Temporarily applies the KMG-only ammunition price policy.</summary>
    internal sealed class CraftMagicItemsAmmunitionCostScope : IDisposable
    {
        private const BindingFlags Static = BindingFlags.Static |
            BindingFlags.Public | BindingFlags.NonPublic;
        private readonly object _settings;
        private readonly FieldInfo _costsNoGoldField;
        private readonly FieldInfo _priceScaleField;
        private readonly object _costsNoGoldSnapshot;
        private readonly object _priceScaleSnapshot;
        private bool _restored;

        private CraftMagicItemsAmmunitionCostScope(object settings,
            FieldInfo costsNoGoldField, FieldInfo priceScaleField,
            object costsNoGoldSnapshot, object priceScaleSnapshot)
        {
            _settings = settings;
            _costsNoGoldField = costsNoGoldField;
            _priceScaleField = priceScaleField;
            _costsNoGoldSnapshot = costsNoGoldSnapshot;
            _priceScaleSnapshot = priceScaleSnapshot;
        }

        internal static CraftMagicItemsAmmunitionCostScope Begin(
            CraftMagicItemsContract contract)
        {
            if (contract == null) throw new ArgumentNullException("contract");
            FieldInfo settingsField = RequireStaticField(contract.MainType,
                "ModSettings");
            object settings = settingsField.GetValue(null);
            if (settings == null) throw new InvalidOperationException(
                "CMI settings are unavailable for KMG ammunition crafting.");
            FieldInfo costsNoGold = RequireField(settings.GetType(),
                "CraftingCostsNoGold");
            FieldInfo priceScale = RequireField(settings.GetType(),
                "CraftingPriceScale");
            if (costsNoGold.FieldType != typeof(bool) ||
                priceScale.FieldType != typeof(float))
                throw new InvalidOperationException(
                    "CMI crafting settings shape changed.");
            var result = new CraftMagicItemsAmmunitionCostScope(settings,
                costsNoGold, priceScale, costsNoGold.GetValue(settings),
                priceScale.GetValue(settings));
            try
            {
                costsNoGold.SetValue(settings, false);
                priceScale.SetValue(settings,
                    AmmunitionCraftingCostPolicy.CraftMagicItemsPriceScale);
                return result;
            }
            catch
            {
                result.Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            if (_restored) return;
            Exception restoreFailure = null;
            try { _costsNoGoldField.SetValue(_settings, _costsNoGoldSnapshot); }
            catch (Exception exception) { restoreFailure = exception; }
            try { _priceScaleField.SetValue(_settings, _priceScaleSnapshot); }
            catch (Exception exception)
            { if (restoreFailure == null) restoreFailure = exception; }
            _restored = true;
            if (restoreFailure != null) throw new InvalidOperationException(
                "CMI ammunition price settings could not be restored.",
                restoreFailure);
        }

        private static FieldInfo RequireStaticField(Type type, string name)
        { return RequireField(type, name, Static); }

        private static FieldInfo RequireField(Type type, string name)
        { return RequireField(type, name, BindingFlags.Instance |
            BindingFlags.Public | BindingFlags.NonPublic); }

        private static FieldInfo RequireField(Type type, string name,
            BindingFlags flags)
        {
            FieldInfo field = type == null ? null : type.GetField(name, flags);
            if (field == null) throw new MissingFieldException(type == null ?
                "<null>" : type.FullName, name);
            return field;
        }
    }
}
