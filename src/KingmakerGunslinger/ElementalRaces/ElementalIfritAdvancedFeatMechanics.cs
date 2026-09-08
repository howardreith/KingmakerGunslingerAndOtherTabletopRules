using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using KingmakerGunslinger.Bootstrap;
using TurnBased.Controllers;

namespace KingmakerGunslinger.ElementalRaces
{
    public enum ElementalFiresightConcealmentKind
    {
        None = 0,
        Fire = 1,
        Smoke = 2,
        FogMistOrCloud = 3
    }

    /// <summary>
    /// Explicit semantic ownership marker for project-created concealment.
    /// Firesight never infers fire or smoke from a name, descriptor, visual,
    /// ability parent, or another mod's blueprint.
    /// </summary>
    [Serializable]
    public sealed class ElementalFiresightConcealmentSource :
        BlueprintComponent
    {
        public ElementalFiresightConcealmentKind Kind;
    }

    [Serializable]
    public sealed class ElementalBlazingAuraAbilityLogic :
        AbilityCustomLogic, IAbilityAvailabilityProvider
    {
        public BlueprintRace Ifrit;
        public BlueprintBuff ScorchingWeaponsMarker;
        public BlueprintBuff Aura;

        public bool IsAvailableFor(AbilityData ability)
        {
            UnitEntityData caster = ability == null || ability.Caster == null
                ? null : ability.Caster.Unit;
            return IsExactIfrit(caster) && ScorchingWeaponsMarker != null &&
                Aura != null && caster.Descriptor.Buffs.GetBuff(
                    ScorchingWeaponsMarker) != null &&
                caster.Descriptor.Buffs.GetBuff(Aura) == null &&
                IsOwnersTurn(caster);
        }

        public string GetReason()
        {
            return "Blazing Aura is available only to an Ifrit on the user's turn while Scorching Weapons is active.";
        }

        public override IEnumerator<AbilityDeliveryTarget> Deliver(
            AbilityExecutionContext context, TargetWrapper target)
        {
            if (context == null || context.Caster == null ||
                !IsAvailableFor(context.Ability))
                throw new InvalidOperationException(
                    "Blazing Aura prerequisites changed before execution.");

            Buff applied = context.Caster.Descriptor.Buffs.AddBuff(Aura,
                context, TimeSpan.FromSeconds(6d));
            if (applied == null || !ReferenceEquals(
                    context.Caster.Descriptor.Buffs.GetBuff(Aura), applied))
            {
                if (applied != null)
                    context.Caster.Descriptor.Buffs.RemoveFact(applied);
                throw new InvalidOperationException(
                    "Kingmaker rejected the Blazing Aura round marker.");
            }
            yield return new AbilityDeliveryTarget(target);
        }

        public override void Cleanup(AbilityExecutionContext context) { }

        private bool IsExactIfrit(UnitEntityData caster)
        {
            return caster != null && caster.Descriptor != null &&
                caster.Descriptor.Progression != null && Ifrit != null &&
                ReferenceEquals(caster.Descriptor.Progression.Race, Ifrit);
        }

        internal static bool IsOwnersTurn(UnitEntityData caster)
        {
            if (caster == null) return false;
            if (!CombatController.IsInTurnBasedCombat()) return true;
            CombatController controller = Game.Instance == null ? null :
                Game.Instance.TurnBasedCombatController;
            return controller != null && controller.CurrentTurn != null &&
                ReferenceEquals(controller.CurrentTurn.Unit, caster);
        }
    }

    internal static class ElementalBlazingAuraRuntime
    {
        private static readonly ConditionalWeakTable<object, object>
            ClaimedTurns = new ConditionalWeakTable<object, object>();
        private static readonly object ClaimMarker = new object();

        internal static void HandleTurnStarted(TurnController turn)
        {
            if (turn != null)
                HandleCreatureTurnStarted(turn.Unit, turn);
        }

        // The object identity is the exact TurnController in production. The
        // separate parameter keeps save-free guarded tests off combat-global
        // controller state while exercising this same damage and dedupe path.
        internal static RuleDealDamage[] HandleCreatureTurnStarted(
            UnitEntityData creature, object turnIdentity)
        {
            var applied = new List<RuleDealDamage>();
            if (turnIdentity == null || !TryClaim(turnIdentity) ||
                !IsLiveCreature(creature) || Game.Instance == null ||
                Game.Instance.State == null ||
                Game.Instance.State.Units == null) return applied.ToArray();
            ElementalFeatBlueprintSet blueprints =
                BlueprintBootstrap.ElementalFeats;
            if (blueprints == null) return applied.ToArray();
            BlueprintBuff aura = blueprints.RequireSymbol<BlueprintBuff>(
                ElementalRaceIdentityCatalog.BlazingAuraBuff);

            foreach (UnitEntityData owner in Game.Instance.State.Units.All
                .Where(IsLiveCreature).ToArray())
            {
                if (ReferenceEquals(owner, creature) ||
                    owner.Descriptor.Buffs.GetBuff(aura) == null ||
                    !ElementalFeatPolicy.BlazingAuraIsAdjacent(
                        owner.DistanceTo(creature), owner.Corpulence,
                        creature.Corpulence)) continue;

                var packet = new EnergyDamage(
                    new DiceFormula(1, DiceType.D6), DamageEnergyType.Fire);
                var damage = new RuleDealDamage(owner, creature,
                    new DamageBundle(packet))
                {
                    DisablePrecisionDamage = true
                };
                Rulebook.Trigger(damage);
                applied.Add(damage);
            }
            return applied.ToArray();
        }

        private static bool IsLiveCreature(UnitEntityData unit)
        {
            return unit != null && unit.Descriptor != null &&
                unit.Descriptor.State != null && unit.IsInGame &&
                !unit.Destroyed && !unit.IsDetached && unit.IsTurnedOn &&
                unit.Descriptor.State.IsConscious &&
                !unit.Descriptor.State.IsDead;
        }

        private static bool TryClaim(object turnIdentity)
        {
            lock (ClaimedTurns)
            {
                object ignored;
                if (ClaimedTurns.TryGetValue(turnIdentity, out ignored))
                    return false;
                ClaimedTurns.Add(turnIdentity, ClaimMarker);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(TurnController), "Prepare", new Type[0])]
    internal static class ElementalBlazingAuraTurnPreparePatch
    {
        private static void Postfix(TurnController __instance)
        {
            try { ElementalBlazingAuraRuntime.HandleTurnStarted(__instance); }
            catch (Exception)
            {
                // Fail closed: a turn-start observer must not interrupt combat.
            }
        }
    }

    internal static class ElementalFiresightRuntime
    {
        private const string NativeInvisibilityComponent =
            "Kingmaker.Designers.Mechanics.Buffs.BuffInvisibility";
        private const string NativeDarknessStatusBuff =
            "64737e33d1d185b4194798e9abee76ca";

        internal static bool ShouldBypass(RuleConcealmentCheck check,
            bool nativeResult)
        {
            RulebookEventContext context = Rulebook.CurrentContext;
            RuleAttackRoll attack = context == null ? null :
                context.LastEvent<RuleAttackRoll>();
            bool exact = attack != null && check != null &&
                ReferenceEquals(attack.ConcealmentCheck, check) &&
                attack.Initiator != null && attack.Target != null;
            if (!exact) return false;

            ElementalFeatBlueprintSet blueprints =
                BlueprintBootstrap.ElementalFeats;
            BlueprintFeature firesight = blueprints == null ? null :
                blueprints.RequireFeature(ElementalFeatId.Firesight);
            UnitEntityData attacker = attack.Initiator;
            UnitEntityData target = attack.Target;
            bool owns = firesight != null && attacker.Descriptor != null &&
                attacker.Descriptor.HasFact(firesight);
            bool canSee = attacker.Descriptor != null &&
                attacker.Descriptor.State != null &&
                !attacker.Descriptor.State.HasCondition(
                    UnitCondition.Blindness) &&
                !HasActiveBuff(attacker, NativeDarknessStatusBuff);
            bool invisible = HasComponent(target,
                NativeInvisibilityComponent);

            int qualifying = 0;
            int unrelated = 0;
            foreach (Buff buff in ActiveBuffs(target))
            {
                AddConcealment[] sources = Components(buff)
                    .OfType<AddConcealment>().ToArray();
                if (sources.Length == 0) continue;
                bool exactNative = ElementalFeatPolicy
                    .IsExactNativeFiresightConcealmentGuid(
                        buff.Blueprint.AssetGuid);
                ElementalFiresightConcealmentSource marker = Components(buff)
                    .OfType<ElementalFiresightConcealmentSource>()
                    .SingleOrDefault();
                bool projectFireOrSmoke = marker != null &&
                    (marker.Kind == ElementalFiresightConcealmentKind.Fire ||
                     marker.Kind == ElementalFiresightConcealmentKind.Smoke);
                bool classificationMatches = sources.Any(value =>
                    value.Concealment == check.Concealment);
                if ((exactNative || projectFireOrSmoke) &&
                    classificationMatches) qualifying++;
                else unrelated++;
            }

            return ElementalFeatPolicy.FiresightCanBypass(!nativeResult,
                exact, owns, canSee, invisible, qualifying, unrelated);
        }

        private static bool HasActiveBuff(UnitEntityData unit, string guid)
        {
            return ActiveBuffs(unit).Any(value => value.Blueprint != null &&
                string.Equals(value.Blueprint.AssetGuid, guid,
                    StringComparison.Ordinal));
        }

        private static bool HasComponent(UnitEntityData unit,
            string componentType)
        {
            return ActiveBuffs(unit).SelectMany(Components).Any(value =>
                value != null && string.Equals(value.GetType().FullName,
                    componentType, StringComparison.Ordinal));
        }

        private static Buff[] ActiveBuffs(UnitEntityData unit)
        {
            return unit == null || unit.Descriptor == null ||
                unit.Descriptor.Buffs == null ? new Buff[0] :
                unit.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(value =>
                    value != null && value.Active &&
                    value.Blueprint != null).ToArray();
        }

        private static BlueprintComponent[] Components(Buff buff)
        {
            return buff == null || buff.Blueprint == null ?
                new BlueprintComponent[0] : buff.Blueprint.ComponentsArray ??
                new BlueprintComponent[0];
        }
    }

    [HarmonyPatch(typeof(RuleConcealmentCheck), "get_Success")]
    internal static class ElementalFiresightConcealmentPatch
    {
        private static void Postfix(RuleConcealmentCheck __instance,
            ref bool __result)
        {
            try
            {
                if (ElementalFiresightRuntime.ShouldBypass(__instance,
                        __result))
                    __result = true;
            }
            catch (Exception)
            {
                // Fail closed to the exact native concealment result.
            }
        }
    }
}
