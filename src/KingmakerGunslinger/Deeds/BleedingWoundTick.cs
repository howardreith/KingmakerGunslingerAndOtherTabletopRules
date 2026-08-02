using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;

namespace KingmakerGunslinger.Deeds
{
    public sealed class BleedingWoundTick :
        OwnedGameLogicComponent<UnitDescriptor>, ITickEachRound
    {
        public BleedingWoundKind Kind;

        public void OnNewRound()
        {
            if (Owner == null || Owner.Unit == null || Fact == null ||
                Fact.MaybeContext == null) return;
            UnitEntityData source = Fact.MaybeContext.MaybeCaster;
            if (source == null) return;
            if (Kind == BleedingWoundKind.HitPoints)
            {
                int amount = Math.Max(0, source.Stats.Dexterity.Bonus);
                if (amount == 0) return;
                var direct = new DirectDamage(new DiceFormula(0, DiceType.D6),
                    amount);
                var damage = new RuleDealDamage(source, Owner.Unit,
                    new DamageBundle(direct)) {
                    DisablePrecisionDamage = true
                };
                Rulebook.Trigger(damage);
                return;
            }
            StatType stat = Kind == BleedingWoundKind.Strength ? StatType.Strength :
                Kind == BleedingWoundKind.Dexterity ? StatType.Dexterity :
                StatType.Constitution;
            Rulebook.Trigger(new RuleDealStatDamage(source, Owner.Unit, stat,
                new DiceFormula(0, DiceType.D6), 1));
        }
    }
}
