using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;

namespace KingmakerGunslinger.Spells.ShieldOther
{
    public sealed class ShieldOtherBuffComponent :
        OwnedGameLogicComponent<UnitDescriptor>, ITickEachRound
    {
        private const float FeetPerMeter = 3.2808399f;

        public void OnNewRound()
        {
            ShieldOtherLinkValidityDecision decision;
            if (!TryEvaluate(out decision) || !decision.Valid)
            {
                Buff buff = Fact as Buff;
                if (buff != null) buff.Remove();
            }
        }

        internal bool TryEvaluate(out ShieldOtherLinkValidityDecision decision)
        {
            decision = null;
            UnitEntityData subject = Owner == null ? null : Owner.Unit;
            UnitEntityData caster = Fact == null || Fact.MaybeContext == null
                ? null : Fact.MaybeContext.MaybeCaster;
            int casterLevel = Fact == null || Fact.MaybeContext == null ||
                Fact.MaybeContext.Params == null ? 0 :
                Fact.MaybeContext.Params.CasterLevel;
            bool casterAlive = caster != null && caster.Descriptor != null &&
                caster.Descriptor.State != null && !caster.Descriptor.State.IsDead;
            bool sameArea = subject != null && caster != null &&
                subject.IsInGame && caster.IsInGame;
            float distanceFeet = subject == null || caster == null ? 0f :
                subject.DistanceTo(caster) * FeetPerMeter;
            decision = ShieldOtherLinkValidityPolicy.Evaluate(
                new ShieldOtherLinkValidityRequest {
                    SubjectPresent = subject != null,
                    CasterPresent = caster != null,
                    CasterAlive = casterAlive,
                    SameArea = sameArea,
                    CasterLevel = casterLevel,
                    DistanceFeet = distanceFeet
                });
            return true;
        }
    }
}
