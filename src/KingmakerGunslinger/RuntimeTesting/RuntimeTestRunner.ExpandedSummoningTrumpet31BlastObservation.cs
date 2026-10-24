using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Read-only native rules. AddBuff raises an inner RuleApplyBuff before
        // the outer callback finishes; retain both events and separately read
        // the real terminal state once per actual physical buff. Native inner/outer
        // contexts may differ while referencing that same buff and private parameters.
        private sealed class TrumpetBlastObservation :
            IGlobalRulebookHandler<RuleSavingThrow>,
            IGlobalRulebookHandler<RuleApplyBuff>,
            IGlobalRulebookHandler<RuleRollDice>,
            IGlobalRulebookHandler<RuleSpellResistanceCheck>
        {
            private readonly BlueprintAbility _blast;
            private readonly BlueprintBuff _paralyzed;
            private readonly HashSet<UnitEntityData> _sources;
            private readonly List<BlastApplication> _applications = new List<BlastApplication>();
            internal readonly JArray Saves = new JArray();
            internal readonly JArray EffectEvents = new JArray();
            internal readonly JArray Dice = new JArray();
            internal readonly JArray Resistance = new JArray();

            private sealed class BlastApplication
            {
                internal UnitEntityData Target;
                internal Buff Buff;
                internal AbilityParams Parameters;
                internal JObject LastEvent;
                internal int NativeEventCount;
                internal JObject Snapshot()
                {
                    var row = (JObject)LastEvent.DeepClone();
                    row["nativeEventCount"] = NativeEventCount;
                    row["terminalSnapshot"] = true;
                    row["disposed"] = Buff?.IsDisposed;
                    row["notDispelable"] = Buff?.IsNotDispelable;
                    row["end"] = Buff?.EndTime.ToString();
                    row["nativeParalyzed"] = Target != null &&
                        Target.Descriptor.State.HasCondition(UnitCondition.Paralyzed);
                    return row;
                }
            }

            internal JArray Effects
            { get { return new JArray(_applications.Select(value => value.Snapshot())); } }

            internal TrumpetBlastObservation(BlueprintAbility blast,
                BlueprintBuff paralyzed, params UnitEntityData[] sources)
            {
                _blast = blast; _paralyzed = paralyzed;
                _sources = new HashSet<UnitEntityData>(sources);
            }

            public void OnEventAboutToTrigger(RuleSavingThrow evt) { }
            public void OnEventDidTrigger(RuleSavingThrow evt)
            {
                var context = evt.Reason?.Context;
                if (context == null || !ReferenceEquals(context.SourceAbility, _blast) ||
                    !_sources.Contains(context.MaybeCaster)) return;
                Saves.Add(new JObject { ["caster"] = context.MaybeCaster.UniqueId,
                    ["target"] = evt.Initiator.UniqueId, ["sourceAbility"] = _blast.AssetGuid,
                    ["dc"] = evt.DifficultyClass, ["type"] = evt.Type.ToString(),
                    ["autoPass"] = evt.AutoPass, ["nativeStatValue"] = evt.StatValue,
                    ["d20"] = evt.D20.Value, ["passed"] = evt.IsPassed,
                    ["cl"] = context.Params.CasterLevel,
                    ["parameters"] = RuntimeHelpers.GetHashCode(context.Params) });
            }

            public void OnEventAboutToTrigger(RuleApplyBuff evt) { }
            public void OnEventDidTrigger(RuleApplyBuff evt)
            {
                if (!ReferenceEquals(evt.Blueprint, _paralyzed) ||
                    evt.Context == null || !_sources.Contains(evt.Context.MaybeCaster)) return;
                var row = new JObject { ["caster"] = evt.Context.MaybeCaster.UniqueId,
                    ["target"] = evt.Initiator.UniqueId, ["buff"] = evt.Blueprint.AssetGuid,
                    ["event"] = RuntimeHelpers.GetHashCode(evt),
                    ["applicationContext"] = RuntimeHelpers.GetHashCode(evt.Context),
                    ["buffInstance"] = evt.AppliedBuff == null ? (int?)null : RuntimeHelpers.GetHashCode(evt.AppliedBuff),
                    ["dc"] = evt.Context.Params.DC, ["cl"] = evt.Context.Params.CasterLevel,
                    ["parameters"] = RuntimeHelpers.GetHashCode(evt.Context.Params),
                    ["durationSeconds"] = evt.Duration?.TotalSeconds,
                    ["canApply"] = evt.CanApply, ["immune"] = evt.Immunity,
                    ["storyImmune"] = evt.StoryModeImmunity,
                    ["applied"] = evt.AppliedBuff != null,
                    ["end"] = evt.AppliedBuff?.EndTime.ToString(),
                    ["notDispelable"] = evt.AppliedBuff?.IsNotDispelable,
                    ["nativeParalyzed"] = evt.Initiator.Descriptor.State.HasCondition(UnitCondition.Paralyzed) };
                EffectEvents.Add(row);
                var application = _applications.FirstOrDefault(value =>
                    evt.AppliedBuff != null ? ReferenceEquals(value.Buff, evt.AppliedBuff) :
                    value.Buff == null && ReferenceEquals(value.Target, evt.Initiator) &&
                    ReferenceEquals(value.Parameters, evt.Context.Params));
                if (application == null)
                {
                    application = new BlastApplication { Target = evt.Initiator,
                        Parameters = evt.Context.Params };
                    _applications.Add(application);
                }
                if (!ReferenceEquals(application.Target, evt.Initiator) ||
                    !ReferenceEquals(application.Parameters, evt.Context.Params) ||
                    application.Buff != null && evt.AppliedBuff != null &&
                    !ReferenceEquals(application.Buff, evt.AppliedBuff))
                    throw new InvalidOperationException("One physical blast buff was reused for another target/private parameter set.");
                application.NativeEventCount++;
                application.Buff = evt.AppliedBuff;
                application.LastEvent = row;
            }

            public void OnEventAboutToTrigger(RuleRollDice evt) { }
            public void OnEventDidTrigger(RuleRollDice evt)
            {
                var context = evt.Reason?.Context;
                if (context == null || !ReferenceEquals(context.SourceAbility, _blast) ||
                    !_sources.Contains(evt.Initiator)) return;
                Dice.Add(new JObject { ["caster"] = evt.Initiator.UniqueId,
                    ["target"] = context.MainTarget.Unit?.UniqueId,
                    ["formula"] = evt.DiceFormula.ToString(),
                    ["nativeResult"] = evt.Result.Value, ["sourceAbility"] = _blast.AssetGuid });
            }

            public void OnEventAboutToTrigger(RuleSpellResistanceCheck evt) { }
            public void OnEventDidTrigger(RuleSpellResistanceCheck evt)
            {
                if (!ReferenceEquals(evt.Ability, _blast) ||
                    !_sources.Contains(evt.Context?.MaybeCaster)) return;
                Resistance.Add(new JObject { ["caster"] = evt.Context.MaybeCaster.UniqueId,
                    ["target"] = evt.Target.UniqueId, ["resistance"] = evt.SpellResistance,
                    ["hasRoll"] = evt.HasResistanceRoll, ["ignored"] = evt.IgnoreSpellResistance,
                    ["resisted"] = evt.IsSpellResisted });
            }
        }
    }
}
