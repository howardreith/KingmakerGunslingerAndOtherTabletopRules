using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Actual native Player/context fixture for a profile that has no
    // compatible campaign save. Trait acceptance still runs CharacterBuildController
    // and Player.RespecCompanion. This class does not select traits or commit them.
    internal sealed class ElementalNativeProfileCreatorFixture : IDisposable
    {
        private readonly UnitReference _mainBefore;
        private readonly UnitEntityData[] _worldBefore;
        private readonly object[] _savedAreasBefore;
        private readonly object[] _inventoryBefore;
        private readonly int[] _itemCountsBefore;
        private ItemEntity[] _setupItems;
        private int[] _setupItemCounts;
        private readonly long _moneyBefore;
        private readonly bool _pauseBefore;
        private readonly ElementalUndineFeatScenario.PortalHarness _native;
        private readonly IList<string> _diagnostics;
        internal JObject Evidence { get; } = new JObject();
        internal UnitEntityData Main { get; private set; }
        internal bool Ready { get; private set; }
        internal bool Restored { get; private set; }
        private bool _disposed;
        private readonly MainMenu _menu;
        private readonly FieldInfo _menuEntry = typeof(MainMenu).GetField("m_EnterGameStarted",
            BindingFlags.Instance | BindingFlags.NonPublic);
        private bool _menuDetached;

        internal ElementalNativeProfileCreatorFixture(IList<string> diagnostics)
        {
            _diagnostics = diagnostics;
            if (Game.Instance == null || Game.Instance.Player == null || Game.Instance.State == null)
                throw new InvalidOperationException("Native profile fixture requires initialized native game services.");
            var player = Game.Instance.Player;
            _mainBefore = player.MainCharacter;
            _worldBefore = Game.Instance.State.Units.All.ToArray();
            _savedAreasBefore = Game.Instance.State.SavedAreaStates.Cast<object>().ToArray();
            _inventoryBefore = player.Inventory.Items.Cast<object>().ToArray();
            _itemCountsBefore = player.Inventory.Items.Select(value => value.Count).ToArray();
            _moneyBefore = player.Money; _pauseBefore = Game.Instance.IsPaused;
            if (_mainBefore.Value != null || _worldBefore.Length != 0 || Game.Instance.CurrentlyLoadedArea != null ||
                Game.Instance.State.LoadedAreaState != null || player.Party.Count != 0 ||
                player.PartyCharacters.Count != 0 || player.RemoteCompanions.Count != 0 ||
                player.CrossSceneState.AllEntityData.Any())
                throw new InvalidOperationException("An owned profile fixture cannot replace any existing campaign, party or world.");
            var build = Game.Instance.UI?.CharacterBuildController;
            if (build == null || build.IsShow || build.LevelUpController != null || Game.Instance.UI.LevelUpController != null)
                throw new InvalidOperationException("A native profile fixture requires an idle creator before registration.");
            _menu = Game.Instance.UI.MainMenu;
            if (_menu == null || _menuEntry == null || _menuEntry.FieldType != typeof(bool) ||
                (bool)_menuEntry.GetValue(_menu) || !EventBus.IsGloballySubscribed(_menu))
                throw new InvalidOperationException("Native profile fixture requires the idle subscribed main menu.");
            _native = new ElementalUndineFeatScenario.PortalHarness(diagnostics);
        }
        internal void Initialize()
        {
            if (Ready || Main != null || _disposed) throw new InvalidOperationException("Profile fixture initialized twice.");
            // DefaultPlayerCharacter intentionally has no race before chargen.
            // Resolve the existing native Human entry by its established identity.
            var human = BlueprintRoot.Instance.Progression.CharacterRaces.Single(value =>
                value.AssetGuid == "0a5d473ead98b0646b94495af250fdc4" && value.name == "HumanRace");
            try { Main = _native.Initialize(human); }
            finally
            {
                _setupItems = Game.Instance.Player.Inventory.Items.ToArray();
                _setupItemCounts = _setupItems.Select(value => value.Count).ToArray();
                Evidence["nativeSetupInventory"] = DescribeItems(_setupItems);
            }
            Main.Descriptor.CustomName = "KMG_COMPLETION_NATIVE_PROFILE_FIXTURE";
            // The public native setter appends to PartyCharacters. Replace the
            // one already registered reference before assigning it, never duplicate it.
            if (Game.Instance.Player.PartyCharacters.Count(value => ReferenceEquals(value.Value, Main)) != 1)
                throw new InvalidOperationException("The native fixture party reference is ambiguous.");
            Game.Instance.Player.PartyCharacters.Remove(Main);
            Game.Instance.Player.MainCharacter = Main;
            Game.Instance.Player.InvalidateCharacterLists(); Game.Instance.Player.UpdateCharacterLists();
            Ready = ReferenceEquals(Game.Instance.Player.MainCharacter.Value, Main) && Main.IsInState &&
                Main.HoldingState != null && Game.Instance.Player.Party.Contains(Main) &&
                Game.Instance.CurrentlyLoadedArea != null && Game.Instance.State.LoadedAreaState != null &&
                Game.Instance.State.Units.All.Count() == 1 &&
                Game.Instance.Player.PartyCharacters.Count(value => ReferenceEquals(value.Value, Main)) == 1;
            // MainMenu.Dispose removes this exact subscription on normal game
            // entry. Its level-up callback otherwise starts a campaign for every
            // creator commit, even a mercenary. Own only this menu boundary;
            // gameplay level-up and Player respec handlers remain subscribed.
            EventBus.Unsubscribe(_menu); _menuDetached = true;
            Ready &= !EventBus.IsGloballySubscribed(_menu) && !(bool)_menuEntry.GetValue(_menu);
            Evidence["nativeMenuSubscriptionBefore"] = true;
            Evidence["nativeMenuSubscriptionDuring"] = EventBus.IsGloballySubscribed(_menu);
            Evidence["nativeMainId"] = Main.UniqueId;
            Evidence["nativePlayerReference"] = ReferenceEquals(Game.Instance.Player.MainCharacter.Value, Main);
            Evidence["nativeAreaMetadata"] = Game.Instance.CurrentlyLoadedArea?.AssetGuid;
            Evidence["ready"] = Ready;
            Evidence["saveBacked"] = false;
            Evidence["scope"] = "registered native request-owned player/scene services; real creator and Player respec callbacks remain the acceptance path";
            if (!Ready) throw new InvalidOperationException("Native profile player registration is not exact.");
        }
        internal void CompleteOwnedRegistration(UnitEntityData unit)
        {
            if (!Ready || _disposed || unit == null || unit.View == null ||
                ReferenceEquals(unit, Main) || !unit.Descriptor.IsCustomCompanion() ||
                EventBus.IsGloballySubscribed(_menu))
                throw new InvalidOperationException("Owned mercenary registration has no exact native profile boundary.");
            var creator = Game.Instance.EntityCreator;
            var queueField = creator.GetType().GetField("m_ToCreate", BindingFlags.Instance | BindingFlags.NonPublic);
            var queue = queueField?.GetValue(creator) as IList;
            if (queue == null) throw new MissingFieldException("EntityCreationController.m_ToCreate");
            if (ReferenceEquals(unit.HoldingState, Game.Instance.Player.CrossSceneState))
            {
                if (queue.Count != 0) throw new InvalidOperationException("Unexpected pending native entity creation after mercenary registration.");
                return;
            }
            if (unit.HoldingState != null || queue.Count != 1)
                throw new InvalidOperationException("Only one exact pending mercenary creation may be advanced.");
            object entry = queue[0];
            var fields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            if (!ReferenceEquals(entry.GetType().GetField("Entity", fields)?.GetValue(entry), unit) ||
                !ReferenceEquals(entry.GetType().GetField("State", fields)?.GetValue(entry), Game.Instance.Player.CrossSceneState))
                throw new InvalidOperationException("Native pending creation does not belong to this mercenary and cross-scene state.");
            var world = Game.Instance.State.Units.All.ToArray();
            var cross = Game.Instance.Player.CrossSceneState.AllEntityData.ToArray();
            creator.Tick();
            bool exact = queue.Count == 0 && ReferenceEquals(unit.HoldingState, Game.Instance.Player.CrossSceneState) &&
                unit.IsInState && Game.Instance.State.Units.All.Where(value => !ReferenceEquals(value, unit)).SequenceEqual(world) &&
                Game.Instance.Player.CrossSceneState.AllEntityData.Where(value => !ReferenceEquals(value, unit)).SequenceEqual(cross);
            var rows = Evidence["nativeMercenaryRegistrations"] as JArray;
            if (rows == null) Evidence["nativeMercenaryRegistrations"] = rows = new JArray();
            rows.Add(new JObject { ["unitId"] = unit.UniqueId, ["ownedQueueCount"] = 1,
                ["nativeControllerTicked"] = true, ["exact"] = exact });
            if (!exact) throw new InvalidOperationException("The native registration changed entities outside the owned mercenary.");
        }

        private static JArray DescribeItems(ItemEntity[] items)
        {
            return new JArray(items.Select(item => new JObject { ["guid"] = item.Blueprint.AssetGuid,
                ["name"] = item.Blueprint.name, ["count"] = item.Count }));
        }
        private void CleanupNativeItems()
        {
            // The host owns only synchronous setup/disposal deltas. Every creator
            // must already have restored the exact post-setup inventory boundary.
            var inventory = Game.Instance.Player.Inventory;
            var after = inventory.Items.ToArray();
            Evidence["nativeTeardownInventory"] = DescribeItems(after);
            if (_inventoryBefore.Any(item => !after.Any(value => ReferenceEquals(value, item))))
                throw new InvalidOperationException("Native host disposal removed a preexisting item reference.");
            var removed = new JArray();
            foreach (var item in after)
            {
                int prior = Array.FindIndex(_inventoryBefore, value => ReferenceEquals(value, item));
                int expected = prior < 0 ? 0 : _itemCountsBefore[prior];
                int excess = item.Count - expected;
                if (excess < 0) throw new InvalidOperationException("Native host disposal consumed a preexisting inventory stack.");
                if (excess == 0) continue;
                removed.Add(new JObject { ["guid"] = item.Blueprint.AssetGuid, ["name"] = item.Blueprint.name,
                    ["count"] = excess, ["newReference"] = prior < 0 });
                inventory.Remove(item, excess).Dispose();
            }
            Evidence["nativeOwnedItemsRemoved"] = removed;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            try
            {
                if (Main != null && !ReferenceEquals(Game.Instance.Player.MainCharacter.Value, Main))
                    throw new InvalidOperationException("The profile fixture lost its native Player ownership.");
                var expectedItems = _setupItems == null ? _inventoryBefore : _setupItems.Cast<object>().ToArray();
                var expectedCounts = _setupItemCounts ?? _itemCountsBefore;
                if (!Game.Instance.Player.Inventory.Items.Cast<object>().SequenceEqual(expectedItems) ||
                    !Game.Instance.Player.Inventory.Items.Select(value => value.Count).SequenceEqual(expectedCounts))
                    throw new InvalidOperationException("Inventory changed outside native host setup or owned creator commits.");
                Game.Instance.Player.MainCharacter = _mainBefore;
                _native.Dispose();
                CleanupNativeItems();
            }
            finally
            {
                if (_menuDetached)
                {
                    if (EventBus.IsGloballySubscribed(_menu))
                        throw new InvalidOperationException("The owned menu subscription changed during the native profile fixture.");
                    EventBus.Subscribe(_menu); _menuDetached = false;
                }
                Evidence["nativeMenuSubscriptionRestored"] = ReferenceEquals(Game.Instance.UI.MainMenu, _menu) &&
                    EventBus.IsGloballySubscribed(_menu) && !(bool)_menuEntry.GetValue(_menu);
            }
            var player = Game.Instance.Player;
            var gates = new JObject {
                ["menu"] = (bool)Evidence["nativeMenuSubscriptionRestored"],
                ["main"] = player.MainCharacter.Value == null,
                ["world"] = Game.Instance.State.Units.All.SequenceEqual(_worldBefore),
                ["loadedArea"] = Game.Instance.CurrentlyLoadedArea == null && Game.Instance.State.LoadedAreaState == null,
                ["savedAreas"] = Game.Instance.State.SavedAreaStates.Cast<object>().SequenceEqual(_savedAreasBefore),
                ["inventoryReferences"] = player.Inventory.Items.Cast<object>().SequenceEqual(_inventoryBefore),
                ["inventoryCounts"] = player.Inventory.Items.Select(value => value.Count).SequenceEqual(_itemCountsBefore),
                ["money"] = player.Money == _moneyBefore,
                ["pause"] = Game.Instance.IsPaused == _pauseBefore,
                ["nativeObserverReleased"] = _native.NativeObservationReleased,
                ["partyCharacters"] = player.PartyCharacters.Count == 0,
                ["remoteCompanions"] = player.RemoteCompanions.Count == 0,
                ["crossScene"] = !player.CrossSceneState.AllEntityData.Any() };
            Evidence["cleanupGates"] = gates;
            Evidence["cleanupValues"] = new JObject {
                ["pauseBefore"] = _pauseBefore, ["pauseAfter"] = Game.Instance.IsPaused,
                ["moneyBefore"] = _moneyBefore, ["moneyAfter"] = player.Money,
                ["worldIds"] = new JArray(Game.Instance.State.Units.All.Select(value => value.UniqueId)),
                ["crossIds"] = new JArray(player.CrossSceneState.AllEntityData.Select(value => value.UniqueId)),
                ["inventoryBeforeCount"] = _inventoryBefore.Length,
                ["inventoryAfter"] = new JArray(player.Inventory.Items.Select(value => new JObject {
                    ["guid"] = value.Blueprint.AssetGuid, ["count"] = value.Count })) };
            Restored = gates.Properties().All(value => (bool)value.Value);
            Evidence["nativeInitializationCompleted"] = Main != null;
            Evidence["nativeAreaRestorationObserved"] = _native.AreaContextRestored;
            Evidence["nativePlayerRestorationObserved"] = _native.PlayerContextRestored;
            Evidence["restored"] = Restored;
            Evidence["nativeDiagnostics"] = new JArray(_diagnostics);
            if (!Restored) throw new InvalidOperationException("Native profile fixture cleanup is not exact.");
        }
    }
}
