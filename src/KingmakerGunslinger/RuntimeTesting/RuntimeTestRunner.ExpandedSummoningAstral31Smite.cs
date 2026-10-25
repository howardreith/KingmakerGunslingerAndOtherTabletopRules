using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firing;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> ReviewAstral31Smite(
            ExpandedSummoningCorrectionFixture fixture)
        {
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool oldPause = Game.Instance.IsPaused;
            UnitEntityData owner = null, sibling = null, dretch = null, wolf = null;
            UnitEntityData target = null;
            Alignment targetAlignment = Alignment.TrueNeutral;
            int targetWill = 0, targetHitPoints = 0, targetDamage = 0;
            bool targetChanged = false;
            BlueprintAbility holySmite = null;
            BlueprintBuff blinded = null;
            int sharedCl = AbilityParams.Empty.CasterLevel;
            int sharedSpellLevel = AbilityParams.Empty.SpellLevel;
            int sharedDc = AbilityParams.Empty.DC;
            CreateExpandedSummoningCorrectionHostile(fixture);
            target = fixture.Hostile;
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;

                owner = CastExpandedSummoningOwnTier(fixture, "astral-deva");
                sibling = CastExpandedSummoningOwnTier(fixture, "astral-deva");
                dretch = CastExpandedSummoningOwnTier(fixture, "dretch");
                wolf = CastExpandedSummoningOwnTier(fixture, "wolf");
                foreach (UnitEntityData unit in new[] { owner, sibling, dretch, wolf })
                {
                    Sprint28SetBrainActivePreservingActions(unit, false);
                    unit.Commands.InterruptAll(true);
                    unit.Commands.RemoveFinishedAndUpdateQueue();
                }

                holySmite = (BlueprintAbility)ExpandedSummoningBlueprints.Registered
                    .BySymbol[ExpandedSummoningSpecialBuilder.Astral31Prefix +
                        "HolySmite"];
                blinded = (BlueprintBuff)ExpandedSummoningBlueprints.Registered
                    .BySymbol[ExpandedSummoningSpecialBuilder.Astral31Prefix +
                        "HolySmiteBlinded"];
                var holyWord = (BlueprintAbility)ExpandedSummoningBlueprints
                    .Registered.BySymbol[ExpandedSummoningSpecialBuilder
                        .Astral31Prefix + "HolyWord"];
                var granted = owner.Descriptor.Abilities.GetAbility(holySmite);
                var data = granted == null ? null : new AbilityData(granted);
                var parameters = data?.CalculateParams();
                var around = holySmite.GetComponent<AbilityTargetsAround>();
                var run = holySmite.GetComponent<AbilityEffectRunAction>();
                var descriptor = holySmite.GetComponent<SpellDescriptorComponent>();
                var action = run?.Actions?.Actions?.OfType<
                    ContextActionAstralHolySmite>().SingleOrDefault();
                var blindCondition = blinded.GetComponents<AddCondition>()
                    .SingleOrDefault(value => value.Condition ==
                        UnitCondition.Blindness);
                var blindDescriptor = blinded.GetComponent<SpellDescriptorComponent>();
                bool mapping = data != null && parameters != null &&
                    holySmite.Type == AbilityType.SpellLike &&
                    holySmite.Range == AbilityRange.Medium &&
                    holySmite.CanTargetPoint && holySmite.CanTargetFriends &&
                    holySmite.CanTargetEnemies && !holySmite.CanTargetSelf &&
                    holySmite.SpellResistance &&
                    holySmite.ActionType == UnitCommand.CommandType.Standard &&
                    ReferenceEquals(holySmite.Icon, holyWord.Icon) &&
                    around != null && around.Targets ==
                        Kingmaker.UnitLogic.Abilities.Components.TargetType.Any &&
                    Math.Abs(around.AoERadius.Value -
                        AstralDevaRulesPolicy.HolySmiteRadiusFeet) < 0.001f &&
                    run != null && run.SavingThrowType == SavingThrowType.Unknown &&
                    action != null && ReferenceEquals(action.SourceUnit,
                        owner.Blueprint) && ReferenceEquals(action.Blinded, blinded) &&
                    action.OutsiderType != null && action.OutsiderType.AssetGuid ==
                        "9054d3988d491d944ac144e27b6bc318" &&
                    descriptor != null && (descriptor.Descriptor &
                        SpellDescriptor.Good) != 0 &&
                    blindCondition != null && blindDescriptor != null &&
                    (blindDescriptor.Descriptor & SpellDescriptor.Blindness) != 0 &&
                    blinded.Stacking == StackingType.Stack &&
                    holySmite.GetComponent<AbilityResourceLogic>() == null &&
                    holySmite.GetComponent<SpellListComponent>() == null &&
                    parameters.CasterLevel == 13 && parameters.SpellLevel == 4 &&
                    parameters.DC == 20 &&
                    !owner.Blueprint.Brain.Actions.OfType<
                        Kingmaker.Controllers.Brain.Blueprints.BlueprintAiCastSpell>()
                        .Any(value => ReferenceEquals(value.Ability, holySmite));
                Sprint25Check("astral-smite-native-mapping", mapping,
                    new JObject { ["owner"] = owner.UniqueId,
                        ["ability"] = holySmite.AssetGuid,
                        ["blind"] = blinded.AssetGuid,
                        ["type"] = holySmite.Type.ToString(),
                        ["range"] = holySmite.Range.ToString(),
                        ["radiusFeet"] = around?.AoERadius.Value,
                        ["targets"] = around?.Targets.ToString(),
                        ["spellResistance"] = holySmite.SpellResistance,
                        ["cl"] = parameters?.CasterLevel,
                        ["spellLevel"] = parameters?.SpellLevel,
                        ["dc"] = parameters?.DC,
                        ["icon"] = holySmite.Icon == null ? null : holySmite.Icon.name,
                        ["holyWordIconExact"] = ReferenceEquals(
                            holySmite.Icon, holyWord.Icon),
                        ["resourceLogic"] = holySmite.GetComponent<
                            AbilityResourceLogic>() != null,
                        ["spellList"] = holySmite.GetComponent<
                            SpellListComponent>() != null,
                        ["aiActions"] = owner.Blueprint.Brain.Actions.OfType<
                            Kingmaker.Controllers.Brain.Blueprints.BlueprintAiCastSpell>()
                            .Count(value => ReferenceEquals(value.Ability, holySmite)),
                        ["manualOnly"] = true },
                    "The hidden Holy Smite is one project-owned CL13/DC20 at-will all-creature burst with the native spell-resistance gate, exact private resolver/blind state, appropriate Holy Word art and no AI/resource/list leakage.");
                if (!mapping) throw new InvalidOperationException(
                    "Astral Holy Smite mapping boundary failed.");

                Vector3 far; string lane, direction;
                Vector3 origin = Sprint24AiCombatLane(fixture.Caster, owner,
                    target, 4f, out far, out lane, out direction,
                    new Feet(AstralDevaRulesPolicy.HolySmiteRadiusFeet).Meters +
                        2f, new[] { sibling, dretch, wolf }, 30);
                PlaceExpandedSummoningUnit(owner, origin);
                PlaceExpandedSummoningUnit(target, far);
                PlaceExpandedSummoningUnit(sibling, far + Vector3.right * 1.5f);
                PlaceExpandedSummoningUnit(dretch, far - Vector3.right * 1.5f);
                PlaceExpandedSummoningUnit(wolf, far + Vector3.forward * 1.5f);
                foreach (UnitEntityData unit in new[] {
                    owner, sibling, dretch, wolf, target })
                {
                    unit.Wake(0f);
                    if (!Game.Instance.State.AwakeUnits.Contains(unit))
                        Game.Instance.State.AwakeUnits.Add(unit);
                    unit.SightCache.ClearIfNeeded();
                }

                targetAlignment = target.Descriptor.Alignment.Value;
                targetWill = target.Stats.SaveWill.BaseValue;
                targetHitPoints = target.Stats.HitPoints.BaseValue;
                targetDamage = target.Damage;
                targetChanged = true;
                target.Descriptor.Alignment.Set(Alignment.ChaoticEvil);
                sibling.Descriptor.Alignment.Set(Alignment.ChaoticEvil);
                dretch.Descriptor.Alignment.Set(Alignment.ChaoticEvil);
                wolf.Descriptor.Alignment.Set(Alignment.TrueNeutral);
                target.Stats.SaveWill.BaseValue = 100;
                sibling.Stats.SaveWill.BaseValue = -100;
                dretch.Stats.SaveWill.BaseValue = -100;
                wolf.Stats.SaveWill.BaseValue = 100;
                foreach (UnitEntityData unit in new[] {
                    target, sibling, dretch, wolf })
                {
                    unit.Stats.HitPoints.BaseValue = 100000;
                    unit.Damage = 0;
                }

                bool protectedClear = Game.Instance.Player.Party.Where(value =>
                    value != null && value.IsInGame).All(value =>
                        Vector3.Distance(value.Position, far) >
                            new Feet(AstralDevaRulesPolicy.HolySmiteRadiusFeet)
                                .Meters + 1f + value.Corpulence);
                bool fixtureExact = protectedClear && owner.IsEnemy(target) &&
                    owner.IsAlly(sibling) && owner.IsAlly(dretch) &&
                    owner.IsAlly(wolf) && data.CanTarget(new TargetWrapper(far)) &&
                    new[] { owner, sibling, dretch, wolf, target }.All(value =>
                        Vector3.Distance(value.Position, far) <=
                            new Feet(AstralDevaRulesPolicy.HolySmiteRadiusFeet)
                                .Meters + value.Corpulence);
                Sprint25Check("astral-smite-owned-fixture", fixtureExact,
                    new JObject { ["owner"] = owner.UniqueId,
                        ["srAlly"] = sibling.UniqueId,
                        ["evilOutsiderAlly"] = dretch.UniqueId,
                        ["neutralAlly"] = wolf.UniqueId,
                        ["evilEnemy"] = target.UniqueId,
                        ["pointTargetLegal"] = data.CanTarget(
                            new TargetWrapper(far)),
                        ["protectedPartyOutsideTwentyFeet"] = protectedClear,
                        ["lane"] = lane, ["direction"] = direction,
                        ["controlledDirectCastNotAi"] = true },
                    "Only disposable actors occupy the all-creature burst: a good caster, evil SR25 outsider ally, evil no-SR outsider ally, neutral ally and evil ordinary enemy; protected party actors remain outside.");
                if (!fixtureExact) throw new InvalidOperationException(
                    "Astral Holy Smite fixture is unsafe or untargetable.");

                owner.Memory.Add(target);
                target.Memory.Add(owner);
                owner.JoinCombat(); target.JoinCombat();
                Game.Instance.Player.UpdateIsInCombat();
                for (int frame = 0; frame < 4; frame++) yield return 0;

                var observed = new Astral31SmiteEvents(owner, holySmite,
                    blinded, action.OutsiderType,
                    new[] { owner, sibling, dretch, wolf, target });
                EventBus.Subscribe(observed);
                try
                {
                    UnitUseAbility command = null;
                    var costs = new JArray();
                    using (var economy = new FirearmReviewCosts(owner, costs))
                    {
                        foreach (int frame in Sprint26Cast(owner, holySmite,
                            new TargetWrapper(far), value => command = value))
                            yield return frame;
                        bool commandExact = command != null && command.IsFinished &&
                            command.Result == UnitCommand.ResultType.Success &&
                            command.ExecutionProcess != null &&
                            command.ExecutionProcess.IsEnded &&
                            economy.Count(command) == 1 &&
                            economy.Exact(command, false) &&
                            command.AiAction == null && !command.Cutscene &&
                            !command.IsIgnoreCooldown;
                        Sprint25Check("astral-smite-command", commandExact,
                            new JObject { ["owner"] = owner.UniqueId,
                                ["command"] = command == null ? null :
                                    Sprint27AiCommand(command),
                                ["costs"] = costs,
                                ["directNativeCastCommands"] = 1,
                                ["aiCommands"] = 0 },
                            "One real manual Holy Smite command pays one standard action and reaches terminal success without a resource debit or AI selection.");
                        if (!commandExact) throw new InvalidOperationException(
                            "Astral Holy Smite command failed.");
                    }

                    for (int frame = 0; frame < 16; frame++) yield return 0;
                    var dretchSave = observed.Save(dretch);
                    var dretchRoll = observed.Roll(dretch);
                    var dretchHit = observed.Damage(dretch);
                    Buff dretchBlind = Astral31SmiteBlind(dretch, owner,
                        blinded);
                    int dretchExpected = dretchRoll == null ? -1 :
                        (int)dretchRoll.Result;
                    bool dretchExact = dretchSave != null &&
                        !dretchSave.IsPassed && dretchSave.DifficultyClass == 20 &&
                        dretchRoll != null && dretchRoll.DiceFormula.Rolls == 10 &&
                        dretchRoll.DiceFormula.Dice == DiceType.D6 &&
                        dretchHit != null && dretchHit.DamageWithoutReduction ==
                            dretchExpected && dretch.Damage == dretchHit.Damage &&
                        dretchBlind != null &&
                        dretch.Descriptor.State.HasCondition(UnitCondition.Blindness) &&
                        dretchBlind.TimeLeft.TotalSeconds > 0d &&
                        dretchBlind.TimeLeft.TotalSeconds <= 6.1d;

                    var wolfSave = observed.Save(wolf);
                    var wolfRoll = observed.Roll(wolf);
                    var wolfHit = observed.Damage(wolf);
                    int wolfRolled = wolfRoll == null ? -1 : (int)wolfRoll.Result;
                    int wolfExpected = wolfRolled < 0 ? -1 : wolfRolled / 4;
                    bool wolfExact = wolfSave != null && wolfSave.IsPassed &&
                        wolfSave.DifficultyClass == 20 && wolfRoll != null &&
                        wolfRoll.DiceFormula.Rolls == 5 &&
                        wolfRoll.DiceFormula.Dice == DiceType.D8 &&
                        wolfHit != null && wolfHit.DamageWithoutReduction ==
                            wolfExpected && wolf.Damage == wolfHit.Damage &&
                        Astral31SmiteBlind(wolf, owner, blinded) == null &&
                        !wolf.Descriptor.State.HasCondition(UnitCondition.Blindness);

                    var targetSave = observed.Save(target);
                    var targetRoll = observed.Roll(target);
                    var targetHit = observed.Damage(target);
                    int targetRolled = targetRoll == null ? -1 :
                        (int)targetRoll.Result;
                    int targetExpected = targetRolled < 0 ? -1 : targetRolled / 2;
                    bool targetExact = targetSave != null && targetSave.IsPassed &&
                        targetSave.DifficultyClass == 20 && targetRoll != null &&
                        targetRoll.DiceFormula.Rolls == 5 &&
                        targetRoll.DiceFormula.Dice == DiceType.D8 &&
                        targetHit != null && targetHit.DamageWithoutReduction ==
                            targetExpected && target.Damage == targetHit.Damage &&
                        Astral31SmiteBlind(target, owner, blinded) == null &&
                        !target.Descriptor.State.HasCondition(UnitCondition.Blindness);

                    RuleSpellResistanceCheck siblingResistance =
                        observed.Resistance(sibling);
                    var siblingSave = observed.Save(sibling);
                    var siblingRoll = observed.Roll(sibling);
                    var siblingHit = observed.Damage(sibling);
                    Buff siblingBlind = Astral31SmiteBlind(sibling, owner,
                        blinded);
                    bool siblingResolved = siblingResistance != null &&
                        siblingResistance.HasSpellResistance &&
                        siblingResistance.HasResistanceRoll &&
                        siblingResistance.SpellResistance == 25 &&
                        siblingResistance.Context.Params.CasterLevel == 13;
                    bool siblingCoherent = siblingResolved &&
                        (siblingResistance.IsSpellResisted ?
                            siblingSave == null && siblingRoll == null &&
                                siblingHit == null && siblingBlind == null &&
                                sibling.Damage == 0 :
                            siblingSave != null && !siblingSave.IsPassed &&
                                siblingRoll != null &&
                                siblingRoll.DiceFormula.Rolls == 10 &&
                                siblingRoll.DiceFormula.Dice == DiceType.D6 &&
                                siblingHit != null &&
                                siblingHit.DamageWithoutReduction ==
                                    (int)siblingRoll.Result &&
                                sibling.Damage == siblingHit.Damage &&
                                siblingBlind != null);
                    bool goodUnaffected = observed.Save(owner) == null &&
                        observed.Roll(owner) == null &&
                        observed.Damage(owner) == null && owner.Damage == 0 &&
                        Astral31SmiteBlind(owner, owner, blinded) == null;
                    Buff[] victimBlinds = new[] { dretchBlind, siblingBlind }
                        .Where(value => value != null).Distinct().ToArray();
                    bool victimParameters = victimBlinds.Length > 0 &&
                        victimBlinds.All(value => value.Context != null &&
                            value.Context.ParentContext != null &&
                            value.Context.ParentContext.ParentContext == null &&
                            ReferenceEquals(value.Context.AssociatedBlueprint, blinded) &&
                            ReferenceEquals(value.Context.ParentContext.AssociatedBlueprint, blinded) &&
                            ReferenceEquals(value.Context.Params,
                                value.Context.ParentContext.Params) &&
                            value.Context.Params.CasterLevel == 13 &&
                            value.Context.Params.SpellLevel == 4 &&
                            value.Context.Params.DC == 20 && value.IsFromSpell &&
                            !value.IsNotDispelable &&
                            !ReferenceEquals(value.Context.Params, AbilityParams.Empty)) &&
                        victimBlinds.Select(value => value.Context.Params).Distinct()
                            .Count() == victimBlinds.Length;
                    bool sharedParameters = AbilityParams.Empty.CasterLevel == sharedCl &&
                        AbilityParams.Empty.SpellLevel == sharedSpellLevel &&
                        AbilityParams.Empty.DC == sharedDc;
                    bool effect = dretchExact && wolfExact && targetExact &&
                        siblingCoherent && goodUnaffected && victimParameters &&
                        sharedParameters;
                    Sprint25Check("astral-smite-alignment-sr-effect", effect,
                        new JObject { ["owner"] = owner.UniqueId,
                            ["goodOwner"] = observed.Row(owner),
                            ["evilSr25OutsiderAlly"] = observed.Row(sibling),
                            ["evilOutsiderAlly"] = observed.Row(dretch),
                            ["neutralAlly"] = observed.Row(wolf),
                            ["evilOrdinaryEnemy"] = observed.Row(target),
                            ["dretchExpected"] = dretchExpected,
                            ["wolfExpectedQuarter"] = wolfExpected,
                            ["targetExpectedHalf"] = targetExpected,
                            ["dretchBlindSeconds"] =
                                dretchBlind?.TimeLeft.TotalSeconds,
                            ["siblingSrOutcome"] = siblingResistance == null ?
                                null : (siblingResistance.IsSpellResisted ?
                                    "resisted" : "penetrated"),
                            ["friendlyEvilAffected"] = dretchExact,
                            ["victimParametersIndependent"] = victimParameters,
                            ["sharedEmptyUnchanged"] = sharedParameters,
                            ["sharedEmptyBefore"] = new JArray(sharedCl,
                                sharedSpellLevel, sharedDc),
                            ["sharedEmptyAfter"] = new JArray(
                                AbilityParams.Empty.CasterLevel,
                                AbilityParams.Empty.SpellLevel, AbilityParams.Empty.DC),
                            ["victimContexts"] = new JArray(victimBlinds.Select(value =>
                                new JObject { ["owner"] = value.Context.MaybeCaster?.UniqueId,
                                    ["associatedBlueprint"] = value.Context.AssociatedBlueprint?.AssetGuid,
                                    ["privateVictimRoot"] = value.Context.ParentContext?.AssociatedBlueprint?.AssetGuid,
                                    ["victimRootParentless"] = value.Context.ParentContext != null &&
                                        value.Context.ParentContext.ParentContext == null,
                                    ["inheritsPrivateRootParameters"] = ReferenceEquals(
                                        value.Context.Params, value.Context.ParentContext?.Params),
                                    ["cl"] = value.Context.Params.CasterLevel,
                                    ["spellLevel"] = value.Context.Params.SpellLevel,
                                    ["dc"] = value.Context.Params.DC,
                                    ["fromSpell"] = value.IsFromSpell,
                                    ["notDispellable"] = value.IsNotDispelable,
                                    ["aliasesSharedEmpty"] = ReferenceEquals(
                                        value.Context.Params, AbilityParams.Empty) })),
                            ["forcedRolls"] = 0 },
                        "One actual all-creature burst leaves good untouched, gives a failed-save evil outsider ally10d6 plus one-round blindness, quarters a successful neutral ally, halves a successful ordinary evil enemy, and honors the evil SR25 outsider ally's native roll before any save or effect.");
                    if (!effect) throw new InvalidOperationException(
                        "Astral Holy Smite alignment/SR effect failed.");

                    double dretchBlindBeforeSourceLoss =
                        dretchBlind.TimeLeft.TotalSeconds;
                    DisposeExpandedSummoningUnits(fixture.Created,
                        new[] { owner });
                    bool blindRemainedAfterOwner = !dretchBlind.IsDisposed &&
                        dretch.Buffs.Enumerable.Contains(dretchBlind) &&
                        dretch.Descriptor.State.HasCondition(
                            UnitCondition.Blindness) &&
                        dretchBlind.TimeLeft.TotalSeconds > 0d &&
                        dretchBlind.TimeLeft.TotalSeconds <=
                            dretchBlindBeforeSourceLoss;
                    DateTime deadline = DateTime.UtcNow.AddSeconds(10);
                    int expiryFrames = 0;
                    while (DateTime.UtcNow < deadline && expiryFrames++ < 1000 &&
                        victimBlinds.Any(value => !value.IsDisposed))
                        yield return 0;
                    bool blindExpired = victimBlinds.All(value =>
                            value.IsDisposed) &&
                        new[] { sibling, dretch, wolf, target }.All(value =>
                            !value.Buffs.Enumerable.Any(buff => ReferenceEquals(
                                buff.Blueprint, blinded))) &&
                        !dretch.Descriptor.State.HasCondition(
                            UnitCondition.Blindness);
                    bool lifetime = owner.Destroyed && blindRemainedAfterOwner &&
                        blindExpired && AbilityParams.Empty.CasterLevel == sharedCl &&
                        AbilityParams.Empty.SpellLevel == sharedSpellLevel &&
                        AbilityParams.Empty.DC == sharedDc;
                    Sprint25Check("astral-smite-native-lifetime", lifetime,
                        new JObject { ["ownerDestroyed"] = owner.Destroyed,
                            ["failedSaveBlindRemainedAfterOwner"] =
                                blindRemainedAfterOwner,
                            ["dretchBlindDisposedAfterOwner"] =
                                dretchBlind.IsDisposed,
                            ["dretchBlindSecondsBeforeSourceLoss"] =
                                dretchBlindBeforeSourceLoss,
                            ["dretchBlindSecondsAfterSourceLoss"] =
                                dretchBlind.IsDisposed ? null :
                                    (double?)dretchBlind.TimeLeft.TotalSeconds,
                            ["dretchContextCasterAfterSourceLoss"] =
                                dretchBlind.Context?.MaybeCaster?.UniqueId,
                            ["victimBuffsObserved"] = victimBlinds.Length,
                            ["allHolySmiteBlindExpired"] = blindExpired,
                            ["expiryFrames"] = expiryFrames,
                            ["manualSourceLossRemoval"] = false },
                        "The failed-save blindness survives source loss and expires on its own one-round victim-effect lifetime; owner loss does not erase or extend it.");
                    if (!lifetime) throw new InvalidOperationException(
                        "Astral Holy Smite victim lifetime failed.");
                }
                finally { EventBus.Unsubscribe(observed); }

                foreach (UnitEntityData unit in new[] { sibling, dretch, wolf })
                {
                    Sprint28SetBrainActivePreservingActions(unit, false);
                    unit.Commands.InterruptAll(true);
                    unit.Commands.RemoveFinishedAndUpdateQueue();
                }
                DisposeExpandedSummoningUnits(fixture.Created,
                    new[] { sibling, dretch, wolf });
                target.Descriptor.Alignment.Set(targetAlignment);
                target.Stats.SaveWill.BaseValue = targetWill;
                target.Stats.HitPoints.BaseValue = targetHitPoints;
                string hostileReset = ResetExpandedSummoningHostile(fixture);
                int cleanupFrames = 0;
                DateTime cleanupDeadline = DateTime.UtcNow.AddSeconds(5);
                while (DateTime.UtcNow < cleanupDeadline &&
                    cleanupFrames++ < 600 && new[] { owner, sibling, dretch, wolf }
                        .Any(value => value != null && !value.Destroyed)) yield return 0;
                bool noOwnedAreas = !Game.Instance.State.AreaEffects.All.Any(
                    value => value?.Context != null && ReferenceEquals(
                        value.Context.MaybeCaster, owner));
                bool cleanup = new[] { owner, sibling, dretch, wolf }.All(value =>
                        value.Destroyed && !value.IsInState) && noOwnedAreas &&
                    target.Descriptor.Alignment.Value == targetAlignment &&
                    target.Stats.SaveWill.BaseValue == targetWill &&
                    target.Stats.HitPoints.BaseValue == targetHitPoints &&
                    target.Damage == fixture.HostileDamage &&
                    target.Descriptor.State.Size == fixture.HostileSize;
                Sprint25Check("astral-smite-owned-cleanup", cleanup,
                    new JObject { ["ownerDestroyed"] = owner.Destroyed,
                        ["siblingDestroyed"] = sibling.Destroyed,
                        ["dretchDestroyed"] = dretch.Destroyed,
                        ["wolfDestroyed"] = wolf.Destroyed,
                        ["ownedAreasAbsent"] = noOwnedAreas,
                        ["hostileReset"] = hostileReset,
                        ["hostileAlignmentRestored"] =
                            target.Descriptor.Alignment.Value == targetAlignment,
                        ["hostileWillRestored"] =
                            target.Stats.SaveWill.BaseValue == targetWill,
                        ["hostileHpRestored"] =
                            target.Stats.HitPoints.BaseValue == targetHitPoints,
                        ["hostileDamageRestored"] =
                            target.Damage == fixture.HostileDamage,
                        ["cleanupFrames"] = cleanupFrames },
                    "The isolated Holy Smite transaction removes its four owned summons, commands, auras and victim state, then restores every changed hostile field.");
                if (!cleanup) throw new InvalidOperationException(
                    "Astral Holy Smite cleanup failed.");
                targetChanged = false;
            }
            finally
            {
                if (targetChanged && target != null && !target.Destroyed)
                {
                    target.Descriptor.Alignment.Set(targetAlignment);
                    target.Stats.SaveWill.BaseValue = targetWill;
                    target.Stats.HitPoints.BaseValue = targetHitPoints;
                    target.Damage = targetDamage;
                    if (blinded != null)
                        foreach (Buff buff in target.Buffs.Enumerable.Where(value =>
                            ReferenceEquals(value.Blueprint, blinded)).ToArray())
                            buff.Remove();
                }
                foreach (UnitEntityData unit in new[] {
                    owner, sibling, dretch, wolf }.Where(value =>
                        value != null && !value.Destroyed))
                {
                    Sprint28SetBrainActivePreservingActions(unit, false);
                    unit.Commands.InterruptAll(true);
                }
                DisposeExpandedSummoningUnits(fixture.Created,
                    new[] { owner, sibling, dretch, wolf }.Where(value =>
                        value != null && !value.Destroyed).ToArray());
                ResetExpandedSummoningHostile(fixture);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = oldPause;
            }
        }

        private static Buff Astral31SmiteBlind(UnitEntityData target,
            UnitEntityData owner, BlueprintBuff blinded)
        {
            return target?.Buffs?.Enumerable.SingleOrDefault(value =>
                ReferenceEquals(value.Blueprint, blinded) &&
                ReferenceEquals(value.Context?.MaybeCaster, owner) &&
                ReferenceEquals(value.Context?.AssociatedBlueprint, blinded));
        }

        private sealed class Astral31SmiteEvents :
            IGlobalRulebookHandler<RuleSpellResistanceCheck>,
            IGlobalRulebookHandler<RuleSavingThrow>,
            IGlobalRulebookHandler<RuleRollDice>,
            IGlobalRulebookHandler<RuleDealDamage>,
            IGlobalRulebookHandler<RuleApplyBuff>
        {
            private readonly UnitEntityData _owner;
            private readonly BlueprintAbility _ability;
            private readonly BlueprintBuff _blinded;
            private readonly BlueprintUnitFact _outsiderType;
            private readonly HashSet<UnitEntityData> _targets;
            internal readonly List<RuleSpellResistanceCheck> Resistances =
                new List<RuleSpellResistanceCheck>();
            internal readonly List<RuleSavingThrow> Saves =
                new List<RuleSavingThrow>();
            internal readonly List<RuleRollDice> Rolls =
                new List<RuleRollDice>();
            internal readonly List<RuleDealDamage> DamageRules =
                new List<RuleDealDamage>();
            internal readonly List<RuleApplyBuff> Applications =
                new List<RuleApplyBuff>();

            internal Astral31SmiteEvents(UnitEntityData owner,
                BlueprintAbility ability, BlueprintBuff blinded,
                BlueprintUnitFact outsiderType,
                IEnumerable<UnitEntityData> targets)
            {
                _owner = owner; _ability = ability; _blinded = blinded;
                _outsiderType = outsiderType;
                _targets = new HashSet<UnitEntityData>(targets);
            }

            private bool OwnContext(Kingmaker.UnitLogic.Mechanics.MechanicsContext context)
            {
                return context != null &&
                    ReferenceEquals(context.MaybeCaster, _owner) &&
                    ReferenceEquals(context.SourceAbility, _ability);
            }

            private bool OwnBlindContext(
                Kingmaker.UnitLogic.Mechanics.MechanicsContext context)
            {
                return context != null &&
                    ReferenceEquals(context.MaybeCaster, _owner) &&
                    ReferenceEquals(context.AssociatedBlueprint, _blinded);
            }

            internal RuleSpellResistanceCheck Resistance(UnitEntityData target)
            { return Resistances.SingleOrDefault(value => ReferenceEquals(value.Target, target)); }
            internal RuleSavingThrow Save(UnitEntityData target)
            { return Saves.SingleOrDefault(value => ReferenceEquals(value.Initiator, target)); }
            internal RuleRollDice Roll(UnitEntityData target)
            {
                return Rolls.SingleOrDefault(value =>
                    ReferenceEquals(value.Reason?.Context?.MainTarget.Unit, target));
            }
            internal RuleDealDamage Damage(UnitEntityData target)
            { return DamageRules.SingleOrDefault(value => ReferenceEquals(value.Target, target)); }

            internal JObject Row(UnitEntityData target)
            {
                RuleSpellResistanceCheck resistance = Resistance(target);
                RuleSavingThrow save = Save(target);
                RuleRollDice roll = Roll(target);
                RuleDealDamage damage = Damage(target);
                var buffs = Applications.Where(value =>
                    ReferenceEquals(value.Initiator, target)).ToArray();
                return new JObject { ["target"] = target.UniqueId,
                    ["alignment"] = target.Descriptor.Alignment.Value.ToString(),
                    ["outsider"] = _outsiderType != null &&
                        target.Descriptor.HasFact(_outsiderType),
                    ["sr"] = resistance?.SpellResistance,
                    ["srRoll"] = resistance != null &&
                        resistance.HasResistanceRoll ?
                            (int?)resistance.Roll.Value : null,
                    ["resisted"] = resistance?.IsSpellResisted,
                    ["saveD20"] = save?.D20.Value,
                    ["saveStat"] = save?.StatValue,
                    ["saveDc"] = save?.DifficultyClass,
                    ["passed"] = save?.IsPassed,
                    ["dice"] = roll == null ? null :
                        roll.DiceFormula.Rolls + "d" +
                            (int)roll.DiceFormula.Dice,
                    ["rolled"] = roll == null ? null : (int?)roll.Result,
                    ["damageBeforeReduction"] = damage?.DamageWithoutReduction,
                    ["damage"] = damage?.Damage,
                    ["targetDamage"] = target.Damage,
                    ["blindApplications"] = new JArray(buffs.Select(value =>
                        new JObject { ["canApply"] = value.CanApply,
                            ["immunity"] = value.Immunity,
                            ["applied"] = value.AppliedBuff != null,
                            ["durationSeconds"] = value.Duration?.TotalSeconds,
                            ["sourceAbility"] = value.Context?.SourceAbility?.AssetGuid,
                            ["associatedBlueprint"] =
                                value.Context?.AssociatedBlueprint?.AssetGuid,
                            ["parentBlueprint"] = value.Context?.ParentContext
                                ?.AssociatedBlueprint?.AssetGuid,
                            ["caster"] = value.Context?.MaybeCaster?.UniqueId })) };
            }

            public void OnEventAboutToTrigger(RuleSpellResistanceCheck evt) { }
            public void OnEventDidTrigger(RuleSpellResistanceCheck evt)
            {
                if (OwnContext(evt.Context) && _targets.Contains(evt.Target))
                    Resistances.Add(evt);
            }
            public void OnEventAboutToTrigger(RuleSavingThrow evt) { }
            public void OnEventDidTrigger(RuleSavingThrow evt)
            {
                if (_targets.Contains(evt.Initiator) &&
                    OwnContext(evt.Reason?.Context)) Saves.Add(evt);
            }
            public void OnEventAboutToTrigger(RuleRollDice evt) { }
            public void OnEventDidTrigger(RuleRollDice evt)
            {
                UnitEntityData target = evt.Reason?.Context?.MainTarget.Unit;
                if (ReferenceEquals(evt.Initiator, _owner) &&
                    _targets.Contains(target) && OwnContext(evt.Reason?.Context))
                    Rolls.Add(evt);
            }
            public void OnEventAboutToTrigger(RuleDealDamage evt) { }
            public void OnEventDidTrigger(RuleDealDamage evt)
            {
                if (ReferenceEquals(evt.Initiator, _owner) &&
                    _targets.Contains(evt.Target) &&
                    ReferenceEquals(evt.SourceAbility, _ability))
                    DamageRules.Add(evt);
            }
            public void OnEventAboutToTrigger(RuleApplyBuff evt) { }
            public void OnEventDidTrigger(RuleApplyBuff evt)
            {
                if (_targets.Contains(evt.Initiator) &&
                    ReferenceEquals(evt.Blueprint, _blinded) &&
                    OwnBlindContext(evt.Context) &&
                    !Applications.Any(value => ReferenceEquals(
                        value.AppliedBuff, evt.AppliedBuff)))
                    Applications.Add(evt);
            }
        }
    }
}
