using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Kingmaker;
using Kingmaker.Blueprints.Area;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.SettingsUI;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
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
                phase == "commands" || phase == "turns" || phase == "ownership" ||
                phase == "disarm" || phase == "catch" || phase == "ai" ||
                phase == "ai-turns", new JObject {
                ["phase"] = phase }, "One exact implemented Sprint 28 review phase.");
            if (phase == "modes")
            {
                foreach (int frame in ReviewSprint28Modes(fixture)) yield return frame;
                yield break;
            }
            if (phase == "commands")
            {
                foreach (int frame in ReviewSprint28Commands(fixture, false)) yield return frame;
                yield break;
            }
            if (phase == "turns")
            {
                foreach (int frame in ReviewSprint28Commands(fixture, true)) yield return frame;
                yield break;
            }
            if (phase == "ownership")
            {
                foreach (int frame in ReviewSprint28Ownership(fixture)) yield return frame;
                yield break;
            }
            if (phase == "disarm")
            {
                foreach (int frame in ReviewSprint28Disarm(fixture)) yield return frame;
                yield break;
            }
            if (phase == "catch")
            {
                foreach (int frame in ReviewSprint28Catch(fixture)) yield return frame;
                yield break;
            }
            if (phase == "ai")
            {
                foreach (int frame in ReviewSprint28Ai(fixture, false)) yield return frame;
                yield break;
            }
            if (phase == "ai-turns")
            {
                foreach (int frame in ReviewSprint28Ai(fixture, true)) yield return frame;
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
                        // A mesh can exist while its summon appearance shader
                        // is fully dissolved. Wait for this exact native view
                        // to settle before treating a frame as body evidence.
                        int appearanceFrames = 0;
                        DateTime appearanceDeadline = DateTime.UtcNow.AddSeconds(25);
                        while (appearanceFrames < MotionReviewFadeBudget &&
                            DateTime.UtcNow < appearanceDeadline &&
                            (LoadingOrScreenFadeActive() ||
                             !EntityFadedIn(owner) ||
                             DissolveAmount(owner) > MotionReviewIntactDissolve ||
                             owner.Descriptor.Buffs.GetBuff(Kingmaker.Blueprints
                                .Root.BlueprintRoot.Instance.SystemMechanics
                                .SummonedUnitAppearBuff) != null))
                        {
                            appearanceFrames++;
                            yield return 0;
                        }
                        bool visible = !LoadingOrScreenFadeActive() &&
                            EntityFadedIn(owner) &&
                            DissolveAmount(owner) <= MotionReviewIntactDissolve &&
                            owner.Descriptor.Buffs.GetBuff(Kingmaker.Blueprints
                                .Root.BlueprintRoot.Instance.SystemMechanics
                                .SummonedUnitAppearBuff) == null;
                        Sprint25Check("s28-" + spec.Key + "-visible-body",
                            visible, new JObject { ["owner"] = owner.UniqueId,
                                ["appearanceFrames"] = appearanceFrames,
                                ["screenFadeActive"] = LoadingOrScreenFadeActive(),
                                ["faderVisible"] = EntityFadedIn(owner),
                                ["dissolve"] = DissolveAmount(owner),
                                ["appearBuff"] = owner.Descriptor.Buffs.GetBuff(
                                    Kingmaker.Blueprints.Root.BlueprintRoot
                                        .Instance.SystemMechanics
                                        .SummonedUnitAppearBuff) != null,
                                ["variant"] = ExpandedSummoningVisualVariantPatch
                                    .DescribeView(owner.View),
                                ["variantOwnership"] =
                                    ExpandedSummoningVisualVariantPatch
                                        .DescribeOwnership(owner.View),
                                ["bodyMaterials"] = new JArray(owner.View
                                    .GetComponentsInChildren<SkinnedMeshRenderer>(true)
                                    .SelectMany(renderer => renderer.sharedMaterials)
                                    .Where(material => material != null)
                                    .Take(6).Select(material => new JObject {
                                        ["name"] = material.name,
                                        ["color"] = material.HasProperty("_Color")
                                            ? material.GetColor("_Color").ToString()
                                            : "<none>",
                                        ["tint"] = material.HasProperty("_TintColor")
                                            ? material.GetColor("_TintColor").ToString()
                                            : "<none>" })) },
                            "The exact giant's summoned native body finishes its appearance and is no longer shader-dissolved.");
                        string variantOutcome = ExpandedSummoningVisualVariantPatch
                            .DescribeView(owner.View);
                        bool shouldTint = spec.Key != "hill-giant";
                        Sprint25Check("s28-" + spec.Key + "-variant-bound",
                            shouldTint
                                ? variantOutcome.StartsWith("variant:applied;",
                                    StringComparison.Ordinal) &&
                                  ExpandedSummoningVisualVariantPatch
                                    .DescribeOwnership(owner.View) != "<none>"
                                : variantOutcome == "<none>",
                            new JObject { ["owner"] = owner.UniqueId,
                                ["variant"] = variantOutcome,
                                ["ownership"] = ExpandedSummoningVisualVariantPatch
                                    .DescribeOwnership(owner.View) },
                            "Only Stone and Fire giant views bind their private visual variant; Hill remains the native donor look.");
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
                        var intimidateCheck = Rulebook.Trigger(
                            new RuleSkillCheck(owner, StatType.CheckIntimidate, 100)
                            { IgnoreDifficultyBonusToDC = true });
                        int printedIntimidate = spec.Key == "hill-giant" ||
                            spec.Key == "stone-giant" ? 12 : 11;
                        // Kingmaker's RuleSkillCheck.StatValue excludes its
                        // event Bonus; BaseRollResult includes both plus D20.
                        int intimidateBonus = (int)intimidateCheck.Bonus;
                        int effectiveIntimidate = intimidateCheck.StatValue +
                            intimidateBonus;
                        Sprint25Check("s28-" + spec.Key + "-intimidate",
                            effectiveIntimidate == printedIntimidate &&
                            intimidateBonus == 3 &&
                            intimidateCheck.BaseRollResult -
                                (int)intimidateCheck.D20 == effectiveIntimidate &&
                            !(stats.SkillPersuasion as
                                ModifiableValueSkill).ClassSkill,
                            new JObject { ["owner"] = owner.UniqueId,
                                ["ranks"] = stats.SkillPersuasion.BaseValue,
                                ["rawPersuasion"] =
                                    stats.SkillPersuasion.ModifiedValue,
                                ["baseStatValue"] = intimidateCheck.StatValue,
                                ["eventBonus"] = intimidateBonus,
                                ["nativeRollModifier"] =
                                    intimidateCheck.BaseRollResult -
                                    (int)intimidateCheck.D20,
                                ["nativeIntimidate"] = effectiveIntimidate,
                                ["printedIntimidate"] = printedIntimidate },
                            "Native Intimidate consumes the giant subtype's class-skill bonus once without raising other Persuasion uses.");
                        string terrainSetting = Game.Instance.CurrentlyLoadedArea
                            ?.LootSetting.ToString();
                        int expectedTerrain = spec.Key == "stone-giant" &&
                            GiantHumanoidRulesPolicy.IsStoneRockyTerrain(
                                terrainSetting)
                            ? GiantHumanoidRulesPolicy.StoneRockyStealthBonus : 0;
                        var stealthCheck = Rulebook.Trigger(
                            new RuleSkillCheck(owner, StatType.SkillStealth, 100)
                            { IgnoreDifficultyBonusToDC = true });
                        int actualTerrain = (int)stealthCheck.Bonus;
                        int effectiveStealth = stealthCheck.StatValue +
                            actualTerrain;
                        Sprint25Check("s28-" + spec.Key + "-terrain-stealth",
                            actualTerrain == expectedTerrain &&
                            stealthCheck.BaseRollResult -
                                (int)stealthCheck.D20 == effectiveStealth &&
                            (spec.Key != "stone-giant" ||
                                stats.SkillStealth.ModifiedValue == 4 &&
                                effectiveStealth == 4 + expectedTerrain),
                            new JObject { ["owner"] = owner.UniqueId,
                                ["area"] = Game.Instance.CurrentlyLoadedArea?.AssetGuid,
                                ["setting"] = terrainSetting,
                                ["rawStealth"] = stats.SkillStealth.ModifiedValue,
                                ["baseStatValue"] = stealthCheck.StatValue,
                                ["checkStealth"] = effectiveStealth,
                                ["eventBonus"] = actualTerrain,
                                ["nativeRollModifier"] =
                                    stealthCheck.BaseRollResult -
                                    (int)stealthCheck.D20,
                                ["expectedBonus"] = expectedTerrain },
                            "The native skill-check consumer grants only Stone's selected-area conditional racial Stealth bonus.");
                        if (spec.Key == "stone-giant")
                            foreach (string setting in new[] {
                                "Hills", "NaturalCaves" })
                            {
                                JObject terrain = Sprint28StoneAreaSkillCheck(
                                    owner, setting);
                                Sprint25Check("s28-stone-giant-" + setting +
                                    "-stealth", (int)terrain["rawStealth"] == 4 &&
                                    (int)terrain["checkStealth"] == 12 &&
                                    (int)terrain["nativeRollModifier"] == 12 &&
                                    (int)terrain["eventBonus"] == 8 &&
                                    (bool)terrain["restored"], terrain,
                                    "Real installed area metadata supplies the selected rocky setting to Stone's native skill check and is restored in the same call.");
                            }
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
                                _request.EvidenceDirectory, "s28-"+spec.Token+"-"+Time.frameCount+".png"),
                            ["oblique"] = WriteExpandedSummoningOverheadStrikeCapture(
                                owner, owner, _request.EvidenceDirectory,
                                "s28-"+spec.Token+"-oblique-"+Time.frameCount+".png",
                                cameraOffset: new Vector3(5f, 6f, -5f))
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
                            if (fixture.Hostile == null)
                                CreateExpandedSummoningCorrectionHostile(fixture);
                            UnitEntityData source = fixture.Hostile;
                            bool legal = source != null &&
                                !source.IsPlayerFaction &&
                                source.CanAttack(owner);
                            Sprint25Check("s28-fire-defense-source", legal,
                                new JObject {
                                    ["source"] = source?.UniqueId,
                                    ["sourcePlayerFaction"] =
                                        source?.IsPlayerFaction,
                                    ["target"] = owner.UniqueId,
                                    ["canAttack"] = source != null &&
                                        source.CanAttack(owner)
                                }, "The incoming energy source is an exact disposable hostile that native damage permits to attack this giant.");
                            if (!legal) throw new InvalidOperationException(
                                "Fire defense lacks a legal hostile damage source.");
                            // Grant and arithmetic are native consumers; actual
                            // Overrun/Sunder commands have a separate review.
                            string[] actionGuids = owner.Descriptor.Abilities
                                .Enumerable.Where(value => value?.Blueprint != null)
                                .Select(value => value.Blueprint.AssetGuid)
                                .ToArray();
                            bool actions = actionGuids.Contains(
                                "1a3b471ecea51f7439a946b23577fd70") &&
                                actionGuids.Contains(
                                "fa9bfb9fd997faf49a108c4b17a00504");
                            Sprint25Check("s28-fire-maneuver-grants", actions,
                                new JObject { ["owner"] = owner.UniqueId,
                                    ["overrun"] = actionGuids.Contains(
                                        "1a3b471ecea51f7439a946b23577fd70"),
                                    ["sunder"] = actionGuids.Contains(
                                        "fa9bfb9fd997faf49a108c4b17a00504") },
                                "Fire's Improved Overrun and Improved Sunder grant their actual native commands to the exact summon.");
                            int ordinaryCmb = Rulebook.Trigger(new RuleCalculateCMB(
                                owner, source, CombatManeuver.Trip)).Result;
                            int overrunCmb = Rulebook.Trigger(new RuleCalculateCMB(
                                owner, source, CombatManeuver.Overrun)).Result;
                            int sunderCmb = Rulebook.Trigger(new RuleCalculateCMB(
                                owner, source, CombatManeuver.SunderArmor)).Result;
                            Sprint25Check("s28-fire-maneuver-attack", overrunCmb ==
                                ordinaryCmb + 2 && sunderCmb == ordinaryCmb + 2,
                                new JObject { ["owner"] = owner.UniqueId,
                                    ["ordinary"] = ordinaryCmb,
                                    ["overrun"] = overrunCmb,
                                    ["sunder"] = sunderCmb },
                                "Native CMB grants each exact printed Improved maneuver +2 only for its own attack check.");
                            int ordinaryCmd = Rulebook.Trigger(new RuleCalculateCMD(
                                source, owner, CombatManeuver.Trip)).Result;
                            int overrunCmd = Rulebook.Trigger(new RuleCalculateCMD(
                                source, owner, CombatManeuver.Overrun)).Result;
                            int sunderCmd = Rulebook.Trigger(new RuleCalculateCMD(
                                source, owner, CombatManeuver.SunderArmor)).Result;
                            Sprint25Check("s28-fire-maneuver-defense", overrunCmd ==
                                ordinaryCmd + 2 && sunderCmd == ordinaryCmd + 2,
                                new JObject { ["owner"] = owner.UniqueId,
                                    ["ordinary"] = ordinaryCmd,
                                    ["overrun"] = overrunCmd,
                                    ["sunder"] = sunderCmd },
                                "Native CMD grants +2 to Fire's Overrun and Sunder defenses without raising unrelated Trip.");
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
                                        source, owner,
                                        new DamageBundle(packet)));
                                    var result = rule.ResultDamage?.SingleOrDefault(
                                        value => ReferenceEquals(value.Source,
                                            packet));
                                    bool correct = energy == DamageEnergyType.Fire
                                        ? packet.Immune && result.HasValue &&
                                          result.Value.FinalValue == 6 &&
                                          rule.Damage == 0
                                        : !packet.Immune && result.HasValue &&
                                          result.Value.FinalValue == 9 &&
                                          rule.Damage > 0;
                                    Sprint25Check("s28-fire-" + energy +
                                        "-defense", correct, new JObject {
                                        ["owner"] = owner.UniqueId,
                                        ["source"] = source.UniqueId,
                                        ["energy"] = energy.ToString(),
                                        ["fixturePreRoll"] = 6,
                                        ["nativeImmune"] = packet.Immune,
                                        ["finalBeforeDifficulty"] =
                                            result?.FinalValue,
                                        ["nativeDamage"] = rule.Damage
                                    }, "Actual owned Fire Giant receives native fire immunity and cold vulnerability through resolved energy damage.");
                                }
                                // Let native queued hit effects settle while both exact fixture actors are live.
                                for (int settled = 0; settled < 3; settled++) yield return 0;
                                Sprint25Check("s28-fire-defense-settled",
                                    !owner.Destroyed && owner.View != null &&
                                    !source.Destroyed && source.View != null,
                                    new JObject { ["owner"] = owner.UniqueId,
                                        ["source"] = source.UniqueId,
                                        ["frames"] = 3 },
                                    "Native damage effects settle before request-owned actor cleanup.");
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
        // The fixture is Forest. Borrow a real installed area blueprint's
        // metadata for one synchronous native skill check; no scene, area
        // state, party or save changes. Restore before the next game frame.
        private static JObject Sprint28StoneAreaSkillCheck(
            UnitEntityData owner, string setting)
        {
            Game game = Game.Instance;
            BlueprintArea before = game.CurrentlyLoadedArea;
            object state = game.State.LoadedAreaState;
            if (before == null || before.LootSetting.ToString() != "Forest" ||
                owner == null || owner.Blueprint == null ||
                owner.Stats.SkillStealth.ModifiedValue != 4 ||
                !GiantHumanoidRulesPolicy.IsStoneRockyTerrain(setting))
                throw new InvalidOperationException(
                    "Stone terrain metadata preconditions are absent.");
            BlueprintArea area = BlueprintBootstrap.Library.GetAllBlueprints()
                .OfType<BlueprintArea>()
                .Where(candidate => candidate != null && !candidate.IsCapital &&
                    candidate.LootSetting.ToString() == setting)
                .OrderBy(candidate => candidate.AssetGuid,
                    StringComparer.Ordinal).FirstOrDefault();
            if (area == null || ReferenceEquals(area, before))
                throw new InvalidOperationException(
                    "No distinct installed native area metadata for " + setting);
            FieldInfo field = typeof(Game).GetField("m_SceneLoader",
                BindingFlags.Instance | BindingFlags.NonPublic);
            object loader = field == null ? null : field.GetValue(game);
            PropertyInfo property = loader == null ? null :
                loader.GetType().GetProperty("CurrentlyLoadedArea",
                    BindingFlags.Instance | BindingFlags.Public |
                    BindingFlags.NonPublic);
            MethodInfo setter = property == null ? null :
                property.GetSetMethod(true);
            if (setter == null || property.PropertyType != typeof(BlueprintArea))
                throw new MissingMethodException(
                    "SceneLoader.CurrentlyLoadedArea exact setter");
            int value = 0;
            int nativeRollModifier = 0;
            bool observed = false;
            try
            {
                setter.Invoke(loader, new object[] { area });
                if (!ReferenceEquals(game.CurrentlyLoadedArea, area) ||
                    !ReferenceEquals(game.State.LoadedAreaState, state))
                    throw new InvalidOperationException(
                        "Request-local terrain metadata did not bind exactly.");
                var check = Rulebook.Trigger(new RuleSkillCheck(
                    owner, StatType.SkillStealth, 100) {
                    IgnoreDifficultyBonusToDC = true });
                value = check.StatValue + (int)check.Bonus;
                nativeRollModifier = check.BaseRollResult - (int)check.D20;
                observed = true;
            }
            finally
            {
                setter.Invoke(loader, new object[] { before });
                if (!ReferenceEquals(game.CurrentlyLoadedArea, before) ||
                    !ReferenceEquals(game.State.LoadedAreaState, state))
                    throw new InvalidOperationException(
                        "Native area metadata was not restored exactly.");
            }
            return new JObject { ["owner"] = owner.UniqueId,
                ["loadedAreaBefore"] = before.AssetGuid,
                ["borrowedNativeArea"] = area.AssetGuid,
                ["setting"] = setting,
                ["rawStealth"] = owner.Stats.SkillStealth.ModifiedValue,
                ["checkStealth"] = value,
                ["nativeRollModifier"] = nativeRollModifier,
                ["eventBonus"] = value -
                    owner.Stats.SkillStealth.ModifiedValue,
                ["observed"] = observed,
                ["restored"] = ReferenceEquals(
                    game.CurrentlyLoadedArea, before) &&
                    ReferenceEquals(game.State.LoadedAreaState, state) };
        }

    }
}
