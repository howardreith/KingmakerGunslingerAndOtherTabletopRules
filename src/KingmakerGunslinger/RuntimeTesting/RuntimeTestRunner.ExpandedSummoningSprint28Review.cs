using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.SettingsUI;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private bool IsSprint28Review { get { return _request.Scenario ==
            RuntimeTestScenarioCatalog.DisposableExpandedSummoningSprint28NativeReview; } }

        private IEnumerable<int> ReviewSprint28(ExpandedSummoningCorrectionFixture fixture)
        {
            string phase = RuntimeTestRequestParser.Sprint28ReviewPhase(_request);
            bool mode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool pause = Game.Instance.IsPaused;
            Sprint25Check("s28-phase", phase == "profiles" || phase == "modes" ||
                phase == "commands" || phase == "ownership" ||
                phase == "catch" || phase == "ai", new JObject {
                ["phase"] = phase }, "One exact implemented Sprint 28 review phase.");
            if (phase == "modes")
            {
                foreach (int frame in ReviewSprint28Modes(fixture)) yield return frame;
                yield break;
            }
            if (phase == "commands")
            {
                foreach (int frame in ReviewSprint28Commands(fixture)) yield return frame;
                yield break;
            }
            if (phase == "ownership")
            {
                foreach (int frame in ReviewSprint28Ownership(fixture)) yield return frame;
                yield break;
            }
            if (phase == "catch")
            {
                foreach (int frame in ReviewSprint28Catch(fixture)) yield return frame;
                yield break;
            }
            if (phase == "ai")
            {
                foreach (int frame in ReviewSprint28Ai(fixture)) yield return frame;
                yield break;
            }
            try
            {
                Game.Instance.IsPaused = true;
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = false;
                for (int i = 0; i < 16; i++) yield return 0;
                foreach (var spec in new[] {
                    new { Key="hill-giant", Token="HillGiant", Scores=new[]{25,8,19,6,10,7},
                        Bab=7, Hp=85, Ac=21, Touch=8, Flat=21, Fort=11, Reflex=2, Will=3,
                        Speed=30, Attack=14, Damage=10, Dice=DiceType.D8, Rolls=2, Armor=true },
                    new { Key="stone-giant", Token="StoneGiant", Scores=new[]{27,15,19,10,12,10},
                        Bab=9, Hp=102, Ac=22, Touch=11, Flat=20, Fort=12, Reflex=6, Will=7,
                        Speed=40, Attack=16, Damage=12, Dice=DiceType.D8, Rolls=2, Armor=false },
                    new { Key="fire-giant", Token="FireGiant", Scores=new[]{31,9,21,10,14,10},
                        Bab=11, Hp=142, Ac=24, Touch=8, Flat=24, Fort=14, Reflex=4, Will=9,
                        Speed=30, Attack=21, Damage=15, Dice=DiceType.D6, Rolls=3, Armor=true }
                })
                {
                    UnitEntityData owner = CastExpandedSummoningOwnTier(fixture, spec.Key);
                    SetExpandedSummoningBrainActive(owner, false);
                    try
                    {
                        for (int i = 0; i < 8; i++) yield return 0;
                        var stats = owner.Stats;
                        int[] scores = {stats.Strength.ModifiedValue, stats.Dexterity.ModifiedValue,
                            stats.Constitution.ModifiedValue, stats.Intelligence.ModifiedValue,
                            stats.Wisdom.ModifiedValue, stats.Charisma.ModifiedValue};
                        bool profile = scores.SequenceEqual(spec.Scores) &&
                            owner.Descriptor.State.Size == Size.Large &&
                            stats.BaseAttackBonus.ModifiedValue == spec.Bab &&
                            stats.HitPoints.ModifiedValue == spec.Hp &&
                            stats.AC.ModifiedValue == spec.Ac && stats.AC.Touch == spec.Touch &&
                            stats.AC.FlatFooted == spec.Flat &&
                            stats.SaveFortitude.ModifiedValue == spec.Fort &&
                            stats.SaveReflex.ModifiedValue == spec.Reflex &&
                            stats.SaveWill.ModifiedValue == spec.Will &&
                            stats.Speed.ModifiedValue == spec.Speed;
                        Sprint25Check("s28-"+spec.Key+"-profile", profile, new JObject {
                            ["owner"] = owner.UniqueId, ["scores"] = new JArray(scores),
                            ["size"] = owner.Descriptor.State.Size.ToString(),
                            ["bab"] = stats.BaseAttackBonus.ModifiedValue,
                            ["hp"] = stats.HitPoints.ModifiedValue,
                            ["ac"] = stats.AC.ModifiedValue, ["touch"] = stats.AC.Touch,
                            ["flat"] = stats.AC.FlatFooted,
                            ["fort"] = stats.SaveFortitude.ModifiedValue,
                            ["reflex"] = stats.SaveReflex.ModifiedValue,
                            ["will"] = stats.SaveWill.ModifiedValue,
                            ["speed"] = stats.Speed.ModifiedValue,
                            ["perception"] = DescribeSprint16Skill(stats.SkillPerception),
                            ["persuasion"] = DescribeSprint16Skill(stats.SkillPersuasion),
                            ["stealth"] = DescribeSprint16Skill(stats.SkillStealth)
                        }, "Actual adult giant instance has its printed base profile with live class, size, feat and equipment effects.");
                        // The native non-player Power Attack watcher starts this
                        // toggle on. Printed PF1 stat blocks compare with it off;
                        // preserve and test the actual native ON contribution.
                        var toggle = owner.Descriptor.ActivatableAbilities.Enumerable
                            .Single(value => value.Blueprint.AssetGuid ==
                                "a7b339e4f6ff93a4697df5d7a87ff619");
                        JObject initialPowerAttack = Sprint22PowerAttackState(owner, toggle);
                        foreach (int frame in Sprint22SetPowerAttack(owner, false)) yield return frame;
                        var weapon = owner.Body.PrimaryHand.MaybeWeapon;
                        var calculated = Rulebook.Trigger(new RuleCalculateWeaponStats(owner, weapon, null));
                        var damage = calculated.DamageDescription[0].CreateDamage();
                        int attack = Rulebook.Trigger(new RuleCalculateAttackBonusWithoutTarget(owner, weapon, 0)).Result;
                        bool weaponExact = attack == spec.Attack && damage.Dice.Rolls == spec.Rolls &&
                            damage.Dice.Dice == spec.Dice && damage.Bonus == spec.Damage;
                        Sprint25Check("s28-"+spec.Key+"-weapon", weaponExact, new JObject {
                            ["owner"] = owner.UniqueId, ["item"] = weapon.Blueprint.AssetGuid,
                            ["itemOwner"] = weapon.Owner == owner.Descriptor,
                            ["attack"] = attack, ["damage"] = Sprint16DamageLine(damage),
                            ["nativeStrengthMultiplier"] = calculated.DamageBonusStatMultiplier,
                            ["damageForms"] = calculated.DamageDescription.Count,
                            ["initialPowerAttack"] = initialPowerAttack,
                            ["offPowerAttack"] = Sprint22PowerAttackState(owner, toggle)
                        }, "Actual equipped Large manufactured weapon resolves its printed line with native Power Attack temporarily off.");
                        foreach (int frame in Sprint22SetPowerAttack(owner, true)) yield return frame;
                        int onAttack = Rulebook.Trigger(new RuleCalculateAttackBonusWithoutTarget(
                            owner, weapon, 0)).Result;
                        int onDamage = Rulebook.Trigger(new RuleCalculateWeaponStats(
                            owner, weapon, null)).DamageDescription[0].CreateDamage().Bonus;
                        int penalty = 1 + spec.Bab / 4;
                        Sprint25Check("s28-"+spec.Key+"-power-attack",
                            toggle.IsOn && toggle.IsRunning && onAttack == attack - penalty &&
                            onDamage == damage.Bonus + 3 * penalty,
                            new JObject { ["owner"] = owner.UniqueId,
                                ["offAttack"] = attack, ["offDamage"] = damage.Bonus,
                                ["onAttack"] = onAttack, ["onDamage"] = onDamage,
                                ["nativeState"] = Sprint22PowerAttackState(owner, toggle) },
                            "The actual native two-handed Power Attack toggle adds its BAB-scaled attack penalty and damage once.");
                        foreach (int frame in Sprint22SetPowerAttack(owner, false)) yield return frame;
                        var icon = ExpandedSummoningProjectIcons.Require(spec.Key);
                        var routes = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly)
                            .Where(value => value.Creature.Key == spec.Key).ToArray();
                        bool icons = ReferenceEquals(owner.Blueprint.Type.Image, icon) &&
                            routes.Length == (spec.Key == "fire-giant" ? 3 : 4) &&
                            routes.All(value => ReferenceEquals(
                                ExpandedSummoningRoot(fixture.Blueprints, value).Icon, icon));
                        Sprint25Check("s28-"+spec.Key+"-icon", icons, new JObject {
                            ["owner"] = owner.UniqueId, ["routes"] = routes.Length,
                            ["loadedSprite"] = icon.GetInstanceID(),
                            ["typeSprite"] = owner.Blueprint.Type.Image?.GetInstanceID(),
                            ["fallbacks"] = ExpandedSummoningProjectIcons.FallbackCount
                        }, "Unit type and all exact SNA route icons use the original species painting.");
                        var meshes = owner.View.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                            .Where(value => value.enabled && value.sharedMesh != null).ToArray();
                        bool body = meshes.Length > 0 && (bool)Sprint22SelectionSurface(owner)["selectionSurface"];
                        Sprint25Check("s28-"+spec.Key+"-body", body, new JObject {
                            ["owner"] = owner.UniqueId,
                            ["prefab"] = owner.Blueprint.Prefab?.AssetId,
                            ["meshes"] = new JArray(meshes.Select(value => value.sharedMesh.name)),
                            ["selection"] = Sprint22SelectionSurface(owner),
                            ["camera"] = WriteExpandedSummoningPartyCameraCapture(owner,
                                _request.EvidenceDirectory, "s28-"+spec.Token+"-"+Time.frameCount+".png")
                        }, "Actual native giant mesh and selectable footprint are present in the live first-area scene.");
                        var armor = owner.Body.Armor == null ? null : owner.Body.Armor.MaybeArmor;
                        bool equipment = weapon.Owner == owner.Descriptor &&
                            (spec.Armor ? armor != null && armor.Owner == owner.Descriptor : armor == null);
                        Sprint25Check("s28-"+spec.Key+"-equipment", equipment, new JObject {
                            ["owner"] = owner.UniqueId,
                            ["weapon"] = weapon.Blueprint.AssetGuid,
                            ["weaponEntity"] = RuntimeHelpers.GetHashCode(weapon),
                            ["weaponNonRemovable"] = weapon.IsNonRemovable,
                            ["armor"] = armor?.Blueprint.AssetGuid,
                            ["armorEntity"] = armor == null ? 0 : RuntimeHelpers.GetHashCode(armor),
                            ["armorNonRemovable"] = armor?.IsNonRemovable
                        }, "Only the exact giant owns its equipped weapon and printed armor; corpse and transfer checks remain separate.");
                        if (spec.Key == "fire-giant")
                        {
                            int injury = owner.Descriptor.Damage;
                            try
                            {
                                foreach (DamageEnergyType energy in new[] {
                                    DamageEnergyType.Fire, DamageEnergyType.Cold })
                                {
                                    var packet = new EnergyDamage(
                                        new DiceFormula(1, DiceType.D6), energy)
                                        { PreRolledValue = 6 };
                                    var rule = Rulebook.Trigger(new RuleDealDamage(
                                        fixture.Caster, owner,
                                        new DamageBundle(packet)));
                                    var result = rule.ResultDamage.Single(value =>
                                        ReferenceEquals(value.Source, packet));
                                    bool correct = energy == DamageEnergyType.Fire
                                        ? packet.Immune && result.FinalValue == 0 &&
                                          rule.Damage == 0
                                        : !packet.Immune &&
                                          result.FinalValue == 9 &&
                                          rule.Damage > 0;
                                    Sprint25Check("s28-fire-" + energy +
                                        "-defense", correct, new JObject {
                                        ["owner"] = owner.UniqueId,
                                        ["source"] = fixture.Caster.UniqueId,
                                        ["energy"] = energy.ToString(),
                                        ["fixturePreRoll"] = 6,
                                        ["nativeImmune"] = packet.Immune,
                                        ["finalBeforeDifficulty"] =
                                            result.FinalValue,
                                        ["nativeDamage"] = rule.Damage
                                    }, "Actual owned Fire Giant receives native fire immunity and cold vulnerability through resolved energy damage.");
                                }
                            }
                            finally { owner.Descriptor.Damage = injury; }
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