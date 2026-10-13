using System;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.View;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// A brief instance-owned club follow-through after a resolved native
    /// strike. The native command, target, hit and rule remain authoritative.
    /// Only this view's reviewed tail chain and skeleton root move, and every
    /// frame starts by restoring the animator's current pose.
    /// </summary>
    [DefaultExecutionOrder(10001)]
    internal sealed class AnkylosaurusTailImpactVisual : MonoBehaviour
    {
        private UnitEntityView _view;
        private SkinnedMeshRenderer _renderer;
        private Mesh _mesh;
        private Transform[] _bones;
        private Matrix4x4[] _bindposes;
        private Matrix4x4[] _skin;
        private Vector3[] _vertices;
        private BoneWeight[] _weights;
        private int[] _clubVertices;
        private Transform _tail;
        private Transform _club;
        private Transform _root;
        private UnitEntityData _target;
        private float _started=-1f;
        private Quaternion _nativeTail,_appliedTail;
        private Vector3 _nativeRootLocal,_appliedRootLocal;
        private Vector3 _nativeScale,_appliedScale;
        private bool _applied;
        internal int Strikes { get; private set; }
        internal string LastTarget { get; private set; }
        internal float GapBefore { get; private set; } = -1f;
        internal float GapAfter { get; private set; } = -1f;
        internal bool IsPresenting
        {
            get
            {
                return _target!=null && _started>=0f &&
                    AnkylosaurusTailImpactPolicy.Weight(
                        Time.unscaledTime-_started)>0f;
            }
        }

        internal void Configure(UnitEntityView view,
            SkinnedMeshRenderer renderer)
        {
            if(view==null || renderer==null || renderer.sharedMesh==null ||
                renderer.sharedMesh.name!="KMG_ankylosaurus_Original" ||
                renderer.rootBone==null || renderer.bones==null ||
                !AnkylosaurusVisualPolicy.IsPermitted(
                    AnkylosaurusRulesPolicy.Key,
                    renderer.bones.Select(value=>value?.name)))
                throw new InvalidOperationException(
                    "Ankylosaurus impact pose requires its exact original skin.");
            _view=view;_renderer=renderer;_mesh=renderer.sharedMesh;
            _bones=renderer.bones;_bindposes=_mesh.bindposes;
            _vertices=_mesh.vertices;_weights=_mesh.boneWeights;
            if(_bindposes.Length!=_bones.Length ||
                _vertices.Length!=_weights.Length)
                throw new InvalidOperationException(
                    "Ankylosaurus impact skin arrays disagree.");
            _tail=_bones.Single(value=>value.name=="Tail0_M");
            _club=_bones.Single(value=>value.name=="Tail5_M");
            _root=renderer.rootBone;
            if(_bones.Any(value=>value==null ||
                    !ReferenceEquals(value,_root) && !value.IsChildOf(_root)) ||
                !_root.IsChildOf(view.transform))
                throw new InvalidOperationException(
                    "Ankylosaurus skeleton root does not own every driver.");
            int club=Array.IndexOf(_bones,_club);
            _clubVertices=Enumerable.Range(0,_weights.Length)
                .Where(index=>Driver(_weights[index],club)>.9f &&
                    Math.Abs(_vertices[index].x)>.12f).ToArray();
            if(_clubVertices.Length==0)
                throw new InvalidOperationException(
                    "Ankylosaurus terminal club surface is absent.");
            _skin=new Matrix4x4[_bones.Length];
        }

        private static float Driver(BoneWeight value,int index)
        {
            float result=0f;
            if(value.boneIndex0==index) result+=value.weight0;
            if(value.boneIndex1==index) result+=value.weight1;
            if(value.boneIndex2==index) result+=value.weight2;
            if(value.boneIndex3==index) result+=value.weight3;
            return result;
        }

        internal static void Observe(UnitEntityData owner,
            UnitEntityData victim)
        {
            var cue=owner?.View==null ? null : owner.View
                .GetComponent<AnkylosaurusTailImpactVisual>();
            if(cue==null || cue._club==null) return;
            cue.Strikes++;
            cue.LastTarget=victim?.UniqueId;
            try
            {
                cue.Restore();
                cue._target=victim;
                cue._started=Time.unscaledTime;
            }
            catch(Exception error)
            {
                cue.enabled=false;
                Debug.LogException(error);
            }
        }

        private void LateUpdate()
        {
            try { ApplyAtCurrentTime(); }
            catch(Exception error)
            {
                enabled=false;
                Debug.LogException(error);
            }
        }

        private void ApplyAtCurrentTime()
        {
            Restore();
            if(_view==null || _renderer==null || _target==null ||
                _target.View==null || _started<0f) return;
            float weight=AnkylosaurusTailImpactPolicy.Weight(
                Time.unscaledTime-_started);
            if(weight<=0f)
            {
                if(Time.unscaledTime-_started>=
                    AnkylosaurusTailImpactPolicy.DurationSeconds)
                    _target=null;
                return;
            }
            var victim=_target.View
                .GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .Where(value=>value!=null && value.enabled &&
                    value.sharedMesh!=null && value.sharedMesh.vertexCount>=100)
                .OrderByDescending(value=>value.bones.Length)
                .ThenByDescending(value=>value.bounds.size.sqrMagnitude)
                .FirstOrDefault();
            if(victim==null) return;
            Bounds bounds=victim.bounds;
            _nativeTail=_tail.rotation;
            _nativeRootLocal=_root.localPosition;
            _nativeScale=_club.localScale;
            RefreshSkin();
            Vector3 centre=ClubCentre();
            Vector3 current=centre-_tail.position;
            Vector3 desired=bounds.ClosestPoint(_tail.position)-_tail.position;
            if(current.sqrMagnitude>.001f && desired.sqrMagnitude>.001f)
                _tail.rotation=Quaternion.Slerp(_nativeTail,
                    Quaternion.FromToRotation(current,desired)*_nativeTail,
                    weight);
            RefreshSkin();
            Vector3 surface=ClosestSurface(bounds,out float gap);
            GapBefore=gap;
            Vector3 approach=bounds.ClosestPoint(surface)-surface;
            float distance=AnkylosaurusTailImpactPolicy.Approach(
                approach.magnitude,weight);
            if(distance>.001f)
                _root.position+=approach.normalized*distance;
            float pulse=Mathf.Sin(Mathf.PI*weight);
            _appliedScale=Vector3.Scale(_nativeScale,
                new Vector3(1f+.06f*pulse,1f-.08f*pulse,
                    1f+.04f*pulse));
            _club.localScale=_appliedScale;
            _appliedTail=_tail.rotation;
            _appliedRootLocal=_root.localPosition;
            _applied=true;
            RefreshSkin();
            ClosestSurface(bounds,out float after);
            GapAfter=after;
        }

        private void RefreshSkin()
        {
            for(int index=0;index<_bones.Length;index++)
                _skin[index]=_bones[index].localToWorldMatrix*
                    _bindposes[index];
        }

        private Vector3 WorldVertex(int index)
        {
            BoneWeight w=_weights[index];Vector3 v=_vertices[index];
            return _skin[w.boneIndex0].MultiplyPoint3x4(v)*w.weight0+
                _skin[w.boneIndex1].MultiplyPoint3x4(v)*w.weight1+
                _skin[w.boneIndex2].MultiplyPoint3x4(v)*w.weight2+
                _skin[w.boneIndex3].MultiplyPoint3x4(v)*w.weight3;
        }

        private Vector3 ClubCentre()
        {
            Vector3 result=Vector3.zero;
            foreach(int index in _clubVertices) result+=WorldVertex(index);
            return result/_clubVertices.Length;
        }

        private Vector3 ClosestSurface(Bounds bounds,out float gap)
        {
            Vector3 nearest=Vector3.zero;gap=float.MaxValue;
            foreach(int index in _clubVertices)
            {
                Vector3 point=WorldVertex(index);
                float candidate=Vector3.Distance(point,
                    bounds.ClosestPoint(point));
                if(candidate<gap) { gap=candidate;nearest=point; }
            }
            return nearest;
        }

        private void Restore()
        {
            if(!_applied) return;
            if(_tail!=null && Quaternion.Angle(_tail.rotation,_appliedTail)<=
                    Quaternion.Angle(_tail.rotation,_nativeTail))
                _tail.rotation=_nativeTail;
            if(_root!=null && Vector3.Distance(_root.localPosition,
                    _appliedRootLocal)<=Vector3.Distance(
                        _root.localPosition,_nativeRootLocal))
                _root.localPosition=_nativeRootLocal;
            if(_club!=null && (_club.localScale-_appliedScale)
                    .sqrMagnitude<.000001f)
                _club.localScale=_nativeScale;
            _applied=false;
        }

        private void OnDisable()
        { Restore();_target=null;_started=-1f; }

        private void OnDestroy()
        { Restore(); }
    }
}
