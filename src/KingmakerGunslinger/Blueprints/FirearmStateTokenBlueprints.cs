using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Ecnchantments;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    /// <summary>
    /// Registers six component-only, mechanically inert weapon enchantments that encode
    /// every non-default state supported by the Sprint 12 capacity-one persistence spike.
    /// </summary>
    internal static class FirearmStateTokenBlueprints
    {
        internal const string LoadedNormalSymbol = "KMG.Test.LoadedStateToken";
        internal const string BrokenEmptySymbol = "KMG.Test.BrokenEmptyStateToken";
        internal const string BrokenLoadedSymbol = "KMG.Test.BrokenLoadedStateToken";
        internal const string WreckedSymbol = "KMG.Test.WreckedStateToken";
        internal const string PaperLoadedNormalSymbol =
            "KMG.Ammunition.PaperLoadedNormalStateToken";
        internal const string PaperBrokenLoadedSymbol =
            "KMG.Ammunition.PaperBrokenLoadedStateToken";

        private const string ComponentName = "$KMG_FirearmStateToken";

        internal static FirearmStateTokenBlueprintSet Register(
            BlueprintRegistry registry,
            ModLogger logger)
        {
            if (registry == null)
            {
                throw new ArgumentNullException("registry");
            }

            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            FirearmStateTokenCatalog catalog =
                FirearmStateTokenCatalog.CreateCapacityOneDiagnostic();
            var byToken = new Dictionary<string, BlueprintWeaponEnchantment>(StringComparer.Ordinal);

            RegisterOne(
                registry,
                byToken,
                LoadedNormalSymbol,
                "KMG_StateToken_LoadedNormal_LeadBall",
                catalog.RequireDefinition(FirearmStateTokenCatalog.LoadedNormalTokenId));
            RegisterOne(
                registry,
                byToken,
                BrokenEmptySymbol,
                "KMG_StateToken_BrokenEmpty",
                catalog.RequireDefinition(FirearmStateTokenCatalog.BrokenEmptyTokenId));
            RegisterOne(
                registry,
                byToken,
                BrokenLoadedSymbol,
                "KMG_StateToken_BrokenLoaded_LeadBall",
                catalog.RequireDefinition(FirearmStateTokenCatalog.BrokenLoadedTokenId));
            RegisterOne(
                registry,
                byToken,
                WreckedSymbol,
                "KMG_StateToken_Wrecked",
                catalog.RequireDefinition(FirearmStateTokenCatalog.WreckedTokenId));
            RegisterOne(
                registry,
                byToken,
                PaperLoadedNormalSymbol,
                "KMG_StateToken_LoadedNormal_PaperCartridge",
                catalog.RequireDefinition(FirearmStateTokenCatalog.PaperLoadedNormalTokenId));
            RegisterOne(
                registry,
                byToken,
                PaperBrokenLoadedSymbol,
                "KMG_StateToken_BrokenLoaded_PaperCartridge",
                catalog.RequireDefinition(FirearmStateTokenCatalog.PaperBrokenLoadedTokenId));

            var set = new FirearmStateTokenBlueprintSet(catalog, byToken);
            Validate(set);
            logger.Info(
                "firearms",
                "persistence.tokens-ready",
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Registered {0} no-op item-owned firearm-state token blueprints; absence encodes empty/normal.",
                    set.Count));
            return set;
        }

        internal static void Validate(FirearmStateTokenBlueprintSet set)
        {
            if (set == null)
            {
                throw new ArgumentNullException("set");
            }

            if (set.Count != 6 || set.Catalog.Definitions.Count != 6)
            {
                throw new InvalidOperationException(
                    "The firearm token blueprint set must contain exactly six non-default states.");
            }

            foreach (FirearmStateTokenDefinition definition in set.Catalog.Definitions)
            {
                BlueprintWeaponEnchantment blueprint = set.RequireBlueprint(definition.TokenId);
                FirearmStateTokenComponent[] markers =
                    (blueprint.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                    .OfType<FirearmStateTokenComponent>()
                    .ToArray();
                if (markers.Length != 1 || blueprint.ComponentsArray.Length != 1)
                {
                    throw new InvalidOperationException(
                        "A firearm-state token enchantment must contain exactly one marker and no gameplay components.");
                }

                if (!string.Equals(markers[0].name, ComponentName, StringComparison.Ordinal) ||
                    !definition.Equals(markers[0].Definition))
                {
                    throw new InvalidOperationException(
                        "A firearm-state token enchantment does not match its catalog definition.");
                }
            }
        }

        private static void RegisterOne(
            BlueprintRegistry registry,
            IDictionary<string, BlueprintWeaponEnchantment> byToken,
            string symbol,
            string internalName,
            FirearmStateTokenDefinition definition)
        {
            BlueprintWeaponEnchantment blueprint =
                registry.Register<BlueprintWeaponEnchantment>(
                    symbol,
                    delegate
                    {
                        BlueprintWeaponEnchantment created =
                            ScriptableObject.CreateInstance<BlueprintWeaponEnchantment>();
                        created.name = internalName;
                        FirearmStateTokenComponent marker =
                            FirearmStateTokenComponent.Create(definition);
                        marker.name = ComponentName;
                        created.ComponentsArray = new BlueprintComponent[] { marker };
                        return created;
                    });
            byToken.Add(definition.TokenId, blueprint);
        }
    }

    /// <summary>
    /// Immutable runtime lookup from strict state-token IDs to the registered no-op
    /// weapon-enchantment blueprints that Kingmaker is expected to serialize on items.
    /// </summary>
    internal sealed class FirearmStateTokenBlueprintSet
    {
        private readonly Dictionary<string, BlueprintWeaponEnchantment> _byToken;

        internal FirearmStateTokenBlueprintSet(
            FirearmStateTokenCatalog catalog,
            IDictionary<string, BlueprintWeaponEnchantment> byToken)
        {
            Catalog = catalog ?? throw new ArgumentNullException("catalog");
            if (byToken == null)
            {
                throw new ArgumentNullException("byToken");
            }

            _byToken = new Dictionary<string, BlueprintWeaponEnchantment>(
                byToken,
                StringComparer.Ordinal);
            if (_byToken.Any(pair => pair.Value == null))
            {
                throw new ArgumentException(
                    "A token blueprint lookup cannot contain null blueprints.",
                    "byToken");
            }
        }

        internal FirearmStateTokenCatalog Catalog { get; private set; }

        internal int Count
        {
            get { return _byToken.Count; }
        }

        internal BlueprintWeaponEnchantment RequireBlueprint(string tokenId)
        {
            BlueprintWeaponEnchantment blueprint;
            if (string.IsNullOrWhiteSpace(tokenId) || !_byToken.TryGetValue(tokenId, out blueprint))
            {
                throw new KeyNotFoundException(
                    "No registered weapon-enchantment blueprint exists for firearm-state token '" +
                    (tokenId ?? "<null>") + "'.");
            }

            return blueprint;
        }

        internal bool TryGetBlueprint(
            string tokenId,
            out BlueprintWeaponEnchantment blueprint)
        {
            blueprint = null;
            return tokenId != null && _byToken.TryGetValue(tokenId, out blueprint);
        }
    }
}
