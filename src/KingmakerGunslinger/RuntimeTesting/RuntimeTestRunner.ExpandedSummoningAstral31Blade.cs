using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> ReviewAstral31Blade(
            ExpandedSummoningCorrectionFixture fixture)
        {
            const string nativeAbilityGuid =
                "36c8971e91f1745418cc3ffdfac17b74";
            const string nativeAreaGuid =
                "cae4347a512809e4388fb3949dc0bc67";
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool oldPause = Game.Instance.IsPaused;
            UnitEntityData owner = null, sibling = null, ally = null;
            AreaEffectEntityData area = null;
            UnitEntityData target = fixture.Hostile;
            int targetReflex = 0, targetHitPoints = 0, targetDamage = 0;
            bool targetChanged = false;
            Game.Instance.IsPaused = true;
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
                ally = CastExpandedSummoningOwnTier(fixture, "wolf");
                foreach (UnitEntityData unit in new[] { owner, sibling, ally })
                {
                    Sprint28SetBrainActivePreservingActions(unit, false);
                    unit.Commands.InterruptAll(true);
                    unit.Commands.RemoveFinishedAndUpdateQueue();
                }
                for (int frame = 0; frame < 8; frame++) yield return 0;

                var blade = (BlueprintAbility)ExpandedSummoningBlueprints.Registered
                    .BySymbol[ExpandedSummoningSpecialBuilder.Astral31Prefix +
                        "BladeBarrier"];
                var resource = (BlueprintAbilityResource)ExpandedSummoningBlueprints
                    .Registered.BySymbol[ExpandedSummoningSpecialBuilder.Astral31Prefix +
                        "BladeBarrierResource"];
                var healResource = (BlueprintAbilityResource)ExpandedSummoningBlueprints
                    .Registered.BySymbol[ExpandedSummoningSpecialBuilder.Astral31Prefix +
                        "HealResource"];
                var native = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(
                    BlueprintBootstrap.Library, nativeAbilityGuid,
                    "native Blade Barrier spell");
                var nativeArea = BlueprintLibraryLookup.RequireExact<
                    BlueprintAbilityAreaEffect>(BlueprintBootstrap.Library,
                        nativeAreaGuid, "native Blade Barrier area");
                var spawn = ExpandedSummoningSpecialBuilder.FindSpawnAreaEffect(blade);
                var cost = blade.GetComponent<AbilityResourceLogic>();
                var granted = owner.Descriptor.Abilities.GetAbility(blade);
                var data = granted == null ? null : new AbilityData(granted);
                var parameters = data?.CalculateParams();
                bool manualOnly = owner.Blueprint.Brain != null &&
                    !(owner.Blueprint.Brain.Actions ?? Array.Empty<BlueprintAiAction>())
                        .OfType<BlueprintAiCastSpell>().Any(value =>
                            ReferenceEquals(value.Ability, blade));
                bool mapping = granted != null && blade.Type == AbilityType.SpellLike &&
                    blade.ActionType == UnitCommand.CommandType.Standard &&
                    !blade.IsFullRoundAction &&
                    blade.ActionType == native.ActionType && blade.Range == native.Range &&
                    blade.CanTargetPoint == native.CanTargetPoint &&
                    blade.CanTargetEnemies == native.CanTargetEnemies &&
                    blade.CanTargetFriends == native.CanTargetFriends &&
                    blade.SpellResistance == native.SpellResistance &&
                    ReferenceEquals(blade.Icon, native.Icon) &&
                    spawn != null && ReferenceEquals(spawn.AreaEffect, nativeArea) &&
                    !blade.GetComponents<SpellListComponent>().Any() &&
                    cost != null && ReferenceEquals(cost.RequiredResource, resource) &&
                    cost.IsSpendResource && cost.Amount == 1 &&
                    parameters != null && parameters.CasterLevel == 13 &&
                    parameters.SpellLevel == 6 && parameters.DC == 22 && manualOnly &&
                    owner.Descriptor.Resources.GetResourceAmount(resource) == 1 &&
                    sibling.Descriptor.Resources.GetResourceAmount(resource) == 1;
                Sprint25Check("astral-blade-native-mapping", mapping, new JObject {
                    ["owner"] = owner.UniqueId, ["sibling"] = sibling.UniqueId,
                    ["ability"] = blade.AssetGuid,
                    ["nativeAbility"] = native.AssetGuid,
                    ["area"] = spawn?.AreaEffect?.AssetGuid,
                    ["type"] = blade.Type.ToString(),
                    ["action"] = blade.ActionType.ToString(),
                    ["fullRound"] = blade.IsFullRoundAction,
                    ["range"] = blade.Range.ToString(),
                    ["canTargetPoint"] = blade.CanTargetPoint,
                    ["canTargetEnemies"] = blade.CanTargetEnemies,
                    ["canTargetFriends"] = blade.CanTargetFriends,
                    ["spellResistance"] = blade.SpellResistance,
                    ["nativeIconExact"] = ReferenceEquals(blade.Icon, native.Icon),
                    ["spellListComponents"] = blade.GetComponents<SpellListComponent>().Count(),
                    ["cl"] = parameters?.CasterLevel,
                    ["spellLevel"] = parameters?.SpellLevel,
                    ["dc"] = parameters?.DC,
                    ["ownerUses"] = owner.Descriptor.Resources.GetResourceAmount(resource),
                    ["siblingUses"] = sibling.Descriptor.Resources.GetResourceAmount(resource),
                    ["manualOnly"] = manualOnly },
                    "The hidden CL13/level6/DC22 SLA preserves the exact native Blade Barrier icon, targeting and area graph, owns one independent use and has no autonomous priority before area safety is qualified.");
                if (!mapping) throw new InvalidOperationException(
                    "Astral Blade Barrier mapping is not exact.");

                Vector3 far; string lane, direction;
                Vector3 origin = Sprint24AiCombatLane(fixture.Caster, owner,
                    target, 8f, out far, out lane, out direction,
                    new Feet(60).Meters + 2f, new[] { sibling, ally }, 30);
                PlaceExpandedSummoningUnit(owner, origin);
                PlaceExpandedSummoningUnit(sibling, origin + Vector3.right * 2f);
                PlaceExpandedSummoningUnit(target, far);
                PlaceExpandedSummoningUnit(ally, far);
                bool protectedClear = Game.Instance.Player.Party.Where(value =>
                    value != null && value.IsInGame).All(value =>
                        Vector3.Distance(value.Position, far) >
                            new Feet(60).Meters + 1f + value.Corpulence);
                bool fixtureExact = protectedClear && owner.IsEnemy(target) &&
                    owner.IsAlly(ally) && data.CanTarget(new TargetWrapper(far));
                Sprint25Check("astral-blade-owned-fixture", fixtureExact,
                    new JObject { ["owner"] = owner.UniqueId,
                        ["target"] = target.UniqueId,
                        ["eligibleAlly"] = ally.UniqueId,
                        ["lane"] = lane, ["direction"] = direction,
                        ["distance"] = owner.DistanceTo(target),
                        ["pointTargetLegal"] = data.CanTarget(new TargetWrapper(far)),
                        ["protectedPartyOutsideSixtyFeet"] = protectedClear,
                        ["controlledDirectCastNotAi"] = true },
                    "Only disposable attributed actors occupy the native all-creature area; every protected party actor stays beyond the displayed sixty-foot envelope.");
                if (!fixtureExact) throw new InvalidOperationException(
                    "Astral Blade Barrier fixture is unsafe or untargetable.");

                targetReflex = target.Stats.SaveReflex.BaseValue;
                targetHitPoints = target.Stats.HitPoints.BaseValue;
                targetDamage = target.Damage;
                targetChanged = true;
                target.Stats.SaveReflex.BaseValue = -100;
                target.Stats.HitPoints.BaseValue = 100000;
                target.Damage = 0;
                ally.Stats.SaveReflex.BaseValue = -100;
                ally.Stats.HitPoints.BaseValue = 100000;
                ally.Damage = 0;
                foreach (UnitEntityData unit in new[] { owner, target, ally })
                {
                    unit.Wake(0f);
                    if (!Game.Instance.State.AwakeUnits.Contains(unit))
                        Game.Instance.State.AwakeUnits.Add(unit);
                }
                owner.Memory.Add(target); target.Memory.Add(owner);
                owner.SightCache.ClearIfNeeded(); target.SightCache.ClearIfNeeded();
                owner.JoinCombat(); target.JoinCombat();
                Game.Instance.Player.UpdateIsInCombat();

                var events = new Astral31BladeEvents(owner, blade,
                    new[] { target, ally });
                EventBus.Subscribe(events);
                try
                {
                    UnitUseAbility command = null;
                    var costs = new JArray();
                    using (var economy = new FirearmReviewCosts(owner, costs))
                    {
                        foreach (int frame in Sprint26Cast(owner, blade,
                            new TargetWrapper(far), value => command = value))
                            yield return frame;
                        bool commandExact = command != null && command.IsFinished &&
                            command.Result == UnitCommand.ResultType.Success &&
                            economy.Count(command) == 1 && economy.Exact(command, false) &&
                            !command.Cutscene && !command.IsIgnoreCooldown;
                        Sprint25Check("astral-blade-command", commandExact,
                            new JObject { ["owner"] = owner.UniqueId,
                                ["command"] = command == null ? null :
                                    Sprint27AiCommand(command),
                                ["costs"] = costs,
                                ["directNativeCastCommands"] = 1,
                                ["aiCommands"] = 0 },
                            "One manually issued native Blade Barrier command pays its standard action and resolves normally; this row makes no AI claim.");
                        if (!commandExact) throw new InvalidOperationException(
                            "Astral Blade Barrier command did not resolve normally.");
                    }
                    for (int frame = 0; frame < 180 &&
                        Sprint29OwnedAreas(owner, nativeArea).Length == 0; frame++)
                        yield return 0;
                    area = Sprint29OwnedAreas(owner, nativeArea).SingleOrDefault();
                    if (area == null) throw new InvalidOperationException(
                        "Astral Blade Barrier did not create one attributed native area.");
                    DateTime effectDeadline = DateTime.UtcNow.AddSeconds(8);
                    int effectFrames = 0;
                    while (DateTime.UtcNow < effectDeadline && effectFrames++ < 900 &&
                        (!events.HasSaveAndDamage(target) ||
                            !events.HasSaveAndDamage(ally))) yield return 0;

                    RuleSavingThrow targetSave = events.Saves.FirstOrDefault(value =>
                        ReferenceEquals(value.Initiator, target));
                    RuleSavingThrow allySave = events.Saves.FirstOrDefault(value =>
                        ReferenceEquals(value.Initiator, ally));
                    RuleDealDamage targetHit = events.Damage.FirstOrDefault(value =>
                        ReferenceEquals(value.Target, target));
                    RuleDealDamage allyHit = events.Damage.FirstOrDefault(value =>
                        ReferenceEquals(value.Target, ally));
                    bool NativePacket(RuleDealDamage value)
                    {
                        var packet = value?.DamageBundle?.SingleOrDefault();
                        return value != null && value.Damage > 0 && packet != null &&
                            packet.Dice.Rolls == 13 && packet.Dice.Dice == DiceType.D6;
                    }
                    double duration = Sprint29AreaDuration(area);
                    bool effect = area.Context != null &&
                        ReferenceEquals(area.Context.MaybeCaster, owner) &&
                        ReferenceEquals(area.Context.SourceAbility, blade) &&
                        area.Context.Params.CasterLevel == 13 &&
                        area.Context.Params.SpellLevel == 6 &&
                        area.Context.Params.DC == 22 && duration > 770d &&
                        duration <= 780.1d && area.UnitsInside.Contains(target) &&
                        area.UnitsInside.Contains(ally) &&
                        targetSave != null && targetSave.DifficultyClass == 22 &&
                        targetSave.D20.Value == 1 && !targetSave.IsPassed &&
                        allySave != null && allySave.DifficultyClass == 22 &&
                        allySave.D20.Value == 1 && !allySave.IsPassed &&
                        NativePacket(targetHit) && NativePacket(allyHit);
                    Sprint25Check("astral-blade-all-creature-effect", effect,
                        new JObject { ["owner"] = owner.UniqueId,
                            ["area"] = area.UniqueId,
                            ["areaBlueprint"] = area.Blueprint.AssetGuid,
                            ["sourceAbility"] = area.Context?.SourceAbility?.AssetGuid,
                            ["cl"] = area.Context?.Params.CasterLevel,
                            ["spellLevel"] = area.Context?.Params.SpellLevel,
                            ["dc"] = area.Context?.Params.DC,
                            ["durationSeconds"] = duration,
                            ["inside"] = new JArray(area.UnitsInside.Select(value =>
                                value.UniqueId)),
                            ["saves"] = events.SaveRows(),
                            ["damage"] = events.DamageRows(),
                            ["effectWaitFrames"] = effectFrames,
                            ["eligibleAllyAffected"] = events.HasSaveAndDamage(ally),
                            ["enemyOnlyRewrite"] = false },
                        "The exact native area keeps its all-creature graph: both an enemy and eligible ally make real seeded natural-one Reflex saves against DC22 and receive attributed 13d6 force damage at CL13.");
                    if (!effect) throw new InvalidOperationException(
                        "Astral Blade Barrier native area effect is not exact.");

                    int ownerUses = owner.Descriptor.Resources.GetResourceAmount(resource);
                    int ownerHeal = owner.Descriptor.Resources.GetResourceAmount(healResource);
                    int siblingUses = sibling.Descriptor.Resources.GetResourceAmount(resource);
                    bool resources = ownerUses == 0 && ownerHeal == 1 &&
                        siblingUses == 1 && !data.IsAvailable;
                    Sprint25Check("astral-blade-independent-resource", resources,
                        new JObject { ["owner"] = owner.UniqueId,
                            ["sibling"] = sibling.UniqueId,
                            ["ownerBlade"] = ownerUses,
                            ["ownerHeal"] = ownerHeal,
                            ["siblingBlade"] = siblingUses,
                            ["bladeAvailable"] = data.IsAvailable },
                        "One cast exhausts only this owner's Blade Barrier resource; Heal and the sibling's independent use remain available.");
                    if (!resources) throw new InvalidOperationException(
                        "Astral Blade Barrier resource ownership failed.");

                    double beforeOwnerLoss = Sprint29AreaDuration(area);
                    DisposeExpandedSummoningUnits(fixture.Created,
                        new[] { owner });
                    bool remainedImmediately = Game.Instance.State.AreaEffects.All
                        .Contains(area);
                    for (int frame = 0; frame < 3; frame++) yield return 0;
                    double afterOwnerLoss = Sprint29AreaDuration(area);
                    bool lifetime = owner.Destroyed && remainedImmediately &&
                        Game.Instance.State.AreaEffects.All.Contains(area) &&
                        afterOwnerLoss > 0d && afterOwnerLoss <= beforeOwnerLoss;
                    Sprint25Check("astral-blade-source-loss-lifetime", lifetime,
                        new JObject { ["owner"] = owner.UniqueId,
                            ["ownerDestroyed"] = owner.Destroyed,
                            ["area"] = area.UniqueId,
                            ["remainedImmediatelyAfterOwner"] = remainedImmediately,
                            ["presentAfterThreeFrames"] = Game.Instance.State
                                .AreaEffects.All.Contains(area),
                            ["durationBeforeOwnerLoss"] = beforeOwnerLoss,
                            ["durationAfterOwnerLoss"] = afterOwnerLoss,
                            ["configuredNativeDurationSeconds"] = 780,
                            ["manualSourceLossRemoval"] = false },
                        "The already-created native area survives source loss with its own finite thirteen-minute lifetime; owner loss does not erase the spell's work.");
                    if (!lifetime) throw new InvalidOperationException(
                        "Astral Blade Barrier source-loss lifetime failed.");
                }
                finally
                {
                    EventBus.Unsubscribe(events);
                }

                if (area != null && Game.Instance.State.AreaEffects.All.Contains(area))
                    EndExpandedSummoningArea(area);
                if (targetChanged && target != null && !target.Destroyed)
                {
                    target.Stats.SaveReflex.BaseValue = targetReflex;
                    target.Stats.HitPoints.BaseValue = targetHitPoints;
                    target.Damage = targetDamage;
                    targetChanged = false;
                }
                DisposeExpandedSummoningUnits(fixture.Created,
                    new[] { sibling, ally }.Where(value => value != null &&
                        !value.Destroyed).ToArray());
                ResetExpandedSummoningHostile(fixture);
                int cleanupFrames = 0;
                DateTime cleanupDeadline = DateTime.UtcNow.AddSeconds(5);
                while (DateTime.UtcNow < cleanupDeadline && cleanupFrames++ < 120 &&
                    Game.Instance.State.AreaEffects.All.Any(value =>
                        value?.Context != null && (ReferenceEquals(
                            value.Context.MaybeCaster, owner) || ReferenceEquals(
                            value.Context.MaybeCaster, sibling)))) yield return 0;
                bool noOwnedAreas = !Game.Instance.State.AreaEffects.All.Any(value =>
                    value?.Context != null && (ReferenceEquals(
                        value.Context.MaybeCaster, owner) || ReferenceEquals(
                        value.Context.MaybeCaster, sibling)));
                bool cleanup = owner.Destroyed && sibling.Destroyed && ally.Destroyed &&
                    !owner.IsInState && !sibling.IsInState && !ally.IsInState &&
                    noOwnedAreas && (area == null || !Game.Instance.State
                        .AreaEffects.All.Contains(area)) &&
                    !owner.Commands.Raw.Any(value => value != null && !value.IsFinished) &&
                    !sibling.Commands.Raw.Any(value => value != null && !value.IsFinished);
                Sprint25Check("astral-blade-owned-cleanup", cleanup,
                    new JObject { ["ownerAbsent"] = !owner.IsInState,
                        ["siblingAbsent"] = !sibling.IsInState,
                        ["allyAbsent"] = !ally.IsInState,
                        ["bladeAreaAbsent"] = area == null || !Game.Instance.State
                            .AreaEffects.All.Contains(area),
                        ["ownedAreasAbsent"] = noOwnedAreas,
                        ["cleanupFrames"] = cleanupFrames,
                        ["targetRestored"] = !targetChanged,
                        ["manualAreaCleanupAfterLifetimeObservation"] = true,
                        ["freshProcessBoundaryRequired"] = true },
                    "After the native lifetime and source-loss observations, explicit fixture cleanup removes only the request-owned area and actors and restores the hostile state before process restoration.");
                if (!cleanup) throw new InvalidOperationException(
                    "Astral Blade Barrier owned cleanup failed.");
            }
            finally
            {
                if (area != null && Game.Instance.State.AreaEffects.All.Contains(area))
                    EndExpandedSummoningArea(area);
                if (targetChanged && target != null && !target.Destroyed)
                {
                    target.Stats.SaveReflex.BaseValue = targetReflex;
                    target.Stats.HitPoints.BaseValue = targetHitPoints;
                    target.Damage = targetDamage;
                }
                foreach (UnitEntityData unit in new[] { owner, sibling, ally }.Where(
                    value => value != null && !value.Destroyed))
                {
                    Sprint28SetBrainActivePreservingActions(unit, false);
                    unit.Commands.InterruptAll(true);
                    if (unit.IsInCombat) unit.LeaveCombat();
                }
                DisposeExpandedSummoningUnits(fixture.Created,
                    new[] { owner, sibling, ally }.Where(value => value != null &&
                        !value.Destroyed).ToArray());
                ResetExpandedSummoningHostile(fixture);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = oldPause;
            }
        }

        private sealed class Astral31BladeEvents :
            IGlobalRulebookHandler<RuleSavingThrow>,
            IGlobalRulebookHandler<RuleDealDamage>
        {
            private readonly UnitEntityData _owner;
            private readonly BlueprintAbility _ability;
            private readonly UnitEntityData[] _targets;
            internal readonly List<RuleSavingThrow> Saves =
                new List<RuleSavingThrow>();
            internal readonly List<RuleDealDamage> Damage =
                new List<RuleDealDamage>();

            internal Astral31BladeEvents(UnitEntityData owner,
                BlueprintAbility ability, UnitEntityData[] targets)
            { _owner = owner; _ability = ability; _targets = targets; }

            private bool ExactSource(RuleReason reason)
            {
                return reason?.Context != null &&
                    ReferenceEquals(reason.Context.MaybeCaster, _owner) &&
                    ReferenceEquals(reason.Context.SourceAbility, _ability);
            }

            public void OnEventAboutToTrigger(RuleSavingThrow evt)
            {
                if (_targets.Contains(evt.Initiator) && ExactSource(evt.Reason))
                    UnityEngine.Random.InitState(FindNativeD20Seed(1));
            }
            public void OnEventDidTrigger(RuleSavingThrow evt)
            {
                if (_targets.Contains(evt.Initiator) && ExactSource(evt.Reason))
                    Saves.Add(evt);
            }
            public void OnEventAboutToTrigger(RuleDealDamage evt) { }
            public void OnEventDidTrigger(RuleDealDamage evt)
            {
                if (_targets.Contains(evt.Target) && ExactSource(evt.Reason))
                    Damage.Add(evt);
            }

            internal bool HasSaveAndDamage(UnitEntityData target)
            {
                return Saves.Any(value => ReferenceEquals(value.Initiator, target)) &&
                    Damage.Any(value => ReferenceEquals(value.Target, target));
            }
            internal JArray SaveRows()
            {
                return new JArray(Saves.Select(value => new JObject {
                    ["target"] = value.Initiator.UniqueId,
                    ["type"] = value.StatType.ToString(),
                    ["dc"] = value.DifficultyClass,
                    ["d20"] = value.D20.Value,
                    ["passed"] = value.IsPassed,
                    ["caster"] = value.Reason?.Context?.MaybeCaster?.UniqueId,
                    ["sourceAbility"] = value.Reason?.Context?.SourceAbility?.AssetGuid }));
            }
            internal JArray DamageRows()
            {
                return new JArray(Damage.Select(value => new JObject {
                    ["target"] = value.Target.UniqueId,
                    ["damage"] = value.Damage,
                    ["caster"] = value.Reason?.Context?.MaybeCaster?.UniqueId,
                    ["sourceAbility"] = value.Reason?.Context?.SourceAbility?.AssetGuid,
                    ["packets"] = new JArray(value.DamageBundle.Select(
                        Sprint16DamageLine)) }));
            }
        }
    }
}