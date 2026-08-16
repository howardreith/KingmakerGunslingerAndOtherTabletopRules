using System;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;

namespace KingmakerGunslinger.UrbanBarbarian
{
    public sealed class CrowdControlComponent :
        OwnedGameLogicComponent<UnitDescriptor>,
        IInitiatorRulebookHandler<RuleCalculateAttackBonusWithoutTarget>,
        IInitiatorRulebookHandler<RuleCalculateAC>
    {
        private const float FiveFeetMeters = 1.524f;
        private const float ToleranceMeters = 0.00031f;

        public void OnEventAboutToTrigger(
            RuleCalculateAttackBonusWithoutTarget evt)
        {
            if (evt != null && HasCrowd()) evt.AddBonus(1, Fact);
        }

        public void OnEventDidTrigger(
            RuleCalculateAttackBonusWithoutTarget evt) { }

        public void OnEventAboutToTrigger(RuleCalculateAC evt)
        {
            if (evt == null || !HasCrowd() || Owner == null ||
                Owner.Stats == null) return;
            ModifiableValue.Modifier modifier = Owner.Stats.AC.AddModifier(1,
                Fact, GetType().FullName, ModifierDescriptor.Dodge);
            if (modifier != null) evt.AddTemporaryModifier(modifier);
        }

        public void OnEventDidTrigger(RuleCalculateAC evt) { }

        internal bool HasCrowd()
        {
            UnitEntityData owner = Owner == null ? null : Owner.Unit;
            return CountAdjacentActiveEnemies(owner) >= 2;
        }

        internal static int CountAdjacentActiveEnemies(UnitEntityData owner)
        {
            if (owner == null || Game.Instance == null ||
                Game.Instance.State == null || Game.Instance.State.Units == null)
                return 0;
            int adjacent = 0;
            foreach (UnitEntityData candidate in Game.Instance.State.Units.All)
            {
                if (!IsAdjacentActiveEnemy(owner, candidate)) continue;
                adjacent++;
            }
            return adjacent;
        }

        private static bool IsAdjacentActiveEnemy(UnitEntityData owner,
            UnitEntityData candidate)
        {
            return candidate != null && !ReferenceEquals(owner, candidate) &&
                candidate.IsInGame && !candidate.Destroyed &&
                !candidate.IsDetached && candidate.IsTurnedOn &&
                candidate.Descriptor != null && candidate.Descriptor.State != null &&
                candidate.Descriptor.State.IsConscious && owner.IsEnemy(candidate) &&
                owner.DistanceTo(candidate) <= FiveFeetMeters + ToleranceMeters;
        }
    }
}
