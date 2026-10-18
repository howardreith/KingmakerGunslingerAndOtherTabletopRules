namespace KingmakerGunslinger.Summoning
{
    // Measured native Manticore weapons expose two feet. Supply only the
    // missing three feet on this exact project creature and its private
    // weapons; native bonuses, reach modifiers and corpulence remain live.
    internal static class GriffonRules
    {
        internal static float Reach(string owner, string weapon, float nativeFeet)
        {
            if (owner != GriffonVisualPolicy.BlueprintGuid) return nativeFeet;
            return weapon == "67549c5f756c495a8592ab27a019fec8" ||
                weapon == "130a92b8f9d64a46908cf05258fbb2b0" ||
                weapon == "4807a234c116416988cd388e5a923776" ? nativeFeet + 3 : nativeFeet;
        }
    }
}
