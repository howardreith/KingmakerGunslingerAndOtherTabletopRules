using System;
using Kingmaker;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Buffs.Components;
using Newtonsoft.Json;

namespace KingmakerGunslinger.Summoning
{
    // Native Filth Fever's graph saves every round. Keep its native disease
    // descriptor/immunity/removal surface in the builder, but own this printed
    // day-scale clock. Every save and ability-damage outcome still uses rules.
    [Serializable]
    public sealed class DevilChills34 : BuffLogic, ITickEachRound
    {
        [JsonProperty] private bool initialized;
        [JsonProperty] private TimeSpan nextExposure;
        [JsonProperty] private int consecutiveSaves;
        internal TimeSpan NextExposure { get { return nextExposure; } }
        internal int ConsecutiveSaves { get { return consecutiveSaves; } }

        public override void OnTurnOn()
        {
            base.OnTurnOn();
            if (initialized) return;
            var source = Buff.Context?.MaybeCaster;
            if (source == null || Game.Instance?.TimeController == null) {
                Buff.Remove(); return;
            }
            var onset = new RuleRollDice(source, new DiceFormula(1, DiceType.D4));
            Buff.Context.TriggerRule(onset);
            int days = onset.Result;
            nextExposure = Game.Instance.TimeController.GameTime + TimeSpan.FromDays(days);
            initialized = true;
        }

        public void OnNewRound()
        {
            var source = Buff.Context?.MaybeCaster;
            if (!InfernalEffectLifetime34.SourceIsLive(source)) { Buff.Remove(); return; }
            if (!initialized || Game.Instance?.TimeController == null) return;
            var now = Game.Instance.TimeController.GameTime;
            while (now >= nextExposure) {
                nextExposure += TimeSpan.FromDays(1);
                var saving = new RuleSavingThrow(Owner.Unit, SavingThrowType.Fortitude, Buff.Context.Params.DC);
                Buff.Context.TriggerRule(saving);
                consecutiveSaves = saving.IsPassed ? consecutiveSaves + 1 : 0;
                if (consecutiveSaves >= 3) { Buff.Remove(); return; }
                if (!saving.IsPassed) {
                    var damage = new RuleDealStatDamage(source, Owner.Unit, StatType.Strength,
                        new DiceFormula(1, DiceType.D4), 0);
                    Buff.Context.TriggerRule(damage);
                }
                if (Owner.State.IsDead) return;
            }
        }
    }
}
