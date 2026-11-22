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
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
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
    /// Sprint 6 checkpoint C2a (VER-02, F1): does an ordinary gameplay sequence reach a permitted save while a summon
    /// hold is active, and what does that save do to the hold in the continuing session?
    ///
    /// Sequence (single-link G1 representative, the Expanded Summoning crocodile):
    /// 1. The main character summons the crocodile through the product's own execution ability (UnitUseAbility). Its
    ///    brain is left as summoned.
    /// 2. A native hostile humanoid is spawned unmodified near it (eligibility for Dominate Person and the crocodile's
    ///    grab size is read from the engine and the product). The party's groups join combat through the engine's own
    ///    perception/join controllers.
    /// 3. The crocodile's own AI attacks; its bite's grab establishes the hold. Native save admission is read (refused
    ///    in combat).
    /// 4. The main character casts the native Dominate Person on the held enemy through UnitUseAbility. The engine's
    ///    ChangeFaction moves it to the caster's side; the party leaves combat through the engine's own controller.
    /// 5. Native save admission is read again, and a native save is requested through Game.SaveGame (which itself refuses
    ///    when saving is not allowed). The continuing session is captured immediately after the save completes.
    ///
    /// Disclosed conveniences: crocodile BAB 100 (hit and grapple), hostile HP 100000 and brain inactive (it neither
    /// kills the main character nor flees; the engine still perceives it as an enemy), Dominate Person granted to the
    /// main character with caster level 20 and DC 100 (cast through the ordinary command), units woken.
    /// Read-only witnesses: SummonHoldComponent.OnTurnOff calls and the grab handler's attack origin.
    ///
    /// Modes: "reach" (the sequence, three saves, then a genuine terminal) and "reachReload" (fresh process).
    /// </summary>
    internal sealed class SaveSuspensionProbe : IOwnedProbe
    {
        private const BindingFlags Any = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        internal const string SaveLabel = "ver02c2ag1";
        internal const string HolderName = "KMG_C2a_Holder";
        internal const string TargetName = "KMG_C2a_Target";
        private const string HoldBuffName = "KMG_Summoning_Special_Grapple_Hold";
        private const string GrappledBuffName = "KMG_Summoning_Special_Grapple_Grappled";

        private static readonly Dictionary<string, int> TurnOffs = new Dictionary<string, int>();
        private static readonly List<JObject> GrabEvents = new List<JObject>();
        private static readonly List<JObject> Trace = new List<JObject>();

        private readonly string mode;
        private readonly JObject result = new JObject();
        private readonly List<string> problems = new List<string>();
        private readonly IEnumerator<object> script;
        private readonly List<UnitEntityData> created = new List<UnitEntityData>();
        private readonly List<UnitEntityData> awakeAdded = new List<UnitEntityData>();
        private readonly HarmonyInstance harmony = HarmonyInstance.Create("KmgLoaderCompatHarness.save-suspension-probe");
        private readonly List<MethodBase> patched = new List<MethodBase>();
        private string verdict;
        private Assembly product;
        private Type runner, linkPart, links, grabType, holdType;

        public SaveSuspensionProbe(string mode)
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
            catch (Exception exception) { problems.Add("probe exception: " + exception); more = false; }
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
                new KeyValuePair<string, Action>("witnesses", () =>
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
            TurnOffs.Clear(); GrabEvents.Clear(); Trace.Clear();
            Observe(holdType.GetMethods(Any).Single(m => m.Name == "MaintainLink" && m.GetParameters().Length == 7), nameof(MaintainPostfix));
            Observe(holdType.GetMethod("Release", Any), nameof(ReleasePostfix));
            Observe(holdType.GetMethod("ReleaseLink", Any), nameof(ReleaseLinkPostfix));
            Observe(typeof(UnitPartGrappleTarget).GetMethod("OnRemove"), nameof(TargetPartRemovedPostfix));
            Observe(typeof(UnitPartGrappleInitiator).GetMethod("OnRemove"), nameof(InitiatorPartRemovedPostfix));
            Observe(holdType.GetMethod("OnTurnOff"), nameof(TurnOffPostfix));
            Observe(grabType.GetMethod("OnEventDidTrigger", new[] { typeof(RuleAttackWithWeapon) }), nameof(GrabPostfix));
            bool paused = Game.Instance.IsPaused;
            result["area"] = Game.Instance.CurrentlyLoadedArea == null ? null : Game.Instance.CurrentlyLoadedArea.name;
            result["turnBased"] = TurnBased.Controllers.CombatController.IsInTurnBasedCombat();
            try
            {
                IEnumerable<object> body = mode == "reach" ? Reach() : mode == "reachReload" ? ReachReload() : null;
                if (body == null) { problems.Add("unknown mode " + mode); yield break; }
                foreach (object step in body) yield return step;
            }
            finally { Game.Instance.IsPaused = paused; }
        }

        private void Observe(MethodBase method, string postfix)
        {
            harmony.Patch(method, null, new HarmonyMethod(typeof(SaveSuspensionProbe).GetMethod(postfix, BindingFlags.Static | BindingFlags.NonPublic)));
            patched.Add(method);
        }

        private static string OwnerId(object logic)
        {
            var descriptor = logic == null ? null : logic.GetType().GetProperty("Owner", Any)?.GetValue(logic, null) as UnitDescriptor;
            return descriptor == null || descriptor.Unit == null ? null : descriptor.Unit.UniqueId;
        }

        private static void TurnOffPostfix(object __instance)
        {
            string id = OwnerId(__instance);
            if (id == null) return;
            int value; TurnOffs.TryGetValue(id, out value); TurnOffs[id] = value + 1;
        }

        private static void GrabPostfix(object __instance, RuleAttackWithWeapon evt)
        {
            if (evt == null || GrabEvents.Count > 40) return;
            UnitEntityData owner = evt.Initiator;
            GrabEvents.Add(new JObject
            {
                ["attacker"] = owner == null ? null : owner.UniqueId, ["target"] = evt.Target == null ? null : evt.Target.UniqueId,
                ["hit"] = evt.AttackRoll != null && evt.AttackRoll.IsHit, ["weapon"] = evt.Weapon == null ? null : evt.Weapon.Blueprint.name,
                ["attackerHasUnitAttackCommand"] = owner != null && owner.Commands.Attack != null,
                ["attackerBrainActive"] = owner != null && owner.IsBrainActive,
                ["gameSeconds"] = Game.Instance.TimeController.GameTime.TotalSeconds
            });
        }

        private static void Note(string what, string detail)
        {
            if (Trace.Count > 60) return;
            string stack = new System.Diagnostics.StackTrace(2, false).GetFrames()?.Take(6)
                .Select(f => f.GetMethod() == null ? "?" : f.GetMethod().DeclaringType?.Name + "." + f.GetMethod().Name)
                .Aggregate((a, b) => a + " < " + b);
            Trace.Add(new JObject { ["what"] = what, ["detail"] = detail, ["gameSeconds"] = Game.Instance.TimeController.GameTime.TotalSeconds,
                ["paused"] = Game.Instance.IsPaused, ["stack"] = stack });
        }

        private static void MaintainPostfix(UnitEntityData owner, UnitEntityData target, string __result) { Note("MaintainLink", __result); }
        private static void ReleasePostfix(UnitEntityData owner, UnitEntityData target) { Note("SummonHoldComponent.Release", target == null ? null : target.UniqueId); }
        private static void ReleaseLinkPostfix(UnitEntityData owner, UnitEntityData target) { Note("SummonHoldComponent.ReleaseLink", target == null ? null : target.UniqueId); }
        private static void TargetPartRemovedPostfix(UnitPartGrappleTarget __instance) { Note("UnitPartGrappleTarget.OnRemove", __instance.Owner?.Unit?.UniqueId); }
        private static void InitiatorPartRemovedPostfix(UnitPartGrappleInitiator __instance) { Note("UnitPartGrappleInitiator.OnRemove", __instance.Owner?.Unit?.UniqueId); }

        private static int TurnOffsOf(UnitEntityData unit)
        {
            int value;
            return unit != null && TurnOffs.TryGetValue(unit.UniqueId, out value) ? value : 0;
        }

        // ---------------------------------------------------------------- reach

        private sealed class Pair { internal UnitEntityData Holder, Target; internal BlueprintBuff HoldBuff, GrappledBuff; }

        private IEnumerable<object> Reach()
        {
            UnitEntityData main = Game.Instance.Player.MainCharacter.Value;
            var steps = new JArray();
            result["sequence"] = steps;
            result["admissionAtStart"] = Admission();

            // 1. Summon.
            UnitEntityData holder = Summon(main, "crocodile");
            if (holder == null) { problems.Add("the crocodile was not summoned"); yield break; }
            holder.Descriptor.Stats.BaseAttackBonus.BaseValue = 100;
            WakeUnit(holder);
            steps.Add("summoned crocodile " + holder.UniqueId + " through the product execution ability (brain left active: " + holder.IsBrainActive + ")");

            // 2. A native hostile humanoid, eligible for Dominate Person and the grab.
            var dominate = ResourcesLibrary.LibraryObject.GetAllBlueprints().OfType<BlueprintAbility>().FirstOrDefault(b => b.name == "DominatePerson");
            if (dominate == null) { problems.Add("native DominatePerson not found"); yield break; }
            bool added = !main.Descriptor.HasFact(dominate);
            if (added) main.Descriptor.AddFact(dominate);
            Ability granted = main.Descriptor.Abilities.GetAbility(dominate);
            object grab = grabType.GetMethod("Find", Any).Invoke(null, new object[] { holder });
            UnitEntityData hostile = null;
            var rejected = new JArray();
            Vector3 at = ObstacleAnalyzer.TraceAlongNavmesh(holder.Position, holder.Position + holder.OrientationDirection * 1.6f);
            foreach (BlueprintUnit candidate in ResourcesLibrary.LibraryObject.GetAllBlueprints().OfType<BlueprintUnit>()
                .Where(u => u.Faction != null && !u.Faction.Peaceful && u.Faction.AttackFactions != null &&
                    u.Faction.AttackFactions.Contains(main.Blueprint.Faction) && u.Size == Kingmaker.Enums.Size.Medium && !u.name.StartsWith("KMG_", StringComparison.Ordinal))
                .OrderBy(u => u.name.IndexOf("Bandit", StringComparison.OrdinalIgnoreCase) >= 0 ? 0 : 1).ThenBy(u => u.AssetGuid).Take(25))
            {
                UnitEntityData unit = Game.Instance.EntityCreator.SpawnUnit(candidate, at, Quaternion.identity, Game.Instance.State.LoadedAreaState.MainState);
                if (unit == null) continue;
                Game.Instance.EntityCreator.Tick();
                bool enemy = unit.IsEnemy(main) && unit.IsEnemy(holder);
                bool dominatable = new AbilityData(granted).CanTarget(new TargetWrapper(unit));
                bool grabbable = (bool)product.GetType("KingmakerGunslinger.Summoning.ExpandedSummoningSpecialProfiles", true).GetMethod("IsGrabSizeAllowed", Any)
                    .Invoke(null, new object[] { (int)unit.Descriptor.State.Size, (int)holder.Descriptor.State.Size, (int)grabType.GetField("MaxTargetSizeDelta").GetValue(grab) });
                if (enemy && dominatable && grabbable) { hostile = unit; break; }
                rejected.Add(candidate.name + ":enemy=" + enemy + ",dominatable=" + dominatable + ",grabbable=" + grabbable);
                unit.Destroy();
            }
            result["hostileRejected"] = rejected;
            if (hostile == null) { problems.Add("no eligible native hostile humanoid was found"); yield break; }
            created.Add(hostile);
            hostile.Descriptor.Stats.HitPoints.BaseValue = 100000;
            runner.GetMethod("SetExpandedSummoningBrainActive", Any).Invoke(null, new object[] { hostile, false });
            WakeUnit(hostile);
            var pair = new Pair
            {
                Holder = holder, Target = hostile,
                HoldBuff = (BlueprintBuff)grabType.GetField("HoldBuff").GetValue(grab),
                GrappledBuff = (BlueprintBuff)grabType.GetField("GrappledBuff").GetValue(grab)
            };
            result["hostile"] = new JObject { ["blueprint"] = hostile.Blueprint.name, ["guid"] = hostile.Blueprint.AssetGuid,
                ["faction"] = hostile.Descriptor.Faction.name, ["size"] = hostile.Descriptor.State.Size.ToString() };
            steps.Add("spawned native hostile " + hostile.Blueprint.name + " (unmodified faction " + hostile.Descriptor.Faction.name + ")");

            // 3. The crocodile's own AI attacks and grabs.
            foreach (object step in Settle(90f, () => IsHeld(pair))) yield return step;
            result["atHold"] = new JObject { ["state"] = State(pair), ["admission"] = Admission(), ["grabEvents"] = new JArray(GrabEvents.ToArray()) };
            if (!IsHeld(pair)) { problems.Add("the crocodile's AI did not establish a hold within the budget: " + result["atHold"].ToString(Formatting.None)); yield break; }
            steps.Add("crocodile AI bite established the hold");
            if (!(bool)result["atHold"]["admission"]["inCombat"]) problems.Add("the party was not in combat while the hostile was held");
            if ((bool)result["atHold"]["admission"]["saveAllowed"]) problems.Add("native save admission allowed a save during combat");

            // 4. Dominate Person on the held enemy, through the ordinary command.
            JObject cast = null;
            for (int attempt = 0; attempt < 3 && hostile.Descriptor.Faction != main.Descriptor.Faction; attempt++)
            {
                granted.OverrideParams = new AbilityParams { CasterLevel = 20, SpellLevel = 4, DC = 100 };
                var data = new AbilityData(granted);
                var command = new UnitUseAbility(data, new TargetWrapper(hostile));
                command.IgnoreCooldown(TimeSpan.Zero);
                main.Commands.Run(command);
                foreach (object step in Settle(20f, () => command.IsFinished)) yield return step;
                cast = new JObject { ["attempt"] = attempt, ["result"] = command.Result.ToString(), ["finished"] = command.IsFinished,
                    ["hostileFaction"] = hostile.Descriptor.Faction.name, ["hostileGroup"] = hostile.GroupId, ["mainGroup"] = main.GroupId };
            }
            if (added && main.Descriptor.HasFact(dominate)) main.Descriptor.RemoveFact(dominate);
            result["dominate"] = cast;
            steps.Add("main character cast native Dominate Person on the held enemy");
            foreach (object step in Settle(30f, () => !Game.Instance.Player.IsInCombat && Game.Instance.SaveManager.IsSaveAllowed())) yield return step;
            Game.Instance.IsPaused = true;
            yield return null;
            JObject before = State(pair);
            JObject admission = Admission();
            result["beforeSave"] = new JObject { ["state"] = before, ["admission"] = admission };
            if (!IsHeld(pair)) { problems.Add("the hold ended before a save became admissible: " + before.ToString(Formatting.None)); yield break; }
            if ((bool)admission["inCombat"] || !(bool)admission["saveAllowed"]) { problems.Add("native save admission did not permit a save with the hold active: " + admission.ToString(Formatting.None)); yield break; }
            steps.Add("party left combat; native save admission permits saving with the hold active");
            holder.Descriptor.CustomName = HolderName;
            hostile.Descriptor.CustomName = TargetName;

            // 5. Native saves; continuing-session capture after each.
            var saves = new JArray();
            result["saves"] = saves;
            for (int index = 0; index < 3; index++)
            {
                int turnOffs = TurnOffsOf(holder);
                int targetDamage = hostile.Descriptor.Damage;
                bool done = false, requested = false;
                SaveInfo info = Game.Instance.SaveManager.CreateNewSave(index == 0 ? SaveLabel : SaveLabel + "r" + index);
                TimeSpan clock = Game.Instance.TimeController.GameTime;
                Game.Instance.SaveGame(info, () => done = true);
                requested = LoadingProcess.Instance.IsLoadingInProcess || done;
                float until = Time.realtimeSinceStartup + 120f;
                while ((!done || Game.Instance.SaveManager.CommitInProgress || LoadingProcess.Instance.IsLoadingInProcess) && Time.realtimeSinceStartup < until)
                    yield return null;
                JObject after = State(pair);
                saves.Add(new JObject
                {
                    ["index"] = index, ["requestedThroughGameSaveGame"] = requested, ["completed"] = done,
                    ["gameClockUnchanged"] = Game.Instance.TimeController.GameTime == clock, ["pausedThroughout"] = Game.Instance.IsPaused,
                    ["holdTurnOffsDuringSave"] = TurnOffsOf(holder) - turnOffs, ["targetDamageDelta"] = hostile.Descriptor.Damage - targetDamage,
                    ["after"] = after
                });
                if (!done) { problems.Add("save " + index + " did not complete"); yield break; }
                if (!IsHeld(pair) || (int)after["rawLinksNamingTarget"] != 1 || !(bool)after["live.targetGrappledBuff"] || !(bool)after["live.holderHoldBuff"])
                    problems.Add("save " + index + ": the hold did not survive the save in the continuing session: " + after.ToString(Formatting.None));
                if (hostile.Descriptor.Damage != targetDamage) problems.Add("save " + index + ": the target took damage during the save");
                if (index == 0 && problems.Count > 0) break;
            }
            if (problems.Count > 0) yield break;
            steps.Add("three native saves; the hold continued after each");

            // 6. The hold still works after the saves: one maintain round passes with the target constrained.
            foreach (object step in SettleGameTime(7.0, 30f)) yield return step;
            Game.Instance.IsPaused = true;
            result["afterRound"] = State(pair);
            if (!IsHeld(pair)) problems.Add("the hold did not continue across a round after the saves");

            // 7. Genuine terminal still releases: the holder's death.
            GameHelper.KillUnit(holder);
            foreach (object step in Settle(12f, () => pair.Target.Get<UnitPartGrappleTarget>() == null && !HasBuff(pair.Target, pair.GrappledBuff))) yield return step;
            Game.Instance.IsPaused = true;
            JObject death = State(pair);
            result["afterHolderDeath"] = death;
            if ((bool)death["live.targetPart"] || (bool)death["live.targetGrappledBuff"] || (bool)death["live.targetCantMove"] || (bool)death["live.targetEntangled"])
                problems.Add("holder death after the saves did not release the target: " + death.ToString(Formatting.None));
            created.Add(holder);
        }

        // ---------------------------------------------------------------- reload

        private IEnumerable<object> ReachReload()
        {
            UnitEntityData holder = Game.Instance.State.Units.FirstOrDefault(u => u.Descriptor.CustomName == HolderName);
            UnitEntityData target = Game.Instance.State.Units.FirstOrDefault(u => u.Descriptor.CustomName == TargetName);
            if (holder == null || target == null) { problems.Add("reload: the named pair is missing"); yield break; }
            created.Add(holder); created.Add(target);
            var pair = new Pair { Holder = holder, Target = target, HoldBuff = BuffNamed(HoldBuffName), GrappledBuff = BuffNamed(GrappledBuffName) };
            var row = new JObject { ["atLoad"] = State(pair), ["admission"] = Admission() };
            result["reload"] = row;
            ExpectReset(pair, "reload.atLoad");
            WakeUnit(holder); WakeUnit(target);
            int damage = target.Descriptor.Damage;
            TimeSpan start = Game.Instance.TimeController.GameTime;
            foreach (object step in SettleGameTime(13.0, 40f)) yield return step;
            row["gameSecondsElapsed"] = (Game.Instance.TimeController.GameTime - start).TotalSeconds;
            row["targetDamageDelta"] = target.Descriptor.Damage - damage;
            row["afterRounds"] = State(pair);
            if ((double)row["gameSecondsElapsed"] < 12.0) problems.Add("reload: fewer than two rounds of game time elapsed");
            if ((int)row["targetDamageDelta"] != 0) problems.Add("reload: the target took damage after the reset");
            ExpectReset(pair, "reload.afterRounds");
        }

        // ---------------------------------------------------------------- helpers

        private static JObject Admission()
        {
            return new JObject
            {
                ["inCombat"] = Game.Instance.Player.IsInCombat,
                ["saveAllowed"] = Game.Instance.SaveManager.IsSaveAllowed(),
                ["turnBased"] = TurnBased.Controllers.CombatController.IsInTurnBasedCombat(),
                ["gameSeconds"] = Game.Instance.TimeController.GameTime.TotalSeconds
            };
        }

        private UnitEntityData Summon(UnitEntityData caster, string key)
        {
            Type multiplicity = product.GetType("KingmakerGunslinger.Summoning.SummonMultiplicity", true);
            object variant = runner.GetMethod("ExpandedSummoningOwnTierVariant", Any).Invoke(null, new[] { key, Enum.Parse(multiplicity, "One") });
            object evidence = Activator.CreateInstance(runner.GetNestedType("ExpandedSummoningMechanicalEvidence", Any), true);
            var library = (LibraryScriptableObject)product.GetType("KingmakerGunslinger.Bootstrap.BlueprintBootstrap", true).GetProperty("Library", Any).GetValue(null, null);
            var units = (UnitEntityData[])runner.GetMethods(Any).Single(m => m.Name == "CastExpandedSummoningVariant" && m.GetParameters().Length == 5)
                .Invoke(null, new object[] { library.GetAllBlueprints().ToArray(), caster, variant, null, evidence });
            UnitEntityData unit = units.Length == 1 ? units[0] : null;
            if (unit != null) created.Add(unit);
            return unit;
        }

        private void WakeUnit(UnitEntityData unit)
        {
            unit.Wake(300f);
            if (!Game.Instance.State.AwakeUnits.Contains(unit)) { Game.Instance.State.AwakeUnits.Add(unit); awakeAdded.Add(unit); }
        }

        private static IEnumerable<object> Settle(float wallBudget, Func<bool> done)
        {
            float until = Time.realtimeSinceStartup + wallBudget;
            for (int frame = 0; Time.realtimeSinceStartup < until; frame++)
            {
                if (Game.Instance.IsPaused) Game.Instance.IsPaused = false;
                if (frame >= 5 && done()) break;
                yield return null;
            }
        }

        private static IEnumerable<object> SettleGameTime(double gameSeconds, float wallBudget)
        {
            TimeSpan start = Game.Instance.TimeController.GameTime;
            float until = Time.realtimeSinceStartup + wallBudget;
            while (Time.realtimeSinceStartup < until && (Game.Instance.TimeController.GameTime - start).TotalSeconds < gameSeconds)
            {
                if (Game.Instance.IsPaused) Game.Instance.IsPaused = false;
                yield return null;
            }
        }

        private static BlueprintBuff BuffNamed(string name)
        {
            return ResourcesLibrary.LibraryObject.GetAllBlueprints().OfType<BlueprintBuff>().FirstOrDefault(b => b.name == name);
        }

        private static bool HasBuff(UnitEntityData unit, BlueprintBuff blueprint)
        {
            return blueprint != null && unit.Descriptor.Buffs.RawFacts.OfType<Buff>().Any(x => ReferenceEquals(x.Blueprint, blueprint));
        }

        private static bool IsHeld(Pair pair)
        {
            UnitPartGrappleInitiator initiator = pair.Holder.Get<UnitPartGrappleInitiator>();
            UnitPartGrappleTarget held = pair.Target.Get<UnitPartGrappleTarget>();
            return initiator != null && held != null && ReferenceEquals(initiator.Target.Value, pair.Target) && ReferenceEquals(held.Initiator.Value, pair.Holder);
        }

        private IList<string> RawLinks(UnitEntityData holder)
        {
            object part = typeof(UnitDescriptor).GetMethods().Single(m => m.Name == "Get" && m.IsGenericMethodDefinition && m.GetParameters().Length == 0)
                .MakeGenericMethod(linkPart).Invoke(holder.Descriptor, null);
            return part == null ? null : (IList<string>)linkPart.GetField("_links", Any).GetValue(part);
        }

        private JObject State(Pair pair)
        {
            IList<string> raw = RawLinks(pair.Holder);
            UnitPartGrappleInitiator initiator = pair.Holder.Get<UnitPartGrappleInitiator>();
            UnitPartGrappleTarget held = pair.Target.Get<UnitPartGrappleTarget>();
            var reconciled = (ICollection)links.GetMethod("Reconcile", Any).Invoke(null, new object[] { pair.Holder });
            UnitState t = pair.Target.Descriptor.State;
            return new JObject
            {
                ["holder"] = pair.Holder.UniqueId, ["target"] = pair.Target.UniqueId,
                ["holderDead"] = pair.Holder.Descriptor.State.IsDead, ["targetDead"] = t.IsDead,
                ["targetFaction"] = pair.Target.Descriptor.Faction.name, ["targetIsEnemyOfHolder"] = pair.Target.IsEnemy(pair.Holder),
                ["raw.links"] = raw == null ? null : new JArray(raw.Cast<object>().ToArray()),
                ["rawLinksNamingTarget"] = raw == null ? 0 : raw.Count(l => l != null && l.StartsWith(pair.Target.UniqueId + "|", StringComparison.Ordinal)),
                ["live.reconciledLinks"] = reconciled.Count,
                ["live.holderInitiatorPart"] = initiator != null,
                ["live.holderHoldBuff"] = HasBuff(pair.Holder, pair.HoldBuff),
                ["live.holderCantAct"] = pair.Holder.Descriptor.State.HasCondition(UnitCondition.CantAct),
                ["live.holderCantMove"] = pair.Holder.Descriptor.State.HasCondition(UnitCondition.CantMove),
                ["live.targetPart"] = held != null,
                ["live.targetGrappledBuff"] = HasBuff(pair.Target, pair.GrappledBuff),
                ["live.targetEntangled"] = t.HasCondition(UnitCondition.Entangled),
                ["live.targetCantMove"] = t.HasCondition(UnitCondition.CantMove),
                ["live.targetCantAct"] = t.HasCondition(UnitCondition.CantAct),
                ["live.targetCanMove"] = t.CanMove, ["live.targetCanAct"] = t.CanAct,
                ["targetDamage"] = pair.Target.Descriptor.Damage,
                ["holdTurnOffs"] = TurnOffsOf(pair.Holder),
                ["holderBuffs"] = new JArray(pair.Holder.Descriptor.Buffs.RawFacts.OfType<Buff>().Select(x => (object)x.Blueprint.name).ToArray()),
                ["targetBuffs"] = new JArray(pair.Target.Descriptor.Buffs.RawFacts.OfType<Buff>().Select(x => (object)x.Blueprint.name).ToArray()),
                ["gameSeconds"] = Game.Instance.TimeController.GameTime.TotalSeconds
            };
        }

        private void ExpectReset(Pair pair, string name)
        {
            JObject s = State(pair);
            if ((bool)s["live.holderInitiatorPart"] || (bool)s["live.holderHoldBuff"] || (bool)s["live.holderCantAct"] || (bool)s["live.holderCantMove"])
                problems.Add(name + ": the holder is not reset: " + s.ToString(Formatting.None));
            // Grapple-owned constraints only: the native CanMove/CanAct flags are also lowered by the native Dominate
            // Person state (observed in 20261122T214832Z with no grapple condition present), so they are recorded only.
            if ((bool)s["live.targetPart"] || (bool)s["live.targetGrappledBuff"] || (bool)s["live.targetEntangled"] || (bool)s["live.targetCantMove"] ||
                (bool)s["live.targetCantAct"])
                problems.Add(name + ": the target is not reset: " + s.ToString(Formatting.None));
            if ((int)s["rawLinksNamingTarget"] != 0 || (int)s["live.reconciledLinks"] != 0) problems.Add(name + ": records remain");
        }
    }
}
