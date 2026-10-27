using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Controllers.Brain;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Parts;
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
            // Piggyback the bounded read-only mapping investigation on this
            // already selected support case; no separate actor or game launch.
            CaptureSprint32SatyrAnimationInputs();
            var bodyResources = new List<UnityEngine.Object>();
            var borrowedBodyResources = new List<UnityEngine.Object>();
            Exception failure = null;
            var iterator = ReviewLillend32SupportCore(fixture, bodyResources, borrowedBodyResources).GetEnumerator();
            // The established tooltip observer pattern lets native destruction
            // settle after a failed case without losing the original failure.
            try
            {
                while (true)
                {
                    bool next = false;
                    try { next = iterator.MoveNext(); }
                    catch (Exception error) { failure = error; }
                    if (!next) break;
                    yield return iterator.Current;
                }
            }
            finally
            {
                try { iterator.Dispose(); }
                catch (Exception error) { failure = failure == null ? error : new AggregateException(failure, error); }
            }
            for (int frame = 0; frame < 8; frame++) yield return 0;
            bool bodyCleanup = bodyResources.Count > 0 && bodyResources.All(value => value == null) &&
                borrowedBodyResources.Count > 0 && borrowedBodyResources.All(value => value != null);
            Sprint25Check("lillend-original-body-cleanup", bodyCleanup, new JObject {
                ["ownedResources"] = bodyResources.Count, ["ownedRemaining"] = bodyResources.Count(value => value != null),
                ["borrowedResources"] = borrowedBodyResources.Distinct().Count(),
                ["borrowedMissing"] = borrowedBodyResources.Count(value => value == null),
                ["originalFailure"] = failure?.ToString() },
                "Native fixture disposal destroys owned body/tail resources and preserves borrowed native actions/clips; expiry/dismissal are separate cases.");
            if (failure != null) throw new InvalidOperationException("Lillend support failed; original error retained after body cleanup observation.", failure);
            if (!bodyCleanup) throw new InvalidOperationException("Lillend body cleanup failed.");
        }

        private IEnumerable<int> ReviewLillend32SupportCore(ExpandedSummoningCorrectionFixture fixture,
            List<UnityEngine.Object> bodyResources, List<UnityEngine.Object> borrowedBodyResources)
        {
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue, oldPause = Game.Instance.IsPaused;
            int casterDamage = fixture.Caster.Damage;
            Vector3 casterPosition = fixture.Caster.Position;
            UnitEntityData owner = null, sibling = null;
            CreateExpandedSummoningCorrectionHostile(fixture);
            PlaceExpandedSummoningUnit(fixture.Hostile, fixture.Caster.Position + Vector3.forward * 40f);
            var target = fixture.Hostile; int ac = target.Stats.AC.BaseValue;
            BlueprintFeature mindImmunity = null;
            LillendHumanVisualAttachment ownerBody = null, siblingBody = null;
            bool addedManeuverImmunity = false;
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;
                owner = CastExpandedSummoningOwnTier(fixture, "lillend-azata");
                sibling = CastExpandedSummoningOwnTier(fixture, "lillend-azata");
                ReadSprint32NatureKnowledge(owner, sibling, "lillend-nature-knowledge");
                foreach (var unit in new[] { owner, sibling })
                { Sprint28SetBrainActivePreservingActions(unit, false); unit.Commands.InterruptAll(true); unit.Commands.RemoveFinishedAndUpdateQueue(); }
                for (int frame = 0; frame < 180; frame++)
                {
                    ownerBody = owner.View?.GetComponent<LillendHumanVisualAttachment>();
                    siblingBody = sibling.View?.GetComponent<LillendHumanVisualAttachment>();
                    if (ownerBody != null && ownerBody.Live && siblingBody != null && siblingBody.Live) break;
                    yield return 0;
                }
                bool bodies = ownerBody != null && ownerBody.Live && siblingBody != null && siblingBody.Live &&
                    ownerBody.NativeActionsUnchanged && siblingBody.NativeActionsUnchanged &&
                    ownerBody.AuxiliaryGeometrySuppressed && siblingBody.AuxiliaryGeometrySuppressed &&
                    !ReferenceEquals(ownerBody.Body.sharedMesh, siblingBody.Body.sharedMesh) &&
                    !ReferenceEquals(ownerBody.Body.sharedMaterial, siblingBody.Body.sharedMaterial) &&
                    !ReferenceEquals(ownerBody.TailAction, siblingBody.TailAction);
                if (bodies) foreach (var attachment in new[] { ownerBody, siblingBody })
                {
                    bodyResources.AddRange(attachment.CaptureOwnedResources()); bodyResources.Add(attachment);
                    borrowedBodyResources.AddRange(attachment.BorrowedResources());
                }
                var bodyRow = new JObject { ["ownerBinding"] = ExpandedSummoningLillendViewPatch.DescribeView(owner.View),
                    ["siblingBinding"] = ExpandedSummoningLillendViewPatch.DescribeView(sibling.View),
                    ["independentOwnedBodies"] = bodies, ["finalVisualQualification"] = false };
                if (bodies)
                {
                    bodyRow["restPose"] = Lillend32BodySample(owner, ownerBody);
                    bodyRow["siblingRestPose"] = Lillend32BodySample(sibling, siblingBody);
                    bodyRow["supportingRestFrame"] = CaptureSprint17OriginalBodyFrame(owner, ownerBody.Body, "s32-lillend-rest");
                    bodies = (bool)bodyRow["restPose"]["finite"] && (bool)bodyRow["siblingRestPose"]["finite"];
                }
                Sprint25Check("lillend-original-body-bound", bodies, bodyRow,
                    "Both exact Lillends load independent original bodies, preserve borrowed native actions and retain finite rest poses.");
                if (!bodies) throw new InvalidOperationException("Original Lillend body failed; native carrier is not an accepted proxy.");
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
                    book.GetKnownSpells(0).Select(value => value.Blueprint.AssetGuid).OrderBy(value => value)
                        .SequenceEqual(new[] { KingmakerGunslinger.Blueprints.BlueprintManifest.Load(_context.ModEntry.Path).ResolveActive("KMG.Summoning.FeyCelestial.LillendAzata.Daze", typeof(Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility)).Id.Value,
                            KingmakerGunslinger.Blueprints.BlueprintManifest.Load(_context.ModEntry.Path).ResolveActive("KMG.Summoning.FeyCelestial.LillendAzata.Lullaby", typeof(Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility)).Id.Value }.OrderBy(value => value)) &&
                    book.GetKnownSpells(1).Count() == 3 &&
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
                // Isolate ordinary weapon fallback from the separate real
                // tail-grab case. A successful native grab disables the owner
                // and can legitimately interrupt attacks and performance.
                if (target.Descriptor.State.HasCondition(UnitCondition.ImmuneToCombatManeuvers))
                    throw new InvalidOperationException("Disposable target already has maneuver immunity; ownership ambiguous.");
                target.Descriptor.State.AddCondition(UnitCondition.ImmuneToCombatManeuvers, null);
                addedManeuverImmunity = true;
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
                    ["distanceMeters"] = owner.DistanceTo(target), ["weaponRangeFeet"] = owner.Body.PrimaryHand.MaybeWeapon?.AttackRange.Value,
                    ["fixtureManeuverImmune"] = target.Descriptor.State.HasCondition(UnitCondition.ImmuneToCombatManeuvers) },
                    "Exact support actor, nearby ally and visible hostile admit the native self-performance decision.");
                if (!ready) throw new InvalidOperationException("Lillend support fixture preconditions failed.");
                foreach (var stage in new[] { "courage", "healing", "ally-healing", "fallback" })
                {
                    Sprint28SetBrainActivePreservingActions(owner, false);
                    owner.Commands.InterruptAll(true); owner.Commands.RemoveFinishedAndUpdateQueue();
                    bool healing = stage == "healing" || stage == "ally-healing";
                    var healRecipient = stage == "ally-healing" ? fixture.Caster : owner;
                    if (stage == "healing") owner.Damage = 18;
                    if (stage == "ally-healing")
                    {
                        // The exact disposable caster is player-faction and is
                        // not native IsAlly to the summon. It must be selected
                        // by real healing AI, not just score as eligible.
                        owner.Damage = 0;
                        PlaceExpandedSummoningUnit(fixture.Caster, owner.Position + Vector3.right * 2f);
                        fixture.Caster.Damage = 18;
                        fixture.Caster.Wake(0f);
                        if (!Game.Instance.State.AwakeUnits.Contains(fixture.Caster)) Game.Instance.State.AwakeUnits.Add(fixture.Caster);
                        owner.Memory.Add(fixture.Caster);
                        owner.SightCache.ClearIfNeeded();
                    }
                    if (stage == "fallback")
                    {
                        fixture.Caster.Damage = casterDamage;
                        PlaceExpandedSummoningUnit(fixture.Caster, casterPosition);
                    }
                    if (stage == "fallback") owner.Damage = 0; // new healthy-target fixture state, after retaining the real healing result
                    var controlScores = fixture.Blueprints.OfType<LillendSpellEligibility32>()
                        .Where(value => ReferenceEquals(value.OwningBlueprint, owner.Blueprint))
                        .ToDictionary(value => value.Choice.ToString(), value => value.Score(new DecisionContext {
                            Unit = owner, Target = new TargetInfo { Unit = target } }));
                    int slotsBefore = book.GetSpontaneousSlots(3), damageBefore = healRecipient.Damage;
                    var commands = new HashSet<UnitCommand>(); var costs = new JArray(); var ambient = new HashSet<string>();
                    var attacks = new Sprint24AttackObserver { Owner = owner, Target = target };
                    var presentation = new JArray();
                    attacks.AfterWeapon = rule => {
                        if (presentation.Count >= 3) return;
                        try {
                            var row = Lillend32BodySample(owner, ownerBody);
                            row["weapon"] = rule.Weapon.Blueprint.AssetGuid;
                            row["category"] = rule.Weapon.Blueprint.Category.ToString();
                            row["tailActEvents"] = ownerBody.TailAction.ActEvents;
                            row["tailNativeTime"] = ownerBody.TailAction.LastNativeTime;
                            row["supportingFrame"] = CaptureSprint17OriginalBodyFrame(owner, ownerBody.Body,
                                "s32-lillend-" + stage + "-attack-" + presentation.Count);
                            presentation.Add(row);
                        } catch (Exception error) { presentation.Add(new JObject { ["error"] = error.ToString() }); }
                    };
                    var starts = new Lillend32CommandStarts { Owner = owner };
                    EventBus.Subscribe(attacks); EventBus.Subscribe(starts);
                    try
                    {
                        Sprint28SetBrainActivePreservingActions(owner, true); owner.Brain.RestoreAvailableActions();
                        using (var economy = new FirearmReviewCosts(owner, costs))
                        {
                            DateTime deadline = DateTime.UtcNow.AddSeconds(35); int frames = 0, ticks = 0;
                            UnitCommand resolved = null; UnitUseAbility delivery = null;
                            while (DateTime.UtcNow < deadline && frames++ < 4000)
                            {
                                if (frames == 1 || frames % 60 == 0 && !owner.Commands.Raw.Any(value => value != null && !value.IsFinished))
                                { foreach (var candidate in Sprint24TickExactAi(owner)) if (candidate != owner.UniqueId) ambient.Add(candidate); ticks++; }
                                foreach (var command in owner.Commands.Raw.Where(value => value != null)) commands.Add(command);
                                foreach (var command in starts.Commands) commands.Add(command);
                                resolved = commands.FirstOrDefault(value => value.IsFinished && value.Result == UnitCommand.ResultType.Success &&
                                    (stage == "courage" ? value is UnitActivateAbility && value.AiAction == performAi &&
                                        ReferenceEquals(((UnitActivateAbility)value).Ability, performance) :
                                     healing ? value is UnitUseAbility && value.AiAction?.name == "KMG_Summoning_FeyCelestial_LillendAzata_HealAi" &&
                                         ReferenceEquals(value.Target.Unit, healRecipient) &&
                                         ((UnitUseAbility)value).ExecutionProcess != null && ((UnitUseAbility)value).ExecutionProcess.IsEnded :
                                     value is UnitAttack && value.AiAction != null));
                                if (stage == "ally-healing" && resolved != null)
                                {
                                    var primary = (UnitUseAbility)resolved;
                                    var sticky = primary.Spell.Blueprint.GetComponent<AbilityEffectStickyTouch>();
                                    var deliveries = commands.OfType<UnitUseAbility>().Where(value =>
                                        !ReferenceEquals(value, primary) && ReferenceEquals(value.Executor, owner) &&
                                        ReferenceEquals(value.Target?.Unit, healRecipient) &&
                                        ReferenceEquals(value.Spell.Blueprint, sticky?.TouchDeliveryAbility) &&
                                        starts.TouchFrom(value, primary.Spell)).ToArray();
                                    if (deliveries.Length == 1) delivery = deliveries[0];
                                    else if (deliveries.Length > 1) throw new InvalidOperationException("Ambiguous natural heal delivery.");
                                }
                                bool healingFinished = !healing || healRecipient.Damage < damageBefore &&
                                    (stage != "ally-healing" || delivery != null && delivery.IsFinished &&
                                        delivery.Result == UnitCommand.ResultType.Success && delivery.ExecutionProcess?.IsEnded == true &&
                                        owner.Get<UnitPartTouch>() == null);
                                if (resolved != null && healingFinished && (stage != "courage" ||
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
                                healing ? healRecipient.Damage < damageBefore && book.GetSpontaneousSlots(3) == slotsBefore - 1 &&
                                    (stage != "ally-healing" || owner.Damage == 0 && fixture.Caster.IsPlayerFaction && !owner.IsEnemy(fixture.Caster)) :
                                attacks.WeaponAttacks.Count > 0 && controlScores.Values.All(value => value == 0f) && healthyScore == 0f && !performAi.IsAvailable(new DecisionContext { Unit = owner }) &&
                                book.GetSpontaneousSlots(3) == slotsBefore;
                            var healingCommands = starts.Commands.OfType<UnitUseAbility>().ToArray();
                            bool oneHeal = !healing || resolved != null &&
                                healingCommands.Length == (stage == "ally-healing" ? 2 : 1) &&
                                healingCommands.All(value => ReferenceEquals(value, resolved) ||
                                    stage == "ally-healing" && ReferenceEquals(value, delivery));
                            bool healAttributed = !healing || starts.ExactHealing(
                                resolved as UnitUseAbility, healRecipient);
                            bool deliveryExact = stage != "ally-healing" || delivery != null &&
                                delivery.AiAction == null && !delivery.Cutscene && starts.FirstRoundTouches.Contains(delivery) &&
                                economy.Count(delivery) == 1 && economy.Unchanged(delivery) &&
                                economy.IgnoredCooldown(delivery) && economy.ForcedCount(delivery) == 0;
                            bool passed = resolved != null && economy.Count(resolved) == 1 && economy.Exact(resolved, false) &&
                                oneHeal && healAttributed && deliveryExact &&
                                resolved.Type == (stage == "courage" ? UnitCommand.CommandType.Move : UnitCommand.CommandType.Standard) &&
                                outcome && siblingBook.GetSpontaneousSlots(3) == 2 && sibling.Descriptor.Resources.GetResourceAmount(resource) == 20;
                            Sprint25Check("lillend-" + stage + "-resolved", passed, new JObject {
                                ["owner"] = owner.UniqueId, ["sibling"] = sibling.UniqueId, ["frames"] = frames, ["aiTicks"] = ticks,
                                ["commands"] = new JArray(commands.Select(Sprint27AiCommand)), ["costs"] = costs,
                                ["nativeCommandStarts"] = starts.Rows, ["bodyAtWeaponEvents"] = presentation,
                                ["healingRules"] = starts.HealRows, ["exactHealingAttribution"] = healAttributed,
                                ["onePrimaryHealingCast"] = oneHeal, ["naturalTouchDeliveryExact"] = deliveryExact,
                                ["naturalTouchDelivery"] = delivery == null ? null : Sprint27AiCommand(delivery),
                                ["heldTouchRemaining"] = owner.Get<UnitPartTouch>() != null,
                                ["tailStarts"] = ownerBody.TailAction.StartedHandles, ["tailActs"] = ownerBody.TailAction.ActEvents,
                                ["tailFinishes"] = ownerBody.TailAction.FinishedHandles, ["tailFailure"] = ownerBody.TailAction.Failure,
                                ["performanceRunning"] = performance.IsRunning, ["performanceRounds"] = owner.Descriptor.Resources.GetResourceAmount(resource),
                                ["allyAttackBefore"] = allyAttack, ["allyAttackAfter"] = sibling.Stats.AdditionalAttackBonus.ModifiedValue,
                                ["ownedAllyEffects"] = ownedEffects.Length, ["enemySafe"] = enemySafe,
                                ["healingRecipient"] = healing ? healRecipient.UniqueId : null,
                                ["healingRecipientIsPlayerFaction"] = healing && healRecipient.IsPlayerFaction,
                                ["healingRecipientNativeIsAlly"] = healing && owner.IsAlly(healRecipient),
                                ["damageBefore"] = damageBefore, ["damageAfter"] = healRecipient.Damage,
                                ["level3SlotsBefore"] = slotsBefore, ["level3SlotsAfter"] = book.GetSpontaneousSlots(3),
                                ["siblingLevel3Slots"] = siblingBook.GetSpontaneousSlots(3), ["siblingPerformanceRounds"] = sibling.Descriptor.Resources.GetResourceAmount(resource),
                                ["controlEligibility"] = new JObject(controlScores.Select(pair => new JProperty(pair.Key, pair.Value))),
                                ["targetMindImmune"] = true,
                                ["fixtureManeuverImmune"] = target.Descriptor.State.HasCondition(UnitCondition.ImmuneToCombatManeuvers),
                                ["healthyHealingScore"] = healthyScore, ["weaponAttackEvents"] = attacks.WeaponAttacks.Count,
                                ["ambientCandidatesNeverTicked"] = new JArray(ambient), ["fixtureCastCommands"] = 0,
                                ["fixtureAttackCommands"] = 0, ["fixtureSlotDebits"] = 0, ["forcedRolls"] = 0 },
                                stage == "courage" ? "Native AI starts Inspire Courage with one move action, grants an ally+2 without buffing the enemy and spends one native performance round." :
                                healing ? "Native AI selects the exact wounded self or player-faction ally, heals with a real bard spell and standard action, and spends one own level3 slot while the sibling retains both." :
                                "With performance running and no useful healing target, native AI attacks without repeating either support command; sibling pools remain untouched.");
                            if (!passed) throw new InvalidOperationException("Lillend early support observation failed: " + stage);
                            if (stage == "courage")
                            {
                                int siblingDamage = sibling.Damage;
                                try
                                {
                                    sibling.Damage = 18;
                                    ObserveSprint32GlobeChoice(fixture, owner, sibling, new AbilityData(heal.Spell, book),
                                        () => heal.Score(new DecisionContext { Unit = owner, Target = new TargetInfo { Unit = sibling } }),
                                        "lillend-healing-performance", performanceGrant: ownedEffects.Single());
                                }
                                finally { sibling.Damage = siblingDamage; }
                            }
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
                fixture.Caster.Damage = casterDamage;
                PlaceExpandedSummoningUnit(fixture.Caster, casterPosition);
                if (addedManeuverImmunity) target.Descriptor.State.RemoveCondition(UnitCondition.ImmuneToCombatManeuvers);
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

        private static JObject Lillend32BodySample(UnitEntityData unit, LillendHumanVisualAttachment attachment)
        {
            Vector3[] points = Sprint17OriginalWorldVertices(attachment.Body);
            bool finite = points.Length > 0 && points.All(point =>
                LillendTailAnimationPolicy.Finite(point.x) && LillendTailAnimationPolicy.Finite(point.y) &&
                LillendTailAnimationPolicy.Finite(point.z));
            return new JObject { ["frame"] = Time.frameCount, ["finite"] = finite,
                ["vertices"] = points.Length, ["worldHeight"] = points.Max(p => p.y) - points.Min(p => p.y),
                ["worldWidth"] = points.Max(p => p.x) - points.Min(p => p.x),
                ["floor"] = Sprint17MeasuredFloor(unit, unit.Position),
                ["lowestAboveActor"] = points.Min(p => p.y) - unit.Position.y,
                ["nativeActionsUnchanged"] = attachment.NativeActionsUnchanged,
                ["gait"] = attachment.DescribeGait() };
        }

        // Event-bound observation retains a command replaced within one frame;
        // frame snapshots alone missed the original in the preserved 2e1 run.
        private sealed class Lillend32CommandStarts : IUnitCommandStartHandler,
            IUnitCommandActHandler, IUnitCommandEndHandler, IGlobalRulebookHandler<RuleAttackWithWeapon>,
            IGlobalRulebookHandler<RuleHealDamage>
        {
            internal UnitEntityData Owner;
            internal bool ObserveRtwpAttackCompletion; // Exact technical-presentation instance opt-in only.
            internal readonly HashSet<UnitCommand> Commands = new HashSet<UnitCommand>();
            internal readonly JArray Rows = new JArray();
            internal readonly JArray AttackRows = new JArray();
            internal readonly JArray HealRows = new JArray();
            internal readonly HashSet<UnitUseAbility> FirstRoundTouches = new HashSet<UnitUseAbility>();
            private readonly Dictionary<RuleHealDamage, JObject> heals = new Dictionary<RuleHealDamage, JObject>();
            private readonly Dictionary<UnitUseAbility, AbilityData> touchSources = new Dictionary<UnitUseAbility, AbilityData>();
            private readonly Dictionary<RuleHealDamage, AbilityData> healSources = new Dictionary<RuleHealDamage, AbilityData>();
            private readonly Dictionary<UnitAttack, BlueprintItemWeapon[]> completedPlans = new Dictionary<UnitAttack, BlueprintItemWeapon[]>();
            private readonly Dictionary<UnitAttack, List<BlueprintItemWeapon>> resolvedWeapons = new Dictionary<UnitAttack, List<BlueprintItemWeapon>>();
            private bool ambiguousWeaponAttribution;
            public void HandleUnitCommandDidStart(UnitCommand command)
            {
                if (command == null || !ReferenceEquals(command.Executor, Owner)) return;
                Commands.Add(command);
                var use = command as UnitUseAbility;
                var touch = Owner.Get<UnitPartTouch>();
                if (use != null && use.Spell.StickyTouch != null) touchSources[use] = use.Spell.StickyTouch;
                if (use != null && touch?.IsCastedInThisRound == true &&
                    ReferenceEquals(use.Spell, touch.Ability?.Data)) FirstRoundTouches.Add(use);
                var row = Sprint27AiCommand(command);
                row["firstRoundHeldTouch"] = use != null && FirstRoundTouches.Contains(use);
                row["ignoreCooldownAtStart"] = command.IsIgnoreCooldown;
                row["commandReference"] = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(command);
                row["ability"] = (command as UnitActivateAbility)?.Ability?.Blueprint?.AssetGuid;
                Rows.Add(row);
            }

            public void OnEventAboutToTrigger(RuleHealDamage evt)
            {
                if (!ReferenceEquals(evt.Initiator, Owner) || evt.Target == null) return;
                var row = new JObject { ["initiator"] = evt.Initiator.UniqueId, ["target"] = evt.Target.UniqueId,
                    ["damageBefore"] = evt.Target.Damage, ["ability"] = evt.Reason?.Ability?.Blueprint?.AssetGuid,
                    ["contextAbility"] = evt.Reason?.Context?.SourceAbility?.AssetGuid };
                // Native Ability.Dispose clears StickyTouch after effect
                // delivery. Capture the live link at the rule boundary; do
                // not query a consumed AbilityData after the command ends.
                healSources.Add(evt, evt.Reason?.Ability?.StickyTouch);
                heals.Add(evt, row); HealRows.Add(row);
            }
            public void OnEventDidTrigger(RuleHealDamage evt)
            {
                JObject row;
                if (!heals.TryGetValue(evt, out row)) return;
                row["damageAfter"] = evt.Target.Damage; row["value"] = evt.Value;
            }
            internal bool TouchFrom(UnitUseAbility command, AbilityData primary)
            {
                AbilityData source;
                return touchSources.TryGetValue(command, out source) && ReferenceEquals(source, primary);
            }
            internal bool ExactHealing(UnitUseAbility primary, UnitEntityData recipient)
            {
                if (primary?.ExecutionProcess == null || !primary.ExecutionProcess.IsEnded) return false;
                var sticky = primary.Spell.Blueprint.GetComponent<AbilityEffectStickyTouch>();
                var matches = heals.Where(pair => ReferenceEquals(pair.Key.Target, recipient) &&
                    ReferenceEquals(healSources[pair.Key], primary.Spell) &&
                    (string)pair.Value["ability"] == sticky?.TouchDeliveryAbility?.AssetGuid &&
                    (string)pair.Value["contextAbility"] == sticky?.TouchDeliveryAbility?.AssetGuid).ToArray();
                foreach (var pair in matches) pair.Value["exactLiveNativeTouchSource"] = true;
                return matches.Length == 1 && matches[0].Key.Value > 0 &&
                    (int?)matches[0].Value["damageBefore"] - (int?)matches[0].Value["damageAfter"] == matches[0].Key.Value;
            }

            public void OnEventAboutToTrigger(RuleAttackWithWeapon evt) { }
            public void OnEventDidTrigger(RuleAttackWithWeapon evt)
            {
                if (!ReferenceEquals(evt.Initiator, Owner) || (!ObserveRtwpAttackCompletion && !TurnBased.Controllers.CombatController.IsInTurnBasedCombat())) return;
                var candidates = Owner.Commands.Raw.OfType<UnitAttack>().Where(value => value.IsRunning &&
                    !value.IsFinished && Commands.Contains(value) && ReferenceEquals(value.Target, evt.Target)).ToArray();
                if (candidates.Length != 1)
                {
                    ambiguousWeaponAttribution = true;
                    AttackRows.Add(new JObject { ["event"] = "weapon-attribution-refused", ["candidates"] = candidates.Length,
                        ["target"] = evt.Target?.UniqueId, ["weapon"] = evt.Weapon.Blueprint.AssetGuid });
                    return;
                }
                var command = candidates[0];
                List<BlueprintItemWeapon> weapons;
                if (!resolvedWeapons.TryGetValue(command, out weapons)) resolvedWeapons.Add(command, weapons = new List<BlueprintItemWeapon>());
                weapons.Add(evt.Weapon.Blueprint);
                var row = AttackSnapshot(command, "weapon-rule-resolved");
                row["weapon"] = evt.Weapon.Blueprint.AssetGuid;
                AttackRows.Add(row);
            }

            public void HandleUnitCommandDidAct(UnitCommand command)
            {
                var attack = command as UnitAttack;
                if (attack == null || !ReferenceEquals(attack.Executor, Owner) || (!ObserveRtwpAttackCompletion && !TurnBased.Controllers.CombatController.IsInTurnBasedCombat())) return;
                var row = AttackSnapshot(attack, "native-command-act");
                AttackRows.Add(row);
                var plan = attack.AllAttacks.Select(value => value.Hand.MaybeWeapon.Blueprint).ToArray();
                if (attack.Result == UnitCommand.ResultType.Success && plan.Length > 0 &&
                    (int)ReadExpandedSummoningOptionalMember(attack, "m_AttackIndex") == plan.Length)
                    completedPlans[attack] = plan;
            }

            public void HandleUnitCommandDidEnd(UnitCommand command)
            {
                var attack = command as UnitAttack;
                if (attack != null && ReferenceEquals(attack.Executor, Owner) && (ObserveRtwpAttackCompletion || TurnBased.Controllers.CombatController.IsInTurnBasedCombat()))
                    AttackRows.Add(AttackSnapshot(attack, "native-command-end"));
            }

            private static JObject AttackSnapshot(UnitAttack attack, string kind)
            {
                var row = Sprint27AiCommand(attack);
                row["event"] = kind;
                row["commandReference"] = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(attack);
                row["attackIndex"] = (int)ReadExpandedSummoningOptionalMember(attack, "m_AttackIndex");
                row["actualPlan"] = new JArray(attack.AllAttacks.Select(value => value.Hand.MaybeWeapon.Blueprint.AssetGuid));
                row["plannedAbsent"] = attack.PlannedAttack == null;
                bool turnBased = TurnBased.Controllers.CombatController.IsInTurnBasedCombat();
                row["turnBased"] = turnBased;
                if (turnBased)
                {
                    row["nativeRound"] = Game.Instance.TurnBasedCombatController.RoundNumber;
                    row["nativeTurn"] = Sprint22TurnState(attack.Executor, attack.Target);
                }
                return row;
            }

            internal bool CompletedAttack(UnitAttack command, UnitEntityData target)
            {
                BlueprintItemWeapon[] plan; List<BlueprintItemWeapon> weapons;
                return command != null && command.IsFinished && !ambiguousWeaponAttribution &&
                    ReferenceEquals(command.Target, target) && Commands.Contains(command) &&
                    (command.Result == UnitCommand.ResultType.Success || command.Result == UnitCommand.ResultType.Interrupt) &&
                    completedPlans.TryGetValue(command, out plan) && resolvedWeapons.TryGetValue(command, out weapons) &&
                    plan.SequenceEqual(weapons) && command.AllAttacks.Count == plan.Length &&
                    (int)ReadExpandedSummoningOptionalMember(command, "m_AttackIndex") == plan.Length;
            }
        }
    }
}
