using System;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ExpandedSummoningSprint13Tests
    {
        /// <summary>
        /// Sprint 13 appends the Poison Frog's flat-1 bite and the three
        /// Wolverine rage blueprints.
        /// </summary>
        internal const int AppendedLedgerIdentities = 4;

        private static string Source(params string[] parts)
        {
            string[] all = new[] { Environment.CurrentDirectory, "src",
                "KingmakerGunslinger" }.Concat(parts).ToArray();
            return File.ReadAllText(Path.Combine(all));
        }

        /// <summary>
        /// The printed rage triggers on taking damage, and only on damage that
        /// actually landed. A miss, a fully resisted hit and a zero-damage
        /// effect are all "was attacked" rather than "takes damage".
        /// </summary>
        internal static void RageSchedulesOnlyOnRealDamageToTheWolverine()
        {
            Assertions.True(SummonRagePolicy.ShouldScheduleRage(1, true, true,
                false, false),
                "One point of real damage must schedule the printed rage.");
            Assertions.False(SummonRagePolicy.ShouldScheduleRage(0, true, true,
                false, false),
                "Zero damage is not taking damage.");
            Assertions.False(SummonRagePolicy.ShouldScheduleRage(-3, true, true,
                false, false),
                "Negative damage is not taking damage.");
            Assertions.False(SummonRagePolicy.ShouldScheduleRage(7, false, true,
                false, false),
                "Damage dealt to someone else must not enrage the wolverine.");
            Assertions.False(SummonRagePolicy.ShouldScheduleRage(7, true, false,
                false, false),
                "A destroyed or dead wolverine must not be scheduled.");
            Assertions.False(SummonRagePolicy.ShouldScheduleRage(7, true, true,
                true, false),
                "The printed rage does not restack on an already raging creature.");
            Assertions.False(SummonRagePolicy.ShouldScheduleRage(7, true, true,
                false, true),
                "A second hit must not restart a rage that is already coming.");
        }

        /// <summary>
        /// "flies into a rage on its next turn". No amount or arrangement of
        /// damage may start the rage before a round boundary has passed.
        /// </summary>
        internal static void RageNeverBeginsOnTheTurnTheDamageLanded()
        {
            Assertions.False(SummonRagePolicy.ShouldBeginRage(0, true, false),
                "The rage must not begin on the turn the damage landed.");
            Assertions.True(SummonRagePolicy.ShouldBeginRage(1, true, false),
                "The rage must begin once its printed round boundary passed.");
            Assertions.True(SummonRagePolicy.ShouldBeginRage(4, true, false),
                "A longer wait must still begin the rage.");
            Assertions.False(SummonRagePolicy.ShouldBeginRage(1, false, false),
                "A gone wolverine must not begin raging.");
            Assertions.False(SummonRagePolicy.ShouldBeginRage(1, true, true),
                "An already raging wolverine must not begin a second rage.");
            Assertions.Equal(1, SummonRagePolicy.WolverineRageOnsetRounds,
                "The printed onset is one round boundary.");
        }

        /// <summary>
        /// The printed effect is "+4 to Strength, +4 to Constitution, and -2 to
        /// AC", and the rage "cannot end voluntarily". A rage that kept the
        /// bonuses and dropped the penalty would be stronger than the stat
        /// block, which is the exact class of defect the owner rejected in
        /// Phase 1.
        /// </summary>
        internal static void RageCarriesThePrintedArmourPenaltyAndNoVoluntaryEnd()
        {
            Assertions.Equal(4, SummonRagePolicy.WolverineRageAbilityBonus,
                "Printed rage ability bonus changed.");
            Assertions.Equal(-2, SummonRagePolicy.WolverineRageArmorClassPenalty,
                "Printed rage armour-class penalty changed or lost its sign.");
            Assertions.False(SummonRagePolicy.MayEndVoluntarily(),
                "The printed rage cannot be ended voluntarily.");
            SummonRagePolicy.Validate();

            string builder = Source("Blueprints",
                "ExpandedSummoningNaturalBuilder.cs");
            Assertions.True(builder.Contains("armourClass.Stat = StatType.AC") &&
                builder.Contains(
                    "armourClass.Value = SummonRagePolicy.WolverineRageArmorClassPenalty"),
                "The rage buff must carry the printed AC term from the policy constant.");
            Assertions.True(
                builder.Contains("strength.Descriptor = ModifierDescriptor.Morale") &&
                builder.Contains(
                    "constitution.Descriptor = ModifierDescriptor.Morale"),
                "The printed rage bonuses are morale bonuses.");
        }

        /// <summary>
        /// The delay must be structural rather than a convention. The trigger
        /// component may apply only the onset marker, and the rage state may be
        /// reached only from the marker's round-boundary action list, so no
        /// code path exists from a damage event to the +4/+4/-2.
        /// </summary>
        internal static void RageReachesItsStateOnlyThroughARoundBoundary()
        {
            string component = Source("Summoning",
                "ExpandedSummoningSprint13CombatComponents.cs");
            Assertions.True(component.Contains("OnsetBuff") &&
                component.Contains("RuleApplyBuff(owner, OnsetBuff"),
                "The damage trigger must apply the onset marker.");
            Assertions.False(component.Contains("RuleApplyBuff(owner, RageBuff"),
                "The damage trigger must never apply the rage state directly.");

            string builder = Source("Blueprints",
                "ExpandedSummoningNaturalBuilder.cs");
            Assertions.True(builder.Contains("SetBuffOnsetDelay") &&
                builder.Contains("delay.OnStart = new ActionList") &&
                builder.Contains("apply.Buff = state"),
                "The rage state must be applied from the onset marker's round-boundary action list.");
            Assertions.True(builder.Contains("apply.Permanent = true"),
                "The printed rage has no duration and runs until the creature dies.");
        }

        /// <summary>
        /// The rage belongs to one animal. Both buffs are applied to the
        /// component's own carrier and to no other unit, so no caster, ally or
        /// other summon can receive them.
        /// </summary>
        internal static void RageStaysOnTheOneWolverine()
        {
            string component = Source("Summoning",
                "ExpandedSummoningSprint13CombatComponents.cs");
            Assertions.True(component.Contains(
                "ReferenceEquals(evt.Target, owner)"),
                "The trigger must only fire for damage dealt to its own carrier.");
            Assertions.False(component.Contains("ToCaster"),
                "Nothing in the summon-local rage may travel to the caster.");

            string builder = Source("Blueprints",
                "ExpandedSummoningNaturalBuilder.cs");
            Assertions.False(builder.Contains("apply.ToCaster = true"),
                "The rage state must not be applied to the caster.");
        }

        /// <summary>
        /// Printed: "bite +3 (1 plus poison)". A flat point, which
        /// <c>DiceType.One</c> expresses exactly, so the previous 1d3 - which
        /// averages 2 and can roll 3 - was simply wrong rather than a necessary
        /// adaptation.
        /// </summary>
        internal static void PoisonFrogBiteIsThePrintedFlatPoint()
        {
            NaturalSummonProfile frog =
                ExpandedSummoningNaturalProfiles.For("poisonous-frog");
            Assertions.Equal("Bite1", frog.PrimaryWeapon,
                "The Poison Frog's printed flat-1 bite changed.");
            Assertions.Equal(0, frog.AdditionalWeapons.Count,
                "The Poison Frog has one printed attack.");
            Assertions.True(frog.Facts.Contains("PoisonFrog"),
                "The accepted Poison Frog poison contract must be preserved.");

            string builder = Source("Blueprints",
                "ExpandedSummoningNaturalBuilder.cs");
            Assertions.True(builder.Contains("Bite1Symbol), Bite1Symbol, 1, DiceType.One"),
                "The flat-1 bite must be minted at exactly one roll of DiceType.One.");
            Assertions.True(builder.Contains(
                "\"KMG.Summoning.Natural.Bite1\""),
                "The flat-1 bite needs its own project-owned identity.");
        }

        /// <summary>
        /// The four Sprint 13 identities are declared in the frozen catalog so
        /// the append-only ledger allocates them, and none replaces an existing
        /// symbol.
        /// </summary>
        internal static void Sprint13IdentitiesAreDeclaredAndAppendOnly()
        {
            string catalog = Source("Summoning",
                "ExpandedSummoningIdentityCatalog.cs");
            string[] symbols = {
                "KMG.Summoning.Natural.Bite1",
                "KMG.Summoning.Natural.Wolverine.Rage",
                "KMG.Summoning.Natural.Wolverine.RageOnset",
                "KMG.Summoning.Natural.Wolverine.RageState" };
            foreach (string symbol in symbols)
                Assertions.True(catalog.Contains("\"" + symbol + "\""),
                    "Sprint 13 identity is not declared: " + symbol);
            Assertions.Equal(AppendedLedgerIdentities, symbols.Length,
                "Sprint 13 ledger count must match its declared identities.");
        }
    }
}
