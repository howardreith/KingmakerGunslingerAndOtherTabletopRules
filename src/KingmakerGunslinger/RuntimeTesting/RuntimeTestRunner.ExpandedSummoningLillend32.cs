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
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> ReviewLillend32Support(ExpandedSummoningCorrectionFixture fixture)
        {
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue, oldPause = Game.Instance.IsPaused;
            UnitEntityData owner = null, sibling = null;
            CreateExpandedSummoningCorrectionHostile(fixture);
            var target = fixture.Hostile; int ac = target.Stats.AC.BaseValue;
            BlueprintFeature mindImmunity = null;
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;
                owner = CastExpandedSummoningOwnTier(fixture, "lillend-azata");
                sibling = CastExpandedSummoningOwnTier(fixture, "lillend-azata");
                foreach (var unit in new[] { owner, sibling })
                { Sprint28SetBrainActivePreservingActions(unit, false); unit.Commands.InterruptAll(true); unit.Commands.RemoveFinishedAndUpdateQueue(); }
                var creation = owner.Blueprint.GetComponents<LillendBardBook32>().Single();
                var book = owner.Descriptor.GetSpellbook(creation.Book);
                var siblingBook = sibling.Descriptor.GetSpellbook(creation.Book);
                var resource = fixture.Blueprints.OfType<BlueprintAbilityResource>().Single(value =>
                    value.name == "KMG_Summoning_FeyCelestial_LillendAzata_PerformanceResource");
                var performAi = owner.Blueprint.Brain.Actions.OfType<LillendCourageAi32>().Single();
                var performance = owner.Descriptor.ActivatableAbilities.Enumerable.Single(value =>
                    ReferenceEquals(value.Blueprint, performAi.Courage));
                var siblingPerformance = sibling.Descriptor.ActivatableAbilities.Enumerable.Single(value =>
                    ReferenceEquals(value.Blueprint, performAi.Courage));
                var heal = fixture.Blueprints.OfType<LillendHealingEligibility32>().Single();
                bool mapping = book != null && siblingBook != null && !ReferenceEquals(book, siblingBook) &&
                    book.Blueprint.Spontaneous && book.CasterLevel == 7 &&
                    new[] { 1, 2, 3 }.Select(book.GetSpontaneousSlots).SequenceEqual(new[] { 5, 4, 2 }) &&
                    new[] { 1, 2, 3 }.Select(siblingBook.GetSpontaneousSlots).SequenceEqual(new[] { 5, 4, 2 }) &&
                    owner.Descriptor.Resources.GetResourceAmount(resource) == 20 &&
                    sibling.Descriptor.Resources.GetResourceAmount(resource) == 20 &&
                    performance.IsActivateWithSameCommand(performance) &&
                    !performance.IsActivateWithSameCommand(siblingPerformance);
                Sprint25Check("lillend-support-native-mapping", mapping, new JObject {
                    ["owner"] = owner.UniqueId, ["sibling"] = sibling.UniqueId, ["cl"] = book?.CasterLevel,
                    ["hp"] = owner.Stats.HitPoints.ModifiedValue, ["slots"] = book == null ? null : new JArray(new[] { 1, 2, 3 }.Select(book.GetSpontaneousSlots)),
                    ["performanceRounds"] = owner.Descriptor.Resources.GetResourceAmount(resource),
                    ["samePerformanceCommand"] = performance.IsActivateWithSameCommand(performance),
                    ["siblingPerformanceCommandRejected"] = !performance.IsActivateWithSameCommand(siblingPerformance),
                    ["independentBooks"] = !ReferenceEquals(book, siblingBook), ["finalProfileQualification"] = false },
                    "Native spontaneous bard7 slots5/4/2 and20 performance rounds are independent per summon.");
                if (!mapping) throw new InvalidOperationException("Lillend support mapping failed.");
                var tail = owner.Body.AdditionalLimbs.Single().MaybeWeapon;
                var providerField = typeof(UnityModManagerNet.UnityModManager).GetField("modEntries",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                var providerList = providerField?.GetValue(null) as System.Collections.IEnumerable;
                if (providerList == null) throw new InvalidOperationException("Native UMM provider inventory unavailable.");
                var providers = providerList.Cast<object>().OfType<UnityModManagerNet.UnityModManager.ModEntry>()
                    .Where(value => value.Info?.Id == Sprint32Suggestion.ModId).ToArray();
                bool expectedSuggestion = providers.Length != 0;
                bool providerReady = providers.Length == 0 || providers.Length == 1 && providers[0].Loaded &&
                    providers[0].Active && !providers[0].ErrorOnLoading && providers[0].Assembly != null;
                var bookKnown = new JObject(Enumerable.Range(0, 4).Select(level => new JProperty(level.ToString(),
                    new JArray(book.GetKnownSpells(level).Select(value => value.Blueprint.AssetGuid)))));
                bool adult = owner.Stats.HitPoints.ModifiedValue == 73 && owner.Stats.AC.ModifiedValue == 20 &&
                    owner.Stats.Strength.ModifiedValue == 20 && owner.Stats.Dexterity.ModifiedValue == 17 &&
                    owner.Stats.Constitution.ModifiedValue == 21 && owner.Stats.Charisma.ModifiedValue == 19 &&
                    owner.Body.PrimaryHand.MaybeWeapon.AttackRange.Value == 10 && tail.AttackRange.Value == 10 &&
                    tail.IsSecondary && tail.Blueprint.name == "KMG_Summoning_FeyCelestial_LillendAzata_Tail" &&
                    book.GetKnownSpells(0).Count() == 1 && book.GetKnownSpells(1).Count() == 3 &&
                    book.GetKnownSpells(2).Count() == (Sprint32Suggestion.Active ? 4 : 3) && book.GetKnownSpells(3).Count() == 2 &&
                    providerReady && Sprint32Suggestion.Settled && Sprint32Suggestion.Active == expectedSuggestion &&
                    book.IsKnown(creation.OptionalSuggestion) == Sprint32Suggestion.Active;
                Sprint25Check("lillend-adult-spell-mapping", adult, new JObject {
                    ["hp"] = owner.Stats.HitPoints.ModifiedValue, ["ac"] = owner.Stats.AC.ModifiedValue,
                    ["strength"] = owner.Stats.Strength.ModifiedValue, ["dexterity"] = owner.Stats.Dexterity.ModifiedValue,
                    ["constitution"] = owner.Stats.Constitution.ModifiedValue, ["charisma"] = owner.Stats.Charisma.ModifiedValue,
                    ["swordReachFeet"] = owner.Body.PrimaryHand.MaybeWeapon.AttackRange.Value,
                    ["nativeReachStatFeet"] = owner.Stats.Reach.ModifiedValue,
                    ["nativeReachContributionFeet"] = owner.Stats.ReachRange.Value,
                    ["swordBlueprintContributionFeet"] = owner.Body.PrimaryHand.MaybeWeapon.Blueprint.AttackRange.Value,
                    ["tailReachFeet"] = tail.AttackRange.Value, ["tailSecondary"] = tail.IsSecondary,
                    ["knownSpells"] = bookKnown, ["suggestionSettled"] = Sprint32Suggestion.Settled,
                    ["suggestionDisposition"] = Sprint32Suggestion.Disposition,
                    ["providerEntries"] = providers.Length, ["providerReady"] = providerReady,
                    ["providerVersion"] = providers.Length == 1 ? providers[0].Info.Version : null,
                    ["suggestionKnown"] = book.IsKnown(creation.OptionalSuggestion), ["finalQualification"] = false },
                    "Composed native adult HP/AC/abilities, Large ten-foot sword/tail reach and real known spell families; optional Suggestion matches settled provider state.");
                if (!adult) throw new InvalidOperationException("Lillend adult/spell composition mismatch.");
                // The expanded spellbook makes the old unguarded fallback foe
                // ineligible for an attack-only expectation. This owned target
                // receives actual native mind immunity; product rules stay intact.
                var immune = ScriptableObject.CreateInstance<BuffDescriptorImmunity>();
                immune.Descriptor = SpellDescriptor.MindAffecting;
                mindImmunity = ScriptableObject.CreateInstance<BlueprintFeature>();
                mindImmunity.name = "KMG_Runtime_Lillend32_MindImmunity"; mindImmunity.Ranks = 1;
                mindImmunity.ComponentsArray = new BlueprintComponent[] { immune };
                target.Descriptor.AddFact(mindImmunity);
                Vector3 far; string lane, direction;
                var origin = Sprint24AiCombatLane(fixture.Caster, owner, target, 1.4f, out far, out lane, out direction);
                PlaceExpandedSummoningUnit(owner, origin); PlaceExpandedSummoningUnit(sibling, origin + Vector3.right);
                PlaceExpandedSummoningUnit(target, far); target.Stats.AC.BaseValue = -100;
                int allyAttack = sibling.Stats.AdditionalAttackBonus.ModifiedValue;
                for (int frame = 0; frame < 8; frame++) yield return 0;
                foreach (var unit in new[] { owner, sibling, target })
                { unit.Wake(0f); if (!Game.Instance.State.AwakeUnits.Contains(unit)) Game.Instance.State.AwakeUnits.Add(unit); }
                owner.Memory.Add(target); target.Memory.Add(owner); owner.SightCache.ClearIfNeeded(); target.SightCache.ClearIfNeeded();
                new Kingmaker.Controllers.Units.UnitMemoryController().Tick();
                owner.JoinCombat(); sibling.JoinCombat(); target.JoinCombat(); Game.Instance.Player.UpdateIsInCombat();
                new UnitCombatJoinController().Tick(); new UnitCombatPrepareController().Tick();
                bool ready = owner.IsEnemy(target) && owner.HasLOS(target) && owner.Memory.ContainsVisible(target) &&
                    owner.HasLOS(sibling) && !owner.IsEnemy(sibling) && owner.Descriptor.State.CanAct &&
                    performAi.IsAvailable(new DecisionContext { Unit = owner });
                Sprint25Check("lillend-support-ready", ready, new JObject { ["lane"] = lane,
                    ["owner"] = owner.UniqueId, ["target"] = target.UniqueId, ["ally"] = sibling.UniqueId,
                    ["enemyVisible"] = owner.Memory.ContainsVisible(target), ["allyLos"] = owner.HasLOS(sibling),
                    ["distanceMeters"] = owner.DistanceTo(target), ["weaponRangeFeet"] = owner.Body.PrimaryHand.MaybeWeapon?.AttackRange.Value },
                    "Exact support actor, nearby ally and visible hostile admit the native self-performance decision.");
                if (!ready) throw new InvalidOperationException("Lillend support fixture preconditions failed.");
                foreach (var stage in new[] { "courage", "healing", "fallback" })
                {
                    Sprint28SetBrainActivePreservingActions(owner, false);
                    owner.Commands.InterruptAll(true); owner.Commands.RemoveFinishedAndUpdateQueue();
                    if (stage == "healing") owner.Damage = 18;
                    if (stage == "fallback") owner.Damage = 0; // new healthy-target fixture state, after retaining the real healing result
                    var controlScores = fixture.Blueprints.OfType<LillendSpellEligibility32>()
                        .Where(value => ReferenceEquals(value.OwningBlueprint, owner.Blueprint))
                        .ToDictionary(value => value.Choice.ToString(), value => value.Score(new DecisionContext {
                            Unit = owner, Target = new TargetInfo { Unit = target } }));
                    int slotsBefore = book.GetSpontaneousSlots(3), damageBefore = owner.Damage;
                    var commands = new HashSet<UnitCommand>(); var costs = new JArray(); var ambient = new HashSet<string>();
                    var attacks = new Sprint24AttackObserver { Owner = owner, Target = target };
                    var starts = new Lillend32CommandStarts { Owner = owner };
                    EventBus.Subscribe(attacks); EventBus.Subscribe(starts);
                    try
                    {
                        Sprint28SetBrainActivePreservingActions(owner, true); owner.Brain.RestoreAvailableActions();
                        using (var economy = new FirearmReviewCosts(owner, costs))
                        {
                            DateTime deadline = DateTime.UtcNow.AddSeconds(35); int frames = 0, ticks = 0;
                            UnitCommand resolved = null;
                            while (DateTime.UtcNow < deadline && frames++ < 4000)
                            {
                                if (frames == 1 || frames % 60 == 0 && !owner.Commands.Raw.Any(value => value != null && !value.IsFinished))
                                { foreach (var candidate in Sprint24TickExactAi(owner)) if (candidate != owner.UniqueId) ambient.Add(candidate); ticks++; }
                                foreach (var command in owner.Commands.Raw.Where(value => value != null)) commands.Add(command);
                                foreach (var command in starts.Commands) commands.Add(command);
                                resolved = commands.FirstOrDefault(value => value.IsFinished && value.Result == UnitCommand.ResultType.Success &&
                                    (stage == "courage" ? value is UnitActivateAbility && value.AiAction == performAi &&
                                        ReferenceEquals(((UnitActivateAbility)value).Ability, performance) :
                                     stage == "healing" ? value is UnitUseAbility && value.AiAction?.name == "KMG_Summoning_FeyCelestial_LillendAzata_HealAi" &&
                                         ((UnitUseAbility)value).ExecutionProcess != null && ((UnitUseAbility)value).ExecutionProcess.IsEnded :
                                     value is UnitAttack && value.AiAction != null));
                                if (resolved != null && (stage != "courage" ||
                                    owner.Descriptor.Resources.GetResourceAmount(resource) < 20 &&
                                    sibling.Stats.AdditionalAttackBonus.ModifiedValue == allyAttack + 2)) break;
                                Game.Instance.IsPaused = false; yield return 0;
                            }
                            var ownedEffects = sibling.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(value =>
                                value.Blueprint.name == "KMG_Summoning_FeyCelestial_LillendAzata_CourageRecipient" &&
                                ReferenceEquals(value.Context?.MaybeCaster, owner)).ToArray();
                            bool enemySafe = !target.Descriptor.Buffs.RawFacts.OfType<Buff>().Any(value =>
                                value.Blueprint.name == "KMG_Summoning_FeyCelestial_LillendAzata_CourageRecipient");
                            float healthyScore = heal.Score(new DecisionContext { Unit = owner, Target = new TargetInfo { Unit = owner } });
                            bool outcome = stage == "courage" ? performance.IsRunning &&
                                commands.OfType<UnitActivateAbility>().Count() == 1 &&
                                starts.Commands.OfType<UnitActivateAbility>().Count() == 1 &&
                                owner.Descriptor.Resources.GetResourceAmount(resource) == 19 &&
                                ownedEffects.Length == 1 && sibling.Stats.AdditionalAttackBonus.ModifiedValue == allyAttack + 2 && enemySafe &&
                                book.GetSpontaneousSlots(3) == slotsBefore :
                                stage == "healing" ? owner.Damage < damageBefore && book.GetSpontaneousSlots(3) == slotsBefore - 1 :
                                attacks.WeaponAttacks.Count > 0 && controlScores.Values.All(value => value == 0f) && healthyScore == 0f && !performAi.IsAvailable(new DecisionContext { Unit = owner }) &&
                                book.GetSpontaneousSlots(3) == slotsBefore;
                            bool passed = resolved != null && economy.Count(resolved) == 1 && economy.Exact(resolved, false) &&
                                resolved.Type == (stage == "courage" ? UnitCommand.CommandType.Move : UnitCommand.CommandType.Standard) &&
                                outcome && siblingBook.GetSpontaneousSlots(3) == 2 && sibling.Descriptor.Resources.GetResourceAmount(resource) == 20;
                            Sprint25Check("lillend-" + stage + "-resolved", passed, new JObject {
                                ["owner"] = owner.UniqueId, ["sibling"] = sibling.UniqueId, ["frames"] = frames, ["aiTicks"] = ticks,
                                ["commands"] = new JArray(commands.Select(Sprint27AiCommand)), ["costs"] = costs,
                                ["nativeCommandStarts"] = starts.Rows,
                                ["performanceRunning"] = performance.IsRunning, ["performanceRounds"] = owner.Descriptor.Resources.GetResourceAmount(resource),
                                ["allyAttackBefore"] = allyAttack, ["allyAttackAfter"] = sibling.Stats.AdditionalAttackBonus.ModifiedValue,
                                ["ownedAllyEffects"] = ownedEffects.Length, ["enemySafe"] = enemySafe,
                                ["damageBefore"] = damageBefore, ["damageAfter"] = owner.Damage,
                                ["level3SlotsBefore"] = slotsBefore, ["level3SlotsAfter"] = book.GetSpontaneousSlots(3),
                                ["siblingLevel3Slots"] = siblingBook.GetSpontaneousSlots(3), ["siblingPerformanceRounds"] = sibling.Descriptor.Resources.GetResourceAmount(resource),
                                ["controlEligibility"] = new JObject(controlScores.Select(pair => new JProperty(pair.Key, pair.Value))),
                                ["targetMindImmune"] = true, ["healthyHealingScore"] = healthyScore, ["weaponAttackEvents"] = attacks.WeaponAttacks.Count,
                                ["ambientCandidatesNeverTicked"] = new JArray(ambient), ["fixtureCastCommands"] = 0,
                                ["fixtureAttackCommands"] = 0, ["fixtureSlotDebits"] = 0, ["forcedRolls"] = 0 },
                                stage == "courage" ? "Native AI starts Inspire Courage with one move action, grants an ally+2 without buffing the enemy and spends one native performance round." :
                                stage == "healing" ? "Native AI heals meaningful damage with a real bard spell and standard action, spending one own level3 slot." :
                                "With performance running and no useful healing target, native AI attacks without repeating either support command; sibling pools remain untouched.");
                            if (!passed) throw new InvalidOperationException("Lillend early support observation failed: " + stage);
                        }
                    }
                    finally { EventBus.Unsubscribe(starts); EventBus.Unsubscribe(attacks); }
                }
            }
            finally
            {
                foreach (var unit in new[] { owner, sibling }.Where(value => value != null && !value.Destroyed))
                { Sprint28SetBrainActivePreservingActions(unit, false); unit.Commands.InterruptAll(true); }
                target.Commands.InterruptAll(true); target.Stats.AC.BaseValue = ac;
                if (mindImmunity != null)
                {
                    target.Descriptor.RemoveFact(mindImmunity);
                    foreach (var component in mindImmunity.ComponentsArray) UnityEngine.Object.DestroyImmediate(component);
                    UnityEngine.Object.DestroyImmediate(mindImmunity);
                }
                DisposeExpandedSummoningUnits(fixture.Created, new[] { owner, sibling }.Where(value => value != null && !value.Destroyed).ToArray());
                ResetExpandedSummoningHostile(fixture);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode;
                Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = oldPause;
            }
        }

        // Event-bound observation retains a command replaced within one frame;
        // frame snapshots alone missed the original in the preserved 2e1 run.
        private sealed class Lillend32CommandStarts : IUnitCommandStartHandler
        {
            internal UnitEntityData Owner;
            internal readonly HashSet<UnitCommand> Commands = new HashSet<UnitCommand>();
            internal readonly JArray Rows = new JArray();
            public void HandleUnitCommandDidStart(UnitCommand command)
            {
                if (command == null || !ReferenceEquals(command.Executor, Owner)) return;
                Commands.Add(command);
                var row = Sprint27AiCommand(command);
                row["commandReference"] = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(command);
                row["ability"] = (command as UnitActivateAbility)?.Ability?.Blueprint?.AssetGuid;
                Rows.Add(row);
            }
        }
    }
}
