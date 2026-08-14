using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using UnityEngine;

namespace KingmakerGunslinger.ElvenBranchedSpear
{
    internal static class MovementOpportunityAccuracyDiagnostics
    {
        internal static int Evaluated { get; private set; }
        internal static int Applied { get; private set; }
        internal static int LastBonus { get; private set; }
        internal static int LastAttackBonus { get; private set; }

        internal static void Reset()
        {
            Evaluated = 0;
            Applied = 0;
            LastBonus = 0;
            LastAttackBonus = int.MinValue;
        }

        internal static void Record(bool applied, int bonus)
        {
            Evaluated++;
            if (!applied) return;
            Applied++;
            LastBonus = bonus;
        }

        internal static void Complete(int attackBonus)
        {
            LastAttackBonus = attackBonus;
        }
    }

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
                    evt.Weapon.Blueprint.Type.Category))
                return;
            bool movement = MovementOpportunityAttackTracker.IsRunning(
                evt.Initiator);
            MovementOpportunityAccuracyDiagnostics.Record(movement,
                movement ? ElvenBranchedSpearCatalog
                    .MovementAttackOfOpportunityBonus : 0);
            if (!movement) return;
            evt.AddBonus(ElvenBranchedSpearCatalog.MovementAttackOfOpportunityBonus,
                Fact);
        }

        public void OnEventDidTrigger(
            RuleCalculateAttackBonusWithoutTarget evt)
        {
            if (evt != null) MovementOpportunityAccuracyDiagnostics.Complete(
                evt.Result);
        }
    }

    /// <summary>
    /// Kingmaker's closed WeaponCategory table does not apply the ordinary -4
    /// nonproficiency modifier to a runtime-added category. Preserve the native
    /// UnitDescriptor.Proficiencies authority and add only that missing rule
    /// modifier for this exact custom family.
    /// </summary>
    internal sealed class ElvenBranchedSpearProficiencyPenaltyComponent :
        WeaponEnchantmentLogic,
        IInitiatorRulebookHandler<RuleAttackRoll>
    {
        internal static ElvenBranchedSpearProficiencyPenaltyComponent Create()
        {
            return ScriptableObject.CreateInstance<
                ElvenBranchedSpearProficiencyPenaltyComponent>();
        }

        public void OnEventAboutToTrigger(
            RuleAttackRoll evt)
        {
            if (evt == null || evt.Initiator == null || evt.Weapon == null ||
                evt.Weapon.Blueprint == null || evt.Weapon.Blueprint.Type == null ||
                !ElvenBranchedSpearCategoryRuntime.Owns(
                    evt.Weapon.Blueprint.Type.Category) ||
                evt.Initiator.Descriptor.Proficiencies.Contains(
                    ElvenBranchedSpearCategoryRuntime.Category))
                return;
            evt.SetAttackBonusPenalty(evt.AttackBonusPenalty + 4);
        }

        public void OnEventDidTrigger(
            RuleAttackRoll evt) { }
    }
}
