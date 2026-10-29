using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Only exact fixture objects are observed. Void hooks never skip/replace native work.
        private sealed class SuccubusDrainTrace33 : IDisposable
        {
            private static SuccubusDrainTrace33 active;
            private readonly HarmonyInstance hooks;
            private readonly List<MethodInfo> methods = new List<MethodInfo>();
            private readonly AddInitiatorAttackWithWeaponTrigger trigger;
            private readonly ContextActionDealDamage action;
            private readonly Buff fact;
            private readonly UnitEntityData owner;
            private readonly UnitEntityData[] actors;
            private RuleAttackWithWeapon current;
            internal readonly JObject Evidence = new JObject();
            private readonly JArray rows = new JArray();
            private readonly JArray patches = new JArray();
            private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            private static int Id(object value) => value == null ? 0 : RuntimeHelpers.GetHashCode(value);
            internal SuccubusDrainTrace33(UnitEntityData owner, Buff fact, AddInitiatorAttackWithWeaponTrigger trigger,
                ContextActionDealDamage action, params UnitEntityData[] actors)
            {
                if (active != null) throw new InvalidOperationException("Nested Succubus trace.");
                this.owner = owner; this.fact = fact; this.trigger = trigger; this.action = action; this.actors = actors;
                hooks = HarmonyInstance.Create("KMG.SuccubusDrain33." + Guid.NewGuid().ToString("N"));
                Evidence["rows"] = rows; Evidence["existingPatches"] = patches;
                Evidence["hooksRemoved"] = false;
                try
                {
                    active = this;
                    Patch(typeof(AddInitiatorAttackWithWeaponTrigger).GetMethod("OnEventDidTrigger", new[] { typeof(RuleAttackWithWeapon) }), nameof(AttackCallback));
                    Patch(typeof(AddInitiatorAttackWithWeaponTrigger).GetMethod("OnEventDidTrigger", new[] { typeof(RuleAttackWithWeaponResolve) }), nameof(ResolveCallback));
                    Patch(typeof(AddInitiatorAttackWithWeaponTrigger).GetMethod("TryRunActions", Flags), nameof(TryActions));
                    Patch(typeof(AddInitiatorAttackWithWeaponTrigger).GetMethod("CheckCondition", Flags), nameof(BeforeCheck), nameof(AfterCheck));
                    Patch(typeof(ActionList).GetMethod("Run"), nameof(ListEntry));
                    Patch(typeof(ContextActionDealDamage).GetMethod("RunAction"), nameof(ActionEntry));
                    Patch(typeof(ContextActionDealDamage).GetMethod("DrainEnergy", Flags), nameof(DrainEntry));
                    Record("attached", null);
                }
                catch { Dispose(); throw; }
            }
            private void Patch(MethodInfo method, string before, string after = null)
            {
                if (method == null) throw new MissingMethodException("Succubus native trace surface");
                var info = hooks.GetPatchedMethods().Contains(method) ? hooks.GetPatchInfo(method) : null;
                patches.Add(new JObject { ["method"] = method.DeclaringType.FullName + "." + method,
                    ["owners"] = new JArray(info?.Owners ?? Enumerable.Empty<string>()) });
                methods.Add(method);
                hooks.Patch(method, new HarmonyMethod(typeof(SuccubusDrainTrace33), before),
                    after == null ? null : new HarmonyMethod(typeof(SuccubusDrainTrace33), after));
            }
            private JObject Fields()
            {
                var values = new JObject();
                foreach (var f in typeof(AddInitiatorAttackWithWeaponTrigger).GetFields(Flags))
                {
                    object value = f.GetValue(trigger);
                    if (f.FieldType == typeof(bool) || f.FieldType.IsEnum) values[f.Name] = JToken.FromObject(value);
                    else if (f.Name == "DistanceLessEqual") values[f.Name] = ((Feet)value).Meters;
                }
                values["WeaponType"] = trigger.WeaponType?.AssetGuid;
                return values;
            }
            private void Record(string stage, RuleAttackWithWeapon rule, bool? condition = null)
            {
                try
                {
                    if (rule != null) current = rule;
                    var r = rule ?? current;
                    var command = owner.Commands.Raw.OfType<UnitAttack>().FirstOrDefault(c => !c.IsFinished);
                    var context = typeof(ContextAction).GetProperty("Context", Flags)?.GetValue(action, null) as MechanicsContext;
                    var target = typeof(ContextAction).GetProperty("Target", Flags)?.GetValue(action, null) as TargetWrapper;
                    rows.Add(new JObject { ["stage"] = stage, ["gameTime"] = Game.Instance.Player.GameTime.TotalSeconds,
                        ["trigger"] = Id(trigger), ["fact"] = Id(fact), ["factBlueprint"] = fact.Blueprint.AssetGuid,
                        ["list"] = Id(trigger.Action), ["action"] = Id(action), ["listContainsExactAction"] = trigger.Action.Actions.Any(a => ReferenceEquals(a, action)),
                        ["listening"] = trigger.IsListeningEvents, ["factOwner"] = owner.UniqueId,
                        ["factCaster"] = fact.Context?.MaybeCaster?.UniqueId, ["fields"] = Fields(),
                        ["rule"] = Id(r), ["command"] = Id(command), ["initiator"] = r?.Initiator?.UniqueId,
                        ["victim"] = r?.Target?.UniqueId, ["weapon"] = r?.Weapon?.Blueprint.AssetGuid,
                        ["category"] = r?.Weapon?.Blueprint.Category.ToString(), ["natural"] = r?.Weapon?.Blueprint.Type.IsNatural,
                        ["hit"] = r?.AttackRoll?.IsHit, ["firstAttack"] = r?.IsFirstAttack, ["fullAttack"] = r?.IsFullAttack,
                        ["critical"] = r?.AttackRoll?.IsCriticalConfirmed, ["sneak"] = r?.AttackRoll?.IsSneakAttack,
                        ["targetHP"] = r?.Target?.HPLeft, ["conditionResult"] = condition,
                        ["durationDiceCountNull"] = action.Duration.DiceCountValue == null,
                        ["executionCaster"] = context?.MaybeCaster?.UniqueId, ["executionTarget"] = target?.Unit?.UniqueId,
                        ["levels"] = new JArray(actors.Where(a => a != null).Select(a => new JObject {
                            ["unit"] = a.UniqueId, ["count"] = a.Get<UnitPartNegativeLevels>()?.Count ?? 0 })) });
                }
                catch (Exception e) { Evidence["error"] = e.ToString(); }
            }
            private static bool Exact(object instance) => active != null && ReferenceEquals(instance, active.trigger);
            private static void AttackCallback(AddInitiatorAttackWithWeaponTrigger __instance, RuleAttackWithWeapon __0)
            { if (Exact(__instance)) active.Record("attack-callback", __0); }
            private static void ResolveCallback(AddInitiatorAttackWithWeaponTrigger __instance, RuleAttackWithWeaponResolve __0)
            { if (Exact(__instance)) active.Record("resolve-callback", __0.AttackWithWeapon); }
            private static void TryActions(AddInitiatorAttackWithWeaponTrigger __instance, RuleAttackWithWeapon __0)
            { if (Exact(__instance)) active.Record("try-actions", __0); }
            private static void BeforeCheck(AddInitiatorAttackWithWeaponTrigger __instance, RuleAttackWithWeapon __0)
            { if (Exact(__instance)) active.Record("condition-before", __0); }
            private static void AfterCheck(AddInitiatorAttackWithWeaponTrigger __instance, RuleAttackWithWeapon __0, bool __result)
            { if (Exact(__instance)) active.Record("condition-after", __0, __result); }
            private static void ListEntry(ActionList __instance)
            { if (active != null && ReferenceEquals(__instance, active.trigger.Action)) active.Record("action-list", null); }
            private static void ActionEntry(ContextActionDealDamage __instance)
            { if (active != null && ReferenceEquals(__instance, active.action)) active.Record("action-entry", null); }
            private static void DrainEntry(ContextActionDealDamage __instance)
            { if (active != null && ReferenceEquals(__instance, active.action)) active.Record("drain-entry", null); }
            public void Dispose()
            {
                hooks.UnpatchAll(hooks.Id);
                Evidence["hooksRemoved"] = methods.All(m => !hooks.GetPatchedMethods().Contains(m) || !hooks.GetPatchInfo(m).Owners.Contains(hooks.Id));
                if (ReferenceEquals(active, this)) active = null;
            }
        }
    }
}
