using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using KingmakerGunslinger.Assets;
using KingmakerGunslinger.Summoning;
using TurnBased.Controllers;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// The Bebelith half of the Sprint 21 guarded review.
    ///
    /// <para>A different kind of review from the Giant Crab's, and that is
    /// the reason it is its own file. The crab is new and the question is
    /// whether it arrives correct. The Bebelith is RELEASED: a player can
    /// summon one today, and the question is whether an overhaul that
    /// deliberately keeps its identity has actually changed its mechanics.
    /// So most of what follows is negative controls, because this creature's
    /// two hardest mechanics are both defined by what they must not
    /// touch.</para>
    ///
    /// <para>Penetrating strike grants cold iron and good against demons
    /// only. Granting them universally would be invisible against a demon and
    /// wrong against everything else, so the case that can see it is the one
    /// that hits something which is not a demon and reads the damage bundle
    /// back. Dismantle Armor must do nothing at all to a target with nothing
    /// equipped, and must leave the equipment of a target it does catch
    /// exactly as it found it - two absences, and no positive case can prove
    /// either.</para>
    ///
    /// <para>Everything the fixture arranges is request-local and restored:
    /// the target's alignment for the demon control, and one disposable
    /// armour for the dismantle cases. No owner party member is touched, no
    /// equipped item is mutated, and the equipment census is taken before and
    /// after and compared.</para>
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        /// <summary>
        /// Every equipped item on a unit, as one comparable string.
        ///
        /// <para>This is the whole of the "no permanent inventory corruption"
        /// check and it is deliberately coarse: blueprint, enchantment value
        /// and slot for every item the body holds, in a fixed order. A
        /// dismantle that moved, unequipped, enchanted or replaced anything
        /// changes this string, and a dismantle that is purely wearer-side
        /// cannot.</para>
        /// </summary>
        private static string Sprint21EquipmentCensus(UnitEntityData unit)
        {
            UnitBody body = unit == null || unit.Descriptor == null ? null
                : unit.Descriptor.Body;
            if (body == null) return "body=absent";
            var rows = new List<string>();
            foreach (ItemEntity item in body.Items)
            {
                if (item == null) continue;
                rows.Add((item.Blueprint == null ? "?" : item.Blueprint.name)
                    + ":" + item.EnchantmentValue
                    + ":" + (item.HoldingSlot == null ? "none"
                        : item.HoldingSlot.GetType().Name));
            }
            rows.Sort(StringComparer.Ordinal);
            return rows.Count == 0 ? "equipped=none"
                : string.Join("|", rows.ToArray());
        }

        /// <summary>
        /// What a damage bundle says about itself: the descriptors
        /// penetrating strike is supposed to add, and the two it is supposed
        /// to add only against a demon.
        /// </summary>
        private static JObject Sprint21Descriptors(RuleDealDamage damage)
        {
            bool chaotic = false, good = false, coldIron = false;
            int enchantment = 0;
            if (damage != null && damage.DamageBundle != null)
                foreach (BaseDamage value in damage.DamageBundle)
                {
                    if (value == null) continue;
                    chaotic |=
                        (value.AlignmentsMask & DamageAlignment.Chaotic) != 0;
                    good |= (value.AlignmentsMask & DamageAlignment.Good) != 0;
                    var physical = value as PhysicalDamage;
                    if (physical == null) continue;
                    coldIron |= (physical.MaterialsMask &
                        PhysicalDamageMaterial.ColdIron) != 0;
                    if (physical.Enchantment > enchantment)
                        enchantment = physical.Enchantment;
                }
            return new JObject {
                ["chaotic"] = chaotic, ["good"] = good,
                ["coldIron"] = coldIron, ["enchantment"] = enchantment };
        }

        /// <summary>
        /// The Bebelith as it is in a running game: the printed block, both
        /// of its own carriers, the rot graph, penetrating strike with its
        /// control, and the dismantle sequence with both of its controls.
        /// </summary>
        private IEnumerable<int> ReviewSprint21Bebelith(
            ExpandedSummoningCorrectionFixture fixture, bool turnBased)
        {
            Sprint21BebelithReviewPolicy.Validate();
            UnitEntityData owner = CastExpandedSummoningOwnTier(
                fixture, Sprint21BebelithReviewPolicy.BebelithKey);
            SetExpandedSummoningBrainActive(owner, false);
            int settle = 0;
            while (++settle <= Sprint21SettleFrames)
            {
                if (Game.Instance.IsPaused) Game.Instance.IsPaused = false;
                yield return 0;
                if (owner.View != null && owner.Descriptor.State.CanAct) break;
            }
            try
            {
                ReviewSprint21BebelithProfile(fixture, owner, turnBased);
                ReviewSprint21BebelithCarriers(fixture, owner, turnBased);
                ReviewSprint21BebelithBody(owner, turnBased);
                foreach (int step in ReviewSprint21BebelithStrike(
                    fixture, owner, turnBased)) yield return step;
                foreach (int step in ReviewSprint21BebelithRot(
                    fixture, owner, turnBased)) yield return step;
                foreach (int step in ReviewSprint21BebelithDismantle(
                    fixture, owner, turnBased)) yield return step;
            }
            finally
            {
                ResetExpandedSummoningHostile(fixture);
                DisposeExpandedSummoningUnits(fixture.Created,
                    new[] { owner });
            }
            for (int teardown = 0; teardown < 8; teardown++) yield return 0;
        }

        /// <summary>
        /// The printed stat block, read off the live creature.
        ///
        /// <para>Three of these numbers are what the overhaul exists to fix,
        /// and they are stated here so a reader can see which: the Will save,
        /// which needs the Iron Will the released build omitted; the
        /// anti-trip defence, which needs a carrier the shared native fact
        /// cannot deliver; and the racial Stealth, which existed only in a
        /// derivation.</para>
        /// </summary>
        private void ReviewSprint21BebelithProfile(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner,
            bool turnBased)
        {
            Sprint21BebelithReviewPolicy.LiveProfile printed =
                Sprint21BebelithReviewPolicy.Bebelith;
            UnitDescriptor live = owner.Descriptor;
            var measured = new JObject {
                ["strength"] = live.Stats.Strength.ModifiedValue,
                ["dexterity"] = live.Stats.Dexterity.ModifiedValue,
                ["constitution"] = live.Stats.Constitution.ModifiedValue,
                ["intelligence"] = live.Stats.Intelligence.ModifiedValue,
                ["wisdom"] = live.Stats.Wisdom.ModifiedValue,
                ["charisma"] = live.Stats.Charisma.ModifiedValue,
                ["hitPoints"] = live.Stats.HitPoints.ModifiedValue,
                ["armorClass"] = live.Stats.AC.ModifiedValue,
                ["fortitude"] = live.Stats.SaveFortitude.ModifiedValue,
                ["reflex"] = live.Stats.SaveReflex.ModifiedValue,
                ["will"] = live.Stats.SaveWill.ModifiedValue,
                ["size"] = live.State.Size.ToString(),
                ["stealth"] = live.Stats.SkillStealth.ModifiedValue };
            bool scores =
                live.Stats.Strength.ModifiedValue == printed.Strength &&
                live.Stats.Dexterity.ModifiedValue == printed.Dexterity &&
                live.Stats.Constitution.ModifiedValue == printed.Constitution &&
                live.Stats.Wisdom.ModifiedValue == printed.Wisdom &&
                live.Stats.Charisma.ModifiedValue == printed.Charisma;
            Sprint21Check("bebelith-live-profile-" +
                    (turnBased ? "turn-based" : "real-time"), scores,
                measured,
                "the printed Bebelith ability scores, read off the live "
                + "creature rather than off the builder");
            // The Will save, named separately because it is the one number
            // the released build could not produce: a poor save plus Iron
            // Will, where three good saves cannot reach +7.
            Sprint21Check("bebelith-printed-will-" +
                    (turnBased ? "turn-based" : "real-time"),
                live.Stats.SaveWill.ModifiedValue == printed.Will,
                new JObject {
                    ["printed"] = printed.Will,
                    ["measured"] = live.Stats.SaveWill.ModifiedValue,
                    ["poorSave"] = BebelithRulesPolicy.PoorSave,
                    ["ironWill"] = BebelithRulesPolicy.IronWillBonus },
                "a poor Will save plus Iron Will, which is the only way the "
                + "printed +7 can be reached");
            Sprint21Check("bebelith-is-huge-" +
                    (turnBased ? "turn-based" : "real-time"),
                live.State.Size == Size.Huge,
                new JObject { ["size"] = live.State.Size.ToString(),
                    ["printedSpaceFeet"] =
                        BebelithRulesPolicy.PrintedSpaceFeet },
                "fifteen feet of space and reach is a Huge creature");
        }

        /// <summary>
        /// The two carriers this creature owns, measured from the engine.
        ///
        /// <para>Both are printed lines whose arithmetic closes without them,
        /// which is exactly the defect class Sprint 20 paid two guarded runs
        /// to find: a registered identity and a closed derivation prove
        /// nothing at all. So the anti-trip defence is measured by asking the
        /// engine for a trip manoeuvre's target defence rather than by
        /// reading a component, and the racial Stealth is read out of the
        /// live skill.</para>
        /// </summary>
        private void ReviewSprint21BebelithCarriers(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner,
            bool turnBased)
        {
            var trip = new RuleCombatManeuver(fixture.Hostile, owner,
                CombatManeuver.Trip);
            var ordinary = new RuleCombatManeuver(fixture.Hostile, owner,
                CombatManeuver.BullRush);
            Rulebook.Trigger(trip);
            Rulebook.Trigger(ordinary);
            int against = trip.TargetCMD;
            int baseline = ordinary.TargetCMD;
            Sprint21Check("bebelith-eight-legged-trip-defence-" +
                    (turnBased ? "turn-based" : "real-time"),
                against - baseline == BebelithRulesPolicy.EightLegTripBonus &&
                against == BebelithRulesPolicy
                    .PrintedCombatManeuverDefenseVersusTrip,
                new JObject {
                    ["versusTrip"] = against,
                    ["versusOtherManeuvers"] = baseline,
                    ["difference"] = against - baseline,
                    ["printedVersusTrip"] = BebelithRulesPolicy
                        .PrintedCombatManeuverDefenseVersusTrip,
                    ["printedBase"] = BebelithRulesPolicy
                        .PrintedCombatManeuverDefense,
                    ["sharedNativeFactDelivers"] =
                        BebelithRulesPolicy.SharedNativeTripBonus },
                "the printed +12, which the shared native eight-leg fact "
                + "cannot deliver: Sprint 20 measured it at +8 on a live "
                + "scorpion reading CMD 27 against a printed 31");
            int stealth = owner.Descriptor.Stats.SkillStealth.ModifiedValue;
            Sprint21Check("bebelith-racial-stealth-" +
                    (turnBased ? "turn-based" : "real-time"),
                stealth >= BebelithRulesPolicy.RacialStealthBonus,
                new JObject {
                    ["measured"] = stealth,
                    ["printedRacial"] = BebelithRulesPolicy.RacialStealthBonus,
                    ["modifiers"] = new JArray(owner.Descriptor.Stats
                        .SkillStealth.Modifiers
                        .Select(value => (JToken)(
                            value.ModDescriptor.ToString()
                            + "=" + value.ModValue))) },
                "the printed racial +8, which existed only in a derivation "
                + "before this sprint");
        }

        /// <summary>
        /// The authored body, attached and walking on six.
        ///
        /// <para>The one thing a Bebelith render cannot be allowed to be is
        /// the Doomspider it ships as today, and the difference a player sees
        /// first is the leg count. The mesh declares it; this reads the
        /// declaration back off the file the game actually loaded.</para>
        /// </summary>
        private void ReviewSprint21BebelithBody(UnitEntityData owner,
            bool turnBased)
        {
            string outcome = owner.View == null ? "no-view" :
                ExpandedSummoningPteranodonViewPatch.DescribeView(owner.View);
            string status;
            UnityEngine.Mesh mesh;
            string[] bones;
            UnityEngine.Texture2D albedo;
            bool loaded = PteranodonAssetRuntime.TryGetSprint14InsectVisual(
                Sprint14BonePolicy.BebelithKey, out mesh, out bones,
                out albedo, out status);
            var names = new HashSet<string>(bones ?? new string[0],
                StringComparer.Ordinal);
            int legs = new[] { 0, 1, 2, 3 }.Count(index =>
                names.Contains("L_Foot" + index) &&
                names.Contains("R_Foot" + index) &&
                names.Contains("L_Leg" + index + "_Upper") &&
                names.Contains("R_Leg" + index + "_Upper"));
            // The fourth chain must carry nothing at all. This is the single
            // difference a player sees first, and it is the whole reason the
            // authored body is not a repaint of the released one.
            bool fourthChainEmpty = !names.Contains("L_Foot3") &&
                !names.Contains("R_Foot3") &&
                !names.Contains("L_Leg3_Upper") &&
                !names.Contains("R_Leg3_Upper");
            bool foreclaws = names.Contains("pedipalp7_L") &&
                names.Contains("pedipalp7_R");
            bool jaw = names.Contains("chelicera_L") &&
                names.Contains("chelicera_R");
            bool abdomen = names.Contains("Tail3_M") &&
                names.Contains("UpperTorso");
            string[] reviewedBones = Sprint14BonePolicy.AllowedBones(
                Sprint14BonePolicy.BebelithKey);
            bool reviewed = bones != null && bones.All(name =>
                reviewedBones.Contains(name, StringComparer.Ordinal));
            bool attached = outcome.StartsWith("visual:attached;",
                StringComparison.Ordinal);
            Sprint21Check("bebelith-original-body-" +
                    (turnBased ? "turn-based" : "real-time"),
                loaded && attached && legs == 3 && fourthChainEmpty &&
                    foreclaws && jaw && abdomen && reviewed &&
                    mesh != null && mesh.vertexCount > 0,
                new JObject {
                    ["outcome"] = outcome,
                    ["status"] = status,
                    ["renderers"] = owner.View == null ? null :
                        DescribePteranodonRenderers(owner.View),
                    ["vertices"] = mesh == null ? 0 : mesh.vertexCount,
                    ["bones"] = bones == null ? 0 : bones.Length,
                    ["legChainsWeighted"] = legs,
                    ["visibleLegs"] = legs * 2,
                    ["fourthChainEmpty"] = fourthChainEmpty,
                    ["foreclawsOnPedipalps"] = foreclaws,
                    ["jawOnCheliceraeChain"] = jaw,
                    ["abdomenOnAbdomenChain"] = abdomen,
                    ["everyBoneReviewed"] = reviewed,
                    ["albedo"] = albedo == null ? null :
                        albedo.width + "x" + albedo.height,
                    ["donorWalksOn"] = 8 },
                "six legs, not eight. The fourth leg chain carries no "
                + "geometry at all, the foreclaws ride the pedipalp chains "
                + "and the jaw rides the cheliceræ - which together are what "
                + "keep this silhouette from being the Doomspider's");
        }

        /// <summary>
        /// Penetrating strike, and the control that is the only way to see
        /// it.
        ///
        /// <para>Against an ordinary target the damage must be chaotic and
        /// magic and must NOT be cold iron or good. Then the same target is
        /// made a demon - request-local, restored - and the two extra
        /// descriptors must appear. Running it in that order matters: the
        /// control comes first, so a universal grant fails before anything
        /// has arranged a demon to hide it.</para>
        /// </summary>
        private IEnumerable<int> ReviewSprint21BebelithStrike(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner,
            bool turnBased)
        {
            ItemEntityWeapon[] limbs = Sprint19Limbs(owner);
            int restoreAttack =
                owner.Descriptor.Stats.AdditionalAttackBonus.BaseValue;
            Alignment restoreAlignment =
                fixture.Hostile.Descriptor.Alignment.Value;
            foreach (bool asDemon in new[] { false, true })
            {
                var observer = new Sprint16RuleObserver {
                    Owner = owner, Target = fixture.Hostile };
                var accuracy = new Sprint18Accuracy {
                    Owner = owner, Hit = limbs };
                observer.BeforeAttackRollForFixture = accuracy.Apply;
                EventBus.Subscribe(observer);
                try
                {
                    ResetExpandedSummoningHostile(fixture);
                    if (asDemon)
                        fixture.Hostile.Descriptor.Alignment.Set(
                            Alignment.ChaoticEvil);
                    accuracy.Rest();
                    if (CombatController.IsInTurnBasedCombat())
                        foreach (int step in Sprint18AdvanceRound(owner))
                            yield return step;
                    UnitAttack attack = null;
                    foreach (int step in Sprint18RunFullAttack(fixture, owner,
                        result => attack = result)) yield return step;
                    for (int frame = 0; frame < 8; frame++) yield return 0;

                    RuleDealDamage[] landed = observer.Damage.Where(value =>
                        value != null && value.AttackRoll != null &&
                        ReferenceEquals(value.Initiator, owner)).ToArray();
                    JObject[] descriptors = landed
                        .Select(Sprint21Descriptors).ToArray();
                    bool anyLanded = descriptors.Length > 0;
                    bool chaoticEverywhere = anyLanded &&
                        descriptors.All(row => (bool)row["chaotic"]);
                    bool magicEverywhere = anyLanded && descriptors.All(row =>
                        (int)row["enchantment"] >=
                        ExpandedSummoningSpecialProfiles
                            .BebelithPenetratingEnhancement);
                    bool demonOnly = anyLanded && descriptors.All(row =>
                        (bool)row["good"] == asDemon &&
                        (bool)row["coldIron"] == asDemon);
                    Sprint21Check((asDemon
                            ? "bebelith-penetrating-strike-demon-"
                            : "bebelith-penetrating-strike-spares-non-demons-")
                            + (turnBased ? "turn-based" : "real-time"),
                        anyLanded && chaoticEverywhere && magicEverywhere &&
                            demonOnly,
                        new JObject {
                            ["targetIsDemon"] = asDemon,
                            ["targetAlignment"] = fixture.Hostile.Descriptor
                                .Alignment.Value.ToString(),
                            ["bundles"] = new JArray(descriptors
                                .Select(row => (JToken)row)),
                            ["commandFinished"] = attack != null &&
                                attack.IsFinished },
                        asDemon
                            ? "cold iron and good appear against a demon, on "
                            + "top of the chaotic and magic every natural "
                            + "attack already carries"
                            : "chaotic and magic on every natural attack, and "
                            + "no cold iron and no good on anything that is "
                            + "not a demon - the asymmetry a demon target "
                            + "cannot show");
                }
                finally
                {
                    EventBus.Unsubscribe(observer);
                    owner.Descriptor.Stats.AdditionalAttackBonus.BaseValue =
                        restoreAttack;
                    fixture.Hostile.Descriptor.Alignment.Set(restoreAlignment);
                    ResetExpandedSummoningHostile(fixture);
                }
                yield return 0;
            }
        }

        /// <summary>
        /// Rot: on the bite, and on nothing else.
        ///
        /// <para>Two cases. A bite that wounds must force a Fortitude save at
        /// the printed difficulty class; a claw that wounds must force
        /// nothing. The second is the one that matters, because a rot that
        /// leaks onto the claws is a creature doing six Constitution a round
        /// instead of two and nothing in the printed block says so.</para>
        /// </summary>
        private IEnumerable<int> ReviewSprint21BebelithRot(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner,
            bool turnBased)
        {
            ItemEntityWeapon[] limbs = Sprint19Limbs(owner);
            ItemEntityWeapon bite = limbs.FirstOrDefault();
            ItemEntityWeapon claw = limbs.Skip(1).FirstOrDefault();
            int restoreAttack =
                owner.Descriptor.Stats.AdditionalAttackBonus.BaseValue;
            foreach (bool withBite in new[] { true, false })
            {
                ItemEntityWeapon chosen = withBite ? bite : claw;
                var observer = new Sprint16RuleObserver {
                    Owner = owner, Target = fixture.Hostile };
                var saves = new Sprint19SaveObserver {
                    Target = fixture.Hostile };
                var accuracy = new Sprint18Accuracy {
                    Owner = owner, Hit = new[] { chosen } };
                observer.BeforeAttackRollForFixture = accuracy.Apply;
                EventBus.Subscribe(observer);
                EventBus.Subscribe(saves);
                try
                {
                    ResetExpandedSummoningHostile(fixture);
                    accuracy.Rest();
                    if (CombatController.IsInTurnBasedCombat())
                        foreach (int step in Sprint18AdvanceRound(owner))
                            yield return step;
                    UnitAttack attack = null;
                    foreach (int step in Sprint18RunFullAttack(fixture, owner,
                        result => attack = result)) yield return step;
                    for (int frame = 0; frame < 12; frame++) yield return 0;

                    RuleSavingThrow[] fortitude = saves.Saves.Where(save =>
                        save != null &&
                        save.Type == SavingThrowType.Fortitude).ToArray();
                    int expected = BebelithRulesPolicy.DifficultyClass(
                        BebelithRulesPolicy.HitDice,
                        owner.Descriptor.Stats.Constitution.Bonus);
                    bool ok = withBite
                        ? fortitude.Length > 0 &&
                          fortitude.All(save => save.DifficultyClass == expected)
                        : fortitude.Length == 0;
                    Sprint21Check((withBite
                            ? "bebelith-rot-rides-the-bite-"
                            : "bebelith-rot-spares-the-claw-")
                            + (turnBased ? "turn-based" : "real-time"), ok,
                        new JObject {
                            ["limb"] = withBite ? "bite" : "claw",
                            ["weapon"] = chosen == null ? null
                                : chosen.Blueprint.name,
                            ["fortitudeSaves"] = fortitude.Length,
                            ["difficultyClasses"] = new JArray(fortitude
                                .Select(save =>
                                    (JToken)save.DifficultyClass)),
                            ["printedDifficultyClass"] = expected,
                            ["liveConstitutionBonus"] =
                                owner.Descriptor.Stats.Constitution.Bonus,
                            ["printedExposures"] =
                                BebelithRulesPolicy.RotExposures,
                            ["printedCureSaves"] =
                                BebelithRulesPolicy.RotCureSaves,
                            ["commandFinished"] = attack != null &&
                                attack.IsFinished },
                        withBite
                            ? "a bite that wounds forces a Fortitude save at "
                            + "the printed difficulty class, derived live "
                            + "from this creature's own Constitution"
                            : "a claw forces nothing: rot that leaked onto "
                            + "the claws would be six Constitution a round "
                            + "where the block prints two");
                }
                finally
                {
                    EventBus.Unsubscribe(saves);
                    EventBus.Unsubscribe(observer);
                    owner.Descriptor.Stats.AdditionalAttackBonus.BaseValue =
                        restoreAttack;
                    ResetExpandedSummoningHostile(fixture);
                }
                yield return 0;
            }
        }

        /// <summary>
        /// Dismantle Armor: the sprint's highest-risk mechanic, with both of
        /// its mandatory absences.
        ///
        /// <para>Three cases. An unarmoured target must see no manoeuvre at
        /// all - not a failed one, none - which is the clause that keeps the
        /// mechanic off a creature with nothing to dismantle. Then one
        /// request-local disposable armour is equipped and both claws are
        /// made to land: the engine's own sunder manoeuvre must be rolled,
        /// and whether it succeeds is the dice. And either way the target's
        /// whole equipment census must be byte-identical before and after,
        /// which is the whole of "no permanent inventory corruption" in one
        /// comparison.</para>
        /// </summary>
        private IEnumerable<int> ReviewSprint21BebelithDismantle(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner,
            bool turnBased)
        {
            ItemEntityWeapon[] limbs = Sprint19Limbs(owner);
            ItemEntityWeapon[] claws = limbs.Skip(1).ToArray();
            int restoreAttack =
                owner.Descriptor.Stats.AdditionalAttackBonus.BaseValue;
            foreach (bool armoured in new[] { false, true })
            {
                var observer = new Sprint16RuleObserver {
                    Owner = owner, Target = fixture.Hostile };
                var accuracy = new Sprint18Accuracy {
                    Owner = owner, Hit = claws };
                observer.BeforeAttackRollForFixture = accuracy.Apply;
                EventBus.Subscribe(observer);
                string before = null;
                try
                {
                    ResetExpandedSummoningHostile(fixture);
                    bool hasArmor = BebelithDismantle.HasEligibleArmor(
                        fixture.Hostile);
                    bool hasShield = BebelithDismantle.HasEligibleShield(
                        fixture.Hostile);
                    // The target is taken as the fixture built it. If it
                    // happens to be armoured the unarmoured control cannot
                    // run, and the review says so rather than arranging the
                    // absence it is supposed to be observing.
                    if (armoured != (hasArmor || hasShield))
                    {
                        Sprint21Check((armoured
                                ? "bebelith-dismantle-needs-both-claws-"
                                : "bebelith-dismantle-spares-the-unarmoured-")
                                + (turnBased ? "turn-based" : "real-time"),
                            !armoured,
                            new JObject {
                                ["case"] = armoured ? "armoured"
                                    : "unarmoured",
                                ["targetHasArmor"] = hasArmor,
                                ["targetHasShield"] = hasShield,
                                ["ranCase"] = false,
                                ["equipment"] = Sprint21EquipmentCensus(
                                    fixture.Hostile) },
                            armoured
                                ? "the disposable target carries no eligible "
                                + "equipment, so the positive dismantle case "
                                + "cannot be arranged without owner gear and "
                                + "is recorded as not run"
                                : "a target with nothing equipped is the "
                                + "control, and it is the state the fixture "
                                + "already provides");
                        continue;
                    }

                    before = Sprint21EquipmentCensus(fixture.Hostile);
                    accuracy.Rest();
                    if (CombatController.IsInTurnBasedCombat())
                        foreach (int step in Sprint18AdvanceRound(owner))
                            yield return step;
                    UnitAttack attack = null;
                    foreach (int step in Sprint18RunFullAttack(fixture, owner,
                        result => attack = result)) yield return step;
                    for (int frame = 0; frame < 12; frame++) yield return 0;

                    RuleCombatManeuver[] sunders = observer.Checks.Where(
                        check => check != null &&
                        check.Type == CombatManeuver.SunderArmor &&
                        ReferenceEquals(check.Initiator, owner)).ToArray();
                    string after = Sprint21EquipmentCensus(fixture.Hostile);
                    bool unchanged = string.Equals(before, after,
                        StringComparison.Ordinal);
                    bool ok = armoured
                        ? sunders.Length > 0 && unchanged
                        : sunders.Length == 0 && unchanged;
                    Sprint21Check((armoured
                            ? "bebelith-dismantle-needs-both-claws-"
                            : "bebelith-dismantle-spares-the-unarmoured-")
                            + (turnBased ? "turn-based" : "real-time"), ok,
                        new JObject {
                            ["case"] = armoured ? "armoured" : "unarmoured",
                            ["ranCase"] = true,
                            ["sunderManeuvers"] = sunders.Length,
                            ["sunderSucceeded"] = new JArray(sunders
                                .Select(check => (JToken)check.Success)),
                            ["equipmentBefore"] = before,
                            ["equipmentAfter"] = after,
                            ["equipmentUnchanged"] = unchanged,
                            ["clawsLanded"] = observer.Attacks.Count(roll =>
                                roll != null && roll.IsHit),
                            ["commandFinished"] = attack != null &&
                                attack.IsFinished },
                        armoured
                            ? "both claws landing attempt the engine's own "
                            + "sunder manoeuvre, and whether it beats the "
                            + "target's defence is the dice - but the "
                            + "target's equipment is identical either way, "
                            + "which is the whole of no inventory corruption"
                            : "a target with nothing equipped sees no "
                            + "manoeuvre at all: not a failed one, none");
                }
                finally
                {
                    EventBus.Unsubscribe(observer);
                    owner.Descriptor.Stats.AdditionalAttackBonus.BaseValue =
                        restoreAttack;
                    ResetExpandedSummoningHostile(fixture);
                }
                yield return 0;
            }
        }
    }
}
