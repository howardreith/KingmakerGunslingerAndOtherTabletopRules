using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
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
        { get { return (string)_request.Parameters?["persistenceScope"] == ExpandedSummoningRosterPersistencePolicy.Scope ||
            Sprint22PersistenceScope || Sprint23PersistenceScope ||
            Sprint24PersistenceScope || Sprint25PersistenceScope || Sprint26PersistenceScope ||
            Sprint27PersistenceScope || Sprint28PersistenceScope ||
            Sprint29PersistenceScope; } }
        private bool Sprint27PersistenceScope
        { get { return (string)_request.Parameters?["persistenceScope"] == ExpandedSummoningRosterPersistencePolicy.GriffonScope; } }
        private bool Sprint28PersistenceScope
        { get { return (string)_request.Parameters?["persistenceScope"] == ExpandedSummoningRosterPersistencePolicy.GiantHumanoidsScope; } }
        private bool Sprint29PersistenceScope
        { get { return (string)_request.Parameters?["persistenceScope"] == ExpandedSummoningRosterPersistencePolicy.GiantHumanoidsTwoScope; } }
        private bool Sprint26PersistenceScope
        { get { return (string)_request.Parameters?["persistenceScope"] == ExpandedSummoningRosterPersistencePolicy.MagicalBeastsScope; } }
        private bool Sprint25PersistenceScope
        { get { return (string)_request.Parameters?["persistenceScope"] == ExpandedSummoningRosterPersistencePolicy.GiantBodiesScope; } }
        private bool Sprint22PersistenceScope
        { get { return (string)_request.Parameters?["persistenceScope"] == ExpandedSummoningRosterPersistencePolicy.AnkylosaurusScope; } }
        private bool Sprint23PersistenceScope
        { get { return (string)_request.Parameters?["persistenceScope"] == ExpandedSummoningRosterPersistencePolicy.ArmoredDinosaursScope; } }
        private bool Sprint24PersistenceScope
        { get { return (string)_request.Parameters?["persistenceScope"] == ExpandedSummoningRosterPersistencePolicy.TheropodsScope; } }
        private string[] RosterSelectedKeys
        { get { return Sprint22PersistenceScope ? new[] { "ankylosaurus" } :
            Sprint23PersistenceScope ? ExpandedSummoningRosterPersistencePolicy.ArmoredDinosaurKeys :
            Sprint24PersistenceScope ? ExpandedSummoningRosterPersistencePolicy.TheropodKeys :
            Sprint25PersistenceScope ? ExpandedSummoningRosterPersistencePolicy.GiantBodyKeys :
            Sprint26PersistenceScope ? ExpandedSummoningRosterPersistencePolicy.MagicalBeastKeys :
            Sprint27PersistenceScope ? ExpandedSummoningRosterPersistencePolicy.GriffonKeys :
            Sprint28PersistenceScope ? ExpandedSummoningRosterPersistencePolicy.GiantHumanoidKeys :
            Sprint29PersistenceScope ? ExpandedSummoningRosterPersistencePolicy.GiantHumanoidsTwoKeys :
            ExpandedSummoningRosterPersistencePolicy.Keys; } }
        private IEnumerator<int> _rosterPersistenceSteps;
        private bool? _rosterPersistencePause;
        private readonly List<RuntimeTestAssertion> _rosterPersistenceChecks = new List<RuntimeTestAssertion>();
        private readonly JArray _rosterPersistenceRows = new JArray();
        private sealed class RosterReadinessEntry
        {
            internal UnitEntityData Unit, Caster;
            internal string Key, UnitId;
            internal RosterReadinessTimeline Timeline;
        }
        private readonly List<RosterReadinessEntry> _rosterReadiness = new List<RosterReadinessEntry>();

        private void RegisterRosterReadiness(UnitEntityData unit, UnitEntityData caster, int? spawnedFrame)
        {
            _rosterReadiness.Add(new RosterReadinessEntry { Unit = unit, Caster = caster,
                Key = unit.Get<UnitPartRelease143RosterReceipt>().Key, UnitId = unit.UniqueId,
                Timeline = new RosterReadinessTimeline(spawnedFrame) });
            ObserveRosterReadiness(_rosterReadiness.Last());
        }

        private static void ObserveRosterReadiness(RosterReadinessEntry entry)
        {
            var unit = entry.Unit; var view = unit.View; float dissolve = DissolveAmount(unit);
            entry.Timeline.Observe(Time.frameCount, new Dictionary<string, bool> {
                { "ViewExists", view != null }, { "ViewDataMatches", view != null && ReferenceEquals(view.Data, unit) },
                { "ViewIsInGame", view != null && view.IsInGame }, { "EntityFadedIn", EntityFadedIn(unit) },
                { "DissolveFinite", !float.IsNaN(dissolve) && !float.IsInfinity(dissolve) },
                { "DissolveSettled", dissolve >= 0f && dissolve <= .02f },
                { "CanAct", unit.Descriptor.State.CanAct }, { "CanMove", unit.Descriptor.State.CanMove },
                { "AppearanceBuffAbsent", unit.Descriptor.Buffs.GetBuff(BlueprintRoot.Instance.SystemMechanics.SummonedUnitAppearBuff) == null }
            });
        }

        private static JObject RosterReadinessBuff(Kingmaker.UnitLogic.Buffs.Buff buff)
        {
            var context = buff.MaybeContext;
            return new JObject { ["guid"] = buff.Blueprint.AssetGuid, ["name"] = buff.Blueprint.name,
                ["timeLeftSeconds"] = buff.TimeLeft.TotalSeconds, ["permanent"] = buff.IsPermanent,
                ["components"] = new JArray(buff.Blueprint.ComponentsArray.Select(c => c.GetType().FullName)),
                ["declaredConditions"] = new JArray(buff.Blueprint.ComponentsArray
                    .OfType<Kingmaker.UnitLogic.FactLogic.AddCondition>().Select(c => c.Condition.ToString())),
                ["context"] = context == null ? null : new JObject {
                    ["type"] = context.GetType().FullName, ["casterId"] = context.MaybeCaster?.UniqueId,
                    ["ownerId"] = context.MaybeOwner?.UniqueId, ["ability"] = context.SourceAbility?.AssetGuid,
                    ["blueprint"] = context.AssociatedBlueprint?.AssetGuid,
                    ["parentBlueprint"] = context.ParentContext?.AssociatedBlueprint?.AssetGuid } };
        }

        private void FlushAndClearRosterReadiness()
        {
            if (_rosterReadiness.Count == 0) return;
            var report = new RosterReadinessReport(_request.RunId, _request.Scenario, _rosterReadiness.Count);
            try
            {
                foreach (var entry in _rosterReadiness)
                {
                    report.AddUnit(entry.Key, entry.UnitId, () =>
                    {
                    ObserveRosterReadiness(entry);
                    var unit = entry.Unit; var view = unit.View; var state = unit.Descriptor.State;
                    var renderers = view == null ? new Renderer[0] : view.GetComponentsInChildren<Renderer>(true);
                    var appearance = unit.Descriptor.Buffs.Enumerable.Where(b => ReferenceEquals(b.Blueprint,
                        BlueprintRoot.Instance.SystemMechanics.SummonedUnitAppearBuff)).ToArray();
                    float dissolve = DissolveAmount(unit);
                    return new JObject {
                        ["key"] = entry.Key, ["unitId"] = unit.UniqueId, ["blueprintGuid"] = unit.Blueprint.AssetGuid,
                        ["blueprintName"] = unit.Blueprint.name, ["retainedNativeWrapper"] = entry.Key.StartsWith("native:", StringComparison.Ordinal),
                        ["spawnedFrame"] = entry.Timeline.SpawnedFrame, ["observedSamples"] = entry.Timeline.SampleCount,
                        ["firstTrueFrames"] = RosterReadinessReport.NullableFrameMap(entry.Timeline.FirstTrueIncludingNever),
                        ["firstFrameAllSnakePredicates"] = entry.Timeline.FirstAllSnakeFrame,
                        ["firstFrameAllObservedPredicates"] = entry.Timeline.FirstAllObservedFrame,
                        ["firstFramePersistenceReady"] = entry.Timeline.FirstPersistenceReadyFrame,
                        ["persistenceReady"] = entry.Timeline.ReadyForSave,
                        ["finalFrame"] = Time.frameCount, ["viewExists"] = view != null,
                        ["viewDataMatches"] = view != null && ReferenceEquals(view.Data, unit), ["viewIsInGame"] = view != null && view.IsInGame,
                        ["entityFadedIn"] = EntityFadedIn(unit), ["dissolve"] = dissolve.ToString("R", System.Globalization.CultureInfo.InvariantCulture),
                        ["dissolveFinite"] = !float.IsNaN(dissolve) && !float.IsInfinity(dissolve),
                        ["canAct"] = state.CanAct, ["canMove"] = state.CanMove,
                        ["conditions"] = new JArray(Enum.GetValues(typeof(UnitCondition)).Cast<UnitCondition>().Distinct()
                            .Where(c => state.HasCondition(c)).Select(c => c.ToString())),
                        ["buffs"] = new JArray(unit.Descriptor.Buffs.Enumerable.Select(RosterReadinessBuff)),
                        ["conditionFeatures"] = new JArray(unit.Descriptor.Progression.Features.Enumerable
                            .Where(f => f.Blueprint.ComponentsArray.OfType<Kingmaker.UnitLogic.FactLogic.AddCondition>().Any())
                            .Select(f => new JObject { ["guid"] = f.Blueprint.AssetGuid, ["name"] = f.Blueprint.name,
                                ["conditions"] = new JArray(f.Blueprint.ComponentsArray.OfType<Kingmaker.UnitLogic.FactLogic.AddCondition>().Select(c => c.Condition.ToString())) })),
                        ["appearanceBuffCount"] = appearance.Length, ["appearanceBuffs"] = new JArray(appearance.Select(RosterReadinessBuff)),
                        ["rendererCount"] = renderers.Length, ["enabledRenderers"] = renderers.Count(r => r.enabled),
                        ["activeInHierarchyRenderers"] = renderers.Count(r => r.gameObject.activeInHierarchy),
                        ["awake"] = Game.Instance.State.AwakeUnits.Contains(unit), ["destroyed"] = unit.Destroyed,
                        ["dead"] = state.IsDead, ["conscious"] = state.IsConscious,
                        ["position"] = new JArray(unit.Position.x, unit.Position.y, unit.Position.z),
                        ["distanceFromCaster"] = Vector3.Distance(unit.Position, entry.Caster.Position),
                        ["failedPredicateNames"] = new JArray(entry.Timeline.FailedSnakePredicates()),
                        ["failedObservedPredicateNames"] = new JArray(entry.Timeline.FailedObservedPredicates())
                    };
                    });
                }
            }
            finally
            {
                report.Finish(() => {
                    try { foreach (var entry in _rosterReadiness) entry.Timeline.Clear(); }
                    finally { _rosterReadiness.Clear(); }
                });
                File.WriteAllText(Path.Combine(_request.EvidenceDirectory, "expanded-summoning-roster-readiness.json"), report.Result.ToString(Formatting.Indented));
            }
            RosterPersistenceCheck("readiness-observer", !report.HasErrors &&
                (int)report.Result["successfulUnitRows"] == (int)report.Result["unitCount"] &&
                (bool)report.Result["observerClearedInFinally"],
                new JObject { ["observerErrors"] = report.ErrorCount,
                    ["unitRows"] = (int)report.Result["successfulUnitRows"],
                    ["cleared"] = (bool)report.Result["observerClearedInFinally"] },
                "complete per-unit report written without observer errors and request-local tracking cleared");
        }

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
                if (Sprint26PersistenceScope && _sprint26PersistenceRolls == null) _sprint26PersistenceRolls = new Sprint26PersistenceRolls();
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
            var profile=new JObject { ["blueprint"] = unit.Blueprint.AssetGuid, ["size"] = unit.Descriptor.State.Size.ToString(),
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
            if (GiantHumanoidRulesPolicy.IsGiant(
                unit.Get<UnitPartRelease143RosterReceipt>()?.Key) ||
                unit.Get<UnitPartRelease143RosterReceipt>()?.Key == "ettin")
            {
                profile["ownedEquipment"] = new JArray(unit.Descriptor.Inventory.Items
                    .OrderBy(item => item.Blueprint.AssetGuid, StringComparer.Ordinal)
                    .Select(item => new JObject {
                        ["blueprint"] = item.Blueprint.AssetGuid,
                        ["owner"] = item.Owner?.Unit?.UniqueId,
                        ["nonRemovable"] = item.IsNonRemovable,
                        ["lootable"] = item.IsLootable,
                        ["count"] = item.Count
                    }));
                var catchPart = unit.Get<UnitPartSummonGiantRockCatch>();
                profile["catchSpentRound"] = catchPart == null ? (JToken)JValue.CreateNull() :
                    new JValue(catchPart.LastRound);
            }
            string key = unit.Get<UnitPartRelease143RosterReceipt>()?.Key;
            if (key == GiantHumanoidRulesPolicy.Cloud ||
                key == GiantHumanoidRulesPolicy.Storm)
            {
                string[] resources = key == GiantHumanoidRulesPolicy.Cloud ?
                    new[] { "CloudFogResource" } :
                    new[] { "StormCallResource", "StormChainResource" };
                profile["sprint29Resources"] = new JObject(resources
                    .Select(name => new JProperty(name,
                        unit.Descriptor.Resources.GetResourceAmount(
                            Sprint29PersistenceResource(name)))));
            }
            if(unit.Blueprint.AssetGuid==Sprint26MagicalBeastRules.GorgonUnitGuid||unit.Blueprint.AssetGuid==Sprint26MagicalBeastRules.BuletteUnitGuid||unit.Blueprint.AssetGuid==GriffonVisualPolicy.BlueprintGuid){
                var chassis=unit.Descriptor.Progression.Features.Enumerable.Where(v=>v.Blueprint.GetComponent<MagicalBeastPrimaryStrength>()!=null).ToArray();
                var names=chassis.SelectMany(v=>v.SelectComponents<GameLogicComponent>()).Select(v=>v.name).ToArray();
                if(chassis.Length!=1||names.Length!=(unit.Blueprint.AssetGuid==GriffonVisualPolicy.BlueprintGuid?5:3)||names.Any(string.IsNullOrWhiteSpace)||names.Distinct(StringComparer.Ordinal).Count()!=names.Length)
                    throw new InvalidOperationException("Exact magical-beast runtime chassis/component identities unavailable.");
                profile["chassisComponentNames"]=new JArray(names);
                profile["composed"]=new JObject{["hp"]=s.HitPoints.ModifiedValue,["ac"]=s.AC.ModifiedValue,["touch"]=s.AC.Touch,["flatFooted"]=s.AC.FlatFooted,
                    ["fort"]=s.SaveFortitude.ModifiedValue,["ref"]=s.SaveReflex.ModifiedValue,["will"]=s.SaveWill.ModifiedValue,
                    ["perception"]=s.SkillPerception.ModifiedValue,["mobility"]=s.SkillMobility.ModifiedValue};
                profile["attacks"]=new JArray(Sprint19Limbs(unit).Select(w=>{
                    var calc=Rulebook.Trigger(new RuleCalculateWeaponStats(unit,w,null));
                    return new JObject{["weapon"]=w.Blueprint.AssetGuid,["secondary"]=w.IsSecondary,
                        ["bonus"]=Rulebook.Trigger(new RuleCalculateAttackBonusWithoutTarget(unit,w,0)).Result,
                        ["damage"]=Sprint16DamageLine(calc.DamageDescription[0].CreateDamage()),["reachFeet"]=w.AttackRange.Value,["threat"]=w.Blueprint.CriticalRollEdge};
                }));
                if(unit.Blueprint.AssetGuid==Sprint26MagicalBeastRules.GorgonUnitGuid){
                    var breath=unit.Descriptor.Abilities.Enumerable.Single(v=>v.Blueprint.name=="KMG_Summoning_Special_Gorgon_Breath");
                    profile["breathDc"]=new AbilityData(breath).CalculateParams().DC;
                }
            }
            return profile;
        }

        private static BlueprintAbilityResource Sprint29PersistenceResource(
            string suffix)
        {
            return BlueprintBootstrap.Library.GetAllBlueprints()
                .OfType<BlueprintAbilityResource>()
                .Single(value => value.name ==
                    "KMG_Summoning_Giant_Sprint29_" + suffix);
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

        private void ObserveSprint28PersistentGiant(UnitEntityData unit,
            UnitPartRelease143RosterReceipt receipt, JObject profile,
            bool prepare, ItemEntity[] partyBefore, int[] partyCounts)
        {
            var body = unit.Blueprint.Body;
            var expected = new List<string> {
                body.PrimaryHand.AssetGuid,
                body.PrimaryHandAlternative1.AssetGuid,
                body.PrimaryHandAlternative2.AssetGuid
            };
            if (body.Armor != null) expected.Add(body.Armor.AssetGuid);
            ItemEntity[] items = unit.Descriptor.Inventory.Items.ToArray();
            bool exact = items.Length == expected.Count &&
                expected.Distinct(StringComparer.Ordinal).Count() == expected.Count &&
                items.Select(item => item.Blueprint.AssetGuid)
                    .OrderBy(guid => guid, StringComparer.Ordinal)
                    .SequenceEqual(expected.OrderBy(guid => guid, StringComparer.Ordinal)) &&
                items.Distinct().Count() == items.Length &&
                items.All(item => ReferenceEquals(item.Owner, unit.Descriptor) &&
                    item.IsNonRemovable && !item.IsLootable && item.Count == 1) &&
                ((receipt.Key == GiantHumanoidRulesPolicy.Stone) == (body.Armor == null)) &&
                Sprint28PartyInventoryExact(partyBefore, partyCounts);
            var catchPart = unit.Get<UnitPartSummonGiantRockCatch>();
            bool stone = receipt.Key == GiantHumanoidRulesPolicy.Stone;
            JToken savedRound = prepare ? profile["catchSpentRound"] :
                JObject.Parse(receipt.Profile)["catchSpentRound"];
            bool catchExact = stone ?
                catchPart != null && savedRound != null &&
                savedRound.Type == JTokenType.Integer &&
                catchPart.LastRound == (long)savedRound &&
                !catchPart.TrySpend(catchPart.LastRound) :
                catchPart == null && (savedRound == null ||
                    savedRound.Type == JTokenType.Null);
            RosterPersistenceCheck("s28-owned-equipment-catch-" + receipt.Key,
                exact && catchExact,
                new JObject {
                    ["key"] = receipt.Key, ["unit"] = unit.UniqueId,
                    ["prepare"] = prepare, ["itemCount"] = items.Length,
                    ["expectedBlueprints"] = new JArray(expected),
                    ["items"] = new JArray(items.Select(item => new JObject {
                        ["processInstance"] = RuntimeHelpers.GetHashCode(item),
                        ["blueprint"] = item.Blueprint.AssetGuid,
                        ["owner"] = item.Owner?.Unit?.UniqueId,
                        ["nonRemovable"] = item.IsNonRemovable,
                        ["lootable"] = item.IsLootable,
                        ["count"] = item.Count
                    })),
                    ["partyInventoryExact"] =
                        Sprint28PartyInventoryExact(partyBefore, partyCounts),
                    ["savedCatchRound"] = savedRound?.DeepClone(),
                    ["loadedCatchRound"] = catchPart == null ?
                        (JToken)JValue.CreateNull() :
                        new JValue(catchPart.LastRound),
                    ["catchExact"] = catchExact
                },
                "Each saved giant owns its exact distinct nontransferable weapon/armor instances; Stone's spent-round catch ledger survives load without another attempt.");
        }

        private void ObserveSprint29PersistentGiant(UnitEntityData unit,
            UnitPartRelease143RosterReceipt receipt, JObject profile,
            bool prepare, ItemEntity[] partyBefore, int[] partyCounts)
        {
            var body = unit.Blueprint.Body;
            string offhand = receipt.Key == "ettin" ?
                unit.Blueprint.AddFacts.OfType<BlueprintFeature>()
                    .SelectMany(value => value.GetComponents<
                        SummonEttinInitialOffhands>()).Single()
                    .OffhandFlail.AssetGuid :
                body.SecondaryHand?.AssetGuid;
            string[] expected = new[] {
                body.PrimaryHand?.AssetGuid, offhand,
                body.PrimaryHandAlternative1?.AssetGuid,
                body.SecondaryHandAlternative1?.AssetGuid,
                body.PrimaryHandAlternative2?.AssetGuid,
                body.SecondaryHandAlternative2?.AssetGuid,
                body.Armor?.AssetGuid
            }.Where(value => value != null).ToArray();
            ItemEntity[] items = unit.Descriptor.Inventory.Items.ToArray();
            bool owned = items.Length == (receipt.Key == "ettin" ? 5 : 4) &&
                items.Length == expected.Length &&
                expected.Distinct(StringComparer.Ordinal).Count() ==
                    expected.Length &&
                items.Select(item => item.Blueprint.AssetGuid)
                    .OrderBy(guid => guid, StringComparer.Ordinal)
                    .SequenceEqual(expected.OrderBy(guid => guid,
                        StringComparer.Ordinal)) &&
                items.Distinct().Count() == items.Length &&
                items.All(item => ReferenceEquals(item.Owner,
                        unit.Descriptor) && item.IsNonRemovable &&
                    !item.IsLootable && item.Count == 1) &&
                Sprint28PartyInventoryExact(partyBefore, partyCounts);
            var catchPart = unit.Get<UnitPartSummonGiantRockCatch>();
            JToken savedRound = prepare ? profile["catchSpentRound"] :
                JObject.Parse(receipt.Profile)["catchSpentRound"];
            bool catchExact = receipt.Key == "ettin" ?
                catchPart == null && (savedRound == null ||
                    savedRound.Type == JTokenType.Null) :
                catchPart != null && savedRound != null &&
                savedRound.Type == JTokenType.Integer &&
                catchPart.LastRound == (long)savedRound &&
                !catchPart.TrySpend(catchPart.LastRound);
            JObject resources = profile["sprint29Resources"] as JObject;
            bool resourceExact = receipt.Key == "ettin" ?
                resources == null :
                receipt.Key == GiantHumanoidRulesPolicy.Cloud ?
                    resources != null && resources.Count == 1 &&
                    (int)resources["CloudFogResource"] == 0 :
                    resources != null && resources.Count == 2 &&
                    (int)resources["StormCallResource"] == 0 &&
                    (int)resources["StormChainResource"] == 1;
            RosterPersistenceCheck("s29-equipment-resources-" +
                    receipt.Key, owned && catchExact && resourceExact,
                new JObject {
                    ["key"] = receipt.Key, ["unit"] = unit.UniqueId,
                    ["prepare"] = prepare,
                    ["expectedBlueprints"] = new JArray(expected),
                    ["items"] = new JArray(items.Select(item =>
                        new JObject {
                            ["processInstance"] = RuntimeHelpers.GetHashCode(
                                item),
                            ["blueprint"] = item.Blueprint.AssetGuid,
                            ["owner"] = item.Owner?.Unit?.UniqueId,
                            ["nonRemovable"] = item.IsNonRemovable,
                            ["lootable"] = item.IsLootable,
                            ["count"] = item.Count })),
                    ["partyInventoryExact"] = Sprint28PartyInventoryExact(
                        partyBefore, partyCounts),
                    ["savedCatchRound"] = savedRound?.DeepClone(),
                    ["loadedCatchRound"] = catchPart == null ?
                        (JToken)JValue.CreateNull() :
                        new JValue(catchPart.LastRound),
                    ["resources"] = resources?.DeepClone(),
                    ["owned"] = owned,
                    ["catchExact"] = catchExact,
                    ["resourceExact"] = resourceExact },
                "Each saved giant owns exact distinct nonlootable arms, preserves its own spent magic and catch ledger, and leaves unrelated party items untouched.");
        }

        private IEnumerable<int> DestroyRosterPersistence(UnitEntityData[] units, string stage)
        {
            foreach (var step in ObserveRosterResourceCleanup(units, stage)) yield return step;
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
            ItemEntity[] giantPartyItems =
                Sprint28PersistenceScope || Sprint29PersistenceScope ?
                Game.Instance.Player.Inventory.Items.ToArray() : null;
            int[] giantPartyCounts = giantPartyItems?.Select(item => item.Count).ToArray();
            var caster = party.First(u => u.HoldingState != null);
            try
            {
            var blueprints = BlueprintBootstrap.Library.GetAllBlueprints().Where(b => b != null).ToArray();
            _rosterPersistencePause = Game.Instance.IsPaused; Game.Instance.IsPaused = true;
            var sceneBefore = caster.HoldingState.AllEntityData.OfType<UnitEntityData>().ToArray();
            var existing = sceneBefore.Where(u => u.Get<UnitPartRelease143RosterReceipt>() != null).ToArray();
            if (existing.Any(u => !RosterPersistenceOwns(u, caster, blueprints)) ||
                ExpandedSummoningPersistentUnits(Game.Instance.State, party).Any(u => !existing.Contains(u)))
                throw new InvalidOperationException("Unowned/malformed summon present;no broad retirement permitted.");
            var unrelated = sceneBefore.Where(u => !existing.Contains(u) &&
                    (!Sprint22PersistenceScope ||
                        u.Get<UnitPartSprint22ControlReceipt>() == null) &&
                    (!Sprint24PersistenceScope ||
                        u.Get<UnitPartSprint24StateReceipt>() == null) &&
                    (!Sprint26PersistenceScope || u.Get<UnitPartSprint26StateReceipt>() == null))
                .Select(u => new { Unit=u, u.Destroyed, u.HoldingState, u.Blueprint }).ToArray();
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
                if (Sprint24PersistenceScope)
                    foreach (int step in Sprint24PersistentStates(caster,
                        existing, false, false)) yield return step;
                if (Sprint26PersistenceScope)
                    foreach (int step in Sprint26PersistentStates(caster, existing, false, false)) yield return step;
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
                    foreach (string key in RosterSelectedKeys)
                    {
                        UnitEntityData unit;
                        if (!key.StartsWith("native:", StringComparison.Ordinal))
                        {
                            var variant = ExpandedSummoningRosterPersistencePolicy.SelectCreatureVariant(
                                (string)_request.Parameters["persistenceScope"], key);
                            caster.Descriptor.Alignment.Set((Kingmaker.Enums.Alignment)
                                ExpandedSummoningRosterPersistencePolicy.CasterAlignmentFor(variant));
                            unit = SpawnExpandedSummoningVariants(blueprints, caster, new[] { variant }, "marked whole-roster persistence").Single();
                        }
                        else
                        {
                            var native = SummonNativeExpansionCatalog.All.First(n => "native:" + n.UnitGuid == key && n.Multiplicity == SummonMultiplicity.One);
                            var ability = blueprints.OfType<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>()
                                .Single(b => b.name == ExpandedSummoningInternalName(native.Symbol));
                            var before = caster.HoldingState.AllEntityData.OfType<UnitEntityData>().ToArray();
                            caster.Descriptor.Alignment.Set((Kingmaker.Enums.Alignment)
                                ExpandedSummoningRosterPersistencePolicy.CasterAlignmentFor(native));
                            caster.Descriptor.AddFact(ability);
                            try { ExecuteExpandedSummoningRuntimeAbility(caster, ability, native.Tier); Game.Instance.EntityCreator.Tick(); }
                            finally { caster.Descriptor.RemoveFact(ability); }
                            unit = caster.HoldingState.AllEntityData.OfType<UnitEntityData>().Single(u => !before.Contains(u) && u.Blueprint.AssetGuid == native.UnitGuid);
                        }
                        var receipt = unit.Ensure<UnitPartRelease143RosterReceipt>();
                        receipt.Scope = ExpandedSummoningRosterPersistencePolicy.ReceiptScope; receipt.Key = key;
                        receipt.UnitId = unit.UniqueId; receipt.CasterId = caster.UniqueId; receipt.BlueprintId = unit.Blueprint.AssetGuid;
                        RegisterRosterReadiness(unit, caster, Time.frameCount);
                        created.Add(unit); SetExpandedSummoningBrainActive(unit, false);
                        if (!Game.Instance.State.AwakeUnits.Contains(unit)) Game.Instance.State.AwakeUnits.Add(unit);
                    }
                }
                finally
                {
                    caster.Descriptor.Alignment.Set(originalAlignment);
                    RosterPersistenceCheck("caster-alignment-restored", caster.Descriptor.Alignment.Value == originalAlignment,
                        new JObject { ["original"] = originalAlignment.ToString(), ["restored"] = caster.Descriptor.Alignment.Value.ToString() },
                        "per-cast legal fixture alignment is restored before any native save");
                }
                units = created.ToArray();
            }
            else foreach (var unit in units) RegisterRosterReadiness(unit, caster, null);
            Game.Instance.IsPaused = false;
            for (int frame = 0; frame < 600; frame++)
            {
                yield return 0;
                foreach (var entry in _rosterReadiness) ObserveRosterReadiness(entry);
                if (frame >= 30 && _rosterReadiness.All(entry => entry.Timeline.ReadyForSave)) break;
            }
            Game.Instance.IsPaused = true;
            bool ready = _rosterReadiness.Count == units.Length && _rosterReadiness.All(entry => entry.Timeline.ReadyForSave) &&
                units.All(unit => !unit.Destroyed && !unit.Descriptor.State.IsDead && unit.Descriptor.State.IsConscious);
            FlushAndClearRosterReadiness();
            RosterPersistenceCheck("native-appearance", ready,
                new JObject { ["units"] = units.Length, ["ready"] = ready },
                "every owned live view naturally clears native appearance lock and has valid view/control/finite dissolve; current camera/fog/invisibility is not a save prerequisite");
            var keys = units.Select(u => u.Get<UnitPartRelease143RosterReceipt>()?.Key).OrderBy(k => k, StringComparer.Ordinal).ToArray();
            // What this asserts depends on the module, exactly as the
            // publication check above it already does. With Expanded Summoning
            // ON every creature and every distinct retained native unit must
            // be live with a finite duration. With it OFF none of them may
            // instantiate at all - which is the whole point of the module-off
            // pass, and which the zero-variant-allocation check beside this
            // one states positively.
            //
            // This demanded all 114 whatever the module said, while recording
            // `moduleEnabled` in its own evidence and not reading it, so the
            // series closure failed it on behaviour that is correct and that
            // three other assertions in the same run confirm: no variant
            // material or texture allocated, nothing published, and no unit to
            // clean up.
            bool moduleOn = _context.FeatureModules.Active.ExpandedSummoning;
            bool savedTargetedSubset = (Sprint22PersistenceScope ||
                Sprint23PersistenceScope || Sprint24PersistenceScope || Sprint25PersistenceScope ||
                Sprint26PersistenceScope || Sprint27PersistenceScope ||
                Sprint28PersistenceScope || Sprint29PersistenceScope) && verify;
            bool exact = moduleOn || savedTargetedSubset
                ? ExpandedSummoningRosterPersistencePolicy.ExactSelectedKeys(
                    (string)_request.Parameters["persistenceScope"], keys) &&
                    units.All(u => RosterPersistenceOwns(u,caster,blueprints)) &&
                    units.All(u => u.View != null && ReferenceEquals(u.View.Data,u) && u.Commands != null &&
                        !u.Descriptor.State.IsDead && u.Descriptor.Buffs.Enumerable.Any(b => ReferenceEquals(b.Blueprint,BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff) &&
                            !b.IsPermanent && b.TimeLeft > TimeSpan.Zero && b.TimeLeft.TotalSeconds <= 127d))
                : units.Length == 0;
            RosterPersistenceCheck("all-identities-context-duration", exact,
                new JObject { ["count"] = units.Length,
                    ["expected"] = moduleOn || savedTargetedSubset ? RosterSelectedKeys.Length : 0,
                    ["keys"] = new JArray(keys), ["moduleEnabled"] = moduleOn },
                moduleOn
                    ? "every KMG creature and every distinct retained native unit, exact saved receipt/caster/blueprint and finite duration"
                    : savedTargetedSubset
                        ? "the exact saved targeted receipts reconstruct natively while module-OFF publication and private visual allocations remain disabled"
                        : "with the module off not one of this mod's creatures may instantiate at all");
            foreach (var unit in units)
            {
                var receipt = unit.Get<UnitPartRelease143RosterReceipt>();
                if (Sprint28PersistenceScope && prepare && receipt.Key == GiantHumanoidRulesPolicy.Stone)
                {
                    // A request-owned spent catch is a saved precondition. The
                    // real catch and its harm suppression are qualified by the
                    // separate native combat observer.
                    var catchPart = unit.Ensure<UnitPartSummonGiantRockCatch>();
                    long round = ContextActionUngulateTrample.CurrentRound();
                    if (!catchPart.TrySpend(round))
                        throw new InvalidOperationException("Fresh Stone fixture already spent its catch.");
                }
                if (Sprint29PersistenceScope && prepare)
                {
                    if (receipt.Key == GiantHumanoidRulesPolicy.Cloud ||
                        receipt.Key == GiantHumanoidRulesPolicy.Storm)
                    {
                        var catchPart = unit.Ensure<
                            UnitPartSummonGiantRockCatch>();
                        if (!catchPart.TrySpend(
                            ContextActionUngulateTrample.CurrentRound()))
                            throw new InvalidOperationException(
                                "Fresh Sprint 29 giant fixture already spent its catch.");
                        var limited = Sprint29PersistenceResource(
                            receipt.Key == GiantHumanoidRulesPolicy.Cloud ?
                                "CloudFogResource" : "StormCallResource");
                        if (unit.Descriptor.Resources.GetResourceAmount(
                                limited) != 1)
                            throw new InvalidOperationException(
                                "Fresh Sprint 29 magic resource is not full.");
                        unit.Descriptor.Resources.Spend(limited, 1);
                    }
                }
                var profile = RosterPersistenceProfile(unit);
                // Keep the stable racial/weapon receipt independent of
                // module-ON-only observations added below. Those observations
                // are evidence, not a second serialized feature schema.
                if (prepare) receipt.Profile = profile.ToString(Formatting.None);
                bool profileSame = receipt.Profile == profile.ToString(Formatting.None);
                if (Sprint28PersistenceScope)
                    ObserveSprint28PersistentGiant(unit, receipt, profile, prepare,
                        giantPartyItems, giantPartyCounts);
                if (Sprint29PersistenceScope)
                    ObserveSprint29PersistentGiant(unit, receipt, profile,
                        prepare, giantPartyItems, giantPartyCounts);
                JObject moduleExpectedProfile = null;
                if ((Sprint26PersistenceScope || Sprint27PersistenceScope) && !moduleOn) {
                    // The saved fixture was prepared ON. The exact-owner reach
                    // correction is module-gated like released natural reach.
                    // Compare its precise OFF delta while preserving every
                    // other saved field and any additional native reach bonus.
                    moduleExpectedProfile = JObject.Parse(receipt.Profile);
                    var attacks = moduleExpectedProfile["attacks"] as JArray;
                    if (attacks == null) throw new InvalidOperationException("Saved magical-beast attacks are absent.");
                    foreach (JObject attack in attacks) {
                        int correction = Sprint27PersistenceScope ? (int)GriffonRules.Reach(unit.Blueprint.AssetGuid, (string)attack["weapon"], 0) : Sprint26MagicalBeastRules.ReachAdjustment(unit.Blueprint.AssetGuid, (string)attack["weapon"]);
                        attack["reachFeet"] = (int)attack["reachFeet"] - correction;
                    }
                    // The native JObject's string-null tokens serialize as
                    // JSON null and parse back with a different JToken type.
                    // Use the same serialized contract as the saved receipt.
                    profileSame = moduleExpectedProfile.ToString(Formatting.None) == profile.ToString(Formatting.None);
                }
                bool giantFactsAbilitiesExact = true;
                if (Sprint25PersistenceScope && moduleOn && GiantBodyRulesPolicy.IsGiantBody(receipt.Key))
                {
                    bool brach = receipt.Key == GiantBodyRulesPolicy.BrachiosaurusKey;
                    string[] expectedFacts = brach
                        ? new[] { "d809b6c4ff2aaff4fa70d712a70f7d7b", "cc9c862ef2e03af4f89be5088851ea35",
                            "63280e8f08c94c3a9b67787ec2d7ab71", "c9f41b8d20e7456eae3d6c1f5a8b7204" }
                        : new[] { "54ee847996c25cd4ba8773d7b8555174", "79042cb55f030614ea29956177977c52",
                            "b3614622866fe7046b787a548bbd7f59", "175d1577bb6c9a04baf88eec99c66334",
                            "9972f33f977fc724c838e59641b2fca5", "f74c6bdf5c5f5374fb9302ecdc1f7d64",
                            "bd380912f7c74843966c363ab281c0c2" };
                    string[] expectedAbilities = brach
                        ? new[] { "6447d104a2222c14d9c9b8a36e4eb242", "309ec3dedcf04c15b3011933f5eafef4", "b41f7d2a6c8e40539af1c2d5e7b30486" }
                        : new[] { "a7c34e1d9b5f4a02ae6d8c71f3b02594" };
                    var facts = new HashSet<string>(unit.Descriptor.Progression.Features.Enumerable.Select(value => value.Blueprint.AssetGuid));
                    string[] abilities = expectedAbilities.Where(guid => unit.Descriptor.Abilities.GetAbility(
                        BlueprintLibraryLookup.RequireExact<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>(
                            BlueprintBootstrap.Library, guid, "giant persistence command")) != null).ToArray();
                    giantFactsAbilitiesExact = expectedFacts.All(facts.Contains) && abilities.Length == expectedAbilities.Length;
                    giantFactsAbilitiesExact &= unit.Stats.SkillPerception.ModifiedValue == (brach ? 28 : 21);
                    profile["giantNativeFacts"] = new JArray(expectedFacts.Where(facts.Contains).OrderBy(value => value, StringComparer.Ordinal));
                    profile["giantCommandGuids"] = new JArray(abilities.OrderBy(value => value, StringComparer.Ordinal));
                    profile["giantPerception"] = unit.Stats.SkillPerception.ModifiedValue;
                }
                var project = unit.View == null ? new UnityEngine.Object[0] : RosterPersistenceResources(unit);
                bool visual = unit.View != null && (_context.FeatureModules.Active.ExpandedSummoning || project.Length == 0);
                string variantOwner = ExpandedSummoningVisualVariantPatch.DescribeOwnership(unit.View);
                string variantOutcome = ExpandedSummoningVisualVariantPatch.DescribeView(unit.View);
                string rimState = ExpandedSummoningRimAnimationPatch.Describe(unit.View);
                bool rimAbsent = !ExpandedSummoningVisualVariantPatch.RimFor(unit.View).HasValue;
                bool registeredVariant = ExpandedSummoningVisualVariantPatch.RegisteredBlueprintNames.Contains(unit.Blueprint.name);
                string originalBodyOutcome = unit.View == null ? "no-view" :
                    ExpandedSummoningPteranodonViewPatch.DescribeView(unit.View);
                UnityEngine.Object[] armoredOwned = unit.View == null ?
                    new UnityEngine.Object[0] :
                    ExpandedSummoningPteranodonViewPatch
                        .CaptureArmoredDinosaurOwnedResources(unit.View);
                bool armoredBodyExact = true;
                if (Sprint23PersistenceScope &&
                    ArmoredDinosaurVisualPolicy.Handles(receipt.Key))
                    armoredBodyExact = moduleOn
                        ? originalBodyOutcome.StartsWith("visual:attached;",
                            StringComparison.Ordinal) &&
                            armoredOwned.Length == 3
                        : ExpandedSummoningRosterPersistencePolicy
                            .ModuleOffArmoredBodyIsClean(originalBodyOutcome,
                                armoredOwned.Length);
                visual &= armoredBodyExact;
                UnityEngine.Object[] theropodOwned = unit.View == null ?
                    new UnityEngine.Object[0] :
                    ExpandedSummoningPteranodonViewPatch
                        .CaptureTheropodOwnedResources(unit.View);
                bool theropodBodyExact = true;
                if (Sprint24PersistenceScope &&
                    TheropodVisualPolicy.Handles(receipt.Key))
                    theropodBodyExact = moduleOn
                        ? originalBodyOutcome.StartsWith("visual:attached;",
                            StringComparison.Ordinal) &&
                            theropodOwned.Length >= 2
                        : ExpandedSummoningRosterPersistencePolicy
                            .ModuleOffArmoredBodyIsClean(originalBodyOutcome,
                                theropodOwned.Length);
                visual &= theropodBodyExact;
                UnityEngine.Object[] giantOwned = unit.View == null ?
                    new UnityEngine.Object[0] : ExpandedSummoningPteranodonViewPatch.CaptureGiantBodyOwnedResources(unit.View);
                bool giantBodyExact = !Sprint25PersistenceScope || !GiantBodyVisualPolicy.Handles(receipt.Key) ||
                    (moduleOn ? originalBodyOutcome.StartsWith("visual:attached;", StringComparison.Ordinal) && giantOwned.Length >= 2 :
                    ExpandedSummoningRosterPersistencePolicy.ModuleOffArmoredBodyIsClean(originalBodyOutcome, giantOwned.Length));
                visual &= giantBodyExact;
                var gorgonOwned = unit.View == null ? new UnityEngine.Object[0] : ExpandedSummoningPteranodonViewPatch.CaptureGorgonOwnedResources(unit.View);
                bool magicalBodyExact = !Sprint26PersistenceScope || receipt.Key != "gorgon" || (moduleOn
                    ? originalBodyOutcome.StartsWith("visual:attached;", StringComparison.Ordinal) && gorgonOwned.Length >= 2
                    : ExpandedSummoningRosterPersistencePolicy.ModuleOffArmoredBodyIsClean(originalBodyOutcome, gorgonOwned.Length));
                visual &= magicalBodyExact;
                var griffonOwned=unit.View==null?new UnityEngine.Object[0]:ExpandedSummoningPteranodonViewPatch.CaptureGriffonOwnedResources(unit.View);
                bool griffonBodyExact=!Sprint27PersistenceScope || receipt.Key!="griffon" || (moduleOn
                    ? originalBodyOutcome.StartsWith("visual:attached;",StringComparison.Ordinal) && griffonOwned.Length>=2
                    : ExpandedSummoningRosterPersistencePolicy.ModuleOffArmoredBodyIsClean(originalBodyOutcome,griffonOwned.Length));
                visual &= griffonBodyExact;
                if(Sprint27PersistenceScope) {
                    bool transientClean=!unit.Commands.Raw.Any(v=>v!=null&&!v.IsFinished) && SummonGrabComponent.Find(unit)==null;
                    RosterPersistenceCheck("griffon-transient-"+receipt.Key,transientClean,
                        new JObject{["unit"]=unit.UniqueId,["activeCommands"]=unit.Commands.Raw.Count(v=>v!=null&&!v.IsFinished),["hasGrab"]=SummonGrabComponent.Find(unit)!=null,["customChargeUnitPart"]=false},
                        "The exact saved Griffon has no orphan action or invented Grab; rake has no serialized command-state field.");
                }
                if (!_context.FeatureModules.Active.ExpandedSummoning)
                    visual &= variantOwner == "<none>" && rimAbsent && rimState == "<none>" &&
                        (!registeredVariant || variantOutcome == SummonVisualVariantModulePolicy.DisabledOutcome);
                if (Sprint29PersistenceScope && receipt.Key == "ettin")
                {
                    JObject headReceipt;
                    bool headExact;
                    if (moduleOn) headExact = Cleanup2EttinAttachmentMaterialExact(unit.View, out headReceipt);
                    else
                    {
                        int roots = unit.View == null ? 0 : unit.View.GetComponentsInChildren<Transform>(true)
                            .Count(value => value.name == EttinSecondHeadVisual.ObjectPrefix + "_Root");
                        int owned = EttinSecondHeadVisual.CaptureOwnedResources(unit.View).Length;
                        headExact = roots == 0 && owned == 0;
                        headReceipt = new JObject { ["rootCount"] = roots, ["ownedResourceCount"] = owned,
                            ["attachment"] = EttinSecondHeadVisual.Describe(unit.View) };
                    }
                    visual &= headExact;
                    headReceipt["unit"] = unit.UniqueId; headReceipt["moduleEnabled"] = moduleOn;
                    headReceipt["phase"] = prepare ? "prepare" : "cold-verify";
                    headReceipt["viewInstance"] = unit.View == null ? 0 : unit.View.GetInstanceID();
                    headReceipt["observerCalledAttachOrRebuild"] = false;
                    RosterPersistenceCheck("ettin-authored-head-" + (prepare ? "prepare" : "cold-verify"), headExact, headReceipt,
                        moduleOn ? "The actual saved/current exact Ettin has one owned two-driver/four-material head with both original textures and exact native controller/cached-baseline fade initialization; this observer never attaches it."
                            : "The exact module-OFF saved Ettin reconstructs without any project head root or private head allocations.");
                }
                RosterPersistenceCheck("profile-view-" + receipt.Key, profileSame && visual && giantFactsAbilitiesExact,
                    new JObject { ["key"] = receipt.Key, ["profile"] = profile, ["savedProfile"] = receipt.Profile,
                        ["expectedProfileForModule"] = moduleExpectedProfile, ["profileSerializedEqual"] = profileSame, ["visualExact"] = visual,
                        ["projectResourceCount"] = project.Length, ["moduleEnabled"] = _context.FeatureModules.Active.ExpandedSummoning,
                        ["variantRegisteredForSaveCompatibility"] = registeredVariant, ["variantOutcome"] = variantOutcome,
                        ["variantOwnership"] = variantOwner, ["rimLookupAbsent"] = rimAbsent, ["rimControllerState"] = rimState,
                        ["originalBodyOutcome"] = originalBodyOutcome,
                        ["armoredBodyPrivateResourceCount"] = armoredOwned.Length,
                        ["armoredBodyExact"] = armoredBodyExact,
                        ["theropodBodyPrivateResourceCount"] =
                            theropodOwned.Length,
                        ["theropodBodyExact"] = theropodBodyExact,
                        ["giantBodyPrivateResourceCount"] = giantOwned.Length,
                        ["giantBodyExact"] = giantBodyExact, ["griffonBodyExact"]=griffonBodyExact,["griffonPrivateObjects"]=griffonOwned.Length,["magicalBeastBodyExact"] = magicalBodyExact, ["gorgonPrivateObjects"] = gorgonOwned.Length,
                        ["giantFactsAbilitiesExact"] = giantFactsAbilitiesExact,
                        ["rendererMaterials"] = new JArray(unit.View == null ? new string[0] :
                            unit.View.GetComponentsInChildren<Renderer>(true).Where(r => r != null)
                                .SelectMany(r => r.sharedMaterials).Select(m => m == null ? "<null>" : m.name)) },
                    "saved racial/profile/weapon identities retained exactly; each enabled armored dinosaur has one original-body receipt and module OFF deserializes natively without attachment");
            }
            if (Sprint28PersistenceScope)
            {
                ItemEntity[] allGiantItems = units.SelectMany(unit =>
                    unit.Descriptor.Inventory.Items).ToArray();
                RosterPersistenceCheck("s28-all-items-distinct",
                    allGiantItems.Length == 11 &&
                    allGiantItems.Distinct().Count() == allGiantItems.Length &&
                    Sprint28PartyInventoryExact(giantPartyItems,
                        giantPartyCounts),
                    new JObject { ["items"] = allGiantItems.Length,
                        ["distinct"] = allGiantItems.Distinct().Count(),
                        ["processInstances"] = new JArray(allGiantItems
                            .Select(RuntimeHelpers.GetHashCode)),
                        ["partyInventoryExact"] =
                            Sprint28PartyInventoryExact(giantPartyItems,
                                giantPartyCounts) },
                    "Three saved giant owners retain eleven distinct items and leave the party inventory unchanged.");
            }
            if (Sprint29PersistenceScope)
            {
                ItemEntity[] allGiantItems = units.SelectMany(unit =>
                    unit.Descriptor.Inventory.Items).ToArray();
                RosterPersistenceCheck("s29-all-items-distinct",
                    allGiantItems.Length == 13 &&
                    allGiantItems.Distinct().Count() ==
                        allGiantItems.Length &&
                    Sprint28PartyInventoryExact(giantPartyItems,
                        giantPartyCounts),
                    new JObject {
                        ["items"] = allGiantItems.Length,
                        ["distinct"] = allGiantItems.Distinct().Count(),
                        ["processInstances"] = new JArray(allGiantItems
                            .Select(RuntimeHelpers.GetHashCode)),
                        ["partyInventoryExact"] =
                            Sprint28PartyInventoryExact(giantPartyItems,
                                giantPartyCounts) },
                    "Three saved Sprint 29 owners retain thirteen distinct nonlootable items and leave unrelated party inventory exact.");
            }
            if (!_context.FeatureModules.Active.ExpandedSummoning)
            {
                int materials, textures, owners;
                string counts = ExpandedSummoningVisualVariantPatch.CountOwnedObjects(out materials, out textures, out owners);
                RosterPersistenceCheck("module-off-zero-variant-allocations", materials == 0 && textures == 0 && owners == 0,
                    new JObject { ["counts"] = counts, ["registeredVariants"] = ExpandedSummoningVisualVariantPatch.RegisteredBlueprintNames.Count },
                    "save-compatible registration remains; disabled views allocate no variant ownership, material or coat texture; no cached rim recolouring");
            }
            if (Sprint22PersistenceScope)
                foreach (int step in Sprint22PersistentControl(caster,units,prepare,verify)) yield return step;
            if (Sprint24PersistenceScope)
                foreach (int step in Sprint24PersistentStates(caster,
                    units, prepare, verify)) yield return step;
            if (Sprint26PersistenceScope)
                foreach (int step in Sprint26PersistentStates(caster, units, prepare, verify)) yield return step;
            if (verify) foreach (var step in DestroyRosterPersistence(units, "cleanup")) yield return step;
            bool preserved = unrelated.All(x => x.Unit.Destroyed == x.Destroyed && ReferenceEquals(x.Unit.HoldingState,x.HoldingState) && ReferenceEquals(x.Unit.Blueprint,x.Blueprint)) && Game.Instance.Player.Party.SequenceEqual(party);
            RosterPersistenceCheck("unrelated-preserved", preserved,
                new JObject { ["preexistingUnits"] = unrelated.Length, ["preserved"] = preserved }, "exact unrelated unit references and party membership preserved");
            if (_rosterPersistenceChecks.Any(a => a.Status != RuntimeTestStatuses.Pass)) { CompleteRosterPersistence(RuntimeTestStatuses.Fail,"No save armed after mandatory failure."); yield break; }
            if (verify && RosterResourceReport.DiagnosisOnly)
            { CompleteRosterPersistence(RuntimeTestStatuses.Pass,"Resource diagnosis only;prepared Working fixture deliberately not overwritten."); yield break; }
            BeginExpandedSummoningPersistenceSave();
            }
            finally { FlushAndClearRosterReadiness(); }
        }

        private void CompleteRosterPersistence(string status, string warning)
        {
            if (_rosterPersistencePause.HasValue) { Game.Instance.IsPaused = _rosterPersistencePause.Value; _rosterPersistencePause=null; }
            _sprint26PersistenceRolls?.Dispose(); _sprint26PersistenceRolls = null;
            var evidence = _workingSaveSmoke.Stop();
            bool writes = _request.Scenario != RuntimeTestScenarioCatalog.WorkingSaveExpandedSummoningVerifyAbsent &&
                !(_request.Scenario == RuntimeTestScenarioCatalog.WorkingSaveExpandedSummoningVerifyCleanup && RosterResourceReport.DiagnosisOnly);
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
