using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs.Components;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using UnityModManagerNet;

namespace KingmakerGunslinger.Summoning
{
    // Optional adapter at the proven first-UMM-update-after-LoadDictionary seam.
    // Only private Sprint32 blueprints change. No provider assembly is linked
    // into this DLL, and no provider blueprint is stored in a summon or save.
    internal static class Sprint32Suggestion
    {
        internal const string ModId = "CowWithHatsCustomSpellsMod";
        internal const string AbilityGuid = "3682197d956543e2a694e163b5fdcc6c";
        internal const string BuffGuid = "729fa06959ad4a0ca1e761b955b6ff62";
        internal const string ExcludedWizardDiscoveryGuid = "1a4bd53d5b8848d9a717084d7f9dfa0c";
        internal const string OwnedBuffSymbol = "KMG.Summoning.FeyCelestial.SuggestionBuff";
        private static UnityModManager.ModEntry _provider;
        private static ModContext _context;
        private static bool _ready;
        internal static bool Active { get { return _ready && _context != null &&
            _context.FeatureModules.Active.ExpandedSummoning && _provider != null && _provider.Loaded &&
            _provider.Active && _provider.HasAssembly && !_provider.ErrorOnLoading && _provider.Assembly != null; } }
        internal static bool Settled { get; private set; }
        internal static string Disposition { get; private set; } = "pending";
        internal static string OwnedBuffGuid { get; private set; }
        internal static void Install(ModContext context)
        {
            if (_context != null || context == null) return;
            _context = context; context.ModEntry.OnUpdate += OnUpdate;
        }
        private static void OnUpdate(UnityModManager.ModEntry entry, float delta)
        {
            if (!BlueprintBootstrap.IsInitialized && !BlueprintBootstrap.IsFailed) return;
            _context.ModEntry.OnUpdate -= OnUpdate;
            try
            {
                if (BlueprintBootstrap.IsFailed || ExpandedSummoningBlueprints.Registered == null)
                { Disposition = "bootstrap-unavailable"; return; }
                var bySymbol = ExpandedSummoningBlueprints.Registered.BySymbol;
                OwnedBuffGuid = bySymbol[OwnedBuffSymbol].AssetGuid;
                if (!_context.FeatureModules.Active.ExpandedSummoning)
                { Disposition = "module-disabled"; return; }
                var field = entry.GetType().DeclaringType.GetField("modEntries",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                var entries = field?.GetValue(null) as IEnumerable;
                if (entries == null) throw new InvalidOperationException("Native UMM mod entries unavailable.");
                var matches = entries.Cast<object>().OfType<UnityModManager.ModEntry>()
                    .Where(value => value.Info?.Id == ModId).ToArray();
                if (matches.Length != 1)
                { Disposition = matches.Length == 0 ? "provider-absent" : "provider-ambiguous"; return; }
                var provider = matches[0];
                if (!provider.Loaded || !provider.Active || !provider.HasAssembly || provider.ErrorOnLoading ||
                    provider.Assembly == null || provider.Assembly.GetName().Name != ModId)
                { Disposition = "provider-inactive-or-mismatched"; return; }
                ExpandedSummoningSpecialBuilder.BindSuggestionProvider32(BlueprintBootstrap.Library, bySymbol);
                _provider = provider; _ready = true;
                Disposition = "active;version=" + provider.Info.Version + ";mvid=" +
                    provider.Assembly.ManifestModule.ModuleVersionId.ToString("N");
            }
            catch (Exception exception)
            {
                _ready = false; Disposition = "provider-contract-failed";
                _context.Logger.Failure("summoning", "sprint32.suggestion-unavailable",
                    "Optional Suggestion remains unavailable; private identities remain loadable.", exception);
            }
            finally
            {
                Settled = true;
                if (Game.Instance?.State?.Units != null)
                    foreach (var unit in Game.Instance.State.Units.All.ToArray())
                    {
                        if (unit == null || unit.Destroyed) continue;
                        unit.Blueprint.GetComponent<LillendBardBook32>()?.ReconcileOptionalSpell(unit);
                        if (!Active)
                            foreach (var buff in unit.Descriptor.Buffs.Enumerable.Where(value =>
                                value.Blueprint.AssetGuid == OwnedBuffGuid).ToArray()) buff.Remove();
                    }
                _context.Logger.Info("summoning", "sprint32.suggestion", Disposition);
            }
        }
    }

    [Serializable]
    public sealed class Sprint32SuggestionAvailable : BlueprintComponent, IAbilityCasterChecker
    {
        public BlueprintUnit OwningBlueprint;
        public bool CorrectCaster(UnitEntityData caster)
        {
            if (!Sprint32Suggestion.Active || caster == null || !ReferenceEquals(caster.Blueprint, OwningBlueprint)) return false;
            var dictionary = BlueprintBootstrap.Library?.BlueprintsByAssetId;
            if (dictionary == null) return false;
            BlueprintScriptableObject discovery;
            if (!dictionary.TryGetValue(Sprint32Suggestion.ExcludedWizardDiscoveryGuid, out discovery)) return true;
            var fact = discovery as Kingmaker.Blueprints.Facts.BlueprintUnitFact;
            // Match ContextConditionCasterHasFact's native membership check,
            // including Logic-owned facts, without serializing a provider link.
            return fact != null && !Kingmaker.UnitLogic.UnitHelper.HasFact(caster.Descriptor, fact);
        }
        public string GetReason() { return "Requires active CowWithHat's Custom Spells Suggestion support."; }
    }

    // The optional effect is our native-component clone. Removing its provider
    // (or disabling Expanded Summoning) cannot leave a saved active suggestion.
    [Serializable]
    public sealed class Sprint32SuggestionPresence : BuffLogic
    {
        public override void OnTurnOn() { if (Sprint32Suggestion.Settled && !Sprint32Suggestion.Active) Buff.Remove(); }
        public override void PostLoad() { base.PostLoad(); if (Sprint32Suggestion.Settled && !Sprint32Suggestion.Active) Buff.Remove(); }
    }
}
