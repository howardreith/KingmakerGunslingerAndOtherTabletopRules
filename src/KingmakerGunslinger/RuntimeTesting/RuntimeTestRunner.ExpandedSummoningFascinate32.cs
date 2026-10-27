using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> ReviewLillendFascinate32(ExpandedSummoningCorrectionFixture fixture, bool combatTransition = false)
        {
            Action<string, bool, JObject, string> check = (name, passed, row, expected) =>
                Sprint25Check(combatTransition ? name + "-combat-case" : name, passed, row, expected);
            string phase = RuntimeTestRequestParser.Sprint32ReviewPhase(_request);
            bool bardic = phase == ExpandedSummoningNativeProfilePolicy.Sprint32BardicSuggestionPhase;
            if (!IsSprint32Review || !_request.ExitAfterCompletion || _request.Parameters?.Count != 1 ||
                (!bardic && phase != ExpandedSummoningNativeProfilePolicy.Sprint32FascinatePhase))
                throw new InvalidOperationException("Exact save-free Fascinate phase required.");
            var game = Game.Instance; var player = game.Player; var party = player.Party.ToArray();
            var anchor = player.MainCharacter.Value; var target = fixture.Hostile;
            bool anchorOwned = anchor != null && target != null && _summoningProfileActor != null &&
                _summoningProfilePreset != null && _summoningProfileOriginal != null &&
                ReferenceEquals(anchor.Blueprint, _summoningProfileActor) &&
                ReferenceEquals(_summoningProfilePreset.PlayerCharacter, _summoningProfileActor) &&
                !ReferenceEquals(_summoningProfileOriginal.PlayerCharacter, _summoningProfileActor) &&
                _summoningProfileActor.name == "KMG_Runtime_Summoning_Profile_Anchor" &&
                _summoningProfileSaveGuard != null && _summoningProfileScenes != null &&
                party.Length == 1 && ReferenceEquals(party[0], anchor) && anchor.Commands.Empty;
            check("fascinate-owned-anchor", anchorOwned, new JObject { ["anchor"] = anchor?.UniqueId },
                "Only the existing save-free cloned anchor is repositioned/suspended; no intake actor or save is changed.");
            if (!anchorOwned) throw new InvalidOperationException("Fascinate anchor ownership ambiguous.");
            bool anchorBrain = anchor.IsBrainActive, casterBrain = fixture.Caster.IsBrainActive;
            var anchorPosition = anchor.Position; var casterPosition = fixture.Caster.Position;
            int will = target.Stats.SaveWill.BaseValue, perception = target.Stats.SkillPerception.ModifiedValue;
            int allyPerception = fixture.Caster.Stats.SkillPerception.ModifiedValue;
            UnitEntityData owner = null, sibling = null, globeEmitter = null;
            Vector3? globeParking = null;
            bool partyAdded = false, immunityAdded = false, complete = false;
            Cleanup2NativeDefaultScope mode = null;
            Lillend32CommandStarts starts = null; Dretch30CloudSaves saves = null; Astral31DispelEvents events = null;
            var cleanup = new List<string>();
            try
            {
                foreach (var unit in new[] { anchor, fixture.Caster, target }) Sprint28SetBrainActivePreservingActions(unit, false);
                owner = CastExpandedSummoningOwnTier(fixture, "lillend-azata");
                sibling = CastExpandedSummoningOwnTier(fixture, "lillend-azata");
                if (bardic)
                {
                    // A summoning setup command is itself a perceived spell
                    // threat. Finish it before natural Fascinate begins.
                    globeEmitter = CastExpandedSummoningOwnTier(fixture, "astral-deva");
                    Sprint28SetBrainActivePreservingActions(globeEmitter, false);
                    PlaceExpandedSummoningUnit(globeEmitter, fixture.Caster.Position + Vector3.forward * 80f);
                }
                foreach (var unit in new[] { owner, sibling, fixture.Caster, target, globeEmitter }.Where(value => value != null))
                {
                    Sprint28SetBrainActivePreservingActions(unit, false);
                    unit.Commands.InterruptAll(true); unit.Commands.RemoveFinishedAndUpdateQueue();
                    unit.Wake(0f); if (!game.State.AwakeUnits.Contains(unit)) game.State.AwakeUnits.Add(unit);
                }
                Vector3 targetPosition; string survey, direction;
                var origin = Sprint24AiCombatLane(fixture.Caster, owner, target, 10f,
                    out targetPosition, out survey, out direction, targetAllyExclusionRadius: 9f,
                    targetAllyExclusionExceptions: new[] { anchor, fixture.Caster, sibling }, sprint32PerformanceSight: true);
                var away = (origin - targetPosition).normalized;
                var side = Vector3.Cross(Vector3.up, away).normalized;
                var requestedAnchor = targetPosition + side * 10f;
                var anchorPoint = AstarPath.active.GetNearest(requestedAnchor);
                if (anchorPoint.node == null || !anchorPoint.node.Walkable ||
                    Vector3.Distance(requestedAnchor, anchorPoint.clampedPosition) > .6f)
                    throw new InvalidOperationException("Fascinate owned anchor has no surveyed walkable point.");
                PlaceExpandedSummoningUnit(anchor, anchorPoint.clampedPosition);
                PlaceExpandedSummoningUnit(owner, origin); PlaceExpandedSummoningUnit(target, targetPosition);
                PlaceExpandedSummoningUnit(fixture.Caster, origin + away); PlaceExpandedSummoningUnit(sibling, origin + away * 2f);
                if (globeEmitter != null)
                {
                    // Brain suspension does not disable native summon follow.
                    // Keep this owned probe within its idle caster's11.7m
                    // follow threshold, outside both tested actors' globe area.
                    var originNode = AstarPath.active.GetNearest(origin).node;
                    float radius = new Kingmaker.Utility.Feet(20).Meters;
                    foreach (float distance in new[] { 8f, 9f, 10f })
                    {
                        var requested = origin + away * distance;
                        var point = AstarPath.active.GetNearest(requested);
                        if (point.node == null || originNode == null || !point.node.Walkable ||
                            point.node.Area != originNode.Area || Vector3.Distance(requested, point.clampedPosition) > .6f ||
                            Vector3.Distance(fixture.Caster.Position, point.clampedPosition) >= 10f ||
                            Vector3.Distance(owner.Position, point.clampedPosition) - owner.Corpulence <= radius ||
                            Vector3.Distance(target.Position, point.clampedPosition) - target.Corpulence <= radius ||
                            Vector3.Distance(target.Position, point.clampedPosition) <= target.VisionRangeMeters + 2f ||
                            Vector3.Distance(sibling.Position, point.clampedPosition) <= sibling.Corpulence + globeEmitter.Corpulence + .25f) continue;
                        PlaceExpandedSummoningUnit(globeEmitter, point.clampedPosition);
                        globeParking = globeEmitter.Position; break;
                    }
                    if (!globeParking.HasValue) throw new InvalidOperationException("No bounded idle globe parking point.");
                }
                Func<bool> globeParked = () => globeEmitter == null || (globeParking.HasValue &&
                    !globeEmitter.Destroyed && !globeEmitter.IsInCombat && !globeEmitter.IsBrainActive &&
                    ReferenceEquals(globeEmitter.Get<UnitPartSummonedMonster>()?.Summoner, fixture.Caster) &&
                    Equals(globeEmitter.GroupId, fixture.Caster.GroupId) && globeEmitter.Commands.Empty && globeEmitter.Commands.Queue.Count == 0 &&
                    (globeEmitter.Get<UnitPartSummonedMonster>().MoveTo == null || globeEmitter.Get<UnitPartSummonedMonster>().MoveTo.IsFinished) &&
                    Vector3.Distance(globeEmitter.Position, globeParking.Value) < .1f &&
                    Vector3.Distance(globeEmitter.Position, fixture.Caster.Position) < 10f &&
                    globeEmitter.DistanceTo(target) > target.VisionRangeMeters && !target.Memory.Contains(globeEmitter));
                Func<JObject> globeState = () => globeEmitter == null ? null : new JObject {
                    ["unit"] = globeEmitter.UniqueId, ["position"] = new JArray(globeEmitter.Position.x, globeEmitter.Position.y, globeEmitter.Position.z),
                    ["summoner"] = globeEmitter.Get<UnitPartSummonedMonster>()?.Summoner?.UniqueId,
                    ["sameCasterGroup"] = Equals(globeEmitter.GroupId, fixture.Caster.GroupId),
                    ["casterDistance"] = Vector3.Distance(globeEmitter.Position, fixture.Caster.Position),
                    ["targetDistance"] = globeEmitter.DistanceTo(target), ["combat"] = globeEmitter.IsInCombat,
                    ["brain"] = globeEmitter.IsBrainActive, ["targetRemembersEmitter"] = target.Memory.Contains(globeEmitter),
                    ["emitterRemembersTarget"] = globeEmitter.Memory.Contains(target),
                    ["commands"] = new JArray(globeEmitter.Commands.Raw.Select(Sprint27AiCommand)),
                    ["nativeFollow"] = globeEmitter.Get<UnitPartSummonedMonster>()?.MoveTo == null ? null :
                        Sprint27AiCommand(globeEmitter.Get<UnitPartSummonedMonster>().MoveTo),
                    ["queueCount"] = globeEmitter.Commands.Queue.Count, ["parked"] = globeParked() };
                if (player.Party.Contains(fixture.Caster)) throw new InvalidOperationException("Caster unexpectedly in intake party.");
                player.PartyCharacters.Add(fixture.Caster); partyAdded = true;
                player.InvalidateCharacterLists(); player.UpdateCharacterLists();
                target.Stats.SaveWill.BaseValue = -100;
                const string prefix = "KMG_Summoning_FeyCelestial_LillendAzata_";
                var resource = fixture.Blueprints.OfType<BlueprintAbilityResource>().Single(value => value.name == prefix + "PerformanceResource");
                var effect = fixture.Blueprints.OfType<BlueprintBuff>().Single(value => value.name == prefix + "Fascinated");
                var resistance = fixture.Blueprints.OfType<BlueprintBuff>().Single(value => value.name == prefix + "FascinateResistance");
                var carrier = fixture.Blueprints.OfType<BlueprintBuff>().Single(value => value.name == prefix + "FascinateCarrier");
                var assistance = fixture.Blueprints.OfType<BlueprintBuff>().Single(value => value.name == prefix + "FascinateAssistance");
                var competenceCarrier = fixture.Blueprints.OfType<BlueprintBuff>().Single(value => value.name == prefix + "CompetenceCarrier");
                var competenceEffect = fixture.Blueprints.OfType<BlueprintBuff>().Single(value => value.name == prefix + "CompetenceRecipient");
                var toggle = owner.Descriptor.ActivatableAbilities.Enumerable.Single(value => ReferenceEquals(value.Blueprint.Buff, carrier));
                var competence = owner.Descriptor.ActivatableAbilities.Enumerable.Single(value => ReferenceEquals(value.Blueprint.Buff, competenceCarrier));
                var bookType = owner.Blueprint.GetComponent<LillendBardBook32>().Book;
                var book = owner.Descriptor.GetSpellbook(bookType); var siblingBook = sibling.Descriptor.GetSpellbook(bookType);
                var slots = Enumerable.Range(0, 4).Select(book.GetSpontaneousSlots).ToArray();
                var siblingSlots = Enumerable.Range(0, 4).Select(siblingBook.GetSpontaneousSlots).ToArray();
                var eligible = game.State.Units.All.Where(value =>
                    LillendFascinateEligibility32.Selectable(owner, value, effect, resistance)).ToArray();
                bool ready = Sprint32Suggestion.Settled && Sprint32Suggestion.Active == bardic &&
                    LillendNoncombat32.Find(owner) != null &&
                    eligible.Length == 1 && ReferenceEquals(eligible[0], target) &&
                    target.DistanceTo(owner) > target.VisionRangeMeters && target.DistanceTo(anchor) > target.VisionRangeMeters &&
                    target.DistanceTo(fixture.Caster) > target.VisionRangeMeters && anchor.DistanceTo(target) < anchor.VisionRangeMeters &&
                    LillendPerformanceSight32.Sees(anchor, target, 90) &&
                    Equals(owner.GroupId, fixture.Caster.GroupId) && owner.Group.IsPlayerParty &&
                    fixture.Caster.Group.IsPlayerParty && anchor.Group.IsPlayerParty && !target.Memory.Contains(owner) &&
                    !target.Memory.Contains(anchor) && !target.Memory.Contains(fixture.Caster) &&
                    !toggle.Blueprint.OnlyInCombat && owner.Descriptor.Resources.GetResourceAmount(resource) == 20 &&
                    sibling.Descriptor.Resources.GetResourceAmount(resource) == 20 && !player.IsInCombat && globeParked();
                check("fascinate-ready", ready, new JObject { ["survey"] = survey, ["direction"] = direction,
                    ["owner"] = owner.UniqueId, ["target"] = target.UniqueId, ["anchor"] = anchor.UniqueId,
                    ["distances"] = new JArray(target.DistanceTo(owner), target.DistanceTo(anchor), target.DistanceTo(fixture.Caster)),
                    ["vision"] = new JArray(owner.VisionRangeMeters, target.VisionRangeMeters, anchor.VisionRangeMeters),
                    ["sameCasterGroup"] = Equals(owner.GroupId, fixture.Caster.GroupId),
                    ["eligible"] = new JArray(eligible.Select(value => value.UniqueId)),
                    ["playerGroups"] = new JArray(owner.Group.IsPlayerParty, fixture.Caster.Group.IsPlayerParty, anchor.Group.IsPlayerParty),
                    ["hostileMemory"] = new JArray(target.Memory.Contains(owner), target.Memory.Contains(anchor), target.Memory.Contains(fixture.Caster)),
                    ["willBase"] = target.Stats.SaveWill.BaseValue, ["providerActive"] = Sprint32Suggestion.Active,
                    ["globeParking"] = globeState() },
                    "Natural physical visibility beyond enemy acquisition, shared summoned-owner group, no reciprocal hostile memory and independent resources; provider presence must match the fixed phase.");
                if (!ready) throw new InvalidOperationException("Fascinate calm geometry/capability preconditions failed.");
                starts = new Lillend32CommandStarts { Owner = owner }; saves = new Dretch30CloudSaves(new[] { target });
                events = new Astral31DispelEvents { Owner = owner }; events.Targets.Add(target);
                EventBus.Subscribe(starts); EventBus.Subscribe(saves); EventBus.Subscribe(events);
                var costs = new JArray();
                using (var economy = new FirearmReviewCosts(owner, costs))
                {
                    mode = Cleanup2EnterNativeDefault(fixture, phase);
                    var quietEnd = player.GameTime + TimeSpan.FromSeconds(7); var limit = DateTime.UtcNow.AddSeconds(20);
                    while (player.GameTime < quietEnd && DateTime.UtcNow < limit)
                    { if (!mode.Current) throw new InvalidOperationException("Fascinate native lease changed."); yield return 0; }
                    eligible = game.State.Units.All.Where(value =>
                        LillendFascinateEligibility32.Selectable(owner, value, effect, resistance)).ToArray();
                    bool calm = player.GameTime >= quietEnd && starts.Commands.Count == 0 && !player.IsInCombat &&
                        !owner.IsInCombat && !target.IsInCombat && !target.Memory.Contains(owner) &&
                        !target.Memory.Contains(anchor) && !target.Memory.Contains(fixture.Caster) &&
                        owner.Descriptor.Resources.GetResourceAmount(resource) == 20 &&
                        eligible.Length == 1 && ReferenceEquals(eligible[0], target) &&
                        owner.Group.IsPlayerParty && fixture.Caster.Group.IsPlayerParty && anchor.Group.IsPlayerParty && globeParked();
                    check("fascinate-calm-brain-refusal", calm, new JObject { ["commands"] = starts.Commands.Count,
                        ["combat"] = player.IsInCombat, ["targetCombat"] = target.IsInCombat,
                        ["eligible"] = new JArray(eligible.Select(value => value.UniqueId)),
                        ["playerGroups"] = new JArray(owner.Group.IsPlayerParty, fixture.Caster.Group.IsPlayerParty, anchor.Group.IsPlayerParty),
                        ["hostileMemory"] = new JArray(target.Memory.Contains(owner), target.Memory.Contains(anchor), target.Memory.Contains(fixture.Caster)),
                        ["gameSeconds"] = player.GameTime.TotalSeconds, ["globeParking"] = globeState() }, "Real Default time preserves calm without memory/combat edits; disabled brain supplies no performance.");
                    if (!calm) throw new InvalidOperationException("Fascinate natural calm boundary failed.");
                    using (var rounds = new Lillend32PerformanceRounds(toggle, resource, false))
                    {
                        Sprint28SetBrainActivePreservingActions(owner, true); limit = DateTime.UtcNow.AddSeconds(30);
                        // The provider phase stops after initial activation,
                        // before the next dispatcher round may cast Suggestion.
                        int requiredRounds = bardic ? 1 : 2;
                        while (DateTime.UtcNow < limit && (rounds.Rows.Count < requiredRounds ||
                            (bardic && !starts.Commands.OfType<UnitActivateAbility>().Any(value =>
                                ReferenceEquals(value.Ability, toggle) && value.IsFinished))))
                        {
                            if (!mode.Current || player.IsInCombat || owner.IsInCombat || target.IsInCombat || !globeParked())
                            {
                                check("fascinate-natural-boundary-refused", false, new JObject {
                                    ["mode"] = mode.Describe(), ["playerCombat"] = player.IsInCombat,
                                    ["ownerCombat"] = owner.IsInCombat, ["targetCombat"] = target.IsInCombat,
                                    ["commands"] = new JArray(starts.Commands.Select(Sprint27AiCommand)),
                                    ["commandEvents"] = starts.Rows, ["costs"] = costs, ["upkeep"] = rounds.Rows,
                                    ["saves"] = saves.Rows, ["globeParking"] = globeState() },
                                    "Native noncombat ownership and the exact parked globe probe must remain intact; record the failing boundary before teardown.");
                                throw new InvalidOperationException("Fascinate noncombat execution boundary changed.");
                            }
                            yield return 0;
                        }
                        Sprint28SetBrainActivePreservingActions(owner, false);
                        var commands = starts.Commands.OfType<UnitActivateAbility>().ToArray();
                        var grants = target.Buffs.Enumerable.Where(value => ReferenceEquals(value.Blueprint, effect) &&
                            ReferenceEquals(value.Context?.MaybeCaster, owner)).ToArray();
                        bool natural = globeParked() && commands.Length == 1 && ReferenceEquals(commands[0].Ability, toggle) &&
                            commands[0].IsActed && commands[0].IsFinished && commands[0].Result == UnitCommand.ResultType.Success &&
                            commands[0].Type == UnitCommand.CommandType.Move && !commands[0].Cutscene && !commands[0].IsIgnoreCooldown &&
                            economy.Count(commands[0]) == 1 && economy.Unchanged(commands[0]) &&
                            rounds.Rows.Count >= requiredRounds && rounds.Exact() && toggle.IsRunning && grants.Length == 1 &&
                            target.Descriptor.State.HasCondition(UnitCondition.Dazed) && target.Stats.SkillPerception.ModifiedValue == perception - 4 &&
                            saves.Rows.Count == 1 && (int)saves.Rows[0]["dc"] == 13 + owner.Stats.Charisma.Bonus && !(bool)saves.Rows[0]["passed"] &&
                            events.Resistances.Count == 0 && !LillendFascinateEligibility32.Selectable(owner, target, effect, resistance) &&
                            sibling.Descriptor.Resources.GetResourceAmount(resource) == 20 &&
                            Enumerable.Range(0, 4).Select(book.GetSpontaneousSlots).SequenceEqual(slots) &&
                            Enumerable.Range(0, 4).Select(siblingBook.GetSpontaneousSlots).SequenceEqual(siblingSlots);
                        check("fascinate-natural-performance", natural, new JObject { ["commands"] = new JArray(starts.Commands.Select(Sprint27AiCommand)),
                            ["costs"] = costs, ["upkeep"] = rounds.Rows, ["saves"] = saves.Rows, ["srChecks"] = events.Resistances.Count,
                            ["running"] = toggle.IsRunning, ["ownedEffects"] = grants.Length,
                            ["perception"] = perception + "->" + target.Stats.SkillPerception.ModifiedValue,
                            ["ownSlots"] = new JArray(Enumerable.Range(0,4).Select(book.GetSpontaneousSlots)),
                            ["siblingPool"] = sibling.Descriptor.Resources.GetResourceAmount(resource), ["globeParking"] = globeState(),
                            ["fixtureActivationCommands"] = 0, ["fixtureRoundCalls"] = 0, ["forcedRolls"] = 0 },
                            "One natural Move performance, native rounds, Will/noSR and owned Fascinated/-4 Perception; no supplied action, roll, effect or combat suppression.");
                        if (!natural) throw new InvalidOperationException("Fascinate natural control not established; preserve actual save outcome.");
                        if (combatTransition)
                        {
                            if (bardic) throw new InvalidOperationException("Bardic provider case does not select the combat transition.");
                            foreach (int frame in ReviewPerformanceCombat32(fixture, mode, owner, sibling,
                                toggle, effect, starts, economy, costs, true)) yield return frame;
                            complete = true; yield break;
                        }
                        if (bardic)
                        {
                            foreach (int frame in ReviewLillendBardicSuggestion32(fixture, mode, owner, sibling,
                                toggle, resource, effect, starts, events, economy, costs, rounds, globeEmitter)) yield return frame;
                            complete = true;
                            yield break;
                        }
                    }
                    toggle.IsOn = false;
                    bool immediate = !target.Buffs.Enumerable.Any(value => ReferenceEquals(value.Blueprint, effect)) &&
                        !game.State.Units.All.Any(unit => unit.Buffs.Enumerable.Any(value => ReferenceEquals(value.Blueprint, assistance) &&
                            ReferenceEquals(value.Context?.MaybeCaster, owner))) && target.Stats.SkillPerception.ModifiedValue == perception;
                    check("fascinate-cancel-effects", immediate, new JObject { ["running"] = toggle.IsRunning,
                        ["perception"] = target.Stats.SkillPerception.ModifiedValue },
                        "Explicit cancellation removes exact harmful effects/helpers immediately; native carrier retirement may finish at its next round.");
                    if (!immediate) throw new InvalidOperationException("Fascinate immediate cancellation cleanup failed.");
                    target.Descriptor.State.AddConditionImmunity(UnitCondition.Dazed); immunityAdded = true;
                    int commandsBefore = starts.Commands.Count, poolBefore = owner.Descriptor.Resources.GetResourceAmount(resource);
                    Sprint28SetBrainActivePreservingActions(owner, true); limit = DateTime.UtcNow.AddSeconds(22);
                    while (DateTime.UtcNow < limit && (!competence.IsRunning ||
                        !fixture.Caster.Buffs.Enumerable.Any(value => ReferenceEquals(value.Blueprint, competenceEffect)) ||
                        !starts.Commands.OfType<UnitActivateAbility>().Any(value => ReferenceEquals(value.Ability, competence) && value.IsFinished)))
                    { if (!mode.Current || player.IsInCombat) throw new InvalidOperationException("Fascinate fallback boundary changed."); yield return 0; }
                    Sprint28SetBrainActivePreservingActions(owner, false);
                    var fallback = starts.Commands.OfType<UnitActivateAbility>().Where(value => ReferenceEquals(value.Ability, competence)).ToArray();
                    bool refused = !LillendFascinateEligibility32.Selectable(owner, target, effect, resistance) && !toggle.IsRunning &&
                        !target.Buffs.Enumerable.Any(value => ReferenceEquals(value.Blueprint, effect)) && fallback.Length == 1 &&
                        fallback[0].IsActed && fallback[0].IsFinished && fallback[0].Result == UnitCommand.ResultType.Success &&
                        fallback[0].Type == UnitCommand.CommandType.Move && !fallback[0].Cutscene && !fallback[0].IsIgnoreCooldown &&
                        economy.Count(fallback[0]) == 1 && economy.Unchanged(fallback[0]) &&
                        fixture.Caster.Stats.SkillPerception.ModifiedValue == allyPerception + 3 &&
                        owner.Descriptor.Resources.GetResourceAmount(resource) < poolBefore && sibling.Descriptor.Resources.GetResourceAmount(resource) == 20;
                    check("fascinate-immune-support-fallback", refused, new JObject { ["commandsBefore"] = commandsBefore,
                        ["commands"] = new JArray(starts.Commands.Select(Sprint27AiCommand)), ["costs"] = costs,
                        ["pool"] = poolBefore + "->" + owner.Descriptor.Resources.GetResourceAmount(resource),
                        ["competenceRunning"] = competence.IsRunning, ["allyPerception"] = fixture.Caster.Stats.SkillPerception.ModifiedValue },
                        "With the owned hostile now explicitly Dazed-immune, natural dispatch refuses control and uses ordinary Competence on the party ally.");
                    if (!refused) throw new InvalidOperationException("Fascinate immune refusal/support fallback failed.");
                }
                // Fresh sources keep the boundary cases independent of the
                // preceding performance pool and original summon deadline.
                target.Descriptor.State.RemoveConditionImmunity(UnitCondition.Dazed); immunityAdded = false;
                DisposeExpandedSummoningUnits(fixture.Created, new[] { owner, sibling });
                foreach (int frame in ReviewLillendFascinateBoundaries32(fixture, mode, origin, targetPosition)) yield return frame;
                complete = true;
            }
            finally
            {
                Action<string, Action> restore = (name, action) => { try { action(); } catch (Exception error) { cleanup.Add(name + ":" + error.Message); } };
                if (starts != null) EventBus.Unsubscribe(starts); if (saves != null) EventBus.Unsubscribe(saves); if (events != null) EventBus.Unsubscribe(events);
                restore("target", () => { target.Stats.SaveWill.BaseValue = will;
                    if (immunityAdded) target.Descriptor.State.RemoveConditionImmunity(UnitCondition.Dazed); });
                restore("summons", () => DisposeExpandedSummoningUnits(fixture.Created, new[] { owner, sibling, globeEmitter }.Where(value => value != null && !value.Destroyed).ToArray()));
                restore("native-mode", () => mode?.Dispose());
                restore("party", () => { if (partyAdded) { player.PartyCharacters.Remove(fixture.Caster); player.InvalidateCharacterLists(); player.UpdateCharacterLists(); }
                    if (!player.Party.SequenceEqual(party)) throw new InvalidOperationException("Exact original party differs."); });
                restore("caster", () => { PlaceExpandedSummoningUnit(fixture.Caster, casterPosition); Sprint28SetBrainActivePreservingActions(fixture.Caster, casterBrain); });
                restore("anchor", () => { PlaceExpandedSummoningUnit(anchor, anchorPosition); Sprint28SetBrainActivePreservingActions(anchor, anchorBrain); });
                check("fascinate-finalizers", cleanup.Count == 0, new JObject { ["errors"] = new JArray(cleanup), ["complete"] = complete },
                    "Owned fixture/Default/party/positions restored; original gameplay failure retained.");
                if (complete && cleanup.Count != 0) throw new InvalidOperationException(string.Join("|", cleanup.ToArray()));
            }
        }
    }
}
