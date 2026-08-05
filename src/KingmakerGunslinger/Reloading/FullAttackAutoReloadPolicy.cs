using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Reloading
{
    internal enum FullAttackReloadDecision
    {
        None = 0,
        ContinueLoaded = 1,
        Reload = 2,
        EndFullAttack = 3
    }

    /// <summary>
    /// Pure gate evaluated immediately before a native iterative attack begins.
    ///
    /// The first attack proceeds normally.  A later attack with the same exact
    /// firearm may continue while the chamber still contains a round, or reload
    /// inside the existing full-attack command only when the effective reload
    /// action is genuinely Free.  Every other empty-firearm case ends the
    /// remaining full attack before Kingmaker creates a fake empty shot.
    /// </summary>
    internal static class FullAttackAutoReloadPolicy
    {
        internal static FullAttackReloadDecision Evaluate(
            bool isFullAttack,
            bool hasPreviousAttack,
            bool hasPlannedAttack,
            bool sameExactWeapon,
            bool targetAlive,
            EffectiveReloadAction reloadAction,
            FirearmState state,
            FirearmCondition effectiveCondition)
        {
            if (state == null) throw new ArgumentNullException("state");
            if (!Enum.IsDefined(typeof(EffectiveReloadAction), reloadAction))
                throw new ArgumentOutOfRangeException("reloadAction");
            if (!Enum.IsDefined(typeof(FirearmCondition), effectiveCondition))
                throw new ArgumentOutOfRangeException("effectiveCondition");

            if (!isFullAttack || !hasPreviousAttack || !hasPlannedAttack ||
                !sameExactWeapon || !targetAlive)
                return FullAttackReloadDecision.None;
            if (effectiveCondition == FirearmCondition.Wrecked)
                return FullAttackReloadDecision.EndFullAttack;
            if (!state.IsEmpty)
                return FullAttackReloadDecision.ContinueLoaded;
            if (reloadAction != EffectiveReloadAction.Free)
                return FullAttackReloadDecision.EndFullAttack;
            return FullAttackReloadDecision.Reload;
        }
    }
}
