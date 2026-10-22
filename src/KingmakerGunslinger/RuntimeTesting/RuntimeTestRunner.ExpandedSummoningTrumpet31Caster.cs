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
            if (!data.IsAvailable || !data.CanTarget(wrapped))
                throw new InvalidOperationException("Native caster manual-command preconditions failed: " + id);
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
                    ["executionEnded"] = command.ExecutionProcess?.IsEnded, ["costs"] = costs,
                    ["manualNativeCommand"] = true, ["fixtureEffectExecutions"] = 0, ["fixtureSlotDebits"] = 0 },
                    "A real manual native command completes and pays one standard action; no AI claim or direct effect/resource execution.");
                if (!exact) throw new InvalidOperationException("Native caster manual command failed: " + id);
            }
        }

        private IEnumerable<int> Trumpet31ObserveAiSpell(UnitEntityData actor, string aiName,
            string id, Action<UnitUseAbility> observe)
        {
            foreach (var command in actor.Commands.Raw.Where(value => value != null && !value.IsFinished))
                throw new InvalidOperationException("Caster AI subcase starts with a live command: " + command.GetType().Name);
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
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue, oldPause = Game.Instance.IsPaused;
            UnitEntityData owner = null, sibling = null, victim = null, blocker = null;
            CreateExpandedSummoningCorrectionHostile(fixture);
            Dretch30CloudSaves saves = null;
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;
                PlaceExpandedSummoningUnit(fixture.Hostile, fixture.Caster.Position + Vector3.forward * 40f);
                owner = CastExpandedSummoningOwnTier(fixture, "trumpet-archon");
                sibling = CastExpandedSummoningOwnTier(fixture, "trumpet-archon");
                victim = CastExpandedSummoningOwnTier(fixture, "lemure");
                blocker = CastExpandedSummoningOwnTier(fixture, "lemure");
                var units = new[] { owner, sibling, victim, blocker };
                foreach (var unit in units) { Sprint28SetBrainActivePreservingActions(unit, false); unit.Commands.InterruptAll(true); unit.Commands.RemoveFinishedAndUpdateQueue(); }
                var creation = owner.Blueprint.GetComponent<TrumpetPreparedBook>();
                var book = owner.Descriptor.GetSpellbook(creation.Book); var otherBook = sibling.Descriptor.GetSpellbook(creation.Book);
                var nativeExtra = BlueprintLibraryLookup.RequireExact<Kingmaker.Blueprints.Facts.BlueprintUnitFact>(BlueprintBootstrap.Library,
                    "136fa0343d5b4b348bdaa05d83408db3", "native extraplanar consumer");
                var skillRows = new JArray();
                foreach (int charisma in new[] { 17, 21 })
                {
                    owner.Stats.Charisma.BaseValue = charisma;
                    foreach (var stat in new[] { StatType.CheckDiplomacy, StatType.CheckIntimidate, StatType.CheckBluff })
                    {
                        var check = Rulebook.Trigger(new RuleSkillCheck(owner, stat, 20));
                        skillRows.Add(new JObject { ["charisma"] = charisma, ["stat"] = stat.ToString(),
                            ["bonus"] = (int)check.Bonus, ["baseRoll"] = check.BaseRollResult, ["rollResult"] = check.RollResult,
                            ["modifiers"] = new JArray(check.Bonus.Modifiers.Select(value => new JObject {
                                ["value"] = value.ModValue, ["source"] = value.Source?.Blueprint?.AssetGuid })) });
                    }
                }
                owner.Stats.Charisma.BaseValue = 17;
                bool named = skillRows.Select(row => (int)row["bonus"]).SequenceEqual(new[] { 24, 5, 3, 26, 7, 5 }) &&
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
                var vestment = creation.Preparations.Single(row => row.Ability.AssetGuid == "2d4263d80f5136b4296d6eb43a221d7d").Ability;
                var vestmentArmor = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library,
                    "956309af83352714aa7ee89fb4ecf201", "captured native Magic Vestment armor variant");
                var vestmentSlot = otherBook.GetMemorizedSpellSlots(3).Single(slot => slot.Available && ReferenceEquals(slot.Spell?.Blueprint, vestment));
                vestmentSlot.Spell.ParamSpellSlot = vestmentSlot;
                var armorData = new AbilityData(vestmentSlot.Spell, vestmentArmor);
                int acBefore = owner.Stats.AC.ModifiedValue;
                foreach (int frame in Trumpet31ManualCommand(sibling, armorData, owner, "trumpet-caster-vestment-command")) yield return frame;
                var vestmentBuff = owner.Buffs.Enumerable.FirstOrDefault(buff => buff.Blueprint.AssetGuid == "9e265139cf6c07c4fb8298cb8b646de9");
                bool armor = vestmentBuff != null && ReferenceEquals(vestmentBuff.Context.MaybeCaster, sibling) &&
                    vestmentBuff.Context.Params.CasterLevel == 14 && owner.Stats.AC.ModifiedValue == acBefore + 3 &&
                    !vestmentSlot.Available && Trumpet31PreparedCopies(book, vestment, 3) == 1;
                Sprint25Check("trumpet-caster-vestment-effect", armor, new JObject { ["caster"] = sibling.UniqueId,
                    ["target"] = owner.UniqueId, ["acBefore"] = acBefore, ["acAfter"] = owner.Stats.AC.ModifiedValue,
                    ["cl"] = vestmentBuff?.Context?.Params.CasterLevel, ["casterRootSlotAvailable"] = vestmentSlot.Available,
                    ["otherRootSlots"] = Trumpet31PreparedCopies(book, vestment, 3) },
                    "The native armor variant grants its actual CL14 enhancement to ordinary clothing and spends only the selected parent's prepared slot; the other book stays independent.");
                if (!armor) throw new InvalidOperationException("Native vestment armor/variant/slot consumer failed.");
                victim.Descriptor.SwitchFactions(fixture.Hostile.Faction, true);
                Vector3 far; string lane, direction;
                var origin = Sprint24AiCombatLane(fixture.Caster, owner, victim, 15f, out far, out lane, out direction,
                    new Feet(40).Meters + 0.5f);
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
                    ["victim"] = victim.UniqueId, ["blocker"] = blocker.UniqueId, ["score"] = safeScore(),
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
                bool aided = aidBuff != null && aidBuff.Context.Params.CasterLevel == 14 && aidTarget.Descriptor.TemporaryHP >= 15 &&
                    Trumpet31PreparedCopies(book, banishment, 6) == 1 && Trumpet31HealSlots(book, creation.Heal) == 2 &&
                    Trumpet31HealSlots(otherBook, creation.Heal) == 2 && !victim.Destroyed;
                Sprint25Check("trumpet-caster-aid-effect", aided, new JObject { ["caster"] = owner.UniqueId,
                    ["target"] = aidTarget?.UniqueId, ["cl"] = aidBuff?.Context?.Params.CasterLevel,
                    ["temporaryHp"] = aidTarget?.Descriptor?.TemporaryHP, ["banishmentSlot"] = Trumpet31PreparedCopies(book, banishment, 6),
                    ["healSlots"] = Trumpet31HealSlots(book, creation.Heal), ["siblingHealSlots"] = Trumpet31HealSlots(otherBook, creation.Heal) },
                    "With unsafe Banishment refused, the native brain chooses useful at-will CL14 Aid and applies real support without spending any prepared spell slot.");
                if (!aided) throw new InvalidOperationException("Trumpet native Aid effect/slot isolation failed.");
                PlaceExpandedSummoningUnit(blocker, origin - Vector3.right * 3f);
                victim.Stats.SaveWill.BaseValue = -100; // owned accuracy fixture; the native die/save is not forced
                for (int frame = 0; frame < 8; frame++) yield return 0;
                bool clear = safeScore() > 0f && owner.IsEnemy(victim) && new AbilityData(banishment, book).CanTarget(new TargetWrapper(victim));
                Sprint25Check("trumpet-caster-banishment-clear", clear, new JObject { ["owner"] = owner.UniqueId,
                    ["victim"] = victim.UniqueId, ["score"] = safeScore(), ["nativeExtraplanar"] = victim.Descriptor.HasFact(nativeExtra),
                    ["targetHd"] = victim.Descriptor.Progression.CharacterLevel, ["fixtureWillBase"] = victim.Stats.SaveWill.BaseValue },
                    "Moving only the disposable ally establishes a clear legal native Banishment target within the CL14 budget; no slot or effect has been injected.");
                if (!clear) throw new InvalidOperationException("Native Banishment clear-target preconditions failed.");
                saves = new Dretch30CloudSaves(new[] { victim }); EventBus.Subscribe(saves);
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
    }
}
