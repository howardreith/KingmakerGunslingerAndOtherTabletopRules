using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;
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
        private const string SummonedFactionDonorGuid =
            "1ed9a630f0d9d7f44855d3d1d1b2cdf2";

        internal static ExpandedSummoningBlueprintSet Register(
            LibraryScriptableObject library, BlueprintRegistry registry)
        {
            if (library == null) throw new ArgumentNullException("library");
            if (registry == null) throw new ArgumentNullException("registry");
            ExpandedSummoningCatalog.Validate();
            IReadOnlyList<SummoningIdentitySpec> identities =
                ExpandedSummoningIdentityCatalog.Build();
            ExpandedSummoningDonorCatalog.Validate();
            BlueprintUnit summonedFactionDonor =
                BlueprintLibraryLookup.RequireExact<BlueprintUnit>(library,
                    SummonedFactionDonorGuid, "dedicated native summon faction donor");
            var unitDonors = ExpandedSummoningCatalog.All.ToDictionary(
                ExpandedSummoningIdentityCatalog.UnitSymbol,
                creature => BlueprintLibraryLookup.RequireExact<BlueprintUnit>(library,
                    ExpandedSummoningDonorCatalog.For(creature.Key).Guid,
                    creature.DisplayName + " donor"), StringComparer.Ordinal);
            var registered = new Dictionary<string, BlueprintScriptableObject>(
                StringComparer.Ordinal);
            foreach (SummoningIdentitySpec identity in identities)
            {
                BlueprintScriptableObject blueprint;
                if (identity.PlannedType == "BlueprintUnit")
                    blueprint = registry.Register<BlueprintUnit>(identity.Symbol,
                        () => CloneUnitShell(unitDonors[identity.Symbol],
                            summonedFactionDonor, identity.Symbol));
                else if (identity.PlannedType == "BlueprintAbility")
                    blueprint = registry.Register<BlueprintAbility>(identity.Symbol,
                        () => CreateAbilityShell(identity.Symbol));
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

        private static BlueprintUnit CloneUnitShell(BlueprintUnit donor,
            BlueprintUnit summonedFactionDonor, string symbol)
        {
            BlueprintUnit result = BlueprintCloneService.Clone(donor,
                InternalName(symbol));
            result.ComponentsArray = (donor.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Where(component => component != null &&
                    !IsForbiddenComponent(component.GetType().Name)).ToArray();
            SetSummonedFaction(result, summonedFactionDonor);
            return result;
        }

        private static bool IsForbiddenComponent(string name)
        {
            string value = (name ?? string.Empty).ToLowerInvariant();
            return value.Contains("experience") || value.Contains("loot") ||
                value.Contains("inventory") || value.Contains("dialog") ||
                value.Contains("interaction") || value.Contains("quest") ||
                value.Contains("cutscene") || value.Contains("companion") ||
                value.Contains("pet") || value.Contains("area") ||
                value.Contains("story") || value.Contains("corpse") ||
                value.Contains("addtags") || value.Contains("mobcaster");
        }

        private static void SetSummonedFaction(BlueprintUnit target,
            BlueprintUnit donor)
        {
            const System.Reflection.BindingFlags flags =
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic;
            System.Reflection.FieldInfo field = typeof(BlueprintUnit).GetField(
                "m_Faction", flags);
            if (field == null) throw new MissingFieldException(
                typeof(BlueprintUnit).FullName, "m_Faction");
            field.SetValue(target, field.GetValue(donor));
        }

        private static BlueprintAbility CreateAbilityShell(string symbol)
        {
            BlueprintAbility result = ScriptableObject.CreateInstance<BlueprintAbility>();
            result.name = InternalName(symbol);
            result.Hidden = true;
            result.ActionBarAutoFillIgnored = true;
            result.ComponentsArray = Array.Empty<BlueprintComponent>();
            result.MaterialComponent = new BlueprintAbility.MaterialComponentData();
            result.ResourceAssetIds = Array.Empty<string>();
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
