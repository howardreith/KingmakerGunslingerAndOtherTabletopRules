using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
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
    /// its native end; during that first blast, while the command yields, an unrelated saving throw and an unrelated
    /// random draw are made and compared with the generator's own control values (review R2-F2); it blasts again and the
    /// step ends while that second cue is still playing, so the next step saves during the playback. Mode "after"
    /// (following saveGame and loadLastSave): the second cue's playing ID has ended, by its native end-of-event callback
    /// either naturally or after the product's retirement stop when the old view went away; nothing was posted again by
    /// the load; the loaded Trumpet's fresh blast posts exactly one cue that ends naturally. Mode "abandon": a blast is
    /// started and the probe stays yielded until the step's timeout abandons it; mode "abandon-check" then confirms that
    /// the abandoned probe's subscription, seeding and command ended.
    ///
    /// Disclosed setup: appear buffs removed, brain off (re-applied after load), time unpaused. The intervention stays
    /// because the blast's native outcome would paralyze non-archons in range, the fixture party included, and that
    /// condition would be carried into the save the reload observation takes (review R2-F2, round 6). It is now the
    /// narrowest one: a saving throw that belongs to the observed blast's own execution (its reason's context is that
    /// execution or derives from it), is top level on the rule stack and whose saver is not the Trumpet passes through
    /// its own AutoPass field. No d20 rule and no shared-generator draw is touched; the owned save's die is rolled
    /// natively. During the first blast's first owned save, a foreign d20 is dispatched directly from inside that save
    /// before its native roll, and the foreign die, the owned save's own die and the following draw are compared with an
    /// untreated generator copy.
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
        private static TrumpetCueProbe s_Abandoned;

        private readonly string mode;
        private readonly JObject result = new JObject();
        private readonly OwnedProbeResources owned = new OwnedProbeResources();
        private readonly YieldedProbeLifecycle life;
        // Read only: whether a die was pre-rolled by anyone.
        private static readonly FieldInfo PreRolled = typeof(RuleRollD20).GetField("m_PreRolledResult", Any);
        private readonly List<RuleSavingThrow> marked = new List<RuleSavingThrow>();
        private int autoPassed;
        private bool nestedPending;
        private RuleSavingThrow nestedSave;
        private JObject nested;
        private int nestedOwnedControl;
        private float nestedFollowingControl;
        private readonly OwnedDisposables<UnitEntityData> units =
            new OwnedDisposables<UnitEntityData>(u => u.UniqueId, u => { if (!u.Destroyed) u.Destroy(); });
        private readonly List<UnitEntityData> awakeAdded = new List<UnitEntityData>();
        private readonly JArray ownedSaves = new JArray();
        private bool subscribed;
        private UnitUseAbility observed;
        private UnitEntityData trumpet;
        private Assembly product;
        private Type runner, cueRuntime;

        public TrumpetCueProbe(string mode)
        {
            this.mode = mode;
            result["mode"] = mode;
            life = new YieldedProbeLifecycle(owned, lifecycle => Run());
        }

        public object Result => result;

        public string Poll()
        {
            string verdict = life.Poll();
            if (verdict != null) result["problems"] = new JArray(life.Problems.Cast<object>().ToArray());
            return verdict;
        }

        // Every exit (normal end, failure, timeout, abandonment while yielded): the iterator is disposed (its finally
        // blocks run), then every held restore runs once, newest first; every failure, from whichever drain, is in the
        // returned report (R3-F2).
        public CleanupReport Release(string testFailure)
        {
            var releases = ProbeReleases.PerItem("awake registration", awakeAdded, u => u.UniqueId, u => Game.Instance.State.AwakeUnits.Remove(u));
            // The "before" Trumpet must survive into the save; it is released by the "after" step (or by any failure).
            // Final unit releases are built after the iterator's cleanup, for units never requested (R5-F5).
            bool keep = mode == "before" && string.IsNullOrEmpty(testFailure) && life.Ended;
            CleanupReport report = life.Release(() => keep ? releases : releases.Concat(units.Releases("disposable unit")).ToList());
            result["problems"] = new JArray(life.Problems.Cast<object>().ToArray());
            result["ownedSaves"] = ownedSaves;
            if (mode == "abandon") s_Abandoned = this;
            return report;
        }

        // ---------------------------------------------------------------- the owned saves (R2-F2)

        private bool OwnedByBlast(RuleSavingThrow evt)
        {
            UnitUseAbility command = observed;
            if (command == null || evt == null || evt.Initiator == null || ReferenceEquals(evt.Initiator, trumpet)) return false;
            AbilityExecutionContext execution = command.ExecutionProcess == null ? null : command.ExecutionProcess.Context;
            Kingmaker.UnitLogic.Mechanics.MechanicsContext context = evt.Reason == null ? null : evt.Reason.Context;
            RulebookEventContext rules = Rulebook.CurrentContext;
            return execution != null && context != null && (ReferenceEquals(context, execution) || ReferenceEquals(context.SourceAbilityContext, execution)) &&
                TrumpetSaveOwnership.TopLevelSave(rules == null ? null : rules.EventStack.Cast<object>(), evt, rule => rule is RuleSavingThrow);
        }

        // The only intervention: the blast's own top-level save passes through its own AutoPass; its d20 rolls natively.
        public void OnEventAboutToTrigger(RuleSavingThrow evt)
        {
            if (!OwnedByBlast(evt)) return;
            evt.AutoPass = true;
            marked.Add(evt); autoPassed++;
            if (!nestedPending) return;
            nestedPending = false; nestedSave = evt;
            nested = NestedForeign(evt);
        }

        public void OnEventDidTrigger(RuleSavingThrow evt)
        {
            if (evt == null || !marked.Remove(evt)) return;
            ownedSaves.Add(new JObject { ["saver"] = evt.Initiator.UniqueId, ["stat"] = evt.StatType.ToString(), ["d20"] = (int)evt.D20,
                ["autoPass"] = evt.AutoPass, ["passed"] = evt.IsPassed });
            if (!ReferenceEquals(evt, nestedSave) || nested == null) return;
            // The owned save's own die and the first draw after the save, against the untreated control sequence.
            float following = UnityEngine.Random.value;
            nested["ownedSaveD20"] = (int)evt.D20; nested["controlOwnedD20"] = nestedOwnedControl;
            nested["ownedAutoPass"] = evt.AutoPass; nested["ownedPassed"] = evt.IsPassed;
            nested["followingDraw"] = following; nested["followingControl"] = nestedFollowingControl;
            nested["followingMatchesControl"] = following == nestedFollowingControl;
            nested["verdict"] = TrumpetSaveOwnership.NestedForeignVerdict((bool)nested["insideOwnedSave"], (int)nested["foreignD20"], (int)nested["controlForeignD20"],
                (bool)nested["foreignPreRolled"], (int)evt.D20, nestedOwnedControl, evt.AutoPass, evt.IsPassed, following == nestedFollowingControl);
            nestedSave = null;
        }

        // R2-F2 decisive interval: a foreign d20 for the main character, dispatched directly from inside the owned save's
        // about-to-trigger, before the save's own native roll (its parent on the rule stack is the owned save). The
        // untreated control is a generator copy drawn in the same order: foreign die, owned save die, following draw.
        private JObject NestedForeign(RuleSavingThrow owned)
        {
            UnitEntityData main = Game.Instance.Player.MainCharacter.Value;
            RulebookEventContext rules = Rulebook.CurrentContext;
            bool inside = rules != null && ReferenceEquals(rules.CurrentEvent, owned);
            UnityEngine.Random.State start = UnityEngine.Random.state;
            int controlForeign = UnityEngine.Random.Range(1, 21);
            nestedOwnedControl = UnityEngine.Random.Range(1, 21);
            nestedFollowingControl = UnityEngine.Random.value;
            UnityEngine.Random.state = start;
            var foreign = new RuleRollD20(main);
            Rulebook.Trigger(foreign);
            bool preRolled = PreRolled != null && ((int?)PreRolled.GetValue(foreign)).HasValue;
            return new JObject { ["insideOwnedSave"] = inside, ["foreignInitiator"] = main.UniqueId, ["foreignD20"] = (int)foreign.Result,
                ["controlForeignD20"] = controlForeign, ["foreignPreRolled"] = preRolled, ["ownedSaveAutoPassAtDispatch"] = owned.AutoPass };
        }

        private void Subscribe() { if (!subscribed) { EventBus.Subscribe(this); subscribed = true; } }
        private void Unsubscribe() { if (subscribed) { EventBus.Unsubscribe(this); subscribed = false; } }

        private IEnumerable<object> Run()
        {
            if (mode == "abandon-check") { AbandonCheck(); yield break; }
            product = ProductProbes.Entry().Assembly;
            runner = product.GetType("KingmakerGunslinger.RuntimeTesting.RuntimeTestRunner", true);
            cueRuntime = product.GetType("KingmakerGunslinger.Summoning.SummonAbilityCastCueRuntime", true);
            result["area"] = Game.Instance.CurrentlyLoadedArea == null ? null : Game.Instance.CurrentlyLoadedArea.name;
            bool paused = Game.Instance.IsPaused;
            owned.Hold("pause", () => Game.Instance.IsPaused = paused);
            Subscribe();
            owned.Hold("save seeding subscription", Unsubscribe);
            Game.Instance.IsPaused = false;
            try
            {
                IEnumerable<object> body = mode == "before" ? Before() : mode == "after" ? After() : mode == "abandon" ? Abandon() : null;
                if (body == null) { life.Problems.Add("unknown mode " + mode); yield break; }
                foreach (object step in body) yield return step;
            }
            finally
            {
                CleanupReport held = owned.ReleaseAll();
                if (!held.Restored) life.Problems.Add("restore failures: " + string.Join("; ", held.Failures.ToArray()));
                result["pauseRestored"] = Game.Instance.IsPaused == paused;
            }
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
            units.Add(unit);
            runner.GetMethod("RemoveExpandedSummoningAppearanceBuffs", Any).Invoke(null, new object[] { unit });
            runner.GetMethod("SetExpandedSummoningBrainActive", Any).Invoke(null, new object[] { unit, false });
            Wake(unit);
            Vector3 forward = Quaternion.Euler(0, main.Orientation, 0) * Vector3.forward;
            Vector3 at = ObstacleAnalyzer.TraceAlongNavmesh(main.Position, main.Position + forward * 4f);
            runner.GetMethod("PlaceExpandedSummoningUnit", Any).Invoke(null, new object[] { unit, at });
            return unit;
        }

        // While the blast yields: an unrelated Will save by the main character and an unrelated draw, each compared with
        // the value the shared generator gives from the same state with no intervention (a copy is drawn first and the
        // state put back).
        private JObject Interleave()
        {
            UnitEntityData main = Game.Instance.Player.MainCharacter.Value;
            int markedBefore = autoPassed;
            UnityEngine.Random.State start = UnityEngine.Random.state;
            float controlDraw = UnityEngine.Random.value;
            int controlD20 = UnityEngine.Random.Range(1, 21);
            UnityEngine.Random.state = start;
            float draw = UnityEngine.Random.value;
            var save = new RuleSavingThrow(main, SavingThrowType.Will, 10);
            Rulebook.Trigger(save);
            return new JObject { ["draw"] = draw, ["controlDraw"] = controlDraw, ["drawMatchesControl"] = draw == controlDraw,
                ["saveD20"] = (int)save.D20, ["controlD20"] = controlD20, ["saveMatchesControl"] = (int)save.D20 == controlD20,
                ["saveAutoPass"] = save.AutoPass, ["markedDuringUnrelated"] = autoPassed - markedBefore, ["ownedSavesOpenAfter"] = marked.Count,
                ["blastExecuting"] = observed != null && observed.ExecutionProcess != null && !observed.ExecutionProcess.IsEnded };
        }

        /// <summary>Runs one blast through the native ability command until the command and its execution have ended.</summary>
        private IEnumerable<object> Cast(UnitEntityData unit, JObject row, bool interleave)
        {
            Ability blast = Blast(unit);
            if (blast == null) { life.Problems.Add(row.Path + ": the Trumpet has no owned blast with the cue"); yield break; }
            var target = new TargetWrapper(unit);
            row["ability"] = blast.Blueprint.name; row["available"] = blast.Data.IsAvailable; row["canTarget"] = blast.Data.CanTarget(target);
            if (!blast.Data.IsAvailable || !blast.Data.CanTarget(target)) { life.Problems.Add("the blast is not available to cast"); yield break; }
            var command = new UnitUseAbility(blast.Data, target);
            int savesFrom = ownedSaves.Count;
            // The observed blast, owned from submission: its seeding correlation ends with it (or on any exit).
            observed = command;
            owned.Hold("observed blast", () =>
            {
                observed = null; marked.Clear(); nestedPending = false;
                if (!command.IsFinished) { unit.Commands.InterruptAll(value => ReferenceEquals(value, command)); unit.Commands.RemoveFinishedAndUpdateQueue(); }
            });
            UnityEngine.Random.State blastStart = UnityEngine.Random.state;
            nestedPending = interleave;
            unit.Commands.Run(command);
            float deadline = Time.realtimeSinceStartup + 30f;
            int frames = 0;
            while (Time.realtimeSinceStartup < deadline && !(command.IsFinished && (command.ExecutionProcess == null || command.ExecutionProcess.IsEnded)))
            {
                yield return null;
                if (interleave && ++frames == 3) row["interleaved"] = Interleave();
            }
            owned.ReleaseNow("observed blast", null);
            row["ownedSavesSeeded"] = ownedSaves.Count - savesFrom;
            row["ownedSavesOpenAfter"] = marked.Count;
            if (interleave) row["nestedForeign"] = nested;
            if (interleave) row["generatorRewoundToBlastStart"] = UnityEngine.Random.state.Equals(blastStart);
            row["finished"] = command.IsFinished; row["result"] = command.Result.ToString();
            row["executionEnded"] = command.ExecutionProcess != null && command.ExecutionProcess.IsEnded;
            if (!command.IsFinished || command.Result != Kingmaker.UnitLogic.Commands.Base.UnitCommand.ResultType.Success) life.Problems.Add(row.Path + ": the blast did not complete");
            if (interleave)
            {
                JToken inter = row["interleaved"];
                if (inter == null || !(bool)inter["drawMatchesControl"] || !(bool)inter["saveMatchesControl"] || (bool)inter["saveAutoPass"] || (int)inter["markedDuringUnrelated"] != 0 ||
                    (bool)row["generatorRewoundToBlastStart"] || (int)row["ownedSavesOpenAfter"] != 0)
                    life.Problems.Add(row.Path + ": an unrelated save or draw during the blast did not keep its ordinary outcome, or the generator was rewound");
                string nestedVerdict = nested == null ? "no-owned-save" : nested["verdict"] == null ? "owned-save-not-ended" : (string)nested["verdict"];
                if (nestedVerdict != null)
                    life.Problems.Add(row.Path + ": the foreign d20 inside the owned save, the owned save's own die or the following draw did not keep its untreated value: " + nestedVerdict);
            }
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
            if (trumpet == null) { life.Problems.Add("the published Trumpet Archon summon produced no unit"); yield break; }
            result["trumpet"] = new JObject { ["id"] = trumpet.UniqueId, ["blueprint"] = trumpet.Blueprint.name };
            foreach (object step in Seconds(2f)) yield return step;

            // 1. A blast whose cue is followed to its native end, with the unrelated save and draw while it yields.
            var first = new JObject(); result["first"] = first;
            int from = Cues().Length;
            foreach (object step in Cast(trumpet, first, true)) yield return step;
            float wait = Time.realtimeSinceStartup + 15f;
            while (Time.realtimeSinceStartup < wait && !CuesOf(trumpet, from).All(r => Field<DateTime?>(r, "EndUtc").HasValue)) yield return null;
            object[] firstCues = CuesOf(trumpet, from);
            first["cues"] = new JArray(firstCues.Select(r => (object)Describe(r)).ToArray());
            if (firstCues.Length != 1 || Field<uint>(firstCues[0], "PlayingId") == 0 || !Field<DateTime?>(firstCues[0], "EndUtc").HasValue ||
                Field<string>(firstCues[0], "StopReason") != null)
                life.Problems.Add("before.first: one accepted cue that ends naturally was expected");

            // 2. A second blast; the step ends while its cue still plays, so the save is taken during the playback.
            var second = new JObject(); result["second"] = second;
            from = Cues().Length;
            foreach (object step in Cast(trumpet, second, false)) yield return step;
            object[] secondCues = CuesOf(trumpet, from);
            second["cues"] = new JArray(secondCues.Select(r => (object)Describe(r)).ToArray());
            if (secondCues.Length != 1 || Field<uint>(secondCues[0], "PlayingId") == 0) { life.Problems.Add("before.second: one accepted cue was expected"); yield break; }
            bool playing = !Field<DateTime?>(secondCues[0], "EndUtc").HasValue;
            second["playingAtStepEnd"] = playing;
            if (!playing) life.Problems.Add("before.second: the cue had already ended before the save could start");
            s_TrumpetId = trumpet.UniqueId; s_PlayingCue = secondCues[0]; s_PlayingId = Field<uint>(secondCues[0], "PlayingId");
            s_BeforeEndedUtc = DateTime.UtcNow;
            result["beforeEndedUtc"] = s_BeforeEndedUtc.ToString("o");
        }

        private IEnumerable<object> After()
        {
            DateTime started = DateTime.UtcNow;
            result["afterStartedUtc"] = started.ToString("o");
            if (s_TrumpetId == null || s_PlayingCue == null) { life.Problems.Add("after: no state from the before step in this process"); yield break; }
            trumpet = Game.Instance.State.Units.FirstOrDefault(u => u.UniqueId == s_TrumpetId && !u.Destroyed);
            result["trumpetLoaded"] = trumpet != null;
            if (trumpet == null) { life.Problems.Add("after: the saved Trumpet Archon is not in the loaded game"); yield break; }
            units.Add(trumpet);
            runner.GetMethod("SetExpandedSummoningBrainActive", Any).Invoke(null, new object[] { trumpet, false });
            Wake(trumpet);

            // 1. The cue that was playing at the save: ended by its native callback, naturally or after retirement.
            float wait = Time.realtimeSinceStartup + 10f;
            while (Time.realtimeSinceStartup < wait && !Field<DateTime?>(s_PlayingCue, "EndUtc").HasValue) yield return null;
            JObject saved = Describe(s_PlayingCue);
            result["savedCue"] = saved;
            DateTime? end = Field<DateTime?>(s_PlayingCue, "EndUtc");
            string stopRequested = (string)product.GetType("KingmakerGunslinger.Summoning.SummonCastCueLifecycle", true).GetField("StopRequested", Any).GetValue(null);
            string endedBy = TrumpetCueVerdict.SavedCue(s_PlayingId, Field<uint>(s_PlayingCue, "PlayingId"), Field<DateTime>(s_PlayingCue, "Utc"), end,
                Field<string>(s_PlayingCue, "StopReason"), Field<string>(s_PlayingCue, "StopResult"), Field<DateTime?>(s_PlayingCue, "StopRequestedUtc"), stopRequested);
            saved["endedBy"] = endedBy;
            saved["productStopRequested"] = stopRequested;
            saved["endedBeforeAfterStep"] = end.HasValue && end.Value <= started;
            if (!TrumpetCueVerdict.Accepted(endedBy)) life.Problems.Add("after: the cue playing at the save did not end acceptably: " + endedBy);

            // 2. No replay: nothing posted for this Trumpet since the before step ended, through a quiet interval.
            foreach (object step in Seconds(5f)) yield return step;
            object[] replays = Cues().Where(r => Field<string>(r, "CasterId") == s_TrumpetId && Field<DateTime>(r, "Utc") > s_BeforeEndedUtc).ToArray();
            result["postLoadCuesBeforeFreshBlast"] = new JArray(replays.Select(r => (object)Describe(r)).ToArray());
            if (replays.Length != 0) life.Problems.Add("after: the load posted the Trumpet cue again");

            // 3. A fresh blast from the loaded Trumpet posts exactly one cue that ends naturally.
            var fresh = new JObject(); result["fresh"] = fresh;
            int from = Cues().Length;
            foreach (object step in Cast(trumpet, fresh, false)) yield return step;
            float done = Time.realtimeSinceStartup + 15f;
            while (Time.realtimeSinceStartup < done && !CuesOf(trumpet, from).All(r => Field<DateTime?>(r, "EndUtc").HasValue)) yield return null;
            object[] freshCues = CuesOf(trumpet, from);
            fresh["cues"] = new JArray(freshCues.Select(r => (object)Describe(r)).ToArray());
            if (freshCues.Length != 1 || Field<uint>(freshCues[0], "PlayingId") == 0 || !Field<DateTime?>(freshCues[0], "EndUtc").HasValue ||
                Field<string>(freshCues[0], "StopReason") != null || Field<uint>(freshCues[0], "PlayingId") == s_PlayingId)
                life.Problems.Add("after.fresh: one new accepted cue that ends naturally was expected");
            s_TrumpetId = null; s_PlayingCue = null; s_PlayingId = 0;
        }

        // A blast started and never awaited: the probe stays yielded until the step's timeout abandons it.
        private IEnumerable<object> Abandon()
        {
            s_Abandoned = null;
            trumpet = SummonTrumpet();
            if (trumpet == null) { life.Problems.Add("abandon: the published Trumpet Archon summon produced no unit"); yield break; }
            Ability blast = Blast(trumpet);
            if (blast == null) { life.Problems.Add("abandon: no owned blast"); yield break; }
            var command = new UnitUseAbility(blast.Data, new TargetWrapper(trumpet));
            observed = command;
            owned.Hold("observed blast", () =>
            {
                observed = null; marked.Clear();
                if (!command.IsFinished) { trumpet.Commands.InterruptAll(value => ReferenceEquals(value, command)); trumpet.Commands.RemoveFinishedAndUpdateQueue(); }
            });
            trumpet.Commands.Run(command);
            result["abandonedCommandRunning"] = true;
            while (observed != null) yield return null;
        }

        private void AbandonCheck()
        {
            TrumpetCueProbe abandoned = s_Abandoned;
            s_Abandoned = null;
            if (abandoned == null) { life.Problems.Add("abandon-check: no abandoned probe in this process"); return; }
            var check = new JObject { ["subscribed"] = abandoned.subscribed, ["ownedSavesOpen"] = abandoned.marked.Count, ["observedCleared"] = abandoned.observed == null,
                ["heldLeft"] = abandoned.owned.Count, ["trumpetDestroyed"] = abandoned.trumpet == null || abandoned.trumpet.Destroyed,
                ["trumpetDestroyRequestedOnce"] = abandoned.trumpet != null && abandoned.units.WasRequested(abandoned.trumpet) && abandoned.units.Remaining.Count == 0,
                ["releasesAttempted"] = new JArray(abandoned.owned.Outcome.Attempted.Cast<object>().ToArray()),
                ["releaseFailures"] = new JArray(abandoned.owned.Outcome.Failures.Cast<object>().ToArray()) };
            result["abandoned"] = check;
            if (abandoned.subscribed || abandoned.marked.Count != 0 || abandoned.observed != null || abandoned.owned.Count != 0 || !(bool)check["trumpetDestroyed"] || !(bool)check["trumpetDestroyRequestedOnce"] ||
                abandoned.owned.Outcome.Failures.Count != 0)
                life.Problems.Add("abandon-check: the abandoned probe left its subscription, seeding, command or Trumpet behind");
        }
    }
}
