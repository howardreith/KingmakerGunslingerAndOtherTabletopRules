using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Blueprints;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private const string Sprint33ControlScope = "KMG_Sprint33_Control_v1";
        private const string Sprint33Domination = "6e1f6eb3e773451dbda9e0ecd07486d9";
        private Buff[] _sprint33LoadedControl = Array.Empty<Buff>();
        private static UnitEntityData[] Sprint33ControlWitnesses(UnitEntityData caster) =>
            Sprint32SavedAuxiliaries(caster).Where(unit => unit.Get<UnitPartSprint32ListenerReceipt>().Scope == Sprint33ControlScope).ToArray();
        private static bool OwnsControl33(UnitEntityData target, UnitEntityData caster, UnitEntityData[] sources)
        {
            var receipt = target.Get<UnitPartSprint32ListenerReceipt>();
            return receipt != null && receipt.Scope == Sprint33ControlScope && receipt.UnitId == target.UniqueId &&
                receipt.CasterId == caster.UniqueId && receipt.Role == "succubus-domination" && !string.IsNullOrEmpty(receipt.PrepareRunId) &&
                ReferenceEquals(target.Blueprint, BlueprintRoot.Instance.DefaultPlayerCharacter) &&
                ReferenceEquals(target.HoldingState, caster.HoldingState) && !Game.Instance.Player.Party.Contains(target) &&
                sources.Any(source => source.UniqueId == receipt.SourceId && source.Get<UnitPartRelease143RosterReceipt>()?.Key == "succubus");
        }
        private IEnumerable<int> PrepareControl33(UnitEntityData caster, UnitEntityData[] sources)
        {
            if (!Game.Instance.IsPaused || Sprint33ControlWitnesses(caster).Length != 0)
                throw new InvalidOperationException("Expected paused empty Sprint33 control fixture.");
            var source = sources.Single(unit => unit.Get<UnitPartRelease143RosterReceipt>()?.Key == "succubus");
            var party = Game.Instance.Player.Party.ToArray();
            var intake = Game.Instance.State.Units.All.Where(unit => unit != null && !sources.Contains(unit) && !party.Contains(unit)).ToArray();
            var faction = SelectSprint32ListenerFaction(caster, sources, party, intake);
            var target = Game.Instance.EntityCreator.SpawnUnit(BlueprintRoot.Instance.DefaultPlayerCharacter,
                caster.Position, Quaternion.identity, caster.HoldingState);
            Game.Instance.EntityCreator.Tick();
            var receipt = target.Ensure<UnitPartSprint32ListenerReceipt>();
            receipt.Scope = Sprint33ControlScope; receipt.UnitId = target.UniqueId; receipt.CasterId = caster.UniqueId;
            receipt.SourceId = source.UniqueId; receipt.Role = "succubus-domination"; receipt.PrepareRunId = _request.RunId;
            receipt.ListenerFaction = faction.AssetGuid;
            SetExpandedSummoningBrainActive(target, false); target.GroupId = target.UniqueId;
            target.Descriptor.SwitchFactions(faction, true); target.AttackFactions.Match(Array.Empty<BlueprintFaction>());
            var group = target.Group;
            if (!OwnsControl33(target, caster, sources) || group.IsPlayerParty || group.Count != 1 || !ReferenceEquals(group[0], target))
                throw new InvalidOperationException("Control recipient group is not privately owned.");
            group.ResetFactionSet(); group.UpdateAttackFactionsCache();
            if (intake.Any(unit => target.IsEnemy(unit) || unit.IsEnemy(target))) throw new InvalidOperationException("Control witness reaches protected intake.");
            // No cast/save/cost claim: the real AI command is separately observed.
            // Native RuleApplyBuff records the production ability context and18s deadline for cold-load ownership only.
            var ability = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library,
                "1662d63944d94cdeaa62562dc9ac9349", "existing Succubus domination");
            var buff = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(BlueprintBootstrap.Library, Sprint33Domination, "existing domination terminal");
            var data = new AbilityData(source.Descriptor.Abilities.GetAbility(ability));
            var context = new AbilityExecutionContext(data, data.CalculateParams(), new TargetWrapper(target), null);
            var rule = new RuleApplyBuff(target, buff, context, TimeSpan.FromSeconds(18),
                (blueprint, parent, duration) => target.Buffs.AddBuff(blueprint, parent, duration));
            Rulebook.Trigger(rule);
            if (rule.AppliedBuff == null || !rule.AppliedBuff.Active || rule.AppliedBuff.IsDisposed)
                throw new InvalidOperationException("Native effect-only control application refused.");
            rule.AppliedBuff.IsNotDispelable = false;
            receipt.ControlState = new JObject { ["source"] = source.UniqueId, ["endTicks"] = rule.AppliedBuff.EndTime.Ticks,
                ["ability"] = ability.AssetGuid, ["buff"] = buff.AssetGuid }.ToString(Formatting.None);
            for (int frame = 0; frame < 3; frame++) yield return 0;
            ObserveControl33(caster, sources, "prepared");
        }
        private void ObserveControl33(UnitEntityData caster, UnitEntityData[] sources, string phase)
        {
            var targets = Sprint33ControlWitnesses(caster);
            if (targets.Length != 1 || !OwnsControl33(targets[0], caster, sources)) throw new InvalidOperationException("Exact saved control witness absent.");
            var target = targets[0]; var receipt = target.Get<UnitPartSprint32ListenerReceipt>();
            var saved = JObject.Parse(receipt.ControlState);
            var buffs = target.Buffs.Enumerable.Where(buff => buff.Blueprint.AssetGuid == Sprint33Domination).ToArray();
            bool exact = buffs.Length == 1 && buffs[0].Active && !buffs[0].IsDisposed && !buffs[0].IsPermanent &&
                buffs[0].Context?.MaybeCaster?.UniqueId == receipt.SourceId && buffs[0].EndTime.Ticks == (long)saved["endTicks"] &&
                buffs[0].Context?.SourceAbility?.AssetGuid == (string)saved["ability"] && buffs[0].EndTime > Game.Instance.Player.GameTime;
            bool disabledAbsent = !_context.FeatureModules.Active.ExpandedSummoning && buffs.Length == 0 && target.Faction.AssetGuid == receipt.ListenerFaction;
            _sprint33LoadedControl = buffs;
            RosterPersistenceCheck("s33-control-" + phase, exact || disabledAbsent, new JObject { ["saved"] = saved,
                ["buffCount"] = buffs.Length, ["exactActiveContextDeadline"] = exact, ["moduleDisabledAbsence"] = disabledAbsent,
                ["recipient"] = target.UniqueId, ["faction"] = target.Faction.AssetGuid, ["effectOnlyFixture"] = true },
                "Native saved domination retains exact source/ability/deadline, or is safely absent with original faction under module OFF; no cast or save-roll claim.");
            if (!exact && !disabledAbsent) throw new InvalidOperationException("Saved domination context or deadline differs.");
        }
        private IEnumerable<int> CleanupControl33(UnitEntityData caster)
        {
            var targets = Sprint33ControlWitnesses(caster);
            if (targets.Length != 1) throw new InvalidOperationException("Control cleanup witness ambiguous.");
            var target = targets[0]; var receipt = target.Get<UnitPartSprint32ListenerReceipt>();
            bool clean = _sprint33LoadedControl.All(buff => buff.IsDisposed) &&
                !target.Buffs.Enumerable.Any(buff => buff.Blueprint.AssetGuid == Sprint33Domination) && target.Faction.AssetGuid == receipt.ListenerFaction;
            RosterPersistenceCheck("s33-control-source-cleanup", clean, new JObject { ["capturedBuffs"] = _sprint33LoadedControl.Length,
                ["disposed"] = _sprint33LoadedControl.All(buff => buff.IsDisposed), ["originalFaction"] = target.Faction.AssetGuid == receipt.ListenerFaction },
                "Destroyed exact source removes its own loaded control and restores recipient faction before fixture-recipient retirement.");
            if (!clean) throw new InvalidOperationException("Loaded source-owned control remains.");
            CleanupExpandedSummoningUnit(target);
            for (int frame = 0; frame < 3; frame++) { Game.Instance.EntityDestroyer.Tick(); yield return 0; }
            _sprint33LoadedControl = Array.Empty<Buff>();
            ControlAbsence33(caster, "cleanup");
        }
        private void ControlAbsence33(UnitEntityData caster, string phase)
        {
            int remaining = Sprint33ControlWitnesses(caster).Length;
            RosterPersistenceCheck("s33-control-" + phase + "-absence", remaining == 0,
                new JObject { ["remainingOwnedRecipients"] = remaining }, "No Sprint33 recipient receipt remains; unrelated actors are not selected.");
            if (remaining != 0) throw new InvalidOperationException("Sprint33 saved recipient remains.");
        }
    }
}