using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using Kingmaker.View;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> ReviewIceDevilSlow40(ExpandedSummoningCorrectionFixture fixture)
        {
            var owned = new List<UnitEntityData>(); var trace = new InfernalEvents34();
            var cells = new JArray(); var receipt = new JObject { ["cells"] = cells,
                ["events"] = trace.Rows, ["fixtureFearIsolation"] = true,
                ["phase"] = ExpandedSummoningNativeProfilePolicy.Sprint40IceDevilSlowPhase };
            Action<string, bool, JObject, string> check = (id, pass, row, why) => {
                row["cell"] = id; cells.Add(row); Sprint25Check("sprint40-" + id, pass, row, why);
                if (!pass) throw new InvalidOperationException("Sprint40 Slow: " + id);
            };
            var target = fixture.Hostile;
            int fort = target.Stats.SaveFortitude.BaseValue, ac = target.Stats.AC.BaseValue;
            bool addedCold = false, fearIsolation = false;
            BlueprintFeature resistance = null, cold = null;
            AddDamageResistancePhysical dr = null;
            BlueprintBuff slow = null;
            Buff hastedSetup = null;
            try
            {
                target.Descriptor.State.AddConditionImmunity(UnitCondition.Frightened); fearIsolation = true;
                target.Stats.SaveFortitude.BaseValue = -100;
                var owner = CastExpandedSummoningOwnTier(fixture, IceDevilVisualPolicy40.Key);
                owned.Add(owner); Sprint28SetBrainActivePreservingActions(owner, false);
                foreach (int frame in Outsider30NativeAppearance(owner, owner.Blueprint.name)) yield return frame;
                var live = owner.Descriptor.Progression.Features.Enumerable.SelectMany(x =>
                    x.SelectComponents<IceDevilSlowOnHit40>()).Single();
                slow = live.Slowed;
                check("slow-live-owner-component", ReferenceEquals(live.Owner, owner.Descriptor) &&
                    ReferenceEquals(live.OwningBlueprint, owner.Blueprint),
                    new JObject { ["source"] = owner.UniqueId, ["nativeSlow"] = slow.AssetGuid },
                    "The actual runtime-owned component, not its blueprint prototype, receives the melee events.");
                var hostility = Nalfeshnee39EstablishHostility(fixture, owner, target);
                check("slow-recipient-hostility", owner.IsEnemy(target) && target.IsEnemy(owner) &&
                    !ReferenceEquals(owner.Group, target.Group), hostility, "Actual native mutual hostility and independent recipient group.");
                InfernalCombatLane34(fixture, owner, target, 3);
                float beforeSpeed = target.CombatSpeedMps;
                // Native defender resistance/immunity form the external test
                // boundary. The product still receives a real hit with zero
                // final physical AND frost damage, not a fabricated attack.
                dr = ScriptableObject.CreateInstance<AddDamageResistancePhysical>();
                dr.name = "$KMG_S40_FixturePhysicalResistance";
                dr.Value = new ContextValue { ValueType = ContextValueType.Simple, Value = 1000 };
                dr.BypassedByAlignment = dr.BypassedByMaterial = dr.BypassedByMagic = false;
                resistance = ScriptableObject.CreateInstance<BlueprintFeature>();
                resistance.name = "KMG_Runtime_S40_RecipientPhysicalResistance"; resistance.Ranks = 1;
                resistance.ComponentsArray = new BlueprintComponent[] { dr };
                target.Descriptor.AddFact(resistance);
                cold = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(BlueprintBootstrap.Library,
                    "9ae23798a9284e044ad2716a772a410e", "native cold immunity defender boundary");
                if (!target.Descriptor.HasFact(cold)) { target.Descriptor.AddFact(cold); addedCold = true; }
                owner.Stats.Constitution.BaseValue += 4;
                var capture = new Sprint24AttackObserver { Owner = owner, Target = target };
                EventBus.Subscribe(capture);
                int start = trace.Rows.Count;
                try
                {
                    foreach (int frame in IceDevilSpearUntil40(owner, target,
                        () => target.Buffs.GetBuff(slow) != null)) yield return frame;
                    var attacks = trace.Rows.Skip(start).Where(x => (string)x["event"] == "attack" &&
                        (string)x["owner"] == owner.UniqueId && (bool?)x["hit"] == true).ToArray();
                    var saves = trace.Rows.Skip(start).Where(x => (string)x["event"] == "save" &&
                        (string)x["owner"] == owner.UniqueId && (string)x["target"] == target.UniqueId &&
                        (int?)x["dc"] == 25).ToArray();
                    check("slow-zero-final-damage-cold-immune-live-dc", attacks.Length > 0 &&
                        attacks.All(x => (int?)x["hpDamage"] == 0) && saves.Length == attacks.Length &&
                        saves.Any(x => !(bool)x["passed"]) && target.Descriptor.State.HasCondition(UnitCondition.Slowed),
                        new JObject { ["attacks"] = new JArray(attacks), ["saves"] = new JArray(saves),
                            ["liveConstitution"] = owner.Stats.Constitution.ModifiedValue,
                            ["coldImmuneFact"] = target.Descriptor.HasFact(cold), ["forcedRolls"] = 0 },
                        "Each real spear hit attempts exactly one Fort save at live Con DC25 despite zero final damage and native cold immunity; at least one fails and applies Slow.");
                    var actual = capture.WeaponAttacks.Last(x => x.AttackRoll?.IsHit == true);
                    var deadline = target.Buffs.GetBuff(slow).EndTime;
                    int duplicateStart = trace.Rows.Count;
                    live.OnEventDidTrigger(actual); live.OnEventDidTrigger(actual);
                    check("slow-duplicate-notification-idempotent", trace.Rows.Count == duplicateStart &&
                        target.Buffs.GetBuff(slow)?.EndTime == deadline,
                        new JObject { ["diagnosticReplayOfRealEvent"] = true,
                            ["additionalRules"] = trace.Rows.Count - duplicateStart,
                            ["additionalAttacks"] = 0, ["deadline"] = deadline.ToString() },
                        "Diagnostic duplicate delivery of the same captured real event emits no new save, application or duration refresh; this is not an extra native attack.");
                }
                finally { EventBus.Unsubscribe(capture); }
                float slowedSpeed = target.CombatSpeedMps;
                // Installed CalculateSpeedModifier halves for Staggered and
                // Slowed separately. Native Slow carries both, so this getter
                // reports a quarter of the unrestricted combat rate. Preserve
                // that native condition/action consumer; do not compensate it.
                check("slow-native-movement-speed", beforeSpeed > 0 && slowedSpeed > 0 &&
                    target.Descriptor.State.HasCondition(UnitCondition.Staggered) &&
                    target.Descriptor.State.HasCondition(UnitCondition.Slowed) &&
                    Math.Abs(slowedSpeed - beforeSpeed * .25f) < .01f,
                    new JObject { ["beforeCombatMps"] = beforeSpeed, ["slowedCombatMps"] = slowedSpeed,
                        ["nativeSpeed"] = Sprint16Speed(target) },
                    "Installed native Slow carries Slowed and Staggered; each halves CalculateSpeedModifier, yielding the observed quarter combat rate. Base30 feet is unchanged; actual directed movement follows.");
                Vector3 position = target.Position;
                Vector3 away = (target.Position - owner.Position).normalized;
                var destination = ObstacleAnalyzer.GetNearestNode(position + away).clampedPosition;
                var move = new UnitMoveTo(destination, .1f); int moveStart = trace.Rows.Count;
                target.Commands.Run(move);
                float velocity = 0; bool slowedWhileMoving = false; int movementFrames = 0;
                DateTime movementDeadline = DateTime.UtcNow.AddSeconds(15);
                while (!move.IsFinished && DateTime.UtcNow < movementDeadline)
                {
                    float current = target.View.MovementAgent.Velocity.magnitude;
                    velocity = Math.Max(velocity, current); movementFrames++;
                    if (current > .01f && target.Descriptor.State.HasCondition(UnitCondition.Slowed)) slowedWhileMoving = true;
                    Game.Instance.IsPaused = false; yield return 0;
                }
                float gap = Vector3.Distance(destination, target.Position);
                var moveEnds = trace.Rows.Skip(moveStart).Where(x => (string)x["event"] == "command-end" &&
                    (int?)x["commandReference"] == RuntimeHelpers.GetHashCode(move)).ToArray();
                bool nativeArrivalInterrupt = move.Result == UnitCommand.ResultType.Interrupt &&
                    moveEnds.Any(x => ((string)x["interruptionCaller"] ?? "").Contains("UnitMovementAgent.CompleteMovement"));
                check("slow-recipient-real-move", move.IsFinished &&
                    (move.Result == UnitCommand.ResultType.Success || nativeArrivalInterrupt) &&
                    Vector3.Distance(position, target.Position) > .3f && gap <= .2f &&
                    velocity > .01f && slowedWhileMoving,
                    new JObject { ["travel"] = Vector3.Distance(position, target.Position),
                        ["goalGap"] = gap, ["maxNativeVelocity"] = velocity, ["movementFrames"] = movementFrames,
                        ["slowedWhileMoving"] = slowedWhileMoving, ["nativeEnds"] = new JArray(moveEnds),
                        ["command"] = Sprint27AiCommand(move), ["observedSlowedCombatMps"] = slowedSpeed },
                    "The affected disposable recipient reaches the directed goal with actual native velocity while slowed. Native arrival/CompleteMovement interruption is recorded; unrelated interruption cannot pass.");
                owner.Commands.InterruptAll(true); owner.Commands.RemoveFinishedAndUpdateQueue();
                InfernalCombatLane34(fixture, owner, target, 3);
                target.Stats.AC.BaseValue = 100;
                bool miss = false; var missRows = new JArray();
                for (int attempt = 0; attempt < 8 && !miss; attempt++)
                {
                    start = trace.Rows.Count; var before = target.Buffs.GetBuff(slow)?.EndTime;
                    var attack = new UnitAttack(target) { IsSingleAttack = true, ForceFullAttack = false };
                    owner.Commands.Run(attack);
                    foreach (int frame in WaitInfernalCommand34(attack, () => true, 20)) yield return frame;
                    var rows = trace.Rows.Skip(start).ToArray();
                    missRows.Add(new JArray(rows));
                    miss = rows.Any(x => (string)x["event"] == "attack" &&
                        (string)x["owner"] == owner.UniqueId && (bool?)x["hit"] == false);
                    if (miss)
                    {
                        var after = target.Buffs.GetBuff(slow)?.EndTime;
                        check("slow-real-miss-no-attempt", !rows.Any(x => (string)x["event"] == "save" &&
                            (string)x["owner"] == owner.UniqueId && (int?)x["dc"] == 25) &&
                            (!after.HasValue || before.HasValue && after.Value <= before.Value),
                            new JObject { ["attempts"] = missRows, ["forcedRolls"] = 0 },
                            "An observed native miss emits no Fort attempt and cannot create or extend the finite condition; natural20 hits are retained as separate attempts.");
                    }
                }
                if (!miss) throw new InvalidOperationException("Unobserved stochastic precondition: no real miss in8 attempts.");
                target.Stats.AC.BaseValue = ac;
                // Haste is a native external effect setup. The product's own
                // failed-save branch must perform the actual countering.
                target.Buffs.GetBuff(slow)?.Remove();
                var haste = IceDevilNativeHaste40();
                var context = new MechanicsContext(target, target.Descriptor, haste, null, new TargetWrapper(target));
                context.Params.CasterLevel = 13; context.Params.SpellLevel = 3;
                hastedSetup = target.Buffs.AddBuff(haste, context, TimeSpan.FromSeconds(78));
                var hasteDeadline = hastedSetup.EndTime; start = trace.Rows.Count;
                foreach (int frame in IceDevilSpearUntil40(owner, target,
                    () => target.Buffs.GetBuff(haste) == null)) yield return frame;
                check("slow-native-haste-counter", target.Buffs.GetBuff(haste) == null &&
                    target.Buffs.GetBuff(slow) == null &&
                    hasteDeadline > Game.Instance.TimeController.GameTime &&
                    trace.Rows.Skip(start).Any(x => (string)x["event"] == "save" &&
                        (string)x["owner"] == owner.UniqueId && (int?)x["dc"] == 25 && !(bool)x["passed"]),
                    new JObject { ["events"] = new JArray(trace.Rows.Skip(start)),
                        ["nativeHasteSetupNotCastProof"] = true, ["unexpiredHasteDeadline"] = hasteDeadline.ToString() },
                    "A real failed Fort save invokes the native branch and counters unexpired Haste without adding Slow; the observer does not remove Haste for the outcome.");

                // Exercise the native buff-immunity consumer using the actual
                // retained condition's descriptor, not a special product exemption.
                var descriptor = ScriptableObject.CreateInstance<BuffDescriptorImmunity>();
                descriptor.Descriptor = slow.GetComponent<SpellDescriptorComponent>().Descriptor;
                var immunity = ScriptableObject.CreateInstance<BlueprintFeature>();
                immunity.name = "KMG_Runtime_S40_SlowDescriptorImmunity"; immunity.Ranks = 1;
                immunity.ComponentsArray = new BlueprintComponent[] { descriptor };
                try
                {
                    target.Descriptor.AddFact(immunity); start = trace.Rows.Count;
                    foreach (int frame in IceDevilSpearUntil40(owner, target, () =>
                        trace.Rows.Skip(start).Any(x => (string)x["event"] == "save" &&
                            (string)x["owner"] == owner.UniqueId && (int?)x["dc"] == 25 &&
                            !(bool)x["passed"]))) yield return frame;
                    check("slow-native-descriptor-immunity", target.Buffs.GetBuff(slow) == null &&
                        !target.Descriptor.State.HasCondition(UnitCondition.Slowed) &&
                        Math.Abs(target.CombatSpeedMps - beforeSpeed) < .01f,
                        new JObject { ["events"] = new JArray(trace.Rows.Skip(start)),
                            ["descriptor"] = descriptor.Descriptor.ToString(),
                            ["combatMps"] = target.CombatSpeedMps },
                        "A real hit and failed Fort save reach native descriptor immunity; no Slow or impaired movement bypasses that defender boundary.");
                }
                finally
                {
                    target.Descriptor.RemoveFact(immunity);
                    UnityEngine.Object.Destroy(immunity); UnityEngine.Object.Destroy(descriptor);
                }

                var second = CastExpandedSummoningOwnTier(fixture, IceDevilVisualPolicy40.Key);
                owned.Add(second); Sprint28SetBrainActivePreservingActions(second, false);
                foreach (int frame in Outsider30NativeAppearance(second, second.Blueprint.name)) yield return frame;
                var secondTarget = Game.Instance.EntityCreator.SpawnUnit(
                    BlueprintRoot.Instance.DefaultPlayerCharacter, target.Position,
                    Quaternion.identity, fixture.Caster.HoldingState);
                fixture.Created.Add(secondTarget); owned.Add(secondTarget);
                Game.Instance.EntityCreator.Tick();
                secondTarget.GroupId = secondTarget.UniqueId;
                Sprint28SetBrainActivePreservingActions(secondTarget, false);
                secondTarget.Stats.HitPoints.BaseValue = 100000;
                secondTarget.Stats.SaveFortitude.BaseValue = -100;
                secondTarget.Descriptor.State.AddConditionImmunity(UnitCondition.Frightened);
                try
                {
                    var secondHostility = Nalfeshnee39EstablishHostility(fixture, second, secondTarget);
                    check("slow-second-recipient-hostility", second.IsEnemy(secondTarget) &&
                        secondTarget.IsEnemy(second) && !ReferenceEquals(second.Group, secondTarget.Group),
                        secondHostility, "A separate disposable recipient has its own native group and actual mutual hostility.");
                    InfernalCombatLane34(fixture, owner, target, 3);
                    foreach (int frame in IceDevilSpearUntil40(owner, target, () =>
                        target.Buffs.GetBuff(slow)?.TimeLeft.TotalSeconds > 8)) yield return frame;
                    InfernalCombatLane34(fixture, second, secondTarget, 3);
                    foreach (int frame in IceDevilSpearUntil40(second, secondTarget, () =>
                        secondTarget.Buffs.GetBuff(slow)?.TimeLeft.TotalSeconds > 8)) yield return frame;
                    var firstBuff = target.Buffs.GetBuff(slow);
                    var secondBuff = secondTarget.Buffs.GetBuff(slow);
                    var firstEnd = firstBuff.EndTime; var secondEnd = secondBuff.EndTime;
                    check("slow-independent-native-owners", firstBuff.IsTurnedOn && secondBuff.IsTurnedOn &&
                        ReferenceEquals(firstBuff.Context.MaybeCaster, owner) &&
                        ReferenceEquals(secondBuff.Context.MaybeCaster, second) &&
                        firstEnd > Game.Instance.TimeController.GameTime &&
                        secondEnd > Game.Instance.TimeController.GameTime,
                        new JObject { ["firstSource"] = owner.UniqueId, ["secondSource"] = second.UniqueId,
                            ["firstDeadline"] = firstEnd.ToString(), ["secondDeadline"] = secondEnd.ToString(),
                            ["secondDc"] = IceDevilRules40Policy.SlowDc(second.Stats.Constitution.Bonus),
                            ["events"] = trace.Rows },
                        "Two real owners apply finite native Slow to independent recipients, with exact source contexts and native rolled deadlines.");
                    DisposeExpandedSummoningUnits(fixture.Created, new[] { owner });
                    check("slow-survives-source-removal-finitely", !firstBuff.IsDisposed &&
                        firstBuff.EndTime == firstEnd && !secondBuff.IsDisposed &&
                        secondBuff.EndTime == secondEnd && !second.Destroyed,
                        new JObject { ["firstSourceDestroyed"] = owner.Destroyed,
                            ["firstDeadlineUnchanged"] = firstBuff.EndTime == firstEnd,
                            ["secondDeadlineUnchanged"] = secondBuff.EndTime == secondEnd },
                        "Removing the first source does not cure or refresh its finite recipient condition and does not alter the other owner's condition.");
                    foreach (int frame in InfernalBoneWaitUntil35(() =>
                        target.Buffs.GetBuff(slow) == null && secondTarget.Buffs.GetBuff(slow) == null, 45))
                    { Game.Instance.IsPaused = false; yield return frame; }
                    check("slow-native-finite-expiry", firstBuff.IsDisposed && secondBuff.IsDisposed &&
                        !target.Descriptor.State.HasCondition(UnitCondition.Slowed) &&
                        !secondTarget.Descriptor.State.HasCondition(UnitCondition.Slowed) &&
                        Game.Instance.TimeController.GameTime >= firstEnd &&
                        Game.Instance.TimeController.GameTime >= secondEnd &&
                        Math.Abs(target.CombatSpeedMps - beforeSpeed) < .01f,
                        new JObject { ["now"] = Game.Instance.TimeController.GameTime.ToString(),
                            ["firstDeadline"] = firstEnd.ToString(), ["secondDeadline"] = secondEnd.ToString(),
                            ["combatMpsRestored"] = target.CombatSpeedMps },
                        "Native timer dispatch expires both rolled conditions and restores movement; the observer never removes either condition for this outcome.");
                }
                finally { secondTarget.Descriptor.State.RemoveConditionImmunity(UnitCondition.Frightened); }
            }
            finally
            {
                if (slow != null) target.Buffs.GetBuff(slow)?.Remove(); // fixture isolation only; no expiry proof here
                hastedSetup?.Remove();
                if (resistance != null) target.Descriptor.RemoveFact(resistance);
                if (addedCold) target.Descriptor.RemoveFact(cold);
                if (fearIsolation) target.Descriptor.State.RemoveConditionImmunity(UnitCondition.Frightened);
                target.Stats.SaveFortitude.BaseValue = fort; target.Stats.AC.BaseValue = ac;
                trace.Dispose(); DisposeExpandedSummoningUnits(fixture.Created, owned);
                if (resistance != null) UnityEngine.Object.Destroy(resistance);
                if (dr != null) UnityEngine.Object.Destroy(dr);
                ResetExpandedSummoningHostile(fixture);
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory,
                    "sprint40-ice-devil-slow.json"), receipt.ToString(Formatting.Indented));
            }
        }

        private static BlueprintBuff IceDevilNativeHaste40()
        {
            // Retained native identity already used by the installed-contract
            // observers. Haste is an external fixture effect, not a new SLA.
            return BlueprintLibraryLookup.RequireExact<BlueprintBuff>(BlueprintBootstrap.Library,
                "03464790f40c3c24aa684b57155f3280", "native Haste defender boundary");
        }

        private IEnumerable<int> IceDevilSpearUntil40(UnitEntityData owner, UnitEntityData target,
            Func<bool> witnessed)
        {
            for (int attempt = 0; attempt < 8 && !witnessed(); attempt++)
            {
                var command = new UnitAttack(target) { IsSingleAttack = true, ForceFullAttack = false };
                owner.Commands.Run(command);
                foreach (int frame in WaitInfernalCommand34(command, () => true, 20)) yield return frame;
            }
            if (!witnessed()) throw new InvalidOperationException(
                "Unobserved stochastic precondition: native spear hits/saves did not yield the required outcome in8 attempts.");
        }
    }
}
