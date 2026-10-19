using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>Exact project rock accuracy and live damage, plus the alternate
    /// slam's plain Strength contribution. Manufactured weapons retain their
    /// ordinary native two-handed calculation.</summary>
    [Serializable]
    public sealed class SummonGiantRockAttack :
        RuleInitiatorLogicComponent<RuleCalculateWeaponStats>,
        IInitiatorRulebookHandler<RuleCalculateAttackBonusWithoutTarget>
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintItemWeapon Rock;
        public BlueprintItemWeapon Slam;

        public void OnEventAboutToTrigger(
            RuleCalculateAttackBonusWithoutTarget evt)
        {
            UnitEntityData owner = Owner == null ? null : Owner.Unit;
            if (evt == null || owner == null ||
                !ReferenceEquals(owner.Blueprint, OwningBlueprint) ||
                !ReferenceEquals(evt.Initiator, owner) || evt.Weapon == null ||
                !ReferenceEquals(evt.Weapon.Blueprint, Rock))
                return;
            evt.AddBonus(GiantHumanoidRulesPolicy.RacialRockAttackBonus, Fact);
        }

        public void OnEventDidTrigger(
            RuleCalculateAttackBonusWithoutTarget evt) { }

        public override void OnEventAboutToTrigger(
            RuleCalculateWeaponStats evt) { }

        public override void OnEventDidTrigger(
            RuleCalculateWeaponStats evt)
        {
            UnitEntityData owner = Owner == null ? null : Owner.Unit;
            if (evt == null || owner == null ||
                !ReferenceEquals(owner.Blueprint, OwningBlueprint) ||
                !ReferenceEquals(evt.Initiator, owner) || evt.Weapon == null ||
                evt.DamageDescription == null ||
                evt.DamageDescription.Count == 0)
                return;
            bool rock = ReferenceEquals(evt.Weapon.Blueprint, Rock);
            bool slam = ReferenceEquals(evt.Weapon.Blueprint, Slam);
            if (!rock && !slam) return;
            int strength = owner.Descriptor.Stats.Strength.Bonus;
            int native = evt.DamageBonusStat == StatType.Strength ?
                (strength > 0 ?
                    (int)(strength * evt.DamageBonusStatMultiplier) :
                    strength) : 0;
            int desired = rock ?
                GiantHumanoidRulesPolicy.OneAndHalfStrength(strength) :
                strength;
            int correction = desired - native;
            if (correction == 0) return;
            var row = evt.DamageDescription[0];
            row.Bonus += correction;
            evt.DamageDescription[0] = row;
        }
    }

    /// <summary>Only the three project giant units have their native full
    /// attack list adjusted. An armed or rock-wielding giant does not also
    /// swing its two-slam alternative. Stone prepares a second rock through
    /// its printed Quick Draw routine; Hill and Fire throw one.</summary>
    [HarmonyPatch(typeof(UnitAttack), "CreateFullAttack")]
    internal static class SummonGiantFullAttackSequence
    {
        private static void Postfix(UnitAttack __instance,
            List<AttackHandInfo> __result)
        {
            if (__instance == null || __result == null) return;
            UnitEntityData owner = __instance.Executor;
            string key = GiantKey(owner);
            if (key == null || owner.Body == null ||
                owner.Body.PrimaryHand == null)
                return;
            ItemEntityWeapon active = owner.Body.PrimaryHand.MaybeWeapon;
            if (active == null || active.Blueprint == null) return;
            bool isSlam = active.Blueprint.name ==
                "KMG_Summoning_Giant_Slam1d8";
            if (isSlam) return;
            var limbs = owner.Body.AdditionalLimbs;
            if (limbs != null)
                __result.RemoveAll(info => info != null &&
                    limbs.Contains(info.Hand));
            bool rock = active.Blueprint.name ==
                "KMG_Summoning_Giant_" +
                    (key == GiantHumanoidRulesPolicy.Hill ? "Hill" :
                     key == GiantHumanoidRulesPolicy.Stone ? "Stone" :
                     "Fire") + "Rock1d8";
            if (!rock) return;
            var shots = __result.Where(info => info != null &&
                ReferenceEquals(info.Hand, owner.Body.PrimaryHand)).ToArray();
            if (shots.Length == 0) return;
            // A charge and a single attack never gain a second throw.
            if (__instance.IsCharge) return;
            int allowed = GiantHumanoidRulesPolicy.RockFullAttackCount(key);
            if (shots.Length > allowed)
            {
                foreach (AttackHandInfo extra in shots.Skip(allowed))
                    __result.Remove(extra);
            }
            else if (allowed == 2 && shots.Length == 1)
            {
                AttackHandInfo first = shots[0];
                __result.Add(new AttackHandInfo(first.Hand,
                    first.AttackBonusPenalty - 5,
                    first.AttackNumber + 1));
            }
        }

        internal static string GiantKey(UnitEntityData owner)
        {
            string name = owner == null || owner.Blueprint == null ?
                null : owner.Blueprint.name;
            if (name == "KMG_Summoning_Unit_HillGiant")
                return GiantHumanoidRulesPolicy.Hill;
            if (name == "KMG_Summoning_Unit_StoneGiant")
                return GiantHumanoidRulesPolicy.Stone;
            if (name == "KMG_Summoning_Unit_FireGiant")
                return GiantHumanoidRulesPolicy.Fire;
            return null;
        }
    }

    /// <summary>A target-side, once-per-round automatic defensive choice.
    /// The native attack has already rolled but has not resolved weapon
    /// damage. Only a would-hit legal rock can prompt a native Reflex save;
    /// on success the stored attack result becomes a miss before damage.</summary>
    [Serializable]
    public sealed class SummonGiantRockCatch :
        OwnedGameLogicComponent<UnitDescriptor>,
        ITargetRulebookHandler<RuleAttackRoll>
    {
        private sealed class Ledger { internal long Round = long.MinValue; }
        private static readonly ConditionalWeakTable<UnitEntityData, Ledger>
            Ledgers = new ConditionalWeakTable<UnitEntityData, Ledger>();
        private static readonly FieldInfo ResultField =
            typeof(RuleAttackRoll).GetField("<Result>k__BackingField",
                BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly object Sync = new object();
        private static readonly List<string> Outcomes = new List<string>();

        public BlueprintUnit OwningBlueprint;
        public bool ImprovedStoneCatch;

        internal static IReadOnlyList<string> ObservedOutcomes
        { get { lock (Sync) { return Outcomes.ToArray(); } } }
        internal static void ClearOutcomes()
        { lock (Sync) { Outcomes.Clear(); } }

        public void OnEventAboutToTrigger(RuleAttackRoll evt) { }

        public void OnEventDidTrigger(RuleAttackRoll evt)
        {
            UnitEntityData target = Owner == null ? null : Owner.Unit;
            if (evt == null || target == null ||
                !ReferenceEquals(target.Blueprint, OwningBlueprint) ||
                !ReferenceEquals(evt.Target, target) || !evt.IsHit ||
                evt.IsTargetFlatFooted || target.Descriptor.State.IsHelpless ||
                evt.Weapon == null || evt.Weapon.Blueprint == null ||
                !IsEligibleRock(evt.Weapon.Blueprint) ||
                ReferenceEquals(evt.Initiator, target) ||
                ResultField == null)
                return;
            Size size;
            if (!ProjectileSize(evt.Initiator, out size)) return;
            long round = ContextActionUngulateTrample.CurrentRound();
            Ledger ledger = Ledgers.GetOrCreateValue(target);
            lock (ledger)
            {
                if (ledger.Round == round) return;
                ledger.Round = round;
            }
            int enhancement = Math.Max(0,
                GameHelper.GetItemEnhancementBonus(evt.Weapon));
            int dc = GiantHumanoidRulesPolicy.CatchDifficultyClass(
                size.ToString(), enhancement);
            // The +4 is only on Stone's catch check; no Reflex save changes.
            var save = new RuleSavingThrow(target, SavingThrowType.Reflex,
                dc - (ImprovedStoneCatch ?
                    GiantHumanoidRulesPolicy.StoneRockCatchBonus : 0));
            Rulebook.Trigger(save);
            if (save.IsPassed)
                ResultField.SetValue(evt, AttackResult.Miss);
            lock (Sync)
            {
                if (Outcomes.Count >= 128) Outcomes.RemoveAt(0);
                Outcomes.Add(target.UniqueId + ":round=" + round +
                    ";dc=" + dc + ";stone=" + ImprovedStoneCatch +
                    ";save=" + save.IsPassed + ";result=" + evt.Result);
            }
        }

        private static bool IsEligibleRock(BlueprintItemWeapon weapon)
        {
            if (weapon == null || weapon.Type == null ||
                !weapon.IsNatural || weapon.AttackType != AttackType.Ranged)
                return false;
            string type = weapon.Type.name;
            return type == "RockThrow" ||
                type == "KMG_Summoning_Giant_HillRockType" ||
                type == "KMG_Summoning_Giant_StoneRockType" ||
                type == "KMG_Summoning_Giant_FireRockType";
        }

        private static bool ProjectileSize(UnitEntityData thrower, out Size size)
        {
            size = Size.Small;
            if (thrower == null || thrower.Descriptor == null) return false;
            switch (thrower.Descriptor.State.Size)
            {
                case Size.Large: size = Size.Small; return true;
                case Size.Huge: size = Size.Medium; return true;
                case Size.Gargantuan: size = Size.Large; return true;
                default: return false;
            }
        }
    }
}
