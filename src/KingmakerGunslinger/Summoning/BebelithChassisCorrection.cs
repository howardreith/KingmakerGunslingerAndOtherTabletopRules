using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// The three printed numbers the native outsider chassis cannot produce,
    /// measured on a live Bebelith and corrected on the creature.
    ///
    /// <para>The first guarded run of this sprint read the creature back and
    /// found its saves assigned to the wrong two slots. The printed block is
    /// Fortitude +15, Reflex +11, Will +7 - one good save, one good save and
    /// one poor one. The chassis delivered Fortitude +11, Reflex +11, Will
    /// +11, which is a poor Fortitude and a good Will: exactly the two
    /// swapped. Reflex was already right and is untouched.</para>
    ///
    /// <para>It also found 162 hit points against a printed 150. Twelve hit
    /// dice and Constitution 24 give 84 from Constitution either way, so the
    /// difference is entirely in the racial half: the printed block averages
    /// a d10 at 5.5 a die and the chassis averages one point more. Twelve
    /// dice, twelve points.</para>
    ///
    /// <para>Every value here is a DIFFERENCE, derived from the chassis the
    /// engine actually applied rather than a number typed to make a total
    /// come out. The shared native outsider class is not touched: it is the
    /// chassis of a dozen released creatures, and correcting one creature by
    /// editing what every creature stands on is how a sprint reaches past its
    /// own boundary.</para>
    /// </summary>
    internal static class BebelithChassisCorrection
    {
        /// <summary>
        /// The gap between a good save and a poor one at this creature's hit
        /// dice. Twelve hit dice: a good save is 8 and a poor one is 4.
        /// </summary>
        internal static int SaveSwap
        {
            get { return BebelithRulesPolicy.GoodSave -
                BebelithRulesPolicy.PoorSave; }
        }

        /// <summary>
        /// What the chassis adds per hit die beyond the printed d10 average.
        /// Measured: 162 live against 150 printed, with the same 84 from
        /// Constitution in both, is twelve points across twelve dice.
        /// </summary>
        internal const int ChassisHitPointsPerDieOverPrinted = 1;

        internal static int HitPointCorrection
        {
            get { return -BebelithRulesPolicy.HitDice *
                ChassisHitPointsPerDieOverPrinted; }
        }

        internal static void Validate()
        {
            // The two saves the chassis swapped, stated as the printed block
            // states them so a drifting constant breaks here first.
            if (BebelithRulesPolicy.GoodSave + BebelithRulesPolicy
                    .ConstitutionModifier != BebelithRulesPolicy.PrintedFortitude)
                throw new InvalidOperationException(
                    "The printed Bebelith Fortitude is a good save plus "
                    + "Constitution; the correction derives from that.");
            if (BebelithRulesPolicy.PoorSave +
                    BebelithRulesPolicy.WisdomModifier +
                    BebelithRulesPolicy.IronWillBonus !=
                    BebelithRulesPolicy.PrintedWill)
                throw new InvalidOperationException(
                    "The printed Bebelith Will is a poor save plus Wisdom and "
                    + "Iron Will; the correction derives from that.");
            if (SaveSwap <= 0)
                throw new InvalidOperationException(
                    "A good save must exceed a poor one, or there is nothing "
                    + "to correct.");
            // The hit points, derived the same way: the printed racial half
            // against what the chassis produced.
            int printedRacial = BebelithRulesPolicy.PrintedHitPoints -
                BebelithRulesPolicy.HitDice *
                BebelithRulesPolicy.ConstitutionModifier;
            if (printedRacial != BebelithRulesPolicy.BaseRacialHitPoints)
                throw new InvalidOperationException(
                    "The printed Bebelith racial hit points changed.");
            if (HitPointCorrection >= 0)
                throw new InvalidOperationException(
                    "The chassis gives more hit points than the block prints, "
                    + "so the correction removes them.");
        }
    }

    /// <summary>
    /// The creature-owned carrier that applies those three corrections.
    ///
    /// <para>One component rather than three, because the three are one
    /// finding: the chassis this creature stands on is not the chassis its
    /// block was written for. Splitting them would suggest three unrelated
    /// adjustments and invite the next sprint to drop one.</para>
    /// </summary>
    [Serializable]
    public sealed class BebelithChassisCorrectionComponent :
        OwnedGameLogicComponent<UnitDescriptor>
    {
        private ModifiableValue.Modifier _fortitude;
        private ModifiableValue.Modifier _will;
        private ModifiableValue.Modifier _hitPoints;

        public override void OnTurnOn()
        {
            Apply();
        }

        public override void OnTurnOff()
        {
            Remove();
        }

        private void Apply()
        {
            Remove();
            if (Owner == null) return;
            BebelithChassisCorrection.Validate();
            int swap = BebelithChassisCorrection.SaveSwap;
            _fortitude = Owner.Stats.SaveFortitude.AddModifier(
                swap, Fact, GetType().FullName,
                ModifierDescriptor.UntypedStackable);
            _will = Owner.Stats.SaveWill.AddModifier(
                -swap, Fact, GetType().FullName,
                ModifierDescriptor.UntypedStackable);
            _hitPoints = Owner.Stats.HitPoints.AddModifier(
                BebelithChassisCorrection.HitPointCorrection, Fact,
                GetType().FullName, ModifierDescriptor.UntypedStackable);
            Owner.Stats.SaveFortitude.UpdateValue();
            Owner.Stats.SaveWill.UpdateValue();
            Owner.Stats.HitPoints.UpdateValue();
        }

        private void Remove()
        {
            foreach (ModifiableValue.Modifier modifier in new[] {
                _fortitude, _will, _hitPoints })
                if (modifier != null) modifier.Remove();
            _fortitude = null;
            _will = null;
            _hitPoints = null;
        }
    }
}
