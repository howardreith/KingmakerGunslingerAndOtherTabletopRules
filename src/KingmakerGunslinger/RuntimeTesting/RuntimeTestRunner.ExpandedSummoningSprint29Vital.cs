using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // One ordinary full attack and two separate native standard-action
        // abilities. Damage dice come from the resolved native packet, never
        // from a queried plan or a fixture-created attack rule.
        private IEnumerable<int> ReviewSprint29Vital(
            ExpandedSummoningCorrectionFixture fixture, bool turnBased)
        {
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool oldPause = Game.Instance.IsPaused;
            Game.Instance.IsPaused = true;
            CreateExpandedSummoningCorrectionHostile(fixture);
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;
                var ordinary = fixture.Blueprints.OfType<BlueprintAbility>()
                    .Single(value => value.AssetGuid ==
                        "efc60c91b8e64f244b95c66b270dbd7c");
                var improved = fixture.Blueprints.OfType<BlueprintAbility>()
                    .Single(value => value.AssetGuid ==
                        "c714cd636700ac24a91ca3df43326b00");
                var greater = fixture.Blueprints.OfType<BlueprintAbility>()
                    .Single(value => value.AssetGuid ==
                        "11f971b6453f74d4594c538e3c88d499");
                var privateImproved = fixture.Blueprints.OfType<BlueprintFeature>()
                    .Single(value => value.name ==
                        "KMG_Summoning_Giant_Sprint29_StormImprovedVitalStrike");
                var nativeImproved = fixture.Blueprints.OfType<BlueprintFeature>()
                    .Single(value => value.AssetGuid ==
                        "52913092cd018da47845f36e6fbe240f");
                foreach (var spec in new[] {
                    new { Name="ordinary-full", Ability=(BlueprintAbility)null,
                        Attacks=3, Rolls=4 },
                    new { Name="vital", Ability=ordinary,
                        Attacks=1, Rolls=8 },
                    new { Name="improved-vital", Ability=improved,
                        Attacks=1, Rolls=12 }
                })
                {
                    UnitEntityData owner = CastExpandedSummoningOwnTier(
                        fixture, "storm-giant");
                    SetExpandedSummoningBrainActive(owner, false);
                    UnitEntityData target = fixture.Hostile;
                    int originalHitPoints = target.Stats.HitPoints.BaseValue;
                    try
                    {
                        for (int frame = 0; frame < 8; frame++) yield return 0;
                        foreach (int frame in Sprint22SetPowerAttack(owner, false))
                            yield return frame;
                        ResetExpandedSummoningHostile(fixture);
                        Vector3 far;
                        string lane, direction;
                        Vector3 origin = Sprint24AiCombatLane(fixture.Caster,
                            owner, target, 8f, out far, out lane,
                            out direction);
                        PlaceExpandedSummoningUnit(owner, origin);
                        PlaceExpandedSummoningUnit(target,
                            origin + (far - origin).normalized *
                                (owner.Corpulence + target.Corpulence + .8f));
                        owner.SightCache.ClearIfNeeded();
                        target.SightCache.ClearIfNeeded();
                        var probe = new UnitAttack(target);
                        probe.Init(owner);
                        bool legal = owner.HasLOS(target) &&
                            probe.IsUnitEnoughClose && owner.IsEnemy(target) &&
                            !target.Descriptor.State.IsDead;
                        Sprint25Check("s29-vital-" + spec.Name + "-lane",
                            legal, new JObject { ["owner"] = owner.UniqueId,
                                ["target"] = target.UniqueId,
                                ["lane"] = lane, ["direction"] = direction,
                                ["los"] = owner.HasLOS(target),
                                ["range"] = probe.IsUnitEnoughClose },
                            "A visible hostile and legal melee lane precede the native action.");
                        if (!legal) throw new InvalidOperationException(
                            "Sprint 29 Vital Strike has no legal lane.");
                        owner.JoinCombat(); target.JoinCombat();
                        owner.Memory.Add(target); target.Memory.Add(owner);
                        JObject admission = null;
                        if (turnBased)
                        {
                            admission = new JObject();
                            foreach (int frame in Sprint22EnrollTurnBasedOwners(
                                new[] { owner }, target, admission))
                                yield return frame;
                            for (int frame = 0; frame < 500 &&
                                !Sprint18OwnerIsActing(owner); frame++)
                                yield return 0;
                            bool acting = (bool)admission["admitted"] &&
                                Sprint18OwnerIsActing(owner);
                            Sprint25Check("s29-vital-" + spec.Name +
                                "-turn-admission", acting,
                                new JObject { ["owner"] = owner.UniqueId,
                                    ["target"] = target.UniqueId,
                                    ["admission"] = admission,
                                    ["turn"] = Sprint22TurnState(owner,
                                        target) },
                                "The exact Storm Giant receives its native turn before the selected action.");
                            if (!acting) throw new InvalidOperationException(
                                "Storm Vital Strike turn admission failed.");
                        }
                        var baseGrant = owner.Descriptor.Abilities
                            .GetAbility(ordinary);
                        var improvedGrant = owner.Descriptor.Abilities
                            .GetAbility(improved);
                        bool grants = baseGrant != null &&
                            improvedGrant != null &&
                            owner.Descriptor.Abilities.GetAbility(greater) == null &&
                            owner.Descriptor.Progression.Features.HasFact(
                                privateImproved) &&
                            !owner.Descriptor.Progression.Features.HasFact(
                                nativeImproved);
                        Sprint25Check("s29-vital-" + spec.Name + "-grants",
                            grants, new JObject { ["owner"] = owner.UniqueId,
                                ["vital"] = baseGrant != null,
                                ["improved"] = improvedGrant != null,
                                ["greater"] = owner.Descriptor.Abilities
                                    .GetAbility(greater) != null,
                                ["privateFeature"] = owner.Descriptor
                                    .Progression.Features.HasFact(privateImproved),
                                ["nativeImprovedFeature"] = owner.Descriptor
                                    .Progression.Features.HasFact(nativeImproved) },
                            "Storm retains two distinct native feat actions without Greater Vital Strike or donor feature grants.");
                        if (!grants) throw new InvalidOperationException(
                            "Storm's two Vital Strike actions are not distinct grants.");
                        owner.Stats.AdditionalAttackBonus.BaseValue += 20;
                        target.Stats.HitPoints.BaseValue = 100000;
                        target.Descriptor.Damage = 0;
                        var powerAttack = owner.Descriptor.ActivatableAbilities
                            .Enumerable.Single(value => value.Blueprint.AssetGuid ==
                                "a7b339e4f6ff93a4697df5d7a87ff619");
                        var powerAtRoll = new List<bool>();
                        var observer = new Sprint24AttackObserver {
                            Owner = owner, Target = target };
                        observer.BeforeRoll = value => powerAtRoll.Add(
                            powerAttack.Blueprint.Buff != null &&
                            owner.Descriptor.Buffs.GetBuff(
                                powerAttack.Blueprint.Buff) != null);
                        var costs = new JArray();
                        var endings = new JArray();
                        var abilityData = spec.Ability == null ? null :
                            new AbilityData(owner.Descriptor.Abilities
                                .GetAbility(spec.Ability));
                        var targetWrapper = new TargetWrapper(target);
                        bool ready = abilityData == null ||
                            abilityData.IsAvailable &&
                            abilityData.CanTarget(targetWrapper);
                        Sprint25Check("s29-vital-" + spec.Name + "-ready",
                            ready, new JObject { ["owner"] = owner.UniqueId,
                                ["ability"] = spec.Ability?.AssetGuid,
                                ["available"] = abilityData?.IsAvailable,
                                ["canTarget"] = abilityData?.CanTarget(
                                    targetWrapper) },
                            "The selected native feat action is directly available against the disposable hostile.");
                        if (!ready) throw new InvalidOperationException(
                            "Storm Vital Strike action is unavailable.");
                        UnitUseAbility abilityCommand = abilityData == null ?
                            null : new UnitUseAbility(abilityData,
                                targetWrapper);
                        UnitCommand command = abilityCommand == null ?
                            (UnitCommand)new UnitAttack(target) {
                                ForceFullAttack = true } : abilityCommand;
                        EventBus.Subscribe(observer);
                        try
                        {
                            using (var economy = new FirearmReviewCosts(owner,
                                costs))
                            using (var trace = new Sprint26CommandTrace(owner,
                                endings))
                            {
                                owner.Commands.Run(command);
                                int frames = 0;
                                DateTime deadline = DateTime.UtcNow
                                    .AddSeconds(35);
                                while (frames++ < 3500 &&
                                    DateTime.UtcNow < deadline &&
                                    (!command.IsFinished || observer
                                        .WeaponAttacks.Count < spec.Attacks ||
                                     abilityCommand != null &&
                                        (abilityCommand.ExecutionProcess == null ||
                                         !abilityCommand.ExecutionProcess.IsEnded)))
                                {
                                    if (abilityCommand != null &&
                                        abilityCommand.ExecutionProcess != null &&
                                        !abilityCommand.ExecutionProcess.IsEnded)
                                        abilityCommand.ExecutionProcess.Tick();
                                    yield return 0;
                                }
                                int hits = observer.WeaponAttacks.Count(value =>
                                    value.AttackRoll != null &&
                                    value.AttackRoll.IsHit);
                                int impactFrames = 0;
                                DateTime impactDeadline = DateTime.UtcNow
                                    .AddSeconds(10);
                                while (observer.Damage.Count < hits &&
                                    impactFrames++ < 600 &&
                                    DateTime.UtcNow < impactDeadline)
                                    yield return 0;
                                var packets = observer.Damage.SelectMany(value =>
                                    value.DamageBundle.OfType<PhysicalDamage>())
                                    .ToArray();
                                bool nativeBonuses = powerAtRoll.Count ==
                                    spec.Attacks && Enumerable.Range(0,
                                        spec.Attacks).All(index =>
                                            observer.WeaponAttacks[index]
                                                .AttackRoll != null &&
                                            (!observer.WeaponAttacks[index]
                                                .AttackRoll.IsHit ||
                                             observer.WeaponAttacks[index]
                                                .MeleeDamage?.DamageBundle
                                                .OfType<PhysicalDamage>()
                                                .SingleOrDefault()?.Bonus ==
                                                21 + (powerAtRoll[index] ?
                                                    12 : 0)));
                                bool exact = command.IsFinished &&
                                    (command.Result == UnitCommand.ResultType
                                        .Success || trace.Exhausted(command)) &&
                                    observer.OwnerWeaponAttacks.Count ==
                                        spec.Attacks &&
                                    observer.WeaponAttacks.Count ==
                                        spec.Attacks &&
                                    observer.Rolls.Count == spec.Attacks &&
                                    hits > 0 &&
                                    observer.Damage.Count == hits &&
                                    packets.Length == hits &&
                                    nativeBonuses &&
                                    packets.All(value => value.Dice.Rolls ==
                                        spec.Rolls && value.Dice.Dice ==
                                        Kingmaker.RuleSystem.DiceType.D6) &&
                                    observer.WeaponAttacks.All(value =>
                                        value.Weapon?.Blueprint?.name ==
                                        "KMG_Summoning_Giant_Sprint29_StormGreatsword4d6") &&
                                    economy.Count(command) == 1 &&
                                    economy.Exact(command, turnBased) &&
                                    !command.Cutscene &&
                                    !command.IsIgnoreCooldown &&
                                    !target.Descriptor.State.IsDead;
                                Sprint25Check("s29-vital-" + spec.Name +
                                    "-resolved", exact, new JObject {
                                        ["owner"] = owner.UniqueId,
                                        ["target"] = target.UniqueId,
                                        ["turnBased"] = turnBased,
                                        ["admission"] = admission,
                                        ["ability"] = spec.Ability?.AssetGuid,
                                        ["finished"] = command.IsFinished,
                                        ["result"] = command.Result.ToString(),
                                        ["frames"] = frames,
                                        ["attacks"] = observer.WeaponAttacks.Count,
                                        ["natural"] = new JArray(observer
                                            .WeaponAttacks.Select(value =>
                                                value.AttackRoll?.Roll.Value)),
                                        ["hits"] = hits,
                                        ["damagePackets"] = packets.Length,
                                        ["dice"] = new JArray(packets.Select(
                                            value => value.Dice.Rolls)),
                                        ["bonuses"] = new JArray(packets.Select(
                                            value => value.Bonus)),
                                        ["powerAttackAtRoll"] =
                                            new JArray(powerAtRoll),
                                        ["powerAttackToggleOnAfter"] =
                                            powerAttack.IsOn,
                                        ["powerAttackRunningAfter"] =
                                            powerAttack.IsRunning,
                                        ["nativeBonusMatch"] = nativeBonuses,
                                        ["weaponStatsBonuses"] = new JArray(
                                            observer.WeaponAttacks.Select(value =>
                                                value.WeaponStats == null ||
                                                value.WeaponStats.DamageDescription
                                                    .Count == 0 ? (int?)null :
                                                value.WeaponStats
                                                    .DamageDescription[0].Bonus)),
                                        ["expectedDice"] = spec.Rolls,
                                        ["nativeExhausted"] =
                                            trace.Exhausted(command),
                                        ["costs"] = costs,
                                        ["nativeEnds"] = endings,
                                        ["forcedRolls"] = 0,
                                        ["fixtureAttackCommands"] = 0 },
                                    "The ordinary full routine retains base dice; each Vital Strike action spends one native action for one weapon attack with its own multiplied base dice.");
                            }
                        }
                        finally { EventBus.Unsubscribe(observer); }
                    }
                    finally
                    {
                        target.Stats.HitPoints.BaseValue = originalHitPoints;
                        SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue =
                            false;
                        Game.Instance.TurnBasedCombatController.Activate();
                        owner.Commands.InterruptAll(true);
                        if (owner.IsInCombat) owner.LeaveCombat();
                        ResetExpandedSummoningHostile(fixture);
                        DisposeExpandedSummoningUnits(fixture.Created,
                            new[] { owner });
                    }
                }
            }
            finally
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue =
                    oldMode;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = oldPause;
            }
        }
    }
}
