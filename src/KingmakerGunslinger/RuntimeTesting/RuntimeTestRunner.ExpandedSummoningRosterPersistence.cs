using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    public sealed class UnitPartRelease143RosterReceipt : UnitPart
    {
        [JsonProperty] public string Scope, Key, UnitId, CasterId, BlueprintId, Profile;
    }

    internal sealed partial class RuntimeTestRunner
    {
        private bool RosterPersistenceScope
        { get { return (string)_request.Parameters?["persistenceScope"] == ExpandedSummoningRosterPersistencePolicy.Scope; } }
        private IEnumerator<int> _rosterPersistenceSteps;
        private bool? _rosterPersistencePause;
        private readonly List<RuntimeTestAssertion> _rosterPersistenceChecks = new List<RuntimeTestAssertion>();
        private readonly JArray _rosterPersistenceRows = new JArray();

        private void RosterPersistenceCheck(string name, bool passed, JObject row, string expected)
        {
            row["check"] = name; row["passed"] = passed; _rosterPersistenceRows.Add(row);
            _rosterPersistenceChecks.Add(Assertion("whole-roster-persistence-" + name, expected,
                row.ToString(Formatting.None), passed, "exact native working-save and receipt-owned full roster"));
        }

        private void StartRosterPersistence()
        {
            try
            {
                if (_rosterPersistenceSteps == null) _rosterPersistenceSteps = RunRosterPersistence().GetEnumerator();
                _rosterPersistenceSteps.MoveNext();
            }
            catch (Exception error)
            {
                RosterPersistenceCheck("exception", false,
                    new JObject { ["exception"] = DescribeExpandedSummoningCorrectionException(error) },
                    "complete closed full-roster fixture without exception");
                CompleteRosterPersistence(RuntimeTestStatuses.Fail, "No save was armed after fixture failure.");
            }
        }

        private static BlueprintUnit RosterPersistenceBlueprint(BlueprintScriptableObject[] blueprints, string key)
        {
            if (key.StartsWith("native:", StringComparison.Ordinal))
                return blueprints.OfType<BlueprintUnit>().Single(b => b.AssetGuid == key.Substring(7));
            var creature = ExpandedSummoningCatalog.All.Single(c => c.Key == key);
            string name = ExpandedSummoningInternalName(ExpandedSummoningIdentityCatalog.UnitSymbol(creature));
            return blueprints.OfType<BlueprintUnit>().Single(b => b.name == name);
        }

        private static bool RosterPersistenceOwns(UnitEntityData unit, UnitEntityData caster, BlueprintScriptableObject[] blueprints)
        {
            var receipt = unit.Get<UnitPartRelease143RosterReceipt>();
            if (receipt == null || !ExpandedSummoningRosterPersistencePolicy.Keys.Contains(receipt.Key, StringComparer.Ordinal)) return false;
            var expected = RosterPersistenceBlueprint(blueprints, receipt.Key);
            return ExpandedSummoningRosterPersistencePolicy.Owns(receipt.Scope, receipt.Key, receipt.UnitId, unit.UniqueId,
                receipt.CasterId, caster.UniqueId, receipt.BlueprintId, expected.AssetGuid, unit.Blueprint.AssetGuid) &&
                ReferenceEquals(unit.Blueprint, expected) && ReferenceEquals(unit.HoldingState, caster.HoldingState) &&
                unit.Descriptor.Buffs.Enumerable.Count(buff => ReferenceEquals(buff.Blueprint,
                    BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff) && !buff.IsPermanent &&
                    buff.MaybeContext != null && ReferenceEquals(buff.MaybeContext.MaybeCaster, caster)) == 1;
        }

        private static JObject RosterPersistenceProfile(UnitEntityData unit)
        {
            var s = unit.Stats;
            return new JObject { ["blueprint"] = unit.Blueprint.AssetGuid, ["size"] = unit.Descriptor.State.Size.ToString(),
                ["str"] = s.Strength.BaseValue, ["dex"] = s.Dexterity.BaseValue, ["con"] = s.Constitution.BaseValue,
                ["int"] = s.Intelligence.BaseValue, ["wis"] = s.Wisdom.BaseValue, ["cha"] = s.Charisma.BaseValue,
                ["hpBase"] = s.HitPoints.BaseValue, ["bab"] = s.BaseAttackBonus.BaseValue,
                ["fort"] = s.SaveFortitude.BaseValue, ["ref"] = s.SaveReflex.BaseValue, ["will"] = s.SaveWill.BaseValue,
                ["perceptionRanks"] = s.SkillPerception.BaseValue, ["stealthRanks"] = s.SkillStealth.BaseValue,
                ["mobilityRanks"] = s.SkillMobility.BaseValue, ["persuasionRanks"] = s.SkillPersuasion.BaseValue,
                ["primary"] = unit.Body.PrimaryHand.MaybeWeapon?.Blueprint.AssetGuid,
                ["limbs"] = new JArray(unit.Body.AdditionalLimbs.Select(l => l.MaybeWeapon?.Blueprint.AssetGuid)),
                ["secondary"] = new JArray(unit.Body.AdditionalLimbs.Where(l => l.MaybeWeapon != null && l.MaybeWeapon.IsSecondary)
                    .Select(l => l.MaybeWeapon.Blueprint.AssetGuid)) };
        }

        private static UnityEngine.Object[] RosterPersistenceResources(UnitEntityData unit)
        {
            if (unit.View == null) return new UnityEngine.Object[0];
            // Read only per-instance project meshes/materials. Never destroy
            // resources here or include cached icons/borrowed native assets.
            var materials = unit.View.GetComponentsInChildren<Renderer>(true)
                .SelectMany(r => r.sharedMaterials).Where(m => m != null).ToArray();
            return materials.OfType<UnityEngine.Object>()
                // Original-body albedos in PteranodonAssetRuntime are shared
                // immutable process caches, not private view-owned textures.
                // Variant coat textures are private; serpentine/human owners
                // enumerate their complete resources through the existing seam.
                .Concat(materials.SelectMany(m => m.GetTexturePropertyNames().Select(slot => m.GetTexture(slot)))
                    .Where(t => t != null && t.name.StartsWith(ExpandedSummoningVisualVariantPatch.VariantMaterialName, StringComparison.Ordinal)))
                .Concat(unit.View.GetComponentsInChildren<SkinnedMeshRenderer>(true).Select(r => r.sharedMesh))
                .Concat(unit.View.GetComponentsInChildren<MeshFilter>(true).Select(r => r.sharedMesh))
                .Concat(Sprint17ViewResources(unit))
                .Where(o => o != null && o.name.StartsWith("KMG_", StringComparison.Ordinal)).Distinct().ToArray();
        }

        private IEnumerable<int> DestroyRosterPersistence(UnitEntityData[] units, string stage)
        {
            var owned = units.SelectMany(RosterPersistenceResources).Distinct().ToArray();
            foreach (var unit in units) CleanupExpandedSummoningUnit(unit);
            for (int frame = 0; frame < 5; frame++) { Game.Instance.EntityDestroyer.Tick(); yield return 0; }
            bool gone = units.All(u => u.Destroyed && u.View == null && u.HoldingState == null) && owned.All(o => o == null);
            RosterPersistenceCheck(stage + "-native-destruction", gone,
                new JObject { ["units"] = units.Length, ["capturedProjectResources"] = owned.Length,
                    ["remainingResources"] = owned.Count(o => o != null),
                    ["remainingUnits"] = units.Count(u => !u.Destroyed || u.View != null || u.HoldingState != null) },
                "only exact receipt-owned units retire through native destruction; all captured private resources gone");
        }

        private IEnumerable<int> RunRosterPersistence()
        {
            bool prepare = _request.Scenario == RuntimeTestScenarioCatalog.WorkingSaveExpandedSummoningPrepare;
            bool verify = _request.Scenario == RuntimeTestScenarioCatalog.WorkingSaveExpandedSummoningVerifyCleanup;
            for (int frame = 0; frame < 600 && _snakeLoadResetTrace != null && !_snakeLoadResetTrace.NativeLoadReady; frame++) yield return 0;
            bool loadReady = _snakeLoadResetTrace != null && _snakeLoadResetTrace.NativeLoadReady;
            RosterPersistenceCheck("native-load-boundary", loadReady,
                new JObject { ["ready"] = loadReady, ["completedFrame"] = _snakeLoadResetTrace?.NativeLoadCompletedFrame },
                "actual native load-complete callback returned for exact loaded state/area before fixture work");
            if (!loadReady) { CompleteRosterPersistence(RuntimeTestStatuses.Fail, "No native load boundary;no mutation/save."); yield break; }
            var party = Game.Instance.Player.Party.Where(u => u != null && u.Descriptor != null).ToArray();
            if (party.Length != WorkingSaveSmokeScenario.ExpectedPartyCount) throw new InvalidOperationException("Working party fingerprint changed.");
            var caster = party.First(u => u.HoldingState != null);
            var blueprints = BlueprintBootstrap.Library.GetAllBlueprints().Where(b => b != null).ToArray();
            _rosterPersistencePause = Game.Instance.IsPaused; Game.Instance.IsPaused = true;
            var sceneBefore = caster.HoldingState.AllEntityData.OfType<UnitEntityData>().ToArray();
            var existing = sceneBefore.Where(u => u.Get<UnitPartRelease143RosterReceipt>() != null).ToArray();
            if (existing.Any(u => !RosterPersistenceOwns(u, caster, blueprints)) ||
                ExpandedSummoningPersistentUnits(Game.Instance.State, party).Any(u => !existing.Contains(u)))
                throw new InvalidOperationException("Unowned/malformed summon present;no broad retirement permitted.");
            var unrelated = sceneBefore.Where(u => !existing.Contains(u)).Select(u => new { Unit=u, u.Destroyed, u.HoldingState, u.Blueprint }).ToArray();
            int published;
            bool publication = ExpandedSummoningPublisher.RequiredBasePublicationIsExact(BlueprintBootstrap.Library,
                _context.FeatureModules.Active.ExpandedSummoning, out published);
            RosterPersistenceCheck("publication", publication,
                new JObject { ["enabled"] = _context.FeatureModules.Active.ExpandedSummoning, ["published"] = published },
                "module-appropriate publication with always-registered save identities");
            if (!prepare && !verify)
            {
                RosterPersistenceCheck("fresh-absence", existing.Length == 0,
                    new JObject { ["remainingReceipts"] = existing.Length }, "fresh load after native cleanup save contains no fixture units/state");
                CompleteRosterPersistence(RuntimeTestStatuses.Pass, ""); yield break;
            }
            var units = existing;
            if (prepare)
            {
                if (!_context.FeatureModules.Active.ExpandedSummoning) throw new InvalidOperationException("Prepare requires module ON.");
                foreach (var step in DestroyRosterPersistence(existing, "stale")) yield return step;
                if (_rosterPersistenceChecks.Any(a => a.Status != RuntimeTestStatuses.Pass)) throw new InvalidOperationException("Owned stale fixture failed to retire.");
                var created = new List<UnitEntityData>();
                var originalAlignment = caster.Descriptor.Alignment.Value;
                try
                {
                    foreach (string key in ExpandedSummoningRosterPersistencePolicy.Keys)
                    {
                        UnitEntityData unit;
                        if (!key.StartsWith("native:", StringComparison.Ordinal))
                        {
                            var variant = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                                .Concat(ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly))
                                .First(v => v.Creature.Key == key && v.Multiplicity == SummonMultiplicity.One && SummonVisibilityCatalog.IsPublished(v));
                            unit = SpawnExpandedSummoningVariants(blueprints, caster, new[] { variant }, "marked whole-roster persistence").Single();
                        }
                        else
                        {
                            var native = SummonNativeExpansionCatalog.All.First(n => "native:" + n.UnitGuid == key && n.Multiplicity == SummonMultiplicity.One);
                            var ability = blueprints.OfType<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>()
                                .Single(b => b.name == ExpandedSummoningInternalName(native.Symbol));
                            var before = caster.HoldingState.AllEntityData.OfType<UnitEntityData>().ToArray();
                            caster.Descriptor.Alignment.Set(native.Branch == SummonNativeSpawnBranch.Evil ? Kingmaker.Enums.Alignment.NeutralEvil : Kingmaker.Enums.Alignment.TrueNeutral);
                            caster.Descriptor.AddFact(ability);
                            try { ExecuteExpandedSummoningRuntimeAbility(caster, ability, native.Tier); Game.Instance.EntityCreator.Tick(); }
                            finally { caster.Descriptor.RemoveFact(ability); }
                            unit = caster.HoldingState.AllEntityData.OfType<UnitEntityData>().Single(u => !before.Contains(u) && u.Blueprint.AssetGuid == native.UnitGuid);
                        }
                        var receipt = unit.Ensure<UnitPartRelease143RosterReceipt>();
                        receipt.Scope = ExpandedSummoningRosterPersistencePolicy.ReceiptScope; receipt.Key = key;
                        receipt.UnitId = unit.UniqueId; receipt.CasterId = caster.UniqueId; receipt.BlueprintId = unit.Blueprint.AssetGuid;
                        created.Add(unit); SetExpandedSummoningBrainActive(unit, false);
                        if (!Game.Instance.State.AwakeUnits.Contains(unit)) Game.Instance.State.AwakeUnits.Add(unit);
                    }
                }
                finally { caster.Descriptor.Alignment.Set(originalAlignment); }
                units = created.ToArray();
            }
            Game.Instance.IsPaused = false;
            for (int frame = 0; frame < 600; frame++) { yield return 0; if (frame >= 30 && units.All(SnakePersistenceReady)) break; }
            Game.Instance.IsPaused = true;
            bool ready = units.All(SnakePersistenceReady);
            RosterPersistenceCheck("native-appearance", ready,
                new JObject { ["units"] = units.Length, ["ready"] = ready }, "all owned live views settle native appearance/control without forcing visibility");
            var keys = units.Select(u => u.Get<UnitPartRelease143RosterReceipt>()?.Key).OrderBy(k => k, StringComparer.Ordinal).ToArray();
            bool exact = keys.SequenceEqual(ExpandedSummoningRosterPersistencePolicy.Keys) && units.All(u => RosterPersistenceOwns(u,caster,blueprints));
            RosterPersistenceCheck("all-identities-context-duration", exact && units.All(u => u.View != null && ReferenceEquals(u.View.Data,u) && u.Commands != null &&
                !u.Descriptor.State.IsDead && u.Descriptor.Buffs.Enumerable.Any(b => ReferenceEquals(b.Blueprint,BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff) &&
                    !b.IsPermanent && b.TimeLeft > TimeSpan.Zero && b.TimeLeft.TotalSeconds <= 127d)),
                new JObject { ["count"] = units.Length, ["expected"] = ExpandedSummoningRosterPersistencePolicy.Keys.Length,
                    ["keys"] = new JArray(keys), ["moduleEnabled"] = _context.FeatureModules.Active.ExpandedSummoning },
                "every KMG creature and every distinct retained native unit, exact saved receipt/caster/blueprint and finite duration");
            foreach (var unit in units)
            {
                var receipt = unit.Get<UnitPartRelease143RosterReceipt>();
                var profile = RosterPersistenceProfile(unit);
                if (prepare) receipt.Profile = profile.ToString(Formatting.None);
                bool profileSame = receipt.Profile == profile.ToString(Formatting.None);
                var project = unit.View == null ? new UnityEngine.Object[0] : RosterPersistenceResources(unit);
                bool visual = unit.View != null && (_context.FeatureModules.Active.ExpandedSummoning || project.Length == 0);
                RosterPersistenceCheck("profile-view-" + receipt.Key, profileSame && visual,
                    new JObject { ["key"] = receipt.Key, ["profile"] = profile, ["savedProfile"] = receipt.Profile,
                        ["projectResourceCount"] = project.Length, ["moduleEnabled"] = _context.FeatureModules.Active.ExpandedSummoning },
                    "saved racial/profile/weapon identities retained exactly; module OFF deserializes natively without original-view attachment");
            }
            if (verify) foreach (var step in DestroyRosterPersistence(units, "cleanup")) yield return step;
            bool preserved = unrelated.All(x => x.Unit.Destroyed == x.Destroyed && ReferenceEquals(x.Unit.HoldingState,x.HoldingState) && ReferenceEquals(x.Unit.Blueprint,x.Blueprint)) && Game.Instance.Player.Party.SequenceEqual(party);
            RosterPersistenceCheck("unrelated-preserved", preserved,
                new JObject { ["preexistingUnits"] = unrelated.Length, ["preserved"] = preserved }, "exact unrelated unit references and party membership preserved");
            if (_rosterPersistenceChecks.Any(a => a.Status != RuntimeTestStatuses.Pass)) { CompleteRosterPersistence(RuntimeTestStatuses.Fail,"No save armed after mandatory failure."); yield break; }
            BeginExpandedSummoningPersistenceSave();
        }

        private void CompleteRosterPersistence(string status, string warning)
        {
            if (_rosterPersistencePause.HasValue) { Game.Instance.IsPaused = _rosterPersistencePause.Value; _rosterPersistencePause=null; }
            var evidence = _workingSaveSmoke.Stop();
            bool writes = _request.Scenario != RuntimeTestScenarioCatalog.WorkingSaveExpandedSummoningVerifyAbsent;
            _rosterPersistenceChecks.Add(Assertion("exact-working-load", "exact working descriptor,distinct baseline",
                "working="+evidence.WorkingMatchCount+";baseline="+evidence.BaselineMatchCount,
                evidence.WorkingMatchCount==1 && evidence.BaselineMatchCount==1 && evidence.DescriptorReferenceCorrelated,"native guarded load sentinel"));
            _rosterPersistenceChecks.Add(Assertion("exact-working-save-write",writes?"one native working save":"no write",
                "count="+evidence.ExpectedWorkingSaveRoutineCount+";stashed="+evidence.ExpectedWorkingStashedAreaCount,
                !evidence.SaveWritingApiObserved && evidence.ExpectedWorkingSaveRoutineCount==(writes?1:0) && (!writes || evidence.ExpectedWorkingStashedAreaCount>=1),"exact descriptor-bound native save sentinel"));
            _rosterPersistenceChecks.Add(Assertion("loaded-mod-version",_request.ExpectedModVersion,_context.ModEntry.Info.Version,
                _request.ExpectedModVersion==_context.ModEntry.Info.Version,"loaded UMM version"));
            if (_rosterPersistenceChecks.Any(a=>a.Status!=RuntimeTestStatuses.Pass)) status=RuntimeTestStatuses.Fail;
            File.WriteAllText(Path.Combine(_request.EvidenceDirectory,"expanded-summoning-whole-roster-persistence.json"),_rosterPersistenceRows.ToString(Formatting.Indented));
            var result=CreateResult(status,_rosterPersistenceChecks,null);result.WorkingSaveSmoke=evidence;
            if (!string.IsNullOrWhiteSpace(warning)) result.Warnings.Add(warning);Complete(result);
        }
    }
}
