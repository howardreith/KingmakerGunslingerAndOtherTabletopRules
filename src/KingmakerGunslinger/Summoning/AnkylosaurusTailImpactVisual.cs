using Kingmaker.EntitySystem.Entities;
using Kingmaker.View;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>A brief club compression after the resolved native strike.
    /// Only this actor's terminal visual driver moves; no rule is generated.</summary>
    [DefaultExecutionOrder(10001)]
    internal sealed class AnkylosaurusTailImpactVisual : MonoBehaviour
    {
        private Transform _club;
        private Vector3 _native, _applied;
        private float _started=-1f;
        private bool _ownsScale;
        internal int Strikes { get; private set; }
        internal string LastTarget { get; private set; }
        internal void Configure(Transform club) { _club=club; }
        internal static void Observe(UnitEntityData owner,UnitEntityData victim)
        {
            var cue=owner?.View==null ? null : owner.View.GetComponent<AnkylosaurusTailImpactVisual>();
            if(cue==null || cue._club==null) return;
            cue.Restore();cue._started=Time.unscaledTime;cue.Strikes++;cue.LastTarget=victim?.UniqueId;
        }
        private void LateUpdate()
        {
            Restore();
            float elapsed=Time.unscaledTime-_started;
            if(_club==null || _started<0 || elapsed<0 || elapsed>=.12f) return;
            float pulse=Mathf.Sin(Mathf.PI*elapsed/.12f);
            _native=_club.localScale;
            _applied=Vector3.Scale(_native,new Vector3(1f+.06f*pulse,1f-.08f*pulse,1f+.04f*pulse));
            _club.localScale=_applied;_ownsScale=true;
        }
        private void Restore()
        {
            if(_ownsScale && _club!=null && (_club.localScale-_applied).sqrMagnitude<.000001f) _club.localScale=_native;
            _ownsScale=false;
        }
        private void OnDisable() { Restore();_started=-1f; }
        private void OnDestroy() { Restore(); }
    }
}
