using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> ReviewDretch30Cloud(ExpandedSummoningCorrectionFixture fixture)
        {
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool oldPause = Game.Instance.IsPaused;
            UnitEntityData owner = null, sibling = null, ally = null, immune = null;
            var areas = new List<AreaEffectEntityData>();
            var target = fixture.Hostile;
            int fort = 0; bool changedFort = false;
            Game.Instance.IsPaused = true;
            CreateExpandedSummoningCorrectionHostile(fixture); target = fixture.Hostile;
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;
                int publicReferences;
                bool publicExact = ExpandedSummoningPublisher.RequiredBasePublicationIsExact(BlueprintBootstrap.Library, true, out publicReferences);
                int choices = ExpandedSummoningInventoryObserver.CanonicalParentGuids.Select(guid =>
                    BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library, guid, "Dretch hidden publication control"))
                    .Sum(value => value.ComponentsArray.OfType<AbilityVariants>().Single().Variants.Length);
                var hiddenRoutes = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                    .Where(value => value.Creature.Key == "dretch").ToArray();
                bool publication = publicExact && publicReferences == SummonVisibilityCatalog.PublishedLogicalPlacementCount &&
                    choices == publicReferences + ExpandedSummoningIdentityCatalog.NativeExpandedOptionIdentityCount &&
                    hiddenRoutes.Length == 7 && hiddenRoutes.All(value => !SummonVisibilityCatalog.IsPublished(value));
                Sprint25Check("dretch-cloud-hidden-publication", publication, new JObject {
                    ["publicReferences"] = publicReferences, ["choices"] = choices, ["dretchHiddenRoots"] = hiddenRoutes.Length,
                    ["publicExact"] = publicExact }, "Every released choice remains exact and all seven new Dretch roots remain hidden.");
                if (!publication) throw new InvalidOperationException("Hidden Dretch changed the public choice surface.");
                owner = CastExpandedSummoningOwnTier(fixture, "dretch");
                sibling = CastExpandedSummoningOwnTier(fixture, "dretch");
                ally = CastExpandedSummoningOwnTier(fixture, "wolf");
                immune = CastExpandedSummoningOwnTier(fixture, "lemure");
                foreach (var unit in new[] { owner, sibling, ally, immune })
                {
                    Sprint28SetBrainActivePreservingActions(unit, false);
                    unit.Commands.InterruptAll(true); unit.Commands.RemoveFinishedAndUpdateQueue();
                }
                for (int frame = 0; frame < 8; frame++) yield return 0;
                var stats = owner.Descriptor.Stats;
                var weapons = new[] { owner.Body.PrimaryHand.MaybeWeapon }
                    .Concat(owner.Body.AdditionalLimbs.Select(value => value.MaybeWeapon)).ToArray();
                var weaponRows = new JArray(weapons.Select(value => Sprint29WeaponRule(owner, value)));
                bool profile = stats.HitPoints.ModifiedValue == 18 && stats.BaseAttackBonus.ModifiedValue == 2 &&
                    stats.AC.ModifiedValue == 14 && stats.SaveFortitude.ModifiedValue == 5 &&
                    stats.SaveReflex.ModifiedValue == 0 && stats.SaveWill.ModifiedValue == 3 &&
                    stats.SkillPerception.ModifiedValue == 5 && stats.SkillStealth.ModifiedValue == 9 &&
                    weapons.Length == 3 && weaponRows.All(value => (int)value["attack"] == 4 &&
                        (int)value["damageBonus"] == 1 && (int)value["diceRolls"] == 1 && (string)value["dice"] == "D4") &&
                    weapons.All(value => value.AttackRange.Value == 5) && owner.Descriptor.State.CanAct;
                Sprint25Check("dretch-cloud-profile", profile, new JObject {
                    ["owner"] = owner.UniqueId, ["hp"] = stats.HitPoints.ModifiedValue,
                    ["bab"] = stats.BaseAttackBonus.ModifiedValue, ["ac"] = stats.AC.ModifiedValue,
                    ["fort"] = stats.SaveFortitude.ModifiedValue, ["reflex"] = stats.SaveReflex.ModifiedValue,
                    ["will"] = stats.SaveWill.ModifiedValue, ["perception"] = stats.SkillPerception.ModifiedValue,
                    ["stealth"] = stats.SkillStealth.ModifiedValue, ["weapons"] = weaponRows,
                    ["canAct"] = owner.Descriptor.State.CanAct, ["escapeArtistConsumerPending"] = true },
                    "The real hidden adult has three primary 1d4+1 natural weapons and its racial profile; this is not attack/AI or Escape Artist proof.");
                if (!owner.Descriptor.State.CanAct || owner.Descriptor.State.IsDead || weapons.Length != 3)
                    throw new InvalidOperationException("Dretch cloud owner is not a usable native actor.");
                var fear = fixture.Blueprints.OfType<BlueprintAbility>().Single(value => value.name == "KMG_Summoning_OutsiderCelestial_Dretch_CauseFear");
                var cloud = fixture.Blueprints.OfType<BlueprintAbility>().Single(value => value.name == "KMG_Summoning_OutsiderCelestial_Dretch_StinkingCloud");
                var fearResource = fixture.Blueprints.OfType<BlueprintAbilityResource>().Single(value => value.name == "KMG_Summoning_OutsiderCelestial_Dretch_CauseFearResource");
                var cloudResource = fixture.Blueprints.OfType<BlueprintAbilityResource>().Single(value => value.name == "KMG_Summoning_OutsiderCelestial_Dretch_StinkingCloudResource");
                var fearData = new AbilityData(owner.Descriptor.Abilities.GetAbility(fear));
                var cloudData = new AbilityData(owner.Descriptor.Abilities.GetAbility(cloud));
                var fearParams = fearData.CalculateParams(); var cloudParams = cloudData.CalculateParams();
                bool parameters = fearParams.CasterLevel == 2 && fearParams.DC == 11 && fearParams.SpellLevel == 1 &&
                    cloudParams.CasterLevel == 2 && cloudParams.DC == 13 && cloudParams.SpellLevel == 3 &&
                    owner.Descriptor.Resources.GetResourceAmount(fearResource) == 1 &&
                    owner.Descriptor.Resources.GetResourceAmount(cloudResource) == 1;
                Sprint25Check("dretch-cloud-parameters", parameters, new JObject {
                    ["owner"] = owner.UniqueId, ["fearCl"] = fearParams.CasterLevel, ["fearDc"] = fearParams.DC,
                    ["cloudCl"] = cloudParams.CasterLevel, ["cloudDc"] = cloudParams.DC,
                    ["fearUses"] = owner.Descriptor.Resources.GetResourceAmount(fearResource),
                    ["cloudUses"] = owner.Descriptor.Resources.GetResourceAmount(cloudResource) },
                    "The two distinct native SLA data consumers use CL2, DC11/DC13 and independent one-use resources.");
                if (!parameters) throw new InvalidOperationException("Dretch SLA parameters are not coherent.");
                Vector3 far; string lane, direction;
                var origin = Sprint24AiCombatLane(fixture.Caster, owner, target, 4f,
                    out far, out lane, out direction, new Feet(20).Meters + 1f);
                PlaceExpandedSummoningUnit(owner, origin); PlaceExpandedSummoningUnit(target, far);
                PlaceExpandedSummoningUnit(ally, far + Vector3.right);
                PlaceExpandedSummoningUnit(immune, far - Vector3.right);
                bool protectedClear = Game.Instance.Player.Party.Where(value => value != null && value.IsInGame)
                    .All(value => Vector3.Distance(value.Position, far) > new Feet(20).Meters + value.Corpulence);
                Sprint25Check("dretch-cloud-owned-fixture", protectedClear && owner.IsAlly(ally) &&
                    owner.IsAlly(immune) && owner.IsEnemy(target), new JObject {
                        ["owner"] = owner.UniqueId, ["target"] = target.UniqueId, ["eligibleAlly"] = ally.UniqueId,
                        ["immuneAlly"] = immune.UniqueId, ["protectedPartyOutside"] = protectedClear,
                        ["lane"] = lane, ["controlledDirectCastNotAi"] = true },
                    "Only disposable attributed actors enter the native cloud; protected party actors stay outside.");
                if (!protectedClear) throw new InvalidOperationException("Cloud would touch a protected fixture party actor.");
                fort = target.Stats.SaveFortitude.BaseValue; changedFort = true;
                target.Stats.SaveFortitude.BaseValue = -100; ally.Stats.SaveFortitude.BaseValue = -100;
                owner.Memory.Add(target); target.Memory.Add(owner);
                owner.JoinCombat(); target.JoinCombat(); Game.Instance.Player.UpdateIsInCombat();
                new UnitCombatJoinController().Tick(); new UnitCombatPrepareController().Tick();
                var areaBlueprint = BlueprintLibraryLookup.RequireExact<BlueprintAbilityAreaEffect>(BlueprintBootstrap.Library,
                    "aa2e0a0fe89693f4e9205fd52c5ba3e5", "general native all-creature Stinking Cloud");
                var saves = new Dretch30CloudSaves(new[] { target, ally, owner, immune });
                EventBus.Subscribe(saves);
                try
                {
                    var costs = new JArray(); UnitUseAbility command = null;
                    using (var economy = new FirearmReviewCosts(owner, costs))
                    {
                        foreach (int frame in Sprint26Cast(owner, cloud, new TargetWrapper(far), value => command = value))
                            yield return frame;
                        bool commandExact = command != null && command.IsFinished && command.Result == UnitCommand.ResultType.Success &&
                            economy.Count(command) == 1 && economy.Exact(command, false) && !command.Cutscene && !command.IsIgnoreCooldown;
                        Sprint25Check("dretch-cloud-command", commandExact, new JObject {
                            ["owner"] = owner.UniqueId, ["command"] = command == null ? null : Sprint27AiCommand(command),
                            ["costs"] = costs, ["directNativeCastCommands"] = 1, ["forcedRolls"] = 0 },
                            "One directly issued native Stinking Cloud command pays its standard action and ends normally; no AI claim.");
                        if (!commandExact) throw new InvalidOperationException("Dretch cloud command did not resolve normally.");
                    }
                    for (int frame = 0; frame < 120 && Sprint29OwnedAreas(owner, areaBlueprint).Length == 0; frame++) yield return 0;
                    areas.AddRange(Sprint29OwnedAreas(owner, areaBlueprint));
                    if (areas.Count != 1) throw new InvalidOperationException("Dretch did not create exactly one attributed cloud area.");
                    var area = areas[0];
                    DateTime deadline = DateTime.UtcNow.AddSeconds(6);
                    int frames = 0;
                    while (DateTime.UtcNow < deadline && frames++ < 600 &&
                        (!target.Descriptor.State.HasCondition(UnitCondition.Nauseated) || !ally.Descriptor.State.HasCondition(UnitCondition.Nauseated)))
                        yield return 0;
                    bool effect = area.Context.Params.CasterLevel == 2 && area.Context.Params.DC == 13 &&
                        Sprint29AreaDuration(area) > 0 && Sprint29AreaDuration(area) <= 12.1 &&
                        area.UnitsInside.Contains(target) && area.UnitsInside.Contains(ally) && area.UnitsInside.Contains(immune) && area.UnitsInside.Contains(owner) &&
                        target.Descriptor.State.HasCondition(UnitCondition.Nauseated) && ally.Descriptor.State.HasCondition(UnitCondition.Nauseated) &&
                        !owner.Descriptor.State.HasCondition(UnitCondition.Nauseated) && !immune.Descriptor.State.HasCondition(UnitCondition.Nauseated) &&
                        saves.Rows.Any(value => (string)value["target"] == target.UniqueId && !(bool)value["passed"] && (int)value["dc"] == 13 && (string)value["caster"] == owner.UniqueId && (string)value["sourceAbility"] == cloud.AssetGuid) &&
                        saves.Rows.Any(value => (string)value["target"] == ally.UniqueId && !(bool)value["passed"] && (int)value["dc"] == 13 && (string)value["caster"] == owner.UniqueId && (string)value["sourceAbility"] == cloud.AssetGuid);
                    Sprint25Check("dretch-cloud-all-creature-effect", effect, new JObject {
                        ["owner"] = owner.UniqueId, ["area"] = area.UniqueId, ["blueprint"] = area.Blueprint.AssetGuid,
                        ["cl"] = area.Context.Params.CasterLevel, ["dc"] = area.Context.Params.DC,
                        ["durationSeconds"] = Sprint29AreaDuration(area), ["inside"] = new JArray(area.UnitsInside.Select(value => value.UniqueId)),
                        ["hostileNauseated"] = target.Descriptor.State.HasCondition(UnitCondition.Nauseated),
                        ["allyNauseated"] = ally.Descriptor.State.HasCondition(UnitCondition.Nauseated),
                        ["ownerNauseated"] = owner.Descriptor.State.HasCondition(UnitCondition.Nauseated),
                        ["immuneNauseated"] = immune.Descriptor.State.HasCondition(UnitCondition.Nauseated), ["saves"] = saves.Rows,
                        ["fixtureTargetFortBase"] = -100, ["fixtureAllyFortBase"] = -100, ["forcedRolls"] = 0, ["explicitAreaTicks"] = 0 },
                        "The unchanged native poison cloud uses real saves and nauseates an eligible ally as well as the foe; poison-immune actors stay eligible for immunity.");
                    bool resources = owner.Descriptor.Resources.GetResourceAmount(cloudResource) == 0 &&
                        owner.Descriptor.Resources.GetResourceAmount(fearResource) == 1 &&
                        sibling.Descriptor.Resources.GetResourceAmount(cloudResource) == 1 &&
                        sibling.Descriptor.Resources.GetResourceAmount(fearResource) == 1 && !cloudData.IsAvailable;
                    Sprint25Check("dretch-cloud-independent-resource", resources, new JObject {
                        ["owner"] = owner.UniqueId, ["sibling"] = sibling.UniqueId,
                        ["ownerCloud"] = owner.Descriptor.Resources.GetResourceAmount(cloudResource),
                        ["ownerFear"] = owner.Descriptor.Resources.GetResourceAmount(fearResource),
                        ["siblingCloud"] = sibling.Descriptor.Resources.GetResourceAmount(cloudResource),
                        ["siblingFear"] = sibling.Descriptor.Resources.GetResourceAmount(fearResource), ["cloudAvailable"] = cloudData.IsAvailable },
                        "One cast exhausts only this owner's cloud resource; Cause Fear and the sibling's resources remain independent.");
                    DisposeExpandedSummoningUnits(fixture.Created, new[] { owner });
                    bool remainedAfterOwner = Game.Instance.State.AreaEffects.All.Contains(area);
                    DateTime expiry = DateTime.UtcNow.AddSeconds(20); int expiryFrames = 0;
                    while (Game.Instance.State.AreaEffects.All.Contains(area) && DateTime.UtcNow < expiry && expiryFrames++ < 2000) yield return 0;
                    bool ended = !Game.Instance.State.AreaEffects.All.Contains(area);
                    Sprint25Check("dretch-cloud-observed-lifetime", ended && owner.Destroyed, new JObject {
                        ["owner"] = owner.UniqueId, ["ownerDestroyed"] = owner.Destroyed,
                        ["area"] = area.UniqueId, ["remainedImmediatelyAfterOwner"] = remainedAfterOwner,
                        ["naturallyEnded"] = ended, ["waitFrames"] = expiryFrames,
                        ["hostileResidualNausea"] = target.Descriptor.State.HasCondition(UnitCondition.Nauseated),
                        ["allyResidualNausea"] = ally.Descriptor.State.HasCondition(UnitCondition.Nauseated),
                        ["manualSourceLossRemoval"] = false },
                        "Record this native SLA area's actual owner-loss behavior and finite expiry; do not add blanket source-loss deletion.");
                }
                finally { EventBus.Unsubscribe(saves); }
            }
            finally
            {
                foreach (var area in areas.Where(value => value != null && Game.Instance.State.AreaEffects.All.Contains(value))) EndExpandedSummoningArea(area);
                foreach (var unit in new[] { target, ally, immune }.Where(value => value != null && !value.Destroyed))
                    foreach (var buff in unit.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(value =>
                        owner != null && ReferenceEquals(value.Context?.MaybeCaster, owner)).ToArray()) unit.Descriptor.Buffs.RemoveFact(buff);
                if (changedFort && target != null && !target.Destroyed) target.Stats.SaveFortitude.BaseValue = fort;
                DisposeExpandedSummoningUnits(fixture.Created, new[] { owner, sibling, ally, immune }.Where(value => value != null && !value.Destroyed).ToArray());
                ResetExpandedSummoningHostile(fixture);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode;
                Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = oldPause;
            }
        }
        private sealed class Dretch30CloudSaves : IGlobalRulebookHandler<RuleSavingThrow>, IGlobalRulebookHandler<RuleApplyBuff>
        {
            private readonly UnitEntityData[] _targets;
            internal readonly JArray Rows = new JArray();
            internal readonly JArray Applications = new JArray();
            // The grouped S32 control case correlates actual cast contexts.
            // Keep immutable rows above for existing evidence consumers.
            internal readonly List<RuleSavingThrow> NativeSaves = new List<RuleSavingThrow>();
            internal readonly List<RuleApplyBuff> NativeApplications = new List<RuleApplyBuff>();
            internal Dretch30CloudSaves(UnitEntityData[] targets) { _targets = targets; }
            public void OnEventAboutToTrigger(RuleSavingThrow evt) { }
            public void OnEventDidTrigger(RuleSavingThrow evt)
            {
                if (!_targets.Contains(evt.Initiator)) return;
                NativeSaves.Add(evt);
                Rows.Add(new JObject {
                    ["target"] = evt.Initiator.UniqueId, ["dc"] = evt.DifficultyClass, ["passed"] = evt.IsPassed,
                    ["sourceAbility"] = evt.Reason?.Context?.SourceAbility?.AssetGuid,
                    ["caster"] = evt.Reason?.Context?.MaybeCaster?.UniqueId });
            }
            public void OnEventAboutToTrigger(RuleApplyBuff evt) { }
            public void OnEventDidTrigger(RuleApplyBuff evt)
            {
                if (!_targets.Contains(evt.Initiator)) return;
                NativeApplications.Add(evt);
                Applications.Add(new JObject {
                    ["target"] = evt.Initiator.UniqueId, ["buff"] = evt.Blueprint?.AssetGuid,
                    ["sourceAbility"] = evt.Context?.SourceAbility?.AssetGuid,
                    ["caster"] = evt.Context?.MaybeCaster?.UniqueId,
                    ["durationSeconds"] = evt.Duration?.TotalSeconds,
                    ["applied"] = evt.AppliedBuff != null });
            }
        }
    }
}
