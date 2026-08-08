using System;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Gunsmithing;
using KingmakerGunslinger.Classes;
using KingmakerGunslinger.Reloading;
using KingmakerGunslinger.Rules;
using KingmakerGunslinger.Deeds;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ArchetypeFoundationTests
    {
        internal static void HandednessCatalogExact()
        {
            Assertions.Equal(FirearmHandedness.OneHanded,
                FirearmHandednessPolicy.Require(FirearmKind.Pistol),
                "Pistol family changed.");
            Assertions.Equal(FirearmHandedness.OneHanded,
                FirearmHandednessPolicy.Require(FirearmKind.Revolver),
                "Revolver family changed.");
            Assertions.Equal(FirearmHandedness.TwoHanded,
                FirearmHandednessPolicy.Require(FirearmKind.Musket),
                "Musket family changed.");
            Assertions.Equal(FirearmHandedness.TwoHanded,
                FirearmHandednessPolicy.Require(FirearmKind.Blunderbuss),
                "Blunderbuss family changed.");
            Assertions.Equal(FirearmHandedness.TwoHanded,
                FirearmHandednessPolicy.Require(FirearmKind.Rifle),
                "Rifle family changed.");
        }

        internal static void HandednessFamilyMatching()
        {
            Assertions.True(FirearmHandednessPolicy.Matches(FirearmKind.Pistol,
                FirearmHandedness.OneHanded), "Pistol did not match one-handed scope.");
            Assertions.True(FirearmHandednessPolicy.Matches(FirearmKind.Revolver,
                FirearmHandedness.OneHanded), "Revolver did not match one-handed scope.");
            Assertions.False(FirearmHandednessPolicy.Matches(FirearmKind.Musket,
                FirearmHandedness.OneHanded), "Musket leaked into one-handed scope.");
            Assertions.True(FirearmHandednessPolicy.Matches(FirearmKind.Musket,
                FirearmHandedness.TwoHanded), "Musket did not match two-handed scope.");
            Assertions.True(FirearmHandednessPolicy.Matches(FirearmKind.Blunderbuss,
                FirearmHandedness.TwoHanded), "Blunderbuss did not match two-handed scope.");
            Assertions.True(FirearmHandednessPolicy.Matches(FirearmKind.Rifle,
                FirearmHandedness.TwoHanded), "Rifle did not match two-handed scope.");
            Assertions.False(FirearmHandednessPolicy.Matches(FirearmKind.Revolver,
                FirearmHandedness.TwoHanded), "Revolver leaked into two-handed scope.");
        }

        internal static void HandednessUnknownFailsClosed()
        {
            FirearmHandedness actual;
            Assertions.False(FirearmHandednessPolicy.TryGet(FirearmKind.Unknown,
                out actual), "Unknown kind received a family.");
            Assertions.Equal(FirearmHandedness.Unknown, actual,
                "Unknown kind returned a non-fail-closed family.");
            Assertions.False(FirearmHandednessPolicy.Matches(FirearmKind.Unknown,
                FirearmHandedness.OneHanded), "Unknown kind matched one-handed scope.");
            Assertions.False(FirearmHandednessPolicy.Matches(FirearmKind.Unknown,
                FirearmHandedness.TwoHanded), "Unknown kind matched two-handed scope.");
            Assertions.False(FirearmHandednessPolicy.Matches(FirearmKind.Pistol,
                FirearmHandedness.Unknown), "Unknown scope matched a firearm.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                FirearmHandednessPolicy.Require(FirearmKind.Unknown),
                "Unknown kind did not fail closed.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                FirearmHandednessPolicy.Require((FirearmKind)999),
                "Undefined kind did not fail closed.");
        }

        internal static void FullProficiencyPermitsAll()
        {
            foreach (FirearmKind kind in new[] { FirearmKind.Pistol,
                FirearmKind.Revolver, FirearmKind.Musket,
                FirearmKind.Blunderbuss, FirearmKind.Rifle })
                Assertions.True(FirearmProficiencyPolicy.CanUse(1, kind,
                    true, false, false), "Full proficiency rejected " + kind + ".");
        }

        internal static void OneHandedProficiencyExact()
        {
            Assertions.True(FirearmProficiencyPolicy.CanUse(1, FirearmKind.Pistol,
                false, true, false), "Pistolero scope rejected Pistol.");
            Assertions.True(FirearmProficiencyPolicy.CanUse(1, FirearmKind.Revolver,
                false, true, false), "Pistolero scope rejected Revolver.");
            foreach (FirearmKind kind in new[] { FirearmKind.Musket,
                FirearmKind.Blunderbuss, FirearmKind.Rifle })
                Assertions.False(FirearmProficiencyPolicy.CanUse(1, kind,
                    false, true, false), "Pistolero scope admitted " + kind + ".");
        }

        internal static void TwoHandedProficiencyExact()
        {
            foreach (FirearmKind kind in new[] { FirearmKind.Musket,
                FirearmKind.Blunderbuss, FirearmKind.Rifle })
                Assertions.True(FirearmProficiencyPolicy.CanUse(1, kind,
                    false, false, true), "Musket Master scope rejected " + kind + ".");
            Assertions.False(FirearmProficiencyPolicy.CanUse(1, FirearmKind.Pistol,
                false, false, true), "Musket Master scope admitted Pistol.");
            Assertions.False(FirearmProficiencyPolicy.CanUse(1, FirearmKind.Revolver,
                false, false, true), "Musket Master scope admitted Revolver.");
        }

        internal static void ProficiencyFailsClosed()
        {
            Assertions.False(FirearmProficiencyPolicy.CanUse(1, FirearmKind.Pistol,
                false, false, false), "Absent proficiency admitted a firearm.");
            Assertions.False(FirearmProficiencyPolicy.CanUse(0, FirearmKind.Pistol,
                true, true, true), "Missing marker admitted a firearm.");
            Assertions.False(FirearmProficiencyPolicy.CanUse(2, FirearmKind.Pistol,
                true, true, true), "Multiple markers admitted a firearm.");
            Assertions.False(FirearmProficiencyPolicy.CanUse(1, FirearmKind.Unknown,
                true, true, true), "Unknown kind was admitted.");
            Assertions.False(FirearmProficiencyPolicy.CanUse(1, (FirearmKind)999,
                true, true, true), "Undefined kind was admitted.");
        }

        internal static void ScopedActionAccess()
        {
            Assertions.False(FirearmProficiencyPolicy.GrantsScatter(
                FirearmHandedness.OneHanded),
                "One-handed proficiency incorrectly grants Scatter Shot.");
            Assertions.True(FirearmProficiencyPolicy.GrantsScatter(
                FirearmHandedness.TwoHanded),
                "Two-handed proficiency did not grant Scatter Shot.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                FirearmProficiencyPolicy.GrantsScatter(FirearmHandedness.Unknown),
                "Unknown proficiency scope did not fail closed.");
        }

        internal static void ExoticWeaponProficiencySelection()
        {
            Assertions.False(FirearmProficiencyPolicy.
                CanSelectExoticWeaponProficiency(0, false),
                "EWP ignored its BAB +1 prerequisite.");
            Assertions.True(FirearmProficiencyPolicy.
                CanSelectExoticWeaponProficiency(1, false),
                "EWP rejected an eligible scoped-proficiency owner.");
            Assertions.True(FirearmProficiencyPolicy.
                CanSelectExoticWeaponProficiency(20, false),
                "EWP rejected an eligible high-BAB owner.");
            Assertions.False(FirearmProficiencyPolicy.
                CanSelectExoticWeaponProficiency(1, true),
                "EWP admitted an owner who already has full proficiency.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                FirearmProficiencyPolicy.CanSelectExoticWeaponProficiency(-1,
                    false), "Negative BAB did not fail closed.");
        }

        internal static void StartingFirearmPrecedence()
        {
            Assertions.Equal(StartingFirearmProfile.BaseDefault,
                StartingFirearmPolicy.Resolve(false, false, false),
                "Base Gunslinger default no longer resolves to Pistol.");
            Assertions.Equal(StartingFirearmProfile.Pistolero,
                StartingFirearmPolicy.Resolve(false, true, true),
                "Pistolero did not override an explicit base Musket signal.");
            Assertions.Equal(StartingFirearmProfile.MusketMaster,
                StartingFirearmPolicy.Resolve(true, false, false),
                "Musket Master did not resolve to its mandatory profile.");
            Assertions.Equal(StartingFirearmProfile.MusketMaster,
                StartingFirearmPolicy.Resolve(true, true, true),
                "Musket Master lost highest starter precedence.");
            Assertions.Equal(StartingFirearmProfile.ExplicitMusket,
                StartingFirearmPolicy.Resolve(false, false, true),
                "Safe explicit base choice did not resolve to Musket.");
        }

        internal static void StartingFirearmExactKind()
        {
            Assertions.False(StartingFirearmPolicy.ExpectsMusket(
                StartingFirearmProfile.BaseDefault),
                "Base default unexpectedly selected Musket.");
            Assertions.False(StartingFirearmPolicy.ExpectsMusket(
                StartingFirearmProfile.Pistolero),
                "Pistolero unexpectedly selected Musket.");
            Assertions.True(StartingFirearmPolicy.ExpectsMusket(
                StartingFirearmProfile.MusketMaster),
                "Musket Master did not select Musket.");
            Assertions.True(StartingFirearmPolicy.ExpectsMusket(
                StartingFirearmProfile.ExplicitMusket),
                "Explicit Musket choice did not select Musket.");
        }

        internal static void TrainingThresholdsAndFamilies()
        {
            Assertions.False(FirearmTrainingPolicy.Evaluate(FirearmKind.Pistol,
                4, false, 0, 0).Eligible, "Level 4 acquired training.");
            for (int rank = 1; rank <= 4; rank++)
            {
                FirearmTrainingEntitlement pistol = FirearmTrainingPolicy.Evaluate(
                    FirearmKind.Pistol, 4, false, rank, 0);
                Assertions.Equal(4 + rank - 1, pistol.DamageBonus,
                    "Pistol Training rank scaling changed.");
                Assertions.True(pistol.ReducedBrokenMisfire,
                    "Pistol Training lost Broken misfire reduction.");
                FirearmTrainingEntitlement musket = FirearmTrainingPolicy.Evaluate(
                    FirearmKind.Musket, 4, false, 0, rank);
                Assertions.Equal(4 + rank - 1, musket.DamageBonus,
                    "Musket Training rank scaling changed.");
            }
            Assertions.False(FirearmTrainingPolicy.Evaluate(FirearmKind.Musket,
                4, false, 4, 0).Eligible,
                "Pistol Training leaked to a two-handed firearm.");
            Assertions.False(FirearmTrainingPolicy.Evaluate(FirearmKind.Revolver,
                4, false, 0, 4).Eligible,
                "Musket Training leaked to a one-handed firearm.");
        }

        internal static void TrainingOverlapAndNegativeDexterity()
        {
            FirearmTrainingEntitlement overlap = FirearmTrainingPolicy.Evaluate(
                FirearmKind.Pistol, 3, true, 4, 0);
            Assertions.Equal(6, overlap.DamageBonus,
                "Overlapping training summed or selected the lower entitlement.");
            FirearmTrainingEntitlement negative = FirearmTrainingPolicy.Evaluate(
                FirearmKind.Musket, -2, false, 0, 1);
            Assertions.Equal(-2, negative.DamageBonus,
                "Negative Dexterity was clamped or ignored.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                FirearmTrainingPolicy.Evaluate(FirearmKind.Unknown, 1,
                    false, 0, 0), "Unknown training kind did not fail closed.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                FirearmTrainingPolicy.Evaluate(FirearmKind.Pistol, 1,
                    false, 5, 0), "Invalid training rank did not fail closed.");
        }

        internal static void FastMusketReloadMatrix()
        {
            var musket = FirearmDefinitions.CreateEarlyMusket();
            var blunderbuss = FirearmDefinitions.CreateEarlyBlunderbuss();
            var rifle = FirearmDefinitions.CreateAdvancedRifle();
            Assertions.Equal(EffectiveReloadAction.Standard,
                ReloadActionEconomy.Evaluate(musket, false, true),
                "Ordinary Rapid Reload Musket changed.");
            Assertions.Equal(EffectiveReloadAction.Move,
                ReloadActionEconomy.Evaluate(musket, true, true),
                "Fast Musket plus Rapid Reload Musket changed.");
            Assertions.Equal(EffectiveReloadAction.Standard,
                ReloadActionEconomy.Evaluate(musket, true, false),
                "Fast Musket did not make Musket use the one-handed profile.");
            Assertions.Equal(EffectiveReloadAction.Standard,
                ReloadActionEconomy.Evaluate(blunderbuss, true, false),
                "Fast Musket did not apply to direct two-handed Blunderbuss reload.");
            Assertions.Equal(EffectiveReloadAction.Move,
                ReloadActionEconomy.Evaluate(blunderbuss, true, true),
                "Fast Musket/Rapid Reload Blunderbuss composition changed.");
            Assertions.Equal(EffectiveReloadAction.Move,
                ReloadActionEconomy.Evaluate(rifle, true, false),
                "Fast Musket incorrectly accelerated an already-Move rifle.");
            Assertions.Equal(EffectiveReloadAction.Free,
                ReloadActionEconomy.Evaluate(rifle, true, true),
                "Rapid Reload did not reduce Fast-Musket-eligible rifle to Free.");
            Assertions.Equal(EffectiveReloadAction.Standard,
                ReloadActionEconomy.Evaluate(FirearmDefinitions.CreateEarlyPistol(),
                    true, false), "Fast Musket affected a one-handed firearm.");
        }

        internal static void EffectiveRangeContextBoundaries()
        {
            var musket = FirearmDefinitions.CreateEarlyMusket();
            Assertions.Equal(50d, EffectiveFirearmRangePolicy.IncrementFeet(
                musket, 10), "Steady Aim did not add exactly 10 feet.");
            double distance = 45d * FirearmArmorClassService.MetersPerFoot;
            var ordinary = FirearmArmorClassService.Select(
                new FirearmArmorClassRequest(true, 1, musket, distance,
                    20, 12, 20, false, false, 0));
            var steady = FirearmArmorClassService.Select(
                new FirearmArmorClassRequest(true, 1, musket, distance,
                    20, 12, 20, false, false, 10));
            Assertions.False(ordinary.UsesTouchArmorClass,
                "Ordinary Musket incorrectly used touch AC past 40 feet.");
            Assertions.True(steady.UsesTouchArmorClass,
                "Steady Aim effective increment did not reach touch AC.");
            DeadeyeDecision ordinaryDeadeye = new DeadeyeService().Evaluate(
                new DeadeyeRequest(true, true, 1, musket, distance, 10, 0));
            DeadeyeDecision steadyDeadeye = new DeadeyeService().Evaluate(
                new DeadeyeRequest(true, true, 1, musket, distance, 10, 10));
            Assertions.Equal(2, ordinaryDeadeye.RangeIncrement,
                "Ordinary Deadeye increment changed.");
            Assertions.Equal(1, steadyDeadeye.RangeIncrement,
                "Deadeye did not consume the effective range first.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                EffectiveFirearmRangePolicy.IncrementFeet(musket, -1),
                "Negative per-attack range context did not fail closed.");
            Assertions.True(SteadyAimPolicy.IsQualifyingShot(true, 1,
                FirearmKind.Musket, false),
                "Direct Musket shot did not qualify for Steady Aim.");
            Assertions.True(SteadyAimPolicy.IsQualifyingShot(true, 1,
                FirearmKind.Blunderbuss, false),
                "Direct Blunderbuss shot did not qualify for Steady Aim.");
            Assertions.True(SteadyAimPolicy.IsQualifyingShot(true, 1,
                FirearmKind.Rifle, false),
                "Direct Rifle shot did not qualify for Steady Aim.");
            Assertions.False(SteadyAimPolicy.IsQualifyingShot(true, 1,
                FirearmKind.Blunderbuss, true),
                "Scatter cone qualified for Steady Aim.");
            Assertions.False(SteadyAimPolicy.IsQualifyingShot(true, 1,
                FirearmKind.Pistol, false),
                "One-handed shot consumed Steady Aim.");
            Assertions.False(SteadyAimPolicy.IsQualifyingShot(false, 0,
                FirearmKind.Musket, false),
                "Non-firearm attack qualified for Steady Aim.");
            Assertions.False(SteadyAimPolicy.IsQualifyingShot(true, 2,
                FirearmKind.Musket, false),
                "Ambiguous firearm markers qualified for Steady Aim.");
            Assertions.False(SteadyAimPolicy.IsQualifyingShot(true, 1,
                FirearmKind.Unknown, false),
                "Unknown firearm kind qualified for Steady Aim.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                SteadyAimPolicy.IsQualifyingShot(true, -1,
                    FirearmKind.Musket, false),
                "Negative marker count did not fail closed.");
        }

        internal static void UpCloseAndDeadlyPolicyContract()
        {
            Assertions.Equal(1, UpCloseAndDeadlyPolicy.DiceAtLevel(1),
                "Level 1 deed dice changed.");
            Assertions.Equal(2, UpCloseAndDeadlyPolicy.DiceAtLevel(5),
                "Level 5 deed dice changed.");
            Assertions.Equal(3, UpCloseAndDeadlyPolicy.DiceAtLevel(10),
                "Level 10 deed dice changed.");
            Assertions.Equal(4, UpCloseAndDeadlyPolicy.DiceAtLevel(15),
                "Level 15 deed dice changed.");
            Assertions.Equal(5, UpCloseAndDeadlyPolicy.DiceAtLevel(20),
                "Level 20 deed dice changed.");
            var hit = UpCloseAndDeadlyPolicy.Evaluate(true, 1,
                FirearmKind.Pistol, false, true, true, false, 10, 1);
            Assertions.True(hit.ConsumeMarker && hit.ApplyDamage,
                "Qualifying hit did not consume and apply.");
            Assertions.Equal(3, hit.Dice, "Qualifying hit dice changed.");
            Assertions.Equal(1f, hit.Modifier, "Hit was not full damage.");
            var miss = UpCloseAndDeadlyPolicy.Evaluate(true, 1,
                FirearmKind.Revolver, false, true, false, false, 20, 1);
            Assertions.True(miss.ConsumeMarker && miss.ApplyDamage,
                "Qualifying miss did not consume and apply.");
            Assertions.Equal(0.5f, miss.Modifier,
                "Miss did not use native half-damage semantics.");
            var immune = UpCloseAndDeadlyPolicy.Evaluate(true, 1,
                FirearmKind.Pistol, false, true, true, true, 5, 1);
            Assertions.True(immune.ConsumeMarker && !immune.ApplyDamage,
                "Precision immunity did not consume without applying.");
            var noGrit = UpCloseAndDeadlyPolicy.Evaluate(true, 1,
                FirearmKind.Pistol, false, true, true, false, 5, 0);
            Assertions.True(noGrit.ConsumeMarker && !noGrit.ApplyDamage,
                "Resolution-time grit loss did not consume without applying.");
            foreach (var rejected in new[] {
                UpCloseAndDeadlyPolicy.Evaluate(true, 1,
                    FirearmKind.Musket, false, true, true, false, 5, 1),
                UpCloseAndDeadlyPolicy.Evaluate(true, 1,
                    FirearmKind.Pistol, true, true, true, false, 5, 1),
                UpCloseAndDeadlyPolicy.Evaluate(false, 0,
                    FirearmKind.Unknown, false, true, true, false, 5, 1) })
                Assertions.False(rejected.ConsumeMarker,
                    "Nonqualifying action consumed the marker.");
            var misfire = UpCloseAndDeadlyPolicy.Evaluate(true, 1,
                FirearmKind.Pistol, false, false, false, false, 5, 1);
            Assertions.False(misfire.ConsumeMarker || misfire.ApplyDamage,
                "Failed discharge consumed or grazed the target.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                UpCloseAndDeadlyPolicy.DiceAtLevel(0),
                "Invalid class level did not fail closed.");
        }
    }
}
