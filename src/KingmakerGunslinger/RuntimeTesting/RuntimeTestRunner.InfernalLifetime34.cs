using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums.Damage;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private static Buff[] SeedInfernalLifetime34(UnitEntityData owner, UnitEntityData target)
        {
            bool kyton = InfernalMartialProfile34.For(owner).Chain != null;
            return kyton ? new[] {
                InfernalOwnedEffect34(owner, owner, InfernalSavedBlueprint34<BlueprintBuff>(true, "ControlledChains")),
                InfernalOwnedEffect34(owner, target, InfernalSavedBlueprint34<BlueprintBuff>(true, "Shaken"), TimeSpan.FromSeconds(18))
            } : new[] {
                InfernalOwnedEffect34(owner, target, InfernalSavedBlueprint34<BlueprintBuff>(false, "InfernalWound")),
                InfernalOwnedEffect34(owner, target, InfernalSavedBlueprint34<BlueprintBuff>(false, "DevilChills"))
            };
        }
        private static UnitCommand InfernalFuture34(UnitEntityData owner, UnitEntityData target)
        {
            if (InfernalMartialProfile34.For(owner).Chain == null) return new UnitAttack(target) { IsSingleAttack = true };
            var ability = InfernalSavedBlueprint34<BlueprintAbility>(true, "UnnervingGaze");
            return new UnitUseAbility(new AbilityData(owner.Descriptor.Abilities.GetAbility(ability)), new TargetWrapper(target));
        }
        private IEnumerable<int> ReviewInfernalLifetime34(ExpandedSummoningCorrectionFixture fixture)
        {
            var created = new List<UnitEntityData>(); var trace = new InfernalEvents34(); var cells = new JArray();
            var receipt = new JObject { ["phase"] = "sprint34-infernal-lifetime", ["cells"] = cells, ["events"] = trace.Rows,
                ["effectOnlyLifecyclePreconditions"] = true };
            Action<string, bool, JObject, string> check = (id, pass, row, description) => {
                row["cell"] = id; cells.Add(row); Sprint25Check("sprint34-" + id, pass, row, description);
                if (!pass) throw new InvalidOperationException("Sprint34 lifetime: " + id);
            };
            var party = Game.Instance.Player.Inventory.Items.ToArray(); var partyCounts = party.Select(item => item.Count).ToArray();
            try {
                foreach (string key in new[] { InfernalVisualPolicy34.Barbazu, InfernalVisualPolicy34.Kyton }) {
                    var owner = CastExpandedSummoningOwnTier(fixture, key); created.Add(owner);
                    var sibling = CastExpandedSummoningOwnTier(fixture, key); created.Add(sibling);
                    foreach (var actor in new[] { owner, sibling }) {
                        Sprint28SetBrainActivePreservingActions(actor, false); actor.Commands.InterruptAll(command => true);
                        foreach (int frame in Outsider30NativeAppearance(actor, key + "-lifetime")) yield return frame;
                    }
                    InfernalCombatLane34(fixture, owner, fixture.Hostile, 8);
                    Game.Instance.IsPaused = true;
                    var effects = SeedInfernalLifetime34(owner, fixture.Hostile);
                    var siblingEffects = SeedInfernalLifetime34(sibling, fixture.Hostile);
                    var items = owner.Descriptor.Inventory.Items.ToArray(); var siblingItems = sibling.Descriptor.Inventory.Items.ToArray();
                    var siblingCounts = siblingItems.Select(item => item.Count).ToArray();
                    var resources = ExpandedSummoningPteranodonViewPatch.CaptureTheropodOwnedResources(owner.View);
                    var siblingResources = ExpandedSummoningPteranodonViewPatch.CaptureTheropodOwnedResources(sibling.View);
                    check("lifetime-ready-" + key, effects.All(buff => buff.Active && !buff.IsDisposed) && siblingEffects.All(buff => buff.Active && !buff.IsDisposed) &&
                        resources.Length > 0 && siblingResources.Length > 0 && !resources.Intersect(siblingResources).Any() && !items.Intersect(siblingItems).Any() &&
                        items.All(item => !item.IsLootable && item.IsNonRemovable),
                        new JObject { ["owner"] = owner.UniqueId, ["sibling"] = sibling.UniqueId,
                            ["owned"] = new JArray(effects.Select(InfernalSavedEffect34)), ["siblingOwned"] = new JArray(siblingEffects.Select(InfernalSavedEffect34)),
                            ["privateResources"] = resources.Length, ["items"] = items.Length },
                        "Two live sources have independently owned effects, equipment and private resources before native cancellation or removal.");
                    int first = trace.Rows.Count; var costs = new JArray();
                    using (var economy = new FirearmReviewCosts(owner, costs)) {
                        var pending = InfernalFuture34(owner, fixture.Hostile); owner.Commands.Run(pending);
                        bool queued = owner.Commands.Raw.Contains(pending) && !pending.IsStarted && !pending.IsFinished;
                        pending.Interrupt(true); owner.Commands.RemoveFinishedAndUpdateQueue();
                        check("native-cancel-" + key, queued && pending.IsFinished && pending.Result != UnitCommand.ResultType.Success &&
                            economy.Count(pending) == 0 && effects.All(buff => buff.Active && !buff.IsDisposed) &&
                            !trace.Rows.Skip(first).Any(row => (string)row["event"] == "attack" || (string)row["event"] == "save" || (string)row["event"] == "damage"),
                            new JObject { ["owner"] = owner.UniqueId, ["queued"] = queued, ["command"] = Sprint27AiCommand(pending), ["costs"] = costs },
                            "Native cancellation retires a genuinely queued attack or gaze before any cost, attack, save or effect.");
                        var future = InfernalFuture34(owner, fixture.Hostile); owner.Commands.Run(future);
                        bool waiting = owner.Commands.Raw.Contains(future) && !future.IsStarted && !future.IsFinished;
                        var expiry = ExpireSprint16OwnedBuff(owner, owner.Buffs.GetBuff(BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff));
                        check("expiry-exact-effect-owners-" + key, waiting && future.IsFinished && future.Result != UnitCommand.ResultType.Success &&
                            economy.Count(future) == 0 && effects.All(buff => buff.IsDisposed) && siblingEffects.All(buff => !buff.IsDisposed && buff.Active) &&
                            Sprint28InventoryExact(sibling.Descriptor.Inventory, siblingItems, siblingCounts),
                            new JObject { ["owner"] = owner.UniqueId, ["sibling"] = sibling.UniqueId, ["expiry"] = expiry,
                                ["future"] = Sprint27AiCommand(future), ["costs"] = costs, ["capturedEffectsDisposed"] = effects.All(buff => buff.IsDisposed),
                                ["capturedSiblingEffectsAlive"] = siblingEffects.All(buff => !buff.IsDisposed && buff.Active) },
                            "The native summon-marker scheduler invokes production retirement: only that source's captured effects and queued action end; sibling instances remain live.");
                        DateTime deadline = DateTime.UtcNow.AddSeconds(20);
                        while (DateTime.UtcNow < deadline && (!Sprints3031SourceAbsent(owner) || resources.Any(value => value != null))) {
                            Game.Instance.IsPaused = false; yield return 0;
                        }
                        var live = Game.Instance.State.Units.All.Where(unit => unit != null && !unit.Destroyed && unit.IsInState).ToArray();
                        var itemRows = new JArray(items.Select(item => OutsiderEscapeItemState(item, owner, live)));
                        check("expiry-private-teardown-" + key, Sprints3031SourceAbsent(owner) && resources.All(value => value == null) &&
                            effects.All(buff => buff.IsDisposed) && future.IsFinished && economy.Count(future) == 0 && itemRows.All(row => (bool)row["safe"]) &&
                            !trace.Rows.Skip(first).Any(row => (string)row["owner"] == owner.UniqueId &&
                                ((string)row["event"] == "attack" || (string)row["event"] == "save" || (string)row["event"] == "damage")) &&
                            siblingResources.All(value => value != null) && Sprint28PartyInventoryExact(party, partyCounts),
                            new JObject { ["owner"] = Sprints3031LifecycleState(owner), ["items"] = itemRows,
                                ["resourcesGone"] = resources.All(value => value == null), ["siblingResourcesAlive"] = siblingResources.All(value => value != null),
                                ["fixtureDisposalsBeforeAssertion"] = 0, ["fixtureInterruptsAfterExpiry"] = 0 },
                            "Native expiry leaves no actor, owned private resources, escaped items, later attack or action cost; sibling presentation and party inventory remain intact.");
                    }
                    // Refresh only the finite gaze precondition before native death;
                    // existing wound, disease and controlled-chain instances stay exact.
                    if (key == InfernalVisualPolicy34.Kyton) {
                        var gaze = siblingEffects.Single(buff => buff.Blueprint == InfernalSavedBlueprint34<BlueprintBuff>(true, "Shaken"));
                        if (gaze.IsDisposed) siblingEffects = siblingEffects.Where(buff => !ReferenceEquals(buff, gaze)).Concat(new[] {
                            InfernalOwnedEffect34(sibling, fixture.Hostile, gaze.Blueprint, TimeSpan.FromSeconds(18)) }).ToArray();
                    }
                    if (siblingEffects.Any(buff => buff.IsDisposed)) throw new InvalidOperationException("Death effect preconditions no longer live.");
                    var fatal = new PhysicalDamage(new DiceFormula(0, DiceType.Zero), PhysicalDamageForm.Slashing); fatal.AddBonus(500); fatal.AddAlignment(DamageAlignment.Good);
                    var death = Rulebook.Trigger(new RuleDealDamage(fixture.Hostile, sibling, new DamageBundle(fatal)));
                    DateTime deathDeadline = DateTime.UtcNow.AddSeconds(8);
                    while (DateTime.UtcNow < deathDeadline && (!sibling.Descriptor.State.IsDead || siblingEffects.Any(buff => !buff.IsDisposed))) {
                        Game.Instance.IsPaused = false; yield return 0;
                    }
                    check("death-owned-effects-" + key, death.Damage > 0 && sibling.Descriptor.State.IsDead && siblingEffects.All(buff => buff.IsDisposed) &&
                        siblingItems.All(item => !item.IsLootable && !Game.Instance.Player.Inventory.Items.Contains(item)),
                        new JObject { ["owner"] = sibling.UniqueId, ["nativeGoodDamage"] = death.Damage,
                            ["dead"] = sibling.Descriptor.State.IsDead, ["capturedEffectsDisposed"] = siblingEffects.All(buff => buff.IsDisposed),
                            ["directNativeDamageConsumerNotAttackDispatch"] = true },
                        "Actual fatal good damage and native death retire the exact source's ongoing effects without transferable equipment.");
                    var marker = sibling.Buffs.GetBuff(BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff);
                    if (marker != null) ExpireSprint16OwnedBuff(sibling, marker);
                    DateTime gone = DateTime.UtcNow.AddSeconds(20);
                    while (DateTime.UtcNow < gone && (!Sprints3031SourceAbsent(sibling) || siblingResources.Any(value => value != null))) {
                        Game.Instance.IsPaused = false; yield return 0;
                    }
                    check("dead-private-teardown-" + key, Sprints3031SourceAbsent(sibling) && siblingResources.All(value => value == null),
                        new JObject { ["owner"] = sibling.UniqueId, ["resourcesGone"] = siblingResources.All(value => value == null) },
                        "The native dead-summon expiry also tears down its private body resources.");
                    DisposeExpandedSummoningUnits(fixture.Created, new[] { owner, sibling }); created.Remove(owner); created.Remove(sibling);
                    ResetExpandedSummoningHostile(fixture);
                }
                foreach (int frame in InfernalSummonerAndArea34(fixture, created, check)) yield return frame;
                check("lifetime-party-inventory", Sprint28PartyInventoryExact(party, partyCounts), new JObject { ["items"] = party.Length },
                    "All lifetime cases preserve the exact party inventory and counts.");
            } finally {
                trace.Dispose(); DisposeExpandedSummoningUnits(fixture.Created, created); ResetExpandedSummoningHostile(fixture);
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory, "sprint34-infernal-lifetime.json"), receipt.ToString(Formatting.Indented));
            }
        }

        private IEnumerable<int> InfernalSummonerAndArea34(ExpandedSummoningCorrectionFixture fixture, List<UnitEntityData> created,
            Action<string, bool, JObject, string> check)
        {
            foreach (bool area in new[] { false, true }) {
                // The ordinary fixture cast supplies a separate, disposable
                // summoner context; no player actor or production SLA is changed.
                var summoner = CastExpandedSummoningOwnTier(fixture, "lillend-azata"); created.Add(summoner);
                Sprint28SetBrainActivePreservingActions(summoner, false); summoner.Commands.InterruptAll(command => true);
                foreach (int frame in WaitInfernalSupport34(summoner)) yield return frame;
                var owners = new[] { CastExpandedSummoningOwnTier(fixture, InfernalVisualPolicy34.Barbazu, summoner),
                    CastExpandedSummoningOwnTier(fixture, InfernalVisualPolicy34.Kyton, summoner) };
                created.AddRange(owners);
                foreach (var actor in owners) {
                    Sprint28SetBrainActivePreservingActions(actor, false); actor.Commands.InterruptAll(command => true);
                    foreach (int frame in WaitInfernalSupport34(actor)) yield return frame;
                }
                if (owners.Any(actor => !ReferenceEquals(actor.Get<UnitPartSummonedMonster>()?.Summoner, summoner) || !InfernalEffectLifetime34.SourceIsLive(actor)))
                    throw new InvalidOperationException("Distinct live summoner ownership precondition differs.");
                var effects = owners.SelectMany(actor => SeedInfernalLifetime34(actor, fixture.Hostile)).ToArray();
                var before = new JArray(effects.Select(InfernalSavedEffect34));
                if (area) {
                    EventBus.RaiseEvent<IPartyLeaveAreaHandler>(handler => handler.HandlePartyLeaveArea(Game.Instance.CurrentlyLoadedArea, null));
                    EventBus.RaiseEvent<IAreaLoadingStagesHandler>(handler => handler.OnAreaLoadingComplete());
                } else Kingmaker.Designers.GameHelper.KillUnit(summoner, fixture.Hostile);
                DateTime deadline = DateTime.UtcNow.AddSeconds(8);
                while (DateTime.UtcNow < deadline && effects.Any(buff => !buff.IsDisposed)) { Game.Instance.IsPaused = false; yield return 0; }
                check(area ? "area-owned-effect-consumers" : "summoner-loss-owned-effects", effects.All(buff => buff.IsDisposed) &&
                    (area || summoner.Descriptor.State.IsDead),
                    new JObject { ["summoner"] = summoner.UniqueId, ["sources"] = new JArray(owners.Select(actor => actor.UniqueId)),
                        ["before"] = before, ["capturedDisposed"] = effects.All(buff => buff.IsDisposed),
                        ["nativeEventBusConsumerNotSceneTransition"] = area, ["summonerDead"] = summoner.Descriptor.State.IsDead },
                    area ? "The actual native leave-area/loading subscribers retire all four private effect kinds; this is an event-consumer probe, not a claimed UI scene transition." :
                        "Native loss of the separately owned summoner retires its children's exact wound, disease, gaze and controlled-chain effects.");
                DisposeExpandedSummoningUnits(fixture.Created, owners.Concat(new[] { summoner }));
                foreach (var actor in owners.Concat(new[] { summoner })) created.Remove(actor);
                for (int frame = 0; frame < 8; frame++) { Game.Instance.IsPaused = false; yield return 0; }
            }
        }
    }
}
