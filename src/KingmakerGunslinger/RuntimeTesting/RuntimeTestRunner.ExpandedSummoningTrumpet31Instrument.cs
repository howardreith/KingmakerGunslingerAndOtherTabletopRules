using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Read-only presentation observations. Native command/cost/save
        // observers remain authoritative for mechanics. No Sync, material
        // cloning, renderer edit, cast, hit, or roll is issued here.
        private static JObject Trumpet31InstrumentState(UnitEntityData owner, string stage)
        {
            var view = owner?.View;
            var instrument = view == null ? null : view.GetComponent<TrumpetArchonInstrumentView>();
            var weapon = owner?.Body?.PrimaryHand?.MaybeWeapon;
            var root = instrument?.Horn;
            var renderer = instrument?.HornRenderer;
            var mesh = root == null ? null : root.GetComponent<MeshFilter>()?.sharedMesh;
            var model = instrument?.BoundModel;
            return new JObject { ["stage"] = stage, ["owner"] = owner?.UniqueId,
                ["unit"] = owner?.Blueprint.AssetGuid, ["status"] = instrument?.Status,
                ["weapon"] = weapon?.Blueprint.AssetGuid,
                ["weaponOwnerExact"] = weapon != null && ReferenceEquals(weapon.Owner, owner.Descriptor),
                ["weaponWielderExact"] = weapon != null && ReferenceEquals(weapon.Wielder?.Unit, owner),
                ["weaponLootable"] = weapon?.IsLootable, ["weaponNonremovable"] = weapon?.IsNonRemovable,
                ["nativeModel"] = model?.name,
                ["nativeModelUnderOwner"] = model != null && view != null && model.transform.IsChildOf(view.transform),
                ["hornUnderNativeModel"] = root != null && model != null && root.transform.IsChildOf(model.transform),
                ["hornVisible"] = instrument != null && instrument.HornVisible,
                ["swordVisible"] = instrument != null && instrument.SwordVisible,
                ["bothFormsVisible"] = instrument != null && instrument.HornVisible && instrument.SwordVisible,
                ["formTransitions"] = instrument?.FormTransitions, ["hornFrames"] = instrument?.HornFrames,
                ["swordFrames"] = instrument?.SwordFrames, ["viewVisible"] = view != null && view.IsVisible,
                ["nativeModelScale"] = model?.transform.lossyScale.ToString(),
                ["hornWorldSize"] = renderer?.bounds.size.ToString(),
                ["hornLargestWorldDimension"] = renderer == null ? 0f : Mathf.Max(renderer.bounds.size.x,
                    Mathf.Max(renderer.bounds.size.y, renderer.bounds.size.z)),
                ["mesh"] = mesh?.name, ["vertices"] = mesh?.vertexCount,
                ["meshInstance"] = mesh == null ? 0 : mesh.GetInstanceID(),
                ["materialInstances"] = new JArray(renderer == null ? Array.Empty<int>() :
                    renderer.sharedMaterials.Where(value => value != null).Select(value => value.GetInstanceID())),
                ["materialNames"] = new JArray(renderer == null ? Array.Empty<string>() :
                    renderer.sharedMaterials.Select(value => value == null ? null : value.name)),
                ["unityFrame"] = Time.frameCount };
        }

        private static UnityEngine.Object[] Trumpet31InstrumentOwnedResources(UnitEntityData owner)
        {
            var view = owner?.View;
            var instrument = view == null ? null : view.GetComponent<TrumpetArchonInstrumentView>();
            return instrument == null ? Array.Empty<UnityEngine.Object>() : instrument.CaptureOwnedResources();
        }

        private static JObject Trumpet31InstrumentDonorState(UnitEntityData owner)
        {
            var sword = owner.Blueprint.Body.PrimaryHand as Kingmaker.Blueprints.Items.Weapons.BlueprintItemWeapon;
            var model = sword?.VisualParameters.Model;
            return new JObject { ["modelInstance"] = model == null ? 0 : model.GetInstanceID(),
                ["localPosition"] = model?.transform.localPosition.ToString(),
                ["localRotation"] = model?.transform.localEulerAngles.ToString(),
                ["renderers"] = new JArray(model == null ? Array.Empty<JObject>() :
                    model.GetComponentsInChildren<Renderer>(true).Select(value => new JObject {
                        ["instance"] = value.GetInstanceID(), ["enabled"] = value.enabled,
                        ["materials"] = new JArray(value.sharedMaterials.Select(material => material == null ? 0 : material.GetInstanceID())) })),
                ["meshes"] = new JArray(model == null ? Array.Empty<int>() : model.GetComponentsInChildren<MeshFilter>(true)
                    .Select(value => value.sharedMesh == null ? 0 : value.sharedMesh.GetInstanceID())) };
        }

        private static JObject Trumpet31InstrumentOwnedCounts()
        {
            return new JObject {
                ["components"] = Resources.FindObjectsOfTypeAll<TrumpetArchonInstrumentView>().Count(value => value != null),
                ["roots"] = Resources.FindObjectsOfTypeAll<GameObject>().Count(value => value != null && value.name == TrumpetArchonInstrumentView.RootName),
                ["meshes"] = Resources.FindObjectsOfTypeAll<Mesh>().Count(value => value != null && value.name == TrumpetArchonInstrumentView.MeshName),
                ["materials"] = Resources.FindObjectsOfTypeAll<Material>().Count(value => value != null && value.name != null &&
                    value.name.StartsWith(TrumpetArchonInstrumentView.MaterialName, StringComparison.Ordinal)) };
        }
    }
}
