using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Brain;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
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
        /// <summary>
        /// One hidden Bone behavior group. Every effect is produced by the
        /// production aura, poison trigger, native spell graph, or a real
        /// attack/ability command. Fixture modifiers change only eligibility
        /// and are restored before the phase returns.
        /// </summary>
        private IEnumerable<int> ReviewInfernalBone35(
            ExpandedSummoningCorrectionFixture fixture)
        {
            var owned = new List<UnitEntityData>();
            var fearOwners = new List<UnitEntityData>();
            var cells = new JArray();
            var trace = new InfernalEvents34();
            var receipt = new JObject {
                ["phase"] = ExpandedSummoningNativeProfilePolicy
                    .Sprint35InfernalBonePhase,
                ["finalQualification"] = false,
                ["cells"] = cells,
                ["events"] = trace.Rows
            };
            UnitEntityData bone = null;
            UnitEntityData target = fixture.Hostile;
            UnitEntityData ally = fixture.Caster;
            BlueprintFaction targetFaction = fixture.OwnedHostileFaction;
            BlueprintFaction[] oldTargetAttackFactions = targetFaction?
                .AttackFactions;
            string prefix = ExpandedSummoningSpecialBuilder.Bone35Prefix;
            var poison = (BlueprintBuff)ExpandedSummoningBlueprints
                .Registered.BySymbol[prefix + "Poison"];
            var frightened = (BlueprintBuff)ExpandedSummoningBlueprints
                .Registered.BySymbol[prefix + "FearFrightened"];
            var memory = (BlueprintBuff)ExpandedSummoningBlueprints
                .Registered.BySymbol[prefix + "FearImmunity"];
            var fearArea = (BlueprintAbilityAreaEffect)
                ExpandedSummoningBlueprints.Registered.BySymbol[
                    prefix + "FearArea"];
            var invisibility = (BlueprintAbility)ExpandedSummoningBlueprints
                .Registered.BySymbol[prefix + "Invisibility"];
            var quick = (BlueprintAbility)ExpandedSummoningBlueprints
                .Registered.BySymbol[prefix + "QuickenedInvisibility"];
            var quickResource = (BlueprintAbilityResource)
                ExpandedSummoningBlueprints.Registered.BySymbol[
                    prefix + "QuickenedInvisibilityResource"];
            var invisAi = (InfernalSpellAi35)ExpandedSummoningBlueprints
                .Registered.BySymbol[prefix + "InvisibilityAi"];
            var quickAi = (InfernalSpellAi35)ExpandedSummoningBlueprints
                .Registered.BySymbol[prefix + "QuickenedInvisibilityAi"];
            var invisible = (BlueprintBuff)ExpandedSummoningBlueprints
                .Registered.BySymbol[prefix + "InvisibilityBuff"];
            var invisibilityBehavior = invisible.ComponentsArray.OfType<
                BuffInvisibility>().Single();

            int oldTargetAc = target.Stats.AC.BaseValue;
            int oldTargetFort = target.Stats.SaveFortitude.BaseValue;
            int oldTargetWill = target.Stats.SaveWill.BaseValue;
            int oldTargetHp = target.Stats.HitPoints.BaseValue;
            int oldTargetStrength = target.Stats.Strength.BaseValue;
            int oldTargetStrengthDamage = target.Stats.Strength.Damage;
            int oldTargetDamage = target.Damage;
            int oldAllyWill = ally.Stats.SaveWill.BaseValue;
            Vector3 oldTargetPosition = target.Position;
            Vector3 oldAllyPosition = ally.Position;
            float oldTargetOrientation = target.Orientation;
            float oldAllyOrientation = ally.Orientation;
            Action<string, bool, JObject, string> check =
                (id, pass, row, description) => {
                    row["cell"] = id;
                    cells.Add(row);
                    Sprint25Check("sprint35-" + id, pass, row, description);
                    if (!pass) throw new InvalidOperationException(
                        "Sprint35 Bone cell failed: " + id);
                };

            try
            {
                // The owner is created beside its summoner. A deliberately
                // low allied Will save makes the native area callback itself
                // prove that the player-visible aura is not enemy-only.
                ally.Stats.SaveWill.BaseValue = -100;
                int allyFearStart = trace.Rows.Count;
                bone = CastExpandedSummoningOwnTier(fixture, "bone-devil");
                owned.Add(bone);
                fearOwners.Add(bone);
                Sprint28SetBrainActivePreservingActions(bone, false);
                bone.Commands.InterruptAll(true);
                bone.Commands.RemoveFinishedAndUpdateQueue();
                foreach (int frame in Outsider30NativeAppearance(bone,
                    bone.Blueprint.name)) yield return frame;

                // The shared turn-hostile fixture deliberately attacks only
                // the native Player faction. Summons use the distinct native
                // Summoned faction, so a manually dispatched attack can land
                // without satisfying BuffInvisibility's real
                // owner.IsEnemy(target) offensive-action predicate. Extend
                // only this request-owned faction, then restore its exact
                // original array before cleanup.
                if (targetFaction == null || !ReferenceEquals(target.Faction,
                        targetFaction))
                    throw new InvalidOperationException(
                        "Bone hostility fixture does not own its faction.");
                targetFaction.AttackFactions = (oldTargetAttackFactions ??
                    Array.Empty<BlueprintFaction>()).Concat(
                        new[] { bone.Faction }).Distinct().ToArray();
                bool boneHostility = bone.IsEnemy(target) &&
                    target.IsEnemy(bone);
                check("bone-private-hostility", boneHostility,
                    new JObject {
                        ["owner"] = bone.UniqueId,
                        ["ownerFaction"] = bone.Faction.name,
                        ["target"] = target.UniqueId,
                        ["targetFaction"] = targetFaction.name,
                        ["ownerIsEnemy"] = bone.IsEnemy(target),
                        ["targetIsEnemy"] = target.IsEnemy(bone),
                        ["originalAttackFactions"] = new JArray(
                            (oldTargetAttackFactions ??
                                Array.Empty<BlueprintFaction>()).Select(
                                    value => value.name)),
                        ["activeAttackFactions"] = new JArray(
                            targetFaction.AttackFactions.Select(
                                value => value.name))
                    }, "The request-owned target is a real native enemy of the Bone Devil before attack-breaking invisibility is observed; no intake or production faction is changed.");

                var profile = InfernalMartialProfile35.For(bone);
                var poisonLogic = poison.ComponentsArray.OfType<
                    BuffPoisonStatDamage>().SingleOrDefault();
                var mapping = new JObject {
                    ["owner"] = bone.UniqueId,
                    ["size"] = bone.Descriptor.State.Size.ToString(),
                    ["reachFeet"] = profile?.Bite?.AttackRange.Value,
                    ["primary"] = bone.Body.PrimaryHand.MaybeWeapon?
                        .Blueprint?.AssetGuid,
                    ["additional"] = new JArray(bone.Body.AdditionalLimbs
                        .Select(value => value.MaybeWeapon?.Blueprint?
                            .AssetGuid)),
                    ["poison"] = poison.AssetGuid,
                    ["poisonStat"] = poisonLogic?.Stat.ToString(),
                    ["poisonDice"] = poisonLogic?.Value.ToString(),
                    ["poisonTicks"] = poisonLogic?.Ticks,
                    ["poisonCureSaves"] = poisonLogic?.SuccesfullSaves,
                    ["fearArea"] = fearArea.AssetGuid,
                    ["fearFeet"] = fearArea.Size.Value,
                    ["invisibility"] = invisibility.AssetGuid,
                    ["quickened"] = quick.AssetGuid,
                    ["invisibilityBuff"] = invisible.AssetGuid,
                    ["breaksOnOffensiveAction"] = !invisibilityBehavior
                        .NotDispellAfterOffensiveAction,
                    ["quickenedUses"] = bone.Descriptor.Resources
                        .GetResourceAmount(quickResource)
                };
                bool mapped = profile != null && profile.Bite != null &&
                    profile.Claw != null && profile.Sting != null &&
                    bone.Descriptor.State.Size == Size.Large &&
                    ReferenceEquals(bone.Body.PrimaryHand.MaybeWeapon?
                        .Blueprint, profile.Bite) &&
                    bone.Body.AdditionalLimbs.Count == 3 &&
                    bone.Body.AdditionalLimbs.Count(value =>
                        ReferenceEquals(value.MaybeWeapon?.Blueprint,
                            profile.Claw)) == 2 &&
                    bone.Body.AdditionalLimbs.Count(value =>
                        ReferenceEquals(value.MaybeWeapon?.Blueprint,
                            profile.Sting)) == 1 &&
                    poison.Stacking == StackingType.Poison &&
                    poisonLogic != null &&
                    poisonLogic.Stat == StatType.Strength &&
                    poisonLogic.Value.Rolls == 1 &&
                    poisonLogic.Value.Dice == DiceType.D3 &&
                    poisonLogic.Ticks == 6 &&
                    poisonLogic.SuccesfullSaves == 2 &&
                    poisonLogic.SaveType == SavingThrowType.Fortitude &&
                    fearArea.Size.Value == 5 &&
                    invisibility.ActionType ==
                        UnitCommand.CommandType.Standard &&
                    quick.ActionType == UnitCommand.CommandType.Swift &&
                    !invisibilityBehavior.NotDispellAfterOffensiveAction &&
                    bone.Descriptor.Resources.GetResourceAmount(
                        quickResource) == 3 &&
                    ReferenceEquals(invisAi.ActiveBuff, invisible) &&
                    ReferenceEquals(quickAi.ActiveBuff, invisible);
                check("bone-mapping", mapped, mapping,
                    "The hidden Large Bone root owns bite, two claws and sting as one four-attack routine, the finite Strength poison, a five-foot source aura, and distinct standard/three-use quickened ordinary invisibility graphs.");

                // A Large summon is placed at a safe native separation from
                // its summoner; that separation is not evidence that the
                // summoner is inside a five-foot aura. Put this disposable
                // ally at one measured in-radius point before asking the
                // native area membership consumer to expose it.
                Vector3 allyFearAxis = ally.Position - bone.Position;
                allyFearAxis.y = 0f;
                if (allyFearAxis.sqrMagnitude < .01f)
                    allyFearAxis = Vector3.right;
                allyFearAxis.Normalize();
                PlaceExpandedSummoningUnit(ally,
                    bone.Position + allyFearAxis * 1f);
                Vector3 allyFearOffset = ally.Position - bone.Position;
                allyFearOffset.y = 0f;
                float allyFearDistance = allyFearOffset.magnitude;
                Vector3 allyFearPosition = ally.Position;
                Vector3 sourceFearPosition = bone.Position;
                DateTime allyFearDeadline = DateTime.UtcNow.AddSeconds(12);
                while (DateTime.UtcNow < allyFearDeadline &&
                    !InfernalBoneSaveRows35(trace, allyFearStart, bone,
                        ally, 19).Any())
                {
                    Game.Instance.IsPaused = false;
                    yield return 0;
                }
                JObject[] allyFear = InfernalBoneSaveRows35(trace,
                    allyFearStart, bone, ally, 19).ToArray();

                Vector3 far;
                string lane, direction;
                Vector3 center = Sprint24AiCombatLane(fixture.Caster, bone,
                    target, 6f, out far, out lane, out direction);
                Vector3 axis = (far - center).normalized;
                Vector3 side = Vector3.Cross(axis, Vector3.up).normalized;
                if (side.sqrMagnitude < .5f) side = Vector3.right;
                PlaceExpandedSummoningUnit(ally, far + axis * 10f);
                target.Stats.SaveWill.BaseValue = -100;
                PlaceExpandedSummoningUnit(target, center);
                PlaceExpandedSummoningUnit(bone, center + side * 1.7f);
                int hostileFearStart = trace.Rows.Count;
                DateTime ownerFearDeadline = DateTime.UtcNow.AddSeconds(12);
                while (DateTime.UtcNow < ownerFearDeadline &&
                    !InfernalBoneSaveRows35(trace, hostileFearStart, bone,
                        target, 19).Any())
                {
                    Game.Instance.IsPaused = false;
                    yield return 0;
                }

                UnitEntityData sibling = CastExpandedSummoningOwnTier(
                    fixture, "bone-devil");
                owned.Add(sibling);
                fearOwners.Add(sibling);
                Sprint28SetBrainActivePreservingActions(sibling, false);
                sibling.Commands.InterruptAll(true);
                sibling.Commands.RemoveFinishedAndUpdateQueue();
                // The summoner is deliberately parked beyond the shared
                // combat lane above, so an additional source is born outside
                // the currently presented lane even though its ordinary
                // spawn state is healthy. Move only this disposable unit
                // beside the already-visible first Bone before observing the
                // native fade. Never write renderer or fader state.
                PlaceExpandedSummoningUnit(sibling, bone.Position);
                foreach (int frame in Outsider30NativeAppearance(sibling,
                    sibling.Blueprint.name)) yield return frame;
                PlaceExpandedSummoningUnit(sibling,
                    center - side * 1.7f);
                DateTime siblingFearDeadline = DateTime.UtcNow.AddSeconds(12);
                while (DateTime.UtcNow < siblingFearDeadline &&
                    !InfernalBoneSaveRows35(trace, hostileFearStart,
                        sibling, target, 19).Any())
                {
                    Game.Instance.IsPaused = false;
                    yield return 0;
                }

                // A third natural source is admitted only if both first
                // hostile saves naturally passed. It preserves every roll and
                // avoids relabeling a chance-sensitive precondition as a
                // mechanics failure.
                bool hostileFailed = fearOwners.Any(source =>
                    InfernalBoneSaveRows35(trace, hostileFearStart, source,
                        target, 19).Any(row =>
                            (bool?)row["passed"] == false));
                if (!hostileFailed)
                {
                    UnitEntityData third = CastExpandedSummoningOwnTier(
                        fixture, "bone-devil");
                    owned.Add(third);
                    fearOwners.Add(third);
                    Sprint28SetBrainActivePreservingActions(third, false);
                    third.Commands.InterruptAll(true);
                    third.Commands.RemoveFinishedAndUpdateQueue();
                    PlaceExpandedSummoningUnit(third, bone.Position);
                    foreach (int frame in Outsider30NativeAppearance(third,
                        third.Blueprint.name)) yield return frame;
                    Vector3 thirdPosition = center + axis * 1.7f;
                    PlaceExpandedSummoningUnit(third, thirdPosition);
                    DateTime thirdDeadline = DateTime.UtcNow.AddSeconds(12);
                    while (DateTime.UtcNow < thirdDeadline &&
                        !InfernalBoneSaveRows35(trace, hostileFearStart,
                            third, target, 19).Any())
                    {
                        Game.Instance.IsPaused = false;
                        yield return 0;
                    }
                    hostileFailed = InfernalBoneSaveRows35(trace,
                        hostileFearStart, third, target, 19).Any(row =>
                            (bool?)row["passed"] == false);
                }

                int hostileSavesBefore = fearOwners.Sum(source =>
                    InfernalBoneSaveRows35(trace, hostileFearStart, source,
                        target, 19).Count());
                double noRepeatUntil = Game.Instance.TimeController.GameTime
                    .TotalSeconds + 1d;
                DateTime noRepeatDeadline = DateTime.UtcNow.AddSeconds(8);
                while (DateTime.UtcNow < noRepeatDeadline &&
                    Game.Instance.TimeController.GameTime.TotalSeconds <
                        noRepeatUntil)
                {
                    Game.Instance.IsPaused = false;
                    yield return 0;
                }
                int hostileSavesAfter = fearOwners.Sum(source =>
                    InfernalBoneSaveRows35(trace, hostileFearStart, source,
                        target, 19).Count());
                var hostileFearRows = new JArray(fearOwners.Select(source =>
                    new JObject {
                        ["source"] = source.UniqueId,
                        ["saves"] = new JArray(InfernalBoneSaveRows35(trace,
                            hostileFearStart, source, target, 19)),
                        ["memory"] = InfernalBoneBuff35(target, memory,
                            source) == null ? null : InfernalSavedEffect34(
                                InfernalBoneBuff35(target, memory, source)),
                        ["frightened"] = InfernalBoneBuff35(target,
                            frightened, source) == null ? null :
                            InfernalSavedEffect34(InfernalBoneBuff35(target,
                                frightened, source))
                    }));
                bool aura = allyFear.Length == 1 &&
                    InfernalBoneBuff35(ally, memory, bone) != null &&
                    fearOwners.All(source =>
                        InfernalBoneSaveRows35(trace, hostileFearStart,
                            source, target, 19).Count() == 1 &&
                        InfernalBoneBuff35(target, memory, source) != null) &&
                    hostileFailed &&
                    fearOwners.Any(source =>
                        InfernalBoneBuff35(target, frightened, source) !=
                            null) &&
                    hostileSavesBefore == hostileSavesAfter &&
                    fearOwners.All(source => Sprint29OwnedAreas(source,
                        fearArea).Length == 1);
                check("bone-fear-aura", aura, new JObject {
                    ["ally"] = ally.UniqueId,
                    ["allyFaction"] = ally.Faction?.name,
                    ["sourceFaction"] = bone.Faction?.name,
                    ["allyPosition"] = new JArray(allyFearPosition.x,
                        allyFearPosition.y, allyFearPosition.z),
                    ["sourcePosition"] = new JArray(sourceFearPosition.x,
                        sourceFearPosition.y, sourceFearPosition.z),
                    ["allySourceDistanceMeters"] = allyFearDistance,
                    ["allySaves"] = new JArray(allyFear),
                    ["allyMemory"] = InfernalBoneBuff35(ally, memory,
                        bone) == null ? null : InfernalSavedEffect34(
                            InfernalBoneBuff35(ally, memory, bone)),
                    ["hostile"] = target.UniqueId,
                    ["sources"] = hostileFearRows,
                    ["savesBeforeSettle"] = hostileSavesBefore,
                    ["savesAfterSettle"] = hostileSavesAfter,
                    ["settleGameSeconds"] = 1d,
                    ["boundedSources"] = fearOwners.Count,
                    ["forcedRolls"] = 0
                }, "The real five-foot aura exposes an allied summoner and a hostile, uses one natural DC19 save and source-specific memory per Bone Devil, applies a naturally failed frightened result, and does not retry every frame.");

                InfernalBoneRemoveEffects35(ally, fearOwners,
                    frightened, memory);
                InfernalBoneRemoveEffects35(target, fearOwners,
                    frightened, memory);
                UnitEntityData[] retiredFearSources = fearOwners.Skip(1)
                    .ToArray();
                DisposeExpandedSummoningUnits(fixture.Created,
                    retiredFearSources);
                foreach (int frame in InfernalBoneWaitUntil35(() =>
                    retiredFearSources.All(value => value.Destroyed) &&
                    retiredFearSources.All(value => Sprint29OwnedAreas(value,
                        fearArea).Length == 0), 5)) yield return frame;

                // The aura row parks the summoner far from the hostile so it
                // cannot become an unintended second recipient. Restore that
                // disposable party anchor before the shared native combat
                // lane survey; the survey deliberately admits only a real
                // walkable party anchor.
                PlaceExpandedSummoningUnit(ally, oldAllyPosition);
                ally.Orientation = oldAllyOrientation;
                bone.Stats.AdditionalAttackBonus.BaseValue = 100;
                bone.Stats.HitPoints.BaseValue = 100000;
                target.Stats.AC.BaseValue = -100;
                target.Stats.SaveFortitude.BaseValue = -100;
                target.Stats.HitPoints.BaseValue = 100000;
                target.Stats.Strength.BaseValue = 60;
                target.Stats.Strength.Damage = 0;
                target.Damage = 0;
                float statDamageScale = Game.Instance.Player.Difficulty
                    .DamageToParty;
                InfernalCombatLane34(fixture, bone, target, 3);

                // A real wounding bite is the other-limb negative control.
                int biteStart = trace.Rows.Count;
                var biteCosts = new JArray();
                var biteCommand = new UnitAttack(target) {
                    IsSingleAttack = true,
                    ForceFullAttack = false
                };
                using (var economy = new FirearmReviewCosts(bone, biteCosts))
                {
                    bone.Commands.Run(biteCommand);
                    foreach (int frame in WaitInfernalCommand34(biteCommand,
                        () => true, 25)) yield return frame;
                    JObject[] attacks = InfernalBoneAttackRows35(trace,
                        biteStart, bone, target, biteCommand).ToArray();
                    bool biteOnly = attacks.Length == 1 &&
                        (bool?)attacks[0]["hit"] == true &&
                        (int?)attacks[0]["hpDamage"] > 0 &&
                        (string)attacks[0]["weapon"] ==
                            profile.Bite.AssetGuid &&
                        !InfernalBoneSaveRows35(trace, biteStart, bone,
                            target, 20).Any() &&
                        InfernalBoneBuff35(target, poison, bone) == null &&
                        economy.Count(biteCommand) == 1 &&
                        economy.Exact(biteCommand, false);
                    check("bone-poison-other-limb-negative", biteOnly,
                        new JObject {
                            ["command"] = Sprint27AiCommand(biteCommand),
                            ["attacks"] = new JArray(attacks),
                            ["poisonSaves"] = new JArray(
                                InfernalBoneSaveRows35(trace, biteStart,
                                    bone, target, 20)),
                            ["poison"] = InfernalBoneBuff35(target, poison,
                                bone) != null,
                            ["costs"] = biteCosts,
                            ["forcedRolls"] = 0
                        }, "A real wounding bite consumes a normal attack but does not deliver the sting-only injury poison.");
                }

                foreach (int frame in InfernalBoneWaitGame35(6.1, 15))
                    yield return frame;
                UnitAttack poisonCommand = null;
                int poisonStart = 0;
                Buff applied = null;
                int initialStrengthBefore = target.Stats.Strength.Damage;
                var poisonAttempts = new JArray();
                for (int attempt = 0; attempt < 3 && applied == null;
                    attempt++)
                {
                    poisonStart = trace.Rows.Count;
                    poisonCommand = new UnitAttack(target) {
                        IsSingleAttack = false,
                        ForceFullAttack = true
                    };
                    bone.Commands.Run(poisonCommand);
                    foreach (int frame in WaitInfernalCommand34(poisonCommand,
                        () => true, 30)) yield return frame;
                    applied = InfernalBoneBuff35(target, poison, bone);
                    JObject[] attacks = InfernalBoneAttackRows35(trace,
                        poisonStart, bone, target, poisonCommand).ToArray();
                    poisonAttempts.Add(new JObject {
                        ["attempt"] = attempt + 1,
                        ["command"] = Sprint27AiCommand(poisonCommand),
                        ["plan"] = InfernalBonePlan35(poisonCommand),
                        ["attacks"] = new JArray(attacks),
                        ["saves"] = new JArray(InfernalBoneSaveRows35(trace,
                            poisonStart, bone, target, 20)),
                        ["poison"] = applied == null ? null :
                            InfernalBonePoison35(applied),
                        ["forcedRolls"] = 0
                    });
                    if (applied == null)
                    {
                        foreach (int frame in InfernalBoneWaitGame35(6.1,
                            15)) yield return frame;
                    }
                }
                if (applied == null) throw new InvalidOperationException(
                    "Three bounded natural full attacks did not deliver Bone poison.");
                JObject[] poisonAttacks = InfernalBoneAttackRows35(trace,
                    poisonStart, bone, target, poisonCommand).ToArray();
                JObject[] injurySaves = InfernalBoneSaveRows35(trace,
                    poisonStart, bone, target, 20).ToArray();
                JObject[] initialStat = InfernalBoneStatRows35(trace,
                    poisonStart, bone, target).ToArray();
                JArray plan = InfernalBonePlan35(poisonCommand);
                bool fullPlan = poisonCommand.IsActed &&
                    (poisonCommand.Result == UnitCommand.ResultType.Success ||
                     poisonCommand.Result == UnitCommand.ResultType.Interrupt) &&
                    plan.Count == 4 &&
                    plan.Count(value => (string)value["weapon"] ==
                        profile.Bite.AssetGuid) == 1 &&
                    plan.Count(value => (string)value["weapon"] ==
                        profile.Claw.AssetGuid) == 2 &&
                    plan.Count(value => (string)value["weapon"] ==
                        profile.Sting.AssetGuid) == 1 &&
                    plan.All(value => (int)value["penalty"] == 0) &&
                    poisonAttacks.Count(value =>
                        (string)value["weapon"] == profile.Sting.AssetGuid &&
                        (bool?)value["hit"] == true &&
                        (int?)value["hpDamage"] > 0) == 1 &&
                    injurySaves.Length == 1 &&
                    (bool?)injurySaves[0]["passed"] == false &&
                    initialStat.Length == 1 &&
                    InfernalBoneNativeStatDamage35(initialStat[0],
                        statDamageScale) &&
                    target.Stats.Strength.Damage - initialStrengthBefore ==
                        (int)initialStat[0]["damage"] &&
                    ReferenceEquals(applied.Context?.MaybeCaster, bone) &&
                    applied.Context?.Params?.DC == 20;
                check("bone-sting-poison", fullPlan, new JObject {
                    ["owner"] = bone.UniqueId,
                    ["target"] = target.UniqueId,
                    ["attempts"] = poisonAttempts,
                    ["acceptedPlan"] = plan,
                    ["acceptedAttacks"] = new JArray(poisonAttacks),
                    ["injurySaves"] = new JArray(injurySaves),
                    ["initialStatDamage"] = new JArray(initialStat),
                    ["strengthDamageBefore"] = initialStrengthBefore,
                    ["strengthDamageAfter"] =
                        target.Stats.Strength.Damage,
                    ["nativeDifficultyDamageScale"] = statDamageScale,
                    ["targetIsPlayersEnemy"] = target.IsPlayersEnemy,
                    ["poisonState"] = InfernalBonePoison35(applied)
                }, "One real equal-role bite/claw/claw/sting plan delivers exactly one DC20 finite Strength poison from its wounding sting, without donor-edition secondary penalties.");

                int cadenceStart = trace.Rows.Count;
                int laterStrengthBefore = target.Stats.Strength.Damage;
                double cadenceBegan = Game.Instance.TimeController.GameTime
                    .TotalSeconds;
                foreach (int frame in InfernalBoneWaitGame35(6.2, 15))
                    yield return frame;
                JObject[] laterSaves = InfernalBoneSaveRows35(trace,
                    cadenceStart, bone, target, 20).ToArray();
                JObject[] laterStat = InfernalBoneStatRows35(trace,
                    cadenceStart, bone, target).ToArray();
                bool laterExposure = laterSaves.Length == 1 &&
                    ((bool?)laterSaves[0]["passed"] == true ?
                        laterStat.Length == 0 &&
                        target.Stats.Strength.Damage == laterStrengthBefore :
                        laterStat.Length == 1 &&
                        InfernalBoneNativeStatDamage35(laterStat[0],
                            statDamageScale) &&
                        target.Stats.Strength.Damage -
                            laterStrengthBefore ==
                            (int)laterStat[0]["damage"]) &&
                    InfernalBoneBuff35(target, poison, bone) != null;

                target.Stats.SaveFortitude.BaseValue = 100;
                int cureStart = trace.Rows.Count;
                DateTime cureDeadline = DateTime.UtcNow.AddSeconds(45);
                double cureGameDeadline = Game.Instance.TimeController.GameTime
                    .TotalSeconds + 36d;
                while (DateTime.UtcNow < cureDeadline &&
                    Game.Instance.TimeController.GameTime.TotalSeconds <
                        cureGameDeadline &&
                    InfernalBoneBuff35(target, poison, bone) != null)
                {
                    Game.Instance.IsPaused = false;
                    yield return 0;
                }
                JObject[] cureSaves = InfernalBoneSaveRows35(trace,
                    cureStart, bone, target, 20).ToArray();
                bool cured = InfernalBoneBuff35(target, poison, bone) ==
                        null &&
                    cureSaves.Length >= 2 &&
                    (bool?)cureSaves[cureSaves.Length - 1]["passed"] == true &&
                    (bool?)cureSaves[cureSaves.Length - 2]["passed"] == true;
                check("bone-poison-cadence-cure", laterExposure && cured,
                    new JObject {
                        ["laterElapsedGameSeconds"] = Game.Instance
                            .TimeController.GameTime.TotalSeconds -
                            cadenceBegan,
                        ["laterSaves"] = new JArray(laterSaves),
                        ["laterStatDamage"] = new JArray(laterStat),
                        ["strengthDamageBefore"] = laterStrengthBefore,
                        ["strengthDamageAfter"] =
                            target.Stats.Strength.Damage,
                        ["nativeDifficultyDamageScale"] = statDamageScale,
                        ["highFortBase"] = 100,
                        ["cureSaves"] = new JArray(cureSaves),
                        ["presentAfterCure"] = InfernalBoneBuff35(target,
                            poison, bone) != null,
                        ["forcedRolls"] = 0
                    }, "The victim-owned poison reaches its next native round exposure, preserves DC20 and 1d3 Strength resolution, then requires two consecutive real Fortitude successes to cure.");

                target.Stats.SaveFortitude.BaseValue = -100;
                foreach (int frame in InfernalBoneWaitGame35(6.1, 15))
                    yield return frame;
                var selfDecision = new DecisionContext {
                    Unit = bone,
                    Target = new TargetInfo { Unit = bone }
                };
                bool standardAiBefore = invisAi.IsAvailable(selfDecision);
                bool quickAiBefore = quickAi.IsAvailable(selfDecision);
                UnitUseAbility standardCommand = null;
                var standardCosts = new JArray();
                AbilityData standardData = new AbilityData(bone.Descriptor
                    .Abilities.GetAbility(invisibility));
                var standardParams = standardData.CalculateParams();
                using (var economy = new FirearmReviewCosts(bone,
                    standardCosts))
                {
                    foreach (int frame in Sprint26Cast(bone, invisibility,
                        new TargetWrapper(bone), value =>
                            standardCommand = value)) yield return frame;
                    Buff standardEffect = InfernalBoneBuff35(bone,
                        invisible, bone);
                    JObject standardEffectRow = standardEffect == null ?
                        null : InfernalSavedEffect34(standardEffect);
                    bool activeGuard = standardEffect != null &&
                        ReferenceEquals(standardEffect.Blueprint, invisible) &&
                        !invisAi.IsAvailable(selfDecision) &&
                        !quickAi.IsAvailable(selfDecision);
                    foreach (int frame in InfernalBoneWaitGame35(6.1, 15))
                        yield return frame;
                    int breakStart = trace.Rows.Count;
                    var breakAttack = new UnitAttack(target) {
                        IsSingleAttack = true,
                        ForceFullAttack = false
                    };
                    bone.Commands.Run(breakAttack);
                    foreach (int frame in WaitInfernalCommand34(breakAttack,
                        () => InfernalBoneBuff35(bone, invisible, bone) ==
                            null, 25)) yield return frame;
                    JObject[] breakAttacks = InfernalBoneAttackRows35(trace,
                        breakStart, bone, target, breakAttack).ToArray();
                    bool standardExact = standardAiBefore && quickAiBefore &&
                        standardCommand != null &&
                        standardCommand.Result ==
                            UnitCommand.ResultType.Success &&
                        standardParams.CasterLevel == 12 &&
                        standardParams.SpellLevel == 2 &&
                        invisibility.ActionType ==
                            UnitCommand.CommandType.Standard &&
                        economy.Count(standardCommand) == 1 &&
                        economy.Exact(standardCommand, false) &&
                        activeGuard && breakAttacks.Length == 1 &&
                        (string)breakAttacks[0]["weapon"] ==
                            profile.Bite.AssetGuid &&
                        InfernalBoneBuff35(bone, invisible, bone) == null;
                    check("bone-standard-invisibility", standardExact,
                        new JObject {
                            ["command"] = Sprint27AiCommand(standardCommand),
                            ["casterLevel"] = standardParams.CasterLevel,
                            ["spellLevel"] = standardParams.SpellLevel,
                            ["aiBefore"] = standardAiBefore,
                            ["quickAiBefore"] = quickAiBefore,
                            ["bothAiRefusedWhileActive"] = activeGuard,
                            ["effect"] = standardEffectRow,
                            ["ownerIsInGame"] = bone.IsInGame,
                            ["targetIsInGame"] = target.IsInGame,
                            ["breaksOnOffensiveAction"] =
                                !invisibilityBehavior
                                    .NotDispellAfterOffensiveAction,
                            ["breakingAttack"] = new JArray(breakAttacks),
                            ["effectAfterAttack"] = InfernalBoneBuff35(bone,
                                invisible, bone) != null,
                            ["costs"] = standardCosts
                        }, "A real standard-action ordinary Invisibility cast is source-attributed, makes both AI actions refuse redundant recasts, and a real bite breaks it.");
                }

                var quickRows = new JArray();
                for (int use = 0; use < 3; use++)
                {
                    foreach (int frame in InfernalBoneWaitGame35(6.1, 15))
                        yield return frame;
                    int before = bone.Descriptor.Resources.GetResourceAmount(
                        quickResource);
                    AbilityData quickData = new AbilityData(bone.Descriptor
                        .Abilities.GetAbility(quick));
                    var quickParams = quickData.CalculateParams();
                    bool available = quickData.IsAvailable &&
                        quickAi.IsAvailable(selfDecision);
                    UnitUseAbility quickCommand = null;
                    var costs = new JArray();
                    using (var economy = new FirearmReviewCosts(bone, costs))
                    {
                        foreach (int frame in Sprint26Cast(bone, quick,
                            new TargetWrapper(bone), value =>
                                quickCommand = value)) yield return frame;
                        int after = bone.Descriptor.Resources
                            .GetResourceAmount(quickResource);
                        Buff effect = InfernalBoneBuff35(bone, invisible,
                            bone);
                        bool activeGuard = effect != null &&
                            !invisAi.IsAvailable(selfDecision) &&
                            !quickAi.IsAvailable(selfDecision);
                        int breakStart = trace.Rows.Count;
                        var breakAttack = new UnitAttack(target) {
                            IsSingleAttack = true,
                            ForceFullAttack = false
                        };
                        bone.Commands.Run(breakAttack);
                        foreach (int frame in WaitInfernalCommand34(
                            breakAttack, () => InfernalBoneBuff35(bone,
                                invisible, bone) == null, 25))
                            yield return frame;
                        JObject[] attacks = InfernalBoneAttackRows35(trace,
                            breakStart, bone, target, breakAttack).ToArray();
                        bool exact = available && quickCommand != null &&
                            quickCommand.Result ==
                                UnitCommand.ResultType.Success &&
                            quickParams.CasterLevel == 12 &&
                            quickParams.SpellLevel == 2 &&
                            quick.ActionType == UnitCommand.CommandType.Swift &&
                            before == 3 - use && after == 2 - use &&
                            economy.Count(quickCommand) == 1 &&
                            economy.Exact(quickCommand, false) &&
                            activeGuard && attacks.Length == 1 &&
                            (string)attacks[0]["weapon"] ==
                                profile.Bite.AssetGuid &&
                            InfernalBoneBuff35(bone, invisible, bone) == null;
                        quickRows.Add(new JObject {
                            ["use"] = use + 1,
                            ["availableBefore"] = available,
                            ["command"] = Sprint27AiCommand(quickCommand),
                            ["resourceBefore"] = before,
                            ["resourceAfter"] = after,
                            ["casterLevel"] = quickParams.CasterLevel,
                            ["spellLevel"] = quickParams.SpellLevel,
                            ["bothAiRefusedWhileActive"] = activeGuard,
                            ["breakingAttack"] = new JArray(attacks),
                            ["effectAfterAttack"] = InfernalBoneBuff35(bone,
                                invisible, bone) != null,
                            ["costs"] = costs,
                            ["passed"] = exact
                        });
                        if (!exact) throw new InvalidOperationException(
                            "Bone quickened invisibility use failed: " +
                            (use + 1));
                    }
                }
                AbilityData spentQuick = new AbilityData(bone.Descriptor
                    .Abilities.GetAbility(quick));
                bool spent = bone.Descriptor.Resources.GetResourceAmount(
                        quickResource) == 0 && !spentQuick.IsAvailable &&
                    !quickAi.IsAvailable(selfDecision) &&
                    quickRows.All(value => (bool)value["passed"]);
                check("bone-quickened-invisibility", spent,
                    new JObject {
                        ["uses"] = quickRows,
                        ["resourceFinal"] = bone.Descriptor.Resources
                            .GetResourceAmount(quickResource),
                        ["abilityAvailableAfterSpend"] =
                            spentQuick.IsAvailable,
                        ["aiAvailableAfterSpend"] =
                            quickAi.IsAvailable(selfDecision),
                        ["forcedRolls"] = 0
                    }, "Three real swift-action ordinary Invisibility casts each spend one use, refuse active-effect recasts, break on real attacks, and end at an unavailable zero-use resource without toggling or mode changes.");

                foreach (int frame in InfernalBoneWaitGame35(6.1, 15))
                    yield return frame;
                target.Stats.SaveFortitude.BaseValue = -100;
                Buff survivingPoison = null;
                UnitAttack survivalCommand = null;
                int survivalStart = 0;
                for (int attempt = 0; attempt < 3 &&
                    survivingPoison == null; attempt++)
                {
                    survivalStart = trace.Rows.Count;
                    survivalCommand = new UnitAttack(target) {
                        IsSingleAttack = false,
                        ForceFullAttack = true
                    };
                    bone.Commands.Run(survivalCommand);
                    foreach (int frame in WaitInfernalCommand34(
                        survivalCommand, () => true, 30)) yield return frame;
                    survivingPoison = InfernalBoneBuff35(target, poison,
                        bone);
                    if (survivingPoison == null)
                        foreach (int frame in InfernalBoneWaitGame35(6.1,
                            15)) yield return frame;
                }
                if (survivingPoison == null)
                    throw new InvalidOperationException(
                        "Source-lifetime poison precondition was not naturally delivered.");
                int savedDc = survivingPoison.Context.Params.DC;
                int strengthBeforeSourceLoss = target.Stats.Strength.Damage;
                DisposeExpandedSummoningUnits(fixture.Created,
                    new[] { bone });
                foreach (int frame in InfernalBoneWaitUntil35(() =>
                    bone.Destroyed && Sprint29OwnedAreas(bone, fearArea)
                        .Length == 0, 5)) yield return frame;
                int laterStart = trace.Rows.Count;
                double sourceLossBegan = Game.Instance.TimeController.GameTime
                    .TotalSeconds;
                foreach (int frame in InfernalBoneWaitGame35(6.2, 15))
                    yield return frame;
                Buff afterSourceLoss = InfernalBoneBuff35(target, poison,
                    bone);
                JObject[] sourceLossSaves = InfernalBoneSaveRows35(trace,
                    laterStart, bone, target, 20).ToArray();
                JObject[] sourceLossStat = InfernalBoneStatRows35(trace,
                    laterStart, bone, target).ToArray();
                bool poisonSurvives = bone.Destroyed &&
                    afterSourceLoss != null && savedDc == 20 &&
                    afterSourceLoss.Context.Params.DC == savedDc &&
                    ReferenceEquals(afterSourceLoss.Context.MaybeCaster,
                        bone) && sourceLossSaves.Length == 1 &&
                    ((bool?)sourceLossSaves[0]["passed"] == true ?
                        sourceLossStat.Length == 0 &&
                        target.Stats.Strength.Damage ==
                            strengthBeforeSourceLoss :
                        sourceLossStat.Length == 1 &&
                        InfernalBoneNativeStatDamage35(sourceLossStat[0],
                            statDamageScale) &&
                        target.Stats.Strength.Damage -
                            strengthBeforeSourceLoss ==
                            (int)sourceLossStat[0]["damage"]) &&
                    Sprint29OwnedAreas(bone, fearArea).Length == 0;
                check("bone-poison-source-independence", poisonSurvives,
                    new JObject {
                        ["owner"] = bone.UniqueId,
                        ["ownerDestroyed"] = bone.Destroyed,
                        ["deliveryCommand"] = Sprint27AiCommand(
                            survivalCommand),
                        ["deliveryAttacks"] = new JArray(
                            InfernalBoneAttackRows35(trace, survivalStart,
                                bone, target, survivalCommand)),
                        ["dcBefore"] = savedDc,
                        ["after"] = afterSourceLoss == null ? null :
                            InfernalBonePoison35(afterSourceLoss),
                        ["laterSaves"] = new JArray(sourceLossSaves),
                        ["laterStatDamage"] = new JArray(sourceLossStat),
                        ["strengthDamageBefore"] =
                            strengthBeforeSourceLoss,
                        ["strengthDamageAfter"] = target.Stats.Strength.Damage,
                        ["nativeDifficultyDamageScale"] = statDamageScale,
                        ["elapsedGameSeconds"] = Game.Instance.TimeController
                            .GameTime.TotalSeconds - sourceLossBegan,
                        ["ownedAuraAreas"] = Sprint29OwnedAreas(bone,
                            fearArea).Length,
                        ["forcedRolls"] = 0
                    }, "Destroying the exact Bone source ends its active aura but does not erase or retarget already-inflicted victim-owned poison or Strength damage; the next native exposure retains DC20.");

                InfernalBoneRemoveEffects35(target, new[] { bone }, poison);
                InfernalBoneRemoveEffects35(target, fearOwners,
                    frightened, memory);
                InfernalBoneRemoveEffects35(ally, fearOwners,
                    frightened, memory);
                target.Stats.AC.BaseValue = oldTargetAc;
                target.Stats.SaveFortitude.BaseValue = oldTargetFort;
                target.Stats.SaveWill.BaseValue = oldTargetWill;
                target.Stats.HitPoints.BaseValue = oldTargetHp;
                target.Stats.Strength.BaseValue = oldTargetStrength;
                target.Stats.Strength.Damage = oldTargetStrengthDamage;
                target.Damage = oldTargetDamage;
                ally.Stats.SaveWill.BaseValue = oldAllyWill;
                targetFaction.AttackFactions = oldTargetAttackFactions;
                PlaceExpandedSummoningUnit(target, oldTargetPosition);
                target.Orientation = oldTargetOrientation;
                PlaceExpandedSummoningUnit(ally, oldAllyPosition);
                ally.Orientation = oldAllyOrientation;
                UnitEntityData[] remaining = owned.Where(value =>
                    value != null && !value.Destroyed).ToArray();
                DisposeExpandedSummoningUnits(fixture.Created, remaining);
                foreach (int frame in InfernalBoneWaitUntil35(() =>
                    owned.All(value => value.Destroyed) &&
                    !Game.Instance.State.AreaEffects.All.Any(value =>
                        owned.Any(source => ReferenceEquals(
                            value?.Context?.MaybeCaster, source))), 5))
                    yield return frame;
                bool cleanup = owned.All(value => value.Destroyed &&
                        !value.IsInState) &&
                    !Game.Instance.State.AreaEffects.All.Any(value =>
                        owned.Any(source => ReferenceEquals(
                            value?.Context?.MaybeCaster, source))) &&
                    InfernalBoneBuff35(target, poison, bone) == null &&
                    fearOwners.All(source =>
                        InfernalBoneBuff35(target, memory, source) == null &&
                        InfernalBoneBuff35(target, frightened, source) ==
                            null &&
                        InfernalBoneBuff35(ally, memory, source) == null &&
                        InfernalBoneBuff35(ally, frightened, source) ==
                            null) &&
                    target.Stats.AC.BaseValue == oldTargetAc &&
                    target.Stats.SaveFortitude.BaseValue == oldTargetFort &&
                    target.Stats.SaveWill.BaseValue == oldTargetWill &&
                    target.Stats.HitPoints.BaseValue == oldTargetHp &&
                    target.Stats.Strength.BaseValue == oldTargetStrength &&
                    target.Stats.Strength.Damage ==
                        oldTargetStrengthDamage &&
                    target.Damage == oldTargetDamage &&
                    ally.Stats.SaveWill.BaseValue == oldAllyWill &&
                    ReferenceEquals(targetFaction.AttackFactions,
                        oldTargetAttackFactions);
                check("bone-owned-cleanup", cleanup, new JObject {
                    ["actors"] = new JArray(owned.Select(value =>
                        new JObject {
                            ["unit"] = value.UniqueId,
                            ["destroyed"] = value.Destroyed,
                            ["inState"] = value.IsInState
                        })),
                    ["ownedAreas"] = Game.Instance.State.AreaEffects.All
                        .Count(value => owned.Any(source => ReferenceEquals(
                            value?.Context?.MaybeCaster, source))),
                    ["poisonPresent"] = InfernalBoneBuff35(target, poison,
                        bone) != null,
                    ["targetStatsRestored"] = target.Stats.AC.BaseValue ==
                            oldTargetAc &&
                        target.Stats.SaveFortitude.BaseValue == oldTargetFort &&
                        target.Stats.SaveWill.BaseValue == oldTargetWill &&
                        target.Stats.Strength.Damage ==
                            oldTargetStrengthDamage &&
                        target.Damage == oldTargetDamage,
                    ["allyWillRestored"] = ally.Stats.SaveWill.BaseValue ==
                        oldAllyWill,
                    ["privateHostilityRestored"] = ReferenceEquals(
                        targetFaction.AttackFactions,
                        oldTargetAttackFactions),
                    ["fixtureOnlyAbilityDamageRestore"] = true
                }, "The grouped transaction retires every owned actor and active aura, removes only fixture-owned timed effects, and restores the disposable victims after recording that production poison and ability damage survived source loss.");
            }
            finally
            {
                trace.Dispose();
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(
                    _request.EvidenceDirectory,
                    "sprint35-infernal-bone.json"),
                    receipt.ToString(Formatting.Indented));
                InfernalBoneRemoveEffects35(target, owned, poison,
                    frightened, memory, invisible);
                InfernalBoneRemoveEffects35(ally, owned, frightened, memory);
                if (targetFaction != null)
                    targetFaction.AttackFactions = oldTargetAttackFactions;
                if (target != null && !target.Destroyed)
                {
                    target.Stats.AC.BaseValue = oldTargetAc;
                    target.Stats.SaveFortitude.BaseValue = oldTargetFort;
                    target.Stats.SaveWill.BaseValue = oldTargetWill;
                    target.Stats.HitPoints.BaseValue = oldTargetHp;
                    target.Stats.Strength.BaseValue = oldTargetStrength;
                    target.Stats.Strength.Damage = oldTargetStrengthDamage;
                    target.Damage = oldTargetDamage;
                    PlaceExpandedSummoningUnit(target, oldTargetPosition);
                    target.Orientation = oldTargetOrientation;
                    if (target.IsInCombat) target.LeaveCombat();
                }
                if (ally != null && !ally.Destroyed)
                {
                    ally.Stats.SaveWill.BaseValue = oldAllyWill;
                    PlaceExpandedSummoningUnit(ally, oldAllyPosition);
                    ally.Orientation = oldAllyOrientation;
                    if (ally.IsInCombat) ally.LeaveCombat();
                }
                foreach (UnitEntityData actor in owned.Where(value =>
                    value != null && !value.Destroyed))
                {
                    Sprint28SetBrainActivePreservingActions(actor, false);
                    actor.Commands.InterruptAll(true);
                    actor.Commands.RemoveFinishedAndUpdateQueue();
                    if (actor.IsInCombat) actor.LeaveCombat();
                }
                Game.Instance.Player.UpdateIsInCombat();
                DisposeExpandedSummoningUnits(fixture.Created,
                    owned.Where(value => value != null && !value.Destroyed)
                        .ToArray());
                ResetExpandedSummoningHostile(fixture);
            }
        }

        private static IEnumerable<JObject> InfernalBoneRows35(
            InfernalEvents34 trace, int start, string kind,
            UnitEntityData owner, UnitEntityData target)
        {
            return trace.Rows.Skip(start).OfType<JObject>().Where(value =>
                (string)value["event"] == kind &&
                (string)value["owner"] == owner?.UniqueId &&
                (string)value["target"] == target?.UniqueId);
        }

        private static IEnumerable<JObject> InfernalBoneSaveRows35(
            InfernalEvents34 trace, int start, UnitEntityData owner,
            UnitEntityData target, int dc)
        {
            return InfernalBoneRows35(trace, start, "save", owner, target)
                .Where(value => (int?)value["dc"] == dc);
        }

        private static IEnumerable<JObject> InfernalBoneStatRows35(
            InfernalEvents34 trace, int start, UnitEntityData owner,
            UnitEntityData target)
        {
            return trace.Rows.Skip(start).OfType<JObject>().Where(value =>
                (string)value["event"] == "stat-damage" &&
                (string)value["target"] == target?.UniqueId &&
                (string)value["source"] == owner?.UniqueId &&
                (string)value["stat"] == StatType.Strength.ToString());
        }

        private static bool InfernalBoneNativeStatDamage35(JObject row,
            float difficultyScale)
        {
            if (row == null) return false;
            int? roll = (int?)row["bonus"];
            int? damage = (int?)row["damage"];
            // BuffPoisonStatDamage pre-rolls its configured DiceFormula and
            // dispatches the result as RuleDealStatDamage.Bonus. The installed
            // engine therefore reports zero remaining dice here. DamageToParty
            // is the native selected-difficulty multiplier used by this saved
            // hostile fixture, and RuleDealStatDamage truncates the scaled
            // value. Preserve that observed engine difference instead of
            // forcing a favorable poison roll.
            return roll >= 1 && roll <= 3 && damage >= 0 &&
                (string)row["dice"] == "0" &&
                damage == (int)(roll.Value * difficultyScale);
        }

        private static IEnumerable<JObject> InfernalBoneAttackRows35(
            InfernalEvents34 trace, int start, UnitEntityData owner,
            UnitEntityData target, UnitAttack command)
        {
            int reference = command == null ? 0 :
                RuntimeHelpers.GetHashCode(command);
            return InfernalBoneRows35(trace, start, "attack", owner, target)
                .Where(value => (int?)value["attackCommand"] == reference);
        }

        private static JArray InfernalBonePlan35(UnitAttack command)
        {
            if (command == null) return new JArray();
            return new JArray(command.AllAttacks.Select(value =>
                    new JObject {
                        ["weapon"] = value.Hand?.MaybeWeapon?.Blueprint?
                            .AssetGuid,
                        ["itemReference"] = value.Hand?.MaybeWeapon == null ?
                            (int?)null : RuntimeHelpers.GetHashCode(
                                value.Hand.MaybeWeapon),
                        ["penalty"] = value.AttackBonusPenalty,
                        ["number"] = value.AttackNumber
                    }));
        }

        private static Buff InfernalBoneBuff35(UnitEntityData victim,
            BlueprintBuff blueprint, UnitEntityData source)
        {
            if (victim == null || blueprint == null) return null;
            return victim.Buffs.Enumerable.FirstOrDefault(value =>
                ReferenceEquals(value.Blueprint, blueprint) &&
                ReferenceEquals(value.Context?.MaybeCaster, source));
        }

        private static void InfernalBoneRemoveEffects35(
            UnitEntityData victim, IEnumerable<UnitEntityData> sources,
            params BlueprintBuff[] blueprints)
        {
            if (victim == null || victim.Destroyed || sources == null ||
                blueprints == null) return;
            UnitEntityData[] exactSources = sources.Where(value =>
                value != null).Distinct().ToArray();
            BlueprintBuff[] exactBlueprints = blueprints.Where(value =>
                value != null).Distinct().ToArray();
            foreach (Buff buff in victim.Buffs.Enumerable.Where(value =>
                exactBlueprints.Contains(value.Blueprint) &&
                exactSources.Contains(value.Context?.MaybeCaster)).ToArray())
                buff.Remove();
        }

        private static JObject InfernalBonePoison35(Buff buff)
        {
            BuffPoisonStatDamage logic = buff?.Components.OfType<
                BuffPoisonStatDamage>().SingleOrDefault();
            return new JObject {
                ["present"] = buff != null,
                ["blueprint"] = buff?.Blueprint?.AssetGuid,
                ["source"] = buff?.Context?.MaybeCaster?.UniqueId,
                ["dc"] = buff?.Context?.Params?.DC,
                ["round"] = buff?.RoundNumber,
                ["ticksConfigured"] = logic?.Ticks,
                ["cureSavesConfigured"] = logic?.SuccesfullSaves,
                ["stat"] = logic?.Stat.ToString(),
                ["dice"] = logic?.Value.ToString(),
                ["timeLeftSeconds"] = buff?.TimeLeft.TotalSeconds
            };
        }

        private IEnumerable<int> InfernalBoneWaitGame35(double seconds,
            int wallSeconds)
        {
            double until = Game.Instance.TimeController.GameTime.TotalSeconds +
                seconds;
            DateTime deadline = DateTime.UtcNow.AddSeconds(wallSeconds);
            while (DateTime.UtcNow < deadline &&
                Game.Instance.TimeController.GameTime.TotalSeconds < until)
            {
                Game.Instance.IsPaused = false;
                yield return 0;
            }
            if (Game.Instance.TimeController.GameTime.TotalSeconds < until)
                throw new InvalidOperationException(
                    "Bone native clock did not advance by " + seconds +
                    " seconds.");
        }

        private IEnumerable<int> InfernalBoneWaitUntil35(Func<bool> ready,
            int wallSeconds)
        {
            DateTime deadline = DateTime.UtcNow.AddSeconds(wallSeconds);
            while (DateTime.UtcNow < deadline && !ready())
                yield return 0;
            if (!ready()) throw new InvalidOperationException(
                "Bone native cleanup did not settle.");
        }
    }
}
