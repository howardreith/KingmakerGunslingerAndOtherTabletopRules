using System;

namespace KingmakerGunslinger.Summoning
{
    internal sealed class SummonCreatureSpec
    {
        internal SummonCreatureSpec(string key, string displayName,
            int? monsterTier, bool monsterTemplated, int? naturesAllyTier,
            string visual)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Creature key is required.", "key");
            if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("Display name is required.", "displayName");
            ValidateTier(monsterTier, "monsterTier"); ValidateTier(naturesAllyTier, "naturesAllyTier");
            if (!monsterTier.HasValue && !naturesAllyTier.HasValue) throw new ArgumentException("A creature must belong to a summon family.");
            if (monsterTemplated && !monsterTier.HasValue) throw new ArgumentException("Only Summon Monster entries are templated.");
            Key = key; DisplayName = displayName; MonsterTier = monsterTier;
            MonsterTemplated = monsterTemplated; NaturesAllyTier = naturesAllyTier;
            Visual = visual ?? displayName;
        }
        internal string Key { get; private set; }
        internal string DisplayName { get; private set; }
        internal int? MonsterTier { get; private set; }
        internal bool MonsterTemplated { get; private set; }
        internal int? NaturesAllyTier { get; private set; }
        internal string Visual { get; private set; }
        internal SummonTemplatePolicy TemplatePolicy(SummonFamily family)
        {
            if (family == SummonFamily.Monster) {
                if (!MonsterTier.HasValue) throw new InvalidOperationException("Creature is not in the Summon Monster roster.");
                return MonsterTemplated ? SummonTemplatePolicy.CelestialOrFiendish : SummonTemplatePolicy.None;
            }
            if (!NaturesAllyTier.HasValue) throw new InvalidOperationException("Creature is not in the Summon Nature's Ally roster.");
            return SummonTemplatePolicy.CasterAlignment;
        }
        private static void ValidateTier(int? tier, string name)
        { if (tier.HasValue && (tier.Value < 1 || tier.Value > 9)) throw new ArgumentOutOfRangeException(name); }
    }
}
