using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Brain;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> Trumpet31ObserveBlastDecision(UnitEntityData owner,
            UnitEntityData target, BlueprintAbility blast, BlueprintBuff paralysis,
            bool wantBlast, string id)
        {
            var commands = new HashSet<UnitCommand>(); var ambient = new HashSet<string>();
            var costs = new JArray(); var endings = new JArray();
            var attacks = new Sprint24AttackObserver { Owner = owner, Target = target };
            var effects = new TrumpetBlastObservation(blast, paralysis, new[] { owner });
            bool clean = !owner.Commands.Raw.Any(value => value != null && !value.IsFinished);
            Sprint25Check(id + "-queue", clean, new JObject { ["actor"] = owner.UniqueId,
                ["commands"] = new JArray(owner.Commands.Raw.Where(value => value != null).Select(Sprint27AiCommand)) },
                "The next exact native brain decision starts without a live command.");
            if (!clean) throw new InvalidOperationException("Trumpet AI decision has a live prior command.");
            owner.Commands.RemoveFinishedAndUpdateQueue();
            Sprint28SetBrainActivePreservingActions(owner, true); owner.Brain.RestoreAvailableActions();
            EventBus.Subscribe(attacks); EventBus.Subscribe(effects);
            int frames = 0, ticks = 0;
            try
            {
                using (var economy = new FirearmReviewCosts(owner, costs))
                using (var trace = new Sprint26CommandTrace(owner, endings))
                {
                    var deadline = DateTime.UtcNow.AddSeconds(45);
                    while (DateTime.UtcNow < deadline && frames++ < 4500)
                    {
                        if (frames == 1 || frames % 60 == 0 && !owner.Commands.Raw.Any(value => value != null && !value.IsFinished))
                        { foreach (string candidate in Sprint24TickExactAi(owner)) if (candidate != owner.UniqueId) ambient.Add(candidate); ticks++; }
                        foreach (var command in owner.Commands.Raw.Where(value => value != null)) commands.Add(command);
                        bool blasted = commands.OfType<UnitUseAbility>().Any(value => value.AiAction?.name ==
                            "KMG_Summoning_OutsiderCelestial_TrumpetArchon_TrumpetBlastAi" && value.IsFinished &&
                            value.Result == UnitCommand.ResultType.Success && value.ExecutionProcess != null && value.ExecutionProcess.IsEnded);
                        bool attacked = commands.OfType<UnitAttack>().Any(value => value.AiAction != null && value.IsFinished &&
                            (value.Result == UnitCommand.ResultType.Success || trace.Exhausted(value))) && attacks.Damage.Any(value => value.Damage > 0);
                        if (wantBlast ? blasted && effects.Saves.Count > 0 : attacked || effects.Saves.Count > 0) break;
                        Game.Instance.IsPaused = false; yield return 0;
                    }
                    var chosen = wantBlast ? (UnitCommand)commands.OfType<UnitUseAbility>().FirstOrDefault(value =>
                        value.AiAction?.name == "KMG_Summoning_OutsiderCelestial_TrumpetArchon_TrumpetBlastAi" && value.IsFinished &&
                        value.Result == UnitCommand.ResultType.Success) : commands.OfType<UnitAttack>().FirstOrDefault(value =>
                        value.AiAction != null && value.IsFinished && (value.Result == UnitCommand.ResultType.Success || trace.Exhausted(value)));
                    bool native = ticks > 0 && chosen != null && !chosen.Cutscene && !chosen.IsIgnoreCooldown &&
                        economy.Count(chosen) == 1 && economy.Exact(chosen, false);
                    var targetSaves = effects.Saves.Where(row => (string)row["target"] == target.UniqueId &&
                        (string)row["caster"] == owner.UniqueId && (int)row["dc"] == 20).ToArray();
                    bool outcome = wantBlast ? targetSaves.Length > 0 &&
                            (targetSaves.All(row => (bool)row["passed"]) || target.Descriptor.State.HasCondition(UnitCondition.Paralyzed)) :
                        effects.Saves.Count == 0 && !commands.OfType<UnitUseAbility>().Any(value =>
                            value.AiAction?.name == "KMG_Summoning_OutsiderCelestial_TrumpetArchon_TrumpetBlastAi") &&
                        attacks.WeaponAttacks.Count > 0 && attacks.Damage.Any(value => value.Damage > 0) &&
                        attacks.WeaponAttacks.All(value => ReferenceEquals(value.Weapon, owner.Body.PrimaryHand.MaybeWeapon));
                    Sprint25Check(id, native && outcome, new JObject { ["owner"] = owner.UniqueId, ["target"] = target.UniqueId,
                        ["wantedBlast"] = wantBlast, ["aiTicks"] = ticks, ["frames"] = frames,
                        ["commands"] = new JArray(commands.Select(Sprint27AiCommand)), ["costs"] = costs, ["endings"] = endings,
                        ["blastSaves"] = effects.Saves.DeepClone(), ["blastEffects"] = effects.Effects,
                        ["attacks"] = new JArray(attacks.WeaponAttacks.Select(value => new JObject {
                            ["owner"] = value.Initiator.UniqueId, ["target"] = value.Target.UniqueId,
                            ["weapon"] = value.Weapon?.Blueprint.AssetGuid, ["hit"] = value.AttackRoll?.IsHit })),
                        ["damage"] = new JArray(attacks.Damage.Select(value => new JObject {
                            ["source"] = value.Initiator.UniqueId, ["target"] = value.Target.UniqueId, ["damage"] = value.Damage })),
                        ["ambientCandidatesNeverTicked"] = new JArray(ambient), ["fixtureBlastCommands"] = 0,
                        ["fixtureAttackCommands"] = 0, ["fixtureSlotDebits"] = 0, ["forcedRolls"] = 0 },
                        wantBlast ? "The real native brain selects its safe blast, pays one standard action and resolves the hostile's actual save/paralysis." :
                        "The native brain refuses an unsafe all-creature blast and selects its own held sword, with a real attack, damage and ordinary action cost.");
                    if (!native || !outcome) throw new InvalidOperationException("Trumpet native autonomous decision failed: " + id);
                }
            }
            finally { Sprint28SetBrainActivePreservingActions(owner, false); EventBus.Unsubscribe(effects); EventBus.Unsubscribe(attacks); }
        }

        private IEnumerable<int> ReviewTrumpet31BlastAi(ExpandedSummoningCorrectionFixture fixture)
        {
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue, oldPause = Game.Instance.IsPaused;
            UnitEntityData owner = null, blocker = null; BlueprintUnit blockerBlueprint = null;
            CreateExpandedSummoningCorrectionHostile(fixture); var target = fixture.Hostile;
            int fort = target.Stats.SaveFortitude.BaseValue, ac = target.Stats.AC.BaseValue;
            int casterFort = fixture.Caster.Stats.SaveFortitude.BaseValue;
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;
                owner = CastExpandedSummoningOwnTier(fixture, "trumpet-archon");
                Sprint28SetBrainActivePreservingActions(owner, false); owner.Commands.InterruptAll(true); owner.Commands.RemoveFinishedAndUpdateQueue();
                Vector3 far; string survey, direction;
                var origin = Sprint24AiCombatLane(fixture.Caster, owner, target, 2f, out far, out survey, out direction);
                PlaceExpandedSummoningUnit(owner, origin); PlaceExpandedSummoningUnit(target, far);
                blockerBlueprint = UnityEngine.Object.Instantiate(BlueprintRoot.Instance.DefaultPlayerCharacter);
                blockerBlueprint.name = "KMG_Runtime_Trumpet31_PlayerFactionBlocker"; blockerBlueprint.IsCheater = true;
                blockerBlueprint.Brain = null; blockerBlueprint.StartingInventory = Array.Empty<Kingmaker.Blueprints.Items.BlueprintItem>();
                blocker = Game.Instance.EntityCreator.SpawnUnit(blockerBlueprint, origin - Vector3.right * 2f, Quaternion.identity, fixture.Scene);
                if (blocker == null) throw new InvalidOperationException("Owned Trumpet AI ally did not instantiate.");
                fixture.Created.Add(blocker); Sprint28SetBrainActivePreservingActions(blocker, false);
                blocker.Commands.InterruptAll(true); blocker.Stats.HitPoints.BaseValue = 100000;
                target.Stats.SaveFortitude.BaseValue = -100; target.Stats.AC.BaseValue = 0;
                foreach (var unit in new[] { owner, target, blocker })
                { unit.Wake(0f); if (!Game.Instance.State.AwakeUnits.Contains(unit)) Game.Instance.State.AwakeUnits.Add(unit); unit.JoinCombat(); unit.SightCache.ClearIfNeeded(); }
                owner.Memory.Add(target); target.Memory.Add(owner);
                new Kingmaker.Controllers.Units.UnitMemoryController().Tick();
                Game.Instance.Player.UpdateIsInCombat(); new UnitCombatJoinController().Tick(); new UnitCombatPrepareController().Tick();
                for (int frame = 0; frame < 8; frame++) yield return 0;
                var map = ExpandedSummoningBlueprints.Registered.BySymbol; string prefix = ExpandedSummoningSpecialBuilder.Trumpet31Prefix;
                var blast = (BlueprintAbility)map[prefix + "TrumpetBlast"];
                var paralysis = (BlueprintBuff)map[prefix + "TrumpetParalyzed"];
                var safety = (TrumpetBlastEligibility)map[prefix + "TrumpetBlastEligibility"];
                var aid = (BlueprintAbility)map[prefix + "Aid"];
                // Real fixture preparation prevents at-will Aid from being the
                // outstanding support decision for either owned ally. It does
                // not change any AI action, resource, roll, or blast effect.
                foreach (int frame in Trumpet31ManualCommand(owner, new AbilityData(owner.Descriptor.Abilities.GetAbility(aid)),
                    owner, "trumpet-blast-ai-fixture-aid-owner")) yield return frame;
                foreach (int frame in Trumpet31ManualCommand(owner, new AbilityData(owner.Descriptor.Abilities.GetAbility(aid)),
                    blocker, "trumpet-blast-ai-fixture-aid-blocker")) yield return frame;
                var data = new AbilityData(owner.Descriptor.Abilities.GetAbility(blast));
                var decision = new DecisionContext { Unit = owner, Target = new TargetInfo { Unit = owner } };
                var book = owner.Descriptor.GetSpellbook(owner.Blueprint.GetComponent<TrumpetPreparedBook>().Book);
                Func<int[]> slots = () => Enumerable.Range(1, 7).Select(level => book.GetMemorizedSpellSlots(level).Count(slot => slot.Available && slot.Spell != null)).ToArray();
                var before = slots();
                bool ready = blocker.IsPlayerFaction && !owner.IsEnemy(blocker) && data.IsAvailable &&
                    safety.Score(decision) == 0f && owner.IsInCombat && owner.Memory.ContainsVisible(target) && owner.HasLOS(target) &&
                    !owner.Commands.Raw.Any(value => value != null && !value.IsFinished);
                Sprint25Check("trumpet-blast-ai-unsafe-ready", ready, new JObject { ["owner"] = owner.UniqueId,
                    ["target"] = target.UniqueId, ["blocker"] = blocker.UniqueId, ["blockerPlayerFaction"] = blocker.IsPlayerFaction,
                    ["blockerIsAlly"] = owner.IsAlly(blocker), ["blockerIsEnemy"] = owner.IsEnemy(blocker),
                    ["score"] = safety.Score(decision), ["manualAvailable"] = data.IsAvailable,
                    ["lane"] = survey, ["visible"] = owner.Memory.ContainsVisible(target), ["slots"] = new JArray(before),
                    ["fixtureAidCommands"] = 2, ["fixtureBlastCommands"] = 0, ["fixtureAttackCommands"] = 0 },
                    "A real nearby player-faction nonenemy blocks only AI selection; manual blast access remains legal. Native memory, combat, full resources and prior Aid are observed.");
                if (!ready) throw new InvalidOperationException("Unsafe Trumpet AI fixture preconditions are unmet.");
                var model = owner.View?.HandsEquipment?.GetWeaponModel(false);
                var viewRow = new JObject { ["owner"] = owner.UniqueId, ["unit"] = owner.Blueprint.AssetGuid,
                    ["view"] = owner.View?.name, ["activeMainHandModel"] = model?.name,
                    ["modelUnderOwnedView"] = model != null && owner.View != null && model.transform.IsChildOf(owner.View.transform),
                    ["modelLocalPosition"] = model?.transform.localPosition.ToString(), ["modelLocalRotation"] = model?.transform.localEulerAngles.ToString(),
                    ["renderers"] = new JArray(owner.View.GetComponentsInChildren<Renderer>(true).Select(value => new JObject {
                        ["name"] = value.name, ["enabled"] = value.enabled, ["active"] = value.gameObject.activeInHierarchy,
                        ["center"] = value.bounds.center.ToString(), ["size"] = value.bounds.size.ToString() })) };
                Sprint25Check("trumpet-blast-ai-body-probe", owner.View != null && model != null && model.transform.IsChildOf(owner.View.transform),
                    viewRow, "Read-only exact owned native body/main-hand model observation for the subsequent trumpet form attachment; no donor mutation or appearance approval.");
                foreach (int frame in Trumpet31ObserveBlastDecision(owner, target, blast, paralysis, false, "trumpet-blast-ai-unsafe-fallback")) yield return frame;
                bool negativeBookRetained = before.SequenceEqual(slots());
                Sprint25Check("trumpet-blast-ai-fallback-book", negativeBookRetained, new JObject {
                    ["owner"] = owner.UniqueId, ["before"] = new JArray(before), ["after"] = new JArray(slots()) },
                    "The actual player-summoned support/attack fallback leaves every prepared slot unchanged.");
                if (!negativeBookRetained) throw new InvalidOperationException("Fallback changed prepared slots.");
                var priorOwner = owner;
                var priorWeapon = priorOwner.Body.PrimaryHand.MaybeWeapon;
                priorOwner.Commands.InterruptAll(true); priorOwner.Commands.RemoveFinishedAndUpdateQueue();
                DisposeExpandedSummoningUnits(fixture.Created, new[] { blocker, priorOwner }); blocker = null; owner = null;
                ResetExpandedSummoningHostile(fixture);
                // The native first-level hall has no ally-clear 100-foot
                // player stance (actual189 eligible surveyed lanes, zero clear).
                // The same exact registered brain is therefore observed from
                // the already supported hostile-caster route. No original
                // faction or actor is edited, removed, disabled, or ticked.
                bool reset = priorOwner.Destroyed &&
                    !Game.Instance.State.Units.All.Contains(priorOwner) &&
                    !priorOwner.Commands.Raw.Any(value => value != null && !value.IsFinished) &&
                    !Game.Instance.State.AreaEffects.All.Any(area => ReferenceEquals(area.Context?.MaybeCaster, priorOwner)) &&
                    priorWeapon != null && !priorWeapon.IsLootable;
                var retained = Game.Instance.State.Units.All.Where(value => value != null && !value.Destroyed)
                    .SelectMany(unit => unit.Buffs.Enumerable.Where(buff => ReferenceEquals(buff.Context?.MaybeCaster, priorOwner))
                        .Select(buff => new JObject { ["target"] = unit.UniqueId, ["blueprint"] = buff.Blueprint.AssetGuid,
                            ["end"] = buff.EndTime.ToString() })).ToArray();
                Sprint25Check("trumpet-blast-ai-owned-reset", reset, new JObject {
                    ["priorOwner"] = priorOwner.UniqueId, ["destroyed"] = priorOwner.Destroyed,
                    ["sourceAreas"] = Game.Instance.State.AreaEffects.All.Count(area => ReferenceEquals(area.Context?.MaybeCaster, priorOwner)),
                    ["sourceLiveCommands"] = priorOwner.Commands.Raw.Count(value => value != null && !value.IsFinished),
                    ["formerWeaponLootable"] = priorWeapon?.IsLootable, ["retainedNativeVictimEffects"] = new JArray(retained),
                    ["negativeBookRetained"] = negativeBookRetained, ["blockerDisposed"] = blocker == null,
                    ["originalActorOrFactionEdits"] = 0, ["protectedActorTicks"] = 0 },
                    "Only the owned prior summon/blocker are disposed; no prior emitter area, live command or loot remains. Native Aid victim effects keep their selected finite lifetime and are recorded rather than erased; they do not change blast eligibility or the fresh actor's slots.");
                if (!reset) throw new InvalidOperationException("Owned actor reset failed; positive AI subcase is unrun.");
                owner = CastExpandedSummoningOwnTier(fixture, "trumpet-archon", fixture.Hostile);
                Sprint28SetBrainActivePreservingActions(owner, false); owner.Commands.InterruptAll(true); owner.Commands.RemoveFinishedAndUpdateQueue();
                var positiveTarget = fixture.Caster;
                positiveTarget.Stats.SaveFortitude.BaseValue = -100;
                try
                {
                    origin = Sprint24AiCombatLane(fixture.Caster, owner, positiveTarget, 2f, out far, out survey, out direction,
                        new Feet(100).Meters + 2.5f, new[] { fixture.Hostile }, 30);
                }
                catch (Exception error)
                {
                    Sprint25Check("trumpet-blast-ai-safe-location", false, new JObject {
                        ["owner"] = owner.UniqueId, ["target"] = positiveTarget.UniqueId, ["ownerFaction"] = owner.Faction?.name,
                        ["exception"] = error.ToString(), ["nearbyNonenemies"] = new JArray(safety.NearbyNonArchonNonEnemies(owner)
                            .Select(value => new JObject { ["unit"] = value.UniqueId, ["blueprint"] = value.Blueprint.AssetGuid,
                                ["name"] = value.Blueprint.name, ["faction"] = value.Faction?.name, ["distance"] = owner.DistanceTo(value) })) },
                        "An unmet native clear-location precondition is recorded explicitly. No positive command is issued and no protected actor is removed to create a pass.");
                    throw;
                }
                PlaceExpandedSummoningUnit(owner, origin); PlaceExpandedSummoningUnit(positiveTarget, far);
                PlaceExpandedSummoningUnit(fixture.Hostile, origin + Vector3.back * 40f);
                foreach (var unit in new[] { owner, positiveTarget })
                { unit.Wake(0f); if (!Game.Instance.State.AwakeUnits.Contains(unit)) Game.Instance.State.AwakeUnits.Add(unit); unit.JoinCombat(); unit.SightCache.ClearIfNeeded(); }
                owner.Memory.Add(positiveTarget); positiveTarget.Memory.Add(owner);
                new Kingmaker.Controllers.Units.UnitMemoryController().Tick();
                Game.Instance.Player.UpdateIsInCombat(); new UnitCombatJoinController().Tick(); new UnitCombatPrepareController().Tick();
                for (int frame = 0; frame < 8; frame++) yield return 0;
                decision = new DecisionContext { Unit = owner, Target = new TargetInfo { Unit = owner } };
                data = new AbilityData(owner.Descriptor.Abilities.GetAbility(blast));
                book = owner.Descriptor.GetSpellbook(owner.Blueprint.GetComponent<TrumpetPreparedBook>().Book);
                before = slots();
                var remaining = safety.NearbyNonArchonNonEnemies(owner).ToArray();
                bool clear = remaining.Length == 0 && owner.IsEnemy(positiveTarget) &&
                    !ReferenceEquals(priorOwner, owner) && ReferenceEquals(owner.Blueprint, priorOwner.Blueprint);
                Sprint25Check("trumpet-blast-ai-safe-location", clear, new JObject { ["owner"] = owner.UniqueId,
                    ["priorOwner"] = priorOwner.UniqueId, ["target"] = positiveTarget.UniqueId, ["ownerFaction"] = owner.Faction?.name,
                    ["targetFaction"] = positiveTarget.Faction?.name, ["targetEnemy"] = owner.IsEnemy(positiveTarget),
                    ["lane"] = survey, ["remainingNonenemies"] = new JArray(remaining.Select(value => value.UniqueId)),
                    ["sameRegisteredBlueprintAndBrain"] = ReferenceEquals(owner.Blueprint, priorOwner.Blueprint),
                    ["originalActorOrFactionEdits"] = 0, ["protectedActorTicks"] = 0 },
                    "A new real hostile-caster summon of the same exact blueprint/brain has a native walkable, visible clear stance. Original world actors/factions are unchanged; only its disposable caster is moved out of radius.");
                if (!clear) throw new InvalidOperationException("Safe hostile-caster native location is not clear.");
                bool positive = safety.Score(decision) == 1f && owner.Memory.ContainsVisible(positiveTarget) &&
                    owner.IsInCombat && !owner.Commands.Raw.Any(value => value != null && !value.IsFinished) && data.IsAvailable;
                Sprint25Check("trumpet-blast-ai-safe-ready", positive, new JObject { ["owner"] = owner.UniqueId, ["target"] = positiveTarget.UniqueId,
                    ["lane"] = survey, ["score"] = safety.Score(decision), ["remainingNonenemies"] = new JArray(remaining.Select(value => value.UniqueId)),
                    ["slotsBefore"] = new JArray(before), ["slotsAfter"] = new JArray(slots()), ["blockerDisposed"] = blocker == null,
                    ["visible"] = owner.Memory.ContainsVisible(positiveTarget), ["manualAvailable"] = data.IsAvailable },
                    "After verified owned reset, native memory/combat/access and clear nonenemy radius admit the unchanged production selection rule; the fresh summon owns its own prepared slots.");
                if (!positive) throw new InvalidOperationException("Safe Trumpet AI location/reset preconditions are unmet.");
                foreach (int frame in Trumpet31ObserveBlastDecision(owner, positiveTarget, blast, paralysis, true, "trumpet-blast-ai-safe-cast")) yield return frame;
                Sprint25Check("trumpet-blast-ai-book-isolation", negativeBookRetained && before.SequenceEqual(slots()), new JObject {
                    ["owner"] = owner.UniqueId, ["negativeOwner"] = priorOwner.UniqueId, ["negativeSlotsRetained"] = negativeBookRetained,
                    ["before"] = new JArray(before), ["after"] = new JArray(slots()) },
                    "Both actual brains retain their independent prepared spell slots; no slot is spent/refilled by the supernatural blast or sword fallback.");
            }
            finally
            {
                if (owner != null) { Sprint28SetBrainActivePreservingActions(owner, false); owner.Commands.InterruptAll(true); }
                target.Commands.InterruptAll(true); target.Stats.SaveFortitude.BaseValue = fort; target.Stats.AC.BaseValue = ac;
                fixture.Caster.Stats.SaveFortitude.BaseValue = casterFort;
                DisposeExpandedSummoningUnits(fixture.Created, new[] { owner, blocker }.Where(value => value != null && !value.Destroyed).ToArray());
                if (blockerBlueprint != null) UnityEngine.Object.Destroy(blockerBlueprint);
                ResetExpandedSummoningHostile(fixture);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode; Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = oldPause;
            }
        }
    }
}
