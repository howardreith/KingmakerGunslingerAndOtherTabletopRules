using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Combat;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Visual.Animation.Kingmaker;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> ReviewSprint17SalamanderLifecycle(ExpandedSummoningCorrectionFixture fixture)
        {
            var owner = CastSprint17FinalSnake(fixture, "salamander");
            string placement;
            var attacker = CreateSprint17ContactTarget(fixture, ExpandedSummoningOpenPoint(owner.Position, 1.5f, new List<int>(), out placement));
            var observer = new Sprint16RuleObserver { Owner = attacker, Target = owner };
            var resources = new List<UnityEngine.Object>(); var samples = new JArray(); var wounds = new JArray();
            bool hitPlayed = false, deathPlayed = false, dead = false, finite = true, subscribed = false;
            float dissolve = 0; var commands = new List<UnitAttack>();
            try
            {
                owner.Stats.HitPoints.BaseValue = 1000;
                foreach (int step in WaitSprint17FinalAppearance(new[] { owner })) yield return step;
                var attachment = owner.View.GetComponent<SalamanderHumanVisualAttachment>();
                resources.AddRange(Sprint17ViewResources(owner));
                var set = owner.View.AnimationManager.AnimationSet;
                var hit = owner.View.AnimationManager.GetAction(UnitAnimationType.Hit);
                var prone = owner.View.AnimationManager.GetAction(UnitAnimationType.Prone);
                bool hasHit = hit != null && hit.Clips != null && hit.Clips.Any(c => c != null);
                bool hasDeath = prone != null && prone.Clips != null && prone.Clips.Any(c => c != null);
                var nativeActions = BlueprintRoot.Instance.HumanAnimationSet.Actions.ToArray();
                CheckSprint17Salamander("lifecycle-carriers", attachment.NativeActionsUnchanged &&
                    set.Actions.Take(nativeActions.Length).SequenceEqual(nativeActions),
                    new JObject { ["nativeActions"] = nativeActions.Length, ["hitAvailable"] = hasHit, ["deathAvailable"] = hasDeath },
                    "exact native human action list is intact; absent animations are recorded without inventing a carrier");
                var faction = UnityEngine.Object.Instantiate(owner.Faction);
                _serpentineContactPrototypes.Add(faction);
                faction.name = "KMG_Runtime_Sprint17_SalamanderLifecycle";
                faction.Peaceful = faction.AlwaysEnemy = faction.Neutral = false; faction.Dummy = null;
                faction.AttackFactions = new[] { attacker.Faction };
                owner.Descriptor.SwitchFactions(faction, false);
                owner.GroupId = "KMG_Runtime_Sprint17_Lifecycle_" + owner.UniqueId;
                owner.AttackFactions.Match(new[] { attacker.Faction }); attacker.AttackFactions.Match(new[] { faction });
                if (!Game.Instance.State.AwakeUnits.Contains(attacker)) Game.Instance.State.AwakeUnits.Add(attacker);
                // A native mundane bite must exceed Salamander DR10 to wound.
                // Raise only the owned attacker's live Strength/accuracy, not
                // an attack result, physical reduction or hit animation.
                attacker.Stats.Strength.BaseValue = 36; attacker.Stats.AdditionalAttackBonus.BaseValue = 100;
                attacker.Memory.Add(owner); owner.Memory.Add(attacker);
                attacker.JoinCombat(); owner.JoinCombat();
                new UnitCombatJoinController().Tick(); new UnitCombatPrepareController().Tick(); Game.Instance.Player.UpdateIsInCombat();
                var isolation = Sprint17LifecycleIsolation(fixture, owner, attacker);
                if (!(bool)isolation["passed"]) throw new InvalidOperationException("Salamander lifecycle pair is not isolated: " + isolation);
                owner.ForceLookAt(attacker.Position);
                JObject facing = null;
                for (int frame = 0; frame < 180; frame++)
                { Game.Instance.IsPaused = false; yield return 0; facing = Sprint17NativeHitFacing(owner, attacker, true); if ((bool)facing["eligible"]) break; }
                if (facing == null || !(bool)facing["eligible"]) throw new InvalidOperationException("Native frontal wound fixture did not settle.");
                observer.ObserveWeaponContact = rule => {
                    if (!ReferenceEquals(rule.Target, owner)) return;
                    var wound = Sprint17NativeHitFacing(owner, attacker, rule.AttackRoll.IsHit);
                    wound["damage"] = rule.MeleeDamage == null ? 0 : rule.MeleeDamage.Damage;
                    wounds.Add(wound);
                };
                EventBus.Subscribe(observer); subscribed = true;
                DateTime deadline = DateTime.UtcNow.AddSeconds(25);
                while (DateTime.UtcNow < deadline)
                {
                    Game.Instance.IsPaused = false;
                    if (commands.Count < 4 && (commands.Count == 0 || commands.Last().IsFinished) && !observer.Damage.Any(d => d.Damage > 0))
                    { var attack = new UnitAttack(owner); attack.Init(attacker); attacker.Commands.Run(attack); commands.Add(attack); }
                    yield return 0;
                    ObserveSprint17LifecyclePose(owner, set, "hit", samples, ref hitPlayed, ref deathPlayed, ref dead, ref finite, ref dissolve, resources);
                    if (observer.Damage.Any(d => d.Damage > 0) && (!hasHit || hitPlayed)) break;
                }
                float start = Time.time;
                while (Time.time - start < .5f && DateTime.UtcNow < deadline)
                { Game.Instance.IsPaused = false; yield return 0; ObserveSprint17LifecyclePose(owner, set, "hit", samples,
                    ref hitPlayed, ref deathPlayed, ref dead, ref finite, ref dissolve, resources); }
                InterruptExpandedSummoningFixtureCommands(attacker);
                CheckSprint17Salamander("native-wound-lifecycle", SerpentineFinalReviewPolicy.FaithfulHitLifecycle(
                    attachment.NativeActionsUnchanged, hasHit, hasHit,
                    wounds.OfType<JObject>().Any(w => (bool)w["eligible"] && (int)w["damage"] > 0),
                    finite && Time.time - start >= .5f && Sprint17BodyIntact(owner.View, SalamanderHumanBindingPolicy.BodyName),
                    !owner.Descriptor.State.IsDead, hitPlayed),
                    new JObject { ["hasNativeHit"] = hasHit, ["hitPlayed"] = hitPlayed, ["wounds"] = wounds, ["samples"] = samples.DeepClone() },
                    "actual frontal native wound retains visible finite original geometry and the donor's available hit behavior");
                GameHelper.KillUnit(owner, attacker); Game.Instance.IsPaused = false;
                deadline = DateTime.UtcNow.AddSeconds(8);
                while (DateTime.UtcNow < deadline && owner.View != null)
                { Game.Instance.IsPaused = false; yield return 0; ObserveSprint17LifecyclePose(owner, set, "death", samples,
                    ref hitPlayed, ref deathPlayed, ref dead, ref finite, ref dissolve, resources); }
                CheckSprint17Salamander("native-death", dead && finite && (!hasDeath || deathPlayed),
                    new JObject { ["dead"] = dead, ["nativeDeathAvailable"] = hasDeath, ["played"] = deathPlayed, ["samples"] = samples.DeepClone() },
                    "native death preserves finite original geometry and plays an available donor death carrier; no synthetic animation");
                JObject expiry = null;
                if (!owner.Destroyed)
                {
                    Buff marker = owner.Descriptor.Buffs.Enumerable.Single(b => ReferenceEquals(b.Blueprint, BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff));
                    expiry = ExpireSprint16OwnedBuff(owner, marker);
                }
                deadline = DateTime.UtcNow.AddSeconds(15);
                while (DateTime.UtcNow < deadline)
                {
                    Game.Instance.IsPaused = false; yield return 0;
                    ObserveSprint17LifecyclePose(owner, set, "expiry-fade", samples, ref hitPlayed, ref deathPlayed, ref dead, ref finite, ref dissolve, resources);
                    if (owner.Destroyed && owner.View == null && resources.All(r => r == null)) break;
                }
                CheckSprint17Salamander("native-fade-despawn", finite && dissolve > .05f && owner.Destroyed && owner.View == null && owner.HoldingState == null,
                    new JObject { ["dissolve"] = dissolve, ["expiry"] = expiry, ["destroyed"] = owner.Destroyed, ["samples"] = samples.DeepClone() },
                    "native marker expiry fades the original material and despawns without visibility or material writes");
                CheckSprint17Salamander("lifecycle-resources", resources.Count >= 29 && resources.All(r => r == null),
                    new JObject { ["captured"] = resources.Count, ["remaining"] = resources.Count(r => r != null) }, "native teardown reclaims every captured mesh/material/texture/action/tail/component");
            }
            finally
            {
                if (subscribed) EventBus.Unsubscribe(observer);
                InterruptExpandedSummoningFixtureCommands(attacker); InterruptExpandedSummoningFixtureCommands(owner);
                DisposeExpandedSummoningUnits(fixture.Created, new[] { owner, attacker }); Game.Instance.Player.UpdateIsInCombat();
            }
            yield return 0; yield return 0;
        }
    }
}
