using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace KingmakerGunslinger.ElementalRaces
{
    /// <summary>Semantic ability identities, not the overbroad native Ray weapon
    /// category or shared projectile art. Native and optional identities are
    /// curated from exact live ability descriptions and delivery contracts.
    /// Optional catalogs create no assembly dependency or owned blueprint.
    /// Unknown effects fail closed.</summary>
    internal static class ElementalCrystallineFormPolicy
    {
        internal const string Description = "Gain a +2 racial bonus to AC against rays. Once per ordinary rest, " +
            "you may deflect one ray that would hit you. Enable Deflect Next Ray to opt in; enabling or canceling " +
            "costs no action and spends no use. Deflection requires awareness, not being flat-footed, and one free " +
            "hand, as Deflect Arrows does. It spends the use only on a successful deflection and then switches off. " +
            "This does not deflect other ranged touch attacks, projectiles, lines, cones, or areas.";

        private static readonly ReadOnlyCollection<string> NativeRays = Array.AsReadOnly(new[] {
            "17696c144a0194c478cbe402b496cb23", // Polar Ray
            "1b4989258e5964149a909e47c72b7f67", // Fire elemental bloodline ray
            "1b95baefa8931574aa15a579e4423063", // Scaled Fist Scorching Ray
            "253673e368edc8949831c589f840964b", // Aasimar Searing Light
            "33e8997912cf76b4c99dca0445082804", // Ghaele light ray
            "37302f72b06ced1408bf5bb965766d46", // Energy Drain
            "435222be97067a447b2b40d3c58a058e", // Acid Scorching Ray
            "450a8d492a3342742917c3a3b357f25e", // Ki Scorching Ray
            "450af0402422b0b4980d9c2175869612", // Ray of Enfeeblement
            "4729c2ac98d02004fb440d17f7786e28", // Air elemental bloodline ray
            "4aa7942c3e62a164387a73184bca3fc1", // Disintegrate
            "6303b404df12b0f4793fa0763b21dd2c", // Elemental Assessor
            "64aca51981fc11346a20b723d7667e47", // Heavenly Fire
            "652739779aa05504a9ad5db1db6d02ae", // Disrupt Undead
            "6b72206a99bf1fc4583d05f106cafe91", // Kalikke/Kanerah Hellfire Ray
            "700cfcbd0cb2975419bcab7dbb8c6210", // Hellfire Ray
            "7ef096fdc8394e149a9e8dced7576fee", // Cold Scorching Ray
            "8aa5b7f955053f246b1bf73b4a319630", // Jabberwock eye rays (Bestiary 2)
            "8c2a0033a591b9247b45af575f12af77", // Earth elemental bloodline ray
            "96ca3143601d6b242802655336620d91", // Electricity Scorching Ray
            "9af2ab69df6538f4793b2f9c3cc85603", // Ray of Frost
            "9b4d07751dd104243a94b495c571c9dd", // Illusion school Blinding Ray
            "9d5cb7c1b77455b4d84169ce081934c6", // Water elemental bloodline ray
            "bf0accce250381a44b857d4af6c8e10d", // Searing Light
            "cdb106d53c65bbc4086183d54c3b97c7", // Scorching Ray
            "d4c2ce6c90094fdfb0fd908312372d72", // Existing project Lantern Archon light ray
            "d66ad81d61ee32344bc66c78ac3d0e4c", // Native Become Dust / Disintegrate
            "d6e72a6f936f8954596451be15fd083a", // Sage Arcane Bolt (printed ray)
            "e50e2db3d78b7ff4aa5c9699ba26febe", // Shadow Elemental Assessor
            "e648e7e21975e1843b6b56c9cfea9d6f", // Staff Scorching Ray
            "f1e7c4904e7db2d4082c4335d777d48f", // Firebrand's granted ray
            "f34fb78eaaec141469079af124bcfa0f", // Enervation
            "f779120fd8e69ca48928e2457ef2a2a1", // Tiefling Scorching Ray
            "fa3078b9976a5b24caf92e20ee9c0f54", // Ray of Sickening
            // Native story rays have stripped or abbreviated descriptions.
            // Their fixed game identities and ray delivery were inspected;
            // these labels are not a runtime name-based eligibility rule.
            "38ad9816b38c4ec4e8a0aef23387be1d", // Artifact_StarGauntletRay / Laser Beam
            "6a36a87c3d0094c46a9bef26afc3cb50", // NyrissaRay / Ray of Annihilation
            "c2e5b967c47a81c4aa355ef213ec5634"  // RaySpellLanternKingStar / Immolation Curse
        });

        // Exact optional spell/revelation identities and copied spell variants.
        // These entries are inert when their owning mod is absent. Names below
        // are review labels only; production classification never reads them.
        private static readonly ReadOnlyCollection<string> OptionalRays = Array.AsReadOnly(new[] {
            "11d12024b52605e025f7965719f16789", // SpiritWhispererLoreManifestationWish3RayOfExhaustionAbility
            "1729c8f20b1e44668248f3cd5f09acce", // MythmakerHierophantSearingLight
            "287de992b45ad819a2f912d6ce6dcf3f", // SkaldSpellKenningClericClassHellfireRay
            "34c54ad193e44adca447de5e294ca4a1", // OccultistImplementAcidEnergyRayAbility
            "34eefa1002c840fda0ccad83c695d40b", // HatredEnervation
            "357fc9de741c44fbb43132c2efe421b0", // OccultistImplementElectricityEnergyRayAbility
            "3b72a12b67faffb9c27a19dd4ec07003", // SkaldSpellKenningClericClassElementalAssessor
            "434550d9ec98d00ebe8a97e46abc615e", // SpiritWhispererLoreManifestationWish6HellfireRay
            "436d9fb234f359a3d468c1f4e8c8039c", // SpiritWhispererLoreManifestationWish4Enervation
            "440ada0e881547809ee0f89145ecd99d", // RelicHunterImplementColdEnergyRayAbility
            "4895067bc3e446b5b0859cb4c4e2d481", // RayOfExhaustionAbility
            "4b2db51553124c2c959e26cc56cf72a9", // RelicHunterImplementAcidEnergyRayAbility
            "4b60c6ec26b2f97d1265b349fbc70957", // MajorMagicRayOfEnfeeblement
            "4c46d1fc8dfab9d0c071de534361a1a6", // ShadowEvocationGreaterHellfireRay
            "4c67af0762dfd980e420a43ed895975e", // SpiritWhispererLoreManifestationWish8PolarRay
            "4f4f4f47cca1017428d59af853504c4e", // ShamanLoreManifestationWish3RayOfExhaustionAbility
            "504a18603f38f7aede099cefea11de62", // SpiritWhispererLoreManifestationWish6ElementalAssessor
            "57738ba61f72455fa0095c12593d2c41", // CharismaHatredEnervation
            "578d7cbe57caf1d704a7b84ad9167f0f", // SpiritWhispererLoreManifestationWish1RayOfEnfeeblement
            "5a4908087a9b413b8a176409887e77cf", // SchoolUnderstandingIllusionSchoolBaseAbility
            "622233466655f57029c01f1ace22cf1d", // ShamanLoreManifestationWish1RayOfEnfeeblement
            "69768f016358051404509bcee68a0411", // SpiritWhispererLoreManifestationWish5WrackingRayAbility
            "6b4d0fdb98301ccd23c572d9552c9deb", // MinorMagicDisruptUndead
            "79ee3848de48e63e9f4c4f819afa3c8f", // SpiritWhispererLoreManifestationWish6Disintegrate
            "89b6925b7fe203a9294aa679fe3d4a42", // SkaldSpellKenningWizardClassRayOfExhaustionAbility
            "94989dc5ddf5713df35300f4b289c946", // MinorMagicRayOfFrost
            "95dc5c79b211fb5bba46e9ab923d6eb6", // SkaldSpellKenningWizardClassScorchingRay
            "96e51497968940cdb8a00f465536d581", // OccultistImplementLifeDrainAbility
            "98e7c422410b4b788c134f501990c880", // DisruptUndeadNotCantrip
            "9f04cb00275cf75186003e5f0c9fbe50", // ShamanLoreManifestationWish2ScorchingRay
            "9f9849bd3a565c52dc5ce1a2fd766bdc", // SkaldSpellKenningWizardClassEnervation
            "a19be489cde4427f95a55e7794ba2b03", // RelicHunterImplementFireEnergyRayAbility
            "a7699106fb3c479c800981a90a685f72", // OccultistImplementFireEnergyRayAbility
            "ac19a12b87bf4c1f8a9963c594262bfc", // WrackingRayAbility
            "aec921c7207c4151936ed9086ebc9c30", // RelicHunterImplementElectricityEnergyRayAbility
            "b8d085f22a46c665d3d57b06617a79c2", // ShamanLoreManifestationWish3SearingLight
            "c0f9a08571abf0d58044ca196a432a8b", // SpiritWhispererLoreManifestationWish2ScorchingRay
            "c44085b60ba40bc3292b7ce8a2f35a62", // ShamanLoreManifestationWish5WrackingRayAbility
            "cb3c6a5a3a74dc6cc3b6ec907cbce0f4", // ShamanLoreManifestationWish8PolarRay
            "ce36668ba123dafd9351b2db80fda4b1", // ShamanLoreManifestationWish6HellfireRay
            "cffdea6966ffe6eca698ca2b18b84da3", // SkaldSpellKenningWizardClassDisintegrate
            "d19be6fb8eb5cf88f2348a96334a0e58", // SkaldSpellKenningClericClassSearingLight
            "d97f137067a3422298ae296863aded5d", // SoulSiphonOrcalRevelationAbility
            "dd392e327283fd5df3d2b9d000501b8d", // ShamanLoreManifestationWish6ElementalAssessor
            "dedf148c761c4d3db87a53e57e198d40", // OccultistImplementColdEnergyRayAbility
            "e41e3122f79f53d3ea5d72c0addc288f", // ShamanLoreManifestationWish4Enervation
            "e64eea9153c1c2f1def777a92bdb5205", // SpiritWhispererLoreManifestationWish3SearingLight
            "e659ca41878ff77ce7dd19456853af4e", // SkaldSpellKenningWizardClassElementalAssessor
            "ea1398c383b248dea2f934d773d559bf", // InfernalArcanaScorchingRay
            "ec43aa14789b062216e0db85a2a04663", // SkaldSpellKenningWizardClassWrackingRayAbility
            "eec521e5b22d78f0135cd0fed10197d8", // ShadowEvocationScorchingRay
            "f1fb2b94bd2d9540d1acf6fbb4d65471", // ShadowEvocationGreaterScorchingRay
            "f49d0e1a93f3eccdb2976abe70bbf960", // ShamanLoreManifestationWish6Disintegrate
            "f504218d854cf5713dc81f0a1858fc42", // SkaldSpellKenningWizardClassRayOfEnfeeblement
            "f55682f8542fd0dc875e124ee8fe1072", // SkaldSpellKenningWizardClassHellfireRay
            "fe26b820b31c4a81b066fc8479a34c1a" // RayOfFrostNotCantrip
        });
        private static readonly ReadOnlyCollection<string> AllRays = Array.AsReadOnly(
            NativeRays.Concat(OptionalRays).ToArray());

        internal static IReadOnlyList<string> NativeRayAbilityGuids { get { return NativeRays; } }
        internal static IReadOnlyList<string> OptionalRayAbilityGuids { get { return OptionalRays; } }
        internal static IReadOnlyList<string> RayAbilityGuids { get { return AllRays; } }

        internal static bool IsRay(IEnumerable<string> effectiveAbilityAndParents,
            bool simpleProjectile, bool needsAttackRoll, bool rayWeapon,
            bool handOfApprentice)
        {
            return simpleProjectile && needsAttackRoll && rayWeapon && !handOfApprentice &&
                effectiveAbilityAndParents != null && effectiveAbilityAndParents.Any(
                    value => value != null && AllRays.Contains(value));
        }

        internal static bool CanDeflect(bool exactTarget, bool ray, bool hit,
            bool optedIn, int uses, bool conscious, bool aware, bool freeHand)
        {
            return exactTarget && ray && hit && optedIn && uses > 0 && conscious && aware && freeHand;
        }
    }
}
