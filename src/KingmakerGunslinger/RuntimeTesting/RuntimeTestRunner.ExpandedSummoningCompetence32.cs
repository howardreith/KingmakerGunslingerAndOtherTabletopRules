using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> ReviewLillendCompetence32(ExpandedSummoningCorrectionFixture fixture)
        {
            const string phase = ExpandedSummoningNativeProfilePolicy.Sprint32PerformancePhase;
            if (!IsSprint32Review || !_request.ExitAfterCompletion || _request.Parameters?.Count != 1 ||
                RuntimeTestRequestParser.Sprint32ReviewPhase(_request) != phase)
                throw new InvalidOperationException("Exact save-free performance phase required.");
            var game = Game.Instance; var player = game.Player; var party = player.Party.ToArray();
            var anchor = player.MainCharacter.Value;
            bool anchorOwned = anchor != null && _summoningProfileActor != null && _summoningProfilePreset != null &&
                _summoningProfileOriginal != null && ReferenceEquals(anchor.Blueprint, _summoningProfileActor) &&
                ReferenceEquals(_summoningProfilePreset.PlayerCharacter, _summoningProfileActor) &&
                !ReferenceEquals(_summoningProfileOriginal.PlayerCharacter, _summoningProfileActor) &&
                _summoningProfileActor.name == "KMG_Runtime_Summoning_Profile_Anchor" &&
                _summoningProfileSaveGuard != null && _summoningProfileScenes != null &&
                party.Length == 1 && ReferenceEquals(party[0], anchor) && anchor.Commands.Empty;
            Sprint25Check("performance-owned-anchor", anchorOwned, new JObject {
                ["anchor"] = anchor?.UniqueId, ["exactIdleProfileAnchor"] = anchorOwned },
                "Only the existing save-free cloned anchor may have its idle brain suspended.");
            if (!anchorOwned) throw new InvalidOperationException("Performance anchor ownership ambiguous.");
            bool anchorBrain = anchor.IsBrainActive, casterBrain = fixture.Caster.IsBrainActive;
            Vector3 casterPosition = fixture.Caster.Position;
            int casterPerception = fixture.Caster.Stats.SkillPerception.ModifiedValue;
            int anchorPerception = anchor.Stats.SkillPerception.ModifiedValue;
            UnitEntityData owner = null, sibling = null; bool partyAdded = false, complete = false;
            bool recipientBlindnessAdded = false;
            Cleanup2NativeDefaultScope mode = null;
            Lillend32CommandStarts starts = null;
            var cleanup = new List<string>();
            try
            {
                Sprint28SetBrainActivePreservingActions(anchor, false);
                Sprint28SetBrainActivePreservingActions(fixture.Caster, false);
                owner = CastExpandedSummoningOwnTier(fixture, "lillend-azata");
                sibling = CastExpandedSummoningOwnTier(fixture, "lillend-azata");
                foreach (var unit in new[] { fixture.Caster, owner, sibling })
                {
                    Sprint28SetBrainActivePreservingActions(unit, false);
                    unit.Commands.InterruptAll(true); unit.Commands.RemoveFinishedAndUpdateQueue();
                    unit.Wake(0f); if (!game.State.AwakeUnits.Contains(unit)) game.State.AwakeUnits.Add(unit);
                }
                Vector3 targetPosition; string survey, direction;
                var origin = Sprint24AiCombatLane(fixture.Caster, owner, fixture.Caster, 3f,
                    out targetPosition, out survey, out direction, originAllyExclusionRadius: 10f);
                PlaceExpandedSummoningUnit(owner, origin); PlaceExpandedSummoningUnit(fixture.Caster, targetPosition);
                PlaceExpandedSummoningUnit(sibling, origin + Vector3.right * 2f);
                if (player.Party.Contains(fixture.Caster)) throw new InvalidOperationException("Caster unexpectedly in intake party.");
                player.PartyCharacters.Add(fixture.Caster); partyAdded = true;
                player.InvalidateCharacterLists(); player.UpdateCharacterLists();
                const string prefix = "KMG_Summoning_FeyCelestial_LillendAzata_";
                var resource = fixture.Blueprints.OfType<BlueprintAbilityResource>().Single(value => value.name == prefix + "PerformanceResource");
                var recipient = fixture.Blueprints.OfType<BlueprintBuff>().Single(value => value.name == prefix + "CompetenceRecipient");
                var carrier = fixture.Blueprints.OfType<BlueprintBuff>().Single(value => value.name == prefix + "CompetenceCarrier");
                var dispatcher = fixture.Blueprints.OfType<BlueprintBuff>().Single(value => value.name == prefix + "NoncombatDispatcher");
                var dispatchFacts = owner.Buffs.Enumerable.Where(value => ReferenceEquals(value.Blueprint, dispatcher)).ToArray();
                var toggle = owner.Descriptor.ActivatableAbilities.Enumerable.Single(value => ReferenceEquals(value.Blueprint.Buff, carrier));
                var bookBlueprint = owner.Blueprint.GetComponent<LillendBardBook32>().Book;
                var book = owner.Descriptor.GetSpellbook(bookBlueprint); var siblingBook = sibling.Descriptor.GetSpellbook(bookBlueprint);
                var ownSlots = Enumerable.Range(0, 4).Select(book.GetSpontaneousSlots).ToArray();
                var siblingSlots = Enumerable.Range(0, 4).Select(siblingBook.GetSpontaneousSlots).ToArray();
                var eligible = player.Party.Where(value => LillendNoncombat32.Selectable(owner, value)).ToArray();
                bool ready = eligible.Length == 1 && ReferenceEquals(eligible[0], fixture.Caster) &&
                    dispatchFacts.Length == 1 && dispatchFacts[0].Active && dispatchFacts[0].IsNotDispelable &&
                    !dispatchFacts[0].IsFromSpell && LillendNoncombat32.Find(owner) != null &&
                    dispatcher.Frequency == Kingmaker.UnitLogic.Mechanics.DurationRate.Rounds && !toggle.Blueprint.OnlyInCombat &&
                    !fixture.Caster.Descriptor.State.HasCondition(Kingmaker.UnitLogic.UnitCondition.Blindness) &&
                    !LillendNoncombat32.Eligible(owner, owner) && !LillendNoncombat32.Eligible(owner, sibling) &&
                    owner.Descriptor.Resources.GetResourceAmount(resource) == 20 &&
                    sibling.Descriptor.Resources.GetResourceAmount(resource) == 20 &&
                    !player.IsInCombat && !owner.IsInCombat && !fixture.Caster.IsInCombat && !sibling.IsInCombat;
                Sprint25Check("competence-ready", ready, new JObject { ["survey"] = survey, ["direction"] = direction,
                    ["eligible"] = new JArray(eligible.Select(value => value.UniqueId)), ["owner"] = owner.UniqueId,
                    ["sibling"] = sibling.UniqueId, ["recipient"] = fixture.Caster.UniqueId,
                    ["distance"] = owner.DistanceTo(fixture.Caster), ["ownSlots"] = new JArray(ownSlots),
                    ["siblingSlots"] = new JArray(siblingSlots), ["selfRefused"] = !LillendNoncombat32.Eligible(owner, owner),
                    ["dispatcherFacts"] = dispatchFacts.Length, ["dispatcherBound"] = LillendNoncombat32.Find(owner) != null,
                    ["dispatcherFrequency"] = dispatcher.Frequency.ToString(),
                    ["onlyInCombat"] = toggle.Blueprint.OnlyInCombat, ["available"] = toggle.IsAvailable,
                    ["queueEmpty"] = owner.Commands.Empty,
                    ["nonpartyRefused"] = !LillendNoncombat32.Eligible(owner, sibling) },
                    "One conscious visible party recipient in the surveyed30ft range; independent native20-round pools and untouched books.");
                if (!ready) throw new InvalidOperationException("Competence preconditions failed.");
                starts = new Lillend32CommandStarts { Owner = owner }; EventBus.Subscribe(starts);
                var costs = new JArray();
                using (var economy = new FirearmReviewCosts(owner, costs))
                using (var rounds = new Lillend32PerformanceRounds(toggle, resource, false))
                {
                    mode = Cleanup2EnterNativeDefault(fixture, phase);
                    var quietEnd = game.Player.GameTime + TimeSpan.FromSeconds(7);
                    DateTime limit = DateTime.UtcNow.AddSeconds(20);
                    while (game.Player.GameTime < quietEnd && DateTime.UtcNow < limit)
                    { if (!mode.Current) throw new InvalidOperationException("Native performance lease changed."); yield return 0; }
                    bool quiet = game.Player.GameTime >= quietEnd && starts.Commands.Count == 0 && !toggle.IsOn &&
                        owner.Descriptor.Resources.GetResourceAmount(resource) == 20 && rounds.Rows.Count == 0;
                    Sprint25Check("competence-brain-refusal", quiet, new JObject { ["commands"] = starts.Commands.Count,
                        ["brainActive"] = owner.IsBrainActive, ["rounds"] = rounds.Rows.Count,
                        ["performance"] = owner.Descriptor.Resources.GetResourceAmount(resource) },
                        "Real outside-combat time passes with native AI disabled; dispatcher supplies no activation or cost.");
                    if (!quiet) throw new InvalidOperationException("Competence brain-disabled refusal failed.");
                    Sprint28SetBrainActivePreservingActions(owner, true);
                    limit = DateTime.UtcNow.AddSeconds(30);
                    while (DateTime.UtcNow < limit && rounds.Rows.Count < 2)
                    {
                        if (!mode.Current || player.IsInCombat || owner.IsInCombat || fixture.Caster.IsInCombat)
                            throw new InvalidOperationException("Natural noncombat performance boundary changed.");
                        yield return 0;
                    }
                    // Reads below are synchronous on the game thread. A Pause
                    // mode is unnecessary and would escape the Default lease
                    // when a failed assertion throws before unpausing.
                    Sprint28SetBrainActivePreservingActions(owner, false);
                    var commands = starts.Commands.OfType<UnitActivateAbility>().ToArray();
                    var grants = fixture.Caster.Buffs.Enumerable.Where(value => ReferenceEquals(value.Blueprint, recipient)).ToArray();
                    bool acted = commands.Length == 1 && ReferenceEquals(commands[0].Ability, toggle) && commands[0].IsActed &&
                        commands[0].IsFinished && commands[0].Result == UnitCommand.ResultType.Success &&
                        commands[0].Type == UnitCommand.CommandType.Move && !commands[0].Cutscene && !commands[0].IsIgnoreCooldown &&
                        economy.Count(commands[0]) == 1 && economy.Unchanged(commands[0]) &&
                        rounds.Rows.Count >= 2 && rounds.Exact() && toggle.IsRunning && grants.Length == 1 &&
                        ReferenceEquals(grants[0].Context?.MaybeCaster, owner) && !string.IsNullOrEmpty(grants[0].SourceAreaEffectId) &&
                        fixture.Caster.Stats.SkillPerception.ModifiedValue == casterPerception + 3 &&
                        !owner.Buffs.Enumerable.Any(value => ReferenceEquals(value.Blueprint, recipient)) &&
                        !sibling.Buffs.Enumerable.Any(value => ReferenceEquals(value.Blueprint, recipient)) &&
                        anchor.Stats.SkillPerception.ModifiedValue == anchorPerception &&
                        sibling.Descriptor.Resources.GetResourceAmount(resource) == 20 &&
                        Enumerable.Range(0, 4).Select(book.GetSpontaneousSlots).SequenceEqual(ownSlots) &&
                        Enumerable.Range(0, 4).Select(siblingBook.GetSpontaneousSlots).SequenceEqual(siblingSlots);
                    Sprint25Check("competence-natural-performance", acted, new JObject {
                        ["commands"] = new JArray(starts.Commands.Select(Sprint27AiCommand)), ["starts"] = starts.Rows,
                        ["costs"] = costs, ["upkeep"] = rounds.Rows, ["nativeUpkeepExact"] = rounds.Exact(),
                        ["running"] = toggle.IsRunning, ["recipientBuffs"] = grants.Length,
                        ["perceptionBefore"] = casterPerception, ["perceptionAfter"] = fixture.Caster.Stats.SkillPerception.ModifiedValue,
                        ["siblingPool"] = sibling.Descriptor.Resources.GetResourceAmount(resource),
                        ["queue"] = new JArray(owner.Commands.Raw.Where(value => value != null).Select(Sprint27AiCommand)),
                        ["available"] = toggle.IsAvailable, ["selectedStillEligible"] = LillendNoncombat32.Eligible(owner, fixture.Caster),
                        ["gameSeconds"] = player.GameTime.TotalSeconds,
                        ["fixtureActivationCommands"] = 0, ["fixtureRoundCalls"] = 0, ["fixtureEffects"] = 0 },
                        "Native Default/round controllers choose and execute one normal Move activation and spend one native round per upkeep. Exploration has no combat cooldown debit; no manual round/cast/effect supplies this result.");
                    if (!acted) throw new InvalidOperationException("Natural Competence performance failed.");
                    int roundsBeforeBlindness = rounds.Rows.Count;
                    fixture.Caster.Descriptor.State.AddCondition(Kingmaker.UnitLogic.UnitCondition.Blindness);
                    recipientBlindnessAdded = true; limit = DateTime.UtcNow.AddSeconds(8);
                    while (DateTime.UtcNow < limit && rounds.Rows.Count == roundsBeforeBlindness)
                    {
                        if (!mode.Current) throw new InvalidOperationException("Audible performance lease changed.");
                        yield return 0;
                    }
                    bool audible = rounds.Rows.Count > roundsBeforeBlindness && rounds.Exact() && toggle.IsRunning &&
                        fixture.Caster.Descriptor.State.HasCondition(Kingmaker.UnitLogic.UnitCondition.Blindness) &&
                        fixture.Caster.Buffs.Enumerable.Any(value => ReferenceEquals(value, grants[0])) &&
                        fixture.Caster.Stats.SkillPerception.ModifiedValue == casterPerception + 3;
                    Sprint25Check("competence-audible-only-maintenance", audible, new JObject {
                        ["blind"] = fixture.Caster.Descriptor.State.HasCondition(Kingmaker.UnitLogic.UnitCondition.Blindness),
                        ["sameGrant"] = fixture.Caster.Buffs.Enumerable.Any(value => ReferenceEquals(value, grants[0])),
                        ["running"] = toggle.IsRunning, ["roundsBefore"] = roundsBeforeBlindness,
                        ["roundsAfter"] = rounds.Rows.Count, ["nativeUpkeepExact"] = rounds.Exact(),
                        ["perception"] = fixture.Caster.Stats.SkillPerception.ModifiedValue },
                        "The already selected blind recipient retains the same audible-only Competence grant through a natural upkeep; native hearing/deafness is not separately modeled.");
                    if (!audible) throw new InvalidOperationException("Audible-only Competence maintenance failed.");
                    fixture.Caster.Descriptor.State.RemoveCondition(Kingmaker.UnitLogic.UnitCondition.Blindness);
                    recipientBlindnessAdded = false;
                    PlaceExpandedSummoningUnit(fixture.Caster, owner.Position + (fixture.Caster.Position - owner.Position).normalized * 12f);
                    // Native WithUnitCommand Stop(false) retires its carrier at
                    // TimeToNextRound. The recipient may already have left the
                    // area; wait for every asserted terminal state, not just it.
                    limit = DateTime.UtcNow.AddSeconds(16);
                    while (DateTime.UtcNow < limit && (toggle.IsOn || toggle.IsRunning ||
                        owner.Buffs.Enumerable.Any(value => ReferenceEquals(value.Blueprint, carrier)) ||
                        fixture.Caster.Buffs.Enumerable.Any(value => ReferenceEquals(value.Blueprint, recipient)))) yield return 0;
                    bool stopped = !toggle.IsOn && !toggle.IsRunning &&
                        !owner.Buffs.Enumerable.Any(value => ReferenceEquals(value.Blueprint, carrier)) &&
                        !fixture.Caster.Buffs.Enumerable.Any(value => ReferenceEquals(value.Blueprint, recipient)) &&
                        fixture.Caster.Stats.SkillPerception.ModifiedValue == casterPerception &&
                        sibling.Descriptor.Resources.GetResourceAmount(resource) == 20;
                    Sprint25Check("competence-recipient-loss", stopped, new JObject {
                        ["distance"] = owner.DistanceTo(fixture.Caster), ["on"] = toggle.IsOn, ["running"] = toggle.IsRunning,
                        ["carriers"] = owner.Buffs.Enumerable.Count(value => ReferenceEquals(value.Blueprint, carrier)),
                        ["recipientBuffs"] = fixture.Caster.Buffs.Enumerable.Count(value => ReferenceEquals(value.Blueprint, recipient)),
                        ["perception"] = fixture.Caster.Stats.SkillPerception.ModifiedValue,
                        ["ownPool"] = owner.Descriptor.Resources.GetResourceAmount(resource),
                        ["siblingPool"] = sibling.Descriptor.Resources.GetResourceAmount(resource) },
                        "Moving only the disposable recipient beyond30ft ends the exact owned performance and grant without retargeting; this is a range/lifecycle fixture, not movement-animation proof.");
                    if (!stopped) throw new InvalidOperationException("Competence recipient-loss cleanup failed.");
                    complete = true;
                }
            }
            finally
            {
                Action<string, Action> restore = (name, action) => { try { action(); } catch (Exception error) { cleanup.Add(name + ":" + error.Message); } };
                if (starts != null) EventBus.Unsubscribe(starts);
                restore("recipient-condition", () => { if (recipientBlindnessAdded)
                    fixture.Caster.Descriptor.State.RemoveCondition(Kingmaker.UnitLogic.UnitCondition.Blindness); });
                restore("summons", () => DisposeExpandedSummoningUnits(fixture.Created,
                    new[] { owner, sibling }.Where(value => value != null && !value.Destroyed).ToArray()));
                restore("native-mode", () => mode?.Dispose());
                restore("party", () => { if (partyAdded) { player.PartyCharacters.Remove(fixture.Caster);
                    player.InvalidateCharacterLists(); player.UpdateCharacterLists(); }
                    if (!player.Party.SequenceEqual(party)) throw new InvalidOperationException("Exact original party differs."); });
                restore("caster", () => { PlaceExpandedSummoningUnit(fixture.Caster, casterPosition);
                    Sprint28SetBrainActivePreservingActions(fixture.Caster, casterBrain); });
                restore("anchor", () => { if (anchor.Destroyed || !ReferenceEquals(anchor.Blueprint, _summoningProfileActor))
                    throw new InvalidOperationException("Owned anchor changed.");
                    Sprint28SetBrainActivePreservingActions(anchor, anchorBrain); });
                Sprint25Check("competence-finalizers", cleanup.Count == 0, new JObject {
                    ["errors"] = new JArray(cleanup), ["complete"] = complete,
                    ["originalFailurePreserved"] = !complete }, "Exact owned fixture and native Default restoration; original gameplay failure is retained.");
                if (complete && cleanup.Count != 0) throw new InvalidOperationException(string.Join("|", cleanup.ToArray()));
            }
        }
    }
}
