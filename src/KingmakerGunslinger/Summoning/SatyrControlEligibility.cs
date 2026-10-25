using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Brain;
using Kingmaker.Controllers.Brain.Blueprints.Considerations;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>Creature-owned choice policy. Native saves, immunity, spell
    /// effects and action/resource costs remain responsible for resolution.</summary>
    public sealed class SatyrControlEligibility : Consideration
    {
        // Installed 2.1.7b keeps this predicate private. Calling it preserves
        // caster exceptions and conditional facts, unlike testing descriptor
        // flags alone. No rule or effect is executed while choosing a target.
        private static readonly MethodInfo BuffImmunity = AccessTools.Method(
            typeof(BuffDescriptorImmunity), "IsImmune", new[] { typeof(MechanicsContext) }) ??
            throw new MissingMethodException(typeof(BuffDescriptorImmunity).FullName, "IsImmune");
        public BlueprintUnit OwningBlueprint;
        public BlueprintAbility Ability;
        public int MaximumHitDice;
        public bool FearCone;
        public UnitCondition? RejectedCondition;

        public override float Score(DecisionContext context)
        {
            var actor = context?.Unit;
            var target = context?.Target?.Unit;
            if (actor == null || target == null || Ability == null ||
                !ReferenceEquals(actor.Blueprint, OwningBlueprint) ||
                !actor.IsEnemy(target) || target.Destroyed || !target.IsInState ||
                target.Descriptor.State.IsDead || !actor.HasLOS(target) ||
                !actor.Memory.ContainsVisible(target) ||
                MaximumHitDice > 0 && target.Descriptor.Progression.CharacterLevel > MaximumHitDice ||
                target.Descriptor.State.HasCondition(UnitCondition.Frightened) ||
                target.Descriptor.State.HasCondition(UnitCondition.Sleeping)) return 0f;
            var granted = actor.Descriptor.Abilities.GetAbility(Ability);
            if (granted == null) return 0f;
            var data = new AbilityData(granted);
            if (!data.IsAvailable || !data.CanTarget(new TargetWrapper(target))) return 0f;
            var effect = new MechanicsContext(actor, actor.Descriptor, Ability,
                null, new TargetWrapper(target));
            var immunity = target.Get<UnitPartSpellResistance>();
            if (immunity != null && immunity.IsImmune(effect)) return 0f;
            if (target.Descriptor.Progression.Features.Enumerable.Cast<Kingmaker.Blueprints.Facts.Fact>()
                .Concat(target.Descriptor.Buffs.Enumerable).Where(fact => fact.IsTurnedOn)
                .SelectMany(fact => fact.SelectComponents<BuffDescriptorImmunity>())
                .Any(value => (bool)BuffImmunity.Invoke(value, new object[] { effect }))) return 0f;
            if (RejectedCondition.HasValue &&
                target.Descriptor.State.HasConditionImmunity(RejectedCondition.Value)) return 0f;
            if (FearCone)
            {
                if (target.Descriptor.State.HasConditionImmunity(UnitCondition.Frightened) ||
                    Game.Instance?.State?.Units == null) return 0f;
                // Conservative footprint includes each ally's collision radius.
                // Refusal never removes an ally from the native spell's targets.
                var direction = target.Position - actor.Position;
                direction.y = 0;
                if (direction.sqrMagnitude < 0.001f) return 0f;
                direction.Normalize();
                float reach = new Feet(30).Meters;
                if (Game.Instance.State.Units.All.Any(ally => {
                    if (ally == null || ally.Destroyed || !ally.IsInState ||
                        ReferenceEquals(ally, actor) || actor.IsEnemy(ally) ||
                        ally.Descriptor.State.IsDead) return false;
                    var delta = ally.Position - actor.Position; delta.y = 0;
                    float distance = delta.magnitude;
                    float radius = Math.Max(0f, ally.Corpulence);
                    return distance <= reach + radius && (distance <= radius ||
                        Vector3.Angle(direction, delta) <= 45f +
                        Mathf.Asin(Mathf.Clamp01(radius / distance)) * Mathf.Rad2Deg);
                })) return 0f;
            }
            return 1f;
        }
    }

    // Native full attacks append every additional limb, regardless of the
    // ranged primary's distance. Keep the Satyr's own horns only when they
    // can actually reach; ordinary melee approach and every other actor keep
    // the native plan. This supplies no attack, command or action credit.
    [HarmonyPatch(typeof(UnitAttack), "CreateFullAttack")]
    internal static class SatyrRangedHorns32
    {
        private static void Postfix(UnitAttack __instance, List<AttackHandInfo> __result)
        {
            var actor = __instance?.Executor;
            var target = __instance?.Target;
            if (__result == null || actor?.Blueprint?.name != "KMG_Summoning_Unit_Satyr" ||
                target == null || actor.Body?.PrimaryHand?.MaybeWeapon?.Blueprint.IsRanged != true) return;
            float distance = actor.DistanceTo(target) - actor.Corpulence - target.Corpulence;
            __result.RemoveAll(info => info?.Hand?.MaybeWeapon?.Blueprint.name ==
                "KMG_Summoning_FeyCelestial_Satyr_Horns" &&
                distance > info.Hand.MaybeWeapon.AttackRange.Meters);
        }
    }

    // Chooses only the native hand-set switch. Hysteresis keeps a moving foe
    // near the melee boundary from provoking repeated switches. The native
    // action still determines availability, chosen set and normal action cost.
    public sealed class SatyrWeaponChoice32 : Consideration
    {
        public BlueprintUnit OwningBlueprint;
        public bool Ranged;
        public override float Score(DecisionContext context)
        {
            var actor = context?.Unit;
            if (actor == null || !ReferenceEquals(actor.Blueprint, OwningBlueprint) ||
                !actor.IsInCombat || Game.Instance?.State?.Units == null) return 0f;
            float nearest = float.PositiveInfinity;
            foreach (var enemy in Game.Instance.State.Units.All)
            {
                if (enemy == null || enemy.Destroyed || !enemy.IsInState || enemy.Descriptor.State.IsDead ||
                    !actor.IsEnemy(enemy) || !actor.Memory.ContainsVisible(enemy) || !actor.HasLOS(enemy)) continue;
                nearest = Math.Min(nearest, actor.DistanceTo(enemy) - actor.Corpulence - enemy.Corpulence);
            }
            if (float.IsPositiveInfinity(nearest)) return 0f;
            return (Ranged ? nearest > new Feet(10).Meters : nearest <= new Feet(5).Meters) ? 1f : 0f;
        }
    }
}
