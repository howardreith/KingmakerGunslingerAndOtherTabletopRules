using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Items;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Controllers.Units;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Firing;
using KingmakerGunslinger.Misfires;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private static void FirearmReviewResolveImpacts(FirearmInputFixture f)
        {
            f.Turns.Execute(() => {
                new Kingmaker.Controllers.Projectiles.ProjectileHitController().Tick();
                typeof(Kingmaker.Controllers.Units.UnitLifeController)
                    .GetMethod("TickOnUnit", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(new Kingmaker.Controllers.Units.UnitLifeController(), new object[] { f.Enemy });
            });
        }
        private IEnumerable<object> FirearmReviewRetargetCases()
        {
            foreach (string condition in new[] { "normal", "broken", "break-on-kill" })
            {
                string prefix = "cr02-" + condition + "-native-retarget-";
                _firearmInputStage = prefix + "fixture";
                bool degrade = condition == "break-on-kill";
                _firearmInputFixture = new FirearmInputFixture(false, _firearmInputRows,
                    initiallyLoaded: !degrade);
                var f = _firearmInputFixture;
                using (var costs = new FirearmReviewCosts(f.Actor, _firearmInputRows))
                {
                    if (condition == "broken")
                        foreach (object step in FirearmReviewBreak(f, prefix)) yield return step;
                    // Ordinary pistol + matching Rapid Reload + paper cartridges
                    // gives the existing free reload. Gun Training supplies real
                    // damage; the target has one HP and Con1 and is mortal.
                    f.Actor.Descriptor.AddFact(BlueprintBootstrap.GunslingerClass.GunTraining.ChoiceFor(FirearmKind.Pistol));
                    f.Actor.Stats.Dexterity.BaseValue = 30;
                    f.Enemy.Stats.Constitution.BaseValue = 1;
                    f.Enemy.Stats.HitPoints.BaseValue = 1;
                    f.Enemy.Descriptor.State.Immortality.Release();
                    var second = f.SpawnAdditional(false, "NativeRetargetEnemy");
                    var foreignTarget = f.SpawnAdditional(false, "NativeForeignAiEnemy");
                    foreignTarget.Position = new UnityEngine.Vector3(4, 0, 2);
                    var cartridge = BlueprintBootstrap.BasicAmmunition.PaperCartridge;
                    Game.Instance.Player.Inventory.Add(cartridge, 12);
                    f.Actor.Descriptor.ActivatableAbilities.Enumerable.Single(a =>
                        ReferenceEquals(a.Blueprint, BlueprintBootstrap.PaperCartridgeMode.Ability)).IsOn = true;
                    if (condition == "broken")
                    {
                        var manual = f.ClickAbility(BlueprintBootstrap.ReloadTestMusketAbility);
                        for (int tick = 0; tick < 240 && !manual.IsFinished; tick++)
                        { f.Pump(); yield return null; }
                        FirearmInputCheck(prefix + "manual-reload-stays-cancelled", f.State.LoadedRounds == 1 &&
                            f.State.Condition == FirearmCondition.Broken && f.Shots.Count == 1 &&
                            BrokenSequenceSuppressionRuntime.Orders.Current(f.Actor) == null,
                            f.Describe() + ";ordinary paper reload alone never restores the degraded order");
                    }
                    // New combat states start with last-move time zero. Native
                    // idle preparation advances past that three-second limit;
                    // no cooldown, action availability or progress is assigned.
                    for (int tick = 0; tick < 16; tick++)
                    { f.Turns.PumpCommands(false); yield return null; }
                    int shots = f.Shots.Count, cartridges = Game.Instance.Player.Inventory.Count(cartridge);
                    FirearmInputCheck(prefix + "native-full-attack-ready", f.Actor.Commands.Empty &&
                        !f.Actor.CombatState.IsFullAttackRestrictedBecauseOfMoveAction &&
                        shots == (condition == "broken" ? 1 : 0), "Native preparation finished before input.");
                    FirearmMisfireRuntime.QueueForcedNaturalRoll(degrade ? 2 : 10);
                    f.Click(f.Enemy);
                    var attack = f.Actor.Commands.Raw.OfType<UnitAttack>().SingleOrDefault();
                    var order = BrokenSequenceSuppressionRuntime.Orders.Current(f.Actor);
                    // A foreign native AI construction cannot use the accepted
                    // sequence's retarget as permission to replace its owner.
                    var wrongTarget = degrade ? null : f.CreateNativeAiCommand(foreignTarget);
                    for (int tick = 0; tick < 240 && f.Shots.Count < shots + (degrade ? 1 : 2) &&
                        (attack == null || !attack.IsFinished); tick++)
                    {
                        if (!FirearmMisfireRuntime.PendingForcedNaturalRoll.HasValue)
                            FirearmMisfireRuntime.QueueForcedNaturalRoll(degrade ? 2 : 10);
                        f.Pump(); FirearmReviewResolveImpacts(f);
                        if (attack == null) attack = f.LastShotCommand ?? f.Actor.Commands.Raw.OfType<UnitAttack>().SingleOrDefault();
                        f.Record(tick); yield return null;
                    }
                    if (degrade)
                    {
                        // Existing firearm policy makes every applicable misfire
                        // miss, even when its native attack roll would hit. Keep
                        // that balance rule. A separate genuine ordinary-weapon
                        // order kills A after the committed break, so native AI
                        // must not retarget/reload the cancelled firearm order.
                        FirearmInputCheck(prefix + "misfire-resolves-as-miss", f.Shots.Count == shots + 1 &&
                            !f.Shots.Last().AttackRoll.IsHit && f.State.Condition == FirearmCondition.Broken &&
                            f.State.IsEmpty && !BrokenSequenceSuppressionRuntime.Orders.IsCurrent(order),
                            "Natural2 paper misfire consumes the round and preserves the existing mandatory miss.");
                        var ally = f.SpawnAdditional(true, "NativeRetargetAlly");
                        ally.Stats.BaseAttackBonus.BaseValue = 11;
                        ally.Stats.Strength.BaseValue = 30;
                        var sword = ResourcesLibrary.TryGetBlueprint<BlueprintItemWeapon>("57c8994d1f1becf49ac4f642e5d8ca9d");
                        if (sword == null) throw new InvalidOperationException("Native ordinary sword fixture is unavailable.");
                        ally.Body.PrimaryHand.InsertItem(new ItemEntityWeapon(sword));
                        Game.Instance.Player.PartyCharacters.Add(ally);
                        Game.Instance.Player.InvalidateCharacterLists(); Game.Instance.Player.UpdateCharacterLists();
                        f.Turns.Execute(() => Game.Instance.UI.SelectionManager.SelectUnit(ally.View, true, false, false));
                        f.Click(f.Enemy);
                        var allyAttack = ally.Commands.Raw.OfType<UnitAttack>().Single();
                        var actions = new UnitActionController();
                        var cooldowns = new Kingmaker.Controllers.Combat.UnitCombatCooldownsController();
                        var tickCooldown = cooldowns.GetType().GetMethod("TickOnUnit", BindingFlags.Instance | BindingFlags.NonPublic);
                        var tickUnit = typeof(UnitActionController).GetMethod("TickOnUnit", BindingFlags.Instance | BindingFlags.NonPublic);
                        float allyDeadline = UnityEngine.Time.realtimeSinceStartup + 3;
                        for (int tick = 0; (tick < 240 || UnityEngine.Time.realtimeSinceStartup < allyDeadline) &&
                            !f.Enemy.Descriptor.State.IsDead; tick++)
                        {
                            f.Pump();
                            f.Turns.Execute(() => {
                                // Native initiative and action cooldowns advance for this
                                // additional owned actor just as for the two main actors.
                                tickCooldown.Invoke(cooldowns, new object[] { ally });
                                ally.View.AnimationManager.Tick(); ally.View.AnimationManager.Update(0.25f);
                                foreach (var command in ally.Commands.Raw.Where(command => command != null))
                                    if (command.Animation != null) command.Animation.IsActed = true;
                                tickUnit.Invoke(actions, new object[] { ally });
                            });
                            FirearmReviewResolveImpacts(f); yield return null;
                        }
                        _firearmInputRows.Add(new JObject { ["review"] = "CR-02", ["nativeAlly"] = ally.UniqueId,
                            ["started"] = allyAttack.IsStarted, ["acted"] = allyAttack.IsActed,
                            ["finished"] = allyAttack.IsFinished, ["result"] = allyAttack.Result.ToString(),
                            ["isHit"] = allyAttack.LastAttackRule?.AttackRoll.IsHit,
                            ["canAct"] = ally.Descriptor.State.CanAct, ["inCombat"] = ally.IsInCombat,
                            ["prepared"] = ally.CombatState.Prepared, ["initiative"] = ally.CombatState.Cooldown.Initiative,
                            ["canStart"] = allyAttack.CanStart, ["enoughClose"] = allyAttack.IsUnitEnoughClose,
                            ["handsBusy"] = ally.AreHandsBusyWithAnimation,
                            ["handsScheduled"] = Game.Instance.HandsEquipmentController.IsUpdateScheduledFor(ally),
                            ["canActInCombat"] = ally.CombatState.CanActInCombat, ["position"] = ally.Position.ToString(),
                            ["viewPosition"] = ally.View.transform.position.ToString(),
                            ["targetPosition"] = f.Enemy.Position.ToString(),
                            ["targetDamage"] = f.Enemy.Descriptor.Damage, ["targetDead"] = f.Enemy.Descriptor.State.IsDead,
                            ["standardCooldown"] = ally.CombatState.Cooldown.StandardAction,
                            ["handsInCombat"] = ally.View.HandsEquipment.InCombat });
                        FirearmInputCheck(prefix + "other-native-order-kills-a", allyAttack.IsActed &&
                            allyAttack.LastAttackRule != null && allyAttack.LastAttackRule.AttackRoll.IsHit &&
                            ReferenceEquals(allyAttack.LastAttackRule.Target, f.Enemy) && f.Enemy.Descriptor.State.IsDead &&
                            NativeFirearmAttackOrder.Get(allyAttack) == null,
                            "A selected ally's genuine native ordinary-weapon click and damage kill A; no target or death assignment.");
                        f.Turns.Execute(() => Game.Instance.UI.SelectionManager.SelectUnit(f.Actor.View, true, false, false));
                        float until = UnityEngine.Time.realtimeSinceStartup + 1;
                        for (int tick = 0; tick < 40 || UnityEngine.Time.realtimeSinceStartup < until; tick++)
                        { f.Pump(); FirearmReviewResolveImpacts(f); f.Record(tick); yield return null; }
                    }
                    _firearmInputRows.Add(new JObject { ["review"] = "CR-02", ["case"] = prefix,
                        ["nativeTarget"] = attack == null ? null : attack.Target.UniqueId,
                        ["originalTarget"] = f.Enemy.UniqueId, ["replacementTarget"] = second.UniqueId,
                        ["firstTargetDead"] = f.Enemy.Descriptor.State.IsDead,
                        ["fullAttack"] = attack != null && attack.IsFullAttack,
                        ["shots"] = f.Shots.Count, ["damage"] = f.Enemy.Descriptor.Damage,
                        ["hitPoints"] = f.Enemy.Stats.HitPoints.ModifiedValue,
                        ["constitution"] = f.Enemy.Stats.Constitution.ModifiedValue,
                        ["immortal"] = (bool)f.Enemy.Descriptor.State.Immortality,
                        ["hitResults"] = new JArray(f.Shots.Skip(shots).Select(shot => shot.AttackRoll.IsHit)),
                        ["orderCurrent"] = BrokenSequenceSuppressionRuntime.Orders.IsCurrent(order) });
                    if (!degrade) FirearmInputCheck(prefix + "real-shot-kills-a", attack != null && attack.IsFullAttack &&
                        f.Shots.Count > shots && ReferenceEquals(f.Shots[shots].Target, f.Enemy) &&
                        f.Shots[shots].AttackRoll.IsHit && f.Enemy.Descriptor.State.IsDead &&
                        f.Enemy.Descriptor.Damage >= f.Enemy.Stats.HitPoints.ModifiedValue + f.Enemy.Stats.Constitution.ModifiedValue,
                        "Real weapon hit -> native projectile impact -> native life controller; no death or target assignment.");
                    if (degrade)
                        FirearmInputCheck(prefix + "break-stops-despite-kill", f.Shots.Count == shots + 1 &&
                            f.State.Condition == FirearmCondition.Broken && f.State.IsEmpty &&
                            !BrokenSequenceSuppressionRuntime.Orders.IsCurrent(order) &&
                            BrokenSequenceSuppressionRuntime.IsSuppressed(f.Actor, f.Weapon) &&
                            !second.Descriptor.State.IsDead && Game.Instance.Player.Inventory.Count(cartridge) == cartridges - 1,
                            f.Describe() + ";natural2 paper misfire; B remains legal but no stale reload or shot");
                    else
                    {
                        FirearmInputCheck(prefix + "same-sequence-fires", f.Shots.Count == shots + 2 &&
                            ReferenceEquals(attack.Target, second) && ReferenceEquals(f.Shots[shots + 1].Target, second) &&
                            ReferenceEquals(f.LastShotCommand, attack) && ReferenceEquals(order.Owner, attack) &&
                            ReferenceEquals(NativeFirearmAttackOrder.Get(attack)?.Order, order) &&
                            BrokenSequenceSuppressionRuntime.Orders.IsCurrent(order) &&
                            f.State.Condition == (condition == "broken" ? FirearmCondition.Broken : FirearmCondition.Normal) &&
                            Game.Instance.Player.Inventory.Count(cartridge) == cartridges - 1 && costs.Exact(attack, false),
                            f.Describe() + ";native target resolution; same command/progress/order; one free reload and one native attack cost");
                        f.Turns.Execute(() => f.Actor.Commands.Run(wrongTarget));
                        FirearmInputCheck(prefix + "foreign-ai-cannot-borrow", wrongTarget != null && !wrongTarget.IsStarted &&
                            !f.Actor.Commands.Raw.Contains(wrongTarget) && !f.Actor.Commands.Queue.Contains(wrongTarget) &&
                            f.Actor.Commands.Raw.Contains(attack) && BrokenSequenceSuppressionRuntime.Orders.IsCurrent(order) &&
                            ReferenceEquals(order.Owner, attack) && ReferenceEquals(attack.Target, second),
                            "An independently constructed native AI command for C cannot borrow or revoke B's surviving order.");
                    }
                }
                f.Dispose();
                FirearmInputCheck(prefix + "cleanup", f.Restored, "Native fixture restored exactly.");
                _firearmInputFixture = null;
            }
        }
    }
}
