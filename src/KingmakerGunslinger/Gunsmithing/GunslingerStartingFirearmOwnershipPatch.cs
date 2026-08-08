using System;
using System.Collections.Generic;
using System.Linq;
using Harmony12;
using Kingmaker;
using Kingmaker.Items;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Entities;
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
            internal ExpectedStartingFirearm Expected;
        }

        private static bool Prefix(UnitDescriptor unit, ref Snapshot __state)
        {
            __state = null;
            if (!IsExactGunslingerReceiver(unit)) return true;
            if (Game.Instance == null || Game.Instance.Player == null ||
                Game.Instance.Player.Inventory == null)
                throw new InvalidOperationException(
                    "The exact player inventory is unavailable before the Gunslinger starting-item grant.");
            var ammunition = BlueprintBootstrap.BasicAmmunition;
            if (ammunition == null)
                throw new InvalidOperationException(
                    "Production basic ammunition is unavailable before the Gunslinger starting-item grant.");
            ExpectedStartingFirearm expected =
                GunslingerStartingFirearmResolver.Resolve(unit);
            if (HasExistingBoundStarter(Game.Instance.Player.Inventory,
                    expected.Item, unit))
                return false;
            __state = new Snapshot
            {
                Descriptor = unit,
                Before = Enumerate(Game.Instance.Player.Inventory),
                PowderBefore = Game.Instance.Player.Inventory.Count(ammunition.BlackPowder),
                BallBefore = Game.Instance.Player.Inventory.Count(ammunition.LeadBall),
                Expected = expected
            };
            return true;
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

            if (__state.Expected == null || __state.Expected.Item == null)
                throw new InvalidOperationException(
                    "The exact expected starting firearm was lost during the native grant.");
            ItemEntityWeapon[] addedFirearms = Enumerate(
                Game.Instance.Player.Inventory)
                .Where(item => !__state.Before.Contains(item))
                .OfType<ItemEntityWeapon>()
                .Where(item => IsProductionFirearm(item.Blueprint))
                .ToArray();
            // Native detached CharGen work can invoke this method without creating
            // a shared-inventory grant. This patch observes and binds a grant; it
            // must not turn an absent native grant into a commit failure.
            if (addedFirearms.Length == 0) return;
            if (addedFirearms.Length != 1 || !ReferenceEquals(
                    addedFirearms[0].Blueprint, __state.Expected.Item))
                throw new InvalidOperationException(
                    "The native Gunslinger starting grant did not create exactly one expected production firearm.");

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
                    addedFirearms[0], __state.Descriptor.Unit);
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

        private static bool HasExistingBoundStarter(object inventory,
            BlueprintItemWeapon expected, UnitDescriptor receiver)
        {
            if (receiver == null || receiver.Unit == null) return false;
            foreach (ItemEntityWeapon item in Enumerate(inventory)
                .OfType<ItemEntityWeapon>().Where(value =>
                    ReferenceEquals(value.Blueprint, expected)))
            {
                UnitEntityData owner;
                if (BatteredFirearmOriginRuntime.TryGetOwner(item, out owner) &&
                    ReferenceEquals(owner, receiver.Unit)) return true;
            }
            return false;
        }

        private static bool IsProductionFirearm(BlueprintItemWeapon item)
        {
            var catalog = BlueprintBootstrap.ProductionFirearms;
            return item != null && catalog != null && catalog.Entries.Any(value =>
                value != null && ReferenceEquals(value.Item, item));
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
