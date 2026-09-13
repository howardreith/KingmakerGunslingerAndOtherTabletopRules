using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Firing;
using KingmakerGunslinger.Misfires;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Native hover prediction computes an actual path, even for a self
        // reload. Supply the empty host's two native graph slots; no action
        // state, path result, command or target resolution is overridden.
        private sealed class FirearmPredictionNavigationScope : IDisposable
        {
            private GameObject root;
            private Mesh plane;
            private HarmonyInstance harmony;
            private static DateTime? pathDeadline;
            private static string lastPathPhase, pathTrace;
            private static int traceCount;
            private static void TracePath(string value)
            {
                lastPathPhase = value;
                if (traceCount++ < 100) System.IO.File.AppendAllText(pathTrace, value + Environment.NewLine);
            }
            private static void PathProgressStatic(MethodBase __originalMethod)
            { PathProgress(__originalMethod, null); }
            private static void PathReturned(MethodBase __originalMethod)
            { if (pathDeadline.HasValue) TracePath("leave " + __originalMethod.DeclaringType.FullName + "." + __originalMethod.Name); }
            private readonly string patchId = "KMG.FirearmPredictionBudget." + Guid.NewGuid().ToString("N");
            private readonly List<MethodInfo> observed = new List<MethodInfo>();
            private int waitDepthBefore;
            internal static void BeginPrediction()
            { pathDeadline = DateTime.UtcNow.AddSeconds(3); lastPathPhase = "native prediction entered"; }
            internal static void EndPrediction() { pathDeadline = null; }
            internal static void RecordPredictionFailure(Exception error)
            { System.IO.File.WriteAllText(pathTrace + ".failure.txt", error.ToString()); }
            private static void PathProgress(MethodBase __originalMethod, object __instance)
            {
                if (!pathDeadline.HasValue) return;
                if (DateTime.UtcNow > pathDeadline.Value)
                    throw new InvalidOperationException("Native prediction path exceeded its observation budget; previous=" +
                        lastPathPhase + ";current=" + __originalMethod.DeclaringType.FullName + "." + __originalMethod.Name);
                TracePath("enter " + __originalMethod.DeclaringType.FullName + "." + __originalMethod.Name +
                    ";receiver=" + (__instance == null ? "none" : __instance.GetType().FullName));
            }
            internal FirearmPredictionNavigationScope(string evidenceDirectory, string label)
            {
                if (AstarPath.active != null || Game.Instance.CurrentScene == null ||
                    Game.Instance.CurrentScene.Area == null || Game.Instance.CurrentScene.Area.InteractiveObjectGrid == null)
                    throw new InvalidOperationException("Prediction navigation requires the disposable empty host.");
                try
                {
                    pathTrace = System.IO.Path.Combine(evidenceDirectory, "firearm-prediction-path-" + label + ".txt"); traceCount = 0;
                    waitDepthBefore = (int)typeof(AstarPath).GetField("waitForPathDepth", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
                    // Observe native path progress with a fail-closed deadline.
                    // No path result is supplied or altered on successful runs.
                    harmony = HarmonyInstance.Create(patchId);
                    var points = new[] {
                        new { Type = typeof(Pathfinding.Path), Names = new[] { "GetState", "Trace" } },
                        new { Type = typeof(Pathfinding.ABPath), Names = new[] { "Prepare", "Initialize", "CalculateStep", "CompletePathIfStartIsValidTarget" } },
                        new { Type = typeof(Pathfinding.XPath), Names = new[] { "Prepare", "Initialize", "CalculateStep", "CompletePathIfStartIsValidTarget" } },
                        new { Type = typeof(Kingmaker.TurnBasedMode.Pathfinding.ABPathWithClearance), Names = new[] { "Prepare", "CanTraverse" } },
                        new { Type = typeof(Kingmaker.TurnBasedMode.Pathfinding.ABPathWithClearance).GetNestedType("LosPathConditionForGrid", BindingFlags.Public | BindingFlags.NonPublic), Names = new[] { "TargetFound" } },
                        new { Type = typeof(AstarPath), Names = new[] { "PerformBlockingActions", "WaitForPath", "GetNearest" } },
                        new { Type = typeof(Pathfinding.NavGraph), Names = new[] { "GetNearest", "GetNearestForce" } },
                        new { Type = typeof(Pathfinding.GridGraph), Names = new[] { "GetNearest", "GetNearestForce", "GetNodesInArea", "GetBoundsMinMax" } },
                        new { Type = typeof(Seeker), Names = new[] { "StartPath" } },
                        new { Type = typeof(Kingmaker.View.UnitMovementAgent), Names = new[] { "FindPath" } },
                        new { Type = typeof(Kingmaker.TurnBasedMode.PathVisualizer), Names = new[] { "CalculatePathForCommand" } }
                    };
                    foreach (var method in points.SelectMany(point => point.Type.GetMethods(
                        BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                        .Where(method => !method.IsAbstract && point.Names.Contains(method.Name))).Distinct())
                    {
                        harmony.Patch(method, new HarmonyMethod(typeof(FirearmPredictionNavigationScope)
                            .GetMethod(method.IsStatic ? "PathProgressStatic" : "PathProgress", BindingFlags.Static | BindingFlags.NonPublic)),
                            new HarmonyMethod(typeof(FirearmPredictionNavigationScope).GetMethod("PathReturned", BindingFlags.Static | BindingFlags.NonPublic)));
                        observed.Add(method);
                    }
                    root = new GameObject("KMG_Runtime_FirearmPredictionNavigation");
                    root.SetActive(false);
                    var path = root.AddComponent<AstarPath>();
                    path.threadCount = ThreadCount.None;
                    path.astarData = new Pathfinding.AstarData();
                    root.SetActive(true);
                    plane = new Mesh { name = "KMG_Runtime_FirearmPredictionPlane" };
                    plane.vertices = new[] { new Vector3(-16, 0, -16), new Vector3(-16, 0, 16),
                        new Vector3(16, 0, 16), new Vector3(16, 0, -16) };
                    plane.triangles = new[] { 0, 1, 2, 0, 2, 3 }; plane.RecalculateBounds();
                    var navigation = (Pathfinding.RecastGraph)path.astarData.AddGraph(typeof(Pathfinding.RecastGraph));
                    navigation.forcedBoundsCenter = Vector3.zero;
                    navigation.forcedBoundsSize = new Vector3(32, 4, 32);
                    navigation.cellSize = 1; navigation.useTiles = false;
                    navigation.scanEmptyGraph = true;
                    navigation.rasterizeMeshes = navigation.rasterizeColliders =
                        navigation.rasterizeTerrain = navigation.rasterizeTrees = false;
                    var grid = (Pathfinding.GridGraph)path.astarData.AddGraph(typeof(Pathfinding.GridGraph));
                    grid.center = Vector3.zero; grid.nodeSize = 1;
                    grid.width = grid.depth = 32; grid.UpdateSizeFromWidthDepth();
                    grid.collision.collisionCheck = grid.collision.heightCheck = false;
                    grid.erodeIterations = 2; grid.erosionUseTags = true;
                    path.Scan();
                    // Kingmaker's TB clearance path specifically reads graph0
                    // as RecastGraph. Install the owned plane through its native
                    // tile API, then compute graph connectivity normally.
                    path.AddWorkItem(new AstarPath.AstarWorkItem(force => {
                        navigation.ReplaceTile(0, 0, plane.vertices.Select(point => (Pathfinding.Int3)point).ToArray(),
                            plane.triangles, true);
                        // Kingmaker's grid scan projects onto graph0. Re-scan
                        // after that owned surface exists, before asking it
                        // for a walkable path (never assign node walkability).
                        grid.ScanGraph();
                        path.QueueWorkItemFloodFill();
                        return true;
                    }));
                    path.FlushWorkItems(true, true);
                    if (!ReferenceEquals(AstarPath.active, path) || path.graphs.Length != 2 ||
                        navigation.GetTiles().Length != 1 || navigation.GetTiles()[0].nodes.Length != 2 ||
                        !navigation.GetTiles()[0].nodes.All(node => node.Walkable) ||
                        path.GetNearest(Vector3.zero, new Pathfinding.NNConstraint { graphMask = 2 }).node == null)
                        throw new InvalidOperationException("Native prediction navigation was not scanned.");
                }
                catch { Dispose(); throw; }
            }
            public void Dispose()
            {
                EndPrediction();
                if (harmony != null)
                    foreach (var method in observed) harmony.Unpatch(method, HarmonyPatchType.All, patchId);
                if (root != null) { UnityEngine.Object.DestroyImmediate(root); root = null; }
                // Native WaitForPath lacks a finally for its recursion counter.
                // Restore the owned observation scope after a deadline exception.
                typeof(AstarPath).GetField("waitForPathDepth", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, waitDepthBefore);
                if (plane != null) { UnityEngine.Object.DestroyImmediate(plane); plane = null; }
                if (AstarPath.active != null) throw new InvalidOperationException("Prediction navigation did not restore the empty host.");
            }
        }
        private static int FirearmScheduledCallbacks()
        {
            return ((System.Collections.ICollection)typeof(Game).GetField("m_BeforeTickActions",
                BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Game.Instance)).Count;
        }
        private static void FirearmPredictHoveredTarget(FirearmInputFixture f)
        {
            f.Turns.Execute(() => {
                var pointer = Game.Instance.DefaultPointerController;
                var type = typeof(Kingmaker.Controllers.Clicks.PointerController);
                // Supply the request-owned view/point at the native pointer
                // input boundary. Handler selection and prediction remain
                // native; never assign an action state or accepted order.
                var fields = new[] { "<Mode>k__BackingField", "<PointerOn>k__BackingField",
                    "<WorldPosition>k__BackingField", "m_WorldPositionForSimulation",
                    "m_MouseDownHandler", "m_SimulateClickHandler" }
                    .Select(name => type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)).ToArray();
                var before = fields.Select(field => field.GetValue(pointer)).ToArray();
                var mainBefore = Game.Instance.Player.MainCharacter;
                try
                {
                    // GetPriority consults the player main character, absent in the
                    // empty host. Bind the owned actor only for this input scope.
                    Game.Instance.Player.MainCharacter = f.Actor;
                    fields[0].SetValue(pointer, Enum.ToObject(fields[0].FieldType, 0));
                    fields[1].SetValue(pointer, f.Enemy.View.gameObject);
                    fields[2].SetValue(pointer, f.Enemy.Position);
                    fields[3].SetValue(pointer, f.Enemy.Position);
                    pointer.UpdateSelectedClickHandler();
                    if (!(fields[5].GetValue(pointer) is Kingmaker.Controllers.Clicks.Handlers.ClickUnitHandler))
                        throw new InvalidOperationException("Native pointer did not select the actual unit click handler; selected=" +
                            (fields[5].GetValue(pointer)?.GetType().FullName ?? "null") + ";priority=" +
                            new Kingmaker.Controllers.Clicks.Handlers.ClickUnitHandler().GetPriority(f.Enemy.View.gameObject, f.Enemy.Position) +
                            ";visible=" + f.Enemy.IsVisibleForPlayer + ";mode=" + pointer.Mode);
                    FirearmPredictionNavigationScope.BeginPrediction();
                    var turn = Game.Instance.TurnBasedCombatController.CurrentTurn;
                    turn.OnHoverObjectChanged(null, f.Enemy.View.gameObject);
                    typeof(TurnBased.Controllers.TurnController).GetMethod("UpdateActionPredictions",
                        BindingFlags.Instance | BindingFlags.NonPublic).Invoke(turn, null);
                }
                catch (Exception error) {
                    FirearmPredictionNavigationScope.RecordPredictionFailure(error);
                    throw;
                }
                finally {
                    FirearmPredictionNavigationScope.EndPrediction();
                    Game.Instance.Player.MainCharacter = mainBefore;
                    for (int i = 0; i < fields.Length; i++) fields[i].SetValue(pointer, before[i]);
                }
            });
        }
        private IEnumerable<object> FirearmReviewReloadTurnCases()
        {
            foreach (string cost in new[] { "move", "standard", "full-round" })
            foreach (string boundary in cost == "move" ? new[] { "same-turn", "after-turn" } :
                new[] { "same-turn", "after-turn", "cancel-waiting", "replace-waiting" })
            {
                bool afterBoundary = boundary == "after-turn";
                string prefix = "cr03-" + cost + "-" + boundary + "-";
                _firearmInputStage = prefix + "fixture";
                _firearmInputFixture = new FirearmInputFixture(true, _firearmInputRows, true,
                    cost == "full-round" ? FirearmKind.Musket : FirearmKind.Pistol, cost == "move");
                var f = _firearmInputFixture;
                using (var prediction = new FirearmSelfPredictionScope())
                using (var costs = new FirearmReviewCosts(f.Actor, _firearmInputRows))
                {
                    foreach (object step in FirearmReviewBreak(f, prefix)) yield return step;
                    f.Turns.EndCurrentTurn(); f.Turns.ReachCasterTurn();
                    var turn = Game.Instance.TurnBasedCombatController.CurrentTurn;
                    int powder = f.Powder;
                    FirearmMisfireRuntime.QueueForcedNaturalRoll(10);
                    f.Click(f.Enemy);
                    var reload = f.Actor.Commands.Raw.OfType<UnitUseAbility>().Single();
                    var order = BrokenSequenceSuppressionRuntime.Orders.Current(f.Actor);
                    FirearmInputCheck(prefix + "genuine-reload-order", order != null &&
                        ReferenceEquals(order.Owner, reload) &&
                        reload.Type == (cost == "move" ? UnitCommand.CommandType.Move : UnitCommand.CommandType.Standard) &&
                        TurnBased.Utility.UnitCommandExtensions.IsFullRoundAction(reload) == (cost == "full-round"),
                        "Native input used legitimate firearm/feat configuration; reload type=" + reload.Type +
                        ";fullRound=" + TurnBased.Utility.UnitCommandExtensions.IsFullRoundAction(reload));
                    for (int tick = 0; tick < 240 && !reload.IsFinished; tick++)
                    { f.Pump(); f.Record(tick); yield return null; }
                    bool standardAvailable = turn.ActionsStates.Standard.CanUse;
                    FirearmInputCheck(prefix + "reload-cost-and-delivery", reload.Result == UnitCommand.ResultType.Success &&
                        f.State.LoadedRounds == 1 && f.State.Condition == FirearmCondition.Broken &&
                        f.Powder == powder - 1 && f.Shots.Count == 1 && costs.Exact(reload, true) &&
                        FirearmScheduledCallbacks() == 1 && ReferenceEquals(turn, Game.Instance.TurnBasedCombatController.CurrentTurn) &&
                        Math.Abs(f.Actor.CombatState.Cooldown.StandardAction - (cost == "move" ? 0 : 6)) < 0.0001f,
                        f.Describe() + ";native reload charged exactly once; standardAvailable=" + standardAvailable +
                        ";callbackCount=" + FirearmScheduledCallbacks() + ";turnStatus=" + turn.Status);
                    if (afterBoundary) f.Turns.EndCurrentTurn();
                    f.Turns.FlushFirearmCallbacks();
                    var queued = f.Actor.Commands.Raw.Concat(f.Actor.Commands.Queue).OfType<UnitAttack>().Distinct().SingleOrDefault();
                    _firearmInputRows.Add(new JObject { ["review"] = "CR-03", ["case"] = prefix,
                        ["turnStatus"] = turn.Status.ToString(), ["standardAvailable"] = standardAvailable,
                        ["nativeStandardCooldown"] = f.Actor.CombatState.Cooldown.StandardAction,
                        ["orderRetained"] = BrokenSequenceSuppressionRuntime.Orders.IsCurrent(order),
                        ["queuedAttack"] = queued != null, ["shots"] = f.Shots.Count,
                        ["callbacks"] = FirearmScheduledCallbacks(), ["reloadCharges"] = costs.Count(reload) });
                    if (afterBoundary)
                    {
                        FirearmInputCheck(prefix + "native-end-cancels", !BrokenSequenceSuppressionRuntime.Orders.IsCurrent(order) &&
                            queued == null && FirearmScheduledCallbacks() == 0 && f.Shots.Count == 1,
                            "Actual native End -> InterruptAll cancelled ownership before the scheduled callback.");
                        f.Turns.ReachCasterTurn();
                        foreach (object step in DriveFirearmIdle(f)) yield return step;
                        FirearmInputCheck(prefix + "no-cross-turn-revival", f.Shots.Count == 1 &&
                            f.State.LoadedRounds == 1 && f.Powder == powder - 1 && costs.Count(reload) == 1,
                            f.Describe() + ";no callback/action/resource duplication");
                    }
                    else if (cost == "move")
                    {
                        for (int tick = 0; tick < 240 && f.Shots.Count == 1; tick++)
                        { f.Pump(); f.Record(tick); yield return null; }
                        FirearmInputCheck(prefix + "one-click-continues", f.Shots.Count == 2 && f.State.IsEmpty &&
                            f.State.Condition == FirearmCondition.Broken && f.Powder == powder - 1 &&
                            costs.Exact(f.LastShotCommand, true) && ReferenceEquals(turn, Game.Instance.TurnBasedCombatController.CurrentTurn),
                            f.Describe() + ";same turn, one Move plus one Standard, no second click");
                    }
                    else
                    {
                        // Standard/FullRound consumed this turn's Standard. A
                        // native queued command may wait, but must never fire or
                        // receive a refund. Native turn end cancels ordinary
                        // command queues, not a new cross-turn feature.
                        FirearmInputCheck(prefix + "unavailable-action-keeps-pending", queued != null &&
                            BrokenSequenceSuppressionRuntime.Orders.IsCurrent(order) &&
                            ReferenceEquals(NativeFirearmAttackOrder.Get(queued)?.Order, order) && !queued.IsActed &&
                            FirearmScheduledCallbacks() == 0,
                            "One native pending attack retains authority while Standard is unavailable. " + f.Describe());
                        if (boundary == "cancel-waiting")
                        {
                            f.Turns.Execute(() => f.Actor.Commands.InterruptAll(true));
                            f.Turns.FlushFirearmCallbacks();
                            FirearmInputCheck(prefix + "explicit-cancel", !BrokenSequenceSuppressionRuntime.Orders.IsCurrent(order) &&
                                !f.Actor.Commands.Raw.Contains(queued) && !f.Actor.Commands.Queue.Contains(queued) &&
                                f.Shots.Count == 1 && f.State.LoadedRounds == 1 && costs.Count(queued) == 0 &&
                                costs.Count(reload) == 1 && FirearmScheduledCallbacks() == 0,
                                "Native cancellation removes pending ownership without refunding the spent reload.");
                        }
                        else if (boundary == "replace-waiting")
                        {
                            // Same-enemy explicit replacement is a new intent and keeps
                            // this turn-boundary fixture within its two owned actors.
                            var replacementTarget = f.Enemy;
                            f.Click(replacementTarget);
                            var replacement = f.Actor.Commands.Raw.Concat(f.Actor.Commands.Queue).OfType<UnitAttack>().Distinct().SingleOrDefault();
                            var nextOrder = BrokenSequenceSuppressionRuntime.Orders.Current(f.Actor);
                            FirearmInputCheck(prefix + "explicit-replacement", order.Cancelled && nextOrder != null &&
                                !ReferenceEquals(order, nextOrder) && replacement != null &&
                                ReferenceEquals(nextOrder.Owner, replacement) && ReferenceEquals(replacement.Target, replacementTarget) &&
                                ReferenceEquals(NativeFirearmAttackOrder.Get(replacement)?.Order, nextOrder),
                                "A fresh ordinary player click replaces the native pending command and owns its exact target.");
                            for (int tick = 0; tick < 8; tick++) { f.Pump(); yield return null; }
                            FirearmInputCheck(prefix + "replacement-cannot-borrow-action", f.Shots.Count == 1 &&
                                f.State.LoadedRounds == 1 && f.Powder == powder - 1 && costs.Count(replacement) == 0 &&
                                costs.Count(reload) == 1 && FirearmScheduledCallbacks() == 0,
                                "Native action rejection spends nothing; no immediate shot, repeated reload or callback.");
                        }
                        for (int tick = 0; tick < 12; tick++) { f.Pump(); yield return null; }
                        FirearmInputCheck(prefix + "no-premature-shot", f.Shots.Count == 1 && costs.Count(queued) == 0 &&
                            costs.Count(reload) == 1 && f.Powder == powder - 1 && f.State.LoadedRounds == 1 &&
                            FirearmScheduledCallbacks() == 0, "Native action gates block the pending attack; no retry callback or second reload.");
                        f.Turns.EndCurrentTurn(); f.Turns.ReachCasterTurn();
                        foreach (object step in DriveFirearmIdle(f)) yield return step;
                        FirearmInputCheck(prefix + "native-turn-end-limit", !BrokenSequenceSuppressionRuntime.Orders.IsCurrent(order) &&
                            f.Shots.Count == 1 && f.Powder == powder - 1 && f.State.LoadedRounds == 1,
                            "Native ordinary queue ends at turn end; no automatic next-turn shot is promised.");
                    }
                    if (cost != "move" || afterBoundary)
                    {
                        FirearmMisfireRuntime.QueueForcedNaturalRoll(10);
                        f.Click(f.Enemy);
                        for (int tick = 0; tick < 240 && f.Shots.Count == 1; tick++)
                        { f.Pump(); f.Record(tick); yield return null; }
                        FirearmInputCheck(prefix + "next-turn-explicit-attack", f.Shots.Count == 2 && f.State.IsEmpty &&
                            f.State.Condition == FirearmCondition.Broken && f.Powder == powder - 1 &&
                            costs.Exact(f.LastShotCommand, true), f.Describe() + ";fresh ordinary input spends its actual next-turn Standard");
                    }
                }
                f.Dispose();
                FirearmInputCheck(prefix + "cleanup", f.Restored, "Native fixture restored exactly.");
                _firearmInputFixture = null;
            }
        }
        private IEnumerable<object> FirearmReviewHoverCases()
        {
            foreach (string cost in new[] { "move", "standard", "full-round" })
            {
                string prefix = "cr03-hover-" + cost + "-";
                _firearmInputStage = prefix + "fixture";
                _firearmInputFixture = new FirearmInputFixture(true, _firearmInputRows, true,
                    cost == "full-round" ? FirearmKind.Musket : FirearmKind.Pistol, cost == "move");
                var f = _firearmInputFixture;
                using (var navigation = new FirearmPredictionNavigationScope(_request.EvidenceDirectory, cost))
                using (var prediction = new FirearmSelfPredictionScope())
                using (var costs = new FirearmReviewCosts(f.Actor, _firearmInputRows))
                {
                    foreach (object step in FirearmReviewBreak(f, prefix)) yield return step;
                    f.Turns.EndCurrentTurn(); f.Turns.ReachCasterTurn();
                    FirearmPredictHoveredTarget(f);
                    FirearmMisfireRuntime.QueueForcedNaturalRoll(10);
                    f.Click(f.Enemy);
                    var order = BrokenSequenceSuppressionRuntime.Orders.Current(f.Actor);
                    var reload = f.Actor.Commands.Raw.OfType<UnitUseAbility>().Single();
                    for (int tick = 0; tick < 240 && !reload.IsFinished; tick++)
                    { f.Pump(); yield return null; }
                    FirearmInputCheck(prefix + "reload-native-cost", costs.Exact(reload, true) &&
                        f.State.LoadedRounds == 1 && f.State.Condition == FirearmCondition.Broken &&
                        f.Powder == 11 && f.Shots.Count == 1 && FirearmScheduledCallbacks() == 1,
                        f.Describe() + ";native Standard.CanUse=" + Game.Instance.TurnBasedCombatController.CurrentTurn.ActionsStates.Standard.CanUse);
                    f.Turns.FlushFirearmCallbacks();
                    var queued = f.Actor.Commands.Raw.Concat(f.Actor.Commands.Queue).OfType<UnitAttack>().SingleOrDefault();
                    FirearmInputCheck(prefix + "continuation-owned", BrokenSequenceSuppressionRuntime.Orders.IsCurrent(order) &&
                        f.Actor.Commands.Raw.Concat(f.Actor.Commands.Queue).OfType<UnitAttack>().Any(command =>
                            ReferenceEquals(NativeFirearmAttackOrder.Get(command)?.Order, order) && !command.IsActed) &&
                        f.Shots.Count == 1, "Real hover prediction cannot cancel the accepted native queue entry.");
                    if (cost == "move")
                    {
                        for (int tick = 0; tick < 240 && f.Shots.Count < 2; tick++)
                        { f.Pump(); yield return null; }
                        FirearmInputCheck(prefix + "one-click-shot", f.Shots.Count == 2 && f.State.IsEmpty &&
                            f.State.Condition == FirearmCondition.Broken && f.Powder == 11 &&
                            costs.Exact(f.LastShotCommand, true) && f.SameItemIdentity,
                            f.Describe() + ";one Move reload and one native Standard attack after real hover input");
                    }
                    else
                    {
                        for (int tick = 0; tick < 12; tick++) { f.Pump(); yield return null; }
                        FirearmInputCheck(prefix + "no-premature-action", f.Shots.Count == 1 && f.State.LoadedRounds == 1 &&
                            f.Powder == 11 && FirearmScheduledCallbacks() == 0 && costs.Count(reload) == 1 &&
                            costs.Count(queued) == 0 && !queued.IsStarted && f.SameItemIdentity,
                            f.Describe() + ";turnStatus=" + Game.Instance.TurnBasedCombatController.CurrentTurn.Status +
                            ";currentOrder=" + BrokenSequenceSuppressionRuntime.Orders.IsCurrent(order) +
                            ";reloadCharges=" + costs.Count(reload) + ";callbacks=" + FirearmScheduledCallbacks() +
                            ";queued=" + queued.IsStarted + "/" + queued.IsActed + "/" + queued.IsFinished + "/" + queued.Result +
                            ";nativeForceFinishes=" + costs.ForcedCount(queued) + ";attackCosts=" + costs.Count(queued));
                        var turn = Game.Instance.TurnBasedCombatController.CurrentTurn;
                        bool ended = turn.Status == TurnBased.Controllers.TurnController.TurnStatus.Ended;
                        FirearmInputCheck(prefix + "native-ownership-boundary", queued.IsFinished &&
                            !BrokenSequenceSuppressionRuntime.Orders.IsCurrent(order) &&
                            !f.Actor.Commands.Raw.Concat(f.Actor.Commands.Queue).Contains(queued) &&
                            (cost == "full-round" ? ended : !ended && costs.ForcedCount(queued) == 1 &&
                                queued.IsActed && queued.Result == UnitCommand.ResultType.Success),
                            "Native spent-action rejection force-finishes Standard without cost; the spent full round also ends its turn. Status=" + turn.Status);
                        f.Turns.FlushFirearmCallbacks();
                        FirearmInputCheck(prefix + "terminal-order-cannot-resume", !NativeFirearmAttackOrder.MayResume(
                            NativeFirearmAttackOrder.Get(queued)) && FirearmScheduledCallbacks() == 0 && f.Shots.Count == 1,
                            "A genuinely native-terminated order has no remaining callback authority.");
                        if (!ended) f.Turns.EndCurrentTurn();
                        f.Turns.FlushFirearmCallbacks();
                        FirearmInputCheck(prefix + "native-end-cancels", !BrokenSequenceSuppressionRuntime.Orders.IsCurrent(order) &&
                            !f.Actor.Commands.Raw.Concat(f.Actor.Commands.Queue).OfType<UnitAttack>().Any() &&
                            f.Shots.Count == 1 && f.State.LoadedRounds == 1,
                            "Native ordinary turn end cancels the pending order; no new cross-turn continuation is introduced.");
                    }
                }
                f.Dispose();
                FirearmInputCheck(prefix + "cleanup", f.Restored, "Native fixture restored exactly.");
                _firearmInputFixture = null;
            }
        }
    }
}
