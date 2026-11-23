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
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.View;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KmgLoaderCompatHarness
{
    /// <summary>
    /// Sprint 6 VER-02 G1/G2 checkpoint: the Barbed Devil's feature-carried grab with its Impale rider and deferred
    /// exact-owner cleanup, and the Giant Crab's two-pincer multi-hold with per-link constrict, on the accepted C3a
    /// product (unchanged).
    ///
    /// Every hold is made by a real attack (the engine's RuleAttackWithWeapon with the creature's exact grab limb), which
    /// reaches the product's SummonGrabComponent and its native grapple maneuver. Maintain checks, Impale, constrict,
    /// release and the deferred cleanup run only from the game's own round ticks and before-tick scheduler. Nothing calls
    /// ReleaseLink, removes a native part, invokes the scheduled callback or forces a die.
    ///
    /// Disclosed setup: holder BAB 100 (hits, grabs and maintains succeed except on a natural 1; holder CMD high); target
    /// HP 100000 and escape stats (CMB, Mobility, Athletics) -100; summon lifetimes extended to 600 s (native deadline),
    /// except where a native expiry is itself the terminal (deadline moved to now); appear buffs removed; brains off; units
    /// woken; navmesh placement; pauses for observation. For the Barbed failed maintain only: the target's additional CMD
    /// 1000 (the Sprint 35 pattern; the natural d20 is kept, so a natural 20 still succeeds), restored immediately after.
    /// For Dismissal only: the main character is granted the native Dismissal fact if absent, its cast parameters are
    /// overridden to CL 40 / spell level 4 (spell resistance is still rolled), and the Barbed Devil's Will base is -100
    /// (the natural d20 is kept). Unrelated control effect: native Bull's Strength.
    ///
    /// Modes: "barbed", "crab", "save" (one native save with an active Barbed hold and a two-link Crab), "reload" (the
    /// fresh process loading that save).
    /// </summary>
    internal sealed partial class BarbedCrabProbe : IOwnedProbe
    {
        private const BindingFlags Any = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const string UnrelatedBuffGuid = "b175001b42b1a02479881b72fe132116";
        private const string NativeDismissalGuid = "95f7cdcec94e293489a85afdf5af1fd7";
        internal static readonly string[] SaveNames = { "KMG_G12_Barbed", "KMG_G12_BarbedTarget", "KMG_G12_Crab", "KMG_G12_CrabTargetA", "KMG_G12_CrabTargetB" };

        private readonly string mode;
        private readonly JObject result = new JObject();
        private readonly List<string> problems = new List<string>();
        private readonly IEnumerator<object> script;
        private readonly List<UnitEntityData> created = new List<UnitEntityData>();
        private readonly List<UnitEntityData> awakeAdded = new List<UnitEntityData>();
        private readonly RuleTrace rules = new RuleTrace();
        private string verdict;
        private bool subscribed;
        private Assembly product;
        private Type runner, grabType, holdType, linksType, linkPart, limbs, profiles;
        private Vector3 origin, forward, right;

        public BarbedCrabProbe(string mode) { this.mode = mode; result["mode"] = mode; script = Run().GetEnumerator(); }
        public object Result => result;

        public string Poll()
        {
            if (verdict != null) return verdict;
            bool more;
            try { more = script.MoveNext(); }
            catch (Exception exception) { problems.Add("probe exception: " + exception); more = false; }
            if (more) return null;
            Unsubscribe();
            result["problems"] = new JArray(problems.Cast<object>().ToArray());
            verdict = problems.Count == 0 ? string.Empty : string.Join(" | ", problems.ToArray());
            return verdict;
        }

        public CleanupReport Release(string testFailure)
        {
            return ProbeCleanup.RunAll(new[]
            {
                new KeyValuePair<string, Action>("rule trace", Unsubscribe),
                new KeyValuePair<string, Action>("main character brain", RestoreMainBrain),
                new KeyValuePair<string, Action>("awake registrations", () => { foreach (UnitEntityData u in awakeAdded) Game.Instance.State.AwakeUnits.Remove(u); awakeAdded.Clear(); }),
                new KeyValuePair<string, Action>("disposable units", () => { foreach (UnitEntityData u in created.Where(u => u != null && !u.Destroyed).ToArray()) u.Destroy(); created.Clear(); })
            });
        }

        private void Unsubscribe() { if (subscribed) { EventBus.Unsubscribe(rules); subscribed = false; } }

        private IEnumerable<object> Run()
        {
            product = ProductProbes.Entry().Assembly;
            runner = product.GetType("KingmakerGunslinger.RuntimeTesting.RuntimeTestRunner", true);
            grabType = product.GetType("KingmakerGunslinger.Summoning.SummonGrabComponent", true);
            holdType = product.GetType("KingmakerGunslinger.Summoning.SummonHoldComponent", true);
            linksType = product.GetType("KingmakerGunslinger.Summoning.SummonGrappleLinks", true);
            linkPart = product.GetType("KingmakerGunslinger.Summoning.UnitPartSummonGrappleLinks", true);
            limbs = product.GetType("KingmakerGunslinger.Summoning.SummonLimbs", true);
            profiles = product.GetType("KingmakerGunslinger.Summoning.ExpandedSummoningSpecialProfiles", true);
            RuleTrace.Succeeded = product.GetType("KingmakerGunslinger.Summoning.SummonManeuverChecks", true).GetMethod("Succeeded", Any);
            if (!HoldLifecycleObserver.Installed) { problems.Add("the hold lifecycle observer is not installed (launch condition observeHoldLifecycle)"); yield break; }
            EventBus.Subscribe(rules);
            subscribed = true;
            result["area"] = Game.Instance.CurrentlyLoadedArea == null ? null : Game.Instance.CurrentlyLoadedArea.name;
            result["turnBased"] = TurnBased.Controllers.CombatController.IsInTurnBasedCombat();
            UnitEntityData main = Game.Instance.Player.MainCharacter.Value;
            origin = main.Position;
            forward = Quaternion.Euler(0, main.Orientation, 0) * Vector3.forward;
            right = Quaternion.Euler(0, main.Orientation + 90f, 0) * Vector3.forward;
            bool paused = Game.Instance.IsPaused;
            try
            {
                IEnumerable<object> body = mode == "barbed" ? Barbed() : mode == "crab" ? Crab() : mode == "save" ? Save() : mode == "reload" ? Reload() :
                    mode.StartsWith("lethal", StringComparison.Ordinal) ? Lethal(mode) : null;
                if (body == null) { problems.Add("unknown mode " + mode); yield break; }
                foreach (object step in body) yield return step;
            }
            finally { Game.Instance.IsPaused = paused; }
        }

        // ================================================================ rule trace (read-only)

        /// <summary>Grapple maneuvers, damage, saves and spell resistance touching tracked units, in the observer's sequence.</summary>
        private sealed class RuleTrace : IGlobalRulebookHandler<RuleCombatManeuver>, IGlobalRulebookHandler<RuleDealDamage>,
            IGlobalRulebookHandler<RuleSavingThrow>, IGlobalRulebookHandler<RuleSpellResistanceCheck>, IGlobalRulebookHandler<RuleSkillCheck>,
            IGlobalRulebookHandler<RuleHealDamage>
        {
            public void OnEventAboutToTrigger(RuleHealDamage evt) { }
            public void OnEventDidTrigger(RuleHealDamage evt)
            {
                if (evt == null || !Tracked.Contains(evt.Target)) return;
                HoldLifecycleObserver.Add("rule.heal", new JObject { ["initiator"] = evt.Initiator?.UniqueId, ["target"] = evt.Target?.UniqueId, ["value"] = evt.Value,
                    ["targetDamageAfter"] = evt.Target.Descriptor.Damage });
            }
            public void OnEventAboutToTrigger(RuleSkillCheck evt) { }
            public void OnEventDidTrigger(RuleSkillCheck evt)
            {
                if (evt == null || !Tracked.Contains(evt.Initiator)) return;
                HoldLifecycleObserver.Add("rule.skill", new JObject { ["unit"] = evt.Initiator?.UniqueId, ["stat"] = evt.StatType.ToString(), ["dc"] = evt.DC, ["passed"] = evt.IsPassed });
            }
            internal static MethodInfo Succeeded;
            internal readonly HashSet<UnitEntityData> Tracked = new HashSet<UnitEntityData>();
            public void OnEventAboutToTrigger(RuleCombatManeuver evt) { }
            public void OnEventDidTrigger(RuleCombatManeuver evt)
            {
                if (evt == null || (!Tracked.Contains(evt.Initiator) && !Tracked.Contains(evt.Target))) return;
                HoldLifecycleObserver.Add("rule.maneuver", new JObject
                {
                    ["type"] = evt.Type.ToString(), ["initiator"] = evt.Initiator?.UniqueId, ["target"] = evt.Target?.UniqueId,
                    ["roll"] = evt.InitiatorRoll == null ? (int?)null : (int)evt.InitiatorRoll, ["cmb"] = evt.InitiatorCMB, ["cmd"] = evt.TargetCMD,
                    ["autoFailure"] = evt.AutoFailure, ["nativeSuccess"] = evt.Success,
                    ["productSuccess"] = Succeeded != null && (bool)Succeeded.Invoke(null, new object[] { evt })
                });
            }
            public void OnEventAboutToTrigger(RuleDealDamage evt) { }
            public void OnEventDidTrigger(RuleDealDamage evt)
            {
                if (evt == null || !Tracked.Contains(evt.Target)) return;
                HoldLifecycleObserver.Add("rule.damage", new JObject
                {
                    ["initiator"] = evt.Initiator?.UniqueId, ["target"] = evt.Target?.UniqueId, ["damage"] = evt.Damage,
                    ["weapon"] = evt.DamageBundle?.Weapon?.Blueprint?.name, ["attackRoll"] = evt.AttackRoll != null,
                    ["targetDamageAfter"] = evt.Target.Descriptor.Damage, ["targetHP"] = (int)evt.Target.Descriptor.Stats.HitPoints,
                    ["targetLifeState"] = evt.Target.Descriptor.State.LifeState.ToString()
                });
            }
            public void OnEventAboutToTrigger(RuleSavingThrow evt) { }
            public void OnEventDidTrigger(RuleSavingThrow evt)
            {
                if (evt == null || !Tracked.Contains(evt.Initiator)) return;
                HoldLifecycleObserver.Add("rule.save", new JObject
                {
                    ["unit"] = evt.Initiator?.UniqueId, ["type"] = evt.Type.ToString(), ["dc"] = evt.DifficultyClass,
                    ["natural"] = evt.D20 == null ? (int?)null : evt.D20.Value, ["total"] = evt.RollResult, ["passed"] = evt.IsPassed
                });
            }
            public void OnEventAboutToTrigger(RuleSpellResistanceCheck evt) { }
            public void OnEventDidTrigger(RuleSpellResistanceCheck evt)
            {
                if (evt == null || !Tracked.Contains(evt.Target)) return;
                HoldLifecycleObserver.Add("rule.spellResistance", new JObject
                {
                    ["target"] = evt.Target?.UniqueId, ["caster"] = evt.Context?.MaybeCaster?.UniqueId, ["ability"] = evt.Ability?.name,
                    ["cl"] = evt.Context?.Params?.CasterLevel, ["resistance"] = evt.SpellResistance, ["penetration"] = evt.SpellPenetration,
                    ["immune"] = evt.TargetIsImmune, ["resisted"] = evt.IsSpellResisted,
                    ["natural"] = evt.HasResistanceRoll ? (int?)evt.Roll.Value : null
                });
            }
        }

        private static int LastSeq => HoldLifecycleObserver.Events.Count == 0 ? 0 : (int)HoldLifecycleObserver.Events.Last()["seq"];
        private static JArray Events(int seq, Func<JObject, bool> filter) =>
            new JArray(HoldLifecycleObserver.Events.Where(e => (int)e["seq"] > seq && filter(e)).Cast<object>().ToArray());
        private static bool Is(JObject e, string what) => (string)e["what"] == what;
        private static bool PairOf(JObject e, UnitEntityData h, UnitEntityData t) => (string)e["owner"] == h.UniqueId && (string)e["target"] == t.UniqueId;
        private static JArray PairEvents(int seq, UnitEntityData h, UnitEntityData t) => Events(seq, e =>
            (PairOf(e, h, t) && (Is(e, "MaintainLink") || Is(e, "DealConstrict") || ((string)e["what"]).StartsWith("SummonHoldComponent.", StringComparison.Ordinal) ||
                ((string)e["what"]).StartsWith("impaleCleanup.", StringComparison.Ordinal))) ||
            (Is(e, "rule.maneuver") && ((string)e["initiator"] == h.UniqueId && (string)e["target"] == t.UniqueId || (string)e["initiator"] == t.UniqueId && (string)e["target"] == h.UniqueId)) ||
            (Is(e, "rule.damage") && (string)e["initiator"] == h.UniqueId && (string)e["target"] == t.UniqueId));
        private static double Now => Game.Instance.TimeController.GameTime.TotalSeconds;

        /// <summary>A maintain outcome that resolved the round (not a waiting or already-resolved note).</summary>
        private static bool Decisive(JObject e)
        {
            string r = (string)e["result"] ?? "";
            return Is(e, "MaintainLink") && !r.Contains("waiting-for-later-round") && !r.Contains("already-resolved");
        }

        private static JObject[] DecisiveSince(int seq, UnitEntityData h, UnitEntityData t) =>
            HoldLifecycleObserver.Events.Where(e => (int)e["seq"] > seq && PairOf(e, h, t) && Decisive(e)).ToArray();

        /// <summary>
        /// Classifies a pair's window. Ordinary only when every release in it is an exact "released" maintain result for this
        /// holder/target with a failed grapple maneuver by this holder on this target in the same window; any invalid-state
        /// release, unattributed release or mixed window is not ordinary.
        /// </summary>
        private static string Classify(int seq, UnitEntityData h, UnitEntityData t)
        {
            JObject[] decisive = DecisiveSince(seq, h, t);
            bool invalid = decisive.Any(e => ((string)e["result"]).StartsWith("released:", StringComparison.Ordinal) || ((string)e["result"]).StartsWith("refused", StringComparison.Ordinal));
            JObject[] ordinary = decisive.Where(e => (string)e["result"] == "released").ToArray();
            int releases = HoldLifecycleObserver.Events.Count(e => (int)e["seq"] > seq && PairOf(e, h, t) && Is(e, "SummonHoldComponent.ReleaseLink"));
            if (invalid) return "invalid-state";
            if (ordinary.Length == 0) return releases == 0 ? "held" : "unattributed-release";
            bool failedRoll = ordinary.All(o => HoldLifecycleObserver.Events.Any(m => Is(m, "rule.maneuver") && (int)m["seq"] > seq && (int)m["seq"] < (int)o["seq"] &&
                (string)m["type"] == "Grapple" && (string)m["initiator"] == h.UniqueId && (string)m["target"] == t.UniqueId && !(bool)m["productSuccess"]));
            if (!failedRoll) return "unattributed-release";
            return releases == ordinary.Length ? "ordinary-failed-maintain" : "mixed";
        }

        /// <summary>
        /// A release with no maintain result, attributed by the round callback it ran inside: the target's own
        /// SummonHeldComponent round with a passed break-free check or maneuver is a native break-free (ordinary); the
        /// holder's multi-hold round without a maintain is its unconscious/out-of-reach branch (not ordinary here).
        /// </summary>
        private static string AttributeRoundRelease(int seq, UnitEntityData h, UnitEntityData t)
        {
            JObject release = HoldLifecycleObserver.Events.LastOrDefault(e => (int)e["seq"] > seq && PairOf(e, h, t) && Is(e, "SummonHoldComponent.ReleaseLink"));
            if (release == null) return "unattributed-release";
            int r = (int)release["seq"];
            JObject heldBegin = HoldLifecycleObserver.Events.LastOrDefault(e => (int)e["seq"] < r && (int)e["seq"] > seq && Is(e, "held.OnNewRound.begin") && (string)e["unit"] == t.UniqueId);
            JObject heldEnd = heldBegin == null ? null : HoldLifecycleObserver.Events.FirstOrDefault(e => (int)e["seq"] > (int)heldBegin["seq"] && Is(e, "held.OnNewRound.end") && (string)e["unit"] == t.UniqueId);
            if (heldBegin != null && (heldEnd == null || (int)heldEnd["seq"] > r))
            {
                bool passed = HoldLifecycleObserver.Events.Any(e => (int)e["seq"] > (int)heldBegin["seq"] && (int)e["seq"] < r &&
                    ((Is(e, "rule.skill") && (string)e["unit"] == t.UniqueId && (bool)e["passed"]) ||
                     (Is(e, "rule.maneuver") && (string)e["initiator"] == t.UniqueId && (string)e["target"] == h.UniqueId && (bool)e["nativeSuccess"])));
                return passed ? "native-break-free" : "held-round-release-without-passed-check";
            }
            JObject multiBegin = HoldLifecycleObserver.Events.LastOrDefault(e => (int)e["seq"] < r && (int)e["seq"] > seq && Is(e, "multiHold.OnNewRound.begin") && (string)e["owner"] == h.UniqueId);
            if (multiBegin != null) return "multihold-round-release:conscious=" + release["targetConscious"] + ";reach=" + release["reachPrimary"] + ";distance=" + release["distance"];
            return "unattributed-release";
        }

        // ================================================================ fixture

        private readonly List<Vector3> spots = new List<Vector3>();

        /// <summary>
        /// A navmesh spot within 10.5 m of the main character (the native summon-follow controller pulls an out-of-combat
        /// summon back beyond 11.7 m, which walked an unconstrained multi-link holder away from its target in the first
        /// rerun) and at least 5.5 m from every spot in use. Candidates lie on rings of 5 m and 9.5 m around the summoner.
        /// </summary>
        private Vector3 NextSpot()
        {
            for (int k = 0; k < 15; k++)
            {
                float radius = k < 5 ? 5f : 9.5f;
                float angle = k < 5 ? k * 72f : (k - 5) * 36f + 18f;
                Vector3 candidate = ObstacleAnalyzer.TraceAlongNavmesh(origin, origin + Quaternion.Euler(0, angle, 0) * forward * radius);
                if (Vector3.Distance(origin, candidate) < 3f || Vector3.Distance(origin, candidate) > 10.5f) continue;
                if (spots.Any(s => Vector3.Distance(s, candidate) < 5.5f)) continue;
                spots.Add(candidate);
                attemptSpots?.Add(candidate);
                ((JArray)(result["spots"] ?? (result["spots"] = new JArray()))).Add(candidate.ToString());
                return candidate;
            }
            problems.Add("fixture: no free navmesh spot within 10.5 m of the main character");
            return origin + forward * 5f;
        }

        /// <summary>Disposes a control whose witness role is over (not a terminal under test) and frees its spot.</summary>
        private void Retire(Control[] controls)
        {
            foreach (Control c in controls)
            {
                spots.RemoveAll(s => Vector3.Distance(s, c.Spot) < 0.5f);
                foreach (UnitEntityData u in new[] { c.H, c.T }) if (u != null && !u.Destroyed) u.Destroy();
            }
        }

        private static Vector3 Around(Vector3 center, float angle, float distance) =>
            ObstacleAnalyzer.TraceAlongNavmesh(center, center + Quaternion.Euler(0, angle, 0) * Vector3.forward * distance);

        private void Wake(UnitEntityData unit)
        {
            unit.Wake(900f);
            if (!Game.Instance.State.AwakeUnits.Contains(unit)) { Game.Instance.State.AwakeUnits.Add(unit); awakeAdded.Add(unit); }
        }

        private UnitEntityData Summon(string key, Vector3 at)
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
            attemptUnits?.Add(unit);
            rules.Tracked.Add(unit);
            runner.GetMethod("RemoveExpandedSummoningAppearanceBuffs", Any).Invoke(null, new object[] { unit });
            runner.GetMethod("SetExpandedSummoningBrainActive", Any).Invoke(null, new object[] { unit, false });
            Wake(unit);
            runner.GetMethod("PlaceExpandedSummoningUnit", Any).Invoke(null, new object[] { unit, at });
            unit.Descriptor.Stats.BaseAttackBonus.BaseValue = 100;
            SetDeadline(unit, TimeSpan.FromSeconds(600));
            return unit;
        }

        /// <summary>G1-R substitutes a native hostile target; every other mode uses SpawnTarget.</summary>
        private Func<Vector3, UnitEntityData> targetFactory;

        private UnitEntityData SpawnTarget(Vector3 at)
        {
            UnitEntityData unit = Game.Instance.EntityCreator.SpawnUnit(BlueprintRoot.Instance.DefaultPlayerCharacter, at, Quaternion.identity, Game.Instance.State.LoadedAreaState.MainState);
            if (unit == null) return null;
            created.Add(unit);
            rules.Tracked.Add(unit);
            Game.Instance.EntityCreator.Tick();
            var stats = unit.Descriptor.Stats;
            stats.HitPoints.BaseValue = 100000;
            stats.AdditionalCMB.BaseValue = -100;
            stats.SkillMobility.BaseValue = -100;
            stats.SkillAthletics.BaseValue = -100;
            runner.GetMethod("SetExpandedSummoningBrainActive", Any).Invoke(null, new object[] { unit, false });
            Wake(unit);
            return unit;
        }

        private Buff AddUnrelated(UnitEntityData unit)
        {
            var blueprint = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>(UnrelatedBuffGuid);
            if (blueprint == null) { problems.Add("the unrelated control buff blueprint is missing"); return null; }
            return unit.Descriptor.Buffs.AddBuff(blueprint, unit, TimeSpan.FromMinutes(30));
        }

        private static bool Kept(UnitEntityData unit, Buff buff) => buff != null && unit != null && !unit.Destroyed && unit.Descriptor.Buffs.RawFacts.Contains(buff);

        private static Buff Marker(UnitEntityData unit) => unit == null || unit.Destroyed ? null :
            unit.Descriptor.Buffs.Enumerable.FirstOrDefault(b => ReferenceEquals(b.Blueprint, BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff));

        /// <summary>Moves the summon's native SummonedUnitBuff deadline (disclosed) and lets the buff collection reschedule and tick.</summary>
        private static JObject SetDeadline(UnitEntityData unit, TimeSpan fromNow)
        {
            Buff marker = Marker(unit);
            if (marker == null) return new JObject { ["marker"] = false };
            TimeSpan now = Game.Instance.TimeController.GameTime;
            double previous = (marker.EndTime - now).TotalSeconds;
            typeof(Buff).GetProperty("EndTime", Any).SetValue(marker, now + fromNow, null);
            unit.Descriptor.Buffs.UpdateNextEvent();
            unit.Descriptor.Buffs.Tick();
            return new JObject { ["marker"] = true, ["previousRemainingSeconds"] = previous, ["newRemainingSeconds"] = fromNow.TotalSeconds,
                ["markerAfterNativeTick"] = unit.Descriptor.Buffs.RawFacts.Contains(marker) };
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

        private object Grab(UnitEntityData holder) => grabType.GetMethod("Find", Any).Invoke(null, new object[] { holder });
        private BlueprintBuff HoldBuffOf(object grab) => grab == null ? null : (BlueprintBuff)grabType.GetField("HoldBuff").GetValue(grab);
        private BlueprintBuff HeldBuffOf(object grab) => grab == null ? null : (BlueprintBuff)grabType.GetField("GrappledBuff").GetValue(grab);

        private ItemEntityWeapon[] GrabWeapons(UnitEntityData holder, object grab)
        {
            var list = new List<ItemEntityWeapon>();
            if (grab == null) return list.ToArray();
            MethodInfo isGrabLimb = grabType.GetMethod("IsGrabLimb", Any);
            var primary = (ItemEntityWeapon)limbs.GetMethod("PrimaryWeapon", Any).Invoke(null, new object[] { holder });
            if (primary != null && (bool)isGrabLimb.Invoke(grab, new object[] { holder, primary })) list.Add(primary);
            if (holder.Body.AdditionalLimbs != null)
                foreach (var slot in holder.Body.AdditionalLimbs)
                    if (slot != null && slot.MaybeWeapon != null && (bool)isGrabLimb.Invoke(grab, new object[] { holder, slot.MaybeWeapon }))
                        list.Add(slot.MaybeWeapon);
            return list.ToArray();
        }

        private IList<string> RawLinks(UnitEntityData holder)
        {
            if (holder == null || holder.Descriptor == null) return null;
            object part = typeof(UnitDescriptor).GetMethods().Single(m => m.Name == "Get" && m.IsGenericMethodDefinition && m.GetParameters().Length == 0)
                .MakeGenericMethod(linkPart).Invoke(holder.Descriptor, null);
            return part == null ? null : (IList<string>)linkPart.GetField("_links", Any).GetValue(part);
        }

        private string[] Reconciled(UnitEntityData holder) =>
            ((IEnumerable)linksType.GetMethod("Reconcile", Any).Invoke(null, new object[] { holder })).Cast<object>()
                .Select(r => r.GetType().GetProperty("TargetId").GetValue(r, null) + "|" + r.GetType().GetProperty("Limb").GetValue(r, null) + "|" +
                    r.GetType().GetProperty("AdditionalIndex").GetValue(r, null)).ToArray();

        private string Occupant(UnitEntityData holder, ItemEntityWeapon weapon) =>
            weapon == null ? null : (linksType.GetMethod("OccupantOf", Any).Invoke(null, new object[] { holder, weapon }) as UnitEntityData)?.UniqueId;

        private string EstablishingLimb(UnitEntityData holder, UnitEntityData target) =>
            HoldLifecycleObserver.LimbOf(holder, linksType.GetMethod("EstablishingWeapon", Any).Invoke(null, new object[] { holder, target }) as ItemEntityWeapon);

        private static JArray Buffs(UnitEntityData unit) => unit == null || unit.Destroyed ? new JArray() :
            new JArray(unit.Descriptor.Buffs.RawFacts.OfType<Buff>().Select(x => (object)x.Blueprint.name).ToArray());

        private static string[] FearOn(UnitEntityData unit) => unit == null || unit.Destroyed ? new string[0] :
            unit.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(b => b.Blueprint.name.IndexOf("Fear", StringComparison.OrdinalIgnoreCase) >= 0 ||
                b.Blueprint.name.IndexOf("Frightened", StringComparison.OrdinalIgnoreCase) >= 0).Select(b => b.Blueprint.name).ToArray();

        private static readonly PropertyInfo NextTick = typeof(Buff).GetProperty("NextTickTime", Any);
        private static JObject BuffClock(Buff buff) => buff == null ? null : new JObject
        {
            ["instance"] = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(buff), ["round"] = buff.RoundNumber,
            ["nextTickTicks"] = NextTick == null ? (long?)null : ((TimeSpan)NextTick.GetValue(buff, null)).Ticks
        };

        // ---------------------------------------------------------------- single-link (Barbed) observation

        private sealed class Link1 { internal UnitEntityData H, T; internal object Grab; internal BlueprintBuff Hold, Held; internal ItemEntityWeapon[] Weapons; internal Buff Unrelated; internal int Seq0; internal string Name; internal Vector3 Spot; }

        private static bool HeldSingle(Link1 s) => HeldSingle(s.H, s.T);
        private static bool HeldSingle(UnitEntityData h, UnitEntityData t)
        {
            if (h == null || t == null || h.Destroyed || t.Destroyed) return false;
            UnitPartGrappleInitiator i = h.Get<UnitPartGrappleInitiator>();
            UnitPartGrappleTarget p = t.Get<UnitPartGrappleTarget>();
            return i != null && p != null && ReferenceEquals(i.Target.Value, t) && ReferenceEquals(p.Initiator.Value, h);
        }

        private Buff HoldInstance(Link1 s) => s.H.Destroyed ? null : s.H.Descriptor.Buffs.RawFacts.OfType<Buff>().FirstOrDefault(b => ReferenceEquals(b.Blueprint, s.Hold));

        private JObject SingleState(Link1 s) => SingleState(s, s.T);
        private JObject SingleState(Link1 s, UnitEntityData t)
        {
            UnitEntityData h = s.H;
            UnitPartGrappleInitiator i = h.Destroyed ? null : h.Get<UnitPartGrappleInitiator>();
            UnitPartGrappleTarget p = t.Destroyed ? null : t.Get<UnitPartGrappleTarget>();
            IList<string> raw = h.Destroyed ? null : RawLinks(h);
            Buff hold = HoldInstance(s);
            object logic = hold == null ? null : hold.Components.FirstOrDefault(c => c.GetType() == holdType);
            UnitState ts = t.Descriptor.State;
            return new JObject
            {
                ["holder"] = h.UniqueId, ["target"] = t.UniqueId, ["holderDead"] = h.Descriptor.State.IsDead, ["holderDestroyed"] = h.Destroyed,
                ["targetDead"] = ts.IsDead, ["targetDestroyed"] = t.Destroyed,
                ["heldTarget"] = h.Destroyed ? null : (holdType.GetMethod("HeldTarget", Any).Invoke(null, new object[] { h }) as UnitEntityData)?.UniqueId,
                ["initiatorPart"] = i != null, ["initiatorTarget"] = i?.Target.Value?.UniqueId,
                ["targetPart"] = p != null, ["targetPartInitiator"] = p?.Initiator.Value?.UniqueId,
                ["holdBuffs"] = h.Destroyed ? 0 : h.Descriptor.Buffs.RawFacts.OfType<Buff>().Count(b => ReferenceEquals(b.Blueprint, s.Hold)),
                ["holdClock"] = BuffClock(hold),
                ["lastRiderRound"] = logic == null ? (int?)null : (int)holdType.GetField("m_LastRiderRound", Any).GetValue(logic),
                ["heldBuffs"] = t.Descriptor.Buffs.RawFacts.OfType<Buff>().Count(b => ReferenceEquals(b.Blueprint, s.Held)),
                ["heldClock"] = BuffClock(t.Descriptor.Buffs.RawFacts.OfType<Buff>().FirstOrDefault(b => ReferenceEquals(b.Blueprint, s.Held))),
                ["establishingLimb"] = h.Destroyed ? null : EstablishingLimb(h, t),
                ["raw.links"] = raw == null ? null : new JArray(raw.Cast<object>().ToArray()),
                ["rawNamingTarget"] = raw == null ? 0 : raw.Count(l => l != null && l.StartsWith(t.UniqueId + "|", StringComparison.Ordinal)),
                ["live.reconciled"] = h.Destroyed ? new JArray() : new JArray(Reconciled(h).Cast<object>().ToArray()),
                ["live.heldCount"] = h.Destroyed || s.Grab == null ? 0 : (int)grabType.GetMethod("HeldCount", Any).Invoke(s.Grab, new object[] { h }),
                ["live.occupancy"] = new JArray(s.Weapons.Select(w => (object)new JObject { ["limb"] = HoldLifecycleObserver.LimbOf(h, w), ["occupant"] = h.Destroyed ? null : Occupant(h, w) }).ToArray()),
                ["holderCantAct"] = !h.Destroyed && h.Descriptor.State.HasCondition(UnitCondition.CantAct),
                ["holderCantMove"] = !h.Destroyed && h.Descriptor.State.HasCondition(UnitCondition.CantMove),
                ["targetCantMove"] = ts.HasCondition(UnitCondition.CantMove), ["targetCantAct"] = ts.HasCondition(UnitCondition.CantAct),
                ["targetEntangled"] = ts.HasCondition(UnitCondition.Entangled), ["targetCanMove"] = ts.CanMove, ["targetCanAct"] = ts.CanAct,
                ["targetFear"] = new JArray(FearOn(t).Cast<object>().ToArray()), ["targetDamage"] = t.Descriptor.Damage,
                ["targetBuffs"] = Buffs(t), ["holderBuffs"] = Buffs(h)
            };
        }

        /// <summary>The asserted Barbed establishment contract.</summary>
        private void ExpectSingleHeld(Link1 s, UnitEntityData t, string name)
        {
            JObject st = SingleState(s, t);
            if (!HeldSingle(s.H, t)) problems.Add(name + ": no reciprocal native hold: " + st.ToString(Formatting.None));
            if ((string)st["heldTarget"] != t.UniqueId) problems.Add(name + ": the hold does not name the target");
            if ((int)st["holdBuffs"] != 1) problems.Add(name + ": the holder does not carry exactly one hold buff");
            if ((int)st["heldBuffs"] != 1) problems.Add(name + ": the target does not carry exactly one grappled buff");
            string primary = HoldLifecycleObserver.LimbOf(s.H, s.Weapons[0]);
            if ((string)st["establishingLimb"] != primary) problems.Add(name + ": the establishing limb is " + st["establishingLimb"] + ", expected the primary claw " + primary);
            if ((int)st["rawNamingTarget"] != 1 || ((JArray)st["live.reconciled"]).Count != 1) problems.Add(name + ": the raw/live link records are not exactly one: " + st["raw.links"]);
            if ((string)((JArray)st["live.occupancy"])[0]["occupant"] != t.UniqueId) problems.Add(name + ": the primary claw is not occupied by the target");
        }

        /// <summary>The asserted end of a Barbed relationship: target-owned state, and the surviving holder's own state.</summary>
        private void ExpectSingleEnded(Link1 s, UnitEntityData t, string name, bool holderSurvives)
        {
            JObject st = SingleState(s, t);
            if ((bool)st["targetPart"] || (int)st["heldBuffs"] != 0) problems.Add(name + ": the target keeps grappled state: " + st.ToString(Formatting.None));
            if (holderSurvives)
            {
                if ((bool)st["initiatorPart"] || (int)st["holdBuffs"] != 0) problems.Add(name + ": the holder keeps its initiator part or hold buff");
                if ((int)st["rawNamingTarget"] != 0 || ((JArray)st["live.reconciled"]).Count != 0 || (int)st["live.heldCount"] != 0) problems.Add(name + ": link records remain: " + st["raw.links"]);
                if (((JArray)st["live.occupancy"]).Any(o => (string)o["occupant"] != null)) problems.Add(name + ": a claw still reports an occupant");
                if ((bool)st["holderCantAct"] || (bool)st["holderCantMove"]) problems.Add(name + ": the surviving holder keeps CantAct/CantMove");
            }
            // A Barbed claw's Frightened is a separate effect: movement/action are asserted only without it.
            if (!(bool)st["targetDead"] && ((JArray)st["targetFear"]).Count == 0 && (!(bool)st["targetCanMove"] || !(bool)st["targetCanAct"]))
                problems.Add(name + ": the surviving target cannot move or act without any fear effect: " + st.ToString(Formatting.None));
        }

        private IEnumerable<object> AttackUntil(string name, UnitEntityData h, UnitEntityData t, ItemEntityWeapon w, Func<bool> held)
        {
            int seq = LastSeq, attacks = 0;
            for (; attacks < 6 && !held(); attacks++)
            {
                Rulebook.Trigger(new RuleAttackWithWeapon(h, t, w, 0));
                yield return null;
            }
            result["grab." + name] = new JObject { ["weapon"] = w?.Blueprint?.name, ["limb"] = HoldLifecycleObserver.LimbOf(h, w), ["attacks"] = attacks, ["held"] = held(),
                ["events"] = PairEvents(seq, h, t) };
            if (!held()) problems.Add(name + ": the real attacks did not establish a hold within six attacks");
        }

        private IEnumerable<object> EstablishBarbed(string name, Action<Link1> done) => EstablishSingle("barbed-devil", name, done);

        private IEnumerable<object> EstablishSingle(string key, string name, Action<Link1> done)
        {
            Vector3 spot = NextSpot();
            UnitEntityData t = (targetFactory ?? SpawnTarget)(spot);
            UnitEntityData h = t == null ? null : Summon(key, Around(spot, 90f, 1.0f));
            if (h == null) { problems.Add(name + ": units not created"); yield break; }
            yield return null;
            object grab = Grab(h);
            var s = new Link1 { Name = name, H = h, T = t, Grab = grab, Hold = HoldBuffOf(grab), Held = HeldBuffOf(grab), Weapons = GrabWeapons(h, grab), Seq0 = LastSeq, Spot = spot };
            if (grab == null || s.Weapons.Length == 0) { problems.Add(name + ": the " + key + " has no grab component or grab limb"); yield break; }
            int seq = LastSeq;
            foreach (object step in AttackUntil(name, h, t, s.Weapons[0], () => HeldSingle(h, t))) yield return step;
            s.Unrelated = AddUnrelated(t);
            if (!HeldSingle(h, t)) yield break;
            ExpectSingleHeld(s, t, name + ".established");
            if (DecisiveSince(seq, h, t).Length != 0 || Events(seq, e => Is(e, "MaintainLink") && PairOf(e, h, t) && ((string)e["result"]).StartsWith("impale:", StringComparison.Ordinal)).Count != 0)
                problems.Add(name + ": a maintain rider resolved on the initial grab");
            s.Seq0 = LastSeq;
            done(s);
        }

        // ---------------------------------------------------------------- controls

        private sealed class Control { internal string Name; internal UnitEntityData H, T; internal Func<bool> Held; internal Buff Unrelated; internal int Seq0; internal Vector3 Spot; }

        private IEnumerable<object> BarbedControls(string name, Action<Control[]> done)
        {
            var list = new List<Control>();
            for (int k = 0; k < 2; k++)
            {
                Link1 s = null;
                foreach (object step in EstablishBarbed(name + ".control" + (k + 1), x => s = x)) yield return step;
                if (s == null) yield break;
                Link1 c = s;
                list.Add(new Control { Name = s.Name, H = s.H, T = s.T, Unrelated = s.Unrelated, Seq0 = s.Seq0, Held = () => HeldSingle(c), Spot = s.Spot });
            }
            done(list.ToArray());
        }

        /// <summary>
        /// At least one control intact; any control that ended did so by its own exactly attributed ordinary failed
        /// maintain (classified, no longer a witness). Unrelated effects on every control target kept.
        /// </summary>
        private void Witness(string at, Control[] controls)
        {
            int intact = 0;
            var outcomes = (JArray)(result["controlOutcomes"] ?? (result["controlOutcomes"] = new JArray()));
            foreach (Control c in controls)
            {
                if (!Kept(c.T, c.Unrelated)) problems.Add(at + ": control " + c.Name + " lost its unrelated effect");
                if (c.Held()) { intact++; continue; }
                string kind = Classify(c.Seq0, c.H, c.T);
                if (kind == "unattributed-release") kind = AttributeRoundRelease(c.Seq0, c.H, c.T);
                outcomes.Add(at + ": " + c.Name + " ended: " + kind);
                if (result["lostControl." + c.Name] == null)
                    result["lostControl." + c.Name] = new JObject { ["classification"] = kind, ["holderPosition"] = c.H.Position.ToString(), ["targetPosition"] = c.T.Position.ToString(),
                        ["events"] = Events(c.Seq0, e => PairOf(e, c.H, c.T) || (string)e["owner"] == c.H.UniqueId && ((string)e["what"]).StartsWith("multiHold.", StringComparison.Ordinal) ||
                            (string)e["unit"] == c.T.UniqueId || (Is(e, "rule.maneuver") && ((string)e["initiator"] == c.T.UniqueId || (string)e["target"] == c.T.UniqueId))) };
                if (kind != "ordinary-failed-maintain" && kind != "native-break-free") problems.Add(at + ": control " + c.Name + " ended without an exactly attributed ordinary event (" + kind + ")");
            }
            if (intact == 0) problems.Add(at + ": no intact independent control witnessed this terminal");
        }

        // ================================================================ Barbed Devil

        private IEnumerable<object> Barbed()
        {
            var cases = new JObject();
            result["cases"] = cases;

            // ---- 1. A later successful native maintain that produces Impale (bounded, attempts preserved).
            Link1 m = null;
            var attempts = new JArray();
            result["attempts.impale"] = attempts;
            for (int attempt = 1; attempt <= 3 && m == null; attempt++)
            {
                Link1 c = null;
                foreach (object step in EstablishBarbed("main" + attempt, x => c = x)) yield return step;
                if (c == null) yield break;
                if (attempt == 1) cases["ownerMap"] = OwnerMap(c);
                int seq = LastSeq;
                int damage = c.T.Descriptor.Damage;
                foreach (object step in SettleGameTime(0.0, 40f, () => DecisiveSince(seq, c.H, c.T).Length > 0 || !HeldSingle(c))) yield return step;
                foreach (object step in SettleGameTime(0.5, 5f, () => true)) yield return step;
                JObject[] decisive = DecisiveSince(seq, c.H, c.T);
                string kind = Classify(seq, c.H, c.T);
                var row = new JObject
                {
                    ["attempt"] = attempt, ["holder"] = c.H.UniqueId, ["target"] = c.T.UniqueId, ["classification"] = kind,
                    ["firstDecisive"] = decisive.Length == 0 ? null : decisive[0]["result"], ["events"] = PairEvents(seq, c.H, c.T),
                    ["state"] = SingleState(c), ["targetDamageBefore"] = damage
                };
                attempts.Add(row);
                if (decisive.Length >= 1 && ((string)decisive[0]["result"]).StartsWith("impale:", StringComparison.Ordinal) && kind == "held") { m = c; break; }
                if (kind == "ordinary-failed-maintain") continue;
                problems.Add("impale: attempt " + attempt + " ended with " + kind + " / " + row["firstDecisive"]);
                yield break;
            }
            if (m == null) { problems.Add("impale: retry budget exhausted by ordinary failed maintains"); yield break; }
            {
                JObject last = (JObject)attempts.Last();
                JArray ev = (JArray)last["events"];
                JObject impale = ev.OfType<JObject>().First(e => Is(e, "MaintainLink") && ((string)e["result"]).StartsWith("impale:", StringComparison.Ordinal));
                string r = (string)impale["result"];
                int strength = m.H.Descriptor.Stats.Strength.Bonus;
                int expected = strength > 0 ? strength + strength / 2 : strength;
                JObject[] maneuvers = ev.OfType<JObject>().Where(e => Is(e, "rule.maneuver") && (string)e["initiator"] == m.H.UniqueId && (int)e["seq"] < (int)impale["seq"]).ToArray();
                JObject[] damage = ev.OfType<JObject>().Where(e => Is(e, "rule.damage")).ToArray();
                var row = new JObject { ["result"] = r, ["liveStrengthBonus"] = strength, ["expectedBonus"] = expected, ["maneuvers"] = new JArray(maneuvers.Cast<object>().ToArray()),
                    ["damage"] = new JArray(damage.Cast<object>().ToArray()), ["stateAfter"] = SingleState(m) };
                cases["impale"] = row;
                if (!r.StartsWith("impale:3d8+" + expected + ";", StringComparison.Ordinal) || !r.Contains("targetDied=False") || !r.Contains("heldKept=True"))
                    problems.Add("impale: unexpected rider result " + r);
                if (maneuvers.Length != 1 || !(bool)maneuvers[0]["productSuccess"]) problems.Add("impale: not exactly one successful maintain maneuver before the rider");
                if (damage.Length != 1 || (bool)damage[0]["attackRoll"] || (string)damage[0]["weapon"] != null) problems.Add("impale: the rider window does not hold exactly one direct (no attack, no weapon) damage rule");
                ExpectSingleHeld(m, m.T, "impale.after");
            }

            // ---- 2. A real failed maintain (disclosed temporary high CMD), deferred exact-owner cleanup.
            Control[] controls = null;
            foreach (object step in BarbedControls("failedMaintain", x => controls = x)) yield return step;
            if (controls == null) yield break;
            {
                var row = new JObject();
                cases["failedMaintain"] = row;
                var stats = m.T.Descriptor.Stats;
                int cmdBefore = stats.AdditionalCMD.BaseValue;
                stats.AdditionalCMD.BaseValue = 1000;
                row["setup"] = "target AdditionalCMD base " + cmdBefore + " -> 1000 until the release, natural d20 kept";
                var rounds = new JArray();
                row["rounds"] = rounds;
                int seqAll = LastSeq;
                JObject released = null;
                int roundSeq = seqAll;
                for (int round = 1; round <= 3 && released == null; round++)
                {
                    roundSeq = LastSeq;
                    int s0 = roundSeq;
                    foreach (object step in SettleGameTime(0.0, 25f, () => DecisiveSince(s0, m.H, m.T).Length > 0 || !HeldSingle(m))) yield return step;
                    JObject[] decisive = DecisiveSince(s0, m.H, m.T);
                    var rr = new JObject { ["round"] = round, ["decisive"] = new JArray(decisive.Cast<object>().ToArray()), ["events"] = PairEvents(s0, m.H, m.T) };
                    rounds.Add(rr);
                    if (decisive.Length == 1 && (string)decisive[0]["result"] == "released") { released = decisive[0]; break; }
                    if (decisive.Length == 1 && ((string)decisive[0]["result"]).StartsWith("impale:", StringComparison.Ordinal)) continue; // natural 20 under high CMD, recorded
                    break;
                }
                stats.AdditionalCMD.BaseValue = cmdBefore;
                row["cmdRestored"] = stats.AdditionalCMD.BaseValue == cmdBefore;
                if (released == null) { problems.Add("failedMaintain: no exact 'released' maintain within three rounds: " + rounds.ToString(Formatting.None)); yield break; }
                int relSeq = (int)released["seq"];
                // Let the before-tick scheduler run; the bounded wait has its own postconditions below.
                foreach (object step in SettleGameTime(0.0, 10f, () => m.H.Get<UnitPartGrappleInitiator>() == null && HoldInstance(m) == null)) yield return step;
                foreach (object step in SettleGameTime(0.2, 5f, () => true)) yield return step;
                JArray window = PairEvents(roundSeq, m.H, m.T);
                row["window"] = window;
                JObject[] w = window.OfType<JObject>().ToArray();
                JObject[] maneuvers = w.Where(e => Is(e, "rule.maneuver") && (string)e["initiator"] == m.H.UniqueId && (int)e["seq"] < relSeq).ToArray();
                JObject releaseLink = w.LastOrDefault(e => Is(e, "SummonHoldComponent.ReleaseLink") && (int)e["seq"] < relSeq);
                JObject schedule = w.FirstOrDefault(e => Is(e, "impaleCleanup.schedule"));
                JObject begin = w.FirstOrDefault(e => Is(e, "impaleCleanup.callback.begin"));
                JObject end = w.FirstOrDefault(e => Is(e, "impaleCleanup.callback.end"));
                row["failedManeuver"] = maneuvers.LastOrDefault();
                row["released"] = released;
                row["releaseLinkAtCall"] = releaseLink;
                row["schedule"] = schedule;
                row["callbackBegin"] = begin;
                row["callbackEnd"] = end;
                row["after"] = SingleState(m);
                if (maneuvers.Length != 1 || (bool)maneuvers[0]["productSuccess"] || (int)maneuvers[0]["cmd"] < 1000)
                    problems.Add("failedMaintain: the final round does not hold exactly one failed holder/target grapple maneuver against the raised CMD");
                if (w.Any(e => Is(e, "MaintainLink") && (int)e["seq"] >= relSeq && ((string)e["result"]).StartsWith("impale:", StringComparison.Ordinal)))
                    problems.Add("failedMaintain: an Impale followed the failed check");
                int failedSeq = maneuvers.Length == 0 ? relSeq : (int)maneuvers.Last()["seq"];
                if (w.Any(e => Is(e, "rule.damage") && (int)e["seq"] > failedSeq)) problems.Add("failedMaintain: the holder damaged the target after the failed check");
                if (releaseLink == null) problems.Add("failedMaintain: no ReleaseLink for the exact pair");
                else row["ownerRetainedAtRelease"] = (bool)releaseLink["ownerInitiatorPart"] && (int)releaseLink["ownerHoldBuffs"] > 0; // captured
                if (schedule == null) problems.Add("failedMaintain: the deferred owner cleanup was not scheduled");
                if (begin == null || end == null) problems.Add("failedMaintain: the scheduled cleanup callback did not run through the game loop");
                else
                {
                    if ((int)begin["frame"] <= (int)released["frame"]) problems.Add("failedMaintain: the cleanup callback ran in the release frame");
                    if ((bool)end["ownerInitiatorPart"] || (int)end["ownerHoldBuffs"] != 0 || (bool)end["targetPart"])
                        problems.Add("failedMaintain: the callback left owner or target state: " + end.ToString(Formatting.None));
                    row["callbackFoundOwnerState"] = (bool)begin["ownerInitiatorPart"]; // captured: whether the callback itself removed it
                }
                ExpectSingleEnded(m, m.T, "failedMaintain", true);
                if (!Kept(m.T, m.Unrelated)) problems.Add("failedMaintain: the target's unrelated effect was lost");
                Witness("failedMaintain", controls);
                // Quiet: no continued rider damage and no recreated relationship.
                int q0 = LastSeq;
                double t0 = Now;
                foreach (object step in SettleGameTime(7.0, 25f, () => true)) yield return step;
                row["quiet.gameSeconds"] = Now - t0;
                row["quiet.events"] = PairEvents(q0, m.H, m.T);
                row["quiet.state"] = SingleState(m);
                if (Now - t0 < 6.5) problems.Add("failedMaintain.quiet: less than one round of game time");
                if (((JArray)row["quiet.events"]).OfType<JObject>().Any(e => Is(e, "rule.damage") || Is(e, "MaintainLink"))) problems.Add("failedMaintain.quiet: rider damage or maintain continued");
                ExpectSingleEnded(m, m.T, "failedMaintain.quiet", true);
                Witness("failedMaintain.quiet", controls);
                Retire(controls);
                row["guardSource"] = "ScheduleReleasedImpaleOwnerCleanup schedules only for the exact Impale owner whose initiator still names the target that no longer names it back; the callback re-checks the same three facts before removing only that owner's initiator part, hold buff and record. A stale-callback race was not constructed (source analysis only).";
            }

            // ---- 3. Reacquire through a real claw at a fresh adjacent living target.
            UnitEntityData t2 = SpawnTarget(Around(m.H.Position, 90f, 1.0f));
            Buff unrelated2 = null;
            {
                var row = new JObject();
                cases["reacquire"] = row;
                if (t2 == null) { problems.Add("reacquire: target not created"); yield break; }
                yield return null;
                int seq = LastSeq;
                row["before"] = SingleState(m, t2);
                foreach (object step in AttackUntil("reacquire", m.H, t2, m.Weapons[0], () => HeldSingle(m.H, t2))) yield return step;
                if (!HeldSingle(m.H, t2)) yield break;
                unrelated2 = AddUnrelated(t2);
                ExpectSingleHeld(m, t2, "reacquire.established");
                int frame0 = Time.frameCount;
                foreach (object step in SettleGameTime(2.0, 10f, () => Time.frameCount - frame0 >= 30)) yield return step;
                row["frames"] = Time.frameCount - frame0;
                row["after"] = SingleState(m, t2);
                row["events"] = PairEvents(seq, m.H, t2);
                if (Time.frameCount - frame0 < 30) problems.Add("reacquire: fewer than 30 engine frames observed");
                ExpectSingleHeld(m, t2, "reacquire.afterFrames");
                if (((JArray)row["events"]).OfType<JObject>().Any(e => Is(e, "SummonHoldComponent.ReleaseLink") || ((string)e["what"]).StartsWith("impaleCleanup.callback", StringComparison.Ordinal)))
                    problems.Add("reacquire: a release or cleanup callback touched the new relationship");
            }

            // ---- 4. Owner terminal: native Dismissal through the ordinary command path (bounded), or native expiry.
            Control[] controls2 = null;
            foreach (object step in BarbedControls("ownerTerminal", x => controls2 = x)) yield return step;
            if (controls2 == null) yield break;
            {
                var row = new JObject();
                cases["ownerTerminal"] = row;
                int seq = LastSeq;
                bool done = false;
                foreach (object step in Dismiss(m, t2, row, x => done = x)) yield return step;
                double terminalAt = Now;
                if (!done)
                {
                    row["fallback"] = "native summon expiry (deadline moved to now)";
                    if (!HeldSingle(m.H, t2)) { problems.Add("ownerTerminal: the hold is not active immediately before the fallback expiry"); yield break; }
                    row["activeImmediatelyBefore"] = SingleState(m, t2);
                    row["expiry"] = SetDeadline(m.H, TimeSpan.FromMilliseconds(-1));
                    foreach (object step in SettleGameTime(0.0, 20f, () => m.H.Destroyed && t2.Get<UnitPartGrappleTarget>() == null)) yield return step;
                }
                if (!done) row["releaseGameSeconds"] = Now - terminalAt;
                row["after"] = SingleState(m, t2);
                ExpectSingleEnded(m, t2, "ownerTerminal", false);
                if (!m.H.Destroyed && !m.H.Descriptor.State.IsDead) problems.Add("ownerTerminal: the Barbed Devil is still present");
                if (!Kept(t2, unrelated2)) problems.Add("ownerTerminal: the target's unrelated effect was lost");
                Witness("ownerTerminal", controls2);
                int q0 = LastSeq;
                double t0 = Now;
                int dmg = t2.Descriptor.Damage;
                foreach (object step in SettleGameTime(13.0, 40f, () => true)) yield return step;
                row["quiet.gameSeconds"] = Now - t0;
                row["quiet.state"] = SingleState(m, t2);
                row["events"] = PairEvents(seq, m.H, t2);
                if (Now - t0 < 12.0) problems.Add("ownerTerminal.quiet: fewer than two rounds of game time");
                if (t2.Descriptor.Damage != dmg || PairEvents(q0, m.H, t2).Count != 0) problems.Add("ownerTerminal.quiet: damage or relationship activity after the terminal");
                ExpectSingleEnded(m, t2, "ownerTerminal.quiet", false);
                Witness("ownerTerminal.quiet", controls2);
            }
        }

        private JObject OwnerMap(Link1 s)
        {
            var map = new JObject
            {
                ["holderBlueprint"] = s.H.Blueprint.name,
                ["grabCarriedByBuffs"] = new JArray(s.H.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(b => b.Blueprint.ComponentsArray != null &&
                    b.Blueprint.ComponentsArray.Any(c => c.GetType() == grabType)).Select(b => (object)b.Blueprint.name).ToArray()),
                ["grabCarriedByFeatures"] = new JArray(s.H.Descriptor.Progression.Features.Enumerable.Where(f => f.Blueprint.ComponentsArray != null &&
                    f.Blueprint.ComponentsArray.Any(c => c.GetType() == grabType)).Select(f => (object)f.Blueprint.name).ToArray()),
                ["impaleOwnerIsHolder"] = ReferenceEquals(grabType.GetField("ImpaleProfileOwner").GetValue(s.Grab), s.H.Blueprint),
                ["maxHeld"] = (int)grabType.GetField("MaxHeldTargets").GetValue(s.Grab),
                ["holdBuff"] = s.Hold?.name, ["heldBuff"] = s.Held?.name,
                ["grabLimbs"] = new JArray(s.Weapons.Select(w => (object)(w.Blueprint.name + "@" + HoldLifecycleObserver.LimbOf(s.H, w))).ToArray()),
                ["holderFacts"] = new JArray(s.H.Descriptor.Progression.Features.Enumerable.Select(f => (object)(f.Blueprint.name + ":" + f.Blueprint.AssetGuid)).ToArray())
            };
            if (((JArray)map["grabCarriedByBuffs"]).Count != 0 || ((JArray)map["grabCarriedByFeatures"]).Count != 1) problems.Add("ownerMap: the Barbed grab is not carried by exactly one feature and no buff");
            if (!(bool)map["impaleOwnerIsHolder"] || (int)map["maxHeld"] != 1) problems.Add("ownerMap: the Impale opt-in or single-link configuration is not the holder's");
            return map;
        }

        /// <summary>Native Dismissal by the main character through UnitUseAbility; up to two casts (a natural-20 save is retained).</summary>
        private IEnumerable<object> Dismiss(Link1 m, UnitEntityData t2, JObject row, Action<bool> done)
        {
            var dismissal = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>(NativeDismissalGuid);
            UnitEntityData caster = Game.Instance.Player.MainCharacter.Value;
            if (dismissal == null) { row["dismissal"] = "native Dismissal blueprint not found"; done(false); yield break; }
            row["ability"] = dismissal.name + ":" + dismissal.AssetGuid;
            row["checkers"] = new JArray((dismissal.ComponentsArray ?? new BlueprintComponent[0]).OfType<AbilityTargetHasFact>().Select(x => (object)new JObject
            {
                ["inverted"] = x.Inverted,
                ["facts"] = new JArray((x.CheckedFacts ?? new Kingmaker.Blueprints.Facts.BlueprintUnitFact[0]).Select(f => (object)(f == null ? null : f.name + ":" + f.AssetGuid)).ToArray()),
                ["holderHas"] = (x.CheckedFacts ?? new Kingmaker.Blueprints.Facts.BlueprintUnitFact[0]).Any(f => f != null && m.H.Descriptor.HasFact(f))
            }).ToArray());
            bool added = !caster.Descriptor.HasFact(dismissal);
            if (added) caster.Descriptor.AddFact(dismissal);
            row["factGranted"] = added;
            Ability granted = caster.Descriptor.Abilities.GetAbility(dismissal);
            int willBefore = m.H.Descriptor.Stats.SaveWill.BaseValue;
            bool success = false;
            try
            {
                if (granted == null) { row["dismissal"] = "the granted Dismissal ability is missing"; yield break; }
                granted.OverrideParams = new AbilityParams { CasterLevel = 40, SpellLevel = 4 };
                var data = new AbilityData(granted);
                bool canTarget = data.CanTarget(new TargetWrapper(m.H));
                row["canTarget"] = canTarget;
                row["targeting"] = new JObject
                {
                    ["canTargetEnemies"] = dismissal.CanTargetEnemies, ["canTargetFriends"] = dismissal.CanTargetFriends, ["canTargetSelf"] = dismissal.CanTargetSelf,
                    ["canTargetPoint"] = dismissal.CanTargetPoint, ["range"] = dismissal.Range.ToString(),
                    ["casterIsAllyOfHolder"] = caster.IsAlly(m.H), ["casterIsEnemyOfHolder"] = caster.IsEnemy(m.H),
                    ["components"] = new JArray((dismissal.ComponentsArray ?? new BlueprintComponent[0]).Select(x => (object)x.GetType().Name).ToArray()),
                    ["checkers"] = new JArray((dismissal.ComponentsArray ?? new BlueprintComponent[0]).OfType<Kingmaker.UnitLogic.Abilities.Components.Base.IAbilityTargetChecker>()
                        .Select(x => (object)(x.GetType().Name + "=" + x.CanTarget(caster, new TargetWrapper(m.H)))).ToArray())
                };
                if (!canTarget) { row["dismissal"] = "legally rejected: AbilityData.CanTarget is false for this Barbed Devil"; yield break; }
                m.H.Descriptor.Stats.SaveWill.BaseValue = -100;
                row["setup"] = "main character Dismissal fact granted=" + added + "; cast parameters CL 40 / spell level 4; Barbed Will base " + willBefore + " -> -100; natural rolls kept";
                var casts = new JArray();
                row["casts"] = casts;
                for (int cast = 1; cast <= 2 && !success; cast++)
                {
                    if (!HeldSingle(m.H, t2)) { problems.Add("ownerTerminal: the hold is not active immediately before cast " + cast); yield break; }
                    var castRow = new JObject { ["cast"] = cast, ["activeImmediatelyBefore"] = SingleState(m, t2) };
                    int seq = LastSeq;
                    double start = Now;
                    UnitCommand command = UnitUseAbility.CreateCastCommand(data, new TargetWrapper(m.H));
                    caster.Commands.Run(command);
                    foreach (object step in SettleGameTime(0.0, 30f, () => command.IsFinished && (m.H.Destroyed || m.H.Descriptor.State.IsDead || Events(seq, e => Is(e, "rule.save") && (string)e["unit"] == m.H.UniqueId).Count > 0))) yield return step;
                    foreach (object step in SettleGameTime(0.0, 10f, () => m.H.Destroyed || m.H.Descriptor.State.IsDead || command.IsFinished)) yield return step;
                    success = m.H.Destroyed || m.H.Descriptor.State.IsDead;
                    castRow["commandFinished"] = command.IsFinished;
                    castRow["commandResult"] = command.Result.ToString();
                    castRow["saves"] = Events(seq, e => Is(e, "rule.save") && (string)e["unit"] == m.H.UniqueId);
                    castRow["spellResistance"] = Events(seq, e => Is(e, "rule.spellResistance") && (string)e["target"] == m.H.UniqueId);
                    castRow["holderGone"] = success;
                    castRow["startGameSeconds"] = start;
                    castRow["gameSecondsToTerminal"] = Now - start;
                    casts.Add(castRow);
                    if (success) row["terminalGameSeconds"] = start;
                }
                if (!success) row["dismissal"] = "no successful cast within two bounded attempts";
            }
            finally
            {
                if (granted != null) granted.OverrideParams = null;
                if (added && caster.Descriptor.HasFact(dismissal)) caster.Descriptor.RemoveFact(dismissal);
                if (!m.H.Destroyed) m.H.Descriptor.Stats.SaveWill.BaseValue = willBefore;
                done(success);
            }
            if (success)
                foreach (object step in SettleGameTime(0.0, 20f, () => t2.Get<UnitPartGrappleTarget>() == null && t2.Descriptor.Buffs.GetBuff(m.Held) == null)) yield return step;
        }

        // ================================================================ Giant Crab

        private sealed class LinkN { internal string Name; internal UnitEntityData H; internal List<UnitEntityData> T = new List<UnitEntityData>(); internal object Grab; internal BlueprintBuff Hold, Held; internal ItemEntityWeapon[] Weapons; internal Vector3 Spot; internal int Seq0; }

        private Buff[] HeldStates(LinkN c, UnitEntityData t) => t.Destroyed ? new Buff[0] : t.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(b => ReferenceEquals(b.Blueprint, c.Held)).ToArray();
        private bool HeldBy(LinkN c, UnitEntityData t) => HeldStates(c, t).Any(b => b.Context != null && ReferenceEquals(b.Context.MaybeCaster, c.H));

        private JObject MultiState(LinkN c)
        {
            UnitEntityData h = c.H;
            IList<string> raw = h.Destroyed ? null : RawLinks(h);
            Buff hold = h.Destroyed ? null : h.Descriptor.Buffs.RawFacts.OfType<Buff>().FirstOrDefault(b => ReferenceEquals(b.Blueprint, c.Hold));
            return new JObject
            {
                ["holder"] = new JObject
                {
                    ["id"] = h.UniqueId, ["dead"] = h.Descriptor.State.IsDead, ["destroyed"] = h.Destroyed,
                    ["holdBuffs"] = h.Destroyed ? 0 : h.Descriptor.Buffs.RawFacts.OfType<Buff>().Count(b => ReferenceEquals(b.Blueprint, c.Hold)),
                    ["holdClock"] = BuffClock(hold),
                    ["nativeInitiatorPart"] = !h.Destroyed && h.Get<UnitPartGrappleInitiator>() != null,
                    ["cantAct"] = !h.Destroyed && h.Descriptor.State.HasCondition(UnitCondition.CantAct),
                    ["cantMove"] = !h.Destroyed && h.Descriptor.State.HasCondition(UnitCondition.CantMove),
                    ["raw.links"] = raw == null ? null : new JArray(raw.Cast<object>().ToArray()),
                    ["live.reconciled"] = h.Destroyed ? new JArray() : new JArray(Reconciled(h).Cast<object>().ToArray()),
                    ["live.heldCount"] = h.Destroyed || c.Grab == null ? 0 : (int)grabType.GetMethod("HeldCount", Any).Invoke(c.Grab, new object[] { h }),
                    ["live.occupancy"] = new JArray(c.Weapons.Select(w => (object)new JObject { ["limb"] = HoldLifecycleObserver.LimbOf(h, w), ["occupant"] = h.Destroyed ? null : Occupant(h, w) }).ToArray())
                },
                ["targets"] = new JArray(c.T.Select(t => (object)new JObject
                {
                    ["id"] = t.UniqueId, ["dead"] = t.Descriptor.State.IsDead, ["destroyed"] = t.Destroyed,
                    ["heldStates"] = new JArray(HeldStates(c, t).Select(b => (object)new JObject { ["caster"] = b.Context?.MaybeCaster?.UniqueId, ["clock"] = BuffClock(b) }).ToArray()),
                    ["rawRecords"] = raw == null ? 0 : raw.Count(l => l != null && l.StartsWith(t.UniqueId + "|", StringComparison.Ordinal)),
                    ["nativeTargetPart"] = !t.Destroyed && t.Get<UnitPartGrappleTarget>() != null,
                    ["entangled"] = t.Descriptor.State.HasCondition(UnitCondition.Entangled), ["cantMove"] = t.Descriptor.State.HasCondition(UnitCondition.CantMove),
                    ["cantAct"] = t.Descriptor.State.HasCondition(UnitCondition.CantAct), ["canMove"] = t.Descriptor.State.CanMove, ["canAct"] = t.Descriptor.State.CanAct,
                    ["damage"] = t.Descriptor.Damage, ["buffs"] = Buffs(t)
                }).ToArray())
            };
        }

        /// <summary>Target t held by the crab through the limb of weapon w: one held state naming the holder, one raw record on that limb, occupancy, conditions.</summary>
        private void ExpectMultiHeld(LinkN c, UnitEntityData t, ItemEntityWeapon w, string name)
        {
            Buff[] states = HeldStates(c, t);
            string limb = HoldLifecycleObserver.LimbOf(c.H, w);
            IList<string> raw = RawLinks(c.H);
            string[] records = raw == null ? new string[0] : raw.Where(l => l != null && l.StartsWith(t.UniqueId + "|", StringComparison.Ordinal)).ToArray();
            if (states.Length != 1 || !ReferenceEquals(states[0].Context?.MaybeCaster, c.H)) problems.Add(name + ": " + t.UniqueId + " does not have exactly one held state naming the crab");
            if (records.Length != 1 || !records[0].StartsWith(t.UniqueId + "|" + limb + "|", StringComparison.Ordinal)) problems.Add(name + ": the raw record for " + t.UniqueId + " is not exactly one on limb " + limb + ": " + string.Join(",", records));
            if (Occupant(c.H, w) != t.UniqueId) problems.Add(name + ": limb " + limb + " is not occupied by " + t.UniqueId + " (" + Occupant(c.H, w) + ")");
            if (EstablishingLimb(c.H, t) != limb) problems.Add(name + ": the establishing limb of " + t.UniqueId + " is " + EstablishingLimb(c.H, t) + ", expected " + limb);
            if (!t.Descriptor.State.HasCondition(UnitCondition.Entangled) || !t.Descriptor.State.HasCondition(UnitCondition.CantMove)) problems.Add(name + ": " + t.UniqueId + " lacks Entangled/CantMove");
            if (t.Get<UnitPartGrappleTarget>() != null || c.H.Get<UnitPartGrappleInitiator>() != null) problems.Add(name + ": a native single-link part is present");
            if (c.H.Descriptor.Buffs.RawFacts.OfType<Buff>().Count(b => ReferenceEquals(b.Blueprint, c.Hold)) != 1) problems.Add(name + ": the crab does not carry exactly one multi-hold buff");
            if (c.H.Descriptor.State.HasCondition(UnitCondition.CantAct) || c.H.Descriptor.State.HasCondition(UnitCondition.CantMove)) problems.Add(name + ": the multi-link holder carries CantAct/CantMove");
        }

        private void ExpectMultiEnded(LinkN c, UnitEntityData t, ItemEntityWeapon w, string name)
        {
            if (HeldStates(c, t).Length != 0) problems.Add(name + ": " + t.UniqueId + " keeps a held state");
            if (!c.H.Destroyed && Reconciled(c.H).Any(r => r.StartsWith(t.UniqueId + "|", StringComparison.Ordinal))) problems.Add(name + ": " + t.UniqueId + " still answers live");
            if (!c.H.Destroyed && w != null && Occupant(c.H, w) == t.UniqueId) problems.Add(name + ": limb " + HoldLifecycleObserver.LimbOf(c.H, w) + " still reports " + t.UniqueId);
            UnitState st = t.Descriptor.State;
            if (!st.IsDead && (st.HasCondition(UnitCondition.Entangled) || st.HasCondition(UnitCondition.CantMove) || st.HasCondition(UnitCondition.CantAct) || !st.CanMove || !st.CanAct))
                problems.Add(name + ": surviving " + t.UniqueId + " is still constrained");
        }

        private IEnumerable<object> EstablishCrab(string name, int links, Action<LinkN> done)
        {
            Vector3 spot = NextSpot();
            UnitEntityData h = Summon("giant-crab", spot);
            if (h == null) { problems.Add(name + ": the crab was not summoned"); yield break; }
            var c = new LinkN { Name = name, H = h, Spot = spot };
            for (int k = 0; k < links; k++)
            {
                UnitEntityData t = SpawnTarget(Around(h.Position, links == 1 ? 0f : k == 0 ? -60f : 60f, 1.1f));
                if (t == null) { problems.Add(name + ": a target was not created"); yield break; }
                c.T.Add(t);
            }
            yield return null;
            c.Grab = Grab(h);
            c.Hold = HoldBuffOf(c.Grab);
            c.Held = HeldBuffOf(c.Grab);
            c.Weapons = GrabWeapons(h, c.Grab);
            var baseline = new JObject
            {
                ["holderBlueprint"] = h.Blueprint.name, ["holderSize"] = h.Descriptor.State.Size.ToString(),
                ["multiLink"] = c.Grab != null && (bool)grabType.GetProperty("MultiLink", Any).GetValue(c.Grab, null),
                ["maxHeld"] = c.Grab == null ? 0 : (int)grabType.GetField("MaxHeldTargets").GetValue(c.Grab),
                ["crabConstrictOwnerIsHolder"] = c.Grab != null && ReferenceEquals(grabType.GetField("CrabProfileOwner").GetValue(c.Grab), h.Blueprint),
                ["grabLimbs"] = new JArray(c.Weapons.Select(w => (object)(w.Blueprint.name + "@" + HoldLifecycleObserver.LimbOf(h, w))).ToArray()),
                ["holdBuff"] = c.Hold?.name, ["heldBuff"] = c.Held?.name,
                ["targets"] = new JArray(c.T.Select(t => (object)new JObject
                {
                    ["id"] = t.UniqueId, ["size"] = t.Descriptor.State.Size.ToString(), ["distance"] = Vector3.Distance(h.Position, t.Position),
                    ["grabSizeAllowed"] = c.Grab != null && (bool)profiles.GetMethod("IsGrabSizeAllowed", Any).Invoke(null, new object[] { (int)t.Descriptor.State.Size, (int)h.Descriptor.State.Size, (int)grabType.GetField("MaxTargetSizeDelta").GetValue(c.Grab) }),
                    ["entangled"] = t.Descriptor.State.HasCondition(UnitCondition.Entangled), ["cantMove"] = t.Descriptor.State.HasCondition(UnitCondition.CantMove)
                }).ToArray())
            };
            result["baseline." + name] = baseline;
            if (!(bool)baseline["multiLink"] || (int)baseline["maxHeld"] != 2 || !(bool)baseline["crabConstrictOwnerIsHolder"] || c.Weapons.Length != 2 ||
                ((JArray)baseline["targets"]).Any(t => !(bool)t["grabSizeAllowed"] || (bool)t["entangled"] || (bool)t["cantMove"]))
            { problems.Add(name + ": fixture baseline is not admissible: " + baseline.ToString(Formatting.None)); yield break; }
            for (int k = 0; k < links; k++)
            {
                UnitEntityData t = c.T[k];
                foreach (object step in AttackUntil(name + ".link" + k, h, t, c.Weapons[k], () => HeldBy(c, t))) yield return step;
                if (!HeldBy(c, t)) yield break;
                ExpectMultiHeld(c, t, c.Weapons[k], name + ".established" + k);
            }
            c.Seq0 = LastSeq;
            done(c);
        }

        private IEnumerable<object> CrabControls(string name, Action<Control[]> done)
        {
            var list = new List<Control>();
            for (int k = 0; k < 2; k++)
            {
                LinkN s = null;
                foreach (object step in EstablishCrab(name + ".control" + (k + 1), 1, x => s = x)) yield return step;
                if (s == null) yield break;
                LinkN c = s;
                list.Add(new Control { Name = s.Name, H = s.H, T = s.T[0], Unrelated = AddUnrelated(s.T[0]), Seq0 = s.Seq0, Held = () => HeldBy(c, c.T[0]), Spot = s.Spot });
            }
            done(list.ToArray());
        }

        /// <summary>The per-link maintain/constrict contract over a window: each listed target's own maintain on its own pincer.</summary>
        private void ExpectPerLinkRound(LinkN c, int seq, UnitEntityData[] targets, ItemEntityWeapon[] weapons, string name, JObject row)
        {
            for (int k = 0; k < targets.Length; k++)
            {
                UnitEntityData t = targets[k];
                string limb = HoldLifecycleObserver.LimbOf(c.H, weapons[k]);
                JObject[] decisive = DecisiveSince(seq, c.H, t);
                JObject[] constrict = HoldLifecycleObserver.Events.Where(e => (int)e["seq"] > seq && Is(e, "DealConstrict") && PairOf(e, c.H, t)).ToArray();
                row[name + ".link" + k] = new JObject { ["target"] = t.UniqueId, ["limb"] = limb, ["maintain"] = new JArray(decisive.Cast<object>().ToArray()), ["constrict"] = new JArray(constrict.Cast<object>().ToArray()) };
                if (decisive.Length < 1 || !((string)decisive[0]["result"]).StartsWith("maintained:", StringComparison.Ordinal) || ((string)decisive[0]["result"]).Contains("substituted"))
                    problems.Add(name + ": link " + k + " has no unsubstituted 'maintained' result: " + (decisive.Length == 0 ? "none" : (string)decisive[0]["result"]));
                if (constrict.Length < 1 || (string)constrict[0]["limb"] != limb || (int)constrict[0]["delta"] <= 0)
                    problems.Add(name + ": link " + k + " lacks a constrict on its own pincer " + limb + ": " + new JArray(constrict.Cast<object>().ToArray()).ToString(Formatting.None));
            }
            // No wrong-target damage: every crab damage rule in the window lands on one of the listed live links.
            var ids = new HashSet<string>(targets.Select(t => t.UniqueId));
            JObject[] stray = HoldLifecycleObserver.Events.Where(e => (int)e["seq"] > seq && Is(e, "rule.damage") && (string)e["initiator"] == c.H.UniqueId && !ids.Contains((string)e["target"])).ToArray();
            if (stray.Length > 0) problems.Add(name + ": crab damage landed on a target that is not a live link: " + new JArray(stray.Cast<object>().ToArray()).ToString(Formatting.None));
            JObject[] strayConstrict = HoldLifecycleObserver.Events.Where(e => (int)e["seq"] > seq && Is(e, "DealConstrict") && (string)e["owner"] == c.H.UniqueId && !ids.Contains((string)e["target"])).ToArray();
            if (strayConstrict.Length > 0) problems.Add(name + ": a constrict was attributed to a target that is not a live link");
        }

        private IEnumerable<object> Crab()
        {
            var cases = new JObject();
            result["cases"] = cases;
            var attempts = new JArray();
            result["attempts.crab"] = attempts;
            for (int attempt = 1; attempt <= 3; attempt++)
            {
                string outcome = null;
                var row = new JObject { ["attempt"] = attempt };
                foreach (object step in CrabAttempt(attempt, row, x => outcome = x)) yield return step;
                row["outcome"] = outcome;
                attempts.Add(row);
                if (outcome != "retry") break;
                if (attempt == 3) problems.Add("crab: retry budget exhausted by ordinary failed maintains");
            }
        }

        /// <summary>One whole two-link Crab case; reports "retry" only for an exactly attributed ordinary failed maintain.</summary>
        private IEnumerable<object> CrabAttempt(int attempt, JObject row, Action<string> outcome)
        {
            int problemsBefore = problems.Count;
            LinkN c = null;
            foreach (object step in EstablishCrab("main" + attempt, 2, x => c = x)) yield return step;
            if (c == null) { outcome("fail"); yield break; }
            UnitEntityData x1 = c.T[0], x2 = c.T[1];
            ItemEntityWeapon p1 = c.Weapons[0], p2 = c.Weapons[1];
            Buff unrelated1 = AddUnrelated(x1), unrelated2 = AddUnrelated(x2);
            row["established"] = MultiState(c);
            if (Occupant(c.H, p1) == Occupant(c.H, p2)) problems.Add("crab: the two pincers report the same occupant");
            Func<UnitEntityData[], int, string> ordinary = (ts, s0) =>
            {
                string[] kinds = ts.Select(t => Classify(s0, c.H, t)).ToArray();
                if (kinds.All(k => k == "held")) return "held";
                return kinds.All(k => k == "held" || k == "ordinary-failed-maintain") ? "ordinary" : string.Join(",", kinds);
            };

            // ---- later-round maintain/constrict for each link
            int seq = LastSeq;
            foreach (object step in SettleGameTime(0.0, 40f, () => DecisiveSince(seq, c.H, x1).Length > 0 && DecisiveSince(seq, c.H, x2).Length > 0)) yield return step;
            foreach (object step in SettleGameTime(0.3, 5f, () => true)) yield return step;
            string k1 = ordinary(new[] { x1, x2 }, seq);
            row["firstRound.classification"] = k1;
            if (k1 == "ordinary") { row["firstRound.events"] = PairEvents(seq, c.H, x1); outcome("retry"); yield break; }
            if (k1 != "held") { problems.Add("crab.firstRound: " + k1); outcome("fail"); yield break; }
            ExpectPerLinkRound(c, seq, new[] { x1, x2 }, new[] { p1, p2 }, "crab.firstRound", row);
            ExpectMultiHeld(c, x1, p1, "crab.firstRound.after");
            ExpectMultiHeld(c, x2, p2, "crab.firstRound.after");

            // ---- end link 1 by target death
            Control[] controls = null;
            foreach (object step in CrabControls("crab.targetDeath" + attempt, x => controls = x)) yield return step;
            if (controls == null) { outcome("fail"); yield break; }
            {
                var death = new JObject();
                row["targetDeath"] = death;
                if (!HeldBy(c, x1) || !HeldBy(c, x2)) { string k = ordinary(new[] { x1, x2 }, seq); if (k == "ordinary") { outcome("retry"); yield break; } problems.Add("crab.targetDeath: links not both active before the death: " + k); outcome("fail"); yield break; }
                death["before"] = MultiState(c);
                int s0 = LastSeq;
                double t0 = Now;
                GameHelper.KillUnit(x1);
                foreach (object step in SettleGameTime(0.0, 15f, () => HeldStates(c, x1).Length == 0 && Occupant(c.H, p1) == null)) yield return step;
                death["releaseGameSeconds"] = Now - t0;
                death["after"] = MultiState(c);
                death["events"] = PairEvents(s0, c.H, x1);
                ExpectMultiEnded(c, x1, p1, "crab.targetDeath");
                if (!HeldBy(c, x2)) { string k = Classify(seq, c.H, x2); if (k == "ordinary-failed-maintain") { outcome("retry"); yield break; } }
                ExpectMultiHeld(c, x2, p2, "crab.targetDeath.sibling");
                IList<string> raw = RawLinks(c.H);
                death["F2.rawRecordForDeadTarget"] = raw == null ? 0 : raw.Count(l => l.StartsWith(x1.UniqueId + "|", StringComparison.Ordinal)); // captured; accepted F2 classification
                if (!Kept(x2, unrelated2)) problems.Add("crab.targetDeath: the sibling's unrelated effect was lost");
                Witness("crab.targetDeath", controls);
                Retire(controls);

                // The sibling keeps its own maintain/constrict; nothing more is attributed to the ended link.
                int s1 = LastSeq;
                foreach (object step in SettleGameTime(0.0, 30f, () => DecisiveSince(s1, c.H, x2).Length > 0)) yield return step;
                foreach (object step in SettleGameTime(0.3, 5f, () => true)) yield return step;
                string ks = Classify(s1, c.H, x2);
                death["siblingRound.classification"] = ks;
                if (ks == "ordinary-failed-maintain") { outcome("retry"); yield break; }
                if (ks != "held") { problems.Add("crab.siblingRound: " + ks); outcome("fail"); yield break; }
                ExpectPerLinkRound(c, s1, new[] { x2 }, new[] { p2 }, "crab.siblingRound", death);
                ExpectMultiHeld(c, x2, p2, "crab.siblingRound.after");
                if (Events(s0, e => PairOf(e, c.H, x1) && (Is(e, "DealConstrict") || Is(e, "MaintainLink"))).Count != 0) problems.Add("crab.targetDeath: a maintain or constrict was attributed to the ended link");
            }

            // ---- reuse the freed pincer on a replacement target
            UnitEntityData x3 = SpawnTarget(Around(c.H.Position, 180f, 1.1f));
            Buff unrelated3 = null;
            {
                var reuse = new JObject();
                row["reuse"] = reuse;
                if (x3 == null) { problems.Add("crab.reuse: target not created"); outcome("fail"); yield break; }
                yield return null;
                c.T.Add(x3);
                int s0 = LastSeq;
                foreach (object step in AttackUntil("crab.reuse" + attempt, c.H, x3, p1, () => HeldBy(c, x3))) yield return step;
                if (!HeldBy(c, x3)) { outcome("fail"); yield break; }
                unrelated3 = AddUnrelated(x3);
                reuse["after"] = MultiState(c);
                ExpectMultiHeld(c, x3, p1, "crab.reuse.new");
                if (!HeldBy(c, x2)) { string k = Classify(s0, c.H, x2); if (k == "ordinary-failed-maintain") { outcome("retry"); yield break; } }
                ExpectMultiHeld(c, x2, p2, "crab.reuse.sibling");
                IList<string> raw = RawLinks(c.H);
                reuse["rawRecordForDeadTargetAfterReuse"] = raw == null ? 0 : raw.Count(l => l.StartsWith(x1.UniqueId + "|", StringComparison.Ordinal)); // captured
            }

            // ---- holder-wide native terminal with both links active immediately before: native summon expiry
            Control[] controls2 = null;
            foreach (object step in CrabControls("crab.holderExpiry" + attempt, x => controls2 = x)) yield return step;
            if (controls2 == null) { outcome("fail"); yield break; }
            {
                var expiry = new JObject();
                row["holderExpiry"] = expiry;
                if (!HeldBy(c, x2) || !HeldBy(c, x3))
                {
                    string k = ordinary(new[] { x2, x3 }, c.Seq0);
                    if (k == "ordinary") { outcome("retry"); yield break; }
                    problems.Add("crab.holderExpiry: both links are not active immediately before: " + k); outcome("fail"); yield break;
                }
                expiry["activeImmediatelyBefore"] = MultiState(c);
                ExpectMultiHeld(c, x2, p2, "crab.holderExpiry.before");
                ExpectMultiHeld(c, x3, p1, "crab.holderExpiry.before");
                int s0 = LastSeq;
                double t0 = Now;
                int d2 = x2.Descriptor.Damage, d3 = x3.Descriptor.Damage;
                expiry["expiry"] = SetDeadline(c.H, TimeSpan.FromMilliseconds(-1));
                foreach (object step in SettleGameTime(0.0, 20f, () => c.H.Destroyed && HeldStates(c, x2).Length == 0 && HeldStates(c, x3).Length == 0)) yield return step;
                expiry["releaseGameSeconds"] = Now - t0;
                expiry["after"] = MultiState(c);
                if (!c.H.Destroyed) problems.Add("crab.holderExpiry: the crab was not destroyed");
                ExpectMultiEnded(c, x2, p2, "crab.holderExpiry");
                ExpectMultiEnded(c, x3, p1, "crab.holderExpiry");
                if (!Kept(x2, unrelated2) || !Kept(x3, unrelated3)) problems.Add("crab.holderExpiry: an unrelated effect was lost");
                Witness("crab.holderExpiry", controls2);
                foreach (object step in SettleGameTime(13.0, 40f, () => true)) yield return step;
                expiry["quiet.gameSeconds"] = Now - t0;
                expiry["quiet.state"] = MultiState(c);
                expiry["events"] = Events(s0, e => (string)e["owner"] == c.H.UniqueId || (Is(e, "rule.damage") && (string)e["initiator"] == c.H.UniqueId));
                if (Now - t0 < 12.0) problems.Add("crab.holderExpiry.quiet: fewer than two rounds");
                if (x2.Descriptor.Damage != d2 || x3.Descriptor.Damage != d3) problems.Add("crab.holderExpiry.quiet: a former link took damage");
                if (Events(s0, e => (Is(e, "DealConstrict") || Is(e, "MaintainLink")) && (string)e["owner"] == c.H.UniqueId).Count != 0) problems.Add("crab.holderExpiry.quiet: a maintain or constrict ran after the terminal");
                ExpectMultiEnded(c, x2, p2, "crab.holderExpiry.quiet");
                ExpectMultiEnded(c, x3, p1, "crab.holderExpiry.quiet");
                Witness("crab.holderExpiry.quiet", controls2);
            }
            outcome(problems.Count == problemsBefore ? "pass" : "fail");
        }

        // ================================================================ save / reload

        private JObject Snapshot(Link1 b, LinkN c)
        {
            JObject bs = SingleState(b), cs = MultiState(c);
            // Buff-name lists are compared as sets: the native condition presentation buff (EntangledBuff) is
            // re-applied by the save's owner off/on cycle and changes list order only (first run, retained).
            foreach (var key in new[] { "targetBuffs", "holderBuffs" }) bs[key] = new JArray(((JArray)bs[key]).Select(x => (string)x).OrderBy(x => x, StringComparer.Ordinal).Cast<object>().ToArray());
            foreach (JObject t in ((JArray)cs["targets"]).OfType<JObject>()) t["buffs"] = new JArray(((JArray)t["buffs"]).Select(x => (string)x).OrderBy(x => x, StringComparer.Ordinal).Cast<object>().ToArray());
            return new JObject { ["barbed"] = bs, ["crab"] = cs };
        }

        private IEnumerable<object> Save()
        {
            var cases = new JObject();
            result["cases"] = cases;
            Link1 b = null;
            LinkN c = null;
            var attempts = new JArray();
            result["attempts.save"] = attempts;
            for (int attempt = 1; attempt <= 2 && c == null; attempt++)
            {
                Link1 bb = null; LinkN cc = null;
                foreach (object step in EstablishBarbed("saveBarbed" + attempt, x => bb = x)) yield return step;
                foreach (object step in EstablishCrab("saveCrab" + attempt, 2, x => cc = x)) yield return step;
                if (bb == null || cc == null) yield break;
                // Age every link through one decisive maintain round (Impale for the Barbed, maintain + constrict per pincer).
                int s0 = LastSeq;
                foreach (object step in SettleGameTime(0.0, 40f, () => new[] { DecisiveSince(s0, bb.H, bb.T).Length, DecisiveSince(s0, cc.H, cc.T[0]).Length, DecisiveSince(s0, cc.H, cc.T[1]).Length }.All(n => n > 0))) yield return step;
                foreach (object step in SettleGameTime(0.3, 5f, () => true)) yield return step;
                string[] kinds = { Classify(s0, bb.H, bb.T), Classify(s0, cc.H, cc.T[0]), Classify(s0, cc.H, cc.T[1]) };
                var row = new JObject { ["attempt"] = attempt, ["classification"] = new JArray(kinds.Cast<object>().ToArray()),
                    ["barbedFirst"] = DecisiveSince(s0, bb.H, bb.T).FirstOrDefault()?["result"], ["crabFirst"] = new JArray(cc.T.Select(t => (object)DecisiveSince(s0, cc.H, t).FirstOrDefault()?["result"]).ToArray()) };
                attempts.Add(row);
                bool impaled = DecisiveSince(s0, bb.H, bb.T).Any(e => ((string)e["result"]).StartsWith("impale:", StringComparison.Ordinal));
                if (kinds.All(k => k == "held") && impaled) { b = bb; c = cc; break; }
                if (kinds.All(k => k == "held" || k == "ordinary-failed-maintain")) continue;
                problems.Add("save: setup round ended with " + string.Join(",", kinds)); yield break;
            }
            if (c == null) { problems.Add("save: setup retry budget exhausted"); yield break; }
            Buff ub = AddUnrelated(b.T), ua = AddUnrelated(c.T[0]), uc = AddUnrelated(c.T[1]);
            Game.Instance.IsPaused = true;
            yield return null;
            ExpectSingleHeld(b, b.T, "save.before");
            ExpectMultiHeld(c, c.T[0], c.Weapons[0], "save.before");
            ExpectMultiHeld(c, c.T[1], c.Weapons[1], "save.before");
            bool allowed = Game.Instance.SaveManager.IsSaveAllowed();
            JObject before = Snapshot(b, c);
            JArray orderBefore = new JArray(c.T.Select(t => (object)Buffs(t)).ToArray());
            cases["beforeSave"] = new JObject { ["saveAllowed"] = allowed, ["snapshot"] = before };
            if (!allowed) { problems.Add("save: native save admission refused"); yield break; }
            UnitEntityData[] named = { b.H, b.T, c.H, c.T[0], c.T[1] };
            for (int k = 0; k < named.Length; k++) { named[k].Descriptor.CustomName = SaveNames[k]; created.Remove(named[k]); awakeAdded.Remove(named[k]); }
            int seq = LastSeq;
            TimeSpan clock = Game.Instance.TimeController.GameTime;
            bool saved = false;
            Game.Instance.SaveGame(Game.Instance.SaveManager.CreateNewSave("ver02g12"), () => saved = true);
            float until = Time.realtimeSinceStartup + 120f;
            while ((!saved || Game.Instance.SaveManager.CommitInProgress || LoadingProcess.Instance.IsLoadingInProcess) && Time.realtimeSinceStartup < until)
                yield return null;
            bool finished = saved && !Game.Instance.SaveManager.CommitInProgress && !LoadingProcess.Instance.IsLoadingInProcess;
            JObject after = Snapshot(b, c);
            JArray during = Events(seq, e => true);
            cases["buffOrderCaptured"] = new JObject { ["before"] = orderBefore, ["after"] = new JArray(c.T.Select(t => (object)Buffs(t)).ToArray()) };
            cases["afterSave"] = new JObject { ["callbackReceived"] = saved, ["commitInProgress"] = Game.Instance.SaveManager.CommitInProgress,
                ["loadingInProgress"] = LoadingProcess.Instance.IsLoadingInProcess, ["finished"] = finished,
                ["gameClockUnchanged"] = Game.Instance.TimeController.GameTime == clock, ["snapshot"] = after, ["eventsDuringSave"] = during,
                ["unchanged"] = JToken.DeepEquals(before, after) };
            if (!finished) { problems.Add("save: the save did not finish"); yield break; }
            if (!JToken.DeepEquals(before, after)) problems.Add("save: the active ownership changed across the save");
            if (during.OfType<JObject>().Any(e => Is(e, "MaintainLink") || Is(e, "DealConstrict") || Is(e, "rule.damage") || Is(e, "SummonHoldComponent.ReleaseLink") || Is(e, "SummonHoldComponent.Release")))
                problems.Add("save: a maintain, rider, damage or release ran during the save");
            ExpectSingleHeld(b, b.T, "save.after");
            ExpectMultiHeld(c, c.T[0], c.Weapons[0], "save.after");
            ExpectMultiHeld(c, c.T[1], c.Weapons[1], "save.after");

            // Normal continuation: the next round gives each link exactly one maintain, no duplicate rider, no unexplained release.
            int s1 = LastSeq;
            var cont = new JObject();
            cases["continuation"] = cont;
            foreach (object step in SettleGameTime(0.0, 40f, () => DecisiveSince(s1, b.H, b.T).Length > 0 && DecisiveSince(s1, c.H, c.T[0]).Length > 0 && DecisiveSince(s1, c.H, c.T[1]).Length > 0)) yield return step;
            foreach (object step in SettleGameTime(0.3, 5f, () => true)) yield return step;
            var pairs = new[] { new { H = b.H, T = b.T, Label = "barbed" }, new { H = c.H, T = c.T[0], Label = "crabA" }, new { H = c.H, T = c.T[1], Label = "crabB" } };
            foreach (var p in pairs)
            {
                JObject[] decisive = DecisiveSince(s1, p.H, p.T);
                string kind = Classify(s1, p.H, p.T);
                int riders = p.Label == "barbed" ? Events(s1, e => Is(e, "rule.damage") && (string)e["initiator"] == p.H.UniqueId && (string)e["target"] == p.T.UniqueId && !(bool)e["attackRoll"] && (string)e["weapon"] == null).Count
                    : Events(s1, e => Is(e, "DealConstrict") && PairOf(e, p.H, p.T)).Count;
                cont[p.Label] = new JObject { ["decisive"] = new JArray(decisive.Cast<object>().ToArray()), ["classification"] = kind, ["riders"] = riders };
                if (decisive.Length != 1) problems.Add("save.continuation." + p.Label + ": " + decisive.Length + " decisive maintains in the first round after the save");
                if (kind == "ordinary-failed-maintain") continue; // an exactly attributed ordinary release is not premature; recorded
                if (kind != "held") problems.Add("save.continuation." + p.Label + ": " + kind);
                else if (riders != 1) problems.Add("save.continuation." + p.Label + ": " + riders + " rider damage events, expected one");
            }
            if (!Kept(b.T, ub) || !Kept(c.T[0], ua) || !Kept(c.T[1], uc)) problems.Add("save.continuation: an unrelated effect was lost");
            created.AddRange(named);
        }

        private IEnumerable<object> Reload()
        {
            UnitEntityData[] units = SaveNames.Select(n => Game.Instance.State.Units.FirstOrDefault(x => x.Descriptor.CustomName == n)).ToArray();
            result["found"] = new JArray(units.Select(x => (object)(x != null)).ToArray());
            if (units.Any(x => x == null)) { problems.Add("reload: a named participant is missing"); yield break; }
            created.AddRange(units);
            foreach (UnitEntityData x in units) { rules.Tracked.Add(x); Wake(x); }
            UnitEntityData bh = units[0], bt = units[1], ch = units[2], xa = units[3], xb = units[4];
            object bg = Grab(bh), cg = Grab(ch);
            var b = new Link1 { Name = "reloadBarbed", H = bh, T = bt, Grab = bg, Hold = HoldBuffOf(bg), Held = HeldBuffOf(bg), Weapons = GrabWeapons(bh, bg) };
            var c = new LinkN { Name = "reloadCrab", H = ch, Grab = cg, Hold = HoldBuffOf(cg), Held = HeldBuffOf(cg), Weapons = GrabWeapons(ch, cg) };
            c.T.Add(xa); c.T.Add(xb);
            var row = new JObject();
            result["reload"] = row;
            row["loadEvents"] = Events(0, e => !((string)e["what"]).StartsWith("stirge.", StringComparison.Ordinal));
            row["atLoad"] = new JObject { ["barbed"] = SingleState(b), ["crab"] = MultiState(c) };
            ExpectReset(b, c, "reload.atLoad");
            // Disclosed: brains off again, so nothing re-attacks during the quiet interval.
            foreach (UnitEntityData x in units) runner.GetMethod("SetExpandedSummoningBrainActive", Any).Invoke(null, new object[] { x, false });
            int s0 = LastSeq;
            int[] damage = { bt.Descriptor.Damage, xa.Descriptor.Damage, xb.Descriptor.Damage };
            double t0 = Now;
            foreach (object step in SettleGameTime(13.0, 40f, () => true)) yield return step;
            row["quiet.gameSeconds"] = Now - t0;
            row["quiet"] = new JObject { ["barbed"] = SingleState(b), ["crab"] = MultiState(c) };
            row["quiet.events"] = Events(s0, e => true);
            if (Now - t0 < 12.0) problems.Add("reload.quiet: fewer than two rounds");
            if (bt.Descriptor.Damage != damage[0] || xa.Descriptor.Damage != damage[1] || xb.Descriptor.Damage != damage[2]) problems.Add("reload.quiet: a former link took damage");
            if (((JArray)row["quiet.events"]).OfType<JObject>().Any(e => Is(e, "MaintainLink") || Is(e, "DealConstrict") || (Is(e, "rule.damage") && ((string)e["initiator"] == bh.UniqueId || (string)e["initiator"] == ch.UniqueId))))
                problems.Add("reload.quiet: a maintain, rider or holder damage ran after the load");
            ExpectReset(b, c, "reload.quiet");
            foreach (UnitEntityData t in new[] { bt, xa, xb })
                if (!t.Descriptor.Buffs.RawFacts.OfType<Buff>().Any(x => x.Blueprint.AssetGuid == UnrelatedBuffGuid)) problems.Add("reload: the unrelated effect on " + t.Descriptor.CustomName + " was lost");
        }

        private void ExpectReset(Link1 b, LinkN c, string name)
        {
            JObject bs = SingleState(b);
            if ((bool)bs["initiatorPart"] || (bool)bs["targetPart"] || (int)bs["holdBuffs"] != 0 || (int)bs["heldBuffs"] != 0) problems.Add(name + ": the Barbed relationship was not reset: " + bs.ToString(Formatting.None));
            if (bs["raw.links"] is JArray && ((JArray)bs["raw.links"]).Count != 0) problems.Add(name + ": Barbed raw link records remain");
            if (((JArray)bs["live.reconciled"]).Count != 0 || ((JArray)bs["live.occupancy"]).Any(o => (string)o["occupant"] != null)) problems.Add(name + ": a Barbed live link or occupied claw remains");
            if ((bool)bs["holderCantAct"] || (bool)bs["holderCantMove"]) problems.Add(name + ": the Barbed Devil keeps grapple conditions");
            if (((JArray)bs["targetFear"]).Count == 0 && (!(bool)bs["targetCanMove"] || !(bool)bs["targetCanAct"])) problems.Add(name + ": the Barbed target cannot move or act without any fear effect");
            JObject cs = MultiState(c);
            JObject h = (JObject)cs["holder"];
            if ((int)h["holdBuffs"] != 0) problems.Add(name + ": the crab keeps a multi-hold buff");
            if (h["raw.links"] is JArray && ((JArray)h["raw.links"]).Count != 0) problems.Add(name + ": crab raw link records remain: " + h["raw.links"]);
            if (((JArray)h["live.reconciled"]).Count != 0 || ((JArray)h["live.occupancy"]).Any(o => (string)o["occupant"] != null)) problems.Add(name + ": a crab live link or occupied pincer remains");
            foreach (JObject t in ((JArray)cs["targets"]).OfType<JObject>())
                if (((JArray)t["heldStates"]).Count != 0 || (bool)t["entangled"] || (bool)t["cantMove"] || (bool)t["cantAct"] || (bool)t["nativeTargetPart"])
                    problems.Add(name + ": crab target " + t["id"] + " is not reset: " + t.ToString(Formatting.None));
        }
    }
}
