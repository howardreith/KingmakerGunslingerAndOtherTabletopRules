using System;
using System.Collections.Generic;
using System.Linq;
using Harmony12;
using Kingmaker;
using Kingmaker.Items;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Gunsmithing
{
    [HarmonyPatch(typeof(LevelUpHelper), "AddStartingItems")]
    internal static class GunslingerStartingFirearmOwnershipPatch
    {
        private sealed class Snapshot
        {
            internal UnitDescriptor Descriptor;
            internal HashSet<object> Before;
        }

        private static void Prefix(UnitDescriptor unit, ref Snapshot __state)
        {
            __state = null;
            if (!IsExactGunslingerReceiver(unit)) return;
            if (Game.Instance == null || Game.Instance.Player == null ||
                Game.Instance.Player.Inventory == null)
                throw new InvalidOperationException(
                    "The exact player inventory is unavailable before the Gunslinger starting-item grant.");
            __state = new Snapshot
            {
                Descriptor = unit,
                Before = Enumerate(Game.Instance.Player.Inventory)
            };
        }

        private static void Postfix(Snapshot __state)
        {
            if (__state == null) return;
            if (!IsExactGunslingerReceiver(__state.Descriptor))
                throw new InvalidOperationException(
                    "The Gunslinger starting-item receiver changed during the native grant.");
            if (__state.Descriptor.Unit == null ||
                string.IsNullOrWhiteSpace(__state.Descriptor.Unit.UniqueId))
                throw new InvalidOperationException(
                    "The Gunslinger starting-item receiver exposes no stable unit identity.");

            var pistol = BlueprintBootstrap.ProductionFirearms == null ? null :
                BlueprintBootstrap.ProductionFirearms.Pistol.Item;
            if (pistol == null)
                throw new InvalidOperationException(
                    "The production Early Pistol blueprint is unavailable after the native grant.");
            ItemEntityWeapon[] addedPistols = Enumerate(
                Game.Instance.Player.Inventory)
                .Where(item => !__state.Before.Contains(item))
                .OfType<ItemEntityWeapon>()
                .Where(item => ReferenceEquals(item.Blueprint, pistol))
                .ToArray();
            if (addedPistols.Length != 1)
                throw new InvalidOperationException(
                    "The native Gunslinger starting grant did not create exactly one new production Early Pistol.");

            BatteredFirearmOriginRuntime.Bind(
                addedPistols[0], __state.Descriptor.Unit);
        }

        private static bool IsExactGunslingerReceiver(UnitDescriptor unit)
        {
            return unit != null && BlueprintBootstrap.GunslingerClass != null &&
                ReferenceEquals(unit.Progression.GetMaxClass(),
                    BlueprintBootstrap.GunslingerClass.CharacterClass);
        }

        private static HashSet<object> Enumerate(object inventory)
        {
            if (!Development.ReflectionAccess.CanEnumerate(inventory))
                throw new MissingMemberException(
                    "The exact shared inventory is not enumerable.");
            return new HashSet<object>(
                Development.ReflectionAccess.Enumerate(inventory),
                ReferenceIdentityComparer.Instance);
        }
    }
}
