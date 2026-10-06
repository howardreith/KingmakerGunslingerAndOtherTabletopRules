using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // This closed slice proves only base/live-modified profiles and body
        // binding. It is NOT poison/constrict cadence, commands/AI, persistence,
        // crowd/routes, publication or complete Sprint17 qualification.
        private IEnumerable<int> ReviewSprint17SnakeProfiles(ExpandedSummoningCorrectionFixture fixture)
        {
            foreach (string key in new[] { "viper", "constrictor-snake" })
            {
                bool viper = key == "viper";
                UnitEntityData unit = CastExpandedSummoningVariant(fixture.Blueprints, fixture.Caster,
                    ExpandedSummoningOwnTierVariant(key, SummonMultiplicity.One), null, fixture.Evidence).Single();
                fixture.Created.Add(unit);
                SetExpandedSummoningBrainActive(unit, false);
                if (!Game.Instance.State.AwakeUnits.Contains(unit)) Game.Instance.State.AwakeUnits.Add(unit);
                try
                {
                    // Keep native appearance state; no helper which removes
                    // its buffs and no AI/real-command proof in this slice.
                    for (int frame = 0; frame < 60; frame++) yield return 0;
                    var stats = unit.Descriptor.Stats;
                    int[] wanted = viper ? new[] { 8, 13, 14, 1, 13, 2 } : new[] { 17, 17, 12, 1, 12, 2 };
                    int[] scores = new[] { StatType.Strength, StatType.Dexterity, StatType.Constitution,
                        StatType.Intelligence, StatType.Wisdom, StatType.Charisma }.Select(stat =>
                            stats.GetStat(stat).ModifiedValue).ToArray();
                    var blueprintRanks = new JObject();
                    foreach (var field in typeof(Kingmaker.Blueprints.BlueprintUnit.UnitSkills)
                        .GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public))
                        blueprintRanks[field.Name] = (int)field.GetValue(unit.Blueprint.Skills);
                    var row = new JObject { ["key"] = key, ["scope"] = "actual untemplated private snake profile",
                        ["scores"] = new JArray(scores), ["size"] = unit.Descriptor.State.Size.ToString(),
                        ["hitDice"] = unit.Descriptor.Progression.CharacterLevel,
                        ["type"] = unit.Blueprint.Type == null ? null : unit.Blueprint.Type.name,
                        ["hp"] = stats.HitPoints.ModifiedValue, ["hpBase"] = stats.HitPoints.BaseValue,
                        ["ac"] = stats.AC.ModifiedValue, ["touch"] = stats.AC.Touch, ["flatFooted"] = stats.AC.FlatFooted,
                        ["fortitude"] = stats.GetStat(StatType.SaveFortitude).ModifiedValue,
                        ["reflex"] = stats.GetStat(StatType.SaveReflex).ModifiedValue,
                        ["will"] = stats.GetStat(StatType.SaveWill).ModifiedValue,
                        ["speed"] = stats.GetStat(StatType.Speed).ModifiedValue,
                        ["initiative"] = stats.GetStat(StatType.Initiative).ModifiedValue,
                        ["blueprintSkillSeeds"] = blueprintRanks,
                        ["creationComponents"] = new JArray(unit.Blueprint.ComponentsArray.Select(value => value.GetType().Name)),
                        ["perception"] = DescribeSprint16Skill(stats.GetStat(StatType.SkillPerception)),
                        ["stealth"] = DescribeSprint16Skill(stats.GetStat(StatType.SkillStealth)),
                        ["mobility"] = DescribeSprint16Skill(stats.GetStat(StatType.SkillMobility)) };
                    CheckSprint17SnakeProfile(key + "-scores", scores.SequenceEqual(wanted) &&
                        unit.Descriptor.State.Size == Size.Medium &&
                        unit.Descriptor.Progression.CharacterLevel == (viper ? 2 : 3) &&
                        (string)row["type"] == "KMG_Summoning_Natural_" + (viper ? "Viper" : "ConstrictorSnake") + "_UnitType",
                        row, "actual printed scores/HD/Medium and own inspectable type, not a Worm identity");
                    CheckSprint17SnakeProfile(key + "-defenses", stats.HitPoints.ModifiedValue == (viper ? 13 : 19) &&
                        stats.AC.ModifiedValue == (viper ? 14 : 15) && stats.AC.Touch == (viper ? 11 : 13) &&
                        stats.AC.FlatFooted == (viper ? 13 : 12) &&
                        stats.GetStat(StatType.SaveFortitude).ModifiedValue == (viper ? 5 : 4) &&
                        stats.GetStat(StatType.SaveReflex).ModifiedValue == (viper ? 4 : 6) &&
                        stats.GetStat(StatType.SaveWill).ModifiedValue == (viper ? 1 : 2) &&
                        stats.GetStat(StatType.Speed).ModifiedValue == 20 &&
                        stats.GetStat(StatType.Initiative).ModifiedValue == (viper ? 5 : 3),
                        row, "live native HP/AC/saves/speed/initiative; no fixture stat correction");
                    CheckSprint17SnakeProfile(key + "-land-skills",
                        stats.GetStat(StatType.SkillPerception).ModifiedValue == (viper ? 9 : 12) &&
                        stats.GetStat(StatType.SkillStealth).ModifiedValue == (viper ? 9 : 11) &&
                        stats.GetStat(StatType.SkillMobility).ModifiedValue == (viper ? 9 : 15) &&
                        stats.GetStat(StatType.SkillPerception).BaseValue == 1 &&
                        stats.GetStat(StatType.SkillStealth).BaseValue == 1 &&
                        stats.GetStat(StatType.SkillMobility).BaseValue == (viper ? 0 : 1) &&
                        blueprintRanks.Count == 11 && blueprintRanks.Properties().All(value => (int)value.Value == 0),
                        row, "native ranks/class/ability/racial/feat breakdown; aquatic uses excluded");

                    var bite = SummonLimbs.PrimaryWeapon(unit);
                    // This slice has no hostile target. The old attack helper
                    // returned int.MinValue before making any rule. Use the
                    // native profile calculation, not a fabricated target or
                    // an attack/on-hit delivery in a no-damage profile check.
                    var attackRule = Rulebook.Trigger(new RuleCalculateAttackBonusWithoutTarget(unit, bite, 0));
                    int attack = attackRule.Result;
                    var grab = SummonGrabComponent.Find(unit);
                    CheckSprint17SnakeProfile(key + "-one-bite", bite != null &&
                        bite.Blueprint.Category == WeaponCategory.Bite && !bite.IsSecondary &&
                        LiveLimbWeapons(unit).Count() == 1 && attack == (viper ? 2 : 5) &&
                        (viper ? grab == null : grab != null && grab.GrabWithPrimaryHand &&
                            grab.GrabAdditionalLimbCount == 0 && grab.IsGrabLimb(unit, bite)),
                        new JObject { ["key"] = key, ["attackBonus"] = attack,
                            ["attackRule"] = attackRule.GetType().Name,
                            ["attackStat"] = attackRule.AttackBonusStat.ToString(),
                            ["attackStatModifier"] = attackRule.AttackBonusStatModifier,
                            ["limbCount"] = LiveLimbWeapons(unit).Count(),
                            ["weapon"] = bite == null ? null : bite.Blueprint.AssetGuid,
                            ["secondary"] = bite != null && bite.IsSecondary, ["grabPresent"] = grab != null },
                        "one real primary bite, native attack calculation and correctly owned grab fact; not a successful hit/hold claim");
                    ProbeSprint17SnakeBite(unit, key + "-base", wanted[0], viper ? -1 : 4, DiceType.D4);
                    foreach (int delta in new[] { 4, 7 - wanted[0] })
                    {
                        var modifier = stats.Strength.AddModifier(delta, null,
                            "KMG_Sprint17_Disposable_Strength", ModifierDescriptor.UntypedStackable);
                        try
                        {
                            int ability = (int)Math.Floor((wanted[0] + delta - 10) / 2d);
                            ProbeSprint17SnakeBite(unit, key + (delta > 0 ? "-strength-plus4" : "-strength-seven"),
                                wanted[0] + delta, ability + Math.Max(0, ability) / 2, DiceType.D4);
                        }
                        finally { stats.Strength.RemoveModifier(modifier); }
                    }
                    var growth = fixture.Blueprints.OfType<BlueprintBuff>().Single(value =>
                        value.name == "AnimalGrowthBuff" && value.ComponentsArray.OfType<ChangeUnitSize>().Any());
                    var grown = unit.Descriptor.AddBuff(growth, unit, TimeSpan.FromMinutes(1));
                    try
                    {
                        int ability = (int)Math.Floor((wanted[0] + 8 - 10) / 2d);
                        ProbeSprint17SnakeBite(unit, key + "-native-animal-growth", wanted[0] + 8,
                            ability + ability / 2, DiceType.D6);
                    }
                    finally { if (grown != null) grown.Remove(); }
                    ProbeSprint17SnakeBite(unit, key + "-modifiers-restored", wanted[0], viper ? -1 : 4, DiceType.D4);
                }
                finally
                {
                    if (!unit.Destroyed)
                    {
                        InterruptExpandedSummoningFixtureCommands(unit);
                        unit.Destroy();
                        Game.Instance.EntityDestroyer.Tick();
                    }
                }
                yield return 0; yield return 0;
            }
        }

        private void ProbeSprint17SnakeBite(UnitEntityData unit, string name,
            int expectedStrength, int expectedBonus, DiceType expectedDie)
        {
            var bite = SummonLimbs.PrimaryWeapon(unit);
            var calculated = Rulebook.Trigger(new RuleCalculateWeaponStats(unit, bite, null));
            BaseDamage[] damage = calculated.DamageDescription.Select(value => value.CreateDamage()).ToArray();
            var physical = damage.OfType<PhysicalDamage>().SingleOrDefault();
            var row = new JObject { ["strengthScore"] = unit.Descriptor.Stats.Strength.ModifiedValue,
                ["strengthModifier"] = unit.Descriptor.Stats.Strength.Bonus,
                ["size"] = unit.Descriptor.State.Size.ToString(),
                ["weaponEntitySize"] = bite.Size.ToString(), ["calculatedWeaponSize"] = calculated.WeaponSize.ToString(),
                ["damage"] = new JArray(damage.Select(Sprint16DamageLine)) };
            CheckSprint17SnakeProfile(name, damage.Length == 1 && physical != null &&
                physical.Dice.Rolls == 1 && physical.Dice.Dice == expectedDie &&
                physical.Bonus == expectedBonus && unit.Descriptor.Stats.Strength.ModifiedValue == expectedStrength,
                row, "actual live RuleCalculateWeaponStats; native positive single-attack bonus and negative penalty; no attack or on-hit replay");
        }

        private void CheckSprint17SnakeProfile(string name, bool pass, JObject row, string contract)
        {
            row = (JObject)row.DeepClone();
            row["check"] = name; row["passed"] = pass;
            _serpentineBodyRows.Add(row);
            _serpentineBodyAssertions.Add(Assertion("sprint17-snake-profile-" + name, contract,
                row.ToString(Formatting.None), pass,
                "Bounded profile/body slice only; poison/constrict cadence, real commands, persistence and Sprint17 acceptance remain separate."));
        }
    }
}
