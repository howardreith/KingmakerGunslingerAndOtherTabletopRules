using System;
using Kingmaker.View;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// The Fire Beetle's luminescent glands, as a light this view owns.
    ///
    /// <para>The primary source gives the beetle a pair of glands that light a
    /// ten-foot radius and no fire damage at all. Sprint 13 established that
    /// Kingmaker has no mechanics-layer illumination model: nothing in the
    /// rules layer consults light level, so a radius of light cannot reveal,
    /// conceal, grant or deny anything to anyone. This component therefore
    /// makes no mechanical claim of any kind, and no record of it may say that
    /// it does. It exists because the creature's painted glands should look
    /// like they are lit, and because a small warm light on a near-black body
    /// is what makes that body readable at party-camera distance at all.</para>
    ///
    /// <para>The owner's four conditions are each met by construction. It is
    /// <em>instance-owned</em>: the light lives on a child object this
    /// component creates and nothing else ever references it. It is
    /// <em>cleaned up</em>: `OnDestroy` destroys the child, and the component
    /// itself is destroyed by the same three teardown paths that already
    /// release the swapped renderer, so a light cannot outlive the summon that
    /// made it. It is <em>bounded</em>: one light per view, a fixed range and
    /// intensity that do not scale with anything, and no shadows, so a bay of
    /// beetles costs a fixed and small amount. And it is <em>visually
    /// useful</em>: the range is set so the glow falls on the creature's own
    /// body and the ground immediately under it rather than lighting a
    /// room.</para>
    /// </summary>
    internal sealed class FireBeetleVisualGlow : MonoBehaviour
    {
        // Deliberately small. This is a creature's glands, not a torch: the
        // light should reach its own body and the ground it stands on, and
        // stop. A radius that lit the room would also be the first thing a
        // reviewer mistook for an illumination system.
        private const float RangeMeters = 1.9f;
        private const float Intensity = 1.35f;
        private static readonly Color Glow = new Color(1.0f, 0.42f, 0.14f);

        private GameObject _carrier;
        private Light _light;

        internal void Configure(UnitEntityView view, SkinnedMeshRenderer renderer)
        {
            if (view == null || renderer == null)
                throw new InvalidOperationException(
                    "The Fire Beetle glow requires its own attached view.");
            // Parented to the renderer's root bone where there is one, so the
            // glow travels with the creature's body rather than with the view
            // origin, and the glands stay lit while it moves.
            Transform parent = renderer.rootBone != null
                ? renderer.rootBone : view.transform;
            _carrier = new GameObject("KMG_FireBeetleGlow");
            _carrier.transform.SetParent(parent, false);
            _carrier.transform.localPosition = new Vector3(0f, 0f, 0.35f);
            _light = _carrier.AddComponent<Light>();
            _light.type = LightType.Point;
            _light.color = Glow;
            _light.range = RangeMeters;
            _light.intensity = Intensity;
            _light.shadows = LightShadows.None;
            // Never a reflection or lightmap contributor: this light is for
            // this creature's own silhouette and nothing in the scene should
            // bake or bounce it.
            _light.renderMode = LightRenderMode.ForceVertex;
        }

        /// <summary>Observation for the guarded runtime review.</summary>
        internal string Describe()
        {
            return "glow=" + (_light != null && _light.enabled ? "on" : "off") +
                ";range=" + (_light == null ? 0f : _light.range) +
                ";intensity=" + (_light == null ? 0f : _light.intensity) +
                ";shadows=" + (_light == null ? "<none>" :
                    _light.shadows.ToString()) +
                ";owned=" + (_carrier != null);
        }

        private void OnDestroy()
        {
            // The carrier owns the light, so destroying it releases both. A
            // view that is torn down mid-frame must not leave a point light
            // standing in the scene with nothing under it.
            if (_carrier != null) UnityEngine.Object.Destroy(_carrier);
            _carrier = null;
            _light = null;
        }
    }
}
