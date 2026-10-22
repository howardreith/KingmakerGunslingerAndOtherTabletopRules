using System;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.RuleSystem;
using Kingmaker.Utility;
using Newtonsoft.Json;

namespace KingmakerGunslinger.Summoning
{
    // Grant the canonical shared circle with this creature's SLA caster level.
    // Native AddFacts passes no context (CL1). Like AddFacts, retain the exact
    // applied fact across reapplication/load and remove only that instance.
    [Serializable]
    public sealed class ArchonConstantMagicCircle : OwnedGameLogicComponent<UnitDescriptor>
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintBuff Carrier;
        public BlueprintAbility Spell;
        public int CasterLevel;
        [JsonProperty] private Buff _applied;

        public override void OnFactActivate()
        {
            if (IsReapplying) { _applied?.Recalculate(); return; }
            var unit = Owner?.Unit;
            if (unit == null || !ReferenceEquals(unit.Blueprint, OwningBlueprint)) return;
            if (Carrier == null || Spell == null || CasterLevel < 1)
                throw new InvalidOperationException("Archon constant circle dependency is invalid.");
            // A bare MechanicsContext inherits Params, ultimately the shared
            // AbilityParams.Empty. A native ability context owns its parameters
            // and its buff/area children retain them without global mutation.
            var context = new AbilityExecutionContext(new AbilityData(Spell, Owner),
                new AbilityParams { CasterLevel = CasterLevel, SpellLevel = 3,
                    DC = 13 + Owner.Stats.Charisma.Bonus },
                new TargetWrapper(unit), Rulebook.CurrentContext);
            _applied = Owner.AddFact(Carrier, context) as Buff;
            if (_applied == null)
                throw new InvalidOperationException("Archon constant circle grant failed.");
        }

        public override void OnFactDeactivate()
        {
            if (IsReapplying) return;
            _applied?.Remove();
            _applied = null;
        }
    }
}
