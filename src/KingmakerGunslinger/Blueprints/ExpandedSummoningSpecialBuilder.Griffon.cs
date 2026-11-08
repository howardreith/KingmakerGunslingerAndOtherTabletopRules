using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static partial class ExpandedSummoningSpecialBuilder
    {
        private static void ConfigureGriffonSpecials(IDictionary<string,BlueprintScriptableObject> bySymbol)
        {
            const string symbol="KMG.Summoning.Special.Griffon.CombatTraits";
            var unit=Require<BlueprintUnit>(bySymbol,"KMG.Summoning.Unit.Griffon");
            var traits=Require<BlueprintBuff>(bySymbol,symbol);
            var rake=ScriptableObject.CreateInstance<SummonRakeComponent>();
            rake.RequireNativeChargeCommand=true;
            traits.name=InternalName(symbol);traits.IsClassFeature=true;traits.Stacking=StackingType.Replace;
            traits.ComponentsArray=new BlueprintComponent[]{rake,ScriptableObject.CreateInstance<GriffonLifetimeBoundary>()};SetBuffFlags(traits,false,true);
            ExpandedSummoningNaturalBuilder.NameMagicalBeastRuntimeComponents(traits);
            BlueprintUnitFactAccess.Resolve().Configure(traits,
                LocalizationService.Create(symbol+".Name","Griffon Pounce and Rake"),
                LocalizationService.Create(symbol+".Description","Two rake claws occur on a qualifying pounce, never as unconditional ordinary attacks. This creature has no special Grab."),null);
            unit.AddFacts=unit.AddFacts.Concat(new BlueprintUnitFact[]{traits}).ToArray();
            // Repair R5 MECH-01: the engine grants the native Charge to every
            // unit; a second explicit grant showed two identical Charge actions.
            if(unit.AddFacts.Any(fact=>fact!=null&&fact.AssetGuid==SummonPlaytestR5Policy.NativeChargeAbilityGuid))
                throw new System.InvalidOperationException("Griffon must not grant the native Charge a second time.");
        }
    }
}
