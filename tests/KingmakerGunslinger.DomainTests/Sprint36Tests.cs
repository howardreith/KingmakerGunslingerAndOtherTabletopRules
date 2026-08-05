using System;
using KingmakerGunslinger.Deeds;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Classes;

namespace KingmakerGunslinger.DomainTests
{
    internal static partial class Program
    {
        private static void GunslingerInitiativePositiveGrit()
        {
            var service = new GunslingerInitiativeService();
            Assertions.Equal(2, service.CalculateBonus(1),
                "One grit did not grant +2 initiative.");
            Assertions.Equal(2, service.CalculateBonus(20),
                "Additional grit changed the fixed initiative bonus.");
        }

        private static void GunslingerInitiativeZeroGrit()
        {
            Assertions.Equal(0,
                new GunslingerInitiativeService().CalculateBonus(0),
                "Zero grit granted initiative.");
        }

        private static void GunslingerInitiativeInvalidInput()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                new GunslingerInitiativeService().CalculateBonus(-1),
                "Negative grit was accepted for initiative.");
        }

        private static void BonusFeatsExactLevels()
        {
            int[] expected = { 4, 8, 12, 16, 20 };
            int[] observed = BonusFeatProgression.Levels;
            Assertions.Equal(expected.Length, observed.Length,
                "Bonus-feat cadence count changed.");
            for (int index = 0; index < expected.Length; index++)
            {
                Assertions.Equal(expected[index], observed[index],
                    "Bonus-feat level changed.");
                Assertions.True(BonusFeatProgression.GrantsAt(expected[index]),
                    "Required bonus-feat level was rejected.");
            }
        }

        private static void BonusFeatsRejectOtherLevels()
        {
            for (int level = 0; level <= 20; level++)
            {
                bool expected = level >= 4 && level % 4 == 0;
                Assertions.Equal(expected, BonusFeatProgression.GrantsAt(level),
                    "Bonus-feat cadence changed at level " + level + ".");
            }
        }

        private static void BonusFeatsRejectInvalidLevels()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                BonusFeatProgression.GrantsAt(-1),
                "Negative Gunslinger level was accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                BonusFeatProgression.GrantsAt(21),
                "Post-cap Gunslinger level was accepted.");
            int[] copy = BonusFeatProgression.Levels;
            copy[0] = 20;
            Assertions.Equal(4, BonusFeatProgression.Levels[0],
                "Bonus-feat cadence exposed mutable storage.");
        }

        private static void GunTrainingExactLevels()
        {
            int[] expected = { 5, 9, 13, 17 };
            int[] observed = GunTrainingProgression.Levels;
            Assertions.Equal(expected.Length, observed.Length,
                "Gun Training cadence count changed.");
            for (int level = 0; level <= 20; level++)
            {
                bool required = level == 5 || level == 9 || level == 13 || level == 17;
                Assertions.Equal(required, GunTrainingProgression.GrantsAt(level),
                    "Gun Training cadence changed at level " + level + ".");
            }
            observed[0] = 17;
            Assertions.Equal(5, GunTrainingProgression.Levels[0],
                "Gun Training cadence exposed mutable storage.");
        }

        private static void GunTrainingDamageKind()
        {
            Assertions.Equal(4, GunTrainingPolicy.DamageBonus(
                FirearmKind.Pistol, FirearmKind.Pistol, 4),
                "Selected pistol did not receive Dexterity damage.");
            Assertions.Equal(0, GunTrainingPolicy.DamageBonus(
                FirearmKind.Pistol, FirearmKind.Musket, 4),
                "Unselected musket received pistol training.");
            Assertions.True(GunTrainingPolicy.IsSupportedKind(FirearmKind.Revolver),
                "Production revolver kind was omitted.");
        }

        private static void GunTrainingDamageModifiers()
        {
            Assertions.Equal(0, GunTrainingPolicy.DamageBonus(
                FirearmKind.Rifle, FirearmKind.Rifle, 0),
                "Zero Dexterity modifier changed damage.");
            Assertions.Equal(-2, GunTrainingPolicy.DamageBonus(
                FirearmKind.Blunderbuss, FirearmKind.Blunderbuss, -2),
                "Negative Dexterity modifier was not preserved.");
        }

        private static void GunTrainingMisfirePolicy()
        {
            Assertions.Equal(2, GunTrainingPolicy.EffectiveMisfireValue(
                2, FirearmCondition.Normal, false),
                "Normal firearm gained a Broken-state increase.");
            Assertions.Equal(6, GunTrainingPolicy.EffectiveMisfireValue(
                2, FirearmCondition.Broken, false),
                "Untrained Broken firearm did not gain +4 misfire.");
            Assertions.Equal(4, GunTrainingPolicy.EffectiveMisfireValue(
                2, FirearmCondition.Broken, true),
                "Trained Broken firearm did not gain +2 misfire.");
            Assertions.Equal(20, GunTrainingPolicy.EffectiveMisfireValue(
                19, FirearmCondition.Broken, false),
                "Broken misfire threshold exceeded the d20 range.");
        }

        private static void GunTrainingInvalidInputs()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                GunTrainingProgression.GrantsAt(21),
                "Post-cap level was accepted for Gun Training.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                GunTrainingPolicy.DamageBonus(FirearmKind.Unknown,
                    FirearmKind.Pistol, 1),
                "Unknown selected firearm kind was accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                GunTrainingPolicy.EffectiveMisfireValue(0,
                    FirearmCondition.Normal, false),
                "Invalid base misfire threshold was accepted.");
            Assertions.Throws<ArgumentException>(() =>
                GunTrainingPolicy.EffectiveMisfireValue(2,
                    FirearmCondition.Wrecked, true),
                "Wrecked firearm entered misfire evaluation.");
        }

        private static void DeadeyeSecondIncrementCostsOne()
        {
            DeadeyeDecision result = Deadeye((20d * 0.3048d) + 0.001d, 1);
            Assertions.Equal(DeadeyeStatus.Eligible, result.Status,
                "Second-increment Deadeye was rejected.");
            Assertions.Equal(2, result.RangeIncrement, "Deadeye increment changed.");
            Assertions.Equal(1, result.GritCost, "Second increment did not cost one grit.");
            Assertions.True(result.UsesTouchArmorClass, "Deadeye did not authorize touch AC.");
        }

        private static void DeadeyeCostScalesBeyondFirst()
        {
            DeadeyeDecision result = Deadeye((60d * 0.3048d) + 0.001d, 3);
            Assertions.Equal(4, result.RangeIncrement, "Fourth increment changed.");
            Assertions.Equal(3, result.GritCost,
                "Deadeye cost was not one per increment beyond first.");
        }

        private static void DeadeyeFirstIncrementDoesNotSpend()
        {
            DeadeyeDecision result = Deadeye(20d * 0.3048d, 5);
            Assertions.Equal(DeadeyeStatus.WithinFirstIncrement, result.Status,
                "First-increment shot incorrectly activated Deadeye.");
            Assertions.Equal(0, result.GritCost, "Rejected first increment spent grit.");
        }

        private static void DeadeyeInsufficientGritFailsAtomic()
        {
            DeadeyeDecision result = Deadeye((60d * 0.3048d) + 0.001d, 2);
            Assertions.Equal(DeadeyeStatus.InsufficientGrit, result.Status,
                "Insufficient grit was accepted.");
            Assertions.Equal(0, result.GritCost,
                "Insufficient Deadeye decision exposed a partial cost.");
        }

        private static void DeadeyeContextFailsClosed()
        {
            FirearmDefinition pistol = ProductionFirearmCatalog.CreatePistol().Definition;
            var service = new DeadeyeService();
            Assertions.Equal(DeadeyeStatus.NotArmed,
                service.Evaluate(new DeadeyeRequest(false, true, 1, pistol,
                    30d * 0.3048d, 2)).Status, "Unarmed Deadeye activated.");
            Assertions.Equal(DeadeyeStatus.NotExactFirearm,
                service.Evaluate(new DeadeyeRequest(true, false, 0, pistol,
                    30d * 0.3048d, 2)).Status, "Non-firearm Deadeye activated.");
        }

        private static void DeadeyeBlunderbussOrdinaryRangeAndInvalidDistance()
        {
            var service = new DeadeyeService();
            DeadeyeDecision blunderbuss = service.Evaluate(new DeadeyeRequest(
                true, true, 1,
                ProductionFirearmCatalog.CreateBlunderbuss().Definition,
                15d * 0.3048d, 5));
            Assertions.Equal(DeadeyeStatus.Eligible, blunderbuss.Status,
                "The ordinary Blunderbuss bullet mode did not support Deadeye.");
            Assertions.Equal(2, blunderbuss.RangeIncrement,
                "Blunderbuss Deadeye increment mismatch.");
            Assertions.Equal(1, blunderbuss.GritCost,
                "Second-increment Blunderbuss Deadeye cost mismatch.");
            Assertions.Equal(DeadeyeStatus.UnsupportedRange,
                service.Evaluate(new DeadeyeRequest(true, true, 1,
                    ProductionFirearmCatalog.CreatePistol().Definition, double.NaN, 5)).Status,
                "Invalid Deadeye distance was accepted.");
        }

        private static void DeadeyeInvalidInputRejected()
        {
            var service = new DeadeyeService();
            Assertions.Throws<ArgumentNullException>(() => service.Evaluate(null),
                "Null Deadeye request was accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                new DeadeyeRequest(true, true, -1,
                    ProductionFirearmCatalog.CreatePistol().Definition, 1d, 1),
                "Negative marker count was accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                new DeadeyeRequest(true, true, 1,
                    ProductionFirearmCatalog.CreatePistol().Definition, 1d, -1),
                "Negative grit was accepted.");
        }

        private static DeadeyeDecision Deadeye(double distanceMeters, int grit)
        {
            return new DeadeyeService().Evaluate(new DeadeyeRequest(true, true, 1,
                ProductionFirearmCatalog.CreatePistol().Definition, distanceMeters, grit));
        }

        private static void GunslingerDodgeMoveExact()
        {
            GunslingerDodgeDecision result = Dodge(GunslingerDodgeMode.MoveFiveFeet,
                true, GunslingerDodgeArmor.Light, GunslingerDodgeLoad.Light, 1);
            Assertions.True(result.ShouldApply, "Eligible movement dodge was rejected.");
            Assertions.Equal(2, result.ArmorClassBonus, "Movement dodge AC changed.");
            Assertions.Equal(1, result.GritCost, "Movement dodge cost changed.");
            Assertions.False(result.ShouldDropProne, "Movement dodge forced prone.");
        }

        private static void GunslingerDodgeProneExact()
        {
            GunslingerDodgeDecision result = Dodge(GunslingerDodgeMode.DropProne,
                true, GunslingerDodgeArmor.Medium, GunslingerDodgeLoad.Light, 1);
            Assertions.True(result.ShouldApply, "Eligible adapted dodge was rejected.");
            Assertions.Equal(2, result.ArmorClassBonus, "Adapted dodge AC changed.");
            Assertions.False(result.ShouldDropProne, "Adapted dodge applied prone.");
        }

        private static void GunslingerDodgeRequiresRangedTrigger()
        {
            Assertions.Equal(GunslingerDodgeStatus.NotRangedAttack,
                Dodge(GunslingerDodgeMode.MoveFiveFeet, false,
                    GunslingerDodgeArmor.Light, GunslingerDodgeLoad.Light, 2).Status,
                "Melee attack activated Gunslinger's Dodge.");
        }

        private static void GunslingerDodgeArmorExact()
        {
            Assertions.Equal(GunslingerDodgeStatus.Eligible,
                Dodge(GunslingerDodgeMode.DropProne, true,
                    GunslingerDodgeArmor.None, GunslingerDodgeLoad.Light, 2).Status,
                "The explicit Kingmaker adaptation incorrectly requires armor.");
            Assertions.Equal(GunslingerDodgeStatus.Eligible,
                Dodge(GunslingerDodgeMode.DropProne, true,
                    GunslingerDodgeArmor.Heavy, GunslingerDodgeLoad.Light, 2).Status,
                "The explicit Kingmaker adaptation incorrectly rejects heavy armor.");
        }

        private static void GunslingerDodgeLoadExact()
        {
            Assertions.Equal(GunslingerDodgeStatus.Eligible,
                Dodge(GunslingerDodgeMode.MoveFiveFeet, true,
                    GunslingerDodgeArmor.Light, GunslingerDodgeLoad.Medium, 2).Status,
                "The explicit Kingmaker adaptation incorrectly rejects medium load.");
        }

        private static void GunslingerDodgeInsufficientAtomic()
        {
            GunslingerDodgeDecision result = Dodge(GunslingerDodgeMode.DropProne,
                true, GunslingerDodgeArmor.Light, GunslingerDodgeLoad.Light, 0);
            Assertions.Equal(GunslingerDodgeStatus.InsufficientGrit, result.Status,
                "Zero grit activated Gunslinger's Dodge.");
            Assertions.Equal(0, result.GritCost, "Rejected dodge exposed a cost.");
            Assertions.Equal(0, result.ArmorClassBonus, "Rejected dodge exposed AC.");
        }

        private static void GunslingerDodgeInvalidInput()
        {
            var service = new GunslingerDodgeService();
            Assertions.Throws<ArgumentNullException>(() => service.Evaluate(null),
                "Null dodge request was accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                new GunslingerDodgeRequest(true, GunslingerDodgeMode.Unknown, true,
                    GunslingerDodgeArmor.Light, GunslingerDodgeLoad.Light, 1),
                "Unknown dodge mode was accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                new GunslingerDodgeRequest(true, GunslingerDodgeMode.MoveFiveFeet, true,
                    GunslingerDodgeArmor.Light, GunslingerDodgeLoad.Light, -1),
                "Negative dodge grit was accepted.");
        }

        private static void GunslingerDodgeAlreadyProneRejected()
        {
            GunslingerDodgeDecision result = new GunslingerDodgeService().Evaluate(
                new GunslingerDodgeRequest(true, GunslingerDodgeMode.DropProne,
                    true, GunslingerDodgeArmor.Light, GunslingerDodgeLoad.Light,
                    2, false));
            Assertions.Equal(GunslingerDodgeStatus.Eligible, result.Status,
                "Existing prone state incorrectly blocked the no-prone adaptation.");
            Assertions.Equal(1, result.GritCost,
                "Eligible adapted reaction lost its grit cost.");
            Assertions.False(result.ShouldDropProne,
                "Adapted reaction attempted to add another prone state.");
        }

        private static GunslingerDodgeDecision Dodge(GunslingerDodgeMode mode,
            bool ranged, GunslingerDodgeArmor armor, GunslingerDodgeLoad load,
            int grit)
        {
            return new GunslingerDodgeService().Evaluate(new GunslingerDodgeRequest(
                true, mode, ranged, armor, load, grit));
        }

        private static void QuickClearStandardExact()
        {
            QuickClearDecision result = QuickClear(QuickClearMode.Standard, true,
                FirearmCondition.Broken, true, 1);
            Assertions.True(result.ShouldRepair, "Eligible standard Quick Clear was rejected.");
            Assertions.Equal(0, result.GritCost, "Standard Quick Clear spent grit.");
        }

        private static void QuickClearMoveExact()
        {
            QuickClearDecision result = QuickClear(QuickClearMode.Move, true,
                FirearmCondition.Broken, true, 1);
            Assertions.True(result.ShouldRepair, "Eligible move Quick Clear was rejected.");
            Assertions.Equal(1, result.GritCost, "Move Quick Clear cost changed.");
        }

        private static void QuickClearGritRequired()
        {
            foreach (QuickClearMode mode in new[] { QuickClearMode.Standard, QuickClearMode.Move })
            {
                QuickClearDecision result = QuickClear(mode, true,
                    FirearmCondition.Broken, true, 0);
                Assertions.Equal(QuickClearStatus.InsufficientGrit, result.Status,
                    "Zero grit activated Quick Clear.");
                Assertions.Equal(0, result.GritCost, "Rejected Quick Clear exposed a cost.");
            }
        }

        private static void QuickClearContextFailsClosed()
        {
            Assertions.Equal(QuickClearStatus.NotExactEquippedFirearm,
                QuickClear(QuickClearMode.Standard, false, FirearmCondition.Broken,
                    true, 1).Status, "Ambiguous firearm activated Quick Clear.");
            Assertions.Equal(QuickClearStatus.NotBroken,
                QuickClear(QuickClearMode.Standard, true, FirearmCondition.Normal,
                    true, 1).Status, "Normal firearm activated Quick Clear.");
            Assertions.Equal(QuickClearStatus.NotMisfireBroken,
                QuickClear(QuickClearMode.Standard, true, FirearmCondition.Broken,
                    false, 1).Status, "Non-misfire break activated Quick Clear.");
        }

        private static void QuickClearInvalidInput()
        {
            var service = new QuickClearService();
            Assertions.Throws<ArgumentNullException>(() => service.Evaluate(null),
                "Null Quick Clear request was accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                new QuickClearRequest(QuickClearMode.Unknown, true,
                    FirearmCondition.Broken, true, 1), "Unknown mode was accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                new QuickClearRequest(QuickClearMode.Standard, true,
                    FirearmCondition.Broken, true, -1), "Negative grit was accepted.");
        }

        private static QuickClearDecision QuickClear(QuickClearMode mode,
            bool exact, FirearmCondition condition, bool misfire, int grit)
        {
            return new QuickClearService().Evaluate(new QuickClearRequest(mode,
                exact, condition, misfire, grit));
        }

        private static void PistolWhipHandednessExact()
        {
            PistolWhipDecision one = PistolWhip(true, false,
                FirearmCondition.Normal, 1);
            PistolWhipDecision two = PistolWhip(true, true,
                FirearmCondition.Broken, 2);
            Assertions.True(one.ShouldAttack && two.ShouldAttack,
                "Eligible Pistol-Whip was rejected.");
            Assertions.Equal(6, one.DamageDieSides,
                "One-handed Pistol-Whip damage changed.");
            Assertions.Equal(10, two.DamageDieSides,
                "Two-handed Pistol-Whip damage changed.");
            Assertions.Equal(1, one.GritCost, "Pistol-Whip grit cost changed.");
        }

        private static void PistolWhipContextFailsClosed()
        {
            Assertions.Equal(PistolWhipStatus.NotExactEquippedFirearm,
                PistolWhip(false, false, FirearmCondition.Normal, 2).Status,
                "Ambiguous firearm activated Pistol-Whip.");
            Assertions.Equal(PistolWhipStatus.Wrecked,
                PistolWhip(true, true, FirearmCondition.Wrecked, 2).Status,
                "Wrecked firearm activated Pistol-Whip.");
        }

        private static void PistolWhipInsufficientAtomic()
        {
            PistolWhipDecision result = PistolWhip(true, false,
                FirearmCondition.Normal, 0);
            Assertions.Equal(PistolWhipStatus.InsufficientGrit, result.Status,
                "Zero grit activated Pistol-Whip.");
            Assertions.Equal(0, result.GritCost,
                "Rejected Pistol-Whip exposed a partial cost.");
            Assertions.Equal(0, result.DamageDieSides,
                "Rejected Pistol-Whip exposed damage.");
        }

        private static void PistolWhipInvalidInput()
        {
            var service = new PistolWhipService();
            Assertions.Throws<ArgumentNullException>(() => service.Evaluate(null),
                "Null Pistol-Whip request was accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                new PistolWhipRequest(true, false, (FirearmCondition)99, 1),
                "Unknown firearm condition was accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                new PistolWhipRequest(true, false, FirearmCondition.Normal, -1),
                "Negative Pistol-Whip grit was accepted.");
        }

        private static PistolWhipDecision PistolWhip(bool exact, bool twoHanded,
            FirearmCondition condition, int grit)
        {
            return new PistolWhipService().Evaluate(new PistolWhipRequest(
                exact, twoHanded, condition, grit));
        }

        private static void StopBleedingEligible()
        {
            StopBleedingDecision result = StopBleeding(true,
                FirearmCondition.Broken, 2, 1, 5d * 0.3048d, 2);
            Assertions.Equal(StopBleedingStatus.Eligible, result.Status,
                "Loaded Broken firearm did not support Stop Bleeding.");
            Assertions.Equal(1, result.RoundsConsumed,
                "Stop Bleeding did not consume exactly one chamber.");
        }

        private static void StopBleedingContextFailsClosed()
        {
            Assertions.Equal(StopBleedingStatus.NotExactEquippedFirearm,
                StopBleeding(false, FirearmCondition.Normal, 1, 1, 0d, 1).Status,
                "Ambiguous firearm activated Stop Bleeding.");
            Assertions.Equal(StopBleedingStatus.Wrecked,
                StopBleeding(true, FirearmCondition.Wrecked, 0, 1, 0d, 1).Status,
                "Wrecked firearm activated Stop Bleeding.");
            Assertions.Equal(StopBleedingStatus.OutOfRange,
                StopBleeding(true, FirearmCondition.Normal, 1, 1,
                    (5d * 0.3048d) + 0.002d, 1).Status,
                "Distant target activated Stop Bleeding.");
        }

        private static void StopBleedingRejectionsAtomic()
        {
            Assertions.Equal(0, StopBleeding(true, FirearmCondition.Normal,
                0, 1, 0d, 1).RoundsConsumed,
                "Empty rejection exposed a partial discharge.");
            Assertions.Equal(StopBleedingStatus.InsufficientGrit,
                StopBleeding(true, FirearmCondition.Normal, 1, 0, 0d, 1).Status,
                "Zero grit activated Stop Bleeding.");
            Assertions.Equal(StopBleedingStatus.NoBleed,
                StopBleeding(true, FirearmCondition.Normal, 1, 1, 0d, 0).Status,
                "Bleed-free target activated Stop Bleeding.");
        }

        private static void StopBleedingInvalidInput()
        {
            var service = new StopBleedingService();
            Assertions.Throws<ArgumentNullException>(() => service.Evaluate(null),
                "Null Stop Bleeding request was accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                new StopBleedingRequest(true, FirearmCondition.Normal, -1, 1, 0d, 1),
                "Negative loaded rounds were accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                new StopBleedingRequest(true, FirearmCondition.Normal, 1, 1,
                    double.NaN, 1), "Invalid distance was accepted.");
        }

        private static StopBleedingDecision StopBleeding(bool exact,
            FirearmCondition condition, int loadedRounds, int grit,
            double distanceMeters, int bleedCount)
        {
            return new StopBleedingService().Evaluate(new StopBleedingRequest(
                exact, condition, loadedRounds, grit, distanceMeters, bleedCount));
        }

        private static void NimbleExactLevels()
        {
            var service = new NimbleService();
            int[] levels = { 2, 6, 10, 14, 18, 20 };
            int[] expected = { 1, 2, 3, 4, 5, 5 };
            for (int index = 0; index < levels.Length; index++)
                Assertions.Equal(expected[index], service.CalculateBonus(levels[index],
                    NimbleArmor.Light, true), "Nimble exact level progression changed.");
        }

        private static void NimbleBetweenLevels()
        {
            var service = new NimbleService();
            Assertions.Equal(0, service.CalculateBonus(1, NimbleArmor.None, true),
                "Nimble activated before level two.");
            Assertions.Equal(1, service.CalculateBonus(5, NimbleArmor.None, true),
                "Nimble increased before level six.");
            Assertions.Equal(4, service.CalculateBonus(17, NimbleArmor.Light, true),
                "Nimble increased before level eighteen.");
        }

        private static void NimbleArmorGate()
        {
            var service = new NimbleService();
            Assertions.Equal(3, service.CalculateBonus(10, NimbleArmor.None, true),
                "No-armor Nimble was rejected.");
            Assertions.Equal(0, service.CalculateBonus(10, NimbleArmor.Medium, true),
                "Medium armor retained Nimble.");
            Assertions.Equal(0, service.CalculateBonus(10, NimbleArmor.Heavy, true),
                "Heavy armor retained Nimble.");
        }

        private static void NimbleDexterityLoss()
        {
            Assertions.Equal(0, new NimbleService().CalculateBonus(18,
                NimbleArmor.Light, false), "Nimble survived Dexterity AC loss.");
        }

        private static void NimbleInvalidInput()
        {
            var service = new NimbleService();
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                service.CalculateBonus(-1, NimbleArmor.Light, true),
                "Negative Gunslinger level was accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                service.CalculateBonus(2, NimbleArmor.Unknown, true),
                "Unknown Nimble armor was accepted.");
        }
    }
}
