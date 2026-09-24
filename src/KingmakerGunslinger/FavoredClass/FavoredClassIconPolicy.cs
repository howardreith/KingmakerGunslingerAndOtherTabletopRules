using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.FavoredClass
{
    /// <summary>Where a counter's icon comes from.</summary>
    internal enum FavoredClassIconSource
    {
        /// <summary>The KMG Grit feature.</summary>
        KmgGrit,
        /// <summary>The KMG Pistol-Whip deed.</summary>
        KmgPistolWhip,
        /// <summary>The KMG Gunslinger's Dodge deed.</summary>
        KmgDodge,
        /// <summary>The KMG Gunslinger Initiative deed.</summary>
        KmgInitiative,
        /// <summary>The KMG Nimble feature.</summary>
        KmgNimble,
        /// <summary>The KMG firearm item of the target's firearm type.</summary>
        KmgFirearm,
        /// <summary>A game blueprint that exists when KMG registers.</summary>
        Native,
        /// <summary>An optional provider's blueprint, resolved when the publication commits.</summary>
        Provider
    }

    /// <summary>One counter's icon donor: its source and ordered candidate identities.</summary>
    internal sealed class FavoredClassIconDonor
    {
        internal FavoredClassIconDonor(FavoredClassIconSource source, string reason, params string[] guids)
        {
            Source = source;
            Reason = reason;
            Guids = guids ?? new string[0];
        }

        internal FavoredClassIconSource Source { get; private set; }

        /// <summary>What the donor art represents (the improved feature, power or performance).</summary>
        internal string Reason { get; private set; }

        /// <summary>Candidate donor identities, first resolvable non-null icon wins.</summary>
        internal string[] Guids { get; private set; }
    }

    /// <summary>
    /// Every published favored-class counter shows the icon of the exact thing
    /// it improves: the KMG feature, deed or firearm; the native feature,
    /// ability, power or performance; or the optional provider's revelation,
    /// performance or eidolon feature (resolved by exact identity when the
    /// publication commits). No counter is published with a blank icon.
    /// </summary>
    internal static class FavoredClassIconPolicy
    {
        internal const string CriticalFocusGuid = "8ac59959b1b23c347a0361dc97cc786d";
        internal const string ImprovedDirtyTrickGuid = "ed699d64870044b43bb5a7fbe3f29494";
        internal const string ImprovedTripGuid = "0f15c6f70d8fb2b49aa6cc24239cc5fa";
        internal const string EidolonNaturalArmorFeatureGuid = "1fd770617aaa4499b8f5482c33e915af";
        internal const string EidolonFeatureSelectionGuid = "56b56bbc2dd3464da28a43f9ed6216cd";

        /// <summary>
        /// Call of the Wild's Eidolon class; its art is the pet's own identity
        /// (the natural armor feature and the eidolon selection have none).
        /// </summary>
        internal const string EidolonClassGuid = "e3b3ad6decb14cdba2e7e14982d90035";

        private static readonly Dictionary<string, FavoredClassIconDonor> ByEffect =
            new Dictionary<string, FavoredClassIconDonor>(StringComparer.Ordinal)
            {
                { FavoredClassCatalog.EffectGrit, new FavoredClassIconDonor(FavoredClassIconSource.KmgGrit, "Grit") },
                { FavoredClassCatalog.EffectMisfire, new FavoredClassIconDonor(FavoredClassIconSource.KmgFirearm,
                    "the chosen firearm type") },
                { FavoredClassCatalog.EffectFirearmConfirmation, new FavoredClassIconDonor(FavoredClassIconSource.Native,
                    "Critical Focus (critical confirmation)", CriticalFocusGuid) },
                { FavoredClassCatalog.EffectPistolWhip, new FavoredClassIconDonor(FavoredClassIconSource.KmgPistolWhip,
                    "Pistol-Whip") },
                { FavoredClassCatalog.EffectHalflingNimble, new FavoredClassIconDonor(FavoredClassIconSource.KmgNimble,
                    "Nimble") },
                { FavoredClassCatalog.EffectHalflingDodge, new FavoredClassIconDonor(FavoredClassIconSource.KmgDodge,
                    "Gunslinger's Dodge") },
                { FavoredClassCatalog.EffectDrowNimble, new FavoredClassIconDonor(FavoredClassIconSource.KmgNimble,
                    "Nimble") },
                { FavoredClassCatalog.EffectInitiative, new FavoredClassIconDonor(FavoredClassIconSource.KmgInitiative,
                    "Gunslinger Initiative") },
                { FavoredClassCatalog.EffectDirtyTrickTrip, new FavoredClassIconDonor(FavoredClassIconSource.Native,
                    "Improved Dirty Trick (dirty trick and trip maneuvers)", ImprovedDirtyTrickGuid, ImprovedTripGuid) },
                { FavoredClassCatalog.EffectBombDamage, new FavoredClassIconDonor(FavoredClassIconSource.Native,
                    "the native Bomb feature", "c59b2f256f5a70a4d896568658315b7d") },
                { FavoredClassCatalog.EffectFireIntimidate, new FavoredClassIconDonor(FavoredClassIconSource.Native,
                    "the native Demoralize action", "7d2233c3b7a0b984ba058a83b736e6ac") },
                { FavoredClassCatalog.EffectDemoralize, new FavoredClassIconDonor(FavoredClassIconSource.Native,
                    "the native Demoralize action", "7d2233c3b7a0b984ba058a83b736e6ac") },
                { FavoredClassCatalog.EffectBullRushDragDefense, new FavoredClassIconDonor(FavoredClassIconSource.Native,
                    "Improved Bull Rush", "b3614622866fe7046b787a548bbd7f59") },
                { FavoredClassCatalog.EffectUnarmedConfirmation, new FavoredClassIconDonor(FavoredClassIconSource.Native,
                    "the native unarmed strike", "7812ad3672a4b9a4fb894ea402095167") },
                { FavoredClassCatalog.EffectAquaticPenetration, new FavoredClassIconDonor(FavoredClassIconSource.Native,
                    "Spell Penetration", "ee7dc126939e4d9438357fbd5980d459") },
                { FavoredClassCatalog.EffectGrappleStunning, new FavoredClassIconDonor(FavoredClassIconSource.Native,
                    "Stunning Fist", "a29a582c3daa4c24bb0e991c596ccb28") },
                { FavoredClassCatalog.EffectPaladinAuras, new FavoredClassIconDonor(FavoredClassIconSource.Native,
                    "Aura of Courage", "e45ab30f49215054e83b4ea12165409f") },
                { FavoredClassCatalog.EffectCompanionArmor, new FavoredClassIconDonor(FavoredClassIconSource.Native,
                    "the animal companion selection", "ee63330662126374e8785cc901941ac7") },
                { FavoredClassCatalog.EffectEidolonArmor, new FavoredClassIconDonor(FavoredClassIconSource.Provider,
                    "Call of the Wild's eidolon natural armor, else the eidolon's own art", EidolonNaturalArmorFeatureGuid,
                    EidolonFeatureSelectionGuid, EidolonClassGuid) },
            };

        /// <summary>The ability each bloodline power target improves (its own art).</summary>
        private static readonly Dictionary<string, string> BloodlinePowerAbilities =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { "FireRay", "1b4989258e5964149a909e47c72b7f67" },
                { "FireBlast", "b2d1d39cd406e0f4185c52fecc73c3b5" },
                { "AirRay", "4729c2ac98d02004fb440d17f7786e28" },
                { "AirBlast", "6d005cc9c3ad3f24e8769aad2fbfdf3f" },
            };

        /// <summary>The icon donor of one counter; throws for an unmapped counter.</summary>
        internal static FavoredClassIconDonor For(string effectId, string targetKey)
        {
            FavoredClassIconDonor donor;
            if (ByEffect.TryGetValue(effectId, out donor))
                return donor;
            switch (effectId)
            {
                case FavoredClassCatalog.EffectSelectedBloodlinePower:
                {
                    string ability;
                    if (targetKey != null && BloodlinePowerAbilities.TryGetValue(targetKey, out ability))
                        return new FavoredClassIconDonor(FavoredClassIconSource.Native,
                            "the chosen bloodline power", ability);
                    break;
                }
                case FavoredClassCatalog.EffectSelectedRevelation:
                    if (targetKey != null)
                        return new FavoredClassIconDonor(FavoredClassIconSource.Provider, "the chosen revelation",
                            FavoredClassRevelationManifest.For(targetKey).FeatureGuids.ToArray());
                    break;
                case FavoredClassCatalog.EffectPerformanceRange:
                    if (targetKey != null)
                    {
                        FavoredClassPerformanceTarget performance = FavoredClassPerformanceManifest.For(targetKey);
                        return new FavoredClassIconDonor(performance.Provider ? FavoredClassIconSource.Provider :
                            FavoredClassIconSource.Native, "the chosen performance", performance.FeatureGuid);
                    }
                    break;
            }
            throw new KeyNotFoundException("No icon donor for " + effectId + "/" + (targetKey ?? "-"));
        }
    }
}
