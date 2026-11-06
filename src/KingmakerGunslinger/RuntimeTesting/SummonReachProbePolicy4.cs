namespace KingmakerGunslinger.RuntimeTesting
{
    // Sprint 4 representative summons. The Ice Devil is deliberately absent:
    // its native phases are restricted to the designated host.
    internal static class SummonReachProbePolicy4
    {
        // Native threat probes: every wielded weapon at the adjacent
        // distance, just inside and just beyond its own range.
        internal static readonly string[] ProbeKeys = {
            // Small/Medium natural attacks and ordinary weapons.
            "lemure", "dretch", "hound-archon", "astral-deva", "barbed-devil", "babau",
            // Medium per-attack extensions.
            "salamander", "kyton-chain-devil",
            // Larger natural reach and tails.
            "hezrou", "vrock", "lillend-azata", "ankylosaurus", "brachiosaurus", "glabrezu",
            // Ordinary manufactured weapons on larger creatures.
            "hill-giant", "fire-giant", "ettin", "storm-giant",
            // Existing exact-item range corrections left unchanged.
            "griffon", "elephant",
            // Controls: the accepted Bone Devil, the genuine reach glaive and
            // ordinary native-weapon summons.
            "bone-devil", "bearded-devil", "wolf", "dire-bear"
        };

        // Native full attacks on the adjacent durable hostile. None of these
        // has a fear aura.
        internal static readonly string[] RoutineKeys = {
            "lemure",        // Medium natural (two claws)
            "hezrou",        // Large natural (bite, two claws)
            "hill-giant",    // Large ordinary manufactured weapon (greatclub)
            "salamander",    // mixed manufactured/natural, unequal reach (spear, tail)
            "storm-giant",   // Huge manufactured weapon on the existing absolute range-override path
            "cloud-giant"    // the same override path; s4d/s4e Storm Giant first slot is under inspection
        };

        // Genuine reach weapons: the only melee weapons that must remain
        // ProperFlanking2 reach weapons.
        internal static readonly string[] GenuineReachWeaponItems = {
            "804dc1aecc9f43b9bcad0c48547c8b28", // Bearded Devil glaive
            "979ea8542538458184a4ae084c8d7f73"  // Babau longspear
        };
    }
}
