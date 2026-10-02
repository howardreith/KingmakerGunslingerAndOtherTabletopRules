using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Enums;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        /// <summary>
        /// The Sprint 13 rules: the two repaired printed attack routines, the
        /// Poison Frog's flat point of bite damage, the Wolverine rage from
        /// trigger to cleanup, and the Shadow Mastiff's bay and shadow blend.
        /// </summary>
        private static void ExerciseExpandedSummoningSprint13RulesPack(
            BlueprintScriptableObject[] blueprints, UnitEntityData caster,
            UnitEntityData hostile, List<UnitEntityData> created,
            ExpandedSummoningMechanicalEvidence evidence)
        {
            ExerciseSprint13PrintedRoutines(blueprints, caster, hostile,
                created, evidence);
            ExerciseSprint13WolverineRage(blueprints, caster, hostile, created,
                evidence);
            ExerciseSprint13ShadowMastiffBay(blueprints, caster, hostile,
                created, evidence);
            ExerciseSprint13ShadowBlend(blueprints, caster, hostile, created,
                evidence);
        }

        /// <summary>
        /// Printed Wolverine: "2 claws +4 (1d6+2), bite +4 (1d4+2)". Every
        /// limb is base attack 2 plus Strength 4 with full Strength damage, so
        /// all three are primary. Printed Shadow Mastiff: "bite +10 (1d8+4
        /// plus trip), tail slap +5 (1d6+2)" - the tail is five lower and
        /// carries half Strength, so it alone is secondary.
        ///
        /// <para>Both facts are measured rather than asserted from the
        /// blueprint shape: each limb is probed on the same freshly summoned
        /// creature with the same fixed natural roll, so the gap between the
        /// live attack bonuses is the engine's own primary/secondary verdict.
        /// A wolverine bite filed as secondary would read five lower than its
        /// claw, which is exactly the defect this sprint repaired.</para>
        /// </summary>
        private static void ExerciseSprint13PrintedRoutines(
            BlueprintScriptableObject[] blueprints, UnitEntityData caster,
            UnitEntityData hostile, List<UnitEntityData> created,
            ExpandedSummoningMechanicalEvidence evidence)
        {
            const string TripDefenceFourLegsGuid =
                "13c87ac5985cc85498ef9d1ac8b78923";
            BlueprintUnitFact tripDefence = blueprints
                .OfType<BlueprintUnitFact>().SingleOrDefault(value =>
                    value.AssetGuid == TripDefenceFourLegsGuid);
            if (tripDefence == null)
                throw new InvalidOperationException(
                    "The exact native quadruped trip-defence fact was not loaded.");

            UnitEntityData wolverine = CastExpandedSummoningCombatUnit(
                blueprints, caster, SummonFamily.NaturesAlly, "wolverine", 3,
                created, evidence);
            RemoveExpandedSummoningAppearanceBuffs(wolverine);
            BlueprintUnit.UnitBody wolverineBody = wolverine.Blueprint.Body;
            string[] wolverineLimbs = DescribeLimbs(wolverineBody);
            BlueprintItemWeapon wolverinePrimary =
                wolverineBody.PrimaryHand as BlueprintItemWeapon;
            bool wolverinePrimaries =
                wolverinePrimary != null && wolverinePrimary.IsNatural &&
                wolverineBody.AdditionalLimbs != null &&
                wolverineBody.AdditionalLimbs.Length == 2 &&
                wolverineBody.AdditionalLimbs.Any(value => value != null &&
                    ReferenceEquals(value, wolverinePrimary)) &&
                wolverineBody.AdditionalLimbs.Any(value => value != null &&
                    value.name == "KMG_Summoning_Natural_Bite1d4") &&
                (wolverineBody.AdditionalSecondaryLimbs == null ||
                    wolverineBody.AdditionalSecondaryLimbs.Length == 0);
            int wolverineClawBonus = ProbeWeaponAttackBonus(wolverine,
                hostile, wolverinePrimary);
            int wolverineBiteBonus = ProbeLimbAttackBonus(wolverine, hostile,
                "KMG_Summoning_Natural_Bite1d4");
            bool wolverineBiteIsPrimary = wolverineClawBonus != int.MinValue &&
                wolverineBiteBonus != int.MinValue &&
                wolverineBiteBonus == wolverineClawBonus;
            bool wolverineTripDefence =
                wolverine.Descriptor.HasFact(tripDefence);

            UnitEntityData frog = CastExpandedSummoningCombatUnit(blueprints,
                caster, SummonFamily.NaturesAlly, "poisonous-frog", 1, created,
                evidence);
            RemoveExpandedSummoningAppearanceBuffs(frog);
            BlueprintItemWeapon frogBite = frog.Blueprint.Body == null ? null :
                frog.Blueprint.Body.PrimaryHand as BlueprintItemWeapon;
            string frogDice = DescribeWeaponDice(frogBite);
            // Printed: "bite +3 (1 plus poison)" - one roll of a one-sided die
            // is a flat point, which is what DiceType.One expresses.
            bool frogBiteIsFlatOne = frogBite != null &&
                frogBite.name == "KMG_Summoning_Natural_Bite1" &&
                frogDice == "1d1";

            UnitEntityData mastiff = CastExpandedSummoningCombatUnit(blueprints,
                caster, SummonFamily.Monster, "shadow-mastiff", 6, created,
                evidence);
            RemoveExpandedSummoningAppearanceBuffs(mastiff);
            BlueprintUnit.UnitBody mastiffBody = mastiff.Blueprint.Body;
            string[] mastiffLimbs = DescribeLimbs(mastiffBody);
            BlueprintItemWeapon mastiffPrimary =
                mastiffBody.PrimaryHand as BlueprintItemWeapon;
            bool mastiffLayout =
                mastiffPrimary != null && mastiffPrimary.IsNatural &&
                (mastiffBody.AdditionalLimbs == null ||
                    mastiffBody.AdditionalLimbs.Length == 0) &&
                mastiffBody.AdditionalSecondaryLimbs != null &&
                mastiffBody.AdditionalSecondaryLimbs.Length == 1 &&
                mastiffBody.AdditionalSecondaryLimbs[0] != null &&
                mastiffBody.AdditionalSecondaryLimbs[0].name ==
                    "KMG_Summoning_Natural_Tail1d6";
            int mastiffBiteBonus = ProbeWeaponAttackBonus(mastiff, hostile,
                mastiffPrimary);
            int mastiffTailBonus = mastiffLayout ? ProbeWeaponAttackBonus(
                mastiff, hostile, mastiffBody.AdditionalSecondaryLimbs[0]) :
                int.MinValue;
            // A PF1 secondary natural attack is five lower than a primary.
            bool mastiffTailIsSecondary = mastiffBiteBonus != int.MinValue &&
                mastiffTailBonus != int.MinValue &&
                mastiffBiteBonus - mastiffTailBonus == 5;
            string mastiffTailDice = mastiffLayout ? DescribeWeaponDice(
                mastiffBody.AdditionalSecondaryLimbs[0]) : "<none>";

            evidence.Sprint13PrintedRoutines = wolverinePrimaries &&
                wolverineBiteIsPrimary && wolverineTripDefence &&
                frogBiteIsFlatOne && mastiffLayout && mastiffTailIsSecondary &&
                mastiffTailDice == "1d6";
            evidence.Sprint13PrintedRoutinesDetail =
                "wolverine[limbs=" + string.Join("/", wolverineLimbs) +
                ";clawBonus=" + Describe(wolverineClawBonus) +
                ";biteBonus=" + Describe(wolverineBiteBonus) +
                ";biteIsPrimary=" + wolverineBiteIsPrimary +
                ";tripDefence=" + wolverineTripDefence +
                "];poisonFrog[bite=" + (frogBite == null ? "<none>" :
                    frogBite.name) + ";dice=" + frogDice +
                ";flatOne=" + frogBiteIsFlatOne +
                "];shadowMastiff[limbs=" + string.Join("/", mastiffLimbs) +
                ";biteBonus=" + Describe(mastiffBiteBonus) +
                ";tailBonus=" + Describe(mastiffTailBonus) +
                ";gap=" + (mastiffBiteBonus == int.MinValue ||
                    mastiffTailBonus == int.MinValue ? "none" :
                    (mastiffBiteBonus - mastiffTailBonus).ToString(
                        CultureInfo.InvariantCulture)) +
                ";tailDice=" + mastiffTailDice +
                ";tailIsSecondary=" + mastiffTailIsSecondary + "]";
        }


        /// <summary>
        /// The printed Wolverine rage, from trigger to cleanup: "A wolverine
        /// that takes damage in combat flies into a rage on its next turn,
        /// clawing and biting madly until either it or its opponent is dead.
        /// It gains +4 to Strength, +4 to Constitution, and -2 to AC. The
        /// creature cannot end its rage voluntarily."
        ///
        /// <para>Five separable claims are measured. The trigger is taking
        /// damage, so a real hostile attack starts it. The onset is the
        /// creature's next turn, so immediately after the blow the hidden
        /// marker is present, the rage state is not, and none of the three
        /// printed numbers has moved yet - this is the claim a rage applied on
        /// the damage event would fail. One round of game time later the
        /// engine's own buff tick runs the marker's round-boundary action, the
        /// marker is gone and the rage state is on with Strength and
        /// Constitution exactly four higher and Armor Class exactly two lower.
        /// A second blow does not restack it. Nothing landed on the caster or
        /// the attacker. Destroying the creature takes both buffs with it and
        /// leaves nothing on anyone.</para>
        ///
        /// <para>The round boundary is reached by advancing the clock and
        /// letting <c>BuffCollection.Tick</c> run, which is how every other
        /// duration gate in this project proves itself and is the engine's own
        /// dispatcher for a buff's round boundary, rather than by calling the
        /// component's handler directly.</para>
        /// </summary>
        private static void ExerciseSprint13WolverineRage(
            BlueprintScriptableObject[] blueprints, UnitEntityData caster,
            UnitEntityData hostile, List<UnitEntityData> created,
            ExpandedSummoningMechanicalEvidence evidence)
        {
            BlueprintBuff onset = blueprints.OfType<BlueprintBuff>().Single(
                value => value.name ==
                    "KMG_Summoning_Natural_Wolverine_RageOnset");
            BlueprintBuff rage = blueprints.OfType<BlueprintBuff>().Single(
                value => value.name ==
                    "KMG_Summoning_Natural_Wolverine_RageState");

            UnitEntityData wolverine = CastExpandedSummoningCombatUnit(
                blueprints, caster, SummonFamily.NaturesAlly, "wolverine", 3,
                created, evidence);
            RemoveExpandedSummoningAppearanceBuffs(wolverine);
            int strengthBefore =
                wolverine.Descriptor.Stats.Strength.ModifiedValue;
            int constitutionBefore =
                wolverine.Descriptor.Stats.Constitution.ModifiedValue;
            int armourBefore = wolverine.Descriptor.Stats.AC.ModifiedValue;
            // Recorded before the rage exists, so the cleanup check below has
            // something real to compare the caster against.
            int casterStrengthBefore =
                caster.Descriptor.Stats.Strength.ModifiedValue;
            int casterArmourBefore = caster.Descriptor.Stats.AC.ModifiedValue;

            // The printed trigger is taking damage, so a real hostile blow
            // starts it rather than a synthesised damage rule.
            ItemEntityWeapon hostileWeapon = LiveLimbWeapons(hostile)
                .FirstOrDefault();
            string blow = "<no hostile weapon>";
            bool damaged = hostileWeapon != null &&
                ExerciseExpandedSummoningWeaponAttack(hostile, wolverine,
                    hostileWeapon, false, 20, out blow);

            bool markerAfterBlow =
                wolverine.Descriptor.Buffs.GetBuff(onset) != null;
            bool rageAfterBlow =
                wolverine.Descriptor.Buffs.GetBuff(rage) != null;
            int strengthAfterBlow =
                wolverine.Descriptor.Stats.Strength.ModifiedValue;
            int armourAfterBlow = wolverine.Descriptor.Stats.AC.ModifiedValue;
            bool delayHeld = markerAfterBlow && !rageAfterBlow &&
                strengthAfterBlow == strengthBefore &&
                armourAfterBlow == armourBefore;

            // One round of game time, then the engine's own buff tick. The
            // marker is expected to still be here: it carries a two-round life
            // so that a round boundary can arrive inside it, and it is checked
            // against that lifetime further down rather than at one round.
            TimeSpan clock = Game.Instance.Player.GameTime;
            bool rageBegan;
            int strengthRaging, constitutionRaging, armourRaging;
            bool markerCleared;
            try
            {
                Game.Instance.Player.GameTime = clock + TimeSpan.FromSeconds(
                    GameConsts.RoundDuration + 1f);
                wolverine.Descriptor.Buffs.Tick();
                rageBegan = wolverine.Descriptor.Buffs.GetBuff(rage) != null;
                strengthRaging =
                    wolverine.Descriptor.Stats.Strength.ModifiedValue;
                constitutionRaging =
                    wolverine.Descriptor.Stats.Constitution.ModifiedValue;
                armourRaging = wolverine.Descriptor.Stats.AC.ModifiedValue;
                // Past the marker's own lifetime it must be gone, and the
                // rage must remain: the printed rage has no duration.
                Game.Instance.Player.GameTime = clock + TimeSpan.FromSeconds(
                    (SummonRagePolicy.WolverineRageOnsetMarkerRounds + 1) *
                    GameConsts.RoundDuration);
                wolverine.Descriptor.Buffs.Tick();
                markerCleared =
                    wolverine.Descriptor.Buffs.GetBuff(onset) == null &&
                    wolverine.Descriptor.Buffs.GetBuff(rage) != null;
            }
            finally
            {
                Game.Instance.Player.GameTime = clock;
            }
            bool printedNumbers =
                strengthRaging - strengthBefore ==
                    SummonRagePolicy.WolverineRageAbilityBonus &&
                constitutionRaging - constitutionBefore ==
                    SummonRagePolicy.WolverineRageAbilityBonus &&
                armourRaging - armourBefore ==
                    SummonRagePolicy.WolverineRageArmorClassPenalty;

            // A second blow must not restack the rage or re-arm the marker.
            string secondBlow = "<not attempted>";
            if (hostileWeapon != null)
                ExerciseExpandedSummoningWeaponAttack(hostile, wolverine,
                    hostileWeapon, false, 20, out secondBlow);
            int rageStacks = wolverine.Descriptor.Buffs.RawFacts.OfType<Buff>()
                .Count(value => ReferenceEquals(value.Blueprint, rage));
            // A second blow must not stack a second rage. The marker is not
            // part of this claim: the trigger declines to re-arm while the
            // rage is already running, which is what rageStacks proves.
            bool noRestack = rageStacks == 1;

            // Summon-local: nothing may land on the caster or the attacker.
            bool noLeak = caster.Descriptor.Buffs.GetBuff(rage) == null &&
                caster.Descriptor.Buffs.GetBuff(onset) == null &&
                hostile.Descriptor.Buffs.GetBuff(rage) == null &&
                hostile.Descriptor.Buffs.GetBuff(onset) == null;

            // The rage leaves with the creature and leaves nothing behind.
            CleanupExpandedSummoningUnit(wolverine);
            Game.Instance.EntityDestroyer.Tick();
            bool cleaned = wolverine.Destroyed &&
                caster.Descriptor.Buffs.GetBuff(rage) == null &&
                caster.Descriptor.Buffs.GetBuff(onset) == null &&
                hostile.Descriptor.Buffs.GetBuff(rage) == null &&
                hostile.Descriptor.Buffs.GetBuff(onset) == null &&
                caster.Descriptor.Stats.Strength.ModifiedValue ==
                    casterStrengthBefore &&
                caster.Descriptor.Stats.AC.ModifiedValue ==
                    casterArmourBefore;

            evidence.Sprint13WolverineRage = damaged && delayHeld &&
                rageBegan && markerCleared && printedNumbers && noRestack &&
                noLeak && cleaned;
            evidence.Sprint13WolverineRageDetail = "blow[" + blow +
                "];damaged=" + damaged +
                ";afterBlow[marker=" + markerAfterBlow + ";rage=" +
                rageAfterBlow + ";str=" + strengthBefore + "->" +
                strengthAfterBlow + ";ac=" + armourBefore + "->" +
                armourAfterBlow + ";delayHeld=" + delayHeld +
                "];afterOneRound[rage=" + rageBegan + ";markerCleared=" +
                markerCleared + ";str=" + strengthBefore + "->" +
                strengthRaging + ";con=" + constitutionBefore + "->" +
                constitutionRaging + ";ac=" + armourBefore + "->" +
                armourRaging + ";printedNumbers=" + printedNumbers +
                "];secondBlow[" + secondBlow + ";rageStacks=" + rageStacks +
                ";noRestack=" + noRestack + "];noLeakToCasterOrAttacker=" +
                noLeak + ";cleanedWithTheSummon=" + cleaned +
                ";casterStr=" + casterStrengthBefore + "->" +
                caster.Descriptor.Stats.Strength.ModifiedValue +
                ";casterAc=" + casterArmourBefore + "->" +
                caster.Descriptor.Stats.AC.ModifiedValue;
        }

        /// <summary>
        /// Printed bay: a 300-foot spread catching every creature except evil
        /// outsiders, a Charisma-based Will save with a +2 racial bonus, panic
        /// for 1d4 rounds on a failure, and a 24-hour immunity to the same
        /// mastiff's bay on a success.
        ///
        /// <para>Four separable claims are measured: the DC the live creature
        /// actually produces, that a failed save panics and a successful one
        /// does not, that the immunity blocks a repeat from the same mastiff,
        /// and that a second Shadow Mastiff - an evil outsider - is never
        /// touched by the first one's bay.</para>
        /// </summary>
        private static void ExerciseSprint13ShadowMastiffBay(
            BlueprintScriptableObject[] blueprints, UnitEntityData caster,
            UnitEntityData hostile, List<UnitEntityData> created,
            ExpandedSummoningMechanicalEvidence evidence)
        {
            BlueprintAbility bay = blueprints.OfType<BlueprintAbility>()
                .Single(value => value.name ==
                    "KMG_Summoning_Special_ShadowMastiff_Bay");
            BlueprintBuff panic = blueprints.OfType<BlueprintBuff>()
                .Single(value => value.name ==
                    "KMG_Summoning_Special_ShadowMastiff_BayPanic");
            BlueprintBuff immunity = blueprints.OfType<BlueprintBuff>()
                .Single(value => value.name ==
                    "KMG_Summoning_Special_ShadowMastiff_BayImmunity");

            UnitEntityData mastiff = CastExpandedSummoningCombatUnit(blueprints,
                caster, SummonFamily.Monster, "shadow-mastiff", 6, created,
                evidence);
            RemoveExpandedSummoningAppearanceBuffs(mastiff);
            UnitEntityData secondMastiff = CastExpandedSummoningCombatUnit(
                blueprints, caster, SummonFamily.Monster, "shadow-mastiff", 6,
                created, evidence);
            RemoveExpandedSummoningAppearanceBuffs(secondMastiff);

            bool carriesBay = mastiff.Descriptor.HasFact(bay);
            int liveCharisma = mastiff.Descriptor.Stats.Charisma.ModifiedValue;
            int expectedDc = SummonShadowMastiffPolicy.BayWillDc(
                ExpandedSummoningSpecialProfiles.ShadowMastiffHitDice,
                liveCharisma);

            // The outcome cannot be steered by seeding one d20: the printed
            // spread is 300 feet of every creature present, so many saves are
            // rolled and a seeded roll lands on whichever resolves first.
            // Everything below is forced through a save stat instead.
            //
            // The subject is the caster, not the hostile. The printed spread
            // does not spare the summoner's own party, and the previous run
            // proved the caster is caught by it while the hostile fixture is
            // not - so the caster is the unit this rule can actually be
            // measured on.
            ModifiableValue casterWill = caster.Descriptor.Stats
                .GetStat(StatType.SaveWill);
            int willBefore = casterWill.BaseValue;

            string panickedByFirstHowl;
            bool panickedOnFailure;
            try
            {
                casterWill.BaseValue = willBefore - 100;
                ClearExpandedSummoningBayFear(panic, immunity, caster,
                    hostile, mastiff, secondMastiff);
                ExecuteExpandedSummoningRuntimeAbility(mastiff, bay, 6,
                    new TargetWrapper(mastiff.Position), true);
                panickedOnFailure =
                    caster.Descriptor.Buffs.GetBuff(panic) != null;
                panickedByFirstHowl = DescribeBayVictims(panic);
            }
            finally { casterWill.BaseValue = willBefore; }

            // Both mastiffs are evil outsiders, so the printed exemption means
            // neither may ever be panicked by the other's howl.
            bool evilOutsiderSpared =
                secondMastiff.Descriptor.Buffs.GetBuff(panic) == null &&
                mastiff.Descriptor.Buffs.GetBuff(panic) == null;

            Buff granted;
            bool immunityOnSuccess;
            try
            {
                casterWill.BaseValue = willBefore + 100;
                ClearExpandedSummoningBayFear(panic, immunity, caster,
                    hostile, mastiff, secondMastiff);
                ExecuteExpandedSummoningRuntimeAbility(mastiff, bay, 6,
                    new TargetWrapper(mastiff.Position), true);
                granted = caster.Descriptor.Buffs.GetBuff(immunity);
                immunityOnSuccess = granted != null &&
                    caster.Descriptor.Buffs.GetBuff(panic) == null;
            }
            finally { casterWill.BaseValue = willBefore; }
            bool immunityIsPerMastiff = granted != null &&
                granted.Context != null &&
                ReferenceEquals(granted.Context.MaybeCaster, mastiff);

            // The window is closed for this mastiff, so a repeat must not
            // panic even with the subject's Will floored.
            bool repeatBlocked;
            try
            {
                casterWill.BaseValue = willBefore - 100;
                caster.Descriptor.Buffs.RemoveFact(panic);
                ExecuteExpandedSummoningRuntimeAbility(mastiff, bay, 6,
                    new TargetWrapper(mastiff.Position), true);
                repeatBlocked = caster.Descriptor.Buffs.GetBuff(panic) == null;
            }
            finally { casterWill.BaseValue = willBefore; }

            // A different mastiff is not inside that window.
            bool otherMastiffUnblocked;
            try
            {
                casterWill.BaseValue = willBefore - 100;
                caster.Descriptor.Buffs.RemoveFact(panic);
                ExecuteExpandedSummoningRuntimeAbility(secondMastiff, bay, 6,
                    new TargetWrapper(secondMastiff.Position), true);
                otherMastiffUnblocked =
                    caster.Descriptor.Buffs.GetBuff(panic) != null;
            }
            finally { casterWill.BaseValue = willBefore; }

            // Leaving a party member panicked and fleeing would contaminate
            // everything after this, so every trace of the fear this gate
            // caused is removed before it returns.
            ClearExpandedSummoningBayFear(panic, immunity, caster, hostile,
                mastiff, secondMastiff);
            string residualFear = DescribeBayVictims(panic);

            evidence.Sprint13ShadowMastiffBay = carriesBay &&
                expectedDc == ExpandedSummoningSpecialProfiles
                    .ShadowMastiffBayPrintedWillDc &&
                panickedOnFailure && evilOutsiderSpared &&
                immunityOnSuccess && immunityIsPerMastiff && repeatBlocked &&
                otherMastiffUnblocked && residualFear == "<none>";
            evidence.Sprint13ShadowMastiffBayDetail = "carriesBay=" +
                carriesBay + ";liveCharisma=" + liveCharisma +
                ";derivedDc=" + expectedDc + ";printedDc=" +
                ExpandedSummoningSpecialProfiles
                    .ShadowMastiffBayPrintedWillDc +
                ";panickedOnFailure=" + panickedOnFailure +
                ";evilOutsidersSpared=" + evilOutsiderSpared +
                ";immunityOnSuccess=" + immunityOnSuccess +
                ";immunityIsPerMastiff=" + immunityIsPerMastiff +
                ";repeatFromSameMastiffBlocked=" + repeatBlocked +
                ";otherMastiffStillWorks=" + otherMastiffUnblocked +
                ";panickedByFirstHowl[" + panickedByFirstHowl +
                "];residualFearAfterCleanup=" + residualFear;
        }

        /// <summary>
        /// Every unit currently panicked by a mastiff's bay, named, so the
        /// evidence says who the printed spread actually reached rather than
        /// only whether one chosen unit was reached.
        /// </summary>
        private static string DescribeBayVictims(BlueprintBuff panic)
        {
            if (Game.Instance == null || Game.Instance.State == null ||
                Game.Instance.State.Units == null) return "<no unit list>";
            string[] names = Game.Instance.State.Units
                .Where(unit => unit != null && unit.Descriptor != null &&
                    unit.Descriptor.Buffs.GetBuff(panic) != null)
                .Select(unit => unit.CharacterName).ToArray();
            return names.Length == 0 ? "<none>" :
                string.Join(",", names);
        }

        /// <summary>
        /// Removes the bay's own payload, its immunity marker and the native
        /// frightened state from the named units and from anyone else the
        /// spread reached, so this gate leaves no fear behind.
        /// </summary>
        private static void ClearExpandedSummoningBayFear(BlueprintBuff panic,
            BlueprintBuff immunity, params UnitEntityData[] named)
        {
            BlueprintBuff frightened = BlueprintRoot.Instance == null ? null :
                BlueprintRoot.Instance.SystemMechanics.FrightenedBuff;
            var units = new List<UnitEntityData>(named ??
                new UnitEntityData[0]);
            if (Game.Instance != null && Game.Instance.State != null &&
                Game.Instance.State.Units != null)
                units.AddRange(Game.Instance.State.Units);
            foreach (UnitEntityData unit in units)
            {
                if (unit == null || unit.Descriptor == null) continue;
                unit.Descriptor.Buffs.RemoveFact(panic);
                unit.Descriptor.Buffs.RemoveFact(immunity);
                if (frightened != null)
                    unit.Descriptor.Buffs.RemoveFact(frightened);
                if (unit.Descriptor.State != null)
                    unit.Descriptor.State.IsPanicked = false;
            }
        }

        /// <summary>
        /// Printed shadow blend: concealment with a 50% miss chance outside
        /// full daylight, which artificial light does not disturb and a
        /// daylight spell does.
        ///
        /// <para>The concealment is read back through the engine's own
        /// calculation rather than from the component, and the negation is
        /// produced the way a player produces it: the caster is granted the
        /// exact native Daylight spell and casts it, rather than a buff being
        /// planted to stand in for the spell.</para>
        /// </summary>
        private static void ExerciseSprint13ShadowBlend(
            BlueprintScriptableObject[] blueprints, UnitEntityData caster,
            UnitEntityData hostile, List<UnitEntityData> created,
            ExpandedSummoningMechanicalEvidence evidence)
        {
            const string NativeDaylightGuid = "2b877386976817a429002e8bb10bb3fc";
            BlueprintBuff blendState = blueprints.OfType<BlueprintBuff>()
                .Single(value => value.name ==
                    "KMG_Summoning_Special_ShadowMastiff_ShadowBlendState");
            // Daylight is a spell, which is what this project's own audited
            // native light-spell census records it as.
            BlueprintAbility daylight = blueprints.OfType<BlueprintAbility>()
                .SingleOrDefault(value => value.AssetGuid == NativeDaylightGuid);

            UnitEntityData mastiff = CastExpandedSummoningCombatUnit(blueprints,
                caster, SummonFamily.Monster, "shadow-mastiff", 6, created,
                evidence);
            RemoveExpandedSummoningAppearanceBuffs(mastiff);

            // The printed ability starts active, so the toggle's own buff must
            // already be on the creature without the player doing anything.
            bool activeByDefault = mastiff.Descriptor.Buffs
                .GetBuff(blendState) != null;
            Concealment withBlend = UnitPartConcealment.Calculate(hostile,
                mastiff, true);
            bool grantsTotal = withBlend == Concealment.Total;

            // A real cast of the real spell, by a caster granted it for the
            // probe, so the negation is the effect a player would create.
            Concealment underDaylight = Concealment.None;
            string daylightShape = daylight == null ? "<not loaded>" :
                DescribeDaylight(daylight);
            bool castDaylight = false;
            string castDetail = "<not attempted>";
            if (daylight != null)
            {
                try
                {
                    // Native Daylight carries AbilityTargetIsPartyMember, so
                    // it cannot target a summon at all. A party member carries
                    // the light instead, which is what the printed negation
                    // amounts to in this engine: the mastiff is standing
                    // inside the spell's own 60-foot radius.
                    caster.Descriptor.AddFact(daylight);
                    ExecuteExpandedSummoningRuntimeAbility(caster, daylight, 3,
                        new TargetWrapper(caster), true);
                    castDaylight = true;
                    // The engine's static concealment calculation raises no
                    // rule, so the entry is refreshed through a round tick
                    // before it is read.
                    TickExpandedSummoningBuffs(mastiff);
                    castDetail = "cast-on-party-member;distance=" +
                        UnityEngine.Vector3.Distance(caster.Position,
                            mastiff.Position).ToString("0.##",
                            CultureInfo.InvariantCulture);
                }
                catch (Exception exception)
                {
                    castDetail = "cast-failed:" + exception.GetType().Name +
                        ":" + exception.Message;
                }
                underDaylight = UnitPartConcealment.Calculate(hostile, mastiff,
                    true);
            }
            bool daylightNegates = castDaylight &&
                underDaylight != Concealment.Total;
            // The negation did not fire with Daylight burning beside the
            // creature, so the evidence records which buff was derived from
            // Daylight's own action list and whether the caster ended up
            // carrying it, instead of leaving the cause to be guessed.
            string derived = DescribeDerivedNegation(blendState, caster);
            evidence.Sprint13ShadowBlend = activeByDefault && grantsTotal &&
                castDaylight && daylightNegates;
            evidence.Sprint13ShadowBlendDetail = "activeByDefault=" +
                activeByDefault + ";concealment=" + withBlend +
                ";printedGrade=Total;daylight[" + daylightShape + ";" +
                castDetail + "];underDaylight=" + underDaylight +
                ";negated=" + daylightNegates + ";derivedNegation[" +
                derived + "]" + "";
        }

        /// <summary>
        /// What the native Daylight spell actually is, recorded so the
        /// evidence names the identity the negation depends on instead of
        /// asserting it blindly.
        /// </summary>
        private static string DescribeDaylight(BlueprintAbility daylight)
        {
            return "name=" + daylight.name + ";type=" + daylight.Type +
                ";components=" + string.Join(",",
                    (daylight.ComponentsArray ??
                        Array.Empty<BlueprintComponent>())
                    .Where(value => value != null)
                    .Select(value => value.GetType().Name).ToArray());
        }

        /// <summary>
        /// Which buff the shadow blend was told to watch for, and whether the
        /// unit that cast Daylight actually carries it. Both are blueprint and
        /// live-fact reads, so this reports the real state rather than the
        /// component's own opinion of it.
        /// </summary>
        private static string DescribeDerivedNegation(BlueprintBuff blendState,
            UnitEntityData caster)
        {
            SummonShadowBlendComponent gate = (blendState.ComponentsArray ??
                Array.Empty<BlueprintComponent>())
                .OfType<SummonShadowBlendComponent>().FirstOrDefault();
            if (gate == null) return "<no blend component on the buff>";
            string[] names = (gate.NegatingBuffs ?? new BlueprintBuff[0])
                .Where(value => value != null)
                .Select(value => value.name + "/carriedByCaster=" +
                    (caster.Descriptor.Buffs.GetBuff(value) != null))
                .ToArray();
            return "radiusFeet=" + gate.NegatingRadiusFeet + ";buffs=" +
                (names.Length == 0 ? "<none>" : string.Join(",", names)) +
                ";casterBuffs=" + string.Join("|",
                    caster.Descriptor.Buffs.RawFacts.OfType<Buff>()
                        .Select(value => value.Blueprint.name).ToArray());
        }

        /// <summary>
        /// Lets the engine recompute buff-driven state after a buff was added
        /// or removed, so a concealment read reflects the new fact set rather
        /// than whatever the entry was last set to.
        /// </summary>
        private static void TickExpandedSummoningBuffs(UnitEntityData unit)
        {
            if (unit == null || unit.Descriptor == null) return;
            unit.Descriptor.Buffs.Tick();
        }

        private static string[] DescribeLimbs(BlueprintUnit.UnitBody body)
        {
            if (body == null) return new[] { "<no body>" };
            var values = new List<string> {
                "primary=" + (body.PrimaryHand == null ? "<none>" :
                    body.PrimaryHand.name) };
            values.Add("additional=" + (body.AdditionalLimbs == null ? "" :
                string.Join(",", body.AdditionalLimbs.Select(value =>
                    value == null ? "<null>" : value.name).ToArray())));
            values.Add("secondary=" + (body.AdditionalSecondaryLimbs == null ?
                "" : string.Join(",", body.AdditionalSecondaryLimbs.Select(
                    value => value == null ? "<null>" : value.name)
                    .ToArray())));
            return values.ToArray();
        }

        /// <summary>
        /// Reads a weapon blueprint's own damage dice, which is where a flat
        /// printed value has to live; a live damage roll cannot distinguish a
        /// flat 1 from 1d1 once a Tiny creature's Strength penalty floors it.
        /// </summary>
        private static string DescribeWeaponDice(BlueprintItemWeapon weapon)
        {
            if (weapon == null) return "<none>";
            FieldInfo field = weapon.GetType().GetField("m_DamageDice",
                BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.NonPublic);
            if (field == null) return "<no dice field>";
            object value = field.GetValue(weapon);
            if (!(value is DiceFormula)) return "<not a dice formula>";
            var formula = (DiceFormula)value;
            return formula.Rolls.ToString(CultureInfo.InvariantCulture) + "d" +
                ((int)formula.Dice).ToString(CultureInfo.InvariantCulture);
        }

        private static int ProbeLimbAttackBonus(UnitEntityData unit,
            UnitEntityData target, string weaponName)
        {
            ItemEntityWeapon weapon = LiveLimbWeapons(unit)
                .FirstOrDefault(value => value.Blueprint != null &&
                    value.Blueprint.name == weaponName);
            if (weapon == null) return int.MinValue;
            return ProbeEntityAttackBonus(unit, target, weapon);
        }

        private static int ProbeWeaponAttackBonus(UnitEntityData unit,
            UnitEntityData target, BlueprintItemWeapon blueprint)
        {
            if (blueprint == null) return int.MinValue;
            ItemEntityWeapon weapon = LiveLimbWeapons(unit)
                .FirstOrDefault(value =>
                    ReferenceEquals(value.Blueprint, blueprint));
            if (weapon == null) return int.MinValue;
            return ProbeEntityAttackBonus(unit, target, weapon);
        }

        /// <summary>
        /// Every weapon the creature actually carries on a limb: its primary
        /// hand plus every additional limb the body granted it.
        /// </summary>
        private static IEnumerable<ItemEntityWeapon> LiveLimbWeapons(
            UnitEntityData unit)
        {
            if (unit == null || unit.Body == null)
                return Enumerable.Empty<ItemEntityWeapon>();
            // The primary hand is a HandSlot and the additional limbs are
            // WeaponSlots, so the weapons are collected rather than the slots.
            var weapons = new List<ItemEntityWeapon>();
            if (unit.Body.PrimaryHand != null &&
                unit.Body.PrimaryHand.MaybeWeapon != null)
                weapons.Add(unit.Body.PrimaryHand.MaybeWeapon);
            if (unit.Body.AdditionalLimbs != null)
                foreach (Kingmaker.Items.Slots.WeaponSlot limb in
                    unit.Body.AdditionalLimbs)
                    if (limb != null && limb.MaybeWeapon != null)
                        weapons.Add(limb.MaybeWeapon);
            return weapons;
        }

        /// <summary>
        /// Probes one limb's live attack bonus without mutating the creature.
        /// The shared attack helper sets an attacker's base attack bonus to
        /// 100 and leaves it there, so it must not be used where two limbs on
        /// the same creature are being compared.
        /// </summary>
        private static int ProbeEntityAttackBonus(UnitEntityData unit,
            UnitEntityData target, ItemEntityWeapon weapon)
        {
            // The target must be a real distinct unit: a creature aimed at
            // itself produces no attack roll at all, which is what made the
            // first run report every limb bonus as zero.
            if (target == null || ReferenceEquals(target, unit))
                return int.MinValue;
            UnityEngine.Random.InitState(FindNativeD20Seed(10));
            var probe = new RuleAttackWithWeapon(unit, target, weapon, 0);
            Rulebook.Trigger(probe);
            return probe.AttackRoll == null ? int.MinValue :
                probe.AttackRoll.AttackBonus;
        }
    }
}
