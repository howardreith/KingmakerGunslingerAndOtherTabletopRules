using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.View.MapObjects;
using Kingmaker.Visual.Decals;
using Kingmaker.Visual.Particles;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.FavoredClass;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Read-only O01 observation in the save-free fixture scene: for every
    /// bardic performance candidate, records the feature, toggle, owner buff
    /// and area graph with the displayed descriptions, then spawns the area
    /// on a fixture bard three times and measures the spawned effect after it
    /// settles: the native instance, an instance whose locator snappers carry
    /// a doubled race scale, and an instance whose ground decals and particle
    /// emitter radii are doubled horizontally. Structured Unity object data
    /// only (decal transforms, particle positions, renderer bounds); no
    /// screenshots. Nothing shared is changed.
    /// </summary>
    internal sealed class FavoredClassPerformanceVisualProbe : IDisposable
    {
        private sealed class Candidate
        {
            internal Candidate(string key, string featureGuid, string areaGuid, string provider)
            {
                Key = key;
                FeatureGuid = featureGuid;
                AreaGuid = areaGuid;
                Provider = provider;
            }

            internal string Key { get; private set; }
            internal string FeatureGuid { get; private set; }
            internal string AreaGuid { get; private set; }
            internal string Provider { get; private set; }
        }

        private static readonly Candidate[] Candidates =
        {
            new Candidate("InspireCourage", "acb4df34b25ca9043a6aba1a4c92bc69", "5d4308fa344af0243b2dd3b1e500b2cc", "Kingmaker"),
            new Candidate("InspireCompetence", "6d3fcfab6d935754c918eb0e004b5ef7", "c08bd33a377d5014a81be94e33ec8ce4", "Kingmaker"),
            new Candidate("Fascinate", "ddaec3a5845bc7d4191792529b687d65", "a4fc1c0798359974e99e1d790935501d", "Kingmaker"),
            new Candidate("DirgeOfDoom", "1d48ab2bded57a74dad8af3da07d313a", "4a15b95f8e173dc4fb56924fe5598dcf", "Kingmaker"),
            new Candidate("InspireGreatness", "9ae0f32c72f8df84dab023d1b34641dc", "23ddd38738bd1d84595f3cdbb8512873", "Kingmaker"),
            new Candidate("FrighteningTune", "cfd8940869a304f4aa9077415f93febe", "55c526a79761a3c48a3cc974a09bfef7", "Kingmaker"),
            new Candidate("InspireHeroics", "199d6fa0de149d044a8ab622a542cc79", "1be964f750eea8748a76e92744746efb", "Kingmaker"),
            new Candidate("InciteRageEnemies", "35ac4bd7990fa0842bfc22e80665c2f9", "8426523287601104085d71d410a6fc42", "Kingmaker"),
            new Candidate("InciteRageAllies", "35ac4bd7990fa0842bfc22e80665c2f9", "9c423eacfb7bb9f408757e651607e125", "Kingmaker"),
            new Candidate("InciteRageAll", "35ac4bd7990fa0842bfc22e80665c2f9", "d63dce0f272ba2d4aa13000470398d63", "Kingmaker"),
            new Candidate("StormCall", "161db4d6c4a1f4640ab52c762e15c1af", "85c1ea0021ce2714f8559fb618bf7ff6", "Kingmaker"),
            new Candidate("FireDance", "3c10a0069e7f110499d2e810f4861a6e", "0bd2c3ff0012e6b468497461448174c7", "Kingmaker"),
            new Candidate("SongOfFieryGaze", "edf5697b6ddc42fca14d20a03affd475", "b556833f0a0a45738863a02c78323fed", "Call of the Wild"),
            new Candidate("Satire", "867e67a274d94c44bf6859b810745b1d", "b1125eb8eae649bdb441f22e3c088535", "Call of the Wild"),
            new Candidate("Mockery", "71a3c675a44d4a8a89c9a0840cb1d92a", "eeb9c36c16be45dda7604b6120d1ab88", "Call of the Wild"),
            new Candidate("GloriousEpic", "d78e50c8ec9c436c82d3be6a028b4572", "0d961603708c4db3abf178e26d32fb1b", "Call of the Wild"),
            new Candidate("Scandal", "88d2e41984ea4c68968197b44ec2f445", "164dba1be13048eab380b302e4f25b7e", "Call of the Wild"),
            new Candidate("DanceOfTheDead", "92d80172888643328f1638a4293fb3d8", "86e88e1394694fa6953e1bb82c76bc40", "Call of the Wild"),
            new Candidate("BlazingRondo", "6e1f8dd4e17b41808e9f49e5a71dd9fc", "1c1bd072246f4a6e91ee7b423cd4c8a7", "Call of the Wild"),
            new Candidate("BansheesRequiem", "22602cf4d9954ba2ae0223bcc27ff744", "1bc7479cc3ec42fba440f82cf09648bd", "Call of the Wild"),
            new Candidate("SymphonyOfTheElysianHeart", "3dc71cb4cbaf467f8ba5c4dbab401603", "e7d7c46a57c24db696e16b64c5e995e8", "Call of the Wild"),
            new Candidate("ClamorOfTheHeavens", "4f1a14d4d9314fd49a42ff2cef1b542b", "b5b5d3f4ebf5486a96f77e6f2e2fa832", "Call of the Wild"),
            new Candidate("DiscordantVoice", "8064adc641c74e4cb821ce048ecd83a2", "84b2a7bb32694610bc137b75ff6ff824", "Call of the Wild"),
        };

        private const float Factor = 2f;
        private const int ScaleFrame = 3;
        private const int MeasureFrame = 16;
        private const string ContextBuffGuid = "b4027a834204042409248889cc8abf67";

        private static readonly Type ParticleSystemType = FindType("UnityEngine.ParticleSystem");
        private static readonly Type ParticleType = FindType("UnityEngine.ParticleSystem+Particle");
        private static readonly FieldInfo SpawnedFxField = typeof(AreaEffectView).GetField("m_SpawnedFx",
            BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo DecalDefaultScaleField = typeof(FxDecal).GetField("m_DefaultScale",
            BindingFlags.Instance | BindingFlags.NonPublic);

        private readonly LibraryScriptableObject _library;
        private readonly List<string> _diagnostics = new List<string>();
        private readonly JArray _rows = new JArray();
        private ElementalUndineFeatScenario.PortalHarness _scene;
        private UnitEntityData _bard;
        private BlueprintBuff _contextBuff;
        private int _index = -1;
        private int _frames;
        private JObject _row;
        private AreaEffectEntityData _native;
        private AreaEffectEntityData _raceScaled;
        private AreaEffectEntityData _childScaled;
        private bool _disposed;

        internal FavoredClassPerformanceVisualProbe(LibraryScriptableObject library)
        {
            _library = library;
            Failures = new List<string>();
        }

        internal bool Done { get; private set; }
        internal List<string> Failures { get; private set; }
        internal int Observed { get; private set; }

        internal JObject Evidence
        {
            get
            {
                return new JObject
                {
                    ["factor"] = Factor,
                    ["particleSystemType"] = ParticleSystemType == null ? null : ParticleSystemType.AssemblyQualifiedName,
                    ["camera"] = Game.GetCamera() != null,
                    ["candidates"] = _rows,
                    ["diagnostics"] = new JArray(_diagnostics),
                    ["failures"] = new JArray(Failures),
                };
            }
        }

        internal void Poll()
        {
            if (Done) return;
            try
            {
                if (_scene == null)
                {
                    Start();
                    return;
                }
                if (_native == null)
                {
                    SpawnNext();
                    return;
                }
                _frames++;
                if (_frames == ScaleFrame)
                {
                    _row["raceScaleApplied"] = ApplyRaceScale(SpawnedFx(_raceScaled));
                    _row["childScaleApplied"] = ApplyChildScale(SpawnedFx(_childScaled));
                    return;
                }
                if (_frames < MeasureFrame) return;
                _row["native"] = Measure(_native);
                _row["raceScaled"] = Measure(_raceScaled);
                _row["childScaled"] = Measure(_childScaled);
                Observed++;
                EndCurrent();
            }
            catch (Exception exception)
            {
                Failures.Add((_row == null ? "probe" : (string)_row["key"]) + ": " + exception.GetType().Name + ": " +
                    exception.Message);
                EndCurrent();
                Finish();
            }
        }

        private void Start()
        {
            if (ParticleSystemType == null || ParticleType == null || SpawnedFxField == null)
                throw new InvalidOperationException("Unity particle or area view members are unavailable.");
            _contextBuff = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(_library, ContextBuffGuid,
                "Inspire Courage performer buff");
            BlueprintRace oread = BlueprintLibraryLookup.RequireExact<BlueprintRace>(_library,
                FavoredClassRaceIdentities.ForAncestry(FavoredClassAncestry.Oread).RaceGuid, "Oread");
            _scene = new ElementalUndineFeatScenario.PortalHarness(_diagnostics);
            _bard = _scene.Initialize(oread);
            _diagnostics.Add("bard-view=" + (_bard.View != null) + ";snap-map=" +
                (_bard.View != null && _bard.View.GetComponent<SnapMapBase>() != null) + ";camera=" +
                (Game.GetCamera() != null));
        }

        private void SpawnNext()
        {
            _index++;
            if (_index >= Candidates.Length)
            {
                Finish();
                return;
            }
            Candidate candidate = Candidates[_index];
            _row = new JObject { ["key"] = candidate.Key, ["provider"] = candidate.Provider };
            _rows.Add(_row);
            BlueprintScriptableObject featureObject, areaObject;
            _library.BlueprintsByAssetId.TryGetValue(candidate.FeatureGuid, out featureObject);
            _library.BlueprintsByAssetId.TryGetValue(candidate.AreaGuid, out areaObject);
            var feature = featureObject as BlueprintFeature;
            var area = areaObject as BlueprintAbilityAreaEffect;
            _row["graph"] = DescribeGraph(feature, area);
            if (area == null)
            {
                _row["absent"] = true;
                return;
            }
            _frames = 0;
            _native = Spawn(area);
            _raceScaled = Spawn(area);
            _childScaled = Spawn(area);
        }

        private AreaEffectEntityData Spawn(BlueprintAbilityAreaEffect area)
        {
            return AreaEffectsController.SpawnAttachedToTarget(
                new MechanicsContext(_bard, _bard.Descriptor, _contextBuff), area, _bard, null);
        }

        private static GameObject SpawnedFx(AreaEffectEntityData data)
        {
            AreaEffectView view = data == null ? null : data.View as AreaEffectView;
            return view == null ? null : SpawnedFxField.GetValue(view) as GameObject;
        }

        private JObject DescribeGraph(BlueprintFeature feature, BlueprintAbilityAreaEffect area)
        {
            var graph = new JObject
            {
                ["feature"] = feature == null ? null : feature.name + ":" + feature.AssetGuid,
                ["featureDescription"] = feature == null ? null : (string)feature.Description,
            };
            var toggles = new JArray();
            if (feature != null)
                foreach (AddFacts facts in feature.GetComponents<AddFacts>())
                    foreach (BlueprintUnitFact fact in facts.Facts ?? new BlueprintUnitFact[0])
                    {
                        var toggle = fact as BlueprintActivatableAbility;
                        if (toggle == null) continue;
                        BlueprintBuff buff = toggle.Buff;
                        AddAreaEffect spawn = buff == null ? null : buff.GetComponent<AddAreaEffect>();
                        toggles.Add(new JObject
                        {
                            ["toggle"] = toggle.name + ":" + toggle.AssetGuid,
                            ["toggleDescription"] = (string)toggle.Description,
                            ["group"] = toggle.Group.ToString(),
                            ["buff"] = buff == null ? null : buff.name + ":" + buff.AssetGuid,
                            ["area"] = spawn == null || spawn.AreaEffect == null ? null :
                                spawn.AreaEffect.name + ":" + spawn.AreaEffect.AssetGuid,
                        });
                    }
            graph["toggles"] = toggles;
            if (area != null)
            {
                graph["area"] = area.name + ":" + area.AssetGuid;
                graph["shape"] = area.Shape.ToString();
                graph["sizeFeet"] = area.Size.Value;
                graph["sizeMeters"] = area.Size.Meters;
                graph["affectEnemies"] = area.AffectEnemies;
                graph["affectDead"] = area.AffectDead;
                graph["fxAssetId"] = area.Fx == null ? null : area.Fx.AssetId;
                graph["components"] = new JArray((area.ComponentsArray ?? new BlueprintComponent[0])
                    .Where(value => value != null).Select(value => value.GetType().Name));
            }
            return graph;
        }

        private static JObject ApplyRaceScale(GameObject fx)
        {
            var applied = new JObject { ["fx"] = fx != null };
            if (fx == null) return applied;
            SnapToLocator[] snaps = fx.GetComponentsInChildren<SnapToLocator>(true);
            foreach (SnapToLocator snap in snaps)
                snap.RaceScale *= Factor;
            applied["snappers"] = snaps.Length;
            if (snaps.Length == 0)
            {
                Vector3 scale = fx.transform.localScale;
                fx.transform.localScale = new Vector3(scale.x * Factor, scale.y, scale.z * Factor);
                applied["rootScaled"] = true;
            }
            return applied;
        }

        private static JObject ApplyChildScale(GameObject fx)
        {
            var applied = new JObject { ["fx"] = fx != null };
            if (fx == null) return applied;
            int decals = 0;
            foreach (FxDecal decal in fx.GetComponentsInChildren<FxDecal>(true))
            {
                if (decal.UseScaleAnimation && DecalDefaultScaleField != null)
                {
                    var current = (Vector3)DecalDefaultScaleField.GetValue(decal);
                    DecalDefaultScaleField.SetValue(decal, new Vector3(current.x * Factor, current.y, current.z * Factor));
                }
                else
                {
                    Vector3 scale = decal.transform.localScale;
                    decal.transform.localScale = new Vector3(scale.x * Factor, scale.y, scale.z * Factor);
                }
                decals++;
            }
            int radii = 0;
            foreach (Component system in fx.GetComponentsInChildren(ParticleSystemType, true))
            {
                object shape = Read(system, "shape");
                if (shape == null || !(bool)Read(shape, "enabled")) continue;
                float radius = (float)Read(shape, "radius");
                shape.GetType().GetProperty("radius").SetValue(shape, radius * Factor, null);
                radii++;
            }
            applied["decals"] = decals;
            applied["particleRadii"] = radii;
            return applied;
        }

        private JObject Measure(AreaEffectEntityData data)
        {
            var result = new JObject();
            AreaEffectView view = data == null ? null : data.View as AreaEffectView;
            GameObject fx = SpawnedFx(data);
            result["fx"] = fx != null;
            if (view == null || fx == null) return result;
            Vector3 center = _bard.Position;
            result["fxName"] = fx.name;
            result["fxActive"] = fx.activeInHierarchy;
            result["rootLocalScale"] = Vec(fx.transform.localScale);
            result["hierarchy"] = new JArray(fx.GetComponentsInChildren<Transform>(true).Select(value => new JObject
            {
                ["path"] = PathOf(value, fx.transform),
                ["active"] = value.gameObject.activeInHierarchy,
                ["localScale"] = Vec(value.localScale),
                ["lossyScale"] = Vec(value.lossyScale),
                ["components"] = new JArray(value.GetComponents<Component>().Where(component => component != null)
                    .Select(component => component.GetType().FullName)),
            }));
            result["snappers"] = new JArray(fx.GetComponentsInChildren<SnapToLocator>(true).Select(snap => new JObject
            {
                ["path"] = PathOf(snap.transform, fx.transform),
                ["bone"] = snap.BoneName,
                ["dontScale"] = snap.DontScale,
                ["dontAttach"] = snap.DontAttach,
                ["raceScale"] = snap.RaceScale,
                ["locator"] = snap.Locator != null && snap.Locator.Transform != null,
            }));
            float decalHalf = 0f;
            var decals = new JArray();
            foreach (FxDecal decal in fx.GetComponentsInChildren<FxDecal>(true))
            {
                Vector3 lossy = decal.transform.lossyScale;
                float half = Math.Max(Math.Abs(lossy.x), Math.Abs(lossy.z)) / 2f;
                if (decal.gameObject.activeInHierarchy) decalHalf = Math.Max(decalHalf, half);
                decals.Add(new JObject
                {
                    ["path"] = PathOf(decal.transform, fx.transform),
                    ["active"] = decal.gameObject.activeInHierarchy,
                    ["useScaleAnimation"] = decal.UseScaleAnimation,
                    ["defaultScale"] = DecalDefaultScaleField == null ? null :
                        Vec((Vector3)DecalDefaultScaleField.GetValue(decal)),
                    ["lossyScale"] = Vec(lossy),
                    ["halfExtentMeters"] = half,
                    ["offsetMeters"] = Horizontal(decal.transform.position, center),
                });
            }
            result["decals"] = decals;
            result["decalHalfExtentMeters"] = decalHalf;
            float particleMax = 0f;
            var systems = new JArray();
            foreach (Component system in fx.GetComponentsInChildren(ParticleSystemType, true))
                systems.Add(DescribeSystem(system, center, ref particleMax));
            result["particleSystems"] = systems;
            result["particleMaxMeters"] = particleMax;
            result["renderers"] = new JArray(fx.GetComponentsInChildren<Renderer>(true).Select(renderer => new JObject
            {
                ["path"] = PathOf(renderer.transform, fx.transform),
                ["type"] = renderer.GetType().Name,
                ["enabled"] = renderer.enabled,
                ["visible"] = renderer.isVisible,
                ["boundsHalfMeters"] = Math.Max(renderer.bounds.extents.x, renderer.bounds.extents.z),
                ["boundsOffsetMeters"] = Horizontal(renderer.bounds.center, center),
            }));
            var cylinder = view.Shape as Kingmaker.View.MapObjects.SriptZones.ScriptZoneCylinder;
            result["mechanicalRadiusMeters"] = cylinder == null ? -1f : cylinder.Radius;
            return result;
        }

        private static JObject DescribeSystem(Component system, Vector3 center, ref float particleMax)
        {
            object main = Read(system, "main");
            object shape = Read(system, "shape");
            int scalingMode = Convert.ToInt32(Read(main, "scalingMode"), CultureInfo.InvariantCulture);
            int space = Convert.ToInt32(Read(main, "simulationSpace"), CultureInfo.InvariantCulture);
            var row = new JObject
            {
                ["path"] = system.gameObject.name,
                ["active"] = system.gameObject.activeInHierarchy,
                ["scalingMode"] = Read(main, "scalingMode").ToString(),
                ["simulationSpace"] = Read(main, "simulationSpace").ToString(),
                ["startSpeed"] = Curve(Read(main, "startSpeed")),
                ["startLifetime"] = Curve(Read(main, "startLifetime")),
                ["startSize"] = Curve(Read(main, "startSize")),
                ["maxParticles"] = (int)Read(main, "maxParticles"),
                ["lossyScale"] = Vec(system.transform.lossyScale),
                ["offsetMeters"] = Horizontal(system.transform.position, center),
            };
            if (shape != null)
                row["shape"] = new JObject
                {
                    ["enabled"] = (bool)Read(shape, "enabled"),
                    ["type"] = Read(shape, "shapeType").ToString(),
                    ["radius"] = (float)Read(shape, "radius"),
                    ["radiusThickness"] = (float)Read(shape, "radiusThickness"),
                    ["scale"] = Vec((Vector3)Read(shape, "scale")),
                    ["position"] = Vec((Vector3)Read(shape, "position")),
                };
            if (!system.gameObject.activeInHierarchy) return row;
            MethodInfo simulate = ParticleSystemType.GetMethod("Simulate",
                new[] { typeof(float), typeof(bool), typeof(bool), typeof(bool) });
            simulate.Invoke(system, new object[] { 1.5f, false, true, true });
            int capacity = Math.Max(1, (int)Read(main, "maxParticles"));
            Array buffer = Array.CreateInstance(ParticleType, capacity);
            int count = (int)ParticleSystemType.GetMethod("GetParticles", new[] { buffer.GetType() })
                .Invoke(system, new object[] { buffer });
            var distances = new List<float>();
            Transform transform = system.transform;
            for (int i = 0; i < count; i++)
            {
                var local = (Vector3)ParticleType.GetProperty("position").GetValue(buffer.GetValue(i), null);
                Vector3 world;
                if (space == 1)
                    world = local;
                else if (scalingMode == 0)
                    world = transform.TransformPoint(local);
                else if (scalingMode == 1)
                    world = transform.position + transform.rotation * Vector3.Scale(local, transform.localScale);
                else
                    world = transform.position + transform.rotation * local;
                distances.Add(Horizontal(world, center));
            }
            distances.Sort();
            row["simulatedParticles"] = count;
            if (count > 0)
            {
                row["particleMaxMeters"] = distances[count - 1];
                row["particleP90Meters"] = distances[(int)Math.Floor((count - 1) * 0.9)];
                particleMax = Math.Max(particleMax, distances[count - 1]);
            }
            return row;
        }

        private void EndCurrent()
        {
            foreach (AreaEffectEntityData data in new[] { _native, _raceScaled, _childScaled })
                if (data != null)
                    try
                    {
                        data.ForceEnd();
                        data.Destroy();
                    }
                    catch (Exception exception)
                    {
                        _diagnostics.Add("destroy: " + exception.GetType().Name + ": " + exception.Message);
                    }
            try
            {
                Game.Instance.EntityDestroyer.Tick();
                Game.Instance.EntityDestroyer.Tick();
            }
            catch (Exception exception)
            {
                _diagnostics.Add("destroyer: " + exception.GetType().Name + ": " + exception.Message);
            }
            _native = _raceScaled = _childScaled = null;
        }

        private void Finish()
        {
            Done = true;
            Dispose();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            if (_scene != null) _scene.Dispose();
        }

        private static object Read(object target, string property)
        {
            return target == null ? null : target.GetType().GetProperty(property).GetValue(target, null);
        }

        private static JObject Curve(object curve)
        {
            if (curve == null) return null;
            return new JObject
            {
                ["mode"] = Read(curve, "mode").ToString(),
                ["constant"] = (float)Read(curve, "constant"),
                ["constantMin"] = (float)Read(curve, "constantMin"),
                ["constantMax"] = (float)Read(curve, "constantMax"),
            };
        }

        private static float Horizontal(Vector3 point, Vector3 center)
        {
            float dx = point.x - center.x, dz = point.z - center.z;
            return (float)Math.Sqrt(dx * dx + dz * dz);
        }

        private static JArray Vec(Vector3 value)
        {
            return new JArray(value.x, value.y, value.z);
        }

        private static string PathOf(Transform value, Transform root)
        {
            var parts = new List<string>();
            for (Transform cursor = value; cursor != null; cursor = cursor.parent)
            {
                parts.Add(cursor.name);
                if (cursor == root) break;
            }
            parts.Reverse();
            return string.Join("/", parts.ToArray());
        }

        private static Type FindType(string fullName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(fullName, false);
                if (type != null) return type;
            }
            return null;
        }
    }
}
