using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics.Actions;
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
        /// The grouped Barbed phase starts with the newly implemented bounded
        /// alignment burst. It issues one real command against an isolated
        /// all-creature footprint and observes production SR, saves, dice,
        /// damage, resource debit, condition lifetime, and exact ownership.
        /// It never supplies a result or changes spell victim eligibility.
        /// </summary>
        private IEnumerable<int> ReviewInfernalBarbed35(
            ExpandedSummoningCorrectionFixture fixture)
        {
            var owned = new List<UnitEntityData>();
            var cells = new JArray();
            var receipt = new JObject {
                ["phase"] = ExpandedSummoningNativeProfilePolicy
                    .Sprint35InfernalBarbedPhase,
                ["finalQualification"] = false,
                ["cells"] = cells
            };
            UnitEntityData owner = null, astral = null, wolf = null,
                dretch = null;
            UnitEntityData target = fixture.Hostile;
            Alignment targetAlignment = Alignment.TrueNeutral;
            int targetWill = 0, targetHitPoints = 0, targetDamage = 0;
            bool targetChanged = false;
            BlueprintBuff sickened = null;
            Action<string, bool, JObject, string> check =
                (id, pass, row, description) => {
                    row["cell"] = id;
                    cells.Add(row);
                    Sprint25Check("sprint35-" + id, pass, row, description);
                    if (!pass) throw new InvalidOperationException(
                        "Sprint35 Barbed native cell failed: " + id);
                };
            try
            {
                owner = CastExpandedSummoningOwnTier(fixture,
                    "barbed-devil");
                astral = CastExpandedSummoningOwnTier(fixture,
                    "astral-deva");
                wolf = CastExpandedSummoningOwnTier(fixture, "wolf");
                dretch = CastExpandedSummoningOwnTier(fixture, "dretch");
                owned.AddRange(new[] { owner, astral, wolf, dretch });
                foreach (UnitEntityData actor in owned)
                {
                    Sprint28SetBrainActivePreservingActions(actor, false);
                    actor.Commands.InterruptAll(true);
                    actor.Commands.RemoveFinishedAndUpdateQueue();
                    foreach (int frame in Outsider30NativeAppearance(actor,
                        actor.Blueprint.name)) yield return frame;
                }

                string prefix = ExpandedSummoningSpecialBuilder.Barbed35Prefix;
                var unholy = (BlueprintAbility)ExpandedSummoningBlueprints
                    .Registered.BySymbol[prefix + "UnholyBlight"];
                var resource = (BlueprintAbilityResource)
                    ExpandedSummoningBlueprints.Registered.BySymbol[
                        prefix + "UnholyBlightResource"];
                var granted = owner.Descriptor.Abilities.GetAbility(unholy);
                var data = granted == null ? null : new AbilityData(granted);
                var calculated = data?.CalculateParams();
                var around = unholy.GetComponent<AbilityTargetsAround>();
                var run = unholy.GetComponent<AbilityEffectRunAction>();
                var action = run?.Actions?.Actions?.OfType<
                    ContextActionInfernalUnholyBlight35>().SingleOrDefault();
                var cost = unholy.GetComponent<AbilityResourceLogic>();
                var parameters = unholy.GetComponent<
                    ContextCalculateAbilityParams>();
                var descriptor = unholy.GetComponent<
                    SpellDescriptorComponent>();
                var school = unholy.GetComponent<SpellComponent>();
                sickened = action?.Sickened;
                var ai = owner.Blueprint.Brain.Actions.OfType<
                    InfernalSpellAi35>().SingleOrDefault(value =>
                        ReferenceEquals(value.Ability, unholy));
                int resourceBefore = owner.Descriptor.Resources
                    .GetResourceAmount(resource);
                int sharedCl = AbilityParams.Empty.CasterLevel;
                int sharedSpellLevel = AbilityParams.Empty.SpellLevel;
                int sharedDc = AbilityParams.Empty.DC;
                bool mapping = data != null && calculated != null &&
                    data.IsAvailable && unholy.Type == AbilityType.SpellLike &&
                    unholy.Range == AbilityRange.Medium &&
                    unholy.CanTargetPoint && unholy.CanTargetFriends &&
                    unholy.CanTargetEnemies && !unholy.CanTargetSelf &&
                    unholy.SpellResistance &&
                    unholy.ActionType == UnitCommand.CommandType.Standard &&
                    around != null && around.Targets ==
                        Kingmaker.UnitLogic.Abilities.Components.TargetType.Any &&
                    Math.Abs(around.AoERadius.Value -
                        InfernalRules35.UnholyBlightRadiusFeet) < 0.001f &&
                    run != null && run.SavingThrowType ==
                        SavingThrowType.Unknown && action != null &&
                    ReferenceEquals(action.SourceUnit, owner.Blueprint) &&
                    action.OutsiderType != null &&
                    action.OutsiderType.AssetGuid ==
                        "9054d3988d491d944ac144e27b6bc318" &&
                    sickened != null && sickened.AssetGuid ==
                        "4e42460798665fd4cb9173ffa7ada323" &&
                    cost != null && ReferenceEquals(cost.RequiredResource,
                        resource) && cost.IsSpendResource && cost.Amount == 1 &&
                    !cost.CostIsCustom && parameters != null &&
                    parameters.StatType == Kingmaker.EntitySystem.Stats
                        .StatType.Charisma &&
                    descriptor != null && (descriptor.Descriptor &
                        SpellDescriptor.Evil) != 0 && school != null &&
                    school.School == SpellSchool.Evocation &&
                    calculated.CasterLevel ==
                        InfernalRules35.SpellLikeCasterLevel &&
                    calculated.SpellLevel ==
                        InfernalRules35.UnholyBlightSpellLevel &&
                    calculated.DC == 18 && resourceBefore == 1 &&
                    ai != null && ai.TargetEnemy && !ai.TargetSelf &&
                    ai.AvoidFriendlyArea && ai.AreaRadiusFeet ==
                        InfernalRules35.UnholyBlightRadiusFeet;
                check("barbed-unholy-mapping", mapping, new JObject {
                    ["owner"] = owner.UniqueId,
                    ["ability"] = unholy.AssetGuid,
                    ["resource"] = resource.AssetGuid,
                    ["resourceBefore"] = resourceBefore,
                    ["type"] = unholy.Type.ToString(),
                    ["range"] = unholy.Range.ToString(),
                    ["targets"] = around?.Targets.ToString(),
                    ["radiusFeet"] = around?.AoERadius.Value,
                    ["spellResistance"] = unholy.SpellResistance,
                    ["cl"] = calculated?.CasterLevel,
                    ["spellLevel"] = calculated?.SpellLevel,
                    ["dc"] = calculated?.DC,
                    ["condition"] = sickened?.AssetGuid,
                    ["ai"] = ai == null ? null : new JObject {
                        ["baseScore"] = ai.BaseScore,
                        ["targetEnemy"] = ai.TargetEnemy,
                        ["avoidFriendlyArea"] = ai.AvoidFriendlyArea,
                        ["radiusFeet"] = ai.AreaRadiusFeet }
                }, "The Barbed Devil owns one CL12/DC18 all-creature Unholy Blight with native per-target SR, one exact resource use, the native Sickened condition, and admission-only ally-safe AI.");

                Vector3 far; string lane, direction;
                Vector3 origin = Sprint24AiCombatLane(fixture.Caster, owner,
                    target, 4f, out far, out lane, out direction,
                    new Feet(InfernalRules35.UnholyBlightRadiusFeet).Meters +
                        2f, new[] { astral, wolf, dretch }, 30);
                PlaceExpandedSummoningUnit(owner, origin);
                PlaceExpandedSummoningUnit(target, far);
                PlaceExpandedSummoningUnit(astral,
                    far + Vector3.right * 1.5f);
                PlaceExpandedSummoningUnit(wolf,
                    far - Vector3.right * 1.5f);
                PlaceExpandedSummoningUnit(dretch,
                    far + Vector3.forward * 1.5f);
                foreach (UnitEntityData actor in owned.Concat(
                    new[] { target }))
                {
                    actor.Wake(0f);
                    if (!Game.Instance.State.AwakeUnits.Contains(actor))
                        Game.Instance.State.AwakeUnits.Add(actor);
                    actor.SightCache.ClearIfNeeded();
                }

                targetAlignment = target.Descriptor.Alignment.Value;
                targetWill = target.Stats.SaveWill.BaseValue;
                targetHitPoints = target.Stats.HitPoints.BaseValue;
                targetDamage = target.Damage;
                targetChanged = true;
                target.Descriptor.Alignment.Set(Alignment.LawfulGood);
                astral.Descriptor.Alignment.Set(Alignment.LawfulGood);
                wolf.Descriptor.Alignment.Set(Alignment.TrueNeutral);
                dretch.Descriptor.Alignment.Set(Alignment.ChaoticEvil);
                target.Stats.SaveWill.BaseValue = -100;
                astral.Stats.SaveWill.BaseValue = -100;
                wolf.Stats.SaveWill.BaseValue = 100;
                dretch.Stats.SaveWill.BaseValue = -100;
                foreach (UnitEntityData actor in new[] {
                    target, astral, wolf, dretch })
                {
                    actor.Stats.HitPoints.BaseValue = 100000;
                    actor.Damage = 0;
                }

                float radius = new Feet(
                    InfernalRules35.UnholyBlightRadiusFeet).Meters;
                bool protectedClear = Game.Instance.Player.Party.Where(value =>
                    value != null && value.IsInGame).All(value =>
                        Vector3.Distance(value.Position, far) > radius + 1f +
                            value.Corpulence);
                UnitEntityData[] footprint = new[] {
                    owner, astral, wolf, dretch, target };
                bool fixtureExact = protectedClear && owner.IsEnemy(target) &&
                    owner.IsAlly(astral) && owner.IsAlly(wolf) &&
                    owner.IsAlly(dretch) &&
                    data.CanTarget(new TargetWrapper(far)) &&
                    footprint.All(value => Vector3.Distance(value.Position,
                        far) <= radius + value.Corpulence) &&
                    !target.Descriptor.HasFact(action.OutsiderType) &&
                    astral.Descriptor.HasFact(action.OutsiderType);
                check("barbed-unholy-owned-fixture", fixtureExact,
                    new JObject {
                        ["owner"] = owner.UniqueId,
                        ["goodOrdinaryEnemy"] = target.UniqueId,
                        ["goodSrOutsiderAlly"] = astral.UniqueId,
                        ["neutralAlly"] = wolf.UniqueId,
                        ["evilAlly"] = dretch.UniqueId,
                        ["pointTargetLegal"] = data.CanTarget(
                            new TargetWrapper(far)),
                        ["protectedPartyOutsideTwentyFeet"] = protectedClear,
                        ["lane"] = lane,
                        ["direction"] = direction,
                        ["controlledDirectCastNotAi"] = true
                    },
                    "Only request-owned summons and the exact private hostile occupy the real all-creature burst; protected intake actors remain outside.");

                owner.Memory.Add(target);
                target.Memory.Add(owner);
                owner.JoinCombat();
                target.JoinCombat();
                Game.Instance.Player.UpdateIsInCombat();
                for (int frame = 0; frame < 4; frame++) yield return 0;

                var observed = new Astral31SmiteEvents(owner, unholy,
                    sickened, action.OutsiderType, footprint);
                EventBus.Subscribe(observed);
                try
                {
                    UnitUseAbility command = null;
                    var costs = new JArray();
                    using (var economy = new FirearmReviewCosts(owner, costs))
                    {
                        foreach (int frame in Sprint26Cast(owner, unholy,
                            new TargetWrapper(far), value => command = value))
                            yield return frame;
                        int resourceAfter = owner.Descriptor.Resources
                            .GetResourceAmount(resource);
                        bool commandExact = command != null &&
                            command.IsFinished && command.Result ==
                                UnitCommand.ResultType.Success &&
                            command.ExecutionProcess != null &&
                            command.ExecutionProcess.IsEnded &&
                            economy.Count(command) == 1 &&
                            economy.Exact(command, false) &&
                            command.AiAction == null && !command.Cutscene &&
                            !command.IsIgnoreCooldown && resourceBefore == 1 &&
                            resourceAfter == 0 && !data.IsAvailable;
                        check("barbed-unholy-command", commandExact,
                            new JObject {
                                ["owner"] = owner.UniqueId,
                                ["command"] = command == null ? null :
                                    Sprint27AiCommand(command),
                                ["costs"] = costs,
                                ["resourceBefore"] = resourceBefore,
                                ["resourceAfter"] = resourceAfter,
                                ["availableAfter"] = data.IsAvailable,
                                ["directNativeCastCommands"] = 1,
                                ["aiCommands"] = 0
                            },
                            "One real manual Unholy Blight pays one standard action, reaches terminal success, and spends exactly its one per-summoning use.");
                    }

                    for (int frame = 0; frame < 16; frame++) yield return 0;
                    RuleSavingThrow targetSave = observed.Save(target);
                    RuleRollDice targetRoll = Infernal35DamageRoll(observed,
                        target);
                    RuleRollDice targetDuration = Infernal35DurationRoll(
                        observed, target);
                    RuleDealDamage targetHit = observed.Damage(target);
                    Buff targetSick = Infernal35Sickened(target, owner,
                        sickened);
                    int targetExpected = targetRoll == null ? -1 :
                        (int)targetRoll.Result;
                    bool targetExact = targetSave != null &&
                        !targetSave.IsPassed &&
                        targetSave.DifficultyClass == 18 &&
                        targetRoll != null &&
                        targetRoll.DiceFormula.Rolls == 5 &&
                        targetRoll.DiceFormula.Dice == DiceType.D8 &&
                        targetDuration != null &&
                        targetDuration.DiceFormula.Rolls == 1 &&
                        targetDuration.DiceFormula.Dice == DiceType.D4 &&
                        targetHit != null &&
                        targetHit.DamageWithoutReduction == targetExpected &&
                        target.Damage == targetHit.Damage &&
                        targetSick != null &&
                        target.Descriptor.State.HasCondition(
                            UnitCondition.Sickened) &&
                        targetSick.TimeLeft.TotalSeconds > 0d &&
                        targetSick.TimeLeft.TotalSeconds <= 24.1d;

                    RuleSavingThrow wolfSave = observed.Save(wolf);
                    RuleRollDice wolfRoll = Infernal35DamageRoll(observed,
                        wolf);
                    RuleDealDamage wolfHit = observed.Damage(wolf);
                    int wolfRolled = wolfRoll == null ? -1 :
                        (int)wolfRoll.Result;
                    int wolfExpected = wolfRolled < 0 ? -1 : wolfRolled / 4;
                    bool wolfExact = wolfSave != null && wolfSave.IsPassed &&
                        wolfSave.DifficultyClass == 18 && wolfRoll != null &&
                        wolfRoll.DiceFormula.Rolls == 5 &&
                        wolfRoll.DiceFormula.Dice == DiceType.D8 &&
                        wolfHit != null &&
                        wolfHit.DamageWithoutReduction == wolfExpected &&
                        wolf.Damage == wolfHit.Damage &&
                        Infernal35Sickened(wolf, owner, sickened) == null &&
                        !wolf.Descriptor.State.HasCondition(
                            UnitCondition.Sickened);

                    RuleSpellResistanceCheck astralResistance =
                        observed.Resistance(astral);
                    RuleSavingThrow astralSave = observed.Save(astral);
                    RuleRollDice astralRoll = Infernal35DamageRoll(observed,
                        astral);
                    RuleRollDice astralDuration = Infernal35DurationRoll(
                        observed, astral);
                    RuleDealDamage astralHit = observed.Damage(astral);
                    Buff astralSick = Infernal35Sickened(astral, owner,
                        sickened);
                    bool astralResolved = astralResistance != null &&
                        astralResistance.HasSpellResistance &&
                        astralResistance.HasResistanceRoll &&
                        astralResistance.SpellResistance == 25 &&
                        astralResistance.Context.Params.CasterLevel == 12;
                    bool astralCoherent = astralResolved &&
                        (astralResistance.IsSpellResisted ?
                            astralSave == null && astralRoll == null &&
                                astralHit == null && astralSick == null &&
                                astral.Damage == 0 :
                            astralSave != null && !astralSave.IsPassed &&
                                astralSave.DifficultyClass == 18 &&
                                astralRoll != null &&
                                astralRoll.DiceFormula.Rolls == 10 &&
                                astralRoll.DiceFormula.Dice == DiceType.D6 &&
                                astralDuration != null &&
                                astralDuration.DiceFormula.Rolls == 1 &&
                                astralDuration.DiceFormula.Dice ==
                                    DiceType.D4 &&
                                astralHit != null &&
                                astralHit.DamageWithoutReduction ==
                                    (int)astralRoll.Result &&
                                astral.Damage == astralHit.Damage &&
                                astralSick != null);

                    bool evilUnaffected = new[] { owner, dretch }.All(value =>
                        observed.Save(value) == null &&
                        observed.Roll(value) == null &&
                        observed.Damage(value) == null && value.Damage == 0 &&
                        Infernal35Sickened(value, owner, sickened) == null);
                    Buff[] victimConditions = new[] {
                        targetSick, astralSick }.Where(value => value != null)
                        .Distinct().ToArray();
                    bool victimParameters = victimConditions.Length > 0 &&
                        victimConditions.All(value => value.Context != null &&
                            value.Context.ParentContext != null &&
                            value.Context.ParentContext.ParentContext == null &&
                            ReferenceEquals(value.Context.AssociatedBlueprint,
                                sickened) &&
                            ReferenceEquals(value.Context.ParentContext
                                .AssociatedBlueprint, sickened) &&
                            ReferenceEquals(value.Context.Params,
                                value.Context.ParentContext.Params) &&
                            value.Context.Params.CasterLevel == 12 &&
                            value.Context.Params.SpellLevel == 4 &&
                            value.Context.Params.DC == 18 && value.IsFromSpell &&
                            !value.IsNotDispelable &&
                            !ReferenceEquals(value.Context.Params,
                                AbilityParams.Empty)) &&
                        victimConditions.Select(value => value.Context.Params)
                            .Distinct().Count() == victimConditions.Length;
                    bool effect = targetExact && wolfExact && astralCoherent &&
                        evilUnaffected && victimParameters &&
                        AbilityParams.Empty.CasterLevel == sharedCl &&
                        AbilityParams.Empty.SpellLevel == sharedSpellLevel &&
                        AbilityParams.Empty.DC == sharedDc;
                    check("barbed-unholy-alignment-sr-effect", effect,
                        new JObject {
                            ["owner"] = owner.UniqueId,
                            ["goodOrdinaryEnemy"] = Infernal35BurstRow(
                                observed, target),
                            ["goodSrOutsiderAlly"] = Infernal35BurstRow(
                                observed, astral),
                            ["neutralAlly"] = Infernal35BurstRow(observed,
                                wolf),
                            ["evilAlly"] = Infernal35BurstRow(observed,
                                dretch),
                            ["evilOwner"] = Infernal35BurstRow(observed,
                                owner),
                            ["targetExpectedFull"] = targetExpected,
                            ["wolfExpectedQuarter"] = wolfExpected,
                            ["astralSrOutcome"] = astralResistance == null ?
                                null : (astralResistance.IsSpellResisted ?
                                    "resisted" : "penetrated"),
                            ["victimParametersIndependent"] =
                                victimParameters,
                            ["sharedEmptyUnchanged"] =
                                AbilityParams.Empty.CasterLevel == sharedCl &&
                                AbilityParams.Empty.SpellLevel ==
                                    sharedSpellLevel &&
                                AbilityParams.Empty.DC == sharedDc,
                            ["victimContexts"] = new JArray(
                                victimConditions.Select(value => new JObject {
                                    ["recipient"] = value.Owner?.Unit?.UniqueId,
                                    ["owner"] = value.Context.MaybeCaster?
                                        .UniqueId,
                                    ["associatedBlueprint"] = value.Context
                                        .AssociatedBlueprint?.AssetGuid,
                                    ["privateVictimRoot"] = value.Context
                                        .ParentContext?.AssociatedBlueprint?
                                        .AssetGuid,
                                    ["cl"] = value.Context.Params.CasterLevel,
                                    ["spellLevel"] = value.Context.Params
                                        .SpellLevel,
                                    ["dc"] = value.Context.Params.DC,
                                    ["fromSpell"] = value.IsFromSpell,
                                    ["notDispellable"] =
                                        value.IsNotDispelable })),
                            ["forcedRolls"] = 0
                        },
                        "One actual all-creature burst gives a failed-save good creature 5d8 plus native Sickened, quarters a successful neutral ally, honors the good outsider ally's SR before 10d6, and leaves evil owner/ally untouched.");

                    double targetBeforeSourceLoss =
                        targetSick.TimeLeft.TotalSeconds;
                    int resourceAtSourceLoss = owner.Descriptor.Resources
                        .GetResourceAmount(resource);
                    DisposeExpandedSummoningUnits(fixture.Created,
                        new[] { owner });
                    bool conditionRemained = !targetSick.IsDisposed &&
                        target.Buffs.Enumerable.Contains(targetSick) &&
                        target.Descriptor.State.HasCondition(
                            UnitCondition.Sickened) &&
                        targetSick.TimeLeft.TotalSeconds > 0d &&
                        targetSick.TimeLeft.TotalSeconds <=
                            targetBeforeSourceLoss;
                    DateTime deadline = DateTime.UtcNow.AddSeconds(30);
                    int expiryFrames = 0;
                    while (DateTime.UtcNow < deadline &&
                        expiryFrames++ < 3000 && victimConditions.Any(value =>
                            !value.IsDisposed)) yield return 0;
                    bool conditionExpired = victimConditions.All(value =>
                            value.IsDisposed) &&
                        new[] { target, astral }.All(value =>
                            !value.Buffs.Enumerable.Any(buff =>
                                ReferenceEquals(buff.Blueprint, sickened))) &&
                        !target.Descriptor.State.HasCondition(
                            UnitCondition.Sickened);
                    bool lifetime = owner.Destroyed &&
                        resourceAtSourceLoss == 0 && conditionRemained &&
                        conditionExpired &&
                        AbilityParams.Empty.CasterLevel == sharedCl &&
                        AbilityParams.Empty.SpellLevel == sharedSpellLevel &&
                        AbilityParams.Empty.DC == sharedDc;
                    check("barbed-unholy-lifetime-resource", lifetime,
                        new JObject {
                            ["ownerDestroyed"] = owner.Destroyed,
                            ["resourceAtSourceLoss"] =
                                resourceAtSourceLoss,
                            ["conditionRemainedAfterOwner"] =
                                conditionRemained,
                            ["targetSecondsBeforeSourceLoss"] =
                                targetBeforeSourceLoss,
                            ["allSickenedExpiredNaturally"] =
                                conditionExpired,
                            ["expiryFrames"] = expiryFrames,
                            ["manualVictimRemoval"] = false
                        },
                        "Spent use belongs to the summoned source, while inflicted Sickened survives source loss and expires only on its own 1d4-round victim clock.");
                }
                finally
                {
                    EventBus.Unsubscribe(observed);
                }

                DisposeExpandedSummoningUnits(fixture.Created,
                    new[] { astral, wolf, dretch });
                target.Descriptor.Alignment.Set(targetAlignment);
                target.Stats.SaveWill.BaseValue = targetWill;
                target.Stats.HitPoints.BaseValue = targetHitPoints;
                string hostileReset = ResetExpandedSummoningHostile(fixture);
                int cleanupFrames = 0;
                DateTime cleanupDeadline = DateTime.UtcNow.AddSeconds(5);
                while (DateTime.UtcNow < cleanupDeadline &&
                    cleanupFrames++ < 600 && owned.Any(value =>
                        value != null && !value.Destroyed)) yield return 0;
                bool noOwnedAreas = !Game.Instance.State.AreaEffects.All.Any(
                    value => value?.Context != null && owned.Any(actor =>
                        ReferenceEquals(value.Context.MaybeCaster, actor)));
                bool cleanup = owned.All(value => value.Destroyed &&
                        !value.IsInState) && noOwnedAreas &&
                    target.Descriptor.Alignment.Value == targetAlignment &&
                    target.Stats.SaveWill.BaseValue == targetWill &&
                    target.Stats.HitPoints.BaseValue == targetHitPoints &&
                    target.Damage == fixture.HostileDamage &&
                    target.Descriptor.State.Size == fixture.HostileSize;
                check("barbed-unholy-owned-cleanup", cleanup, new JObject {
                    ["ownedDestroyed"] = new JArray(owned.Select(value =>
                        new JObject { ["unit"] = value.UniqueId,
                            ["destroyed"] = value.Destroyed,
                            ["inState"] = value.IsInState })),
                    ["ownedAreasAbsent"] = noOwnedAreas,
                    ["hostileReset"] = hostileReset,
                    ["hostileAlignmentRestored"] = target.Descriptor.Alignment
                        .Value == targetAlignment,
                    ["hostileWillRestored"] = target.Stats.SaveWill.BaseValue ==
                        targetWill,
                    ["hostileHpRestored"] = target.Stats.HitPoints.BaseValue ==
                        targetHitPoints,
                    ["hostileDamageRestored"] = target.Damage ==
                        fixture.HostileDamage,
                    ["cleanupFrames"] = cleanupFrames
                },
                    "The isolated Barbed transaction removes its four owned summons, commands, areas and victim state, then restores every changed hostile field.");
                targetChanged = false;
            }
            finally
            {
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(
                    _request.EvidenceDirectory,
                    "sprint35-infernal-barbed.json"),
                    receipt.ToString(Formatting.Indented));
                if (targetChanged && target != null && !target.Destroyed)
                {
                    target.Descriptor.Alignment.Set(targetAlignment);
                    target.Stats.SaveWill.BaseValue = targetWill;
                    target.Stats.HitPoints.BaseValue = targetHitPoints;
                    target.Damage = targetDamage;
                    if (sickened != null)
                        foreach (Buff buff in target.Buffs.Enumerable.Where(
                            value => ReferenceEquals(value.Blueprint,
                                sickened)).ToArray()) buff.Remove();
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

        private static Buff Infernal35Sickened(UnitEntityData target,
            UnitEntityData owner, BlueprintBuff sickened)
        {
            return target?.Buffs?.Enumerable.SingleOrDefault(value =>
                ReferenceEquals(value.Blueprint, sickened) &&
                ReferenceEquals(value.Context?.MaybeCaster, owner) &&
                ReferenceEquals(value.Context?.AssociatedBlueprint,
                    sickened));
        }

        private static JObject Infernal35BurstRow(
            Astral31SmiteEvents observed, UnitEntityData target)
        {
            RuleSpellResistanceCheck resistance = observed.Resistance(target);
            RuleSavingThrow save = observed.Save(target);
            RuleRollDice damageRoll = Infernal35DamageRoll(observed, target);
            RuleRollDice durationRoll = Infernal35DurationRoll(observed,
                target);
            RuleDealDamage damage = observed.Damage(target);
            return new JObject {
                ["target"] = target.UniqueId,
                ["alignment"] = target.Descriptor.Alignment.Value.ToString(),
                ["sr"] = resistance?.SpellResistance,
                ["srRoll"] = resistance != null &&
                    resistance.HasResistanceRoll ?
                        (int?)resistance.Roll.Value : null,
                ["resisted"] = resistance?.IsSpellResisted,
                ["saveD20"] = save?.D20.Value,
                ["saveStat"] = save?.StatValue,
                ["saveDc"] = save?.DifficultyClass,
                ["passed"] = save?.IsPassed,
                ["damageDice"] = damageRoll == null ? null :
                    damageRoll.DiceFormula.Rolls + "d" +
                        (int)damageRoll.DiceFormula.Dice,
                ["damageRolled"] = damageRoll == null ? null :
                    (int?)damageRoll.Result,
                ["durationDice"] = durationRoll == null ? null :
                    durationRoll.DiceFormula.Rolls + "d" +
                        (int)durationRoll.DiceFormula.Dice,
                ["durationRolled"] = durationRoll == null ? null :
                    (int?)durationRoll.Result,
                ["damageBeforeReduction"] = damage?.DamageWithoutReduction,
                ["damage"] = damage?.Damage,
                ["targetDamage"] = target.Damage,
                ["conditionApplications"] = new JArray(observed.Applications
                    .Where(value => ReferenceEquals(value.Initiator, target))
                    .Select(value => new JObject {
                        ["canApply"] = value.CanApply,
                        ["immunity"] = value.Immunity,
                        ["applied"] = value.AppliedBuff != null,
                        ["durationSeconds"] = value.Duration?.TotalSeconds,
                        ["sourceAbility"] = value.Context?.SourceAbility?
                            .AssetGuid,
                        ["associatedBlueprint"] = value.Context?
                            .AssociatedBlueprint?.AssetGuid,
                        ["parentBlueprint"] = value.Context?.ParentContext?
                            .AssociatedBlueprint?.AssetGuid,
                        ["caster"] = value.Context?.MaybeCaster?.UniqueId }))
            };
        }

        private static RuleRollDice Infernal35DamageRoll(
            Astral31SmiteEvents observed, UnitEntityData target)
        {
            return observed.Rolls.SingleOrDefault(value =>
                ReferenceEquals(value.Reason?.Context?.MainTarget.Unit,
                    target) && value.DiceFormula.Rolls > 1 &&
                (value.DiceFormula.Dice == DiceType.D6 ||
                 value.DiceFormula.Dice == DiceType.D8));
        }

        private static RuleRollDice Infernal35DurationRoll(
            Astral31SmiteEvents observed, UnitEntityData target)
        {
            return observed.Rolls.SingleOrDefault(value =>
                ReferenceEquals(value.Reason?.Context?.MainTarget.Unit,
                    target) && value.DiceFormula.Rolls == 1 &&
                value.DiceFormula.Dice == DiceType.D4);
        }
    }
}
