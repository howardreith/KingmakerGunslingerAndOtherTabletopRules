using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.EntitySystem.Entities;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// The Sprint 19 hidden-candidate runtime review: what the Girallon and
    /// the Xill actually are in a running game.
    ///
    /// <para>Everything here reads a live creature summoned through the
    /// registered execution into the guarded working save and attacks a
    /// disposable hostile with real commands and native rules. The fixture
    /// raises only this disposable attacker's accuracy and only this
    /// disposable target's hit points, and restores both; no result is forced,
    /// no rule is replayed and no damage is manufactured.</para>
    ///
    /// <para>Three things this review checks that Sprint 18's could not. A
    /// limb carries its own attack bonus, because the Xill's claws are +13
    /// and its bite +12. A rend needs four claws, so three of four must be
    /// proved not to rend. And the Xill's eight printed skills land on five
    /// Kingmaker skills, so the review counts five and refuses a sixth.</para>
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        private const int Sprint19SettleFrames = 240;
        private const int Sprint19AttackFrames = 900;

        private ExpandedSummoningCorrectionFixture _sprint19Fixture;
        private IEnumerator<int> _sprint19Steps;
        private readonly List<RuntimeTestAssertion> _sprint19Assertions =
            new List<RuntimeTestAssertion>();
        private readonly JArray _sprint19Rows = new JArray();
        private int _sprint19LoadingFrames;
        private bool _sprint19Complete;
        private UnityEngine.Object[] _sprint19Owned = new UnityEngine.Object[0];
        private string _sprint19OwnedKey;

        private void PollSprint19Review()
        {
            if (_sprint19Complete) return;
            try
            {
                if (_sprint19Steps == null)
                {
                    string loading;
                    if (ExpandedSummoningLoadingActive(out loading))
                    {
                        if (++_sprint19LoadingFrames <
                            ExpandedSummoningLoadingGateFrames) return;
                        throw new InvalidOperationException(
                            "Native loading did not settle: " + loading);
                    }
                    _sprint19Fixture = BeginExpandedSummoningCorrectionFixture(
                        "KMG_Runtime_Sprint19_Caster");
                    _sprint19Steps = ReviewSprint19(_sprint19Fixture).GetEnumerator();
                }
                if (_sprint19Steps.MoveNext()) return;
            }
            catch (Exception error)
            {
                Sprint19Check(_sprint19Assertions, _sprint19Rows,
                    "review-exception", false, new JObject {
                        ["exception"] =
                            DescribeExpandedSummoningCorrectionException(error) },
                    "the whole Sprint 19 review completes without an exception");
            }
            FinishSprint19Review();
        }

        private static void Sprint19Check(List<RuntimeTestAssertion> assertions,
            JArray rows, string name, bool passed, JObject row, string expected)
        {
            row["case"] = name;
            row["passed"] = passed;
            rows.Add(row);
            assertions.Add(Assertion("sprint19-" + name, expected,
                row.ToString(Formatting.None), passed,
                "request-local live Girallon and Xill; native rules and the "
                + "registered execution"));
        }

        private IEnumerable<int> ReviewSprint19(
            ExpandedSummoningCorrectionFixture fixture)
        {
            bool pause = Game.Instance.IsPaused;
            bool turnBasedSetting = Kingmaker.UI.SettingsUI.SettingsRoot
                .Instance.EnableTurnBasedMode.CurrentValue;
            UnityEngine.Random.State random = UnityEngine.Random.state;
            try
            {
                Game.Instance.IsPaused = false;
                CreateExpandedSummoningCorrectionHostile(fixture);
                bool profiled = false;
                foreach (bool turnBased in Sprint19ReviewPolicy.CombatModes)
                {
                    Kingmaker.UI.SettingsUI.SettingsRoot.Instance
                        .EnableTurnBasedMode.CurrentValue = turnBased;
                    Game.Instance.TurnBasedCombatController.Activate();
                    foreach (string key in Sprint19ReviewPolicy.Keys)
                    {
                        UnitEntityData owner =
                            CastExpandedSummoningOwnTier(fixture, key);
                        SetExpandedSummoningBrainActive(owner, false);
                        int settle = 0;
                        while (++settle <= Sprint19SettleFrames)
                        {
                            if (Game.Instance.IsPaused)
                                Game.Instance.IsPaused = false;
                            yield return 0;
                            if (owner.View != null &&
                                owner.Descriptor.State.CanAct) break;
                        }
                        try
                        {
                            if (!profiled)
                                ReviewSprint19Body(owner, key,
                                    _sprint19Assertions, _sprint19Rows);
                            foreach (int step in ReviewSprint19Routine(
                                fixture, owner, key, turnBased)) yield return step;
                            // Only now is the creature in combat and no
                            // longer flat-footed, which is the state a
                            // printed stat block describes. Sprint 18's first
                            // run read the profile before the creature had
                            // acted and measured it denied its Dexterity in
                            // armour class, touch and combat manoeuvre
                            // defence; this review reads it after.
                            if (!profiled)
                                ReviewSprint19Profile(fixture, owner, key,
                                    _sprint19Assertions, _sprint19Rows);
                            if (key == GirallonRulesPolicy.GirallonKey)
                                foreach (int step in ReviewSprint19Rend(
                                    fixture, owner, turnBased)) yield return step;
                            if (key == XillRulesPolicy.XillKey)
                                foreach (int step in ReviewSprint19Paralysis(
                                    fixture, owner, turnBased)) yield return step;
                        }
                        finally
                        {
                            _sprint19Owned = ReviewSprint19OwnedResources(owner);
                            _sprint19OwnedKey = profiled ? null : key;
                            ResetExpandedSummoningHostile(fixture);
                            DireApeRendGate.ForgetObserved(owner);
                            DisposeExpandedSummoningUnits(fixture.Created,
                                new[] { owner });
                        }
                        // Native views are destroyed at the end of a frame, so
                        // the instance's resources are still alive in the frame
                        // its unit was dismissed in. Let the native teardown
                        // run before counting what survived.
                        for (int teardown = 0; teardown < 8; teardown++)
                            yield return 0;
                        if (_sprint19OwnedKey != null)
                            ReviewSprint19Release(_sprint19OwnedKey,
                                _sprint19Owned, _sprint19Assertions,
                                _sprint19Rows);
                        _sprint19Owned = new UnityEngine.Object[0];
                        _sprint19OwnedKey = null;
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
        /// combat manoeuvre defence, every limb's own attack bonus and damage,
        /// the mapped skills, and - for the Xill - spell resistance, the
        /// grapple bonus and the paralysis difficulty class.
        /// </summary>
        private static void ReviewSprint19Profile(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner,
            string key, List<RuntimeTestAssertion> assertions, JArray rows)
        {
            Sprint19LiveProfile expected = Sprint19ReviewPolicy.For(key);
            var stats = owner.Descriptor.Stats;
            int[] scores = new[] { StatType.Strength, StatType.Dexterity,
                StatType.Constitution, StatType.Intelligence, StatType.Wisdom,
                StatType.Charisma }.Select(stat =>
                    stats.GetStat(stat).ModifiedValue).ToArray();
            ItemEntityWeapon[] limbs = Sprint19Limbs(owner);
            var limbRows = new JArray();
            bool limbsExact = limbs.Length == expected.Limbs.Length;
            for (int index = 0; index < limbs.Length; index++)
            {
                RuleCalculateWeaponStats calculated = Rulebook.Trigger(
                    new RuleCalculateWeaponStats(owner, limbs[index], null));
                BaseDamage damage = calculated.DamageDescription[0].CreateDamage();
                int attack = ProbeEntityAttackBonus(owner, fixture.Hostile,
                    limbs[index]);
                int[] want = index < expected.Limbs.Length ?
                    expected.Limbs[index] : new[] { 0, 0, 0, 0 };
                // Each limb carries its own attack bonus. The Xill's claws and
                // bite differ by one, and a shared expectation would hide it.
                bool exact = attack == want[3] &&
                    damage.Dice.Rolls == want[0] &&
                    (int)damage.Dice.Dice == want[1] &&
                    damage.Bonus == want[2] && !limbs[index].IsSecondary &&
                    !calculated.SecondaryWeapon;
                limbsExact &= exact;
                limbRows.Add(new JObject {
                    ["limb"] = index,
                    ["weapon"] = limbs[index].Blueprint.AssetGuid,
                    ["attackBonus"] = attack,
                    ["expectedAttackBonus"] = want[3],
                    ["damage"] = Sprint16DamageLine(damage),
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
            // the game escapes it, so it is subtracted and named. Sprint 18
            // identified this term; it is not a Sprint 19 finding.
            int difficulty = stats.AC.Modifiers.Where(modifier =>
                modifier.ModDescriptor == ModifierDescriptor.Difficulty)
                .Sum(modifier => modifier.ModValue);
            int deniedDex = cmd.DexterityDenied || cmd.FlatFooted ?
                stats.Dexterity.Bonus : 0;
            Size size = key == GirallonRulesPolicy.GirallonKey ?
                Size.Large : Size.Medium;
            var skillRows = new JObject();
            bool skillsExact = true;
            foreach (KeyValuePair<string, int> want in expected.Skills)
            {
                StatType stat = Sprint19Skill(want.Key);
                int live = stats.GetStat(stat).ModifiedValue;
                skillsExact &= live == want.Value;
                skillRows[want.Key] = DescribeSprint16Skill(stats.GetStat(stat));
            }
            // Spell resistance is a unit part rather than a stat, so it is
            // read where the engine keeps it. A creature that prints none has
            // no part at all, which reads as zero.
            Kingmaker.UnitLogic.Parts.UnitPartSpellResistance resistance =
                owner.Descriptor.Get<
                    Kingmaker.UnitLogic.Parts.UnitPartSpellResistance>();
            int spellResistance = resistance == null ? 0 :
                resistance.GetValue(null);
            bool exactProfile = scores.SequenceEqual(new[] {
                    expected.Strength, expected.Dexterity,
                    expected.Constitution, expected.Intelligence,
                    expected.Wisdom, expected.Charisma }) &&
                owner.Descriptor.State.Size == size &&
                owner.Descriptor.Progression.CharacterLevel == expected.HitDice &&
                stats.HitPoints.ModifiedValue == expected.HitPoints &&
                stats.AC.ModifiedValue - difficulty == expected.ArmorClass &&
                stats.AC.Touch - difficulty == expected.Touch &&
                stats.AC.FlatFooted - difficulty == expected.FlatFooted &&
                stats.GetStat(StatType.SaveFortitude).ModifiedValue ==
                    expected.Fortitude &&
                stats.GetStat(StatType.SaveReflex).ModifiedValue ==
                    expected.Reflex &&
                stats.GetStat(StatType.SaveWill).ModifiedValue == expected.Will &&
                cmd.Result + deniedDex - difficulty ==
                    expected.CombatManeuverDefense &&
                spellResistance == expected.SpellResistance &&
                skillsExact && limbsExact;
            Sprint19Check(assertions, rows, key + "-live-profile", exactProfile,
                new JObject {
                    ["scores"] = new JArray(scores),
                    ["size"] = owner.Descriptor.State.Size.ToString(),
                    ["hitDice"] = owner.Descriptor.Progression.CharacterLevel,
                    ["hitPoints"] = stats.HitPoints.ModifiedValue,
                    ["armor"] = stats.AC.ModifiedValue,
                    ["touch"] = stats.AC.Touch,
                    ["flatFooted"] = stats.AC.FlatFooted,
                    ["fortitude"] =
                        stats.GetStat(StatType.SaveFortitude).ModifiedValue,
                    ["reflex"] = stats.GetStat(StatType.SaveReflex).ModifiedValue,
                    ["will"] = stats.GetStat(StatType.SaveWill).ModifiedValue,
                    ["cmd"] = cmd.Describe("CMD"),
                    ["deniedDexRecovered"] = deniedDex,
                    ["armorModifiers"] = DescribeSprint18Modifiers(stats.AC),
                    ["armorDexterityBonus"] = stats.AC.DexterityBonus,
                    ["additionalCmd"] = stats.AdditionalCMD.ModifiedValue,
                    ["nativeDifficultyTerm"] = difficulty,
                    ["armorNetOfDifficulty"] = stats.AC.ModifiedValue - difficulty,
                    ["touchNetOfDifficulty"] = stats.AC.Touch - difficulty,
                    ["flatFootedNetOfDifficulty"] =
                        stats.AC.FlatFooted - difficulty,
                    ["cmdNetOfDifficulty"] = cmd.Result + deniedDex - difficulty,
                    ["spellResistance"] = spellResistance,
                    ["expectedSpellResistance"] = expected.SpellResistance,
                    ["limbs"] = limbRows,
                    ["skills"] = skillRows,
                    ["skillCount"] = expected.Skills.Count,
                    // What this creature does not represent, recorded as
                    // numbers rather than silences. Nothing stands in for any
                    // of them.
                    ["printedClimbOmitted"] =
                        key == GirallonRulesPolicy.GirallonKey ?
                            GirallonRulesPolicy.PrintedClimbSkill : 0,
                    ["athleticsUnraised"] =
                        stats.GetStat(StatType.SkillAthletics).BaseValue,
                    ["mergedPrintedSkills"] = key == XillRulesPolicy.XillKey ?
                        new JArray("Intimidate->Persuasion",
                            "SenseMotive->Perception",
                            "KnowledgePlanes->KnowledgeArcana") : new JArray(),
                    ["unitType"] = owner.Blueprint.Type == null ? null :
                        owner.Blueprint.Type.name,
                },
                "the live creature is the printed stat block, limb for limb, "
                + "with each limb at its own printed attack bonus");
        }

        /// <summary>
        /// The Kingmaker skill a printed skill is represented by. Only the
        /// five that carry a printed Sprint 19 skill are reachable; a name
        /// outside them is a review bug, not a creature.
        /// </summary>
        private static StatType Sprint19Skill(string name)
        {
            switch (name)
            {
                case "Mobility": return StatType.SkillMobility;
                case "Stealth": return StatType.SkillStealth;
                case "Perception": return StatType.SkillPerception;
                case "Persuasion": return StatType.SkillPersuasion;
                case "KnowledgeArcana": return StatType.SkillKnowledgeArcana;
                default:
                    throw new ArgumentOutOfRangeException("name", name,
                        "Sprint 19 represents five Kingmaker skills.");
            }
        }

        /// <summary>
        /// The original body: attached, project-owned, with the donor's
        /// equipment skin blanked and nothing shared touched. Both creatures
        /// wear four arms on the donor's two driver chains, which the shipped
        /// mesh declares and this check does not pretend otherwise about.
        /// </summary>
        private static void ReviewSprint19Body(UnitEntityData owner, string key,
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
                attachment.Body.sharedMesh.name.StartsWith(stem,
                    StringComparison.Ordinal) &&
                attachment.DriverNames != null &&
                PrimateVisualPolicy.PermitsBones(key, attachment.DriverNames) &&
                equipment != null && equipment.sharedMesh != null &&
                equipment.sharedMesh.vertexCount == 0;
            UnityEngine.Object[] owned = attachment == null ?
                new UnityEngine.Object[0] : attachment.CaptureOwnedResources();
            bool instanceOwned = owned.Length > 0 && owned.All(value =>
                PrimateVisualPolicy.IsPrimateInstanceResource(key,
                    attachment.Body.sharedMesh.name, value.name));
            Sprint19Check(assertions, rows, key + "-original-body",
                original && instanceOwned, new JObject {
                    ["outcome"] = ExpandedSummoningPrimateViewPatch
                        .DescribeView(owner.View),
                    ["bodyMesh"] = attachment == null || attachment.Body == null ||
                        attachment.Body.sharedMesh == null ? null :
                        attachment.Body.sharedMesh.name,
                    ["vertices"] = attachment == null || attachment.Body == null ||
                        attachment.Body.sharedMesh == null ? 0 :
                        attachment.Body.sharedMesh.vertexCount,
                    ["drivers"] = attachment == null ||
                        attachment.DriverNames == null ? 0 :
                        attachment.DriverNames.Length,
                    ["equipmentBlanked"] = equipment != null &&
                        equipment.sharedMesh != null &&
                        equipment.sharedMesh.vertexCount == 0,
                    // Recorded, not required: the attachment never sets either
                    // of these, so whatever the donor's own faders and
                    // appearance locks left them at is the right answer.
                    ["equipmentRendererEnabled"] = equipment != null &&
                        equipment.enabled,
                    ["equipmentObjectActive"] = equipment != null &&
                        equipment.gameObject.activeInHierarchy,
                    ["ownedResources"] =
                        new JArray(owned.Select(value => value.name)),
                    ["instanceOwned"] = instanceOwned,
                    // The authored limitation, recorded on every run rather
                    // than left in a design document.
                    ["armDriverChains"] = 2,
                    ["visibleArms"] = 4,
                    ["limitation"] = "FOUR_ARMS_SHARE_TWO_DRIVER_CHAINS",
                },
                "the live creature wears its own body, the donor equipment is "
                + "blank, and every created resource is named for this instance");
        }

        private static UnityEngine.Object[] ReviewSprint19OwnedResources(
            UnitEntityData owner)
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
        private static void ReviewSprint19Release(string key,
            UnityEngine.Object[] owned,
            List<RuntimeTestAssertion> assertions, JArray rows)
        {
            var alive = owned.Where(value => value != null)
                .Select(value => value.name).ToArray();
            Sprint19Check(assertions, rows, key + "-resources-released",
                owned.Length > 0 && alive.Length == 0,
                new JObject { ["captured"] = owned.Length,
                    ["stillAlive"] = new JArray(alive) },
                "dismissal destroys every resource the instance created and "
                + "nothing else");
        }

        private static ItemEntityWeapon[] Sprint19Limbs(UnitEntityData owner)
        {
            ItemEntityWeapon primary = SummonLimbs.PrimaryWeapon(owner);
            return new[] { primary }.Concat(LiveLimbWeapons(owner).Where(value =>
                !ReferenceEquals(value, primary))).ToArray();
        }
    }
}
