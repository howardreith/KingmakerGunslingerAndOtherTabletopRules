using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Components;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json;

namespace KingmakerGunslinger.TurnUndead
{
    /// <summary>
    /// The owned Turn Undead flight effect. It applies the native Frightened
    /// condition exactly as the native undead-turning buff does (fleeing AI and
    /// condition presentation stay native), and replaces that buff's
    /// unconditional every-round save with the feat's intelligent-only recovery.
    ///
    /// The buff carries no spell descriptor, so undead mind/fear descriptor
    /// immunities stay intact for every other effect; this is the native
    /// FrightenedUndead exception. Separate Frightened condition immunity (for
    /// example a story protection) still blocks the condition.
    ///
    /// Primitive [JsonProperty] fields round-trip through native
    /// Fact.PreSave/PostLoad by this component's stable name. No live object is
    /// serialized. Classification and DC are fixed when the effect is applied,
    /// so a reload never reclassifies, refreshes the minute or re-rolls.
    /// </summary>
    [Serializable]
    public sealed class TurnUndeadFleeingState : BuffLogic, ITickEachRound
    {
        internal const string ComponentName = "$KMG_TurnUndead_FleeingState";

        [JsonProperty] private bool _initialized;
        [JsonProperty] private bool _mindless;
        [JsonProperty] private bool _ownerReview;
        [JsonProperty] private int _dc;
        [JsonProperty] private string _rule;
        [JsonProperty] private string _sourceAbility;

        internal bool Initialized { get { return _initialized; } }
        internal bool Mindless { get { return _mindless; } }
        internal int Dc { get { return _dc; } }
        internal string Rule { get { return _rule; } }
        internal string SourceAbility { get { return _sourceAbility; } }

        public override void OnFactActivate()
        {
            if (_initialized) return;
            UnitEntityData unit = Owner == null ? null : Owner.Unit;
            if (unit == null || Buff == null || Buff.Context == null || Buff.Context.Params == null)
                throw new InvalidOperationException(
                    "Turn Undead flight requires its owner and the turning ability's context.");
            TurnUndeadClassification classification =
                TurnUndeadClassificationPolicy.Classify(TurnUndeadUnitAdapter.Read(unit));
            _mindless = classification.Mind == TurnUndeadMind.Mindless;
            _ownerReview = classification.OwnerReview;
            _rule = classification.Rule;
            _dc = Buff.Context.Params.DC;
            _sourceAbility = Buff.Context.SourceAbility == null ? string.Empty :
                Buff.Context.SourceAbility.AssetGuid;
            _initialized = true;
            TurnUndeadDiagnostics.Info("flight.applied", "unit=" + unit.UniqueId +
                ";blueprint=" + (unit.Blueprint == null ? "" : unit.Blueprint.AssetGuid) +
                ";mind=" + classification.Mind + ";rule=" + _rule + ";ownerReview=" +
                _ownerReview + ";dc=" + _dc + ";source=" + _sourceAbility +
                ";evidence=" + classification.Evidence);
        }

        public override void OnTurnOn()
        { Owner.State.AddCondition(UnitCondition.Frightened, Buff); }

        public override void OnTurnOff()
        { Owner.State.RemoveCondition(UnitCondition.Frightened); }

        public void OnNewRound()
        {
            if (TurnUndeadRules.DecideRound(_initialized, _mindless) !=
                TurnUndeadRoundDecision.RollRepeatSave)
                return;
            // Reason = this buff, as the native round save does. Its context's
            // SourceAbility is the turning ability, so channel resistance and
            // other ability-specific save modifiers apply to the repeat save.
            RuleSavingThrow save = Game.Instance.Rulebook.TriggerEvent(
                new RuleSavingThrow(Owner.Unit, SavingThrowType.Will, _dc) { Reason = Fact });
            TurnUndeadDiagnostics.Info("flight.repeat-save", "unit=" + Owner.Unit.UniqueId +
                ";dc=" + _dc + ";passed=" + save.IsPassed);
            if (save.IsPassed) Buff.Remove();
        }
    }

    /// <summary>Reads the stable identities the classification policy needs.</summary>
    internal static class TurnUndeadUnitAdapter
    {
        private static BlueprintUnitFact[] _markers;

        internal static TurnUndeadUnitRecord Read(UnitEntityData unit)
        {
            if (unit == null) throw new ArgumentNullException("unit");
            BlueprintUnit blueprint = unit.Blueprint;
            string unitGuid = blueprint == null ? string.Empty : blueprint.AssetGuid;
            string typeGuid = blueprint == null || blueprint.Type == null ? string.Empty :
                blueprint.Type.AssetGuid;
            var facts = new List<string>();
            foreach (BlueprintUnitFact marker in Markers())
                if (unit.Descriptor.HasFact(marker)) facts.Add(marker.AssetGuid);
            return new TurnUndeadUnitRecord(unitGuid, typeGuid, facts);
        }

        private static BlueprintUnitFact[] Markers()
        {
            if (_markers != null) return _markers;
            // A marker absent from this installation simply never matches.
            _markers = TurnUndeadClassificationPolicy.IntelligentFactGuids
                .Select(guid => ResourcesLibrary.TryGetBlueprint<BlueprintUnitFact>(guid))
                .Where(value => value != null).ToArray();
            return _markers;
        }
    }

    internal static class TurnUndeadDiagnostics
    {
        internal const string Category = "turn-undead";

        internal static void Info(string code, string message)
        {
            ModContext context;
            if (ModContext.TryGet(out context)) context.Logger.Info(Category, code, message);
        }

        internal static void Warning(string code, string message)
        {
            ModContext context;
            if (ModContext.TryGet(out context)) context.Logger.Warning(Category, code, message);
        }

        internal static void Failure(string code, string message, Exception exception)
        {
            ModContext context;
            if (ModContext.TryGet(out context))
                context.Logger.Failure(Category, code, message, exception);
        }
    }
}
