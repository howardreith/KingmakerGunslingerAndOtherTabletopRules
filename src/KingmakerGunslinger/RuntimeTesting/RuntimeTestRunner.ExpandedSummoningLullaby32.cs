using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Controllers.Brain;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private LillendDazeRoundTicks32 _lillendDazeRoundTicks32;
        private void Sprint32TickLife(UnitEntityData unit)
        {
            Sprint26TickLife(unit);
            _lillendDazeRoundTicks32?.TickExact(unit);
            _lillendHoldOwnerTicks32?.TickExact(unit);
        }
        // The existing Dialog fixture clock lacks the native round controller.
        // Its life callback already runs inside the exact-owned Pulse with the
        // real frame delta. Only this selected actor receives the missing tick.
        private sealed class LillendDazeRoundTicks32 : Kingmaker.Controllers.Units.UnitTicksController
        {
            private readonly UnitEntityData _actor;
            private readonly Kingmaker.Controllers.Brain.Blueprints.BlueprintAiAction _action;
            private int _frame = -1, _lastCooldown = -1;
            internal int TickCount { get; private set; }
            internal readonly JArray Cooldowns = new JArray();
            internal LillendDazeRoundTicks32(UnitEntityData actor,
                Kingmaker.Controllers.Brain.Blueprints.BlueprintAiAction action)
            { _actor = actor; _action = action; }
            internal void TickExact(UnitEntityData unit)
            {
                if (!ReferenceEquals(unit, _actor) || unit.Destroyed || !unit.IsInGame) return;
                if (_frame == Time.frameCount) throw new InvalidOperationException("Duplicate owned Daze round tick.");
                _frame = Time.frameCount;
                if (!base.ShouldTickOnUnit(unit)) return;
                int before = unit.CombatState.AIData.GetCooldown(_action);
                base.TickOnUnit(unit); TickCount++;
                int after = unit.CombatState.AIData.GetCooldown(_action);
                if (before != _lastCooldown || after != before) Cooldowns.Add(new JObject {
                    ["before"] = before, ["after"] = after, ["frame"] = Time.frameCount,
                    ["gameSeconds"] = Game.Instance.Player.GameTime.TotalSeconds });
                _lastCooldown = after;
            }
        }
        // Existing owned target, AI and command observers. Pre-spends establish
        // choices; rule probes establish modifiers, neither is cast-cost proof.
        private IEnumerable<int> ReviewLullaby32(ExpandedSummoningCorrectionFixture fixture)
        {
            UnitEntityData owner = null, sibling = null;
            BlueprintFeature resistanceFact = null;
            bool addedManeuverImmunity = false;
            var target = fixture.Hostile;
            int oldWill = target.Stats.SaveWill.BaseValue;
            bool oldPause = Game.Instance.IsPaused;
            const string prefix = "KMG_Summoning_FeyCelestial_LillendAzata_";
            try
            {
                ResetExpandedSummoningHostile(fixture);
                PlaceExpandedSummoningUnit(target, fixture.Caster.Position + Vector3.forward * 40f);
                owner = CastExpandedSummoningOwnTier(fixture, "lillend-azata");
                sibling = CastExpandedSummoningOwnTier(fixture, "lillend-azata");
                foreach (var unit in new[] { owner, sibling })
                { Sprint28SetBrainActivePreservingActions(unit, false); unit.Commands.InterruptAll(true); unit.Commands.RemoveFinishedAndUpdateQueue(); }
                var ability = fixture.Blueprints.OfType<BlueprintAbility>().Single(value => value.name == prefix + "Lullaby");
                var effectBlueprint = fixture.Blueprints.OfType<BlueprintBuff>().Single(value => value.name == prefix + "LullabyBuff");
                var eligibility = fixture.Blueprints.OfType<LillendSpellEligibility32>().Single(value => ReferenceEquals(value.Spell, ability));
                var bookBlueprint = owner.Blueprint.GetComponent<LillendBardBook32>().Book;
                var book = owner.Descriptor.GetSpellbook(bookBlueprint); var siblingBook = sibling.Descriptor.GetSpellbook(bookBlueprint);
                var setup = new JArray();
                foreach (int level in new[] { 2, 3 })
                {
                    int before = book.GetSpontaneousSlots(level); var known = book.GetKnownSpells(level).First();
                    for (int count = 0; count < before; count++) known.Spend();
                    setup.Add(new JObject { ["level"] = level, ["before"] = before, ["after"] = book.GetSpontaneousSlots(level) });
                }
                var resources = fixture.Blueprints.OfType<BlueprintAbilityResource>().Where(value =>
                    value.name == prefix + "PerformanceResource" || value.name == prefix + "CharmPersonSlaResource").ToArray();
                if (resources.Length != 2) throw new InvalidOperationException("Lullaby requires exact two competing pools.");
                foreach (var resource in resources)
                {
                    int before = owner.Descriptor.Resources.GetResourceAmount(resource); owner.Descriptor.Resources.Spend(resource, before);
                    setup.Add(new JObject { ["resource"] = resource.AssetGuid, ["before"] = before,
                        ["after"] = owner.Descriptor.Resources.GetResourceAmount(resource) });
                }
                var siblingPools = resources.Select(value => sibling.Descriptor.Resources.GetResourceAmount(value)).ToArray();
                var ownSlots = Enumerable.Range(1, 3).Select(book.GetSpontaneousSlots).ToArray();
                var siblingSlots = Enumerable.Range(1, 3).Select(siblingBook.GetSpontaneousSlots).ToArray();
                int ownCantripSlots = book.GetSpontaneousSlots(0), siblingCantripSlots = siblingBook.GetSpontaneousSlots(0);
                Vector3 far; string lane, direction;
                var origin = Sprint24AiCombatLane(fixture.Caster, owner, target, 8f, out far, out lane, out direction,
                    originAllyExclusionRadius: new Feet(30).Meters + 1f);
                PlaceExpandedSummoningUnit(owner, origin); PlaceExpandedSummoningUnit(target, far);
                PlaceExpandedSummoningUnit(sibling, origin + Vector3.right * 2f);
                target.Stats.SaveWill.BaseValue = -100;
                // Daze refusal permits the ordinary melee fallback between
                // cantrip casts. Isolate that legal choice from the separately
                // required grab/constrict case: a successful grab disables
                // the holder and would contaminate the repeated-cast probe.
                if (target.Descriptor.State.HasCondition(UnitCondition.ImmuneToCombatManeuvers))
                    throw new InvalidOperationException("Cantrip target already has maneuver immunity.");
                target.Descriptor.State.AddCondition(UnitCondition.ImmuneToCombatManeuvers, null);
                addedManeuverImmunity = true;
                // A disclosed low native SR ensures the real cast visits the
                // resistance seam without forcing a check result or a roll.
                var resistance = ScriptableObject.CreateInstance<AddSpellResistance>();
                resistance.Value = new ContextValue { ValueType = ContextValueType.Simple, Value = 5 };
                resistance.AddCR = false;
                resistanceFact = ScriptableObject.CreateInstance<BlueprintFeature>(); resistanceFact.Ranks = 1;
                resistanceFact.name = "KMG_Runtime_Lullaby32_LowSpellResistance";
                resistanceFact.ComponentsArray = new BlueprintComponent[] { resistance };
                target.Descriptor.AddFact(resistanceFact);
                foreach (var unit in new[] { owner, target })
                { unit.Wake(0f); if (!Game.Instance.State.AwakeUnits.Contains(unit)) Game.Instance.State.AwakeUnits.Add(unit); }
                owner.Memory.Add(target); target.Memory.Add(owner);
                owner.SightCache.ClearIfNeeded(); target.SightCache.ClearIfNeeded(); new Kingmaker.Controllers.Units.UnitMemoryController().Tick();
                owner.JoinCombat(); target.JoinCombat(); Game.Instance.Player.UpdateIsInCombat();
                new UnitCombatJoinController().Tick(); new UnitCombatPrepareController().Tick();
                var choiceContext = new DecisionContext { Unit = owner, Target = new TargetInfo { Unit = target } };
                float ordinaryScore = eligibility.Score(choiceContext);
                var siblingPosition = sibling.Position;
                PlaceExpandedSummoningUnit(sibling, target.Position + Vector3.right);
                float overlapScore = eligibility.Score(choiceContext);
                PlaceExpandedSummoningUnit(sibling, siblingPosition);
                if (target.Descriptor.State.HasConditionImmunity(UnitCondition.Sleeping))
                    throw new InvalidOperationException("Lullaby target already has Sleep immunity.");
                float immuneScore;
                target.Descriptor.State.AddConditionImmunity(UnitCondition.Sleeping);
                try { immuneScore = eligibility.Score(choiceContext); }
                finally { target.Descriptor.State.RemoveConditionImmunity(UnitCondition.Sleeping); }
                var grant = owner.Descriptor.Abilities.GetAbility(ability);
                if (grant == null) throw new InvalidOperationException("Lullaby has no native cantrip fact.");
                var data = new AbilityData(grant); var parameters = data.CalculateParams();
                var bookData = new AbilityData(ability, book);
                int perception = target.Stats.SkillPerception.ModifiedValue;
                int[] savesBefore = Lullaby32SaveValues(owner, target, eligibility.SleepSpell, ability);
                bool ready = ordinaryScore > 0f && overlapScore == 0f && immuneScore == 0f &&
                    target.Descriptor.Progression.CharacterLevel <= 4 && data.IsAvailable && data.CanTarget(new TargetWrapper(target)) &&
                    parameters.CasterLevel == 7 && parameters.DC == 14 && ability.ActionType == UnitCommand.CommandType.Standard &&
                    !ability.IsFullRoundAction && ownSlots.SequenceEqual(new[] { 5, 0, 0 }) && siblingSlots.SequenceEqual(new[] { 5, 4, 2 }) &&
                    owner.Descriptor.State.CanAct && owner.HasLOS(target) && owner.Memory.ContainsVisible(target);
                Sprint25Check("lullaby-ready-and-refusals", ready, new JObject {
                    ["owner"] = owner.UniqueId, ["sibling"] = sibling.UniqueId, ["target"] = target.UniqueId,
                    ["targetHd"] = target.Descriptor.Progression.CharacterLevel, ["lane"] = lane,
                    ["ordinaryScore"] = ordinaryScore, ["allyOverlapScore"] = overlapScore, ["sleepImmuneScore"] = immuneScore,
                    ["cl"] = parameters.CasterLevel, ["dc"] = parameters.DC, ["fixturePreSpends"] = setup,
                    ["factAvailable"] = data.IsAvailable, ["factCanTarget"] = data.CanTarget(new TargetWrapper(target)),
                    ["cantripComponent"] = ability.GetComponent<Kingmaker.Blueprints.Classes.Spells.CantripComponent>() != null,
                    ["bookBackedAvailable"] = bookData.IsAvailable, ["levelZeroSlots"] = book.GetSpontaneousSlots(0),
                    ["ownSlots"] = new JArray(ownSlots), ["siblingSlots"] = new JArray(siblingSlots),
                    ["willBase"] = target.Stats.SaveWill.BaseValue, ["fixtureSpellResistance"] = 5,
                    ["fixtureManeuverImmune"] = target.Descriptor.State.HasCondition(UnitCondition.ImmuneToCombatManeuvers),
                    ["fullRound"] = ability.IsFullRoundAction },
                    "Normal cantrip is useful for available Sleep; AI refuses allied footprint and Sleep immunity before acting.");
                if (!ready) throw new InvalidOperationException("Lullaby preconditions failed before AI execution.");
                var commands = new HashSet<UnitCommand>(); var costs = new JArray();
                var starts = new Lillend32CommandStarts { Owner = owner };
                var events = new Astral31DispelEvents { Owner = owner }; events.Targets.Add(target);
                EventBus.Subscribe(starts); EventBus.Subscribe(events);
                Buff effect = null;
                try
                {
                    Sprint28SetBrainActivePreservingActions(owner, true); owner.Brain.RestoreAvailableActions();
                    using (var economy = new FirearmReviewCosts(owner, costs))
                    {
                        DateTime deadline = DateTime.UtcNow.AddSeconds(55); int frames = 0, ticks = 0;
                        while (DateTime.UtcNow < deadline && frames++ < 5000)
                        {
                            foreach (var command in owner.Commands.Raw.Where(value => value != null)) commands.Add(command);
                            foreach (var command in starts.Commands) commands.Add(command);
                            effect = target.Buffs.Enumerable.SingleOrDefault(value => ReferenceEquals(value.Blueprint, effectBlueprint) &&
                                ReferenceEquals(value.Context?.MaybeCaster, owner));
                            int finished = commands.OfType<UnitUseAbility>().Count(value => value.Spell?.Blueprint == ability &&
                                value.IsFinished && value.ExecutionProcess != null && value.ExecutionProcess.IsEnded);
                            if (effect != null && finished > 0 || finished >= 3) break;
                            if (frames == 1 || frames % 60 == 0 && !owner.Commands.Raw.Any(value => value != null && !value.IsFinished))
                            { Sprint24TickExactAi(owner); ticks++; }
                            Game.Instance.IsPaused = false; yield return 0;
                        }
                        Game.Instance.IsPaused = true; Sprint28SetBrainActivePreservingActions(owner, false);
                        var casts = commands.OfType<UnitUseAbility>().Where(value => value.Spell?.Blueprint == ability &&
                            value.IsFinished && value.Result == UnitCommand.ResultType.Success && value.ExecutionProcess != null && value.ExecutionProcess.IsEnded).ToArray();
                        var saves = events.Saves.Where(value => value.Reason?.Context?.SourceAbility == ability).ToArray();
                        var resistanceChecks = events.Resistances.Where(value => value.Context?.SourceAbility == ability &&
                            ReferenceEquals(value.Target, target)).ToArray();
                        int[] savesAfter = Lullaby32SaveValues(owner, target, eligibility.SleepSpell, ability);
                        bool exact = casts.Length > 0 && casts.Length <= 3 && casts.All(value => economy.Count(value) == 1 &&
                            economy.Exact(value, false) && value.Type == UnitCommand.CommandType.Standard && !value.Cutscene && !value.IsIgnoreCooldown &&
                            ReferenceEquals(value.Spell.Fact, grant) && value.Spell.Spellbook == null) &&
                            saves.Length == casts.Length && saves.All(value => value.DifficultyClass == 14) && saves.Any(value => !value.IsPassed) &&
                            resistanceChecks.Length == casts.Length && resistanceChecks.All(value => value.SpellResistance == 5 &&
                                !value.TargetIsImmune && !value.IsSpellResisted) &&
                            effect != null && effect.Context.Params.CasterLevel == 7 && effect.Context.Params.DC == 14 &&
                            effect.TimeLeft.TotalSeconds > 34 && effect.TimeLeft.TotalSeconds <= 42.1 &&
                            target.Stats.SkillPerception.ModifiedValue == perception - 5 && eligibility.Score(choiceContext) == 0f &&
                            savesAfter.SequenceEqual(new[] { savesBefore[0] - 2, savesBefore[1], savesBefore[2], savesBefore[3] }) &&
                            target.Stats.SaveWill.ModifiedValue == savesBefore[1] &&
                            Enumerable.Range(1, 3).Select(book.GetSpontaneousSlots).SequenceEqual(ownSlots) &&
                            Enumerable.Range(1, 3).Select(siblingBook.GetSpontaneousSlots).SequenceEqual(siblingSlots) &&
                            book.GetSpontaneousSlots(0) == ownCantripSlots && siblingBook.GetSpontaneousSlots(0) == siblingCantripSlots &&
                            resources.All(value => owner.Descriptor.Resources.GetResourceAmount(value) == 0) &&
                            resources.Select(value => sibling.Descriptor.Resources.GetResourceAmount(value)).SequenceEqual(siblingPools);
                        Sprint25Check("lullaby-natural-cast-and-modifiers", exact, new JObject {
                            ["commands"] = new JArray(commands.Select(Sprint27AiCommand)), ["costs"] = costs, ["starts"] = starts.Rows,
                            ["frames"] = frames, ["aiTicks"] = ticks, ["nativeCasts"] = casts.Length,
                            ["saves"] = new JArray(saves.Select(value => new JObject { ["dc"] = value.DifficultyClass, ["passed"] = value.IsPassed })),
                            ["resistanceChecks"] = new JArray(resistanceChecks.Select(Astral31ResistanceRow)),
                            ["effectSeconds"] = effect?.TimeLeft.TotalSeconds, ["perceptionBefore"] = perception,
                            ["perceptionAfter"] = target.Stats.SkillPerception.ModifiedValue,
                            ["probeOrder"] = "Sleep Will; unrelated Will; Sleep Fortitude; Sleep Reflex",
                            ["saveValuesBefore"] = new JArray(savesBefore), ["saveValuesAfter"] = new JArray(savesAfter),
                            ["activeEffectScore"] = eligibility.Score(choiceContext),
                            ["ownSlotsAfter"] = new JArray(Enumerable.Range(1, 3).Select(book.GetSpontaneousSlots)),
                            ["siblingSlotsAfter"] = new JArray(Enumerable.Range(1, 3).Select(siblingBook.GetSpontaneousSlots)),
                            ["exactOwnedCantripFacts"] = casts.All(value => ReferenceEquals(value.Spell.Fact, grant) && value.Spell.Spellbook == null),
                            ["levelZeroBeforeAfter"] = new JArray(ownCantripSlots, book.GetSpontaneousSlots(0), siblingCantripSlots, siblingBook.GetSpontaneousSlots(0)),
                            ["fixtureCastCommands"] = 0, ["fixtureBuffApplications"] = 0, ["forcedRolls"] = 0 },
                            "Natural Standard cantrip and native Will/SR; CL7 tail, exact Perception and Will-only Sleep penalties. Separate native rule probes are not cast/action-cost proof.");
                        if (!exact) throw new InvalidOperationException("Lullaby native cast/modifier outcome failed.");
                    }
                    owner.Commands.InterruptAll(true); owner.Commands.RemoveFinishedAndUpdateQueue();
                    TimeSpan clock = Game.Instance.Player.GameTime, originalEnd = effect.EndTime;
                    bool beforeExpiry = false, afterExpiry = false;
                    try
                    {
                        Game.Instance.Player.GameTime = originalEnd - TimeSpan.FromSeconds(.1); target.Buffs.Tick();
                        beforeExpiry = target.Buffs.Enumerable.Contains(effect) && target.Stats.SkillPerception.ModifiedValue == perception - 5;
                        Game.Instance.Player.GameTime = originalEnd + TimeSpan.FromSeconds(.1); target.Buffs.Tick();
                        afterExpiry = !target.Buffs.Enumerable.Contains(effect) && target.Stats.SkillPerception.ModifiedValue == perception &&
                            Lullaby32SaveValues(owner, target, eligibility.SleepSpell, ability).SequenceEqual(savesBefore);
                    }
                    finally { Game.Instance.Player.GameTime = clock; }
                    Sprint25Check("lullaby-original-deadline-expiry", beforeExpiry && afterExpiry && Game.Instance.Player.GameTime == clock,
                        new JObject { ["beforeExpiry"] = beforeExpiry, ["afterExpiry"] = afterExpiry,
                            ["originalEnd"] = originalEnd.TotalSeconds, ["clockRestored"] = Game.Instance.Player.GameTime == clock },
                        "Paused original-deadline seam ticks only the owned target's native buffs; no shortened duration or direct removal supplies expiry proof.");
                    if (!beforeExpiry || !afterExpiry) throw new InvalidOperationException("Lullaby native expiry failed.");
                }
                finally { EventBus.Unsubscribe(events); EventBus.Unsubscribe(starts); }
                foreach (int wait in ReviewLillendDaze32(fixture, owner, sibling)) yield return wait;
            }
            finally
            {
                foreach (var unit in new[] { owner, sibling }.Where(value => value != null && !value.Destroyed))
                { Sprint28SetBrainActivePreservingActions(unit, false); unit.Commands.InterruptAll(true); }
                target.Commands.InterruptAll(true);
                foreach (var buff in target.Buffs.Enumerable.Where(value => owner != null && ReferenceEquals(value.Context?.MaybeCaster, owner)).ToArray()) buff.Remove();
                target.Stats.SaveWill.BaseValue = oldWill;
                if (addedManeuverImmunity) target.Descriptor.State.RemoveCondition(UnitCondition.ImmuneToCombatManeuvers);
                if (resistanceFact != null)
                {
                    target.Descriptor.RemoveFact(resistanceFact);
                    foreach (var component in resistanceFact.ComponentsArray) UnityEngine.Object.DestroyImmediate(component);
                    UnityEngine.Object.DestroyImmediate(resistanceFact);
                }
                DisposeExpandedSummoningUnits(fixture.Created, new[] { owner, sibling }.Where(value => value != null && !value.Destroyed).ToArray());
                ResetExpandedSummoningHostile(fixture); Game.Instance.IsPaused = oldPause;
            }
        }

        private IEnumerable<int> ReviewLillendDaze32(ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, UnitEntityData sibling)
        {
            var target = fixture.Hostile;
            var eligibility = fixture.Blueprints.OfType<LillendSpellEligibility32>().Single(value =>
                value.Choice == LillendSpellChoice32.Daze);
            var lullaby = fixture.Blueprints.OfType<LillendSpellEligibility32>().Single(value =>
                value.Choice == LillendSpellChoice32.Lullaby);
            var ability = eligibility.Spell;
            var effectBlueprint = fixture.Blueprints.OfType<BlueprintBuff>().Single(value =>
                value.name == "KMG_Summoning_FeyCelestial_LillendAzata_DazeBuff");
            var nativeDaze = fixture.Blueprints.OfType<BlueprintAbility>().Single(value => value.AssetGuid == "55f14bc84d7c85446b07a1b5dd6b2b4c");
            var nativeDazeBuff = fixture.Blueprints.OfType<BlueprintBuff>().Single(value => value.AssetGuid == "9934fedff1b14994ea90205d189c8759");
            var book = owner.Descriptor.GetSpellbook(eligibility.Book);
            var siblingBook = sibling.Descriptor.GetSpellbook(eligibility.Book);
            int remaining = book.GetSpontaneousSlots(1);
            var known = book.GetKnownSpells(1).First();
            for (int count = 0; count < remaining; count++) known.Spend();
            var ownSlots = Enumerable.Range(0, 4).Select(book.GetSpontaneousSlots).ToArray();
            var siblingSlots = Enumerable.Range(0, 4).Select(siblingBook.GetSpontaneousSlots).ToArray();
            var resources = fixture.Blueprints.OfType<BlueprintAbilityResource>().Where(value =>
                value.name == "KMG_Summoning_FeyCelestial_LillendAzata_PerformanceResource" ||
                value.name == "KMG_Summoning_FeyCelestial_LillendAzata_CharmPersonSlaResource").ToArray();
            var ownPools = resources.Select(value => owner.Descriptor.Resources.GetResourceAmount(value)).ToArray();
            var siblingPools = resources.Select(value => sibling.Descriptor.Resources.GetResourceAmount(value)).ToArray();
            var grant = owner.Descriptor.Abilities.GetAbility(ability);
            if (grant == null) throw new InvalidOperationException("Daze has no native cantrip fact.");
            var data = new AbilityData(grant); var parameters = data.CalculateParams();
            var context = new DecisionContext { Unit = owner, Target = new TargetInfo { Unit = target } };
            var lifetime = Game.Instance.BlueprintRoot.SystemMechanics.SummonedUnitBuff;
            double ownerSeconds = owner.Buffs.Enumerable.Single(value => ReferenceEquals(value.Blueprint, lifetime)).TimeLeft.TotalSeconds;
            double siblingSeconds = sibling.Buffs.Enumerable.Single(value => ReferenceEquals(value.Blueprint, lifetime)).TimeLeft.TotalSeconds;
            float score = eligibility.Score(context);
            bool ready = score > 0 && lullaby.Score(context) == 0 && ownSlots.All(value => value == 0) &&
                ownerSeconds >= 80 && siblingSeconds >= 80 &&
                data.IsAvailable && data.CanTarget(new TargetWrapper(target)) && parameters.CasterLevel == 7 &&
                parameters.DC == 14 && ability.Type == AbilityType.Spell &&
                ability.GetComponent<Kingmaker.Blueprints.Classes.Spells.CantripComponent>() != null &&
                !target.Descriptor.State.HasCondition(UnitCondition.Dazed);
            Sprint25Check("lillend-daze-cantrip-ready", ready, new JObject {
                ["score"] = score, ["lullabyExhaustedSleepScore"] = lullaby.Score(context),
                ["fixtureLevelOnePreSpend"] = remaining, ["ownSlots"] = new JArray(ownSlots),
                ["siblingSlots"] = new JArray(siblingSlots), ["factAvailable"] = data.IsAvailable,
                ["ownerSummonSeconds"] = ownerSeconds, ["siblingSummonSeconds"] = siblingSeconds,
                ["cl"] = parameters.CasterLevel, ["dc"] = parameters.DC },
                "The affected native Daze cantrip remains available with no leveled slots; Lullaby refuses when Sleep is exhausted.");
            if (!ready) throw new InvalidOperationException("Daze cantrip preconditions failed.");
            if (_lillendDazeRoundTicks32 != null) throw new InvalidOperationException("Daze round ownership already active.");
            var starts = new Lillend32CommandStarts { Owner = owner };
            var events = new Astral31DispelEvents { Owner = owner }; events.Targets.Add(target);
            var commands = new HashSet<UnitCommand>(); var costs = new JArray();
            EventBus.Subscribe(starts); EventBus.Subscribe(events);
            try
            {
                var ai = owner.Blueprint.Brain.Actions.OfType<Kingmaker.Controllers.Brain.Blueprints.BlueprintAiCastSpell>()
                    .Single(value => ReferenceEquals(value.Ability, ability));
                _lillendDazeRoundTicks32 = new LillendDazeRoundTicks32(owner, ai);
                Sprint28SetBrainActivePreservingActions(owner, true); owner.Brain.RestoreAvailableActions();
                using (var economy = new FirearmReviewCosts(owner, costs))
                {
                    DateTime deadline = DateTime.UtcNow.AddSeconds(85); int frames = 0;
                    bool refusedWhileDazed = false;
                    bool immuneAfterDaze = false, immunityExpired = false, nativeProbeRan = false, nativeProbeBlocked = false, siblingRefused = false;
                    Buff firstEffect = null, firstImmunity = null;
                    double firstEffectSeconds = 0, firstImmunitySeconds = 0;
                    double immunityEnd = -1, immunityLastPresent = -1, immunityFirstAbsent = -1;
                    var immunities = new HashSet<Buff>();
                    var immunityRows = new JArray();
                    JObject postDazeImmunity = null;
                    while (DateTime.UtcNow < deadline && frames++ < 10000)
                    {
                        foreach (var command in owner.Commands.Raw.Where(value => value != null)) commands.Add(command);
                        foreach (var command in starts.Commands) commands.Add(command);
                        int finished = commands.OfType<UnitUseAbility>().Count(value => value.Spell?.Blueprint == ability &&
                            value.IsFinished && value.ExecutionProcess != null && value.ExecutionProcess.IsEnded);
                        var effect = target.Buffs.Enumerable.SingleOrDefault(value => ReferenceEquals(value.Blueprint, effectBlueprint) &&
                            ReferenceEquals(value.Context?.MaybeCaster, owner));
                        var marker = target.Buffs.Enumerable.SingleOrDefault(value => ReferenceEquals(value.Blueprint, eligibility.RefusedBuff));
                        if (firstEffect == null && effect != null) { firstEffect = effect; firstEffectSeconds = effect.TimeLeft.TotalSeconds; }
                        if (marker != null && immunities.Add(marker))
                        {
                            if (firstImmunity == null) { firstImmunity = marker; firstImmunitySeconds = marker.TimeLeft.TotalSeconds;
                                immunityEnd = marker.EndTime.TotalSeconds; }
                            immunityRows.Add(new JObject { ["endGameSeconds"] = marker.EndTime.TotalSeconds,
                                ["observedGameSeconds"] = Game.Instance.Player.GameTime.TotalSeconds,
                                ["remainingSeconds"] = marker.TimeLeft.TotalSeconds, ["source"] = marker.Context?.MaybeCaster?.UniqueId });
                        }
                        if (target.Descriptor.State.HasCondition(UnitCondition.Dazed) && eligibility.Score(context) == 0)
                            refusedWhileDazed = true;
                        if (firstImmunity != null && target.Buffs.Enumerable.Contains(firstImmunity) &&
                            !target.Descriptor.State.HasCondition(UnitCondition.Dazed))
                        {
                            float immuneScore = eligibility.Score(context);
                            bool canTarget = data.CanTarget(new TargetWrapper(target));
                            immuneAfterDaze |= immuneScore == 0 && !canTarget;
                            if (postDazeImmunity == null) postDazeImmunity = new JObject {
                                ["markerRemainingSeconds"] = firstImmunity.TimeLeft.TotalSeconds,
                                ["dazed"] = false, ["score"] = immuneScore, ["canTarget"] = canTarget };
                            if (!nativeProbeRan)
                            {
                                nativeProbeRan = true;
                                siblingRefused = !new AbilityData(sibling.Descriptor.Abilities.GetAbility(ability)).CanTarget(new TargetWrapper(target));
                                // Separate native application probe, not a cast:
                                // the one-minute marker must also block the
                                // unchanged game's Daze buff from another source.
                                var nativeContext = new MechanicsContext(sibling, target.Descriptor, nativeDaze, null, new TargetWrapper(target));
                                var attempted = target.Descriptor.AddBuff(nativeDazeBuff, nativeContext, TimeSpan.FromSeconds(6));
                                nativeProbeBlocked = attempted == null && !target.Descriptor.State.HasCondition(UnitCondition.Dazed);
                                if (attempted != null) attempted.Remove();
                            }
                        }
                        if (firstImmunity != null)
                        {
                            double now = Game.Instance.Player.GameTime.TotalSeconds;
                            if (target.Buffs.Enumerable.Contains(firstImmunity)) immunityLastPresent = now;
                            else if (immunityFirstAbsent < 0) immunityFirstAbsent = now;
                            // Absence long after an early removal is not expiry
                            // evidence. Bracket this exact marker's original
                            // deadline with live presence and its first absence.
                            immunityExpired = immunityLastPresent >= immunityEnd - 1 && immunityLastPresent < immunityEnd &&
                                immunityFirstAbsent >= immunityEnd && immunityFirstAbsent <= immunityEnd + 1;
                        }
                        // A successful Will save is a completed cast too. The
                        // second actual effect can belong to a third/fourth
                        // attempt whose command is still naturally finishing.
                        bool allDazeCommandsEnded = commands.OfType<UnitUseAbility>().Where(value => value.Spell?.Blueprint == ability)
                            .All(value => value.IsFinished && value.ExecutionProcess != null && value.ExecutionProcess.IsEnded);
                        if ((immunities.Count >= 2 && finished >= 2 && allDazeCommandsEnded &&
                            target.Descriptor.State.HasCondition(UnitCondition.Dazed)) || finished >= 4) break;
                        // Native TickBrain owns NextCommandTime and each
                        // command's CanInterrupt decision. Repeating melee can
                        // have an unstarted interruptible command in its queue;
                        // requiring an empty queue starves lawful reconsideration.
                        if (frames == 1 || frames % 60 == 0)
                            Sprint24TickExactAi(owner);
                        Game.Instance.IsPaused = false; yield return 0;
                    }
                    Game.Instance.IsPaused = true; Sprint28SetBrainActivePreservingActions(owner, false);
                    var casts = commands.OfType<UnitUseAbility>().Where(value => value.Spell?.Blueprint == ability &&
                        value.IsFinished && value.Result == UnitCommand.ResultType.Success &&
                        value.ExecutionProcess != null && value.ExecutionProcess.IsEnded).ToArray();
                    var saves = events.Saves.Where(value => value.Reason?.Context?.SourceAbility == ability).ToArray();
                    var resistance = events.Resistances.Where(value => value.Context?.SourceAbility == ability &&
                        ReferenceEquals(value.Target, target)).ToArray();
                    var attempts = commands.OfType<UnitUseAbility>().Where(value => value.Spell?.Blueprint == ability).ToArray();
                    bool allAttemptsEnded = attempts.All(value => value.IsFinished && value.ExecutionProcess != null && value.ExecutionProcess.IsEnded);
                    bool exact = allAttemptsEnded && casts.Length >= 2 && casts.Length <= 4 && casts.All(value => economy.Count(value) == 1 &&
                        economy.Exact(value, false) && value.Type == UnitCommand.CommandType.Standard && !value.Cutscene && !value.IsIgnoreCooldown &&
                        ReferenceEquals(value.Spell.Fact, grant) && value.Spell.Spellbook == null) &&
                        saves.Length == casts.Length && saves.All(value => value.DifficultyClass == 14) && saves.Any(value => !value.IsPassed) &&
                        resistance.Length == casts.Length && resistance.All(value => value.SpellResistance == 5 &&
                            !value.TargetIsImmune && !value.IsSpellResisted) &&
                        target.Descriptor.State.HasCondition(UnitCondition.Dazed) && refusedWhileDazed && eligibility.Score(context) == 0 &&
                        immunities.Count == 2 && immuneAfterDaze && immunityExpired && nativeProbeRan && nativeProbeBlocked && siblingRefused &&
                        _lillendDazeRoundTicks32.Cooldowns.OfType<JObject>().Any(value => (int)value["before"] == 1 && (int)value["after"] == 0) &&
                        firstEffectSeconds > 4 && firstEffectSeconds <= 6.1 && firstImmunitySeconds > 58 && firstImmunitySeconds <= 60.1 &&
                        Enumerable.Range(0, 4).Select(book.GetSpontaneousSlots).SequenceEqual(ownSlots) &&
                        Enumerable.Range(0, 4).Select(siblingBook.GetSpontaneousSlots).SequenceEqual(siblingSlots) &&
                        resources.Select(value => owner.Descriptor.Resources.GetResourceAmount(value)).SequenceEqual(ownPools) &&
                        resources.Select(value => sibling.Descriptor.Resources.GetResourceAmount(value)).SequenceEqual(siblingPools);
                    Sprint25Check("lillend-daze-natural-cantrip", exact, new JObject {
                        ["commands"] = new JArray(commands.Select(Sprint27AiCommand)), ["costs"] = costs,
                        ["starts"] = starts.Rows, ["nativeCasts"] = casts.Length,
                        ["observedDazeAttempts"] = attempts.Length, ["allDazeAttemptsEnded"] = allAttemptsEnded,
                        ["saves"] = new JArray(saves.Select(value => new JObject { ["dc"] = value.DifficultyClass, ["passed"] = value.IsPassed })),
                        ["resistanceChecks"] = new JArray(resistance.Select(Astral31ResistanceRow)),
                        ["dazed"] = target.Descriptor.State.HasCondition(UnitCondition.Dazed),
                        ["activeEffectScore"] = eligibility.Score(context),
                        ["refusedWhileDazed"] = refusedWhileDazed,
                        ["immunityInstances"] = immunityRows, ["immuneAfterDaze"] = immuneAfterDaze,
                        ["postDazeImmunity"] = postDazeImmunity,
                        ["naturalImmunityExpiry"] = immunityExpired, ["firstDazeSeconds"] = firstEffectSeconds,
                        ["immunityOriginalEnd"] = immunityEnd, ["immunityLastPresent"] = immunityLastPresent,
                        ["immunityFirstAbsent"] = immunityFirstAbsent,
                        ["nativeRoundTicks"] = _lillendDazeRoundTicks32.TickCount,
                        ["nativeAiCooldowns"] = _lillendDazeRoundTicks32.Cooldowns,
                        ["firstImmunitySeconds"] = firstImmunitySeconds,
                        ["siblingPrivateDazeRefused"] = siblingRefused, ["separateNativeBuffProbeBlocked"] = nativeProbeBlocked,
                        ["nativeBuffApplicationProbes"] = nativeProbeRan ? 1 : 0,
                        ["exactOwnedCantripFacts"] = casts.All(value => ReferenceEquals(value.Spell.Fact, grant) && value.Spell.Spellbook == null),
                        ["ownSlotsAfter"] = new JArray(Enumerable.Range(0, 4).Select(book.GetSpontaneousSlots)),
                        ["siblingSlotsAfter"] = new JArray(Enumerable.Range(0, 4).Select(siblingBook.GetSpontaneousSlots)),
                        ["fixtureCastCommands"] = 0, ["forcedRolls"] = 0 },
                        "Two natural Daze effects separated by native sixty-second immunity; Standard/Will/SR, one-round Dazed, cross-source immunity/refusal and unchanged independent budgets. The native buff application probe is not a second casting-source execution.");
                    if (!exact) throw new InvalidOperationException("Daze natural cantrip failed.");
                }
            }
            finally
            {
                _lillendDazeRoundTicks32 = null;
                Sprint28SetBrainActivePreservingActions(owner, false);
                EventBus.Unsubscribe(events); EventBus.Unsubscribe(starts);
            }
        }

        private static int[] Lullaby32SaveValues(UnitEntityData caster, UnitEntityData target, BlueprintAbility sleep, BlueprintAbility unrelated)
        {
            var spells = new[] { sleep, unrelated, sleep, sleep };
            var types = new[] { SavingThrowType.Will, SavingThrowType.Will, SavingThrowType.Fortitude, SavingThrowType.Reflex };
            return Enumerable.Range(0, 4).Select(index => {
                var context = new MechanicsContext(caster, caster.Descriptor, spells[index], null, new TargetWrapper(target));
                var rule = new RuleSavingThrow(target, types[index], 100) { Reason = context };
                context.TriggerRule(rule); return rule.StatValue;
            }).ToArray();
        }
    }
}
