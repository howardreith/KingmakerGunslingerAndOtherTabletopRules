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
    /// <para><b>Why the light is not simply left on.</b> A Light on a child
    /// object is not governed by anything that governs the creature: it is not
    /// faded by the view's fader, not hidden when the renderer is disabled, not
    /// dissolved when the creature dies, and not culled when the creature is
    /// not drawn. Left alone it would keep illuminating the scene from inside a
    /// beetle nobody can see. So every frame it is matched to the view's own
    /// visibility and the donor renderer's own enabled state, which is what
    /// hidden, faded, dissolving and culled all reduce to; and when visibility
    /// returns, so does the glow.</para>
    ///
    /// <para><b>Ownership and release.</b> The light lives on a child object
    /// this component creates and nothing else ever references it. Release is
    /// idempotent and can be demanded immediately: the view patch's release
    /// path destroys the carrier in the same frame rather than queueing it,
    /// because a point light standing for a frame over a creature that has
    /// already gone is exactly the artefact a crowd of expiring beetles would
    /// show. Destruction of the view destroys the carrier too, since it is
    /// parented inside the view's own hierarchy, and <see cref="OnDestroy"/>
    /// releases again in case it is not.</para>
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

        private UnitEntityView _view;
        private SkinnedMeshRenderer _renderer;
        private GameObject _carrier;
        private Light _light;
        private bool _released;
        private int _enables;
        private int _disables;

        internal void Configure(UnitEntityView view, SkinnedMeshRenderer renderer)
        {
            if (view == null || renderer == null)
                throw new InvalidOperationException(
                    "The Fire Beetle glow requires its own attached view.");
            _view = view;
            _renderer = renderer;
            // Parented to the renderer's root bone where there is one, so the
            // glow travels with the creature's body rather than with the view
            // origin, and so it is destroyed with the view's own hierarchy.
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
            Apply(ShouldGlow());
        }

        /// <summary>
        /// Whether the creature is currently drawn at all.
        ///
        /// <para>Hidden, faded out, dissolving on death and culled all end in
        /// the same two observable places - the view reports itself invisible,
        /// or the donor renderer is disabled or detached - so one test covers
        /// every state the owner's contract names without this component having
        /// to know which of them is happening.</para>
        /// </summary>
        private bool ShouldGlow()
        {
            if (_released || _light == null) return false;
            if (_view == null || !_view.IsVisible) return false;
            if (_renderer == null || !_renderer.enabled) return false;
            return _renderer.gameObject != null &&
                _renderer.gameObject.activeInHierarchy;
        }

        private void Apply(bool glow)
        {
            if (_light == null) return;
            if (_light.enabled == glow) return;
            _light.enabled = glow;
            if (glow) _enables++;
            else _disables++;
        }

        private void LateUpdate()
        {
            // After the view has had its own chance to fade, hide or dissolve
            // this frame, so the glow never outlives the body by a frame.
            Apply(ShouldGlow());
        }

        /// <summary>Observation for the guarded runtime review.</summary>
        internal string Describe()
        {
            return "glow=" + (_light != null && _light.enabled ? "on" : "off") +
                ";range=" + (_light == null ? 0f : _light.range) +
                ";intensity=" + (_light == null ? 0f : _light.intensity) +
                ";shadows=" + (_light == null ? "<none>" :
                    _light.shadows.ToString()) +
                ";owned=" + (_carrier != null) +
                ";released=" + _released +
                ";viewVisible=" + (_view != null && _view.IsVisible) +
                ";rendererEnabled=" + (_renderer != null && _renderer.enabled) +
                ";enables=" + _enables + ";disables=" + _disables;
        }

        /// <summary>
        /// Destroy the carrier and the light it owns. Idempotent, and when
        /// <paramref name="immediate"/> is set the objects are gone before this
        /// call returns rather than at the end of the frame.
        /// </summary>
        internal void Release(bool immediate)
        {
            _released = true;
            if (_light != null) _light.enabled = false;
            GameObject carrier = _carrier;
            _carrier = null;
            _light = null;
            _view = null;
            _renderer = null;
            if (carrier == null) return;
            if (immediate) UnityEngine.Object.DestroyImmediate(carrier);
            else UnityEngine.Object.Destroy(carrier);
        }

        private void OnDestroy()
        {
            // The carrier is parented inside the view's hierarchy, so it is
            // usually destroyed with it; this is the case where it is not.
            Release(false);
        }
    }
}
