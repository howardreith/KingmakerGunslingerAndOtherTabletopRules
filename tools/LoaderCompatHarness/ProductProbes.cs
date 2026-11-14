using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Root;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityModManagerNet;

namespace KmgLoaderCompatHarness
{
    /// <summary>
    /// Reads Tabletop Rules Expansion (mod Id KingmakerGunslinger) state by reflection, so the harness has no compile-time
    /// dependency on the candidate and observes exactly the bytes the loader loaded. A missing member fails the step.
    /// </summary>
    internal static class ProductProbes
    {
        public const string ProductId = "KingmakerGunslinger";
        private const BindingFlags Any = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public static UnityModManager.ModEntry Entry()
        {
            UnityModManager.ModEntry entry = UnityModManager.modEntries.SingleOrDefault(e => e.Info != null && e.Info.Id == ProductId);
            if (entry == null) throw new InvalidOperationException("the loader has no entry for " + ProductId);
            return entry;
        }

        private static Assembly ProductAssembly()
        {
            UnityModManager.ModEntry entry = Entry();
            if (entry.Assembly == null) throw new InvalidOperationException(ProductId + " has no loaded assembly");
            return entry.Assembly;
        }

        private static object Read(Type type, string member, object instance = null)
        {
            FieldInfo field = type.GetField(member, Any);
            if (field != null) return field.GetValue(instance);
            PropertyInfo property = type.GetProperty(member, Any);
            if (property != null) return property.GetValue(instance, null);
            throw new MissingMemberException(type.FullName, member);
        }

        private static object Context()
        {
            Type context = ProductAssembly().GetType("KingmakerGunslinger.Bootstrap.ModContext", true);
            object current = Read(context, "_current");
            if (current == null) throw new InvalidOperationException("the product published no mod context");
            return current;
        }

        /// <summary>
        /// Bootstrap completed exactly once and without failure; the product's Harmony 1.2 owner holds patches; each named
        /// blueprint resolves; and each named character class is published exactly once (no duplicate content).
        /// </summary>
        public static JObject Bootstrap(IEnumerable<string> requiredGuids, IEnumerable<string> uniqueClassGuids)
        {
            UnityModManager.ModEntry entry = Entry();
            if (!entry.Active || !entry.Loaded || entry.ErrorOnLoading)
                throw new InvalidOperationException(ProductId + " entry: active=" + entry.Active + " loaded=" + entry.Loaded + " error=" + entry.ErrorOnLoading);

            Assembly product = ProductAssembly();
            string loaderState = Read(product.GetType("KingmakerGunslinger.Main", true), "_state").ToString();
            if (loaderState != "Loaded") throw new InvalidOperationException("Main.Load state is " + loaderState);
            object context = Context();
            if ((bool)Read(context.GetType(), "IsFailed", context)) throw new InvalidOperationException("the product context is failed");
            bool initialized = (bool)Read(product.GetType("KingmakerGunslinger.Bootstrap.BlueprintBootstrap", true), "IsInitialized");
            if (!initialized) throw new InvalidOperationException("BlueprintBootstrap.IsInitialized is false");

            HarmonyInstance probe = HarmonyInstance.Create("KmgLoaderCompatHarness.patch-census");
            int owned = 0;
            int patchedMethods = 0;
            foreach (MethodBase method in probe.GetPatchedMethods())
            {
                Patches info = probe.GetPatchInfo(method);
                if (info == null) continue;
                patchedMethods++;
                if (info.Owners.Contains(ProductId)) owned++;
            }

            if (owned == 0) throw new InvalidOperationException("no Harmony 1.2 patch is owned by " + ProductId);

            JObject result = new JObject
            {
                ["mainState"] = loaderState,
                ["blueprintBootstrapInitialized"] = initialized,
                ["harmony12MethodsPatchedByProduct"] = owned,
                ["harmony12MethodsPatchedTotal"] = patchedMethods
            };

            JArray resolved = new JArray();
            foreach (string guid in requiredGuids.Where(g => !string.IsNullOrWhiteSpace(g)))
            {
                BlueprintScriptableObject blueprint = ResourcesLibrary.TryGetBlueprint(guid.Trim());
                if (blueprint == null) throw new InvalidOperationException("required blueprint does not resolve: " + guid);
                resolved.Add(new JObject { ["guid"] = guid.Trim(), ["name"] = blueprint.name, ["type"] = blueprint.GetType().Name });
            }

            result["resolved"] = resolved;
            JArray classes = new JArray();
            BlueprintCharacterClass[] published = BlueprintRoot.Instance.Progression.CharacterClasses;
            foreach (string guid in uniqueClassGuids.Where(g => !string.IsNullOrWhiteSpace(g)))
            {
                int count = published.Count(c => c != null && c.AssetGuid == guid.Trim());
                classes.Add(new JObject { ["guid"] = guid.Trim(), ["publishedCount"] = count });
                if (count != 1) throw new InvalidOperationException("character class " + guid + " is published " + count + " times");
            }

            result["characterClasses"] = classes;
            result["characterClassTotal"] = published.Length;
            return result;
        }

        /// <summary>The live Active and Pending module configuration and the persisted FeatureModules.json.</summary>
        public static JObject ModuleState()
        {
            object context = Context();
            object state = Read(context.GetType(), "FeatureModules", context);
            JObject result = new JObject
            {
                ["active"] = Configuration(Read(state.GetType(), "Active", state)),
                ["pending"] = Configuration(Read(state.GetType(), "Pending", state)),
                ["restartRequired"] = (bool)Read(state.GetType(), "RestartRequired", state)
            };
            string file = Path.Combine(Entry().Path, "FeatureModules.json");
            result["file"] = File.Exists(file) ? JObject.Parse(File.ReadAllText(file)) : null;
            result["fileSha256"] = File.Exists(file) ? LoaderIdentity.Sha256(file) : null;
            return result;
        }

        private static JObject Configuration(object configuration)
        {
            JObject values = new JObject();
            foreach (PropertyInfo property in configuration.GetType().GetProperties(Any).Where(p => p.PropertyType == typeof(bool)))
            {
                values[property.Name] = (bool)property.GetValue(configuration, null);
            }

            return values;
        }

        /// <summary>Each "Name=bool" pair must hold in the live Active configuration; every unnamed module must be true.</summary>
        public static void AssertActiveModules(JObject state, string expectations)
        {
            JObject active = (JObject)state["active"];
            Dictionary<string, bool> expected = expectations.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(pair => pair.Split('='))
                .ToDictionary(pair => pair[0].Trim(), pair => bool.Parse(pair[1].Trim()));
            foreach (JProperty property in active.Properties())
            {
                if (property.Name == "ShowOriginalSummonElemental") continue;
                bool want = expected.TryGetValue(property.Name, out bool named) ? named : true;
                if ((bool)property.Value != want)
                    throw new InvalidOperationException("module " + property.Name + " is " + property.Value + " in this process, expected " + want);
            }

            foreach (string name in expected.Keys)
            {
                if (active[name] == null) throw new InvalidOperationException("no module property named " + name);
            }
        }
    }

    /// <summary>
    /// Simulates exactly one user click on a named toggle in the product's own settings GUI: the next real IMGUI call to
    /// ImmediateModeGui.Toggle with that label returns the inverted value, as GUILayout.Toggle does when clicked. The
    /// product's own Draw then records it through SetPending; nothing else in the product is touched.
    /// </summary>
    internal static class ToggleClick
    {
        private static string pendingLabel;

        public static string Clicked { get; private set; }

        public static void Install(HarmonyInstance harmony)
        {
            Type gui = ProductProbes.Entry().Assembly.GetType("KingmakerGunslinger.Development.ImmediateModeGui", true);
            MethodInfo toggle = gui.GetMethod("Toggle", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, null,
                new[] { typeof(bool), typeof(string) }, null);
            if (toggle == null) throw new MissingMethodException(gui.FullName, "Toggle(bool,string)");
            harmony.Patch(toggle, null, new HarmonyMethod(typeof(ToggleClick).GetMethod(nameof(Postfix), BindingFlags.Static | BindingFlags.NonPublic)));
        }

        public static void Arm(string label)
        {
            pendingLabel = label;
            Clicked = null;
        }

        private static void Postfix(bool value, string text, ref bool __result)
        {
            if (pendingLabel == null || text != pendingLabel || Event.current == null || Event.current.type != EventType.Repaint) return;
            __result = !value;
            Clicked = text + ": " + value + " -> " + __result;
            pendingLabel = null;
        }
    }
}
