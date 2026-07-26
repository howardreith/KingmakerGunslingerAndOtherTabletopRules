using System;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.UnitLogic;
using UnityEngine;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Item-level gate for firearms. The reused crossbow weapon category remains an
    /// engine/animation adapter; possession of the dedicated feature is the source of
    /// firearm identity for equipment permission.
    /// </summary>
    [Serializable]
    public sealed class FirearmProficiencyRestriction : EquipmentRestriction
    {
        [SerializeField]
        private BlueprintFeature m_RequiredProficiency;

        internal BlueprintFeature RequiredProficiency
        {
            get { return m_RequiredProficiency; }
        }

        internal static FirearmProficiencyRestriction Create(
            BlueprintFeature requiredProficiency)
        {
            if (requiredProficiency == null)
            {
                throw new ArgumentNullException("requiredProficiency");
            }

            FirearmProficiencyRestriction restriction =
                ScriptableObject.CreateInstance<FirearmProficiencyRestriction>();
            restriction.m_RequiredProficiency = requiredProficiency;
            return restriction;
        }

        public override bool CanBeEquippedBy(UnitDescriptor unit)
        {
            return unit != null &&
                m_RequiredProficiency != null &&
                unit.Progression != null &&
                unit.Progression.Features.GetRank(m_RequiredProficiency) > 0;
        }
    }
}
