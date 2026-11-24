using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility;
using Kingmaker.View;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KmgLoaderCompatHarness
{
    /// <summary>
    /// Sprint 7 T7 / VIS-06 (review R1-F12): the Trumpet Archon's owned cast cue across an in-process save and load,
    /// on the unchanged Sprint 7 product. Uses the admitted same-process reload route (saveGame, loadLastSave) in the
    /// disposable fixture save; nothing is written outside the game copy.
    ///
    /// Mode "before": a published Trumpet Archon (the product's own summon path) blasts once and its cue is followed to
    /// its native end; it blasts again and the step ends while that second cue is still playing, so the next step saves
    /// during the playback. Mode "after" (following saveGame and loadLastSave): the second cue's playing ID has ended,
    /// by its native end-of-event callback either naturally or after the product's retirement stop when the old view
    /// went away; nothing was posted again by the load; the loaded Trumpet's fresh blast posts exactly one cue that ends
    /// naturally.
    ///
    /// Disclosed setup: appear buffs removed, brain off (re-applied after load), time unpaused, and every saving throw
    /// other than the Trumpet's own seeded to a natural 20 so the blast paralyzes nobody in the fixture.
    /// </summary>
    internal sealed class TrumpetCueProbe : IOwnedProbe, IGlobalRulebookHandler<RuleSavingThrow>
    {
        private const BindingFlags Any = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const string CueEvent = "InspireCourage";

        // Carried from "before" to "after" inside the one process.
        private static string s_TrumpetId;
        private static object s_PlayingCue;
        private static uint s_PlayingId;
        private static DateTime s_BeforeEndedUtc;

        private readonly string mode;
        private readonly JObject result = new JObject();
        private readonly List<string> problems = new List<string>();
        private readonly IEnumerator<object> script;
        private readonly List<UnitEntityData> created = new List<UnitEntityData>();
        private readonly List<UnitEntityData> awakeAdded = new List<UnitEntityData>();
        private string verdict;
        private bool subscribed;
        private int seed = -1;
        private UnitEntityData trumpet;
        private Assembly product;
        private Type runner, cueRuntime;

        public TrumpetCueProbe(string mode) { this.mode = mode; result["mode"] = mode; script = Run().GetEnumerator(); }
        public object Result => result;

        public string Poll()
        {
            if (verdict != null) return verdict;
            bool more;
            try { more = script.MoveNext(); }
            catch (Exception exception) { problems.Add("probe exception: " + exception); more = false; }
            if (more) return null;
            result["problems"] = new JArray(problems.Cast<object>().ToArray());
            verdict = problems.Count == 0 ? string.Empty : string.Join(" | ", problems.ToArray());
            return verdict;
        }

        public CleanupReport Release(string testFailure)
        {
            var releases = new List<KeyValuePair<string, Action>>
            {
                new KeyValuePair<string, Action>("save seeding", Unsubscribe),
                new KeyValuePair<string, Action>("awake registrations", () => { foreach (UnitEntityData u in awakeAdded) Game.Instance.State.AwakeUnits.Remove(u); awakeAdded.Clear(); })
            };
            // The "before" Trumpet must survive into the save; it is released by the "after" step (or by any failure).
            if (mode != "before" || !string.IsNullOrEmpty(testFailure))
                releases.Add(new KeyValuePair<string, Action>("disposable units", () => { foreach (UnitEntityData u in created.Where(u => u != null && !u.Destroyed).ToArray()) u.Destroy(); created.Clear(); }));
            return ProbeCleanup.RunAll(releases);
        }

        public void OnEventAboutToTrigger(RuleSavingThrow evt)
        {
            if (evt != null && evt.Initiator != null && !ReferenceEquals(evt.Initiator, trumpet) && seed >= 0) UnityEngine.Random.InitState(seed);
        }

        public void OnEventDidTrigger(RuleSavingThrow evt) { }

        private void Subscribe() { if (!subscribed) { EventBus.Subscribe(this); subscribed = true; } }
        private void Unsubscribe() { if (subscribed) { EventBus.Unsubscribe(this); subscribed = false; } }

        private static int D20Seed(int expected)
        {
            for (int value = 0; value < 100000; value++)
            {
                UnityEngine.Random.InitState(value);
                if (UnityEngine.Random.Range(1, 21) == expected) return value;
            }
            throw new InvalidOperationException("no d20 seed for " + expected);
        }

        private IEnumerable<object> Run()
        {
            product = ProductProbes.Entry().Assembly;
            runner = product.GetType("KingmakerGunslinger.RuntimeTesting.RuntimeTestRunner", true);
            cueRuntime = product.GetType("KingmakerGunslinger.Summoning.SummonAbilityCastCueRuntime", true);
            seed = D20Seed(20);
            result["area"] = Game.Instance.CurrentlyLoadedArea == null ? null : Game.Instance.CurrentlyLoadedArea.name;
            bool paused = Game.Instance.IsPaused;
            Subscribe();
            try
            {
                Game.Instance.IsPaused = false;
                IEnumerable<object> body = mode == "before" ? Before() : mode == "after" ? After() : null;
                if (body == null) { problems.Add("unknown mode " + mode); yield break; }
                foreach (object step in body) yield return step;
            }
            finally { Unsubscribe(); Game.Instance.IsPaused = paused; }
        }

        // ---------------------------------------------------------------- cue records (read-only, by reflection)

        private object[] Cues() => ((Array)cueRuntime.GetMethod("Snapshot", Any).Invoke(null, null)).Cast<object>().ToArray();
        private static T Field<T>(object record, string name) => (T)record.GetType().GetField(name, Any).GetValue(record);

        private static JObject Describe(object record)
        {
            if (record == null) return null;
            DateTime utc = Field<DateTime>(record, "Utc");
            DateTime? end = Field<DateTime?>(record, "EndUtc");
            DateTime? stop = Field<DateTime?>(record, "StopRequestedUtc");
            return new JObject { ["event"] = Field<string>(record, "EventName"), ["ability"] = Field<string>(record, "Ability"),
                ["caster"] = Field<string>(record, "CasterId"), ["playingId"] = Field<uint>(record, "PlayingId"), ["utc"] = utc.ToString("o"),
                ["frame"] = Field<int>(record, "Frame"), ["endUtc"] = end.HasValue ? end.Value.ToString("o") : null,
                ["seconds"] = end.HasValue ? (end.Value - utc).TotalSeconds : (double?)null, ["stopReason"] = Field<string>(record, "StopReason"),
                ["stopRequestedUtc"] = stop.HasValue ? stop.Value.ToString("o") : null, ["stopResult"] = Field<string>(record, "StopResult") };
        }

        private object[] CuesOf(UnitEntityData unit, int from) =>
            Cues().Skip(from).Where(r => Field<string>(r, "CasterId") == unit.UniqueId && Field<string>(r, "EventName") == CueEvent).ToArray();

        // ---------------------------------------------------------------- fixture

        private void Wake(UnitEntityData unit)
        {
            unit.Wake(900f);
            if (!Game.Instance.State.AwakeUnits.Contains(unit)) { Game.Instance.State.AwakeUnits.Add(unit); awakeAdded.Add(unit); }
        }

        private Ability Blast(UnitEntityData unit)
        {
            return unit.Descriptor.Abilities.Enumerable.FirstOrDefault(a => a.Blueprint != null && a.Blueprint.ComponentsArray != null &&
                a.Blueprint.ComponentsArray.Any(c => c.GetType().Name == "SummonAbilityCastCue" &&
                    (string)c.GetType().GetField("EventName", Any).GetValue(c) == CueEvent));
        }

        private UnitEntityData SummonTrumpet()
        {
            UnitEntityData main = Game.Instance.Player.MainCharacter.Value;
            Type multiplicity = product.GetType("KingmakerGunslinger.Summoning.SummonMultiplicity", true);
            object variant = runner.GetMethod("ExpandedSummoningOwnTierVariant", Any).Invoke(null, new[] { "trumpet-archon", Enum.Parse(multiplicity, "One") });
            object evidence = Activator.CreateInstance(runner.GetNestedType("ExpandedSummoningMechanicalEvidence", Any), true);
            var library = (LibraryScriptableObject)product.GetType("KingmakerGunslinger.Bootstrap.BlueprintBootstrap", true).GetProperty("Library", Any).GetValue(null, null);
            var units = (UnitEntityData[])runner.GetMethods(Any).Single(m => m.Name == "CastExpandedSummoningVariant" && m.GetParameters().Length == 5)
                .Invoke(null, new object[] { library.GetAllBlueprints().ToArray(), main, variant, null, evidence });
            UnitEntityData unit = units.Length == 1 ? units[0] : null;
            if (unit == null) return null;
            created.Add(unit);
            runner.GetMethod("RemoveExpandedSummoningAppearanceBuffs", Any).Invoke(null, new object[] { unit });
            runner.GetMethod("SetExpandedSummoningBrainActive", Any).Invoke(null, new object[] { unit, false });
            Wake(unit);
            Vector3 forward = Quaternion.Euler(0, main.Orientation, 0) * Vector3.forward;
            Vector3 at = ObstacleAnalyzer.TraceAlongNavmesh(main.Position, main.Position + forward * 4f);
            runner.GetMethod("PlaceExpandedSummoningUnit", Any).Invoke(null, new object[] { unit, at });
            return unit;
        }

        /// <summary>Runs one blast through the native ability command until the command and its execution have ended.</summary>
        private IEnumerable<object> Cast(UnitEntityData unit, JObject row)
        {
            Ability blast = Blast(unit);
            if (blast == null) { problems.Add(row.Path + ": the Trumpet has no owned blast with the cue"); yield break; }
            var target = new TargetWrapper(unit);
            row["ability"] = blast.Blueprint.name; row["available"] = blast.Data.IsAvailable; row["canTarget"] = blast.Data.CanTarget(target);
            if (!blast.Data.IsAvailable || !blast.Data.CanTarget(target)) { problems.Add("the blast is not available to cast"); yield break; }
            var command = new UnitUseAbility(blast.Data, target);
            unit.Commands.Run(command);
            float deadline = Time.realtimeSinceStartup + 30f;
            while (Time.realtimeSinceStartup < deadline && !(command.IsFinished && (command.ExecutionProcess == null || command.ExecutionProcess.IsEnded))) yield return null;
            row["finished"] = command.IsFinished; row["result"] = command.Result.ToString();
            row["executionEnded"] = command.ExecutionProcess != null && command.ExecutionProcess.IsEnded;
            if (!command.IsFinished || command.Result != Kingmaker.UnitLogic.Commands.Base.UnitCommand.ResultType.Success) problems.Add(row.Path + ": the blast did not complete");
        }

        private static IEnumerable<object> Seconds(float seconds)
        {
            float end = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < end) yield return null;
        }

        // ---------------------------------------------------------------- modes

        private IEnumerable<object> Before()
        {
            s_TrumpetId = null; s_PlayingCue = null; s_PlayingId = 0;
            trumpet = SummonTrumpet();
            if (trumpet == null) { problems.Add("the published Trumpet Archon summon produced no unit"); yield break; }
            result["trumpet"] = new JObject { ["id"] = trumpet.UniqueId, ["blueprint"] = trumpet.Blueprint.name };
            foreach (object step in Seconds(2f)) yield return step;

            // 1. A blast whose cue is followed to its native end.
            var first = new JObject(); result["first"] = first;
            int from = Cues().Length;
            foreach (object step in Cast(trumpet, first)) yield return step;
            float wait = Time.realtimeSinceStartup + 15f;
            while (Time.realtimeSinceStartup < wait && !CuesOf(trumpet, from).All(r => Field<DateTime?>(r, "EndUtc").HasValue)) yield return null;
            object[] firstCues = CuesOf(trumpet, from);
            first["cues"] = new JArray(firstCues.Select(r => (object)Describe(r)).ToArray());
            if (firstCues.Length != 1 || Field<uint>(firstCues[0], "PlayingId") == 0 || !Field<DateTime?>(firstCues[0], "EndUtc").HasValue ||
                Field<string>(firstCues[0], "StopReason") != null)
                problems.Add("before.first: one accepted cue that ends naturally was expected");

            // 2. A second blast; the step ends while its cue still plays, so the save is taken during the playback.
            var second = new JObject(); result["second"] = second;
            from = Cues().Length;
            foreach (object step in Cast(trumpet, second)) yield return step;
            object[] secondCues = CuesOf(trumpet, from);
            second["cues"] = new JArray(secondCues.Select(r => (object)Describe(r)).ToArray());
            if (secondCues.Length != 1 || Field<uint>(secondCues[0], "PlayingId") == 0) { problems.Add("before.second: one accepted cue was expected"); yield break; }
            bool playing = !Field<DateTime?>(secondCues[0], "EndUtc").HasValue;
            second["playingAtStepEnd"] = playing;
            if (!playing) problems.Add("before.second: the cue had already ended before the save could start");
            s_TrumpetId = trumpet.UniqueId; s_PlayingCue = secondCues[0]; s_PlayingId = Field<uint>(secondCues[0], "PlayingId");
            s_BeforeEndedUtc = DateTime.UtcNow;
            result["beforeEndedUtc"] = s_BeforeEndedUtc.ToString("o");
        }

        private IEnumerable<object> After()
        {
            DateTime started = DateTime.UtcNow;
            result["afterStartedUtc"] = started.ToString("o");
            if (s_TrumpetId == null || s_PlayingCue == null) { problems.Add("after: no state from the before step in this process"); yield break; }
            trumpet = Game.Instance.State.Units.FirstOrDefault(u => u.UniqueId == s_TrumpetId && !u.Destroyed);
            result["trumpetLoaded"] = trumpet != null;
            if (trumpet == null) { problems.Add("after: the saved Trumpet Archon is not in the loaded game"); yield break; }
            created.Add(trumpet);
            runner.GetMethod("SetExpandedSummoningBrainActive", Any).Invoke(null, new object[] { trumpet, false });
            Wake(trumpet);

            // 1. The cue that was playing at the save: ended by its native callback, naturally or after retirement.
            float wait = Time.realtimeSinceStartup + 10f;
            while (Time.realtimeSinceStartup < wait && !Field<DateTime?>(s_PlayingCue, "EndUtc").HasValue) yield return null;
            JObject saved = Describe(s_PlayingCue);
            result["savedCue"] = saved;
            DateTime? end = Field<DateTime?>(s_PlayingCue, "EndUtc");
            string reason = Field<string>(s_PlayingCue, "StopReason");
            bool natural = end.HasValue && reason == null;
            bool retired = end.HasValue && reason != null && Field<string>(s_PlayingCue, "StopResult") == "StopRequested";
            saved["endedBy"] = natural ? "natural" : retired ? "retirement:" + reason : "unended";
            saved["endedBeforeAfterStep"] = end.HasValue && end.Value <= started;
            if (!natural && !retired) problems.Add("after: the cue playing at the save has no native end");
            if (Field<uint>(s_PlayingCue, "PlayingId") != s_PlayingId) problems.Add("after: the saved cue's playing ID changed");

            // 2. No replay: nothing posted for this Trumpet since the before step ended, through a quiet interval.
            foreach (object step in Seconds(5f)) yield return step;
            object[] replays = Cues().Where(r => Field<string>(r, "CasterId") == s_TrumpetId && Field<DateTime>(r, "Utc") > s_BeforeEndedUtc).ToArray();
            result["postLoadCuesBeforeFreshBlast"] = new JArray(replays.Select(r => (object)Describe(r)).ToArray());
            if (replays.Length != 0) problems.Add("after: the load posted the Trumpet cue again");

            // 3. A fresh blast from the loaded Trumpet posts exactly one cue that ends naturally.
            var fresh = new JObject(); result["fresh"] = fresh;
            int from = Cues().Length;
            foreach (object step in Cast(trumpet, fresh)) yield return step;
            float done = Time.realtimeSinceStartup + 15f;
            while (Time.realtimeSinceStartup < done && !CuesOf(trumpet, from).All(r => Field<DateTime?>(r, "EndUtc").HasValue)) yield return null;
            object[] freshCues = CuesOf(trumpet, from);
            fresh["cues"] = new JArray(freshCues.Select(r => (object)Describe(r)).ToArray());
            if (freshCues.Length != 1 || Field<uint>(freshCues[0], "PlayingId") == 0 || !Field<DateTime?>(freshCues[0], "EndUtc").HasValue ||
                Field<string>(freshCues[0], "StopReason") != null || Field<uint>(freshCues[0], "PlayingId") == s_PlayingId)
                problems.Add("after.fresh: one new accepted cue that ends naturally was expected");
            s_TrumpetId = null; s_PlayingCue = null; s_PlayingId = 0;
        }
    }
}
