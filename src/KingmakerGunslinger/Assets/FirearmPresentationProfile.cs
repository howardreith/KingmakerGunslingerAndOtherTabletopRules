using System;
using System.Collections.Generic;
using Kingmaker.View.Animation;
using KingmakerGunslinger.Firearms;
using UnityEngine;

namespace KingmakerGunslinger.Assets
{
    internal enum FirearmPresentationReadiness
    {
        NativeFallback = 0,
        AutonomousCandidate = 1,
        HumanAccepted = 2
    }

    /// <summary>
    /// Declares which custom presentation capabilities are trusted for each
    /// firearm. A disabled or unavailable capability preserves the cloned native
    /// Light/Heavy Crossbow presentation instead of replacing it with null.
    /// </summary>
    internal sealed class FirearmPresentationProfile
    {
        private static readonly Dictionary<FirearmKind, FirearmPresentationProfile>
            Profiles = new Dictionary<FirearmKind, FirearmPresentationProfile>
            {
                // Every custom firearm wrapper has failed at least one supervised
                // visibility, grip, orientation, clipping, or idle-state check.
                // Stabilization therefore keeps the cloned native presentation for
                // all five weapons until an individual replacement is human-approved.
                { FirearmKind.Pistol, new FirearmPresentationProfile(
                    FirearmKind.Pistol, FirearmPresentationReadiness.NativeFallback,
                    false, null, false) },
                { FirearmKind.Revolver, new FirearmPresentationProfile(
                    FirearmKind.Revolver, FirearmPresentationReadiness.NativeFallback,
                    false, null, false) },

                // Long-gun wrappers have repeatedly been invisible, inverted, or
                // body-clipping. Preserve visible native crossbow fallbacks until
                // each replacement is calibrated and human-approved.
                { FirearmKind.Musket, new FirearmPresentationProfile(
                    FirearmKind.Musket, FirearmPresentationReadiness.AutonomousCandidate,
                    false, null, false) },
                { FirearmKind.Blunderbuss, new FirearmPresentationProfile(
                    FirearmKind.Blunderbuss, FirearmPresentationReadiness.AutonomousCandidate,
                    false, null, false) },
                { FirearmKind.Rifle, new FirearmPresentationProfile(
                    FirearmKind.Rifle, FirearmPresentationReadiness.AutonomousCandidate,
                    false, null, false) }
            };

        private FirearmPresentationProfile(FirearmKind kind,
            FirearmPresentationReadiness equippedReadiness,
            bool useCustomBeltModel,
            WeaponAnimationStyle? animation, bool overrideAttachSlots)
        {
            Kind = kind;
            EquippedReadiness = equippedReadiness;
            UseCustomBeltModel = useCustomBeltModel;
            Animation = animation;
            OverrideAttachSlots = overrideAttachSlots;
        }

        internal FirearmKind Kind { get; private set; }
        internal FirearmPresentationReadiness EquippedReadiness { get; private set; }
        internal bool UseCustomBeltModel { get; private set; }
        internal WeaponAnimationStyle? Animation { get; private set; }
        internal bool OverrideAttachSlots { get; private set; }
        internal string EquippedPolicy
        {
            get { return EquippedReadiness.ToString(); }
        }
        internal string HolsterPolicy
        {
            get { return BeltModel == null ? "native-fallback" : "custom-belt/back"; }
        }
        internal GameObject EquippedModel
        {
            get
            {
                if (EquippedReadiness ==
                    FirearmPresentationReadiness.NativeFallback) return null;
                return FirearmAssetRuntime.HasValidatedPrefab(Kind)
                    ? FirearmAssetRuntime.GetPrefab(Kind) : null;
            }
        }
        internal GameObject BeltModel
        {
            get
            {
                return UseCustomBeltModel
                    ? FirearmAssetRuntime.GetBeltPrefab(Kind)
                    : null;
            }
        }
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
