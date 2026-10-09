using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Designers.Mechanics.Buffs;
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
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Parts;
using KingmakerGunslinger.Summoning;
using KingmakerGunslinger.Blueprints;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private void CheckSprint17Salamander(string id, bool pass, JObject row, string expected)
        {
            row = (JObject)row.DeepClone(); row["key"] = "salamander"; row["check"] = id; row["passed"] = pass;
            _serpentineBodyRows.Add(row);
            _serpentineBodyAssertions.Add(Assertion("sprint17-salamander-" + id, expected,
                row.ToString(Formatting.None), pass, "Exact production Salamander; fixture inputs disclosed; all other Sprint gates remain required."));
        }

        private static bool Sprint17SalamanderFirePacket(BaseDamage damage)
        {
            var fire = damage as EnergyDamage;
            return fire != null && fire.EnergyType == DamageEnergyType.Fire && fire.Dice.Rolls == 1 &&
                fire.Dice.Dice == DiceType.D6 && fire.Bonus == 0 && (!fire.CriticalModifier.HasValue || fire.CriticalModifier.Value == 1);
        }

        private IEnumerable<int> ReviewSprint17SalamanderMechanics(ExpandedSummoningCorrectionFixture fixture)
        {
            // The preceding snake signatures already own a hostile target.
            // Reuse it rather than overwrite its only cleanup receipt.
            if (fixture.Hostile == null || fixture.Hostile.Destroyed)
                CreateExpandedSummoningCorrectionHostile(fixture);
            var target = fixture.Hostile;
            var owner = CastSprint17FinalSnake(fixture, "salamander");
            foreach (int step in WaitSprint17FinalAppearance(new[] { owner })) yield return step;
            var grab = SummonGrabComponent.Find(owner);
            var spear = owner.Body.PrimaryHand.MaybeWeapon;
            var tail = owner.Body.AdditionalLimbs.Single().MaybeWeapon;
            if (grab == null || !ReferenceEquals(grab.SalamanderProfileOwner, owner.Blueprint))
                throw new InvalidOperationException("Exact Salamander tail grab owner is absent.");
            var observer = new Sprint16RuleObserver { Owner = owner, Target = target };
            var random = UnityEngine.Random.state;
            int bonus = owner.Stats.AdditionalAttackBonus.BaseValue, cmb = owner.Stats.AdditionalCMB.BaseValue;
            var powerAttack = owner.Descriptor.ActivatableAbilities.Enumerable.Single(a =>
                a.Blueprint.AssetGuid == EasternWeaponNamedBlueprints.PowerAttackToggleGuid);
            bool powerAttackBefore = powerAttack.IsOn;
            EventBus.Subscribe(observer);
            try
            {
                owner.Stats.AdditionalAttackBonus.BaseValue = 100;
                owner.Stats.AdditionalCMB.BaseValue = 100;
                target.Stats.HitPoints.BaseValue = 100000; target.Descriptor.Damage = 0;
                powerAttack.IsOn = false; powerAttack.Stop(true);
                var incomingOwner = CastExpandedSummoningQuietUnit(fixture, "wolf", target);
                try { ProbeSprint17SalamanderDefenses(fixture, owner, incomingOwner); }
                finally { DisposeExpandedSummoningUnits(fixture.Created, new[] { incomingOwner }); }
                PlaceExpandedSummoningUnit(target, owner.Position + Vector3.forward);
                foreach (string kind in new[] { "spear-hit", "spear-miss", "spear-critical", "spear-fire-resistance", "foreign-weapon" })
                {
                    observer.Clear();
                    bool critical = kind == "spear-critical", miss = kind == "spear-miss", resistance = kind == "spear-fire-resistance";
                    var weapon = kind == "foreign-weapon" ? SummonLimbs.PrimaryWeapon(fixture.Caster) : spear;
                    if (weapon == null || kind == "foreign-weapon" && ReferenceEquals(weapon.Blueprint, spear.Blueprint))
                        throw new InvalidOperationException("Owned foreign weapon negative control unavailable.");
                    observer.BeforeAttackRollForFixture = evt => { if (critical) { evt.AutoCriticalThreat = true; evt.AutoCriticalConfirmation = true; } };
                    var resistanceFact = resistance ? target.Descriptor.AddFact(fixture.Blueprints.OfType<BlueprintUnitFact>()
                        .Single(b => b.AssetGuid == "24700a71dd3dc844ea585345f6dd18f6")) : null;
                    try
                    {
                        UnityEngine.Random.InitState(FindNativeD20Seed(miss ? 1 : 10));
                        var attack = Rulebook.Trigger(new RuleAttackWithWeapon(owner, target, weapon, 0));
                        var damage = attack.MeleeDamage;
                        bool resolved = damage != null && damage.ResultDamage != null;
                        var fire = damage == null ? new BaseDamage[0] : damage.DamageBundle.Where(d => d is EnergyDamage).ToArray();
                        bool heat = Sprint17ObservationPolicy.ResolvedStrike(miss, attack.AttackRoll.IsHit,
                            resolved, observer.Damage.Count) && (miss || (kind == "foreign-weapon" ? fire.Length == 0 :
                                fire.Length == 1 && Sprint17SalamanderFirePacket(fire[0])));
                        bool reduction = !resistance || resolved && damage.ResultDamage.Any(d =>
                            ReferenceEquals(d.Source, fire.Single()) && d.FinalValue == 0 && d.Reduction >= d.RolledValue);
                        bool crit = !critical || attack.AttackRoll.IsCriticalConfirmed && damage != null &&
                            damage.DamageBundle.OfType<PhysicalDamage>().Single().CriticalModifier == 3;
                        CheckSprint17Salamander(kind, heat && reduction && crit && observer.WeaponAttacks == 1 &&
                            observer.Checks.Count == 0 && SummonHoldComponent.HeldTarget(owner) == null,
                            new JObject { ["hit"] = attack.AttackRoll.IsHit, ["critical"] = attack.AttackRoll.IsCriticalConfirmed,
                                ["fixture"] = "owned +100 accuracy/CMB; native d20 seed; explicit native auto threat/confirmation in critical case only",
                                ["damage"] = damage == null ? null : Sprint16DamageEvent(damage),
                                ["resolved"] = resolved, ["damageEvents"] = observer.Damage.Count,
                                ["values"] = !resolved ? new JArray() : new JArray(damage.ResultDamage.Select(d => new JObject {
                                    ["source"] = Sprint16DamageLine(d.Source), ["rolled"] = d.RolledValue, ["reduction"] = d.Reduction, ["final"] = d.FinalValue })),
                                ["grappleChecks"] = observer.Checks.Count },
                            "owned spear heat follows hit/miss/critical/native fire resistance; foreign weapon receives no heat; spear never grabs");
                    }
                    finally { if (resistanceFact != null) target.Descriptor.RemoveFact(resistanceFact); }
                }
                observer.BeforeAttackRollForFixture = null; observer.Clear();
                var flaming = spear.AddEnchantment(fixture.Blueprints.OfType<BlueprintWeaponEnchantment>()
                    .Single(b => b.AssetGuid == EasternWeaponNamedBlueprints.FlamingGuid), null, null);
                try
                {
                    var weaponStats = Rulebook.Trigger(new RuleCalculateWeaponStats(owner, spear, null));
                    var repeated = weaponStats.DamageDescription.ToArray();
                    // Re-deliver the very same native weapon-stats event to the
                    // exact component. Its per-rule claim must not add heat again.
                    owner.Descriptor.Buffs.Enumerable.SelectMany(b => b.Components)
                        .OfType<SummonSalamanderHeat>().Single().OnEventAboutToTrigger(weaponStats);
                    bool replayUnchanged = weaponStats.DamageDescription.SequenceEqual(repeated);
                    observer.Clear(); UnityEngine.Random.InitState(FindNativeD20Seed(10));
                    var attack = Rulebook.Trigger(new RuleAttackWithWeapon(owner, target, spear, 0));
                    var damage = attack.MeleeDamage;
                    CheckSprint17Salamander("supplemental-and-replay", replayUnchanged && repeated.Length == 3 &&
                        damage != null && damage.DamageBundle.Count() == 3 &&
                        damage.DamageBundle.OfType<EnergyDamage>().Count(Sprint17SalamanderFirePacket) == 2 &&
                        ReferenceEquals(damage.DamageBundle.Weapon, spear) && observer.WeaponAttacks == 1 &&
                        observer.Checks.Count == 0,
                        new JObject { ["fixture"] = "native Flaming on this owned spear only; duplicate delivery of the same weapon-stats event",
                            ["unchangedReplay"] = replayUnchanged, ["damage"] = damage == null ? null : Sprint16DamageEvent(damage) },
                        "native supplemental Flaming and own Heat remain separate 1d6 packets; same event replay adds none; spear attribution and no grab are preserved");
                }
                finally { if (flaming != null) spear.RemoveEnchantment(flaming); }
                observer.Clear();
                target.Descriptor.State.Size = Size.Large;
                bool oversized = grab.TryGrab(target, tail, true);
                target.Descriptor.State.Size = Size.Medium;
                bool missGrab = grab.TryGrab(target, tail, false), spearGrab = grab.TryGrab(target, spear, true);
                CheckSprint17Salamander("tail-grab-rejections", !oversized && !missGrab && !spearGrab &&
                    observer.Checks.Count == 0 && observer.Damage.Count == 0,
                    new JObject { ["oversized"] = oversized, ["miss"] = missGrab, ["spear"] = spearGrab },
                    "only a successful tail hit against same-size-or-smaller live prey can grab");
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                var tailAttack = Rulebook.Trigger(new RuleAttackWithWeapon(owner, target, tail, 0));
                var held = SummonHoldComponent.HeldState(owner, target, grab);
                CheckSprint17Salamander("tail-delivery", tailAttack.AttackRoll.IsHit && held != null &&
                    ReferenceEquals(SummonHoldComponent.HeldTarget(owner), target) &&
                    ReferenceEquals(SummonGrappleLinks.EstablishingWeapon(owner, target), tail) &&
                    observer.WeaponAttacks == 1 && observer.Checks.Count == 1 && observer.Damage.Count == 2 &&
                    observer.Damage.Count(d => d.AttackRoll == null) == 1 &&
                    observer.Damage.All(d => d.DamageBundle.Count() == 2 && d.DamageBundle.Count(Sprint17SalamanderFirePacket) == 1),
                    new JObject { ["damage"] = new JArray(observer.Damage.Select(Sprint16DamageEvent)), ["checks"] = observer.Checks.Count },
                    "one actual tail strike, one grapple check and one2d6+4/1d6fire initial constrict; no extra attack");
                if (held == null) throw new InvalidOperationException("Actual tail grab did not establish a hold.");
                observer.Clear(); var hold = owner.Descriptor.Buffs.GetBuff(grab.HoldBuff);
                hold.TickMechanics();
                CheckSprint17Salamander("application-frame", observer.Damage.Count == 0 && observer.Checks.Count == 0,
                    new JObject { ["damage"] = observer.Damage.Count, ["checks"] = observer.Checks.Count }, "application frame never repeats constrict/maintain");
                UnityEngine.Random.InitState(FindNativeD20Seed(10)); held.TickMechanics();
                observer.Clear(); UnityEngine.Random.InitState(FindNativeD20Seed(20)); hold.TickMechanics();
                int first = observer.Damage.Count, checks = observer.Checks.Count; hold.TickMechanics();
                CheckSprint17Salamander("later-maintain-once", first == 2 && checks == 1 && observer.Damage.Count == 2 &&
                    observer.Checks.Count == 1 && observer.WeaponAttacks == 0 && observer.Attacks.Count == 0 &&
                    observer.Damage.All(d => ReferenceEquals(d.DamageBundle.Weapon, tail) && d.DamageBundle.Count() == 2 &&
                        d.DamageBundle.Count(Sprint17SalamanderFirePacket) == 1),
                    new JObject { ["damage"] = new JArray(observer.Damage.Select(Sprint16DamageEvent)), ["checks"] = observer.Checks.Count },
                    "one later maintain resolves tail damage plus one constrict; duplicate tick adds no roll, damage or heat");
                Sprint16Release(fixture, owner, grab);
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                if (!grab.TryGrab(target, tail, true)) throw new InvalidOperationException("Exact live tail link required for constrict modifier probes.");
                foreach (int delta in new[] { 0, 4, -9 })
                {
                    var modifier = owner.Stats.Strength.AddModifier(delta, null, "KMG_Sprint17_Salamander_Constrict", ModifierDescriptor.UntypedStackable);
                    try { ProbeSprint17SalamanderConstrict(owner, target, grab, observer, "strength-" + (16 + delta), 2); }
                    finally { owner.Stats.Strength.RemoveModifier(modifier); }
                }
                var enlarge = fixture.Blueprints.OfType<BlueprintBuff>().Single(b => b.name == "EnlargePersonBuff" && b.ComponentsArray.OfType<ChangeUnitSize>().Any());
                Buff enlarged = owner.Descriptor.AddBuff(enlarge, owner, TimeSpan.FromMinutes(1));
                try { ProbeSprint17SalamanderConstrict(owner, target, grab, observer, "native-size", 3); }
                finally { if (enlarged != null) enlarged.Remove(); }
                ProbeSprint17SalamanderConstrict(owner, target, grab, observer, "restored", 2);
                Sprint16Release(fixture, owner, grab);
                ExerciseSprint17SalamanderTerminal(fixture, owner, grab, tail);
            }
            finally
            {
                EventBus.Unsubscribe(observer); observer.BeforeAttackRollForFixture = null;
                Sprint16Release(fixture, owner, grab); owner.Stats.AdditionalAttackBonus.BaseValue = bonus;
                owner.Stats.AdditionalCMB.BaseValue = cmb; UnityEngine.Random.state = random;
                powerAttack.IsOn = powerAttackBefore;
                if (!powerAttackBefore) powerAttack.Stop(true);
                DisposeExpandedSummoningUnits(fixture.Created, new[] { owner, target });
                // Keep the exact destroyed hostile receipt until the outer
                // finalizer disposes its owned blueprint and verifies census.
            }
            yield return 0; yield return 0;
        }

        private void ProbeSprint17SalamanderDefenses(ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, UnitEntityData source)
        {
            int injury = owner.Descriptor.Damage;
            try
            {
                foreach (DamageEnergyType type in new[] { DamageEnergyType.Fire, DamageEnergyType.Cold })
                {
                    var packet = new EnergyDamage(new DiceFormula(1, DiceType.D6), type) { PreRolledValue = 6 };
                    var rule = Rulebook.Trigger(new RuleDealDamage(source, owner, new DamageBundle(packet)));
                    var result = rule.ResultDamage.Single(v => ReferenceEquals(v.Source, packet));
                    CheckSprint17Salamander("defense-" + type, type == DamageEnergyType.Fire ?
                        Sprint17ObservationPolicy.FireImmunity(packet.Immune, rule.Damage) :
                        !packet.Immune && result.FinalValue == 9 && rule.Damage > 0,
                        new JObject { ["energy"] = type.ToString(), ["fixturePreRoll"] = 6, ["finalBeforeDifficulty"] = result.FinalValue,
                            ["nativeDamage"] = rule.Damage, ["immune"] = packet.Immune }, "native fire immunity and cold vulnerability process actual incoming damage");
                }
                foreach (int magic in new[] { 0, 1 })
                {
                    var weapon = SummonLimbs.PrimaryWeapon(source);
                    if (weapon == null) throw new InvalidOperationException("Owned native incoming weapon is absent.");
                    var enhancement = magic == 0 ? null : weapon.AddEnchantment(fixture.Blueprints.OfType<BlueprintWeaponEnchantment>()
                        .Single(b => b.AssetGuid == EasternWeaponBlueprints.NativeEnhancementOneGuid), null, null);
                    try
                    {
                        var stats = Rulebook.Trigger(new RuleCalculateWeaponStats(source, weapon, null));
                        var packet = stats.DamageDescription.Select(d => d.CreateDamage()).OfType<PhysicalDamage>().Single();
                        packet.PreRolledValue = 15;
                        var bundle = new DamageBundle(packet) { Weapon = weapon };
                        var rule = Rulebook.Trigger(new RuleDealDamage(source, owner, bundle));
                        var result = rule.ResultDamage.Single(v => ReferenceEquals(v.Source, packet));
                        CheckSprint17Salamander("defense-dr-magic-" + magic, Sprint17ObservationPolicy.MagicReduction(
                            magic, ReferenceEquals(rule.DamageBundle.Weapon, weapon), packet.Enchantment,
                            result.RolledValue, result.Reduction, result.FinalValue),
                            new JObject { ["fixturePreRoll"] = 15, ["enhancement"] = packet.Enchantment,
                                ["weapon"] = weapon.Blueprint.AssetGuid, ["weaponAttributed"] = ReferenceEquals(rule.DamageBundle.Weapon, weapon),
                                ["rolled"] = result.RolledValue, ["nativeReduction"] = result.Reduction, ["finalBeforeDifficulty"] = result.FinalValue },
                            "actual native mundane/+1 owned weapon attribution controls DR10/magic; no raw enhancement-only packet proxy");
                    }
                    finally { if (enhancement != null) weapon.RemoveEnchantment(enhancement); }
                }
            }
            finally { owner.Descriptor.Damage = injury; }
        }

        private void ProbeSprint17SalamanderConstrict(UnitEntityData owner, UnitEntityData target,
            SummonGrabComponent grab, Sprint16RuleObserver observer, string id, int rolls)
        {
            observer.Clear(); grab.DealConstrict(owner, target, null);
            var rule = observer.Damage.SingleOrDefault();
            var physical = rule == null ? null : rule.DamageBundle.OfType<PhysicalDamage>().SingleOrDefault();
            CheckSprint17Salamander("constrict-" + id, rule != null && physical != null &&
                physical.Dice.Rolls == rolls && physical.Dice.Dice == DiceType.D6 &&
                physical.Bonus == SalamanderRulesPolicy.ConstrictStrengthBonus(owner.Stats.Strength.Bonus) &&
                physical.Form == PhysicalDamageForm.Bludgeoning && rule.DamageBundle.Count() == 2 &&
                rule.DamageBundle.Count(Sprint17SalamanderFirePacket) == 1 && rule.AttackRoll == null &&
                observer.Attacks.Count == 0 && observer.WeaponAttacks == 0 && observer.Checks.Count == 0,
                new JObject { ["strength"] = owner.Stats.Strength.ModifiedValue, ["modifier"] = owner.Stats.Strength.Bonus,
                    ["size"] = owner.Descriptor.State.Size.ToString(), ["damage"] = rule == null ? null : Sprint16DamageEvent(rule) },
                "live1.5positive Strength, negative once, native size dice, one separate unmultiplied fire packet; no attack/rider replay");
        }

        private void ExerciseSprint17SalamanderTerminal(ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, SummonGrabComponent grab, ItemEntityWeapon tail)
        {
            foreach (bool killOwner in new[] { false, true })
            {
                var target = CastExpandedSummoningQuietUnit(fixture, "wolf", fixture.Hostile);
                var observer = new Sprint16RuleObserver { Owner = owner, Target = target };
                target.Stats.HitPoints.BaseValue = 100000;
                EventBus.Subscribe(observer);
                try
                {
                    UnityEngine.Random.InitState(FindNativeD20Seed(20));
                    bool linked = grab.TryGrab(target, tail, true);
                    var dying = killOwner ? owner : target;
                    Kingmaker.Designers.GameHelper.KillUnit(dying, killOwner ? target : owner);
                    typeof(Kingmaker.Controllers.Units.UnitLifeController).GetMethod("TickOnUnit",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .Invoke(new Kingmaker.Controllers.Units.UnitLifeController(), new object[] { dying });
                    var hold = owner.Descriptor.Buffs.GetBuff(grab.HoldBuff);
                    if (hold != null) hold.TickMechanics();
                    var controller = new Kingmaker.Controllers.Units.UnitGrappleController();
                    var tick = controller.GetType().GetMethod("TickOnUnit", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    tick.Invoke(controller, new object[] { target }); tick.Invoke(controller, new object[] { owner });
                    bool free = SummonHoldComponent.HeldTarget(owner) == null &&
                        SummonGrappleLinks.EstablishingWeapon(owner, target) == null && owner.Get<UnitPartGrappleInitiator>() == null &&
                        target.Get<UnitPartGrappleTarget>() == null && !target.Descriptor.HasFact(grab.GrappledBuff) && !owner.Descriptor.HasFact(grab.HoldBuff);
                    CheckSprint17Salamander(killOwner ? "owner-death" : "prey-death", linked && dying.Descriptor.State.IsDead && free &&
                        observer.Checks.Count == 1 && observer.Damage.Count == 1,
                        new JObject { ["linked"] = linked, ["dead"] = dying.Descriptor.State.IsDead, ["free"] = free,
                            ["damageEvents"] = observer.Damage.Count, ["checks"] = observer.Checks.Count },
                        "native death releases exact tail ownership/buffs/parts without another maintain or damage");
                    if (!killOwner)
                    {
                        observer.Clear(); bool rejected = !grab.TryGrab(target, tail, true);
                        target.Destroy(); Game.Instance.EntityDestroyer.Tick(); rejected &= !grab.TryGrab(target, tail, true);
                        int round = -1; SummonHoldComponent.MaintainLink(owner, target, grab, null, null, null, ref round);
                        CheckSprint17Salamander("dead-destroyed-prey", rejected && observer.Checks.Count == 0 && observer.Damage.Count == 0,
                            new JObject { ["rejected"] = rejected, ["checks"] = observer.Checks.Count }, "dead or destroyed prey cannot restart tail grab or maintain");
                    }
                }
                finally
                {
                    EventBus.Unsubscribe(observer); SummonHoldComponent.ReleaseLink(owner, target, grab, true);
                    if (!target.Destroyed) { target.Destroy(); Game.Instance.EntityDestroyer.Tick(); }
                }
            }
        }
    }
}
