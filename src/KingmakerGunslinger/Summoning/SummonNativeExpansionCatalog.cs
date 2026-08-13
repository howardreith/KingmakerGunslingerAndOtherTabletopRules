using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    internal enum SummonNativeSpawnBranch
    {
        Direct,
        NonEvil,
        Evil
    }

    internal sealed class SummonNativeExpansionSpec
    {
        internal SummonNativeExpansionSpec(int tier, string creatureKey,
            string displayName, SummonMultiplicity multiplicity,
            string sourceAbilityGuid, string unitGuid,
            SummonNativeSpawnBranch branch)
        {
            Tier = tier; CreatureKey = creatureKey; DisplayName = displayName;
            Multiplicity = multiplicity; SourceAbilityGuid = sourceAbilityGuid;
            UnitGuid = unitGuid; Branch = branch;
        }

        internal int Tier { get; private set; }
        internal string CreatureKey { get; private set; }
        internal string DisplayName { get; private set; }
        internal SummonMultiplicity Multiplicity { get; private set; }
        internal string SourceAbilityGuid { get; private set; }
        internal string UnitGuid { get; private set; }
        internal SummonNativeSpawnBranch Branch { get; private set; }
        internal string Symbol { get { return "KMG.Summoning.NativeOption.SM.Tier" +
            Tier + "." + CreatureKey + "." + Multiplicity; } }
    }

    internal static class SummonNativeExpansionCatalog
    {
        private static readonly SummonNativeExpansionSpec[] Values = {
            N(5,"Redcap","Redcap",SummonMultiplicity.One,
                "0964bf88b582bed41b74e79596c4f6d9","af8bcff074b93c84b97938e486184222",SummonNativeSpawnBranch.Evil),
            N(6,"Axiomite","Axiomite",SummonMultiplicity.One,
                "02de4dd8add69aa42a3d1330b573e2ab","5adafaa653dc4de40b3ac73fa002a2ec",SummonNativeSpawnBranch.NonEvil),
            N(6,"SoulEater","Soul Eater",SummonMultiplicity.One,
                "02de4dd8add69aa42a3d1330b573e2ab","1832be68f9814254dbbdab6df7fd5d0b",SummonNativeSpawnBranch.Evil),
            N(6,"Redcap","Redcap",SummonMultiplicity.OneD3,
                "237d76aebbb28334e95d83475611cb47","af8bcff074b93c84b97938e486184222",SummonNativeSpawnBranch.Evil),
            N(7,"Bogeyman","Bogeyman",SummonMultiplicity.One,
                "2920d48574933c24391fbb9e18f87bf5","4f04734e4ad930e438eec3182dfd95fe",SummonNativeSpawnBranch.Direct),
            N(7,"Axiomite","Axiomite",SummonMultiplicity.OneD3,
                "43f763d347eb2744caed9c656ba89531","5adafaa653dc4de40b3ac73fa002a2ec",SummonNativeSpawnBranch.NonEvil),
            N(7,"SoulEater","Soul Eater",SummonMultiplicity.OneD3,
                "43f763d347eb2744caed9c656ba89531","1832be68f9814254dbbdab6df7fd5d0b",SummonNativeSpawnBranch.Evil),
            N(7,"Redcap","Redcap",SummonMultiplicity.OneD4PlusOne,
                "6e805a9e3ff445146a6386f5a704d4bc","af8bcff074b93c84b97938e486184222",SummonNativeSpawnBranch.Evil),
            N(8,"MovanicDeva","Movanic Deva",SummonMultiplicity.One,
                "eb6df7ddfc0669d4fb3fc9af4bd34bca","afe56099ff6046b40a359b7562c0424e",SummonNativeSpawnBranch.NonEvil),
            N(8,"FrostGiant","Frost Giant",SummonMultiplicity.One,
                "eb6df7ddfc0669d4fb3fc9af4bd34bca","590cd3d5e76fdc649a5f97bc984cd3c4",SummonNativeSpawnBranch.Evil),
            N(8,"Bogeyman","Bogeyman",SummonMultiplicity.OneD3,
                "ddc1d195a4434374e860b1568cfc7d11","4f04734e4ad930e438eec3182dfd95fe",SummonNativeSpawnBranch.Direct),
            N(8,"Axiomite","Axiomite",SummonMultiplicity.OneD4PlusOne,
                "12815be343acc4c45b24554089c8c4bc","5adafaa653dc4de40b3ac73fa002a2ec",SummonNativeSpawnBranch.NonEvil),
            N(8,"SoulEater","Soul Eater",SummonMultiplicity.OneD4PlusOne,
                "12815be343acc4c45b24554089c8c4bc","1832be68f9814254dbbdab6df7fd5d0b",SummonNativeSpawnBranch.Evil),
            N(9,"Thanadaemon","Thanadaemon",SummonMultiplicity.One,
                "e96593e67d206ab49ad1b567327d1e75","e287515e761bb1a48a61e1bbcb3527b1",SummonNativeSpawnBranch.Evil),
            N(9,"MovanicDeva","Movanic Deva",SummonMultiplicity.OneD3,
                "4988b2e622c6f2d4b897894e3be13f09","afe56099ff6046b40a359b7562c0424e",SummonNativeSpawnBranch.NonEvil),
            N(9,"FrostGiant","Frost Giant",SummonMultiplicity.OneD3,
                "4988b2e622c6f2d4b897894e3be13f09","590cd3d5e76fdc649a5f97bc984cd3c4",SummonNativeSpawnBranch.Evil),
            N(9,"Bogeyman","Bogeyman",SummonMultiplicity.OneD4PlusOne,
                "22b9580c961c5e64f95f10ca9a82c564","4f04734e4ad930e438eec3182dfd95fe",SummonNativeSpawnBranch.Direct)
        };

        internal static IReadOnlyList<SummonNativeExpansionSpec> All
        { get { return Array.AsReadOnly(Values); } }

        internal static IReadOnlyList<SummonNativeExpansionSpec> ForTier(int tier)
        { return Values.Where(value => value.Tier == tier).ToArray(); }

        internal static bool Replaces(int tier, string sourceGuid)
        { return Values.Any(value => value.Tier == tier && string.Equals(
            value.SourceAbilityGuid, sourceGuid, StringComparison.Ordinal)); }

        internal static void Validate()
        {
            if (Values.Length != 17 || Values.Any(value => value.Tier < 5 ||
                value.Tier > 9 || value.SourceAbilityGuid.Length != 32 ||
                value.UnitGuid.Length != 32) || Values.Select(value =>
                    value.Symbol).Distinct(StringComparer.Ordinal).Count() !=
                        Values.Length)
                throw new InvalidOperationException(
                    "Frozen native summon expansion catalog is malformed.");
            string[] sourceGuids = Values.Select(value => value.SourceAbilityGuid)
                .Distinct(StringComparer.Ordinal).ToArray();
            if (sourceGuids.Length != 12 || sourceGuids.Any(guid =>
                !SummonNativeOptionCatalog.All.Any(value =>
                    value.Family == SummonFamily.Monster &&
                    value.Guid == guid && !value.IsSemanticDuplicate)))
                throw new InvalidOperationException(
                    "Native expansion source is not an exact unique Owlcat option.");
        }

        private static SummonNativeExpansionSpec N(int tier, string key,
            string name, SummonMultiplicity multiplicity, string source,
            string unit, SummonNativeSpawnBranch branch)
        { return new SummonNativeExpansionSpec(tier, key, name, multiplicity,
            source, unit, branch); }
    }
}
