using System;
using Kingmaker.ElementsSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Mechanics.Actions;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Runs its wrapped actions only when the attack that triggered them dealt
    /// positive final damage.
    ///
    /// <para>This wraps the native poison graph rather than replacing it. The
    /// whole native action list - the saving throw, its context, the conditional
    /// branch and the buff with its repeat-save lifecycle - is preserved
    /// untouched inside <see cref="Actions"/>, so nothing Sprint 10 qualified
    /// about how the venom behaves once applied is disturbed. What changes is
    /// only whether it is reached.</para>
    ///
    /// <para>The in-flight attack is read through the rulebook context, which is
    /// how this codebase reaches a resolving rule from inside an action
    /// elsewhere. If the rule cannot be found the actions do not run: an injury
    /// poison with no evidence of an injury is the case this exists to
    /// stop.</para>
    /// </summary>
    [Serializable]
    public sealed class ContextActionOnlyIfWeaponWounded : ContextAction
    {
        public ActionList Actions;

        public override string GetCaption()
        {
            return "Deliver injury poison only on a wounding hit";
        }

        public override void RunAction()
        {
            if (Actions == null || Actions.Actions == null ||
                Actions.Actions.Length == 0) return;
            RulebookEventContext context = Rulebook.CurrentContext;
            RuleAttackWithWeapon attack = context == null ? null :
                context.LastEvent<RuleAttackWithWeapon>();
            int damage = attack == null || attack.MeleeDamage == null ? 0 :
                Math.Max(0, attack.MeleeDamage.Damage);
            bool hit = attack != null && attack.AttackRoll != null &&
                attack.AttackRoll.IsHit;
            if (!SummonInjuryPoisonPolicy.ShouldDeliver(attack != null, hit,
                    damage))
                return;
            Actions.Run();
        }
    }
}
