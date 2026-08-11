using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class ExpandedSummoningBlueprintSet
    {
        internal ExpandedSummoningBlueprintSet(
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        { BySymbol = bySymbol ?? throw new ArgumentNullException("bySymbol"); }
        internal IDictionary<string, BlueprintScriptableObject> BySymbol
        { get; private set; }
        internal int Count { get { return BySymbol.Count; } }
    }

    internal static class ExpandedSummoningBlueprints
    {
        private const string RegistrationUnitDonorGuid =
            "1ed9a630f0d9d7f44855d3d1d1b2cdf2";
        private const string RegistrationParentGuid =
            "8fd74eddd9b6c224693d9ab241f25e84";

        internal static ExpandedSummoningBlueprintSet Register(
            LibraryScriptableObject library, BlueprintRegistry registry)
        {
            if (library == null) throw new ArgumentNullException("library");
            if (registry == null) throw new ArgumentNullException("registry");
            ExpandedSummoningCatalog.Validate();
            IReadOnlyList<SummoningIdentitySpec> identities =
                ExpandedSummoningIdentityCatalog.Build();
            BlueprintUnit unitDonor = BlueprintLibraryLookup.RequireExact<BlueprintUnit>(
                library, RegistrationUnitDonorGuid,
                "dedicated native summon registration donor");
            BlueprintAbility parent = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(
                library, RegistrationParentGuid,
                "native Summon Monster I registration parent");
            AbilityVariants variants = (parent.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).OfType<AbilityVariants>().Single();
            BlueprintAbility abilityDonor = (variants.Variants ??
                Array.Empty<BlueprintAbility>()).First(value => value != null);
            var registered = new Dictionary<string, BlueprintScriptableObject>(
                StringComparer.Ordinal);
            foreach (SummoningIdentitySpec identity in identities)
            {
                BlueprintScriptableObject blueprint;
                if (identity.PlannedType == "BlueprintUnit")
                    blueprint = registry.Register<BlueprintUnit>(identity.Symbol,
                        () => CloneUnitShell(unitDonor, identity.Symbol));
                else if (identity.PlannedType == "BlueprintAbility")
                    blueprint = registry.Register<BlueprintAbility>(identity.Symbol,
                        () => CloneAbilityShell(abilityDonor, identity.Symbol));
                else if (identity.PlannedType == "BlueprintBuff")
                    blueprint = registry.Register<BlueprintBuff>(identity.Symbol,
                        () => CreateBuffShell(identity.Symbol));
                else throw new InvalidOperationException(
                    "Unsupported Expanded Summoning planned type " +
                    identity.PlannedType + ".");
                registered.Add(identity.Symbol, blueprint);
            }
            var result = new ExpandedSummoningBlueprintSet(registered);
            if (result.Count != ExpandedSummoningIdentityCatalog.FoundationIdentityCount)
                throw new InvalidOperationException(
                    "Expanded Summoning registration count mismatch.");
            return result;
        }

        private static BlueprintUnit CloneUnitShell(BlueprintUnit donor, string symbol)
        { return BlueprintCloneService.Clone(donor, InternalName(symbol)); }

        private static BlueprintAbility CloneAbilityShell(BlueprintAbility donor,
            string symbol)
        {
            BlueprintAbility result = BlueprintCloneService.Clone(donor,
                InternalName(symbol));
            result.Hidden = true;
            result.ActionBarAutoFillIgnored = true;
            return result;
        }

        private static BlueprintBuff CreateBuffShell(string symbol)
        {
            BlueprintBuff result = ScriptableObject.CreateInstance<BlueprintBuff>();
            result.name = InternalName(symbol);
            result.ComponentsArray = Array.Empty<BlueprintComponent>();
            return result;
        }

        private static string InternalName(string symbol)
        { return symbol.Replace('.', '_').Replace('-', '_'); }
    }
}
