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
using Kingmaker.UnitLogic.Commands.Base;
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
        private readonly Sprint26LifeController _sprint26Life = new Sprint26LifeController();
        private void Sprint26TickLife(UnitEntityData unit)
        {
            if (_sprint25Fixture != null && (_sprint25Fixture.Created.Contains(unit) ||
                ReferenceEquals(unit, _sprint25Fixture.Caster) || ReferenceEquals(unit, _sprint25Fixture.Hostile)))
                _sprint26Life.TickOwned(unit);
        }
        private sealed class Sprint26LifeController : Kingmaker.Controllers.Units.UnitLifeController
        {
            private readonly Dictionary<string, int> _ticks = new Dictionary<string, int>(StringComparer.Ordinal);
            internal int TicksFor(UnitEntityData unit) { int value; return _ticks.TryGetValue(unit.UniqueId, out value) ? value : 0; }
            internal void TickOwned(UnitEntityData unit)
            {
                if (unit == null || unit.Destroyed || !unit.IsInGame || !base.ShouldTickOnUnit(unit)) return;
                _ticks[unit.UniqueId] = TicksFor(unit) + 1;
                base.TickOnUnit(unit);
            }
        }

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
                            if (!turnBased) {
                                if (key == "gorgon") foreach (int frame in Sprint26PowerAttack(owner)) yield return frame;
                                foreach (int frame in Sprint26SignedStrength(fixture, owner, key)) yield return frame;
                                if (key == "bulette") Sprint26Critical(fixture, owner);
                            }
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
                        if (key == "gorgon") foreach (int frame in Sprint26Trample(fixture, turnBased)) yield return frame;
                    }
                    if (!turnBased) {
                        var control = CastExpandedSummoningOwnTier(fixture, "elephant");
                        SetExpandedSummoningBrainActive(control, false);
                        try {
                            foreach (int frame in Sprint26Appearance(control, "released-elephant-control", false)) yield return frame;
                            foreach (int frame in Sprint22SetPowerAttack(control, false)) yield return frame;
                            Sprint25Profile(fixture, control, "elephant");
                        } finally { DisposeExpandedSummoningUnits(fixture.Created, new[] {control}); }
                    }
                    yield break;
                }
                if (phase == "visual-lifecycle") { foreach (int frame in Sprint26VisualLifecycle(fixture)) yield return frame; yield break; }
                if (phase == "rtwp-ai") { foreach (int frame in Sprint26Ai(fixture)) yield return frame; yield break; }
                if (phase.StartsWith("bulette-leap-", StringComparison.Ordinal))
                {
                    foreach (bool success in new[] { true, false })
                        foreach (int frame in Sprint26Leap(fixture, turnBased, success)) yield return frame;
                    if (!turnBased) {
                        foreach (int frame in Sprint26LeapOpportunity(fixture)) yield return frame;
                        foreach (bool lost in new[] { false, true })
                            foreach (int frame in Sprint26LeapCancellation(fixture, lost)) yield return frame;
                        foreach (int frame in Sprint26NativeChargeControl(fixture)) yield return frame;
                    }
                    yield break;
                }
                if (phase.StartsWith("gorgon-breath-", StringComparison.Ordinal))
                {
                    foreach (int frame in Sprint26Breath(fixture, turnBased)) yield return frame;
                    if (!turnBased) {
                        foreach (int frame in Sprint26GorgonCastBoundaries(fixture)) yield return frame;
                        foreach (int frame in Sprint26GorgonLifeBoundaries(fixture)) yield return frame;
                    }
                    yield break;
                }
                // A phase cannot silently fall back to a smaller native subset.
                throw new InvalidOperationException("Sprint26 phase implementation is not yet complete: " + phase);
            }
            finally
            {
                var timing = _sprint25Clock.DescribeTickOwnership();
                Sprint25Check("single-native-frame-tick", (bool)timing["frameSynchronous"] &&
                    (int)timing["maximumActionTicksPerUnitPerUnityFrame"] <= 1&&
                    (int)timing["maximumAbilityExecutionTicksPerProcessPerUnityFrame"]<=1, timing,
                    "The exact fixture actor set receives one native action/cooldown tick per Unity frame; ambient duplicates are withheld and no brain is ticked by the clock.");
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
                    "sprint26-" + key + "-" + state + "-" + owner.UniqueId.Substring(0, 8) + "-" + Time.frameCount + ".png") }, "The exact original Gorgon or native Bulette body is visible, finite and selectable.");
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
                owner.Stats.HitPoints.ModifiedValue == (gorgon ? 100 : 84) && owner.Stats.AC.ModifiedValue == (gorgon ? 20 : 22) &&
                owner.Stats.AC.Touch == (gorgon ? 9 : 10) && owner.Stats.AC.FlatFooted == 20 &&
                owner.Stats.SaveFortitude.ModifiedValue == (gorgon ? 13 : 11) && owner.Stats.SaveReflex.ModifiedValue == (gorgon ? 6 : 8) &&
                owner.Stats.SaveWill.ModifiedValue == (gorgon ? 7 : 5) && owner.Stats.SkillPerception.ModifiedValue == (gorgon ? 17 : 11) &&
                (gorgon || owner.Stats.SkillMobility.ModifiedValue == 9) && trip.Result - ordinary.Result == 4;
            var configuredRanks = owner.Blueprint.GetComponent<SummonExactSkillRanks>();
            int[] scores = new[] { owner.Stats.Strength.ModifiedValue, owner.Stats.Dexterity.ModifiedValue,
                owner.Stats.Constitution.ModifiedValue, owner.Stats.Intelligence.ModifiedValue,
                owner.Stats.Wisdom.ModifiedValue, owner.Stats.Charisma.ModifiedValue };
            int cmb = Rulebook.Trigger(new RuleCalculateCMB(owner, fixture.Hostile, CombatManeuver.Grapple)).Result;
            exact &= scores.SequenceEqual(gorgon ? new[] {24,10,24,2,16,9} : new[] {23,15,20,2,13,6}) &&
                owner.Descriptor.State.Size == (gorgon ? Size.Large : Size.Huge) &&
                owner.Stats.Speed.ModifiedValue == (gorgon ? 30 : 40) && cmb == 16;
            Sprint25Check("profile-" + key, exact, new JObject { ["unit"] = owner.UniqueId, ["attacks"] = rows,
                ["hp"] = owner.Stats.HitPoints.ModifiedValue, ["ruleAc"] = ac, ["ruleTouch"] = touch,
                ["ac"] = owner.Stats.AC.ModifiedValue, ["touch"] = owner.Stats.AC.Touch, ["flatFooted"] = owner.Stats.AC.FlatFooted,
                ["scores"] = new JArray(scores), ["size"] = owner.Descriptor.State.Size.ToString(),
                ["speed"] = owner.Stats.Speed.ModifiedValue, ["cmb"] = cmb,
                ["rankCarrier"] = configuredRanks == null ? null : new JObject { ["perception"] = configuredRanks.PerceptionRanks,
                    ["mobility"] = configuredRanks.MobilityRanks, ["owner"] = configuredRanks.OwningBlueprint.AssetGuid },
                ["blueprintComponents"] = new JArray(owner.Blueprint.ComponentsArray.Select(v => v.GetType().FullName)),
                ["fort"] = owner.Stats.SaveFortitude.ModifiedValue, ["ref"] = owner.Stats.SaveReflex.ModifiedValue,
                ["will"] = owner.Stats.SaveWill.ModifiedValue, ["perception"] = DescribeSprint16Skill(owner.Stats.SkillPerception),
                ["mobility"] = DescribeSprint16Skill(owner.Stats.SkillMobility), ["cmd"] = ordinary.Describe("CMD"),
                ["trip"] = trip.Describe("trip"), ["facts"] = new JArray(owner.Descriptor.Progression.Features.Enumerable.Select(v => v.Blueprint.name)) },
                "The composed adult profile, printed attack roles, dice, threat range, skills and defenses match the frozen contract.");
        }

        private IEnumerable<int> Sprint26SignedStrength(ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner, string key)
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
                        if (key == "gorgon") foreach (int frame in Sprint22SetPowerAttack(owner, false)) yield return frame;
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
                owner.Stats.Strength.BaseValue=strength;
                int additionalDamage=owner.Stats.AdditionalDamage.BaseValue,reachBonus=owner.Stats.Reach.BaseValue;
                try {
                    owner.Stats.AdditionalDamage.BaseValue=additionalDamage+3;
                    owner.Stats.Reach.BaseValue=reachBonus+5;
                    int index=0;
                    foreach(var weapon in Sprint19Limbs(owner)) {
                        if(key=="gorgon")foreach(int f in Sprint22SetPowerAttack(owner,false))yield return f;
                        var toggle=key=="gorgon"?owner.Descriptor.ActivatableAbilities.Enumerable.Single(v=>v.Blueprint.AssetGuid=="a7b339e4f6ff93a4697df5d7a87ff619"):null;
                        bool toggleOn=toggle!=null&&toggle.IsOn,toggleRunning=toggle!=null&&toggle.IsRunning;
                        bool powerBuff=toggle!=null&&owner.Buffs.GetBuff(toggle.Blueprint.Buff)!=null;
                        var calculated=Rulebook.Trigger(new RuleCalculateWeaponStats(owner,weapon,null));
                        observer.Clear();UnityEngine.Random.InitState(FindNativeD20Seed(18));
                        Rulebook.Trigger(new RuleAttackWithWeapon(owner,fixture.Hostile,weapon,0));
                        int baseline=key=="gorgon"?(index==0?7:3):(index==0?9:6);
                        var damage=observer.Damage.FirstOrDefault();
                        int rangeFeet=weapon.AttackRange.Value;
                        bool row=damage!=null&&damage.DamageBundle.First.Bonus==baseline+3&&rangeFeet==(key=="gorgon"?10:15);
                        exact&=row;rows.Add(new JObject{["liveAdditionalDamage"]=3,["liveReachFeet"]=5,["weapon"]=weapon.Blueprint.AssetGuid,
                            ["rangeFeet"]=rangeFeet,["strengthScore"]=owner.Stats.Strength.ModifiedValue,["strengthModifier"]=owner.Stats.Strength.Bonus,
                            ["secondary"]=weapon.IsSecondary,["ruleSecondary"]=calculated.SecondaryWeapon,["strengthMultiplier"]=calculated.DamageBonusStatMultiplier,
                            ["additionalStrengthMultiplier"]=calculated.AdditionalDamageBonusStatMultiplier,["calculatedBonus"]=calculated.DamageDescription[0].Bonus,
                            ["nativeAdditionalDamage"]=owner.Stats.AdditionalDamage.ModifiedValue,["powerAttackOn"]=toggleOn,["powerAttackRunning"]=toggleRunning,["powerAttackBuff"]=powerBuff,
                            ["damage"]=damage==null?null:Sprint16DamageEvent(damage),["exact"]=row});index++;
                    }
                }finally{owner.Stats.AdditionalDamage.BaseValue=additionalDamage;owner.Stats.Reach.BaseValue=reachBonus;}
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
            Vector3 origin = Sprint24AiCombatLane(owner, owner, target, reach + radii + .2f, out far, out lane, out direction);
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
            var endings = new JArray();
            EventBus.Subscribe(observer);
            try
            {
                using (var economy = new FirearmReviewCosts(owner, costs))
                using (var trace = new Sprint26CommandTrace(owner, endings))
                {
                    owner.Commands.Run(command);
                    int frames = 0; DateTime deadline = DateTime.UtcNow.AddSeconds(30);
                    while (!command.IsFinished && frames++ < 3000 && DateTime.UtcNow < deadline) yield return 0;
                    bool exact = command.IsFinished && (command.Result == UnitCommand.ResultType.Success || trace.Exhausted(command)) && observer.WeaponAttacks.Count == 3 && command.AllAttacks.Count == 3 &&
                        economy.Count(command) == 1 && economy.Exact(command, turnBased) && !command.Cutscene && !command.IsIgnoreCooldown;
                    Sprint25Check("ordinary-" + key + "-" + turnBased, exact, new JObject { ["finished"] = command.IsFinished,
                        ["result"] = command.Result.ToString(), ["frames"] = frames, ["planned"] = command.AllAttacks.Count, ["nativeEnds"] = endings, ["nativeListExhausted"] = trace.Exhausted(command),
                        ["allOwnerAttacks"] = new JArray(observer.OwnerWeaponAttacks.Select(v => new JObject { ["target"] = v.Target?.UniqueId, ["weapon"] = v.Weapon?.Blueprint?.AssetGuid })),
                        ["weapons"] = new JArray(observer.WeaponAttacks.Select(v => v.Weapon.Blueprint.AssetGuid)),
                        ["costs"] = costs, ["ownerDestroyed"] = owner.Destroyed, ["ownerDead"] = owner.Descriptor.State.IsDead,
                        ["ownerPosition"] = SurveyVector(owner.Position), ["targetPosition"] = SurveyVector(target.Position),
                        ["nativeDistance"] = owner.DistanceTo(target), ["targetDead"] = target.Descriptor.State.IsDead,
                        ["targetDestroyed"] = target.Destroyed, ["ownerCanAct"] = owner.Descriptor.State.CanAct,
                        ["ownerCombat"] = owner.IsInCombat, ["targetCombat"] = target.IsInCombat,
                        ["summonBuffs"] = new JArray(owner.Buffs.Enumerable.Select(v => v.Blueprint.name + ":" + v.TimeLeft.TotalSeconds)),
                        ["forcedAnimationEvents"] = 0 }, "The real full attack completes three ordinary attacks with one native action cost and no fixture animation completion.");
                    var normalCmd=ProbeCombatManeuverDefenceParts(target,owner,CombatManeuver.Grapple);
                    var normalTrip=ProbeCombatManeuverDefenceParts(target,owner,CombatManeuver.Trip);
                    Sprint25Check("post-action-cmd-"+key+"-"+turnBased,normalCmd.Result==(key=="gorgon"?26:28)&&normalTrip.Result-normalCmd.Result==4,
                        new JObject{["cmd"]=normalCmd.Describe("CMD"),["trip"]=normalTrip.Describe("trip")},
                        "After its real ordinary action the creature retains normal CMD and quadruped trip defense, including Bulette's Dexterity.");
                    Sprint26Body(owner, key, "attack-" + turnBased);
                }
            }
            finally { EventBus.Unsubscribe(observer); owner.Commands.InterruptAll(true); ResetExpandedSummoningHostile(fixture); }
        }

        private void Sprint26Routes(ExpandedSummoningCorrectionFixture fixture)
        {
            Sprint26IconConsumers(fixture);
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
                // Cost application is deliberately inert outside combat. Join
                // these exact disposable actors before observing expenditure.
                if (!turnBased && success) Sprint26LeapGeometryRefusals(fixture, owner, target, data);
                owner.JoinCombat(); target.JoinCombat();
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
                        float maximumVelocity = 0f;
                        int frames = 0; DateTime deadline = DateTime.UtcNow.AddSeconds(35);
                        while (frames++ < 4000 && DateTime.UtcNow < deadline)
                        {
                            maximumVelocity = Math.Max(maximumVelocity, owner.View.MovementAgent.Velocity.magnitude);
                            attack = attack ?? owner.Commands.Raw.OfType<UnitAttack>().FirstOrDefault();
                            if (command.ExecutionProcess != null && !command.ExecutionProcess.IsEnded) command.ExecutionProcess.Tick();
                            yield return 0;
                            if (attack != null && attack.IsFinished && command.IsFinished) break;
                        }
                        var attempt = BuletteLeapRuntime.Observation(attack);
                        var landing = Kingmaker.View.ObstacleAnalyzer.GetNearestNode(owner.Position);
                        var startNode = Kingmaker.View.ObstacleAnalyzer.GetNearestNode(start);
                        var traceEnd = Kingmaker.View.ObstacleAnalyzer.TraceAlongNavmesh(start, owner.Position);
                        float projectionGap = Vector3.Distance(landing.clampedPosition, owner.Position);
                        float continuityGap = Vector3.Distance(traceEnd, owner.Position);
                        var overlaps = Game.Instance.State.Units.All.Where(v => v != null && v.View != null &&
                            !ReferenceEquals(v, owner) && !v.Destroyed && !v.Descriptor.State.IsDead &&
                            owner.DistanceTo(v) < owner.Corpulence + v.Corpulence - .05f).Select(v => v.UniqueId).ToArray();
                        // A native connected route may cross graph/area identifiers.
                        // Use the same native query and ray trace as charge setup;
                        // retain the identifiers as observations, not a substitute.
                        bool legalLanding = landing.node != null && startNode.node != null && landing.node.Walkable &&
                            projectionGap <= .6f && continuityGap <= .6f && overlaps.Length == 0;
                        var childCosts = costs.OfType<JObject>().Where(v => (string)v["nativeCost"] == "UnitAttack").ToArray();
                        bool childUnchanged = attack != null && attack.IsIgnoreCooldown && economy.Count(attack) == 1 && childCosts.Length == 1 &&
                            childCosts[0]["before"].Values<float>().SequenceEqual(childCosts[0]["after"].Values<float>());
                        bool exact = attempt != null && attempt.Check.IsPassed == success && seed.Count == 1 &&
                            attempt.Check.BaseRollResult == (success ? 37 : 18) && attempt.JumpBonus == 8 &&
                            owner.Stats.SkillMobility.ModifiedValue == 9 && attack != null && attack.IsFinished &&
                            observer.WeaponAttacks.Count == (success ? 4 : 1) &&
                            (!success || observer.WeaponAttacks.All(v => ReferenceEquals(v.Weapon.Blueprint, attempt.Claw))) &&
                            economy.Count(command) == 1 && economy.Exact(command, turnBased) && childUnchanged &&
                            !BuletteLeapRuntime.IsArmed(owner) && attack.IsCharge && legalLanding &&
                            Sprint26MagicalBeastRules.ChargeTravelIsLegal(Vector3.Distance(start, owner.Position)) &&
                            attempt.EligibleAtStart && Sprint26MagicalBeastRules.ChargeTravelIsLegal(attempt.TravelAtQueue) && maximumVelocity > 0f;
                        Sprint25Check("leap-" + turnBased + "-" + success, exact, new JObject { ["route"] = route,
                            ["finished"] = command.IsFinished, ["result"] = command.Result.ToString(), ["frames"] = frames,
                            ["start"] = SurveyVector(start), ["landing"] = SurveyVector(owner.Position),
                            ["travel"] = Vector3.Distance(start, owner.Position), ["legalLanding"] = legalLanding,
                            ["landingWalkable"] = landing.node != null && landing.node.Walkable,
                            ["landingProjectionGap"] = projectionGap, ["navmeshContinuityGap"] = continuityGap,
                            ["landingArea"] = landing.node == null ? -1 : (long)landing.node.Area,
                            ["startArea"] = startNode.node == null ? -1 : (long)startNode.node.Area,
                            ["landingGraph"] = landing.node == null ? -1 : (long)landing.node.GraphIndex,
                            ["startGraph"] = startNode.node == null ? -1 : (long)startNode.node.GraphIndex,
                            ["overlappingUnits"] = new JArray(overlaps),
                            ["maximumNativeVelocity"] = maximumVelocity, ["eligibleRunupAtStart"] = attempt?.EligibleAtStart,
                            ["nativeTravelAtQueue"] = attempt?.TravelAtQueue, ["minimumTravelMeters"] = Sprint26MagicalBeastRules.ChargeMinimumMeters, ["childAddsNoCost"] = childUnchanged,
                            ["ownerInCombat"] = owner.IsInCombat, ["childIsCharge"] = attack?.IsCharge, ["childIgnoresCooldown"] = attack?.IsIgnoreCooldown,
                            ["checkCount"] = seed.Count, ["checkTotal"] = attempt?.Check.BaseRollResult,
                            ["checkSuccess"] = attempt?.Check.IsPassed, ["jumpBonus"] = attempt?.JumpBonus,
                            ["mobility"] = owner.Stats.SkillMobility.ModifiedValue, ["plannedClaws"] = attempt?.PlannedClaws,
                            ["attackFinished"] = attack?.IsFinished, ["weapons"] = new JArray(observer.WeaponAttacks.Select(v => v.Weapon.Blueprint.AssetGuid)),
                            ["costs"] = costs, ["armedAfter"] = BuletteLeapRuntime.IsArmed(owner), ["forcedAnimationEvents"] = 0 },
                            "A genuine seeded native jump check chooses four claws/no bite or one failed-charge fallback, with native movement, attacks and action cost.");
                        Sprint26Body(owner, "bulette", "leap-" + success);
                    }
                }
                if (success)
                    foreach (int frame in Sprint26Ordinary(fixture, owner, "bulette", turnBased)) yield return frame;
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
                Vector3 origin, forward; JObject lane;
                bool safeLane = Sprint26FindBreathLane(fixture, owner, second, target, breath, out origin, out forward, out lane);
                Sprint25Check("breath-safe-cone-" + turnBased, safeLane, lane,
                    "One legal native 60-foot cone reaches the disposable target, refuses the fixture ally and accepts the restored safe layout.");
                if (!safeLane) throw new InvalidOperationException("Safe cone precondition not obtained in bounded native search.");
                var data = new AbilityData(owner.Descriptor.Abilities.GetAbility(breath));
                if (turnBased)
                {
                    var admission = new JObject();
                    foreach (int frame in Sprint22EnrollTurnBasedOwners(new[] { owner, second }, target, admission)) yield return frame;
                    for (int i = 0; i < 500 && !Sprint18OwnerIsActing(owner); i++) yield return 0;
                    if (!(bool)admission["admitted"] || !Sprint18OwnerIsActing(owner)) throw new InvalidOperationException("Breath owner native turn missing.");
                }
                int damageBeforePetrification = target.Descriptor.Damage;
                int lifeTicksBefore = _sprint26Life.TicksFor(target);
                target.Stats.SaveFortitude.BaseValue = -100;
                saves.DesiredRoll = 2;
                UnitUseAbility first = null;
                var costs = new JArray();
                bool clockSeen = false, independentSeen = false; double initialSeconds = -1;
                using (var economy = new FirearmReviewCosts(owner, costs))
                using (var projectile = new Sprint26BreathProjectileTrace(owner))
                {
                    bool fxFrameCaptured=false;
                    foreach (int frame in Sprint26Cast(owner, breath, new TargetWrapper(target), v => first = v, v => {
                        if(!fxFrameCaptured&&projectile.LaunchFrame>=0&&Time.frameCount>=projectile.LaunchFrame+8){
                            Sprint26Body(owner,"gorgon","breath-projectile-"+turnBased);fxFrameCaptured=true;
                        }
                        var activeClock = owner.Buffs.GetBuff(cooldown);
                        if (activeClock != null && !clockSeen) {
                            clockSeen = !data.IsAvailable;
                            Sprint26Body(owner, "gorgon", "breath-" + turnBased);
                            independentSeen = second.Buffs.GetBuff(cooldown) == null;
                            initialSeconds = (activeClock.EndTime - Game.Instance.TimeController.GameTime).TotalSeconds;
                        }
                    })) yield return frame;
                    if(projectile.Rows.Count>0&&!fxFrameCaptured){
                        for(int visualFrame=0;visualFrame<8;visualFrame++)yield return 0;Sprint26Body(owner,"gorgon","breath-projectile-"+turnBased);fxFrameCaptured=true;
                    }
                    Sprint25Check("breath-native-projectile-readback-"+turnBased,projectile.Rows.Count>0&&fxFrameCaptured&&
                        projectile.Rows.OfType<JObject>().All(v=>(string)v["nativeSourceBone"]=="Jaw"&&(bool)v["viewPresent"]&&
                            (bool)v["nativeSourceBoneMatchesSkinnedBone"]&&(bool)v["resolvedSourceFound"]&&(float)v["resolvedSourceGapMeters"]<.001f),
                        new JObject{["projectiles"]=projectile.Rows,["postLaunchFrameCaptured"]=fxFrameCaptured,["observerMovedProjectile"]=false},
                        "The exact native breath projectile launch/source-bone coordinates and an actual later FX frame are captured read-only; mouth placement is assessed from these coordinates and real pixels.");
                    var applied = target.Buffs.GetBuff(stone);
                    var state = applied == null ? null : applied.SelectComponents<GorgonPetrificationState>().Single();
                    var activation = GorgonBreathRuntime.Latest(owner);
                    bool firstExact = !target.Descriptor.State.IsDead && target.Descriptor.Damage == damageBeforePetrification &&
                        _sprint26Life.TicksFor(target) > lifeTicksBefore && applied != null && state != null && state.Initialized && !state.Permanent &&
                        state.RecoveryDc == 21 && state.FirstSourceId == owner.UniqueId &&
                        GorgonPetrificationLife.ConditionCount(target.Descriptor.State) == 1 &&
                        !target.Buffs.Enumerable.Any(v => v.Blueprint.AssetGuid == Sprint26MagicalBeastRules.PetrifiedDonor) &&
                        target.Descriptor.State.HasCondition(Kingmaker.UnitLogic.UnitCondition.Petrified) && activation != null &&
                        activation.CooldownRounds >= 2 && activation.CooldownRounds <= 5 && economy.Count(first) == 1 &&
                        activation.Targets.All(id=>(id==target.UniqueId||fixture.Created.Any(v=>v!=null&&v.UniqueId==id))&&!fixture.UnitsBefore.OfType<UnitEntityData>().Any(v=>v!=null&&v.UniqueId==id));
                    Sprint25Check("breath-first-" + turnBased, firstExact, new JObject { ["unit"] = owner.UniqueId,
                        ["victim"] = target.UniqueId, ["petrified"] = target.Descriptor.State.HasCondition(Kingmaker.UnitLogic.UnitCondition.Petrified),
                        ["lifeStateDead"] = target.Descriptor.State.IsDead, ["damageBefore"] = damageBeforePetrification,
                        ["damageAfter"] = target.Descriptor.Damage, ["nativeLifeTicks"] = _sprint26Life.TicksFor(target) - lifeTicksBefore,
                        ["petrifiedConditionCount"] = GorgonPetrificationLife.ConditionCount(target.Descriptor.State),
                        ["petrificationBuffs"] = new JArray(target.Buffs.Enumerable.Where(v => v.Blueprint.AssetGuid == stone.AssetGuid ||
                            v.Blueprint.AssetGuid == Sprint26MagicalBeastRules.PetrifiedDonor).Select(v => v.Blueprint.AssetGuid)), ["permanent"] = state?.Permanent,
                        ["recoveryDc"] = state?.RecoveryDc, ["rolledCooldownRounds"] = activation?.CooldownRounds,
                        ["actualExposureIds"]=activation==null?null:new JArray(activation.Targets),
                        ["allExposuresRequestOwned"]=activation!=null&&activation.Targets.All(id=>(id==target.UniqueId||fixture.Created.Any(v=>v!=null&&v.UniqueId==id))&&!fixture.UnitsBefore.OfType<UnitEntityData>().Any(v=>v!=null&&v.UniqueId==id)),
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
                    double recoveryDeadline = Game.Instance.Player.GameTime.TotalSeconds + 12;
                    DateTime recoveryWall = DateTime.UtcNow.AddSeconds(25);
                    while (target.Buffs.GetBuff(stone) != null && Game.Instance.Player.GameTime.TotalSeconds < recoveryDeadline &&
                        DateTime.UtcNow < recoveryWall && frames++ < 3000) { Sprint26ProgressTurn(owner); yield return 0; }
                    Sprint25Check("breath-natural-recovery-" + turnBased, target.Buffs.GetBuff(stone) == null &&
                        !target.Descriptor.State.HasCondition(Kingmaker.UnitLogic.UnitCondition.Petrified) &&
                        !target.Descriptor.State.IsDead && target.Descriptor.Damage == damageBeforePetrification,
                        new JObject { ["frames"] = frames, ["saves"] = saves.Rows.DeepClone(),
                            ["stoneAbsent"] = target.Buffs.GetBuff(stone) == null }, "A genuine subsequent native round save removes the temporary state and condition.");
                }
                // A new first exposure after recovery, then a different source's
                // real cone, qualifies the permanent transition and attribution.
                target.Stats.SaveFortitude.BaseValue = -100; saves.DesiredRoll = 2;
                double clockDeadline = Game.Instance.Player.GameTime.TotalSeconds + 36;
                DateTime clockWall = DateTime.UtcNow.AddSeconds(45);
                for (int i = 0; owner.Buffs.GetBuff(cooldown) != null && i < 5000 &&
                    Game.Instance.Player.GameTime.TotalSeconds < clockDeadline && DateTime.UtcNow < clockWall; i++)
                    { Sprint26ProgressTurn(owner); yield return 0; }
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
                double permanentStarted = Game.Instance.Player.GameTime.TotalSeconds;
                DateTime permanentWall = DateTime.UtcNow.AddSeconds(25);
                for (int i = 0; i < 3000 && Game.Instance.Player.GameTime.TotalSeconds - permanentStarted < 12 &&
                    DateTime.UtcNow < permanentWall; i++) { Sprint26ProgressTurn(second); yield return 0; }
                bool permanentExact = target.Descriptor.State.IsDead && ReferenceEquals(temporary, permanent) && permanentState != null && permanentState.Permanent &&
                    permanentState.FirstSourceId == owner.UniqueId && permanentState.LastSourceId == second.UniqueId && saves.Count == beforeRounds &&
                    Game.Instance.Player.GameTime.TotalSeconds - permanentStarted >= 12;
                Sprint25Check("breath-permanent-" + turnBased, permanentExact, new JObject { ["sameVictimBuff"] = ReferenceEquals(temporary, permanent),
                    ["permanent"] = permanentState?.Permanent, ["firstSource"] = permanentState?.FirstSourceId,
                    ["laterSource"] = permanentState?.LastSourceId, ["savesBeforeRounds"] = beforeRounds, ["savesAfterRounds"] = saves.Count, ["nativeGameSeconds"] = Game.Instance.Player.GameTime.TotalSeconds - permanentStarted },
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

        private static bool Sprint26WalkablePoint(Vector3 requested, Pathfinding.NNInfo anchor, out Vector3 point)
        {
            var nearest = AstarPath.active.GetNearest(requested);
            point = nearest.clampedPosition;
            return nearest.node != null && nearest.node.Walkable && nearest.node.Area == anchor.node.Area &&
                nearest.node.GraphIndex == anchor.node.GraphIndex && Vector3.Distance(requested, point) <= .6f;
        }

        private static bool Sprint26FindBreathLane(ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, UnitEntityData second, UnitEntityData target, BlueprintAbility breath,
            out Vector3 origin, out Vector3 forward, out JObject report)
        {
            var anchor = AstarPath.active.GetNearest(fixture.Caster.Position);
            if (anchor.node == null || !anchor.node.Walkable) throw new InvalidOperationException("No native cone survey anchor.");
            var delivery = breath.GetComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityDeliverProjectile>();
            var data = new AbilityData(owner.Descriptor.Abilities.GetAbility(breath));
            var party = fixture.PartyBefore.OfType<UnitEntityData>().Where(v => v != null && !v.Destroyed).ToArray();
            int origins = 0, connected = 0, reached = 0, safe = 0;
            var lastBlockers = new JArray();
            origin = forward = Vector3.zero;
            report = new JObject { ["rangeMode"] = breath.Range.ToString(), ["effectiveRangeFeet"] = breath.GetRange().Value,
                ["deliveryLengthFeet"] = delivery.Length.Value, ["sourceEnemy"] = owner.IsEnemy(target),
                ["sourceCanAttack"] = owner.CanAttack(target) };
            foreach (int distance in new[] { 6, 10, 14 })
                for (int dx = -15; dx <= 15; dx += 3)
                    for (int dz = -15; dz <= 15; dz += 3)
                    {
                        Vector3 candidate;
                        if (!Sprint26WalkablePoint(anchor.clampedPosition + new Vector3(dx, 0, dz), anchor, out candidate)) continue;
                        origins++;
                        foreach (Vector3 direction in CompassOffsets)
                        {
                            Vector3 point, behind, other, middle;
                            if (!Sprint26WalkablePoint(candidate + direction * distance, anchor, out point) ||
                                !Sprint26WalkablePoint(candidate - direction * 4, anchor, out behind) ||
                                !Sprint26WalkablePoint(candidate - direction * 2, anchor, out other) ||
                                !Sprint26WalkablePoint(candidate + direction * (distance / 2f), anchor, out middle)) continue;
                            if (party.Any(v => Math.Min(Vector3.Distance(v.Position, candidate), Vector3.Distance(v.Position, point)) < 3.5f)) continue;
                            connected++;
                            PlaceExpandedSummoningUnit(owner, candidate); PlaceExpandedSummoningUnit(target, point);
                            if (second != null) PlaceExpandedSummoningUnit(second, other); PlaceExpandedSummoningUnit(fixture.Caster, behind);
                            if (!delivery.WouldTargetUnit(data, target.Position, target)) continue;
                            reached++;
                            // Experiments may expose only request-owned victims,
                            // even if an original scene actor has a hostile faction.
                            if(fixture.UnitsBefore.OfType<UnitEntityData>().Any(v=>v!=null&&!v.Destroyed&&delivery.WouldTargetUnit(data,target.Position,v)))continue;
                            if (!data.CanTarget(new TargetWrapper(target))) {
                                lastBlockers = new JArray(Game.Instance.State.Units.All.Where(v => v != null && !v.Destroyed &&
                                    !ReferenceEquals(v, owner) && !owner.IsEnemy(v) && delivery.WouldTargetUnit(data, target.Position, v))
                                    .Select(v => v.UniqueId));
                                continue;
                            }
                            safe++;
                            PlaceExpandedSummoningUnit(fixture.Caster, middle);
                            bool allyReached = delivery.WouldTargetUnit(data, target.Position, fixture.Caster);
                            bool allyRefused = allyReached && !data.CanTarget(new TargetWrapper(target));
                            PlaceExpandedSummoningUnit(fixture.Caster, behind);
                            bool restoredSafe = data.CanTarget(new TargetWrapper(target));
                            if (!allyRefused || !restoredSafe) continue;
                            origin = candidate; forward = (point - candidate).normalized;
                            report["originCandidates"] = origins; report["connectedCandidates"] = connected;
                            report["targetReachedCandidates"] = reached; report["safeCandidates"] = safe;
                            report["origin"] = SurveyVector(origin); report["target"] = SurveyVector(point);
                            report["allyReached"] = allyReached; report["allyRefused"] = allyRefused; report["safeAccepted"] = restoredSafe;
                            return breath.GetRange().Value == 60 && delivery.Length.Value == 60;
                        }
                    }
            report["originCandidates"] = origins; report["connectedCandidates"] = connected;
            report["targetReachedCandidates"] = reached; report["safeCandidates"] = safe;
            report["lastNonenemyHits"] = lastBlockers;
            report["setupPreconditionObtained"] = false;
            return false;
        }

        private IEnumerable<int> Sprint26PowerAttack(UnitEntityData owner)
        {
            var weapons = Sprint19Limbs(owner);
            foreach (int frame in Sprint22SetPowerAttack(owner, false)) yield return frame;
            int[] attacks = weapons.Select(v => Rulebook.Trigger(new RuleCalculateAttackBonusWithoutTarget(owner, v, 0)).Result).ToArray();
            int[] damage = weapons.Select(v => Rulebook.Trigger(new RuleCalculateWeaponStats(owner, v, null)).DamageDescription[0].Bonus).ToArray();
            foreach (int frame in Sprint22SetPowerAttack(owner, true)) yield return frame;
            int[] activeAttacks = weapons.Select(v => Rulebook.Trigger(new RuleCalculateAttackBonusWithoutTarget(owner, v, 0)).Result).ToArray();
            int[] activeDamage = weapons.Select(v => Rulebook.Trigger(new RuleCalculateWeaponStats(owner, v, null)).DamageDescription[0].Bonus).ToArray();
            Sprint25Check("gorgon-power-attack", Enumerable.Range(0,3).All(i => activeAttacks[i] == attacks[i] - 3 && activeDamage[i] == damage[i] + (i==0 ? 6 : 3)),
                new JObject { ["attacksOff"] = new JArray(attacks), ["attacksOn"] = new JArray(activeAttacks),
                    ["damageOff"] = new JArray(damage), ["damageOn"] = new JArray(activeDamage) },
                "The printed native feat applies BAB8 penalty and distinct primary/secondary Power Attack damage.");
            foreach (int frame in Sprint22SetPowerAttack(owner, false)) yield return frame;
        }

        private void Sprint26Critical(ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner)
        {
            var target = fixture.Hostile; var bite = owner.Body.PrimaryHand.MaybeWeapon;
            int before = target.Stats.AC.BaseValue; var random = UnityEngine.Random.state;
            var rows = new JArray(); bool exact = true;
            try {
                int attack = Rulebook.Trigger(new RuleCalculateAttackBonusWithoutTarget(owner, bite, 0)).Result;
                target.Stats.AC.BaseValue += attack + 10 - Rulebook.Trigger(new RuleCalculateAC(owner, target, AttackType.Melee)).TargetAC;
                foreach (int confirmation in new[] {1,20}) {
                    UnityEngine.Random.InitState(Sprint24FindD20PairSeed(19, confirmation));
                    var hit = Rulebook.Trigger(new RuleAttackWithWeapon(owner, target, bite, 0));
                    var roll = hit.AttackRoll;
                    bool row = roll != null && roll.Roll.Value == 19 && roll.IsCriticalRoll &&
                        roll.CriticalConfirmationRoll.Value == confirmation && roll.IsCriticalConfirmed == (confirmation==20) &&
                        !roll.AutoCriticalConfirmation && !roll.AutoCriticalThreat;
                    exact &= row;
                    rows.Add(new JObject { ["roll"] = roll?.Roll.Value, ["confirmation"] = roll?.CriticalConfirmationRoll.Value,
                        ["confirmed"] = roll?.IsCriticalConfirmed, ["autoConfirmation"] = roll?.AutoCriticalConfirmation, ["exact"] = row });
                }
            } finally {target.Stats.AC.BaseValue=before; UnityEngine.Random.state=random; ResetExpandedSummoningHostile(fixture);}
            Sprint25Check("bulette-native-confirmation", exact, new JObject { ["rows"] = rows },
                "The printed19-20 bite threatens at19 and retains genuine failed/successful native confirmation.");
        }

        private IEnumerable<int> Sprint26Trample(ExpandedSummoningCorrectionFixture fixture, bool turnBased)
        {
            var owner=CastExpandedSummoningQuietUnit(fixture, "gorgon");
            var target=fixture.Hostile;
            var originalSize=target.Descriptor.State.Size; int reflex=target.Stats.SaveReflex.BaseValue;
            var observer=new ExpandedSummoningTrampleObserver {Caster=owner,Target=target};
            try {
                foreach(int frame in Sprint26Appearance(owner,"gorgon")) yield return frame;
                target.Descriptor.State.Size=Size.Medium; target.Stats.SaveReflex.BaseValue=-100;
                target.CombatState.OnNewRound(); target.CombatState.AttackOfOpportunityCount=0;
                var ability=owner.Descriptor.Abilities.Enumerable.Single(v=>v.Blueprint.name=="KMG_Summoning_Special_Gorgon_Trample");
                var data=new AbilityData(ability); Vector3 start; string route;
                var selected=Sprint23FindTrampleRoute(fixture,owner,target,data,out start,out route);
                if(selected==null)throw new InvalidOperationException("No legal Gorgon trample lane: "+route);
                owner.JoinCombat();target.JoinCombat();
                if(turnBased){var admission=new JObject();foreach(int frame in Sprint22EnrollTurnBasedOwners(new[]{owner},target,admission))yield return frame;
                    if(!(bool)admission["admitted"])throw new InvalidOperationException("No Gorgon trample native turn enrollment.");}
                EventBus.Subscribe(observer);var costs=new JArray();UnitUseAbility command=null;bool bodyCaptured=false;
                using(var economy=new FirearmReviewCosts(owner,costs)){
                    foreach(int frame in Sprint26Cast(owner,ability.Blueprint,selected,v=>command=v,v=>{
                        if(!bodyCaptured && observer.Damage.Count>0){Sprint26Body(owner,"gorgon","trample-contact-"+turnBased);bodyCaptured=true;}
                    }))yield return frame;
                    var damage=observer.Damage.FirstOrDefault();
                    bool exact=observer.Saves.Count==1 && observer.Saves[0].DifficultyClass==21 && observer.Damage.Count==1 &&
                        damage.DamageBundle.First.Dice.Rolls==2 && damage.DamageBundle.First.Dice.Dice==DiceType.D8 && damage.DamageBundle.First.Bonus==10 &&
                        Vector3.Distance(start,owner.Position)>1 && economy.Count(command)==1 && economy.Exact(command,turnBased) && bodyCaptured;
                    Sprint25Check("gorgon-trample-command-"+turnBased,exact,new JObject{["route"]=route,["travel"]=Vector3.Distance(start,owner.Position),
                        ["saves"]=new JArray(observer.Saves.Select(v=>v.DifficultyClass)),["damage"]=new JArray(observer.Damage.Select(Sprint16DamageEvent)),
                        ["costs"]=costs,["contactBodyCaptured"]=bodyCaptured,["forcedAnimationEvents"]=0},
                        "A real lawful trample command completes2d8+10/DC21 contact and one native full-round action without fixture animation events.");
                }
            }finally{EventBus.Unsubscribe(observer);SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue=false;Game.Instance.TurnBasedCombatController.Activate();
                owner.Commands.InterruptAll(true);DisposeExpandedSummoningUnits(fixture.Created,new[]{owner});target.Descriptor.State.Size=originalSize;
                target.Stats.SaveReflex.BaseValue=reflex;ResetExpandedSummoningHostile(fixture);}
        }

        private IEnumerable<int> Sprint26Ai(ExpandedSummoningCorrectionFixture fixture)
        {
            foreach (string key in new[] { "gorgon", "bulette" })
                foreach (bool fallback in new[] { false, true })
                    foreach (int frame in Sprint26AiCase(fixture, key, fallback)) yield return frame;
        }

        private IEnumerable<int> Sprint26AiCase(ExpandedSummoningCorrectionFixture fixture, string key, bool fallback)
        {
            ResetExpandedSummoningHostile(fixture);
            // Keep the summoned runtime brain and its production actions intact.
            var owner = CastExpandedSummoningOwnTier(fixture, key);
            var target = fixture.Hostile;
            var casterPosition = fixture.Caster.Position;
            int fort = target.Stats.SaveFortitude.BaseValue;
            var random = UnityEngine.Random.state;
            var attacks = new Sprint24AttackObserver { Owner = owner, Target = target };
            var commands = new HashSet<UnitCommand>();
            var ambient = new HashSet<string>(StringComparer.Ordinal);
            var specialName = "KMG_Summoning_Special_" + (key == "gorgon" ? "Gorgon_Breath" : "Bulette_Leap");
            var special = owner.Descriptor.Abilities.Enumerable.Single(v => v.Blueprint.name == specialName);
            var data = new AbilityData(special);
            string lane, direction; Vector3 far;
            int ticks = 0; bool effect = false; bool fallbackPrecondition = false;
            GorgonBreathActivation activation = null;
            BuletteLeapAttempt leap = null;
            var saves = new Sprint26Saves(target);
            try
            {
                foreach (int frame in Sprint26Appearance(owner, key)) yield return frame;
                owner.Commands.InterruptAll(true); target.Commands.InterruptAll(true);
                owner.Commands.RemoveFinishedAndUpdateQueue(); target.Commands.RemoveFinishedAndUpdateQueue();
                owner.JoinCombat(); target.JoinCombat();
                float distance = key == "gorgon" ? 8f : fallback ? owner.Corpulence + target.Corpulence + .2f : 10f;
                Vector3 origin, forward;
                if (key == "gorgon") {
                    JObject cone;
                    if (!Sprint26FindBreathLane(fixture, owner, null, target, special.Blueprint, out origin, out forward, out cone))
                        throw new InvalidOperationException("AI safe cone precondition not obtained: " + cone);
                    far = target.Position; lane = cone.ToString(Newtonsoft.Json.Formatting.None);
                    if (fallback) {
                        var point = AstarPath.active.GetNearest(origin + forward * 3f);
                        if (point.node == null || !point.node.Walkable) throw new InvalidOperationException("AI fallback ally point unavailable.");
                        PlaceExpandedSummoningUnit(fixture.Caster, point.clampedPosition);
                    }
                } else if (!fallback) {
                    var selected = Sprint23FindChargeRoute(fixture, owner, target, data, out origin, out lane);
                    if (selected == null) throw new InvalidOperationException("AI Leap lane unavailable: " + lane);
                    far = target.Position; forward = (far - origin).normalized;
                } else {
                    origin = Sprint24AiCombatLane(fixture.Caster, owner, target, distance, out far, out lane, out direction);
                    PlaceExpandedSummoningUnit(owner, origin); PlaceExpandedSummoningUnit(target, far);
                    forward = (far - origin).normalized;
                }
                fallbackPrecondition = !fallback || !data.CanTarget(new TargetWrapper(target));
                if (!fallbackPrecondition) throw new InvalidOperationException("AI fallback precondition not obtained: " + key);
                if (!fallback && !data.CanTarget(new TargetWrapper(target))) throw new InvalidOperationException("AI special target is not legal: " + key);
                target.Stats.SaveFortitude.BaseValue = -100; saves.DesiredRoll = 2;
                foreach (var unit in new[] { owner, target }) {
                    unit.Wake(0f);
                    if (!Game.Instance.State.AwakeUnits.Contains(unit)) Game.Instance.State.AwakeUnits.Add(unit);
                }
                owner.Memory.Add(target); target.Memory.Add(owner);
                owner.SightCache.ClearIfNeeded(); target.SightCache.ClearIfNeeded();
                new Kingmaker.Controllers.Units.UnitMemoryController().Tick();
                SetExpandedSummoningBrainActive(owner, true);
                owner.Brain.RestoreAvailableActions();
                owner.JoinCombat(); target.JoinCombat();
                Game.Instance.Player.UpdateIsInCombat();
                new Kingmaker.Controllers.Combat.UnitCombatJoinController().Tick();
                new Kingmaker.Controllers.Combat.UnitCombatPrepareController().Tick();
                EventBus.Subscribe(attacks);
                float travel = 0;
                DateTime deadline = DateTime.UtcNow.AddSeconds(30);
                using (var seed = new Sprint26JumpSeed(owner, 20))
                {
                    for (int frame = 0; frame < 1800 && DateTime.UtcNow < deadline; frame++)
                    {
                        if (frame == 0 || frame % 60 == 0 && !owner.Commands.Raw.Any(v => v != null && !v.IsFinished))
                        {
                            foreach (string id in Sprint24TickExactAi(owner)) ambient.Add(id);
                            ticks++;
                        }
                        foreach (var command in owner.Commands.Raw.Where(v => v != null)) {
                            commands.Add(command);
                            var cast = command as UnitUseAbility;
                            if (cast?.ExecutionProcess != null && !cast.ExecutionProcess.IsEnded) cast.ExecutionProcess.Tick();
                            var attack = command as UnitAttack;
                            if (attack != null) leap = BuletteLeapRuntime.Observation(attack) ?? leap;
                        }
                        travel = Math.Max(travel, Vector3.Distance(origin, owner.Position));
                        activation = GorgonBreathRuntime.Latest(owner) ?? activation;
                        effect = fallback ? attacks.WeaponAttacks.Count > 0 : key == "gorgon"
                            ? target.Buffs.Enumerable.Any(v => v.Blueprint.AssetGuid == Sprint26MagicalBeastRules.PetrificationGuid)
                            : leap != null && leap.PlannedClaws == 4 && attacks.WeaponAttacks.Count == 4;
                        if (effect) break;
                        Game.Instance.IsPaused = false;
                        yield return 0;
                    }
                    bool selected = fallback ? commands.OfType<UnitAttack>().Any(v => v.AiAction != null)
                        : commands.OfType<UnitUseAbility>().Any(v => v.AiAction != null && ReferenceEquals(v.Spell.Blueprint, special.Blueprint));
                    bool exact = selected && effect && ticks > 0 && owner.IsBrainActive && fallbackPrecondition &&
                        (fallback || (key == "gorgon" ? activation != null : leap != null && leap.Check.IsPassed && seed.Count == 1));
                    Sprint25Check("ai-" + key + "-" + (fallback ? "fallback" : "special"), exact,
                        new JObject { ["unit"] = owner.UniqueId, ["target"] = target.UniqueId, ["productionBrain"] = owner.Blueprint.Brain.name,
                            ["lane"] = lane, ["fallbackPrecondition"] = fallbackPrecondition, ["specialAvailable"] = data.IsAvailable,
                            ["exactOwnerTicks"] = ticks, ["ambientCandidatesNeverTicked"] = new JArray(ambient),
                            ["fixtureAttackCommands"] = 0, ["fixtureAbilityCommands"] = 0, ["effectObserved"] = effect,
                            ["travel"] = travel, ["weapons"] = new JArray(attacks.WeaponAttacks.Select(v => v.Weapon.Blueprint.AssetGuid)),
                            ["selected"] = selected, ["commands"] = new JArray(commands.Select(v => new JObject {
                                ["type"] = v.GetType().Name, ["aiAction"] = v.AiAction?.name,
                                ["finished"] = v.IsFinished, ["result"] = v.Result.ToString() })) },
                        "The exact owner's unmodified native brain selects and executes a useful special or lawful ordinary fallback without fixture-issued combat commands.");
                }
            }
            finally
            {
                EventBus.Unsubscribe(attacks); saves.Dispose(); UnityEngine.Random.state = random;
                SetExpandedSummoningBrainActive(owner, false); owner.Commands.InterruptAll(true);
                target.Buffs.Enumerable.FirstOrDefault(v => v.Blueprint.AssetGuid == Sprint26MagicalBeastRules.PetrificationGuid)?.Remove();
                target.Stats.SaveFortitude.BaseValue = fort; PlaceExpandedSummoningUnit(fixture.Caster, casterPosition);
                DisposeExpandedSummoningUnits(fixture.Created, new[] { owner }); ResetExpandedSummoningHostile(fixture);
            }
        }

        // Request-local read-only traces distinguish native attack-list exhaustion
        // from early target/range/animation interruption. No command is advanced
        // or completed by this observer.
        private sealed class Sprint26ClockGate : IDisposable
        {
            private static Sprint26ClockGate active;
            private readonly Func<UnitEntityData, bool> owns;
            private readonly Harmony12.HarmonyInstance harmony;
            private readonly string id = "KMG.Sprint26.ClockGate." + Guid.NewGuid().ToString("N");
            private readonly System.Reflection.MethodInfo actions = typeof(Kingmaker.Controllers.Units.UnitActionController)
                .GetMethod("TickOnUnit", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            private readonly System.Reflection.MethodInfo cooldowns = typeof(Kingmaker.Controllers.Combat.UnitCombatCooldownsController)
                .GetMethod("TickOnUnit", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            private readonly System.Reflection.MethodInfo executions=typeof(Kingmaker.Controllers.AbilityExecutionProcess).GetMethod("Tick");
            private readonly Dictionary<Kingmaker.Controllers.AbilityExecutionProcess,int> executionFrames=new Dictionary<Kingmaker.Controllers.AbilityExecutionProcess,int>();
            private bool inPulse;
            private NativeDefaultLease nativeDefault;
            private int nativeActions, nativeCooldowns, nativeExecutions, refusedNativeTicks;
            private sealed class NativeDefaultLease : IDisposable
            {
                internal Sprint26ClockGate Gate;
                internal Func<bool> Current;
                public void Dispose()
                {
                    if(Gate==null)return;
                    if(!ReferenceEquals(active,Gate) || !ReferenceEquals(Gate.nativeDefault,this) || Gate.inPulse)
                        throw new InvalidOperationException("Native Default clock-gate ownership changed.");
                    Gate.nativeDefault=null;Gate.executionFrames.Clear();Gate=null;
                }
            }
            internal IDisposable AllowOwnedNativeDefault(Func<bool> current)
            {
                if(!ReferenceEquals(active,this) || inPulse || nativeDefault!=null || current==null || !current())
                    throw new InvalidOperationException("One exact active native Default scope is required.");
                executionFrames.Clear();
                return nativeDefault=new NativeDefaultLease {Gate=this,Current=current};
            }
            internal void AssertManualPulseAllowed()
            {
                if(nativeDefault!=null)throw new InvalidOperationException("Manual fixture pulse refused while native Default owns progression.");
            }
            private bool NativeDefaultCurrent()
            {
                bool current=nativeDefault.Current();
                if(!current)refusedNativeTicks++;
                return current;
            }
            private int blockedActions, blockedCooldowns, allowedActions, allowedCooldowns, maximumPerFrame;
            private int allowedExecutions,blockedDuplicateExecutions,maximumExecutionTicks;
            private readonly Dictionary<string, int> perFrame = new Dictionary<string, int>(StringComparer.Ordinal);
            internal bool Restored { get; private set; }
            internal Sprint26ClockGate(Func<UnitEntityData, bool> owns)
            {
                if (active != null || actions == null || cooldowns == null || executions == null) throw new InvalidOperationException("Native frame tick ownership is unavailable or nested.");
                this.owns = owns; harmony = Harmony12.HarmonyInstance.Create(id);
                var flags = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic;
                try {
                    harmony.Patch(actions, new Harmony12.HarmonyMethod(typeof(Sprint26ClockGate).GetMethod("Actions", flags)));
                    harmony.Patch(cooldowns, new Harmony12.HarmonyMethod(typeof(Sprint26ClockGate).GetMethod("Cooldowns", flags)));
                    harmony.Patch(executions,new Harmony12.HarmonyMethod(typeof(Sprint26ClockGate).GetMethod("Executions",flags)));
                    active = this;
                } catch { Dispose(); throw; }
            }
            internal void BeginPulse() { AssertManualPulseAllowed(); if (inPulse) throw new InvalidOperationException("Nested native frame pulse."); inPulse = true; }
            internal void EndPulse() { inPulse = false; }
            private static bool Actions(UnitEntityData __0)
            {
                var gate = active;
                if (gate == null || !gate.owns(__0)) return true;
                if(gate.nativeDefault!=null){bool valid=gate.NativeDefaultCurrent();if(valid)gate.nativeActions++;return valid;}
                if (!gate.inPulse) { gate.blockedActions++; return false; }
                gate.allowedActions++;
                string key = __0.UniqueId + ":" + Time.frameCount;
                int count; gate.perFrame.TryGetValue(key, out count); gate.perFrame[key] = ++count;
                gate.maximumPerFrame = Math.Max(gate.maximumPerFrame, count);
                return true;
            }
            private static bool Cooldowns(UnitEntityData __0)
            {
                var gate = active;
                if (gate == null || !gate.owns(__0)) return true;
                if(gate.nativeDefault!=null){bool valid=gate.NativeDefaultCurrent();if(valid)gate.nativeCooldowns++;return valid;}
                if (!gate.inPulse) { gate.blockedCooldowns++; return false; }
                gate.allowedCooldowns++; return true;
            }
            private static bool Executions(Kingmaker.Controllers.AbilityExecutionProcess __instance)
            {
                var gate=active;var owner=__instance?.Context?.MaybeCaster;
                if(gate==null||owner==null||!gate.owns(owner))return true;
                if(gate.nativeDefault!=null){bool valid=gate.NativeDefaultCurrent();if(valid)gate.nativeExecutions++;return valid;}
                int last;
                if(gate.executionFrames.TryGetValue(__instance,out last)&&last==Time.frameCount){gate.blockedDuplicateExecutions++;return false;}
                gate.executionFrames[__instance]=Time.frameCount;gate.allowedExecutions++;
                string key="execution:"+System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(__instance)+":"+Time.frameCount;
                int count;gate.perFrame.TryGetValue(key,out count);gate.perFrame[key]=++count;gate.maximumExecutionTicks=Math.Max(gate.maximumExecutionTicks,count);return true;
            }
            internal JObject Snapshot()
            {
                return new JObject { ["nativeDefaultLeaseActive"]=nativeDefault!=null,["nativeDefaultActions"]=nativeActions,
                    ["nativeDefaultCooldowns"]=nativeCooldowns,["nativeDefaultExecutions"]=nativeExecutions,["refusedStaleNativeTicks"]=refusedNativeTicks,
                    ["allowedAbilityExecutionTicks"]=allowedExecutions,["blockedDuplicateAbilityExecutionTicks"]=blockedDuplicateExecutions,
                    ["maximumAbilityExecutionTicksPerProcessPerUnityFrame"]=maximumExecutionTicks,
                    ["allowedActionTicks"] = allowedActions, ["allowedCooldownTicks"] = allowedCooldowns,
                    ["blockedAmbientActionTicks"] = blockedActions, ["blockedAmbientCooldownTicks"] = blockedCooldowns,
                    ["maximumActionTicksPerUnitPerUnityFrame"] = maximumPerFrame,
                    ["brainTicks"] = 0, ["scope"] = "exact request fixture/turn-clock actor set" };
            }
            public void Dispose()
            {
                if (ReferenceEquals(active, this)) active = null;
                harmony.Unpatch(actions, Harmony12.HarmonyPatchType.All, id);
                harmony.Unpatch(cooldowns, Harmony12.HarmonyPatchType.All, id);
                harmony.Unpatch(executions,Harmony12.HarmonyPatchType.All,id);executionFrames.Clear();
                var a = harmony.GetPatchInfo(actions); var c = harmony.GetPatchInfo(cooldowns);var e=harmony.GetPatchInfo(executions);
                Restored = (a == null || !a.Prefixes.Any(v => v.owner == id)) &&
                    (c == null || !c.Prefixes.Any(v => v.owner == id))&&(e==null||!e.Prefixes.Any(v=>v.owner==id));
                if (!Restored) throw new InvalidOperationException("Native frame tick ownership hooks remain.");
            }
        }

        private sealed class Sprint26CommandTrace : IDisposable
        {
            private static Sprint26CommandTrace active;
            private readonly UnitEntityData owner;
            private readonly JArray rows;
            private readonly HashSet<UnitCommand> exhausted = new HashSet<UnitCommand>();
            internal bool Exhausted(UnitCommand command) { return exhausted.Contains(command); }
            private readonly Harmony12.HarmonyInstance harmony;
            private readonly string id = "KMG.Sprint26.CommandTrace." + Guid.NewGuid().ToString("N");
            private readonly System.Reflection.MethodInfo interrupt = typeof(UnitCommand).GetMethod("Interrupt", new[] {typeof(bool)});
            private readonly System.Reflection.MethodInfo target = typeof(UnitAttack).GetMethod("UpdateTarget", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            internal Sprint26CommandTrace(UnitEntityData owner, JArray rows)
            {
                if (active != null || interrupt == null || target == null) throw new InvalidOperationException("Native command trace boundary unavailable or nested.");
                this.owner = owner; this.rows = rows;
                harmony = Harmony12.HarmonyInstance.Create(id);
                var flags = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic;
                try {
                    harmony.Patch(interrupt, new Harmony12.HarmonyMethod(typeof(Sprint26CommandTrace).GetMethod("Interrupting", flags)));
                    harmony.Patch(target, null, new Harmony12.HarmonyMethod(typeof(Sprint26CommandTrace).GetMethod("Targeted", flags)));
                    active = this;
                } catch {
                    harmony.Unpatch(interrupt, Harmony12.HarmonyPatchType.All, id);
                    harmony.Unpatch(target, Harmony12.HarmonyPatchType.All, id);
                    throw;
                }
            }
            private static void Interrupting(UnitCommand __instance)
            {
                var trace = active;
                if (trace == null || __instance.IsFinished || !ReferenceEquals(__instance.Executor, trace.owner)) return;
                var attack = __instance as UnitAttack;
                JObject row = Snapshot(attack);
                row["event"] = "native-Interrupt";
                row["commandType"] = __instance.GetType().FullName;
                row["callers"] = new JArray(new System.Diagnostics.StackTrace().GetFrames().Skip(1).Take(10).Select(v =>
                    v.GetMethod().DeclaringType?.FullName + "." + v.GetMethod().Name));
                if (attack != null && (bool)row["plannedAbsent"] && (int)row["plannedIndex"] >= attack.AllAttacks.Count &&
                    row["callers"].Values<string>().Any(v => v.Contains("UnitAttack.OnTick"))) trace.exhausted.Add(attack);
                trace.rows.Add(row);
            }
            private static void Targeted(UnitAttack __instance, bool __result)
            {
                var trace = active;
                if (__result || trace == null || !ReferenceEquals(__instance.Executor, trace.owner)) return;
                JObject row = Snapshot(__instance);
                row["event"] = "native-UpdateTarget-false";
                trace.rows.Add(row);
            }
            private static JObject Snapshot(UnitAttack attack)
            {
                if (attack == null) return new JObject();
                var planned = ReadExpandedSummoningOptionalMember(attack, "PlannedAttack") as AttackHandInfo;
                return new JObject {
                    ["frame"] = Time.frameCount, ["timeSinceStart"] = attack.TimeSinceStart, ["targetInState"] = attack.Target?.IsInState, ["plannedIndex"] = (int)ReadExpandedSummoningOptionalMember(attack, "m_AttackIndex"),
                    ["plannedCount"] = attack.AllAttacks.Count, ["plannedAbsent"] = planned == null,
                    ["plannedWeapon"] = planned?.Hand?.MaybeWeapon?.Blueprint?.AssetGuid, ["plannedRange"] = planned?.WeaponRange,
                    ["target"] = attack.Target?.UniqueId, ["targetDestroyed"] = attack.Target?.Destroyed,
                    ["targetDead"] = attack.Target?.Descriptor?.State?.IsDead,
                    ["targetUnconscious"] = attack.Target?.Descriptor?.State?.IsUnconscious,
                    ["approachRadius"] = attack.ApproachRadius, ["enoughClose"] = attack.IsUnitEnoughClose,
                    ["distance"] = attack.Target == null ? -1f : attack.Executor.DistanceTo(attack.Target),
                    ["ownerCanAct"] = attack.Executor.Descriptor.State.CanAct,
                    ["animationActed"] = attack.Animation?.IsActed, ["animationFinished"] = attack.Animation?.IsFinished,
                    ["unityFrameDelta"] = Time.deltaTime, ["nativeGameDelta"] = Game.Instance.TimeController.GameDeltaTime,
                    ["gameSeconds"] = Game.Instance.Player.GameTime.TotalSeconds
                };
            }
            public void Dispose()
            {
                harmony.Unpatch(interrupt, Harmony12.HarmonyPatchType.All, id);
                harmony.Unpatch(target, Harmony12.HarmonyPatchType.All, id);
                if (ReferenceEquals(active, this)) active = null;
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
