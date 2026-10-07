using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Utility;
using KingmakerGunslinger.Summoning;
using KingmakerGunslinger.Blueprints;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Actual published Salamander, no prototype/profile correction by the
        // fixture. The historical snake-profile request now includes this
        // third, closed Sprint17 identity. No save writes or publication.
        private IEnumerable<int> ReviewSprint17SalamanderProfile(ExpandedSummoningCorrectionFixture fixture)
        {
            UnitEntityData unit = CastExpandedSummoningVariant(fixture.Blueprints, fixture.Caster,
                ExpandedSummoningOwnTierVariant("salamander", SummonMultiplicity.One), null, fixture.Evidence).Single();
            if (!fixture.Created.Contains(unit)) fixture.Created.Add(unit);
            SetExpandedSummoningBrainActive(unit, false);
            if (!Game.Instance.State.AwakeUnits.Contains(unit)) Game.Instance.State.AwakeUnits.Add(unit);
            var powerAttack = unit.Descriptor.ActivatableAbilities.Enumerable.Single(a =>
                a.Blueprint.AssetGuid == EasternWeaponNamedBlueprints.PowerAttackToggleGuid);
            bool powerAttackBefore = powerAttack.IsOn;
            try
            {
                for (int frame = 0; frame < 60; frame++) yield return 0;
                // The printed attack line is without optional Power Attack.
                // Stop only this owned unit's native toggle, then prove its
                // live enabled line separately; never remove the native feat.
                powerAttack.IsOn = false; powerAttack.Stop(true);
                var stats = unit.Descriptor.Stats;
                int[] scores = new[] { StatType.Strength, StatType.Dexterity, StatType.Constitution,
                    StatType.Intelligence, StatType.Wisdom, StatType.Charisma }.Select(s => stats.GetStat(s).ModifiedValue).ToArray();
                var levels = unit.Blueprint.ComponentsArray.OfType<AddClassLevels>().Single();
                BlueprintCharacterClass racial = levels.CharacterClass;
                var row = new JObject {
                    ["key"] = "salamander", ["guid"] = unit.Blueprint.AssetGuid,
                    ["scores"] = new JArray(scores), ["hd"] = unit.Descriptor.Progression.CharacterLevel,
                    ["bab"] = stats.BaseAttackBonus.ModifiedValue, ["size"] = unit.Descriptor.State.Size.ToString(),
                    ["hp"] = stats.HitPoints.ModifiedValue, ["hpBase"] = stats.HitPoints.BaseValue,
                    ["ac"] = stats.AC.ModifiedValue, ["touch"] = stats.AC.Touch, ["flatFooted"] = stats.AC.FlatFooted,
                    ["fortitude"] = stats.SaveFortitude.ModifiedValue, ["fortitudeBase"] = stats.SaveFortitude.BaseValue,
                    ["reflex"] = stats.SaveReflex.ModifiedValue, ["reflexBase"] = stats.SaveReflex.BaseValue,
                    ["will"] = stats.SaveWill.ModifiedValue, ["willBase"] = stats.SaveWill.BaseValue,
                    ["speed"] = stats.Speed.ModifiedValue, ["initiative"] = stats.Initiative.ModifiedValue,
                    ["perception"] = DescribeSprint16Skill(stats.SkillPerception),
                    ["mobility"] = DescribeSprint16Skill(stats.SkillMobility),
                    ["persuasion"] = DescribeSprint16Skill(stats.SkillPersuasion),
                    ["type"] = unit.Blueprint.Type == null ? null : unit.Blueprint.Type.name,
                    ["nativeOutsider"] = new JObject { ["guid"] = racial.AssetGuid, ["hitDie"] = racial.HitDie.ToString(),
                        ["fortitudeAt8"] = racial.FortitudeSave.GetBonus(8), ["reflexAt8"] = racial.ReflexSave.GetBonus(8),
                        ["willAt8"] = racial.WillSave.GetBonus(8), ["classSkills"] = new JArray(racial.ClassSkills.Select(s => s.ToString())) }
                };
                CheckSprint17SalamanderProfile("racial-profile", SalamanderRulesPolicy.IsOwner(unit.Blueprint.AssetGuid, unit.Blueprint.name) &&
                    scores.SequenceEqual(new[] { 16, 13, 18, 14, 15, 13 }) && unit.Descriptor.Progression.CharacterLevel == 8 &&
                    stats.BaseAttackBonus.ModifiedValue == 8 && unit.Descriptor.State.Size == Size.Medium &&
                    (string)row["type"] == "KMG_Summoning_Special_Salamander_UnitType", row, "exact 8HD Medium outsider and printed ability scores");
                CheckSprint17SalamanderProfile("defenses", stats.HitPoints.ModifiedValue == 76 && stats.HitPoints.BaseValue == 44 &&
                    stats.AC.ModifiedValue == 18 && stats.AC.Touch == 11 && stats.AC.FlatFooted == 17 &&
                    stats.SaveFortitude.ModifiedValue == 10 && stats.SaveReflex.ModifiedValue == 7 && stats.SaveWill.ModifiedValue == 6 &&
                    stats.SaveFortitude.BaseValue == 6 && stats.SaveReflex.BaseValue == 6 && stats.SaveWill.BaseValue == 2 &&
                    stats.Speed.ModifiedValue == 20 && stats.Initiative.ModifiedValue == 1, row, "printed HP, armor, saves, speed and initiative with native modifier breakdown");
                CheckSprint17SalamanderProfile("land-skills", stats.SkillPerception.ModifiedValue == 16 &&
                    stats.SkillPerception.BaseValue == 8 && stats.SkillMobility.BaseValue == 0 && stats.SkillPersuasion.BaseValue == 0 &&
                    levels.Skills.Length == 0, row, "exact Perception ranks/class/Wisdom/SkillFocus and no donor Mobility/Persuasion ranks");
                var feats = new JObject();
                foreach (string guid in new[] { "d809b6c4ff2aaff4fa70d712a70f7d7b", "175d1577bb6c9a04baf88eec99c66334",
                    "9972f33f977fc724c838e59641b2fca5", "f74c6bdf5c5f5374fb9302ecdc1f7d64", "c1b26f97b974aec469613f968439e7bb" })
                {
                    var fact = fixture.Blueprints.OfType<BlueprintUnitFact>().Single(b => b.AssetGuid == guid);
                    feats[fact.name] = unit.Descriptor.HasFact(fact);
                }
                CheckSprint17SalamanderProfile("native-feats", feats.Count == 5 && feats.Properties().All(p => (bool)p.Value), feats,
                    "native Cleave, IronWill, PowerAttack, SkillFocusPerception and trip immunity; no WeaponFocus proxy");
                var spear = SummonLimbs.PrimaryWeapon(unit);
                var tail = unit.Body.AdditionalLimbs.Single().MaybeWeapon;
                var grab = SummonGrabComponent.Find(unit);
                var attack = Rulebook.Trigger(new RuleCalculateAttackBonusWithoutTarget(unit, spear, 0));
                var iterative = Rulebook.Trigger(new RuleCalculateAttackBonusWithoutTarget(unit, spear, 5));
                var secondary = Rulebook.Trigger(new RuleCalculateAttackBonusWithoutTarget(unit, tail, 0));
                var weapons = new JObject { ["spear"] = spear.Blueprint.AssetGuid, ["tail"] = tail.Blueprint.AssetGuid,
                    ["spearAttack"] = attack.Result, ["iterativeAttack"] = iterative.Result, ["tailAttack"] = secondary.Result,
                    ["spearRangeFeet"] = spear.AttackRange.Value, ["tailRangeFeet"] = tail.AttackRange.Value,
                    ["bodyReachFeet"] = stats.Reach.ModifiedValue, ["tailSecondary"] = tail.IsSecondary,
                    ["nativeMinimumFeet"] = GameConsts.MinWeaponRange.Value,
                    ["spearTypeRangeFeet"] = spear.Blueprint.Type.AttackRange.Value,
                    ["tailTypeRangeFeet"] = tail.Blueprint.Type.AttackRange.Value,
                    ["powerAttackDefaultOn"] = powerAttackBefore, ["powerAttackBaselineOn"] = powerAttack.IsOn,
                    ["spearNatural"] = spear.Blueprint.IsNatural, ["tailNatural"] = tail.Blueprint.IsNatural,
                    ["tailSize"] = tail.Blueprint.Size.ToString(), ["tailOverridesDice"] = tail.Blueprint.IsDamageDiceOverridden };
                CheckSprint17SalamanderProfile("attacks-and-tail-only-grab", attack.Result == 11 && iterative.Result == 6 && secondary.Result == 6 &&
                    !spear.Blueprint.IsNatural && tail.Blueprint.IsNatural && tail.IsSecondary &&
                    tail.Blueprint.AssetGuid == SalamanderRulesPolicy.TailGuid && grab != null &&
                    ReferenceEquals(grab.SalamanderProfileOwner, unit.Blueprint) && grab.ConstrictProfileOwner == null &&
                    grab.IsGrabLimb(unit, tail) && !grab.IsGrabLimb(unit, spear), weapons, "manufactured +11/+6 spear; secondary +6 tail; tail alone grabs");
                CheckSprint17SalamanderProfile("native-per-weapon-reach", Sprint17ObservationPolicy.NativeReach(
                    stats.Reach.ModifiedValue, spear.Blueprint.Type.AttackRange.Value, tail.Blueprint.Type.AttackRange.Value,
                    GameConsts.MinWeaponRange.Value, spear.AttackRange.Value, tail.AttackRange.Value) &&
                    tail.Blueprint.Size == Size.Medium &&
                    !tail.Blueprint.IsDamageDiceOverridden, weapons,
                    "raw types retain 5/10 feet; native four-foot allowance and minimum floor produce actual 2/6-foot weapon approach ranges; no five-foot difference is claimed");
                ProbeSprint17SalamanderWeapons(unit, spear, tail, "base", 16);
                foreach (int delta in new[] { 4, -9 })
                {
                    var modifier = stats.Strength.AddModifier(delta, null, "KMG_Sprint17_Salamander_Strength", ModifierDescriptor.UntypedStackable);
                    try { ProbeSprint17SalamanderWeapons(unit, spear, tail, "strength-" + (16 + delta), 16 + delta); }
                    finally { stats.Strength.RemoveModifier(modifier); }
                }
                ProbeSprint17SalamanderWeapons(unit, spear, tail, "restored", 16);
                var enlarge = fixture.Blueprints.OfType<BlueprintBuff>().Single(b => b.name == "EnlargePersonBuff" &&
                    b.ComponentsArray.OfType<ChangeUnitSize>().Any());
                Buff enlarged = unit.Descriptor.AddBuff(enlarge, unit, TimeSpan.FromMinutes(1));
                try { ProbeSprint17SalamanderWeapons(unit, spear, tail, "native-size", 18); }
                finally { if (enlarged != null) enlarged.Remove(); }
                ProbeSprint17SalamanderWeapons(unit, spear, tail, "size-restored", 16);
                powerAttack.IsOn = true;
                var powerSpear = Rulebook.Trigger(new RuleCalculateWeaponStats(unit, spear, null));
                var powerTail = Rulebook.Trigger(new RuleCalculateWeaponStats(unit, tail, null));
                CheckSprint17SalamanderProfile("native-power-attack", powerAttack.IsOn &&
                    Rulebook.Trigger(new RuleCalculateAttackBonusWithoutTarget(unit, spear, 0)).Result == 8 &&
                    Rulebook.Trigger(new RuleCalculateAttackBonusWithoutTarget(unit, spear, 5)).Result == 3 &&
                    Rulebook.Trigger(new RuleCalculateAttackBonusWithoutTarget(unit, tail, 0)).Result == 3 &&
                    powerSpear.DamageDescription.Select(d => d.CreateDamage()).OfType<PhysicalDamage>().Single().Bonus == 13 &&
                    powerTail.DamageDescription.Select(d => d.CreateDamage()).OfType<PhysicalDamage>().Single().Bonus == 4,
                    new JObject { ["toggle"] = powerAttack.Blueprint.AssetGuid, ["enabled"] = powerAttack.IsOn,
                        ["spear"] = new JArray(powerSpear.DamageDescription.Select(d => Sprint16DamageLine(d.CreateDamage()))),
                        ["tail"] = new JArray(powerTail.DamageDescription.Select(d => Sprint16DamageLine(d.CreateDamage()))) },
                    "native BAB8 Power Attack: -3 attacks, +9 two-handed spear and +3 secondary tail; feat and optional native toggle remain meaningful");
            }
            finally
            {
                powerAttack.IsOn = powerAttackBefore;
                if (!powerAttackBefore) powerAttack.Stop(true);
                if (!unit.Destroyed)
                {
                    InterruptExpandedSummoningFixtureCommands(unit);
                    unit.Destroy(); Game.Instance.EntityDestroyer.Tick();
                }
            }
            yield return 0; yield return 0;
        }

        private void ProbeSprint17SalamanderWeapons(UnitEntityData unit, ItemEntityWeapon spear, ItemEntityWeapon tail,
            string phase, int expectedStrength)
        {
            int strength = (int)Math.Floor((expectedStrength - 10) / 2d);
            foreach (ItemEntityWeapon weapon in new[] { spear, tail })
            {
                bool primary = ReferenceEquals(weapon, spear);
                var stats = Rulebook.Trigger(new RuleCalculateWeaponStats(unit, weapon, null));
                BaseDamage[] damage = stats.DamageDescription.Select(d => d.CreateDamage()).ToArray();
                var physical = damage.OfType<PhysicalDamage>().SingleOrDefault();
                var fire = damage.OfType<EnergyDamage>().SingleOrDefault();
                int expectedBonus = strength <= 0 ? strength : primary ? strength * 3 / 2 : strength / 2;
                bool enlarged = phase == "native-size";
                CheckSprint17SalamanderProfile(phase + (primary ? "-spear" : "-tail"), damage.Length == 2 && physical != null && fire != null &&
                    physical.Dice.Rolls == (enlarged ? (primary ? 2 : 3) : (primary ? 1 : 2)) &&
                    physical.Dice.Dice == (primary && !enlarged ? DiceType.D8 : DiceType.D6) &&
                    physical.Bonus == expectedBonus && fire.EnergyType == DamageEnergyType.Fire && fire.Dice.Rolls == 1 &&
                    fire.Dice.Dice == DiceType.D6 && fire.Bonus == 0 && unit.Stats.Strength.ModifiedValue == expectedStrength &&
                    unit.Descriptor.State.Size == (enlarged ? Size.Large : Size.Medium),
                    new JObject { ["strength"] = unit.Stats.Strength.ModifiedValue, ["modifier"] = unit.Stats.Strength.Bonus,
                        ["damage"] = new JArray(damage.Select(Sprint16DamageLine)), ["weaponSize"] = stats.WeaponSize.ToString() },
                    "live physical damage/Strength/native size plus exactly one unmultiplied 1d6 fire packet; no attack/on-hit replay");
            }
        }

        private void CheckSprint17SalamanderProfile(string name, bool pass, JObject row, string expected)
        {
            row = (JObject)row.DeepClone(); row["key"] = "salamander"; row["check"] = name; row["passed"] = pass;
            _serpentineBodyRows.Add(row);
            _serpentineBodyAssertions.Add(Assertion("sprint17-salamander-profile-" + name, expected,
                row.ToString(Formatting.None), pass, "Profile-only proof. Real commands, heat resistance/critical, constrict cadence, visuals and persistence are separate mandatory gates."));
        }
    }
}
