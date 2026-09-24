using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Ammunition;
using KingmakerGunslinger.Classes;
using KingmakerGunslinger.FavoredClass;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Misfires;
using KingmakerGunslinger.Scatter;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>Phase 2: the Gunslinger first-party core and its third-party variants.</summary>
    internal static class FavoredClassGunslingerCoreTests
    {
        // Every Gunslinger counter, its target and exact rank capacities
        // (T = min(20, d * cap); full floor(T/d), partial T - floor(T/d)).
        internal static void EveryGunslingerCounterHasExactCapacities()
        {
            var expected = new[]
            {
                Tuple.Create(FavoredClassCatalog.EffectGrit, (string)null, 5, 15),
                Tuple.Create(FavoredClassCatalog.EffectMisfire, "Pistol", 5, 15),
                Tuple.Create(FavoredClassCatalog.EffectMisfire, "Musket", 5, 15),
                Tuple.Create(FavoredClassCatalog.EffectMisfire, "Blunderbuss", 5, 15),
                Tuple.Create(FavoredClassCatalog.EffectFirearmConfirmation, (string)null, 5, 10),
                Tuple.Create(FavoredClassCatalog.EffectPistolWhip, (string)null, 6, 14),
                Tuple.Create(FavoredClassCatalog.EffectHalflingNimble, (string)null, 2, 6),
                Tuple.Create(FavoredClassCatalog.EffectHalflingDodge, (string)null, 5, 15),
                Tuple.Create(FavoredClassCatalog.EffectDrowNimble, (string)null, 2, 10),
                Tuple.Create(FavoredClassCatalog.EffectInitiative, (string)null, 10, 10),
                Tuple.Create(FavoredClassCatalog.EffectDirtyTrickTrip, (string)null, 10, 10),
                // Phase 3 (divisor one has no partial leaf: 0 below).
                Tuple.Create(FavoredClassCatalog.EffectBombDamage, (string)null, 10, 10),
                Tuple.Create(FavoredClassCatalog.EffectFireIntimidate, (string)null, 10, 10),
                Tuple.Create(FavoredClassCatalog.EffectDemoralize, (string)null, 10, 10),
                Tuple.Create(FavoredClassCatalog.EffectBullRushDragDefense, (string)null, 20, 0),
                Tuple.Create(FavoredClassCatalog.EffectUnarmedConfirmation, (string)null, 5, 10),
                Tuple.Create(FavoredClassCatalog.EffectAquaticPenetration, (string)null, 20, 0),
                Tuple.Create(FavoredClassCatalog.EffectGrappleStunning, (string)null, 6, 14),
                // Phase 3 advanced (1/4, uncapped).
                Tuple.Create(FavoredClassCatalog.EffectPaladinAuras, (string)null, 5, 15),
                Tuple.Create(FavoredClassCatalog.EffectCompanionArmor, (string)null, 5, 15),
                Tuple.Create(FavoredClassCatalog.EffectEidolonArmor, (string)null, 5, 15),
                // Phase 4: selected bloodline powers (1/6, max +2 per power).
                Tuple.Create(FavoredClassCatalog.EffectSelectedBloodlinePower, "FireRay", 2, 10),
                Tuple.Create(FavoredClassCatalog.EffectSelectedBloodlinePower, "FireBlast", 2, 10),
                Tuple.Create(FavoredClassCatalog.EffectSelectedBloodlinePower, "AirRay", 2, 10),
                Tuple.Create(FavoredClassCatalog.EffectSelectedBloodlinePower, "AirBlast", 2, 10),
            }.Concat(
                // Selected revelations (1/6, uncapped): one counter per manifest target.
                FavoredClassRevelationManifest.All.Select(target => Tuple.Create(
                    FavoredClassCatalog.EffectSelectedRevelation, target.Key, 3, 17))).Concat(
                // Performance range (1/1, max +30 feet): one full-only counter per performance.
                FavoredClassPerformanceManifest.All.Select(target => Tuple.Create(
                    FavoredClassCatalog.EffectPerformanceRange, target.Key, 6, 0))).ToArray();
            IList<FavoredClassLeafSpec> leaves = FavoredClassLeafCatalog.AllLeaves();
            Assertions.Equal(expected.Sum(counter => counter.Item4 > 0 ? 2 : 1), leaves.Count,
                "Published counter leaf count.");
            foreach (Tuple<string, string, int, int> counter in expected)
            {
                FavoredClassLeafSpec full = leaves.Single(leaf => leaf.EffectId == counter.Item1 &&
                    leaf.TargetKey == counter.Item2 && leaf.Role == FavoredClassInvestmentRole.Full);
                FavoredClassLeafSpec partial = leaves.SingleOrDefault(leaf => leaf.EffectId == counter.Item1 &&
                    leaf.TargetKey == counter.Item2 && leaf.Role == FavoredClassInvestmentRole.Partial);
                string label = counter.Item1 + "/" + (counter.Item2 ?? "-");
                Assertions.Equal(counter.Item3, full.Ranks, label + " full capacity.");
                Assertions.Equal(counter.Item4, partial == null ? 0 : partial.Ranks, label + " partial capacity.");
                Assertions.True(full.Symbol.EndsWith(".Full", StringComparison.Ordinal) &&
                    (partial == null || partial.Symbol.EndsWith(".Partial", StringComparison.Ordinal)),
                    label + " roles.");
                if (counter.Item2 != null)
                {
                    Assertions.True(full.Symbol.Contains("." + counter.Item2 + "."),
                        label + " target is part of the stable symbol.");
                    Assertions.True(full.Description.Contains("separate count"),
                        label + " discloses its own counter.");
                    Assertions.True(full.Name.Contains(FavoredClassLeafCatalog.TargetTitle(counter.Item1,
                        counter.Item2)), label + " names its target.");
                }
            }
            Assertions.Equal(leaves.Count, leaves.Select(leaf => leaf.Symbol)
                .Distinct(StringComparer.Ordinal).Count(), "Leaf symbols are unique.");
            // Legacy Rifle/Revolver identities stay readable but are never targets.
            Assertions.False(leaves.Any(leaf => leaf.TargetKey == "Rifle" || leaf.TargetKey == "Revolver"),
                "Legacy firearm kinds are not favored-class targets.");
        }

        // Dormant investment and replacement are disclosed where they apply.
        internal static void ConditionsAreDisclosed()
        {
            Func<string, string> text = effect => FavoredClassLeafCatalog.LeavesFor(effect)
                .First(leaf => leaf.Role == FavoredClassInvestmentRole.Full).Description;
            Assertions.True(text(FavoredClassCatalog.EffectHalflingNimble).Contains("Mysterious Stranger") &&
                text(FavoredClassCatalog.EffectHalflingNimble).Contains("level 2"),
                "Nimble discloses its unlock level and the replacing archetype.");
            Assertions.True(text(FavoredClassCatalog.EffectDrowNimble).Contains("Mysterious Stranger"),
                "Drow Nimble discloses the replacing archetype.");
            Assertions.True(text(FavoredClassCatalog.EffectHalflingDodge).Contains("Musket Master"),
                "Dodge discloses the replacing archetype.");
            Assertions.True(text(FavoredClassCatalog.EffectInitiative).Contains("at least 1 grit") &&
                text(FavoredClassCatalog.EffectInitiative).Contains("3rd-level"),
                "Initiative discloses the deed's own activation.");
            Assertions.True(text(FavoredClassCatalog.EffectPistolWhip).Contains("trip") &&
                text(FavoredClassCatalog.EffectPistolWhip).Contains("damage"),
                "Pistol-Whip discloses what it never changes.");
            Assertions.True(text(FavoredClassCatalog.EffectFirearmConfirmation).Contains("Critical Focus"),
                "Confirmation discloses the nonstacking rule.");
            Assertions.True(text(FavoredClassCatalog.EffectMisfire).Contains("below 1") &&
                text(FavoredClassCatalog.EffectMisfire).Contains("stays 0"),
                "Misfire discloses its floor.");
            Assertions.True(text(FavoredClassCatalog.EffectDirtyTrickTrip).Contains("Jon Brazer Enterprises") &&
                text(FavoredClassCatalog.EffectDrowNimble).Contains("Jon Brazer Enterprises"),
                "Third-party effects attribute their publisher.");
            Assertions.True(text(FavoredClassCatalog.EffectBombDamage).Contains("never per die") &&
                text(FavoredClassCatalog.EffectBombDamage).Contains("Vivisectionist"),
                "Bomb damage discloses its scope and the replacing archetypes.");
            Assertions.True(text(FavoredClassCatalog.EffectFireIntimidate).Contains("CRPG adaptation") &&
                text(FavoredClassCatalog.EffectDemoralize).Contains("CRPG adaptation") &&
                text(FavoredClassCatalog.EffectBullRushDragDefense).Contains("CRPG adaptation"),
                "The three adaptations disclose their omitted portion.");
            string partialU04 = FavoredClassLeafCatalog.LeavesFor(FavoredClassCatalog.EffectGrappleStunning)
                .Single(leaf => leaf.Role == FavoredClassInvestmentRole.Partial).Description;
            Assertions.True(partialU04.Contains("+1 CMD against grapple at once") &&
                !partialU04.Contains("grants nothing by itself"),
                "The mixed-rate partial leaf discloses its immediate grapple defense.");
        }

        // M07: the favored-class reduction applies last, floors at 1, never
        // raises a zero, and leaves every other ordering unchanged.
        internal static void MisfireReductionAppliesLastWithItsFloor()
        {
            AmmunitionId loose = ReloadAmmunitionProfileCatalog.LooseBasic.LoadedAmmunition;
            AmmunitionId paper = ReloadAmmunitionProfileCatalog.PaperCartridge.LoadedAmmunition;
            foreach (FirearmCondition condition in new[] { FirearmCondition.Normal, FirearmCondition.Broken })
                foreach (bool trained in new[] { false, true })
                    foreach (AmmunitionId ammunition in new[] { loose, paper })
                        for (int value = 1; value <= 20; value++)
                            for (int reliable = 0; reliable <= 5; reliable++)
                            {
                                int control = EffectiveFirearmMisfireValuePolicy.Evaluate(value,
                                    condition, trained, ammunition, reliable);
                                Assertions.Equal(control, EffectiveFirearmMisfireValuePolicy.Evaluate(
                                    value, condition, trained, ammunition, reliable, 0),
                                    "A zero reduction changes nothing.");
                                for (int reduction = 1; reduction <= 5; reduction++)
                                {
                                    int reduced = EffectiveFirearmMisfireValuePolicy.Evaluate(value,
                                        condition, trained, ammunition, reliable, reduction);
                                    if (control == 0)
                                        Assertions.Equal(0, reduced, "A zero threshold stays zero.");
                                    else
                                        Assertions.True(reduced >= 1 && reduced <= control,
                                            "A positive threshold floors at 1 and never rises.");
                                }
                            }
            Assertions.Equal(1, EffectiveFirearmMisfireValuePolicy.Evaluate(
                1, FirearmCondition.Normal, false, loose, 0, 5), "Pistol base 1 stays 1.");
            Assertions.Equal(3, EffectiveFirearmMisfireValuePolicy.Evaluate(
                1, FirearmCondition.Broken, false, loose, 0, 2), "Broken 5 minus 2.");
            Assertions.Equal(1, EffectiveFirearmMisfireValuePolicy.Evaluate(
                1, FirearmCondition.Broken, false, loose, 0, 9), "Broken floors at 1.");
            Assertions.Equal(4, EffectiveFirearmMisfireValuePolicy.Evaluate(
                2, FirearmCondition.Broken, false, paper, 1, 2),
                "Broken, paper and Reliable first (6), then the favored-class reduction.");
            Assertions.Equal(0, EffectiveFirearmMisfireValuePolicy.Evaluate(
                1, FirearmCondition.Normal, false, loose, 1, 3), "Reliable's zero is kept.");
            Assertions.Equal(20, EffectiveFirearmMisfireValuePolicy.Evaluate(
                20, FirearmCondition.Broken, false, paper, 0, 1),
                "The reduction subtracts from the unclamped 21, then clamps at 20.");
            Assertions.Equal(18, EffectiveFirearmMisfireValuePolicy.Evaluate(
                20, FirearmCondition.Broken, false, paper, 0, 3), "21 - 3.");
            Assertions.Throws<ArgumentOutOfRangeException>(() => EffectiveFirearmMisfireValuePolicy.Evaluate(
                1, FirearmCondition.Normal, false, loose, 0, -1), "A negative reduction is rejected.");
        }

        // The scatter all-roll aggregate uses the effective threshold that
        // decided each native roll; zero means no roll misfires.
        internal static void ScatterAggregateUsesTheEffectiveThreshold()
        {
            FirearmDefinition blunderbuss = FirearmDefinitions.CreateEarlyBlunderbuss();
            object first = new object();
            object second = new object();
            ScatterTargetPlan plan = Plan(first, second);
            ScatterAttackRollObservation[] ones =
            {
                new ScatterAttackRollObservation(first, "a", 1, false, false, false),
                new ScatterAttackRollObservation(second, "b", 1, false, false, false)
            };
            ScatterAttackRollObservation[] fives =
            {
                new ScatterAttackRollObservation(first, "a", 5, false, false, false),
                new ScatterAttackRollObservation(second, "b", 4, false, false, false)
            };
            var service = new ScatterAttackVolleyService();
            Assertions.False(service.Evaluate(blunderbuss, plan, ones, 0).AllRollsMisfire,
                "A zero effective threshold never misfires.");
            Assertions.True(service.Evaluate(blunderbuss, plan, ones, 1).AllRollsMisfire,
                "Natural 1s misfire at threshold 1.");
            Assertions.True(service.Evaluate(blunderbuss, plan, fives, 6).AllRollsMisfire,
                "A raised (broken) threshold counts rolls above the base value.");
            Assertions.False(service.Evaluate(blunderbuss, plan, fives, 3).AllRollsMisfire,
                "A reduced threshold excludes those rolls.");
            Assertions.Equal(service.Evaluate(blunderbuss, plan, fives).MisfireRollCount,
                service.Evaluate(blunderbuss, plan, fives, blunderbuss.MisfireValue).MisfireRollCount,
                "The base-value overload is the effective overload at the base value.");
            Assertions.Throws<ArgumentOutOfRangeException>(
                () => service.Evaluate(blunderbuss, plan, ones, 21), "Threshold above 20 rejected.");
            Assertions.False(ones[0].IsMisfire(0), "An observation never misfires at threshold 0.");
        }

        // M08: the better of the earned bonus and Critical Focus applies.
        internal static void ConfirmationPreservesTheBetterBonus()
        {
            for (int earned = 0; earned <= 5; earned++)
            {
                Assertions.Equal(earned, FavoredClassMechanicsPolicy.ConfirmationContribution(earned, 0),
                    "Without Critical Focus the whole earned bonus applies.");
                int withFocus = FavoredClassMechanicsPolicy.ConfirmationContribution(earned, 4);
                Assertions.Equal(Math.Max(4, earned), 4 + withFocus,
                    "Critical Focus plus the contribution is the better of the two.");
            }
            Assertions.Equal(1, FavoredClassMechanicsPolicy.ConfirmationContribution(5, 4),
                "An earned +5 is not disabled by Critical Focus's +4.");
            Assertions.Equal(0, FavoredClassMechanicsPolicy.ConfirmationContribution(3, 4),
                "A smaller earned bonus adds nothing beside Critical Focus.");
            Assertions.Throws<ArgumentOutOfRangeException>(
                () => FavoredClassMechanicsPolicy.ConfirmationContribution(-1, 0), "Negative steps rejected.");
        }

        // M13 / golden example: +0/+1/+10 at 1/2/20 while the deed applies.
        internal static void InitiativeImprovesOnlyTheActiveDeed()
        {
            var service = new GunslingerInitiativeService();
            var rate = FavoredClassCatalog.Effect(FavoredClassCatalog.EffectInitiative).Rate;
            foreach (int investments in new[] { 1, 2, 20 })
            {
                int full = FavoredClassRankPolicy.FullRankAfter(investments, rate.Divisor);
                int steps = FavoredClassRankPolicy.BenefitSteps(rate, full);
                Assertions.Equal(investments / 2, steps, "Initiative steps at N=" + investments);
                Assertions.Equal(2 + steps, service.CalculateBonus(1, steps),
                    "With grit the deed's +2 is improved.");
                Assertions.Equal(0, service.CalculateBonus(0, steps),
                    "Without the deed's grit the favored-class steps grant nothing.");
            }
            Assertions.Equal(service.CalculateBonus(3), service.CalculateBonus(3, 0),
                "The deed alone is unchanged.");
            Assertions.Throws<ArgumentOutOfRangeException>(() => service.CalculateBonus(1, -1),
                "Negative steps rejected.");
        }

        // Structural guards for mechanics applied inside existing calculations.
        internal static void MechanicsLiveInTheAuthoritativeCalculations()
        {
            string root = Path.Combine(Environment.CurrentDirectory, "src", "KingmakerGunslinger");
            Func<string, string> read = relative => File.ReadAllText(Path.Combine(root, relative));
            foreach (string site in new[] { "Misfires/FirearmMisfireRuntime.cs",
                "Deeds/DeadShotRuntime.cs", "Scatter/ScatterShotRuntime.cs" })
                Assertions.True(read(site).Contains("FavoredClassEarnedSteps") &&
                    read(site).Contains(".MisfireReduction("),
                    site + " passes the wielder's favored-class misfire reduction.");
            Assertions.True(read("Scatter/ScatterShotRuntime.cs").Contains(
                "firearm.Definition, plan, observations, threshold);"),
                "The scatter aggregate receives the effective threshold.");
            Assertions.True(read("Deeds/GunslingerDodgeArmorClassBonus.cs").Contains(
                "Bonus + FavoredClass.Mechanics.FavoredClassEarnedSteps.DodgeBonus(Owner)"),
                "The Dodge branch augments the deed's own bounded bonus.");
            Assertions.True(read("Classes/GunslingerInitiativeBonus.cs").Contains(
                "FavoredClassEarnedSteps.InitiativeBonus(Owner)"),
                "The Initiative branch improves the deed's own bonus.");
            string nimble = read("FavoredClass/Mechanics/FavoredClassNimbleArmorClassBonus.cs");
            Assertions.True(nimble.Contains("NimbleArmorClassBonus.IsEligibleArmor(Owner)") &&
                nimble.Contains("HasFact(Nimble)") && nimble.Contains("ModifierDescriptor.Dodge"),
                "Nimble improvements share Nimble's own conditions and descriptor.");
            string blueprints = read("FavoredClass/FavoredClassBlueprints.cs");
            Assertions.True(blueprints.Contains("gunslinger.MysteriousStranger.Archetype") &&
                blueprints.Contains("gunslinger.MusketMaster.Archetype") &&
                blueprints.Contains("PrerequisiteNoArchetype"),
                "Permanently replaced features are not offered to their replacing archetype.");
            string confirmation = read("FavoredClass/Mechanics/FavoredClassFirearmConfirmationBonus.cs");
            Assertions.True(confirmation.Contains("evt.CriticalConfirmationBonus +=") &&
                !confirmation.Contains("AttackBonusPenalty") && !confirmation.Contains("CriticalEdge"),
                "Confirmation changes only the confirmation bonus.");
            string whip = read("FavoredClass/Mechanics/FavoredClassPistolWhipAttackBonus.cs");
            Assertions.True(whip.Contains("OneHandedSurrogate") && whip.Contains("TwoHandedSurrogate") &&
                !whip.Contains("RuleCalculateCMB") && !whip.Contains("Damage"),
                "Pistol-Whip changes only the deed attack.");
        }

        private static ScatterTargetPlan Plan(params object[] targets)
        {
            var candidates = new ScatterTargetCandidate[targets.Length];
            for (int index = 0; index < targets.Length; index++)
                candidates[index] = new ScatterTargetCandidate(targets[index],
                    ((char)('a' + index)).ToString(), "Target", index + 1,
                    ScatterGeometryDisposition.Inside);
            return new ScatterTargetPlanService().Build(new object(), candidates);
        }
    }
}
