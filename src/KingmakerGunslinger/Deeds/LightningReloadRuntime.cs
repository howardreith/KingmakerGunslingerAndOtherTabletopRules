using System;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints.Items;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic;
using Kingmaker.EntitySystem.Entities;
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
            KingmakerReloadAmmunitionInventory inventory = null;
            FirearmReloadPlan reloadPlan = null;
            bool ammunition = false;
            Game game = Game.Instance;
            if (exact && blackPowder != null && leadBall != null && game != null &&
                game.Player != null && game.Player.Inventory != null)
            {
                inventory = new KingmakerReloadAmmunitionInventory(
                    game.Player.Inventory, blackPowder, leadBall,
                    BlueprintBootstrap.BasicAmmunition.PaperCartridge);
                ReloadTestMusketAvailability normal = ReloadTestMusketRuntime.Evaluate(
                    caster, firearm.Weapon.Blueprint, blackPowder, leadBall);
                reloadPlan = normal.Plan;
                ammunition = normal.IsAvailable;
            }
            bool used = caster != null && usedMarker != null &&
                caster.Buffs.RawFacts.OfType<Buff>().Any(value =>
                    ReferenceEquals(value.Blueprint, usedMarker));
            LightningReloadDecision decision = Policy.Evaluate(
                new LightningReloadRequest(exact,
                    exact ? firearm.EffectiveCondition : state.Condition,
                    state.LoadedRounds, capacity, ReadGrit(caster), ammunition,
                    used, ResolveAction(caster, exact ? firearm : null,
                        reloadPlan)));
            return new LightningReloadAvailability(decision,
                decision.IsAvailable ? firearm : null,
                decision.IsAvailable ? inventory : null,
                decision.IsAvailable ? reloadPlan : null);
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
                FirearmStateRules rules = FirearmStateRules.CreateForDefinition(
                    availability.Firearm.Firearm.Definition);
                FirearmReloadResult result = new FirearmReloadTransactionService()
                    .TryReloadRounds(stateStore, availability.Inventory, rules,
                        availability.ReloadPlan.Profile, 1);
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

        internal static FirearmReloadResult ExecuteInline(UnitEntityData caster,
            BlueprintItem blackPowder, BlueprintItem leadBall,
            BlueprintBuff usedMarker)
        {
            if (caster == null || caster.Descriptor == null)
                throw new ArgumentNullException("caster");
            BlueprintAbility blueprint = BlueprintBootstrap.GunslingerClass == null
                ? null : BlueprintBootstrap.GunslingerClass.LightningReload.Ability;
            Kingmaker.UnitLogic.Abilities.Ability granted = blueprint == null
                ? null : caster.Descriptor.Abilities.GetAbility(blueprint);
            if (granted == null)
                throw new InvalidOperationException(
                    "Lightning Reload is not granted to the exact unit.");
            var data = new AbilityData(granted);
            var context = new AbilityExecutionContext(data, new AbilityParams(),
                new Kingmaker.Utility.TargetWrapper(caster), null);
            LightningReloadAvailability availability = Evaluate(caster.Descriptor,
                blackPowder, leadBall, usedMarker);
            if (!availability.Decision.IsAvailable ||
                availability.Decision.Action != LightningReloadAction.Free)
                throw new InvalidOperationException(
                    "Inline Lightning Reload is not currently legal and Free.");
            return Execute(caster.Descriptor, context, blackPowder, leadBall,
                usedMarker);
        }

        private static int ReadGrit(UnitDescriptor caster)
        {
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            if (caster == null || gunslinger == null) return 0;
            int current = caster.Resources.GetResourceAmount(gunslinger.Grit.Resource);
            return TrueGritRuntime.Evaluate(caster, TrueGritDeed.LightningReload,
                0, true).Available ? Math.Max(1, current) : current;
        }

        private static LightningReloadAction ResolveAction(UnitDescriptor caster,
            ExactEquippedFirearmContext firearm, FirearmReloadPlan plan)
        {
            if (plan != null && plan.Profile.SourceKind ==
                ReloadAmmunitionSourceKind.PaperCartridge)
                return LightningReloadAction.Free;
            return firearm != null && RapidReloadRuntime.HasMatchingChoice(caster,
                firearm.Firearm.Definition.Kind)
                ? LightningReloadAction.Free : LightningReloadAction.Swift;
        }
    }
}
