using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using UnityModManagerNet;

namespace KingmakerGunslinger.Summoning
{
    // Repair R5 optional adapter at the proven first-UMM-update-after-
    // LoadDictionary seam (the Sprint 32 Suggestion pattern). Call of the
    // Wild creates its spells inside its own LoadDictionary work, so they are
    // resolved only here. Only private KMG blueprints change; no provider
    // assembly is linked and no provider blueprint is mutated. Each spell
    // binds independently; anything missing or changed leaves that owned
    // ability hidden and unusable (clean omission, no substitute).
    internal static class R5CotwSpells
    {
        private static UnityModManager.ModEntry _provider;
        private static ModContext _context;
        internal static bool MistBound { get; private set; }
        internal static bool DanceBound { get; private set; }
        internal static bool Settled { get; private set; }
        internal static string Disposition { get; private set; } = "pending";
        internal static string MistDisposition { get; private set; } = "pending";
        internal static string DanceDisposition { get; private set; } = "pending";

        private static bool ProviderLive { get { return _context != null && _context.FeatureModules.Active.ExpandedSummoning &&
            _provider != null && _provider.Loaded && _provider.Active && _provider.HasAssembly && !_provider.ErrorOnLoading && _provider.Assembly != null; } }
        internal static bool MistActive { get { return MistBound && ProviderLive; } }
        internal static bool DanceActive { get { return DanceBound && ProviderLive; } }

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
                if (!_context.FeatureModules.Active.ExpandedSummoning) { Disposition = "module-disabled"; return; }
                var field = entry.GetType().DeclaringType.GetField("modEntries",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                var entries = field?.GetValue(null) as IEnumerable;
                if (entries == null) throw new InvalidOperationException("Native UMM mod entries unavailable.");
                var matches = entries.Cast<object>().OfType<UnityModManager.ModEntry>()
                    .Where(value => value.Info?.Id == SummonPlaytestR5Policy.CotwModId).ToArray();
                var provider = matches.Length == 1 ? matches[0] : null;
                Disposition = SummonPlaytestR5Policy.ProviderDisposition(matches.Length,
                    provider != null && provider.Loaded, provider != null && provider.Active,
                    provider != null && provider.HasAssembly, provider != null && provider.ErrorOnLoading,
                    provider?.Assembly?.GetName().Name);
                if (Disposition != "provider-active") { MistDisposition = DanceDisposition = Disposition; return; }
                _provider = provider;
                var bySymbol = ExpandedSummoningBlueprints.Registered.BySymbol;
                // Each binding is all or nothing and independent: a failure
                // leaves that ability's hidden, guarded placeholder (its guard
                // refuses every caster unless the binding reported active).
                Exception failure;
                try
                {
                    MistDisposition = ExpandedSummoningSpecialBuilder.BindCotwObscuringMistR5(
                        BlueprintBootstrap.Library, bySymbol, out failure);
                }
                catch (Exception exception) { MistDisposition = SummonPlaytestR5Policy.BindingOmitted; failure = exception; }
                MistBound = MistDisposition == SummonPlaytestR5Policy.BindingActive;
                if (!MistBound)
                    _context.Logger.Failure("summoning", "r5.cotw-mist-unavailable",
                        "Cloud Giant Obscuring Mist remains omitted (" + MistDisposition + "); private identities stay loadable.", failure);
                try
                {
                    DanceDisposition = ExpandedSummoningSpecialBuilder.BindCotwIrresistibleDanceR5(
                        BlueprintBootstrap.Library, bySymbol, out failure);
                }
                catch (Exception exception) { DanceDisposition = SummonPlaytestR5Policy.BindingOmitted; failure = exception; }
                DanceBound = DanceDisposition == SummonPlaytestR5Policy.BindingActive;
                if (!DanceBound)
                    _context.Logger.Failure("summoning", "r5.cotw-dance-unavailable",
                        "Pixie Irresistible Dance remains omitted (" + DanceDisposition + "); private identities stay loadable.", failure);
                Disposition = "provider-active;version=" + provider.Info.Version + ";mvid=" +
                    provider.Assembly.ManifestModule.ModuleVersionId.ToString("N");
            }
            catch (Exception exception)
            {
                Disposition = "provider-resolution-failed";
                _context.Logger.Failure("summoning", "r5.cotw-unavailable",
                    "Call of the Wild spells remain omitted; private identities stay loadable.", exception);
            }
            finally
            {
                Settled = true;
                _context.Logger.Info("summoning", "r5.cotw-spells",
                    Disposition + ";mist=" + MistDisposition + ";dance=" + DanceDisposition);
            }
        }
    }

    /// <summary>The owned provider-backed spell-like ability is usable only while its exact provider binding is live.</summary>
    [Serializable]
    public sealed class R5CotwSpellAvailable : BlueprintComponent, IAbilityCasterChecker
    {
        public BlueprintUnit OwningBlueprint;
        public bool Dance;

        public bool CorrectCaster(UnitEntityData caster)
        {
            if (caster == null || !ReferenceEquals(caster.Blueprint, OwningBlueprint)) return false;
            return Dance ? R5CotwSpells.DanceActive : R5CotwSpells.MistActive;
        }

        public string GetReason() { return "Requires the active Call of the Wild implementation of this spell."; }
    }
}
