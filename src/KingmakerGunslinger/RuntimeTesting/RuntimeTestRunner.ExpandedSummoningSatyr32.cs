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
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
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
            CaptureSprint32ControlGraphs();
            if (RuntimeTestRequestParser.Sprint32ReviewPhase(_request) == ExpandedSummoningNativeProfilePolicy.Sprint32LillendSupportPhase)
            {
                foreach (int frame in ReviewLillend32Support(fixture)) yield return frame;
                yield break;
            }
            // Resettable RTWP cases, each with a fresh exact-owner summon.
            // Final mode/profile/persistence qualification remains separate.
            CreateExpandedSummoningCorrectionHostile(fixture);
            foreach (bool immune in new[] { false, true })
                foreach (int frame in ReviewSatyr32Control(fixture, immune)) yield return frame;
            foreach (int frame in ReviewSatyr32Control(fixture, true, true)) yield return frame;
        }

        private IEnumerable<int> ReviewSatyr32Control(ExpandedSummoningCorrectionFixture fixture, bool immune, bool melee = false)
        {
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool oldPause = Game.Instance.IsPaused;
            UnitEntityData owner = null;
            var target = fixture.Hostile;
            int will = target.Stats.SaveWill.BaseValue, ac = target.Stats.AC.BaseValue;
            BlueprintFeature mindImmunity = null;
            string id = melee ? "satyr-melee-fallback" : immune ? "satyr-immune-fallback" : "satyr-control";
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
                var origin = Sprint24AiCombatLane(fixture.Caster, owner, target, melee ? 1.4f : 8f,
                    out far, out lane, out direction,
                    originAllyExclusionRadius: new Feet(30).Meters + 1f);
                PlaceExpandedSummoningUnit(owner, origin); PlaceExpandedSummoningUnit(target, far);
                bool protectedClear = Game.Instance.Player.Party.Where(value => value != null && value.IsInGame)
                    .All(value => Vector3.Distance(value.Position, origin) > new Feet(30).Meters + value.Corpulence);
                if (!protectedClear) throw new InvalidOperationException("Satyr cone overlaps protected party footprint.");
                target.Stats.SaveWill.BaseValue = -100; target.Stats.AC.BaseValue = -100;
                if (immune)
                {
                    var component = ScriptableObject.CreateInstance<BuffDescriptorImmunity>();
                    component.Descriptor = SpellDescriptor.MindAffecting;
                    mindImmunity = ScriptableObject.CreateInstance<BlueprintFeature>();
                    mindImmunity.name = "KMG_Runtime_Satyr32_MindImmunity";
                    mindImmunity.Ranks = 1; mindImmunity.ComponentsArray = new BlueprintComponent[] { component };
                    target.Descriptor.AddFact(mindImmunity);
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
                var controlScores = fixture.Blueprints.OfType<SatyrControlEligibility>()
                    .Where(value => ReferenceEquals(value.OwningBlueprint, owner.Blueprint))
                    .ToDictionary(value => value.Ability.name, value => value.Score(
                        new DecisionContext { Unit = owner, Target = new TargetInfo { Unit = target } }));
                var weapon = owner.Body.PrimaryHand.MaybeWeapon;
                bool ready = protectedClear && owner.IsAIEnabled && owner.Descriptor.State.CanAct &&
                    owner.IsInCombat && owner.IsEnemy(target) && owner.HasLOS(target) &&
                    owner.Memory.ContainsVisible(target) && score == (immune ? 0f : 1f) &&
                    (!immune || controlScores.Values.All(value => value == 0f)) &&
                    owner.Descriptor.Resources.GetResourceAmount(resource) == 1 &&
                    weapon?.Blueprint.AssetGuid == "6d40d84e239bdf345b349ff52e3c00a9" &&
                    owner.DistanceTo(target) <= new Feet(30).Meters;
                Sprint25Check(id + "-ready", ready, new JObject {
                    ["owner"] = owner.UniqueId, ["target"] = target.UniqueId, ["lane"] = lane,
                    ["visible"] = owner.Memory.ContainsVisible(target), ["los"] = owner.HasLOS(target),
                    ["canAct"] = owner.Descriptor.State.CanAct, ["aiEnabled"] = owner.IsAIEnabled,
                    ["distanceMeters"] = owner.DistanceTo(target), ["weaponRangeFeet"] = weapon?.AttackRange.Value,
                    ["eligibility"] = score, ["targetFearImmune"] = target.Descriptor.State.HasConditionImmunity(UnitCondition.Frightened),
                    // Native JSON defaults encode dictionaries as arrays. Build
                    // this receipt object explicitly; no default converter.
                    ["controlEligibility"] = new JObject(controlScores.Select(pair =>
                        new JProperty(pair.Key, pair.Value))), ["targetMindImmune"] = mindImmunity != null,
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
                        var change = commands.OfType<UnitSwitchHandEquipmentSet>().FirstOrDefault(value =>
                            value.AiAction?.name == "KMG_Summoning_FeyCelestial_Satyr_MeleeAi" &&
                            value.IsFinished && value.Result == UnitCommand.ResultType.Success);
                        var planQuery = new UnitAttack(target); planQuery.Init(owner);
                        var plan = planQuery.CreateFullAttack().Select(value => value.Hand.MaybeWeapon.Blueprint).ToArray();
                        bool weaponPlan = melee ? plan.Length == 2 &&
                            plan[0].name == "KMG_Summoning_FeyCelestial_Satyr_Dagger" &&
                            plan[1].name == "KMG_Summoning_FeyCelestial_Satyr_Horns" :
                            plan.Length == 1 && plan[0].AssetGuid == "6d40d84e239bdf345b349ff52e3c00a9";
                        UnitCommand resolved = immune ? (UnitCommand)attack : cast;
                        bool outcome = immune ? cast == null && attacks.WeaponAttacks.Count > 0 &&
                            owner.Descriptor.Resources.GetResourceAmount(resource) == 1 && weaponPlan &&
                            (melee ? change != null && economy.Count(change) == 1 && economy.Exact(change, false) &&
                                attacks.WeaponAttacks.Any(value => value.Weapon.Blueprint.name == "KMG_Summoning_FeyCelestial_Satyr_Dagger") :
                                attacks.WeaponAttacks.All(value => value.Weapon.Blueprint.AssetGuid == "6d40d84e239bdf345b349ff52e3c00a9")) :
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
                            ["weaponEvents"] = new JArray(attacks.WeaponAttacks.Select(value => value.Weapon.Blueprint.name)),
                            ["queriedFullAttackPlan"] = new JArray(plan.Select(value => value.name)),
                            ["planQueryWasNotIssued"] = true,
                            ["targetFrightened"] = target.Descriptor.State.HasCondition(UnitCondition.Frightened),
                            ["fearUses"] = owner.Descriptor.Resources.GetResourceAmount(resource),
                            ["nativeBuffs"] = new JArray(target.Descriptor.Buffs.RawFacts.OfType<Buff>()
                                .Where(value => ReferenceEquals(value.Context?.MaybeCaster, owner))
                                .Select(value => new JObject { ["guid"] = value.Blueprint.AssetGuid,
                                    ["cl"] = value.Context.Params.CasterLevel, ["dc"] = value.Context.Params.DC })),
                            ["ambientCandidatesNeverTicked"] = new JArray(ambient), ["fixtureCastCommands"] = 0,
                            ["fixtureAttackCommands"] = 0, ["forcedRolls"] = 0, ["fixtureResourceDebits"] = 0 },
                            melee ? "Native AI pays for its hand-set switch and attacks with its dagger; the queried melee full-attack plan also contains its secondary horns. This does not alone prove a resolved two-hit full attack." :
                            immune ? "Native AI rejects all control against a mind-affecting-immune hostile and fires its real bow without spending Fear; the queried ranged full-attack plan contains no out-of-reach horns." :
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
                if (mindImmunity != null)
                {
                    target.Descriptor.RemoveFact(mindImmunity);
                    foreach (var component in mindImmunity.ComponentsArray) UnityEngine.Object.DestroyImmediate(component);
                    UnityEngine.Object.DestroyImmediate(mindImmunity);
                }
                target.Stats.SaveWill.BaseValue = will; target.Stats.AC.BaseValue = ac;
                DisposeExpandedSummoningUnits(fixture.Created, new[] { owner }.Where(value => value != null && !value.Destroyed).ToArray());
                ResetExpandedSummoningHostile(fixture);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode;
                Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = oldPause;
            }
        }
    }
}
