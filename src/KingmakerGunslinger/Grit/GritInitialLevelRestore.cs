using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;

namespace KingmakerGunslinger.Grit
{
    /// <summary>
    /// Reconciles the first grit fill after level mechanics and attribute
    /// recalculation complete. Later Gunslinger or unrelated levels never refill.
    /// </summary>
    public sealed class GritInitialLevelRestore : OwnedGameLogicComponent<UnitDescriptor>,
        IUnitReapplyFeaturesOnLevelUpHandler, IUnitSubscriber
    {
        public BlueprintAbilityResource Resource;
        public BlueprintCharacterClass CharacterClass;

        public void HandleUnitReapplyFeaturesOnLevelUp()
        {
            if (Owner == null || Resource == null || CharacterClass == null ||
                Owner.Progression == null ||
                Owner.Progression.GetClassLevel(CharacterClass) != 1)
                return;
            Owner.Resources.Restore(Resource);
        }
    }
}
