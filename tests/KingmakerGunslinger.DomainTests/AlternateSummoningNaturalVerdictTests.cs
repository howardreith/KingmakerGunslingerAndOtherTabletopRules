using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class AlternateSummoningNaturalVerdictTests
    {
        private static NaturalCastObservation Completed(SummonMultiplicity multiplicity, int count)
        {
            var units = new string[count];
            for (int index = 0; index < count; index++) units[index] = "unit";
            return new NaturalCastObservation {
                TargetClickAccepted = true, CommandIssued = true, WaitConditionReached = true,
                CommandFinished = true, CooldownCleared = true, Result = "Success", Acted = true,
                ResourceBefore = 9, ResourceAfter = 8, ExpectedCost = 1,
                Multiplicity = multiplicity, ExpectedUnit = "unit", SummonedUnits = units,
                AllOwnedByCaster = true };
        }

        internal static void OrdinaryCompletionPasses()
        {
            Assertions.Equal(null, AlternateSummoningNaturalVerdict.Verify(
                Completed(SummonMultiplicity.One, 1)), "A single creature cast that completed passes.");
            Assertions.Equal(null, AlternateSummoningNaturalVerdict.Verify(
                Completed(SummonMultiplicity.OneD3, 3)), "Three creatures are legal for 1d3.");
            Assertions.Equal(null, AlternateSummoningNaturalVerdict.Verify(
                Completed(SummonMultiplicity.OneD4PlusOne, 2)), "Two creatures are legal for 1d4+1.");
        }

        internal static void UnfinishedCommandFails()
        {
            NaturalCastObservation value = Completed(SummonMultiplicity.One, 1);
            value.WaitConditionReached = false;
            value.CommandFinished = false;
            Assertions.Equal("wait:command-stalled", AlternateSummoningNaturalVerdict.Verify(value),
                "A creature that appeared does not pass while the command is unfinished.");
        }

        internal static void UnclearedCooldownFails()
        {
            NaturalCastObservation value = Completed(SummonMultiplicity.One, 1);
            value.WaitConditionReached = false;
            value.CooldownCleared = false;
            Assertions.Equal("wait:cooldown-stalled", AlternateSummoningNaturalVerdict.Verify(value),
                "A finished command does not pass while the standard-action cooldown remains.");
        }

        internal static void DeadlineFails()
        {
            NaturalCastObservation value = Completed(SummonMultiplicity.One, 1);
            value.WaitConditionReached = false;
            Assertions.Equal("wait:deadline", AlternateSummoningNaturalVerdict.Verify(value),
                "A wait that ended at the deadline fails even if the state settled afterwards.");
        }

        internal static void OutOfRangeGroupFails()
        {
            Assertions.Equal("quantity:OneD4PlusOne:expected=[2,5];actual=1",
                AlternateSummoningNaturalVerdict.Verify(Completed(SummonMultiplicity.OneD4PlusOne, 1)),
                "One creature is below the 1d4+1 minimum.");
            Assertions.Equal("quantity:OneD3:expected=[1,3];actual=4",
                AlternateSummoningNaturalVerdict.Verify(Completed(SummonMultiplicity.OneD3, 4)),
                "Four creatures exceed 1d3.");
            Assertions.Equal("quantity:One:expected=[1,1];actual=2",
                AlternateSummoningNaturalVerdict.Verify(Completed(SummonMultiplicity.One, 2)),
                "A single option yields exactly one creature.");
        }

        internal static void AssistanceAndAccountingFail()
        {
            NaturalCastObservation value = Completed(SummonMultiplicity.One, 1);
            value.CooldownOverride = true;
            Assertions.Equal("assisted:cooldown-override", AlternateSummoningNaturalVerdict.Verify(value),
                "A cooldown override is not natural timing.");
            value = Completed(SummonMultiplicity.One, 1);
            value.AssistedAct = true;
            Assertions.Equal("assisted:forced-act", AlternateSummoningNaturalVerdict.Verify(value),
                "A forced act is not natural timing.");
            value = Completed(SummonMultiplicity.One, 1);
            value.DirectCommandTick = true;
            Assertions.Equal("assisted:direct-command-tick", AlternateSummoningNaturalVerdict.Verify(value),
                "A directly ticked command is not natural timing.");
            value = Completed(SummonMultiplicity.One, 1);
            value.ResourceAfter = 9;
            Assertions.Equal("resource:expected=1;actual=0", AlternateSummoningNaturalVerdict.Verify(value),
                "A cast that spent nothing fails.");
            value = Completed(SummonMultiplicity.One, 1);
            value.SummonedUnits = new[] { "other" };
            Assertions.Equal("unit:expected=unit;actual=[other]", AlternateSummoningNaturalVerdict.Verify(value),
                "The wrong creature fails.");
        }

        internal static void NativeCountModifiersAreExplicit()
        {
            int minimum, maximum;
            AlternateSummoningNaturalVerdict.Bounds(SummonMultiplicity.OneD4PlusOne, true, false,
                out minimum, out maximum);
            Assertions.True(minimum == 5 && maximum == 5, "Maximize fixes 1d4+1 at five.");
            AlternateSummoningNaturalVerdict.Bounds(SummonMultiplicity.OneD3, false, true,
                out minimum, out maximum);
            Assertions.True(minimum == 1 && maximum == 4, "Empower adds half, rounded down, to 1d3.");
            AlternateSummoningNaturalVerdict.Bounds(SummonMultiplicity.One, true, true,
                out minimum, out maximum);
            Assertions.True(minimum == 1 && maximum == 1, "A single creature has no dice to modify.");
            NaturalCastObservation maximized = Completed(SummonMultiplicity.OneD4PlusOne, 4);
            maximized.Maximize = true;
            Assertions.Equal("quantity:OneD4PlusOne:expected=[5,5];actual=4",
                AlternateSummoningNaturalVerdict.Verify(maximized),
                "An active Maximize is accounted for, not ignored.");
            NaturalCastObservation empowered = Completed(SummonMultiplicity.OneD3, 4);
            empowered.Empower = true;
            Assertions.Equal(null, AlternateSummoningNaturalVerdict.Verify(empowered),
                "An active Empower widens the legal count explicitly.");
        }
    }
}
