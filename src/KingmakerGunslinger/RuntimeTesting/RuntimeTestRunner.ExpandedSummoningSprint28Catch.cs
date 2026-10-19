using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private sealed class Sprint28CatchSaves :
            IGlobalRulebookHandler<RuleSavingThrow>, IDisposable
        {
            private readonly UnitEntityData _target;
            internal readonly List<RuleSavingThrow> Saves =
                new List<RuleSavingThrow>();

            internal Sprint28CatchSaves(UnitEntityData target)
            {
                _target = target;
                EventBus.Subscribe(this);
            }

            public void OnEventAboutToTrigger(RuleSavingThrow evt) { }

            public void OnEventDidTrigger(RuleSavingThrow evt)
            {
                if (ReferenceEquals(evt.Initiator, _target))
                    Saves.Add(evt);
            }

            public void Dispose() { EventBus.Unsubscribe(this); }
        }

        private static void CaptureSprint28CatchProjectiles(
            IList<Projectile> owned, Projectile[] prior,
            UnitEntityData thrower)
        {
            foreach (Projectile projectile in Game.Instance.ProjectileController
                .Projectiles.Where(value => value != null &&
                    !prior.Contains(value) &&
                    ReferenceEquals(value.Launcher, thrower) &&
                    !owned.Contains(value)))
                owned.Add(projectile);
        }

        private static JObject DescribeSprint28CatchProjectile(
            Projectile projectile)
        {
            return new JObject {
                ["blueprint"] = projectile.Blueprint == null ? null :
                    projectile.Blueprint.name,
                ["launcher"] = projectile.Launcher == null ? null :
                    projectile.Launcher.UniqueId,
                ["target"] = projectile.Target?.Unit?.UniqueId,
                ["cleared"] = projectile.Cleared,
                ["destroyed"] = projectile.Destroyed,
                ["isHit"] = projectile.IsHit,
                ["result"] = projectile.AttackResult.ToString(),
                ["rollResult"] = projectile.AttackRoll?.Result.ToString(),
                ["speed"] = projectile.Blueprint?.Speed,
                ["lifetimeAfterHit"] =
                    projectile.Blueprint?.LifetimeParticlesAfterHit,
                ["passedTime"] = ReadExpandedSummoningOptionalMember(
                    projectile, "PassedTime")?.ToString(),
                ["timeAfterHit"] = ReadExpandedSummoningOptionalMember(
                    projectile, "TimeAfterHit")?.ToString(),
                ["passedDistance"] = ReadExpandedSummoningOptionalMember(
                    projectile, "PassedDistance")?.ToString()
            };
        }

        private static void ConfigureSprint28CatchFaction(
            BlueprintFaction faction, BlueprintFaction enemy)
        {
            faction.Peaceful = false;
            faction.AlwaysEnemy = false;
            faction.Neutral = false;
            faction.IsDirectlyControllable = false;
            faction.Dummy = null;
            faction.AttackFactions = new[] { enemy };
        }

        // The fixture positions and wakes two disposable exact-owned giants.
        // It does not fabricate a hit or save. A native rock command supplies
        // the attack, and a target-side rule handler supplies the catch.
        private IEnumerable<int> ReviewSprint28Catch(
            ExpandedSummoningCorrectionFixture fixture)
        {
            bool mode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool pause = Game.Instance.IsPaused;
            Game.Instance.IsPaused = true;
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;
                if (Game.Instance.ProjectileController.Projectiles.Any())
                    throw new InvalidOperationException(
                        "Native catch fixture began with unrelated projectiles.");
                var results = new JArray();
                foreach (var spec in new[] {
                    new { Target="hill-giant", Reflex=40, Dc=15 },
                    new { Target="stone-giant", Reflex=-40, Dc=11 },
                    new { Target="fire-giant", Reflex=40, Dc=15 }
                })
                {
                    // Use an exact project giant as the disposable thrower;
                    // the target remains a real summon.
                    BlueprintUnit hill = fixture.Blueprints
                        .OfType<BlueprintUnit>().Single(value =>
                            value.name == "KMG_Summoning_Unit_HillGiant");
                    UnitEntityData thrower = Game.Instance.EntityCreator
                        .SpawnUnit(hill, fixture.Caster.Position +
                            Vector3.right * 3f, Quaternion.identity,
                            fixture.Scene);
                    fixture.Created.Add(thrower);
                    PlaceExpandedSummoningUnit(thrower, thrower.Position);
                    UnitEntityData target = CastExpandedSummoningOwnTier(
                        fixture, spec.Target);
                    SetExpandedSummoningBrainActive(thrower, false);
                    SetExpandedSummoningBrainActive(target, false);
                    BlueprintFaction throwerFaction = null;
                    BlueprintFaction targetFaction = null;
                    bool casterPartyAdded = false;
                    bool casterJoined = false;
                    var ownedProjectiles = new List<Projectile>();
                    Projectile[] priorProjectiles =
                        Game.Instance.ProjectileController.Projectiles.ToArray();
                    try
                    {
                        // Both factions are unregistered clones owned by this
                        // disposable case. No shared native faction graph or
                        // protected party actor is changed.
                        throwerFaction = UnityEngine.Object.Instantiate(thrower.Faction);
                        targetFaction = UnityEngine.Object.Instantiate(target.Faction);
                        throwerFaction.name = "KMG_Runtime_S28_CatchThrowerFaction";
                        targetFaction.name = "KMG_Runtime_S28_CatchTargetFaction";
                        ConfigureSprint28CatchFaction(throwerFaction, targetFaction);
                        ConfigureSprint28CatchFaction(targetFaction, throwerFaction);
                        // Native IsEnemy reads per-group cached attack
                        // factions. Isolate each owned actor before touching
                        // that cache; no player or campaign group is modified.
                        thrower.Descriptor.SwitchFactions(throwerFaction, false);
                        thrower.GroupId = "KMG_Runtime_S28_CatchThrower_" + thrower.UniqueId;
                        target.Descriptor.SwitchFactions(targetFaction, false);
                        target.GroupId = "KMG_Runtime_S28_CatchTarget_" + target.UniqueId;
                        thrower.AttackFactions.Match(new[] { targetFaction });
                        target.AttackFactions.Match(new[] { throwerFaction });
                        thrower.Group.UpdateAttackFactionsCache();
                        target.Group.UpdateAttackFactionsCache();
                        thrower.Stats.BaseAttackBonus.BaseValue = 50;
                        target.Stats.SaveReflex.BaseValue = spec.Reflex;
                        var switchCommand = new UnitSwitchHandEquipmentSet(2);
                        thrower.Commands.Run(switchCommand);
                        DateTime switchDeadline = DateTime.UtcNow.AddSeconds(20);
                        for (int frame = 0; frame < 1000 &&
                            !switchCommand.IsFinished &&
                            DateTime.UtcNow < switchDeadline; frame++)
                            yield return 0;
                        ItemEntityWeapon rock =
                            thrower.Body.PrimaryHand.MaybeWeapon;
                        if (!switchCommand.IsFinished ||
                            thrower.Body.CurrentHandEquipmentSetIndex != 2 ||
                            rock == null)
                            throw new InvalidOperationException(
                                "Exact disposable thrower did not equip its rock.");
                        Vector3 far; string lane, direction;
                        Vector3 origin = Sprint24AiCombatLane(fixture.Caster,
                            thrower, target, 8f, out far, out lane,
                            out direction);
                        PlaceExpandedSummoningUnit(thrower, origin);
                        PlaceExpandedSummoningUnit(target, far);
                        foreach (UnitEntityData unit in new[] {
                            thrower, target })
                        {
                            unit.Wake(0f);
                            if (!Game.Instance.State.AwakeUnits.Contains(unit))
                                Game.Instance.State.AwakeUnits.Add(unit);
                        }
                        // The native preparation controller requires a
                        // controllable group in combat. Enroll only this
                        // request-owned caster, then restore its party and
                        // combat membership in this case's finally block.
                        if (!Game.Instance.Player.Party.Contains(fixture.Caster))
                        {
                            Game.Instance.Player.Party.Add(fixture.Caster);
                            casterPartyAdded = true;
                        }
                        if (!fixture.Caster.CombatState.IsInCombat)
                        {
                            fixture.Caster.JoinCombat();
                            casterJoined = true;
                        }
                        thrower.JoinCombat(); target.JoinCombat();
                        Game.Instance.Player.UpdateIsInCombat();
                        new UnitCombatJoinController().Tick();
                        new UnitCombatPrepareController().Tick();
                        thrower.CombatState.OnNewRound();
                        target.CombatState.OnNewRound();
                        thrower.Memory.Add(target);
                        target.Memory.Add(thrower);
                        thrower.SightCache.ClearIfNeeded();
                        target.SightCache.ClearIfNeeded();
                        new Kingmaker.Controllers.Units.UnitMemoryController()
                            .Tick();
                        // Both real geometry checks below must still pass.
                        // The fixture marks an already visible enemy as
                        // noticed before testing the free defensive response.
                        thrower.Memory.Add(target).Visible = true;
                        target.Memory.Add(thrower).Visible = true;
                        // Catch is unavailable to a flat-footed target.
                        // Admit both actors through the native combat/round
                        // controllers, then verify the actual defensive rule.
                        int initiativeFrames = 0;
                        while (!target.CombatState.CanActInCombat &&
                            initiativeFrames++ < 160) yield return 0;
                        bool flatFooted = Rulebook.Trigger(
                            new RuleCheckTargetFlatFooted(thrower, target))
                            .IsFlatFooted;
                        var probe = new UnitAttack(target);
                        probe.Init(thrower);
                        int unrelatedEnemies = fixture.UnitsBefore
                            .OfType<UnitEntityData>().Count(value =>
                                value.IsEnemy(thrower) || thrower.IsEnemy(value) ||
                                value.IsEnemy(target) || target.IsEnemy(value));
                        bool ready = !thrower.Group.IsPlayerParty &&
                            !target.Group.IsPlayerParty && unrelatedEnemies == 0 &&
                            thrower.IsEnemy(target) &&
                            target.IsEnemy(thrower) &&
                            thrower.HasLOS(target) &&
                            target.HasLOS(thrower) &&
                            target.Memory.ContainsVisible(thrower) &&
                            !flatFooted && probe.IsUnitEnoughClose &&
                            target.Descriptor.State.CanAct;
                        Sprint25Check("s28-catch-" + spec.Target + "-lane",
                            ready, new JObject {
                                ["thrower"] = thrower.UniqueId,
                                ["target"] = target.UniqueId,
                                ["lane"] = lane, ["distance"] =
                                    thrower.DistanceTo(target),
                                ["enemy"] = thrower.IsEnemy(target),
                                ["targetEnemy"] = target.IsEnemy(thrower),
                                ["throwerFaction"] = thrower.Faction == null
                                    ? null : thrower.Faction.name,
                                ["targetFaction"] = target.Faction == null
                                    ? null : target.Faction.name,
                                ["throwerPlayerFaction"] =
                                    thrower.IsPlayerFaction,
                                ["throwerGroup"] = thrower.GroupId,
                                ["targetGroup"] = target.GroupId,
                                ["throwerGroupIsParty"] =
                                    thrower.Group.IsPlayerParty,
                                ["targetGroupIsParty"] =
                                    target.Group.IsPlayerParty,
                                ["unrelatedEnemies"] = unrelatedEnemies,
                                ["throwerAttackFactions"] = new JArray(
                                    thrower.AttackFactions.Select(value =>
                                        value.name)),
                                ["targetAttackFactions"] = new JArray(
                                    target.AttackFactions.Select(value =>
                                        value.name)),
                                ["targetPlayerFaction"] =
                                    target.IsPlayerFaction,
                                ["los"] = thrower.HasLOS(target),
                                ["targetLos"] = target.HasLOS(thrower),
                                ["targetAware"] =
                                    target.Memory.ContainsVisible(thrower),
                                ["targetMemoryContainsThrower"] =
                                    target.Memory.Contains(thrower),
                                ["playerInCombat"] =
                                    Game.Instance.Player.IsInCombat,
                                ["casterPartyAdded"] = casterPartyAdded,
                                ["casterGroupCombat"] =
                                    fixture.Caster.Group.IsInCombat.ToString(),
                                ["throwerCombat"] =
                                    thrower.CombatState.IsInCombat,
                                ["targetCombat"] =
                                    target.CombatState.IsInCombat,
                                ["targetPrepared"] =
                                    target.CombatState.Prepared.ToString(),
                                ["targetCanActInCombat"] =
                                    target.CombatState.CanActInCombat,
                                ["targetInitiativeCooldown"] =
                                    target.CombatState.Cooldown.Initiative,
                                ["initiativeFrames"] = initiativeFrames,
                                ["nativeFlatFooted"] = flatFooted,
                                ["inRange"] = probe.IsUnitEnoughClose,
                                ["canAct"] =
                                    target.Descriptor.State.CanAct },
                            "The target is aware of one visible legal incoming native rock attack.");
                        if (!ready) throw new InvalidOperationException(
                            "Rock catching lacks an aware visible target.");
                        int before = target.Descriptor.Damage;
                        int projectilesBefore = priorProjectiles.Length;
                        SummonGiantRockCatch.ClearOutcomes();
                        var observer = new Sprint24AttackObserver {
                            Owner = thrower, Target = target };
                        var command = new UnitAttack(target) {
                            IsSingleAttack = true };
                        EventBus.Subscribe(observer);
                        try
                        {
                            using (var saves = new Sprint28CatchSaves(target))
                            {
                                thrower.Commands.Run(command);
                                DateTime deadline =
                                    DateTime.UtcNow.AddSeconds(35);
                                int frames = 0;
                                while (frames++ < 3500 &&
                                    DateTime.UtcNow < deadline &&
                                    (!command.IsFinished ||
                                     observer.WeaponAttacks.Count == 0))
                                {
                                    CaptureSprint28CatchProjectiles(
                                        ownedProjectiles, priorProjectiles,
                                        thrower);
                                    yield return 0;
                                }
                                int impactFrames = 0;
                                DateTime impactDeadline =
                                    DateTime.UtcNow.AddSeconds(10);
                                while (impactFrames++ < 600 &&
                                    DateTime.UtcNow < impactDeadline &&
                                    ((saves.Saves.Count == 0 &&
                                      observer.Damage.Count == 0) ||
                                     (saves.Saves.Count == 1 &&
                                      !saves.Saves[0].IsPassed &&
                                      observer.Damage.Count == 0) ||
                                     Game.Instance.ProjectileController
                                         .Projectiles.Count() > projectilesBefore))
                                {
                                    CaptureSprint28CatchProjectiles(
                                        ownedProjectiles, priorProjectiles,
                                        thrower);
                                    yield return 0;
                                }
                                for (int frame = 0; frame < 12; frame++)
                                {
                                    CaptureSprint28CatchProjectiles(
                                        ownedProjectiles, priorProjectiles,
                                        thrower);
                                    yield return 0;
                                }
                                CaptureSprint28CatchProjectiles(
                                    ownedProjectiles, priorProjectiles, thrower);
                                RuleSavingThrow save =
                                    saves.Saves.SingleOrDefault();
                                var attack = observer.WeaponAttacks
                                    .SingleOrDefault();
                                bool caught = save != null &&
                                    save.IsPassed;
                                bool normalMiss = save == null &&
                                    SummonGiantRockCatch.ObservedOutcomes.Count == 0 &&
                                    attack?.AttackRoll != null &&
                                    !attack.AttackRoll.IsHit &&
                                    observer.Damage.Count == 0 &&
                                    target.Descriptor.Damage == before;
                                int activeOwned = ownedProjectiles.Count(value =>
                                    Game.Instance.ProjectileController.Projectiles
                                        .Contains(value));
                                bool exact = command.IsFinished &&
                                    impactFrames < 600 &&
                                    activeOwned == 0 &&
                                    Game.Instance.ProjectileController
                                        .Projectiles.SequenceEqual(priorProjectiles) &&
                                    observer.OwnerWeaponAttacks.Count == 1 &&
                                    observer.WeaponAttacks.Count == 1 &&
                                    ReferenceEquals(attack?.Weapon.Blueprint,
                                        rock.Blueprint) &&
                                    (normalMiss || save != null &&
                                     save.DifficultyClass == spec.Dc &&
                                     SummonGiantRockCatch.ObservedOutcomes.Count == 1 &&
                                     (caught
                                         ? attack.AttackRoll != null &&
                                           !attack.AttackRoll.IsHit &&
                                           observer.Damage.Count == 0 &&
                                           target.Descriptor.Damage == before
                                         : attack.AttackRoll != null &&
                                           attack.AttackRoll.IsHit &&
                                           observer.Damage.Count == 1 &&
                                           target.Descriptor.Damage > before));
                                var row = new JObject {
                                    ["thrower"] = thrower.UniqueId,
                                    ["target"] = target.UniqueId,
                                    ["rock"] = rock.Blueprint.AssetGuid,
                                    ["commandFinished"] = command.IsFinished,
                                    ["commandResult"] =
                                        command.Result.ToString(),
                                    ["frames"] = frames,
                                    ["impactFrames"] = impactFrames,
                                    ["projectilesBefore"] =
                                        projectilesBefore,
                                    ["projectilesAfter"] =
                                        Game.Instance.ProjectileController
                                            .Projectiles.Count(),
                                    ["ownedProjectiles"] =
                                        ownedProjectiles.Count,
                                    ["activeOwnedProjectiles"] = activeOwned,
                                    ["projectileStates"] = new JArray(
                                        ownedProjectiles.Select(
                                            DescribeSprint28CatchProjectile)),
                                    ["normalMiss"] = normalMiss,
                                    ["nativeFlatFootedBefore"] = flatFooted,
                                    ["attacks"] =
                                        observer.WeaponAttacks.Count,
                                    ["natural"] =
                                        attack?.AttackRoll?.Roll.Value,
                                    ["attackResult"] =
                                        attack?.AttackRoll?.Result.ToString(),
                                    ["flatFooted"] =
                                        attack?.AttackRoll?.IsTargetFlatFooted,
                                    ["saveCount"] = saves.Saves.Count,
                                    ["saveDc"] =
                                        save?.DifficultyClass,
                                    ["savePassed"] = save?.IsPassed,
                                    ["catchOutcomes"] = new JArray(
                                        SummonGiantRockCatch.ObservedOutcomes),
                                    ["damagePackets"] =
                                        observer.Damage.Count,
                                    ["damageBefore"] = before,
                                    ["damageAfter"] =
                                        target.Descriptor.Damage,
                                    ["forcedRolls"] = 0,
                                    ["fixtureAttacks"] = 0
                                };
                                results.Add(new JObject {
                                    ["target"] = spec.Target,
                                    ["caught"] = caught,
                                    ["eligible"] = save != null,
                                    ["normalMiss"] = normalMiss,
                                    ["exact"] = exact });
                                Sprint25Check("s28-catch-" + spec.Target +
                                    "-resolved", exact, row,
                                    "A would-hit native rock prompts one exact-owner Reflex check; its actual result either prevents harm before damage or permits one physical packet.");
                            }
                        }
                        finally { EventBus.Unsubscribe(observer); }
                    }
                    finally
                    {
                        thrower.Commands.InterruptAll(true);
                        target.Commands.InterruptAll(true);
                        CaptureSprint28CatchProjectiles(
                            ownedProjectiles, priorProjectiles, thrower);
                        foreach (Projectile projectile in ownedProjectiles)
                            projectile.Cleared = true;
                        Game.Instance.ProjectileController.Tick();
                        Game.Instance.ProjectileController.Tick();
                        Sprint25Check("s28-catch-" + spec.Target +
                            "-owned-projectile-cleanup",
                            Game.Instance.ProjectileController.Projectiles
                                .SequenceEqual(priorProjectiles),
                            new JObject {
                                ["owned"] = ownedProjectiles.Count,
                                ["remaining"] = Game.Instance.ProjectileController
                                    .Projectiles.Count(),
                                ["clearedByFixture"] =
                                    ownedProjectiles.Count(value => value.Cleared)
                            }, "Only this fixture's launched projectiles are cleared after natural lifecycle observation; all prior projectile references are preserved.");
                        if (thrower.IsInCombat) thrower.LeaveCombat();
                        if (target.IsInCombat) target.LeaveCombat();
                        if (casterJoined && fixture.Caster.IsInCombat)
                            fixture.Caster.LeaveCombat();
                        if (casterPartyAdded)
                            Game.Instance.Player.Party.Remove(fixture.Caster);
                        Game.Instance.Player.UpdateIsInCombat();
                        DisposeExpandedSummoningUnits(fixture.Created,
                            new[] { thrower, target });
                        if (throwerFaction != null)
                            UnityEngine.Object.Destroy(throwerFaction);
                        if (targetFaction != null)
                            UnityEngine.Object.Destroy(targetFaction);
                    }
                }
                bool both = results.Any(value => (bool)value["eligible"] &&
                    (bool)value["caught"]) &&
                    results.Any(value => (bool)value["eligible"] &&
                        !(bool)value["caught"]) &&
                    results.All(value => (bool)value["exact"]);
                Sprint25Check("s28-catch-both-outcomes", both,
                    new JObject { ["cases"] = results },
                    "At least one native catch prevents harm and one failed catch permits the same rock damage pipeline.");
            }
            finally
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = mode;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = pause;
            }
        }
    }
}
