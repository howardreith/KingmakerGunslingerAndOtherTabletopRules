using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.UrbanBarbarian
{
    internal sealed class UrbanBarbarianIdentitySpec
    {
        internal UrbanBarbarianIdentitySpec(string symbol, string guid,
            string plannedType)
        { Symbol = symbol; Guid = guid; PlannedType = plannedType; }
        internal string Symbol { get; private set; }
        internal string Guid { get; private set; }
        internal string PlannedType { get; private set; }
    }

    internal static class UrbanBarbarianIdentityCatalog
    {
        internal const int IdentityCount = 70;
        internal const string Archetype = "KMG.UrbanBarbarian.Archetype";
        internal const string Proficiency =
            "KMG.UrbanBarbarian.Proficiency.Feature";
        internal const string CrowdControl =
            "KMG.UrbanBarbarian.CrowdControl.Feature";
        internal const string ControlledRage =
            "KMG.UrbanBarbarian.ControlledRage.Feature";
        internal const string GreaterDefault =
            "KMG.UrbanBarbarian.ControlledRage.GreaterDefault.Feature";
        internal const string MightyDefault =
            "KMG.UrbanBarbarian.ControlledRage.MightyDefault.Feature";
        internal const string RageBuff =
            "KMG.UrbanBarbarian.ControlledRage.Buff";
        internal const string Selector =
            "KMG.UrbanBarbarian.ControlledRage.SelectionAbility";

        private static readonly IDictionary<string, string[]> AllocationGuids =
            new Dictionary<string, string[]>(StringComparer.Ordinal) {
                ["KMG.UrbanBarbarian.Allocation.T4.S4.D0.C0"] = new[] { "c50b25c9f09e50c7a62679dd2bb9c553", "93b75bc68cec245203838c8db1d9546a" },
                ["KMG.UrbanBarbarian.Allocation.T4.S0.D4.C0"] = new[] { "d66252e59d7bdfc8d23b300eef07f066", "b121eb92257de77110416b805f6a9372" },
                ["KMG.UrbanBarbarian.Allocation.T4.S0.D0.C4"] = new[] { "f49d2801f4b07c0f5e187afd62808463", "f6544766636fd197501c5a81f1720847" },
                ["KMG.UrbanBarbarian.Allocation.T4.S2.D2.C0"] = new[] { "b9b6e8732dc7be933061073232f3e3c2", "5ccb92e3ac5c59fad5f555f484847a6c" },
                ["KMG.UrbanBarbarian.Allocation.T4.S2.D0.C2"] = new[] { "67e0106c8db6f94a64bb60cb2fd1d596", "b98a130225ae6c81c5f22f12c1dbd9ec" },
                ["KMG.UrbanBarbarian.Allocation.T4.S0.D2.C2"] = new[] { "c9dac714e8097afb2f2ef866b1b59798", "1b9283f751b06e27bdf5d608f0d6ac31" },
                ["KMG.UrbanBarbarian.Allocation.T6.S6.D0.C0"] = new[] { "6b5b7526c3d55cb76ab04e61d9564d11", "00e051e2e2b8d51a51e6fca62c3d26da" },
                ["KMG.UrbanBarbarian.Allocation.T6.S0.D6.C0"] = new[] { "931a4d5d6a1fbbbb23b753c060084d42", "9112621d1472d5111bbb2df96a689254" },
                ["KMG.UrbanBarbarian.Allocation.T6.S0.D0.C6"] = new[] { "c2b22cd72a899b7628d1557e7eacb501", "a2e2f16332f7ef4e4d858dc902ef39f9" },
                ["KMG.UrbanBarbarian.Allocation.T6.S4.D2.C0"] = new[] { "7409cfea692dbb16d9e76e4cefd58e81", "b5b8d96b97714c8e468d9f8a360d86f8" },
                ["KMG.UrbanBarbarian.Allocation.T6.S4.D0.C2"] = new[] { "eb0fd7c9d326607495a2e39d3249f6dd", "de71cb83692622ae63f787d15316ae21" },
                ["KMG.UrbanBarbarian.Allocation.T6.S2.D4.C0"] = new[] { "6817582448f33a6db1227d96e20d513e", "5e8cf923d92356893482bd5975a6bd67" },
                ["KMG.UrbanBarbarian.Allocation.T6.S0.D4.C2"] = new[] { "b7155b8d4817e4b8b2630cb68972b5fb", "9402a3736112840a8686e1badc122f42" },
                ["KMG.UrbanBarbarian.Allocation.T6.S2.D0.C4"] = new[] { "aecbf16f28fe26f871d299a301af3847", "7d90c27de24801aa86697d3304e199d2" },
                ["KMG.UrbanBarbarian.Allocation.T6.S0.D2.C4"] = new[] { "28d12630923e12427067739a35848241", "601ab3b7a205ae0cfa5ae182d93dad31" },
                ["KMG.UrbanBarbarian.Allocation.T6.S2.D2.C2"] = new[] { "41ce02ba95054b20cf3558eabde0b965", "16d19fb6b901166f5bc6554a57ad8cc7" },
                ["KMG.UrbanBarbarian.Allocation.T8.S8.D0.C0"] = new[] { "6ecebe52517eff77fab34b56f5c00624", "e9ca91fcd33ce2ab056458830dac6cb7" },
                ["KMG.UrbanBarbarian.Allocation.T8.S0.D8.C0"] = new[] { "57db8867a9f37a878f358856d74a52fb", "7a1f6de5ff9b5e3c72dc4a3fdc5b7069" },
                ["KMG.UrbanBarbarian.Allocation.T8.S0.D0.C8"] = new[] { "e90c29577ba1ecb498d858f68cd7119e", "6f6e4328198682957c35c9c99f186347" },
                ["KMG.UrbanBarbarian.Allocation.T8.S6.D2.C0"] = new[] { "466fbcb99f3c57f4cdbc676fc6904e7c", "5cf8f0b41dc49cbdf1489716cd40f2ac" },
                ["KMG.UrbanBarbarian.Allocation.T8.S6.D0.C2"] = new[] { "741da13794d9bb6c2abae3856791aab5", "a0a92307a91c1ee02e4a333e2cc8dc97" },
                ["KMG.UrbanBarbarian.Allocation.T8.S2.D6.C0"] = new[] { "8295761ba1b71866f97d0bb9afae6f44", "bc0d8b7745e45e4f126bf31a0788d2af" },
                ["KMG.UrbanBarbarian.Allocation.T8.S0.D6.C2"] = new[] { "7651c176b5a88869f3de2ac87fe0b7fa", "55010dc791d1f4bcfc11baa584e1c5d0" },
                ["KMG.UrbanBarbarian.Allocation.T8.S2.D0.C6"] = new[] { "ae978a6045cc231bccc74c7e8e839cb7", "230b9337f27b7121e4669f49b5ce39b8" },
                ["KMG.UrbanBarbarian.Allocation.T8.S0.D2.C6"] = new[] { "9a1b22d89bb31dd889102c9203ea11ec", "8bd379c41a289ee814de1b0c331089a4" },
                ["KMG.UrbanBarbarian.Allocation.T8.S4.D4.C0"] = new[] { "b77ee34067f450b05aa939a1566e6f30", "ef4d553094bb2eab2219e4b00ca540ae" },
                ["KMG.UrbanBarbarian.Allocation.T8.S4.D0.C4"] = new[] { "6fcbd67547a21cdf7e7700ce0138a12c", "433cd62a7b1cb8a5c3884547ce7dc9fe" },
                ["KMG.UrbanBarbarian.Allocation.T8.S0.D4.C4"] = new[] { "be796f78208b038acc2cdc474a0b8f4d", "6d5b62bd1bf9be4f6d11efb7acdc1c19" },
                ["KMG.UrbanBarbarian.Allocation.T8.S4.D2.C2"] = new[] { "5f11226d2c2f7388e04583f712a4fc33", "63a0d8be6c7ba52ec0cb4f8286f0a6b9" },
                ["KMG.UrbanBarbarian.Allocation.T8.S2.D4.C2"] = new[] { "b0511155f2d6eab90fea88829b19831c", "ffc4ab2e5b35bb6de0ba652e4315a3db" },
                ["KMG.UrbanBarbarian.Allocation.T8.S2.D2.C4"] = new[] { "0d69ef55092ed0cb23b467e929a8f495", "639e90f156a420e0cad87315f2377590" }
            };

        internal static IReadOnlyList<UrbanBarbarianIdentitySpec> All
        {
            get
            {
                var result = new List<UrbanBarbarianIdentitySpec> {
                    Spec(Archetype, "28f179bf6d325fecec769cebc560abbd", "BlueprintArchetype"),
                    Spec(Proficiency, "08eb5c3d109b1de3c9207c10a860b1fa", "BlueprintFeature"),
                    Spec(CrowdControl, "1644b9cf0e7751a59def80ea8b1b09af", "BlueprintFeature"),
                    Spec(ControlledRage, "7de402be93a4d62d520463cf695dad2c", "BlueprintFeature"),
                    Spec(GreaterDefault, "9fa79d9937a926b6ce54afa85059aac2", "BlueprintFeature"),
                    Spec(MightyDefault, "ba99cfe4d6d8e7c5730c4d61a23120ab", "BlueprintFeature"),
                    Spec(RageBuff, "6359e4bc26d1f311c36c0e8f7aee8f2d", "BlueprintBuff"),
                    Spec(Selector, "f43181528e5111bb5b86f5317997c8e2", "BlueprintAbility") };
                foreach (ControlledRageTier tier in new[] {
                    ControlledRageTier.Ordinary, ControlledRageTier.Greater,
                    ControlledRageTier.Mighty })
                foreach (ControlledRageAllocation allocation in
                    ControlledRageAllocationPolicy.Generate(tier))
                {
                    string[] ids = AllocationGuids[allocation.Symbol];
                    result.Add(Spec(SelectionFeature(allocation), ids[0],
                        "BlueprintFeature"));
                    result.Add(Spec(SelectionAbility(allocation), ids[1],
                        "BlueprintAbility"));
                }
                if (result.Count != IdentityCount || result.Select(value =>
                    value.Guid).Distinct(StringComparer.Ordinal).Count() != IdentityCount)
                    throw new InvalidOperationException(
                        "Urban Barbarian identity catalog is incomplete or colliding.");
                return result.AsReadOnly();
            }
        }

        internal static string SelectionFeature(ControlledRageAllocation allocation)
        { return allocation.Symbol + ".Feature"; }
        internal static string SelectionAbility(ControlledRageAllocation allocation)
        { return allocation.Symbol + ".Ability"; }

        private static UrbanBarbarianIdentitySpec Spec(string symbol, string guid,
            string type)
        { return new UrbanBarbarianIdentitySpec(symbol, guid, type); }
    }
}
