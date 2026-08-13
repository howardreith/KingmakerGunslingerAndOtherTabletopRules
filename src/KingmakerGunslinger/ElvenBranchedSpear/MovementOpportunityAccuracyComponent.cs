using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using UnityEngine;

namespace KingmakerGunslinger.ElvenBranchedSpear
{
    internal sealed class MovementOpportunityAccuracyComponent :
        WeaponEnchantmentLogic,
        IInitiatorRulebookHandler<RuleCalculateAttackBonusWithoutTarget>
    {
        internal static MovementOpportunityAccuracyComponent Create()
        {
            return ScriptableObject.CreateInstance<
                MovementOpportunityAccuracyComponent>();
        }

        public void OnEventAboutToTrigger(
            RuleCalculateAttackBonusWithoutTarget evt)
        {
            if (evt == null || evt.Initiator == null || evt.Weapon == null ||
                evt.Weapon.Blueprint == null || evt.Weapon.Blueprint.Type == null ||
                !ElvenBranchedSpearCategoryRuntime.Owns(
                    evt.Weapon.Blueprint.Type.Category) ||
                !MovementOpportunityAttackTracker.IsRunning(evt.Initiator))
                return;
            evt.AddBonus(ElvenBranchedSpearCatalog.MovementAttackOfOpportunityBonus,
                Fact);
        }

        public void OnEventDidTrigger(
            RuleCalculateAttackBonusWithoutTarget evt) { }
    }
}
