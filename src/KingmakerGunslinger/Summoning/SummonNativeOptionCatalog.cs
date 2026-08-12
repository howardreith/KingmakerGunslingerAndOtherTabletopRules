using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    internal sealed class SummonNativeOptionSpec
    {
        internal SummonNativeOptionSpec(SummonFamily family, int tier,
            string guid, SummonMultiplicity multiplicity,
            string equivalentCreatureKey)
        {
            Family = family;
            Tier = tier;
            Guid = guid;
            Multiplicity = multiplicity;
            EquivalentCreatureKey = equivalentCreatureKey;
        }

        internal SummonFamily Family { get; private set; }
        internal int Tier { get; private set; }
        internal string Guid { get; private set; }
        internal SummonMultiplicity Multiplicity { get; private set; }
        internal string EquivalentCreatureKey { get; private set; }
        internal bool IsSemanticDuplicate
        { get { return EquivalentCreatureKey != null; } }
    }

    internal static class SummonNativeOptionCatalog
    {
        private static readonly SummonNativeOptionSpec[] Values = {
            N(SummonFamily.Monster,1,"6c7915c9dc494849918e958618f61db0",SummonMultiplicity.One,"dog"),
            N(SummonFamily.Monster,2,"7ab27a0d547742741beb5d089f1c3852",SummonMultiplicity.One,"wolf"),
            N(SummonFamily.Monster,2,"6223958f3615eeb4d91e52ba57820878",SummonMultiplicity.OneD3,"dog"),
            N(SummonFamily.Monster,3,"15b5efe371d47c944b58444e7b734ffb",SummonMultiplicity.One,"monitor-lizard"),
            N(SummonFamily.Monster,3,"28f49845fad6a534b95a65b6cea8f8d6",SummonMultiplicity.OneD3,"wolf"),
            N(SummonFamily.Monster,3,"7b531828b885e8b47a7b06a6f1a34805",SummonMultiplicity.OneD4PlusOne,"dog"),
            N(SummonFamily.Monster,4,"efa433a38e9c7c14bb4e780f8a3fe559",SummonMultiplicity.One,"dire-wolf"),
            N(SummonFamily.Monster,4,"e73c4562e99c7764a9207710facc61d2",SummonMultiplicity.OneD3,"monitor-lizard"),
            N(SummonFamily.Monster,4,"24ed31f7b42eb5f4ab461be90d479066",SummonMultiplicity.OneD4PlusOne,"wolf"),
            N(SummonFamily.Monster,5,"0964bf88b582bed41b74e79596c4f6d9",SummonMultiplicity.One,null),
            N(SummonFamily.Monster,5,"715f208d545be2f4aa2d3693e6347a5a",SummonMultiplicity.OneD3,"dire-wolf"),
            N(SummonFamily.Monster,5,"e331ef5f6b235f54aaab5f2bf7a64eb6",SummonMultiplicity.OneD4PlusOne,"monitor-lizard"),
            N(SummonFamily.Monster,6,"02de4dd8add69aa42a3d1330b573e2ab",SummonMultiplicity.One,null),
            N(SummonFamily.Monster,6,"237d76aebbb28334e95d83475611cb47",SummonMultiplicity.OneD3,null),
            N(SummonFamily.Monster,6,"59d28e07b948d4e45a7477ec0065ccb3",SummonMultiplicity.OneD4PlusOne,"dire-wolf"),
            N(SummonFamily.Monster,7,"2920d48574933c24391fbb9e18f87bf5",SummonMultiplicity.One,null),
            N(SummonFamily.Monster,7,"43f763d347eb2744caed9c656ba89531",SummonMultiplicity.OneD3,null),
            N(SummonFamily.Monster,7,"6e805a9e3ff445146a6386f5a704d4bc",SummonMultiplicity.OneD4PlusOne,null),
            N(SummonFamily.Monster,8,"eb6df7ddfc0669d4fb3fc9af4bd34bca",SummonMultiplicity.One,null),
            N(SummonFamily.Monster,8,"ddc1d195a4434374e860b1568cfc7d11",SummonMultiplicity.OneD3,null),
            N(SummonFamily.Monster,8,"12815be343acc4c45b24554089c8c4bc",SummonMultiplicity.OneD4PlusOne,null),
            N(SummonFamily.Monster,9,"e96593e67d206ab49ad1b567327d1e75",SummonMultiplicity.One,null),
            N(SummonFamily.Monster,9,"4988b2e622c6f2d4b897894e3be13f09",SummonMultiplicity.OneD3,null),
            N(SummonFamily.Monster,9,"22b9580c961c5e64f95f10ca9a82c564",SummonMultiplicity.OneD4PlusOne,null),

            N(SummonFamily.NaturesAlly,1,"b5b7cf07ffeb4533a1320fbd06072cc5",SummonMultiplicity.One,null),
            N(SummonFamily.NaturesAlly,2,"848bd9df8b2643143a7020be7cde8800",SummonMultiplicity.One,"giant-frog"),
            N(SummonFamily.NaturesAlly,2,"b8ac9c653789b2a46ad85a075734c0e2",SummonMultiplicity.OneD3,null),
            N(SummonFamily.NaturesAlly,3,"6db23a29c0c55c546a0feaef0c8d33d6",SummonMultiplicity.One,"leopard"),
            N(SummonFamily.NaturesAlly,3,"06d11dfa15e63bd41b01e09d5464ee8f",SummonMultiplicity.OneD3,"giant-frog"),
            N(SummonFamily.NaturesAlly,3,"bb1bac85be6b1e44eafdc54a3b757c3e",SummonMultiplicity.OneD4PlusOne,null),
            N(SummonFamily.NaturesAlly,4,"71dfb899a04db614e9db458ed4e43f56",SummonMultiplicity.One,"dire-boar"),
            N(SummonFamily.NaturesAlly,4,"eb259941d7c2c5144844a31e72810642",SummonMultiplicity.OneD3,"leopard"),
            N(SummonFamily.NaturesAlly,4,"3050599c1ca9a9b40940a9426d4f861b",SummonMultiplicity.OneD4PlusOne,"giant-frog"),
            N(SummonFamily.NaturesAlly,5,"28ea1b2e0c4a9094da208b4c186f5e4f",SummonMultiplicity.One,null),
            N(SummonFamily.NaturesAlly,5,"03e8e9605925b7140bdd331232b78d25",SummonMultiplicity.OneD3,"dire-boar"),
            N(SummonFamily.NaturesAlly,5,"87c64591b0e6f7542807429d14bb1723",SummonMultiplicity.OneD4PlusOne,"leopard"),
            N(SummonFamily.NaturesAlly,6,"060afb9e13d8a3547ad0dd20c407c0a5",SummonMultiplicity.One,"dire-tiger"),
            N(SummonFamily.NaturesAlly,6,"2aab2a0c280ed3e408a09967ec6bb281",SummonMultiplicity.OneD3,null),
            N(SummonFamily.NaturesAlly,6,"7aefdbd7e0933b744b9c85566d16e504",SummonMultiplicity.OneD4PlusOne,"dire-boar"),
            N(SummonFamily.NaturesAlly,7,"6d8d59aa38713be4fa3be76c19107cc0",SummonMultiplicity.One,"mastodon"),
            N(SummonFamily.NaturesAlly,7,"533f8cee65aa2fc448d6d2a7e5d28bb6",SummonMultiplicity.OneD3,"dire-tiger"),
            N(SummonFamily.NaturesAlly,7,"b81bb947975c4e34395ab4e09a036a16",SummonMultiplicity.OneD4PlusOne,null),
            N(SummonFamily.NaturesAlly,8,"8d3d5b62878d5b24391c1d7834d0d706",SummonMultiplicity.One,null),
            N(SummonFamily.NaturesAlly,8,"256739c1e61e3f64eaf71734d271f4be",SummonMultiplicity.OneD3,"mastodon"),
            N(SummonFamily.NaturesAlly,8,"86f4287572bef49449b9d06c66adf456",SummonMultiplicity.OneD4PlusOne,"dire-tiger"),
            N(SummonFamily.NaturesAlly,9,"f6751c3b22dbd884093e350a37420368",SummonMultiplicity.One,null),
            N(SummonFamily.NaturesAlly,9,"780cbc629e74c1049b041b2a2f979863",SummonMultiplicity.OneD3,null),
            N(SummonFamily.NaturesAlly,9,"9bd8cb6180842f44e9302c58e47b91f0",SummonMultiplicity.OneD4PlusOne,"mastodon")
        };

        internal static IReadOnlyList<SummonNativeOptionSpec> All
        { get { return Array.AsReadOnly(Values); } }

        internal static SummonNativeOptionSpec Find(SummonFamily family,
            int tier, string guid)
        {
            return Values.SingleOrDefault(value => value.Family == family &&
                value.Tier == tier && string.Equals(value.Guid, guid,
                    StringComparison.Ordinal));
        }

        internal static void Validate()
        {
            if (Values.Length != 48 || Values.Any(value => value.Tier < 1 ||
                value.Tier > 9 || value.Guid == null || value.Guid.Length != 32) ||
                Values.Select(value => value.Guid).Distinct(
                    StringComparer.Ordinal).Count() != Values.Length)
                throw new InvalidOperationException(
                    "Frozen native summon option catalog is malformed.");
            foreach (SummonNativeOptionSpec duplicate in Values.Where(value =>
                value.IsSemanticDuplicate))
            {
                SummonCreatureSpec creature = ExpandedSummoningCatalog.All
                    .SingleOrDefault(value => value.Key ==
                        duplicate.EquivalentCreatureKey);
                if (creature == null || (duplicate.Family == SummonFamily.Monster ?
                    creature.MonsterTier : creature.NaturesAllyTier) == null)
                    throw new InvalidOperationException(
                        "Native duplicate mapping lacks a KMG creature: " +
                        duplicate.Guid + ".");
            }
        }

        private static SummonNativeOptionSpec N(SummonFamily family, int tier,
            string guid, SummonMultiplicity multiplicity, string equivalent)
        { return new SummonNativeOptionSpec(family, tier, guid, multiplicity,
            equivalent); }
    }
}
