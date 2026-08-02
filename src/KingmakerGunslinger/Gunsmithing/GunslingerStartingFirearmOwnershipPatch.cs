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
            internal int PowderBefore;
            internal int BallBefore;
        }

        private static void Prefix(UnitDescriptor unit, ref Snapshot __state)
        {
            __state = null;
            if (!IsExactGunslingerReceiver(unit)) return;
            if (Game.Instance == null || Game.Instance.Player == null ||
                Game.Instance.Player.Inventory == null)
                throw new InvalidOperationException(
                    "The exact player inventory is unavailable before the Gunslinger starting-item grant.");
            var ammunition = BlueprintBootstrap.BasicAmmunition;
            if (ammunition == null)
                throw new InvalidOperationException(
                    "Production basic ammunition is unavailable before the Gunslinger starting-item grant.");
            __state = new Snapshot
            {
                Descriptor = unit,
                Before = Enumerate(Game.Instance.Player.Inventory),
                PowderBefore = Game.Instance.Player.Inventory.Count(ammunition.BlackPowder),
                BallBefore = Game.Instance.Player.Inventory.Count(ammunition.LeadBall)
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
            // Native detached CharGen work can invoke this method without creating
            // a shared-inventory grant. This patch observes and binds a grant; it
            // must not turn an absent native grant into a commit failure.
            if (addedPistols.Length == 0) return;
            if (addedPistols.Length != 1)
                throw new InvalidOperationException(
                    "The native Gunslinger starting grant created multiple new production Early Pistols.");

            var basic = BlueprintBootstrap.BasicAmmunition;
            int powderDelta = Game.Instance.Player.Inventory.Count(basic.BlackPowder) -
                __state.PowderBefore;
            int ballDelta = Game.Instance.Player.Inventory.Count(basic.LeadBall) -
                __state.BallBefore;
            if (powderDelta != 1 || ballDelta != 1)
                throw new InvalidOperationException(
                    "The native Gunslinger starting grant did not create exactly one of each basic ammunition component.");
            try
            {
                Game.Instance.Player.Inventory.Add(basic.BlackPowder, 19);
                Game.Instance.Player.Inventory.Add(basic.LeadBall, 19);
                if (Game.Instance.Player.Inventory.Count(basic.BlackPowder) -
                        __state.PowderBefore != 20 ||
                    Game.Instance.Player.Inventory.Count(basic.LeadBall) -
                        __state.BallBefore != 20)
                    throw new InvalidOperationException(
                        "The Gunslinger starting ammunition stacks did not reach 20/20.");
                BatteredFirearmOriginRuntime.Bind(
                    addedPistols[0], __state.Descriptor.Unit);
            }
            catch
            {
                int extraPowder = Game.Instance.Player.Inventory.Count(basic.BlackPowder) -
                    __state.PowderBefore - 1;
                int extraBall = Game.Instance.Player.Inventory.Count(basic.LeadBall) -
                    __state.BallBefore - 1;
                if (extraPowder > 0)
                    Game.Instance.Player.Inventory.Remove(basic.BlackPowder, extraPowder);
                if (extraBall > 0)
                    Game.Instance.Player.Inventory.Remove(basic.LeadBall, extraBall);
                throw;
            }
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
