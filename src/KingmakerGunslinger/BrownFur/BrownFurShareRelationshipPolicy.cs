namespace KingmakerGunslinger.BrownFur
{
    internal sealed class BrownFurShareRelationshipFacts
    {
        internal bool IsSelf { get; set; }
        internal bool IsPartyMember { get; set; }
        internal bool IsControlledCompanion { get; set; }
        internal bool IsAnimalCompanion { get; set; }
        internal bool IsControlledSummon { get; set; }
        internal bool CasterFactionKnown { get; set; }
        internal bool TargetFactionKnown { get; set; }
        internal bool SameFaction { get; set; }
        internal bool CasterSeesEnemy { get; set; }
        internal bool TargetSeesEnemy { get; set; }
        internal bool CasterCanAttackTarget { get; set; }
        internal bool TargetCanAttackCaster { get; set; }
    }

    /// <summary>
    /// Converts native relationship facts into the mission-authorized willing
    /// target categories. Unknown or contradictory faction states fail closed.
    /// </summary>
    internal static class BrownFurShareRelationshipPolicy
    {
        internal static BrownFurShareTargetRelationship Decide(
            BrownFurShareRelationshipFacts facts)
        {
            if (facts == null)
                return BrownFurShareTargetRelationship.Unknown;

            bool enemy = facts.CasterSeesEnemy || facts.TargetSeesEnemy;
            bool controlled = facts.IsSelf || facts.IsPartyMember ||
                facts.IsControlledCompanion || facts.IsAnimalCompanion ||
                facts.IsControlledSummon;
            if (enemy && controlled)
                return BrownFurShareTargetRelationship.Ambiguous;
            if (enemy)
                return BrownFurShareTargetRelationship.Enemy;

            if (facts.IsSelf)
                return BrownFurShareTargetRelationship.Self;
            if (facts.IsAnimalCompanion)
                return BrownFurShareTargetRelationship.AnimalCompanion;
            if (facts.IsControlledSummon)
                return BrownFurShareTargetRelationship.ControlledSummon;
            if (facts.IsPartyMember)
                return BrownFurShareTargetRelationship.PartyMember;
            if (facts.IsControlledCompanion)
                return BrownFurShareTargetRelationship.ControlledCompanion;

            if (!facts.CasterFactionKnown || !facts.TargetFactionKnown)
                return BrownFurShareTargetRelationship.Unknown;

            bool attackable = facts.CasterCanAttackTarget ||
                facts.TargetCanAttackCaster;
            if (facts.SameFaction)
                return attackable ?
                    BrownFurShareTargetRelationship.FriendlyAttackable :
                    BrownFurShareTargetRelationship.FriendlyUnattackable;
            if (attackable)
                return BrownFurShareTargetRelationship.HostileNeutral;
            return BrownFurShareTargetRelationship.Ambiguous;
        }
    }
}
