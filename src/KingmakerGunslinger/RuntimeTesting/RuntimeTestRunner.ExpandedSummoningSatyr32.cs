using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Brain;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
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
        private bool IsSprint32Review { get { return _request.Scenario ==
            RuntimeTestScenarioCatalog.DisposableExpandedSummoningSprint32NativeReview; } }

        private IEnumerable<int> ReviewSprint32(ExpandedSummoningCorrectionFixture fixture)
        {
            // Two resettable RTWP cases, each with a fresh exact-owner summon.
            // Final mode/profile/persistence qualification remains separate.
            CreateExpandedSummoningCorrectionHostile(fixture);
            foreach (bool immune in new[] { false, true })
                foreach (int frame in ReviewSatyr32Control(fixture, immune)) yield return frame;
        }

        private IEnumerable<int> ReviewSatyr32Control(ExpandedSummoningCorrectionFixture fixture, bool immune)
        {
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool oldPause = Game.Instance.IsPaused;
            UnitEntityData owner = null;
            var target = fixture.Hostile;
            int will = target.Stats.SaveWill.BaseValue, ac = target.Stats.AC.BaseValue;
            bool addedImmunity = false;
            string id = immune ? "satyr-immune-fallback" : "satyr-control";
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;
                owner = CastExpandedSummoningOwnTier(fixture, "satyr");
                Sprint28SetBrainActivePreservingActions(owner, false);
                owner.Commands.InterruptAll(true); owner.Commands.RemoveFinishedAndUpdateQueue();
                for (int frame = 0; frame < 8; frame++) yield return 0;
                var fear = fixture.Blueprints.OfType<BlueprintAbility>().Single(value =>
                    value.name == "KMG_Summoning_FeyCelestial_Satyr_Fear");
                var resource = fixture.Blueprints.OfType<BlueprintAbilityResource>().Single(value =>
                    value.name == "KMG_Summoning_FeyCelestial_Satyr_FearResource");
                var eligibility = fixture.Blueprints.OfType<SatyrControlEligibility>().Single(value =>
                    ReferenceEquals(value.OwningBlueprint, owner.Blueprint) && ReferenceEquals(value.Ability, fear));
                Vector3 far; string lane, direction;
                var origin = Sprint24AiCombatLane(fixture.Caster, owner, target, 8f,
                    out far, out lane, out direction,
                    originAllyExclusionRadius: new Feet(30).Meters + 1f);
                PlaceExpandedSummoningUnit(owner, origin); PlaceExpandedSummoningUnit(target, far);
                bool protectedClear = Game.Instance.Player.Party.Where(value => value != null && value.IsInGame)
                    .All(value => Vector3.Distance(value.Position, origin) > new Feet(30).Meters + value.Corpulence);
                if (!protectedClear) throw new InvalidOperationException("Satyr cone overlaps protected party footprint.");
                target.Stats.SaveWill.BaseValue = -100; target.Stats.AC.BaseValue = -100;
                if (immune)
                {
                    target.Descriptor.State.AddConditionImmunity(UnitCondition.Frightened);
                    addedImmunity = true;
                }
                foreach (var unit in new[] { owner, target })
                {
                    unit.Wake(0f);
                    if (!Game.Instance.State.AwakeUnits.Contains(unit)) Game.Instance.State.AwakeUnits.Add(unit);
                }
                owner.Memory.Add(target); target.Memory.Add(owner);
                owner.SightCache.ClearIfNeeded(); target.SightCache.ClearIfNeeded();
                new Kingmaker.Controllers.Units.UnitMemoryController().Tick();
                owner.JoinCombat(); target.JoinCombat(); Game.Instance.Player.UpdateIsInCombat();
                new UnitCombatJoinController().Tick(); new UnitCombatPrepareController().Tick();
                Sprint28SetBrainActivePreservingActions(owner, true); owner.Brain.RestoreAvailableActions();
                float score = eligibility.Score(new DecisionContext { Unit = owner, Target = new TargetInfo { Unit = target } });
                var weapon = owner.Body.PrimaryHand.MaybeWeapon;
                bool ready = protectedClear && owner.IsAIEnabled && owner.Descriptor.State.CanAct &&
                    owner.IsInCombat && owner.IsEnemy(target) && owner.HasLOS(target) &&
                    owner.Memory.ContainsVisible(target) && score == (immune ? 0f : 1f) &&
                    owner.Descriptor.Resources.GetResourceAmount(resource) == 1 &&
                    weapon?.Blueprint.AssetGuid == "6d40d84e239bdf345b349ff52e3c00a9" &&
                    owner.DistanceTo(target) <= new Feet(30).Meters;
                Sprint25Check(id + "-ready", ready, new JObject {
                    ["owner"] = owner.UniqueId, ["target"] = target.UniqueId, ["lane"] = lane,
                    ["visible"] = owner.Memory.ContainsVisible(target), ["los"] = owner.HasLOS(target),
                    ["canAct"] = owner.Descriptor.State.CanAct, ["aiEnabled"] = owner.IsAIEnabled,
                    ["distanceMeters"] = owner.DistanceTo(target), ["weaponRangeFeet"] = weapon?.AttackRange.Value,
                    ["eligibility"] = score, ["targetFearImmune"] = target.Descriptor.State.HasConditionImmunity(UnitCondition.Frightened),
                    ["fearUses"] = owner.Descriptor.Resources.GetResourceAmount(resource),
                    ["fixtureWillBase"] = -100, ["fixtureAcBase"] = -100,
                    ["fixtureResourceDebits"] = 0, ["protectedPartyOutside"] = protectedClear },
                    "Visible exact-owner archer, target inside its native Fear range, real unspent resource, and explicit eligibility before native AI ticks.");
                if (!ready) throw new InvalidOperationException("Satyr control fixture preconditions unmet: " + id);
                var commands = new HashSet<UnitCommand>(); var costs = new JArray(); var ambient = new HashSet<string>();
                var saves = new Dretch30CloudSaves(new[] { target });
                var attacks = new Sprint24AttackObserver { Owner = owner, Target = target };
                EventBus.Subscribe(saves); EventBus.Subscribe(attacks);
                try
                {
                    using (var economy = new FirearmReviewCosts(owner, costs))
                    {
                        DateTime deadline = DateTime.UtcNow.AddSeconds(40); int frames = 0, ticks = 0;
                        while (DateTime.UtcNow < deadline && frames++ < 4000)
                        {
                            if (frames == 1 || frames % 60 == 0 && !owner.Commands.Raw.Any(value => value != null && !value.IsFinished))
                            {
                                foreach (var candidate in Sprint24TickExactAi(owner)) if (candidate != owner.UniqueId) ambient.Add(candidate);
                                ticks++;
                            }
                            foreach (var command in owner.Commands.Raw.Where(value => value != null)) commands.Add(command);
                            if (immune ? attacks.WeaponAttacks.Count > 0 && commands.OfType<UnitAttack>().Any(value => value.IsFinished) :
                                commands.OfType<UnitUseAbility>().Any(value => value.IsFinished && value.ExecutionProcess != null && value.ExecutionProcess.IsEnded)) break;
                            Game.Instance.IsPaused = false; yield return 0;
                        }
                        var cast = commands.OfType<UnitUseAbility>().FirstOrDefault(value =>
                            value.AiAction?.name == "KMG_Summoning_FeyCelestial_Satyr_FearAi" &&
                            value.IsFinished && value.Result == UnitCommand.ResultType.Success);
                        var attack = commands.OfType<UnitAttack>().FirstOrDefault(value =>
                            value.AiAction != null && value.IsFinished && value.Result == UnitCommand.ResultType.Success);
                        UnitCommand resolved = immune ? (UnitCommand)attack : cast;
                        bool outcome = immune ? cast == null && attacks.WeaponAttacks.Count > 0 &&
                            owner.Descriptor.Resources.GetResourceAmount(resource) == 1 :
                            cast != null && target.Descriptor.State.HasCondition(UnitCondition.Frightened) &&
                            owner.Descriptor.Resources.GetResourceAmount(resource) == 0 &&
                            saves.Rows.Any(value => (int)value["dc"] == 18 && (string)value["caster"] == owner.UniqueId &&
                                (string)value["sourceAbility"] == fear.AssetGuid);
                        bool exact = ticks > 0 && resolved != null && economy.Count(resolved) == 1 &&
                            economy.Exact(resolved, false) && !resolved.Cutscene && !resolved.IsIgnoreCooldown && outcome;
                        Sprint25Check(id + "-resolved", exact, new JObject {
                            ["owner"] = owner.UniqueId, ["target"] = target.UniqueId, ["frames"] = frames, ["aiTicks"] = ticks,
                            ["commands"] = new JArray(commands.Select(Sprint27AiCommand)), ["costs"] = costs,
                            ["saves"] = saves.Rows, ["weaponAttackEvents"] = attacks.WeaponAttacks.Count,
                            ["targetFrightened"] = target.Descriptor.State.HasCondition(UnitCondition.Frightened),
                            ["fearUses"] = owner.Descriptor.Resources.GetResourceAmount(resource),
                            ["nativeBuffs"] = new JArray(target.Descriptor.Buffs.RawFacts.OfType<Buff>()
                                .Where(value => ReferenceEquals(value.Context?.MaybeCaster, owner))
                                .Select(value => new JObject { ["guid"] = value.Blueprint.AssetGuid,
                                    ["cl"] = value.Context.Params.CasterLevel, ["dc"] = value.Context.Params.DC })),
                            ["ambientCandidatesNeverTicked"] = new JArray(ambient), ["fixtureCastCommands"] = 0,
                            ["fixtureAttackCommands"] = 0, ["forcedRolls"] = 0, ["fixtureResourceDebits"] = 0 },
                            immune ? "Native AI rejects an explicitly fear-immune hostile and fires its real bow without spending Fear." :
                                "Native AI casts Fear at DC18, spends one owned resource and a normal action, and applies native Frightened.");
                        if (!exact) throw new InvalidOperationException("Satyr native AI observation failed: " + id);
                    }
                }
                finally { EventBus.Unsubscribe(saves); EventBus.Unsubscribe(attacks); }
            }
            finally
            {
                if (owner != null) { Sprint28SetBrainActivePreservingActions(owner, false); owner.Commands.InterruptAll(true); }
                target.Commands.InterruptAll(true);
                foreach (var buff in target.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(value =>
                    owner != null && ReferenceEquals(value.Context?.MaybeCaster, owner)).ToArray()) target.Descriptor.Buffs.RemoveFact(buff);
                if (addedImmunity) target.Descriptor.State.RemoveConditionImmunity(UnitCondition.Frightened);
                target.Stats.SaveWill.BaseValue = will; target.Stats.AC.BaseValue = ac;
                DisposeExpandedSummoningUnits(fixture.Created, new[] { owner }.Where(value => value != null && !value.Destroyed).ToArray());
                ResetExpandedSummoningHostile(fixture);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode;
                Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = oldPause;
            }
        }
    }
}
