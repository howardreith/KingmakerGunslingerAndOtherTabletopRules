using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.View;
using Kingmaker.View.Animation;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.Animation.Actions;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>Closed research-only native piercing action adoption. One
    /// instance-owned action-list container; native clips/actions remain
    /// borrowed and unchanged. No style relabel or forced animation.</summary>
    internal sealed class SerpentineNativeSpearAnimation
    {
        private readonly UnitEntityView _view;
        private AnimationSet _original, _donor;
        private AnimationActionBase[] _originalActions, _donorActions, _expectedActions;
        private UnitAnimationActionHandAttack _borrowed;
        internal AnimationSet OwnedSet { get; private set; }
        internal string Observation { get; private set; }

        internal SerpentineNativeSpearAnimation(UnitEntityView view) { _view = view; }

        internal void Bind(string key, SkinnedMeshRenderer nativeBody, string ownedName)
        {
            BlueprintUnit source = BlueprintLibraryLookup.RequireExact<BlueprintUnit>(
                BlueprintBootstrap.Library, SerpentineVisualPolicy.PiercingDonorBlueprint,
                "archived Lizardfolk shortspear animation carrier");
            var weapon = source.Body == null ? null : source.Body.PrimaryHand as BlueprintItemWeapon;
            string sourcePrefab = source.Prefab == null ? null : source.Prefab.AssetId;
            if (sourcePrefab != SerpentineVisualPolicy.PiercingDonorPrefab || weapon == null ||
                weapon.AssetGuid != SerpentineVisualPolicy.PiercingDonorWeapon || source.Body.SecondaryHand != null)
                throw new InvalidDataException("Native piercing donor differs from the archived census.");
            // Read a single fixed shared prefab, never instantiate/activate a
            // campaign NPC, mutate its manager or drive a detached clip.
            UnitEntityView prefab = source.Prefab.Load(false);
            var manager = _view.AnimationManager;
            var donorManager = prefab == null ? null : prefab.GetComponent<UnitAnimationManager>();
            if (manager == null || donorManager == null || donorManager.AnimationSet == null)
                throw new InvalidDataException("Native piercing animation set is unavailable.");
            _original = manager.AnimationSet; _donor = donorManager.AnimationSet;
            if (_original == null || !_donor.name.StartsWith("Lizardfolk_", StringComparison.Ordinal))
                throw new InvalidDataException("Native piercing set cannot fall back to a human set.");
            _originalActions = _original.Actions.ToArray(); _donorActions = _donor.Actions.ToArray();
            var oldHand = _originalActions.OfType<UnitAnimationActionHandAttack>()
                .SingleOrDefault(value => value.Type == UnitAnimationType.MainHandAttack);
            _borrowed = _donorActions.OfType<UnitAnimationActionHandAttack>()
                .SingleOrDefault(value => value.Type == UnitAnimationType.MainHandAttack);
            bool piercing = HasUsableNativePiercingVariants(_borrowed);
            string rigEvidence;
            bool rig = MatchingNativeRig(nativeBody, prefab, out rigEvidence);
            var primary = _view.EntityData.Body.PrimaryHand.MaybeWeapon;
            Observation = "donor=" + source.AssetGuid + ";prefab=" + sourcePrefab +
                ";set=" + _donor.name + ";action=" + (_borrowed == null ? "missing" : _borrowed.name) +
                ";piercing=" + piercing + ";exact39BonePathsAndBinds=" + rig + ";rig=" + rigEvidence;
            if (oldHand == null || !SerpentineVisualPolicy.PermitsNativePiercingAction(key,
                _view.EntityData.Blueprint.Prefab.AssetId, primary == null ? null : primary.Blueprint.AssetGuid,
                source.AssetGuid, sourcePrefab, weapon.AssetGuid, false, piercing, rig))
                throw new InvalidDataException("Native piercing action rejected: " + Observation);
            if (_original.Transitions.Any() || _donor.Transitions.Any())
                throw new InvalidDataException("Explicit native transitions require separate review: " + Observation);

            _expectedActions = SerpentineVisualPolicy.CopyWithOneNativeSpearAction(
                _originalActions, (AnimationActionBase)oldHand, _borrowed);
            OwnedSet = UnityEngine.Object.Instantiate(_original);
            OwnedSet.name = ownedName + "_SpearAnimationSet";
            // Explicit fresh list even if Unity's serialization ever aliases
            // collections. No shared action, clip or transition is edited.
            FieldInfo actions = typeof(AnimationSet).GetField("m_Actions", BindingFlags.Instance | BindingFlags.NonPublic);
            if (actions == null) throw new InvalidDataException("Native action list field changed.");
            actions.SetValue(OwnedSet, new List<AnimationActionBase>(_expectedActions));
            if (ReferenceEquals(OwnedSet.Actions, _original.Actions))
                throw new InvalidOperationException("Owned action list aliases its native source.");
            manager.AnimationSet = OwnedSet; // native instance initialization, before any attack command
            if (!BoundAndNativeUnchanged) throw new InvalidOperationException("Native action adoption changed another reference.");
            Observation += ";instanceOnlyMainHand=true;otherActionsUnchanged=true";
        }

        internal bool BoundAndNativeUnchanged
        {
            get { return _view != null && _view.AnimationManager != null && OwnedSet != null &&
                ReferenceEquals(_view.AnimationManager.AnimationSet, OwnedSet) &&
                _original != null && _donor != null && _borrowed != null &&
                _original.Actions.SequenceEqual(_originalActions) && _donor.Actions.SequenceEqual(_donorActions) &&
                OwnedSet.Actions.SequenceEqual(_expectedActions); }
        }

        internal UnityEngine.Object[] BorrowedResources()
        {
            var rows = new List<UnityEngine.Object>();
            if (_original != null) rows.Add(_original);
            if (_donor != null) rows.Add(_donor);
            if (_borrowed != null)
            { rows.Add(_borrowed); if (_borrowed.Clips != null) rows.AddRange(_borrowed.Clips.Where(value => value != null)); }
            return rows.Distinct().ToArray();
        }

        internal void Release()
        {
            if (OwnedSet == null) return;
            var manager = _view == null ? null : _view.AnimationManager;
            try
            {
                if (manager != null && ReferenceEquals(manager.AnimationSet, OwnedSet))
                    manager.AnimationSet = _original;
            }
            finally
            {
                // Even if native reinitialization throws after assigning the
                // original, release our now-unreferenced container. Never
                // destroy a container still referenced by a live manager.
                if (manager == null || !ReferenceEquals(manager.AnimationSet, OwnedSet))
                { UnityEngine.Object.DestroyImmediate(OwnedSet); OwnedSet = null; }
            }
        }

        private static bool MatchingNativeRig(SkinnedMeshRenderer owner, UnitEntityView donor, out string evidence)
        {
            evidence = "missing-native-owner-or-donor";
            if (owner == null || owner.sharedMesh == null || donor == null || owner.bones.Length != 39) return false;
            var candidates = donor.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .Where(value => value.sharedMesh != null && value.bones.Length == 39).ToArray();
            evidence = "donor39BoneBodies=" + candidates.Length;
            if (candidates.Length != 1) return false;
            var other = candidates[0];
            var ownerView = owner.GetComponentInParent<UnitEntityView>();
            evidence = "missing-owner-view";
            if (ownerView == null) return false;
            Transform ownerRoot = ownerView.transform;
            evidence = "nonowned-or-null-bone";
            if (owner.bones.Any(value => value == null || !value.IsChildOf(ownerRoot)) ||
                other.bones.Any(value => value == null || !value.IsChildOf(donor.transform))) return false;
            string[] names = owner.bones.Select(value => value.name).ToArray();
            string[] donorNames = other.bones.Select(value => value.name).ToArray();
            evidence = "bone-name-set-differs";
            if (!SerpentineVisualPolicy.ExactSet(names, donorNames)) return false;
            Matrix4x4[] left = owner.sharedMesh.bindposes, right = other.sharedMesh.bindposes;
            evidence = "bind-counts=" + left.Length + "/" + right.Length;
            if (left.Length != 39 || right.Length != 39) return false;
            for (int i = 0; i < names.Length; i++)
            {
                int j = Array.IndexOf(donorNames, names[i]);
                string ownerPath = PathInside(ownerRoot, owner.bones[i]), donorPath = PathInside(donor.transform, other.bones[j]);
                evidence = "path=" + names[i] + ":" + ownerPath + "/" + donorPath;
                if (ownerPath != donorPath) return false;
                for (int k = 0; k < 16; k++)
                {
                    evidence = "bind=" + names[i] + ";cell=" + k + ";owner=" + left[i][k] + ";donor=" + right[j][k];
                    if (float.IsNaN(left[i][k]) || float.IsInfinity(left[i][k]) ||
                        float.IsNaN(right[j][k]) || float.IsInfinity(right[j][k]) ||
                        Mathf.Abs(left[i][k] - right[j][k]) > .00001f) return false;
                }
            }
            evidence = "all39-exact-paths-and-binds-within1e-5";
            return true;
        }

        private static bool HasUsableNativePiercingVariants(UnitAnimationActionHandAttack action)
        {
            if (action == null || !action.HasSetting(WeaponAnimationStyle.PiercingTwoHanded)) return false;
            FieldInfo field = typeof(UnitAnimationActionHandAttack).GetField("m_Settings",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var settings = field == null ? null : field.GetValue(action) as Array;
            if (settings == null || settings.Length > 32) return false;
            var matches = settings.Cast<object>().Where(value => value != null &&
                (WeaponAnimationStyle)value.GetType().GetField("Style").GetValue(value) ==
                    WeaponAnimationStyle.PiercingTwoHanded).ToArray();
            if (matches.Length != 1) return false;
            var clips = matches[0].GetType().GetField("Variants").GetValue(matches[0]) as AnimationClip[];
            return clips != null && clips.Length > 0 && clips.Length <= 32 && clips.All(value =>
                value != null && !float.IsNaN(value.length) && !float.IsInfinity(value.length) && value.length > 0);
        }

        private static string PathInside(Transform root, Transform leaf)
        {
            var parts = new List<string>();
            for (Transform next = leaf; next != null && next != root; next = next.parent) parts.Add(next.name);
            parts.Reverse(); return string.Join("/", parts.ToArray());
        }
    }
}
