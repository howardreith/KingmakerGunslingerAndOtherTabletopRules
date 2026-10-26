using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Additional assertions in the same owned, save-free Default fixture.
        // No Nereid execution helper, seeded roll, forced path or effect tick.
        private IEnumerable<int> ReviewLillendFascinateBoundaries32(ExpandedSummoningCorrectionFixture fixture,
            Cleanup2NativeDefaultScope mode, Vector3 origin, Vector3 center)
        {
            var game = Game.Instance; var player = game.Player;
            var extra = new List<UnitEntityData>(); var targets = new List<UnitEntityData> { fixture.Hostile };
            UnitEntityData owner = null, sibling = null;
            Dretch30CloudSaves saves = null; Lillend32CommandStarts starts = null, siblingStarts = null;
            bool complete = false;
            var away = (origin - center).normalized; var side = Vector3.Cross(Vector3.up, away).normalized;
            const string prefix = "KMG_Summoning_FeyCelestial_LillendAzata_";
            var effect = fixture.Blueprints.OfType<BlueprintBuff>().Single(value => value.name == prefix + "Fascinated");
            var resistance = fixture.Blueprints.OfType<BlueprintBuff>().Single(value => value.name == prefix + "FascinateResistance");
            var carrier = fixture.Blueprints.OfType<BlueprintBuff>().Single(value => value.name == prefix + "FascinateCarrier");
            var assistance = fixture.Blueprints.OfType<BlueprintBuff>().Single(value => value.name == prefix + "FascinateAssistance");
            var area = fixture.Blueprints.OfType<BlueprintAbilityAreaEffect>().Single(value => value.name == prefix + "FascinateArea");
            var shake = fixture.Blueprints.OfType<BlueprintAbility>().Single(value => value.name == prefix + "FascinateShakeFree");
            var resource = fixture.Blueprints.OfType<BlueprintAbilityResource>().Single(value => value.name == prefix + "PerformanceResource");
            Func<UnitEntityData, UnitEntityData, bool> affected = (actor, target) => target.Buffs.Enumerable.Any(value =>
                ReferenceEquals(value.Blueprint, effect) && ReferenceEquals(value.Context?.MaybeCaster, actor));
            Func<UnitEntityData, UnitEntityData, bool> marked = (actor, target) => target.Buffs.Enumerable.Any(value =>
                ReferenceEquals(value.Blueprint, resistance) && ReferenceEquals(value.Context?.MaybeCaster, actor));
            Func<UnitEntityData, UnitEntityData[]> selectable = actor => game.State.Units.All.Where(value =>
                LillendFascinateEligibility32.Selectable(actor, value, effect, resistance))
                .OrderBy(value => actor.DistanceTo(value)).ThenBy(value => value.UniqueId, StringComparer.Ordinal).ToArray();
            Func<UnitEntityData, ActivatableAbility> toggle = actor => actor.Descriptor.ActivatableAbilities.Enumerable
                .Single(value => ReferenceEquals(value.Blueprint.Buff, carrier));
            Func<UnitEntityData, int> saveCount = target => saves.Rows.Count(value => (string)value["target"] == target.UniqueId);
            try
            {
                PlaceExpandedSummoningUnit(fixture.Hostile, center);
                PlaceExpandedSummoningUnit(fixture.Caster, origin + away);
                var offsets = new[] { away * .9f, -side * .9f, away * .9f - side * .9f };
                foreach (var offset in offsets)
                {
                    var point = AstarPath.active.GetNearest(center + offset);
                    if (point.node == null || !point.node.Walkable || Vector3.Distance(point.clampedPosition, center + offset) > .3f)
                        throw new InvalidOperationException("Fascinate boundary target lacks a surveyed point.");
                    var target = game.EntityCreator.SpawnUnit(fixture.HostileBlueprint, point.clampedPosition, Quaternion.identity, fixture.Scene);
                    extra.Add(target); fixture.Created.Add(target); targets.Add(target);
                    target.Stats.HitPoints.BaseValue = 100000;
                }
                owner = CastExpandedSummoningOwnTier(fixture, "lillend-azata");
                sibling = CastExpandedSummoningOwnTier(fixture, "lillend-azata");
                foreach (var unit in targets.Concat(new[] { owner, sibling }))
                {
                    Sprint28SetBrainActivePreservingActions(unit, false);
                    unit.Commands.InterruptAll(true); unit.Commands.RemoveFinishedAndUpdateQueue();
                    unit.Wake(0f); if (!game.State.AwakeUnits.Contains(unit)) game.State.AwakeUnits.Add(unit);
                }
                PlaceExpandedSummoningUnit(owner, origin); PlaceExpandedSummoningUnit(sibling, origin + away * 2f);
                foreach (var target in targets) target.Stats.SaveWill.BaseValue = -100;
                var quietEnd = player.GameTime + TimeSpan.FromSeconds(7);
                foreach (int frame in WaitLillendFascinate32(mode, targets, () => player.GameTime >= quietEnd, 18)) yield return frame;
                var chosen = selectable(owner);
                var anchor = player.MainCharacter.Value;
                bool ready = chosen.Length == 4 && chosen.All(targets.Contains) && targets.All(target =>
                    target.DistanceTo(owner) > target.VisionRangeMeters && target.DistanceTo(sibling) > target.VisionRangeMeters &&
                    target.DistanceTo(anchor) > target.VisionRangeMeters && target.DistanceTo(fixture.Caster) > target.VisionRangeMeters &&
                    !target.Memory.Contains(owner) && !target.Memory.Contains(sibling) && !target.Memory.Contains(anchor) && !target.Memory.Contains(fixture.Caster)) &&
                    owner.Commands.Empty && sibling.Commands.Empty && owner.Descriptor.Resources.GetResourceAmount(resource) == 20 &&
                    sibling.Descriptor.Resources.GetResourceAmount(resource) == 20;
                Sprint25Check("fascinate-boundaries-ready", ready, new JObject {
                    ["eligible"] = new JArray(chosen.Select(value => value.UniqueId)),
                    ["distances"] = new JArray(chosen.Select(value => owner.DistanceTo(value))), ["freshSources"] = true },
                    "Four exact owned hostiles, fresh sources and naturally calm Default geometry; no memory/combat edits.");
                if (!ready) throw new InvalidOperationException("Fascinate boundary actors or calm geometry differ.");
                var resistant = chosen[0]; resistant.Stats.SaveWill.BaseValue = 100;
                var fourth = chosen[3]; var selected = chosen.Take(3).ToArray();
                var performance = toggle(owner); var siblingPerformance = toggle(sibling);
                var bookType = owner.Blueprint.GetComponent<LillendBardBook32>().Book;
                var book = owner.Descriptor.GetSpellbook(bookType); var siblingBook = sibling.Descriptor.GetSpellbook(bookType);
                var initialSlots = Enumerable.Range(0, 4).Select(book.GetSpontaneousSlots).ToArray();
                var siblingSlots = Enumerable.Range(0, 4).Select(siblingBook.GetSpontaneousSlots).ToArray();
                saves = new Dretch30CloudSaves(targets.ToArray()); starts = new Lillend32CommandStarts { Owner = owner };
                siblingStarts = new Lillend32CommandStarts { Owner = sibling };
                EventBus.Subscribe(saves); EventBus.Subscribe(starts); EventBus.Subscribe(siblingStarts);
                var costs = new JArray();
                using (var economy = new FirearmReviewCosts(owner, costs))
                using (var rounds = new Lillend32PerformanceRounds(performance, resource, false))
                {
                    Sprint28SetBrainActivePreservingActions(owner, true);
                    foreach (int frame in WaitLillendFascinate32(mode, targets, () => rounds.Rows.Count >= 2, 24)) yield return frame;
                    Sprint28SetBrainActivePreservingActions(owner, false);
                    var commands = starts.Commands.OfType<UnitActivateAbility>().Where(value => ReferenceEquals(value.Ability, performance)).ToArray();
                    var marker = resistant.Buffs.Enumerable.SingleOrDefault(value => ReferenceEquals(value.Blueprint, resistance) &&
                        ReferenceEquals(value.Context?.MaybeCaster, owner));
                    bool cap = commands.Length == 1 && NormalFascinateCommand32(commands[0], UnitCommand.CommandType.Move, economy) &&
                        rounds.Rows.Count >= 2 && rounds.Exact() && saves.Rows.Count == 3 && selected.All(target => saveCount(target) == 1) &&
                        saveCount(fourth) == 0 && !affected(owner, fourth) && selected.Count(target => affected(owner, target)) == 2 &&
                        marker != null && marker.TimeLeft.TotalSeconds > 86380 && marker.TimeLeft.TotalSeconds <= 86400 &&
                        marker.IsNotDispelable && !marker.IsFromSpell && marked(owner, resistant) && !marked(sibling, resistant) &&
                        !LillendFascinateEligibility32.Selectable(owner, resistant, effect, resistance) &&
                        LillendFascinateEligibility32.Selectable(sibling, resistant, effect, resistance) &&
                        Enumerable.Range(0,4).Select(book.GetSpontaneousSlots).SequenceEqual(initialSlots);
                    Sprint25Check("fascinate-cap-and-resistance", cap, new JObject {
                        ["selected"] = new JArray(selected.Select(value => value.UniqueId)), ["fourth"] = fourth.UniqueId,
                        ["willSetup"] = new JArray(chosen.Select(value => value.Stats.SaveWill.BaseValue)), ["saves"] = saves.Rows.DeepClone(),
                        ["costs"] = costs, ["rounds"] = rounds.Rows, ["markerSeconds"] = marker?.TimeLeft.TotalSeconds,
                        ["markerEnd"] = marker?.EndTime.TotalSeconds, ["gameSeconds"] = player.GameTime.TotalSeconds },
                        "Three natural saves and no fourth exposure; real successful-save source marker has a24h deadline. Not24h expiry proof. Unseeded outcomes retained.");
                    if (!cap) throw new InvalidOperationException("Cap/resistance proof incomplete; inspect actual unforced saves before classification.");
                }
                PlaceExpandedSummoningUnit(fourth, center + away * .6f - side * 1.5f);
                int beforeFrozen = saves.Rows.Count;
                var frozenEnd = player.GameTime + TimeSpan.FromSeconds(7);
                foreach (int frame in WaitLillendFascinate32(mode, targets, () => player.GameTime >= frozenEnd, 18)) yield return frame;
                bool frozen = owner.DistanceTo(fourth) < selected.Max(value => owner.DistanceTo(value)) &&
                    saves.Rows.Count == beforeFrozen && !affected(owner, fourth) && saveCount(fourth) == 0;
                Sprint25Check("fascinate-frozen-selection", frozen, new JObject {
                    ["fourthDistance"] = owner.DistanceTo(fourth), ["selectedFarthest"] = selected.Max(value => owner.DistanceTo(value)),
                    ["savesBefore"] = beforeFrozen, ["savesAfter"] = saves.Rows.Count, ["fourthAffected"] = affected(owner, fourth) },
                    "Repositioning the owned unselected fourth closer does not add a save or replace a selected listener; no movement-animation claim.");
                if (!frozen) throw new InvalidOperationException("Fascinate selection was not frozen.");
                var shaken = selected.Where(value => affected(owner, value)).OrderBy(value => fourth.DistanceTo(value)).First();
                var preserved = selected.Single(value => !ReferenceEquals(value, shaken) && affected(owner, value));
                var shakeFact = fourth.Descriptor.Abilities.GetAbility(shake);
                var shakeData = shakeFact == null ? null : new AbilityData(shakeFact);
                if (shakeData == null || !shakeData.CanTarget(new TargetWrapper(shaken)) ||
                    fourth.DistanceTo(shaken) > shakeData.GetApproachDistance(shaken))
                    throw new InvalidOperationException("Source-granted Shake Free is unavailable at actual touch reach.");
                var shakeCommand = new UnitUseAbility(shakeData, new TargetWrapper(shaken)); var shakeCosts = new JArray();
                using (var economy = new FirearmReviewCosts(fourth, shakeCosts))
                {
                    fourth.Commands.Run(shakeCommand);
                    foreach (int frame in WaitLillendFascinate32(mode, targets, () => shakeCommand.IsFinished, 16)) yield return frame;
                    var noRepeatEnd = player.GameTime + TimeSpan.FromSeconds(7);
                    foreach (int frame in WaitLillendFascinate32(mode, targets, () => player.GameTime >= noRepeatEnd, 18)) yield return frame;
                    bool freed = NormalFascinateCommand32(shakeCommand, UnitCommand.CommandType.Standard, economy) &&
                        !affected(owner, shaken) && affected(owner, preserved) && saveCount(shaken) == 1 && marked(owner, resistant);
                    Sprint25Check("fascinate-shake-free", freed, new JObject {
                        ["command"] = Sprint27AiCommand(shakeCommand), ["costs"] = shakeCosts,
                        ["shaken"] = shaken.UniqueId, ["preserved"] = preserved.UniqueId,
                        ["shakenAffected"] = affected(owner, shaken), ["otherAffected"] = affected(owner, preserved),
                        ["shakenSaves"] = saveCount(shaken) },
                        "One ordinary source-granted touch/Standard Shake Free removes only its target, with no reapplication in that activation.");
                    if (!freed) throw new InvalidOperationException("Native Shake Free isolation failed.");
                }
                performance.IsOn = false;
                foreach (int frame in WaitLillendFascinate32(mode, targets, () => !owner.Buffs.Enumerable.Any(value => ReferenceEquals(value.Blueprint, carrier)), 16)) yield return frame;
                // A second ordinary performance supplies fresh affected subjects
                // for movement and obvious-threat observations; no state rewind.
                PlaceExpandedSummoningUnit(fixture.Caster, origin + away * 2f + side * 2f);
                int secondStart = saves.Rows.Count, secondPool = owner.Descriptor.Resources.GetResourceAmount(resource);
                var earlierCommands = new HashSet<UnitCommand>(starts.Commands);
                var secondCosts = new JArray();
                using (var economy = new FirearmReviewCosts(owner, secondCosts))
                {
                    Sprint28SetBrainActivePreservingActions(owner, true);
                    foreach (int frame in WaitLillendFascinate32(mode, targets, () => saves.Rows.Count >= secondStart + 3 &&
                        starts.Commands.OfType<UnitActivateAbility>().Any(value => !earlierCommands.Contains(value) &&
                            ReferenceEquals(value.Ability, performance) && value.IsFinished), 24)) yield return frame;
                    Sprint28SetBrainActivePreservingActions(owner, false);
                    var command = starts.Commands.OfType<UnitActivateAbility>().Single(value => !earlierCommands.Contains(value) &&
                        ReferenceEquals(value.Ability, performance));
                    bool restarted = NormalFascinateCommand32(command, UnitCommand.CommandType.Move, economy) &&
                        saves.Rows.Count == secondStart + 3 && !affected(owner, resistant) && saveCount(resistant) == 1 &&
                        targets.Any(value => affected(owner, value)) && owner.Descriptor.Resources.GetResourceAmount(resource) < secondPool;
                    Sprint25Check("fascinate-resistance-restart", restarted, new JObject {
                        ["saves"] = new JArray(saves.Rows.Skip(secondStart)), ["costs"] = secondCosts,
                        ["resistantSaves"] = saveCount(resistant), ["pool"] = secondPool + "->" + owner.Descriptor.Resources.GetResourceAmount(resource) },
                        "A fresh normal activation respects the earlier24h source refusal and spends its own remaining pool; no replenishment.");
                    if (!restarted) throw new InvalidOperationException("Source resistance or natural restart failed.");
                }
                var movingSubjects = targets.Where(value => affected(owner, value)).ToArray();
                var beforeMove = movingSubjects.ToDictionary(value => value, saveCount);
                var moveStart = fixture.Caster.Position; var move = new UnitMoveTo(moveStart - away);
                var moveCosts = new JArray();
                using (var economy = new FirearmReviewCosts(fixture.Caster, moveCosts))
                {
                    fixture.Caster.Commands.Run(move);
                    foreach (int frame in WaitLillendFascinate32(mode, targets, () => move.IsFinished, 18)) yield return frame;
                    var settled = player.GameTime + TimeSpan.FromSeconds(7);
                    foreach (int frame in WaitLillendFascinate32(mode, targets, () => player.GameTime >= settled, 18)) yield return frame;
                    // Native UnitMoveTo deliberately sets IgnoreCooldown in
                    // its constructor. Movement is the potential-threat cue;
                    // this case must not invent an exploration action debit.
                    bool approached = move.IsFinished && move.Result == UnitCommand.ResultType.Success &&
                        !move.Cutscene && move.IsIgnoreCooldown &&
                        Vector3.Distance(moveStart, fixture.Caster.Position) > .1f && movingSubjects.All(value =>
                            fixture.Caster.DistanceTo(value) < Vector3.Distance(moveStart, value.Position) && saveCount(value) == beforeMove[value] + 1) &&
                        targets.All(value => fixture.Caster.DistanceTo(value) > value.VisionRangeMeters) && targets.Any(value => affected(owner, value));
                    Sprint25Check("fascinate-native-approach", approached, new JObject {
                        ["command"] = Sprint27AiCommand(move), ["start"] = moveStart.ToString(), ["end"] = fixture.Caster.Position.ToString(),
                        ["displacement"] = Vector3.Distance(moveStart, fixture.Caster.Position), ["costs"] = moveCosts,
                        ["nativeIgnoreCooldown"] = move.IsIgnoreCooldown, ["costObservations"] = economy.Count(move),
                        ["subjects"] = new JArray(movingSubjects.Select(value => new JObject { ["unit"] = value.UniqueId,
                            ["before"] = beforeMove[value], ["after"] = saveCount(value), ["affected"] = affected(owner, value) })),
                        ["saves"] = saves.Rows.DeepClone(), ["forcedPaths"] = 0 },
                        "Actual native exploration movement toward listeners, outside acquisition, causes exactly one further save per subject for that command; native ignored cooldown is recorded, not called a spent action.");
                    if (!approached) throw new InvalidOperationException("Natural approach/save proof incomplete; inspect path, command and real outcomes.");
                }
                var cure = book.GetKnownSpells(1).Single(value => value.Blueprint.AssetGuid == "5590652e1c2225c4ca30c4a699ab3649");
                int slotsBefore = book.GetSpontaneousSlots(1); owner.Damage = 5;
                if (!cure.IsAvailable || !cure.CanTarget(new TargetWrapper(owner))) throw new InvalidOperationException("Ordinary self cure unavailable for threat case.");
                var spell = new UnitUseAbility(cure, new TargetWrapper(owner)); var spellCosts = new JArray();
                using (var economy = new FirearmReviewCosts(owner, spellCosts))
                {
                    owner.Commands.Run(spell);
                    foreach (int frame in WaitLillendFascinate32(mode, targets, () => spell.IsFinished && owner.Damage < 5, 18)) yield return frame;
                    bool obvious = NormalFascinateCommand32(spell, UnitCommand.CommandType.Standard, economy) &&
                        !targets.Any(value => affected(owner, value)) && book.GetSpontaneousSlots(1) == slotsBefore - 1 &&
                        Enumerable.Range(0,4).Select(siblingBook.GetSpontaneousSlots).SequenceEqual(siblingSlots);
                    Sprint25Check("fascinate-visible-spell-threat", obvious, new JObject {
                        ["command"] = Sprint27AiCommand(spell), ["costs"] = spellCosts,
                        ["level1Slots"] = slotsBefore + "->" + book.GetSpontaneousSlots(1), ["damageAfter"] = owner.Damage,
                        ["remainingEffects"] = targets.Count(value => affected(owner, value)) },
                        "A visible ordinary self-targeted bard spell is an obvious threat: normal action and real slot debit, with no offensive memory workaround.");
                    if (!obvious) throw new InvalidOperationException("Visible spell threat did not end fascination.");
                }
                performance.IsOn = false;
                foreach (int frame in WaitLillendFascinate32(mode, targets, () => !owner.Buffs.Enumerable.Any(value => ReferenceEquals(value.Blueprint, carrier)), 16)) yield return frame;
                PlaceExpandedSummoningUnit(sibling, origin);
                var siblingChoice = selectable(sibling);
                if (siblingChoice.Length != 4 || !siblingChoice.Take(3).Contains(resistant) || !marked(owner, resistant) || marked(sibling, resistant))
                    throw new InvalidOperationException("Independent sibling/source-marker preconditions differ.");
                int originalPool = owner.Descriptor.Resources.GetResourceAmount(resource);
                var siblingCosts = new JArray();
                using (var economy = new FirearmReviewCosts(sibling, siblingCosts))
                {
                    Sprint28SetBrainActivePreservingActions(sibling, true);
                    foreach (int frame in WaitLillendFascinate32(mode, targets, () => saves.Rows.Any(value =>
                        (string)value["caster"] == sibling.UniqueId && (string)value["target"] == resistant.UniqueId) &&
                        siblingStarts.Commands.OfType<UnitActivateAbility>().Any(value => value.IsFinished), 24)) yield return frame;
                    Sprint28SetBrainActivePreservingActions(sibling, false);
                    var command = siblingStarts.Commands.OfType<UnitActivateAbility>().Single(value => ReferenceEquals(value.Ability, siblingPerformance));
                    var siblingSaves = saves.Rows.Where(value => (string)value["caster"] == sibling.UniqueId).ToArray();
                    bool independent = NormalFascinateCommand32(command, UnitCommand.CommandType.Move, economy) &&
                        siblingSaves.Count(value => (string)value["target"] == resistant.UniqueId) == 1 && marked(owner, resistant) &&
                        owner.Descriptor.Resources.GetResourceAmount(resource) == originalPool &&
                        sibling.Descriptor.Resources.GetResourceAmount(resource) < 20 && targets.Any(value => affected(sibling, value));
                    Sprint25Check("fascinate-sibling-source-isolation", independent, new JObject {
                        ["saves"] = new JArray(siblingSaves), ["costs"] = siblingCosts,
                        ["originalPool"] = originalPool, ["siblingPool"] = sibling.Descriptor.Resources.GetResourceAmount(resource) },
                        "The sibling makes its own natural save against the original performer's resistant target and spends only its own performance pool.");
                    if (!independent) throw new InvalidOperationException("Sibling source/response isolation failed.");
                }
                // Capture exact instances before disposing their caster: a null
                // MaybeCaster after disposal must not hide an orphaned fact.
                var originalMarkers = game.State.Units.All.SelectMany(value => value.Buffs.Enumerable).Where(value =>
                    ReferenceEquals(value.Blueprint, resistance) && ReferenceEquals(value.Context?.MaybeCaster, owner)).ToArray();
                var siblingFacts = game.State.Units.All.SelectMany(value => value.Buffs.Enumerable).Where(value =>
                    (ReferenceEquals(value.Blueprint, resistance) || ReferenceEquals(value.Blueprint, effect)) &&
                    ReferenceEquals(value.Context?.MaybeCaster, sibling)).ToArray();
                DisposeExpandedSummoningUnits(fixture.Created, new[] { owner });
                bool sourceClean = originalMarkers.Length > 0 && originalMarkers.All(value => !value.Owner.Buffs.Enumerable.Contains(value)) &&
                    siblingFacts.Length > 0 && siblingFacts.All(value => value.Owner.Buffs.Enumerable.Contains(value));
                Sprint25Check("fascinate-source-removal", sourceClean, new JObject {
                    ["originalMarkers"] = originalMarkers.Length, ["siblingFacts"] = siblingFacts.Length },
                    "Removing the original source removes its exact captured marker instances while preserving the sibling's exact effects and markers.");
                if (!sourceClean) throw new InvalidOperationException("Source disposal removed foreign state or left its own marker.");
                var helpers = game.State.Units.All.Where(value => value.Descriptor.Abilities.GetAbility(shake) != null).ToArray();
                DisposeExpandedSummoningUnits(fixture.Created, new[] { sibling });
                Func<bool> clean = () => !game.State.Units.All.Any(unit => unit.Buffs.Enumerable.Any(value =>
                    ReferenceEquals(value.Blueprint, effect) || ReferenceEquals(value.Blueprint, carrier) ||
                    ReferenceEquals(value.Blueprint, assistance) || ReferenceEquals(value.Blueprint, resistance))) &&
                    !game.State.AreaEffects.All.Any(value => ReferenceEquals(value.Blueprint, area) && !value.IsEnded) &&
                    helpers.All(value => value.Destroyed || value.Descriptor.Abilities.GetAbility(shake) == null) &&
                    starts.Commands.All(value => value.IsFinished) && siblingStarts.Commands.All(value => value.IsFinished);
                foreach (int frame in WaitLillendFascinate32(mode, targets, clean, 12)) yield return frame;
                Sprint25Check("fascinate-boundaries-cleanup", clean(), new JObject {
                    ["helpers"] = helpers.Length, ["ownerDestroyed"] = owner.Destroyed, ["siblingDestroyed"] = sibling.Destroyed },
                    "Both sources leave no live private area, effect, marker, granted Shake Free or unfinished observed command.");
                if (!clean()) throw new InvalidOperationException("Fascinate boundary state remains after source disposal.");
                complete = true;
            }
            finally
            {
                if (saves != null) EventBus.Unsubscribe(saves);
                if (starts != null) EventBus.Unsubscribe(starts);
                if (siblingStarts != null) EventBus.Unsubscribe(siblingStarts);
                var cleanup = new List<string>();
                foreach (var unit in extra.Concat(new[] { owner, sibling }).Where(value => value != null && !value.Destroyed).ToArray())
                {
                    try { DisposeExpandedSummoningUnits(fixture.Created, new[] { unit }); }
                    catch (Exception error) { cleanup.Add(unit.UniqueId + ":" + error.Message); }
                }
                bool cleaned = cleanup.Count == 0 && extra.All(value => value.Destroyed) &&
                    (owner == null || owner.Destroyed) && (sibling == null || sibling.Destroyed);
                Sprint25Check("fascinate-boundaries-finalizers", cleaned,
                    new JObject { ["extraTargets"] = extra.Count, ["complete"] = complete, ["errors"] = new JArray(cleanup) },
                    "Every additional actor is explicitly owned and disposed; the outer original-target/party/Default restoration remains responsible for its intake.");
                if (complete && !cleaned) throw new InvalidOperationException("Fascinate boundary finalizers failed: " + string.Join("|", cleanup.ToArray()));
            }
        }

        private IEnumerable<int> WaitLillendFascinate32(Cleanup2NativeDefaultScope mode,
            IEnumerable<UnitEntityData> targets, Func<bool> finished, int seconds)
        {
            var deadline = DateTime.UtcNow.AddSeconds(seconds);
            while (true)
            {
                if (!mode.Current || Game.Instance.Player.IsInCombat || targets.Any(value => value.IsInCombat))
                    throw new InvalidOperationException("Fascinate boundary lost its exact calm native Default context.");
                if (finished()) yield break;
                if (DateTime.UtcNow >= deadline) throw new InvalidOperationException("Fascinate boundary observation timed out.");
                yield return 0;
            }
        }
        private static bool NormalFascinateCommand32(UnitCommand command, UnitCommand.CommandType type, FirearmReviewCosts economy)
        {
            return command != null && command.IsActed && command.IsFinished && command.Result == UnitCommand.ResultType.Success &&
                command.Type == type && !command.Cutscene && !command.IsIgnoreCooldown &&
                economy.Count(command) == 1 && economy.Unchanged(command);
        }
    }
}
