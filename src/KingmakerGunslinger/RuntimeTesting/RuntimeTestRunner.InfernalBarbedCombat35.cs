using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Mechanics.Components;
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
        /// The remaining Barbed combat slice uses real attack and spell
        /// commands. The observer changes only disposable accuracy, saves,
        /// CMD, size, and immunity preconditions; production event handlers
        /// establish every fear, grapple, retaliation, ray, and damage result.
        /// </summary>
        private IEnumerable<int> ReviewInfernalBarbedCombat35(
            ExpandedSummoningCorrectionFixture fixture)
        {
            var owned = new List<UnitEntityData>();
            var cells = new JArray();
            var trace = new InfernalEvents34();
            var receipt = new JObject {
                ["phase"] = ExpandedSummoningNativeProfilePolicy
                    .Sprint35InfernalBarbedCombatPhase,
                ["finalQualification"] = false,
                ["cells"] = cells,
                ["events"] = trace.Rows
            };
            UnitEntityData owner = null;
            UnitEntityData target = fixture.Hostile;
            BlueprintFeature mindImmunity = null;
            BlueprintBuff frightened = null;
            int oldAc = target.Stats.AC.BaseValue;
            int oldWill = target.Stats.SaveWill.BaseValue;
            int oldHitPoints = target.Stats.HitPoints.BaseValue;
            int oldCmb = target.Stats.AdditionalCMB.BaseValue;
            int oldCmd = target.Stats.AdditionalCMD.BaseValue;
            int oldMobility = target.Stats.SkillMobility.BaseValue;
            int oldAthletics = target.Stats.SkillAthletics.BaseValue;
            int oldDamage = target.Damage;
            Size oldSize = target.Descriptor.State.Size;
            bool maneuverImmunityAdded = false;
            Action<string, bool, JObject, string> check =
                (id, pass, row, description) => {
                    row["cell"] = id;
                    cells.Add(row);
                    Sprint25Check("sprint35-" + id, pass, row, description);
                    if (!pass) throw new InvalidOperationException(
                        "Sprint35 Barbed combat cell failed: " + id);
                };
            try
            {
                owner = CastExpandedSummoningOwnTier(fixture,
                    "barbed-devil");
                owned.Add(owner);
                Sprint28SetBrainActivePreservingActions(owner, false);
                owner.Commands.InterruptAll(true);
                owner.Commands.RemoveFinishedAndUpdateQueue();
                foreach (int frame in Outsider30NativeAppearance(owner,
                    owner.Blueprint.name)) yield return frame;

                var profile = InfernalMartialProfile35.For(owner);
                var grab = SummonGrabComponent.Find(owner);
                var featureGrab = owner.Descriptor.Progression.Features
                    .Enumerable.Where(value => value != null &&
                        value.IsTurnedOn).SelectMany(value =>
                            value.SelectComponents<SummonGrabComponent>())
                    .SingleOrDefault();
                string prefix = ExpandedSummoningSpecialBuilder.Barbed35Prefix;
                frightened = (BlueprintBuff)ExpandedSummoningBlueprints
                    .Registered.BySymbol[prefix + "FearFrightened"];
                var ray = (BlueprintAbility)ExpandedSummoningBlueprints
                    .Registered.BySymbol[prefix + "ScorchingRay"];
                var defense = owner.Descriptor.Progression.Features.Enumerable
                    .SelectMany(value => value.SelectComponents<
                        BarbedDefense35>()).SingleOrDefault();
                var fear = owner.Descriptor.Progression.Features.Enumerable
                    .SelectMany(value => value.SelectComponents<
                        BarbedClawFear35>()).SingleOrDefault();
                var delivery = ray.GetComponent<AbilityDeliverProjectile>();
                bool mapping = profile != null && profile.Claw != null &&
                    owner.Descriptor.State.Size == Size.Medium &&
                    owner.Body.PrimaryHand.MaybeWeapon != null &&
                    ReferenceEquals(owner.Body.PrimaryHand.MaybeWeapon
                        .Blueprint, profile.Claw) &&
                    owner.Body.AdditionalLimbs.Count == 1 &&
                    ReferenceEquals(owner.Body.AdditionalLimbs[0]
                        .MaybeWeapon?.Blueprint, profile.Claw) &&
                    defense != null && ReferenceEquals(defense.OwningBlueprint,
                        owner.Blueprint) && fear != null &&
                    ReferenceEquals(fear.OwningBlueprint, owner.Blueprint) &&
                    ReferenceEquals(fear.Claw, profile.Claw) &&
                    ReferenceEquals(fear.Frightened, frightened) &&
                    grab != null && grab.GrabWithPrimaryHand &&
                    ReferenceEquals(grab, featureGrab) &&
                    grab.GrabAdditionalLimbCount == 1 &&
                    grab.MaxTargetSizeDelta == 0 &&
                    grab.MaxHeldTargets == 1 &&
                    ReferenceEquals(grab.ImpaleProfileOwner,
                        owner.Blueprint) && delivery != null &&
                    delivery.Projectiles != null &&
                    delivery.Projectiles.Length == 2 &&
                    !delivery.UseMaxProjectilesCount;
                check("barbed-combat-mapping", mapping, new JObject {
                    ["owner"] = owner.UniqueId,
                    ["size"] = owner.Descriptor.State.Size.ToString(),
                    ["claw"] = profile?.Claw?.AssetGuid,
                    ["primary"] = owner.Body.PrimaryHand.MaybeWeapon?
                        .Blueprint?.AssetGuid,
                    ["additional"] = new JArray(owner.Body.AdditionalLimbs
                        .Select(value => value.MaybeWeapon?.Blueprint?
                            .AssetGuid)),
                    ["grabMaxTargetSizeDelta"] = grab?.MaxTargetSizeDelta,
                    ["activeFeatureGrabMatches"] = ReferenceEquals(grab,
                        featureGrab),
                    ["impaleOwner"] = grab?.ImpaleProfileOwner?.AssetGuid,
                    ["ray"] = ray.AssetGuid,
                    ["rayProjectiles"] = delivery?.Projectiles?.Length,
                    ["rayUsesMaxProjectileCount"] = delivery == null ?
                        (bool?)null : delivery.UseMaxProjectilesCount
                }, "The exact Medium Barbed root owns two primary claws, its four separate combat consumers, and an explicit two-projectile Scorching Ray graph.");

                // Close the weak-table isolation requirement with two real
                // defenders. A single hostile ordinary claw strikes each;
                // neither defender's event key can suppress the other's.
                var sibling = CastExpandedSummoningOwnTier(fixture,
                    "barbed-devil");
                var attacker = CastExpandedSummoningOwnTier(fixture,
                    InfernalVisualPolicy34.Barbazu);
                owned.Add(sibling);
                owned.Add(attacker);
                Sprint28SetBrainActivePreservingActions(sibling, false);
                Sprint28SetBrainActivePreservingActions(attacker, false);
                sibling.Commands.InterruptAll(true);
                attacker.Commands.InterruptAll(true);
                sibling.Stats.HitPoints.BaseValue = 100000;
                owner.Stats.HitPoints.BaseValue = 100000;
                sibling.Stats.AC.BaseValue = -100;
                owner.Stats.AC.BaseValue = -100;
                attacker.Stats.AdditionalAttackBonus.BaseValue = 100;
                attacker.Stats.HitPoints.BaseValue = 100000;
                attacker.GroupId = "KMG_S35_Barbed_Isolation_" +
                    attacker.UniqueId;
                attacker.Descriptor.SwitchFactions(target.Faction, true);
                attacker.UpdateGroup();
                var attackProfile = InfernalMartialProfile34.For(attacker);
                foreach (int frame in InfernalSwitch34(attacker,
                    attackProfile.Claw)) yield return frame;
                foreach (int frame in InfernalEarlyContact35(fixture,
                    attacker, owner, trace, check, "independent-first", 3,
                    true)) yield return frame;
                foreach (int frame in InfernalEarlyContact35(fixture,
                    attacker, sibling, trace, check, "independent-second", 3,
                    true)) yield return frame;
                foreach (int frame in InfernalSwitch34(attacker,
                    attackProfile.Glaive)) yield return frame;
                foreach (int frame in InfernalEarlyContact35(fixture,
                    attacker, owner, trace, check, "reach-glaive", 8,
                    false)) yield return frame;
                // Retain the already-settled second Barbed for the ray cell.
                // The grapple owner below must be allowed to keep its proved
                // live hold until normal owned-actor disposal; a test teardown
                // is not a second gameplay release mechanism.
                DisposeExpandedSummoningUnits(fixture.Created,
                    new[] { attacker });
                sibling.Commands.InterruptAll(true);
                sibling.Commands.RemoveFinishedAndUpdateQueue();

                owner.Stats.AdditionalAttackBonus.BaseValue = 100;
                owner.Stats.AdditionalCMB.BaseValue = 100;
                owner.Stats.HitPoints.BaseValue = 100000;
                target.Stats.AC.BaseValue = -100;
                target.Stats.SaveWill.BaseValue = -100;
                target.Stats.HitPoints.BaseValue = 100000;
                target.Stats.AdditionalCMB.BaseValue = -100;
                target.Stats.AdditionalCMD.BaseValue = -100;
                target.Stats.SkillMobility.BaseValue = -100;
                target.Stats.SkillAthletics.BaseValue = -100;
                target.Damage = 0;

                // Fear is proved first with maneuver immunity so the same
                // successful claw cannot turn this into a grapple assertion.
                target.Descriptor.State.AddCondition(
                    UnitCondition.ImmuneToCombatManeuvers, null);
                maneuverImmunityAdded = true;
                InfernalCombatLane34(fixture, owner, target, 3);
                int fearStart = trace.Rows.Count;
                var fearCosts = new JArray();
                var fearCommand = new UnitAttack(target) {
                    IsSingleAttack = true, ForceFullAttack = false };
                using (var economy = new FirearmReviewCosts(owner, fearCosts))
                {
                    owner.Commands.Run(fearCommand);
                    foreach (int frame in WaitInfernalCommand34(fearCommand,
                        () => true, 25)) yield return frame;
                    var attacks = InfernalBarbedRows35(trace, fearStart,
                        "attack", owner, target).ToArray();
                    var saves = InfernalBarbedRows35(trace, fearStart,
                        "save", owner, target).Where(value =>
                            (int?)value["dc"] == 20).ToArray();
                    Buff applied = target.Buffs.Enumerable.SingleOrDefault(
                        value => ReferenceEquals(value.Blueprint,
                                frightened) &&
                            ReferenceEquals(value.Context?.MaybeCaster,
                                owner));
                    bool exact = fearCommand.Result ==
                            UnitCommand.ResultType.Success &&
                        attacks.Length == 1 &&
                        (bool?)attacks[0]["hit"] == true &&
                        (int?)attacks[0]["hpDamage"] > 0 &&
                        (string)attacks[0]["weapon"] ==
                            profile.Claw.AssetGuid && saves.Length == 1 &&
                        (bool?)saves[0]["passed"] == false &&
                        applied != null && applied.TimeLeft.TotalSeconds > 0d &&
                        applied.TimeLeft.TotalSeconds <= 24.1d &&
                        ReferenceEquals(applied.Context?.AssociatedBlueprint,
                            frightened) &&
                        SummonHoldComponent.HeldTarget(owner) == null &&
                        economy.Count(fearCommand) == 1 &&
                        economy.Exact(fearCommand, false);
                    check("barbed-claw-fear", exact, new JObject {
                        ["owner"] = owner.UniqueId,
                        ["target"] = target.UniqueId,
                        ["command"] = Sprint27AiCommand(fearCommand),
                        ["attacks"] = new JArray(attacks),
                        ["saves"] = new JArray(saves),
                        ["frightened"] = applied == null ? null :
                            InfernalSavedEffect34(applied),
                        ["maneuverImmune"] = true,
                        ["held"] = SummonHoldComponent.HeldTarget(owner)?
                            .UniqueId,
                        ["costs"] = fearCosts,
                        ["forcedRolls"] = 0
                    }, "One real wounding claw makes exactly one DC20 Will save and applies the exact 1d4-round source-attributed Frightened effect; maneuver immunity keeps grab a separate predicate.");
                }
                target.Descriptor.State.RemoveCondition(
                    UnitCondition.ImmuneToCombatManeuvers);
                maneuverImmunityAdded = false;
                InfernalRemoveBarbedFear35(target, owner, frightened);

                var mind = ScriptableObject.CreateInstance<
                    BuffDescriptorImmunity>();
                mind.Descriptor = SpellDescriptor.MindAffecting;
                mindImmunity = ScriptableObject.CreateInstance<
                    BlueprintFeature>();
                mindImmunity.name = "KMG_Runtime_S35_Barbed_MindImmunity";
                mindImmunity.Ranks = 1;
                mindImmunity.ComponentsArray = new BlueprintComponent[] {
                    mind };
                target.Descriptor.AddFact(mindImmunity);

                var escapes = new OutsiderEscapeEvents(target);
                EventBus.Subscribe(escapes);
                try
                {
                    using (var maneuvers = new InfernalBarbedManeuvers35(owner,
                        target))
                    {
                        foreach (int frame in InfernalBarbedGrabNegative35(
                            fixture, owner, target, trace, maneuvers, grab,
                            frightened, Size.Large, false, "oversize", check))
                            yield return frame;
                        foreach (int frame in InfernalBarbedGrabNegative35(
                            fixture, owner, target, trace, maneuvers, grab,
                            frightened, Size.Medium, true, "maneuver-immune",
                            check))
                            yield return frame;

                    bool impaled = false;
                    bool establishmentRecorded = false;
                    var holdAttempts = new JArray();
                    for (int attempt = 0; attempt < 3 && !impaled; attempt++)
                    {
                        target.Descriptor.State.Size = Size.Medium;
                        target.Damage = 0;
                        InfernalRemoveBarbedFear35(target, owner, frightened);
                        if (ReferenceEquals(SummonHoldComponent.HeldTarget(
                                owner), target))
                            SummonHoldComponent.ReleaseLink(owner, target,
                                grab, false);
                        owner.Commands.InterruptAll(true);
                        owner.Commands.RemoveFinishedAndUpdateQueue();
                        InfernalCombatLane34(fixture, owner, target, 3);
                        int eventStart = trace.Rows.Count;
                        int maneuverStart = maneuvers.Rules.Count;
                        int escapeSkillStart = escapes.Skills.Count;
                        int escapeCmbStart = escapes.Cmb.Count;
                        int escapeManeuverStart = escapes.Maneuvers.Count;
                        var command = new UnitAttack(target) {
                            IsSingleAttack = true,
                            ForceFullAttack = false };
                        var costs = new JArray();
                        using (var economy = new FirearmReviewCosts(owner,
                            costs))
                        {
                            owner.Commands.Run(command);
                            foreach (int frame in WaitInfernalCommand34(command,
                                () => true, 25)) yield return frame;
                            var attacks = InfernalBarbedRows35(trace,
                                eventStart, "attack", owner, target).ToArray();
                            var initialManeuvers = maneuvers.Rules.Skip(
                                maneuverStart).ToArray();
                            var initialEscapeSkills = escapes.Skills.Skip(
                                escapeSkillStart).ToArray();
                            var initialEscapeCmb = escapes.Cmb.Skip(
                                escapeCmbStart).ToArray();
                            var initialEscapeManeuvers = escapes.Maneuvers.Skip(
                                escapeManeuverStart).ToArray();
                            bool initialTargetEscape =
                                InfernalBarbedTargetEscaped35(
                                    initialEscapeSkills,
                                    initialEscapeCmb,
                                    initialEscapeManeuvers);
                            Buff held = SummonHoldComponent.HeldState(owner,
                                target, grab);
                            bool commandResolvedAttack = command.IsActed &&
                                (command.Result ==
                                    UnitCommand.ResultType.Success ||
                                 command.Result ==
                                    UnitCommand.ResultType.Interrupt);
                            bool ownerPart = owner.Get<
                                UnitPartGrappleInitiator>() != null;
                            bool targetPart = target.Get<
                                UnitPartGrappleTarget>() != null;
                            bool paid = economy.Count(command) == 1 &&
                                economy.Exact(command, false);
                            bool established = commandResolvedAttack &&
                                attacks.Length == 1 &&
                                (bool?)attacks[0]["hit"] == true &&
                                (int?)attacks[0]["hpDamage"] > 0 &&
                                initialManeuvers.Length == 1 &&
                                SummonManeuverChecks.Succeeded(
                                    initialManeuvers[0]) &&
                                ReferenceEquals(SummonHoldComponent
                                    .HeldTarget(owner), target) &&
                                held != null && ownerPart && targetPart &&
                                ReferenceEquals(SummonGrappleLinks
                                    .EstablishingWeapon(owner, target),
                                    owner.Body.PrimaryHand.MaybeWeapon) &&
                                paid &&
                                !InfernalBarbedImpaleRows35(trace,
                                    eventStart, owner, target).Any();
                            var attemptRow = new JObject {
                                ["attempt"] = attempt + 1,
                                ["command"] = Sprint27AiCommand(command),
                                ["attacks"] = new JArray(attacks),
                                ["initialManeuvers"] =
                                    InfernalBarbedManeuverRows35(
                                        initialManeuvers),
                                ["initialTargetEscapeSkills"] = new JArray(
                                    initialEscapeSkills.Select(
                                        OutsiderEscapeSkill)),
                                ["initialTargetEscapeCmb"] = new JArray(
                                    initialEscapeCmb.Select(value =>
                                        value.Result)),
                                ["initialTargetEscapeManeuvers"] =
                                    InfernalBarbedManeuverRows35(
                                        initialEscapeManeuvers),
                                ["established"] = established,
                                ["commandActed"] = command.IsActed,
                                ["commandResultAccepted"] =
                                    commandResolvedAttack,
                                ["heldTarget"] = SummonHoldComponent
                                    .HeldTarget(owner)?.UniqueId,
                                ["holdBuffPresent"] = held != null,
                                ["ownerInitiatorPart"] = ownerPart,
                                ["targetGrapplePart"] = targetPart,
                                ["grappledBuffPresent"] = target.Buffs
                                    .GetBuff(grab.GrappledBuff) != null,
                                ["establishingWeapon"] =
                                    SummonGrappleLinks.EstablishingWeapon(
                                        owner, target)?.Blueprint?.AssetGuid,
                                ["attackCostExact"] = paid,
                                ["heldRound"] = SummonHoldComponent
                                    .RoundsHeld(held),
                                ["costs"] = costs
                            };
                            holdAttempts.Add(attemptRow);
                            if (!established)
                            {
                                bool ownerRefusal = attacks.Length == 1 &&
                                    (bool?)attacks[0]["hit"] == true &&
                                    initialManeuvers.Length == 1 &&
                                    !SummonManeuverChecks.Succeeded(
                                        initialManeuvers[0]);
                                bool targetEscape = attacks.Length == 1 &&
                                    (bool?)attacks[0]["hit"] == true &&
                                    initialManeuvers.Length == 1 &&
                                    SummonManeuverChecks.Succeeded(
                                        initialManeuvers[0]) &&
                                    initialTargetEscape &&
                                    InfernalBarbedHoldClear35(owner, target,
                                        grab);
                                check((targetEscape ?
                                        "barbed-target-escape-after-grab-" :
                                        "barbed-grab-natural-refusal-") +
                                    (attempt + 1), ownerRefusal || targetEscape,
                                    (JObject)attemptRow.DeepClone(),
                                    targetEscape ?
                                        "A real target-owned native break-free event releases the newly established hold and permits only the next bounded ordinary claw attempt." :
                                        "A chance-dependent native grapple refusal is retained and permits only the next bounded ordinary claw attempt.");
                                continue;
                            }
                            if (!establishmentRecorded)
                            {
                                check("barbed-grab-establishes", true,
                                    (JObject)attemptRow.DeepClone(),
                                    "One real wounding primary claw and successful native grapple check establish reciprocal exact-limb hold state without initial impale damage.");
                                establishmentRecorded = true;
                            }

                            int maintainEventStart = trace.Rows.Count;
                            int maintainManeuverStart = maneuvers.Rules.Count;
                            int maintainEscapeSkillStart = escapes.Skills.Count;
                            int maintainEscapeCmbStart = escapes.Cmb.Count;
                            int maintainEscapeManeuverStart =
                                escapes.Maneuvers.Count;
                            bool cleanupProbe = attempt == 0;
                            int maintainCmdBefore =
                                target.Stats.AdditionalCMD.BaseValue;
                            if (cleanupProbe)
                                target.Stats.AdditionalCMD.BaseValue = 1000;
                            DateTime deadline = DateTime.UtcNow.AddSeconds(25);
                            JObject[] impales = Array.Empty<JObject>();
                            while (DateTime.UtcNow < deadline &&
                                ReferenceEquals(SummonHoldComponent.HeldTarget(
                                    owner), target) && impales.Length == 0)
                            {
                                Game.Instance.IsPaused = false;
                                yield return 0;
                                impales = InfernalBarbedImpaleRows35(trace,
                                    maintainEventStart, owner, target).ToArray();
                            }
                            if (cleanupProbe)
                                target.Stats.AdditionalCMD.BaseValue =
                                    maintainCmdBefore;
                            var maintainManeuvers = maneuvers.Rules.Skip(
                                maintainManeuverStart).ToArray();
                            var maintainEscapeSkills = escapes.Skills.Skip(
                                maintainEscapeSkillStart).ToArray();
                            var maintainEscapeCmb = escapes.Cmb.Skip(
                                maintainEscapeCmbStart).ToArray();
                            var maintainEscapeManeuvers = escapes.Maneuvers.Skip(
                                maintainEscapeManeuverStart).ToArray();
                            var replayedAttacks = InfernalBarbedRows35(trace,
                                maintainEventStart, "attack", owner, target)
                                .ToArray();
                            var replayedFearSaves = InfernalBarbedRows35(trace,
                                maintainEventStart, "save", owner, target)
                                .Where(value => (int?)value["dc"] == 20)
                                .ToArray();
                            bool maintained = impales.Length == 1 &&
                                !cleanupProbe &&
                                maintainManeuvers.Length == 1 &&
                                SummonManeuverChecks.Succeeded(
                                    maintainManeuvers[0]) &&
                                replayedAttacks.Length == 0 &&
                                replayedFearSaves.Length == 0 &&
                                ReferenceEquals(SummonHoldComponent.HeldTarget(
                                    owner), target) &&
                                ((JArray)impales[0]["packets"]).Count == 1 &&
                                (string)impales[0]["packets"]?[0]?["dice"] ==
                                    "3d8" &&
                                (int?)impales[0]["packets"]?[0]?["bonus"] ==
                                    InfernalRules35.ImpaleStrengthBonus(
                                        owner.Stats.Strength.Bonus);
                            attemptRow["maintainManeuvers"] =
                                InfernalBarbedManeuverRows35(
                                    maintainManeuvers);
                            attemptRow["maintainTargetEscapeSkills"] =
                                new JArray(maintainEscapeSkills.Select(
                                    OutsiderEscapeSkill));
                            attemptRow["maintainTargetEscapeCmb"] =
                                new JArray(maintainEscapeCmb.Select(value =>
                                    value.Result));
                            attemptRow["maintainTargetEscapeManeuvers"] =
                                InfernalBarbedManeuverRows35(
                                    maintainEscapeManeuvers);
                            attemptRow["maintainHighCmdProbe"] = cleanupProbe;
                            attemptRow["maintainProbeCmdBase"] = cleanupProbe ?
                                1000 : maintainCmdBefore;
                            attemptRow["maintainCmdRestored"] =
                                target.Stats.AdditionalCMD.BaseValue ==
                                    maintainCmdBefore;
                            attemptRow["impaleDamage"] = new JArray(impales);
                            attemptRow["replayedAttacks"] =
                                new JArray(replayedAttacks);
                            attemptRow["replayedFearSaves"] =
                                new JArray(replayedFearSaves);
                            attemptRow["heldAfterMaintain"] =
                                ReferenceEquals(SummonHoldComponent.HeldTarget(
                                    owner), target);
                            attemptRow["forcedRolls"] = 0;
                            if (maintained)
                            {
                                impaled = true;
                                check("barbed-impale-maintain", true,
                                    new JObject {
                                        ["owner"] = owner.UniqueId,
                                        ["target"] = target.UniqueId,
                                        ["attempts"] = holdAttempts,
                                        ["liveStrengthBonus"] = owner.Stats
                                            .Strength.Bonus,
                                        ["expectedImpaleBonus"] =
                                            InfernalRules35
                                                .ImpaleStrengthBonus(owner
                                                    .Stats.Strength.Bonus)
                                    }, "A later natural round supplies one successful grapple check and exactly one direct 3d8+live-Strength impale, with no replayed claw or default grapple-damage attack.");
                            }
                            else
                            {
                                int settleFrames = 0;
                                DateTime settleDeadline =
                                    DateTime.UtcNow.AddSeconds(5);
                                while (DateTime.UtcNow < settleDeadline &&
                                    settleFrames++ < 600 &&
                                    !InfernalBarbedHoldClear35(owner, target,
                                        grab))
                                {
                                    Game.Instance.IsPaused = false;
                                    yield return 0;
                                }
                                bool ownerRefusal = impales.Length == 0 &&
                                    maintainManeuvers.Length == 1 &&
                                    !SummonManeuverChecks.Succeeded(
                                        maintainManeuvers[0]) &&
                                    InfernalBarbedHoldClear35(owner, target,
                                        grab);
                                bool targetEscape = impales.Length == 0 &&
                                    maintainManeuvers.Length == 0 &&
                                    InfernalBarbedTargetEscaped35(
                                        maintainEscapeSkills,
                                        maintainEscapeCmb,
                                        maintainEscapeManeuvers) &&
                                    InfernalBarbedHoldClear35(owner, target,
                                        grab);
                                attemptRow["releaseSettleFrames"] =
                                    settleFrames;
                                attemptRow["releaseState"] =
                                    InfernalBarbedHoldState35(owner, target,
                                        grab);
                                attemptRow["releaseSettled"] =
                                    InfernalBarbedHoldClear35(owner, target,
                                        grab);
                                check((targetEscape ?
                                        "barbed-target-escape-before-maintain-" :
                                        cleanupProbe ?
                                        "barbed-maintain-high-cmd-refusal-" :
                                        "barbed-maintain-natural-refusal-") +
                                    (attempt + 1), cleanupProbe ?
                                        ownerRefusal :
                                        ownerRefusal || targetEscape,
                                    (JObject)attemptRow.DeepClone(),
                                    targetEscape ?
                                        "A real target-owned native break-free event releases the hold before maintain, without impale, and permits only the next bounded reacquisition." :
                                    cleanupProbe ?
                                        "A lawful temporary high-CMD fixture precondition retains the natural d20 and makes one real later grapple check fail on any non-20, then is restored; the exact Barbed hold releases without Impale and the next bounded ordinary claw may reacquire." :
                                        "A genuine failed later grapple check releases the hold without impale and permits only the next bounded reacquisition.");
                            }
                        }
                    }
                    if (!impaled) throw new InvalidOperationException(
                        "Barbed impale precondition remained unobserved after three bounded natural holds.");
                    }
                }
                finally
                {
                    EventBus.Unsubscribe(escapes);
                }

                InfernalRemoveBarbedFear35(target, owner, frightened);
                UnitEntityData rayOwner = sibling;
                UnitEntityData rayTarget = target;
                rayOwner.Commands.InterruptAll(true);
                rayOwner.Commands.RemoveFinishedAndUpdateQueue();
                rayTarget.Damage = 0;
                rayTarget.Stats.AC.BaseValue = -100;
                InfernalCombatLane34(fixture, rayOwner, rayTarget, 3);
                int rayStart = trace.Rows.Count;
                var rayCosts = new JArray();
                UnitUseAbility rayCommand = null;
                var rayData = new AbilityData(rayOwner.Descriptor.Abilities
                    .GetAbility(ray));
                var rayParams = rayData.CalculateParams();
                using (var economy = new FirearmReviewCosts(rayOwner,
                    rayCosts))
                {
                    foreach (int frame in Sprint26Cast(rayOwner, ray,
                        new TargetWrapper(rayTarget), value =>
                            rayCommand = value)) yield return frame;
                    DateTime deadline = DateTime.UtcNow.AddSeconds(12);
                    JObject[] rayRolls = Array.Empty<JObject>();
                    JObject[] rayDamage = Array.Empty<JObject>();
                    while (DateTime.UtcNow < deadline)
                    {
                        rayRolls = InfernalBarbedAbilityRows35(trace,
                            rayStart, "attack-roll", rayOwner, rayTarget, ray)
                            .ToArray();
                        rayDamage = InfernalBarbedAbilityRows35(trace,
                            rayStart, "damage", rayOwner, rayTarget, ray)
                            .ToArray();
                        int hits = rayRolls.Count(value =>
                            (bool?)value["hit"] == true);
                        if (rayRolls.Length >= 2 &&
                            rayDamage.Length >= hits) break;
                        Game.Instance.IsPaused = false;
                        yield return 0;
                    }
                    int hitCount = rayRolls.Count(value =>
                        (bool?)value["hit"] == true);
                    var rayFearSaves = InfernalBarbedRows35(trace,
                        rayStart, "save", rayOwner, rayTarget).Where(value =>
                            (int?)value["dc"] == 20).ToArray();
                    bool exact = rayCommand != null &&
                        rayCommand.IsFinished && rayCommand.Result ==
                            UnitCommand.ResultType.Success &&
                        rayCommand.ExecutionProcess != null &&
                        rayCommand.ExecutionProcess.IsEnded &&
                        rayRolls.Length == 2 && rayRolls.All(value =>
                            (string)value["attackType"] ==
                                Kingmaker.RuleSystem.AttackType.RangedTouch
                                    .ToString()) &&
                        rayDamage.Length == hitCount &&
                        rayDamage.All(value => (int?)value["actual"] > 0) &&
                        rayFearSaves.Length == 0 &&
                        delivery.Projectiles.Length == 2 &&
                        !delivery.UseMaxProjectilesCount &&
                        rayParams.CasterLevel ==
                            InfernalRules35.SpellLikeCasterLevel &&
                        rayParams.SpellLevel == 2 &&
                        ray.ActionType == UnitCommand.CommandType.Standard &&
                        economy.Count(rayCommand) == 1 &&
                        economy.Exact(rayCommand, false);
                    check("barbed-scorching-two-rays", exact,
                        new JObject {
                            ["owner"] = rayOwner.UniqueId,
                            ["target"] = rayTarget.UniqueId,
                            ["ability"] = ray.AssetGuid,
                            ["command"] = Sprint27AiCommand(rayCommand),
                            ["projectilesConfigured"] =
                                delivery.Projectiles.Length,
                            ["useMaxProjectilesCount"] =
                                delivery.UseMaxProjectilesCount,
                            ["casterLevel"] = rayParams.CasterLevel,
                            ["spellLevel"] = rayParams.SpellLevel,
                            ["rolls"] = new JArray(rayRolls),
                            ["hits"] = hitCount,
                            ["damage"] = new JArray(rayDamage),
                            ["clawFearSaves"] = new JArray(rayFearSaves),
                            ["impaleOwnerStillHolding"] =
                                ReferenceEquals(SummonHoldComponent
                                    .HeldTarget(owner), target),
                            ["costs"] = rayCosts,
                            ["forcedRolls"] = 0
                        }, "One real standard-action SLA command fires exactly two separately attributed native ranged-touch rays; each natural hit produces its own ability-attributed damage event.");
                }

                if (mindImmunity != null)
                {
                    target.Descriptor.RemoveFact(mindImmunity);
                    foreach (BlueprintComponent component in
                        mindImmunity.ComponentsArray)
                        UnityEngine.Object.DestroyImmediate(component);
                    UnityEngine.Object.DestroyImmediate(mindImmunity);
                    mindImmunity = null;
                }
                target.Stats.AC.BaseValue = oldAc;
                target.Stats.SaveWill.BaseValue = oldWill;
                target.Stats.HitPoints.BaseValue = oldHitPoints;
                target.Stats.AdditionalCMB.BaseValue = oldCmb;
                target.Stats.AdditionalCMD.BaseValue = oldCmd;
                target.Stats.SkillMobility.BaseValue = oldMobility;
                target.Stats.SkillAthletics.BaseValue = oldAthletics;
                target.Descriptor.State.Size = oldSize;
                target.Damage = oldDamage;
                InfernalRemoveBarbedFear35(target, owner, frightened);
                UnitEntityData[] cleanupActors = { owner, sibling };
                DisposeExpandedSummoningUnits(fixture.Created,
                    cleanupActors);
                int cleanupFrames = 0;
                DateTime cleanupDeadline = DateTime.UtcNow.AddSeconds(5);
                while (DateTime.UtcNow < cleanupDeadline &&
                    cleanupFrames++ < 600 && (cleanupActors.Any(actor =>
                        !actor.Destroyed) ||
                    Game.Instance.State.AreaEffects.All.Any(value =>
                        cleanupActors.Any(actor => ReferenceEquals(
                            value?.Context?.MaybeCaster, actor)))))
                    yield return 0;
                bool cleanup = cleanupActors.All(actor => actor.Destroyed &&
                        !actor.IsInState) &&
                    !Game.Instance.State.AreaEffects.All.Any(value =>
                        cleanupActors.Any(actor => ReferenceEquals(
                            value?.Context?.MaybeCaster, actor))) &&
                    InfernalBarbedTargetReleased35(owner, target, grab) &&
                    target.Stats.AC.BaseValue == oldAc &&
                    target.Stats.SaveWill.BaseValue == oldWill &&
                    target.Stats.HitPoints.BaseValue == oldHitPoints &&
                    target.Stats.AdditionalCMB.BaseValue == oldCmb &&
                    target.Stats.AdditionalCMD.BaseValue == oldCmd &&
                    target.Stats.SkillMobility.BaseValue == oldMobility &&
                    target.Stats.SkillAthletics.BaseValue == oldAthletics &&
                    target.Descriptor.State.Size == oldSize &&
                    target.Damage == oldDamage;
                check("barbed-combat-owned-cleanup", cleanup,
                    new JObject {
                        ["actors"] = new JArray(cleanupActors.Select(actor =>
                            new JObject {
                                ["unit"] = actor.UniqueId,
                                ["destroyed"] = actor.Destroyed,
                                ["inState"] = actor.IsInState
                            })),
                        ["ownedAreas"] = Game.Instance.State.AreaEffects.All
                            .Count(value => cleanupActors.Any(actor =>
                                ReferenceEquals(value?.Context?.MaybeCaster,
                                    actor))),
                        ["targetHoldReleased"] =
                            InfernalBarbedTargetReleased35(owner, target, grab),
                        ["disposedOwnerHoldState"] =
                            InfernalBarbedHoldState35(owner, target, grab),
                        ["hostileAcRestored"] = target.Stats.AC.BaseValue ==
                            oldAc,
                        ["hostileWillRestored"] = target.Stats.SaveWill
                            .BaseValue == oldWill,
                        ["hostileHpRestored"] = target.Stats.HitPoints
                            .BaseValue == oldHitPoints,
                        ["hostileCmbRestored"] = target.Stats.AdditionalCMB
                            .BaseValue == oldCmb,
                        ["hostileCmdRestored"] = target.Stats.AdditionalCMD
                            .BaseValue == oldCmd,
                        ["hostileMobilityRestored"] = target.Stats
                            .SkillMobility.BaseValue == oldMobility,
                        ["hostileAthleticsRestored"] = target.Stats
                            .SkillAthletics.BaseValue == oldAthletics,
                        ["hostileSizeRestored"] = target.Descriptor.State
                            .Size == oldSize,
                        ["hostileDamageRestored"] = target.Damage ==
                            oldDamage,
                        ["cleanupFrames"] = cleanupFrames
                    }, "The grouped Barbed transaction destroys every owned actor, removes every live target-side hold and area, and restores every fixture-only modifier; inaccessible native part objects on an already destroyed actor are recorded but are not treated as live gameplay state.");
            }
            finally
            {
                trace.Dispose();
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(
                    _request.EvidenceDirectory,
                    "sprint35-infernal-barbed-combat.json"),
                    receipt.ToString(Formatting.Indented));
                if (target != null && !target.Destroyed)
                {
                    if (maneuverImmunityAdded)
                        target.Descriptor.State.RemoveCondition(
                            UnitCondition.ImmuneToCombatManeuvers);
                    if (mindImmunity != null)
                    {
                        target.Descriptor.RemoveFact(mindImmunity);
                        foreach (BlueprintComponent component in
                            mindImmunity.ComponentsArray)
                            UnityEngine.Object.DestroyImmediate(component);
                        UnityEngine.Object.DestroyImmediate(mindImmunity);
                    }
                    InfernalRemoveBarbedFear35(target, owner, frightened);
                    target.Stats.AC.BaseValue = oldAc;
                    target.Stats.SaveWill.BaseValue = oldWill;
                    target.Stats.HitPoints.BaseValue = oldHitPoints;
                    target.Stats.AdditionalCMB.BaseValue = oldCmb;
                    target.Stats.AdditionalCMD.BaseValue = oldCmd;
                    target.Stats.SkillMobility.BaseValue = oldMobility;
                    target.Stats.SkillAthletics.BaseValue = oldAthletics;
                    target.Descriptor.State.Size = oldSize;
                    target.Damage = oldDamage;
                }
                foreach (UnitEntityData actor in owned.Where(value =>
                    value != null && !value.Destroyed))
                {
                    Sprint28SetBrainActivePreservingActions(actor, false);
                    actor.Commands.InterruptAll(true);
                    actor.Commands.RemoveFinishedAndUpdateQueue();
                    var actorGrab = SummonGrabComponent.Find(actor);
                    UnitEntityData held = SummonHoldComponent.HeldTarget(actor);
                    if (actorGrab != null && held != null)
                        SummonHoldComponent.ReleaseLink(actor, held,
                            actorGrab, false);
                    if (actor.IsInCombat) actor.LeaveCombat();
                }
                if (target != null && !target.Destroyed && target.IsInCombat)
                    target.LeaveCombat();
                Game.Instance.Player.UpdateIsInCombat();
                DisposeExpandedSummoningUnits(fixture.Created,
                    owned.Where(value => value != null && !value.Destroyed)
                        .ToArray());
                ResetExpandedSummoningHostile(fixture);
            }
        }

        private IEnumerable<int> InfernalBarbedGrabNegative35(
            ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, UnitEntityData target,
            InfernalEvents34 trace, InfernalBarbedManeuvers35 maneuvers,
            SummonGrabComponent grab, BlueprintBuff frightened, Size size,
            bool immune,
            string label, Action<string, bool, JObject, string> check)
        {
            target.Descriptor.State.Size = size;
            if (immune)
                target.Descriptor.State.AddCondition(
                    UnitCondition.ImmuneToCombatManeuvers, null);
            var attempts = new JArray();
            bool observedWound = false;
            try
            {
                for (int attempt = 0; attempt < 4 && !observedWound;
                    attempt++)
                {
                    target.Damage = 0;
                    owner.Commands.InterruptAll(true);
                    owner.Commands.RemoveFinishedAndUpdateQueue();
                    InfernalCombatLane34(fixture, owner, target, 3);
                    int eventStart = trace.Rows.Count;
                    int maneuverStart = maneuvers.Rules.Count;
                    var command = new UnitAttack(target) {
                        IsSingleAttack = true, ForceFullAttack = false };
                    owner.Commands.Run(command);
                    foreach (int frame in WaitInfernalCommand34(command,
                        () => true, 25)) yield return frame;
                    var attacks = InfernalBarbedRows35(trace, eventStart,
                        "attack", owner, target).ToArray();
                    var checks = maneuvers.Rules.Skip(maneuverStart)
                        .ToArray();
                    bool wound = attacks.Length == 1 &&
                        (bool?)attacks[0]["hit"] == true &&
                        (int?)attacks[0]["hpDamage"] > 0;
                    attempts.Add(new JObject {
                        ["attempt"] = attempt + 1,
                        ["command"] = Sprint27AiCommand(command),
                        ["attacks"] = new JArray(attacks),
                        ["maneuvers"] =
                            InfernalBarbedManeuverRows35(checks),
                        ["held"] = SummonHoldComponent.HeldTarget(owner)?
                            .UniqueId
                    });
                    if (!wound) continue;
                    observedWound = true;
                    bool exactChecks = immune ? checks.Length == 1 &&
                        !SummonManeuverChecks.Succeeded(checks[0]) :
                        checks.Length == 0;
                    bool fearImmune = !target.Buffs.Enumerable.Any(value =>
                        ReferenceEquals(value.Blueprint, frightened) &&
                        ReferenceEquals(value.Context?.MaybeCaster, owner));
                    bool exact = command.Result ==
                            UnitCommand.ResultType.Success && exactChecks &&
                        fearImmune &&
                        SummonHoldComponent.HeldTarget(owner) == null &&
                        SummonHoldComponent.HeldState(owner, target,
                            grab) == null;
                    check("barbed-grab-" + label, exact, new JObject {
                        ["owner"] = owner.UniqueId,
                        ["target"] = target.UniqueId,
                        ["targetSize"] = size.ToString(),
                        ["maneuverImmune"] = immune,
                        ["mindAffectingImmunityBlockedFear"] = fearImmune,
                        ["attempts"] = attempts,
                        ["forcedRolls"] = 0
                    }, immune ?
                        "A real wounding claw reaches the native maneuver-immunity consumer, records a failed grapple check, and creates no hold." :
                        "A real wounding claw against a Large foe never attempts grab because the Medium Barbed Devil's explicit cap rejects it before a maneuver roll.");
                }
                if (!observedWound) throw new InvalidOperationException(
                    "Barbed " + label +
                    " wounding-claw precondition was not observed in four bounded attempts.");
            }
            finally
            {
                if (immune)
                    target.Descriptor.State.RemoveCondition(
                        UnitCondition.ImmuneToCombatManeuvers);
            }
        }

        private static IEnumerable<JObject> InfernalBarbedRows35(
            InfernalEvents34 trace, int start, string kind,
            UnitEntityData owner, UnitEntityData target)
        {
            return trace.Rows.Skip(start).OfType<JObject>().Where(value =>
                (string)value["event"] == kind &&
                (string)value["owner"] == owner.UniqueId &&
                (string)value["target"] == target.UniqueId);
        }

        private static IEnumerable<JObject> InfernalBarbedAbilityRows35(
            InfernalEvents34 trace, int start, string kind,
            UnitEntityData owner, UnitEntityData target,
            BlueprintAbility ability)
        {
            return InfernalBarbedRows35(trace, start, kind, owner, target)
                .Where(value => (string)value["ability"] ==
                        ability.AssetGuid ||
                    (string)value["contextAbility"] == ability.AssetGuid);
        }

        private static IEnumerable<JObject> InfernalBarbedImpaleRows35(
            InfernalEvents34 trace, int start, UnitEntityData owner,
            UnitEntityData target)
        {
            return InfernalBarbedRows35(trace, start, "damage", owner,
                target).Where(value =>
                    (bool?)value["hasAttackRoll"] == false &&
                    value["packets"] is JArray &&
                    ((JArray)value["packets"]).OfType<JObject>().Any(packet =>
                        (string)packet["dice"] == "3d8"));
        }

        private static void InfernalRemoveBarbedFear35(
            UnitEntityData target, UnitEntityData owner,
            BlueprintBuff frightened)
        {
            if (target == null || frightened == null) return;
            foreach (Buff buff in target.Buffs.Enumerable.Where(value =>
                ReferenceEquals(value.Blueprint, frightened) &&
                (owner == null || ReferenceEquals(value.Context?.MaybeCaster,
                    owner))).ToArray()) buff.Remove();
        }

        private static bool InfernalBarbedHoldClear35(UnitEntityData owner,
            UnitEntityData target, SummonGrabComponent grab)
        {
            return owner != null && target != null && grab != null &&
                SummonHoldComponent.HeldTarget(owner) == null &&
                owner.Get<UnitPartGrappleInitiator>() == null &&
                target.Get<UnitPartGrappleTarget>() == null &&
                SummonGrappleLinks.EstablishingWeapon(owner, target) == null &&
                owner.Buffs.GetBuff(grab.HoldBuff) == null &&
                target.Buffs.GetBuff(grab.GrappledBuff) == null;
        }

        private static bool InfernalBarbedTargetReleased35(
            UnitEntityData owner, UnitEntityData target,
            SummonGrabComponent grab)
        {
            if (owner == null || target == null || grab == null) return false;
            UnitPartGrappleTarget held = target.Get<UnitPartGrappleTarget>();
            bool exactPart = held != null &&
                ReferenceEquals(held.Initiator.Value, owner);
            bool exactBuff = grab.GrappledBuff != null &&
                target.Buffs.Enumerable.Any(value =>
                    ReferenceEquals(value.Blueprint, grab.GrappledBuff) &&
                    ReferenceEquals(value.Context?.MaybeCaster, owner));
            return !exactPart && !exactBuff &&
                SummonHoldComponent.HeldTarget(owner) == null;
        }

        private static JObject InfernalBarbedHoldState35(UnitEntityData owner,
            UnitEntityData target, SummonGrabComponent grab)
        {
            UnitPartGrappleInitiator initiator = owner == null ? null :
                owner.Get<UnitPartGrappleInitiator>();
            UnitPartGrappleTarget held = target == null ? null :
                target.Get<UnitPartGrappleTarget>();
            return new JObject {
                ["heldTarget"] = owner == null ? null :
                    SummonHoldComponent.HeldTarget(owner)?.UniqueId,
                ["ownerInitiator"] = initiator != null,
                ["ownerInitiatorTarget"] = initiator == null ? null :
                    initiator.Target.Value?.UniqueId,
                ["targetPart"] = held != null,
                ["targetPartOwner"] = held == null ? null :
                    held.Initiator.Value?.UniqueId,
                ["establishingWeapon"] = owner == null || target == null ?
                    null : SummonGrappleLinks.EstablishingWeapon(owner,
                        target)?.Blueprint?.AssetGuid,
                ["holdBuff"] = owner != null && grab?.HoldBuff != null &&
                    owner.Buffs.GetBuff(grab.HoldBuff) != null,
                ["grappledBuff"] = target != null &&
                    grab?.GrappledBuff != null &&
                    target.Buffs.GetBuff(grab.GrappledBuff) != null,
                ["clear"] = InfernalBarbedHoldClear35(owner, target, grab)
            };
        }

        private static JArray InfernalBarbedManeuverRows35(
            IEnumerable<RuleCombatManeuver> rules)
        {
            return new JArray(rules.Select(value => new JObject {
                ["type"] = value.Type.ToString(),
                ["initiator"] = value.Initiator?.UniqueId,
                ["target"] = value.Target?.UniqueId,
                ["roll"] = (int)value.InitiatorRoll,
                ["cmb"] = value.InitiatorCMB,
                ["cmd"] = value.TargetCMD,
                ["autoFailure"] = value.AutoFailure,
                ["nativeSuccess"] = value.Success,
                ["productSuccess"] =
                    SummonManeuverChecks.Succeeded(value)
            }));
        }

        private static bool InfernalBarbedTargetEscaped35(
            IEnumerable<RuleSkillCheck> skills,
            IEnumerable<RuleCalculateCMB> cmb,
            IEnumerable<RuleCombatManeuver> maneuvers)
        {
            RuleSkillCheck[] skillRows = skills.ToArray();
            RuleCalculateCMB[] cmbRows = cmb.ToArray();
            RuleCombatManeuver[] maneuverRows = maneuvers.ToArray();
            bool skillPath = skillRows.Length == 1 &&
                cmbRows.Length == 0 && maneuverRows.Length == 0 &&
                skillRows[0].StatType ==
                    Kingmaker.EntitySystem.Stats.StatType.SkillMobility &&
                skillRows[0].IsPassed;
            bool cmbPath = skillRows.Length == 0 && cmbRows.Length > 0 &&
                maneuverRows.Length == 1 && maneuverRows[0].Success;
            return skillPath || cmbPath;
        }

        private sealed class InfernalBarbedManeuvers35 : IDisposable,
            IGlobalRulebookHandler<RuleCombatManeuver>
        {
            private readonly UnitEntityData _owner;
            private readonly UnitEntityData _target;
            internal readonly List<RuleCombatManeuver> Rules =
                new List<RuleCombatManeuver>();

            internal InfernalBarbedManeuvers35(UnitEntityData owner,
                UnitEntityData target)
            {
                _owner = owner;
                _target = target;
                EventBus.Subscribe(this);
            }

            public void Dispose() { EventBus.Unsubscribe(this); }
            public void OnEventAboutToTrigger(RuleCombatManeuver evt) { }
            public void OnEventDidTrigger(RuleCombatManeuver evt)
            {
                if (evt != null && evt.Type == CombatManeuver.Grapple &&
                    ReferenceEquals(evt.Initiator, _owner) &&
                    ReferenceEquals(evt.Target, _target)) Rules.Add(evt);
            }
        }
    }
}
