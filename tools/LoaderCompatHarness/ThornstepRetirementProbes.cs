using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.DirectSerialization;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Loot;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Items;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KmgLoaderCompatHarness
{
    /// <summary>
    /// Sprint 6 D3 / LIM-03 test-only fixture construction. The fixture area has no loot container, so the seed launch
    /// spawns the engine's own dropped-loot bags (real MapObjectView + LootComponent, saved and re-attached by
    /// MapObjectEntityData.EnsureComponentData on load) and gives each a constructed LootPersistentData. In later
    /// launches, condition "thornstepInducedSource" INDUCES the exact former-source reference: a first-priority prefix
    /// on LootComponent.OnDataSetted sets the named bags' LootTables to Forest_GoodLoot01 (3322c56f…) before the
    /// product's prefix and the native UpdateLoot run, and snapshots the loaded data; a last-priority postfix snapshots
    /// the result. This is an induced component on a dropped-loot bag, not a Riverine Rise container, not an organic old
    /// save and not an area transition.
    /// </summary>
    internal static class ThornstepFixture
    {
        internal const string Condition = "thornstepInducedSource";
        internal const string FormerSourceGuid = "3322c56f38031eb4983b6f87c95081b7";
        internal const string ThornstepGuid = "676faa5f811d851c9f14204bf864e1ec";
        internal const string ManifestName = "kmg-d3-thornstep-manifest.json";
        internal const string VerifiedName = "kmg-d3-thornstep-verified.json";
        internal const BindingFlags Any = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        internal static string ProfileDir;
        internal static readonly List<Record> Records = new List<Record>();
        private static readonly Dictionary<LootComponent, Record> Pending = new Dictionary<LootComponent, Record>();
        private static Dictionary<string, bool> inducedById = new Dictionary<string, bool>();
        internal static bool Installed;

        internal sealed class Record
        {
            public string Id, Phase;
            public bool Induced;
            public List<ItemEntity> PreItems, PostItems;
            public JObject Pre, Post;
            public string OutcomeBefore, OutcomeAfter;
        }

        internal static string Path(string name) => System.IO.Path.Combine(ProfileDir ?? ".", name);

        internal static FieldInfo KnownField => typeof(LootComponent.LootPersistentData).GetField("m_KnownItems", Any);

        internal static string IdOf(LootComponent c)
        {
            MapObjectView view = c == null ? null : c.MapObject;
            if (view == null) return null;
            return view.Data != null ? view.Data.UniqueId : view.UniqueId;
        }

        internal static string ProductOutcome()
        {
            try
            {
                Type t = ProductProbes.Entry().Assembly.GetType("KingmakerGunslinger.Acquisition.RetiredLootReconciliationPatch", false);
                return t == null ? "<no type>" : (string)t.GetField("LastOutcome", Any).GetValue(null);
            }
            catch (Exception e) { return "<unreadable:" + e.GetType().Name + ">"; }
        }

        internal static JObject Snapshot(LootComponent.LootPersistentData data)
        {
            if (data == null) return new JObject { ["data"] = null };
            var known = KnownField.GetValue(data) as Dictionary<BlueprintItem, int>;
            return new JObject
            {
                ["items"] = new JArray((data.Loot == null ? new ItemEntity[0] : data.Loot.Items.ToArray()).Select(i => (object)Sig(i)).ToArray()),
                ["known"] = known == null ? null : new JObject(known.OrderBy(k => k.Key == null ? "" : k.Key.name, StringComparer.Ordinal)
                    .Select(k => new JProperty(k.Key == null ? "<null>" : k.Key.name + "#" + k.Key.AssetGuid, k.Value)))
            };
        }

        internal static JObject Sig(ItemEntity i) => new JObject
        {
            ["bp"] = i.Blueprint == null ? null : i.Blueprint.name, ["guid"] = i.Blueprint == null ? null : i.Blueprint.AssetGuid,
            ["count"] = i.Count, ["timeTicks"] = i.Time.Ticks, ["marker"] = i.SellTime.HasValue ? (int)i.SellTime.Value.TotalSeconds : (int?)null,
            ["held"] = i.HoldingSlot != null
        };

        internal static void Install(HarmonyInstance harmony)
        {
            string manifest = Path(ManifestName);
            if (!File.Exists(manifest)) throw new FileNotFoundException("D3 manifest missing; the seed launch must run first", manifest);
            JObject m = JObject.Parse(File.ReadAllText(manifest));
            inducedById = ((JArray)m["bags"]).Cast<JObject>().ToDictionary(b => (string)b["id"], b => (bool)b["induced"]);
            MethodInfo target = typeof(LootComponent).GetMethod("OnDataSetted", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);
            harmony.Patch(target,
                new HarmonyMethod(typeof(ThornstepFixture).GetMethod(nameof(InducePrefix), Any)) { prioritiy = Priority.First },
                new HarmonyMethod(typeof(ThornstepFixture).GetMethod(nameof(ObservePostfix), Any)) { prioritiy = Priority.Last });
            Installed = true;
        }

        internal static string Phase = "load";

        private static void InducePrefix(LootComponent __instance)
        {
            try
            {
                string id = IdOf(__instance);
                bool induced;
                if (id == null || !inducedById.TryGetValue(id, out induced)) return;
                if (induced)
                {
                    BlueprintLoot source = ResourcesLibrary.TryGetBlueprint<BlueprintLoot>(FormerSourceGuid);
                    __instance.LootTables = new[] { new BlueprintReference(source) }; // TEST-ONLY induced former-source reference
                }
                var data = ((MapObjectComponent)__instance).Data as LootComponent.LootPersistentData;
                Pending[__instance] = new Record
                {
                    Id = id, Phase = Phase, Induced = induced, OutcomeBefore = ProductOutcome(),
                    PreItems = data == null || data.Loot == null ? new List<ItemEntity>() : data.Loot.Items.ToList(), Pre = Snapshot(data)
                };
            }
            catch (Exception e) { Records.Add(new Record { Id = "<prefix-fault>", Phase = e.ToString() }); }
        }

        private static void ObservePostfix(LootComponent __instance)
        {
            Record r;
            if (!Pending.TryGetValue(__instance, out r)) return;
            Pending.Remove(__instance);
            try
            {
                var data = ((MapObjectComponent)__instance).Data as LootComponent.LootPersistentData;
                r.PostItems = data == null || data.Loot == null ? new List<ItemEntity>() : data.Loot.Items.ToList();
                r.Post = Snapshot(data);
                r.OutcomeAfter = ProductOutcome();
            }
            catch (Exception e) { r.Post = new JObject { ["fault"] = e.ToString() }; }
            Records.Add(r);
        }
    }

    /// <summary>
    /// "seed": spawns seven dropped-loot bags, constructs their loot data through the native LootPersistentData
    /// constructor and native UpdateLoot (engine-added copy), performs real party-inventory transfers and returns, and
    /// writes the predeclared expectations to the isolated profile before any product reconciliation of these inputs,
    /// then saves. "reconcile": a fresh process loads the seed save with the induced former-source reference; the
    /// automatic load-time OnDataSetted records are compared to the predeclared expectations; the component's native
    /// fresh generation is checked; a direct repeat of the native callback must change nothing; then saves. "reload": a
    /// fresh process loads that save; the automatic records must equal the reconcile launch's verified state.
    /// </summary>
    internal sealed class ThornstepRetirementProbe : IOwnedProbe
    {
        private const BindingFlags Any = ThornstepFixture.Any;
        private readonly string mode;
        private readonly JObject result = new JObject();
        private readonly List<string> problems = new List<string>();
        private readonly IEnumerator<object> script;
        private string verdict;
        private BlueprintLoot source;
        private BlueprintItem thornstep;

        public ThornstepRetirementProbe(string mode) { this.mode = mode; result["mode"] = mode; script = Run().GetEnumerator(); }
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

        // Fixture state lives in the disposable written saves; nothing in the running process needs giving back.
        public CleanupReport Release(string testFailure) => ProbeCleanup.RunAll(new KeyValuePair<string, Action>[0]);

        private IEnumerable<object> Run()
        {
            result["scenario"] = "induced former-source reference on engine dropped-loot bags (test-only construction); not Riverine Rise, not an organic old save, not an area transition";
            source = ResourcesLibrary.TryGetBlueprint<BlueprintLoot>(ThornstepFixture.FormerSourceGuid);
            thornstep = ResourcesLibrary.TryGetBlueprint<BlueprintItem>(ThornstepFixture.ThornstepGuid);
            if (source == null || thornstep == null) { problems.Add("former source or Thornstep blueprint missing"); yield break; }
            result["source"] = new JObject { ["name"] = source.name, ["area"] = source.Area == null ? null : source.Area.name, ["thornstep"] = thornstep.name };
            yield return null;
            IEnumerable<object> body = mode == "seed" ? Seed() : mode == "reconcile" ? Reconcile() : mode == "reload" ? Reload() : null;
            if (body == null) { problems.Add("unknown mode " + mode); yield break; }
            foreach (object step in body) yield return step;
        }

        private LootEntry[] Historical(int thornsteps) =>
            (source.Items ?? new LootEntry[0]).Concat(thornsteps > 0 ? new[] { new LootEntry { Item = thornstep, Count = thornsteps } } : new LootEntry[0]).ToArray();

        private ItemEntity[] Thornsteps(ItemsCollection c) => c.Items.Where(i => i != null && ReferenceEquals(i.Blueprint, thornstep)).ToArray();

        private static JArray Unrelated(JObject snap, string thornGuid) => new JArray(((JArray)snap["items"]).Cast<JObject>()
            .Where(i => (string)i["guid"] != thornGuid).Select(i => (object)((string)i["bp"] + "|" + (int)i["count"] + "|" + (long)i["timeTicks"]))
            .OrderBy(s => (string)s, StringComparer.Ordinal).ToArray());

        private static int[] Markers(JObject snap, string thornGuid) => ((JArray)snap["items"]).Cast<JObject>()
            .Where(i => (string)i["guid"] == thornGuid).Select(i => i["marker"].Type == JTokenType.Null ? -1 : (int)i["marker"]).ToArray();

        private static int? KnownThornstep(JObject snap, string thornGuid)
        {
            if (snap["known"] == null || snap["known"].Type == JTokenType.Null) return null;
            JProperty p = ((JObject)snap["known"]).Properties().FirstOrDefault(x => x.Name.EndsWith("#" + thornGuid, StringComparison.Ordinal));
            return p == null ? (int?)-1 : (int)p.Value; // -1 = no row
        }

        private JObject Party()
        {
            UnitEntityData main = Game.Instance.Player.MainCharacter.Value;
            Func<ItemsCollection, JArray> thorn = c => new JArray(Thornsteps(c).Select(i => (object)new JObject { ["marker"] = i.SellTime.HasValue ? (int)i.SellTime.Value.TotalSeconds : -1, ["timeTicks"] = i.Time.Ticks }).ToArray());
            return new JObject
            {
                ["inventoryThornstep"] = thorn(Game.Instance.Player.Inventory), ["inventoryTotal"] = Game.Instance.Player.Inventory.Items.Sum(x => x.Count),
                ["stashThornstep"] = thorn(Game.Instance.Player.SharedStash), ["stashTotal"] = Game.Instance.Player.SharedStash.Items.Sum(x => x.Count),
                ["mainEquipment"] = new JArray(main.Body.AllSlots.Where(s => s.HasItem).Select(s => (object)s.Item.Blueprint.name).OrderBy(n => (string)n, StringComparer.Ordinal).ToArray())
            };
        }

        private static void Mark(ItemEntity item, int marker) { item.SellTime = TimeSpan.FromSeconds(marker); } // TEST-ONLY identity marker

        /// <summary>Replaces the bag entity's saved LootComponent data without firing OnDataSetted, so no reconciliation runs in the seed launch.</summary>
        private void Attach(DroppedLoot bag, LootComponent.LootPersistentData data)
        {
            var entity = (MapObjectEntityData)bag.Data;
            var list = (IList)typeof(MapObjectEntityData).GetField("m_ComponentsData", Any).GetValue(entity);
            object entry = list.Cast<object>().Single(e => (string)e.GetType().GetField("ComponentTypeName", Any).GetValue(e) == "LootComponent");
            entry.GetType().GetField("Data", Any).SetValue(entry, data);
            typeof(MapObjectComponent).GetField("m_Data", Any).SetValue(bag.GetComponent<LootComponent>(), data);
        }

        private IEnumerable<object> Seed()
        {
            // Fresh publication/generation: the definition and its native constructor contain no Thornstep.
            var freshFromNative = new LootComponent.LootPersistentData(source.Items ?? new LootEntry[0]);
            result["fresh"] = new JObject
            {
                ["definitionItems"] = new JArray((source.Items ?? new LootEntry[0]).Where(e => e != null && e.Item != null).Select(e => (object)(e.Item.name + "x" + e.Count)).ToArray()),
                ["definitionThornstep"] = (source.Items ?? new LootEntry[0]).Count(e => e != null && ReferenceEquals(e.Item, thornstep)),
                ["nativeConstructorThornstep"] = Thornsteps(freshFromNative.Loot).Length
            };
            if ((int)result["fresh"]["definitionThornstep"] != 0 || (int)result["fresh"]["nativeConstructorThornstep"] != 0)
                problems.Add("seed: fresh publication/generation contains Thornstep");
            if (Game.Instance.Player.GameTime == TimeSpan.Zero) { problems.Add("seed: game time is zero, party stamps would be indistinguishable"); yield break; }

            UnitEntityData main = Game.Instance.Player.MainCharacter.Value;
            DroppedLoot prefab = BlueprintRoot.Instance.Prefabs.DroppedLootBag;
            MethodInfo updateLoot = typeof(LootComponent.LootPersistentData).GetMethod("UpdateLoot", Any);
            var bags = new JArray();
            result["bags"] = bags;
            string[] roles = { "untouched", "engineAdded", "returned", "mixed", "overCount", "missingHistory", "unscoped" };
            for (int n = 0; n < roles.Length; n++)
            {
                string role = roles[n];
                Vector3 at = main.Position + new Vector3(1.5f * (n - 3), 0f, 2f);
                DroppedLoot bag = Game.Instance.EntityCreator.SpawnEntityView(prefab, at, Quaternion.identity, Game.Instance.State.LoadedAreaState.MainState);
                if (bag == null) { problems.Add("seed: bag spawn failed for " + role); yield break; }
                yield return null;
                LootComponent.LootPersistentData data;
                var bagRow = new JObject { ["role"] = role, ["id"] = bag.Data.UniqueId, ["induced"] = role != "unscoped" };
                int m0 = 1000 + 10 * n;
                switch (role)
                {
                    case "engineAdded":
                    {
                        data = new LootComponent.LootPersistentData(source.Items ?? new LootEntry[0]); // native generation from current rows
                        int before = Thornsteps(data.Loot).Length;
                        updateLoot.Invoke(data, new object[] { Historical(1) }); // native UpdateLootItem → ItemsCollection.Add(blueprint, 1)
                        ItemEntity[] added = Thornsteps(data.Loot);
                        bagRow["nativeAdd"] = new JObject { ["before"] = before, ["after"] = added.Length, ["timeTicks"] = added.Length == 1 ? added[0].Time.Ticks : -1,
                            ["knownAfter"] = KnownThornstep(ThornstepFixture.Snapshot(data), ThornstepFixture.ThornstepGuid) };
                        if (before != 0 || added.Length != 1 || added[0].Time != TimeSpan.Zero) problems.Add("seed: native UpdateLoot did not add exactly one zero-time Thornstep");
                        Mark(added[0], m0 + 1);
                        break;
                    }
                    case "unscoped":
                    {
                        LootComponent comp = bag.GetComponent<LootComponent>();
                        data = new LootComponent.LootPersistentData(comp.MapObjectLoot); // the bag's own (empty) definition
                        ItemEntity copy = thornstep.CreateEntity();
                        data.Loot.Add(copy);
                        Mark(copy, m0 + 1);
                        data.Loot.Add(source.Items.First(e => e != null && e.Item != null).Item.CreateEntity());
                        break;
                    }
                    default:
                    {
                        data = new LootComponent.LootPersistentData(Historical(role == "mixed" ? 2 : 1)); // native generation from the historical rows
                        ItemEntity[] generated = Thornsteps(data.Loot);
                        for (int k = 0; k < generated.Length; k++) Mark(generated[k], m0 + 1 + k);
                        if (role == "overCount")
                        {
                            ItemEntity extra = thornstep.CreateEntity(); // TEST-ONLY: a second zero-time copy beyond the recorded count
                            data.Loot.Add(extra);
                            Mark(extra, m0 + 2);
                        }
                        if (role == "missingHistory") ThornstepFixture.KnownField.SetValue(data, null); // TEST-ONLY historical input
                        break;
                    }
                }
                Attach(bag, data);
                if (role == "returned" || role == "mixed")
                {
                    // Real party transfer and return through the native ItemsCollection.Transfer (party inventory stamps Time).
                    ItemEntity copy = Thornsteps(data.Loot).Last();
                    ItemEntity moved = data.Loot.Transfer(copy, Game.Instance.Player.Inventory);
                    long stamped = moved == null ? -1 : moved.Time.Ticks;
                    yield return null;
                    ItemEntity back = Game.Instance.Player.Inventory.Transfer(moved, data.Loot);
                    bagRow["transfer"] = new JObject { ["sameInstanceOut"] = ReferenceEquals(moved, copy), ["sameInstanceBack"] = ReferenceEquals(back, copy),
                        ["stampedTicks"] = stamped, ["afterReturnTicks"] = copy.Time.Ticks, ["inContainer"] = data.Loot.Items.Contains(copy) };
                    if (!ReferenceEquals(moved, copy) || !ReferenceEquals(back, copy) || stamped <= 0 || copy.Time.Ticks != stamped || !data.Loot.Items.Contains(copy))
                        problems.Add("seed: " + role + " party transfer/return did not stamp and return the exact instance");
                }
                JObject snap = ThornstepFixture.Snapshot(data);
                bagRow["declaredInput"] = snap;
                Declare(role, bagRow, snap);
                bags.Add(bagRow);
            }
            // Party-held copies (inventory is stamped by the native add; the stash is not the party inventory).
            ItemEntity inv = thornstep.CreateEntity(); Mark(inv, 2001); Game.Instance.Player.Inventory.Add(inv);
            ItemEntity stash = thornstep.CreateEntity(); Mark(stash, 2002); Game.Instance.Player.SharedStash.Add(stash);
            result["party"] = Party();
            var manifest = new JObject { ["thornstepGuid"] = ThornstepFixture.ThornstepGuid, ["formerSource"] = ThornstepFixture.FormerSourceGuid,
                ["bags"] = bags, ["party"] = result["party"], ["declaredUtc"] = DateTime.UtcNow.ToString("o") };
            File.WriteAllText(ThornstepFixture.Path(ThornstepFixture.ManifestName), manifest.ToString(Formatting.Indented));
            result["manifest"] = ThornstepFixture.Path(ThornstepFixture.ManifestName);
            if (problems.Count > 0) yield break;
            foreach (object step in SaveAs("d3seed")) yield return step;
        }

        /// <summary>Predeclared expectations, written before any product reconciliation sees the inputs.</summary>
        private static void Declare(string role, JObject row, JObject input)
        {
            string g = ThornstepFixture.ThornstepGuid;
            int[] markers = Markers(input, g);
            int[] removed;
            string decision;
            switch (role)
            {
                case "untouched": case "engineAdded": removed = markers; decision = "RemovedUntouched"; break;
                case "returned": removed = new int[0]; decision = "RetainedAmbiguous"; break;
                case "mixed": removed = markers.Take(1).ToArray(); decision = "RemovedUntouchedRetainedAmbiguous"; break; // the zero-time generated copy
                case "overCount": removed = markers.Take(1).ToArray(); decision = "RemovedUntouchedRetainedAmbiguous"; break; // at most the recorded 1, first in collection order
                case "missingHistory": removed = new int[0]; decision = "NoHistoryRetained"; break;
                default: removed = new int[0]; decision = null; break; // unscoped: the product must not act
            }
            row["expected"] = new JObject
            {
                ["removedMarkers"] = new JArray(removed.Cast<object>().ToArray()),
                ["retainedMarkers"] = new JArray(markers.Where(m => !removed.Contains(m)).Cast<object>().ToArray()),
                ["unrelated"] = Unrelated(input, g),
                ["firstDecision"] = decision,
                ["repeatDecision"] = role == "unscoped" ? null : role == "missingHistory" ? "NoHistoryRetained" : "NotInHistory",
                ["knownAfter"] = role == "unscoped" ? KnownThornstep(input, g) : role == "missingHistory" ? (int?)null : 0
            };
        }

        private IEnumerable<object> SaveAs(string label)
        {
            if (!Game.Instance.SaveManager.IsSaveAllowed()) { problems.Add(label + ": native save admission refused"); yield break; }
            bool saved = false;
            Game.Instance.SaveGame(Game.Instance.SaveManager.CreateNewSave(label), () => saved = true);
            float until = Time.realtimeSinceStartup + 120f;
            while ((!saved || Game.Instance.SaveManager.CommitInProgress || LoadingProcess.Instance.IsLoadingInProcess) && Time.realtimeSinceStartup < until) yield return null;
            bool finished = saved && !Game.Instance.SaveManager.CommitInProgress && !LoadingProcess.Instance.IsLoadingInProcess;
            result["save:" + label] = finished;
            if (!finished) problems.Add(label + ": the save did not finish");
        }

        private static string Decision(string outcome)
        {
            if (string.IsNullOrEmpty(outcome)) return null;
            int c = outcome.IndexOf(':'); int s = outcome.IndexOf(';');
            return c < 0 ? outcome : s < 0 ? outcome.Substring(c + 1) : outcome.Substring(c + 1, s - c - 1);
        }

        /// <summary>Checks one OnDataSetted record against the expected marker set, decision, history row and unrelated contents.</summary>
        private JObject Check(string label, ThornstepFixture.Record r, JObject bag, int[] expectPre, int[] expectPost, string expectDecision, JArray expectUnrelated)
        {
            string g = ThornstepFixture.ThornstepGuid;
            JObject exp = (JObject)bag["expected"];
            string role = (string)bag["role"];
            int[] pre = Markers(r.Pre, g), post = Markers(r.Post, g);
            var preThorn = r.PreItems.Where(i => ReferenceEquals(i.Blueprint, thornstep)).ToList();
            var postThorn = r.PostItems.Where(i => ReferenceEquals(i.Blueprint, thornstep)).ToList();
            var removedRefs = preThorn.Where(i => !postThorn.Any(p => ReferenceEquals(p, i))).ToList();
            bool retainedAreSameObjects = postThorn.All(p => preThorn.Any(i => ReferenceEquals(i, p)));
            string decision = r.OutcomeAfter == r.OutcomeBefore && !r.Induced ? null : Decision(r.OutcomeAfter);
            int? knownAfter = KnownThornstep(r.Post, g);
            var row = new JObject
            {
                ["label"] = label, ["role"] = role, ["induced"] = r.Induced, ["phase"] = r.Phase,
                ["preMarkers"] = new JArray(pre.Cast<object>().ToArray()), ["postMarkers"] = new JArray(post.Cast<object>().ToArray()),
                ["removedMarkers"] = new JArray(removedRefs.Select(i => (object)(i.SellTime.HasValue ? (int)i.SellTime.Value.TotalSeconds : -1)).ToArray()),
                ["retainedSameObjects"] = retainedAreSameObjects, ["productOutcome"] = r.OutcomeAfter, ["outcomeChanged"] = r.OutcomeAfter != r.OutcomeBefore,
                ["knownThornstepBefore"] = KnownThornstep(r.Pre, g), ["knownThornstepAfter"] = knownAfter,
                ["unrelatedPre"] = Unrelated(r.Pre, g), ["unrelatedPost"] = Unrelated(r.Post, g),
                ["postTimes"] = new JArray(postThorn.Select(i => (object)i.Time.Ticks).ToArray())
            };
            Func<int[], int[], bool> same = (a, b) => a.OrderBy(x => x).SequenceEqual(b.OrderBy(x => x));
            if (!same(pre, expectPre)) problems.Add(label + "/" + role + ": loaded input markers " + string.Join(",", pre) + " != expected " + string.Join(",", expectPre));
            if (!same(post, expectPost)) problems.Add(label + "/" + role + ": result markers " + string.Join(",", post) + " != expected " + string.Join(",", expectPost));
            if (!retainedAreSameObjects) problems.Add(label + "/" + role + ": a retained Thornstep is not the loaded instance");
            if (post.Length > pre.Length) problems.Add(label + "/" + role + ": Thornstep was added");
            if (!JToken.DeepEquals(Unrelated(r.Post, g), expectUnrelated) || !JToken.DeepEquals(Unrelated(r.Pre, g), expectUnrelated))
                problems.Add(label + "/" + role + ": unrelated loot differs from the expectation");
            if (role == "unscoped") { if (r.OutcomeAfter != r.OutcomeBefore) problems.Add(label + ": the product acted on the unscoped bag"); }
            else if (decision != expectDecision) problems.Add(label + "/" + role + ": product decision " + decision + " != expected " + expectDecision);
            int? expectKnown = exp["knownAfter"].Type == JTokenType.Null ? (int?)null : (int)exp["knownAfter"];
            if (knownAfter != expectKnown) problems.Add(label + "/" + role + ": Thornstep history " + knownAfter + " != expected " + expectKnown);
            return row;
        }

        private JObject Manifest(string name) => JObject.Parse(File.ReadAllText(ThornstepFixture.Path(name)));

        private IEnumerable<object> Reconcile()
        {
            JObject manifest = Manifest(ThornstepFixture.ManifestName);
            result["inductionInstalled"] = ThornstepFixture.Installed;
            if (!ThornstepFixture.Installed) { problems.Add("reconcile: induction condition not installed"); yield break; }
            var rows = new JArray(); result["automaticLoad"] = rows;
            var verified = new JObject(); var verifiedBags = new JArray(); verified["bags"] = verifiedBags;
            var comps = UnityEngine.Object.FindObjectsOfType<LootComponent>().Where(c => ThornstepFixture.IdOf(c) != null)
                .GroupBy(c => ThornstepFixture.IdOf(c)).ToDictionary(g => g.Key, g => g.First());
            foreach (JObject bag in ((JArray)manifest["bags"]).Cast<JObject>())
            {
                string id = (string)bag["id"]; JObject exp = (JObject)bag["expected"];
                var recs = ThornstepFixture.Records.Where(r => r.Id == id && r.Phase == "load").ToList();
                if (recs.Count != 1) { problems.Add("reconcile: " + bag["role"] + " has " + recs.Count + " automatic load callbacks, expected 1"); continue; }
                int[] input = Markers((JObject)bag["declaredInput"], ThornstepFixture.ThornstepGuid);
                int[] retained = ((JArray)exp["retainedMarkers"]).Select(t => (int)t).ToArray();
                rows.Add(Check("automaticLoad", recs[0], bag, input, retained, (string)exp["firstDecision"], (JArray)exp["unrelated"]));
                verifiedBags.Add(new JObject { ["id"] = id, ["role"] = bag["role"], ["post"] = recs[0].Post });
            }
            var prefaults = ThornstepFixture.Records.Where(r => r.Id == "<prefix-fault>").ToList();
            if (prefaults.Count > 0) problems.Add("induction prefix faulted: " + prefaults[0].Phase);
            yield return null;
            // Native fresh generation through the induced component (CreateData → new LootPersistentData(MapObjectLoot)); not assigned.
            JObject anyScoped = ((JArray)manifest["bags"]).Cast<JObject>().First(b => (bool)b["induced"]);
            LootComponent scopedComp;
            if (comps.TryGetValue((string)anyScoped["id"], out scopedComp))
            {
                var fresh = (LootComponent.LootPersistentData)scopedComp.CreateData();
                result["freshGenerationThroughComponent"] = new JObject { ["thornstep"] = Thornsteps(fresh.Loot).Length, ["items"] = fresh.Loot.Items.Count(),
                    ["tables"] = new JArray(scopedComp.LootTables.Select(t => (object)(t.Get() == null ? null : t.Get().AssetGuid)).ToArray()) };
                if (Thornsteps(fresh.Loot).Length != 0) problems.Add("reconcile: fresh generation through the induced component contains Thornstep");
            }
            else problems.Add("reconcile: induced bag component not found");
            // Repeat reconciliation: the native Data setter fires OnDataSetted again (direct callback).
            ThornstepFixture.Phase = "repeat";
            var repeat = new JArray(); result["directRepeat"] = repeat;
            foreach (JObject bag in ((JArray)manifest["bags"]).Cast<JObject>())
            {
                LootComponent comp;
                if (!comps.TryGetValue((string)bag["id"], out comp)) { problems.Add("reconcile: bag " + bag["role"] + " not in scene"); continue; }
                ((MapObjectComponent)comp).Data = ((MapObjectComponent)comp).Data;
                ThornstepFixture.Record r = ThornstepFixture.Records.LastOrDefault(x => x.Id == (string)bag["id"] && x.Phase == "repeat");
                if (r == null) { problems.Add("reconcile: no repeat callback for " + bag["role"]); continue; }
                int[] retained = ((JArray)((JObject)bag["expected"])["retainedMarkers"]).Select(t => (int)t).ToArray();
                repeat.Add(Check("directRepeat", r, bag, retained, retained, (string)bag["expected"]["repeatDecision"], (JArray)bag["expected"]["unrelated"]));
                if (!r.PreItems.SequenceEqual(r.PostItems)) problems.Add("directRepeat/" + bag["role"] + ": the repeat changed the collection");
            }
            ThornstepFixture.Phase = "after";
            JObject party = Party(); result["party"] = party;
            if (!JToken.DeepEquals(party, manifest["party"])) problems.Add("reconcile: party controls differ from the seed declaration");
            verified["party"] = party;
            File.WriteAllText(ThornstepFixture.Path(ThornstepFixture.VerifiedName), verified.ToString(Formatting.Indented));
            if (problems.Count > 0) yield break;
            foreach (object step in SaveAs("d3recon")) yield return step;
        }

        private IEnumerable<object> Reload()
        {
            JObject manifest = Manifest(ThornstepFixture.ManifestName), verified = Manifest(ThornstepFixture.VerifiedName);
            if (!ThornstepFixture.Installed) { problems.Add("reload: induction condition not installed"); yield break; }
            var rows = new JArray(); result["automaticLoad"] = rows;
            foreach (JObject bag in ((JArray)manifest["bags"]).Cast<JObject>())
            {
                string id = (string)bag["id"];
                JObject prior = ((JArray)verified["bags"]).Cast<JObject>().FirstOrDefault(b => (string)b["id"] == id);
                var recs = ThornstepFixture.Records.Where(r => r.Id == id && r.Phase == "load").ToList();
                if (recs.Count != 1 || prior == null) { problems.Add("reload: " + bag["role"] + " callbacks " + recs.Count + ", verified state " + (prior != null)); continue; }
                int[] retained = ((JArray)((JObject)bag["expected"])["retainedMarkers"]).Select(t => (int)t).ToArray();
                rows.Add(Check("reloadLoad", recs[0], bag, retained, retained, (string)bag["expected"]["repeatDecision"], (JArray)bag["expected"]["unrelated"]));
                // Compared with the reconcile launch's verified post-state: same items (bp, count, time, marker) and history.
                if (!JToken.DeepEquals(recs[0].Pre, prior["post"])) problems.Add("reload/" + bag["role"] + ": loaded state differs from the reconcile launch's verified state");
                if (!JToken.DeepEquals(recs[0].Post, prior["post"])) problems.Add("reload/" + bag["role"] + ": reconciliation changed the reloaded state");
            }
            yield return null;
            JObject party = Party(); result["party"] = party;
            if (!JToken.DeepEquals(party, manifest["party"])) problems.Add("reload: party controls differ from the seed declaration");
        }
    }
}
