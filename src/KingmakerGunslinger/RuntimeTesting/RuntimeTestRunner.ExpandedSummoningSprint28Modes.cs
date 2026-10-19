using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic.Commands;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Uses the native switch command and native weapon/full-attack rules.
        // This phase records plans and stat calculations, not resolved attacks.
        private IEnumerable<int> ReviewSprint28Modes(
            ExpandedSummoningCorrectionFixture fixture)
        {
            bool mode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool pause = Game.Instance.IsPaused;
            Game.Instance.IsPaused = true;
            CreateExpandedSummoningCorrectionHostile(fixture);
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = false;
                for (int i = 0; i < 16; i++) yield return 0;
                foreach (var spec in new[] {
                    new { Key="hill-giant", Token="HillGiant", Bab=7, WeaponAttacks=2,
                        WeaponAttack=14, SlamAttack=13, RockAttack=6, Damage=10,
                        SlamDamage=7, RockDamage=10, RockShots=1, Range=120 },
                    new { Key="stone-giant", Token="StoneGiant", Bab=9, WeaponAttacks=2,
                        WeaponAttack=16, SlamAttack=16, RockAttack=11, Damage=12,
                        SlamDamage=8, RockDamage=12, RockShots=2, Range=180 },
                    new { Key="fire-giant", Token="FireGiant", Bab=11, WeaponAttacks=3,
                        WeaponAttack=21, SlamAttack=20, RockAttack=10, Damage=15,
                        SlamDamage=10, RockDamage=15, RockShots=1, Range=120 }
                })
                {
                    UnitEntityData owner = CastExpandedSummoningOwnTier(fixture, spec.Key);
                    SetExpandedSummoningBrainActive(owner, false);
                    try
                    {
                        for (int i = 0; i < 8; i++) yield return 0;
                        foreach (int frame in Sprint22SetPowerAttack(owner, false))
                            yield return frame;
                        var sets = owner.Body.HandsEquipmentSets;
                        Sprint25Check("s28-"+spec.Key+"-hand-sets",
                            sets.Count >= 3 && owner.Body.CurrentHandEquipmentSetIndex == 0,
                            new JObject { ["owner"] = owner.UniqueId,
                                ["count"] = sets.Count,
                                ["active"] = owner.Body.CurrentHandEquipmentSetIndex,
                                ["primary"] = new JArray(sets.Select(value =>
                                    value?.PrimaryHand?.MaybeWeapon?.Blueprint?.AssetGuid)) },
                            "Three native hand sets exist for the weapon, two-slam and rock alternatives.");
                        for (int index = 0; index < 3; index++)
                        {
                            if (index != 0)
                            {
                                var command = new UnitSwitchHandEquipmentSet(index);
                                owner.Commands.Run(command);
                                int frames = 0;
                                DateTime deadline = DateTime.UtcNow.AddSeconds(20);
                                while (!command.IsFinished && frames++ < 1000 &&
                                    DateTime.UtcNow < deadline) yield return 0;
                                Sprint25Check("s28-"+spec.Key+"-switch-"+index,
                                    command.IsFinished &&
                                    owner.Body.CurrentHandEquipmentSetIndex == index,
                                    new JObject { ["owner"] = owner.UniqueId,
                                        ["finished"] = command.IsFinished,
                                        ["result"] = command.Result.ToString(),
                                        ["frames"] = frames,
                                        ["active"] = owner.Body.CurrentHandEquipmentSetIndex },
                                    "A real native hand-set switch command selects the exact summon-owned alternative.");
                            }
                            ItemEntityWeapon weapon = owner.Body.PrimaryHand.MaybeWeapon;
                            if (weapon == null) throw new InvalidOperationException(
                                "Giant mode has no active primary weapon: " + spec.Key + "/" + index);
                            var attack = new UnitAttack(fixture.Hostile);
                            attack.Init(owner);
                            var planned = attack.CreateFullAttack();
                            var stats = Rulebook.Trigger(new RuleCalculateWeaponStats(owner, weapon, null));
                            var damage = stats.DamageDescription[0].CreateDamage();
                            int bonus = Rulebook.Trigger(new RuleCalculateAttackBonusWithoutTarget(
                                owner, weapon, 0)).Result;
                            int expectedCount = index == 0 ? spec.WeaponAttacks :
                                index == 1 ? 2 : spec.RockShots;
                            int expectedAttack = index == 0 ? spec.WeaponAttack :
                                index == 1 ? spec.SlamAttack : spec.RockAttack;
                            int expectedDamage = index == 0 ? spec.Damage :
                                index == 1 ? spec.SlamDamage : spec.RockDamage;
                            int expectedRolls = index == 0 && spec.Key == "fire-giant" ? 3 :
                                index == 0 ? 2 : 1;
                            JObject heat = stats.DamageDescription.Count > 1 ?
                                Sprint16DamageLine(stats.DamageDescription[1].CreateDamage()) : null;
                            bool heatExact = index != 2 || spec.Key != "fire-giant" ||
                                heat != null && (string)heat["type"] == "Energy" &&
                                (string)heat["dice"] == "1d6" &&
                                (int)heat["bonus"] == 0 &&
                                (string)heat["energy"] == "Fire";
                            bool exact = heatExact && planned.Count == expectedCount &&
                                bonus == expectedAttack && damage.Bonus == expectedDamage &&
                                damage.Dice.Rolls == expectedRolls &&
                                (index != 2 || weapon.Blueprint.Type.AttackRange.Value == spec.Range) &&
                                ReferenceEquals(weapon.Owner, owner.Descriptor) &&
                                weapon.IsNonRemovable &&
                                (index != 2 || spec.Key != "stone-giant" ||
                                    planned.Count == 2 &&
                                    planned[1].AttackBonusPenalty == 5) &&
                                (index != 2 || stats.DamageDescription.Count ==
                                    (spec.Key == "fire-giant" ? 2 : 1));
                            Sprint25Check("s28-"+spec.Key+"-mode-"+index,
                                exact, new JObject { ["owner"] = owner.UniqueId,
                                    ["index"] = index,
                                    ["weapon"] = weapon.Blueprint.AssetGuid,
                                    ["itemOwner"] = weapon.Owner == owner.Descriptor,
                                    ["nonRemovable"] = weapon.IsNonRemovable,
                                    ["planned"] = planned.Count,
                                    ["plan"] = new JArray(planned.Select(value =>
                                        new JObject { ["hand"] = value?.Hand?.ToString(),
                                            ["penalty"] = value?.AttackBonusPenalty,
                                            ["number"] = value?.AttackNumber })),
                                    ["attack"] = bonus,
                                    ["damage"] = Sprint16DamageLine(damage),
                                    ["forms"] = new JArray(stats.DamageDescription.Select(
                                        value => Sprint16DamageLine(value.CreateDamage()))),
                                    ["configuredIncrementFeet"] = weapon.Blueprint.Type.AttackRange.Value,
                                    ["nativeEquippedRangeFeet"] = weapon.AttackRange.Value },
                                "Native active mode resolves only its own printed attacks, damage forms and range increment.");
                            int original = owner.Stats.Strength.BaseValue;
                            var signed = new JArray();
                            bool signedExact = true;
                            try
                            {
                                foreach (int score in new[] { original, original+1, 10, 8 })
                                {
                                    owner.Stats.Strength.BaseValue = score;
                                    int modifier = owner.Stats.Strength.Bonus;
                                    var live = Rulebook.Trigger(new RuleCalculateWeaponStats(
                                        owner, weapon, null));
                                    var physical = live.DamageDescription[0].CreateDamage();
                                    int expected = index == 1 ? modifier :
                                        GiantHumanoidRulesPolicy.OneAndHalfStrength(modifier);
                                    bool row = physical.Bonus == expected &&
                                        live.DamageDescription.Count ==
                                        (index == 2 && spec.Key == "fire-giant" ? 2 : 1);
                                    signedExact &= row;
                                    signed.Add(new JObject { ["strength"] = score,
                                        ["modifier"] = modifier, ["expected"] = expected,
                                        ["physical"] = Sprint16DamageLine(physical),
                                        ["forms"] = live.DamageDescription.Count,
                                        ["exact"] = row });
                                }
                            }
                            finally { owner.Stats.Strength.BaseValue = original; }
                            Sprint25Check("s28-"+spec.Key+"-strength-"+index,
                                signedExact, new JObject { ["owner"] = owner.UniqueId,
                                    ["mode"] = index, ["rows"] = signed,
                                    ["forcedRolls"] = 0 },
                                "Native weapon-stat pipeline applies live signed Strength once for manufactured, slam and thrown-rock modes.");
                        }
                    }
                    finally
                    {
                        owner.Commands.InterruptAll(true);
                        if (owner.IsInCombat) owner.LeaveCombat();
                        DisposeExpandedSummoningUnits(fixture.Created, new[] { owner });
                    }
                }
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