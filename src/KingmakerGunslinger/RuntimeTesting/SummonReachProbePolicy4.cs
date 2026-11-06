namespace KingmakerGunslinger.RuntimeTesting
{
    // Sprint 4 representative summons for native threat probes. The Ice
    // Devil is deliberately absent: its native phases are restricted to the
    // designated host.
    internal static class SummonReachProbePolicy4
    {
        internal static readonly string[] ProbeKeys = {
            // Small/Medium natural attacks carried by raw nine-foot types.
            "lemure", "dretch", "hound-archon", "lantern-archon", "astral-deva", "barbed-devil", "babau",
            // Larger natural reach and tails.
            "hezrou", "lillend-azata", "salamander", "ankylosaurus", "stegosaurus", "brachiosaurus",
            // Existing exact-item range corrections.
            "griffon", "elephant", "triceratops", "gorgon",
            // Manufactured weapons on larger creatures.
            "hill-giant", "fire-giant", "ettin", "cloud-giant", "storm-giant",
            // Controls: the accepted Bone Devil, the Glaive (genuine reach)
            // and ordinary native-weapon summons.
            "bone-devil", "bearded-devil", "wolf", "dire-bear"
        };
    }
}
