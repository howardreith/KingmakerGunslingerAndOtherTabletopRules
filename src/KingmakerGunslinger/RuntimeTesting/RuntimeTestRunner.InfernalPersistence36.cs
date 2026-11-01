using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private const string InfernalSavedScope36 =
            "KMG_Sprint36_InfernalDemons_v1";
        private Buff _infernalLoadedNausea36;

        private static T InfernalSavedBlueprint36<T>(string suffix)
            where T : BlueprintScriptableObject
        {
            return (T)ExpandedSummoningBlueprints.Registered.BySymbol[
                ExpandedSummoningSpecialBuilder.Hezrou36Prefix + suffix];
        }

        private static UnitEntityData[] InfernalSavedState36(
            UnitEntityData caster)
        {
            return Sprint32SavedAuxiliaries(caster).Where(unit =>
                unit.Get<UnitPartSprint32ListenerReceipt>()?.Scope ==
                    InfernalSavedScope36).ToArray();
        }

        private static bool OwnsInfernalState36(UnitEntityData target,
            UnitEntityData caster, UnitEntityData[] sources)
        {
            var receipt = target?.Get<UnitPartSprint32ListenerReceipt>();
            if (receipt == null || receipt.Scope != InfernalSavedScope36 ||
                receipt.Role != "infernal36-held" ||
                receipt.UnitId != target.UniqueId ||
                receipt.CasterId != caster.UniqueId ||
                string.IsNullOrEmpty(receipt.PrepareRunId) ||
                !ReferenceEquals(target.Blueprint,
                    BlueprintRoot.Instance.DefaultPlayerCharacter) ||
                !ReferenceEquals(target.HoldingState, caster.HoldingState) ||
                Game.Instance.Player.Party.Contains(target))
                return false;
            return sources.Any(source => source.UniqueId == receipt.SourceId &&
                source.Get<UnitPartRelease143RosterReceipt>()?.Key ==
                    InfernalVisualPolicy36.Hezrou);
        }

        private static BlueprintAbilityResource InfernalBlasphemyResource36()
        {
            return InfernalSavedBlueprint36<BlueprintAbilityResource>(
                "BlasphemyResource");
        }

        private static BlueprintBuff InfernalHeldNausea36()
        {
            return InfernalSavedBlueprint36<BlueprintBuff>(
                "HeldNauseated");
        }

        private IEnumerable<int> PrepareInfernalSaved36(
            UnitEntityData caster, UnitEntityData[] sources)
        {
            if (!Game.Instance.IsPaused ||
                InfernalSavedState36(caster).Length != 0)
                throw new InvalidOperationException(
                    "Expected paused empty Sprint36 persistence state.");
            UnitEntityData babau = sources.Single(unit =>
                unit.Get<UnitPartRelease143RosterReceipt>()?.Key ==
                    InfernalVisualPolicy36.Babau);
            UnitEntityData hezrou = sources.Single(unit =>
                unit.Get<UnitPartRelease143RosterReceipt>()?.Key ==
                    InfernalVisualPolicy36.Hezrou);
            UnitEntityData[] party = Game.Instance.Player.Party.ToArray();
            UnitEntityData[] intake = Game.Instance.State.Units.All.Where(
                unit => unit != null && !sources.Contains(unit) &&
                    !party.Contains(unit)).ToArray();
            BlueprintFaction faction = SelectSprint32ListenerFaction(caster,
                sources, party, intake);
            UnitEntityData target = Game.Instance.EntityCreator.SpawnUnit(
                BlueprintRoot.Instance.DefaultPlayerCharacter,
                hezrou.Position + Vector3.forward * 1.5f,
                Quaternion.identity, caster.HoldingState);
            Game.Instance.EntityCreator.Tick();
            var receipt = target.Ensure<UnitPartSprint32ListenerReceipt>();
            receipt.Scope = InfernalSavedScope36;
            receipt.UnitId = target.UniqueId;
            receipt.CasterId = caster.UniqueId;
            receipt.SourceId = hezrou.UniqueId;
            receipt.Role = "infernal36-held";
            receipt.PrepareRunId = _request.RunId;
            receipt.ListenerFaction = faction.AssetGuid;
            SetExpandedSummoningBrainActive(target, false);
            target.GroupId = target.UniqueId;
            target.Descriptor.SwitchFactions(faction, true);
            target.AttackFactions.Match(Array.Empty<BlueprintFaction>());
            target.Stats.SaveFortitude.BaseValue = -100;
            target.Descriptor.State.Size = Size.Medium;
            target.Stats.HitPoints.BaseValue = 100000;
            target.Group.ResetFactionSet();
            target.Group.UpdateAttackFactionsCache();
            if (!OwnsInfernalState36(target, caster, sources) ||
                target.Group.IsPlayerParty || target.Group.Count != 1 ||
                intake.Any(unit => target.IsEnemy(unit) ||
                    unit.IsEnemy(target)))
                throw new InvalidOperationException(
                    "Sprint36 held recipient is not privately owned.");

            SummonGrabComponent grab = SummonGrabComponent.Find(hezrou);
            bool held = false;
            int attempts = 0;
            while (grab != null && !held && attempts < 3)
            {
                attempts++;
                held = grab.TryGrab(target,
                    hezrou.Body.PrimaryHand.MaybeWeapon, true);
            }
            if (!held) throw new InvalidOperationException(
                "Sprint36 saved hold precondition remained unobserved after three bounded native checks.");
            HezrouHeldNausea36 heldLogic = hezrou.Descriptor.Progression
                .Features.Enumerable.SelectMany(value =>
                    value.SelectComponents<HezrouHeldNausea36>()).Single();
            heldLogic.OnNewRound();
            BlueprintBuff nauseaBlueprint = InfernalHeldNausea36();
            Buff nausea = InfernalCoreSourceBuff36(target,
                nauseaBlueprint, hezrou);
            if (nausea == null || nausea.IsDisposed || !nausea.Active)
                throw new InvalidOperationException(
                    "Sprint36 held nausea precondition refused.");

            BlueprintAbilityResource blasphemy =
                InfernalBlasphemyResource36();
            int resourceBefore = hezrou.Descriptor.Resources
                .GetResourceAmount(blasphemy);
            if (resourceBefore != 1)
                throw new InvalidOperationException(
                    "Sprint36 Blasphemy resource intake differs.");
            hezrou.Descriptor.Resources.Spend(blasphemy, 1);
            receipt.ControlState = new JObject {
                ["babau"] = babau.UniqueId,
                ["hezrou"] = hezrou.UniqueId,
                ["target"] = target.UniqueId,
                ["holder"] = SummonHeldComponent.HolderOf(target,
                    grab.GrappledBuff)?.UniqueId,
                ["grabAttempts"] = attempts,
                ["blasphemy"] = hezrou.Descriptor.Resources
                    .GetResourceAmount(blasphemy),
                ["nausea"] = InfernalSavedEffect34(nausea)
            }.ToString(Newtonsoft.Json.Formatting.None);
            ObserveInfernalSaved36(caster, sources, "prepared");
            yield break;
        }

        private void ObserveInfernalSaved36(UnitEntityData caster,
            UnitEntityData[] sources, string phase)
        {
            UnitEntityData hezrou = sources.Single(unit =>
                unit.Get<UnitPartRelease143RosterReceipt>()?.Key ==
                    InfernalVisualPolicy36.Hezrou);
            UnitEntityData[] owned = InfernalSavedState36(caster);
            UnitEntityData target = owned.SingleOrDefault(unit =>
                unit.Get<UnitPartSprint32ListenerReceipt>()?.Role ==
                    "infernal36-held");
            if (target == null || !OwnsInfernalState36(target, caster,
                    sources))
                throw new InvalidOperationException(
                    "Exact saved Sprint36 held recipient absent.");
            var receipt = target.Get<UnitPartSprint32ListenerReceipt>();
            BlueprintBuff nauseaBlueprint = InfernalHeldNausea36();
            Buff[] nausea = target.Buffs.Enumerable.Where(buff =>
                ReferenceEquals(buff.Blueprint, nauseaBlueprint)).ToArray();
            Buff effect = nausea.SingleOrDefault();
            SummonGrabComponent grab = SummonGrabComponent.Find(hezrou);
            UnitEntityData holder = grab?.GrappledBuff == null ? null :
                SummonHeldComponent.HolderOf(target, grab.GrappledBuff);
            double recovery = effect == null ? -1 :
                effect.TimeLeft.TotalSeconds;
            bool prepared = phase == "prepared";
            bool sourceExact = effect != null && effect.Active &&
                !effect.IsDisposed &&
                CircleOriginalCasterId(effect.Context) == hezrou.UniqueId;
            bool holdExact = prepared ? ReferenceEquals(holder, hezrou) :
                holder == null;
            int blasphemy = hezrou.Descriptor.Resources.GetResourceAmount(
                InfernalBlasphemyResource36());
            bool exact = nausea.Length == 1 && sourceExact && holdExact &&
                recovery > 0 && recovery <= 61 && blasphemy == 0;
            _infernalLoadedNausea36 = effect;
            RosterPersistenceCheck("s36-state-" + phase, exact,
                new JObject {
                    ["recipient"] = target.UniqueId,
                    ["moduleEnabled"] =
                        _context.FeatureModules.Active.ExpandedSummoning,
                    ["saved"] = JObject.Parse(receipt.ControlState),
                    ["nausea"] = effect == null ? null :
                        InfernalSavedEffect34(effect),
                    ["serializedSource"] = effect == null ? null :
                        CircleOriginalCasterId(effect.Context),
                    ["holder"] = holder?.UniqueId,
                    ["expectedPreparedHold"] = prepared,
                    ["recoverySeconds"] = recovery,
                    ["blasphemy"] = blasphemy
                }, prepared
                    ? "The prepared mixed fixture owns a real Hezrou hold, exact source nausea and one spent Blasphemy resource."
                    : "Fresh load preserves the spent finite resource and independently timed source nausea while the accepted summon-grapple contract safely resets the active hold.");
            if (!exact) throw new InvalidOperationException(
                "Sprint36 saved hold, nausea, or resource state differs.");
        }

        private IEnumerable<int> CleanupInfernalSaved36(
            UnitEntityData caster)
        {
            UnitEntityData[] owned = InfernalSavedState36(caster);
            UnitEntityData target = owned.SingleOrDefault(unit =>
                unit.Get<UnitPartSprint32ListenerReceipt>()?.Role ==
                    "infernal36-held");
            if (target == null) throw new InvalidOperationException(
                "Sprint36 cleanup recipient ambiguous.");
            string sourceId = target.Get<UnitPartSprint32ListenerReceipt>()
                .SourceId;
            BlueprintBuff nauseaBlueprint = InfernalHeldNausea36();
            Buff nausea = target.Buffs.Enumerable.SingleOrDefault(buff =>
                ReferenceEquals(buff.Blueprint, nauseaBlueprint));
            bool independent = nausea != null && nausea.Active &&
                !nausea.IsDisposed && nausea.Context?.MaybeCaster == null &&
                CircleOriginalCasterId(nausea.Context) == sourceId &&
                nausea.TimeLeft > TimeSpan.Zero &&
                nausea.TimeLeft.TotalSeconds <= 61;
            RosterPersistenceCheck("s36-source-cleanup", independent,
                new JObject {
                    ["nauseaPresent"] = nausea != null,
                    ["liveCaster"] = nausea?.Context?.MaybeCaster?.UniqueId,
                    ["serializedCaster"] = nausea == null ? null :
                        CircleOriginalCasterId(nausea.Context),
                    ["expectedSource"] = sourceId,
                    ["recoverySeconds"] = nausea?.TimeLeft.TotalSeconds,
                    ["loadedInstanceSame"] = ReferenceEquals(nausea,
                        _infernalLoadedNausea36)
                }, "Source retirement cannot restore the accepted active grapple, but it preserves the already delivered one-minute nausea without erasing another source's effects.");
            if (!independent) throw new InvalidOperationException(
                "Sprint36 source cleanup lifetime distinction failed.");
            nausea.Remove();
            CleanupExpandedSummoningUnit(target);
            for (int frame = 0; frame < 4; frame++)
            {
                Game.Instance.EntityDestroyer.Tick();
                yield return 0;
            }
            _infernalLoadedNausea36 = null;
            InfernalSavedAbsence36(caster, "cleanup");
        }

        private void InfernalSavedAbsence36(UnitEntityData caster,
            string phase)
        {
            BlueprintBuff nausea = InfernalHeldNausea36();
            int recipients = InfernalSavedState36(caster).Length;
            int effects = Game.Instance.State.Units.All.Where(unit =>
                unit != null).Sum(unit => unit.Buffs.Enumerable.Count(buff =>
                    ReferenceEquals(buff.Blueprint, nausea)));
            int holders = Game.Instance.State.Units.All.Where(unit =>
                unit != null && !unit.Destroyed &&
                ExpandedSummoningRosterPersistencePolicy.InfernalDemonKeys
                    .Contains(unit.Get<UnitPartRelease143RosterReceipt>()?.Key,
                        StringComparer.Ordinal)).Count(unit =>
                    unit.Get<UnitPartSummonGrappleLinks>()?.Count > 0);
            bool exact = recipients == 0 && effects == 0 && holders == 0;
            RosterPersistenceCheck("s36-state-" + phase + "-absence",
                exact, new JObject {
                    ["recipients"] = recipients,
                    ["nauseaEffects"] = effects,
                    ["activeGrappleLedgers"] = holders
                }, "No Sprint36 fixture recipient, held nausea, or active summon-grapple ledger remains after the cleanup save.");
            if (!exact) throw new InvalidOperationException(
                "Sprint36 saved state remains.");
        }

        private void ObserveInfernalItems36(UnitEntityData[] units,
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
                    .Concat(body.AdditionalLimbs ??
                        Array.Empty<BlueprintItemWeapon>())
                    .Concat(body.AdditionalSecondaryLimbs ??
                        Array.Empty<BlueprintItemWeapon>())
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
            RosterPersistenceCheck("s36-items-owned", exact,
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
                }, "Babau and Hezrou retain exact separate nontransferable natural/manufactured equipment and leave protected party inventory unchanged.");
            if (!exact) throw new InvalidOperationException(
                "Sprint36 saved item ownership differs.");
        }
    }
}
