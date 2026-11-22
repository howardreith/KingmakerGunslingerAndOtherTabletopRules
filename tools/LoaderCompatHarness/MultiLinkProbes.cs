using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KmgLoaderCompatHarness
{
    /// <summary>
    /// Sprint 6 checkpoint C2 (VER-02): terminal cleanup of the multi-link hold owner (G2).
    ///
    /// Owner: a SummonGrabComponent with MaxHeldTargets > 1 (the Giant Flytrap: four bites, one held foe per bite). A
    /// grab adds the holder's multi-hold buff (SummonMultiHoldComponent) once and one held-state buff per target
    /// (Entangled, CantMove, SummonHeldComponent) whose mechanics context names the holder as caster, and records the
    /// limb in the holder's UnitPartSummonGrappleLinks. No native grapple part is involved, so the native
    /// UnitGrappleController plays no part in releasing it.
    ///
    /// Representative: the published Giant Flytrap, summoned through the product's own execution ability. Targets are
    /// Expanded Summoning horses (Large), so the published engulf rule (Medium or smaller) cannot promote a hold into
    /// G3; eligibility is read from the product's own size rules and recorded. Holds are made through the engine's
    /// RuleAttackWithWeapon with distinct grab limbs (holder BAB raised to 100 so attack and maneuver succeed;
    /// disclosed). Round callbacks are witnessed by a read-only Harmony postfix on SummonMultiHoldComponent.OnNewRound,
    /// SummonHeldComponent.OnNewRound and BuffCollection.Tick, and waits are bounded by game time, not wall time.
    ///
    /// Modes: "multilink" (terminal cases; one two-link pair left holding, captured and saved by the probe itself while
    /// paused) and "multilinkReload" (a fresh process loading that save).
    /// </summary>
    internal sealed class MultiLinkProbe : IOwnedProbe
    {
        private const BindingFlags Any = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const string UnrelatedBuffGuid = "b175001b42b1a02479881b72fe132116"; // native Bull's Strength buff
        internal const string SaveLabel = "ver02c2";
        internal const string ReloadHolderName = "KMG_C2_ReloadHolder";
        internal const string ReloadTargetPrefix = "KMG_C2_ReloadTarget";
        private const string MultiHoldName = "KMG_Summoning_Special_Grapple_MultiHold";
        private const string MultiHeldName = "KMG_Summoning_Special_Grapple_MultiHeld";

        private static readonly Dictionary<string, int> HoldRounds = new Dictionary<string, int>();
        private static readonly Dictionary<string, int> HeldRounds = new Dictionary<string, int>();
        private static readonly Dictionary<string, int> BuffTicks = new Dictionary<string, int>();
        private static readonly Dictionary<string, int> HoldTurnOffs = new Dictionary<string, int>();
        private static readonly List<JObject> Trace = new List<JObject>();
        private static readonly FieldInfo CasterReference = typeof(MechanicsContext).GetField("m_CasterReference", BindingFlags.Instance | BindingFlags.NonPublic);

        private readonly string mode;
        private readonly JObject result = new JObject();
        private readonly List<string> problems = new List<string>();
        private readonly IEnumerator<object> script;
        private readonly List<UnitEntityData> created = new List<UnitEntityData>();
        private readonly List<UnitEntityData> awakeAdded = new List<UnitEntityData>();
        private readonly HarmonyInstance harmony = HarmonyInstance.Create("KmgLoaderCompatHarness.multilink-probe");
        private readonly List<MethodBase> patched = new List<MethodBase>();
        private string verdict;

        private Assembly product;
        private Type runner, linkPart, links, grabType, holdType, multiType, heldType, limbs, profiles;
        private Vector3 anchor, caseSpot, pairSpot;

        public MultiLinkProbe(string mode)
        {
            this.mode = mode;
            result["mode"] = mode;
            script = Run().GetEnumerator();
        }

        public object Result => result;

        public string Poll()
        {
            if (verdict != null) return verdict;
            bool more;
            try { more = script.MoveNext(); }
            catch (Exception exception)
            {
                problems.Add("probe exception: " + exception);
                more = false;
            }
            if (more) return null;
            result["trace"] = new JArray(Trace.ToArray());
            result["problems"] = new JArray(problems.Cast<object>().ToArray());
            verdict = problems.Count == 0 ? string.Empty : string.Join(" | ", problems.ToArray());
            return verdict;
        }

        public CleanupReport Release(string testFailure)
        {
            return ProbeCleanup.RunAll(new[]
            {
                new KeyValuePair<string, Action>("round observers", () =>
                {
                    foreach (MethodBase method in patched) harmony.Unpatch(method, HarmonyPatchType.Postfix, harmony.Id);
                    patched.Clear();
                }),
                new KeyValuePair<string, Action>("awake registrations", () =>
                {
                    foreach (UnitEntityData unit in awakeAdded) Game.Instance.State.AwakeUnits.Remove(unit);
                    awakeAdded.Clear();
                }),
                new KeyValuePair<string, Action>("disposable units", () =>
                {
                    foreach (UnitEntityData unit in created.Where(u => u != null && !u.Destroyed).ToArray()) unit.Destroy();
                    created.Clear();
                })
            });
        }

        private IEnumerable<object> Run()
        {
            product = ProductProbes.Entry().Assembly;
            runner = product.GetType("KingmakerGunslinger.RuntimeTesting.RuntimeTestRunner", true);
            linkPart = product.GetType("KingmakerGunslinger.Summoning.UnitPartSummonGrappleLinks", true);
            links = product.GetType("KingmakerGunslinger.Summoning.SummonGrappleLinks", true);
            grabType = product.GetType("KingmakerGunslinger.Summoning.SummonGrabComponent", true);
            holdType = product.GetType("KingmakerGunslinger.Summoning.SummonHoldComponent", true);
            multiType = product.GetType("KingmakerGunslinger.Summoning.SummonMultiHoldComponent", true);
            heldType = product.GetType("KingmakerGunslinger.Summoning.SummonHeldComponent", true);
            limbs = product.GetType("KingmakerGunslinger.Summoning.SummonLimbs", true);
            profiles = product.GetType("KingmakerGunslinger.Summoning.ExpandedSummoningSpecialProfiles", true);
            bool paused = Game.Instance.IsPaused;
            result["area"] = Game.Instance.CurrentlyLoadedArea == null ? null : Game.Instance.CurrentlyLoadedArea.name;
            result["turnBased"] = TurnBased.Controllers.CombatController.IsInTurnBasedCombat();
            HoldRounds.Clear(); HeldRounds.Clear(); BuffTicks.Clear(); HoldTurnOffs.Clear(); Trace.Clear();
            Observe(holdType.GetMethods(Any).Single(m => m.Name == "MaintainLink" && m.GetParameters().Length == 7), nameof(MaintainPostfix));
            Observe(holdType.GetMethod("ReleaseLink", Any), nameof(ReleaseLinkPostfix));
            Observe(multiType.GetMethod("OnTurnOff"), nameof(HoldTurnOffPostfix));
            Observe(multiType.GetMethod("OnNewRound"), nameof(HoldRoundPostfix));
            Observe(heldType.GetMethod("OnNewRound"), nameof(HeldRoundPostfix));
            Observe(typeof(BuffCollection).GetMethod("Tick", Type.EmptyTypes), nameof(BuffTickPostfix));
            try
            {
                IEnumerable<object> body = mode == "multilink" ? Cases() : mode == "multilinkReload" ? Reload() :
                    mode == "multilinkSave" ? SaveCases() : null;
                if (body == null) { problems.Add("unknown mode " + mode); yield break; }
                foreach (object step in body) yield return step;
            }
            finally { Game.Instance.IsPaused = paused; }
        }

        // ---------------------------------------------------------------- round observers (read-only)

        private void Observe(MethodBase method, string postfix)
        {
            harmony.Patch(method, null, new HarmonyMethod(typeof(MultiLinkProbe).GetMethod(postfix, BindingFlags.Static | BindingFlags.NonPublic)));
            patched.Add(method);
        }

        private static string OwnerId(object logic)
        {
            var descriptor = logic == null ? null : logic.GetType().GetProperty("Owner", Any)?.GetValue(logic, null) as UnitDescriptor;
            return descriptor == null || descriptor.Unit == null ? null : descriptor.Unit.UniqueId;
        }

        private static void Count(Dictionary<string, int> table, string id)
        {
            if (id == null) return;
            int value;
            table.TryGetValue(id, out value);
            table[id] = value + 1;
        }

        private static void HoldRoundPostfix(object __instance) { Count(HoldRounds, OwnerId(__instance)); }
        private static void HeldRoundPostfix(object __instance) { Count(HeldRounds, OwnerId(__instance)); }
        private static void HoldTurnOffPostfix(object __instance) { Count(HoldTurnOffs, OwnerId(__instance)); }

        private static void Note(string what, UnitEntityData owner, UnitEntityData target, string detail)
        {
            if (Trace.Count > 120) return;
            string stack = new System.Diagnostics.StackTrace(2, false).GetFrames()?.Take(4)
                .Select(f => f.GetMethod() == null ? "?" : f.GetMethod().DeclaringType?.Name + "." + f.GetMethod().Name)
                .Aggregate((a, b) => a + " < " + b);
            Trace.Add(new JObject { ["what"] = what, ["holder"] = owner?.UniqueId, ["target"] = target?.UniqueId, ["detail"] = detail,
                ["gameSeconds"] = Game.Instance.TimeController.GameTime.TotalSeconds, ["stack"] = stack });
        }

        private static void MaintainPostfix(UnitEntityData owner, UnitEntityData target, string __result) { Note("MaintainLink", owner, target, __result); }
        private static void ReleaseLinkPostfix(UnitEntityData owner, UnitEntityData target) { Note("ReleaseLink", owner, target, null); }
        private static void BuffTickPostfix(BuffCollection __instance)
        {
            UnitDescriptor owner = __instance == null ? null : __instance.Owner;
            if (owner != null && owner.Unit != null) Count(BuffTicks, owner.Unit.UniqueId);
        }

        private static int Of(Dictionary<string, int> table, UnitEntityData unit)
        {
            int value;
            return unit != null && table.TryGetValue(unit.UniqueId, out value) ? value : 0;
        }

        // ---------------------------------------------------------------- cases

        private sealed class Case
        {
            internal string Name;
            internal UnitEntityData Holder;
            internal UnitEntityData[] Targets;
            internal ItemEntityWeapon[] Weapons;
            internal object Grab;
            internal BlueprintBuff HoldBuff, HeldBuff;
        }

        private IEnumerable<object> Cases()
        {
            UnitEntityData main = Game.Instance.Player.MainCharacter.Value;
            anchor = main.Position;
            Vector3 right = Quaternion.Euler(0, main.Orientation + 90f, 0) * Vector3.forward;
            caseSpot = ObstacleAnalyzer.TraceAlongNavmesh(anchor, anchor - right * 6f);
            pairSpot = ObstacleAnalyzer.TraceAlongNavmesh(anchor, anchor + right * 6f);
            result["spots"] = new JObject { ["anchor"] = anchor.ToString(), ["case"] = caseSpot.ToString(), ["pair"] = pairSpot.ToString() };
            var cases = new JObject();
            result["cases"] = cases;

            // Independent concurrent pair: a different flytrap holding one horse through every case.
            Case pair = null;
            foreach (object step in Establish("concurrentPair", 1, pairSpot, c => pair = c)) yield return step;
            if (pair == null) yield break;

            // 1. One link ends (target death) while its sibling stays on the same holder.
            Case s = null;
            foreach (object step in Establish("sibling", 2, caseSpot, c => s = c)) yield return step;
            if (s == null) yield break;
            Buff unrelated = AddUnrelated(s.Targets[1]);
            var row = new JObject { ["terminal"] = "GameHelper.KillUnit(first target)", ["before"] = State(s), ["pairBefore"] = State(pair) };
            cases["siblingTargetDeath"] = row;
            int holdRounds = Of(HoldRounds, s.Holder);
            GameHelper.KillUnit(s.Targets[0]);
            foreach (object step in SettleGameTime(7.0, 30f, () => Of(HoldRounds, s.Holder) > holdRounds)) yield return step;
            row["holderRoundsWitnessed"] = Of(HoldRounds, s.Holder) - holdRounds;
            row["after"] = State(s);
            row["pairAfter"] = State(pair);
            row["unrelatedKept"] = unrelated != null && s.Targets[1].Descriptor.Buffs.RawFacts.Contains(unrelated);
            if ((int)row["holderRoundsWitnessed"] < 1) problems.Add("siblingTargetDeath: no holder OnNewRound was witnessed after the death");
            ExpectEnded(s, 0, row, "siblingTargetDeath", true);
            ExpectHeld(s, 1, "siblingTargetDeath.sibling");
            ExpectHolding(s, "siblingTargetDeath");
            ExpectHeld(pair, 0, "siblingTargetDeath.pair");
            if (!(bool)row["unrelatedKept"]) problems.Add("siblingTargetDeath: the unrelated effect on the sibling was not kept");
            // Held target cannot walk (control for the behavior check below).
            row["siblingMoveControl"] = Move(s.Targets[1]);
            foreach (object step in SettleGameTime(1.5, 6f, () => false)) yield return step;
            row["siblingMoveControl"] = Moved((JObject)row["siblingMoveControl"], s.Targets[1]);

            // 1a. Consequence of the ended link on the surviving holder: the freed limb takes a new target through a real
            // bite, while the sibling stays held.
            row = new JObject { ["terminal"] = "consequence: a new horse bitten by the freed limb", ["before"] = State(s) };
            cases["freedLimbRegrab"] = row;
            UnitEntityData third = Summon(main, "horse");
            if (third == null) { problems.Add("freedLimbRegrab: the third horse was not summoned"); yield break; }
            third.Descriptor.Stats.HitPoints.BaseValue = 100000;
            Place(third, ObstacleAnalyzer.TraceAlongNavmesh(s.Holder.Position, s.Holder.Position + s.Holder.OrientationDirection * 3.0f));
            WakeUnit(third);
            yield return null;
            row["thirdEngulfSizeAllowed"] = (bool)grabType.GetMethod("IsSwallowSizeAllowed", Any).Invoke(s.Grab, new object[] { s.Holder, third });
            var regrab = new Case { Name = "regrab", Holder = s.Holder, Targets = new[] { third }, Weapons = new[] { s.Weapons[0] }, Grab = s.Grab, HoldBuff = s.HoldBuff, HeldBuff = s.HeldBuff };
            int bites = 0;
            for (; bites < 6 && !IsHeldBy(regrab, 0); bites++)
            {
                Rulebook.Trigger(new RuleAttackWithWeapon(s.Holder, third, s.Weapons[0], 0));
                yield return null;
            }
            row["bites"] = bites;
            row["after"] = State(s);
            row["third"] = State(regrab);
            ExpectHeld(regrab, 0, "freedLimbRegrab.third");
            ExpectHeld(s, 1, "freedLimbRegrab.sibling");
            row["staleRecordForDeadTargetAfterRegrab"] = RawLinks(s.Holder) == null ? 0 : RawLinks(s.Holder).Count(l => l.StartsWith(s.Targets[0].UniqueId + "|", StringComparison.Ordinal));
            s.Targets = new[] { s.Targets[0], s.Targets[1], third };
            s.Weapons = new[] { s.Weapons[0], s.Weapons[1], s.Weapons[0] };

            // 1b. A second real terminal on the same holder: its death releases the remaining links.
            row = new JObject { ["terminal"] = "second terminal: GameHelper.KillUnit(holder) with one remaining link", ["before"] = State(s) };
            cases["siblingHolderDeath"] = row;
            int heldRounds = Of(HeldRounds, s.Targets[1]);
            GameHelper.KillUnit(s.Holder);
            foreach (object step in SettleGameTime(1.0, 30f, () => TargetFree(s, 1) && TargetFree(s, 2))) yield return step;
            row["after"] = State(s);
            row["siblingHeldRoundsWitnessed"] = Of(HeldRounds, s.Targets[1]) - heldRounds;
            ExpectEnded(s, 1, row, "siblingHolderDeath", false);
            ExpectEnded(s, 2, row, "siblingHolderDeath", false);
            ExpectHeld(pair, 0, "siblingHolderDeath.pair");

            // 1c. Controls only: the shared release helper again on the ended links.
            row = new JObject { ["terminal"] = "control: SummonHoldComponent.ReleaseLink and SummonGrappleLinks.Release x3 on the ended links" };
            cases["repeatedReleaseControl"] = row;
            try
            {
                MethodInfo releaseLink = holdType.GetMethod("ReleaseLink", Any);
                MethodInfo release = links.GetMethod("Release", Any);
                for (int index = 0; index < 3; index++)
                    foreach (UnitEntityData target in s.Targets)
                    {
                        releaseLink.Invoke(null, new object[] { s.Holder, target, s.Grab, true });
                        release.Invoke(null, new object[] { s.Holder, target });
                    }
            }
            catch (Exception exception) { row["error"] = (exception.InnerException ?? exception).ToString(); problems.Add("repeatedReleaseControl threw"); }
            row["after"] = State(s);
            ExpectHeld(pair, 0, "repeatedReleaseControl.pair");
            Dispose(s);

            // 2. Holder death with two links: both targets released; one of them then walks.
            Case d = null;
            foreach (object step in Establish("holderDeath", 2, caseSpot, c => d = c)) yield return step;
            if (d == null) yield break;
            row = new JObject { ["terminal"] = "GameHelper.KillUnit(holder)", ["before"] = State(d) };
            cases["holderDeath"] = row;
            GameHelper.KillUnit(d.Holder);
            foreach (object step in SettleGameTime(1.0, 30f, () => TargetFree(d, 0) && TargetFree(d, 1))) yield return step;
            row["after"] = State(d);
            ExpectEnded(d, 0, row, "holderDeath", false);
            ExpectEnded(d, 1, row, "holderDeath", false);
            row["releasedMove"] = Move(d.Targets[0]);
            row["pairMoveControl"] = Move(pair.Targets[0]);
            foreach (object step in SettleGameTime(1.5, 6f, () => false)) yield return step;
            row["releasedMove"] = Moved((JObject)row["releasedMove"], d.Targets[0]);
            row["pairMoveControl"] = Moved((JObject)row["pairMoveControl"], pair.Targets[0]);
            if ((double)row["releasedMove"]["moved"] < 0.5) problems.Add("holderDeath: the released target did not walk: " + row["releasedMove"]);
            if ((double)row["pairMoveControl"]["moved"] > 0.25) problems.Add("holderDeath: the still-held pair target walked: " + row["pairMoveControl"]);
            ExpectHeld(pair, 0, "holderDeath.pair");
            Dispose(d);

            // 3. Accelerated summon expiry with two links.
            Case e = null;
            foreach (object step in Establish("summonExpiry", 2, caseSpot, c => e = c)) yield return step;
            if (e == null) yield break;
            row = new JObject { ["terminal"] = "SummonedUnitBuff deadline shortened, then explicit native BuffCollection.UpdateNextEvent/Tick", ["before"] = State(e) };
            cases["summonExpiry"] = row;
            row["expiry"] = Expire(e.Holder);
            foreach (object step in SettleGameTime(1.0, 30f, () => TargetFree(e, 0) && TargetFree(e, 1) && e.Holder.Destroyed)) yield return step;
            row["after"] = State(e);
            row["holderDestroyed"] = e.Holder.Destroyed;
            ExpectEnded(e, 0, row, "summonExpiry", false);
            ExpectEnded(e, 1, row, "summonExpiry", false);
            if (!e.Holder.Destroyed) problems.Add("summonExpiry: the summon was not destroyed after its native expiry");
            ExpectHeld(pair, 0, "summonExpiry.pair");
            Dispose(e);

            cases["pairFinal"] = State(pair);
            ExpectHeld(pair, 0, "final.pair");

            // 4. A two-link hold left active, captured while paused and saved by the probe itself.
            Case r = null;
            foreach (object step in Establish("reload", 2, caseSpot, c => r = c)) yield return step;
            if (r == null) yield break;
            Dispose(pair);
            r.Holder.Descriptor.CustomName = ReloadHolderName;
            for (int index = 0; index < r.Targets.Length; index++) r.Targets[index].Descriptor.CustomName = ReloadTargetPrefix + index;
            foreach (UnitEntityData unit in new[] { r.Holder }.Concat(r.Targets)) { created.Remove(unit); awakeAdded.Remove(unit); }
            Game.Instance.IsPaused = true;
            yield return null;
            cases["reloadAtSave"] = State(r);
            ExpectHeld(r, 0, "reloadAtSave");
            ExpectHeld(r, 1, "reloadAtSave");
            bool saved = false;
            int turnOffsBefore = Of(HoldTurnOffs, r.Holder);
            SaveInfo info = Game.Instance.SaveManager.CreateNewSave(SaveLabel);
            TimeSpan clock = Game.Instance.TimeController.GameTime;
            Game.Instance.SaveGame(info, () => saved = true);
            float until = Time.realtimeSinceStartup + 120f;
            while ((!saved || Game.Instance.SaveManager.CommitInProgress || LoadingProcess.Instance.IsLoadingInProcess) && Time.realtimeSinceStartup < until)
                yield return null;
            result["save"] = new JObject { ["completed"] = saved, ["label"] = SaveLabel, ["pausedThroughout"] = Game.Instance.IsPaused,
                ["gameClockUnchanged"] = Game.Instance.TimeController.GameTime == clock,
                ["multiHoldTurnOffsDuringSave"] = Of(HoldTurnOffs, r.Holder) - turnOffsBefore };
            if (!saved) problems.Add("the probe's save did not complete");
            // Captured, not asserted: the live session right after the save, and one round later.
            cases["reloadAfterSave"] = State(r);
            foreach (object step in SettleGameTime(7.0, 30f, () => false)) yield return step;
            Game.Instance.IsPaused = true;
            cases["reloadAfterSaveOneRound"] = State(r);
        }

        // ---------------------------------------------------------------- save suspension (C2a)

        internal const string SaveSuspensionLabel = "ver02c2ag2";

        /// <summary>
        /// C2a: a two-link hold through three native saves (asserted preserved with no drift), a round of ordinary
        /// maintenance, a sibling terminal, accelerated expiry; then a fresh two-link hold saved for the reload launch.
        /// </summary>
        private IEnumerable<object> SaveCases()
        {
            UnitEntityData main = Game.Instance.Player.MainCharacter.Value;
            anchor = main.Position;
            Vector3 right = Quaternion.Euler(0, main.Orientation + 90f, 0) * Vector3.forward;
            caseSpot = ObstacleAnalyzer.TraceAlongNavmesh(anchor, anchor - right * 6f);
            var cases = new JObject();
            result["cases"] = cases;

            Case s = null;
            foreach (object step in Establish("saveSuspension", 2, caseSpot, c => s = c)) yield return step;
            if (s == null) yield break;
            Buff unrelated = AddUnrelated(s.Targets[1]);
            Game.Instance.IsPaused = true;
            yield return null;
            JObject first = State(s);
            cases["beforeSaves"] = first;
            var saves = new JArray();
            cases["saves"] = saves;
            for (int index = 0; index < 3; index++)
            {
                JObject before = State(s);
                int turnOffs = Of(HoldTurnOffs, s.Holder);
                bool done = false;
                SaveInfo info = Game.Instance.SaveManager.CreateNewSave("ver02c2ag2s" + index);
                TimeSpan clock = Game.Instance.TimeController.GameTime;
                Game.Instance.SaveGame(info, () => done = true);
                float until = Time.realtimeSinceStartup + 120f;
                while ((!done || Game.Instance.SaveManager.CommitInProgress || LoadingProcess.Instance.IsLoadingInProcess) && Time.realtimeSinceStartup < until)
                    yield return null;
                JObject after = State(s);
                saves.Add(new JObject { ["index"] = index, ["completed"] = done, ["gameClockUnchanged"] = Game.Instance.TimeController.GameTime == clock,
                    ["holdTurnOffCallsDuringSave"] = Of(HoldTurnOffs, s.Holder) - turnOffs, ["after"] = after });
                if (!done) { problems.Add("saveSuspension: save " + index + " did not complete"); yield break; }
                ExpectHeld(s, 0, "saveSuspension.save" + index);
                ExpectHeld(s, 1, "saveSuspension.save" + index);
                if (!JToken.DeepEquals(before["holder"]["raw.links"], after["holder"]["raw.links"])) problems.Add("saveSuspension: save " + index + " changed the raw records");
                for (int t = 0; t < 2; t++)
                {
                    JObject tb = TargetRow(before, t), ta = TargetRow(after, t);
                    if ((int)tb["damage"] != (int)ta["damage"]) problems.Add("saveSuspension: save " + index + " changed target " + t + " damage");
                    if ((int)tb["heldRounds"] != (int)ta["heldRounds"]) problems.Add("saveSuspension: save " + index + " ran a held round");
                    if (((JArray)ta["heldStates"]).Count != 1) problems.Add("saveSuspension: save " + index + " duplicated or removed a held state on target " + t);
                }
                if ((int)after["holder"]["holdRounds"] != (int)before["holder"]["holdRounds"]) problems.Add("saveSuspension: save " + index + " ran a hold round");
            }
            if (problems.Count > 0) yield break;
            row(cases, "unrelatedKeptAfterSaves", unrelated != null && s.Targets[1].Descriptor.Buffs.RawFacts.Contains(unrelated));

            // Ordinary play resumes: one holder round passes, both links maintained.
            int rounds = Of(HoldRounds, s.Holder);
            foreach (object step in SettleGameTime(7.0, 30f, () => Of(HoldRounds, s.Holder) > rounds)) yield return step;
            Game.Instance.IsPaused = true;
            cases["afterRound"] = State(s);
            if (Of(HoldRounds, s.Holder) <= rounds) problems.Add("saveSuspension: no holder round followed the saves");
            ExpectHeld(s, 0, "saveSuspension.afterRound");
            ExpectHeld(s, 1, "saveSuspension.afterRound");

            // Genuine terminals still release: one target dies (sibling kept), then accelerated expiry.
            var r1 = new JObject();
            cases["siblingTargetDeath"] = r1;
            rounds = Of(HoldRounds, s.Holder);
            GameHelper.KillUnit(s.Targets[0]);
            foreach (object step in SettleGameTime(7.0, 30f, () => Of(HoldRounds, s.Holder) > rounds)) yield return step;
            Game.Instance.IsPaused = true;
            r1["after"] = State(s);
            ExpectEnded(s, 0, r1, "saveSuspension.siblingTargetDeath", true);
            ExpectHeld(s, 1, "saveSuspension.siblingTargetDeath.sibling");
            var r2 = new JObject();
            cases["summonExpiry"] = r2;
            r2["expiry"] = Expire(s.Holder);
            foreach (object step in SettleGameTime(1.0, 30f, () => TargetFree(s, 1) && s.Holder.Destroyed)) yield return step;
            Game.Instance.IsPaused = true;
            r2["after"] = State(s);
            ExpectEnded(s, 1, r2, "saveSuspension.summonExpiry", false);
            if (!s.Holder.Destroyed) problems.Add("saveSuspension: the holder was not destroyed after its expiry");
            Dispose(s);
            if (problems.Count > 0) yield break;

            // A fresh two-link hold, saved for the reload launch.
            Case r = null;
            foreach (object step in Establish("reload", 2, caseSpot, c => r = c)) yield return step;
            if (r == null) yield break;
            r.Holder.Descriptor.CustomName = ReloadHolderName;
            for (int index = 0; index < r.Targets.Length; index++) r.Targets[index].Descriptor.CustomName = ReloadTargetPrefix + index;
            foreach (UnitEntityData unit in new[] { r.Holder }.Concat(r.Targets)) { created.Remove(unit); awakeAdded.Remove(unit); }
            Game.Instance.IsPaused = true;
            yield return null;
            cases["reloadAtSave"] = State(r);
            bool saved = false;
            Game.Instance.SaveGame(Game.Instance.SaveManager.CreateNewSave(SaveSuspensionLabel), () => saved = true);
            float deadline = Time.realtimeSinceStartup + 120f;
            while ((!saved || Game.Instance.SaveManager.CommitInProgress || LoadingProcess.Instance.IsLoadingInProcess) && Time.realtimeSinceStartup < deadline)
                yield return null;
            cases["reloadAfterSave"] = State(r);
            if (!saved) problems.Add("the reload save did not complete");
            ExpectHeld(r, 0, "reloadAfterSave");
            ExpectHeld(r, 1, "reloadAfterSave");
        }

        private static void row(JObject cases, string key, bool value) { cases[key] = value; }

        // ---------------------------------------------------------------- reload

        private IEnumerable<object> Reload()
        {
            UnitEntityData holder = Game.Instance.State.Units.FirstOrDefault(u => u.Descriptor.CustomName == ReloadHolderName);
            UnitEntityData[] targets = Enumerable.Range(0, 2).Select(i => Game.Instance.State.Units.FirstOrDefault(u => u.Descriptor.CustomName == ReloadTargetPrefix + i)).ToArray();
            if (holder == null || targets.Any(t => t == null)) { problems.Add("reload: the named units are missing"); yield break; }
            created.Add(holder);
            created.AddRange(targets);
            object grab = grabType.GetMethod("Find", Any).Invoke(null, new object[] { holder });
            var c = new Case
            {
                Name = "reload", Holder = holder, Targets = targets, Grab = grab,
                HoldBuff = grab == null ? BuffNamed(MultiHoldName) : (BlueprintBuff)grabType.GetField("HoldBuff").GetValue(grab),
                HeldBuff = grab == null ? BuffNamed(MultiHeldName) : (BlueprintBuff)grabType.GetField("GrappledBuff").GetValue(grab),
                Weapons = GrabWeapons(holder, grab)
            };
            var row = new JObject { ["atLoad"] = State(c) };
            result["reload"] = row;
            ExpectReset(c, "reload.atLoad");
            int[] damage = targets.Select(t => t.Descriptor.Damage).ToArray();
            foreach (UnitEntityData unit in new[] { holder }.Concat(targets)) WakeUnit(unit);
            int[] ticks = new[] { holder }.Concat(targets).Select(u => Of(BuffTicks, u)).ToArray();
            TimeSpan start = Game.Instance.TimeController.GameTime;
            foreach (object step in SettleGameTime(13.0, 40f, () => false)) yield return step;
            double elapsed = (Game.Instance.TimeController.GameTime - start).TotalSeconds;
            row["afterRounds"] = State(c);
            row["gameSecondsElapsed"] = elapsed;
            row["buffCollectionTicks"] = new JArray(new[] { holder }.Concat(targets).Select((u, i) => (object)(Of(BuffTicks, u) - ticks[i])).ToArray());
            row["holdRounds"] = Of(HoldRounds, holder);
            row["heldRounds"] = new JArray(targets.Select(t => (object)Of(HeldRounds, t)).ToArray());
            row["damageDelta"] = new JArray(targets.Select((t, i) => (object)(t.Descriptor.Damage - damage[i])).ToArray());
            if (elapsed < 12.0) problems.Add("reload: fewer than two rounds of game time elapsed (" + elapsed + " s)");
            if (((JArray)row["buffCollectionTicks"]).Any(v => (int)v < 1)) problems.Add("reload: a unit's buff collection was not ticked by the engine");
            if (((JArray)row["damageDelta"]).Any(v => (int)v != 0)) problems.Add("reload: a target took damage after the reset");
            ExpectReset(c, "reload.afterRounds");
        }

        // ---------------------------------------------------------------- fixture

        private IEnumerable<object> Establish(string name, int linkCount, Vector3 spot, Action<Case> done)
        {
            UnitEntityData main = Game.Instance.Player.MainCharacter.Value;
            UnitEntityData holder = Summon(main, "giant-flytrap");
            if (holder == null) { problems.Add(name + ": the flytrap was not summoned"); yield break; }
            Place(holder, spot);
            var targets = new List<UnitEntityData>();
            for (int index = 0; index < linkCount; index++)
            {
                UnitEntityData horse = Summon(main, "horse");
                if (horse == null) { problems.Add(name + ": a horse was not summoned"); yield break; }
                horse.Descriptor.Stats.HitPoints.BaseValue = 100000;
                float angle = holder.Orientation + (linkCount == 1 ? 0f : index == 0 ? -45f : 45f);
                Place(horse, ObstacleAnalyzer.TraceAlongNavmesh(holder.Position, holder.Position + Quaternion.Euler(0, angle, 0) * Vector3.forward * 3.0f));
                targets.Add(horse);
            }
            holder.Descriptor.Stats.BaseAttackBonus.BaseValue = 100;
            foreach (UnitEntityData unit in new[] { holder }.Concat(targets)) WakeUnit(unit);
            yield return null;
            object grab = grabType.GetMethod("Find", Any).Invoke(null, new object[] { holder });
            if (grab == null) { problems.Add(name + ": the flytrap has no grab component"); yield break; }
            var c = new Case
            {
                Name = name, Holder = holder, Targets = targets.ToArray(), Grab = grab,
                HoldBuff = (BlueprintBuff)grabType.GetField("HoldBuff").GetValue(grab),
                HeldBuff = (BlueprintBuff)grabType.GetField("GrappledBuff").GetValue(grab),
                Weapons = GrabWeapons(holder, grab)
            };
            int holderSize = (int)holder.Descriptor.State.Size;
            var baseline = new JObject
            {
                ["holderBlueprint"] = holder.Blueprint.name, ["holderSize"] = holder.Descriptor.State.Size.ToString(),
                ["multiLink"] = (bool)grabType.GetProperty("MultiLink", Any).GetValue(grab, null),
                ["maxHeld"] = (int)grabType.GetField("MaxHeldTargets").GetValue(grab),
                ["grabWeapons"] = new JArray(c.Weapons.Select(w => (object)(w == null ? null : w.Blueprint.name)).ToArray()),
                ["holderCantAct"] = holder.Descriptor.State.HasCondition(UnitCondition.CantAct),
                ["holderCantMove"] = holder.Descriptor.State.HasCondition(UnitCondition.CantMove),
                ["holderBuffs"] = Buffs(holder),
                ["targets"] = new JArray(targets.Select(t => (object)new JObject
                {
                    ["id"] = t.UniqueId, ["blueprint"] = t.Blueprint.name, ["size"] = t.Descriptor.State.Size.ToString(),
                    ["grabSizeAllowed"] = (bool)profiles.GetMethod("IsGrabSizeAllowed", Any).Invoke(null, new object[] { (int)t.Descriptor.State.Size, holderSize, (int)grabType.GetField("MaxTargetSizeDelta").GetValue(grab) }),
                    ["engulfSizeAllowed"] = (bool)grabType.GetMethod("IsSwallowSizeAllowed", Any).Invoke(grab, new object[] { holder, t }),
                    ["cantMove"] = t.Descriptor.State.HasCondition(UnitCondition.CantMove),
                    ["cantAct"] = t.Descriptor.State.HasCondition(UnitCondition.CantAct),
                    ["entangled"] = t.Descriptor.State.HasCondition(UnitCondition.Entangled),
                    ["distance"] = Vector3.Distance(holder.Position, t.Position),
                    ["buffs"] = Buffs(t)
                }).ToArray())
            };
            result["baseline." + name] = baseline;
            if (!(bool)baseline["multiLink"] || c.Weapons.Length < linkCount || (bool)baseline["holderCantAct"] || (bool)baseline["holderCantMove"] ||
                ((JArray)baseline["targets"]).Any(t => !(bool)t["grabSizeAllowed"] || (bool)t["engulfSizeAllowed"] || (bool)t["cantMove"] || (bool)t["cantAct"] || (bool)t["entangled"]))
            {
                problems.Add(name + ": fixture baseline is not admissible: " + baseline.ToString(Formatting.None));
                yield break;
            }
            for (int index = 0; index < linkCount; index++)
            {
                int attacks = 0;
                for (; attacks < 6 && !IsHeldBy(c, index); attacks++)
                {
                    Rulebook.Trigger(new RuleAttackWithWeapon(holder, c.Targets[index], c.Weapons[index], 0));
                    yield return null;
                }
                baseline["attacks" + index] = attacks;
            }
            JObject state = State(c);
            result["established." + name] = state;
            for (int index = 0; index < linkCount; index++) ExpectHeld(c, index, name + ".established");
            if (problems.Any(p => p.StartsWith(name + ".established", StringComparison.Ordinal))) yield break;
            done(c);
        }

        private ItemEntityWeapon[] GrabWeapons(UnitEntityData holder, object grab)
        {
            var result = new List<ItemEntityWeapon>();
            if (grab == null) return result.ToArray();
            MethodInfo isGrabLimb = grabType.GetMethod("IsGrabLimb", Any);
            var primary = (ItemEntityWeapon)limbs.GetMethod("PrimaryWeapon", Any).Invoke(null, new object[] { holder });
            if (primary != null && (bool)isGrabLimb.Invoke(grab, new object[] { holder, primary })) result.Add(primary);
            if (holder.Body.AdditionalLimbs != null)
                foreach (var slot in holder.Body.AdditionalLimbs)
                    if (slot != null && slot.MaybeWeapon != null && (bool)isGrabLimb.Invoke(grab, new object[] { holder, slot.MaybeWeapon }))
                        result.Add(slot.MaybeWeapon);
            return result.ToArray();
        }

        private void Place(UnitEntityData unit, Vector3 position)
        {
            runner.GetMethod("PlaceExpandedSummoningUnit", Any).Invoke(null, new object[] { unit, position });
        }

        private void WakeUnit(UnitEntityData unit)
        {
            unit.Wake(300f);
            if (!Game.Instance.State.AwakeUnits.Contains(unit)) { Game.Instance.State.AwakeUnits.Add(unit); awakeAdded.Add(unit); }
        }

        private UnitEntityData Summon(UnitEntityData caster, string key)
        {
            Type multiplicity = product.GetType("KingmakerGunslinger.Summoning.SummonMultiplicity", true);
            object variant = runner.GetMethod("ExpandedSummoningOwnTierVariant", Any).Invoke(null, new[] { key, Enum.Parse(multiplicity, "One") });
            object evidence = Activator.CreateInstance(runner.GetNestedType("ExpandedSummoningMechanicalEvidence", Any), true);
            var library = (LibraryScriptableObject)product.GetType("KingmakerGunslinger.Bootstrap.BlueprintBootstrap", true)
                .GetProperty("Library", Any).GetValue(null, null);
            BlueprintScriptableObject[] all = library.GetAllBlueprints().ToArray();
            var units = (UnitEntityData[])runner.GetMethods(Any).Single(m => m.Name == "CastExpandedSummoningVariant" && m.GetParameters().Length == 5)
                .Invoke(null, new object[] { all, caster, variant, null, evidence });
            UnitEntityData unit = units.Length == 1 ? units[0] : null;
            if (unit == null) return null;
            created.Add(unit);
            runner.GetMethod("RemoveExpandedSummoningAppearanceBuffs", Any).Invoke(null, new object[] { unit });
            runner.GetMethod("SetExpandedSummoningBrainActive", Any).Invoke(null, new object[] { unit, false });
            return unit;
        }

        private void Dispose(Case c)
        {
            foreach (UnitEntityData unit in new[] { c.Holder }.Concat(c.Targets))
                if (unit != null && !unit.Destroyed) unit.Destroy();
        }

        private Buff AddUnrelated(UnitEntityData target)
        {
            var blueprint = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>(UnrelatedBuffGuid);
            if (blueprint == null) { problems.Add("the unrelated control buff blueprint is missing"); return null; }
            return target.Descriptor.Buffs.AddBuff(blueprint, target, TimeSpan.FromMinutes(10));
        }

        private JObject Expire(UnitEntityData owner)
        {
            Buff marker = owner.Descriptor.Buffs.Enumerable.FirstOrDefault(value => ReferenceEquals(value.Blueprint, BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff));
            if (marker == null) { problems.Add("no SummonedUnitBuff on " + owner.UniqueId); return new JObject(); }
            TimeSpan previous = marker.EndTime;
            TimeSpan now = Game.Instance.TimeController.GameTime;
            typeof(Buff).GetProperty("EndTime", Any).SetValue(marker, now - TimeSpan.FromMilliseconds(1), null);
            owner.Descriptor.Buffs.UpdateNextEvent();
            owner.Descriptor.Buffs.Tick();
            return new JObject
            {
                ["previousRemainingSeconds"] = (previous - now).TotalSeconds,
                ["attachedAfterNativeTick"] = owner.Descriptor.Buffs.RawFacts.Contains(marker)
            };
        }

        /// <summary>Unpaused until at least the given game time has passed and the condition holds, or the wall budget ends.</summary>
        private IEnumerable<object> SettleGameTime(double gameSeconds, float wallBudget, Func<bool> done)
        {
            bool wasPaused = Game.Instance.IsPaused;
            TimeSpan start = Game.Instance.TimeController.GameTime;
            float until = Time.realtimeSinceStartup + wallBudget;
            while (Time.realtimeSinceStartup < until)
            {
                if (Game.Instance.IsPaused) Game.Instance.IsPaused = false;
                if ((Game.Instance.TimeController.GameTime - start).TotalSeconds >= gameSeconds && done()) break;
                yield return null;
            }
            Game.Instance.IsPaused = wasPaused;
        }

        private static JObject Move(UnitEntityData unit)
        {
            Vector3 from = unit.Position;
            Vector3 to = ObstacleAnalyzer.TraceAlongNavmesh(from, from + unit.OrientationDirection * -3f);
            var command = new UnitMoveTo(to, 0.3f);
            unit.Commands.Run(command);
            return new JObject { ["from"] = from.ToString(), ["to"] = to.ToString(), ["requested"] = Vector3.Distance(from, to),
                ["canMoveFlag"] = unit.Descriptor.State.CanMove, ["x"] = from.x, ["z"] = from.z };
        }

        private static JObject Moved(JObject start, UnitEntityData unit)
        {
            var from = new Vector3((float)start["x"], unit.Position.y, (float)start["z"]);
            start["moved"] = (double)Vector3.Distance(from, unit.Position);
            start["at"] = unit.Position.ToString();
            unit.Commands.InterruptAll();
            return start;
        }

        private static BlueprintBuff BuffNamed(string name)
        {
            return ResourcesLibrary.LibraryObject.GetAllBlueprints().OfType<BlueprintBuff>().FirstOrDefault(b => b.name == name);
        }

        // ---------------------------------------------------------------- observation

        private Buff[] HeldStates(Case c, UnitEntityData target)
        {
            return target.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(b => ReferenceEquals(b.Blueprint, c.HeldBuff)).ToArray();
        }

        private bool IsHeldBy(Case c, int index)
        {
            return HeldStates(c, c.Targets[index]).Any(b => b.Context != null && ReferenceEquals(b.Context.MaybeCaster, c.Holder));
        }

        private bool TargetFree(Case c, int index)
        {
            UnitEntityData t = c.Targets[index];
            return HeldStates(c, t).Length == 0 && !t.Descriptor.State.HasCondition(UnitCondition.Entangled) &&
                !t.Descriptor.State.HasCondition(UnitCondition.CantMove);
        }

        private static string SavedCaster(MechanicsContext context)
        {
            object reference = context == null ? null : CasterReference.GetValue(context);
            return reference is UnitReference ? ((UnitReference)reference).UniqueId : null;
        }

        private static JArray Buffs(UnitEntityData unit)
        {
            return new JArray(unit.Descriptor.Buffs.RawFacts.OfType<Buff>().Select(x => (object)x.Blueprint.name).ToArray());
        }

        private IList<string> RawLinks(UnitEntityData holder)
        {
            if (holder.Descriptor == null) return null;
            object part = typeof(UnitDescriptor).GetMethods().Single(m => m.Name == "Get" && m.IsGenericMethodDefinition && m.GetParameters().Length == 0)
                .MakeGenericMethod(linkPart).Invoke(holder.Descriptor, null);
            return part == null ? null : (IList<string>)linkPart.GetField("_links", Any).GetValue(part);
        }

        /// <summary>Raw stored records, filtered live answers, limb occupancy, buffs and conditions, read separately.</summary>
        private JObject State(Case c)
        {
            UnitEntityData h = c.Holder;
            IList<string> raw = RawLinks(h);
            var reconciled = ((IEnumerable)links.GetMethod("Reconcile", Any).Invoke(null, new object[] { h })).Cast<object>()
                .Select(r => (object)(r.GetType().GetProperty("TargetId").GetValue(r, null) + "|" + r.GetType().GetProperty("Limb").GetValue(r, null) + "|" +
                    r.GetType().GetProperty("AdditionalIndex").GetValue(r, null))).ToArray();
            MethodInfo occupant = links.GetMethod("OccupantOf", Any);
            var occupancy = new JArray(c.Weapons.Select(w => (object)new JObject
            {
                ["weapon"] = w == null ? null : w.Blueprint.name,
                ["limb"] = LimbKey(h, w),
                ["occupant"] = w == null ? null : (occupant.Invoke(null, new object[] { h, w }) as UnitEntityData)?.UniqueId
            }).ToArray());
            var holderState = new JObject
            {
                ["id"] = h.UniqueId, ["dead"] = h.Descriptor.State.IsDead, ["destroyed"] = h.Destroyed,
                ["holdBuff"] = h.Descriptor.Buffs.RawFacts.OfType<Buff>().Count(b => ReferenceEquals(b.Blueprint, c.HoldBuff)),
                ["nativeInitiatorPart"] = h.Get<UnitPartGrappleInitiator>() != null,
                ["cantAct"] = h.Descriptor.State.HasCondition(UnitCondition.CantAct),
                ["cantMove"] = h.Descriptor.State.HasCondition(UnitCondition.CantMove),
                ["raw.linkPart"] = raw != null,
                ["raw.links"] = raw == null ? null : new JArray(raw.Cast<object>().ToArray()),
                ["live.reconciled"] = new JArray(reconciled),
                ["live.heldCount"] = c.Grab == null ? -1 : (int)grabType.GetMethod("HeldCount", Any).Invoke(c.Grab, new object[] { h }),
                ["live.occupancy"] = occupancy,
                ["holdRounds"] = Of(HoldRounds, h),
                ["buffs"] = h.Destroyed ? null : Buffs(h)
            };
            var targets = new JArray();
            foreach (UnitEntityData t in c.Targets)
            {
                UnitState st = t.Descriptor.State;
                targets.Add(new JObject
                {
                    ["id"] = t.UniqueId, ["dead"] = st.IsDead, ["destroyed"] = t.Destroyed,
                    ["heldStates"] = new JArray(HeldStates(c, t).Select(b => (object)new JObject
                    {
                        ["caster"] = b.Context == null || b.Context.MaybeCaster == null ? null : b.Context.MaybeCaster.UniqueId,
                        ["savedCaster"] = b.Context == null ? null : SavedCaster(b.Context)
                    }).ToArray()),
                    ["rawRecords"] = raw == null ? 0 : raw.Count(l => l != null && l.StartsWith(t.UniqueId + "|", StringComparison.Ordinal)),
                    ["nativeTargetPart"] = t.Get<UnitPartGrappleTarget>() != null,
                    ["entangled"] = st.HasCondition(UnitCondition.Entangled),
                    ["cantMove"] = st.HasCondition(UnitCondition.CantMove),
                    ["cantAct"] = st.HasCondition(UnitCondition.CantAct),
                    ["canMoveFlag"] = st.CanMove, ["canActFlag"] = st.CanAct,
                    ["damage"] = t.Descriptor.Damage,
                    ["heldRounds"] = Of(HeldRounds, t),
                    ["buffs"] = Buffs(t)
                });
            }
            return new JObject { ["holder"] = holderState, ["targets"] = targets, ["gameSeconds"] = Game.Instance.TimeController.GameTime.TotalSeconds };
        }

        /// <summary>The product's own limb classification of a weapon: "kind|additionalIndex", as records store it.</summary>
        private string LimbKey(UnitEntityData holder, ItemEntityWeapon weapon)
        {
            if (weapon == null) return null;
            var args = new object[] { holder, weapon, -1 };
            object kind = limbs.GetMethod("Classify", Any).Invoke(null, args);
            return (int)kind + "|" + (int)args[2];
        }

        private JObject TargetRow(JObject s, int index) { return (JObject)((JArray)s["targets"])[index]; }

        /// <summary>Link index holds: exact holder context, one held state, one raw record on its own limb, occupancy, conditions, no native parts.</summary>
        private void ExpectHeld(Case c, int index, string name)
        {
            JObject s = State(c);
            JObject t = TargetRow(s, index);
            var held = (JArray)t["heldStates"];
            string id = c.Holder.UniqueId;
            ItemEntityWeapon weapon = c.Weapons[index];
            string occupant = (string)((JArray)s["holder"]["live.occupancy"])[index]["occupant"];
            var raw = (JArray)s["holder"]["raw.links"];
            string record = raw == null ? null : raw.Select(x => (string)x).FirstOrDefault(x => x.StartsWith(c.Targets[index].UniqueId + "|", StringComparison.Ordinal));
            string limb = LimbKey(c.Holder, weapon);
            if (held.Count != 1 || (string)held[0]["caster"] != id || (string)held[0]["savedCaster"] != id)
                problems.Add(name + ": link " + index + " does not have exactly one held state naming its holder: " + t.ToString(Formatting.None));
            if ((int)t["rawRecords"] != 1 || record == null || !record.StartsWith(c.Targets[index].UniqueId + "|" + limb + "|", StringComparison.Ordinal))
                problems.Add(name + ": link " + index + " raw record is not exactly one on its limb: " + raw);
            if (occupant != c.Targets[index].UniqueId) problems.Add(name + ": limb " + limb + " is not occupied by link " + index + " (" + occupant + ")");
            if (!(bool)t["entangled"] || !(bool)t["cantMove"]) problems.Add(name + ": link " + index + " target lacks Entangled/CantMove");
            if ((bool)t["nativeTargetPart"] || (bool)s["holder"]["nativeInitiatorPart"]) problems.Add(name + ": a native single-link grapple part is present");
            if ((int)s["holder"]["holdBuff"] != 1) problems.Add(name + ": the holder does not carry exactly one multi-hold buff");
        }

        private void ExpectHolding(Case c, string name)
        {
            JObject s = State(c);
            if ((bool)s["holder"]["cantAct"] || (bool)s["holder"]["cantMove"]) problems.Add(name + ": the multi-link holder carries CantAct/CantMove");
        }

        /// <summary>Link index ended: no held state, no live answer, its limb free, the surviving target unconstrained.</summary>
        private void ExpectEnded(Case c, int index, JObject row, string name, bool holderSurvives)
        {
            JObject s = State(c);
            JObject t = TargetRow(s, index);
            ItemEntityWeapon weapon = c.Weapons[index];
            string occupant = (string)((JArray)s["holder"]["live.occupancy"])[index]["occupant"];
            if (((JArray)t["heldStates"]).Count != 0) problems.Add(name + ": link " + index + " target keeps a held state");
            if (((JArray)s["holder"]["live.reconciled"]).Any(x => ((string)x).StartsWith(c.Targets[index].UniqueId + "|", StringComparison.Ordinal)))
                problems.Add(name + ": link " + index + " still answers live");
            if (occupant != null && occupant == c.Targets[index].UniqueId) problems.Add(name + ": limb " + LimbKey(c.Holder, weapon) + " still reports ended link " + index);
            if (!(bool)t["dead"] && ((bool)t["entangled"] || (bool)t["cantMove"] || (bool)t["cantAct"] || !(bool)t["canMoveFlag"] || !(bool)t["canActFlag"]))
                problems.Add(name + ": link " + index + " surviving target is still constrained: " + t.ToString(Formatting.None));
            // A surviving holder's raw record for an ended link is a finding for review, recorded either way.
            row["rawRecordsAfter." + index] = (int)t["rawRecords"];
            if (holderSurvives && (int)t["rawRecords"] != 0)
                ((JArray)(result["rawRetainedOnSurvivingHolder"] ?? (result["rawRetainedOnSurvivingHolder"] = new JArray()))).Add(name + ": link " + index + ": " + s["holder"]["raw.links"]);
        }

        private void ExpectReset(Case c, string name)
        {
            JObject s = State(c);
            if ((int)s["holder"]["holdBuff"] != 0) problems.Add(name + ": the holder keeps a multi-hold buff");
            if (s["holder"]["raw.links"] is JArray && ((JArray)s["holder"]["raw.links"]).Count != 0) problems.Add(name + ": raw records remain");
            if (((JArray)s["holder"]["live.reconciled"]).Count != 0) problems.Add(name + ": live links remain");
            if (((JArray)s["holder"]["live.occupancy"]).Any(o => (string)o["occupant"] != null)) problems.Add(name + ": a limb reports an occupant");
            if ((bool)s["holder"]["cantAct"] || (bool)s["holder"]["cantMove"]) problems.Add(name + ": the holder keeps CantAct/CantMove");
            for (int index = 0; index < c.Targets.Length; index++)
            {
                JObject t = TargetRow(s, index);
                if (((JArray)t["heldStates"]).Count != 0 || (bool)t["entangled"] || (bool)t["cantMove"] || (bool)t["cantAct"] ||
                    !(bool)t["canMoveFlag"] || !(bool)t["canActFlag"] || (bool)t["nativeTargetPart"])
                    problems.Add(name + ": target " + index + " is not reset: " + t.ToString(Formatting.None));
            }
        }
    }
}
