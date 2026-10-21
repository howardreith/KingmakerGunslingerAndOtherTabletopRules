using System;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>The native brain inventories usable spell actions when combat
    /// starts. Call Lightning grants its bolt later through the active state,
    /// so refresh this exact owner's inventory after that native grant.</summary>
    [Serializable]
    public sealed class Sprint29StormLightningBrainRefresh :
        OwnedGameLogicComponent<UnitDescriptor>
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintBuff State;
        public BlueprintAbility Bolt;

        public override void OnTurnOn()
        {
            if (Owner?.Unit == null || Fact == null ||
                !ReferenceEquals(Owner.Unit.Blueprint, OwningBlueprint) ||
                !ReferenceEquals(Fact.Blueprint, State) ||
                Owner.Unit.Brain == null || Bolt == null ||
                Owner.Abilities.GetAbility(Bolt) == null)
                return;
            Owner.Unit.Brain.RestoreAvailableActions();
        }
    }
}
