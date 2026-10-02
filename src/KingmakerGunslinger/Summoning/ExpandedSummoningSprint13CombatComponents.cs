using System;
using System.Linq;
using Kingmaker;
using Kingmaker.AreaLogic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Enums;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// The printed Wolverine rage trigger: "A wolverine that takes damage in
    /// combat flies into a rage on its next turn."
    ///
    /// <para>This component only schedules the rage. It watches damage
    /// resolved against its own carrier and, on actual positive damage,
    /// applies the hidden onset marker whose own round-boundary component
    /// starts the rage one round later. Nothing here applies the +4/+4/-2,
    /// which is what keeps the printed delay impossible to skip: there is no
    /// code path from a damage event to the rage state that does not pass
    /// through a round boundary.</para>
    ///
    /// <para>The rage is summon-local by construction. Both buffs are applied
    /// to <c>Owner.Unit</c> and to nothing else, so no owner, caster, ally or
    /// other summon can receive them, and both leave with the creature.</para>
    /// </summary>
    [Serializable]
    public sealed class SummonRageOnDamageComponent :
        RuleTargetLogicComponent<RuleDealDamage>
    {
        /// <summary>
        /// The hidden marker that waits one round boundary and then applies
        /// <see cref="RageBuff"/>.
        /// </summary>
        public BlueprintBuff OnsetBuff;

        /// <summary>
        /// The rage state itself, carried here only so this component can see
        /// whether the creature is already raging and decline to re-trigger.
        /// </summary>
        public BlueprintBuff RageBuff;

        public override void OnEventAboutToTrigger(RuleDealDamage evt) { }

        public override void OnEventDidTrigger(RuleDealDamage evt)
        {
            UnitEntityData owner = Owner == null ? null : Owner.Unit;
            if (owner == null || evt == null || OnsetBuff == null ||
                RageBuff == null) return;
            int actualDamage = Math.Max(0, evt.Damage);
            bool ownerIsTarget = ReferenceEquals(evt.Target, owner);
            bool ownerAvailable = SummonDiseaseExposure.IsAvailable(owner);
            bool alreadyRaging = HasBuff(owner, RageBuff);
            bool onsetPending = HasBuff(owner, OnsetBuff);
            if (!SummonRagePolicy.ShouldScheduleRage(actualDamage,
                ownerIsTarget, ownerAvailable, alreadyRaging, onsetPending))
                return;
            var context = new MechanicsContext(owner, owner.Descriptor,
                OnsetBuff, Fact == null ? null : Fact.MaybeContext,
                new TargetWrapper(owner));
            var apply = new RuleApplyBuff(owner, OnsetBuff, context, null,
                (buff, source, time) =>
                    owner.Descriptor.Buffs.AddBuff(buff, source, time));
            Rulebook.Trigger(apply);
        }

        internal static bool HasBuff(UnitEntityData unit, BlueprintBuff buff)
        {
            if (unit == null || unit.Descriptor == null || buff == null)
                return false;
            return unit.Descriptor.Buffs.GetBuff(buff) != null;
        }
    }

    /// <summary>
    /// The printed Shadow Mastiff bay, applied to one creature caught by the
    /// ability's own 300-foot spread.
    ///
    /// <para>Bestiary 3: "When a shadow mastiff howls or barks, all creatures
    /// within a 300-foot spread except evil outsiders must succeed at a DC 16
    /// Will save or become panicked for 1d4 rounds. This is a sonic,
    /// mind-affecting fear effect. A creature that successfully saves cannot
    /// be affected by the same mastiff's bay for 24 hours."</para>
    ///
    /// <para>The spread, the save, the duration and the exposure of the
    /// summoner's own party are all exactly as printed. What is bounded is who
    /// decides to use it: bay is a player-activated ability and the mastiff's
    /// own brain never selects it, so the charter's prohibition on repeated
    /// friendly-fire effects from constrained caster AI is satisfied without
    /// weakening the rule. The rule's own 24-hour immunity bounds repeat use.</para>
    ///
    /// <para>The immunity is matched by comparing the stored buff's caster to
    /// this mastiff, which is exactly right in every case that can arise: the
    /// same living mastiff matches and is blocked, a different mastiff does not
    /// match and may bay as printed, and a mastiff that has since expired
    /// resolves to no caster and could never bay again anyway.</para>
    /// </summary>
    [Serializable]
    public sealed class ContextActionShadowMastiffBay : ContextAction
    {
        public BlueprintUnit SourceUnit;
        public BlueprintBuff PanicBuff;
        public BlueprintBuff ImmunityBuff;

        /// <summary>The exact native evil subtype feature.</summary>
        public BlueprintUnitFact EvilSubtype;

        /// <summary>The exact native outsider class whose levels mark the type.</summary>
        public BlueprintCharacterClass OutsiderClass;

        public override string GetCaption()
        { return "Resolve the printed Shadow Mastiff bay against one creature"; }

        public override void RunAction()
        {
            UnitEntityData caster = Context == null ? null : Context.MaybeCaster;
            UnitEntityData target = Target == null ? null : Target.Unit;
            if (caster == null || target == null || SourceUnit == null ||
                PanicBuff == null || ImmunityBuff == null ||
                !ReferenceEquals(caster.Blueprint, SourceUnit)) return;
            bool available = SummonDiseaseExposure.IsAvailable(target);
            bool evilOutsider = IsEvilOutsider(target);
            bool immune = IsImmuneTo(target, caster);
            if (!SummonShadowMastiffPolicy.ShouldRollBay(available,
                evilOutsider, immune)) return;
            int dc = SummonShadowMastiffPolicy.BayWillDc(
                ExpandedSummoningSpecialProfiles.ShadowMastiffHitDice,
                caster.Stats.Charisma.ModifiedValue);
            var saveContext = new MechanicsContext(caster, target.Descriptor,
                PanicBuff, Context, new TargetWrapper(target));
            saveContext.Params.DC = dc;
            var saving = new RuleSavingThrow(target, SavingThrowType.Will, dc);
            saving.Reason = saveContext;
            saveContext.TriggerRule(saving);
            if (SummonShadowMastiffPolicy.GrantsImmunityOnSave(saving.IsPassed))
            {
                Apply(caster, target, Context, ImmunityBuff,
                    TimeSpan.FromHours(ExpandedSummoningSpecialProfiles
                        .ShadowMastiffBayImmunityHours));
                return;
            }
            if (!SummonShadowMastiffPolicy.AppliesPanicOnFailedSave(
                saving.IsPassed)) return;
            int rounds = 0;
            for (int roll = 0; roll < ExpandedSummoningSpecialProfiles
                .ShadowMastiffBayPanicDiceCount; roll++)
                rounds += UnityEngine.Random.Range(1,
                    ExpandedSummoningSpecialProfiles
                        .ShadowMastiffBayPanicDieSides + 1);
            Apply(caster, target, Context, PanicBuff, TimeSpan.FromSeconds(
                rounds * GameConsts.RoundDuration));
        }

        private static void Apply(UnitEntityData caster, UnitEntityData target,
            MechanicsContext parent, BlueprintBuff payload, TimeSpan duration)
        {
            var context = new MechanicsContext(caster, target.Descriptor,
                payload, parent, new TargetWrapper(target));
            var apply = new RuleApplyBuff(target, payload, context, duration,
                (buff, source, time) =>
                    target.Descriptor.Buffs.AddBuff(buff, source, time));
            Rulebook.Trigger(apply);
        }

        /// <summary>
        /// The printed exemption is for evil outsiders, which is the creature
        /// type plus the subtype. Both are read from exact blueprint references
        /// rather than from a name or an alignment guess, and the mastiff
        /// itself satisfies both, so it is exempt by the printed clause rather
        /// than by a special case for the caster.
        /// </summary>
        private bool IsEvilOutsider(UnitEntityData unit)
        {
            if (unit == null || unit.Descriptor == null ||
                EvilSubtype == null || OutsiderClass == null) return false;
            return unit.Descriptor.HasFact(EvilSubtype) &&
                unit.Descriptor.Progression.GetClassLevel(OutsiderClass) > 0;
        }

        private bool IsImmuneTo(UnitEntityData target, UnitEntityData mastiff)
        {
            if (target == null || target.Descriptor == null ||
                ImmunityBuff == null) return false;
            foreach (Buff buff in target.Descriptor.Buffs.RawFacts
                .OfType<Buff>())
            {
                if (!ReferenceEquals(buff.Blueprint, ImmunityBuff)) continue;
                if (buff.Context != null &&
                    ReferenceEquals(buff.Context.MaybeCaster, mastiff))
                    return true;
            }
            return false;
        }
    }

    /// <summary>
    /// The printed Shadow Mastiff shadow blend: "In any condition of
    /// illumination other than full daylight, a shadow mastiff disappears into
    /// the shadows, giving it concealment (50% miss chance). Artificial
    /// illumination, even a light or continual flame spell, does not negate
    /// this ability; a daylight spell, however, does. A shadow mastiff can
    /// suspend or resume this ability as a free action."
    ///
    /// <para>The grade is <c>Concealment.Total</c>, the engine's 50% miss
    /// chance; <c>Partial</c> is the 20% grade and would halve the ability.</para>
    ///
    /// <para>This component owns its concealment entry rather than leaving a
    /// native <c>AddConcealment</c> to add one unconditionally. The obvious
    /// alternative - carrying <c>AddConcealment</c> and suppressing the buff
    /// when the negations hold - does not work: <c>Buff.IsSuppressed</c> is a
    /// plain field that gates only the per-round mechanics tick, so it never
    /// turns a component off and the entry would have survived full daylight,
    /// leaving the creature stronger than its own stat block.</para>
    ///
    /// <para>Because the entry is owned here the decision is re-made at every
    /// concealment check rather than on a round cadence, which is what the
    /// printed wording says. Ownership is explicit in both directions: added
    /// only while the printed condition holds, and removed by
    /// <see cref="OnTurnOff"/> when the player suspends the ability or the
    /// creature leaves.</para>
    ///
    /// <para>The engine models no ambient illumination at all - no light-level
    /// type outside the rendering namespaces, and no light or darkness spell
    /// descriptor - so "full daylight" is read from the only
    /// illumination-adjacent state it exposes: the sun is up and this area's
    /// lighting follows it. Artificial light is deliberately absent because the
    /// printed text says it does not matter.</para>
    /// </summary>
    [Serializable]
    public sealed class SummonShadowBlendComponent :
        RuleTargetLogicComponent<RuleConcealmentCheck>
    {
        /// <summary>The printed grade: Total is the 50% miss chance.</summary>
        public Concealment Grade = Concealment.Total;

        /// <summary>
        /// The concealment family this belongs to. Deliberately not
        /// TargetIsInvisible, which would let See Invisibility defeat an
        /// ability that is not invisibility, and not Fog, which would tie it to
        /// wind and weather effects.
        /// </summary>
        public ConcealmentDescriptor Descriptor = ConcealmentDescriptor.Blur;

        /// <summary>
        /// Exact native spells whose effect negates shadow blend. These are
        /// abilities, not buffs: the audited native Daylight identity is a
        /// spell blueprint, so negation is detected by finding a buff whose
        /// own context names one of these as the ability that applied it.
        /// </summary>
        public BlueprintAbility[] NegatingAbilities;

        private bool _added;

        /// <summary>
        /// Set while the component is active, for the runtime gate to read:
        /// what the live creature actually carried and what the printed
        /// condition decided from it.
        /// </summary>
        [NonSerialized] internal string LastDecision = "<not evaluated>";

        public override void OnTurnOn() { Refresh(); }

        public override void OnTurnOff() { Remove(); }

        public override void OnEventAboutToTrigger(RuleConcealmentCheck evt)
        {
            UnitEntityData owner = Owner == null ? null : Owner.Unit;
            if (owner == null || evt == null ||
                !ReferenceEquals(evt.Target, owner)) return;
            Refresh();
        }

        public override void OnEventDidTrigger(RuleConcealmentCheck evt) { }

        private void Refresh()
        {
            UnitEntityData owner = Owner == null ? null : Owner.Unit;
            if (owner == null) { Remove(); return; }
            bool daylight = IsFullDaylight();
            string negatingSource;
            bool negated = HasNegatingEffect(owner, out negatingSource);
            bool grants = SummonShadowMastiffPolicy.GrantsShadowConcealment(
                daylight, negated);
            LastDecision = "fullDaylight=" + daylight + ";negatedBy=" +
                negatingSource + ";grants=" + grants;
            if (grants && !_added)
            {
                Owner.Ensure<UnitPartConcealment>().AddConcealment(Entry());
                _added = true;
            }
            else if (!grants && _added)
            {
                Owner.Ensure<UnitPartConcealment>().RemoveConcealement(Entry());
                _added = false;
            }
        }

        private void Remove()
        {
            if (!_added || Owner == null) { _added = false; return; }
            Owner.Ensure<UnitPartConcealment>().RemoveConcealement(Entry());
            _added = false;
        }

        /// <summary>
        /// Built the way the native component builds its own, so the engine's
        /// by-value removal matches what was added.
        /// </summary>
        private UnitPartConcealment.ConcealmentEntry Entry()
        {
            return new UnitPartConcealment.ConcealmentEntry {
                Concealment = Grade,
                Descriptor = Descriptor,
                OnlyForAttacks = true
            };
        }

        /// <summary>
        /// The sun is up and this area's lighting follows it. An area flagged
        /// as a single light scene does not change with the time of day, which
        /// is how the engine marks interiors and dungeons, so such an area is
        /// never full daylight however bright its fixed lighting looks.
        /// </summary>
        internal static bool IsFullDaylight()
        {
            if (Game.Instance == null) return false;
            BlueprintArea area = Game.Instance.CurrentlyLoadedArea;
            if (area == null || area.IsSingleLightScene) return false;
            return Game.Instance.TimeOfDay == TimeOfDay.Day;
        }

        private bool HasNegatingEffect(UnitEntityData unit, out string source)
        {
            source = "<none>";
            if (NegatingAbilities == null || unit == null ||
                unit.Descriptor == null) return false;
            foreach (Buff buff in unit.Descriptor.Buffs.RawFacts.OfType<Buff>())
            {
                MechanicsContext context = buff.MaybeContext;
                BlueprintAbility applied = context == null ? null :
                    context.SourceAbility;
                if (applied == null) continue;
                for (int index = 0; index < NegatingAbilities.Length; index++)
                    if (NegatingAbilities[index] != null &&
                        ReferenceEquals(applied, NegatingAbilities[index]))
                    {
                        source = applied.name;
                        return true;
                    }
            }
            return false;
        }
    }
}
