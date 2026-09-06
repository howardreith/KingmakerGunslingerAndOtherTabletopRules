using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace KingmakerGunslinger.ElementalRaces
{
    /// <summary>Semantic ability identities, not the overbroad native Ray weapon
    /// category or shared projectile art. IDs come from the KMG-only native
    /// audit; no optional-mod identity is imported. Unknown effects fail closed.</summary>
    internal static class ElementalCrystallineFormPolicy
    {
        internal const string Description = "Gain a +2 racial bonus to AC against rays. Once per ordinary rest, " +
            "you may deflect one ray that would hit you. Enable Deflect Next Ray to opt in; enabling or canceling " +
            "costs no action and spends no use. Deflection requires awareness, not being flat-footed, and one free " +
            "hand, as Deflect Arrows does. It spends the use only on a successful deflection and then switches off. " +
            "This does not deflect other ranged touch attacks, projectiles, lines, cones, or areas.";

        private static readonly ReadOnlyCollection<string> Rays = Array.AsReadOnly(new[] {
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
            "fa3078b9976a5b24caf92e20ee9c0f54"  // Ray of Sickening
        });

        internal static IReadOnlyList<string> RayAbilityGuids { get { return Rays; } }

        internal static bool IsRay(IEnumerable<string> effectiveAbilityAndParents,
            bool simpleProjectile, bool needsAttackRoll, bool rayWeapon,
            bool handOfApprentice)
        {
            return simpleProjectile && needsAttackRoll && rayWeapon && !handOfApprentice &&
                effectiveAbilityAndParents != null && effectiveAbilityAndParents.Any(
                    value => value != null && Rays.Contains(value));
        }

        internal static bool CanDeflect(bool exactTarget, bool ray, bool hit,
            bool optedIn, int uses, bool conscious, bool aware, bool freeHand)
        {
            return exactTarget && ray && hit && optedIn && uses > 0 && conscious && aware && freeHand;
        }
    }
}
