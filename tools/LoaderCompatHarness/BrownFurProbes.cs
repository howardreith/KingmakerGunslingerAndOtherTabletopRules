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
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KmgLoaderCompatHarness
{
    /// <summary>
    /// S6 T4 / RISK-03 (A2). Records every actual UnitPartBrownFurModifierPersistence PreSave, PostLoad and
    /// ResolveIncrease, with one global sequence number, so the first consumer of a loaded part is observable. Also records
    /// engine-logged exceptions that name Brown-Fur code. It changes nothing unless a probe arms an injection: then, only
    /// on the named parts and only after the product's own PreSave has run, it writes SYNTHETIC entries into the part so
    /// that the save file holds them.
    /// </summary>
    internal static class BrownFurObserver
    {
        public const string Condition = "observeBrownFurPersistence";
        private const BindingFlags Any = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static bool requested;
        private static int sequence;
        private static readonly Dictionary<object, JObject> open = new Dictionary<object, JObject>(new ReferenceComparer());

        public static bool Installed { get; private set; }
        public static Type PartType { get; private set; }
        public static FieldInfo RecordsField { get; private set; }
        public static FieldInfo RejectedField { get; private set; }
        public static readonly List<JObject> Calls = new List<JObject>();
        public static readonly List<string> Exceptions = new List<string>();

        /// <summary>Part instance -> SYNTHETIC (_records, _rejectedRecords) values written after the product's PreSave.</summary>
        public static readonly Dictionary<object, Tuple<object, object>> Injections =
            new Dictionary<object, Tuple<object, object>>(new ReferenceComparer());

        public static void Request() => requested = true;

        public static JToken Install(HarmonyInstance harmony)
        {
            if (!requested || Installed) return null;
            Assembly product = ProductProbes.Entry().Assembly;
            PartType = product.GetType("KingmakerGunslinger.BrownFur.UnitPartBrownFurModifierPersistence", true);
            RecordsField = PartType.GetField("_records", Any) ?? throw new MissingFieldException(PartType.FullName, "_records");
            RejectedField = PartType.GetField("_rejectedRecords", Any);
            const BindingFlags Own = BindingFlags.Static | BindingFlags.NonPublic;
            foreach (string name in new[] { "PreSave", "PostLoad" })
                harmony.Patch(PartType.GetMethod(name, BindingFlags.Instance | BindingFlags.Public),
                    new HarmonyMethod(typeof(BrownFurObserver).GetMethod(name + "Prefix", Own)),
                    new HarmonyMethod(typeof(BrownFurObserver).GetMethod(name + "Postfix", Own)));
            harmony.Patch(PartType.GetMethod("ResolveIncrease", Any),
                new HarmonyMethod(typeof(BrownFurObserver).GetMethod(nameof(ResolvePrefix), Own)),
                new HarmonyMethod(typeof(BrownFurObserver).GetMethod(nameof(ResolvePostfix), Own)));
            int exceptionHooks = 0;
            foreach (MethodInfo method in typeof(UberDebug).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(m => m.Name == "LogException" && m.GetParameters().Length >= 1 && m.GetParameters()[0].ParameterType == typeof(Exception)))
            {
                harmony.Patch(method, new HarmonyMethod(typeof(BrownFurObserver).GetMethod(nameof(ExceptionPrefix), Own)), null);
                exceptionHooks++;
            }
            Installed = true;
            return new JObject { ["part"] = PartType.FullName, ["rejectedField"] = RejectedField != null, ["exceptionHooks"] = exceptionHooks };
        }

        public static JArray CallsSince(int index) => new JArray(Calls.Skip(index).Cast<object>().ToArray());

        private static void PreSavePrefix(object __instance) => Begin("PreSave", __instance, null);
        private static void PostLoadPrefix(object __instance) => Begin("PostLoad", __instance, null);
        private static void ResolvePrefix(object __instance, object __0) => Begin("ResolveIncrease", __instance, __0);

        private static void PreSavePostfix(object __instance)
        {
            JObject call = Finish(__instance, null);
            Tuple<object, object> raw;
            if (call == null || !Injections.TryGetValue(__instance, out raw)) return;
            RecordsField.SetValue(__instance, raw.Item1);
            RejectedField?.SetValue(__instance, raw.Item2);
            call["syntheticArmed"] = true;
            call["syntheticRecords"] = Describe(raw.Item1);
            call["syntheticRejected"] = Describe(raw.Item2);
        }

        private static void PostLoadPostfix(object __instance) => Finish(__instance, null);
        private static void ResolvePostfix(object __instance, int __result) => Finish(__instance, __result);

        private static void Begin(string kind, object part, object probe)
        {
            var call = new JObject
            {
                ["seq"] = ++sequence, ["kind"] = kind, ["part"] = RuntimeHelpers.GetHashCode(part), ["owner"] = Owner(part),
                ["recordsBefore"] = Describe(RecordsField.GetValue(part)),
                ["rejectedBefore"] = RejectedField == null ? (JToken)"<field absent>" : Describe(RejectedField.GetValue(part)),
                ["completed"] = false
            };
            if (probe != null) call["probe"] = Reflect(probe);
            open[part] = call;
            if (Calls.Count < 4000) Calls.Add(call);
        }

        private static JObject Finish(object part, int? result)
        {
            JObject call;
            if (!open.TryGetValue(part, out call)) return null;
            open.Remove(part);
            call["completed"] = true;
            call["recordsAfter"] = Describe(RecordsField.GetValue(part));
            if (RejectedField != null) call["rejectedAfter"] = Describe(RejectedField.GetValue(part));
            if (result.HasValue) call["result"] = result.Value;
            return call;
        }

        private static void ExceptionPrefix(Exception __0)
        {
            string text = __0 == null ? null : __0.ToString();
            if (text != null && text.Contains("BrownFur") && Exceptions.Count < 50) Exceptions.Add(text);
        }

        /// <summary>A record list as JSON: each entry's retained raw token when it has one, otherwise its members.</summary>
        public static JToken Describe(object list)
        {
            var items = list as IEnumerable;
            if (items == null) return JValue.CreateNull();
            var result = new JArray();
            foreach (object entry in items) result.Add(entry == null ? JValue.CreateNull() : Entry(entry));
            return result;
        }

        public static JToken Entry(object entry)
        {
            PropertyInfo raw = entry.GetType().GetProperty("Raw", Any);
            var token = raw == null ? null : raw.GetValue(entry, null) as JToken;
            JObject members = Reflect(entry);
            // A rejected entry's raw text goes in as a string: it may hold numbers (1e400, beyond Int64) that the
            // runner's own JSON reader cannot parse.
            if (token is JRaw) members["$text"] = Convert.ToString(((JRaw)token).Value, CultureInfo.InvariantCulture);
            else if (token != null) members["$raw"] = token.DeepClone();
            return members;
        }

        public static JObject Reflect(object value)
        {
            var result = new JObject();
            foreach (PropertyInfo property in value.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(p => p.GetIndexParameters().Length == 0 && p.Name != "Raw"))
            {
                object item;
                try { item = property.GetValue(value, null); }
                catch (Exception exception) { item = "<" + exception.GetType().Name + ">"; }
                result[property.Name] = item == null ? JValue.CreateNull() : item is Enum ? (JToken)(item + "=" + Convert.ToInt64(item, CultureInfo.InvariantCulture)) :
                    item is string || item.GetType().IsPrimitive ? JToken.FromObject(item) : (JToken)item.ToString();
            }
            return result;
        }

        private static string Owner(object part)
        {
            try { return (part as UnitPart)?.Owner?.Unit?.UniqueId ?? "unknown"; }
            catch (Exception exception) { return "<" + exception.GetType().Name + ">"; }
        }

        private sealed class ReferenceComparer : IEqualityComparer<object>
        {
            public new bool Equals(object x, object y) => ReferenceEquals(x, y);
            public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
        }
    }

    /// <summary>
    /// The disposable Brown-Fur persistence fixture (S6 A2). A real level-five CotW Arcanist with the Brown-Fur Powerful
    /// Change and Share Transmutation features (spawned from the registered default player blueprint, so it deserializes)
    /// casts Bull's Strength through UnitUseAbility with the Strength toggle on; the product's own committed cast path
    /// records the enhanced modifier. Characterization modes ("characterize-create", "characterize-reload") record what
    /// the engine serializer (DefaultJsonSettings, Newtonsoft 8) does with SYNTHETIC variants of that real record, and the
    /// order of the first consumer after a fresh-process load.
    /// </summary>
    internal sealed partial class BrownFurProbe : IOwnedProbe
    {
        private const BindingFlags Any = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const string FixtureFile = "brown-fur-persistence-fixture.json";
        private const string BullsStrength = "4c3d08935262b6544ae97599b3a9556d";
        private const string BullsStrengthBuff = "b175001b42b1a02479881b72fe132116";
        private const string CatsGrace = "de7a025d48ad5da4991e7d3c682cf69d";

        private readonly string profileDir;
        private readonly string mode;
        private readonly string saveLabel;
        private readonly JObject result = new JObject();
        private readonly List<string> problems = new List<string>();
        private readonly IEnumerator<object> script;
        private readonly HarmonyInstance harmony = HarmonyInstance.Create("KmgLoaderCompatHarness.brown-fur-probe");
        private readonly List<Action> cleanups = new List<Action>();
        private string verdict;

        private Assembly product;
        private object resolution, contract, blueprints;
        private Type intentRuntime, executionRuntime;

        public BrownFurProbe(string profileDir, string mode, IDictionary<string, string> invariants)
        {
            this.profileDir = profileDir;
            this.mode = mode;
            string label;
            saveLabel = invariants != null && invariants.TryGetValue("saveLabel", out label) ? label : null;
            result["mode"] = mode;
            result["syntheticNotice"] = "Every malformed, duplicate or unsupported Brown-Fur record in this probe is SYNTHETIC test input in a disposable fixture.";
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
            result["exceptionsNamingBrownFur"] = new JArray(BrownFurObserver.Exceptions.Cast<object>().ToArray());
            verdict = problems.Count == 0 ? string.Empty : string.Join(" | ", problems.ToArray());
            return verdict;
        }

        public CleanupReport Release(string testFailure)
        {
            var releases = cleanups.Select((action, index) => new KeyValuePair<string, Action>("cleanup-" + index, action)).ToList();
            releases.Add(new KeyValuePair<string, Action>("harmony", () => harmony.UnpatchAll(harmony.Id)));
            releases.Add(new KeyValuePair<string, Action>("injections", () => BrownFurObserver.Injections.Clear()));
            return ProbeCleanup.RunAll(releases);
        }

        private IEnumerable<object> Run()
        {
            if (!BrownFurObserver.Installed) { problems.Add("the persistence observer is not installed (condition " + BrownFurObserver.Condition + ")"); yield break; }
            product = ProductProbes.Entry().Assembly;
            result["exceptionsAtStart"] = BrownFurObserver.Exceptions.Count;
            IEnumerable<object> body;
            switch (mode)
            {
                case "characterize-create": body = CharacterizeCreate(); break;
                case "characterize-reload": body = CharacterizeReload(); break;
                default: body = Qualification(); break;
            }
            foreach (object step in body) yield return step;
            if (BrownFurObserver.Exceptions.Count != 0)
                problems.Add(BrownFurObserver.Exceptions.Count + " engine-logged exception(s) name Brown-Fur code");
        }

        // ---- characterization ----

        private IEnumerable<object> CharacterizeCreate()
        {
            result["serializerSettings"] = SettingsDescription();
            foreach (object step in AwaitBrownFur()) yield return step;
            if (!ResolveBrownFur()) yield break;
            UnitEntityData caster, target;
            if (!SpawnCaster(out caster) || !SpawnTarget("characterizationTarget", 0, out target)) yield break;
            result["units"] = new JObject { ["caster"] = caster.UniqueId, ["target"] = target.UniqueId };
            yield return null;
            JObject cast = CastAbilityBonus(caster, target, BullsStrength, StatType.Strength, 0, 6);
            result["realCast"] = cast;
            if (problems.Count != 0) yield break;
            object part = Part(target);
            result["recordsAfterCast"] = BrownFurObserver.Describe(BrownFurObserver.RecordsField.GetValue(part));
            File.WriteAllText(Path.Combine(profileDir, FixtureFile), new JObject { ["units"] = result["units"] }.ToString());

            int callIndex = BrownFurObserver.Calls.Count;
            foreach (object step in Save()) yield return step;
            result["saveCalls"] = BrownFurObserver.CallsSince(callIndex);

            JToken written = WrittenRecords(target.UniqueId, caster.UniqueId);
            result["writtenRecordsFromFile"] = written;
            var array = written as JArray;
            if (array == null || array.Count != 1 || !(array[0] is JObject))
            {
                problems.Add("the saved file did not hold exactly one record object for the target: " + (written?.ToString(Formatting.None) ?? "null"));
                yield break;
            }
            result["serializerMatrix"] = SerializerMatrix((JObject)array[0]);
        }

        private IEnumerable<object> CharacterizeReload()
        {
            Dictionary<string, UnitEntityData> units = Units();
            if (units == null) yield break;
            UnitEntityData target = units["target"];
            object part = Part(target);
            int id = part == null ? -1 : RuntimeHelpers.GetHashCode(part);
            JObject[] own = BrownFurObserver.Calls.Where(c => (int)c["part"] == id).ToArray();
            result["loadCallsForTarget"] = new JArray(own.Cast<object>().ToArray());
            JObject firstResolve = own.FirstOrDefault(c => (string)c["kind"] == "ResolveIncrease");
            JObject postLoad = own.FirstOrDefault(c => (string)c["kind"] == "PostLoad");
            result["firstConsumer"] = own.Length == 0 ? "none" : (string)own[0]["kind"];
            result["resolveBeforePostLoad"] = firstResolve != null && postLoad != null && (int)firstResolve["seq"] < (int)postLoad["seq"];
            result["targetStrength"] = Modifiers(target, StatType.Strength, BullsStrength);
            yield break;
        }

        private JObject SettingsDescription()
        {
            JsonSerializerSettings settings = DefaultJsonSettings.DefaultSettings;
            return new JObject
            {
                ["newtonsoft"] = typeof(JsonSerializer).Assembly.GetName().Version.ToString(),
                ["contractResolver"] = settings.ContractResolver?.GetType().FullName,
                ["typeNameHandling"] = settings.TypeNameHandling.ToString(),
                ["preserveReferencesHandling"] = settings.PreserveReferencesHandling.ToString(),
                ["missingMemberHandling"] = settings.MissingMemberHandling.ToString(),
                ["errorHandler"] = settings.Error != null,
                ["converters"] = new JArray(settings.Converters.Select(c => (object)c.GetType().FullName).ToArray())
            };
        }

        /// <summary>
        /// SYNTHETIC variants of the real written record, each deserialized into the product's part type by a serializer
        /// built from the engine's own DefaultJsonSettings (as AreaDataStash and ThreadedGameLoader build theirs), then read
        /// through the owner's Count (its whole-list validation) and written back by the same serializer.
        /// </summary>
        private JObject SerializerMatrix(JObject written)
        {
            var variants = new List<KeyValuePair<string, JToken>>();
            Func<Action<JObject>, JObject> vary = edit =>
            {
                var copy = (JObject)written.DeepClone();
                copy.Remove("$id");
                edit(copy);
                return copy;
            };
            variants.Add(Pair("valid-as-written", new JArray(written.DeepClone())));
            variants.Add(Pair("valid-without-id", new JArray(vary(r => { }))));
            variants.Add(Pair("unknown-member", new JArray(vary(r => r["FutureField"] = new JObject { ["nested"] = new JArray(1, "two", true, null, 1.5) }))));
            variants.Add(Pair("schema-2", new JArray(vary(r => r["SchemaVersion"] = 2))));
            variants.Add(Pair("ability-name-unknown", new JArray(vary(r => r["AbilityScore"] = "Strenght"))));
            variants.Add(Pair("ability-name-lowercase", new JArray(vary(r => r["AbilityScore"] = ((string)r["AbilityScore"] ?? string.Empty).ToLowerInvariant()))));
            variants.Add(Pair("ability-number-undefined", new JArray(vary(r => r["AbilityScore"] = 99))));
            variants.Add(Pair("ability-number-defined", new JArray(vary(r => r["AbilityScore"] = 1))));
            variants.Add(Pair("increase-string", new JArray(vary(r => r["Increase"] = "four"))));
            variants.Add(Pair("ticks-float", new JArray(vary(r => r["EndTimeTicks"] = 1.5))));
            variants.Add(Pair("member-missing", new JArray(vary(r => r.Remove("OriginalDescriptor")))));
            variants.Add(Pair("element-string", new JArray("garbage")));
            variants.Add(Pair("element-null", new JArray(JValue.CreateNull())));
            variants.Add(Pair("container-object", new JObject { ["unexpected"] = true }));
            variants.Add(Pair("duplicate-exact", new JArray(vary(r => { }), vary(r => { }))));
            variants.Add(Pair("duplicate-conflicting", new JArray(vary(r => { }), vary(r => r["Increase"] = 4))));
            var matrix = new JObject();
            foreach (KeyValuePair<string, JToken> variant in variants)
                matrix[variant.Key] = Deserialize(new JObject { ["_records"] = variant.Value });
            return matrix;
        }

        private static KeyValuePair<string, JToken> Pair(string name, JToken token) => new KeyValuePair<string, JToken>(name, token);

        private JObject Deserialize(JObject document)
        {
            var row = new JObject { ["input"] = document.ToString(Formatting.None) };
            JsonSerializer serializer = JsonSerializer.Create(DefaultJsonSettings.DefaultSettings);
            object part;
            try
            {
                using (var reader = new JsonTextReader(new StringReader(document.ToString(Formatting.None))))
                    part = serializer.Deserialize(reader, BrownFurObserver.PartType);
            }
            catch (Exception exception)
            {
                row["deserialize"] = "threw " + exception.GetType().FullName + ": " + Clip(exception.Message);
                return row;
            }
            row["deserialize"] = part == null ? "null" : "ok";
            if (part == null) return row;
            row["records"] = BrownFurObserver.Describe(BrownFurObserver.RecordsField.GetValue(part));
            if (BrownFurObserver.RejectedField != null) row["rejected"] = BrownFurObserver.Describe(BrownFurObserver.RejectedField.GetValue(part));
            try { row["ownerCount"] = (int)BrownFurObserver.PartType.GetProperty("Count", Any).GetValue(part, null); }
            catch (TargetInvocationException exception)
            {
                Exception inner = exception.InnerException ?? exception;
                row["ownerCount"] = "threw " + inner.GetType().FullName + ": " + Clip(inner.Message);
            }
            try
            {
                var text = new StringWriter(CultureInfo.InvariantCulture);
                serializer.Serialize(text, part, BrownFurObserver.PartType);
                row["reserialized"] = text.ToString();
            }
            catch (Exception exception) { row["reserialized"] = "threw " + exception.GetType().FullName + ": " + Clip(exception.Message); }
            return row;
        }

        private static string Clip(string text) => text == null ? null : text.Length <= 300 ? text : text.Substring(0, 300) + "...";

        /// <summary>The target's _records array exactly as the game wrote it into the save file.</summary>
        private JToken WrittenRecords(string targetId, string casterId)
        {
            string path = Directory.GetFiles(Game.Instance.SaveManager.SavePath, "*_" + saveLabel + ".zks")
                .OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault();
            if (path == null) { problems.Add("the written save " + saveLabel + " was not found"); return null; }
            result["writtenSave"] = Path.GetFileName(path);
            // The engine's own (internal) zip saver, through reflection.
            Type saverType = typeof(SaveManager).Assembly.GetType("Kingmaker.EntitySystem.Persistence.ZipSaver", true);
            object saver = Activator.CreateInstance(saverType, path);
            MethodInfo readJson = saverType.GetMethod("ReadJson", Any);
            try
            {
                var files = (IEnumerable<string>)saverType.GetMethod("GetAllFiles", Any).Invoke(saver, null);
                foreach (string entry in files.Where(n => n.EndsWith(".json", StringComparison.Ordinal)).ToArray())
                {
                    var text = (string)readJson.Invoke(saver, new object[] { entry.Substring(0, entry.Length - 5) });
                    if (text == null) continue;
                    for (int index = text.IndexOf("\"_records\":", StringComparison.Ordinal); index >= 0;
                         index = text.IndexOf("\"_records\":", index + 1, StringComparison.Ordinal))
                    {
                        var reader = new JsonTextReader(new StringReader(text.Substring(index + "\"_records\":".Length)))
                        { DateParseHandling = DateParseHandling.None, FloatParseHandling = FloatParseHandling.Decimal };
                        JToken token = JToken.ReadFrom(reader);
                        if (token.ToString(Formatting.None).Contains(casterId))
                        {
                            result["writtenEntry"] = entry;
                            return token;
                        }
                    }
                }
            }
            finally { saverType.GetMethod("Dispose", Any).Invoke(saver, null); }
            return null;
        }

        // ---- fixture: real Brown-Fur cast ----

        /// <summary>Brown-Fur publishes after Call of the Wild's library load; wait (bounded) for its resolution.</summary>
        private IEnumerable<object> AwaitBrownFur()
        {
            Type coordinator = product.GetType("KingmakerGunslinger.BrownFur.BrownFurOptionalExtensionCoordinator", true);
            float deadline = Time.realtimeSinceStartup + 90f;
            while (Time.realtimeSinceStartup < deadline && (coordinator.GetProperty("Current", Any).GetValue(null, null) == null ||
                       coordinator.GetProperty("Blueprints", Any).GetValue(null, null) == null))
                yield return null;
        }

        private bool ResolveBrownFur()
        {
            Type coordinator = product.GetType("KingmakerGunslinger.BrownFur.BrownFurOptionalExtensionCoordinator", true);
            resolution = coordinator.GetProperty("Current", Any).GetValue(null, null);
            blueprints = coordinator.GetProperty("Blueprints", Any).GetValue(null, null);
            object decision = resolution?.GetType().GetProperty("Decision", Any).GetValue(resolution, null);
            bool compatible = decision != null && (bool)decision.GetType().GetProperty("IsCompatible", Any).GetValue(decision, null);
            contract = resolution?.GetType().GetProperty("Contract", Any).GetValue(resolution, null);
            intentRuntime = product.GetType("KingmakerGunslinger.BrownFur.BrownFurPlayerIntentRuntime", true);
            executionRuntime = product.GetType("KingmakerGunslinger.BrownFur.BrownFurCastExecutionRuntime", true);
            result["brownFur"] = new JObject { ["compatible"] = compatible, ["blueprints"] = blueprints != null, ["contract"] = contract != null };
            if (!compatible || contract == null || blueprints == null) { problems.Add("Brown-Fur is not published (CotW contract or blueprints unavailable)"); return false; }
            return true;
        }

        private object C(string name) => contract.GetType().GetProperty(name, Any).GetValue(contract, null);
        private object B(string name) => blueprints.GetType().GetProperty(name, Any).GetValue(blueprints, null);

        private bool SpawnCaster(out UnitEntityData caster)
        {
            caster = Spawn(new Vector3(2f, 0f, 2f));
            if (caster == null) { problems.Add("the game did not spawn the disposable caster"); return false; }
            caster.Descriptor.Stats.Intelligence.BaseValue = 30;
            caster.Descriptor.Stats.HitPoints.BaseValue = 10000;
            Advance(caster.Descriptor, (BlueprintCharacterClass)C("ArcanistClass"), 5);
            Spellbook casting = Book(caster, "CastingSpellbook"), preparation = Book(caster, "MemorizationSpellbook");
            if (casting == null || preparation == null) { problems.Add("the disposable Arcanist did not own both resolved spellbooks"); return false; }
            int level = caster.Descriptor.Progression.GetClassLevel((BlueprintCharacterClass)C("ArcanistClass"));
            foreach (Spellbook book in new[] { casting, preparation })
            {
                while (book.CasterLevel < level) book.AddCasterLevel();
                book.UpdateAllSlotsSize(false);
                book.Rest();
            }
            caster.Descriptor.Resources.Add((BlueprintAbilityResource)C("Reservoir"), true);
            foreach (string feature in new[] { "PowerfulChange", "ShareTransmutation" })
            {
                var blueprint = (BlueprintUnitFact)B(feature);
                if (!caster.Descriptor.HasFact(blueprint) && caster.Descriptor.AddFact(blueprint) == null)
                { problems.Add("Brown-Fur feature " + feature + " could not be granted"); return false; }
            }
            result["casterLevel"] = level;
            return true;
        }

        private bool SpawnTarget(string name, int index, out UnitEntityData target)
        {
            target = Spawn(new Vector3(2.5f + 0.75f * index, 0f, 2.5f));
            if (target == null) { problems.Add("the game did not spawn disposable target " + name); return false; }
            target.Descriptor.Stats.HitPoints.BaseValue = 10000;
            return true;
        }

        private UnitEntityData Spawn(Vector3 offset)
        {
            UnitEntityData main = Game.Instance.Player.MainCharacter.Value;
            UnitEntityData unit = Game.Instance.EntityCreator.SpawnUnit(BlueprintRoot.Instance.DefaultPlayerCharacter,
                main.Position + offset, Quaternion.identity, Game.Instance.State.LoadedAreaState.MainState);
            if (unit == null || unit.View == null) return null;
            if (!unit.Descriptor.IsTurnedOn) unit.Descriptor.TurnOn();
            unit.IsBrainActive = false;
            return unit;
        }

        private Spellbook Book(UnitEntityData caster, string property)
        {
            var blueprint = (BlueprintSpellbook)C(property);
            return caster.Descriptor.Spellbooks.SingleOrDefault(b => b != null && ReferenceEquals(b.Blueprint, blueprint));
        }

        private static void Advance(UnitDescriptor owner, BlueprintCharacterClass characterClass, int levels)
        {
            Type type = typeof(Kingmaker.UnitLogic.Class.LevelUp.LevelUpController);
            MethodInfo start = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                .Single(m => m.Name == "StartWithoutAssigningStaticInstance" && m.GetParameters().Length == 5);
            MethodInfo select = type.GetMethod("SelectClass", BindingFlags.Public | BindingFlags.Instance, null,
                new[] { typeof(BlueprintCharacterClass), typeof(bool) }, null);
            MethodInfo mechanics = type.GetMethod("ApplyClassMechanics", BindingFlags.Public | BindingFlags.Instance);
            MethodInfo apply = type.GetMethod("ApplyLevelup", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo cancel = type.GetMethod("Cancel", BindingFlags.Public | BindingFlags.Instance);
            object charGen = Enum.Parse(start.GetParameters()[4].ParameterType, "CharGen", false);
            for (int index = 0; index < levels; index++)
            {
                object controller = start.Invoke(null, new object[] { owner, false, null, null, charGen });
                if (!(bool)select.Invoke(controller, new object[] { characterClass, false }))
                    throw new InvalidOperationException("Arcanist class selection failed at level " + (index + 1));
                mechanics.Invoke(controller, null);
                apply.Invoke(controller, new object[] { owner });
                cancel.Invoke(controller, null);
            }
        }

        /// <summary>
        /// The product's real committed cast path: a known arcanist spell cast through UnitUseAbility with the score
        /// toggle on (armed) or off. Returns the observed typed modifier; the buff stays on the target.
        /// </summary>
        private JObject CastAbilityBonus(UnitEntityData caster, UnitEntityData target, string spellGuid, StatType stat, int scoreIndex,
            int expectedModifier, bool armed = true)
        {
            var row = new JObject { ["spell"] = spellGuid, ["target"] = target.UniqueId, ["armed"] = armed };
            var spell = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>(spellGuid);
            Spellbook casting = Book(caster, "CastingSpellbook");
            var reservoir = (BlueprintAbilityResource)C("Reservoir");
            if (spell == null || spell.School != SpellSchool.Transmutation || casting == null) { problems.Add("cast fixture unavailable for " + spellGuid); return row; }
            // Both resolved books are rested and know the spell, as in the product's BrownFurNativeCastScenario.
            Spellbook preparation = Book(caster, "MemorizationSpellbook");
            caster.Commands.InterruptAll(true);
            // Touch spells: stand within reach, since the synchronous drive cannot walk the caster there.
            caster.Position = target.Position + new Vector3(0.6f, 0f, 0f);
            row["distance"] = Vector3.Distance(caster.Position, target.Position);
            foreach (Spellbook book in new[] { casting, preparation })
            {
                if (book == null) continue;
                book.Rest();
                if (!book.IsKnown(spell)) book.AddKnown(2, spell, true);
            }
            int reservoirNow = caster.Descriptor.Resources.GetResourceAmount(reservoir);
            if (reservoirNow < 4) caster.Descriptor.Resources.Restore(reservoir, 4 - reservoirNow);
            intentRuntime.GetMethod("Clear", Any).Invoke(null, new[] { caster.Descriptor, blueprints });
            var toggles = (BlueprintActivatableAbility[])B("ScoreActivatables");
            var toggle = (ActivatableAbility)intentRuntime.GetMethod("Find", Any).Invoke(null, new object[] { caster.Descriptor, toggles[scoreIndex] });
            if (toggle == null) { problems.Add("the Powerful Change toggle " + scoreIndex + " is unavailable"); return row; }
            toggle.IsOn = armed;
            var data = new AbilityData(spell, casting);
            var castTarget = new TargetWrapper(target);
            row["targetable"] = data.CanTarget(castTarget);
            row["reservoirBefore"] = caster.Descriptor.Resources.GetResourceAmount(reservoir);
            UnitUseAbility command;
            var cutscene = new Kingmaker.AreaLogic.Cutscenes.CutsceneParametersContext();
            using (cutscene.Data) command = new UnitUseAbility(data, castTarget);
            row["transactionsAfterCommand"] = (int)executionRuntime.GetProperty("ActiveTransactionCount", Any).GetValue(null, null);
            command.IgnoreCooldown(TimeSpan.Zero);
            caster.Commands.Run(command);
            command.Start();
            if (!command.IsRunning)
            {
                row["availableBeforeStart"] = data.IsAvailable;
                row["canStart"] = command.CanStart;
                row["commandResult"] = command.Result.ToString();
                row["castingSlots"] = casting.Blueprint.Spontaneous ? casting.GetSpontaneousSlots(2) : -1;
                problems.Add("the cast command did not start for " + spellGuid + ": " + row.ToString(Formatting.None));
                return row;
            }
            if (command.Animation != null) command.Animation.IsActed = true;
            command.Tick();
            if (command.ExecutionProcess == null) { problems.Add("the cast did not create an execution process; result " + command.Result); return row; }
            for (int tick = 0; tick < 5000 && !command.ExecutionProcess.IsEnded; tick++) command.ExecutionProcess.Tick();
            FinishAnimation(command);
            if (!command.IsFinished) command.Tick();
            row["commandResult"] = command.Result.ToString();
            row["transactionTerminal"] = (string)executionRuntime.GetProperty("LastTerminalState", Any).GetValue(null, null)?.ToString();
            row["reservoirAfter"] = caster.Descriptor.Resources.GetResourceAmount(reservoir);
            JObject observed = Modifiers(target, stat, spellGuid);
            row["modifier"] = observed;
            if ((int)observed["count"] != 1 || (int)observed["value"] != expectedModifier || (string)observed["descriptor"] != "Enhancement")
                problems.Add("cast " + spellGuid + " on " + target.UniqueId + " produced " + observed.ToString(Formatting.None) + ", expected one +" + expectedModifier + " Enhancement");
            return row;
        }

        private static void FinishAnimation(UnitUseAbility command)
        {
            if (command == null || command.Animation == null) return;
            PropertyInfo property = null;
            for (Type type = command.Animation.GetType(); type != null && property == null; type = type.BaseType)
                property = type.GetProperty("IsFinished", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            property?.GetSetMethod(true)?.Invoke(command.Animation, new object[] { true });
        }

        /// <summary>The target's modifiers on one stat whose source is a buff from the named spell.</summary>
        private static JObject Modifiers(UnitEntityData unit, StatType stat, string spellGuid)
        {
            ModifiableValue value = unit.Descriptor.Stats.GetStat(stat);
            ModifiableValue.Modifier[] modifiers = value.Modifiers.Where(m => m.Source is Buff && ((Buff)m.Source).Context != null &&
                ((Buff)m.Source).Context.SourceAbility != null && ((Buff)m.Source).Context.SourceAbility.AssetGuid == spellGuid).ToArray();
            ModifiableValue.Modifier first = modifiers.FirstOrDefault();
            Buff buff = first == null ? null : (Buff)first.Source;
            return new JObject
            {
                ["count"] = modifiers.Length, ["value"] = first?.ModValue ?? 0, ["descriptor"] = first?.ModDescriptor.ToString(),
                ["sourceComponent"] = first?.SourceComponent, ["statValue"] = value.ModifiedValue, ["statBase"] = value.BaseValue,
                ["buff"] = buff?.Blueprint.AssetGuid, ["buffEndTicks"] = buff == null ? 0L : buff.EndTime.Ticks,
                ["caster"] = buff?.Context?.MaybeCaster?.UniqueId
            };
        }

        private IEnumerable<object> Save()
        {
            bool completed = false;
            SaveInfo created = Game.Instance.SaveManager.CreateNewSave(saveLabel ?? "risk03bf");
            result["savedGame"] = created.FolderName;
            Game.Instance.SaveGame(created, () => completed = true);
            float deadline = Time.realtimeSinceStartup + 120f;
            while (!(completed && !Game.Instance.SaveManager.CommitInProgress && !LoadingProcess.Instance.IsLoadingInProcess) &&
                   Time.realtimeSinceStartup < deadline)
                yield return null;
            if (!completed) problems.Add("the save did not complete");
        }

        private static object Part(UnitEntityData unit) =>
            typeof(UnitDescriptor).GetMethod("Get").MakeGenericMethod(BrownFurObserver.PartType).Invoke(unit.Descriptor, null);

        private Dictionary<string, UnitEntityData> Units()
        {
            string path = Path.Combine(profileDir, FixtureFile);
            if (!File.Exists(path)) { problems.Add("no fixture record at " + path); return null; }
            var units = (JObject)JObject.Parse(File.ReadAllText(path))["units"];
            result["units"] = units;
            var found = new Dictionary<string, UnitEntityData>();
            foreach (JProperty unit in units.Properties())
            {
                UnitEntityData value = Game.Instance.State.Units.FirstOrDefault(u => u.UniqueId == (string)unit.Value);
                if (value == null) { problems.Add(unit.Name + ": unit " + unit.Value + " is not in the loaded game"); continue; }
                found[unit.Name] = value;
            }
            return problems.Count == 0 ? found : null;
        }
    }
}
