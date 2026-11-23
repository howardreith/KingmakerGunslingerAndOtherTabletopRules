using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KmgLoaderCompatHarness
{
    /// <summary>
    /// S6 C2a R1: a read-only, process-wide observer of the hold owners' lifecycle callbacks, installed at launch by the
    /// "observeHoldLifecycle" condition so that it survives a same-process load. It records, in order: the engine's
    /// Game.HandleAreaBeginUnloading and Game.OnAreaLoaded, each hold buff's OnAreaBeginUnloading / OnAreaDidLoad /
    /// OnTurnOn / OnTurnOff / OnFactDeactivate (with the owner's IsTurnedOn and the instance's own unload state seen at
    /// that moment), MaintainLink outcomes, Release/ReleaseLink calls and ResetLoadedGrapples. It never alters a call.
    /// </summary>
    internal static class HoldLifecycleObserver
    {
        public const string Condition = "observeHoldLifecycle";
        private const BindingFlags Any = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static bool requested, installed;
        internal static readonly List<JObject> Events = new List<JObject>();
        private static int sequence;
        private static Type holdType, multiType;

        public static void Request() => requested = true;
        internal static bool Installed => installed;

        public static JToken Install(HarmonyInstance harmony)
        {
            if (!requested || installed) return null;
            Assembly product = ProductProbes.Entry().Assembly;
            holdType = product.GetType("KingmakerGunslinger.Summoning.SummonHoldComponent", true);
            multiType = product.GetType("KingmakerGunslinger.Summoning.SummonMultiHoldComponent", true);
            var patched = new JArray();
            foreach (Type owner in new[] { holdType, multiType })
            {
                Patch(harmony, owner.GetMethod("OnAreaBeginUnloading"), null, nameof(BeginUnloadingPostfix), patched);
                Patch(harmony, owner.GetMethod("OnAreaDidLoad"), null, nameof(DidLoadPostfix), patched);
                Patch(harmony, owner.GetMethod("OnTurnOn"), null, nameof(TurnOnPostfix), patched);
                Patch(harmony, owner.GetMethod("OnTurnOff"), nameof(TurnOffPrefix), null, patched);
                Patch(harmony, owner.GetMethod("OnFactDeactivate"), nameof(DeactivatePrefix), null, patched);
            }
            Patch(harmony, holdType.GetMethods(Any).Single(m => m.Name == "MaintainLink" && m.GetParameters().Length == 7), null, nameof(MaintainPostfix), patched);
            Patch(harmony, holdType.GetMethod("Release", Any), null, nameof(ReleasePostfix), patched);
            Patch(harmony, holdType.GetMethod("ReleaseLink", Any), null, nameof(ReleaseLinkPostfix), patched);
            Patch(harmony, product.GetType("KingmakerGunslinger.Summoning.SummonGrappleAreaSafeguard", true).GetMethod("ResetLoadedGrapples", Any),
                nameof(ResetPrefix), null, patched);
            // C3a: the swallow lifecycle owner and the native swallow parts (read-only).
            Type lifecycle = product.GetType("KingmakerGunslinger.Summoning.SummonSwallowLifecycleComponent", true);
            Patch(harmony, lifecycle.GetMethod("OnTurnOff"), nameof(SwallowTurnOffPrefix), null, patched);
            Patch(harmony, lifecycle.GetMethod("HandleUnitDeath"), nameof(SwallowDeathPrefix), null, patched);
            Patch(harmony, typeof(UnitPartSwallowWhole).GetMethod("Swallow"), null, nameof(NativeSwallowPostfix), patched);
            Patch(harmony, typeof(UnitPartSwallowWhole).GetMethod("SpitOut"), nameof(NativeSpitOutPrefix), null, patched);
            Patch(harmony, typeof(UnitPartSwallowWhole).GetMethod("Free"), nameof(NativeFreePrefix), null, patched);
            Patch(harmony, typeof(UnitPartSwallowed).GetMethod("OnRemove"), nameof(NativeSwallowedRemovePrefix), null, patched);
            // G4: the retained Stirge attachment owner (read-only).
            stirgeHold = product.GetType("KingmakerGunslinger.Summoning.StirgeHoldComponent", true);
            Patch(harmony, product.GetType("KingmakerGunslinger.Summoning.StirgeAttachComponent", true).GetMethod("TryAttach", Any), null, nameof(StirgeTryAttachPostfix), patched);
            Patch(harmony, stirgeHold.GetMethod("OnNewRound"), nameof(StirgeNewRoundPrefix), nameof(StirgeNewRoundPostfix), patched);
            Patch(harmony, stirgeHold.GetMethod("OnTurnOff"), nameof(StirgeTurnOffPrefix), null, patched);
            Patch(harmony, stirgeHold.GetMethod("Detach", Any), nameof(StirgeDetachPrefix), nameof(StirgeDetachPostfix), patched);
            Patch(harmony, stirgeHold.GetMethod("ReleaseRemoveAction", Any), nameof(StirgeReleaseActionPrefix), nameof(StirgeReleaseActionPostfix), patched);
            Patch(harmony, product.GetType("KingmakerGunslinger.Summoning.ContextActionRemoveStirge", true).GetMethod("RunAction"), nameof(StirgeRemoveActionPrefix), nameof(StirgeRemoveActionPostfix), patched);
            Patch(harmony, product.GetType("KingmakerGunslinger.Summoning.SummonGrappleAreaSafeguard", true).GetMethod("SweepStirge", Any), null, nameof(StirgeSweepPostfix), patched);
            // G1/G2 (Barbed Devil Impale, Giant Crab): the deferred exact-owner cleanup, its scheduled callback, and the
            // per-link constrict rider (read-only).
            MethodInfo schedule = holdType.GetMethod("ScheduleReleasedImpaleOwnerCleanup", Any);
            Patch(harmony, schedule, nameof(ImpaleSchedulePrefix), null, patched);
            MethodInfo callback = holdType.GetNestedTypes(Any).SelectMany(t => t.GetMethods(Any))
                .Single(m => m.Name.Contains("ScheduleReleasedImpaleOwnerCleanup") && m.GetParameters().Length == 0);
            Patch(harmony, callback, nameof(ImpaleCallbackPrefix), nameof(ImpaleCallbackPostfix), patched);
            grabType = product.GetType("KingmakerGunslinger.Summoning.SummonGrabComponent", true);
            linksType = product.GetType("KingmakerGunslinger.Summoning.SummonGrappleLinks", true);
            limbsType = product.GetType("KingmakerGunslinger.Summoning.SummonLimbs", true);
            Patch(harmony, grabType.GetMethod("DealConstrict", Any), nameof(ConstrictPrefix), nameof(ConstrictPostfix), patched);
            Patch(harmony, multiType.GetMethod("OnNewRound"), nameof(MultiRoundPrefix), nameof(MultiRoundPostfix), patched);
            // G1-R diagnostics: writes to a watched unit's Damage, and the native life controller's tick on it (read-only).
            Patch(harmony, typeof(UnitDescriptor).GetProperty("Damage").GetSetMethod(true), nameof(DamageSetPrefix), null, patched);
            Patch(harmony, typeof(Kingmaker.Controllers.Units.UnitLifeController).GetMethod("TickOnUnit", Any), nameof(LifeTickPrefix), nameof(LifeTickPostfix), patched);
            Patch(harmony, product.GetType("KingmakerGunslinger.Summoning.SummonGrappleDamage", true).GetMethod("DealWeaponDamage", Any), nameof(WeaponDamagePrefix), nameof(WeaponDamagePostfix), patched);
            Patch(harmony, grabType.GetMethod("DealDeathRoll", Any), nameof(DeathRollPrefix), nameof(DeathRollPostfix), patched);
            Patch(harmony, grabType.GetMethod("DealImpale", Any), nameof(ImpalePrefix), nameof(ImpalePostfix), patched);
            Patch(harmony, product.GetType("KingmakerGunslinger.Summoning.SalamanderConstrictDamage", true).GetMethod("Deal", Any), nameof(SalamanderPrefix), nameof(SalamanderPostfix), patched);
            Patch(harmony, product.GetType("KingmakerGunslinger.Summoning.SummonHeldComponent", true).GetMethod("OnNewRound"), nameof(HeldRoundPrefix), nameof(HeldRoundPostfix), patched);
            Patch(harmony, typeof(Game).GetMethod("HandleAreaBeginUnloading", Any), nameof(GameUnloadPrefix), null, patched);
            Patch(harmony, typeof(Game).GetMethod("OnAreaLoaded", Any), nameof(GameLoadedPrefix), nameof(GameLoadedPostfix), patched);
            installed = true;
            return patched;
        }

        private static void Patch(HarmonyInstance harmony, MethodBase method, string prefix, string postfix, JArray patched)
        {
            if (method == null) throw new MissingMethodException("hold lifecycle observer target missing");
            harmony.Patch(method, prefix == null ? null : new HarmonyMethod(typeof(HoldLifecycleObserver).GetMethod(prefix, BindingFlags.Static | BindingFlags.NonPublic)),
                postfix == null ? null : new HarmonyMethod(typeof(HoldLifecycleObserver).GetMethod(postfix, BindingFlags.Static | BindingFlags.NonPublic)));
            patched.Add(method.DeclaringType.Name + "." + method.Name);
        }

        internal static void Add(string what, JObject detail)
        {
            if (Events.Count >= 6000) return;
            detail["seq"] = ++sequence;
            detail["what"] = what;
            detail["gameSeconds"] = Game.Instance == null || Game.Instance.TimeController == null ? 0 : Game.Instance.TimeController.GameTime.TotalSeconds;
            detail["loading"] = LoadingProcess.Instance != null && LoadingProcess.Instance.IsLoadingInProcess;
            detail["frame"] = Time.frameCount;
            Events.Add(detail);
        }

        private static UnitDescriptor OwnerOf(object logic) => logic.GetType().GetProperty("Owner", Any)?.GetValue(logic, null) as UnitDescriptor;

        internal static bool? AreaUnloadingOf(object logic)
        {
            FieldInfo field = logic.GetType().GetField("m_AreaUnloading", Any);
            return field == null ? (bool?)null : (bool)field.GetValue(logic);
        }

        private static JObject Logic(object logic)
        {
            UnitDescriptor owner = OwnerOf(logic);
            var row = new JObject
            {
                ["component"] = logic.GetType().Name,
                ["owner"] = owner?.Unit?.UniqueId, ["ownerName"] = owner?.CustomName,
                ["ownerTurnedOn"] = owner != null && owner.IsTurnedOn,
                ["instanceAreaUnloading"] = AreaUnloadingOf(logic),
                ["instance"] = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(logic)
            };
            if (owner != null && owner.Unit != null)
            {
                UnitPartGrappleInitiator initiator = owner.Unit.Get<UnitPartGrappleInitiator>();
                UnitEntityData target = initiator == null ? null : initiator.Target.Value;
                row["initiatorTarget"] = target?.UniqueId;
                row["targetPartNamesHolder"] = target != null && target.Get<UnitPartGrappleTarget>() != null &&
                    ReferenceEquals(target.Get<UnitPartGrappleTarget>().Initiator.Value, owner.Unit);
            }
            return row;
        }

        private static void BeginUnloadingPostfix(object __instance) { Add("hold.OnAreaBeginUnloading", Logic(__instance)); }
        private static void DidLoadPostfix(object __instance) { Add("hold.OnAreaDidLoad", Logic(__instance)); }
        private static void TurnOnPostfix(object __instance) { Add("hold.OnTurnOn", Logic(__instance)); }
        private static void TurnOffPrefix(object __instance) { Add("hold.OnTurnOff", Logic(__instance)); }
        private static void DeactivatePrefix(object __instance) { Add("hold.OnFactDeactivate", Logic(__instance)); }
        private static void MaintainPostfix(UnitEntityData owner, UnitEntityData target, string __result)
        {
            Add("MaintainLink", new JObject { ["owner"] = owner?.UniqueId, ["target"] = target?.UniqueId, ["result"] = __result,
                ["targetLifeState"] = target?.Descriptor?.State.LifeState.ToString(), ["targetDamage"] = target?.Descriptor?.Damage });
        }

        // ---- G1-R: brackets around each rider damage owner, with the target's life state (read-only)
        private static JObject Rider(UnitEntityData owner, UnitEntityData target) => new JObject
        {
            ["owner"] = owner?.UniqueId, ["target"] = target?.UniqueId,
            ["targetLifeState"] = target?.Descriptor?.State.LifeState.ToString(), ["targetDamage"] = target?.Descriptor?.Damage,
            ["targetHP"] = target?.Descriptor == null ? (int?)null : (int)target.Descriptor.Stats.HitPoints
        };
        private static void WeaponDamagePrefix(UnitEntityData owner, UnitEntityData target) { Add("rider.weapon.begin", Rider(owner, target)); }
        private static void WeaponDamagePostfix(UnitEntityData owner, UnitEntityData target, int __result) { JObject r = Rider(owner, target); r["dealt"] = __result; Add("rider.weapon.end", r); }
        private static void DeathRollPrefix(UnitEntityData owner, UnitEntityData target) { Add("rider.deathRoll.begin", Rider(owner, target)); }
        private static void DeathRollPostfix(UnitEntityData owner, UnitEntityData target, string __result) { JObject r = Rider(owner, target); r["result"] = __result; Add("rider.deathRoll.end", r); }
        private static void ImpalePrefix(UnitEntityData owner, UnitEntityData target) { Add("rider.impale.begin", Rider(owner, target)); }
        private static void ImpalePostfix(UnitEntityData owner, UnitEntityData target, string __result) { JObject r = Rider(owner, target); r["result"] = __result; Add("rider.impale.end", r); }
        private static void SalamanderPrefix(UnitEntityData owner, UnitEntityData target) { Add("rider.salamanderConstrict.begin", Rider(owner, target)); }
        private static void SalamanderPostfix(UnitEntityData owner, UnitEntityData target) { Add("rider.salamanderConstrict.end", Rider(owner, target)); }
        private static void ReleasePostfix(UnitEntityData owner, UnitEntityData target)
        { Add("SummonHoldComponent.Release", Pair(owner, target)); }
        private static void ReleaseLinkPostfix(UnitEntityData owner, UnitEntityData target)
        { Add("SummonHoldComponent.ReleaseLink", Pair(owner, target)); }

        /// <summary>The native single-link parts of a holder/target pair, read at the moment of the call (G1/G2).</summary>
        internal static JObject Pair(UnitEntityData owner, UnitEntityData target)
        {
            UnitPartGrappleInitiator initiator = owner == null || owner.Descriptor == null ? null : owner.Get<UnitPartGrappleInitiator>();
            UnitPartGrappleTarget held = target == null || target.Descriptor == null ? null : target.Get<UnitPartGrappleTarget>();
            return new JObject
            {
                ["owner"] = owner?.UniqueId, ["target"] = target?.UniqueId,
                ["ownerInitiatorPart"] = initiator != null, ["ownerInitiatorTarget"] = initiator?.Target.Value?.UniqueId,
                ["ownerHoldBuffs"] = owner == null || owner.Descriptor == null ? 0 : owner.Descriptor.Buffs.RawFacts.OfType<Buff>().Count(b => b.Blueprint != null &&
                    b.Blueprint.ComponentsArray != null && b.Blueprint.ComponentsArray.Any(c => c.GetType() == holdType || c.GetType() == multiType)),
                ["targetPart"] = held != null, ["targetPartInitiator"] = held?.Initiator.Value?.UniqueId,
                ["distance"] = owner == null || target == null ? (double?)null : Vector3.Distance(owner.Position, target.Position),
                ["targetLifeState"] = target?.Descriptor?.State.LifeState.ToString(),
                ["targetConscious"] = target != null && target.Descriptor != null && target.Descriptor.State.IsConscious,
                ["reachPrimary"] = owner != null && target != null && owner.Body != null && owner.Body.PrimaryHand != null &&
                    Kingmaker.Controllers.Combat.UnitEngagementExtension.IsReach(owner, target, owner.Body.PrimaryHand)
            };
        }

        internal static readonly HashSet<UnitDescriptor> WatchedDamage = new HashSet<UnitDescriptor>();
        private static void DamageSetPrefix(UnitDescriptor __instance, int value)
        {
            if (__instance == null || !WatchedDamage.Contains(__instance)) return;
            int current = __instance.Damage;
            if (value == current) return;
            var row = new JObject { ["unit"] = __instance.Unit?.UniqueId, ["from"] = current, ["to"] = value, ["lifeState"] = __instance.State.LifeState.ToString() };
            if (value < current)
            {
                string stack = new System.Diagnostics.StackTrace(1, false).ToString();
                row["stack"] = stack.Length > 1800 ? stack.Substring(0, 1800) : stack;
            }
            Add("diag.damageSet", row);
        }
        private static void LifeTickPrefix(UnitEntityData unit, out string __state)
        {
            __state = unit?.Descriptor?.State.LifeState.ToString();
            if (unit?.Descriptor == null || !WatchedDamage.Contains(unit.Descriptor)) return;
            if (unit.Descriptor.Damage < (int)unit.Descriptor.Stats.HitPoints) return;
            Add("diag.lifeTick.begin", new JObject { ["unit"] = unit.UniqueId, ["damage"] = unit.Descriptor.Damage, ["hp"] = (int)unit.Descriptor.Stats.HitPoints,
                ["con"] = (int)unit.Descriptor.Stats.Constitution, ["lifeState"] = __state, ["allowDying"] = (bool)unit.Descriptor.State.AllowDyingCondition,
                ["regenerate"] = (bool)unit.Descriptor.State.IsRegenerate, ["markedForDeath"] = unit.Descriptor.State.MarkedForDeath });
        }
        private static void LifeTickPostfix(UnitEntityData unit, string __state)
        {
            if (unit?.Descriptor == null || !WatchedDamage.Contains(unit.Descriptor)) return;
            string now = unit.Descriptor.State.LifeState.ToString();
            if (now == __state && unit.Descriptor.Damage < (int)unit.Descriptor.Stats.HitPoints) return;
            Add("diag.lifeTick.end", new JObject { ["unit"] = unit.UniqueId, ["damage"] = unit.Descriptor.Damage, ["lifeState"] = now, ["was"] = __state });
        }

        private static void MultiRoundPrefix(object __instance) { Add("multiHold.OnNewRound.begin", new JObject { ["owner"] = OwnerOf(__instance)?.Unit?.UniqueId }); }
        private static void MultiRoundPostfix(object __instance) { Add("multiHold.OnNewRound.end", new JObject { ["owner"] = OwnerOf(__instance)?.Unit?.UniqueId }); }
        private static void HeldRoundPrefix(object __instance) { Add("held.OnNewRound.begin", new JObject { ["unit"] = OwnerOf(__instance)?.Unit?.UniqueId }); }
        private static void HeldRoundPostfix(object __instance) { Add("held.OnNewRound.end", new JObject { ["unit"] = OwnerOf(__instance)?.Unit?.UniqueId }); }

        // ---- G1/G2 Barbed Devil Impale cleanup and Giant Crab constrict (read-only)
        private static Type grabType, linksType, limbsType;

        private static void ImpaleSchedulePrefix(UnitEntityData owner, UnitEntityData target) { Add("impaleCleanup.schedule", Pair(owner, target)); }

        private static UnitEntityData ClosureUnit(object closure, string field) => closure.GetType().GetField(field, Any)?.GetValue(closure) as UnitEntityData;
        private static void ImpaleCallbackPrefix(object __instance) { Add("impaleCleanup.callback.begin", Pair(ClosureUnit(__instance, "owner"), ClosureUnit(__instance, "target"))); }
        private static void ImpaleCallbackPostfix(object __instance) { Add("impaleCleanup.callback.end", Pair(ClosureUnit(__instance, "owner"), ClosureUnit(__instance, "target"))); }

        internal static string LimbOf(UnitEntityData owner, ItemEntityWeapon weapon)
        {
            if (owner == null || weapon == null) return null;
            var args = new object[] { owner, weapon, -1 };
            object kind = limbsType.GetMethod("Classify", Any).Invoke(null, args);
            return (int)kind + "|" + (int)args[2];
        }

        private static void ConstrictPrefix(UnitEntityData owner, UnitEntityData target, out int __state)
        {
            __state = target == null || target.Descriptor == null ? -1 : target.Descriptor.Damage;
            Add("DealConstrict.begin", Rider(owner, target));
        }

        private static void ConstrictPostfix(object __instance, UnitEntityData owner, UnitEntityData target, int __state)
        {
            var weapon = linksType.GetMethod("EstablishingWeapon", Any).Invoke(null, new object[] { owner, target }) as ItemEntityWeapon;
            int after = target == null || target.Descriptor == null ? -1 : target.Descriptor.Damage;
            Add("DealConstrict", new JObject { ["owner"] = owner?.UniqueId, ["target"] = target?.UniqueId, ["limb"] = LimbOf(owner, weapon),
                ["constrictDice"] = __instance == null ? 0 : (int)grabType.GetField("ConstrictDiceCount").GetValue(__instance),
                ["weapon"] = weapon?.Blueprint?.name, ["damageBefore"] = __state, ["damageAfter"] = after, ["delta"] = after - __state,
                ["targetDead"] = target != null && target.Descriptor != null && target.Descriptor.State.IsDead,
                ["targetLifeState"] = target?.Descriptor?.State.LifeState.ToString() });
        }
        private static void ResetPrefix() { Add("ResetLoadedGrapples", new JObject()); }

        private static JArray Swallowed(UnitEntityData owner)
        {
            UnitPartSwallowWhole part = owner == null ? null : owner.Get<UnitPartSwallowWhole>();
            return part == null ? new JArray() : new JArray(part.SwallowedUnits.Select(r => (object)r.Value?.UniqueId).ToArray());
        }

        private static void SwallowTurnOffPrefix(object __instance)
        {
            UnitDescriptor owner = OwnerOf(__instance);
            Add("swallow.OnTurnOff", new JObject { ["owner"] = owner?.Unit?.UniqueId, ["ownerTurnedOn"] = owner != null && owner.IsTurnedOn,
                ["instanceAreaUnloading"] = AreaUnloadingOf(__instance), ["swallowed"] = Swallowed(owner?.Unit) });
        }
        private static void SwallowDeathPrefix(object __instance, UnitEntityData unit)
        {
            UnitDescriptor owner = OwnerOf(__instance);
            if (unit == null || owner?.Unit == null || !Swallowed(owner.Unit).Any(v => (string)v == unit.UniqueId)) return;
            Add("swallow.HandleUnitDeath", new JObject { ["owner"] = owner.Unit.UniqueId, ["unit"] = unit.UniqueId, ["swallowed"] = Swallowed(owner.Unit) });
        }
        private static void NativeSwallowPostfix(UnitPartSwallowWhole __instance, UnitEntityData unit)
        { Add("native.Swallow", new JObject { ["owner"] = __instance.Owner?.Unit?.UniqueId, ["unit"] = unit?.UniqueId }); }
        private static void NativeSpitOutPrefix(UnitPartSwallowWhole __instance, bool all)
        { Add("native.SpitOut", new JObject { ["owner"] = __instance.Owner?.Unit?.UniqueId, ["all"] = all, ["swallowed"] = Swallowed(__instance.Owner?.Unit) }); }
        private static void NativeFreePrefix(UnitPartSwallowWhole __instance, UnitReference unitRef)
        { Add("native.Free", new JObject { ["owner"] = __instance.Owner?.Unit?.UniqueId, ["unit"] = unitRef.Value?.UniqueId }); }
        private static void NativeSwallowedRemovePrefix(UnitPartSwallowed __instance)
        { Add("native.UnitPartSwallowed.OnRemove", new JObject { ["unit"] = __instance.Owner?.Unit?.UniqueId, ["swallower"] = __instance.Swallower.Value?.UniqueId }); }
        private static void GameUnloadPrefix() { Add("Game.HandleAreaBeginUnloading", new JObject()); }
        private static void GameLoadedPrefix() { Add("Game.OnAreaLoaded.begin", new JObject()); }
        private static void GameLoadedPostfix() { Add("Game.OnAreaLoaded.end", new JObject()); }

        // ---- G4 Stirge observation (read-only)
        private static Type stirgeHold;

        private static UnitEntityData StirgeTargetOf(object logic) => stirgeHold.GetField("m_Target", Any).GetValue(logic) as UnitEntityData;
        private static int StirgeCumulativeOf(object logic) => (int)stirgeHold.GetField("m_CumulativeDamage", Any).GetValue(logic);
        internal static UnitEntityData StirgeAttachedTarget(UnitEntityData owner) => stirgeHold.GetMethod("AttachedTarget", Any).Invoke(null, new object[] { owner }) as UnitEntityData;
        private static bool HasRemove(UnitEntityData unit)
        {
            var remove = stirgeHold.GetField("RemoveAbility", Any).GetValue(null) as Kingmaker.Blueprints.Facts.BlueprintUnitFact;
            return unit != null && unit.Descriptor != null && remove != null && unit.Descriptor.HasFact(remove);
        }
        private static JObject StirgeLogic(object logic)
        {
            UnitDescriptor owner = OwnerOf(logic);
            UnitEntityData target = StirgeTargetOf(logic);
            return new JObject { ["owner"] = owner?.Unit?.UniqueId, ["ownerName"] = owner?.CustomName, ["ownerTurnedOn"] = owner != null && owner.IsTurnedOn,
                ["ownerDead"] = owner != null && owner.State.IsDead, ["target"] = target?.UniqueId, ["targetDead"] = target != null && target.Descriptor.State.IsDead,
                ["cumulative"] = StirgeCumulativeOf(logic), ["targetConDamage"] = target == null ? (int?)null : target.Descriptor.Stats.Constitution.Damage };
        }
        private static void StirgeTryAttachPostfix(object __instance, UnitEntityData target, bool touchHit, bool __result)
        { Add("stirge.TryAttach", new JObject { ["owner"] = OwnerOf(__instance)?.Unit?.UniqueId, ["target"] = target?.UniqueId, ["touchHit"] = touchHit, ["attached"] = __result }); }
        private static void StirgeNewRoundPrefix(object __instance) { Add("stirge.OnNewRound.begin", StirgeLogic(__instance)); }
        private static void StirgeNewRoundPostfix(object __instance) { Add("stirge.OnNewRound.end", StirgeLogic(__instance)); }
        private static void StirgeTurnOffPrefix(object __instance) { Add("stirge.OnTurnOff", StirgeLogic(__instance)); }
        private static void StirgeDetachPrefix(UnitEntityData owner)
        { Add("stirge.Detach.begin", new JObject { ["owner"] = owner?.UniqueId, ["attachedTarget"] = StirgeAttachedTarget(owner)?.UniqueId }); }
        private static void StirgeDetachPostfix(UnitEntityData owner, bool __result)
        { Add("stirge.Detach.end", new JObject { ["owner"] = owner?.UniqueId, ["removedHold"] = __result }); }
        private static void StirgeReleaseActionPrefix(UnitEntityData target)
        { Add("stirge.ReleaseRemoveAction.begin", new JObject { ["target"] = target?.UniqueId, ["hadAction"] = HasRemove(target) }); }
        private static void StirgeReleaseActionPostfix(UnitEntityData target)
        { Add("stirge.ReleaseRemoveAction.end", new JObject { ["target"] = target?.UniqueId, ["hasAction"] = HasRemove(target) }); }
        private static UnitEntityData ActionUnit(object action, string property)
        {
            object value = action.GetType().GetProperty(property, Any)?.GetValue(action, null);
            if (value is Kingmaker.UnitLogic.Mechanics.MechanicsContext context) return context.MaybeCaster;
            if (value is Kingmaker.Utility.TargetWrapper wrapper) return wrapper.Unit;
            return null;
        }
        private static void StirgeRemoveActionPrefix(object __instance)
        {
            UnitEntityData stirge = ActionUnit(__instance, "Target");
            Add("stirge.RemoveAction.begin", new JObject { ["prey"] = ActionUnit(__instance, "Context")?.UniqueId, ["stirge"] = stirge?.UniqueId, ["attachedTarget"] = StirgeAttachedTarget(stirge)?.UniqueId });
        }
        private static void StirgeRemoveActionPostfix(object __instance)
        {
            UnitEntityData stirge = ActionUnit(__instance, "Target");
            Add("stirge.RemoveAction.end", new JObject { ["prey"] = ActionUnit(__instance, "Context")?.UniqueId, ["stirge"] = stirge?.UniqueId, ["attachedTarget"] = StirgeAttachedTarget(stirge)?.UniqueId });
        }
        private static void StirgeSweepPostfix(int __result) { Add("stirge.SweepStirge", new JObject { ["released"] = __result }); }
    }

    /// <summary>
    /// S6 C2a R1 probe. Modes:
    /// "removal" - genuine removal of an active carrier through the native fact removal API, per changed owner (G1 hold
    ///   buff with the holder alive; G2 multi-hold buff on a two-link holder), with an independent holding pair and an
    ///   unrelated effect as controls; plus a G1 accelerated-expiry case for disposal attribution.
    /// "cadence" - an aged G1 hold and an aged G2 two-link hold through two paused native saves (exact records,
    ///   ownership, native Buff.RoundNumber and next tick time, unchanged game time, unrelated effect kept), then the next
    ///   scheduled round; both left holding, named, for the harness save and the same-process load that follow.
    /// "afterLoad" - after the harness's same-process loadLastSave: the observed unload/arrival ordering and the
    ///   disposition of the named relationships, the loaded reset, then fresh G1 and G2 holds through a native save.
    /// Disclosed setup as in C1/C2: BAB 100, target HP raised, appear buffs removed, brains off, units woken.
    /// </summary>
    internal sealed class HoldLifecycleProbe : IOwnedProbe
    {
        private const BindingFlags Any = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const string UnrelatedBuffGuid = "b175001b42b1a02479881b72fe132116";
        private static readonly FieldInfo CasterReference = typeof(Kingmaker.UnitLogic.Mechanics.MechanicsContext).GetField("m_CasterReference", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly PropertyInfo NextTickTime = typeof(Buff).GetProperty("NextTickTime", Any);
        private static readonly PropertyInfo TickTime = typeof(Buff).GetProperty("TickTime", Any);
        internal static readonly List<string> NamedHolders = new List<string>();
        internal static int SaveEventSeq;

        private readonly string mode;
        private readonly JObject result = new JObject();
        private readonly List<string> problems = new List<string>();
        private readonly IEnumerator<object> script;
        private readonly List<UnitEntityData> created = new List<UnitEntityData>();
        private readonly List<UnitEntityData> awakeAdded = new List<UnitEntityData>();
        private string verdict;
        private Assembly product;
        private Type runner, linkPart, links, grabType, holdType, limbs;

        public HoldLifecycleProbe(string mode) { this.mode = mode; result["mode"] = mode; script = Run().GetEnumerator(); }
        public object Result => result;

        public string Poll()
        {
            if (verdict != null) return verdict;
            bool more;
            try { more = script.MoveNext(); }
            catch (Exception exception) { problems.Add("probe exception: " + exception); more = false; }
            if (more) return null;
            result["problems"] = new JArray(problems.Cast<object>().ToArray());
            verdict = problems.Count == 0 ? string.Empty : string.Join(" | ", problems.ToArray());
            return verdict;
        }

        public CleanupReport Release(string testFailure)
        {
            return ProbeCleanup.RunAll(new[]
            {
                new KeyValuePair<string, Action>("awake registrations", () => { foreach (UnitEntityData u in awakeAdded) Game.Instance.State.AwakeUnits.Remove(u); awakeAdded.Clear(); }),
                new KeyValuePair<string, Action>("disposable units", () => { foreach (UnitEntityData u in created.Where(u => u != null && !u.Destroyed).ToArray()) u.Destroy(); created.Clear(); })
            });
        }

        private IEnumerable<object> Run()
        {
            product = ProductProbes.Entry().Assembly;
            runner = product.GetType("KingmakerGunslinger.RuntimeTesting.RuntimeTestRunner", true);
            linkPart = product.GetType("KingmakerGunslinger.Summoning.UnitPartSummonGrappleLinks", true);
            links = product.GetType("KingmakerGunslinger.Summoning.SummonGrappleLinks", true);
            grabType = product.GetType("KingmakerGunslinger.Summoning.SummonGrabComponent", true);
            holdType = product.GetType("KingmakerGunslinger.Summoning.SummonHoldComponent", true);
            limbs = product.GetType("KingmakerGunslinger.Summoning.SummonLimbs", true);
            if (!HoldLifecycleObserver.Installed) { problems.Add("the hold lifecycle observer is not installed (launch condition observeHoldLifecycle)"); yield break; }
            bool paused = Game.Instance.IsPaused;
            try
            {
                IEnumerable<object> body = mode == "removal" ? Removal() : mode == "cadence" ? Cadence() : mode == "afterLoad" ? AfterLoad() : null;
                if (body == null) { problems.Add("unknown mode " + mode); yield break; }
                foreach (object step in body) yield return step;
            }
            finally { Game.Instance.IsPaused = paused; }
        }

        // ---------------------------------------------------------------- fixture

        private sealed class Hold
        {
            internal UnitEntityData Holder;
            internal UnitEntityData[] Targets;
            internal ItemEntityWeapon[] Weapons;
            internal object Grab;
            internal BlueprintBuff HoldBuff, HeldBuff;
            internal bool Multi;
        }

        private UnitEntityData Summon(string key)
        {
            UnitEntityData main = Game.Instance.Player.MainCharacter.Value;
            Type multiplicity = product.GetType("KingmakerGunslinger.Summoning.SummonMultiplicity", true);
            object variant = runner.GetMethod("ExpandedSummoningOwnTierVariant", Any).Invoke(null, new[] { key, Enum.Parse(multiplicity, "One") });
            object evidence = Activator.CreateInstance(runner.GetNestedType("ExpandedSummoningMechanicalEvidence", Any), true);
            var library = (LibraryScriptableObject)product.GetType("KingmakerGunslinger.Bootstrap.BlueprintBootstrap", true).GetProperty("Library", Any).GetValue(null, null);
            var units = (UnitEntityData[])runner.GetMethods(Any).Single(m => m.Name == "CastExpandedSummoningVariant" && m.GetParameters().Length == 5)
                .Invoke(null, new object[] { library.GetAllBlueprints().ToArray(), main, variant, null, evidence });
            UnitEntityData unit = units.Length == 1 ? units[0] : null;
            if (unit == null) return null;
            created.Add(unit);
            runner.GetMethod("RemoveExpandedSummoningAppearanceBuffs", Any).Invoke(null, new object[] { unit });
            runner.GetMethod("SetExpandedSummoningBrainActive", Any).Invoke(null, new object[] { unit, false });
            Wake(unit);
            return unit;
        }

        private void Wake(UnitEntityData unit)
        {
            unit.Wake(600f);
            if (!Game.Instance.State.AwakeUnits.Contains(unit)) { Game.Instance.State.AwakeUnits.Add(unit); awakeAdded.Add(unit); }
        }

        private void Place(UnitEntityData unit, Vector3 position) { runner.GetMethod("PlaceExpandedSummoningUnit", Any).Invoke(null, new object[] { unit, position }); }

        private ItemEntityWeapon[] GrabWeapons(UnitEntityData holder, object grab)
        {
            var result = new List<ItemEntityWeapon>();
            MethodInfo isGrabLimb = grabType.GetMethod("IsGrabLimb", Any);
            var primary = (ItemEntityWeapon)limbs.GetMethod("PrimaryWeapon", Any).Invoke(null, new object[] { holder });
            if (primary != null && (bool)isGrabLimb.Invoke(grab, new object[] { holder, primary })) result.Add(primary);
            if (holder.Body.AdditionalLimbs != null)
                foreach (var slot in holder.Body.AdditionalLimbs)
                    if (slot != null && slot.MaybeWeapon != null && (bool)isGrabLimb.Invoke(grab, new object[] { holder, slot.MaybeWeapon })) result.Add(slot.MaybeWeapon);
            return result.ToArray();
        }

        /// <summary>G1: a crocodile holding a spawned target. G2: a Giant Flytrap holding two Large horses on distinct limbs.</summary>
        private IEnumerable<object> Establish(string name, bool multi, Vector3? spot, Action<Hold> done)
        {
            UnitEntityData holder = Summon(multi ? "giant-flytrap" : "crocodile");
            if (holder == null) { problems.Add(name + ": holder not summoned"); yield break; }
            if (spot.HasValue) Place(holder, spot.Value);
            holder.Descriptor.Stats.BaseAttackBonus.BaseValue = 100;
            var targets = new List<UnitEntityData>();
            if (!multi)
            {
                UnitEntityData target = Game.Instance.EntityCreator.SpawnUnit(BlueprintRoot.Instance.DefaultPlayerCharacter,
                    holder.Position + holder.OrientationDirection * 1.0f, Quaternion.identity, Game.Instance.State.LoadedAreaState.MainState);
                if (target == null) { problems.Add(name + ": target not spawned"); yield break; }
                created.Add(target);
                Game.Instance.EntityCreator.Tick();
                target.Descriptor.Stats.HitPoints.BaseValue = 100000;
                runner.GetMethod("SetExpandedSummoningBrainActive", Any).Invoke(null, new object[] { target, false });
                Wake(target);
                targets.Add(target);
            }
            else
                for (int index = 0; index < 2; index++)
                {
                    UnitEntityData horse = Summon("horse");
                    if (horse == null) { problems.Add(name + ": horse not summoned"); yield break; }
                    horse.Descriptor.Stats.HitPoints.BaseValue = 100000;
                    Place(horse, ObstacleAnalyzer.TraceAlongNavmesh(holder.Position, holder.Position + Quaternion.Euler(0, holder.Orientation + (index == 0 ? -45f : 45f), 0) * Vector3.forward * 3.0f));
                    targets.Add(horse);
                }
            yield return null;
            object grab = grabType.GetMethod("Find", Any).Invoke(null, new object[] { holder });
            var h = new Hold
            {
                Holder = holder, Targets = targets.ToArray(), Grab = grab, Multi = multi,
                HoldBuff = (BlueprintBuff)grabType.GetField("HoldBuff").GetValue(grab),
                HeldBuff = (BlueprintBuff)grabType.GetField("GrappledBuff").GetValue(grab),
                Weapons = GrabWeapons(holder, grab)
            };
            for (int index = 0; index < h.Targets.Length; index++)
                for (int attack = 0; attack < 6 && !IsHeld(h, index); attack++)
                {
                    Rulebook.Trigger(new RuleAttackWithWeapon(holder, h.Targets[index], h.Weapons[index], 0));
                    yield return null;
                }
            result["established." + name] = State(h);
            for (int index = 0; index < h.Targets.Length; index++)
                if (!IsHeld(h, index)) { problems.Add(name + ": link " + index + " was not established: " + result["established." + name].ToString(Formatting.None)); yield break; }
            done(h);
        }

        private Buff AddUnrelated(UnitEntityData unit)
        {
            var blueprint = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>(UnrelatedBuffGuid);
            return blueprint == null ? null : unit.Descriptor.Buffs.AddBuff(blueprint, unit, TimeSpan.FromMinutes(30));
        }

        private static IEnumerable<object> SettleGameTime(double gameSeconds, float wallBudget, Func<bool> done)
        {
            TimeSpan start = Game.Instance.TimeController.GameTime;
            float until = Time.realtimeSinceStartup + wallBudget;
            while (Time.realtimeSinceStartup < until)
            {
                if (Game.Instance.IsPaused) Game.Instance.IsPaused = false;
                if ((Game.Instance.TimeController.GameTime - start).TotalSeconds >= gameSeconds && done()) break;
                yield return null;
            }
            Game.Instance.IsPaused = true;
        }

        private IEnumerable<object> NativeSave(string label, Action<bool> done)
        {
            bool saved = false;
            Game.Instance.SaveGame(Game.Instance.SaveManager.CreateNewSave(label), () => saved = true);
            float until = Time.realtimeSinceStartup + 120f;
            while ((!saved || Game.Instance.SaveManager.CommitInProgress || LoadingProcess.Instance.IsLoadingInProcess) && Time.realtimeSinceStartup < until)
                yield return null;
            done(saved);
        }

        // ---------------------------------------------------------------- observation

        private bool IsHeld(Hold h, int index)
        {
            UnitEntityData t = h.Targets[index];
            if (!h.Multi)
            {
                UnitPartGrappleInitiator initiator = h.Holder.Get<UnitPartGrappleInitiator>();
                UnitPartGrappleTarget held = t.Get<UnitPartGrappleTarget>();
                return initiator != null && held != null && ReferenceEquals(initiator.Target.Value, t) && ReferenceEquals(held.Initiator.Value, h.Holder);
            }
            return t.Descriptor.Buffs.RawFacts.OfType<Buff>().Any(b => ReferenceEquals(b.Blueprint, h.HeldBuff) && b.Context != null && ReferenceEquals(b.Context.MaybeCaster, h.Holder));
        }

        private IList<string> RawLinks(UnitEntityData holder)
        {
            object part = typeof(UnitDescriptor).GetMethods().Single(m => m.Name == "Get" && m.IsGenericMethodDefinition && m.GetParameters().Length == 0)
                .MakeGenericMethod(linkPart).Invoke(holder.Descriptor, null);
            return part == null ? null : (IList<string>)linkPart.GetField("_links", Any).GetValue(part);
        }

        private static JObject BuffRound(Buff buff)
        {
            if (buff == null) return null;
            return new JObject
            {
                ["blueprint"] = buff.Blueprint.name, ["instance"] = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(buff),
                ["roundNumber"] = buff.RoundNumber,
                ["nextTickTicks"] = ((TimeSpan)NextTickTime.GetValue(buff, null)).Ticks,
                ["tickTimeTicks"] = ((TimeSpan)TickTime.GetValue(buff, null)).Ticks,
                ["active"] = buff.Active, ["turnedOn"] = buff.IsTurnedOn,
                ["casterId"] = buff.Context == null ? null : buff.Context.MaybeCaster?.UniqueId,
                ["savedCasterId"] = buff.Context == null ? null : (CasterReference.GetValue(buff.Context) as UnitReference?)?.UniqueId
            };
        }

        private Buff HoldBuffOf(Hold h) => h.Holder.Descriptor.Buffs.RawFacts.OfType<Buff>().FirstOrDefault(b => ReferenceEquals(b.Blueprint, h.HoldBuff));

        private object HoldLogicOf(Hold h)
        {
            Buff buff = HoldBuffOf(h);
            return buff?.Components.FirstOrDefault(c => c.GetType().Name == (h.Multi ? "SummonMultiHoldComponent" : "SummonHoldComponent"));
        }

        private JObject State(Hold h)
        {
            UnitEntityData holder = h.Holder;
            IList<string> raw = holder.Destroyed ? null : RawLinks(holder);
            object logic = holder.Destroyed ? null : HoldLogicOf(h);
            var reconciled = holder.Destroyed ? new object[0] : ((IEnumerable)links.GetMethod("Reconcile", Any).Invoke(null, new object[] { holder })).Cast<object>().ToArray();
            MethodInfo occupant = links.GetMethod("OccupantOf", Any);
            var state = new JObject
            {
                ["holder"] = holder.UniqueId, ["holderName"] = holder.Descriptor.CustomName, ["holderDead"] = holder.Descriptor.State.IsDead, ["holderDestroyed"] = holder.Destroyed,
                ["raw.links"] = raw == null ? null : new JArray(raw.Cast<object>().ToArray()),
                ["live.reconciled"] = reconciled.Length,
                ["holdBuff"] = BuffRound(holder.Destroyed ? null : HoldBuffOf(h)),
                ["holdInstanceAreaUnloading"] = logic == null ? null : (JToken)HoldLifecycleObserver.AreaUnloadingOf(logic),
                ["initiatorPart"] = holder.Get<UnitPartGrappleInitiator>() != null,
                ["holderCantAct"] = holder.Descriptor.State.HasCondition(UnitCondition.CantAct),
                ["holderCantMove"] = holder.Descriptor.State.HasCondition(UnitCondition.CantMove),
                ["occupancy"] = holder.Destroyed ? null : new JArray(h.Weapons.Take(h.Targets.Length).Select(w => (object)((occupant.Invoke(null, new object[] { holder, w }) as UnitEntityData)?.UniqueId)).ToArray()),
                ["gameSeconds"] = Game.Instance.TimeController.GameTime.TotalSeconds
            };
            var targets = new JArray();
            foreach (UnitEntityData t in h.Targets)
            {
                UnitState st = t.Descriptor.State;
                targets.Add(new JObject
                {
                    ["id"] = t.UniqueId, ["dead"] = st.IsDead,
                    ["targetPart"] = t.Get<UnitPartGrappleTarget>() != null,
                    ["heldBuffs"] = new JArray(t.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(b => ReferenceEquals(b.Blueprint, h.HeldBuff)).Select(b => (object)BuffRound(b)).ToArray()),
                    ["entangled"] = st.HasCondition(UnitCondition.Entangled), ["cantMove"] = st.HasCondition(UnitCondition.CantMove), ["cantAct"] = st.HasCondition(UnitCondition.CantAct),
                    ["damage"] = t.Descriptor.Damage,
                    ["buffs"] = new JArray(t.Descriptor.Buffs.RawFacts.OfType<Buff>().Select(b => (object)b.Blueprint.name).ToArray())
                });
            }
            state["targets"] = targets;
            return state;
        }

        private void ExpectHeld(Hold h, string name)
        {
            JObject s = State(h);
            for (int index = 0; index < h.Targets.Length; index++)
            {
                JObject t = (JObject)((JArray)s["targets"])[index];
                string id = h.Targets[index].UniqueId;
                if (!IsHeld(h, index)) problems.Add(name + ": link " + index + " is not held");
                if (((JArray)s["raw.links"] ?? new JArray()).Count(l => ((string)l).StartsWith(id + "|", StringComparison.Ordinal)) != 1) problems.Add(name + ": link " + index + " raw record is not exactly one");
                if ((string)((JArray)s["occupancy"])[index] != id) problems.Add(name + ": link " + index + " limb is not occupied by its target");
                if (((JArray)t["heldBuffs"]).Count != 1) problems.Add(name + ": link " + index + " target does not have exactly one held buff");
                if (!(bool)t["cantMove"]) problems.Add(name + ": link " + index + " target lacks CantMove");
                if (h.Multi && !(bool)t["entangled"]) problems.Add(name + ": link " + index + " target lacks Entangled");
            }
            if (s["holdBuff"].Type == JTokenType.Null) problems.Add(name + ": the holder has no hold buff");
        }

        private void ExpectEnded(Hold h, string name)
        {
            JObject s = State(h);
            for (int index = 0; index < h.Targets.Length; index++)
            {
                JObject t = (JObject)((JArray)s["targets"])[index];
                if (IsHeld(h, index) || (bool)t["targetPart"] || ((JArray)t["heldBuffs"]).Count != 0 || (bool)t["entangled"] || (bool)t["cantMove"] || (bool)t["cantAct"])
                    problems.Add(name + ": link " + index + " target is still constrained: " + t.ToString(Formatting.None));
            }
            if ((int)s["live.reconciled"] != 0) problems.Add(name + ": live links remain");
        }

        private JArray EventsSince(int seq, Func<JObject, bool> filter = null)
        {
            return new JArray(HoldLifecycleObserver.Events.Where(e => (int)e["seq"] > seq && (filter == null || filter(e))).Cast<object>().ToArray());
        }

        private static int LastSeq => HoldLifecycleObserver.Events.Count == 0 ? 0 : (int)HoldLifecycleObserver.Events.Last()["seq"];

        // ---------------------------------------------------------------- removal

        private IEnumerable<object> Removal()
        {
            var cases = new JObject();
            result["cases"] = cases;
            Hold control = null;
            foreach (object step in Establish("control", false, null, x => control = x)) yield return step;
            if (control == null) yield break;
            Buff controlUnrelated = AddUnrelated(control.Targets[0]);

            // G1: remove the holder's actual hold-buff instance through the native fact removal API, holder alive.
            Hold g1 = null;
            foreach (object step in Establish("g1Removal", false, null, x => g1 = x)) yield return step;
            if (g1 == null) yield break;
            Buff unrelated1 = AddUnrelated(g1.Targets[0]);
            Game.Instance.IsPaused = true;
            yield return null;
            var row = new JObject { ["before"] = State(g1) };
            cases["g1HoldBuffRemoval"] = row;
            int seq = LastSeq;
            Buff hold = HoldBuffOf(g1);
            g1.Holder.Descriptor.Buffs.RemoveFact(hold);
            row["immediately"] = State(g1);
            row["callbacks"] = EventsSince(seq);
            foreach (object step in SettleGameTime(1.0, 10f, () => g1.Holder.Get<UnitPartGrappleInitiator>() == null)) yield return step;
            row["afterNativeTick"] = State(g1);
            row["callbacksAll"] = EventsSince(seq);
            row["holderAlive"] = !g1.Holder.Descriptor.State.IsDead && !g1.Holder.Destroyed;
            ExpectEnded(g1, "g1HoldBuffRemoval");
            JObject a1 = (JObject)row["afterNativeTick"];
            if ((bool)a1["initiatorPart"] || (bool)a1["holderCantAct"] || (bool)a1["holderCantMove"] || ((JArray)a1["raw.links"]).Count != 0)
                problems.Add("g1HoldBuffRemoval: holder-side state remains: " + a1.ToString(Formatting.None));
            if (!(bool)row["holderAlive"]) problems.Add("g1HoldBuffRemoval: the holder did not stay alive");
            if (!((JArray)row["callbacks"]).Any(e => (string)e["what"] == "hold.OnFactDeactivate")) problems.Add("g1HoldBuffRemoval: OnFactDeactivate was not delivered");
            if (unrelated1 == null || !g1.Targets[0].Descriptor.Buffs.RawFacts.Contains(unrelated1)) problems.Add("g1HoldBuffRemoval: the unrelated effect on the target was not kept");
            ExpectHeld(control, "g1HoldBuffRemoval.control");

            // G2: remove a two-link holder's actual multi-hold buff.
            UnitEntityData main = Game.Instance.Player.MainCharacter.Value;
            Vector3 right = Quaternion.Euler(0, main.Orientation + 90f, 0) * Vector3.forward;
            Vector3 caseSpot = ObstacleAnalyzer.TraceAlongNavmesh(main.Position, main.Position - right * 6f);
            Hold g2 = null;
            foreach (object step in Establish("g2Removal", true, caseSpot, x => g2 = x)) yield return step;
            if (g2 == null) yield break;
            Buff unrelated2 = AddUnrelated(g2.Targets[1]);
            Game.Instance.IsPaused = true;
            yield return null;
            row = new JObject { ["before"] = State(g2) };
            cases["g2MultiHoldRemoval"] = row;
            seq = LastSeq;
            g2.Holder.Descriptor.Buffs.RemoveFact(HoldBuffOf(g2));
            row["immediately"] = State(g2);
            row["callbacks"] = EventsSince(seq);
            row["holderAlive"] = !g2.Holder.Descriptor.State.IsDead && !g2.Holder.Destroyed;
            ExpectEnded(g2, "g2MultiHoldRemoval");
            if (((JArray)((JObject)row["immediately"])["raw.links"]).Count != 0) problems.Add("g2MultiHoldRemoval: raw records remain");
            if (!(bool)row["holderAlive"]) problems.Add("g2MultiHoldRemoval: the holder did not stay alive");
            if (!((JArray)row["callbacks"]).Any(e => (string)e["what"] == "hold.OnFactDeactivate")) problems.Add("g2MultiHoldRemoval: OnFactDeactivate was not delivered");
            if (unrelated2 == null || !g2.Targets[1].Descriptor.Buffs.RawFacts.Contains(unrelated2)) problems.Add("g2MultiHoldRemoval: the unrelated effect was not kept");
            ExpectHeld(control, "g2MultiHoldRemoval.control");

            // G1 accelerated expiry, for disposal attribution (shortened deadline, explicit native UpdateNextEvent/Tick).
            Hold e1 = null;
            foreach (object step in Establish("g1Expiry", false, null, x => e1 = x)) yield return step;
            if (e1 == null) yield break;
            row = new JObject { ["before"] = State(e1) };
            cases["g1Expiry"] = row;
            seq = LastSeq;
            Buff marker = e1.Holder.Descriptor.Buffs.Enumerable.First(b => ReferenceEquals(b.Blueprint, BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff));
            typeof(Buff).GetProperty("EndTime", Any).SetValue(marker, Game.Instance.TimeController.GameTime - TimeSpan.FromMilliseconds(1), null);
            e1.Holder.Descriptor.Buffs.UpdateNextEvent();
            e1.Holder.Descriptor.Buffs.Tick();
            foreach (object step in SettleGameTime(1.0, 20f, () => e1.Holder.Destroyed && e1.Targets[0].Get<UnitPartGrappleTarget>() == null)) yield return step;
            row["after"] = State(e1);
            row["callbacks"] = EventsSince(seq);
            ExpectEnded(e1, "g1Expiry");
            ExpectHeld(control, "g1Expiry.control");
            if (controlUnrelated == null || !control.Targets[0].Descriptor.Buffs.RawFacts.Contains(controlUnrelated)) problems.Add("removal: the control's unrelated effect was not kept");
            cases["controlFinal"] = State(control);
        }

        // ---------------------------------------------------------------- cadence

        private IEnumerable<object> Cadence()
        {
            var cases = new JObject();
            result["cases"] = cases;
            UnitEntityData main = Game.Instance.Player.MainCharacter.Value;
            Vector3 right = Quaternion.Euler(0, main.Orientation + 90f, 0) * Vector3.forward;
            Hold g1 = null, g2 = null;
            foreach (object step in Establish("cadenceG1", false, null, x => g1 = x)) yield return step;
            foreach (object step in Establish("cadenceG2", true, ObstacleAnalyzer.TraceAlongNavmesh(main.Position, main.Position - right * 6f), x => g2 = x)) yield return step;
            if (g1 == null || g2 == null) yield break;
            Buff unrelated = AddUnrelated(g1.Targets[0]);
            // Age both holds through at least one real round (native RoundNumber >= 1 on each hold buff).
            foreach (object step in SettleGameTime(1.0, 30f, () => HoldBuffOf(g1)?.RoundNumber >= 1 && HoldBuffOf(g2)?.RoundNumber >= 1)) yield return step;
            Game.Instance.IsPaused = true;
            yield return null;
            JObject b1 = State(g1), b2 = State(g2);
            cases["beforeSaves"] = new JObject { ["g1"] = b1, ["g2"] = b2 };
            if ((int)b1["holdBuff"]["roundNumber"] < 1 || (int)b2["holdBuff"]["roundNumber"] < 1) { problems.Add("cadence: the holds did not age through a round"); yield break; }
            ExpectHeld(g1, "cadence.before"); ExpectHeld(g2, "cadence.before");
            if (problems.Count > 0) yield break;
            var saves = new JArray();
            cases["saves"] = saves;
            for (int index = 0; index < 2; index++)
            {
                TimeSpan clock = Game.Instance.TimeController.GameTime;
                int seq = LastSeq;
                bool saved = false;
                foreach (object step in NativeSave("ver02c2ar1c" + index, x => saved = x)) yield return step;
                JObject a1 = State(g1), a2 = State(g2);
                var row = new JObject { ["index"] = index, ["completed"] = saved, ["gameClockUnchanged"] = Game.Instance.TimeController.GameTime == clock,
                    ["g1"] = a1, ["g2"] = a2, ["callbacks"] = EventsSince(seq) };
                saves.Add(row);
                if (!saved) { problems.Add("cadence: save " + index + " did not complete"); yield break; }
                if (!(bool)row["gameClockUnchanged"]) problems.Add("cadence: save " + index + " changed the game clock");
                foreach (var pair in new[] { new { b = b1, a = a1, n = "g1" }, new { b = b2, a = a2, n = "g2" } })
                {
                    if (!JToken.DeepEquals(pair.b["raw.links"], pair.a["raw.links"])) problems.Add("cadence: save " + index + " changed " + pair.n + " raw records");
                    if (!JToken.DeepEquals(pair.b["occupancy"], pair.a["occupancy"])) problems.Add("cadence: save " + index + " changed " + pair.n + " limb occupancy");
                    foreach (string key in new[] { "instance", "roundNumber", "nextTickTicks", "tickTimeTicks", "casterId", "savedCasterId" })
                        if (!JToken.DeepEquals(pair.b["holdBuff"][key], pair.a["holdBuff"][key])) problems.Add("cadence: save " + index + " changed " + pair.n + " hold buff " + key);
                    for (int t = 0; t < ((JArray)pair.b["targets"]).Count; t++)
                    {
                        JObject tb = (JObject)pair.b["targets"][t], ta = (JObject)pair.a["targets"][t];
                        if ((int)tb["damage"] != (int)ta["damage"]) problems.Add("cadence: save " + index + " changed " + pair.n + " target " + t + " damage");
                        if (!JToken.DeepEquals(tb["heldBuffs"], ta["heldBuffs"])) problems.Add("cadence: save " + index + " changed " + pair.n + " target " + t + " held buff state");
                    }
                }
                ExpectHeld(g1, "cadence.save" + index); ExpectHeld(g2, "cadence.save" + index);
                if (unrelated == null || !g1.Targets[0].Descriptor.Buffs.RawFacts.Contains(unrelated)) problems.Add("cadence: save " + index + " lost the unrelated effect");
            }
            if (problems.Count > 0) yield break;

            // The next scheduled round: exactly one more round at the pre-save schedule, not restarted or duplicated.
            long next1 = (long)b1["holdBuff"]["nextTickTicks"], next2 = (long)b2["holdBuff"]["nextTickTicks"];
            long tick1 = (long)b1["holdBuff"]["tickTimeTicks"], tick2 = (long)b2["holdBuff"]["tickTimeTicks"];
            int round1 = (int)b1["holdBuff"]["roundNumber"], round2 = (int)b2["holdBuff"]["roundNumber"];
            long target = Math.Max(next1, next2) + TimeSpan.FromSeconds(0.5).Ticks;
            int seqRound = LastSeq;
            foreach (object step in SettleGameTime(0.0, 30f, () => Game.Instance.TimeController.GameTime.Ticks >= target)) yield return step;
            JObject r1 = State(g1), r2 = State(g2);
            var nextRound = new JObject { ["g1"] = r1, ["g2"] = r2, ["maintain"] = EventsSince(seqRound, e => (string)e["what"] == "MaintainLink" || ((string)e["what"]).StartsWith("SummonHoldComponent.", StringComparison.Ordinal)) };
            cases["nextRound"] = nextRound;
            bool g1Released = ((JArray)nextRound["maintain"]).Any(e => (string)e["what"] == "MaintainLink" && (string)e["owner"] == g1.Holder.UniqueId && ((string)e["result"] ?? "").StartsWith("released", StringComparison.Ordinal));
            bool g2Released = ((JArray)nextRound["maintain"]).Any(e => (string)e["what"] == "MaintainLink" && (string)e["owner"] == g2.Holder.UniqueId && ((string)e["result"] ?? "").StartsWith("released", StringComparison.Ordinal));
            nextRound["legitimateMaintainRelease"] = new JObject { ["g1"] = g1Released, ["g2"] = g2Released };
            CheckCadence(r1, round1, next1, tick1, g1Released, "g1");
            CheckCadence(r2, round2, next2, tick2, g2Released, "g2");
            if (!g1Released) ExpectHeld(g1, "cadence.nextRound");
            if (!g2Released) ExpectHeld(g2, "cadence.nextRound");

            // Keep both relationships for the harness save and the same-process load.
            g1.Holder.Descriptor.CustomName = "KMG_C2aR1_G1Holder";
            g2.Holder.Descriptor.CustomName = "KMG_C2aR1_G2Holder";
            NamedHolders.Clear();
            NamedHolders.Add(g1.Holder.UniqueId); NamedHolders.Add(g2.Holder.UniqueId);
            foreach (UnitEntityData unit in new[] { g1.Holder, g2.Holder }.Concat(g1.Targets).Concat(g2.Targets)) { created.Remove(unit); awakeAdded.Remove(unit); }
            cases["handedToUnload"] = new JObject { ["g1"] = State(g1), ["g2"] = State(g2) };
            SaveEventSeq = LastSeq;
        }

        private void CheckCadence(JObject after, int round, long next, long tick, bool released, string name)
        {
            JToken hold = after["holdBuff"];
            if (released && hold.Type == JTokenType.Null) return;
            if (hold.Type == JTokenType.Null) { problems.Add("cadence: " + name + " lost its hold buff in the next round without a traced maintain release"); return; }
            if ((int)hold["roundNumber"] != round + 1) problems.Add("cadence: " + name + " RoundNumber " + hold["roundNumber"] + " after the next scheduled round, expected " + (round + 1));
            if ((long)hold["nextTickTicks"] != next + tick) problems.Add("cadence: " + name + " next tick " + hold["nextTickTicks"] + ", expected the pre-save schedule plus one tick " + (next + tick));
        }

        // ---------------------------------------------------------------- after load

        private IEnumerable<object> AfterLoad()
        {
            var cases = new JObject();
            result["cases"] = cases;
            JArray all = EventsSince(SaveEventSeq);
            result["lifecycle"] = all;
            int unloadSeq = all.Where(e => (string)e["what"] == "Game.HandleAreaBeginUnloading").Select(e => (int)e["seq"]).DefaultIfEmpty(0).First();
            int loadedSeq = all.Where(e => (string)e["what"] == "Game.OnAreaLoaded.begin").Select(e => (int)e["seq"]).DefaultIfEmpty(0).LastOrDefault();
            int resetSeq = all.Where(e => (string)e["what"] == "ResetLoadedGrapples").Select(e => (int)e["seq"]).DefaultIfEmpty(0).LastOrDefault();
            var ordering = new JObject { ["handleAreaBeginUnloading"] = unloadSeq, ["onAreaLoaded"] = loadedSeq, ["resetLoadedGrapples"] = resetSeq };
            cases["ordering"] = ordering;
            if (unloadSeq == 0) problems.Add("afterLoad: Game.HandleAreaBeginUnloading was not observed (route did not unload)");
            if (loadedSeq == 0) problems.Add("afterLoad: Game.OnAreaLoaded was not observed");
            foreach (string id in NamedHolders)
            {
                var mine = new JArray(all.Where(e => (string)e["owner"] == id).Cast<object>().ToArray());
                JObject begin = mine.OfType<JObject>().FirstOrDefault(e => (string)e["what"] == "hold.OnAreaBeginUnloading" && (int)e["seq"] < loadedSeq);
                JObject offOld = mine.OfType<JObject>().FirstOrDefault(e => ((string)e["what"] == "hold.OnTurnOff" || (string)e["what"] == "hold.OnFactDeactivate") && (int)e["seq"] > unloadSeq && (int)e["seq"] < loadedSeq);
                JObject releaseOld = mine.OfType<JObject>().FirstOrDefault(e => ((string)e["what"]).StartsWith("SummonHoldComponent.Release", StringComparison.Ordinal) && (int)e["seq"] > unloadSeq && (int)e["seq"] < loadedSeq);
                JObject didLoad = mine.OfType<JObject>().FirstOrDefault(e => (string)e["what"] == "hold.OnAreaDidLoad" && (int)e["seq"] > loadedSeq);
                cases["holder." + id] = new JObject
                {
                    ["events"] = mine, ["unloadSeen"] = begin, ["firstTurnOffOrDeactivateDuringUnload"] = offOld,
                    ["firstReleaseDuringUnload"] = releaseOld, ["didLoadOnLoadedInstance"] = didLoad
                };
                if (begin == null) problems.Add("afterLoad: holder " + id + " hold buff did not observe OnAreaBeginUnloading before arrival");
                if (offOld == null) problems.Add("afterLoad: holder " + id + " hold buff was not turned off or deactivated during the unload");
                if (offOld != null && (string)offOld["what"] == "hold.OnTurnOff" && offOld["ownerTurnedOn"].Type == JTokenType.Boolean && !(bool)offOld["ownerTurnedOn"] && offOld["instanceAreaUnloading"].Type == JTokenType.Boolean && !(bool)offOld["instanceAreaUnloading"])
                    problems.Add("afterLoad: holder " + id + " was suspended at unload without its unload state");
            }
            // Loaded relationships are reset (the accepted policy).
            var loadedHolders = NamedHolders.Select(id => Game.Instance.State.Units.FirstOrDefault(u => u.UniqueId == id)).ToArray();
            cases["loadedHolders"] = new JArray(loadedHolders.Select(u => (object)(u == null ? null : new JObject
            {
                ["id"] = u.UniqueId, ["holdBuffs"] = new JArray(u.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(b => b.Blueprint.name.Contains("Grapple_Hold") || b.Blueprint.name.Contains("Grapple_MultiHold")).Select(b => (object)b.Blueprint.name).ToArray()),
                ["initiatorPart"] = u.Get<UnitPartGrappleInitiator>() != null, ["raw.links"] = RawLinks(u) == null ? null : new JArray(RawLinks(u).Cast<object>().ToArray())
            })).ToArray());
            foreach (UnitEntityData u in loadedHolders.Where(u => u != null))
            {
                created.Add(u);
                if (u.Get<UnitPartGrappleInitiator>() != null || u.Descriptor.Buffs.RawFacts.OfType<Buff>().Any(b => b.Blueprint.name.Contains("Grapple_Hold") || b.Blueprint.name.Contains("Grapple_MultiHold")) ||
                    (RawLinks(u) != null && RawLinks(u).Count != 0))
                    problems.Add("afterLoad: a loaded holder was not reset: " + u.UniqueId);
            }
            foreach (UnitEntityData u in Game.Instance.State.Units.Where(u => u.Get<UnitPartGrappleTarget>() != null || u.Descriptor.Buffs.RawFacts.OfType<Buff>().Any(b => b.Blueprint.name.EndsWith("Grapple_Grappled", StringComparison.Ordinal) || b.Blueprint.name.EndsWith("Grapple_MultiHeld", StringComparison.Ordinal))).ToArray())
                problems.Add("afterLoad: a unit still carries held state after the load: " + u.UniqueId);

            // Fresh relationships after the load, and a permitted save: the stale unload state must not decide it.
            UnitEntityData main = Game.Instance.Player.MainCharacter.Value;
            Vector3 right = Quaternion.Euler(0, main.Orientation + 90f, 0) * Vector3.forward;
            Hold g1 = null, g2 = null;
            foreach (object step in Establish("afterLoadG1", false, null, x => g1 = x)) yield return step;
            foreach (object step in Establish("afterLoadG2", true, ObstacleAnalyzer.TraceAlongNavmesh(main.Position, main.Position - right * 6f), x => g2 = x)) yield return step;
            if (g1 == null || g2 == null) yield break;
            Game.Instance.IsPaused = true;
            yield return null;
            JObject b1 = State(g1), b2 = State(g2);
            cases["freshBeforeSave"] = new JObject { ["g1"] = b1, ["g2"] = b2, ["saveAllowed"] = Game.Instance.SaveManager.IsSaveAllowed() };
            if (b1["holdInstanceAreaUnloading"].Type != JTokenType.Boolean || (bool)b1["holdInstanceAreaUnloading"] ||
                b2["holdInstanceAreaUnloading"].Type != JTokenType.Boolean || (bool)b2["holdInstanceAreaUnloading"])
                problems.Add("afterLoad: a fresh hold's own unload state is not false before the save");
            int seq = LastSeq;
            TimeSpan clock = Game.Instance.TimeController.GameTime;
            bool saved = false;
            foreach (object step in NativeSave("ver02c2ar1post", x => saved = x)) yield return step;
            JObject a1 = State(g1), a2 = State(g2);
            cases["freshAfterSave"] = new JObject { ["completed"] = saved, ["gameClockUnchanged"] = Game.Instance.TimeController.GameTime == clock,
                ["g1"] = a1, ["g2"] = a2, ["callbacks"] = EventsSince(seq) };
            if (!saved) problems.Add("afterLoad: the post-load save did not complete");
            if (Game.Instance.TimeController.GameTime != clock) problems.Add("afterLoad: the post-load save changed the game clock");
            ExpectHeld(g1, "afterLoad.freshAfterSave");
            ExpectHeld(g2, "afterLoad.freshAfterSave");
            if (!JToken.DeepEquals(b1["raw.links"], a1["raw.links"]) || !JToken.DeepEquals(b2["raw.links"], a2["raw.links"])) problems.Add("afterLoad: the post-load save changed raw records");
            var turnOffs = ((JArray)((JObject)cases["freshAfterSave"])["callbacks"]).OfType<JObject>().Where(e => (string)e["what"] == "hold.OnTurnOff").ToArray();
            if (turnOffs.Length < 2) problems.Add("afterLoad: the post-load save did not turn both fresh holds off");
            if (turnOffs.Any(e => e["instanceAreaUnloading"].Type != JTokenType.Boolean || (bool)e["instanceAreaUnloading"]))
                problems.Add("afterLoad: a fresh hold saw unload state during the post-load save");
        }
    }
}
