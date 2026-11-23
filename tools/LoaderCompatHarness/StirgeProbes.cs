using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.View;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KmgLoaderCompatHarness
{
    /// <summary>
    /// Sprint 6 checkpoint G4 (VER-02): the retained Stirge attachment lifecycle and its shared Remove Stirge action, on
    /// the accepted C3a product (unchanged).
    ///
    /// Publication: the Stirge stays withdrawn from every public summon menu (CUT-01). The probe invokes the product's
    /// retained (unpublished) Stirge execution ability directly, as an older reference would resolve it; it changes no
    /// publication or suppression. Attachments are made only by real touch attacks (the engine's RuleAttackWithWeapon with
    /// the Stirge's exact touch weapon), which reach the product's StirgeAttachComponent. Nothing calls Attach or Detach
    /// to manufacture an outcome; one direct Detach call is a labelled idempotence diagnostic after a native terminal.
    ///
    /// Disclosed setup: Stirge BAB 100 (touch hit), prey HP raised, appear buffs removed, brains off (so nothing re-attacks
    /// during quiet intervals), units woken, navmesh placement, pauses for observation; for the removal action only, the
    /// prey's Mobility raised to 300 so the genuine check succeeds; summon expiry by a shortened native deadline; in the
    /// save mode, one clearly labelled synthetic orphan Remove Stirge fact on a disposable unit no Stirge targets.
    ///
    /// Modes: "lifecycle" (two Stirges on one prey: first-owner death, last-owner removal through the prey's own ability;
    /// then active summon expiry and prey death), "save" (an active attachment through a finished native save), and
    /// "reload" (the fresh process loading that save).
    /// </summary>
    internal sealed class StirgeProbe : IOwnedProbe
    {
        private const BindingFlags Any = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const string UnrelatedBuffGuid = "b175001b42b1a02479881b72fe132116";
        private const string FilthFeverGuid = "9545a5550d89feb47a84edaeb4e63d0b";
        internal static readonly string[] SaveNames = { "KMG_G4_Stirge", "KMG_G4_Prey", "KMG_G4_SyntheticOrphan" };

        private readonly string mode;
        private readonly JObject result = new JObject();
        private readonly List<string> problems = new List<string>();
        private readonly IEnumerator<object> script;
        private readonly List<UnitEntityData> created = new List<UnitEntityData>();
        private readonly List<UnitEntityData> awakeAdded = new List<UnitEntityData>();
        private string verdict;
        private Assembly product;
        private Type runner, holdType;
        private BlueprintAbility remove;
        private BlueprintComponent checker;
        private Vector3 origin, forward, right;
        private int spotIndex;

        public StirgeProbe(string mode) { this.mode = mode; result["mode"] = mode; script = Run().GetEnumerator(); }
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
            return ProbeCleanup.RunAll(new[]
            {
                new KeyValuePair<string, Action>("awake registrations", () => { foreach (UnitEntityData u in awakeAdded) Game.Instance.State.AwakeUnits.Remove(u); awakeAdded.Clear(); }),
                new KeyValuePair<string, Action>("disposable units", () => { foreach (UnitEntityData u in created.Where(u => u != null && !u.Destroyed).ToArray()) u.Destroy(); created.Clear(); })
            });
        }

        private IEnumerable<object> Run()
        {
            product = ProductProbes.Entry().Assembly;
            runner = product.GetType("KingmakerGunslinger.RuntimeTesting.RuntimeTestRunner", true);
            holdType = product.GetType("KingmakerGunslinger.Summoning.StirgeHoldComponent", true);
            remove = holdType.GetField("RemoveAbility", Any).GetValue(null) as BlueprintAbility;
            if (!HoldLifecycleObserver.Installed) { problems.Add("the hold lifecycle observer is not installed (launch condition observeHoldLifecycle)"); yield break; }
            if (remove == null) { problems.Add("the retained Remove Stirge ability is not configured"); yield break; }
            checker = remove.ComponentsArray.FirstOrDefault(c => c.GetType().Name == "RemoveStirgeTargetChecker");
            result["area"] = Game.Instance.CurrentlyLoadedArea == null ? null : Game.Instance.CurrentlyLoadedArea.name;
            result["turnBased"] = TurnBased.Controllers.CombatController.IsInTurnBasedCombat();
            result["removeAbility"] = new JObject { ["name"] = remove.name, ["guid"] = remove.AssetGuid, ["hidden"] = remove.Hidden, ["range"] = remove.Range.ToString(),
                ["actionType"] = remove.ActionType.ToString(), ["canTargetEnemies"] = remove.CanTargetEnemies, ["canTargetFriends"] = remove.CanTargetFriends,
                ["canTargetSelf"] = remove.CanTargetSelf, ["checker"] = checker?.GetType().Name };
            UnitEntityData main = Game.Instance.Player.MainCharacter.Value;
            origin = main.Position;
            forward = Quaternion.Euler(0, main.Orientation, 0) * Vector3.forward;
            right = Quaternion.Euler(0, main.Orientation + 90f, 0) * Vector3.forward;
            bool paused = Game.Instance.IsPaused;
            try
            {
                IEnumerable<object> body = mode == "lifecycle" ? Lifecycle() : mode == "save" ? Save() : mode == "reload" ? Reload() : null;
                if (body == null) { problems.Add("unknown mode " + mode); yield break; }
                foreach (object step in body) yield return step;
            }
            finally { Game.Instance.IsPaused = paused; }
        }

        // ---------------------------------------------------------------- fixture

        /// <summary>A fresh, separated spot for one prey and its Stirges.</summary>
        private Vector3 NextSpot()
        {
            int k = spotIndex++;
            Vector3 offset = right * (k % 2 == 0 ? -6f : 6f) + forward * (3f + 5f * (k / 2));
            return ObstacleAnalyzer.TraceAlongNavmesh(origin, origin + offset);
        }

        private UnitEntityData SummonStirge(Vector3 near, float angle)
        {
            UnitEntityData main = Game.Instance.Player.MainCharacter.Value;
            Type multiplicity = product.GetType("KingmakerGunslinger.Summoning.SummonMultiplicity", true);
            object variant = runner.GetMethod("ExpandedSummoningOwnTierVariant", Any).Invoke(null, new[] { "stirge", Enum.Parse(multiplicity, "One") });
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
            Vector3 at = ObstacleAnalyzer.TraceAlongNavmesh(near, near + Quaternion.Euler(0, angle, 0) * Vector3.forward * 1.0f);
            runner.GetMethod("PlaceExpandedSummoningUnit", Any).Invoke(null, new object[] { unit, at });
            unit.Descriptor.Stats.BaseAttackBonus.BaseValue = 100;
            return unit;
        }

        private UnitEntityData SpawnPrey(Vector3 at)
        {
            UnitEntityData unit = Game.Instance.EntityCreator.SpawnUnit(BlueprintRoot.Instance.DefaultPlayerCharacter, at, Quaternion.identity, Game.Instance.State.LoadedAreaState.MainState);
            if (unit == null) return null;
            created.Add(unit);
            Game.Instance.EntityCreator.Tick();
            unit.Descriptor.Stats.HitPoints.BaseValue = 100000;
            runner.GetMethod("SetExpandedSummoningBrainActive", Any).Invoke(null, new object[] { unit, false });
            Wake(unit);
            return unit;
        }

        private void Wake(UnitEntityData unit)
        {
            unit.Wake(900f);
            if (!Game.Instance.State.AwakeUnits.Contains(unit)) { Game.Instance.State.AwakeUnits.Add(unit); awakeAdded.Add(unit); }
        }

        private Buff AddUnrelated(UnitEntityData unit)
        {
            var blueprint = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>(UnrelatedBuffGuid);
            return blueprint == null ? null : unit.Descriptor.Buffs.AddBuff(blueprint, unit, TimeSpan.FromMinutes(30));
        }

        private UnitEntityData Attached(UnitEntityData stirge) => holdType.GetMethod("AttachedTarget", Any).Invoke(null, new object[] { stirge }) as UnitEntityData;

        private static Buff HoldOf(UnitEntityData stirge) =>
            stirge == null || stirge.Destroyed ? null : stirge.Descriptor.Buffs.RawFacts.OfType<Buff>().FirstOrDefault(b => b.Blueprint != null && b.Blueprint.ComponentsArray != null &&
                b.Blueprint.ComponentsArray.Any(c => c.GetType().Name == "StirgeHoldComponent"));

        private int Cumulative(UnitEntityData stirge)
        {
            Buff hold = HoldOf(stirge);
            object logic = hold == null ? null : hold.Components.FirstOrDefault(c => c.GetType().Name == "StirgeHoldComponent");
            return logic == null ? -1 : (int)holdType.GetField("m_CumulativeDamage", Any).GetValue(logic);
        }

        private int RemoveCount(UnitEntityData unit) =>
            unit == null || unit.Destroyed ? 0 : unit.Descriptor.Abilities.RawFacts.Count(a => ReferenceEquals(a.Blueprint, remove));

        private bool CheckerAllows(UnitEntityData prey, UnitEntityData stirge) =>
            checker != null && ((IAbilityTargetChecker)checker).CanTarget(prey, new TargetWrapper(stirge));

        private bool NativeCanTarget(UnitEntityData prey, UnitEntityData stirge)
        {
            Ability ability = prey.Descriptor.Abilities.GetAbility(remove);
            return ability != null && ability.Data.CanTarget(new TargetWrapper(stirge));
        }

        private static bool HasBuff(UnitEntityData unit, string guid) =>
            unit != null && !unit.Destroyed && unit.Descriptor.Buffs.RawFacts.OfType<Buff>().Any(b => b.Blueprint.AssetGuid == guid);

        private JObject StirgeState(UnitEntityData s)
        {
            UnitState st = s.Descriptor.State;
            return new JObject
            {
                ["id"] = s.UniqueId, ["name"] = s.Descriptor.CustomName, ["dead"] = st.IsDead, ["destroyed"] = s.Destroyed,
                ["attachedTarget"] = Attached(s)?.UniqueId, ["holdBuff"] = HoldOf(s) != null, ["cumulative"] = Cumulative(s),
                ["cantMove"] = st.HasCondition(UnitCondition.CantMove), ["cantAct"] = st.HasCondition(UnitCondition.CantAct),
                ["loseDexterityToAC"] = st.HasCondition(UnitCondition.LoseDexterityToAC),
                ["summonedMarker"] = !s.Destroyed && s.Descriptor.Buffs.GetBuff(BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff) != null,
                ["buffs"] = s.Destroyed ? new JArray() : new JArray(s.Descriptor.Buffs.RawFacts.OfType<Buff>().Select(b => (object)b.Blueprint.name).ToArray())
            };
        }

        private JObject PreyState(UnitEntityData p)
        {
            UnitState st = p.Descriptor.State;
            return new JObject
            {
                ["id"] = p.UniqueId, ["name"] = p.Descriptor.CustomName, ["dead"] = st.IsDead, ["destroyed"] = p.Destroyed,
                ["removeActionFacts"] = RemoveCount(p), ["constitutionDamage"] = p.Descriptor.Stats.Constitution.Damage, ["damage"] = p.Descriptor.Damage,
                ["cantMove"] = st.HasCondition(UnitCondition.CantMove), ["cantAct"] = st.HasCondition(UnitCondition.CantAct),
                ["grappleTargetPart"] = p.Get<UnitPartGrappleTarget>() != null,
                ["filthFever"] = HasBuff(p, FilthFeverGuid), ["unrelated"] = HasBuff(p, UnrelatedBuffGuid),
                ["buffs"] = p.Destroyed ? new JArray() : new JArray(p.Descriptor.Buffs.RawFacts.OfType<Buff>().Select(b => (object)b.Blueprint.name).ToArray())
            };
        }

        private static IEnumerable<object> SettleGameTime(double gameSeconds, float wallBudget, Func<bool> done)
        {
            TimeSpan start = Game.Instance.TimeController.GameTime;
            float until = Time.realtimeSinceStartup + wallBudget;
            while (Time.realtimeSinceStartup < until)
            {
                if (Game.Instance.IsPaused) Game.Instance.IsPaused = false;
                if ((Game.Instance.TimeController.GameTime - start).TotalSeconds >= gameSeconds && done()) break;
                yield return null;
            }
            Game.Instance.IsPaused = true;
        }

        private static int LastSeq => HoldLifecycleObserver.Events.Count == 0 ? 0 : (int)HoldLifecycleObserver.Events.Last()["seq"];
        private static JArray Events(int seq, Func<JObject, bool> filter) =>
            new JArray(HoldLifecycleObserver.Events.Where(e => (int)e["seq"] > seq && filter(e)).Cast<object>().ToArray());
        private static JArray StirgeEvents(int seq) => Events(seq, e => ((string)e["what"]).StartsWith("stirge.", StringComparison.Ordinal));
        private static double Now => Game.Instance.TimeController.GameTime.TotalSeconds;

        /// <summary>A real touch attack (exact touch weapon) until this Stirge's attach path names the prey; bounded.</summary>
        private IEnumerable<object> TouchAttach(string name, UnitEntityData stirge, UnitEntityData prey)
        {
            ItemEntityWeapon touch = stirge.Body.PrimaryHand.MaybeWeapon;
            int seq = LastSeq, attacks = 0;
            for (; attacks < 6 && !ReferenceEquals(Attached(stirge), prey); attacks++)
            {
                Rulebook.Trigger(new RuleAttackWithWeapon(stirge, prey, touch, 0));
                yield return null;
            }
            result["attach." + name] = new JObject { ["weapon"] = touch?.Blueprint.name, ["attacks"] = attacks, ["attached"] = ReferenceEquals(Attached(stirge), prey),
                ["tryAttach"] = Events(seq, e => (string)e["what"] == "stirge.TryAttach" && (string)e["owner"] == stirge.UniqueId) };
            if (!ReferenceEquals(Attached(stirge), prey)) problems.Add(name + ": the touch attack did not attach within six attacks");
        }

        /// <summary>The asserted attachment contract: the Stirge-owned hold and its owner constraints; the prey free, with the shared action.</summary>
        private void ExpectAttached(string name, UnitEntityData stirge, UnitEntityData prey)
        {
            if (stirge.Destroyed || stirge.Descriptor.State.IsDead) problems.Add(name + ": the Stirge is not alive");
            if (prey.Destroyed || prey.Descriptor.State.IsDead) problems.Add(name + ": the prey is not alive");
            if (!ReferenceEquals(Attached(stirge), prey)) problems.Add(name + ": the Stirge does not name the prey");
            if (HoldOf(stirge) == null) problems.Add(name + ": the Stirge lacks its hold buff");
            UnitState st = stirge.Descriptor.State;
            if (!st.HasCondition(UnitCondition.CantMove) || !st.HasCondition(UnitCondition.CantAct) || !st.HasCondition(UnitCondition.LoseDexterityToAC))
                problems.Add(name + ": the Stirge lacks its owned hold constraints");
            if (RemoveCount(prey) != 1) problems.Add(name + ": the prey has " + RemoveCount(prey) + " Remove Stirge facts, expected one");
            if (!NativeCanTarget(prey, stirge) || !CheckerAllows(prey, stirge)) problems.Add(name + ": the prey's Remove Stirge action cannot target the attached Stirge");
            if (prey.Descriptor.State.HasCondition(UnitCondition.CantMove) || prey.Descriptor.State.HasCondition(UnitCondition.CantAct) || prey.Get<UnitPartGrappleTarget>() != null)
                problems.Add(name + ": the prey is constrained or grappled, which the Stirge owner does not do");
        }

        /// <summary>The Stirge-side release: no target reference, no hold buff, no owned constraints (if alive).</summary>
        private void ExpectStirgeReleased(string name, UnitEntityData stirge)
        {
            if (Attached(stirge) != null) problems.Add(name + ": the Stirge still names a target");
            if (HoldOf(stirge) != null) problems.Add(name + ": the Stirge keeps its hold buff");
            if (!stirge.Destroyed && !stirge.Descriptor.State.IsDead)
            {
                UnitState st = stirge.Descriptor.State;
                if (st.HasCondition(UnitCondition.CantMove) || st.HasCondition(UnitCondition.CantAct) || st.HasCondition(UnitCondition.LoseDexterityToAC))
                    problems.Add(name + ": the living Stirge keeps hold constraints");
            }
        }

        private sealed class Pair { internal UnitEntityData Stirge, Prey; internal Buff Unrelated; internal string Name; }

        /// <summary>An independent Stirge/prey ownership control with an unrelated effect on the prey.</summary>
        private IEnumerable<object> FreshControl(string name, Action<Pair> done)
        {
            Vector3 spot = NextSpot();
            UnitEntityData prey = SpawnPrey(spot);
            UnitEntityData stirge = prey == null ? null : SummonStirge(spot, 90f);
            if (stirge == null) { problems.Add(name + ": control not created"); yield break; }
            yield return null;
            foreach (object step in TouchAttach(name, stirge, prey)) yield return step;
            var pair = new Pair { Name = name, Stirge = stirge, Prey = prey, Unrelated = AddUnrelated(prey) };
            ExpectAttached(name, stirge, prey);
            done(pair);
        }

        /// <summary>The control still attached; or legitimately ended its four-point meal, and so no longer a witness.</summary>
        private void ExpectControl(Pair c, string at)
        {
            if (c.Unrelated == null || !c.Prey.Descriptor.Buffs.RawFacts.Contains(c.Unrelated)) problems.Add(at + ": the control prey's unrelated effect was lost");
            if (ReferenceEquals(Attached(c.Stirge), c.Prey)) { ExpectAttached(at + ".control", c.Stirge, c.Prey); return; }
            bool meal = HoldLifecycleObserver.Events.Any(e => (string)e["what"] == "stirge.OnNewRound.end" && (string)e["owner"] == c.Stirge.UniqueId && (int)e["cumulative"] == 4);
            ((JArray)(result["controlOutcomes"] ?? (result["controlOutcomes"] = new JArray()))).Add(at + ": " + c.Name + " ended; four-point meal traced " + meal + "; no longer a witness");
            if (!meal) problems.Add(at + ": control " + c.Name + " ended without completing its meal");
        }

        private void ExpectPersistentKept(string at, UnitEntityData prey, int conBefore, bool feverBefore)
        {
            if (prey.Descriptor.Stats.Constitution.Damage < conBefore) problems.Add(at + ": already-inflicted Constitution damage was healed");
            if (feverBefore && !HasBuff(prey, FilthFeverGuid)) problems.Add(at + ": native filth fever was removed");
        }

        /// <summary>No attachment-driven Constitution damage and no recreated link or action across about two rounds.</summary>
        private IEnumerable<object> Quiet(string at, UnitEntityData prey, UnitEntityData[] stirges, JObject row)
        {
            int con = prey.Descriptor.Stats.Constitution.Damage;
            TimeSpan start = Game.Instance.TimeController.GameTime;
            foreach (object step in SettleGameTime(13.0, 40f, () => true)) yield return step;
            row["quiet.gameSeconds"] = (Game.Instance.TimeController.GameTime - start).TotalSeconds;
            row["quiet.prey"] = PreyState(prey);
            row["quiet.stirges"] = new JArray(stirges.Select(s => (object)StirgeState(s)).ToArray());
            if ((double)row["quiet.gameSeconds"] < 12.0) problems.Add(at + ": fewer than two rounds of game time in the quiet check");
            if (prey.Descriptor.Stats.Constitution.Damage != con) problems.Add(at + ": the prey took Constitution damage after the release");
            if (RemoveCount(prey) != 0) problems.Add(at + ": a Remove Stirge action is back on the prey");
            foreach (UnitEntityData s in stirges) ExpectStirgeReleased(at + ".quiet", s);
        }

        private void Expire(UnitEntityData stirge)
        {
            Buff marker = stirge.Descriptor.Buffs.Enumerable.First(b => ReferenceEquals(b.Blueprint, BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff));
            typeof(Buff).GetProperty("EndTime", Any).SetValue(marker, Game.Instance.TimeController.GameTime - TimeSpan.FromMilliseconds(1), null);
            stirge.Descriptor.Buffs.UpdateNextEvent();
            stirge.Descriptor.Buffs.Tick();
        }

        // ---------------------------------------------------------------- lifecycle

        private IEnumerable<object> Lifecycle()
        {
            var cases = new JObject();
            result["cases"] = cases;

            // Two Stirges A and B on one prey P.
            Vector3 spot = NextSpot();
            UnitEntityData p = SpawnPrey(spot);
            UnitEntityData a = SummonStirge(spot, -60f), b = SummonStirge(spot, 60f);
            if (p == null || a == null || b == null) { problems.Add("two-owner: units not created"); yield break; }
            Buff unrelatedP = AddUnrelated(p);
            yield return null;
            Pair control = null;
            foreach (object step in FreshControl("control1", x => control = x)) yield return step;
            if (control == null) yield break;
            foreach (object step in TouchAttach("A", a, p)) yield return step;
            foreach (object step in TouchAttach("B", b, p)) yield return step;
            Game.Instance.IsPaused = true;
            yield return null;
            int before = problems.Count;
            var two = new JObject { ["A"] = StirgeState(a), ["B"] = StirgeState(b), ["P"] = PreyState(p),
                ["checker.PtoA"] = CheckerAllows(p, a), ["checker.PtoB"] = CheckerAllows(p, b), ["checker.PtoControlStirge"] = CheckerAllows(p, control.Stirge),
                ["checker.controlPreyToA"] = CheckerAllows(control.Prey, a), ["native.PtoA"] = NativeCanTarget(p, a), ["native.PtoB"] = NativeCanTarget(p, b) };
            cases["twoOwners"] = two;
            ExpectAttached("twoOwners.A", a, p); ExpectAttached("twoOwners.B", b, p);
            if (CheckerAllows(p, control.Stirge)) problems.Add("twoOwners: the prey's action may target a Stirge attached to someone else");
            if (CheckerAllows(control.Prey, a)) problems.Add("twoOwners: another unit's action may target A");
            ExpectControl(control, "twoOwners");
            if (problems.Count > before) yield break;

            // First-owner termination: A dies through the native death path; B stays attached.
            var first = new JObject { ["before"] = new JObject { ["A"] = StirgeState(a), ["B"] = StirgeState(b), ["P"] = PreyState(p) } };
            cases["firstOwnerDeath"] = first;
            int seq = LastSeq;
            double killedAt = Now;
            GameHelper.KillUnit(a);
            foreach (object step in SettleGameTime(0.0, 15f, () => Attached(a) == null && HoldOf(a) == null)) yield return step;
            first["releaseGameSeconds"] = Now - killedAt;
            first["after"] = new JObject { ["A"] = StirgeState(a), ["B"] = StirgeState(b), ["P"] = PreyState(p) };
            first["events"] = StirgeEvents(seq);
            ExpectStirgeReleased("firstOwnerDeath.A", a);
            ExpectAttached("firstOwnerDeath.B", b, p);
            if (CheckerAllows(p, a)) problems.Add("firstOwnerDeath: the prey's action may still target the dead A");
            ExpectControl(control, "firstOwnerDeath");
            if (problems.Count > before) yield break;
            // B's own drain continues: one round of B's meal, with A contributing nothing.
            int conBefore = p.Descriptor.Stats.Constitution.Damage, cumB = Cumulative(b);
            first["bMealBefore"] = cumB;
            if (cumB >= 3) { problems.Add("firstOwnerDeath: B's meal is too far along to observe a further round (" + cumB + ")"); yield break; }
            seq = LastSeq;
            foreach (object step in SettleGameTime(0.0, 15f, () => Cumulative(b) == cumB + 1)) yield return step;
            first["bRound"] = new JObject { ["bMealAfter"] = Cumulative(b), ["conDamage"] = new JArray(conBefore, p.Descriptor.Stats.Constitution.Damage),
                ["events"] = StirgeEvents(seq) };
            if (Cumulative(b) != cumB + 1) problems.Add("firstOwnerDeath: B did not drain in the following round");
            else if (p.Descriptor.Stats.Constitution.Damage - conBefore != 1) problems.Add("firstOwnerDeath: the prey's Constitution damage in B's round is " + (p.Descriptor.Stats.Constitution.Damage - conBefore) + ", expected B's one point only");
            if (HoldLifecycleObserver.Events.Any(e => (int)e["seq"] > seq && (string)e["what"] == "stirge.OnNewRound.begin" && (string)e["owner"] == a.UniqueId))
                problems.Add("firstOwnerDeath: A's attachment ticked after its death");
            ExpectAttached("firstOwnerDeath.B.afterRound", b, p);
            if (problems.Count > before) yield break;

            // Last-owner termination through P's actual Remove Stirge ability (native command and ability execution).
            Game.Instance.IsPaused = true;
            yield return null;
            ExpectAttached("lastOwnerRemoval.precondition", b, p);
            if (problems.Count > before) yield break;
            var last = new JObject();
            cases["lastOwnerRemoval"] = last;
            last["mobilityBefore"] = p.Descriptor.Stats.SkillMobility.ModifiedValue;
            p.Descriptor.Stats.SkillMobility.BaseValue = 300;
            last["mobilitySet"] = p.Descriptor.Stats.SkillMobility.ModifiedValue;
            int conAtRemoval = p.Descriptor.Stats.Constitution.Damage;
            bool feverAtRemoval = HasBuff(p, FilthFeverGuid);
            last["before"] = new JObject { ["B"] = StirgeState(b), ["P"] = PreyState(p) };
            seq = LastSeq;
            double castAt = Now;
            AbilityData data = p.Descriptor.Abilities.GetAbility(remove).Data;
            p.Commands.Run(UnitUseAbility.CreateCastCommand(data, new TargetWrapper(b)));
            foreach (object step in SettleGameTime(0.0, 30f, () => Attached(b) == null && HoldOf(b) == null)) yield return step;
            last["releaseGameSeconds"] = Now - castAt;
            last["after"] = new JObject { ["B"] = StirgeState(b), ["P"] = PreyState(p) };
            JArray removal = StirgeEvents(seq);
            last["events"] = removal;
            bool actionRan = removal.OfType<JObject>().Any(e => (string)e["what"] == "stirge.RemoveAction.begin" && (string)e["prey"] == p.UniqueId && (string)e["stirge"] == b.UniqueId);
            bool detachedByAction = removal.OfType<JObject>().Any(e => (string)e["what"] == "stirge.RemoveAction.end" && (string)e["prey"] == p.UniqueId && (string)e["stirge"] == b.UniqueId && (string)e["attachedTarget"] == null);
            last["removeActionRan"] = actionRan; last["detachedInsideAction"] = detachedByAction;
            if (!actionRan || !detachedByAction) problems.Add("lastOwnerRemoval: the release was not made by P's Remove Stirge action against B");
            ExpectStirgeReleased("lastOwnerRemoval.B", b);
            if (RemoveCount(p) != 0) problems.Add("lastOwnerRemoval: the shared action remains after the last attachment");
            ExpectPersistentKept("lastOwnerRemoval", p, conAtRemoval, feverAtRemoval);
            if (unrelatedP == null || !p.Descriptor.Buffs.RawFacts.Contains(unrelatedP)) problems.Add("lastOwnerRemoval: P's unrelated effect was lost");
            ExpectControl(control, "lastOwnerRemoval");
            foreach (object step in Quiet("lastOwnerRemoval", p, new[] { a, b }, last)) yield return step;
            ExpectPersistentKept("lastOwnerRemoval.quiet", p, conAtRemoval, feverAtRemoval);
            ExpectControl(control, "lastOwnerRemoval.quiet");
            last["diagnostic.repeatedDetach"] = (bool)holdType.GetMethod("Detach", Any).Invoke(null, new object[] { b });
            if (problems.Count > before) yield break;

            // Active summon expiry.
            var expiry = new JObject();
            cases["activeExpiry"] = expiry;
            Pair ctl2 = null;
            foreach (object step in FreshControl("control2", x => ctl2 = x)) yield return step;
            if (ctl2 == null) yield break;
            Vector3 spot2 = NextSpot();
            UnitEntityData r = SpawnPrey(spot2), d = SummonStirge(spot2, 0f);
            if (r == null || d == null) { problems.Add("activeExpiry: units not created"); yield break; }
            yield return null;
            foreach (object step in TouchAttach("D", d, r)) yield return step;
            Game.Instance.IsPaused = true;
            yield return null;
            ExpectAttached("activeExpiry.precondition", d, r);
            if (problems.Count > before) yield break;
            int conR = r.Descriptor.Stats.Constitution.Damage; bool feverR = HasBuff(r, FilthFeverGuid);
            expiry["before"] = new JObject { ["D"] = StirgeState(d), ["R"] = PreyState(r) };
            seq = LastSeq;
            double expiredAt = Now;
            Expire(d);
            foreach (object step in SettleGameTime(0.0, 25f, () => Attached(d) == null && HoldOf(d) == null && RemoveCount(r) == 0)) yield return step;
            expiry["releaseGameSeconds"] = Now - expiredAt;
            expiry["after"] = new JObject { ["D"] = StirgeState(d), ["R"] = PreyState(r) };
            expiry["events"] = StirgeEvents(seq);
            ExpectStirgeReleased("activeExpiry.D", d);
            if (RemoveCount(r) != 0) problems.Add("activeExpiry: the prey keeps the Remove Stirge action");
            ExpectControl(ctl2, "activeExpiry");
            foreach (object step in Quiet("activeExpiry", r, new[] { d }, expiry)) yield return step;
            ExpectPersistentKept("activeExpiry", r, conR, feverR);
            ExpectControl(ctl2, "activeExpiry.quiet");
            if (problems.Count > before) yield break;

            // Prey death while attached.
            var preyDeath = new JObject();
            cases["preyDeath"] = preyDeath;
            Pair ctl3 = null;
            foreach (object step in FreshControl("control3", x => ctl3 = x)) yield return step;
            if (ctl3 == null) yield break;
            Vector3 spot3 = NextSpot();
            UnitEntityData s3 = SpawnPrey(spot3), e3 = SummonStirge(spot3, 0f);
            if (s3 == null || e3 == null) { problems.Add("preyDeath: units not created"); yield break; }
            yield return null;
            foreach (object step in TouchAttach("E", e3, s3)) yield return step;
            Game.Instance.IsPaused = true;
            yield return null;
            ExpectAttached("preyDeath.precondition", e3, s3);
            if (problems.Count > before) yield break;
            preyDeath["before"] = new JObject { ["E"] = StirgeState(e3), ["S"] = PreyState(s3) };
            seq = LastSeq;
            double diedAt = Now;
            GameHelper.KillUnit(s3);
            foreach (object step in SettleGameTime(0.0, 20f, () => Attached(e3) == null && HoldOf(e3) == null)) yield return step;
            preyDeath["releaseGameSeconds"] = Now - diedAt;
            preyDeath["after"] = new JObject { ["E"] = StirgeState(e3), ["S"] = PreyState(s3) };
            preyDeath["events"] = StirgeEvents(seq);
            ExpectStirgeReleased("preyDeath.E", e3);
            if (RemoveCount(s3) != 0) problems.Add("preyDeath: the dead prey keeps the Remove Stirge action");
            int conS = s3.Descriptor.Stats.Constitution.Damage;
            TimeSpan quietStart = Game.Instance.TimeController.GameTime;
            foreach (object step in SettleGameTime(13.0, 40f, () => true)) yield return step;
            preyDeath["quiet.gameSeconds"] = (Game.Instance.TimeController.GameTime - quietStart).TotalSeconds;
            preyDeath["quiet.E"] = StirgeState(e3);
            if ((double)preyDeath["quiet.gameSeconds"] < 12.0) problems.Add("preyDeath: fewer than two rounds of game time in the quiet check");
            if (s3.Descriptor.Stats.Constitution.Damage != conS) problems.Add("preyDeath: attachment drain continued on the dead prey");
            ExpectStirgeReleased("preyDeath.quiet", e3);
            if (RemoveCount(s3) != 0) problems.Add("preyDeath: the Remove Stirge action came back");
            ExpectControl(ctl3, "preyDeath");
        }

        // ---------------------------------------------------------------- save and reload

        private IEnumerable<object> Save()
        {
            var cases = new JObject();
            result["cases"] = cases;
            Vector3 spot = NextSpot();
            UnitEntityData t = SpawnPrey(spot), f = SummonStirge(spot, 0f);
            UnitEntityData u = SpawnPrey(NextSpot());
            if (t == null || f == null || u == null) { problems.Add("save: units not created"); yield break; }
            Buff unrelatedT = AddUnrelated(t);
            yield return null;
            foreach (object step in TouchAttach("F", f, t)) yield return step;
            // One drain round, so the serialized meal counter and the prey's Constitution damage are nonzero.
            foreach (object step in SettleGameTime(0.0, 15f, () => Cumulative(f) >= 1)) yield return step;
            Game.Instance.IsPaused = true;
            yield return null;
            // Synthetic orphan (labelled): a Remove Stirge fact on a disposable unit that no Stirge targets.
            u.Descriptor.AddFact(remove);
            int before = problems.Count;
            ExpectAttached("save.before", f, t);
            if (Cumulative(f) < 1) problems.Add("save.before: no drain round before the save");
            if (RemoveCount(u) != 1) problems.Add("save.before: the synthetic orphan fact was not added");
            bool allowed = Game.Instance.SaveManager.IsSaveAllowed();
            cases["beforeSave"] = new JObject { ["saveAllowed"] = allowed, ["F"] = StirgeState(f), ["T"] = PreyState(t), ["syntheticOrphan"] = PreyState(u) };
            if (!allowed) problems.Add("save: native save admission refused");
            if (problems.Count > before) yield break;
            f.Descriptor.CustomName = SaveNames[0]; t.Descriptor.CustomName = SaveNames[1]; u.Descriptor.CustomName = SaveNames[2];
            foreach (UnitEntityData x in new[] { f, t, u }) { created.Remove(x); awakeAdded.Remove(x); }
            int seq = LastSeq;
            TimeSpan clock = Game.Instance.TimeController.GameTime;
            bool saved = false;
            Game.Instance.SaveGame(Game.Instance.SaveManager.CreateNewSave("ver02g4"), () => saved = true);
            float until = Time.realtimeSinceStartup + 120f;
            while ((!saved || Game.Instance.SaveManager.CommitInProgress || LoadingProcess.Instance.IsLoadingInProcess) && Time.realtimeSinceStartup < until)
                yield return null;
            bool finished = saved && !Game.Instance.SaveManager.CommitInProgress && !LoadingProcess.Instance.IsLoadingInProcess;
            var after = new JObject { ["callbackReceived"] = saved, ["commitInProgress"] = Game.Instance.SaveManager.CommitInProgress, ["loadingInProgress"] = LoadingProcess.Instance.IsLoadingInProcess,
                ["finished"] = finished, ["gameClockUnchanged"] = Game.Instance.TimeController.GameTime == clock,
                ["F"] = StirgeState(f), ["T"] = PreyState(t), ["syntheticOrphan"] = PreyState(u), ["events"] = StirgeEvents(seq) };
            cases["afterSave"] = after;
            if (!finished) { problems.Add("save: the save did not finish"); yield break; }
            // Running process after the save: observe what ended, then require the bounded legacy release.
            int conT = t.Descriptor.Stats.Constitution.Damage; bool feverT = HasBuff(t, FilthFeverGuid);
            seq = LastSeq;
            double resumedAt = Now;
            foreach (object step in SettleGameTime(0.0, 20f, () => Attached(f) == null && HoldOf(f) == null)) yield return step;
            var running = new JObject { ["releaseGameSecondsAfterResume"] = Now - resumedAt, ["F"] = StirgeState(f), ["T"] = PreyState(t), ["syntheticOrphan"] = PreyState(u), ["events"] = StirgeEvents(seq) };
            cases["runningAfterSave"] = running;
            ExpectStirgeReleased("runningAfterSave.F", f);
            if (RemoveCount(t) != 0) problems.Add("runningAfterSave: the prey keeps the Remove Stirge action");
            foreach (object step in Quiet("runningAfterSave", t, new[] { f }, running)) yield return step;
            ExpectPersistentKept("runningAfterSave", t, conT, feverT);
            if (unrelatedT == null || !t.Descriptor.Buffs.RawFacts.Contains(unrelatedT)) problems.Add("runningAfterSave: the prey's unrelated effect was lost");
            created.Add(f); created.Add(t); created.Add(u);
        }

        private IEnumerable<object> Reload()
        {
            UnitEntityData[] units = SaveNames.Select(n => Game.Instance.State.Units.FirstOrDefault(x => x.Descriptor.CustomName == n)).ToArray();
            result["found"] = new JArray(units.Select(x => (object)(x != null)).ToArray());
            if (units.Any(x => x == null)) { problems.Add("reload: a named participant is missing"); yield break; }
            created.AddRange(units);
            foreach (UnitEntityData x in units) Wake(x);
            UnitEntityData f = units[0], t = units[1], u = units[2];
            var row = new JObject();
            result["reload"] = row;
            row["loadEvents"] = Events(0, e => ((string)e["what"]).StartsWith("stirge.", StringComparison.Ordinal) || ((string)e["what"]).StartsWith("Game.", StringComparison.Ordinal) || (string)e["what"] == "ResetLoadedGrapples");
            row["atLoad"] = new JObject { ["F"] = StirgeState(f), ["T"] = PreyState(t), ["syntheticOrphan"] = PreyState(u) };
            ExpectStirgeReleased("reload.atLoad.F", f);
            if (RemoveCount(t) != 0) problems.Add("reload.atLoad: the prey has a Remove Stirge action");
            if (RemoveCount(u) != 0) problems.Add("reload.atLoad: the synthetic orphan Remove Stirge fact survived the load");
            if (!HasBuff(t, UnrelatedBuffGuid)) problems.Add("reload.atLoad: the prey's unrelated effect was lost");
            int conT = t.Descriptor.Stats.Constitution.Damage; bool feverT = HasBuff(t, FilthFeverGuid);
            foreach (object step in Quiet("reload", t, new[] { f }, row)) yield return step;
            ExpectPersistentKept("reload", t, conT, feverT);
            if (RemoveCount(u) != 0) problems.Add("reload: the synthetic orphan Remove Stirge fact came back");
        }
    }
}
