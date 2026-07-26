using System;
using Kingmaker.Blueprints;
using UnityEngine;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Passive marker/configuration component attached to firearm weapon blueprints.
    /// It stores immutable definition fields only. Loaded ammunition, item condition,
    /// jams, owners, and combat event state belong to later per-item services.
    /// </summary>
    [Serializable]
    public sealed class FirearmDefinitionComponent : BlueprintComponent
    {
        [SerializeField]
        private FirearmEra m_Era;

        [SerializeField]
        private FirearmKind m_Kind;

        [SerializeField]
        private int m_Capacity;

        [SerializeField]
        private int m_RangeIncrementFeet;

        [SerializeField]
        private int m_MisfireValue;

        [SerializeField]
        private int m_MisfireBurstRadiusFeet;

        [SerializeField]
        private ReloadActionType m_BaseReloadAction;

        [SerializeField]
        private bool m_RequiresFreeHand;

        [SerializeField]
        private int m_RoundsPerAction;

        [SerializeField]
        private string m_AmmunitionId;

        [SerializeField]
        private bool m_IsScatter;

        internal FirearmDefinition Definition
        {
            get
            {
                ReloadProfile reload = new ReloadProfile(
                    m_BaseReloadAction,
                    m_RequiresFreeHand,
                    m_RoundsPerAction,
                    new AmmunitionId(m_AmmunitionId));
                return new FirearmDefinition(
                    m_Era,
                    m_Kind,
                    m_Capacity,
                    m_RangeIncrementFeet,
                    m_MisfireValue,
                    m_MisfireBurstRadiusFeet,
                    reload,
                    m_IsScatter);
            }
        }

        internal static FirearmDefinitionComponent Create(FirearmDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException("definition");
            }

            FirearmDefinitionComponent component =
                ScriptableObject.CreateInstance<FirearmDefinitionComponent>();
            component.m_Era = definition.Era;
            component.m_Kind = definition.Kind;
            component.m_Capacity = definition.Capacity;
            component.m_RangeIncrementFeet = definition.RangeIncrementFeet;
            component.m_MisfireValue = definition.MisfireValue;
            component.m_MisfireBurstRadiusFeet = definition.MisfireBurstRadiusFeet;
            component.m_BaseReloadAction = definition.Reload.BaseAction;
            component.m_RequiresFreeHand = definition.Reload.RequiresFreeHand;
            component.m_RoundsPerAction = definition.Reload.RoundsPerAction;
            component.m_AmmunitionId = definition.Reload.Ammunition.Value;
            component.m_IsScatter = definition.IsScatter;
            return component;
        }
    }
}
