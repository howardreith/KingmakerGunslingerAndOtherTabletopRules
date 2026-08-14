using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;

namespace KingmakerGunslinger.EasternWeapons
{
    internal enum EasternNamedWeaponEffectKind
    {
        FallingPetal,
        MountainSunder,
        UnfixedForm
    }

    [Serializable]
    internal sealed class EasternNamedWeaponEffectComponent :
        WeaponEnchantmentLogic,
        IInitiatorRulebookHandler<RuleAttackWithWeapon>,
        IInitiatorRulebookHandler<RuleCalculateWeaponStats>
    {
        public EasternNamedWeaponEffectKind Kind;
        public BlueprintBuff EffectBuff;
        public BlueprintBuff RoundMarker;
        public BlueprintActivatableAbility PowerAttack;

        public void OnEventAboutToTrigger(RuleAttackWithWeapon evt) { }

        public void OnEventDidTrigger(RuleAttackWithWeapon evt)
        {
            if (!IsExactAttack(evt) || evt.AttackRoll == null) return;
            if (Kind == EasternNamedWeaponEffectKind.FallingPetal)
            {
                if (!evt.AttackRoll.IsHit ||
                    !evt.AttackRoll.IsCriticalConfirmed) return;
                Refresh(evt.Initiator, EffectBuff);
                EasternNamedWeaponEffectDiagnostics.FallingPetalApplications++;
                return;
            }
            if (Kind != EasternNamedWeaponEffectKind.MountainSunder ||
                !evt.AttackRoll.IsHit || evt.Target == null ||
                HasBuff(evt.Initiator, RoundMarker) ||
                !PowerAttackIsRunning(evt.Initiator)) return;

            Refresh(evt.Initiator, RoundMarker);
            var damage = new RuleDealDamage(evt.Initiator, evt.Target,
                new DamageBundle(new ForceDamage(
                    new DiceFormula(1, DiceType.D6))))
            {
                DisablePrecisionDamage = true
            };
            Rulebook.Trigger(damage);
            EasternNamedWeaponEffectDiagnostics.MountainSunderApplications++;
            EasternNamedWeaponEffectDiagnostics.LastMountainSunderDamage =
                damage.Damage;
        }

        public void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
        {
            if (Kind != EasternNamedWeaponEffectKind.UnfixedForm || evt == null ||
                evt.Initiator == null || evt.Weapon == null || Owner == null ||
                !ReferenceEquals(evt.Weapon, Owner)) return;
            UnitDescriptor descriptor = evt.Initiator.Descriptor;
            if (descriptor == null || descriptor.State == null ||
                descriptor.Body == null) return;
            bool changedSize = descriptor.State.Size != descriptor.OriginalSize;
            bool polymorphed = descriptor.Body.IsPolymorphed;
            if (!changedSize && !polymorphed) return;
            evt.IncreaseWeaponSize();
            EasternNamedWeaponEffectDiagnostics.UnfixedFormApplications++;
        }

        public void OnEventDidTrigger(RuleCalculateWeaponStats evt) { }

        private bool IsExactAttack(RuleAttackWithWeapon evt)
        {
            return evt != null && evt.Initiator != null && evt.Weapon != null &&
                Owner != null && ReferenceEquals(evt.Weapon, Owner);
        }

        private bool PowerAttackIsRunning(UnitEntityData unit)
        {
            if (unit == null || unit.Descriptor == null || PowerAttack == null)
                return false;
            ActivatableAbility ability = unit.Descriptor.GetFact(PowerAttack) as
                ActivatableAbility;
            return ability != null && ability.IsRunning;
        }

        private static bool HasBuff(UnitEntityData unit, BlueprintBuff buff)
        {
            return unit != null && unit.Descriptor != null && buff != null &&
                unit.Descriptor.Buffs.GetBuff(buff) != null;
        }

        private void Refresh(UnitEntityData unit, BlueprintBuff buff)
        {
            if (unit == null || unit.Descriptor == null || buff == null) return;
            Buff existing = unit.Descriptor.Buffs.GetBuff(buff);
            if (existing != null) unit.Descriptor.Buffs.RemoveFact(existing);
            unit.Descriptor.Buffs.AddBuff(buff, Context,
                TimeSpan.FromSeconds(6d));
        }
    }

    [Serializable]
    internal sealed class EasternEquipmentStatBonus :
        OwnedGameLogicComponent<UnitDescriptor>, IUnitEquipmentHandler,
        IUnitActiveEquipmentSetHandler, IUnitSubscriber
    {
        public BlueprintItemWeapon Weapon;
        public StatType Stat;
        public int Value;
        public ModifierDescriptor Descriptor;
        public bool RequireOneHanded;
        private ModifiableValue.Modifier _modifier;

        public override void OnTurnOn() { Refresh(); }
        public override void OnTurnOff() { Remove(); }

        public void HandleEquipmentSlotUpdated(ItemSlot slot,
            ItemEntity previousItem)
        { Refresh(); }

        public void HandleUnitChangeActiveEquipmentSet(UnitDescriptor unit)
        { if (ReferenceEquals(unit, Owner)) Refresh(); }

        private void Refresh()
        {
            Remove();
            if (Owner == null || Owner.Body == null || Owner.Stats == null ||
                Weapon == null || !IsElectedFact()) return;
            ItemEntityWeapon active = ActiveWeapon();
            if (active == null || (RequireOneHanded && active.HoldInTwoHands))
                return;
            ModifiableValue stat = Owner.Stats.GetStat(Stat);
            if (stat != null)
                _modifier = stat.AddModifier(Value, Fact, GetType().FullName,
                    Descriptor);
        }

        private ItemEntityWeapon ActiveWeapon()
        {
            ItemEntityWeapon primary = Owner.Body.PrimaryHand == null ? null :
                Owner.Body.PrimaryHand.MaybeWeapon;
            if (primary != null && ReferenceEquals(primary.Blueprint, Weapon))
                return primary;
            ItemEntityWeapon secondary = Owner.Body.SecondaryHand == null ? null :
                Owner.Body.SecondaryHand.MaybeWeapon;
            return secondary != null && ReferenceEquals(secondary.Blueprint,
                Weapon) ? secondary : null;
        }

        private bool IsElectedFact()
        {
            if (!(Fact is Buff) || Owner.Buffs == null) return true;
            Buff first = Owner.Buffs.RawFacts.OfType<Buff>().FirstOrDefault(
                value => ReferenceEquals(value.Blueprint,
                    ((Buff)Fact).Blueprint));
            return first == null || ReferenceEquals(first, Fact);
        }

        private void Remove()
        {
            if (_modifier != null && Owner != null && Owner.Stats != null)
            {
                ModifiableValue stat = Owner.Stats.GetStat(Stat);
                if (stat != null) stat.RemoveModifier(_modifier);
            }
            _modifier = null;
        }
    }

    [Serializable]
    internal sealed class MoonlitCrossingDamageBonus :
        OwnedGameLogicComponent<UnitDescriptor>,
        IInitiatorRulebookHandler<RuleCalculateWeaponStats>
    {
        public BlueprintItemWeapon Weapon;

        public void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
        {
            if (Owner == null || Weapon == null || evt == null ||
                evt.Initiator == null || evt.Weapon == null ||
                !ReferenceEquals(evt.Initiator.Descriptor, Owner) ||
                !ReferenceEquals(evt.Weapon.Blueprint, Weapon) ||
                !evt.Weapon.HoldInTwoHands) return;
            evt.AddBonusDamage(2);
            EasternNamedWeaponEffectDiagnostics.MoonlitDamageApplications++;
        }

        public void OnEventDidTrigger(RuleCalculateWeaponStats evt) { }
    }

    internal static class EasternNamedWeaponEffectDiagnostics
    {
        internal static int FallingPetalApplications;
        internal static int MountainSunderApplications;
        internal static int LastMountainSunderDamage;
        internal static int UnfixedFormApplications;
        internal static int MoonlitDamageApplications;

        internal static void Reset()
        {
            FallingPetalApplications = 0;
            MountainSunderApplications = 0;
            LastMountainSunderDamage = 0;
            UnfixedFormApplications = 0;
            MoonlitDamageApplications = 0;
        }
    }

    internal static class MightyCleavingRuntime
    {
        private const string ThreatHandField = "<threatHand>5__4";
        private const string IsGreaterField = "<isGreater>5__3";
        private static BlueprintItemWeapon _mountainSunder;

        internal static void Configure(BlueprintItemWeapon mountainSunder)
        {
            if (mountainSunder == null) throw new ArgumentNullException(
                "mountainSunder");
            if (_mountainSunder != null &&
                !ReferenceEquals(_mountainSunder, mountainSunder))
                throw new InvalidOperationException(
                    "Mighty Cleaving was configured with another blueprint.");
            _mountainSunder = mountainSunder;
        }

        internal static IEnumerator<AbilityDeliveryTarget> Wrap(
            IEnumerator<AbilityDeliveryTarget> delivery)
        {
            return delivery == null || _mountainSunder == null ? delivery :
                new MightyCleavingEnumerator(delivery, _mountainSunder);
        }

        private sealed class MightyCleavingEnumerator :
            IEnumerator<AbilityDeliveryTarget>
        {
            private readonly IEnumerator<AbilityDeliveryTarget> _inner;
            private readonly BlueprintItemWeapon _weapon;
            private FieldInfo _isGreaterField;
            private bool _initialized;
            private bool _eligible;
            private bool _nativeGreater;
            private int _successfulTargets;

            internal MightyCleavingEnumerator(
                IEnumerator<AbilityDeliveryTarget> inner,
                BlueprintItemWeapon weapon)
            { _inner = inner; _weapon = weapon; }

            public AbilityDeliveryTarget Current { get { return _inner.Current; } }
            object IEnumerator.Current { get { return Current; } }

            public bool MoveNext()
            {
                if (_initialized && _eligible && !_nativeGreater)
                    _isGreaterField.SetValue(_inner,
                        _successfulTargets == 2);
                bool moved = _inner.MoveNext();
                if (!_initialized) Initialize();
                if (moved) _successfulTargets++;
                else RestoreNativeState();
                return moved;
            }

            public void Reset()
            {
                RestoreNativeState();
                _inner.Reset();
                _initialized = false;
                _eligible = false;
                _nativeGreater = false;
                _successfulTargets = 0;
            }

            public void Dispose()
            {
                RestoreNativeState();
                _inner.Dispose();
            }

            private void Initialize()
            {
                Type type = _inner.GetType();
                FieldInfo threatHand = type.GetField(ThreatHandField,
                    BindingFlags.Instance | BindingFlags.NonPublic |
                    BindingFlags.Public);
                _isGreaterField = type.GetField(IsGreaterField,
                    BindingFlags.Instance | BindingFlags.NonPublic |
                    BindingFlags.Public);
                if (threatHand == null ||
                    !typeof(WeaponSlot).IsAssignableFrom(threatHand.FieldType) ||
                    _isGreaterField == null ||
                    _isGreaterField.FieldType != typeof(bool))
                    throw new MissingFieldException(type.FullName,
                        ThreatHandField + "/" + IsGreaterField);
                WeaponSlot slot = (WeaponSlot)threatHand.GetValue(_inner);
                ItemEntityWeapon active = slot == null ? null :
                    slot.MaybeWeapon;
                _eligible = active != null && ReferenceEquals(active.Blueprint,
                    _weapon);
                _nativeGreater = (bool)_isGreaterField.GetValue(_inner);
                _initialized = true;
            }

            private void RestoreNativeState()
            {
                if (_initialized && _eligible && !_nativeGreater)
                    _isGreaterField.SetValue(_inner, false);
            }
        }
    }

    [HarmonyPatch(typeof(AbilityCustomCleave), "Deliver", new[] {
        typeof(AbilityExecutionContext), typeof(TargetWrapper) })]
    internal static class MightyCleavingCleavePatch
    {
        private static void Postfix(
            ref IEnumerator<AbilityDeliveryTarget> __result)
        { __result = MightyCleavingRuntime.Wrap(__result); }
    }
}
