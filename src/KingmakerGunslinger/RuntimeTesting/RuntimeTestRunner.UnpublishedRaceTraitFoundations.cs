using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.ResourceLinks;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    [Serializable]
    public sealed class RaceTraitFixtureActors : ContextCondition
    {
        [NonSerialized] public UnitEntityData[] Actors;
        protected override string GetConditionCaption() { return "closed disposable trait fixture population"; }
        protected override bool CheckCondition() { return Actors != null && Target?.Unit != null && Actors.Any(u => ReferenceEquals(u, Target.Unit)); }
    }
    internal sealed partial class RuntimeTestRunner
    {
        private RuntimeTestResult RunUnpublishedRaceTraitFoundations()
        {
            if (_request.Scenario != RuntimeTestScenarioCatalog.ObserveUnpublishedRaceTraitFoundations ||
                !_request.ExitAfterCompletion || _workingSaveSmoke == null || !_workingSaveSmoke.Complete || _workingSaveSmoke.WriteObserved)
                throw new InvalidOperationException("Unpublished foundations require exact no-write working-save readiness and automatic exit.");
            var assertions = new List<RuntimeTestAssertion>();
            var actors = new List<UnitEntityData>(); var prototypes = new List<BlueprintUnit>();
            var transient = new List<UnityEngine.Object>();
            var ownedAreaBlueprints = new HashSet<BlueprintAbilityAreaEffect>();
            TraitAreaLookupLease areaLookup = null;
            var game = Game.Instance; bool paused = game.IsPaused; var random = UnityEngine.Random.state;
            var beforeUnits = game.State.Units.All.ToArray(); var beforeAreas = game.State.AreaEffects.All.ToArray();
            var party = game.Player.Party.ToArray(); var buffs = beforeUnits.ToDictionary(u => u, u => u.Buffs.Enumerable.ToArray());
            var registered = BlueprintBootstrap.Library.GetAllBlueprints().ToArray();
            var lookupBefore = BlueprintBootstrap.Library.BlueprintsByAssetId.ToArray();
            string failure = null;
            try
            {
                game.IsPaused = true;
                var anchor = party.First(u => u.IsInGame && u.View != null);
                var holder = CircleSpawn("TraitHolder", anchor.Position, anchor, actors, prototypes);
                var ally = CircleSpawn("TraitAlly", anchor.Position + new Vector3(2.7432f, 0, 0), anchor, actors, prototypes);
                var second = CircleSpawn("TraitHolderTwo", anchor.Position + new Vector3(0, 0, 1), anchor, actors, prototypes);
                var faction = UnityEngine.Object.Instantiate(holder.Blueprint.Faction); transient.Add(faction);
                faction.name = "KMG_Runtime_Trait_Hostile"; faction.Peaceful = false; faction.AlwaysEnemy = false;
                faction.Neutral = false; faction.IsDirectlyControllable = false; faction.Dummy = null;
                faction.AttackFactions = new[] { holder.Blueprint.Faction };
                var enemy = CircleSpawn("TraitEnemy", anchor.Position + new Vector3(1, 0, 0), anchor, actors, prototypes, faction);
                TraitCheck(assertions, "fixture-hostility", holder.IsAlly(ally) && holder.IsEnemy(enemy), "exact native ally/enemy relationships");
                var production=ElementalCharacterTraitPublicationCoordinator.Graph;
                if(!ElementalCharacterTraitPublicationCoordinator.Published || production==null)
                    throw new InvalidOperationException("All-four canonical traits must be published before qualification.");
                var fieryDefinition=ElementalCharacterTraitCatalog.Get(ElementalCharacterTraitId.FieryGlare);
                var fiery=new FieryGlareFoundation {
                    Buff=(BlueprintBuff)production.Resolve(fieryDefinition.Node(CharacterTraitNodeKind.ActivationBuff).Symbol),
                    Toggle=(BlueprintActivatableAbility)production.Resolve(fieryDefinition.Node(CharacterTraitNodeKind.Toggle).Symbol) };
                QualifyFiery(fiery, holder, ally, assertions);
                bool stoicContractRejected = false;
                try { StoicDignityRuntime.VerifyContract(typeof(string)); }
                catch (InvalidOperationException) { stoicContractRejected = true; }
                TraitCheck(assertions, "stoic-contract-fails-closed", stoicContractRejected,
                    "wrong exact context type rejected before construction; no fallback handler");
                var stoicDefinition=ElementalCharacterTraitCatalog.Get(ElementalCharacterTraitId.StoicDignity);
                var stoic=new StoicDignityFoundation {
                    Provider=(BlueprintBuff)production.Resolve(stoicDefinition.Node(CharacterTraitNodeKind.ProviderBuff).Symbol),
                    Recipient=(BlueprintBuff)production.Resolve(stoicDefinition.Node(CharacterTraitNodeKind.RecipientBuff).Symbol),
                    Area=(BlueprintAbilityAreaEffect)production.Resolve(stoicDefinition.Node(CharacterTraitNodeKind.Area).Symbol) };
                ownedAreaBlueprints.Add(stoic.Area);
                QualifyStoic(stoic, holder, ally, second, enemy, actors, transient, assertions);
                areaLookup?.Dispose();
                TraitCheck(assertions, "no-registry-publication", registered.SequenceEqual(BlueprintBootstrap.Library.GetAllBlueprints()) &&
                    lookupBefore.SequenceEqual(BlueprintBootstrap.Library.BlueprintsByAssetId),
                    "both library indexes restored exactly; only the request-local area lookup was temporarily cached; no ordinary bootstrap reachability");
                TraitCheck(assertions, "no-icons", fiery.Buff.Icon == null && fiery.Toggle.Icon != null &&
                    stoic.Provider.Icon == null && stoic.Recipient.Icon == null, "canonical visible toggle icon; hidden production buffs have no independent icon");
            }
            catch (Exception error) { failure = error.ToString(); }
            finally
            {
                foreach (var area in game.State.AreaEffects.All.Except(beforeAreas).Where(a => ownedAreaBlueprints.Contains(a.Blueprint)).ToArray()) { area.ForceEnd(); area.Tick(); }
                foreach (var actor in actors) if (actor != null && !actor.Destroyed) actor.Destroy();
                game.EntityDestroyer.Tick(); game.EntityDestroyer.Tick();
                areaLookup?.Dispose();
                foreach (var prototype in prototypes) UnityEngine.Object.Destroy(prototype);
                foreach (var value in transient.Distinct()) if (value != null) UnityEngine.Object.Destroy(value);
                game.IsPaused = paused; UnityEngine.Random.state = random;
            }
            TraitCheck(assertions, "fixture-exact-cleanup", game.State.Units.All.SequenceEqual(beforeUnits) &&
                game.State.AreaEffects.All.SequenceEqual(beforeAreas) && game.Player.Party.SequenceEqual(party) &&
                buffs.All(pair => pair.Key.Buffs.Enumerable.SequenceEqual(pair.Value)) &&
                registered.SequenceEqual(BlueprintBootstrap.Library.GetAllBlueprints()) &&
                lookupBefore.SequenceEqual(BlueprintBootstrap.Library.BlueprintsByAssetId) && !_workingSaveSmoke.WriteObserved,
                "original unit/area/party/buff references preserved; all disposable entities removed; no save writes");
            if (failure != null) TraitCheck(assertions, "native-execution", false, failure);
            var result = CreateResult(failure == null && assertions.All(a => a.Status == "PASS") ? "PASS" : "FAIL", assertions, failure);
            string path = Path.Combine(_request.EvidenceDirectory, "unpublished-race-trait-foundations.json");
            WriteTeleportationForensicJson(path, new { schemaVersion = 1, runId = _request.RunId,
                FieryGlarePublished = ElementalCharacterTraitPublicationCoordinator.Published, StoicDignityPublished = ElementalCharacterTraitPublicationCoordinator.Published, assertions,
                saveWriteObserved = _workingSaveSmoke.WriteObserved, error = failure,
                identityPolicy = "Exact effect blueprint first; otherwise identical source ability or direct explicit ancestor. Distinct known effects and sibling variants do not match. Caster identity is irrelevant. Missing/ambiguous correlation grants." });
            result.EvidenceFiles.Add(path); result.WorkingSaveSmoke = _workingSaveSmoke.Stop(); return result;
        }
        // Native AreaEffectView constructs a GUID-only BlueprintReference. Its
        // exact Get() implementation requires a lookup even for a disposable
        // area. Follow the qualified diagnostic ProbeRegistration rollback
        // precedent; never add this graph to bootstrap or any player selector.
        private sealed class TraitAreaLookupLease : IDisposable
        {
            private readonly BlueprintAbilityAreaEffect _area;
            private readonly ICollection<BlueprintScriptableObject> _all;
            private bool _owned;
            internal TraitAreaLookupLease(BlueprintAbilityAreaEffect area)
            {
                _area = area; _all = BlueprintBootstrap.Library.GetAllBlueprints();
                if (string.IsNullOrEmpty(area.AssetGuid) || BlueprintBootstrap.Library.BlueprintsByAssetId.ContainsKey(area.AssetGuid))
                    throw new InvalidOperationException("Request-local trait area GUID missing or colliding.");
                _all.Add(area);
                try { BlueprintBootstrap.Library.BlueprintsByAssetId.Add(area.AssetGuid, area); _owned = true; }
                catch { _all.Remove(area); throw; }
            }
            public void Dispose()
            {
                if (!_owned) return;
                BlueprintScriptableObject current;
                if (!BlueprintBootstrap.Library.BlueprintsByAssetId.TryGetValue(_area.AssetGuid, out current) || !ReferenceEquals(current, _area))
                    throw new InvalidOperationException("Request-local trait area lookup ownership changed.");
                BlueprintBootstrap.Library.BlueprintsByAssetId.Remove(_area.AssetGuid);
                _all.Remove(_area); _owned = false;
            }
        }
        private static void PrepareTraitBlueprints(params BlueprintScriptableObject[] blueprints)
        {
            var guid = typeof(BlueprintScriptableObject).GetField("m_AssetGuid", BindingFlags.Instance | BindingFlags.NonPublic);
            if (guid == null || guid.FieldType != typeof(string)) throw new MissingFieldException("BlueprintScriptableObject.m_AssetGuid");
            foreach (var blueprint in blueprints) guid.SetValue(blueprint, Guid.NewGuid().ToString("N"));
        }
        private static void TraitCheck(ICollection<RuntimeTestAssertion> output, string id, bool pass, string observed)
        { output.Add(Assertion("race-trait-" + id, "frozen unpublished foundation contract", observed, pass, "real native skill/save/fact/area lifecycle; request-local fixture")); }
        private static Fact TraitFeatureGrant(UnitEntityData unit, ElementalCharacterTraitId id)
        {
            var graph=ElementalCharacterTraitPublicationCoordinator.Graph;
            if(!ElementalCharacterTraitPublicationCoordinator.Published || graph==null)
                throw new InvalidOperationException("Canonical character traits not published: "+ElementalCharacterTraitPublicationCoordinator.Failure);
            var definition=ElementalCharacterTraitCatalog.Get(id);
            var feature=(Kingmaker.Blueprints.Classes.BlueprintFeature)graph.Resolve(definition.Feature.Symbol);
            var fact=unit.Descriptor.AddFact(feature);
            var grant=fact.SelectComponents<ElementalCharacterTraitOwnedGrant>().Single();
            if(grant.OwnedFact==null || !ReferenceEquals(grant.OwnedFact.Blueprint,graph.Resolve(definition.GrantedNode.Symbol)))
                throw new InvalidOperationException("Published feature did not grant its exact canonical provider: "+id);
            return fact;
        }
        private static Buff TraitOwnedBuff(Fact feature)
        {
            var buff=feature.SelectComponents<ElementalCharacterTraitOwnedGrant>().Single().OwnedFact as Buff;
            if(buff==null) throw new InvalidOperationException("Published feature has no owned provider buff.");
            return buff;
        }
        private static RuleSkillCheck TraitSkill(UnitEntityData unit, StatType skill, int dc, int seed)
        {
            var random = UnityEngine.Random.state;
            try { UnityEngine.Random.InitState(seed); return Rulebook.Trigger(new RuleSkillCheck(unit, skill, dc) { Silent = true, IgnoreDifficultyBonusToDC = true }); }
            finally { UnityEngine.Random.state = random; }
        }
        private static int TraitSeed(int d20)
        {
            var random = UnityEngine.Random.state;
            try { for (int seed = 0; seed < 10000; seed++) { UnityEngine.Random.InitState(seed); if (RulebookEvent.Dice.D20.Value == d20) return seed; } }
            finally { UnityEngine.Random.state = random; }
            throw new InvalidOperationException("No deterministic native d20 seed.");
        }
        private static void QualifyFiery(FieryGlareFoundation graph, UnitEntityData unit, UnitEntityData other, List<RuntimeTestAssertion> assertions)
        {
            int seed = TraitSeed(17);
            var absent = TraitSkill(unit, StatType.CheckIntimidate, 1, seed);
            TraitCheck(assertions, "fiery-absent", !absent.Take10ForSuccess && absent.D20.Value == 17, "d20=" + absent.D20.Value);
            var fact = TraitFeatureGrant(unit, ElementalCharacterTraitId.FieryGlare);
            var toggle = unit.Descriptor.ActivatableAbilities.Enumerable.Single(a => ReferenceEquals(a.Blueprint, graph.Toggle));
            var off = TraitSkill(unit, StatType.CheckIntimidate, 1, seed);
            TraitCheck(assertions, "fiery-off-default", !toggle.IsOn && !off.Take10ForSuccess && off.D20.Value == 17, "default off; d20=" + off.D20.Value);
            var commands = unit.Commands.Queue.ToArray();
            toggle.IsOn = true;
            var success = TraitSkill(unit, StatType.CheckIntimidate, 1, seed);
            TraitCheck(assertions, "fiery-take10-success", success.Take10ForSuccess && success.D20.Value == 10 && success.IsPassed, "d20=" + success.D20.Value);
            var failure = TraitSkill(unit, StatType.CheckIntimidate, 1000, seed);
            TraitCheck(assertions, "fiery-roll-on-take10-failure", failure.Take10ForSuccess && failure.D20.Value == 17 && !failure.IsPassed, "d20=" + failure.D20.Value + ";native fallback to roll");
            foreach (var skill in new[] { StatType.CheckBluff, StatType.CheckDiplomacy, StatType.SkillPersuasion, StatType.SkillUseMagicDevice, StatType.SkillPerception })
            {
                var rule = TraitSkill(unit, skill, 1, seed);
                TraitCheck(assertions, "fiery-unaffected-" + skill, !rule.Take10ForSuccess && rule.D20.Value == 17, "d20=" + rule.D20.Value);
            }
            var otherRule = TraitSkill(other, StatType.CheckIntimidate, 1, seed);
            TraitCheck(assertions, "fiery-independent-units", !otherRule.Take10ForSuccess && otherRule.D20.Value == 17, "unrelated actor unaffected");
            TraitCheck(assertions, "fiery-free-activation", graph.Toggle.ActivateImmediately &&
                graph.Toggle.ActivateWithUnitCommandType == Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Free &&
                !graph.Toggle.ComponentsArray.OfType<ActivatableAbilityResourceLogic>().Any() && unit.Commands.Queue.SequenceEqual(commands),
                "native immediate toggle; Free contract; no resource component or queued action");
            unit.CombatState.JoinCombat();
            var combat = TraitSkill(unit, StatType.CheckIntimidate, 1, seed);
            TraitCheck(assertions, "fiery-combat", unit.IsInCombat && combat.D20.Value == 10, "native combat flag and CheckIntimidate");
            for (int i = 0; i < 4; i++) { toggle.IsOn = false; toggle.IsOn = true; }
            TraitCheck(assertions, "fiery-repeat-one-component", unit.Buffs.Enumerable.Count(b => ReferenceEquals(b.Blueprint, graph.Buff)) == 1 &&
                unit.Buffs.Enumerable.Single(b => ReferenceEquals(b.Blueprint, graph.Buff)).SelectComponents<Take10ForSuccess>().Count() == 1,
                "one native buff and one stat-specific subscriber after repeated off/on");
            toggle.IsOn = false;
            var removed = TraitSkill(unit, StatType.CheckIntimidate, 1, seed);
            TraitCheck(assertions, "fiery-deactivation", !removed.Take10ForSuccess && removed.D20.Value == 17 &&
                !unit.Buffs.Enumerable.Any(b => ReferenceEquals(b.Blueprint, graph.Buff)), "d20=" + removed.D20.Value + ";take10=" + removed.Take10ForSuccess + ";buffCount=" + unit.Buffs.Enumerable.Count(b => ReferenceEquals(b.Blueprint, graph.Buff)));
            unit.Descriptor.RemoveFact(fact);
            TraitCheck(assertions, "fiery-fact-cleanup", !unit.Descriptor.ActivatableAbilities.Enumerable.Any(a => ReferenceEquals(a.Blueprint, graph.Toggle)), "transient toggle removed");
            bool rejected = false; try { UnpublishedRaceTraitFoundationFactory.VerifyFieryContract(typeof(string)); } catch (InvalidOperationException) { rejected = true; }
            TraitCheck(assertions, "fiery-contract-fails-closed", rejected, "wrong exact native type rejected before graph construction");
        }
        private static BlueprintBuff TraitCondition(string suffix, SpellDescriptor descriptor, ICollection<UnityEngine.Object> transient)
        {
            var buff = ScriptableObject.CreateInstance<BlueprintBuff>(); buff.name = "KMG_Runtime_Trait_" + suffix;
            typeof(BlueprintBuff).GetField("m_Flags", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(buff, Enum.Parse(typeof(BlueprintBuff).GetField("m_Flags", BindingFlags.Instance | BindingFlags.NonPublic).FieldType, "HiddenInUi", false));
            buff.FxOnStart = new PrefabLink(); buff.FxOnRemove = new PrefabLink(); buff.ResourceAssetIds = Array.Empty<string>(); buff.Stacking = StackingType.Stack;
            var component = ScriptableObject.CreateInstance<SpellDescriptorComponent>(); component.Descriptor = descriptor;
            buff.ComponentsArray = new BlueprintComponent[] { component }; transient.Add(buff); transient.Add(component); PrepareTraitBlueprints(buff); return buff;
        }
        private static BlueprintAbility TraitEffect(string suffix, SpellDescriptor descriptor, ICollection<UnityEngine.Object> transient)
        {
            var ability = ScriptableObject.CreateInstance<BlueprintAbility>(); ability.name = "KMG_Runtime_Trait_" + suffix;
            var component = ScriptableObject.CreateInstance<SpellDescriptorComponent>(); component.Descriptor = descriptor;
            ability.ComponentsArray = new BlueprintComponent[] { component }; transient.Add(ability); transient.Add(component); PrepareTraitBlueprints(ability); return ability;
        }
        private static MechanicsContext TraitContext(UnitEntityData source, UnitEntityData target, BlueprintScriptableObject effect, MechanicsContext parent = null)
        { return new MechanicsContext(source, source.Descriptor, effect, parent, new TargetWrapper(target)); }
        private static RuleSavingThrow TraitSave(UnitEntityData target, MechanicsContext context)
        { return context.TriggerRule(new RuleSavingThrow(target, SavingThrowType.Will, 100) { Reason = context }); }
        private static void QualifyStoic(StoicDignityFoundation graph, UnitEntityData holder, UnitEntityData ally, UnitEntityData second,
            UnitEntityData enemy, List<UnitEntityData> actors, ICollection<UnityEngine.Object> transient, List<RuntimeTestAssertion> assertions)
        {
            var charm = TraitEffect("CharmA", SpellDescriptor.MindAffecting | SpellDescriptor.Charm, transient);
            var fear = TraitEffect("FearB", SpellDescriptor.MindAffecting | SpellDescriptor.Fear, transient);
            var ordinary = TraitEffect("Ordinary", SpellDescriptor.None, transient);
            var condition = TraitCondition("ActiveCharm", SpellDescriptor.MindAffecting | SpellDescriptor.Charm, transient);
            var fearCondition = TraitCondition("ActiveFear", SpellDescriptor.MindAffecting | SpellDescriptor.Fear, transient);
            var charmContext = TraitContext(holder, ally, charm);
            var baselines = actors.ToDictionary(u => u, u => TraitSave(u, TraitContext(holder, u, charm)).StatValue);
            var holderFeature = TraitFeatureGrant(holder, ElementalCharacterTraitId.StoicDignity);
            var provider = TraitOwnedBuff(holderFeature);
            var add = provider.SelectComponents<AddAreaEffect>().Single();
            var area = (AreaEffectEntityData)typeof(AddAreaEffect).GetField("m_AreaEffectInstance", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(add);
            if (area == null) throw new InvalidOperationException("Native AddAreaEffect did not attach the transient emanation.");
            CircleRefresh(area, actors);
            TraitCheck(assertions, "stoic-self-trait", TraitSave(holder, TraitContext(holder, holder, charm)).StatValue == baselines[holder] + 1 &&
                !holder.Buffs.Enumerable.Any(b => ReferenceEquals(b.Blueprint, graph.Recipient)), "self +1; no own aura recipient");
            TraitCheck(assertions, "stoic-ally-nine-feet", TraitSave(ally, charmContext).StatValue == baselines[ally] + 1 &&
                ally.Buffs.Enumerable.Any(b => ReferenceEquals(b.Blueprint, graph.Recipient)), "native recipient and save +1");
            TraitCheck(assertions, "stoic-enemy", TraitSave(enemy, TraitContext(holder, enemy, charm)).StatValue == baselines[enemy] &&
                !enemy.Buffs.Enumerable.Any(b => ReferenceEquals(b.Blueprint, graph.Recipient)), "enemy within range excluded");
            TraitCheck(assertions, "stoic-nonmind", TraitSave(ally, TraitContext(holder, ally, ordinary)).StatValue == baselines[ally], "non-mind-affecting control");
            Buff active = ally.Descriptor.AddBuff(condition, charmContext, null);
            var beforeEffects = ally.Buffs.Enumerable.ToArray(); var end = active.EndTime;
            TraitCheck(assertions, "stoic-same-ability", TraitSave(ally, charmContext).StatValue == baselines[ally], "active Charm A suppresses only incoming A");
            TraitCheck(assertions, "stoic-unrelated-charm-fear", TraitSave(ally, TraitContext(holder, ally, fear)).StatValue == baselines[ally] + 1, "active Charm A does not suppress Fear B");
            TraitCheck(assertions, "stoic-same-buff", TraitSave(ally, TraitContext(holder, ally, condition, charmContext)).StatValue == baselines[ally], "exact incoming buff identity");
            var otherSource = TraitContext(second, ally, charm);
            TraitCheck(assertions, "stoic-different-caster-same-ability", TraitSave(ally, otherSource).StatValue == baselines[ally], "effect-blueprint policy independent of caster");
            // Native MechanicsContext rejects a null blueprint. An exact
            // descriptor-only area source supplies mind-affecting evidence
            // but no ability, buff or fact identity that could prove a match.
            var unknownSource = ScriptableObject.CreateInstance<BlueprintAbilityAreaEffect>();
            unknownSource.name = "KMG_Runtime_Trait_DescriptorOnlySource";
            var unknownDescriptor = ScriptableObject.CreateInstance<SpellDescriptorComponent>();
            unknownDescriptor.Descriptor = SpellDescriptor.MindAffecting;
            unknownSource.ComponentsArray = new BlueprintComponent[] { unknownDescriptor };
            transient.Add(unknownSource); transient.Add(unknownDescriptor); PrepareTraitBlueprints(unknownSource);
            var unknown = TraitContext(holder, ally, unknownSource);
            TraitCheck(assertions, "stoic-ambiguous-grants", TraitSave(ally, unknown).StatValue == baselines[ally] + 1, "known descriptor but no effect identity grants");
            var distinct = TraitContext(holder, ally, fearCondition, charmContext);
            TraitCheck(assertions, "stoic-distinct-buff-same-source", TraitSave(ally, distinct).StatValue == baselines[ally] + 1, "different known effect stronger than shared ability");
            var child = TraitEffect("CharmChild", SpellDescriptor.MindAffecting, transient); child.Parent = charm;
            TraitCheck(assertions, "stoic-explicit-parent", TraitSave(ally, TraitContext(holder, ally, child)).StatValue == baselines[ally], "explicit child-to-active-source parent lineage");
            TraitCheck(assertions, "stoic-no-cleanse-or-immunity", beforeEffects.SequenceEqual(ally.Buffs.Enumerable) && active.Active &&
                active.EndTime == end && TraitSave(ally, TraitContext(holder, ally, fear)).AutoPass == false,
                "effect references, duration and active state intact; no automatic save pass");
            active.Remove(); active = ally.Descriptor.AddBuff(fearCondition, TraitContext(holder, ally, fear), null);
            TraitCheck(assertions, "stoic-unrelated-fear-charm", TraitSave(ally, charmContext).StatValue == baselines[ally] + 1, "active Fear B does not suppress Charm A"); active.Remove();
            // IsConscious tests native LifeState, not the independent
            // UnitCondition.Unconscious flag. Use the exact qualified
            // request-local LifeState fixture precedent and restore it.
            var originalLifeState = holder.Descriptor.State.LifeState;
            try
            {
                holder.Descriptor.State.LifeState = UnitLifeState.Unconscious;
                int allySave = TraitSave(ally, charmContext).StatValue;
                int selfSave = TraitSave(holder, TraitContext(holder, holder, charm)).StatValue;
                TraitCheck(assertions, "stoic-unconscious-save-time", !holder.Descriptor.State.IsConscious &&
                    allySave == baselines[ally] && selfSave == baselines[holder],
                    "native life=" + holder.Descriptor.State.LifeState + ";ally=" + allySave + ";self=" + selfSave + ";before area refresh");
                holder.Descriptor.State.LifeState = UnitLifeState.Dead;
                TraitCheck(assertions, "stoic-dead-save-time", holder.Descriptor.State.IsDead &&
                    TraitSave(ally, charmContext).StatValue == baselines[ally] &&
                    TraitSave(holder, TraitContext(holder, holder, charm)).StatValue == baselines[holder],
                    "native dead holder gives neither self nor ally bonus");
            }
            finally { holder.Descriptor.State.LifeState = originalLifeState; }
            TraitCheck(assertions, "stoic-conscious-again", holder.Descriptor.State.IsConscious && TraitSave(ally, charmContext).StatValue == baselines[ally] + 1, "later save benefits without periodic scan");
            var original = ally.Position; ally.Translocate(holder.Position + new Vector3(3.3528f, 0, 0), null);
            TraitCheck(assertions, "stoic-outside-save-time", TraitSave(ally, charmContext).StatValue == baselines[ally], "11 feet excluded even before stale aura reconciliation");
            CircleRefresh(area, actors);
            TraitCheck(assertions, "stoic-recipient-exit", !ally.Buffs.Enumerable.Any(b => ReferenceEquals(b.Blueprint, graph.Recipient)), "native area exit removed its recipient");
            ally.Translocate(original, null); CircleRefresh(area, actors);
            TraitCheck(assertions, "stoic-recipient-reentry", ally.Buffs.Enumerable.Count(b => ReferenceEquals(b.Blueprint, graph.Recipient)) == 1, "native reentry restored exactly one recipient");
            var holderPosition = holder.Position;
            holder.Translocate(holderPosition + new Vector3(10, 0, 0), null); CircleRefresh(area, actors);
            TraitCheck(assertions, "stoic-moving-holder-exit", !ally.Buffs.Enumerable.Any(b => ReferenceEquals(b.Blueprint, graph.Recipient)), "moving native attached aura removed distant ally recipient");
            holder.Translocate(holderPosition, null); CircleRefresh(area, actors);
            TraitCheck(assertions, "stoic-moving-holder-return", ally.Buffs.Enumerable.Count(b => ReferenceEquals(b.Blueprint, graph.Recipient)) == 1, "attached area followed holder back exactly once");
            var secondFeature = TraitFeatureGrant(second, ElementalCharacterTraitId.StoicDignity);
            var secondProvider = TraitOwnedBuff(secondFeature);
            var secondArea = (AreaEffectEntityData)typeof(AddAreaEffect).GetField("m_AreaEffectInstance", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(secondProvider.SelectComponents<AddAreaEffect>().Single()); CircleRefresh(secondArea, actors);
            TraitCheck(assertions, "stoic-two-morale-providers", ally.Buffs.Enumerable.Count(b => ReferenceEquals(b.Blueprint, graph.Recipient)) == 2 &&
                TraitSave(ally, charmContext).StatValue == baselines[ally] + 1, "two real recipient facts; native morale maximum +1");
            var modifier = ally.Stats.SaveWill.AddModifier(2, secondProvider, "request-local foreign morale control", ModifierDescriptor.Morale);
            try { TraitCheck(assertions, "stoic-foreign-morale", TraitSave(ally, charmContext).StatValue == baselines[ally] + 2, "foreign +2 morale wins; no additive +3"); }
            finally { modifier.Remove(); }
            TraitCheck(assertions, "stoic-self-and-foreign-aura", TraitSave(holder, TraitContext(holder, holder, charm)).StatValue == baselines[holder] + 2,
                "self trait and OTHER holder's morale stack, never own morale");
            var traitModifier = holder.Stats.SaveWill.AddModifier(2, provider, "request-local foreign trait control", ModifierDescriptor.Trait);
            try { TraitCheck(assertions, "stoic-foreign-trait", TraitSave(holder, TraitContext(holder, holder, charm)).StatValue == baselines[holder] + 3, "foreign +2 trait wins plus foreign +1 morale"); }
            finally { traitModifier.Remove(); }
            var replay = new TraitReplayProbe(ally, graph.Provider, graph.Recipient); EventBus.Subscribe(replay);
            try { TraitCheck(assertions, "stoic-replay-once", TraitSave(ally, charmContext).StatValue == baselines[ally] + 1 && replay.Deliveries >= 2,
                "request-local duplicate delivery of same native save rule; deliveries=" + replay.Deliveries); }
            finally { EventBus.Unsubscribe(replay); }
            second.Descriptor.RemoveFact(secondFeature); holder.Descriptor.RemoveFact(holderFeature); area.Tick(); secondArea.Tick(); Game.Instance.EntityDestroyer.Tick();
            TraitCheck(assertions, "stoic-area-deactivation", !Game.Instance.State.AreaEffects.All.Contains(area) && !Game.Instance.State.AreaEffects.All.Contains(secondArea) &&
                actors.All(u => !u.Buffs.Enumerable.Any(b => ReferenceEquals(b.Blueprint, graph.Recipient) || ReferenceEquals(b.Blueprint, graph.Provider))),
                "native provider/area/recipient cleanup");
            TraitCheck(assertions, "stoic-absent-after-cleanup", TraitSave(ally, charmContext).StatValue == baselines[ally], "no foundation no modifier");
        }

        private sealed class TraitReplayProbe : IGlobalRulebookHandler<RuleSavingThrow>
        {
            private readonly UnitEntityData _unit; private readonly BlueprintBuff _provider, _recipient;
            internal int Deliveries;
            internal TraitReplayProbe(UnitEntityData unit, BlueprintBuff provider, BlueprintBuff recipient) { _unit = unit; _provider = provider; _recipient = recipient; }
            public void OnEventAboutToTrigger(RuleSavingThrow evt)
            {
                if (!ReferenceEquals(evt.Initiator, _unit)) return;
                foreach (var component in _unit.Buffs.Enumerable.Where(b => ReferenceEquals(b.Blueprint, _provider) || ReferenceEquals(b.Blueprint, _recipient))
                    .SelectMany(b => b.SelectComponents<StoicDignitySaveBonus>()))
                { component.OnEventAboutToTrigger(evt); component.OnEventAboutToTrigger(evt); Deliveries += 2; }
            }
            public void OnEventDidTrigger(RuleSavingThrow evt) { }
        }
    }
}
