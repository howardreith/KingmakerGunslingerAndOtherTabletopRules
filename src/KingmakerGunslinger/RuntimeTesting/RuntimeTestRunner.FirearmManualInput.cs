using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Kingmaker.TurnBasedMode;
using Kingmaker.UnitLogic;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Firing;
using KingmakerGunslinger.Misfires;
using Kingmaker.UnitLogic.Commands;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private sealed class FirearmSelfPredictionScope : IDisposable
        {
            private readonly PathVisualizer before = PathVisualizer.Instance;
            private readonly PropertyInfo instance = typeof(PathVisualizer).GetProperty("Instance",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            private GameObject root;
            internal FirearmSelfPredictionScope()
            {
                if (!TurnBased.Controllers.CombatController.IsInTurnBasedCombat() || before != null) return;
                root = new GameObject("KMG_Runtime_Firearm_SelfActionPath");
                root.hideFlags = HideFlags.HideAndDontSave;
                root.SetActive(false);
                try
                {
                    var prototype = new GameObject("PathLine");
                    prototype.transform.SetParent(root.transform, false);
                    var line = prototype.AddComponent<LineRenderer>();
                    var visualizer = root.AddComponent<PathVisualizer>();
                    visualizer.enabled = false;
                    foreach (string field in new[] { "m_BreakMoveDecal", "m_BreakStandardDecal" })
                    {
                        var decal = new GameObject(field);
                        decal.transform.SetParent(root.transform, false);
                        typeof(PathVisualizer).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic)
                            .SetValue(visualizer, decal.AddComponent<MeshRenderer>());
                    }
                    typeof(PathVisualizer).GetField("m_MoveLineP", BindingFlags.Instance | BindingFlags.NonPublic)
                        .SetValue(visualizer, line);
                    root.SetActive(true); // Native Awake establishes the service.
                    if (!ReferenceEquals(PathVisualizer.Instance, visualizer) || visualizer.GetCurrentPathLength() != 0)
                        throw new InvalidOperationException("Native personal-action path context was not empty.");
                }
                catch { Dispose(); throw; }
            }
            public void Dispose()
            {
                if (root == null) return;
                UnityEngine.Object.DestroyImmediate(root);
                instance.SetValue(null, before, null);
                root = null;
            }
        }
        private IEnumerable<object> DriveFirearmIdle(FirearmInputFixture f)
        {
            // Native AI uses Unity time for its 0.3-0.4 second retry interval.
            // A fixed number of frames at an uncapped menu frame rate is not
            // evidence that AI has had an opportunity to retry the old order.
            float until = Time.realtimeSinceStartup + 1f;
            for (int tick = 0; tick < 40 || Time.realtimeSinceStartup < until; tick++)
            { f.Pump(); f.Record(tick); yield return null; }
        }
        private IEnumerable<object> FirearmManualRecoveryCases()
        {
            foreach (bool turnBased in new[] { false, true })
            foreach (bool quickClear in new[] { false, true })
            {
                string prefix = (turnBased ? "tb-" : "rtwp-") + (quickClear ? "quick-clear-" : "manual-reload-");
                _firearmInputStage = prefix + "fixture";
                _firearmInputFixture = new FirearmInputFixture(turnBased, _firearmInputRows);
                var f = _firearmInputFixture;
                var gunslinger = BlueprintBootstrap.GunslingerClass;
                if (quickClear)
                {
                    f.Actor.Descriptor.AddFact(gunslinger.Grit.Feature);
                    f.Actor.Descriptor.AddFact(gunslinger.QuickClear.Feature);
                    if (f.Actor.Descriptor.Resources.GetResourceAmount(gunslinger.Grit.Resource) < 1)
                        throw new InvalidOperationException("The disposable Quick Clear owner has no native grit entitlement.");
                }
                FirearmMisfireRuntime.QueueForcedNaturalRoll(1);
                f.Click(f.Enemy);
                for (int tick = 0; tick < 240 && f.Shots.Count == 0; tick++)
                { f.Pump(); f.Record(tick); yield return null; }
                int powder = f.Powder;
                foreach (object step in DriveFirearmIdle(f)) yield return step;
                FirearmInputCheck(prefix + "real-break-old-order-stopped", f.Shots.Count == 1 &&
                    f.State.Condition == FirearmCondition.Broken && f.State.IsEmpty && f.Powder == powder &&
                    BrokenSequenceSuppressionRuntime.IsSuppressed(f.Actor, f.Weapon), f.Describe());
                if (turnBased) { f.Turns.EndCurrentTurn(); f.Turns.ReachCasterTurn(); }
                _firearmInputStage = prefix + "ordinary-action-bar-command";
                int grit = f.Actor.Descriptor.Resources.GetResourceAmount(gunslinger.Grit.Resource);
                if (!f.SameItemIdentity) throw new InvalidOperationException("Native item identity changed.");
                var ability = quickClear ? gunslinger.QuickClear.StandardAbility : BlueprintBootstrap.ReloadTestMusketAbility;
                var command = f.ClickAbility(ability);
                for (int tick = 0; tick < 240 && (quickClear
                    ? f.State.Condition != FirearmCondition.Normal : f.State.IsEmpty); tick++)
                { f.Pump(); f.Record(tick); yield return null; }
                foreach (object step in DriveFirearmIdle(f)) yield return step;
                FirearmInputCheck(prefix + "alone-never-revives-old-order", f.Shots.Count == 1 &&
                    f.State.Condition == (quickClear ? FirearmCondition.Normal : FirearmCondition.Broken) &&
                    f.State.LoadedRounds == (quickClear ? 0 : 1) && f.Powder == powder - (quickClear ? 0 : 1) &&
                    f.SameItemIdentity &&
                    BrokenSequenceSuppressionRuntime.Orders.Current(f.Actor) == null &&
                    BrokenSequenceSuppressionRuntime.IsSuppressed(f.Actor, f.Weapon) && command.IsActed &&
                    !command.Cutscene && !command.IsIgnoreCooldown && f.PeakActionCost(command) > 0 &&
                    grit == f.Actor.Descriptor.Resources.GetResourceAmount(gunslinger.Grit.Resource),
                    f.Describe() + ";action=" + command.Type + ";nativeCooldownPeak=" + f.PeakActionCost(command));
                if (turnBased && quickClear) { f.Turns.EndCurrentTurn(); f.Turns.ReachCasterTurn(); }
                _firearmInputStage = prefix + "new-explicit-attack";
                powder = f.Powder;
                FirearmMisfireRuntime.QueueForcedNaturalRoll(10);
                f.Click(f.Enemy);
                if (!quickClear)
                    FirearmInputCheck(prefix + "loaded-native-attack-submitted", f.Actor.Commands.Raw.OfType<UnitAttack>().Any() &&
                        !f.Actor.Commands.Raw.OfType<UnitUseAbility>().Any(value => !value.IsFinished),
                        "The new ordinary click submitted a loaded Broken firearm attack without reloading.");
                for (int tick = 0; tick < 240 && f.Shots.Count < 2; tick++)
                { f.Pump(); f.Record(tick); yield return null; }
                FirearmInputCheck(prefix + "new-attack-fires", f.Shots.Count == 2 &&
                    f.State.Condition == (quickClear ? FirearmCondition.Normal : FirearmCondition.Broken) &&
                    f.Powder == powder - (quickClear ? 1 : 0) &&
                    f.SameItemIdentity,
                    f.Describe() + ";same exact firearm; non-misfire roll=10");
                f.Dispose();
                FirearmInputCheck(prefix + "cleanup", f.Restored, "Native fixture restored exactly.");
                _firearmInputFixture = null;
            }
        }
    }
}
