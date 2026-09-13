using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.UI;
using Kingmaker.UI.Selection;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Recovery;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private void CheckFirearmRepairWarnings(FirearmInputFixture f)
        {
            // These assigned conditions exercise rejection presentation only.
            // Broken/Wrecked firing qualification uses actual misfire rules.
            CheckRepairWarning(f, f.Actor, "This firearm is not broken.", "repair-normal");
            SetRepairWarningCondition(f, FirearmCondition.Broken);
            f.Actor.Body.PrimaryHand.RemoveItem(false);
            CheckRepairWarning(f, f.Actor, "Equip a firearm to repair.", "repair-no-firearm");
            f.Actor.Body.PrimaryHand.InsertItem(f.Weapon);
            var second = new ItemEntityWeapon(BlueprintBootstrap.ProductionFirearms.Pistol.Item);
            try
            {
                f.Actor.Body.SecondaryHand.InsertItem(second);
                CheckRepairWarning(f, f.Actor, "Equip only one firearm to repair.", "repair-ambiguous");
            }
            finally
            {
                f.Actor.Body.SecondaryHand.RemoveItem(false);
                FirearmRuntimeState.Service.Forget(second);
            }
            f.Actor.Descriptor.RemoveFact(BlueprintBootstrap.GunslingerClass.Gunsmithing);
            CheckRepairWarning(f, f.Actor, "Requires Gunsmithing.", "repair-capability");
            f.Actor.Descriptor.AddFact(BlueprintBootstrap.GunslingerClass.Gunsmithing);
            foreach (var condition in new[] { UnitCondition.Stunned, UnitCondition.Staggered })
            {
                f.Actor.Descriptor.State.AddCondition(condition, null);
                try { CheckRepairWarning(f, f.Actor, "Cannot repair right now.", "repair-unable-" + condition); }
                finally { f.Actor.Descriptor.State.RemoveCondition(condition); }
            }
            var inventory = Game.Instance.Player.Inventory;
            var kitBlueprint = BlueprintBootstrap.GunsmithingSupplies.GunsmithKit;
            var kits = inventory.Items.Where(item => ReferenceEquals(item.Blueprint, kitBlueprint)).ToArray();
            if (kits.Length != 1 || kits[0].Count != 1)
                throw new InvalidOperationException("The repair warning fixture requires exactly its one disposable reusable kit.");
            inventory.Remove(kits[0], 1);
            CheckRepairWarning(f, f.Actor, "Requires a Gunsmith's Kit.", "repair-kit");
            inventory.Add(kits[0]);
            SetRepairWarningCondition(f, FirearmCondition.Wrecked);
            CheckRepairWarning(f, f.Actor, "This firearm is Wrecked. A full rest is required.", "repair-wrecked");
            CheckRepairWarning(f, f.Actor, "This firearm is Wrecked. A full rest is required.",
                "repair-legacy-alias", BlueprintBootstrap.OverhaulTestMusketAbility);
            CheckRepairSelectionIsolation(f);
            var logic = BlueprintBootstrap.RepairTestMusketAbility.ComponentsArray
                .OfType<RepairTestMusketAbilityLogic>().Single();
            FirearmInputCheck("repair-unknown-context", !logic.IsAvailableFor(null) &&
                logic.GetReasonFor(null) == "Cannot repair right now." &&
                logic.GetReason() == "Cannot repair right now.", "No parameterless or unknown-context state cache.");
        }
        private static void SetRepairWarningCondition(FirearmInputFixture f, FirearmCondition condition)
        {
            FirearmRuntimeState.Service.Set(f.Weapon, new FirearmState(FirearmState.CurrentSchemaVersion,
                condition == FirearmCondition.Wrecked ? 0 : 1,
                condition == FirearmCondition.Wrecked ? null : FirearmStateTokenCatalog.DiagnosticLeadBall, condition));
        }
        private void CheckRepairSelectionIsolation(FirearmInputFixture f)
        {
            const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            if (SelectionManager.Instance != null)
                throw new InvalidOperationException("Repair selection fixture requires no existing selection service.");
            var selectedObject = new GameObject("KMG_Runtime_FirearmRepair_Selection") { hideFlags = HideFlags.HideAndDontSave };
            var selection = selectedObject.AddComponent<SelectionManager>();
            try
            {
                f.Enemy.Descriptor.AddFact(BlueprintBootstrap.GunslingerClass.Gunsmithing);
                foreach (var actor in new[] { f.Actor, f.Enemy, f.Actor, f.Enemy })
                {
                    selection.SelectUnit(actor.View, true, true, false);
                    FirearmInputCheck("repair-selection-exact", ReferenceEquals(selection.SingleSelectedUnit, actor), actor.UniqueId);
                    CheckRepairWarning(f, actor, ReferenceEquals(actor, f.Actor)
                        ? "This firearm is Wrecked. A full rest is required." : "Equip a firearm to repair.",
                        "repair-selected-character-" + actor.UniqueId);
                }
            }
            finally
            {
                typeof(SelectionManager).GetMethod("Clear", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Invoke(selection, null);
                UnityEngine.Object.DestroyImmediate(selectedObject);
                typeof(SelectionManager).GetProperty("Instance", flags).SetValue(null, null, null);
            }
        }
        private void CheckRepairWarning(FirearmInputFixture f, UnitEntityData actor, string expected,
            string name, Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility blueprint = null)
        {
            var ability = new AbilityData(blueprint ?? BlueprintBootstrap.RepairTestMusketAbility, actor.Descriptor);
            var slot = new MechanicActionBarSlotAbility { Unit = actor, Ability = ability };
            var items = Game.Instance.Player.Inventory.Items.ToArray();
            var counts = items.Select(item => item.Count).ToArray();
            var commands = actor.Commands.Raw.ToArray();
            var queued = actor.Commands.Queue.ToArray();
            var cooldown = RepairWarningCooldowns(actor);
            int grit = actor.Descriptor.Resources.GetResourceAmount(BlueprintBootstrap.GunslingerClass.Grit.Resource);
            string state = f.State.ToString();
            string itemId = FirearmRuntimeState.Service.GetOrCreate(f.Weapon).ItemRuntimeId;
            var messages = new FirearmWarningObserver();
            EventBus.Subscribe(messages);
            try
            {
                Action click = () => slot.OnClick();
                if (f.Turns == null) click(); else f.Turns.Execute(click);
            }
            finally { EventBus.Unsubscribe(messages); }
            FirearmInputCheck(name, messages.Text.Count == 1 && messages.Text[0] == expected &&
                !ability.IsAvailable && state == f.State.ToString() && itemId == FirearmRuntimeState.Service.GetOrCreate(f.Weapon).ItemRuntimeId &&
                items.SequenceEqual(Game.Instance.Player.Inventory.Items) && counts.SequenceEqual(items.Select(item => item.Count)) &&
                commands.SequenceEqual(actor.Commands.Raw) && queued.SequenceEqual(actor.Commands.Queue) &&
                cooldown.SequenceEqual(RepairWarningCooldowns(actor)) &&
                grit == actor.Descriptor.Resources.GetResourceAmount(BlueprintBootstrap.GunslingerClass.Grit.Resource),
                "native action-bar warning=" + string.Join(" | ", messages.Text) +
                ";expected=" + expected + ";item=" + itemId + ";state=" + f.State +
                ";commands/resources/ammunition/kit/cooldowns unchanged");
        }
        private static float[] RepairWarningCooldowns(UnitEntityData actor)
        {
            return new[] { actor.CombatState.Cooldown.StandardAction, actor.CombatState.Cooldown.MoveAction,
                actor.CombatState.Cooldown.SwiftAction };
        }
        private sealed class FirearmWarningObserver : IWarningNotificationUIHandler
        {
            internal readonly List<string> Text = new List<string>();
            public void HandleWarning(string text, bool addToLog = true) { Text.Add(text); }
            public void HandleWarning(WarningNotificationType warningType, bool addToLog = true)
            { Text.Add("<native enum:" + warningType + ">"); }
        }
    }
}
