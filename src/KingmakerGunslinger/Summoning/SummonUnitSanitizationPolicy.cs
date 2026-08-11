using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    [Flags]
    internal enum SummonDonorHazard
    {
        None = 0,
        Experience = 1,
        Loot = 2,
        Inventory = 4,
        Interaction = 8,
        Dialogue = 16,
        QuestOrStory = 32,
        CompanionOrPet = 64,
        CampaignPersistence = 128,
        TeleportationOrPlanarTravel = 256,
        CreatureSummoningOrConjuration = 512,
        ExpensiveMaterialComponent = 1024,
        PersistentCorpse = 2048
    }

    internal sealed class SummonDonorMember
    {
        internal SummonDonorMember(string key, SummonDonorHazard hazards,
            bool requiredCombatMechanic)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("A donor-member key is required.", "key");
            Key = key;
            Hazards = hazards;
            RequiredCombatMechanic = requiredCombatMechanic;
        }

        internal string Key { get; private set; }
        internal SummonDonorHazard Hazards { get; private set; }
        internal bool RequiredCombatMechanic { get; private set; }
    }

    internal sealed class SummonUnitSanitizationPlan
    {
        internal SummonUnitSanitizationPlan(IList<SummonDonorMember> retained,
            IList<SummonDonorMember> removed, IList<SummonDonorMember> replacements)
        {
            Retained = new List<SummonDonorMember>(retained).AsReadOnly();
            Removed = new List<SummonDonorMember>(removed).AsReadOnly();
            RequiredReplacements = new List<SummonDonorMember>(replacements).AsReadOnly();
        }

        internal IReadOnlyList<SummonDonorMember> Retained { get; private set; }
        internal IReadOnlyList<SummonDonorMember> Removed { get; private set; }
        internal IReadOnlyList<SummonDonorMember> RequiredReplacements { get; private set; }
    }

    internal static class SummonUnitSanitizationPolicy
    {
        private static readonly string[] ForbiddenRuntimeTokens = {
            "summon", "conjuration", "teleport", "dimensiondoor",
            "dimension door", "planeshift", "plane shift", "greaterteleport",
            "gate", "profane gift", "profanegift", "dialog", "interaction",
            "quest", "cutscene", "companion", "animalcompanion", "petfeature",
            "loot", "inventory", "story", "kingdom"
        };
        internal const SummonDonorHazard ForbiddenHazards =
            SummonDonorHazard.Experience |
            SummonDonorHazard.Loot |
            SummonDonorHazard.Inventory |
            SummonDonorHazard.Interaction |
            SummonDonorHazard.Dialogue |
            SummonDonorHazard.QuestOrStory |
            SummonDonorHazard.CompanionOrPet |
            SummonDonorHazard.CampaignPersistence |
            SummonDonorHazard.TeleportationOrPlanarTravel |
            SummonDonorHazard.CreatureSummoningOrConjuration |
            SummonDonorHazard.ExpensiveMaterialComponent |
            SummonDonorHazard.PersistentCorpse;

        internal static SummonUnitSanitizationPlan CreatePlan(
            IEnumerable<SummonDonorMember> donorMembers)
        {
            if (donorMembers == null) throw new ArgumentNullException("donorMembers");
            var retained = new List<SummonDonorMember>();
            var removed = new List<SummonDonorMember>();
            var replacements = new List<SummonDonorMember>();
            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (SummonDonorMember member in donorMembers)
            {
                if (member == null) throw new ArgumentException("Donor members cannot contain null.", "donorMembers");
                if (!keys.Add(member.Key)) throw new ArgumentException("Duplicate donor-member key: " + member.Key, "donorMembers");
                if ((member.Hazards & ~ForbiddenHazards) != 0)
                    throw new ArgumentOutOfRangeException("donorMembers", "Unknown donor hazard on " + member.Key + ".");
                if ((member.Hazards & ForbiddenHazards) == 0) retained.Add(member);
                else
                {
                    removed.Add(member);
                    if (member.RequiredCombatMechanic) replacements.Add(member);
                }
            }
            return new SummonUnitSanitizationPlan(retained, removed, replacements);
        }

        internal static bool IsForbiddenRuntimeMemberKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return true;
            string normalized = key.Replace("_", " ").Replace("-", " ")
                .ToLowerInvariant();
            return ForbiddenRuntimeTokens.Any(token => normalized.Contains(token));
        }
    }
}
