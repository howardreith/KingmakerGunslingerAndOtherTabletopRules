using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Utility;
using Kingmaker.View.MapObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KmgLoaderCompatHarness
{
    /// <summary>
    /// Sprint 6 checkpoint C1 (VER-02): terminal cleanup of the single-link native grapple owner.
    ///
    /// Owner: SummonGrabComponent with one held target. The grab installs the native UnitPartGrappleInitiator on the
    /// holder (which owns the hold buff carrying SummonHoldComponent) and UnitPartGrappleTarget on the target (which owns
    /// the grappled buff), and records the limb in the holder's UnitPartSummonGrappleLinks.
    ///
    /// Representative: the Expanded Summoning crocodile, summoned by the product's own execution ability through the
    /// engine's UnitUseAbility. Holds are established by triggering the engine's RuleAttackWithWeapon for the bite, so
    /// the product's grab handler and grapple maneuver run as they do for a real attack (holder BAB raised to 100 so the
    /// attack and maneuver succeed: disclosed setup).
    ///
    /// Each terminal is a real engine operation. The raw stored link records (the part's private list) and the live
    /// constraints are read separately, and every outcome is captured before harness teardown.
    ///
    /// Modes: "terminals" (in-session terminals, then one pair left holding with named units for the save),
    /// "reload" (a fresh process loading that save), "transition" (a crocodile holds a party companion and the party
    /// takes the area's exit through the engine's AreaTransitionGroupCommand.ExecuteTransition) and "afterTransition"
    /// (the arrival area, once loading has finished).
    /// </summary>
    internal sealed class GrappleProbe : IOwnedProbe
    {
        private const BindingFlags Any = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const string UnrelatedBuffGuid = "b175001b42b1a02479881b72fe132116"; // native Bull's Strength buff (unrelated effect)
        private const string NativeDismissalGuid = "95f7cdcec94e293489a85afdf5af1fd7";
        internal const string ReloadHolderName = "KMG_C1_ReloadHolder";
        internal const string ReloadTargetName = "KMG_C1_ReloadTarget";
        internal const string TransitionHolderName = "KMG_C1_TransitionHolder";
        internal const string TransitionTargetName = "KMG_C1_TransitionTarget";
        private const string HoldBuffName = "KMG_Summoning_Special_Grapple_Hold";
        private const string GrappledBuffName = "KMG_Summoning_Special_Grapple_Grappled";

        private readonly string mode;
        private readonly JObject result = new JObject();
        private readonly List<string> problems = new List<string>();
        private readonly IEnumerator<object> script;
        private readonly List<UnitEntityData> created = new List<UnitEntityData>();
        private readonly List<UnitEntityData> awakeAdded = new List<UnitEntityData>();
        private string verdict;

        private Assembly product;
        private Type runner, linkPart, links, grabType, holdType, limbs;

        public GrappleProbe(string mode)
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
            result["problems"] = new JArray(problems.Cast<object>().ToArray());
            verdict = problems.Count == 0 ? string.Empty : string.Join(" | ", problems.ToArray());
            return verdict;
        }

        public CleanupReport Release(string testFailure)
        {
            return ProbeCleanup.RunAll(new[]
            {
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
            limbs = product.GetType("KingmakerGunslinger.Summoning.SummonLimbs", true);
            bool paused = Game.Instance.IsPaused;
            result["area"] = Game.Instance.CurrentlyLoadedArea == null ? null : Game.Instance.CurrentlyLoadedArea.name;
            result["turnBased"] = TurnBased.Controllers.CombatController.IsInTurnBasedCombat();
            try
            {
                IEnumerable<object> body = mode == "terminals" ? Terminals() : mode == "reload" ? Reload() :
                    mode == "transition" ? Transition() : mode == "afterTransition" ? AfterTransition() : null;
                if (body == null) { problems.Add("unknown mode " + mode); yield break; }
                foreach (object step in body) yield return step;
            }
            finally { Game.Instance.IsPaused = paused; }
        }

        // ---------------------------------------------------------------- terminals

        private IEnumerable<object> Terminals()
        {
            var cases = new JObject();
            result["cases"] = cases;
            result["areaTransitions"] = Transitions();

            // 1. Holder death. An independent pair (overlap) and an unrelated effect on the freed target are controls.
            Pair a = null, b = null;
            foreach (object step in Establish("holderDeath", p => a = p)) yield return step;
            foreach (object step in Establish("overlap", p => b = p)) yield return step;
            if (a == null || b == null) yield break;
            Buff unrelated = AddUnrelated(a.Target);
            var row = new JObject { ["terminal"] = "GameHelper.KillUnit(holder)", ["before"] = State(a), ["overlapBefore"] = State(b) };
            cases["holderDeath"] = row;
            GameHelper.KillUnit(a.Holder);
            foreach (object step in Settle(12f, () => TargetFree(a))) yield return step;
            Capture(row, a, b);
            row["unrelatedKept"] = unrelated != null && a.Target.Descriptor.Buffs.RawFacts.Contains(unrelated);
            ExpectFree(a, "holderDeath");
            ExpectHeld(b, "holderDeath");
            if (!(bool)row["unrelatedKept"]) problems.Add("holderDeath: the unrelated effect on the freed target was not kept");

            // 1b. Controls only: the shared release helpers called again on the ended pair.
            row = new JObject { ["terminal"] = "control: SummonHoldComponent.Release and SummonGrappleLinks.Release x3 on the ended pair" };
            cases["repeatedReleaseControl"] = row;
            try
            {
                for (int index = 0; index < 3; index++)
                {
                    holdType.GetMethod("Release", Any).Invoke(null, new object[] { a.Holder, a.Target });
                    links.GetMethod("Release", Any).Invoke(null, new object[] { a.Holder, a.Target });
                }
            }
            catch (Exception exception) { row["error"] = (exception.InnerException ?? exception).ToString(); problems.Add("repeatedReleaseControl threw"); }
            Capture(row, a, b);
            ExpectHeld(b, "repeatedReleaseControl");

            // 2. Target death: the holder survives and must not keep the ended relationship.
            Pair c = null;
            foreach (object step in Establish("targetDeath", p => c = p)) yield return step;
            if (c == null) yield break;
            row = new JObject { ["terminal"] = "GameHelper.KillUnit(target)", ["before"] = State(c) };
            cases["targetDeath"] = row;
            GameHelper.KillUnit(c.Target);
            foreach (object step in Settle(12f, () => HolderFree(c) && TargetFree(c))) yield return step;
            Capture(row, c, b);
            ExpectFree(c, "targetDeath");
            ExpectHolderFree(c, "targetDeath");
            ExpectHeld(b, "targetDeath");

            // 2b. A second, real terminal on the already ended pair: the surviving holder's own natural expiry.
            row = new JObject { ["terminal"] = "second terminal: native expiry of the targetDeath holder", ["before"] = State(c) };
            cases["repeatedTerminal"] = row;
            row["expiry"] = Expire(c.Holder);
            foreach (object step in Settle(20f, () => c.Holder.Destroyed)) yield return step;
            Capture(row, c, b);
            row["holderDestroyed"] = c.Holder.Destroyed;
            if (!c.Holder.Destroyed) problems.Add("repeatedTerminal: the holder was not destroyed after its native expiry");
            ExpectHeld(b, "repeatedTerminal");

            // 3. Natural expiry of a holding summon: SummonedUnitBuff's deadline is moved to now and the engine's own
            // buff scheduler expires it (disclosed setup; no direct removal).
            Pair d = null;
            foreach (object step in Establish("summonExpiry", p => d = p)) yield return step;
            if (d == null) yield break;
            row = new JObject { ["terminal"] = "SummonedUnitBuff native expiry (BuffCollection.UpdateNextEvent/Tick)", ["before"] = State(d) };
            cases["summonExpiry"] = row;
            row["expiry"] = Expire(d.Holder);
            foreach (object step in Settle(20f, () => TargetFree(d) && d.Holder.Destroyed)) yield return step;
            Capture(row, d, b);
            row["holderDestroyed"] = d.Holder.Destroyed;
            ExpectFree(d, "summonExpiry");
            if (!d.Holder.Destroyed) problems.Add("summonExpiry: the summon was not destroyed after its native expiry");
            ExpectHeld(b, "summonExpiry");

            // 4. Dismissal: the game's own Dismissal spell, cast by the main character, if it applies to this summon.
            Pair e = null;
            foreach (object step in Establish("dismissal", p => e = p)) yield return step;
            if (e == null) yield break;
            row = new JObject { ["terminal"] = "native Dismissal (UnitUseAbility)", ["before"] = State(e) };
            cases["dismissal"] = row;
            foreach (object step in Dismiss(e, b, row)) yield return step;

            // The independent pair still holds at the end; the harness teardown alone releases it.
            cases["overlapFinal"] = State(b);
            ExpectHeld(b, "final");

            // 5. The pair left holding for the save; the reload launch checks the native load reset.
            Pair r = null;
            foreach (object step in Establish("reload", p => r = p)) yield return step;
            if (r == null) yield break;
            r.Holder.Descriptor.CustomName = ReloadHolderName;
            r.Target.Descriptor.CustomName = ReloadTargetName;
            created.Remove(r.Holder);
            created.Remove(r.Target);
            cases["reloadPairAtSave"] = State(r);
        }

        private IEnumerable<object> Dismiss(Pair e, Pair b, JObject row)
        {
            var dismissal = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>(NativeDismissalGuid);
            UnitEntityData caster = Game.Instance.Player.MainCharacter.Value;
            if (dismissal == null) { row["disposition"] = "native Dismissal blueprint not found"; problems.Add("dismissal: native Dismissal not found"); yield break; }
            row["ability"] = dismissal.name;
            row["components"] = new JArray((dismissal.ComponentsArray ?? new BlueprintComponent[0]).Select(x => (object)x.GetType().FullName).ToArray());
            row["targetCheckers"] = new JArray((dismissal.ComponentsArray ?? new BlueprintComponent[0]).OfType<AbilityTargetHasFact>()
                .Select(x => (object)new JObject
                {
                    ["inverted"] = x.Inverted,
                    ["facts"] = new JArray((x.CheckedFacts ?? new Kingmaker.Blueprints.Facts.BlueprintUnitFact[0]).Select(f => (object)(f == null ? null : f.name + ":" + f.AssetGuid)).ToArray())
                }).ToArray());
            row["holderFeatures"] = new JArray(e.Holder.Descriptor.Progression.Features.Enumerable.Select(f => (object)f.Blueprint.name).ToArray());
            bool added = !caster.Descriptor.HasFact(dismissal);
            if (added) caster.Descriptor.AddFact(dismissal);
            bool cast = false;
            try
            {
                Ability granted = caster.Descriptor.Abilities.GetAbility(dismissal);
                var target = new TargetWrapper(e.Holder);
                var data = new AbilityData(granted);
                bool canTarget = data.CanTarget(target);
                row["canTarget"] = canTarget;
                if (!canTarget) row["disposition"] = "inapplicable: the engine's Dismissal cannot target this summoned crocodile";
                else
                {
                    granted.OverrideParams = new AbilityParams { CasterLevel = 40, SpellLevel = 4 };
                    var command = new UnitUseAbility(data, target);
                    command.IgnoreCooldown(TimeSpan.Zero);
                    caster.Commands.Run(command);
                    command.Start();
                    if (command.Animation != null) command.Animation.IsActed = true;
                    command.Tick();
                    for (int tick = 0; command.ExecutionProcess != null && tick < 5000 && !command.ExecutionProcess.IsEnded; tick++)
                        command.ExecutionProcess.Tick();
                    row["commandResult"] = command.Result.ToString();
                    cast = true;
                }
            }
            finally
            {
                if (added && caster.Descriptor.HasFact(dismissal)) caster.Descriptor.RemoveFact(dismissal);
            }
            if (!cast) yield break;
            foreach (object step in Settle(20f, () => TargetFree(e) && (e.Holder.Destroyed || e.Holder.Descriptor.State.IsDead))) yield return step;
            Capture(row, e, b);
            bool gone = e.Holder.Destroyed || e.Holder.Descriptor.State.IsDead;
            row["holderGone"] = gone;
            if (gone) ExpectFree(e, "dismissal");
            else row["disposition"] = "Dismissal resolved without removing the summon (save or rule); no dismissal terminal reached";
            ExpectHeld(b, "dismissal");
        }

        // ---------------------------------------------------------------- transition

        private IEnumerable<object> Transition()
        {
            AreaTransition[] exits = UnityEngine.Object.FindObjectsOfType<AreaTransition>();
            result["areaTransitions"] = Transitions();
            AreaTransition exit = exits.Length == 1 ? exits[0] : null;
            if (exit == null || exit.AreaEnterPoint == null || exit.AreaEnterPoint.Area == Game.Instance.CurrentlyLoadedArea)
            { problems.Add("transition: the fixture area does not have exactly one exit to another area"); yield break; }
            UnitEntityData user = Game.Instance.Player.ControllableCharacters.FirstOrDefault(u => u.IsDirectlyControllable && !u.Descriptor.IsPet);
            bool scripted = exit.Blueprint != null && exit.Blueprint.Actions.Any(ca => ca.Condition.Check());
            var gate = new JObject
            {
                ["targetArea"] = exit.AreaEnterPoint.Area.name, ["enterPoint"] = exit.AreaEnterPoint.name,
                ["scriptedAction"] = scripted, ["restrictionsPass"] = user != null && exit.CheckRestrictions(user),
                ["autoSaveMode"] = exit.AutoSaveMode.ToString()
            };
            result["exit"] = gate;
            // A scripted exit runs its own actions (possibly a dialog) instead of the transition: refuse rather than
            // drive unexpected UI.
            if (scripted || !(bool)gate["restrictionsPass"]) { problems.Add("transition: the exit is scripted or restricted: " + gate.ToString(Formatting.None)); yield break; }

            Pair pair = null;
            foreach (object step in Establish("transition", p => pair = p, true)) yield return step;
            if (pair == null) yield break;
            pair.Holder.Descriptor.CustomName = TransitionHolderName;
            pair.Target.Descriptor.CustomName = TransitionTargetName;
            created.Remove(pair.Holder);
            created.Remove(pair.Target);
            var row = new JObject { ["before"] = State(pair), ["targetInParty"] = Game.Instance.Player.Party.Contains(pair.Target) };
            result["transition"] = row;
            // The relationship being qualified must be a party-held one before the exit is used.
            if (!IsHeld(pair) || !(bool)row["targetInParty"]) { problems.Add("transition: the party companion is not held before the exit"); yield break; }
            Plan = new TransitionPlan
            {
                Origin = Game.Instance.CurrentlyLoadedArea.name, Destination = exit.AreaEnterPoint.Area.name,
                EnterPoint = exit.AreaEnterPoint.name, HolderId = pair.Holder.UniqueId, TargetId = pair.Target.UniqueId
            };
            result["plan"] = Plan.ToJson();
            // The party uses the exit: the engine raises IPartyLeaveAreaHandler and then starts LoadArea. The leave
            // boundary is read synchronously here, before any arrival-time ResetLoadedGrapples can run.
            AreaTransitionGroupCommand.ExecuteTransition(exit);
            row["afterLeaveEvent"] = State(pair);
            row["loadingStarted"] = LoadingProcess.Instance.IsLoadingInProcess;
            row["areaAtLeaveBoundary"] = Game.Instance.CurrentlyLoadedArea == null ? null : Game.Instance.CurrentlyLoadedArea.name;
            ExpectFree(pair, "transition.afterLeaveEvent");
            // Holder-side residue at the leave boundary is captured, not assumed cleared (source ordering is a hypothesis).
            JObject leave = (JObject)row["afterLeaveEvent"];
            row["leaveBoundaryHolderResidue"] = new JObject
            {
                ["initiatorPart"] = leave["live.holderInitiatorPart"], ["holdBuff"] = leave["live.holderHoldBuff"],
                ["rawLinksNamingTarget"] = leave["rawLinksNamingTarget"], ["cantAct"] = leave["live.holderCantAct"], ["cantMove"] = leave["live.holderCantMove"]
            };
            if (!(bool)row["loadingStarted"]) problems.Add("transition: the engine did not start loading the next area");
        }

        /// <summary>Carried from the leave phase to the arrival phase within one process.</summary>
        private sealed class TransitionPlan
        {
            internal string Origin, Destination, EnterPoint, HolderId, TargetId;
            internal JObject ToJson()
            {
                return new JObject { ["origin"] = Origin, ["destination"] = Destination, ["enterPoint"] = EnterPoint, ["holder"] = HolderId, ["target"] = TargetId };
            }
        }

        private static TransitionPlan Plan;

        private IEnumerable<object> AfterTransition()
        {
            TransitionPlan plan = Plan;
            if (plan == null) { problems.Add("afterTransition: no leave-phase plan in this process"); yield break; }
            result["plan"] = plan.ToJson();
            float until = Time.realtimeSinceStartup + 150f;
            while ((LoadingProcess.Instance.IsLoadingInProcess || Game.Instance.CurrentlyLoadedArea == null) && Time.realtimeSinceStartup < until)
                yield return null;
            if (LoadingProcess.Instance.IsLoadingInProcess) { problems.Add("afterTransition: the loading deadline expired with loading still in progress"); yield break; }
            if (Game.Instance.CurrentlyLoadedArea == null) { problems.Add("afterTransition: no area is loaded at the deadline"); yield break; }
            for (int frame = 0; frame < 30; frame++) yield return null;
            result["area"] = Game.Instance.CurrentlyLoadedArea.name;
            if (Game.Instance.CurrentlyLoadedArea.name != plan.Destination)
            { problems.Add("afterTransition: the loaded area " + Game.Instance.CurrentlyLoadedArea.name + " is not the intended destination " + plan.Destination); yield break; }
            UnitEntityData target = Game.Instance.Player.Party.FirstOrDefault(u => u.UniqueId == plan.TargetId);
            UnitEntityData holder = Game.Instance.State.Units.FirstOrDefault(u => u.UniqueId == plan.HolderId);
            result["holderInLoadedArea"] = holder != null;
            result["holderDisposition"] = holder != null ? "loaded in the destination" :
                Game.Instance.State.Units.All.Any(u => u.UniqueId == plan.HolderId) ? "known but not loaded" : "not among loaded units (left in the origin area state or destroyed)";
            if (target == null) { problems.Add("afterTransition: the party companion target is missing"); yield break; }
            var pair = new Pair { Holder = holder, Target = target, HoldBuff = BuffNamed(HoldBuffName), GrappledBuff = BuffNamed(GrappledBuffName) };
            var row = new JObject { ["atArrival"] = State(pair), ["targetPosition"] = target.Position.ToString() };
            result["afterTransition"] = row;
            ExpectFree(pair, "afterTransition.atArrival");
            if (holder != null) ExpectHolderFree(pair, "afterTransition.atArrival");
            WakeUnit(target);
            foreach (object step in Settle(6f, () => false)) yield return step;
            row["afterTicks"] = State(pair);
            ExpectFree(pair, "afterTransition.afterTicks");
            if (holder != null) ExpectHolderFree(pair, "afterTransition.afterTicks");
        }

        private static BlueprintBuff BuffNamed(string name)
        {
            var library = ResourcesLibrary.LibraryObject;
            return library == null ? null : library.GetAllBlueprints().OfType<BlueprintBuff>().FirstOrDefault(b => b.name == name);
        }

        // ---------------------------------------------------------------- reload

        private IEnumerable<object> Reload()
        {
            UnitEntityData holder = Game.Instance.State.Units.FirstOrDefault(u => u.Descriptor.CustomName == ReloadHolderName);
            UnitEntityData target = Game.Instance.State.Units.FirstOrDefault(u => u.Descriptor.CustomName == ReloadTargetName);
            if (holder == null || target == null) { problems.Add("reload: the named pair is missing (holder " + (holder != null) + ", target " + (target != null) + ")"); yield break; }
            created.Add(holder);
            created.Add(target);
            var pair = new Pair { Holder = holder, Target = target };
            object grab = grabType.GetMethod("Find", Any).Invoke(null, new object[] { holder });
            if (grab == null) problems.Add("reload: the crocodile has no grab component after load");
            else
            {
                pair.HoldBuff = (BlueprintBuff)grabType.GetField("HoldBuff").GetValue(grab);
                pair.GrappledBuff = (BlueprintBuff)grabType.GetField("GrappledBuff").GetValue(grab);
            }
            var row = new JObject { ["atLoad"] = State(pair) };
            result["reload"] = row;
            ExpectFree(pair, "reload.atLoad");
            ExpectHolderFree(pair, "reload.atLoad");
            // No re-establishment over time: wake both and let the native controllers run.
            WakeUnit(holder);
            WakeUnit(target);
            foreach (object step in Settle(8f, () => false)) yield return step;
            row["afterTicks"] = State(pair);
            ExpectFree(pair, "reload.afterTicks");
            ExpectHolderFree(pair, "reload.afterTicks");
        }

        // ---------------------------------------------------------------- fixture

        private sealed class Pair
        {
            internal UnitEntityData Holder, Target;
            internal BlueprintBuff HoldBuff, GrappledBuff;
        }

        private IEnumerable<object> Establish(string name, Action<Pair> done, bool partyTarget = false)
        {
            UnitEntityData main = Game.Instance.Player.MainCharacter.Value;
            UnitEntityData holder = Summon(main, "crocodile");
            if (holder == null) { problems.Add(name + ": the crocodile was not summoned"); yield break; }
            Vector3 at = holder.Position + holder.OrientationDirection * 1.0f;
            UnitEntityData target = partyTarget ? CreateCompanion(at) : Game.Instance.EntityCreator.SpawnUnit(BlueprintRoot.Instance.DefaultPlayerCharacter, at,
                Quaternion.identity, Game.Instance.State.LoadedAreaState.MainState);
            if (target == null) { problems.Add(name + ": the target was not created"); yield break; }
            created.Add(target);
            Game.Instance.EntityCreator.Tick();
            target.Descriptor.Stats.HitPoints.BaseValue = 500;
            runner.GetMethod("SetExpandedSummoningBrainActive", Any).Invoke(null, new object[] { target, false });
            WakeUnit(holder);
            WakeUnit(target);
            holder.Descriptor.Stats.BaseAttackBonus.BaseValue = 100;
            yield return null;
            var baseline = new JObject
            {
                ["holderBuffsAtSummon"] = Buffs(holder),
                ["holderCantActAtSummon"] = holder.Descriptor.State.HasCondition(UnitCondition.CantAct),
                ["holderCantMoveAtSummon"] = holder.Descriptor.State.HasCondition(UnitCondition.CantMove),
                ["appearBuffsRemoved"] = (int)runner.GetMethod("RemoveExpandedSummoningAppearanceBuffs", Any).Invoke(null, new object[] { holder }),
                ["targetInUnits"] = Game.Instance.State.Units.All.Contains(target)
            };
            baseline["holderBuffsBeforeBite"] = Buffs(holder);
            baseline["holderCantActBeforeBite"] = holder.Descriptor.State.HasCondition(UnitCondition.CantAct);
            baseline["holderCantMoveBeforeBite"] = holder.Descriptor.State.HasCondition(UnitCondition.CantMove);
            baseline["targetCantMoveBeforeBite"] = target.Descriptor.State.HasCondition(UnitCondition.CantMove);
            baseline["targetCantActBeforeBite"] = target.Descriptor.State.HasCondition(UnitCondition.CantAct);
            result["baseline." + name] = baseline;
            if ((bool)baseline["holderCantActBeforeBite"] || (bool)baseline["holderCantMoveBeforeBite"] ||
                (bool)baseline["targetCantMoveBeforeBite"] || (bool)baseline["targetCantActBeforeBite"] || !(bool)baseline["targetInUnits"])
            {
                problems.Add(name + ": fixture baseline is not free before the bite: " + baseline.ToString(Formatting.None));
                yield break;
            }
            object grab = grabType.GetMethod("Find", Any).Invoke(null, new object[] { holder });
            if (grab == null) { problems.Add(name + ": the crocodile has no grab component"); yield break; }
            var pair = new Pair
            {
                Holder = holder, Target = target,
                HoldBuff = (BlueprintBuff)grabType.GetField("HoldBuff").GetValue(grab),
                GrappledBuff = (BlueprintBuff)grabType.GetField("GrappledBuff").GetValue(grab)
            };
            var weapon = (ItemEntityWeapon)limbs.GetMethod("PrimaryWeapon", Any).Invoke(null, new object[] { holder });
            int attacks = 0;
            for (; attacks < 6 && weapon != null && !IsHeld(pair); attacks++)
            {
                Rulebook.Trigger(new RuleAttackWithWeapon(holder, target, weapon, 0));
                yield return null;
            }
            JObject state = State(pair);
            state["attacks"] = attacks;
            state["weapon"] = weapon == null ? null : weapon.Blueprint.name;
            state["distance"] = Vector3.Distance(holder.Position, target.Position);
            result["established." + name] = state;
            if (!IsHeld(pair) || (int)state["rawLinksNamingTarget"] != 1 || (int)state["live.reconciledLinks"] != 1)
            {
                problems.Add(name + ": the bite did not establish a recorded hold: " + state.ToString(Formatting.None));
                yield break;
            }
            done(pair);
        }

        /// <summary>A party companion through the engine's persistent-party path (as checkpoint B's fixture).</summary>
        private static UnitEntityData CreateCompanion(Vector3 at)
        {
            MethodInfo add = typeof(Game).GetMethod("AddUnitToPersistentSate", BindingFlags.Instance | BindingFlags.NonPublic);
            var unit = (UnitEntityData)add.Invoke(Game.Instance, new object[] { BlueprintRoot.Instance.DefaultPlayerCharacter });
            unit.Descriptor.State.CanRemoveFromParty = true;
            unit.AttachToViewOnLoad(null);
            Game.Instance.Player.AddCompanion(unit);
            unit.IsInGame = true;
            unit.Position = at;
            return unit.View == null ? null : unit;
        }

        private void WakeUnit(UnitEntityData unit)
        {
            unit.Wake(120f);
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
            runner.GetMethod("SetExpandedSummoningBrainActive", Any).Invoke(null, new object[] { unit, false });
            return unit;
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
                ["attachedAfterNativeTick"] = owner.Descriptor.Buffs.RawFacts.Contains(marker),
                ["setup"] = "deadline moved to now; native BuffCollection.UpdateNextEvent/Tick removes it"
            };
        }

        /// <summary>Unpaused frames until the condition holds or the real-time budget ends (at least five frames).</summary>
        private static IEnumerable<object> Settle(float seconds, Func<bool> done)
        {
            bool wasPaused = Game.Instance.IsPaused;
            Game.Instance.IsPaused = false;
            float until = Time.realtimeSinceStartup + seconds;
            for (int frame = 0; Time.realtimeSinceStartup < until; frame++)
            {
                if (Game.Instance.IsPaused) Game.Instance.IsPaused = false;
                if (frame >= 5 && done()) break;
                yield return null;
            }
            Game.Instance.IsPaused = wasPaused;
        }

        private static JArray Transitions()
        {
            var rows = new JArray();
            foreach (AreaTransition transition in UnityEngine.Object.FindObjectsOfType<AreaTransition>())
            {
                var point = transition.AreaEnterPoint;
                rows.Add(new JObject
                {
                    ["object"] = transition.gameObject.name,
                    ["enterPoint"] = point == null ? null : point.name,
                    ["targetArea"] = point == null || point.Area == null ? null : point.Area.name,
                    ["sameArea"] = point != null && point.Area == Game.Instance.CurrentlyLoadedArea,
                    ["blueprint"] = transition.Blueprint == null ? null : transition.Blueprint.name
                });
            }
            return rows;
        }

        // ---------------------------------------------------------------- observation

        private static bool IsHeld(Pair pair)
        {
            UnitPartGrappleInitiator initiator = pair.Holder.Get<UnitPartGrappleInitiator>();
            UnitPartGrappleTarget held = pair.Target.Get<UnitPartGrappleTarget>();
            return initiator != null && held != null && ReferenceEquals(initiator.Target.Value, pair.Target) &&
                ReferenceEquals(held.Initiator.Value, pair.Holder);
        }

        private static bool HasBuff(UnitEntityData unit, BlueprintBuff blueprint)
        {
            return blueprint != null && unit.Descriptor.Buffs.RawFacts.OfType<Buff>().Any(x => ReferenceEquals(x.Blueprint, blueprint));
        }

        private static bool TargetFree(Pair pair)
        {
            return pair.Target.Get<UnitPartGrappleTarget>() == null && !HasBuff(pair.Target, pair.GrappledBuff);
        }

        private static bool HolderFree(Pair pair)
        {
            return pair.Holder.Destroyed || pair.Holder.Get<UnitPartGrappleInitiator>() == null && !HasBuff(pair.Holder, pair.HoldBuff);
        }

        private IList<string> RawLinks(UnitEntityData holder)
        {
            if (holder.Descriptor == null) return null;
            object part = typeof(UnitDescriptor).GetMethods().Single(m => m.Name == "Get" && m.IsGenericMethodDefinition && m.GetParameters().Length == 0)
                .MakeGenericMethod(linkPart).Invoke(holder.Descriptor, null);
            return part == null ? null : (IList<string>)linkPart.GetField("_links", Any).GetValue(part);
        }

        /// <summary>The raw stored records and the live constraints, read separately; Reconcile only filters.</summary>
        private JObject State(Pair pair)
        {
            UnitPartGrappleTarget held = pair.Target.Get<UnitPartGrappleTarget>();
            UnitState t = pair.Target.Descriptor.State;
            if (pair.Holder == null)
                return new JObject
                {
                    ["holder"] = null, ["target"] = pair.Target.UniqueId, ["targetDead"] = t.IsDead, ["targetDestroyed"] = pair.Target.Destroyed,
                    ["live.reconciledLinks"] = 0,
                    ["live.targetPart"] = held != null,
                    ["live.targetPartInitiator"] = held == null || held.Initiator.Value == null ? null : held.Initiator.Value.UniqueId,
                    ["live.targetGrappledBuff"] = HasBuff(pair.Target, pair.GrappledBuff),
                    ["live.targetCantMove"] = t.HasCondition(UnitCondition.CantMove),
                    ["live.targetCantAct"] = t.HasCondition(UnitCondition.CantAct),
                    ["live.targetCanMove"] = t.CanMove, ["live.targetCanAct"] = t.CanAct,
                    ["live.targetBuffs"] = Buffs(pair.Target)
                };
            IList<string> raw = RawLinks(pair.Holder);
            UnitPartGrappleInitiator initiator = pair.Holder.Get<UnitPartGrappleInitiator>();
            var reconciled = (ICollection)links.GetMethod("Reconcile", Any).Invoke(null, new object[] { pair.Holder });
            return new JObject
            {
                ["holder"] = pair.Holder.UniqueId, ["target"] = pair.Target.UniqueId,
                ["holderDead"] = pair.Holder.Descriptor.State.IsDead, ["holderDestroyed"] = pair.Holder.Destroyed,
                ["targetDead"] = t.IsDead, ["targetDestroyed"] = pair.Target.Destroyed,
                ["raw.linkPart"] = raw != null,
                ["raw.links"] = raw == null ? null : new JArray(raw.Cast<object>().ToArray()),
                ["rawLinksNamingTarget"] = raw == null ? 0 : raw.Count(l => l != null && l.StartsWith(pair.Target.UniqueId + "|", StringComparison.Ordinal)),
                ["live.reconciledLinks"] = reconciled.Count,
                ["live.holderInitiatorPart"] = initiator != null,
                ["live.holderInitiatorTarget"] = initiator == null || initiator.Target.Value == null ? null : initiator.Target.Value.UniqueId,
                ["live.holderHoldBuff"] = HasBuff(pair.Holder, pair.HoldBuff),
                ["live.holderCantAct"] = pair.Holder.Descriptor.State.HasCondition(UnitCondition.CantAct),
                ["live.holderCantMove"] = pair.Holder.Descriptor.State.HasCondition(UnitCondition.CantMove),
                ["live.targetPart"] = held != null,
                ["live.targetPartInitiator"] = held == null || held.Initiator.Value == null ? null : held.Initiator.Value.UniqueId,
                ["live.targetGrappledBuff"] = HasBuff(pair.Target, pair.GrappledBuff),
                ["live.targetCantMove"] = t.HasCondition(UnitCondition.CantMove),
                ["live.targetCantAct"] = t.HasCondition(UnitCondition.CantAct),
                ["live.targetCanMove"] = t.CanMove, ["live.targetCanAct"] = t.CanAct,
                ["live.targetBuffs"] = Buffs(pair.Target),
                ["live.holderBuffs"] = pair.Holder.Destroyed ? null : Buffs(pair.Holder)
            };
        }

        private static JArray Buffs(UnitEntityData unit)
        {
            return new JArray(unit.Descriptor.Buffs.RawFacts.OfType<Buff>().Select(x => (object)x.Blueprint.name).ToArray());
        }

        private void Capture(JObject row, Pair pair, Pair overlap)
        {
            row["after"] = State(pair);
            row["overlapAfter"] = State(overlap);
        }

        /// <summary>The ended relationship holds nothing on the target, and a surviving target can move and act.</summary>
        private void ExpectFree(Pair pair, string name)
        {
            JObject s = State(pair);
            if ((bool)s["live.targetPart"]) problems.Add(name + ": the target keeps a native grapple-target part (initiator " + s["live.targetPartInitiator"] + ")");
            if ((bool)s["live.targetGrappledBuff"]) problems.Add(name + ": the target keeps the grappled buff");
            if ((int)s["live.reconciledLinks"] != 0) problems.Add(name + ": live (reconciled) links remain");
            if (!(bool)s["targetDead"] && !(bool)s["targetDestroyed"] &&
                ((bool)s["live.targetCantMove"] || (bool)s["live.targetCantAct"] || !(bool)s["live.targetCanMove"] || !(bool)s["live.targetCanAct"]))
                problems.Add(name + ": the surviving target cannot move or act: " + s.ToString(Formatting.None));
        }

        /// <summary>A surviving holder keeps no part, buff, condition or stored record of the ended relationship.</summary>
        private void ExpectHolderFree(Pair pair, string name)
        {
            if (pair.Holder == null || pair.Holder.Destroyed) return;
            JObject s = State(pair);
            if ((bool)s["live.holderInitiatorPart"]) problems.Add(name + ": the holder keeps its native initiator part");
            if ((bool)s["live.holderHoldBuff"]) problems.Add(name + ": the holder keeps its hold buff");
            if (!(bool)s["holderDead"] && ((bool)s["live.holderCantAct"] || (bool)s["live.holderCantMove"])) problems.Add(name + ": the surviving holder keeps grapple conditions");
            if ((int)s["rawLinksNamingTarget"] != 0) problems.Add(name + ": the holder's raw stored records still name the ended target: " + s["raw.links"]);
        }

        private void ExpectHeld(Pair pair, string name)
        {
            JObject s = State(pair);
            if (!IsHeld(pair) || (int)s["rawLinksNamingTarget"] != 1 || (int)s["live.reconciledLinks"] != 1 || !(bool)s["live.targetGrappledBuff"])
                problems.Add(name + ": the independent overlapping pair lost its hold or record: " + s.ToString(Formatting.None));
        }
    }
}
