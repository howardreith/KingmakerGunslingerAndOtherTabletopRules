using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Blueprints;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Two separate fresh-world transactions keep Cloud's all-creature
        // fog from obscuring Storm's target. Both use actual native commands,
        // resource owners and effects; no spell result is injected.
        private IEnumerable<int> ReviewSprint29CloudMagic(
            ExpandedSummoningCorrectionFixture fixture)
        {
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool oldPause = Game.Instance.IsPaused;
            UnitEntityData owner = null, sibling = null;
            Game.Instance.IsPaused = true;
            CreateExpandedSummoningCorrectionHostile(fixture);
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;
                owner = CastExpandedSummoningOwnTier(fixture, "cloud-giant");
                sibling = CastExpandedSummoningOwnTier(fixture, "cloud-giant");
                SetExpandedSummoningBrainActive(owner, false);
                SetExpandedSummoningBrainActive(sibling, false);
                UnitEntityData hostile = fixture.Hostile;
                Vector3 far; string lane, direction;
                Vector3 origin = Sprint24AiCombatLane(fixture.Caster,
                    owner, hostile, 8f, out far, out lane, out direction);
                PlaceExpandedSummoningUnit(owner, origin);
                PlaceExpandedSummoningUnit(hostile, far);
                owner.SightCache.ClearIfNeeded();
                hostile.SightCache.ClearIfNeeded();
                owner.JoinCombat(); hostile.JoinCombat();
                owner.Memory.Add(hostile); hostile.Memory.Add(owner);
                BlueprintAbility mist = Sprint29MagicAbility(fixture,
                    "CloudObscuringMist");
                BlueprintAbility fog = Sprint29MagicAbility(fixture,
                    "CloudFogCloud");
                BlueprintAbilityResource resource = Sprint29MagicResource(
                    fixture, "CloudFogResource");
                BlueprintAbilityAreaEffect area = BlueprintLibraryLookup
                    .RequireExact<BlueprintAbilityAreaEffect>(
                        BlueprintBootstrap.Library,
                        "e09010a73354a794293ebc7b33c2d130",
                        "Cloud native obscuring area");
                var mistData = new AbilityData(owner.Descriptor.Abilities
                    .GetAbility(mist));
                var selfUnit = new TargetWrapper(owner);
                var selfPoint = new TargetWrapper(owner.Position);
                bool unitTarget = mistData.CanTarget(selfUnit);
                bool pointTarget = mistData.CanTarget(selfPoint);
                Sprint25Check("cloud-mist-target", unitTarget || pointTarget,
                    new JObject { ["owner"] = owner.UniqueId,
                        ["unitTarget"] = unitTarget,
                        ["pointTarget"] = pointTarget,
                        ["lane"] = lane },
                    "The granted at-will native mist has a direct self target.");
                if (!unitTarget && !pointTarget)
                    throw new InvalidOperationException(
                        "Cloud native mist has no direct self target.");
                int before = Sprint29OwnedAreas(owner, area).Length;
                foreach (int frame in Sprint29MagicCast(owner, mist,
                    unitTarget ? selfUnit : selfPoint, "cloud-mist", 16, 0))
                    yield return frame;
                for (int frame = 0; frame < 100 &&
                    Sprint29OwnedAreas(owner, area).Length <= before; frame++)
                    yield return 0;
                var mistAreas = Sprint29OwnedAreas(owner, area);
                int fogBefore = owner.Descriptor.Resources.GetResourceAmount(
                    resource);
                bool mistExact = mistAreas.Length == before + 1 &&
                    Sprint29AreaDuration(mistAreas.Last()) > 0 &&
                    Sprint29AreaDuration(mistAreas.Last()) <= 960.1 &&
                    mistAreas.Last().Context?.Params.CasterLevel == 16 &&
                    fogBefore == 1 &&
                    sibling.Descriptor.Resources.GetResourceAmount(resource) == 1 &&
                    !mist.GetComponents<AbilityResourceLogic>().Any();
                Sprint25Check("cloud-mist-effect", mistExact,
                    new JObject { ["owner"] = owner.UniqueId,
                        ["areasBefore"] = before,
                        ["areasAfter"] = mistAreas.Length,
                        ["durationSeconds"] = mistAreas.LastOrDefault() == null
                            ? -1d : Sprint29AreaDuration(mistAreas.Last()),
                        ["casterLevel"] = mistAreas.LastOrDefault()
                            ?.Context?.Params.CasterLevel,
                        ["fogResource"] = fogBefore,
                        ["siblingResource"] = sibling.Descriptor.Resources
                            .GetResourceAmount(resource) },
                    "At-will CL16 mist creates its real finite all-creature area without spending fog cloud.");
                var fogTarget = new TargetWrapper(hostile.Position);
                Sprint25Check("cloud-fog-target",
                    new AbilityData(owner.Descriptor.Abilities.GetAbility(fog))
                        .CanTarget(fogTarget),
                    new JObject { ["owner"] = owner.UniqueId,
                        ["target"] = hostile.UniqueId,
                        ["lane"] = lane,
                        ["distance"] = owner.DistanceTo(hostile) },
                    "The distinct limited fog action can target a visible remote point.");
                foreach (int frame in Sprint29MagicCast(owner, fog,
                    fogTarget, "cloud-fog", 16, 0)) yield return frame;
                for (int frame = 0; frame < 100 &&
                    Sprint29OwnedAreas(owner, area).Length <= mistAreas.Length;
                    frame++) yield return 0;
                var both = Sprint29OwnedAreas(owner, area);
                bool fogExact = both.Length == mistAreas.Length + 1 &&
                    both.Any(value => Sprint29AreaDuration(value) > 9500 &&
                        Sprint29AreaDuration(value) <= 9600.1 &&
                        value.Context?.Params.CasterLevel == 16) &&
                    owner.Descriptor.Resources.GetResourceAmount(resource) == 0 &&
                    sibling.Descriptor.Resources.GetResourceAmount(resource) == 1 &&
                    !new AbilityData(owner.Descriptor.Abilities.GetAbility(fog))
                        .IsAvailable;
                Sprint25Check("cloud-fog-effect", fogExact,
                    new JObject { ["owner"] = owner.UniqueId,
                        ["areas"] = new JArray(both.Select(value =>
                            new JObject { ["durationSeconds"] =
                                Sprint29AreaDuration(value),
                                ["caster"] = value.Context?.MaybeCaster?.UniqueId,
                                ["casterLevel"] = value.Context?.Params.CasterLevel })),
                        ["fogResource"] = owner.Descriptor.Resources
                            .GetResourceAmount(resource),
                        ["siblingResource"] = sibling.Descriptor.Resources
                            .GetResourceAmount(resource) },
                    "The distinct CL16 remote fog lasts 160 minutes, spends only its owner's one use and cannot recast.");
            }
            finally
            {
                if (owner != null)
                {
                    owner.Commands.InterruptAll(true);
                    if (owner.IsInCombat) owner.LeaveCombat();
                }
                if (sibling != null)
                {
                    sibling.Commands.InterruptAll(true);
                    if (sibling.IsInCombat) sibling.LeaveCombat();
                }
                DisposeExpandedSummoningUnits(fixture.Created,
                    new[] { owner, sibling }.Where(value => value != null)
                        .ToArray());
                ResetExpandedSummoningHostile(fixture);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = oldPause;
            }
        }

        private IEnumerable<int> ReviewSprint29StormMagic(
            ExpandedSummoningCorrectionFixture fixture)
        {
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool oldPause = Game.Instance.IsPaused;
            UnitEntityData owner = null, sibling = null;
            Game.Instance.IsPaused = true;
            CreateExpandedSummoningCorrectionHostile(fixture);
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;
                owner = CastExpandedSummoningOwnTier(fixture, "storm-giant");
                sibling = CastExpandedSummoningOwnTier(fixture, "storm-giant");
                SetExpandedSummoningBrainActive(owner, false);
                SetExpandedSummoningBrainActive(sibling, false);
                UnitEntityData hostile = fixture.Hostile;
                Vector3 far; string lane, direction;
                Vector3 origin = Sprint24AiCombatLane(fixture.Caster,
                    owner, hostile, 8f, out far, out lane, out direction);
                PlaceExpandedSummoningUnit(owner, origin);
                PlaceExpandedSummoningUnit(hostile, far);
                owner.SightCache.ClearIfNeeded();
                hostile.SightCache.ClearIfNeeded();
                owner.JoinCombat(); hostile.JoinCombat();
                owner.Memory.Add(hostile); hostile.Memory.Add(owner);
                BlueprintBuff freedom = BlueprintLibraryLookup
                    .RequireExact<BlueprintBuff>(BlueprintBootstrap.Library,
                        "1533e782fca42b84ea370fc1dcbf4fc1",
                        "Storm constant Freedom of Movement");
                BlueprintFeature immunity = BlueprintLibraryLookup
                    .RequireExact<BlueprintFeature>(BlueprintBootstrap.Library,
                        "cd1e5ab641a833c49994aff99db98952",
                        "Storm electricity immunity");
                bool defenses = owner.Descriptor.Buffs.GetBuff(freedom) != null &&
                    sibling.Descriptor.Buffs.GetBuff(freedom) != null &&
                    owner.Descriptor.HasFact(immunity) &&
                    sibling.Descriptor.HasFact(immunity);
                Sprint25Check("storm-constant-defenses", defenses,
                    new JObject { ["owner"] = owner.UniqueId,
                        ["sibling"] = sibling.UniqueId,
                        ["freedom"] = owner.Descriptor.Buffs.GetBuff(freedom)
                            != null,
                        ["siblingFreedom"] = sibling.Descriptor.Buffs
                            .GetBuff(freedom) != null,
                        ["electricityImmune"] = owner.Descriptor
                            .HasFact(immunity) },
                    "Each hidden Storm has its native constant movement protection and electricity immunity.");
                BlueprintAbility call = Sprint29MagicAbility(fixture,
                    "StormCallLightning");
                BlueprintAbility bolt = Sprint29MagicAbility(fixture,
                    "StormLightningBolt");
                BlueprintAbility chain = Sprint29MagicAbility(fixture,
                    "StormChainLightning");
                BlueprintBuff state = fixture.Blueprints.OfType<BlueprintBuff>()
                    .Single(value => value.name ==
                        "KMG_Summoning_Giant_Sprint29_StormLightningState");
                BlueprintAbilityResource callResource = Sprint29MagicResource(
                    fixture, "StormCallResource");
                BlueprintAbilityResource chainResource = Sprint29MagicResource(
                    fixture, "StormChainResource");
                var callData = new AbilityData(owner.Descriptor.Abilities
                    .GetAbility(call));
                var selfUnit = new TargetWrapper(owner);
                var selfPoint = new TargetWrapper(owner.Position);
                bool unitTarget = callData.CanTarget(selfUnit);
                bool pointTarget = callData.CanTarget(selfPoint);
                Sprint25Check("storm-call-target", unitTarget || pointTarget,
                    new JObject { ["owner"] = owner.UniqueId,
                        ["unitTarget"] = unitTarget,
                        ["pointTarget"] = pointTarget,
                        ["lane"] = lane },
                    "The CL15 Call Lightning activation has a direct native self target.");
                if (!unitTarget && !pointTarget)
                    throw new InvalidOperationException(
                        "Storm Call Lightning activation has no self target.");
                foreach (int frame in Sprint29MagicCast(owner, call,
                    unitTarget ? selfUnit : selfPoint, "storm-call", 15, 15))
                    yield return frame;
                for (int frame = 0; frame < 100 &&
                    owner.Descriptor.Buffs.GetBuff(state) == null; frame++)
                    yield return 0;
                var active = owner.Descriptor.Buffs.GetBuff(state);
                int callUses = owner.Descriptor.Resources.GetResourceAmount(
                    callResource);
                int chainUses = owner.Descriptor.Resources.GetResourceAmount(
                    chainResource);
                bool activated = active != null &&
                    active.TimeLeft.TotalSeconds > 0 &&
                    active.Context?.Params.CasterLevel == 15 &&
                    owner.Descriptor.Abilities.GetAbility(bolt) != null &&
                    callUses == 0 && chainUses == 1 &&
                    sibling.Descriptor.Resources.GetResourceAmount(
                        callResource) == 1;
                Sprint25Check("storm-call-activation", activated,
                    new JObject { ["owner"] = owner.UniqueId,
                        ["stateRemaining"] = active?.TimeLeft.TotalSeconds,
                        ["casterLevel"] = active?.Context?.Params.CasterLevel,
                        ["boltGranted"] = owner.Descriptor.Abilities
                            .GetAbility(bolt) != null,
                        ["callUses"] = callUses,
                        ["chainUses"] = chainUses,
                        ["siblingCallUses"] = sibling.Descriptor.Resources
                            .GetResourceAmount(callResource) },
                    "One native CL15 activation spends Call Lightning only and grants a finite bolt state.");
                if (!activated) yield break;
                var attack = new Sprint24AttackObserver {
                    Owner = owner, Target = hostile };
                var saves = new Sprint29MagicSaves(hostile);
                EventBus.Subscribe(attack);
                EventBus.Subscribe(saves);
                try
                {
                    double beforeBolt = active.TimeLeft.TotalSeconds;
                    foreach (int frame in Sprint29MagicCast(owner, bolt,
                        new TargetWrapper(hostile), "storm-bolt", 15, 15))
                        yield return frame;
                    for (int frame = 0; frame < 600 &&
                        attack.Damage.Count == 0; frame++) yield return 0;
                    var afterBolt = owner.Descriptor.Buffs.GetBuff(state);
                    bool boltExact = attack.Damage.Count > 0 &&
                        saves.Rows.Count > 0 && saves.Rows.OfType<JObject>()
                            .All(value => (int)value["dc"] == 15) &&
                        afterBolt != null &&
                        afterBolt.TimeLeft.TotalSeconds < beforeBolt &&
                        owner.Descriptor.Resources.GetResourceAmount(
                            callResource) == 0 &&
                        owner.Descriptor.Resources.GetResourceAmount(
                            chainResource) == 1;
                    Sprint25Check("storm-bolt-effect", boltExact,
                        new JObject { ["owner"] = owner.UniqueId,
                            ["target"] = hostile.UniqueId,
                            ["damagePackets"] = attack.Damage.Count,
                            ["saves"] = saves.Rows.DeepClone(),
                            ["stateBefore"] = beforeBolt,
                            ["stateAfter"] = afterBolt?.TimeLeft.TotalSeconds,
                            ["callUses"] = owner.Descriptor.Resources
                                .GetResourceAmount(callResource),
                            ["chainUses"] = owner.Descriptor.Resources
                                .GetResourceAmount(chainResource) },
                        "A separate native bolt deals electricity with a DC15 save and spends finite state, not another daily use.");
                    attack.Clear(); saves.Rows.Clear();
                    foreach (int frame in Sprint29MagicCast(owner, chain,
                        new TargetWrapper(hostile), "storm-chain", 15, 18))
                        yield return frame;
                    for (int frame = 0; frame < 600 &&
                        attack.Damage.Count == 0; frame++) yield return 0;
                    bool chainExact = attack.Damage.Count > 0 &&
                        saves.Rows.Count > 0 && saves.Rows.OfType<JObject>()
                            .All(value => (int)value["dc"] == 18) &&
                        owner.Descriptor.Resources.GetResourceAmount(
                            callResource) == 0 &&
                        owner.Descriptor.Resources.GetResourceAmount(
                            chainResource) == 0 &&
                        sibling.Descriptor.Resources.GetResourceAmount(
                            callResource) == 1 &&
                        sibling.Descriptor.Resources.GetResourceAmount(
                            chainResource) == 1;
                    Sprint25Check("storm-chain-effect", chainExact,
                        new JObject { ["owner"] = owner.UniqueId,
                            ["target"] = hostile.UniqueId,
                            ["damagePackets"] = attack.Damage.Count,
                            ["saves"] = saves.Rows.DeepClone(),
                            ["callUses"] = owner.Descriptor.Resources
                                .GetResourceAmount(callResource),
                            ["chainUses"] = owner.Descriptor.Resources
                                .GetResourceAmount(chainResource),
                            ["siblingCallUses"] = sibling.Descriptor.Resources
                                .GetResourceAmount(callResource),
                            ["siblingChainUses"] = sibling.Descriptor.Resources
                                .GetResourceAmount(chainResource) },
                        "Native CL15 Chain Lightning has its own DC18 save, damage and independent one-use resource.");
                }
                finally
                {
                    EventBus.Unsubscribe(attack);
                    EventBus.Unsubscribe(saves);
                }
            }
            finally
            {
                if (owner != null)
                {
                    owner.Commands.InterruptAll(true);
                    if (owner.IsInCombat) owner.LeaveCombat();
                }
                if (sibling != null)
                {
                    sibling.Commands.InterruptAll(true);
                    if (sibling.IsInCombat) sibling.LeaveCombat();
                }
                DisposeExpandedSummoningUnits(fixture.Created,
                    new[] { owner, sibling }.Where(value => value != null)
                        .ToArray());
                ResetExpandedSummoningHostile(fixture);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = oldPause;
            }
        }

        private static BlueprintAbility Sprint29MagicAbility(
            ExpandedSummoningCorrectionFixture fixture, string suffix)
        {
            return fixture.Blueprints.OfType<BlueprintAbility>().Single(value =>
                value.name == "KMG_Summoning_Giant_Sprint29_" + suffix);
        }

        private static BlueprintAbilityResource Sprint29MagicResource(
            ExpandedSummoningCorrectionFixture fixture, string suffix)
        {
            return fixture.Blueprints.OfType<BlueprintAbilityResource>()
                .Single(value => value.name ==
                    "KMG_Summoning_Giant_Sprint29_" + suffix);
        }

        private static AreaEffectEntityData[] Sprint29OwnedAreas(
            UnitEntityData owner,
                BlueprintAbilityAreaEffect blueprint)
        {
            return Game.Instance.State.AreaEffects.All.Where(value =>
                value != null && ReferenceEquals(value.Blueprint, blueprint) &&
                value.Context != null &&
                ReferenceEquals(value.Context.MaybeCaster, owner)).ToArray();
        }

        private static double Sprint29AreaDuration(AreaEffectEntityData area)
        {
            object value = ReadExpandedSummoningOptionalMember(area,
                "m_Duration");
            return value is TimeSpan ? ((TimeSpan)value).TotalSeconds : -1d;
        }

        private IEnumerable<int> Sprint29MagicCast(UnitEntityData owner,
            BlueprintAbility blueprint, TargetWrapper target, string name,
            int casterLevel, int dc)
        {
            var granted = owner.Descriptor.Abilities.GetAbility(blueprint);
            if (granted == null) throw new InvalidOperationException(
                "Sprint 29 spell-like action not granted: " + blueprint.name);
            var data = new AbilityData(granted);
            var parameters = data.CalculateParams();
            bool ready = data.IsAvailable && data.CanTarget(target) &&
                parameters.CasterLevel == casterLevel &&
                (dc == 0 || parameters.DC == dc);
            Sprint25Check(name + "-ready", ready,
                new JObject { ["owner"] = owner.UniqueId,
                    ["ability"] = blueprint.AssetGuid,
                    ["available"] = data.IsAvailable,
                    ["canTarget"] = data.CanTarget(target),
                    ["casterLevel"] = parameters.CasterLevel,
                    ["dc"] = parameters.DC,
                    ["spellLevel"] = parameters.SpellLevel },
                "The direct native spell-like command has its printed caster level, DC and legal target.");
            if (!ready) throw new InvalidOperationException(
                "Sprint 29 native spell-like action is unavailable: " + name);
            UnitUseAbility command = null;
            var costs = new JArray();
            using (var economy = new FirearmReviewCosts(owner, costs))
            {
                foreach (int frame in Sprint26Cast(owner, blueprint, target,
                    value => command = value)) yield return frame;
                bool exact = command != null && command.IsFinished &&
                    command.Result == UnitCommand.ResultType.Success &&
                    economy.Count(command) == 1 &&
                    economy.Exact(command, false) &&
                    !command.Cutscene && !command.IsIgnoreCooldown;
                Sprint25Check(name + "-cast", exact,
                    new JObject { ["owner"] = owner.UniqueId,
                        ["ability"] = blueprint.AssetGuid,
                        ["finished"] = command?.IsFinished,
                        ["result"] = command?.Result.ToString(),
                        ["costs"] = costs,
                        ["fixtureCastCommands"] = 0 },
                    "The exact owner issues one native spell-like command and pays its ordinary action cost.");
            }
        }

        private sealed class Sprint29MagicSaves :
            IGlobalRulebookHandler<RuleSavingThrow>
        {
            private readonly UnitEntityData _target;
            internal readonly JArray Rows = new JArray();
            internal Sprint29MagicSaves(UnitEntityData target)
            { _target = target; }
            public void OnEventAboutToTrigger(RuleSavingThrow evt) { }
            public void OnEventDidTrigger(RuleSavingThrow evt)
            {
                if (ReferenceEquals(evt.Initiator, _target))
                    Rows.Add(new JObject { ["dc"] = evt.DifficultyClass,
                        ["passed"] = evt.IsPassed });
            }
        }
    }
}
