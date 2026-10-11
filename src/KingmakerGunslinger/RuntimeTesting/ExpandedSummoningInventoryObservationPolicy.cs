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
            // Sprints 18 through 21. This table stopped at Sprint 17
            // because the only thing that reads it is the inventory observer,
            // and the only thing that runs the inventory observer is the
            // compatibility matrix - a tranche-closure gate that had not run
            // since v0.0.141. The series closure is the first thing to look,
            // and it found twenty-four project facts granted directly on
            // project units with no row here. Every one is this mod's own
            // blueprint on this mod's own unit, which is what this table
            // records; what it exists to catch is a donor or native reference
            // leaking onto a unit, and none of these is one.
            //
            // The shared Primate full-Strength limb carrier appears five
            // times, on both apes, the Girallon, the Giant Scorpion and the
            // Giant Crab. That is reuse of a released seam rather than five
            // copies of it, and a row per unit is how this table says so.
            new[] { "c53c3e23097e4f25a411c50ce2868c60", "KMG_Summoning_Unit_Ape", "98d94d61f4d440e18a12493756e7e99d", "KMG_Summoning_Natural_Primate_FullStrengthLimbs", "AddFacts" },
            new[] { "5482b49785a3492aa7d29a4ee575cd66", "KMG_Summoning_Unit_DireApe", "904a13db0fc440c9b7a2490e821dcd9e", "KMG_Summoning_Special_DireApe_Rend", "AddFacts" },
            new[] { "5482b49785a3492aa7d29a4ee575cd66", "KMG_Summoning_Unit_DireApe", "98d94d61f4d440e18a12493756e7e99d", "KMG_Summoning_Natural_Primate_FullStrengthLimbs", "AddFacts" },
            new[] { "b9187f2a07a74c84b07d5a34280753a6", "KMG_Summoning_Unit_Girallon", "6fb87c0c6fc94981a3ce9d55f135af22", "KMG_Summoning_Special_Girallon_Rend", "AddFacts" },
            new[] { "b9187f2a07a74c84b07d5a34280753a6", "KMG_Summoning_Unit_Girallon", "98d94d61f4d440e18a12493756e7e99d", "KMG_Summoning_Natural_Primate_FullStrengthLimbs", "AddFacts" },
            new[] { "440bd760511f4bdd9515ff59a0c5eb33", "KMG_Summoning_Unit_Xill", "15ccb5e823fc44bcbbf0b865ca017cce", "KMG_Summoning_Special_Xill_FullStrengthLimbs", "AddFacts" },
            new[] { "440bd760511f4bdd9515ff59a0c5eb33", "KMG_Summoning_Unit_Xill", "26a15f2d35e349979e091eefcf87252b", "KMG_Summoning_Special_Xill_CombatTraits", "AddFacts" },
            new[] { "4b5ec54e8e7a4e57b838541006affd3d", "KMG_Summoning_Unit_GiantScorpion", "fb2d0445064c401a96253bab6a4e2024", "KMG_Summoning_Natural_GiantScorpion_TripDefense", "AddFacts" },
            new[] { "4b5ec54e8e7a4e57b838541006affd3d", "KMG_Summoning_Unit_GiantScorpion", "94ef1868a3944f7785ec867404c5e679", "KMG_Summoning_Natural_GiantScorpion_RacialSkills", "AddFacts" },
            new[] { "4b5ec54e8e7a4e57b838541006affd3d", "KMG_Summoning_Unit_GiantScorpion", "334504cf63f0405cbe8098dfef97b1fe", "KMG_Summoning_Natural_GiantScorpion_Poison", "AddFacts" },
            new[] { "4b5ec54e8e7a4e57b838541006affd3d", "KMG_Summoning_Unit_GiantScorpion", "7f16ea2ce3004d50b58cea54794a45d7", "KMG_Summoning_Natural_GiantScorpion_MindlessImmunity", "AddFacts" },
            new[] { "4b5ec54e8e7a4e57b838541006affd3d", "KMG_Summoning_Unit_GiantScorpion", "98d94d61f4d440e18a12493756e7e99d", "KMG_Summoning_Natural_Primate_FullStrengthLimbs", "AddFacts" },
            new[] { "4b5ec54e8e7a4e57b838541006affd3d", "KMG_Summoning_Unit_GiantScorpion", "a0d7ba193be14f82a24f138c024b3ef0", "KMG_Summoning_Special_GiantScorpion_Traits", "AddFacts" },
            new[] { "5965d242ffe1409d98e38f2793543aad", "KMG_Summoning_Unit_GiantCrab", "8072e28907b043d788b4beaa6a0e260f", "KMG_Summoning_Natural_GiantCrab_TripDefense", "AddFacts" },
            new[] { "5965d242ffe1409d98e38f2793543aad", "KMG_Summoning_Unit_GiantCrab", "13d5e8f00bab4725a6b3d4c55347c7ff", "KMG_Summoning_Natural_GiantCrab_RacialSkills", "AddFacts" },
            new[] { "5965d242ffe1409d98e38f2793543aad", "KMG_Summoning_Unit_GiantCrab", "7a10f0f9fd484d70a1261ca5840973d2", "KMG_Summoning_Natural_GiantCrab_MindlessImmunity", "AddFacts" },
            new[] { "5965d242ffe1409d98e38f2793543aad", "KMG_Summoning_Unit_GiantCrab", "98d94d61f4d440e18a12493756e7e99d", "KMG_Summoning_Natural_Primate_FullStrengthLimbs", "AddFacts" },
            new[] { "5965d242ffe1409d98e38f2793543aad", "KMG_Summoning_Unit_GiantCrab", "62fc6e43ea1942f1b86c043c8c040d41", "KMG_Summoning_Special_GiantCrab_Traits", "AddFacts" },
            new[] { "9d7d36e71ea141258509b8a32557577e", "KMG_Summoning_Unit_Bebelith", "3e50280cbbda42ec8e0699c2db2c9457", "KMG_Summoning_Special_Bebelith_Rot", "AddFacts" },
            new[] { "9d7d36e71ea141258509b8a32557577e", "KMG_Summoning_Unit_Bebelith", "1d07e89fc90746dd9e71606f32f2d03d", "KMG_Summoning_Special_Bebelith_PenetratingStrike", "AddFacts" },
            new[] { "9d7d36e71ea141258509b8a32557577e", "KMG_Summoning_Unit_Bebelith", "a4f0548fb8274794a132cc8c39dca8e4", "KMG_Summoning_Special_Bebelith_TripDefense", "AddFacts" },
            new[] { "9d7d36e71ea141258509b8a32557577e", "KMG_Summoning_Unit_Bebelith", "adf498a88b3c4fd1a5c40f9d2c537f0f", "KMG_Summoning_Special_Bebelith_RacialSkills", "AddFacts" },
            new[] { "9d7d36e71ea141258509b8a32557577e", "KMG_Summoning_Unit_Bebelith", "d749d5a287dacef12983e7f3ea9205fd", "KMG_Summoning_Special_Bebelith_ChassisCorrection", "AddFacts" },
            new[] { "9d7d36e71ea141258509b8a32557577e", "KMG_Summoning_Unit_Bebelith", "9206c7293c114d92b2ec731c906c02df", "KMG_Summoning_Special_Bebelith_Web", "AddFacts" },
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

