using System;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Reloading;
using KingmakerGunslinger.Firing;
using Feats = KingmakerGunslinger.Feats;

namespace KingmakerGunslinger.DomainTests
{
    internal static partial class Program
    {
        private static void DependentFeatAttackKind()
        {
            var value = Feats.FirearmWeaponFeatPolicy.Evaluate(FirearmKind.Pistol,
                FirearmKind.Pistol, Feats.FirearmWeaponFeatEffect.Attack, 1);
            Assertions.Equal(1, value.AttackBonus, "Greater Weapon Focus attack bonus changed.");
            Assertions.Equal(0, value.DamageBonus, "Attack feat leaked damage.");
        }

        private static void DependentFeatDamageKind()
        {
            var value = Feats.FirearmWeaponFeatPolicy.Evaluate(FirearmKind.Rifle,
                FirearmKind.Rifle, Feats.FirearmWeaponFeatEffect.Damage, 2);
            Assertions.Equal(2, value.DamageBonus, "Weapon Specialization damage changed.");
            Assertions.Equal(0, value.AttackBonus, "Damage feat leaked attack.");
        }

        private static void DependentFeatCriticalKind()
        {
            var value = Feats.FirearmWeaponFeatPolicy.Evaluate(FirearmKind.Revolver,
                FirearmKind.Revolver, Feats.FirearmWeaponFeatEffect.DoubleCriticalEdge, 0);
            Assertions.True(value.DoubleCriticalEdge, "Improved Critical did not double the edge.");
        }

        private static void DependentFeatWrongKind()
        {
            var value = Feats.FirearmWeaponFeatPolicy.Evaluate(FirearmKind.Pistol,
                FirearmKind.Musket, Feats.FirearmWeaponFeatEffect.Damage, 2);
            Assertions.Equal(0, value.DamageBonus, "Wrong firearm kind gained damage.");
            Assertions.False(value.DoubleCriticalEdge, "Wrong firearm kind gained critical edge.");
        }

        private static void DependentFeatInvalid()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                Feats.FirearmWeaponFeatPolicy.Evaluate(FirearmKind.Pistol,
                    FirearmKind.Pistol, (Feats.FirearmWeaponFeatEffect)99, 1),
                "Unknown feat effect must fail closed.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                Feats.FirearmWeaponFeatPolicy.Evaluate(FirearmKind.Pistol,
                    FirearmKind.Pistol, Feats.FirearmWeaponFeatEffect.Attack, -1),
                "Negative feat bonus must fail closed.");
        }
        private static void ReloadActionsBaseProfiles()
        {
            Assertions.Equal(EffectiveReloadAction.Standard,
                ReloadActionEconomy.Evaluate(FirearmDefinitions.CreateEarlyPistol(), false),
                "Early Pistol base reload changed.");
            Assertions.Equal(EffectiveReloadAction.FullRound,
                ReloadActionEconomy.Evaluate(FirearmDefinitions.CreateEarlyMusket(), false),
                "Early Musket base reload changed.");
            Assertions.Equal(EffectiveReloadAction.Move,
                ReloadActionEconomy.Evaluate(FirearmDefinitions.CreateAdvancedRifle(), false),
                "Advanced Rifle base reload changed.");
            Assertions.Equal(EffectiveReloadAction.Move,
                ReloadActionEconomy.Evaluate(FirearmDefinitions.CreateAdvancedRevolver(), false),
                "Advanced Revolver base reload changed.");
        }

        private static void ReloadActionsRapidReload()
        {
            Assertions.Equal(EffectiveReloadAction.Move,
                ReloadActionEconomy.Evaluate(FirearmDefinitions.CreateEarlyPistol(), true),
                "Rapid Reload (Pistol) must reduce standard to move.");
            Assertions.Equal(EffectiveReloadAction.Standard,
                ReloadActionEconomy.Evaluate(FirearmDefinitions.CreateEarlyMusket(), true),
                "Rapid Reload (Musket) must reduce full-round to standard.");
            Assertions.Equal(EffectiveReloadAction.Free,
                ReloadActionEconomy.Evaluate(FirearmDefinitions.CreateAdvancedRifle(), true),
                "Rapid Reload (Rifle) must reduce move to free.");
            Assertions.Equal(EffectiveReloadAction.Free,
                ReloadActionEconomy.Evaluate(FirearmDefinitions.CreateAdvancedRevolver(), true),
                "Rapid Reload (Revolver) must reduce move to free.");
        }

        private static void ReloadActionsWrongChoice()
        {
            Assertions.Equal(EffectiveReloadAction.Standard,
                ReloadActionEconomy.Evaluate(FirearmDefinitions.CreateEarlyPistol(), false),
                "A nonmatching Rapid Reload choice must not reduce Pistol reload.");
        }

        private static void ReloadActionsInvalid()
        {
            Assertions.Throws<ArgumentNullException>(() =>
                ReloadActionEconomy.Evaluate(null, false),
                "Null firearm definition must fail closed.");
        }

        private static void EmptyCommandLoadedAllows()
        {
            Assertions.Equal(EmptyFirearmCommandDisposition.Allow,
                EmptyFirearmAttackPolicy.Evaluate(true, false,
                    FirearmStateMachine.Load(EmptyState(),
                        new FirearmStateRules(1, new[] { BasicRound() }),
                        BasicRound(), 1), false, false),
                "A loaded firearm command must remain legal.");
        }

        private static void EmptyCommandUnloadedRejects()
        {
            Assertions.Equal(EmptyFirearmCommandDisposition.RejectUnloaded,
                EmptyFirearmAttackPolicy.Evaluate(true, false,
                    EmptyState(), false, true),
                "An unloaded firearm must reject before attack construction.");
        }

        private static void EmptyCommandWreckedRejects()
        {
            Assertions.Equal(EmptyFirearmCommandDisposition.RejectWrecked,
                EmptyFirearmAttackPolicy.Evaluate(true, false,
                    FirearmStateMachine.Wreck(EmptyState()), true, true),
                "A Wrecked firearm must never auto-reload or attack.");
        }

        private static void EmptyCommandAutoQueuesLegalReload()
        {
            Assertions.Equal(EmptyFirearmCommandDisposition.QueueReload,
                EmptyFirearmAttackPolicy.Evaluate(true, false,
                    EmptyState(), true, true),
                "Auto-reload may replace only a legal unloaded attack.");
            Assertions.Equal(EmptyFirearmCommandDisposition.RejectUnloaded,
                EmptyFirearmAttackPolicy.Evaluate(true, false,
                    EmptyState(), true, false),
                "Auto-reload without a legal reload must fail closed.");
        }

        private static void EmptyCommandAmbiguousRejects()
        {
            Assertions.Equal(EmptyFirearmCommandDisposition.RejectAmbiguous,
                EmptyFirearmAttackPolicy.Evaluate(false, true, null, true, true),
                "Ambiguous firearms must fail closed before state access.");
        }

        private static FirearmState EmptyState()
        {
            return new FirearmState(FirearmState.CurrentSchemaVersion, 0,
                null, FirearmCondition.Normal);
        }

        private static AmmunitionId BasicRound()
        {
            return new AmmunitionId("basic-round");
        }
    }
}
