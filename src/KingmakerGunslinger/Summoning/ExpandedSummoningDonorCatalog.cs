using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace KingmakerGunslinger.Summoning
{
    internal sealed class SummonDonorSpec
    {
        internal SummonDonorSpec(string key, string guid, bool dedicated)
        { CreatureKey = key; Guid = guid; DedicatedSummon = dedicated; }
        internal string CreatureKey { get; private set; }
        internal string Guid { get; private set; }
        internal bool DedicatedSummon { get; private set; }
    }

    internal static class ExpandedSummoningDonorCatalog
    {
        private static readonly SummonDonorSpec[] Values = Build();
        internal static IReadOnlyList<SummonDonorSpec> All
        { get { return Array.AsReadOnly(Values); } }
        internal static SummonDonorSpec For(string key)
        { return Values.Single(value => value.CreatureKey == key); }

        internal static void Validate()
        {
            string[] expected = ExpandedSummoningCatalog.All.Select(v => v.Key)
                .OrderBy(v => v, StringComparer.Ordinal).ToArray();
            string[] actual = Values.Select(v => v.CreatureKey)
                .OrderBy(v => v, StringComparer.Ordinal).ToArray();
            if (!expected.SequenceEqual(actual, StringComparer.Ordinal))
                throw new InvalidOperationException("Every creature requires exactly one donor.");
            if (Values.Any(v => !Regex.IsMatch(v.Guid ?? "", "^[0-9a-f]{32}$")))
                throw new InvalidOperationException("A donor GUID is invalid.");
        }

        private static SummonDonorSpec[] Build()
        {
            return Parse(new[] {
                "dog|76597216769b0d540aafafa07edf0cec|1", "eagle|406c1e1af5400ac4881e330502ccbd9e|0", "poisonous-frog|30080a8d8ae40bb43aca496b11b74c6b|0",
                "giant-centipede|2f65fd8032e5182418ee83dd4f7858dd|0", "wolf|76597216769b0d540aafafa07edf0cec|1", "giant-frog|1ed9a630f0d9d7f44855d3d1d1b2cdf2|1", "giant-spider|9e120b5e0ad3c794491c049aa24b9fde|1",
                "small-air-elemental|04944455200bc224d955a8e9bbd64f3f|1", "small-earth-elemental|651600a51edd20141adb67696986c582|1", "small-fire-elemental|46cede83b1f34ad4fa46b8776e352b02|1", "small-water-elemental|56372b0a2749c224392a5ee74105c534|1", "goblin-dog|313a17cbd273d1f40bd1654ee2ae186e|0", "hyena|76597216769b0d540aafafa07edf0cec|1",
                "boar|5f968d63d756f994ebff0d774e88e4ab|0", "leopard|768275c9885dd954fb3c84ba69ac4281|1", "monitor-lizard|4109b40f6bbb49640840644cc84ada67|1", "cheetah|768275c9885dd954fb3c84ba69ac4281|1", "crocodile|4109b40f6bbb49640840644cc84ada67|1", "dire-bat|406c1e1af5400ac4881e330502ccbd9e|0", "wolverine|313a17cbd273d1f40bd1654ee2ae186e|0", "lantern-archon|24719a49b84c5cd43b894268d22d9c89|0",
                "dire-boar|6ec9c63c41a1e754ea4dcd85557625b4|1", "dire-wolf|03dd28e92faf2e44eb9564a6ba01fdd0|1", "grizzly-bear|0b214d8e81a563549ba0be37cd1c16d0|0", "medium-air-elemental|676f8b7d0a170674cb6e504e0e30b4f0|1", "medium-earth-elemental|812c9a0348e004242ba4e46efa91e38e|1", "medium-fire-elemental|a0ab0c31b1a92554291a82e598f39ba4|1", "medium-water-elemental|62a3e860e6e72e6499c38bb8b2fe303e|1", "air-mephit|50782bc4eb36aac4287023e20ee00808|1", "earth-mephit|46779f56cab2cb0438161fec0129790d|1", "fire-mephit|10a820de0a417f345866f794324205ad|1", "water-mephit|4615328295cd7e84bb2ef09d3dba8403|1", "lion|768275c9885dd954fb3c84ba69ac4281|1", "pteranodon|406c1e1af5400ac4881e330502ccbd9e|0", "hell-hound|ece348345859351439e1263115f5fdb9|1",
                "large-air-elemental|3764b43791a00e1468257adbca43ce9b|1", "large-earth-elemental|d3d9ab560534bd948b10ac00abbff083|1", "large-fire-elemental|ba5026596b06b204eb2efed2b411c5b9|1", "large-water-elemental|680b5b61c80af664daec46af7644486c|1", "dire-lion|beae4985629a6f64eb98081e3171e4c1|1", "ankylosaurus|c3524f96954a1d94f8525b86e7626633|0", "bralani-azata|58574e8d1d4dc464c976f396d9115b1a|1", "salamander|e8276e28b2234a745900fed80670bfdb|0",
                "dire-bear|260da5b557e3fb04bb4960a36a5d1dc4|0", "dire-tiger|beae4985629a6f64eb98081e3171e4c1|1", "huge-air-elemental|2e24256e459468743b91fbb9aa85e1ab|1", "huge-earth-elemental|3b86a449e7264174eaccef9b8f02fe20|1", "huge-fire-elemental|640fb7efb7c916945837bbcab995267e|1", "huge-water-elemental|877c154a296ee8e45be1a00668319923|1", "elephant|028cc6f46e7998f46855a33ffde89567|1", "erinyes-devil|6ea3a75279bab234aa723989e30cb15a|0", "invisible-stalker|676f8b7d0a170674cb6e504e0e30b4f0|1", "shadow-demon|1832be68f9814254dbbdab6df7fd5d0b|1", "succubus|0cc7a2526e4557945b1d8eb277d1fb3a|0",
                "greater-air-elemental|e770cfbb96b528c4db258d7d03fe6533|1", "greater-earth-elemental|cda7013db24f4c547b79bfc5c617066b|1", "greater-fire-elemental|b0b4091bdaebb464e903857a95189dea|1", "greater-water-elemental|fcc939e3acf355b458ddf9617d8c6c28|1", "mastodon|028cc6f46e7998f46855a33ffde89567|1", "roc|406c1e1af5400ac4881e330502ccbd9e|0", "bebelith|51c66b0783a748c4b9538f0f0678c4d7|0",
                "elder-air-elemental|33bb90ffd13c87b4c8e45d920313752a|1", "elder-earth-elemental|6b4cb9b6116f2194192e1e7e379c48d7|1", "elder-fire-elemental|ea0f0bbc6e5e471428d535501b21eb26|1", "elder-water-elemental|3bd31a0b4d800f04a8c5b7b1a6d7061e|1", "ghaele-azata|bc8ca1437c0f48948b317b7e64febf0d|1", "pixie|394610e32cfbc4f43a0efaab16faae49|0"
            });
        }

        private static SummonDonorSpec[] Parse(IEnumerable<string> rows)
        { return rows.Select(row => { string[] p = row.Split('|'); return new SummonDonorSpec(p[0], p[1], p[2] == "1"); }).ToArray(); }
    }
}
