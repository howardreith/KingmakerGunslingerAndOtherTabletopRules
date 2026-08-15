using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Selection;
using Kingmaker.View.Animation;
using Kingmaker.View.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using KingmakerGunslinger.Actions;
using KingmakerGunslinger.Assets;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;
using UnityEngine;

namespace KingmakerGunslinger.Development
{
    internal sealed class FirearmCalibrationState
    {
        internal const int SchemaVersion = 1;
        internal FirearmKind Kind;
        internal Vector3 VisualPosition;
        internal Vector3 VisualEuler;
        internal float VisualScale = 1f;
        internal Vector3 SupportPosition;
        internal Vector3 SupportEuler;
        internal Vector3 MuzzlePosition;
        internal Vector3 MuzzleEuler;
        internal Vector3 ButtPosition;
        internal WeaponAnimationStyle Animation = WeaponAnimationStyle.Crossbow;
        internal bool UseCustomCandidate = true;

        internal FirearmCalibrationState Clone()
        {
            return (FirearmCalibrationState)MemberwiseClone();
        }

        internal bool IsFinite()
        {
            return Finite(VisualPosition) && Finite(VisualEuler) &&
                Finite(VisualScale) && VisualScale > 0f &&
                Finite(SupportPosition) && Finite(SupportEuler) &&
                Finite(MuzzlePosition) && Finite(MuzzleEuler) &&
                Finite(ButtPosition);
        }

        internal string ToJson(string prefabName, string bundleIdentity)
        {
            if (!IsFinite()) throw new InvalidOperationException("Calibration contains invalid numeric values.");
            var output = new StringBuilder();
            output.AppendLine("{");
            output.AppendLine("  \"schemaVersion\": 1,");
            output.AppendLine("  \"firearmKind\": \"" + Kind + "\",");
            output.AppendLine("  \"prefabName\": \"" + Escape(prefabName) + "\",");
            output.AppendLine("  \"bundleIdentity\": \"" + Escape(bundleIdentity) + "\",");
            output.AppendLine("  \"visualPosition\": " + VectorJson(VisualPosition) + ",");
            output.AppendLine("  \"visualEuler\": " + VectorJson(VisualEuler) + ",");
            output.AppendLine("  \"visualScale\": " + Number(VisualScale) + ",");
            output.AppendLine("  \"supportPosition\": " + VectorJson(SupportPosition) + ",");
            output.AppendLine("  \"supportEuler\": " + VectorJson(SupportEuler) + ",");
            output.AppendLine("  \"muzzlePosition\": " + VectorJson(MuzzlePosition) + ",");
            output.AppendLine("  \"muzzleEuler\": " + VectorJson(MuzzleEuler) + ",");
            output.AppendLine("  \"buttPosition\": " + VectorJson(ButtPosition) + ",");
            output.AppendLine("  \"candidateAnimation\": \"" + Animation + "\",");
            output.AppendLine("  \"humanAccepted\": false");
            output.AppendLine("}");
            return output.ToString();
        }

        private static string VectorJson(Vector3 value)
        { return "{\"x\":" + Number(value.x) + ",\"y\":" + Number(value.y) + ",\"z\":" + Number(value.z) + "}"; }
        private static string Number(float value)
        { return value.ToString("R", CultureInfo.InvariantCulture); }
        private static string Escape(string value)
        { return (value ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\""); }
        private static bool Finite(Vector3 value)
        { return Finite(value.x) && Finite(value.y) && Finite(value.z); }
        private static bool Finite(float value)
        { return !float.IsNaN(value) && !float.IsInfinity(value); }
    }

    internal static class FirearmVisualCalibration
    {
        private static readonly object Gate = new object();
        private static readonly Dictionary<FirearmKind, FirearmCalibrationState> Committed =
            new Dictionary<FirearmKind, FirearmCalibrationState>();
        private static readonly Dictionary<FirearmKind, FirearmCalibrationState> Session =
            new Dictionary<FirearmKind, FirearmCalibrationState>();
        private static readonly Dictionary<FirearmKind, GameObject> NativeModels =
            new Dictionary<FirearmKind, GameObject>();
        private static string _lastResult = "No calibration action has run.";

        static FirearmVisualCalibration()
        {
            foreach (FirearmKind kind in Enum.GetValues(typeof(FirearmKind)))
            {
                FirearmRigCapability capability = FirearmAssetRuntime.GetCapability(kind);
                var value = new FirearmCalibrationState { Kind = kind };
                if (capability.VisualPosition.HasValue) value.VisualPosition = capability.VisualPosition.Value;
                if (capability.VisualEuler.HasValue) value.VisualEuler = capability.VisualEuler.Value;
                if (capability.VisualScale.HasValue) value.VisualScale = capability.VisualScale.Value;
                if (capability.MuzzlePosition.HasValue) value.MuzzlePosition = capability.MuzzlePosition.Value;
                if (capability.SupportPosition.HasValue) value.SupportPosition = capability.SupportPosition.Value;
                if (capability.ButtPosition.HasValue) value.ButtPosition = capability.ButtPosition.Value;
                Committed[kind] = value;
                Session[kind] = value.Clone();
            }
        }

        internal static string LastResult { get { lock (Gate) return _lastResult; } }
        internal static FirearmCalibrationState Get(FirearmKind kind)
        { lock (Gate) return Session[kind].Clone(); }
        internal static void Update(FirearmCalibrationState value)
        {
            if (value == null || !value.IsFinite()) throw new ArgumentException("Calibration must be finite and positively scaled.");
            lock (Gate) Session[value.Kind] = value.Clone();
        }
        internal static void Reset(FirearmKind kind)
        { lock (Gate) Session[kind] = Committed[kind].Clone(); }
        internal static void ResetAll()
        { lock (Gate) foreach (var entry in Committed) Session[entry.Key] = entry.Value.Clone(); }

        internal static bool TryResolveSelected(out UnitEntityData unit,
            out ExactEquippedFirearmContext firearm, out string reason)
        {
            unit = null; firearm = null;
            SelectionManager selection = SelectionManager.Instance;
            unit = selection == null ? null : selection.GetSingleSelectedUnit();
            if (unit == null) { reason = "Select exactly one unit."; return false; }
            return ExactEquippedFirearmResolver.TryResolve(unit.Descriptor, out firearm, out reason);
        }

        internal static string ApplySelected()
        {
            UnitEntityData unit; ExactEquippedFirearmContext firearm; string reason;
            if (!TryResolveSelected(out unit, out firearm, out reason)) return SetResult("FAILED: " + reason);
            FirearmCalibrationState state = Get(firearm.Definition.Kind);
            FirearmRigCapability capability = FirearmAssetRuntime.GetCapability(firearm.Definition.Kind);
            if (!capability.IsValidated) return SetResult("FAILED: custom rig is not validated: " + capability.Failure);
            Transform model = FindUnique(unit.View == null ? null : unit.View.transform, capability.PrefabName);
            if (model == null) return SetResult("FAILED: exact active candidate instance was not found uniquely; toggle/refresh through the native equipment lifecycle first.");
            Transform visual = model.Find("Visual"); Transform muzzle = model.Find("Muzzle");
            Transform support = model.Find("SupportHandTarget");
            Transform butt = model.Find("Butt");
            if (visual == null || muzzle == null || (capability.RequiresTwoHandRig &&
                (support == null || butt == null)))
                return SetResult("FAILED: active candidate hierarchy is incomplete.");
            visual.localPosition = state.VisualPosition;
            visual.localRotation = Quaternion.Euler(state.VisualEuler);
            visual.localScale = Vector3.one * state.VisualScale;
            muzzle.localPosition = state.MuzzlePosition;
            muzzle.localRotation = Quaternion.Euler(state.MuzzleEuler);
            if (butt != null) butt.localPosition = state.ButtPosition;
            if (support != null)
            {
                support.localPosition = state.SupportPosition;
                support.localRotation = Quaternion.Euler(state.SupportEuler);
                EquipmentOffsets offsets = model.GetComponent<EquipmentOffsets>();
                if (offsets == null || !ReferenceEquals(offsets.IkTargetLeftHand, support))
                    return SetResult("FAILED: instantiated EquipmentOffsets does not target SupportHandTarget.");
            }
            return SetResult("PASS: applied session calibration to exact world candidate; inventory doll refresh unavailable in the confirmed Kingmaker contract and was not mutated.");
        }

        internal static string ToggleSelectedCandidate(bool useCustom)
        {
            UnitEntityData unit; ExactEquippedFirearmContext firearm; string reason;
            if (!TryResolveSelected(out unit, out firearm, out reason)) return SetResult("FAILED: " + reason);
            FirearmKind kind = firearm.Definition.Kind;
            FirearmRigCapability capability = FirearmAssetRuntime.GetCapability(kind);
            if (useCustom && !capability.IsValidated)
                return SetResult("FAILED: custom rig is not validated: " + capability.Failure);
            object blueprint = firearm.Weapon.Blueprint;
            FieldInfo visualField = FindField(blueprint.GetType(), "m_VisualParameters");
            WeaponVisualParameters visual = visualField.GetValue(blueprint) as WeaponVisualParameters;
            if (visual == null) return SetResult("FAILED: selected firearm has no materialized visual parameters.");
            FieldInfo modelField = FindField(typeof(WeaponVisualParameters), "m_WeaponModel");
            lock (Gate)
            {
                if (!NativeModels.ContainsKey(kind)) NativeModels[kind] = visual.Model;
                GameObject model = useCustom ? FirearmAssetRuntime.GetPrefab(kind) : NativeModels[kind];
                if (model == null) return SetResult("FAILED: requested model is null; native fallback preserved.");
                modelField.SetValue(visual, model);
                FirearmCalibrationState state = Session[kind];
                state.UseCustomCandidate = useCustom;
            }
            if (unit.View == null || unit.View.HandsEquipment == null)
                return SetResult("FAILED: world HandsEquipment is unavailable; visual value changed only on the project-owned firearm blueprint.");
            unit.View.HandsEquipment.UpdateAll();
            string applied = useCustom ? ApplySelected() : "native fallback restored";
            return SetResult("PASS: world HandsEquipment.UpdateAll; " + applied +
                "; inventory doll refresh unavailable and was not reported as successful.");
        }

        internal static string ShowSelectedMusketDiagnostic(string identity)
        {
            UnitEntityData unit; ExactEquippedFirearmContext firearm; string reason;
            if (!TryResolveSelected(out unit, out firearm, out reason))
                return SetResult("FAILED: " + reason);
            if (firearm.Definition.Kind != FirearmKind.Musket)
                return SetResult("FAILED: Musket diagnostic candidates require an exact equipped Musket.");
            if (!FirearmAssetRuntime.HasValidatedDiagnosticPrefab(identity))
                return SetResult("FAILED: diagnostic Musket rig is unavailable or rejected: " +
                    (identity ?? "<null>"));
            GameObject model = FirearmAssetRuntime.GetDiagnosticPrefab(identity);
            object blueprint = firearm.Weapon.Blueprint;
            FieldInfo visualField = FindField(blueprint.GetType(), "m_VisualParameters");
            WeaponVisualParameters visual = visualField.GetValue(blueprint) as
                WeaponVisualParameters;
            if (visual == null)
                return SetResult("FAILED: selected Musket has no materialized visual parameters.");
            FieldInfo modelField = FindField(typeof(WeaponVisualParameters),
                "m_WeaponModel");
            lock (Gate)
            {
                if (!NativeModels.ContainsKey(FirearmKind.Musket))
                    NativeModels[FirearmKind.Musket] = visual.Model;
                modelField.SetValue(visual, model);
            }
            if (unit.View == null || unit.View.HandsEquipment == null)
                return SetResult("FAILED: world HandsEquipment is unavailable; diagnostic visual was assigned but not refreshed.");
            unit.View.HandsEquipment.UpdateAll();
            return SetResult("PASS: selected diagnostic-only " + identity +
                " and refreshed world HandsEquipment; close/reopen inventory for a clean doll rebuild; doll refresh is not automatically claimed.");
        }

        internal static string ExportSelected()
        {
            UnitEntityData unit; ExactEquippedFirearmContext firearm; string reason;
            if (!TryResolveSelected(out unit, out firearm, out reason)) return SetResult("FAILED: " + reason);
            ModContext context; if (!ModContext.TryGet(out context)) return SetResult("FAILED: mod context unavailable.");
            FirearmRigCapability capability = FirearmAssetRuntime.GetCapability(firearm.Definition.Kind);
            string directory = Path.Combine(context.ModEntry.Path, "development", "firearm-calibration");
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, firearm.Definition.Kind.ToString().ToLowerInvariant() + ".json");
            File.WriteAllText(path, Get(firearm.Definition.Kind).ToJson(capability.PrefabName, "runtime-loaded-bundle"), new UTF8Encoding(false));
            return SetResult("PASS: exported session-only calibration with humanAccepted=false to " + path);
        }

        internal static bool IsAllowedAnimation(FirearmKind kind, WeaponAnimationStyle value)
        {
            if (kind == FirearmKind.Musket || kind == FirearmKind.Blunderbuss || kind == FirearmKind.Rifle)
                return value == WeaponAnimationStyle.Crossbow;
            return value == WeaponAnimationStyle.PiercingOneHanded ||
                value == WeaponAnimationStyle.Fencing || value == WeaponAnimationStyle.Dagger ||
                value == WeaponAnimationStyle.Crossbow;
        }

        private static Transform FindUnique(Transform root, string prefabName)
        {
            if (root == null || string.IsNullOrEmpty(prefabName)) return null;
            Transform found = null;
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (!child.name.StartsWith(prefabName, StringComparison.OrdinalIgnoreCase)) continue;
                if (found != null) return null;
                found = child;
            }
            return found;
        }
        private static FieldInfo FindField(Type type, string name)
        {
            for (Type current = type; current != null; current = current.BaseType)
            {
                FieldInfo field = current.GetField(name, BindingFlags.Instance |
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (field != null) return field;
            }
            throw new MissingFieldException(type.FullName, name);
        }
        private static string SetResult(string value) { lock (Gate) _lastResult = value; return value; }
    }
}
