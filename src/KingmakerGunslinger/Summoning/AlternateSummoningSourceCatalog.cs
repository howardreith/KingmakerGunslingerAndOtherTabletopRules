using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    internal enum AlternateSummoningProvider
    {
        Native,
        CallOfTheWild
    }

    /// <summary>
    /// Which host roots own a frozen preservation clone. A direct host root
    /// becomes a menu parent when it is published, so its original single
    /// cast is kept as a separate child rather than lost or cast twice.
    /// </summary>
    internal enum AlternateSummoningPreservation
    {
        /// <summary>Every root is direct: one per tier and quantity.</summary>
        PerQuantity,
        /// <summary>Only the tier-one root is direct; the rest are menus.</summary>
        TierOneOnly
    }

    /// <summary>
    /// The exact identity of one host template child: the caller's own
    /// executable ability for a tier and quantity, cloned for every creature
    /// published at that tier and quantity.
    /// </summary>
    internal sealed class AlternateSummoningTemplateSpec
    {
        internal AlternateSummoningTemplateSpec(SummonMultiplicity multiplicity,
            string guid, string name)
        {
            if (string.IsNullOrWhiteSpace(guid) || guid.Length != 32)
                throw new ArgumentException("A 32-character template GUID is required.", "guid");
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("A template name is required.", "name");
            Multiplicity = multiplicity; Guid = guid; Name = name;
        }

        internal SummonMultiplicity Multiplicity { get; private set; }
        internal string Guid { get; private set; }
        internal string Name { get; private set; }
    }

    /// <summary>
    /// One host level of a caller: a class or extract level, the summon tier
    /// it casts, the feature that grants it, and either the host root's exact
    /// identity or, for native roots that are only reachable through their
    /// granting feature, nothing more than that feature.
    /// </summary>
    internal sealed class AlternateSummoningRootSpec
    {
        internal AlternateSummoningRootSpec(int ordinal, int tier,
            string featureGuid, string featureName, string rootGuid,
            string rootName, IEnumerable<AlternateSummoningTemplateSpec> templates)
        {
            if (ordinal < 1 || ordinal > 9) throw new ArgumentOutOfRangeException("ordinal");
            if (tier < 1 || tier > 9) throw new ArgumentOutOfRangeException("tier");
            if (string.IsNullOrWhiteSpace(featureGuid) || featureGuid.Length != 32)
                throw new ArgumentException("A 32-character feature GUID is required.", "featureGuid");
            if ((rootGuid == null) != (rootName == null))
                throw new ArgumentException("A root GUID and name are declared together.");
            Ordinal = ordinal; Tier = tier; FeatureGuid = featureGuid;
            FeatureName = featureName; RootGuid = rootGuid; RootName = rootName;
            Templates = (templates ?? Enumerable.Empty<AlternateSummoningTemplateSpec>())
                .ToArray();
        }

        /// <summary>The caller's own level ordinal (tier, or extract level).</summary>
        internal int Ordinal { get; private set; }
        /// <summary>The Summon Monster / Nature's Ally tier this root casts.</summary>
        internal int Tier { get; private set; }
        internal string FeatureGuid { get; private set; }
        internal string FeatureName { get; private set; }
        /// <summary>Null when the roots are resolved through the feature.</summary>
        internal string RootGuid { get; private set; }
        internal string RootName { get; private set; }
        /// <summary>Empty when the root itself is the direct one-creature cast.</summary>
        internal IReadOnlyList<AlternateSummoningTemplateSpec> Templates
        { get; private set; }
        internal bool IsDirect { get { return Templates.Count == 0; } }
    }

    internal sealed class AlternateSummoningFamilySpec
    {
        internal AlternateSummoningFamilySpec(string key, string displayName,
            SummonFamily family, AlternateSummoningProvider provider,
            AlternateSummoningPreservation preservation,
            IEnumerable<AlternateSummoningRootSpec> roots)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("A family key is required.", "key");
            Key = key; DisplayName = displayName; Family = family;
            Provider = provider; Preservation = preservation;
            Roots = (roots ?? Enumerable.Empty<AlternateSummoningRootSpec>()).ToArray();
        }

        internal string Key { get; private set; }
        internal string DisplayName { get; private set; }
        internal SummonFamily Family { get; private set; }
        internal AlternateSummoningProvider Provider { get; private set; }
        internal AlternateSummoningPreservation Preservation { get; private set; }
        internal IReadOnlyList<AlternateSummoningRootSpec> Roots { get; private set; }
        internal IEnumerable<int> Tiers { get { return Roots.Select(value => value.Tier); } }
    }

    /// <summary>
    /// The alternate summoning callers this module adapts, with the exact
    /// identities of the installed host graph. The roster is not here: every
    /// caller projects the one central catalog through
    /// <see cref="AlternateSummoningProjection"/>.
    ///
    /// <para>The Call of the Wild identities are the fixed name-to-GUID
    /// entries of its shipped registry (1.14.4c-2.1, blueprints.txt SHA-256
    /// f227b1c3...), which it assigns to these names on every load. They are
    /// exact lookups, never searches; a changed name or graph blocks that one
    /// family with a precise status and leaves its original behavior in
    /// place.</para>
    ///
    /// <para>The <c>I(...)</c> identity rows are read by
    /// <c>tools/expanded_summoning_manifest.py</c>, which allocates every
    /// adapter identity from the catalog. Adding a creature to the catalog
    /// and running that tool is the whole of roster propagation; nothing here
    /// names a creature.</para>
    /// </summary>
    internal static class AlternateSummoningSourceCatalog
    {
        internal const string MonsterTacticianKey = "MonsterTactician";
        internal const string SummonerKey = "Summoner";
        internal const string MasterSummonerKey = "MasterSummoner";
        internal const string OccultistKey = "Occultist";
        internal const string NaturalistKey = "Naturalist";
        internal const string FeralHunterKey = "FeralHunter";
        internal const string PreservationistKey = "Preservationist";

        internal const string CotwModId = "CallOfTheWild";

        // Identity rows. Format is load-bearing for the manifest tool:
        // I("<key>", SummonFamily.<family>, "<tiers>", AlternateSummoningPreservation.<mode>)
        private static readonly AlternateSummoningFamilyIdentity[] IdentityRows = {
            I("MonsterTactician", SummonFamily.Monster, "1,2,3,4,5,6,7,8,9", AlternateSummoningPreservation.PerQuantity),
            I("Summoner", SummonFamily.Monster, "1,2,3,4,5,6,7,8,9", AlternateSummoningPreservation.TierOneOnly),
            I("MasterSummoner", SummonFamily.Monster, "1,2,3,4,5,6,7,8,9", AlternateSummoningPreservation.TierOneOnly),
            I("Occultist", SummonFamily.Monster, "1,2,3,4,5,6,7,8,9", AlternateSummoningPreservation.TierOneOnly),
            I("Naturalist", SummonFamily.NaturesAlly, "1,2,3,4,5,6,7,8,9", AlternateSummoningPreservation.TierOneOnly),
            I("FeralHunter", SummonFamily.NaturesAlly, "1,2,3,4,5,6,7,8,9", AlternateSummoningPreservation.TierOneOnly),
            I("Preservationist", SummonFamily.NaturesAlly, "1,2,4,5,7,9", AlternateSummoningPreservation.TierOneOnly)
        };

        // Built in the static constructor, after every field initializer the
        // builder reads has run.
        private static readonly AlternateSummoningFamilySpec[] Families;

        static AlternateSummoningSourceCatalog()
        {
            Families = Build();
        }

        internal static IReadOnlyList<AlternateSummoningFamilySpec> All
        { get { return Array.AsReadOnly(Families); } }

        internal static IReadOnlyList<AlternateSummoningFamilyIdentity> Identities
        { get { return Array.AsReadOnly(IdentityRows); } }

        internal static AlternateSummoningFamilySpec Find(string key)
        {
            return Families.SingleOrDefault(value => string.Equals(value.Key,
                key, StringComparison.Ordinal));
        }

        /// <summary>
        /// The native Monster Tactician summon features, tiers I-IX. Their
        /// granted abilities are resolved from these features at runtime.
        /// </summary>
        internal static readonly string[] MonsterTacticianFeatureGuids = {
            "c19f6f34ed0bc364cbdec88b49a54f67", "45e466127a8961d40bb3030816ed245b",
            "ea26b3a3acb98074fa34f80fcc4e497d", "03168f4f13ff26f429d912085e88baba",
            "00fda605a917fcc4e89612dd31683bdd", "9b14b05456142914888a48354a0eec17",
            "667fd017406abd548b89292edd7dbfb7", "20d72612311ba914aaba5cc8a4cf312c",
            "f63d23b4e41b3264fa6aa2be8079d28d" };

        internal static void Validate()
        {
            if (Families.Length != IdentityRows.Length ||
                Families.Select(value => value.Key).Distinct(StringComparer.Ordinal)
                    .Count() != Families.Length)
                throw new InvalidOperationException(
                    "Alternate summoning family table is malformed.");
            foreach (AlternateSummoningFamilySpec family in Families)
            {
                AlternateSummoningFamilyIdentity identity = IdentityRows.Single(
                    value => value.Key == family.Key);
                if (identity.Family != family.Family ||
                    identity.Preservation != family.Preservation ||
                    !identity.Tiers.SequenceEqual(family.Tiers))
                    throw new InvalidOperationException(
                        "Alternate summoning identity row disagrees with family " +
                        family.Key + ".");
                if (family.Roots.Select(value => value.Ordinal).Distinct().Count() !=
                    family.Roots.Count ||
                    family.Roots.Select(value => value.Tier).Distinct().Count() !=
                    family.Roots.Count)
                    throw new InvalidOperationException(
                        "Alternate summoning family repeats a level: " + family.Key);
                foreach (AlternateSummoningRootSpec root in family.Roots)
                {
                    SummonMultiplicity[] expected = Quantities(root.Tier).ToArray();
                    if (root.RootGuid == null)
                    {
                        if (family.Provider != AlternateSummoningProvider.Native ||
                            family.Preservation != AlternateSummoningPreservation.PerQuantity)
                            throw new InvalidOperationException(
                                "Only native per-quantity roots are feature-resolved: " +
                                family.Key);
                        continue;
                    }
                    if (root.IsDirect != (root.Tier == 1) ||
                        (!root.IsDirect && !root.Templates.Select(value =>
                            value.Multiplicity).SequenceEqual(expected)) ||
                        root.Templates.Select(value => value.Guid).Distinct(
                            StringComparer.Ordinal).Count() != root.Templates.Count)
                        throw new InvalidOperationException(
                            "Alternate summoning root template set is malformed: " +
                            family.Key + " tier " + root.Tier + ".");
                }
            }
            string[] guids = Families.SelectMany(family => family.Roots)
                .SelectMany(root => new[] { root.FeatureGuid, root.RootGuid }
                    .Concat(root.Templates.Select(value => value.Guid)))
                .Where(value => value != null).ToArray();
            if (guids.Distinct(StringComparer.Ordinal).Count() != guids.Length)
                throw new InvalidOperationException(
                    "Alternate summoning host identities collide.");
        }

        /// <summary>The quantities a summon tier offers: one creature always,
        /// 1d3 from tier II, 1d4+1 from tier III.</summary>
        internal static IEnumerable<SummonMultiplicity> Quantities(int tier)
        {
            if (tier < 1 || tier > 9) throw new ArgumentOutOfRangeException("tier");
            yield return SummonMultiplicity.One;
            if (tier >= 2) yield return SummonMultiplicity.OneD3;
            if (tier >= 3) yield return SummonMultiplicity.OneD4PlusOne;
        }

        private static AlternateSummoningFamilySpec[] Build()
        {
            return new[] {
                new AlternateSummoningFamilySpec(MonsterTacticianKey,
                    "Monster Tactician Summon Monster", SummonFamily.Monster,
                    AlternateSummoningProvider.Native,
                    AlternateSummoningPreservation.PerQuantity,
                    MonsterTacticianFeatureGuids.Select((guid, index) =>
                        new AlternateSummoningRootSpec(index + 1, index + 1, guid,
                            null, null, null, null))),
                CotwMonster(SummonerKey, "Summoner Summon Monster",
                    "SummonerSummon", "SummonerSummonMonster", new[] {
                    "ecab08a1d14e4d268a3242da8133d837|156cbdb36ef141dc88f16775fede4b27|",
                    "01368ec31bcf4e0498a57b3cc40ec65d|70aa881ddfbc4160ad169834177e4812|693d0b15426d41afb86ba1cf7325a369,6eb93a3cfd644a48ac05279ea49fb244",
                    "ac6cceacb2734a318cf510cbf435f14b|48126175b6bc4af6985ce5aab973ce48|731757a714a7449482710c6015566a8d,ec5791c8402d432e95f2551ebec2328e,d7aab616c4f14136aefe4f84fd107644",
                    "4b3dd07f0f60486b9bc9f5fae67d70d5|8a90a3c110604aac928b1eac3ed3fdb1|0f39db6b45444b6793cc9ec1127b3a14,1c466745122046319435481549123ffa,1b85099406d3429f9fd419dd654fcb5a",
                    "534b5a92cb314bec8f5e287c6b4c8c3f|b65c15d2e6be4e13a832ee6590740022|c8aaf0e8df3e4f6b8e5758895b82bbdc,6fffdd5ec4f845a7930e719b4cbab5b0,348193e582d54795a4c660e19871db81",
                    "77cce254daed4cc1af8f5c138b60b336|9a0705fdb209492aacee2621db342ad4|f5c115b9a4cd41f4914b0dead785f5a4,ce43eb5a87a14d44bbcf0785d67b4276,40b2bfaf178a49de819d481d56ad6efb",
                    "4a961241591741c89ef6f96a721e53c8|1afbb13d91e141d288087108a797bee6|88655a5eafc34bbb9c3690324753d327,0952c0f400d7497c9235b73ce3a18ab7,6a62b709b82b4ca2a4696d817cc86f28",
                    "f5170a2bf3724181aac0bae7fee1d3d1|a993275cd3df4368baa0f8073fa090f8|e745631679eb4c31b6a25016637339a3,08f02932cc564ced8f751f7261975ace,b99eaf2304e34b90ae220e377b5b8a5b",
                    "73479f3f323b4d4ab4cdae72b78b0e03|d001c51f18e24533aa840b47d6de0b3f|49af943679c64944a7551fe7ba5bc603,a2ff89ee81434d11a0d321047bbfb177,e0fee9e4ee7948299bb574b197488c70" }),
                CotwMonster(MasterSummonerKey, "Master Summoner Summon Monster",
                    "MasterSummonerSummon", "MasterSummonerSummonMonster", new[] {
                    "9fe7d4397f5c410d9397ea47773251cb|abcff90e1fdc4729a26f886c12aa0b9a|",
                    "0aea0ac3dd34488599d9fd2d43228616|59bf489221944543af3f60a389edf865|079e5ea1044944b4b40690bb89b63667,52c19f137e7b4fc39956993caa91e384",
                    "dd670ffffea04ff5a773baa07b5abf2e|bc03ebc61f314cb4a3fe60f12971cf80|d71051e0b6c1415695da7ae6aa1a077e,028ab49c8d2142009e82f3893a7e7c0b,f5ab645ec6334a37a72df4142103e900",
                    "5554be32d5164877acbdd892b2f13155|00917b1ae75a44fbb8be4f096a2cc3fe|60da9f1b5e81430ebd2c921111438855,4ec0b0aa8b924ef48567acfce574805c,67505a230bbd4aeeb4685b7871f34797",
                    "f3942720d7524fbc804ec6d8c22f3842|8b2258533da143a6a16c21dc88b1ef3d|ccb5cb4f69244f3bbcd9a000362a2491,e0e5aa0c93e64ff4b6945c8e46b5c1aa,ba1e38dfd4ed4186b5fab9800072807c",
                    "05a3353e57564bca905b01180f97bb54|9de437f9cfd84d00b9cb3f1b4357a244|02860d5120504c5b977b8850ec9002c3,6e17c7af8c694f55867c44a1f6a2fc81,2841e4e8b3af4d0fae1938de6ffb4123",
                    "3b4b5904ae87411bb470cb447dcb9a0c|8ec51077e67844ef92abaf33de6c7c17|df04ef00af974c4fb46085ef50df004d,df88d3c5f02f46329414f1d4e77fb8f6,871a5ed396ef4b4c8dfd2e6ce12209ea",
                    "4808a41fad5c412183e49d587b98be06|933039e7336e4790b683e3bae3bee3ba|48681a7cefcf40de9c928ed33c608c39,03a9b707ac0e4434a644d5f1e4071d54,dea4a24f0c7f4c388601e8486bd38893",
                    "9d2356a224be452797be68f72105185f|292b92dd18e74503805ecfab6f79d8c6|8e3020520d3e4b91942bc8fc37f5d360,7da4b5d0e9694ef5ad83e3d346575618,e4b90d8445ee4c3b9d04936fd6c5efc0" }),
                CotwMonster(OccultistKey, "Occultist Conjurer's Focus",
                    "OccultistSummon", "OccultistSummonMonster", new[] {
                    "d6d71d5c6141461ebef4c6cb8012a058|0934dc6675574727ab454ce91b0a8d48|",
                    "e50c2695c01f468aab1d4179be85e8c0|595ae4f22ee04497b90b4ab6f9f8b0aa|a284bd40aabe4380bdae121576a361b7,89313a0d934d4adb9c1b3828007a5a14",
                    "e63a46a9c0fc4abc880b0bae72fea726|2a7fd62bc646439d9d96231b0be4ad7e|8956ee9a6e8d4b9597239eb238c26ffe,1414b58b1aeb455886a48c982e2a53c9,22214fe254d14d6bb4ad8701e650b961",
                    "24472dfe55f54e69a2fdb81cc59fbc67|b7ed8df5fa634a7185763550972ef21a|50437c5bfce94f0fa8534d412c816e2e,787b0024535b4f17a7bd9d0439e8e4e7,36b82001acb045d1b97f8c1f9ce37ea5",
                    "1dd0dc764ec54daface5401aef7b2a12|3668ae38ec1041d4bd4ef680e2fadeef|7165a20124db4d61af02e3c2fba4c8ed,b341e5ba259c431c83101170de2c0ba9,2b727356e5074777b9252c6ca77a03c6",
                    "e050b9ab32254626b325b1f87b32ace0|10976761dae041e88a65331c9f665333|af4f50c1313048f88c41f986bbfb1df5,bf22b3b4996b4870accb6e3190774b3b,8b7f6ba54c1c4a4a873f4e3fb6706c01",
                    "a27a84be6ff7480a85cc85d8b0ceba1e|25064004719049d3b8d09bb29a092e36|565764d1e8454e5cb4e9c06d9f4d4c50,ef0f0af4b88640eeb4b9545718e41b3b,f6a42680f8e04082a8b5ac0a33fc44fe",
                    "359596fb5f56452797e7bbf01ceb40d7|ddfeb2c3f87348d89067a9fce4e30fef|450cbec367bd41b98cc0ac7247a4be5e,49b92a032c9d43cda47a60624ae02601,56bc0a3c5b69427282ccb33275ebd08b",
                    "af0377bc58e945729072c84cf4fa3d85|e5e0482c66f44ec3a47b84c86a6c4d27|230ea7417d594b7496872b496d3da49f,c3bfa708f591459a8ac4b57b8260338a,44509febf69b4b97997af22e7e4ecba9" }),
                // The Naturalist's Summon Nature's Ally is CotW's shared
                // implementation whose identifiers say FeyCaller; the Fey
                // Caller archetype itself is not offered by this CotW build.
                CotwAlly(NaturalistKey, "Naturalist Summon Nature's Ally",
                    "FeyCallerSummonNaturesAlly", "SummonerSummonNaturesAlly",
                    "SummonerSummonNaturesAlly", new[] {
                    "59dc4905a9604e0f8699634f3eef3e58|a427024a2d6a4bd9abdc310366e5c4f4|",
                    "45e63334f5bf4bd89daeed7d8ccb20a9|7336aeb9aa774b8e9527c0dd93a0d012|ce65b1d4100b4766a6417af496a0c15b,86d78f11799c4a8e9047a5a03f8b5948",
                    "2f342e87c07e49bd8360f8d799838502|a9c3133c5ebd4e8789c38703efd5c24d|4d3a3a1206d04bf9bd228af72cd27b5b,689f38cfdb584334af107e55482b3208,cd1eb75dd6c24e2a8330d0a4a51f2e81",
                    "2a6ae8b7ec4845f3bd78f3fa3e42aab9|f73b113af4e4492d88eac359275f257d|e07cb954309d46f2bd0e4f457d0e96ed,2b766e3a22cb4aa9a6e672c68c776143,aec9daa6cc984d3c88bf7179ee82d18f",
                    "8babb7e7b5a04dca86d4f12612b90b1c|d177c053afdc4c27bec53474af2c1a5a|8b380ae6772f47ac920cac6e70f6367f,9b127216425d47a196de4c0ccb7d450c,d29ada02ab3a4ed5a603887a987ff5dd",
                    "33c8ab816a0c4e5d8745522f69a8f1a6|fdf6a0dec4274f5a8e84ad2149677bd8|994e9094232648c9aebeb953a38b6bd2,655e008086a54fde835d28a219b17c28,b1c4373df8ae42d8b9091952756c993a",
                    "a1ec389a633b435eb332006816dee0be|a879f32df7ff45dc98848504255c1795|423043d9dd1243a38bf07d45f689368d,313dba287386411db886cbff3a06c22a,38843cbc89c7404daec07215df6735cd",
                    "c5e7eec835a2475dbc9a47e52c34c831|f6e04d364e284b7d9864bb94707445be|3797293ac94548af87cabda7d206542c,f1cefddedce84fb3b49143497bb64e03,b4d8ae28345c4e79a0f595c84f1e7c62",
                    "e37c9e16de064aeb987024aa014a4121|5de58cbd302d46448055df689fac0b10|ee3a11f266b944e08e09127b45a1e991,a9c64329ded64067ad9bfe30410d825c,ead8341ac4f54bbf9b99551d4eed2498" }),
                CotwAlly(FeralHunterKey, "Feral Hunter Summon Nature's Ally",
                    "FeralHunterSummonNaturesAlly", "FeralHunterSummonNaturesAlly",
                    "FeralHunterSummonNaturesAlly", new[] {
                    "d9c501fd65a244ef8ffd27818b11633b|badb1d322573481b8da66f18a7533047|",
                    "2735ea914ee24a70b368142d70ed6bc7|8acc7c1befe14e2c95c6ac22bb8bd0a5|cbf16c10f3d54451936073430c061495,48c6ea1c6a3d4afdaab1e3381b9a7496",
                    "769917b1d13a4b0c9e3696ce9f05d16b|94d6de6d75a24fe0871b594ec3e07e87|31f835915d564f95b8cf42b8b2355a3b,b606534474914a118efbf10e5db2a591,3762aaaa882c41fe82b180988f57b67e",
                    "f27cb8da662d492ca9927d056573b1b1|29d2760e342c482cb11b50d740176bf0|a60b9221cccf4f76b3132dfee730433f,28c2ad13bde44601a02071b2681eac72,80ef9422a5f84f43beb7be51c51d9bc5",
                    "0882fbfbac114023955f319d003678a0|aa2823b4b3f64204976a5c927f7855c3|fb9923440550422fb5d1438be736ace3,d08860562a0b4254890f361624f9cbc8,f39f9617c5f64ca88d4bd36433f33b16",
                    "cd938d00ff944388a5ea71706ea66091|11a718d95ef548419090be5d1f07397c|f0b2d15c030f459884d5a2af8f5d62cb,29777c6049f34be1af97026cf8e8e5c9,c552956e84b545d2ad27699576d038f9",
                    "7dfa9c2ce5404d86bc5da2def47232cc|c1a17dcb835e48ef95ae0ccf393329c7|bd533545d605479d882b40db18578948,df92d128763744d8bc0d82e7fb8facc6,6ae3a14be4314ea6883f3318580c1ad0",
                    "f60da87d8bfb407fa76b3dbad6b9e006|2b381f6fc47e485da5ba3236dc385427|537fed2c78ca4fb4bb8796739afed42a,b9bb91ea433d4366a13537b6ffd934c4,ebf546fe6d1f47658d612e518f2829fd",
                    "2fcc2926d8904323806a2a2e9276ac56|c1448ec7d24d47a7a90b40995d631e1a|e4f82654734e41b4a8fb4e12453349f9,bb7f284b01624ca8a7369d90a874fa4f,cdbace1d3119493aa8b23bc7e90c86ce" }),
                Preservationist()
            };
        }

        private static readonly string[] Roman = {
            "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX" };
        private static readonly string[] QuantityToken = {
            "Single", "d3", "d4plus1" };

        /// <summary>
        /// Call of the Wild's three Summon Monster callers share one
        /// construction: tier I is a direct copy of the native Monster
        /// Tactician tier-I ability and tiers II-IX are CotW variant wrappers
        /// over copies of the tactician's single/1d3/1d4+1 abilities.
        /// </summary>
        private static AlternateSummoningFamilySpec CotwMonster(string key,
            string display, string prefix, string featurePrefix, string[] rows)
        {
            return new AlternateSummoningFamilySpec(key, display,
                SummonFamily.Monster, AlternateSummoningProvider.CallOfTheWild,
                AlternateSummoningPreservation.TierOneOnly,
                rows.Select((row, index) => CotwRoot(index + 1, index + 1, row,
                    index == 0 ? prefix + "ISingle" : prefix + (index + 1) + "Base",
                    featurePrefix + (index + 1) + "Feature",
                    prefix + Roman[index])));
        }

        /// <summary>
        /// The Naturalist and Feral Hunter copy the native Summon Nature's
        /// Ally children: tier I is the copied direct spell, II-IX are CotW
        /// wrappers over the copied single/1d3/1d4+1 children.
        /// </summary>
        private static AlternateSummoningFamilySpec CotwAlly(string key,
            string display, string childPrefix, string wrapperPrefix,
            string featurePrefix, string[] rows)
        {
            return new AlternateSummoningFamilySpec(key, display,
                SummonFamily.NaturesAlly, AlternateSummoningProvider.CallOfTheWild,
                AlternateSummoningPreservation.TierOneOnly,
                rows.Select((row, index) => CotwRoot(index + 1, index + 1, row,
                    index == 0 ? childPrefix + "I" : wrapperPrefix + (index + 1) + "Base",
                    featurePrefix + (index + 1) + "Feature",
                    childPrefix + Roman[index])));
        }

        /// <summary>
        /// Bottled Ally I-VI: the preservationist formula book gains copies of
        /// Summon Nature's Ally I, II, IV, V, VII and IX as 1st- to 6th-level
        /// extracts. The extract level is the root ordinal; the summon tier is
        /// the copied spell's.
        /// </summary>
        private static AlternateSummoningFamilySpec Preservationist()
        {
            string[] rows = {
                "1|1a0edb8e3fb345359bda0426ab894992|4a07f2476f794d8bb4a2e23df470747b|",
                "2|a36f29614d9247669ad7a417306a8ea5|83aa67e4e91e41e29be39479f45a02f6|aa133f92ec994d1584741cefd936bf0c,fbed2efa4aba4b0bbb84cef9e4af00ef",
                "4|975de352739241388a00d07d830d6678|de350ca901d1423d9a823beab1d9d254|84c6f0ed3fa147229cd6af1e0a3b563a,b40976a0d26c48a7a88a4af8e21bab43,2892668a69404a6396a77557c4e259a1",
                "5|3eb0c1b3b63f4179a2a9d37cf52b61af|8fbda9dfa8194a84b0f9e5f8e6437168|a73d76389aeb4858a55d50c7c154b13a,123466f3736048cda1dd2a5611a23497,d7440feeab0547ae994e9852d910ae96",
                "7|e8dfb9a2d6eb479485606e5e37298639|321dd90519604fb5bb07ae260e73ea9b|615fb275f6ac4919b239cb8904f685bf,b4944679727c4649a3ca358536783506,9dc9ab14b06a4e62a0f4bdb2b8af95cb",
                "9|80d17e321001412db70925570286366f|ac3dba900cb246ada9f6026543760fdf|abee55530236447e8405d72fa56679b2,808e76f745ce44c78d495def323434da,7be82e7eef8242ba9f8e7b663071ac96" };
            return new AlternateSummoningFamilySpec(PreservationistKey,
                "Preservationist Bottled Ally", SummonFamily.NaturesAlly,
                AlternateSummoningProvider.CallOfTheWild,
                AlternateSummoningPreservation.TierOneOnly,
                rows.Select((row, index) =>
                {
                    int tier = int.Parse(row.Substring(0, row.IndexOf('|')),
                        System.Globalization.CultureInfo.InvariantCulture);
                    string name = "PreservationistSummonNaturesAlly" + Roman[tier - 1];
                    return CotwRoot(index + 1, tier, row.Substring(row.IndexOf('|') + 1),
                        name, "PreservationistBottledAlly" + (index + 1) + "Feature",
                        name);
                }));
        }

        private static AlternateSummoningRootSpec CotwRoot(int ordinal, int tier,
            string row, string rootName, string featureName, string childStem)
        {
            string[] parts = row.Split('|');
            if (parts.Length != 3) throw new InvalidOperationException(
                "Malformed alternate summoning root row: " + row);
            string[] children = parts[2].Length == 0 ? new string[0] :
                parts[2].Split(',');
            return new AlternateSummoningRootSpec(ordinal, tier, parts[1],
                featureName, parts[0], rootName, children.Select((guid, index) =>
                    new AlternateSummoningTemplateSpec((SummonMultiplicity)index,
                        guid, childStem + QuantityToken[index])));
        }

        private static AlternateSummoningFamilyIdentity I(string key,
            SummonFamily family, string tiers,
            AlternateSummoningPreservation preservation)
        {
            return new AlternateSummoningFamilyIdentity(key, family,
                tiers.Split(',').Select(value => int.Parse(value,
                    System.Globalization.CultureInfo.InvariantCulture)),
                preservation);
        }
    }

    /// <summary>
    /// The part of a family that decides its persistent identities: its key,
    /// summon family, the tiers it casts and which roots are direct. Nothing
    /// else about a caller can move an adapter GUID.
    /// </summary>
    internal sealed class AlternateSummoningFamilyIdentity
    {
        internal AlternateSummoningFamilyIdentity(string key, SummonFamily family,
            IEnumerable<int> tiers, AlternateSummoningPreservation preservation)
        {
            Key = key; Family = family; Tiers = tiers.ToArray();
            Preservation = preservation;
            if (Tiers.Count == 0 || Tiers.Any(value => value < 1 || value > 9) ||
                Tiers.Distinct().Count() != Tiers.Count)
                throw new ArgumentException("Adapter tiers are malformed: " + key);
        }

        internal string Key { get; private set; }
        internal SummonFamily Family { get; private set; }
        internal IReadOnlyList<int> Tiers { get; private set; }
        internal AlternateSummoningPreservation Preservation { get; private set; }
    }
}
