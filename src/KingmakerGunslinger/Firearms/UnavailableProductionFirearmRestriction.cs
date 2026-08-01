using System;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.UnitLogic;
using UnityEngine;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Fail-closed equipment gate used only while a production content entry has an
    /// explicitly unimplemented mechanical prerequisite such as scatter range.
    /// </summary>
    [Serializable]
    public sealed class UnavailableProductionFirearmRestriction : EquipmentRestriction
    {
        internal static UnavailableProductionFirearmRestriction Create()
        {
            return ScriptableObject.CreateInstance<UnavailableProductionFirearmRestriction>();
        }

        public override bool CanBeEquippedBy(UnitDescriptor unit)
        {
            return false;
        }
    }
}
