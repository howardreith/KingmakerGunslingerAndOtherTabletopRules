using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // The existing disposable fixture owns every actor and its clock. This
        // cell issues native commands and observes results; it never supplies
        // a hit, saving throw, healing amount, damage or production effect.
        private IEnumerable<int> ReviewInfernalEarly34(ExpandedSummoningCorrectionFixture fixture)
        {
            var owners = new List<UnitEntityData>();
            var receipt = new JObject { ["phase"] = "sprint34-infernal-early", ["finalQualification"] = false };
            var cells = new JArray(); receipt["cells"] = cells;
            var trace = new InfernalEvents34(); receipt["events"] = trace.Rows;
            try
            {
                CaptureInfernalInputs34(fixture, receipt);
                CreateSprint32TurnHostile(fixture);
                var target = fixture.Hostile;
                var barbazu = CastExpandedSummoningOwnTier(fixture, "bearded-devil"); owners.Add(barbazu);
                var kyton = CastExpandedSummoningOwnTier(fixture, "kyton-chain-devil"); owners.Add(kyton);
                var healer = CastExpandedSummoningOwnTier(fixture, "lillend-azata"); owners.Add(healer);
                foreach (var actor in owners) {
                    Sprint28SetBrainActivePreservingActions(actor, false);
                    actor.Commands.InterruptAll(true); actor.Commands.RemoveFinishedAndUpdateQueue();
                    if (!ReferenceEquals(actor, healer)) {
                        foreach (int frame in Outsider30NativeAppearance(actor, actor.Blueprint.name)) yield return frame;
                    } else {
                        DateTime appearance = DateTime.UtcNow.AddSeconds(10);
                        while (DateTime.UtcNow < appearance && (actor.View == null || !actor.Descriptor.State.CanAct || !EntityFadedIn(actor)))
                        { Game.Instance.IsPaused = false; yield return 0; }
                        if (actor.View == null || !actor.Descriptor.State.CanAct) throw new InvalidOperationException("Native healer appearance incomplete.");
                    }
                }
                foreach (int frame in Sprint22SetPowerAttack(barbazu, false)) yield return frame;
                receipt["profiles"] = new JArray(new[] { barbazu, kyton }.Select(actor => new JObject {
                    ["owner"] = actor.UniqueId, ["blueprint"] = actor.Blueprint.AssetGuid,
                    ["hp"] = actor.Stats.HitPoints.ModifiedValue, ["ac"] = actor.Stats.AC.ModifiedValue,
                    ["bab"] = actor.Stats.BaseAttackBonus.ModifiedValue,
                    ["fort"] = actor.Stats.SaveFortitude.ModifiedValue, ["reflex"] = actor.Stats.SaveReflex.ModifiedValue,
                    ["will"] = actor.Stats.SaveWill.ModifiedValue, ["body"] = ExpandedSummoningPteranodonViewPatch.DescribeView(actor.View),
                    ["primary"] = actor.Body.PrimaryHand.MaybeWeapon?.Blueprint.AssetGuid,
                    ["additional"] = new JArray(actor.Body.AdditionalLimbs.Select(slot => slot.MaybeWeapon?.Blueprint.AssetGuid)) }));

                var chainProfile = InfernalMartialProfile34.For(kyton);
                var controlBlueprint = fixture.Blueprints.OfType<BlueprintAbility>().Single(value => value.name == "KMG_Summoning_Infernal_KytonChainDevil_DancingChains");
                var control = new AbilityData(kyton.Descriptor.Abilities.GetAbility(controlBlueprint));
                InfernalCombatLane34(fixture, kyton, target, 8);
                float beforeReach = kyton.Body.PrimaryHand.MaybeWeapon.AttackRange.Value;
                int attacksBefore = trace.Rows.Count(row => (string)row["event"] == "attack");
                var activate = new UnitUseAbility(control, new TargetWrapper(kyton)); var controlCosts = new JArray();
                using (var costs = new FirearmReviewCosts(kyton, controlCosts)) {
                    kyton.Commands.Run(activate);
                    foreach (int frame in WaitInfernalCommand34(activate, () => chainProfile.ControlsOwnChains(kyton), 20)) yield return frame;
                    bool pass = activate.Result == UnitCommand.ResultType.Success && activate.IsActed &&
                        control.RuntimeActionType == UnitCommand.CommandType.Standard && !control.RequireFullRoundAction &&
                        chainProfile.ControlsOwnChains(kyton) && beforeReach == 10 && kyton.Body.PrimaryHand.MaybeWeapon.AttackRange.Value == 25 &&
                        trace.Rows.Count(row => (string)row["event"] == "attack") == attacksBefore && costs.Count(activate) == 1 &&
                        (costs.InCombat(activate) ? costs.Exact(activate, false) : costs.Unchanged(activate));
                    var cell = new JObject { ["command"] = Sprint27AiCommand(activate), ["costs"] = controlCosts,
                        ["owner"] = kyton.UniqueId, ["rangeBefore"] = beforeReach, ["rangeAfter"] = kyton.Body.PrimaryHand.MaybeWeapon.AttackRange.Value,
                        ["attacksAdded"] = trace.Rows.Count(value => (string)value["event"] == "attack") - attacksBefore };
                    cells.Add(cell); Sprint25Check("sprint34-chain-control-native", pass, cell, "Native standard-action control extends only owned chains and makes no attacks.");
                    if (!pass) throw new InvalidOperationException("Chain control command/cost/outcome mismatch.");
                }
                InfernalCombatLane34(fixture, kyton, target, 20);
                var attack = new UnitAttack(target) { ForceFullAttack = true, IsSingleAttack = false };
                int chainStart = trace.Rows.Count;
                kyton.Commands.Run(attack);
                foreach (int frame in WaitInfernalCommand34(attack, () => trace.Rows.Skip(chainStart).Count(row =>
                    (string)row["event"] == "attack" && (string)row["owner"] == kyton.UniqueId) >= 4, 30)) yield return frame;
                var chainRows = trace.Rows.Skip(chainStart).Where(row => (string)row["event"] == "attack" &&
                    (string)row["owner"] == kyton.UniqueId).ToArray();
                bool chainPass = attack.IsFinished && attack.AllAttacks.Count == 4 && chainRows.Length == 4 &&
                    chainRows.All(row => (string)row["weapon"] == chainProfile.Chain.AssetGuid);
                var routine = new JObject { ["command"] = Sprint27AiCommand(attack), ["owner"] = kyton.UniqueId,
                    ["target"] = target.UniqueId, ["nativePlanCount"] = attack.AllAttacks.Count, ["attacks"] = new JArray(chainRows) };
                cells.Add(routine); Sprint25Check("sprint34-four-controlled-chains", chainPass, routine,
                    "One later native full-attack command executes four chain attacks; no added attack routine.");
                if (!chainPass) throw new InvalidOperationException("Native chain routine incomplete or extra attacks observed.");
                kyton.Commands.InterruptAll(true); kyton.Commands.RemoveFinishedAndUpdateQueue();

                var wound = fixture.Blueprints.OfType<BlueprintBuff>().Single(value => value.name == "KMG_Summoning_Infernal_BeardedDevil_InfernalWound");
                Func<bool> wounded = () => target.Buffs.Enumerable.Any(buff => ReferenceEquals(buff.Blueprint, wound) && ReferenceEquals(buff.Context?.MaybeCaster, barbazu));
                var creation = healer.Blueprint.GetComponent<LillendBardBook32>();
                var book = healer.Descriptor.GetSpellbook(creation.Book);
                bool rejected = false, cured = false;
                for (int attempt = 0; attempt < 7 && !(rejected && cured); attempt++) {
                    InfernalCombatLane34(fixture, barbazu, target, 8);
                    for (int woundAttempt = 0; woundAttempt < 4 && !wounded(); woundAttempt++) {
                        var hit = new UnitAttack(target) { IsSingleAttack = true, ForceFullAttack = false };
                        barbazu.Commands.Run(hit);
                        foreach (int frame in WaitInfernalCommand34(hit, () => true, 20)) yield return frame;
                    }
                    if (!wounded()) throw new InvalidOperationException("Unobserved precondition: no damaging glaive hit in four native attempts.");
                    // The actual weapon injured this patient. No fixture HP write.
                    barbazu.Commands.InterruptAll(true); barbazu.Commands.RemoveFinishedAndUpdateQueue();
                    var cure = Enumerable.Range(1, 3).Where(level => book.GetSpontaneousSlots(level) > 0)
                        .SelectMany(level => book.GetKnownSpells(level)).FirstOrDefault(value =>
                            value.Blueprint.AssetGuid == "5590652e1c2225c4ca30c4a699ab3649" ||
                            value.Blueprint.AssetGuid == "3361c5df793b4c8448756146a88026ad");
                    if (cure == null) throw new InvalidOperationException("Native healer has no remaining selected cure slot.");
                    PlaceExpandedSummoningUnit(healer, target.Position + Vector3.right * (target.Corpulence + healer.Corpulence + .3f));
                    if (!cure.IsAvailable || !cure.CanTarget(new TargetWrapper(target)))
                        throw new InvalidOperationException("Native cure target eligibility failed; examine raw patient/faction evidence.");
                    int levelUsed = book.GetSpellLevel(cure.Blueprint), slots = book.GetSpontaneousSlots(book.GetSpellLevel(cure.Blueprint));
                    int start = trace.Rows.Count, damageBefore = target.Damage;
                    var command = new UnitUseAbility(cure, new TargetWrapper(target)); var healCosts = new JArray();
                    using (var costs = new FirearmReviewCosts(healer, healCosts)) {
                        healer.Commands.Run(command);
                        var deliveries = new JArray();
                        UnitUseAbility delivery = command;
                        int deliveryStart = start;
                        // A hostile cure may miss its native touch attack. Retain
                        // that distinction and retry only the actual held charge.
                        // No new slot, dice override or manufactured healing event.
                        for (int touchAttempt = 0; touchAttempt < 4; touchAttempt++) {
                            foreach (int frame in WaitInfernalCommand34(delivery, () =>
                                !healer.Commands.Raw.Concat(healer.Commands.Queue).OfType<UnitUseAbility>().Any(value => !value.IsFinished) &&
                                trace.Rows.Skip(deliveryStart).Any(row =>
                                    (string)row["owner"] == healer.UniqueId && (string)row["target"] == target.UniqueId &&
                                    ((string)row["event"] == "heal" || (string)row["event"] == "attack-roll")), 25)) yield return frame;
                            var held = healer.Get<UnitPartTouch>()?.Ability?.Data;
                            var outcome = trace.Rows.Skip(deliveryStart).Where(row => (string)row["owner"] == healer.UniqueId &&
                                (string)row["target"] == target.UniqueId &&
                                ((string)row["event"] == "heal" || (string)row["event"] == "attack-roll")).ToArray();
                            deliveries.Add(new JObject { ["attempt"] = touchAttempt, ["command"] = Sprint27AiCommand(delivery),
                                ["held"] = held?.Blueprint?.AssetGuid, ["heldSource"] = held?.StickyTouch?.Blueprint?.AssetGuid,
                                ["events"] = new JArray(outcome), ["slots"] = book.GetSpontaneousSlots(levelUsed) });
                            if (outcome.Any(row => (string)row["event"] == "heal")) break;
                            bool missed = outcome.Any(row => (string)row["event"] == "attack-roll" && (bool?)row["hit"] == false);
                            if (!missed || held == null || !ReferenceEquals(held.StickyTouch?.Blueprint, cure.Blueprint))
                                throw new InvalidOperationException("Cure produced no healing without a matching native missed-touch charge.");
                            if (touchAttempt == 3) {
                                cells.Add(new JObject { ["attempt"] = attempt, ["classification"] = "NOT_OBSERVED",
                                    ["reason"] = "Four actual native touch misses", ["deliveries"] = deliveries });
                                throw new InvalidOperationException("Unobserved precondition: four native cure touches missed; charge retained.");
                            }
                            deliveryStart = trace.Rows.Count;
                            delivery = new UnitUseAbility(held, new TargetWrapper(target));
                            healer.Commands.Run(delivery);
                        }
                        var heals = trace.Rows.Skip(start).Where(row => (string)row["event"] == "heal" && (string)row["target"] == target.UniqueId).ToArray();
                        var decision = heals.Select(row => row["woundCheck"]).FirstOrDefault(value => value != null);
                        bool passed = (bool?)decision?["passed"] == true;
                        bool eventPass = decision != null && (bool?)decision["spell"] == true &&
                            (passed ? heals.Any(row => (int?)row["actual"] > 0) && !wounded() :
                                heals.All(row => (int?)row["actual"] == 0 && (int?)row["before"] == (int?)row["after"]) && wounded());
                        bool costPass = command.IsActed && book.GetSpontaneousSlots(levelUsed) == slots - 1;
                        var cell = new JObject { ["attempt"] = attempt, ["owner"] = barbazu.UniqueId, ["patient"] = target.UniqueId,
                            ["healer"] = healer.UniqueId, ["command"] = Sprint27AiCommand(command), ["costs"] = healCosts,
                            ["damageBefore"] = damageBefore, ["damageAfter"] = target.Damage, ["woundRemains"] = wounded(),
                            ["heals"] = new JArray(heals), ["deliveries"] = deliveries,
                            ["slotsBefore"] = slots, ["slotsAfter"] = book.GetSpontaneousSlots(levelUsed) };
                        cells.Add(cell); Sprint25Check("sprint34-wound-native-heal-" + attempt, eventPass && costPass, cell,
                            "A real cure command spends its slot; native wound CL check either rejects all healing or heals and closes the owned wound.");
                        if (!eventPass || !costPass) throw new InvalidOperationException("Infernal Wound native healing consumer mismatch.");
                        rejected |= !passed; cured |= passed;
                    }
                }
                Sprint25Check("sprint34-wound-both-natural-outcomes", rejected && cured,
                    new JObject { ["rejectionObserved"] = rejected, ["cureObserved"] = cured, ["maximumCureAttempts"] = 7, ["forcedRolls"] = 0 },
                    "Bounded real native casts observed both caster-level outcomes; missing natural eligibility is not a forced PASS.");
                if (!(rejected && cured)) throw new InvalidOperationException("Unobserved precondition: bounded native cures did not produce both CL outcomes.");

                var sibling = CastExpandedSummoningOwnTier(fixture, "bearded-devil"); owners.Add(sibling);
                Sprint28SetBrainActivePreservingActions(sibling, false);
                foreach (int frame in Outsider30NativeAppearance(sibling, sibling.Blueprint.name)) yield return frame;
                foreach (int frame in Sprint22SetPowerAttack(sibling, false)) yield return frame;
                Func<UnitEntityData, bool> hasWound = source => target.Buffs.Enumerable.Any(buff =>
                    ReferenceEquals(buff.Blueprint, wound) && ReferenceEquals(buff.Context?.MaybeCaster, source));
                foreach (var source in new[] { barbazu, sibling }) {
                    InfernalCombatLane34(fixture, source, target, 8);
                    for (int attempt = 0; attempt < 4 && !hasWound(source); attempt++) {
                        var hit = new UnitAttack(target) { IsSingleAttack = true, ForceFullAttack = false };
                        source.Commands.Run(hit);
                        foreach (int frame in WaitInfernalCommand34(hit, () => true, 20)) yield return frame;
                    }
                    if (!hasWound(source)) throw new InvalidOperationException("Unobserved precondition: no sibling glaive injury in four attempts.");
                    source.Commands.InterruptAll(true); source.Commands.RemoveFinishedAndUpdateQueue();
                }
                var ownedWounds = new JObject { ["patient"] = target.UniqueId, ["firstSource"] = barbazu.UniqueId,
                    ["secondSource"] = sibling.UniqueId, ["bothPresent"] = hasWound(barbazu) && hasWound(sibling) };
                int bleedStart = trace.Rows.Count, bleedDamageBefore = target.Damage;
                double bleedTime = Game.Instance.TimeController.GameTime.TotalSeconds;
                DateTime bleedDeadline = DateTime.UtcNow.AddSeconds(20);
                while (DateTime.UtcNow < bleedDeadline && !trace.Rows.Skip(bleedStart).Any(row =>
                    (string)row["event"] == "damage" && (string)row["target"] == target.UniqueId))
                { Game.Instance.IsPaused = false; yield return 0; }
                var ticks = trace.Rows.Skip(bleedStart).Where(row => (string)row["event"] == "damage" &&
                    (string)row["target"] == target.UniqueId).ToArray();
                ownedWounds["ticks"] = new JArray(ticks); ownedWounds["damageBefore"] = bleedDamageBefore;
                ownedWounds["damageAfter"] = target.Damage;
                ownedWounds["elapsedGameSeconds"] = Game.Instance.TimeController.GameTime.TotalSeconds - bleedTime;
                bool bleedPass = ticks.Length == 1 && (int?)ticks[0]["actual"] == 2 && target.Damage == bleedDamageBefore + 2;
                barbazu.Buffs.GetBuff(Kingmaker.Blueprints.Root.BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff).Remove();
                ownedWounds["firstAfterRemoval"] = hasWound(barbazu); ownedWounds["secondAfterFirstRemoval"] = hasWound(sibling);
                bool firstPass = !hasWound(barbazu) && hasWound(sibling);
                sibling.Buffs.GetBuff(Kingmaker.Blueprints.Root.BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff).Remove();
                ownedWounds["secondAfterRemoval"] = hasWound(sibling);
                bool ownershipPass = firstPass && !hasWound(sibling);
                cells.Add(ownedWounds); Sprint25Check("sprint34-wound-two-source-retirement", bleedPass && ownershipPass,
                    ownedWounds, "Two real glaive sources produce one two-HP native tick; each summon removal retires only its own wound.");
                if (!bleedPass || !ownershipPass) throw new InvalidOperationException("Native overlapping wound tick or exact-source retirement mismatch.");
            }
            finally {
                trace.Dispose();
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory, "sprint34-infernal-early.json"), receipt.ToString(Formatting.Indented));
                foreach (var owner in owners.Where(value => value != null && !value.Destroyed)) {
                    owner.Commands.InterruptAll(true); owner.Commands.RemoveFinishedAndUpdateQueue();
                }
                foreach (var actor in owners.Concat(new[] { fixture.Hostile }).Where(value => value != null && !value.Destroyed)) {
                    actor.Commands.InterruptAll(true); actor.Commands.RemoveFinishedAndUpdateQueue();
                    if (actor.IsInCombat) actor.LeaveCombat();
                }
                Game.Instance.Player.UpdateIsInCombat();
                DisposeExpandedSummoningUnits(fixture.Created, owners.ToArray());
                ResetExpandedSummoningHostile(fixture);
            }
        }

        private IEnumerable<int> WaitInfernalCommand34(UnitCommand command, Func<bool> observed, int seconds)
        {
            DateTime deadline = DateTime.UtcNow.AddSeconds(seconds);
            while (DateTime.UtcNow < deadline && !(command.IsFinished && observed())) {
                Game.Instance.IsPaused = false; yield return 0;
            }
            if (!command.IsFinished || !observed())
                throw new InvalidOperationException("Native Sprint34 command/outcome timeout: " + command.GetType().Name + "; result=" + command.Result);
        }

        private void InfernalCombatLane34(ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner, UnitEntityData target, int feet)
        {
            Vector3 end; string lane, direction;
            Vector3 origin = Sprint24AiCombatLane(fixture.Caster, owner, target, 6f, out end, out lane, out direction);
            PlaceExpandedSummoningUnit(owner, origin);
            PlaceExpandedSummoningUnit(target, origin + (end - origin).normalized *
                (owner.Corpulence + target.Corpulence + new Feet(feet).Meters));
            foreach (var actor in new[] { owner, target }) {
                actor.Wake(0f); if (!Game.Instance.State.AwakeUnits.Contains(actor)) Game.Instance.State.AwakeUnits.Add(actor);
                actor.SightCache.ClearIfNeeded();
            }
            if (owner.HasLOS(target)) owner.Memory.Add(target).Visible = true;
            if (target.HasLOS(owner)) target.Memory.Add(owner).Visible = true;
            owner.JoinCombat(); target.JoinCombat(); Game.Instance.Player.UpdateIsInCombat();
            new UnitCombatJoinController().Tick(); new UnitCombatPrepareController().Tick();
            var probe = new UnitAttack(target); probe.Init(owner);
            bool ready = owner.Descriptor.State.CanAct && owner.IsEnemy(target) && owner.HasLOS(target) && probe.IsUnitEnoughClose;
            Sprint25Check("sprint34-native-lane-" + owner.Blueprint.name + "-" + feet, ready,
                new JObject { ["owner"] = owner.UniqueId, ["target"] = target.UniqueId, ["feetBeyondBodies"] = feet,
                    ["actualWeaponFeet"] = owner.Body.PrimaryHand.MaybeWeapon?.AttackRange.Value,
                    ["nativeApproachRadius"] = probe.ApproachRadius, ["distance"] = owner.DistanceTo(target), ["lane"] = lane },
                "The native attack probe uses this creature's actual weapon; close claws and extended chains have separate measured distances.");
            if (!ready) throw new InvalidOperationException("Native Sprint34 lane eligibility is not established.");
        }

        private void CaptureInfernalInputs34(ExpandedSummoningCorrectionFixture fixture, JObject receipt)
        {
            receipt["nativeBleed"] = DescribeGraph(fixture.Blueprints.OfType<BlueprintBuff>().Single(value =>
                value.AssetGuid == "16249b8075ab8684ca105a78a047a5ef"), 0, new HashSet<object>(NativeDonorReferenceComparer.Instance), 12);
            receipt["nativeUnitTypes"] = new JArray(fixture.Blueprints.OfType<BlueprintUnitType>().Where(value =>
                !value.name.StartsWith("KMG_", StringComparison.Ordinal)).Select(value => new JObject {
                    ["guid"] = value.AssetGuid, ["name"] = value.name, ["displayName"] = value.Name?.ToString() }));
            var apostate = fixture.Blueprints.OfType<BlueprintUnit>().Single(value => value.AssetGuid == "07c0597c519b8ba4db5ae6a01c8b2080");
            receipt["apostateCandidate"] = new JObject { ["guid"] = apostate.AssetGuid, ["name"] = apostate.name,
                ["prefab"] = apostate.Prefab?.AssetId, ["type"] = apostate.Type?.AssetGuid,
                ["renderers"] = new JArray(apostate.Prefab.Load(false).GetComponentsInChildren<SkinnedMeshRenderer>(true).Select(renderer => new JObject {
                    ["name"] = renderer.name, ["bones"] = new JArray(renderer.bones.Select(bone => bone?.name)),
                    ["bounds"] = renderer.localBounds.ToString() })) };
        }
    }
}
