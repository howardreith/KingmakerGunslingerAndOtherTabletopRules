using System;
using System.Reflection;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Enums.Damage;
using Kingmaker.Localization;
using Kingmaker.RuleSystem;
using Kingmaker.Utility;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Blueprints
{
    /// <summary>
    /// Exact private-field adapter for production weapon-type mechanics. Every field
    /// is resolved by its installed Kingmaker name and type, assigned once, and
    /// verified through the public read surface.
    /// </summary>
    internal sealed class WeaponTypeMechanicalAccess
    {
        private const BindingFlags Fields =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private readonly FieldInfo _typeName;
        private readonly FieldInfo _defaultName;
        private readonly FieldInfo _description;
        private readonly FieldInfo _masterworkDescription;
        private readonly FieldInfo _magicDescription;
        private readonly FieldInfo _attackRange;
        private readonly FieldInfo _baseDamage;
        private readonly FieldInfo _criticalRollEdge;
        private readonly FieldInfo _criticalModifier;
        private readonly FieldInfo _weight;
        private readonly FieldInfo _isTwoHanded;

        private WeaponTypeMechanicalAccess()
        {
            Type type = typeof(BlueprintWeaponType);
            _typeName = Require(type, "m_TypeNameText", typeof(LocalizedString));
            _defaultName = Require(type, "m_DefaultNameText", typeof(LocalizedString));
            _description = Require(type, "m_DescriptionText", typeof(LocalizedString));
            _masterworkDescription = Require(type, "m_MasterworkDescriptionText", typeof(LocalizedString));
            _magicDescription = Require(type, "m_MagicDescriptionText", typeof(LocalizedString));
            _attackRange = Require(type, "m_AttackRange", typeof(Feet));
            _baseDamage = Require(type, "m_BaseDamage", typeof(DiceFormula));
            _criticalRollEdge = Require(type, "m_CriticalRollEdge", typeof(int));
            _criticalModifier = Require(type, "m_CriticalModifier", typeof(DamageCriticalModifierType));
            _weight = Require(type, "m_Weight", typeof(float));
            _isTwoHanded = Require(type, "m_IsTwoHanded", typeof(bool));
        }

        internal static WeaponTypeMechanicalAccess Resolve()
        {
            return new WeaponTypeMechanicalAccess();
        }

        internal void Configure(
            BlueprintWeaponType weaponType,
            ProductionFirearmWeaponSpec spec,
            LocalizedString name,
            LocalizedString description)
        {
            if (weaponType == null) throw new ArgumentNullException("weaponType");
            if (spec == null) throw new ArgumentNullException("spec");
            if (name == null) throw new ArgumentNullException("name");
            if (description == null) throw new ArgumentNullException("description");

            _typeName.SetValue(weaponType, name);
            _defaultName.SetValue(weaponType, name);
            _description.SetValue(weaponType, description);
            _masterworkDescription.SetValue(weaponType, description);
            _magicDescription.SetValue(weaponType, description);
            if (spec.Definition.HasFixedRangeIncrement)
            {
                _attackRange.SetValue(
                    weaponType,
                    new Feet(spec.Definition.RangeIncrementFeet));
            }
            _baseDamage.SetValue(
                weaponType,
                new DiceFormula(spec.DamageDiceCount, ResolveDie(spec.DamageDieSides)));
            _criticalRollEdge.SetValue(weaponType, 20);
            _criticalModifier.SetValue(
                weaponType,
                (DamageCriticalModifierType)spec.CriticalMultiplier);
            _weight.SetValue(weaponType, spec.WeightPounds);
            _isTwoHanded.SetValue(weaponType, spec.IsTwoHanded);

            Validate(weaponType, spec, name, description);
        }

        internal void Validate(
            BlueprintWeaponType weaponType,
            ProductionFirearmWeaponSpec spec,
            LocalizedString name,
            LocalizedString description)
        {
            if (!ReferenceEquals(weaponType.TypeName, name) ||
                !ReferenceEquals(weaponType.DefaultName, name) ||
                !ReferenceEquals(weaponType.Description, description) ||
                !ReferenceEquals(weaponType.MasterworkDescription, description) ||
                !ReferenceEquals(weaponType.MagicDescription, description))
            {
                throw new InvalidOperationException(
                    "Production firearm localization did not persist through the public read surface.");
            }

            var expectedDamage = new DiceFormula(
                spec.DamageDiceCount,
                ResolveDie(spec.DamageDieSides));
            if (!weaponType.BaseDamage.Equals(expectedDamage) ||
                weaponType.CriticalRollEdge != 20 ||
                weaponType.CriticalModifier != (DamageCriticalModifierType)spec.CriticalMultiplier ||
                weaponType.IsTwoHanded != spec.IsTwoHanded ||
                !weaponType.Weight.Equals(spec.WeightPounds))
            {
                throw new InvalidOperationException(
                    "Production firearm damage, critical, handedness, or weight did not persist.");
            }

            if (spec.Definition.HasFixedRangeIncrement &&
                weaponType.AttackRange.Value != spec.Definition.RangeIncrementFeet)
            {
                throw new InvalidOperationException(
                    "Production firearm attack range did not persist.");
            }
        }

        private static DiceType ResolveDie(int sides)
        {
            DiceType result = (DiceType)sides;
            if (!Enum.IsDefined(typeof(DiceType), result) || result == DiceType.Zero)
            {
                throw new ArgumentOutOfRangeException("sides");
            }

            return result;
        }

        private static FieldInfo Require(Type type, string name, Type expectedType)
        {
            FieldInfo field = type.GetField(name, Fields);
            if (field == null || field.FieldType != expectedType)
            {
                throw new MissingFieldException(type.FullName, name);
            }

            return field;
        }
    }
}
