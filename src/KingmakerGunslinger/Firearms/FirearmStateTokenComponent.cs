using System;
using Kingmaker.Blueprints;
using UnityEngine;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Passive marker carried by a no-op weapon-enchantment blueprint. The item
    /// enchantment instance is the candidate serialized carrier; this component defines
    /// the strict state represented by its stable blueprint GUID.
    /// </summary>
    [Serializable]
    public sealed class FirearmStateTokenComponent : BlueprintComponent
    {
        [SerializeField]
        private string m_TokenId;

        [SerializeField]
        private int m_SchemaVersion;

        [SerializeField]
        private int m_LoadedRounds;

        [SerializeField]
        private string m_LoadedAmmunitionId;

        [SerializeField]
        private FirearmCondition m_Condition;

        internal string TokenId
        {
            get { return m_TokenId; }
        }

        internal FirearmStateTokenDefinition Definition
        {
            get
            {
                AmmunitionId ammunition = string.IsNullOrWhiteSpace(m_LoadedAmmunitionId)
                    ? null
                    : new AmmunitionId(m_LoadedAmmunitionId);
                return new FirearmStateTokenDefinition(
                    m_TokenId,
                    new FirearmState(
                        m_SchemaVersion,
                        m_LoadedRounds,
                        ammunition,
                        m_Condition));
            }
        }

        internal static FirearmStateTokenComponent Create(
            FirearmStateTokenDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException("definition");
            }

            FirearmStateTokenComponent component =
                ScriptableObject.CreateInstance<FirearmStateTokenComponent>();
            component.m_TokenId = definition.TokenId;
            component.m_SchemaVersion = definition.State.SchemaVersion;
            component.m_LoadedRounds = definition.State.LoadedRounds;
            component.m_LoadedAmmunitionId = definition.State.LoadedAmmunition == null
                ? null
                : definition.State.LoadedAmmunition.Value;
            component.m_Condition = definition.State.Condition;
            if (!definition.Equals(component.Definition))
            {
                throw new InvalidOperationException(
                    "The firearm-state token component failed its definition round-trip.");
            }

            return component;
        }
    }
}
