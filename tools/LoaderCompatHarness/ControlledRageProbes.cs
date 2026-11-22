using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KmgLoaderCompatHarness
{
    /// <summary>
    /// S6 T4 / RISK-03 (A1). Records every actual UnitPartControlledRageSelection PreSave and PostLoad the game makes
    /// (entered, completed, field values before and after) and every engine-logged exception that names Urban Barbarian
    /// code. Installed after every mod's Load when a launch asks for condition observeControlledRagePersistence. It
    /// changes nothing unless a probe arms an injection: then, only on the named units and only after the product's own
    /// PreSave has run, it writes a SYNTHETIC raw _state into the part so that the save file holds it.
    /// </summary>
    internal static class ControlledRageObserver
    {
        public const string Condition = "observeControlledRagePersistence";
        private const BindingFlags Any = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static bool requested;
        private static object current;
        private static JObject currentCall;

        public static bool Installed { get; private set; }
        public static Type PartType { get; private set; }
        public static FieldInfo StateField { get; private set; }
        public static FieldInfo RejectedField { get; private set; }
        public static readonly List<JObject> Calls = new List<JObject>();
        public static readonly List<string> Exceptions = new List<string>();

        /// <summary>Part instance -> synthetic raw _state written after the product's PreSave; armed by a probe.</summary>
        public static readonly Dictionary<object, string> Injections = new Dictionary<object, string>(new ReferenceComparer());

        public static void Request() => requested = true;

        public static JToken Install(HarmonyInstance harmony)
        {
            if (!requested || Installed) return null;
            Assembly product = ProductProbes.Entry().Assembly;
            PartType = product.GetType("KingmakerGunslinger.UrbanBarbarian.UnitPartControlledRageSelection", true);
            StateField = PartType.GetField("_state", Any) ?? throw new MissingFieldException(PartType.FullName, "_state");
            RejectedField = PartType.GetField("_rejectedStates", Any);
            const BindingFlags Own = BindingFlags.Static | BindingFlags.NonPublic;
            harmony.Patch(PartType.GetMethod("PreSave", BindingFlags.Instance | BindingFlags.Public),
                new HarmonyMethod(typeof(ControlledRageObserver).GetMethod(nameof(PreSavePrefix), Own)),
                new HarmonyMethod(typeof(ControlledRageObserver).GetMethod(nameof(PreSavePostfix), Own)));
            harmony.Patch(PartType.GetMethod("PostLoad", BindingFlags.Instance | BindingFlags.Public),
                new HarmonyMethod(typeof(ControlledRageObserver).GetMethod(nameof(PostLoadPrefix), Own)),
                new HarmonyMethod(typeof(ControlledRageObserver).GetMethod(nameof(PostLoadPostfix), Own)));
            int exceptionHooks = 0;
            foreach (MethodInfo method in typeof(UberDebug).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(m => m.Name == "LogException" && m.GetParameters().Length >= 1 && m.GetParameters()[0].ParameterType == typeof(Exception)))
            {
                harmony.Patch(method, new HarmonyMethod(typeof(ControlledRageObserver).GetMethod(nameof(ExceptionPrefix), Own)), null);
                exceptionHooks++;
            }
            Installed = true;
            return new JObject
            {
                ["part"] = PartType.FullName, ["rejectedField"] = RejectedField != null, ["exceptionHooks"] = exceptionHooks
            };
        }

        public static JArray CallsSince(int index) => new JArray(Calls.Skip(index).Cast<object>().ToArray());

        private static void PreSavePrefix(object __instance) => Begin("PreSave", __instance);

        private static void PostLoadPrefix(object __instance) => Begin("PostLoad", __instance);

        private static void PreSavePostfix(object __instance)
        {
            JObject call = Finish(__instance);
            string raw;
            if (call == null || !Injections.TryGetValue(__instance, out raw)) return;
            StateField.SetValue(__instance, raw);
            RejectedField?.SetValue(__instance, null);
            call["syntheticInjection"] = raw;
        }

        private static void PostLoadPostfix(object __instance) => Finish(__instance);

        private static void Begin(string kind, object part)
        {
            current = part;
            currentCall = new JObject
            {
                ["kind"] = kind, ["part"] = RuntimeHelpers.GetHashCode(part), ["owner"] = Owner(part),
                ["stateBefore"] = State(part), ["rejectedBefore"] = Rejected(part), ["completed"] = false
            };
            if (Calls.Count < 2000) Calls.Add(currentCall);
        }

        private static JObject Finish(object part)
        {
            if (!ReferenceEquals(part, current) || currentCall == null) return null;
            JObject call = currentCall;
            call["completed"] = true;
            call["stateAfter"] = State(part);
            call["rejectedAfter"] = Rejected(part);
            current = null;
            currentCall = null;
            return call;
        }

        private static void ExceptionPrefix(Exception __0)
        {
            string text = __0 == null ? null : __0.ToString();
            if (text != null && (text.Contains("UrbanBarbarian") || text.Contains("ControlledRage")) && Exceptions.Count < 50)
                Exceptions.Add(text);
        }

        public static string State(object part) => part == null ? null : (string)StateField.GetValue(part);

        public static JToken Rejected(object part)
        {
            if (part == null) return null;
            if (RejectedField == null) return "<field absent>";
            var list = RejectedField.GetValue(part) as IList<string>;
            return list == null ? null : new JArray(list.Cast<object>().ToArray());
        }

        private static string Owner(object part)
        {
            try
            {
                UnitDescriptor owner = (part as UnitPart)?.Owner;
                return owner?.Unit?.UniqueId ?? "unknown";
            }
            catch (Exception exception) { return "<" + exception.GetType().Name + ">"; }
        }

        private sealed class ReferenceComparer : IEqualityComparer<object>
        {
            public new bool Equals(object x, object y) => ReferenceEquals(x, y);
            public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
        }
    }

    /// <summary>
    /// The disposable Controlled Rage persistence fixture. "create" builds valid state through the real feature grants
    /// and allocation abilities on the main character and four disposable units spawned (from the registered default
    /// player blueprint, so they deserialize) beside it, arms SYNTHETIC raw _state injections on the four disposable
    /// units, saves, and restores the in-memory values. "reload" (a fresh process on that save) checks the native
    /// PostLoad, the restored state, rage grants and repeated reads, then performs ordinary mutations and a forced failed
    /// write. "final" (a fresh process on the following save) checks that the mutations and the retained payloads
    /// survived the next native PreSave and PostLoad.
    /// </summary>
    internal sealed class ControlledRageProbe : IOwnedProbe
    {
        private const BindingFlags Any = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const string FixtureFile = "controlled-rage-persistence-fixture.json";

        // Synthetic raw values (labelled synthetic in every record). Lengths appear in the product's diagnostic.
        private const string MalformedSibling = "4:2,2,0;6:6,2,0";
        private const string ConflictingDuplicate = "4:0,4,0;4:4,0,0";
        private const string Unattributable = "{\"schema\":2,\"tiers\":{\"4\":[0,0,4]}}";
        private const string SiblingsUnknownAndIdentical = "4:0,2,2;4:0,2,2;6:2,2,2;10:4,4,2";

        private readonly string profileDir;
        private readonly string mode;
        private readonly string saveLabel;
        private readonly JObject result = new JObject();
        private readonly List<string> problems = new List<string>();
        private readonly IEnumerator<object> script;
        private readonly HarmonyInstance harmony = HarmonyInstance.Create("KmgLoaderCompatHarness.controlled-rage-probe");
        private readonly List<object> injectedParts = new List<object>();
        private readonly Dictionary<object, Tuple<string, JToken>> productWritten = new Dictionary<object, Tuple<string, JToken>>();
        private bool addFactPatched;
        private string verdict;

        private static BlueprintUnitFact failAddFact;

        private Assembly product;
        private object set;
        private Type runtime, tierType;
        private IDictionary allocationsByAbility, factsByAllocation;

        public ControlledRageProbe(string profileDir, string mode, IDictionary<string, string> invariants)
        {
            this.profileDir = profileDir;
            this.mode = mode;
            string label;
            saveLabel = invariants != null && invariants.TryGetValue("saveLabel", out label) ? label : null;
            result["mode"] = mode;
            result["syntheticNotice"] = "Every raw _state injected by this probe is SYNTHETIC test corruption in a disposable fixture save.";
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
            result["exceptionsNamingUrbanBarbarian"] = new JArray(ControlledRageObserver.Exceptions.Cast<object>().ToArray());
            verdict = problems.Count == 0 ? string.Empty : string.Join(" | ", problems.ToArray());
            return verdict;
        }

        public CleanupReport Release(string testFailure)
        {
            return ProbeCleanup.RunAll(new[]
            {
                new KeyValuePair<string, Action>("synthetic injections", () =>
                {
                    foreach (object part in injectedParts) ControlledRageObserver.Injections.Remove(part);
                    injectedParts.Clear();
                }),
                new KeyValuePair<string, Action>("in-memory product values", () =>
                {
                    foreach (KeyValuePair<object, Tuple<string, JToken>> pair in productWritten)
                    {
                        ControlledRageObserver.StateField.SetValue(pair.Key, pair.Value.Item1);
                        if (ControlledRageObserver.RejectedField != null)
                            ControlledRageObserver.RejectedField.SetValue(pair.Key, pair.Value.Item2 == null || pair.Value.Item2.Type == JTokenType.Null ? null
                                : pair.Value.Item2.ToObject<List<string>>());
                    }
                    productWritten.Clear();
                }),
                new KeyValuePair<string, Action>("forced AddFact failure", () =>
                {
                    failAddFact = null;
                    if (addFactPatched) harmony.UnpatchAll(harmony.Id);
                    addFactPatched = false;
                })
            });
        }

        // ---- script ----

        private IEnumerable<object> Run()
        {
            if (!ControlledRageObserver.Installed) { problems.Add("the persistence observer is not installed (condition " + ControlledRageObserver.Condition + ")"); yield break; }
            Resolve();
            result["exceptionsAtStart"] = ControlledRageObserver.Exceptions.Count;
            IEnumerable<object> body = mode == "create" ? Create() : mode == "reload" ? Reload() : mode == "final" ? Final() : null;
            if (body == null) { problems.Add("unknown mode " + mode); yield break; }
            foreach (object step in body) yield return step;
            if (ControlledRageObserver.Exceptions.Count != 0)
                problems.Add(ControlledRageObserver.Exceptions.Count + " engine-logged exception(s) name Urban Barbarian code");
        }

        private IEnumerable<object> Create()
        {
            UnitEntityData main = Game.Instance.Player.MainCharacter.Value;
            var units = new JObject { ["valid"] = main.UniqueId };
            var cases = new Dictionary<string, UnitEntityData> { ["valid"] = main };
            string[] names = { "malformedSibling", "conflictingDuplicate", "unattributable", "siblingsUnknownIdentical" };
            for (int index = 0; index < names.Length; index++)
            {
                UnitEntityData unit = Game.Instance.EntityCreator.SpawnUnit(BlueprintRoot.Instance.DefaultPlayerCharacter,
                    main.Position + new Vector3(2f + 1.5f * index, 0f, 2f), Quaternion.identity, Game.Instance.State.LoadedAreaState.MainState);
                if (unit == null || unit.View == null) { problems.Add("the game did not spawn disposable unit " + names[index]); yield break; }
                if (!unit.Descriptor.IsTurnedOn) unit.Descriptor.TurnOn();
                unit.IsBrainActive = false;
                cases[names[index]] = unit;
                units[names[index]] = unit.UniqueId;
            }
            result["units"] = units;
            yield return null;

            // Valid state through the real paths: the feature grant runs the production tier controller (first-unlock
            // default), and the allocation abilities run through UnitUseAbility.
            var gameplay = new JObject();
            foreach (object step in Build(cases["valid"], new[] { 0, 0, 4 }, new[] { 2, 4, 0 }, gameplay, "valid")) yield return step;
            foreach (object step in Build(cases["malformedSibling"], new[] { 2, 2, 0 }, new[] { 4, 2, 0 }, gameplay, "malformedSibling")) yield return step;
            foreach (object step in Build(cases["conflictingDuplicate"], new[] { 0, 4, 0 }, null, gameplay, "conflictingDuplicate")) yield return step;
            foreach (object step in Build(cases["unattributable"], new[] { 0, 0, 4 }, null, gameplay, "unattributable")) yield return step;
            foreach (object step in Build(cases["siblingsUnknownIdentical"], new[] { 0, 2, 2 }, new[] { 2, 2, 2 }, gameplay, "siblingsUnknownIdentical")) yield return step;
            result["gameplay"] = gameplay;
            if (problems.Count != 0) yield break;

            ExpectState(cases["valid"], "valid.created", "4:0,0,4;6:2,4,0", null);
            ExpectState(cases["malformedSibling"], "malformedSibling.created", "4:2,2,0;6:4,2,0", null);
            ExpectState(cases["conflictingDuplicate"], "conflictingDuplicate.created", "4:0,4,0", null);
            ExpectState(cases["unattributable"], "unattributable.created", "4:0,0,4", null);
            ExpectState(cases["siblingsUnknownIdentical"], "siblingsUnknownIdentical.created", "4:0,2,2;6:2,2,2", null);
            var rage = new JObject();
            ExpectRage(cases["valid"], rage, "valid", 2, 4, 0);
            ExpectRage(cases["malformedSibling"], rage, "malformedSibling", 4, 2, 0);
            ExpectRage(cases["conflictingDuplicate"], rage, "conflictingDuplicate", 0, 4, 0);
            ExpectRage(cases["siblingsUnknownIdentical"], rage, "siblingsUnknownIdentical", 2, 2, 2);
            result["rageCreated"] = rage;
            if (problems.Count != 0) yield break;

            File.WriteAllText(Path.Combine(profileDir, FixtureFile), new JObject { ["units"] = units }.ToString());

            // Arm the synthetic injections and save; the observer writes them after the product's own PreSave.
            var injections = new JObject();
            Arm(cases["malformedSibling"], MalformedSibling, injections, "malformedSibling");
            Arm(cases["conflictingDuplicate"], ConflictingDuplicate, injections, "conflictingDuplicate");
            Arm(cases["unattributable"], Unattributable, injections, "unattributable");
            Arm(cases["siblingsUnknownIdentical"], SiblingsUnknownAndIdentical, injections, "siblingsUnknownIdentical");
            result["syntheticInjections"] = injections;
            int callIndex = ControlledRageObserver.Calls.Count;
            foreach (object step in Save()) yield return step;
            JArray calls = ControlledRageObserver.CallsSince(callIndex);
            result["saveCalls"] = calls;
            foreach (KeyValuePair<string, JToken> pair in injections)
            {
                string owner = cases[pair.Key].UniqueId;
                if (!calls.OfType<JObject>().Any(c => (string)c["kind"] == "PreSave" && (string)c["owner"] == owner && (bool)c["completed"] &&
                        (string)c["syntheticInjection"] == (string)pair.Value))
                    problems.Add(pair.Key + ": the save's PreSave did not complete with the synthetic injection");
            }
            if (!calls.OfType<JObject>().Any(c => (string)c["owner"] == main.UniqueId && (bool)c["completed"] && (string)c["stateAfter"] == "4:0,0,4;6:2,4,0"))
                problems.Add("valid: the save's PreSave did not complete with the released state");
        }

        private IEnumerable<object> Reload()
        {
            Dictionary<string, UnitEntityData> cases = Units();
            if (cases == null) yield break;
            JArray loadCalls = ControlledRageObserver.CallsSince(0);
            result["loadCalls"] = loadCalls;
            ExpectPostLoad(loadCalls, cases["valid"], "valid", "4:0,0,4;6:2,4,0", null, "4:0,0,4;6:2,4,0", null);
            ExpectPostLoad(loadCalls, cases["malformedSibling"], "malformedSibling", MalformedSibling, null, "4:2,2,0", MalformedSibling);
            ExpectPostLoad(loadCalls, cases["conflictingDuplicate"], "conflictingDuplicate", ConflictingDuplicate, null, null, ConflictingDuplicate);
            ExpectPostLoad(loadCalls, cases["unattributable"], "unattributable", Unattributable, null, null, Unattributable);
            ExpectPostLoad(loadCalls, cases["siblingsUnknownIdentical"], "siblingsUnknownIdentical", SiblingsUnknownAndIdentical, null,
                "4:0,2,2;6:2,2,2", SiblingsUnknownAndIdentical);

            ExpectState(cases["valid"], "valid.loaded", "4:0,0,4;6:2,4,0", null, "");
            ExpectState(cases["malformedSibling"], "malformedSibling.loaded", "4:2,2,0", MalformedSibling, "6");
            ExpectState(cases["conflictingDuplicate"], "conflictingDuplicate.loaded", null, ConflictingDuplicate, "4");
            ExpectState(cases["unattributable"], "unattributable.loaded", null, Unattributable, "4,6,8");
            ExpectState(cases["siblingsUnknownIdentical"], "siblingsUnknownIdentical.loaded", "4:0,2,2;6:2,2,2", SiblingsUnknownAndIdentical, "");
            var rage = new JObject();
            ExpectRage(cases["valid"], rage, "valid", 2, 4, 0);
            ExpectRage(cases["malformedSibling"], rage, "malformedSibling", 0, 0, 0);
            ExpectRage(cases["conflictingDuplicate"], rage, "conflictingDuplicate", 0, 0, 0);
            ExpectRage(cases["unattributable"], rage, "unattributable", 0, 0, 0);
            ExpectRage(cases["siblingsUnknownIdentical"], rage, "siblingsUnknownIdentical", 2, 2, 2);
            result["rageLoaded"] = rage;

            // Repeated production reads and unlocks change nothing and add no fact.
            var repeated = new JObject();
            foreach (KeyValuePair<string, UnitEntityData> pair in cases)
            {
                JObject before = Snapshot(pair.Value);
                for (int index = 0; index < 3; index++)
                {
                    Invoke("ResolveSelection", pair.Value.Descriptor, true);
                    // Only tiers the unit actually holds, as the production tier controllers would.
                    Invoke("UnlockTier", pair.Value.Descriptor, Tier(4));
                    if (pair.Value.Descriptor.HasFact((BlueprintUnitFact)Set("GreaterDefault"))) Invoke("UnlockTier", pair.Value.Descriptor, Tier(6));
                }
                JObject after = Snapshot(pair.Value);
                repeated[pair.Key] = after;
                if (!JToken.DeepEquals(before, after)) problems.Add(pair.Key + ": repeated reads/unlocks changed the state: " + before.ToString(Newtonsoft.Json.Formatting.None) +
                    " -> " + after.ToString(Newtonsoft.Json.Formatting.None));
            }
            result["repeated"] = repeated;
            if (problems.Count != 0) yield break;

            // Ordinary mutations through the allocation abilities and a later tier grant.
            var mutations = new JObject();
            foreach (object step in Select(cases["valid"], 6, new[] { 0, 2, 4 }, mutations, "valid")) yield return step;
            foreach (object step in Select(cases["malformedSibling"], 6, new[] { 0, 6, 0 }, mutations, "malformedSibling.explicitOverBlocked")) yield return step;
            foreach (object step in Select(cases["siblingsUnknownIdentical"], 6, new[] { 0, 4, 2 }, mutations, "siblingsUnknownIdentical")) yield return step;
            UnitDescriptor unattributable = cases["unattributable"].Descriptor;
            bool grantedGreater = unattributable.AddFact((BlueprintUnitFact)Set("NativeGreaterRage")) != null &&
                unattributable.AddFact((BlueprintUnitFact)Set("GreaterDefault")) != null;
            mutations["unattributable.greaterGranted"] = grantedGreater;
            if (!grantedGreater) problems.Add("unattributable: Greater Rage could not be granted");
            yield return null;
            ForcedFailedWrite(cases["conflictingDuplicate"], mutations);
            result["mutations"] = mutations;

            ExpectMutated(cases, "mutated");
        }

        private IEnumerable<object> Final()
        {
            Dictionary<string, UnitEntityData> cases = Units();
            if (cases == null) yield break;
            JArray loadCalls = ControlledRageObserver.CallsSince(0);
            result["loadCalls"] = loadCalls;
            ExpectPostLoad(loadCalls, cases["valid"], "valid", "4:0,0,4;6:0,2,4", null, "4:0,0,4;6:0,2,4", null);
            ExpectPostLoad(loadCalls, cases["malformedSibling"], "malformedSibling", "4:2,2,0;6:0,6,0", Array(MalformedSibling), "4:2,2,0;6:0,6,0", MalformedSibling);
            ExpectPostLoad(loadCalls, cases["conflictingDuplicate"], "conflictingDuplicate", null, Array(ConflictingDuplicate), null, ConflictingDuplicate);
            ExpectPostLoad(loadCalls, cases["unattributable"], "unattributable", null, Array(Unattributable), null, Unattributable);
            ExpectPostLoad(loadCalls, cases["siblingsUnknownIdentical"], "siblingsUnknownIdentical", "4:0,2,2;6:0,4,2", Array(SiblingsUnknownAndIdentical),
                "4:0,2,2;6:0,4,2", SiblingsUnknownAndIdentical);
            ExpectMutated(cases, "final");
            yield break;
        }

        private void ExpectMutated(Dictionary<string, UnitEntityData> cases, string phase)
        {
            ExpectState(cases["valid"], "valid." + phase, "4:0,0,4;6:0,2,4", null, "");
            ExpectState(cases["malformedSibling"], "malformedSibling." + phase, "4:2,2,0;6:0,6,0", MalformedSibling, "");
            ExpectState(cases["conflictingDuplicate"], "conflictingDuplicate." + phase, null, ConflictingDuplicate, "4");
            ExpectState(cases["unattributable"], "unattributable." + phase, null, Unattributable, "4,6,8");
            ExpectState(cases["siblingsUnknownIdentical"], "siblingsUnknownIdentical." + phase, "4:0,2,2;6:0,4,2", SiblingsUnknownAndIdentical, "");
            if (!cases["unattributable"].Descriptor.HasFact((BlueprintUnitFact)Set("GreaterDefault")))
                problems.Add("unattributable." + phase + ": the Greater grant is missing");
            var rage = new JObject();
            ExpectRage(cases["valid"], rage, "valid", 0, 2, 4);
            ExpectRage(cases["malformedSibling"], rage, "malformedSibling", 0, 6, 0);
            ExpectRage(cases["conflictingDuplicate"], rage, "conflictingDuplicate", 0, 0, 0);
            ExpectRage(cases["unattributable"], rage, "unattributable", 0, 0, 0);
            ExpectRage(cases["siblingsUnknownIdentical"], rage, "siblingsUnknownIdentical", 0, 4, 2);
            result["rage" + char.ToUpperInvariant(phase[0]) + phase.Substring(1)] = rage;
        }

        // ---- gameplay ----

        private IEnumerable<object> Build(UnitEntityData unit, int[] ordinary, int[] greater, JObject record, string name)
        {
            var row = new JObject();
            record[name] = row;
            if (unit.Descriptor.AddFact((BlueprintUnitFact)Set("ControlledRage")) == null) { problems.Add(name + ": Controlled Rage could not be granted"); yield break; }
            row["afterGrant"] = Snapshot(unit);
            yield return null;
            foreach (object step in Select(unit, 4, ordinary, row, "ordinary")) yield return step;
            if (greater == null) yield break;
            if (unit.Descriptor.AddFact((BlueprintUnitFact)Set("NativeGreaterRage")) == null ||
                unit.Descriptor.AddFact((BlueprintUnitFact)Set("GreaterDefault")) == null) { problems.Add(name + ": Greater Rage could not be granted"); yield break; }
            row["afterGreaterGrant"] = Snapshot(unit);
            yield return null;
            foreach (object step in Select(unit, 6, greater, row, "greater")) yield return step;
        }

        private IEnumerable<object> Select(UnitEntityData unit, int tier, int[] values, JObject record, string name)
        {
            var row = new JObject { ["tier"] = tier, ["allocation"] = Format(values) };
            record[name] = row;
            BlueprintAbility selector = (BlueprintAbility)Set(tier == 4 ? "OrdinarySelector" : tier == 6 ? "GreaterSelector" : "MightySelector");
            Ability parent = unit.Descriptor.Abilities.GetAbility(selector);
            BlueprintAbility allocation = AllocationAbility(values);
            if (parent == null || allocation == null) { problems.Add(name + ": no selector " + selector.name + " or allocation ability " + Format(values)); yield break; }
            var data = new AbilityData(allocation, unit.Descriptor) { ConvertedFrom = new AbilityData(parent) };
            row["availableBeforeUse"] = data.IsAvailable;
            unit.Commands.InterruptAll(true);
            var command = new UnitUseAbility(data, new TargetWrapper(unit));
            command.IgnoreCooldown(null);
            unit.Commands.Run(command);
            float deadline = Time.realtimeSinceStartup + 30f;
            while (!(command.IsFinished && (command.ExecutionProcess == null || command.ExecutionProcess.IsEnded)) && Time.realtimeSinceStartup < deadline)
                yield return null;
            row["commandResult"] = command.Result.ToString();
            row["finished"] = command.IsFinished;
            row["after"] = Snapshot(unit);
            string selected = (string)((JObject)row["after"])["selections"][tier.ToString(CultureInfo.InvariantCulture)];
            if (selected != Format(values)) problems.Add(name + ": the allocation ability did not select " + Format(values) + " (selected " + selected + ", command " + command.Result + ")");
        }

        private void ForcedFailedWrite(UnitEntityData unit, JObject record)
        {
            // The product's TrySelect restores its exact prior snapshot when the selection fact cannot be added.
            object allocation = Allocation(new[] { 2, 2, 0 });
            failAddFact = (BlueprintUnitFact)factsByAllocation[allocation];
            MethodInfo addFact = typeof(UnitHelper).GetMethods(BindingFlags.Static | BindingFlags.Public).Single(m => m.Name == "AddFact" && !m.IsGenericMethod);
            harmony.Patch(addFact, new HarmonyMethod(typeof(ControlledRageProbe).GetMethod(nameof(FailAddFactPrefix), BindingFlags.Static | BindingFlags.NonPublic)), null);
            addFactPatched = true;
            object part = Part(unit);
            string stateBefore = ControlledRageObserver.State(part);
            JToken rejectedBefore = ControlledRageObserver.Rejected(part);
            JObject snapshotBefore = Snapshot(unit);
            bool returned;
            try { returned = (bool)Invoke("TrySelect", unit.Descriptor, allocation); }
            finally
            {
                failAddFact = null;
                harmony.UnpatchAll(harmony.Id);
                addFactPatched = false;
            }
            var row = new JObject
            {
                ["forced"] = "UnitHelper.AddFact returns null for the ordinary STR +2 / DEX +2 selection fact",
                ["trySelectReturned"] = returned, ["stateBefore"] = stateBefore, ["rejectedBefore"] = rejectedBefore,
                ["after"] = Snapshot(unit)
            };
            record["conflictingDuplicate.forcedFailedWrite"] = row;
            if (returned) problems.Add("conflictingDuplicate: TrySelect reported success although its selection fact could not be added");
            if (ControlledRageObserver.State(part) != stateBefore || !JToken.DeepEquals(ControlledRageObserver.Rejected(part), rejectedBefore))
                problems.Add("conflictingDuplicate: the failed write did not preserve the previous persisted state");
            if (!JToken.DeepEquals(snapshotBefore, row["after"]))
                problems.Add("conflictingDuplicate: the failed write changed selections, blocked tiers, selection facts or selectors: " +
                    snapshotBefore.ToString(Newtonsoft.Json.Formatting.None) + " -> " + row["after"].ToString(Newtonsoft.Json.Formatting.None));
        }

        private static bool FailAddFactPrefix(BlueprintUnitFact blueprint, ref Fact __result)
        {
            if (failAddFact == null || !ReferenceEquals(blueprint, failAddFact)) return true;
            __result = null;
            return false;
        }

        private IEnumerable<object> Save()
        {
            bool completed = false;
            Kingmaker.EntitySystem.Persistence.SaveInfo created = Game.Instance.SaveManager.CreateNewSave(saveLabel ?? "risk03cr");
            result["savedGame"] = created.FolderName;
            Game.Instance.SaveGame(created, () => completed = true);
            float deadline = Time.realtimeSinceStartup + 120f;
            while (!(completed && !Game.Instance.SaveManager.CommitInProgress && !Kingmaker.EntitySystem.Persistence.LoadingProcess.Instance.IsLoadingInProcess) &&
                   Time.realtimeSinceStartup < deadline)
                yield return null;
            if (!completed) problems.Add("the save did not complete");
        }

        private void Arm(UnitEntityData unit, string raw, JObject record, string name)
        {
            object part = Part(unit);
            productWritten[part] = Tuple.Create(ControlledRageObserver.State(part), ControlledRageObserver.Rejected(part));
            ControlledRageObserver.Injections[part] = raw;
            injectedParts.Add(part);
            record[name] = new JObject { ["owner"] = unit.UniqueId, ["synthetic"] = raw, ["length"] = raw.Length };
        }

        // ---- checks ----

        private void ExpectState(UnitEntityData unit, string name, string state, string retained, string blocked = null)
        {
            JObject snapshot = Snapshot(unit);
            result["state." + name] = snapshot;
            if ((string)snapshot["state"] != state) problems.Add(name + ": _state " + snapshot["state"] + ", expected " + (state ?? "null"));
            JToken expectedRejected = retained == null ? (JToken)JValue.CreateNull() : Array(retained);
            if (!JToken.DeepEquals(snapshot["rejected"] ?? JValue.CreateNull(), expectedRejected))
                problems.Add(name + ": _rejectedStates " + snapshot["rejected"] + ", expected " + expectedRejected.ToString(Newtonsoft.Json.Formatting.None));
            if (blocked != null && (string)snapshot["blocked"] != blocked) problems.Add(name + ": blocked tiers " + snapshot["blocked"] + ", expected " + blocked);
            foreach (JProperty tier in ((JObject)snapshot["selectionFactsByTier"]).Properties())
                if ((int)tier.Value > 1) problems.Add(name + ": tier " + tier.Name + " holds " + tier.Value + " selection facts");
            if ((int)snapshot["selectors"] != 1) problems.Add(name + ": holds " + snapshot["selectors"] + " tier selectors");
        }

        private void ExpectPostLoad(JArray calls, UnitEntityData unit, string name, string stateBefore, JToken rejectedBefore, string stateAfter, string retained)
        {
            object part = Part(unit);
            int id = part == null ? -1 : RuntimeHelpers.GetHashCode(part);
            JObject[] own = calls.OfType<JObject>().Where(c => (string)c["kind"] == "PostLoad" && (int)c["part"] == id).ToArray();
            if (own.Length != 1) { problems.Add(name + ": " + own.Length + " native PostLoad calls observed on its part, expected 1"); return; }
            JObject call = own[0];
            if (!(bool)call["completed"]) problems.Add(name + ": the native PostLoad did not complete");
            if ((string)call["stateBefore"] != stateBefore) problems.Add(name + ": PostLoad read _state " + call["stateBefore"] + ", expected " + (stateBefore ?? "null"));
            if (!JToken.DeepEquals(call["rejectedBefore"] ?? JValue.CreateNull(), rejectedBefore ?? JValue.CreateNull()))
                problems.Add(name + ": PostLoad read _rejectedStates " + call["rejectedBefore"]);
            if ((string)call["stateAfter"] != stateAfter) problems.Add(name + ": PostLoad wrote _state " + call["stateAfter"] + ", expected " + (stateAfter ?? "null"));
            JToken expectedAfter = retained == null ? (JToken)JValue.CreateNull() : Array(retained);
            if (!JToken.DeepEquals(call["rejectedAfter"] ?? JValue.CreateNull(), expectedAfter))
                problems.Add(name + ": PostLoad wrote _rejectedStates " + call["rejectedAfter"]);
        }

        private void ExpectRage(UnitEntityData unit, JObject record, string name, int strength, int dexterity, int constitution)
        {
            var context = new MechanicsContext(unit, unit.Descriptor, (BlueprintScriptableObject)Set("ControlledRage"), null, new TargetWrapper(unit));
            Buff buff = unit.Descriptor.Buffs.AddBuff((BlueprintBuff)Set("NativeRageBuff"), context, null);
            var row = new JObject { ["substituted"] = buff != null && ReferenceEquals(buff.Blueprint, Set("RageBuff")) };
            if (buff != null)
            {
                row["strength"] = Modifiers(unit.Descriptor.Stats.Strength, buff);
                row["dexterity"] = Modifiers(unit.Descriptor.Stats.Dexterity, buff);
                row["constitution"] = Modifiers(unit.Descriptor.Stats.Constitution, buff);
                unit.Descriptor.RemoveFact(buff);
                row["removed"] = !unit.Descriptor.Buffs.RawFacts.Contains(buff) &&
                    Modifiers(unit.Descriptor.Stats.Strength, buff).Count + Modifiers(unit.Descriptor.Stats.Dexterity, buff).Count +
                    Modifiers(unit.Descriptor.Stats.Constitution, buff).Count == 0;
            }
            record[name] = row;
            string expected = Format(new[] { strength, dexterity, constitution });
            string observed = buff == null ? "no buff" : Sum(row["strength"]) + "," + Sum(row["dexterity"]) + "," + Sum(row["constitution"]);
            if (!(bool)row["substituted"]) problems.Add(name + ": native Rage was not substituted with Urban Rage");
            else if (observed != expected) problems.Add(name + ": raging granted " + observed + ", expected " + expected);
            else if (new[] { "strength", "dexterity", "constitution" }.Any(stat => ((JArray)row[stat]).Count > 1))
                problems.Add(name + ": a stat received more than one Urban Rage modifier");
            else if (!(bool)row["removed"]) problems.Add(name + ": the Urban Rage buff or a modifier remained after removal");
        }

        private static JArray Modifiers(ModifiableValue stat, Buff buff) =>
            new JArray(stat.Modifiers.Where(m => ReferenceEquals(m.Source, buff)).Select(m => (object)(m.ModValue + " " + m.ModDescriptor)).ToArray());

        private static int Sum(JToken modifiers) => modifiers.Sum(m => int.Parse(((string)m).Split(' ')[0], CultureInfo.InvariantCulture));

        private JObject Snapshot(UnitEntityData unit)
        {
            object part = Part(unit);
            var selections = new JObject();
            var blocked = new List<string>();
            var factsByTier = new JObject();
            MethodInfo isBlocked = ControlledRageObserver.PartType.GetMethod("IsBlocked", Any);
            foreach (int tier in new[] { 4, 6, 8 })
            {
                string key = tier.ToString(CultureInfo.InvariantCulture);
                object selection = part == null ? null : ControlledRageObserver.PartType.GetMethod("SelectionFor", Any).Invoke(part, new[] { Tier(tier) });
                selections[key] = selection == null ? null : Format(Values(selection));
                if (part != null && isBlocked != null && (bool)isBlocked.Invoke(part, new[] { Tier(tier) })) blocked.Add(key);
                factsByTier[key] = 0;
            }
            var facts = new JArray();
            foreach (DictionaryEntry entry in factsByAllocation)
            {
                if (!unit.Descriptor.HasFact((BlueprintUnitFact)entry.Value)) continue;
                int[] values = Values(entry.Key);
                string total = (values[0] + values[1] + values[2]).ToString(CultureInfo.InvariantCulture);
                facts.Add(total + ":" + Format(values));
                factsByTier[total] = (int)factsByTier[total] + 1;
            }
            int selectors = new[] { "OrdinarySelector", "GreaterSelector", "MightySelector" }
                .Count(name => unit.Descriptor.Abilities.GetAbility((BlueprintAbility)Set(name)) != null);
            return new JObject
            {
                ["part"] = part != null, ["state"] = ControlledRageObserver.State(part), ["rejected"] = ControlledRageObserver.Rejected(part),
                ["selections"] = selections, ["blocked"] = isBlocked == null ? "<method absent>" : string.Join(",", blocked.ToArray()),
                ["selectionFacts"] = facts, ["selectionFactsByTier"] = factsByTier, ["selectors"] = selectors
            };
        }

        // ---- product access ----

        private void Resolve()
        {
            product = ProductProbes.Entry().Assembly;
            set = product.GetType("KingmakerGunslinger.Bootstrap.BlueprintBootstrap", true).GetProperty("UrbanBarbarian", Any).GetValue(null, null)
                ?? throw new InvalidOperationException("no Urban Barbarian blueprint set (initialization did not complete or the module is off)");
            runtime = product.GetType("KingmakerGunslinger.UrbanBarbarian.ControlledRageRuntime", true);
            tierType = product.GetType("KingmakerGunslinger.UrbanBarbarian.ControlledRageTier", true);
            allocationsByAbility = (IDictionary)runtime.GetField("_allocationsByAbility", Any).GetValue(null);
            factsByAllocation = (IDictionary)runtime.GetField("_factsByAllocation", Any).GetValue(null);
        }

        private object Set(string property) => set.GetType().GetProperty(property, Any).GetValue(set, null);

        private object Tier(int value) => Enum.ToObject(tierType, value);

        private object Invoke(string method, params object[] arguments)
        {
            try { return runtime.GetMethod(method, Any).Invoke(null, arguments); }
            catch (TargetInvocationException exception) { throw exception.InnerException ?? exception; }
        }

        private static object Part(UnitEntityData unit) =>
            typeof(UnitDescriptor).GetMethod("Get").MakeGenericMethod(ControlledRageObserver.PartType).Invoke(unit.Descriptor, null);

        private static int[] Values(object allocation)
        {
            Type type = allocation.GetType();
            return new[] { "Strength", "Dexterity", "Constitution" }.Select(name => (int)type.GetProperty(name, Any).GetValue(allocation, null)).ToArray();
        }

        private object Allocation(int[] values)
        {
            foreach (object key in factsByAllocation.Keys) if (Values(key).SequenceEqual(values)) return key;
            throw new InvalidOperationException("no allocation " + Format(values));
        }

        private BlueprintAbility AllocationAbility(int[] values)
        {
            foreach (DictionaryEntry entry in allocationsByAbility) if (Values(entry.Value).SequenceEqual(values)) return (BlueprintAbility)entry.Key;
            return null;
        }

        private Dictionary<string, UnitEntityData> Units()
        {
            string path = Path.Combine(profileDir, FixtureFile);
            if (!File.Exists(path)) { problems.Add("no fixture record at " + path); return null; }
            JObject units = (JObject)JObject.Parse(File.ReadAllText(path))["units"];
            result["units"] = units;
            var cases = new Dictionary<string, UnitEntityData>();
            foreach (JProperty unit in units.Properties())
            {
                UnitEntityData found = Game.Instance.State.Units.FirstOrDefault(u => u.UniqueId == (string)unit.Value);
                if (found == null) { problems.Add(unit.Name + ": unit " + unit.Value + " is not in the loaded game"); continue; }
                cases[unit.Name] = found;
            }
            return problems.Count == 0 ? cases : null;
        }

        private static JArray Array(string value) => new JArray(value);

        private static string Format(int[] values) => values[0] + "," + values[1] + "," + values[2];
    }
}
