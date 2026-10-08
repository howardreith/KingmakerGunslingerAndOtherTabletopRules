using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private Sprint17LoadResetTrace _snakeLoadResetTrace;
        private string _snakeLoadResetTraceInstallError;

        private void StartSprint17LoadResetTrace()
        {
            if (!SerpentinePersistenceReviewPolicy.ObserveLoad(_request.Scenario,
                (string)_request.Parameters?["persistenceScope"]) &&
                !ExpandedSummoningRosterPersistencePolicy.ObserveLoad(_request.Scenario,
                    (string)_request.Parameters?["persistenceScope"])) return;
            try { _snakeLoadResetTrace = new Sprint17LoadResetTrace(_context.Harmony); }
            catch (Exception exception) { _snakeLoadResetTraceInstallError = exception.ToString(); }
        }

        private void StopSprint17LoadResetTrace(RuntimeTestResult result)
        {
            if (_snakeLoadResetTrace == null && _snakeLoadResetTraceInstallError == null) return;
            try
            {
                if (_snakeLoadResetTraceInstallError != null)
                    throw new InvalidOperationException(_snakeLoadResetTraceInstallError);
                _snakeLoadResetTrace.Capture("scenario-complete", null);
                _snakeLoadResetTrace.Dispose();
                string path = Path.Combine(_request.EvidenceDirectory, "sprint17-snake-load-reset-trace.json");
                RuntimeTestResultWriter.WriteAtomic(path,
                    _snakeLoadResetTrace.Rows.ToString(Formatting.Indented));
                if (result.EvidenceFiles == null) result.EvidenceFiles = new List<string>();
                result.EvidenceFiles.Add(path);
                if (_snakeLoadResetTrace.Errors != 0)
                    throw new InvalidOperationException("Load-reset observation has " +
                        _snakeLoadResetTrace.Errors + " read/overflow errors; inspect its trace.");
            }
            catch (Exception exception)
            {
                result.Status = RuntimeTestStatuses.Fail;
                result.Diagnostics.Add("Snake load-reset observer: " + exception);
            }
        }

        // Request-local read-only hooks. No event is raised, part/buff removed,
        // clock advanced or unit collection changed by this observer.
        private sealed class Sprint17LoadResetTrace : IDisposable
        {
            private static Sprint17LoadResetTrace _active;
            private readonly HarmonyInstance _harmony;
            private readonly List<KeyValuePair<MethodInfo, MethodInfo>> _patches =
                new List<KeyValuePair<MethodInfo, MethodInfo>>();
            internal readonly JArray Rows = new JArray();
            private readonly SerpentineLoadBoundaryReadiness _boundary = new SerpentineLoadBoundaryReadiness();
            internal bool NativeLoadReady
            {
                get { return Errors == 0 && Game.Instance?.State != null &&
                    _boundary.Ready(Game.Instance.State, Game.Instance.CurrentlyLoadedArea); }
            }
            internal int NativeLoadCompletedFrame { get { return _boundary.CompletedFrame; } }
            internal int Errors;
            private bool _disposed;

            internal Sprint17LoadResetTrace(HarmonyInstance harmony)
            {
                if (_active != null) throw new InvalidOperationException("A load-reset trace already owns the hooks.");
                _harmony = harmony;
                _active = this;
                try
                {
                    Patch("OnAreaScenesLoaded", "ScenesPrefix", null);
                    Patch("OnAreaLoadingComplete", "CompletePrefix", "CompletePostfix");
                    Patch("ResetLoadedGrapples", "ResetPrefix", "ResetPostfix");
                    Application.logMessageReceived += OnLog;
                    Capture("observer-armed", null);
                }
                catch { Dispose(); throw; }
            }

            private void Patch(string name, string prefix, string postfix)
            {
                const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance | BindingFlags.Static;
                var original = typeof(SummonGrappleAreaSafeguard).GetMethod(name, flags);
                var before = prefix == null ? null : typeof(Sprint17LoadResetTrace).GetMethod(prefix, flags);
                var after = postfix == null ? null : typeof(Sprint17LoadResetTrace).GetMethod(postfix, flags);
                if (original == null || (prefix != null && before == null) || (postfix != null && after == null))
                    throw new MissingMethodException("Exact load safeguard observation seam: " + name);
                _harmony.Patch(original, before == null ? null : new HarmonyMethod(before),
                    after == null ? null : new HarmonyMethod(after), null);
                if (before != null) _patches.Add(new KeyValuePair<MethodInfo, MethodInfo>(original, before));
                if (after != null) _patches.Add(new KeyValuePair<MethodInfo, MethodInfo>(original, after));
            }

            private static void ScenesPrefix() { _active?.Capture("scenes-loaded", null); }
            private static void CompletePrefix() { _active?.Capture("load-complete-before", null); }
            private static void CompletePostfix() { _active?.Capture("load-complete-after", null); }
            private static void ResetPrefix(IEnumerable<UnitEntityData> units) { _active?.Capture("reset-before", units); }
            private static void ResetPostfix(IEnumerable<UnitEntityData> units) { _active?.Capture("reset-after", units); }

            private void OnLog(string message, string stack, LogType type)
            {
                if ((type != LogType.Exception && type != LogType.Error) ||
                    ((message ?? "") + (stack ?? "")).IndexOf("SummonGrappleAreaSafeguard",
                        StringComparison.Ordinal) < 0) return;
                Add(new JObject { ["phase"] = "native-safeguard-error",
                    ["frame"] = Time.frameCount, ["message"] = message, ["stack"] = stack });
            }

            private void Add(JObject row)
            {
                if (Rows.Count < 32) Rows.Add(row);
                else Errors++;
            }

            internal void Capture(string phase, IEnumerable<UnitEntityData> supplied)
            {
                try
                {
                    var state = Game.Instance?.State;
                    var areaBlueprint = state == null ? null : Game.Instance.CurrentlyLoadedArea;
                    if (phase == "scenes-loaded") _boundary.ScenesLoaded(state, areaBlueprint, Time.frameCount);
                    if (phase == "load-complete-after") _boundary.Completed(state, areaBlueprint, Time.frameCount);
                    var player = state?.PlayerState;
                    var pool = state?.Units?.All?.ToArray() ?? new UnitEntityData[0];
                    var party = player?.Party?.ToArray() ?? new UnitEntityData[0];
                    var area = party.Where(value => value?.HoldingState != null)
                        .Select(value => value.HoldingState).Distinct()
                        .SelectMany(value => value.AllEntityData.OfType<UnitEntityData>()).Distinct().ToArray();
                    var input = supplied == null ? new UnitEntityData[0] : supplied.ToArray();
                    var instance = typeof(SummonGrappleAreaSafeguard).GetField("_instance",
                        BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
                    Add(new JObject { ["phase"] = phase, ["frame"] = Time.frameCount,
                        ["gameSeconds"] = player == null ? (double?)null : player.GameTime.TotalSeconds,
                        ["subscribed"] = EventBus.IsGloballySubscribed(instance),
                        ["nativeLoadReady"] = _boundary.Ready(state, areaBlueprint),
                        ["poolCount"] = pool.Length, ["areaCount"] = area.Length,
                        ["supplied"] = new JArray(input.Where(SummonGrappleAreaSafeguard.IsKmgSummon)
                            .Select(value => value.UniqueId)),
                        ["units"] = new JArray(pool.Concat(area).Concat(input).Distinct()
                            .Where(value => value?.Descriptor != null && SummonGrappleAreaSafeguard.IsKmgSummon(value))
                            .Select(value => Describe(value, pool, area, input))) });
                }
                catch (Exception exception)
                {
                    Errors++;
                    Add(new JObject { ["phase"] = phase, ["observationError"] = exception.ToString() });
                }
            }

            private static JObject Describe(UnitEntityData unit, UnitEntityData[] pool,
                UnitEntityData[] area, UnitEntityData[] input)
            {
                var initiator = unit.Get<UnitPartGrappleInitiator>();
                var target = unit.Get<UnitPartGrappleTarget>();
                var links = unit.Get<UnitPartSummonGrappleLinks>();
                var grab = SummonGrabComponent.Find(unit);
                return new JObject { ["id"] = unit.UniqueId, ["blueprint"] = unit.Blueprint.name,
                    ["role"] = unit.Get<UnitPartSprint17PersistenceReceipt>()?.Role,
                    ["inPool"] = pool.Any(value => ReferenceEquals(value, unit)),
                    ["inPartyArea"] = area.Any(value => ReferenceEquals(value, unit)),
                    ["inResetInput"] = input.Any(value => ReferenceEquals(value, unit)),
                    ["initiatorPart"] = initiator != null, ["targetPart"] = target != null,
                    ["initiatorTarget"] = initiator == null ? null : initiator.Target.Value?.UniqueId,
                    ["targetOwner"] = target == null ? null : target.Initiator.Value?.UniqueId,
                    ["grabPresent"] = grab != null, ["storedLinks"] = links == null ? 0 : links.Count,
                    ["holdBuff"] = grab != null && unit.Descriptor.HasFact(grab.HoldBuff),
                    ["grappledBuff"] = grab != null && unit.Descriptor.HasFact(grab.GrappledBuff),
                    ["cantAct"] = unit.Descriptor.State.HasCondition(UnitCondition.CantAct),
                    ["cantMove"] = unit.Descriptor.State.HasCondition(UnitCondition.CantMove) };
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                Application.logMessageReceived -= OnLog;
                _active = null;
                foreach (var patch in _patches) _harmony.Unpatch(patch.Key, patch.Value);
                _patches.Clear();
            }
        }
    }
}
