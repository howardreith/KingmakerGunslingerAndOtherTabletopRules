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
                        float initialInitiativeCooldown =
                            target.CombatState.Cooldown.Initiative;
                        DateTime initiativeDeadline =
                            DateTime.UtcNow.AddSeconds(30);
                        int initiativeFrames = 0;
                        // The native prepared actor is still flat-footed
                        // while its real initiative cooldown is positive.
                        // Wait for that exact state to clear, with a bounded
                        // clock and frame limit; never assign initiative.
                        while (!target.CombatState.CanActInCombat &&
                            target.CombatState.Cooldown.Initiative > 0f &&
                            initiativeFrames++ < 1000 &&
                            DateTime.UtcNow < initiativeDeadline)
                            yield return 0;
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
                                ["initialInitiativeCooldown"] =
                                    initialInitiativeCooldown,
                                ["targetInitiativeCooldown"] =
                                    target.CombatState.Cooldown.Initiative,
                                ["initiativeFrames"] = initiativeFrames,
                                ["initiativeDeadlineReached"] =
                                    DateTime.UtcNow >= initiativeDeadline,
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
                                // Rulebook-only negative controls run while the
                                // same legal lane and non-flat-footed target
                                // are intact. Neither produces a projectile or
                                // spends a native attack command.
                                ItemEntityWeapon club = thrower.Descriptor
                                    .Inventory.Items.OfType<ItemEntityWeapon>()
                                    .Single(value => ReferenceEquals(
                                        value.Blueprint,
                                        thrower.Blueprint.Body.PrimaryHand));
                                var controls = new JArray();
                                RuleAttackRoll nonrock = null;
                                for (int attempt = 0; attempt < 8; attempt++)
                                {
                                    nonrock = Rulebook.Trigger(new RuleAttackRoll(
                                        thrower, target, club, 0));
                                    controls.Add(new JObject {
                                        ["kind"] = "nonrock",
                                        ["natural"] = nonrock.Roll.Value,
                                        ["result"] = nonrock.Result.ToString() });
                                    if (nonrock.Roll.Value != 1) break;
                                }
                                bool nonrockExcluded = nonrock != null &&
                                    nonrock.Roll.Value != 1 && nonrock.IsHit &&
                                    saves.Saves.Count == 0 &&
                                    target.Get<UnitPartSummonGiantRockCatch>() == null &&
                                    SummonGiantRockCatch.ObservedOutcomes.Count == 0;
                                Sprint25Check("s28-catch-" + spec.Target +
                                    "-nonrock-control", nonrockExcluded,
                                    new JObject { ["target"] = target.UniqueId,
                                        ["attacks"] = controls,
                                        ["saves"] = saves.Saves.Count,
                                        ["ledger"] = target.Get<
                                            UnitPartSummonGiantRockCatch>() != null },
                                    "A would-hit manufactured club rule cannot spend the target's rock catch or prompt a Reflex save.");
                                RuleAttackRoll unaware = null;
                                target.Memory.Add(thrower).Visible = false;
                                bool unawareAtRollStart =
                                    target.Memory.ContainsVisible(thrower);
                                bool unawareAfterRoll = false;
                                try
                                {
                                    for (int attempt = 0; attempt < 8;
                                        attempt++)
                                    {
                                        unaware = Rulebook.Trigger(
                                            new RuleAttackRoll(thrower,
                                                target, rock, 0));
                                        controls.Add(new JObject {
                                            ["kind"] = "unaware-rock",
                                            ["natural"] =
                                                unaware.Roll.Value,
                                            ["result"] =
                                                unaware.Result.ToString(),
                                            ["flatFooted"] =
                                                unaware.IsTargetFlatFooted });
                                        unawareAfterRoll =
                                            target.Memory.ContainsVisible(thrower);
                                        if (unaware.Roll.Value != 1) break;
                                    }
                                }
                                finally
                                { target.Memory.Add(thrower).Visible = true; }
                                bool unawareExcluded = unaware != null &&
                                    unaware.Roll.Value != 1 && unaware.IsHit &&
                                    !unawareAtRollStart && !unawareAfterRoll &&
                                    target.Memory.ContainsVisible(thrower) &&
                                    saves.Saves.Count == 0 &&
                                    target.Get<UnitPartSummonGiantRockCatch>() == null &&
                                    SummonGiantRockCatch.ObservedOutcomes.Count == 0;
                                Sprint25Check("s28-catch-" + spec.Target +
                                    "-unaware-control", unawareExcluded,
                                    new JObject { ["target"] = target.UniqueId,
                                        ["attacks"] = controls,
                                        ["saves"] = saves.Saves.Count,
                                        ["visibleAtRollStart"] =
                                            unawareAtRollStart,
                                        ["visibleAfterRoll"] =
                                            unawareAfterRoll,
                                        ["visibilityRestored"] =
                                            target.Memory.ContainsVisible(thrower),
                                        ["ledger"] = target.Get<
                                            UnitPartSummonGiantRockCatch>() != null },
                                    "A would-hit unseen rock cannot prompt the defensive catch; exact actor memory is restored before the native command.");
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
                                    (!command.IsFinished ||
                                     observer.WeaponAttacks.Count == 0 ||
                                     ownedProjectiles.Count == 0 ||
                                     ownedProjectiles.Any(value => !value.IsHit) ||
                                     (observer.WeaponAttacks.Count == 1 &&
                                      observer.WeaponAttacks[0].AttackRoll.IsHit &&
                                      saves.Saves.Count == 0 &&
                                      observer.Damage.Count == 0) ||
                                     (saves.Saves.Count == 1 &&
                                      !saves.Saves[0].IsPassed &&
                                      observer.Damage.Count == 0)))
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
                                // Native impact resolves before the rock's
                                // 20-second visual particle lifetime expires.
                                // Check the exact owned projectile and preserve
                                // every prior projectile; fixture cleanup below
                                // removes only this case's lingering visual.
                                bool projectileExact =
                                    ownedProjectiles.Count == 1 &&
                                    ownedProjectiles[0].IsHit &&
                                    ReferenceEquals(ownedProjectiles[0].Launcher,
                                        thrower) &&
                                    ownedProjectiles[0].AttackResult ==
                                        attack?.AttackRoll?.Result &&
                                    activeOwned <= 1 &&
                                    Game.Instance.ProjectileController.Projectiles
                                        .Where(value => !ownedProjectiles.Contains(value))
                                        .SequenceEqual(priorProjectiles);
                                bool exact = command.IsFinished &&
                                    impactFrames < 600 &&
                                    projectileExact &&
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
                                    ["projectileExact"] = projectileExact,
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
                                // Exercise the same target-side rulebook seam
                                // again in this native round without issuing a
                                // second command or supplying a hit. These
                                // roll-only probes cannot create projectiles
                                // or damage; the command above proves impact.
                                long round = ContextActionUngulateTrample.CurrentRound();
                                var probes = new JArray();
                                for (int attempt = 0; attempt < 8 &&
                                    target.Get<UnitPartSummonGiantRockCatch>() == null;
                                    attempt++)
                                {
                                    RuleAttackRoll spendProbe = Rulebook.Trigger(
                                        new RuleAttackRoll(thrower, target,
                                            rock, 0));
                                    probes.Add(new JObject {
                                        ["kind"] = "first-eligible",
                                        ["natural"] = spendProbe.Roll.Value,
                                        ["result"] = spendProbe.Result.ToString(),
                                        ["saveCount"] = saves.Saves.Count });
                                }
                                var spent = target.Get<UnitPartSummonGiantRockCatch>();
                                int spentSaves = saves.Saves.Count;
                                RuleAttackRoll repeat = null;
                                for (int attempt = 0; attempt < 8; attempt++)
                                {
                                    repeat = Rulebook.Trigger(new RuleAttackRoll(
                                        thrower, target, rock, 0));
                                    probes.Add(new JObject {
                                        ["kind"] = "same-round-repeat",
                                        ["natural"] = repeat.Roll.Value,
                                        ["result"] = repeat.Result.ToString(),
                                        ["saveCount"] = saves.Saves.Count });
                                    if (repeat.Roll.Value != 1) break;
                                }
                                bool once = spent != null &&
                                    spent.LastRound == round && spentSaves == 1 &&
                                    ContextActionUngulateTrample.CurrentRound() == round &&
                                    repeat != null && repeat.Roll.Value != 1 &&
                                    repeat.IsHit && saves.Saves.Count == spentSaves &&
                                    SummonGiantRockCatch.ObservedOutcomes.Count == 1;
                                Sprint25Check("s28-catch-" + spec.Target +
                                    "-once-round", once, new JObject {
                                        ["target"] = target.UniqueId,
                                        ["round"] = round,
                                        ["ledgerRound"] = spent?.LastRound,
                                        ["savesAfterSpend"] = spentSaves,
                                        ["firstSavePassed"] = spentSaves == 0
                                            ? (bool?)null : saves.Saves[0].IsPassed,
                                        ["spentByNativeCommand"] = save != null,
                                        ["savesAfterRepeat"] = saves.Saves.Count,
                                        ["probes"] = probes,
                                        ["fixtureGrantedHit"] = false,
                                        ["fixtureIssuedCommand"] = false },
                                    "The exact target's serialized catch ledger permits one Reflex attempt this round; a later ordinary native rock attack roll gets no second save.");
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
                bool complete = results.Count == 3 &&
                    results.All(value => (bool)value["exact"] &&
                        ((bool)value["eligible"] ||
                         (bool)value["normalMiss"]));
                Sprint25Check("s28-catch-exact-command-outcomes",
                    complete, new JObject { ["cases"] = results },
                    "Each native rock command has an exact catch or ordinary miss result; a failed save, when rolled, must permit its actual physical damage packet. The fixture does not require random success and failure in three attacks.");
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
