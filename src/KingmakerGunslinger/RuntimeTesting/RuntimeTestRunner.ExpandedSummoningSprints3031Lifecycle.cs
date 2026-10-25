using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic.Buffs;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // One fixed simultaneous fixture, not a programmable case runner.
        // Expiry uses the established owned-buff scheduler boundary. No scene
        // actor, area, victim buff or item is deleted to supply a verdict.
        private IEnumerable<int> ReviewSprints3031SourceLifecycle(ExpandedSummoningCorrectionFixture fixture)
        {
            string[] keys = { "lemure", "dretch", "hound-archon", "lantern-archon", "astral-deva", "trumpet-archon" };
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue, oldPause = Game.Instance.IsPaused;
            var owners = new Dictionary<string, UnitEntityData>();
            var siblings = new Dictionary<string, UnitEntityData>();
            var resources = new Dictionary<UnitEntityData, UnityEngine.Object[]>();
            var items = new Dictionary<UnitEntityData, ItemEntity[]>();
            var boundAreas = new Dictionary<UnitEntityData, AreaEffectEntityData[]>();
            var boundGrants = new Dictionary<UnitEntityData, Buff[]>();
            var areasBefore = Game.Instance.State.AreaEffects.All.ToArray();
            var partyItems = Game.Instance.Player.Inventory.Items.ToArray();
            var partyCounts = partyItems.Select(value => value.Count).ToArray();
            var captured = new List<UnitEntityData>();
            // Retain object references while the caster is alive: a native
            // UnitReference may stop resolving after removal. A null caster
            // must not make a stranded area or victim grant invisible.
            Func<UnitEntityData, bool> grantsGone = actor =>
                boundAreas[actor].All(area => !Game.Instance.State.AreaEffects.All.Contains(area)) &&
                boundGrants[actor].All(buff => buff.IsDisposed || !buff.Active || buff.Owner.Unit.Destroyed);
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;
                foreach (string key in keys)
                {
                    owners.Add(key, CastExpandedSummoningOwnTier(fixture, key));
                    siblings.Add(key, CastExpandedSummoningOwnTier(fixture, key));
                    foreach (var actor in new[] { owners[key], siblings[key] })
                    {
                        if (!fixture.Created.Any(value => ReferenceEquals(value, actor)))
                            throw new InvalidOperationException("Lifecycle actor is not request-owned.");
                        captured.Add(actor); Sprint28SetBrainActivePreservingActions(actor, false);
                        actor.Commands.InterruptAll(true); actor.Commands.RemoveFinishedAndUpdateQueue();
                    }
                }
                var deadline = DateTime.UtcNow.AddSeconds(12);
                while (DateTime.UtcNow < deadline && captured.Any(actor =>
                    actor.View == null || !EntityFadedIn(actor) || DissolveAmount(actor) > MotionReviewIntactDissolve ||
                    actor.Buffs.GetBuff(BlueprintRoot.Instance.SystemMechanics.SummonedUnitAppearBuff) != null))
                { Game.Instance.IsPaused = false; yield return 0; }
                foreach (string key in keys)
                {
                    var pair = new[] { owners[key], siblings[key] };
                    foreach (var actor in pair)
                    {
                        resources.Add(actor, Sprints3031PrivateResources(actor, key));
                        items.Add(actor, actor.Descriptor.Inventory.Items.ToArray());
                        boundAreas.Add(actor, Game.Instance.State.AreaEffects.All.Where(area => area != null &&
                            ReferenceEquals(area.Context?.MaybeCaster, actor)).ToArray());
                        boundGrants.Add(actor, Game.Instance.State.Units.All.Where(unit => unit != null && !unit.Destroyed)
                            .SelectMany(unit => unit.Buffs.Enumerable).Where(buff =>
                                ReferenceEquals(buff.Context?.MaybeCaster, actor)).ToArray());
                    }
                    bool ready = pair.All(actor => actor.View != null && actor.IsInState && !actor.Destroyed &&
                        EntityFadedIn(actor) && DissolveAmount(actor) <= MotionReviewIntactDissolve &&
                        actor.Buffs.Enumerable.Count(buff => ReferenceEquals(buff.Blueprint,
                            BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff) &&
                            ReferenceEquals(buff.Context?.MaybeCaster, fixture.Caster) && !buff.IsPermanent &&
                            buff.TimeLeft > TimeSpan.Zero) == 1 && items[actor].All(item => !item.IsLootable)) &&
                        !resources[pair[0]].Intersect(resources[pair[1]]).Any() &&
                        (key == "astral-deva" || pair.All(actor => resources[actor].Length > 0));
                    if (key == "hound-archon" || key == "astral-deva" || key == "trumpet-archon")
                        ready &= pair.All(actor => actor.Body.PrimaryHand.MaybeWeapon != null &&
                            ReferenceEquals(actor.Body.PrimaryHand.MaybeWeapon.Owner, actor.Descriptor) &&
                            !actor.Body.PrimaryHand.MaybeWeapon.IsLootable);
                    if (key == "hound-archon" || key == "lantern-archon" || key == "astral-deva" || key == "trumpet-archon")
                        ready &= pair.All(actor => Sprints3031OwnedAreaCount(actor) > 0 && Sprints3031LiveSourceGrantCount(actor) > 0);
                    var row = new JObject { ["creature"] = key, ["owners"] = new JArray(pair.Select(Sprints3031LifecycleState)),
                        ["privateObjects"] = new JArray(pair.Select(actor => resources[actor].Length)),
                        ["independentPrivateObjects"] = !resources[pair[0]].Intersect(resources[pair[1]]).Any(),
                        ["initialItems"] = new JArray(pair.SelectMany(actor => items[actor].Select(item => new JObject {
                            ["owner"] = actor.UniqueId, ["item"] = item.Blueprint.AssetGuid, ["lootable"] = item.IsLootable }))) };
                    Sprint25Check("lifecycle-" + key + "-ready", ready, row,
                        "Two actual own-tier summons have finite native summon ownership, intact views, independent private resources and nonlootable equipment.");
                    if (!ready) throw new InvalidOperationException("Lifecycle fixture failed: " + key);
                }
                var siblingAreas = Game.Instance.State.AreaEffects.All.Where(area => area != null &&
                    siblings.Values.Any(actor => ReferenceEquals(area.Context?.MaybeCaster, actor))).ToArray();
                var expiry = new Dictionary<string, JObject>();
                foreach (string key in keys)
                {
                    var actor = owners[key];
                    expiry.Add(key, ExpireSprint16OwnedBuff(actor, actor.Buffs.Enumerable.Single(buff =>
                        ReferenceEquals(buff.Blueprint, BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff))));
                }
                // Real scheduler/entity/view updates must settle before observing
                // absence. This wait never removes an area, item or victim grant.
                deadline = DateTime.UtcNow.AddSeconds(18);
                while (DateTime.UtcNow < deadline && owners.Values.Any(actor => !Sprints3031SourceAbsent(actor) ||
                    !grantsGone(actor) || resources[actor].Any(value => value != null)))
                { Game.Instance.IsPaused = false; yield return 0; }
                for (int frame = 0; frame < 12; frame++) yield return 0;
                var live = Game.Instance.State.Units.All.Where(actor => actor != null && !actor.Destroyed && actor.IsInState).ToArray();
                foreach (string key in keys)
                {
                    var actor = owners[key]; var sibling = siblings[key];
                    var itemRows = new JArray(items[actor].Select(item => OutsiderEscapeItemState(item, actor, live)));
                    bool absent = Sprints3031SourceAbsent(actor) && resources[actor].All(value => value == null) &&
                        grantsGone(actor) && itemRows.All(row => (bool)row["safe"]);
                    bool isolated = sibling.IsInState && !sibling.Destroyed && sibling.View != null &&
                        resources[sibling].All(value => value != null) &&
                        items[sibling].SequenceEqual(sibling.Descriptor.Inventory.Items) &&
                        siblingAreas.All(area => Game.Instance.State.AreaEffects.All.Contains(area));
                    Sprint25Check("lifecycle-" + key + "-native-expiry", absent && isolated,
                        new JObject { ["creature"] = key, ["expiry"] = expiry[key], ["expired"] = Sprints3031LifecycleState(actor),
                            ["sibling"] = Sprints3031LifecycleState(sibling), ["items"] = itemRows,
                            ["privateResourcesAbsent"] = resources[actor].All(value => value == null),
                            ["capturedAreaAndGrantReferencesGone"] = grantsGone(actor),
                            ["capturedAreas"] = boundAreas[actor].Length, ["capturedGrants"] = boundGrants[actor].Length,
                            ["siblingResourcesAlive"] = resources[sibling].All(value => value != null),
                            ["allSiblingAreasRetained"] = siblingAreas.All(area => Game.Instance.State.AreaEffects.All.Contains(area)),
                            ["fixtureDisposalsBeforeAssertion"] = 0 },
                        "Native scheduled expiry removes only the selected summon, its emitter areas/grants and private view objects; captured equipment cannot escape and all six siblings retain their areas/items/resources.");
                    if (!absent || !isolated) throw new InvalidOperationException("Native expiry/isolation failed: " + key);
                }
                foreach (var actor in siblings.Values) Kingmaker.Designers.GameHelper.KillUnit(actor, fixture.Caster);
                deadline = DateTime.UtcNow.AddSeconds(10);
                while (DateTime.UtcNow < deadline && siblings.Values.Any(actor =>
                    !actor.Descriptor.State.IsDead || !grantsGone(actor) || Sprints3031OwnedAreaCount(actor) != 0 || Sprints3031LiveSourceGrantCount(actor) != 0))
                { Game.Instance.IsPaused = false; yield return 0; }
                foreach (string key in keys)
                {
                    var actor = siblings[key];
                    bool dead = actor.Descriptor.State.IsDead && Sprints3031OwnedAreaCount(actor) == 0 &&
                        grantsGone(actor) && Sprints3031LiveSourceGrantCount(actor) == 0 && items[actor].All(item => !item.IsLootable &&
                            !Game.Instance.Player.Inventory.Items.Contains(item));
                    Sprint25Check("lifecycle-" + key + "-native-death", dead,
                        new JObject { ["creature"] = key, ["actor"] = Sprints3031LifecycleState(actor),
                            ["allEquipmentNonlootable"] = items[actor].All(item => !item.IsLootable),
                            ["capturedAreaAndGrantReferencesGone"] = grantsGone(actor),
                            ["fixtureAreaOrGrantRemoval"] = 0 },
                        "Native KillUnit on the disposable sibling closes emitter-owned areas and recipient grants while its equipment remains nonlootable. No blanket effect deletion supplies the result.");
                    if (!dead) throw new InvalidOperationException("Native death aura/item boundary failed: " + key);
                    var marker = actor.Buffs.Enumerable.SingleOrDefault(buff => ReferenceEquals(buff.Blueprint,
                        BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff));
                    if (marker != null) ExpireSprint16OwnedBuff(actor, marker);
                }
                deadline = DateTime.UtcNow.AddSeconds(18);
                while (DateTime.UtcNow < deadline && siblings.Values.Any(actor => !Sprints3031SourceAbsent(actor) ||
                    !grantsGone(actor) || resources[actor].Any(value => value != null)))
                { Game.Instance.IsPaused = false; yield return 0; }
                for (int frame = 0; frame < 12; frame++) yield return 0;
                live = Game.Instance.State.Units.All.Where(actor => actor != null && !actor.Destroyed && actor.IsInState).ToArray();
                foreach (string key in keys)
                {
                    var actor = siblings[key];
                    var itemRows = new JArray(items[actor].Select(item => OutsiderEscapeItemState(item, actor, live)));
                    bool exact = Sprints3031SourceAbsent(actor) && resources[actor].All(value => value == null) &&
                        grantsGone(actor) && itemRows.All(row => (bool)row["safe"]);
                    Sprint25Check("lifecycle-" + key + "-dead-expiry", exact,
                        new JObject { ["creature"] = key, ["actor"] = Sprints3031LifecycleState(actor), ["items"] = itemRows,
                            ["privateResourcesAbsent"] = resources[actor].All(value => value == null) },
                        "The dead summon also leaves no live source, aura, recipient grant, private object or transferable equipment after native expiry.");
                    if (!exact) throw new InvalidOperationException("Dead summon expiry failed: " + key);
                }
                bool reset = captured.All(Sprints3031SourceAbsent) && captured.All(grantsGone) &&
                    Game.Instance.State.AreaEffects.All.SequenceEqual(areasBefore) &&
                    partyItems.SequenceEqual(Game.Instance.Player.Inventory.Items) &&
                    partyCounts.SequenceEqual(partyItems.Select(value => value.Count));
                Sprint25Check("lifecycle-batch-reset", reset, new JObject { ["sources"] = captured.Count,
                    ["areasBefore"] = areasBefore.Length, ["areasAfter"] = Game.Instance.State.AreaEffects.All.Count(),
                    ["partyItemsExact"] = partyItems.SequenceEqual(Game.Instance.Player.Inventory.Items),
                    ["partyCountsExact"] = partyCounts.SequenceEqual(partyItems.Select(value => value.Count)),
                    ["nativeProcesses"] = 1, ["fixtureDisposalsBeforeAssertions"] = 0 },
                    "All twelve native lifecycle paths finish in the same fixed fixture with exact initial areas and party inventory; later cases are not marked passed after a failed precondition.");
                if (!reset) throw new InvalidOperationException("Lifecycle batch reset failed.");
            }
            finally
            {
                DisposeExpandedSummoningUnits(fixture.Created, captured);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode;
                Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = oldPause;
            }
        }

        private static UnityEngine.Object[] Sprints3031PrivateResources(UnitEntityData actor, string key)
        {
            if (actor.View == null) return Array.Empty<UnityEngine.Object>();
            if (key == "lemure" || key == "dretch" || key == "hound-archon")
                return ExpandedSummoningPteranodonViewPatch.CaptureTheropodOwnedResources(actor.View);
            if (key == "lantern-archon")
                return actor.View.GetComponent<LanternArchonOrbView>()?.CaptureOwnedResources() ?? Array.Empty<UnityEngine.Object>();
            if (key == "trumpet-archon") return Trumpet31InstrumentOwnedResources(actor);
            return Array.Empty<UnityEngine.Object>(); // Astral retains its native body, no original attachment.
        }
        private static int Sprints3031OwnedAreaCount(UnitEntityData actor)
        {
            return Game.Instance.State.AreaEffects.All.Count(area => area != null && ReferenceEquals(area.Context?.MaybeCaster, actor));
        }
        private static int Sprints3031LiveSourceGrantCount(UnitEntityData actor)
        {
            return Game.Instance.State.Units.All.Where(unit => unit != null && !unit.Destroyed && unit.IsInState)
                .Sum(unit => unit.Buffs.Enumerable.Count(buff => ReferenceEquals(buff.Context?.MaybeCaster, actor)));
        }
        private static bool Sprints3031SourceAbsent(UnitEntityData actor)
        {
            return actor.Destroyed && !actor.IsInState && actor.View == null && actor.HoldingState == null &&
                Sprints3031OwnedAreaCount(actor) == 0 && Sprints3031LiveSourceGrantCount(actor) == 0;
        }
        private static JObject Sprints3031LifecycleState(UnitEntityData actor)
        {
            return new JObject { ["owner"] = actor.UniqueId, ["blueprint"] = actor.Blueprint.AssetGuid,
                ["destroyed"] = actor.Destroyed, ["inState"] = actor.IsInState, ["dead"] = actor.Descriptor.State.IsDead,
                ["viewPresent"] = actor.View != null, ["holdingStatePresent"] = actor.HoldingState != null,
                ["ownedAreas"] = Sprints3031OwnedAreaCount(actor), ["liveSourceGrants"] = Sprints3031LiveSourceGrantCount(actor),
                ["unfinishedRawCommands"] = actor.Commands.Raw.Count(command => command != null && !command.IsFinished),
                ["commandsNote"] = "Snapshot only; action retirement requires the calling case's attributed command/queue observations." };
        }
    }
}
