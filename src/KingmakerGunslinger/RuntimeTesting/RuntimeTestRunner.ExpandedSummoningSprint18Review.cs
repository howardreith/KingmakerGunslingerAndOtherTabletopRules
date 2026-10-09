using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Commands;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// The Sprint 18 hidden-candidate runtime review: what the two apes
    /// actually are in a running game.
    ///
    /// <para>Everything here reads a live creature summoned through the
    /// registered execution into the guarded working save and attacks a
    /// disposable hostile with real commands and native rules. The fixture
    /// raises only this disposable attacker's accuracy and only this
    /// disposable target's hit points, and restores both; no result is
    /// forced, no rule is replayed and no damage is manufactured.</para>
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        private const int Sprint18SettleFrames = 240;
        private const int Sprint18AttackFrames = 900;

        private ExpandedSummoningCorrectionFixture _primateFixture;
        private IEnumerator<int> _primateSteps;
        private readonly List<RuntimeTestAssertion> _primateAssertions =
            new List<RuntimeTestAssertion>();
        private readonly JArray _primateRows = new JArray();
        private int _primateLoadingFrames;
        private bool _primateComplete;
        private UnityEngine.Object[] _primateOwned = new UnityEngine.Object[0];
        private string _primateOwnedKey;

        private void PollSprint18Review()
        {
            if (_primateComplete) return;
            try
            {
                if (_primateSteps == null)
                {
                    string loading;
                    if (ExpandedSummoningLoadingActive(out loading))
                    {
                        if (++_primateLoadingFrames < ExpandedSummoningLoadingGateFrames) return;
                        throw new InvalidOperationException("Native loading did not settle: " + loading);
                    }
                    _primateFixture = BeginExpandedSummoningCorrectionFixture(
                        "KMG_Runtime_Sprint18_Caster");
                    _primateSteps = ReviewSprint18(_primateFixture).GetEnumerator();
                }
                if (_primateSteps.MoveNext()) return;
            }
            catch (Exception error)
            {
                Sprint18Check(_primateAssertions, _primateRows,
                    "review-exception", false, new JObject {
                        ["exception"] = DescribeExpandedSummoningCorrectionException(error) },
                    "the whole Sprint 18 review completes without an exception");
            }
            FinishSprint18Review();
        }

        private static void Sprint18Check(List<RuntimeTestAssertion> assertions,
            JArray rows, string name, bool passed, JObject row, string expected)
        {
            row["case"] = name;
            row["passed"] = passed;
            rows.Add(row);
            assertions.Add(Assertion("sprint18-" + name, expected,
                row.ToString(Formatting.None), passed,
                "request-local live apes; native rules and the registered execution"));
        }

        private IEnumerable<int> ReviewSprint18(ExpandedSummoningCorrectionFixture fixture)
        {
            bool pause = Game.Instance.IsPaused;
            bool turnBasedSetting =
                Kingmaker.UI.SettingsUI.SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            UnityEngine.Random.State random = UnityEngine.Random.state;
            try
            {
                Game.Instance.IsPaused = false;
                CreateExpandedSummoningCorrectionHostile(fixture);
                bool profiled = false;
                foreach (bool turnBased in PrimateReviewPolicy.CombatModes)
                {
                    Kingmaker.UI.SettingsUI.SettingsRoot.Instance
                        .EnableTurnBasedMode.CurrentValue = turnBased;
                    Game.Instance.TurnBasedCombatController.Activate();
                    foreach (string key in PrimateVisualPolicy.Keys)
                    {
                        UnitEntityData owner = CastExpandedSummoningOwnTier(fixture, key);
                        SetExpandedSummoningBrainActive(owner, false);
                        int settle = 0;
                        while (++settle <= Sprint18SettleFrames)
                        {
                            if (Game.Instance.IsPaused) Game.Instance.IsPaused = false;
                            yield return 0;
                            if (owner.View != null && owner.Descriptor.State.CanAct) break;
                        }
                        try
                        {
                            if (!profiled)
                                ReviewSprint18Body(owner, key,
                                    _primateAssertions, _primateRows);
                            foreach (int step in ReviewSprint18Routine(
                                fixture, owner, key, turnBased)) yield return step;
                            // Only now is the creature in combat and no
                            // longer flat-footed, which is the state a
                            // printed stat block describes. Reading it
                            // before it had acted denied it its Dexterity in
                            // armour class, touch and combat manoeuvre
                            // defence, and the first run measured exactly
                            // that.
                            if (!profiled)
                                ReviewSprint18Profile(fixture, owner, key,
                                    _primateAssertions, _primateRows);
                            if (key == PrimateRulesPolicy.DireApeKey)
                                foreach (int step in ReviewSprint18Rend(
                                    fixture, owner, turnBased)) yield return step;
                        }
                        finally
                        {
                            _primateOwned = ReviewSprint18OwnedResources(owner);
                            _primateOwnedKey = profiled ? null : key;
                            ResetExpandedSummoningHostile(fixture);
                            DireApeRendGate.ForgetObserved(owner);
                            DisposeExpandedSummoningUnits(fixture.Created, new[] { owner });
                        }
                        // Native views are destroyed at the end of a frame,
                        // so the instance's resources are still alive in the
                        // frame its unit was dismissed in. Let the native
                        // teardown run before counting what survived.
                        for (int teardown = 0; teardown < 8; teardown++) yield return 0;
                        if (_primateOwnedKey != null)
                            ReviewSprint18Release(_primateOwnedKey, _primateOwned,
                                _primateAssertions, _primateRows);
                        _primateOwned = new UnityEngine.Object[0];
                        _primateOwnedKey = null;
                    }
                    profiled = true;
                }
            }
            finally
            {
                UnityEngine.Random.state = random;
                Kingmaker.UI.SettingsUI.SettingsRoot.Instance
                    .EnableTurnBasedMode.CurrentValue = turnBasedSetting;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = pause;
            }
        }

        /// <summary>
        /// The printed stat block, read off the live creature: scores, size,
        /// hit dice, hit points, the three armour classes, the three saves,
        /// combat manoeuvre defence, every limb's attack bonus and damage,
        /// and the skills - including the Climb that is honestly absent.
        /// </summary>
        private static void ReviewSprint18Profile(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner, string key,
            List<RuntimeTestAssertion> assertions, JArray rows)
        {
            PrimateLiveProfile expected = PrimateReviewPolicy.For(key);
            var stats = owner.Descriptor.Stats;
            int[] scores = new[] { StatType.Strength, StatType.Dexterity,
                StatType.Constitution, StatType.Intelligence, StatType.Wisdom,
                StatType.Charisma }.Select(stat => stats.GetStat(stat).ModifiedValue).ToArray();
            ItemEntityWeapon[] limbs = Sprint18Limbs(owner);
            var limbRows = new JArray();
            bool limbsExact = limbs.Length == expected.Limbs.Length;
            for (int index = 0; index < limbs.Length; index++)
            {
                RuleCalculateWeaponStats calculated = Rulebook.Trigger(
                    new RuleCalculateWeaponStats(owner, limbs[index], null));
                BaseDamage damage = calculated.DamageDescription[0].CreateDamage();
                int attack = ProbeEntityAttackBonus(owner, fixture.Hostile, limbs[index]);
                int[] want = index < expected.Limbs.Length ? expected.Limbs[index] : new[] { 0, 0, 0 };
                bool exact = attack == expected.AttackBonus &&
                    damage.Dice.Rolls == want[0] && (int)damage.Dice.Dice == want[1] &&
                    damage.Bonus == want[2] && !limbs[index].IsSecondary &&
                    !calculated.SecondaryWeapon;
                limbsExact &= exact;
                limbRows.Add(new JObject {
                    ["limb"] = index, ["weapon"] = limbs[index].Blueprint.AssetGuid,
                    ["attackBonus"] = attack, ["damage"] = Sprint16DamageLine(damage),
                    ["secondaryEntity"] = limbs[index].IsSecondary,
                    ["secondaryRule"] = calculated.SecondaryWeapon,
                    ["matches"] = exact });
            }
            Sprint14CmdBreakdown cmd = ProbeCombatManeuverDefenceParts(
                fixture.Hostile, owner, CombatManeuver.BullRush);
            // The player's difficulty setting puts a Difficulty-descriptor
            // modifier on this creature's armour class, and the engine
            // computes combat manoeuvre defence from touch armour class, so
            // the same term reaches all four defensive numbers exactly once.
            // The printed stat block does not include it and no creature in
            // the game escapes it, so it is subtracted and named.
            int difficulty = stats.AC.Modifiers.Where(modifier =>
                modifier.ModDescriptor == ModifierDescriptor.Difficulty)
                .Sum(modifier => modifier.ModValue);
            int deniedDex = cmd.DexterityDenied || cmd.FlatFooted ?
                stats.Dexterity.Bonus : 0;
            int stealth = stats.GetStat(StatType.SkillStealth).ModifiedValue;
            bool exactProfile = scores.SequenceEqual(new[] { expected.Strength,
                    expected.Dexterity, expected.Constitution, expected.Intelligence,
                    expected.Wisdom, expected.Charisma }) &&
                owner.Descriptor.State.Size == Size.Large &&
                owner.Descriptor.Progression.CharacterLevel == expected.HitDice &&
                stats.HitPoints.ModifiedValue == expected.HitPoints &&
                stats.AC.ModifiedValue - difficulty == expected.ArmorClass &&
                stats.AC.Touch - difficulty == expected.Touch &&
                stats.AC.FlatFooted - difficulty == expected.FlatFooted &&
                stats.GetStat(StatType.SaveFortitude).ModifiedValue == expected.Fortitude &&
                stats.GetStat(StatType.SaveReflex).ModifiedValue == expected.Reflex &&
                stats.GetStat(StatType.SaveWill).ModifiedValue == expected.Will &&
                cmd.Result + deniedDex - difficulty == expected.CombatManeuverDefense &&
                stats.GetStat(StatType.SkillMobility).ModifiedValue == expected.Mobility &&
                stats.GetStat(StatType.SkillPerception).ModifiedValue == expected.Perception &&
                (expected.Stealth.HasValue
                    ? stealth == expected.Stealth.Value
                    : stats.GetStat(StatType.SkillStealth).BaseValue == 0) &&
                limbsExact;
            Sprint18Check(assertions, rows, key + "-live-profile", exactProfile, new JObject {
                ["scores"] = new JArray(scores),
                ["size"] = owner.Descriptor.State.Size.ToString(),
                ["hitDice"] = owner.Descriptor.Progression.CharacterLevel,
                ["hitPoints"] = stats.HitPoints.ModifiedValue,
                ["armor"] = stats.AC.ModifiedValue, ["touch"] = stats.AC.Touch,
                ["flatFooted"] = stats.AC.FlatFooted,
                ["fortitude"] = stats.GetStat(StatType.SaveFortitude).ModifiedValue,
                ["reflex"] = stats.GetStat(StatType.SaveReflex).ModifiedValue,
                ["will"] = stats.GetStat(StatType.SaveWill).ModifiedValue,
                ["cmd"] = cmd.Describe("CMD"), ["deniedDexRecovered"] = deniedDex,
                ["armorModifiers"] = DescribeSprint18Modifiers(stats.AC),
                ["armorDexterityBonus"] = stats.AC.DexterityBonus,
                ["additionalCmd"] = stats.AdditionalCMD.ModifiedValue,
                ["nativeDifficultyTerm"] = difficulty,
                ["armorNetOfDifficulty"] = stats.AC.ModifiedValue - difficulty,
                ["touchNetOfDifficulty"] = stats.AC.Touch - difficulty,
                ["flatFootedNetOfDifficulty"] = stats.AC.FlatFooted - difficulty,
                ["cmdNetOfDifficulty"] = cmd.Result + deniedDex - difficulty,
                ["limbs"] = limbRows,
                ["mobility"] = DescribeSprint16Skill(stats.GetStat(StatType.SkillMobility)),
                ["perception"] = DescribeSprint16Skill(stats.GetStat(StatType.SkillPerception)),
                ["stealth"] = DescribeSprint16Skill(stats.GetStat(StatType.SkillStealth)),
                // The printed Climb this project does not represent, recorded
                // as a number rather than a silence. Nothing stands in for it.
                ["printedClimbOmitted"] = PrimateRulesPolicy.For(key).PrintedClimbSkill,
                ["athleticsUnraised"] = stats.GetStat(StatType.SkillAthletics).BaseValue,
                ["unitType"] = owner.Blueprint.Type == null ? null : owner.Blueprint.Type.name,
            }, "the live creature is the printed stat block, limb for limb");
        }

        /// <summary>
        /// The original body: attached, project-owned, with the donor's
        /// equipment skin blanked and nothing shared touched.
        /// </summary>
        private static void ReviewSprint18Body(UnitEntityData owner, string key,
            List<RuntimeTestAssertion> assertions, JArray rows)
        {
            var attachment = owner.View == null ? null :
                owner.View.GetComponent<PrimateVisualAttachment>();
            var skins = owner.View == null ? new SkinnedMeshRenderer[0] :
                owner.View.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            SkinnedMeshRenderer equipment = skins.FirstOrDefault(value =>
                value.name == PrimateVisualPolicy.EquipmentRenderer);
            string stem = "KMG_" + key + "_Original_";
            bool original = attachment != null && attachment.OriginalBodyLive &&
                attachment.Body != null && attachment.Body.sharedMesh != null &&
                attachment.Body.sharedMesh.name.StartsWith(stem, StringComparison.Ordinal) &&
                attachment.DriverNames != null &&
                PrimateVisualPolicy.PermitsBones(key, attachment.DriverNames) &&
                equipment != null && equipment.sharedMesh != null &&
                equipment.sharedMesh.vertexCount == 0;
            UnityEngine.Object[] owned = attachment == null ?
                new UnityEngine.Object[0] : attachment.CaptureOwnedResources();
            bool instanceOwned = owned.Length > 0 && owned.All(value =>
                PrimateVisualPolicy.IsPrimateInstanceResource(key,
                    attachment.Body.sharedMesh.name, value.name));
            Sprint18Check(assertions, rows, key + "-original-body", original && instanceOwned,
                new JObject {
                    ["outcome"] = ExpandedSummoningPrimateViewPatch.DescribeView(owner.View),
                    ["bodyMesh"] = attachment == null || attachment.Body == null ||
                        attachment.Body.sharedMesh == null ? null :
                        attachment.Body.sharedMesh.name,
                    ["vertices"] = attachment == null || attachment.Body == null ||
                        attachment.Body.sharedMesh == null ? 0 :
                        attachment.Body.sharedMesh.vertexCount,
                    ["drivers"] = attachment == null || attachment.DriverNames == null ?
                        0 : attachment.DriverNames.Length,
                    ["equipmentBlanked"] = equipment != null &&
                        equipment.sharedMesh != null && equipment.sharedMesh.vertexCount == 0,
                    // Recorded, not required: the attachment never sets
                    // either of these, so whatever the donor's own faders and
                    // appearance locks left them at is the right answer.
                    ["equipmentRendererEnabled"] = equipment != null && equipment.enabled,
                    ["equipmentObjectActive"] = equipment != null &&
                        equipment.gameObject.activeInHierarchy,
                    ["ownedResources"] = new JArray(owned.Select(value => value.name)),
                    ["instanceOwned"] = instanceOwned,
                }, "the live ape wears its own body, the donor equipment is blank, " +
                   "and every created resource is named for this instance");
        }

        /// <summary>
        /// Every modifier on a value, with its size, descriptor and source.
        /// A total alone cannot say which term is unexpected.
        /// </summary>
        private static string DescribeSprint18Modifiers(ModifiableValue value)
        {
            return value == null ? null : "total:" + value.ModifiedValue +
                ",base:" + value.BaseValue + ",modifiers:[" +
                string.Join("|", value.Modifiers.Select(modifier =>
                    modifier.ModValue + "/" + modifier.ModDescriptor + "/" +
                    modifier.Source).ToArray()) + "]";
        }

        private static UnityEngine.Object[] ReviewSprint18OwnedResources(UnitEntityData owner)
        {
            var attachment = owner == null || owner.View == null ? null :
                owner.View.GetComponent<PrimateVisualAttachment>();
            return attachment == null ? new UnityEngine.Object[0] :
                attachment.CaptureOwnedResources();
        }

        /// <summary>
        /// After dismissal, every project-owned mesh, texture and material is
        /// destroyed. A leak here would accumulate one body per summon.
        /// </summary>
        private static void ReviewSprint18Release(string key, UnityEngine.Object[] owned,
            List<RuntimeTestAssertion> assertions, JArray rows)
        {
            var alive = owned.Where(value => value != null).Select(value => value.name).ToArray();
            Sprint18Check(assertions, rows, key + "-resources-released", owned.Length > 0 && alive.Length == 0,
                new JObject { ["captured"] = owned.Length,
                    ["stillAlive"] = new JArray(alive) },
                "dismissal destroys every resource the instance created and nothing else");
        }

        private static ItemEntityWeapon[] Sprint18Limbs(UnitEntityData owner)
        {
            ItemEntityWeapon primary = SummonLimbs.PrimaryWeapon(owner);
            return new[] { primary }.Concat(LiveLimbWeapons(owner).Where(value =>
                !ReferenceEquals(value, primary))).ToArray();
        }
    }
}
