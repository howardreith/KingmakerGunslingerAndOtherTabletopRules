using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Blueprints.Items.Armors;
using KingmakerGunslinger.Bootstrap;
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
                ReviewSprint21BebelithBody(owner, turnBased);
                foreach (int step in ReviewSprint21BebelithStrike(
                    fixture, owner, turnBased)) yield return step;
                // After the creature has acted, which is the state a printed
                // stat block describes. The second guarded run read its
                // combat-manoeuvre defence at 33 against a printed 34 and
                // the engine's own working said why: the Dexterity bonus was
                // denied because the creature was still flat-footed. The
                // crab's review already orders its reading this way.
                ReviewSprint21BebelithCarriers(fixture, owner, turnBased);
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
            // The fixture's armour comes off here, not in the case that put
            // it on. Three guarded runs measured Kingmaker refusing to
            // unequip armour while the fight is live - `permitted=False` -
            // and permitting it once the fight is over, which is why the
            // next case's own strip always succeeded and the same-frame one
            // never did. So the restoration is asserted where the engine
            // allows it: after the creature is dismissed and the combat it
            // was in has ended.
            string strip = Sprint21StripFixtureArmor(fixture.Hostile);
            string bareAgain = Sprint21EquipmentCensus(fixture.Hostile);
            Sprint21Check("bebelith-dismantle-fixture-restored-" +
                    (turnBased ? "turn-based" : "real-time"),
                string.Equals(bareAgain, "equipped=none",
                    StringComparison.Ordinal),
                new JObject {
                    ["strip"] = strip, ["equipment"] = bareAgain },
                "the request-local armour this fixture equipped is gone and "
                + "the disposable target is bare again. The engine refuses "
                + "to unequip armour mid-combat, so the removal is taken "
                + "where it is permitted rather than asserted where it is "
                + "not");
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
            // The two numbers the chassis correction exists for, beside the
            // one it leaves alone. The first guarded run read Fortitude +11
            // against a printed +15 and 162 hit points against a printed
            // 150, with Reflex already right, so all three are asserted here
            // rather than merely recorded.
            Sprint21Check("bebelith-printed-saves-and-hit-points-" +
                    (turnBased ? "turn-based" : "real-time"),
                live.Stats.SaveFortitude.ModifiedValue == printed.Fortitude &&
                live.Stats.SaveReflex.ModifiedValue == printed.Reflex &&
                live.Stats.HitPoints.ModifiedValue == printed.HitPoints,
                new JObject {
                    ["printedFortitude"] = printed.Fortitude,
                    ["measuredFortitude"] =
                        live.Stats.SaveFortitude.ModifiedValue,
                    ["printedReflex"] = printed.Reflex,
                    ["measuredReflex"] = live.Stats.SaveReflex.ModifiedValue,
                    ["printedHitPoints"] = printed.HitPoints,
                    ["measuredHitPoints"] =
                        live.Stats.HitPoints.ModifiedValue,
                    ["chassisSaveSwap"] =
                        BebelithChassisCorrection.SaveSwap,
                    ["chassisHitPointCorrection"] =
                        BebelithChassisCorrection.HitPointCorrection },
                "the printed Fortitude, Reflex and hit points. The native "
                + "outsider chassis assigns this creature's Fortitude and "
                + "Will to each other's slots and is one point a die "
                + "tougher than its block, and this is where the "
                + "creature-owned correction is proved rather than assumed");
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
            // The engine's own working, not just the two totals. A total
            // one point from the printed number cannot be explained without
            // knowing which term produced it, and the first run left exactly
            // that question open.
            Sprint14CmdBreakdown tripParts = ProbeCombatManeuverDefenceParts(
                fixture.Hostile, owner, CombatManeuver.Trip);
            Sprint14CmdBreakdown ordinaryParts =
                ProbeCombatManeuverDefenceParts(fixture.Hostile, owner,
                    CombatManeuver.BullRush);
            int against = tripParts.Result;
            int baseline = ordinaryParts.Result;
            Sprint21Check("bebelith-eight-legged-trip-defence-" +
                    (turnBased ? "turn-based" : "real-time"),
                against - baseline == BebelithRulesPolicy.EightLegTripBonus &&
                !tripParts.FlatFooted &&
                against == BebelithRulesPolicy
                    .PrintedCombatManeuverDefenseVersusTrip &&
                baseline == BebelithRulesPolicy
                    .PrintedCombatManeuverDefense,
                new JObject {
                    ["versusTrip"] = tripParts.Describe("CMD vs trip"),
                    ["versusOtherManeuvers"] =
                        ordinaryParts.Describe("CMD"),
                    ["liveVersusTrip"] = against,
                    ["liveOrdinary"] = baseline,
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
            // The printed block states a racial MODIFIER, not a total. A
            // Huge creature carries a -8 size penalty on Stealth, so a
            // correct Bebelith reads +1 overall and the first version of
            // this check failed a creature that was right.
            int stealth = owner.Descriptor.Stats.SkillStealth.ModifiedValue;
            int racial = owner.Descriptor.Stats.SkillStealth.Modifiers
                .Where(value => value.ModDescriptor == ModifierDescriptor.Racial)
                .Sum(value => value.ModValue);
            Sprint21Check("bebelith-racial-stealth-" +
                    (turnBased ? "turn-based" : "real-time"),
                racial == BebelithRulesPolicy.RacialStealthBonus,
                new JObject {
                    ["total"] = stealth,
                    ["racialModifier"] = racial,
                    ["printedRacial"] = BebelithRulesPolicy.RacialStealthBonus,
                    ["modifiers"] = new JArray(owner.Descriptor.Stats
                        .SkillStealth.Modifiers
                        .Select(value => (JToken)(
                            value.ModDescriptor.ToString()
                            + "=" + value.ModValue))) },
                "the printed racial +8, which existed only in a derivation "
                + "before this sprint. The total is lower because a Huge "
                + "creature carries a size penalty on Stealth, and the "
                + "printed block states the racial modifier");
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
            // A demon is a chaotic evil OUTSIDER, and the discrimination
            // correctly requires both. The first run arranged only the
            // alignment, so the control measured penetrating strike refusing
            // a chaotic evil non-outsider - which is right, and is not the
            // question the case was asking.
            BlueprintUnitFact outsiderType = BlueprintBootstrap.Library == null
                ? null
                : BlueprintBootstrap.Library.BlueprintsByAssetId.TryGetValue(
                    "9054d3988d491d944ac144e27b6bc318", out BlueprintScriptableObject found)
                    ? found as BlueprintUnitFact : null;
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
                    {
                        fixture.Hostile.Descriptor.Alignment.Set(
                            Alignment.ChaoticEvil);
                        if (outsiderType != null &&
                            !fixture.Hostile.Descriptor.HasFact(outsiderType))
                            fixture.Hostile.Descriptor.AddFact(outsiderType);
                    }
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
                    if (outsiderType != null)
                        fixture.Hostile.Descriptor.RemoveFact(outsiderType);
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
                    // Rot's saves, split by what forced them. The
                    // APPLICATION save comes from the rot fact and is the
                    // one the printed difficulty class describes. The saves
                    // the applied state forces afterwards are a different
                    // question: Kingmaker stacks poison, each bite is
                    // another dose, and three guarded runs measured them at
                    // four and eight above the single-dose number. That is
                    // correct, so they are recorded rather than required.
                    RuleSavingThrow[] mine = fortitude.Where(save =>
                        save.Reason.Fact != null &&
                        save.Reason.Fact.Blueprint != null &&
                        save.Reason.Fact.Blueprint.name.IndexOf(
                            "Bebelith", StringComparison.OrdinalIgnoreCase)
                            >= 0).ToArray();
                    RuleSavingThrow[] applications = mine.Where(save =>
                        save.Reason.Fact.Blueprint.name.IndexOf(
                            "RotState", StringComparison.OrdinalIgnoreCase)
                            < 0).ToArray();
                    bool ok = withBite
                        ? mine.Length > 0 &&
                          applications.All(save =>
                              save.DifficultyClass == expected)
                        : mine.Length == 0;
                    Sprint21Check((withBite
                            ? "bebelith-rot-rides-the-bite-"
                            : "bebelith-rot-spares-the-claw-")
                            + (turnBased ? "turn-based" : "real-time"), ok,
                        new JObject {
                            ["limb"] = withBite ? "bite" : "claw",
                            ["weapon"] = chosen == null ? null
                                : chosen.Blueprint.name,
                            ["fortitudeSaves"] = fortitude.Length,
                            ["rotApplicationSaves"] = applications.Length,
                            ["rotApplicationDifficultyClasses"] = new JArray(
                                applications.Select(save =>
                                    (JToken)save.DifficultyClass)),
                            ["rotSaves"] = fortitude.Count(save =>
                                save.Reason.Fact != null &&
                                save.Reason.Fact.Blueprint != null &&
                                save.Reason.Fact.Blueprint.name.IndexOf(
                                    "Bebelith",
                                    StringComparison.OrdinalIgnoreCase) >= 0),
                            // Every save names its source. The first run
                            // saw difficulty classes of 27, 31 and 33 beside
                            // rot's 23 and could not say what produced them,
                            // which is half a measurement.
                            ["saves"] = new JArray(fortitude.Select(save =>
                                (JToken)new JObject {
                                    ["difficultyClass"] = save.DifficultyClass,
                                    ["fact"] = save.Reason.Fact == null ? null
                                        : save.Reason.Fact.Blueprint == null
                                            ? "?"
                                            : save.Reason.Fact.Blueprint.name,
                                    ["caster"] = save.Reason.Caster == null
                                        ? null
                                        : save.Reason.Caster.Blueprint == null
                                            ? "?"
                                            : save.Reason.Caster.Blueprint.name,
                                    ["passed"] = save.IsPassed })),
                            ["printedDifficultyClass"] = expected,
                            // What the state's own component pinned, beside
                            // the difficulty class the engine then used. Two
                            // numbers, because one cannot say whether a pin
                            // failed to reach the save or the save added to
                            // it.
                            ["statePin"] =
                                BebelithRotStateDifficultyClass.LastPin,
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
                            + "from this creature's own Constitution. The "
                            + "applied state's later saves come out higher "
                            + "because Kingmaker stacks poison and each bite "
                            + "is another dose, which is correct and is "
                            + "recorded rather than required"
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

        /// <summary>
        /// Takes this fixture's armour back off a disposable target, and
        /// says what the engine allowed.
        ///
        /// <para>The sixth guarded run found a suit still equipped after the
        /// case that put it there, which turned the next mode's unarmoured
        /// control into an armoured one. Kingmaker refuses to unequip armour
        /// in some states, and a fixture that cannot report the refusal
        /// leaks it into the next case's evidence instead.</para>
        /// </summary>
        private static string Sprint21StripFixtureArmor(UnitEntityData unit)
        {
            ArmorSlot slot = unit == null || unit.Descriptor == null ? null
                : unit.Descriptor.Body.Armor;
            if (slot == null) return "no-armour-slot";
            if (!slot.HasArmor) return "already-bare";
            bool permitted = slot.CanRemoveItem();
            bool removed = slot.RemoveItem(false);
            if (slot.HasArmor) removed = slot.RemoveItem(true) || removed;
            return "permitted=" + permitted + ";removed=" + removed +
                ";stillEquipped=" + slot.HasArmor;
        }

        /// <summary>
        /// The one native armour this fixture equips on a disposable target
        /// so Dismantle Armor has something to catch.
        ///
        /// <para>Pinned rather than searched, and pinned to an identity a
        /// released scenario already uses: a fixture that picked whichever
        /// armour the library happened to return first would measure a
        /// different creature on a different installation.</para>
        /// </summary>
        private const string Sprint21DisposableArmorGuid =
            "559b0b6f194656c428c403a000ceee78";

        /// <summary>
        /// Dismantle Armor: the sprint's highest-risk mechanic, with both of
        /// its mandatory absences.
        ///
        /// <para>Three things are measured. An unarmoured target must see no
        /// manoeuvre at all - not a failed one, none - which is the clause
        /// that keeps the mechanic off a creature with nothing to dismantle.
        /// An armoured one must see the engine's own sunder manoeuvre rolled,
        /// and whether it beats the target's defence is the dice. And in
        /// both cases the target's whole equipment census must be identical
        /// before and after, which is the whole of "no permanent inventory
        /// corruption" in one comparison.</para>
        ///
        /// <para>The armour is request-local: one pinned native asset, on the
        /// disposable hostile this fixture created, removed in the finally
        /// block, with a third census proving the removal. No owner party
        /// member is touched and no equipped item is mutated.</para>
        /// </summary>
        private IEnumerable<int> ReviewSprint21BebelithDismantle(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner,
            bool turnBased)
        {
            ItemEntityWeapon[] limbs = Sprint19Limbs(owner);
            ItemEntityWeapon[] claws = limbs.Skip(1).ToArray();
            int restoreAttack =
                owner.Descriptor.Stats.AdditionalAttackBonus.BaseValue;
            BlueprintScriptableObject found;
            BlueprintItemArmor armorBlueprint =
                BlueprintBootstrap.Library != null &&
                BlueprintBootstrap.Library.BlueprintsByAssetId.TryGetValue(
                    Sprint21DisposableArmorGuid, out found)
                    ? found as BlueprintItemArmor : null;
            foreach (bool armoured in new[] { false, true })
            {
                var observer = new Sprint16RuleObserver {
                    Owner = owner, Target = fixture.Hostile };
                var accuracy = new Sprint18Accuracy {
                    Owner = owner, Hit = claws };
                observer.BeforeAttackRollForFixture = accuracy.Apply;
                EventBus.Subscribe(observer);
                string bare = null, before = null, after = null;
                bool equipped = false;
                string removal = "not-attempted";
                try
                {
                    ResetExpandedSummoningHostile(fixture);
                    // Clear anything an earlier case left equipped before
                    // reading the bare state. Kingmaker refuses to unequip
                    // armour in some states, and the sixth guarded run found
                    // a leftover suit turning the next mode's unarmoured
                    // control into an armoured one.
                    removal = Sprint21StripFixtureArmor(fixture.Hostile);
                    bare = Sprint21EquipmentCensus(fixture.Hostile);
                    if (armoured)
                    {
                        if (armorBlueprint == null)
                        {
                            Sprint21Check("bebelith-dismantle-needs-both-claws-"
                                    + (turnBased ? "turn-based" : "real-time"),
                                false,
                                new JObject {
                                    ["case"] = "armoured",
                                    ["ranCase"] = false,
                                    ["armorBlueprint"] =
                                        Sprint21DisposableArmorGuid,
                                    ["reason"] = "the pinned disposable "
                                        + "armour is not in this library" },
                                "the positive dismantle case needs one "
                                + "request-local armour and will not "
                                + "substitute owner gear for it");
                            continue;
                        }
                        fixture.Hostile.Descriptor.Body.Armor.InsertItem(
                            new ItemEntityArmor(armorBlueprint));
                        equipped = true;
                    }

                    bool hasArmor = BebelithDismantle.HasEligibleArmor(
                        fixture.Hostile);
                    bool hasShield = BebelithDismantle.HasEligibleShield(
                        fixture.Hostile);
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
                    after = Sprint21EquipmentCensus(fixture.Hostile);
                    bool unchanged = string.Equals(before, after,
                        StringComparison.Ordinal);
                    int clawsLanded = observer.Attacks.Count(roll =>
                        roll != null && roll.IsHit && roll.Weapon != null &&
                        claws.Any(claw => ReferenceEquals(roll.Weapon, claw)));
                    // Both claws have to land before the printed trigger can
                    // fire at all, so a round where the fixture could only
                    // land one is reported rather than failed: that is the
                    // dice, not the rule.
                    // The control also asserts its own precondition: a
                    // target that arrived already wearing something is not an
                    // unarmoured control, however the armour got there.
                    bool ok = armoured
                        ? unchanged && (clawsLanded < 2 || sunders.Length > 0)
                        : unchanged && sunders.Length == 0 &&
                          !hasArmor && !hasShield &&
                          string.Equals(bare, "equipped=none",
                              StringComparison.Ordinal);
                    Sprint21Check((armoured
                            ? "bebelith-dismantle-needs-both-claws-"
                            : "bebelith-dismantle-spares-the-unarmoured-")
                            + (turnBased ? "turn-based" : "real-time"), ok,
                        new JObject {
                            ["case"] = armoured ? "armoured" : "unarmoured",
                            ["ranCase"] = true,
                            ["targetHasArmor"] = hasArmor,
                            ["targetHasShield"] = hasShield,
                            ["sunderManeuvers"] = sunders.Length,
                            ["sunderSucceeded"] = new JArray(sunders
                                .Select(check => (JToken)check.Success)),
                            ["clawsLanded"] = clawsLanded,
                            ["equipmentBare"] = bare,
                            ["equipmentBefore"] = before,
                            ["equipmentAfter"] = after,
                            ["equipmentUnchanged"] = unchanged,
                            ["fixtureArmorRemoval"] = removal,
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
                    if (equipped)
                        removal = Sprint21StripFixtureArmor(fixture.Hostile);
                    EventBus.Unsubscribe(observer);
                    owner.Descriptor.Stats.AdditionalAttackBonus.BaseValue =
                        restoreAttack;
                    ResetExpandedSummoningHostile(fixture);
                }
                for (int settle = 0; settle < 12; settle++) yield return 0;
                yield return 0;
            }
        }
    }
}
