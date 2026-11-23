using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
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
    /// Sprint 6 VER-02 G1-R: five selected lethal-release paths on four representative single-link owners.
    ///
    /// A  Constrictor Snake: ordinary maintained weapon damage kills the held target (MaintainLink's death return before
    ///    constrict). B  Constrictor Snake: the target survives the maintained weapon damage, then the physical constrict
    ///    kills it (the DealConstrict post-damage release shared with Lillend). C  Salamander: its constrict kills
    ///    (SalamanderConstrictDamage, then the Salamander post-damage release). D  Crocodile: a later-round death roll
    ///    kills (DealDeathRoll's release). E  Barbed Devil: a later-round Impale kills (DealImpale's release and the
    ///    owner cleanup through the engine loop).
    ///
    /// Each case: a real hold from a real attack; one ordinary rider round (the rider is eligible and the target survives
    /// it); a disclosed fixture health setting (target Damage = HP − margin, the native dying allowance released on the
    /// disposable target so the native death threshold is HP); then the decisive native round. The probe identifies the
    /// damage rule that crossed the native death threshold, the rider bracket it ran in, the actual native Dead
    /// transition, and where the release ran. Nothing calls a rider, a release helper or KillUnit, sets a death flag or
    /// removes a part. The margin for B/C is the establishing weapon's maximum (read-only RuleCalculateWeaponStats) + 1;
    /// a weapon-lethal or non-lethal round is classified and retried (bounded), never counted for B/C.
    /// </summary>
    internal sealed partial class BarbedCrabProbe
    {
        private sealed class LethalSpec
        {
            internal string Id, Key, FirstRider, Begin, End, Path;
            internal bool ConstrictLethal;
        }

        private static readonly LethalSpec[] LethalSpecs =
        {
            new LethalSpec { Id = "A", Key = "constrictor-snake", FirstRider = "maintained:", Begin = "rider.weapon.begin", End = "rider.weapon.end", Path = "MaintainLink live-constrict weapon-damage death return" },
            new LethalSpec { Id = "B", Key = "constrictor-snake", FirstRider = "maintained:", Begin = "DealConstrict.begin", End = "DealConstrict", Path = "DealConstrict physical post-damage release", ConstrictLethal = true },
            new LethalSpec { Id = "C", Key = "salamander", FirstRider = "maintained:", Begin = "rider.salamanderConstrict.begin", End = "rider.salamanderConstrict.end", Path = "Salamander post-damage release", ConstrictLethal = true },
            new LethalSpec { Id = "D", Key = "crocodile", FirstRider = "death-roll:", Begin = "rider.deathRoll.begin", End = "rider.deathRoll.end", Path = "DealDeathRoll release" },
            new LethalSpec { Id = "E", Key = "barbed-devil", FirstRider = "impale:", Begin = "rider.impale.begin", End = "rider.impale.end", Path = "DealImpale release and owner cleanup" },
        };

        private IEnumerable<object> Lethal(string match)
        {
            string[] ids = match.Contains(":") ? match.Substring(match.IndexOf(':') + 1).Split(',') : LethalSpecs.Select(s => s.Id).ToArray();
            var summary = new JObject();
            result["lethal.summary"] = summary;
            foreach (LethalSpec spec in LethalSpecs.Where(s => ids.Contains(s.Id)))
            {
                var attempts = new JArray();
                result["lethal." + spec.Id + ".attempts"] = attempts;
                string outcome = null;
                for (int attempt = 1; attempt <= 3; attempt++)
                {
                    int before = problems.Count;
                    var row = new JObject { ["case"] = spec.Id, ["attempt"] = attempt, ["creature"] = spec.Key, ["intendedPath"] = spec.Path };
                    string o = null;
                    foreach (object step in LethalAttempt(spec, attempt, row, x => o = x)) yield return step;
                    if (o == "pass" && problems.Count > before) o = "fail";
                    row["outcome"] = o;
                    row["problems"] = new JArray(problems.Skip(before).Cast<object>().ToArray());
                    attempts.Add(row);
                    outcome = o;
                    if (o != "retry") break;
                }
                if (outcome == "retry") problems.Add("lethal." + spec.Id + ": retry budget exhausted");
                summary[spec.Id] = outcome;
            }
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

        private static JObject[] PairKind(int seq, UnitEntityData h, UnitEntityData t, string what) =>
            HoldLifecycleObserver.Events.Where(e => (int)e["seq"] > seq && Is(e, what) && PairOf(e, h, t)).ToArray();

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
            try
            {
                // Native death threshold on the disposable target: release the dying allowance so Dead = Damage >= HP.
                row["allowDyingBefore"] = (bool)t.Descriptor.State.AllowDyingCondition;
                t.Descriptor.State.AllowDyingCondition.ReleaseAll();
                row["allowDyingAfter"] = (bool)t.Descriptor.State.AllowDyingCondition;
                // Native survival rules past the threshold, released on the disposable target only (recorded; in the
                // fixture both Ferocity flags were already false).
                var features = t.Descriptor.State.Features;
                row["ferocityBefore"] = new JObject { ["ferocity"] = (bool)features.Ferocity, ["halfOrcFerocity"] = (bool)features.HalfOrcFerocity };
                features.Ferocity.ReleaseAll();
                features.HalfOrcFerocity.ReleaseAll();
                row["ferocityAfter"] = new JObject { ["ferocity"] = (bool)features.Ferocity, ["halfOrcFerocity"] = (bool)features.HalfOrcFerocity };
                if ((bool)features.Ferocity || (bool)features.HalfOrcFerocity || (bool)t.Descriptor.State.AllowDyingCondition || (bool)t.Descriptor.State.IsRegenerate)
                    problems.Add(at + ": the target still has a native survival rule past the threshold");
                // The fixture's DefaultPlayerCharacter is a Gunslinger: the product's Cheat Death deed (an independent
                // effect) resets Damage to MaxHP - 1 once per grit spend (baseline 20261123T152834Z). Its grit is spent
                // on the disposable target so the selected rider, not Cheat Death, decides the outcome.
                Type cheat = product.GetType("KingmakerGunslinger.Deeds.CheatDeathDamageHandler", true);
                var gritRows = new JArray();
                foreach (var fact in t.Descriptor.Progression.Features.Enumerable)
                    foreach (var component in fact.Blueprint.ComponentsArray ?? new Kingmaker.Blueprints.BlueprintComponent[0])
                        if (component.GetType() == cheat)
                        {
                            var grit = cheat.GetField("Grit").GetValue(component) as Kingmaker.Blueprints.BlueprintAbilityResource;
                            int amount = grit == null ? 0 : t.Descriptor.Resources.GetResourceAmount(grit);
                            if (grit != null && amount > 0) t.Descriptor.Resources.Spend(grit, amount);
                            gritRows.Add(new JObject { ["fact"] = fact.Blueprint.name, ["grit"] = grit?.name, ["before"] = amount,
                                ["after"] = grit == null ? 0 : t.Descriptor.Resources.GetResourceAmount(grit) });
                        }
                row["cheatDeathGrit"] = gritRows;
                if (gritRows.OfType<JObject>().Any(g => (int)g["after"] > 0)) problems.Add(at + ": the target's Cheat Death grit was not spent");
                row["holder"] = h.UniqueId; row["target"] = t.UniqueId; row["holderBlueprint"] = h.Blueprint.name;
                row["grabLimbs"] = new JArray(m.Weapons.Select(x => (object)(x.Blueprint.name + "@" + HoldLifecycleObserver.LimbOf(h, x))).ToArray());
            }
            catch (Exception e) { problems.Add(at + ": fixture setup threw " + e.Message); outcome("fail"); yield break; }

            // ---- one ordinary rider round: the rider is eligible and the target survives it
            int s1 = LastSeq;
            foreach (object step in SettleGameTime(0.0, 30f, () => DecisiveSince(s1, h, t).Length > 0 || !HeldSingle(m))) yield return step;
            foreach (object step in SettleGameTime(0.3, 5f, () => true)) yield return step;
            JObject[] first = DecisiveSince(s1, h, t);
            string k1 = Classify(s1, h, t);
            row["firstRound"] = new JObject { ["classification"] = k1, ["result"] = first.Length == 0 ? null : first[0]["result"], ["events"] = PairEvents(s1, h, t) };
            if (k1 == "ordinary-failed-maintain") { Retire(controls); RetireUnits(m.Spot, h, t); outcome("retry"); yield break; }
            if (first.Length == 0 || k1 != "held") { problems.Add(at + ": no held ordinary rider round (" + k1 + ")"); outcome("fail"); yield break; }
            if (!((string)first[0]["result"]).StartsWith(spec.FirstRider, StringComparison.Ordinal)) { problems.Add(at + ": the first round's rider is " + first[0]["result"] + ", not " + spec.FirstRider); outcome("fail"); yield break; }
            if (t.Descriptor.State.IsDead || !t.Descriptor.State.IsConscious) { problems.Add(at + ": the target did not survive the ordinary round"); outcome("fail"); yield break; }

            // ---- disclosed health setting, then the pre-decisive state
            int hp = (int)t.Descriptor.Stats.HitPoints;
            int weaponMax = WeaponMax(h, m.Weapons[0]);
            int margin = spec.ConstrictLethal ? weaponMax + 1 : 1;
            row["fixture"] = new JObject { ["targetHP"] = hp, ["damageBefore"] = t.Descriptor.Damage, ["margin"] = margin, ["weaponMax"] = weaponMax,
                ["setDamage"] = hp - margin, ["note"] = "target Damage set to HP - margin; native Dead at Damage >= HP with the dying allowance released" };
            HoldLifecycleObserver.WatchedDamage.Add(t.Descriptor);
            t.Descriptor.Damage = hp - margin;
            yield return null; yield return null;
            JObject pre = SingleState(m);
            row["preDecisive"] = pre;
            row["preDecisiveLimb"] = pre["establishingLimb"];
            if (t.Descriptor.State.IsDead || !t.Descriptor.State.IsConscious) { problems.Add(at + ": the target is not alive and conscious before the decisive round"); outcome("fail"); yield break; }
            if (h.Descriptor.State.IsDead || !h.Descriptor.State.IsConscious || h.Destroyed) { problems.Add(at + ": the holder is not alive and valid before the decisive round"); outcome("fail"); yield break; }
            ExpectSingleHeld(m, t, at + ".preDecisive");

            // ---- the decisive native round
            int s0 = LastSeq;
            int deadFrame = -1;
            double deadGame = -1;
            int lastDamage = t.Descriptor.Damage;
            var decreases = new JArray();
            row["damageDecreases"] = decreases;
            Func<bool> watch = () =>
            {
                int d = t.Descriptor.Damage;
                if (d < lastDamage && decreases.Count < 20) decreases.Add(new JObject { ["frame"] = Time.frameCount, ["gameSeconds"] = Now, ["from"] = lastDamage, ["to"] = d, ["lifeState"] = t.Descriptor.State.LifeState.ToString() });
                lastDamage = d;
                if (deadFrame < 0 && t.Descriptor.State.IsDead) { deadFrame = Time.frameCount; deadGame = Now; }
                return DecisiveSince(s0, h, t).Length > 0 || !HeldSingle(m) && deadFrame >= 0;
            };
            foreach (object step in SettleGameTime(0.0, 30f, watch)) yield return step;
            foreach (object step in SettleGameTime(0.0, 5f, () => { watch(); return deadFrame >= 0; })) yield return step;
            // Let cleanup and any scheduled callback complete; the asserted state is read after.
            foreach (object step in SettleGameTime(0.0, 10f, () => { watch(); return h.Get<UnitPartGrappleInitiator>() == null && HoldInstance(m) == null && t.Get<UnitPartGrappleTarget>() == null; })) yield return step;
            foreach (object step in SettleGameTime(0.3, 5f, () => { watch(); return true; })) yield return step;
            JArray window = Events(s0, e => PairOf(e, h, t) || (Is(e, "rule.damage") && (string)e["initiator"] == h.UniqueId && (string)e["target"] == t.UniqueId) ||
                (Is(e, "rule.maneuver") && (string)e["initiator"] == h.UniqueId && (string)e["target"] == t.UniqueId));
            row["decisiveWindow"] = window;
            JObject[] w = window.OfType<JObject>().ToArray();
            JObject[] decisive = w.Where(e => Is(e, "MaintainLink") && Decisive(e)).ToArray();
            JObject kill = w.FirstOrDefault(e => Is(e, "rule.damage") && (int)e["targetDamageAfter"] >= (int)e["targetHP"]);
            row["decisive"] = decisive.Length == 0 ? null : decisive[0];
            row["killingDamage"] = kill;
            row["nativeDead"] = new JObject { ["isDead"] = t.Descriptor.State.IsDead, ["lifeState"] = t.Descriptor.State.LifeState.ToString(), ["firstDeadFrame"] = deadFrame,
                ["firstDeadGameSeconds"] = deadGame, ["killFrame"] = kill?["frame"], ["killGameSeconds"] = kill?["gameSeconds"] };
            if (kill != null)
            {
                // The crossing is the lethal event only if the native Dead transition follows before any later damage
                // rule from this holder (a crossing that the engine does not turn into death is not a kill).
                JObject later = w.FirstOrDefault(e => Is(e, "rule.damage") && (int)e["seq"] > (int)kill["seq"] && (int)e["frame"] > (int)kill["frame"]);
                row["laterHolderDamageAfterCrossing"] = later;
                if (deadFrame < 0 || (later != null && (int)later["frame"] < deadFrame))
                    problems.Add(at + ": the threshold crossing at frame " + kill["frame"] + " was not followed by the native Dead transition before further damage (dead frame " + deadFrame + ")");
                row["framesFromCrossingToDead"] = deadFrame < 0 ? (int?)null : deadFrame - (int)kill["frame"];
            }
            row["heals"] = Events(s0, e => Is(e, "rule.heal") && (string)e["target"] == t.UniqueId);
            row["diag"] = Events(s0 - 40 < 0 ? 0 : s0 - 40, e => ((string)e["what"]).StartsWith("diag.", StringComparison.Ordinal) && (string)e["unit"] == t.UniqueId);
            if (decisive.Length == 0) { problems.Add(at + ": no decisive maintain in the decisive window"); outcome("fail"); yield break; }
            string k0 = Classify(s0, h, t);
            row["decisiveClassification"] = k0;
            if (kill == null)
            {
                if (k0 == "ordinary-failed-maintain") { Retire(controls); RetireUnits(m.Spot, h, t); outcome("retry"); yield break; }
                if (spec.ConstrictLethal) { row["nonLethal"] = "no damage rule crossed the native threshold"; Retire(controls); RetireUnits(m.Spot, h, t); outcome("retry"); yield break; }
                problems.Add(at + ": no damage rule crossed the native death threshold"); outcome("fail"); yield break;
            }
            int killSeq = (int)kill["seq"];
            JObject begin = w.LastOrDefault(e => Is(e, spec.Begin) && (int)e["seq"] < killSeq);
            JObject end = begin == null ? null : w.FirstOrDefault(e => Is(e, spec.End) && (int)e["seq"] > (int)begin["seq"]);
            bool inBracket = begin != null && (end == null || (int)end["seq"] > killSeq);
            JObject weaponBegin = w.LastOrDefault(e => Is(e, "rider.weapon.begin") && (int)e["seq"] < killSeq);
            JObject weaponEnd = weaponBegin == null ? null : w.FirstOrDefault(e => Is(e, "rider.weapon.end") && (int)e["seq"] > (int)weaponBegin["seq"]);
            bool weaponKilled = weaponBegin != null && (weaponEnd == null || (int)weaponEnd["seq"] > killSeq);
            row["killer"] = new JObject { ["expectedBracket"] = spec.Begin, ["inExpectedBracket"] = inBracket, ["weaponKilled"] = weaponKilled, ["begin"] = begin, ["end"] = end };
            if (spec.ConstrictLethal && weaponKilled) { row["weaponLethal"] = "the ordinary weapon damage crossed the threshold; this attempt does not qualify constrict-lethal cleanup"; Retire(controls); RetireUnits(m.Spot, h, t); outcome("retry"); yield break; }
            if (!inBracket) problems.Add(at + ": the killing damage did not run inside " + spec.Begin + ": " + kill.ToString(Formatting.None));
            if (spec.ConstrictLethal)
            {
                JObject wDamage = w.LastOrDefault(e => Is(e, "rule.damage") && weaponBegin != null && (int)e["seq"] > (int)weaponBegin["seq"] && weaponEnd != null && (int)e["seq"] < (int)weaponEnd["seq"]);
                row["survivedWeapon"] = wDamage;
                if (wDamage == null || (int)wDamage["targetDamageAfter"] >= (int)wDamage["targetHP"]) problems.Add(at + ": no surviving ordinary maintained weapon damage precedes the constrict kill");
            }
            // The intended release: a ReleaseLink for the pair inside the decisive maintain, after the killing damage.
            int decisiveSeq = (int)decisive[0]["seq"];
            JObject release = w.FirstOrDefault(e => Is(e, "SummonHoldComponent.ReleaseLink") && (int)e["seq"] > killSeq && (int)e["seq"] < decisiveSeq);
            row["releaseInsideRider"] = release;
            row["firstReleaseAfterKill"] = w.FirstOrDefault(e => (Is(e, "SummonHoldComponent.ReleaseLink") || Is(e, "SummonHoldComponent.Release")) && (int)e["seq"] > killSeq);
            if (release == null) problems.Add(at + ": the intended " + spec.Path + " did not run (no ReleaseLink between the killing damage and the maintain result " + decisive[0]["result"] + ")");
            if (spec.Id == "A" && w.Any(e => Is(e, "DealConstrict.begin") && (int)e["seq"] > killSeq)) problems.Add(at + ": constrict executed after the lethal weapon damage");
            if (!t.Descriptor.State.IsDead) problems.Add(at + ": the target never reached the native Dead state");

            // ---- immediate state after cleanup
            JObject after = SingleState(m);
            row["after"] = after;
            row["rawRecordsAfter"] = after["rawNamingTarget"]; // stored records: captured (accepted inert-residue policy)
            if ((bool)after["targetPart"] || (int)after["heldBuffs"] != 0) problems.Add(at + ": the dead target keeps grapple-owned state");
            if ((bool)after["initiatorPart"] || (int)after["holdBuffs"] != 0) problems.Add(at + ": the holder keeps its initiator part or hold buff");
            if (((JArray)after["live.reconciled"]).Count != 0 || (int)after["live.heldCount"] != 0 || ((JArray)after["live.occupancy"]).Any(o => (string)o["occupant"] != null))
                problems.Add(at + ": a live link or occupied limb remains");
            if (h.Destroyed || h.Descriptor.State.IsDead || !h.Descriptor.State.IsConscious || (bool)after["holderCantAct"] || (bool)after["holderCantMove"])
                problems.Add(at + ": the surviving holder is not usable");
            if (spec.Id == "E")
            {
                row["impaleCleanup"] = new JArray(w.Where(e => ((string)e["what"]).StartsWith("impaleCleanup.", StringComparison.Ordinal)).Cast<object>().ToArray());
            }
            Witness(at + ".decisive", controls);

            // ---- quiet: two native rounds, nothing more from the ended relationship
            int q0 = LastSeq;
            double t0 = Now;
            int dmg = t.Descriptor.Damage;
            foreach (object step in SettleGameTime(13.0, 40f, () => true)) yield return step;
            double quiet = Now - t0;
            row["quiet"] = new JObject { ["gameSeconds"] = quiet, ["events"] = PairEvents(q0, h, t), ["state"] = SingleState(m) };
            if (quiet < 12.0) problems.Add(at + ".quiet: fewer than two rounds of game time (" + quiet + ")");
            if (t.Descriptor.Damage != dmg || PairEvents(q0, h, t).OfType<JObject>().Any(e => Is(e, "rule.damage") || Is(e, "MaintainLink") || Is(e, "DealConstrict.begin")))
                problems.Add(at + ".quiet: damage or rider activity from the ended relationship");
            if (HeldSingle(h, t) || t.Get<UnitPartGrappleTarget>() != null) problems.Add(at + ".quiet: a live hold was recreated");

            // ---- reacquire a fresh living target
            UnitEntityData t2 = SpawnTarget(Around(h.Position, 90f, 1.0f));
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
