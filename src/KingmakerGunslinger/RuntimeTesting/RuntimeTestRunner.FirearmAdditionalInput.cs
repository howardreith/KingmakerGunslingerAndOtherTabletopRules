using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UI._ConsoleUI;
using Kingmaker.UI._ConsoleUI.InputLayers.InGameLayer;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Firing;
using KingmakerGunslinger.Misfires;
using UnityEngine;
using ConsoleSelection = Kingmaker.UI._ConsoleUI.Models.UnitSelectionManager;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Real console selection and input-layer constructor. The inactive cursor
        // supplies only its native mode bit; no Rewired device, OS input, raycast,
        // visual navigation or fabricated attack authorization is involved.
        private sealed class FirearmControllerInputScope : IDisposable
        {
            private const BindingFlags Members = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            private readonly Game.ControllerModeType modeBefore = Game.Instance.ControllerMode;
            private readonly bool cursorVisible = Cursor.visible;
            private readonly RewiredCursorController cursorBefore = RewiredCursorController.Instance;
            private readonly PropertyInfo cursorInstance = typeof(RewiredCursorController).GetProperty("Instance",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            private GameObject selectionRoot, cursorRoot;
            private ConsoleSelection selection;
            private InGameInputLayer layer;
            internal FirearmControllerInputScope(UnitEntityData actor, bool cursorMode)
            {
                if (ConsoleSelection.Instance != null || cursorBefore != null)
                    throw new InvalidOperationException("The save-free console input service boundary is not empty.");
                try
                {
                    selectionRoot = new GameObject("KMG_Runtime_Firearm_ConsoleSelection");
                    selectionRoot.hideFlags = HideFlags.HideAndDontSave;
                    selection = selectionRoot.AddComponent<ConsoleSelection>();
                    selection.SelectUnit(actor.View, true, false, false);
                    if (!ReferenceEquals(selection.CurrentSelectUnitValue, actor))
                        throw new InvalidOperationException("Native console selection did not select the exact actor.");
                    cursorRoot = new GameObject("KMG_Runtime_Firearm_CursorMode");
                    cursorRoot.hideFlags = HideFlags.HideAndDontSave;
                    cursorRoot.SetActive(false);
                    var cursor = cursorRoot.AddComponent<RewiredCursorController>();
                    foreach (string name in new[] { "m_Image", "m_AbilityImage", "m_AbilityForbiddenImage", "m_OutOfRangeImage", "m_Text" })
                    {
                        var field = typeof(RewiredCursorController).GetField(name, Members);
                        if (field == null || !typeof(Component).IsAssignableFrom(field.FieldType))
                            throw new InvalidOperationException("Native cursor presentation contract changed: " + name);
                        var child = new GameObject(name, typeof(RectTransform));
                        child.transform.SetParent(cursorRoot.transform, false);
                        field.SetValue(cursor, child.AddComponent(field.FieldType));
                    }
                    cursorInstance.SetValue(null, cursor, null);
                    typeof(RewiredCursorController).GetField("m_Enabled", Members).SetValue(cursor, cursorMode);
                    if (cursor.Enabled != cursorMode) throw new InvalidOperationException("Native cursor mode fixture disagrees.");
                    Game.Instance.ControllerMode = (Game.ControllerModeType)1;
                    layer = new InGameInputLayer();
                    layer.StopUpdates(); // no autonomous device polling in this fixture
                }
                catch { Dispose(); throw; }
            }
            internal void Interact(UnitEntityData target)
            {
                typeof(InGameInputLayer).GetField("m_ChoosenInteractableObject", Members)
                    .SetValue(layer, target.View.gameObject);
                layer.OnInteract();
            }
            public void Dispose()
            {
                if (layer != null) { EventBus.Unsubscribe(layer); layer = null; }
                Game.Instance.ControllerMode = modeBefore;
                Cursor.visible = cursorVisible;
                if (selectionRoot != null) UnityEngine.Object.DestroyImmediate(selectionRoot);
                if (cursorRoot != null)
                {
                    var cursor = cursorRoot.GetComponent<RewiredCursorController>();
                    var sprites = (Dictionary<Texture2D, Sprite>)typeof(RewiredCursorController).GetField("m_Cursors", Members)
                        .GetValue(cursor);
                    foreach (var sprite in sprites.Values) UnityEngine.Object.DestroyImmediate(sprite);
                    sprites.Clear();
                    UnityEngine.Object.DestroyImmediate(cursorRoot);
                }
                cursorInstance.SetValue(null, cursorBefore, null);
                selectionRoot = cursorRoot = null;
                if (ConsoleSelection.Instance != null)
                    throw new InvalidOperationException("Owned console selection was not restored.");
            }
        }

        private IEnumerable<object> FirearmAdditionalInputCases()
        {
            foreach (string variant in new[] { "different-target", "controller-empty", "controller-loaded", "repair" })
            {
                string prefix = variant + "-";
                _firearmInputStage = prefix + "initial-misfire";
                _firearmInputFixture = new FirearmInputFixture(false, _firearmInputRows);
                var f = _firearmInputFixture;
                FirearmMisfireRuntime.QueueForcedNaturalRoll(1);
                f.Click(f.Enemy);
                for (int tick = 0; tick < 240 && f.Shots.Count == 0; tick++)
                { f.Pump(); f.Record(tick); yield return null; }
                int powder = f.Powder;
                foreach (object step in DriveFirearmIdle(f)) yield return step;
                FirearmInputCheck(prefix + "real-break-stopped", f.Shots.Count == 1 &&
                    f.State.Condition == FirearmCondition.Broken && f.State.IsEmpty && f.Powder == powder &&
                    f.SameItemIdentity && BrokenSequenceSuppressionRuntime.IsSuppressed(f.Actor, f.Weapon), f.Describe());
                UnitEntityData finalTarget = f.Enemy;
                if (variant == "different-target")
                {
                    var otherEnemy = f.SpawnAdditional(false, "OtherLegalEnemy");
                    var ally = f.SpawnAdditional(true, "OtherSelectedActor");
                    Game.Instance.Player.PartyCharacters.Add(ally);
                    Game.Instance.Player.InvalidateCharacterLists(); Game.Instance.Player.UpdateCharacterLists();
                    f.Turns.Execute(() => Game.Instance.UI.SelectionManager.SelectUnit(ally.View, true, false, false));
                    f.Click(f.Enemy);
                    FirearmInputCheck(prefix + "other-actor-ordinary-weapon", ally.Commands.Raw.OfType<UnitAttack>().Any() &&
                        BrokenSequenceSuppressionRuntime.Orders.Current(f.Actor) == null &&
                        BrokenSequenceSuppressionRuntime.IsSuppressed(f.Actor, f.Weapon),
                        "A genuine other-character ordinary-weapon order does not authorize the broken firearm.");
                    ally.Commands.InterruptAll(true); ally.Commands.RemoveFinishedAndUpdateQueue();
                    f.Turns.Execute(() => Game.Instance.UI.SelectionManager.SelectUnit(f.Actor.View, true, false, false));
                    f.Click(f.Enemy);
                    var previous = BrokenSequenceSuppressionRuntime.Orders.Current(f.Actor);
                    var reload = f.Actor.Commands.Raw.OfType<UnitUseAbility>().First();
                    var wrongTarget = f.CreateNativeAiCommand(otherEnemy);
                    f.Turns.Execute(() => f.Actor.Commands.Run(wrongTarget));
                    FirearmInputCheck(prefix + "wrong-target-ai-rejected", wrongTarget != null &&
                        !f.Actor.Commands.Raw.Contains(wrongTarget) && !f.Actor.Commands.Queue.Contains(wrongTarget) &&
                        BrokenSequenceSuppressionRuntime.Orders.IsCurrent(previous) && f.Actor.Commands.Raw.Contains(reload),
                        "Native AI resolves a different target and cannot borrow this accepted reload order.");
                    _firearmInputStage = prefix + "native-reload-completion-before-new-target";
                    var scheduled = (List<Action>)typeof(Game).GetField("m_BeforeTickActions",
                        BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Game.Instance);
                    // Result becomes Success when OnAction begins the ability;
                    // the delivered reload and OnEnded callback finish later.
                    for (int tick = 0; tick < 240 && scheduled.Count == 0; tick++)
                    { f.Pump(); f.Record(tick); yield return null; }
                    FirearmInputCheck(prefix + "real-reload-callback-pending", reload.IsActed &&
                        reload.Result == UnitCommand.ResultType.Success && scheduled.Count > 0 &&
                        f.State.LoadedRounds == 1 && f.Shots.Count == 1,
                        f.Describe() + ";result=" + reload.Result + ";native scheduled=" + scheduled.Count +
                        ";successful reload must queue its actual continuation before the next explicit order.");
                    f.Turns.SetFirearmPaused(true);
                    FirearmMisfireRuntime.QueueForcedNaturalRoll(10);
                    f.Click(otherEnemy);
                    var firstPending = f.Actor.Commands.Raw.OfType<UnitAttack>().First();
                    var firstNew = BrokenSequenceSuppressionRuntime.Orders.Current(f.Actor);
                    f.Click(otherEnemy);
                    var secondNew = BrokenSequenceSuppressionRuntime.Orders.Current(f.Actor);
                    var survivor = f.Actor.Commands.Raw.OfType<UnitAttack>().First();
                    FirearmInputCheck(prefix + "paused-repeat-order-supersedes", previous.Cancelled &&
                        firstNew != null && firstNew.Cancelled && secondNew != null &&
                        !ReferenceEquals(firstNew, secondNew) && ReferenceEquals(secondNew.Target, otherEnemy) &&
                        f.Actor.Commands.Raw.Contains(survivor) && f.LastClickPaused && Game.Instance.IsPaused &&
                        !firstPending.IsStarted && !survivor.IsStarted &&
                        ReferenceEquals(NativeFirearmAttackOrder.Get(survivor).Order, secondNew),
                        "Native paused submission replaces an unstarted pending command; the exact second order owns the new command.");
                    long cancelled = EmptyFirearmAttackCommandPatch.AutoReloadCanceledAttacks;
                    f.Turns.FlushFirearmCallbacks();
                    FirearmInputCheck(prefix + "obsolete-callback-keeps-new-order", scheduled.Count == 0 &&
                        EmptyFirearmAttackCommandPatch.AutoReloadCanceledAttacks > cancelled &&
                        BrokenSequenceSuppressionRuntime.Orders.IsCurrent(secondNew) &&
                        f.Actor.Commands.Raw.Contains(survivor),
                        "The real completed old reload callback is discarded without revoking the new target order.");
                    f.Turns.SetFirearmPaused(false);
                    finalTarget = otherEnemy;
                }
                else if (variant.StartsWith("controller-", StringComparison.Ordinal))
                {
                    if (variant == "controller-loaded")
                    {
                        var reload = f.ClickAbility(BlueprintBootstrap.ReloadTestMusketAbility);
                        for (int tick = 0; tick < 240 && f.State.IsEmpty; tick++)
                        { f.Pump(); f.Record(tick); yield return null; }
                        foreach (object step in DriveFirearmIdle(f)) yield return step;
                        FirearmInputCheck(prefix + "manual-reload-alone", reload.IsActed && f.PeakActionCost(reload) > 0 &&
                            f.State.Condition == FirearmCondition.Broken && f.State.LoadedRounds == 1 &&
                            f.Shots.Count == 1 && BrokenSequenceSuppressionRuntime.Orders.Current(f.Actor) == null,
                            f.Describe());
                    }
                    f.Turns.LeaveCombatForRecovery();
                    f.EnableController();
                    FirearmMisfireRuntime.QueueForcedNaturalRoll(10);
                    _firearmInputStage = prefix + "native-direct-interact";
                    f.ControllerInteract(f.Enemy);
                    var order = BrokenSequenceSuppressionRuntime.Orders.Current(f.Actor);
                    FirearmInputCheck(prefix + "accepted-native-controller-order", order != null &&
                        ReferenceEquals(order.Target, f.Enemy) && ReferenceEquals(order.Weapon, f.Weapon) &&
                        f.Actor.Commands.Raw.Any(command => command != null &&
                            ReferenceEquals(NativeFirearmAttackOrder.Get(command)?.Order, order)),
                        "Native OnInteract with its selected actor and resolved target view accepted an ordinary firearm order.");
                }
                else
                {
                    f.MakeEnemyPeaceful(true);
                    f.Turns.LeaveCombatForRecovery();
                    CheckFirearmInteractionInputs(f);
                    _firearmInputStage = prefix + "out-of-combat-action-bar";
                    var kit = Game.Instance.Player.Inventory.Items.Single(item =>
                        ReferenceEquals(item.Blueprint, BlueprintBootstrap.GunsmithingSupplies.GunsmithKit));
                    var repair = f.ClickAbility(BlueprintBootstrap.RepairTestMusketAbility);
                    for (int tick = 0; tick < 240 && f.State.Condition == FirearmCondition.Broken; tick++)
                    { f.Pump(); f.Record(tick); yield return null; }
                    FirearmInputCheck(prefix + "native-repair-preserves-item-kit", repair.IsActed &&
                        f.State.Condition == FirearmCondition.Normal && f.State.IsEmpty && f.SameItemIdentity &&
                        f.Shots.Count == 1 && f.Powder == powder && kit.Count == 1 &&
                        Game.Instance.Player.Inventory.Items.Contains(kit) &&
                        BrokenSequenceSuppressionRuntime.Orders.Current(f.Actor) == null &&
                        BrokenSequenceSuppressionRuntime.IsSuppressed(f.Actor, f.Weapon), f.Describe() +
                        ";repairResult=" + repair.Result + ";partyCombat=" + Game.Instance.Player.IsInCombat);
                    f.MakeEnemyPeaceful(false);
                    FirearmMisfireRuntime.QueueForcedNaturalRoll(10);
                    f.Click(f.Enemy);
                }
                _firearmInputStage = prefix + "native-new-shot";
                for (int tick = 0; tick < 300 && f.Shots.Count < 2; tick++)
                { f.Pump(); f.Record(tick); yield return null; }
                FirearmInputCheck(prefix + "new-order-fires-exact-target", f.Shots.Count == 2 &&
                    ReferenceEquals(f.Shots[1].Target, finalTarget) && f.SameItemIdentity &&
                    f.State.Condition == (variant == "repair" ? FirearmCondition.Normal : FirearmCondition.Broken),
                    f.Describe() + ";exact target=" + finalTarget.UniqueId);
                f.Dispose();
                FirearmInputCheck(prefix + "cleanup", f.Restored, "All native fixture state restored.");
                _firearmInputFixture = null;
            }
        }
    }
}
