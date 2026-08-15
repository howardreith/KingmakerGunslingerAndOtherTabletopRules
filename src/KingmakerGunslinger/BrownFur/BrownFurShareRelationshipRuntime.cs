using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.BrownFur
{
    internal static class BrownFurShareRelationshipRuntime
    {
        internal static BrownFurShareTargetRelationship Classify(
            UnitDescriptor caster, UnitEntityData target)
        {
            if (caster == null || caster.Unit == null || target == null ||
                target.Descriptor == null)
                return BrownFurShareTargetRelationship.Unknown;
            try
            {
                UnitEntityData casterUnit = caster.Unit;
                List<UnitEntityData> party = Game.Instance == null ||
                    Game.Instance.Player == null ||
                    Game.Instance.Player.Party == null ?
                    new List<UnitEntityData>() : Game.Instance.Player.Party;
                bool partyMember = party.Any(value =>
                    ReferenceEquals(value, target));
                bool animalCompanion = ReferenceEquals(caster.Pet, target) ||
                    party.Any(value => value != null &&
                        value.Descriptor != null &&
                        ReferenceEquals(value.Descriptor.Pet, target));
                UnitPartSummonedMonster summoned =
                    target.Get<UnitPartSummonedMonster>();
                bool controlledSummon = summoned != null &&
                    summoned.IsDirectlyControllable &&
                    (ReferenceEquals(summoned.Summoner, casterUnit) ||
                     party.Any(value => ReferenceEquals(value,
                         summoned.Summoner)) ||
                     (summoned.Summoner != null &&
                      summoned.Summoner.IsDirectlyControllable));

                return BrownFurShareRelationshipPolicy.Decide(
                    new BrownFurShareRelationshipFacts {
                        IsSelf = ReferenceEquals(casterUnit, target),
                        IsPartyMember = partyMember,
                        IsAnimalCompanion = animalCompanion,
                        IsControlledSummon = controlledSummon,
                        IsControlledCompanion =
                            target.IsDirectlyControllable &&
                            !partyMember && !animalCompanion &&
                            !controlledSummon,
                        CasterFactionKnown = casterUnit.Faction != null,
                        TargetFactionKnown = target.Faction != null,
                        SameFaction = casterUnit.Faction != null &&
                            ReferenceEquals(casterUnit.Faction, target.Faction),
                        CasterSeesEnemy = casterUnit.IsEnemy(target),
                        TargetSeesEnemy = target.IsEnemy(casterUnit),
                        CasterCanAttackTarget = casterUnit.CanAttack(target),
                        TargetCanAttackCaster = target.CanAttack(casterUnit)
                    });
            }
            catch (Exception exception)
            {
                ModContext context;
                if (ModContext.TryGet(out context))
                    BrownFurDiagnostics.Failure(context,
                        "share.relationship.blocked",
                        "Native willing-target classification failed closed.",
                        exception);
                return BrownFurShareTargetRelationship.Ambiguous;
            }
        }
    }
}
