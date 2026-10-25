using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Controllers.Brain;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Utility;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // One bounded provider case per adult, on the existing owned fixture.
        // Low Will and other-control immunity are declared target preconditions;
        // rolls, chosen commands, costs, slots and the effect remain native.
        private IEnumerable<int> ReviewSuggestion32(ExpandedSummoningCorrectionFixture fixture, bool lillend)
        {
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue, oldPause = Game.Instance.IsPaused;
            UnitEntityData owner = null, sibling = null;
            var target = fixture.Hostile;
            int oldWill = target.Stats.SaveWill.BaseValue;
            BlueprintFeature charmImmunity = null;
            var addedConditions = new List<UnitCondition>();
            string key = lillend ? "lillend-azata" : "satyr";
            string prefix = "KMG_Summoning_FeyCelestial_" + (lillend ? "LillendAzata" : "Satyr") + "_";
            string id = key + "-suggestion";
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;
                if (!Sprint32Suggestion.Settled || !Sprint32Suggestion.Active)
                    throw new InvalidOperationException("Suggestion execution requires the settled investigated provider: " + Sprint32Suggestion.Disposition);
                owner = CastExpandedSummoningOwnTier(fixture, key); sibling = CastExpandedSummoningOwnTier(fixture, key);
                foreach (var unit in new[] { owner, sibling })
                { Sprint28SetBrainActivePreservingActions(unit, false); unit.Commands.InterruptAll(true); unit.Commands.RemoveFinishedAndUpdateQueue(); }
                var ability = fixture.Blueprints.OfType<BlueprintAbility>().Single(value => value.name == prefix + "Suggestion");
                var resource = fixture.Blueprints.OfType<BlueprintAbilityResource>().Single(value =>
                    value.name == prefix + (lillend ? "PerformanceResource" : "FearResource"));
                int resourceBefore = owner.Descriptor.Resources.GetResourceAmount(resource);
                if (resourceBefore != (lillend ? 20 : 1)) throw new InvalidOperationException("Suggestion fixture requires a fresh independent pool.");
                owner.Descriptor.Resources.Spend(resource, resourceBefore);
                Spellbook book = null, siblingBook = null;
                var setupSpends = new JArray();
                if (lillend)
                {
                    var creation = owner.Blueprint.GetComponent<LillendBardBook32>();
                    book = owner.Descriptor.GetSpellbook(creation.Book); siblingBook = sibling.Descriptor.GetSpellbook(creation.Book);
                    foreach (int level in new[] { 1, 3 })
                    {
                        var known = book.GetKnownSpells(level).First();
                        int before = book.GetSpontaneousSlots(level);
                        for (int index = 0; index < before; index++) known.Spend();
                        setupSpends.Add(new JObject { ["level"] = level, ["before"] = before, ["after"] = book.GetSpontaneousSlots(level) });
                        if (book.GetSpontaneousSlots(level) != 0) throw new InvalidOperationException("Native fixture slot pre-spend failed.");
                    }
                }
                var component = ScriptableObject.CreateInstance<BuffDescriptorImmunity>(); component.Descriptor = SpellDescriptor.Charm;
                charmImmunity = ScriptableObject.CreateInstance<BlueprintFeature>(); charmImmunity.Ranks = 1;
                charmImmunity.name = "KMG_Runtime_Suggestion32_CharmOnlyImmunity";
                charmImmunity.ComponentsArray = new BlueprintComponent[] { component }; target.Descriptor.AddFact(charmImmunity);
                foreach (var condition in new[] { UnitCondition.Sleeping, UnitCondition.Paralyzed, UnitCondition.Stunned })
                {
                    if (target.Descriptor.State.HasConditionImmunity(condition)) throw new InvalidOperationException("Unexpected target immunity: " + condition);
                    target.Descriptor.State.AddConditionImmunity(condition); addedConditions.Add(condition);
                }
                target.Stats.SaveWill.BaseValue = -100;
                Vector3 far; string lane, direction;
                var origin = Sprint24AiCombatLane(fixture.Caster, owner, target, 8f, out far, out lane, out direction);
                PlaceExpandedSummoningUnit(owner, origin); PlaceExpandedSummoningUnit(target, far);
                PlaceExpandedSummoningUnit(sibling, origin + Vector3.right * 2f);
                foreach (var unit in new[] { owner, target })
                { unit.Wake(0f); if (!Game.Instance.State.AwakeUnits.Contains(unit)) Game.Instance.State.AwakeUnits.Add(unit); }
                owner.Memory.Add(target); target.Memory.Add(owner);
                owner.SightCache.ClearIfNeeded(); target.SightCache.ClearIfNeeded(); new Kingmaker.Controllers.Units.UnitMemoryController().Tick();
                owner.JoinCombat(); target.JoinCombat(); Game.Instance.Player.UpdateIsInCombat();
                new UnitCombatJoinController().Tick(); new UnitCombatPrepareController().Tick();
                var context = new DecisionContext { Unit = owner, Target = new TargetInfo { Unit = target } };
                var satyrChoice = fixture.Blueprints.OfType<SatyrControlEligibility>().SingleOrDefault(value =>
                    ReferenceEquals(value.OwningBlueprint, owner.Blueprint) && ReferenceEquals(value.Ability, ability));
                var lillendChoice = fixture.Blueprints.OfType<LillendSpellEligibility32>().SingleOrDefault(value =>
                    ReferenceEquals(value.OwningBlueprint, owner.Blueprint) && ReferenceEquals(value.Spell, ability));
                Func<float> score = () => lillend ? lillendChoice.Score(context) : satyrChoice.Score(context);
                var data = lillend ? new AbilityData(ability, book) : new AbilityData(owner.Descriptor.Abilities.GetAbility(ability));
                var parameters = data.CalculateParams();
                bool ready = data.IsAvailable && data.CanTarget(new TargetWrapper(target)) && score() == 1f &&
                    owner.Descriptor.State.CanAct && owner.IsEnemy(target) && owner.HasLOS(target) && owner.Memory.ContainsVisible(target) &&
                    parameters.CasterLevel == (lillend ? 7 : 8) && parameters.DC == (lillend ? 16 : 17) &&
                    owner.Descriptor.Resources.GetResourceAmount(resource) == 0 && sibling.Descriptor.Resources.GetResourceAmount(resource) == resourceBefore &&
                    (!lillend || book.GetSpontaneousSlots(2) == 4 && siblingBook.GetSpontaneousSlots(2) == 4);
                Sprint25Check(id + "-ready", ready, new JObject { ["owner"] = owner.UniqueId, ["sibling"] = sibling.UniqueId,
                    ["target"] = target.UniqueId, ["ability"] = ability.AssetGuid, ["lane"] = lane,
                    ["cl"] = parameters.CasterLevel, ["dc"] = parameters.DC, ["spellLevel"] = parameters.SpellLevel,
                    ["fixtureWillBase"] = -100, ["fixtureCharmImmune"] = true, ["fixtureConditionImmunity"] = new JArray(addedConditions.Select(value => value.ToString())),
                    ["fixturePoolSpent"] = resourceBefore, ["fixtureSlotPreSpends"] = setupSpends, ["suggestionScore"] = score() },
                    "Provider-bound private Suggestion is eligible on the exact adult; other controls and performance have explicit fixture preconditions, with native slots and sibling pools retained.");
                if (!ready) throw new InvalidOperationException("Suggestion native fixture preconditions failed.");
                var commands = new HashSet<UnitCommand>(); var costs = new JArray();
                var starts = new Lillend32CommandStarts { Owner = owner };
                var events = new Astral31DispelEvents { Owner = owner }; events.Targets.Add(target);
                EventBus.Subscribe(starts); EventBus.Subscribe(events);
                try
                {
                    Sprint28SetBrainActivePreservingActions(owner, true); owner.Brain.RestoreAvailableActions();
                    using (var economy = new FirearmReviewCosts(owner, costs))
                    {
                        DateTime deadline = DateTime.UtcNow.AddSeconds(55); int frames = 0, ticks = 0;
                        Buff effect = null;
                        while (DateTime.UtcNow < deadline && frames++ < 5000)
                        {
                            foreach (var command in owner.Commands.Raw.Where(value => value != null)) commands.Add(command);
                            foreach (var command in starts.Commands) commands.Add(command);
                            effect = target.Descriptor.Buffs.RawFacts.OfType<Buff>().SingleOrDefault(value =>
                                value.Blueprint.AssetGuid == Sprint32Suggestion.OwnedBuffGuid && ReferenceEquals(value.Context?.MaybeCaster, owner));
                            var casts = commands.OfType<UnitUseAbility>().Where(value => value.AiAction?.name == prefix + "SuggestionAi" &&
                                value.IsFinished && value.ExecutionProcess != null && value.ExecutionProcess.IsEnded).ToArray();
                            if (effect != null && casts.Length > 0 || casts.Length >= 3) break;
                            if (frames == 1 || frames % 60 == 0 && !owner.Commands.Raw.Any(value => value != null && !value.IsFinished))
                            { Sprint24TickExactAi(owner); ticks++; }
                            Game.Instance.IsPaused = false; yield return 0;
                        }
                        Sprint28SetBrainActivePreservingActions(owner, false);
                        var resolved = commands.OfType<UnitUseAbility>().Where(value => value.AiAction?.name == prefix + "SuggestionAi" &&
                            value.IsFinished && value.Result == UnitCommand.ResultType.Success && value.ExecutionProcess != null && value.ExecutionProcess.IsEnded).ToArray();
                        var saves = events.Saves.Where(value => value.Reason?.Context?.SourceAbility?.AssetGuid == ability.AssetGuid).ToArray();
                        bool exact = resolved.Length > 0 && resolved.Length <= 3 && resolved.All(value => economy.Count(value) == 1 && economy.Exact(value, false) &&
                            value.Type == UnitCommand.CommandType.Standard && !value.Cutscene && !value.IsIgnoreCooldown) &&
                            saves.Length == resolved.Length && saves.All(value => value.DifficultyClass == (lillend ? 16 : 17)) && saves.Any(value => !value.IsPassed) &&
                            effect != null && effect.Context.Params.CasterLevel == (lillend ? 7 : 8) && effect.Context.Params.DC == (lillend ? 16 : 17) &&
                            effect.TimeLeft.TotalSeconds > (lillend ? 7 : 8) * 60 - 8 && effect.TimeLeft.TotalSeconds <= (lillend ? 7 : 8) * 60 + .1 &&
                            target.Descriptor.State.HasCondition(UnitCondition.Dazed) && score() == 0f &&
                            sibling.Descriptor.Resources.GetResourceAmount(resource) == resourceBefore && owner.Descriptor.Resources.GetResourceAmount(resource) == 0 &&
                            (!lillend || book.GetSpontaneousSlots(2) == 4 - resolved.Length && siblingBook.GetSpontaneousSlots(2) == 4 &&
                                book.GetSpontaneousSlots(1) == 0 && book.GetSpontaneousSlots(3) == 0);
                        Sprint25Check(id + "-resolved", exact, new JObject { ["owner"] = owner.UniqueId, ["target"] = target.UniqueId,
                            ["commands"] = new JArray(commands.Select(Sprint27AiCommand)), ["costs"] = costs, ["nativeStarts"] = starts.Rows,
                            ["frames"] = frames, ["aiTicks"] = ticks, ["nativeCasts"] = resolved.Length,
                            ["saves"] = new JArray(saves.Select(value => new JObject { ["dc"] = value.DifficultyClass, ["passed"] = value.IsPassed,
                                ["source"] = value.Reason.Context.SourceAbility.AssetGuid, ["caster"] = value.Reason.Context.MaybeCaster.UniqueId })),
                            ["resistanceChecks"] = new JArray(events.Resistances.Select(Astral31ResistanceRow)),
                            ["effectCl"] = effect?.Context.Params.CasterLevel, ["effectDc"] = effect?.Context.Params.DC,
                            ["effectSeconds"] = effect?.TimeLeft.TotalSeconds, ["targetDazed"] = target.Descriptor.State.HasCondition(UnitCondition.Dazed),
                            ["activeEffectScore"] = score(), ["ownLevel2Slots"] = book?.GetSpontaneousSlots(2), ["siblingLevel2Slots"] = siblingBook?.GetSpontaneousSlots(2),
                            ["ownOtherPool"] = owner.Descriptor.Resources.GetResourceAmount(resource), ["siblingOtherPool"] = sibling.Descriptor.Resources.GetResourceAmount(resource),
                            ["fixtureCastCommands"] = 0, ["fixtureEffectApplications"] = 0, ["forcedRolls"] = 0,
                            ["lifetimeExpiryAndDamageBreakQualified"] = false },
                            "Native AI casts the optional Suggestion with real Will save, normal Standard action and independent slot cost; native caster-level duration and active-effect refusal are observed. Actual expiry/damage break remain separate.");
                        if (!exact) throw new InvalidOperationException("Suggestion native outcome failed: " + key);
                    }
                }
                finally { EventBus.Unsubscribe(events); EventBus.Unsubscribe(starts); }
            }
            finally
            {
                foreach (var unit in new[] { owner, sibling }.Where(value => value != null && !value.Destroyed))
                { Sprint28SetBrainActivePreservingActions(unit, false); unit.Commands.InterruptAll(true); }
                target.Commands.InterruptAll(true);
                foreach (var buff in target.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(value => owner != null && ReferenceEquals(value.Context?.MaybeCaster, owner)).ToArray())
                    target.Descriptor.Buffs.RemoveFact(buff);
                foreach (var condition in addedConditions) target.Descriptor.State.RemoveConditionImmunity(condition);
                if (charmImmunity != null)
                { target.Descriptor.RemoveFact(charmImmunity); foreach (var component in charmImmunity.ComponentsArray) UnityEngine.Object.DestroyImmediate(component); UnityEngine.Object.DestroyImmediate(charmImmunity); }
                target.Stats.SaveWill.BaseValue = oldWill;
                DisposeExpandedSummoningUnits(fixture.Created, new[] { owner, sibling }.Where(value => value != null && !value.Destroyed).ToArray());
                ResetExpandedSummoningHostile(fixture);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode;
                Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = oldPause;
            }
        }
    }
}
