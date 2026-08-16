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
        ITargetRulebookHandler<RuleCalculateAC>
    {
        private const float FiveFeetMeters = 1.524f;
        private const float ToleranceMeters = 0.00031f;
        internal static string LastAttackObservation { get; private set; }
        internal static string LastArmorClassObservation { get; private set; }

        public void OnEventAboutToTrigger(
            RuleCalculateAttackBonusWithoutTarget evt)
        {
            int count = CountAdjacentActiveEnemies(Owner == null ? null :
                Owner.Unit);
            bool applies = evt != null && count >= 2;
            LastAttackObservation = "rule=" + (evt == null ? "<null>" :
                evt.GetType().FullName) + ";owner=" + Identity(Owner == null ?
                    null : Owner.Unit) + ";adjacent=" + count +
                ";applies=" + applies + ";value=" + (applies ? 1 : 0) +
                ";descriptor=Untyped;source=" +
                (Fact == null || Fact.Blueprint == null ? "<null>" :
                    Fact.Blueprint.AssetGuid);
            if (applies) evt.AddBonus(1, Fact);
        }

        public void OnEventDidTrigger(
            RuleCalculateAttackBonusWithoutTarget evt) { }

        public void OnEventAboutToTrigger(RuleCalculateAC evt)
        {
            int count = CountAdjacentActiveEnemies(Owner == null ? null :
                Owner.Unit);
            bool applies = evt != null && count >= 2 && Owner != null &&
                Owner.Stats != null;
            LastArmorClassObservation = "rule=" + (evt == null ? "<null>" :
                evt.GetType().FullName) + ";owner=" + Identity(Owner == null ?
                    null : Owner.Unit) + ";adjacent=" + count +
                ";applies=" + applies + ";value=" + (applies ? 1 : 0) +
                ";descriptor=" + ModifierDescriptor.Dodge + ";source=" +
                (Fact == null || Fact.Blueprint == null ? "<null>" :
                    Fact.Blueprint.AssetGuid);
            if (!applies ||
                Owner.Stats == null) return;
            ModifiableValue.Modifier modifier = Owner.Stats.AC.AddModifier(1,
                Fact, GetType().FullName, ModifierDescriptor.Dodge);
            if (modifier != null)
            {
                Owner.Stats.AC.UpdateValue();
                evt.AddTemporaryModifier(modifier);
            }
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
                EdgeDistance(owner, candidate) <=
                    FiveFeetMeters + ToleranceMeters;
        }

        internal static string DescribeCandidate(UnitEntityData owner,
            UnitEntityData candidate)
        {
            if (candidate == null) return "candidate=<null>";
            float center = owner == null ? float.PositiveInfinity :
                owner.DistanceTo(candidate);
            float edge = EdgeDistance(owner, candidate);
            return "owner=" + Identity(owner) + ";candidate=" +
                Identity(candidate) + ";center=" + center +
                ";ownerCorpulence=" + (owner == null ? float.NaN :
                    owner.Corpulence) + ";candidateCorpulence=" +
                candidate.Corpulence + ";edge=" + edge + ";isInGame=" +
                candidate.IsInGame + ";destroyed=" + candidate.Destroyed +
                ";detached=" + candidate.IsDetached + ";turnedOn=" +
                candidate.IsTurnedOn + ";conscious=" +
                (candidate.Descriptor != null && candidate.Descriptor.State !=
                    null && candidate.Descriptor.State.IsConscious) +
                ";enemy=" + (owner != null && owner.IsEnemy(candidate)) +
                ";adjacentActiveEnemy=" + IsAdjacentActiveEnemy(owner,
                    candidate);
        }

        private static string Identity(UnitEntityData unit)
        {
            return unit == null ? "<null>" : unit.UniqueId + "/" +
                (unit.Blueprint == null ? "<null>" : unit.Blueprint.AssetGuid);
        }

        internal static float EdgeDistance(UnitEntityData owner,
            UnitEntityData candidate)
        {
            if (owner == null || candidate == null) return float.PositiveInfinity;
            return (float)CrowdControlPolicy.EdgeDistance(
                owner.DistanceTo(candidate), owner.Corpulence,
                candidate.Corpulence);
        }
    }
}
