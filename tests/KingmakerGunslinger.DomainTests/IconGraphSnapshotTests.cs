using System;
using System.Linq;
using KingmakerGunslinger.RuntimeTesting;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class IconGraphSnapshotTests
    {
        internal static void ControlRequiresExactNoSaveRequest()
        {
            var parameters = new JObject { { "iconCensusControl", true } };
            const string scenario = "icon-overhaul-visual-evidence";
            Assertions.True(IconCensusControlPolicy.IsControl(scenario, true, parameters), "Exact control rejected.");
            Assertions.False(IconCensusControlPolicy.IsControl("working-save-smoke", true, parameters) ||
                IconCensusControlPolicy.IsControl(scenario, false, parameters) ||
                IconCensusControlPolicy.IsControl(scenario, true, null) ||
                IconCensusControlPolicy.IsControl(scenario, true, new JObject()), "Control escaped its exact scope.");
            foreach (JToken invalid in new JToken[] { new JValue(false), new JValue("true"), new JValue(1), JValue.CreateNull() })
                Assertions.False(IconCensusControlPolicy.IsControl(scenario, true,
                    new JObject { { "iconCensusControl", invalid } }), "Invalid control parameter accepted.");
            parameters.Add("saveName", "KMG_AUTOMATION_WORKING");
            Assertions.False(IconCensusControlPolicy.IsControl(scenario, true, parameters), "Save/extra parameter accepted.");
        }

        private sealed class Reference
        {
            public string Id;
            public ActionNode ForeignActions;
        }

        private sealed class ActionNode
        {
            public ActionNode Next;
            public ActionNode[] Branches;
            public Reference Target;
        }

        private static string[] Read(ActionNode root, int limit = 4096) => IconGraphSnapshot.Capture(
            root, "root", type => type == typeof(Reference), value => ((Reference)value).Id,
            type => type == typeof(ActionNode), limit).ToArray();

        internal static void DeepActionsAreObservedWithoutRecursion()
        {
            var root = new ActionNode();
            var current = root;
            for (int index = 0; index < 32; index++) current = current.Next = new ActionNode();
            current.Target = new Reference { Id = "deep-target" };
            Assertions.True(Read(root).Any(row => row.EndsWith(".Target=deep-target")),
                "A deeply nested action target was omitted.");
        }

        internal static void CyclesAndSharedNodesRetainTheirEdges()
        {
            var root = new ActionNode();
            var shared = new ActionNode { Target = new Reference { Id = "shared" }, Next = root };
            root.Branches = new[] { shared, shared, null };
            string[] rows = Read(root);
            Assertions.True(rows.Contains("root.Branches[1]=@root.Branches[0]") &&
                rows.Contains("root.Branches[0].Next=@root") &&
                rows.Contains("root.Branches[2]=null") && rows.Contains("root.Branches.count=3"),
                "Cycles, sharing, null slots or array lengths were lost.");
        }

        internal static void ForeignBlueprintGraphsAreNeverTraversed()
        {
            var foreign = new ActionNode { Target = new Reference { Id = "foreign-target" } };
            var root = new ActionNode { Target = new Reference { Id = "owned-link", ForeignActions = foreign } };
            string[] rows = Read(root);
            Assertions.True(rows.Contains("root.Target=owned-link") &&
                !rows.Any(row => row.Contains("foreign-target") || row.Contains("ForeignActions")),
                "Observation crossed a terminal blueprint reference.");
        }

        internal static void NodeBudgetFailsWithAnExactPath()
        {
            var root = new ActionNode { Next = new ActionNode { Next = new ActionNode() } };
            bool rejected = false;
            try { Read(root, 2); }
            catch (InvalidOperationException error) { rejected = error.Message.Contains("root.Next.Next"); }
            Assertions.True(rejected, "An oversized graph did not fail with its exact path.");
        }

        internal static void ReferenceAndTopologyChangesInvalidateSnapshot()
        {
            var root = new ActionNode { Branches = new[] {
                new ActionNode { Target = new Reference { Id = "a" } },
                new ActionNode { Target = new Reference { Id = "b" } } } };
            string[] before = Read(root);
            root.Branches = root.Branches.Reverse().ToArray();
            Assertions.False(before.SequenceEqual(Read(root)), "Reordered action targets were not detected.");
            before = Read(root);
            root.Branches[0].Target.Id = "changed";
            Assertions.False(before.SequenceEqual(Read(root)), "A changed target identity was not detected.");
        }
    }
}
