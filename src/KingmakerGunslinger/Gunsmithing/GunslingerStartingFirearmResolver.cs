using System;
using System.Linq;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.UnitLogic;
using KingmakerGunslinger.Blueprints;

namespace KingmakerGunslinger.Gunsmithing
{
    internal sealed class ExpectedStartingFirearm
    {
        internal ExpectedStartingFirearm(BlueprintItemWeapon item,
            string source)
        {
            Item = item ?? throw new ArgumentNullException("item");
            Source = source ?? string.Empty;
        }

        internal BlueprintItemWeapon Item { get; private set; }
        internal string Source { get; private set; }
    }

    internal static class GunslingerStartingFirearmResolver
    {
        private static BlueprintCharacterClass _gunslinger;
        private static BlueprintItemWeapon _pistol;
        private static BlueprintItemWeapon _musket;
        private static BlueprintArchetype _pistolero;
        private static BlueprintArchetype _musketMaster;

        internal static void Configure(BlueprintCharacterClass gunslinger,
            BlueprintItemWeapon pistol, BlueprintItemWeapon musket,
            BlueprintArchetype pistolero, BlueprintArchetype musketMaster)
        {
            _gunslinger = gunslinger ?? throw new ArgumentNullException("gunslinger");
            _pistol = pistol ?? throw new ArgumentNullException("pistol");
            _musket = musket ?? throw new ArgumentNullException("musket");
            _pistolero = pistolero;
            _musketMaster = musketMaster;
        }

        internal static ExpectedStartingFirearm Resolve(UnitDescriptor unit)
        {
            if (unit == null || unit.Progression == null || _gunslinger == null ||
                _pistol == null || _musket == null)
                throw new InvalidOperationException(
                    "The exact Gunslinger starting-firearm resolver is unavailable.");
            ClassData data = unit.Progression.GetClassData(_gunslinger);
            if (data == null)
                throw new InvalidOperationException(
                    "The receiver has no exact Gunslinger class data.");
            BlueprintArchetype[] archetypes = (data.Archetypes ??
                new System.Collections.Generic.List<BlueprintArchetype>()).ToArray();
            StartingFirearmProfile profile = StartingFirearmPolicy.Resolve(
                _musketMaster != null && archetypes.Any(value =>
                    ReferenceEquals(value, _musketMaster)),
                _pistolero != null && archetypes.Any(value =>
                    ReferenceEquals(value, _pistolero)), false);
            BlueprintItemWeapon item = StartingFirearmPolicy.ExpectsMusket(profile)
                ? _musket : _pistol;
            return new ExpectedStartingFirearm(item,
                profile.ToString().ToLowerInvariant());
        }

        internal static bool MatchesConfiguration(BlueprintCharacterClass gunslinger,
            BlueprintItemWeapon pistol, BlueprintItemWeapon musket,
            BlueprintArchetype pistolero, BlueprintArchetype musketMaster)
        {
            return ReferenceEquals(_gunslinger, gunslinger) &&
                ReferenceEquals(_pistol, pistol) &&
                ReferenceEquals(_musket, musket) &&
                ReferenceEquals(_pistolero, pistolero) &&
                ReferenceEquals(_musketMaster, musketMaster);
        }

        internal static void Rollback()
        {
            _gunslinger = null;
            _pistol = null;
            _musket = null;
            _pistolero = null;
            _musketMaster = null;
        }
    }
}
