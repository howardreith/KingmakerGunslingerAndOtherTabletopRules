using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
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
        private IEnumerable<int> ReviewSprints3031SourceLifecycle(ExpandedSummoningCorrectionFixture fixture, bool sprint32 = false)
        {
            string[] keys = sprint32 ? new[] { "satyr", "lillend-azata" } :
                new[] { "lemure", "dretch", "hound-archon", "lantern-archon", "astral-deva", "trumpet-archon" };
            if (sprint32 && (!IsSprint32Review || RuntimeTestRequestParser.Sprint32ReviewPhase(_request) !=
                ExpandedSummoningNativeProfilePolicy.Sprint32LifecyclePhase))
                throw new InvalidOperationException("Sprint32 lifecycle requires its exact fixed phase.");
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
            Func<UnitEntityData, Buff[]> courageRecipients = actor => fixture.Caster.Buffs.Enumerable.Where(buff =>
                buff.Active && !buff.IsDisposed && buff.Blueprint.name == "KMG_Summoning_FeyCelestial_LillendAzata_CourageRecipient" &&
                ReferenceEquals(buff.Context?.MaybeCaster, actor)).ToArray();
            var capturedCourageRecipients = new Dictionary<UnitEntityData, Buff>();
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
                if (sprint32)
                {
                    // Explicit lifecycle precondition, not AI selection evidence:
                    // use the proven owned combat/native command path, allowing
                    // normal controllers to create the carrier/area/recipient.
                    // The recipient is the persistent request-owned caster,
                    // which survives every source removal. Only one identical
                    // emitter is ON at a time: no stacking replacement can hide
                    // a stranded external recipient behind source destruction.
                    var actor = owners["lillend-azata"];
                    PlaceExpandedSummoningUnit(actor, fixture.Caster.Position + Vector3.right * 4f);
                    PlaceExpandedSummoningUnit(siblings["lillend-azata"], fixture.Caster.Position + Vector3.forward * 40f);
                    foreach (int frame in PrepareLillend32LifecycleCourage(fixture, actor, courageRecipients, "expiry")) yield return frame;
                    var recipients = courageRecipients(actor);
                    if (recipients.Length != 1) throw new InvalidOperationException("Lifecycle requires the exact Courage recipient on the surviving owned caster.");
                    capturedCourageRecipients.Add(actor, recipients.Single());
                }
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
                        (key == "astral-deva" || key == "satyr" || pair.All(actor => resources[actor].Length > 0));
                    if (key == "hound-archon" || key == "astral-deva" || key == "trumpet-archon" || sprint32)
                        ready &= pair.All(actor => actor.Body.PrimaryHand.MaybeWeapon != null &&
                            ReferenceEquals(actor.Body.PrimaryHand.MaybeWeapon.Owner, actor.Descriptor) &&
                            !actor.Body.PrimaryHand.MaybeWeapon.IsLootable);
                    if (key == "hound-archon" || key == "lantern-archon" || key == "astral-deva" || key == "trumpet-archon")
                        ready &= pair.All(actor => Sprints3031OwnedAreaCount(actor) > 0 && Sprints3031LiveSourceGrantCount(actor) > 0);
                    if (key == "lillend-azata")
                        ready &= Sprints3031OwnedAreaCount(pair[0]) > 0 && capturedCourageRecipients.ContainsKey(pair[0]) &&
                            boundGrants[pair[0]].Contains(capturedCourageRecipients[pair[0]]) &&
                            Sprints3031OwnedAreaCount(pair[1]) == 0 && fixture.Caster.IsInState && !fixture.Caster.Descriptor.State.IsDead;
                    var row = new JObject { ["creature"] = key, ["owners"] = new JArray(pair.Select(Sprints3031LifecycleState)),
                        ["performanceSetup"] = key == "lillend-azata" ? "One owned native activation command in an isolated combat; exact CourageRecipient captured on surviving fixture caster. Sibling remains OFF until the death case. No AI selection claim." : null,
                        ["externalRecipient"] = key == "lillend-azata" ? fixture.Caster.UniqueId : null,
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
                    if (key == "lillend-azata") absent &= fixture.Caster.IsInState && !fixture.Caster.Descriptor.State.IsDead &&
                        (capturedCourageRecipients[actor].IsDisposed || !capturedCourageRecipients[actor].Active);
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
                            ["externalRecipientAlive"] = key == "lillend-azata" ? (bool?)(fixture.Caster.IsInState && !fixture.Caster.Descriptor.State.IsDead) : null,
                            ["capturedExternalGrantGone"] = key == "lillend-azata" ? (bool?)(capturedCourageRecipients[actor].IsDisposed || !capturedCourageRecipients[actor].Active) : null,
                            ["siblingResourcesAlive"] = resources[sibling].All(value => value != null),
                            ["allSiblingAreasRetained"] = siblingAreas.All(area => Game.Instance.State.AreaEffects.All.Contains(area)),
                            ["fixtureDisposalsBeforeAssertion"] = 0 },
                        "Native scheduled expiry removes only the selected summon, its emitter areas/grants and private view objects; captured equipment cannot escape and all scoped siblings retain their areas/items/resources.");
                    if (!absent || !isolated) throw new InvalidOperationException("Native expiry/isolation failed: " + key);
                }
                if (sprint32)
                {
                    var actor = siblings["lillend-azata"];
                    PlaceExpandedSummoningUnit(actor, fixture.Caster.Position + Vector3.right * 4f);
                    foreach (int frame in PrepareLillend32LifecycleCourage(fixture, actor, courageRecipients, "death")) yield return frame;
                    var recipients = courageRecipients(actor);
                    boundAreas[actor] = Game.Instance.State.AreaEffects.All.Where(area => area != null && ReferenceEquals(area.Context?.MaybeCaster, actor)).ToArray();
                    boundGrants[actor] = Game.Instance.State.Units.All.Where(unit => unit != null && !unit.Destroyed)
                        .SelectMany(unit => unit.Buffs.Enumerable).Where(buff => ReferenceEquals(buff.Context?.MaybeCaster, actor)).ToArray();
                    bool ready = recipients.Length == 1 && boundAreas[actor].Length > 0 && boundGrants[actor].Contains(recipients.SingleOrDefault()) &&
                        fixture.Caster.IsInState && !fixture.Caster.Descriptor.State.IsDead;
                    Sprint25Check("lifecycle-lillend-azata-death-recipient-ready", ready, new JObject {
                        ["source"] = actor.UniqueId, ["recipient"] = fixture.Caster.UniqueId,
                        ["exactExternalGrants"] = recipients.Length, ["capturedAreas"] = boundAreas[actor].Length },
                        "The surviving caster receives the sibling's exact native Courage effect after the expired owner's grant is gone; capture references before native death.");
                    if (!ready) throw new InvalidOperationException("Death lifecycle requires a live exact external Courage recipient.");
                    capturedCourageRecipients.Add(actor, recipients.Single());
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
                    if (key == "lillend-azata") dead &= fixture.Caster.IsInState && !fixture.Caster.Descriptor.State.IsDead &&
                        (capturedCourageRecipients[actor].IsDisposed || !capturedCourageRecipients[actor].Active);
                    Sprint25Check("lifecycle-" + key + "-native-death", dead,
                        new JObject { ["creature"] = key, ["actor"] = Sprints3031LifecycleState(actor),
                            ["allEquipmentNonlootable"] = items[actor].All(item => !item.IsLootable),
                            ["capturedAreaAndGrantReferencesGone"] = grantsGone(actor),
                            ["externalRecipientAlive"] = key == "lillend-azata" ? (bool?)(fixture.Caster.IsInState && !fixture.Caster.Descriptor.State.IsDead) : null,
                            ["capturedExternalGrantGone"] = key == "lillend-azata" ? (bool?)(capturedCourageRecipients[actor].IsDisposed || !capturedCourageRecipients[actor].Active) : null,
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
                    "All scoped native lifecycle paths finish in the same fixed fixture with exact initial areas and party inventory; later cases are not marked passed after a failed precondition.");
                if (!reset) throw new InvalidOperationException("Lifecycle batch reset failed.");
            }
            finally
            {
                DisposeExpandedSummoningUnits(fixture.Created, captured);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode;
                Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = oldPause;
            }
        }

        private IEnumerable<int> PrepareLillend32LifecycleCourage(ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData actor, Func<UnitEntityData, Buff[]> recipients, string stage)
        {
            // Bounded setup for source removal, not a substitute for natural AI
            // evidence. Keep the owned encounter alive while each source expires
            // or dies, so combat-end cancellation cannot pre-empt the tested cause.
            var target = fixture.Hostile;
            if (target == null || fixture.OwnedHostileFaction == null)
                throw new InvalidOperationException("Lifecycle combat requires its isolated owned hostile.");
            PlaceExpandedSummoningUnit(target, fixture.Caster.Position + Vector3.forward * 8f);
            foreach (var unit in new[] { actor, fixture.Caster, target })
            {
                unit.Wake(0f);
                if (!Game.Instance.State.AwakeUnits.Contains(unit)) Game.Instance.State.AwakeUnits.Add(unit);
                unit.Memory.Add(ReferenceEquals(unit, target) ? fixture.Caster : target);
                unit.SightCache.ClearIfNeeded(); unit.JoinCombat();
            }
            new Kingmaker.Controllers.Units.UnitMemoryController().Tick();
            Game.Instance.Player.UpdateIsInCombat();
            new UnitCombatJoinController().Tick(); new UnitCombatPrepareController().Tick();
            var courage = actor.Blueprint.Brain.Actions.OfType<LillendCourageAi32>().Single().Courage;
            var performance = actor.Descriptor.ActivatableAbilities.Enumerable.Single(value => ReferenceEquals(value.Blueprint, courage));
            var resource = fixture.Blueprints.OfType<Kingmaker.Blueprints.BlueprintAbilityResource>().Single(value =>
                value.name == "KMG_Summoning_FeyCelestial_LillendAzata_PerformanceResource");
            var command = new UnitActivateAbility(performance);
            bool admitted = actor.Descriptor.State.CanAct && actor.IsInCombat && fixture.Caster.IsInCombat &&
                !actor.IsEnemy(fixture.Caster) && actor.HasLOS(fixture.Caster) && performance.IsAvailable &&
                !performance.IsOn && !performance.IsRunning;
            if (admitted) actor.Commands.Run(command);
            var deadline = DateTime.UtcNow.AddSeconds(12);
            while (admitted && DateTime.UtcNow < deadline &&
                (Sprints3031OwnedAreaCount(actor) == 0 || recipients(actor).Length != 1))
            { Game.Instance.IsPaused = false; yield return 0; }
            bool ready = admitted && performance.IsOn && performance.IsRunning &&
                Sprints3031OwnedAreaCount(actor) > 0 && recipients(actor).Length == 1;
            Sprint25Check("lifecycle-lillend-" + stage + "-activation-ready", ready, new JObject {
                ["source"] = actor.UniqueId, ["recipient"] = fixture.Caster.UniqueId,
                ["admitted"] = admitted, ["canAct"] = actor.Descriptor.State.CanAct,
                ["awake"] = Game.Instance.State.AwakeUnits.Contains(actor), ["inCombat"] = actor.IsInCombat,
                ["recipientInCombat"] = fixture.Caster.IsInCombat, ["hostileInCombat"] = target.IsInCombat,
                ["allyLos"] = actor.HasLOS(fixture.Caster), ["distanceMeters"] = actor.DistanceTo(fixture.Caster),
                ["isOn"] = performance.IsOn, ["isRunning"] = performance.IsRunning, ["available"] = performance.IsAvailable,
                ["rounds"] = actor.Descriptor.Resources.GetResourceAmount(resource),
                ["command"] = new JObject { ["started"] = command.IsStarted, ["acted"] = command.IsActed,
                    ["finished"] = command.IsFinished, ["result"] = command.Result.ToString() },
                ["areas"] = Sprints3031OwnedAreaCount(actor),
                ["exactExternalRecipients"] = recipients(actor).Length,
                ["sourceState"] = Sprints3031LifecycleState(actor), ["suppliedEffectsOrCosts"] = false },
                "A fixture-issued native activation in the owned encounter establishes the exact external recipient before source removal; state is retained on failure.");
            if (!ready) throw new InvalidOperationException("Lifecycle Courage activation prerequisite failed: " + stage);
        }

        private static UnityEngine.Object[] Sprints3031PrivateResources(UnitEntityData actor, string key)
        {
            if (actor.View == null) return Array.Empty<UnityEngine.Object>();
            if (key == "lillend-azata")
            {
                var attachment = actor.View.GetComponent<LillendHumanVisualAttachment>();
                return attachment == null ? Array.Empty<UnityEngine.Object>() :
                    attachment.CaptureOwnedResources().Concat(new UnityEngine.Object[] { attachment }).ToArray();
            }
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
