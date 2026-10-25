using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private sealed class OutsiderEscapeEvents : IGlobalRulebookHandler<RuleSkillCheck>,
            IGlobalRulebookHandler<RuleCalculateCMB>, IGlobalRulebookHandler<RuleCombatManeuver>
        {
            private readonly UnitEntityData _actor;
            internal readonly List<RuleSkillCheck> Skills = new List<RuleSkillCheck>();
            internal readonly List<RuleCalculateCMB> Cmb = new List<RuleCalculateCMB>();
            internal readonly List<RuleCombatManeuver> Maneuvers = new List<RuleCombatManeuver>();
            internal OutsiderEscapeEvents(UnitEntityData actor) { _actor = actor; }
            public void OnEventAboutToTrigger(RuleSkillCheck evt) { }
            public void OnEventDidTrigger(RuleSkillCheck evt) { if (ReferenceEquals(evt.Initiator, _actor)) Skills.Add(evt); }
            public void OnEventAboutToTrigger(RuleCalculateCMB evt) { }
            public void OnEventDidTrigger(RuleCalculateCMB evt) { if (ReferenceEquals(evt.Initiator, _actor)) Cmb.Add(evt); }
            public void OnEventAboutToTrigger(RuleCombatManeuver evt) { }
            public void OnEventDidTrigger(RuleCombatManeuver evt) { if (ReferenceEquals(evt.Initiator, _actor)) Maneuvers.Add(evt); }
        }
        private static JObject OutsiderEscapeSkill(RuleSkillCheck check)
        {
            return new JObject { ["stat"] = check.StatType.ToString(), ["dc"] = check.DC,
                ["statValue"] = check.StatValue, ["eventBonus"] = (int)check.Bonus,
                ["difficultyDcAdjustment"] = check.DifficultyDCAdjustment, ["ensureSuccess"] = check.EnsureSuccess,
                ["take10"] = check.Take10ForSuccess, ["d20"] = check.D20.Value, ["baseRoll"] = check.BaseRollResult, ["passed"] = check.IsPassed,
                ["effectiveBonus"] = check.BaseRollResult - check.D20.Value,
                ["sources"] = new JArray(check.Bonus.Modifiers.Select(modifier => new JObject {
                    ["value"] = modifier.ModValue, ["source"] = modifier.Source?.Blueprint?.AssetGuid })) };
        }
        private void ObserveOutsiderEscape(UnitEntityData actor, UnitEntityData holder,
            string id, int strength, int dexterity, int? expectedSkill, int dc, Func<JObject> resources = null)
        {
            actor.Stats.Strength.BaseValue = strength; actor.Stats.Dexterity.BaseValue = dexterity;
            var resourcesBefore = resources?.Invoke() ?? new JObject();
            int raw = actor.Stats.SkillMobility.ModifiedValue;
            var ordinaryBefore = Rulebook.Trigger(new RuleSkillCheck(actor, StatType.SkillMobility, -1000));
            var events = new OutsiderEscapeEvents(actor); bool escaped;
            EventBus.Subscribe(events);
            try { escaped = UnitHelper.TryBreakFree(actor, holder, UnitHelper.BreakFreeFlags.Default, null, dc); }
            finally { EventBus.Unsubscribe(events); }
            var ordinaryAfter = Rulebook.Trigger(new RuleSkillCheck(actor, StatType.SkillMobility, -1000));
            bool skillPath = expectedSkill.HasValue && events.Skills.Count == 1 && events.Maneuvers.Count == 0 &&
                events.Skills[0].StatType == StatType.SkillMobility &&
                events.Skills[0].BaseRollResult - events.Skills[0].D20.Value == expectedSkill.Value &&
                events.Skills[0].StatValue + (int)events.Skills[0].Bonus == expectedSkill.Value &&
                escaped == events.Skills[0].IsPassed && (dc < 0 ? escaped : !escaped);
            bool cmbPath = !expectedSkill.HasValue && events.Skills.Count == 0 && events.Maneuvers.Count == 1 &&
                events.Cmb.Count > 0 && escaped == events.Maneuvers[0].Success;
            bool ordinary = actor.Stats.SkillMobility.ModifiedValue == raw &&
                ordinaryBefore.BaseRollResult - ordinaryBefore.D20.Value == raw &&
                ordinaryAfter.BaseRollResult - ordinaryAfter.D20.Value == raw;
            var power = actor.Descriptor.ActivatableAbilities.Enumerable.FirstOrDefault(value =>
                value.Blueprint.AssetGuid == "a7b339e4f6ff93a4697df5d7a87ff619");
            bool powerOff = power == null || !power.IsOn && !power.IsRunning && actor.Buffs.GetBuff(power.Blueprint.Buff) == null;
            var resourcesAfter = resources?.Invoke() ?? new JObject();
            bool passed = (skillPath || cmbPath) && ordinary && powerOff && JToken.DeepEquals(resourcesBefore, resourcesAfter);
            Sprint25Check("outsider-escape-" + id, passed, new JObject { ["actor"] = actor.UniqueId,
                ["blueprint"] = actor.Blueprint.AssetGuid, ["holder"] = holder.UniqueId,
                ["strength"] = actor.Stats.Strength.ModifiedValue, ["dexterity"] = actor.Stats.Dexterity.ModifiedValue,
                ["expectedEscapeArtist"] = expectedSkill, ["rawMobility"] = raw, ["returned"] = escaped,
                ["checks"] = new JArray(events.Skills.Select(OutsiderEscapeSkill)),
                ["nativeCmb"] = new JArray(events.Cmb.Select(check => check.Result)),
                ["nativeManeuvers"] = new JArray(events.Maneuvers.Select(check => new JObject { ["success"] = check.Success })),
                ["ordinaryBefore"] = OutsiderEscapeSkill(ordinaryBefore), ["ordinaryAfter"] = OutsiderEscapeSkill(ordinaryAfter),
                ["powerAttackOff"] = powerOff, ["resourcesBefore"] = resourcesBefore, ["resourcesAfter"] = resourcesAfter, ["nativeHelperCalls"] = 1, ["forcedRolls"] = 0,
                ["scope"] = "Actual native break-free rule consumer; no held-state/action-economy or autonomous escape claim" },
                "Native skill branches consume real attributed Escape Artist rolls; the adult CMB row is a native routing/outcome control. Ordinary Mobility remains unchanged; no held-state/action claim.");
            if (!passed) throw new InvalidOperationException("Native outsider escape consumer failed: " + id);
        }
        private static JObject OutsiderEscapeItemState(ItemEntity item, UnitEntityData original, UnitEntityData[] live)
        {
            var slot = item.HoldingSlot; var slotOwner = slot?.Owner?.Unit;
            var wielder = item.Wielder?.Unit;
            bool slotPoints = slot != null && ReferenceEquals(slot.MaybeItem, item);
            bool inactiveSlot = !slotPoints || ReferenceEquals(slotOwner, original) && original.Destroyed && !original.IsInState;
            bool inactiveWielder = wielder == null || ReferenceEquals(wielder, original) && original.Destroyed && !original.IsInState;
            bool collectionContains = item.Collection != null && item.Collection.Items.Contains(item);
            bool inactiveCollection = !collectionContains || ReferenceEquals(item.Collection.Owner, original.Descriptor) && original.Destroyed && !original.IsInState;
            var liveInventories = live.Where(unit => unit.Descriptor.Inventory.Items.Contains(item)).Select(unit => unit.UniqueId).ToArray();
            bool partyContains = Game.Instance.Player.Inventory.Items.Contains(item);
            bool safe = inactiveCollection && inactiveSlot && inactiveWielder && liveInventories.Length == 0 && !partyContains && !item.IsLootable;
            return new JObject { ["object"] = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(item),
                ["blueprint"] = item.Blueprint.AssetGuid, ["createdOwner"] = original.UniqueId,
                ["createdOwnerDestroyed"] = original.Destroyed, ["createdOwnerInState"] = original.IsInState,
                ["collectionNull"] = item.Collection == null, ["collectionContains"] = collectionContains,
                ["collectionOwner"] = item.Collection?.Owner?.Unit?.UniqueId, ["inactiveOwnedCollection"] = inactiveCollection,
                ["slotNull"] = slot == null, ["slotPointsToItem"] = slotPoints, ["slotOwner"] = slotOwner?.UniqueId,
                ["inactiveOwnedSlot"] = inactiveSlot, ["inactiveOwnedWielder"] = inactiveWielder,
                ["wielder"] = wielder?.UniqueId, ["liveInventories"] = new JArray(liveInventories),
                ["partyContains"] = partyContains, ["lootable"] = item.IsLootable, ["safe"] = safe };
        }
        private IEnumerable<int> ReviewOutsiderEscape30And31(ExpandedSummoningCorrectionFixture fixture)
        {
            var completed = new List<UnitEntityData>(); var items = new List<ItemEntity>();
            var itemOwners = new Dictionary<ItemEntity, UnitEntityData>();
            var sceneBaseline = SnapshotReferences(fixture.SceneEntities);
            var targetBuffs = fixture.Hostile.Buffs.RawFacts.Select(value => value.Blueprint.AssetGuid).OrderBy(value => value, StringComparer.Ordinal).ToArray();
            var casterBuffs = fixture.Caster.Buffs.RawFacts.Select(value => value.Blueprint.AssetGuid).OrderBy(value => value, StringComparer.Ordinal).ToArray();
            var areas = Game.Instance.State.AreaEffects.All.ToArray();
            var partyItems = Game.Instance.Player.Inventory.Items.ToArray();
            var partyCounts = partyItems.Select(item => item.Count).ToArray();
            UnitEntityData dretch = null, trumpet = null, control = null;
            try
            {
                dretch = CastExpandedSummoningOwnTier(fixture, "dretch");
                trumpet = CastExpandedSummoningOwnTier(fixture, "trumpet-archon");
                control = CastExpandedSummoningOwnTier(fixture, "lemure");
                foreach (var actor in new[] { dretch, trumpet, control })
                {
                    completed.Add(actor); items.AddRange(actor.Descriptor.Inventory.Items);
                    foreach (var item in actor.Descriptor.Inventory.Items) itemOwners.Add(item, actor);
                    Sprint28SetBrainActivePreservingActions(actor, false); actor.Commands.InterruptAll(true);
                }
                var ownershipDeadline = DateTime.UtcNow.AddSeconds(5);
                while (DateTime.UtcNow < ownershipDeadline && items.Any(item => item.IsLootable)) yield return 0;
                bool ownedReady = items.Count > 0 && items.All(item => !item.IsLootable);
                Sprint25Check("outsider-escape-owned-item-preconditions", ownedReady,
                    new JObject { ["items"] = new JArray(items.Select(item => new JObject {
                        ["object"] = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(item),
                        ["blueprint"] = item.Blueprint.AssetGuid, ["createdOwner"] = itemOwners[item].UniqueId,
                        ["lootable"] = item.IsLootable, ["nonRemovable"] = item.IsNonRemovable })) },
                    "All observed helper equipment is summon-owned and nonlootable before rule probes; no fixture clears loot protections.");
                if (!ownedReady) throw new InvalidOperationException("Escape helper equipment ownership preconditions failed.");
                var liveBefore = Game.Instance.State.Units.All.Where(unit => unit != null && !unit.Destroyed && unit.IsInState).ToArray();
                var liveItems = new JArray(items.Select(item => OutsiderEscapeItemState(item, itemOwners[item], liveBefore)));
                bool rejectsLive = completed.All(actor => !actor.Destroyed && actor.IsInState) && liveItems.All(row => !(bool)row["safe"]);
                Sprint25Check("outsider-escape-reset-rejects-live-items", rejectsLive,
                    new JObject { ["itemStates"] = liveItems },
                    "The same actual reachability observer rejects every live owned helper item before cleanup; nonloot flags alone cannot make a residue pass.");
                if (!rejectsLive) throw new InvalidOperationException("Escape item reset observer failed its live-residue negative control.");
                foreach (int frame in Sprint22SetPowerAttack(trumpet, false)) yield return frame;
                var fear = fixture.Blueprints.OfType<BlueprintAbilityResource>().Single(value => value.name == "KMG_Summoning_OutsiderCelestial_Dretch_CauseFearResource");
                var cloud = fixture.Blueprints.OfType<BlueprintAbilityResource>().Single(value => value.name == "KMG_Summoning_OutsiderCelestial_Dretch_StinkingCloudResource");
                Func<JObject> dretchResources = () => new JObject { ["fear"] = dretch.Descriptor.Resources.GetResourceAmount(fear),
                    ["cloud"] = dretch.Descriptor.Resources.GetResourceAmount(cloud) };
                var preparation = trumpet.Blueprint.GetComponent<TrumpetPreparedBook>();
                var book = trumpet.Descriptor.GetSpellbook(preparation.Book);
                Func<JObject> trumpetResources = () => new JObject { ["cl"] = book.CasterLevel,
                    ["preparations"] = new JArray(preparation.Preparations.Select(row => new JObject {
                        ["spell"] = row.Ability.AssetGuid, ["level"] = row.Level, ["copies"] = row.Copies,
                        ["available"] = book.GetMemorizedSpellSlots(row.Level).Count(slot => slot.Available && ReferenceEquals(slot.Spell?.Blueprint, row.Ability)) })) };
                ObserveOutsiderEscape(dretch, fixture.Hostile, "dretch-adult", 12, 10, 5, -1000, dretchResources);
                ObserveOutsiderEscape(dretch, fixture.Hostile, "dretch-negative-dexterity", 12, 6, 3, -1000, dretchResources);
                ObserveOutsiderEscape(dretch, fixture.Hostile, "dretch-real-failed-check", 12, 10, 5, 1000, dretchResources);
                ObserveOutsiderEscape(trumpet, fixture.Hostile, "trumpet-adult-cmb", 20, 17, null, -1000, trumpetResources);
                ObserveOutsiderEscape(trumpet, fixture.Hostile, "trumpet-escape-artist", 6, 17, 17, -1000, trumpetResources);
                ObserveOutsiderEscape(trumpet, fixture.Hostile, "trumpet-negative-dexterity", 6, 9, 13, -1000, trumpetResources);
                ObserveOutsiderEscape(control, fixture.Hostile, "unconfigured-lemure-control", 6, 18, 4, -1000);
            }
            finally
            {
                foreach (var actor in completed)
                {
                    actor.Commands.InterruptAll(true); if (actor.IsInCombat) actor.LeaveCombat();
                    if (fixture.Hostile.Memory.Contains(actor)) fixture.Hostile.Memory.Remove(actor);
                    if (actor.Memory.Contains(fixture.Hostile)) actor.Memory.Remove(fixture.Hostile);
                }
                fixture.Hostile.Commands.InterruptAll(true); fixture.Hostile.Commands.RemoveFinishedAndUpdateQueue();
                if (fixture.Hostile.IsInCombat) fixture.Hostile.LeaveCombat();
                Game.Instance.Player.UpdateIsInCombat();
                DisposeExpandedSummoningUnits(fixture.Created, completed); ResetExpandedSummoningHostile(fixture);
            }
            // Observe actual native source cleanup before the next fixed case.
            // No broad removal or protected actor ticking to make it pass.
            var deadline = DateTime.UtcNow.AddSeconds(5);
            while (DateTime.UtcNow < deadline && Game.Instance.State.AreaEffects.All.Any(value => !areas.Contains(value))) yield return 0;
            var reset = Lemure30ResetState(fixture, completed, targetBuffs, areas);
            bool casterExact = casterBuffs.SequenceEqual(fixture.Caster.Buffs.RawFacts.Select(value => value.Blueprint.AssetGuid).OrderBy(value => value, StringComparer.Ordinal));
            var live = Game.Instance.State.Units.All.Where(unit => unit != null && !unit.Destroyed && unit.IsInState).ToArray();
            var itemRows = new JArray(items.Select(item => OutsiderEscapeItemState(item, itemOwners[item], live)));
            bool itemsUnreachable = itemRows.All(row => (bool)row["safe"]);
            bool sceneExact = SameReferences(sceneBaseline, SnapshotReferences(fixture.SceneEntities));
            reset["fixtureCasterBuffsExact"] = casterExact; reset["ownedItemsUnreachable"] = itemsUnreachable;
            reset["itemStates"] = itemRows; reset["sceneReferencesExact"] = sceneExact;
            reset["itemCount"] = items.Count; reset["subcases"] = 7;
            bool partyExact = Sprint28PartyInventoryExact(partyItems, partyCounts); reset["partyInventoryExact"] = partyExact;
            bool clean = (bool)reset["clean"] && casterExact && itemsUnreachable && partyExact && sceneExact;
            Sprint25Check("outsider-escape-reset-before-caster", clean, reset,
                "Only exact disposable helper actors are removed; their commands/memory/items/areas/buffs and global combat/RTWP preconditions are observed before caster cases continue.");
            if (!clean) throw new InvalidOperationException("Outsider escape subcase contamination; later caster cases are unrun.");
        }
    }
}
