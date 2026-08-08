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

        [SerializeField]
        private BlueprintFeature m_OneHandedProficiency;

        [SerializeField]
        private BlueprintFeature m_TwoHandedProficiency;

        [SerializeField]
        private FirearmKind m_FirearmKind;

        internal BlueprintFeature RequiredProficiency
        {
            get { return m_RequiredProficiency; }
        }

        internal BlueprintFeature OneHandedProficiency
        {
            get { return m_OneHandedProficiency; }
        }

        internal BlueprintFeature TwoHandedProficiency
        {
            get { return m_TwoHandedProficiency; }
        }

        internal FirearmKind FirearmKind
        {
            get { return m_FirearmKind; }
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
            restriction.m_FirearmKind = FirearmKind.Unknown;
            return restriction;
        }

        internal static FirearmProficiencyRestriction Create(
            BlueprintFeature fullProficiency,
            BlueprintFeature oneHandedProficiency,
            BlueprintFeature twoHandedProficiency,
            FirearmKind firearmKind)
        {
            if (fullProficiency == null) throw new ArgumentNullException("fullProficiency");
            if (oneHandedProficiency == null) throw new ArgumentNullException("oneHandedProficiency");
            if (twoHandedProficiency == null) throw new ArgumentNullException("twoHandedProficiency");
            FirearmHandednessPolicy.Require(firearmKind);
            FirearmProficiencyRestriction restriction =
                ScriptableObject.CreateInstance<FirearmProficiencyRestriction>();
            restriction.m_RequiredProficiency = fullProficiency;
            restriction.m_OneHandedProficiency = oneHandedProficiency;
            restriction.m_TwoHandedProficiency = twoHandedProficiency;
            restriction.m_FirearmKind = firearmKind;
            return restriction;
        }

        public override bool CanBeEquippedBy(UnitDescriptor unit)
        {
            if (unit == null || unit.Progression == null ||
                m_RequiredProficiency == null) return false;
            bool full = unit.Progression.Features.GetRank(m_RequiredProficiency) > 0;
            // The retained one-argument factory is used only by the development
            // Test Musket and preserves its historical full-proficiency gate.
            if (m_FirearmKind == FirearmKind.Unknown) return full;
            return FirearmProficiencyPolicy.CanUse(1, m_FirearmKind, full,
                m_OneHandedProficiency != null &&
                    unit.Progression.Features.GetRank(m_OneHandedProficiency) > 0,
                m_TwoHandedProficiency != null &&
                    unit.Progression.Features.GetRank(m_TwoHandedProficiency) > 0);
        }
    }
}
