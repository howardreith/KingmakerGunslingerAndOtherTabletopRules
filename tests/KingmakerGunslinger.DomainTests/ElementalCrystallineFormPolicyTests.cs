using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.ElementalRaces;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ElementalCrystallineFormPolicyTests
    {
        internal static void ExactSemanticRayBoundaries()
        {
            IReadOnlyList<string> rays = ElementalCrystallineFormPolicy.RayAbilityGuids;
            Assertions.Equal(37, ElementalCrystallineFormPolicy.NativeRayAbilityGuids.Count, "The original 34 rays plus three exact native story rays must remain explicit.");
            Assertions.Equal(56, ElementalCrystallineFormPolicy.OptionalRayAbilityGuids.Count, "Every reviewed optional ray and copied spell needs its exact identity.");
            Assertions.Equal(93, rays.Count, "The combined semantic ray inventory must be explicit.");
            Assertions.True(rays.SequenceEqual(ElementalCrystallineFormPolicy.NativeRayAbilityGuids
                .Concat(ElementalCrystallineFormPolicy.OptionalRayAbilityGuids)), "Native ordering must be preserved when adding optional identities.");
            Assertions.True(((IList<string>)ElementalCrystallineFormPolicy.NativeRayAbilityGuids).IsReadOnly &&
                ((IList<string>)ElementalCrystallineFormPolicy.OptionalRayAbilityGuids).IsReadOnly, "Callers cannot mutate either provenance catalog.");
            Assertions.Equal(rays.Count, rays.Distinct(StringComparer.Ordinal).Count(), "No duplicate ray identities.");
            Assertions.True(((IList<string>)rays).IsReadOnly, "Callers cannot extend the shared ray catalog.");
            foreach (string ray in rays)
            {
                Assertions.True(ElementalCrystallineFormPolicy.IsRay(new[] { ray }, true, true, true, false), "Exact ray accepted.");
                Assertions.True(ElementalCrystallineFormPolicy.IsRay(new[] { "unknown-variant", ray }, true, true, true, false), "Ray parent is recognized once.");
                for (int mask = 0; mask < 16; mask++)
                    Assertions.Equal(mask == 7, ElementalCrystallineFormPolicy.IsRay(new[] { ray, ray },
                        (mask & 1) != 0, (mask & 2) != 0, (mask & 4) != 0, (mask & 8) != 0),
                        "Catalog identity does not broaden delivery geometry or weapon attacks.");
            }
            string[] excluded = {
                "0c852a2405dd9f14a8bbcfaf245ff823", // Acid Splash
                "9f10909f0be1f5141bf1c102041f93d9", // Snowball
                "0a2f7c6aa81bc6548ac7780d8b70bcbc", // Battering Blast
                "9a46dfd390f943647ab4395fc997936d", // Acid Arrow shares Scorching Ray acid art
                "5e1db2ef80ff361448549beeb7785791", // Icicle shares Ray of Frost art
                "4ac47ddb9fa1eaf43a1b6809980cfbd2", // Magic Missile
                "9779c8578acd919419f563c33d7b2af5", // Spit Venom
                "31acd268039966940872c916782ae018", // Moonfire is described as a blast, not a ray
                "4ba9818b263827049adc102196c65522", // Opaque Prismatic Surge
                "RayOfExhaustionAbility", "ScorchingRay", "unknown", "", null
            };
            foreach (string nonray in excluded)
                Assertions.True(!ElementalCrystallineFormPolicy.IsRay(new[] { nonray }, true, true, true, false),
                    "Ray-category weapons and shared projectiles do not make an effect a ray.");
            Assertions.True(!ElementalCrystallineFormPolicy.IsRay(null, true, true, true, false), "Missing source fails closed.");
        }

        internal static void DeflectionRequiresEveryNativeBoundary()
        {
            for (int mask = 0; mask < 128; mask++)
                for (int uses = -1; uses <= 2; uses++)
                    Assertions.Equal(mask == 127 && uses > 0,
                        ElementalCrystallineFormPolicy.CanDeflect((mask & 1) != 0, (mask & 2) != 0,
                            (mask & 4) != 0, (mask & 8) != 0, uses, (mask & 16) != 0,
                            (mask & 32) != 0, (mask & 64) != 0),
                        "Only an opted-in aware free-handed living exact target can spend on a hit.");
        }
    }
}
