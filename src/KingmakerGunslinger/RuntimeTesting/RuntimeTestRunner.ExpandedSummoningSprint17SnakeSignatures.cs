using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Rule delivery, not command/AI qualification. Only newly summoned
        // owned actors are adjusted. Every native result is read back; the
        // seeded rule cases disclose their setup and never fabricate a PASS.
        private IEnumerable<int> ReviewSprint17SnakeSignatures(ExpandedSummoningCorrectionFixture fixture)
        {
            CreateExpandedSummoningCorrectionHostile(fixture);
            foreach (string key in new[] { "viper", "constrictor-snake" })
            {
                UnitEntityData owner = CastExpandedSummoningVariant(fixture.Blueprints, fixture.Caster,
                    ExpandedSummoningOwnTierVariant(key, SummonMultiplicity.One), null, fixture.Evidence).Single();
                fixture.Created.Add(owner);
                SetExpandedSummoningBrainActive(owner, false);
                if (!Game.Instance.State.AwakeUnits.Contains(owner)) Game.Instance.State.AwakeUnits.Add(owner);
                try
                {
                    // Native appearance completes; no removal of the locks or
                    // visibility forcing inherited from old survey helpers.
                    for (int frame = 0; frame < 60; frame++) yield return 0;
                    if (key == "viper") ExerciseSprint17ViperPoison(fixture, owner);
                    else ExerciseSprint17Constrict(fixture, owner);
                }
                finally
                {
                    if (!owner.Destroyed)
                    {
                        InterruptExpandedSummoningFixtureCommands(owner);
                        owner.Destroy();
                        Game.Instance.EntityDestroyer.Tick();
                    }
                }
                yield return 0; yield return 0;
            }
        }

        private void CheckSprint17Signature(string name, bool passed, JObject row, string expected)
        {
            row["signature"] = name; row["passed"] = passed;
            _serpentineBodyRows.Add(row);
            _serpentineBodyAssertions.Add(Assertion("sprint17-snake-signature-" + name,
                expected, row.ToString(Formatting.None), passed,
                "Closed owned-unit native rules slice; seeded rule setup is not manual/AI command proof."));
        }

        private static Buff DeliverSprint17Venom(UnitEntityData owner, UnitEntityData target,
            BlueprintBuff venom, out RuleAttackWithWeapon attack)
        {
            ClearSprint14Venom(target, venom);
            UnityEngine.Random.InitState(FindNativeD20Seed(20));
            attack = Rulebook.Trigger(new RuleAttackWithWeapon(owner, target,
                SummonLimbs.PrimaryWeapon(owner), 0));
            return target.Descriptor.Buffs.GetBuff(venom);
        }

        private static JObject DescribeSprint17Venom(Buff applied)
        {
            var logic = applied == null ? null : applied.Components.OfType<BuffPoisonStatDamage>().SingleOrDefault();
            return new JObject { ["present"] = applied != null,
                ["dc"] = applied == null || applied.Context == null ? -1 : applied.Context.Params.DC,
                ["ticks"] = logic == null ? -1 : (int)ReadExactMember(logic, "m_TicksPassed"),
                ["saves"] = logic == null ? -1 : (int)ReadExactMember(logic, "m_SavesSucceeded"),
                ["round"] = applied == null ? -1 : applied.RoundNumber };
        }

        private static void DueSprint17OwnedBuff(UnitEntityData target, Buff buff)
        {
            if (buff == null) throw new InvalidOperationException("No owned buff at the native due boundary.");
            PropertyInfo next = typeof(Buff).GetProperty("NextTickTime",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (next == null || !next.CanWrite) throw new MissingMemberException("Buff.NextTickTime");
            // Same native scheduler seam qualified on the crocodilian slice:
            // advance this fixture buff only, never world time or a shared fact.
            next.SetValue(buff, Game.Instance.TimeController.GameTime, null);
            target.Descriptor.Buffs.UpdateNextEvent();
            UnityEngine.Random.InitState(FindNativeD20Seed(10));
            target.Descriptor.Buffs.Tick();
        }

        private void ExerciseSprint17ViperPoison(ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner)
        {
            var target = fixture.Hostile;
            var stats = target.Descriptor.Stats;
            int fort = stats.SaveFortitude.BaseValue, con = stats.Constitution.BaseValue;
            int conDamage = stats.Constitution.Damage, hpDamage = target.Descriptor.Damage;
            var venom = fixture.Blueprints.OfType<BlueprintBuff>().Single(value =>
                value.name == "KMG_Summoning_Natural_Viper_Venom");
            var nativePoison = venom.ComponentsArray.OfType<BuffPoisonStatDamage>().Single();
            UnityEngine.Random.State random = UnityEngine.Random.state;
            try
            {
                stats.SaveFortitude.BaseValue = -100;
                stats.Constitution.BaseValue = 60; // Own inert target survives all six exposures.
                stats.Constitution.Damage = 0;
                CheckSprint17Signature("viper-contract", venom.Stacking == StackingType.Poison &&
                    nativePoison.Stat == Kingmaker.EntitySystem.Stats.StatType.Constitution &&
                    nativePoison.Value.Rolls == 1 && nativePoison.Value.Dice == DiceType.D2 &&
                    nativePoison.Ticks == 6 && nativePoison.SuccesfullSaves == 1 &&
                    nativePoison.SaveType == SavingThrowType.Fortitude,
                    new JObject { ["buff"] = venom.AssetGuid, ["stat"] = nativePoison.Stat.ToString(),
                        ["dice"] = nativePoison.Value.ToString(), ["ticks"] = nativePoison.Ticks,
                        ["cureSaves"] = nativePoison.SuccesfullSaves, ["targetFortBase"] = -100,
                        ["targetConBase"] = 60 }, "owned native 1d2 Constitution injury poison, six exposures, one Fortitude cure");
                foreach (string kind in new[] { "wounds", "misses", "zero-damage" })
                {
                    string detail = DescribeSprint14PoisonCase(fixture.Blueprints, owner, target,
                        SummonLimbs.PrimaryWeapon(owner).Blueprint, venom, kind != "misses",
                        kind != "zero-damage", "viper-" + kind, kind == "wounds");
                    CheckSprint17Signature("viper-" + kind, detail.EndsWith("=ok", StringComparison.Ordinal),
                        new JObject { ["nativeResult"] = detail }, "actual bite situation and matching wound-gated poison delivery");
                }
                // A real non-bite weapon entity from our disposable caster,
                // never equipped on the snake or borrowed from a party member.
                var foreign = SummonLimbs.PrimaryWeapon(fixture.Caster);
                if (foreign == null || foreign.Blueprint.Category == WeaponCategory.Bite)
                    throw new InvalidOperationException("Disposable non-bite negative control unavailable.");
                ClearSprint14Venom(target, venom);
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                var nonBite = Rulebook.Trigger(new RuleAttackWithWeapon(owner, target, foreign, 0));
                CheckSprint17Signature("viper-non-bite", nonBite.AttackRoll.IsHit &&
                    nonBite.MeleeDamage != null && nonBite.MeleeDamage.Damage > 0 && !target.Descriptor.HasFact(venom),
                    new JObject { ["category"] = foreign.Blueprint.Category.ToString(),
                        ["hit"] = nonBite.AttackRoll.IsHit, ["damage"] = nonBite.MeleeDamage == null ? 0 : nonBite.MeleeDamage.Damage,
                        ["venom"] = target.Descriptor.HasFact(venom) }, "a wounding non-bite does not deliver Viper venom");

                foreach (int delta in new[] { 0, 4, -6 })
                {
                    var modifier = owner.Descriptor.Stats.Constitution.AddModifier(delta, null,
                        "KMG_Sprint17_Disposable_Constitution", ModifierDescriptor.UntypedStackable);
                    try
                    {
                        RuleAttackWithWeapon attack;
                        Buff applied = DeliverSprint17Venom(owner, target, venom, out attack);
                        int expected = delta == 0 ? 13 : delta == 4 ? 15 : 10;
                        var row = DescribeSprint17Venom(applied);
                        row["sourceConstitution"] = owner.Descriptor.Stats.Constitution.ModifiedValue;
                        row["sourceModifier"] = owner.Descriptor.Stats.Constitution.Bonus;
                        row["applications"] = target.Descriptor.Buffs.Enumerable.Count(value => ReferenceEquals(value.Blueprint, venom));
                        CheckSprint17Signature("viper-live-dc-" + delta,
                            attack.AttackRoll.IsHit && attack.MeleeDamage != null && attack.MeleeDamage.Damage > 0 &&
                            applied != null && (int)row["dc"] == expected && (int)row["applications"] == 1 &&
                            ReferenceEquals(applied.Context.MaybeCaster, owner), row,
                            "one real bite application with live Constitution DC " + expected);
                    }
                    finally { ClearSprint14Venom(target, venom); owner.Descriptor.Stats.Constitution.RemoveModifier(modifier); }
                }

                RuleAttackWithWeapon initialAttack;
                int before = stats.Constitution.Damage;
                Buff poison = DeliverSprint17Venom(owner, target, venom, out initialAttack);
                var initial = DescribeSprint17Venom(poison);
                int initialDamage = stats.Constitution.Damage - before;
                int after = stats.Constitution.Damage;
                target.Descriptor.Buffs.Tick();
                bool cadence = poison != null && initialDamage >= 1 && initialDamage <= 2 &&
                    (int)initial["ticks"] == 1 && stats.Constitution.Damage == after;
                var exposures = new JArray { new JObject { ["exposure"] = 1, ["damage"] = initialDamage, ["native"] = initial } };
                for (int exposure = 2; exposure <= 6 && poison != null; exposure++)
                {
                    before = stats.Constitution.Damage;
                    DueSprint17OwnedBuff(target, poison);
                    int delta = stats.Constitution.Damage - before;
                    var state = DescribeSprint17Venom(poison);
                    after = stats.Constitution.Damage;
                    target.Descriptor.Buffs.Tick();
                    cadence &= delta >= 1 && delta <= 2 && (int)state["ticks"] == exposure &&
                        target.Descriptor.HasFact(venom) && stats.Constitution.Damage == after;
                    exposures.Add(new JObject { ["exposure"] = exposure, ["damage"] = delta,
                        ["native"] = state, ["presentAfter"] = target.Descriptor.HasFact(venom),
                        ["duplicateTickDamage"] = stats.Constitution.Damage - after });
                }
                // Exact native OnNewRound tests Ticks <= m_TicksPassed at
                // entry; exposure six increments to six and the NEXT due
                // boundary removes the buff without a seventh save/damage.
                // Observe that boundary instead of changing the native fact.
                before = stats.Constitution.Damage;
                DueSprint17OwnedBuff(target, poison);
                var exhausted = DescribeSprint17Venom(poison);
                int expiryDamage = stats.Constitution.Damage - before;
                target.Descriptor.Buffs.Tick();
                CheckSprint17Signature("viper-six-exposures", cadence && exposures.Count == 6 &&
                    !target.Descriptor.HasFact(venom) && (int)exhausted["ticks"] == 6 &&
                    expiryDamage == 0 && stats.Constitution.Damage == before,
                    new JObject { ["exposures"] = exposures, ["exhaustedBoundary"] = exhausted,
                        ["presentAfterExhaustion"] = target.Descriptor.HasFact(venom),
                        ["seventhExposureDamage"] = expiryDamage,
                        ["duplicateExpiryDamage"] = stats.Constitution.Damage - before },
                    "exactly six 1d2 Con exposures; next native due boundary removes without seventh/duplicate damage");

                poison = DeliverSprint17Venom(owner, target, venom, out initialAttack);
                stats.SaveFortitude.BaseValue = 100;
                before = stats.Constitution.Damage;
                DueSprint17OwnedBuff(target, poison);
                CheckSprint17Signature("viper-native-cure", !target.Descriptor.HasFact(venom) &&
                    stats.Constitution.Damage == before, new JObject { ["present"] = target.Descriptor.HasFact(venom),
                        ["extraConDamage"] = stats.Constitution.Damage - before, ["targetFortBase"] = 100 },
                    "one successful native Fortitude save removes venom without another damage exposure");

                stats.SaveFortitude.BaseValue = -100;
                poison = DeliverSprint17Venom(owner, target, venom, out initialAttack);
                int savedDc = poison == null ? -1 : poison.Context.Params.DC;
                owner.Destroy(); Game.Instance.EntityDestroyer.Tick();
                before = stats.Constitution.Damage;
                DueSprint17OwnedBuff(target, poison);
                Buff surviving = target.Descriptor.Buffs.GetBuff(venom);
                int laterDamage = stats.Constitution.Damage - before;
                CheckSprint17Signature("viper-source-destruction", owner.Destroyed && surviving != null &&
                    savedDc == 13 && surviving.Context.Params.DC == savedDc &&
                    laterDamage >= 1 && laterDamage <= 2 &&
                    !ReferenceEquals(surviving.Context.MaybeCaster, fixture.Caster) &&
                    !ReferenceEquals(surviving.Context.MaybeCaster, target),
                    new JObject { ["sourceDestroyed"] = owner.Destroyed, ["dcBefore"] = savedDc,
                        ["after"] = DescribeSprint17Venom(surviving), ["laterConDamage"] = laterDamage },
                    "native victim-owned poison outlives exact source without DC drift or retargeting");
            }
            finally
            {
                ClearSprint14Venom(target, venom);
                stats.SaveFortitude.BaseValue = fort; stats.Constitution.BaseValue = con;
                stats.Constitution.Damage = conDamage; target.Descriptor.Damage = hpDamage;
                UnityEngine.Random.state = random;
            }
        }

        private void ExerciseSprint17Constrict(ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner)
        {
            var target = fixture.Hostile;
            var grab = SummonGrabComponent.Find(owner);
            if (grab == null || !ReferenceEquals(grab.ConstrictProfileOwner, owner.Blueprint))
                throw new InvalidOperationException("Exact Constrictor grab owner is absent.");
            var observer = new Sprint16RuleObserver { Owner = owner, Target = target };
            int bab = owner.Descriptor.Stats.BaseAttackBonus.BaseValue;
            UnityEngine.Random.State random = UnityEngine.Random.state;
            EventBus.Subscribe(observer);
            try
            {
                var bite = SummonLimbs.PrimaryWeapon(owner);
                var foreign = SummonLimbs.PrimaryWeapon(fixture.Caster);
                if (foreign == null || foreign.Blueprint.Category == WeaponCategory.Bite)
                    throw new InvalidOperationException("Disposable non-bite rejection input unavailable.");
                bool miss = grab.TryGrab(target, bite, false);
                bool wrongLimb = grab.TryGrab(target, foreign, true);
                target.Descriptor.State.Size = Size.Large;
                bool tooLarge = grab.TryGrab(target, bite, true);
                CheckSprint17Signature("constrict-grab-rejections", !miss && !wrongLimb && !tooLarge &&
                    observer.Checks.Count == 0 && observer.Damage.Count == 0,
                    new JObject { ["miss"] = miss, ["foreignLimb"] = wrongLimb,
                        ["tooLarge"] = tooLarge, ["checks"] = observer.Checks.Count,
                        ["damageEvents"] = observer.Damage.Count }, "miss/non-bite/oversized target never starts a grapple");

                target.Descriptor.State.Size = Size.Medium;
                owner.Descriptor.Stats.BaseAttackBonus.BaseValue = 100; // Disclosed isolated rule fixture, NOT printed attack proof.
                PlaceExpandedSummoningUnit(target, owner.Position + Vector3.forward);
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                var attack = Rulebook.Trigger(new RuleAttackWithWeapon(owner, target, bite, 0));
                var held = SummonHoldComponent.HeldState(owner, target, grab);
                CheckSprint17Signature("constrict-bite-delivery", attack.AttackRoll.IsHit && held != null &&
                    ReferenceEquals(SummonHoldComponent.HeldTarget(owner), target) &&
                    observer.Checks.Count == 1 && observer.WeaponAttacks == 1 && observer.Damage.Count == 2 &&
                    observer.Damage.Count(value => value.AttackRoll == null) == 1,
                    new JObject { ["fixtureBaseAttack"] = 100, ["checks"] = observer.Checks.Count,
                        ["damage"] = new JArray(observer.Damage.Select(Sprint16DamageEvent)) },
                    "one actual bite hit, one grapple check, one initial constrict; no second weapon attack");
                if (held == null) throw new InvalidOperationException("Cannot qualify maintain without actual initial bite grab.");
                observer.Clear();
                Buff hold = owner.Descriptor.Buffs.GetBuff(grab.HoldBuff);
                hold.TickMechanics();
                CheckSprint17Signature("constrict-no-application-frame-maintain", observer.Checks.Count == 0 &&
                    observer.Damage.Count == 0, new JObject { ["checks"] = observer.Checks.Count,
                        ["damageEvents"] = observer.Damage.Count }, "no maintain or extra constrict during the initial held round");
                UnityEngine.Random.InitState(FindNativeD20Seed(10));
                held.TickMechanics();
                observer.Clear();
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                hold.TickMechanics();
                int firstDamage = observer.Damage.Count, firstChecks = observer.Checks.Count;
                hold.TickMechanics();
                CheckSprint17Signature("constrict-later-maintain-once", firstDamage == 2 && firstChecks == 1 &&
                    observer.Damage.Count == 2 && observer.Checks.Count == 1 &&
                    observer.WeaponAttacks == 0 && observer.Attacks.Count == 0 &&
                    ReferenceEquals(SummonHoldComponent.HeldTarget(owner), target),
                    new JObject { ["damage"] = new JArray(observer.Damage.Select(Sprint16DamageEvent)),
                        ["checks"] = observer.Checks.Count, ["attackRolls"] = observer.Attacks.Count,
                        ["weaponAttacks"] = observer.WeaponAttacks },
                    "later native hold tick deals bite plus one constrict; replay does not reroll or deal damage");
                Sprint16Release(fixture, owner, grab);

                foreach (int delta in new[] { 0, 4, -10 })
                {
                    var modifier = owner.Descriptor.Stats.Strength.AddModifier(delta, null,
                        "KMG_Sprint17_Disposable_ConstrictStrength", ModifierDescriptor.UntypedStackable);
                    try { ProbeSprint17Constrict(owner, target, grab, observer,
                        "constrict-strength-" + delta, delta == 0 ? 4 : delta == 4 ? 7 : -2, DiceType.D4); }
                    finally { owner.Descriptor.Stats.Strength.RemoveModifier(modifier); }
                }
                var growth = fixture.Blueprints.OfType<BlueprintBuff>().Single(value => value.name == "AnimalGrowthBuff" &&
                    value.ComponentsArray.OfType<ChangeUnitSize>().Any());
                Buff grown = owner.Descriptor.AddBuff(growth, owner, TimeSpan.FromMinutes(1));
                try { ProbeSprint17Constrict(owner, target, grab, observer, "constrict-native-growth", 10, DiceType.D6); }
                finally { if (grown != null) grown.Remove(); }
                ProbeSprint17Constrict(owner, target, grab, observer, "constrict-modifiers-restored", 4, DiceType.D4);
                ExerciseSprint17ConstrictTerminal(fixture, owner, grab);
            }
            finally
            {
                EventBus.Unsubscribe(observer);
                Sprint16Release(fixture, owner, grab);
                owner.Descriptor.Stats.BaseAttackBonus.BaseValue = bab;
                UnityEngine.Random.state = random;
            }
        }

        private void ExerciseSprint17ConstrictTerminal(ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, SummonGrabComponent grab)
        {
            foreach (bool killOwner in new[] { false, true })
            {
                UnitEntityData victim = CastExpandedSummoningQuietUnit(fixture, "wolf", fixture.Hostile);
                var observer = new Sprint16RuleObserver { Owner = owner, Target = victim };
                EventBus.Subscribe(observer);
                try
                {
                    victim.Descriptor.State.Size = Size.Medium;
                    victim.Descriptor.Stats.Constitution.BaseValue = 3;
                    victim.Descriptor.Stats.HitPoints.BaseValue = killOwner ? 100000 : 20;
                    victim.Descriptor.Damage = killOwner ? 0 : victim.Descriptor.Stats.HitPoints.ModifiedValue - 1;
                    PlaceExpandedSummoningUnit(victim, owner.Position + Vector3.forward);
                    UnityEngine.Random.InitState(FindNativeD20Seed(20));
                    bool established = grab.TryGrab(victim, SummonLimbs.PrimaryWeapon(owner), true);
                    int initialDamage = observer.Damage.Count;
                    if (killOwner)
                    {
                        // Only this fixture owner receives a lethal injury
                        // input; the actual life controller settles death.
                        owner.Descriptor.Damage = owner.Descriptor.Stats.HitPoints.ModifiedValue +
                            owner.Descriptor.Stats.Constitution.ModifiedValue + 10;
                    }
                    UnitEntityData dying = killOwner ? owner : victim;
                    typeof(Kingmaker.Controllers.Units.UnitLifeController).GetMethod("TickOnUnit",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .Invoke(new Kingmaker.Controllers.Units.UnitLifeController(), new object[] { dying });
                    Buff hold = owner.Descriptor.Buffs.GetBuff(grab.HoldBuff);
                    if (hold != null) hold.TickMechanics();
                    bool initiatorBeforeNativeTick = owner.Get<UnitPartGrappleInitiator>() != null;
                    // ReleaseLink intentionally leaves the initiator part to
                    // this native controller (avoiding buff-removal re-entry).
                    // Settle the exact owned actors before trying a second
                    // terminal case in this otherwise synchronous rule slice.
                    var grappleController = new Kingmaker.Controllers.Units.UnitGrappleController();
                    MethodInfo grappleTick = typeof(Kingmaker.Controllers.Units.UnitGrappleController)
                        .GetMethod("TickOnUnit", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    grappleTick.Invoke(grappleController, new object[] { victim });
                    grappleTick.Invoke(grappleController, new object[] { owner });
                    bool free = SummonHoldComponent.HeldTarget(owner) == null &&
                        SummonGrappleLinks.EstablishingWeapon(owner, victim) == null &&
                        victim.Get<UnitPartGrappleTarget>() == null && !victim.Descriptor.HasFact(grab.GrappledBuff) &&
                        owner.Get<UnitPartGrappleInitiator>() == null && !owner.Descriptor.HasFact(grab.HoldBuff);
                    CheckSprint17Signature(killOwner ? "constrict-owner-death" : "constrict-lethal-prey",
                        established && dying.Descriptor.State.IsDead && free && initialDamage == 1 &&
                        observer.Damage.Count == 1 && observer.Checks.Count == 1,
                        new JObject { ["established"] = established, ["dead"] = dying.Descriptor.State.IsDead,
                            ["ownerDeathFixtureInjury"] = killOwner, ["free"] = free,
                            ["initiatorBeforeNativeTick"] = initiatorBeforeNativeTick,
                            ["initiatorAfterNativeTick"] = owner.Get<UnitPartGrappleInitiator>() != null,
                            ["damage"] = new JArray(observer.Damage.Select(Sprint16DamageEvent)),
                            ["checks"] = observer.Checks.Count },
                        "native death, production hold and native grapple-controller cleanup; no maintain on dead owner or prey");
                    if (!killOwner)
                    {
                        observer.Clear();
                        bool deadGrab = grab.TryGrab(victim, SummonLimbs.PrimaryWeapon(owner), true);
                        int round = -1;
                        string deadMaintain = SummonHoldComponent.MaintainLink(owner, victim, grab, null,
                            null, null, ref round);
                        CheckSprint17Signature("constrict-dead-prey-rejected", !deadGrab &&
                            observer.Checks.Count == 0 && observer.Damage.Count == 0,
                            new JObject { ["grab"] = deadGrab, ["maintain"] = deadMaintain },
                            "dead prey cannot start another grapple or maintain/damage event");
                        victim.Destroy(); Game.Instance.EntityDestroyer.Tick();
                        bool destroyedGrab = grab.TryGrab(victim, SummonLimbs.PrimaryWeapon(owner), true);
                        string destroyedMaintain = SummonHoldComponent.MaintainLink(owner, victim, grab, null,
                            null, null, ref round);
                        CheckSprint17Signature("constrict-destroyed-prey-rejected", victim.Destroyed && !destroyedGrab &&
                            observer.Checks.Count == 0 && observer.Damage.Count == 0,
                            new JObject { ["destroyed"] = victim.Destroyed, ["grab"] = destroyedGrab,
                                ["maintain"] = destroyedMaintain }, "destroyed prey has no rule delivery or re-established link");
                    }
                }
                finally
                {
                    EventBus.Unsubscribe(observer);
                    SummonHoldComponent.ReleaseLink(owner, victim, grab, true);
                    if (!victim.Destroyed) { victim.Destroy(); Game.Instance.EntityDestroyer.Tick(); }
                }
            }
        }

        private void ProbeSprint17Constrict(UnitEntityData owner, UnitEntityData target, SummonGrabComponent grab,
            Sprint16RuleObserver observer, string name, int expectedBonus, DiceType expectedDie)
        {
            observer.Clear();
            grab.DealConstrict(owner, target, null);
            RuleDealDamage rule = observer.Damage.SingleOrDefault();
            PhysicalDamage damage = rule == null ? null : rule.DamageBundle.OfType<PhysicalDamage>().SingleOrDefault();
            CheckSprint17Signature(name, damage != null && rule.DamageBundle.Count() == 1 &&
                damage.Dice.Rolls == 1 && damage.Dice.Dice == expectedDie && damage.Bonus == expectedBonus &&
                damage.Form == PhysicalDamageForm.Bludgeoning && ReferenceEquals(rule.Initiator, owner) &&
                rule.AttackRoll == null && observer.Attacks.Count == 0 && observer.WeaponAttacks == 0 &&
                observer.Checks.Count == 0, new JObject { ["strength"] = owner.Descriptor.Stats.Strength.ModifiedValue,
                    ["modifier"] = owner.Descriptor.Stats.Strength.Bonus, ["size"] = owner.Descriptor.State.Size.ToString(),
                    ["damage"] = rule == null ? null : Sprint16DamageEvent(rule) },
                "one native live-strength/size constrict bundle; no attack/on-hit/maneuver replay");
        }
    }
}
