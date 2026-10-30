using System;
using Harmony12;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.View;
using KingmakerGunslinger.Bootstrap;
using Pathfinding;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Gives only the private Wall of Ice section a native dynamic navmesh
    /// cut.  The section unit remains the serialized lifetime/HP authority;
    /// this view-owned component is recreated by OnDataAttached after load.
    /// </summary>
    public sealed class InfernalWallNavmesh35 : MonoBehaviour
    {
        internal const float SectionLengthMeters = 3.05f;
        internal const float SectionThicknessMeters = .75f;
        internal const float SectionHeightMeters = 3.05f;

        private UnitEntityView _view;
        private UnitEntityData _owner;
        private NavmeshCut _cut;
        private bool _released;

        internal NavmeshCut Cut { get { return _cut; } }
        internal UnitEntityData Owner { get { return _owner; } }

        internal static bool IsExactIdentity(UnitEntityView view)
        {
            UnitEntityData owner = view == null ? null :
                view.EntityData as UnitEntityData;
            return owner != null && owner.Blueprint != null &&
                owner.Blueprint.AssetGuid == InfernalRules35.WallSectionUnitGuid &&
                owner.Blueprint.name == InfernalRules35.WallSectionUnitName;
        }

        internal void Configure(UnitEntityView view)
        {
            if (!IsExactIdentity(view))
                throw new InvalidOperationException(
                    "Wall navmesh cut requires the exact private section identity.");
            if (view.GetComponent<NavmeshCut>() != null)
                throw new InvalidOperationException(
                    "Wall section donor unexpectedly already owns a navmesh cut.");
            _view = view;
            _owner = view.EntityData as UnitEntityData;
            _cut = view.gameObject.AddComponent<NavmeshCut>();
            _cut.type = NavmeshCut.MeshType.Rectangle;
            _cut.rectangleSize = new Vector2(SectionLengthMeters,
                SectionThicknessMeters);
            _cut.height = SectionHeightMeters;
            _cut.center = Vector3.zero;
            _cut.isDual = false;
            _cut.cutsAddedGeom = true;
            _cut.useRotation = true;
            _cut.updateDistance = .05f;
            _cut.updateRotationDistance = 1f;
            _cut.ForceUpdate();
        }

        internal void Release()
        {
            if (_released) return;
            _released = true;
            enabled = false;
            if (_cut != null) UnityEngine.Object.DestroyImmediate(_cut);
            _cut = null;
            _owner = null;
            _view = null;
        }

        private void OnDestroy() { Release(); }
    }

    [HarmonyPatch(typeof(UnitEntityView), "OnDataAttached")]
    internal static class InfernalWallNavmesh35AttachPatch
    {
        private static void Postfix(UnitEntityView __instance)
        {
            ModContext context;
            if (!InfernalWallNavmesh35.IsExactIdentity(__instance) ||
                !ModContext.TryGet(out context) ||
                context.FeatureModules?.Active == null ||
                !context.FeatureModules.Active.ExpandedSummoning ||
                __instance.GetComponent<InfernalWallNavmesh35>() != null)
                return;
            var owned = __instance.gameObject
                .AddComponent<InfernalWallNavmesh35>();
            try { owned.Configure(__instance); }
            catch (Exception error)
            {
                owned.Release();
                Debug.LogException(error);
            }
        }
    }

    [HarmonyPatch(typeof(UnitEntityView), "OnDestroy")]
    internal static class InfernalWallNavmesh35TeardownPatch
    {
        private static void Prefix(UnitEntityView __instance)
        {
            var owned = __instance == null ? null :
                __instance.GetComponent<InfernalWallNavmesh35>();
            if (owned == null) return;
            try { owned.Release(); }
            catch (Exception)
            {
                // Never prevent the native view destruction path.
            }
        }
    }
}
