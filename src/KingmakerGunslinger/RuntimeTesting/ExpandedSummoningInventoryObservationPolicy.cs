using System;
using System.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Evidence interpretation only. Never grants a fact or changes sanitation.
    internal static class ExpandedSummoningInventoryObservationPolicy
    {
        // Read only the two profile facts the public census previously lacked.
        // These exact identities already exist on the qualified Elephant; this
        // does not grant facts, accept a name prefix or exempt unknown facts.
        internal static string GiantProfileFactGuid(string unitGuid, string fact)
        {
            if (unitGuid != "9dd8544097234f05bcd35d400d91b510") return null;
            if (fact == "Endurance") return "54ee847996c25cd4ba8773d7b8555174";
            if (fact == "ElephantChassis") return "bd380912f7c74843966c363ab281c0c2";
            return null;
        }

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
            // Sprint 22. The inventory gate did not run during the
            // Ankylosaurus sprint, so its two exact project-owned AddFacts
            // reached the next public census without ownership rows.
            new[] { "10c80d5cfa594332bd3e5127799e426f", "KMG_Summoning_Unit_Ankylosaurus", "e5f977fc42c943a88902b9c3a170a7ea", "KMG_Summoning_Natural_Ankylosaurus_Chassis", "AddFacts" },
            new[] { "10c80d5cfa594332bd3e5127799e426f", "KMG_Summoning_Unit_Ankylosaurus", "e8a085864606409ba25dc123f14d27a2", "KMG_Summoning_Special_Ankylosaurus_Stun", "AddFacts" },
            // Sprint 23. These are all five project-owned facts referenced
            // directly from the two units' AddFacts arrays. The public native
            // census measured charge and trample there as well as the chassis
            // and trip carriers; every row remains exact owner/fact/carrier.
            new[] { "ce540cd4b3b74861a4a91458d742a692", "KMG_Summoning_Unit_Triceratops", "5e08305bfe3b4ab2bd65c74dd472c65c", "KMG_Summoning_Natural_Triceratops_Chassis", "AddFacts" },
            new[] { "ce540cd4b3b74861a4a91458d742a692", "KMG_Summoning_Unit_Triceratops", "7168413e07084ac086f3eb409b1199fc", "KMG_Summoning_Special_Triceratops_PowerfulCharge", "AddFacts" },
            new[] { "ce540cd4b3b74861a4a91458d742a692", "KMG_Summoning_Unit_Triceratops", "20c83d35a8234c9fb35e8f174d19e6ba", "KMG_Summoning_Special_Triceratops_Trample", "AddFacts" },
            new[] { "39cab5be4dc148339f4ce8b0157bd716", "KMG_Summoning_Unit_Stegosaurus", "95278500c12647adbf53f8b77b7d3955", "KMG_Summoning_Natural_Stegosaurus_Chassis", "AddFacts" },
            new[] { "39cab5be4dc148339f4ce8b0157bd716", "KMG_Summoning_Unit_Stegosaurus", "ce0af5b47a1a462095e9f308a5d1bba1", "KMG_Summoning_Special_Stegosaurus_Trip", "AddFacts" },
            // Sprint 24, published in v0.0.155. Same pattern as the rows
            // above: this mod's own registered blueprints granted on this
            // mod's own units. Every GUID below is in the pinned Sprint 24
            // identity set, so none is a donor or native reference leaking
            // onto a unit, which is what this table exists to catch.
            new[] { "10a13f87e3664e30b642a3c59cc8c700", "KMG_Summoning_Unit_Deinonychus", "fed90fe9166348a8b7ae8bff5f03566a", "KMG_Summoning_Natural_Deinonychus_Chassis", "AddFacts" },
            new[] { "04b1a75a76544a9581ee832045f8227a", "KMG_Summoning_Unit_Tyrannosaurus", "7c6bb677353045189045129c097e4c2f", "KMG_Summoning_Natural_Tyrannosaurus_Chassis", "AddFacts" },
            new[] { "04b1a75a76544a9581ee832045f8227a", "KMG_Summoning_Unit_Tyrannosaurus", "c1a9b9554bb744a9a22f7ee1557dd678", "KMG_Summoning_Special_Tyrannosaurus_PowerfulBite", "AddFacts" },
            new[] { "04b1a75a76544a9581ee832045f8227a", "KMG_Summoning_Unit_Tyrannosaurus", "e45b109a0e96496d9efd42edd11f95c8", "KMG_Summoning_Special_Tyrannosaurus_BleedingCritical", "AddFacts" },
            new[] { "04b1a75a76544a9581ee832045f8227a", "KMG_Summoning_Unit_Tyrannosaurus", "e22d935930eb4a9398e1160699f87b09", "KMG_Summoning_Special_Tyrannosaurus_CombatTraits", "AddFacts" },
            // Sprint 25: exact project facts on their own registered owners.
            new[] { "9093aef39ecc416092ef68655a7e56fb", "KMG_Summoning_Unit_Brachiosaurus", "63280e8f08c94c3a9b67787ec2d7ab71", "KMG_Summoning_Natural_Brachiosaurus_Chassis", "AddFacts" },
            new[] { "9093aef39ecc416092ef68655a7e56fb", "KMG_Summoning_Unit_Brachiosaurus", "309ec3dedcf04c15b3011933f5eafef4", "KMG_Summoning_Special_Brachiosaurus_Trample", "AddFacts" },
            new[] { "9093aef39ecc416092ef68655a7e56fb", "KMG_Summoning_Unit_Brachiosaurus", "b41f7d2a6c8e40539af1c2d5e7b30486", "KMG_Summoning_Special_Brachiosaurus_Overrun", "AddFacts" },
            new[] { "9093aef39ecc416092ef68655a7e56fb", "KMG_Summoning_Unit_Brachiosaurus", "c9f41b8d20e7456eae3d6c1f5a8b7204", "KMG_Summoning_Special_Brachiosaurus_GreaterOverrun", "AddFacts" },
            new[] { "9dd8544097234f05bcd35d400d91b510", "KMG_Summoning_Unit_Elephant", "a7c34e1d9b5f4a02ae6d8c71f3b02594", "KMG_Summoning_Special_Elephant_Trample", "AddFacts" },
            new[] { "9dd8544097234f05bcd35d400d91b510", "KMG_Summoning_Unit_Elephant", "bd380912f7c74843966c363ab281c0c2", "KMG_Summoning_Natural_Elephant_Chassis", "AddFacts" },
            // Sprints 26-29: exact already-released project owner/fact pairs.
            // Measured by the retained 20261021T223500 inventory FAIL; these
            // rows repair interpretation only and grant no gameplay fact.
            new[] { "ce2fc58942b54e558208ea67d595cb7b", "KMG_Summoning_Unit_Gorgon", "3d4461a55b224472ad2853699c596b4f", "KMG_Summoning_Natural_Gorgon_Chassis", "AddFacts" },
            new[] { "ce2fc58942b54e558208ea67d595cb7b", "KMG_Summoning_Unit_Gorgon", "6e0309f69ef04b13b31147bd402dccd1", "KMG_Summoning_Special_Gorgon_Breath", "AddFacts" },
            new[] { "ce2fc58942b54e558208ea67d595cb7b", "KMG_Summoning_Unit_Gorgon", "c55343b590f54f0ea18fd258c92dfcd5", "KMG_Summoning_Special_Gorgon_Trample", "AddFacts" },
            new[] { "f84a0babcc864630bae225b2019d6bba", "KMG_Summoning_Unit_Bulette", "dee2bf278a5c4f54bb274d616584e5fb", "KMG_Summoning_Natural_Bulette_Chassis", "AddFacts" },
            new[] { "f84a0babcc864630bae225b2019d6bba", "KMG_Summoning_Unit_Bulette", "3aff604814c3419184037ef39511dbf4", "KMG_Summoning_Special_Bulette_Leap", "AddFacts" },
            new[] { "4b55d1f8da5e4a3ea23f9a95191f8737", "KMG_Summoning_Unit_Griffon", "90b8b75d5117401e978a2f94a4f11546", "KMG_Summoning_Natural_Griffon_Chassis", "AddFacts" },
            new[] { "4b55d1f8da5e4a3ea23f9a95191f8737", "KMG_Summoning_Unit_Griffon", "689e319d9fcd4198a1edd44a9224c143", "KMG_Summoning_Special_Griffon_CombatTraits", "AddFacts" },
            new[] { "58770207fd8c4084a0d9addbac591616", "KMG_Summoning_Unit_HillGiant", "8c9560a29844441ea929f6da1384b60a", "KMG_Summoning_Giant_HillTraits", "AddFacts" },
            new[] { "58770207fd8c4084a0d9addbac591616", "KMG_Summoning_Unit_HillGiant", "9b979adb2ff640a79ebc4dda5cbced22", "KMG_Summoning_Giant_CleaveFeature", "AddFacts" },
            new[] { "956501547dd54479b329a315aab07a05", "KMG_Summoning_Unit_StoneGiant", "34cdbc0d44c441219d99cd332dde9f29", "KMG_Summoning_Giant_StoneNaturalArmor11", "AddFacts" },
            new[] { "956501547dd54479b329a315aab07a05", "KMG_Summoning_Unit_StoneGiant", "206644422e4a4f0194dcb4bea7d150f2", "KMG_Summoning_Giant_StoneTraits", "AddFacts" },
            new[] { "65481592185641c99c502e21ba9a5fa2", "KMG_Summoning_Unit_FireGiant", "724e7e13cce64adba5ac6a9e7b4635a5", "KMG_Summoning_Giant_FireTraits", "AddFacts" },
            new[] { "65481592185641c99c502e21ba9a5fa2", "KMG_Summoning_Unit_FireGiant", "9b979adb2ff640a79ebc4dda5cbced22", "KMG_Summoning_Giant_CleaveFeature", "AddFacts" },
            new[] { "65481592185641c99c502e21ba9a5fa2", "KMG_Summoning_Unit_FireGiant", "b45179f0d157465f899c0b9d096b67a6", "KMG_Summoning_Giant_FireImprovedOverrun", "AddFacts" },
            new[] { "2481bc73abd64ef0ae581b48438b5404", "KMG_Summoning_Unit_Ettin", "c63cf06db0d84e1b8d25a39113feb357", "KMG_Summoning_Giant_Sprint29_EttinNaturalArmor8", "AddFacts" },
            new[] { "2481bc73abd64ef0ae581b48438b5404", "KMG_Summoning_Unit_Ettin", "c6bcf99daa9746b28fd62ec6bac9dffa", "KMG_Summoning_Giant_Sprint29_EttinTraits", "AddFacts" },
            new[] { "2481bc73abd64ef0ae581b48438b5404", "KMG_Summoning_Unit_Ettin", "9b979adb2ff640a79ebc4dda5cbced22", "KMG_Summoning_Giant_CleaveFeature", "AddFacts" },
            new[] { "2481bc73abd64ef0ae581b48438b5404", "KMG_Summoning_Unit_Ettin", "b45179f0d157465f899c0b9d096b67a6", "KMG_Summoning_Giant_FireImprovedOverrun", "AddFacts" },
            new[] { "ff4696c6336141a3b2236cb0a22c86f3", "KMG_Summoning_Unit_CloudGiant", "6a3e878e9d7343879448943f7b6fffa0", "KMG_Summoning_Giant_Sprint29_CloudNaturalArmor12", "AddFacts" },
            new[] { "ff4696c6336141a3b2236cb0a22c86f3", "KMG_Summoning_Unit_CloudGiant", "d3d4da54b1c743a687756a8ba7a374a8", "KMG_Summoning_Giant_Sprint29_CloudTraits", "AddFacts" },
            new[] { "ff4696c6336141a3b2236cb0a22c86f3", "KMG_Summoning_Unit_CloudGiant", "9b979adb2ff640a79ebc4dda5cbced22", "KMG_Summoning_Giant_CleaveFeature", "AddFacts" },
            new[] { "ff4696c6336141a3b2236cb0a22c86f3", "KMG_Summoning_Unit_CloudGiant", "b45179f0d157465f899c0b9d096b67a6", "KMG_Summoning_Giant_FireImprovedOverrun", "AddFacts" },
            new[] { "ff4696c6336141a3b2236cb0a22c86f3", "KMG_Summoning_Unit_CloudGiant", "e2caf080405e45628d796a6bb94a444e", "KMG_Summoning_Giant_Sprint29_AwesomeBlowFeature", "AddFacts" },
            new[] { "ff4696c6336141a3b2236cb0a22c86f3", "KMG_Summoning_Unit_CloudGiant", "9ea1c943e1ff46e4a458cb5c637540db", "KMG_Summoning_Giant_Sprint29_CloudObscuringMist", "AddFacts" },
            new[] { "ff4696c6336141a3b2236cb0a22c86f3", "KMG_Summoning_Unit_CloudGiant", "aa562c8636c5472db98ac55fb59103a8", "KMG_Summoning_Giant_Sprint29_CloudFogCloud", "AddFacts" },
            new[] { "4b1143140c614a0aaaa1118c8a44c9ae", "KMG_Summoning_Unit_StormGiant", "6244a2e6ba88439e8ab1cd884b6eb303", "KMG_Summoning_Giant_Sprint29_StormNaturalArmor12", "AddFacts" },
            new[] { "4b1143140c614a0aaaa1118c8a44c9ae", "KMG_Summoning_Unit_StormGiant", "d9ef1385b3fb4adc91b558bcff85c94a", "KMG_Summoning_Giant_Sprint29_StormTraits", "AddFacts" },
            new[] { "4b1143140c614a0aaaa1118c8a44c9ae", "KMG_Summoning_Unit_StormGiant", "9b979adb2ff640a79ebc4dda5cbced22", "KMG_Summoning_Giant_CleaveFeature", "AddFacts" },
            new[] { "4b1143140c614a0aaaa1118c8a44c9ae", "KMG_Summoning_Unit_StormGiant", "65f622a9f1e34109a2a26a5f15bf6c66", "KMG_Summoning_Giant_Sprint29_StormImprovedVitalStrike", "AddFacts" },
            new[] { "4b1143140c614a0aaaa1118c8a44c9ae", "KMG_Summoning_Unit_StormGiant", "e2caf080405e45628d796a6bb94a444e", "KMG_Summoning_Giant_Sprint29_AwesomeBlowFeature", "AddFacts" },
            new[] { "4b1143140c614a0aaaa1118c8a44c9ae", "KMG_Summoning_Unit_StormGiant", "10475a340d3245daad7033f3c402f99e", "KMG_Summoning_Giant_Sprint29_StormCallLightning", "AddFacts" },
            new[] { "4b1143140c614a0aaaa1118c8a44c9ae", "KMG_Summoning_Unit_StormGiant", "c8094ceebd49415386f9a08067b79d90", "KMG_Summoning_Giant_Sprint29_StormChainLightning", "AddFacts" },
        };

        internal static string[][] QualifiedReferenceRows
        { get { return References.Select(r => (string[])r.Clone()).ToArray(); } }

        internal static bool QualifiedProjectReference(string unitGuid, string unitName,
            string referenceGuid, string referenceName, string carrier)
        {
            return References.Any(r => r[0] == unitGuid && r[1] == unitName &&
                r[2] == referenceGuid && r[3] == referenceName && r[4] == carrier);
        }

        internal static bool NaturalProfileFactComparable(string referenceName,
            bool expectedProfileFact)
        {
            // Special builders prove supplemental facts elsewhere, but a
            // Special_* fact explicitly named by the frozen natural profile
            // remains part of chassis equality. A prefix cannot discard it.
            return expectedProfileFact ||
                (!string.IsNullOrEmpty(referenceName) &&
                 !referenceName.StartsWith("KMG_Summoning_Special_",
                    StringComparison.Ordinal));
        }

        internal static bool BebelithPrimaryBite(string guid, int dice, int sides)
        {
            return guid == "d2f99947db522e24293a7ec4eded453f" &&
                dice == 2 && sides == 6;
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
