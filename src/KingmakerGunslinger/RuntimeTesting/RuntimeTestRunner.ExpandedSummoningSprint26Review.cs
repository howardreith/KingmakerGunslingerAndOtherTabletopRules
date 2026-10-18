using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;
using KingmakerGunslinger.Assets;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using TurnBased.Controllers;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Reuses Sprint25's request-local iterator/clock/teardown, never its
        // creature-specific expectations or forced animation-event helpers.
        private IEnumerable<int> ReviewSprint26(ExpandedSummoningCorrectionFixture fixture)
        {
            bool mode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool pause = Game.Instance.IsPaused;
            string phase = RuntimeTestRequestParser.Sprint26ReviewPhase(_request);
            try
            {
                Sprint25Check("phase", phase != null, new JObject { ["phase"] = phase }, "One closed Sprint26 phase is selected.");
                Game.Instance.IsPaused = true;
                CreateExpandedSummoningCorrectionHostile(fixture);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = false;
                for (int i = 0; i < 16; i++) yield return 0;
                if (phase == "routes") { Sprint26Routes(fixture); yield break; }
                bool turnBased = phase == "turn-based-commands" || phase.EndsWith("turn-based", StringComparison.Ordinal);
                if (phase == "rtwp-foundation" || phase == "turn-based-commands")
                {
                    foreach (string key in new[] { "gorgon", "bulette" })
                    {
                        var owner = CastExpandedSummoningOwnTier(fixture, key);
                        SetExpandedSummoningBrainActive(owner, false);
                        try
                        {
                            foreach (int frame in Sprint26Appearance(owner, key)) yield return frame;
                            if (key == "gorgon") foreach (int frame in Sprint22SetPowerAttack(owner, false)) yield return frame;
                            Sprint26Profile(fixture, owner, key);
                            if (!turnBased) Sprint26SignedStrength(fixture, owner, key);
                            foreach (int frame in Sprint26Ordinary(fixture, owner, key, turnBased)) yield return frame;
                        }
                        finally
                        {
                            SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                            Game.Instance.TurnBasedCombatController.Activate();
                            owner.Commands.InterruptAll(true);
                            if (owner.IsInCombat) owner.LeaveCombat();
                            DisposeExpandedSummoningUnits(fixture.Created, new[] { owner });
                        }
                    }
                    yield break;
                }
                if (phase.StartsWith("bulette-leap-", StringComparison.Ordinal))
                {
                    foreach (bool success in new[] { true, false })
                        foreach (int frame in Sprint26Leap(fixture, turnBased, success)) yield return frame;
                    yield break;
                }
                if (phase.StartsWith("gorgon-breath-", StringComparison.Ordinal))
                {
                    foreach (int frame in Sprint26Breath(fixture, turnBased)) yield return frame;
                    yield break;
                }
                // A phase cannot silently fall back to a smaller native subset.
                throw new InvalidOperationException("Sprint26 phase implementation is not yet complete: " + phase);
            }
            finally
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = mode;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = pause;
            }
        }

        private IEnumerable<int> Sprint26Appearance(UnitEntityData owner, string key, bool verifyBody = true)
        {
            var nearest = AstarPath.active.GetNearest(owner.Position);
            if (nearest.node == null || !nearest.node.Walkable) throw new InvalidOperationException("No walkable appearance anchor.");
            PlaceExpandedSummoningUnit(owner, nearest.clampedPosition);
            bool before = _sprint25WaitingForUnityAppearance;
            int frames = 0;
            DateTime deadline = DateTime.UtcNow.AddSeconds(12);
            bool ready = false;
            try
            {
                _sprint25WaitingForUnityAppearance = false;
                while (frames++ < 2500 && DateTime.UtcNow < deadline)
                {
                    ready = owner.View != null && owner.Descriptor.State.CanAct && owner.Descriptor.State.CanMove &&
                        owner.Descriptor.Buffs.GetBuff(Kingmaker.Blueprints.Root.BlueprintRoot.Instance.SystemMechanics.SummonedUnitAppearBuff) == null &&
                        EntityFadedIn(owner) && DissolveAmount(owner) >= 0 && DissolveAmount(owner) <= .02f;
                    if (ready) break;
                    _sprint25WaitingForUnityAppearance = owner.View != null && EntityFadedIn(owner) &&
                        owner.Descriptor.Buffs.GetBuff(Kingmaker.Blueprints.Root.BlueprintRoot.Instance.SystemMechanics.SummonedUnitAppearBuff) == null;
                    Game.Instance.IsPaused = false;
                    yield return 0;
                }
            }
            finally { _sprint25WaitingForUnityAppearance = before; }
            Sprint25Check("appearance-" + key, ready, new JObject { ["unit"] = owner.UniqueId, ["frames"] = frames,
                ["canAct"] = owner.Descriptor.State.CanAct, ["canMove"] = owner.Descriptor.State.CanMove,
                ["dissolve"] = DissolveAmount(owner) }, "Native appearance settles without renderer or fader writes.");
            if (!ready) throw new InvalidOperationException("Appearance did not settle: " + key);
            if (verifyBody) Sprint26Body(owner, key, "idle");
        }

        private void Sprint26Body(UnitEntityData owner, string key, string state)
        {
            var skins = owner.View.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            var bodies = skins.Where(v => v.enabled && v.sharedMesh != null &&
                (key == "gorgon" ? v.sharedMesh.name == "KMG_gorgon_Original" : v.name == "Bulette")).ToArray();
            bool exact = bodies.Length == 1 && (bool)Sprint22SelectionSurface(owner)["selectionSurface"];
            string asset = "native exact Bulette prefab";
            if (key == "gorgon")
            {
                Mesh cache; string[] bones; Texture2D albedo;
                exact &= PteranodonAssetRuntime.TryGetUngulateVisual(key, out cache, out bones, out albedo, out asset);
                exact &= bodies.Length == 1 && bodies[0].sharedMesh != cache && bodies[0].bones.Length == bones.Length &&
                    ReferenceEquals(bodies[0].sharedMaterial.mainTexture, albedo);
            }
            JObject pose = bodies.Length == 1 ? Sprint26Pose(bodies[0]) : null;
            exact &= pose != null && (bool)pose["finite"];
            Sprint25Check("body-" + key + "-" + state, exact, new JObject { ["unit"] = owner.UniqueId,
                ["bodyCount"] = bodies.Length, ["assetStatus"] = asset,
                ["pose"] = pose,
                ["partyCamera"] = WriteExpandedSummoningPartyCameraCapture(owner, _request.EvidenceDirectory,
                    "sprint26-" + key + "-" + state + ".png") }, "The exact original Gorgon or native Bulette body is visible, finite and selectable.");
        }

        private static JObject Sprint26Pose(SkinnedMeshRenderer body)
        {
            var bones = body.bones;
            var bind = body.sharedMesh.bindposes;
            bool finite = bones != null && bones.Length > 0 && bones.Length == bind.Length &&
                bones.All(v => v != null && Enumerable.Range(0, 16).All(i =>
                    !float.IsNaN(v.localToWorldMatrix[i]) && !float.IsInfinity(v.localToWorldMatrix[i]))) &&
                !float.IsNaN(body.bounds.center.sqrMagnitude + body.bounds.extents.sqrMagnitude) &&
                !float.IsInfinity(body.bounds.center.sqrMagnitude + body.bounds.extents.sqrMagnitude);
            return new JObject { ["frame"] = Time.frameCount, ["finite"] = finite,
                ["vertexCount"] = body.sharedMesh.vertexCount, ["boneCount"] = bones?.Length,
                ["worldBoundsCenter"] = SurveyVector(body.bounds.center), ["worldBoundsExtents"] = SurveyVector(body.bounds.extents),
                ["boneNames"] = new JArray((bones ?? new Transform[0]).Select(v => v == null ? null : v.name)) };
        }

        private void Sprint26Profile(ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner, string key)
        {
            bool gorgon = key == "gorgon";
            var limbs = Sprint19Limbs(owner);
            var rows = new JArray();
            bool exact = limbs.Length == 3;
            for (int i = 0; i < limbs.Length; i++)
            {
                var stats = Rulebook.Trigger(new RuleCalculateWeaponStats(owner, limbs[i], null));
                BaseDamage damage = stats.DamageDescription[0].CreateDamage();
                int attack = Rulebook.Trigger(new RuleCalculateAttackBonusWithoutTarget(owner, limbs[i], 0)).Result;
                bool secondary = gorgon && i != 0;
                int bonus = gorgon ? (i == 0 ? 7 : 3) : (i == 0 ? 9 : 6);
                int expectedAttack = gorgon ? (i == 0 ? 14 : 9) : (i == 0 ? 13 : 12);
                bool row = limbs[i].IsSecondary == secondary && attack == expectedAttack && damage.Bonus == bonus &&
                    damage.Dice.Rolls == (gorgon && i != 0 ? 1 : 2) &&
                    damage.Dice.Dice == (i == 0 ? DiceType.D8 : DiceType.D6) &&
                    limbs[i].Blueprint.CriticalRollEdge == (!gorgon && i == 0 ? 19 : 20);
                exact &= row;
                rows.Add(new JObject { ["weapon"] = limbs[i].Blueprint.AssetGuid, ["secondary"] = limbs[i].IsSecondary,
                    ["attack"] = attack, ["damage"] = Sprint16DamageLine(damage), ["threat"] = limbs[i].Blueprint.CriticalRollEdge,
                    ["reach"] = limbs[i].AttackRange.Value, ["exact"] = row });
            }
            int ac = Rulebook.Trigger(new RuleCalculateAC(fixture.Hostile, owner, AttackType.Melee)).TargetAC;
            int touch = Rulebook.Trigger(new RuleCalculateAC(fixture.Hostile, owner, AttackType.Touch)).TargetAC;
            var ordinary = ProbeCombatManeuverDefenceParts(fixture.Hostile, owner, CombatManeuver.Grapple);
            var trip = ProbeCombatManeuverDefenceParts(fixture.Hostile, owner, CombatManeuver.Trip);
            exact &= owner.Descriptor.Progression.CharacterLevel == 8 && owner.Stats.BaseAttackBonus.ModifiedValue == 8 &&
                owner.Stats.HitPoints.ModifiedValue == (gorgon ? 100 : 84) && ac == (gorgon ? 20 : 22) && touch == (gorgon ? 9 : 10) &&
                owner.Stats.SaveFortitude.ModifiedValue == (gorgon ? 13 : 11) && owner.Stats.SaveReflex.ModifiedValue == (gorgon ? 6 : 8) &&
                owner.Stats.SaveWill.ModifiedValue == (gorgon ? 7 : 5) && owner.Stats.SkillPerception.ModifiedValue == (gorgon ? 17 : 11) &&
                (gorgon || owner.Stats.SkillMobility.ModifiedValue == 9) && trip.Result - ordinary.Result == 4;
            Sprint25Check("profile-" + key, exact, new JObject { ["unit"] = owner.UniqueId, ["attacks"] = rows,
                ["hp"] = owner.Stats.HitPoints.ModifiedValue, ["ac"] = ac, ["touch"] = touch,
                ["fort"] = owner.Stats.SaveFortitude.ModifiedValue, ["ref"] = owner.Stats.SaveReflex.ModifiedValue,
                ["will"] = owner.Stats.SaveWill.ModifiedValue, ["perception"] = DescribeSprint16Skill(owner.Stats.SkillPerception),
                ["mobility"] = DescribeSprint16Skill(owner.Stats.SkillMobility), ["cmd"] = ordinary.Describe("CMD"),
                ["trip"] = trip.Describe("trip"), ["facts"] = new JArray(owner.Descriptor.Progression.Features.Enumerable.Select(v => v.Blueprint.name)) },
                "The composed adult profile, printed attack roles, dice, threat range, skills and defenses match the frozen contract.");
        }

        private void Sprint26SignedStrength(ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner, string key)
        {
            int strength = owner.Stats.Strength.BaseValue;
            int extra = owner.Stats.AdditionalAttackBonus.BaseValue;
            var random = UnityEngine.Random.state;
            var observer = new Sprint24AttackObserver { Owner = owner, Target = fixture.Hostile };
            var rows = new JArray(); bool exact = true;
            EventBus.Subscribe(observer);
            try
            {
                owner.Stats.AdditionalAttackBonus.BaseValue = 100;
                foreach (int[] entry in new[] { new[] { 4, -3, -5, -3 }, new[] { 10, 0, 0, 0 }, new[] { 26, 8, 12, 8 } })
                {
                    owner.Stats.Strength.BaseValue = entry[0];
                    int index = 0;
                    foreach (ItemEntityWeapon weapon in Sprint19Limbs(owner))
                    {
                        int expected = key == "bulette" && index == 0 ? entry[2] :
                            key == "gorgon" && index != 0 && entry[1] > 0 ? entry[1] / 2 : entry[1];
                        observer.Clear();
                        UnityEngine.Random.InitState(FindNativeD20Seed(18));
                        Rulebook.Trigger(new RuleAttackWithWeapon(owner, fixture.Hostile, weapon, 0));
                        var dealt = observer.Damage.FirstOrDefault();
                        bool row = dealt != null && dealt.DamageBundle.First.Bonus == expected;
                        exact &= row;
                        rows.Add(new JObject { ["score"] = owner.Stats.Strength.ModifiedValue, ["modifier"] = owner.Stats.Strength.Bonus,
                            ["weapon"] = weapon.Blueprint.AssetGuid, ["expected"] = expected, ["dealt"] = dealt == null ? null : Sprint16DamageEvent(dealt), ["exact"] = row });
                        index++;
                    }
                }
            }
            finally { owner.Stats.Strength.BaseValue = strength; owner.Stats.AdditionalAttackBonus.BaseValue = extra; UnityEngine.Random.state = random; EventBus.Unsubscribe(observer); }
            Sprint25Check("signed-damage-" + key, exact, new JObject { ["rows"] = rows }, "Actual weapon damage carries signed, zero and increased live Strength with correct limb roles.");
        }

        private IEnumerable<int> Sprint26Ordinary(ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner, string key, bool turnBased)
        {
            var target = fixture.Hostile;
            owner.JoinCombat(); target.JoinCombat();
            float reach = (key == "gorgon" ? 5 : 10) * .3048f;
            float radii = owner.Corpulence + target.Corpulence;
            Vector3 far; string lane, direction;
            Vector3 origin = Sprint24AiCombatLane(fixture.Caster, owner, target, reach + radii + .2f, out far, out lane, out direction);
            Vector3 forward = (far - origin).normalized;
            PlaceExpandedSummoningUnit(owner, origin);
            var range = new UnitAttack(target); range.Init(owner);
            PlaceExpandedSummoningUnit(target, origin + forward * (reach + radii - .05f)); bool inside = range.IsUnitEnoughClose;
            PlaceExpandedSummoningUnit(target, origin + forward * (reach + radii + .05f)); bool outside = range.IsUnitEnoughClose;
            Sprint25Check("reach-" + key + "-" + turnBased, inside && !outside && Math.Abs(range.ApproachRadius - reach - radii) < .02f,
                new JObject { ["inside"] = inside, ["outside"] = outside, ["approachRadius"] = range.ApproachRadius, ["radii"] = radii, ["lane"] = lane }, "Actual UnitAttack discriminates the printed reach boundary.");
            PlaceExpandedSummoningUnit(target, origin + forward * (radii + .8f));
            owner.Memory.Add(target); target.Memory.Add(owner);
            if (turnBased)
            {
                var admission = new JObject();
                foreach (int frame in Sprint22EnrollTurnBasedOwners(new[] { owner }, target, admission)) yield return frame;
                for (int i = 0; i < 500 && !Sprint18OwnerIsActing(owner); i++) yield return 0;
                if (!(bool)admission["admitted"] || !Sprint18OwnerIsActing(owner)) throw new InvalidOperationException("Exact owner turn missing.");
            }
            var observer = new Sprint24AttackObserver { Owner = owner, Target = target };
            var costs = new JArray();
            var command = new UnitAttack(target) { ForceFullAttack = true, IsSingleAttack = false };
            EventBus.Subscribe(observer);
            try
            {
                using (var economy = new FirearmReviewCosts(owner, costs))
                {
                    owner.Commands.Run(command);
                    int frames = 0; DateTime deadline = DateTime.UtcNow.AddSeconds(30);
                    while (!command.IsFinished && frames++ < 3000 && DateTime.UtcNow < deadline) yield return 0;
                    bool exact = command.IsFinished && observer.WeaponAttacks.Count == 3 && command.AllAttacks.Count == 3 &&
                        economy.Count(command) == 1 && economy.Exact(command, turnBased) && !command.Cutscene && !command.IsIgnoreCooldown;
                    Sprint25Check("ordinary-" + key + "-" + turnBased, exact, new JObject { ["finished"] = command.IsFinished,
                        ["result"] = command.Result.ToString(), ["frames"] = frames, ["planned"] = command.AllAttacks.Count,
                        ["weapons"] = new JArray(observer.WeaponAttacks.Select(v => v.Weapon.Blueprint.AssetGuid)),
                        ["costs"] = costs, ["forcedAnimationEvents"] = 0 }, "The real full attack completes three ordinary attacks with one native action cost and no fixture animation completion.");
                    Sprint26Body(owner, key, "attack-" + turnBased);
                }
            }
            finally { EventBus.Unsubscribe(observer); owner.Commands.InterruptAll(true); ResetExpandedSummoningHostile(fixture); }
        }

        private void Sprint26Routes(ExpandedSummoningCorrectionFixture fixture)
        {
            var routes = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster).Concat(
                ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly)).Where(v => v.Creature.Key == "gorgon" || v.Creature.Key == "bulette").ToArray();
            Sprint25Check("route-census", routes.Length == 6 && routes.Count(v => v.Creature.Key == "gorgon") == 2 &&
                routes.All(v => !v.Creature.MonsterTemplated), new JObject { ["routes"] = new JArray(routes.Select(v => v.StableKey)) }, "Exactly two Monster Gorgon and four Nature's Ally Bulette roots, untemplated.");
            foreach (var route in routes.Where(v => !SummonVisibilityCatalog.IsPublished(v)))
            {
                var units = CastExpandedSummoningVariant(fixture.Blueprints, fixture.Caster, route, null, fixture.Evidence); fixture.Created.AddRange(units);
                int low = route.Multiplicity == SummonMultiplicity.OneD4PlusOne ? 2 : 1;
                int high = route.Multiplicity == SummonMultiplicity.One ? 1 : route.Multiplicity == SummonMultiplicity.OneD3 ? 3 : 5;
                Sprint25Check("hidden-root-" + route.StableKey, units.Length >= low && units.Length <= high && units.All(v => Sprint24TemplateCount(v) == 0),
                    new JObject { ["route"] = route.StableKey, ["units"] = new JArray(units.Select(Sprint22UnitReadback)), ["execution"] = _expandedSummoningLastAbilityExecution }, "The withheld exact root summons its legal untemplated quantity.");
                DisposeExpandedSummoningUnits(fixture.Created, units);
            }
            Sprint24PlayerRoutes(fixture, routes.Where(SummonVisibilityCatalog.IsPublished).ToArray(), Sprint25Check, true);
        }

        private IEnumerable<int> Sprint26Leap(ExpandedSummoningCorrectionFixture fixture, bool turnBased, bool success)
        {
            var owner = CastExpandedSummoningOwnTier(fixture, "bulette");
            SetExpandedSummoningBrainActive(owner, false);
            var target = fixture.Hostile;
            var leap = owner.Descriptor.Abilities.Enumerable.Single(v => v.Blueprint.name == "KMG_Summoning_Special_Bulette_Leap");
            var data = new AbilityData(leap);
            var observer = new Sprint24AttackObserver { Owner = owner, Target = target };
            UnityEngine.Random.State random = UnityEngine.Random.state;
            UnitUseAbility command = null; UnitAttack attack = null;
            try
            {
                foreach (int frame in Sprint26Appearance(owner, "bulette")) yield return frame;
                Vector3 start; string route;
                TargetWrapper selected = Sprint23FindChargeRoute(fixture, owner, target, data, out start, out route);
                if (selected == null) throw new InvalidOperationException("No legal Leap charge lane: " + route);
                if (turnBased)
                {
                    var admission = new JObject();
                    foreach (int frame in Sprint22EnrollTurnBasedOwners(new[] { owner }, target, admission)) yield return frame;
                    for (int i = 0; i < 500 && !Sprint18OwnerIsActing(owner); i++) yield return 0;
                    if (!(bool)admission["admitted"] || !Sprint18OwnerIsActing(owner)) throw new InvalidOperationException("Leap native turn missing.");
                }
                EventBus.Subscribe(observer);
                using (var seed = new Sprint26JumpSeed(owner, success ? 20 : 1))
                {
                    command = new UnitUseAbility(data, selected);
                    var costs = new JArray();
                    using (var economy = new FirearmReviewCosts(owner, costs))
                    {
                        owner.Commands.Run(command);
                        int frames = 0; DateTime deadline = DateTime.UtcNow.AddSeconds(35);
                        while (frames++ < 4000 && DateTime.UtcNow < deadline)
                        {
                            attack = attack ?? owner.Commands.Raw.OfType<UnitAttack>().FirstOrDefault();
                            if (command.ExecutionProcess != null && !command.ExecutionProcess.IsEnded) command.ExecutionProcess.Tick();
                            yield return 0;
                            if (attack != null && attack.IsFinished && command.IsFinished) break;
                        }
                        var attempt = BuletteLeapRuntime.Observation(attack);
                        bool exact = attempt != null && attempt.Check.IsPassed == success && seed.Count == 1 &&
                            attempt.Check.BaseRollResult == (success ? 37 : 18) && attempt.JumpBonus == 8 &&
                            owner.Stats.SkillMobility.ModifiedValue == 9 && attack != null && attack.IsFinished &&
                            observer.WeaponAttacks.Count == (success ? 4 : 1) &&
                            (!success || observer.WeaponAttacks.All(v => ReferenceEquals(v.Weapon.Blueprint, attempt.Claw))) &&
                            economy.Count(command) == 1 && economy.Exact(command, turnBased);
                        Sprint25Check("leap-" + turnBased + "-" + success, exact, new JObject { ["route"] = route,
                            ["finished"] = command.IsFinished, ["result"] = command.Result.ToString(), ["frames"] = frames,
                            ["checkCount"] = seed.Count, ["checkTotal"] = attempt?.Check.BaseRollResult,
                            ["checkSuccess"] = attempt?.Check.IsPassed, ["jumpBonus"] = attempt?.JumpBonus,
                            ["mobility"] = owner.Stats.SkillMobility.ModifiedValue, ["plannedClaws"] = attempt?.PlannedClaws,
                            ["attackFinished"] = attack?.IsFinished, ["weapons"] = new JArray(observer.WeaponAttacks.Select(v => v.Weapon.Blueprint.AssetGuid)),
                            ["costs"] = costs, ["armedAfter"] = BuletteLeapRuntime.IsArmed(owner), ["forcedAnimationEvents"] = 0 },
                            "A genuine seeded native jump check chooses four claws/no bite or one failed-charge fallback, with native movement, attacks and action cost.");
                        Sprint26Body(owner, "bulette", "leap-" + success);
                    }
                }
            }
            finally
            {
                EventBus.Unsubscribe(observer); UnityEngine.Random.state = random;
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false; Game.Instance.TurnBasedCombatController.Activate();
                owner.Commands.InterruptAll(true); if (owner.IsInCombat) owner.LeaveCombat();
                DisposeExpandedSummoningUnits(fixture.Created, new[] { owner }); ResetExpandedSummoningHostile(fixture);
            }
        }

        private IEnumerable<int> Sprint26Cast(UnitEntityData owner, BlueprintAbility blueprint,
            TargetWrapper target, Action<UnitUseAbility> result, Action<UnitUseAbility> observeFrame = null)
        {
            var ability = owner.Descriptor.Abilities.GetAbility(blueprint);
            if (ability == null) throw new InvalidOperationException("Ability not granted: " + blueprint.name);
            if (CombatController.IsInTurnBasedCombat())
            {
                int waited = 0;
                while (!Sprint18OwnerIsActing(owner) && waited++ < 700) yield return 0;
                if (!Sprint18OwnerIsActing(owner)) throw new InvalidOperationException("Caster's exact native turn is unavailable.");
            }
            var data = new AbilityData(ability);
            if (!data.IsAvailable || !data.CanTarget(target)) throw new InvalidOperationException("Native ability unavailable: " + blueprint.name);
            var command = new UnitUseAbility(data, target);
            result(command);
            owner.Commands.Run(command);
            DateTime deadline = DateTime.UtcNow.AddSeconds(25);
            int frames = 0;
            while (frames++ < 3000 && DateTime.UtcNow < deadline)
            {
                if (command.ExecutionProcess != null && !command.ExecutionProcess.IsEnded) command.ExecutionProcess.Tick();
                observeFrame?.Invoke(command);
                yield return 0;
                if (command.IsFinished && command.ExecutionProcess != null && command.ExecutionProcess.IsEnded) break;
            }
            if (!command.IsFinished || command.ExecutionProcess == null || !command.ExecutionProcess.IsEnded)
                throw new InvalidOperationException("Native ability did not finish without fixture animation events: " + blueprint.name);
        }

        private IEnumerable<int> Sprint26Breath(ExpandedSummoningCorrectionFixture fixture, bool turnBased)
        {
            var owner = CastExpandedSummoningOwnTier(fixture, "gorgon");
            var second = CastExpandedSummoningOwnTier(fixture, "gorgon");
            SetExpandedSummoningBrainActive(owner, false); SetExpandedSummoningBrainActive(second, false);
            var target = fixture.Hostile;
            var breath = fixture.Blueprints.OfType<BlueprintAbility>().Single(v => v.name == "KMG_Summoning_Special_Gorgon_Breath");
            var stone = fixture.Blueprints.OfType<BlueprintBuff>().Single(v => v.name == "KMG_Summoning_Special_Gorgon_Petrification");
            var cooldown = fixture.Blueprints.OfType<BlueprintBuff>().Single(v => v.name == "KMG_Summoning_Special_Gorgon_Cooldown");
            int fort = target.Stats.SaveFortitude.BaseValue;
            var random = UnityEngine.Random.state;
            var casterPosition = fixture.Caster.Position;
            var beforeFaction = target.Faction;
            bool partyAdded = false;
            Kingmaker.UnitLogic.Abilities.Ability cureGrant = null;
            var saves = new Sprint26Saves(target);
            try
            {
                foreach (int frame in Sprint26Appearance(owner, "gorgon")) yield return frame;
                foreach (int frame in Sprint26Appearance(second, "gorgon")) yield return frame;
                owner.JoinCombat(); target.JoinCombat(); second.JoinCombat();
                Vector3 far; string lane, direction;
                Vector3 origin = Sprint24AiCombatLane(fixture.Caster, owner, target, 10f, out far, out lane, out direction);
                Vector3 forward = (far - origin).normalized;
                PlaceExpandedSummoningUnit(owner, origin); PlaceExpandedSummoningUnit(target, far);
                PlaceExpandedSummoningUnit(second, origin - forward * 5f);
                var data = new AbilityData(owner.Descriptor.Abilities.GetAbility(breath));
                PlaceExpandedSummoningUnit(fixture.Caster, origin + forward * 4f);
                bool allyRefused = !data.CanTarget(new TargetWrapper(target));
                PlaceExpandedSummoningUnit(fixture.Caster, origin - forward * 7f);
                bool safeAccepted = data.CanTarget(new TargetWrapper(target));
                Sprint25Check("breath-safe-cone-" + turnBased, allyRefused && safeAccepted,
                    new JObject { ["allyRefused"] = allyRefused, ["safeAccepted"] = safeAccepted, ["lane"] = lane,
                        ["lengthFeet"] = breath.GetComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityDeliverProjectile>().Length.Value },
                    "The exact native 60-foot blocked cone refuses an ally in its area and admits the safe cast.");
                if (!allyRefused || !safeAccepted) throw new InvalidOperationException("Safe cone precondition not obtained.");
                if (turnBased)
                {
                    var admission = new JObject();
                    foreach (int frame in Sprint22EnrollTurnBasedOwners(new[] { owner, second }, target, admission)) yield return frame;
                    for (int i = 0; i < 500 && !Sprint18OwnerIsActing(owner); i++) yield return 0;
                    if (!(bool)admission["admitted"] || !Sprint18OwnerIsActing(owner)) throw new InvalidOperationException("Breath owner native turn missing.");
                }
                target.Stats.SaveFortitude.BaseValue = -100;
                saves.DesiredRoll = 2;
                UnitUseAbility first = null;
                var costs = new JArray();
                bool clockSeen = false, independentSeen = false; double initialSeconds = -1;
                using (var economy = new FirearmReviewCosts(owner, costs))
                {
                    foreach (int frame in Sprint26Cast(owner, breath, new TargetWrapper(target), v => first = v, v => {
                        var activeClock = owner.Buffs.GetBuff(cooldown);
                        if (activeClock != null && !clockSeen) {
                            clockSeen = !data.IsAvailable;
                            independentSeen = second.Buffs.GetBuff(cooldown) == null;
                            initialSeconds = (activeClock.EndTime - Game.Instance.TimeController.GameTime).TotalSeconds;
                        }
                    })) yield return frame;
                    var applied = target.Buffs.GetBuff(stone);
                    var state = applied == null ? null : applied.SelectComponents<GorgonPetrificationState>().Single();
                    var activation = GorgonBreathRuntime.Latest(owner);
                    bool firstExact = applied != null && state != null && state.Initialized && !state.Permanent &&
                        state.RecoveryDc == 21 && state.FirstSourceId == owner.UniqueId &&
                        target.Descriptor.State.HasCondition(Kingmaker.UnitLogic.UnitCondition.Petrified) && activation != null &&
                        activation.CooldownRounds >= 2 && activation.CooldownRounds <= 5 && economy.Count(first) == 1;
                    Sprint25Check("breath-first-" + turnBased, firstExact, new JObject { ["unit"] = owner.UniqueId,
                        ["victim"] = target.UniqueId, ["petrified"] = target.Descriptor.State.HasCondition(Kingmaker.UnitLogic.UnitCondition.Petrified),
                        ["lifeStateDead"] = target.Descriptor.State.IsDead, ["permanent"] = state?.Permanent,
                        ["recoveryDc"] = state?.RecoveryDc, ["rolledCooldownRounds"] = activation?.CooldownRounds,
                        ["source"] = state?.FirstSourceId, ["saves"] = saves.Rows.DeepClone(), ["costs"] = costs },
                        "One native cone activation produces temporary victim-owned petrification, Constitution DC21 and an actual 2?5 round clock.");
                    if (!firstExact) throw new InvalidOperationException("Initial petrification did not match its required stage.");
                    AbilityExecutionContext context;
                    if (!activation.Context.TryGetTarget(out context)) throw new InvalidOperationException("Breath context lost before duplicate control.");
                    int saveCount = saves.Count;
                    string duplicate = GorgonBreathRuntime.Apply(context, target, stone);
                    Sprint25Check("breath-duplicate-" + turnBased, duplicate == "duplicate" && !state.Permanent && saves.Count == saveCount,
                        new JObject { ["outcome"] = duplicate, ["permanent"] = state.Permanent, ["savesBefore"] = saveCount, ["savesAfter"] = saves.Count },
                        "A repeated delivery callback from the same activation cannot grant another exposure or save.");
                    bool sourceClock = clockSeen && initialSeconds > 0 && initialSeconds <= activation.CooldownRounds * 6;
                    bool independent = independentSeen;
                    Sprint25Check("breath-independent-clock-" + turnBased, sourceClock && independent,
                        new JObject { ["firstBlocked"] = sourceClock, ["initialSeconds"] = initialSeconds, ["secondAvailableClock"] = independent }, "The rolled cooldown belongs only to the breathing instance.");
                    // Recovery is observed through the ordinary native Buffs tick, never direct removal.
                    target.Stats.SaveFortitude.BaseValue = 100; saves.DesiredRoll = 20;
                    int frames = 0;
                    while (target.Buffs.GetBuff(stone) != null && frames++ < 300) { Sprint26ProgressTurn(owner); yield return 0; }
                    Sprint25Check("breath-natural-recovery-" + turnBased, target.Buffs.GetBuff(stone) == null &&
                        !target.Descriptor.State.HasCondition(Kingmaker.UnitLogic.UnitCondition.Petrified),
                        new JObject { ["frames"] = frames, ["saves"] = saves.Rows.DeepClone(),
                            ["stoneAbsent"] = target.Buffs.GetBuff(stone) == null }, "A genuine subsequent native round save removes the temporary state and condition.");
                }
                // A new first exposure after recovery, then a different source's
                // real cone, qualifies the permanent transition and attribution.
                target.Stats.SaveFortitude.BaseValue = -100; saves.DesiredRoll = 2;
                for (int i = 0; owner.Buffs.GetBuff(cooldown) != null && i < 300; i++) { Sprint26ProgressTurn(owner); yield return 0; }
                foreach (int frame in Sprint26Cast(owner, breath, new TargetWrapper(target), v => { })) yield return frame;
                var temporary = target.Buffs.GetBuff(stone);
                if (temporary == null) throw new InvalidOperationException("Second temporary exposure missing.");
                // Keep source two in the same surveyed lane and source one safely behind it.
                PlaceExpandedSummoningUnit(owner, origin - forward * 7f);
                PlaceExpandedSummoningUnit(second, origin);
                if (turnBased)
                {
                    for (int i = 0; i < 700 && !Sprint18OwnerIsActing(second); i++) yield return 0;
                    if (!Sprint18OwnerIsActing(second)) throw new InvalidOperationException("Cross-source turn missing.");
                }
                foreach (int frame in Sprint26Cast(second, breath, new TargetWrapper(target), v => { })) yield return frame;
                var permanent = target.Buffs.GetBuff(stone);
                var permanentState = permanent == null ? null : permanent.SelectComponents<GorgonPetrificationState>().Single();
                int beforeRounds = saves.Count;
                for (int i = 0; i < 60; i++) { Sprint26ProgressTurn(second); yield return 0; }
                bool permanentExact = ReferenceEquals(temporary, permanent) && permanentState != null && permanentState.Permanent &&
                    permanentState.FirstSourceId == owner.UniqueId && permanentState.LastSourceId == second.UniqueId && saves.Count == beforeRounds;
                Sprint25Check("breath-permanent-" + turnBased, permanentExact, new JObject { ["sameVictimBuff"] = ReferenceEquals(temporary, permanent),
                    ["permanent"] = permanentState?.Permanent, ["firstSource"] = permanentState?.FirstSourceId,
                    ["laterSource"] = permanentState?.LastSourceId, ["savesBeforeRounds"] = beforeRounds, ["savesAfterRounds"] = saves.Count },
                    "A distinct cross-source breath escalates the same own temporary buff to permanent with no natural recovery saves.");
                if (!permanentExact) throw new InvalidOperationException("Permanent transition failed.");
                owner.Commands.InterruptAll(true); second.Commands.InterruptAll(true);
                DisposeExpandedSummoningUnits(fixture.Created, new[] { owner, second });
                Sprint25Check("breath-source-removal-" + turnBased, owner.Destroyed && second.Destroyed && target.Buffs.GetBuff(stone) == permanent && permanentState.Permanent,
                    new JObject { ["sourceRemoved"] = owner.Destroyed && second.Destroyed, ["victimStillPermanent"] = target.Buffs.GetBuff(stone) == permanent },
                    "Removing the summons does not shorten the permanent victim-owned state.");
                // Production Stone to Flesh is party-targeted. Admit only this
                // disposable opponent to the disposable fixture party for its
                // real eligibility and removal path, restoring both memberships.
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate();
                target.Descriptor.SwitchFactions(fixture.Caster.Faction, false);
                Game.Instance.Player.Party.Add(target); partyAdded = true;
                var cure = fixture.Blueprints.OfType<BlueprintAbility>().Single(v => v.name == "StoneToFlesh");
                cureGrant = fixture.Caster.Descriptor.AddFact(cure) as Kingmaker.UnitLogic.Abilities.Ability;
                target.Stats.SaveFortitude.BaseValue = 100; saves.DesiredRoll = 20;
                foreach (int frame in Sprint26Cast(fixture.Caster, cure, new TargetWrapper(target), v => { })) yield return frame;
                Sprint25Check("breath-production-cure-" + turnBased, target.Buffs.GetBuff(stone) == null &&
                    !target.Descriptor.State.HasCondition(Kingmaker.UnitLogic.UnitCondition.Petrified) && !target.Descriptor.State.IsDead,
                    new JObject { ["nativeCure"] = cure.AssetGuid, ["victim"] = target.UniqueId,
                        ["stoneAbsent"] = target.Buffs.GetBuff(stone) == null, ["dead"] = target.Descriptor.State.IsDead },
                    "The actual native Stone to Flesh command removes permanent petrification from a disposable eligible party actor.");
            }
            finally
            {
                saves.Dispose(); UnityEngine.Random.state = random; target.Stats.SaveFortitude.BaseValue = fort;
                if (partyAdded) Game.Instance.Player.Party.Remove(target);
                target.Descriptor.SwitchFactions(beforeFaction, false);
                if (cureGrant != null) fixture.Caster.Descriptor.RemoveFact(cureGrant);
                PlaceExpandedSummoningUnit(fixture.Caster, casterPosition);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false; Game.Instance.TurnBasedCombatController.Activate();
                foreach (var actor in new[] { owner, second }) if (!actor.Destroyed) actor.Commands.InterruptAll(true);
                DisposeExpandedSummoningUnits(fixture.Created, new[] { owner, second });
                // This is cleanup only; it never supplies the cure predicate.
                target.Buffs.GetBuff(stone)?.Remove(); ResetExpandedSummoningHostile(fixture);
            }
        }

        private void Sprint26ProgressTurn(UnitEntityData owner)
        {
            if (CombatController.IsInTurnBasedCombat() && Sprint18OwnerIsActing(owner))
                Sprint18EndTurn(Game.Instance.TurnBasedCombatController.CurrentTurn);
        }

        private sealed class Sprint26Saves : IGlobalRulebookHandler<RuleSavingThrow>, IDisposable
        {
            private readonly UnitEntityData _victim;
            internal int DesiredRoll = 2; internal int Count; internal readonly JArray Rows = new JArray();
            internal Sprint26Saves(UnitEntityData victim) { _victim = victim; EventBus.Subscribe(this); }
            public void OnEventAboutToTrigger(RuleSavingThrow evt)
            { if (ReferenceEquals(evt.Initiator, _victim)) UnityEngine.Random.InitState(FindNativeD20Seed(DesiredRoll)); }
            public void OnEventDidTrigger(RuleSavingThrow evt)
            { if (ReferenceEquals(evt.Initiator, _victim)) { Count++; Rows.Add(new JObject { ["dc"] = evt.DifficultyClass,
                ["passed"] = evt.IsPassed, ["reason"] = evt.Reason.ToString(), ["gameTime"] = Game.Instance.Player.GameTime.TotalSeconds }); } }
            public void Dispose() { EventBus.Unsubscribe(this); }
        }

        private sealed class Sprint26JumpSeed : IGlobalRulebookHandler<RuleSkillCheck>, IDisposable
        {
            private readonly UnitEntityData _owner; private readonly int _roll;
            internal int Count;
            internal Sprint26JumpSeed(UnitEntityData owner, int roll) { _owner = owner; _roll = roll; EventBus.Subscribe(this); }
            public void OnEventAboutToTrigger(RuleSkillCheck evt)
            { if (ReferenceEquals(evt.Initiator, _owner) && evt.StatType == Kingmaker.EntitySystem.Stats.StatType.SkillMobility) { Count++; UnityEngine.Random.InitState(FindNativeD20Seed(_roll)); } }
            public void OnEventDidTrigger(RuleSkillCheck evt) { }
            public void Dispose() { EventBus.Unsubscribe(this); }
        }
    }
}
