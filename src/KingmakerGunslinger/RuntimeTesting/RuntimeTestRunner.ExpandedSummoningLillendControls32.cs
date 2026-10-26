using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Controllers.Brain;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums.Damage;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs;
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
        // Two finite cases over the existing isolated scene, native AI/commands,
        // real spellbook and exact-owned clock. Native animal targets exclude
        // humanoid control; five actual HD exclude Sleep/Daze, without changing
        // any production creature or supplying a cast, save or damage result.
        private IEnumerable<int> ReviewLillendControls32(ExpandedSummoningCorrectionFixture fixture, bool burst, bool turnBased)
        {
            var game = Game.Instance;
            var actors = new List<UnitEntityData>();
            UnitEntityData owner = null, sibling = null, target = null, second = null;
            Dretch30CloudSaves saves = null; Lillend32CommandStarts starts = null;
            Sprint16RuleObserver damage = null, otherDamage = null;
            Buff charmEffect = null;
            string id = "lillend-" + (burst ? "burst" : "charm-monster") + (turnBased ? "-turn" : "-rtwp");
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue, oldPause = game.IsPaused;
            if (_lillendHoldOwnerTicks32 != null) throw new InvalidOperationException("Another exact control clock is active.");
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                game.TurnBasedCombatController.Activate(); game.IsPaused = false;
                owner = CastExpandedSummoningOwnTier(fixture, "lillend-azata"); actors.Add(owner);
                sibling = CastExpandedSummoningOwnTier(fixture, "lillend-azata"); actors.Add(sibling);
                foreach (var actor in actors) Sprint28SetBrainActivePreservingActions(actor, false);
                var wolf = BlueprintLibraryLookup.RequireExact<BlueprintUnit>(BlueprintBootstrap.Library,
                    "76597216769b0d540aafafa07edf0cec", "native Wolf nonhumanoid control recipient");
                var fighter = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                    "48ac8db94d5de7645906c7d0ad3bcfbd", "native five-HD fixture advancement");
                Func<UnitEntityData> spawn = () => {
                    var actor = game.EntityCreator.SpawnUnit(wolf, fixture.Caster.Position + Vector3.forward * 40f,
                        Quaternion.identity, fixture.Scene);
                    fixture.Created.Add(actor); actors.Add(actor); game.EntityCreator.Tick();
                    Sprint28SetBrainActivePreservingActions(actor, false);
                    actor.Descriptor.SwitchFactions(fixture.OwnedHostileFaction, true);
                    actor.AttackFactions.Match(new[] { fixture.Caster.Faction });
                    int advance = 5 - actor.Descriptor.Progression.CharacterLevel;
                    if (advance < 0) throw new InvalidOperationException("Native Wolf exceeds the declared fixture HD.");
                    ElementalSpellAffinityScenario.Advance(actor.Descriptor, fighter, advance);
                    actor.Stats.HitPoints.BaseValue = 100000; actor.Stats.SaveWill.BaseValue = -100;
                    return actor;
                };
                target = spawn(); if (burst) second = spawn();
                var choice = burst ? LillendSpellChoice32.SoundBurst : LillendSpellChoice32.CharmMonster;
                var policies = fixture.Blueprints.OfType<LillendSpellEligibility32>().ToArray();
                var policy = policies.Single(value => value.Choice == choice);
                var burstPolicy = policies.Single(value => value.Choice == LillendSpellChoice32.SoundBurst);
                var book = owner.Descriptor.GetSpellbook(policy.Book);
                var siblingBook = sibling.Descriptor.GetSpellbook(policy.Book);
                var ability = policy.Spell; var data = new AbilityData(ability, book); var parameters = data.CalculateParams();
                var ai = owner.Blueprint.Brain.Actions.OfType<BlueprintAiCastSpell>().Single(value => ReferenceEquals(value.Ability, ability));
                int level = burst ? 2 : 3, initialSlots = book.GetSpontaneousSlots(level);
                Func<JObject> siblingBudget = () => new JObject { ["slots"] = new JArray(Enumerable.Range(1, 3).Select(siblingBook.GetSpontaneousSlots)),
                    ["resources"] = Sprint32SavedBudgetForControl(sibling, fixture) };
                var siblingBefore = siblingBudget();
                Vector3 far; string lane, direction;
                var origin = Sprint24AiCombatLane(fixture.Caster, owner, target, 8f, out far, out lane, out direction,
                    originAllyExclusionRadius: new Feet(30).Meters + 1f);
                PlaceExpandedSummoningUnit(owner, origin); PlaceExpandedSummoningUnit(target, far);
                var side = Vector3.Cross(Vector3.up, (far - origin).normalized).normalized;
                var near = AstarPath.active.GetNearest(far + side * 1.2f);
                if (near.node == null || !near.node.Walkable || Vector3.Distance(near.clampedPosition, far) > 2f)
                    throw new InvalidOperationException("Control case has no separate nearby walkable point.");
                PlaceExpandedSummoningUnit(sibling, near.clampedPosition);
                foreach (var actor in actors)
                { actor.Wake(0f); if (!game.State.AwakeUnits.Contains(actor)) game.State.AwakeUnits.Add(actor); }
                owner.Memory.Add(target); target.Memory.Add(owner);
                var memory = new Kingmaker.Controllers.Units.UnitMemoryController();
                var limit = DateTime.UtcNow.AddSeconds(8); int frames = 0;
                do { game.IsPaused = false; yield return 0; frames++; memory.Tick(); }
                while (DateTime.UtcNow < limit && (frames < 8 || !owner.HasLOS(target) || !owner.Memory.ContainsVisible(target)));
                Func<LillendSpellEligibility32, UnitEntityData, float> score = (value, actor) => value.Score(new DecisionContext {
                    Unit = owner, Target = new TargetInfo { Unit = actor } });
                bool unsafeRefused = burstPolicy.Score(new DecisionContext { Unit = owner, Target = new TargetInfo { Unit = target } }) == 0;
                if (burst)
                {
                    PlaceExpandedSummoningUnit(sibling, origin);
                    PlaceExpandedSummoningUnit(second, near.clampedPosition);
                    target.Stats.SaveFortitude.BaseValue = -100; second.Stats.SaveFortitude.BaseValue = 100;
                    owner.Memory.Add(second); second.Memory.Add(owner);
                    limit = DateTime.UtcNow.AddSeconds(8); frames = 0;
                    do { game.IsPaused = false; yield return 0; frames++; memory.Tick(); }
                    while (DateTime.UtcNow < limit && (frames < 8 || !owner.HasLOS(second) || !owner.Memory.ContainsVisible(second)));
                }
                owner.JoinCombat(); target.JoinCombat(); if (second != null) second.JoinCombat();
                game.Player.UpdateIsInCombat(); new UnitCombatJoinController().Tick(); new UnitCombatPrepareController().Tick();
                var admission = new JObject();
                if (turnBased)
                {
                    foreach (int frame in Sprint22EnrollTurnBasedOwners(new[] { owner }, target, admission)) yield return frame;
                    limit = DateTime.UtcNow.AddSeconds(30);
                    while (DateTime.UtcNow < limit && !Sprint18OwnerIsActing(owner)) yield return 0;
                    if (!(bool)admission["admitted"] || !Sprint18OwnerIsActing(owner)) throw new InvalidOperationException("Control turn admission failed.");
                }
                bool ready = unsafeRefused && target.Blueprint == wolf && target.Descriptor.Progression.CharacterLevel == 5 &&
                    owner.IsEnemy(target) && owner.HasLOS(target) && owner.Memory.ContainsVisible(target) &&
                    score(policy, target) == 1 && data.IsAvailable && data.CanTarget(new TargetWrapper(target)) &&
                    parameters.CasterLevel == 7 && parameters.DC == (burst ? 16 : 17) &&
                    data.RuntimeActionType == UnitCommand.CommandType.Standard && !data.RequireFullRoundAction &&
                    new[] { LillendSpellChoice32.Daze, LillendSpellChoice32.Sleep, LillendSpellChoice32.HoldPerson,
                        LillendSpellChoice32.CharmPerson, LillendSpellChoice32.CharmPersonSla }.All(value =>
                            score(policies.Single(item => item.Choice == value), target) == 0) &&
                    (!burst || second.Descriptor.Progression.CharacterLevel == 5 && owner.HasLOS(second) && owner.Memory.ContainsVisible(second));
                Sprint25Check(id + "-ready", ready, new JObject { ["lane"] = lane, ["admission"] = admission,
                    ["targetBlueprint"] = target.Blueprint.AssetGuid, ["targetHd"] = target.Descriptor.Progression.CharacterLevel,
                    ["targetFacts"] = new JArray(target.Descriptor.Progression.Features.Enumerable.Select(value => value.Blueprint.AssetGuid)),
                    ["cl"] = parameters.CasterLevel, ["dc"] = parameters.DC, ["slots"] = initialSlots,
                    ["unsafeFootprintRefused"] = unsafeRefused, ["selectedScore"] = score(policy, target),
                    ["siblingBudget"] = siblingBefore, ["second"] = second?.UniqueId },
                    "Actual native five-HD nonhumanoid recipients exclude competing humanoid/HD controls; allied footprint refusal precedes safe natural casting, without changing production choices.");
                if (!ready) throw new InvalidOperationException("Control case native preconditions failed.");
                if (!turnBased) _lillendHoldOwnerTicks32 = new LillendDazeRoundTicks32(owner, ai);
                starts = new Lillend32CommandStarts { Owner = owner };
                saves = new Dretch30CloudSaves(second == null ? new[] { target } : new[] { target, second });
                damage = new Sprint16RuleObserver { Owner = owner, Target = target };
                otherDamage = new Sprint16RuleObserver { Owner = owner, Target = second };
                EventBus.Subscribe(starts); EventBus.Subscribe(saves); EventBus.Subscribe(damage); EventBus.Subscribe(otherDamage);
                var commands = new HashSet<UnitCommand>(); var costs = new JArray(); int ticks = 0, turnFrames = 0;
                var charmBuff = fixture.Blueprints.OfType<Kingmaker.UnitLogic.Buffs.Blueprints.BlueprintBuff>()
                    .Single(value => value.name == "KMG_Summoning_FeyCelestial_CharmBuff");
                using (var economy = new FirearmReviewCosts(owner, costs))
                {
                    Sprint28SetBrainActivePreservingActions(owner, true); owner.Brain.RestoreAvailableActions();
                    limit = DateTime.UtcNow.AddSeconds(50); frames = 0;
                    while (DateTime.UtcNow < limit)
                    {
                        frames++;
                        foreach (var command in owner.Commands.Raw.Where(value => value != null).Concat(starts.Commands)) commands.Add(command);
                        charmEffect = target.Buffs.Enumerable.SingleOrDefault(value => ReferenceEquals(value.Blueprint, charmBuff) && ReferenceEquals(value.Context?.MaybeCaster, owner));
                        var completed = commands.OfType<UnitUseAbility>().Where(value => ReferenceEquals(value.Spell?.Blueprint, ability) &&
                            value.IsFinished && value.ExecutionProcess?.IsEnded == true).ToArray();
                        bool observed = burst ? completed.Any(cast => saves.NativeSaves.Any(value => ReferenceEquals(value.Initiator, target) &&
                            ReferenceEquals(value.Reason?.Context?.SourceAbilityContext, cast.ExecutionProcess.Context) && value.DifficultyClass == 16 && !value.IsPassed) &&
                            saves.NativeSaves.Any(value => ReferenceEquals(value.Initiator, second) &&
                            ReferenceEquals(value.Reason?.Context?.SourceAbilityContext, cast.ExecutionProcess.Context) && value.DifficultyClass == 16 && value.IsPassed)) : charmEffect != null;
                        if (completed.Length > 0 && observed || completed.Length >= Math.Min(3, initialSlots)) break;
                        if (!turnBased && (frames == 1 || frames % 60 == 0)) { Sprint24TickExactAi(owner); ticks++; }
                        if (turnBased && Sprint18OwnerIsActing(owner)) turnFrames++;
                        game.IsPaused = false; yield return 0;
                    }
                    Sprint28SetBrainActivePreservingActions(owner, false);
                    var casts = commands.OfType<UnitUseAbility>().Where(value => ReferenceEquals(value.Spell?.Blueprint, ability) &&
                        value.IsFinished && value.ExecutionProcess?.IsEnded == true).ToArray();
                    bool castPass = casts.Length > 0 && casts.All(value => value.Result == UnitCommand.ResultType.Success &&
                        value.AiAction == ai && value.Spell.Spellbook == book && value.Type == UnitCommand.CommandType.Standard &&
                        !value.Spell.RequireFullRoundAction && !value.Cutscene && !value.IsIgnoreCooldown && economy.Count(value) == 1 && economy.Exact(value, turnBased)) &&
                        book.GetSpontaneousSlots(level) == initialSlots - casts.Length && JToken.DeepEquals(siblingBefore, siblingBudget()) &&
                        (turnBased ? ticks == 0 && turnFrames > 0 : ticks > 0);
                    if (burst)
                    {
                        var hits = damage.Damage.Concat(otherDamage.Damage).Where(value => ReferenceEquals(value.Initiator, owner) &&
                            ReferenceEquals(value.SourceAbility, ability) && ReferenceEquals(value.Reason?.Ability?.Blueprint, ability)).ToArray();
                        bool damagePass = hits.Any(value => ReferenceEquals(value.Target, target)) && hits.Any(value => ReferenceEquals(value.Target, second)) &&
                            hits.All(value => value.Damage >= 1 && value.Damage <= 8 && value.DamageBundle.Count() == 1 &&
                                value.DamageBundle.Single() is EnergyDamage && ((EnergyDamage)value.DamageBundle.Single()).EnergyType == DamageEnergyType.Sonic &&
                                value.DamageBundle.Single().Dice.Rolls == 1 && value.DamageBundle.Single().Dice.Dice == DiceType.D8);
                        var correlated = new JArray(); bool sawBothOutcomes = false, stunPass = casts.Length > 0;
                        for (int index = 0; index < casts.Length; index++)
                        {
                            var context = casts[index].ExecutionProcess.Context;
                            var recipients = new JArray(); bool lowFailed = false, highPassed = false;
                            foreach (var recipient in new[] { target, second })
                            {
                                var rolls = saves.NativeSaves.Where(value => ReferenceEquals(value.Initiator, recipient) &&
                                    ReferenceEquals(value.Reason?.Context?.SourceAbilityContext, context) &&
                                    ReferenceEquals(value.Reason?.Context?.SourceAbility, ability) && ReferenceEquals(value.Reason?.Context?.MaybeCaster, owner)).ToArray();
                                var applications = saves.NativeApplications.Where(value => ReferenceEquals(value.Initiator, recipient) &&
                                    ReferenceEquals(value.Context?.SourceAbilityContext, context) && ReferenceEquals(value.Context?.SourceAbility, ability) &&
                                    ReferenceEquals(value.Context?.MaybeCaster, owner) && value.Blueprint.AssetGuid == "09d39b38bb7c6014394b6daced9bacd3").ToArray();
                                var dealt = hits.Where(value => ReferenceEquals(value.Target, recipient) &&
                                    ReferenceEquals(value.Reason?.Context?.SourceAbilityContext, context)).ToArray();
                                bool saved = rolls.Length == 1 && rolls[0].IsPassed;
                                bool matched = rolls.Length == 1 && rolls[0].DifficultyClass == 16 && dealt.Length == 1 &&
                                    (saved ? applications.Length == 0 : applications.Length == 1 && applications[0].AppliedBuff != null && applications[0].Duration?.TotalSeconds == 6);
                                stunPass &= matched;
                                lowFailed |= ReferenceEquals(recipient, target) && matched && !saved;
                                highPassed |= ReferenceEquals(recipient, second) && matched && saved;
                                recipients.Add(new JObject { ["target"] = recipient.UniqueId, ["saveCount"] = rolls.Length,
                                    ["dc"] = rolls.Length == 1 ? (JToken)new JValue(rolls[0].DifficultyClass) : JValue.CreateNull(),
                                    ["passed"] = saved, ["stunApplications"] = applications.Length, ["damageEvents"] = dealt.Length, ["matched"] = matched });
                            }
                            sawBothOutcomes |= lowFailed && highPassed;
                            correlated.Add(new JObject { ["castOrdinal"] = index, ["ability"] = ability.AssetGuid, ["caster"] = owner.UniqueId, ["recipients"] = recipients });
                        }
                        stunPass &= sawBothOutcomes;
                        bool allySafe = sibling.Damage == 0 && !sibling.Descriptor.State.HasCondition(UnitCondition.Stunned);
                        Sprint25Check(id + "-natural-cast", castPass && damagePass && stunPass && allySafe, new JObject { ["commands"] = new JArray(commands.Select(Sprint27AiCommand)),
                            ["costs"] = costs, ["saves"] = saves.Rows, ["applications"] = saves.Applications, ["castCorrelations"] = correlated,
                            ["damageSourceAbilities"] = new JArray(hits.Select(value => value.SourceAbility.AssetGuid)), ["damage"] = new JArray(hits.Select(Sprint16DamageEvent)),
                            ["slotsAfter"] = book.GetSpontaneousSlots(level), ["siblingBudgetAfter"] = siblingBudget(),
                            ["siblingDamage"] = sibling.Damage, ["aiTicks"] = ticks, ["nativeTurnFrames"] = turnFrames, ["fixtureCastCommands"] = 0, ["forcedRolls"] = 0 },
                            "Natural safe Sound Burst pays real Standard/L2 costs and deals1d8 sonic on both Fort outcomes; a failed save applies the native one-round stun.");
                        if (!castPass || !damagePass || !stunPass || !allySafe) throw new InvalidOperationException("Natural Sound Burst outcome failed.");
                    }
                    else
                    {
                        bool effectPass = charmEffect != null && charmEffect.Active && !charmEffect.IsDisposed && charmEffect.Context.Params.CasterLevel == 7 &&
                            charmEffect.Context.Params.DC == 17 && !owner.IsEnemy(target) && score(policy, target) == 0 &&
                            saves.Applications.Any(value => (string)value["buff"] == charmBuff.AssetGuid && (bool)value["applied"] &&
                                (string)value["sourceAbility"] == ability.AssetGuid && (string)value["caster"] == owner.UniqueId &&
                                (double?)value["durationSeconds"] == 604800);
                        Sprint25Check(id + "-natural-cast", castPass && effectPass, new JObject { ["commands"] = new JArray(commands.Select(Sprint27AiCommand)),
                            ["costs"] = costs, ["saves"] = saves.Rows, ["applications"] = saves.Applications, ["slotsAfter"] = book.GetSpontaneousSlots(level),
                            ["siblingBudgetAfter"] = siblingBudget(), ["effectEnd"] = charmEffect?.EndTime.TotalSeconds,
                            ["activeTargetScore"] = score(policy, target), ["aiTicks"] = ticks, ["nativeTurnFrames"] = turnFrames,
                            ["fixtureCastCommands"] = 0, ["forcedRolls"] = 0 },
                            "Natural Charm Monster affects a real nonhumanoid, pays Standard/L3 costs, uses CL7/DC17/seven days, and refuses its active target.");
                        if (!castPass || !effectPass) throw new InvalidOperationException("Natural Charm Monster outcome failed.");
                        if (!turnBased)
                        {
                        // A supplied ordinary healing command is a harmless
                        // negative probe, not another autonomous choice claim.
                        var healData = book.GetKnownSpells(1).Single(value => value.Blueprint.AssetGuid == "5590652e1c2225c4ca30c4a699ab3649");
                        target.Descriptor.Damage = 12;
                        int healSlots = book.GetSpontaneousSlots(1);
                        if (!healData.IsAvailable || !healData.CanTarget(new TargetWrapper(target)))
                            throw new InvalidOperationException("Ordinary healing negative lacks native eligibility.");
                        var heal = new UnitUseAbility(healData, new TargetWrapper(target));
                        owner.Commands.Run(heal); limit = DateTime.UtcNow.AddSeconds(24);
                        while (DateTime.UtcNow < limit && (!heal.IsFinished || heal.ExecutionProcess?.IsEnded != true || target.Damage >= 12))
                        { game.IsPaused = false; yield return 0; }
                        bool harmless = heal.IsActed && heal.IsFinished && heal.Result == UnitCommand.ResultType.Success &&
                            heal.ExecutionProcess?.IsEnded == true && target.Damage < 12 && charmEffect.Active && !charmEffect.IsDisposed &&
                            book.GetSpontaneousSlots(1) == healSlots - 1 && economy.Count(heal) == 1 &&
                            (economy.InCombat(heal) ? economy.Exact(heal, false) : economy.Unchanged(heal)) &&
                            JToken.DeepEquals(siblingBefore, siblingBudget());
                        Sprint25Check(id + "-harmless-heal", harmless, new JObject { ["command"] = Sprint27AiCommand(heal),
                            ["allyMetadata"] = healData.Blueprint.EffectOnAlly.ToString(), ["enemyMetadata"] = healData.Blueprint.EffectOnEnemy.ToString(),
                            ["damageBefore"] = 12, ["damageAfter"] = target.Damage, ["slotsBefore"] = healSlots,
                            ["slotsAfter"] = book.GetSpontaneousSlots(1), ["effectActive"] = charmEffect.Active,
                            ["effectDisposed"] = charmEffect.IsDisposed, ["fixtureHealCommands"] = 1 },
                            "This caster's real helpful spell heals the charmed animal, spends its native slot/action and leaves Charm intact; hostile-metadata fallback does not misclassify healing.");
                        if (!harmless) throw new InvalidOperationException("Harmless healing interrupted Charm or failed its normal cast.");
                        // The next supplied ordinary command is an RTWP threat probe,
                        // not additional autonomous attack or casting evidence.
                        SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false; game.TurnBasedCombatController.Activate();
                        var attack = new UnitAttack(target); double deadline = charmEffect.EndTime.TotalSeconds;
                        owner.Commands.Run(attack); limit = DateTime.UtcNow.AddSeconds(24);
                        while (DateTime.UtcNow < limit && (!attack.IsStarted || charmEffect.Active && !charmEffect.IsDisposed || !owner.IsEnemy(target)))
                        { game.IsPaused = false; yield return 0; }
                        bool broken = attack.IsStarted && (!charmEffect.Active || charmEffect.IsDisposed) && owner.IsEnemy(target) &&
                            deadline > game.Player.GameTime.TotalSeconds && owner.IsInState && !owner.Destroyed && !target.Descriptor.State.IsDead;
                        Sprint25Check(id + "-caster-threat", broken, new JObject { ["command"] = Sprint27AiCommand(attack),
                            ["originalEnd"] = deadline, ["gameSeconds"] = game.Player.GameTime.TotalSeconds,
                            ["effectDisposed"] = charmEffect.IsDisposed, ["enemyAgain"] = owner.IsEnemy(target), ["fixtureAttackCommands"] = 1 },
                            "A normal attack directed by this live caster breaks the exact Charm before its deadline and restores hostility; no direct buff removal establishes the verdict.");
                        if (!broken) throw new InvalidOperationException("Caster threat did not break private Charm.");
                        }
                    }
                }
            }
            finally
            {
                _lillendHoldOwnerTicks32 = null;
                if (starts != null) EventBus.Unsubscribe(starts); if (saves != null) EventBus.Unsubscribe(saves);
                if (damage != null) EventBus.Unsubscribe(damage); if (otherDamage != null) EventBus.Unsubscribe(otherDamage);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false; game.TurnBasedCombatController.Activate();
                foreach (var actor in actors.Where(value => value != null && !value.Destroyed))
                { Sprint28SetBrainActivePreservingActions(actor, false); actor.Commands.InterruptAll(true); if (actor.IsInCombat) actor.LeaveCombat(); }
                if (charmEffect != null && !charmEffect.IsDisposed) charmEffect.Remove();
                game.Player.UpdateIsInCombat();
                DisposeExpandedSummoningUnits(fixture.Created, actors.Where(value => value != null && !value.Destroyed).ToArray());
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode; game.TurnBasedCombatController.Activate(); game.IsPaused = oldPause;
            }
        }

        private static JObject Sprint32SavedBudgetForControl(UnitEntityData actor, ExpandedSummoningCorrectionFixture fixture)
        {
            return new JObject(fixture.Blueprints.OfType<BlueprintAbilityResource>().Where(value =>
                value.name.StartsWith("KMG_Summoning_FeyCelestial_LillendAzata_", StringComparison.Ordinal))
                .Select(value => new JProperty(value.AssetGuid, actor.Descriptor.Resources.GetResourceAmount(value))));
        }
    }
}
