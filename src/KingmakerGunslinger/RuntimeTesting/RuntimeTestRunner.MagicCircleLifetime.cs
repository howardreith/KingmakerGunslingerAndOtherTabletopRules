using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Components;
using KingmakerGunslinger.Blueprints;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private static void CircleLifecycle(UnitEntityData caster, UnitEntityData bearer,
            UnitEntityData recipient, Spellbook book, MagicCircleBlueprintSet circle,
            List<UnitEntityData> actors, List<RuntimeTestAssertion> assertions, List<string> diagnostics)
        {
            var game = Game.Instance;
            CircleDispel(caster, bearer, recipient, book, circle, actors, assertions, diagnostics);
            book.Rest();
            var metamagic = new MetamagicData { SpellLevelCost = Metamagic.Extend.DefaultCost() };
            metamagic.Add(Metamagic.Extend);
            var extended = new AbilityData(circle.Spell, book) { MetamagicData = metamagic };
            int slots = book.GetSpontaneousSlots(4);
            CircleCast(caster, bearer, extended, diagnostics);
            var carrier = CircleBuffs(bearer, circle.Carrier).Single();
            var area = CircleArea(carrier);
            CircleRefresh(area, actors);
            assertions.Add(Assertion("circle-extend-native-context-and-cost", "double original duration; one level-4 slot",
                "seconds=" + carrier.TimeLeft.TotalSeconds + ";slots=" + slots + "->" + book.GetSpontaneousSlots(4),
                extended.SpellLevel == 4 && carrier.Context.HasMetamagic(Metamagic.Extend) &&
                area.Context.HasMetamagic(Metamagic.Extend) &&
                Math.Abs(carrier.TimeLeft.TotalSeconds - 1200 * book.CasterLevel) < 2 &&
                book.GetSpontaneousSlots(4) == slots - 1 &&
                ReferenceEquals(area.Context.MaybeOwner, bearer),
                "native metamagic AbilityData/UnitUseAbility, original caster and different area owner"));

            TimeSpan deadline = carrier.EndTime;
            foreach (var unit in new[] { bearer, recipient })
            {
                var originalFaction = unit.Faction;
                var alternative = ReferenceEquals(unit, bearer) ? recipient.Blueprint.Faction : bearer.Blueprint.Faction;
                var contribution = CircleBuffs(unit, circle.Recipient).Single();
                try {
                    unit.Descriptor.SwitchFactions(alternative, true);
                    CircleRefresh(area, actors);
                    assertions.Add(Assertion("circle-faction-change-" + (unit == bearer ? "bearer" : "recipient"),
                        "same contribution and carrier deadline after native faction change",
                        "changed=" + ReferenceEquals(unit.Faction, alternative) + ";count=" + CircleBuffs(unit, circle.Recipient).Length,
                        !ReferenceEquals(originalFaction, alternative) && ReferenceEquals(unit.Faction, alternative) &&
                        CircleBuffs(unit, circle.Recipient).Length == 1 && unit.Buffs.Enumerable.Contains(contribution) &&
                        carrier.EndTime == deadline && !area.IsEnded,
                        "native SwitchFactions(resetAttackFactions=true); no faction-sensitive coverage filter"));
                }
                finally { unit.Descriptor.SwitchFactions(originalFaction, true); CircleRefresh(area, actors); }
            }
            foreach (var unit in new[] { caster, bearer })
            {
                int damage = unit.Damage;
                bool cheater = unit.Blueprint.IsCheater;
                try {
                    unit.Blueprint.IsCheater = false;
                    unit.Damage = unit.Stats.HitPoints.ModifiedValue + 1;
                    CircleLifeTick(unit);
                    CircleRefresh(area, actors);
                    assertions.Add(Assertion("circle-unconscious-" + (unit == caster ? "caster" : "bearer"),
                        "unconscious living creature; same carrier deadline and protection",
                        "conscious=" + unit.Descriptor.State.IsConscious + ";dead=" + unit.Descriptor.State.IsDead,
                        !unit.Descriptor.State.IsConscious && !unit.Descriptor.State.IsDead &&
                        carrier.Active && carrier.EndTime == deadline && !area.IsEnded &&
                        CircleBuffs(recipient, circle.Recipient).Length == 1,
                        "native UnitLifeController resolves negative HP; no class-aura shutdown component"));
                }
                finally { unit.Damage = damage; CircleLifeTick(unit); unit.Blueprint.IsCheater = cheater; }
            }

            // Exercise both sides of the ORIGINAL deadline. Only owned buff/area
            // controllers run while the game is paused; restore the global clock
            // before returning to normal updates, just as existing aura fixtures do.
            TimeSpan clock = game.Player.GameTime;
            bool beforeExpiry = false, afterExpiry = false;
            try {
                game.Player.GameTime = deadline - TimeSpan.FromSeconds(.1);
                bearer.Buffs.Tick(); CircleRefresh(area, actors);
                beforeExpiry = carrier.Active && !area.IsEnded && CircleBuffs(recipient, circle.Recipient).Length == 1;
                game.Player.GameTime = deadline + TimeSpan.FromSeconds(.1);
                bearer.Buffs.Tick(); CircleRefresh(area, actors);
                afterExpiry = !bearer.Buffs.Enumerable.Contains(carrier) && area.IsEnded &&
                    CircleBuffs(recipient, circle.Recipient).Length == 0;
            }
            finally { game.Player.GameTime = clock; }
            assertions.Add(Assertion("circle-native-original-expiration", "present before deadline; absent after deadline",
                "before=" + beforeExpiry + ";after=" + afterExpiry,
                beforeExpiry && afterExpiry && game.Player.GameTime == clock,
                "original extended EndTime; native BuffCollection.Tick and area exit/destruction; clock restored"));

            book.Rest();
            CircleCast(caster, bearer, new AbilityData(circle.Spell, book), diagnostics);
            carrier = CircleBuffs(bearer, circle.Carrier).Single();
            area = CircleArea(carrier);
            CircleRefresh(area, actors);
            deadline = carrier.EndTime;
            caster.Blueprint.IsCheater = false;
            // The working save may disable true death for the player faction.
            // Use this request-local actor's ordinary hostile-faction death path,
            // with experience explicitly disabled; never change difficulty or
            // set the expected final-death result directly.
            caster.Blueprint.Faction = recipient.Blueprint.Faction;
            caster.GiveExperienceOnDeath = false;
            caster.Descriptor.SwitchFactions(recipient.Blueprint.Faction, true);
            if (caster.IsPlayerFaction || ReferenceEquals(caster.Blueprint.Faction,
                Kingmaker.Blueprints.Root.BlueprintRoot.Instance.PlayerFaction))
                throw new InvalidOperationException("Final-death fixture requires its owned non-player faction.");
            var damageRule = Rulebook.Trigger(new RuleDealDamage(caster, caster,
                new DamageBundle(new DirectDamage(new DiceFormula(0, DiceType.Zero), caster.HPLeft + 100))) {
                    DisablePrecisionDamage = true, IgnoreDamageReduction = true });
            CircleLifeTick(caster);
            caster.Buffs.Tick();
            CircleRefresh(area, actors.Where(unit => !ReferenceEquals(unit, caster)).ToList());
            assertions.Add(Assertion("circle-original-caster-death", "dead original caster; living bearer retains original timed circle",
                "damage=" + damageRule.Damage + ";dead=" + caster.Descriptor.State.IsDead +
                    ";finallyDead=" + caster.Descriptor.State.IsFinallyDead + ";areaEnded=" + area.IsEnded,
                caster.Descriptor.State.IsDead && caster.Descriptor.State.IsFinallyDead && carrier.Active &&
                carrier.EndTime == deadline && !area.IsEnded && CircleBuffs(recipient, circle.Recipient).Length == 1 &&
                ReferenceEquals(carrier.Context.MaybeCaster, caster) && carrier.Context.Params.CasterLevel == book.CasterLevel,
                "actual native lethal damage and life controller; no source substitution or refreshed timer"));
            bearer.Blueprint.IsCheater = false;
            bearer.GiveExperienceOnDeath = false;
            var bearerDamage = Rulebook.Trigger(new RuleDealDamage(bearer, bearer,
                new DamageBundle(new DirectDamage(new DiceFormula(0, DiceType.Zero), bearer.HPLeft + 100))) {
                    DisablePrecisionDamage = true, IgnoreDamageReduction = true });
            CircleLifeTick(bearer);
            bearer.Buffs.Tick();
            CircleRefresh(area, actors.Where(unit => !ReferenceEquals(unit, caster) && !ReferenceEquals(unit, bearer)).ToList());
            assertions.Add(Assertion("circle-bearer-death-owned-cleanup", "bearer death ends its exact carrier, area and contributions",
                "damage=" + bearerDamage.Damage + ";dead=" + bearer.Descriptor.State.IsDead +
                    ";finallyDead=" + bearer.Descriptor.State.IsFinallyDead + ";ended=" + area.IsEnded +
                    ";recipientCount=" + CircleBuffs(recipient, circle.Recipient).Length,
                bearer.Descriptor.State.IsDead && !bearer.Buffs.Enumerable.Contains(carrier) &&
                    area.IsEnded && CircleBuffs(recipient, circle.Recipient).Length == 0,
                "owner-approved bearer-death adaptation; actual lethal damage, native death cleanup and AddAreaEffect deactivation"));
        }

        private static AreaEffectEntityData CircleArea(Buff carrier)
        {
            var component = carrier.SelectComponents<AddAreaEffect>().Single();
            var field = typeof(AddAreaEffect).GetField("m_AreaEffectInstance", BindingFlags.Instance | BindingFlags.NonPublic);
            return (AreaEffectEntityData)field.GetValue(component);
        }

        private static void CircleLifeTick(UnitEntityData unit)
        {
            typeof(UnitLifeController).GetMethod("TickOnUnit", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Invoke(new UnitLifeController(), new object[] { unit });
        }
    }
}
