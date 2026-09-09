using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Globalmap;
using Kingmaker.Globalmap.State;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Spells.Teleportation;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> RunTeleportationTravelers(GlobalMapLocation origin, GlobalMapLocation target,
            List<TeleportResourceFixtureOwner> owners)
        {
            if (!IsTeleportationTravelersFixture || !_request.ExitAfterCompletion || _workingSaveSmoke.WriteObserved)
                throw new InvalidOperationException("Only the named guarded travelers request may create temporary native pets.");
            var player = Game.Instance.Player;
            var originals = TeleportationTravelers.Read(player).Units;
            var masters = player.Party.Where(value => value.Descriptor.Pet == null && value.Descriptor.Master.Value == null).Take(3).ToArray();
            if (masters.Length != 3 || originals.Any(value => value.Descriptor.State.IsDead || value.Descriptor.State.IsUnconscious ||
                value.Stats.HitPoints.ModifiedValue - value.Damage <= 3))
                throw new InvalidOperationException("Three unassociated native owners and HP-safe original travelers are required.");
            var originalDamage = originals.Select(value => value.Damage).ToArray();
            var originalLife = originals.Select(value => value.Descriptor.State.LifeState).ToArray();
            var originalLastDamage = originals.Select(value => value.LastHandledDamage).ToArray();
            var setLastDamage = typeof(UnitEntityData).GetProperty("LastHandledDamage").GetSetMethod(true);
            var originalEntities = player.CrossSceneState.AllEntityData.ToArray();
            var originalParty = player.Party.ToArray();
            var originalTime = player.GameTime;
            var pets = new List<UnitEntityData>();
            var observer = new TeleportTravelerNativeObserver();
            var life = new TeleportFixtureLifeController();
            var blueprint = BlueprintLibraryLookup.RequireExact<BlueprintUnit>(BlueprintBootstrap.Library,
                "54cf380dee486ff42b803174d1b9da1b", "native animal companion leopard fixture");
            var prefab = blueprint.Prefab.Load(false);
            if (prefab == null || blueprint.CustomizationPreset != null) throw new InvalidOperationException("Exact native pet prefab required.");
            string originalPrefabId = prefab.UniqueId;
            bool retainedMasterDying = false;
            EventBus.Subscribe(observer);
            try
            {
                var wizard = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                    "ba34257984f4c41408ce1dc2004e342e", "native traveler fixture Wizard");
                var caster = originalParty.First(value => TeleportationSpellbookAdapter.CasterAvailable(value) && value.Descriptor.GetSpellbook(wizard.Spellbook) == null);
                var owner = new TeleportResourceFixtureOwner(caster); owners.Add(owner);
                var book = owner.AddBook(wizard.Spellbook);
                book.AddKnown(5, BlueprintBootstrap.Teleportation.Teleport, true);
                if (!book.Memorize(new AbilityData(BlueprintBootstrap.Teleportation.Teleport, book), null))
                    throw new InvalidOperationException("Native traveler fixture preparation failed.");
                book.Rest();
                ValidateTeleportationNativeSnapshots(book, origin, target);
                for (int index = 0; index < 4; index++)
                {
                    UnitEntityData unit;
                    try { unit = Game.Instance.EntityCreator.SpawnUnit(blueprint, masters[0].Position, Quaternion.identity, player.CrossSceneState); }
                    finally { prefab.UniqueId = originalPrefabId; }
                    if (unit == null) throw new InvalidOperationException("Native pet creation returned no unit.");
                    pets.Add(unit);
                    unit.Stats.HitPoints.BaseValue = 30;
                    if (index < 3) unit.Descriptor.SetMaster(masters[index]);
                    unit.IsInGame = index < 3 && masters[index].IsInGame;
                }
                player.InvalidateCharacterLists(); player.UpdateCharacterLists();
                for (int frame = 0; frame < 4; frame++) yield return 0;
                UnitEntityData healthy = pets[0], endangered = pets[1], dead = pets[2], nonTraveler = pets[3];
                CaptureTeleportInteraction("native-pet-setup", new { units = pets.Select(TeleportTravelerUnitEvidence).ToArray(),
                    originalUnits = originals.Select(TeleportTravelerUnitEvidence).ToArray(), roster = TeleportationTravelers.Read(player).Evidence() });
                TeleportInteractionAssert("canonical-associated-roster", "three native reciprocal pets join transport; the unowned cross-scene unit is excluded",
                    "travelers=" + TeleportationTravelers.Read(player).Units.Length,
                    originalParty.SequenceEqual(player.Party) && pets.Take(3).All(unit => TeleportationTravelers.Read(player).Units.Contains(unit)) &&
                    !TeleportationTravelers.Read(player).Units.Contains(nonTraveler) && pets.Take(3).Select((unit, index) =>
                        ReferenceEquals(unit.Descriptor.Master.Value, masters[index]) && ReferenceEquals(masters[index].Descriptor.Pet, unit)).All(value => value));
                if (pets.Take(3).Any(unit => unit.View == null || unit.Descriptor.State.IsDead || unit.Descriptor.State.IsUnconscious))
                    throw new InvalidOperationException("The native pet life-state probe requires initial living unit views.");
                // Setup uses native damage and the exact native life controller.
                // No direct LifeState assignment or invented death threshold runs.
                Rulebook.Trigger(new RuleDealDamage(caster, dead, new DamageBundle(new DirectDamage(
                    new DiceFormula(0, DiceType.D10), dead.Stats.HitPoints.ModifiedValue + dead.Stats.Constitution.ModifiedValue + 1))));
                life.Refresh(dead);
                CaptureTeleportInteraction("native-predead-control", new { unit = TeleportTravelerUnitEvidence(dead), lifeEvents = observer.LifeEvents.ToArray() });
                if (!dead.Descriptor.State.IsDead) throw new InvalidOperationException("Native controller did not establish the dead control.");
                // Only this disposable pet receives fragile native attributes.
                endangered.Stats.Constitution.BaseValue = 1;
                endangered.Stats.HitPoints.BaseValue = 20;
                endangered.Damage = endangered.Stats.HitPoints.ModifiedValue - 1;
                if (endangered.Stats.Constitution.ModifiedValue != 1 || endangered.Damage < 0)
                    throw new InvalidOperationException("Native fragile-pet attribute prerequisites differ.");
                life.Refresh(endangered);
                if (endangered.Descriptor.State.IsDead || endangered.Descriptor.State.IsUnconscious)
                    throw new InvalidOperationException("Fragile pet must be living with one HP before confirmation.");
                var beforeDamage = pets.Select(unit => unit.Damage).ToArray();
                observer.DamageRules.Clear(); observer.LifeEvents.Clear();
                var rolls = new TeleportationFixtureRolls(new[] { 97, 100, 76 }, new[] { 2, 1 });
                var request = OpenTeleportationFixtureConfirmation(TeleportationFixturePanel(), target,
                    TeleportSpellKind.Teleport, TeleportCastSourceKind.Prepared, rolls);
                for (int frame = 0; frame < 4; frame++) yield return 0;
                // Native out-of-combat recovery may revive a non-finally-dead
                // control during the confirmation frames. Establish and record
                // the dead control immediately before the actual commit instead.
                if (!dead.Descriptor.State.IsDead)
                {
                    Rulebook.Trigger(new RuleDealDamage(caster, dead, new DamageBundle(new DirectDamage(
                        new DiceFormula(0, DiceType.D10), dead.Stats.HitPoints.ModifiedValue + dead.Stats.Constitution.ModifiedValue + 1))));
                    life.Refresh(dead);
                }
                beforeDamage[2] = dead.Damage;
                CaptureTeleportInteraction("dead-control-at-commit", new { unit = TeleportTravelerUnitEvidence(dead),
                    trueDeath = player.Difficulty.TrueDeath, deathDoor = player.Difficulty.DeathDoorCondition });
                if (!dead.Descriptor.State.IsDead) throw new InvalidOperationException("Control is not dead at the actual contextual commitment.");
                observer.DamageRules.Clear(); observer.LifeEvents.Clear(); observer.LifeUnitIds.Clear();
                TeleportationFixtureDialogButton("m_ButtonYes").onClick.Invoke();
                CaptureTeleportInteraction("repeated-mishap-immediate", new { transaction = request.Transaction.State.ToString(),
                    request.Transaction.Diagnostic, execution = request.Execution.LastEvidence, units = pets.Select(TeleportTravelerUnitEvidence).ToArray(),
                    nativeDamageEvents = observer.DamageEvidence(), lifeEvents = observer.LifeEvents.ToArray() });
                TeleportInteractionAssert("one-slot-and-protected-arrival", "contextual repeated mishap spends exactly one preparation and preserves canonical party/pet placement invariants",
                    "state=" + request.Transaction.State, request.Transaction.State == TeleportTransactionState.Completed &&
                    request.Execution.Resource.ObserveExpenditure() == TeleportExpenditure.ExactlyOne && request.Transaction.Result.Mishaps == 2 &&
                    rolls.D100Count == 3 && rolls.D10Count == 2 && GlobalMapRules.State.PartyLocation == target.Blueprint &&
                    originalParty.SequenceEqual(player.Party) && player.GameTime == originalTime);
                TeleportInteractionAssert("living-and-associated-damage", "each original living traveler and the healthy pet receives the same two native damage packets",
                    "healthyDelta=" + (healthy.Damage - beforeDamage[0]), originals.Select((unit, index) => unit.Damage - originalDamage[index] == 3 &&
                        observer.DamageRules.Count(rule => ReferenceEquals(rule.Target, unit)) == 2).All(value => value) &&
                    healthy.Damage - beforeDamage[0] == 3 && observer.DamageRules.Count(rule => ReferenceEquals(rule.Target, healthy)) == 2);
                TeleportInteractionAssert("dead-and-nontraveling-excluded", "already dead pet and unowned nontraveler receive no mishap damage",
                    "deadDelta=" + (dead.Damage - beforeDamage[2]) + ";nontravelerDelta=" + (nonTraveler.Damage - beforeDamage[3]),
                    dead.Damage == beforeDamage[2] && nonTraveler.Damage == beforeDamage[3] &&
                    observer.DamageRules.All(rule => !ReferenceEquals(rule.Target, dead) && !ReferenceEquals(rule.Target, nonTraveler)));
                TeleportInteractionAssert("newly-dead-excluded-on-reroll", "native death after first mishap excludes the pet from the second packet without an HP floor",
                    "state=" + endangered.Descriptor.State.LifeState + ";damageDelta=" + (endangered.Damage - beforeDamage[1]),
                    endangered.Descriptor.State.IsDead && endangered.Damage - beforeDamage[1] == 2 &&
                    observer.DamageRules.Count(rule => ReferenceEquals(rule.Target, endangered)) == 1);
                RestoreTeleportTravelerDamage(originals, originalDamage, originalLastDamage, setLastDamage);
                for (int frame = 0; frame < 4; frame++) yield return 0;
                CaptureTeleportInteraction("native-life-after-frames", new { unit = TeleportTravelerUnitEvidence(endangered), lifeEvents = observer.LifeEvents.ToArray() });
                TeleportInteractionAssert("native-life-controller-boundary", "the production native update emitted the affected pet's normal life-state event",
                    "lifeEvents=" + observer.LifeUnitIds.Count, observer.LifeUnitIds.Contains(endangered.UniqueId));

                GlobalMapRules.Instance.SetCurrentPosition(new MapPosition(origin.Blueprint)); GlobalMapRules.Instance.UpdatePawnPosition();
                book.Rest();
                healthy.Stats.Constitution.BaseValue = 20;
                healthy.Stats.HitPoints.BaseValue = 30;
                healthy.Damage = healthy.Stats.HitPoints.ModifiedValue - 1;
                if (!healthy.Descriptor.State.AllowDyingCondition) healthy.Descriptor.State.AllowDyingCondition.Retain();
                if (!masters[0].Descriptor.State.AllowDyingCondition) { masters[0].Descriptor.State.AllowDyingCondition.Retain(); retainedMasterDying = true; }
                life.Refresh(healthy);
                int healthyBefore = healthy.Damage;
                observer.DamageRules.Clear(); observer.LifeEvents.Clear();
                var unconscious = OpenTeleportationFixtureConfirmation(TeleportationFixturePanel(), target,
                    TeleportSpellKind.Teleport, TeleportCastSourceKind.Prepared, new TeleportationFixtureRolls(new[] { 97, 100, 76 }, new[] { 2, 1 }));
                TeleportationFixtureDialogButton("m_ButtonYes").onClick.Invoke();
                CaptureTeleportInteraction("unconscious-mishap-immediate", new { transaction = unconscious.Transaction.State.ToString(),
                    unconscious.Transaction.Diagnostic, execution = unconscious.Execution.LastEvidence, unit = TeleportTravelerUnitEvidence(healthy),
                    nativeDamageEvents = observer.DamageEvidence(), lifeEvents = observer.LifeEvents.ToArray() });
                TeleportInteractionAssert("unconscious-living-reroll", "native unconsciousness remains living; the second mishap still damages that associated pet",
                    "state=" + healthy.Descriptor.State.LifeState + ";damageDelta=" + (healthy.Damage - healthyBefore),
                    unconscious.Transaction.State == TeleportTransactionState.Completed && healthy.Descriptor.State.IsUnconscious &&
                    !healthy.Descriptor.State.IsDead && healthy.Damage - healthyBefore == 3 &&
                    observer.DamageRules.Count(rule => ReferenceEquals(rule.Target, healthy)) == 2 &&
                    unconscious.Execution.Resource.ObserveExpenditure() == TeleportExpenditure.ExactlyOne);
                RestoreTeleportTravelerDamage(originals, originalDamage, originalLastDamage, setLastDamage);
                for (int frame = 0; frame < 4; frame++) yield return 0;
            }
            finally
            {
                CloseTeleportationFixturePanels();
                EventBus.Unsubscribe(observer);
                RestoreTeleportTravelerDamage(originals, originalDamage, originalLastDamage, setLastDamage);
                if (retainedMasterDying) masters[0].Descriptor.State.AllowDyingCondition.Release();
                foreach (var unit in pets.AsEnumerable().Reverse())
                {
                    unit.Descriptor.SetMaster(null);
                    if (unit.HoldingState != null) unit.HoldingState.RemoveEntityData(unit);
                    else unit.Dispose();
                }
                prefab.UniqueId = originalPrefabId;
                player.InvalidateCharacterLists(); player.UpdateCharacterLists();
                bool restored = originalEntities.SequenceEqual(player.CrossSceneState.AllEntityData) && originalParty.SequenceEqual(player.Party) &&
                    originals.Select(unit => unit.Damage).SequenceEqual(originalDamage) && originals.Select(unit => unit.Descriptor.State.LifeState).SequenceEqual(originalLife) &&
                    originals.Select(unit => unit.LastHandledDamage).SequenceEqual(originalLastDamage) && masters.All(unit => unit.Descriptor.Pet == null) &&
                    pets.All(unit => !player.AllCharacters.Contains(unit)) && !_workingSaveSmoke.WriteObserved;
                CaptureTeleportInteraction("pet-fixture-cleanup", new { restored, prefabIdentityRestored = prefab.UniqueId == originalPrefabId,
                    remainingCrossSceneIds = player.CrossSceneState.AllEntityData.Select(unit => unit.UniqueId).ToArray() });
                TeleportInteractionAssert("pet-fixture-cleanup", "all temporary native pets/nontravelers, ownership, original HP/life/damage attribution and prefab identity restored",
                    "restored=" + restored, restored);
            }
        }
        private void ValidateTeleportationNativeSnapshots(Spellbook book, GlobalMapLocation origin, GlobalMapLocation target)
        {
            var player = Game.Instance.Player;
            var map = GlobalMapRules.State;
            var baseline = new TeleportationWorldSnapshot(TeleportationWorldMapAdapter.Capture(false));
            string json = TeleportationDiagnosticJson.Serialize(baseline.State);
            var parsed = Newtonsoft.Json.Linq.JObject.Parse(json);
            TeleportInteractionAssert("snapshot-fields-present", "flattened production snapshot retains real world time, points, edges and roster under native JSON defaults",
                "characters=" + json.Length, parsed["world"] != null && parsed["world"]["gameTimeTicks"] != null &&
                parsed["world"]["points"].HasValues && parsed["world"]["edges"].HasValues && parsed["roster"]["partyIds"].HasValues);
            Action<string, Action, Action> reject = (name, mutate, restore) => {
                bool rejected = false;
                try { mutate(); try { baseline.Verify(origin.Blueprint.AssetGuid); } catch (InvalidOperationException) { rejected = true; } }
                finally { restore(); }
                baseline.Verify(origin.Blueprint.AssetGuid);
                TeleportInteractionAssert("snapshot-detects-" + name, "production protected-state comparison rejects changed native data and accepts exact restoration",
                    "rejected=" + rejected, rejected);
            };
            var time = player.GameTime;
            reject("time", () => player.GameTime = time + TimeSpan.FromSeconds(1), () => player.GameTime = time);
            float miles = map.MilesTravelled;
            reject("miles", () => map.MilesTravelled = miles + 1, () => map.MilesTravelled = miles);
            bool seen = target.Data.IsSeen;
            reject("point-flags", () => target.Data.IsSeen = !seen, () => target.Data.IsSeen = seen);
            var visited = target.Data.LastVisited;
            reject("point-visit-time", () => target.Data.LastVisited = visited + TimeSpan.FromSeconds(1), () => target.Data.LastVisited = visited);
            string beforeBook = TeleportResourceFingerprint(book);
            var ability = book.GetMemorizedSpellSlots(5).First(value => value.Available && value.Spell.Blueprint == BlueprintBootstrap.Teleportation.Teleport).Spell;
            if (!book.Spend(ability, false)) throw new InvalidOperationException("Native negative-control debit failed.");
            string spentBook = TeleportResourceFingerprint(book);
            book.Rest();
            TeleportInteractionAssert("resource-fingerprint-detects-debit", "native resource fingerprint detects actual expenditure and exact rest of this temporary book",
                "changed=" + (spentBook != beforeBook), spentBook != beforeBook && beforeBook == TeleportResourceFingerprint(book) &&
                Newtonsoft.Json.Linq.JObject.Parse(beforeBook)["levels"].HasValues);
        }
        private static void RestoreTeleportTravelerDamage(UnitEntityData[] units, int[] damage, RuleDealDamage[] last, MethodInfo setter)
        {
            for (int index = 0; index < units.Length; index++) { units[index].Damage = damage[index]; setter.Invoke(units[index], new object[] { last[index] }); }
        }
        private static object TeleportTravelerUnitEvidence(UnitEntityData unit)
        {
            return new { id = unit.UniqueId, blueprint = unit.Blueprint.AssetGuid, unit.IsInGame, unit.IsDetached,
                masterId = unit.Descriptor.Master.Value == null ? null : unit.Descriptor.Master.Value.UniqueId,
                hasView = unit.View != null, activeView = unit.View != null && unit.View.gameObject.activeInHierarchy,
                awake = Game.Instance.State.AwakeUnits.Contains(unit), hp = unit.Stats.HitPoints.ModifiedValue,
                constitution = unit.Stats.Constitution.ModifiedValue, damage = unit.Damage, life = unit.Descriptor.State.LifeState.ToString(),
                isDead = unit.Descriptor.State.IsDead, isUnconscious = unit.Descriptor.State.IsUnconscious,
                allowDying = unit.Descriptor.State.AllowDyingCondition.Count };
        }
        private sealed class TeleportFixtureLifeController : UnitLifeController
        {
            internal void Refresh(UnitEntityData unit) { if (ShouldTickOnUnit(unit)) TickOnUnit(unit); }
        }
        private sealed class TeleportTravelerNativeObserver : IDamageHandler, IUnitLifeStateChanged
        {
            internal readonly List<RuleDealDamage> DamageRules = new List<RuleDealDamage>();
            internal readonly List<object> LifeEvents = new List<object>();
            internal readonly List<string> LifeUnitIds = new List<string>();
            public void HandleDamageDealt(RuleDealDamage rule) { DamageRules.Add(rule); }
            public void HandleUnitLifeStateChanged(UnitEntityData unit, UnitLifeState previous)
            { LifeUnitIds.Add(unit.UniqueId); LifeEvents.Add(new { id = unit.UniqueId, previous = previous.ToString(), current = unit.Descriptor.State.LifeState.ToString(), frame = Time.frameCount }); }
            internal object DamageEvidence()
            { return DamageRules.Select(rule => new { id = rule.Target.UniqueId, damage = rule.Damage,
                sourceAbility = rule.SourceAbility == null ? null : rule.SourceAbility.AssetGuid, minimumHitPoints = rule.MinHPAfterDamage }).ToArray(); }
        }
    }
}
