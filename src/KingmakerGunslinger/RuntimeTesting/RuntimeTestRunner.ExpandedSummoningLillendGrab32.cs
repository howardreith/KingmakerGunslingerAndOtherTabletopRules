using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using TurnBased.Controllers;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Parts;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Fixed isolated cells only. Native attack/roll/buff-round events establish
        // the hold and its riders. No TryGrab, MaintainLink, seeded roll or supplied
        // damage supplies a verdict. The separate Salamander cell checks the one
        // directly affected live-constrict integration consumer.
        private IEnumerable<int> ReviewLillendGrab32(ExpandedSummoningCorrectionFixture fixture, bool salamander, bool turnBased)
        {
            var game = Game.Instance; var target = fixture.Hostile;
            string key = salamander ? "salamander" : "lillend-azata";
            string id = "grab-" + key + (turnBased ? "-turn" : "-rtwp");
            UnitEntityData owner = null; BlueprintFeature immunity = null;
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue, oldPause = game.IsPaused;
            bool oldEngagementPause = SettingsRoot.Instance.PauseOnEngagement.CurrentValue;
            Cleanup2NativeDefaultScope nativeMode = null;
            UnitEntityData anchor = null; bool anchorBrainBefore = false, anchorBrainCaptured = false, complete = false;
            int oldAc = target.Stats.AC.BaseValue, oldTargetCmb = target.Stats.AdditionalCMB.BaseValue;
            var nonparticipantPasses = new JArray();
            var row = new JObject(); var strikes = new JArray(); var riders = new JArray(); var costs = new JArray();
            Sprint16RuleObserver observer = null; Lillend32CommandStarts starts = null;
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                game.TurnBasedCombatController.Activate(); game.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;
                owner = CastSprint32IdleFixtureSummon(fixture, key);
                Sprint28SetBrainActivePreservingActions(owner, false);
                owner.Commands.InterruptAll(true); owner.Commands.RemoveFinishedAndUpdateQueue();
                var originalActions = owner.Brain.Actions.ToArray();
                var grab = SummonGrabComponent.Find(owner);
                if (grab == null) throw new InvalidOperationException("Exact production grab component absent.");
                var tail = owner.Body.AdditionalLimbs.Select(value => value.MaybeWeapon)
                    .Single(value => value != null && grab.IsGrabLimb(owner, value));
                if (target.Descriptor.State.HasCondition(UnitCondition.ImmuneToCombatManeuvers) ||
                    target.Descriptor.State.Size != Kingmaker.Enums.Size.Medium || grab == null)
                    throw new InvalidOperationException("Grab requires a real Medium nonimmune fixture target.");
                var mind = ScriptableObject.CreateInstance<BuffDescriptorImmunity>(); mind.Descriptor = SpellDescriptor.MindAffecting;
                immunity = ScriptableObject.CreateInstance<BlueprintFeature>(); immunity.name = "KMG_Runtime_S32_Grab_MindImmunity";
                immunity.Ranks = 1; immunity.ComponentsArray = new BlueprintComponent[] { mind }; target.Descriptor.AddFact(immunity);
                var spent = new JObject();
                if (!salamander)
                {
                    var creation = owner.Blueprint.GetComponents<LillendBardBook32>().Single();
                    var book = owner.Descriptor.GetSpellbook(creation.Book);
                    foreach (int level in new[] { 1, 2, 3 })
                    {
                        int before = book.GetSpontaneousSlots(level);
                        for (int debit = 0; debit < before; debit++) book.GetKnownSpells(level).First().Spend();
                        if (book.GetSpontaneousSlots(level) != 0) throw new InvalidOperationException("Disclosed slot exhaustion failed.");
                        spent["level" + level] = before;
                    }
                    foreach (string suffix in new[] { "PerformanceResource", "CharmPersonSlaResource" })
                    {
                        var resource = fixture.Blueprints.OfType<BlueprintAbilityResource>().Single(value => value.name == "KMG_Summoning_FeyCelestial_LillendAzata_" + suffix);
                        int amount = owner.Descriptor.Resources.GetResourceAmount(resource);
                        owner.Descriptor.Resources.Spend(resource, amount); spent[suffix] = amount;
                    }
                }
                owner.Stats.AdditionalAttackBonus.BaseValue = 100; owner.Stats.AdditionalCMB.BaseValue = 100;
                owner.Stats.HitPoints.BaseValue = 100000; target.Stats.HitPoints.BaseValue = 100000; target.Stats.AC.BaseValue = -100;
                target.Stats.AdditionalCMB.BaseValue = -100; // native target escape rolls remain enabled
                Vector3 far; string lane, direction;
                var origin = Sprint24AiCombatLane(fixture.Caster, owner, target, 1.4f, out far, out lane, out direction);
                PlaceExpandedSummoningUnit(owner, origin); PlaceExpandedSummoningUnit(target, far);
                foreach (var actor in new[] { owner, target })
                { actor.Wake(0f); if (!game.State.AwakeUnits.Contains(actor)) game.State.AwakeUnits.Add(actor); }
                anchor = Sprint32OwnedQuietAnchor(fixture, id);
                anchorBrainBefore = anchor.IsBrainActive; anchorBrainCaptured = true;
                Sprint28SetBrainActivePreservingActions(anchor, false);
                row["exactIdleOwnedAnchor"] = anchor.UniqueId;
                SettingsRoot.Instance.PauseOnEngagement.CurrentValue = false;
                nativeMode = Cleanup2EnterNativeDefault(fixture, ExpandedSummoningNativeProfilePolicy.Sprint32GrabPhase);
                owner.Memory.Add(target); target.Memory.Add(owner);
                DateTime sightLimit = DateTime.UtcNow.AddSeconds(8); int sightFrames = 0;
                do
                {
                    if (!nativeMode.Current) throw new InvalidOperationException("Grab native Default lease changed.");
                    game.IsPaused = false; yield return 0; sightFrames++;
                    owner.SightCache.ClearIfNeeded(); target.SightCache.ClearIfNeeded();
                    if (!nativeMode.Current) throw new InvalidOperationException("Grab native Default lease changed.");
                } while (DateTime.UtcNow < sightLimit && (sightFrames < 8 || !owner.HasLOS(target) || !owner.Memory.ContainsVisible(target) ||
                    owner.View == null || !EntityFadedIn(owner) || !owner.Descriptor.State.CanAct || !owner.Descriptor.State.CanMove ||
                    owner.Buffs.GetBuff(BlueprintRoot.Instance.SystemMechanics.SummonedUnitAppearBuff) != null));
                owner.JoinCombat(); target.JoinCombat(); game.Player.UpdateIsInCombat();
                // Native Default controllers now own join, preparation, rounds and grapple.
                var admission = new JObject();
                if (turnBased)
                {
                    using (var enrollment = Sprint22EnrollTurnBasedOwners(new[] { owner }, target, admission).GetEnumerator())
                        while (true)
                        {
                            if (!nativeMode.Current) throw new InvalidOperationException("Grab native Default lease changed.");
                            if (!enrollment.MoveNext()) break;
                            yield return enrollment.Current;
                        }
                    var until = DateTime.UtcNow.AddSeconds(30);
                    while (!PollGrab32Turn(fixture, owner, target, anchor, nativeMode, nonparticipantPasses) && DateTime.UtcNow < until)
                    { if (!nativeMode.Current) throw new InvalidOperationException("Grab native Default lease changed."); yield return 0; }
                }
                bool ready = owner.HasLOS(target) && owner.Memory.ContainsVisible(target) && owner.IsEnemy(target) &&
                    owner.Descriptor.State.CanAct && owner.DistanceTo(target) < tail.AttackRange.Meters &&
                    owner.Descriptor.State.CanMove && owner.View != null && EntityFadedIn(owner) &&
                    owner.Buffs.GetBuff(BlueprintRoot.Instance.SystemMechanics.SummonedUnitAppearBuff) == null &&
                    (turnBased ? PollGrab32Turn(fixture, owner, target, anchor, nativeMode, nonparticipantPasses) : !TurnBased.Controllers.CombatController.IsInTurnBasedCombat());
                row["lane"] = lane; row["admission"] = admission; row["tail"] = tail.Blueprint.AssetGuid;
                row["distanceMeters"] = owner.DistanceTo(target); row["tailReachMeters"] = tail.AttackRange.Meters;
                row["fixtureSpentPools"] = spent; row["fixtureAccuracyAndCmb"] = 100;
                row["fixtureMindImmune"] = true; row["fixtureManeuverImmune"] = false;
                row["strikes"] = strikes; row["riders"] = riders; row["costs"] = costs;
                Sprint25Check(id + "-ready", ready, row, "Actual tail reach, native mode, visible Medium nonimmune hostile, unmodified AI actions and disclosed spent pools isolate normal melee.");
                if (!ready) throw new InvalidOperationException("Grab fixture readiness failed.");
                if (new[] { owner, target }.Any(actor => new[] { UnitCondition.CantAct, UnitCondition.CantMove,
                    UnitCondition.Entangled }.Any(condition => actor.Descriptor.State.HasCondition(condition))))
                    throw new InvalidOperationException("Grab fixture has preexisting blocking conditions.");
                observer = new Sprint16RuleObserver { Owner = owner, Target = target };
                starts = new Lillend32CommandStarts { Owner = owner };
                var attackAttribution = new Dictionary<RuleAttackRoll, UnitAttack>();
                RuleAttackRoll currentRoll = null, establishingRoll = null;
                UnitAttack establishingCommand = null;
                var successfulTailRolls = new HashSet<RuleAttackRoll>();
                observer.BeforeAttackRollForFixture = rule => {
                    currentRoll = rule;
                    var active = owner.Commands.Raw.OfType<UnitAttack>().Where(value => value.IsStarted && !value.IsFinished &&
                        ReferenceEquals(value.Target, target)).ToArray();
                    if (active.Length == 1) attackAttribution.Add(rule, active[0]);
                };
                observer.ObserveWeaponContact = rule => {
                    if (!ReferenceEquals(rule.Target, target)) return;
                    UnitAttack command;
                    bool attributed = rule.AttackRoll != null && attackAttribution.TryGetValue(rule.AttackRoll, out command);
                    if (attributed && ReferenceEquals(rule.Weapon, tail) && rule.AttackRoll.IsHit)
                        successfulTailRolls.Add(rule.AttackRoll);
                    strikes.Add(new JObject { ["weapon"] = rule.Weapon.Blueprint.AssetGuid,
                        ["exactTail"] = ReferenceEquals(rule.Weapon, tail), ["hit"] = rule.AttackRoll?.IsHit == true,
                        ["commandCandidates"] = attributed ? 1 : 0, ["mode"] = TurnBased.Controllers.CombatController.IsInTurnBasedCombat(),
                        ["damage"] = rule.MeleeDamage == null ? null : Sprint16DamageEvent(rule.MeleeDamage) });
                };
                observer.ObserveRiderContact = rule => {
                    var state = SummonHoldComponent.HeldState(owner, target, grab);
                    if (SummonHoldComponent.RoundsHeld(state) == 0 && establishingRoll == null && currentRoll != null &&
                        ReferenceEquals(currentRoll.Weapon, tail) && currentRoll.IsHit &&
                        attackAttribution.TryGetValue(currentRoll, out establishingCommand)) establishingRoll = currentRoll;
                    riders.Add(new JObject {
                    ["roundHeld"] = SummonHoldComponent.RoundsHeld(SummonHoldComponent.HeldState(owner, target, grab)),
                    ["strengthBonus"] = owner.Stats.Strength.Bonus, ["weaponAttacks"] = observer.WeaponAttacks,
                    ["gameSeconds"] = game.Player.GameTime.TotalSeconds, ["damage"] = Sprint16DamageEvent(rule) }); };
                EventBus.Subscribe(observer); EventBus.Subscribe(starts);
                using (var economy = new FirearmReviewCosts(owner, costs))
                {
                    Sprint28SetBrainActivePreservingActions(owner, true); owner.Brain.RestoreAvailableActions();
                    DateTime deadline = DateTime.UtcNow.AddSeconds(55); int frames = 0, ticks = 0;
                    while (DateTime.UtcNow < deadline && !ReferenceEquals(SummonHoldComponent.HeldTarget(owner), target))
                    {
                        if (!nativeMode.Current) throw new InvalidOperationException("Grab native Default lease changed.");
                        if (turnBased) PollGrab32Turn(fixture, owner, target, anchor, nativeMode, nonparticipantPasses);
                        frames++; game.IsPaused = false; yield return 0;
                    }
                    var held = SummonHoldComponent.HeldState(owner, target, grab);
                    bool linked = ReferenceEquals(SummonHoldComponent.HeldTarget(owner), target) && held != null &&
                        target.Get<UnitPartGrappleTarget>() != null && owner.Get<UnitPartGrappleInitiator>() != null &&
                        ReferenceEquals(SummonGrappleLinks.EstablishingWeapon(owner, target), tail);
                    var initial = riders.OfType<JObject>().Where(value => (int)value["roundHeld"] == 0).ToArray();
                    bool tailHit = establishingRoll != null && successfulTailRolls.Contains(establishingRoll) &&
                        TurnBased.Controllers.CombatController.IsInTurnBasedCombat() == turnBased;
                    bool initialDamage = initial.Length == 1 && Grab32Damage(initial[0], salamander, owner.Stats.Strength.Bonus);
                    bool paid = establishingCommand != null && establishingCommand.AiAction != null &&
                        economy.Count(establishingCommand) == 1 && economy.Exact(establishingCommand, turnBased);
                    row["fixtureAiTicks"] = ticks; row["nativeMode"] = nativeMode.Describe(); row["nativeCommandStarts"] = starts.Rows;
                    row["linked"] = linked; row["initialDamageExact"] = initialDamage; row["paidAttack"] = paid;
                    Sprint25Check(id + "-established", linked && tailHit && initialDamage && paid && originalActions.SequenceEqual(owner.Brain.Actions), row,
                        "Natural exact-tail hit and real grapple roll establish reciprocal native parts and one initial constrict with normal attack cost.");
                    if (!linked || !tailHit || !initialDamage || !paid) throw new InvalidOperationException("Natural grab establishment failed.");
                    int establishedAttacks = observer.WeaponAttacks, strengthBefore = owner.Stats.Strength.BaseValue;
                    owner.Stats.Strength.BaseValue = strengthBefore + 2; // prospective live-stat fixture change
                    int liveStrength = owner.Stats.Strength.Bonus;
                    JObject[] later = null;
                    deadline = DateTime.UtcNow.AddSeconds(35);
                    while (DateTime.UtcNow < deadline)
                    {
                        if (!nativeMode.Current) throw new InvalidOperationException("Grab native Default lease changed.");
                        later = riders.OfType<JObject>().Where(value => (int)value["roundHeld"] > 0)
                            .GroupBy(value => (int)value["roundHeld"]).Select(group => group.ToArray()).FirstOrDefault(group => group.Length >= 2);
                        if (later != null || !ReferenceEquals(SummonHoldComponent.HeldTarget(owner), target)) break;
                        if (turnBased) PollGrab32Turn(fixture, owner, target, anchor, nativeMode, nonparticipantPasses);
                        game.IsPaused = false; yield return 0;
                    }
                    bool maintained = later != null && later.Length == 2 && observer.WeaponAttacks == establishedAttacks &&
                        ReferenceEquals(SummonHoldComponent.HeldTarget(owner), target) &&
                        ReferenceEquals(SummonGrappleLinks.EstablishingWeapon(owner, target), tail) &&
                        (salamander ? later.Count(value => Grab32Damage(value, true, liveStrength)) == 1 &&
                            later.Count(value => Grab32TailDamage(value, tail.Blueprint.AssetGuid, liveStrength)) == 1 :
                            later.Count(value => (string)value["damage"]["weapon"] == tail.Blueprint.AssetGuid) == 1 &&
                            later.Count(value => Grab32Damage(value, false, liveStrength)) == 1);
                    row["strengthBefore"] = strengthBefore; row["prospectiveStrengthBase"] = owner.Stats.Strength.BaseValue;
                    row["liveStrengthBonus"] = liveStrength; row["establishedWeaponAttacks"] = establishedAttacks;
                    row["weaponAttacksAfterMaintain"] = observer.WeaponAttacks; row["laterRound"] = later == null ? null : new JArray(later.Select(value => value.DeepClone()));
                    row["nonparticipantPasses"] = nonparticipantPasses;
                    Sprint25Check(id + "-maintained", maintained, row,
                        "One later native buff round supplies establishing-tail damage plus one constrict, with live Strength and no extra weapon attack; Salamander preserves both one-fire bundles.");
                    if (!maintained) throw new InvalidOperationException("Natural later hold rider failed.");
                    int checksBefore = observer.Checks.Count, ridersBefore = riders.Count;
                    owner.Stats.AdditionalCMB.BaseValue = -100; // prospective low CMB, never an overridden roll
                    deadline = DateTime.UtcNow.AddSeconds(35);
                    while (ReferenceEquals(SummonHoldComponent.HeldTarget(owner), target) && DateTime.UtcNow < deadline)
                    {
                        if (!nativeMode.Current) throw new InvalidOperationException("Grab native Default lease changed.");
                        if (turnBased) PollGrab32Turn(fixture, owner, target, anchor, nativeMode, nonparticipantPasses);
                        game.IsPaused = false; yield return 0;
                    }
                    var releaseChecks = observer.Checks.Skip(checksBefore).Where(value => ReferenceEquals(value.Initiator, owner) &&
                        ReferenceEquals(value.Target, target)).ToArray();
                    bool released = releaseChecks.Any(value => !SummonManeuverChecks.Succeeded(value)) &&
                        owner.Get<UnitPartGrappleInitiator>() == null && target.Get<UnitPartGrappleTarget>() == null &&
                        SummonGrappleLinks.EstablishingWeapon(owner, target) == null &&
                        owner.Buffs.GetBuff(grab.HoldBuff) == null && target.Buffs.GetBuff(grab.GrappledBuff) == null &&
                        !owner.Descriptor.State.HasCondition(UnitCondition.CantAct) && !owner.Descriptor.State.HasCondition(UnitCondition.CantMove) &&
                        !target.Descriptor.State.HasCondition(UnitCondition.Entangled) && !target.Descriptor.State.HasCondition(UnitCondition.CantMove) &&
                        riders.Count == ridersBefore;
                    row["actualPostChangeManeuvers"] = new JArray(releaseChecks.Select(value => new JObject {
                        ["success"] = SummonManeuverChecks.Succeeded(value), ["target"] = value.Target.UniqueId }));
                    row["releasedBeforeFixtureCleanup"] = released; row["ridersBeforeRelease"] = ridersBefore; row["ridersAfterRelease"] = riders.Count;
                    Sprint25Check(id + "-released", released, row,
                        "Actual failed native maintain releases both parts, buffs, blocked actions and exact weapon link without another rider; no fixture release establishes this verdict.");
                    if (!released) throw new InvalidOperationException("Natural failed-maintain release failed.");
                    Sprint28SetBrainActivePreservingActions(owner, false); owner.Commands.InterruptAll(true);
                    // Restore only the owned fixture's prospective stat change.
                    owner.Stats.Strength.BaseValue = strengthBefore;
                    owner.Stats.AdditionalCMB.BaseValue = 100;
                    complete = true;
                }
            }
            finally
            {
                var errors = new List<string>();
                Action<string, Action> restore = (name, action) => { try { action(); } catch (Exception error) { errors.Add(name + ":" + error); } };
                restore("observers", () => { if (observer != null) EventBus.Unsubscribe(observer); if (starts != null) EventBus.Unsubscribe(starts); });
                foreach (var actor in new[] { owner, target, fixture.Caster, anchor }.Where(value => value != null && !value.Destroyed))
                    restore("owned-combat-" + actor.UniqueId, () => {
                        if (ReferenceEquals(actor, owner) || ReferenceEquals(actor, target))
                        { Sprint28SetBrainActivePreservingActions(actor, false); actor.Commands.InterruptAll(true); }
                        if (actor.IsInCombat) actor.LeaveCombat(); });
                restore("owned-hold", () => {
                    if (owner == null || owner.Destroyed) return;
                    var grab = SummonGrabComponent.Find(owner);
                    if (grab != null && ReferenceEquals(SummonHoldComponent.HeldTarget(owner), target)) SummonHoldComponent.ReleaseLink(owner, target, grab, false); });
                restore("owned-summon", () => { if (owner != null && !owner.Destroyed) DisposeExpandedSummoningUnits(fixture.Created, new[] { owner }); });
                restore("target-state", () => {
                    target.Stats.AC.BaseValue = oldAc; target.Stats.AdditionalCMB.BaseValue = oldTargetCmb;
                    if (immunity != null) {
                        target.Descriptor.RemoveFact(immunity);
                        foreach (var component in immunity.ComponentsArray) UnityEngine.Object.DestroyImmediate(component);
                        UnityEngine.Object.DestroyImmediate(immunity); }
                    ResetExpandedSummoningHostile(fixture); game.Player.UpdateIsInCombat(); });
                restore("native-default", () => { nativeMode?.Dispose();
                    if (Cleanup2NativeDefaultPending) throw new InvalidOperationException("Native Default restoration remains pending."); });
                if (anchorBrainCaptured) restore("owned-anchor-brain", () => {
                    if (anchor == null || anchor.Destroyed || !ReferenceEquals(anchor.Blueprint, _summoningProfileActor))
                        throw new InvalidOperationException("Owned anchor identity changed before restoration.");
                    Sprint28SetBrainActivePreservingActions(anchor, anchorBrainBefore);
                    if (anchor.IsBrainActive != anchorBrainBefore) throw new InvalidOperationException("Owned anchor brain not restored."); });
                restore("engagement-pause", () => SettingsRoot.Instance.PauseOnEngagement.CurrentValue = oldEngagementPause);
                restore("original-mode", () => {
                    if (Cleanup2NativeDefaultPending) throw new InvalidOperationException("Foreign mode preserved for outer rollback.");
                    SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode; game.TurnBasedCombatController.Activate();
                    game.IsPaused = oldPause; });
                Sprint25Check(id + "-finalizers", errors.Count == 0, new JObject { ["errors"] = new JArray(errors), ["complete"] = complete },
                    "Independent exact-owned finalizers run after success or refusal; cleanup is not gameplay proof.");
                if (errors.Count != 0 && complete) throw new InvalidOperationException(string.Join("|", errors.ToArray()));
            }
        }
        private UnitEntityData CastSprint32IdleFixtureSummon(ExpandedSummoningCorrectionFixture fixture, string key)
        {
            var caster = fixture.Caster;
            if (caster == null || !ReferenceEquals(caster.Blueprint, fixture.CasterBlueprint) ||
                !ReferenceEquals(caster.HoldingState, fixture.Scene) || fixture.UnitsBefore.Contains(caster) ||
                caster.IsBrainActive || !caster.Commands.Empty || caster.Commands.Queue.Count != 0)
                throw new InvalidOperationException("Exact owned idle setup caster required before summon.");
            var expected = ResolveExpandedSummoningExecution(fixture.Blueprints,
                ExpandedSummoningOwnTierVariant(key, SummonMultiplicity.One), null);
            var unit = CastExpandedSummoningOwnTier(fixture, key);
            // The synchronous setup helper completes its native command but leaves
            // the finished entry in UnitCommands. Only retire its proven own entry;
            // this is fixture setup, never evidence of a natural player cast.
            var remaining = caster.Commands.Raw.Where(command => command != null).ToArray();
            bool exact = !caster.IsBrainActive && caster.Commands.Queue.Count == 0 && remaining.All(command => command is UnitUseAbility &&
                ReferenceEquals(command.Executor, caster) && command.IsFinished &&
                command.Result == UnitCommand.ResultType.Success &&
                ReferenceEquals(((UnitUseAbility)command).Spell.Blueprint, expected));
            Sprint25Check("s32-idle-setup-" + unit.UniqueId, exact,
                new JObject { ["caster"] = caster.UniqueId, ["summon"] = unit.UniqueId,
                    ["execution"] = expected.AssetGuid, ["remainingCommands"] = remaining.Length, ["queuedCommands"] = caster.Commands.Queue.Count,
                    ["allExactFinishedSetupCommands"] = exact }, "Native setup command retirement is confined to the proven owned caster.");
            if (!exact) throw new InvalidOperationException("Unexpected unfinished/unowned setup command.");
            caster.Commands.RemoveFinishedAndUpdateQueue();
            if (!caster.Commands.Empty || caster.Commands.Queue.Count != 0 || caster.IsBrainActive)
                throw new InvalidOperationException("Native owned setup queue did not retire cleanly.");
            return unit;
        }
        private UnitEntityData Sprint32OwnedQuietAnchor(ExpandedSummoningCorrectionFixture fixture, string id)
        {
            var anchor = Game.Instance.Player.MainCharacter.Value;
            var party = Game.Instance.Player.Party.ToArray();
            var predicates = new JObject {
                ["actorPresent"] = anchor != null && _summoningProfileActor != null,
                ["actorBlueprintExact"] = anchor != null && ReferenceEquals(anchor.Blueprint, _summoningProfileActor),
                ["presetExact"] = _summoningProfilePreset != null && ReferenceEquals(_summoningProfilePreset.PlayerCharacter, _summoningProfileActor),
                ["originalDistinct"] = _summoningProfileOriginal != null && !ReferenceEquals(_summoningProfileOriginal.PlayerCharacter, _summoningProfileActor),
                ["actorNameExact"] = _summoningProfileActor?.name == "KMG_Runtime_Summoning_Profile_Anchor",
                ["saveGuardOwned"] = _summoningProfileSaveGuard != null, ["scenesOwned"] = _summoningProfileScenes != null,
                ["onlyPartyAnchor"] = party.Length == 1 && ReferenceEquals(party[0], anchor),
                ["anchorCommandsEmpty"] = anchor != null && anchor.Commands.Empty && anchor.Commands.Queue.Count == 0,
                ["casterCommandsEmpty"] = fixture.Caster.Commands.Empty && fixture.Caster.Commands.Queue.Count == 0,
                ["casterBrainDisabled"] = !fixture.Caster.IsBrainActive };
            bool exact = predicates.Properties().All(property => (bool)property.Value);
            Sprint25Check(id + "-quiet-anchor", exact, predicates, "Every exact ownership and idle predicate must hold before native Default admission.");
            if (!exact) throw new InvalidOperationException("Exact idle profile anchor/caster not proven.");
            return anchor;
        }
        private static bool PollGrab32Turn(ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, UnitEntityData target, UnitEntityData anchor, Cleanup2NativeDefaultScope nativeMode, JArray passed)
        {
            if (!nativeMode.Current) throw new InvalidOperationException("Grab native Default lease changed before turn polling.");
            var turn = Game.Instance.TurnBasedCombatController.CurrentTurn;
            if (turn == null) return false;
            bool acting = turn.Status == TurnController.TurnStatus.Preparing || turn.Status == TurnController.TurnStatus.Acting;
            if (ReferenceEquals(turn.Unit, owner)) return acting;
            if (ReferenceEquals(turn.Unit, target)) return false;
            // Only the isolated profile's idle caster/party anchor can be passed.
            // Neither tested actor is ended, made able to act, or given an action.
            if ((!ReferenceEquals(turn.Unit, fixture.Caster) && !ReferenceEquals(turn.Unit, anchor)) ||
                turn.Unit.IsBrainActive || !turn.Unit.Commands.Empty || turn.Unit.Commands.Queue.Count != 0)
                throw new InvalidOperationException("Unexpected nonparticipant in native grab turn order.");
            if (acting && turn.CanEndTurnAndNoActing())
            {
                passed.Add(new JObject { ["unit"] = turn.Unit.UniqueId, ["round"] = Game.Instance.TurnBasedCombatController.RoundNumber });
                EndSprint17SnakeTurn(turn);
            }
            return false;
        }
        private static bool Grab32Damage(JObject row, bool salamander, int strength)
        {
            var chunks = ((JArray)row["damage"]["chunks"]).OfType<JObject>().ToArray();
            return salamander ? (string)row["damage"]["weapon"] == SalamanderRulesPolicy.TailGuid && chunks.Length == 2 &&
                chunks.Count(value => (string)value["energy"] == "Fire" && (string)value["dice"] == "1d6" && (int)value["bonus"] == 0) == 1 &&
                chunks.Count(value => (string)value["type"] == "Physical" && (string)value["dice"] == "2d6" &&
                    (int)value["bonus"] == strength + Math.Max(0, strength) / 2) == 1 :
                (string)row["damage"]["weapon"] == null && chunks.Length == 1 &&
                (string)chunks[0]["type"] == "Physical" && (string)chunks[0]["dice"] == "2d6" && (int)chunks[0]["bonus"] == strength;
        }
        private static bool Grab32TailDamage(JObject row, string weapon, int strength)
        {
            var chunks = ((JArray)row["damage"]["chunks"]).OfType<JObject>().ToArray();
            return (string)row["damage"]["weapon"] == weapon && chunks.Length == 2 &&
                chunks.Count(value => (string)value["energy"] == "Fire" && (string)value["dice"] == "1d6" && (int)value["bonus"] == 0) == 1 &&
                chunks.Count(value => (string)value["type"] == "Physical" && (string)value["dice"] == "2d6" &&
                    (int)value["bonus"] == (strength > 0 ? strength / 2 : strength)) == 1;
        }
    }
}
