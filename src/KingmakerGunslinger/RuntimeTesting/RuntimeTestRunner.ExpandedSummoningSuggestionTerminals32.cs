using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
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
        // Terminal behavior only: supplied ordinary casts/attack and a disclosed
        // native 600s fixture summon. Natural AI/public duration have separate
        // evidence. No effect deadline, clock, roll or damage is supplied.
        private IEnumerable<int> ReviewSuggestionTerminals32(ExpandedSummoningCorrectionFixture fixture)
        {
            const string phase = ExpandedSummoningNativeProfilePolicy.Sprint32SuggestionTerminalsPhase;
            if (!IsSprint32Review || RuntimeTestRequestParser.Sprint32ReviewPhase(_request) != phase ||
                !Sprint32Suggestion.Settled || !Sprint32Suggestion.Active)
                throw new InvalidOperationException("Exact optional-provider terminal phase required.");
            var game = Game.Instance; var target = fixture.Hostile;
            UnitEntityData owner = null, second = null, anchor = null;
            bool anchorBrain = false, anchorCaptured = false;
            bool oldEngagement = SettingsRoot.Instance.PauseOnEngagement.CurrentValue;
            int oldWill = target.Stats.SaveWill.BaseValue;
            Cleanup2NativeDefaultScope mode = null;
            var effects = new Buff[2]; var deadlines = new double[2];
            var costs = new JArray(); var commands = new List<UnitUseAbility>();
            var observers = new List<Sprint16RuleObserver>();
            Astral31DispelEvents saves = null;
            try
            {
                anchor = Sprint32OwnedQuietAnchor(fixture, phase);
                anchorBrain = anchor.IsBrainActive; anchorCaptured = true;
                Sprint28SetBrainActivePreservingActions(anchor, false);
                var blueprint = fixture.Blueprints.OfType<BlueprintUnit>().Single(value => value.name == LillendTailAnimationPolicy.UnitName);
                var execution = ResolveExpandedSummoningExecution(fixture.Blueprints,
                    ExpandedSummoningOwnTierVariant("lillend-azata", SummonMultiplicity.One), null);
                var summonData = new AbilityData(execution, fixture.Caster.Descriptor);
                var summonContext = new AbilityExecutionContext(summonData, summonData.CalculateParams(),
                    new TargetWrapper(fixture.Caster.Position), Rulebook.CurrentContext);
                var control = new Sprint16ManualSummonControl { Caster = fixture.Caster, Blueprint = blueprint };
                EventBus.Subscribe(control);
                try
                {
                    var rule = new RuleSummonUnit(fixture.Caster, blueprint, fixture.Caster.Position, 100.Rounds(), 0)
                        { Context = summonContext, Reason = summonContext };
                    Rulebook.Trigger(rule); owner = rule.SummonedUnit;
                    if (owner == null) throw new InvalidOperationException("Native long-lived fixture summon failed.");
                    fixture.Created.Add(owner); game.EntityCreator.Tick();
                }
                finally { EventBus.Unsubscribe(control); }
                if (!ReferenceEquals(owner.Blueprint, blueprint) || owner.Descriptor.Progression.CharacterLevel != 7)
                    throw new InvalidOperationException("Fixture changed the registered adult identity/HD.");
                second = game.EntityCreator.SpawnUnit(fixture.HostileBlueprint, target.Position + Vector3.right * 5f,
                    Quaternion.identity, fixture.Scene);
                fixture.Created.Add(second); game.EntityCreator.Tick();
                var targets = new[] { target, second };
                foreach (var unit in targets.Concat(new[] { owner }))
                {
                    Sprint28SetBrainActivePreservingActions(unit, false);
                    unit.Commands.InterruptAll(true); unit.Commands.RemoveFinishedAndUpdateQueue();
                    unit.Wake(0f); if (!game.State.AwakeUnits.Contains(unit)) game.State.AwakeUnits.Add(unit);
                }
                foreach (var unit in targets) { unit.Stats.SaveWill.BaseValue = -100; unit.Stats.HitPoints.BaseValue = 100000; }
                var marker = owner.Buffs.Enumerable.Single(value => ReferenceEquals(value.Blueprint, BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff));
                double sourceDeadline = marker.EndTime.TotalSeconds;
                var ability = fixture.Blueprints.OfType<BlueprintAbility>().Single(value => value.name == "KMG_Summoning_FeyCelestial_LillendAzata_Suggestion");
                var book = owner.Descriptor.GetSpellbook(owner.Blueprint.GetComponent<LillendBardBook32>().Book);
                if (book.GetSpontaneousSlots(2) != 4 || sourceDeadline - game.Player.GameTime.TotalSeconds < 595)
                    throw new InvalidOperationException("Fresh book or explicit native 600s lifetime differs.");
                saves = new Astral31DispelEvents { Owner = owner };
                foreach (var unit in targets) saves.Targets.Add(unit);
                EventBus.Subscribe(saves);
                foreach (var unit in targets)
                {
                    var observer = new Sprint16RuleObserver { Owner = owner, Target = unit };
                    observers.Add(observer); EventBus.Subscribe(observer);
                }
                SettingsRoot.Instance.PauseOnEngagement.CurrentValue = false;
                mode = Cleanup2EnterNativeDefault(fixture, phase);
                using (var economy = new FirearmReviewCosts(owner, costs))
                {
                    for (int index = 0; index < targets.Length; index++)
                    {
                        var recipient = targets[index]; Vector3 far; string lane, direction;
                        var origin = Sprint24AiCombatLane(fixture.Caster, owner, recipient, 8f, out far, out lane, out direction);
                        PlaceExpandedSummoningUnit(owner, origin); PlaceExpandedSummoningUnit(recipient, far);
                        owner.Memory.Add(recipient); recipient.Memory.Add(owner); owner.JoinCombat(); recipient.JoinCombat(); game.Player.UpdateIsInCombat();
                        DateTime readyLimit = DateTime.UtcNow.AddSeconds(15);
                        while (DateTime.UtcNow < readyLimit && (!owner.HasLOS(recipient) || !owner.Memory.ContainsVisible(recipient) ||
                            !EntityFadedIn(owner) || !owner.Descriptor.State.CanAct || owner.Buffs.GetBuff(BlueprintRoot.Instance.SystemMechanics.SummonedUnitAppearBuff) != null))
                        { if (!mode.Current) throw new InvalidOperationException("Terminal lease changed during readiness."); game.IsPaused = false; yield return 0; }
                        if (!owner.HasLOS(recipient) || !owner.Memory.ContainsVisible(recipient) || !owner.Descriptor.State.CanAct || !owner.IsEnemy(recipient))
                            throw new InvalidOperationException("Ordinary Suggestion recipient is not visible/eligible.");
                        // At most two genuine attempts per recipient and four
                        // total native L2 slots. An actual save can still fail this case.
                        for (int attempt = 0; attempt < 2 && effects[index] == null; attempt++)
                        {
                            var data = new AbilityData(ability, book); var parameters = data.CalculateParams();
                            if (!data.IsAvailable || !data.CanTarget(new TargetWrapper(recipient)) || parameters.CasterLevel != 7 || parameters.DC != 16)
                                throw new InvalidOperationException("Native private Suggestion cast precondition differs.");
                            var cast = new UnitUseAbility(data, new TargetWrapper(recipient)); commands.Add(cast); owner.Commands.Run(cast);
                            var limit = DateTime.UtcNow.AddSeconds(25);
                            while (DateTime.UtcNow < limit && (!cast.IsFinished || cast.ExecutionProcess == null || !cast.ExecutionProcess.IsEnded))
                            { if (!mode.Current) throw new InvalidOperationException("Terminal cast lease changed."); game.IsPaused = false; yield return 0; }
                            if (!cast.IsFinished || cast.Result != UnitCommand.ResultType.Success || cast.ExecutionProcess == null || !cast.ExecutionProcess.IsEnded ||
                                cast.Type != UnitCommand.CommandType.Standard || cast.Cutscene || cast.IsIgnoreCooldown || economy.Count(cast) != 1 || !economy.Exact(cast, false) ||
                                book.GetSpontaneousSlots(2) != 4 - commands.Count)
                                throw new InvalidOperationException("Ordinary Suggestion command/cost/slot did not resolve exactly.");
                            effects[index] = recipient.Buffs.Enumerable.SingleOrDefault(value => value.Blueprint.AssetGuid == Sprint32Suggestion.OwnedBuffGuid &&
                                ReferenceEquals(value.Context?.MaybeCaster, owner));
                        }
                        var effect = effects[index];
                        if (effect == null || !effect.Active || effect.IsDisposed || effect.Context.Params.CasterLevel != 7 ||
                            effect.TimeLeft.TotalSeconds < 412 || effect.TimeLeft.TotalSeconds > 420.1 || !recipient.Descriptor.State.HasCondition(UnitCondition.Dazed))
                            throw new InvalidOperationException("Native unshortened Suggestion effect absent or duration differs.");
                        deadlines[index] = effect.EndTime.TotalSeconds;
                        if (sourceDeadline <= deadlines[index] + 12) throw new InvalidOperationException("Fixture source does not outlive original effect deadline.");
                    }
                    Sprint25Check("suggestion-terminals-cast", true, new JObject { ["source"] = owner.UniqueId,
                        ["targets"] = new JArray(targets.Select(value => value.UniqueId)), ["commands"] = new JArray(commands.Select(Sprint27AiCommand)),
                        ["costs"] = costs.DeepClone(), ["sourceDeadline"] = sourceDeadline, ["effectDeadlines"] = new JArray(deadlines),
                        ["fixtureSourceSeconds"] = 600, ["fixtureCastCommands"] = commands.Count, ["remainingL2Slots"] = book.GetSpontaneousSlots(2),
                        ["actualSaves"] = saves.Saves.Count }, "Ordinary real-book casts establish two unshortened CL7 effects; fixture source duration is explicitly600s, with no supplied roll/effect.");

                    var weaponDamage = new HashSet<RuleDealDamage>();
                    observers[0].ObserveWeaponContact = rule => { if (ReferenceEquals(rule.Target, target) && rule.MeleeDamage != null) weaponDamage.Add(rule.MeleeDamage); };
                    Func<RuleDealDamage, bool> exactDamage = value => !value.IsFake && value.Damage > 0 && ReferenceEquals(value.Initiator, owner) &&
                        ReferenceEquals(value.Target, target) && weaponDamage.Contains(value);
                    int oldAttack = owner.Stats.AdditionalAttackBonus.BaseValue;
                    var attacks = new List<UnitAttack>();
                    int damageBefore = observers[0].Damage.Count;
                    try
                    {
                        owner.Stats.AdditionalAttackBonus.BaseValue = 100;
                        Vector3 contact; string lane, direction;
                        var origin = Sprint24AiCombatLane(fixture.Caster, owner, target, 1.4f, out contact, out lane, out direction);
                        PlaceExpandedSummoningUnit(owner, origin); PlaceExpandedSummoningUnit(target, contact);
                        var plan = new UnitAttack(target); plan.Init(owner); var hands = plan.CreateFullAttack();
                        if (!hands.Any() || hands.Any(value => value.Hand.MaybeWeapon == null) || owner.DistanceTo(target) >= hands.Min(value => value.Hand.MaybeWeapon.AttackRange.Meters))
                            throw new InvalidOperationException("Actual attack reach differs from damage probe.");
                        if (damageBefore != 0 || !effects[0].Active || effects[0].IsDisposed || !target.Descriptor.State.HasCondition(UnitCondition.Dazed) ||
                            owner.Destroyed || owner.Descriptor.State.IsDead || game.Player.GameTime.TotalSeconds >= deadlines[0])
                            throw new InvalidOperationException("Original live undamaged effect not present immediately before damage probe.");
                        for (int attempt = 0; attempt < 3 && !observers[0].Damage.Any(exactDamage); attempt++)
                        {
                            var attack = new UnitAttack(target); attacks.Add(attack); owner.Commands.Run(attack);
                            var limit = DateTime.UtcNow.AddSeconds(25);
                            while (DateTime.UtcNow < limit && !attack.IsFinished)
                            { if (!mode.Current) throw new InvalidOperationException("Damage probe lost native lease."); game.IsPaused = false; yield return 0; }
                            if (!attack.IsFinished) throw new InvalidOperationException("Ordinary damage probe did not complete.");
                        }
                    }
                    finally { owner.Stats.AdditionalAttackBonus.BaseValue = oldAttack; }
                    var firstDamage = observers[0].Damage.Skip(damageBefore).FirstOrDefault();
                    bool broken = firstDamage != null && exactDamage(firstDamage) && effects[0].IsDisposed && !target.Descriptor.State.HasCondition(UnitCondition.Dazed) &&
                        !owner.Destroyed && !owner.Descriptor.State.IsDead && game.Player.GameTime.TotalSeconds < deadlines[0] &&
                        effects[1].Active && !effects[1].IsDisposed && second.Descriptor.State.HasCondition(UnitCondition.Dazed);
                    Sprint25Check("suggestion-terminals-damage", broken, new JObject { ["damage"] = new JArray(observers[0].Damage.Where(exactDamage).Select(Sprint16DamageEvent)),
                        ["attacks"] = new JArray(attacks.Select(Sprint27AiCommand)), ["originalEnd"] = deadlines[0], ["observedSeconds"] = game.Player.GameTime.TotalSeconds,
                        ["firstSubsequentDamage"] = firstDamage == null ? null : Sprint16DamageEvent(firstDamage), ["damageBeforeProbe"] = damageBefore,
                        ["exactEffectDisposed"] = effects[0].IsDisposed, ["untouchedSiblingEffectActive"] = effects[1].Active, ["fixtureAttackBonus"] = 100 },
                        "Exact positive nonfake native weapon damage breaks only the damaged subject's original Suggestion before its deadline while source survives.");
                    if (!broken) throw new InvalidOperationException("Actual damage terminal or independent effect failed.");

                    double lastPresent = game.Player.GameTime.TotalSeconds, firstAbsent = -1, previousSample = lastPresent, maxStep = 0;
                    var waitLimit = DateTime.UtcNow.AddSeconds(450);
                    while (DateTime.UtcNow < waitLimit && game.Player.GameTime.TotalSeconds <= deadlines[1] + 8)
                    {
                        maxStep = Math.Max(maxStep, game.Player.GameTime.TotalSeconds - previousSample);
                        previousSample = game.Player.GameTime.TotalSeconds;
                        if (!mode.Current || !Sprint32Suggestion.Active || owner.Destroyed || owner.Descriptor.State.IsDead || second.Destroyed || second.Descriptor.State.IsDead ||
                            marker.IsDisposed || marker.EndTime.TotalSeconds != sourceDeadline || observers[1].Damage.Count != 0)
                            throw new InvalidOperationException("Suggestion expiry lost source, provider, lease or undamaged recipient.");
                        if (effects[1].IsDisposed || !effects[1].Active)
                        { firstAbsent = game.Player.GameTime.TotalSeconds; break; }
                        if (effects[1].EndTime.TotalSeconds != deadlines[1] || !second.Descriptor.State.HasCondition(UnitCondition.Dazed))
                            throw new InvalidOperationException("Original Suggestion deadline/condition changed before expiry.");
                        lastPresent = game.Player.GameTime.TotalSeconds;
                        game.IsPaused = false; yield return 0;
                    }
                    bool expired = firstAbsent >= deadlines[1] && firstAbsent <= deadlines[1] + 1 && lastPresent >= deadlines[1] - 1 &&
                        lastPresent <= deadlines[1] + 1 && firstAbsent - lastPresent <= 1 &&
                        effects[1].IsDisposed && !second.Descriptor.State.HasCondition(UnitCondition.Dazed) && book.GetSpontaneousSlots(2) == 4 - commands.Count;
                    Sprint25Check("suggestion-terminals-expiry", expired, new JObject { ["originalEnd"] = deadlines[1], ["lastPresent"] = lastPresent,
                        ["firstAbsent"] = firstAbsent, ["sourceEnd"] = sourceDeadline, ["sourceAlive"] = !owner.Destroyed && !owner.Descriptor.State.IsDead,
                        ["terminalSampleStep"] = firstAbsent - lastPresent, ["largestObservedGameStep"] = maxStep, ["safetyTimeoutSecondsAfterEnd"] = 8,
                        ["recipientDamageEvents"] = observers[1].Damage.Count, ["nativeMode"] = mode.Describe(), ["effectDeadlineWrites"] = 0, ["manualTicks"] = 0 },
                        "Original420s effect persists until its native deadline, then expires under native Default with living source and undamaged recipient; no shortened deadline or manual clock.");
                    if (!expired) throw new InvalidOperationException("Unshortened Suggestion expiry not established.");
                }
            }
            finally
            {
                if (saves != null) EventBus.Unsubscribe(saves);
                foreach (var observer in observers) EventBus.Unsubscribe(observer);
                try
                {
                    foreach (var unit in new[] { owner, second, target }.Where(value => value != null && !value.Destroyed))
                    { Sprint28SetBrainActivePreservingActions(unit, false); unit.Commands.InterruptAll(true); if (unit.IsInCombat) unit.LeaveCombat(); }
                    foreach (var effect in effects.Where(value => value != null && !value.IsDisposed)) effect.Remove();
                    target.Stats.SaveWill.BaseValue = oldWill;
                    DisposeExpandedSummoningUnits(fixture.Created, new[] { owner, second }.Where(value => value != null && !value.Destroyed).ToArray());
                    ResetExpandedSummoningHostile(fixture); game.Player.UpdateIsInCombat();
                }
                finally
                {
                    try { mode?.Dispose(); }
                    finally
                    {
                        SettingsRoot.Instance.PauseOnEngagement.CurrentValue = oldEngagement;
                        if (anchorCaptured && !Cleanup2NativeDefaultPending)
                        {
                            if (anchor == null || anchor.Destroyed || !ReferenceEquals(anchor.Blueprint, _summoningProfileActor))
                                throw new InvalidOperationException("Suggestion terminal anchor identity changed.");
                            Sprint28SetBrainActivePreservingActions(anchor, anchorBrain);
                        }
                    }
                }
            }
        }
    }
}
