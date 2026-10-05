using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Parts;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        /// <summary>
        /// A request-scoped observer, never a production patch. The optional
        /// reorder preserves the native Flaming description object and moves
        /// it ahead of the base bite after weapon calculation. It challenges
        /// identity selection without inventing an elemental damage rider.
        /// </summary>
        private sealed class Sprint16RuleObserver :
            IGlobalRulebookHandler<RuleCalculateWeaponStats>,
            IGlobalRulebookHandler<RuleDealDamage>,
            IGlobalRulebookHandler<RuleAttackRoll>,
            IGlobalRulebookHandler<RuleAttackWithWeapon>,
            IGlobalRulebookHandler<RuleCastSpell>,
            IGlobalRulebookHandler<RuleCombatManeuver>
        {
            internal UnitEntityData Owner;
            internal UnitEntityData Target;
            internal bool ReorderEnergy;
            internal int Reordered;
            internal readonly List<RuleDealDamage> Damage = new List<RuleDealDamage>();
            internal readonly List<RuleAttackRoll> Attacks = new List<RuleAttackRoll>();
            internal readonly List<RuleCombatManeuver> Checks = new List<RuleCombatManeuver>();
            internal readonly List<RuleCastSpell> Casts = new List<RuleCastSpell>();
            internal int WeaponAttacks;
            internal Action<RuleAttackWithWeapon> ObserveWeaponContact;
            internal Action<RuleDealDamage> ObserveRiderContact;

            public void OnEventAboutToTrigger(RuleCalculateWeaponStats evt) { }
            public void OnEventDidTrigger(RuleCalculateWeaponStats evt)
            {
                if (!ReorderEnergy || !ReferenceEquals(evt.Initiator, Owner)) return;
                DamageDescription energy = evt.DamageDescription.FirstOrDefault(value =>
                    value.TypeDescription.Type == DamageType.Energy);
                if (energy == null) return;
                evt.DamageDescription.Remove(energy);
                evt.DamageDescription.Insert(0, energy);
                Reordered++;
            }
            public void OnEventAboutToTrigger(RuleDealDamage evt) { }
            public void OnEventDidTrigger(RuleDealDamage evt)
            {
                if (!ReferenceEquals(evt.Target, Target)) return;
                Damage.Add(evt);
                if (ReferenceEquals(evt.Initiator, Owner) && evt.AttackRoll == null &&
                    ObserveRiderContact != null) ObserveRiderContact(evt);
            }
            public void OnEventAboutToTrigger(RuleAttackRoll evt) { }
            public void OnEventDidTrigger(RuleAttackRoll evt)
            { if (ReferenceEquals(evt.Initiator, Owner)) Attacks.Add(evt); }
            public void OnEventAboutToTrigger(RuleAttackWithWeapon evt) { }
            public void OnEventDidTrigger(RuleAttackWithWeapon evt)
            {
                if (!ReferenceEquals(evt.Initiator, Owner)) return;
                WeaponAttacks++;
                if (ObserveWeaponContact != null) ObserveWeaponContact(evt);
            }
            public void OnEventAboutToTrigger(RuleCastSpell evt) { }
            public void OnEventDidTrigger(RuleCastSpell evt)
            { if (ReferenceEquals(evt.Initiator, Owner)) Casts.Add(evt); }
            public void OnEventAboutToTrigger(RuleCombatManeuver evt) { }
            public void OnEventDidTrigger(RuleCombatManeuver evt)
            {
                if (ReferenceEquals(evt.Initiator, Owner) ||
                    ReferenceEquals(evt.Initiator, Target)) Checks.Add(evt);
            }
            internal void Clear()
            { Damage.Clear(); Attacks.Clear(); Checks.Clear(); Casts.Clear(); WeaponAttacks = 0; }
        }

        private static JObject Sprint16DamageLine(BaseDamage damage)
        {
            PhysicalDamage physical = damage as PhysicalDamage;
            EnergyDamage energy = damage as EnergyDamage;
            return new JObject {
                ["type"] = damage.Type.ToString(),
                ["dice"] = damage.Dice.Rolls + "d" + (int)damage.Dice.Dice,
                ["bonus"] = damage.Bonus,
                ["form"] = physical == null ? null : physical.Form.ToString(),
                ["material"] = physical == null ? null : physical.MaterialsMask.ToString(),
                ["enhancement"] = physical == null ? 0 : physical.Enchantment,
                ["enhancementTotal"] = physical == null ? 0 : physical.EnchantmentTotal,
                ["energy"] = energy == null ? null : energy.EnergyType.ToString()
            };
        }

        private static JObject Sprint16DamageEvent(RuleDealDamage rule)
        {
            return new JObject {
                ["initiator"] = rule.Initiator == null ? null : rule.Initiator.UniqueId,
                ["target"] = rule.Target == null ? null : rule.Target.UniqueId,
                ["weapon"] = rule.DamageBundle.Weapon == null ? null :
                    rule.DamageBundle.Weapon.Blueprint.AssetGuid,
                ["weaponDamageIndex"] = rule.DamageBundle.ToList().FindIndex(value =>
                    ReferenceEquals(value, rule.DamageBundle.WeaponDamage)),
                ["chunks"] = new JArray(rule.DamageBundle.Select(Sprint16DamageLine)),
                ["damage"] = rule.Damage,
                ["withoutReduction"] = rule.DamageWithoutReduction,
                ["beforeDifficulty"] = rule.DamageBeforeDifficulty,
                ["attackRoll"] = rule.AttackRoll != null,
                ["critical"] = rule.AttackRoll != null && rule.AttackRoll.IsCriticalConfirmed,
                ["source"] = rule.Reason == null || rule.Reason.Context == null ||
                    rule.Reason.Context.AssociatedBlueprint == null ? null :
                    rule.Reason.Context.AssociatedBlueprint.name
            };
        }

        private static void Sprint16Check(List<RuntimeTestAssertion> assertions,
            JArray rows, string name, bool passed, JObject row, string expected)
        {
            row["case"] = name;
            row["passed"] = passed;
            rows.Add(row);
            assertions.Add(Assertion("sprint16-" + name, expected,
                row.ToString(Formatting.None), passed,
                "request-local live units; native rules and exact production methods"));
        }

        private void ExerciseSprint16DamageAndMaintain(
            ExpandedSummoningCorrectionFixture fixture,
            List<RuntimeTestAssertion> assertions)
        {
            var rows = new JArray();
            UnityEngine.Random.State randomBefore = UnityEngine.Random.state;
            try
            {
                CreateExpandedSummoningCorrectionHostile(fixture);
                foreach (string key in new[] { "crocodile", "dire-crocodile" })
                {
                    UnitEntityData owner = CastExpandedSummoningOwnTier(fixture, key);
                    SetExpandedSummoningBrainActive(owner, false);
                    try
                    {
                        ExerciseSprint16Profile(fixture, owner, key, assertions, rows);
                        ExerciseSprint16GrabDelivery(fixture, owner, key, assertions, rows);
                        ExerciseSprint16DeathRollDamage(fixture, owner, key, assertions, rows);
                        ExerciseSprint16Maintain(fixture, owner, key, assertions, rows);
                        ExerciseSprint16SessionReset(fixture, owner, key, assertions, rows);
                    }
                    finally
                    {
                        ResetExpandedSummoningHostile(fixture);
                        DisposeExpandedSummoningUnits(fixture.Created, new[] { owner });
                    }
                }
            }
            catch (Exception exception)
            {
                Sprint16Check(assertions, rows, "damage-maintain-exception", false,
                    new JObject { ["exception"] =
                        DescribeExpandedSummoningCorrectionException(exception) },
                    "all live damage and maintain cases complete without exception");
            }
            finally
            {
                UnityEngine.Random.state = randomBefore;
                File.WriteAllText(Path.Combine(_request.EvidenceDirectory,
                    "sprint16-damage-maintain.json"), rows.ToString(Formatting.Indented));
            }
        }

        private static void ExerciseSprint16SessionReset(ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, string key, List<RuntimeTestAssertion> assertions, JArray rows)
        {
            UnitEntityData control = CastExpandedSummoningQuietUnit(fixture, "purple-worm");
            UnitEntityData prey = CastExpandedSummoningQuietUnit(fixture, "wolf", fixture.Hostile);
            SummonGrabComponent grab = SummonGrabComponent.Find(owner);
            SummonGrabComponent controlGrab = SummonGrabComponent.Find(control);
            try
            {
                control.Descriptor.Stats.BaseAttackBonus.BaseValue = 100;
                PlaceExpandedSummoningUnit(prey, control.Position + Vector3.forward);
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                bool controlHeld = controlGrab.TryGrab(prey, SummonLimbs.PrimaryWeapon(control), true);
                var controlPart = control.Get<UnitPartGrappleInitiator>();
                var preyPart = prey.Get<UnitPartGrappleTarget>();
                Buff controlBuff = control.Descriptor.Buffs.GetBuff(controlGrab.HoldBuff);
                Buff held = Sprint16EstablishHold(fixture, owner, grab,
                    key == "crocodile" ? owner.Descriptor.State.Size : Size.Large, true);
                if (key == "dire-crocodile")
                {
                    int claimed = -1;
                    UnityEngine.Random.InitState(FindNativeD20Seed(20));
                    SummonHoldComponent.MaintainLink(owner, fixture.Hostile, grab, null,
                        owner.Descriptor.Buffs.GetBuff(grab.HoldBuff), held, ref claimed);
                }
                bool armed = key == "crocodile" ? ReferenceEquals(SummonHoldComponent.HeldTarget(owner), fixture.Hostile) :
                    fixture.Hostile.Get<UnitPartSwallowed>() != null;
                // The negative control is deliberately outside the explicit reset cohort.
                var cohort = new[] { owner, fixture.Hostile };
                SummonGrappleAreaSafeguard.ResetLoadedGrapples(cohort);
                SummonGrappleAreaSafeguard.ResetLoadedGrapples(cohort);
                bool free = Sprint16RelationshipReleased(owner, fixture.Hostile, grab) &&
                    owner.Get<UnitPartGrappleInitiator>() == null && !owner.Descriptor.HasFact(grab.HoldBuff) &&
                    (owner.Get<UnitPartSummonGrappleLinks>() == null || owner.Get<UnitPartSummonGrappleLinks>().Count == 0);
                bool untouched = controlHeld && controlPart != null && preyPart != null && controlBuff != null &&
                    ReferenceEquals(control.Get<UnitPartGrappleInitiator>(), controlPart) &&
                    ReferenceEquals(prey.Get<UnitPartGrappleTarget>(), preyPart) &&
                    ReferenceEquals(control.Descriptor.Buffs.GetBuff(controlGrab.HoldBuff), controlBuff) &&
                    ReferenceEquals(SummonHoldComponent.HeldTarget(control), prey);
                Sprint16Check(assertions, rows, key + "-owned-session-reset", armed && free && untouched,
                    new JObject { ["armed"] = armed, ["clearedTwice"] = free, ["purpleWormHoldUntouched"] = untouched },
                    "production load-reset path is idempotent for the explicit crocodilian cohort; the separate Purple Worm/prey cohort is untouched; full fresh reload remains mandatory");
            }
            finally
            {
                SummonHoldComponent.ReleaseLink(control, prey, controlGrab, true);
                if (control.Get<UnitPartGrappleInitiator>() != null) control.Remove<UnitPartGrappleInitiator>();
                Sprint16Release(fixture, owner, grab);
                DisposeExpandedSummoningUnits(fixture.Created, new[] { control, prey });
            }
        }

        private static void ExerciseSprint16Profile(ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, string key, List<RuntimeTestAssertion> assertions, JArray rows)
        {
            bool dire = key == "dire-crocodile";
            int[] expectedScores = dire ? new[] { 37, 10, 25, 1, 14, 2 } :
                new[] { 19, 12, 17, 1, 12, 2 };
            int[] scores = new[] { StatType.Strength, StatType.Dexterity, StatType.Constitution,
                StatType.Intelligence, StatType.Wisdom, StatType.Charisma }.Select(stat =>
                    owner.Descriptor.Stats.GetStat(stat).ModifiedValue).ToArray();
            ItemEntityWeapon bite = SummonLimbs.PrimaryWeapon(owner);
            ItemEntityWeapon tail = LiveLimbWeapons(owner).Single(value => !ReferenceEquals(value, bite));
            RuleCalculateWeaponStats biteStats = Rulebook.Trigger(new RuleCalculateWeaponStats(owner, bite, null));
            RuleCalculateWeaponStats tailStats = Rulebook.Trigger(new RuleCalculateWeaponStats(owner, tail, null));
            BaseDamage biteDamage = biteStats.DamageDescription[0].CreateDamage();
            BaseDamage tailDamage = tailStats.DamageDescription[0].CreateDamage();
            int biteAttack = ProbeEntityAttackBonus(owner, fixture.Hostile, bite);
            int tailAttack = ProbeEntityAttackBonus(owner, fixture.Hostile, tail);
            Sprint14CmdBreakdown cmd = ProbeCombatManeuverDefenceParts(fixture.Hostile, owner, CombatManeuver.BullRush);
            Sprint14CmdBreakdown trip = ProbeCombatManeuverDefenceParts(fixture.Hostile, owner, CombatManeuver.Trip);
            int deniedDex = cmd.DexterityDenied || cmd.FlatFooted ? owner.Descriptor.Stats.Dexterity.Bonus : 0;
            var stats = owner.Descriptor.Stats;
            SummonGrabComponent grab = SummonGrabComponent.Find(owner);
            bool exact = scores.SequenceEqual(expectedScores) &&
                owner.Descriptor.State.Size == (dire ? Size.Gargantuan : Size.Large) &&
                owner.Descriptor.Progression.CharacterLevel == (dire ? 12 : 3) &&
                stats.HitPoints.ModifiedValue == (dire ? 138 : 22) &&
                stats.AC.ModifiedValue == (dire ? 21 : 14) && stats.AC.Touch == (dire ? 6 : 10) &&
                stats.AC.FlatFooted == (dire ? 21 : 13) &&
                stats.GetStat(StatType.SaveFortitude).ModifiedValue == (dire ? 15 : 6) &&
                stats.GetStat(StatType.SaveReflex).ModifiedValue == (dire ? 8 : 4) &&
                stats.GetStat(StatType.SaveWill).ModifiedValue == (dire ? 8 : 2) &&
                stats.GetStat(StatType.SkillPerception).ModifiedValue == (dire ? 14 : 8) &&
                stats.GetStat(StatType.SkillStealth).ModifiedValue == (dire ? 0 : 5) &&
                stats.GetStat(StatType.SkillMobility).BaseValue == 0 &&
                cmd.Result + deniedDex == (dire ? 36 : 18) &&
                trip.Result - cmd.Result == 4 &&
                biteAttack == (dire ? 18 : 5) && tailAttack == (dire ? 13 : 0) &&
                biteDamage.Dice.Rolls == (dire ? 3 : 1) && biteDamage.Dice.Dice == (dire ? DiceType.D6 : DiceType.D8) &&
                biteDamage.Bonus == (dire ? 13 : 4) &&
                tailDamage.Dice.Rolls == (dire ? 4 : 1) && tailDamage.Dice.Dice == (dire ? DiceType.D8 : DiceType.D12) &&
                tailDamage.Bonus == (dire ? 6 : 2) && !bite.IsSecondary && tail.IsSecondary &&
                biteStats.DoubleCriticalEdge == dire &&
                grab != null && grab.IsGrabLimb(owner, bite) && !grab.IsGrabLimb(owner, tail);
            Sprint16Check(assertions, rows, key + "-live-profile", exact, new JObject {
                ["scores"] = new JArray(scores), ["size"] = owner.Descriptor.State.Size.ToString(),
                ["hitDice"] = owner.Descriptor.Progression.CharacterLevel,
                ["hitPoints"] = stats.HitPoints.ModifiedValue,
                ["hitPointsBase"] = stats.HitPoints.BaseValue,
                ["armor"] = stats.AC.ModifiedValue, ["touch"] = stats.AC.Touch, ["flatFooted"] = stats.AC.FlatFooted,
                ["fortitude"] = stats.GetStat(StatType.SaveFortitude).ModifiedValue,
                ["reflex"] = stats.GetStat(StatType.SaveReflex).ModifiedValue,
                ["will"] = stats.GetStat(StatType.SaveWill).ModifiedValue,
                ["cmd"] = cmd.Describe("CMD"), ["trip"] = trip.Describe("trip"), ["deniedDexRecovered"] = deniedDex,
                ["biteAttack"] = biteAttack, ["tailAttack"] = tailAttack,
                ["bite"] = Sprint16DamageLine(biteDamage), ["tail"] = Sprint16DamageLine(tailDamage),
                ["biteEntitySize"] = bite.Size.ToString(), ["biteRuleSize"] = biteStats.WeaponSize.ToString(),
                ["biteBlueprintBaseDice"] = bite.Blueprint.BaseDamage.ToString(),
                ["biteExplicitDice"] = bite.Blueprint.IsDamageDiceOverridden,
                ["tailBlueprintBaseDice"] = tail.Blueprint.BaseDamage.ToString(),
                ["tailExplicitDice"] = tail.Blueprint.IsDamageDiceOverridden,
                ["tailEntitySize"] = tail.Size.ToString(), ["tailRuleSize"] = tailStats.WeaponSize.ToString(),
                ["tailSecondary"] = tail.IsSecondary,
                ["tailSecondaryRuleOverride"] = tailStats.SecondaryWeapon,
                ["biteImprovedCritical"] = biteStats.DoubleCriticalEdge,
                ["perception"] = DescribeSprint16Skill(stats.GetStat(StatType.SkillPerception)),
                ["stealth"] = DescribeSprint16Skill(stats.GetStat(StatType.SkillStealth)),
                ["mobility"] = DescribeSprint16Skill(stats.GetStat(StatType.SkillMobility))
            }, "unmodified own-tier live creature matches the approved profile, primary bite and secondary tail");
        }

        private static void ExerciseSprint16DeathRollDamage(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner,
            string key, List<RuntimeTestAssertion> assertions, JArray rows)
        {
            ItemEntityWeapon bite = SummonLimbs.PrimaryWeapon(owner);
            CrocodilianRulesProfile profile = CrocodilianRulesPolicy.For(key);
            var observer = new Sprint16RuleObserver { Owner = owner, Target = fixture.Hostile };
            EventBus.Subscribe(observer);
            try
            {
                ProbeSprint16DeathRoll(owner, bite, observer, key + "-base", assertions, rows,
                    profile.DeathRollDiceCount + "d" + profile.DeathRollDieSides,
                    profile.StrengthModifier, profile.DeathRollBonus);

                // Real temporary native modifiers; never replace the static
                // blueprint or its profile. The penalty crosses below ten so
                // the no-invented-extra-negative-half rule is observable.
                foreach (int delta in new[] { 4, 7 - profile.Strength })
                {
                    var modifier = owner.Descriptor.Stats.Strength.AddModifier(delta,
                        null, "KMG_Sprint16_Disposable_Strength", ModifierDescriptor.UntypedStackable);
                    try
                    {
                        int ability = (int)Math.Floor((profile.Strength + delta - 10) / 2d);
                        ProbeSprint16DeathRoll(owner, bite, observer,
                            key + (delta > 0 ? "-strength-increase" : "-strength-penalty"),
                            assertions, rows, profile.DeathRollDiceCount + "d" +
                                profile.DeathRollDieSides, ability,
                            ability + Math.Max(0, ability) / 2);
                    }
                    finally { owner.Descriptor.Stats.Strength.RemoveModifier(modifier); }
                }

                BlueprintBuff growth = fixture.Blueprints.OfType<BlueprintBuff>().Single(value =>
                    value.name == "AnimalGrowthBuff" &&
                    value.ComponentsArray.OfType<ChangeUnitSize>().Any());
                Size sizeBeforeGrowth = owner.Descriptor.State.Size;
                BaseDamage beforeGrowth = Rulebook.Trigger(new RuleCalculateWeaponStats(owner, bite, null))
                    .DamageDescription.Select(value => value.CreateDamage()).OfType<PhysicalDamage>().Single();
                string diceBeforeGrowth = beforeGrowth.Dice.Rolls + "d" + (int)beforeGrowth.Dice.Dice;
                Buff grown = owner.Descriptor.AddBuff(growth, owner, TimeSpan.FromMinutes(1));
                try
                {
                    RuleCalculateWeaponStats current = Rulebook.Trigger(
                        new RuleCalculateWeaponStats(owner, bite, null));
                    BaseDamage live = current.DamageDescription.Select(value => value.CreateDamage())
                        .OfType<PhysicalDamage>().Single();
                    string dice = live.Dice.Rolls + "d" + (int)live.Dice.Dice;
                    ProbeSprint16DeathRoll(owner, bite, observer, key + "-animal-growth",
                        assertions, rows, dice, live.Bonus,
                        live.Bonus + Math.Max(0, owner.Descriptor.Stats.Strength.Bonus) / 2);
                    Sprint16Check(assertions, rows, key + "-growth-changes-dice",
                        grown != null && (int)owner.Descriptor.State.Size == (int)sizeBeforeGrowth + 1 &&
                            diceBeforeGrowth == profile.DeathRollDiceCount + "d" + profile.DeathRollDieSides &&
                            dice != diceBeforeGrowth && dice == (key == "crocodile" ? "2d6" : "4d6"),
                        new JObject { ["buff"] = growth.AssetGuid,
                            ["sizeBefore"] = sizeBeforeGrowth.ToString(), ["diceBefore"] = diceBeforeGrowth,
                            ["itemSize"] = bite.Size.ToString(), ["ruleSize"] = current.WeaponSize.ToString(),
                            ["size"] = owner.Descriptor.State.Size.ToString(), ["dice"] = dice },
                        "native Animal Growth changes the actual bite dice");
                }
                finally { if (grown != null) grown.Remove(); }

                var enhancements = new List<ItemEnchantment>();
                try
                {
                    foreach (string guid in new[] { EasternWeaponNamedBlueprints.FlamingGuid,
                        EasternWeaponBlueprints.NativeEnhancementOneGuid })
                        enhancements.Add(bite.AddEnchantment(fixture.Blueprints
                            .OfType<BlueprintWeaponEnchantment>().Single(value =>
                                value.AssetGuid == guid), null, null));
                    observer.ReorderEnergy = true;
                    ProbeSprint16DeathRoll(owner, bite, observer, key + "-flaming-plus-one-reordered",
                        assertions, rows, profile.DeathRollDiceCount + "d" +
                            profile.DeathRollDieSides, profile.StrengthModifier + 1,
                        profile.DeathRollBonus + 1, true);
                    observer.ReorderEnergy = false;
                    ExerciseSprint16DamageReduction(fixture, owner, bite, observer, key,
                        assertions, rows);
                }
                finally
                {
                    observer.ReorderEnergy = false;
                    foreach (ItemEnchantment enchantment in enhancements)
                        if (enchantment != null) bite.RemoveEnchantment(enchantment);
                }
            }
            finally { EventBus.Unsubscribe(observer); }
        }

        private static void ExerciseSprint16GrabDelivery(ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, string key, List<RuntimeTestAssertion> assertions, JArray rows)
        {
            UnitEntityData target = fixture.Hostile;
            SummonGrabComponent grab = SummonGrabComponent.Find(owner);
            ItemEntityWeapon bite = SummonLimbs.PrimaryWeapon(owner);
            ItemEntityWeapon tail = LiveLimbWeapons(owner).Single(value => !ReferenceEquals(value, bite));
            int bab = owner.Descriptor.Stats.BaseAttackBonus.BaseValue;
            var observer = new Sprint16RuleObserver { Owner = owner, Target = target };
            EventBus.Subscribe(observer);
            try
            {
                Sprint16Release(fixture, owner, grab);
                target.Descriptor.State.Size = owner.Descriptor.State.Size;
                owner.Descriptor.Stats.BaseAttackBonus.BaseValue = 100;
                UnityEngine.Random.InitState(FindNativeD20Seed(10));
                Rulebook.Trigger(new RuleAttackWithWeapon(owner, target, tail, 0));
                bool tailExcluded = observer.Attacks.Count == 1 && observer.Attacks[0].IsHit &&
                    observer.Checks.Count == 0 && observer.Damage.Count == 1 &&
                    SummonHoldComponent.HeldTarget(owner) == null;
                var attempts = new JArray();
                bool taken = false;
                // Native natural-one failures remain failures of that roll;
                // bounded deterministic seeds allow a later genuine hit/check.
                foreach (int natural in new[] { 10, 12, 14 })
                {
                    observer.Clear();
                    UnityEngine.Random.InitState(FindNativeD20Seed(natural));
                    Rulebook.Trigger(new RuleAttackWithWeapon(owner, target, bite, 0));
                    taken = ReferenceEquals(SummonHoldComponent.HeldTarget(owner), target);
                    attempts.Add(new JObject { ["attackNatural"] = natural,
                        ["held"] = taken, ["maneuvers"] = observer.Checks.Count,
                        ["damageEvents"] = observer.Damage.Count });
                    if (taken) break;
                }
                bool biteOnly = taken && observer.Checks.Count == 1 && observer.Damage.Count == 1 &&
                    ReferenceEquals(SummonGrappleLinks.EstablishingWeapon(owner, target), bite) &&
                    target.Get<UnitPartSwallowed>() == null;
                Sprint16Check(assertions, rows, key + "-actual-bite-only-grab",
                    tailExcluded && biteOnly, new JObject { ["woundingTailNeverGrabs"] = tailExcluded,
                        ["biteEstablishesExactLink"] = biteOnly, ["biteAttempts"] = attempts },
                    "actual tail hit does not grab; actual bite hit establishes one exact hold without an initial rider");
            }
            finally
            {
                EventBus.Unsubscribe(observer);
                Sprint16Release(fixture, owner, grab);
                owner.Descriptor.Stats.BaseAttackBonus.BaseValue = bab;
            }
        }

        private static void ExerciseSprint16DamageReduction(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner,
            ItemEntityWeapon bite, Sprint16RuleObserver observer, string key,
            List<RuntimeTestAssertion> assertions, JArray rows)
        {
            BlueprintBuff stoneskin = fixture.Blueprints.OfType<BlueprintBuff>().Single(value =>
                value.name == "StoneskinBuff");
            Buff shield = fixture.Hostile.Descriptor.AddBuff(stoneskin, fixture.Hostile,
                TimeSpan.FromMinutes(1));
            int bab = owner.Descriptor.Stats.BaseAttackBonus.BaseValue;
            try
            {
                owner.Descriptor.Stats.BaseAttackBonus.BaseValue = 100;
                fixture.Hostile.Descriptor.State.AddCondition(UnitCondition.ImmuneToCombatManeuvers, null);
                observer.Clear();
                UnityEngine.Random.InitState(FindNativeD20Seed(10));
                Rulebook.Trigger(new RuleAttackWithWeapon(owner, fixture.Hostile, bite, 0));
                RuleDealDamage hit = observer.Damage.SingleOrDefault();
                int attacks = observer.Attacks.Count;
                observer.Clear();
                int dealt;
                SummonGrappleDamage.DealDeathRollDamage(owner, fixture.Hostile, bite, null, out dealt);
                RuleDealDamage roll = observer.Damage.SingleOrDefault();
                PhysicalDamage hitPhysical = hit == null ? null : hit.DamageBundle.WeaponDamage as PhysicalDamage;
                PhysicalDamage rollPhysical = roll == null ? null : roll.DamageBundle.WeaponDamage as PhysicalDamage;
                bool exact = shield != null && hitPhysical != null && rollPhysical != null &&
                    attacks == 1 && hit.AttackRoll != null && !hit.AttackRoll.IsCriticalConfirmed &&
                    hitPhysical.Form == rollPhysical.Form &&
                    hitPhysical.MaterialsMask == rollPhysical.MaterialsMask &&
                    hitPhysical.Enchantment == 1 && rollPhysical.Enchantment == 1 &&
                    ReferenceEquals(hit.DamageBundle.Weapon, bite) &&
                    ReferenceEquals(roll.DamageBundle.Weapon, bite) &&
                    hit.DamageWithoutReduction > hit.DamageBeforeDifficulty &&
                    roll.DamageWithoutReduction > roll.DamageBeforeDifficulty &&
                    observer.Attacks.Count == 0 && observer.Checks.Count == 0 &&
                    observer.WeaponAttacks == 0 && roll.AttackRoll == null;
                Sprint16Check(assertions, rows, key + "-native-bite-dr-attribution", exact,
                    new JObject { ["defense"] = stoneskin.AssetGuid,
                        ["nativeBite"] = hit == null ? null : Sprint16DamageEvent(hit),
                        ["deathRoll"] = roll == null ? null : Sprint16DamageEvent(roll) },
                    "actual bite and Death Roll preserve weapon, form, material and +1; native DR applies to both");
            }
            finally
            {
                fixture.Hostile.Descriptor.State.RemoveConditionAll(UnitCondition.ImmuneToCombatManeuvers);
                owner.Descriptor.Stats.BaseAttackBonus.BaseValue = bab;
                if (shield != null) shield.Remove();
            }
        }

        private static void ProbeSprint16DeathRoll(UnitEntityData owner,
            ItemEntityWeapon bite, Sprint16RuleObserver observer, string label,
            List<RuntimeTestAssertion> assertions, JArray rows,
            string expectedDice, int expectedBiteBonus, int expectedRollBonus,
            bool supplemental = false)
        {
            observer.Target.Descriptor.Damage = 0;
            observer.Clear();
            RuleCalculateWeaponStats before = Rulebook.Trigger(
                new RuleCalculateWeaponStats(owner, bite, null));
            BaseDamage[] biteDamage = before.DamageDescription.Select(value =>
                value.CreateDamage()).ToArray();
            int dealt;
            string detail = SummonGrappleDamage.DealDeathRollDamage(owner, observer.Target,
                bite, null, out dealt);
            RuleDealDamage actual = observer.Damage.SingleOrDefault();
            BaseDamage baseBite = biteDamage.OfType<PhysicalDamage>().SingleOrDefault();
            BaseDamage baseRoll = actual == null ? null : actual.DamageBundle.WeaponDamage;
            var energy = actual == null ? new EnergyDamage[0] :
                actual.DamageBundle.OfType<EnergyDamage>().ToArray();
            bool exact = baseBite != null && baseRoll != null &&
                baseBite.Dice.Rolls + "d" + (int)baseBite.Dice.Dice == expectedDice &&
                baseRoll.Dice.Rolls == baseBite.Dice.Rolls &&
                baseRoll.Dice.Dice == baseBite.Dice.Dice &&
                baseBite.Bonus == expectedBiteBonus && baseRoll.Bonus == expectedRollBonus &&
                ReferenceEquals(actual.DamageBundle.Weapon, bite) &&
                ReferenceEquals(actual.Initiator, owner) && dealt == actual.Damage &&
                actual.AttackRoll == null && observer.Attacks.Count == 0 &&
                observer.WeaponAttacks == 0 && observer.Checks.Count == 0 &&
                owner.Descriptor.Stats.Strength.Bonus == (int)Math.Floor(
                    (owner.Descriptor.Stats.Strength.ModifiedValue - 10) / 2d) &&
                (supplemental ? observer.Reordered >= 2 &&
                    biteDamage[0] is EnergyDamage && detail.Contains(";baseBiteIndex=1;") &&
                    energy.Length == 1 && energy[0].Dice.Rolls == 1 &&
                    energy[0].Dice.Dice == DiceType.D6 && energy[0].Bonus == 0 &&
                    ((PhysicalDamage)baseRoll).Enchantment == 1 : energy.Length == 0);
            Sprint16Check(assertions, rows, label, exact, new JObject {
                ["strengthScore"] = owner.Descriptor.Stats.Strength.ModifiedValue,
                ["strengthModifier"] = owner.Descriptor.Stats.Strength.Bonus,
                ["creatureSize"] = owner.Descriptor.State.Size.ToString(),
                ["itemSize"] = bite.Size.ToString(), ["ruleSize"] = before.WeaponSize.ToString(),
                ["bite"] = new JArray(biteDamage.Select(Sprint16DamageLine)),
                ["deathRoll"] = actual == null ? null : Sprint16DamageEvent(actual),
                ["productionDetail"] = detail, ["attackRolls"] = observer.Attacks.Count,
                ["weaponAttacks"] = observer.WeaponAttacks, ["maneuvers"] = observer.Checks.Count,
                ["reordered"] = observer.Reordered
            }, "bite " + expectedDice + "+" + expectedBiteBonus + " -> death roll " +
                expectedDice + "+" + expectedRollBonus + "; no attack-only event; exact base identity");
        }

        private static void Sprint16Release(ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, SummonGrabComponent grab)
        {
            SummonHoldComponent.ReleaseLink(owner, fixture.Hostile, grab, true);
            if (owner.Get<UnitPartGrappleInitiator>() != null)
                owner.Remove<UnitPartGrappleInitiator>();
            ResetExpandedSummoningHostile(fixture);
            fixture.Hostile.Descriptor.State.RemoveConditionAll(UnitCondition.Prone);
        }

        private static Buff Sprint16EstablishHold(ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, SummonGrabComponent grab, Size size, bool laterRound)
        {
            Sprint16Release(fixture, owner, grab);
            fixture.Hostile.Descriptor.State.Size = size;
            owner.Descriptor.Stats.BaseAttackBonus.BaseValue = 100;
            PlaceExpandedSummoningUnit(fixture.Hostile, owner.Position + Vector3.forward);
            UnityEngine.Random.InitState(FindNativeD20Seed(20));
            if (!grab.TryGrab(fixture.Hostile, SummonLimbs.PrimaryWeapon(owner), true))
                throw new InvalidOperationException("Crocodilian request-local native grapple failed.");
            Buff held = SummonHoldComponent.HeldState(owner, fixture.Hostile, grab);
            if (held == null) throw new InvalidOperationException("Exact reciprocal held state is absent.");
            if (laterRound)
            {
                UnityEngine.Random.InitState(FindNativeD20Seed(10));
                held.TickMechanics();
                if (!ReferenceEquals(SummonHoldComponent.HeldTarget(owner), fixture.Hostile))
                    throw new InvalidOperationException("Fixture target unexpectedly escaped its native hold.");
            }
            return held;
        }

        private static void ExerciseSprint16Maintain(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner,
            string key, List<RuntimeTestAssertion> assertions, JArray rows)
        {
            UnitEntityData target = fixture.Hostile;
            SummonGrabComponent grab = SummonGrabComponent.Find(owner);
            var observer = new Sprint16RuleObserver { Owner = owner, Target = target };
            EventBus.Subscribe(observer);
            try
            {
                foreach (string variant in new[] { "fresh-hold", "own-size", "already-prone",
                    "prone-immune", "grew-ineligible", "smaller" })
                {
                    Size size = variant == "smaller" ? (Size)((int)owner.Descriptor.State.Size - 1) :
                        owner.Descriptor.State.Size;
                    Buff held = Sprint16EstablishHold(fixture, owner, grab, size,
                        variant != "fresh-hold");
                    bool immunity = variant == "prone-immune";
                    if (variant == "already-prone") target.Descriptor.State.AddCondition(UnitCondition.Prone, null);
                    if (immunity) target.Descriptor.State.AddConditionImmunity(UnitCondition.Prone);
                    if (variant == "grew-ineligible")
                        target.Descriptor.State.Size = (Size)((int)owner.Descriptor.State.Size + 1);
                    observer.Clear();
                    int claimed = -1;
                    string outcome;
                    bool proneDuring;
                    try
                    {
                        UnityEngine.Random.InitState(FindNativeD20Seed(20));
                        outcome = SummonHoldComponent.MaintainLink(owner, target, grab, null,
                            owner.Descriptor.Buffs.GetBuff(grab.HoldBuff), held, ref claimed);
                        proneDuring = target.Descriptor.State.HasCondition(UnitCondition.Prone);
                    }
                    finally
                    { if (immunity) target.Descriptor.State.RemoveConditionImmunity(UnitCondition.Prone); }
                    bool swallow = key == "dire-crocodile" && variant == "smaller";
                    bool plain = variant == "fresh-hold" || variant == "grew-ineligible";
                    string expected = swallow ? "swallowed:" : plain ? "maintained:" : "death-roll:";
                    int eventCount = observer.Damage.Count;
                    JObject initial = observer.Damage.Count == 1 ? Sprint16DamageEvent(observer.Damage[0]) : null;
                    bool prone = target.Descriptor.State.HasCondition(UnitCondition.Prone);
                    bool exact = outcome.StartsWith(expected, StringComparison.Ordinal) &&
                        observer.Checks.Count == 1 && observer.Damage.Count == 1 &&
                        observer.Attacks.Count == 0 && observer.WeaponAttacks == 0 &&
                        (target.Get<UnitPartSwallowed>() != null) == swallow &&
                        (plain || swallow || (proneDuring == !immunity && prone == !immunity));
                    string replay = "not-applicable";
                    if (!swallow && variant != "fresh-hold")
                    {
                        replay = SummonHoldComponent.MaintainLink(owner, target, grab, null,
                            owner.Descriptor.Buffs.GetBuff(grab.HoldBuff), held, ref claimed);
                        exact = exact && replay.StartsWith("crocodilian-maintain:already-resolved", StringComparison.Ordinal) &&
                            observer.Checks.Count == 1 && observer.Damage.Count == eventCount;
                    }
                    Sprint16Check(assertions, rows, key + "-maintain-" + variant, exact,
                        new JObject { ["adaptation"] = CrocodilianRulesPolicy.MaintainAdaptation,
                            ["outcome"] = outcome, ["replay"] = replay, ["claimedRound"] = claimed,
                            ["damage"] = initial, ["damageEvents"] = eventCount,
                            ["maneuvers"] = observer.Checks.Count, ["prone"] = prone,
                            ["swallowed"] = target.Get<UnitPartSwallowed>() != null },
                        "one actual maintain -> " + expected + "; no duplicate rider or second attack");
                    if (swallow) ExerciseSprint16SwallowedCadence(fixture, owner, grab, observer,
                        assertions, rows);
                    Sprint16Release(fixture, owner, grab);
                }
                // The live buff component owns its own serialized round guard.
                // Exercise that instance as well as the explicit method above.
                Buff next = Sprint16EstablishHold(fixture, owner, grab,
                    owner.Descriptor.State.Size, true);
                Buff hold = owner.Descriptor.Buffs.GetBuff(grab.HoldBuff);
                observer.Clear();
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                hold.TickMechanics();
                int once = observer.Damage.Count;
                hold.TickMechanics();
                bool duplicateStopped = once == 1 && observer.Damage.Count == 1 &&
                    observer.Checks.Count == 1;
                UnityEngine.Random.InitState(FindNativeD20Seed(10));
                next.TickMechanics();
                observer.Clear();
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                hold.TickMechanics();
                Sprint16Check(assertions, rows, key + "-live-hold-component-round-guard",
                    duplicateStopped && observer.Damage.Count == 1 && observer.Checks.Count == 1,
                    new JObject { ["duplicateStopped"] = duplicateStopped,
                        ["nextRoundDamageEvents"] = observer.Damage.Count,
                        ["nextRoundChecks"] = observer.Checks.Count },
                    "live hold buff rejects a duplicate tick and allows exactly one later-round rider");
                Sprint16Release(fixture, owner, grab);
                ExerciseSprint16LethalRoll(fixture, owner, grab, key, assertions, rows);
            }
            finally
            {
                EventBus.Unsubscribe(observer);
                Sprint16Release(fixture, owner, grab);
            }
        }

        private static void ExerciseSprint16LethalRoll(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner,
            SummonGrabComponent grab, string key, List<RuntimeTestAssertion> assertions, JArray rows)
        {
            UnitEntityData victim = CastExpandedSummoningQuietUnit(fixture, "wolf", fixture.Hostile);
            var observer = new Sprint16RuleObserver { Owner = owner, Target = victim };
            EventBus.Subscribe(observer);
            try
            {
                victim.Descriptor.State.Size = owner.Descriptor.State.Size;
                victim.Descriptor.Stats.Constitution.BaseValue = 3;
                victim.Descriptor.Damage = Math.Max(0, victim.Descriptor.Stats.HitPoints.ModifiedValue - 1);
                PlaceExpandedSummoningUnit(victim, owner.Position + Vector3.forward);
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                bool held = grab.TryGrab(victim, SummonLimbs.PrimaryWeapon(owner), true);
                Buff state = SummonHoldComponent.HeldState(owner, victim, grab);
                UnityEngine.Random.InitState(FindNativeD20Seed(10));
                if (state != null) state.TickMechanics();
                observer.Clear();
                int claimed = -1;
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                string outcome = SummonHoldComponent.MaintainLink(owner, victim, grab, null,
                    owner.Descriptor.Buffs.GetBuff(grab.HoldBuff), state, ref claimed);
                bool deadOnDamageReturn = victim.Descriptor.State.IsDead;
                int damageBeforeLife = observer.Damage.Count;
                // RuleDealDamage records damage; the native life controller
                // settles death on its subsequent tick. Tick only this exact
                // disposable victim, then the production holder's round path.
                typeof(Kingmaker.Controllers.Units.UnitLifeController).GetMethod("TickOnUnit",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Invoke(new Kingmaker.Controllers.Units.UnitLifeController(), new object[] { victim });
                int damageAfterLife = observer.Damage.Count;
                Buff remainingHold = owner.Descriptor.Buffs.GetBuff(grab.HoldBuff);
                if (remainingHold != null) remainingHold.TickMechanics();
                int checksAfterHold = observer.Checks.Count;
                string deadReplay = SummonHoldComponent.MaintainLink(owner, victim, grab, null,
                    remainingHold, state, ref claimed);
                bool dead = victim.Descriptor.State.IsDead;
                bool free = SummonHoldComponent.HeldTarget(owner) == null &&
                    SummonGrappleLinks.EstablishingWeapon(owner, victim) == null &&
                    victim.Get<UnitPartGrappleTarget>() == null &&
                    !victim.Descriptor.HasFact(grab.GrappledBuff);
                Sprint16Check(assertions, rows, key + "-lethal-roll-cleanup",
                    held && dead && free && damageBeforeLife == 1 && observer.Damage.Count == 1 &&
                    observer.Checks.Count == checksAfterHold &&
                    (deadReplay == "refused:no-exact-held-target" ||
                     deadReplay == "released:invalid-crocodilian-owner-or-target") &&
                    observer.Attacks.Count == 0 && outcome.StartsWith("death-roll:", StringComparison.Ordinal),
                    new JObject { ["held"] = held, ["dead"] = dead, ["free"] = free,
                        ["deadOnDamageReturn"] = deadOnDamageReturn,
                        ["deadReplay"] = deadReplay, ["beforeNativeLife"] = damageBeforeLife,
                        ["afterNativeLife"] = damageAfterLife,
                        ["outcome"] = outcome, ["damageEvents"] = observer.Damage.Count,
                        ["damage"] = new JArray(observer.Damage.Select(Sprint16DamageEvent)) },
                    "one lethal Death Roll; exact native per-unit life tick then production hold tick releases reciprocal state without more damage");
            }
            finally
            {
                EventBus.Unsubscribe(observer);
                SummonHoldComponent.ReleaseLink(owner, victim, grab, true);
                if (owner.Get<UnitPartGrappleInitiator>() != null)
                    owner.Remove<UnitPartGrappleInitiator>();
                DisposeExpandedSummoningUnits(fixture.Created, new[] { victim });
            }
        }

        private static void ExerciseSprint16SwallowedCadence(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner,
            SummonGrabComponent grab, Sprint16RuleObserver observer,
            List<RuntimeTestAssertion> assertions, JArray rows)
        {
            UnitEntityData target = fixture.Hostile;
            UnitPartSwallowed part = target.Get<UnitPartSwallowed>();
            Buff swallowed = target.Descriptor.Buffs.GetBuff(grab.SwallowedBuff);
            RuleDealDamage initial = observer.Damage.SingleOrDefault();
            TimeSpan gameTime = Game.Instance.TimeController.GameTime;
            PropertyInfo nextTick = typeof(Buff).GetProperty("NextTickTime",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (nextTick == null || !nextTick.CanWrite)
                throw new MissingMemberException("Native swallowed Buff.NextTickTime setter is unavailable.");
            double initialDelay = swallowed == null ? -1d :
                ((TimeSpan)nextTick.GetValue(swallowed, null) - gameTime).TotalSeconds;
            bool exact = part != null && swallowed != null &&
                ReferenceEquals(part.Swallower.Value, owner) &&
                ReferenceEquals(swallowed.MaybeContext.MaybeCaster, owner) &&
                part.SecondsToBreakFreeAttempt == 6f && initialDelay > 0d && initial != null &&
                initial.DamageBundle.Count() == 1 && initial.DamageBundle.First.Dice.Rolls == 3 &&
                initial.DamageBundle.First.Dice.Dice == DiceType.D6 &&
                initial.DamageBundle.First.Bonus == 13;
            var ticks = new JArray();
            for (int round = 1; round <= 3 && swallowed != null; round++)
            {
                observer.Clear();
                // Advance only this buff's native due boundary, not world
                // time. BuffCollection owns delivery and scheduling; calling
                // it again without advancing the boundary must do nothing.
                int priorRound = swallowed.RoundNumber;
                nextTick.SetValue(swallowed, gameTime, null);
                target.Descriptor.Buffs.UpdateNextEvent();
                target.Descriptor.Buffs.Tick();
                RuleDealDamage damage = observer.Damage.SingleOrDefault();
                BaseDamage chunk = damage == null ? null : damage.DamageBundle.First;
                bool one = chunk is PhysicalDamage && damage.DamageBundle.Count() == 1 &&
                    chunk.Dice.Rolls == 3 && chunk.Dice.Dice == DiceType.D6 && chunk.Bonus == 13 &&
                    ReferenceEquals(damage.Initiator, owner) && damage.AttackRoll == null &&
                    observer.Attacks.Count == 0 && observer.Checks.Count == 0 &&
                    swallowed.RoundNumber == priorRound + 1 &&
                    (TimeSpan)nextTick.GetValue(swallowed, null) > gameTime;
                target.Descriptor.Buffs.Tick();
                one = one && observer.Damage.Count == 1;
                exact = exact && one;
                ticks.Add(new JObject { ["laterRound"] = round, ["oneOwnedBundle"] = one,
                    ["nativeRound"] = swallowed.RoundNumber,
                    ["nextTickDelaySeconds"] = ((TimeSpan)nextTick.GetValue(swallowed, null) - gameTime).TotalSeconds,
                    ["damage"] = damage == null ? null : Sprint16DamageEvent(damage) });
            }
            observer.Clear();
            if (part != null) part.TryToBreakFree();
            bool timing = part != null && !part.ShouldBeFree && observer.Checks.Count == 0 &&
                part.SecondsToBreakFreeAttempt > 0;
            if (part != null)
            {
                // Only the request-local native countdown is advanced to its
                // due boundary; the native check itself is not replaced.
                part.SecondsToBreakFreeAttempt = 0;
                target.Descriptor.Stats.BaseAttackBonus.BaseValue = 200;
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                part.TryToBreakFree();
            }
            bool nativeEscape = part != null && part.ShouldBeFree && observer.Checks.Count == 1;
            observer.Clear();
            UnitPartSwallowWhole swallower = owner.Get<UnitPartSwallowWhole>();
            if (swallower != null) swallower.Free(target);
            bool removal = target.Get<UnitPartSwallowed>() == null &&
                !target.Descriptor.HasFact(grab.SwallowedBuff) && observer.Damage.Count == 0 &&
                !target.Descriptor.State.HasCondition(UnitCondition.CantAct);
            Sprint16Check(assertions, rows, "dire-crocodile-swallowed-cadence",
                exact && timing && nativeEscape && removal, new JObject {
                    ["initialBite"] = initial == null ? null : Sprint16DamageEvent(initial),
                    ["initialRoundDamageDelaySeconds"] = initialDelay,
                    ["laterRoundTicks"] = ticks, ["nativeInitialSixSecondGate"] = timing,
                    ["nativeEscapeCheck"] = nativeEscape, ["removalWithoutDamage"] = removal,
                    ["interiorDisposition"] = "OwnerAcceptedEngineLimitation: SWALLOW_WHOLE_INTERIOR_AC_HP_UNMODELED"
                }, "one initial bite; one owned 3d6+13 per later buff round; native escape; zero removal damage");
        }
    }
}
