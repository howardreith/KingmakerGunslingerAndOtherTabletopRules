using System;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using UnityEngine;

namespace KingmakerGunslinger.ElementalRaces
{
    // Unregistered, no bootstrap call, no icon/localization or acquisition.
    // This deliberately does not edit the earlier trait foundation factory.
    internal static class UnpublishedAerialObserverFoundationFactory
    {
        internal static BlueprintBuff Create(BlueprintBuff canonicalFlightCarrier)
        {
            AerialObserverFlightContract.VerifyCarrier(canonicalFlightCarrier);
            var flags = typeof(BlueprintBuff).GetField("m_Flags", BindingFlags.Instance | BindingFlags.NonPublic);
            if (flags == null || flags.FieldType.FullName != "Kingmaker.UnitLogic.Buffs.Blueprints.BlueprintBuff+Flags" ||
                !Enum.IsDefined(flags.FieldType, "HiddenInUi"))
                throw new MissingFieldException("BlueprintBuff.m_Flags");
            var provider = ScriptableObject.CreateInstance<BlueprintBuff>();
            provider.name = "KMG_Unpublished_AerialObserver_Provider";
            flags.SetValue(provider, Enum.Parse(flags.FieldType, "HiddenInUi", false));
            provider.Stacking = StackingType.Stack;
            provider.IsClassFeature = true;
            provider.FxOnStart = new PrefabLink(); provider.FxOnRemove = new PrefabLink();
            provider.ResourceAssetIds = Array.Empty<string>();
            var bonus = ScriptableObject.CreateInstance<AerialObserverPerceptionBonus>();
            bonus.name = "$UnpublishedAerialObserverPerception";
            bonus.FlightCarrier = canonicalFlightCarrier;
            provider.ComponentsArray = new BlueprintComponent[] { bonus };
            return provider;
        }
    }
}
