using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> ReviewAstral31Hammer(
            ExpandedSummoningCorrectionFixture fixture)
        {
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool oldPause = Game.Instance.IsPaused;
            UnitEntityData owner = null, sibling = null;
            CreateExpandedSummoningCorrectionHostile(fixture);
            UnitEntityData target = fixture.Hostile;
            var areaBaseline = Game.Instance.State.AreaEffects.All.ToArray();
            var targetBuffBaseline = target.Buffs.Enumerable.ToArray();
            var items = new List<ItemEntity>();
            bool stunImmunity = false, undead = false;
            bool observerSubscribed = false, cleanupRecorded = false;
            var stunnedBlueprint = ExpandedSummoningBlueprints.Registered.BySymbol[
                ExpandedSummoningSpecialBuilder.Astral31Prefix + "Stunned"] as
                    BlueprintBuff;
            var undeadBlueprint = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(
                BlueprintBootstrap.Library, "734a29b693e9ec346ba2951b27987e33",
                "native Undead type for Astral disruption target");
            var observer = new Astral31HammerObserver(target, stunnedBlueprint);
            EventBus.Subscribe(observer);
            observerSubscribed = true;
            try
            {
                if (stunnedBlueprint == null)
                    throw new InvalidOperationException(
                        "Astral private Stunned identity is unavailable.");
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;

                owner = CastExpandedSummoningOwnTier(fixture, "astral-deva");
                sibling = CastExpandedSummoningOwnTier(fixture, "astral-deva");
                foreach (UnitEntityData unit in new[] { owner, sibling })
                {
                    Sprint28SetBrainActivePreservingActions(unit, false);
                    unit.Commands.InterruptAll(true);
                    unit.Commands.RemoveFinishedAndUpdateQueue();
                    items.AddRange(unit.Descriptor.Inventory.Items);
                    var powerAttack = unit.Descriptor.ActivatableAbilities.Enumerable
                        .Single(value => value.Blueprint.AssetGuid ==
                            "a7b339e4f6ff93a4697df5d7a87ff619");
                    foreach (int frame in Sprint22SetPowerAttack(unit, false))
                        yield return frame;
                    if (powerAttack.IsOn || powerAttack.IsRunning ||
                        powerAttack.Blueprint.Buff == null ||
                        unit.Descriptor.Buffs.GetBuff(
                            powerAttack.Blueprint.Buff) != null)
                        throw new InvalidOperationException(
                            "Astral Power Attack off precondition failed.");
                }
                Vector3 origin = fixture.Caster.Position;
                PlaceExpandedSummoningUnit(owner, origin + Vector3.right * 3f);
                PlaceExpandedSummoningUnit(sibling, origin + Vector3.left * 3f);
                PlaceExpandedSummoningUnit(target, origin + Vector3.forward * 2f);
                PlaceExpandedSummoningUnit(fixture.Caster,
                    origin + Vector3.back * 12f);
                target.Descriptor.Stats.HitPoints.BaseValue = 100000;
                target.Descriptor.Stats.AC.BaseValue = -100;
                bool targetCanActBeforeStun = target.Descriptor.State.CanAct;
                if (!targetCanActBeforeStun)
                    throw new InvalidOperationException(
                        "Astral Stunned target was not actable before the attack.");

                ItemEntityWeapon hammer = owner.Body.PrimaryHand.MaybeWeapon;
                ItemEntityWeapon siblingHammer = sibling.Body.PrimaryHand.MaybeWeapon;
                observer.SetOwner(owner);
                UnitAttack hammerCommand = null;
                foreach (int frame in Astral31HammerCommand(owner, target, true,
                    value => hammerCommand = value)) yield return frame;
                int hammerHits = observer.Attacks.Count(value =>
                    value.AttackRoll != null && value.AttackRoll.IsHit);
                AstralDevaStunTracker hammerTracker =
                    AstralDevaHammerStun.ObservedTracker(owner);
                Buff stun = target.Buffs.Enumerable.SingleOrDefault(value =>
                    ReferenceEquals(value.Blueprint, stunnedBlueprint));
                int rolledRounds = observer.DurationRolls.Count == 1 ?
                    observer.DurationRolls[0].Result.Value : -1;
                bool targetCanActDuringStun = target.Descriptor.State.CanAct;
                bool hammerExact = hammer != null &&
                    hammer.Blueprint.AssetGuid ==
                        ExpandedSummoningBlueprints.Registered.BySymbol[
                            ExpandedSummoningSpecialBuilder.Astral31Prefix +
                            "Warhammer1d8"].AssetGuid &&
                    hammer.Enchantments.Count(value => value.Blueprint.AssetGuid ==
                        MagicFirearmBlueprints.Enhancement2Guid) == 1 &&
                    hammer.Enchantments.Count(value => value.Blueprint.AssetGuid ==
                        "0f20d79b7049c0f4ca54ca3d1ea44baa") == 1 &&
                    hammerCommand != null && hammerCommand.IsFinished &&
                    hammerCommand.Result == UnitCommand.ResultType.Success &&
                    hammerCommand.AllAttacks.Count == 3 &&
                    observer.Attacks.Count == 3 && hammerHits == 3 &&
                    observer.FactionIsolationExact(3) &&
                    Astral31PowerAttackOff(owner) &&
                    observer.Attacks.All(value => ReferenceEquals(value.Weapon, hammer) &&
                        value.AttackRoll != null && value.AttackRoll.Roll.Value == 10 &&
                        !value.AttackRoll.AutoHit &&
                        value.AttackRoll.WeaponStats.DamageDescription.First().Bonus == 14) &&
                    observer.Damage.Count == 3 &&
                    observer.Saves.Count == 1 &&
                    observer.Saves[0].StatType == StatType.SaveFortitude &&
                    observer.Saves[0].DifficultyClass == 25 &&
                    observer.Saves[0].D20.Value == 1 &&
                    !observer.Saves[0].IsPassed &&
                    observer.DurationRolls.Count == 1 &&
                    rolledRounds >= 1 && rolledRounds <= 6 &&
                    observer.Applications.Count == 1 &&
                    observer.Applications[0].Duration.HasValue &&
                    Math.Abs(observer.Applications[0].Duration.Value.TotalSeconds -
                        rolledRounds * 6d) <= .001d &&
                    stun != null && target.Descriptor.State.HasCondition(
                        UnitCondition.Stunned) &&
                    targetCanActBeforeStun && !targetCanActDuringStun &&
                    ReferenceEquals(stun.Context?.MaybeCaster, owner) &&
                    stun.TimeLeft.TotalSeconds > 0d &&
                    stun.TimeLeft.TotalSeconds <= rolledRounds * 6d + .001d &&
                    hammerTracker != null &&
                    hammerTracker.HitCount(target.UniqueId) == 2 &&
                    hammerTracker.WasAttempted(target.UniqueId);
                Sprint25Check("astral-hammer-three-hit-stun", hammerExact,
                    new JObject {
                        ["owner"] = owner.UniqueId, ["target"] = target.UniqueId,
                        ["commandFinished"] = hammerCommand?.IsFinished,
                        ["commandResult"] = hammerCommand?.Result.ToString(),
                        ["planned"] = hammerCommand?.AllAttacks.Count,
                        ["allNativeRolls"] = observer.Rolls.Count == 3 &&
                            observer.Rolls.All(value => !value.AutoHit),
                        ["attacks"] = new JArray(observer.Attacks.Select(
                            Astral31HammerAttackRow)),
                        ["damage"] = new JArray(observer.Damage.Select(
                            Sprint16DamageEvent)),
                        ["saves"] = observer.SaveRows(),
                        ["durationD6"] = new JArray(observer.DurationRolls
                            .Select(value => value.Result.Value)),
                        ["applications"] = observer.ApplicationRows(),
                        ["trackerHits"] = hammerTracker?.HitCount(target.UniqueId),
                        ["trackerAttempted"] = hammerTracker?.WasAttempted(
                            target.UniqueId),
                        ["stunCondition"] = target.Descriptor.State.HasCondition(
                            UnitCondition.Stunned),
                        ["victimCanActBeforeStun"] = targetCanActBeforeStun,
                        ["victimCanActDuringStun"] = targetCanActDuringStun,
                        ["stunSource"] = stun?.Context?.MaybeCaster?.UniqueId,
                        ["stunSecondsLeft"] = stun?.TimeLeft.TotalSeconds,
                        ["powerAttack"] = Sprint22PowerAttackState(owner,
                            owner.Descriptor.ActivatableAbilities.Enumerable.Single(
                                value => value.Blueprint.AssetGuid ==
                                    "a7b339e4f6ff93a4697df5d7a87ff619")),
                        ["factionIsolation"] = observer.FactionRow(),
                        ["forcedAttackRoll"] = 10, ["forcedSaveRoll"] = 1,
                        ["autoHit"] = false },
                    "One native full-attack command resolves three +2 disrupting hammer hits; only the second same-opponent hit asks for one live-DC Fortitude save, the third cannot repeat it, and failure owns one native d6 Stunned effect.");
                if (!hammerExact) throw new InvalidOperationException(
                    "Astral native hammer/stun command failed.");

                double beforeSourceLoss = stun.TimeLeft.TotalSeconds;
                items.AddRange(owner.Descriptor.Inventory.Items.Where(value =>
                    !items.Contains(value)));
                if (owner.IsInCombat) owner.LeaveCombat();
                DisposeExpandedSummoningUnits(fixture.Created, new[] { owner });
                for (int frame = 0; frame < 4; frame++) yield return 0;
                bool survivedSource = target.Buffs.Enumerable.Contains(stun) &&
                    target.Descriptor.State.HasCondition(UnitCondition.Stunned) &&
                    stun.TimeLeft.TotalSeconds > 0d &&
                    stun.TimeLeft.TotalSeconds <= beforeSourceLoss;
                double expiryStart = Game.Instance.TimeController.GameTime.TotalSeconds;
                int expiryFrames = 0;
                DateTime expiryDeadline = DateTime.UtcNow.AddSeconds(45);
                while (DateTime.UtcNow < expiryDeadline && expiryFrames++ < 3600 &&
                    (target.Buffs.Enumerable.Contains(stun) ||
                        target.Descriptor.State.HasCondition(UnitCondition.Stunned) ||
                        target.Descriptor.State.CanAct != targetCanActBeforeStun))
                    yield return 0;
                double expiryElapsed = Game.Instance.TimeController.GameTime.TotalSeconds -
                    expiryStart;
                bool targetCanActAfterExpiry = target.Descriptor.State.CanAct;
                bool actionStateRestored = targetCanActAfterExpiry ==
                    targetCanActBeforeStun;
                bool expired = !target.Buffs.Enumerable.Contains(stun) &&
                    !target.Descriptor.State.HasCondition(UnitCondition.Stunned) &&
                    actionStateRestored;
                Sprint25Check("astral-hammer-source-loss-and-expiry",
                    survivedSource && expired, new JObject {
                        ["sourceAbsent"] = !owner.IsInState,
                        ["survivedSource"] = survivedSource,
                        ["secondsBeforeSourceLoss"] = beforeSourceLoss,
                        ["nativeDurationRounds"] = rolledRounds,
                        ["expiryFrames"] = expiryFrames,
                        ["nativeElapsedSeconds"] = expiryElapsed,
                        ["effectGone"] = !target.Buffs.Enumerable.Contains(stun),
                        ["conditionGone"] = !target.Descriptor.State.HasCondition(
                            UnitCondition.Stunned),
                        ["victimCanActBeforeStun"] = targetCanActBeforeStun,
                        ["victimCanActDuringStun"] = targetCanActDuringStun,
                        ["victimCanActAfterExpiry"] = targetCanActAfterExpiry,
                        ["actionStateRestored"] = actionStateRestored },
                    "The finite victim effect survives its summoner's removal, expires on its own native clock, and returns the victim's action state.");
                if (!survivedSource || !expired)
                    throw new InvalidOperationException(
                        "Astral Stunned source-loss/lifetime contract failed.");

                observer.SetOwner(sibling);
                sibling.CombatState.Cooldown.StandardAction = 0;
                sibling.CombatState.Cooldown.MoveAction = 0;
                foreach (int frame in Astral31SwitchWeapon(sibling, 1))
                    yield return frame;
                observer.Clear();
                UnitAttack slamCommand = null;
                foreach (int frame in Astral31HammerCommand(sibling, target, false,
                    value => slamCommand = value)) yield return frame;
                ItemEntityWeapon slam = sibling.Body.PrimaryHand.MaybeWeapon;
                bool slamExact = slam != null && slamCommand != null &&
                    slamCommand.IsFinished &&
                    slamCommand.Result == UnitCommand.ResultType.Success &&
                    observer.Attacks.Count == 1 &&
                    observer.FactionIsolationExact(1) &&
                    Astral31PowerAttackOff(sibling) &&
                    observer.Attacks[0].AttackRoll != null &&
                    observer.Attacks[0].AttackRoll.IsHit &&
                    !observer.Attacks[0].AttackRoll.AutoHit &&
                    ReferenceEquals(observer.Attacks[0].Weapon, slam) &&
                    observer.Attacks[0].AttackRoll.WeaponStats.DamageDescription
                        .First().Bonus == 12 && observer.Damage.Count == 1 &&
                    observer.Saves.Count == 0 &&
                    observer.DurationRolls.Count == 0 &&
                    observer.Applications.Count == 0 &&
                    AstralDevaHammerStun.ObservedTracker(sibling) == null;
                Sprint25Check("astral-hammer-alternative-slam", slamExact,
                    new JObject {
                        ["owner"] = sibling.UniqueId,
                        ["target"] = target.UniqueId,
                        ["commandFinished"] = slamCommand?.IsFinished,
                        ["attack"] = observer.Attacks.Count == 0 ? null :
                            Astral31HammerAttackRow(observer.Attacks[0]),
                        ["damage"] = new JArray(observer.Damage.Select(
                            Sprint16DamageEvent)),
                        ["powerAttack"] = Sprint22PowerAttackState(sibling,
                            sibling.Descriptor.ActivatableAbilities.Enumerable
                                .Single(value => value.Blueprint.AssetGuid ==
                                    "a7b339e4f6ff93a4697df5d7a87ff619")),
                        ["factionIsolation"] = observer.FactionRow(),
                        ["stunSaves"] = observer.Saves.Count,
                        ["stunTrackerAbsent"] =
                            AstralDevaHammerStun.ObservedTracker(sibling) == null },
                    "One native standard attack resolves the alternative primary slam with its signed live damage and cannot enter the hammer-only stun tracker.");
                if (!slamExact) throw new InvalidOperationException(
                    "Astral alternative slam resolution failed.");

                sibling.CombatState.Cooldown.StandardAction = 0;
                sibling.CombatState.Cooldown.MoveAction = 0;
                foreach (int frame in Astral31SwitchWeapon(sibling, 0))
                    yield return frame;
                sibling.Descriptor.Stats.Strength.BaseValue += 2;
                target.Descriptor.State.AddConditionImmunity(UnitCondition.Stunned);
                stunImmunity = true;
                observer.Clear();
                UnitAttack immunityCommand = null;
                foreach (int frame in Astral31HammerCommand(sibling, target, true,
                    value => immunityCommand = value)) yield return frame;
                AstralDevaStunTracker immunityTracker =
                    AstralDevaHammerStun.ObservedTracker(sibling);
                int immunityRounds = observer.DurationRolls.Count == 1 ?
                    observer.DurationRolls[0].Result.Value : -1;
                Buff immunityBuff = target.Buffs.Enumerable.SingleOrDefault(
                    value => ReferenceEquals(value.Blueprint, stunnedBlueprint));
                bool immunityConditionPresent = target.Descriptor.State
                    .HasCondition(UnitCondition.Stunned);
                bool targetCanActWithImmunity = target.Descriptor.State.CanAct;
                bool immunityApplicationExact = observer.Applications.Count == 1 &&
                    observer.Applications[0].Duration.HasValue &&
                    Math.Abs(observer.Applications[0].Duration.Value.TotalSeconds -
                        immunityRounds * 6d) <= .001d &&
                    ReferenceEquals(observer.Applications[0].AppliedBuff,
                        immunityBuff) &&
                    ReferenceEquals(immunityBuff?.Context?.MaybeCaster, sibling);
                target.Descriptor.State.RemoveConditionImmunity(
                    UnitCondition.Stunned);
                stunImmunity = false;
                immunityBuff?.Remove();
                for (int frame = 0; frame < 2; frame++) yield return 0;
                bool immunityReset = !target.Descriptor.State.HasCondition(
                        UnitCondition.Stunned) &&
                    !target.Buffs.Enumerable.Any(value => ReferenceEquals(
                        value.Blueprint, stunnedBlueprint)) &&
                    target.Descriptor.State.CanAct == targetCanActBeforeStun;
                bool immunityExact = immunityCommand != null &&
                    immunityCommand.IsFinished &&
                    immunityCommand.Result == UnitCommand.ResultType.Success &&
                    observer.Attacks.Count == 3 &&
                    observer.FactionIsolationExact(3) &&
                    Astral31PowerAttackOff(sibling) &&
                    observer.Attacks.All(value => value.AttackRoll != null &&
                        value.AttackRoll.IsHit && !value.AttackRoll.AutoHit &&
                        ReferenceEquals(value.Weapon,
                            sibling.Body.PrimaryHand.MaybeWeapon)) &&
                    observer.Saves.Count == 1 &&
                    observer.Saves[0].StatType == StatType.SaveFortitude &&
                    observer.Saves[0].DifficultyClass == 26 &&
                    observer.Saves[0].D20.Value == 1 &&
                    !observer.Saves[0].IsPassed &&
                    observer.DurationRolls.Count == 1 &&
                    immunityRounds >= 1 && immunityRounds <= 6 &&
                    immunityApplicationExact && immunityBuff != null &&
                    !immunityConditionPresent && targetCanActWithImmunity &&
                    immunityReset &&
                    immunityTracker != null &&
                    immunityTracker.HitCount(target.UniqueId) == 2 &&
                    immunityTracker.WasAttempted(target.UniqueId);
                Sprint25Check("astral-hammer-native-stun-immunity", immunityExact,
                    new JObject {
                        ["owner"] = sibling.UniqueId,
                        ["target"] = target.UniqueId,
                        ["liveStrength"] = sibling.Stats.Strength.ModifiedValue,
                        ["powerAttack"] = Sprint22PowerAttackState(sibling,
                            sibling.Descriptor.ActivatableAbilities.Enumerable
                                .Single(value => value.Blueprint.AssetGuid ==
                                    "a7b339e4f6ff93a4697df5d7a87ff619")),
                        ["factionIsolation"] = observer.FactionRow(),
                        ["saves"] = observer.SaveRows(),
                        ["durationD6"] = new JArray(observer.DurationRolls
                            .Select(value => value.Result.Value)),
                        ["applications"] = observer.ApplicationRows(),
                        ["conditionPresentWithImmunity"] =
                            immunityConditionPresent,
                        ["privateBuffPresentWithImmunity"] =
                            immunityBuff != null,
                        ["victimCanActWithImmunity"] =
                            targetCanActWithImmunity,
                        ["immunityApplicationExact"] =
                            immunityApplicationExact,
                        ["ownedResetExact"] = immunityReset,
                        ["trackerHits"] = immunityTracker?.HitCount(
                            target.UniqueId),
                        ["trackerAttempted"] = immunityTracker?.WasAttempted(
                            target.UniqueId) },
                    "The second hammer hit uses live Strength for DC26; native Stunned-condition immunity blocks the condition while retaining one inert finite source-owned buff, grants no extra attempt that round, and resets only that owned effect before the next case.");
                if (!immunityExact) throw new InvalidOperationException(
                    "Astral native stun-immunity contract failed.");

                sibling.CombatState.Cooldown.StandardAction = 0;
                sibling.CombatState.Cooldown.MoveAction = 0;
                if (target.Descriptor.AddFact(undeadBlueprint) == null ||
                    !target.Descriptor.IsUndead)
                    throw new InvalidOperationException(
                        "Disposable disruption target did not become native Undead.");
                undead = true;
                observer.Clear();
                ItemEntityWeapon disruptingHammer = sibling.Body.PrimaryHand.MaybeWeapon;
                RuleAttackWithWeapon disruption = Rulebook.Trigger(
                    new RuleAttackWithWeapon(sibling, target, disruptingHammer, 0));
                for (int frame = 0; frame < 4; frame++) yield return 0;
                RuleSavingThrow disruptionSave = observer.Saves.SingleOrDefault();
                bool disruptionExact = disruption.AttackRoll != null &&
                    observer.FactionIsolationExact(1) &&
                    Astral31PowerAttackOff(sibling) &&
                    disruption.AttackRoll.IsHit &&
                    !disruption.AttackRoll.AutoHit &&
                    disruption.AttackRoll.Roll.Value == 10 &&
                    ReferenceEquals(disruption.Weapon, disruptingHammer) &&
                    disruptingHammer.Enchantments.Any(value =>
                        value.Blueprint.AssetGuid ==
                            "0f20d79b7049c0f4ca54ca3d1ea44baa") &&
                    disruptionSave != null &&
                    disruptionSave.StatType == StatType.SaveWill &&
                    disruptionSave.DifficultyClass == 14 &&
                    disruptionSave.D20.Value == 1 &&
                    !disruptionSave.IsPassed &&
                    (target.Descriptor.State.IsDead || target.Destroyed) &&
                    observer.DurationRolls.Count == 0 &&
                    !target.Buffs.Enumerable.Any(value => ReferenceEquals(
                        value.Blueprint, stunnedBlueprint));
                Sprint25Check("astral-hammer-native-disruption", disruptionExact,
                    new JObject {
                        ["owner"] = sibling.UniqueId,
                        ["target"] = target.UniqueId,
                        ["targetUndead"] = target.Descriptor.IsUndead,
                        ["attack"] = Astral31HammerAttackRow(disruption),
                        ["powerAttack"] = Sprint22PowerAttackState(sibling,
                            sibling.Descriptor.ActivatableAbilities.Enumerable
                                .Single(value => value.Blueprint.AssetGuid ==
                                    "a7b339e4f6ff93a4697df5d7a87ff619")),
                        ["factionIsolation"] = observer.FactionRow(),
                        ["saves"] = observer.SaveRows(),
                        ["damage"] = new JArray(observer.Damage.Select(
                            Sprint16DamageEvent)),
                        ["targetDead"] = target.Descriptor.State.IsDead,
                        ["targetDestroyed"] = target.Destroyed,
                        ["stunDurationRolls"] = observer.DurationRolls.Count,
                        ["stunBuffPresent"] = target.Buffs.Enumerable.Any(value =>
                            ReferenceEquals(value.Blueprint, stunnedBlueprint)) },
                    "One actual hit from the exact private disrupting hammer makes the native DC14 Will save against a disposable native Undead target and destroys it on failure; the already-consumed same-round stun gate does not fire again.");
                if (!disruptionExact) throw new InvalidOperationException(
                    "Astral native disruption contract failed.");

                observer.RestoreFaction();
                EventBus.Unsubscribe(observer);
                observerSubscribed = false;
                foreach (UnitEntityData unit in new[] { owner, sibling }.Where(
                    value => value != null))
                {
                    AstralDevaHammerStun.ForgetObserved(unit);
                    if (!unit.Destroyed)
                    {
                        Sprint28SetBrainActivePreservingActions(unit, false);
                        unit.Commands.InterruptAll(true);
                        if (unit.IsInCombat) unit.LeaveCombat();
                        if (target.Memory.Contains(unit)) target.Memory.Remove(unit);
                        if (unit.Memory.Contains(target)) unit.Memory.Remove(target);
                    }
                }
                DisposeExpandedSummoningUnits(fixture.Created,
                    new[] { owner, sibling }.Where(value => value != null &&
                        !value.Destroyed).ToArray());
                if (!target.Destroyed) target.Destroy();
                Game.Instance.EntityDestroyer.Tick();
                int cleanupFrames = 0;
                DateTime cleanupDeadline = DateTime.UtcNow.AddSeconds(5);
                while (DateTime.UtcNow < cleanupDeadline && cleanupFrames++ < 120 &&
                    Game.Instance.State.AreaEffects.All.Except(areaBaseline).Any())
                    yield return 0;
                Game.Instance.EntityDestroyer.Tick();
                bool cleanup = owner.Destroyed && sibling.Destroyed &&
                    target.Destroyed &&
                    !Game.Instance.State.AreaEffects.All.Except(areaBaseline).Any() &&
                    items.Count >= 4 && items.All(value =>
                        !value.IsLootable && value.Collection == null) &&
                    !target.Buffs.Enumerable.Any(value =>
                        !targetBuffBaseline.Contains(value) &&
                        ReferenceEquals(value.Blueprint, stunnedBlueprint));
                Sprint25Check("astral-hammer-owned-cleanup", cleanup,
                    new JObject {
                        ["ownerAbsent"] = !owner.IsInState,
                        ["siblingAbsent"] = !sibling.IsInState,
                        ["targetRetired"] = target.Destroyed,
                        ["areasExact"] = !Game.Instance.State.AreaEffects.All
                            .Except(areaBaseline).Any(),
                        ["cleanupFrames"] = cleanupFrames,
                        ["items"] = new JArray(items.Select(value => new JObject {
                            ["blueprint"] = value.Blueprint.AssetGuid,
                            ["lootable"] = value.IsLootable,
                            ["collectionAbsent"] = value.Collection == null })),
                        ["freshProcessBoundaryRequired"] = true,
                        ["phaseEndsAfterCleanup"] = true },
                    "Both disposable Astrals, the exact private items, emitters and hostile are absent; this combat phase always proceeds directly to fixture/profile restoration in its fresh process.");
                if (!cleanup) throw new InvalidOperationException(
                    "Astral hammer owned cleanup failed.");
                cleanupRecorded = true;
            }
            finally
            {
                observer.RestoreFaction();
                if (observerSubscribed) EventBus.Unsubscribe(observer);
                if (stunImmunity && target != null && !target.Destroyed)
                    target.Descriptor.State.RemoveConditionImmunity(
                        UnitCondition.Stunned);
                if (undead && target != null && !target.Destroyed)
                    target.Descriptor.RemoveFact(undeadBlueprint);
                foreach (UnitEntityData unit in new[] { owner, sibling }.Where(
                    value => value != null))
                {
                    AstralDevaHammerStun.ForgetObserved(unit);
                    if (!unit.Destroyed)
                    {
                        Sprint28SetBrainActivePreservingActions(unit, false);
                        unit.Commands.InterruptAll(true);
                        if (unit.IsInCombat) unit.LeaveCombat();
                        if (target != null && target.Memory.Contains(unit))
                            target.Memory.Remove(unit);
                        if (target != null && unit.Memory.Contains(target))
                            unit.Memory.Remove(target);
                    }
                }
                if (!cleanupRecorded)
                {
                    DisposeExpandedSummoningUnits(fixture.Created,
                        new[] { owner, sibling }.Where(value => value != null &&
                            !value.Destroyed).ToArray());
                    if (target != null && !target.Destroyed)
                    {
                        foreach (Buff buff in target.Buffs.Enumerable.Where(value =>
                            ReferenceEquals(value.Blueprint,
                                stunnedBlueprint)).ToArray())
                            buff.Remove();
                        if (target.IsInCombat) target.LeaveCombat();
                        ResetExpandedSummoningHostile(fixture);
                        target.Destroy();
                    }
                    Game.Instance.EntityDestroyer.Tick();
                }
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = oldPause;
            }
        }

        private static bool Astral31PowerAttackOff(UnitEntityData owner)
        {
            var toggle = owner.Descriptor.ActivatableAbilities.Enumerable.Single(
                value => value.Blueprint.AssetGuid ==
                    "a7b339e4f6ff93a4697df5d7a87ff619");
            return !toggle.IsOn && !toggle.IsRunning &&
                toggle.Blueprint.Buff != null &&
                owner.Descriptor.Buffs.GetBuff(toggle.Blueprint.Buff) == null;
        }

        private IEnumerable<int> Astral31HammerCommand(UnitEntityData owner,
            UnitEntityData target, bool full, Action<UnitAttack> complete)
        {
            var command = new UnitAttack(target) {
                ForceFullAttack = full, IsSingleAttack = !full };
            owner.Commands.Run(command);
            int frames = 0;
            DateTime deadline = DateTime.UtcNow.AddSeconds(45);
            while (!command.IsFinished && frames++ < 4000 &&
                DateTime.UtcNow < deadline) yield return 0;
            complete(command);
        }

        private IEnumerable<int> Astral31SwitchWeapon(UnitEntityData owner,
            int index)
        {
            if (owner.Body.CurrentHandEquipmentSetIndex == index) yield break;
            var command = new UnitSwitchHandEquipmentSet(index);
            owner.Commands.Run(command);
            int frames = 0;
            DateTime deadline = DateTime.UtcNow.AddSeconds(20);
            while (!command.IsFinished && frames++ < 1000 &&
                DateTime.UtcNow < deadline) yield return 0;
            if (!command.IsFinished ||
                owner.Body.CurrentHandEquipmentSetIndex != index)
                throw new InvalidOperationException(
                    "Astral native weapon-set command failed in hammer review.");
        }

        private static JObject Astral31HammerAttackRow(
            RuleAttackWithWeapon value)
        {
            return new JObject {
                ["weapon"] = value?.Weapon?.Blueprint?.AssetGuid,
                ["target"] = value?.Target?.UniqueId,
                ["hit"] = value?.AttackRoll?.IsHit,
                ["autoHit"] = value?.AttackRoll?.AutoHit,
                ["naturalRoll"] = value?.AttackRoll?.Roll.Value,
                ["attackBonus"] = value?.AttackRoll?.AttackBonus,
                ["attackPenalty"] = value?.AttackRoll?.AttackBonusPenalty,
                ["damageBonus"] = value?.AttackRoll?.WeaponStats?
                    .DamageDescription.FirstOrDefault()?.Bonus,
                ["critical"] = value?.AttackRoll?.IsCriticalConfirmed };
        }

        private sealed class Astral31HammerObserver :
            IGlobalRulebookHandler<RuleAttackWithWeapon>,
            IGlobalRulebookHandler<RuleCalculateWeaponStats>,
            IGlobalRulebookHandler<RuleAttackRoll>,
            IGlobalRulebookHandler<RuleDealDamage>,
            IGlobalRulebookHandler<RuleSavingThrow>,
            IGlobalRulebookHandler<RuleRollDice>,
            IGlobalRulebookHandler<RuleApplyBuff>
        {
            private readonly UnitEntityData _target;
            private readonly BlueprintBuff _stunned;
            internal UnitEntityData Owner;
            private BlueprintFaction _originalFaction;
            private BlueprintFaction _playerFaction;
            internal int FactionSwitches;
            internal int FactionRestorations;
            internal bool AllRollsPlayerFaction;
            internal readonly List<RuleAttackWithWeapon> Attacks =
                new List<RuleAttackWithWeapon>();
            internal readonly List<RuleAttackRoll> Rolls =
                new List<RuleAttackRoll>();
            internal readonly List<RuleDealDamage> Damage =
                new List<RuleDealDamage>();
            internal readonly List<RuleSavingThrow> Saves =
                new List<RuleSavingThrow>();
            internal readonly List<RuleRollDice> DurationRolls =
                new List<RuleRollDice>();
            internal readonly List<RuleApplyBuff> Applications =
                new List<RuleApplyBuff>();

            internal Astral31HammerObserver(UnitEntityData target,
                BlueprintBuff stunned)
            { _target = target; _stunned = stunned; }

            internal void SetOwner(UnitEntityData owner)
            {
                RestoreFaction();
                Owner = owner;
                _originalFaction = owner?.Descriptor?.Faction;
                _playerFaction = BlueprintRoot.Instance.PlayerFaction;
                Clear();
            }

            internal void Clear()
            {
                Attacks.Clear(); Rolls.Clear(); Damage.Clear(); Saves.Clear();
                DurationRolls.Clear(); Applications.Clear();
                FactionSwitches = 0;
                FactionRestorations = 0;
                AllRollsPlayerFaction = true;
            }

            internal void RestoreFaction()
            {
                if (Owner != null && _originalFaction != null &&
                    ReferenceEquals(Owner.Descriptor.Faction, _playerFaction))
                    Owner.Descriptor.Faction = _originalFaction;
            }

            internal bool FactionIsolationExact(int expectedRolls)
            {
                return expectedRolls > 0 &&
                    FactionSwitches == expectedRolls &&
                    FactionRestorations == expectedRolls &&
                    AllRollsPlayerFaction &&
                    Owner != null &&
                    ReferenceEquals(Owner.Descriptor.Faction, _originalFaction);
            }

            internal JObject FactionRow()
            {
                return new JObject {
                    ["switches"] = FactionSwitches,
                    ["restorations"] = FactionRestorations,
                    ["allRollsPlayerFaction"] = AllRollsPlayerFaction,
                    ["restored"] = Owner != null && ReferenceEquals(
                        Owner.Descriptor.Faction, _originalFaction),
                    ["original"] = _originalFaction?.AssetGuid,
                    ["player"] = _playerFaction?.AssetGuid,
                    ["current"] = Owner?.Descriptor?.Faction?.AssetGuid };
            }

            private bool IsObservedAttack(RuleAttackWithWeapon evt)
            {
                return evt != null && ReferenceEquals(evt.Initiator, Owner) &&
                    ReferenceEquals(evt.Target, _target);
            }

            public void OnEventAboutToTrigger(RuleCalculateWeaponStats evt) { }
            public void OnEventDidTrigger(RuleCalculateWeaponStats evt)
            {
                RuleAttackWithWeapon attack = evt?.AttackWithWeapon;
                if (!IsObservedAttack(attack) || Owner == null ||
                    _originalFaction == null || _playerFaction == null ||
                    !ReferenceEquals(Owner.Descriptor.Faction,
                        _originalFaction)) return;
                // The isolated fixture's request-owned summons are non-player
                // faction. Switch only after native weapon stats are final so
                // the installed non-player PowerAttackWatcher honors the
                // explicit off toggle for the child attack roll. Restore as
                // soon as that roll completes.
                Owner.Descriptor.Faction = _playerFaction;
                FactionSwitches++;
            }

            public void OnEventAboutToTrigger(RuleAttackWithWeapon evt) { }
            public void OnEventDidTrigger(RuleAttackWithWeapon evt)
            {
                if (ReferenceEquals(evt.Initiator, Owner) &&
                    ReferenceEquals(evt.Target, _target)) Attacks.Add(evt);
            }
            public void OnEventAboutToTrigger(RuleAttackRoll evt)
            {
                if (ReferenceEquals(evt.Initiator, Owner) &&
                    ReferenceEquals(evt.Target, _target))
                {
                    AllRollsPlayerFaction &= Owner.IsPlayerFaction;
                    UnityEngine.Random.InitState(FindNativeD20Seed(10));
                }
            }
            public void OnEventDidTrigger(RuleAttackRoll evt)
            {
                if (ReferenceEquals(evt.Initiator, Owner) &&
                    ReferenceEquals(evt.Target, _target))
                {
                    Rolls.Add(evt);
                    if (ReferenceEquals(Owner.Descriptor.Faction,
                        _playerFaction))
                    {
                        Owner.Descriptor.Faction = _originalFaction;
                        FactionRestorations++;
                    }
                }
            }
            public void OnEventAboutToTrigger(RuleDealDamage evt) { }
            public void OnEventDidTrigger(RuleDealDamage evt)
            {
                if (ReferenceEquals(evt.Initiator, Owner) &&
                    ReferenceEquals(evt.Target, _target)) Damage.Add(evt);
            }
            public void OnEventAboutToTrigger(RuleSavingThrow evt)
            {
                if (ReferenceEquals(evt.Initiator, _target))
                    UnityEngine.Random.InitState(FindNativeD20Seed(1));
            }
            public void OnEventDidTrigger(RuleSavingThrow evt)
            {
                if (ReferenceEquals(evt.Initiator, _target)) Saves.Add(evt);
            }
            public void OnEventAboutToTrigger(RuleRollDice evt) { }
            public void OnEventDidTrigger(RuleRollDice evt)
            {
                if (ReferenceEquals(evt.Initiator, Owner) &&
                    evt.DiceFormula.Rolls == 1 &&
                    evt.DiceFormula.Dice == DiceType.D6)
                    DurationRolls.Add(evt);
            }
            public void OnEventAboutToTrigger(RuleApplyBuff evt) { }
            public void OnEventDidTrigger(RuleApplyBuff evt)
            {
                if (ReferenceEquals(evt.Initiator, _target) &&
                    ReferenceEquals(evt.Blueprint, _stunned)) Applications.Add(evt);
            }

            internal JArray SaveRows()
            {
                return new JArray(Saves.Select(value => new JObject {
                    ["type"] = value.StatType.ToString(),
                    ["dc"] = value.DifficultyClass,
                    ["d20"] = value.D20.Value,
                    ["passed"] = value.IsPassed,
                    ["reason"] = value.Reason == null ? null :
                        value.Reason.ToString() }));
            }

            internal JArray ApplicationRows()
            {
                return new JArray(Applications.Select(value => new JObject {
                    ["buff"] = value.Blueprint?.AssetGuid,
                    ["durationSeconds"] = value.Duration?.TotalSeconds,
                    ["applied"] = value.AppliedBuff != null,
                    ["source"] = value.AppliedBuff?.Context?.MaybeCaster?.UniqueId }));
            }
        }
    }
}
