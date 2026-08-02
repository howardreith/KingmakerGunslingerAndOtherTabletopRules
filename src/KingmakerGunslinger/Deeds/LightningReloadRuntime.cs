using System;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints.Items;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using KingmakerGunslinger.Actions;
using KingmakerGunslinger.Ammunition;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Reloading;

namespace KingmakerGunslinger.Deeds
{
    internal static class LightningReloadRuntime
    {
        private static readonly LightningReloadService Policy =
            new LightningReloadService();

        internal static LightningReloadAvailability Evaluate(UnitDescriptor caster,
            BlueprintItem blackPowder, BlueprintItem leadBall,
            BlueprintBuff usedMarker)
        {
            ExactEquippedFirearmContext firearm;
            string reason;
            bool exact = ExactEquippedFirearmResolver.TryResolve(caster,
                out firearm, out reason);
            FirearmState state = exact ? firearm.Firearm.Repository.State :
                FirearmState.CreateEmpty();
            int capacity = exact ? firearm.Firearm.Definition.Capacity : 1;
            KingmakerBasicAmmunitionInventory inventory = null;
            bool ammunition = false;
            Game game = Game.Instance;
            if (exact && blackPowder != null && leadBall != null && game != null &&
                game.Player != null && game.Player.Inventory != null)
            {
                inventory = new KingmakerBasicAmmunitionInventory(
                    game.Player.Inventory, blackPowder, leadBall);
                BasicAmmunitionInventorySnapshot snapshot =
                    BasicAmmunitionInventorySnapshot.Capture(inventory);
                ammunition = snapshot.BlackPowderCharges > 0 &&
                    snapshot.LeadBalls > 0;
            }
            bool used = caster != null && usedMarker != null &&
                caster.Buffs.RawFacts.OfType<Buff>().Any(value =>
                    ReferenceEquals(value.Blueprint, usedMarker));
            LightningReloadDecision decision = Policy.Evaluate(
                new LightningReloadRequest(exact, state.Condition,
                    state.LoadedRounds, capacity, ReadGrit(caster), ammunition,
                    used));
            return new LightningReloadAvailability(decision,
                decision.IsAvailable ? firearm : null,
                decision.IsAvailable ? inventory : null);
        }

        internal static FirearmReloadResult Execute(UnitDescriptor caster,
            AbilityExecutionContext abilityContext, BlueprintItem blackPowder,
            BlueprintItem leadBall, BlueprintBuff usedMarker)
        {
            if (caster == null || abilityContext == null || usedMarker == null)
                throw new ArgumentNullException("caster");
            LightningReloadAvailability availability = Evaluate(caster,
                blackPowder, leadBall, usedMarker);
            if (!availability.Decision.IsAvailable)
                throw new InvalidOperationException(
                    "Lightning Reload is unavailable: " +
                    availability.Decision.Status);

            caster.Buffs.AddBuff(usedMarker, abilityContext, null);
            Buff marker = caster.Buffs.RawFacts.OfType<Buff>().SingleOrDefault(
                value => ReferenceEquals(value.Blueprint, usedMarker));
            if (marker == null)
                throw new InvalidOperationException(
                    "Lightning Reload round marker was rejected.");
            try
            {
                var stateStore = new FirearmItemReloadStateStore(
                    FirearmRuntimeState.Service,
                    availability.Firearm.Weapon);
                var rules = new FirearmStateRules(
                    availability.Firearm.Firearm.Definition.Capacity,
                    new[] { availability.Firearm.Firearm.Definition.Reload.Ammunition });
                FirearmReloadResult result = new FirearmReloadTransactionService()
                    .TryReloadOneBasicRound(stateStore, availability.Inventory,
                        rules,
                        availability.Firearm.Firearm.Definition.Reload.Ammunition);
                if (!result.Succeeded)
                {
                    caster.Buffs.RemoveFact(marker);
                    throw new InvalidOperationException(
                        "Lightning Reload became unavailable during delivery: " +
                        result.Status);
                }
                return result;
            }
            catch
            {
                caster.Buffs.RemoveFact(marker);
                throw;
            }
        }

        private static int ReadGrit(UnitDescriptor caster)
        {
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            if (caster == null || gunslinger == null) return 0;
            int current = caster.Resources.GetResourceAmount(gunslinger.Grit.Resource);
            return TrueGritRuntime.Evaluate(caster, TrueGritDeed.LightningReload,
                0, true).Available ? Math.Max(1, current) : current;
        }
    }
}
