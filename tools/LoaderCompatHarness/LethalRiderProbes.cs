using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KmgLoaderCompatHarness
{
    /// <summary>
    /// Sprint 6 VER-02 G1-R: five selected lethal-terminal damage owners on four single-link holders, qualified under the
    /// reviewed native life-update contract (2026-11-23 review of PR #212).
    ///
    /// Contract: the synchronous maintain/rider may finish while the native life state is pending; once the engine
    /// establishes death, the exact relationship must clean up through the normal native/product lifecycle, with no later
    /// damaging round from the ended relationship, no recreated hold, no loss of unrelated ownership and no stuck holder.
    /// Rider work between the threshold crossing and native death (for example constrict or prone) is retained behavior
    /// and is captured, never presented as a synchronous release.
    ///
    /// A  Constrictor Snake: maintained weapon damage crosses the native death threshold.
    /// B  Constrictor Snake: maintained weapon damage is survived; the physical constrict crosses it.
    /// C  Salamander: maintained weapon damage is survived; its constrict crosses it.
    /// D  Crocodile: a later-round death roll crosses it.
    /// E  Barbed Devil: a later-round Impale crosses it.
    ///
    /// Fixture: disposable native hostile NPC targets with their own blueprint faction and death policy (no player-faction
    /// unit, no difficulty change, no forced combat or death, no released survival flags); each spawned target is verified
    /// to have no Ferocity, regeneration, dying allowance, immortality or Cheat Death component. Ordinary RTWP combat
    /// follows naturally. Disclosed: target HP base 100000 then Damage = HP − margin before the decisive round (margin 1
    /// for A/D/E; the establishing weapon's maximum + 1 for B/C, read-only RuleCalculateWeaponStats), target escape stats
    /// −100, no XP award on death, brains off (IsBrainActive only, for the main character and every pre-existing living
    /// area NPC with an active brain, restored at teardown; disposable targets and holders use the runner helper), holder BAB
    /// 100, summon deadlines 600 s, placement within 10.5 m.
    ///
    /// Cleanup deadline (chosen before the run, from the native sequence: life tick → UnitGrappleController removes the
    /// initiator on a non-conscious target → hold buff removal): every grapple-owned element gone within 10 engine frames
    /// and 1.0 s of game time after the native Dead transition.
    /// </summary>
    internal sealed partial class BarbedCrabProbe
    {
        private const int CleanupFrames = 10;
        private const double CleanupGameSeconds = 1.0;

        private sealed class LethalSpec
        {
            internal string Id, Key, FirstRider, Begin, End, Path;
            internal bool ConstrictLethal;
        }

        private static readonly LethalSpec[] LethalSpecs =
        {
            new LethalSpec { Id = "A", Key = "constrictor-snake", FirstRider = "maintained:", Begin = "rider.weapon.begin", End = "rider.weapon.end", Path = "maintained weapon damage (MaintainLink)" },
            new LethalSpec { Id = "B", Key = "constrictor-snake", FirstRider = "maintained:", Begin = "DealConstrict.begin", End = "DealConstrict", Path = "physical constrict (DealConstrict, shared with Lillend)", ConstrictLethal = true },
            new LethalSpec { Id = "C", Key = "salamander", FirstRider = "maintained:", Begin = "rider.salamanderConstrict.begin", End = "rider.salamanderConstrict.end", Path = "Salamander constrict (SalamanderConstrictDamage)", ConstrictLethal = true },
            new LethalSpec { Id = "D", Key = "crocodile", FirstRider = "death-roll:", Begin = "rider.deathRoll.begin", End = "rider.deathRoll.end", Path = "death roll (DealDeathRoll)" },
            new LethalSpec { Id = "E", Key = "barbed-devil", FirstRider = "impale:", Begin = "rider.impale.begin", End = "rider.impale.end", Path = "Impale (DealImpale)" },
        };

        private BlueprintUnit hostileBlueprint;
        private List<UnitEntityData> attemptUnits;
        private List<Vector3> attemptSpots;

        /// <summary>Retires everything one attempt created, on any exit path (fixture hygiene, not a terminal).</summary>
        private void RetireAttempt()
        {
            if (attemptUnits != null) foreach (UnitEntityData u in attemptUnits) if (u != null && !u.Destroyed) u.Destroy();
            if (attemptSpots != null) foreach (Vector3 s in attemptSpots) spots.RemoveAll(x => Vector3.Distance(x, s) < 0.5f);
            attemptUnits = null; attemptSpots = null;
        }

        /// <summary>A bounded wait with its own record: game time, frames, wall time, pause/re-pause and combat state.</summary>
        private IEnumerable<object> Wait(JObject row, string name, double gameSeconds, float wall, Func<bool> done)
        {
            var stats = new JObject { ["gameStart"] = Now, ["inCombatStart"] = Game.Instance.Player.IsInCombat, ["timeScaleStart"] = Time.timeScale };
            int f0 = Time.frameCount, repaused = 0;
            float w0 = Time.realtimeSinceStartup;
            TimeSpan start = Game.Instance.TimeController.GameTime;
            bool met = false;
            while (Time.realtimeSinceStartup < w0 + wall)
            {
                if (Game.Instance.IsPaused) { repaused++; Game.Instance.IsPaused = false; }
                if ((Game.Instance.TimeController.GameTime - start).TotalSeconds >= gameSeconds && done()) { met = true; break; }
                yield return null;
            }
            Game.Instance.IsPaused = true;
            stats["met"] = met; stats["gameElapsed"] = Now - (double)stats["gameStart"]; stats["frames"] = Time.frameCount - f0;
            stats["wallElapsed"] = Time.realtimeSinceStartup - w0; stats["repausedFrames"] = repaused;
            stats["inCombatEnd"] = Game.Instance.Player.IsInCombat; stats["timeScaleEnd"] = Time.timeScale;
            var waits = (JObject)(row["waits"] ?? (row["waits"] = new JObject()));
            waits[name] = stats;
        }
        private UnitEntityData brainOffMain;
        private bool mainBrainWas;
        private readonly List<UnitEntityData> brainOffBystanders = new List<UnitEntityData>();

        private IEnumerable<object> Lethal(string match)
        {
            string[] ids = match.Contains(":") ? match.Substring(match.IndexOf(':') + 1).Split(',') : LethalSpecs.Select(s => s.Id).ToArray();
            result["contract"] = "native life-update timing (review of PR #212): synchronous rider may finish before native death; exact cleanup within " +
                CleanupFrames + " frames / " + CleanupGameSeconds + " s of native Dead; no activity after native death";
            targetFactory = SpawnHostile;
            // Disclosed: the main character's brain off for the launch (no unrelated party attack), restored at teardown.
            UnitEntityData main = Game.Instance.Player.MainCharacter.Value;
            mainBrainWas = main.IsBrainActive;
            SetBrainOnly(main, false);
            brainOffMain = main;
            main.Commands.InterruptAll();
            // Disclosed: pre-existing living area NPCs with active brains (the fixture holds companion "_Neutral" variants
            // that join combat against the hostile targets; session 20261123T162428Z) have IsBrainActive cleared for the
            // launch and restored at teardown. No faction, combat, death or difficulty state changes.
            var bystanders = new JArray();
            foreach (UnitEntityData u in Game.Instance.State.Units.All.ToArray())
            {
                if (u == null || u.Descriptor == null || ReferenceEquals(u, main) || created.Contains(u) || !u.IsInGame || u.Descriptor.State.IsDead || !u.IsBrainActive) continue;
                SetBrainOnly(u, false);
                u.Commands.InterruptAll();
                brainOffBystanders.Add(u);
                bystanders.Add(new JObject { ["id"] = u.UniqueId, ["blueprint"] = u.Blueprint.name, ["name"] = u.CharacterName, ["faction"] = u.Descriptor.Faction?.name,
                    ["inParty"] = Game.Instance.Player.Party.Contains(u) });
            }
            result["bystanderBrainsOff"] = bystanders;
            var summary = new JObject();
            result["lethal.summary"] = summary;
            try
            {
                foreach (LethalSpec spec in LethalSpecs.Where(s => ids.Contains(s.Id)))
                {
                    var attempts = new JArray();
                    result["lethal." + spec.Id + ".attempts"] = attempts;
                    string outcome = null;
                    for (int attempt = 1; attempt <= 3; attempt++)
                    {
                        int before = problems.Count;
                        var row = new JObject { ["case"] = spec.Id, ["attempt"] = attempt, ["creature"] = spec.Key, ["damageOwner"] = spec.Path };
                        string o = null;
                        attemptUnits = new List<UnitEntityData>(); attemptSpots = new List<Vector3>();
                        foreach (object step in LethalAttempt(spec, attempt, row, x => o = x)) yield return step;
                        RetireAttempt();
                        if (o == "pass" && problems.Count > before) o = "fail";
                        if (o == null) o = "fail";
                        row["outcome"] = o;
                        row["problems"] = new JArray(problems.Skip(before).Cast<object>().ToArray());
                        row["observerDropped"] = HoldLifecycleObserver.Dropped;
                        attempts.Add(row);   // completed before its single insertion
                        outcome = o;
                        if (o != "retry") break;
                    }
                    if (outcome == "retry") problems.Add("lethal." + spec.Id + ": bounded fresh-setup budget exhausted");
                    summary[spec.Id] = outcome;
                }
            }
            finally { RestoreMainBrain(); RetireAttempt(); }
            if (HoldLifecycleObserver.Dropped > 0) problems.Add("observer: " + HoldLifecycleObserver.Dropped + " events dropped at the cap; evidence is truncated");
        }

        private static void SetBrainOnly(UnitEntityData unit, bool active)
        {
            var setter = typeof(UnitEntityData).GetProperty("IsBrainActive", Any)?.GetSetMethod(true);
            if (setter != null) setter.Invoke(unit, new object[] { active });
        }

        private void RestoreMainBrain()
        {
            foreach (UnitEntityData u in brainOffBystanders) if (u != null && !u.Destroyed) SetBrainOnly(u, true);
            brainOffBystanders.Clear();
            if (brainOffMain == null) return;
            SetBrainOnly(brainOffMain, mainBrainWas);
            brainOffMain = null;
        }

        /// <summary>Native hostile survival rules that would change the selected death semantics; any one rejects the unit.</summary>
        private JObject SurvivalRules(UnitEntityData unit)
        {
            UnitState st = unit.Descriptor.State;
            Type cheat = product.GetType("KingmakerGunslinger.Deeds.CheatDeathDamageHandler", true);
            int cheatComponents = unit.Descriptor.Progression.Features.Enumerable.Count(f => (f.Blueprint.ComponentsArray ?? new BlueprintComponent[0]).Any(c => c.GetType() == cheat));
            return new JObject
            {
                ["ferocity"] = (bool)st.Features.Ferocity, ["halfOrcFerocity"] = (bool)st.Features.HalfOrcFerocity,
                ["regenerate"] = (bool)st.IsRegenerate, ["allowDying"] = (bool)st.AllowDyingCondition, ["immortality"] = (bool)st.Immortality,
                ["cheatDeathComponents"] = cheatComponents,
                ["playerFactionBlueprint"] = ReferenceEquals(unit.Blueprint.Faction, Kingmaker.Blueprints.Root.BlueprintRoot.Instance.PlayerFaction),
                ["enemyOfMain"] = unit.IsEnemy(Game.Instance.Player.MainCharacter.Value)
            };
        }

        private static bool Admissible(JObject r) => !(bool)r["ferocity"] && !(bool)r["halfOrcFerocity"] && !(bool)r["regenerate"] && !(bool)r["allowDying"] &&
            !(bool)r["immortality"] && (int)r["cheatDeathComponents"] == 0 && !(bool)r["playerFactionBlueprint"] && (bool)r["enemyOfMain"];

        /// <summary>A disposable native hostile NPC with its own faction and death policy, verified per spawned unit.</summary>
        private UnitEntityData SpawnHostile(Vector3 at)
        {
            UnitEntityData main = Game.Instance.Player.MainCharacter.Value;
            // The native blueprint can randomize race per spawned unit (a half-orc instance carries Ferocity), so each
            // candidate gets several spawns, each verified; other candidates stay as a fallback.
            IEnumerable<BlueprintUnit> pool = ResourcesLibrary.LibraryObject.GetAllBlueprints().OfType<BlueprintUnit>()
                .Where(u => u.Faction != null && !u.Faction.Peaceful && u.Faction.AttackFactions != null && u.Faction.AttackFactions.Contains(main.Blueprint.Faction) &&
                    u.Size == Kingmaker.Enums.Size.Medium && !u.name.StartsWith("KMG_", StringComparison.Ordinal))
                .OrderBy(u => u.name.IndexOf("Bandit", StringComparison.OrdinalIgnoreCase) >= 0 ? 0 : 1).ThenBy(u => u.AssetGuid).Take(25).ToArray();
            IEnumerable<BlueprintUnit> candidates = hostileBlueprint == null ? pool : new[] { hostileBlueprint }.Concat(pool.Where(u => !ReferenceEquals(u, hostileBlueprint)));
            var rejected = (JArray)(result["hostileRejected"] ?? (result["hostileRejected"] = new JArray()));
            foreach (BlueprintUnit candidate in candidates)
            for (int spawn = 0; spawn < 6; spawn++)
            {
                UnitEntityData unit = Game.Instance.EntityCreator.SpawnUnit(candidate, at, Quaternion.identity, Game.Instance.State.LoadedAreaState.MainState);
                if (unit == null) break;
                Game.Instance.EntityCreator.Tick();
                JObject survival = SurvivalRules(unit);
                survival["unitSize"] = unit.Descriptor.State.Size.ToString();
                if (!Admissible(survival)) { if (rejected.Count < 40) rejected.Add(candidate.name + ":" + survival.ToString(Formatting.None)); unit.Destroy(); continue; }
                hostileBlueprint = candidate;
                if (result["hostile"] == null)
                    result["hostile"] = new JObject { ["blueprint"] = candidate.name, ["guid"] = candidate.AssetGuid, ["faction"] = candidate.Faction.name,
                        ["size"] = unit.Descriptor.State.Size.ToString(), ["survivalRules"] = survival };
                created.Add(unit);
                attemptUnits?.Add(unit);
                rules.Tracked.Add(unit);
                ((JArray)(result["hostileSpawns"] ?? (result["hostileSpawns"] = new JArray()))).Add(candidate.name + ":" + unit.Descriptor.State.Size + ":" + unit.Descriptor.Progression.Race?.name);
                var stats = unit.Descriptor.Stats;
                stats.HitPoints.BaseValue = 100000;
                stats.AdditionalCMB.BaseValue = -100;
                stats.SkillMobility.BaseValue = -100;
                stats.SkillAthletics.BaseValue = -100;
                unit.GiveExperienceOnDeath = false;
                runner.GetMethod("SetExpandedSummoningBrainActive", Any).Invoke(null, new object[] { unit, false });
                Wake(unit);
                return unit;
            }
            problems.Add("fixture: no admissible native hostile target");
            return null;
        }

        private int WeaponMax(UnitEntityData holder, ItemEntityWeapon weapon)
        {
            var stats = Rulebook.Trigger(new RuleCalculateWeaponStats(holder, weapon, null));
            int max = 0;
            if (stats.DamageDescription != null)
                foreach (DamageDescription d in stats.DamageDescription)
                    if (d != null) max += d.Dice.Rolls * (int)d.Dice.Dice + d.Bonus;
            return max;
        }

        private void RetireUnits(Vector3 spot, params UnitEntityData[] units)
        {
            spots.RemoveAll(s => Vector3.Distance(s, spot) < 0.5f);
            foreach (UnitEntityData u in units) if (u != null && !u.Destroyed) u.Destroy();
        }

        private IEnumerable<object> SingleControls(string key, string name, Action<Control[]> done)
        {
            var list = new List<Control>();
            for (int k = 0; k < 2; k++)
            {
                Link1 s = null;
                foreach (object step in EstablishSingle(key, name + ".control" + (k + 1), x => s = x)) yield return step;
                if (s == null) yield break;
                Link1 c = s;
                list.Add(new Control { Name = s.Name, H = s.H, T = s.T, Unrelated = s.Unrelated, Seq0 = s.Seq0, Held = () => HeldSingle(c), Spot = s.Spot });
            }
            done(list.ToArray());
        }

        private static JObject Bracket(JObject[] w, string begin, string end, UnitEntityData h, UnitEntityData t, int inside)
        {
            JObject b = w.LastOrDefault(e => Is(e, begin) && PairOf(e, h, t) && (int)e["seq"] < inside);
            JObject en = b == null ? null : w.FirstOrDefault(e => Is(e, end) && PairOf(e, h, t) && (int)e["seq"] > (int)b["seq"]);
            return new JObject { ["begin"] = b, ["end"] = en, ["complete"] = b != null && en != null && (int)en["seq"] > inside };
        }

        private IEnumerable<object> LethalAttempt(LethalSpec spec, int attempt, JObject row, Action<string> outcome)
        {
            string at = "lethal." + spec.Id + attempt;
            Control[] controls = null;
            foreach (object step in SingleControls(spec.Key, at, x => controls = x)) yield return step;
            if (controls == null) { outcome("fail"); yield break; }
            Link1 m = null;
            foreach (object step in EstablishSingle(spec.Key, at + ".main", x => m = x)) yield return step;
            if (m == null) { outcome("fail"); Retire(controls); yield break; }
            UnitEntityData h = m.H, t = m.T;
            HoldLifecycleObserver.WatchedDamage.Add(t.Descriptor);
            row["holder"] = h.UniqueId; row["target"] = t.UniqueId; row["holderBlueprint"] = h.Blueprint.name; row["targetBlueprint"] = t.Blueprint.name;
            row["targetSurvivalRules"] = SurvivalRules(t);
            row["grabLimbs"] = new JArray(m.Weapons.Select(x => (object)(x.Blueprint.name + "@" + HoldLifecycleObserver.LimbOf(h, x))).ToArray());
            row["establishingAttack"] = result["grab." + at + ".main"];
            if (!Admissible((JObject)row["targetSurvivalRules"])) { problems.Add(at + ": the target has a native survival rule"); outcome("fail"); yield break; }
            if (t.Descriptor.State.IsDead || !t.Descriptor.State.IsConscious) { problems.Add(at + ": the target did not survive the establishing attack"); outcome("fail"); yield break; }

            // ---- one ordinary rider round: the rider is eligible and the target survives it
            int s1 = LastSeq;
            foreach (object step in Wait(row, "ordinaryRound", 0.0, 90f, () => DecisiveSince(s1, h, t).Length > 0 || !HeldSingle(m))) yield return step;
            foreach (object step in SettleGameTime(0.3, 5f, () => true)) yield return step;
            JObject[] first = DecisiveSince(s1, h, t);
            string k1 = Classify(s1, h, t);
            row["firstRound"] = new JObject { ["classification"] = k1, ["result"] = first.Length == 0 ? null : first[0]["result"], ["events"] = PairEvents(s1, h, t) };
            if (k1 == "ordinary-failed-maintain") { Retire(controls); RetireUnits(m.Spot, h, t); outcome("retry"); yield break; }
            if (first.Length == 0) { problems.Add(at + ": postcondition failed: no decisive ordinary rider round within the wait (" + k1 + ")"); outcome("fail"); yield break; }
            if (k1 != "held") { problems.Add(at + ": the ordinary rider round ended the hold (" + k1 + ")"); outcome("fail"); yield break; }
            if (!((string)first[0]["result"]).StartsWith(spec.FirstRider, StringComparison.Ordinal)) { problems.Add(at + ": the ordinary round's rider is " + first[0]["result"] + ", not " + spec.FirstRider); outcome("fail"); yield break; }
            if (t.Descriptor.State.IsDead || !t.Descriptor.State.IsConscious) { problems.Add(at + ": the target did not survive the ordinary round"); outcome("fail"); yield break; }

            // ---- disclosed health setting, then the pre-decisive state
            int hp = (int)t.Descriptor.Stats.HitPoints;
            int weaponMax = WeaponMax(h, m.Weapons[0]);
            int margin = spec.ConstrictLethal ? weaponMax + 1 : 1;
            row["fixture"] = new JObject { ["targetHP"] = hp, ["damageBefore"] = t.Descriptor.Damage, ["nonLethal"] = t.Descriptor.DamageNonLethal, ["margin"] = margin,
                ["weaponMax"] = weaponMax, ["setDamage"] = hp - margin, ["threshold"] = "native: Dead at Damage + NonLethal >= HP (no dying allowance, Ferocity or regeneration; verified)" };
            t.Descriptor.Damage = hp - margin;
            yield return null; yield return null;
            JObject pre = SingleState(m);
            row["preDecisive"] = pre;
            if (t.Descriptor.State.IsDead || !t.Descriptor.State.IsConscious) { problems.Add(at + ": the target is not alive and conscious before the decisive round"); outcome("fail"); yield break; }
            if (h.Destroyed || h.Descriptor.State.IsDead || !h.Descriptor.State.IsConscious) { problems.Add(at + ": the holder is not alive and valid before the decisive round"); outcome("fail"); yield break; }
            ExpectSingleHeld(m, t, at + ".preDecisive");

            // ---- the decisive native round, the native Dead transition and the cleanup
            int s0 = LastSeq;
            foreach (object step in Wait(row, "decisiveRound", 0.0, 90f, () => DecisiveSince(s0, h, t).Length > 0 || !HeldSingle(m))) yield return step;
            if (DecisiveSince(s0, h, t).Length == 0)
            {
                row["noDecisiveWindow"] = Events(s0, e => PairOf(e, h, t) || (string)e["unit"] == t.UniqueId || (string)e["owner"] == h.UniqueId || (string)e["target"] == t.UniqueId ||
                    (string)e["initiator"] == t.UniqueId);
                row["noDecisiveState"] = SingleState(m);
                problems.Add(at + ": postcondition failed: no decisive maintain within the wait (" + Classify(s0, h, t) + ")"); outcome("fail"); yield break;
            }
            Func<JObject> deadEvent = () => HoldLifecycleObserver.Events.FirstOrDefault(e => (int)e["seq"] > s0 && Is(e, "life.set") && (string)e["unit"] == t.UniqueId && (string)e["to"] == "Dead");
            foreach (object step in Wait(row, "nativeDead", 0.0, 5f, () => deadEvent() != null || !HeldSingle(m) && t.Descriptor.State.IsConscious)) yield return step;
            int cleanFrame = -1; double cleanGame = -1;
            Func<bool> clean = () =>
            {
                bool c = h.Get<UnitPartGrappleInitiator>() == null && HoldInstance(m) == null && t.Get<UnitPartGrappleTarget>() == null &&
                    t.Descriptor.Buffs.RawFacts.OfType<Kingmaker.UnitLogic.Buffs.Buff>().All(b => !ReferenceEquals(b.Blueprint, m.Held));
                if (c && cleanFrame < 0) { cleanFrame = Time.frameCount; cleanGame = Now; }
                return c;
            };
            foreach (object step in Wait(row, "cleanup", 0.0, 5f, clean)) yield return step;
            foreach (object step in SettleGameTime(0.3, 5f, () => true)) yield return step;

            JArray window = Events(s0, e => PairOf(e, h, t) || ((string)e["unit"] == t.UniqueId && ((string)e["what"]).StartsWith("life.", StringComparison.Ordinal)) ||
                (Is(e, "rule.damage") && (string)e["target"] == t.UniqueId) || (Is(e, "rule.maneuver") && (string)e["initiator"] == h.UniqueId && (string)e["target"] == t.UniqueId));
            row["decisiveWindow"] = window;
            JObject[] w = window.OfType<JObject>().ToArray();
            JObject decisive = w.FirstOrDefault(e => Is(e, "MaintainLink") && Decisive(e));
            JObject crossing = w.FirstOrDefault(e => Is(e, "rule.damage") && (string)e["initiator"] == h.UniqueId && (int)e["targetDamageAfter"] >= (int)e["targetHP"]);
            JObject dead = deadEvent();
            JObject recovery = HoldLifecycleObserver.Events.FirstOrDefault(e => (int)e["seq"] > s0 && Is(e, "life.recovery") && (string)e["unit"] == t.UniqueId);
            row["decisive"] = decisive;
            row["crossing"] = crossing;
            row["nativeDead"] = dead;
            row["recovery"] = recovery;
            string k0 = Classify(s0, h, t);
            row["decisiveClassification"] = k0;
            JObject[] unrelated = w.Where(e => Is(e, "rule.damage") && (string)e["initiator"] != h.UniqueId).ToArray();
            if (unrelated.Length > 0)
            {
                // An unrelated attacker invalidates this setup; it is identified and the case gets a bounded fresh setup.
                var who = new JArray();
                foreach (string id in unrelated.Select(e => (string)e["initiator"]).Distinct())
                {
                    UnitEntityData u = Game.Instance.State.Units.All.FirstOrDefault(x => x.UniqueId == id);
                    who.Add(new JObject { ["id"] = id, ["blueprint"] = u?.Blueprint?.name, ["name"] = u?.CharacterName, ["faction"] = u?.Descriptor?.Faction?.name,
                        ["isMainCharacter"] = u != null && ReferenceEquals(u, Game.Instance.Player.MainCharacter.Value), ["inParty"] = u != null && Game.Instance.Player.Party.Contains(u),
                        ["brainActive"] = u != null && u.IsBrainActive });
                }
                row["unrelatedDamage"] = new JObject { ["events"] = new JArray(unrelated.Cast<object>().ToArray()), ["attackers"] = who };
                Retire(controls); RetireUnits(m.Spot, h, t); outcome("retry"); yield break;
            }

            if (crossing == null)
            {
                // Non-lethal decisive round: only a demonstrated, still-held expected execution may be retried (B/C).
                bool expected = decisive != null && ((string)decisive["result"]).StartsWith(spec.FirstRider, StringComparison.Ordinal) && k0 == "held" &&
                    HeldSingle(m) && w.Any(e => Is(e, spec.End) && PairOf(e, h, t));
                row["nonLethal"] = new JObject { ["expectedExecution"] = expected, ["classification"] = k0, ["stillHeld"] = HeldSingle(m) };
                if (spec.ConstrictLethal && expected) { Retire(controls); RetireUnits(m.Spot, h, t); outcome("retry"); yield break; }
                problems.Add(at + ": no damage rule from the holder crossed the native threshold (" + k0 + ", decisive " + decisive?["result"] + ")");
                outcome("fail"); yield break;
            }
            int crossSeq = (int)crossing["seq"];
            JObject bracket = Bracket(w, spec.Begin, spec.End, h, t, crossSeq);
            JObject weaponBracket = Bracket(w, "rider.weapon.begin", "rider.weapon.end", h, t, crossSeq);
            row["damageOwnerBrackets"] = new JObject { ["expected"] = bracket, ["weapon"] = weaponBracket };
            if (spec.ConstrictLethal && (bool)weaponBracket["complete"])
            {
                row["weaponLethal"] = "the maintained weapon damage crossed the threshold; this setup does not qualify constrict-lethal attribution";
                Retire(controls); RetireUnits(m.Spot, h, t); outcome("retry"); yield break;
            }
            if (!(bool)bracket["complete"]) problems.Add(at + ": the crossing is not inside a completed " + spec.Begin + "/" + spec.End + " bracket for the exact pair");
            if (spec.ConstrictLethal)
            {
                JObject wb = w.LastOrDefault(e => Is(e, "rider.weapon.begin") && PairOf(e, h, t) && (int)e["seq"] < crossSeq);
                JObject we = wb == null ? null : w.FirstOrDefault(e => Is(e, "rider.weapon.end") && PairOf(e, h, t) && (int)e["seq"] > (int)wb["seq"]);
                JObject wd = wb == null || we == null ? null : w.LastOrDefault(e => Is(e, "rule.damage") && (string)e["initiator"] == h.UniqueId && (int)e["seq"] > (int)wb["seq"] && (int)e["seq"] < (int)we["seq"]);
                row["survivedWeapon"] = wd;
                if (wd == null || we == null || (int)we["seq"] > crossSeq || (int)wd["targetDamageAfter"] >= (int)wd["targetHP"])
                    problems.Add(at + ": no completed, survived maintained weapon damage precedes the constrict crossing");
            }
            // Native death after the crossing, with no revival and no later damaging round substituted for the terminal.
            if (dead == null) { problems.Add(at + ": postcondition failed: no native Dead transition after the crossing"); outcome("fail"); yield break; }
            int deadSeq = (int)dead["seq"];
            if (deadSeq < crossSeq) problems.Add(at + ": the native Dead transition preceded the crossing");
            JObject laterRound = w.FirstOrDefault(e => Is(e, "rule.damage") && (int)e["seq"] > crossSeq && (int)e["seq"] < deadSeq && (int)e["frame"] > (int)crossing["frame"]);
            if (laterRound != null) problems.Add(at + ": a later damaging round preceded native death: " + laterRound.ToString(Formatting.None));
            if (recovery != null || w.Any(e => Is(e, "life.set") && (string)e["from"] == "Dead")) problems.Add(at + ": the target was revived");
            row["finalDeath"] = (bool)dead["finallyDead"] || t.Descriptor.State.IsFinallyDead;
            if (!(bool)row["finalDeath"]) problems.Add(at + ": the native death is not final");
            // Synchronous rider work between the crossing and native death: retained behavior, captured.
            row["synchronousContinuation"] = new JArray(w.Where(e => (int)e["seq"] > crossSeq && (int)e["seq"] < deadSeq &&
                (PairOf(e, h, t) || Is(e, "rule.damage"))).Cast<object>().ToArray());
            // Nothing from the ended relationship after native death.
            JObject[] afterDeath = HoldLifecycleObserver.Events.Where(e => (int)e["seq"] > deadSeq && ((Is(e, "rule.damage") && (string)e["initiator"] == h.UniqueId && (string)e["target"] == t.UniqueId) ||
                (PairOf(e, h, t) && (Is(e, "MaintainLink") || Is(e, "DealConstrict.begin") || ((string)e["what"]).StartsWith("rider.", StringComparison.Ordinal))))).ToArray();
            row["activityAfterDeath"] = new JArray(afterDeath.Cast<object>().ToArray());
            if (afterDeath.Length > 0) problems.Add(at + ": the ended relationship acted after native death");
            // Exact cleanup within the stated deadline.
            row["releasePath"] = new JArray(w.Where(e => (int)e["seq"] > crossSeq && (((string)e["what"]).StartsWith("SummonHoldComponent.", StringComparison.Ordinal) ||
                ((string)e["what"]).StartsWith("impaleCleanup.", StringComparison.Ordinal) || ((string)e["what"]).StartsWith("hold.", StringComparison.Ordinal))).Cast<object>().ToArray());
            row["cleanup"] = new JObject { ["deadFrame"] = dead["frame"], ["deadGameSeconds"] = dead["gameSeconds"], ["cleanFrame"] = cleanFrame, ["cleanGameSeconds"] = cleanGame,
                ["frames"] = cleanFrame < 0 ? (int?)null : cleanFrame - (int)dead["frame"], ["gameSeconds"] = cleanGame < 0 ? (double?)null : cleanGame - (double)dead["gameSeconds"],
                ["deadline"] = CleanupFrames + " frames / " + CleanupGameSeconds + " s" };
            if (cleanFrame < 0 || cleanFrame - (int)dead["frame"] > CleanupFrames || cleanGame - (double)dead["gameSeconds"] > CleanupGameSeconds)
                problems.Add(at + ": grapple-owned state was not clean within the cleanup deadline: " + row["cleanup"].ToString(Formatting.None));
            JObject after = SingleState(m);
            row["after"] = after;
            row["rawRecordsAfter"] = after["rawNamingTarget"]; // stored records: captured (accepted inert-residue policy)
            if ((bool)after["targetPart"] || (int)after["heldBuffs"] != 0) problems.Add(at + ": the dead target keeps grapple-owned state");
            if ((bool)after["initiatorPart"] || (int)after["holdBuffs"] != 0) problems.Add(at + ": the holder keeps its initiator part or hold buff");
            if (((JArray)after["live.reconciled"]).Count != 0 || (int)after["live.heldCount"] != 0 || ((JArray)after["live.occupancy"]).Any(o => (string)o["occupant"] != null))
                problems.Add(at + ": a live link or occupied limb remains");
            if (h.Destroyed || h.Descriptor.State.IsDead || !h.Descriptor.State.IsConscious || (bool)after["holderCantAct"] || (bool)after["holderCantMove"])
                problems.Add(at + ": the surviving holder is not usable");
            Witness(at + ".decisive", controls);
            if (HoldLifecycleObserver.Dropped > 0) problems.Add(at + ": observer evidence truncated");

            // ---- quiet: two native rounds, nothing more from the ended relationship
            int q0 = LastSeq;
            double t0 = Now;
            int dmg = t.Descriptor.Damage;
            foreach (object step in Wait(row, "quiet", 13.0, 90f, () => true)) yield return step;
            double quiet = Now - t0;
            row["quiet"] = new JObject { ["gameSeconds"] = quiet, ["events"] = PairEvents(q0, h, t), ["state"] = SingleState(m) };
            if (quiet < 12.0) problems.Add(at + ".quiet: fewer than two rounds of game time (" + quiet + ")");
            if (t.Descriptor.Damage != dmg || PairEvents(q0, h, t).OfType<JObject>().Any(e => Is(e, "rule.damage") || Is(e, "MaintainLink") || Is(e, "DealConstrict.begin")))
                problems.Add(at + ".quiet: damage or rider activity from the ended relationship");
            if (HeldSingle(h, t) || t.Get<UnitPartGrappleTarget>() != null) problems.Add(at + ".quiet: a live hold was recreated");
            if (!t.Descriptor.State.IsDead) problems.Add(at + ".quiet: the target is no longer dead");

            // ---- reacquire a fresh living target
            UnitEntityData t2 = SpawnHostile(Around(h.Position, 90f, 1.0f));
            if (t2 == null) { problems.Add(at + ": reacquire target not created"); outcome("fail"); yield break; }
            yield return null;
            int r0 = LastSeq;
            foreach (object step in AttackUntil(at + ".reacquire", h, t2, m.Weapons[0], () => HeldSingle(h, t2))) yield return step;
            if (HeldSingle(h, t2))
            {
                ExpectSingleHeld(m, t2, at + ".reacquire");
                int f0 = Time.frameCount;
                foreach (object step in SettleGameTime(0.0, 10f, () => Time.frameCount - f0 >= 30)) yield return step;
                int frames = Time.frameCount - f0;
                row["reacquire"] = new JObject { ["frames"] = frames, ["state"] = SingleState(m, t2), ["events"] = PairEvents(r0, h, t2) };
                if (frames < 30) problems.Add(at + ".reacquire: fewer than 30 engine frames observed");
                ExpectSingleHeld(m, t2, at + ".reacquire.afterFrames");
                if (PairEvents(r0, h, t2).OfType<JObject>().Any(e => Is(e, "SummonHoldComponent.ReleaseLink") || ((string)e["what"]).StartsWith("impaleCleanup.callback", StringComparison.Ordinal)))
                    problems.Add(at + ".reacquire: a release or deferred callback touched the new relationship");
            }
            Witness(at + ".quiet", controls);
            Retire(controls);
            RetireUnits(m.Spot, h, t, t2);
            outcome("pass");
        }
    }
}
