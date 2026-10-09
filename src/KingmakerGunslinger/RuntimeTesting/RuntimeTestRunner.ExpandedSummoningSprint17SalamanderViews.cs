using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic.Commands;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private static string Sprint17ReviewKey(UnitEntityData unit)
        {
            if (unit == null || unit.Blueprint == null) return null;
            if (SalamanderRulesPolicy.IsOwner(unit.Blueprint.AssetGuid, unit.Blueprint.name)) return "salamander";
            string key;
            return SerpentineVisualPolicy.TryProductionSnake(true, unit.Blueprint.AssetGuid,
                unit.Blueprint.name, unit.Blueprint.Prefab.AssetId, out key) ? key : null;
        }

        private static string Sprint17BodyName(string key)
        { return key == "salamander" ? SalamanderHumanBindingPolicy.BodyName : SerpentineVisualPolicy.BodyRenderer(key); }

        private static SkinnedMeshRenderer Sprint17OriginalBody(UnitEntityData unit)
        {
            if (unit == null || unit.View == null) return null;
            if (Sprint17ReviewKey(unit) == "salamander")
            {
                var human = unit.View.GetComponent<SalamanderHumanVisualAttachment>();
                return human != null && human.Live ? human.Body : null;
            }
            var snake = unit.View.GetComponent<SerpentineVisualAttachment>();
            return snake != null && snake.OriginalBodyLive ? snake.Body : null;
        }

        private static UnityEngine.Object[] Sprint17ViewResources(UnitEntityData unit)
        {
            if (unit == null || unit.View == null) return new UnityEngine.Object[0];
            var human = unit.View.GetComponent<SalamanderHumanVisualAttachment>();
            if (human != null) return human.CaptureOwnedResources().Concat(new UnityEngine.Object[] { human }).Distinct().ToArray();
            var snake = unit.View.GetComponent<SerpentineVisualAttachment>();
            return snake == null ? new UnityEngine.Object[0] : snake.CaptureOwnedResources()
                .Concat(new UnityEngine.Object[] { snake }).Distinct().ToArray();
        }

        private static string Sprint17ViewOutcome(UnitEntityData unit)
        { return Sprint17ReviewKey(unit) == "salamander" ? ExpandedSummoningSalamanderViewPatch.DescribeView(unit.View) :
            ExpandedSummoningSerpentineViewPatch.DescribeView(unit.View); }

        // Read only the exact attachment's already captured pre-swap state.
        // No donor palette, mesh, material or animation is rewritten by the observer.
        private sealed class Sprint17NativeHumanSnapshot
        {
            internal sealed class Skin
            {
                internal SkinnedMeshRenderer Renderer;
                internal Mesh Mesh;
                internal Transform[] Bones;
                internal Material[] Materials;
                internal SkinQuality Quality;
                internal Transform Root;
                internal bool Enabled, Active, Offscreen;
            }
            internal Skin[] Skins;
            internal Kingmaker.Visual.Animation.AnimationSet Set;
            internal Kingmaker.Visual.Animation.Actions.AnimationActionBase[] Actions;
            internal MeshFilter[] Filters;
            internal Mesh[] StaticMeshes;
            internal Skin Body { get { return Skins.Single(s => s.Renderer.name == SalamanderHumanBindingPolicy.BodyName); } }
            internal Sprint17NativeHumanSnapshot(SalamanderHumanVisualAttachment attachment)
            {
                var captured = Traverse.Create(attachment).Field("_skins").GetValue() as Array;
                if (captured == null) throw new InvalidOperationException("Exact human swap snapshot missing.");
                Func<object, string, object> read = (row, name) => row.GetType().GetField(name,
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue(row);
                Skins = captured.Cast<object>().Select(row => {
                    var renderer = (SkinnedMeshRenderer)read(row, "Renderer");
                    return new Skin { Renderer = renderer, Mesh = (Mesh)read(row, "Mesh"),
                        Bones = (Transform[])read(row, "Bones"), Materials = (Material[])read(row, "Materials"),
                        Quality = (SkinQuality)read(row, "Quality"), Root = renderer.rootBone,
                        Enabled = renderer.enabled, Active = renderer.gameObject.activeSelf,
                        Offscreen = renderer.updateWhenOffscreen };
                }).ToArray();
                Set = Traverse.Create(attachment).Field("_nativeSet").GetValue<Kingmaker.Visual.Animation.AnimationSet>();
                Actions = Set.Actions.ToArray();
                Filters = attachment.GetComponentsInChildren<MeshFilter>(true);
                StaticMeshes = Filters.Select(f => f.sharedMesh).ToArray();
            }
            internal bool Restored(Kingmaker.View.UnitEntityView view)
            {
                return Skins.All(s => s.Renderer != null && ReferenceEquals(s.Renderer.sharedMesh, s.Mesh) &&
                    s.Renderer.bones.SequenceEqual(s.Bones) && s.Renderer.quality == s.Quality &&
                    s.Renderer.sharedMaterials.Select(m => m.shader).SequenceEqual(s.Materials.Select(m => m.shader)) &&
                    s.Renderer.sharedMaterials.Select(m => m.mainTexture).SequenceEqual(s.Materials.Select(m => m.mainTexture)) &&
                    ReferenceEquals(s.Renderer.rootBone, s.Root) &&
                    s.Renderer.enabled == s.Enabled && s.Renderer.gameObject.activeSelf == s.Active &&
                    s.Renderer.updateWhenOffscreen == s.Offscreen) &&
                    ReferenceEquals(view.AnimationManager.AnimationSet, Set) && Set.Actions.SequenceEqual(Actions) &&
                    Filters.Select(f => f.sharedMesh).SequenceEqual(StaticMeshes);
            }
        }

        private IEnumerable<int> ReviewSprint17ProductionSalamanderViews(ExpandedSummoningCorrectionFixture fixture)
        {
            var donor = fixture.Blueprints.OfType<BlueprintUnit>().Single(b => b.AssetGuid == SalamanderProductionViewPolicy.DonorGuid);
            var control = Game.Instance.EntityCreator.SpawnUnit(donor, fixture.Caster.Position, Quaternion.identity, fixture.Scene);
            if (control == null) throw new InvalidOperationException("Native human negative control did not spawn.");
            fixture.Created.Add(control); SetExpandedSummoningBrainActive(control, false);
            var nativeSet = control.View.AnimationManager.AnimationSet;
            var nativeActions = nativeSet.Actions.ToArray();
            _reviewingProductionSalamander = true;
            try
            {
                yield return 0; yield return 0;
                CheckHumanSalamander("native-human-negative-control", control.View.GetComponent<SalamanderHumanVisualAttachment>() == null &&
                    ExpandedSummoningSalamanderViewPatch.DescribeView(control.View) == "not-attempted" &&
                    ReferenceEquals(control.View.AnimationManager.AnimationSet, nativeSet) && nativeSet.Actions.SequenceEqual(nativeActions),
                    new JObject { ["guid"] = donor.AssetGuid, ["nativeActions"] = nativeActions.Length },
                    "exact native human receives no original hook or native animation mutation");
                control.Destroy(); Game.Instance.EntityDestroyer.Tick();
                foreach (bool fault in new[] { true, false })
                {
                    UnitEntityData owner = null;
                    Sprint17NativeHumanSnapshot snapshot = null;
                    Sprint17HumanGripSurface nativeGrip = null;
                    GameObject nativeAnchor = null;
                    var owned = new List<UnityEngine.Object>();
                    UnityEngine.Object[] borrowed = new UnityEngine.Object[0];
                    int reached = 0;
                    var blueprint = fixture.Blueprints.OfType<BlueprintUnit>().Single(b => SalamanderRulesPolicy.IsOwner(b.AssetGuid, b.name));
                    var manual = new Sprint16ManualSummonControl { Caster = fixture.Caster, Blueprint = blueprint };
                    string suffix = fault ? "rollback" : "normal";
                    var row = new JObject { ["key"] = "salamander", ["scope"] = "actual production automatic human/tail view",
                        ["faultInjected"] = fault, ["nativeGripControl"] = new JObject() };
                    _serpentineBodyRows.Add(row);
                    try
                    {
                        SalamanderHumanVisualAttachment.PostSwapFaultForTest = () => {
                            var attachment = owner.View.GetComponent<SalamanderHumanVisualAttachment>();
                            snapshot = new Sprint17NativeHumanSnapshot(attachment);
                            owned.AddRange(attachment.CaptureOwnedResources()); borrowed = attachment.BorrowedResources(); reached++;
                            if (fault) throw new InvalidOperationException("Exact production Salamander rollback drill");
                        };
                        EventBus.Subscribe(manual);
                        try { owner = CastSprint17FinalSnake(fixture, "salamander"); }
                        finally { EventBus.Unsubscribe(manual); }
                        if (Game.Instance.CurrentlyLoadedArea != null && Game.Instance.CurrentlyLoadedArea.IsCapital)
                            owner.Descriptor.Master = Game.Instance.Player.MainCharacter;
                        else if (!owner.Faction.IsDirectlyControllable)
                            owner.Descriptor.SwitchFactions(Game.Instance.Player.MainCharacter.Value.Faction, false);
                        int frame = 0;
                        for (; frame < 600; frame++)
                        {
                            Game.Instance.IsPaused = false; yield return 0;
                            if (frame >= 60 && owner.Descriptor.State.CanAct && owner.IsDirectlyControllable &&
                                Sprint17BodyIntact(owner.View, SalamanderHumanBindingPolicy.BodyName) &&
                                Sprint17ViewOutcome(owner) != "human-tail:waiting-for-native-settlement") break;
                        }
                        string outcome = Sprint17ViewOutcome(owner);
                        row["settlementFrames"] = frame; row["outcome"] = outcome;
                        bool bound = fault ? reached == 1 && snapshot != null && snapshot.Restored(owner.View) &&
                            owned.Count >= 29 && owned.All(v => v == null) && outcome.StartsWith("human-tail:failed:", StringComparison.Ordinal) :
                            reached == 1 && snapshot != null && Sprint17OriginalBody(owner) != null &&
                            outcome.StartsWith("human-tail:attached;", StringComparison.Ordinal);
                        CheckHumanSalamander("automatic-" + suffix, bound && manual.Matched == 1 &&
                            Sprint17BodyIntact(owner.View, SalamanderHumanBindingPolicy.BodyName), row,
                            "one automatic exact-owner swap or exact donor rollback after native appearance/control settlement");
                        var component = owner.View.GetComponent<SalamanderHumanVisualAttachment>();
                        ExpandedSummoningSalamanderViewPatch.Postfix(owner.View);
                        yield return 0;
                        CheckHumanSalamander("once-" + suffix, reached == 1 &&
                            Sprint17ViewOutcome(owner) == outcome && ReferenceEquals(component,
                                owner.View.GetComponent<SalamanderHumanVisualAttachment>()) &&
                            owner.View.GetComponent<SalamanderProductionViewBinding>() == null, outcome,
                            "repeat callback never retries a failed swap or adds a second original component");
                        SalamanderHumanVisualAttachment.PostSwapFaultForTest = null;
                        if (!bound) throw new InvalidOperationException("Production view did not bind or roll back: " + outcome);
                        if (!fault)
                        {
                            var attachment = component;
                            row["restPose"] = Sprint17OriginalBodySample(owner, attachment.Body);
                            CheckHumanSalamander("rest", attachment.Live && attachment.AuxiliaryGeometrySuppressed &&
                                (bool)row["restPose"]["finite"] && (bool)row["restPose"]["poseFinite"], row["restPose"],
                                "finite visible original body, native actions intact and no native cape geometry");
                            var move = new UnitMoveTo(Sprint17BodyMoveDestination(owner.Position));
                            Vector3 origin = owner.Position; move.Init(owner); owner.Commands.Run(move);
                            DateTime deadline = DateTime.UtcNow.AddSeconds(18);
                            while (!move.IsFinished && DateTime.UtcNow < deadline)
                            { Game.Instance.IsPaused = false; yield return 0; }
                            CheckHumanSalamander("movement", move.IsStarted && move.IsFinished &&
                                Vector3.Distance(origin, owner.Position) > 1 && attachment.Live,
                                new JObject { ["distance"] = Vector3.Distance(origin, owner.Position), ["pose"] = Sprint17OriginalBodySample(owner, attachment.Body) },
                                "actual native movement and turns retain the original hybrid and native action set");
                            // CPU-only exact pre-swap body anchor. No native geometry export or renderer mutation.
                            var anchorObject = nativeAnchor = new GameObject(SalamanderHumanBindingPolicy.BodyName);
                            owned.Add(anchorObject); owned.Add(anchorObject.transform);
                            anchorObject.transform.position = snapshot.Body.Renderer.transform.position;
                            anchorObject.transform.rotation = snapshot.Body.Renderer.transform.rotation;
                            anchorObject.transform.localScale = snapshot.Body.Renderer.transform.lossyScale;
                            var anchor = anchorObject.AddComponent<SkinnedMeshRenderer>(); owned.Add(anchor);
                            anchor.enabled = false; anchor.sharedMesh = snapshot.Body.Mesh; anchor.bones = snapshot.Body.Bones;
                            anchor.rootBone = snapshot.Body.Root;
                            nativeGrip = new Sprint17HumanGripSurface(anchor, owned, (JObject)row["nativeGripControl"], nativeDeformers: true);
                            foreach (int step in ReviewHumanSalamanderAttack(fixture, owner, attachment, nativeGrip, owned, row)) yield return step;
                            nativeGrip.Dispose(); nativeGrip = null;
                            UnityEngine.Object.Destroy(anchorObject);
                            CheckHumanSalamander("reference-isolation", attachment.NativeActionsUnchanged &&
                                snapshot.Set.Actions.SequenceEqual(snapshot.Actions) &&
                                snapshot.Filters.Select(f => f.sharedMesh).SequenceEqual(snapshot.StaticMeshes), row["outcome"],
                                "all borrowed actions/equipment meshes remain exact; no native spear remount");
                        }
                    }
                    finally
                    {
                        SalamanderHumanVisualAttachment.PostSwapFaultForTest = null;
                        if (nativeGrip != null) nativeGrip.Dispose();
                        if (nativeAnchor != null) UnityEngine.Object.Destroy(nativeAnchor);
                        if (owner != null && !owner.Destroyed)
                        {
                            owned.AddRange(Sprint17ViewResources(owner));
                            InterruptExpandedSummoningFixtureCommands(owner); owner.CombatState.LeaveCombat();
                            owner.Destroy(); Game.Instance.EntityDestroyer.Tick(); Game.Instance.Player.UpdateIsInCombat();
                        }
                    }
                    yield return 0; yield return 0;
                    CheckHumanSalamander("destruction-" + suffix, owned.Count >= 29 && owned.All(v => v == null) &&
                        borrowed.Length >= 25 && borrowed.All(v => v != null),
                        new JObject { ["captured"] = owned.Distinct().Count(), ["remaining"] = owned.Count(v => v != null),
                            ["borrowedAlive"] = borrowed.Count(v => v != null) }, "native unit teardown reclaims every captured project object and preserves native assets");
                }
                CheckHumanSalamander("native-human-assets-preserved", nativeSet != null && nativeSet.Actions.SequenceEqual(nativeActions),
                    new JObject { ["nativeActions"] = nativeActions.Length }, "borrowed native assets remain intact after production swaps and native teardown");
            }
            finally
            {
                SalamanderHumanVisualAttachment.PostSwapFaultForTest = null;
                _reviewingProductionSalamander = false;
                if (!control.Destroyed) { control.Destroy(); Game.Instance.EntityDestroyer.Tick(); }
            }
        }
    }
}
