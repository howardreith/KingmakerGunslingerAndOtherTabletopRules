using System;
using System.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Evidence interpretation only. Never grants a fact or changes sanitation.
    internal static class ExpandedSummoningInventoryObservationPolicy
    {
        private static readonly string[][] References = {
            new[] { "714b04102eae4cd2be200a8f09f04bc3", "KMG_Summoning_Unit_FireBeetle", "cec8ed5eaae2452799e8e56230ee6a92", "KMG_Summoning_Natural_FireBeetle_Luminescence", "AddFacts" },
            new[] { "9db94712f7b540679b8ff650dbaf746a", "KMG_Summoning_Unit_GiantAntWorker", "d20e4532fc324720952eac8af51b1f49", "KMG_Summoning_Natural_GiantAnt_RacialSkills", "AddFacts" },
            new[] { "ef2cd2abd3254cb2a90952253add918a", "KMG_Summoning_Unit_GiantAntSoldier", "d20e4532fc324720952eac8af51b1f49", "KMG_Summoning_Natural_GiantAnt_RacialSkills", "AddFacts" },
            new[] { "ef2cd2abd3254cb2a90952253add918a", "KMG_Summoning_Unit_GiantAntSoldier", "f6e3fe99f6e14fb2a7f8a7b574047f56", "KMG_Summoning_Natural_GiantAnt_Poison", "AddFacts" },
            new[] { "ef2cd2abd3254cb2a90952253add918a", "KMG_Summoning_Unit_GiantAntSoldier", "f4002c9c7d51450eb7fdd19acc018525", "KMG_Summoning_Special_GiantAntSoldier_Traits", "AddFacts" },
            new[] { "9387ef0ca1cd403a985893a85a6c7806", "KMG_Summoning_Unit_GiantAntDrone", "d20e4532fc324720952eac8af51b1f49", "KMG_Summoning_Natural_GiantAnt_RacialSkills", "AddFacts" },
            new[] { "9387ef0ca1cd403a985893a85a6c7806", "KMG_Summoning_Unit_GiantAntDrone", "f6e3fe99f6e14fb2a7f8a7b574047f56", "KMG_Summoning_Natural_GiantAnt_Poison", "AddFacts" },
            new[] { "9387ef0ca1cd403a985893a85a6c7806", "KMG_Summoning_Unit_GiantAntDrone", "12a93411b5e24c8a90d07470e43a613e", "KMG_Summoning_Special_GiantAntDrone_Traits", "AddFacts" },
            new[] { "fc294cc691284525814324ac5bc5bf75", "KMG_Summoning_Unit_GiantStagBeetle", "e4421da48c564c4f8553c4538a5ae0c4", "KMG_Summoning_Special_GiantStagBeetle_Trample", "AddFacts" },
            new[] { "ed3fa562802b418ab062a7da622874da", "KMG_Summoning_Unit_Crocodile", "727024ee29ac445a826c3b0fd6fa6056", "KMG_Summoning_Special_Crocodile_Sprint", "AddAbilityToCharacterComponent.Abilities" },
            new[] { "ed3fa562802b418ab062a7da622874da", "KMG_Summoning_Unit_Crocodile", "a254feca264547ce9157b781c83de88c", "KMG_Summoning_Special_Crocodile_CombatTraits", "AddFacts" },
            new[] { "95bd522bb3a444d49986ba12b7822f10", "KMG_Summoning_Unit_DireCrocodile", "73f3223a33ee435cbaa960638a912175", "KMG_Summoning_Special_DireCrocodile_Sprint", "AddAbilityToCharacterComponent.Abilities" },
            new[] { "95bd522bb3a444d49986ba12b7822f10", "KMG_Summoning_Unit_DireCrocodile", "89704fe84b90420a82bfcb2565bdc14d", "KMG_Summoning_Special_DireCrocodile_CombatTraits", "AddFacts" },
            new[] { "d8be82543ab64dc988c33e9f13608bad", "KMG_Summoning_Unit_Viper", "6205f9c3e04c4af699c819010948333a", "KMG_Summoning_Natural_Viper_CombatProfile", "AddFacts" },
            new[] { "d8be82543ab64dc988c33e9f13608bad", "KMG_Summoning_Unit_Viper", "bcfdbf44461e4617beec2dee43cea9d0", "KMG_Summoning_Natural_Viper_Poison", "AddFacts" },
            new[] { "f1a2eadf588e4c3b9fb670724d706364", "KMG_Summoning_Unit_ConstrictorSnake", "829b15325a114e6c93a8561c5960736e", "KMG_Summoning_Natural_ConstrictorSnake_CombatProfile", "AddFacts" },
            new[] { "f1a2eadf588e4c3b9fb670724d706364", "KMG_Summoning_Unit_ConstrictorSnake", "f83dfefcac58495c9a0f5c89a4483ddf", "KMG_Summoning_Special_ConstrictorSnake_CombatTraits", "AddFacts" },
        };

        internal static string[][] QualifiedReferenceRows
        { get { return References.Select(r => (string[])r.Clone()).ToArray(); } }

        internal static bool QualifiedProjectReference(string unitGuid, string unitName,
            string referenceGuid, string referenceName, string carrier)
        {
            return References.Any(r => r[0] == unitGuid && r[1] == unitName &&
                r[2] == referenceGuid && r[3] == referenceName && r[4] == carrier);
        }

        internal static bool SalamanderCarriers(bool spearNatural, int tailCount,
            bool tailNatural, int tailDice, int tailSides, bool exactRacialOwner,
            int heatCount, bool exactHeatOwnerAndWeapons, int grabCount,
            bool exactGrabOwnerAndTail, int legacyAttackTriggers, int legacyDamageActions)
        {
            return !spearNatural && tailCount == 1 && tailNatural && tailDice == 2 &&
                tailSides == 6 && exactRacialOwner && heatCount == 1 &&
                exactHeatOwnerAndWeapons && grabCount == 1 && exactGrabOwnerAndTail &&
                legacyAttackTriggers == 0 && legacyDamageActions == 0;
        }
    }
}

