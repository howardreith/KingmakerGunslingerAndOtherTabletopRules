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
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KmgLoaderCompatHarness
{
    /// <summary>
    /// Sprint 6 checkpoint C3a (VER-02, G3): swallow/engulf lifecycle of the Purple Worm (single-link hold promoted to
    /// swallow whole) and the Giant Flytrap (multi-link hold promoted to engulf), on the accepted C2a R1 product.
    ///
    /// Every principal relationship is made the ordinary way: the swallower is summoned through the product's own
    /// execution ability, a real eligible bite (the engine's RuleAttackWithWeapon) establishes the hold, and the
    /// product's own later-round maintain promotes it (MaintainLink "swallowed:"). Nothing inserts parts, payloads or
    /// records, and SwallowHeld / native Swallow are never called by the probe.
    ///
    /// Disclosed setup: holder BAB 100 (hit, grapple, maintain), victims' HP raised, appear buffs removed, brains off,
    /// units woken, navmesh placement. Medium DefaultPlayerCharacter spawns are swallow/engulf-eligible; a Large horse is
    /// engulf-ineligible and stays held. Maintain results and swallow callbacks are recorded by the read-only
    /// HoldLifecycleObserver (launch condition observeHoldLifecycle).
    ///
    /// Modes: "victims" (promotion, payload rounds, victim death with consequence checks, Flytrap mixed case and its
    /// expiry), "holders" (Worm holder death, Flytrap combat-traits carrier removal, a repeated terminal), "save"
    /// (an active Worm swallow and an active Flytrap engulf with a held sibling through a native save), and "reload"
    /// (the fresh process loading that save).
    /// </summary>
    internal sealed class SwallowProbe : IOwnedProbe
    {
        private const BindingFlags Any = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const string UnrelatedBuffGuid = "b175001b42b1a02479881b72fe132116";
        internal const string SaveLabel = "ver02c3a";

        private readonly string mode;
        private readonly JObject result = new JObject();
        private readonly List<string> problems = new List<string>();
        private readonly IEnumerator<object> script;
        private readonly List<UnitEntityData> created = new List<UnitEntityData>();
        private readonly List<UnitEntityData> awakeAdded = new List<UnitEntityData>();
        private string verdict;
        private Assembly product;
        private Type runner, linkPart, links, grabType, limbs;
        private Vector3 wormSpot, trapSpot;
        private bool promotionOk;

        public SwallowProbe(string mode) { this.mode = mode; result["mode"] = mode; script = Run().GetEnumerator(); }
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
            linkPart = product.GetType("KingmakerGunslinger.Summoning.UnitPartSummonGrappleLinks", true);
            links = product.GetType("KingmakerGunslinger.Summoning.SummonGrappleLinks", true);
            grabType = product.GetType("KingmakerGunslinger.Summoning.SummonGrabComponent", true);
            limbs = product.GetType("KingmakerGunslinger.Summoning.SummonLimbs", true);
            if (!HoldLifecycleObserver.Installed) { problems.Add("the hold lifecycle observer is not installed (launch condition observeHoldLifecycle)"); yield break; }
            result["area"] = Game.Instance.CurrentlyLoadedArea == null ? null : Game.Instance.CurrentlyLoadedArea.name;
            result["turnBased"] = TurnBased.Controllers.CombatController.IsInTurnBasedCombat();
            UnitEntityData main = Game.Instance.Player.MainCharacter.Value;
            Vector3 right = Quaternion.Euler(0, main.Orientation + 90f, 0) * Vector3.forward;
            wormSpot = ObstacleAnalyzer.TraceAlongNavmesh(main.Position, main.Position - right * 7f);
            trapSpot = ObstacleAnalyzer.TraceAlongNavmesh(main.Position, main.Position + right * 7f);
            bool paused = Game.Instance.IsPaused;
            try
            {
                IEnumerable<object> body = mode == "victims" ? Victims() : mode == "holders" ? Holders() : mode == "save" ? Save() : mode == "reload" ? Reload() :
                    mode == "reach" ? Reach() : null;
                if (body == null) { problems.Add("unknown mode " + mode); yield break; }
                foreach (object step in body) yield return step;
            }
            finally { Game.Instance.IsPaused = paused; }
        }

        // ---------------------------------------------------------------- fixture

        private sealed class Rel
        {
            internal string Name;
            internal UnitEntityData Holder;
            internal UnitEntityData[] Targets;
            internal ItemEntityWeapon[] Weapons;
            internal object Grab;
            internal BlueprintBuff HoldBuff, HeldBuff, SwallowedBuff, TraitsBuff;
            internal bool Multi;
        }

        private UnitEntityData Summon(string key) { return Summon(key, false); }

        private UnitEntityData Summon(string key, bool keepBrain)
        {
            UnitEntityData main = Game.Instance.Player.MainCharacter.Value;
            Type multiplicity = product.GetType("KingmakerGunslinger.Summoning.SummonMultiplicity", true);
            object variant = runner.GetMethod("ExpandedSummoningOwnTierVariant", Any).Invoke(null, new[] { key, Enum.Parse(multiplicity, "One") });
            object evidence = Activator.CreateInstance(runner.GetNestedType("ExpandedSummoningMechanicalEvidence", Any), true);
            var library = (LibraryScriptableObject)product.GetType("KingmakerGunslinger.Bootstrap.BlueprintBootstrap", true).GetProperty("Library", Any).GetValue(null, null);
            var units = (UnitEntityData[])runner.GetMethods(Any).Single(m => m.Name == "CastExpandedSummoningVariant" && m.GetParameters().Length == 5)
                .Invoke(null, new object[] { library.GetAllBlueprints().ToArray(), main, variant, null, evidence });
            UnitEntityData unit = units.Length == 1 ? units[0] : null;
            if (unit == null) return null;
            created.Add(unit);
            runner.GetMethod("RemoveExpandedSummoningAppearanceBuffs", Any).Invoke(null, new object[] { unit });
            if (!keepBrain) runner.GetMethod("SetExpandedSummoningBrainActive", Any).Invoke(null, new object[] { unit, false });
            Wake(unit);
            return unit;
        }

        private UnitEntityData SpawnMedium(Vector3 at)
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

        private void Place(UnitEntityData unit, Vector3 position) { runner.GetMethod("PlaceExpandedSummoningUnit", Any).Invoke(null, new object[] { unit, position }); }

        private ItemEntityWeapon[] GrabWeapons(UnitEntityData holder, object grab)
        {
            var list = new List<ItemEntityWeapon>();
            MethodInfo isGrabLimb = grabType.GetMethod("IsGrabLimb", Any);
            var primary = (ItemEntityWeapon)limbs.GetMethod("PrimaryWeapon", Any).Invoke(null, new object[] { holder });
            if (primary != null && (bool)isGrabLimb.Invoke(grab, new object[] { holder, primary })) list.Add(primary);
            if (holder.Body.AdditionalLimbs != null)
                foreach (var slot in holder.Body.AdditionalLimbs)
                    if (slot != null && slot.MaybeWeapon != null && (bool)isGrabLimb.Invoke(grab, new object[] { holder, slot.MaybeWeapon })) list.Add(slot.MaybeWeapon);
            return list.ToArray();
        }

        private Buff TraitsOf(UnitEntityData holder)
        {
            return holder.Descriptor.Buffs.RawFacts.OfType<Buff>().FirstOrDefault(b => b.Blueprint != null && b.Blueprint.ComponentsArray != null &&
                b.Blueprint.ComponentsArray.Any(c => c.GetType().Name == "SummonSwallowLifecycleComponent"));
        }

        /// <summary>
        /// A swallower holding its victims. kinds[i] is "medium" (swallow/engulf-eligible) or "horse" (Large, engulf-ineligible
        /// for the Flytrap). Holds come from real bites on distinct grab limbs.
        /// </summary>
        private IEnumerable<object> Establish(string name, string key, Vector3 spot, string[] kinds, Action<Rel> done)
        {
            UnitEntityData holder = Summon(key);
            if (holder == null) { problems.Add(name + ": " + key + " not summoned"); yield break; }
            Place(holder, spot);
            holder.Descriptor.Stats.BaseAttackBonus.BaseValue = 100;
            yield return null;
            object grab = grabType.GetMethod("Find", Any).Invoke(null, new object[] { holder });
            if (grab == null) { problems.Add(name + ": no grab component"); yield break; }
            var r = new Rel
            {
                Name = name, Holder = holder, Grab = grab, Multi = (bool)grabType.GetProperty("MultiLink", Any).GetValue(grab, null),
                HoldBuff = (BlueprintBuff)grabType.GetField("HoldBuff").GetValue(grab),
                HeldBuff = (BlueprintBuff)grabType.GetField("GrappledBuff").GetValue(grab),
                SwallowedBuff = (BlueprintBuff)grabType.GetField("SwallowedBuff").GetValue(grab),
                Weapons = GrabWeapons(holder, grab)
            };
            r.TraitsBuff = TraitsOf(holder)?.Blueprint;
            var targets = new List<UnitEntityData>();
            for (int index = 0; index < kinds.Length; index++)
            {
                float angle = holder.Orientation + (kinds.Length == 1 ? 0f : index == 0 ? -40f : 40f);
                Vector3 at = ObstacleAnalyzer.TraceAlongNavmesh(holder.Position, holder.Position + Quaternion.Euler(0, angle, 0) * Vector3.forward * (key == "purple-worm" ? 4.0f : key == "crocodile" ? 1.0f : 3.0f));
                UnitEntityData t;
                if (kinds[index] == "horse")
                {
                    t = Summon("horse");
                    if (t != null) { t.Descriptor.Stats.HitPoints.BaseValue = 100000; Place(t, at); }
                }
                else t = SpawnMedium(at);
                if (t == null) { problems.Add(name + ": victim " + index + " not created"); yield break; }
                targets.Add(t);
            }
            r.Targets = targets.ToArray();
            yield return null;
            var eligibility = new JArray();
            MethodInfo swallowSize = grabType.GetMethod("IsSwallowSizeAllowed", Any);
            foreach (UnitEntityData t in r.Targets)
                eligibility.Add(new JObject { ["id"] = t.UniqueId, ["size"] = t.Descriptor.State.Size.ToString(),
                    ["swallowSizeAllowed"] = (bool)swallowSize.Invoke(grab, new object[] { holder, t }) });
            result["eligibility." + name] = eligibility;
            for (int index = 0; index < r.Targets.Length; index++)
                for (int attack = 0; attack < 6 && !IsHeld(r, index) && !IsSwallowed(r, index); attack++)
                {
                    Rulebook.Trigger(new RuleAttackWithWeapon(holder, r.Targets[index], r.Weapons[index], 0));
                    yield return null;
                }
            result["established." + name] = State(r);
            for (int index = 0; index < r.Targets.Length; index++)
                if (!IsHeld(r, index)) { problems.Add(name + ": hold " + index + " was not established by the bite"); yield break; }
            done(r);
        }

        /// <summary>Lets the product's own later-round maintain promote every eligible victim (bounded by game time).</summary>
        private IEnumerable<object> AwaitPromotion(Rel r, int[] eligible, string name)
        {
            int seq = LastSeq;
            foreach (object step in SettleGameTime(1.0, 45f, () => eligible.All(i => IsSwallowed(r, i)))) yield return step;
            JArray maintain = Events(seq, e => (string)e["what"] == "MaintainLink" && (string)e["owner"] == r.Holder.UniqueId);
            result["promotion." + name] = new JObject { ["maintain"] = maintain, ["state"] = State(r),
                ["nativeSwallow"] = Events(seq, e => (string)e["what"] == "native.Swallow" && (string)e["owner"] == r.Holder.UniqueId) };
            promotionOk = eligible.All(i => IsSwallowed(r, i));
            foreach (int i in eligible)
                if (!IsSwallowed(r, i))
                    problems.Add(name + ": victim " + i + " was not promoted by the maintain path (maintain trace: " + maintain.ToString(Formatting.None) + ")");
        }

        private Buff AddUnrelated(UnitEntityData unit)
        {
            var blueprint = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>(UnrelatedBuffGuid);
            return blueprint == null ? null : unit.Descriptor.Buffs.AddBuff(blueprint, unit, TimeSpan.FromMinutes(30));
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

        // ---------------------------------------------------------------- observation

        private bool IsHeld(Rel r, int i)
        {
            UnitEntityData t = r.Targets[i];
            if (!r.Multi)
            {
                UnitPartGrappleInitiator init = r.Holder.Get<UnitPartGrappleInitiator>();
                UnitPartGrappleTarget held = t.Get<UnitPartGrappleTarget>();
                return init != null && held != null && ReferenceEquals(init.Target.Value, t) && ReferenceEquals(held.Initiator.Value, r.Holder);
            }
            return t.Descriptor.Buffs.RawFacts.OfType<Buff>().Any(b => ReferenceEquals(b.Blueprint, r.HeldBuff) && b.Context != null && ReferenceEquals(b.Context.MaybeCaster, r.Holder));
        }

        private bool IsSwallowed(Rel r, int i)
        {
            UnitEntityData t = r.Targets[i];
            UnitPartSwallowWhole whole = r.Holder.Get<UnitPartSwallowWhole>();
            UnitPartSwallowed part = t.Get<UnitPartSwallowed>();
            return whole != null && whole.SwallowedUnits.Any(u => ReferenceEquals(u.Value, t)) && part != null && ReferenceEquals(part.Swallower.Value, r.Holder);
        }

        private IList<string> RawLinks(UnitEntityData holder)
        {
            if (holder.Destroyed) return null;
            object part = typeof(UnitDescriptor).GetMethods().Single(m => m.Name == "Get" && m.IsGenericMethodDefinition && m.GetParameters().Length == 0)
                .MakeGenericMethod(linkPart).Invoke(holder.Descriptor, null);
            return part == null ? null : (IList<string>)linkPart.GetField("_links", Any).GetValue(part);
        }

        private JObject State(Rel r)
        {
            UnitEntityData h = r.Holder;
            UnitPartSwallowWhole whole = h.Destroyed ? null : h.Get<UnitPartSwallowWhole>();
            var reconciled = h.Destroyed ? new object[0] : ((IEnumerable)links.GetMethod("Reconcile", Any).Invoke(null, new object[] { h })).Cast<object>()
                .Select(x => (object)(x.GetType().GetProperty("TargetId").GetValue(x, null) + "|engulfed=" + x.GetType().GetProperty("Engulfed").GetValue(x, null))).ToArray();
            MethodInfo occupant = links.GetMethod("OccupantOf", Any);
            var s = new JObject
            {
                ["holder"] = h.UniqueId, ["holderDead"] = h.Descriptor.State.IsDead, ["holderDestroyed"] = h.Destroyed,
                ["native.swallowedUnits"] = whole == null ? null : new JArray(whole.SwallowedUnits.Select(u => (object)u.Value?.UniqueId).ToArray()),
                ["raw.links"] = RawLinks(h) == null ? null : new JArray(RawLinks(h).Cast<object>().ToArray()),
                ["live.reconciled"] = new JArray(reconciled),
                ["live.heldCount"] = h.Destroyed || r.Grab == null ? -1 : (int)grabType.GetMethod("HeldCount", Any).Invoke(r.Grab, new object[] { h }),
                ["occupancy"] = h.Destroyed ? null : new JArray(r.Weapons.Take(Math.Max(1, r.Targets.Length)).Select(w => (object)((occupant.Invoke(null, new object[] { h, w }) as UnitEntityData)?.UniqueId)).ToArray()),
                ["holdBuff"] = !h.Destroyed && h.Descriptor.Buffs.RawFacts.OfType<Buff>().Any(b => ReferenceEquals(b.Blueprint, r.HoldBuff)),
                ["traitsBuff"] = !h.Destroyed && TraitsOf(h) != null,
                ["initiatorPart"] = !h.Destroyed && h.Get<UnitPartGrappleInitiator>() != null,
                ["gameSeconds"] = Game.Instance.TimeController.GameTime.TotalSeconds
            };
            var ts = new JArray();
            foreach (UnitEntityData t in r.Targets)
            {
                UnitState st = t.Descriptor.State;
                UnitPartSwallowed sw = t.Get<UnitPartSwallowed>();
                Buff payload = r.SwallowedBuff == null ? null : t.Descriptor.Buffs.RawFacts.OfType<Buff>().FirstOrDefault(b => ReferenceEquals(b.Blueprint, r.SwallowedBuff));
                ts.Add(new JObject
                {
                    ["id"] = t.UniqueId, ["dead"] = st.IsDead, ["destroyed"] = t.Destroyed,
                    ["native.swallowedPart"] = sw != null, ["native.swallower"] = sw?.Swallower.Value?.UniqueId, ["native.shouldBeFree"] = sw != null && sw.ShouldBeFree,
                    ["payload"] = payload == null ? null : new JObject { ["blueprint"] = payload.Blueprint.name, ["caster"] = payload.Context?.MaybeCaster?.UniqueId,
                        ["roundNumber"] = payload.RoundNumber, ["active"] = payload.Active },
                    ["heldBuff"] = t.Descriptor.Buffs.RawFacts.OfType<Buff>().Any(b => ReferenceEquals(b.Blueprint, r.HeldBuff)),
                    ["targetPart"] = t.Get<UnitPartGrappleTarget>() != null,
                    ["untargetable"] = (bool)st.IsUntargetable,
                    ["cantMove"] = st.HasCondition(UnitCondition.CantMove), ["cantAct"] = st.HasCondition(UnitCondition.CantAct),
                    ["entangled"] = st.HasCondition(UnitCondition.Entangled), ["prone"] = st.HasCondition(UnitCondition.Prone),
                    ["canMoveFlag"] = st.CanMove, ["canActFlag"] = st.CanAct,
                    ["damage"] = t.Descriptor.Damage,
                    ["buffs"] = new JArray(t.Descriptor.Buffs.RawFacts.OfType<Buff>().Select(b => (object)b.Blueprint.name).ToArray())
                });
            }
            s["targets"] = ts;
            return s;
        }

        private JObject T(JObject s, int i) => (JObject)((JArray)s["targets"])[i];

        private void ExpectSwallowed(Rel r, int i, string name)
        {
            JObject s = State(r), t = T(s, i);
            string id = r.Targets[i].UniqueId;
            if (!IsSwallowed(r, i)) problems.Add(name + ": victim " + i + " is not in a reciprocal native swallow");
            if (t["payload"].Type == JTokenType.Null || (string)t["payload"]["caster"] != r.Holder.UniqueId) problems.Add(name + ": victim " + i + " lacks the owned payload: " + t.ToString(Formatting.None));
            if (!(bool)t["cantMove"] || !(bool)t["cantAct"] || !(bool)t["untargetable"]) problems.Add(name + ": victim " + i + " lacks the swallowed constraints");
            if ((bool)t["heldBuff"] || (bool)t["targetPart"]) problems.Add(name + ": victim " + i + " still carries the superseded held state");
            var raw = s["raw.links"] as JArray ?? new JArray();
            if (raw.Count(l => ((string)l).StartsWith(id + "|", StringComparison.Ordinal) && ((string)l).Split('|')[4] == "1") != 1) problems.Add(name + ": victim " + i + " has no single raw Engulfed record: " + raw);
            if (!((JArray)s["live.reconciled"]).Any(l => ((string)l) == id + "|engulfed=True")) problems.Add(name + ": victim " + i + " is not a live engulfed link");
            var occ = s["occupancy"] as JArray;
            if (occ == null || (string)occ[i] != id) problems.Add(name + ": victim " + i + "'s establishing mouth is not occupied by it");
        }

        private void ExpectHeld(Rel r, int i, string name)
        {
            JObject s = State(r), t = T(s, i);
            string id = r.Targets[i].UniqueId;
            if (!IsHeld(r, i)) problems.Add(name + ": target " + i + " is not held");
            if (IsSwallowed(r, i)) problems.Add(name + ": target " + i + " was swallowed");
            if (!(bool)t["cantMove"]) problems.Add(name + ": target " + i + " lacks CantMove");
            var occ = s["occupancy"] as JArray;
            if (occ == null || (string)occ[i] != id) problems.Add(name + ": target " + i + "'s mouth is not occupied by it");
        }

        /// <summary>A released victim: no reciprocal swallow, no payload or held state, and (if alive) its capabilities back.</summary>
        private void ExpectFreed(Rel r, int i, string name)
        {
            JObject s = State(r), t = T(s, i);
            if (IsSwallowed(r, i) || (bool)t["native.swallowedPart"] || (s["native.swallowedUnits"] as JArray ?? new JArray()).Any(v => (string)v == r.Targets[i].UniqueId))
                problems.Add(name + ": victim " + i + " still has a native swallow relationship: " + t.ToString(Formatting.None));
            if (t["payload"].Type != JTokenType.Null) problems.Add(name + ": victim " + i + " keeps the swallowed payload");
            if ((bool)t["heldBuff"] || (bool)t["targetPart"]) problems.Add(name + ": victim " + i + " keeps a held state");
            if ((s["live.reconciled"] as JArray ?? new JArray()).Any(l => ((string)l).StartsWith(r.Targets[i].UniqueId + "|", StringComparison.Ordinal))) problems.Add(name + ": victim " + i + " still answers as a live link");
            if (!(bool)t["dead"] && ((bool)t["cantMove"] || (bool)t["cantAct"] || (bool)t["untargetable"] || (bool)t["entangled"]))
                problems.Add(name + ": living victim " + i + " is still constrained: " + t.ToString(Formatting.None));
        }

        /// <summary>No renewed relationship or continuing payload damage across bounded engine rounds.</summary>
        private IEnumerable<object> ExpectQuiet(Rel r, int[] survivors, string name, JObject row)
        {
            int[] damage = survivors.Select(i => r.Targets[i].Descriptor.Damage).ToArray();
            TimeSpan start = Game.Instance.TimeController.GameTime;
            foreach (object step in SettleGameTime(13.0, 40f, () => true)) yield return step;
            row["quiet.gameSeconds"] = (Game.Instance.TimeController.GameTime - start).TotalSeconds;
            row["quiet.after"] = State(r);
            for (int k = 0; k < survivors.Length; k++)
            {
                int i = survivors[k];
                if (r.Targets[i].Descriptor.Damage != damage[k]) problems.Add(name + ": released victim " + i + " took damage over the following rounds");
                ExpectFreed(r, i, name + ".quiet");
            }
            if ((double)row["quiet.gameSeconds"] < 12.0) problems.Add(name + ": fewer than two rounds of game time passed in the quiet check");
        }

        // ---------------------------------------------------------------- victims

        private IEnumerable<object> Victims()
        {
            var cases = new JObject();
            result["cases"] = cases;
            Rel control = null;
            foreach (object step in EstablishControl(x => control = x)) yield return step;
            if (control == null) yield break;

            // Worm: promotion, payload rounds, then the swallowed victim's death.
            Rel w = null;
            foreach (object step in Establish("wormVictim", "purple-worm", wormSpot, new[] { "medium" }, x => w = x)) yield return step;
            if (w == null) yield break;
            foreach (object step in AwaitPromotion(w, new[] { 0 }, "wormVictim")) yield return step;
            if (!promotionOk) yield break;
            ExpectSwallowed(w, 0, "wormVictim.promoted");
            var row = new JObject { ["promoted"] = State(w) };
            cases["wormVictimDeath"] = row;
            int dmg = w.Targets[0].Descriptor.Damage;
            foreach (object step in SettleGameTime(7.0, 30f, () => true)) yield return step;
            row["afterPayloadRound"] = State(w);
            row["payloadDamage"] = w.Targets[0].Descriptor.Damage - dmg;
            if ((int)row["payloadDamage"] <= 0) problems.Add("wormVictim: no payload damage in a real round");
            int seq = LastSeq;
            GameHelper.KillUnit(w.Targets[0]);
            foreach (object step in SettleGameTime(7.0, 30f, () => true)) yield return step;
            row["afterVictimDeath"] = State(w);
            row["callbacks"] = Events(seq, e => ((string)e["what"]).StartsWith("native.", StringComparison.Ordinal) || ((string)e["what"]).StartsWith("swallow.", StringComparison.Ordinal) || (string)e["what"] == "MaintainLink");
            // Consequence: can the worm's mouth take a new victim through a real bite?
            UnitEntityData fresh = SpawnMedium(ObstacleAnalyzer.TraceAlongNavmesh(w.Holder.Position, w.Holder.Position + w.Holder.OrientationDirection * 4f));
            int bites = 0;
            for (; bites < 4; bites++)
            {
                Rulebook.Trigger(new RuleAttackWithWeapon(w.Holder, fresh, w.Weapons[0], 0));
                yield return null;
                if (fresh.Get<UnitPartGrappleTarget>() != null) break;
            }
            row["reuse"] = new JObject { ["bites"] = bites, ["newTargetHeld"] = fresh.Get<UnitPartGrappleTarget>() != null, ["state"] = State(w) };
            ExpectFreed(w, 0, "wormVictimDeath");
            if (fresh.Get<UnitPartGrappleTarget>() == null) problems.Add("wormVictimDeath: the worm's freed mouth could not take a new victim (reuse failed)");
            ExpectHeld(control, 0, "wormVictimDeath.control");

            // Flytrap mixed: an engulfed Medium victim on one mouth and a held Large horse on another; the engulfed victim dies.
            Rel f = null;
            foreach (object step in Establish("trapMixed", "giant-flytrap", trapSpot, new[] { "medium", "horse" }, x => f = x)) yield return step;
            if (f == null) yield break;
            Buff unrelated = AddUnrelated(f.Targets[1]);
            foreach (object step in AwaitPromotion(f, new[] { 0 }, "trapMixed")) yield return step;
            if (!promotionOk) yield break;
            ExpectSwallowed(f, 0, "trapMixed.promoted");
            ExpectHeld(f, 1, "trapMixed.promoted.sibling");
            row = new JObject { ["promoted"] = State(f) };
            cases["trapEngulfedVictimDeath"] = row;
            dmg = f.Targets[0].Descriptor.Damage;
            foreach (object step in SettleGameTime(7.0, 30f, () => true)) yield return step;
            row["afterPayloadRound"] = State(f);
            row["payloadDamage"] = f.Targets[0].Descriptor.Damage - dmg;
            if ((int)row["payloadDamage"] <= 0) problems.Add("trapMixed: no payload damage in a real round");
            ExpectHeld(f, 1, "trapMixed.afterRound.sibling");
            seq = LastSeq;
            GameHelper.KillUnit(f.Targets[0]);
            foreach (object step in SettleGameTime(7.0, 30f, () => true)) yield return step;
            row["afterVictimDeath"] = State(f);
            row["callbacks"] = Events(seq, e => ((string)e["what"]).StartsWith("native.", StringComparison.Ordinal) || ((string)e["what"]).StartsWith("swallow.", StringComparison.Ordinal) || (string)e["what"] == "MaintainLink");
            ExpectFreed(f, 0, "trapEngulfedVictimDeath");
            ExpectHeld(f, 1, "trapEngulfedVictimDeath.sibling");
            if (unrelated == null || !f.Targets[1].Descriptor.Buffs.RawFacts.Contains(unrelated)) problems.Add("trapEngulfedVictimDeath: the sibling's unrelated effect was not kept");
            UnitEntityData third = SpawnMedium(ObstacleAnalyzer.TraceAlongNavmesh(f.Holder.Position, f.Holder.Position + f.Holder.OrientationDirection * 3f));
            int bites2 = 0;
            for (; bites2 < 4; bites2++)
            {
                Rulebook.Trigger(new RuleAttackWithWeapon(f.Holder, third, f.Weapons[0], 0));
                yield return null;
                if (third.Descriptor.Buffs.RawFacts.OfType<Buff>().Any(b => ReferenceEquals(b.Blueprint, f.HeldBuff))) break;
            }
            bool thirdHeld = third.Descriptor.Buffs.RawFacts.OfType<Buff>().Any(b => ReferenceEquals(b.Blueprint, f.HeldBuff));
            row["reuse"] = new JObject { ["bites"] = bites2, ["newTargetHeld"] = thirdHeld, ["state"] = State(f) };
            if (!thirdHeld) problems.Add("trapEngulfedVictimDeath: the freed mouth could not take a new victim (reuse failed)");
            ExpectHeld(control, 0, "trapEngulfedVictimDeath.control");

            // Flytrap holder-wide: accelerated summon expiry (shortened deadline, explicit native UpdateNextEvent/Tick).
            row = new JObject { ["before"] = State(f) };
            cases["trapExpiry"] = row;
            seq = LastSeq;
            Buff marker = f.Holder.Descriptor.Buffs.Enumerable.First(b => ReferenceEquals(b.Blueprint, BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff));
            typeof(Buff).GetProperty("EndTime", Any).SetValue(marker, Game.Instance.TimeController.GameTime - TimeSpan.FromMilliseconds(1), null);
            f.Holder.Descriptor.Buffs.UpdateNextEvent();
            f.Holder.Descriptor.Buffs.Tick();
            foreach (object step in SettleGameTime(1.0, 20f, () => f.Holder.Destroyed && !IsHeld(f, 1))) yield return step;
            row["after"] = State(f);
            row["callbacks"] = Events(seq, e => ((string)e["what"]).StartsWith("native.", StringComparison.Ordinal) || ((string)e["what"]).StartsWith("swallow.", StringComparison.Ordinal) || ((string)e["what"]).StartsWith("SummonHoldComponent.", StringComparison.Ordinal));
            if (!f.Holder.Destroyed) problems.Add("trapExpiry: the holder was not destroyed");
            ExpectFreed(f, 1, "trapExpiry");
            ExpectHeld(control, 0, "trapExpiry.control");
            cases["controlFinal"] = State(control);
        }

        private IEnumerable<object> EstablishControl(Action<Rel> done)
        {
            Rel c = null;
            foreach (object step in Establish("control", "crocodile", Game.Instance.Player.MainCharacter.Value.Position, new[] { "medium" }, x => c = x)) yield return step;
            if (c != null) AddUnrelated(c.Targets[0]);
            done(c);
        }

        // ---------------------------------------------------------------- holders

        private IEnumerable<object> Holders()
        {
            var cases = new JObject();
            result["cases"] = cases;
            Rel control = null;
            foreach (object step in EstablishControl(x => control = x)) yield return step;
            if (control == null) yield break;

            // Worm holder death with an active swallow.
            Rel w = null;
            foreach (object step in Establish("wormHolder", "purple-worm", wormSpot, new[] { "medium" }, x => w = x)) yield return step;
            if (w == null) yield break;
            Buff unrelated = AddUnrelated(w.Targets[0]);
            foreach (object step in AwaitPromotion(w, new[] { 0 }, "wormHolder")) yield return step;
            if (!promotionOk) yield break;
            ExpectSwallowed(w, 0, "wormHolder.promoted");
            var row = new JObject { ["before"] = State(w) };
            cases["wormHolderDeath"] = row;
            int seq = LastSeq;
            GameHelper.KillUnit(w.Holder);
            foreach (object step in SettleGameTime(1.0, 20f, () => !IsSwallowed(w, 0))) yield return step;
            row["after"] = State(w);
            row["callbacks"] = Events(seq, e => ((string)e["what"]).StartsWith("native.", StringComparison.Ordinal) || ((string)e["what"]).StartsWith("swallow.", StringComparison.Ordinal));
            ExpectFreed(w, 0, "wormHolderDeath");
            if (unrelated == null || !w.Targets[0].Descriptor.Buffs.RawFacts.Contains(unrelated)) problems.Add("wormHolderDeath: the freed victim's unrelated effect was not kept");
            foreach (object step in ExpectQuiet(w, new[] { 0 }, "wormHolderDeath", row)) yield return step;
            ExpectHeld(control, 0, "wormHolderDeath.control");

            // A repeated terminal on the ended relationship: the dead worm's own expiry deadline passes natively.
            row = new JObject { ["before"] = State(w) };
            cases["repeatedTerminal"] = row;
            Buff marker = w.Holder.Descriptor.Buffs.Enumerable.FirstOrDefault(b => ReferenceEquals(b.Blueprint, BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff));
            if (marker != null)
            {
                typeof(Buff).GetProperty("EndTime", Any).SetValue(marker, Game.Instance.TimeController.GameTime - TimeSpan.FromMilliseconds(1), null);
                w.Holder.Descriptor.Buffs.UpdateNextEvent();
                w.Holder.Descriptor.Buffs.Tick();
            }
            foreach (object step in SettleGameTime(2.0, 20f, () => true)) yield return step;
            row["markerPresent"] = marker != null;
            row["after"] = State(w);
            ExpectFreed(w, 0, "repeatedTerminal");
            ExpectHeld(control, 0, "repeatedTerminal.control");

            // Flytrap: genuine removal of the combat-traits carrier that owns the swallow lifecycle (engulfed + held sibling).
            Rel f = null;
            foreach (object step in Establish("trapCarrier", "giant-flytrap", trapSpot, new[] { "medium", "horse" }, x => f = x)) yield return step;
            if (f == null) yield break;
            foreach (object step in AwaitPromotion(f, new[] { 0 }, "trapCarrier")) yield return step;
            if (!promotionOk) yield break;
            ExpectSwallowed(f, 0, "trapCarrier.promoted");
            ExpectHeld(f, 1, "trapCarrier.promoted.sibling");
            row = new JObject { ["before"] = State(f) };
            cases["trapCarrierRemoval"] = row;
            seq = LastSeq;
            Buff traits = TraitsOf(f.Holder);
            row["carrier"] = traits == null ? null : traits.Blueprint.name;
            f.Holder.Descriptor.Buffs.RemoveFact(traits);
            row["immediately"] = State(f);
            foreach (object step in SettleGameTime(7.0, 30f, () => true)) yield return step;
            row["afterRound"] = State(f);
            row["callbacks"] = Events(seq, e => true);
            row["holderAlive"] = !f.Holder.Descriptor.State.IsDead && !f.Holder.Destroyed;
            ExpectFreed(f, 0, "trapCarrierRemoval.engulfed");
            ExpectFreed(f, 1, "trapCarrierRemoval.held");
            foreach (object step in ExpectQuiet(f, new[] { 0, 1 }, "trapCarrierRemoval", row)) yield return step;
            ExpectHeld(control, 0, "trapCarrierRemoval.control");
            cases["controlFinal"] = State(control);
        }

        // ---------------------------------------------------------------- save

        private IEnumerable<object> Save()
        {
            var cases = new JObject();
            result["cases"] = cases;
            Rel w = null, f = null;
            foreach (object step in Establish("wormSave", "purple-worm", wormSpot, new[] { "medium" }, x => w = x)) yield return step;
            foreach (object step in Establish("trapSave", "giant-flytrap", trapSpot, new[] { "medium", "horse" }, x => f = x)) yield return step;
            if (w == null || f == null) yield break;
            int[] none = new int[0];
            foreach (object step in AwaitPromotion(w, new[] { 0 }, "wormSave")) yield return step;
            foreach (object step in AwaitPromotion(f, new[] { 0 }, "trapSave")) yield return step;
            if (problems.Count > 0) yield break;
            Game.Instance.IsPaused = true;
            yield return null;
            ExpectSwallowed(w, 0, "save.before"); ExpectSwallowed(f, 0, "save.before"); ExpectHeld(f, 1, "save.before.sibling");
            bool allowed = Game.Instance.SaveManager.IsSaveAllowed();
            var row = new JObject { ["saveAllowed"] = allowed, ["inCombat"] = Game.Instance.Player.IsInCombat, ["worm"] = State(w), ["trap"] = State(f) };
            cases["beforeSave"] = row;
            if (!allowed) { problems.Add("save: native save admission refused the save"); yield break; }
            if (problems.Count > 0) yield break;
            w.Holder.Descriptor.CustomName = "KMG_C3a_Worm"; w.Targets[0].Descriptor.CustomName = "KMG_C3a_WormVictim";
            f.Holder.Descriptor.CustomName = "KMG_C3a_Trap"; f.Targets[0].Descriptor.CustomName = "KMG_C3a_TrapVictim"; f.Targets[1].Descriptor.CustomName = "KMG_C3a_TrapHeld";
            foreach (UnitEntityData u in new[] { w.Holder, f.Holder }.Concat(w.Targets).Concat(f.Targets)) { created.Remove(u); awakeAdded.Remove(u); }
            int seq = LastSeq;
            TimeSpan clock = Game.Instance.TimeController.GameTime;
            bool saved = false;
            Game.Instance.SaveGame(Game.Instance.SaveManager.CreateNewSave(SaveLabel), () => saved = true);
            float until = Time.realtimeSinceStartup + 120f;
            while ((!saved || Game.Instance.SaveManager.CommitInProgress || LoadingProcess.Instance.IsLoadingInProcess) && Time.realtimeSinceStartup < until)
                yield return null;
            var after = new JObject { ["completed"] = saved, ["gameClockUnchanged"] = Game.Instance.TimeController.GameTime == clock,
                ["worm"] = State(w), ["trap"] = State(f), ["callbacks"] = Events(seq, e => true) };
            cases["afterSave"] = after;
            if (!saved) { problems.Add("save: the native save did not complete"); yield break; }
            // The accepted policy: a permitted save keeps the ongoing relationship in the running session.
            ExpectSwallowed(w, 0, "save.after");
            ExpectSwallowed(f, 0, "save.after");
            ExpectHeld(f, 1, "save.after.sibling");
        }

        // ---------------------------------------------------------------- reach (ordinary route)

        /// <summary>
        /// Ordinary route for the G3 save boundary: the Purple Worm's own AI grabs an unmodified native hostile humanoid;
        /// the main character casts native Dominate Person on the held (still targetable) enemy through the ordinary
        /// ability command; combat ends through the engine; the worm's own later maintain swallows the now-allied victim;
        /// native admission is read and a native save requested. Disclosed: worm BAB 100, hostile HP 100000 and brain
        /// off, Dominate Person granted at caster level 20 and DC 100.
        /// </summary>
        private IEnumerable<object> Reach()
        {
            UnitEntityData main = Game.Instance.Player.MainCharacter.Value;
            var steps = new JArray();
            result["sequence"] = steps;
            UnitEntityData worm = Summon("purple-worm", true);
            if (worm == null) { problems.Add("reach: the worm was not summoned"); yield break; }
            // Left where the summon spell placed it, near the caster (as the accepted C2a G1 route).
            worm.Descriptor.Stats.BaseAttackBonus.BaseValue = 100;
            steps.Add("summoned purple worm (brain active: " + worm.IsBrainActive + ")");
            var dominate = ResourcesLibrary.LibraryObject.GetAllBlueprints().OfType<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>().FirstOrDefault(b => b.name == "DominatePerson");
            if (dominate == null) { problems.Add("reach: native DominatePerson not found"); yield break; }
            bool added = !main.Descriptor.HasFact(dominate);
            if (added) main.Descriptor.AddFact(dominate);
            Kingmaker.UnitLogic.Abilities.Ability granted = main.Descriptor.Abilities.GetAbility(dominate);
            object grab = grabType.GetMethod("Find", Any).Invoke(null, new object[] { worm });
            UnitEntityData hostile = null;
            Vector3 at = ObstacleAnalyzer.TraceAlongNavmesh(worm.Position, worm.Position + worm.OrientationDirection * 4f);
            foreach (BlueprintUnit candidate in ResourcesLibrary.LibraryObject.GetAllBlueprints().OfType<BlueprintUnit>()
                .Where(u => u.Faction != null && !u.Faction.Peaceful && u.Faction.AttackFactions != null && u.Faction.AttackFactions.Contains(main.Blueprint.Faction) &&
                    u.Size == Kingmaker.Enums.Size.Medium && !u.name.StartsWith("KMG_", StringComparison.Ordinal))
                .OrderBy(u => u.name.IndexOf("Bandit", StringComparison.OrdinalIgnoreCase) >= 0 ? 0 : 1).ThenBy(u => u.AssetGuid).Take(25))
            {
                UnitEntityData unit = Game.Instance.EntityCreator.SpawnUnit(candidate, at, Quaternion.identity, Game.Instance.State.LoadedAreaState.MainState);
                if (unit == null) continue;
                Game.Instance.EntityCreator.Tick();
                bool ok = unit.IsEnemy(main) && unit.IsEnemy(worm) && new Kingmaker.UnitLogic.Abilities.AbilityData(granted).CanTarget(new Kingmaker.Utility.TargetWrapper(unit)) &&
                    (bool)grabType.GetMethod("IsSwallowSizeAllowed", Any).Invoke(grab, new object[] { worm, unit });
                if (ok) { hostile = unit; break; }
                unit.Destroy();
            }
            if (hostile == null) { problems.Add("reach: no eligible native hostile humanoid"); yield break; }
            created.Add(hostile);
            hostile.Descriptor.Stats.HitPoints.BaseValue = 100000;
            runner.GetMethod("SetExpandedSummoningBrainActive", Any).Invoke(null, new object[] { hostile, false });
            Wake(hostile);
            var r = new Rel { Name = "reach", Holder = worm, Targets = new[] { hostile }, Grab = grab, Multi = false,
                HoldBuff = (BlueprintBuff)grabType.GetField("HoldBuff").GetValue(grab), HeldBuff = (BlueprintBuff)grabType.GetField("GrappledBuff").GetValue(grab),
                SwallowedBuff = (BlueprintBuff)grabType.GetField("SwallowedBuff").GetValue(grab), Weapons = GrabWeapons(worm, grab) };
            result["hostile"] = new JObject { ["blueprint"] = hostile.Blueprint.name, ["faction"] = hostile.Descriptor.Faction.name, ["size"] = hostile.Descriptor.State.Size.ToString() };
            steps.Add("spawned native hostile " + hostile.Blueprint.name + " (unmodified faction " + hostile.Descriptor.Faction.name + ")");
            int seq = LastSeq;
            foreach (object step in SettleGameTime(0.0, 90f, () => IsHeld(r, 0) || IsSwallowed(r, 0))) yield return step;
            result["atHold"] = new JObject { ["state"] = State(r), ["inCombat"] = Game.Instance.Player.IsInCombat, ["saveAllowed"] = Game.Instance.SaveManager.IsSaveAllowed(),
                ["events"] = Events(seq, e => true) };
            if (!IsHeld(r, 0)) { problems.Add("reach: the worm's own AI did not hold the hostile (swallowed early: " + IsSwallowed(r, 0) + ")"); yield break; }
            if (!Game.Instance.Player.IsInCombat || Game.Instance.SaveManager.IsSaveAllowed()) problems.Add("reach: at the hold the party was not in combat with saving refused");
            steps.Add("worm AI bite established the hold; saving refused in combat");
            var attempts = new JArray();
            result["dominate"] = attempts;
            Kingmaker.UnitLogic.Commands.UnitUseAbility command = null;
            for (int attempt = 0; attempt < 3 && hostile.Descriptor.Faction != main.Descriptor.Faction && !IsSwallowed(r, 0); attempt++)
            {
                granted.OverrideParams = new Kingmaker.UnitLogic.Abilities.AbilityParams { CasterLevel = 20, SpellLevel = 4, DC = 100 };
                command = new Kingmaker.UnitLogic.Commands.UnitUseAbility(new Kingmaker.UnitLogic.Abilities.AbilityData(granted), new Kingmaker.Utility.TargetWrapper(hostile));
                command.IgnoreCooldown(TimeSpan.Zero);
                main.Commands.Run(command);
                foreach (object step in SettleGameTime(0.0, 20f, () => command.IsFinished)) yield return step;
                attempts.Add(new JObject { ["attempt"] = attempt, ["result"] = command.Result.ToString(), ["hostileFaction"] = hostile.Descriptor.Faction.name,
                    ["distance"] = Vector3.Distance(main.Position, hostile.Position), ["swallowedAlready"] = IsSwallowed(r, 0) });
            }
            if (added && main.Descriptor.HasFact(dominate)) main.Descriptor.RemoveFact(dominate);
            if (hostile.Descriptor.Faction != main.Descriptor.Faction) { problems.Add("reach: Dominate Person did not take the held enemy before the swallow (" + attempts.ToString(Formatting.None) + ")"); yield break; }
            steps.Add("main character cast native Dominate Person on the held enemy (" + command.Result + ")");
            foreach (object step in SettleGameTime(0.0, 60f, () => IsSwallowed(r, 0) && !Game.Instance.Player.IsInCombat && Game.Instance.SaveManager.IsSaveAllowed())) yield return step;
            Game.Instance.IsPaused = true;
            yield return null;
            bool allowed = Game.Instance.SaveManager.IsSaveAllowed();
            result["beforeSave"] = new JObject { ["state"] = State(r), ["inCombat"] = Game.Instance.Player.IsInCombat, ["saveAllowed"] = allowed,
                ["maintain"] = Events(seq, e => (string)e["what"] == "MaintainLink" || (string)e["what"] == "native.Swallow") };
            if (!IsSwallowed(r, 0)) { problems.Add("reach: the worm's maintain did not swallow the dominated victim within the budget"); yield break; }
            if (!allowed) { problems.Add("reach: native save admission refused with the swallow active"); yield break; }
            steps.Add("combat ended; the worm swallowed the dominated victim; native admission permits saving");
            ExpectSwallowed(r, 0, "reach.beforeSave");
            seq = LastSeq;
            TimeSpan clock = Game.Instance.TimeController.GameTime;
            bool saved = false;
            Game.Instance.SaveGame(Game.Instance.SaveManager.CreateNewSave("ver02c3areach"), () => saved = true);
            float until = Time.realtimeSinceStartup + 120f;
            while ((!saved || Game.Instance.SaveManager.CommitInProgress || LoadingProcess.Instance.IsLoadingInProcess) && Time.realtimeSinceStartup < until)
                yield return null;
            result["afterSave"] = new JObject { ["completed"] = saved, ["gameClockUnchanged"] = Game.Instance.TimeController.GameTime == clock, ["state"] = State(r),
                ["callbacks"] = Events(seq, e => true) };
            if (!saved) { problems.Add("reach: the native save did not complete"); yield break; }
            if (Game.Instance.TimeController.GameTime != clock) problems.Add("reach: the game clock changed during the paused save");
            ExpectSwallowed(r, 0, "reach.afterSave");
            created.Add(worm);
        }

        // ---------------------------------------------------------------- reload

        private IEnumerable<object> Reload()
        {
            var names = new[] { "KMG_C3a_Worm", "KMG_C3a_WormVictim", "KMG_C3a_Trap", "KMG_C3a_TrapVictim", "KMG_C3a_TrapHeld" };
            UnitEntityData[] units = names.Select(n => Game.Instance.State.Units.FirstOrDefault(u => u.Descriptor.CustomName == n)).ToArray();
            result["found"] = new JArray(units.Select(u => (object)(u != null)).ToArray());
            if (units.Any(u => u == null)) { problems.Add("reload: a named unit is missing"); yield break; }
            created.AddRange(units);
            foreach (UnitEntityData u in units) Wake(u);
            var row = new JObject();
            result["reload"] = row;
            row["atLoad"] = Snapshot(units);
            CheckReset(units, "reload.atLoad");
            int[] damage = units.Select(u => u.Descriptor.Damage).ToArray();
            TimeSpan start = Game.Instance.TimeController.GameTime;
            foreach (object step in SettleGameTime(13.0, 40f, () => true)) yield return step;
            row["gameSeconds"] = (Game.Instance.TimeController.GameTime - start).TotalSeconds;
            row["afterRounds"] = Snapshot(units);
            if ((double)row["gameSeconds"] < 12.0) problems.Add("reload: fewer than two rounds of game time passed");
            for (int i = 0; i < units.Length; i++) if (units[i].Descriptor.Damage != damage[i]) problems.Add("reload: " + names[i] + " took damage after the reset");
            CheckReset(units, "reload.afterRounds");
        }

        private static JArray Snapshot(UnitEntityData[] units)
        {
            return new JArray(units.Select(u => (object)new JObject
            {
                ["id"] = u.UniqueId, ["name"] = u.Descriptor.CustomName,
                ["swallowWhole"] = u.Get<UnitPartSwallowWhole>() == null ? null : new JArray(u.Get<UnitPartSwallowWhole>().SwallowedUnits.Select(r => (object)r.Value?.UniqueId).ToArray()),
                ["swallowed"] = u.Get<UnitPartSwallowed>() != null, ["targetPart"] = u.Get<UnitPartGrappleTarget>() != null, ["initiatorPart"] = u.Get<UnitPartGrappleInitiator>() != null,
                ["untargetable"] = (bool)u.Descriptor.State.IsUntargetable,
                ["cantMove"] = u.Descriptor.State.HasCondition(UnitCondition.CantMove), ["cantAct"] = u.Descriptor.State.HasCondition(UnitCondition.CantAct),
                ["buffs"] = new JArray(u.Descriptor.Buffs.RawFacts.OfType<Buff>().Select(b => (object)b.Blueprint.name).ToArray()),
                ["damage"] = u.Descriptor.Damage
            }).ToArray());
        }

        private void CheckReset(UnitEntityData[] units, string name)
        {
            foreach (UnitEntityData u in units)
            {
                bool residue = (u.Get<UnitPartSwallowWhole>() != null && u.Get<UnitPartSwallowWhole>().SwallowedUnits.Count != 0) || u.Get<UnitPartSwallowed>() != null ||
                    u.Get<UnitPartGrappleTarget>() != null || u.Get<UnitPartGrappleInitiator>() != null || (bool)u.Descriptor.State.IsUntargetable ||
                    u.Descriptor.State.HasCondition(UnitCondition.CantMove) || u.Descriptor.State.HasCondition(UnitCondition.CantAct) ||
                    u.Descriptor.Buffs.RawFacts.OfType<Buff>().Any(b => b.Blueprint.name.Contains("Swallowed") || b.Blueprint.name.Contains("Engulfed") ||
                        b.Blueprint.name.EndsWith("Grapple_Hold", StringComparison.Ordinal) || b.Blueprint.name.EndsWith("Grapple_MultiHold", StringComparison.Ordinal) ||
                        b.Blueprint.name.EndsWith("Grapple_Grappled", StringComparison.Ordinal) || b.Blueprint.name.EndsWith("Grapple_MultiHeld", StringComparison.Ordinal));
                IList<string> raw = u.Blueprint.name.StartsWith("KMG_Summoning_Unit_", StringComparison.Ordinal) ? RawLinks(u) : null;
                if (residue || (raw != null && raw.Count != 0)) problems.Add(name + ": " + u.Descriptor.CustomName + " is not reset");
            }
        }
    }
}
