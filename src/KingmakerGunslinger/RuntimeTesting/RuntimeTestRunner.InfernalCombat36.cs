using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Brain;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UI.LevelUp;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> ReviewInfernalCombat36(
            ExpandedSummoningCorrectionFixture fixture)
        {
            var owned = new List<UnitEntityData>();
            var cells = new JArray();
            var trace = new InfernalEvents34();
            var receipt = new JObject {
                ["phase"] = ExpandedSummoningNativeProfilePolicy
                    .Sprint36InfernalCombatPhase,
                ["finalQualification"] = false,
                ["cells"] = cells,
                ["events"] = trace.Rows
            };
            UnitEntityData target = fixture.Hostile;
            UnitEntityData ally = fixture.Caster;
            Alignment targetAlignment = target.Descriptor.Alignment.Value;
            Alignment allyAlignment = ally.Descriptor.Alignment.Value;
            int targetAc = target.Stats.AC.BaseValue;
            int targetWill = target.Stats.SaveWill.BaseValue;
            int targetFort = target.Stats.SaveFortitude.BaseValue;
            int targetHp = target.Stats.HitPoints.BaseValue;
            int targetCmd = target.Stats.AdditionalCMD.BaseValue;
            int targetDamage = target.Damage;
            Size targetSize = target.Descriptor.State.Size;
            int allyWill = ally.Stats.SaveWill.BaseValue;
            int allyHp = ally.Stats.HitPoints.BaseValue;
            int allyDamage = ally.Damage;
            Vector3 targetPosition = target.Position;
            Vector3 allyPosition = ally.Position;
            Action<string, bool, JObject, string> check =
                (id, pass, row, description) => {
                    row["cell"] = id;
                    cells.Add(row);
                    Sprint25Check("sprint36-" + id, pass, row,
                        description);
                    if (!pass) throw new InvalidOperationException(
                        "Sprint36 infernal combat cell failed: " + id);
                };
            try
            {
                target.Stats.AC.BaseValue = -100;
                target.Stats.HitPoints.BaseValue = 100000;
                target.Stats.AdditionalCMD.BaseValue = -100;

                UnitEntityData babau = CastExpandedSummoningOwnTier(
                    fixture, "babau");
                owned.Add(babau);
                Sprint28SetBrainActivePreservingActions(babau, false);
                babau.Commands.InterruptAll(true);
                babau.Commands.RemoveFinishedAndUpdateQueue();
                babau.Stats.HitPoints.BaseValue = 100000;
                babau.Stats.AdditionalAttackBonus.BaseValue = 100;
                foreach (int frame in Outsider30NativeAppearance(babau,
                    babau.Blueprint.name)) yield return frame;
                foreach (int frame in InfernalBabauCombat36(fixture, babau,
                    target, trace, check)) yield return frame;
                DisposeExpandedSummoningUnits(fixture.Created,
                    new[] { babau });
                owned.Remove(babau);
                ResetExpandedSummoningHostile(fixture);
                target.Stats.AC.BaseValue = -100;
                target.Stats.HitPoints.BaseValue = 100000;
                target.Stats.AdditionalCMD.BaseValue = -100;

                UnitEntityData hezrou = CastExpandedSummoningOwnTier(
                    fixture, "hezrou");
                owned.Add(hezrou);
                Sprint28SetBrainActivePreservingActions(hezrou, false);
                hezrou.Commands.InterruptAll(true);
                hezrou.Commands.RemoveFinishedAndUpdateQueue();
                hezrou.Stats.HitPoints.BaseValue = 100000;
                hezrou.Stats.AdditionalAttackBonus.BaseValue = 100;
                hezrou.Stats.AdditionalCMB.BaseValue = 100;
                foreach (int frame in Outsider30NativeAppearance(hezrou,
                    hezrou.Blueprint.name)) yield return frame;
                foreach (int frame in InfernalHezrouMelee36(fixture,
                    hezrou, target, trace, check)) yield return frame;
                foreach (int frame in InfernalHezrouSpells36(fixture,
                    hezrou, target, ally, trace, check)) yield return frame;
                foreach (int frame in InfernalHezrouFallback36(fixture,
                    hezrou, target, ally, trace, check)) yield return frame;
            }
            finally
            {
                target.Descriptor.Alignment.Set(targetAlignment);
                ally.Descriptor.Alignment.Set(allyAlignment);
                target.Stats.AC.BaseValue = targetAc;
                target.Stats.SaveWill.BaseValue = targetWill;
                target.Stats.SaveFortitude.BaseValue = targetFort;
                target.Stats.HitPoints.BaseValue = targetHp;
                target.Stats.AdditionalCMD.BaseValue = targetCmd;
                target.Descriptor.State.Size = targetSize;
                target.Damage = targetDamage;
                ally.Stats.SaveWill.BaseValue = allyWill;
                ally.Stats.HitPoints.BaseValue = allyHp;
                ally.Damage = allyDamage;
                PlaceExpandedSummoningUnit(target, targetPosition);
                PlaceExpandedSummoningUnit(ally, allyPosition);
                foreach (UnitEntityData actor in owned.Where(value =>
                    value != null && !value.Destroyed))
                {
                    SummonGrappleLinks.ReleaseAll(actor);
                    Sprint28SetBrainActivePreservingActions(actor, false);
                    actor.Commands.InterruptAll(true);
                    actor.Commands.RemoveFinishedAndUpdateQueue();
                    if (actor.IsInCombat) actor.LeaveCombat();
                }
                if (target.IsInCombat) target.LeaveCombat();
                Game.Instance.Player.UpdateIsInCombat();
                trace.Dispose();
                DisposeExpandedSummoningUnits(fixture.Created, owned);
                ResetExpandedSummoningHostile(fixture);
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(
                    _request.EvidenceDirectory,
                    "sprint36-infernal-combat.json"), receipt.ToString(
                        Formatting.Indented));
            }
        }

        private IEnumerable<int> InfernalBabauCombat36(
            ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData babau, UnitEntityData target,
            InfernalEvents34 trace,
            Action<string, bool, JObject, string> check)
        {
            InfernalMartialProfile36 profile =
                InfernalMartialProfile36.For(babau);
            if (profile == null) throw new InvalidOperationException(
                "Babau combat profile is absent.");
            ItemEntityWeapon bite = babau.Body.AdditionalLimbs.Select(
                value => value.MaybeWeapon).Single(value => value != null &&
                    ReferenceEquals(value.Blueprint, profile.Bite));
            ItemEntityWeapon secondClaw = babau.Body.AdditionalLimbs.Select(
                value => value.MaybeWeapon).Single(value => value != null &&
                    ReferenceEquals(value.Blueprint, profile.Claw));

            foreach (int frame in InfernalSwitch34(babau, profile.Claw))
                yield return frame;
            JObject natural = null;
            foreach (int frame in InfernalFullAttack36(fixture, babau,
                target, trace, "babau-natural", 3, check,
                value => natural = value)) yield return frame;
            var naturalPrimary = Rulebook.Trigger(
                new RuleCalculateWeaponStats(babau,
                    babau.Body.PrimaryHand.MaybeWeapon, null));
            var naturalSecond = Rulebook.Trigger(
                new RuleCalculateWeaponStats(babau, secondClaw, null));
            var naturalBite = Rulebook.Trigger(
                new RuleCalculateWeaponStats(babau, bite, null));
            int strengthBonus = babau.Descriptor.Stats.Strength.Bonus;
            bool naturalStrength =
                !babau.Body.PrimaryHand.MaybeWeapon.IsSecondary &&
                !secondClaw.IsSecondary && !bite.IsSecondary &&
                InfernalDamageBonus36(naturalPrimary) == strengthBonus &&
                InfernalDamageBonus36(naturalSecond) == strengthBonus &&
                InfernalDamageBonus36(naturalBite) == strengthBonus;
            check("babau-natural-strength", naturalStrength,
                new JObject {
                    ["owner"] = babau.UniqueId,
                    ["primary"] = InfernalWeaponStats36(naturalPrimary,
                        babau.Body.PrimaryHand.MaybeWeapon),
                    ["secondClaw"] = InfernalWeaponStats36(naturalSecond,
                        secondClaw),
                    ["bite"] = InfernalWeaponStats36(naturalBite, bite),
                    ["fullAttack"] = natural
                }, "The natural routine gives two primary claws and a primary bite their live full Strength contribution.");
            foreach (int frame in InfernalSingleAndOpportunity34(fixture,
                babau, trace, check, "babau-natural", 3))
                yield return frame;

            foreach (int frame in InfernalBabauSneak36(babau, target,
                trace, check)) yield return frame;

            foreach (int frame in InfernalSwitch34(babau,
                profile.Longspear)) yield return frame;
            JObject armed = null;
            foreach (int frame in InfernalFullAttack36(fixture, babau,
                target, trace, "babau-armed", 8, check,
                value => armed = value)) yield return frame;
            var spearStats = Rulebook.Trigger(new RuleCalculateWeaponStats(
                babau, babau.Body.PrimaryHand.MaybeWeapon, null));
            var armedBite = Rulebook.Trigger(new RuleCalculateWeaponStats(
                babau, bite, null));
            bool armedStrength =
                !babau.Body.PrimaryHand.MaybeWeapon.IsSecondary &&
                bite.IsSecondary &&
                InfernalDamageBonus36(spearStats) ==
                    (int)(strengthBonus * 1.5f) &&
                InfernalDamageBonus36(armedBite) ==
                    (int)(strengthBonus * .5f);
            check("babau-armed-strength", armedStrength,
                new JObject {
                    ["owner"] = babau.UniqueId,
                    ["longspear"] = InfernalWeaponStats36(spearStats,
                        babau.Body.PrimaryHand.MaybeWeapon),
                    ["bite"] = InfernalWeaponStats36(armedBite, bite),
                    ["fullAttack"] = armed
                }, "The armed routine gives the two-handed longspear one-and-a-half Strength and its retained secondary bite half Strength.");
            foreach (int frame in InfernalSingleAndOpportunity34(fixture,
                babau, trace, check, "babau-armed", 8))
                yield return frame;
        }

        private IEnumerable<int> InfernalFullAttack36(
            ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, UnitEntityData target,
            InfernalEvents34 trace, string label, int feet,
            Action<string, bool, JObject, string> check,
            Action<JObject> result)
        {
            owner.Commands.InterruptAll(true);
            owner.Commands.RemoveFinishedAndUpdateQueue();
            InfernalCombatLane34(fixture, owner, target, feet);
            int first = trace.Rows.Count;
            var command = new UnitAttack(target) { ForceFullAttack = true };
            var costs = new JArray();
            using (var economy = new FirearmReviewCosts(owner, costs))
            {
                owner.Commands.Run(command);
                foreach (int frame in WaitInfernalCommand34(command,
                    () => true, 30)) yield return frame;
                JObject[] attacks = trace.Rows.Skip(first).OfType<JObject>()
                    .Where(row => (string)row["event"] == "attack" &&
                        (string)row["owner"] == owner.UniqueId &&
                        (string)row["target"] == target.UniqueId).ToArray();
                InfernalMartialProfile36 profile =
                    InfernalMartialProfile36.For(owner);
                bool babau = profile?.Longspear != null;
                bool natural = babau && ReferenceEquals(
                    owner.Body.PrimaryHand.MaybeWeapon.Blueprint,
                    profile.Claw);
                bool exactWeapons = !babau ?
                    attacks.Length == 3 &&
                        attacks.Count(row => (string)row["weapon"] ==
                            profile.Bite.AssetGuid) == 1 &&
                        attacks.Count(row => (string)row["weapon"] ==
                            profile.Claw.AssetGuid) == 2 :
                    natural ? attacks.Length == 3 &&
                        attacks.Count(row => (string)row["weapon"] ==
                            profile.Claw.AssetGuid) == 2 &&
                        attacks.Count(row => (string)row["weapon"] ==
                            profile.Bite.AssetGuid) == 1 &&
                        !attacks.Any(row => (string)row["weapon"] ==
                            profile.Longspear.AssetGuid) :
                        attacks.Length == 3 &&
                        attacks.Count(row => (string)row["weapon"] ==
                            profile.Longspear.AssetGuid) == 2 &&
                        attacks.Count(row => (string)row["weapon"] ==
                            profile.Bite.AssetGuid) == 1 &&
                        !attacks.Any(row => (string)row["weapon"] ==
                            profile.Claw.AssetGuid);
                var resultRow = new JObject {
                    ["owner"] = owner.UniqueId,
                    ["target"] = target.UniqueId,
                    ["command"] = Sprint27AiCommand(command),
                    ["plan"] = new JArray(command.AllAttacks.Select(value =>
                        new JObject {
                            ["weapon"] = value.Hand?.MaybeWeapon?.Blueprint?
                                .AssetGuid,
                            ["penalty"] = value.AttackBonusPenalty,
                            ["number"] = value.AttackNumber
                        })),
                    ["attacks"] = new JArray(attacks),
                    ["costs"] = costs,
                    ["feetBeyondBodies"] = feet,
                    ["forcedRolls"] = 0
                };
                bool exact = command.Result ==
                        UnitCommand.ResultType.Success && exactWeapons &&
                    economy.Count(command) == 1 &&
                    (economy.InCombat(command) ?
                        economy.Exact(command, false) :
                        economy.Unchanged(command));
                result(resultRow);
                check(label + "-full-attack", exact, resultRow,
                    babau ? "The real full-attack command keeps Babau's natural and armed routines mutually exclusive, including the mode-dependent bite role." :
                    "The real full-attack command delivers exactly the Hezrou bite and two claws through their native weapon instances.");
            }
        }

        private static JObject InfernalWeaponStats36(
            RuleCalculateWeaponStats stats, ItemEntityWeapon weapon)
        {
            return new JObject {
                ["weapon"] = weapon?.Blueprint?.AssetGuid,
                ["secondary"] = weapon?.IsSecondary,
                ["strengthMultiplier"] = stats?.DamageBonusStatMultiplier,
                ["damage"] = stats == null ? null : new JArray(
                    stats.DamageDescription.Select(value => new JObject {
                        ["dice"] = value.Dice.ToString(),
                        ["bonus"] = value.Bonus
                    }))
            };
        }

        private static int? InfernalDamageBonus36(
            RuleCalculateWeaponStats stats)
        {
            return stats?.DamageDescription != null &&
                stats.DamageDescription.Count == 1 ?
                (int?)stats.DamageDescription[0].Bonus : null;
        }

        private IEnumerable<int> InfernalBabauSneak36(
            UnitEntityData babau, UnitEntityData target,
            InfernalEvents34 trace,
            Action<string, bool, JObject, string> check)
        {
            foreach (int frame in InfernalSwitch34(babau,
                InfernalMartialProfile36.For(babau).Claw))
                yield return frame;
            ItemEntityWeapon weapon = babau.Body.PrimaryHand.MaybeWeapon;
            target.Stats.AC.BaseValue = -100;
            int damage = target.Damage;
            RuleAttackWithWeapon ordinary = null;
            var ordinaryAttempts = new JArray();
            for (int attempt = 0; attempt < 3 &&
                (ordinary == null || ordinary.AttackRoll?.IsHit != true);
                attempt++)
            {
                ordinary = Rulebook.Trigger(new RuleAttackWithWeapon(babau,
                    target, weapon, 0));
                ordinaryAttempts.Add(new JObject {
                    ["d20"] = ordinary.AttackRoll?.Roll.Value,
                    ["hit"] = ordinary.AttackRoll?.IsHit,
                    ["sneak"] = ordinary.AttackRoll?.IsSneakAttackUsed
                });
                target.Damage = damage;
            }
            UnitEntityData flanker = null;
            RuleAttackWithWeapon qualified = null;
            var qualifiedAttempts = new JArray();
            try
            {
                flanker = InfernalFlankingAlly36(babau, target);
                for (int attempt = 0; attempt < 3 &&
                    (qualified == null || qualified.AttackRoll?.IsHit != true);
                    attempt++)
                {
                    qualified = Rulebook.Trigger(new RuleAttackWithWeapon(
                        babau, target, weapon, 0));
                    qualifiedAttempts.Add(new JObject {
                        ["d20"] = qualified.AttackRoll?.Roll.Value,
                        ["hit"] = qualified.AttackRoll?.IsHit,
                        ["sneak"] = qualified.AttackRoll?
                            .IsSneakAttackUsed,
                        ["sneakDamage"] = InfernalSneakDamage36(qualified)
                    });
                    target.Damage = damage;
                }
                bool exact = babau.Stats.SneakAttack.ModifiedValue == 2 &&
                    ordinary?.AttackRoll?.IsHit == true &&
                    ordinary.AttackRoll.IsSneakAttackUsed == false &&
                    InfernalSneakDamage36(ordinary) == 0 &&
                    qualified?.AttackRoll?.IsHit == true &&
                    qualified.AttackRoll.IsSneakAttackUsed &&
                    InfernalSneakDamage36(qualified) > 0 &&
                    target.CombatState.IsFlanked;
                check("babau-native-sneak", exact, new JObject {
                    ["owner"] = babau.UniqueId,
                    ["target"] = target.UniqueId,
                    ["sneakDice"] = babau.Stats.SneakAttack.ModifiedValue,
                    ["ordinary"] = ordinaryAttempts,
                    ["qualified"] = qualifiedAttempts,
                    ["targetFlanked"] = target.CombatState.IsFlanked,
                    ["directRulePurpose"] =
                        "Native precision qualification and damage packets; ordinary command attacks are proved separately.",
                    ["forcedRolls"] = 0
                }, "Native flat-footed/flanking qualification adds Babau's 2d6 sneak packet only to the qualifying hit, never every hit.");
            }
            finally
            {
                target.Damage = damage;
                flanker?.Dispose();
            }
        }

        private static int InfernalSneakDamage36(
            RuleAttackWithWeapon attack)
        {
            if (attack?.MeleeDamage?.ResultDamage == null) return 0;
            return attack.MeleeDamage.ResultDamage.Where(value =>
                value.Source != null && value.Source.Sneak &&
                !value.Source.Immune && value.FinalValue > 0)
                .Sum(value => value.FinalValue);
        }

        private static UnitEntityData InfernalFlankingAlly36(
            UnitEntityData attacker, UnitEntityData target)
        {
            UnitEntityData ally = new ChargenUnit(
                BlueprintRoot.Instance.DefaultPlayerCharacter).Unit;
            if (ally == null) throw new InvalidOperationException(
                "The request-local Babau flanking ally could not be created.");
            ally.CombatState.JoinCombat();
            attacker.CombatState.Engage(target);
            ally.CombatState.Engage(target);
            attacker.Commands.Run(new UnitAttack(target));
            ally.Commands.Run(new UnitAttack(target));
            if (!target.CombatState.IsFlanked)
            {
                ally.Dispose();
                throw new InvalidOperationException(
                    "Two native allied engagements did not flank the Babau target.");
            }
            return ally;
        }

        private IEnumerable<int> InfernalHezrouMelee36(
            ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData hezrou, UnitEntityData target,
            InfernalEvents34 trace,
            Action<string, bool, JObject, string> check)
        {
            target.Descriptor.State.Size = Size.Large;
            target.Stats.AdditionalCMD.BaseValue = -100;
            JObject full = null;
            foreach (int frame in InfernalFullAttack36(fixture, hezrou,
                target, trace, "hezrou-ordinary", 8, check,
                value => full = value)) yield return frame;
            SummonGrabComponent grab = SummonGrabComponent.Find(hezrou);
            UnitEntityData holder = grab == null ? null :
                SummonHeldComponent.HolderOf(target, grab.GrappledBuff);
            ItemEntityWeapon established = holder == null ? null :
                SummonGrappleLinks.EstablishingWeapon(hezrou, target);
            bool exact = ReferenceEquals(holder, hezrou) &&
                established != null && grab.IsGrabLimb(hezrou,
                    established) && grab.HeldCount(hezrou) == 1 &&
                !grab.HasImpale && grab.ConstrictDiceCount == 0;
            check("hezrou-attack-grab", exact, new JObject {
                ["owner"] = hezrou.UniqueId,
                ["target"] = target.UniqueId,
                ["holder"] = holder?.UniqueId,
                ["establishingWeapon"] = established?.Blueprint?.AssetGuid,
                ["heldCount"] = grab?.HeldCount(hezrou),
                ["impale"] = grab?.HasImpale,
                ["constrictDice"] = grab?.ConstrictDiceCount,
                ["fullAttack"] = full,
                ["fixtureGrabDispatch"] = 0
            }, "An actual Hezrou bite/claw command establishes one legal native hold through the struck limb without Impale, constrict or swallow riders.");
            SummonGrappleLinks.ReleaseAll(hezrou);
        }

        private IEnumerable<int> InfernalHezrouSpells36(
            ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData hezrou, UnitEntityData target,
            UnitEntityData ally, InfernalEvents34 trace,
            Action<string, bool, JObject, string> check)
        {
            string prefix = ExpandedSummoningSpecialBuilder.Hezrou36Prefix;
            var chaos = (BlueprintAbility)ExpandedSummoningBlueprints
                .Registered.BySymbol[prefix + "ChaosHammer"];
            var unholy = (BlueprintAbility)ExpandedSummoningBlueprints
                .Registered.BySymbol[prefix + "UnholyBlight"];
            var blasphemy = (BlueprintAbility)ExpandedSummoningBlueprints
                .Registered.BySymbol[prefix + "Blasphemy"];
            var resource = (BlueprintAbilityResource)
                ExpandedSummoningBlueprints.Registered.BySymbol[
                    prefix + "BlasphemyResource"];
            var chaosRun = chaos.GetComponent<AbilityEffectRunAction>();
            var chaosAction = chaosRun?.Actions?.Actions?.OfType<
                ContextActionInfernalChaosHammer36>().SingleOrDefault();
            var unholyRun = unholy.GetComponent<AbilityEffectRunAction>();
            var unholyAction = unholyRun?.Actions?.Actions?.OfType<
                ContextActionInfernalUnholyBlight35>().SingleOrDefault();
            AbilityData chaosData = new AbilityData(
                hezrou.Descriptor.Abilities.GetAbility(chaos));
            AbilityData unholyData = new AbilityData(
                hezrou.Descriptor.Abilities.GetAbility(unholy));
            AbilityData blasphemyData = new AbilityData(
                hezrou.Descriptor.Abilities.GetAbility(blasphemy));
            var chaosParams = chaosData.CalculateParams();
            var unholyParams = unholyData.CalculateParams();
            var blasphemyParams = blasphemyData.CalculateParams();
            int resourceBefore = hezrou.Descriptor.Resources
                .GetResourceAmount(resource);
            bool mapping = chaosAction != null && unholyAction != null &&
                chaos.Type == AbilityType.SpellLike &&
                unholy.Type == AbilityType.SpellLike &&
                chaos.Range == AbilityRange.Medium &&
                unholy.Range == AbilityRange.Medium &&
                chaos.SpellResistance && unholy.SpellResistance &&
                chaos.CanTargetPoint && unholy.CanTargetPoint &&
                chaos.CanTargetFriends && unholy.CanTargetFriends &&
                chaos.GetComponent<AbilityResourceLogic>() == null &&
                unholy.GetComponent<AbilityResourceLogic>() == null &&
                chaosParams.CasterLevel == 13 &&
                unholyParams.CasterLevel == 13 &&
                chaosParams.SpellLevel == 4 &&
                unholyParams.SpellLevel == 4 &&
                blasphemyParams.CasterLevel == 13 &&
                blasphemyParams.SpellLevel == 7 && resourceBefore == 1;
            check("hezrou-spell-mapping", mapping, new JObject {
                ["owner"] = hezrou.UniqueId,
                ["chaos"] = InfernalAbility36(chaos, chaosParams),
                ["unholy"] = InfernalAbility36(unholy, unholyParams),
                ["blasphemy"] = InfernalAbility36(blasphemy,
                    blasphemyParams),
                ["blasphemyResource"] = resource.AssetGuid,
                ["resourceBefore"] = resourceBefore
            }, "Hezrou owns two real CL13 at-will all-creature bursts plus one separately resourced CL13 Blasphemy, with their native action and SR gates intact.");

            Vector3 far;
            string lane, direction;
            Vector3 origin = Sprint24AiCombatLane(fixture.Caster, hezrou,
                target, 4f, out far, out lane, out direction,
                new Feet(22).Meters, new[] { ally }, 30);
            PlaceExpandedSummoningUnit(hezrou, origin);
            PlaceExpandedSummoningUnit(target, far);
            PlaceExpandedSummoningUnit(ally,
                far + Vector3.right * (target.Corpulence +
                    ally.Corpulence + .3f));
            foreach (UnitEntityData actor in new[] { hezrou, target, ally })
            {
                actor.Wake(0f);
                if (!Game.Instance.State.AwakeUnits.Contains(actor))
                    Game.Instance.State.AwakeUnits.Add(actor);
                actor.SightCache.ClearIfNeeded();
            }
            target.Descriptor.Alignment.Set(Alignment.LawfulGood);
            ally.Descriptor.Alignment.Set(Alignment.TrueNeutral);
            target.Stats.SaveWill.BaseValue = -100;
            ally.Stats.SaveWill.BaseValue = -100;
            target.Stats.HitPoints.BaseValue = 100000;
            ally.Stats.HitPoints.BaseValue = 100000;
            target.Damage = 0;
            ally.Damage = 0;
            foreach (int frame in InfernalHezrouBurst36(hezrou, chaos,
                chaosAction.Slowed, chaosAction.OutsiderType, target, ally,
                check, "chaos")) yield return frame;
            foreach (Buff buff in new[] { target, ally }.SelectMany(unit =>
                unit.Buffs.Enumerable).Where(value => ReferenceEquals(
                    value.Context?.MaybeCaster, hezrou)).ToArray())
                buff.Remove();
            target.Damage = 0;
            ally.Damage = 0;
            foreach (int frame in InfernalHezrouBurst36(hezrou, unholy,
                unholyAction.Sickened, unholyAction.OutsiderType, target,
                ally, check, "unholy")) yield return frame;

            foreach (Buff buff in new[] { target, ally }.SelectMany(unit =>
                unit.Buffs.Enumerable).Where(value => ReferenceEquals(
                    value.Context?.MaybeCaster, hezrou)).ToArray())
                buff.Remove();
            target.Damage = 0;
            ally.Damage = 0;
            int first = trace.Rows.Count;
            UnitUseAbility command = null;
            var costs = new JArray();
            using (var economy = new FirearmReviewCosts(hezrou, costs))
            {
                foreach (int frame in Sprint26Cast(hezrou, blasphemy,
                    new TargetWrapper(hezrou), value => command = value))
                    yield return frame;
                int resourceAfter = hezrou.Descriptor.Resources
                    .GetResourceAmount(resource);
                JObject[] effects = trace.Rows.Skip(first).OfType<JObject>()
                    .Where(row => (string)row["event"] ==
                            "effect-application" &&
                        (string)row["owner"] == hezrou.UniqueId &&
                        (string)row["target"] == target.UniqueId).ToArray();
                bool exact = command != null && command.IsFinished &&
                    command.Result == UnitCommand.ResultType.Success &&
                    resourceAfter == 0 && !blasphemyData.IsAvailable &&
                    effects.Length > 0 && economy.Count(command) == 1 &&
                    economy.Exact(command, false);
                check("hezrou-blasphemy-resource", exact, new JObject {
                    ["owner"] = hezrou.UniqueId,
                    ["target"] = target.UniqueId,
                    ["command"] = command == null ? null :
                        Sprint27AiCommand(command),
                    ["resourceBefore"] = resourceBefore,
                    ["resourceAfter"] = resourceAfter,
                    ["availableAfter"] = blasphemyData.IsAvailable,
                    ["targetEffects"] = new JArray(effects),
                    ["costs"] = costs
                }, "One real native Blasphemy command spends exactly its single per-summoning resource, affects the legal non-evil target, and cannot be repeated.");
            }
        }

        private static JObject InfernalAbility36(BlueprintAbility ability,
            AbilityParams parameters)
        {
            var around = ability.GetComponent<AbilityTargetsAround>();
            return new JObject {
                ["guid"] = ability.AssetGuid,
                ["type"] = ability.Type.ToString(),
                ["range"] = ability.Range.ToString(),
                ["action"] = ability.ActionType.ToString(),
                ["spellResistance"] = ability.SpellResistance,
                ["targetPoint"] = ability.CanTargetPoint,
                ["targetFriends"] = ability.CanTargetFriends,
                ["targetEnemies"] = ability.CanTargetEnemies,
                ["radiusFeet"] = around?.AoERadius.Value,
                ["casterLevel"] = parameters?.CasterLevel,
                ["spellLevel"] = parameters?.SpellLevel,
                ["dc"] = parameters?.DC
            };
        }

        private IEnumerable<int> InfernalHezrouBurst36(
            UnitEntityData hezrou, BlueprintAbility ability,
            BlueprintBuff condition, BlueprintUnitFact outsider,
            UnitEntityData target, UnitEntityData ally,
            Action<string, bool, JObject, string> check, string label)
        {
            var observed = new Astral31SmiteEvents(hezrou, ability,
                condition, outsider, new[] { target, ally });
            EventBus.Subscribe(observed);
            try
            {
                UnitUseAbility command = null;
                var costs = new JArray();
                using (var economy = new FirearmReviewCosts(hezrou, costs))
                {
                    foreach (int frame in Sprint26Cast(hezrou, ability,
                        new TargetWrapper(target.Position),
                        value => command = value)) yield return frame;
                    RuleSavingThrow targetSave = observed.Save(target);
                    RuleSavingThrow allySave = observed.Save(ally);
                    RuleDealDamage targetHit = observed.Damage(target);
                    RuleDealDamage allyHit = observed.Damage(ally);
                    Buff targetCondition = target.Buffs.Enumerable
                        .SingleOrDefault(value => ReferenceEquals(
                            value.Blueprint, condition) && ReferenceEquals(
                            value.Context?.MaybeCaster, hezrou));
                    Buff allyCondition = ally.Buffs.Enumerable
                        .SingleOrDefault(value => ReferenceEquals(
                            value.Blueprint, condition) && ReferenceEquals(
                            value.Context?.MaybeCaster, hezrou));
                    AbilityData availability = new AbilityData(
                        hezrou.Descriptor.Abilities.GetAbility(ability));
                    bool exact = command != null && command.IsFinished &&
                        command.Result == UnitCommand.ResultType.Success &&
                        targetSave != null && !targetSave.IsPassed &&
                        allySave != null && !allySave.IsPassed &&
                        targetHit != null && targetHit.Damage > 0 &&
                        allyHit != null && allyHit.Damage > 0 &&
                        targetCondition != null && allyCondition == null &&
                        availability.IsAvailable &&
                        economy.Count(command) == 1 &&
                        economy.Exact(command, false);
                    check("hezrou-" + label + "-collateral", exact,
                        new JObject {
                            ["owner"] = hezrou.UniqueId,
                            ["command"] = command == null ? null :
                                Sprint27AiCommand(command),
                            ["target"] = Infernal35BurstRow(observed,
                                target),
                            ["ally"] = Infernal35BurstRow(observed, ally),
                            ["targetCondition"] = targetCondition == null ?
                                null : InfernalSavedEffect34(targetCondition),
                            ["allyCondition"] = allyCondition == null ?
                                null : InfernalSavedEffect34(allyCondition),
                            ["availableAfter"] = availability.IsAvailable,
                            ["costs"] = costs,
                            ["aiCommands"] = 0
                        }, "The real at-will " + label +
                        " burst resolves separate saves and alignment-sensitive damage against both enemy and ally; AI safety does not rewrite the player ability enemy-only.");
                }
            }
            finally { EventBus.Unsubscribe(observed); }
        }

        private IEnumerable<int> InfernalHezrouFallback36(
            ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData hezrou, UnitEntityData target,
            UnitEntityData ally, InfernalEvents34 trace,
            Action<string, bool, JObject, string> check)
        {
            SummonGrappleLinks.ReleaseAll(hezrou);
            hezrou.Commands.InterruptAll(true);
            hezrou.Commands.RemoveFinishedAndUpdateQueue();
            // The preceding finite-resource proof intentionally lets native
            // Blasphemy resolve against this target.  Do not reuse its
            // long-lived paralysis as an alleged ordinary-attack target.
            // Reset only this disposable owner's delivered effects, exactly
            // as the two preceding burst cells do between independent casts.
            Buff[] resetEffects = new[] { target, ally }.SelectMany(unit =>
                unit.Buffs.Enumerable).Where(value => ReferenceEquals(
                    value.Context?.MaybeCaster, hezrou)).ToArray();
            string[] resetEffectNames = resetEffects.Select(value =>
                value.Blueprint.name).ToArray();
            foreach (Buff buff in resetEffects) buff.Remove();
            target.Stats.AC.BaseValue = -100;
            target.Stats.HitPoints.BaseValue = 100000;
            InfernalCombatLane34(fixture, hezrou, target, 8);
            PlaceExpandedSummoningUnit(ally, target.Position +
                Vector3.right * (target.Corpulence + ally.Corpulence + .3f));
            ally.Descriptor.Alignment.Set(Alignment.NeutralGood);
            hezrou.Memory.Add(target).Visible = true;
            target.Memory.Add(hezrou).Visible = true;
            // Match the already-qualified autonomous-action fixture: update
            // native visibility/combat caches and make this exact owner's
            // decision clock due before ticking its real brain. Direct casts
            // earlier in this grouped observer do not drive those AI-only
            // caches or clock themselves.
            new Kingmaker.Controllers.Units.UnitMemoryController().Tick();
            new UnitCombatJoinController().Tick();
            new UnitCombatPrepareController().Tick();
            var commands = new HashSet<UnitCommand>();
            var ambientCandidates = new HashSet<string>(
                StringComparer.Ordinal);
            int first = trace.Rows.Count;
            var costs = new JArray();
            using (var economy = new FirearmReviewCosts(hezrou, costs))
            {
                Sprint28SetBrainActivePreservingActions(hezrou, true);
                hezrou.Brain.RestoreAvailableActions();
                float nextCommandBefore = hezrou.CombatState.AIData
                    .NextCommandTime;
                hezrou.CombatState.AIData.NextCommandTime = Time.time - .01f;
                string[] brainActions = hezrou.Blueprint.Brain.Actions
                    .Select(value => value.GetType().FullName + ":" +
                        value.name).ToArray();
                var availableActions = new JArray(hezrou.Brain
                    .AvailableActions.Select(value => new JObject {
                        ["type"] = value.Blueprint.GetType().FullName,
                        ["name"] = value.Blueprint.name,
                        ["baseScore"] = value.BaseScore
                    }));
                bool ordinaryFallback = hezrou.Blueprint.Brain.Actions
                    .Any(value => value.GetType().FullName ==
                        "Kingmaker.Controllers.Brain.Blueprints." +
                        "BlueprintAiAttack");
                bool targetInGroupMemory = hezrou.Group.Memory.Enemies.Any(
                    value => ReferenceEquals(value.Unit, target));
                bool nativeReady = hezrou.IsInGame &&
                    hezrou.Descriptor.State.CanAct && hezrou.IsBrainActive &&
                    hezrou.IsInCombat && hezrou.Group != null &&
                    hezrou.Group.IsInCombat &&
                    Game.Instance.AwakeUnitGroups.Contains(hezrou.Group) &&
                    target.IsInState && !target.Descriptor.State.IsDead &&
                    target.Descriptor.State.CanAct && hezrou.HasLOS(target) &&
                    hezrou.IsEnemy(target) &&
                    hezrou.Memory.ContainsVisible(target) &&
                    targetInGroupMemory;
                DateTime deadline = DateTime.UtcNow.AddSeconds(45);
                int frames = 0, aiTicks = 0;
                while (DateTime.UtcNow < deadline)
                {
                    if (!hezrou.Commands.Raw.Any(value => value != null &&
                            !value.IsFinished) && frames % 60 == 0)
                    {
                        foreach (string candidate in
                            Sprint24TickExactAi(hezrou))
                            if (candidate != hezrou.UniqueId)
                                ambientCandidates.Add(candidate);
                        aiTicks++;
                    }
                    foreach (UnitCommand command in hezrou.Commands.Raw
                        .Where(value => value != null)) commands.Add(command);
                    UnitAttack attack = commands.OfType<UnitAttack>()
                        .FirstOrDefault(value => value.IsFinished &&
                            value.Result == UnitCommand.ResultType.Success);
                    if (attack != null) break;
                    frames++;
                    Game.Instance.IsPaused = false;
                    yield return 0;
                }
                UnitAttack fallback = commands.OfType<UnitAttack>()
                    .FirstOrDefault(value => value.IsFinished &&
                        value.Result == UnitCommand.ResultType.Success);
                UnitUseAbility[] spellCommands = commands.OfType<
                    UnitUseAbility>().Where(value => value.AiAction is
                        InfernalSpellAi35).ToArray();
                JObject[] attacks = trace.Rows.Skip(first).OfType<JObject>()
                    .Where(row => (string)row["event"] == "attack" &&
                        (string)row["owner"] == hezrou.UniqueId &&
                        (string)row["target"] == target.UniqueId).ToArray();
                bool exact = ordinaryFallback && nativeReady &&
                    fallback != null && attacks.Length > 0 &&
                    spellCommands.Length == 0 &&
                    economy.Count(fallback) == 1 &&
                    economy.Exact(fallback, false);
                check("hezrou-ai-friendly-fallback", exact,
                    new JObject {
                        ["owner"] = hezrou.UniqueId,
                        ["target"] = target.UniqueId,
                        ["ally"] = ally.UniqueId,
                        ["allyIsEnemy"] = hezrou.IsEnemy(ally),
                        ["allyDistanceToTarget"] = ally.DistanceTo(target),
                        ["commands"] = new JArray(commands.Select(
                            Sprint27AiCommand)),
                        ["spellCommands"] = spellCommands.Length,
                        ["attacks"] = new JArray(attacks),
                        ["costs"] = costs,
                        ["brain"] = hezrou.Blueprint.Brain.name,
                        ["brainActions"] = new JArray(brainActions),
                        ["runtimeActions"] = hezrou.Brain.Actions.Count,
                        ["availableActions"] = availableActions,
                        ["resetEffects"] = new JArray(resetEffectNames),
                        ["targetCanAct"] = target.Descriptor.State.CanAct,
                        ["targetHasLos"] = hezrou.HasLOS(target),
                        ["targetInGroupMemory"] = targetInGroupMemory,
                        ["ordinaryFallback"] = ordinaryFallback,
                        ["nativeReady"] = nativeReady,
                        ["nextCommandTimeBefore"] = nextCommandBefore,
                        ["nextCommandTimeArmed"] = hezrou.CombatState.AIData
                            .NextCommandTime,
                        ["frames"] = frames,
                        ["aiTicks"] = aiTicks,
                        ["ambientCandidatesNeverTicked"] = new JArray(
                            ambientCandidates),
                        ["fixtureCommands"] = 0
                    }, "With a non-exempt ally inside every burst, the real Hezrou AI refuses collateral casting and completes an effective ordinary melee fallback.");
            }
            Sprint28SetBrainActivePreservingActions(hezrou, false);
            hezrou.Commands.InterruptAll(true);
            hezrou.Commands.RemoveFinishedAndUpdateQueue();
        }
    }
}
