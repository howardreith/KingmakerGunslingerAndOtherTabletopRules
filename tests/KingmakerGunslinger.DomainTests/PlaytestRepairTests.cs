using System;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Reloading;

namespace KingmakerGunslinger.DomainTests
{
    internal static partial class Program
    {
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
    }
}
