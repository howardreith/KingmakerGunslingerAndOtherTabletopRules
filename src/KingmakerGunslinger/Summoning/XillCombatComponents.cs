using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// The Xill's racial bases, set once at creation on the exact unit that
    /// carries this component.
    ///
    /// <para>The native outsider class the chassis is built on gives a generic
    /// outsider's progression, which is not this creature's. Three things have
    /// to be corrected and nothing else: the printed racial hit points, the
    /// creature-specific good and poor save pair, and the five skills the
    /// printed stat block spends ranks on. These are racial bases, changed
    /// once, never live totals, so every buff, item, level and condition that
    /// should still reach this creature still does.</para>
    ///
    /// <para>Modelled on <see cref="SummonSalamanderRacialProfile"/>, which
    /// does the same three things for the Sprint 17 Salamander.</para>
    /// </summary>
    [Serializable]
    public sealed class SummonXillRacialProfile :
        OwnedGameLogicComponent<UnitDescriptor>,
        IHandleEntityComponent<UnitEntityData>
    {
        public BlueprintUnit OwningBlueprint;

        public void OnEntityCreated(UnitEntityData unit)
        {
            if (unit == null || OwningBlueprint == null ||
                !ReferenceEquals(unit.Blueprint, OwningBlueprint))
                throw new InvalidOperationException(
                    "Xill racial profile requires its exact owner.");
            var stats = unit.Descriptor.Stats;
            stats.HitPoints.BaseValue = XillRulesPolicy.BaseRacialHitPoints;
            // Good Fortitude and Reflex, poor Will. The generic outsider donor
            // does not choose that creature-specific pair.
            stats.SaveFortitude.BaseValue = XillRulesPolicy.GoodSave;
            stats.SaveReflex.BaseValue = XillRulesPolicy.GoodSave;
            stats.SaveWill.BaseValue = XillRulesPolicy.PoorSave;
            // The printed ranks, placed on the five Kingmaker skills that
            // carry a printed skill of their own. Acrobatics becomes Mobility
            // and Bluff becomes Persuasion. Intimidate, Sense Motive and
            // Knowledge (planes) have no Kingmaker skill of their own and each
            // lands on a skill one of the five already holds, so each is
            // merged rather than added: adding would hand the creature
            // competence its stat block does not print. The merge is recorded
            // in XillRulesPolicy and in the release notes.
            stats.SkillMobility.BaseValue = XillRulesPolicy.MobilityRanks;
            stats.SkillStealth.BaseValue = XillRulesPolicy.StealthRanks;
            stats.SkillPerception.BaseValue = XillRulesPolicy.PerceptionRanks;
            stats.SkillPersuasion.BaseValue = XillRulesPolicy.PersuasionRanks;
            stats.SkillKnowledgeArcana.BaseValue =
                XillRulesPolicy.KnowledgeArcanaRanks;
        }

        public void OnEntityRemoved(UnitEntityData unit) { }
    }

    /// <summary>
    /// Sets the Xill's paralysis save to its printed derivation rather than a
    /// pinned 16: ten plus half its hit dice plus its live Constitution
    /// modifier. An unmodified Xill saves at DC 16, and a buffed or weakened
    /// one saves at what it actually supports.
    /// </summary>
    [Serializable]
    public sealed class ContextActionSetXillParalysisDc : ContextAction
    {
        public override string GetCaption()
        {
            return "Set Xill paralysis DC";
        }

        public override void RunAction()
        {
            if (Context == null || Context.MaybeCaster == null ||
                Context.Params == null) return;
            UnitEntityData caster = Context.MaybeCaster;
            int hitDice = caster.Descriptor == null ? XillRulesPolicy.HitDice :
                Math.Max(1, caster.Descriptor.Progression.CharacterLevel);
            Context.Params.DC = XillRulesPolicy.ParalysisDifficultyClass(
                hitDice, caster.Stats.Constitution.Bonus);
        }
    }
}
