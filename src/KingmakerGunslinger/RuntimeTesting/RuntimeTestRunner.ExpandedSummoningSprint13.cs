using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Enums;
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
        /// The Sprint 13 rules that resolve synchronously: the two repaired
        /// printed attack routines, the Poison Frog's flat point of bite
        /// damage, and the Shadow Mastiff's bay and shadow blend. The Wolverine
        /// rage is deliberately absent, because its printed onset is "on its
        /// next turn" and proving that needs real game time to pass; it is
        /// gated in the frame-stepped creature review instead.
        /// </summary>
        private static void ExerciseExpandedSummoningSprint13RulesPack(
            BlueprintScriptableObject[] blueprints, UnitEntityData caster,
            UnitEntityData hostile, List<UnitEntityData> created,
            ExpandedSummoningMechanicalEvidence evidence)
        {
            ExerciseSprint13PrintedRoutines(blueprints, caster, created,
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
            List<UnitEntityData> created,
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
                wolverinePrimary != null &&
                wolverinePrimary.name == "KMG_Summoning_Natural_Claw1d6" &&
                wolverineBody.AdditionalLimbs != null &&
                wolverineBody.AdditionalLimbs.Length == 2 &&
                wolverineBody.AdditionalLimbs.Any(value => value != null &&
                    value.name == "KMG_Summoning_Natural_Claw1d6") &&
                wolverineBody.AdditionalLimbs.Any(value => value != null &&
                    value.name == "KMG_Summoning_Natural_Bite1d4") &&
                (wolverineBody.AdditionalSecondaryLimbs == null ||
                    wolverineBody.AdditionalSecondaryLimbs.Length == 0);
            int wolverineClawBonus = ProbeLimbAttackBonus(wolverine,
                "KMG_Summoning_Natural_Claw1d6");
            int wolverineBiteBonus = ProbeLimbAttackBonus(wolverine,
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
            int mastiffBiteBonus = ProbeWeaponAttackBonus(mastiff,
                mastiffPrimary);
            int mastiffTailBonus = mastiffLayout ? ProbeWeaponAttackBonus(
                mastiff, mastiffBody.AdditionalSecondaryLimbs[0]) :
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

            // Force the first save to fail, then to succeed, by fixing the
            // native d20 the save consumes.
            hostile.Descriptor.Buffs.RemoveFact(panic);
            hostile.Descriptor.Buffs.RemoveFact(immunity);
            UnityEngine.Random.InitState(FindNativeD20Seed(1));
            ExecuteExpandedSummoningRuntimeAbility(mastiff, bay, 6,
                new TargetWrapper(mastiff.Position), true);
            bool panickedOnFailure = hostile.Descriptor.Buffs
                .GetBuff(panic) != null;
            bool evilOutsiderSpared = secondMastiff.Descriptor.Buffs
                .GetBuff(panic) == null &&
                mastiff.Descriptor.Buffs.GetBuff(panic) == null;

            hostile.Descriptor.Buffs.RemoveFact(panic);
            UnityEngine.Random.InitState(FindNativeD20Seed(20));
            ExecuteExpandedSummoningRuntimeAbility(mastiff, bay, 6,
                new TargetWrapper(mastiff.Position), true);
            Buff granted = hostile.Descriptor.Buffs.GetBuff(immunity);
            bool immunityOnSuccess = granted != null &&
                hostile.Descriptor.Buffs.GetBuff(panic) == null;
            bool immunityIsPerMastiff = granted != null &&
                granted.Context != null &&
                ReferenceEquals(granted.Context.MaybeCaster, mastiff);

            // A repeat from the same mastiff must find the window closed, even
            // on a roll that would otherwise fail.
            UnityEngine.Random.InitState(FindNativeD20Seed(1));
            ExecuteExpandedSummoningRuntimeAbility(mastiff, bay, 6,
                new TargetWrapper(mastiff.Position), true);
            bool repeatBlocked = hostile.Descriptor.Buffs
                .GetBuff(panic) == null;

            // A different mastiff is not blocked by the first one's window.
            UnityEngine.Random.InitState(FindNativeD20Seed(1));
            ExecuteExpandedSummoningRuntimeAbility(secondMastiff, bay, 6,
                new TargetWrapper(secondMastiff.Position), true);
            bool otherMastiffUnblocked = hostile.Descriptor.Buffs
                .GetBuff(panic) != null;
            hostile.Descriptor.Buffs.RemoveFact(panic);
            hostile.Descriptor.Buffs.RemoveFact(immunity);

            evidence.Sprint13ShadowMastiffBay = carriesBay &&
                expectedDc == ExpandedSummoningSpecialProfiles
                    .ShadowMastiffBayPrintedWillDc &&
                panickedOnFailure && evilOutsiderSpared &&
                immunityOnSuccess && immunityIsPerMastiff && repeatBlocked &&
                otherMastiffUnblocked;
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
                ";otherMastiffStillWorks=" + otherMastiffUnblocked;
        }

        /// <summary>
        /// Printed shadow blend: concealment with a 50% miss chance outside
        /// full daylight, which artificial light does not disturb and a
        /// daylight spell does. The concealment is read back through the
        /// engine's own calculation rather than from the component, and the
        /// daylight negation is exercised by actually applying native Daylight
        /// to the creature.
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
            BlueprintBuff daylight = blueprints.OfType<BlueprintBuff>()
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

            // Native Daylight negates it, and the engine must stop reporting
            // the concealment, not merely stop being asked for it.
            Concealment underDaylight = Concealment.None;
            bool daylightAvailable = daylight != null;
            if (daylightAvailable)
            {
                mastiff.Descriptor.Buffs.AddBuff(daylight, mastiff,
                    TimeSpan.FromMinutes(1));
                TickExpandedSummoningBuffs(mastiff);
                underDaylight = UnitPartConcealment.Calculate(hostile, mastiff,
                    true);
                mastiff.Descriptor.Buffs.RemoveFact(daylight);
                TickExpandedSummoningBuffs(mastiff);
            }
            bool daylightNegates = daylightAvailable &&
                underDaylight != Concealment.Total;
            Concealment restored = UnitPartConcealment.Calculate(hostile,
                mastiff, true);
            bool restoredAfterDaylight = restored == Concealment.Total;

            evidence.Sprint13ShadowBlend = activeByDefault && grantsTotal &&
                daylightNegates && restoredAfterDaylight;
            evidence.Sprint13ShadowBlendDetail = "activeByDefault=" +
                activeByDefault + ";concealment=" + withBlend +
                ";printedGrade=Total;daylightLoaded=" + daylightAvailable +
                ";underDaylight=" + underDaylight + ";negated=" +
                daylightNegates + ";restored=" + restored + "/" +
                restoredAfterDaylight;
        }

        /// <summary>
        /// Lets the engine recompute buff-driven state after a buff was added
        /// or removed, so a concealment read reflects the new fact set.
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
            string weaponName)
        {
            ItemEntityWeapon weapon = LiveLimbWeapons(unit)
                .FirstOrDefault(value => value.Blueprint != null &&
                    value.Blueprint.name == weaponName);
            if (weapon == null) return int.MinValue;
            return ProbeEntityAttackBonus(unit, weapon);
        }

        private static int ProbeWeaponAttackBonus(UnitEntityData unit,
            BlueprintItemWeapon blueprint)
        {
            if (blueprint == null) return int.MinValue;
            ItemEntityWeapon weapon = LiveLimbWeapons(unit)
                .FirstOrDefault(value =>
                    ReferenceEquals(value.Blueprint, blueprint));
            if (weapon == null) return int.MinValue;
            return ProbeEntityAttackBonus(unit, weapon);
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
            ItemEntityWeapon weapon)
        {
            UnityEngine.Random.InitState(FindNativeD20Seed(10));
            var probe = new RuleAttackWithWeapon(unit, unit, weapon, 0);
            Rulebook.Trigger(probe);
            return probe.AttackRoll == null ? int.MinValue :
                probe.AttackRoll.AttackBonus;
        }
    }
}
