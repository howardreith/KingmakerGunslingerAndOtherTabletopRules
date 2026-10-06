using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker.UnitLogic.Parts;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Request-local replay injection only; production handling still runs through
    // the native Rulebook. It measures actual modifiers, not a simulated bonus.
    [Serializable]
    public sealed class AerialObserverReplayProbe : RuleInitiatorLogicComponent<RuleSkillCheck>
    {
        [NonSerialized] public int OwnModifierCount;
        public override void OnEventAboutToTrigger(RuleSkillCheck evt)
        {
            if (evt.StatType != StatType.SkillPerception) return;
            foreach (var buff in Owner.Buffs.Enumerable.Where(b => b.Active && !b.IsSuppressed))
                foreach (var component in buff.SelectComponents<AerialObserverPerceptionBonus>())
                {
                    component.OnEventAboutToTrigger(evt);
                    component.OnEventAboutToTrigger(evt);
                }
            OwnModifierCount = Owner.Stats.SkillPerception.Modifiers.Count(m =>
                m.Source != null && m.Source.Blueprint.GetComponent<AerialObserverPerceptionBonus>() != null);
        }
        public override void OnEventDidTrigger(RuleSkillCheck evt) { }
    }

    internal sealed partial class RuntimeTestRunner
    {
        private RuntimeTestResult RunUnpublishedAerialObserverFoundation()
        {
            if (_request.Scenario != RuntimeTestScenarioCatalog.ObserveUnpublishedAerialObserverFoundation ||
                !_request.ExitAfterCompletion || _workingSaveSmoke == null || !_workingSaveSmoke.Complete || _workingSaveSmoke.WriteObserved)
                throw new InvalidOperationException("Unpublished Aerial Observer requires exact no-write working-save readiness and automatic exit.");
            var assertions = new List<RuntimeTestAssertion>();
            var actors = new List<UnitEntityData>(); var prototypes = new List<BlueprintUnit>();
            var transient = new List<UnityEngine.Object>();
            var game = Game.Instance; bool paused = game.IsPaused; var random = UnityEngine.Random.state;
            var time = game.Player.GameTime;
            var beforeUnits = game.State.Units.All.ToArray(); var beforeAreas = game.State.AreaEffects.All.ToArray();
            var party = game.Player.Party.ToArray();
            var buffs = beforeUnits.ToDictionary(u => u, u => u.Buffs.Enumerable.ToArray());
            var positions = beforeUnits.ToDictionary(u => u, u => u.Position);
            var registered = BlueprintBootstrap.Library.GetAllBlueprints().ToArray();
            var lookup = BlueprintBootstrap.Library.BlueprintsByAssetId.ToArray();
            string failure = null;
            try
            {
                game.IsPaused = true;
                var anchor = party.First(u => u.IsInGame && u.View != null);
                var holder = CircleSpawn("AerialObserverHolder", anchor.Position, anchor, actors, prototypes);
                var other = CircleSpawn("AerialObserverControl", anchor.Position + new Vector3(1, 0, 0), anchor, actors, prototypes);
                var carrier = BlueprintBootstrap.ElementalFeats.RequireSymbol<BlueprintBuff>(ElementalRaceIdentityCatalog.WingsOfAirBuff);
                AerialObserverFlightContract.VerifyCarrier(carrier);
                TraitCheck(assertions, "aerial-exact-carrier-contract", carrier.AssetGuid == AerialObserverFlightContract.CarrierGuid,
                    "canonical released flight buff with all three native mechanical components");
                int basis = holder.Stats.SkillPerception.ModifiedValue;
                int otherBasis = other.Stats.SkillPerception.ModifiedValue;
                int otherSkill = holder.Stats.SkillStealth.ModifiedValue;
                var absent = AerialSkill(holder, false);
                TraitCheck(assertions, "aerial-absent", absent.StatValue == basis, "ordinary native Perception without the foundation");
                var flight = AerialBuff(holder, carrier);
                TraitCheck(assertions, "aerial-flight-without-foundation", AerialObserverFlightContract.HasExactFlight(holder.Descriptor, carrier) &&
                    AerialSkill(holder, false).StatValue == basis, "flight alone supplies no Perception bonus");
                flight.Remove();
                var provider = UnpublishedAerialObserverFoundationFactory.Create(carrier);
                var duplicate = UnpublishedAerialObserverFoundationFactory.Create(carrier);
                transient.Add(provider); transient.Add(duplicate);
                transient.AddRange(provider.ComponentsArray); transient.AddRange(duplicate.ComponentsArray);
                PrepareTraitBlueprints(provider, duplicate);
                var first = AerialBuff(holder, provider);
                var secondUnit = AerialBuff(other, provider);
                TraitCheck(assertions, "aerial-grounded", AerialSkill(holder, false).StatValue == basis &&
                    AerialSkill(holder, true).StatValue == basis, "active and cached native grounded controls");
                holder.FlyHeight = 4;
                TraitCheck(assertions, "aerial-visual-height-only", AerialSkill(holder, false).StatValue == basis,
                    "request-owned FlyHeight alone does not establish mechanical flight; no movement controller is changed");
                holder.FlyHeight = 0;
                flight = AerialBuff(holder, carrier);
                TraitCheck(assertions, "aerial-native-flight-state", flight.Active && !flight.IsSuppressed &&
                    holder.Descriptor.State.HasConditionImmunity(UnitCondition.DifficultTerrain),
                    "real native flight fact activates the mechanical terrain-immunity state");
                TraitCheck(assertions, "aerial-passive-stat-consumers", holder.Stats.SkillPerception.ModifiedValue == basis + 2 &&
                    AerialPassiveValue(holder, "Kingmaker.Controllers.GlobalMap.LocationRevealController", "<Tick>b__0_0") == basis + 2 &&
                    AerialPassiveValue(holder, "Kingmaker.Visual.FogOfWar.FogOfWarSettings", "<get_Radius>b__19_0") == basis + 2,
                    "actual native passive Perception lambdas consume +2 before any skill rule; no discovery/radius controller is ticked");
                var active = AerialSkill(holder, false);
                TraitCheck(assertions, "aerial-active-perception", active.StatValue == basis + 2,
                    "native RuleSkillCheck stat=" + active.StatValue + ";base=" + basis);
                var cached = AerialSkill(holder, true);
                TraitCheck(assertions, "aerial-cached-perception", cached.StatValue == basis + 2,
                    "real RuleCachedPerceptionCheck uses the same +2 Trait stat resolution");
                TraitCheck(assertions, "aerial-rule-cleanup", holder.Stats.SkillPerception.ModifiedValue == basis + 2 &&
                    holder.Stats.SkillPerception.Modifiers.Count(m => ReferenceEquals(m.Source, first)) == 1,
                    "one event-maintained Trait modifier survives rules; neither rule adds a second modifier");
                TraitCheck(assertions, "aerial-other-skill", TraitSkill(holder, StatType.SkillStealth, 1000, 317).StatValue == otherSkill,
                    "native Stealth remains unchanged while flight is active");
                TraitCheck(assertions, "aerial-independent-units", AerialSkill(other, false).StatValue == otherBasis,
                    "the second foundation holder is grounded and receives no bonus");
                var foreign = holder.Stats.SkillPerception.AddModifier(1, first, "AerialFixtureForeignTrait", ModifierDescriptor.Trait);
                holder.Stats.SkillPerception.UpdateValue();
                TraitCheck(assertions, "aerial-foreign-lesser-trait", AerialSkill(holder, false).StatValue == basis + 2,
                    "native +1 Trait is replaced by the effective +2, not summed");
                foreign.Remove(); holder.Stats.SkillPerception.UpdateValue();
                foreign = holder.Stats.SkillPerception.AddModifier(3, first, "AerialFixtureForeignTrait", ModifierDescriptor.Trait);
                holder.Stats.SkillPerception.UpdateValue();
                TraitCheck(assertions, "aerial-foreign-greater-trait", AerialSkill(holder, false).StatValue == basis + 3,
                    "native +3 Trait wins over the foundation's +2");
                foreign.Remove(); holder.Stats.SkillPerception.UpdateValue();
                foreign = holder.Stats.SkillPerception.AddModifier(3, first, "AerialFixtureForeignCompetence", ModifierDescriptor.Competence);
                holder.Stats.SkillPerception.UpdateValue();
                TraitCheck(assertions, "aerial-foreign-distinct-descriptor", AerialSkill(holder, false).StatValue == basis + 5,
                    "different native descriptor adds normally");
                foreign.Remove(); holder.Stats.SkillPerception.UpdateValue();
                var duplicateFact = AerialBuff(holder, duplicate);
                TraitCheck(assertions, "aerial-duplicate-native-trait", AerialSkill(holder, false).StatValue == basis + 2,
                    "two actual providers still yield one effective +2 Trait");
                var replayBuff = UnpublishedAerialObserverFoundationFactory.Create(carrier);
                foreach (var component in replayBuff.ComponentsArray) transient.Add(component);
                var replayComponent = ScriptableObject.CreateInstance<AerialObserverReplayProbe>();
                replayComponent.name = "$AerialObserverOwnedReplayProbe";
                replayBuff.ComponentsArray = new BlueprintComponent[] { replayComponent };
                transient.Add(replayBuff); transient.Add(replayComponent); PrepareTraitBlueprints(replayBuff);
                var replayFact = AerialBuff(holder, replayBuff);
                var replay = AerialSkill(holder, false);
                var probe = replayFact.SelectComponents<AerialObserverReplayProbe>().Single();
                TraitCheck(assertions, "aerial-replay-once-per-provider", replay.StatValue == basis + 2 && probe.OwnModifierCount == 2,
                    "each of two native provider instances receives replay twice; exact actual modifier count=" + probe.OwnModifierCount);
                replayFact.Remove(); duplicateFact.Remove();
                flight.Deactivate();
                TraitCheck(assertions, "aerial-flight-inactive", !flight.Active && holder.Stats.SkillPerception.ModifiedValue == basis && AerialSkill(holder, false).StatValue == basis &&
                    AerialSkill(holder, true).StatValue == basis, "inactive exact carrier does not qualify either native Perception path");
                flight.Activate();
                TraitCheck(assertions, "aerial-flight-reactivated", flight.Active && holder.Stats.SkillPerception.ModifiedValue == basis + 2 && AerialSkill(holder, false).StatValue == basis + 2,
                    "later native reactivation benefits a new rule without polling");
                var suppression = holder.Descriptor.Ensure<UnitPartBuffSuppress>();
                suppression.Suppress(carrier);
                TraitCheck(assertions, "aerial-native-flight-suppression", flight.IsSuppressed && !flight.Active &&
                    holder.Stats.SkillPerception.ModifiedValue == basis && AerialSkill(holder, true).StatValue == basis,
                    "native exact-buff suppression removes the bonus before raw/cached Perception resolution");
                suppression.Release(carrier);
                TraitCheck(assertions, "aerial-native-flight-suppression-release", !flight.IsSuppressed && flight.Active &&
                    holder.Stats.SkillPerception.ModifiedValue == basis + 2,
                    "native release reactivates the flight listener and bonus without a rule or polling");
                flight.Remove();
                TraitCheck(assertions, "aerial-flight-removed", holder.Stats.SkillPerception.ModifiedValue == basis && AerialSkill(holder, false).StatValue == basis &&
                    AerialSkill(holder, true).StatValue == basis && !holder.Descriptor.State.HasConditionImmunity(UnitCondition.DifficultTerrain),
                    "native flight removal restores grounded active/cached Perception and terrain-immunity baseline");
                for (int cycle = 0; cycle < 3; ++cycle)
                {
                    flight = AerialBuff(holder, carrier);
                    if (AerialSkill(holder, false).StatValue != basis + 2) throw new InvalidOperationException("Repeated flight activation lost its bonus.");
                    flight.Remove();
                    if (AerialSkill(holder, false).StatValue != basis) throw new InvalidOperationException("Repeated flight removal left a stale bonus.");
                }
                TraitCheck(assertions, "aerial-repeated-transitions", holder.Buffs.Enumerable.Count(b => ReferenceEquals(b.Blueprint, provider)) == 1,
                    "three native flight grant/remove cycles retain one provider");
                flight = AerialBuff(holder, carrier);
                first.Remove(); secondUnit.Remove();
                TraitCheck(assertions, "aerial-flight-listener-detached", flight.Components.Count == carrier.ComponentsArray.Length &&
                    !flight.Components.OfType<AerialObserverFlightTransition>().Any() && holder.Stats.SkillPerception.ModifiedValue == basis,
                    "foundation removal detaches only its owned listener and modifier from the still-active native flight fact");
                TraitCheck(assertions, "aerial-provider-removal", AerialSkill(holder, false).StatValue == basis,
                    "real mechanical flight after foundation removal supplies no bonus");
                flight.Remove();
                bool wrongRule = false;
                try { AerialObserverFlightContract.VerifyNativeContract(typeof(string)); } catch (InvalidOperationException) { wrongRule = true; }
                TraitCheck(assertions, "aerial-contract-fails-closed", wrongRule, "wrong native rule type fails before construction");
                bool wrongCarrier = false;
                var clone = UnityEngine.Object.Instantiate(carrier); transient.Add(clone);
                try { AerialObserverFlightContract.VerifyCarrier(clone); } catch (InvalidOperationException) { wrongCarrier = true; }
                TraitCheck(assertions, "aerial-cloned-carrier-rejected", wrongCarrier,
                    "copied name/GUID/components without canonical object identity cannot broaden flight");
                TraitCheck(assertions, "aerial-unregistered-idempotent-builders", provider.AssetGuid != duplicate.AssetGuid && provider.Icon == null &&
                    duplicate.Icon == null && registered.SequenceEqual(BlueprintBootstrap.Library.GetAllBlueprints()) &&
                    lookup.SequenceEqual(BlueprintBootstrap.Library.BlueprintsByAssetId),
                    "fresh random request identities only; repeated builders register no blueprint or icon consumer");
            }
            catch (Exception error) { failure = error.ToString(); }
            finally
            {
                foreach (var actor in actors) if (actor != null && !actor.Destroyed) actor.Destroy();
                game.EntityDestroyer.Tick(); game.EntityDestroyer.Tick();
                foreach (var prototype in prototypes) UnityEngine.Object.Destroy(prototype);
                foreach (var value in transient.Distinct()) if (value != null) UnityEngine.Object.Destroy(value);
                game.IsPaused = paused; UnityEngine.Random.state = random;
            }
            TraitCheck(assertions, "aerial-fixture-exact-cleanup", game.State.Units.All.SequenceEqual(beforeUnits) &&
                game.State.AreaEffects.All.SequenceEqual(beforeAreas) && game.Player.Party.SequenceEqual(party) &&
                buffs.All(p => p.Key.Buffs.Enumerable.SequenceEqual(p.Value)) && positions.All(p => p.Key.Position == p.Value) &&
                game.Player.GameTime == time && registered.SequenceEqual(BlueprintBootstrap.Library.GetAllBlueprints()) &&
                lookup.SequenceEqual(BlueprintBootstrap.Library.BlueprintsByAssetId) && !_workingSaveSmoke.WriteObserved,
                "original units/positions/areas/party/facts/library/time preserved; disposable actors/facts removed; no save write");
            if (failure != null) TraitCheck(assertions, "aerial-native-execution", false, failure);
            var result = CreateResult(failure == null && assertions.All(a => a.Status == "PASS") ? "PASS" : "FAIL", assertions, failure);
            string path = Path.Combine(_request.EvidenceDirectory, "unpublished-aerial-observer-foundation.json");
            WriteTeleportationForensicJson(path, new { schemaVersion = 1, runId = _request.RunId, LungePublished = false,
                AerialObserverPublished = false, carrierGuid = AerialObserverFlightContract.CarrierGuid,
                adaptation = "+2 Trait Perception during exact active released Wings of Air mechanical flight; no altitude inference",
                assertions, saveWriteObserved = _workingSaveSmoke.WriteObserved, error = failure });
            result.EvidenceFiles.Add(path); result.WorkingSaveSmoke = _workingSaveSmoke.Stop(); return result;
        }

        private static int AerialPassiveValue(UnitEntityData unit, string outerName, string methodName)
        {
            var outer = typeof(UnitEntityData).Assembly.GetType(outerName, true);
            var nested = outer.GetNestedType("<>c", BindingFlags.NonPublic);
            var singleton = nested?.GetField("<>9", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
            var method = nested?.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                null, new[] { typeof(UnitEntityData) }, null);
            if (singleton == null || method == null || method.ReturnType != typeof(int))
                throw new InvalidOperationException("Exact native passive Perception stat-reader contract missing.");
            return (int)method.Invoke(singleton, new object[] { unit });
        }

        private static Buff AerialBuff(UnitEntityData unit, BlueprintBuff blueprint)
        {
            var context = new MechanicsContext(unit, unit.Descriptor, blueprint, null, new TargetWrapper(unit));
            var buff = unit.Descriptor.AddBuff(blueprint, context, null);
            if (buff == null) throw new InvalidOperationException("Native Aerial Observer fixture buff was rejected.");
            return buff;
        }
        private static RuleSkillCheck AerialSkill(UnitEntityData unit, bool cached)
        {
            if (!cached) return TraitSkill(unit, StatType.SkillPerception, 1000, 317);
            var rule = new RuleCachedPerceptionCheck(unit, 1000) { Silent = true, IgnoreDifficultyBonusToDC = true };
            return Rulebook.Trigger(rule);
        }
    }
}
