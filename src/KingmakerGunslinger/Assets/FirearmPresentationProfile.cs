using System;
using System.Collections.Generic;
using Kingmaker.View.Animation;
using KingmakerGunslinger.Firearms;
using UnityEngine;

namespace KingmakerGunslinger.Assets
{
    internal sealed class FirearmPresentationProfile
    {
        private static readonly Dictionary<FirearmKind, FirearmPresentationProfile>
            Profiles = new Dictionary<FirearmKind, FirearmPresentationProfile>
            {
                { FirearmKind.Pistol, new FirearmPresentationProfile(FirearmKind.Pistol, null, false) },
                { FirearmKind.Revolver, new FirearmPresentationProfile(FirearmKind.Revolver, null, false) },
                { FirearmKind.Musket, new FirearmPresentationProfile(FirearmKind.Musket, null, false) },
                { FirearmKind.Blunderbuss, new FirearmPresentationProfile(FirearmKind.Blunderbuss, null, false) },
                { FirearmKind.Rifle, new FirearmPresentationProfile(FirearmKind.Rifle, null, false) }
            };

        private FirearmPresentationProfile(FirearmKind kind,
            WeaponAnimationStyle? animation, bool overrideAttachSlots)
        { Kind = kind; Animation = animation; OverrideAttachSlots = overrideAttachSlots; }

        internal FirearmKind Kind { get; private set; }
        internal WeaponAnimationStyle? Animation { get; private set; }
        internal bool OverrideAttachSlots { get; private set; }
        internal string HolsterPolicy { get { return BeltModel == null ? "native" : "explicit-belt/back"; } }
        internal GameObject EquippedModel { get { return FirearmAssetRuntime.GetPrefab(Kind); } }
        internal GameObject BeltModel { get { return FirearmAssetRuntime.GetBeltPrefab(Kind); } }
        internal GameObject SheathModel { get { return null; } }

        internal static FirearmPresentationProfile Require(FirearmKind kind)
        {
            FirearmPresentationProfile result;
            if (!Profiles.TryGetValue(kind, out result))
                throw new InvalidOperationException("No firearm presentation profile: " + kind);
            return result;
        }
    }
}
