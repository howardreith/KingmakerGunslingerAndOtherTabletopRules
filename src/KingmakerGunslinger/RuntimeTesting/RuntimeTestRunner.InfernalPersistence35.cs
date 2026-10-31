using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.View;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private const string InfernalSavedScope35 =
            "KMG_Sprint35_InfernalReachII_v1";
        private Buff _infernalLoadedPoison35;
        private UnitEntityData _infernalLoadedWall35;

        private static T InfernalSavedBlueprint35<T>(bool bone,
            string suffix) where T : BlueprintScriptableObject
        {
            return (T)ExpandedSummoningBlueprints.Registered.BySymbol[
                (bone ? ExpandedSummoningSpecialBuilder.Bone35Prefix :
                    ExpandedSummoningSpecialBuilder.Barbed35Prefix) +
                suffix];
        }

        private static UnitEntityData[] InfernalSavedState35(
            UnitEntityData caster)
        {
            return Sprint32SavedAuxiliaries(caster).Where(unit =>
                unit.Get<UnitPartSprint32ListenerReceipt>()?.Scope ==
                    InfernalSavedScope35).ToArray();
        }

        private static bool OwnsInfernalState35(UnitEntityData target,
            UnitEntityData caster, UnitEntityData[] sources)
        {
            var receipt = target?.Get<UnitPartSprint32ListenerReceipt>();
            if (receipt == null || receipt.Scope != InfernalSavedScope35 ||
                receipt.UnitId != target.UniqueId ||
                receipt.CasterId != caster.UniqueId ||
                string.IsNullOrEmpty(receipt.PrepareRunId) ||
                !ReferenceEquals(target.HoldingState, caster.HoldingState))
                return false;
            UnitEntityData source = sources.FirstOrDefault(unit =>
                unit.UniqueId == receipt.SourceId &&
                unit.Get<UnitPartRelease143RosterReceipt>()?.Key ==
                    InfernalVisualPolicy35.Bone);
            if (source == null) return false;
            if (receipt.Role == "infernal35-poison")
                return ReferenceEquals(target.Blueprint,
                        BlueprintRoot.Instance.DefaultPlayerCharacter) &&
                    !Game.Instance.Player.Party.Contains(target);
            if (receipt.Role == "infernal35-wall")
                return target.Blueprint?.AssetGuid ==
                    InfernalRules35.WallSectionUnitGuid &&
                    ReferenceEquals(target.Get<UnitPartSummonedMonster>()?
                        .Summoner, source);
            return false;
        }

        private static JObject InfernalSavedResources35(
            UnitEntityData barbed, UnitEntityData bone)
        {
            var unholy = InfernalSavedBlueprint35<BlueprintAbilityResource>(
                false, "UnholyBlightResource");
            var quick = InfernalSavedBlueprint35<BlueprintAbilityResource>(
                true, "QuickenedInvisibilityResource");
            return new JObject {
                ["barbedUnholy"] = barbed.Descriptor.Resources
                    .GetResourceAmount(unholy),
                ["boneQuickenedInvisibility"] = bone.Descriptor.Resources
                    .GetResourceAmount(quick)
            };
        }

        private static JObject InfernalSavedWall35(UnitEntityData section)
        {
            var binding = section?.View == null ? null : section.View
                .GetComponent<InfernalWallNavmesh35>();
            return new JObject {
                ["present"] = section != null,
                ["unit"] = section?.UniqueId,
                ["blueprint"] = section?.Blueprint?.AssetGuid,
                ["source"] = section?.Get<UnitPartSummonedMonster>()?
                    .Summoner?.UniqueId,
                ["summonedLifetime"] = section?.Buffs?.GetBuff(
                    BlueprintRoot.Instance.SystemMechanics
                        .SummonedUnitBuff) != null,
                ["viewBinding"] = binding != null,
                ["gridNodesClaimed"] = binding?.GridNodesClaimed,
                ["activeGridClaims"] =
                    InfernalWallNavmesh35.ActiveGridClaimCount
            };
        }

        private IEnumerable<int> PrepareInfernalSaved35(
            UnitEntityData caster, UnitEntityData[] sources)
        {
            if (!Game.Instance.IsPaused ||
                InfernalSavedState35(caster).Length != 0)
                throw new InvalidOperationException(
                    "Expected paused empty Sprint35 persistence state.");
            UnitEntityData barbed = sources.Single(unit =>
                unit.Get<UnitPartRelease143RosterReceipt>()?.Key ==
                    InfernalVisualPolicy35.Barbed);
            UnitEntityData bone = sources.Single(unit =>
                unit.Get<UnitPartRelease143RosterReceipt>()?.Key ==
                    InfernalVisualPolicy35.Bone);
            UnitEntityData[] party = Game.Instance.Player.Party.ToArray();
            UnitEntityData[] intake = Game.Instance.State.Units.All.Where(
                unit => unit != null && !sources.Contains(unit) &&
                    !party.Contains(unit)).ToArray();
            BlueprintFaction faction = SelectSprint32ListenerFaction(caster,
                sources, party, intake);
            UnitEntityData target = Game.Instance.EntityCreator.SpawnUnit(
                BlueprintRoot.Instance.DefaultPlayerCharacter,
                caster.Position, Quaternion.identity, caster.HoldingState);
            Game.Instance.EntityCreator.Tick();
            var receipt = target.Ensure<UnitPartSprint32ListenerReceipt>();
            receipt.Scope = InfernalSavedScope35;
            receipt.UnitId = target.UniqueId;
            receipt.CasterId = caster.UniqueId;
            receipt.SourceId = bone.UniqueId;
            receipt.Role = "infernal35-poison";
            receipt.PrepareRunId = _request.RunId;
            receipt.ListenerFaction = faction.AssetGuid;
            SetExpandedSummoningBrainActive(target, false);
            target.GroupId = target.UniqueId;
            target.Descriptor.SwitchFactions(faction, true);
            target.AttackFactions.Match(Array.Empty<BlueprintFaction>());
            target.Stats.SaveFortitude.BaseValue = -100;
            target.Stats.Strength.BaseValue = 30;
            target.Group.ResetFactionSet();
            target.Group.UpdateAttackFactionsCache();
            if (!OwnsInfernalState35(target, caster, sources) ||
                target.Group.IsPlayerParty || target.Group.Count != 1 ||
                intake.Any(unit => target.IsEnemy(unit) ||
                    unit.IsEnemy(target)))
                throw new InvalidOperationException(
                    "Sprint35 poison recipient is not privately owned.");

            BlueprintBuff poison = InfernalSavedBlueprint35<BlueprintBuff>(
                true, "Poison");
            MechanicsContext poisonContext = AstralVictimEffectContext.Create(
                bone, target, poison, new AbilityParams {
                    DC = InfernalRules35.BonePoisonDc,
                    CasterLevel = InfernalRules35.SpellLikeCasterLevel,
                    SpellLevel = 0
                });
            var apply = new RuleApplyBuff(target, poison, poisonContext, null,
                (blueprint, parent, duration) => target.Buffs.AddBuff(
                    blueprint, parent, duration));
            Rulebook.Trigger(apply);
            if (apply.AppliedBuff == null || !apply.AppliedBuff.Active ||
                apply.AppliedBuff.IsDisposed)
                throw new InvalidOperationException(
                    "Sprint35 poison save precondition refused.");

            var unholy = InfernalSavedBlueprint35<BlueprintAbilityResource>(
                false, "UnholyBlightResource");
            var quick = InfernalSavedBlueprint35<BlueprintAbilityResource>(
                true, "QuickenedInvisibilityResource");
            int unholyBefore = barbed.Descriptor.Resources.GetResourceAmount(
                unholy);
            int quickBefore = bone.Descriptor.Resources.GetResourceAmount(
                quick);
            if (unholyBefore != 1 || quickBefore != 3)
                throw new InvalidOperationException(
                    "Sprint35 per-summon resource intake differs.");
            barbed.Descriptor.Resources.Spend(unholy, unholyBefore);
            bone.Descriptor.Resources.Spend(quick, quickBefore);

            UnitEntityData wall = null;
            foreach (int frame in PrepareInfernalWall35(caster, bone,
                sources, value => wall = value)) yield return frame;
            if (wall == null) throw new InvalidOperationException(
                "Sprint35 persistence wall did not form.");
            var wallReceipt = wall.Ensure<UnitPartSprint32ListenerReceipt>();
            wallReceipt.Scope = InfernalSavedScope35;
            wallReceipt.UnitId = wall.UniqueId;
            wallReceipt.CasterId = caster.UniqueId;
            wallReceipt.SourceId = bone.UniqueId;
            wallReceipt.Role = "infernal35-wall";
            wallReceipt.PrepareRunId = _request.RunId;
            wallReceipt.ListenerFaction = bone.Faction.AssetGuid;

            receipt.ControlState = new JObject {
                ["poison"] = InfernalBonePoison35(apply.AppliedBuff),
                ["resources"] = InfernalSavedResources35(barbed, bone),
                ["wall"] = InfernalSavedWall35(wall),
                ["barbed"] = barbed.UniqueId,
                ["bone"] = bone.UniqueId
            }.ToString(Formatting.None);
            ObserveInfernalSaved35(caster, sources, "prepared");
        }

        private IEnumerable<int> PrepareInfernalWall35(
            UnitEntityData caster, UnitEntityData bone,
            UnitEntityData[] sources, Action<UnitEntityData> assign)
        {
            BlueprintAbility wallBlueprint =
                InfernalSavedBlueprint35<BlueprintAbility>(true,
                    "WallOfIce");
            BlueprintUnit sectionBlueprint =
                InfernalSavedBlueprint35<BlueprintUnit>(true,
                    "WallSectionUnit");
            AbilityData data = new AbilityData(
                bone.Descriptor.Abilities.GetAbility(wallBlueprint));
            Vector3[] directions = {
                Vector3.forward, Vector3.right, Vector3.back, Vector3.left,
                (Vector3.forward + Vector3.right).normalized,
                (Vector3.forward + Vector3.left).normalized,
                (Vector3.back + Vector3.right).normalized,
                (Vector3.back + Vector3.left).normalized
            };
            TargetWrapper selected = null;
            foreach (float distance in new[] { 5f, 7f, 9f, 11f })
            foreach (Vector3 direction in directions)
            {
                Vector3 point = ObstacleAnalyzer.GetNearestNode(
                    bone.Position + direction * distance).clampedPosition;
                if (Game.Instance.State.Units.All.Any(unit => unit != null &&
                        !unit.Destroyed && !sources.Contains(unit) &&
                        !ReferenceEquals(unit, caster) &&
                        Vector3.Distance(unit.Position, point) < 2.2f))
                    continue;
                var target = new TargetWrapper(point);
                if (data.IsAvailable && data.CanTarget(target)) {
                    selected = target;
                    break;
                }
            }
            if (selected == null) throw new InvalidOperationException(
                "No legal bounded Sprint35 persistence wall point.");
            var prior = new HashSet<string>(Game.Instance.State.Units.All
                .Where(unit => ReferenceEquals(unit.Blueprint,
                    sectionBlueprint)).Select(unit => unit.UniqueId));
            var command = new UnitUseAbility(data, selected);
            bone.Commands.Run(command);
            DateTime deadline = DateTime.UtcNow.AddSeconds(30);
            UnitEntityData section = null;
            while (DateTime.UtcNow < deadline &&
                (!command.IsFinished || section == null))
            {
                Game.Instance.IsPaused = false;
                yield return 0;
                section = Game.Instance.State.Units.All.FirstOrDefault(unit =>
                    ReferenceEquals(unit.Blueprint, sectionBlueprint) &&
                    !prior.Contains(unit.UniqueId));
            }
            Game.Instance.IsPaused = true;
            if (command.Result != UnitCommand.ResultType.Success ||
                section == null)
                throw new InvalidOperationException(
                    "Native Sprint35 persistence wall command failed: " +
                    command.Result);
            DateTime claimsDeadline = DateTime.UtcNow.AddSeconds(15);
            while (DateTime.UtcNow < claimsDeadline &&
                (section.View?.GetComponent<InfernalWallNavmesh35>()?
                    .GridNodesClaimed ?? 0) == 0)
            {
                Game.Instance.IsPaused = false;
                yield return 0;
            }
            Game.Instance.IsPaused = true;
            assign(section);
        }

        private void ObserveInfernalSaved35(UnitEntityData caster,
            UnitEntityData[] sources, string phase)
        {
            UnitEntityData barbed = sources.Single(unit =>
                unit.Get<UnitPartRelease143RosterReceipt>()?.Key ==
                    InfernalVisualPolicy35.Barbed);
            UnitEntityData bone = sources.Single(unit =>
                unit.Get<UnitPartRelease143RosterReceipt>()?.Key ==
                    InfernalVisualPolicy35.Bone);
            UnitEntityData[] owned = InfernalSavedState35(caster);
            UnitEntityData target = owned.SingleOrDefault(unit =>
                unit.Get<UnitPartSprint32ListenerReceipt>()?.Role ==
                    "infernal35-poison");
            UnitEntityData wall = owned.SingleOrDefault(unit =>
                unit.Get<UnitPartSprint32ListenerReceipt>()?.Role ==
                    "infernal35-wall");
            if (target == null || !OwnsInfernalState35(target, caster,
                    sources))
                throw new InvalidOperationException(
                    "Exact saved Sprint35 poison recipient absent.");
            JObject saved = JObject.Parse(target.Get<
                UnitPartSprint32ListenerReceipt>().ControlState);
            BlueprintBuff poisonBlueprint =
                InfernalSavedBlueprint35<BlueprintBuff>(true, "Poison");
            Buff[] poisons = target.Buffs.Enumerable.Where(buff =>
                ReferenceEquals(buff.Blueprint, poisonBlueprint)).ToArray();
            Buff poison = poisons.SingleOrDefault();
            JObject resources = InfernalSavedResources35(barbed, bone);
            bool enabled = _context.FeatureModules.Active.ExpandedSummoning;
            bool poisonExact = poison != null && poison.Active &&
                !poison.IsDisposed && poison.Context?.Params?.DC ==
                    InfernalRules35.BonePoisonDc &&
                CircleOriginalCasterId(poison.Context) == bone.UniqueId &&
                poison.SelectComponents<BuffPoisonStatDamage>().Single()
                    .Ticks == 6 &&
                poison.SelectComponents<BuffPoisonStatDamage>().Single()
                    .SuccesfullSaves == 2 &&
                poison.SelectComponents<BuffPoisonStatDamage>().Single()
                    .Stat == StatType.Strength;
            bool resourceExact = (int?)resources["barbedUnholy"] == 0 &&
                (int?)resources["boneQuickenedInvisibility"] == 0;
            bool wallExact = enabled
                ? wall != null && OwnsInfernalState35(wall, caster,
                    sources) &&
                    wall.View?.GetComponent<InfernalWallNavmesh35>() != null &&
                    wall.View.GetComponent<InfernalWallNavmesh35>()
                        .GridNodesClaimed > 0 &&
                    InfernalWallNavmesh35.ActiveGridClaimCount > 0
                : wall == null &&
                    InfernalWallNavmesh35.ActiveGridClaimCount == 0;
            bool exact = poisons.Length == 1 && poisonExact &&
                resourceExact && wallExact;
            _infernalLoadedPoison35 = poison;
            _infernalLoadedWall35 = wall;
            RosterPersistenceCheck("s35-state-" + phase, exact,
                new JObject {
                    ["recipient"] = target.UniqueId,
                    ["moduleEnabled"] = enabled,
                    ["saved"] = saved,
                    ["poison"] = InfernalBonePoison35(poison),
                    ["resources"] = resources,
                    ["wall"] = InfernalSavedWall35(wall),
                    ["poisonSourceId"] = poison == null ? null :
                        CircleOriginalCasterId(poison.Context),
                    ["effectOnlyPoisonPrecondition"] = true,
                    ["wallUsedNativeCommand"] = true
                }, "Cold load preserves the independently timed DC20 poison and exact spent resources; ON recreates the serialized wall obstruction, while OFF retires it with zero navigation claims.");
            if (!exact) throw new InvalidOperationException(
                "Sprint35 saved poison, wall, or resource state differs.");
        }

        private IEnumerable<int> CleanupInfernalSaved35(
            UnitEntityData caster)
        {
            UnitEntityData[] owned = InfernalSavedState35(caster);
            UnitEntityData target = owned.SingleOrDefault(unit =>
                unit.Get<UnitPartSprint32ListenerReceipt>()?.Role ==
                    "infernal35-poison");
            if (target == null) throw new InvalidOperationException(
                "Sprint35 cleanup recipient ambiguous.");
            string sourceId = target.Get<UnitPartSprint32ListenerReceipt>()
                .SourceId;
            DateTime deadline = DateTime.UtcNow.AddSeconds(15);
            while (DateTime.UtcNow < deadline &&
                ((_infernalLoadedWall35 != null &&
                    !_infernalLoadedWall35.Destroyed) ||
                 InfernalWallNavmesh35.ActiveGridClaimCount != 0))
            {
                Game.Instance.IsPaused = false;
                Game.Instance.EntityDestroyer.Tick();
                yield return 0;
            }
            Game.Instance.IsPaused = true;
            BlueprintBuff poisonBlueprint =
                InfernalSavedBlueprint35<BlueprintBuff>(true, "Poison");
            Buff poison = target.Buffs.Enumerable.SingleOrDefault(buff =>
                ReferenceEquals(buff.Blueprint, poisonBlueprint));
            bool poisonIndependent = poison != null && poison.Active &&
                !poison.IsDisposed && poison.Context?.MaybeCaster == null &&
                CircleOriginalCasterId(poison.Context) == sourceId;
            bool wallRetired = (_infernalLoadedWall35 == null ||
                _infernalLoadedWall35.Destroyed ||
                !_infernalLoadedWall35.IsInState) &&
                InfernalWallNavmesh35.ActiveGridClaimCount == 0;
            RosterPersistenceCheck("s35-source-cleanup",
                poisonIndependent && wallRetired,
                new JObject {
                    ["poisonPresent"] = poison != null,
                    ["poisonLiveCaster"] = poison?.Context?.MaybeCaster?
                        .UniqueId,
                    ["poisonSerializedCaster"] = poison == null ? null :
                        CircleOriginalCasterId(poison.Context),
                    ["expectedSource"] = sourceId,
                    ["wallAbsent"] = wallRetired,
                    ["activeGridClaims"] =
                        InfernalWallNavmesh35.ActiveGridClaimCount
                }, "Source retirement removes the source-owned wall and navigation but does not erase the independently timed poison or already applied ability damage.");
            if (!poisonIndependent || !wallRetired)
                throw new InvalidOperationException(
                    "Sprint35 source cleanup lifetime distinction failed.");
            poison.Remove();
            CleanupExpandedSummoningUnit(target);
            for (int frame = 0; frame < 4; frame++) {
                Game.Instance.EntityDestroyer.Tick();
                yield return 0;
            }
            _infernalLoadedPoison35 = null;
            _infernalLoadedWall35 = null;
            InfernalSavedAbsence35(caster, "cleanup");
        }

        private void InfernalSavedAbsence35(UnitEntityData caster,
            string phase)
        {
            BlueprintBuff poison =
                InfernalSavedBlueprint35<BlueprintBuff>(true, "Poison");
            int recipients = InfernalSavedState35(caster).Length;
            int poisons = Game.Instance.State.Units.All.Where(unit =>
                unit != null).Sum(unit => unit.Buffs.Enumerable.Count(buff =>
                    ReferenceEquals(buff.Blueprint, poison)));
            int walls = Game.Instance.State.Units.All.Count(unit =>
                unit != null && !unit.Destroyed && unit.Blueprint?.AssetGuid ==
                    InfernalRules35.WallSectionUnitGuid);
            bool exact = recipients == 0 && poisons == 0 && walls == 0 &&
                InfernalWallNavmesh35.ActiveGridClaimCount == 0;
            RosterPersistenceCheck("s35-state-" + phase + "-absence",
                exact, new JObject {
                    ["recipients"] = recipients,
                    ["poisons"] = poisons,
                    ["walls"] = walls,
                    ["activeGridClaims"] =
                        InfernalWallNavmesh35.ActiveGridClaimCount
                }, "No Sprint35 fixture recipient, poison, wall, or navigation claim remains after the cleanup save.");
            if (!exact) throw new InvalidOperationException(
                "Sprint35 saved state remains.");
        }

        private void ObserveInfernalItems35(UnitEntityData[] units,
            ItemEntity[] partyItems, int[] partyCounts)
        {
            ItemEntity[] all = units.SelectMany(unit =>
                unit.Descriptor.Inventory.Items).ToArray();
            bool exact = all.Length != 0 &&
                all.Distinct().Count() == all.Length &&
                Sprint28PartyInventoryExact(partyItems, partyCounts);
            foreach (UnitEntityData unit in units)
            {
                var body = unit.Blueprint.Body;
                string[] expected = new BlueprintItem[] {
                    body.PrimaryHand, body.SecondaryHand,
                    body.PrimaryHandAlternative1,
                    body.SecondaryHandAlternative1,
                    body.PrimaryHandAlternative2,
                    body.SecondaryHandAlternative2,
                    body.PrimaryHandAlternative3,
                    body.SecondaryHandAlternative3
                }.Where(value => value != null)
                    .Concat(body.AdditionalLimbs ?? Array.Empty<
                        BlueprintItemWeapon>())
                    .Concat(body.AdditionalSecondaryLimbs ?? Array.Empty<
                        BlueprintItemWeapon>())
                    .Select(value => value.AssetGuid)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray();
                ItemEntity[] items = unit.Descriptor.Inventory.Items
                    .ToArray();
                exact &= items.Select(item => item.Blueprint.AssetGuid)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .SequenceEqual(expected) && items.All(item =>
                        ReferenceEquals(item.Owner, unit.Descriptor) &&
                        item.IsNonRemovable && !item.IsLootable &&
                        item.Count == 1);
            }
            RosterPersistenceCheck("s35-items-owned", exact,
                new JObject {
                    ["units"] = new JArray(units.Select(unit =>
                        new JObject {
                            ["unit"] = unit.UniqueId,
                            ["key"] = unit.Get<
                                UnitPartRelease143RosterReceipt>()?.Key,
                            ["items"] = InfernalSavedItems34(unit)
                        })),
                    ["instances"] = new JArray(all.Select(
                        RuntimeHelpers.GetHashCode)),
                    ["partyInventoryExact"] =
                        Sprint28PartyInventoryExact(partyItems, partyCounts)
                }, "Both saved Sprint35 owners retain exact separate nontransferable limbs and leave party inventory unchanged.");
            if (!exact) throw new InvalidOperationException(
                "Sprint35 saved item ownership differs.");
        }
    }
}
