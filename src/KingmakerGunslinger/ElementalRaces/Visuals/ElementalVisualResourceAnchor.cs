using Kingmaker.Visual.CharacterSystem;
using UnityEngine;

namespace KingmakerGunslinger.ElementalRaces.Visuals
{
    /// <summary>
    /// Keeps exactly the registered elemental visual proxies and their bound
    /// native donors reachable from a live hidden GameObject. Unity's unused
    /// asset sweep may destroy any asset that is referenced only from managed
    /// state, so the exact owned identity set needs one native holder for the
    /// whole session; nothing outside that set is pinned.
    /// </summary>
    internal sealed class ElementalVisualResourceAnchor : MonoBehaviour
    {
        public EquipmentEntity[] Proxies;
        public EquipmentEntity[] Donors;
    }
}
