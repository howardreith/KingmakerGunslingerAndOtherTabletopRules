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

    internal enum FirearmHolsterPolicy
    {
        NativeFallback = 0,
        Custom = 1,
        Hidden = 2
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
                    FirearmKind.Pistol, FirearmPresentationReadiness.AutonomousCandidate,
                    FirearmHolsterPolicy.Hidden,
                    WeaponAnimationStyle.PiercingOneHanded, false) },
                { FirearmKind.Revolver, new FirearmPresentationProfile(
                    FirearmKind.Revolver, FirearmPresentationReadiness.AutonomousCandidate,
                    FirearmHolsterPolicy.Hidden,
                    WeaponAnimationStyle.PiercingOneHanded, false) },

                // All long guns use canonical project meshes with independent
                // Heavy-Crossbow-donor-calibrated back frames.
                { FirearmKind.Musket, new FirearmPresentationProfile(
                    FirearmKind.Musket, FirearmPresentationReadiness.AutonomousCandidate,
                    FirearmHolsterPolicy.Custom, null, false) },
                { FirearmKind.Blunderbuss, new FirearmPresentationProfile(
                    FirearmKind.Blunderbuss, FirearmPresentationReadiness.AutonomousCandidate,
                    FirearmHolsterPolicy.Custom, null, false) },
                { FirearmKind.Rifle, new FirearmPresentationProfile(
                    FirearmKind.Rifle, FirearmPresentationReadiness.AutonomousCandidate,
                    FirearmHolsterPolicy.Custom, null, false) }
            };

        private FirearmPresentationProfile(FirearmKind kind,
            FirearmPresentationReadiness equippedReadiness,
            FirearmHolsterPolicy holsterPolicy,
            WeaponAnimationStyle? animation, bool overrideAttachSlots)
        {
            Kind = kind;
            EquippedReadiness = equippedReadiness;
            Holster = holsterPolicy;
            Animation = animation;
            OverrideAttachSlots = overrideAttachSlots;
        }

        internal FirearmKind Kind { get; private set; }
        internal FirearmPresentationReadiness EquippedReadiness { get; private set; }
        internal FirearmHolsterPolicy Holster { get; private set; }
        internal WeaponAnimationStyle? Animation { get; private set; }
        internal bool OverrideAttachSlots { get; private set; }
        internal string EquippedPolicy
        {
            get { return EquippedReadiness.ToString(); }
        }
        internal string HolsterPolicy
        {
            get
            {
                return Holster == FirearmHolsterPolicy.Hidden ? "hidden" :
                    Holster == FirearmHolsterPolicy.Custom ? "custom-belt/back" :
                    "native-fallback";
            }
        }
        internal bool HideHolsteredModel
        {
            get { return Holster == FirearmHolsterPolicy.Hidden; }
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
                return Holster == FirearmHolsterPolicy.Custom
                    ? FirearmAssetRuntime.GetBeltPrefab(Kind)
                    : null;
            }
        }
        internal GameObject SheathModel { get { return null; } }

        internal bool IsLongGun
        {
            get { return Kind == FirearmKind.Musket ||
                Kind == FirearmKind.Blunderbuss || Kind == FirearmKind.Rifle; }
        }

        internal static FirearmPresentationProfile Require(FirearmKind kind)
        {
            FirearmPresentationProfile result;
            if (!Profiles.TryGetValue(kind, out result))
                throw new InvalidOperationException("No firearm presentation profile: " + kind);
            return result;
        }
    }
}
