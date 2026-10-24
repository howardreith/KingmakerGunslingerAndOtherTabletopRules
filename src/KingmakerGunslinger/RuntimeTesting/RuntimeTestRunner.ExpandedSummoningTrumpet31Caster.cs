using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> Trumpet31ManualCommand(UnitEntityData actor, AbilityData data,
            UnitEntityData target, string id)
        {
            var parameters = data.CalculateParams(); var wrapped = new TargetWrapper(target);
            bool ready = actor.IsInCombat && data.IsAvailable && data.CanTarget(wrapped);
            Sprint25Check(id + "-ready", ready, new JObject { ["actor"] = actor.UniqueId,
                ["target"] = target.UniqueId, ["ability"] = data.Blueprint.AssetGuid,
                ["actorInCombat"] = actor.IsInCombat, ["targetInCombat"] = target.IsInCombat,
                ["available"] = data.IsAvailable, ["canTarget"] = data.CanTarget(wrapped),
                ["cl"] = parameters.CasterLevel, ["spellLevel"] = parameters.SpellLevel, ["dc"] = parameters.DC,
                ["book"] = data.Spellbook?.Blueprint.AssetGuid, ["commands"] = new JArray(actor.Commands.Raw
                    .Where(queued => queued != null).Select(Sprint27AiCommand)) },
                "Actual native ability availability, target, parameters and queue are observed before the manual command.");
            if (!ready) throw new InvalidOperationException("Native caster manual-command preconditions failed: " + id);
            var command = new UnitUseAbility(data, wrapped); var costs = new JArray();
            using (var economy = new FirearmReviewCosts(actor, costs))
            {
                actor.Commands.Run(command); var deadline = DateTime.UtcNow.AddSeconds(30);
                while (DateTime.UtcNow < deadline && (!command.IsFinished ||
                    command.ExecutionProcess != null && !command.ExecutionProcess.IsEnded))
                { Game.Instance.IsPaused = false; yield return 0; }
                bool exact = command.IsFinished && command.Result == UnitCommand.ResultType.Success &&
                    command.ExecutionProcess != null && command.ExecutionProcess.IsEnded &&
                    !command.Cutscene && !command.IsIgnoreCooldown && command.AiAction == null &&
                    economy.Count(command) == 1 && economy.Exact(command, false);
                Sprint25Check(id, exact, new JObject { ["actor"] = actor.UniqueId, ["target"] = target.UniqueId,
                    ["ability"] = data.Blueprint.AssetGuid, ["cl"] = parameters.CasterLevel,
                    ["spellLevel"] = parameters.SpellLevel, ["dc"] = parameters.DC,
                    ["book"] = data.Spellbook?.Blueprint.AssetGuid, ["command"] = Sprint27AiCommand(command),
                    ["executionEnded"] = command.ExecutionProcess?.IsEnded, ["actorInCombatAfter"] = actor.IsInCombat, ["costs"] = costs,
                    ["manualNativeCommand"] = true, ["fixtureEffectExecutions"] = 0, ["fixtureSlotDebits"] = 0 },
                    "A real manual native command completes and pays one standard action; no AI claim or direct effect/resource execution.");
                if (!exact) throw new InvalidOperationException("Native caster manual command failed: " + id);
            }
        }

        private IEnumerable<int> Trumpet31ObserveAiSpell(UnitEntityData actor, string aiName,
            string id, Action<UnitUseAbility> observe)
        {
            var priorCommands = actor.Commands.Raw.Where(command => command != null).ToArray();
            bool cleanQueue = !priorCommands.Any(command => !command.IsFinished);
            Sprint25Check(id + "-queue", cleanQueue, new JObject { ["actor"] = actor.UniqueId,
                ["commands"] = new JArray(priorCommands.Select(Sprint27AiCommand)) },
                "No leftover live command may contaminate the next owned native AI decision.");
            if (!cleanQueue) throw new InvalidOperationException("Caster AI subcase starts with a live command.");
            actor.Commands.RemoveFinishedAndUpdateQueue();
            Sprint28SetBrainActivePreservingActions(actor, true); actor.Brain.RestoreAvailableActions();
            var commands = new HashSet<UnitCommand>(); var ambient = new HashSet<string>(); var costs = new JArray();
            int ticks = 0, frames = 0;
            try
            {
                using (var economy = new FirearmReviewCosts(actor, costs))
                {
                    var deadline = DateTime.UtcNow.AddSeconds(30);
                    while (DateTime.UtcNow < deadline && frames++ < 3000)
                    {
                        if (frames == 1 || frames % 60 == 0 && !actor.Commands.Raw.Any(value => value != null && !value.IsFinished))
                        { foreach (var candidate in Sprint24TickExactAi(actor)) if (candidate != actor.UniqueId) ambient.Add(candidate); ticks++; }
                        foreach (var command in actor.Commands.Raw.Where(value => value != null)) commands.Add(command);
                        if (commands.OfType<UnitUseAbility>().Any(command => command.AiAction != null && command.IsFinished &&
                            command.ExecutionProcess != null && command.ExecutionProcess.IsEnded)) break;
                        Game.Instance.IsPaused = false; yield return 0;
                    }
                    var cast = commands.OfType<UnitUseAbility>().FirstOrDefault(command => command.AiAction?.name == aiName &&
                        command.IsFinished && command.Result == UnitCommand.ResultType.Success);
                    bool exact = cast != null && !cast.Cutscene && !cast.IsIgnoreCooldown && economy.Count(cast) == 1 && economy.Exact(cast, false);
                    Sprint25Check(id, exact, new JObject { ["actor"] = actor.UniqueId, ["aiTicks"] = ticks,
                        ["commands"] = new JArray(commands.Select(Sprint27AiCommand)), ["costs"] = costs,
                        ["ambientCandidatesNeverTicked"] = new JArray(ambient), ["fixtureCasts"] = 0,
                        ["fixtureSlotDebits"] = 0, ["forcedRolls"] = 0 },
                        "The exact actor's real native brain selects the named spell and pays its standard action; fixture supplies no cast or slot debit.");
                    if (!exact) throw new InvalidOperationException("Native caster AI command failed: " + id);
                    observe(cast);
                }
            }
            finally { Sprint28SetBrainActivePreservingActions(actor, false); }
        }

        private IEnumerable<int> ReviewTrumpet31Caster(ExpandedSummoningCorrectionFixture fixture)
        {
            CaptureCelestial31CasterInputs();
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue, oldPause = Game.Instance.IsPaused;
            UnitEntityData owner = null, sibling = null, victim = null, blocker = null;
            CreateExpandedSummoningCorrectionHostile(fixture);
            Trumpet31CasterSaves saves = null;
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;
                PlaceExpandedSummoningUnit(fixture.Hostile, fixture.Caster.Position + Vector3.forward * 40f);
                foreach (int frame in ReviewOutsiderEscape30And31(fixture)) yield return frame;
                owner = CastExpandedSummoningOwnTier(fixture, "trumpet-archon");
                sibling = CastExpandedSummoningOwnTier(fixture, "trumpet-archon");
                victim = CastExpandedSummoningOwnTier(fixture, "lemure", fixture.Hostile);
                blocker = CastExpandedSummoningOwnTier(fixture, "lemure");
                var units = new[] { owner, sibling, victim, blocker };
                foreach (var unit in units) { Sprint28SetBrainActivePreservingActions(unit, false); unit.Commands.InterruptAll(true); unit.Commands.RemoveFinishedAndUpdateQueue(); }
                // Final successful S28/S29 command fixtures enroll real combat
                // before inspecting action cooldowns. Native RTWP deliberately
                // returns from UpdateCooldowns when the executor is out of combat.
                Vector3 initialFar; string initialLane, initialDirection;
                var initialOrigin = Sprint24AiCombatLane(fixture.Caster, owner, fixture.Hostile, 8f,
                    out initialFar, out initialLane, out initialDirection, new Feet(20).Meters + 1f);
                PlaceExpandedSummoningUnit(owner, initialOrigin); PlaceExpandedSummoningUnit(sibling, initialOrigin + Vector3.right);
                PlaceExpandedSummoningUnit(victim, initialOrigin - Vector3.right * 2f);
                PlaceExpandedSummoningUnit(blocker, initialOrigin - Vector3.right * 3f);
                PlaceExpandedSummoningUnit(fixture.Hostile, initialFar);
                foreach (var unit in new[] { owner, sibling, fixture.Hostile })
                { unit.Wake(0f); if (!Game.Instance.State.AwakeUnits.Contains(unit)) Game.Instance.State.AwakeUnits.Add(unit); }
                foreach (var actor in new[] { owner, sibling })
                { actor.Memory.Add(fixture.Hostile); fixture.Hostile.Memory.Add(actor); actor.SightCache.ClearIfNeeded(); }
                fixture.Hostile.SightCache.ClearIfNeeded(); new Kingmaker.Controllers.Units.UnitMemoryController().Tick();
                owner.JoinCombat(); sibling.JoinCombat(); fixture.Hostile.JoinCombat(); Game.Instance.Player.UpdateIsInCombat();
                new UnitCombatJoinController().Tick(); new UnitCombatPrepareController().Tick();
                for (int frame = 0; frame < 4; frame++) yield return 0;
                bool combatReady = owner.IsInCombat && sibling.IsInCombat && fixture.Hostile.IsInCombat;
                Sprint25Check("trumpet-caster-combat-fixture", combatReady, new JObject { ["owner"] = owner.UniqueId,
                    ["sibling"] = sibling.UniqueId, ["hostile"] = fixture.Hostile.UniqueId,
                    ["ownerInCombat"] = owner.IsInCombat, ["siblingInCombat"] = sibling.IsInCombat,
                    ["hostileInCombat"] = fixture.Hostile.IsInCombat, ["lane"] = initialLane,
                    ["turnBased"] = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue },
                    "Both manual-command actors and the disposable hostile have real native RTWP combat enrollment before any action-cost assertion; brains remain disabled.");
                if (!combatReady) throw new InvalidOperationException("Native caster combat-enrollment preconditions failed.");
                var creation = owner.Blueprint.GetComponent<TrumpetPreparedBook>();
                var book = owner.Descriptor.GetSpellbook(creation.Book); var otherBook = sibling.Descriptor.GetSpellbook(creation.Book);
                var nativeExtra = BlueprintLibraryLookup.RequireExact<Kingmaker.Blueprints.Classes.BlueprintFeature>(BlueprintBootstrap.Library,
                    "136fa0343d5b4b348bdaa05d83408db3", "native extraplanar consumer");
                var preparedInputs = new JArray(creation.Preparations.Select(row => new JObject {
                    ["spell"] = row.Ability.AssetGuid, ["level"] = row.Level, ["copies"] = row.Copies,
                    ["ownerPrepared"] = Trumpet31PreparedCopies(book, row.Ability, row.Level),
                    ["siblingPrepared"] = Trumpet31PreparedCopies(otherBook, row.Ability, row.Level),
                    ["nativeSpellLevel"] = new AbilityData(row.Ability, book).CalculateParams().SpellLevel }));
                bool prepared = preparedInputs.All(row => (int)row["ownerPrepared"] == (int)row["copies"] &&
                    (int)row["siblingPrepared"] == (int)row["copies"] && (int)row["nativeSpellLevel"] == (int)row["level"]);
                Sprint25Check("trumpet-caster-preparations", prepared, new JObject { ["owner"] = owner.UniqueId,
                    ["sibling"] = sibling.UniqueId, ["cl"] = book.CasterLevel, ["preparations"] = preparedInputs },
                    "Every actual initial preparation, including native Magic Vestment, has its own legal native level and independent available copies.");
                if (!prepared) throw new InvalidOperationException("Native caster initial preparations failed.");
                var skillRows = new JArray();
                foreach (int charisma in new[] { 17, 21 })
                {
                    owner.Stats.Charisma.BaseValue = charisma;
                    foreach (var stat in new[] { StatType.CheckDiplomacy, StatType.CheckIntimidate, StatType.CheckBluff })
                    {
                        var check = Rulebook.Trigger(new RuleSkillCheck(owner, stat, 20));
                        skillRows.Add(new JObject { ["charisma"] = charisma, ["stat"] = stat.ToString(),
                            ["bonus"] = (int)check.Bonus, ["nativeStatValue"] = check.StatValue,
                            ["d20"] = check.D20.Value, ["baseRoll"] = check.BaseRollResult, ["rollResult"] = check.RollResult,
                            ["effectiveBonus"] = check.StatValue + (int)check.Bonus,
                            ["nativeRolledBonus"] = check.BaseRollResult - check.D20.Value,
                            ["modifiers"] = new JArray(check.Bonus.Modifiers.Select(value => new JObject {
                                ["value"] = value.ModValue, ["source"] = value.Source?.Blueprint?.AssetGuid })) });
                    }
                }
                owner.Stats.Charisma.BaseValue = 17;
                // Final successful S28 native convention: StatValue excludes the
                // event Bonus; the consumed BaseRollResult includes both and D20.
                bool named = skillRows.Select(row => (int)row["effectiveBonus"]).SequenceEqual(new[] { 24, 5, 3, 26, 7, 5 }) &&
                    skillRows.All(row => (int)row["nativeRolledBonus"] == (int)row["effectiveBonus"]) &&
                    owner.Stats.SkillPerception.ModifiedValue == 22 && units.All(unit => unit.Descriptor.HasFact(nativeExtra));
                Sprint25Check("trumpet-caster-named-skills", named, new JObject { ["owner"] = owner.UniqueId,
                    ["checks"] = skillRows, ["perception"] = owner.Stats.SkillPerception.ModifiedValue,
                    ["nativeExtraplanar"] = new JArray(units.Select(unit => new JObject { ["unit"] = unit.UniqueId,
                        ["present"] = unit.Descriptor.HasFact(nativeExtra) })), ["forcedRolls"] = 0 },
                    "Actual named native skill checks use printed Diplomacy ranks/class skill/Persuasive and untrained Bluff/Intimidate, with live Charisma; no Perception grant. Native extraplanar consumers are present.");
                if (!named) throw new InvalidOperationException("Trumpet native named skill/subtype checks failed.");
                var virtue = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library,
                    "d3a852385ba4cd740992d1970170301a", "native Virtue orison");
                var virtueBuff = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(BlueprintBootstrap.Library,
                    "a13ad2502d9e4904082868eb71efb0c5", "native Virtue temporary HP");
                var orison = new AbilityData(owner.Descriptor.Abilities.GetAbility(virtue));
                var parameters = orison.CalculateParams();
                int[] initialSlots = Enumerable.Range(1, 7).Select(level => book.GetMemorizedSpellSlots(level).Count(slot => slot.Available && slot.Spell != null)).ToArray();
                bool orisonReady = orison.Spellbook == null && virtue.Type == AbilityType.Spell &&
                    parameters.CasterLevel == 14 && parameters.SpellLevel == 0 && parameters.DC == 15 && orison.IsAvailable;
                Sprint25Check("trumpet-caster-orison-ready", orisonReady, new JObject { ["owner"] = owner.UniqueId,
                    ["ability"] = virtue.AssetGuid, ["type"] = virtue.Type.ToString(), ["book"] = orison.Spellbook?.Blueprint.AssetGuid,
                    ["cl"] = parameters.CasterLevel, ["spellLevel"] = parameters.SpellLevel, ["dc"] = parameters.DC,
                    ["available"] = orison.IsAvailable },
                    "The granted native orison has actual direct access and its own book-derived parameters before any effect is attempted.");
                if (!orisonReady) throw new InvalidOperationException("Trumpet native orison access/parameter preconditions failed.");
                foreach (int frame in Trumpet31ManualCommand(owner, orison, owner, "trumpet-caster-virtue-first-command")) yield return frame;
                foreach (int frame in Trumpet31ManualCommand(owner, new AbilityData(owner.Descriptor.Abilities.GetAbility(virtue)), owner,
                    "trumpet-caster-virtue-second-command")) yield return frame;
                var actualVirtue = owner.Buffs.Enumerable.SingleOrDefault(buff => ReferenceEquals(buff.Blueprint, virtueBuff));
                bool orisonEffect = actualVirtue != null && actualVirtue.Context.Params.CasterLevel == 14 && owner.Descriptor.TemporaryHP == 1 &&
                    initialSlots.SequenceEqual(Enumerable.Range(1, 7).Select(level => book.GetMemorizedSpellSlots(level).Count(slot => slot.Available && slot.Spell != null)));
                Sprint25Check("trumpet-caster-virtue-effect", orisonEffect, new JObject { ["owner"] = owner.UniqueId,
                    ["cl"] = actualVirtue?.Context?.Params.CasterLevel, ["temporaryHp"] = owner.Descriptor.TemporaryHP,
                    ["end"] = actualVirtue?.EndTime.ToString(), ["preparedSlotsBefore"] = new JArray(initialSlots),
                    ["preparedSlotsAfter"] = new JArray(Enumerable.Range(1, 7).Select(level => book.GetMemorizedSpellSlots(level).Count(slot => slot.Available && slot.Spell != null))) },
                    "Printed at-will Virtue is a native Spell/Cantrip fact, pays actions twice, grants one temporary HP without stacking and never spends or refills leveled slots.");
                if (!orisonEffect) throw new InvalidOperationException("Native Virtue effect or leveled-resource isolation failed.");
                var resistance = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library,
                    ExpandedSummoningSpecialBuilder.TrumpetResistanceSpell31Guid, "selected legal Resistance cantrip");
                var resistanceBuff = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(BlueprintBootstrap.Library,
                    ExpandedSummoningSpecialBuilder.NativeResistanceBuff31Guid, "captured native Resistance effect");
                int[] savesBeforeResistance = { owner.Stats.SaveFortitude.ModifiedValue,
                    owner.Stats.SaveReflex.ModifiedValue, owner.Stats.SaveWill.ModifiedValue };
                foreach (int frame in Trumpet31ManualCommand(owner,
                    new AbilityData(owner.Descriptor.Abilities.GetAbility(resistance)), owner,
                    "trumpet-caster-resistance-first-command")) yield return frame;
                foreach (int frame in Trumpet31ManualCommand(owner,
                    new AbilityData(owner.Descriptor.Abilities.GetAbility(resistance)), owner,
                    "trumpet-caster-resistance-second-command")) yield return frame;
                var resistances = owner.Buffs.Enumerable.Where(buff => ReferenceEquals(buff.Blueprint, resistanceBuff)).ToArray();
                int[] savesAfterResistance = { owner.Stats.SaveFortitude.ModifiedValue,
                    owner.Stats.SaveReflex.ModifiedValue, owner.Stats.SaveWill.ModifiedValue };
                bool resisted = resistances.Length == 1 && ReferenceEquals(resistances[0].Context.MaybeCaster, owner) &&
                    resistances[0].Context.Params.CasterLevel == 14 &&
                    savesAfterResistance.SequenceEqual(savesBeforeResistance.Select(value => value + 1)) &&
                    initialSlots.SequenceEqual(Enumerable.Range(1, 7).Select(level => book.GetMemorizedSpellSlots(level).Count(slot => slot.Available && slot.Spell != null)));
                Sprint25Check("trumpet-caster-resistance-effect", resisted, new JObject {
                    ["owner"] = owner.UniqueId, ["buffCount"] = resistances.Length,
                    ["cl"] = resistances.FirstOrDefault()?.Context?.Params.CasterLevel,
                    ["savesBefore"] = new JArray(savesBeforeResistance), ["savesAfter"] = new JArray(savesAfterResistance),
                    ["availableAgain"] = new AbilityData(owner.Descriptor.Abilities.GetAbility(resistance)).IsAvailable,
                    ["leveledSlotsBefore"] = new JArray(initialSlots),
                    ["leveledSlotsAfter"] = new JArray(Enumerable.Range(1, 7).Select(level => book.GetMemorizedSpellSlots(level).Count(slot => slot.Available && slot.Spell != null))) },
                    "The legal native Resistance cantrip pays two actual standard actions, supplies one nonstacking +1 resistance save effect at CL14 and neither spends nor refills leveled slots.");
                if (!resisted) throw new InvalidOperationException("Native Resistance parameters/effect/resource isolation failed.");

                var freedomBuff = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(BlueprintBootstrap.Library,
                    ExpandedSummoningSpecialBuilder.NativeFreedomBuff31Guid, "published native Freedom of Movement effect");
                var freedom = creation.Preparations.Single(row => row.Level == 4 &&
                    ExpandedSummoningSpecialBuilder.IsTrumpetFreedomPreparation31(row.Ability, freedomBuff)).Ability;
                var freedomSlot = book.GetMemorizedSpellSlots(4).Single(slot => slot.Available && ReferenceEquals(slot.Spell?.Blueprint, freedom));
                freedomSlot.Spell.ParamSpellSlot = freedomSlot;
                var freedomEffects = new Trumpet31CasterEffects(owner);
                EventBus.Subscribe(freedomEffects);
                try
                {
                    foreach (int frame in Trumpet31ManualCommand(owner, freedomSlot.Spell, owner,
                        "trumpet-caster-freedom-command")) yield return frame;
                }
                finally { EventBus.Unsubscribe(freedomEffects); }
                var actualFreedom = owner.Buffs.Enumerable.SingleOrDefault(buff => ReferenceEquals(buff.Blueprint, freedomBuff));
                double freedomRemaining = actualFreedom == null ? -1 :
                    (actualFreedom.EndTime - Game.Instance.TimeController.GameTime).TotalSeconds;
                bool freed = actualFreedom != null && ReferenceEquals(actualFreedom.Context.MaybeCaster, owner) &&
                    actualFreedom.Context.Params.CasterLevel == 14 && freedomRemaining > 8390 && freedomRemaining <= 8400.1 &&
                    !freedomSlot.Available && Trumpet31PreparedCopies(otherBook, freedom, 4) == 1 &&
                    initialSlots.Select((value, index) => index == 3 ? value - 1 : value).SequenceEqual(
                        Enumerable.Range(1, 7).Select(level => book.GetMemorizedSpellSlots(level).Count(slot => slot.Available && slot.Spell != null)));
                Sprint25Check("trumpet-caster-freedom-effect", freed, new JObject {
                    ["owner"] = owner.UniqueId, ["cl"] = actualFreedom?.Context?.Params.CasterLevel,
                    ["sourceAbility"] = actualFreedom?.Context?.SourceAbility?.AssetGuid,
                    ["remainingSeconds"] = freedomRemaining, ["nativeEffects"] = freedomEffects.Rows,
                    ["ownSlotAvailable"] = freedomSlot.Available, ["siblingCopies"] = Trumpet31PreparedCopies(otherBook, freedom, 4),
                    ["nativeParalysisImmunity"] = owner.Descriptor.State.HasConditionImmunity(UnitCondition.Paralyzed) },
                    "The legal fourth-level native Freedom of Movement preparation pays one standard action, applies its actual CL14/140-minute protection and spends exactly its own slot while the sibling's copy remains available. Its published native protection consumer is retained; this row is spell/access/resource proof.");
                if (!freed) throw new InvalidOperationException("Native Freedom preparation/effect/resource isolation failed.");
                // Native sight caps player units at11.7m and an out-of-combat
                // enemy at8.5m. Enroll this exact disposable pair before the
                // survey;10.5m fits real combat sight and clears a30ft burst.
                victim.Wake(0f); if (!Game.Instance.State.AwakeUnits.Contains(victim)) Game.Instance.State.AwakeUnits.Add(victim);
                owner.Memory.Add(victim); victim.Memory.Add(owner);
                owner.JoinCombat(); victim.JoinCombat(); Game.Instance.Player.UpdateIsInCombat();
                new UnitCombatJoinController().Tick(); new UnitCombatPrepareController().Tick();
                for (int frame = 0; frame < 4; frame++) yield return 0;
                bool offenseCombat = owner.IsEnemy(victim) && !owner.IsEnemy(blocker) && owner.IsInCombat && victim.IsInCombat &&
                    owner.VisionRangeMeters > 10.5f && victim.VisionRangeMeters > 10.5f;
                Sprint25Check("trumpet-caster-offense-combat", offenseCombat, new JObject {
                    ["owner"] = owner.UniqueId, ["victim"] = victim.UniqueId,
                    ["ownerInCombat"] = owner.IsInCombat, ["victimInCombat"] = victim.IsInCombat,
                    ["ownerVisionMeters"] = owner.VisionRangeMeters, ["victimVisionMeters"] = victim.VisionRangeMeters,
                    ["ownerConsidersVictimEnemy"] = owner.IsEnemy(victim), ["ownerConsidersBlockerEnemy"] = owner.IsEnemy(blocker),
                    ["victimSummoner"] = victim.Get<Kingmaker.UnitLogic.Parts.UnitPartSummonedMonster>()?.Summoner?.UniqueId,
                    ["expectedHostileSummoner"] = fixture.Hostile.UniqueId, ["victimGroup"] = victim.GroupId, ["ownerGroup"] = owner.GroupId },
                    "The actual enemy pair is in native combat and can see a10.5m lane before searching; no sight or immunity setting is changed.");
                if (!offenseCombat) throw new InvalidOperationException("Native caster offense combat/sight preconditions failed.");
                Vector3 far; string lane, direction;
                var origin = Sprint24AiCombatLane(fixture.Caster, owner, victim, 10.5f, out far, out lane, out direction,
                    new Feet(30).Meters + 0.1f);
                PlaceExpandedSummoningUnit(owner, origin); PlaceExpandedSummoningUnit(sibling, origin + Vector3.right);
                PlaceExpandedSummoningUnit(victim, far); PlaceExpandedSummoningUnit(blocker, far + Vector3.right);
                PlaceExpandedSummoningUnit(fixture.Hostile, origin + Vector3.forward * 40f);
                foreach (var unit in new[] { owner, victim })
                { unit.Wake(0f); if (!Game.Instance.State.AwakeUnits.Contains(unit)) Game.Instance.State.AwakeUnits.Add(unit); }
                owner.Memory.Add(victim); victim.Memory.Add(owner); owner.SightCache.ClearIfNeeded(); victim.SightCache.ClearIfNeeded();
                new Kingmaker.Controllers.Units.UnitMemoryController().Tick();
                owner.JoinCombat(); victim.JoinCombat(); Game.Instance.Player.UpdateIsInCombat();
                new UnitCombatJoinController().Tick(); new UnitCombatPrepareController().Tick();
                var safety = fixture.Blueprints.OfType<TrumpetBanishmentEligibility>().Single();
                Func<float> safeScore = () => safety.Score(new Kingmaker.Controllers.Brain.DecisionContext {
                    Unit = owner, Target = new Kingmaker.Controllers.Brain.TargetInfo { Unit = victim } });
                var banishment = creation.Preparations.Single(row => row.Ability.AssetGuid == "d361391f645db984bbf58907711a146a").Ability;
                bool unsafeTarget = owner.IsEnemy(victim) && !owner.IsEnemy(blocker) && safeScore() == 0f &&
                    Trumpet31PreparedCopies(book, banishment, 6) == 1 && Trumpet31PreparedCopies(otherBook, banishment, 6) == 1;
                Sprint25Check("trumpet-caster-banishment-ally-refusal", unsafeTarget, new JObject { ["owner"] = owner.UniqueId,
                    ["victim"] = victim.UniqueId, ["blocker"] = blocker.UniqueId,
                    ["ownerConsidersVictimEnemy"] = owner.IsEnemy(victim), ["ownerConsidersBlockerEnemy"] = owner.IsEnemy(blocker),
                    ["score"] = safeScore(),
                    ["distance"] = owner.DistanceTo(victim), ["lane"] = lane, ["ownSlots"] = Trumpet31PreparedCopies(book, banishment, 6),
                    ["siblingSlots"] = Trumpet31PreparedCopies(otherBook, banishment, 6) },
                    "An eligible owned ally in the unchanged native burst blocks Banishment selection; both real prepared slots remain available.");
                if (!unsafeTarget) throw new InvalidOperationException("Banishment ally-refusal fixture/consumer failed.");
                UnitUseAbility aidCast = null;
                foreach (int frame in Trumpet31ObserveAiSpell(owner, "KMG_Summoning_OutsiderCelestial_TrumpetArchon_AidAi",
                    "trumpet-caster-aid-native-ai", value => aidCast = value)) yield return frame;
                var aidTarget = aidCast?.Target?.Unit;
                var aidBuff = aidTarget?.Buffs.Enumerable.FirstOrDefault(buff => buff.Blueprint.AssetGuid == "319b4679f25779e4e9d04360381254e1" &&
                    ReferenceEquals(buff.Context.MaybeCaster, owner));
                int aidHitPointBonus = Math.Min(10, aidBuff?.Context?.Params?.CasterLevel ?? 0);
                bool aided = aidBuff != null && aidBuff.Context.Params.CasterLevel == 14 &&
                    aidTarget.Descriptor.TemporaryHP >= 1 + aidHitPointBonus && aidTarget.Descriptor.TemporaryHP <= 8 + aidHitPointBonus &&
                    Trumpet31PreparedCopies(book, banishment, 6) == 1 && Trumpet31HealSlots(book, creation.Heal) == 2 &&
                    Trumpet31HealSlots(otherBook, creation.Heal) == 2 && !victim.Destroyed;
                Sprint25Check("trumpet-caster-aid-effect", aided, new JObject { ["caster"] = owner.UniqueId,
                    ["target"] = aidTarget?.UniqueId, ["cl"] = aidBuff?.Context?.Params.CasterLevel,
                    ["temporaryHp"] = aidTarget?.Descriptor?.TemporaryHP, ["temporaryHpCasterBonus"] = aidHitPointBonus,
                    ["temporaryHpBounds"] = new JArray(1 + aidHitPointBonus, 8 + aidHitPointBonus), ["banishmentSlot"] = Trumpet31PreparedCopies(book, banishment, 6),
                    ["healSlots"] = Trumpet31HealSlots(book, creation.Heal), ["siblingHealSlots"] = Trumpet31HealSlots(otherBook, creation.Heal) },
                    "With unsafe Banishment refused, the native brain chooses useful at-will CL14 Aid and applies real support without spending any prepared spell slot.");
                if (!aided) throw new InvalidOperationException("Trumpet native Aid effect/slot isolation failed.");
                PlaceExpandedSummoningUnit(blocker, origin - Vector3.right * 3f);
                victim.Stats.SaveWill.BaseValue = -100; // owned accuracy fixture; the native die/save is not forced
                for (int frame = 0; frame < 8; frame++) yield return 0;
                bool clear = safeScore() > 0f && owner.IsEnemy(victim) && new AbilityData(banishment, book).CanTarget(new TargetWrapper(victim));
                Sprint25Check("trumpet-caster-banishment-clear", clear, new JObject { ["owner"] = owner.UniqueId,
                    ["victim"] = victim.UniqueId, ["score"] = safeScore(), ["nativeExtraplanar"] = victim.Descriptor.HasFact(nativeExtra),
                    ["targetHd"] = victim.Descriptor.Progression.CharacterLevel, ["fixtureWillBase"] = victim.Stats.SaveWill.BaseValue,
                    ["nativeCanTarget"] = new AbilityData(banishment, book).CanTarget(new TargetWrapper(victim)),
                    ["ownerConsidersVictimEnemy"] = owner.IsEnemy(victim), ["ownerConsidersBlockerEnemy"] = owner.IsEnemy(blocker) },
                    "Moving only the disposable ally establishes a clear legal native Banishment target within the CL14 budget; no slot or effect has been injected.");
                if (!clear) throw new InvalidOperationException("Native Banishment clear-target preconditions failed.");
                saves = new Trumpet31CasterSaves(victim); EventBus.Subscribe(saves);
                foreach (int frame in Trumpet31ObserveAiSpell(owner, "KMG_Summoning_OutsiderCelestial_TrumpetArchon_BanishmentAi",
                    "trumpet-caster-banishment-native-ai", value => { })) yield return frame;
                var ownSaves = saves.Rows.Where(row => (string)row["sourceAbility"] == banishment.AssetGuid &&
                    (string)row["caster"] == owner.UniqueId).ToArray();
                bool banished = ownSaves.Length == 1 && (int)ownSaves[0]["dc"] == 21 && !(bool)ownSaves[0]["passed"] &&
                    (victim.Destroyed || !victim.IsInState || victim.Descriptor.State.IsDead) &&
                    Trumpet31PreparedCopies(book, banishment, 6) == 0 && Trumpet31PreparedCopies(otherBook, banishment, 6) == 1 &&
                    !blocker.Destroyed && !blocker.Descriptor.State.IsDead && !sibling.Destroyed;
                Sprint25Check("trumpet-caster-banishment-effect", banished, new JObject { ["caster"] = owner.UniqueId,
                    ["victim"] = victim.UniqueId, ["saves"] = saves.Rows, ["victimDestroyed"] = victim.Destroyed,
                    ["victimInState"] = victim.IsInState, ["victimDead"] = victim.Descriptor.State.IsDead,
                    ["ownSlots"] = Trumpet31PreparedCopies(book, banishment, 6), ["siblingSlots"] = Trumpet31PreparedCopies(otherBook, banishment, 6),
                    ["blockerAlive"] = !blocker.Destroyed && !blocker.Descriptor.State.IsDead, ["forcedRolls"] = 0 },
                    "A genuine native DC21 failed save removes the extraplanar enemy and spends exactly one own prepared Banishment; other owners/slots survive. A native successful save leaves this effect requirement unmet, never relabeled product PASS.");
                if (!banished) throw new InvalidOperationException("Native Banishment effect/save/slot requirement unmet; inspect actual outcome.");
                var vestment = creation.Preparations.Single(row => row.Ability.AssetGuid == "2d4263d80f5136b4296d6eb43a221d7d").Ability;
                var vestmentArmor = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library,
                    "956309af83352714aa7ee89fb4ecf201", "captured native Magic Vestment armor variant");
                var vestmentSlot = otherBook.GetMemorizedSpellSlots(3).Single(slot => slot.Available && ReferenceEquals(slot.Spell?.Blueprint, vestment));
                vestmentSlot.Spell.ParamSpellSlot = vestmentSlot;
                var armorData = new AbilityData(vestmentSlot.Spell, vestmentArmor);
                // Independent spell-effect diagnosis is last: it must not mask
                // support/offense evidence or contaminate their initial slots.
                var vestmentTarget = sibling; // native self-cast never contests the caster's own SR
                int acBefore = vestmentTarget.Stats.AC.ModifiedValue;
                var effectTrace = new Trumpet31CasterEffects(vestmentTarget);
                var nativeVestmentBuff = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(BlueprintBootstrap.Library,
                    "9e265139cf6c07c4fb8298cb8b646de9", "captured native Magic Vestment buff");
                RuntimeTestResultWriter.WriteAtomic(System.IO.Path.Combine(_request.EvidenceDirectory,
                    "trumpet-caster-vestment-inputs.json"), new JObject {
                    ["runId"] = _request.RunId, ["mvid"] = typeof(RuntimeTestRunner).Assembly.ManifestModule.ModuleVersionId.ToString(),
                    ["spell"] = DescribeGraph(vestmentArmor, 0, new HashSet<object>(NativeDonorReferenceComparer.Instance), 8),
                    ["buff"] = DescribeGraph(nativeVestmentBuff, 0, new HashSet<object>(NativeDonorReferenceComparer.Instance), 6),
                    ["spellResistance"] = vestmentArmor.SpellResistance,
                    ["effectOnAlly"] = vestmentArmor.EffectOnAlly.ToString(),
                    ["effectOnEnemy"] = vestmentArmor.EffectOnEnemy.ToString(),
                    ["targetArmor"] = vestmentTarget.Body.Armor.MaybeArmor?.Blueprint?.AssetGuid,
                    ["targetBuffsBefore"] = new JArray(vestmentTarget.Buffs.Enumerable.Select(buff => new JObject {
                        ["buff"] = buff.Blueprint.AssetGuid, ["caster"] = buff.Context?.MaybeCaster?.UniqueId,
                        ["end"] = buff.EndTime.ToString() })) }.ToString());
                EventBus.Subscribe(effectTrace);
                try { foreach (int frame in Trumpet31ManualCommand(sibling, armorData, vestmentTarget, "trumpet-caster-vestment-command")) yield return frame; }
                finally { EventBus.Unsubscribe(effectTrace); }
                var vestmentBuff = vestmentTarget.Buffs.Enumerable.FirstOrDefault(buff => buff.Blueprint.AssetGuid == "9e265139cf6c07c4fb8298cb8b646de9");
                bool armor = vestmentBuff != null && ReferenceEquals(vestmentBuff.Context.MaybeCaster, sibling) &&
                    vestmentBuff.Context.Params.CasterLevel == 14 && vestmentTarget.Stats.AC.ModifiedValue == acBefore + 3 &&
                    !vestmentSlot.Available && Trumpet31PreparedCopies(book, vestment, 3) == 1 &&
                    effectTrace.Rows.Any(row => (string)row["kind"] == "native-buff-application" &&
                        (string)row["buff"] == nativeVestmentBuff.AssetGuid && (bool)row["applied"] &&
                        (double?)row["durationSeconds"] == 14d * 3600d);
                Sprint25Check("trumpet-caster-vestment-effect", armor, new JObject { ["caster"] = sibling.UniqueId,
                    ["target"] = vestmentTarget.UniqueId, ["acBefore"] = acBefore, ["acAfter"] = vestmentTarget.Stats.AC.ModifiedValue,
                    ["cl"] = vestmentBuff?.Context?.Params.CasterLevel, ["casterRootSlotAvailable"] = vestmentSlot.Available,
                    ["otherRootSlots"] = Trumpet31PreparedCopies(book, vestment, 3),
                    ["nativeEffects"] = effectTrace.Rows,
                    ["selfCast"] = ReferenceEquals(sibling, vestmentTarget),
                    ["acModifiers"] = new JArray(vestmentTarget.Stats.AC.Modifiers.Select(modifier => new JObject {
                        ["value"] = modifier.ModValue, ["descriptor"] = modifier.ModDescriptor.ToString(),
                        ["source"] = modifier.Source?.Blueprint?.AssetGuid })),
                    ["targetBuffsAfter"] = new JArray(vestmentTarget.Buffs.Enumerable.Select(buff => new JObject {
                        ["buff"] = buff.Blueprint.AssetGuid, ["caster"] = buff.Context?.MaybeCaster?.UniqueId,
                        ["cl"] = buff.Context?.Params?.CasterLevel, ["end"] = buff.EndTime.ToString() })) },
                    "The native armor variant self-cast grants the actual CL14 enhancement/14-hour buff to ordinary clothing and spends only its selected parent's prepared slot; the other book stays independent. External native SR remains intact and is not disabled to force success.");
                if (!armor) throw new InvalidOperationException("Native vestment armor/variant/slot consumer failed.");
            }
            finally
            {
                if (saves != null) EventBus.Unsubscribe(saves);
                foreach (var unit in new[] { owner, sibling, victim, blocker }.Where(value => value != null && !value.Destroyed))
                { Sprint28SetBrainActivePreservingActions(unit, false); unit.Commands.InterruptAll(true); }
                DisposeExpandedSummoningUnits(fixture.Created, new[] { owner, sibling, victim, blocker }.Where(value => value != null && !value.Destroyed).ToArray());
                ResetExpandedSummoningHostile(fixture); SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode;
                Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = oldPause;
            }
        }

        private sealed class Trumpet31CasterEffects : IGlobalRulebookHandler<RuleSpellResistanceCheck>, IGlobalRulebookHandler<RuleApplyBuff>
        {
            private readonly UnitEntityData _target;
            internal readonly JArray Rows = new JArray();
            internal Trumpet31CasterEffects(UnitEntityData target) { _target = target; }
            public void OnEventAboutToTrigger(RuleSpellResistanceCheck evt) { }
            public void OnEventDidTrigger(RuleSpellResistanceCheck evt)
            {
                if (!ReferenceEquals(evt.Target, _target)) return;
                Rows.Add(new JObject { ["kind"] = "native-spell-resistance", ["target"] = _target.UniqueId,
                    ["ability"] = evt.Ability?.AssetGuid, ["caster"] = evt.Context?.MaybeCaster?.UniqueId,
                    ["cl"] = evt.Context?.Params?.CasterLevel, ["resistance"] = evt.SpellResistance,
                    ["penetration"] = evt.SpellPenetration, ["ignore"] = evt.IgnoreSpellResistance,
                    ["immune"] = evt.TargetIsImmune, ["resisted"] = evt.IsSpellResisted,
                    ["hasRoll"] = evt.HasResistanceRoll, ["nativeD20"] = evt.HasResistanceRoll ? (int?)evt.Roll.Value : null });
            }
            public void OnEventAboutToTrigger(RuleApplyBuff evt) { }
            public void OnEventDidTrigger(RuleApplyBuff evt)
            {
                if (!ReferenceEquals(evt.Initiator, _target)) return;
                Rows.Add(new JObject { ["kind"] = "native-buff-application", ["target"] = _target.UniqueId,
                    ["buff"] = evt.Blueprint?.AssetGuid, ["ability"] = evt.Context?.SourceAbility?.AssetGuid,
                    ["caster"] = evt.Context?.MaybeCaster?.UniqueId, ["cl"] = evt.Context?.Params?.CasterLevel,
                    ["durationSeconds"] = evt.Duration?.TotalSeconds, ["canApply"] = evt.CanApply,
                    ["immune"] = evt.Immunity, ["storyImmune"] = evt.StoryModeImmunity,
                    ["applied"] = evt.AppliedBuff != null, ["end"] = evt.AppliedBuff?.EndTime.ToString() });
            }
        }

        private sealed class Trumpet31CasterSaves : IGlobalRulebookHandler<RuleSavingThrow>
        {
            private readonly UnitEntityData _target;
            internal readonly JArray Rows = new JArray();
            internal Trumpet31CasterSaves(UnitEntityData target) { _target = target; }
            public void OnEventAboutToTrigger(RuleSavingThrow evt) { }
            public void OnEventDidTrigger(RuleSavingThrow evt)
            {
                if (!ReferenceEquals(evt.Initiator, _target)) return;
                Rows.Add(new JObject { ["target"] = evt.Initiator.UniqueId,
                    ["d20"] = evt.D20.Value, ["nativeStatValue"] = evt.StatValue,
                    ["dc"] = evt.DifficultyClass, ["passed"] = evt.IsPassed,
                    ["sourceAbility"] = evt.Reason?.Context?.SourceAbility?.AssetGuid,
                    ["caster"] = evt.Reason?.Context?.MaybeCaster?.UniqueId });
            }
        }
    }
}
