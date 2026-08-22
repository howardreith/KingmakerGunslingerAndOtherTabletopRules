using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.BodyguardFeats;

namespace KingmakerGunslinger.DomainTests
{
    internal static class BodyguardPolicyTests
    {
        internal static void EligibilityRequiresEveryGate()
        {
            BodyguardEligibilityRequest valid = ValidEligibility();
            Assertions.True(BodyguardEligibilityPolicy.Evaluate(valid).Eligible,
                "Fully eligible Bodyguard request was rejected.");
            var mutations = new Action<BodyguardEligibilityRequest>[]
            {
                value => value.ModuleEnabled = false,
                value => value.HostileAttackRoll = false,
                value => value.TargetIsAlly = false,
                value => value.AttackerIsHostile = false,
                value => value.ProtectorIsAttacker = true,
                value => value.ProtectorIsTarget = true,
                value => value.HasBodyguard = false,
                value => value.BodyguardModeActive = false,
                value => value.Alive = false,
                value => value.Conscious = false,
                value => value.AbleToAct = false,
                value => value.NativeAooAllowed = false,
                value => value.AooRemaining = 0,
                value => value.ProtectorTargetEdgeDistanceFeet = 5.002d,
                value => value.ThreatensAttacker = false
            };
            BodyguardEligibilityFailure[] expected =
            {
                BodyguardEligibilityFailure.ModuleDisabled,
                BodyguardEligibilityFailure.NotHostileAttack,
                BodyguardEligibilityFailure.NotAlly,
                BodyguardEligibilityFailure.AttackerNotHostile,
                BodyguardEligibilityFailure.ProtectorIsAttacker,
                BodyguardEligibilityFailure.ProtectorIsTarget,
                BodyguardEligibilityFailure.FeatureAbsent,
                BodyguardEligibilityFailure.ModeOff,
                BodyguardEligibilityFailure.Dead,
                BodyguardEligibilityFailure.Unconscious,
                BodyguardEligibilityFailure.UnableToAct,
                BodyguardEligibilityFailure.NativeAooDenied,
                BodyguardEligibilityFailure.NoAooRemaining,
                BodyguardEligibilityFailure.NotAdjacent,
                BodyguardEligibilityFailure.AttackerNotThreatened
            };
            for (int index = 0; index < mutations.Length; index++)
            {
                BodyguardEligibilityRequest request = ValidEligibility();
                mutations[index](request);
                Assertions.Equal(expected[index],
                    BodyguardEligibilityPolicy.Evaluate(request).Failure,
                    "Bodyguard eligibility gate changed at index " + index + ".");
            }
        }

        internal static void AdjacencyBoundariesAreEdgeAware()
        {
            foreach (double edgeDistance in new[] { -2d, 0d, 4.999d, 5d, 5.001d })
            {
                BodyguardEligibilityRequest request = ValidEligibility();
                request.ProtectorTargetEdgeDistanceFeet = edgeDistance;
                bool expected = edgeDistance <= 5.001d;
                Assertions.Equal(expected,
                    BodyguardEligibilityPolicy.Evaluate(request).Eligible,
                    "Small/Medium/Large edge-distance boundary changed at " +
                    edgeDistance + ".");
            }
            foreach (double invalid in new[] { double.NaN,
                double.PositiveInfinity, double.NegativeInfinity })
            {
                BodyguardEligibilityRequest request = ValidEligibility();
                request.ProtectorTargetEdgeDistanceFeet = invalid;
                Assertions.Equal(BodyguardEligibilityFailure.NotAdjacent,
                    BodyguardEligibilityPolicy.Evaluate(request).Failure,
                    "Non-finite adjacency did not fail closed.");
            }
        }

        internal static void ThreatAndNativeAooRemainAuthoritative()
        {
            BodyguardEligibilityRequest rangedDistant = ValidEligibility();
            rangedDistant.ThreatensAttacker = false;
            Assertions.Equal(BodyguardEligibilityFailure.AttackerNotThreatened,
                BodyguardEligibilityPolicy.Evaluate(rangedDistant).Failure,
                "A distant ranged attacker became Bodyguard-eligible from ally adjacency alone.");
            BodyguardEligibilityRequest rangedNear = ValidEligibility();
            Assertions.True(BodyguardEligibilityPolicy.Evaluate(rangedNear).Eligible,
                "A ranged attacker inside native melee threat was rejected.");
            BodyguardEligibilityRequest threatOnly = ValidEligibility();
            threatOnly.ProtectorTargetEdgeDistanceFeet = 5.1d;
            Assertions.Equal(BodyguardEligibilityFailure.NotAdjacent,
                BodyguardEligibilityPolicy.Evaluate(threatOnly).Failure,
                "Threat without ally adjacency was accepted.");
            BodyguardEligibilityRequest flatFootedDenied = ValidEligibility();
            flatFootedDenied.NativeAooAllowed = false;
            Assertions.Equal(BodyguardEligibilityFailure.NativeAooDenied,
                BodyguardEligibilityPolicy.Evaluate(flatFootedDenied).Failure,
                "Native flat-footed AoO denial was overridden.");
            BodyguardEligibilityRequest combatReflexesAllowed = ValidEligibility();
            combatReflexesAllowed.NativeAooAllowed = true;
            Assertions.True(BodyguardEligibilityPolicy.Evaluate(
                combatReflexesAllowed).Eligible,
                "Native Combat Reflexes flat-footed permission was overridden.");
        }

        internal static void AidAnotherNaturalAndTotalRulesAreExact()
        {
            Assertions.True(!new BodyguardAidResult("p", 10, -1).Success,
                "Aid total 9 must fail.");
            Assertions.True(new BodyguardAidResult("p", 10, 0).Success,
                "Aid total 10 must succeed.");
            Assertions.True(!new BodyguardAidResult("p", 1, 99).Success,
                "Natural 1 must fail regardless of total.");
            Assertions.True(new BodyguardAidResult("p", 20, -99).Success,
                "Natural 20 must succeed regardless of total.");
            Assertions.Equal(10, BodyguardAidPolicy.TargetArmorClass,
                "Aid Another target AC changed.");
            Assertions.Equal(2, BodyguardAidPolicy.SuccessArmorClassBonus,
                "Bodyguard AC contribution changed.");
        }

        internal static void ResourceSpendPrecedesAndControlsAidRoll()
        {
            var order = new List<string>();
            BodyguardAttemptExecution success =
                BodyguardAttemptCoordinator.Execute("protector", 4,
                    () => { order.Add("spend"); return true; },
                    () => { order.Add("roll"); return 6; });
            Assertions.True(order.SequenceEqual(new[] { "spend", "roll" }),
                "Aid result was inspected before native AoO expenditure.");
            Assertions.True(success.Spent && success.RollAttempted &&
                success.Result != null && success.Result.Success,
                "Committed AoO did not produce the exact Aid total 10 result.");

            int deniedRolls = 0;
            BodyguardAttemptExecution denied =
                BodyguardAttemptCoordinator.Execute("protector", 99,
                    () => false, () => { deniedRolls++; return 20; });
            Assertions.True(!denied.Spent && !denied.RollAttempted &&
                denied.Result == null && deniedRolls == 0,
                "A failed native AoO spend still rolled Aid Another.");

            BodyguardAttemptExecution failedAid =
                BodyguardAttemptCoordinator.Execute("protector", 0,
                    () => true, () => 9);
            Assertions.True(failedAid.Spent && failedAid.RollAttempted &&
                failedAid.Result != null && !failedAid.Result.Success,
                "A failed Aid check did not retain its committed AoO spend.");

            BodyguardAttemptExecution fault =
                BodyguardAttemptCoordinator.Execute("protector", 0,
                    () => true, () => { throw new InvalidOperationException(
                        "fixture"); });
            Assertions.True(fault.Spent && fault.RollAttempted &&
                fault.Result == null && fault.Fault is InvalidOperationException,
                "An Aid-roll exception incorrectly refunded or erased the spend.");
        }

        internal static void AidStackingAndOneAttemptAreExact()
        {
            BodyguardAidResult[] attempts =
            {
                new BodyguardAidResult("a", 10, 0),
                new BodyguardAidResult("b", 1, 99),
                new BodyguardAidResult("c", 20, -99)
            };
            Assertions.Equal(4, BodyguardAidPolicy.StackArmorClassBonus(attempts),
                "Two distinct successful Bodyguards must stack to +4.");
            Assertions.Equal(0, BodyguardAidPolicy.StackArmorClassBonus(
                Array.Empty<BodyguardAidResult>()), "Zero successes must add +0.");
            Assertions.Equal(6, BodyguardAidPolicy.StackArmorClassBonus(new[]
            {
                new BodyguardAidResult("a", 10, 0),
                new BodyguardAidResult("b", 10, 0),
                new BodyguardAidResult("c", 10, 0)
            }), "Three distinct successes must stack to +6.");
            bool duplicateRejected = false;
            try
            {
                BodyguardAidPolicy.StackArmorClassBonus(new[]
                {
                    new BodyguardAidResult("a", 10, 0),
                    new BodyguardAidResult("a", 20, 0)
                });
            }
            catch (InvalidOperationException) { duplicateRejected = true; }
            Assertions.True(duplicateRejected,
                "One protector contributed more than +2 to one attack.");
        }

        internal static void ArmorClassAttributionIsTruthfulAndScoped()
        {
            BodyguardArmorClassAttributionPlan one =
                BodyguardArmorClassAttributionPolicy.Create(13, new[]
                {
                    new BodyguardAidResult("protector-one", 10, 0)
                });
            Assertions.Equal(13, one.NativeArmorClass,
                "Bodyguard changed the native AC baseline.");
            Assertions.Equal(2, one.TotalBonus,
                "One successful Bodyguard did not contribute exactly +2.");
            Assertions.Equal(15, one.FinalArmorClass,
                "The observed 13 AC case no longer resolves to 15.");
            Assertions.Equal(1, one.Contributions.Count,
                "One success did not produce one truthful source entry.");
            Assertions.Equal("protector-one",
                one.Contributions[0].ProtectorId,
                "The Bodyguard source lost its protector correlation.");
            Assertions.Equal(2, one.Contributions[0].Bonus,
                "The Bodyguard source value changed.");

            BodyguardArmorClassAttributionPlan two =
                BodyguardArmorClassAttributionPolicy.Create(13, new[]
                {
                    new BodyguardAidResult("protector-one", 20, -99),
                    new BodyguardAidResult("protector-two", 10, 0)
                });
            Assertions.Equal(4, two.TotalBonus,
                "Two successful Bodyguards did not stack to +4.");
            Assertions.Equal(17, two.FinalArmorClass,
                "Two successful Bodyguards double-counted or lost AC.");
            Assertions.True(two.Contributions.Count == 2 &&
                two.Contributions.All(value => value.Bonus == 2),
                "Two successful protectors lack two truthful +2 sources.");

            BodyguardArmorClassAttributionPlan failure =
                BodyguardArmorClassAttributionPolicy.Create(13, new[]
                {
                    new BodyguardAidResult("protector-one", 1, 99)
                });
            Assertions.True(failure.TotalBonus == 0 &&
                failure.FinalArmorClass == 13 &&
                failure.Contributions.Count == 0,
                "A failed Aid attempt produced AC or source attribution.");

            BodyguardArmorClassAttributionPlan firearmTouch =
                BodyguardArmorClassAttributionPolicy.Create(11, new[]
                {
                    new BodyguardAidResult("protector-one", 10, 0)
                });
            Assertions.True(firearmTouch.NativeArmorClass == 11 &&
                firearmTouch.TotalBonus == 2 &&
                firearmTouch.FinalArmorClass == 13,
                "Bodyguard did not preserve the already-selected firearm touch AC baseline.");
        }

        internal static void AttackSelectionIsPreRollAndStable()
        {
            var low = new object();
            var highLate = new object();
            var highEarly = new object();
            BodyguardAttackCandidate<object>[] candidates =
            {
                new BodyguardAttackCandidate<object>(highLate, "z", 12, 3, true),
                new BodyguardAttackCandidate<object>(low, "a", 8, 0, true),
                new BodyguardAttackCandidate<object>(highEarly, "b", 12, 1, true)
            };
            Assertions.True(ReferenceEquals(highEarly,
                BodyguardAttackSelectionPolicy.Select(candidates).Attack),
                "Highest target-aware bonus/stable slot did not win.");
            Assertions.True(ReferenceEquals(highEarly,
                BodyguardAttackSelectionPolicy.Select(candidates.Reverse()).Attack),
                "Input enumeration order changed the qualifying attack.");
            BodyguardAttackCandidate<object> tieA =
                new BodyguardAttackCandidate<object>(low, "a", 12, 1, true);
            BodyguardAttackCandidate<object> tieB =
                new BodyguardAttackCandidate<object>(highLate, "b", 12, 1, true);
            Assertions.True(ReferenceEquals(low,
                BodyguardAttackSelectionPolicy.Select(new[] { tieB, tieA }).Attack),
                "Persistent attack identity did not break an exact slot tie.");
            Assertions.True(BodyguardAttackSelectionPolicy.Select(new[]
            {
                new BodyguardAttackCandidate<object>(low, "a", 99, 0, false)
            }) == null, "A nonthreatening attack was selected.");
        }

        internal static void AttackFramePreauthorizesAndDeduplicates()
        {
            var frame = new BodyguardAttackFrame("attack", "enemy", "ally");
            Assertions.True(frame.TryRecordAttempt(new BodyguardAidResult(
                "protector", 10, 0)), "First Bodyguard attempt was rejected.");
            Assertions.True(!frame.TryRecordAttempt(new BodyguardAidResult(
                "protector", 20, 0)), "Duplicate callback rerolled Bodyguard.");
            frame.FinishBodyguard();
            object ac = new object();
            bool firstArmorClassCallback = frame.TryApplyArmorClass(ac);
            BodyguardArmorClassAttributionPlan applied =
                BodyguardArmorClassAttributionPolicy.Create(13, frame.Attempts);
            bool duplicateArmorClassCallback = frame.TryApplyArmorClass(ac);
            int attributedSourceCount = firstArmorClassCallback ?
                applied.Contributions.Count : 0;
            int attributedArmorClass = firstArmorClassCallback ?
                applied.FinalArmorClass : 13;
            Assertions.True(firstArmorClassCallback &&
                !duplicateArmorClassCallback,
                "Duplicate AC callback duplicated a Bodyguard contribution.");
            Assertions.True(attributedSourceCount == 1 &&
                attributedArmorClass == 15,
                "Duplicate AC callback changed either the AC total or source count.");
            Assertions.Equal(2, frame.ArmorClassBonus,
                "Frame Bodyguard contribution changed.");
            Assertions.True(frame.TryResolveAttack(false),
                "Attack result could not be recorded after preauthorization.");
            Assertions.True(!frame.TryResolveAttack(true),
                "Duplicate attack-resolution callback changed the result.");
            frame.Complete();
            Assertions.Equal(BodyguardAttackStage.Completed, frame.Stage,
                "Completed frame did not terminate.");

            var overwhelmingHit = new BodyguardAttackFrame("attack-2", "enemy", "ally");
            overwhelmingHit.TryRecordAttempt(new BodyguardAidResult("protector", 1, 99));
            overwhelmingHit.FinishBodyguard();
            overwhelmingHit.TryResolveAttack(true);
            Assertions.Equal(1, overwhelmingHit.Attempts.Count,
                "Bodyguard was not spent when its bonus could not prevent a hit.");
            var alreadyMiss = new BodyguardAttackFrame("attack-3", "enemy", "ally");
            alreadyMiss.TryRecordAttempt(new BodyguardAidResult("protector", 10, 0));
            alreadyMiss.FinishBodyguard();
            alreadyMiss.TryResolveAttack(false);
            Assertions.Equal(1, alreadyMiss.Attempts.Count,
                "Bodyguard was not spent when the attack already missed.");
        }

        internal static void AttackFramesRemainNestedAndFaultSafe()
        {
            var parent = new BodyguardAttackFrame("parent", "enemy", "ally");
            var synthetic = new BodyguardAttackFrame("synthetic", "protector", "enemy");
            parent.TryRecordAttempt(new BodyguardAidResult("p", 10, 0));
            synthetic.TryRecordAttempt(new BodyguardAidResult("q", 1, 0));
            Assertions.Equal(2, parent.ArmorClassBonus,
                "Nested synthetic frame corrupted its parent.");
            Assertions.Equal(0, synthetic.ArmorClassBonus,
                "Nested frame reused its parent's AC contribution.");
            parent.FinishBodyguard();
            parent.TryResolveAttack(true);
            Assertions.True(parent.TryIntercept("p"),
                "Successful parent Bodyguard could not intercept.");
            parent.Fault();
            Assertions.Equal("ally", parent.FinalTargetId,
                "Fault cleanup did not restore the original target.");
            Assertions.True(parent.InterceptorId == null,
                "Fault cleanup retained an interceptor.");
        }

        internal static void InterceptorEligibilityGatesAreExact()
        {
            BodyguardInterceptorCandidate valid = Candidate("p", 0);
            Assertions.Equal(1, BodyguardInterceptionPolicy.OrderEligible(true, true,
                false, new[] { valid }).Length,
                "One eligible interceptor was not selected.");
            Assertions.Equal(0, BodyguardInterceptionPolicy.OrderEligible(false, true,
                false, new[] { valid }).Length, "Module OFF intercepted.");
            Assertions.Equal(0, BodyguardInterceptionPolicy.OrderEligible(true, false,
                false, new[] { valid }).Length, "A missed attack intercepted.");
            Assertions.Equal(0, BodyguardInterceptionPolicy.OrderEligible(true, true,
                true, new[] { valid }).Length, "An already-intercepted attack chained.");
            var gates = new[]
            {
                new BodyguardInterceptorCandidate("p", 0, false, true, true, true, true),
                new BodyguardInterceptorCandidate("p", 0, true, false, true, true, true),
                new BodyguardInterceptorCandidate("p", 0, true, true, false, true, true),
                new BodyguardInterceptorCandidate("p", 0, true, true, true, false, true),
                new BodyguardInterceptorCandidate("p", 0, true, true, true, true, false)
            };
            foreach (BodyguardInterceptorCandidate candidate in gates)
                Assertions.Equal(0, BodyguardInterceptionPolicy.OrderEligible(true,
                    true, false, new[] { candidate }).Length,
                    "An In Harm's Way eligibility gate was bypassed.");
        }

        internal static void InterceptorOrderingIsStable()
        {
            BodyguardInterceptorCandidate partySecond = Candidate("a", 1);
            BodyguardInterceptorCandidate partyFirstZ = Candidate("z", 0);
            BodyguardInterceptorCandidate partyFirstA = Candidate("a", 0);
            BodyguardInterceptorCandidate[] ordered =
                BodyguardInterceptionPolicy.OrderEligible(true, true, false,
                    new[] { partySecond, partyFirstZ, partyFirstA });
            Assertions.True(ordered.SequenceEqual(new[] { partyFirstA,
                partyFirstZ, partySecond }),
                "Party order and persistent identity arbitration changed.");
            BodyguardInterceptorCandidate[] reversed =
                BodyguardInterceptionPolicy.OrderEligible(true, true, false,
                    new[] { partyFirstA, partyFirstZ, partySecond }.Reverse());
            Assertions.True(ordered.SequenceEqual(reversed),
                "Candidate enumeration order changed interceptor arbitration.");
        }

        private static BodyguardEligibilityRequest ValidEligibility()
        {
            return new BodyguardEligibilityRequest
            {
                ModuleEnabled = true,
                HostileAttackRoll = true,
                TargetIsAlly = true,
                AttackerIsHostile = true,
                HasBodyguard = true,
                BodyguardModeActive = true,
                Alive = true,
                Conscious = true,
                AbleToAct = true,
                NativeAooAllowed = true,
                AooRemaining = 1,
                ProtectorTargetEdgeDistanceFeet = 5d,
                AdjacencyFeet = 5d,
                DistanceToleranceFeet = 0.001d,
                ThreatensAttacker = true
            };
        }

        private static BodyguardInterceptorCandidate Candidate(string id, int order)
        { return new BodyguardInterceptorCandidate(id, order, true, true, true,
            true, true); }
    }
}
