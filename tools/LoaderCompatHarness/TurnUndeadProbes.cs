using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.LevelUp;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility;
using Kingmaker.View;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KmgLoaderCompatHarness
{
    /// <summary>
    /// Turn Undead native observations on the candidate product (planning/turn-undead/MISSION.md).
    ///
    /// Mode "first-turn" (intermediate checkpoint): a disposable cleric 5 / fighter 2 with Charisma 16 receives its
    /// real positive channel and the product's Turn Undead feat, then uses the product's granted cleric action through
    /// the game's ability command, in real time, against a mixed group placed around it: an ordinary skeleton (mindless)
    /// and a zombie lord (intelligent) whose Will totals are lowered so their genuine saves fail, a zombie lord whose
    /// Will is raised so its save succeeds, an ordinary skeleton beyond 30 feet and a living hostile inside it. The
    /// probe records the cleric's existing channel pool before and after, every RuleSavingThrow and damage/heal rule on
    /// the tracked units, the product's persisted flight state, the native Frightened/panic state and distances over
    /// time, each intelligent round save, a recovery after the zombie lord's Will is raised again, and the mindless
    /// skeleton's unrefreshed one-minute expiry.
    ///
    /// Disclosed setup (fixture, not product logic): Will totals set directly on the spawned undead; the cleric's
    /// positive channel added as the cleric channel selection's choice when the level-up controller leaves it
    /// unselected; the feat added as a fact (normal level-up acquisition is a separate final-checkpoint observation);
    /// brains off on every spawned unit so AI does not issue other commands; tracked units kept alive.
    /// </summary>
    internal sealed class TurnUndeadProbe : IOwnedProbe
    {
        private const BindingFlags Any = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const string ClericClass = "67819271767a9dd4fbfd4ae700befea0";
        private const string FighterClass = "48ac8db94d5de7645906c7d0ad3bcfbd";
        private const string ChannelPositive = "a79013ff4bcd4864cb669622a29ddafb";
        private const string ChannelResource = "5e2bba3e07c37be42909a12945c27de7";
        private const string Skeleton = "e8658c2a211043e4b983a666d3c7068e";
        private const string ZombieLord = "43e731641be4c4d4b906b6ee4f59c3fc";
        private const string UndeadType = "734a29b693e9ec346ba2951b27987e33";

        private readonly string mode;
        private readonly JObject result = new JObject();
        private readonly List<string> problems = new List<string>();
        private readonly IEnumerator<object> script;
        private readonly List<UnitEntityData> created = new List<UnitEntityData>();
        private readonly List<UnitEntityData> awakeAdded = new List<UnitEntityData>();
        private readonly List<BlueprintUnit> clones = new List<BlueprintUnit>();
        private readonly Trace trace = new Trace();
        private IDisposable subscription;
        private string verdict;
        private object set;
        private Vector3 origin, forward, right;
        private int unitIndex;

        public TurnUndeadProbe(string mode) { this.mode = mode; result["mode"] = mode; script = Run().GetEnumerator(); }
        public object Result => result;

        public string Poll()
        {
            if (verdict != null) return verdict;
            bool more;
            try { more = script.MoveNext(); }
            catch (Exception exception) { problems.Add("probe exception: " + exception); more = false; }
            if (more) return null;
            result["trace"] = new JArray(trace.Events.Cast<object>().ToArray());
            result["problems"] = new JArray(problems.Cast<object>().ToArray());
            verdict = problems.Count == 0 ? string.Empty : string.Join(" | ", problems.ToArray());
            return verdict;
        }

        public CleanupReport Release(string testFailure)
        {
            return ProbeCleanup.RunAll(new[]
            {
                new KeyValuePair<string, Action>("rule trace", () => { if (subscription != null) subscription.Dispose(); subscription = null; }),
                new KeyValuePair<string, Action>("awake registrations", () => { foreach (UnitEntityData u in awakeAdded) Game.Instance.State.AwakeUnits.Remove(u); awakeAdded.Clear(); }),
                new KeyValuePair<string, Action>("disposable units", () => { foreach (UnitEntityData u in created.Where(u => u != null && !u.Destroyed).ToArray()) u.Destroy(); created.Clear(); }),
                new KeyValuePair<string, Action>("unit blueprint clones", () => { foreach (BlueprintUnit c in clones) UnityEngine.Object.DestroyImmediate(c); clones.Clear(); })
            });
        }

        private IEnumerable<object> Run()
        {
            Assembly product = ProductProbes.Entry().Assembly;
            Type module = product.GetType("KingmakerGunslinger.TurnUndead.TurnUndeadModule", true);
            result["moduleStatus"] = (string)module.GetProperty("Status", Any).GetValue(null, null);
            set = module.GetProperty("Blueprints", Any).GetValue(null, null);
            if (set == null) { problems.Add("Turn Undead is not registered: " + result["moduleStatus"]); yield break; }
            result["featPublished"] = (bool)module.GetProperty("FeatPublished", Any).GetValue(null, null);
            result["resistanceEvidence"] = new JArray(((IEnumerable<string>)module.GetProperty("ResistanceEvidence", Any).GetValue(null, null)).Cast<object>().ToArray());
            result["area"] = Game.Instance.CurrentlyLoadedArea == null ? null : Game.Instance.CurrentlyLoadedArea.name;
            UnitEntityData main = Game.Instance.Player.MainCharacter.Value;
            origin = ObstacleAnalyzer.TraceAlongNavmesh(main.Position, main.Position + Quaternion.Euler(0, main.Orientation, 0) * Vector3.forward * 8f);
            forward = Quaternion.Euler(0, main.Orientation, 0) * Vector3.forward;
            right = Quaternion.Euler(0, main.Orientation + 90f, 0) * Vector3.forward;
            subscription = EventBus.Subscribe(trace);
            bool paused = Game.Instance.IsPaused;
            try
            {
                IEnumerable<object> body = mode == "first-turn" ? FirstTurn() : null;
                if (body == null) { problems.Add("unknown mode " + mode); yield break; }
                foreach (object step in body) yield return step;
            }
            finally { Game.Instance.IsPaused = paused; }
        }

        // ---------------------------------------------------------------- product identities

        private BlueprintFeature Feat => (BlueprintFeature)Member(set, "Feat");
        private BlueprintBuff Fleeing => (BlueprintBuff)Member(set, "Fleeing");

        private BlueprintAbility Direct(string kind)
        {
            foreach (object source in (Array)Member(set, "Sources"))
                if (Member(Member(source, "Contract"), "Kind").ToString() == kind) return (BlueprintAbility)Member(source, "Direct");
            throw new InvalidOperationException("no Turn Undead source " + kind);
        }

        private static object Member(object instance, string name)
        {
            PropertyInfo property = instance.GetType().GetProperty(name, Any);
            if (property != null) return property.GetValue(instance, null);
            FieldInfo field = instance.GetType().GetField(name, Any);
            if (field != null) return field.GetValue(instance);
            throw new MissingMemberException(instance.GetType().FullName, name);
        }

        private static T Blueprint<T>(string guid) where T : BlueprintScriptableObject
        {
            T value = ResourcesLibrary.TryGetBlueprint<T>(guid);
            if (value == null) throw new InvalidOperationException("missing native blueprint " + guid);
            return value;
        }

        // ---------------------------------------------------------------- fixture

        private Vector3 Spot(float forwardMeters, float rightMeters) =>
            ObstacleAnalyzer.TraceAlongNavmesh(origin, origin + forward * forwardMeters + right * rightMeters);

        private UnitEntityData Track(UnitEntityData unit, string label)
        {
            if (unit == null || unit.View == null) throw new InvalidOperationException("the game did not spawn a live " + label);
            created.Add(unit);
            Game.Instance.EntityCreator.Tick();
            if (!unit.Descriptor.IsTurnedOn) unit.Descriptor.TurnOn();
            unit.IsBrainActive = false;
            unit.Descriptor.State.Immortality.Retain();
            unit.Wake(900f);
            if (!Game.Instance.State.AwakeUnits.Contains(unit)) { Game.Instance.State.AwakeUnits.Add(unit); awakeAdded.Add(unit); }
            trace.Tracked.Add(unit);
            return unit;
        }

        private UnitEntityData SpawnFriendly(string label, Vector3 at)
        {
            BlueprintUnit clone = UnityEngine.Object.Instantiate(BlueprintRoot.Instance.DefaultPlayerCharacter);
            clone.name = "KmgTurnUndead_" + label + "_" + (++unitIndex);
            clone.IsCheater = false;
            clones.Add(clone);
            return Track(Game.Instance.EntityCreator.SpawnUnit(clone, at, Quaternion.identity, Game.Instance.State.LoadedAreaState.MainState), label);
        }

        private UnitEntityData SpawnNative(string guid, string label, Vector3 at) =>
            Track(Game.Instance.EntityCreator.SpawnUnit(Blueprint<BlueprintUnit>(guid), at, Quaternion.identity, Game.Instance.State.LoadedAreaState.MainState), label);

        /// <summary>A living copy of the default character under the first loaded faction hostile to the cleric.</summary>
        private UnitEntityData SpawnLivingHostile(UnitEntityData enemyOf, Vector3 at)
        {
            BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
            var seen = new HashSet<BlueprintFaction>();
            foreach (BlueprintUnit candidate in ResourcesLibrary.GetBlueprints<BlueprintUnit>().OrderBy(value => value.AssetGuid, StringComparer.Ordinal))
            {
                BlueprintFaction faction = candidate.Faction;
                if (faction == null || ReferenceEquals(faction, source.Faction) || !seen.Add(faction)) continue;
                BlueprintUnit clone = UnityEngine.Object.Instantiate(source);
                clone.name = "KmgTurnUndead_Living_" + (++unitIndex);
                clone.Faction = faction;
                UnitEntityData unit = null;
                try { unit = Game.Instance.EntityCreator.SpawnUnit(clone, at, Quaternion.identity, Game.Instance.State.LoadedAreaState.MainState); }
                catch (Exception) { }
                if (unit != null && unit.IsEnemy(enemyOf)) { clones.Add(clone); return Track(unit, "living hostile"); }
                if (unit != null) unit.Dispose();
                UnityEngine.Object.DestroyImmediate(clone);
            }
            throw new InvalidOperationException("no loaded faction produced a living hostile");
        }

        private static void Level(UnitEntityData unit, BlueprintCharacterClass characterClass, int levels)
        {
            MethodInfo apply = typeof(LevelUpController).GetMethod("ApplyLevelup", Any);
            if (apply == null) throw new MissingMethodException("LevelUpController.ApplyLevelup");
            for (int level = 1; level <= levels; level++)
            {
                bool first = unit.Descriptor.Progression.CharacterLevel == 0;
                LevelUpController controller = LevelUpController.StartWithoutAssigningStaticInstance(unit.Descriptor, false, null, null,
                    first ? LevelUpState.CharBuildMode.CharGen : LevelUpState.CharBuildMode.LevelUp);
                try
                {
                    if (!controller.SelectClass(characterClass, false)) throw new InvalidOperationException("the level-up controller refused " + characterClass.name);
                    controller.ApplyClassMechanics();
                    apply.Invoke(controller, new object[] { unit.Descriptor });
                }
                finally { controller.Cancel(); }
            }
        }

        private static void FleeContext(UnitEntityData cleric, UnitEntityData undead)
        {
            cleric.CombatState.JoinCombat();
            undead.CombatState.JoinCombat();
            if (!undead.Memory.Contains(cleric)) undead.Memory.Add(cleric);
            if (!cleric.Memory.Contains(undead)) cleric.Memory.Add(undead);
        }

        private static IEnumerable<object> Settle(double gameSeconds, float wallBudget, Func<bool> done)
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

        private static double Now => Game.Instance.TimeController.GameTime.TotalSeconds;

        // ---------------------------------------------------------------- observation

        private Buff FleeingOn(UnitEntityData unit) =>
            unit == null || unit.Destroyed ? null : unit.Descriptor.Buffs.RawFacts.OfType<Buff>().FirstOrDefault(b => ReferenceEquals(b.Blueprint, Fleeing));

        private static JObject State(Buff buff)
        {
            if (buff == null) return null;
            object logic = buff.Components.FirstOrDefault(c => c.GetType().Name == "TurnUndeadFleeingState");
            if (logic == null) return new JObject { ["component"] = false };
            Func<string, object> field = name => logic.GetType().GetField(name, Any).GetValue(logic);
            return new JObject
            {
                ["component"] = true, ["componentName"] = ((UnityEngine.Object)logic).name,
                ["initialized"] = (bool)field("_initialized"), ["mindless"] = (bool)field("_mindless"), ["ownerReview"] = (bool)field("_ownerReview"),
                ["dc"] = (int)field("_dc"), ["rule"] = (string)field("_rule"), ["sourceAbility"] = (string)field("_sourceAbility"),
                ["timeLeftSeconds"] = Math.Round(buff.TimeLeft.TotalSeconds, 2), ["contextDc"] = buff.Context.Params.DC,
                ["contextSourceAbility"] = buff.Context.SourceAbility == null ? null : buff.Context.SourceAbility.AssetGuid
            };
        }

        private JObject Unit(UnitEntityData unit, UnitEntityData from)
        {
            Buff buff = FleeingOn(unit);
            return new JObject
            {
                ["id"] = unit.UniqueId, ["blueprint"] = unit.Blueprint.AssetGuid, ["name"] = unit.Blueprint.name,
                ["undead"] = unit.Descriptor.HasFact(Blueprint<BlueprintUnitFact>(UndeadType)),
                ["distance"] = Math.Round(Vector3.Distance(unit.Position, from.Position), 2),
                ["frightened"] = unit.Descriptor.State.HasCondition(UnitCondition.Frightened), ["panicked"] = unit.Descriptor.State.IsPanicked,
                ["damage"] = unit.Descriptor.Damage, ["fleeing"] = State(buff),
                ["saveWill"] = unit.Stats.SaveWill.ModifiedValue
            };
        }

        private JArray Saves(UnitEntityData unit, int fromSeq) => new JArray(trace.Events
            .Where(e => (int)e["seq"] > fromSeq && (string)e["what"] == "save" && (string)e["unit"] == unit.UniqueId).Cast<object>().ToArray());

        private int Seq => trace.Events.Count == 0 ? 0 : (int)trace.Events.Last()["seq"];

        // ---------------------------------------------------------------- first turn

        private IEnumerable<object> FirstTurn()
        {
            BlueprintAbility clericTurn = Direct("Cleric");
            result["blueprints"] = new JObject
            {
                ["feat"] = new JObject { ["guid"] = Feat.AssetGuid, ["name"] = Feat.Name, ["icon"] = Feat.Icon == null ? null : Feat.Icon.name, ["groups"] = string.Join(",", Feat.Groups.Select(g => g.ToString()).ToArray()) },
                ["clericAction"] = new JObject { ["guid"] = clericTurn.AssetGuid, ["name"] = clericTurn.Name, ["icon"] = clericTurn.Icon == null ? null : clericTurn.Icon.name,
                    ["type"] = clericTurn.Type.ToString(), ["actionType"] = clericTurn.ActionType.ToString(), ["metamagic"] = clericTurn.AvailableMetamagic.ToString(),
                    ["components"] = new JArray(clericTurn.ComponentsArray.Select(c => (object)(c.GetType().Name + ":" + c.name)).ToArray()) },
                ["fleeing"] = new JObject { ["guid"] = Fleeing.AssetGuid, ["name"] = Fleeing.Name, ["icon"] = Fleeing.Icon == null ? null : Fleeing.Icon.name,
                    ["stacking"] = Fleeing.Stacking.ToString(), ["descriptor"] = Fleeing.ComponentsArray.Length }
            };
            Game.Instance.IsPaused = true;
            BlueprintAbilityResource pool = Blueprint<BlueprintAbilityResource>(ChannelResource);

            // ---- cleric 5 / fighter 2, Charisma 16
            UnitEntityData cleric = SpawnFriendly("Cleric", origin);
            cleric.Descriptor.Stats.Charisma.BaseValue = 16;
            cleric.Descriptor.Stats.Wisdom.BaseValue = 14;
            Level(cleric, Blueprint<BlueprintCharacterClass>(ClericClass), 5);
            var channel = Blueprint<BlueprintFeature>(ChannelPositive);
            bool channelFromLevelUp = cleric.Descriptor.HasFact(channel);
            if (!channelFromLevelUp) cleric.Descriptor.AddFact(channel);
            Level(cleric, Blueprint<BlueprintCharacterClass>(FighterClass), 2);
            yield return null;
            int poolBeforeFeat = cleric.Descriptor.Resources.GetResourceAmount(pool);
            cleric.Descriptor.AddFact(Feat);
            yield return null;
            int poolAfterFeat = cleric.Descriptor.Resources.GetResourceAmount(pool);
            JObject setup = new JObject
            {
                ["clericLevel"] = cleric.Descriptor.Progression.GetClassLevel(Blueprint<BlueprintCharacterClass>(ClericClass)),
                ["characterLevel"] = cleric.Descriptor.Progression.CharacterLevel,
                ["charismaBonus"] = cleric.Stats.Charisma.Bonus, ["channelFromLevelUp"] = channelFromLevelUp,
                ["poolBeforeFeat"] = poolBeforeFeat, ["poolAfterFeat"] = poolAfterFeat,
                ["hasFeat"] = cleric.Descriptor.HasFact(Feat),
                ["grantedActions"] = new JArray(new[] { "Cleric", "Paladin", "Hospitaler", "Empyreal" }
                    .Where(k => cleric.Descriptor.Abilities.GetAbility(Direct(k)) != null).Cast<object>().ToArray())
            };
            result["setup"] = setup;
            if (poolAfterFeat != poolBeforeFeat) problems.Add("learning the feat changed the channel pool from " + poolBeforeFeat + " to " + poolAfterFeat);
            if (setup["grantedActions"].Count() != 1 || (string)setup["grantedActions"][0] != "Cleric")
                problems.Add("the feat granted " + setup["grantedActions"] + " instead of exactly the cleric action");
            if (poolBeforeFeat < 1) { problems.Add("the cleric has no channel use to spend"); yield break; }

            // ---- targets
            UnitEntityData skeleton = SpawnNative(Skeleton, "skeleton", Spot(4f, 0f));
            UnitEntityData lord = SpawnNative(ZombieLord, "zombie lord", Spot(3f, 2.5f));
            UnitEntityData strong = SpawnNative(ZombieLord, "strong zombie lord", Spot(3f, -2.5f));
            UnitEntityData far = SpawnNative(Skeleton, "far skeleton", Spot(13f, 0f));
            UnitEntityData living = SpawnLivingHostile(cleric, Spot(-3f, 0f));
            skeleton.Stats.SaveWill.BaseValue = -100;
            lord.Stats.SaveWill.BaseValue = -100;
            strong.Stats.SaveWill.BaseValue = 100;
            foreach (UnitEntityData undead in new[] { skeleton, lord, strong, far }) FleeContext(cleric, undead);
            yield return null;
            var units = new[] { skeleton, lord, strong, far, living };
            result["before"] = new JArray(units.Select(u => (object)Unit(u, cleric)).ToArray());

            // ---- the cast
            Ability fact = cleric.Descriptor.Abilities.GetAbility(clericTurn);
            var data = new AbilityData(fact);
            int expectedDc = 10 + 5 / 2 + cleric.Stats.Charisma.Bonus;
            JObject cast = new JObject
            {
                ["available"] = data.IsAvailable, ["previewDc"] = data.CalculateParams().DC, ["expectedDc"] = expectedDc,
                ["nativeCharacterLevelDcWouldBe"] = 10 + cleric.Descriptor.Progression.CharacterLevel / 2 + cleric.Stats.Charisma.Bonus,
                ["poolBefore"] = cleric.Descriptor.Resources.GetResourceAmount(pool)
            };
            result["cast"] = cast;
            int seqBefore = Seq;
            double castAt = Now;
            cleric.Commands.InterruptAll(true);
            var command = new UnitUseAbility(data, new TargetWrapper(cleric));
            command.IgnoreCooldown(null);
            cleric.Commands.Run(command);
            foreach (object step in Settle(0.0, 30f, () => command.IsFinished && (command.ExecutionProcess == null || command.ExecutionProcess.IsEnded))) yield return step;
            cast["commandResult"] = command.Result.ToString();
            cast["poolAfter"] = cleric.Descriptor.Resources.GetResourceAmount(pool);
            cast["gameSecondsToFinish"] = Math.Round(Now - castAt, 2);
            cast["clericSaves"] = Saves(cleric, seqBefore);
            JArray after = new JArray(units.Select(u => (object)Unit(u, cleric)).ToArray());
            result["afterCast"] = after;
            foreach (UnitEntityData u in units) cast["saves." + u.UniqueId] = Saves(u, seqBefore);
            if ((int)cast["previewDc"] != expectedDc) problems.Add("preview DC " + cast["previewDc"] + " is not 10 + cleric 5 / 2 + Charisma = " + expectedDc);
            if ((int)cast["poolAfter"] != (int)cast["poolBefore"] - 1) problems.Add("the channel pool went from " + cast["poolBefore"] + " to " + cast["poolAfter"] + ", not exactly one use");
            ExpectInitialSave("skeleton", skeleton, seqBefore, expectedDc, false);
            ExpectInitialSave("zombie lord", lord, seqBefore, expectedDc, false);
            ExpectInitialSave("strong zombie lord", strong, seqBefore, expectedDc, true);
            if (Saves(far, seqBefore).Count != 0 || FleeingOn(far) != null) problems.Add("the skeleton beyond 30 feet was affected");
            if (Saves(living, seqBefore).Count != 0 || FleeingOn(living) != null) problems.Add("the living hostile was affected");
            ExpectState("skeleton", skeleton, true, expectedDc, clericTurn.AssetGuid, "mindless-family");
            ExpectState("zombie lord", lord, false, expectedDc, clericTurn.AssetGuid, "intelligent-identity-fact");
            if (FleeingOn(strong) != null) problems.Add("the zombie lord that saved still received the effect");
            if (trace.Events.Any(e => (int)e["seq"] > seqBefore && ((string)e["what"] == "damage" || (string)e["what"] == "heal") && (string)e["initiator"] == cleric.UniqueId))
                problems.Add("Turn Undead dealt damage or healing");

            // ---- flight and rounds (real time)
            int seqRounds = Seq;
            double roundsStart = Now;
            float skeletonStart = Vector3.Distance(skeleton.Position, cleric.Position);
            float lordStart = Vector3.Distance(lord.Position, cleric.Position);
            foreach (object step in Settle(13.0, 60f, () => true)) yield return step;
            JObject rounds = new JObject
            {
                ["gameSeconds"] = Math.Round(Now - roundsStart, 2),
                ["skeletonDistance"] = new JArray(Math.Round(skeletonStart, 2), Math.Round(Vector3.Distance(skeleton.Position, cleric.Position), 2)),
                ["lordDistance"] = new JArray(Math.Round(lordStart, 2), Math.Round(Vector3.Distance(lord.Position, cleric.Position), 2)),
                ["skeletonRepeatSaves"] = Saves(skeleton, seqRounds), ["lordRepeatSaves"] = Saves(lord, seqRounds),
                ["skeleton"] = Unit(skeleton, cleric), ["lord"] = Unit(lord, cleric), ["strong"] = Unit(strong, cleric)
            };
            result["rounds"] = rounds;
            if (Vector3.Distance(skeleton.Position, cleric.Position) - skeletonStart < 2f) problems.Add("the failed mindless skeleton did not move away (native flight not observed)");
            if (Saves(skeleton, seqRounds).Count != 0) problems.Add("the mindless skeleton received repeat saves");
            int lordRounds = Saves(lord, seqRounds).Count;
            if (lordRounds < 1 || lordRounds > 3) problems.Add("the intelligent zombie lord made " + lordRounds + " repeat saves in about two rounds");
            if (FleeingOn(lord) == null) problems.Add("the zombie lord's failed repeat saves did not keep the effect");

            // ---- intelligent recovery
            lord.Stats.SaveWill.BaseValue = 100;
            int seqRecovery = Seq;
            double recoveryStart = Now;
            foreach (object step in Settle(0.0, 40f, () => FleeingOn(lord) == null)) yield return step;
            result["recovery"] = new JObject
            {
                ["gameSeconds"] = Math.Round(Now - recoveryStart, 2), ["saves"] = Saves(lord, seqRecovery), ["lord"] = Unit(lord, cleric),
                ["skeletonStillFleeing"] = FleeingOn(skeleton) != null
            };
            if (FleeingOn(lord) != null) problems.Add("a successful repeat save did not end the zombie lord's effect");
            if (lord.Descriptor.State.HasCondition(UnitCondition.Frightened)) problems.Add("the zombie lord kept Frightened after recovery");
            if (FleeingOn(skeleton) == null) problems.Add("the mindless skeleton's effect ended early");

            // ---- unrefreshed one-minute expiry of the mindless effect
            Buff skeletonBuff = FleeingOn(skeleton);
            double expiresIn = skeletonBuff == null ? 0 : skeletonBuff.TimeLeft.TotalSeconds;
            foreach (object step in Settle(0.0, 120f, () => FleeingOn(skeleton) == null)) yield return step;
            result["expiry"] = new JObject
            {
                ["predictedSecondsLeft"] = Math.Round(expiresIn, 2), ["endedAfterCastSeconds"] = Math.Round(Now - castAt, 2),
                ["skeleton"] = Unit(skeleton, cleric), ["skeletonRepeatSavesWholeEffect"] = Saves(skeleton, seqRounds).Count
            };
            double total = Now - castAt;
            if (FleeingOn(skeleton) != null) problems.Add("the mindless skeleton's effect did not expire");
            else if (total < 57 || total > 70) problems.Add("the mindless effect lasted " + Math.Round(total, 1) + " s instead of about one minute");
            if (skeleton.Descriptor.State.HasCondition(UnitCondition.Frightened)) problems.Add("the skeleton kept Frightened after expiry");
            if (trace.Events.Any(e => (string)e["what"] == "damage" && (string)e["initiator"] == cleric.UniqueId)) problems.Add("the cleric dealt damage");
        }

        private void ExpectInitialSave(string label, UnitEntityData unit, int fromSeq, int dc, bool passed)
        {
            JArray saves = Saves(unit, fromSeq);
            if (saves.Count != 1) { problems.Add(label + " made " + saves.Count + " initial saves, expected one"); return; }
            JToken save = saves[0];
            if ((string)save["type"] != "Will" || (int)save["dc"] != dc) problems.Add(label + " save was " + save["type"] + " DC " + save["dc"] + ", expected Will DC " + dc);
            if ((bool)save["passed"] != passed)
            {
                int natural = (int?)save["natural"] ?? 0;
                problems.Add(label + " save passed=" + save["passed"] + " (natural " + natural + "), expected " + passed +
                    (natural == 1 || natural == 20 ? " - a natural " + natural + " decides the fixture; rerun" : ""));
            }
        }

        private void ExpectState(string label, UnitEntityData unit, bool mindless, int dc, string source, string rule)
        {
            Buff buff = FleeingOn(unit);
            if (buff == null) { problems.Add(label + " has no Turn Undead flight effect after failing"); return; }
            JObject state = State(buff);
            if (!(bool)state["component"] || !(bool)state["initialized"] || (bool)state["mindless"] != mindless || (int)state["dc"] != dc ||
                (string)state["sourceAbility"] != source || (string)state["rule"] != rule)
                problems.Add(label + " flight state is " + state.ToString(Newtonsoft.Json.Formatting.None));
            if (!unit.Descriptor.State.HasCondition(UnitCondition.Frightened)) problems.Add(label + " is not Frightened");
        }

        // ---------------------------------------------------------------- rule trace (read-only)

        private sealed class Trace : IGlobalRulebookHandler<RuleSavingThrow>, IGlobalRulebookHandler<RuleDealDamage>, IGlobalRulebookHandler<RuleHealDamage>
        {
            internal readonly HashSet<UnitEntityData> Tracked = new HashSet<UnitEntityData>();
            internal readonly List<JObject> Events = new List<JObject>();

            private void Add(string what, JObject data)
            {
                data["seq"] = Events.Count + 1;
                data["what"] = what;
                data["gameSeconds"] = Math.Round(Game.Instance.TimeController.GameTime.TotalSeconds, 2);
                if (Events.Count < 5000) Events.Add(data);
            }

            public void OnEventAboutToTrigger(RuleSavingThrow evt) { }
            public void OnEventDidTrigger(RuleSavingThrow evt)
            {
                if (evt == null || !Tracked.Contains(evt.Initiator)) return;
                string ability, reason;
                MechanicsContextReason(evt, out ability, out reason);
                Add("save", new JObject
                {
                    ["unit"] = evt.Initiator.UniqueId, ["type"] = evt.Type.ToString(), ["dc"] = evt.DifficultyClass,
                    ["natural"] = evt.D20 == null ? (int?)null : evt.D20.Value, ["total"] = evt.RollResult, ["passed"] = evt.IsPassed,
                    ["sourceAbility"] = ability, ["reason"] = reason
                });
            }

            private static void MechanicsContextReason(RuleSavingThrow evt, out string ability, out string reason)
            {
                ability = evt.Reason.Context == null || evt.Reason.Context.SourceAbility == null ? null : evt.Reason.Context.SourceAbility.AssetGuid;
                reason = evt.Reason.Fact != null ? "fact:" + evt.Reason.Fact.Blueprint.AssetGuid : evt.Reason.Context != null ? "context" : "other";
            }

            public void OnEventAboutToTrigger(RuleDealDamage evt) { }
            public void OnEventDidTrigger(RuleDealDamage evt)
            {
                if (evt == null || !Tracked.Contains(evt.Target)) return;
                Add("damage", new JObject { ["initiator"] = evt.Initiator?.UniqueId, ["target"] = evt.Target.UniqueId, ["damage"] = evt.Damage });
            }

            public void OnEventAboutToTrigger(RuleHealDamage evt) { }
            public void OnEventDidTrigger(RuleHealDamage evt)
            {
                if (evt == null || !Tracked.Contains(evt.Target)) return;
                Add("heal", new JObject { ["initiator"] = evt.Initiator?.UniqueId, ["target"] = evt.Target.UniqueId, ["value"] = evt.Value });
            }
        }
    }
}
