using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Items;
using Kingmaker.UnitLogic;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Gunsmithing;

namespace KingmakerGunslinger.Actions
{
    /// <summary>
    /// Resolves exactly one marked firearm across both hands. Marker validation is
    /// deliberately completed before state-repository access, so an ordinary native
    /// weapon cannot acquire firearm metadata during an availability query.
    /// </summary>
    internal static class ExactEquippedFirearmResolver
    {
        private static readonly object AccessGate = new object();
        private static WeaponBlueprintAccess _weaponTypeAccess;

        internal static bool TryResolve(
            UnitDescriptor caster,
            out ExactEquippedFirearmContext context,
            out string reason)
        {
            context = null;
            reason = null;
            if (caster == null)
            {
                reason = "No concrete caster descriptor is available.";
                return false;
            }

            if (caster.Body == null)
            {
                reason = "The caster has no equipment body.";
                return false;
            }

            var equipped = new List<ItemEntityWeapon>();
            AddDistinct(equipped, caster.Body.PrimaryHand == null
                ? null
                : caster.Body.PrimaryHand.MaybeWeapon);
            AddDistinct(equipped, caster.Body.SecondaryHand == null
                ? null
                : caster.Body.SecondaryHand.MaybeWeapon);

            var marked = new List<MarkedWeapon>();
            foreach (ItemEntityWeapon weapon in equipped)
            {
                FirearmDefinition definition;
                if (TryReadDefinition(weapon, out definition))
                {
                    marked.Add(new MarkedWeapon(weapon, definition));
                }
            }

            if (marked.Count == 0)
            {
                reason = "Equip exactly one marked firearm.";
                return false;
            }

            if (marked.Count != 1)
            {
                reason = "More than one distinct marked firearm is equipped; target selection is ambiguous.";
                return false;
            }

            FirearmItemStateSnapshot firearm;
            if (!FirearmRuntimeState.Service.TryGetOrCreate(
                marked[0].Weapon,
                out firearm,
                out reason))
            {
                return false;
            }

            BatteredFirearmUseDecision use =
                new BatteredFirearmRuntimeUseResolver().Evaluate(
                    marked[0].Weapon, caster.Unit,
                    firearm.Repository.State.Condition, 0);
            context = new ExactEquippedFirearmContext(
                marked[0].Weapon,
                marked[0].Definition,
                firearm,
                use.EffectiveCondition);
            return true;
        }

        private static bool TryReadDefinition(
            ItemEntityWeapon weapon,
            out FirearmDefinition definition)
        {
            definition = null;
            if (weapon == null || weapon.Blueprint == null)
            {
                return false;
            }

            BlueprintWeaponType weaponType = GetWeaponTypeAccess().Get(weapon.Blueprint);
            FirearmDefinitionComponent[] markers =
                (weaponType.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                .OfType<FirearmDefinitionComponent>()
                .ToArray();
            if (markers.Length != 1)
            {
                return false;
            }

            definition = markers[0].Definition;
            return definition != null;
        }

        private static WeaponBlueprintAccess GetWeaponTypeAccess()
        {
            lock (AccessGate)
            {
                if (_weaponTypeAccess == null)
                {
                    _weaponTypeAccess = WeaponBlueprintAccess.Resolve();
                }

                return _weaponTypeAccess;
            }
        }

        private static void AddDistinct(
            ICollection<ItemEntityWeapon> candidates,
            ItemEntityWeapon candidate)
        {
            if (candidate == null)
            {
                return;
            }

            foreach (ItemEntityWeapon existing in candidates)
            {
                if (ReferenceEquals(existing, candidate))
                {
                    return;
                }
            }

            candidates.Add(candidate);
        }

        private sealed class MarkedWeapon
        {
            internal MarkedWeapon(ItemEntityWeapon weapon, FirearmDefinition definition)
            {
                Weapon = weapon;
                Definition = definition;
            }

            internal ItemEntityWeapon Weapon { get; private set; }

            internal FirearmDefinition Definition { get; private set; }
        }
    }
}
