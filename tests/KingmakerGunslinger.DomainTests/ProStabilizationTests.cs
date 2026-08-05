using System;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Reloading;

namespace KingmakerGunslinger.DomainTests
{
    internal static partial class Program
    {
        private static FirearmState EmptyNormalFirearm()
        {
            return FirearmState.CreateEmpty();
        }

        private static void FullAttackAutoReloadEligibleFreeAction()
        {
            Assertions.Equal(FullAttackReloadDecision.Reload,
                FullAttackAutoReloadPolicy.Evaluate(
                    true, true, true, true, true,
                    EffectiveReloadAction.Free,
                    EmptyNormalFirearm(), FirearmCondition.Normal),
                "A truly free reload immediately before a later iterative attack should reload.");
        }

        private static void FullAttackAutoReloadInterruptsNonFreeActions()
        {
            foreach (EffectiveReloadAction action in new[] {
                EffectiveReloadAction.Unknown,
                EffectiveReloadAction.Move,
                EffectiveReloadAction.Standard,
                EffectiveReloadAction.FullRound })
            {
                Assertions.Equal(FullAttackReloadDecision.EndFullAttack,
                    FullAttackAutoReloadPolicy.Evaluate(
                        true, true, true, true, true,
                        action, EmptyNormalFirearm(), FirearmCondition.Normal),
                    "A non-free reload must end remaining empty attacks: " + action);
            }
        }

        private static void FullAttackAutoReloadContinuesLoadedCapacity()
        {
            FirearmState loaded = new FirearmState(
                FirearmState.CurrentSchemaVersion, 1,
                FirearmStateTokenCatalog.DiagnosticLeadBall,
                FirearmCondition.Normal);
            Assertions.Equal(FullAttackReloadDecision.ContinueLoaded,
                FullAttackAutoReloadPolicy.Evaluate(
                    true, true, true, true, true,
                    EffectiveReloadAction.Free,
                    loaded, FirearmCondition.Normal),
                "A firearm with a remaining round must continue without reloading.");
        }

        private static void FullAttackAutoReloadRequiresSameWeaponAndNextAttack()
        {
            Assertions.Equal(FullAttackReloadDecision.None,
                FullAttackAutoReloadPolicy.Evaluate(
                    false, true, true, true, true,
                    EffectiveReloadAction.Free,
                    EmptyNormalFirearm(), FirearmCondition.Normal),
                "A non-full attack must not reload inside the attack command.");
            Assertions.Equal(FullAttackReloadDecision.None,
                FullAttackAutoReloadPolicy.Evaluate(
                    true, false, true, true, true,
                    EffectiveReloadAction.Free,
                    EmptyNormalFirearm(), FirearmCondition.Normal),
                "The first attack must not reload before it has fired.");
            Assertions.Equal(FullAttackReloadDecision.None,
                FullAttackAutoReloadPolicy.Evaluate(
                    true, true, true, false, true,
                    EffectiveReloadAction.Free,
                    EmptyNormalFirearm(), FirearmCondition.Normal),
                "A different planned weapon must not be reloaded.");
            Assertions.Equal(FullAttackReloadDecision.None,
                FullAttackAutoReloadPolicy.Evaluate(
                    true, true, false, true, true,
                    EffectiveReloadAction.Free,
                    EmptyNormalFirearm(), FirearmCondition.Normal),
                "No reload is needed when no iterative attack is planned.");
            Assertions.Equal(FullAttackReloadDecision.None,
                FullAttackAutoReloadPolicy.Evaluate(
                    true, true, true, true, false,
                    EffectiveReloadAction.Free,
                    EmptyNormalFirearm(), FirearmCondition.Normal),
                "A missing live target must not consume ammunition through an unnecessary reload.");
        }

        private static void FullAttackAutoReloadInterruptsWreckedFirearm()
        {
            Assertions.Equal(FullAttackReloadDecision.EndFullAttack,
                FullAttackAutoReloadPolicy.Evaluate(
                    true, true, true, true, true,
                    EffectiveReloadAction.Free,
                    EmptyNormalFirearm(), FirearmCondition.Wrecked),
                "An empty Wrecked firearm must end remaining iterative attacks.");
            FirearmState loaded = new FirearmState(
                FirearmState.CurrentSchemaVersion, 1,
                FirearmStateTokenCatalog.DiagnosticLeadBall,
                FirearmCondition.Normal);
            Assertions.Equal(FullAttackReloadDecision.EndFullAttack,
                FullAttackAutoReloadPolicy.Evaluate(
                    true, true, true, true, true,
                    EffectiveReloadAction.Free,
                    loaded, FirearmCondition.Wrecked),
                "A loaded firearm with an effective Wrecked condition must not continue attacking.");
        }

        private static void FullAttackAutoReloadRejectsInvalidInputs()
        {
            Assertions.Throws<ArgumentNullException>(() =>
                FullAttackAutoReloadPolicy.Evaluate(true, true, true, true, true,
                    EffectiveReloadAction.Free, null,
                    FirearmCondition.Normal),
                "A null firearm state must fail closed.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                FullAttackAutoReloadPolicy.Evaluate(true, true, true, true, true,
                    (EffectiveReloadAction)99,
                    EmptyNormalFirearm(), FirearmCondition.Normal),
                "An undefined reload action must fail closed.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                FullAttackAutoReloadPolicy.Evaluate(true, true, true, true, true,
                    EffectiveReloadAction.Free,
                    EmptyNormalFirearm(), (FirearmCondition)99),
                "An undefined effective condition must fail closed.");
        }
    }
}
