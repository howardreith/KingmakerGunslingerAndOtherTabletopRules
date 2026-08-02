using UnityEngine;

namespace KingmakerGunslinger.Gunsmithing
{
    internal sealed class BatteredFirearmOriginComponent :
        Kingmaker.Blueprints.BlueprintComponent
    {
        internal static BatteredFirearmOriginComponent Create()
        {
            return ScriptableObject.CreateInstance<BatteredFirearmOriginComponent>();
        }
    }
}
