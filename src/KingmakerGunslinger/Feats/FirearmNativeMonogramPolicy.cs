using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Feats
{
    internal static class FirearmNativeMonogramPolicy
    {
        internal static bool TryLetter(FirearmKind kind, bool integratedNativeParameter,
            bool registeredRapidChoice, out string letter)
        {
            letter = null;
            if ((!integratedNativeParameter && !registeredRapidChoice) || !OfficialFirearmSupport.IsOfficial(kind))
                return false;
            switch (kind)
            {
                case FirearmKind.Pistol: letter = "P"; return true;
                case FirearmKind.Musket: letter = "M"; return true;
                case FirearmKind.Blunderbuss: letter = "B"; return true;
                default: return false;
            }
        }
    }
}
