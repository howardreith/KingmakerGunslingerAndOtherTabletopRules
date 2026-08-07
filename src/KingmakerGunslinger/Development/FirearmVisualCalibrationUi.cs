using Kingmaker.View.Animation;
using KingmakerGunslinger.Actions;
using KingmakerGunslinger.Firearms;
using UnityEngine;

namespace KingmakerGunslinger.Development
{
    internal static class FirearmVisualCalibrationUi
    {
        private static float _step = 0.01f;
        internal static void Draw()
        {
            ImmediateModeGui.Space(8f);
            ImmediateModeGui.Label("Firearm native-rig calibration lab (session-local; humanAccepted=false)");
            Kingmaker.EntitySystem.Entities.UnitEntityData unit; ExactEquippedFirearmContext firearm; string reason;
            if (!FirearmVisualCalibration.TryResolveSelected(out unit, out firearm, out reason))
            { ImmediateModeGui.Label("Unavailable: " + reason); return; }
            FirearmKind kind = firearm.Definition.Kind;
            FirearmCalibrationState state = FirearmVisualCalibration.Get(kind);
            ImmediateModeGui.Label("Selected=" + unit.CharacterName + "; kind=" + kind + "; readiness=NativeFallback/diagnostic-candidate; last=" + FirearmVisualCalibration.LastResult);
            ImmediateModeGui.BeginHorizontal();
            if (ImmediateModeGui.Button("Step 0.10")) _step = 0.1f;
            if (ImmediateModeGui.Button("Step 0.01")) _step = 0.01f;
            if (ImmediateModeGui.Button("Step 0.001")) _step = 0.001f;
            ImmediateModeGui.EndHorizontal();
            state.VisualPosition = VectorControl("Visual position", state.VisualPosition, _step);
            state.VisualEuler = VectorControl("Visual rotation", state.VisualEuler, _step * 100f);
            state.VisualScale = ScalarControl("Visual scale", state.VisualScale, _step);
            state.MuzzlePosition = VectorControl("Muzzle position", state.MuzzlePosition, _step);
            state.MuzzleEuler = VectorControl("Muzzle rotation", state.MuzzleEuler, _step * 100f);
            if (kind == FirearmKind.Musket || kind == FirearmKind.Blunderbuss || kind == FirearmKind.Rifle)
            {
                state.SupportPosition = VectorControl("Support position", state.SupportPosition, _step);
                state.SupportEuler = VectorControl("Support rotation", state.SupportEuler, _step * 100f);
                state.ButtPosition = VectorControl("Butt position", state.ButtPosition, _step);
            }
            if (kind == FirearmKind.Pistol || kind == FirearmKind.Revolver)
            {
                if (ImmediateModeGui.Button("Animation: PiercingOneHanded")) state.Animation = WeaponAnimationStyle.PiercingOneHanded;
                if (ImmediateModeGui.Button("Animation: Fencing")) state.Animation = WeaponAnimationStyle.Fencing;
                if (ImmediateModeGui.Button("Animation: Dagger")) state.Animation = WeaponAnimationStyle.Dagger;
                if (ImmediateModeGui.Button("Animation: Crossbow fallback")) state.Animation = WeaponAnimationStyle.Crossbow;
            }
            FirearmVisualCalibration.Update(state);
            if (ImmediateModeGui.Button("Show custom candidate + native refresh")) FirearmVisualCalibration.ToggleSelectedCandidate(true);
            if (ImmediateModeGui.Button("Restore native fallback + native refresh")) FirearmVisualCalibration.ToggleSelectedCandidate(false);
            if (ImmediateModeGui.Button("Apply to exact active custom instance")) FirearmVisualCalibration.ApplySelected();
            if (ImmediateModeGui.Button("Export calibration JSON")) FirearmVisualCalibration.ExportSelected();
            if (ImmediateModeGui.Button("Reset selected calibration")) FirearmVisualCalibration.Reset(kind);
            if (ImmediateModeGui.Button("Reset all session calibrations")) FirearmVisualCalibration.ResetAll();
        }

        private static Vector3 VectorControl(string label, Vector3 value, float step)
        {
            ImmediateModeGui.Label(label + ": " + value.ToString("R"));
            ImmediateModeGui.BeginHorizontal();
            if (ImmediateModeGui.Button("X-")) value.x -= step; if (ImmediateModeGui.Button("X+")) value.x += step;
            if (ImmediateModeGui.Button("Y-")) value.y -= step; if (ImmediateModeGui.Button("Y+")) value.y += step;
            if (ImmediateModeGui.Button("Z-")) value.z -= step; if (ImmediateModeGui.Button("Z+")) value.z += step;
            ImmediateModeGui.EndHorizontal(); return value;
        }
        private static float ScalarControl(string label, float value, float step)
        {
            ImmediateModeGui.Label(label + ": " + value.ToString("R"));
            ImmediateModeGui.BeginHorizontal();
            if (ImmediateModeGui.Button("-")) value = Mathf.Max(0.001f, value - step);
            if (ImmediateModeGui.Button("+")) value += step;
            ImmediateModeGui.EndHorizontal(); return value;
        }
    }
}
